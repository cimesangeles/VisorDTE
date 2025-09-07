// /Models/Apendice.cs
using System.Text.Json.Serialization;

namespace VisorDTE.Models;

public class Apendice
{
    [JsonPropertyName("campo")]
    public string Campo { get; set; }

    [JsonPropertyName("etiqueta")]
    public string Etiqueta { get; set; }

    [JsonPropertyName("valor")]
    public string Valor { get; set; }
}