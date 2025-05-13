using ControlDeGastosMVC.API.Models;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

namespace ControlDeGastosMVC.API.Services
{
    public interface IGeneratePdfService
    {
         byte[] GeneratePdf(List<Gasto> gastos, int mes, int anio);
    }

}
