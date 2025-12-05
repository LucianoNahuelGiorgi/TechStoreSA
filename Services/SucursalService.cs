using System;
using System.Collections.Generic;
using System.Text;
using TechStoreSA.Data;
using TechStoreSA.Models;

namespace TechStoreSA.Services
{
    public class SucursalService
    {
        private readonly TechStoreContext _context;

        public SucursalService(TechStoreContext context)
        {
            _context = context;
        }

        // 1. Obtener todas las sucursales
        public List<Sucursal> ObtenerTodas()
        {
            return _context.Sucursales
                           .OrderBy(s => s.Nombre)
                           .ToList();
        }

        // 2. Obtener por ID
        public Sucursal? ObtenerPorId(int id)
        {
            return _context.Sucursales.Find(id);
        }

        // 3. Agregar nueva sucursal
        public void Agregar(Sucursal sucursal)
        {
            // Validar que no exista otra sucursal con el mismo nombre exacto
            bool existe = _context.Sucursales.Any(s => s.Nombre.ToLower() == sucursal.Nombre.ToLower());
            if (existe)
            {
                throw new Exception("Ya existe una sucursal con ese nombre.");
            }

            _context.Sucursales.Add(sucursal);
            _context.SaveChanges();
        }

        // 4. Editar sucursal existente
        public void Editar(Sucursal sucursal)
        {
            var sucursalExistente = _context.Sucursales.Find(sucursal.Id);
            if (sucursalExistente == null)
            {
                throw new Exception("La sucursal no existe.");
            }

            // Validar duplicados en edición
            bool nombreDuplicado = _context.Sucursales
                .Any(s => s.Nombre.ToLower() == sucursal.Nombre.ToLower() && s.Id != sucursal.Id);

            if (nombreDuplicado)
            {
                throw new Exception("El nombre ya está siendo utilizado por otra sucursal.");
            }

            sucursalExistente.Nombre = sucursal.Nombre;
            sucursalExistente.Direccion = sucursal.Direccion;

            // ELIMINADO: sucursalExistente.Telefono y Email porque no están en el modelo original.

            _context.SaveChanges();
        }

        // 5. Eliminar sucursal
        public void Eliminar(int id)
        {
            // ELIMINADO: Verificación de usuarios (A) porque Usuario no tiene SucursalId en el modelo original.

            // B. Verificar si la sucursal tiene stock de productos
            // (Esto sí se mantiene porque StockSucursal es una tabla intermedia que sí relaciona ambos)
            bool tieneStock = _context.StocksSucursales.Any(s => s.SucursalId == id && s.Cantidad > 0);
            if (tieneStock)
            {
                throw new Exception("No se puede eliminar la sucursal porque tiene productos en stock.");
            }

            // C. Verificar historial de ventas
            // Si Venta.cs tiene SucursalId, esto podría descomentarse.
            /*
            bool tieneVentas = _context.Ventas.Any(v => v.SucursalId == id);
            if (tieneVentas)
            {
                throw new Exception("No se puede eliminar la sucursal porque tiene historial de ventas.");
            }
            */

            var sucursal = _context.Sucursales.Find(id);
            if (sucursal != null)
            {
                _context.Sucursales.Remove(sucursal);
                _context.SaveChanges();
            }
        }
    }
}
