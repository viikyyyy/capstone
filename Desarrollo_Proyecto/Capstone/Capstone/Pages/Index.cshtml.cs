using Capstone.Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context) => _context = context;

        public string Nombre { get; private set; } = string.Empty;
        public string Rol { get; private set; } = string.Empty;
        public bool EsPersonal { get; private set; }
        public int Pacientes { get; private set; }
        public int FichasHoy { get; private set; }
        public int FichasMes { get; private set; }
        public List<FichaResumen> UltimasFichas { get; private set; } = [];

        public async Task OnGetAsync()
        {
            Nombre = User.Identity?.Name ?? "Usuario";
            Rol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "usuario";
            EsPersonal = Rol.Equals("medico", StringComparison.OrdinalIgnoreCase) ||
                         Rol.Equals("administrativo", StringComparison.OrdinalIgnoreCase) ||
                         Rol.Equals("admin", StringComparison.OrdinalIgnoreCase);

            if (!EsPersonal)
            {
                Response.Redirect("/MisFichas");
                return;
            }

            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            Pacientes = await _context.Pacientes.CountAsync();
            FichasHoy = await _context.Fichas.CountAsync(f => f.FechaCreacion >= hoy);
            FichasMes = await _context.Fichas.CountAsync(f => f.FechaCreacion >= inicioMes);
            UltimasFichas = await _context.Fichas.AsNoTracking().Include(f => f.Paciente)
                .OrderByDescending(f => f.FechaCreacion).Take(5)
                .Select(f => new FichaResumen(f.IdFicha, f.IdPaciente, $"{f.Paciente!.Pnombre} {f.Paciente.Apellidop}", f.FechaAtencion, f.MotivoConsulta)).ToListAsync();
        }

        public record FichaResumen(int IdFicha, int IdPaciente, string Paciente, DateTime FechaAtencion, string Motivo);
    }
}
