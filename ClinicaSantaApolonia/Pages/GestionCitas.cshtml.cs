using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicaSantaApolonia.Pages
{
    public class ManageAppointmentsModel : PageModel
    {
        private readonly ClinicaDentalContext _context;

        public ManageAppointmentsModel(ClinicaDentalContext context)
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
        public List<Odontologo> Odontologos { get; set; }
        public List<NotasTratamiento> NotasTratamientos { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();

        public string ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            Pacientes = await _context.Pacientes.ToListAsync();
            Odontologos = await _context.Odontologos.ToListAsync();
            NotasTratamientos = await _context.NotasTratamientos.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ValidationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
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
                ValidationErrors.Add("El odontólogo no está disponible en el horario seleccionado.");
                return Page();
            }

            // Recuperar todas las citas del odontólogo para el día seleccionado
            var citasDelDia = await _context.Citas
                .Where(c => c.OdontologoId == OdontologoId && c.FechaHora.Date == FechaHora.Date)
                .ToListAsync();

            // Verificar si hay solapamientos con el intervalo de una hora
            var haySolapamiento = citasDelDia.Any(cita =>
            {
                var citaFin = cita.FechaHora.TimeOfDay.Add(new TimeSpan(1, 0, 0));
                return (cita.FechaHora.TimeOfDay < FechaHora.TimeOfDay.Add(new TimeSpan(1, 0, 0)) && citaFin > FechaHora.TimeOfDay);
            });

            if (haySolapamiento)
            {
                ValidationErrors.Add("El horario seleccionado ya está ocupado o no permite el intervalo de una hora.");
                return Page();
            }

            var cita = new Cita
            {
                PacienteId = PacienteId,
                OdontologoId = OdontologoId,
                NotasTratamientoId = TipoTratamientoId,
                FechaHora = FechaHora,
                Notas = Notas,
                HorarioId = horarioDisponible.Id // Asignar el ID del horario disponible
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            return RedirectToPage("/GestionCitas"); // Redirige a una página de éxito o lista de citas
        }



        public string GetSpanishDayName(DayOfWeek dayOfWeek)
        {
            // Crea una cultura española para obtener los nombres de los días
            var culture = new CultureInfo("es-ES");
            return culture.DateTimeFormat.GetDayName(dayOfWeek);
        }
    }
}
