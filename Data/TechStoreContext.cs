using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TechStoreSA.Models;

namespace TechStoreSA.Data
{
    public class TechStoreContext : DbContext
    {
        // --- 1. ESTE ES EL CONSTRUCTOR QUE FALTABA ---
        public TechStoreContext(DbContextOptions<TechStoreContext> options) : base(options)
        {
        }

        // Sets de tablas
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<StockSucursal> StocksSucursales { get; set; } // Nombre corregido para coincidir con tu propiedad
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configuración de respaldo por si no se pasa desde Program.cs
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TechStoreDB;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CONFIGURACIÓN DE RELACIONES MUCHOS A MUCHOS (StockSucursal)
            modelBuilder.Entity<StockSucursal>()
                .HasKey(ss => new { ss.SucursalId, ss.ProductoId });

            modelBuilder.Entity<StockSucursal>()
                .HasOne(ss => ss.Sucursal)
                .WithMany(s => s.Stocks)
                .HasForeignKey(ss => ss.SucursalId);

            modelBuilder.Entity<StockSucursal>()
                .HasOne(ss => ss.Producto)
                .WithMany(p => p.Stocks)
                .HasForeignKey(ss => ss.ProductoId);

            // ÍNDICES ÚNICOS
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Documento)
                .IsUnique();

            modelBuilder.Entity<Producto>()
                .HasIndex(p => p.Codigo)
                .IsUnique();

            // BORRADO EN CASCADA PARA DETALLES
            modelBuilder.Entity<DetalleVenta>()
               .HasOne(d => d.Venta)
               .WithMany(v => v.Detalles)
               .OnDelete(DeleteBehavior.Cascade);

            // --- 2. SEED DE DATOS (Usuario Admin por defecto) ---
            // Importante para poder loguearte la primera vez
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    NombreCompleto = "Administrador Sistema",
                    NombreUsuario = "admin",
                    PasswordHash = "1234", // Contraseña por defecto
                    EsAdministrador = true
                }
            );
        }
    }
}
