using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaSantaApolonia.Pages
{
    public class NotasTratamiento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }
        [Required]

        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; } // Agregado para el precio
                                            // Agrega una colección de Citas si es necesario
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
