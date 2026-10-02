using System.Security.Claims;
using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages.Account;

public class LoginModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public LoginModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public void OnGet(string? rol)
    {
        SelectedRole = NormalizarRol(rol);
        if (User.Identity?.IsAuthenticated == true)
        {
            Response.Redirect("/");
        }
    }

    public string? SelectedRole { get; private set; }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var identificador = Input.Identificador.Trim();
        var rut = NormalizarRut(identificador);
        var rol = NormalizarRol(Input.Rol);

        if (rol is null)
        {
            ErrorMessage = "Selecciona el tipo de usuario antes de ingresar.";
            return Page();
        }
        SelectedRole = rol;

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u =>
                (u.Correo == identificador.ToLower() || (rut != null && u.Rut == rut)) &&
                u.Rol.ToLower() == rol);

        if (usuario is null || !string.Equals(usuario.Estado, "activo", StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = "El correo/RUT o la contraseña son incorrectos.";
            return Page();
        }

        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.ContrasenaHash, Input.Contrasena);
        if (resultado == PasswordVerificationResult.Failed)
        {
            ErrorMessage = "El correo/RUT o la contraseña son incorrectos.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Name, $"{usuario.Pnombre} {usuario.Apellidop}"),
            new(ClaimTypes.Email, usuario.Correo),
            new(ClaimTypes.Role, usuario.Rol)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return LocalRedirect(returnUrl ?? "/");
    }

    private static string? NormalizarRut(string identificador)
    {
        var limpio = identificador.Replace(".", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty).ToUpperInvariant();
        return limpio.Length > 1 && limpio.All(c => char.IsDigit(c) || c == 'K')
            ? limpio[..^1]
            : null;
    }

    public class LoginInput
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Ingresa tu correo o RUT.")]
        public string Identificador { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Ingresa tu contraseña.")]
        public string Contrasena { get; set; } = string.Empty;

        public string? Rol { get; set; }
    }

    private static string? NormalizarRol(string? rol) => rol?.Trim().ToLowerInvariant() switch
    {
        "medico" => "medico",
        "paciente" => "paciente",
        _ => null
    };
}
