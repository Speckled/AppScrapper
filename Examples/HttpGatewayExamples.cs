using AppExtractor.Gateway;
using AppExtractor.Gateway.Models;
using AppExtractor.Gateway.Services;

namespace AppExtractor.Examples;

/// <summary>
/// Examples showing how to use the HttpGateway class
/// </summary>
public static class HttpGatewayExamples
{
  /// <summary>
  /// Example 1: Simple JSON POST request
  /// </summary>
  public static async Task SimpleJsonPostExample()
  {
    using var gateway = new HttpGateway();

    // Set timeout
    gateway.SetTimeout(TimeSpan.FromSeconds(30));

    // Example data
    var requestData = new
    {
      Name = "John Doe",
      Email = "john@example.com",
      Age = 30
    };

    try
    {
      // Send JSON POST request
      var response = await gateway.PostJsonAsync<object, dynamic>(
          "https://jsonplaceholder.typicode.com/posts",
          requestData
      );

      Console.WriteLine($"Response received: {response}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error: {ex.Message}");
    }
  }

  /// <summary>
  /// Example 2: Form data POST request
  /// </summary>
  public static async Task FormPostExample()
  {
    using var gateway = new HttpGateway();

    var formData = new Dictionary<string, string>
        {
            {"username", "testuser"},
            {"password", "testpass"},
            {"remember", "true"}
        };

    try
    {
      var response = await gateway.PostFormAsync(
          "https://httpbin.org/post",
          formData
      );

      var responseContent = await response.Content.ReadAsStringAsync();
      Console.WriteLine($"Form response: {responseContent}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error: {ex.Message}");
    }
  }

  /// <summary>
  /// Example 3: Authenticated API request
  /// </summary>
  public static async Task AuthenticatedRequestExample()
  {
    using var gateway = new HttpGateway();

    // Set bearer token
    gateway.SetBearerToken("your-jwt-token-here");

    // Or set API key
    gateway.SetApiKey("X-API-Key", "your-api-key-here");

    // Or set basic auth
    gateway.SetBasicAuthentication("username", "password");

    var data = new { message = "Hello, authenticated API!" };

    try
    {
      var response = await gateway.PostRawAsync(
          "https://api.example.com/secure-endpoint",
          System.Text.Json.JsonSerializer.Serialize(data),
          "application/json"
      );

      if (response.IsSuccessStatusCode)
      {
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Authenticated response: {content}");
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error: {ex.Message}");
    }
  }

  /// <summary>
  /// Example 4: File upload with multipart form data
  /// </summary>
  public static async Task FileUploadExample()
  {
    using var gateway = new HttpGateway();
    using var formData = new MultipartFormDataContent();

    // Add text fields
    formData.Add(new StringContent("Test Document"), "title");
    formData.Add(new StringContent("This is a test upload"), "description");

    // Add file (example with a text file)
    var fileContent = System.Text.Encoding.UTF8.GetBytes("This is file content");
    var byteContent = new ByteArrayContent(fileContent);
    byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
    formData.Add(byteContent, "file", "test.txt");

    try
    {
      var response = await gateway.PostMultipartAsync(
          "https://httpbin.org/post",
          formData
      );

      var responseContent = await response.Content.ReadAsStringAsync();
      Console.WriteLine($"Upload response: {responseContent}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error: {ex.Message}");
    }
  }

  /// <summary>
  /// Example 5: Using ApiService for structured API calls
  /// </summary>
  public static async Task ApiServiceExample()
  {
    var apiService = new ApiService("https://api.example.com");

    try
    {
      // Login
      var loginResult = await apiService.LoginAsync("testuser", "testpass");
      if (loginResult != null)
      {
        Console.WriteLine($"Login successful. Token expires at: {loginResult.ExpiresAt}");

        // Submit form data
        var formData = new Dictionary<string, string>
                {
                    {"field1", "value1"},
                    {"field2", "value2"}
                };

        var formSuccess = await apiService.SubmitFormAsync(formData);
        Console.WriteLine($"Form submission: {(formSuccess ? "Success" : "Failed")}");

        // Send JSON data
        var jsonData = new { action = "update", value = 123 };
        var jsonSuccess = await apiService.SendJsonDataAsync("/api/data", jsonData);
        Console.WriteLine($"JSON submission: {(jsonSuccess ? "Success" : "Failed")}");
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"API Service Error: {ex.Message}");
    }
    finally
    {
      apiService.DisposeResources();
    }
  }

  /// <summary>
  /// Example 6: Custom headers and error handling
  /// </summary>
  public static async Task CustomHeadersExample()
  {
    using var gateway = new HttpGateway();

    // Set custom headers
    var customHeaders = new Dictionary<string, string>
        {
            {"X-Custom-Header", "CustomValue"},
            {"X-Request-ID", Guid.NewGuid().ToString()},
            {"Accept-Language", "en-US,en;q=0.9"}
        };

    gateway.SetHeaders(customHeaders);

    var requestData = new { test = "data" };

    try
    {
      var response = await gateway.PostAsync(
          "https://httpbin.org/post",
          new StringContent(
              System.Text.Json.JsonSerializer.Serialize(requestData),
              System.Text.Encoding.UTF8,
              "application/json"
          ),
          new Dictionary<string, string> { { "X-Additional-Header", "AdditionalValue" } }
      );

      // Get response headers
      var responseHeaders = new Dictionary<string, string>();
      foreach (var header in response.Headers)
      {
        responseHeaders[header.Key] = string.Join(", ", header.Value);
      }

      Console.WriteLine("Response Headers:");
      foreach (var header in responseHeaders)
      {
        Console.WriteLine($"  {header.Key}: {header.Value}");
      }

      var content = await response.Content.ReadAsStringAsync();
      Console.WriteLine($"Response: {content}");
    }
    catch (HttpRequestException httpEx)
    {
      Console.WriteLine($"HTTP Error: {httpEx.Message}");
    }
    catch (TaskCanceledException tcEx)
    {
      Console.WriteLine($"Request Timeout: {tcEx.Message}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"General Error: {ex.Message}");
    }
  }
}