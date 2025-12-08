using Microsoft.EntityFrameworkCore;
using TechStoreSA.Data;
using TechStoreSA.Enums;
using TechStoreSA.Models;

namespace TechStoreSA.Services
{
    public class ClienteService
    {
        private readonly TechStoreContext _context;

        public ClienteService(TechStoreContext context)
        {
            _context = context;
        }

        // 1. Listar todos los clientes
        public List<Cliente> ObtenerTodos()
        {
            return _context.Clientes
                           .OrderBy(c => c.NombreCompleto)
                           .ToList();
        }

        // 2. Buscar clientes (por Nombre o DNI/CUIT)
        public List<Cliente> Buscar(string criterio)
        {
            return _context.Clientes
                           .Where(c => c.NombreCompleto.Contains(criterio) ||
                                       c.Documento.Contains(criterio))
                           .ToList();
        }

        // 3. Obtener un cliente específico (con historial de compras opcional)
        public Cliente? ObtenerPorId(int id)
        {
            return _context.Clientes
                           .Include(c => c.Compras) // Cargar historial si se necesita ver
                           .FirstOrDefault(c => c.Id == id);
        }

        // 4. Buscar por Documento exacto (útil al iniciar una venta para cargar cliente rápido)
        public Cliente? ObtenerPorDocumento(string documento)
        {
            return _context.Clientes.FirstOrDefault(c => c.Documento == documento);
        }

        // 5. Crear Cliente
        public void Crear(Cliente cliente)
        {
            // Validación: El documento (DNI/CUIT) debe ser único
            bool existeDocumento = _context.Clientes.Any(c => c.Documento == cliente.Documento);
            if (existeDocumento)
            {
                throw new Exception($"El documento '{cliente.Documento}' ya está registrado.");
            }

            // Validar datos mínimos
            if (string.IsNullOrWhiteSpace(cliente.NombreCompleto)) throw new Exception("El nombre es requerido.");

            _context.Clientes.Add(cliente);
            _context.SaveChanges();
        }

        // 6. Editar Cliente
        public void Editar(Cliente cliente)
        {
            var clienteExistente = _context.Clientes.Find(cliente.Id);
            if (clienteExistente == null)
            {
                throw new Exception("El cliente no existe.");
            }

            // Validar duplicidad de documento en edición
            bool documentoDuplicado = _context.Clientes
                .Any(c => c.Documento == cliente.Documento && c.Id != cliente.Id);

            if (documentoDuplicado)
            {
                throw new Exception($"El documento '{cliente.Documento}' ya pertenece a otro cliente.");
            }

            clienteExistente.NombreCompleto = cliente.NombreCompleto;
            clienteExistente.Documento = cliente.Documento;
            clienteExistente.Email = cliente.Email;
            clienteExistente.Tipo = cliente.Tipo; // Puede cambiar de Minorista a Mayorista

            _context.SaveChanges();
        }

        // 7. Eliminar Cliente
        public void Eliminar(int id)
        {
            // No borrar clientes con compras
            bool tieneCompras = _context.Ventas.Any(v => v.ClienteId == id);

            if (tieneCompras)
            {
                throw new Exception("No se puede eliminar el cliente porque tiene compras registradas en el sistema.");
            }

            var cliente = _context.Clientes.Find(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                _context.SaveChanges();
            }
        }

        // 8. Regla de Negocio: Obtener Descuento
        // Centralizamos aquí cuánto descuento corresponde según el tipo.
        // Si mañana la política cambia, solo tocas este método.
        public decimal ObtenerPorcentajeDescuento(TipoCliente tipo)
        {
            // Ejemplo de política:
            // Mayorista: 10% de descuento (0.10)
            // Minorista: 0% de descuento (0.00)

            if (tipo == TipoCliente.Mayorista)
            {
                return 0.10m;
            }

            return 0m;
        }
    }
}
