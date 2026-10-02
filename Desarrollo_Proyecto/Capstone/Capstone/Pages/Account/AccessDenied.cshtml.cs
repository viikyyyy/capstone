using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Capstone.Pages.Account;

[AllowAnonymous]
public class AccessDeniedModel : PageModel
{
}
