using Microsoft.AspNetCore.Identity;

namespace ClinicaSantaApolonia.Models
{
    public class HistorialMedico
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public Paciente Paciente { get; set; }
        public string AntecedentesMedicos { get; set; }
        public string TratamientosPrevios { get; set; }
        public string Alergias { get; set; }
        public string NotasAdicionales { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
    }

}
