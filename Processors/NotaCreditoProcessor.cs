// /Processors/NotaCreditoProcessor.cs
using System.IO;
using System.Text.Json;
using VisorDTE.Interfaces;
using VisorDTE.Models;

namespace VisorDTE.Processors;

public class NotaCreditoProcessor : IDteProcessor
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string HandledDteType => "05";
    public string DteTypeName => "Nota de Crédito";

    public Dte Parse(string jsonContent)
    {
        var dte = JsonSerializer.Deserialize<Dte>(jsonContent, _jsonOptions);

        if (dte is null)
        {
            throw new InvalidDataException("El archivo JSON no corresponde a una estructura de DTE válida.");
        }

        if (dte.Identificacion?.TipoDte != HandledDteType)
        {
            throw new InvalidDataException($"El archivo se intentó procesar como una '{DteTypeName}' (05), pero su contenido indica que es un tipo '{dte.Identificacion?.TipoDte}'.");
        }

        return dte;
    }
}