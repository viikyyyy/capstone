using System.Security.Cryptography;
using Capstone.Datos;
using Capstone.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Pages;

[Authorize(Roles = "medico,administrativo,admin")]
[RequestSizeLimit(25 * 1024 * 1024)]
public class SubirArchivoModel : PageModel
{
    private static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
    { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx" };
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly AuditoriaServicio _auditoria;
    public SubirArchivoModel(AppDbContext context, IWebHostEnvironment environment, AuditoriaServicio auditoria)
    {
        _context = context;
        _environment = environment;
        _auditoria = auditoria;
    }

    [BindProperty(SupportsGet = true)] public int IdFicha { get; set; }
    [BindProperty] public IFormFile? Archivo { get; set; }
    public Ficha? Ficha { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Ficha = await _context.Fichas.AsNoTracking().FirstOrDefaultAsync(f => f.IdFicha == IdFicha);
        if (Ficha is null) return NotFound();
        if (!PuedeGestionar(Ficha)) return Forbid();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Ficha = await _context.Fichas.FirstOrDefaultAsync(f => f.IdFicha == IdFicha);
        if (Ficha is null) return NotFound();
        if (!PuedeGestionar(Ficha)) return Forbid();
        if (Archivo is null || Archivo.Length == 0)
        {
            ErrorMessage = "Selecciona un archivo válido.";
            return Page();
        }
        if (Archivo.Length > 20 * 1024 * 1024)
        {
            ErrorMessage = "El archivo no puede superar los 20 MB.";
            return Page();
        }
        var extension = Path.GetExtension(Archivo.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !ExtensionesPermitidas.Contains(extension))
        {
            ErrorMessage = "Tipo de archivo no permitido. Usa PDF, imágenes, Word o Excel.";
            return Page();
        }
        var tiposPermitidos = new[] { "application/pdf", "image/jpeg", "image/png", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" };
        if (!tiposPermitidos.Contains(Archivo.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            ErrorMessage = "El tipo de contenido del archivo no es válido.";
            return Page();
        }

        var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var uploads = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
        Directory.CreateDirectory(uploads);
        var nombre = $"{Guid.NewGuid():N}{Path.GetExtension(Archivo.FileName).ToLowerInvariant()}";
        var ruta = Path.Combine(uploads, nombre);
        await using (var entrada = Archivo.OpenReadStream())
        await using (var salida = System.IO.File.Create(ruta))
        {
            await entrada.CopyToAsync(salida);
        }
        await using var hashStream = System.IO.File.OpenRead(ruta);
        var hash = Convert.ToHexString(await SHA256.HashDataAsync(hashStream));
        var archivo = new Archivo
        {
            IdFicha = IdFicha, IdUsuario = usuarioId, TipoArchivo = Archivo.ContentType,
            NombreArchivo = Path.GetFileName(Archivo.FileName), UrlStorage = $"/uploads/{nombre}",
            HashIntegridad = hash, TamanoBytes = Archivo.Length, FechaCarga = HoraChile.Ahora
        };
        _context.Archivos.Add(archivo);
        await _context.SaveChangesAsync();
        await _auditoria.RegistrarAsync(usuarioId, "subir", "archivo", archivo.IdArchivo, $"Ficha: {IdFicha}");
        return RedirectToPage("/DetallePaciente", new { id_paciente = Ficha.IdPaciente });
    }

    private bool PuedeGestionar(Ficha ficha) => User.IsInRole("administrativo") || User.IsInRole("admin") ||
        (int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) && ficha.IdUsuario == id);
}
