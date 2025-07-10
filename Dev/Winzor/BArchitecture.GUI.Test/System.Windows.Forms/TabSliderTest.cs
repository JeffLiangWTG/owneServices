using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

[SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
class TabSliderTest
{
	[Test]
	public async Task TabSliderRightArrowMovesTabsRight()
	{
		using var ctx = new WinzorTestContext();
		TabControl tabControl = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl() { Width = 500, Height = 500 };
			for (int i = 0; i < 20; i++)
			{
				var tabPage = new TabPage();
				tabPage.Text = "Text";
				tabControl.TabPages.Add(tabPage);
			}
			tabControl.SelectedIndex = 0;
			return tabControl;
		});

		var tabControlSliderRightArrow = rendered.Find(".tabcontrol__slider__arrow--right");

		Assert.That(tabControlSliderRightArrow, Is.Not.Null);
		var visibleTabsCount = tabControl.GetVisibleTabs().Count;
		Assert.That(tabControl.GetVisibleTabs(), Does.Not.Contain(tabControl.TabPages[visibleTabsCount]));
		tabControlSliderRightArrow.Click();
		Assert.That(tabControl.GetVisibleTabs(), Does.Not.Contain(tabControl.TabPages[0]));
		Assert.That(tabControl.GetVisibleTabs(), Does.Contain(tabControl.TabPages[visibleTabsCount]));
	}

	[Test]
	public async Task TabSliderRightArrowDoesNotGoOutsideOfBounds()
	{
		using var ctx = new WinzorTestContext();
		TabControl tabControl = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl() { Width = 500, Height = 500 };
			for (int i = 0; i < 20; i++)
			{
				var tabPage = new TabPage();
				tabPage.Text = "Text";
				tabControl.TabPages.Add(tabPage);
			}
			tabControl.SelectedIndex = 0;
			return tabControl;
		});

		var tabControlSliderRightArrow = rendered.Find(".tabcontrol__slider__arrow--right");

		Assert.That(tabControlSliderRightArrow, Is.Not.Null);
		for (int i = 0; i < 100; i++)
		{
			tabControlSliderRightArrow.Click();
		}
		Assert.That(tabControl.GetVisibleTabs(), Does.Not.Contain(tabControl.TabPages[0]));
		Assert.That(tabControl.GetVisibleTabs(), Does.Contain(tabControl.TabPages[tabControl.TabCount - 1]));
	}

	[Test]
	public async Task TabSliderLeftArrowMovesTabsLeft()
	{
		using var ctx = new WinzorTestContext();
		TabControl tabControl = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl() { Width = 500, Height = 500 };
			for (int i = 0; i < 20; i++)
			{
				var tabPage = new TabPage();
				tabPage.Text = "Text";
				tabControl.TabPages.Add(tabPage);
			}
			tabControl.SelectedIndex = 0;
			return tabControl;
		});

		var tabControlSliderRightArrow = rendered.Find(".tabcontrol__slider__arrow--right");
		var tabControlSliderLeftArrow = rendered.Find(".tabcontrol__slider__arrow--left");

		Assert.That(tabControlSliderRightArrow, Is.Not.Null);
		Assert.That(tabControlSliderLeftArrow, Is.Not.Null);
		tabControlSliderRightArrow.Click();
		tabControlSliderLeftArrow.Click();
		Assert.That(tabControl.GetVisibleTabs(), Does.Contain(tabControl.TabPages[0]));
	}

	[Test]
	public async Task TabSliderLeftArrowDoesNotGoOutsideOfBounds()
	{
		using var ctx = new WinzorTestContext();
		TabControl tabControl = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			tabControl = new TabControl() { Width = 500, Height = 500 };
			for (int i = 0; i < 20; i++)
			{
				var tabPage = new TabPage();
				tabPage.Text = "Text";
				tabControl.TabPages.Add(tabPage);
			}
			tabControl.SelectedIndex = 0;
			return tabControl;
		});

		var tabControlSliderLeftArrow = rendered.Find(".tabcontrol__slider__arrow--left");

		Assert.That(tabControlSliderLeftArrow, Is.Not.Null);
		var visibleTabsCount = tabControl.GetVisibleTabs().Count;
		Assert.That(tabControl.GetVisibleTabs(), Does.Contain(tabControl.TabPages[visibleTabsCount - 1]));
		for (int i = 0; i < 100; i++)
		{
			tabControlSliderLeftArrow.Click();
		}
		Assert.That(tabControl.GetVisibleTabs(), Does.Contain(tabControl.TabPages[0]));
		Assert.That(tabControl.GetVisibleTabs(), Does.Contain(tabControl.TabPages[visibleTabsCount - 1]));
	}

	[Test]
	public async Task TabSliderRightArrowRendersRightMostTabFully()
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

		var tabControlSliderRightArrow = rendered.Find(".tabcontrol__slider__arrow--right");

		Assert.That(tabControlSliderRightArrow, Is.Not.Null);
		for (var i = 0; i < 100; i++)
		{
			tabControlSliderRightArrow.Click();
		}
		var visibleTabs = tabControl.GetVisibleTabs();
		Assert.That(visibleTabs.Count, Is.EqualTo(13));
		var totalVisibleTabsWidth = visibleTabs.Sum(x => tabControl.GetTabRect(x.TabIndex).Width);
		Assert.That(totalVisibleTabsWidth + TabSlider.SLIDER_WIDTH, Is.LessThanOrEqualTo(500));
	}

	[Test, WithPlaywrightPage]
	public async Task TabSliderHasCorrectWidth()
	{
		await TestTabSliderAsync(async (page) =>
		{
			var tabControlSlider = await page.WaitForSelectorAsync(".tabcontrol__slider");
			var tabControlSliderWidth = await getComputedStyle(tabControlSlider, "width");

			Assert.That(tabControlSliderWidth, Is.EqualTo("32px"));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task TabSliderLeftButtonHasCorrectWidth()
	{
		await TestTabSliderAsync(async (page) =>
		{
			var tabControlSliderLeft = await page.WaitForSelectorAsync(".tabcontrol__slider > .tabcontrol__slider__arrow--left");
			var tabControlSliderLeftWidth = await getComputedStyle(tabControlSliderLeft, "width");

			Assert.That(tabControlSliderLeftWidth, Is.EqualTo("16px"));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task TabSliderLeftButtonHasCorrectMargin()
	{
		await TestTabSliderAsync(async (page) =>
		{
			var tabControlSliderLeft = await page.WaitForSelectorAsync(".tabcontrol__slider > .tabcontrol__slider__arrow--left");
			var tabControlSliderLeftMarginLeft = await getComputedStyle(tabControlSliderLeft, "margin-left");

			Assert.That(tabControlSliderLeftMarginLeft, Is.EqualTo("0px"));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task TabSliderLeftButtonHasCorrectBorder()
	{
		await TestTabSliderAsync(async (page) =>
		{
			var tabControlSliderLeft = await page.WaitForSelectorAsync(".tabcontrol__slider > .tabcontrol__slider__arrow--left");
			var tabControlSliderLeftBorder = await getComputedStyle(tabControlSliderLeft, "border");

			Assert.That(tabControlSliderLeftBorder, Is.EqualTo("1px solid rgb(172, 172, 172)"));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task TabSliderRightButtonHasCorrectWidth()
	{
		await TestTabSliderAsync(async (page) =>
		{
			var tabControlSliderRight = await page.WaitForSelectorAsync(".tabcontrol__slider > .tabcontrol__slider__arrow--right");
			var tabControlSliderRightWidth = await getComputedStyle(tabControlSliderRight, "width");

			Assert.That(tabControlSliderRightWidth, Is.EqualTo("16px"));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task TabSliderRightButtonHasCorrectMargin()
	{
		await TestTabSliderAsync(async (page) =>
		{
			var tabControlSliderRight = await page.WaitForSelectorAsync(".tabcontrol__slider > .tabcontrol__slider__arrow--right");
			var tabControlSliderRightMarginLeft = await getComputedStyle(tabControlSliderRight, "margin-left");

			Assert.That(tabControlSliderRightMarginLeft, Is.EqualTo("16px"));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task TabSliderRightButtonHasCorrectBorder()
	{
		await TestTabSliderAsync(async (page) =>
		{
			var tabControlSliderRight = await page.WaitForSelectorAsync(".tabcontrol__slider > .tabcontrol__slider__arrow--right");
			var tabControlSliderRightBorder = await getComputedStyle(tabControlSliderRight, "border");

			Assert.That(tabControlSliderRightBorder, Is.EqualTo("1px solid rgb(172, 172, 172)"));
		});
	}

	async Task TestTabSliderAsync(Func<IPage, Task> testTask)
	{
		await using var appServerCtx = new InMemoryTestServerContext();
		var page = await appServerCtx.LoadFormAsync(() =>
		{
			var form = new Form();
			var tabControl = new TabControl() { Width = 500, Height = 500 };
			for (int i = 0; i < 20; i++)
			{
				var tabPage = new TabPage();
				tabPage.Text = "Text";
				tabControl.TabPages.Add(tabPage);
			}
			tabControl.SelectedIndex = 0;
			form.Controls.Add(tabControl);
			return form;
		});

		await testTask(page);
	}

	async Task<string> getComputedStyle(IElementHandle e, string property)
	{
		return (await e.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('{property}')")).Value.ToString();
	}

	[Test, WithPlaywrightPage]
	public async Task TabSliderTabsUseCorrectWidths()
	{
		TabControlForTest tabControl = null;

		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 500 };

			tabControl = new TabControlForTest() { Width = 500, Height = 500 };
			tabControl.TabPages.Add(new TabPage() { Text = "A" });
			tabControl.TabPages.Add(new TabPage() { Text = "Really Long Tab Name" });
			tabControl.TabPages.Add(new TabPage() { Text = "123456" });
			tabControl.TabPages.Add(new TabPage() { Text = "Text" });
			tabControl.TabPages.Add(new TabPage() { Text = "!!!!!!!!" });
			tabControl.TabPages.Add(new TabPage() { Text = "Tab" });
			tabControl.TabPages.Add(new TabPage() { Text = "Tab Name" });
			tabControl.TabPages.Add(new TabPage() { Text = "Test Text!!!" });
			tabControl.TabPages.Add(new TabPage() { Text = "AAAAAAAAAAAAAAA" });
			tabControl.TabPages.Add(new TabPage() { Text = "AAAAAAAAAAAAAAAAAAA" });
			tabControl.SelectedIndex = 0;
			tabControl.SizeMode = TabSizeMode.Normal;
			form.Controls.Add(tabControl);

			return form;
		});

		Assert.That(tabControl.GetTabRect(0), Is.Not.EqualTo(tabControl.GetTabRect(1)));

		var rightArrow = await page.WaitForSelectorAsync(".tabcontrol__slider__arrow--right");
		for (var click = 0; click < 5; click++)
		{
			await TestTabsAsync(click);
			await rightArrow.ClickAsync();
		}

		var leftArrow = await page.WaitForSelectorAsync(".tabcontrol__slider__arrow--left");
		for (var click = 4; click >= 0; click--)
		{
			await TestTabsAsync(click);
			await leftArrow.ClickAsync();
		}

		async Task TestTabsAsync(int click)
		{
			Assert.That(() => tabControl.StartingIndexOfVisibleTabs, Is.EqualTo(click).After(2000, 100));

			var visibleTabs = tabControl.GetVisibleTabs();
			IReadOnlyList<IElementHandle> tabButtons = null;
			Assert.That(async () => (tabButtons = await page.QuerySelectorAllAsync(".tabcontrol__button")).Count, Is.EqualTo(visibleTabs.Count).After(2000, 100));

			for (var tabIndex = 0; tabIndex < visibleTabs.Count - 1; tabIndex++)
			{
				Assert.That(await tabButtons[tabIndex].EvaluateAsync<string>("e => e.style['width']"), Is.EqualTo(tabControl.GetTabRect(click + tabIndex).Width + "px"), $"click = {click}, tabIndex = {tabIndex}");
			}
		}
	}

	class TabControlForTest : TabControl
	{
		public new int StartingIndexOfVisibleTabs => base.StartingIndexOfVisibleTabs;
	}
}
