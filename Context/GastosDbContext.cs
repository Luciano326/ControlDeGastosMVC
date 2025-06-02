using ControlDeGastosMVC.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace ControlDeGastosMVC.API.Context
{
    public class GastosDbContext : DbContext
    {
        internal static readonly object _context;

        public GastosDbContext(DbContextOptions<GastosDbContext> options) : base(options) { }

        public DbSet<Gasto> Gastos { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<RecuperacionPassword> RecuperacionPassword { get; set; }

    }
}
 