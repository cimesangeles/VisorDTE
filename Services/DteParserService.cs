// /Services/DteParserService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using VisorDTE.Interfaces;
using VisorDTE.Models;
using VisorDTE.Processors;

namespace VisorDTE.Services;

public class DteParserService
{
    private readonly List<IDteProcessor> _activeProcessors;

    public DteParserService(List<IDteProcessor> activeProcessors)
    {
        _activeProcessors = activeProcessors ?? [];
    }

    public Dte ParseDte(string jsonContent)
    {
        var jsonNode = JsonNode.Parse(jsonContent);
        var dteType = jsonNode?["identificacion"]?["tipoDte"]?.GetValue<string>();

        if (string.IsNullOrEmpty(dteType))
        {
            throw new InvalidDataException("El JSON no contiene el campo 'tipoDte' en la sección 'identificacion'.");
        }

        var processor = _activeProcessors.FirstOrDefault(p => p.HandledDteType == dteType);

        return processor is null
            ? throw new NotSupportedException($"El tipo de DTE '{dteType}' no es soportado. Por favor, adquiera el complemento correspondiente desde la tienda para habilitar esta funcionalidad.")
            : processor.Parse(jsonContent);
    }
}