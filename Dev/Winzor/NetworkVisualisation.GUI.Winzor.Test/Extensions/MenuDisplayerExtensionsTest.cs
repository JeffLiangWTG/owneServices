using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Extensions;
using CargoWise.NetworkVisualisation.Integration;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Moq;
using WinzorFramework.JSInterop;
using WinzorTestFramework;

namespace NetworkVisualisation.GUI.Winzor.Test.Extensions;

class MenuDisplayerExtensionsTest
{
	[Test]
	public async Task MenuDisplayerShowContextMenuCallsInteropAsync()
	{
		var menuDisplayer = new Mock<IMenuDisplayer>();

		var menuItems = new[]
		{
			new NetworkActionMenuItem("Item One", "Item One Tooltip", true, false, Enumerable.Empty<NetworkActionMenuItem>()),
			new NetworkActionMenuItem("Item Two", "Item Two Tooltip", true, true, Enumerable.Empty<NetworkActionMenuItem>()),
			new NetworkActionMenuItem("Item Three", "Item Three Tooltip", false, false, Enumerable.Empty<NetworkActionMenuItem>()),
			new NetworkActionMenuItem("Item Four", "Item Four Tooltip", false, true, Enumerable.Empty<NetworkActionMenuItem>()),
		};

		await menuDisplayer.Object.ShowContextMenuAsync(new WebMouseEventArgs(), menuItems, (_, _) => Task.CompletedTask);

		menuDisplayer.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		var menuInteropModel = menuDisplayer.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel, Is.Not.Null);
		Assert.That(menuItems.Length, Is.EqualTo(menuInteropModel!.MenuItems!.Length));
		Assert.Multiple(() => {
			for (var i = 0; i < menuItems.Length; i++)
			{
				var expected = menuItems[i];
				var actual = menuInteropModel.MenuItems[i];
				Assert.That(actual!.Text, Is.EqualTo(expected.Name));
				Assert.That(actual.Enabled, Is.EqualTo(expected.Enabled));
				Assert.That(actual.IsChecked, Is.EqualTo(expected.Ticked));
			}
		});
	}

	[Test]
	public async Task MenuDisplayerShowContextMenuWithChildItemsCallsInteropWithLoadableMenuAsync()
	{
		var menuDisplayer = new Mock<IMenuDisplayer>();

		var childItems = new[]
		{
			new NetworkActionMenuItem("Child Item One", "Child Item One Tooltip", true, false, Enumerable.Empty<NetworkActionMenuItem>()),
			new NetworkActionMenuItem("Child Item Two", "Child Item Two Tooltip", true, true, Enumerable.Empty<NetworkActionMenuItem>()),
			new NetworkActionMenuItem("Child Item Three", "Child Item Three Tooltip", false, false, Enumerable.Empty<NetworkActionMenuItem>()),
			new NetworkActionMenuItem("Child Item Four", "Child Item Four Tooltip", false, true, Enumerable.Empty<NetworkActionMenuItem>()),
		};

		var menuItems = new[]
		{
			new NetworkActionMenuItem("Item One", "Item One Tooltip", true, false, Enumerable.Empty<NetworkActionMenuItem>()),
			new NetworkActionMenuItem("Item Two", "Item Two Tooltip", true, false, childItems),
		};

		await menuDisplayer.Object.ShowContextMenuAsync(new WebMouseEventArgs(), menuItems, (_, _) => Task.CompletedTask);

		menuDisplayer.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		var menuInteropModel = menuDisplayer.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel, Is.Not.Null);
		Assert.That(menuItems.Length, Is.EqualTo(menuInteropModel!.MenuItems!.Length));

		var menuItemOne = menuInteropModel.MenuItems[0];
		var menuItemTwo = menuInteropModel.MenuItems[1];

		Assert.That(menuItemOne!.Loadable, Is.False);
		Assert.That(menuItemTwo!.Loadable, Is.True);
		Assert.That(menuItemTwo.SubMenuItems, Is.Null);

		var loadSubMenu = menuDisplayer.Invocations.Single().Arguments[1] as Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>;
		var submenuResult = await loadSubMenu!(new SubMenuLoadRequest(menuInteropModel.Id!.Value, menuItemTwo.Id!.Value));

		Assert.That(submenuResult, Is.Not.Null);
		Assert.That(childItems.Length, Is.EqualTo(submenuResult!.Length));
		Assert.Multiple(() => {
			for (var i = 0; i < menuItems.Length; i++)
			{
				var expected = childItems[i];
				var actual = submenuResult[i];
				Assert.That(actual!.Text, Is.EqualTo(expected.Name));
				Assert.That(actual.Enabled, Is.EqualTo(expected.Enabled));
				Assert.That(actual.IsChecked, Is.EqualTo(expected.Ticked));
			}
		});
	}

	[Test]
	public async Task MenuDisplayerShowContextMenuNullItemIsSeparatorAsync()
	{
		var menuDisplayer = new Mock<IMenuDisplayer>();

		var menuItems = new NetworkActionMenuItem?[]
		{
			null,
		};

		await menuDisplayer.Object.ShowContextMenuAsync(new WebMouseEventArgs(), menuItems, (_, _) => Task.CompletedTask);

		menuDisplayer.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		var menuInteropModel = menuDisplayer.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel!.MenuItems!.Length, Is.EqualTo(1));
		Assert.That(menuInteropModel!.MenuItems.First()!.IsSeparator, Is.True);
	}

	[Test]
	public async Task MenuDisplayerShowContextMenuAtMousePositionAsync()
	{
		var menuDisplayer = new Mock<IMenuDisplayer>();

		var childItems = new[]
		{
			new NetworkActionMenuItem("Child Item One", "Child Item One Tooltip", true, false, Enumerable.Empty<NetworkActionMenuItem>()),
			new NetworkActionMenuItem("Child Item Two", "Child Item Two Tooltip", true, true, Enumerable.Empty<NetworkActionMenuItem>()),
			new NetworkActionMenuItem("Child Item Three", "Child Item Three Tooltip", false, false, Enumerable.Empty<NetworkActionMenuItem>()),
			new NetworkActionMenuItem("Child Item Four", "Child Item Four Tooltip", false, true, Enumerable.Empty<NetworkActionMenuItem>()),
		};

		var menuItems = new[]
		{
			new NetworkActionMenuItem("Item One", "Item One Tooltip", true, false, Enumerable.Empty<NetworkActionMenuItem>()),
			new NetworkActionMenuItem("Item Two", "Item Two Tooltip", true, false, childItems),
		};

		await menuDisplayer.Object.ShowContextMenuAsync(new WebMouseEventArgs() { ClientX = 250, ClientY = 350 }, menuItems, (_, _) => Task.CompletedTask);

		menuDisplayer.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		var menuInteropModel = menuDisplayer.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel!.X, Is.EqualTo(250));
		Assert.That(menuInteropModel.Y, Is.EqualTo(350));
		Assert.That(menuInteropModel.DropDownDirection, Is.EqualTo(DropDownDirection.BelowRight));
	}

	[Test]
	public async Task MenuDisplayerShowContextMenuClickEventFiresCallbackWithActionAsync()
	{
		var menuDisplayer = new Mock<IMenuDisplayer>();
		var networkAction = new Mock<INetworkAction>();

		var menuItems = new NetworkActionMenuItem?[]
		{
			new NetworkActionMenuItem("Item One", "Item One Tooltip", true, false, Enumerable.Empty<NetworkActionMenuItem>(), networkAction.Object),
		};

		INetworkAction? clickedItemAction = null;
		WebMouseEventArgs? clickedItemMouseArgs = null;

		await menuDisplayer.Object.ShowContextMenuAsync(new WebMouseEventArgs() { ClientX = 250, ClientY = 350 }, menuItems, (INetworkAction networkAction, WebMouseEventArgs webMouseEventArgs) =>
		{
			clickedItemAction = networkAction;
			clickedItemMouseArgs = webMouseEventArgs;
			return Task.CompletedTask;
		});

		menuDisplayer.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		var menuInteropModel = menuDisplayer.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel!.MenuItems!.Length, Is.EqualTo(1));

		var closeMenu = menuDisplayer.Invocations.Single().Arguments[2] as Func<MenuClosedResult, Task>;
		var item = menuInteropModel.MenuItems[0];
		await closeMenu!(new MenuClosedResult(MenuClosedResultCode.SelectionMade, menuInteropModel.Id!.Value, item!.Id!.Value));

		Assert.That(clickedItemAction, Is.EqualTo(networkAction.Object));
		Assert.That(clickedItemMouseArgs!.ClientX, Is.EqualTo(250));
		Assert.That(clickedItemMouseArgs.ClientY, Is.EqualTo(350));
	}

	[Test]
	public async Task MenuDisplayerShowRibbonContextMenuCallsInteropAsync()
	{
		var menuDisplayer = new Mock<IMenuDisplayer>();

		var ribbonViewModel = new RibbonViewModel();
		ribbonViewModel.AddResources(new Dictionary<string, string>() {
			{ "TestImage1/base64", "SomeBase64ImageString1" },
			{ "TestImage2/base64", "SomeBase64ImageString2" },
			{ "TestImage3/base64", "SomeBase64ImageString3" },
		});

		var menuItems = new[]
		{
			new RibbonButtonViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Ribbon Button 1"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip 1"), new StaticNetworkAction(iconName: "TestImage1", isActivated: false)),
			new RibbonButtonViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Ribbon Button 2"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip 2"), new StaticNetworkAction(iconName: "TestImage2", isActivated: false)),
			new RibbonButtonViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Ribbon Button 3"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip 3"), new StaticNetworkAction(iconName: "TestImage3", isActivated: true)),
			new RibbonButtonViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Ribbon Button 4"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip 4"), null),
		};

		await menuDisplayer.Object.ShowRibbonContextMenuAsync(200, 200, menuItems, (_) => Task.CompletedTask);

		menuDisplayer.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		var menuInteropModel = menuDisplayer.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel, Is.Not.Null);

		Assert.That(menuItems.Length, Is.EqualTo(menuInteropModel!.MenuItems!.Length));

		Assert.Multiple(() => {
			for (var i = 0; i < menuItems.Length; i++)
			{
				var expected = menuItems[i];
				var actual = menuInteropModel.MenuItems[i];
				Assert.That(actual!.Text, Is.EqualTo(expected.Label.ToString()));
				Assert.That(actual.Description, Is.EqualTo(expected.Tooltip));
				Assert.That(actual.Image, Is.EqualTo(expected.Image64));
				Assert.That(actual.IsChecked, Is.EqualTo(expected.IsChecked));
			}
		});
	}

	[Test]
	public async Task MenuDisplayerShowRibbonContextMenuClickEventFiresCallbackWithActionAsync()
	{
		var menuDisplayer = new Mock<IMenuDisplayer>();

		var networkAction = new Mock<INetworkAction>();
		networkAction.Setup(a => a.IsEnabled()).Returns(() =>
		{
			var isEnabled = new Mock<INetworkActionAccessibility>();
			isEnabled.Setup(i => i.IsAllowed).Returns(true);
			return isEnabled.Object;
		});

		var menuItems = new RibbonButtonViewModel?[]
		{
			new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button 1"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip 1"), networkAction.Object),
		};

		INetworkAction? clickedItemAction = null;

		await menuDisplayer.Object.ShowRibbonContextMenuAsync(200, 200, menuItems, (INetworkAction networkAction) =>
		{
			clickedItemAction = networkAction;
			return Task.CompletedTask;
		});

		menuDisplayer.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		var menuInteropModel = menuDisplayer.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel!.MenuItems!.Length, Is.EqualTo(1));

		var closeMenu = menuDisplayer.Invocations.Single().Arguments[2] as Func<MenuClosedResult, Task>;
		var item = menuInteropModel.MenuItems[0];
		await closeMenu!(new MenuClosedResult(MenuClosedResultCode.SelectionMade, menuInteropModel.Id!.Value, item!.Id!.Value));

		Assert.That(clickedItemAction, Is.EqualTo(networkAction.Object));
	}

	[Test]
	public async Task MenuDisplayerShowRibbonContextMenuAtProvidedPositionAsync()
	{
		var menuDisplayer = new Mock<IMenuDisplayer>();

		var menuItems = new[]
		{
			new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button 1"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip 1"), null),
			new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button 2"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip 2"), null),
			new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button 3"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip 3"), null),
			new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button 4"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip 4"), null),
		};

		await menuDisplayer.Object.ShowRibbonContextMenuAsync(250, 350, menuItems, (_) => Task.CompletedTask);

		menuDisplayer.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		var menuInteropModel = menuDisplayer.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel!.X, Is.EqualTo(250));
		Assert.That(menuInteropModel.Y, Is.EqualTo(350));
		Assert.That(menuInteropModel.DropDownDirection, Is.EqualTo(DropDownDirection.BelowRight));
	}

	[Test]
	public async Task MenuDisplayerShowClipboardContextMenuHasCorrectItemsAsync()
	{
		var menuDisplayer = new Mock<IMenuDisplayer>();
		var jsModule = new Mock<IJSObjectReference>();
		jsModule.Setup(i => i.InvokeAsync<AvailableClipboardActions>("clipboard.getSupportedActions", It.IsAny<object[]>())).Returns(ValueTask.FromResult(new AvailableClipboardActions(true, true, true)));
		var jsRuntime = new Mock<IJSRuntimeWithMonitor>();
		var path = "/_content/WinzorFramework/js/module/clipboard.js";
		jsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>("import", new object[] { path })).ReturnsAsync(jsModule.Object);
		await using var clipboardInterop = new ClipboardJSInterop(jsRuntime.Object, new DummyFileVersionHash());
		var elementReference = new ElementReference(Guid.NewGuid().ToString());

		await menuDisplayer.Object.ShowClipboardContextMenuAsync(new WebMouseEventArgs(), clipboardInterop, elementReference);

		menuDisplayer.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		var menuInteropModel = menuDisplayer.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel, Is.Not.Null);

		Assert.That(menuInteropModel!.MenuItems!.Length, Is.EqualTo(3));
		Assert.That(menuInteropModel.MenuItems[0] !.Text, Is.EqualTo("Cut"));
		Assert.That(menuInteropModel.MenuItems[1] !.Text, Is.EqualTo("Copy"));
		Assert.That(menuInteropModel.MenuItems[2] !.Text, Is.EqualTo("Paste"));
	}

	[Test]
	public async Task MenuDisplayerShowClipboardContextMenuHasShortcutKeysAsync()
	{
		var menuDisplayer = new Mock<IMenuDisplayer>();
		var jsModule = new Mock<IJSObjectReference>();
		jsModule.Setup(i => i.InvokeAsync<AvailableClipboardActions>("clipboard.getSupportedActions", It.IsAny<object[]>())).Returns(ValueTask.FromResult(new AvailableClipboardActions(true, true, true)));
		var jsRuntime = new Mock<IJSRuntimeWithMonitor>();
		var path = "/_content/WinzorFramework/js/module/clipboard.js";
		jsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>("import", new object[] { path })).ReturnsAsync(jsModule.Object);
		await using var clipboardInterop = new ClipboardJSInterop(jsRuntime.Object, new DummyFileVersionHash());
		var elementReference = new ElementReference(Guid.NewGuid().ToString());

		await menuDisplayer.Object.ShowClipboardContextMenuAsync(new WebMouseEventArgs(), clipboardInterop, elementReference);

		menuDisplayer.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		var menuInteropModel = menuDisplayer.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel, Is.Not.Null);

		Assert.That(menuInteropModel!.MenuItems!.Length, Is.EqualTo(3));
		Assert.That(menuInteropModel.MenuItems[0] !.ShortcutString, Is.EqualTo("Ctrl+X"));
		Assert.That(menuInteropModel.MenuItems[1] !.ShortcutString, Is.EqualTo("Ctrl+C"));
		Assert.That(menuInteropModel.MenuItems[2] !.ShortcutString, Is.EqualTo("Ctrl+V"));
	}

	[Test]
	public async Task MenuDisplayerShowClipboardContextMenuEnabledIfAvailableAsync([Values]bool isAvailable)
	{
		var menuDisplayer = new Mock<IMenuDisplayer>();
		var jsModule = new Mock<IJSObjectReference>();
		jsModule.Setup(i => i.InvokeAsync<AvailableClipboardActions>("clipboard.getSupportedActions", It.IsAny<object[]>())).Returns(ValueTask.FromResult(new AvailableClipboardActions(isAvailable, isAvailable, isAvailable)));
		var jsRuntime = new Mock<IJSRuntimeWithMonitor>();
		var path = "/_content/WinzorFramework/js/module/clipboard.js";
		jsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>("import", new object[] { path })).ReturnsAsync(jsModule.Object);
		await using var clipboardInterop = new ClipboardJSInterop(jsRuntime.Object, new DummyFileVersionHash());
		var elementReference = new ElementReference(Guid.NewGuid().ToString());

		await menuDisplayer.Object.ShowClipboardContextMenuAsync(new WebMouseEventArgs(), clipboardInterop, elementReference);

		menuDisplayer.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		var menuInteropModel = menuDisplayer.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel, Is.Not.Null);

		Assert.That(menuInteropModel!.MenuItems!.Length, Is.EqualTo(3));
		Assert.That(menuInteropModel.MenuItems[0] !.Enabled, Is.EqualTo(isAvailable));
		Assert.That(menuInteropModel.MenuItems[1] !.Enabled, Is.EqualTo(isAvailable));
		Assert.That(menuInteropModel.MenuItems[2] !.Enabled, Is.EqualTo(isAvailable));
	}

	[Test]
	[TestCase("Cut", "clipboard.cut")]
	[TestCase("Copy", "clipboard.copy")]
	[TestCase("Paste", "clipboard.paste")]
	public async Task MenuDisplayerShowClipboardContextMenuPerformsClipboardActionAsync(string itemText, string jsMethodName)
	{
		var menuDisplayer = new Mock<IMenuDisplayer>();
		menuDisplayer.Setup(m => m.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()))
			.Returns(async (MenuInteropModel menu, Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>> _, Func<MenuClosedResult, Task> menuClosedCallback) =>
			{
				await menuClosedCallback(new MenuClosedResult(MenuClosedResultCode.SelectionMade, menu.Id!.Value, menu.MenuItems!.Where(i => i!.Text == itemText).Single()!.Id));
				return MenuShowResultCode.Shown;
			});
		var jsModule = new Mock<IJSObjectReference>();
		jsModule.Setup(i => i.InvokeAsync<AvailableClipboardActions>("clipboard.getSupportedActions", It.IsAny<object[]>())).Returns(ValueTask.FromResult(new AvailableClipboardActions(true, true, true)));
		var jsRuntime = new Mock<IJSRuntimeWithMonitor>();
		var path = "/_content/WinzorFramework/js/module/clipboard.js";
		jsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>("import", new object[] { path })).ReturnsAsync(jsModule.Object);
		await using var clipboardInterop = new ClipboardJSInterop(jsRuntime.Object, new DummyFileVersionHash());
		var elementReference = new ElementReference(Guid.NewGuid().ToString());

		await menuDisplayer.Object.ShowClipboardContextMenuAsync(new WebMouseEventArgs(), clipboardInterop, elementReference);

		menuDisplayer.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		jsModule.Verify(m => m.InvokeAsync<object>(jsMethodName, new object[] { elementReference }), Times.Once);
	}
}
