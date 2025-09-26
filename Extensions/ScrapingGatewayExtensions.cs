using AppExtractor.Gateway;
using PuppeteerSharp;

namespace AppExtractor.Extensions;

/// <summary>
/// Extension methods that integrate HTTP Gateway with Puppeteer scraping
/// </summary>
public static class ScrapingGatewayExtensions
{
  /// <summary>
  /// Send projects data as JSON to an API endpoint
  /// </summary>
  /// <param name="projectsData">List of Project objects</param>
  /// <param name="apiUrl">API endpoint URL</param>
  /// <param name="httpGateway">HTTP Gateway instance</param>
  /// <param name="userEmail">User email for metadata</param>
  /// <param name="sourceUrl">Source URL for metadata</param>
  /// <param name="includeMetadata">Whether to include metadata wrapper</param>
  /// <returns>Success status and response</returns>
  public static async Task<(bool Success, string? Response)> SendProjectsDataAsync(
      this List<Project> projectsData,
      string apiUrl,
      HttpGateway httpGateway,
      string? userEmail = null,
      string? sourceUrl = null,
      bool includeMetadata = true)
  {
    try
    {
      object payload;

      if (includeMetadata)
      {
        payload = new
        {
          Timestamp = DateTime.UtcNow,
          UserEmail = userEmail,
          SourceUrl = sourceUrl,
          TotalProjects = projectsData.Count,
          Projects = projectsData.Select(project => new
          {
            ReferenceId = project.ReferenceId,
            ApplicationNumber = project.ApplicationNumber,
            Unit = project.Unit,
            StatusMoreInfo = project.StatusMoreInfo,
            Status = project.Status,
            StatusDate = project.StatusDate,
            Address = project.Address,
            ApplicationType = project.ApplicationType,
            AssignedStaff = project.AssignedStaff,
            SysRef = project.SysRef,
            Archived = project.Archived
          }).ToArray()
        };
      }
      else
      {
        // Send just the projects array
        payload = projectsData;
      }

      Console.WriteLine($"Sending {projectsData.Count} projects to API...");

      var response = await httpGateway.PostJsonAsync<object, dynamic>(
          apiUrl,
          payload
      );

      if (response != null)
      {
        var responseString = System.Text.Json.JsonSerializer.Serialize(response);
        Console.WriteLine($"✅ Projects data sent successfully! Response: {responseString}");
        return (true, responseString);
      }

      Console.WriteLine("❌ Failed to send projects data - no response received");
      return (false, null);
    }
    catch (HttpRequestException httpEx)
    {
      Console.WriteLine($"❌ HTTP Error sending projects: {httpEx.Message}");
      return (false, httpEx.Message);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Error sending projects: {ex.Message}");
      return (false, ex.Message);
    }
  }

