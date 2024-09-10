using ClinicaSantaApolonia.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicaSantaApolonia.Pages
{
    public class ManageRolesModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ClinicaDentalContext _context;

        // Diccionario de páginas asignadas por rol
        private readonly Dictionary<string, List<string>> _paginasPorRol = new Dictionary<string, List<string>>
        {
            { "Administrador", new List<string> { "ManageRoles", "GestionUsuarios", "ManageOdontologos", "NotasTratamiento" } },
            { "Recepcionista", new List<string> { "GestionPacientes", "GestionCitas", "Factura", "ManageHistorialMedico" } },
            { "Odontologo", new List<string> { "CitasOdontologo" } },
            { "Paciente", new List<string> { "MisCitas" } }
        };

        public ManageRolesModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ClinicaDentalContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public List<IdentityRole> Roles { get; set; } = new List<IdentityRole>();
        public List<string> PaginasDisponibles { get; set; } = new List<string>(); // Lista de páginas disponibles
        public List<string> PermisosAsignados { get; set; } = new List<string>(); // Permisos asignados al rol actual

        public async Task OnGetAsync(string id = null)
        {
            Roles = new List<IdentityRole>(await _roleManager.Roles.ToListAsync());

            // Páginas disponibles en la aplicación
            PaginasDisponibles = new List<string>
    {
        "CitasOdontologo",
        "Factura",
        "GestionCitas",
        "GestionPacientes",
        "GestionUsuarios",
        "ManageHistorialMedico",
        "ManageOdontologos",
        "ManageRoles",
        "MisCitas",
        "NotasTratamiento",
        "VerCitas"
    };

            if (!string.IsNullOrEmpty(id))
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role != null)
                {
                    // Cargar los permisos desde la base de datos, combinando con los predefinidos
                    PermisosAsignados = await _context.PermisosRoles
                        .Where(p => p.RolId == id && p.TieneAcceso)
                        .Select(p => p.NombrePagina)
                        .ToListAsync();

                    if (_paginasPorRol.ContainsKey(role.Name))
                    {
                        var paginasPredefinidas = _paginasPorRol[role.Name];
                        PermisosAsignados = PermisosAsignados.Union(paginasPredefinidas).ToList();
                    }
                }
            }
        }


        public async Task<IActionResult> OnPostEditAsync(string id, string roleName, List<string> permisos)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["ErrorMessage"] = "No se encontró el rol.";
                return RedirectToPage();
            }

            role.Name = roleName;
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                // Eliminar los permisos actuales del rol en la base de datos
                var permisosActuales = await _context.PermisosRoles.Where(p => p.RolId == role.Id).ToListAsync();
                _context.PermisosRoles.RemoveRange(permisosActuales);

                // Agregar los permisos nuevos
                foreach (var permiso in permisos)
                {
                    _context.PermisosRoles.Add(new PermisoRol
                    {
                        RolId = role.Id,
                        NombrePagina = permiso,
                        TieneAcceso = true
                    });
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Los cambios se guardaron correctamente.";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            TempData["ErrorMessage"] = "Error al actualizar el rol.";
            return RedirectToPage();
        }

        // Método para crear un nuevo rol
        public async Task<IActionResult> OnPostAsync(string roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    return RedirectToPage();
                }
            }
            return Page();
        }

        // Método para eliminar un rol
        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["ErrorMessage"] = "No se encontró el rol.";
                return RedirectToPage();
            }

            // Primero, eliminamos todas las asociaciones de usuarios a este rol
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            foreach (var user in usersInRole)
            {
                var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = "Error al eliminar el rol de los usuarios.";
                    return RedirectToPage();
                }
            }

            // Intentar eliminar el rol
            var deleteResult = await _roleManager.DeleteAsync(role);
            if (deleteResult.Succeeded)
            {
                TempData["SuccessMessage"] = "El rol fue eliminado correctamente y se actualizó la información de los usuarios.";
                return RedirectToPage();
            }

            TempData["ErrorMessage"] = "Error al eliminar el rol. Asegúrate de que no está siendo usado.";
            return RedirectToPage();
        }
    }
}
