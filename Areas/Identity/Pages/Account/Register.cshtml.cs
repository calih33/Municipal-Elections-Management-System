using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Municipal_Elections_Management_System.Areas.Identity.Pages.Account;

// Overrides the default Identity UI Register page so the public can't create accounts.
[AllowAnonymous]
public class RegisterModel : PageModel
{
    public IActionResult OnGet() => NotFound();

    public IActionResult OnPost() => NotFound();
}
