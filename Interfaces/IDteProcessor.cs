// /Interfaces/IDteProcessor.cs
using VisorDTE.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VisorDTE.Interfaces;

public interface IDteProcessor
{
    /// <summary>
    /// Identificador único del tipo de DTE que este procesador maneja (ej: "01" para Factura, "03" para Crédito Fiscal).
    /// </summary>
    string HandledDteType { get; }

    /// <summary>
    /// El nombre descriptivo del documento para mostrar en la interfaz (ej: "Factura de Consumidor Final").
    /// </summary>
    string DteTypeName { get; }

    /// <summary>
    /// Parsea el contenido JSON y lo convierte en el modelo de datos DTE principal.
    /// </summary>
    /// <param name="jsonContent">El string JSON del archivo que se va a procesar.</param>
    /// <returns>Un objeto Dte con todos los datos extraídos.</returns>
    Dte Parse(string jsonContent);
}