using QuestPDFContainer = QuestPDF.Infrastructure.IContainer;
using QuestPDFDocument = QuestPDF.Fluent.Document;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using VisorDTE.ViewModels;

namespace VisorDTE.Services
{
    public class PdfExportService
    {
        public void ExportAsPdf(string filePath, IEnumerable<DteViewModel> dteViewModels)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            QuestPDFDocument.Create(container =>
            {
                foreach (var vm in dteViewModels)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter);
                        page.Margin(1, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(9));

                        page.Header().Element(header => BuildHeader(header, vm));
                        page.Content().Element(content => BuildContent(content, vm));
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                        });
                    });
                }
            }).GeneratePdf(filePath);
        }

        private void BuildHeader(QuestPDFContainer container, DteViewModel vm)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(vm.TipoDteDescripcion).SemiBold().FontSize(18);
                    col.Item().Text($"Cód. Generación: {vm.Dte.Identificacion.CodigoGeneracion}").FontSize(8).FontColor(Colors.Grey.Medium);
                    col.Item().Text($"No. Control: {vm.Dte.Identificacion.NumeroControl}").FontSize(8).FontColor(Colors.Grey.Medium);
                });
                row.ConstantItem(120).Column(col =>
                {
                    col.Item().AlignRight().Text($"NIT: {vm.Dte.Emisor.Nit}");
                    col.Item().AlignRight().Text($"NRC: {vm.Dte.Emisor.Nrc}");
                });
            });
        }

        private void BuildContent(QuestPDFContainer container, DteViewModel vm)
        {
            container.PaddingVertical(15).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(c =>
                    {
                        c.Item().Text("EMISOR").Bold();
                        c.Item().Text(vm.Dte.Emisor.Nombre);
                        c.Item().Text($"NIT: {vm.Dte.Emisor.Nit} / NRC: {vm.Dte.Emisor.Nrc}");
                        c.Item().Text(vm.EmisorDireccionCompleta).FontSize(8);
                        c.Item().PaddingTop(5).Text(vm.Dte.Emisor.Correo).FontSize(8);
                    });
                    row.ConstantItem(15);
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(c =>
                    {
                        c.Item().Text("RECEPTOR").Bold();
                        c.Item().Text(vm.Dte.Receptor.Nombre);
                        c.Item().Text($"Doc: {vm.Dte.Receptor.NumDocumento}");
                        c.Item().Text(vm.ReceptorDireccionCompleta).FontSize(8);
                        c.Item().PaddingTop(5).Text(vm.Dte.Receptor.Correo).FontSize(8);
                    });
                });

                col.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                    });
                    table.Header(header =>
                    {
                        header.Cell().Text("Cant.");
                        header.Cell().Text("Descripción");
                        header.Cell().AlignRight().Text("P. Unitario");
                        header.Cell().AlignRight().Text("Descuento");
                        header.Cell().AlignRight().Text("Gravado");
                    });

                    if (vm.Dte.CuerpoDocumento != null)
                    {
                        foreach (var item in vm.Dte.CuerpoDocumento)
                        {
                            table.Cell().Text(item.Cantidad.ToString("N2"));
                            table.Cell().Text(item.Descripcion);
                            table.Cell().AlignRight().Text(item.PrecioUni.ToString("C"));
                            table.Cell().AlignRight().Text(item.MontoDescu.ToString("C"));
                            table.Cell().AlignRight().Text(item.VentaGravada.ToString("C"));
                        }
                    }
                });

                col.Item().AlignRight().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem();
                    row.ConstantItem(250).Column(col_totals =>
                    {
                        col_totals.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal:");
                            r.ConstantItem(80).AlignRight().Text(vm.Dte.Resumen.SubTotal.ToString("C"));
                        });
                        col_totals.Item().Row(r =>
                        {
                            r.RelativeItem().Text("IVA (13%):");
                            r.ConstantItem(80).AlignRight().Text((vm.Dte.Resumen.TotalIva ?? vm.Dte.Resumen.IvaRete1 ?? 0).ToString("C"));
                        });
                        col_totals.Item().PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text("Total a Pagar:").Bold();
                            r.ConstantItem(80).AlignRight().Text(vm.Dte.Resumen.TotalPagar.ToString("C")).Bold();
                        });
                    });
                });

                col.Item().PaddingTop(20).Text(vm.Dte.Resumen.TotalLetras).Italic();
            });
        }
    }
}