using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Menus;
using Moq;
using NUnit.Framework;
using WinzorFramework.Extensions;
using WinzorTestFramework;

namespace WinzorFramework;

sealed class MenuInteropTest
{
	[Test]
	public async Task MenuInteropShowAsyncRequestsMenuAtProvidedPosition()
	{
		using var ctx = new WinzorTestContext();

		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		MenuInterop menuInterop = null;
		var rendered = await ctx.RenderFormAsync(() => {
			var form = new Form();
			var textBox = new TextBox();
			menuInterop = new MenuInterop(textBox, MenuType.ContextMenu, null);
			form.Controls.Add(textBox);
			return form;
		}, clientServices);

		await menuInterop.ShowAsync(Array.Empty<IWinzorMenuItem>(), new Point(123, 456));

		Assert.That(mockMenuDisplayer.Invocations.Count, Is.EqualTo(1));
		var menuInteropModel = (MenuInteropModel)mockMenuDisplayer.Invocations[0].Arguments[0];
		Assert.That(menuInteropModel.X, Is.EqualTo(123));
		Assert.That(menuInteropModel.Y, Is.EqualTo(456));
	}

	[Test]
	public async Task MenuInteropShowAsyncRequestsMenuItemWithFontStyle()
	{
		using var ctx = new WinzorTestContext();

		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		MenuInterop menuInterop = null;
		MenuItem menuItem = null;
		var rendered = await ctx.RenderFormAsync(() => {
			var form = new Form();
			var textBox = new TextBox();
			menuInterop = new MenuInterop(textBox, MenuType.ContextMenu, null);
			menuItem = new MenuItem()
			{
				Font = new Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold)
			};
			form.Controls.Add(textBox);
			return form;
		}, clientServices);

		await menuInterop.ShowAsync(new IWinzorMenuItem[] { menuItem }, new Point());

