// /ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VisorDTE.Interfaces;
using VisorDTE.Models;
using VisorDTE.Processors;
using VisorDTE.Services;
using VisorDTE.Views;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace VisorDTE.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<DteViewModel> _dteViewModels = [];

        private readonly ObservableCollection<DteViewModel> _allDtes = [];

        [ObservableProperty]
        private string _statusText = "Listo para abrir archivos DTE (.json)";

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private DteViewModel _selectedDte;

        [ObservableProperty]
        private ObservableCollection<JsonPropertyNode> _jsonTreeNodes = [];

        [ObservableProperty]
        private bool isInspectorVisible = false;

        public string ToggleInspectorLabel => IsInspectorVisible ? "Ocultar Inspector" : "Mostrar Inspector";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ToggleExpandCollapseLabel))]
        private bool _isTreeExpanded = false;

        public string ToggleExpandCollapseLabel => IsTreeExpanded ? "Contraer Todo" : "Expandir Todo";

        public Action<bool> ExpandCollapseAllAction { get; set; }

        private readonly CatalogService _catalogService;
        private readonly PdfExportService _pdfExportService;
        private DteParserService _parserService;
        private readonly StoreLicensingService _licensingService;
        private readonly Dictionary<string, Func<IDteProcessor>> _availableAddons;

        private bool _areServicesInitialized = false;

        public MainViewModel()
        {
            _catalogService = new CatalogService();
            _pdfExportService = new PdfExportService();
            _licensingService = new StoreLicensingService();

            _availableAddons = new Dictionary<string, Func<IDteProcessor>>
            {
                { "9N123ABCDEF1", () => new ComprobanteCreditoFiscalProcessor() },
                { "9N123ABCDEF2", () => new NotaCreditoProcessor() },
                { "9N123ABCDEF3", () => new FacturaExportacionProcessor() }
            };
        }

        private async Task InitializeServicesAsync()
        {
            if (_areServicesInitialized) return;
            await _catalogService.InitializeAsync();
            var purchasedAddons = await _licensingService.GetPurchasedAddonIdsAsync();

            var activeProcessors = new List<IDteProcessor>
            {
                new FacturaConsumidorFinalProcessor()
            };

            foreach (var addonId in purchasedAddons)
            {
                if (_availableAddons.TryGetValue(addonId, out var processorFactory))
                {
                    activeProcessors.Add(processorFactory());
                }
            }

            _parserService = new DteParserService(activeProcessors);
            _areServicesInitialized = true;
        }

        [RelayCommand]
        private async Task OpenFilesAsync()
        {
            try
            {
                await InitializeServicesAsync();

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
                        var dteModel = _parserService.ParseDte(jsonContent);
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

        // --- INICIO DE LA MODIFICACIÓN ---
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
                    // Se llama al nuevo método público del DteViewModel
                    node.Value = dteViewModel.GetTranslatedValue(prop.Name, value.ToString());
                }
                else if (value is IEnumerable list && prop.PropertyType != typeof(string))
                {
                    foreach (var item in list)
                    {
                        if (item != null)
                        {
                            var childNode = new JsonPropertyNode { PropertyName = $"[{children.Count}]" };
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

        // El método GetCatalogDescription se ha eliminado de esta clase
        // --- FIN DE LA MODIFICACIÓN ---

        #region Código sin cambios

        partial void OnIsInspectorVisibleChanged(bool value)
        {
            OnPropertyChanged(nameof(ToggleInspectorLabel));
            if (!value) IsTreeExpanded = false;
        }

        partial void OnSearchQueryChanged(string value)
        {
            FilterDtes();
        }

        partial void OnSelectedDteChanged(DteViewModel value)
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

        private void BuildJsonTree(DteViewModel dteViewModel)
        {
            JsonTreeNodes.Clear();
            if (dteViewModel?.Dte == null) return;
            var rootNode = new JsonPropertyNode { PropertyName = dteViewModel.TipoDteDescripcion };
            PopulateChildren(dteViewModel.Dte, rootNode.Children, dteViewModel);
            JsonTreeNodes.Add(rootNode);
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(decimal?);
        }

        [RelayCommand]
        private void ToggleInspector()
        {
            IsInspectorVisible = !IsInspectorVisible;
        }

        [RelayCommand]
        private void ToggleExpandCollapse()
        {
            IsTreeExpanded = !IsTreeExpanded;
            ExpandCollapseAllAction?.Invoke(IsTreeExpanded);
        }

        private async Task ShowErrorSummaryDialog(List<FileError> errors)
        {
            var errorView = new ErrorSummaryView { Errors = errors, SuccessCount = _allDtes.Count };
            var dialog = new ContentDialog { Title = "Resumen de Carga de Archivos", Content = errorView, CloseButtonText = "OK", XamlRoot = App.MainWindow.Content.XamlRoot };
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

        private static async Task ShowErrorDialog(string title, string content)
        {
            var dialog = new ContentDialog { Title = title, Content = content, CloseButtonText = "OK", XamlRoot = App.MainWindow.Content.XamlRoot };
            await dialog.ShowAsync();
        }

        [RelayCommand]
        private static async Task ShowAboutDialogAsync()
        {
            var aboutDialog = new ContentDialog { Title = "Acerca de Visor DTE CIPS", CloseButtonText = "Cerrar", XamlRoot = App.MainWindow.Content.XamlRoot, Content = new StackPanel { Spacing = 12, Children = { new TextBlock { Text = "Visor de Documentos Tributarios Electrónicos" }, new TextBlock { Text = "Versión: 1.0.0" }, new TextBlock { Text = "© 2025 Crazy Intelligence Programming Studio (CIPS)" }, new HyperlinkButton { Content = "Soporte: cips-support@outlook.com", NavigateUri = new Uri("mailto:cips-support@outlook.com") } } } };
            await aboutDialog.ShowAsync();
        }

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