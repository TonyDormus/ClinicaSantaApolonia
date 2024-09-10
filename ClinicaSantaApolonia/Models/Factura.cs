using System;
using System.ComponentModel.DataAnnotations;

public class Factura
{
    public int Id { get; set; }

    [Required]
    public int CitaId { get; set; }

    public Cita Cita { get; set; }

    [Required]
    public DateTime FechaEmision { get; set; }

    [Required]
    public Paciente Paciente { get; set; }

    [Required]
    public int PacienteId { get; set; }

    [Required]
    public Odontologo Odontologo { get; set; }

    public int? OdontologoId { get; set; }

    [Required]
    public decimal PrecioTratamiento { get; set; }

    [Required]
    public decimal CantidadPagada { get; set; }

    [Required]
    public decimal MontoDevuelto { get; set; }

    public decimal? Descuento { get; set; } // Descuento opcional

    // Propiedad para calcular el subtotal
    public decimal Subtotal
    {
        get
        {
            return PrecioTratamiento - (Descuento ?? 0);
        }
    }

    // Propiedad para calcular el total
    public decimal Total
    {
        get
        {
            return Subtotal - MontoDevuelto;
        }
    }

    // Constructor
    public Factura()
    {
        // Constructor vacío necesario para Entity Framework
    }

    public Factura(int citaId, DateTime fechaEmision, Paciente paciente, int pacienteId, Odontologo odontologo, int? odontologoId, decimal precioTratamiento, decimal cantidadPagada, decimal montoDevuelto, decimal? descuento = null)
    {
        CitaId = citaId;
        FechaEmision = fechaEmision;
        Paciente = paciente;
        PacienteId = pacienteId;
        Odontologo = odontologo;
        OdontologoId = odontologoId;
        PrecioTratamiento = precioTratamiento;
        CantidadPagada = cantidadPagada;
        MontoDevuelto = montoDevuelto;
        Descuento = descuento;
    }
}
