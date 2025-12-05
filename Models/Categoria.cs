using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TechStoreSA.Models
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Descripcion { get; set; }

        // Relación: Una categoría tiene muchos productos
        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
