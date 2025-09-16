// /ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VisorDTE.Interfaces;
using VisorDTE.Models;
using VisorDTE.Processors;
using VisorDTE.Services;
using VisorDTE.Views;
using Windows.ApplicationModel.DataTransfer;
using Windows.Services.Store;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace VisorDTE.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public XamlRoot? MainXamlRoot { get; set; }

        [ObservableProperty]
        private ObservableCollection<DteViewModel> _dteViewModels = [];

        private readonly ObservableCollection<DteViewModel> _allDtes = [];

        [ObservableProperty]
        private string _statusText = "Listo para abrir archivos DTE (.json)";

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private DteViewModel? _selectedDte;

        [ObservableProperty]
        private ObservableCollection<JsonPropertyNode> _jsonTreeNodes = [];

        [ObservableProperty]
        private bool isInspectorVisible = false;

        public string ToggleInspectorLabel => IsInspectorVisible ? "Ocultar Inspector" : "Mostrar Inspector";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ToggleExpandCollapseLabel))]
        private bool _isTreeExpanded = false;

        public string ToggleExpandCollapseLabel => IsTreeExpanded ? "Contraer Todo" : "Expandir Todo";

        public Action<bool>? ExpandCollapseAllAction { get; set; }

        private readonly CatalogService _catalogService;
        private readonly PdfExportService _pdfExportService;
        private readonly StoreLicensingService _licensingService;
        private readonly Dictionary<string, Func<IDteProcessor>> _availableAddons;

        public MainViewModel()
        {
            _catalogService = new CatalogService();
            _pdfExportService = new PdfExportService();
            _licensingService = new StoreLicensingService();

            _availableAddons = new Dictionary<string, Func<IDteProcessor>>
            {
                { "9P4J2RR16ZNC", () => new ComprobanteCreditoFiscalProcessor() },
                { "9PKJRBNZ47MR", () => new NotaCreditoProcessor() },
                { "9PP3715D0MVG", () => new FacturaExportacionProcessor() },
                { "9PHC1JQZ128B", () => new NotaRemisionProcessor() },
                { "9P27V243Q3PF", () => new NotaDebitoProcessor() },
                { "9NXFMZVF6W70", () => new ComprobanteRetencionProcessor() },
                { "9N4DS5MVPMHX", () => new ComprobanteLiquidacionProcessor() },
                { "9MVQBPKT6ZSR", () => new DocumentoContableLiquidacionProcessor() },
                { "9NTDV40T370G", () => new FacturaSujetoExcluidoProcessor() },
                { "9P24VGPFXBVQ", () => new ComprobanteDonacionProcessor() }
            };
        }

        private async Task<DteParserService> GetConfiguredParserServiceAsync()
        {
            await _catalogService.InitializeAsync();
            var purchasedAddons = await _licensingService.GetPurchasedAddonIdsAsync();

            var activeProcessors = new List<IDteProcessor>
            {
                new FacturaConsumidorFinalProcessor() // Procesador base siempre activo
            };

            Debug.WriteLine($"Licencias activas encontradas: {purchasedAddons.Count}");
            foreach (var addonId in purchasedAddons)
            {
                if (_availableAddons.TryGetValue(addonId, out var processorFactory))
                {
                    Debug.WriteLine($"addonId: {addonId.ToString()}");
                    var processor = processorFactory();
                    activeProcessors.Add(processor);
                    Debug.WriteLine($"Activando procesador para: {processor.DteTypeName} ({processor.HandledDteType})");
                }
            }

            return new DteParserService(activeProcessors);
        }

        [RelayCommand]
        private async Task OpenFilesAsync()
        {
            try
            {
                var parserService = await GetConfiguredParserServiceAsync();

                var filePicker = new FileOpenPicker { ViewMode = PickerViewMode.List, SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
                filePicker.FileTypeFilter.Add(".json");
                var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
                InitializeWithWindow.Initialize(filePicker, hwnd);
                var files = await filePicker.PickMultipleFilesAsync();
                if (files is null || files.Count == 0) return;

                _allDtes.Clear();
                StatusText = "Cargando archivos...";
                var fileErrors = new List<FileError>();
                var tempDtes = new List<DteViewModel>();

                foreach (var file in files)
                {
                    try
                    {
                        var jsonContent = await File.ReadAllTextAsync(file.Path);
                        var dteModel = parserService.ParseDte(jsonContent);
                        var dteViewModel = await DteViewModel.CreateAsync(dteModel, _catalogService);
                        tempDtes.Add(dteViewModel);
                    }
                    catch (Exception ex) { fileErrors.Add(new FileError { FileName = file.Name, ErrorMessage = ex.Message }); }
                }

                var orderedDtes = tempDtes.OrderBy(vm => vm.Dte.Identificacion.FecEmi).ThenBy(vm => vm.Dte.Identificacion.HorEmi);
                foreach (var vm in orderedDtes) { _allDtes.Add(vm); }

                FilterDtes();
                StatusText = $"{_allDtes.Count} DTE(s) cargados correctamente.";

                if (fileErrors.Count > 0)
                {
                    StatusText += $" {fileErrors.Count} archivo(s) con error.";
                    await ShowErrorSummaryDialog(fileErrors);
                }
            }
            catch (Exception ex)
            {
                StatusText = "Error al intentar abrir archivos.";
                await ShowErrorDialog("Ocurrió un error inesperado", $"Detalle: {ex.Message}");
            }
        }

        partial void OnIsInspectorVisibleChanged(bool value)
        {
            OnPropertyChanged(nameof(ToggleInspectorLabel));
            if (!value) IsTreeExpanded = false;
        }

        partial void OnSearchQueryChanged(string value)
        {
            FilterDtes();
        }

        partial void OnSelectedDteChanged(DteViewModel? value)
        {
            BuildJsonTree(value);
            IsTreeExpanded = false;
        }

        private void FilterDtes()
        {
            DteViewModels.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchQuery)
                ? _allDtes
                : _allDtes.Where(vm => vm.Dte.Identificacion.NumeroControl.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            foreach (var vm in filtered) { DteViewModels.Add(vm); }
        }

        private void BuildJsonTree(DteViewModel? dteViewModel)
        {
            JsonTreeNodes.Clear();
            if (dteViewModel?.Dte == null) return;
            var rootNode = new JsonPropertyNode { PropertyName = dteViewModel.TipoDteDescripcion };
            PopulateChildren(dteViewModel.Dte, rootNode.Children, dteViewModel);
            JsonTreeNodes.Add(rootNode);
        }

        private void PopulateChildren(object source, ObservableCollection<JsonPropertyNode> children, DteViewModel dteViewModel)
        {
            if (source == null) return;
            var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (prop.GetIndexParameters().Length > 0) continue;
                var value = prop.GetValue(source);
                if (value == null) continue;
                var node = new JsonPropertyNode { PropertyName = prop.Name };
                if (IsSimpleType(prop.PropertyType))
                {
                    node.Value = dteViewModel.GetTranslatedValue(prop.Name, value.ToString() ?? string.Empty);
                }
                else if (value is IEnumerable list && prop.PropertyType != typeof(string))
                {
                    int index = 0;
                    foreach (var item in list)
                    {
                        if (item != null)
                        {
                            var childNode = new JsonPropertyNode { PropertyName = $"[{index++}]" };
                            PopulateChildren(item, childNode.Children, dteViewModel);
                            if (childNode.Children.Count > 0) node.Children.Add(childNode);
                        }
                    }
                }
                else
                {
                    PopulateChildren(value, node.Children, dteViewModel);
                }
                if (!string.IsNullOrEmpty(node.Value) || node.Children.Count > 0) children.Add(node);
            }
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(decimal?);
        }

        [RelayCommand]
        private void ToggleInspector() => IsInspectorVisible = !IsInspectorVisible;

        [RelayCommand]
        private void ToggleExpandCollapse()
        {
            IsTreeExpanded = !IsTreeExpanded;
            ExpandCollapseAllAction?.Invoke(IsTreeExpanded);
        }

        private async Task ShowErrorSummaryDialog(List<FileError> errors)
        {
            var errorView = new ErrorSummaryView { Errors = errors, SuccessCount = _allDtes.Count };
            var dialog = new ContentDialog
            {
                Title = "Resumen de Carga de Archivos",
                Content = errorView,
                CloseButtonText = "OK",
                XamlRoot = this.MainXamlRoot,
                RequestedTheme = App.MainRoot.RequestedTheme // <-- CORRECCIÓN
            };
            await dialog.ShowAsync();
        }

        [RelayCommand]
        private async Task ExportToPdfAsync()
        {
            if (DteViewModels.Count == 0) { StatusText = "No hay DTEs para exportar."; return; }
            try
            {
                var fileSaver = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary, SuggestedFileName = $"Exportacion_DTE_{DateTime.Now:yyyyMMdd}" };
                fileSaver.FileTypeChoices.Add("Archivo PDF", [".pdf"]);
                var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
                InitializeWithWindow.Initialize(fileSaver, hwnd);
                var file = await fileSaver.PickSaveFileAsync();
                if (file != null)
                {
                    StatusText = "Exportando a PDF...";
                    try
                    {
                        await Task.Run(() => _pdfExportService.ExportAsPdf(file.Path, [.. DteViewModels]));
                        StatusText = $"Exportado a {file.Name} correctamente.";
                    }
                    catch (Exception ex) { await ShowErrorDialog("Error al generar el PDF", $"Detalle: {ex.Message}"); }
                }
            }
            catch (Exception ex)
            {
                StatusText = "Error al exportar.";
                await ShowErrorDialog("No se pudo mostrar el diálogo para guardar", $"Detalle: {ex.Message}");
            }
        }

        private async Task ShowErrorDialog(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.MainXamlRoot,
                RequestedTheme = App.MainRoot.RequestedTheme // <-- CORRECCIÓN
            };
            await dialog.ShowAsync();
        }

        [RelayCommand]
        private async Task ExportF07AnexoAsync()
        {
            if (_allDtes.Count == 0)
            {
                StatusText = "No hay documentos cargados para generar un anexo.";
                await ShowErrorDialog("No hay Datos", "Por favor, abra uno o más archivos DTE antes de generar un anexo F07.");
                return;
            }

            var anexoSelectorDialog = new ContentDialog
            {
                Title = "Seleccionar Anexo F07 a Generar",
                PrimaryButtonText = "Generar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.MainXamlRoot,
                RequestedTheme = App.MainRoot.RequestedTheme // <-- CORRECCIÓN
            };

            var stackPanel = new StackPanel { Spacing = 12 };
            var anexo1Radio = new RadioButton { Content = "Anexo de Ventas a Consumidor Final (Facturas)", Tag = AnexoF07Type.VentasConsumidorFinal, IsChecked = true };
            var anexo2Radio = new RadioButton { Content = "Anexo de Ventas a Contribuyentes (Crédito Fiscal)", Tag = AnexoF07Type.VentasContribuyentes };
            stackPanel.Children.Add(anexo1Radio);
            stackPanel.Children.Add(anexo2Radio);
            anexoSelectorDialog.Content = stackPanel;

            var result = await anexoSelectorDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var selectedAnexo = (anexo1Radio.IsChecked == true) ? (AnexoF07Type)anexo1Radio.Tag : (AnexoF07Type)anexo2Radio.Tag;
                IEnumerable<DteViewModel> dtesToExport;

                if (selectedAnexo == AnexoF07Type.VentasConsumidorFinal)
                {
                    dtesToExport = _allDtes.Where(vm => vm.Dte.Identificacion.TipoDte == "01" || vm.Dte.Identificacion.TipoDte == "11");
                }
                else
                {
                    dtesToExport = _allDtes.Where(vm => vm.Dte.Identificacion.TipoDte == "03");
                }

                if (!dtesToExport.Any())
                {
                    StatusText = "No se encontraron DTEs del tipo seleccionado.";
                    await ShowErrorDialog("Sin Documentos", "No hay documentos del tipo requerido para generar el anexo seleccionado.");
                    return;
                }

                var firstDte = dtesToExport.First().Dte;
                var nitEmisor = firstDte.Emisor.Nit.Replace("-", "");
                var fechaPeriodo = DateTime.Parse(firstDte.Identificacion.FecEmi);
                var anio = fechaPeriodo.ToString("yyyy");
                var mes = fechaPeriodo.ToString("MM");
                var version = "14";

                string defaultFileName = $"{nitEmisor}F07{anio}{mes}V{version}.csv";

                var fileSaver = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    SuggestedFileName = defaultFileName
                };
                fileSaver.FileTypeChoices.Add("Archivo CSV", new List<string> { ".csv" });

                var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
                InitializeWithWindow.Initialize(fileSaver, hwnd);

                var file = await fileSaver.PickSaveFileAsync();
                if (file != null)
                {
                    StatusText = "Generando archivo CSV...";
                    try
                    {
                        var csvService = new F07AnexoCsvService();
                        byte[] csvContentBytes;
                        var dteModels = dtesToExport.Select(vm => vm.Dte).Cast<Dte>();

                        if (selectedAnexo == AnexoF07Type.VentasConsumidorFinal)
                        {
                            csvContentBytes = await csvService.GenerateAnexoConsumidorFinalCsv(dteModels);
                        }
                        else
                        {
                            csvContentBytes = await csvService.GenerateAnexoVentasContribuyenteCsv(dteModels);
                        }

                        await File.WriteAllBytesAsync(file.Path, csvContentBytes);

                        StatusText = $"Anexo '{file.Name}' generado correctamente con {dtesToExport.Count()} registros.";
                    }
                    catch (Exception ex)
                    {
                        StatusText = "Error al generar el archivo CSV.";
                        await ShowErrorDialog("Error de Exportación", $"No se pudo generar el archivo CSV.\nDetalle: {ex.Message}");
                    }
                }
            }
        }

        #region Compras en la Aplicación

        [RelayCommand]
        private async Task ShowPurchaseAddonsDialogAsync()
        {
            Debug.WriteLine("--- Iniciando consulta de complementos a la Tienda ---");
            try
            {
                var storeContext = StoreContext.GetDefault();
                if (storeContext == null)
                {
                    await ShowErrorDialog("Error Crítico", "No se pudo obtener el contexto de la Tienda.");
                    return;
                }

                var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
                InitializeWithWindow.Initialize(storeContext, hwnd);

                string[] productKinds = { "Durable" };
                var filterList = new List<string>(productKinds);

                StoreProductQueryResult queryResult = await storeContext.GetAssociatedStoreProductsAsync(filterList);

                if (queryResult.ExtendedError != null)
                {
                    await ShowErrorDialog("Error de la Tienda", $"No se pudo contactar con la Microsoft Store.\nCódigo: 0x{queryResult.ExtendedError.HResult:X}");
                    return;
                }

                var availableAddons = new List<AddonViewModel>();
                foreach (var product in queryResult.Products.Values)
                {
                    availableAddons.Add(new AddonViewModel
                    {
                        StoreId = product.StoreId,
                        Title = product.Title,
                        Description = product.Description,
                        Price = product.Price.FormattedPrice,
                        IsPurchased = product.IsInUserCollection,
                        PurchaseCommand = new AsyncRelayCommand(() => PurchaseAddonAsync(product.StoreId))
                    });
                }

                var purchaseView = new PurchaseAddonsView { AvailableAddons = availableAddons };

                var dialog = new ContentDialog
                {
                    Title = "Comprar Complementos",
                    Content = purchaseView,
                    CloseButtonText = "Cerrar",
                    XamlRoot = this.MainXamlRoot,
                    RequestedTheme = App.MainRoot.RequestedTheme // <-- CORRECCIÓN
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Error Inesperado", $"Ocurrió una excepción: {ex.Message}");
            }
        }

        private async Task PurchaseAddonAsync(string storeId)
        {
            var storeContext = StoreContext.GetDefault();
            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(storeContext, hwnd);
            StorePurchaseResult result = await storeContext.RequestPurchaseAsync(storeId);

            if (result.ExtendedError != null)
            {
                await ShowErrorDialog("Error en la compra", "Ocurrió un error durante la transacción. No se te ha cobrado nada.");
                return;
            }

            switch (result.Status)
            {
                case StorePurchaseStatus.Succeeded:
                    await new ContentDialog
                    {
                        Title = "¡Compra completada!",
                        Content = "Gracias por tu apoyo. La nueva funcionalidad estará disponible la próxima vez que inicies la aplicación.",
                        CloseButtonText = "OK",
                        XamlRoot = this.MainXamlRoot,
                        RequestedTheme = App.MainRoot.RequestedTheme // <-- CORRECCIÓN
                    }.ShowAsync();
                    break;
                case StorePurchaseStatus.NotPurchased:
                    break;
                case StorePurchaseStatus.NetworkError:
                case StorePurchaseStatus.ServerError:
                    await ShowErrorDialog("Error de red", "No se pudo completar la compra. Por favor, revisa tu conexión a internet e inténtalo de nuevo.");
                    break;
                default:
                    await ShowErrorDialog("Error desconocido", "Ocurrió un error inesperado durante la compra.");
                    break;
            }
        }

        #endregion

        #region Comandos de Configuración y Ayuda

        [RelayCommand]
        private async Task ShowSettingsDialogAsync()
        {
            var settingsDialog = new ContentDialog
            {
                Title = "Configuración de Tema",
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.MainXamlRoot,
                RequestedTheme = App.MainRoot.RequestedTheme // <-- CORRECCIÓN
            };

            var lightRadio = new RadioButton { Content = "Claro", Tag = "Light" };
            var darkRadio = new RadioButton { Content = "Oscuro", Tag = "Dark" };
            var systemRadio = new RadioButton { Content = "Usar configuración del sistema", Tag = "Default" };

            var savedTheme = ApplicationData.Current.LocalSettings.Values["appTheme"] as string;
            switch (savedTheme)
            {
                case "Light":
                    lightRadio.IsChecked = true;
                    break;
                case "Dark":
                    darkRadio.IsChecked = true;
                    break;
                default:
                    systemRadio.IsChecked = true;
                    break;
            }

            var stackPanel = new StackPanel { Spacing = 12 };
            stackPanel.Children.Add(lightRadio);
            stackPanel.Children.Add(darkRadio);
            stackPanel.Children.Add(systemRadio);
            settingsDialog.Content = stackPanel;

            var result = await settingsDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                string selectedTheme = "Default";
                if (lightRadio.IsChecked == true) selectedTheme = "Light";
                if (darkRadio.IsChecked == true) selectedTheme = "Dark";

                ApplicationData.Current.LocalSettings.Values["appTheme"] = selectedTheme;

                App.ApplyTheme();
            }
        }

        [RelayCommand]
        private async Task ShowAboutDialogAsync()
        {
            var aboutDialog = new ContentDialog
            {
                Title = "Acerca de Visor DTE CIPS",
                CloseButtonText = "Cerrar",
                XamlRoot = this.MainXamlRoot,
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock { Text = "Visor de Documentos Tributarios Electrónicos" },
                        new TextBlock { Text = "Versión: 1.0.22" },
                        new TextBlock { Text = "© 2025 Crazy Intelligence Programming Studio (CIPS)" },
                        new HyperlinkButton
                        {
                            Content = "Soporte: cips-support@outlook.com",
                            NavigateUri = new Uri("mailto:cips-support@outlook.com")
                        }
                    }
                },
                RequestedTheme = App.MainRoot.RequestedTheme // <-- CORRECCIÓN
            };
            await aboutDialog.ShowAsync();
        }

        #endregion

        #region Comandos de Portapapeles

        [RelayCommand]
        private void CopyToClipboard(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
        }

        [RelayCommand]
        private async Task PasteSearchAsync()
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();
                if (!string.IsNullOrEmpty(text))
                {
                    SearchQuery = text;
                }
            }
        }

        #endregion
    }
}