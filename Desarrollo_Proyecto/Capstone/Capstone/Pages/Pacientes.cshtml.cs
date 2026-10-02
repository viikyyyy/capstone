using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages;

[Authorize(Roles = "medico,administrativo,admin")]
public class PacientesModel : PageModel
{
    private readonly AppDbContext _context;
    public PacientesModel(AppDbContext context) => _context = context;
    public string? Busqueda { get; private set; }
    public List<Paciente> Resultados { get; private set; } = [];

    public async Task OnGetAsync(string? busqueda)
    {
        Busqueda = busqueda?.Trim();
        var consulta = _context.Pacientes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(Busqueda))
        {
            var texto = Busqueda.Replace(".", "").Replace("-", "").Replace(" ", "").ToLower();
            consulta = consulta.Where(p => p.Rut.ToLower().Contains(texto) ||
                p.Apellidop.ToLower().Contains(Busqueda.ToLower()) ||
                (p.Apellidom != null && p.Apellidom.ToLower().Contains(Busqueda.ToLower())));
        }
        Resultados = await consulta.OrderBy(p => p.Apellidop).ThenBy(p => p.Pnombre).Take(100).ToListAsync();
    }
}
