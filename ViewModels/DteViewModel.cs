using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VisorDTE.Models;
using VisorDTE.Services;
using Windows.ApplicationModel.DataTransfer;

namespace VisorDTE.ViewModels
{
    public partial class DteViewModel : ObservableObject
    {
        public IDte Dte { get; }
        internal readonly CatalogService _catalogService; // Cambia de private a protected

        public DteViewModel(IDte dte, CatalogService catalogService)
        {
            Dte = dte;
            _catalogService = catalogService;
        }

        [RelayCommand]
        private void CopyToClipboard(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
        }

        public string TipoDteDescripcion => _catalogService.GetDescription("CAT-002-TipoDocumento", Dte.Identificacion.TipoDte);

        public string EmisorDireccionCompleta => Dte.Emisor?.Direccion != null ?
            $"{Dte.Emisor.Direccion.Complemento}, " +
            $"{_catalogService.GetDescription("CAT-013-Municipio", Dte.Emisor.Direccion.Municipio)}, " +
            $"{_catalogService.GetDescription("CAT-012-Departamento", Dte.Emisor.Direccion.Departamento)}" : "N/A";

        public string ReceptorDireccionCompleta => Dte.Receptor?.Direccion != null ?
            $"{Dte.Receptor.Direccion.Complemento}, " +
            $"{_catalogService.GetDescription("CAT-013-Municipio", Dte.Receptor.Direccion.Municipio)}, " +
            $"{_catalogService.GetDescription("CAT-012-Departamento", Dte.Receptor.Direccion.Departamento)}" : "N/A";

        public string CondicionOperacionDescripcion => _catalogService.GetDescription("CAT-016-CondicionOperacion", Dte.Resumen.CondicionOperacion.ToString());

        public decimal IvaCalculado => Dte.Resumen.TotalIva ?? Dte.Resumen.IvaRete1 ?? 0;
    }
}