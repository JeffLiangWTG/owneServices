using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.SearchBox;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;
class ZSearchBoxTest
{
	[Test]
	public async Task ResultsAreDismissedWhenCloseButtonClicked()
	{
		using var ctx = new EnterpriseTestContext();
		var tcs = new TaskCompletionSource<bool>();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var searchBox = new ZArchitecture.GUI.SearchBox.ZSearchBox();
			searchBox.AutoSearch = true;
			searchBox.Search = _ =>
			{
				return new[] { DisplayItemFactory.CreateSearchItem(() => { }, "Hello", "World") };
			};
			searchBox.SearchResultsDisplay.VisibleChanged += (s, e) =>
			{
				var control = s as Control;
				if (control.Visible)
				{
					tcs.SetResult(true);
				}
			};
			return searchBox;
		});

		var input = rendered.Find("input");
		await input.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "H" });
		Assert.That(await tcs.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);

		var searchResults = rendered.WaitForElement("ul", TimeSpan.FromSeconds(3));

		Assert.That(searchResults, Is.Not.Null);

		var searchButton = rendered.Find("button");

		await searchButton.ClickAsync(new WebMouseEventArgs());

		Assert.That(() => rendered.FindAll("ul"), Is.Empty);
	}

	[Test]
	public async Task ResultsAreDismissedWhenAnotherElementGainsFocus()
	{
		using var ctx = new EnterpriseTestContext();
		ZButton button = null;
		var tcs = new TaskCompletionSource<bool>();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			button = new ZButton();
			var searchBox = new ZArchitecture.GUI.SearchBox.ZSearchBox();
			searchBox.AutoSearch = true;
			searchBox.Search = (text) =>
			{
				return new[] { DisplayItemFactory.CreateSearchItem(() => { }, "Hello", "World") };
			};
			searchBox.SearchResultsDisplay.VisibleChanged += (s, e) =>
			{
				var control = s as Control;
				if (control.Visible)
				{
					tcs.SetResult(true);
				}
			};
			form.Controls.Add(searchBox);
			form.Controls.Add(button);
			return form;
		});

		var input = rendered.Find("input");
		await input.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "H" });
		Assert.That(await tcs.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);

		var searchResults = rendered.WaitForElement("ul", TimeSpan.FromSeconds(3));

		Assert.That(searchResults, Is.Not.Null);

		var otherButton = rendered.Find($"[data-winzor-control-id=\"{button.WinzorControlId}\"]");

		await otherButton.ClickAsync(new WebMouseEventArgs());

		Assert.That(() => rendered.FindAll("ul"), Is.Empty);
	}

	[Test, WithPlaywrightPage]
	public async Task SearchButtonTextIsVerticallyCentered()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZArchitecture.GUI.SearchBox.ZSearchBox searchBox = null;
		var page = await ctx.LoadControlOnFormAsync(() => searchBox = new ZArchitecture.GUI.SearchBox.ZSearchBox());

		var expectedButton = await page.WaitForSelectorAsync($"[data-winzor-control-id=\"{searchBox.WinzorControlId}\"]");
		Assert.That(expectedButton, Is.Not.Null);

		var expectedButtonTextDiv = await page.WaitForSelectorAsync(".button__text");
		Assert.That(await expectedButtonTextDiv.GetComputedStyleAsync("align-items"), Is.EqualTo("self-end"));
		Assert.That(await expectedButtonTextDiv.GetComputedStyleAsync("display"), Is.EqualTo("flex"));
		Assert.That(await expectedButtonTextDiv.GetComputedStyleAsync("justify-content"), Is.EqualTo("end"));
	}
}
