using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AppExtractor.Gateway;

/// <summary>
/// HTTP Gateway class for making HTTP requests with various authentication and content types
/// </summary>
public class HttpGateway : IDisposable
{
  private readonly HttpClient _httpClient;
  private bool _disposed = false;

  /// <summary>
  /// Initialize HTTP Gateway with optional custom HttpClient
  /// </summary>
  /// <param name="httpClient">Optional custom HttpClient instance</param>
  public HttpGateway(HttpClient? httpClient = null)
  {
    _httpClient = httpClient ?? new HttpClient();

    // Set default headers
    _httpClient.DefaultRequestHeaders.Add("User-Agent",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
  }

  /// <summary>
  /// Send a POST request with JSON payload
  /// </summary>
  /// <typeparam name="T">Type of the request object</typeparam>
  /// <typeparam name="TResponse">Type of the response object</typeparam>
  /// <param name="url">Request URL</param>
  /// <param name="payload">Object to serialize as JSON</param>
  /// <param name="headers">Optional additional headers</param>
  /// <returns>Deserialized response object</returns>
  public async Task<TResponse?> PostJsonAsync<T, TResponse>(
      string url,
      T payload,
      Dictionary<string, string>? headers = null)
  {
    var json = JsonSerializer.Serialize(payload);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await PostAsync(url, content, headers);
    response.EnsureSuccessStatusCode();

    var responseContent = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<TResponse>(responseContent);
  }

  /// <summary>
  /// Send a POST request with form data
  /// </summary>
  /// <param name="url">Request URL</param>
  /// <param name="formData">Form data as key-value pairs</param>
  /// <param name="headers">Optional additional headers</param>
  /// <returns>HTTP response</returns>
  public async Task<HttpResponseMessage> PostFormAsync(
      string url,
      Dictionary<string, string> formData,
      Dictionary<string, string>? headers = null)
  {
    var content = new FormUrlEncodedContent(formData);
    return await PostAsync(url, content, headers);
  }

  /// <summary>
  /// Send a POST request with multipart form data
  /// </summary>
  /// <param name="url">Request URL</param>
  /// <param name="formData">Form data including files</param>
  /// <param name="headers">Optional additional headers</param>
  /// <returns>HTTP response</returns>
  public async Task<HttpResponseMessage> PostMultipartAsync(
      string url,
      MultipartFormDataContent formData,
      Dictionary<string, string>? headers = null)
  {
    return await PostAsync(url, formData, headers);
  }

  /// <summary>
  /// Send a POST request with raw string content
  /// </summary>
  /// <param name="url">Request URL</param>
  /// <param name="content">Raw content string</param>
  /// <param name="contentType">Content type (default: application/json)</param>
  /// <param name="headers">Optional additional headers</param>
  /// <returns>HTTP response</returns>
  public async Task<HttpResponseMessage> PostRawAsync(
      string url,
      string content,
      string contentType = "application/json",
      Dictionary<string, string>? headers = null)
  {
    var stringContent = new StringContent(content, Encoding.UTF8, contentType);
    return await PostAsync(url, stringContent, headers);
  }

  /// <summary>
  /// Send a POST request with custom HttpContent
  /// </summary>
  /// <param name="url">Request URL</param>
  /// <param name="content">HTTP content</param>
  /// <param name="headers">Optional additional headers</param>
  /// <returns>HTTP response</returns>
  public async Task<HttpResponseMessage> PostAsync(
      string url,
      HttpContent content,
      Dictionary<string, string>? headers = null)
  {
    using var request = new HttpRequestMessage(HttpMethod.Post, url)
    {
      Content = content
    };

    // Add custom headers
    if (headers != null)
    {
      foreach (var header in headers)
      {
        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
      }
    }

    Console.WriteLine($"Sending POST request to: {url}");
    var response = await _httpClient.SendAsync(request);
    Console.WriteLine($"Response status: {response.StatusCode}");

    return response;
  }

  /// <summary>
  /// Set bearer token for authentication
  /// </summary>
  /// <param name="token">Bearer token</param>
  public void SetBearerToken(string token)
  {
    _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
  }

  /// <summary>
  /// Set basic authentication
  /// </summary>
  /// <param name="username">Username</param>
  /// <param name="password">Password</param>
  public void SetBasicAuthentication(string username, string password)
  {
    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
    _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Basic", credentials);
  }

  /// <summary>
  /// Set API key header
  /// </summary>
  /// <param name="headerName">Header name (e.g., "X-API-Key")</param>
  /// <param name="apiKey">API key value</param>
  public void SetApiKey(string headerName, string apiKey)
  {
    _httpClient.DefaultRequestHeaders.Add(headerName, apiKey);
  }

  /// <summary>
  /// Set custom headers
  /// </summary>
  /// <param name="headers">Headers to add</param>
  public void SetHeaders(Dictionary<string, string> headers)
  {
    foreach (var header in headers)
    {
      _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
    }
  }

  /// <summary>
  /// Clear all default headers
  /// </summary>
  public void ClearHeaders()
  {
    _httpClient.DefaultRequestHeaders.Clear();
  }

  /// <summary>
  /// Set timeout for requests
  /// </summary>
  /// <param name="timeout">Timeout duration</param>
  public void SetTimeout(TimeSpan timeout)
  {
    _httpClient.Timeout = timeout;
  }

  /// <summary>
  /// Get response as string
  /// </summary>
  /// <param name="response">HTTP response</param>
  /// <returns>Response content as string</returns>
  public async Task<string> GetResponseStringAsync(HttpResponseMessage response)
  {
    return await response.Content.ReadAsStringAsync();
  }

  /// <summary>
  /// Get response as deserialized object
  /// </summary>
  /// <typeparam name="T">Type to deserialize to</typeparam>
  /// <param name="response">HTTP response</param>
  /// <returns>Deserialized object</returns>
  public async Task<T?> GetResponseObjectAsync<T>(HttpResponseMessage response)
  {
    var content = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<T>(content);
  }

  /// <summary>
  /// Get response headers
  /// </summary>
  /// <param name="response">HTTP response</param>
  /// <returns>Response headers as dictionary</returns>
  public Dictionary<string, string> GetResponseHeaders(HttpResponseMessage response)
  {
    var headers = new Dictionary<string, string>();

    foreach (var header in response.Headers)
    {
      headers[header.Key] = string.Join(", ", header.Value);
    }

    foreach (var header in response.Content.Headers)
    {
      headers[header.Key] = string.Join(", ", header.Value);
    }

    return headers;
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposed && disposing)
    {
      _httpClient?.Dispose();
      _disposed = true;
    }
  }
}