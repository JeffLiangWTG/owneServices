using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Common
{
	public class WebScraper : IWebScraper
	{
		private readonly IPlaywright _playwright;
		private readonly IBrowser _browser;

		private WebScraper(IPlaywright playwright, IBrowser browser)
		{
			_playwright = playwright;
			_browser = browser;
		}

		public static async Task<IWebScraper> CreateAsync()
		{
			string assemblyPath = Assembly.GetExecutingAssembly().Location;
			string directoryPath = Path.GetDirectoryName(assemblyPath);
			var browsersPath = Path.Join(directoryPath, "playwright-browsers");
			Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", browsersPath);

			var playwright = await Playwright.CreateAsync();

			var launchOptions = new BrowserTypeLaunchOptions
			{
				Headless = true,
				Proxy = new Proxy
				{
					Server = Constants.Smartproxy.ProxyUrl,
					Username = ConfigurationProvider.ProxyUser,
					Password = ConfigurationProvider.ProxyPassword
				}
			};

			var browser = await playwright.Firefox.LaunchAsync(launchOptions);

			return new WebScraper(playwright, browser);
		}

		public async Task<string> ScrapeWithRetriesAsync(Uri uri, int maxAttempts = Constants.WebSraper.DefaultMaxAttempts)
		{
			for (int attempt = 1; attempt <= maxAttempts; attempt++)
			{
				try
				{
					var result = await ScrapePageAsync(uri);
					if (result != null)
					{
						return result;
					}
				}
				catch (Exception ex) when (ex is PlaywrightException || ex is TimeoutException)
				{
					Console.WriteLine($"Attempt {attempt} failed for {uri}: {ex.Message}");
					if (attempt == maxAttempts)
					{
						Console.WriteLine($"Failed to scrape {uri} - {ex.Message}");
					}
					await Task.Delay(CommonHelper.RandomInteger(100, 1000));
				}
			}
			return null;
		}

		async Task<string> ScrapePageAsync(Uri uri)
		{
			await using var context = await _browser.NewContextAsync();

			await context.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
			{
				{ "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/111.0.0.0 Safari/537.36" },
				{ "Accept-Language", "en-US,en;q=0.9" }
			});

			var page = await context.NewPageAsync();

			try
			{
				var response = await page.GotoAsync(uri.ToString(), new PageGotoOptions { Timeout = 15000 });
				Console.WriteLine($"Navigated to {uri} - Status: {response?.Status}");

				return await page.ContentAsync();
			}

			finally
			{
				await page.CloseAsync();
			}
		}

		public async ValueTask DisposeAsync()
		{
			if (_browser != null)
			{
				await _browser.CloseAsync();
			}

			_playwright?.Dispose();
		}
	}
}
