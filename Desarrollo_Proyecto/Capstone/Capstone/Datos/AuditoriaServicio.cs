using Capstone.Modelos;

namespace Capstone.Datos;

public class AuditoriaServicio
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContext;
    public AuditoriaServicio(AppDbContext context, IHttpContextAccessor httpContext)
    {
        _context = context;
        _httpContext = httpContext;
    }

    public async Task RegistrarAsync(int? idUsuario, string accion, string entidad, int idEntidad, string? detalle = null)
    {
        _context.Auditorias.Add(new Auditoria
        {
            IdUsuario = idUsuario,
            Accion = accion,
            EntidadAfectada = entidad,
            IdEntidadAfectada = idEntidad,
            Detalle = detalle,
            IpOrigen = _httpContext.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "local",
            FechaHora = HoraChile.Ahora
        });
        await _context.SaveChangesAsync();
    }
}
