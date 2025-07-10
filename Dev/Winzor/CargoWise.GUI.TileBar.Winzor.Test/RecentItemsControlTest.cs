using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Bunit;
using CargoWise.Main.Navigation;
using Enterprise.Winzor.Architecture.Test;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WTG.PlaywrightTesting;
using MenuItem = CargoWise.Main.Navigation.MenuItem;

namespace CargoWise.GUI.TileBar.Winzor.Test;

public class RecentItemsControlTest
{
#pragma warning disable CW1178 // Do Not Invoke Old Res.GetString Methods; test code only
	[Test]
	public async Task RecentItemsControlItemShouldHaveAFavoriteStarAndALinkAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var menuSection = new MenuSection("Recent Items Test", "RecentItemsTest", ResString.GetMultilingualString("RecentItemsTest", "Recent Items Test"));
		menuSection.AddItem(new MenuItem(ResString.GetMultilingualString("RIT1", "Recent Item Test 1")));

		var rendered = await ctx.RenderControlOnFormAsync(() => new RecentItemsControl()
		{
			DataContext = menuSection
		});

		var list = rendered.Find("ul");
		Assert.That(list, Is.Not.Null);
		Assert.That(list.ChildElementCount, Is.EqualTo(1));
		Assert.That(rendered.Find("ul li .button--star-inactive"), Is.Not.Null);
		Assert.That(rendered.Find("ul li .button--link span").InnerHtml, Is.EqualTo("Recent Item Test 1"));
	}

	[Test]
	public async Task RecentItemsControlItemAddToFavoritesAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var onFavoriteActionCalled = new TaskCompletionSource<bool>();
		var menuSection = new MenuSection("Recent Items Test", "RecentItemsTest", ResString.GetMultilingualString("RecentItemsTest", "Recent Items Test"));
		var menuItem = new MenuItem(ResString.GetMultilingualString("RIT1", "Recent Item Test 1"));
		menuItem.IsInFavorites = false;
		menuItem.SetFavoriteAction(() =>
		{
			menuItem.IsInFavorites = true;
			onFavoriteActionCalled.SetResult(true);
		});
		menuSection.AddItem(menuItem);

		var rendered = await ctx.RenderControlOnFormAsync(() => new RecentItemsControl()
		{
			DataContext = menuSection
		});

		Assert.That(menuItem.IsInFavorites, Is.False);
		await rendered.Find("ul li .button--star-inactive").ClickAsync(new WebMouseEventArgs());
		Assert.That(async () => await onFavoriteActionCalled.Task, Is.EqualTo(true).After(100));
		Assert.That(menuItem.IsInFavorites, Is.True);
		rendered.WaitForElement("ul li .button--module-star-active");
	}

	[Test]
	public async Task RecentItemsControlItemRemoveFromFavoritesAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var onFavoriteActionCalled = new TaskCompletionSource<bool>();
		var menuSection = new MenuSection("Recent Items Test", "RecentItemsTest", ResString.GetMultilingualString("RecentItemsTest", "Recent Items Test"));
		var menuItem = new MenuItem(ResString.GetMultilingualString("RIT1", "Recent Item Test 1"));
		menuItem.IsInFavorites = true;
		menuItem.SetFavoriteAction(() =>
		{
			menuItem.IsInFavorites = false;
			onFavoriteActionCalled.SetResult(true);
		});
		menuSection.AddItem(menuItem);

		var rendered = await ctx.RenderControlOnFormAsync(() => new RecentItemsControl()
		{
			DataContext = menuSection
		});

		Assert.That(menuItem.IsInFavorites, Is.True);
		await rendered.Find("ul li .button--module-star-active").ClickAsync(new WebMouseEventArgs());
#pragma warning disable VSTHRD103
		Assert.That(onFavoriteActionCalled.Task.Wait(1000), Is.EqualTo(true));
#pragma warning restore VSTHRD103
		Assert.That(menuItem.IsInFavorites, Is.False);
		rendered.WaitForElement("ul li .button--star-inactive");
	}

	[Test]
	public async Task RecentItemsControlItemRightClickFavoriteStarShouldNotDoAnythingAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var onFavoriteActionCalled = new TaskCompletionSource<bool>();
		var menuSection = new MenuSection("Recent Items Test", "RecentItemsTest", ResString.GetMultilingualString("RecentItemsTest", "Recent Items Test"));
		var menuItem = new MenuItem(ResString.GetMultilingualString("RIT1", "Recent Item Test 1"));
		menuItem.IsInFavorites = true;
		menuItem.SetFavoriteAction(() =>
		{
			menuItem.IsInFavorites = false;
			onFavoriteActionCalled.SetResult(true);
		});
		menuSection.AddItem(menuItem);

		var rendered = await ctx.RenderControlOnFormAsync(() => new RecentItemsControl()
		{
			DataContext = menuSection
		});

		Assert.That(menuItem.IsInFavorites, Is.True);
		await rendered.Find("ul li .button--module-star-active").ClickAsync(new WebMouseEventArgs() { Button = 2 });
