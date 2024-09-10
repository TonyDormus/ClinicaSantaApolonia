using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ClinicaSantaApolonia.Pages
{
    public class CitasOdontologoModel : PageModel
    {
        private readonly ClinicaDentalContext _context;

        public CitasOdontologoModel(ClinicaDentalContext context)
        {
            _context = context;
        }

        public IList<Cita> Citas { get; set; }
        public string ErrorMessage { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("No se pudo determinar el usuario actual.");
            }

            var odontologo = await _context.Odontologos
                .Where(o => o.UserId == userId)
                .FirstOrDefaultAsync();

            if (odontologo == null)
            {
                return NotFound("No se encontró un odontólogo para el usuario actual.");
            }

            Citas = await _context.Citas
                .Where(c => c.OdontologoId == odontologo.Id)
                .Include(c => c.Paciente)
                .Include(c => c.TipoTratamiento)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var cita = await _context.Citas.FindAsync(id);

            if (cita == null)
            {
                return NotFound();
            }

            _context.Citas.Remove(cita);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var cita = await _context.Citas.FindAsync(int.Parse(Request.Form["CitaId"]));

            if (cita == null)
            {
                ErrorMessage = "Cita no encontrada.";
                return Page();
            }

            var fechaHora = DateTime.Parse(Request.Form["FechaHora"]);

            // Validaciones de fecha y hora
            if (fechaHora.DayOfWeek == DayOfWeek.Sunday)
            {
                ErrorMessage = "Solo se permiten citas de lunes a sábado.";
                return Page();
            }

            if ((fechaHora.DayOfWeek == DayOfWeek.Saturday && (fechaHora.Hour < 8 || fechaHora.Hour >= 12)) ||
                (fechaHora.DayOfWeek != DayOfWeek.Saturday && (fechaHora.Hour < 8 || fechaHora.Hour >= 17)))
            {
                ErrorMessage = "Las citas solo se permiten entre las 8:00 y las 17:00 de lunes a viernes y entre las 8:00 y las 12:00 los sábados.";
                return Page();
            }

            var horarioDisponible = await _context.OdontologosHorariosDetalles
                .Where(hd => hd.Horario.OdontologoId == cita.OdontologoId &&
                             hd.DiaSemana == fechaHora.DayOfWeek &&
                             fechaHora.TimeOfDay >= hd.HoraInicio &&
                             fechaHora.TimeOfDay <= hd.HoraFin)
                .FirstOrDefaultAsync();

            if (horarioDisponible == null)
            {
                ErrorMessage = "El odontólogo no está disponible en el horario seleccionado.";
                return Page();
            }

            var citasDelDia = await _context.Citas
                .Where(c => c.OdontologoId == cita.OdontologoId && c.FechaHora.Date == fechaHora.Date && c.Id != cita.Id) // Ignora la cita actual
                .ToListAsync();

            var haySolapamiento = citasDelDia.Any(existingCita =>
            {
                var citaFin = existingCita.FechaHora.TimeOfDay.Add(new TimeSpan(1, 0, 0));
                return (existingCita.FechaHora.TimeOfDay < fechaHora.TimeOfDay.Add(new TimeSpan(1, 0, 0)) && citaFin > fechaHora.TimeOfDay);
            });

            if (haySolapamiento)
            {
                ErrorMessage = "El horario seleccionado ya está ocupado o no permite el intervalo de una hora.";
                return Page();
            }

            cita.FechaHora = fechaHora;
            cita.Estado = Request.Form["Estado"];
            cita.Notas = Request.Form["Notas"];

            _context.Citas.Update(cita);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Los cambios se guardaron correctamente.";
            return RedirectToPage();
        }


    }
}
