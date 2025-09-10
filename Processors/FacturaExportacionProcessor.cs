// /Processors/FacturaExportacionProcessor.cs
using System.IO;
using System.Text.Json;
using VisorDTE.Interfaces;
using VisorDTE.Models;

namespace VisorDTE.Processors;

public class FacturaExportacionProcessor : IDteProcessor
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string HandledDteType => "11";
    public string DteTypeName => "Factura de Exportación";

    public Dte Parse(string jsonContent)
    {
        var dte = JsonSerializer.Deserialize<Dte>(jsonContent, _jsonOptions);

        if (dte is null)
        {
            throw new InvalidDataException("El archivo JSON no corresponde a una estructura de DTE válida.");
        }

        if (dte.Identificacion?.TipoDte != HandledDteType)
        {
            throw new InvalidDataException($"El archivo se intentó procesar como una '{DteTypeName}' (11), pero su contenido indica que es un tipo '{dte.Identificacion?.TipoDte}'.");
        }

        return dte;
    }
}