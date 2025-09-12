// /Processors/DocumentoContableLiquidacionProcessor.cs
using System.IO;
using System.Text.Json;
using VisorDTE.Interfaces;
using VisorDTE.Models;

namespace VisorDTE.Processors;

public class DocumentoContableLiquidacionProcessor : IDteProcessor
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    public string HandledDteType => "09";
    public string DteTypeName => "Documento Contable de Liquidación";

    public Dte Parse(string jsonContent)
    {
        var dte = JsonSerializer.Deserialize<Dte>(jsonContent, _jsonOptions);
        if (dte?.Identificacion?.TipoDte != HandledDteType)
        {
            throw new InvalidDataException($"El archivo no es un '{DteTypeName}' válido.");
        }
        return dte;
    }
}