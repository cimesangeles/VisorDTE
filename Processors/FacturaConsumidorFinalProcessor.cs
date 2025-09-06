// /Processors/FacturaConsumidorFinalProcessor.cs
using System.IO;
using System.Text.Json;
using VisorDTE.Interfaces;
using VisorDTE.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VisorDTE.Processors;

/// <summary>
/// Este es nuestro primer "Add-on".
/// Es el procesador especializado en leer Documentos de Consumidor Final (Tipo DTE 01).
/// </summary>
public class FacturaConsumidorFinalProcessor : IDteProcessor
{
    // Implementación del contrato IDteProcessor:

    // 1. Le decimos a la aplicación qué tipo de DTE manejamos.
    public string HandledDteType => "01";

    // 2. Le damos un nombre amigable para mostrar en la interfaz.
    public string DteTypeName => "Factura de Consumidor Final";

    // 3. Definimos cómo se debe parsear este tipo de documento.
    public Dte Parse(string jsonContent)
    {
        // Esta es la lógica de deserialización que moveremos desde DteParserService.
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var dte = JsonSerializer.Deserialize<Dte>(jsonContent, options);

        // Validamos que el JSON se haya podido convertir al objeto Dte.
        if (dte == null)
        {
            throw new InvalidDataException("El archivo JSON no corresponde a una estructura de DTE válida.");
        }

        // Aquí se podrían añadir validaciones futuras específicas para Facturas (si fuera necesario).

        return dte;
    }
}