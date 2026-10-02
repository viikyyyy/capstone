using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages;

[Authorize(Roles = "administrativo,admin")]
public class AuditoriaModel : PageModel
{
    private readonly AppDbContext _context;
    public AuditoriaModel(AppDbContext context) => _context = context;
    public List<Auditoria> Registros { get; private set; } = [];
    [BindProperty(SupportsGet = true)] public int? IdUsuario { get; set; }
    [BindProperty(SupportsGet = true)] public string? Accion { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? Desde { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? Hasta { get; set; }

    public async Task OnGetAsync()
    {
        var consulta = _context.Auditorias.AsNoTracking().Include(a => a.Usuario).AsQueryable();
        if (IdUsuario.HasValue) consulta = consulta.Where(a => a.IdUsuario == IdUsuario);
        if (!string.IsNullOrWhiteSpace(Accion)) consulta = consulta.Where(a => a.Accion.Contains(Accion));
        if (Desde.HasValue) consulta = consulta.Where(a => a.FechaHora >= Desde.Value);
        if (Hasta.HasValue) consulta = consulta.Where(a => a.FechaHora < Hasta.Value.Date.AddDays(1));
        Registros = await consulta.OrderByDescending(a => a.FechaHora).Take(500).ToListAsync();
    }
}
