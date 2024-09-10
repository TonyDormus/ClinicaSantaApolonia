using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Odontologo
{
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; }

    [Required]
    public string Apellido { get; set; }

    [Required]

    public string Especialidad { get; set; }
    [Required]
    public string Email { get; set; }
    public ICollection<OdontologoHorario> OdontologoHorarios { get; set; } = new List<OdontologoHorario>();

    public string UserId { get; set; } // Asociar con IdentityUser

    // Agrega una colección de Citas si es necesario
    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
