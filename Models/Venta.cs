using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

using TechStoreSA.Enums;

namespace TechStoreSA.Models
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public int ClienteId { get; set; }
        public virtual Cliente Cliente { get; set; } = null!;

        public int VendedorId { get; set; } // Usuario
        public virtual Usuario Vendedor { get; set; } = null!;

        public int SucursalId { get; set; }
        public virtual Sucursal Sucursal { get; set; } = null!;

        public MetodoPago MetodoPago { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DescuentoAplicado { get; set; } // Guardamos el valor monetario del descuento

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public virtual ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
