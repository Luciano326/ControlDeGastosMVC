using ControlDeGastosMVC.API.Context;
using ControlDeGastosMVC.API.Models;
using ControlDeGastosMVC.API.Services;
using ControlDeGastosMVC.API.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace ControlDeGastosMVC.API.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index","Home");
            }
            return View();
        }

        private readonly GastosDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly IEmailService _emailService;

        public LoginController(GastosDbContext context, IPasswordHasher<Usuario> passwordHasher, IEmailService emailService)
        {
            _emailService = emailService;
            _context = context;
            _passwordHasher = passwordHasher;
        }
        #region Login - Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginUser model)
        {
            if(!ModelState.IsValid)
                return View(model);

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (usuario == null || !usuario.EsActivo ||
                _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, model.Password) != PasswordVerificationResult.Success)
            {
                if (usuario != null && !usuario.EsActivo)
                {
                    ModelState.AddModelError(string.Empty, "Cuenta suspendida o inactiva.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                }

                return View(model);
            }


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()), // 👈 Necesario
                new Claim("UserId", usuario.Id.ToString()), 
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol) // si estás usando roles
            };


            var claimIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimIdentity),
                authProperties
            );
            TempData["ToastMensaje"] = "Ha iniciado sesión correctamente.";
            TempData["ToastTipo"] = "success"; 

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            TempData["ToastMensaje"] = "Ha cerrado sesión correctamente.";
            TempData["ToastTipo"] = "secondary";

            return RedirectToAction("Login", "Login");
        }
        #endregion

        #region Recuperar Clave
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RecuperarClave()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarClave(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ToastMensaje"] = "Debes ingresar un correo electrónico.";
                TempData["ToastTipo"] = "warning";
                return RedirectToAction("Login");
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null || !usuario.EsActivo)
            {
                TempData["ToastMensaje"] = "No se encontró un usuario con ese correo.";
                TempData["ToastTipo"] = "warning";
                return RedirectToAction("VerificarToken");
            }

            // Generar token aleatorio
            var token = Guid.NewGuid().ToString("N").Substring(0, 6); // Ejemplo: 6 caracteres
            var fechaExpiracion = DateTime.UtcNow.AddMinutes(1);  // Guardar siempre en UTC
            var recuperacion = new RecuperacionPassword
            {
                UsuarioId = usuario.Id,
                Token = token,
                ExpiraEn = fechaExpiracion, // o más si querés
                Usado = false
            };
            
            _context.RecuperacionPassword.Add(recuperacion);
            await _context.SaveChangesAsync();

            var correo = new EmailDTO()
            {
                Para = email,
                Asunto = $"Recuperación de contraseña {usuario.NombreCompleto}",
                Contenido = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <style>\r\n        .email-container {{\r\n            " +
                $"font-family: Arial, sans-serif;\r\n            background-color: #f9f9f9;\r\n            padding: 20px;\r\n            border-radius: 8px;\r\n            " +
                $"color: #333;\r\n            max-width: 500px;\r\n            margin: auto;\r\n            border: 1px solid #ddd;\r\n        }}\r\n        .token {{\r\n            " +
                $"font-size: 20px;\r\n            font-weight: bold;\r\n            color: #0056b3;\r\n            background-color: #eef5ff;\r\n            padding: 10px;\r\n            " +
                $"border-radius: 5px;\r\n            display: inline-block;\r\n            margin-top: 10px;\r\n        }}\r\n    </style>\r\n</head>\r\n<body>\r\n    " +
                $"<div class=\"email-container\">\r\n        <h2>Recuperación de contraseña</h2>\r\n        <p>Se ha generado un token de recuperación para tu cuenta.</p>\r\n       " +
                $" <p>Utilizá el siguiente token para completar el proceso:</p>\r\n        <div class=\"token\">{token}</div>\r\n        " +
                $"<p style=\"margin-top: 20px;\">Este token es válido solo por un tiempo limitado. Si no solicitaste este código, podés ignorar este correo.</p>\r\n    " +
                $"</div>\r\n</body>\r\n</html>\r\n",
            };

            _emailService.SendEmail(correo);

            TempData["ToastMensaje"] = $"Token enviado correctamente. Verificar correo {email}";
            TempData["ToastTipo"] = "info";
            return RedirectToAction("VerificarToken", new { email = email });
        }

        #endregion

        #region Verificar Token
        // Acción GET: Verifica que el correo se pase correctamente
        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerificarToken(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ToastMensaje"] = "Correo electrónico no válido.";
                TempData["ToastTipo"] = "warning";
                return RedirectToAction("RecuperarClave", "Login");  // Redirigir si el correo es nulo
            }

            ViewBag.Email = email;
            return View();
        }

        // Acción POST: Verifica el token enviado por el usuario
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerificarToken(string email, string token)
        {
            // Validar que ambos campos no estén vacíos
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["ToastMensaje"] = "Datos incompletos. Asegúrate de ingresar el correo y el token.";
                TempData["ToastTipo"] = "danger";
                return RedirectToAction("RecuperarClave", "Login"); // Redirigir si falta información
            }

            // Buscar usuario por correo
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null)
            {
                TempData["ToastMensaje"] = "No se encontró un usuario con ese correo.";
                TempData["ToastTipo"] = "danger";
                return RedirectToAction("RecuperarClave", "Login");  // Redirigir si no se encuentra el usuario
            }

            // Buscar el token de recuperación para el usuario
            var recuperacion = await _context.RecuperacionPassword
                .Where(r => r.UsuarioId == usuario.Id && r.Token == token && !r.Usado)  // Verificar que el token no haya sido usado
                .OrderByDescending(r => r.ExpiraEn)  // Ordenar por fecha de expiración (para tomar el más reciente)
                .FirstOrDefaultAsync();

            // Si no se encuentra el token o el token ha expirado
            if (recuperacion == null || recuperacion.ExpiraEn < DateTime.UtcNow)
            {
                TempData["ToastMensaje"] = "El código es inválido o ha expirado.";
                TempData["ToastTipo"] = "warning";
                return RedirectToAction("RecuperarClave", "Login");  // Redirigir si el token es inválido o expirado
            }

            // Redirigir a la vista de cambio de contraseña
            return RedirectToAction("CambiarClave", "Login", new { email = email, token = token });
        }
        #endregion

        #region Cambiar Contraseña

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CambiarClave(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["ToastMensaje"] = "El enlace no es válido.";
                TempData["ToastTipo"] = "danger";
                return RedirectToAction("Login");
            }

            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarClave(string email, string token, string nuevaClave)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(nuevaClave))
            {
                TempData["ToastMensaje"] = "Datos incompletos.";
                TempData["ToastTipo"] = "danger";
                return RedirectToAction("Login");
            }

            // Buscar al usuario por su correo electrónico
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null)
            {
                TempData["ToastMensaje"] = "Usuario no encontrado.";
                TempData["ToastTipo"] = "danger";
                return RedirectToAction("Login");
            }

            // Buscar el token de recuperación correspondiente al usuario
            var recuperacion = await _context.RecuperacionPassword
                .Where(r => r.UsuarioId == usuario.Id && r.Token == token)
                .OrderByDescending(r => r.ExpiraEn)
                .FirstOrDefaultAsync();

            // Logs para depuración
            Console.WriteLine($"Recuperación encontrado: {recuperacion?.Token}, Expira en: {recuperacion?.ExpiraEn}, Usado: {recuperacion?.Usado}");

            // Verificar si el token es válido
            if (recuperacion == null)
            {
                TempData["ToastMensaje"] = "El token no es válido.";
                TempData["ToastTipo"] = "danger";
                return RedirectToAction("Login");
            }

            if (recuperacion.ExpiraEn < DateTime.UtcNow)
            {
                TempData["ToastMensaje"] = "El token ha expirado.";
                TempData["ToastTipo"] = "warning";
                return RedirectToAction("Login");
            }

            if (recuperacion.Usado)
            {
                TempData["ToastMensaje"] = "El token ya fue utilizado.";
                TempData["ToastTipo"] = "warning";
                return RedirectToAction("Login");
            }

            // Cambiar la contraseña
            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, nuevaClave);
            _context.Usuarios.Update(usuario);

             recuperacion.Usado = true;
            _context.RecuperacionPassword.Update(recuperacion);

            await _context.SaveChangesAsync();

            TempData["ToastMensaje"] = "Contraseña actualizada exitosamente.";
            TempData["ToastTipo"] = "success";

            return RedirectToAction("Login");
        }
        #endregion
    }
}
