using Microsoft.EntityFrameworkCore;
using TechStoreSA.Data;
using TechStoreSA.Enums;
using TechStoreSA.Models;

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

        // Inyectamos también el ClienteService para reutilizar la lógica del descuento
        public VentaService(TechStoreContext context, ClienteService clienteService)
        {
            _context = context;
            _clienteService = clienteService;
        }

        // MÉTODO PRINCIPAL: REALIZAR VENTA
        public void CrearVenta(int clienteId, int vendedorId, int sucursalId, MetodoPago metodoPago, List<ItemVenta> items)
        {
            // Usamos una transacción explícita para asegurar integridad
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // 1. Validaciones iniciales
                if (items == null || !items.Any())
                    throw new Exception("No se puede crear una venta sin productos.");

                var cliente = _context.Clientes.Find(clienteId)
                    ?? throw new Exception("Cliente no encontrado.");

                // 2. Instanciar la Venta (Encabezado)
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

                // 3. Procesar cada item (Detalle)
                foreach (var item in items)
                {
                    // A. Obtener producto para sacar el Precio Actual
                    var producto = _context.Productos.Find(item.ProductoId)
                        ?? throw new Exception($"Producto con ID {item.ProductoId} no encontrado.");

                    // B. Verificar Stock en la Sucursal específica
                    var stockEnSucursal = _context.StocksSucursales
                        .FirstOrDefault(s => s.SucursalId == sucursalId && s.ProductoId == item.ProductoId);

                    if (stockEnSucursal == null || stockEnSucursal.Cantidad < item.Cantidad)
                    {
                        throw new Exception($"Stock insuficiente para '{producto.Nombre}'. Disponible: {stockEnSucursal?.Cantidad ?? 0}");
                    }

                    // C. Descontar Stock (Actualizar Inventario)
                    stockEnSucursal.Cantidad -= item.Cantidad;

                    // D. Crear el Detalle
                    var detalle = new DetalleVenta
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        // Guardamos el precio del momento (Snapshot)
                        PrecioUnitario = producto.PrecioActual,
                        Importe = producto.PrecioActual * item.Cantidad
                    };

                    nuevaVenta.Detalles.Add(detalle);
                    subTotalAcumulado += detalle.Importe;
                }

                // 4. Calcular Totales y Descuentos
                nuevaVenta.SubTotal = subTotalAcumulado;

                // Consultamos el % de descuento al servicio de clientes (Lógica centralizada)
                decimal porcentajeDescuento = _clienteService.ObtenerPorcentajeDescuento(cliente.Tipo);

                nuevaVenta.DescuentoAplicado = subTotalAcumulado * porcentajeDescuento;
                nuevaVenta.Total = nuevaVenta.SubTotal - nuevaVenta.DescuentoAplicado;

                // 5. Guardar en Base de Datos
                _context.Ventas.Add(nuevaVenta);
                _context.SaveChanges();

                // 6. Confirmar transacción
                transaction.Commit();
            }
            catch (Exception)
            {
                // Si algo falla, deshacer todo (incluyendo la resta de stock)
                transaction.Rollback();
                throw; // Re-lanzar el error para que lo muestre el Formulario
            }
        }
    }
}
