using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages;

[Authorize]
public class PerfilModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly AuditoriaServicio _auditoria;
    public PerfilModel(AppDbContext context, AuditoriaServicio auditoria) { _context = context; _auditoria = auditoria; }
    [BindProperty] public Usuario Datos { get; set; } = new();
    public string? Mensaje { get; private set; }

    public async Task<IActionResult> OnGetAsync() => await CargarAsync();
    public async Task<IActionResult> OnPostAsync()
    {
        var id = ObtenerId(); if (id is null) return Forbid();
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
        if (usuario is null) return NotFound();
        usuario.Pnombre = Datos.Pnombre.Trim(); usuario.Apellidop = Datos.Apellidop.Trim();
        usuario.Apellidom = Datos.Apellidom?.Trim(); usuario.Correo = Datos.Correo.Trim().ToLowerInvariant();
        await _context.SaveChangesAsync(); await _auditoria.RegistrarAsync(id, "editar", "usuario", id.Value, "Actualización de perfil"); Mensaje = "Datos actualizados."; Datos = usuario; return Page();
    }
    private async Task<IActionResult> CargarAsync()
    {
        var id = ObtenerId(); if (id is null) return Forbid();
        Datos = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == id) ?? new();
        return Page();
    }
    private int? ObtenerId() => int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
}
