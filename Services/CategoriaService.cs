using Microsoft.EntityFrameworkCore;
using TechStoreSA.Data;
using TechStoreSA.Models;

namespace TechStoreSA.Services
{
    public class CategoriaService
    {
        private readonly TechStoreContext _context;

        // Inyección del contexto (o creación en el constructor si es WinForms simple)
        public CategoriaService(TechStoreContext context)
        {
            _context = context;
        }

        // 1. Listar todas las categorías (para llenar ComboBoxes o Grillas)
        public List<Categoria> ObtenerTodas()
        {
            return _context.Categorias
                           .OrderBy(c => c.Nombre)
                           .ToList();
        }

        // 2. Obtener una por ID
        public Categoria? ObtenerPorId(int id)
        {
            return _context.Categorias.Find(id);
        }

        // 3. Crear nueva categoría
        public void Agregar(Categoria categoria)
        {
            // Validación de negocio: No permitir nombres duplicados
            bool existe = _context.Categorias.Any(c => c.Nombre.ToLower() == categoria.Nombre.ToLower());
            if (existe)
            {
                throw new Exception("Ya existe una categoría con ese nombre.");
            }

            _context.Categorias.Add(categoria);
            _context.SaveChanges();
        }

        // 4. Editar categoría existente
        public void Editar(Categoria categoria)
        {
            var categoriaExistente = _context.Categorias.Find(categoria.Id);
            if (categoriaExistente == null)
            {
                throw new Exception("La categoría no existe.");
            }

            // Validar que el nuevo nombre no choque con otra categoría distinta
            bool nombreDuplicado = _context.Categorias
                .Any(c => c.Nombre.ToLower() == categoria.Nombre.ToLower() && c.Id != categoria.Id);

            if (nombreDuplicado)
            {
                throw new Exception("El nombre ya está en uso por otra categoría.");
            }

            categoriaExistente.Nombre = categoria.Nombre;
            categoriaExistente.Descripcion = categoria.Descripcion;

            _context.SaveChanges();
        }

        // 5. Eliminar categoría
        public void Eliminar(int id)
        {
            var categoria = _context.Categorias
                                    .Include(c => c.Productos) // Incluimos productos para verificar dependencias
                                    .FirstOrDefault(c => c.Id == id);

            if (categoria == null) return;

            // No borrar si tiene productos asociados
            if (categoria.Productos.Any())
            {
                throw new Exception("No se puede eliminar la categoría porque tiene productos asociados.");
            }

            _context.Categorias.Remove(categoria);
            _context.SaveChanges();
        }
    }
}
