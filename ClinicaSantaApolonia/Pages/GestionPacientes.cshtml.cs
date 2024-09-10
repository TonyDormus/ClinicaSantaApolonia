using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ClinicaSantaApolonia.Pages
{
    public class ManagePatientsModel : PageModel
    {
        private readonly ClinicaDentalContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ManagePatientsModel(ClinicaDentalContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Paciente> Patients { get; set; } = new List<Paciente>();

        [BindProperty(SupportsGet = true)]
        public int PacienteId { get; set; }

        public async Task OnGetAsync()
        {
            Patients = await _context.Pacientes.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(string nombre, string apellido, string email, string telefono, string direccion, string contrasena)
        {
            if (ModelState.IsValid)
            {
                // Verifica si el paciente ya existe
                var existingPaciente = await _context.Pacientes
                    .Where(p => p.Nombre == nombre && p.Apellido == apellido && p.Email == email)
                    .FirstOrDefaultAsync();

                if (existingPaciente != null)
                {
                    ModelState.AddModelError(string.Empty, "Este paciente ya está registrado.");
                }
                else
                {
                    // Verifica si el email ya está registrado en el sistema
                    var existingUser = await _userManager.FindByEmailAsync(email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError(string.Empty, "El email ya está registrado.");
                    }
                    else
                    {
                        // Crea un nuevo usuario de identidad
                        var user = new IdentityUser
                        {
                            UserName = email,
                            Email = email
                        };

                        // Crea el usuario en la base de datos con la contraseña proporcionada
                        var result = await _userManager.CreateAsync(user, contrasena);

                        if (result.Succeeded)
                        {
                            // Asigna el rol "Paciente" al nuevo usuario
                            await _userManager.AddToRoleAsync(user, "Paciente");

                            // Crea un nuevo paciente asociado con el usuario
                            var paciente = new Paciente
                            {
                                Nombre = nombre,
                                Apellido = apellido,
                                Email = email,
                                Telefono = telefono,
                                Direccion = direccion,
                                UserId = user.Id // Asigna el ID del nuevo usuario
                            };

                            _context.Pacientes.Add(paciente);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                }
            }



            // Recargar la lista de pacientes
            Patients = await _context.Pacientes.ToListAsync();

            // Retornar la vista con el estado actualizado
            return Page();
        }

        public async Task<IActionResult> OnPostEditAsync(string nombre, string apellido, string email, string telefono, string direccion, string contrasena)
        {
            var paciente = await _context.Pacientes.FindAsync(PacienteId);
            if (paciente == null)
            {
                TempData["ErrorMessage"] = "No se encontró el paciente.";
                return RedirectToPage();
            }

            // Actualizar los datos del paciente
            paciente.Nombre = nombre;
            paciente.Apellido = apellido;
            paciente.Email = email;
            paciente.Telefono = telefono;
            paciente.Direccion = direccion;

            // Obtener el usuario asociado a este paciente
            var user = await _userManager.FindByIdAsync(paciente.UserId);
            if (user != null)
            {
                // Actualizar el correo electrónico y el nombre de usuario (que es el mismo que el correo)
                user.Email = email;
                user.UserName = email;

                // Actualizar la contraseña solo si se proporciona una nueva
                if (!string.IsNullOrEmpty(contrasena))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, contrasena);
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

                // Guardar los cambios en el usuario
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

            // Guardar los cambios en el paciente
            _context.Pacientes.Update(paciente);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Los cambios se guardaron correctamente.";
            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // Buscar el paciente por ID e incluir las entidades relacionadas
            var paciente = await _context.Pacientes
           
                .FirstOrDefaultAsync(p => p.Id == id);

            if (paciente == null)
            {
                return NotFound();
            }

            // Buscar el usuario asociado en IdentityUser
            var user = await _userManager.FindByIdAsync(paciente.UserId);
            if (user != null)
            {
                // Eliminar el usuario asociado del sistema de identidad
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = "Error al eliminar el usuario asociado al paciente.";
                    return RedirectToPage();
                }
            }

        

            // Eliminar el paciente
            _context.Pacientes.Remove(paciente);
            await _context.SaveChangesAsync();

            // Redirigir a la página de gestión de pacientes
            return RedirectToPage("/GestionPacientes");
        }


    }
}
