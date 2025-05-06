using ControlDeGastosMVC.API.Models;
using QuestPDF.Fluent;

namespace ControlDeGastosMVC.API.Services
{
    public interface IGeneratePdfService
    {
        byte[] GeneratePdf(List<Gasto> gastos);
    }

}
