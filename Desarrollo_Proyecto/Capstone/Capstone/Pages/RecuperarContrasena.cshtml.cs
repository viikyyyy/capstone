using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages;

public class RecuperarContrasenaModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly AuditoriaServicio _auditoria;
    public RecuperarContrasenaModel(AppDbContext context, AuditoriaServicio auditoria) { _context = context; _auditoria = auditoria; }
    [BindProperty] public string Identificador { get; set; } = string.Empty;
    public string? CodigoDemo { get; private set; }
    public string? Mensaje { get; private set; }
    public async Task<IActionResult> OnPostAsync()
    {
        var texto = Identificador.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == texto || u.Rut == texto.Replace(".", "").Replace("-", ""));
        if (usuario is null) { await _auditoria.RegistrarAsync(null, "recuperacion_solicitada", "usuario", 0); Mensaje = "Si los datos existen, se generó un código de recuperación."; return Page(); }
        var codigo = Random.Shared.Next(100000, 999999).ToString();
        _context.CodigosRecuperacion.Add(new CodigoRecuperacion { IdUsuario = usuario.IdUsuario, Codigo = codigo, Expiracion = HoraChile.Ahora.AddMinutes(15) });
        await _context.SaveChangesAsync();
        await _auditoria.RegistrarAsync(usuario.IdUsuario, "recuperacion_solicitada", "usuario", usuario.IdUsuario);
        // TODO: integrar MailKit/SMTP para enviar este código en producción.
        CodigoDemo = codigo; Mensaje = "Código generado. En modo demo se muestra directamente en pantalla."; return Page();
    }
}
