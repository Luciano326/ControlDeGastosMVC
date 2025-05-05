using QuestPDF.Fluent;

namespace ControlDeGastosMVC.API.Services
{
    /*public class GeneratePdfService : IGeneratePdfService
    {
        public Document GeneratePdfQuest()
        {
            var report =  Document.Create(container =>
            {
                container.Page(page =>
                    {
                        page.Margin(50);
                        page.Header().Text("Control de Gastos").FontSize(20).Bold();
                        page.Content().Element(ComposeContent);
                        page.Footer().AlignCenter().Text("Control de Gastos - Todos los derechos reservados");
                    });
            });
        }
    }*/
}
