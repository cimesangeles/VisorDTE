// /ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VisorDTE.Services;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace VisorDTE.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<DteViewModel> _dteViewModels = new();

    private List<DteViewModel> _allDtes = new();

    [ObservableProperty]
    private string _statusText = "Listo para abrir archivos DTE (.json)";

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    private readonly CatalogService _catalogService;
    private readonly DteParserService _parserService;
    private readonly PdfExportService _pdfExportService;

    public MainViewModel()
    {
        _catalogService = new CatalogService();
        _parserService = new DteParserService();
        _pdfExportService = new PdfExportService();
    }

    partial void OnSearchQueryChanged(string value)
    {
        FilterDtes();
    }

    private void FilterDtes()
    {
        DteViewModels.Clear();

        IEnumerable<DteViewModel> filtered;

        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            filtered = _allDtes;
        }
        else
        {
            filtered = _allDtes.Where(vm =>
                vm.Dte.Identificacion.NumeroControl.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)
            );
        }

        foreach (var vm in filtered)
        {
            DteViewModels.Add(vm);
        }
    }

    [RelayCommand]
    private async Task OpenFilesAsync()
    {
        try
        {
            await _catalogService.InitializeAsync();

            var filePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            };
            filePicker.FileTypeFilter.Add(".json");

            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(filePicker, hwnd);

            var files = await filePicker.PickMultipleFilesAsync();
            if (files is null || files.Count == 0) return;

            _allDtes.Clear();
            StatusText = "Cargando archivos...";

            foreach (var file in files)
            {
                try
                {
                    var jsonContent = await File.ReadAllTextAsync(file.Path);
                    var dteModel = _parserService.ParseDte(jsonContent);
                    var dteViewModel = new DteViewModel(dteModel, _catalogService);
                    _allDtes.Add(dteViewModel);
                }
                catch (Exception ex)
                {
                    StatusText = $"Error al leer {file.Name}.";
                    await ShowErrorDialog($"No se pudo procesar el archivo '{file.Name}'.", $"Detalle: {ex.Message}");
                }
            }

            _allDtes = _allDtes
                .OrderBy(vm => vm.Dte.Identificacion.FecEmi)
                .ThenBy(vm => vm.Dte.Identificacion.HorEmi)
                .ToList();

            FilterDtes();
            StatusText = $"{_allDtes.Count} DTE(s) cargados correctamente.";
        }
        catch (Exception ex)
        {
            StatusText = "Error al intentar abrir archivos.";
            await ShowErrorDialog("Ocurrió un error inesperado", $"Detalle: {ex.Message}");
        }
    }

    private async Task ShowErrorDialog(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "OK",
            XamlRoot = App.MainWindow.Content.XamlRoot
        };
        await dialog.ShowAsync();
    }

    [RelayCommand]
    private async Task ExportToPdfAsync()
    {
        if (DteViewModels.Count == 0)
        {
            StatusText = "No hay DTEs para exportar.";
            return;
        }

        // ===== CAMBIO CLAVE: Añadimos un bloque try-catch =====
        try
        {
            var fileSaver = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = $"Exportacion_DTE_{DateTime.Now:yyyyMMdd}"
            };
            fileSaver.FileTypeChoices.Add("Archivo PDF", new List<string> { ".pdf" });

            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(fileSaver, hwnd);

            var file = await fileSaver.PickSaveFileAsync();
            if (file != null)
            {
                StatusText = "Exportando a PDF...";
                // La generación del PDF en sí ya estaba en un try-catch, lo mantenemos
                try
                {
                    await Task.Run(() => _pdfExportService.ExportAsPdf(file.Path, DteViewModels.ToList()));
                    StatusText = $"Exportado a {file.Name} correctamente.";
                }
                catch (Exception ex)
                {
                    await ShowErrorDialog("Error al generar el PDF", $"Detalle: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            // Este catch capturará errores al mostrar el diálogo de guardar
            StatusText = "Error al exportar.";
            await ShowErrorDialog("No se pudo mostrar el diálogo para guardar", $"Detalle: {ex.Message}");
        }
    }
}