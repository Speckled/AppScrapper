using PuppeteerSharp;
using AppExtractor.Extensions;
using AppExtractor.Gateway;
using AppExtractor;

var email = "Info@usfireinc.com";
var password = "11589Tuxford";

// Initialize browser using the BrowserManager class
using var browserManager = new BrowserManager();
await browserManager.InitializeAsync();

// Create a new page
var page = await browserManager.NewPageAsync();

await page.GoToAsync("https://eplanla.lacity.org/dashboard/dashboard");
Console.WriteLine("Wait for load");
Console.WriteLine("Attempting to login...");

var loginSuccess = await page.LoginToLACityEPlanAsync(email, password);

if (!loginSuccess)
{
  Console.WriteLine("Login failed!");
  return;
}
await page.ScreenshotAsync("5_after_login.png");

var archivedPage = await browserManager.NewPageAsync();
await archivedPage.GoToAsync("https://eplanla.lacity.org/Dashboard/Dashboard?Arc=True");
Console.WriteLine("Wait for load of archived page");

var projectsData = await page.RetrieveProjectsData("Active");



// Using HttpGateway to send the projects data as JSON
using var httpGateway = new HttpGateway();

Console.WriteLine($"Retrieved {projectsData.Count} projects from the page");

try
{
  Console.WriteLine("Sending projects data as JSON...");

  // Replace with your actual API endpoint
  var apiEndpoint = "https://script.google.com/macros/s/AKfycbwWV7Az6QCe-obKRk0p1ilj83UzZ_GAE9vmaCH0aMhsrU5jzdBHbz7C1NEfw63q4DA/exec";

  // Send the projects data using the extension method
  await projectsData.SendProjectsDataAsync(
      apiEndpoint,
      httpGateway,
      email,
      page.Url
  );

}
catch (HttpRequestException httpEx)
{
  Console.WriteLine($"❌ HTTP Error sending projects: {httpEx.Message}");
}
catch (Exception ex)
{
  Console.WriteLine($"❌ Error sending projects: {ex.Message}");
}

Console.WriteLine("Login successful!");
