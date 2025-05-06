using ControlDeGastosMVC.API.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.ComponentModel;
using IContainer = QuestPDF.Infrastructure.IContainer;

namespace ControlDeGastosMVC.API.Services
{
    public class GeneratePdfService : IGeneratePdfService
    {
        public byte[] GeneratePdf(List<Gasto> gastos)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Text("Reporte de Gastos")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(25); // #
                            columns.RelativeColumn(3);  // Descripción
                            columns.RelativeColumn(3);   // Categoría
                            columns.RelativeColumn(2);   // Fecha
                            columns.RelativeColumn(2);   // Monto
                        });

                        // Encabezado
                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("#");
                            header.Cell().Element(HeaderStyle).Text("Descripción");
                            header.Cell().Element(HeaderStyle).Text("Categoría");
                            header.Cell().Element(HeaderStyle).Text("Fecha");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Monto");

                            static IContainer HeaderStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold())
                                                .PaddingVertical(5)
                                                .BorderBottom(1)
                                                .BorderColor(Colors.Black);
                            }
                        });

                        int index = 1;
                        foreach (var gasto in gastos)
                        {
                            table.Cell().Element(RowStyle).Text(index++);
                            table.Cell().Element(RowStyle).Text(gasto.Descripcion);
                            table.Cell().Element(RowStyle).Text(gasto.Categoria ?? "Sin categoría");
                            table.Cell().Element(RowStyle).Text(gasto.Fecha.ToShortDateString());
                            table.Cell().Element(RowStyle).AlignRight().Text($"${gasto.Monto:N2}");

                            static IContainer RowStyle(IContainer container)
                            {
                                return container.BorderBottom(1)
                                                .BorderColor(Colors.Grey.Lighten2)
                                                .PaddingVertical(5);
                            }
                        }
                        // Después del foreach de las filas de la tabla
                        table.Cell().ColumnSpan(4).AlignRight().Text("Total:").Bold();
                        table.Cell().AlignRight().Text($"${gastos.Sum(g => g.Monto):N2}").Bold();


                    });
                                
                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("Reporte generado automáticamente - ").FontSize(9);
                        txt.Span(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(9).SemiBold();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
