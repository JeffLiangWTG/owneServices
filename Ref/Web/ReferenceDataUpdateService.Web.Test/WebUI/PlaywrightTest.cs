using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Playwright;
using NUnit.Framework;

namespace ReferenceDataUpdateService.Web.Test.WebUI
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	[CreateDatabase("42E84E5D04B342AF860AB398AD2D657E", DbSchema.RefDbRepoSafe, "9879243546C341CABECCEFCB10F381FA", DbSchema.RefDbRepoStaging)]
	[CreateStagingService("9879243546C341CABECCEFCB10F381FA")]
	[CreateSafeDataUpdateService("42E84E5D04B342AF860AB398AD2D657E", ActionTargets.Test)]
	[CreateWebPortalService]
	public abstract class PlaywrightTest
	{
		private IPlaywright Playwright { get; set; }
		private IBrowser Browser { get; set; }
		protected IPage Page { get; private set; }

		[SetUp]
		public virtual async Task SetUp()
		{
			// Configure the path so Playwright can find the browsers
			// https://devops.wisetechglobal.com/wtg/RefDataRepo/_wiki/wikis/RefDataRepo.wiki/15330/DAT-vs-Local-environment
			string assemblyPath = Assembly.GetExecutingAssembly().Location;
			string directoryPath = Path.GetDirectoryName(assemblyPath);
			var browsersPath = Path.Join(directoryPath, "playwright-browsers");
			Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", browsersPath);

			Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
			Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

			Page = await Browser.NewPageAsync();
			Page.Console += (_, msg) => Console.WriteLine($"Browser console message: {msg.Text}");

			await Page.GotoAsync("http://localhost:19488");
		}

		[TearDown]
		public virtual async Task TearDown()
		{
			await Page.CloseAsync();
			await Browser.CloseAsync();
			Playwright.Dispose();
		}
	}
}
