# HttpGateway - HTTP POST Request Handler

A comprehensive HTTP client wrapper for making various types of HTTP POST requests with authentication, file uploads, and error handling.

## Features

- ✅ JSON POST requests with automatic serialization/deserialization
- ✅ Form data submissions (application/x-www-form-urlencoded)
- ✅ Multipart form data for file uploads
- ✅ Raw content posting (XML, plain text, etc.)
- ✅ Multiple authentication methods (Bearer token, Basic auth, API key)
- ✅ Custom headers support
- ✅ Timeout configuration
- ✅ Response handling utilities
- ✅ Integration with Puppeteer for web scraping workflows

## Quick Start

### Basic JSON POST Request

```csharp
using var gateway = new HttpGateway();

var requestData = new
{
    Name = "John Doe",
    Email = "john@example.com"
};

var response = await gateway.PostJsonAsync<object, ApiResponse>(
    "https://api.example.com/users",
    requestData
);
```

### Form Data Submission

```csharp
using var gateway = new HttpGateway();

var formData = new Dictionary<string, string>
{
    {"username", "testuser"},
    {"password", "testpass"},
    {"remember", "true"}
};

var response = await gateway.PostFormAsync(
    "https://api.example.com/login",
    formData
);
```

### File Upload

```csharp
using var gateway = new HttpGateway();
using var formData = new MultipartFormDataContent();

// Add file
var fileBytes = await File.ReadAllBytesAsync("document.pdf");
var fileContent = new ByteArrayContent(fileBytes);
fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
formData.Add(fileContent, "file", "document.pdf");

// Add additional fields
formData.Add(new StringContent("Document Title"), "title");

var response = await gateway.PostMultipartAsync(
    "https://api.example.com/upload",
    formData
);
```

## Authentication

### Bearer Token (JWT)

```csharp
gateway.SetBearerToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");
```

### API Key

```csharp
gateway.SetApiKey("X-API-Key", "your-api-key-here");
```

### Basic Authentication

```csharp
gateway.SetBasicAuthentication("username", "password");
```

### Custom Headers

```csharp
var headers = new Dictionary<string, string>
{
    {"X-Custom-Header", "CustomValue"},
    {"Accept-Language", "en-US"}
};

gateway.SetHeaders(headers);
```

## Integration with Puppeteer

The library includes extension methods that integrate HTTP requests with Puppeteer scraping:

### Extract and Send Data

```csharp
// Extract data from page elements and send to API
var success = await page.ExtractAndSendDataAsync(
    "https://api.example.com/data",
    ".data-item", // CSS selector
    httpGateway
);
```

### Upload Screenshots

```csharp
// Take screenshot and upload to API
var success = await page.ScreenshotAndUploadAsync(
    "https://api.example.com/upload",
    httpGateway,
    "page_screenshot"
);
```

### Send Form Data

```csharp
// Extract form data and send to API
var success = await page.ExtractFormAndSendAsync(
    "https://api.example.com/form-data",
    "#login-form", // Form selector
    httpGateway
);
```

## Error Handling

```csharp
try
{
    var response = await gateway.PostJsonAsync<object, ApiResponse>(url, data);

    if (response?.Success == true)
    {
        Console.WriteLine("Request successful!");
    }
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
```

## Configuration

### Timeout

```csharp
gateway.SetTimeout(TimeSpan.FromSeconds(30));
```

### Clear Headers

```csharp
gateway.ClearHeaders();
```

## ApiService Class

For structured API interactions, use the `ApiService` class:

```csharp
var apiService = new ApiService("https://api.example.com");

// Login and get JWT token
var loginResult = await apiService.LoginAsync("username", "password");

if (loginResult != null)
{
    // Token is automatically set for subsequent requests

    // Submit form data
    var formData = new Dictionary<string, string> { {"key", "value"} };
    var success = await apiService.SubmitFormAsync(formData);

    // Upload file
    var uploadSuccess = await apiService.UploadFileAsync("/path/to/file.pdf");
}

apiService.DisposeResources();
```

## Response Utilities

```csharp
var response = await gateway.PostFormAsync(url, formData);

// Get response as string
var content = await response.Content.ReadAsStringAsync();

// Get response headers
var headers = new Dictionary<string, string>();
foreach (var header in response.Headers)
{
    headers[header.Key] = string.Join(", ", header.Value);
}

// Check success
if (response.IsSuccessStatusCode)
{
    Console.WriteLine("Request successful!");
}
```

## Thread Safety

The `HttpGateway` class uses `HttpClient` internally, which is thread-safe for sending requests. However, configuration methods (like `SetBearerToken`, `SetHeaders`) should be called before making concurrent requests.

## Disposal

Always dispose of the `HttpGateway` instance when done:

```csharp
using var gateway = new HttpGateway();
// ... use gateway

// Or manually dispose
var gateway = new HttpGateway();
try
{
    // ... use gateway
}
finally
{
    gateway.Dispose();
}
```

## Dependencies

- System.Net.Http
- System.Text.Json
- PuppeteerSharp (for extension methods)
