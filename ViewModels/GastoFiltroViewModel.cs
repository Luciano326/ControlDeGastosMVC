using ControlDeGastosMVC.API.Models;

namespace ControlDeGastosMVC.API.ViewModels
{
    public class GastoFiltroViewModel
    {
        public List<Gasto> Gastos { get; set; }

        public int? Mes { get; set; }
        public int? Anio { get; set; }
        public string? SearchString { get; set; }

        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
