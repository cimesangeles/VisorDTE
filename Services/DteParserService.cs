// /Services/DteParserService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using VisorDTE.Interfaces;
using VisorDTE.Models;
using VisorDTE.Processors; // Importante: añadir el using para encontrar los procesadores

namespace VisorDTE.Services;

/// <summary>
/// Este servicio ahora actúa como un administrador.
/// No sabe cómo parsear ningún DTE, pero sabe a quién preguntarle.
/// </summary>
public class DteParserService
{
    // Una lista privada que contiene todos los "Add-ons" disponibles para la aplicación.
    private readonly List<IDteProcessor> _processors;

    public DteParserService()
    {
        // Al iniciar, registramos todos los procesadores que hemos creado.
        // Si mañana creamos un Add-on para Notas de Crédito, simplemente lo añadimos aquí.
        _processors =
        [
            new FacturaConsumidorFinalProcessor()
            // new NotaDeCreditoProcessor(), <-- Así se añadiría el siguiente
        ];
    }

    public Dte ParseDte(string jsonContent)
    {
        // 1. "Espiamos" el JSON para ver qué tipo de DTE es, sin procesar el archivo completo.
        var jsonNode = JsonNode.Parse(jsonContent);
        var dteType = jsonNode?["identificacion"]?["tipoDte"]?.GetValue<string>();

        if (string.IsNullOrEmpty(dteType))
        {
            throw new InvalidDataException("El JSON no contiene el campo 'tipoDte' en la sección 'identificacion'.");
        }

        // 2. Buscamos en nuestra lista de Add-ons si alguno puede manejar este tipo de DTE.
        var processor = _processors.FirstOrDefault(p => p.HandledDteType == dteType);

        // 3. Si no encontramos a ningún experto, lanzamos un error claro.
        if (processor == null)
        {
            throw new NotSupportedException($"El tipo de DTE '{dteType}' no es soportado. No se encontró un Add-on para procesarlo.");
        }

        // 4. Si lo encontramos, le delegamos todo el trabajo de parseo.
        return processor.Parse(jsonContent);
    }
}