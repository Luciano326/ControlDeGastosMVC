using ControlDeGastosMVC.API.Context;
using ControlDeGastosMVC.API.Models;
using ControlDeGastosMVC.API.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace ControlDeGastosMVC.API.Controllers
{
    public class RegisterController : Controller
    {
        public IActionResult Register()
        {
            return View();
        }

        private readonly GastosDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        

        public RegisterController(GastosDbContext context, IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterUser model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Email == model.Email);
            if (usuarioExiste)
            {
                ModelState.AddModelError("Email", "Ya existe un usuario con este correo.");
                return View(model);
            }

            var nuevoUsuario = new Usuario
            {
                NombreCompleto = model.NombreCompleto,
                Email = model.Email,
                Rol = "Usuario",
                EsActivo = true,
                FechaCreacion = DateTime.Now
            };

            nuevoUsuario.PasswordHash = _passwordHasher.HashPassword(nuevoUsuario, model.Password);

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            TempData["ToastMensaje"] = "Registro exitoso. Por favor inicie sesión.";
            TempData["ToastTipo"] = "info";

            return RedirectToAction("Login", "Login");
        }
    }
}
