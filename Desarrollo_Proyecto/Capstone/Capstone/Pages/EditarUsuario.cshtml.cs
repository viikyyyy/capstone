using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages;
[Authorize(Roles = "administrativo,admin")]
public class EditarUsuarioModel : PageModel
{
    private readonly AppDbContext _context; private readonly PasswordHasher<Usuario> _hasher = new(); private readonly AuditoriaServicio _auditoria;
    public EditarUsuarioModel(AppDbContext context, AuditoriaServicio auditoria) { _context = context; _auditoria = auditoria; }
    [BindProperty] public Usuario Datos { get; set; } = new();
    [BindProperty] public string? NuevaContrasena { get; set; }
    public async Task<IActionResult> OnGetAsync(int id_usuario) { Datos = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == id_usuario) ?? new(); return Datos.IdUsuario == 0 ? NotFound() : Page(); }
    public async Task<IActionResult> OnPostAsync()
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == Datos.IdUsuario); if (usuario is null) return NotFound();
        usuario.Pnombre = Datos.Pnombre.Trim(); usuario.Apellidop = Datos.Apellidop.Trim(); usuario.Apellidom = Datos.Apellidom?.Trim(); usuario.Correo = Datos.Correo.Trim().ToLowerInvariant(); usuario.Rol = Datos.Rol; usuario.Estado = Datos.Estado;
        if (!string.IsNullOrWhiteSpace(NuevaContrasena)) usuario.ContrasenaHash = _hasher.HashPassword(usuario, NuevaContrasena);
        await _context.SaveChangesAsync(); var id = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var adminId) ? adminId : (int?)null; await _auditoria.RegistrarAsync(id, "editar", "usuario", usuario.IdUsuario); return RedirectToPage("/GestionUsuarios");
    }
}
