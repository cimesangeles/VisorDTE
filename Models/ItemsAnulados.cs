// /Models/ItemsAnulados.cs
using System.Text.Json.Serialization;

namespace VisorDTE.Models;

public class ItemsAnulados
{
    [JsonPropertyName("tipoDoc")]
    public string TipoDoc { get; set; }

    [JsonPropertyName("numDocumento")]
    public string NumDocumento { get; set; }

    [JsonPropertyName("fechaAnulacion")]
    public string FechaAnulacion { get; set; }

    [JsonPropertyName("horaAnulacion")]
    public string HoraAnulacion { get; set; }
}