using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Capstone.Pages;

[Authorize(Roles = "medico,administrativo,admin")]
public class CrearFichaModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly AuditoriaServicio _auditoria;
    private readonly PasswordHasher<Usuario> _hasher = new();
    public CrearFichaModel(AppDbContext context, AuditoriaServicio auditoria) { _context = context; _auditoria = auditoria; }
    public List<Paciente> Pacientes { get; private set; } = [];
    [BindProperty] public Ficha Ficha { get; set; } = new();
    [BindProperty] public bool CrearUsuario { get; set; }
    [BindProperty] public NuevoPacienteInput NuevoPaciente { get; set; } = new();
    public async Task OnGetAsync()
    {
        Ficha.FechaAtencion = HoraChile.Ahora;
        Pacientes = await _context.Pacientes.AsNoTracking().OrderBy(p => p.Apellidop).ToListAsync();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        var id = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var usuarioId) ? usuarioId : 0;
        if (id == 0) return Forbid();
        if (CrearUsuario)
        {
            if (string.IsNullOrWhiteSpace(NuevoPaciente.Correo) || string.IsNullOrWhiteSpace(NuevoPaciente.Contrasena))
                ModelState.AddModelError("NuevoPaciente", "El correo y la contraseña son obligatorios para crear el acceso.");
            if (await _context.Usuarios.AnyAsync(u => u.Correo == NuevoPaciente.Correo.Trim().ToLower()))
                ModelState.AddModelError("NuevoPaciente.Correo", "Ya existe un usuario con ese correo.");
            if (await _context.Pacientes.AnyAsync(p => p.Rut == NuevoPaciente.Rut.Trim()))
                ModelState.AddModelError("NuevoPaciente.Rut", "Ya existe un paciente con ese RUT.");
            if (!ModelState.IsValid) { await CargarPacientesAsync(); return Page(); }

            var usuario = new Usuario
            {
                Rut = NuevoPaciente.Rut.Trim(), DvRut = NuevoPaciente.DvRut.Trim().ToUpperInvariant(),
                Pnombre = NuevoPaciente.Pnombre.Trim(), Apellidop = NuevoPaciente.Apellidop.Trim(),
                Correo = NuevoPaciente.Correo.Trim().ToLowerInvariant(), Rol = "paciente",
                FechaCreacion = HoraChile.Ahora
            };
            usuario.ContrasenaHash = _hasher.HashPassword(usuario, NuevoPaciente.Contrasena);
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            var paciente = new Paciente { IdUsuario = usuario.IdUsuario, Rut = usuario.Rut, DvRut = usuario.DvRut, Pnombre = usuario.Pnombre, Apellidop = usuario.Apellidop, FechaNacimiento = NuevoPaciente.FechaNacimiento ?? new DateTime(2000, 1, 1) };
            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();
            Ficha.IdPaciente = paciente.IdPaciente;
        }
        if (!await _context.Pacientes.AnyAsync(p => p.IdPaciente == Ficha.IdPaciente))
        {
            ModelState.AddModelError("Ficha.IdPaciente", "Selecciona un paciente válido o crea uno nuevo.");
            await CargarPacientesAsync(); return Page();
        }
        Ficha.IdUsuario = id; Ficha.FechaCreacion = HoraChile.Ahora;
        if (Ficha.FechaAtencion == default) Ficha.FechaAtencion = HoraChile.Ahora;
        _context.Fichas.Add(Ficha); await _context.SaveChangesAsync();
        await _auditoria.RegistrarAsync(id, "crear", "ficha", Ficha.IdFicha, $"Paciente: {Ficha.IdPaciente}");
        return RedirectToPage("/DetallePaciente", new { id_paciente = Ficha.IdPaciente });
    }

    private async Task CargarPacientesAsync() => Pacientes = await _context.Pacientes.AsNoTracking().OrderBy(p => p.Apellidop).ToListAsync();

    public class NuevoPacienteInput
    {
        public string Rut { get; set; } = string.Empty;
        public string DvRut { get; set; } = string.Empty;
        public string Pnombre { get; set; } = string.Empty;
        public string Apellidop { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
    }
}
