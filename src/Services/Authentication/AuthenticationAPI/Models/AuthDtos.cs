namespace AuthService.Models;

public record RegisterRequest(string Username, string Password, string? AdminCode);
public record LoginRequest(string Username, string Password);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record UserProfileResponse(
    string UserId,
    string Username,
    string Role,
    DateTime CreatedAt);