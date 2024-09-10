using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;


namespace ClinicaSantaApolonia.Areas.Identity.Pages.Account// Reemplaza con el espacio de nombres correcto
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly ClinicaDentalContext _context;
        private readonly IEmailSender _emailSender; // Asegúrate de tener un servicio para enviar correos

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            ClinicaDentalContext context,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            public string Nombre { get; set; }

            [Required]
            public string Apellido { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Telefono { get; set; }

            [Required]
            public string Direccion { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "La contraseña y la contraseña de comfirmación no coinciden.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Asignar rol de paciente
                    await _userManager.AddToRoleAsync(user, "Paciente");

                    // Guardar información del paciente en la base de datos
                    var paciente = new Paciente
                    {
                        Nombre = Input.Nombre,
                        Apellido = Input.Apellido,
                        Email = Input.Email,
                        Telefono = Input.Telefono,
                        Direccion = Input.Direccion,
                        UserId = user.Id // Asignar UserId del usuario creado
                    };

                    _context.Pacientes.Add(paciente);
                    await _context.SaveChangesAsync();

                    // Redirigir según el rol
                    if (await _userManager.IsInRoleAsync(user, "Paciente"))
                    {
                        return RedirectToPage("/MisCitas"); // Ruta de la página para crear citas
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}
