using System.ComponentModel.DataAnnotations;

namespace ControlDeGastosMVC.API.ViewModels
{
    public class RegisterUser
    {
        [Required]
        public string NombreCompleto { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }
    }
}
