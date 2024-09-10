using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicaSantaApolonia.Pages
{
    public class FacturaModel : PageModel
    {
        private readonly ClinicaDentalContext _context;

        public FacturaModel(ClinicaDentalContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int CitaId { get; set; }
        [BindProperty]
        public int OdontologoId { get; set; }
        [BindProperty]
        public int PacienteId { get; set; } // Añade esta línea
        [BindProperty]
        public decimal Monto { get; set; }
        [BindProperty]
        public decimal? Descuento { get; set; }
        [BindProperty]
        public decimal CantidadPagada { get; set; }
        [BindProperty]
        public decimal CantidadDevuelta { get; set; }

        public List<Cita> Citas { get; set; }
        public List<Factura> Facturas { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();

        public async Task OnGetAsync()
        {
            Citas = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Odontologo)
                .Include(c => c.TipoTratamiento)
                .ToListAsync();

            Facturas = await _context.Facturas
                .Include(f => f.Cita)
                    .ThenInclude(c => c.Paciente)
                .Include(f => f.Cita)
                    .ThenInclude(c => c.Odontologo)
                .Include(f => f.Cita)
                    .ThenInclude(c => c.TipoTratamiento)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ValidationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Page();
            }

            var cita = await _context.Citas.FindAsync(CitaId);
            if (cita == null)
            {
                ValidationErrors.Add("Cita no encontrada.");
                return Page();
            }

            var factura = new Factura
            {
                CitaId = CitaId,
                OdontologoId = OdontologoId,
                PacienteId = PacienteId, // Asegúrate de que esto se establezca correctamente
                PrecioTratamiento = Monto,
                Descuento = Descuento,
                CantidadPagada = CantidadPagada,
                MontoDevuelto = CantidadDevuelta,
            };

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura == null)
            {
                return NotFound();
            }

            _context.Facturas.Remove(factura);
            await _context.SaveChangesAsync();

            return RedirectToPage(); // Redirige a la misma página para actualizar la lista de facturas
        }

    }



}
