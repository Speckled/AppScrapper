using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;

namespace AppExtractor;

public class BrowserManager : IDisposable
{
  private IBrowser? _browserInstance;
  private bool _disposed = false;

  public BrowserManager()
  {
    // Constructor only initializes the class, actual browser setup happens in InitializeAsync
  }

  /// <summary>
  /// Initialize the browser instance asynchronously
  /// </summary>
  public async Task InitializeAsync()
  {
    if (_browserInstance != null)
      return; // Already initialized

    // Ensure Chrome is downloaded
    var browserFetcher = new BrowserFetcher();
    await browserFetcher.DownloadAsync();

    var extra = new PuppeteerExtra();

    // Enable the Stealth plugin
    extra.Use(new StealthPlugin());

    var launchOptions = new LaunchOptions
    {
      Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" },
      Headless = false // Set to true if you want headless mode
    };

    _browserInstance = await extra.LaunchAsync(launchOptions);
  }

  /// <summary>
  /// Create a new page with default user agent
  /// </summary>
  public async Task<IPage> NewPageAsync()
  {
    if (_browserInstance == null)
      throw new InvalidOperationException("Browser not initialized. Call InitializeAsync() first.");

    var page = await _browserInstance.NewPageAsync();
    await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                                 "AppleWebKit/537.36 (KHTML, like Gecko) " +
                                 "Chrome/120.0.0.0 Safari/537.36");
    return page;
  }

  /// <summary>
  /// Close the browser and dispose resources
  /// </summary>
  public async Task CloseAsync()
  {
    if (_browserInstance != null && !_disposed)
    {
      await _browserInstance.CloseAsync();
    }
  }

  /// <summary>
  /// Check if the browser is initialized
  /// </summary>
  public bool IsInitialized => _browserInstance != null;

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposed && disposing)
    {
      if (_browserInstance != null)
      {
        // Note: CloseAsync should be called explicitly before disposal
        // This is a fallback for synchronous disposal
        _browserInstance.CloseAsync().Wait();
      }
      _disposed = true;
    }
  }
}
