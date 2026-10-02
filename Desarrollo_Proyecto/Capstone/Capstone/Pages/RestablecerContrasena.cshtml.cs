using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages;

public class RestablecerContrasenaModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<Usuario> _hasher = new();
    private readonly AuditoriaServicio _auditoria;
    public RestablecerContrasenaModel(AppDbContext context, AuditoriaServicio auditoria) { _context = context; _auditoria = auditoria; }
    [BindProperty] public string Correo { get; set; } = string.Empty;
    [BindProperty] public string Codigo { get; set; } = string.Empty;
    [BindProperty] public string NuevaContrasena { get; set; } = string.Empty;
    public string? Mensaje { get; private set; }
    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(NuevaContrasena) || NuevaContrasena.Length < 12 ||
            !NuevaContrasena.Any(char.IsUpper) || !NuevaContrasena.Any(char.IsLower) ||
            !NuevaContrasena.Any(char.IsDigit) || NuevaContrasena.All(char.IsLetterOrDigit))
        {
            Mensaje = "La contraseña debe tener al menos 12 caracteres, mayúscula, minúscula, número y símbolo.";
            return Page();
        }
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == Correo.Trim().ToLowerInvariant());
        var token = usuario is null ? null : await _context.CodigosRecuperacion.Where(c => c.IdUsuario == usuario.IdUsuario && c.Codigo == Codigo.Trim() && !c.Usado && c.Expiracion > HoraChile.Ahora).OrderByDescending(c => c.Expiracion).FirstOrDefaultAsync();
        if (usuario is null || token is null) { await _auditoria.RegistrarAsync(usuario?.IdUsuario, "restablecimiento_fallido", "usuario", usuario?.IdUsuario ?? 0); Mensaje = "El código no es válido o expiró."; return Page(); }
        usuario.ContrasenaHash = _hasher.HashPassword(usuario, NuevaContrasena); token.Usado = true; await _context.SaveChangesAsync(); await _auditoria.RegistrarAsync(usuario.IdUsuario, "contraseña_actualizada", "usuario", usuario.IdUsuario); Mensaje = "Contraseña actualizada correctamente."; return Page();
    }
}
