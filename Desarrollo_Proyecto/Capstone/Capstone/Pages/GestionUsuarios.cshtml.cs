using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages;

[Authorize(Roles = "administrativo,admin")]
public class GestionUsuariosModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly AuditoriaServicio _auditoria;
    private readonly PasswordHasher<Usuario> _hasher = new();
    public GestionUsuariosModel(AppDbContext context, AuditoriaServicio auditoria) { _context = context; _auditoria = auditoria; }
    public List<Usuario> Usuarios { get; private set; } = [];
    [BindProperty] public UsuarioInput Nuevo { get; set; } = new();
    public string? Mensaje { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync() => await CargarAsync();

    public async Task<IActionResult> OnPostCrearAsync()
    {
        if (!ModelState.IsValid) { await CargarAsync(); return Page(); }
        if (await _context.Usuarios.AnyAsync(u => u.Correo == Nuevo.Correo.ToLowerInvariant()))
        { ErrorMessage = "El correo ya está registrado."; await CargarAsync(); return Page(); }
        var usuario = new Usuario { Rut = Nuevo.Rut, DvRut = Nuevo.DvRut.ToUpperInvariant(), Pnombre = Nuevo.Pnombre.Trim(), Apellidop = Nuevo.Apellidop.Trim(), Correo = Nuevo.Correo.Trim().ToLowerInvariant(), Rol = Nuevo.Rol, Estado = "activo", FechaCreacion = HoraChile.Ahora };
        usuario.ContrasenaHash = _hasher.HashPassword(usuario, Nuevo.Contrasena);
        _context.Usuarios.Add(usuario); await _context.SaveChangesAsync();
        if (usuario.Rol.Equals("paciente", StringComparison.OrdinalIgnoreCase))
        {
            _context.Pacientes.Add(new Paciente { IdUsuario = usuario.IdUsuario, Rut = usuario.Rut, DvRut = usuario.DvRut, Pnombre = usuario.Pnombre, Apellidop = usuario.Apellidop, FechaNacimiento = Nuevo.FechaNacimiento ?? new DateTime(2000, 1, 1) });
            await _context.SaveChangesAsync();
        }
        await _auditoria.RegistrarAsync(ObtenerId(), "crear", "usuario", usuario.IdUsuario); Mensaje = "Usuario creado."; await CargarAsync(); return Page();
    }

    public async Task<IActionResult> OnPostCambiarEstadoAsync(int idUsuario)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        if (usuario is null) return NotFound();
        usuario.Estado = usuario.Estado.Equals("activo", StringComparison.OrdinalIgnoreCase) ? "inactivo" : "activo";
        await _context.SaveChangesAsync(); await _auditoria.RegistrarAsync(ObtenerId(), "cambiar_estado", "usuario", usuario.IdUsuario, usuario.Estado); return RedirectToPage();
    }

    private async Task CargarAsync() => Usuarios = await _context.Usuarios.AsNoTracking().OrderBy(u => u.Apellidop).ThenBy(u => u.Pnombre).ToListAsync();
    private int? ObtenerId() => int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    public class UsuarioInput
    {
        [System.ComponentModel.DataAnnotations.Required] public string Rut { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required] public string DvRut { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required] public string Pnombre { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required] public string Apellidop { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.EmailAddress] public string Correo { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required] public string Contrasena { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required] public string Rol { get; set; } = "paciente";
        public DateTime? FechaNacimiento { get; set; }
    }
}
