// This file contains the AuthApiController class (mobile app register / login) for the Municipal Elections Management System.
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Municipal_Elections_Management_System.Models.Api;
using Municipal_Elections_Management_System.Services;

namespace Municipal_Elections_Management_System.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly JwtTokenService _tokens;

    public AuthApiController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, JwtTokenService tokens)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokens = tokens;
    }

    // POST: api/auth/register
    // mobile users get no roles, so they can't use the Admin area
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim();
        var name = request.Name.Trim();

        if (await _userManager.FindByEmailAsync(email) != null)
        {
            ModelState.AddModelError(nameof(RegisterRequest.Email), "An account with this email already exists.");
            return ValidationProblem(ModelState);
        }

        var user = new IdentityUser { UserName = email, Email = email };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            // e.g. password rules; put each error under the field it belongs to
            foreach (var error in result.Errors)
            {
                var field = error.Code.StartsWith("Password") ? nameof(RegisterRequest.Password) : nameof(RegisterRequest.Email);
                ModelState.AddModelError(field, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        // IdentityUser has no name column, so the display name is stored as a user claim
        var claimResult = await _userManager.AddClaimAsync(user, new Claim(JwtRegisteredClaimNames.Name, name));
        if (!claimResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not save the name for '{email}': {string.Join("; ", claimResult.Errors.Select(e => e.Description))}");
        }

        // signed in straight away
        return Ok(new AuthResponse(_tokens.CreateToken(user, name)));
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user == null)
        {
            return Problem(title: "Invalid email or password.", statusCode: StatusCodes.Status401Unauthorized);
        }

        // locks the account for a few minutes after too many wrong passwords
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            return Problem(title: "Too many failed attempts. Try again in a few minutes.", statusCode: StatusCodes.Status401Unauthorized);
        }
        if (!result.Succeeded)
        {
            return Problem(title: "Invalid email or password.", statusCode: StatusCodes.Status401Unauthorized);
        }

        return Ok(new AuthResponse(_tokens.CreateToken(user, await GetNameAsync(user))));
    }

    // GET: api/auth/me
    // the signed-in user, for the profile screen
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<UserProfileDto>> Me()
    {
        var user = await _userManager.FindByIdAsync(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? string.Empty);
        if (user == null)
        {
            // token is valid but the account was deleted
            return Problem(title: "This account no longer exists.", statusCode: StatusCodes.Status401Unauthorized);
        }

        return new UserProfileDto(user.Id, await GetNameAsync(user), user.Email ?? string.Empty);
    }

    // seeded admins have no name claim, so fall back to their user name
    private async Task<string> GetNameAsync(IdentityUser user)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        return claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value
            ?? user.UserName
            ?? string.Empty;
    }
}
