using ClinicaSantaApolonia.Pages;
using System;
using System.ComponentModel.DataAnnotations;

public class Cita
{
    public int Id { get; set; }

    [Required]
    public DateTime FechaHora { get; set; }

    [Required]
    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; }

    public int? OdontologoId { get; set; }
    public Odontologo Odontologo { get; set; }

    public int? NotasTratamientoId { get; set; }
    public NotasTratamiento TipoTratamiento { get; set; }

    public int? HorarioId { get; set; }
    public OdontologoHorarioDetalle HorarioDetalle { get; set; }

    public string Estado { get; set; } = "Pendiente";

    public string Notas { get; set; }

    public decimal Precio { get; set; }
}
