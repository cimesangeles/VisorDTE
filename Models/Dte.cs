// /Models/Dte.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VisorDTE.Models;

/// <summary>
/// Representa la estructura raíz de un Documento Tributario Electrónico (DTE).
/// Esta clase es el modelo principal que contiene todas las secciones del JSON.
/// </summary>
public class Dte
{
    [JsonPropertyName("identificacion")]
    public Identificacion Identificacion { get; set; }

    [JsonPropertyName("emisor")]
    public Emisor Emisor { get; set; }

    [JsonPropertyName("receptor")]
    public Receptor Receptor { get; set; }

    [JsonPropertyName("cuerpoDocumento")]
    public List<CuerpoDocumento> CuerpoDocumento { get; set; }

    [JsonPropertyName("resumen")]
    public Resumen Resumen { get; set; }

    [JsonPropertyName("extension")]
    public Extension Extension { get; set; }

    [JsonPropertyName("apendice")]
    public List<Apendice> Apendice { get; set; }

    // Este campo puede no estar presente en todos los DTE, por eso es anulable.
    [JsonPropertyName("itemsAnulados")]
    public List<ItemsAnulados>? ItemsAnulados { get; set; }
}