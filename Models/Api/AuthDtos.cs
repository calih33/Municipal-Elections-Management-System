using System.ComponentModel.DataAnnotations;

namespace Municipal_Elections_Management_System.Models.Api;

// POST: api/auth/register
public class RegisterRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    // Identity also requires an uppercase letter, a lowercase letter, a digit and a symbol
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

// POST: api/auth/login
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public record AuthResponse(string Token);

public record UserProfileDto(string Id, string Name, string Email);