		Assert.That(mockMenuDisplayer.Invocations.Count, Is.EqualTo(1));
		var menuInteropModel = (MenuInteropModel)mockMenuDisplayer.Invocations[0].Arguments[0];
		Assert.That(menuInteropModel.MenuItems.Length, Is.EqualTo(1));
		Assert.That(menuInteropModel.MenuItems[0].FontStyle, Is.EqualTo(CargoWise.Blazor.Client.Integration.Messaging.FontStyle.Bold));
	}

	[Test]
	[TestCase("Ctrl+Shift+F", Shortcut.CtrlShiftC, "Ctrl+Shift+F")]
	[TestCase("Ctrl+Shift+F", Shortcut.None, "Ctrl+Shift+F")]
	[TestCase(null, Shortcut.CtrlShiftC, "Ctrl+Shift+C")]
	public async Task MenuInteropShowAsyncRequestsMenuItemWithShortcut(string shortcutString, Shortcut shortcut, string expected)
	{
		using var ctx = new WinzorTestContext();

		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		MenuInterop menuInterop = null;
		MenuItem menuItem = null;
		var rendered = await ctx.RenderFormAsync(() => {
			var form = new Form();
			var textBox = new TextBox();
			menuInterop = new MenuInterop(textBox, MenuType.ContextMenu, textBox.WinzorControlGuid);
			menuItem = new MenuItem()
			{
				ShortcutString = shortcutString,
				Shortcut = shortcut
			};
			form.Controls.Add(textBox);
			return form;
		}, clientServices);

		await menuInterop.ShowAsync(new IWinzorMenuItem[] { menuItem }, new Point());

		Assert.That(mockMenuDisplayer.Invocations.Count, Is.EqualTo(1));
		var menuInteropModel = (MenuInteropModel)mockMenuDisplayer.Invocations[0].Arguments[0];
		Assert.That(menuInteropModel.MenuItems.Length, Is.EqualTo(1));
		Assert.That(menuInteropModel.MenuItems[0].ShortcutString, Is.EqualTo(expected));
	}

	[Test]
	public async Task MenuInteropShowAsyncRequestsMenuItemWithImage()
	{
		using var ctx = new WinzorTestContext();

		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		MenuInterop menuInterop = null;
		MenuItem menuItem = null;
		var rendered = await ctx.RenderFormAsync(() => {
			var form = new Form();
			var textBox = new TextBox();
			menuInterop = new MenuInterop(textBox, MenuType.ContextMenu, null);
			menuItem = new MenuItem()
			{
				Image = UnfilledStar,
			};
			form.Controls.Add(textBox);
			return form;
		}, clientServices);

		await menuInterop.ShowAsync(new IWinzorMenuItem[] { menuItem }, new Point());

		Assert.That(mockMenuDisplayer.Invocations.Count, Is.EqualTo(1));
		var menuInteropModel = (MenuInteropModel)mockMenuDisplayer.Invocations[0].Arguments[0];
		Assert.That(menuInteropModel.MenuItems.Length, Is.EqualTo(1));
		Assert.That(menuInteropModel.MenuItems[0].Image, Is.EqualTo(UnfilledStarImageBase64));
		Assert.That(menuInteropModel.ImageData, Is.Null);
	}

	[Test]
	public async Task MenuInteropShowAsyncRequestsMenuItemWithImageHoverState()
	{
		using var ctx = new WinzorTestContext();

		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		MenuInterop menuInterop = null;
		var rendered = await ctx.RenderFormAsync(() => {
			var form = new Form();
			var textBox = new TextBox();
			menuInterop = new MenuInterop(textBox, MenuType.ContextMenu, null);
			form.Controls.Add(textBox);
			return form;
		}, clientServices);

		var menuItem = new Mock<IWinzorMenuItem>();
		menuItem.Setup(i => i.Enabled).Returns(true);
		menuItem.Setup(i => i.Visible).Returns(true);
		menuItem.Setup(i => i.WinzorControlGuid).Returns(Guid.NewGuid());
		menuItem.Setup(i => i.Font).Returns(Control.DefaultFont);
		menuItem.Setup(i => i.Image).Returns(() => UnfilledStar);
		menuItem.Setup(i => i.ToolTipText).Returns(() => "Action Text");
		menuItem.Setup(i => i.HoverImage).Returns(() => FilledStar);
		menuItem.Setup(i => i.HoverToolTipText).Returns(() => "Add Favourite");

		await menuInterop.ShowAsync(new IWinzorMenuItem[] { menuItem.Object }, new Point());

		Assert.That(mockMenuDisplayer.Invocations.Count, Is.EqualTo(1));
		var menuInteropModel = (MenuInteropModel)mockMenuDisplayer.Invocations[0].Arguments[0];
		Assert.That(menuInteropModel.MenuItems.Length, Is.EqualTo(1));
		Assert.That(menuInteropModel.MenuItems[0].Image, Is.EqualTo(UnfilledStarImageBase64));
		Assert.That(menuInteropModel.MenuItems[0].ToolTipText, Is.EqualTo("Action Text"));
		Assert.That(menuInteropModel.MenuItems[0].ImageHoverState, Is.Not.Null);
		Assert.That(menuInteropModel.MenuItems[0].ImageHoverState.Image, Is.EqualTo(FilledStarImageBase64));
		Assert.That(menuInteropModel.MenuItems[0].ImageHoverState.ToolTipText, Is.EqualTo("Add Favourite"));
		Assert.That(menuInteropModel.ImageData, Is.Null);
	}

	[TestCase("S&ave && Close", "S&ave && Close")]
	[TestCase("Save and Close", "Save and Close")]
	public async Task TextShouldKeepMnemonicSymbolInMenuItem(string text, string expected)
	{
		using var ctx = new WinzorTestContext();
		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);
		MenuInterop menuInterop = null;
		MenuItem menuItem = null;
		var rendered = await ctx.RenderFormAsync(() => {
			var form = new Form();
			var textBox = new TextBox();
			menuInterop = new MenuInterop(textBox, MenuType.ContextMenu, null);
			menuItem = new MenuItem()
			{
				Text = text
			};
			form.Controls.Add(textBox);
			return form;
		}, clientServices);
		await menuInterop.ShowAsync(new IWinzorMenuItem[] { menuItem }, new Point());
		Assert.That(mockMenuDisplayer.Invocations.Count, Is.EqualTo(1));
		var menuInteropModel = (MenuInteropModel)mockMenuDisplayer.Invocations[0].Arguments[0];
		Assert.That(menuInteropModel.MenuItems[0].Text, Is.EqualTo(expected));
	}

	[Test]
	public async Task HandleAsSelectableMenuItemIsNotLoadable()
	{
		using var ctx = new WinzorTestContext();

		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		MenuInterop menuInterop = null;
		MenuItem menuItem = null;
		var rendered = await ctx.RenderFormAsync(() => {
			var form = new Form();
			var textBox = new TextBox();
			menuInterop = new MenuInterop(textBox, MenuType.ContextMenu, textBox.WinzorControlGuid);

			menuItem = new MenuItem()
			{
				HandleAsSelectable = true,
			};
			menuItem.Select += (_, _) => { };

			form.Controls.Add(textBox);
			return form;
		}, clientServices);

		Assert.That(((IWinzorMenuItem)menuItem).Loadable, Is.True);

		await menuInterop.ShowAsync(new IWinzorMenuItem[] { menuItem }, new Point());

		Assert.That(mockMenuDisplayer.Invocations.Count, Is.EqualTo(1));
		var menuInteropModel = (MenuInteropModel)mockMenuDisplayer.Invocations[0].Arguments[0];
		Assert.That(menuInteropModel.MenuItems.Length, Is.EqualTo(1));
		Assert.That(menuInteropModel.MenuItems[0].Loadable, Is.False);
	}

	[TestCase(MenuClosedResultCode.NoSelection, TestName = "{m}_MenuClosedResultCode_NoSelection")]
	[TestCase(MenuClosedResultCode.SelectionMade, TestName = "{m}_MenuClosedResultCode_SelectionMade")]
	[TestCase(MenuClosedResultCode.ImageSelectionMade, TestName = "{m}_MenuClosedResultCode_ImageSelectionMade")]
	public async Task MenuInteropDisabledItemShouldNeverInvokeClickCallback(MenuClosedResultCode menuClosedResultCode)
	{
		using var ctx = new WinzorTestContext();

		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		MenuInterop menuInterop = null;
		var menuId = Guid.NewGuid();
		var rendered = await ctx.RenderFormAsync(() => {
			var form = new Form();
			var textBox = new TextBox();
			menuInterop = new MenuInterop(textBox, MenuType.ContextMenu, menuId);
			form.Controls.Add(textBox);
			return form;
		}, clientServices);

		var menuItem = new Mock<IWinzorMenuItem>();
		menuItem.Setup(i => i.Enabled).Returns(false);
		menuItem.Setup(i => i.Visible).Returns(true);
		menuItem.Setup(i => i.WinzorControlGuid).Returns(Guid.NewGuid());
		menuItem.Setup(i => i.Font).Returns(Control.DefaultFont);

		await menuInterop.ShowAsync(new[] { menuItem.Object }, new Point(123, 456));
		mockMenuDisplayer.Verify(i => i.SendShowMenuRequestAsync(It.Is<MenuInteropModel>(m => m.MenuItems.Any()), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);
		await menuInterop.OnMenuClosedAsync(new MenuClosedResult(menuClosedResultCode, menuId, menuItem.Object.WinzorControlGuid));

		menuItem.Verify(i => i.PerformClick(), Times.Never);
		menuItem.Verify(i => i.OnImageMouseEnter(), Times.Never);
		menuItem.Verify(i => i.OnImageMouseLeave(), Times.Never);
	}

	[Test]
	public async Task MenuInteropDisabledItemShouldNotInvokeSelectCallback()
	{
		using var ctx = new WinzorTestContext();

		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		MenuInterop menuInterop = null;
		var menuId = Guid.NewGuid();
		var rendered = await ctx.RenderFormAsync(() => {
			var form = new Form();
			var textBox = new TextBox();
			menuInterop = new MenuInterop(textBox, MenuType.ContextMenu, menuId);
			form.Controls.Add(textBox);
			return form;
		}, clientServices);

		var menuItem = new Mock<IWinzorMenuItem>();
		menuItem.Setup(i => i.Enabled).Returns(false);
		menuItem.Setup(i => i.Visible).Returns(true);
		menuItem.Setup(i => i.WinzorControlGuid).Returns(Guid.NewGuid());
		menuItem.Setup(i => i.Font).Returns(Control.DefaultFont);

		await menuInterop.ShowAsync(new[] { menuItem.Object }, new Point(123, 456));
		mockMenuDisplayer.Verify(i => i.SendShowMenuRequestAsync(It.Is<MenuInteropModel>(m => m.MenuItems.Any()), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);
		var subMenu = await menuInterop.OnLoadSubMenuAsync(new SubMenuLoadRequest(menuId, menuItem.Object.WinzorControlGuid));

		menuItem.Verify(i => i.OnSelect(), Times.Never);
	}

	[Test]
	public async Task MenuInteropLoadSubMenuWithNonExistingItemIdShouldNotThrowException()
	{
		using var ctx = new WinzorTestContext();

		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		MenuInterop menuInterop = null;
		var menuId = Guid.NewGuid();
		var rendered = await ctx.RenderFormAsync(() => {
			var form = new Form();
			var textBox = new TextBox();
			menuInterop = new MenuInterop(textBox, MenuType.ContextMenu, menuId);
			form.Controls.Add(textBox);
			return form;
		}, clientServices);

		var menuItem = new Mock<IWinzorMenuItem>();
		menuItem.Setup(i => i.Enabled).Returns(true);
		menuItem.Setup(i => i.Visible).Returns(true);
		menuItem.Setup(i => i.WinzorControlGuid).Returns(Guid.NewGuid());
		menuItem.Setup(i => i.Font).Returns(Control.DefaultFont);

		await menuInterop.ShowAsync(new[] { menuItem.Object }, new Point(123, 456));
		mockMenuDisplayer.Verify(i => i.SendShowMenuRequestAsync(It.Is<MenuInteropModel>(m => m.MenuItems.Any()), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		Assert.DoesNotThrowAsync(async () => await menuInterop.OnLoadSubMenuAsync(new SubMenuLoadRequest(menuId, Guid.NewGuid())));
	}

	[Test]
	public async Task OnMenuClosedWithSelectionMadeShouldInvokeClickCallback()
	{
		using var ctx = new WinzorTestContext();

		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		MenuInterop menuInterop = null;
		var menuId = Guid.NewGuid();
		var rendered = await ctx.RenderFormAsync(() => {
			var form = new Form();
			var textBox = new TextBox();
			menuInterop = new MenuInterop(textBox, MenuType.ContextMenu, menuId);
			form.Controls.Add(textBox);
			return form;
		}, clientServices);

		var menuItem = new Mock<IWinzorMenuItem>();
		menuItem.Setup(i => i.Enabled).Returns(true);
		menuItem.Setup(i => i.Visible).Returns(true);
		menuItem.Setup(i => i.WinzorControlGuid).Returns(Guid.NewGuid());
		menuItem.Setup(i => i.Font).Returns(Control.DefaultFont);

		menuItem.Setup(i => i.PerformClick())
			.Callback(() =>
			{
				menuItem.Verify(i => i.OnImageMouseEnter(), Times.Never);
				menuItem.Verify(i => i.OnImageMouseLeave(), Times.Never);
			})
			.Verifiable(Times.Once);

		await menuInterop.ShowAsync(new[] { menuItem.Object }, new Point(123, 456));
		mockMenuDisplayer.Verify(i => i.SendShowMenuRequestAsync(It.Is<MenuInteropModel>(m => m.MenuItems.Any()), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);
		await menuInterop.OnMenuClosedAsync(new MenuClosedResult(MenuClosedResultCode.SelectionMade, menuId, menuItem.Object.WinzorControlGuid));

		menuItem.Verify();
	}

	[Test]
	public async Task OnMenuClosedWithImageSelectionMadeShouldHoverImageAndInvokeClickCallback()
	{
		using var ctx = new WinzorTestContext();

		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		MenuInterop menuInterop = null;
		var menuId = Guid.NewGuid();
		var rendered = await ctx.RenderFormAsync(() => {
			var form = new Form();
			var textBox = new TextBox();
			menuInterop = new MenuInterop(textBox, MenuType.ContextMenu, menuId);
			form.Controls.Add(textBox);
			return form;
		}, clientServices);

		var menuItem = new Mock<IWinzorMenuItem>();
		menuItem.Setup(i => i.Enabled).Returns(true);
		menuItem.Setup(i => i.Visible).Returns(true);
		menuItem.Setup(i => i.WinzorControlGuid).Returns(Guid.NewGuid());
		menuItem.Setup(i => i.Font).Returns(Control.DefaultFont);

		menuItem.Setup(i => i.PerformClick())
			.Callback(() =>
			{
				menuItem.Verify(i => i.OnImageMouseEnter(), Times.Once);
				menuItem.Verify(i => i.OnImageMouseLeave(), Times.Never);
			})
			.Verifiable(Times.Once);

		await menuInterop.ShowAsync(new[] { menuItem.Object }, new Point(123, 456));
		mockMenuDisplayer.Verify(i => i.SendShowMenuRequestAsync(It.Is<MenuInteropModel>(m => m.MenuItems.Any()), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);
		await menuInterop.OnMenuClosedAsync(new MenuClosedResult(MenuClosedResultCode.ImageSelectionMade, menuId, menuItem.Object.WinzorControlGuid));

		menuItem.Verify();
		menuItem.Verify(i => i.OnImageMouseEnter(), Times.Once);
		menuItem.Verify(i => i.OnImageMouseLeave(), Times.Once);
	}

	[Test]
	public async Task TestCreateMenuInteropModelDoesNotAddImageData()
	{
		using var ctx = new WinzorTestContext();

		Control control = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			control = new Control();
		});

		var menuItem = new Mock<IWinzorMenuItem>();
		var menuItem2 = new Mock<IWinzorMenuItem>();
		using var image = new Bitmap(2, 2);
		using var image2 = new Bitmap(2, 2);
		menuItem.Setup(m => m.Visible).Returns(true);
		menuItem2.Setup(m => m.Visible).Returns(true);
		menuItem.Setup(m => m.Image).Returns(image);
		menuItem2.Setup(m => m.Image).Returns(image2);
		menuItem.Setup(m => m.Font).Returns(Control.DefaultFont);
		menuItem2.Setup(m => m.Font).Returns(Control.DefaultFont);

		var menuInterop = new MenuInterop(control, MenuType.ContextMenu, null);
		var model = menuInterop.CreateInteropModel(new IWinzorMenuItem[] { menuItem.Object, menuItem2.Object }, new Point(), ToolStripDropDownDirection.Default);

		Assert.That(model.ImageData, Is.Null);
		Assert.That(model.MenuItems[0].Image, Is.EqualTo(image.ToBase64()));
		Assert.That(model.MenuItems[1].Image, Is.EqualTo(image2.ToBase64()));
	}

	[Test]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1093:Do Not Use System.Windows.Forms.ToolStrip Controls", Justification = "Testing")]
	public async Task OnLoadSubMenuAsyncShouldHaveCorrectSubmenus()
	{
		using var ctx = new WinzorTestContext();

		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));

		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object);

		MenuInterop menuInterop = null;
		ContextMenuStrip contextMenuStrip = null;

		var menuId = Guid.NewGuid();

		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => {
			form = new Form();

			var textBox = new TextBox();
			form.Controls.Add(textBox);

			menuInterop = new MenuInterop(textBox, MenuType.ContextMenu, menuId);

			contextMenuStrip = new ContextMenuStrip();

			var menuItem = new TestMenuItem("Parent") { Visible = true };
			menuItem.DropDownItems.Add(new ToolStripMenuItem("Child A") { Visible = true, Image = UnfilledStar });
			menuItem.DropDownItems.Add(new ToolStripMenuItem("Child B") { Visible = true, Image = UnfilledStar });
			menuItem.DropDownItems.Add(new ToolStripMenuItem("Child C") { Visible = true, Image = UnfilledStar });
			contextMenuStrip.Items.Add(menuItem);

			return form;
		}, clientServices);

		await menuInterop.ShowAsync(contextMenuStrip.Items.ToArray(), new Point(123, 456));
		mockMenuDisplayer.Verify(i => i.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()),
			Times.Once);

		var subMenu = await menuInterop.OnLoadSubMenuAsync(new SubMenuLoadRequest(Guid.NewGuid(), contextMenuStrip.Items[0].WinzorControlGuid));
		Assert.That(subMenu.Length, Is.EqualTo(3));
	}

	class TestMenuItem : ToolStripDropDownItem
	{
		public TestMenuItem(string text)
		{
		}

		public override bool Visible => true;
	}

	public static Image UnfilledStar => Image.FromStream(new MemoryStream(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAIPSURBVDhPTZMHr+JADITz/38W5FEEh0R5lIRQQyekQ6i+/cyBLpK1ya49nvFsrDCMJYoSSZJM1yiKNMIwlDRNZbFYyG63k9PpJHEcmwjNWWBy+A7FohCQ04nCRIIgMGCJ5Hkuvu9LtVqV0Wik+wBSnKaxZFkix+MegETRiSzLzOZRu/M+GAykVqvJeDzW7uyRBxgsATQSwi9lwEgqikIPe72edDod6ff7stlsdI8gjxqALBAvl4tu7vf7L9hsNlMG7Hmep7P4AMAGieQpg+12K47jqFZ0D4dDaTQaMplMFHg6naoUAJfLpXS7Xc1XBgzFcYZi2yVTVNdkCgnmYZkHEKQwi+l0LqWSLT8/Vfn9HYiFHYTnuQa5rXph9Hq95Ha7qaTr9arxfD7FdT3T6I/pHsrh8I8BtiRJpLZAHxB03u93BUAvs4I2XRcLX87nwswgfjMIgoMCFMVZqbdaLVmv1+oGEngYpG3bxo2dYfYwwO/LZ9GJLgSa0YkD2In2+XyuFnK+Wq10iAB/XeDgc4nYxC7XdTURys1mUwfIXGDH3cBK8qm1+PhcX1Ys4vqyfmyFFbOpVCrSbrd1oNTR2II2H3THwnK5LPV6XX+gx+OhTkAVRtwF5gAo3ZFvZdnZIEXmxh11QOgFGdAPVewDCND12jfr5jt4c5Xff2Ga5rpS/P9geYcB76zYTnGeI/0gfwFU2q0RSltprgAAAABJRU5ErkJggg==")));

	public static Image FilledStar => Image.FromStream(new MemoryStream(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAANYSURBVDhPHZPtVxRlGMbnT+pb/QMdT54U+NDB6BzsxCnMOIaCUYgeEAqykJZEhV1zQZKXlZcTUIZHlPIlxRcycBF1CRZ2l52ZZ+aZmZ1FIH89w4c5z6f7uq+57uun+TKPMCwcx8O0BKap49gSz3KJz8b568+7rCVXEUJgOxamY2A5OpadRcp1NCtr4zk5crkcupHGz0tsS0fqOtfHJ/jgvWKGB2NYwkDYJm7ORjczOK5A2gbaZm4bI2vuDFsyhedl+G9LsOmu833jMUoK99LTdZ4Nz1UDAkMNS2lhCxfH8tGk4eBKh+1th1ebWVznJWyt8PTxGO0th+jtbCbUXMvsvRtYRlL9agpHGjuuLSOHlvMcpawr62my2VlSqT+w9WlG+o7SFz7AzNQZwqEKJoZDZFbv4fsvMPQ4vsrDl0rAEllisW6+/e4oA4OnGB9rZqD3M7o7C7k28hEr823cmqwj2lVO/881XBlqoLunnlMttdy6eR0tsBOJ/MCed9+kqqqIwf4aYpfLmJp4n+WFSuzU1yTm6vhttJyxXw5z4eJ+vqx9h8LCt4j+FEJzHZNMJsHIcISaL0qIhivJLF9ATx7HzVaz9vxTVl9Wk15uZXmpg/7Yx5w4uZvR0TO8WJxRIapT8PoVec9kemqI8+2H6b9YpgTqlYMP0VcO4ol6FueOExsopS1UwO27Z8n7y0ixhhbc3HMttjfybPomC08maTy2i3SiUW0+iEgfIRE/xOJ8Iy3NbzM3H8H1HqgrJHeC1DxViEBkw8/jWgaT472c/qaU9L/txB9XsBSvwsy0knh+lqaTe7n6exuu+wxXXc7WlUAQYiAQdIHXW/RdOsePrRXM3j/NpXABLQ1vcLmnhIf3O+g8V0k02rSz3VW1DzDQXHsd21btUlmsrS5x4JMSvqoupa5mD9PXmnk6GyEaqaCpYT/l5QWcqP+cZPIZ0rLV5ygHVkZlIPBUx2ce3KGoaBfVR8q4MdlLeuW2amVC5bPE9M1hysqKKd5XyN9PHqrG2limUE2UNtn1ACKH+MI/dIU7ePTojqJxnbwiLniFrhhRQa+lV4kNXeHXqxPkN9ydxZpUKp4rVUU9PF8iVS8CaHIqE08oToSn8M4hTEcBp5YJhX4AlsJaiBT/A/hLV/7Z+oiqAAAAAElFTkSuQmCC")));

	string UnfilledStarImageBase64 => Convert.ToBase64String((byte[])new ImageConverter().ConvertTo(UnfilledStar, typeof(byte[])));

	string FilledStarImageBase64 => Convert.ToBase64String((byte[])new ImageConverter().ConvertTo(FilledStar, typeof(byte[])));
}