  /// <summary>
  /// Send projects data with custom headers
  /// </summary>
  /// <param name="projectsData">List of Project objects</param>
  /// <param name="apiUrl">API endpoint URL</param>
  /// <param name="httpGateway">HTTP Gateway instance</param>
  /// <param name="customHeaders">Custom headers to include</param>
  /// <returns>Success status and response</returns>
  public static async Task<(bool Success, string? Response)> SendProjectsDataWithHeadersAsync(
      this List<Project> projectsData,
      string apiUrl,
      HttpGateway httpGateway,
      Dictionary<string, string> customHeaders)
  {
    try
    {
      Console.WriteLine($"Sending {projectsData.Count} projects with custom headers...");

      var jsonContent = System.Text.Json.JsonSerializer.Serialize(projectsData);
      var response = await httpGateway.PostAsync(
          apiUrl,
          new StringContent(
              jsonContent,
              System.Text.Encoding.UTF8,
              "application/json"
          ),
          customHeaders
      );

      if (response.IsSuccessStatusCode)
      {
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"✅ Projects sent with custom headers successfully!");
        return (true, responseContent);
      }

      var errorContent = await response.Content.ReadAsStringAsync();
      Console.WriteLine($"❌ Failed to send projects: {response.StatusCode} - {errorContent}");
      return (false, $"{response.StatusCode}: {errorContent}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Error sending projects with headers: {ex.Message}");
      return (false, ex.Message);
    }
  }
  /// <summary>
  /// Extract data from page and send it via HTTP POST
  /// </summary>
  /// <param name="page">Puppeteer page</param>
  /// <param name="apiUrl">API endpoint URL</param>
  /// <param name="dataSelector">CSS selector for data elements</param>
  /// <param name="httpGateway">HTTP Gateway instance</param>
  /// <returns>Success status</returns>
  public static async Task<bool> ExtractAndSendDataAsync(
      this IPage page,
      string apiUrl,
      string dataSelector,
      HttpGateway httpGateway)
  {
    try
    {
      // Extract data from the page
      var extractedData = await page.EvaluateExpressionAsync<object[]>($@"
                Array.from(document.querySelectorAll('{dataSelector}')).map(el => ({{
                    text: el.textContent?.trim(),
                    html: el.innerHTML,
                    href: el.href || null,
                    id: el.id || null,
                    className: el.className || null
                }}))
            ");

      Console.WriteLine($"Extracted {extractedData.Length} items from page");

      // Send data via HTTP POST
      var payload = new
      {
        Timestamp = DateTime.UtcNow,
        SourceUrl = page.Url,
        Data = extractedData
      };

      var response = await httpGateway.PostJsonAsync<object, dynamic>(
          apiUrl,
          payload
      );

      if (response != null)
      {
        Console.WriteLine("Data sent successfully to API");
        return true;
      }

      return false;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error extracting and sending data: {ex.Message}");
      return false;
    }
  }

  /// <summary>
  /// Send form data extracted from a page
  /// </summary>
  /// <param name="page">Puppeteer page</param>
  /// <param name="apiUrl">API endpoint URL</param>
  /// <param name="formSelector">CSS selector for form</param>
  /// <param name="httpGateway">HTTP Gateway instance</param>
  /// <returns>Success status</returns>
  public static async Task<bool> ExtractFormAndSendAsync(
      this IPage page,
      string apiUrl,
      string formSelector,
      HttpGateway httpGateway)
  {
    try
    {
      // Extract form data
      var formData = await page.EvaluateExpressionAsync<Dictionary<string, string>>($@"
                (() => {{
                    const form = document.querySelector('{formSelector}');
                    if (!form) return {{}};
                    
                    const data = {{}};
                    const inputs = form.querySelectorAll('input, select, textarea');
                    
                    inputs.forEach(input => {{
                        if (input.name && input.value) {{
                            data[input.name] = input.value;
                        }}
                    }});
                    
                    return data;
                }})()
            ");

      if (formData.Count == 0)
      {
        Console.WriteLine("No form data found");
        return false;
      }

      Console.WriteLine($"Extracted form with {formData.Count} fields");

      // Send form data
      var response = await httpGateway.PostFormAsync(apiUrl, formData);

      if (response.IsSuccessStatusCode)
      {
        Console.WriteLine("Form data sent successfully");
        return true;
      }

      Console.WriteLine($"Failed to send form data: {response.StatusCode}");
      return false;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error extracting and sending form: {ex.Message}");
      return false;
    }
  }

  /// <summary>
  /// Take screenshot and upload it via HTTP
  /// </summary>
  /// <param name="page">Puppeteer page</param>
  /// <param name="uploadUrl">Upload API endpoint</param>
  /// <param name="httpGateway">HTTP Gateway instance</param>
  /// <param name="screenshotName">Name for the screenshot file</param>
  /// <returns>Success status</returns>
  public static async Task<bool> ScreenshotAndUploadAsync(
      this IPage page,
      string uploadUrl,
      HttpGateway httpGateway,
      string screenshotName = "screenshot")
  {
    try
    {
      // Take screenshot
      var screenshotBytes = await page.ScreenshotDataAsync(new ScreenshotOptions
      {
        Type = ScreenshotType.Png,
        FullPage = true
      });

      Console.WriteLine($"Screenshot taken: {screenshotBytes.Length} bytes");

      // Create multipart form data
      using var formData = new MultipartFormDataContent();

      var fileContent = new ByteArrayContent(screenshotBytes);
      fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
      formData.Add(fileContent, "screenshot", $"{screenshotName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

      // Add metadata
      formData.Add(new StringContent(page.Url), "source_url");
      formData.Add(new StringContent(DateTime.UtcNow.ToString("O")), "timestamp");

      // Upload
      var response = await httpGateway.PostMultipartAsync(uploadUrl, formData);

      if (response.IsSuccessStatusCode)
      {
        Console.WriteLine("Screenshot uploaded successfully");
        return true;
      }

      Console.WriteLine($"Failed to upload screenshot: {response.StatusCode}");
      return false;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error taking screenshot and uploading: {ex.Message}");
      return false;
    }
  }

  /// <summary>
  /// Send page cookies to an API
  /// </summary>
  /// <param name="page">Puppeteer page</param>
  /// <param name="apiUrl">API endpoint URL</param>
  /// <param name="httpGateway">HTTP Gateway instance</param>
  /// <returns>Success status</returns>
  public static async Task<bool> SendCookiesToApiAsync(
      this IPage page,
      string apiUrl,
      HttpGateway httpGateway)
  {
    try
    {
      // Get cookies from the page
      var cookies = await page.GetCookiesAsync();

      var cookieData = cookies.Select(c => new
      {
        Name = c.Name,
        Value = c.Value,
        Domain = c.Domain,
        Path = c.Path,
        Expires = c.Expires,
        HttpOnly = c.HttpOnly,
        Secure = c.Secure,
        SameSite = c.SameSite?.ToString()
      }).ToArray();

      Console.WriteLine($"Sending {cookieData.Length} cookies to API");

      var payload = new
      {
        Url = page.Url,
        Timestamp = DateTime.UtcNow,
        Cookies = cookieData
      };

      var response = await httpGateway.PostJsonAsync<object, dynamic>(
          apiUrl,
          payload
      );

      if (response != null)
      {
        Console.WriteLine("Cookies sent successfully");
        return true;
      }

      return false;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error sending cookies: {ex.Message}");
      return false;
    }
  }
}