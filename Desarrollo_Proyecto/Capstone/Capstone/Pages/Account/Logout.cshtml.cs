using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Capstone.Datos;

namespace Capstone.Pages.Account;

[Authorize]
public class LogoutModel : PageModel
{
    private readonly AuditoriaServicio _auditoria;
    public LogoutModel(AuditoriaServicio auditoria) => _auditoria = auditoria;
    public async Task<IActionResult> OnPostAsync()
    {
        if (int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id))
            await _auditoria.RegistrarAsync(id, "cierre_sesion", "usuario", id);
        await HttpContext.SignOutAsync();
        return RedirectToPage("/SelectorIngreso");
    }
}
