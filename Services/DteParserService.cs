// /Services/DteParserService.cs
using System;
using System.Text.Json;
using VisorDTE.Models;

namespace VisorDTE.Services;

public class DteParserService
{
    public IDte ParseDte(string jsonContent)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var jsonDocument = JsonDocument.Parse(jsonContent);

            if (jsonDocument.RootElement.TryGetProperty("dteJson", out var dteJsonElement))
            {
                jsonContent = dteJsonElement.GetRawText();
                jsonDocument = JsonDocument.Parse(jsonContent);
            }

            var tipoDte = jsonDocument.RootElement
                .GetProperty("identificacion")
                .GetProperty("tipoDte")
                .GetString();

            return tipoDte switch
            {
                "01" => JsonSerializer.Deserialize<Factura>(jsonContent, jsonOptions),
                "03" => JsonSerializer.Deserialize<ComprobanteCreditoFiscal>(jsonContent, jsonOptions),
                _ => throw new NotSupportedException($"El tipo de DTE '{tipoDte}' no es soportado.")
            };
        }
        catch (Exception ex)
        {
            throw new JsonException($"Error al parsear el DTE. Verifique el formato. Detalle: {ex.Message}");
        }
    }
}