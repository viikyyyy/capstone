using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages;

[Authorize(Roles = "paciente")]
public class MisFichasModel : PageModel
{
    private readonly AppDbContext _context;
    public MisFichasModel(AppDbContext context) => _context = context;
    public List<Ficha> Fichas { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(id, out var idUsuario)) return;
        Fichas = await _context.Fichas.AsNoTracking()
            .Include(f => f.Paciente).Include(f => f.Archivos)
            .Where(f => f.Paciente != null && f.Paciente.IdUsuario == idUsuario)
            .OrderByDescending(f => f.FechaAtencion).ToListAsync();
    }
}
