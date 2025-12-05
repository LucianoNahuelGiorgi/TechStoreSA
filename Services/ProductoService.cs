using Microsoft.EntityFrameworkCore;
using TechStoreSA.Data;
using TechStoreSA.Models;

namespace TechStoreSA.Services
{
    public class ProductoService
    {
        private readonly TechStoreContext _context;

        public ProductoService(TechStoreContext context)
        {
            _context = context;
        }

        // 1. Obtener todos (incluyendo el nombre de la categoría para mostrar en la grilla)
        public List<Producto> ObtenerTodos()
        {
            return _context.Productos
                           .Include(p => p.Categoria) // Join con Categoría
                           .OrderBy(p => p.Nombre)
                           .ToList();
        }

        // 2. Buscar por coincidencia (Nombre o Código)
        public List<Producto> Buscar(string criterio)
        {
            return _context.Productos
                           .Include(p => p.Categoria)
                           .Where(p => p.Nombre.Contains(criterio) || p.Codigo.Contains(criterio))
                           .ToList();
        }

        // 3. Obtener un producto específico
        public Producto? ObtenerPorId(int id)
        {
            return _context.Productos.Find(id);
        }

        // 4. Obtener producto por Código (SKU) - Útil para el lector de código de barras
        public Producto? ObtenerPorCodigo(string codigo)
        {
            return _context.Productos
                           .Include(p => p.Stocks) // Traemos stocks por si necesitamos consultar
                           .FirstOrDefault(p => p.Codigo == codigo);
        }

        // 5. Crear Producto
        public void Crear(Producto producto)
        {
            // Validar que el código SKU no exista
            bool codigoExiste = _context.Productos.Any(p => p.Codigo == producto.Codigo);
            if (codigoExiste)
            {
                throw new Exception($"El código '{producto.Codigo}' ya está registrado en otro producto.");
            }

            // Validar que la categoría exista
            if (!_context.Categorias.Any(c => c.Id == producto.CategoriaId))
            {
                throw new Exception("La categoría seleccionada no es válida.");
            }

            // Validar precio positivo
            if (producto.PrecioActual < 0)
            {
                throw new Exception("El precio no puede ser negativo.");
            }

            _context.Productos.Add(producto);
            _context.SaveChanges();
        }

        // 6. Editar Producto (Actualizar información y precios)
        public void Editar(Producto producto)
        {
            var productoExistente = _context.Productos.Find(producto.Id);
            if (productoExistente == null)
            {
                throw new Exception("El producto no existe.");
            }

            // Validar que si cambiaron el código, el nuevo no choque con otro
            bool codigoDuplicado = _context.Productos
                .Any(p => p.Codigo == producto.Codigo && p.Id != producto.Id);

            if (codigoDuplicado)
            {
                throw new Exception($"El código '{producto.Codigo}' ya pertenece a otro producto.");
            }

            // Actualizamos campos
            productoExistente.Codigo = producto.Codigo;
            productoExistente.Nombre = producto.Nombre;
            productoExistente.Descripcion = producto.Descripcion;
            productoExistente.PrecioActual = producto.PrecioActual; // Actualización de precio
            productoExistente.CategoriaId = producto.CategoriaId;

            _context.SaveChanges();
        }

        // 7. Eliminar Producto
        public void Eliminar(int id)
        {
            // Validar integridad: No borrar si ya se ha vendido
            // Revisamos si existe algún DetalleVenta con este ProductoId
            bool tieneVentas = _context.DetallesVenta.Any(d => d.ProductoId == id);

            if (tieneVentas)
            {
                // Opción arquitectónica: Soft Delete (marcar como inactivo) 
                // Pero para este ejercicio lanzamos excepción.
                throw new Exception("No se puede eliminar el producto porque tiene historial de ventas.");
            }

            // También revisar si tiene stock físico asignado en alguna sucursal
            bool tieneStock = _context.StocksSucursales.Any(s => s.ProductoId == id && s.Cantidad > 0);
            if (tieneStock)
            {
                throw new Exception("No se puede eliminar un producto que tiene stock físico en sucursales.");
            }

            var producto = _context.Productos.Find(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                _context.SaveChanges();
            }
        }

        // 8. Requerimiento: Consultar disponibilidad de productos por sucursal
        public int ConsultarStock(int productoId, int sucursalId)
        {
            var stockEntidad = _context.StocksSucursales
                .FirstOrDefault(s => s.ProductoId == productoId && s.SucursalId == sucursalId);

            return stockEntidad?.Cantidad ?? 0; // Si es null, retorna 0
        }

        // Extra: Método para ajustar stock manual (ej: Carga inicial o ajuste de inventario)
        // Nota: Las ventas restarán stock automáticamente a través del VentaService
        public void AjustarStockManual(int productoId, int sucursalId, int nuevaCantidad)
        {
            var stock = _context.StocksSucursales
               .FirstOrDefault(s => s.ProductoId == productoId && s.SucursalId == sucursalId);

            if (stock == null)
            {
                // Si no existe el registro de stock para esa sucursal, lo creamos
                var nuevoStock = new StockSucursal
                {
                    ProductoId = productoId,
                    SucursalId = sucursalId,
                    Cantidad = nuevaCantidad
                };
                _context.StocksSucursales.Add(nuevoStock);
            }
            else
            {
                stock.Cantidad = nuevaCantidad;
            }

            _context.SaveChanges();
        }
    }
}
