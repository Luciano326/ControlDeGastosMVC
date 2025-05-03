using ControlDeGastosMVC.API.Context;
using ControlDeGastosMVC.API.Models;
using ControlDeGastosMVC.API.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace ControlDeGastosMVC.API.Controllers
{
    public class UsersController : Controller
    {
        private readonly GastosDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public UsersController(GastosDbContext context, IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Users()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return View(usuarios);
        }
        #region Estado
        [Authorize(Roles = "Administrador")]

        public async Task<IActionResult> Desactivar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            if (usuario.Id == 21)
            {
                TempData["ToastMensaje"] = "No puedes utilizar esta función en " + usuario.Email;
                TempData["ToastTipo"] = "warning";
                return RedirectToAction("Users");
            }

            usuario.EsActivo = false;
            await _context.SaveChangesAsync();

            TempData["ToastMensaje"] = "El usuario fue desactivado correctamente.";
            TempData["ToastTipo"] = "secondary";

            return RedirectToAction("Users");
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Activar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            if (usuario.Id == 21)
            {
                TempData["ToastMensaje"] = "No puedes utilizar esta función en " + usuario.Email;
                TempData["ToastTipo"] = "warning";
                return RedirectToAction("Users");
            }
            usuario.EsActivo = true;
            await _context.SaveChangesAsync();

            TempData["ToastMensaje"] = "El usuario fue activado correctamente.";
            TempData["ToastTipo"] = "primary";
            
            return RedirectToAction("Users");
        }
        #endregion

        #region Rol
        [Authorize(Roles = "Administrador")]

        public async Task<IActionResult> Descender(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            if (usuario.Id == 21)
            {
                TempData["ToastMensaje"] = "No puedes utilizar esta función en " + usuario.Email;
                TempData["ToastTipo"] = "warning";
                return RedirectToAction("Users");
            }

            usuario.Rol = "Usuario";
            await _context.SaveChangesAsync();

            TempData["ToastMensaje"] = usuario.NombreCompleto + " fue descendido correctamente.";
            TempData["ToastTipo"] = "secondary";

            return RedirectToAction("Users");
        }

        [Authorize(Roles = "Administrador")]

        public async Task<IActionResult> Ascender(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();


            if (usuario.Id == 21)
            {
                TempData["ToastMensaje"] = "No puedes utilizar esta función en " + usuario.Email;
                TempData["ToastTipo"] = "warning";
                return RedirectToAction("Users");
            }

            usuario.Rol = "Administrador";
            await _context.SaveChangesAsync();

            TempData["ToastMensaje"] = usuario.NombreCompleto + " fue ascendido correctamente.";
            TempData["ToastTipo"] = "primary";

            return RedirectToAction("Users");
        }
        #endregion

        #region Editar Perfil de Usuario
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditPerfil()
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            int userId = int.Parse(userIdClaim);
            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario == null) return NotFound();

            // Mapea el modelo Usuario a UsuarioPerfil
            var usuarioPerfil = new UsuarioPerfil
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email
            };

            return View(usuarioPerfil);
        }

        [HttpPost]
        [Authorize]
        [ActionName("EditPerfil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPerfil(UsuarioPerfil model)
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var usuarioOriginal = await _context.Usuarios.FindAsync(userId);
            if (usuarioOriginal == null) return NotFound();

            if (ModelState.IsValid)
            {
                usuarioOriginal.NombreCompleto = model.NombreCompleto;
                usuarioOriginal.Email = model.Email;

                // Cambio de contraseña (si los campos están completos)
                if (!string.IsNullOrWhiteSpace(model.PasswordActual) &&
                    !string.IsNullOrWhiteSpace(model.NuevaPassword) &&
                    !string.IsNullOrWhiteSpace(model.ConfirmarPassword))
                {
                    if (_passwordHasher.VerifyHashedPassword(usuarioOriginal, usuarioOriginal.PasswordHash, model.PasswordActual)
                        != PasswordVerificationResult.Success)
                    {
                        ModelState.AddModelError("PasswordActual", "La contraseña actual es incorrecta.");
                        return View(model);
                    }

                    if (model.NuevaPassword != model.ConfirmarPassword)
                    {
                        ModelState.AddModelError("ConfirmarPassword", "Las contraseñas nuevas no coinciden.");
                        return View(model);
                    }

                    usuarioOriginal.PasswordHash = _passwordHasher.HashPassword(usuarioOriginal, model.NuevaPassword);
                }

                await _context.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuarioOriginal.Id.ToString()),
                    new Claim("UserId", usuarioOriginal.Id.ToString()),
                    new Claim(ClaimTypes.Name, model.NombreCompleto),
                    new Claim(ClaimTypes.Email, model.Email),
                    new Claim(ClaimTypes.Role, usuarioOriginal.Rol)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                TempData["ToastMensaje"] = "El perfil fue editado correctamente.";
                TempData["ToastTipo"] = "primary";
                return RedirectToAction("EditPerfil");
            }

            return View(model);
        }


        #endregion

    }
}
