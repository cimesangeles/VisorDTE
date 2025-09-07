// /Processors/FacturaConsumidorFinalProcessor.cs
using System.IO;
using System.Text.Json;
using VisorDTE.Interfaces;
using VisorDTE.Models; // <-- AÑADIDO: Para encontrar la clase Dte

namespace VisorDTE.Processors;

public class FacturaConsumidorFinalProcessor : IDteProcessor
{
    // CACHEADO: Para mejorar el rendimiento, creamos las opciones una sola vez.
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string HandledDteType => "01";
    public string DteTypeName => "Factura de Consumidor Final";

    public Dte Parse(string jsonContent)
    {
        var dte = JsonSerializer.Deserialize<Dte>(jsonContent, _jsonOptions);

        // Validamos que el JSON se haya podido convertir al objeto Dte.
        if (dte is null) // SIMPLIFICADO: Sugerencia del compilador (IDE0270).
        {
            throw new InvalidDataException("El archivo JSON no corresponde a una estructura de DTE válida.");
        }

        return dte;
    }
}