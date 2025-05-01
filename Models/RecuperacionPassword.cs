using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ControlDeGastosMVC.API.Models
{
    public class RecuperacionPassword
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }

        public string Token { get; set; }

        public DateTime ExpiraEn { get; set; }

        public bool Usado { get; set; } = false;

        // Navegación
        public Usuario Usuario { get; set; }
    }
}
