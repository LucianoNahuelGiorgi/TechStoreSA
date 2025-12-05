using Microsoft.EntityFrameworkCore;
using TechStoreSA.Data;
using TechStoreSA.Models;

namespace TechStoreSA.Services
{
    // DTOs: Clases simples para transportar los resultados de los reportes a la Vista
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

    public class ReporteService
    {
        private readonly TechStoreContext _context;

        public ReporteService(TechStoreContext context)
        {
            _context = context;
        }

        // 1. REPORTE: Productos más vendidos (Top N) 
        // Responde a: "¿Qué es lo que más se vende?"
        public List<ReporteProductoDTO> ObtenerProductosMasVendidos(DateTime desde, DateTime hasta, int topN = 5)
        {
            // Consultamos los DETALLES de venta, que es donde está el producto y la cantidad
            var query = _context.DetallesVenta
                .Include(d => d.Venta)
                .Where(d => d.Venta.Fecha >= desde && d.Venta.Fecha <= hasta) // Filtrar por fecha de la venta cabecera
                .GroupBy(d => d.Producto.Nombre) // Agrupar por nombre de producto
                .Select(g => new ReporteProductoDTO
                {
                    Producto = g.Key,
                    CantidadVendida = g.Sum(d => d.Cantidad), // Sumar cantidades
                    IngresosGenerados = g.Sum(d => d.Importe) // Sumar dinero generado
                })
                .OrderByDescending(r => r.CantidadVendida) // Ordenar del más vendido al menos
                .Take(topN) // Tomar solo los primeros N
                .ToList();

            return query;
        }

        // 2. REPORTE: Desempeño de Vendedores 
        // Responde a: "¿Quién está vendiendo más?"
        public List<ReporteVendedorDTO> ObtenerVentasPorVendedor(DateTime desde, DateTime hasta)
        {
            var query = _context.Ventas
                .Where(v => v.Fecha >= desde && v.Fecha <= hasta)
                .GroupBy(v => v.Vendedor.NombreCompleto) // Agrupar por nombre del vendedor
                .Select(g => new ReporteVendedorDTO
                {
                    Vendedor = g.Key,
                    CantidadVentas = g.Count(),      // Cuántas facturas hizo
                    TotalFacturado = g.Sum(v => v.Total) // Cuánto dinero recaudó
                })
                .OrderByDescending(r => r.TotalFacturado)
                .ToList();

            return query;
        }

        // 3. REPORTE: Ventas por Sucursal (Totalizado) 
        public Dictionary<string, decimal> ObtenerTotalVentasPorSucursal(DateTime desde, DateTime hasta)
        {
            return _context.Ventas
                .Where(v => v.Fecha >= desde && v.Fecha <= hasta)
                .GroupBy(v => v.Sucursal.Nombre)
                .Select(g => new { Sucursal = g.Key, Total = g.Sum(v => v.Total) })
                .ToDictionary(k => k.Sucursal, v => v.Total);
        }

        // 4. CONSULTA: Historial/Estado de Cliente [cite: 25]
        // Muestra todas las compras de un cliente específico.
        public List<Venta> ObtenerHistorialCliente(int clienteId)
        {
            return _context.Ventas
                .Include(v => v.Sucursal)
                .Include(v => v.Vendedor)
                // Opcional: Incluir detalles si se quiere ver qué compró en cada una
                // .Include(v => v.Detalles).ThenInclude(d => d.Producto) 
                .Where(v => v.ClienteId == clienteId)
                .OrderByDescending(v => v.Fecha)
                .ToList();
        }

        // 5. REPORTE GENERAL: Listado de Ventas detallado por período
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
