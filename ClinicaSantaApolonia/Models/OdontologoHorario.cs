using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class OdontologoHorario
{
    public int Id { get; set; }

    [Required]
    public int OdontologoId { get; set; }
    public Odontologo Odontologo { get; set; }

    public ICollection<OdontologoHorarioDetalle> Detalles { get; set; } = new List<OdontologoHorarioDetalle>();
}
