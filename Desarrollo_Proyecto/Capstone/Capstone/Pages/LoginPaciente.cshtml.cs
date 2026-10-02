using System.Security.Claims;
using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages;

public class LoginPacienteModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly AuditoriaServicio _auditoria;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public LoginPacienteModel(AppDbContext context, AuditoriaServicio auditoria) { _context = context; _auditoria = auditoria; }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true
        ? RedirectToPage("/Index") : Page();

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid) return Page();

        var identificador = Input.Identificador.Trim();
        var rut = NormalizarRut(identificador);
        if (rut != null && !RutValido(identificador))
        {
            ErrorMessage = "El RUT ingresado no es válido.";
            return Page();
        }

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u =>
            (u.Correo == identificador.ToLower() || (rut != null && u.Rut == rut)) &&
            u.Rol.ToLower() == "paciente");

        if (usuario is null || !usuario.Estado.Equals("activo", StringComparison.OrdinalIgnoreCase) ||
            _passwordHasher.VerifyHashedPassword(usuario, usuario.ContrasenaHash, Input.Contrasena) == PasswordVerificationResult.Failed)
        {
            await _auditoria.RegistrarAsync(usuario?.IdUsuario, "inicio_sesion_fallido", "usuario", usuario?.IdUsuario ?? 0);
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
        await _auditoria.RegistrarAsync(usuario.IdUsuario, "inicio_sesion", "usuario", usuario.IdUsuario);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

        return LocalRedirect(returnUrl ?? "/");
    }

    private static string? NormalizarRut(string identificador)
    {
        var limpio = identificador.Replace(".", "").Replace("-", "").Replace(" ", "").ToUpperInvariant();
        return limpio.Length > 1 && limpio.All(c => char.IsDigit(c) || c == 'K') ? limpio[..^1] : null;
    }

    private static bool RutValido(string identificador)
    {
        var limpio = identificador.Replace(".", "").Replace("-", "").Replace(" ", "").ToUpperInvariant();
        if (limpio.Length < 2 || !limpio[..^1].All(char.IsDigit) || (limpio[^1] != 'K' && !char.IsDigit(limpio[^1]))) return false;
        var suma = 0; var multiplicador = 2;
        for (var i = limpio.Length - 2; i >= 0; i--)
        {
            suma += (limpio[i] - '0') * multiplicador;
            multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
        }
        var resultado = 11 - suma % 11;
        var esperado = resultado == 11 ? '0' : resultado == 10 ? 'K' : (char)('0' + resultado);
        return limpio[^1] == esperado;
    }

    public class LoginInput
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Ingresa tu correo o RUT.")]
        public string Identificador { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Ingresa tu contraseña.")]
        public string Contrasena { get; set; } = string.Empty;
    }
}
