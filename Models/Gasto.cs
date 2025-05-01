using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ControlDeGastosMVC.API.Models
{
    public class Gasto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public decimal Monto { get; set; }

        public string? Categoria { get; set; }

        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        [ValidateNever]
        public Usuario Usuario { get; set; } 

    }
}
