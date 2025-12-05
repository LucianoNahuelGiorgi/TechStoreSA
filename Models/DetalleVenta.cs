using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TechStoreSA.Models
{
    public class DetalleVenta
    {
        [Key]
        public int Id { get; set; }

        public int VentaId { get; set; }
        public virtual Venta Venta { get; set; } = null!;

        public int ProductoId { get; set; }
        public virtual Producto Producto { get; set; } = null!;

        public int Cantidad { get; set; }

        // Importante: Guardar el precio HISTÓRICO al momento de la venta
        // Si el producto cambia de precio mañana, esta venta no debe cambiar.
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Importe { get; set; } // Cantidad * PrecioUnitario
    }
}
