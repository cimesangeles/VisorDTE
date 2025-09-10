// /ViewModels/DteViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using VisorDTE.Models;
using VisorDTE.Services;

namespace VisorDTE.ViewModels;

public partial class DteViewModel : ObservableObject
{
    public IDte Dte { get; }
    // --- INICIO DE LA MODIFICACIÓN 1: El campo ahora es privado ---
    private readonly CatalogService _catalogService;

    private DteViewModel(IDte dte, CatalogService catalogService)
    {
        Dte = dte;
        _catalogService = catalogService;
    }

    public static async Task<DteViewModel> CreateAsync(IDte dte, CatalogService catalogService)
    {
        await catalogService.InitializeAsync();
        return new DteViewModel(dte, catalogService);
    }

    // Propiedades que construyen la dirección completa
    public string EmisorDireccionCompleta => GetCompleteAddress(Dte.Emisor?.Direccion);
    public string ReceptorDireccionCompleta => GetCompleteAddress(Dte.Receptor?.Direccion);

    public string TipoDteDescripcion => _catalogService.GetDescription("CAT-002-TipoDocumento", Dte.Identificacion.TipoDte);
    public string CondicionOperacionDescripcion => _catalogService.GetDescription("CAT-016-CondicionOperacion", Dte.Resumen.CondicionOperacion.ToString());

    private string GetCompleteAddress(Direccion direccion)
    {
        if (direccion == null) return "Dirección no especificada";

        var departamento = _catalogService.GetDescription("CAT-012-Departamento", direccion.Departamento);
        var municipio = _catalogService.GetDescription("CAT-013-Municipio", direccion.Municipio);

        return $"{direccion.DireccionComplemento}, {municipio}, {departamento}";
    }

    // --- INICIO DE LA MODIFICACIÓN 2: Nuevo método público para traducciones ---
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
            _ => code // Devuelve el código original si no hay una traducción específica
        };
    }
    // --- FIN DE LA MODIFICACIÓN 2 ---

    public double TotalIva => Dte.Resumen.Tributos?.FirstOrDefault(t => t.Codigo == "20")?.Valor ?? 0;
}