using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TechStoreSA.Models
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Codigo { get; set; } = string.Empty; // SKU

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(18,2)")] // Importante para dinero
        public decimal PrecioActual { get; set; }

        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; } = null!;

        // Relación: Stock por sucursal
        public virtual ICollection<StockSucursal> Stocks { get; set; } = new List<StockSucursal>();
    }
}
