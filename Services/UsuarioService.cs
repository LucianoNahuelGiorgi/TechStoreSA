using Microsoft.EntityFrameworkCore;
using TechStoreSA.Data;
using TechStoreSA.Models;

namespace TechStoreSA.Services
{
    public class UsuarioService
    {
        private readonly TechStoreContext _context;

        public UsuarioService(TechStoreContext context)
        {
            _context = context;
        }

        // 1. LOGIN: Validar credenciales
        public Usuario? Login(string nombreUsuario, string password)
        {
            // Nota: password aquí se compara contra PasswordHash directamente.
            // (Para producción recuerda usar hasheo real).
            return _context.Usuarios
                           // ELIMINADO: .Include(u => u.Sucursal) porque no existe la relación en el modelo
                           .FirstOrDefault(u => u.NombreUsuario == nombreUsuario &&
                                                u.PasswordHash == password);
        }

        // 2. Obtener todos los usuarios
        public List<Usuario> ObtenerTodos()
        {
            return _context.Usuarios
                           .OrderBy(u => u.NombreCompleto)
                           .ToList();
        }

        // 3. Obtener por ID
        public Usuario? ObtenerPorId(int id)
        {
            return _context.Usuarios.Find(id);
        }

        // 4. Crear nuevo usuario
        public void Crear(Usuario usuario, string password)
        {
            // Validar nombre de usuario único
            bool usuarioExiste = _context.Usuarios.Any(u => u.NombreUsuario == usuario.NombreUsuario);
            if (usuarioExiste)
            {
                throw new Exception($"El nombre de usuario '{usuario.NombreUsuario}' ya está en uso.");
            }

            // ELIMINADO: Validación de SucursalId (no existe en el modelo)

            // Asignar contraseña
            usuario.PasswordHash = password;

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
        }

        // 5. Editar usuario
        public void Editar(Usuario usuario, string? nuevoPassword = null)
        {
            var usuarioExistente = _context.Usuarios.Find(usuario.Id);
            if (usuarioExistente == null)
            {
                throw new Exception("El usuario no existe.");
            }

            // Validar duplicidad
            bool usuarioDuplicado = _context.Usuarios
                .Any(u => u.NombreUsuario == usuario.NombreUsuario && u.Id != usuario.Id);

            if (usuarioDuplicado)
            {
                throw new Exception($"El nombre de usuario '{usuario.NombreUsuario}' ya pertenece a otro empleado.");
            }

            // Actualizar datos
            usuarioExistente.NombreCompleto = usuario.NombreCompleto;
            usuarioExistente.NombreUsuario = usuario.NombreUsuario;
            usuarioExistente.EsAdministrador = usuario.EsAdministrador;

            // ELIMINADO: usuarioExistente.SucursalId (no existe)

            // Actualizar contraseña si se proporciona
            if (!string.IsNullOrWhiteSpace(nuevoPassword))
            {
                usuarioExistente.PasswordHash = nuevoPassword;
            }

            _context.SaveChanges();
        }

        // 6. Eliminar Usuario
        public void Eliminar(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario == null) throw new Exception("Usuario no encontrado.");

            // Validar integridad (Ventas)
            bool tieneVentas = _context.Ventas.Any(v => v.VendedorId == id);
            if (tieneVentas)
            {
                throw new Exception("No se puede eliminar el usuario porque tiene historial de ventas.");
            }

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();
        }
    }
}
