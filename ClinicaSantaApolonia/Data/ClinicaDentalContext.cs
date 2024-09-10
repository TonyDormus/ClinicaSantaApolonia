using ClinicaSantaApolonia.Models;
using ClinicaSantaApolonia.Pages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ClinicaDentalContext : IdentityDbContext<IdentityUser>
{
    public ClinicaDentalContext(DbContextOptions<ClinicaDentalContext> options)
        : base(options)
    {
    }

    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<Cita> Citas { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<Odontologo> Odontologos { get; set; }
    public DbSet<HistorialMedico> HistorialesMedicos { get; set; }
    public DbSet<NotasTratamiento> NotasTratamientos { get; set; }
    public DbSet<OdontologoHorario> OdontologosHorarios { get; set; }
    public DbSet<OdontologoHorarioDetalle> OdontologosHorariosDetalles { get; set; }
    public DbSet<PermisoRol> PermisosRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Importante: llamar a base.OnModelCreating(modelBuilder) para configurar las entidades de identidad
        base.OnModelCreating(modelBuilder);

        // Configuración de la relación entre PermisoRol
        modelBuilder.Entity<PermisoRol>()
            .HasIndex(pr => new { pr.RolId, pr.NombrePagina })
            .IsUnique();

        // Configuración de la relación entre HistorialMedico y Paciente
        modelBuilder.Entity<HistorialMedico>()
            .HasOne(h => h.Paciente)
            .WithMany(p => p.Historial)
            .HasForeignKey(h => h.PacienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configuración de la relación entre Cita y Odontologo
        modelBuilder.Entity<Cita>()
            .HasOne(c => c.Odontologo)
            .WithMany(o => o.Citas) // Asume que Odontologo tiene una colección de Citas
            .HasForeignKey(c => c.OdontologoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configuración de la relación entre Cita y NotasTratamiento
        modelBuilder.Entity<Cita>()
            .HasOne(c => c.TipoTratamiento)
            .WithMany(n => n.Citas) // Asume que NotasTratamiento tiene una colección de Citas
            .HasForeignKey(c => c.NotasTratamientoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configuración de la relación entre Cita y OdontologoHorarioDetalle
        modelBuilder.Entity<Cita>()
            .HasOne(c => c.HorarioDetalle)
            .WithMany(d => d.Citas) // Asume que OdontologoHorarioDetalle tiene una colección de Citas
            .HasForeignKey(c => c.HorarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configuración de la relación entre Odontologo y OdontologoHorario
        modelBuilder.Entity<Odontologo>()
            .HasMany(o => o.OdontologoHorarios)
            .WithOne(h => h.Odontologo)
            .HasForeignKey(h => h.OdontologoId);

        // Configuración de la relación entre OdontologoHorario y OdontologoHorarioDetalle
        modelBuilder.Entity<OdontologoHorario>()
            .HasMany(h => h.Detalles)
            .WithOne(d => d.Horario)
            .HasForeignKey(d => d.HorarioId);
    }
}
