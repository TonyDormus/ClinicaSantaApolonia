using System;
using System.ComponentModel.DataAnnotations;

public class OdontologoHorarioDetalle
{
    public int Id { get; set; }

    [Required]
    public int HorarioId { get; set; }
    public OdontologoHorario Horario { get; set; }

    [Required]
    public DayOfWeek DiaSemana { get; set; }

    [Required]
    public TimeSpan HoraInicio { get; set; }

    [Required]
    public TimeSpan HoraFin { get; set; }

    [Required]
    public DateTime FechaInicio { get; set; }

    [Required]
    public DateTime FechaFin { get; set; }

    [Required]
    public int OdontologoId { get; set; } // Agregado
    public Odontologo Odontologo { get; set; } // Agregado

    // Agrega una colección de Citas si es necesario
    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
