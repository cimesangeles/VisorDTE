// /Services/F07AnexoCsvService.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using VisorDTE.Models;
using VisorDTE.ViewModels;

namespace VisorDTE.Services;

public enum AnexoF07Type
{
    VentasConsumidorFinal, // Anexo 1
    VentasContribuyentes   // Anexo 2
}

public class F07AnexoCsvService
{
    public string GenerateAnexoCsv(IEnumerable<DteViewModel> dteViewModels, AnexoF07Type anexoType)
    {
        var csvBuilder = new StringBuilder();
        var culture = new CultureInfo("en-US"); // Usar punto como separador decimal

        if (anexoType == AnexoF07Type.VentasConsumidorFinal)
        {
            // Lógica especial para Anexo de Consumidor Final (Resumen Diario)
            // Agrupamos todos los DTE por su fecha de emisión.
            var ventasPorDia = dteViewModels.GroupBy(vm => vm.Dte.Identificacion.FecEmi);
            int correlativo = 1;

            foreach (var dia in ventasPorDia)
            {
                var dtesDelDia = dia.ToList();
                if (dtesDelDia.Count == 0) continue;

                // Columnas requeridas según Manual de Usuario, páginas 11-14
                var noCorrelativo = correlativo++;
                var fecha = DateTime.Parse(dia.Key).ToString("dd/MM/yyyy");
                var primerDocumento = dtesDelDia.First().Dte.Identificacion.NumeroControl;
                var ultimoDocumento = dtesDelDia.Last().Dte.Identificacion.NumeroControl;
                var numeroCaja = "0"; // Valor por defecto
                var totalVentasExentas = dtesDelDia.Sum(vm => vm.Dte.Resumen.TotalExenta).ToString("F2", culture);
                var totalVentasNoSujetas = dtesDelDia.Sum(vm => vm.Dte.Resumen.TotalNoSuj).ToString("F2", culture);
                var totalVentasGravadas = dtesDelDia.Sum(vm => vm.Dte.Resumen.TotalGravada).ToString("F2", culture);
                var totalDebitoFiscal = dtesDelDia.Sum(vm => vm.TotalIva).ToString("F2", culture);
                var ventasCtaTerceros = "0.00"; // Valor por defecto
                var totalVentas = dtesDelDia.Sum(vm => vm.Dte.Resumen.TotalPagar).ToString("F2", culture);

                csvBuilder.AppendLine($"{noCorrelativo},{fecha},{primerDocumento},{ultimoDocumento},{numeroCaja},{totalVentasExentas},{totalVentasNoSujetas},{totalVentasGravadas},{totalDebitoFiscal},{ventasCtaTerceros},{totalVentas}");
            }
        }
        else // La lógica para Anexo de Ventas a Contribuyentes permanece igual (documento por documento)
        {
            foreach (var vm in dteViewModels)
            {
                csvBuilder.AppendLine(FormatAnexoVentasContribuyente(vm.Dte, vm.TotalIva, culture));
            }
        }

        return csvBuilder.ToString();
    }

    /// <summary>
    /// Formatea una línea para el Anexo 2: Detalle de Ventas a Contribuyentes.
    /// Esta lógica es correcta y no se modifica.
    /// </summary>
    private string FormatAnexoVentasContribuyente(IDte dte, double totalIva, CultureInfo culture)
    {
        var fechaEmi = DateTime.Parse(dte.Identificacion.FecEmi).ToString("dd/MM/yyyy");
        var numeroDocumento = dte.Identificacion.NumeroControl;
        var nitDui = dte.Receptor?.Nit ?? dte.Receptor?.NumDocumento ?? "N/A";
        var nombreCliente = (dte.Receptor?.Nombre ?? "N/A").Replace(',', ' ');
        var ventasExentas = dte.Resumen.TotalExenta.ToString("F2", culture);
        var ventasGravadas = dte.Resumen.TotalGravada.ToString("F2", culture);
        var ventasNoSujetas = dte.Resumen.TotalNoSuj.ToString("F2", culture);
        var ventasCtaTerceros = "0.00";
        var debitoFiscal = totalIva.ToString("F2", culture);
        var ivaRetenido = dte.Resumen.IvaRete1.ToString("F2", culture);
        var ivaPercibido = dte.Resumen.Tributos?.FirstOrDefault(t => t.Codigo == "55")?.Valor.ToString("F2", culture) ?? "0.00";
        var otrosImpuestos = dte.Resumen.Tributos?
            .Where(t => t.Codigo != "20" && t.Codigo != "55")
            .Sum(t => t.Valor)
            .ToString("F2", culture) ?? "0.00";
        var retencionCtaTerceros = "0.00";
        var montoTotal = dte.Resumen.TotalPagar.ToString("F2", culture);
        var claseDocumento = dte.Identificacion.TipoDte == "03" ? "CF" : dte.Identificacion.TipoDte;

        return $"{fechaEmi},{numeroDocumento},{nitDui},\"{nombreCliente}\",{ventasExentas},{ventasGravadas},{ventasNoSujetas},{ventasCtaTerceros},{debitoFiscal},{ivaRetenido},{ivaPercibido},{otrosImpuestos},{retencionCtaTerceros},{montoTotal},{claseDocumento}";
    }
}