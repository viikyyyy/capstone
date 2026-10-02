using Capstone.Datos;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages;

public class PruebaConexionModel : PageModel
{
    private readonly AppDbContext _context;

    public PruebaConexionModel(AppDbContext context)
    {
        _context = context;
    }

    public bool ConexionExitosa { get; private set; }
    public string Mensaje { get; private set; } = string.Empty;
    public int CantidadUsuarios { get; private set; }

    public async Task OnGetAsync()
    {
        try
        {
            CantidadUsuarios = await _context.Usuarios.CountAsync();
            ConexionExitosa = true;
            Mensaje = "La conexión a la base de datos funciona correctamente.";
        }
        catch (Exception ex)
        {
            ConexionExitosa = false;
            Mensaje = $"No fue posible conectar con la base de datos: {ex.Message}";
        }
    }
}
