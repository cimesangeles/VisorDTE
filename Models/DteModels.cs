// /Models/DteModels.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace VisorDTE.Models;

// Clases actualizadas para soportar la estructura de CCF

public class Identificacion
{
    [JsonPropertyName("version")] public int Version { get; set; }
    [JsonPropertyName("ambiente")] public string Ambiente { get; set; }
    [JsonPropertyName("tipoDte")] public string TipoDte { get; set; }
    [JsonPropertyName("numeroControl")] public string NumeroControl { get; set; }
    [JsonPropertyName("codigoGeneracion")] public string CodigoGeneracion { get; set; }
    [JsonPropertyName("fecEmi")] public string FecEmi { get; set; }
    [JsonPropertyName("horEmi")] public string HorEmi { get; set; }
    [JsonPropertyName("tipoMoneda")] public string TipoMoneda { get; set; }
}

public class Direccion
{
    [JsonPropertyName("departamento")] public string Departamento { get; set; }
    [JsonPropertyName("municipio")] public string Municipio { get; set; }
    [JsonPropertyName("complemento")] public string Complemento { get; set; }
}

public class Emisor
{
    [JsonPropertyName("nit")] public string Nit { get; set; }
    [JsonPropertyName("nrc")] public string Nrc { get; set; }
    [JsonPropertyName("nombre")] public string Nombre { get; set; }
    [JsonPropertyName("codActividad")] public string CodActividad { get; set; }
    [JsonPropertyName("descActividad")] public string DescActividad { get; set; }
    [JsonPropertyName("nombreComercial")] public string NombreComercial { get; set; }
    [JsonPropertyName("tipoEstablecimiento")] public string TipoEstablecimiento { get; set; }
    [JsonPropertyName("direccion")] public Direccion Direccion { get; set; }
    [JsonPropertyName("telefono")] public string Telefono { get; set; }
    [JsonPropertyName("correo")] public string Correo { get; set; }
}

public class Receptor
{
    [JsonPropertyName("tipoDocumento")] public string TipoDocumento { get; set; }
    [JsonPropertyName("numDocumento")] public string NumDocumento { get; set; }
    [JsonPropertyName("nit")] public string Nit { get; set; }
    [JsonPropertyName("nrc")] public string Nrc { get; set; }
    [JsonPropertyName("nombre")] public string Nombre { get; set; }
    [JsonPropertyName("direccion")] public Direccion Direccion { get; set; }
    [JsonPropertyName("telefono")] public string Telefono { get; set; }
    [JsonPropertyName("correo")] public string Correo { get; set; }
}

public class CuerpoDocumento
{
    [JsonPropertyName("numItem")] public int NumItem { get; set; }
    [JsonPropertyName("cantidad")] public decimal Cantidad { get; set; }
    [JsonPropertyName("codigo")] public string Codigo { get; set; }
    [JsonPropertyName("uniMedida")] public int UniMedida { get; set; }
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; }
    [JsonPropertyName("precioUni")] public decimal PrecioUni { get; set; }
    [JsonPropertyName("montoDescu")] public decimal MontoDescu { get; set; }
    [JsonPropertyName("ventaGravada")] public decimal VentaGravada { get; set; }
    [JsonPropertyName("tributos")] public List<string> Tributos { get; set; }
}

public class TributoResumen
{
    [JsonPropertyName("codigo")] public string Codigo { get; set; }
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; }
    [JsonPropertyName("valor")] public decimal Valor { get; set; }
}

public class Extension
{
    [JsonPropertyName("nombEntrega")] public string NombEntrega { get; set; }
    [JsonPropertyName("docuEntrega")] public string DocuEntrega { get; set; }
    [JsonPropertyName("observaciones")] public string Observaciones { get; set; }
    [JsonPropertyName("placaVehiculo")] public string PlacaVehiculo { get; set; }
}

public class Resumen
{
    [JsonPropertyName("totalGravada")] public decimal TotalGravada { get; set; }
    [JsonPropertyName("totalDescu")] public decimal TotalDescu { get; set; }
    [JsonPropertyName("subTotal")] public decimal SubTotal { get; set; }
    [JsonPropertyName("ivaRete1")] public decimal? IvaRete1 { get; set; }
    [JsonPropertyName("totalIva")] public decimal? TotalIva { get; set; }
    [JsonPropertyName("tributos")] public List<TributoResumen> Tributos { get; set; }
    [JsonPropertyName("montoTotalOperacion")] public decimal MontoTotalOperacion { get; set; }
    [JsonPropertyName("totalPagar")] public decimal TotalPagar { get; set; }
    [JsonPropertyName("totalLetras")] public string TotalLetras { get; set; }
    [JsonPropertyName("condicionOperacion")] public int CondicionOperacion { get; set; }
}

// La interfaz IDte ya no está aquí.

public class Factura : IDte
{
    [JsonPropertyName("identificacion")] public Identificacion Identificacion { get; set; }
    [JsonPropertyName("emisor")] public Emisor Emisor { get; set; }
    [JsonPropertyName("receptor")] public Receptor Receptor { get; set; }
    [JsonPropertyName("cuerpoDocumento")] public List<CuerpoDocumento> CuerpoDocumento { get; set; }
    [JsonPropertyName("resumen")] public Resumen Resumen { get; set; }
    [JsonPropertyName("extension")] public Extension Extension { get; set; }
}

public class ComprobanteCreditoFiscal : IDte
{
    [JsonPropertyName("identificacion")] public Identificacion Identificacion { get; set; }
    [JsonPropertyName("emisor")] public Emisor Emisor { get; set; }
    [JsonPropertyName("receptor")] public Receptor Receptor { get; set; }
    [JsonPropertyName("cuerpoDocumento")] public List<CuerpoDocumento> CuerpoDocumento { get; set; }
    [JsonPropertyName("resumen")] public Resumen Resumen { get; set; }
    [JsonPropertyName("extension")] public Extension Extension { get; set; }
}