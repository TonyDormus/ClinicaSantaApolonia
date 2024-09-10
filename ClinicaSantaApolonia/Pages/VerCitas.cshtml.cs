using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ClinicaSantaApolonia.Pages
{
    public class VerCitasModel : PageModel
    {
        private readonly ClinicaDentalContext _context;
        private readonly ILogger<VerCitasModel> _logger;

        public VerCitasModel(ClinicaDentalContext context, ILogger<VerCitasModel> logger)
        {
            _context = context;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IList<Cita> Citas { get; set; }

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Registrar el userId para depuración
            _logger.LogInformation("UserId: {UserId}", userId);

            // Obtener el Id del odontólogo utilizando el UserId
            var odontologoId = await _context.Odontologos
                .Where(o => o.UserId == userId)
                .Select(o => o.Id)
                .FirstOrDefaultAsync();

            if (odontologoId == 0)
            {
                _logger.LogWarning("No se encontró un odontólogo para el UserId: {UserId}", userId);
                Citas = new List<Cita>();
                return;
            }

            // Consultar las citas del odontólogo
            Citas = await _context.Citas
                .Where(c => c.OdontologoId == odontologoId)
                .Include(c => c.Paciente)
                .ToListAsync();

            if (Citas == null || !Citas.Any())
            {
                _logger.LogInformation("No se encontraron citas para el odontólogo con Id: {OdontologoId}", odontologoId);
            }
        }
    }
}
