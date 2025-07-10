using System.Drawing;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace System.Windows.Forms;

class ScrollableControlTest
{
	[Test, WithPlaywrightPage]
	public async Task ScrollableControlShouldHaveScrollBarWhenOverflow()
	{
		await using var ctx = new InMemoryTestServerContext();
		Panel panel = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(300, 200) };
			panel = new Panel() { AutoScroll = true, Width = 200, Height = 100 };
			var button1 = new Button()
			{
				Text = "button1",
				Height = 100,
				Width = 100,
				Top = 0,
				Left = 0,
			};
			var button2 = new Button()
			{
				Text = "button2",
				Height = 100,
				Width = 100,
				Top = 0,
				Left = 100,
			};
			panel.Controls.Add(button1);
			panel.Controls.Add(button2);
			form.Controls.Add(panel);
			return form;
		});

		var panelEl = await page.WaitForSelectorAsync(".panel");
		CheckScrollProperty(panelEl, false, false, "overflow", "hidden");

		await panel.InvokeWinzorDispatcherAsync(() => { panel.Size = new Size(150, 150); });
		CheckScrollProperty(panelEl, true, false, "overflow-x");

		await panel.InvokeWinzorDispatcherAsync(() => { panel.Size = new Size(250, 50); });
		CheckScrollProperty(panelEl, false, true, "overflow-y");

		await panel.InvokeWinzorDispatcherAsync(() => { panel.Size = new Size(150, 100); });
		CheckScrollProperty(panelEl, true, true, "overflow-x");

		await panel.InvokeWinzorDispatcherAsync(() => { panel.Size = new Size(200, 50); });
		CheckScrollProperty(panelEl, true, true, "overflow-y");

		await panel.InvokeWinzorDispatcherAsync(() => { panel.Size = new Size(150, 50); });
		CheckScrollProperty(panelEl, true, true, "overflow");
	}

	[Test, WithPlaywrightPage(Headless = false)]
	public async Task ScrollableControlShouldHaveDifferentScrollTypes()
	{
		await using var ctx = new InMemoryTestServerContext();
		Panel panel = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(150, 150) };
			panel = new Panel() { AutoScroll = true, Width = 100, Height = 100 };
			var button = new Button()
			{
				Text = "button",
				Height = 100,
				Width = 100,
				Top = 0,
				Left = 0,
			};

			panel.Controls.Add(button);
			form.Controls.Add(panel);
			return form;
		});

		var panelEl = await page.WaitForSelectorAsync(".panel");
		CheckScrollProperty(panelEl, false, false, "overflow", "hidden");

		await panel.InvokeWinzorDispatcherAsync(() => { panel.Size = new Size(50, 50); });
		CheckScrollProperty(panelEl, true, true, "overflow");

		await panel.InvokeWinzorDispatcherAsync(() => { panel.Size = new Size(50, 150); });
		CheckScrollProperty(panelEl, true, false, "overflow-x");

		await panel.InvokeWinzorDispatcherAsync(() => { panel.Size = new Size(150, 50); });
		CheckScrollProperty(panelEl, false, true, "overflow-y");
	}

	[Test, WithPlaywrightPage]
	public async Task ScrollableControlShouldOverflowWithPropertiesWhenFalseAutoScroll()
	{
		await using var ctx = new InMemoryTestServerContext();
		Panel panel = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(150, 150) };
			panel = new Panel() { AutoScroll = false, Width = 100, Height = 100 };
			var button = new Button()
			{
				Text = "button",
				Height = 100,
				Width = 100,
				Top = 0,
				Left = 0,
			};

			panel.Controls.Add(button);
			form.Controls.Add(panel);
			return form;
		});

		var panelEl = await page.WaitForSelectorAsync(".panel");
		CheckScrollProperty(panelEl, false, false, "overflow", "hidden");

		await panel.InvokeWinzorDispatcherAsync(() =>
		{
			panel.Size = new Size(50, 50);
		});
		CheckScrollProperty(panelEl, true, true, "overflow", "hidden");

		await panel.InvokeWinzorDispatcherAsync(() =>
		{
			panel.HorizontalScroll.Visible = true;
			panel.VerticalScroll.Visible = false;
		});
		CheckScrollProperty(panelEl, true, true, "overflow-x");
		CheckScrollProperty(panelEl, true, true, "overflow-y", "hidden");

		await panel.InvokeWinzorDispatcherAsync(() =>
		{
			panel.HorizontalScroll.Visible = false;
			panel.VerticalScroll.Visible = true;
		});
		CheckScrollProperty(panelEl, true, true, "overflow-x", "hidden");
		CheckScrollProperty(panelEl, true, true, "overflow-y");

		await panel.InvokeWinzorDispatcherAsync(() =>
		{
			panel.HorizontalScroll.Visible = true;
			panel.VerticalScroll.Visible = true;
		});
		CheckScrollProperty(panelEl, true, true, "overflow");
	}

	// Testing scrollbars in Headless mode is unreliable. - WI00642021
	[Test, WithPlaywrightPage(Headless = false)]
	public async Task ClientSizeDoesNotIncludeSpaceTakenFromHorizontalScrollbar()
	{
		await using var ctx = new InMemoryTestServerContext();
		Panel panel = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(120, 100) };
			panel = new Panel() { AutoScroll = true, Width = 100, Height = 100 };
			var button = new Button()
			{
				Text = "button",
				Height = 60,
				Width = 110,
				Top = 0,
				Left = 0,
			};
			panel.Controls.Add(button);
			form.Controls.Add(panel);
			return form;
		});

		await page.WaitForSelectorAsync(".panel");
		var panelEl = await page.QuerySelectorAsync(".panel");
		CheckScrollProperty(panelEl, true, false, "overflow-x");
		Assert.That(await panelEl.EvaluateAsync<int>("element =>  element.clientWidth"), Is.EqualTo(100));
		Assert.That(await panelEl.EvaluateAsync<int>("element => element.clientHeight"), Is.EqualTo(100 - SystemInformation.VerticalScrollBarWidth).Within(2));
	}

	// Testing scrollbars in Headless mode is unreliable. - WI00642021
	[Test, WithPlaywrightPage(Headless = false)]
	public async Task ClientSizeDoesNotIncludeSpaceTakenFromVerticalScrollbar()
	{
		await using var ctx = new InMemoryTestServerContext();
		Panel panel = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(100, 120) };
			panel = new Panel() { AutoScroll = true, Width = 100, Height = 100 };
			var button = new Button()
			{
				Text = "button",
				Height = 110,
				Width = 60,
				Top = 0,
				Left = 0,
			};
			panel.Controls.Add(button);
			form.Controls.Add(panel);
			return form;
		});

		await page.WaitForSelectorAsync(".panel");
		var panelEl = await page.QuerySelectorAsync(".panel");
		CheckScrollProperty(panelEl, false, true, "overflow-y");
		Assert.That(await panelEl.EvaluateAsync<int>("element =>  element.clientHeight"), Is.EqualTo(100));
		Assert.That(await panelEl.EvaluateAsync<int>("element => element.clientWidth"), Is.EqualTo(100 - SystemInformation.VerticalScrollBarWidth).Within(2));
	}

	[Test, WithPlaywrightPage]
	public async Task HorizontalScrollbarNotDisplayedWhenControlsFitInClientRectangle()
	{
		await using var ctx = new InMemoryTestServerContext();
		Panel panel = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(100, 110) };
			panel = new Panel() { AutoScroll = true, Width = 100, Height = 100 };
			var button = new Button()
			{
				Text = "button",
				Height = 110,
				Width = 50,
				Top = 0,
				Left = 0,
			};
			panel.Controls.Add(button);
			form.Controls.Add(panel);
			return form;
		});
		await page.WaitForSelectorAsync(".panel");
		var panelEl = await page.QuerySelectorAsync(".panel");
		CheckScrollProperty(panelEl, false, true, "overflow-y");
	}

	void CheckScrollProperty(IElementHandle panelEl, bool horizontalScroll, bool verticalScroll, string overflowCoordinate, string overflowType = "auto")
	{
		Assert.That(async () => await panelEl.EvaluateAsync<string>($"e => window.getComputedStyle(e).getPropertyValue('{overflowCoordinate}')"), Is.EqualTo(overflowType));
		Assert.That(async () => await panelEl.EvaluateAsync<bool>("element => element.scrollWidth > element.clientWidth"), Is.EqualTo(horizontalScroll));
		Assert.That(async () => await panelEl.EvaluateAsync<bool>("element => element.scrollHeight > element.clientHeight"), Is.EqualTo(verticalScroll));
	}

	[TestCase(true, true)]
	[TestCase(false, false)]
	public async Task TestDoLayoutOnVisibleChanged(bool controlVisible, bool doLayout)
	{
		using var ctx = new WinzorTestContext();
		Panel panel;
		var layoutTriggered = false;

		await ctx.RenderControlOnFormAsync(() =>
		{
			panel = new Panel { Width = 100, Height = 100, Visible = controlVisible };
			panel.Layout += delegate
			{
				layoutTriggered = true;
			};
			return panel;
		});

		Assert.That(layoutTriggered, Is.EqualTo(doLayout));
	}
}
