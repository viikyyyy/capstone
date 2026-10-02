using Capstone.Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages;

[Authorize]
public class DescargarArchivoModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    public DescargarArchivoModel(AppDbContext context, IWebHostEnvironment environment) { _context = context; _environment = environment; }
    public async Task<IActionResult> OnGetAsync(int id_archivo)
    {
        var archivo = await _context.Archivos.Include(a => a.Ficha).ThenInclude(f => f!.Paciente).FirstOrDefaultAsync(a => a.IdArchivo == id_archivo);
        if (archivo is null) return NotFound();
        if (!TienePermiso(archivo)) return Forbid();
        var ruta = Path.Combine(_environment.WebRootPath, "uploads", Path.GetFileName(archivo.UrlStorage));
        if (!System.IO.File.Exists(ruta)) return NotFound();
        return PhysicalFile(ruta, archivo.TipoArchivo, archivo.NombreArchivo);
    }
    private bool TienePermiso(Modelos.Archivo archivo)
    {
        if (User.IsInRole("administrativo") || User.IsInRole("admin")) return true;
        if (!int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id)) return false;
        return archivo.Ficha?.IdUsuario == id || (User.IsInRole("paciente") && archivo.Ficha?.Paciente?.IdUsuario == id);
    }
}
