using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages;

[Authorize(Roles = "medico,administrativo,admin")]
public class DetallePacienteModel : PageModel
{
    private readonly AppDbContext _context;
    public DetallePacienteModel(AppDbContext context) => _context = context;
    public Paciente? Paciente { get; private set; }
    public List<Ficha> Fichas { get; private set; } = [];
    public async Task<IActionResult> OnGetAsync(int id_paciente)
    {
        Paciente = await _context.Pacientes.AsNoTracking().FirstOrDefaultAsync(p => p.IdPaciente == id_paciente);
        if (Paciente is null) return NotFound();
        Fichas = await _context.Fichas.AsNoTracking().Include(f => f.Archivos).Include(f => f.Usuario).Where(f => f.IdPaciente == id_paciente).OrderByDescending(f => f.FechaAtencion).ToListAsync();
        return Page();
    }
}
