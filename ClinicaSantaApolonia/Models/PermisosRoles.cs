namespace ClinicaSantaApolonia.Models
{
    public class PermisoRol
    {
        public int Id { get; set; }
        public string RolId { get; set; } // Relacionado con AspNetRoles.Id
        public string NombrePagina { get; set; }
        public bool TieneAcceso { get; set; }
    }
}
