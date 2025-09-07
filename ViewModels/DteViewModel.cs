using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using VisorDTE.Models;
using VisorDTE.Services;

namespace VisorDTE.ViewModels;

public partial class DteViewModel(IDte dte, CatalogService catalogService) : ObservableObject
{
    public IDte Dte { get; } = dte;
    private readonly CatalogService _catalogService = catalogService;

    private static string GetCompleteAddress(Direccion direccion)
    {
        if (direccion == null) return "Dirección no especificada";
        return $"{direccion.DireccionComplemento}, {direccion.Municipio}, {direccion.Departamento}";
    }

    public string EmisorDireccionCompleta => GetCompleteAddress(Dte.Emisor?.Direccion);
    public string ReceptorDireccionCompleta => GetCompleteAddress(Dte.Receptor?.Direccion);
    public string TipoDteDescripcion => _catalogService.GetDescription("CAT-002-TipoDocumento", Dte.Identificacion.TipoDte);
    public string CondicionOperacionDescripcion => _catalogService.GetDescription("CAT-016-CondicionOperacion", Dte.Resumen.CondicionOperacion.ToString());
    public double TotalIva => Dte.Resumen.Tributos?.FirstOrDefault(t => t.Codigo == "20")?.Valor ?? 0;

    // --- NUEVO MÉTODO PÚBLICO ---
    // Este método permite que otras clases usen el servicio de catálogos de forma segura.
    public string GetCatalogDescription(string catalogName, string code)
    {
        return _catalogService.GetDescription(catalogName, code);
    }
}