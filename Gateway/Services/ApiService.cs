using AppExtractor.Gateway.Models;

namespace AppExtractor.Gateway.Services;

/// <summary>
/// Example service showing how to use HttpGateway for various API calls
/// </summary>
public class ApiService
{
  private readonly HttpGateway _httpGateway;
  private readonly string _baseUrl;

  public ApiService(string baseUrl, HttpGateway? httpGateway = null)
  {
    _baseUrl = baseUrl.TrimEnd('/');
    _httpGateway = httpGateway ?? new HttpGateway();
  }

  /// <summary>
  /// Example: Login to an API
  /// </summary>
  /// <param name="username">Username</param>
  /// <param name="password">Password</param>
  /// <returns>Login response</returns>
  public async Task<LoginResponse?> LoginAsync(string username, string password)
  {
    var loginRequest = new LoginRequest
    {
      Username = username,
      Password = password,
      RememberMe = false
    };

    try
    {
      var response = await _httpGateway.PostJsonAsync<LoginRequest, ApiResponse<LoginResponse>>(
          $"{_baseUrl}/api/auth/login",
          loginRequest
      );

      if (response?.Success == true && response.Data != null)
      {
        // Set the bearer token for future requests
        _httpGateway.SetBearerToken(response.Data.Token);
        return response.Data;
      }

      Console.WriteLine($"Login failed: {response?.Message ?? "Unknown error"}");
      return null;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Login error: {ex.Message}");
      return null;
    }
  }

  /// <summary>
  /// Example: Submit form data
  /// </summary>
  /// <param name="formData">Form data to submit</param>
  /// <returns>Success status</returns>
  public async Task<bool> SubmitFormAsync(Dictionary<string, string> formData)
  {
    try
    {
      var response = await _httpGateway.PostFormAsync(
          $"{_baseUrl}/api/forms/submit",
          formData
      );

      if (response.IsSuccessStatusCode)
      {
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Form submitted successfully: {responseContent}");
        return true;
      }

      Console.WriteLine($"Form submission failed: {response.StatusCode}");
      return false;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Form submission error: {ex.Message}");
      return false;
    }
  }

  /// <summary>
  /// Example: Upload file with form data
  /// </summary>
  /// <param name="filePath">Path to file to upload</param>
  /// <param name="additionalData">Additional form fields</param>
  /// <returns>Success status</returns>
  public async Task<bool> UploadFileAsync(string filePath, Dictionary<string, string>? additionalData = null)
  {
    try
    {
      if (!File.Exists(filePath))
      {
        Console.WriteLine($"File not found: {filePath}");
        return false;
      }

      using var formData = new MultipartFormDataContent();

      // Add file
      var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
      fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
      formData.Add(fileContent, "file", Path.GetFileName(filePath));

      // Add additional form fields
      if (additionalData != null)
      {
        foreach (var kvp in additionalData)
        {
          formData.Add(new StringContent(kvp.Value), kvp.Key);
        }
      }

      var response = await _httpGateway.PostMultipartAsync(
          $"{_baseUrl}/api/upload",
          formData
      );

      if (response.IsSuccessStatusCode)
      {
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"File uploaded successfully: {responseContent}");
        return true;
      }

      Console.WriteLine($"File upload failed: {response.StatusCode}");
      return false;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"File upload error: {ex.Message}");
      return false;
    }
  }

  /// <summary>
  /// Example: Send JSON data
  /// </summary>
  /// <typeparam name="T">Type of data to send</typeparam>
  /// <param name="endpoint">API endpoint</param>
  /// <param name="data">Data to send</param>
  /// <returns>Success status</returns>
  public async Task<bool> SendJsonDataAsync<T>(string endpoint, T data)
  {
    try
    {
      var response = await _httpGateway.PostJsonAsync<T, ApiResponse<object>>(
          $"{_baseUrl}{endpoint}",
          data
      );

      if (response?.Success == true)
      {
        Console.WriteLine("JSON data sent successfully");
        return true;
      }

      Console.WriteLine($"JSON data submission failed: {response?.Message ?? "Unknown error"}");
      return false;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"JSON data submission error: {ex.Message}");
      return false;
    }
  }

  /// <summary>
  /// Example: Send raw data (XML, plain text, etc.)
  /// </summary>
  /// <param name="endpoint">API endpoint</param>
  /// <param name="data">Raw data</param>
  /// <param name="contentType">Content type</param>
  /// <returns>Response content</returns>
  public async Task<string?> SendRawDataAsync(string endpoint, string data, string contentType = "application/xml")
  {
    try
    {
      var response = await _httpGateway.PostRawAsync(
          $"{_baseUrl}{endpoint}",
          data,
          contentType
      );

      if (response.IsSuccessStatusCode)
      {
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Raw data sent successfully");
        return responseContent;
      }

      Console.WriteLine($"Raw data submission failed: {response.StatusCode}");
      return null;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Raw data submission error: {ex.Message}");
      return null;
    }
  }

  /// <summary>
  /// Configure API key authentication
  /// </summary>
  /// <param name="apiKey">API key</param>
  /// <param name="headerName">Header name (default: X-API-Key)</param>
  public void SetApiKey(string apiKey, string headerName = "X-API-Key")
  {
    _httpGateway.SetApiKey(headerName, apiKey);
  }

  /// <summary>
  /// Configure basic authentication
  /// </summary>
  /// <param name="username">Username</param>
  /// <param name="password">Password</param>
  public void SetBasicAuth(string username, string password)
  {
    _httpGateway.SetBasicAuthentication(username, password);
  }

  /// <summary>
  /// Set custom headers for all requests
  /// </summary>
  /// <param name="headers">Headers to set</param>
  public void SetCustomHeaders(Dictionary<string, string> headers)
  {
    _httpGateway.SetHeaders(headers);
  }

  /// <summary>
  /// Dispose resources
  /// </summary>
  public void DisposeResources()
  {
    _httpGateway?.Dispose();
  }
}