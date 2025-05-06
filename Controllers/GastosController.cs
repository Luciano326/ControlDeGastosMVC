using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControlDeGastosMVC.API.Models;
using ControlDeGastosMVC.API.Context;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ControlDeGastosMVC.API.ViewModels;
using ControlDeGastosMVC.API.Services;

namespace ControlDeGastosMVC.API.Controllers
{
    [Authorize]
    public class GastosController : Controller
    {
        private readonly GastosDbContext _context;
        private readonly IGeneratePdfService _pdfService;

        public GastosController(GastosDbContext context, IGeneratePdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }
        #region 1 Get Gasto        

        [Authorize]
        public async Task<IActionResult> Index(int? mes, int? anio, string searchString, int page = 1)
        {
            int pageSize = 10;
            int usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var gastosQuery = _context.Gastos.Where(g => g.UsuarioId == usuarioId);

            if (mes.HasValue && anio.HasValue)
                gastosQuery = gastosQuery.Where(g => g.Fecha.Month == mes && g.Fecha.Year == anio);

            if (!string.IsNullOrEmpty(searchString))
                gastosQuery = gastosQuery.Where(g =>
                    g.Descripcion.Contains(searchString) ||
                    (g.Categoria != null && g.Categoria.Contains(searchString)));

            var totalGastos = await gastosQuery.CountAsync();
            var gastosPaginados = await gastosQuery
                .OrderByDescending(g => g.Fecha)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new GastoFiltroViewModel
            {
                Gastos = gastosPaginados,
                Mes = mes,
                Anio = anio,
                SearchString = searchString,
                TotalPages = (int)Math.Ceiling(totalGastos / (double)pageSize),
                CurrentPage = page
            };

            return View(viewModel);
        }

        #endregion

        #region 2 GET: Gasto/Details/5        
        [Authorize]

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gasto = await _context.Gastos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gasto == null)
            {
                return NotFound();
            }

            return View(gasto);
        }
        #endregion

        #region 3 POST: Gasto/Create       
        
        [Authorize]

        public IActionResult Create()
        {
            return View();
        }
        // POST: Gastoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descripcion,Categoria,Fecha")] Gasto gasto, string MontoText)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int usuarioId))
            {
                return Unauthorized();
            }

            gasto.UsuarioId = usuarioId;

            try
            {
                if (!decimal.TryParse(MontoText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal montoConvertido))
                {
                    ModelState.AddModelError("Monto", "Monto inválido. Usá coma o punto para decimales.");
                }
                else
                {
                    gasto.Monto = montoConvertido;
                }

                if (ModelState.IsValid)
                {
                    _context.Add(gasto);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error inesperado: {ex.Message}");
            }

            return View(gasto);
        }


        #endregion

        #region 4 PUT: Gasto/Edit/5     

        [Authorize]

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gasto = await _context.Gastos.FindAsync(id);
            if (gasto == null)
            {
                return NotFound();
            }
            return View(gasto);
        }

        // POST: Gastoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("Gastos/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, [Bind("Id,Descripcion,Categoria,Fecha")] Gasto gasto, string MontoText)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int usuarioId = int.Parse(userIdClaim.Value);

            var gastoOriginal = await _context.Gastos.FirstOrDefaultAsync(g => g.Id == id && g.UsuarioId == usuarioId);
            if (gastoOriginal == null)
            {
                return NotFound(); // Gasto no encontrado o no pertenece al usuario
            }


            try
            {
                // Validar y convertir el monto
                if (!decimal.TryParse(MontoText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal montoConvertido))
                {
                    ModelState.AddModelError("Monto", "Monto inválido. Usá coma o punto para decimales.");
                }
                else
                {
                    // Solo actualizamos los campos editables
                    gastoOriginal.Descripcion = gasto.Descripcion;
                    gastoOriginal.Categoria = gasto.Categoria;
                    gastoOriginal.Fecha = gasto.Fecha;
                    gastoOriginal.Monto = montoConvertido;
                }

                if (ModelState.IsValid)
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error inesperado: {ex.Message}");
            }

            return View(gastoOriginal); // Devolvemos el original con los datos actualizados parcialmente
        }


        #endregion

        #region 5 DELETE: Gasto/Delete/5    
        
        [Authorize]

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gasto = await _context.Gastos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gasto == null)
            {
                return NotFound();
            }

            return View(gasto);
        }

        // POST: Gastoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gasto = await _context.Gastos.FindAsync(id);
            if (gasto != null)
            {
                _context.Gastos.Remove(gasto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GastoExists(int id)
        {
            return _context.Gastos.Any(e => e.Id == id);
        }
        #endregion

        #region 6 GET: Gasto/Estadisticas
        [Authorize]
        public async Task<IActionResult> Estadisticas(int? mes, int? anio)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int usuarioId))
            {
                return Unauthorized();
            }

            var gastosQuery = _context.Gastos
                .Where(g => g.UsuarioId == usuarioId);

            if (mes.HasValue && anio.HasValue)
            {
                gastosQuery = gastosQuery.Where(g => g.Fecha.Month == mes && g.Fecha.Year == anio);
            }

            var gastos = await gastosQuery.ToListAsync();

            var total = gastos.Sum(g => g.Monto);
            var categoriaTop = gastos
                .GroupBy(g => g.Categoria)
                .OrderByDescending(g => g.Sum(x => x.Monto))
                .Select(g => g.Key)
                .FirstOrDefault() ?? "Sin categoría";

            var viewModel = new EstadisticaGastosViewModel
            {
                TotalGastado = total,
                CategoriaTop = categoriaTop,
                CantidadGastos = gastos.Count,
                Categorias = gastos.GroupBy(g => g.Categoria ?? "Sin categoría").Select(g => g.Key).ToList(),
                Montos = gastos.GroupBy(g => g.Categoria ?? "Sin categoría").Select(g => g.Sum(x => x.Monto)).ToList()
            };

            return View(viewModel);
        }

        [Authorize]
        public IActionResult DescargarPdf()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var gastos = _context.Gastos
                .Where(g => g.UsuarioId == userId)
                .ToList();

            var pdfBytes = _pdfService.GeneratePdf(gastos);
            return File(pdfBytes, "application/pdf", "Reporte-Gastos.pdf");
        }

        #endregion
    }
}
