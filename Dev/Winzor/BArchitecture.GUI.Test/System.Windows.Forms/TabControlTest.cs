using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

[SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
[SuppressMessage("CargoWiseOne", "CW1111:DoNotUseSystemWindowsFormsTabDrawModeOwnerDrawFixed", Justification = "Testing")]
class TabControlTest
{
	[Test, WithPlaywrightPage]
	public async Task TabControlHasCorrectPadding()
	{
		await using var appServerCtx = new InMemoryTestServerContext();
		Form form = null;
		var page = await appServerCtx.LoadFormAsync(() =>
		{
			form = new Form();
			var tabControl = new TabControl();
			var tabPage = new TabPage();
			tabControl.TabPages.Add(tabPage);
			form.Controls.Add(tabControl);
			return form;
		});

		var tabControlNavigation = await page.WaitForSelectorAsync(".tabcontrol__navigation");
		var navigationTopPadding = await getComputedStyle(tabControlNavigation, "padding-top");
		Assert.That(navigationTopPadding, Is.EqualTo("2px"));
		var navigationLeftPadding = await getComputedStyle(tabControlNavigation, "padding-left");
		Assert.That(navigationLeftPadding, Is.EqualTo("2px"));
	}

	[TestCase(false, TestName = "{m}_SkipsOneOption")]
	[TestCase(true, TestName = "{m}_SkipsTwoOptions")]
	public async Task TabControlShowsFullTabOnClick(bool showsDetails)
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form { Width = 720, Height = 600 };
			var tabControl = new TabControl { Width = 720, Height = 600 };
			if (showsDetails)
			{
				var t1TabPage = new TabPage { Name = "T1", Text = "Details" };
				tabControl.TabPages.Add(t1TabPage);
			}
			var t2TabPage = new TabPage { Name = "T2", Text = "GST Tax Overrides" };
			var t3TabPage = new TabPage { Name = "T3", Text = "Type Overrides" };
			var t4TabPage = new TabPage { Name = "T4", Text = "Branch Overrides" };
			var t5TabPage = new TabPage { Name = "T5", Text = "Creditor Overrides" };
			var t6TabPage = new TabPage { Name = "T6", Text = "Revenue Recognition Overrides" };
			var t7TabPage = new TabPage { Name = "T7", Text = "Apportionment Method Overrides" };
			var t8TabPage = new TabPage { Name = "T8", Text = "GL Posting Overrides" };
			var t9TabPage = new TabPage { Name = "T9", Text = "eDocs" };

			tabControl.TabPages.Add(t2TabPage);
			tabControl.TabPages.Add(t3TabPage);
			tabControl.TabPages.Add(t4TabPage);
			tabControl.TabPages.Add(t5TabPage);
			tabControl.TabPages.Add(t6TabPage);
			tabControl.TabPages.Add(t7TabPage);
			tabControl.TabPages.Add(t8TabPage);
			tabControl.TabPages.Add(t9TabPage);

			form.Controls.Add(tabControl);
			return form;
		});

		Assert.That(rendered.Markup, Does.Contain("GST Tax Overrides"));
		Assert.That(rendered.Markup, Does.Not.Contain("GL Posting Overrides"));
		Assert.That(rendered.Markup, Does.Not.Contain("eDocs").IgnoreCase);
		await rendered.Find("button:contains('Apportionment Method Overrides')").ClickAsync(new WebMouseEventArgs());
		Assert.That(rendered.Markup, Does.Contain("GL Posting Overrides").IgnoreCase);
		Assert.That(rendered.Markup, Does.Not.Contain("GST Tax Overrides").IgnoreCase);
		Assert.That(rendered.Markup, Does.Not.Contain("eDocs").IgnoreCase);
	}

	[Test, WithPlaywrightPage]
	public async Task ActiveButtonStyles()
	{
		await using var appServerCtx = new InMemoryTestServerContext();
		Form form = null;
		var page = await appServerCtx.LoadFormAsync(() =>
		{
			form = new Form();
			var tabControl = new TabControl();
			var fooTabPage = new TabPage { Name = "Foo", Text = "Foo Fighter" };
			fooTabPage.Controls.Add(new Label { Text = "Foo Label" });

			var barTabPage = new TabPage { Name = "Bar", Text = "Bar Room Blitz" };
			barTabPage.Controls.Add(new Label { Text = "Bar Label" });

			tabControl.TabPages.Add(fooTabPage);
			tabControl.TabPages.Add(barTabPage);

			form.Controls.Add(tabControl);
			return form;
		});

		var activeButton = await page.WaitForSelectorAsync(".tabcontrol > .tabcontrol__navigation > .tabcontrol__tabs > .active");
		var activeButtonLeftMargin = await getComputedStyle(activeButton, "margin-left");
		Assert.That(activeButtonLeftMargin, Is.EqualTo("-2px"));
	}

	[Test]
	public async Task TabPageHasCorrectTopOffset()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl() { Top = 10 };
			var tabPage = new TabPage { Name = "Foo", Text = "Foo Fighter" };
			tabPage.Controls.Add(new Label { Text = "Foo Label" });
			tabControl.TabPages.Add(tabPage);
			return tabControl;
		});

		var tabPage = rendered.Find(".tabcontrol__page");
		Assert.That(tabPage.GetAttribute("style"), Does.Contain("top:24px;"));
	}

	[Test, WithPlaywrightPage]
	public async Task TabControlContentBackColorShouldAlwaysBeWhite()
	{
		await using var ctx = new InMemoryTestServerContext();

		TabControl tabControl = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			tabControl = new TabControl();
			tabControl.Dock = DockStyle.Fill;
			return tabControl;
		});

		var tabcontrolContent = await page.WaitForSelectorAsync(".tabcontrol > .tabcontrol__content");
		Assert.That(await tabcontrolContent.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(255, 255, 255)"));
	}

	[Test, WithPlaywrightPage]
	public async Task ChildControlInTabControlShouldHaveCorrectBackColor()
	{
		await using var ctx = new InMemoryTestServerContext();

		TabControl tabControl = null;

		TabPage tabPage = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			tabControl = new TabControl();
			tabControl.Dock = DockStyle.Fill;

			tabPage = new TabPage();
			tabPage.Text = "Foo";
			tabPage.TabIndex = 0;
			tabPage.BackColor = Color.Transparent;
			tabControl.TabPages.Add(tabPage);

			form.Controls.Add(tabControl);
			return form;
		});

		var tabPageElement = await page.WaitForSelectorAsync(".tabcontrol__page");
		Assert.That(async () => await tabPageElement.GetAttributeAsync("style"), Does.Contain("background-color:var(--color-control);"));

		await tabPage.InvokeWinzorDispatcherAsync(() => tabPage.BackColor = Color.DodgerBlue);
		tabPageElement = await page.WaitForSelectorAsync(".tabcontrol__page");
		Assert.That(async () => await tabPageElement.GetAttributeAsync("style"), Does.Contain("background-color:#1E90FFFF;"));
	}

	[Test, WithPlaywrightPage]
	public async Task TabPageShouldHaveCorrectBackColor()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 500, Height = 500 };

			TabControl tabControl = new TabControl();
			tabControl.Dock = DockStyle.Fill;

			TabPage tabPage = new TabPage();
			tabPage.Text = "Foo";
			tabPage.TabIndex = 0;
			tabPage.BackColor = Color.Red;
			tabPage.UseVisualStyleBackColor = true;
			tabControl.TabPages.Add(tabPage);

			form.Controls.Add(tabControl);

			return form;
		});

		var tabPageElement = page.Locator(".tabcontrol__page");
		Assert.That(await tabPageElement.GetComputedStyleAsync("background-color"), Is.EqualTo("rgba(0, 0, 0, 0)"));
	}

	[Test]
	public async Task TabControlOnLeaveShouldTriggerSelectedTabPageFireLeave()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBoxOutSide = null;
		TextBox textBoxInSide = null;
		TabPageForTest tabPage = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form() { Size = new(600, 600) };
			var tabControl = new TabControl() { Dock = DockStyle.Fill };
			textBoxInSide = new TextBox();
			textBoxOutSide = new TextBox() { Dock = DockStyle.Top };
			tabPage = new TabPageForTest();

			form.Controls.Add(textBoxOutSide);
			form.Controls.Add(tabControl);

			tabPage.Controls.Add(textBoxInSide);
			tabControl.TabPages.Add(tabPage);

			return form;
		});

		await textBoxInSide.InvokeWinzorDispatcherAsync(() => textBoxInSide.Focus());
		Assert.That(!tabPage.IsOnLeaveFired);

		await textBoxOutSide.InvokeWinzorDispatcherAsync(() => textBoxOutSide.Focus());
		Assert.That(tabPage.IsOnLeaveFired);
	}

	[Test]
	public async Task TabControlOnLeaveShouldTriggerFromPreviousTabWhenClickNewTabPage()
	{
		using var ctx = new WinzorTestContext();
		TabPageForTest tabPage1 = null;
		TabPageForTest tabPage2 = null;
		TabControl tabControl = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			tabControl = new TabControl();
			tabPage1 = new TabPageForTest { Name = "One", Text = "One" };
			tabPage2 = new TabPageForTest { Name = "Two", Text = "Two" };
			tabControl.TabPages.Add(tabPage1);
			tabControl.TabPages.Add(tabPage2);
			form.Controls.Add(tabControl);
			return form;
		});

		var firstTabPage = rendered.Find("button:contains('One')");
		var secondTabPage = rendered.Find("button:contains('Two')");

		Assert.That(tabPage1.IsOnLeaveFired, Is.False);
		Assert.That(tabPage2.IsOnLeaveFired, Is.False);

		await firstTabPage.ClickAsync(new WebMouseEventArgs());
		Assert.That(tabPage1.IsOnLeaveFired, Is.False);
		Assert.That(tabPage2.IsOnLeaveFired, Is.False);
		Assert.That(tabPage1.OnLeaveCount, Is.EqualTo(0));
		Assert.That(tabPage2.OnLeaveCount, Is.EqualTo(0));

		await secondTabPage.ClickAsync(new WebMouseEventArgs());
		Assert.That(tabPage1.IsOnLeaveFired, Is.True);
		Assert.That(tabPage2.IsOnLeaveFired, Is.False);
		Assert.That(tabPage1.OnLeaveCount, Is.EqualTo(1));
		Assert.That(tabPage2.OnLeaveCount, Is.EqualTo(0));

		tabPage1.IsOnLeaveFired = false;
		tabPage1.OnLeaveCount = 0;
		await firstTabPage.ClickAsync(new WebMouseEventArgs());
		Assert.That(tabPage1.IsOnLeaveFired, Is.False);
		Assert.That(tabPage2.IsOnLeaveFired, Is.True);
		Assert.That(tabPage1.OnLeaveCount, Is.EqualTo(0));
		Assert.That(tabPage2.OnLeaveCount, Is.EqualTo(1));

		tabPage2.OnLeaveCount = 0;
		tabPage2.IsOnLeaveFired = false;
		Assert.That(tabControl.SelectedIndex, Is.EqualTo(0));
		await tabControl.InvokeWinzorDispatcherAsync(() => tabControl.SelectedIndex = 1);
		Assert.That(tabPage1.IsOnLeaveFired, Is.True);
		Assert.That(tabPage2.IsOnLeaveFired, Is.False);
		Assert.That(tabPage1.OnLeaveCount, Is.EqualTo(1));
		Assert.That(tabPage2.OnLeaveCount, Is.EqualTo(0));
	}

	[Test]
	public async Task TabControlOnEnterShouldTriggerSelectedTabPageFireEnter()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBoxOutSide = null;
		TextBox textBoxInSide = null;
		TabPageForTest tabPage = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form() { Size = new(600, 600) };
			var tabControl = new TabControl() { Dock = DockStyle.Fill };
			textBoxInSide = new TextBox();
			textBoxOutSide = new TextBox() { Dock = DockStyle.Top };
			tabPage = new TabPageForTest();

			form.Controls.Add(textBoxOutSide);
			form.Controls.Add(tabControl);

			tabPage.Controls.Add(textBoxInSide);
			tabControl.TabPages.Add(tabPage);

			return form;
		});

		Assert.That(!tabPage.IsOnEnterFired);

		await textBoxInSide.InvokeWinzorDispatcherAsync(() => textBoxInSide.Focus());
		Assert.That(tabPage.IsOnEnterFired);
	}

	[Test, WithPlaywrightPage]
	public async Task TabControlCanTriggerSelectedTabPageFireEnterByTabkey()
	{
		TextBox textBoxOutSide = null;
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		TabPageForTest tabPage = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();

			textBoxOutSide = new TextBox() { Dock = DockStyle.Top };
			form.Controls.Add(textBoxOutSide);

			var tabControl = new TabControl { Dock = DockStyle.Bottom };
			tabPage = new TabPageForTest();
			tabPage.Text = "Foo";
			tabPage.TabIndex = 0;
			tabControl.TabPages.Add(tabPage);

			var textBox = new TextBox();
			tabPage.Controls.Add(textBox);

			form.Controls.Add(tabControl);
			return form;
		});

		var textBoxElement = await page.WaitForSelectorAsync("input");
		await textBoxElement.ClickAsync();

		Assert.That(tabPage.IsOnEnterFired, Is.False);
		await page.Keyboard.PressAsync("Tab", new KeyboardPressOptions { Delay = 1000 });
		Assert.That(tabPage.IsOnEnterFired, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task TabControlCanMoveTabPageByArrowRightAndArrowKey()
	{
		KeyboardPressOptions keyboardPressOptions = new KeyboardPressOptions { Delay = 500 };
		TextBox textBoxOutSide = null;
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		TabControl tabControl = null;
		TabPageForTest tabPage = null;
		TabPageForTest tabPage2 = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			textBoxOutSide = new TextBox() { Dock = DockStyle.Top };
			form.Controls.Add(textBoxOutSide);
			tabControl = new TabControl { Dock = DockStyle.Bottom };
			tabPage = new TabPageForTest();
			tabPage.Text = "Foo";
			tabPage.TabIndex = 0;
			tabPage2 = new TabPageForTest();
			tabPage2.Text = "Apple";
			tabPage2.TabIndex = 0;
			var t2TabPage = new TabPageForTest { Name = "T2", Text = "GST Tax Overrides" };
			var t3TabPage = new TabPageForTest { Name = "T3", Text = "Type Overrides" };
			var t4TabPage = new TabPageForTest { Name = "T4", Text = "Branch Overrides" };
			var t5TabPage = new TabPageForTest { Name = "T5", Text = "Creditor Overrides" };
			tabControl.TabPages.Add(tabPage);
			tabControl.TabPages.Add(tabPage2);
			tabControl.TabPages.Add(t2TabPage);
			tabControl.TabPages.Add(t3TabPage);
			tabControl.TabPages.Add(t4TabPage);
			tabControl.TabPages.Add(t5TabPage);

			var textBox = new TextBox();
			tabPage.Controls.Add(textBox);
			form.Controls.Add(tabControl);

			return form;
		});
		var textBoxElement = await page.WaitForSelectorAsync("input");
		await textBoxElement.ClickAsync();

		await page.Keyboard.PressAsync("Tab", keyboardPressOptions);
		Assert.That(tabPage.IsOnEnterFired, Is.True);
		Assert.That(tabControl.SelectedIndex, Is.EqualTo(0));
		int i = 0;
		while (++i < 5)
		{
			await page.Keyboard.PressAsync("ArrowRight", keyboardPressOptions);
			Assert.That(tabControl.SelectedIndex, Is.EqualTo(i));
			Assert.That(tabControl.Focused, Is.True);
			Assert.That(tabControl.GetVisibleTabs(), Does.Contain(tabControl.TabPages[i]));
		}
		i--;
		while (--i > 0)
		{
			await page.Keyboard.PressAsync("ArrowLeft", keyboardPressOptions);
			Assert.That(tabControl.SelectedIndex, Is.EqualTo(i));
			Assert.That(tabControl.Focused, Is.True);
			Assert.That(tabControl.GetVisibleTabs(), Does.Contain(tabControl.TabPages[i]));
		}
	}

	[TestCase, WithPlaywrightPage]
	public async Task ValidateCurrentTabIsActiveWhenNavigatingToHiddenTab()
	{
		var keyboardPressOptions = new KeyboardPressOptions { Delay = 100 };

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var textBoxOutSide = new TextBox { Dock = DockStyle.Top };
			var tabControl = new TabControl { Dock = DockStyle.Bottom };
			var t0TabPage = new TabPageForTest { Name = "T0", Text = "Foo", TabIndex = 0 };
			var t1TabPage = new TabPageForTest { Name = "T1", Text = "Apple", TabIndex = 0 };
			var t2TabPage = new TabPageForTest { Name = "T2", Text = "GST Tax Overrides" };
			var t3TabPage = new TabPageForTest { Name = "T3", Text = "Type Overrides" };
			var t4TabPage = new TabPageForTest { Name = "T4", Text = "Branch Overrides" };
			var t5TabPage = new TabPageForTest { Name = "T5", Text = "Creditor Overrides" };
			var t6TabPage = new TabPageForTest { Name = "T6", Text = "A New Tab" };
			var t7TabPage = new TabPageForTest { Name = "T7", Text = "A New New Tab" };
			tabControl.TabPages.Add(t0TabPage);
			tabControl.TabPages.Add(t1TabPage);
			tabControl.TabPages.Add(t2TabPage);
			tabControl.TabPages.Add(t3TabPage);
			tabControl.TabPages.Add(t4TabPage);
			tabControl.TabPages.Add(t5TabPage);
			tabControl.TabPages.Add(t6TabPage);
			tabControl.TabPages.Add(t7TabPage);

			t0TabPage.Controls.Add(new TextBox());
			form.Controls.Add(textBoxOutSide);
			form.Controls.Add(tabControl);

			return form;
		}, width: 1000, height: 600);

		await page.WaitForSelectorAsync("input");

		var buttons = await page.QuerySelectorAllAsync("button");
		await buttons[3].ClickAsync();

		int currentTabIndex = 3;
		while (currentTabIndex++ <= 7)
		{
			await page.Keyboard.PressAsync("ArrowRight", keyboardPressOptions);
		}
		await Task.Delay(500); // delay afterrender action gets completed
		Assert.That(async () => await page.EvaluateAsync<string>("document.activeElement.innerText"), Is.EqualTo("A New New Tab").After(500, 100));

		while (--currentTabIndex > 0)
		{
			await page.Keyboard.PressAsync("ArrowLeft", keyboardPressOptions);
		}
		await Task.Delay(500); // delay afterrender action gets completed
		Assert.That(async () => await page.EvaluateAsync<string>("document.activeElement.innerText"), Is.EqualTo("Foo").After(500, 100));
	}

	[Test]
	public async Task TabControlHasCorrectClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			var tabPage = new TabPage();
			tabControl.TabPages.Add(tabPage);
			return tabControl;
		});

		Assert.That(rendered.Find(".tabcontrol__content").GetAttribute("Class"), Contains.Substring("tabcontrol__content"));
		Assert.That(rendered.Find(".tabcontrol__page").GetAttribute("Class"), Contains.Substring("tabcontrol__page"));
	}

	[Test]
	public async Task TabControlHasCorrectBackgroundColor()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			var tabPage = new TabPage();
			tabControl.TabPages.Add(tabPage);
			tabPage.BackColor = SystemColors.Control;
			return tabControl;
		});

		Assert.That(rendered.Find(".tabcontrol__page").GetAttribute("style"), Does.Contain("background-color:var(--color-control)"));
	}

	[Test]
	public async Task OnSelectedIndexChanged()
	{
		await ControlAssert.ImplementsProtectedOnMethodAsync<TabControl, EventArgs>(
			"OnSelectedIndexChanged",
			tabControl =>
			{
				tabControl.TabPages.Add(new TabPage { Name = "Foo", Text = "Foo" });
				tabControl.TabPages.Add(new TabPage { Name = "Bar", Text = "Bar" });
				tabControl.SelectedIndex = 0;
			},
			"button:contains('Bar')",
			e => e.Click()
		);
	}

	[Test]
	public async Task SelectedIndexChanged()
	{
		await ControlAssert.ImplementsEventAsync<TabControl, EventHandler>(
			nameof(TabControl.SelectedIndexChanged),
			tabControl =>
			{
				tabControl.TabPages.Add(new TabPage { Name = "Foo", Text = "Foo" });
				tabControl.TabPages.Add(new TabPage { Name = "Bar", Text = "Bar" });
				tabControl.SelectedIndex = 0;
			},
			a => new EventHandler((s, e) => a()),
			"button:contains('Bar')",
			e => e.Click()
		);
	}

	[Test]
	public async Task TabPageChanges()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			var fooTabPage = new TabPage { Name = "Foo", Text = "Foo Fighter" };
			fooTabPage.Controls.Add(new Label { Text = "Foo Label" });
			var barTabPage = new TabPage { Name = "Bar", Text = "Bar Room Blitz" };
			barTabPage.Controls.Add(new Label { Text = "Bar Label" });
			tabControl.TabPages.Add(fooTabPage);
			tabControl.TabPages.Add(barTabPage);
			return tabControl;
		});

		Assert.That(rendered.Markup, Does.Contain("Foo Label"));
		Assert.That(rendered.Markup, Does.Not.Contain("Bar Label"));
		await rendered.Find("button:contains('Bar Room Blitz')").ClickAsync(new WebMouseEventArgs());
		Assert.That(rendered.Markup, Does.Contain("Bar Label").IgnoreCase);
		Assert.That(rendered.Markup, Does.Not.Contain("Foo Label").IgnoreCase);
	}

	[Test]
	public async Task TabPageEnterEventRaisedWhenSelected()
	{
		using var ctx = new WinzorTestContext();
		var enterEventRaised = false;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			var fooTabPage = new TabPage { Name = "Foo", Text = "Foo Fighter" };
			var barTabPage = new TabPage { Name = "Bar", Text = "Bar Room Blitz" };
			barTabPage.Enter += (sender, e) => enterEventRaised = true;

			fooTabPage.Controls.Add(new Label { Text = "Foo Label" });
			barTabPage.Controls.Add(new Label { Text = "Bar Label" });
			tabControl.TabPages.Add(fooTabPage);
			tabControl.TabPages.Add(barTabPage);

			return tabControl;
		});

		await rendered.Find("button:contains('Bar Room Blitz')").ClickAsync(new WebMouseEventArgs());
		Assert.That(enterEventRaised, Is.True);
	}

	[Test]
	public async Task TabPageLeaveEventRaisedWhenSelectedOtherTabPage()
	{
		using var ctx = new WinzorTestContext();
		var leaveEventRaised = false;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			var fooTabPage = new TabPage { Name = "Foo", Text = "Foo Fighter" };
			var barTabPage = new TabPage { Name = "Bar", Text = "Bar Room Blitz" };
			barTabPage.Leave += (sender, e) => leaveEventRaised = true;

			fooTabPage.Controls.Add(new TextBox { Text = "Foo TextBox" });
			barTabPage.Controls.Add(new TextBox { Text = "Bar TextBox" });
			tabControl.TabPages.Add(fooTabPage);
			tabControl.TabPages.Add(barTabPage);
			return tabControl;
		});

		await rendered.Find("button:contains('Bar Room Blitz')").ClickAsync(new WebMouseEventArgs());
		await rendered.Find("button:contains('Foo Fighter')").ClickAsync(new WebMouseEventArgs());
		Assert.That(leaveEventRaised, Is.True);
	}

	[Test]
	public async Task TabControlShouldHaveButtonsWithStylingClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			var fooTabPage = new TabPage { Name = "Foo", Text = "Foo Fighter" };
			fooTabPage.Controls.Add(new Label { Text = "Foo Label" });
			var barTabPage = new TabPage { Name = "Bar", Text = "Bar Room Blitz" };
			barTabPage.Controls.Add(new Label { Text = "Bar Label" });
			tabControl.TabPages.Add(fooTabPage);
			tabControl.TabPages.Add(barTabPage);
			return tabControl;
		});

		var tabControlTabs = rendered.Find(".tabcontrol > .tabcontrol__navigation > .tabcontrol__tabs");
		Assert.That(tabControlTabs, Is.Not.Null);
		var buttons = tabControlTabs.Children.Where(c => c.ClassName.Contains("tabcontrol__button", StringComparison.Ordinal)).ToList();
		Assert.That(buttons.Count, Is.EqualTo(2));
	}

	[Test]
	public async Task TabControlShouldHavePageWithStylingClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			var tabPage = new TabPage { Name = "Foo", Text = "Foo Fighter" };
			tabPage.Controls.Add(new Label { Text = "Foo Label" });
			tabControl.TabPages.Add(tabPage);
			return tabControl;
		});

		var tabPage = rendered.Find(".tabcontrol__page");
		Assert.That(tabPage, Is.Not.Null);
	}

	[Test]
	public async Task TabShouldBeOrderedCorrectly()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl() { Width = 1000, Height = 500 };
			var bravoTabPage = new TabPage { Name = "Bravo", Text = "Bravo Text" };
			var deltaTabPage = new TabPage { Name = "Delta", Text = "Delta Text" };
			var charlieTabPage = new TabPage { Name = "Charlie", Text = "Charlie Text" };
			var alphaTabPage = new TabPage { Name = "Alpha", Text = "Alpha Text" };
			tabControl.TabPages.Add(bravoTabPage);
			tabControl.TabPages.Add(deltaTabPage);
			tabControl.TabPages.Add(charlieTabPage);
			tabControl.TabPages.Add(alphaTabPage);
			return tabControl;
		});

		var tabControlTabs = rendered.Find(".tabcontrol > .tabcontrol__navigation > .tabcontrol__tabs");
		var buttons = tabControlTabs.Children.ToList();
		Assert.That(buttons[0].TextContent, Is.EqualTo("Bravo Text"));
		Assert.That(buttons[1].TextContent, Is.EqualTo("Delta Text"));
		Assert.That(buttons[2].TextContent, Is.EqualTo("Charlie Text"));
		Assert.That(buttons[3].TextContent, Is.EqualTo("Alpha Text"));
	}

	[Test]
	public async Task TabPageControlVisibility()
	{
		using var ctx = new WinzorTestContext();
		var visibleChangedEventRaised = false;
		TextBox textBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			var tabPage1 = new TabPage { Name = "One", Text = "One" };
			var tabPage2 = new TabPage { Name = "Two", Text = "Two" };
			textBox = new TextBox();
			textBox.VisibleChanged += TextBox_VisibleChanged;
			tabPage2.Controls.Add(textBox);
			tabControl.TabPages.Add(tabPage1);
			tabControl.TabPages.Add(tabPage2);
			tabControl.SelectedIndex = 0;
			return tabControl;
		});

		Assert.That(visibleChangedEventRaised, Is.False);
		Assert.That(textBox.Visible, Is.False);

		await rendered.Find("button:contains('Two')").ClickAsync(new WebMouseEventArgs());
		Assert.That(visibleChangedEventRaised, Is.True);
		Assert.That(textBox.Visible, Is.True);

		void TextBox_VisibleChanged(object sender, EventArgs e)
		{
			visibleChangedEventRaised = true;
		}
	}

	[Test]
	public async Task TabPageParentSetToNullAfterRemoveFromTabControl()
	{
		using var ctx = new WinzorTestContext();
		TabControl tabControl = null;
		TabPage tabPage = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl();
			tabPage = new TabPage() { Name = "Uno", Text = "Uno text" };
			tabControl.TabPages.Add(tabPage);
			return tabControl;
		});

		Assert.That(tabPage.Parent, Is.EqualTo(tabControl));
		await tabControl.InvokeWinzorDispatcherAsync(() => tabControl.TabPages.Remove(tabPage));
		Assert.That(tabPage.Parent, Is.Null);
	}

	[Test]
	public async Task TestTabPageHasCorrectTabIndex([Values] bool tabStop)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			var fooTabPage = new TabPage { Name = "Foo", Text = "Foo Fighter" };
			fooTabPage.Controls.Add(new Label { Text = "Foo Label" });
			var barTabPage = new TabPage { Name = "Bar", Text = "Bar Room Blitz" };
			barTabPage.Controls.Add(new Label { Text = "Bar Label" });
			tabControl.TabPages.Add(fooTabPage);
			tabControl.TabPages.Add(barTabPage);
			tabControl.SelectedIndex = 0;
			tabControl.TabStop = tabStop;
			return tabControl;
		});

		var tabControlTabs = rendered.Find(".tabcontrol > .tabcontrol__navigation > .tabcontrol__tabs");
		Assert.That(tabControlTabs, Is.Not.Null);
		var buttons = tabControlTabs.Children.Where(c => c.ClassName.Contains("tabcontrol__button", StringComparison.Ordinal)).ToList();

		if (tabStop)
		{
			Assert.That(buttons[0].ClassList, Does.Contain("active"));
			Assert.That(buttons[0].GetAttribute("tabindex"), Is.EqualTo("0"));
			Assert.That(buttons[1].ClassList, Does.Not.Contain("active"));
			Assert.That(buttons[1].GetAttribute("tabindex"), Is.EqualTo("-1"));
		}
		else
		{
			Assert.That(buttons[0].ClassList, Does.Contain("active"));
			Assert.That(buttons[0].GetAttribute("tabindex"), Is.EqualTo("-1"));
			Assert.That(buttons[1].ClassList, Does.Not.Contain("active"));
			Assert.That(buttons[1].GetAttribute("tabindex"), Is.EqualTo("-1"));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task SelectedTabControlsShouldHaveAppropriateStylings()
	{
		await using var ctx = new InMemoryTestServerContext();
		TabControl tabControl = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			tabControl = new TabControl();
			var fooTabPage = new TabPage { Name = "Tab1", Text = "Tab 1" };
			fooTabPage.Controls.Add(new Label { Text = "Tab1 Label" });
			tabControl.TabPages.Add(fooTabPage);

			var fooTabPage2 = new TabPage { Name = "Tab2", Text = "Tab 2" };
			fooTabPage2.Controls.Add(new Label { Text = "Tab2 Label" });
			tabControl.TabPages.Add(fooTabPage2);

			var barTabPage = new TabPage { Name = "Tab3", Text = "Tab 3" };
			barTabPage.Controls.Add(new Label { Text = "Tab3 Label" });
			tabControl.TabPages.Add(barTabPage);

			form.Controls.Add(tabControl);
			return form;
		});
		var selectedTab = page.Locator("button", new PageLocatorOptions() { HasTextString = "Tab 2" });
		Assert.That(async () => await selectedTab.InnerTextAsync(), Is.EqualTo("Tab 2"));
		await page.WaitForTimeoutAsync(100);

		await tabControl.InvokeWinzorDispatcherAsync(() =>
		{
			tabControl.SelectedTab = tabControl.TabPages[1];
		});

		Assert.That(async () => await selectedTab.GetAttributeAsync("class"), Contains.Substring("active").After(100));

		var selectedTabLeftMargin = await getComputedStyle(selectedTab, "margin-top");
		Assert.That(selectedTabLeftMargin, Is.EqualTo("-3px"));
		var selectedTabBorderBottom = await getComputedStyle(selectedTab, "border-bottom");
		Assert.That(selectedTabBorderBottom, Is.EqualTo("0px none rgb(0, 0, 0)"));
		var selectedTabBorderRight = await getComputedStyle(selectedTab, "border-right");
		Assert.That(selectedTabBorderRight, Is.EqualTo("1px solid rgb(220, 220, 220)"));

		var immediateSelectedTab = page.Locator("button", new PageLocatorOptions() { HasTextString = "Tab 3" });
		var immediateSelecteTabBorderBottom = await getComputedStyle(immediateSelectedTab, "border-left");
		Assert.That(selectedTabBorderBottom, Is.EqualTo("0px none rgb(0, 0, 0)"));
		var immediateSelecteTabBorderRight = await getComputedStyle(immediateSelectedTab, "border-bottom");
		Assert.That(immediateSelecteTabBorderRight, Is.EqualTo("1px solid rgb(220, 220, 220)"));

		var nonSelectedTab = page.Locator("button", new PageLocatorOptions() { HasTextString = "Tab 1" });
		var nonSelectedTabBorderLeft = await getComputedStyle(nonSelectedTab, "border-left");
		Assert.That(nonSelectedTabBorderLeft, Is.EqualTo("1px solid rgb(220, 220, 220)"));
		var nonSelectedTabBorderBottom = await getComputedStyle(nonSelectedTab, "border-bottom");
		Assert.That(nonSelectedTabBorderBottom, Is.EqualTo("1px solid rgb(220, 220, 220)"));
	}

	[Test, WithPlaywrightPage]
	public async Task TextChangeShouldUpdateTabPageText()
	{
		await using var ctx = new InMemoryTestServerContext();

		TabControl tabControl = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			tabControl = new TabControl();
			var fooTabPage = new TabPage { Name = "Tab1", Text = "Tab 1" };
			fooTabPage.Controls.Add(new Label { Text = "Tab1 Label" });
			tabControl.TabPages.Add(fooTabPage);

			tabControl.SelectedIndex = 0;
			form.Controls.Add(tabControl);
			return form;
		});

		var tabPage2 = await page.Locator("button", new PageLocatorOptions { HasText = "Tab 1" }).ElementHandleAsync();
		Assert.That(async () => await tabPage2.InnerTextAsync(), Is.EqualTo("Tab 1"));

		await page.WaitForTimeoutAsync(100);

		await tabControl.InvokeWinzorDispatcherAsync(() =>
		{
			tabControl.TabPages[0].Text = "Test";
		});

		Assert.That(async () => await tabPage2.InnerTextAsync(), Is.EqualTo("Test").After(100));
	}

	[Test]
	public async Task TabControlImageDisplayedIfImageIndexSet()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			tabControl.ImageList = new ImageList();
			tabControl.ImageList.Images.Add(new Bitmap(32, 32));
			var tabPage = new TabPage { Text = "Tab", ImageIndex = 0 };
			tabControl.TabPages.Add(tabPage);
			return tabControl;
		});

		var tabPageImages = rendered.FindAll(".tabcontrol__tabimage img");
		Assert.That(tabPageImages.Count, Is.EqualTo(1));
	}

	[Test]
	public async Task TabControlNoImageDisplayedIfImageIndexNotSet()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			tabControl.ImageList = new ImageList();
			tabControl.ImageList.Images.Add(new Bitmap(32, 32));
			var tabPage = new TabPage { Text = "Tab" };
			tabControl.TabPages.Add(tabPage);
			return tabControl;
		});

		var tabPageImages = rendered.FindAll(".tabcontrol__tabimage img");
		Assert.That(tabPageImages.Count, Is.EqualTo(0));
	}

	[Test]
	public async Task TabControlImageSizeIsSet()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			tabControl.ImageList = new ImageList();
			tabControl.ImageList.Images.Add(new Bitmap(32, 32));
			tabControl.ImageList.ImageSize = new Size(20, 20);
			var tabPage = new TabPage { Text = "Tab", ImageIndex = 0 };
			tabControl.TabPages.Add(tabPage);
			return tabControl;
		});

		var tabPageImage = rendered.Find(".tabcontrol__tabimage img");
		Assert.That(tabPageImage.GetAttribute("style"), Is.EqualTo("width:20px;height:20px;"));
	}

	[Test]
	public async Task TabControlResizeUpdatesTabPageSize()
	{
		using var ctx = new WinzorTestContext();
		TabControl tabControl = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl() { Width = 500, Height = 500 };
			var tabPage = new TabPage { Name = "One", Text = "One" };
			tabControl.TabPages.Add(tabPage);
			tabControl.SelectedIndex = 0;
			return tabControl;
		});

		var tabControlComponent = rendered.Find(".tabcontrol");
		Assert.That(tabControlComponent.GetAttribute("style"), Does.Contain("width:500px"));
		Assert.That(tabControlComponent.GetAttribute("style"), Does.Contain("height:500px"));

		var tabPage = rendered.Find(".tabcontrol__page");
		Assert.That(tabPage.GetAttribute("style"), Does.Contain("width:492px"));
		Assert.That(tabPage.GetAttribute("style"), Does.Contain("height:473px"));
		Assert.That(tabPage.GetAttribute("style"), Does.Contain("top:24px"));

		await tabControl.InvokeWinzorDispatcherAsync(() =>
		{
			tabControl.Width = 600;
			tabControl.Height = 600;
		});

		Assert.That(tabControlComponent.GetAttribute("style"), Does.Contain("width:600px"));
		Assert.That(tabControlComponent.GetAttribute("style"), Does.Contain("height:600px"));

		Assert.That(tabPage.GetAttribute("style"), Does.Contain("width:592px"));
		Assert.That(tabPage.GetAttribute("style"), Does.Contain("height:573px"));
		Assert.That(tabPage.GetAttribute("style"), Does.Contain("top:24px"));
	}

	[Test]
	public async Task SelectedIndexChangedNotTriggeredBeforeFormIsRendered()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
			tabControl.TabPages.Add(new TabPage());
			tabControl.TabPages.Add(new TabPage());
			tabControl.SelectedIndex = 1;
			return tabControl;
		});

		void TabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			Assert.That(((TabControl)sender).FindForm(), Is.Not.Null);
		}
	}

	[Test]
	public async Task SetVisibleCoreCalledWhenTabpageAddedToTabControl()
	{
		using var dispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
		await dispatcher.InvokeAsync(() =>
		{
			var tabControl = new TabControl();
			var tabPage = new TabPageWithSetVisibleCoreOverride();
			tabControl.TabPages.Add(tabPage);
			Assert.That(tabPage.SetVisibleCoreCalled, Is.True);
		});
	}

	[Test]
	public async Task TabControlImageUnDraggable()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			tabControl.ImageList = new ImageList();
			tabControl.ImageList.Images.Add(new Bitmap(32, 32));
			var tabPage = new TabPage { Text = "Tab", ImageIndex = 0 };
			tabControl.TabPages.Add(tabPage);
			return tabControl;
		});

		var tabPageImages = rendered.Find(".tabcontrol__tabimage img");
		Assert.That(tabPageImages.GetAttribute("draggable"), Is.EqualTo("false"));
	}

	class TabPageWithSetVisibleCoreOverride : TabPage
	{
		protected override void SetVisibleCore(bool value)
		{
			SetVisibleCoreCalled = true;
			base.SetVisibleCore(value);
		}

		public bool SetVisibleCoreCalled { get; private set; }
	}

	[Test]
	public async Task OnSelecting()
	{
		await ControlAssert.ImplementsEventAsync<TabControl, TabControlCancelEventHandler>(
			nameof(TabControl.Selecting),
			tabControl =>
			{
				tabControl.TabPages.Add(new TabPage { Name = "Foo", Text = "Foo" });
				tabControl.TabPages.Add(new TabPage { Name = "Bar", Text = "Bar" });
				tabControl.SelectedIndex = 0;
			},
			a => new TabControlCancelEventHandler((s, e) => a()),
			"button:contains('Bar')",
			e => e.Click()
		);
	}

	[Test]
	public async Task OnSelected()
	{
		await ControlAssert.ImplementsEventAsync<TabControl, TabControlEventHandler>(
			nameof(TabControl.Selected),
			tabControl =>
			{
				tabControl.TabPages.Add(new TabPage { Name = "Foo", Text = "Foo" });
				tabControl.TabPages.Add(new TabPage { Name = "Bar", Text = "Bar" });
				tabControl.SelectedIndex = 0;
			},
			a => new TabControlEventHandler((s, e) => a()),
			"button:contains('Bar')",
			e => e.Click()
		);
	}

	[Test]
	public async Task DisabledControlFocusedInTabControlContainerIsActiveControl()
	{
		using var ctx = new WinzorTestContext();

		TabControl tabControl = null;
		TabPage tabPage = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl();
			tabPage = new TabPage { Name = "One", Text = "One" };
			tabControl.TabPages.Add(tabPage);
			tabControl.SelectedIndex = 0;
			var textbox = new TextBox();
			textbox.Enabled = false;
			tabPage.Controls.Add(textbox);
			return tabControl;
		});

		await rendered.Find(".textbox").TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());

		Assert.That(tabControl.GetContainerControl().ActiveControl, Is.EqualTo(tabControl));
	}

	[Test]
	public async Task TabPageButtonEscapesMnemonics()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			var tabPage = new TabPage { Name = "Test", Text = "&Test && Another Test" };
			tabControl.TabPages.Add(tabPage);
			return tabControl;
		});

		Assert.That(rendered.Find(".tabcontrol__tabtext").InnerHtml, Does.Contain("Test &amp; Another Test"));
	}

	[Test]
	public async Task TabControlDisplayRectangle()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var tabControl = new TabControl()
			{
				Width = 100,
				Height = 200,
				Top = 5,
				Left = 10
			};

			Assert.That(tabControl.DisplayRectangle.Width, Is.EqualTo(92));
			Assert.That(tabControl.DisplayRectangle.Height, Is.EqualTo(173));
			Assert.That(tabControl.DisplayRectangle.X, Is.EqualTo(4));
			Assert.That(tabControl.DisplayRectangle.Y, Is.EqualTo(24));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task TabControlPagesDoNotOverflow()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var tabControl = new TabControl()
			{
				Width = 300,
				Height = 300,
			};
			var tabPage = new TabPage("Tab With Docked Content");
			tabPage.Controls.Add(new Panel() { Dock = DockStyle.Fill });
			tabControl.TabPages.Add(tabPage);
			form.Controls.Add(tabControl);
			return form;
		});

		var tabControl = await page.WaitForSelectorAsync(".tabcontrol");
		Assert.That(async () => await tabControl.IsOverflowingAsync(), Is.False, "The contents of the TabControl are causing it to overflow");
		var tabPage = await page.WaitForSelectorAsync(".tabcontrol__page");
		Assert.That(async () => await tabPage.IsOverflowingAsync(), Is.False, "The contents of the TabPage are causing it to overflow");
	}

	[Test, WithPlaywrightPage]
	public async Task TabControlPagesShouldHaveTextRendered()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			tabControl.TabPages.Add(new TabPage("Bar"));
			tabControl.TabPages.Add(new TabPage("Car"));
			return tabControl;
		});

		var tabTexts = rendered.FindAll(".tabcontrol__navigation .tabcontrol__tabtext");

		Assert.That(tabTexts.Count, Is.EqualTo(2));
		Assert.That(tabTexts[0].TextContent, Is.EqualTo("Bar"));
		Assert.That(tabTexts[1].TextContent, Is.EqualTo("Car"));
	}

	[Test, WithPlaywrightPage]
	public async Task TabControlPagesTextWhiteSpaceShouldBePre()
	{
		await using var appServerCtx = new InMemoryTestServerContext();
		Form form = null;
		var page = await appServerCtx.LoadFormAsync(() =>
		{
			form = new Form();
			var tabControl = new TabControl();
			var tabPage = new TabPage("Bar");
			tabControl.TabPages.Add(tabPage);
			form.Controls.Add(tabControl);
			return form;
		});

		var tabControlText = await page.WaitForSelectorAsync(".tabcontrol__navigation .tabcontrol__tabtext");

		var navigationTopPadding = await getComputedStyle(tabControlText, "white-space");
		Assert.That(navigationTopPadding, Is.EqualTo("pre"));
	}

	[Test]
	public async Task TabControlSelectedIndexIsCorrectAfterRemoveTabBeforeSelected()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var tabControl = new TabControl();
			var tabPage1 = new TabPage();
			var tabPage2 = new TabPage();
			tabControl.Controls.AddRange(new TabPage[] { tabPage1, tabPage2 });
			tabControl.SelectedIndex = 1;
			Assert.That(tabControl.TabCount, Is.EqualTo(2));
			Assert.That(tabControl.SelectedIndex, Is.EqualTo(1));
			tabControl.Controls.Remove(tabPage1);
			Assert.That(tabControl.TabCount, Is.EqualTo(1));
			Assert.That(tabControl.SelectedIndex, Is.EqualTo(0));
		});
	}

	[Test]
	public async Task TabControlSelectedIndexIsCorrectAfterRemoveTabAfterSelected()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var tabControl = new TabControl();
			var tabPage1 = new TabPage();
			var tabPage2 = new TabPage();
			tabControl.Controls.AddRange(new TabPage[] { tabPage1, tabPage2 });
			tabControl.SelectedIndex = 0;
			Assert.That(tabControl.TabCount, Is.EqualTo(2));
			Assert.That(tabControl.SelectedIndex, Is.EqualTo(0));
			tabControl.Controls.Remove(tabPage2);
			Assert.That(tabControl.TabCount, Is.EqualTo(1));
			Assert.That(tabControl.SelectedIndex, Is.EqualTo(0));
		});
	}

	[TestCase(2, 0, TestName = "{m}_TabsFitInForm")]
	[TestCase(20, 1, TestName = "{m}_TabsDoNotFitInForm")]
	public async Task TabControlHasTabSliders(int numberOfTabs, int shouldHaveSlider)
	{
		using var ctx = new WinzorTestContext();
		TabControl tabControl = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl() { Width = 500, Height = 500 };
			for (int i = 0; i < numberOfTabs; i++)
			{
				var tabPage = new TabPage();
				tabPage.Text = "Text";
				tabControl.TabPages.Add(tabPage);
			}
			tabControl.SelectedIndex = 0;
			return tabControl;
		});

		Assert.That(rendered.FindAll(".tabcontrol__slider").Count, Is.EqualTo(shouldHaveSlider));
	}

	[TestCase(500, 1000, 1, 0, TestName = "{m}_HiddenSliderOnResize")]
	[TestCase(1000, 500, 0, 1, TestName = "{m}_ShowSliderOnResize")]
	public async Task TabControlDoesNotHaveTabSlidersOnResize(int originalSize, int changedSize, int originalCount, int changedCount)
	{
		using var ctx = new WinzorTestContext();
		TabControl tabControl = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl() { Width = originalSize };
			for (int i = 0; i < 20; i++)
			{
				var tabPage = new TabPage();
				tabPage.Text = "Text";
				tabControl.TabPages.Add(tabPage);
			}
			tabControl.SelectedIndex = 0;
			return tabControl;
		});

		Assert.That(rendered.FindAll(".tabcontrol__slider").Count, Is.EqualTo(originalCount));
		await tabControl.InvokeWinzorDispatcherAsync(() =>
		{
			tabControl.Width = changedSize;
		});
		Assert.That(rendered.FindAll(".tabcontrol__slider").Count, Is.EqualTo(changedCount));
	}

	[Test, WithPlaywrightPage]
	public async Task LeaveNotRaisedOnTabPageWhenNoNewActiveControl()
	{
		await using var ctx = new InMemoryTestServerContext();
		var tabPageLeave = false;
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var tabControl = new TabControl { Dock = DockStyle.Fill };
			var tabPage = new TabPage();
			tabPage.Text = "Foo";
			tabPage.Leave += (s, e) => tabPageLeave = true;
			tabControl.TabPages.Add(tabPage);
			var textBox = new TextBox();
			tabPage.Controls.Add(textBox);
			form.Controls.Add(tabControl);
			return form;
		});
		var textBoxElement = await page.WaitForSelectorAsync("div input");
		await textBoxElement.ClickAsync();
		Assert.That(() => form.ActiveControl, Is.InstanceOf<TextBox>().After(3000, 100));
		await page.Mouse.ClickAsync(80, 80);
		Assert.That(() => form.ActiveControl, Is.InstanceOf<TabPage>().After(3000, 100));
		Assert.That(tabPageLeave, Is.False);
	}

	[Test]
	public async Task TestLastVisibleTabRenderedPartiallyIfGoingOutOfBounds()
	{
		using var ctx = new WinzorTestContext();
		TabControl tabControl = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl() { Width = 500, Height = 500 };
			for (var i = 0; i < 20; i++)
			{
				var tabPage = new TabPage();
				tabPage.Text = "Text";
				tabControl.TabPages.Add(tabPage);
			}
			tabControl.SelectedIndex = 0;
			return tabControl;
		});

		var visibleTabs = tabControl.GetVisibleTabs();
		Assert.That(visibleTabs.Count, Is.EqualTo(14));
		var totalVisibleTabsWidth = visibleTabs.Sum(x => tabControl.GetTabRect(x.TabIndex).Width);
		Assert.That(totalVisibleTabsWidth + TabSlider.SLIDER_WIDTH, Is.GreaterThan(500));
		Assert.That(totalVisibleTabsWidth - tabControl.GetTabRect(visibleTabs.Last().TabIndex).Width + TabSlider.SLIDER_WIDTH, Is.LessThanOrEqualTo(500));
	}

	[Test]
	public async Task TestTabPageHasCorrectPosition()
	{
		using var ctx = new WinzorTestContext();
		TabControl tabControl = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl() { Width = 500, Height = 500 };
			for (var i = 0; i < 20; i++)
			{
				var tabPage = new TabPage();
				tabPage.Text = $"TabPage{i}";
				tabControl.TabPages.Add(tabPage);
			}
			tabControl.SelectedIndex = 0;
			return tabControl;
		});

		var visibleTabs = tabControl.GetVisibleTabs();
		Assert.That(visibleTabs.Count, Is.EqualTo(8));

		for (var i = 0; i < visibleTabs.Count; i++)
		{
			var left = tabControl.GetTabRect(i).X;
			Assert.That(left, Is.EqualTo(61 * i));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task TabControlTabsShouldNotOverflow()
	{
		await using var appServerCtx = new InMemoryTestServerContext();
		Form form = null;
		var page = await appServerCtx.LoadFormAsync(() =>
		{
			form = new Form();
			var tabControl = new TabControl();
			for (int i = 0; i < 20; i++)
			{
				var tabPage = new TabPage();
				tabPage.Text = "Text";
				tabControl.TabPages.Add(tabPage);
			}
			form.Controls.Add(tabControl);
			return form;
		});

		var tabControlTabs = await page.WaitForSelectorAsync(".tabcontrol__navigation > .tabcontrol__tabs");

		var tabControlTabsOverflow = await getComputedStyle(tabControlTabs, "overflow");
		Assert.That(tabControlTabsOverflow, Is.EqualTo("hidden"));
	}

	[Test, WithPlaywrightPage]
	public async Task ButtonOutlineShouldDisplayWhenTabbed()
	{
		KeyboardPressOptions keyboardPressOptions = new KeyboardPressOptions { Delay = 300 };
		TextBox textBoxOutSide = null;
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		TabControl tabControl = null;
		TabPageForTest tabPage = null;
		TabPageForTest tabPage2 = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			textBoxOutSide = new TextBox() { Dock = DockStyle.Top };
			form.Controls.Add(textBoxOutSide);
			tabControl = new TabControl { Dock = DockStyle.Bottom };
			tabPage = new TabPageForTest();
			tabPage.Text = "Tab 1";
			tabPage2 = new TabPageForTest();
			tabPage2.Text = "Tab 2";
			tabControl.TabPages.Add(tabPage);
			tabControl.TabPages.Add(tabPage2);
			var textBox = new TextBox();
			tabPage.Controls.Add(textBox);
			form.Controls.Add(tabControl);

			return form;
		});
		var textBoxElement = await page.WaitForSelectorAsync("input");
		await textBoxElement.ClickAsync();

		await page.Keyboard.PressAsync("Tab", keyboardPressOptions);
		var tab1 = await page.Locator(".tabcontrol__button:focus-visible").ElementHandleAsync();
		Assert.That(await getComputedStyle(tab1, "outline"), Does.Contain("dotted 1px"));
		Assert.That(await getComputedStyle(tab1, "outline-offset"), Is.EqualTo("-4px"));

		await page.Keyboard.PressAsync("ArrowRight", keyboardPressOptions);
		var tab2 = await page.Locator(".tabcontrol__button:focus-visible").ElementHandleAsync();
		Assert.That(await getComputedStyle(tab2, "outline"), Does.Contain("dotted 1px"));
		Assert.That(await getComputedStyle(tab2, "outline-offset"), Is.EqualTo("-4px"));

		var tab1NotFocused = await page.Locator(".tabcontrol__button").Nth(0).ElementHandleAsync();
		Assert.That(await getComputedStyle(tab1NotFocused, "outline"), Does.Not.Contain("dotted 1px"));
	}

	[Test, WithPlaywrightPage]
	public async Task SettingItemSizeUpdatesTabControlNavigationHeight()
	{
		TabControl tabControl = null;

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			tabControl = new TabControl() { ItemSize = new Size(15, 15) };
			form.Controls.Add(tabControl);
			return form;
		});

		foreach (var height in new int[] { 20, 30, 40 })
		{
			var oldHeight = await (await page.WaitForSelectorAsync(".tabcontrol__navigation")).EvaluateAsync<float>("e => e.getBoundingClientRect().height");
			tabControl.Invoke(() => tabControl.ItemSize = new Size(15, height));
			Assert.That(async () => await (await page.WaitForSelectorAsync(".tabcontrol__navigation")).EvaluateAsync<float>("e => e.getBoundingClientRect().height"), Is.GreaterThan(oldHeight).After(2000, 100));
		}
	}

	[TestCase(TabSizeMode.Normal)]
	[TestCase(TabSizeMode.Fixed)]
	public async Task SettingItemSizeAffectsSizeReturned(TabSizeMode mode)
	{
		TabControl tabControl = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl() { SizeMode = mode };
			tabControl.TabPages.Add("Tab Tab Tab Tab");
			tabControl.TabPages.Add("1");
			tabControl.TabPages.Add("");
			tabControl.ItemSize = new Size(999, 999);
			return tabControl;
		});

		for (var i = 0; i <= 2; i++)
		{
			Assert.That(tabControl.GetTabRect(i).Width == 999, Is.EqualTo(tabControl.SizeMode == TabSizeMode.Fixed), $"Invalid width for tab {i}");
			Assert.That(tabControl.GetTabRect(i).Height, Is.EqualTo(999), $"Invalid height for tab {i}");
		}
	}

	[Test]
	public async Task GetTabRectWidthIncreasesAsTextWidthIncreases()
	{
		TabControl tabControl = null;

		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var form = new Form() { Width = 500 };
			tabControl = new TabControl() { SizeMode = TabSizeMode.Normal, Width = 500 };
			form.Controls.Add(tabControl);
			tabControl.TabPages.Add(new TabPage());

			var oldWidth = tabControl.GetTabRect(0).Width;

			for (var i = 0; i < 5; i++)
			{
				tabControl.TabPages[0].Text += "a";
				var newWidth = tabControl.GetTabRect(0).Width;
				Assert.That(newWidth, Is.GreaterThan(oldWidth));
				oldWidth = newWidth;
			}
		});
	}

	[Test]
	public async Task TabControlItemSizeMatchesFirstTab()
	{
		TabControl tabControl = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl() { SizeMode = TabSizeMode.Normal };
			tabControl.TabPages.Add(new TabPage() { Text = "a very long tab name" });
			tabControl.TabPages.Add(new TabPage() { Text = "short" });
			return tabControl;
		});

		Assert.That(tabControl.ItemSize, Is.EqualTo(tabControl.GetTabRect(0).Size));
	}

	[Test]
	public async Task TabControlTabWidthCalculatedProperlyInNormalMode()
	{
		TabControl tabControl = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl() { SizeMode = TabSizeMode.Normal };
			tabControl.TabPages.Add(new TabPage() { Text = "Tab Text 123456" });
			tabControl.ItemSize = new Size(1, 1);
			return tabControl;
		});

		//Margin of error to allow minor TextRenderer differences. 
		Assert.That(tabControl.GetTabRect(0).Width, Is.EqualTo(111).Within(20), $"TabControl width was {tabControl.GetTabRect(0).Width}, expected 141");
	}

	[Test, WithPlaywrightPage]
	public async Task TabControlTabButtonAppliesCorrectCustomStyle()
	{
		await using var appServerCtx = new InMemoryTestServerContext();
		Form form = null;
		var page = await appServerCtx.LoadFormAsync(() =>
		{
			form = new Form();
			var tabControl = new TabControlForTest();
			var tabPage = new TabPage();
			tabControl.TabControlButtonAdditionalStyles = "height: 30px; background-color: black;";
			tabControl.TabPages.Add(tabPage);
			tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
			form.Controls.Add(tabControl);
			return form;
		});

		var tabButton = await page.WaitForSelectorAsync(".tabcontrol__tabs button");
		Assert.That(await getComputedStyle(tabButton, "height"), Is.EqualTo("30px"));
		Assert.That(await getComputedStyle(tabButton, "background-color"), Is.EqualTo("rgb(0, 0, 0)"));
	}

	[Test, WithPlaywrightPage]
	public async Task TabControlTabButtonAppliesCorrectItemSize()
	{
		await using var appServerCtx = new InMemoryTestServerContext();
		Form form = null;
		var page = await appServerCtx.LoadFormAsync(() =>
		{
			form = new Form();
			var tabControl = new TabControlForTest();
			var tabPage = new TabPage();
			tabControl.TabControlButtonAdditionalStyles = "height: 30px; background-color: black;";
			tabControl.TabPages.Add(tabPage);
			tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
			tabControl.SizeMode = TabSizeMode.Fixed;
			tabControl.Width = 300;
			var size = new Size(200, 25);
			tabControl.ItemSize = size;
			form.Controls.Add(tabControl);
			return form;
		});

		var tabButton = await page.WaitForSelectorAsync(".tabcontrol__button");

		Assert.That(await getComputedStyle(tabButton, "width"), Is.EqualTo("200px"));
	}

	[Test, WithPlaywrightPage]
	public async Task TabControlTabButtonShouldHaveMinWidth()
	{
		await using var appServerCtx = new InMemoryTestServerContext();
		Form form = null;
		var page = await appServerCtx.LoadFormAsync(() =>
		{
			form = new Form();
			var tabControl = new TabControlForTest();
			var tabPage = new TabPage();
			tabControl.TabControlButtonAdditionalStyles = "height: 30px; background-color: black;";
			tabControl.TabPages.Add(tabPage);
			tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
			tabControl.SizeMode = TabSizeMode.Fixed;
			tabControl.Width = 300;
			var size = new Size(200, 25);
			tabControl.ItemSize = size;
			form.Controls.Add(tabControl);
			return form;
		});
		var tabButton = await page.WaitForSelectorAsync(".tabcontrol__button");

		var tabButtonWidth = await getComputedStyle(tabButton, "width");
		var tabButtonMinWidth = await getComputedStyle(tabButton, "min-width");
		Assert.That(tabButtonMinWidth, Is.EqualTo(tabButtonWidth));
	}

	[Test, WithPlaywrightPage]
	public async Task TabControlTabNavigationAppliesCorrectCustomStyle()
	{
		await using var appServerCtx = new InMemoryTestServerContext();
		Form form = null;
		var page = await appServerCtx.LoadFormAsync(() =>
		{
			form = new Form();
			var tabControl = new TabControlForTest();
			var tabPage = new TabPage();
			tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
			tabControl.TabControlNavigationAdditionalStyles = "width: 100px; height: 100px; display: flex; flex-direction: column;";
			tabControl.TabPages.Add(tabPage);
			form.Controls.Add(tabControl);
			return form;
		});

		var tabNavigator = await page.WaitForSelectorAsync(".tabcontrol__tabs");
		Assert.That(await getComputedStyle(tabNavigator, "width"), Is.EqualTo("100px"));
		Assert.That(await getComputedStyle(tabNavigator, "height"), Is.EqualTo("100px"));
		Assert.That(await getComputedStyle(tabNavigator, "display"), Is.EqualTo("flex"));
		Assert.That(await getComputedStyle(tabNavigator, "flex-direction"), Is.EqualTo("column"));
	}

	[Test, WithPlaywrightPage]
	public async Task TabControlTabPageAppliesCorrectCustomStyle()
	{
		await using var appServerCtx = new InMemoryTestServerContext();
		Form form = null;
		var page = await appServerCtx.LoadFormAsync(() =>
		{
			form = new Form();
			var tabControl = new TabControlForTest();
			var tabPage = new TabPage();
			tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
			tabControl.TabPageAdditionalStyles = "left: 100px; top: 25px; width: 1000px;";
			tabControl.TabPages.Add(tabPage);
			form.Controls.Add(tabControl);
			return form;
		});

		var tabPage = await page.WaitForSelectorAsync(".tabcontrol__page");
		Assert.That(await getComputedStyle(tabPage, "left"), Is.EqualTo("100px"));
		Assert.That(await getComputedStyle(tabPage, "top"), Is.EqualTo("25px"));
		Assert.That(await getComputedStyle(tabPage, "width"), Is.EqualTo("1000px"));
	}

	[Test, WithPlaywrightPage]
	public async Task TabControlTabsAlignmentAppliesCorrectClassAndStyling()
	{
		await using var appServerCtx = new InMemoryTestServerContext();
		Form form = null;
		var page = await appServerCtx.LoadFormAsync(() =>
		{
			form = new Form();
			var tabControl = new TabControlForTest();
			var tabPage = new TabPage();
			var tabPage2 = new TabPage();
			tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
			tabControl.TabControlAlignmentStyle = "vertical";
			tabControl.TabPages.Add(tabPage);
			tabControl.TabPages.Add(tabPage2);
			form.Controls.Add(tabControl);
			return form;
		});

		var tabNavigator = await page.WaitForSelectorAsync(".tabcontrol__tabs--vertical");

		Assert.That(await getComputedStyle(tabNavigator, "flex-direction"), Is.EqualTo("column"));
	}

	[Test]
	public async Task TabControlWidthsAreDifferentInNormalSizeMode()
	{
		TabControl tabControl = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl() { SizeMode = TabSizeMode.Normal };
			tabControl.TabPages.Add(new TabPage() { Text = "a very long tab name" });
			tabControl.TabPages.Add(new TabPage() { Text = "shorter name" });
			tabControl.TabPages.Add(new TabPage() { Text = "" });
			return tabControl;
		});

		Assert.That(tabControl.GetTabRect(0).Width, Is.GreaterThan(tabControl.GetTabRect(1).Width));
		Assert.That(tabControl.GetTabRect(1).Width, Is.GreaterThan(tabControl.GetTabRect(2).Width));
	}

	[Test]
	public async Task TabWidthIncludesIconWidth()
	{
		TabControl tabControl = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl() { SizeMode = TabSizeMode.Normal };
			tabControl.ImageList = new ImageList();
			tabControl.ImageList.Images.Add(new Bitmap(32, 32));

			var tabPage1 = new TabPage();
			var tabPage2 = new TabPage();
			tabControl.TabPages.Add(tabPage1);
			tabControl.TabPages.Add(tabPage2);

			tabPage1.Text = "Tab";
			tabPage2.Text = "Tab";
			tabPage1.ImageIndex = 0;

			return tabControl;
		});

		var width1 = tabControl.GetTabRect(0).Width;
		var width2 = tabControl.GetTabRect(1).Width;

		Assert.That(width1, Is.EqualTo(width2 + 36)); //36 = 32px icon + 4px gap
	}

	[Test]
	public async Task TabControlSelectedIndexIsCorrectWhenSetBeforeHandleCreation()
	{
		TabControl tabControl = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl();
			tabControl.TabPages.Add("0");
			tabControl.TabPages.Add("1");
			Assert.That(tabControl.IsHandleCreated, Is.EqualTo(false));
			tabControl.SelectedIndex = 1;
			return tabControl;
		});

		Assert.That(() => tabControl.IsHandleCreated, Is.EqualTo(true).After(2000, 100));
		Assert.That(tabControl.SelectedIndex, Is.EqualTo(1));
	}

	[Test]
	public async Task TabControlSelectedIndexReturnsValidValueIfHandledValueIsInvalid()
	{
		TabControl tabControl = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl();
			tabControl.TabPages.Add("0");
			tabControl.TabPages.Add("1");
			Assert.That(tabControl.IsHandleCreated, Is.EqualTo(false));
			tabControl.SelectedIndex = 999;
			return tabControl;
		});

		Assert.That(() => tabControl.IsHandleCreated, Is.EqualTo(true).After(2000, 100));
		Assert.That(tabControl.SelectedIndex, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task TabControlSelectedIndexBeforeHandleCreationRendersCorrectTab()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();

			var tabControl = new TabControl();
			for (var i = 0; i < 5; i++)
			{
				var tabPage = new TabPage();
				tabPage.Text = i.ToString();
				tabPage.Controls.Add(new Label() { Text = $"Tab page {i} Is Visible!" });
				tabControl.TabPages.Add(tabPage);
			}
			form.Controls.Add(tabControl);
			tabControl.SelectedIndex = 2;

			return form;
		});

		Assert.That(async () => await page.EvaluateAsync<string>("let label = document.getElementsByClassName(\"label__text\")[0]; label != null ? label.textContent : ''"), Is.EqualTo($"Tab page 2 Is Visible!").After(3000, 100));
		Assert.That(await page.EvaluateAsync<int>("document.getElementsByClassName(\"label__text\").length"), Is.EqualTo(1));
	}

	[Test, WithPlaywrightPage]
	public async Task TabControlSettingSelectedIndexChangeRerendersCorrectTab()
	{
		TabControl tabControl = null;

		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();

			tabControl = new TabControl();
			for (var i = 0; i < 5; i++)
			{
				var tabPage = new TabPage();
				tabPage.Text = i.ToString();
				tabPage.Controls.Add(new Label() { Text = $"Tab page {i} Is Visible!" });
				tabControl.TabPages.Add(tabPage);
			}
			form.Controls.Add(tabControl);

			return form;
		});

		foreach (var i in new int[] { 1, 4, 0 })
		{
			tabControl.Invoke(() => tabControl.SelectedIndex = i);
			Assert.That(tabControl.SelectedIndex, Is.EqualTo(i));
			Assert.That(async () => await page.EvaluateAsync<string>("let label = document.getElementsByClassName(\"label__text\")[0]; label != null ? label.textContent : ''"), Is.EqualTo($"Tab page {i} Is Visible!").After(3000, 100));
			Assert.That(await page.EvaluateAsync<int>("document.getElementsByClassName(\"label__text\").length"), Is.EqualTo(1));
		}
	}

	[TestCase(Keys.Left, true)]
	[TestCase(Keys.Right, true)]
	[TestCase(Keys.Up, true)]
	[TestCase(Keys.Down, true)]
	[TestCase(Keys.A, false)]
	public async Task TabControlReturnsCorrectValueForIsInputKey(Keys key, bool expectedValue)
	{
		TabControlForTest tabControl = null;
		using var ctx = new WinzorTestContext();
		await ctx.RenderControlOnFormAsync(() => tabControl = new TabControlForTest());
		Assert.That(tabControl.IsInputKey(key), Is.EqualTo(expectedValue));
	}

	[Test]
	public async Task TabControlSelectedTabNotChangeWhenSelectingEventCancelled()
	{
		TabControl tabControl = null;
		TabPage tabPage1 = null;
		TabPage tabPage2 = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			void TabControl_Selecting(object sender, TabControlCancelEventArgs e)
			{
				e.Cancel = true;
			}
			tabControl = new TabControl();
			tabPage1 = new TabPage("tabPage1");
			tabPage2 = new TabPage("tabPage2");
			tabControl.Selecting += TabControl_Selecting;
			tabControl.TabPages.Add(tabPage1);
			tabControl.TabPages.Add(tabPage2);

			Assert.That(tabControl.IsHandleCreated, Is.EqualTo(false));
			Assert.That(tabControl.SelectedIndex, Is.EqualTo(-1));

			return tabControl;
		});

		Assert.That(tabControl.SelectedIndex, Is.EqualTo(0));
		await tabControl.InvokeWinzorDispatcherAsync(() => tabControl.SelectedTab = tabPage2);
		Assert.That(tabControl.SelectedIndex, Is.EqualTo(0));
		Assert.That(tabControl.SelectedTab, Is.EqualTo(tabPage1));
	}

	[Test]
	public async Task TabPageRowCountIsOne()
	{
		using var ctx = new WinzorTestContext();

		TabControl tabControl = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl();
			var tabPage = new TabPage();
			tabControl.TabPages.Add(tabPage);
			return tabControl;
		});
		//current we did not implement Multiline property
		//our tab controls always single row.
		Assert.That(tabControl.RowCount, Is.EqualTo(1));
		Assert.That(tabControl.Multiline, Is.EqualTo(false));
	}

	[Test]
	public async Task TabControlImageDisplayedIfIconIndexSet()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			tabControl.ImageList = new ImageList();
			tabControl.ImageList.Images.Add(new Bitmap(32, 32));
			var tabPage = new TabPage { Text = "Tab", IconIndex = 0 };
			tabControl.TabPages.Add(tabPage);
			return tabControl;
		});

		var tabPageImages = rendered.FindAll(".tabcontrol__tabicon img");
		Assert.That(tabPageImages.Count, Is.EqualTo(1));
	}

	[Test]
	public async Task TabPageRenderBackGroundColorWhenCaptionBackGroundColorSetAsync()
	{
		using var ctx = new WinzorTestContext();
		TabPage tabPage = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			tabPage = new TabPage();
			tabControl.TabPages.Add(tabPage);
			tabPage.CaptionBackgroundColor = Color.FromArgb(176, 216, 255);
			return tabControl;
		});

		Assert.That(rendered.Find(".tabcontrol__button").GetAttribute("style"), Does.Contain("background-color: rgb(176, 216, 255)"));
	}

	#region Helper

	async Task<string> getComputedStyle(IElementHandle e, string property)
	{
		return (await e.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('{property}')")).Value.ToString();
	}

	async Task<string> getComputedStyle(ILocator e, string property)
	{
		return (await e.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('{property}')")).Value.ToString();
	}

	class TabPageForTest : TabPage
	{
		public bool IsOnLeaveFired;
		public bool IsOnEnterFired;
		public int OnLeaveCount;

		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);
			IsOnLeaveFired = true;
			OnLeaveCount++;
		}

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			IsOnEnterFired = true;
		}
	}

	class TabControlForTest : TabControl
	{
		public string TabControlAlignmentStyle { get; set; }

		public string TabControlNavigationAdditionalStyles { get; set; }

		public string TabControlButtonAdditionalStyles { get; set; }

		public string TabPageAdditionalStyles { get; set; }

		internal new bool IsInputKey(Keys keyData) => base.IsInputKey(keyData);

		protected override string TabControlAlignment
		{
			get
			{
				return DrawMode is TabDrawMode.OwnerDrawFixed ? TabControlAlignmentStyle : "";
			}
		}

		protected override string TabControlNavigationStyleString
		{
			get
			{
				return DrawMode is TabDrawMode.OwnerDrawFixed ? TabControlNavigationAdditionalStyles : "";
			}
		}

		protected override string TabControlButtonStyleString
		{
			get
			{
				return DrawMode is TabDrawMode.OwnerDrawFixed ? TabControlButtonAdditionalStyles : "";
			}
		}

		protected override string TabControlTabPageStyleString
		{
			get
			{
				return DrawMode is TabDrawMode.OwnerDrawFixed ? TabPageAdditionalStyles : "";
			}
		}
	}

	#endregion
}
