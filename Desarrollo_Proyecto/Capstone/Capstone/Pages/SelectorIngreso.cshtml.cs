using Capstone.Datos;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Capstone.Pages;

public class SelectorIngresoModel : PageModel
{
    // Se inyecta para mantener el mismo patrón de acceso de las páginas del sistema.
    // Esta vista no ejecuta consultas: solo selecciona el portal de acceso.
    private readonly AppDbContext _context;

    public SelectorIngresoModel(AppDbContext context)
    {
        _context = context;
    }
}
