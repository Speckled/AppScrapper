using PuppeteerSharp;

namespace AppExtractor.Extensions;
/// <summary>
/// Extension methods for IPage to add site-specific functionality
/// </summary>
public static class PageExtensions
{
  /// <summary>
  /// Login to LA City ePlan system
  /// </summary>
  /// <param name="page">The page instance</param>
  /// <param name="username">Username for login</param>
  /// <param name="password">Password for login</param>
  /// <param name="timeout">Timeout in milliseconds (default: 30000)</param>
  /// <returns>True if login was successful, false otherwise</returns>
  public static async Task<bool> LoginToLACityEPlanAsync(this IPage page, string username, string password, int timeout = 30000)
  {
    try
    {
      var usernameSelector = "input[name='username']";
      var passwordSelector = "input[name='password']";
      var buttonContinueSelector = "button";
      var loggedSelector = ".ArchLabel";

      await page.WaitForSelectorAsync(usernameSelector, new WaitForSelectorOptions { Timeout = timeout });
      await page.ScreenshotAsync("2_after_wait_username.png");
      await page.WaitForSelectorAsync(buttonContinueSelector, new WaitForSelectorOptions { Timeout = timeout });
      await page.TypeAsync(usernameSelector, username);

      var buttons = await page.QuerySelectorAllAsync(buttonContinueSelector);
      foreach (var btn in buttons)
      {
        var btnText = (await (await btn.GetPropertyAsync("innerText")).JsonValueAsync<string>()).Trim().ToLower();
        if (btnText == "continue" || btnText == "next" || btnText == "sign in" || btnText == "submit" || btnText == "log in")
        {
          await btn.ClickAsync();
        }
      }

      await page.WaitForNavigationAsync(new NavigationOptions { Timeout = timeout });
      await page.InspectPageAsync();
      await page.WaitForSelectorAsync(passwordSelector, new WaitForSelectorOptions { Timeout = timeout });
      await page.TypeAsync(passwordSelector, password);
      await page.ScreenshotAsync("4_after_type_password.png");

      buttons = await page.QuerySelectorAllAsync(buttonContinueSelector);

      foreach (var btn in buttons)
      {
        var btnText = (await (await btn.GetPropertyAsync("innerText")).JsonValueAsync<string>()).Trim().ToLower();
        if (btnText == "continue" || btnText == "next" || btnText == "sign in" || btnText == "submit" || btnText == "log in")
        {
          await btn.ClickAsync();
        }
      }

      await page.WaitForNavigationAsync(new NavigationOptions { Timeout = timeout });
      await page.WaitForSelectorAsync(loggedSelector, new WaitForSelectorOptions { Timeout = timeout });

      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Login failed: {ex.Message}");
      return false;
    }
  }

  public static async Task<List<Project>> RetrieveProjectsData(this IPage page, string archivedStatus)
  {
    var projects = new List<Project>();
    var projectSelector = ".searchField";

    try
    {
      var projectElements = await page.QuerySelectorAllAsync(projectSelector);
      foreach (var project in projectElements)
      {
        var innerTextHandle = await project.GetPropertyAsync("innerText");
        var projectInfo = await innerTextHandle.JsonValueAsync<string>();

        projects.Add(new Project(projectInfo, archivedStatus));
      }

      return projects;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Failed to retrieve projects data: {ex.Message}");
      return [];
    }
  }

  /// <summary>
  /// Inspect page content to debug what elements are available
  /// </summary>
  /// <param name="page">The page instance</param>
  /// <returns>Task</returns>
  public static async Task InspectPageAsync(this IPage page)
  {
    try
    {
      Console.WriteLine("=== PAGE INSPECTION ===");
      var pageInfo = await page.EvaluateExpressionAsync<string>(@"
        (() => {
          const info = {
            title: document.title,
            url: window.location.href,
            inputs: Array.from(document.querySelectorAll('input')).map(inp => ({
              type: inp.type,
              name: inp.name,
              id: inp.id,
              placeholder: inp.placeholder,
              className: inp.className
            })),
            buttons: Array.from(document.querySelectorAll('button')).map(btn => ({
              type: btn.type,
              name: btn.name,
              id: btn.id,
              text: btn.textContent?.trim(),
              className: btn.className
            })),
            checkboxes: Array.from(document.querySelectorAll('input[type=""checkbox""]')).map(cb => ({
              name: cb.name,
              id: cb.id,
              checked: cb.checked,
              className: cb.className,
              parentText: cb.parentElement?.textContent?.trim()
            })),
            bodyText: document.body.textContent?.includes('human') ? 'Contains human text' : 'No human text found'
          };
          return JSON.stringify(info, null, 2);
        })()
      ");

      Console.WriteLine($"Page Info:\n{pageInfo}");
      Console.WriteLine("=== END INSPECTION ===");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Page inspection failed: {ex.Message}");
    }
  }

}