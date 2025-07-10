using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using WTG.RtfConverter.Html;

namespace System.Windows.Forms;

public class CheckedListBoxTest
{
	[Test]
	public async Task CheckedListBoxRenderred()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, checkedListBox) = await TestCheckedListBoxAsync(ctx);
		Assert.That(rendered.FindAll(".checkedlistbox").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll("span").Select(tr => tr.InnerHtml), Is.EqualTo(new string[] { "item1", "item2", "item3", "item4" }));
	}

	[TestCase(false, "display: flex;flex-direction: column;overflow-y:")]
	[TestCase(true, "display: flex;flex-direction: column;flex-wrap: wrap;overflow-x:")]
	public async Task CheckedListBoxMultiColumnStyle(bool multiColumn, string expectString)
	{
		using var ctx = new WinzorTestContext();
		CheckedListBox newCheckedListBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			newCheckedListBox = new CheckedListBox();
			newCheckedListBox.MultiColumn = multiColumn;
			var list = new string[] { "item1", "item2", "item3", "item4" };
			foreach (var item in list)
			{
				newCheckedListBox.Items.Add(item);
			}

			return newCheckedListBox;
		});
		var checkedlistbox = rendered.WaitForElement(".checkedlistbox");
		var styleString = checkedlistbox.GetAttribute("style");
		Assert.That(styleString, Does.Contain(expectString));
	}

	[TestCase(false, "overflow-y: auto;")]
	[TestCase(true, "overflow-y: scroll;")]
	public async Task CheckedListBoxScrollAlwaysVisibleStyle(bool scrollAlwaysVisible, string expectString)
	{
		using var ctx = new WinzorTestContext();
		CheckedListBox newCheckedListBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			newCheckedListBox = new CheckedListBox();
			newCheckedListBox.ScrollAlwaysVisible = scrollAlwaysVisible;
			var list = new string[] { "item1", "item2", "item3", "item4" };
			foreach (var item in list)
			{
				newCheckedListBox.Items.Add(item);
			}

			return newCheckedListBox;
		});
		var checkedlistbox = rendered.WaitForElement(".checkedlistbox");
		var styleString = checkedlistbox.GetAttribute("style");
		Assert.That(styleString, Does.Contain(expectString));
	}

	[TestCase(true, 0, "min-width: 150px;max-width: 150px;")]
	[TestCase(true, 234, "min-width: 234px;max-width: 234px;")]
	[TestCase(false, 0, null)]
	public async Task CheckedListBoxColumnWidthStyle(bool multiColumn, int columnWidth, string expectStyleString)
	{
		using var ctx = new WinzorTestContext();
		CheckedListBox newCheckedListBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			newCheckedListBox = new CheckedListBox();
			newCheckedListBox.MultiColumn = multiColumn;
			newCheckedListBox.ColumnWidth = columnWidth;
			var list = new string[] { "item1" };
			foreach (var item in list)
			{
				newCheckedListBox.Items.Add(item);
			}

			return newCheckedListBox;
		});
		var checkedlistbox = rendered.WaitForElement(".checkedlistbox__label");
		var styleString = checkedlistbox.GetAttribute("style");
		if (expectStyleString != null)
		{
			Assert.That(styleString, Does.Contain(expectStyleString));
		}
		else
		{
			Assert.That(styleString, Does.Not.Contain("min-width:"));
			Assert.That(styleString, Does.Not.Contain("max-width:"));
		}
	}

	[Test]
	public async Task CheckedListBoxAddOperations()
	{
		using var ctx = new WinzorTestContext();
		CheckedListBox newCheckedListBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			newCheckedListBox = new CheckedListBox();
			var list = new string[] { "item1", "item2", "item3", "item4" };
			foreach (var item in list)
			{
				newCheckedListBox.Items.Add(item);
			}

			return newCheckedListBox;
		});

		Assert.That(rendered.FindAll(".checkedlistbox__label").Count, Is.EqualTo(4));
		Assert.That(rendered.FindAll("span").Count, Is.EqualTo(4));
		Assert.That(rendered.FindAll("span").Select(tr => tr.InnerHtml), Is.EqualTo(new string[] { "item1", "item2", "item3", "item4" }));
	}

	[Test]
	public async Task CheckedListBoxSelectedItemStyleCount()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestCheckedListBoxAsync(ctx);

		var list = rendered.FindAll(".checkedlistbox__input");
		if (list.Count > 0)
		{
			await list[0].ClickAsync(new WebMouseEventArgs());
		}
		Assert.That(rendered.FindAll(".checkedlistbox__item--selected").Count, Is.EqualTo(1));
	}

	[Test]
	public async Task CheckOnClick()
	{
		using var ctx = new WinzorTestContext();
		CheckedListBox newCheckedListBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			newCheckedListBox = new CheckedListBox { CheckOnClick = true };
			newCheckedListBox.Items.Add("item", true);

			return newCheckedListBox;
		});

		await rendered.Find(".checkedlistbox__input")
			.ClickAsync(new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".checkedlistbox__input:checked").Count, Is.EqualTo(0));
	}

	[Test]
	public async Task CheckedListBoxNotFireClickEventIfDisabled()
	{
		using var ctx = new WinzorTestContext();
		CheckedListBox newCheckedListBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			newCheckedListBox = new CheckedListBox { CheckOnClick = true, Enabled = false };
			newCheckedListBox.Items.Add("item", true);
			return newCheckedListBox;
		});

		await rendered.Find(".checkedlistbox__input")
			.ClickAsync(new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".checkedlistbox__input:checked").Count, Is.EqualTo(1));
	}

	[Test]
	public async Task CheckOnClick_Default()
	{
		using var ctx = new WinzorTestContext();
		CheckedListBox newCheckedListBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			newCheckedListBox = new CheckedListBox();
			newCheckedListBox.Items.Add("item", true);

			return newCheckedListBox;
		});

		await rendered.Find(".checkedlistbox__input")
			.ClickAsync(new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".checkedlistbox__input:checked").Count, Is.EqualTo(1));

		await rendered.Find(".checkedlistbox__input")
			.ClickAsync(new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".checkedlistbox__input:checked").Count, Is.EqualTo(0));
	}

	[Test]
	public async Task TestCheckedListBoxItemHeight()
	{
		using var ctx = new WinzorTestContext();
		CheckedListBox checkedListBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			checkedListBox = new CheckedListBox();
			return checkedListBox;
		});

		Assert.That(checkedListBox.ItemHeight, Is.EqualTo(15));

		await checkedListBox.InvokeWinzorDispatcherAsync(() =>
		{
			checkedListBox.ItemHeight = 20;
		});

		Assert.That(checkedListBox.ItemHeight, Is.EqualTo(15));
	}

	[Test]
	public async Task CheckListBoxAddsCorrectIndex()
	{
		using var ctx = new WinzorTestContext();
		CheckedListBox newCheckedListBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			newCheckedListBox = new CheckedListBox();
			for (int i = 0; i < 5; i++)
			{
				int index = newCheckedListBox.Items.Add($"item{i}");
				Assert.That(index, Is.EqualTo(i));
			}

			return newCheckedListBox;
		});
	}

	[Test]
	public async Task CheckListBoxAddsCorrectSortedIndex()
	{
		using var ctx = new WinzorTestContext();
		CheckedListBox newCheckedListBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			newCheckedListBox = new CheckedListBox() { Sorted = true };
			var list = new string[] { "john", "alice", "bob", "danny" };
			var listofIndicies = new int[] { 0, 0, 1, 2 };
			for (int i = 0; i < list.Length; i++)
			{
				var index = newCheckedListBox.Items.Add(list[i]);
				Assert.That(index, Is.EqualTo(listofIndicies[i]));
			}

			return newCheckedListBox;
		});
	}

	[Test, WithPlaywrightPage]
	public async Task CheckedListBoxHasCorrectBorder()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();

			var checkedListBox = new CheckedListBox();

			form.Controls.Add(checkedListBox);
			return form;
		});

		var checkedListBox = await page.WaitForSelectorAsync(".checkedlistbox");
		AssertElementStyle(checkedListBox, "border", "1px solid rgb(130, 135, 144)");
	}

	[Test, WithPlaywrightPage]
	public async Task CheckedListBoxItemStyleTest()
	{
		await using var ctx = new InMemoryTestServerContext();
		CheckedListBox checkedListBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			checkedListBox = new CheckedListBox() { Dock = DockStyle.Fill };
			checkedListBox.Items.Add("item 1");
			checkedListBox.Items.Add("item 2");
			checkedListBox.SelectedIndex = 0;

			form.Controls.Add(checkedListBox);
			return form;
		});

		await page.WaitForSelectorAsync(".checkedlistbox");
		await page.Locator(".checkedlistbox", new PageLocatorOptions() { HasText = "item 1" }).ClickAsync();

		var element = await page.QuerySelectorAsync("body > div.form > div > div > div:nth-child(1)");
		AssertElementStyle(element, "display", "flex");

		element = await page.QuerySelectorAsync("body > div.form > div > div > div:nth-child(1) > label");
		AssertElementStyle(element, "white-space", "nowrap");
		AssertElementStyle(element, "background-color", "rgb(0, 120, 215)");
		AssertElementStyle(element, "color", "rgb(255, 255, 255)");
		AssertElementStyle(element, "box-sizing", "border-box");
		AssertElementStyle(element, "border-style", "dotted");

		element = await page.QuerySelectorAsync("body > div.form > div > div > div:nth-child(2)");
		AssertElementStyle(element, "display", "flex");

		element = await page.QuerySelectorAsync("body > div.form > div > div > div:nth-child(2) > label");
		AssertElementStyle(element, "white-space", "nowrap");
		AssertElementStyle(element, "background-color", "rgba(0, 0, 0, 0)");
		AssertElementStyle(element, "color", "rgb(0, 0, 0)");
		AssertElementStyle(element, "box-sizing", "border-box");
		AssertElementStyle(element, "border-color", "rgba(0, 0, 0, 0)");
	}

	void AssertElementStyle(IElementHandle element, string property, string expected)
	{
		Assert.That(
				async () => await element.EvaluateAsync<string>($"e => getComputedStyle(e).getPropertyValue('{property}')"),
				Is.EqualTo(expected).After(2000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task CheckedListBoxItemShouldInvertStateWhenPressSpaceKey()
	{
		await using var ctx = new InMemoryTestServerContext();
		CheckedListBox checkedListBox = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			checkedListBox = new CheckedListBox() { Dock = DockStyle.Fill };
			checkedListBox.Items.Add("item 1");
			checkedListBox.SelectedIndex = 0;
			return checkedListBox;
		});
		var checkedListBoxInput = page.Locator(".checkedlistbox__input");

		// Sanity check that the checkedlistbox is functional with click to rule out performance and network issue
		await Assertions.Expect(checkedListBoxInput).ToBeCheckedAsync();
		Assert.That(checkedListBox.GetItemChecked(0), Is.True);

		try
		{
			await checkedListBoxInput.UncheckAsync();
		}
		catch
		{
			await Assertions.Expect(checkedListBoxInput).Not.ToBeCheckedAsync();
		}
		Assert.That(checkedListBox.GetItemChecked(0), Is.False);

		// Assert that the checkedlistbox is functional with space key
		var div = page.Locator("div[data-type='System.Windows.Forms.CheckedListBox']");
		await div.WaitForAsync();
		await div.PressAsync("Space");
		await Assertions.Expect(checkedListBoxInput).ToBeCheckedAsync();
		Assert.That(checkedListBox.GetItemChecked(0), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task CheckedListBoxHasCorrectTabStop()
	{
		await using var ctx = new InMemoryTestServerContext();
		CheckedListBox checkedListBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();

			var button1 = new Button() { Dock = DockStyle.Top, TabIndex = 0, Text = "button 1" };
			var button2 = new Button() { Dock = DockStyle.Top, TabIndex = 2, Text = "button 2" };
			checkedListBox = new CheckedListBox() { Dock = DockStyle.Top, TabIndex = 1 };

			form.Controls.Add(button2);
			form.Controls.Add(checkedListBox);
			form.Controls.Add(button1);
			return form;
		});

		var items = Array.Empty<string>();
		await AssertCheckedListBoxTabStopAsync(page, checkedListBox, items, -1, string.Empty);

		items = ["item 1", "item 2", "item 3"];
		await AssertCheckedListBoxTabStopAsync(page, checkedListBox, items, -1, "item 1");
		await AssertCheckedListBoxTabStopAsync(page, checkedListBox, items, 0, "item 1");
		await AssertCheckedListBoxTabStopAsync(page, checkedListBox, items, 1, "item 2");
	}

	async Task AssertCheckedListBoxTabStopAsync(IPage page, CheckedListBox checkedListBox, string[] items, int selectedIndex, string exceptedItem)
	{
		await checkedListBox.InvokeWinzorDispatcherAsync(() =>
		{
			checkedListBox.Items.Clear();
			checkedListBox.Items.AddRange(items);
			checkedListBox.SelectedIndex = selectedIndex;
		});

		await page.GetByRole(AriaRole.Button, new() { Name = "button 1" }).ClickAsync();
		Assert.That(async () => await page.EvaluateAsync<string>("document.activeElement.textContent"), Is.EqualTo("button 1").After(2000, 200));

		await page.GetByRole(AriaRole.Button, new() { Name = "button 1" }).PressAsync("Tab");
		Assert.That(async () => await page.EvaluateAsync<string>("document.activeElement.className"), Is.EqualTo("checkedlistbox").After(2000, 200));
		Assert.That((await page.QuerySelectorAllAsync(".checkedlistbox__item--tabstop")).Count, Is.EqualTo(1));
		Assert.That(await ((await page.QuerySelectorAsync(".checkedlistbox__item--tabstop")).TextContentAsync()), Is.EqualTo(exceptedItem));

		await page.Locator(".checkedlistbox").PressAsync("Tab");
		Assert.That(async () => await page.EvaluateAsync<string>("document.activeElement.textContent"), Is.EqualTo("button 2").After(2000, 200));
	}

	[Test]
	public async Task CheckedListBoxSelectedItem()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestCheckedListBoxAsync(ctx);

		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(4));
		Assert.That(listbox.SelectedIndices.Count, Is.EqualTo(4));
		Assert.That(listbox.GetSelected(3), Is.True);

		AssertSelectedItems(listbox, [0, 1, 2, 3]);
	}

	[Test]
	public async Task CheckedListBoxSelectedIndexSet()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestCheckedListBoxAsync(ctx);

		var checkElements = rendered.FindAll(".checkedlistbox__input")[0];
		Assert.That(rendered.FindAll(".checkedlistbox__item--selected").Count, Is.EqualTo(0));
		Assert.That(listbox.GetItemChecked(0), Is.True);

		await checkElements.ClickAsync(new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".checkedlistbox__item--selected").Count, Is.EqualTo(1));
		Assert.That(listbox.GetItemChecked(0), Is.True);

		checkElements = rendered.FindAll(".checkedlistbox__input")[0];
		await checkElements.ClickAsync(new WebMouseEventArgs());
		Assert.That(listbox.GetItemChecked(0), Is.False);

		checkElements = rendered.FindAll(".checkedlistbox__input")[0];
		await checkElements.ClickAsync(new WebMouseEventArgs());
		Assert.That(listbox.GetItemChecked(0), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task MouseOverShouldSetCurrentIndex()
	{
		await using var ctx = new InMemoryTestServerContext();
		CheckedListBox checkedListBox = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			checkedListBox = new CheckedListBox() { Dock = DockStyle.Fill };
			checkedListBox.Items.Add("item 1");
			checkedListBox.Items.Add("item 2");
			return checkedListBox;
		});

		var elementRect = await page.Locator(".checkedlistbox__item").Nth(0).BoundingBoxAsync();
		await page.Mouse.MoveAsync(elementRect.X + 5, elementRect.Y + 5);
		Assert.That(() => checkedListBox.IndexFromPoint(new Drawing.Point()), Is.EqualTo(0).After(3000, 500));

		var elementRect1 = await page.Locator(".checkedlistbox__item").Nth(1).BoundingBoxAsync();
		await page.Mouse.MoveAsync(elementRect1.X + 5, elementRect1.Y + 5);
		Assert.That(() => checkedListBox.IndexFromPoint(new Drawing.Point()), Is.EqualTo(1).After(3000, 500));
	}

	[Test, WithPlaywrightPage]
	public async Task CheckedListBoxSetItemChecked()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestCheckedListBoxAsync(ctx);

		await listbox.InvokeWinzorDispatcherAsync(() =>
		{
			listbox.SetItemChecked(1, false);
			listbox.SetItemChecked(2, false);
			listbox.SetItemChecked(1, true);
		});

		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(3));
		Assert.That(listbox.SelectedIndices.Count, Is.EqualTo(3));
		Assert.That(listbox.GetSelected(3), Is.True);
		Assert.That(listbox.GetSelected(1), Is.True);
		Assert.That(listbox.GetSelected(2), Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task CheckedListBoxSetItemCheckedWithCheckedBool()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestCheckedListBoxWithCheckedBoolAsync(ctx);

		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(2));
		Assert.That(listbox.GetSelected(0), Is.True);
		Assert.That(listbox.GetSelected(1), Is.True);
		Assert.That(listbox.GetSelected(2), Is.False);
		Assert.That(listbox.GetSelected(3), Is.False);
	}

	[Test]
	public async Task CheckedListBox_CheckedItems_Count_IsRight()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestCheckedListBoxWithCheckedBoolAsync(ctx);

		Assert.That(listbox.CheckedItems.Count, Is.EqualTo(2));
	}

	[Test]
	public async Task CheckedListBox_CheckedItems_CopyTo()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestCheckedListBoxWithCheckedBoolAsync(ctx);

		string[] dest = new string[5];
		listbox.CheckedItems.CopyTo(dest, 1);

		Assert.Multiple(() =>
		{
			Assert.That(dest[0], Is.Null);
			Assert.That(dest[1], Is.EqualTo("item1"));
			Assert.That(dest[2], Is.EqualTo("item2"));
			Assert.That(dest[3], Is.Null);
			Assert.That(dest[4], Is.Null);
		});
	}

	[Test]
	public async Task CheckedListBox_CheckedIndices_Count_IsRight()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestCheckedListBoxWithCheckedBoolAsync(ctx);

		Assert.That(listbox.CheckedIndices.Count, Is.EqualTo(2));
	}

	[Test]
	public async Task CheckedListBox_CheckedIndices_CopyTo()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestCheckedListBoxWithCheckedBoolAsync(ctx);

		var dest = new int[5];
		listbox.CheckedIndices.CopyTo(dest, 1);

		Assert.Multiple(() =>
		{
			Assert.That(dest[0], Is.EqualTo(0));
			Assert.That(dest[1], Is.EqualTo(0));
			Assert.That(dest[2], Is.EqualTo(1));
			Assert.That(dest[3], Is.EqualTo(0));
			Assert.That(dest[4], Is.EqualTo(0));
		});
	}

	[Test]
	public async Task CheckedListBox_CheckedIndices_IndexOf()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestCheckedListBoxWithCheckedBoolAsync(ctx);

		Assert.That(listbox.CheckedIndices.IndexOf(0), Is.EqualTo(0));
		Assert.That(listbox.CheckedIndices.IndexOf(1), Is.EqualTo(1));
	}

	[Test]
	public async Task CheckedListBox_CheckedIndices_Contains()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestCheckedListBoxWithCheckedBoolAsync(ctx);

		Assert.That(listbox.CheckedIndices.Contains(0), Is.True);
		Assert.That(listbox.CheckedIndices.Contains(1), Is.True);
		Assert.That(listbox.CheckedIndices.Contains(2), Is.False);
		Assert.That(listbox.CheckedIndices.Contains(3), Is.False);
	}

	[Test]
	public async Task CheckedListBox_CheckedIndices_GetEnumerator()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestCheckedListBoxWithCheckedBoolAsync(ctx);

		var enumerator = listbox.CheckedIndices.GetEnumerator();
		Assert.That(enumerator.MoveNext(), Is.True);
		Assert.That(enumerator.Current, Is.EqualTo(0));
		Assert.That(enumerator.MoveNext(), Is.True);
		Assert.That(enumerator.Current, Is.EqualTo(1));
		Assert.That(enumerator.MoveNext(), Is.False);
	}

	[Test]
	public async Task CheckedListBox_CheckedIndices_Get()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestCheckedListBoxWithCheckedBoolAsync(ctx);

		await listbox.InvokeWinzorDispatcherAsync(() =>
		{
			listbox.SetItemChecked(3, true);
		});

		Assert.That(listbox.CheckedIndices.Count, Is.EqualTo(3));
		Assert.That(listbox.CheckedIndices[0], Is.EqualTo(0));
		Assert.That(listbox.CheckedIndices[1], Is.EqualTo(1));
		Assert.That(listbox.CheckedIndices[2], Is.EqualTo(3));
	}

	[Test]
	public async Task CheckedIndicesShouldStoreIndexofCheckedItemsWhenSetItemChecked()
	{
		using var ctx = new WinzorTestContext();
		CheckedListBox checkedListBox = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			checkedListBox = new CheckedListBox();
			var list = new string[] { "item1", "item2" };
			foreach (var item in list)
			{
				checkedListBox.Items.Add(item, false);
			}

			return checkedListBox;
		});

		Assert.That(checkedListBox.Items.Count, Is.EqualTo(2));
		Assert.That(checkedListBox.CheckedIndices.Count, Is.EqualTo(0));
		Assert.That(checkedListBox.GetSelected(0), Is.False);
		Assert.That(checkedListBox.GetSelected(1), Is.False);

		await checkedListBox.InvokeWinzorDispatcherAsync(() =>
		{
			checkedListBox.SetItemChecked(1, true);
		});

		Assert.That(checkedListBox.CheckedIndices.Count, Is.EqualTo(1));
		Assert.That(checkedListBox.CheckedIndices[0], Is.EqualTo(1));
		Assert.That(checkedListBox.GetSelected(1), Is.True);
	}

	[Test]
	public async Task SelectedIndexChangedShouldNotBeCalledAfterSelectWhenInit()
	{
		using var ctx = new WinzorTestContext();
		CheckedListBox checkedListBox = null;
		var isSelectedIndexChangedInvoked = false;
		await ctx.RenderControlOnFormAsync(() =>
		{
			checkedListBox = new CheckedListBox();
			var list = new string[] { "item1", "item2" };
			foreach (var item in list)
			{
				checkedListBox.Items.Add(item, false);
			}
			checkedListBox.SelectedIndexChanged += (s, e) => isSelectedIndexChangedInvoked = true;
			Assert.That(isSelectedIndexChangedInvoked, Is.False, "isSelectedIndexChangedInvoked initial state should be False");
			checkedListBox.SetItemChecked(1, true);

			return checkedListBox;
		});

		Assert.That(checkedListBox.Items.Count, Is.EqualTo(2));
		Assert.That(checkedListBox.GetSelected(0), Is.False);
		Assert.That(checkedListBox.GetSelected(1), Is.True);
		Assert.That(isSelectedIndexChangedInvoked, Is.False, "SelectedIndexChanged should not be invoked when init.");

		await checkedListBox.InvokeWinzorDispatcherAsync(() => checkedListBox.SetItemChecked(0, true));

		Assert.That(checkedListBox.GetSelected(0), Is.True);
		Assert.That(isSelectedIndexChangedInvoked, Is.True, "SelectedIndexChanged should be invoked after selecting item 0.");
	}

	async Task<(IRenderedFragment, CheckedListBox)> TestCheckedListBoxAsync(WinzorTestContext ctx)
	{
		CheckedListBox newCheckedListBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			newCheckedListBox = new CheckedListBox();
			var list = new string[] { "item1", "item2", "item3", "item4" };
			foreach (var item in list)
			{
				newCheckedListBox.Items.Add(item, true);
			}

			return newCheckedListBox;
		});

		return (rendered, newCheckedListBox);
	}

	async Task<(IRenderedFragment, CheckedListBox)> TestCheckedListBoxWithCheckedBoolAsync(WinzorTestContext ctx)
	{
		CheckedListBox newCheckedListBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			newCheckedListBox = new CheckedListBox();
			var list = new string[] { "item1", "item2", "item3", "item4" };

			newCheckedListBox.Items.Add(list[0], true);
			newCheckedListBox.Items.Add(list[1], true);
			newCheckedListBox.Items.Add(list[2], false);
			newCheckedListBox.Items.Add(list[3], false);

			return newCheckedListBox;
		});

		return (rendered, newCheckedListBox);
	}

	void AssertSelectedItems(CheckedListBox checklistBox, int[] expectedIndexes)
	{
		var items = checklistBox.SelectedItems;
		var indices = checklistBox.SelectedIndices;

		Assert.That(items.Count, Is.EqualTo(expectedIndexes.Length));
		Assert.That(indices.Count, Is.EqualTo(expectedIndexes.Length));

		for (var i = 0; i < expectedIndexes.Length; i++)
		{
			Assert.That(indices, Does.Contain(expectedIndexes[i]));
		}

		for (var i = 0; i < expectedIndexes.Length; i++)
		{
			Assert.That(items[i].ToString(), Does.Contain($"item{expectedIndexes[i] + 1}"));
		}
	}
}
