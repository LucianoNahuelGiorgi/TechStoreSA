using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TechStoreSA.Models;

namespace TechStoreSA.Data
{
    public class TechStoreContext : DbContext
    {
        // Sets de tablas
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<StockSucursal> StocksSucursales { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configuración para LocalDB (incluida en Visual Studio)
            // Si usas SQL Express, cambia el Data Source.
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TechStoreDB;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CONFIGURACIÓN DE RELACIONES MUCHOS A MUCHOS (StockSucursal)
            // Definimos que la PK es compuesta
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

            // CONFIGURACIÓN ADICIONAL
            // Asegurar que el DNI/Documento del cliente sea único
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Documento)
                .IsUnique();

            // Asegurar que el código SKU del producto sea único
            modelBuilder.Entity<Producto>()
                .HasIndex(p => p.Codigo)
                .IsUnique();

            // Configuración de borrado en cascada (Opcional, pero recomendado revisar)
            // Por ejemplo, si borro una Venta, se borran sus detalles.
            modelBuilder.Entity<DetalleVenta>()
               .HasOne(d => d.Venta)
               .WithMany(v => v.Detalles)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
