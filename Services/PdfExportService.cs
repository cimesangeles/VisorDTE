using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using VisorDTE.ViewModels;

namespace VisorDTE.Services;

public class PdfExportService
{
    public void ExportAsPdf(string filePath, List<DteViewModel> dteViewModels)
    {
        Document.Create(container =>
        {
            foreach (var vm in dteViewModels)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(36);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Helvetica));

                    page.Header().Element(headerContainer => BuildHeader(headerContainer, vm));
                    page.Content().Element(contentContainer => BuildContent(contentContainer, vm));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            }
        }).GeneratePdf(filePath);
    }

    private void BuildHeader(IContainer container, DteViewModel vm)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(vm.Dte.Emisor.Nombre).Bold().FontSize(14);
                col.Item().Text(vm.Dte.Emisor.NombreComercial);
                col.Item().Text($"NIT: {vm.Dte.Emisor.Nit} / NRC: {vm.Dte.Emisor.Nrc}");
                col.Item().Text(vm.EmisorDireccionCompleta);
            });

            row.ConstantItem(150).Column(col =>
            {
                col.Item().Border(1).AlignCenter().Text(vm.TipoDteDescripcion).Bold();
                col.Item().Text($"No. Control: {vm.Dte.Identificacion.NumeroControl}");
                col.Item().Text($"Cód. Generación: {vm.Dte.Identificacion.CodigoGeneracion}");
                col.Item().Text($"Fecha: {vm.Dte.Identificacion.FecEmi} {vm.Dte.Identificacion.HorEmi}");
            });
        });
    }

    private void BuildContent(IContainer container, DteViewModel vm)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(20).Element(innerContainer => BuildReceptorInfo(innerContainer, vm));
            col.Item().PaddingTop(20).Element(BuildItemsTable);
            col.Item().Element(innerContainer => BuildTotals(innerContainer, vm));
        });

        void BuildReceptorInfo(IContainer innerContainer, DteViewModel dteVm)
        {
            innerContainer.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Cliente:").SemiBold();
                    col.Item().Text(dteVm.Dte.Receptor.Nombre);
                    col.Item().Text($"Documento: {dteVm.Dte.Receptor.NumDocumento ?? "N/A"}");
                    col.Item().Text($"Dirección: {dteVm.ReceptorDireccionCompleta}");
                });
            });
        }

        void BuildItemsTable(IContainer tableContainer)
        {
            tableContainer.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Text("Descripción").Bold();
                    header.Cell().AlignRight().Text("P. Unitario").Bold();
                    header.Cell().AlignRight().Text("Cantidad").Bold();
                    header.Cell().AlignRight().Text("Subtotal").Bold();
                });

                foreach (var item in vm.Dte.CuerpoDocumento)
                {
                    table.Cell().Text(item.Descripcion);
                    table.Cell().AlignRight().Text(item.PrecioUni.ToString("N2"));
                    table.Cell().AlignRight().Text(item.Cantidad.ToString("N2"));
                    table.Cell().AlignRight().Text(item.VentaGravada.ToString("N2"));
                }
            });
        }
    }

    private void BuildTotals(IContainer container, DteViewModel vm)
    {
        container.AlignRight().PaddingTop(20).Width(200).Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Text("Subtotal:");
                row.ConstantItem(80).AlignRight().Text(vm.Dte.Resumen.SubTotal.ToString("C2"));
            });
            col.Item().Row(row =>
            {
                row.RelativeItem().Text("IVA (13%):");
                var totalIva = vm.Dte.Resumen.Tributos?.FirstOrDefault(t => t.Codigo == "20")?.Valor ?? 0;
                row.ConstantItem(80).AlignRight().Text(totalIva.ToString("C2"));
            });
            col.Item().Row(row =>
            {
                row.RelativeItem().Text("Total a Pagar:").Bold();
                row.ConstantItem(80).AlignRight().Text(vm.Dte.Resumen.TotalPagar.ToString("C2")).Bold();
            });
        });
    }
}