// /Processors/ComprobanteDonacionProcessor.cs
using System.IO;
using System.Text.Json;
using VisorDTE.Interfaces;
using VisorDTE.Models;

namespace VisorDTE.Processors;

public class ComprobanteDonacionProcessor : IDteProcessor
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    public string HandledDteType => "15";
    public string DteTypeName => "Comprobante de Donación";

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