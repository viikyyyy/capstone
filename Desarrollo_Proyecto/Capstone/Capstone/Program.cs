using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditoriaServicio>();

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
         options.LoginPath = "/SelectorIngreso";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });
builder.Services.AddRazorPages();

var app = builder.Build();

await CrearTablaRecuperacionAsync(app);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

await SembrarUsuariosPruebaAsync(app);

app.Run();

static async Task SembrarUsuariosPruebaAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = new PasswordHasher<Usuario>();

    var usuarios = new[]
    {
        new
        {
            Rut = "11111111", Dv = "1", Nombre = "Benjamin", Apellido = "Veliz",
            Correo = "medico.prueba@lifeguardbup.cl", Contrasena = "Medico123!", Rol = "medico"
        },
        new
        {
            Rut = "22222222", Dv = "2", Nombre = "Paciente", Apellido = "Prueba",
            Correo = "paciente.prueba@lifeguardbup.cl", Contrasena = "Paciente123!", Rol = "paciente"
        },
        new
        {
            Rut = "33333333", Dv = "4", Nombre = "Administrativo", Apellido = "Prueba",
            Correo = "administrativo.prueba@lifeguardbup.cl", Contrasena = "Admin123!", Rol = "administrativo"
        }
    };

    foreach (var datos in usuarios)
    {
        var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Correo == datos.Correo);
        if (usuario is null)
        {
            usuario = new Usuario
            {
                Rut = datos.Rut,
                DvRut = datos.Dv,
                Pnombre = datos.Nombre,
                Apellidop = datos.Apellido,
                Correo = datos.Correo,
                Rol = datos.Rol,
                Estado = "activo",
                FechaCreacion = HoraChile.Ahora
            };
            usuario.ContrasenaHash = passwordHasher.HashPassword(usuario, datos.Contrasena);
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
        }

        if (datos.Rol == "paciente" && !await context.Pacientes.AnyAsync(p => p.IdUsuario == usuario.IdUsuario))
        {
            context.Pacientes.Add(new Paciente
            {
                IdUsuario = usuario.IdUsuario,
                Rut = datos.Rut,
                DvRut = datos.Dv,
                Pnombre = datos.Nombre,
                Apellidop = datos.Apellido,
                FechaNacimiento = new DateTime(2000, 1, 1)
            });
            await context.SaveChangesAsync();
        }
    }
}

static async Task CrearTablaRecuperacionAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.ExecuteSqlRawAsync("IF OBJECT_ID('codigo_recuperacion', 'U') IS NULL CREATE TABLE codigo_recuperacion (id_codigo INT IDENTITY(1,1) NOT NULL PRIMARY KEY, id_usuario INT NOT NULL, codigo VARCHAR(20) NOT NULL, expiracion DATETIME2 NOT NULL, usado BIT NOT NULL DEFAULT 0, CONSTRAINT FK_codigo_recuperacion_usuario FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario) ON DELETE CASCADE);");
}
