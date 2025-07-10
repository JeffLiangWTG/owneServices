using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Microsoft.JSInterop;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

sealed class SafeClipboardTest
{
	[Explicit("Clipboard is not working when the screen is locked (DAT case). There is no solution or workaround.")]
	[Test, WithPlaywrightPage(Headless = false)]
	public async Task TestFetchDataFromClipboard()
	{
		const string ClipboardJS = "/_content/WinzorFramework/js/module/clipboard.js";

		await using var ctx = new InMemoryAppServerTestContext();
		var form = default(Form);
		await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-write", "clipboard-read" });

		var clickTask = new TaskCompletionSource();

		Dictionary<string, string> clipboardItems = new Dictionary<string, string>()
		{
			["Text"] = "WI0000000679 GNA Header",
			["Html"] = "< a href = edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=WorkItem&BusinessEntityPK=1e9eea51-d432-4949-8120-a1e4457bfe2f&Hash=%2b53QFqJfjLqiPajGJBD4Ddwgw%2fPkuTg9X > WI00000003 - MY  Process Header < / a >",
		};

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var button = new Button
			{
				Text = "Click To Fetch Clipboard Value",
			};

#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
			button.Click += async void (s, e) =>
			{
				var dotNetReference = DotNetObjectReference.Create(form.CargoWiseClientServices);
				var module = await form.CargoWiseClientServices.JSRuntime.InvokeAsync<IJSObjectReference>("import", ClipboardJS);
				await module.InvokeVoidAsync("clipboard.setDataObject", dotNetReference, clipboardItems, false);
				await SafeClipboard.FetchClipboardDataAsync(form.CargoWiseClientServices.JSRuntime);
				clickTask.SetResult();
			};
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates

			form.Controls.Add(button);
			return form;
		});

		var button = page.Locator("button");
		await button.ClickAsync();

		await clickTask.Task;

		var fetchData = SafeClipboard.GetDataObjectDict();

		fetchData["Html"] = WebUtility.HtmlDecode(fetchData["Html"]);

		Assert.That(clipboardItems, Is.EqualTo(fetchData));
	}
}
