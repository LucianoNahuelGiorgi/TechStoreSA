using System;
using System.Collections.Generic;
using System.Text;

namespace TechStoreSA.Models
{
    // Entidad intermedia para manejar stock por sucursal
    public class StockSucursal
    {
        public int SucursalId { get; set; }
        public virtual Sucursal Sucursal { get; set; } = null!;

        public int ProductoId { get; set; }
        public virtual Producto Producto { get; set; } = null!;

        public int Cantidad { get; set; }
    }
}
