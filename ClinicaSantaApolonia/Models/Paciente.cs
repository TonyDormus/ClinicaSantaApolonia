using ClinicaSantaApolonia.Models;
using Microsoft.AspNetCore.Identity;

public class Paciente
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    public string Direccion { get; set; }
    public string UserId { get; set; } // Asociar con IdentityUser

    // Puedes incluir la navegación al usuario aquí si es necesario
    public IdentityUser Usuario { get; set; }

    public ICollection<HistorialMedico> Historial { get; set; } = new List<HistorialMedico>();



}
