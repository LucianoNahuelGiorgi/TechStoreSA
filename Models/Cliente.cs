using System.ComponentModel.DataAnnotations;

using TechStoreSA.Enums;

namespace TechStoreSA.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Documento { get; set; } = string.Empty; // DNI o CUIT

        [MaxLength(100)]
        public string? Email { get; set; }

        public TipoCliente Tipo { get; set; }

        public virtual ICollection<Venta> Compras { get; set; } = new List<Venta>();
    }
}
