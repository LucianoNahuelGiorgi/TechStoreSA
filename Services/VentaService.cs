using Microsoft.EntityFrameworkCore;
using TechStoreSA.Data;
using TechStoreSA.Enums;
using TechStoreSA.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace TechStoreSA.Services
{
    // Clase auxiliar para pasar los items desde el formulario
    public class ItemVenta
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class VentaService
    {
        private readonly TechStoreContext _context;
        private readonly ClienteService _clienteService;

        public VentaService(TechStoreContext context, ClienteService clienteService)
        {
            _context = context;
            _clienteService = clienteService;
        }

        // CAMBIO PRINCIPAL: Ahora devuelve 'Venta' en lugar de 'void'
        public Venta CrearVenta(int clienteId, int vendedorId, int sucursalId, MetodoPago metodoPago, List<ItemVenta> items)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                if (items == null || !items.Any())
                    throw new Exception("No se puede crear una venta sin productos.");

                var cliente = _context.Clientes.Find(clienteId)
                    ?? throw new Exception("Cliente no encontrado.");

                var nuevaVenta = new Venta
                {
                    Fecha = DateTime.Now,
                    ClienteId = clienteId,
                    VendedorId = vendedorId,
                    SucursalId = sucursalId,
                    MetodoPago = metodoPago,
                    Detalles = new List<DetalleVenta>()
                };

                decimal subTotalAcumulado = 0;

                foreach (var item in items)
                {
                    var producto = _context.Productos.Find(item.ProductoId)
                        ?? throw new Exception($"Producto con ID {item.ProductoId} no encontrado.");

                    var stockEnSucursal = _context.StocksSucursales
                        .FirstOrDefault(s => s.SucursalId == sucursalId && s.ProductoId == item.ProductoId);

                    if (stockEnSucursal == null || stockEnSucursal.Cantidad < item.Cantidad)
                    {
                        throw new Exception($"Stock insuficiente para '{producto.Nombre}'. Disponible: {stockEnSucursal?.Cantidad ?? 0}");
                    }

                    stockEnSucursal.Cantidad -= item.Cantidad;

                    var detalle = new DetalleVenta
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.PrecioActual,
                        Importe = producto.PrecioActual * item.Cantidad
                    };

                    nuevaVenta.Detalles.Add(detalle);
                    subTotalAcumulado += detalle.Importe;
                }

                nuevaVenta.SubTotal = subTotalAcumulado;
                decimal porcentajeDescuento = _clienteService.ObtenerPorcentajeDescuento(cliente.Tipo);
                nuevaVenta.DescuentoAplicado = subTotalAcumulado * porcentajeDescuento;
                nuevaVenta.Total = nuevaVenta.SubTotal - nuevaVenta.DescuentoAplicado;

                _context.Ventas.Add(nuevaVenta);
                _context.SaveChanges();
                transaction.Commit();

                // RETORNO CLAVE: Devolvemos la venta recargada con todos los datos para la factura
                return _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Sucursal)
                    .Include(v => v.Vendedor)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .First(v => v.Id == nuevaVenta.Id);
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Venta> ObtenerHistorial(DateTime? desde, DateTime? hasta, int? sucursalId)
        {
            var query = _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Vendedor)
                .Include(v => v.Sucursal)
                .AsQueryable();

            if (desde.HasValue) query = query.Where(v => v.Fecha >= desde.Value);
            if (hasta.HasValue) query = query.Where(v => v.Fecha <= hasta.Value);
            if (sucursalId.HasValue) query = query.Where(v => v.SucursalId == sucursalId.Value);

            return query.OrderByDescending(v => v.Fecha).ToList();
        }

        public Venta? ObtenerVentaConDetalles(int ventaId)
        {
            return _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Vendedor)
                .Include(v => v.Sucursal)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefault(v => v.Id == ventaId);
        }
    }
}