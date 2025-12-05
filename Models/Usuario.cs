using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TechStoreSA.Models
{
    public class Usuario // Vendedor / Administrador
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty; // Nunca guardar texto plano

        public bool EsAdministrador { get; set; }

        public virtual ICollection<Venta> VentasRealizadas { get; set; } = new List<Venta>();
    }
}
