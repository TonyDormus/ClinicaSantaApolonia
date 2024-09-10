using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicaSantaApolonia.Pages
{
    public class GestionUsuarioModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ClinicaDentalContext _context;

        public GestionUsuarioModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ClinicaDentalContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public List<IdentityUser> Users { get; set; } = new List<IdentityUser>(); // Inicializar como lista vacía
        public Dictionary<string, List<string>> UserRoles { get; set; } = new Dictionary<string, List<string>>(); // Inicializar como diccionario vacío
        public List<string> Roles { get; set; } = new List<string>(); // Inicializar como lista vacía

        public async Task OnGetAsync()
        {
            Users = await _userManager.Users.ToListAsync();

            UserRoles = new Dictionary<string, List<string>>();
            foreach (var user in Users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                UserRoles[user.Id] = roles.ToList();
            }

            Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(string userName, string email, string password, string[] selectedRoles)
        {
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                var user = new IdentityUser
                {
                    UserName = userName,
                    Email = email
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    if (selectedRoles != null)
                    {
                        foreach (var role in selectedRoles)
                        {
                            if (await _roleManager.RoleExistsAsync(role))
                            {
                                await _userManager.AddToRoleAsync(user, role);
                            }
                        }
                    }

                    return RedirectToPage();
                }
            }

            // Si llegamos aquí, hubo un error en la creación del usuario, redibujar el formulario
            return Page();
        }

        // Método para editar un usuario
        public async Task<IActionResult> OnPostEditAsync(string id, string userName, string email, string password)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "No se encontró el usuario.";
                return RedirectToPage();
            }

            var roles = await _userManager.GetRolesAsync(user);

            // No permitir cambiar roles si es Paciente o Odontologo
            if (!roles.Contains("Paciente") && !roles.Contains("Odontologo"))
            {
                // Aquí podrías agregar lógica para cambiar roles si se desea, pero se omitirá según tu requerimiento.
            }

            // Actualizar los datos del usuario
            user.UserName = userName;
            user.Email = email;

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

            TempData["SuccessMessage"] = "Los cambios se guardaron correctamente.";
            return RedirectToPage();
        }

        // Método para eliminar un usuario
        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "No se encontró el usuario.";
                return RedirectToPage();
            }

            // Eliminar todos los datos relacionados con el usuario en otras tablas
            var odontologo = await _context.Odontologos.FirstOrDefaultAsync(o => o.UserId == id);
            if (odontologo != null)
            {
                _context.Odontologos.Remove(odontologo);
            }

            var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.UserId == id);
            if (paciente != null)
            {
                _context.Pacientes.Remove(paciente);
            }

            var citas = await _context.Citas.Where(c => c.Odontologo.UserId == id || c.Paciente.UserId == id).ToListAsync();
            if (citas.Any())
            {
                _context.Citas.RemoveRange(citas);
            }

            await _context.SaveChangesAsync();

            // Eliminar el usuario de la base de datos de Identity
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Error al eliminar el usuario.";
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "El usuario y todos los datos relacionados fueron eliminados correctamente.";
            return RedirectToPage();
        }
    }
}
