using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

[TestFixture]
public class ListBoxTest
{
	[Test, WithPlaywrightPage]
	public async Task ListBoxFocus()
	{
		await using var ctx = new InMemoryTestServerContext();
		ListBox listBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			listBox = new ListBox { Top = 30 };
			form.Controls.Add(new TextBox());
			form.Controls.Add(listBox);
			return form;
		});

		Assert.That(listBox.Focused, Is.False);

		await page.Locator(".listbox").ClickAsync();
		Assert.That(() => listBox.Focused, Is.True.After(2000, 100));

		await page.Locator(".textbox").Nth(0).ClickAsync();
		Assert.That(() => listBox.Focused, Is.False.After(2000, 100));

		await page.Locator(".listbox").ClickAsync();
		Assert.That(() => listBox.Focused, Is.True.After(2000, 100));

		await page.Mouse.ClickAsync(0, listBox.Bottom + 5);
		Assert.That(() => listBox.Focused, Is.False.After(2000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ListBoxComputedStyle()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var listBox = new ListBox { Width = 200, Height = 100, MultiColumn = false, ScrollAlwaysVisible = false, IntegralHeight = false };
			listBox.Items.AddRange(Enumerable.Range(1, 20).Select(x => $"Item {x}"));
			return listBox;
		});
		var listBox = page.Locator(".listbox");
		var listBoxWrapper = listBox.Locator(".listbox__wrapper");
		var listBoxItem = listBox.Locator(".listbox__item").Nth(0);

		Assert.That(await listBox.GetComputedStyleAsync("overflow-x"), Is.EqualTo("auto"), listBox.GetStyleMessage("overflow-x"));
		Assert.That(await listBox.GetComputedStyleAsync("overflow-y"), Is.EqualTo("auto"), listBox.GetStyleMessage("overflow-y"));
		Assert.That(await listBox.GetComputedStyleAsync("border"), Is.EqualTo("1px solid rgb(130, 135, 144)"), listBox.GetStyleMessage("border"));
		Assert.That(await listBox.GetComputedStyleAsync("width"), Is.EqualTo("200px"), listBox.GetStyleMessage("width"));
		Assert.That(await listBox.GetComputedStyleAsync("height"), Is.EqualTo("100px"), listBox.GetStyleMessage("height"));
		Assert.That(await listBox.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(255, 255, 255)"), listBox.GetStyleMessage("background-color"));

		Assert.That(await listBoxWrapper.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(255, 255, 255)"), listBoxWrapper.GetStyleMessage("background-color"));
		Assert.That(await listBoxWrapper.GetComputedStyleAsync("padding"), Is.EqualTo("1px"), listBoxWrapper.GetStyleMessage("padding"));
		Assert.That(await listBoxWrapper.GetComputedStyleAsync("display"), Is.EqualTo("inline-block"), listBoxWrapper.GetStyleMessage("display"));
		Assert.That(await listBoxWrapper.GetComputedStyleAsync("white-space"), Is.EqualTo("nowrap"), listBoxWrapper.GetStyleMessage("white-space"));
		Assert.That(await listBoxWrapper.GetComputedStyleAsync("min-width"), Is.EqualTo("100%"), listBoxWrapper.GetStyleMessage("min-width"));

		Assert.That(await listBoxItem.GetComputedStyleAsync("padding"), Is.EqualTo("2px"), listBoxItem.GetStyleMessage("padding"));
		Assert.That(await listBoxItem.GetComputedStyleAsync("cursor"), Is.EqualTo("auto"), listBoxItem.GetStyleMessage("cursor"));
		Assert.That(await listBoxItem.GetComputedStyleAsync("white-space"), Is.EqualTo("nowrap"), listBoxItem.GetStyleMessage("white-space"));
	}

	[TestCase(false, false, "overflow-y:auto;")]
	[TestCase(false, true, "overflow-y:scroll;")]
	[TestCase(true, false, "")]
	[TestCase(true, true, "")]
	public async Task ScrollableControlShouldHaveScrollBarWhenOverflow(bool multiColumn, bool scrollAlwaysVisible, string expectStyle)
	{
		using var ctx = new WinzorTestContext();
		ListBox listBox = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			listBox = new ListBox() { Width = 200, Height = 100, MultiColumn = multiColumn, ScrollAlwaysVisible = scrollAlwaysVisible };
			for (var x = 1; x <= 50; x++)
			{
				listBox.Items.Add("Item " + x.ToString());
			}

			return listBox;
		});

		var listboxEl = rendered.Find(".listbox");
		Assert.That(listboxEl.GetAttribute("style"), Does.Contain(expectStyle));
	}

	[Test]
	public async Task ListBoxRendered()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx);
		Assert.That(rendered.FindAll(".listbox").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll(".listbox__item:not(:last-child)").Select(i => i.InnerHtml), Is.EqualTo(new string[] { "item1", "item2", "item3" }));
	}

	[Test]
	public async Task ListBoxEmpty()
	{
		ListBox listbox = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			listbox = new ListBox();
			return listbox;
		});
		Assert.That(rendered.FindAll(".listbox").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll(".listbox table").Count, Is.EqualTo(0));
		Assert.That(listbox.SelectionMode, Is.EqualTo(SelectionMode.One));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(-1));
		Assert.That(listbox.SelectedIndices.Count, Is.EqualTo(0));
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(0));
	}

	[Test]
	[TestCase(true, 39)]
	[TestCase(false, 50)]
	public async Task ListBoxShouldHaveCorrectHeight(bool integralHeight, int expectedHeight)
	{
		ListBox listbox = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			listbox = new ListBox() { IntegralHeight = integralHeight, Height = 50 };
			return listbox;
		});

		Assert.That(listbox.Visible, Is.True);
		Assert.That(listbox.ItemHeight, Is.EqualTo(13));
		Assert.That(listbox.Height, Is.EqualTo(expectedHeight));
	}

	[Test]
	public async Task ListBoxSetSelectedIndex()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx, SelectionMode.MultiSimple);

		await listbox.InvokeWinzorDispatcherAsync(() => listbox.SelectedIndex = 2);
		await listbox.InvokeWinzorDispatcherAsync(() => listbox.SelectedIndex = 1);

		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(2));
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(2));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(1));
		Assert.That(listbox.GetSelected(1), Is.True);
		Assert.That(listbox.GetSelected(2), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item2"));
		AssertSelectedItems(listbox, new int[] { 1, 2 });
	}

	[Test]
	public async Task ListBoxCanSetSelectedIndexAsNoMatches()
	{
		using var ctx = new WinzorTestContext();
		var (_, listbox) = await TestListBoxAsync(ctx, SelectionMode.MultiSimple);

		await listbox.InvokeWinzorDispatcherAsync(() => listbox.SelectedIndex = 2);
		Assert.That(listbox.SelectedIndex, Is.EqualTo(2));
		await listbox.InvokeWinzorDispatcherAsync(() => listbox.SelectedIndex = ListBox.NoMatches);
		Assert.That(listbox.SelectedIndex, Is.EqualTo(ListBox.NoMatches));
	}

	[Test]
	public async Task ListBoxSetSelectedIndexException()
	{
		using var ctx = new WinzorTestContext();
		var (_, listbox) = await TestListBoxAsync(ctx, SelectionMode.MultiSimple);
		var (_, listbox2) = await TestListBoxAsync(ctx, SelectionMode.None);

		Assert.Throws<ArgumentOutOfRangeException>(() => listbox.SelectedIndex = 9);
		Assert.Throws<ArgumentOutOfRangeException>(() => listbox.SetSelected(9, true));
		Assert.Throws<ArgumentException>(() => listbox2.SelectedIndex = 0);
	}

	[Test]
	public async Task ListBoxSetSelectedItem()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx);

		await listbox.InvokeWinzorDispatcherAsync(() => listbox.SelectedItem = listbox.Items[2]);

		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(1));
		rendered.Find(".listbox__item:nth-child(3).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(2));
		Assert.That(listbox.GetSelected(2), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item3"));

		AssertSelectedItems(listbox, new int[] { 2 });
	}

	[Test]
	public async Task ListBoxAddRemoveSelectedItems()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx, SelectionMode.MultiSimple);

		await listbox.InvokeWinzorDispatcherAsync(() => {
			listbox.SelectedItems.Add("item2");
			listbox.SelectedItems.Add("item3");
		});

		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(2));
		rendered.Find(".listbox__item:nth-child(3).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(2));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(1));
		Assert.That(listbox.GetSelected(1), Is.True);
		Assert.That(listbox.GetSelected(2), Is.True);

		AssertSelectedItems(listbox, new int[] { 1, 2 });

		await listbox.InvokeWinzorDispatcherAsync(() => listbox.SelectedItems.Remove("item2"));
		AssertSelectedItems(listbox, new int[] { 2 });

		await listbox.InvokeWinzorDispatcherAsync(() => listbox.SelectedItems.Remove("item3"));
		AssertSelectedItems(listbox, Array.Empty<int>());
	}

	[Test]
	public async Task ListBoxAddRemoveSelectedIndicies()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx, SelectionMode.MultiSimple);

		await listbox.InvokeWinzorDispatcherAsync(() => {
			listbox.SelectedIndices.Add(1);
			listbox.SelectedIndices.Add(2);
		});

		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(2));
		rendered.Find(".listbox__item:nth-child(3).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(2));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(1));
		Assert.That(listbox.GetSelected(1), Is.True);
		Assert.That(listbox.GetSelected(2), Is.True);

		AssertSelectedItems(listbox, new int[] { 1, 2 });

		await listbox.InvokeWinzorDispatcherAsync(() => listbox.SelectedIndices.Remove(1));
		AssertSelectedItems(listbox, new int[] { 2 });

		await listbox.InvokeWinzorDispatcherAsync(() => listbox.SelectedIndices.Remove(2));
		AssertSelectedItems(listbox, Array.Empty<int>());
	}

	[Test]
	public async Task ListBoxSetSelected()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx);

		await listbox.InvokeWinzorDispatcherAsync(() => {
			listbox.SetSelected(1, true);
			listbox.SetSelected(2, true);
		});

		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(1));
		rendered.Find(".listbox__item:nth-child(3).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(2));
		Assert.That(listbox.GetSelected(2), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item3"));

		AssertSelectedItems(listbox, new int[] { 2 });
	}

	[Test]
	public async Task ListBoxClearSelectedItems()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx);

		await listbox.InvokeWinzorDispatcherAsync(() => {
			listbox.SetSelected(1, true);
		});

		AssertSelectedItems(listbox, new int[] { 1 });

		await listbox.InvokeWinzorDispatcherAsync(() => listbox.ClearSelected());
		AssertSelectedItems(listbox, Array.Empty<int>());
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(0));
	}

	[Test]
	public async Task ListBoxSetSelectedAfterRemove()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx);

		await listbox.InvokeWinzorDispatcherAsync(() => listbox.SetSelected(0, true));

		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(1));
		rendered.Find(".listbox__item:nth-child(1).listbox__item--selected");
		Assert.That(listbox.Items.Count, Is.EqualTo(3));
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(0));
		Assert.That(listbox.GetSelected(0), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item1"));

		await listbox.InvokeWinzorDispatcherAsync(() =>
		{
			listbox.Items.RemoveAt(0);
			listbox.SetSelected(0, true);
		});

		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(1));
		rendered.Find(".listbox__item:nth-child(1).listbox__item--selected");
		Assert.That(listbox.Items.Count, Is.EqualTo(2));
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(0));
		Assert.That(listbox.GetSelected(0), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item2"));
	}

	[Test]
	public async Task ListBoxSelectOneItem()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx);

		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(0));
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(0));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(-1));

		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("onmousedown",new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(1));
		rendered.Find(".listbox__item:nth-child(1).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(0));
		Assert.That(listbox.GetSelected(0), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item1"));
		AssertSelectedItems(listbox, new int[] { 0 });

		await rendered.Find(".listbox__item:nth-child(2)").TriggerEventAsync("onmousedown",new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(1));
		rendered.Find(".listbox__item:nth-child(2).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(1));
		Assert.That(listbox.GetSelected(1), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item2"));
		AssertSelectedItems(listbox, new int[] { 1 });
	}

	[Test]
	public async Task ListBoxMultiSimple()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx, SelectionMode.MultiSimple);

		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(0));
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(0));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(-1));
		Assert.That(listbox.SelectedItem, Is.Null);

		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("onmousedown",new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(1));
		rendered.Find(".listbox__item:nth-child(1).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(0));
		Assert.That(listbox.GetSelected(0), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item1"));
		AssertSelectedItems(listbox, new int[] { 0 });

		await rendered.Find(".listbox__item:nth-child(2)").TriggerEventAsync("onmousedown",new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(2));
		rendered.Find(".listbox__item:nth-child(2).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(2));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(0));
		Assert.That(listbox.GetSelected(1), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item1"));
		AssertSelectedItems(listbox, new int[] { 0, 1 });

		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(1));
		rendered.Find(".listbox__item:nth-child(2).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(1));
		Assert.That(listbox.GetSelected(0), Is.False);
		Assert.That(listbox.GetSelected(1), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item2"));
		AssertSelectedItems(listbox, new int[] { 1 });

		await rendered.Find(".listbox__item:nth-child(2)").TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(0));
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(0));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(-1));
		Assert.That(listbox.GetSelected(0), Is.False);
		Assert.That(listbox.GetSelected(1), Is.False);
		Assert.That(listbox.SelectedItem, Is.Null);
		AssertSelectedItems(listbox, Array.Empty<int>());
	}

	[Test]
	[TestCase(SelectionMode.One)]
	[TestCase(SelectionMode.MultiSimple)]
	[TestCase(SelectionMode.MultiExtended)]
	public async Task ListBoxAddRemoveItems(SelectionMode selectionMode)
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx, selectionMode);

		Assert.That(listbox.Items.Count, Is.EqualTo(3));
		Assert.That(rendered.FindAll(".listbox__item:not(:last-child)").Count, Is.EqualTo(3));

		await listbox.InvokeWinzorDispatcherAsync(() => listbox.Items.Add("item4"));
		Assert.That(listbox.Items.Count, Is.EqualTo(4));
		Assert.That(rendered.FindAll(".listbox__item:not(:last-child)").Count, Is.EqualTo(4));
		Assert.That(rendered.Find(".listbox__item:nth-child(4)").TextContent, Is.EqualTo("item4"));

		await listbox.InvokeWinzorDispatcherAsync(() => listbox.Items.Remove("item2"));
		Assert.That(listbox.Items.Count, Is.EqualTo(3));
		Assert.That(rendered.FindAll(".listbox__item:not(:last-child)").Count, Is.EqualTo(3));
		Assert.That(rendered.Find(".listbox__item:nth-child(2)").TextContent, Is.EqualTo("item3"));

		await listbox.InvokeWinzorDispatcherAsync(() =>
		{
			listbox.Items.Add("item5");
			listbox.Items.Add("item6");
		});
		Assert.That(listbox.Items.Count, Is.EqualTo(5));
		Assert.That(rendered.FindAll(".listbox__item:not(:last-child)").Count, Is.EqualTo(5));
		Assert.That(rendered.Find(".listbox__item:nth-child(4)").TextContent, Is.EqualTo("item5"));
		Assert.That(rendered.Find(".listbox__item:nth-child(5)").TextContent, Is.EqualTo("item6"));

		await listbox.InvokeWinzorDispatcherAsync(() => {
			listbox.Items.Remove("item1");
			listbox.Items.Remove("item3");
			listbox.Items.Remove("item4");
			listbox.Items.Remove("item5");
			listbox.Items.Remove("item6");
		});
		Assert.That(listbox.Items.Count, Is.EqualTo(0));
		Assert.That(rendered.FindAll(".listbox__item:not(:last-child)").Count, Is.EqualTo(0));
	}

	[Test]
	public async Task ListBoxMultiSimpleCtrlShiftClick()
	{
		await MultiSelect(new WebMouseEventArgs() { CtrlKey = true });
		await MultiSelect(new WebMouseEventArgs() { ShiftKey = true });
	}

	async Task MultiSelect(WebMouseEventArgs arg)
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx, SelectionMode.MultiSimple);

		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("onmousedown",arg);
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(1));
		rendered.Find(".listbox__item:nth-child(1).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(0));
		Assert.That(listbox.GetSelected(0), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item1"));
		AssertSelectedItems(listbox, new int[] { 0 });

		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("onmousedown",arg);
		await rendered.Find(".listbox__item:nth-child(2)").TriggerEventAsync("onmousedown",arg);
		await rendered.Find(".listbox__item:nth-child(3)").TriggerEventAsync("onmousedown",arg);
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(2));
		Assert.That(rendered.FindAll(".listbox__item:nth-child(1).listbox__item--selected").Count, Is.EqualTo(0));
		rendered.Find(".listbox__item:nth-child(2).listbox__item--selected");
		rendered.Find(".listbox__item:nth-child(3).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(2));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(1));
		Assert.That(listbox.GetSelected(0), Is.False);
		Assert.That(listbox.GetSelected(1), Is.True);
		Assert.That(listbox.GetSelected(2), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item2"));
		AssertSelectedItems(listbox, new int[] { 1, 2 });
	}

	[Test]
	public async Task ListBoxMultiExtendedCtrlClick()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx, SelectionMode.MultiExtended);

		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(0));
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(0));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(-1));
		Assert.That(listbox.SelectedItem, Is.Null);

		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("onmousedown",new WebMouseEventArgs() { CtrlKey = true });
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(1));
		rendered.Find(".listbox__item:nth-child(1).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(0));
		Assert.That(listbox.GetSelected(0), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item1"));
		AssertSelectedItems(listbox, new int[] { 0 });

		await rendered.Find(".listbox__item:nth-child(3)").TriggerEventAsync("onmousedown",new WebMouseEventArgs() { CtrlKey = true });
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(2));
		rendered.Find(".listbox__item:nth-child(3).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(2));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(0));
		Assert.That(listbox.GetSelected(0), Is.True);
		Assert.That(listbox.GetSelected(2), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item1"));
		AssertSelectedItems(listbox, new int[] { 0, 2 });

		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("onmousedown",new WebMouseEventArgs() { CtrlKey = true });
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll(".listbox__item:nth-child(2).listbox__item--selected").Count, Is.EqualTo(0));
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(2));
		Assert.That(listbox.GetSelected(0), Is.False);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item3"));
		AssertSelectedItems(listbox, new int[] { 2 });

		await rendered.Find(".listbox__item:nth-child(3)").TriggerEventAsync("onmousedown",new WebMouseEventArgs() { CtrlKey = true });
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(0));
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(0));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(-1));
		Assert.That(listbox.SelectedItem, Is.Null);
		AssertSelectedItems(listbox, Array.Empty<int>());
	}

	[Test]
	public async Task ListBoxMultiExtendedShiftClick()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx, SelectionMode.MultiExtended);

		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(0));
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(0));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(-1));
		Assert.That(listbox.SelectedItem, Is.Null);

		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("onmousedown",new WebMouseEventArgs() { Buttons = 1 });
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(1));
		rendered.Find(".listbox__item:nth-child(1).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(0));
		Assert.That(listbox.GetSelected(0), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item1"));
		AssertSelectedItems(listbox, new int[] { 0 });

		await rendered.Find(".listbox__item:nth-child(3)").TriggerEventAsync("onmousedown",new WebMouseEventArgs() { Buttons = 1, ShiftKey = true });
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(3));
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(3));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(0));
		Assert.That(listbox.GetSelected(0), Is.True);
		Assert.That(listbox.GetSelected(1), Is.True);
		Assert.That(listbox.GetSelected(2), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item1"));
		AssertSelectedItems(listbox, new int[] { 0, 1, 2 });

		await rendered.Find(".listbox__item:nth-child(2)").TriggerEventAsync("onmousedown",new WebMouseEventArgs() { Buttons = 1, ShiftKey = true });
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(2));
		rendered.Find(".listbox__item:nth-child(1).listbox__item--selected");
		rendered.Find(".listbox__item:nth-child(2).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(2));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(0));
		Assert.That(listbox.GetSelected(0), Is.True);
		Assert.That(listbox.GetSelected(1), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item1"));
		AssertSelectedItems(listbox, new int[] { 0, 1 });

		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("onmousedown",new WebMouseEventArgs() { Buttons = 1, ShiftKey = true });
		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(1));
		rendered.Find(".listbox__item:nth-child(1).listbox__item--selected");
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(0));
		Assert.That(listbox.GetSelected(0), Is.True);
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item1"));
		AssertSelectedItems(listbox, new int[] { 0 });
	}

	[Test, WithPlaywrightPage]
	public async Task ListBoxMultiExtendedUnfocused_ClickShouldSelectOnlyOne()
	{
		await using var ctx = new InMemoryTestServerContext();
		ListBox listbox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			listbox = new ListBox() { SelectionMode = SelectionMode.MultiExtended };
			listbox.Items.Add("item1");
			listbox.Items.Add("item2");
			form.Controls.Add(new TextBox() { Top = 100 });
			form.Controls.Add(listbox);
			return form;
		});

		Assert.That(listbox.Focused, Is.False);
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(0));

		await page.Locator(".listbox__item").Nth(0).ClickAsync();
		Assert.That(() => listbox.Focused, Is.True.After(2000, 100));
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedItem?.ToString(), Is.EqualTo("item1"));

		await page.Locator(".textbox").First.ClickAsync();
		Assert.That(() => listbox.Focused, Is.False.After(2000, 100));

		await page.Locator(".listbox__item").Nth(1).ClickAsync();
		Assert.That(() => listbox.Focused, Is.True.After(2000, 100));
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedItem?.ToString(), Is.EqualTo("item2"));
	}

	[Test, WithPlaywrightPage]
	public async Task MouseOverShouldSetCurrentIndex()
	{
		await using var ctx = new InMemoryTestServerContext();
		ListBox listbox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			listbox = new ListBox() { SelectionMode = SelectionMode.MultiExtended };
			listbox.Items.Add("item1");
			listbox.Items.Add("item2");
			form.Controls.Add(listbox);
			return form;
		});

		var elementRect = await page.Locator(".listbox__item").Nth(0).BoundingBoxAsync();
		await page.Mouse.MoveAsync(elementRect.X + 5, elementRect.Y + 5);
		Assert.That(() => listbox.IndexFromPoint(new Point()), Is.EqualTo(0).After(3000, 500));

		var elementRect1 = await page.Locator(".listbox__item").Nth(1).BoundingBoxAsync();
		await page.Mouse.MoveAsync(elementRect1.X + 5, elementRect1.Y + 5);
		Assert.That(() => listbox.IndexFromPoint(new Point()), Is.EqualTo(1).After(3000, 500));
	}

	[Test]
	public async Task ListBoxSelectNone()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx, SelectionMode.None);

		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("onmousedown",new WebMouseEventArgs());

		Assert.That(rendered.FindAll(".listbox__item.listbox__item--selected").Count, Is.EqualTo(0));
		Assert.That(listbox.SelectedItems.Count, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task SortedListBoxDraggedWithoutOverlayStyle()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var newListbox = new ListBox() { Sorted = true, AllowDrop = true };
			var list = new string[] { "item1", "item2", "item3" };
			foreach (var item in list)
			{
				newListbox.Items.Add(item);
			}

			form.Controls.Add(newListbox);
			return form;
		});
		var listbox = await page.WaitForSelectorAsync(".listbox");
		await (await page.QuerySelectorAllAsync(".listbox .listbox__item"))[0].DispatchEventAsync("dragover");
		var over = await page.QuerySelectorAllAsync(".listbox__item.listbox__item--over");
		Assert.That(over.Count, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task UnsortedListBoxDraggedWithOverlayStyle()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var newListbox = new ListBox() { Sorted = false, AllowDrop = true };
			var list = new string[] { "item1", "item2", "item3" };
			foreach (var item in list)
			{
				newListbox.Items.Add(item);
			}

			form.Controls.Add(newListbox);
			return form;
		});
		var listbox = await page.WaitForSelectorAsync(".listbox");
		await (await page.QuerySelectorAllAsync(".listbox .listbox__item"))[0].DispatchEventAsync("dragover");
		var over = await page.QuerySelectorAllAsync(".listbox .listbox__item.listbox__item--over");
		Assert.That(over.Count, Is.EqualTo(1));
	}

	[Test, WithPlaywrightPage]
	public async Task ListBoxItemsShouldNotWrap()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var newListbox = new ListBox() { Sorted = false, AllowDrop = true };
			var list = new string[] { "item1", "item2", "item3" };
			foreach (var item in list)
			{
				newListbox.Items.Add(item);
			}

			form.Controls.Add(newListbox);
			return form;
		});
		var listbox_item = await page.WaitForSelectorAsync(".listbox .listbox__item");
		Assert.That(async () => await listbox_item.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('white-space')"), Is.EqualTo("nowrap"));
	}

	[Test]
	public async Task ListBoxSortedRenderred()
	{
		using var ctx = new WinzorTestContext(); 
		ListBox newListbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			newListbox = new ListBox() { Sorted = false };
			var list = new string[] { "john", "bob", "alice", "danny" };
			foreach (var item in list)
			{
				newListbox.Items.Add(item);
			}

			return newListbox;
		});
		Assert.That(rendered.FindAll(".listbox").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll(".listbox__item:not(:last-child)").Select(i => i.InnerHtml), Is.EqualTo(new string[] { "john", "bob", "alice", "danny" }));

		await newListbox.InvokeWinzorDispatcherAsync(() => {
			newListbox.Sorted = true;
		});
		Assert.That(rendered.FindAll(".listbox__item:not(:last-child)").Select(i => i.InnerHtml), Is.EqualTo(new string[] { "alice", "bob", "danny", "john" }));
	}

	[Test]
	public async Task ListBoxAddOperations()
	{
		using var ctx = new WinzorTestContext();
		ListBox newListbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			newListbox = new ListBox() { Sorted = true };
			var list = new string[] { "john", "bob", "alice", "danny" };
			foreach (var item in list)
			{
				newListbox.Items.Add(item);
			}

			return newListbox;
		});

		Assert.That(rendered.FindAll(".listbox").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll(".listbox__item:not(:last-child)").Select(i => i.InnerHtml), Is.EqualTo(new string[] { "alice", "bob", "danny", "john" }));
	}

	[Test]
	public async Task ListBoxAddRangeOperations()
	{
		using var ctx = new WinzorTestContext();
		ListBox newListbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			newListbox = new ListBox() { Sorted = true };
			var list = new string[] { "john", "bob", "alice", "danny" };
			newListbox.Items.AddRange(list);
			return newListbox;
		});

		Assert.That(rendered.FindAll(".listbox").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll(".listbox__item:not(:last-child)").Select(i => i.InnerHtml), Is.EqualTo(new string[] { "alice", "bob", "danny", "john" }));
	}

	[Test]
	public async Task ListBoxInsertOperations()
	{
		using var ctx = new WinzorTestContext();
		ListBox newListbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			newListbox = new ListBox() { Sorted = true };
			var list = new string[] { "john", "bob", "alice", "danny" };
			for (var index = 0; index < list.Length; index++)
			{
				newListbox.Items.Insert(index, list[index]);
			}
			return newListbox;
		});

		Assert.That(rendered.FindAll(".listbox").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll(".listbox__item:not(:last-child)").Select(i => i.InnerHtml), Is.EqualTo(new string[] { "alice", "bob", "danny", "john" }));

		await newListbox.InvokeWinzorDispatcherAsync(() => {
			newListbox.Sorted = false;
			newListbox.Items.Insert(0, "testDummy");
		});
		Assert.That(rendered.FindAll(".listbox__item:not(:last-child)").Select(i => i.InnerHtml), Is.EqualTo(new string[] { "testDummy","alice", "bob", "danny", "john" }));
	}

	[Test]
	public async Task ListBoxSelectItemWithSameItems()
	{
		using var ctx = new WinzorTestContext();
		ListBox newListbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			newListbox = new ListBox() { Sorted = true };
			var list = new string[] { "john", "john", "john", "john" };
			newListbox.Items.AddRange(list);

			newListbox.Select(1);
			return newListbox;
		});

		var items = rendered.FindAll(".listbox__wrapper div");
		Assert.That(items.Count, Is.EqualTo(5));
		Assert.That(items[0].GetAttribute("class"), Is.EqualTo("listbox__item "));
		Assert.That(items[1].GetAttribute("class"), Is.EqualTo("listbox__item listbox__item--selected"));
		Assert.That(items[2].GetAttribute("class"), Is.EqualTo("listbox__item "));
		Assert.That(items[3].GetAttribute("class"), Is.EqualTo("listbox__item "));
		Assert.That(items[4].GetAttribute("class"), Is.EqualTo("listbox__item"));
	}

	[Test]
	public async Task ListBoxItemHasNoChildElements()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listbox) = await TestListBoxAsync(ctx);
		Assert.That(rendered.FindAll(".listbox").Count, Is.EqualTo(1));
		Assert.That(rendered.Find(".listbox__item").ChildElementCount, Is.EqualTo(0));
	}

	[TestCase(SelectionMode.One)]
	[TestCase(SelectionMode.MultiSimple)]
	[TestCase(SelectionMode.MultiExtended)]
	public async Task ListBoxSelectMultipleItems(SelectionMode mode)
	{
		var indicesToSelect = new int[] { 1, 2, 4 };
		var itemsToAdd = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" };

		using var ctx = new WinzorTestContext();
		ListBox listbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			listbox = new ListBox() { SelectionMode = mode };

			foreach (var item in itemsToAdd)
			{
				listbox.Items.Add(item);
			}

			foreach (var index in indicesToSelect)
			{
				listbox.SetSelected(index, value: true);
			}

			return listbox;
		});

		if (mode == SelectionMode.One)
		{
			Assert.That(listbox.SelectedIndices.Count, Is.EqualTo(1));
			Assert.That(listbox.Items[indicesToSelect.Last()], Is.EqualTo(listbox.SelectedItems[0]));
		}
		else
		{
			Assert.That(listbox.SelectedIndices.Count, Is.EqualTo(indicesToSelect.Length));
			for (var i = 0; i < indicesToSelect.Length; i++)
			{
				Assert.That(listbox.SelectedItems[i], Is.EqualTo(itemsToAdd[indicesToSelect[i]]));
			}
		}
	}

	async Task<(IRenderedFragment, ListBox)> TestListBoxAsync(WinzorTestContext ctx, SelectionMode mode = SelectionMode.One)
	{
		ListBox newListbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			newListbox = new ListBox() { SelectionMode = mode };
			var list = new string[] { "item1", "item2", "item3" };
			foreach (var item in list)
			{
				newListbox.Items.Add(item);
			}

			return newListbox;
		});

		return (rendered, newListbox);
	}

	void AssertSelectedItems(ListBox listBox, int[] expectedIndexes)
	{
		var items = listBox.SelectedItems;
		var indices = listBox.SelectedIndices;

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

	[Test]
	public async Task LastSelectedIndexIsResetWhenRemovingItems()
	{
		using var ctx = new WinzorTestContext();
		ListBox listbox = null;

		var rendered = await ctx.RenderControlOnFormAsync(() => {
			listbox = new ListBox() { SelectionMode = SelectionMode.MultiExtended };
			var list = new string[] { "1", "2", "3", "4", "5", "6" };
			foreach (var item in list)
			{
				listbox.Items.Add(item);
			}

			return listbox;
		});

		await listbox.InvokeWinzorDispatcherAsync(() =>
		{
			listbox.SetSelected(4, true, new WebMouseEventArgs() { Buttons = 1 });
			listbox.SetSelected(5, true, new WebMouseEventArgs() { Buttons = 1, ShiftKey = true });

			listbox.Items.RemoveAt(5);

			Assert.DoesNotThrow(() => listbox.SetSelected(0, true, new WebMouseEventArgs() { Buttons = 1, ShiftKey = true }));
			Assert.That(listbox.SelectedIndices.Count, Is.EqualTo(1));
		});
	}

	[Test]
	public async Task ListBoxUpdateSelectedIndicesWhenRemovingItems()
	{
		using var ctx = new WinzorTestContext();
		ListBox listbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			listbox = new ListBox() { SelectionMode = SelectionMode.MultiExtended };
			var list = new string[] { "1", "2", "3", "4", "5", "6" };
			foreach (var item in list)
			{
				listbox.Items.Add(item);
			}

			listbox.SetSelected(2, true);
			listbox.SetSelected(4, true);

			return listbox;
		});

		Assert.That(listbox.SelectedIndices.Count, Is.EqualTo(2));
		Assert.That(listbox.SelectedIndices, Is.EqualTo(new int[] { 2, 4 }));

		await listbox.InvokeWinzorDispatcherAsync(() => listbox.Items.RemoveAt(4));
		Assert.That(listbox.SelectedIndices.Count, Is.EqualTo(1));
		Assert.That(listbox.SelectedIndices, Is.EqualTo(new int[] { 2 }));

		await listbox.InvokeWinzorDispatcherAsync(() => listbox.Items.RemoveAt(2));
		Assert.That(listbox.SelectedIndices.Count, Is.EqualTo(0));
	}

	[Test]
	public async Task ListBoxUpdateDataSource()
	{
		using var ctx = new WinzorTestContext();
		ListBox listbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			listbox = new ListBox() { };
			var list = new string[] { "item1", "item2", "item3" };
			foreach (var item in list)
			{
				listbox.Items.Add(item);
			}

			listbox.SetSelected(2, true);

			return listbox;
		});

		Assert.That(listbox.Items.Count, Is.EqualTo(3));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(2));
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("item3"));

		await listbox.InvokeWinzorDispatcherAsync(() => listbox.DataSource = new string[] { "1", "2", "3", "4", "5", "6" });
		Assert.That(listbox.Items.Count, Is.EqualTo(6));
		Assert.That(listbox.SelectedIndex, Is.EqualTo(0));
		Assert.That(listbox.SelectedItem, Is.EqualTo("1"));

		await listbox.InvokeWinzorDispatcherAsync(() => listbox.SelectedIndex = 4);
		Assert.That(listbox.SelectedIndex, Is.EqualTo(4));
		Assert.That(listbox.SelectedItem.ToString(), Is.EqualTo("5"));
	}

	[TestCase(DrawMode.Normal, false)]
	[TestCase(DrawMode.OwnerDrawFixed, true)]
	[TestCase(DrawMode.OwnerDrawVariable, true)]
	public async Task ListBoxDrawItemEventInvocation(DrawMode drawMode, bool shouldInvoke)
	{
		var invokeCounts = 0;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			var listBox = new ListBox() { DrawMode = drawMode };
			listBox.Items.AddRange(new string[] { "item1", "item2", "item3" });

			listBox.DrawItem += new DrawItemEventHandler((object sender, DrawItemEventArgs e) =>
			{
				invokeCounts++;
			});

			return listBox;
		});

		Assert.That(invokeCounts, shouldInvoke ? Is.GreaterThan(0) : Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task ItemTextColor()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var listBox = new ListBox();
			listBox.Items.AddRange(new string[] { "item1", "item2", "item3" });
			listBox.ListBoxItemsData[0].TextColor = Color.Red;
			listBox.ListBoxItemsData[1].TextColor = Color.Black;
			listBox.ListBoxItemsData[2].TextColor = Color.FromArgb(11, 12, 13);
			return listBox;
		});

		Assert.That(await page.Locator(".listbox__item").Nth(0).GetComputedStyleAsync("color"), Is.EqualTo("rgb(255, 0, 0)"));
		Assert.That(await page.Locator(".listbox__item").Nth(1).GetComputedStyleAsync("color"), Is.EqualTo("rgb(0, 0, 0)"));
		Assert.That(await page.Locator(".listbox__item").Nth(2).GetComputedStyleAsync("color"), Is.EqualTo("rgb(11, 12, 13)"));
	}

	[Test, WithPlaywrightPage]
	public async Task SelectedItemTextColor()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var listBox = new ListBox();
			listBox.Items.AddRange(new string[] { "item1", "item2", "item3" });
			return listBox;
		});

		Assert.That(await page.Locator(".listbox__item").Nth(0).GetComputedStyleAsync("color"), Is.EqualTo("rgb(0, 0, 0)"));
		Assert.That(await page.Locator(".listbox__item").Nth(1).GetComputedStyleAsync("color"), Is.EqualTo("rgb(0, 0, 0)"));
		Assert.That(await page.Locator(".listbox__item").Nth(2).GetComputedStyleAsync("color"), Is.EqualTo("rgb(0, 0, 0)"));

		await page.Locator(".listbox__item").Nth(1).ClickAsync();

		Assert.That(await page.Locator(".listbox__item").Nth(0).GetComputedStyleAsync("color"), Is.EqualTo("rgb(0, 0, 0)"));
		Assert.That(async () => await page.Locator(".listbox__item").Nth(1).GetComputedStyleAsync("color"), Is.EqualTo("rgb(255, 255, 255)").After(2000, 100));
		Assert.That(await page.Locator(".listbox__item").Nth(2).GetComputedStyleAsync("color"), Is.EqualTo("rgb(0, 0, 0)"));
	}

	[Test, WithPlaywrightPage]
	public async Task ListBoxUsesDefaultBackColorIfEmpty()
	{
		await using var ctx = new InMemoryTestServerContext();
		ListBox listBox = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			listBox = new ListBox { Width = 200, Height = 100 };
			return listBox;
		});
		var listBoxLocator = page.Locator(".listbox");

		Assert.That(listBox.BackColor, Is.EqualTo(SystemColors.Window));
		Assert.That(await listBoxLocator.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(255, 255, 255)"));
	}

	[Test, WithPlaywrightPage]
	public async Task ListBoxUsesSpecifiedBackColor()
	{
		await using var ctx = new InMemoryTestServerContext();
		ListBox listBox = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			listBox = new ListBox { Width = 200, Height = 100, BackColor = Color.Blue };
			return listBox;
		});
		var listBoxLocator = page.Locator(".listbox");

		Assert.That(listBox.BackColor, Is.EqualTo(Color.Blue));
		Assert.That(await listBoxLocator.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(0, 0, 255)"));
	}

	[Test, WithPlaywrightPage]
	public async Task ListBoxInvokeSelectedIndexChangedAfterGotFocus()
	{
		await using var ctx = new InMemoryTestServerContext();
		string lastEventHandlerName;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new TextBox());
			var listBox = new ListBox();
			listBox.Items.AddRange(new string[] { "item1", "item2", "item3" });
			listBox.GotFocus += (sender, e) => lastEventHandlerName = "GotFocus";
			listBox.SelectedIndexChanged += (sender, e) => lastEventHandlerName = "SelectedIndexChanged";
			form.Controls.Add(listBox);
			return form;
		});

		lastEventHandlerName = string.Empty;
		await page.Locator(".listbox__item").Nth(0).ClickAsync();
		Assert.That(() => lastEventHandlerName, Is.EqualTo("SelectedIndexChanged").After(2000, 100));
	}

	[Test]
	public async Task ListBoxMouseDownFiresAfterSelection()
	{
		using var ctx = new WinzorTestContext();
		string lastEventHandlerName;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var listBox = new ListBox();
			listBox.Items.Add("item");
			listBox.MouseDown += (sender, e) => lastEventHandlerName = "MouseDown";
			listBox.SelectedIndexChanged += (sender, e) => lastEventHandlerName = "SelectedIndexChanged";
			return listBox;
		});

		lastEventHandlerName = string.Empty;
		await rendered.Find(".listbox__item").MouseDownAsync(new WebMouseEventArgs() { Detail = 2 });
		Assert.That(lastEventHandlerName, Is.EqualTo("MouseDown"));
	}
}
