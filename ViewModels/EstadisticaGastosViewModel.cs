namespace ControlDeGastosMVC.API.ViewModels
{
    public class EstadisticaGastosViewModel
    {
        public decimal TotalGastado { get; set; }
        public string CategoriaTop { get; set; } = "";
        public int CantidadGastos { get; set; }
        public List<string> Categorias { get; set; } = [];
        public List<decimal> Montos { get; set; } = [];
    }
}
