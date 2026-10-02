using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages;

[Authorize(Roles = "administrativo,admin")]
public class RespaldosModel : PageModel
{
    private readonly AppDbContext _context;
    public RespaldosModel(AppDbContext context) => _context = context;
    public List<Backup> Respaldos { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Respaldos = await _context.Backups.AsNoTracking()
            .OrderByDescending(b => b.FechaInicio).Take(100).ToListAsync();
    }
}
