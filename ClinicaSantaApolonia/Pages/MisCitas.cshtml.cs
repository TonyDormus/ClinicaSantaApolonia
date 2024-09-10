using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ClinicaSantaApolonia.Pages
{
    public class MisCitasModel : PageModel
    {
        private readonly ClinicaDentalContext _context;

        public MisCitasModel(ClinicaDentalContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int PacienteId { get; set; }

        [BindProperty]
        public int OdontologoId { get; set; }

        [BindProperty]
        public int TipoTratamientoId { get; set; }

        [BindProperty]
        public DateTime FechaHora { get; set; }

        [BindProperty]
        public string Notas { get; set; }

        [BindProperty]
        public int HorarioId { get; set; } // Agregado para manejar el HorarioId

        public List<Paciente> Pacientes { get; set; }
        public IList<Cita> Citas { get; set; }
        public List<Odontologo> Odontologos { get; set; }
        public List<NotasTratamiento> NotasTratamientos { get; set; }
        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            PacienteId = await _context.Pacientes
                .Where(p => p.UserId == userId)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            Citas = await _context.Citas
                .Where(c => c.PacienteId == PacienteId)
                .Include(c => c.Odontologo)
                .Include(c => c.TipoTratamiento)
                .ToListAsync();

            Odontologos = await _context.Odontologos.ToListAsync();
            NotasTratamientos = await _context.NotasTratamientos.ToListAsync() ?? new List<NotasTratamiento>();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            PacienteId = await _context.Pacientes
                .Where(p => p.UserId == userId)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (!ModelState.IsValid)
            {
                // Asignar el precio del tratamiento seleccionado
                var tratamiento = await _context.NotasTratamientos.FindAsync(TipoTratamientoId);
                if (tratamiento != null)
                {
                    ViewData["Precio"] = tratamiento.Precio;
                }
                return Page();
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Error en los datos proporcionados.";
                return Page();
            }

            // Verificar disponibilidad del horario
            var horarioDisponible = await _context.OdontologosHorariosDetalles
                .Where(hd => hd.Horario.OdontologoId == OdontologoId &&
                             hd.DiaSemana == FechaHora.DayOfWeek &&
                             FechaHora.TimeOfDay >= hd.HoraInicio &&
                             FechaHora.TimeOfDay <= hd.HoraFin)
                .FirstOrDefaultAsync();

            if (FechaHora.DayOfWeek == DayOfWeek.Sunday)
            {
                ErrorMessage = "Solo se permiten citas de lunes a sábado.";
                return Page();
            }

            if ((FechaHora.DayOfWeek == DayOfWeek.Saturday && (FechaHora.Hour < 8 || FechaHora.Hour >= 12)) ||
                (FechaHora.DayOfWeek != DayOfWeek.Saturday && (FechaHora.Hour < 8 || FechaHora.Hour >= 17)))
            {
                ErrorMessage = "Las citas solo se permiten entre las 8:00 y las 17:00 de lunes a viernes y entre las 8:00 y las 12:00 los sábados.";
                return Page();
            }

            if (horarioDisponible == null)
            {
                ErrorMessage = "El odontólogo no está disponible en el horario o Fecha seleccionado.";
                return Page();
            }

            var citasDelDia = await _context.Citas
                .Where(c => c.OdontologoId == OdontologoId && c.FechaHora.Date == FechaHora.Date)
                .ToListAsync();

            var haySolapamiento = citasDelDia.Any(cita =>
            {
                var citaFin = cita.FechaHora.TimeOfDay.Add(new TimeSpan(1, 0, 0));
                return (cita.FechaHora.TimeOfDay < FechaHora.TimeOfDay.Add(new TimeSpan(1, 0, 0)) && citaFin > FechaHora.TimeOfDay);
            });

            if (haySolapamiento)
            {
                ErrorMessage = "El horario seleccionado ya está ocupado o no permite el intervalo de una hora.";
                return Page();
            }

            var tipoTratamiento = await _context.NotasTratamientos.FindAsync(TipoTratamientoId);

            if (tipoTratamiento == null)
            {
                ErrorMessage = "Tipo de tratamiento no encontrado.";
                return Page();
            }

            var cita = new Cita
            {
                PacienteId = PacienteId,
                OdontologoId = OdontologoId,
                NotasTratamientoId = TipoTratamientoId,
                Precio = tipoTratamiento.Precio,
                FechaHora = FechaHora,
                Notas = Notas,
                HorarioId = horarioDisponible.Id,
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Los cambios se guardaron correctamente.";
            return RedirectToPage("/MisCitas");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null)
            {
                ErrorMessage = "Cita no encontrada.";
                return Page();
            }

            _context.Citas.Remove(cita);
            await _context.SaveChangesAsync();

            return RedirectToPage("/MisCitas");
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
            var tipoTratamientoId = int.Parse(Request.Form["TipoTratamientoId"]);

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

            var tipoTratamiento = await _context.NotasTratamientos.FindAsync(tipoTratamientoId);
            if (tipoTratamiento == null)
            {
                ErrorMessage = "Tipo de tratamiento no encontrado.";
                return Page();
            }

            cita.FechaHora = fechaHora;
            cita.Notas = Request.Form["Notas"];
            cita.NotasTratamientoId = tipoTratamientoId;
            cita.Precio = tipoTratamiento.Precio;

            try
            {
                _context.Citas.Update(cita);
                await _context.SaveChangesAsync();
                return RedirectToPage();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CitaExists(cita.Id))
                {
                    ErrorMessage = "Error al guardar los cambios.";
                }
                else
                {
                    throw;
                }
            }

            return Page();
        }

        private bool CitaExists(int id)
        {
            return _context.Citas.Any(c => c.Id == id);
        }

        public string GetSpanishDayName(DayOfWeek dayOfWeek)
        {
            var culture = new CultureInfo("es-ES");
            return culture.DateTimeFormat.GetDayName(dayOfWeek);
        }
    }
}
