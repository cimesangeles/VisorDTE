// /Processors/ComprobanteCreditoFiscalProcessor.cs
using System.IO;
using System.Text.Json;
using VisorDTE.Interfaces;
using VisorDTE.Models;

namespace VisorDTE.Processors;

public class ComprobanteCreditoFiscalProcessor : IDteProcessor
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string HandledDteType => "03";
    public string DteTypeName => "Comprobante de Crédito Fiscal";

    public Dte Parse(string jsonContent)
    {
        var dte = JsonSerializer.Deserialize<Dte>(jsonContent, _jsonOptions);

        if (dte is null)
        {
            throw new InvalidDataException("El archivo JSON no corresponde a una estructura de DTE válida.");
        }

        if (dte.Identificacion?.TipoDte != HandledDteType)
        {
            throw new InvalidDataException($"El archivo se intentó procesar como un '{DteTypeName}' (03), pero su contenido indica que es un tipo '{dte.Identificacion?.TipoDte}'.");
        }

        return dte;
    }
}