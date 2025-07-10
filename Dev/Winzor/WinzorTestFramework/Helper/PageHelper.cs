using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace WinzorTestFramework;

public static class PageHelper
{
	public static async Task<IPage> LoadPageAsync(Uri uri, int width, int height)
	{
		var page = await GetPageAsync();
		// ensure default Playwright viewport to make tests more deterministic
		await page.SetViewportSizeAsync(width, height);
		var response = await page.GotoAsync(uri.ToString());
		Assert.That(response!.Ok, Is.True);
		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		return page;
	}

	public static async Task<IPage> LoadPageWithScriptAsync(Uri uri, string script)
	{
		var page = await GetPageAsync();
		await page.AddInitScriptAsync(script);
		var response = await page.GotoAsync(uri.ToString());
		Assert.That(response!.Ok, Is.True);
		return page;
	}

	public static async Task<IPage> GetPageAsync()
	{
		CheckForMissingAttribute();
		return Page.Url == "about:blank" ? Page : await BrowserContext.NewPageAsync();
	}

	static void CheckForMissingAttribute()
	{
		if (WithPlaywrightPageAttribute.Context is null)
		{
			Assert.Fail($"To use playwright in this test, add the [{nameof(WithPlaywrightPageAttribute).Replace("Attribute", string.Empty)}] attribute.");
		}
	}
}
