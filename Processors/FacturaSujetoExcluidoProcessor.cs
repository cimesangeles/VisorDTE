// /Processors/FacturaSujetoExcluidoProcessor.cs
using System.IO;
using System.Text.Json;
using VisorDTE.Interfaces;
using VisorDTE.Models;

namespace VisorDTE.Processors;

public class FacturaSujetoExcluidoProcessor : IDteProcessor
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    public string HandledDteType => "14";
    public string DteTypeName => "Factura de Sujeto Excluido";

    public Dte Parse(string jsonContent)
    {
        var dte = JsonSerializer.Deserialize<Dte>(jsonContent, _jsonOptions);
        if (dte?.Identificacion?.TipoDte != HandledDteType)
        {
            throw new InvalidDataException($"El archivo no es una '{DteTypeName}' válida.");
        }
        return dte;
    }
}