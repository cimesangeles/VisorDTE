// /ViewModels/DteViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input; // <-- AÑADIR ESTE USING
using System.Linq;
using System.Threading.Tasks;
using VisorDTE.Models;
using VisorDTE.Services;

namespace VisorDTE.ViewModels
{
    public partial class DteViewModel : ObservableObject
    {
        public IDte Dte { get; }
        private readonly CatalogService _catalogService;

        // --- INICIO DE LA MODIFICACIÓN 1 ---
        public IRelayCommand<string> CopyToClipboardCommand { get; }

        private DteViewModel(IDte dte, CatalogService catalogService, IRelayCommand<string> copyCommand)
        {
            Dte = dte;
            _catalogService = catalogService;
            CopyToClipboardCommand = copyCommand;
        }

        public static async Task<DteViewModel> CreateAsync(IDte dte, CatalogService catalogService, IRelayCommand<string> copyCommand)
        {
            await catalogService.InitializeAsync();
            return new DteViewModel(dte, catalogService, copyCommand);
        }
        // --- FIN DE LA MODIFICACIÓN 1 ---

        public string EmisorDireccionCompleta => GetCompleteAddress(Dte.Emisor?.Direccion);
        public string ReceptorDireccionCompleta => GetCompleteAddress(Dte.Receptor?.Direccion);
        public string TipoDteDescripcion => _catalogService.GetDescription("CAT-002-TipoDocumento", Dte.Identificacion.TipoDte);
        public string CondicionOperacionDescripcion => _catalogService.GetDescription("CAT-016-CondicionOperacion", Dte.Resumen.CondicionOperacion.ToString());

        public bool HasAnnulledItems => Dte.ItemsAnulados != null && Dte.ItemsAnulados.Any();

        private string GetCompleteAddress(Direccion direccion)
        {
            if (direccion == null) return "Dirección no especificada";
            var departamento = _catalogService.GetDescription("CAT-012-Departamento", direccion.Departamento);
            var municipio = _catalogService.GetDescription("CAT-013-Municipio", direccion.Municipio);
            return $"{direccion.DireccionComplemento}, {municipio}, {departamento}";
        }

        public string GetTranslatedValue(string propertyName, string code)
        {
            if (string.IsNullOrEmpty(code)) return "N/A";

            return propertyName switch
            {
                "TipoDte" => _catalogService.GetDescription("CAT-002-TipoDocumento", code),
                "Departamento" => _catalogService.GetDescription("CAT-012-Departamento", code),
                "Municipio" => _catalogService.GetDescription("CAT-013-Municipio", code),
                "CondicionOperacion" => _catalogService.GetDescription("CAT-016-CondicionOperacion", code),
                "TipoEstablecimiento" => _catalogService.GetDescription("CAT-009-TipoEstablecimiento", code),
                "UnidadMedida" => _catalogService.GetDescription("CAT-014-UnidadMedida", code),
                _ => code
            };
        }

        public double TotalIva => Dte.Resumen.Tributos?.FirstOrDefault(t => t.Codigo == "20")?.Valor ?? 0;
    }
}