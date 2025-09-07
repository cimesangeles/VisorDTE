// /Services/DteParserService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using VisorDTE.Interfaces;
using VisorDTE.Models;       // <-- AÑADIDO: Para encontrar la clase Dte
using VisorDTE.Processors;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VisorDTE.Services;

public class DteParserService
{
    private readonly List<IDteProcessor> _processors;

    public DteParserService()
    {
        _processors =
        [
            new FacturaConsumidorFinalProcessor()
        ];
    }



    public Dte ParseDte(string jsonContent)
    {
        var jsonNode = JsonNode.Parse(jsonContent);
        var dteType = jsonNode?["identificacion"]?["tipoDte"]?.GetValue<string>();

        if (string.IsNullOrEmpty(dteType))
        {
            throw new InvalidDataException("El JSON no contiene el campo 'tipoDte' en la sección 'identificacion'.");
        }

        var processor = _processors.FirstOrDefault(p => p.HandledDteType == dteType);

        if (processor is null) // SIMPLIFICADO: Sugerencia del compilador (IDE0270).
        {
            throw new NotSupportedException($"El tipo de DTE '{dteType}' no es soportado. No se encontró un Add-on para procesarlo.");
        }

        return processor.Parse(jsonContent);
    }
}