#pragma warning disable VSTHRD103
		Assert.That(onFavoriteActionCalled.Task.Wait(1000), Is.EqualTo(false));
#pragma warning restore VSTHRD103
		Assert.That(menuItem.IsInFavorites, Is.True);
		rendered.WaitForElement("ul li .button--module-star-active");
	}

	[Test, WithPlaywrightPage]
	public async Task RecentItemsControlItemTurnsGreyOnHoverAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var recentItemsControl = new RecentItemsControl();
			var menuSection = new MenuSection("Recent Items Test", "RecentItemsTest", ResString.GetMultilingualString("RecentItemsTest", "Recent Items Test"));
			var menuItem1 = new MenuItem(ResString.GetMultilingualString("RIT1", "Recent Item Test 1"));
			menuItem1.IsInFavorites = false;
			menuSection.AddItem(menuItem1);
			recentItemsControl.DataContext = menuSection;
			form.Controls.Add(recentItemsControl);
			return form;
		});
		var button = await page.WaitForSelectorAsync(".button--link");
		await button.HoverAsync();
		Assert.That(async () => await button.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('opacity')"), Is.EqualTo("0.8"));
	}

	[Test]
	public async Task RecentItemsControlLinkLeftClickCalledLinkActionAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var onlinkActionCalled = new TaskCompletionSource<bool>();
		var onlinkRightActionCalled = new TaskCompletionSource<bool>();
		var menuSection = new MenuSection("Recent Items Test", "RecentItemsTest", ResString.GetMultilingualString("RecentItemsTest", "Recent Items Test"));
		menuSection.AddItem(new MenuItem("RIT1", ResString.GetMultilingualString("RIT1", "Recent Item Test 1"),
			() => onlinkActionCalled.SetResult(true), () => onlinkRightActionCalled.SetResult(true), null));

		var rendered = await ctx.RenderControlOnFormAsync(() => new RecentItemsControl()
		{
			DataContext = menuSection
		});

		await rendered.Find("ul li .button--link").ClickAsync(new WebMouseEventArgs());
#pragma warning disable VSTHRD103
		Assert.That(onlinkActionCalled.Task.Wait(1000), Is.EqualTo(true));
#pragma warning restore VSTHRD103
#pragma warning disable VSTHRD103
		Assert.That(onlinkRightActionCalled.Task.Wait(1000), Is.EqualTo(false));
#pragma warning restore VSTHRD103
	}

	[Test]
	public async Task RecentItemsControlLinkRightClickCalledLinkActionAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var onlinkActionCalled = new TaskCompletionSource<bool>();
		var onlinkRightActionCalled = new TaskCompletionSource<bool>();
		var menuSection = new MenuSection("Recent Items Test", "RecentItemsTest", ResString.GetMultilingualString("RecentItemsTest", "Recent Items Test"));
		menuSection.AddItem(new MenuItem("RIT1", ResString.GetMultilingualString("RIT1", "Recent Item Test 1"),
			() => onlinkActionCalled.SetResult(true), () => onlinkRightActionCalled.SetResult(true), null));

		var rendered = await ctx.RenderControlOnFormAsync(() => new RecentItemsControl()
		{
			DataContext = menuSection
		});

		await rendered.Find("ul li .button--link").ClickAsync(new WebMouseEventArgs() { Button = 2 });
#pragma warning disable VSTHRD103
		Assert.That(onlinkRightActionCalled.Task.Wait(1000), Is.EqualTo(true));
		Assert.That(onlinkActionCalled.Task.Wait(1000), Is.EqualTo(false));
