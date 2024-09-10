using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClinicaSantaApolonia.Models;

namespace ClinicaSantaApolonia.Pages
{
    public class ManageOdontologosModel : PageModel
    {
        private readonly ClinicaDentalContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ManageOdontologosModel(ClinicaDentalContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public Odontologo NuevoOdontologo { get; set; }

        [BindProperty]
        public int? OdontologoId { get; set; }

        [BindProperty]
        public int HorarioDetalleId { get; set; }

        [BindProperty]
        public DateTime FechaInicio { get; set; }

        [BindProperty]
        public DateTime FechaFin { get; set; }

        [BindProperty]
        public TimeSpan HoraInicio { get; set; }

        [BindProperty]
        public TimeSpan HoraFin { get; set; }

        public List<Odontologo> Odontologos { get; set; }

        public async Task OnGetAsync()
        {
            Odontologos = await _context.Odontologos
                .Include(o => o.OdontologoHorarios)
                    .ThenInclude(h => h.Detalles)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(string nombre, string apellido, string especialidad, string email, string password, int? odontologoId, DateTime? fechaInicio, DateTime? fechaFin, TimeSpan? horaInicio, TimeSpan? horaFin)
        {
            if (!string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(apellido) && !string.IsNullOrEmpty(especialidad) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                // Crear el usuario
                var user = new IdentityUser { UserName = email, Email = email };
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    // Asignar rol de Odontólogo
                    if (!await _roleManager.RoleExistsAsync("Odontologo"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Odontologo"));
                    }

                    await _userManager.AddToRoleAsync(user, "Odontologo");

                    // Crear el odontólogo
                    var odontologo = new Odontologo
                    {
                        Nombre = nombre,
                        Apellido = apellido,
                        Especialidad = especialidad,
                        UserId = user.Id,
                        Email = email // Asegurarse de guardar también el email en el modelo de Odontologo
                    };

                    _context.Odontologos.Add(odontologo);
                    await _context.SaveChangesAsync();

                    return RedirectToPage();
                }

                ModelState.AddModelError(string.Empty, "Error al crear el usuario.");
                return Page();
            }

            if (odontologoId.HasValue && fechaInicio.HasValue && fechaFin.HasValue && horaInicio.HasValue && horaFin.HasValue)
            {
                var odontologo = await _context.Odontologos
                    .Include(o => o.OdontologoHorarios)
                        .ThenInclude(h => h.Detalles)
                    .FirstOrDefaultAsync(o => o.Id == odontologoId.Value);

                if (odontologo != null)
                {
                    var horarioExistente = odontologo.OdontologoHorarios.SelectMany(h => h.Detalles)
                        .FirstOrDefault(d => d.FechaInicio == fechaInicio.Value && d.FechaFin == fechaFin.Value);

                    if (horarioExistente != null)
                    {
                        // Editar horario existente
                        horarioExistente.HoraInicio = horaInicio.Value;
                        horarioExistente.HoraFin = horaFin.Value;
                    }
                    else
                    {
                        // Crear un nuevo horario si no existe uno para el rango de fechas
                        var nuevoHorarioDetalle = new OdontologoHorarioDetalle
                        {
                            OdontologoId = odontologo.Id,
                            DiaSemana = fechaInicio.Value.DayOfWeek,
                            HoraInicio = horaInicio.Value,
                            HoraFin = horaFin.Value,
                            FechaInicio = fechaInicio.Value,
                            FechaFin = fechaFin.Value
                        };

                        var horario = odontologo.OdontologoHorarios.FirstOrDefault();
                        if (horario == null)
                        {
                            horario = new OdontologoHorario
                            {
                                Odontologo = odontologo,
                                Detalles = new List<OdontologoHorarioDetalle>()
                            };
                            odontologo.OdontologoHorarios.Add(horario);
                        }
                        horario.Detalles.Add(nuevoHorarioDetalle);
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToPage();
                }

                ModelState.AddModelError(string.Empty, "Error al agregar o editar el horario.");
                return Page();
            }

            // Si ninguna de las condiciones anteriores se cumple, retornar la página con un mensaje de error
            ModelState.AddModelError(string.Empty, "Debe proporcionar todos los campos requeridos.");
            return Page();
        }


        // Método para editar un odontólogo y sus horarios
        public async Task<IActionResult> OnPostEditAsync(string nombre, string apellido, string especialidad, string email, string password, DateTime? fechaInicio, DateTime? fechaFin, TimeSpan? horaInicio, TimeSpan? horaFin)
        {
            var odontologo = await _context.Odontologos
                .Include(o => o.OdontologoHorarios)
                    .ThenInclude(h => h.Detalles)
                .FirstOrDefaultAsync(o => o.Id == OdontologoId);

            if (odontologo == null)
            {
                TempData["ErrorMessage"] = "No se encontró el odontólogo.";
                return RedirectToPage();
            }

            // Actualizar los datos del odontólogo
            odontologo.Nombre = nombre;
            odontologo.Apellido = apellido;
            odontologo.Especialidad = especialidad;

            // Actualizar correo electrónico y contraseña en el usuario asociado
            var user = await _userManager.FindByIdAsync(odontologo.UserId);
            if (user != null)
            {
                user.Email = email;
                user.UserName = email;

                if (!string.IsNullOrEmpty(password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, password);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        TempData["ErrorMessage"] = "Error al actualizar la contraseña.";
                        return RedirectToPage();
                    }
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    TempData["ErrorMessage"] = "Error al actualizar los datos del usuario.";
                    return RedirectToPage();
                }
            }

            // Actualizar o agregar horarios si se proporcionan
            if (fechaInicio.HasValue && fechaFin.HasValue && horaInicio.HasValue && horaFin.HasValue)
            {
                var horarios = odontologo.OdontologoHorarios.FirstOrDefault();
                if (horarios != null)
                {
                    var detalleHorario = horarios.Detalles.FirstOrDefault(d => d.FechaInicio == fechaInicio.Value && d.FechaFin == fechaFin.Value);
                    if (detalleHorario != null)
                    {
                        detalleHorario.HoraInicio = horaInicio.Value;
                        detalleHorario.HoraFin = horaFin.Value;
                    }
                    else
                    {
                        horarios.Detalles.Add(new OdontologoHorarioDetalle
                        {
                            OdontologoId = odontologo.Id,
                            DiaSemana = fechaInicio.Value.DayOfWeek,
                            HoraInicio = horaInicio.Value,
                            HoraFin = horaFin.Value,
                            FechaInicio = fechaInicio.Value,
                            FechaFin = fechaFin.Value
                        });
                    }
                }
            }

            // Guardar los cambios en el odontólogo
            _context.Odontologos.Update(odontologo);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Los cambios se guardaron correctamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // Buscar el odontólogo por ID
            var odontologo = await _context.Odontologos
                .Include(o => o.OdontologoHorarios)
                    .ThenInclude(h => h.Detalles)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (odontologo == null)
            {
                return NotFound();
            }

      
            var user = await _userManager.FindByIdAsync(odontologo.UserId);
            if (user != null)
            {
                // Eliminar al usuario del sistema
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = "Error al eliminar el usuario asociado al odontólogo.";
                    return RedirectToPage();
                }
            }

            // Eliminar los horarios asociados al odontólogo
            var horarios = odontologo.OdontologoHorarios;
            if (horarios != null)
            {
                _context.OdontologosHorarios.RemoveRange(horarios);
            }

            // Eliminar el odontólogo
            _context.Odontologos.Remove(odontologo);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "El odontólogo y todos los datos relacionados fueron eliminados correctamente.";
            return RedirectToPage("/ManageOdontologos");
        }

        public async Task<IActionResult> OnPostEditHorarioAsync()
        {
            var detalle = await _context.OdontologosHorariosDetalles.FindAsync(HorarioDetalleId);
            if (detalle == null)
            {
                TempData["ErrorMessage"] = "No se encontró el horario.";
                return RedirectToPage();
            }

            detalle.FechaInicio = FechaInicio;
            detalle.FechaFin = FechaFin;
            detalle.HoraInicio = HoraInicio;
            detalle.HoraFin = HoraFin;

            _context.OdontologosHorariosDetalles.Update(detalle);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Horario actualizado correctamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteHorarioAsync(int id)
        {
            var detalle = await _context.OdontologosHorariosDetalles.FindAsync(id);
            if (detalle == null)
            {
                TempData["ErrorMessage"] = "No se encontró el horario.";
                return RedirectToPage();
            }

            _context.OdontologosHorariosDetalles.Remove(detalle);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Horario eliminado correctamente.";
            return RedirectToPage();
        }

        public string GetSpanishDayName(DayOfWeek dayOfWeek)
        {
            // Crea una cultura española para obtener los nombres de los días
            var culture = new CultureInfo("es-ES");
            return culture.DateTimeFormat.GetDayName(dayOfWeek);
        }
    }
}
