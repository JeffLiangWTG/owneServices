using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting.Helpers;

namespace CargoWise.Blazor.Testing.Common
{
	public static class PageExtensions
	{
		/// <summary>
		/// Click through category > section > subsection button in TileNavigationBar to open the corresponding module page
		/// </summary>
		/// <param name="categoryName">Category name. Eg Maintain</param>
		/// <param name="sectionName">Section name. Eg ReferenceFiles</param>
		/// <param name="subsectionName">Sub section name. Eg Airlines</param>
		public static async Task ClickTileNavAsync(this IPage page, string categoryName, string sectionName, string subsectionName)
		{
			var categoryButtonSelector = $"[id=\"CategoryButton_{categoryName}\"]";
			//fail fast if it looks like the nav is not present (eg. broken or incorrect page) instead of hitting default timeout
			var categoryButton = await page.QuerySelectorAsync(categoryButtonSelector) ?? throw new InvalidOperationException($"TileNav category button not found on {page.Url} (expected {categoryButtonSelector} to be available immediate after first blazor render cycle)");
			await categoryButton.ClickAsync();
			await page.ClickAsync($"[id=\"SectionTile_{sectionName}\"]");
			await page.ClickAsync($"[id=\"SubsectionButton_{subsectionName}\"]");
		}

		/// <summary>
		/// Waits for a complete Blazor render 'cycle' as defined in AppServer App.razor
		/// Use this after opening a page for an indication that the Blazor client/server connection is up running and Blazor is rendering correctly
		/// Sequence: bootstrap > Blazor.start > first render > set flag via JSRuntime > detect flag with WaitForFunction
		/// </summary>
		public static async Task WaitForBlazorAppRenderAsync(this IPage page)
		{
			await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

			const string AfterFirstRenderEventField = "cargoWise.blazorAppIsAfterFirstRender";
			await page.WaitForFunctionAsync(AfterFirstRenderEventField);
		}

		/// <summary>
		/// Wait for the cargoWise testing client to receive a postMessage invocation with a specific message
		/// </summary>
		public static Task WaitForClientApplicationMessagePostAsync(this IPage page, string messageType) => page.WaitForFunctionAsync($"cargoWiseTesting.postMessageParams.find((element) => element.$type == '{messageType}')");

		/// <summary>
		/// Capture a screenshot of the current page and save to %temp%\cw-blazor-test\playwright-screenshots\
		/// </summary>
		/// <param name="label">Optional string to include in screenshot filename</param>
		public static async Task<string> ScreenshotToTempAsync(this IPage page, string label = null)
		{
			var pageTitle = await page.TitleAsync();
			var labelSegment = (string.IsNullOrEmpty(label) ? string.Empty : $"{label}_");
			var fileName = $"{labelSegment}{pageTitle}_{Path.GetRandomFileName()}.png".ReplaceInvalidFileNameChars();
#pragma warning disable EDI011 // Temp Path Rule
			var filePath = Path.Combine(Path.GetTempPath(), "cw-blazor-test", "playwright-screenshots", fileName);
#pragma warning restore EDI011 // Temp Path Rule
			await page.ScreenshotAsync(new PageScreenshotOptions { Path = filePath, FullPage = true });
			return filePath;
		}

		/// <summary>
		/// Attach a screenshot to the test output (for local debugging only, will fail in DAT)
		/// </summary>
		public static async Task DebugScreenshotAsync(this IPage page)
		{
			if (TestEnvironment.IsDatTesting)
			{
				throw new InvalidOperationException($"This is not intended for use in DAT");
			}

			var filePath = await page.ScreenshotToTempAsync(TestContext.CurrentContext.Test.Name);
			TestContext.AddTestAttachment(filePath, $"{Path.GetFileName(filePath)}");
		}
	}
}