#pragma warning restore VSTHRD103
	}

	[Test]
	public async Task RecentItemsControlItemFavoriteStarTitleAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var menuSection = new MenuSection("Recent Items Test", "RecentItemsTest", ResString.GetMultilingualString("RecentItemsTest", "Recent Items Test"));
		var menuItem1 = new MenuItem(ResString.GetMultilingualString("RIT1", "Recent Item Test 1"));
		var menuItem2 = new MenuItem(ResString.GetMultilingualString("RIT2", "Recent Item Test 2"));
		menuItem2.IsInFavorites = true;
		menuSection.AddItem(menuItem1);
		menuSection.AddItem(menuItem2);

		var rendered = await ctx.RenderControlOnFormAsync(() => new RecentItemsControl()
		{
			DataContext = menuSection
		});

		var list = rendered.Find("ul");
		Assert.That(list.ChildElementCount, Is.EqualTo(2));

		var firstListItemFavoriteStarButton = list.Children[0].Children[0];
		Assert.That(firstListItemFavoriteStarButton.ClassList, Contains.Item("button--star-inactive"));
		var firstListItemFavoriteStarButtonTitle = firstListItemFavoriteStarButton.GetAttribute("title");
		Assert.That(firstListItemFavoriteStarButtonTitle, Is.EqualTo(menuItem1.AddToFavoriteTooltip.ToString()));

		var secondListItemFavoriteStarButton = list.Children[1].Children[0];
		Assert.That(secondListItemFavoriteStarButton.ClassList, Contains.Item("button--module-star-active"));
		var secondListItemFavoriteStarButtonTitle = secondListItemFavoriteStarButton.GetAttribute("title");
		Assert.That(secondListItemFavoriteStarButtonTitle, Is.EqualTo(menuItem2.RemoveFromFavoritesTooltip.ToString()));
	}

	[Test]
	public async Task RecentItemsControlItemLinkTitleAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var menuSection = new MenuSection("Recent Items Test", "RecentItemsTest", ResString.GetMultilingualString("RecentItemsTest", "Recent Items Test"));
		menuSection.AddItem(new MenuItem(ResString.GetMultilingualString("RIT1", "Recent Item Test 1")));

		var rendered = await ctx.RenderControlOnFormAsync(() => new RecentItemsControl()
		{
			DataContext = menuSection
		});

		Assert.That(rendered.Find("ul li .button--link[title=\"Recent Item Test 1\"]").InnerHtml, Is.Not.Null);
	}

	[TestCase(".button--star-inactive", 111, 222, TestName = "{m}_FavouriteButton")]
	[TestCase(".button--link", 333, 444, TestName = "{m}_LinkButton")]
	public async Task RecentItemsClickEventUpdatesSavedMousePositionAsync(string cssSelector, int x, int y)
	{
		using var ctx = new EnterpriseTestContext();
		var menuSection = new MenuSection("Recent Items Test", "RecentItemsTest", ResString.GetMultilingualString("RecentItemsTest", "Recent Items Test"));
		menuSection.AddItem(new MenuItem(ResString.GetMultilingualString("RIT1", "Recent Item Test 1")));

		var rendered = await ctx.RenderControlOnFormAsync(() => new RecentItemsControl()
		{
			DataContext = menuSection
		});

		Assert.That(Control.MousePosition.X, Is.Not.EqualTo(x));
		Assert.That(Control.MousePosition.Y, Is.Not.EqualTo(y));
		await rendered.Find(cssSelector).ClickAsync(new WebMouseEventArgs()
		{
			ClientX = x,
			ClientY = y,
		});
		Assert.That(Control.MousePosition.X, Is.EqualTo(x));
		Assert.That(Control.MousePosition.Y, Is.EqualTo(y));
	}

	[Test, WithPlaywrightPage]
	public async Task RecentItemsFavoriteStarButtonStyleAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var recentItemsControl = new RecentItemsControl();
			var menuSection = new MenuSection("Recent Items Test", "RecentItemsTest", ResString.GetMultilingualString("RecentItemsTest", "Recent Items Test"));
			var menuItem1 = new MenuItem(ResString.GetMultilingualString("RIT1", "Recent Item Test 1"));
			var menuItem2 = new MenuItem(ResString.GetMultilingualString("RIT2", "Recent Item Test 2"));
			menuItem1.IsInFavorites = false;
			menuItem2.IsInFavorites = true;
			menuSection.AddItem(menuItem1);
			menuSection.AddItem(menuItem2);
			recentItemsControl.DataContext = menuSection;
			form.Controls.Add(recentItemsControl);
			return form;
		});
		var activeStarButtonImageurl = $"url(\"{ctx.ServerBaseUrl}/_content/CargoWise.GUI.TileBar/images/star-active.png\")";
		var inactiveStarButtonImageurl = $"url(\"{ctx.ServerBaseUrl}/_content/CargoWise.GUI.TileBar/images/star-inactive.png\")";
		var inactiveStarbutton = await page.WaitForSelectorAsync(".button--star-inactive");
		Assert.That(async () => await inactiveStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('opacity')"), Is.EqualTo("0.25"));
		Assert.That(async () => await inactiveStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgba(0, 0, 0, 0)"));
		Assert.That(async () => await inactiveStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-image')"), Is.EqualTo(inactiveStarButtonImageurl));
		Assert.That(async () => await inactiveStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-size')"), Is.EqualTo("12px"));
		Assert.That(async () => await inactiveStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-repeat')"), Is.EqualTo("no-repeat"));
		Assert.That(async () => await inactiveStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-position')"), Is.EqualTo("50% 50%"));
		Assert.That(async () => await inactiveStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('flex-shrink')"), Is.EqualTo("0"));

		await inactiveStarbutton.HoverAsync();
		Assert.That(async () => await inactiveStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('opacity')"), Is.EqualTo("1"));
		Assert.That(async () => await inactiveStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgba(0, 0, 0, 0)"));
		Assert.That(async () => await inactiveStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-image')"), Is.EqualTo(activeStarButtonImageurl));

		var activeStarbutton = await page.WaitForSelectorAsync(".button--module-star-active");
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('opacity')"), Is.EqualTo("1"));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgba(0, 0, 0, 0)"));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-image')"), Is.EqualTo(activeStarButtonImageurl));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-size')"), Is.EqualTo("12px"));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-repeat')"), Is.EqualTo("no-repeat"));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-position')"), Is.EqualTo("50% 50%"));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('flex-shrink')"), Is.EqualTo("0"));

		await activeStarbutton.HoverAsync();
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('opacity')"), Is.EqualTo("1"));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgba(0, 0, 0, 0)"));
		Assert.That(async () => await activeStarbutton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-image')"), Is.EqualTo(activeStarButtonImageurl));
	}

	[Test]
	public async Task RecentItemsControlIsRenderedWithinElementHostAsync()
	{
		using var ctx = new EnterpriseTestContext();

		Panel panel = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			panel = new Panel { Width = 200, Height = 300, Left = 50, Top = 100 };
			form.Controls.Add(panel);
			var elementHost = new ElementHost();
			var menuSection = new MenuSection("Recent Items Test", "RecentItemsTest", ResString.GetMultilingualString("RecentItemsTest", "Recent Items Test"));
			menuSection.AddItem(new MenuItem(ResString.GetMultilingualString("RIT1", "Recent Item Test 1")));
			elementHost.Child = new RecentItemsControl() { DataContext = menuSection };
			elementHost.Dock = DockStyle.Fill;
			panel.Controls.Add(elementHost);
			return form;
		});

		Assert.That(() => rendered.Find(".recentitemscontrol"), Throws.Nothing);
	}

	[TestCase(true, true, 1, TestName = "{m}_RerendersOnceWhenItemAdded")]
	[TestCase(true, false, 0, TestName = "{m}_DoesNotRerenderWhenNoChanges")]
	[TestCase(false, true, 0, TestName = "{m}_DoesNotRerenderWhenDataContextNull")]
	public async Task RecentItemsControl_ShouldRerenderCorrectlyAsync(
		bool isDataContextSet,
		bool shouldModifyCollection,
		int expectedRenderCalls)
	{
		using var ctx = new EnterpriseTestContext();
		var menuSection = new MenuSection("Recent Items Test", "RecentItemsTest", ResString.GetMultilingualString("RecentItemsTest", "Recent Items Test"));
		RecentItemsControl recentItemsControl = null;

		var page = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			recentItemsControl = new RecentItemsControl();
			recentItemsControl.DataContext = isDataContextSet ? menuSection : null;
			form.Controls.Add(recentItemsControl);
			return form;
		});

		var mockDispatcherContext = Mock.Of<IWinzorDispatcherContext>();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using var context = ctx.WinzorDispatcher.WithContext(mockDispatcherContext);
			MenuItem menuItem;
			if (shouldModifyCollection)
			{
				menuItem = new MenuItem(ResString.GetMultilingualString("RIT", "Recent Item Test"));
				menuSection.AddItem(menuItem);
			}
		});

		Mock.Get(mockDispatcherContext).Verify(
			dispatcherContext => dispatcherContext.NotifyRenderRequired(recentItemsControl),
			Times.Exactly(expectedRenderCalls));
	}

	[Test]
	public async Task RecentItemsControl_ShouldNotResubscribeToNotifyRenderWhenSameObjectReAssignedToDataContextAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var menuSection = new MenuSection("Recent Items Test", "RecentItemsTest", ResString.GetMultilingualString("RecentItemsTest", "Recent Items Test"));
		RecentItemsControl recentItemsControl = null;

		var page = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			recentItemsControl = new RecentItemsControl();
			recentItemsControl.DataContext = menuSection;
			form.Controls.Add(recentItemsControl);
			return form;
		});
		Assert.That(recentItemsControl, Is.Not.Null);

		var mockDispatcherContext = Mock.Of<IWinzorDispatcherContext>();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using var context = ctx.WinzorDispatcher.WithContext(mockDispatcherContext);
			var menuItem = new MenuItem(ResString.GetMultilingualString("RIT", "Recent Item Test"));
			menuSection.AddItem(menuItem);
			recentItemsControl.DataContext = menuSection;
			var menuItem2 = new MenuItem(ResString.GetMultilingualString("RIT", "Recent Item Test 2"));
			menuSection.AddItem(menuItem2);
		});

		Mock.Get(mockDispatcherContext).Verify(
			dispatcherContext => dispatcherContext.NotifyRenderRequired(recentItemsControl),
			Times.Exactly(2));
	}
#pragma warning restore CW1178 // Do Not Invoke Old Res.GetString Methods
}
