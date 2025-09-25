// /Services/F07AnexoCsvService.cs
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VisorDTE.Models;

namespace VisorDTE.Services
{
    public class F07AnexoCsvService
    {
        // ... (Los métodos GenerateAnexoConsumidorFinalCsv, GenerateAnexoVentasContribuyenteCsv, GenerateAnexoComprasCsv y GenerateAnexoDocumentosAnuladosCsv permanecen aquí sin cambios)

        /// <summary>
        /// Genera el Anexo de Ventas a Consumidor Final (para DTE 01 y 11), agrupado por día.
        /// </summary>
        public async Task<byte[]> GenerateAnexoConsumidorFinalCsv(IEnumerable<IDte> dtes)
        {
            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, System.Text.Encoding.UTF8))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    HasHeaderRecord = false
                };

                using var csv = new CsvWriter(writer, config);

                var groupedByDate = dtes
                    .Where(d => d.Identificacion?.TipoDte == "01" || d.Identificacion?.TipoDte == "11")
                    .GroupBy(d => d.Identificacion.FecEmi)
                    .OrderBy(g => DateTime.Parse(g.Key));

                foreach (var dayGroup in groupedByDate)
                {
                    var orderedDtesInDay = dayGroup.OrderBy(d => TimeSpan.Parse(d.Identificacion.HorEmi)).ToList();
                    var firstDte = orderedDtesInDay.First();
                    var lastDte = orderedDtesInDay.Last();

                    var totalExenta = dayGroup.Sum(d => d.Resumen.TotalExenta);
                    var totalGravada = dayGroup.Sum(d => d.Resumen.TotalGravada);
                    var totalNoSuj = dayGroup.Sum(d => d.Resumen.TotalNoSuj);
                    var montoTotalOperacion = dayGroup.Sum(d => d.Resumen.MontoTotalOperacion);
                    var totalIva = dayGroup.Sum(d => d.Resumen.Tributos?.FirstOrDefault(t => t.Codigo == "20")?.Valor ?? 0);
                    var otrosTributos = dayGroup.Sum(d => d.Resumen.Tributos?.Where(t => t.Codigo != "20").Sum(t => t.Valor) ?? 0);

                    var record = new object[]
                    {
                        DateTime.Parse(dayGroup.Key).ToString("dd/MM/yyyy"),
                        firstDte.Identificacion.TipoDte == "11" ? 5 : 4,
                        firstDte.Identificacion.TipoDte,
                        "N/A", "N/A", "N/A", "N/A",
                        firstDte.Identificacion.CodigoGeneracion,
                        lastDte.Identificacion.CodigoGeneracion,
                        "",
                        totalExenta.ToString("F2", CultureInfo.InvariantCulture),
                        "0.00", "0.00",
                        totalGravada.ToString("F2", CultureInfo.InvariantCulture),
                        "0.00", "0.00",
                        totalIva.ToString("F2", CultureInfo.InvariantCulture),
                        totalNoSuj.ToString("F2", CultureInfo.InvariantCulture),
                        otrosTributos.ToString("F2", CultureInfo.InvariantCulture),
                        montoTotalOperacion.ToString("F2", CultureInfo.InvariantCulture),
                        firstDte.Resumen.CondicionOperacion,
                        2,
                        2
                    };

                    foreach (var field in record) csv.WriteField(field);
                    csv.NextRecord();
                }
            }
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Genera el Anexo de Ventas a Contribuyentes (para DTE 03), detallado por documento.
        /// </summary>
        public async Task<byte[]> GenerateAnexoVentasContribuyenteCsv(IEnumerable<IDte> dtes)
        {
            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, System.Text.Encoding.UTF8))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    HasHeaderRecord = false
                };
                using var csv = new CsvWriter(writer, config);

                var contribuyenteDtes = dtes
                    .Where(d => d.Identificacion?.TipoDte == "03")
                    .OrderBy(d => DateTime.Parse(d.Identificacion.FecEmi))
                    .ThenBy(d => TimeSpan.Parse(d.Identificacion.HorEmi));

                foreach (var dte in contribuyenteDtes)
                {
                    var cuerpo = dte.CuerpoDocumento ?? new List<CuerpoDocumento>();
                    var bienesExentos = cuerpo.Where(c => c.TipoItem == 1).Sum(c => c.VentaExenta);
                    var serviciosExentos = cuerpo.Where(c => c.TipoItem == 2).Sum(c => c.VentaExenta);
                    var bienesGravados = cuerpo.Where(c => c.TipoItem == 1).Sum(c => c.VentaGravada);
                    var serviciosGravados = cuerpo.Where(c => c.TipoItem == 2).Sum(c => c.VentaGravada);
                    var ventaTerceroNoSujeto = 0.0; // dte.ventaTercero?.totalNoSuj ?? 0; // Esta propiedad no existe en el modelo Dte
                    var iva = dte.Resumen.Tributos?.FirstOrDefault(t => t.Codigo == "20")?.Valor;

                    var record = new object[]
                    {
                        DateTime.Parse(dte.Identificacion.FecEmi).ToString("dd/MM/yyyy"),
                        1,
                        dte.Identificacion.TipoDte,
                        "N/A",
                        "N/A",
                        dte.Receptor?.Nit ?? "",
                        dte.Receptor?.Nrc ?? "",
                        dte.Identificacion.CodigoGeneracion,
                        "",
                        dte.Receptor?.Nombre ?? "",
                        dte.Resumen.TotalExenta.ToString("F2", CultureInfo.InvariantCulture),
                        bienesExentos.ToString("F2", CultureInfo.InvariantCulture),
                        serviciosExentos.ToString("F2", CultureInfo.InvariantCulture),
                        dte.Resumen.TotalGravada.ToString("F2", CultureInfo.InvariantCulture),
                        bienesGravados.ToString("F2", CultureInfo.InvariantCulture),
                        serviciosGravados.ToString("F2", CultureInfo.InvariantCulture),
                        iva?.ToString("F2", CultureInfo.InvariantCulture) ?? "0.00",
                        dte.Resumen.IvaRete1.ToString("F2", CultureInfo.InvariantCulture),
                        dte.Resumen.ReteRenta.ToString("F2", CultureInfo.InvariantCulture),
                        dte.Resumen.TotalNoSuj.ToString("F2", CultureInfo.InvariantCulture),
                        ventaTerceroNoSujeto.ToString("F2", CultureInfo.InvariantCulture),
                        dte.Resumen.MontoTotalOperacion.ToString("F2", CultureInfo.InvariantCulture),
                        dte.Resumen.CondicionOperacion,
                        2,
                        2
                    };

                    foreach (var field in record) csv.WriteField(field);
                    csv.NextRecord();
                }
            }
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Genera el Anexo de Compras (Anexo 3), basado en la estructura del manual F-07.
        /// </summary>
        public async Task<byte[]> GenerateAnexoComprasCsv(IEnumerable<IDte> dtes)
        {
            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, System.Text.Encoding.UTF8))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    HasHeaderRecord = false
                };
                using var csv = new CsvWriter(writer, config);

                var purchaseDtes = dtes
                    .Where(d => d.Identificacion?.TipoDte == "03" || d.Identificacion?.TipoDte == "05" || d.Identificacion?.TipoDte == "06")
                    .OrderBy(d => DateTime.Parse(d.Identificacion.FecEmi))
                    .ThenBy(d => TimeSpan.Parse(d.Identificacion.HorEmi));

                foreach (var dte in purchaseDtes)
                {
                    var record = new object[]
                    {
                        // A: FECHA DE EMISIÓN
                        DateTime.Parse(dte.Identificacion.FecEmi).ToString("dd/MM/yyyy"),
                        // B: CLASE DE DOCUMENTO (4 = DTE)
                        "4",
                        // C: TIPO DE DOCUMENTO
                        dte.Identificacion.TipoDte,
                        // D: NÚMERO DE DOCUMENTO (Código de Generación para DTE)
                        dte.Identificacion.CodigoGeneracion,
                        // E: NIT O NRC DEL PROVEEDOR
                        dte.Emisor?.Nit ?? dte.Emisor?.Nrc ?? "",
                        // F: NOMBRE DEL PROVEEDOR
                        dte.Emisor?.Nombre ?? "",
                        // G: COMPRAS INTERNAS EXENTAS Y/O NO SUJETAS
                        (dte.Resumen.TotalExenta + dte.Resumen.TotalNoSuj).ToString("F2", CultureInfo.InvariantCulture),
                        // H, I: INTERNACIONES/IMPORTACIONES (No aplica para DTE estándar)
                        "0.00", "0.00",
                        // J: COMPRAS INTERNAS GRAVADAS
                        dte.Resumen.TotalGravada.ToString("F2", CultureInfo.InvariantCulture),
                        // K, L, M: INTERNACIONES/IMPORTACIONES (No aplica para DTE estándar)
                        "0.00", "0.00", "0.00",
                        // N: CRÉDITO FISCAL
                        (dte.Resumen.Tributos?.FirstOrDefault(t => t.Codigo == "20")?.Valor ?? 0).ToString("F2", CultureInfo.InvariantCulture),
                        // O: TOTAL DE COMPRAS
                        dte.Resumen.TotalPagar.ToString("F2", CultureInfo.InvariantCulture),
                        // P: DUI DEL PROVEEDOR (No disponible en el modelo DTE.Emisor)
                        "",
                        // Q, R, S, T: Nuevos campos de Renta. Se colocan "0" por defecto según manual.
                        "0", "0", "0", "0",
                        // U: NÚMERO DE ANEXO
                        "3"
                    };

                    foreach (var field in record) csv.WriteField(field);
                    csv.NextRecord();
                }
            }
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Genera el Anexo de Documentos Anulados y/o Extraviados.
        /// </summary>
        public async Task<byte[]> GenerateAnexoDocumentosAnuladosCsv(IEnumerable<IDte> dtes)
        {
            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, System.Text.Encoding.UTF8))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    HasHeaderRecord = false
                };
                using var csv = new CsvWriter(writer, config);

                var dtesWithAnnulments = dtes.Where(d => d.ItemsAnulados != null && d.ItemsAnulados.Any());

                foreach (var dte in dtesWithAnnulments)
                {
                    foreach (var itemAnulado in dte.ItemsAnulados)
                    {
                        var record = new object[]
                       {
                            // A: NÚMERO DE RESOLUCIÓN (Para DTE es el Número de Control)
                            dte.Identificacion.NumeroControl,
                            // B: CLASE DE DOCUMENTO (4 = DTE)
                            "4",
                            // C: DESDE (PREIMPRESO) (0 para DTE)
                            "0",
                            // D: HASTA (PREIMPRESO) (0 para DTE)
                            "0",
                            // E: TIPO DE DOCUMENTO
                            itemAnulado.TipoDoc,
                            // F: TIPO DE DETALLE ('A' para Anulado/Invalidado)
                            "A",
                            // G: SERIE (Para DTE es Sello de Recepción, no disponible en modelo)
                            "", // Se deja vacío por falta de datos en el modelo JSON
                            // H: DESDE (0 para DTE)
                            "0",
                            // I: HASTA (0 para DTE)
                            "0",
                            // J: CÓDIGO DE GENERACIÓN (NumDocumento del item anulado)
                            itemAnulado.NumDocumento
                       };

                        foreach (var field in record) csv.WriteField(field);
                        csv.NextRecord();
                    }
                }
            }
            return memoryStream.ToArray();
        }

        // --- INICIO DEL CÓDIGO A AÑADIR ---

        /// <summary>
        /// Genera el Anexo de Compras a Sujetos Excluidos (Anexo 5), Casilla 66.
        /// </summary>
        public async Task<byte[]> GenerateAnexoComprasSujetoExcluidoCsv(IEnumerable<IDte> dtes)
        {
            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, System.Text.Encoding.UTF8))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    HasHeaderRecord = false
                };
                using var csv = new CsvWriter(writer, config);

                var excludedDtes = dtes
                    .Where(d => d.Identificacion?.TipoDte == "14") // DTE Tipo 14 = Factura de Sujeto Excluido
                    .OrderBy(d => DateTime.Parse(d.Identificacion.FecEmi))
                    .ThenBy(d => TimeSpan.Parse(d.Identificacion.HorEmi));

                foreach (var dte in excludedDtes)
                {
                    // El emisor del DTE es el sujeto excluido al que le compramos.
                    // El receptor del DTE es nuestra propia empresa (el declarante).
                    string tipoDocumentoIdentidad = "1"; // 1=NIT, 2=DUI, 3=Otro
                    if (!string.IsNullOrEmpty(dte.Emisor?.NumDocumento) && string.IsNullOrEmpty(dte.Emisor?.Nit)) tipoDocumentoIdentidad = "2";

                    var record = new object[]
                    {
                        // A: TIPO DE DOCUMENTO (1=NIT, 2=DUI, 3=Otro)
                        tipoDocumentoIdentidad,
                        // B: NÚMERO DE NIT, DUI, U OTRO DOCUMENTO
                        dte.Emisor?.Nit ?? dte.Emisor?.NumDocumento ?? "",
                        // C: NOMBRE, RAZÓN SOCIAL O DENOMINACIÓN
                        dte.Emisor?.Nombre ?? "",
                        // D: FECHA DE EMISIÓN DEL DOCUMENTO
                        DateTime.Parse(dte.Identificacion.FecEmi).ToString("dd/MM/yyyy"),
                        // E: NÚMERO DE SERIE DEL DOCUMENTO (Sello de recepción, no disponible)
                        "",
                        // F: NÚMERO DE DOCUMENTO (Código de generación)
                        dte.Identificacion.CodigoGeneracion,
                        // G: MONTO DE LA OPERACIÓN
                        dte.Resumen.TotalPagar.ToString("F2", CultureInfo.InvariantCulture),
                        // H: MONTO DE LA RETENCIÓN IVA 13%
                        (dte.Resumen.Tributos?.FirstOrDefault(t => t.Descripcion.Contains("RETENCION"))?.Valor ?? 0).ToString("F2", CultureInfo.InvariantCulture),
                        // I, J, K, L: Nuevos campos de Renta. Se colocan "0" por defecto según manual.
                        "0", "0", "0", "0",
                        // M: NÚMERO DE ANEXO
                        "5"
                    };

                    foreach (var field in record) csv.WriteField(field);
                    csv.NextRecord();
                }
            }
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Genera el Anexo de Retención IVA 1% Efectuada al Declarante (Anexo 7), Casilla 162.
        /// </summary>
        public async Task<byte[]> GenerateAnexoRetencionIvaRecibidaCsv(IEnumerable<IDte> dtes)
        {
            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, System.Text.Encoding.UTF8))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    HasHeaderRecord = false
                };
                using var csv = new CsvWriter(writer, config);

                var retentionDtes = dtes
                    .Where(d => d.Identificacion?.TipoDte == "07") // DTE Tipo 07 = Comprobante de Retención
                    .OrderBy(d => DateTime.Parse(d.Identificacion.FecEmi))
                    .ThenBy(d => TimeSpan.Parse(d.Identificacion.HorEmi));

                foreach (var dte in retentionDtes)
                {
                    // El emisor del DTE es el agente que nos retuvo el IVA.
                    var record = new object[]
                    {
                        // A: NIT DEL AGENTE
                        dte.Emisor?.Nit ?? "",
                        // B: FECHA DE EMISIÓN
                        DateTime.Parse(dte.Identificacion.FecEmi).ToString("dd/MM/yyyy"),
                        // C: TIPO DE DOCUMENTO
                        dte.Identificacion.TipoDte,
                        // D: SERIE (Sello de recepción, no disponible)
                        "",
                        // E: NÚMERO DE DOCUMENTO (Código de generación)
                        dte.Identificacion.CodigoGeneracion,
                        // F: MONTO SUJETO (El cuerpo del documento debe tener el monto sujeto a retención)
                        dte.CuerpoDocumento.FirstOrDefault()?.VentaGravada.ToString("F2", CultureInfo.InvariantCulture) ?? "0.00",
                        // G: MONTO RETENCIÓN 1%
                        dte.Resumen.IvaRete1.ToString("F2", CultureInfo.InvariantCulture),
                        // H: DUI DEL AGENTE (No disponible en el modelo DTE.Emisor)
                        "",
                        // I: NUMERO DE ANEXO
                        "7"
                    };

                    foreach (var field in record) csv.WriteField(field);
                    csv.NextRecord();
                }
            }
            return memoryStream.ToArray();
        }
        // --- FIN DEL CÓDIGO A AÑADIR ---
    }
}