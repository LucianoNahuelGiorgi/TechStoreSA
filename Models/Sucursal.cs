using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TechStoreSA.Models
{
    public class Sucursal
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Direccion { get; set; } = string.Empty;

        public virtual ICollection<StockSucursal> Stocks { get; set; } = new List<StockSucursal>();
        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
