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
    }
}