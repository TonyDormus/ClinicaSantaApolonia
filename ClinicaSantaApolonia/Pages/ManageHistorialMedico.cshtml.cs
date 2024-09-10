using ClinicaSantaApolonia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicaSantaApolonia.Pages
{
    public class ManageHistorialMedicoModel : PageModel
    {
        private readonly ClinicaDentalContext _context;

        public ManageHistorialMedicoModel(ClinicaDentalContext context)
        {
            _context = context;
        }

        public List<HistorialMedico> HistorialesMedicos { get; set; } = new List<HistorialMedico>();
        public List<Paciente> Pacientes { get; set; } = new List<Paciente>();

        [BindProperty(SupportsGet = true)]
        public int HistorialId { get; set; }

        public async Task OnGetAsync()
        {
            HistorialesMedicos = await _context.HistorialesMedicos.Include(h => h.Paciente).ToListAsync();
            Pacientes = await _context.Pacientes.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(int pacienteId, string antecedentesMedicos, string tratamientosPrevios, string alergias, string notasAdicionales)
        {
            if (ModelState.IsValid)
            {
                var historialMedico = new HistorialMedico
                {
                    PacienteId = pacienteId,
                    AntecedentesMedicos = antecedentesMedicos,
                    TratamientosPrevios = tratamientosPrevios,
                    Alergias = alergias,
                    NotasAdicionales = notasAdicionales
                };

                _context.HistorialesMedicos.Add(historialMedico);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Historial médico guardado correctamente.";
                return RedirectToPage();
            }

            TempData["ErrorMessage"] = "Error al guardar el historial médico.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int historialId, int pacienteId, string antecedentesMedicos, string tratamientosPrevios, string alergias, string notasAdicionales)
        {
            var historialMedico = await _context.HistorialesMedicos.FindAsync(historialId);
            if (historialMedico == null)
            {
                TempData["ErrorMessage"] = "No se encontró el historial médico.";
                return RedirectToPage();
            }

            historialMedico.PacienteId = pacienteId;
            historialMedico.AntecedentesMedicos = antecedentesMedicos;
            historialMedico.TratamientosPrevios = tratamientosPrevios;
            historialMedico.Alergias = alergias;
            historialMedico.NotasAdicionales = notasAdicionales;

            _context.HistorialesMedicos.Update(historialMedico);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Los cambios en el historial médico se guardaron correctamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var historialMedico = await _context.HistorialesMedicos.FindAsync(id);
            if (historialMedico == null)
            {
                TempData["ErrorMessage"] = "No se encontró el historial médico.";
                return RedirectToPage();
            }

            _context.HistorialesMedicos.Remove(historialMedico);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Historial médico eliminado correctamente.";
            return RedirectToPage();
        }
    }
}
