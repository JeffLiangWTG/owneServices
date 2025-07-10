using System;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

public class ZFilterStripTest
{
	const string dropEditSelector = "div[data-type='Enterprise.ZArchitecture.GUI.Internal.ZFilterStripDropEdit']";

	[Test]
	public async Task ChangeFilterStripShouldMatchInput()
	{
		ZFilterStrip zStrip = null;
		ModuleFilterCollection filters = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			filters = new MockFilterStripBizO().GetModuleFilters();
			var strip = new FilterStrip(filters);
			zStrip = new ZFilterStrip();  
			zStrip.SetDataBinding(strip, "");
			return zStrip;
		});

		for (var i = 0; i < filters.Filter_List.Count; i++)
		{
			var filter = filters.Filter_List[i];
			if (filter is ModuleFilter)
			{
				await rendered.Find("button").MouseDownAsync(new WebMouseEventArgs() { Detail = 1 });
				await rendered.FindAll($".zdropform tr")[i].ClickAsync(new WebMouseEventArgs());
				Assert.That(rendered.Find("input").Attributes["value"]?.Value, Is.EqualTo(filter.Description));
			}
		}
	}

	[Test, WithPlaywrightPage]
	public async Task DateRangeFilterStripZIndex()
	{
		ZFilterStrip zStrip = null;
		ModuleFilterCollection filters = null;

		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			filters = new MockFilterStripBizO().GetModuleFilters();
			var strip = new FilterStrip(filters);
			zStrip = new ZFilterStrip();
			zStrip.SetDataBinding(strip, "");
			return zStrip;
		});

		await page.Locator(".zdropcodebox").PressAsync("ArrowDown");

		var index = filters.Filter_List.IndexOfCode("Date Range");
		await page.WaitForSelectorAsync(".zdropform tr");
		await (await page.QuerySelectorAllAsync(".zdropform tr"))[index].ClickAsync();

		var dropEdit = await page.WaitForSelectorAsync(dropEditSelector);
		Assert.That(async () => await dropEdit.EvaluateAsync<string>("element => window.getComputedStyle(element).zIndex"), Is.EqualTo("3").After(3000, 100));

		await page.Locator(".zdropcodebox").First.PressAsync("ArrowDown");
		Assert.That(async () => await dropEdit.EvaluateAsync<string>("element => window.getComputedStyle(element).zIndex"), Is.EqualTo(short.MaxValue.ToString()).After(3000, 100));

		await page.EvaluateAsync("document.activeElement.blur();");
		Assert.That(async () => await dropEdit.EvaluateAsync<string>("element => window.getComputedStyle(element).zIndex"), Is.EqualTo("3").After(3000, 100));

		var gripHolderPanel = await page.EvaluateAsync<int>("document.elementsFromPoint(0, 3).findIndex(e => e.getAttribute('data-type') === 'CargoWise.Windows.UI.KPanel')");
		Assert.That(gripHolderPanel, Is.Not.EqualTo(-1));
		var codeMappingFilter = await page.EvaluateAsync<int>("document.elementsFromPoint(3, 3).findIndex(e => e.getAttribute('data-name') === 'ZFilterStrip')");
		Assert.That(codeMappingFilter, Is.Not.EqualTo(-1));
		Assert.That(gripHolderPanel, Is.LessThan(codeMappingFilter));
	}

	[Test, WithPlaywrightPage]
	public async Task NumberRangeFilterStripZIndex()
	{
		ZFilterStrip zStrip = null;
		ModuleFilterCollection filters = null;

		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);
		var page = await ctx.LoadFormAsync(() =>
		{
			filters = new MockFilterStripBizO().GetModuleFilters();
			var strip = new FilterStrip(filters);
			var form = new WinzorTestForm();
			zStrip = new ZFilterStrip();
			zStrip.SetDataBinding(strip, "");
			form.Controls.Add(zStrip);
			return form;
		});
		var button = await page.WaitForSelectorAsync("button");
		await button.ClickAsync();

		var index = filters.Filter_List.IndexOfCode("Number Range (Short)");
		await page.WaitForSelectorAsync(".zdropform tr");
		await (await page.QuerySelectorAllAsync(".zdropform tr"))[index].ClickAsync();

		var numberRangeControl = await page.WaitForSelectorAsync("[data-name=\"NumberRangeControl\"]");
		var removeButton = await page.WaitForSelectorAsync("body > div.form > div.zfilterstrip > button:nth-child(3)");
		var lockButton = await page.WaitForSelectorAsync("body > div.form > div.zfilterstrip > button:nth-child(4)");

		var zIndexNumberRange = await numberRangeControl.EvaluateAsync<string>("element => window.getComputedStyle(element).zIndex");
		var zIndexRemoveButton = await removeButton.EvaluateAsync<string>("element => window.getComputedStyle(element).zIndex");
		var zIndexLockButton = await lockButton.EvaluateAsync<string>("element => window.getComputedStyle(element).zIndex");

		if (zIndexNumberRange == "auto")
		{
			zIndexNumberRange = "0";
		}

		Assert.That(zIndexRemoveButton, Is.Not.EqualTo("auto"));
		Assert.That(zIndexLockButton, Is.Not.EqualTo("auto"));
		Assert.That(int.Parse(zIndexRemoveButton), Is.GreaterThan(int.Parse(zIndexNumberRange)));
		Assert.That(int.Parse(zIndexLockButton), Is.GreaterThan(int.Parse(zIndexNumberRange)));
	}

	[Test, WithPlaywrightPage]
	[TestCase(false, null, "rgb(109, 109, 109)")]
	[TestCase(true, "button", "rgb(0, 0, 255)")]
	[TestCase(false, "button", "rgb(0, 0, 0)")]
	public async Task ZDropCodeBoxShouldInheritItsColorFromZFilterStripDropEdit(bool isExclusiveHelper, string buttonSelector, string expectedColor)
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);
		ZFilterStrip zStrip = null;
		ModuleFilterCollection filters = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			filters = new MockFilterStripBizO().GetModuleFilters();
			filters["Date Range"].IsExclusiveHelper = isExclusiveHelper;
			var strip = new FilterStrip(filters);
			var form = new WinzorTestForm();
			zStrip = new ZFilterStrip();
			zStrip.SetDataBinding(strip, "");
			form.Controls.Add(zStrip);
			return form;
		});

		if (buttonSelector != null)
		{
			var button = await page.WaitForSelectorAsync(buttonSelector);
			await button.ClickAsync();

			var index = filters.Filter_List.IndexOfCode("Date Range");
			await page.WaitForSelectorAsync(".zdropform tr");
			await (await page.QuerySelectorAllAsync(".zdropform tr"))[index].ClickAsync();
		}

		var filterStripDropEdit = await page.WaitForSelectorAsync(dropEditSelector);
		var dropCodeBox = await page.WaitForSelectorAsync(".zdropcodebox");

		Assert.That(async () => await filterStripDropEdit.EvaluateAsync<string>("element => window.getComputedStyle(element).color"), Is.EqualTo(expectedColor).After(3000, 100));
		Assert.That(async () => await dropCodeBox.EvaluateAsync<string>("element => window.getComputedStyle(element).color"), Is.EqualTo(expectedColor).After(3000, 100));
	} 

	[Test, WithPlaywrightPage]
	public async Task ZFilterStripDragDropToTheCorrectPosition()
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);

		var page = await ctx.CaptureFormShowAndLoadAsync(() =>
		{
			var form = new MockFilterStripForm(new MockFilterStripBizO());
			const string city = "City";
			for (var i = 0; i < 20; i++)
			{
				form.AddFilterStrip(city);
			}

			form.Show();
		});

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		var filterStripsPanel = await page.WaitForSelectorAsync("[data-name='FilterStripsPanel']");
		var filterStripLocator = page.Locator(".zfilterstrip");
		await filterStripsPanel.EvaluateAsync("e => e.scrollTo(0, 50)");

		Assert.That(await filterStripLocator.CountAsync(), Is.EqualTo(21));
		Assert.That(await filterStripLocator.Nth(4).EvaluateAsync<int>("e => e.offsetTop"), Is.EqualTo(95));
		Assert.That(await filterStripLocator.Nth(8).EvaluateAsync<int>("e => e.offsetTop"), Is.EqualTo(187));

		var gripHolder4 = filterStripLocator.Nth(4).Locator("[data-name='GripHolderPanel']");
		var gripHolder8 = filterStripLocator.Nth(8).Locator("[data-name='GripHolderPanel']");

		await ElementMouseMoveAsync(page, gripHolder4, 2, 2);
		await page.Mouse.DownAsync();
		await ElementMouseMoveAsync(page, gripHolder8, 2, 2);
		await page.Mouse.UpAsync();

		Assert.That(async () => await filterStripLocator.Nth(4).EvaluateAsync<int>("e => e.offsetTop"), Is.EqualTo(187).After(3000, 100));
		Assert.That(async () => await filterStripLocator.Nth(8).EvaluateAsync<int>("e => e.offsetTop"), Is.EqualTo(164).After(3000, 100));
		Assert.That(async () => await filterStripLocator.Nth(8).GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(220, 225, 228)").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestEmptyZFilterStripTabbing()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new ZFilterStrip());
		var input = page.Locator("[data-name=FilterDescriptionDropEdit] input");
		await input.ClickAsync();
		await page.Keyboard.PressAsync("Tab");
		var focus = page.Locator("button:focus");
		Assert.That(await focus.GetAttributeAsync("title"), Is.EqualTo("Remove Filter"));
	}

	[TestCase("Tex", "Text", "zdropcodebox textbox", "starts with")]
	[TestCase("Text R", "Text Range", "textbox", "")]
	[TestCase("Date R", "Date Range", "zdropcodebox textbox", "")]
	[TestCase("Single D", "Single Date", "textbox", "")]
	[TestCase("Time R", "Time Range", "zdropcodebox textbox", "")]
	[TestCase("Custom S", "Custom SQL Filter", "textbox", "")]
	[TestCase("Flags F", "Flags Filter", "checkbox__input", null)]
	[WithPlaywrightPage]
	public async Task TestEmptyZFilterStripTabbingChangeFilter(string inputText, string filterTitle, string targetClass, string targetValue)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() => new MockFilterStripForm(new MockFilterStripBizO()) { Width = 1000, Height = 1000 });

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		var input = page.GetByRole(AriaRole.Textbox);
		await Assertions.Expect(input).ToBeFocusedAsync();
		await input.PressSequentiallyAsync(inputText, new () { Delay = 300 });
		await input.WaitForAsync();
		await page.Keyboard.PressAsync("Tab");

		switch (targetClass)
		{
			case var str when str.Contains("checkbox__input"):
				await page.GetByRole(AriaRole.Checkbox).First.WaitForAsync(new() { Timeout = 8000 });
				break;
			default:
				await page.GetByRole(AriaRole.Textbox).Nth(1).WaitForAsync(new() { Timeout = 8000 });
				break;
		}

		var focus = page.Locator("*:focus");
		Assert.That(() => focus.GetAttributeAsync("class"), Does.Contain(targetClass).After(3000, 100));
		if (targetValue is not null)
		{
			await Assertions.Expect(focus).ToHaveValueAsync(targetValue);
		}
	}

	[Test, WithPlaywrightPage]
	public async Task TestZFilterStripTabbing()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZFilterStrip strip = null;
		var page = await ctx.CaptureFormShowAndLoadAsync(() =>
		{
			var form = new MockFilterStripForm(new MockFilterStripBizO());
			strip = form.AddFilterStrip("Text");
			form.Show();
		});
		
		var input = page.Locator("[data-name=FilterDescriptionDropEdit] input").Last;
		await input.ClickAsync();
		await page.Keyboard.PressAsync("Tab");
		var focus = page.Locator("input:focus");
		Assert.That(await focus.InputValueAsync(), Is.EqualTo("starts with"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestZFilterStripTabbingChangeFilter()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.CaptureFormShowAndLoadAsync(() =>
		{
			var form = new MockFilterStripForm(new MockFilterStripBizO());
			form.AddFilterStrip("Text");
			form.Show();
		});

		var inputLocator = page.Locator(".zfilterstrip:nth-child(2) [data-name=FilterDescriptionDropEdit] input");
		await inputLocator.FillAsync("Tim");
		await page.Locator("[title='Time Range']:has(.selected)").WaitForAsync(new () { Timeout = 3000 });
		await page.Keyboard.PressAsync("Tab");
		Assert.That(async () => await inputLocator.InputValueAsync(), Is.EqualTo("Time Range").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ZFilterStripDragDropBetweenGroups()
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);

		GroupStripControl groupControlFirst = null;
		GroupStripControl groupControlSecond = null;

		var page = await ctx.CaptureFormShowAndLoadAsync(() =>
		{
			var form = new MockFilterStripForm(new MockFilterStripBizO());
			form.Width = 1200;

			groupControlFirst = form.AddGroupStripControl("Group 1");
			groupControlSecond = form.AddGroupStripControl("Group 2");

			form.AddFilterStrip("Auckland", groupControlFirst);
			form.AddFilterStrip("Wellington", groupControlFirst);

			form.AddFilterStrip("Sydney", groupControlSecond);
			form.AddFilterStrip("Brisbane", groupControlSecond);

			form.Show();
		});
		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		var filterStripLocator = page.Locator(".zfilterstrip");
		var filterStripsPanel = await page.WaitForSelectorAsync("[data-name='FilterStripsPanel']");
		var gripPanel = await page.WaitForSelectorAsync("[data-name='GripHolderPanel']");

		var childCount1 = groupControlFirst.Children[3].Children.OfType<ZFilterStrip>().ToArray().Length;
		var childCount2 = groupControlSecond.Children[3].Children.OfType<ZFilterStrip>().ToArray().Length;

		Assert.That(childCount1, Is.EqualTo(2));
		Assert.That(childCount2, Is.EqualTo(2));

		var gripHolder1 = filterStripLocator.Nth(1).Locator("[data-name='GripHolderPanel']");
		var gripHolder4 = filterStripLocator.Nth(4).Locator("[data-name='GripHolderPanel']");

		await ElementMouseMoveAsync(page, gripHolder1, 2, 2);
		await page.Mouse.DownAsync();
		await ElementMouseMoveAsync(page, gripHolder4, 2, 2);
		await page.Mouse.UpAsync();

		Assert.That(() => groupControlFirst.Children[3].Children.OfType<ZFilterStrip>().ToArray(), Has.Length.EqualTo(1).After(3000, 100));
		Assert.That(() => groupControlSecond.Children[3].Children.OfType<ZFilterStrip>().ToArray(), Has.Length.EqualTo(3).After(3000, 100));
	}

	async Task ElementMouseMoveAsync(IPage page, ILocator locator, float x, float y, int steps = 10)
	{
		var elementRect = await locator.BoundingBoxAsync();
		await page.Mouse.MoveAsync(elementRect.X + x, elementRect.Y + y, new () { Steps = steps });
	}
}
