using Microsoft.EntityFrameworkCore;
using TechStoreSA.Data;
using TechStoreSA.Models;

namespace TechStoreSA.Services
{
    // --- DTOs: Objetos para mostrar datos en las grillas ---

    public class ReporteProductoDTO
    {
        public string Producto { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal IngresosGenerados { get; set; }
    }

    public class ReporteVendedorDTO
    {
        public string Vendedor { get; set; } = string.Empty;
        public int CantidadVentas { get; set; }
        public decimal TotalFacturado { get; set; }
    }

    public class ReporteEstadoClienteDTO
    {
        public string Cliente { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // Minorista/Mayorista
        public int CantidadCompras { get; set; }
        public decimal TotalGastado { get; set; }
    }

    // --- SERVICIO DE REPORTES ---

    public class ReporteService
    {
        private readonly TechStoreContext _context;

        public ReporteService(TechStoreContext context)
        {
            _context = context;
        }

        // 1. REPORTE: Productos más vendidos (Con corrección de descuento)
        public List<ReporteProductoDTO> ObtenerProductosMasVendidos(DateTime desde, DateTime hasta, int topN = 5)
        {
            var query = _context.DetallesVenta
                .Include(d => d.Venta)
                .Where(d => d.Venta.Fecha >= desde && d.Venta.Fecha <= hasta)
                .GroupBy(d => d.Producto.Nombre)
                .Select(g => new ReporteProductoDTO
                {
                    Producto = g.Key,
                    CantidadVendida = g.Sum(d => d.Cantidad),

                    IngresosGenerados = g.Sum(d => 
                        d.Venta.SubTotal > 0 
                        ? d.Importe * (d.Venta.Total / d.Venta.SubTotal) 
                        : 0)
                })
                .OrderByDescending(r => r.CantidadVendida)
                .Take(topN)
                .ToList();

            return query;
        }

        // 2. REPORTE: Desempeño de Vendedores
        public List<ReporteVendedorDTO> ObtenerVentasPorVendedor(DateTime desde, DateTime hasta)
        {
            return _context.Ventas
                .Where(v => v.Fecha >= desde && v.Fecha <= hasta)
                .GroupBy(v => v.Vendedor.NombreCompleto)
                .Select(g => new ReporteVendedorDTO
                {
                    Vendedor = g.Key,
                    CantidadVentas = g.Count(),
                    TotalFacturado = g.Sum(v => v.Total)
                })
                .OrderByDescending(r => r.TotalFacturado)
                .ToList();
        }

        // 3. REPORTE: Estado de Cuentas corrientes de Clientes
        public List<ReporteEstadoClienteDTO> ObtenerEstadoClientes()
        {

            return _context.Clientes
                .Select(c => new ReporteEstadoClienteDTO
                {
                    Cliente = c.NombreCompleto,
                    Tipo = c.Tipo.ToString(),
                    CantidadCompras = c.Compras.Count(),
                    TotalGastado = c.Compras.Sum(v => v.Total) 
                })
                .OrderByDescending(r => r.TotalGastado)
                .ToList();
        }

        // 4. REPORTE: Ventas por Sucursal 
        public Dictionary<string, decimal> ObtenerTotalVentasPorSucursal(DateTime desde, DateTime hasta)
        {
            return _context.Ventas
                .Where(v => v.Fecha >= desde && v.Fecha <= hasta)
                .GroupBy(v => v.Sucursal.Nombre)
                .Select(g => new { Sucursal = g.Key, Total = g.Sum(v => v.Total) })
                .ToDictionary(k => k.Sucursal, v => v.Total);
        }

        // 6. REPORTE GENERAL: Listado detallado de ventas
        public List<Venta> ObtenerVentasDetalladas(DateTime desde, DateTime hasta, int? sucursalId = null)
        {
            var query = _context.Ventas
               .Include(v => v.Cliente)
               .Include(v => v.Vendedor)
               .Include(v => v.Sucursal)
               .Where(v => v.Fecha >= desde && v.Fecha <= hasta);

            if (sucursalId.HasValue)
            {
                query = query.Where(v => v.SucursalId == sucursalId.Value);
            }

            return query.OrderByDescending(v => v.Fecha).ToList();
        }
    }
}
