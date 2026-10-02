using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages;

[Authorize(Roles = "medico,administrativo,admin")]
public class EditarFichaModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly AuditoriaServicio _auditoria;
    public EditarFichaModel(AppDbContext context, AuditoriaServicio auditoria) { _context = context; _auditoria = auditoria; }

    [BindProperty]
    public Ficha Ficha { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id_ficha)
    {
        var ficha = await _context.Fichas.AsNoTracking().FirstOrDefaultAsync(f => f.IdFicha == id_ficha);
        if (ficha is null) return NotFound();
        if (!PuedeEditar(ficha)) return Forbid();
        Ficha = ficha;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var ficha = await _context.Fichas.FirstOrDefaultAsync(f => f.IdFicha == Ficha.IdFicha);
        if (ficha is null) return NotFound();
        if (!PuedeEditar(ficha)) return Forbid();
        if (!ModelState.IsValid) return Page();

        ficha.FechaAtencion = Ficha.FechaAtencion;
        ficha.MotivoConsulta = Ficha.MotivoConsulta.Trim();
        ficha.Diagnostico = Ficha.Diagnostico?.Trim();
        ficha.Tratamiento = Ficha.Tratamiento?.Trim();
        ficha.Observaciones = Ficha.Observaciones?.Trim();
        ficha.FechaModificacion = HoraChile.Ahora;
        await _context.SaveChangesAsync();
        var idUsuario = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : (int?)null;
        await _auditoria.RegistrarAsync(idUsuario, "editar", "ficha", ficha.IdFicha);
        return RedirectToPage("/DetallePaciente", new { id_paciente = ficha.IdPaciente });
    }

    private bool PuedeEditar(Ficha ficha)
    {
        if (User.IsInRole("administrativo") || User.IsInRole("admin")) return true;
        return int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id)
               && ficha.IdUsuario == id;
    }
}
