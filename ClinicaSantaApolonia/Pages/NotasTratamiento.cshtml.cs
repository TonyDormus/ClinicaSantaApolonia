using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ClinicaSantaApolonia.Pages
{
    public class NotasTratamientoModel : PageModel
    {
        private readonly ClinicaDentalContext _context;

        public NotasTratamientoModel(ClinicaDentalContext context)
        {
            _context = context;
            // Inicializar NuevoTratamiento para evitar que sea null
            NuevoTratamiento = new NotasTratamiento();
        }

        public List<NotasTratamiento> Tratamientos { get; set; }

        [BindProperty]
        public NotasTratamiento NuevoTratamiento { get; set; } = new NotasTratamiento(); // Aquí no debería haber error

        [BindProperty(SupportsGet = true)]
        public int TratamientoId { get; set; }

        public async Task OnGetAsync()
        {
            Tratamientos = await _context.NotasTratamientos.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.NotasTratamientos.Add(NuevoTratamiento);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var tratamiento = await _context.NotasTratamientos.FindAsync(TratamientoId);
            if (tratamiento == null)
            {
                return NotFound();
            }

            tratamiento.Nombre = NuevoTratamiento.Nombre;
            tratamiento.Precio = NuevoTratamiento.Precio;

            _context.NotasTratamientos.Update(tratamiento);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Los cambios se guardaron correctamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var tratamiento = await _context.NotasTratamientos.FindAsync(TratamientoId);
            if (tratamiento == null)
            {
                return NotFound();
            }

            _context.NotasTratamientos.Remove(tratamiento);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
