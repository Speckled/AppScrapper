namespace AppExtractor.Gateway.Models;

/// <summary>
/// Base API response model
/// </summary>
public class ApiResponse<T>
{
  public bool Success { get; set; }
  public string Message { get; set; } = string.Empty;
  public T? Data { get; set; }
  public string? Error { get; set; }
}

/// <summary>
/// Login request model
/// </summary>
public class LoginRequest
{
  public string Username { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public bool RememberMe { get; set; }
}

/// <summary>
/// Login response model
/// </summary>
public class LoginResponse
{
  public string Token { get; set; } = string.Empty;
  public string RefreshToken { get; set; } = string.Empty;
  public DateTime ExpiresAt { get; set; }
  public UserInfo? User { get; set; }
}

/// <summary>
/// User information model
/// </summary>
public class UserInfo
{
  public string Id { get; set; } = string.Empty;
  public string Username { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string FullName { get; set; } = string.Empty;
  public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Generic data submission model
/// </summary>
public class DataSubmissionRequest<T>
{
  public string Action { get; set; } = string.Empty;
  public T? Payload { get; set; }
  public Dictionary<string, string> Metadata { get; set; } = new();
}