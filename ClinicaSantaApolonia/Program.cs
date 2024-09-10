using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddDbContext<ClinicaDentalContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Agregar logging a los servicios
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false) // Cambiar a false para no requerir confirmación de email
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ClinicaDentalContext>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/MisCitas", "Paciente"); // Configura la política aquí
});

// Agrega políticas de autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Paciente", policy =>
        policy.RequireRole("Paciente")); // Asegúrate de que esta política esté configurada
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/VerCitas", "Odontologo"); // Configura la política aquí
});

// Agrega políticas de autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Odontologo", policy =>
        policy.RequireRole("Odontologo")); // Asegúrate de que esta política esté configurada
});

var app = builder.Build();

// Configuración de la aplicación
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Configuración para redirigir la página de inicio a la página de inicio de sesión
app.MapGet("/", async context =>
{
    context.Response.Redirect("/Identity/Account/Login"); // Cambia la ruta según tu estructura
});

app.MapRazorPages();

// Crear roles y usuarios
async Task CreateRolesAndUsers(IServiceScope serviceScope)
{
    var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    string[] roles = { "Administrador", "Recepcionista", "Odontologo", "Paciente" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Administrador
    var adminUser = new IdentityUser
    {
        UserName = "admin@clinicadental.com",
        Email = "admin@clinicadental.com",
        EmailConfirmed = true
    };

    string adminPassword = "Admin@123";

    var existingAdminUser = await userManager.FindByEmailAsync(adminUser.Email);

    if (existingAdminUser == null)
    {
        var createAdminUser = await userManager.CreateAsync(adminUser, adminPassword);
        if (createAdminUser.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrador");
        }
    }

    // Recepcionista
    var recepUser = new IdentityUser
    {
        UserName = "recep@clinicadental.com",
        Email = "recep@clinicadental.com",
        EmailConfirmed = true
    };

    string recepPassword = "Recep@123";

    var re = await userManager.FindByEmailAsync(recepUser.Email);

    if (re == null)
    {
        var createUser = await userManager.CreateAsync(recepUser, recepPassword);
        if (createUser.Succeeded)
        {
            await userManager.AddToRoleAsync(recepUser, "Recepcionista");
        }
    }

}

// Llamar al método para crear roles y usuarios
using (var serviceScope = app.Services.CreateScope())
{
    await CreateRolesAndUsers(serviceScope);
}

app.Run();
