using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VisorDTE.Models
{
    public class Identificacion
    {
        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("ambiente")]
        public string Ambiente { get; set; }

        [JsonPropertyName("tipoDte")]
        public string TipoDte { get; set; }

        [JsonPropertyName("numeroControl")]
        public string NumeroControl { get; set; }

        [JsonPropertyName("codigoGeneracion")]
        public string CodigoGeneracion { get; set; }

        [JsonPropertyName("tipoModelo")]
        public int TipoModelo { get; set; }

        [JsonPropertyName("tipoOperacion")]
        public int TipoOperacion { get; set; }

        [JsonPropertyName("tipoContingencia")]
        public object TipoContingencia { get; set; }

        [JsonPropertyName("motivoContin")]
        public object MotivoContin { get; set; }

        [JsonPropertyName("fecEmi")]
        public string FecEmi { get; set; }

        [JsonPropertyName("horEmi")]
        public string HorEmi { get; set; }

        [JsonPropertyName("tipoMoneda")]
        public string TipoMoneda { get; set; }
    }

    public class Direccion
    {
        [JsonPropertyName("departamento")]
        public string Departamento { get; set; }

        [JsonPropertyName("municipio")]
        public string Municipio { get; set; }

        [JsonPropertyName("direccionComplemento")]
        public string DireccionComplemento { get; set; }
    }

    public class Emisor
    {
        [JsonPropertyName("nit")]
        public string Nit { get; set; }

        [JsonPropertyName("nrc")]
        public string Nrc { get; set; }

        [JsonPropertyName("numDocumento")]
        public string? NumDocumento { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("codActividad")]
        public string CodActividad { get; set; }

        [JsonPropertyName("descActividad")]
        public string DescActividad { get; set; }

        [JsonPropertyName("nombreComercial")]
        public string NombreComercial { get; set; }

        [JsonPropertyName("tipoEstablecimiento")]
        public string TipoEstablecimiento { get; set; }

        [JsonPropertyName("direccion")]
        public Direccion Direccion { get; set; }

        [JsonPropertyName("telefono")]
        public string Telefono { get; set; }

        [JsonPropertyName("correo")]
        public string Correo { get; set; }
    }

    public class Receptor
    {
        [JsonPropertyName("nit")]
        public string? Nit { get; set; }

        [JsonPropertyName("nrc")]
        public string? Nrc { get; set; }

        [JsonPropertyName("numDocumento")]
        public string? NumDocumento { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("direccion")]
        public Direccion Direccion { get; set; }

        [JsonPropertyName("correo")]
        public string Correo { get; set; }

        [JsonPropertyName("telefono")]
        public string Telefono { get; set; }
    }

    public class CuerpoDocumento
    {
        [JsonPropertyName("numItem")]
        public int NumItem { get; set; }

        [JsonPropertyName("tipoItem")]
        public int TipoItem { get; set; }

        [JsonPropertyName("cantidad")]
        public double Cantidad { get; set; }

        [JsonPropertyName("codigo")]
        public string Codigo { get; set; }

        [JsonPropertyName("uniMedida")]
        public int UniMedida { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("precioUni")]
        public double PrecioUni { get; set; }

        [JsonPropertyName("montoDescu")]
        public double MontoDescu { get; set; }

        [JsonPropertyName("ventaNoSuj")]
        public double VentaNoSuj { get; set; }

        [JsonPropertyName("ventaExenta")]
        public double VentaExenta { get; set; }

        [JsonPropertyName("ventaGravada")]
        public double VentaGravada { get; set; }

        [JsonPropertyName("tributos")]
        public List<string> Tributos { get; set; }

        [JsonPropertyName("psrv")]
        public double Psrv { get; set; }
    }

    public class TributoResumen
    {
        [JsonPropertyName("codigo")]
        public string Codigo { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("valor")]
        public double Valor { get; set; }
    }

    public class Resumen
    {
        [JsonPropertyName("totalNoSuj")]
        public double TotalNoSuj { get; set; }

        [JsonPropertyName("totalExenta")]
        public double TotalExenta { get; set; }

        [JsonPropertyName("totalGravada")]
        public double TotalGravada { get; set; }

        [JsonPropertyName("subTotalVentas")]
        public double SubTotalVentas { get; set; }

        [JsonPropertyName("descuNoSuj")]
        public double DescuNoSuj { get; set; }

        [JsonPropertyName("descuExenta")]
        public double DescuExenta { get; set; }

        [JsonPropertyName("descuGravada")]
        public double DescuGravada { get; set; }

        [JsonPropertyName("porcentajeDescuento")]
        public double PorcentajeDescuento { get; set; }

        [JsonPropertyName("totalDescu")]
        public double TotalDescu { get; set; }

        [JsonPropertyName("tributos")]
        public List<TributoResumen> Tributos { get; set; }

        [JsonPropertyName("subTotal")]
        public double SubTotal { get; set; }

        [JsonPropertyName("ivaRete1")]
        public double IvaRete1 { get; set; }

        [JsonPropertyName("ivaPerci1")]
        public double IvaPerci1 { get; set; }

        [JsonPropertyName("reteRenta")]
        public double ReteRenta { get; set; }

        [JsonPropertyName("montoTotalOperacion")]
        public double MontoTotalOperacion { get; set; }

        [JsonPropertyName("totalNoGravado")]
        public double TotalNoGravado { get; set; }

        [JsonPropertyName("totalPagar")]
        public double TotalPagar { get; set; }

        [JsonPropertyName("totalLetras")]
        public string TotalLetras { get; set; }

        [JsonPropertyName("condicionOperacion")]
        public int CondicionOperacion { get; set; }

        [JsonPropertyName("pagos")]
        public object Pagos { get; set; }
    }

    public class Extension
    {
        [JsonPropertyName("nombEntrega")]
        public object NombEntrega { get; set; }

        [JsonPropertyName("docuEntrega")]
        public object DocuEntrega { get; set; }

        [JsonPropertyName("nombRecibe")]
        public object NombRecibe { get; set; }

        [JsonPropertyName("docuRecibe")]
        public object DocuRecibe { get; set; }

        [JsonPropertyName("observaciones")]
        public object Observaciones { get; set; }

        [JsonPropertyName("placaVehiculo")]
        public object PlacaVehiculo { get; set; }
    }

    public class Apendice
    {
        [JsonPropertyName("campo")]
        public string Campo { get; set; }

        [JsonPropertyName("etiqueta")]
        public string Etiqueta { get; set; }

        [JsonPropertyName("valor")]
        public string Valor { get; set; }
    }

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

    public interface IDte
    {
        Identificacion Identificacion { get; set; }
        Emisor Emisor { get; set; }
        Receptor Receptor { get; set; }
        List<CuerpoDocumento> CuerpoDocumento { get; set; }
        Resumen Resumen { get; set; }
        Extension Extension { get; set; }
        List<Apendice> Apendice { get; set; }
        List<ItemsAnulados>? ItemsAnulados { get; set; }
    }

    public abstract class DteBase : IDte
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

        [JsonPropertyName("itemsAnulados")]
        public List<ItemsAnulados>? ItemsAnulados { get; set; }
    }

    public class Factura : DteBase
    {
    }

    public class ComprobanteCreditoFiscal : DteBase
    {
    }
}