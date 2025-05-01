using System.ComponentModel.DataAnnotations;

namespace ControlDeGastosMVC.API.ViewModels
{
    public class UsuarioPerfil
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreCompleto { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string? PasswordActual { get; set; }

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres.")]
        public string? NuevaPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public string? ConfirmarPassword { get; set; }
    }

}
