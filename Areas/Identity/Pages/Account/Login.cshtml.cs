using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Municipal_Elections_Management_System.Areas.Identity.Pages.Account;

// Replaces the default Identity UI login, which only accepts email addresses.
// The seeded admins sign in with user names ("super", "pm").
[AllowAnonymous]
public class LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        // Clear any existing external cookie to ensure a clean login process.
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            Input.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("User {UserName} logged in.", Input.UserName);
            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            logger.LogWarning("User {UserName} is locked out.", Input.UserName);
            ModelState.AddModelError(string.Empty, "This account is locked. Try again in a few minutes.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Invalid username or password.");
        return Page();
    }
}
