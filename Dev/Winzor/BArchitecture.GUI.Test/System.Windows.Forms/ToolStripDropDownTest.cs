using System.Collections.Generic;
using System.Threading.Tasks;
using Bunit;
using CargoWise.Blazor.Client.Integration.Menus;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

class ToolStripDropDownTest
{
	[Test]
	public async Task ToolStripDropDownOpensOnShowCloseOnHide()
	{
		var onOpeningFired = new TaskCompletionSource();
		var onClosingFired = new TaskCompletionSource();
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var form = new Form();
			var textbox = new TextBox();
			textbox.ContextMenuStrip = new ContextMenuStrip();
			textbox.ContextMenuStrip.Items.Add("Item");
			textbox.ContextMenuStrip.Opening += (s, e) => onOpeningFired.SetResult();
			textbox.ContextMenuStrip.Closing += (s, e) => onClosingFired.SetResult();
			Assert.That(textbox.ContextMenuStrip.Visible, Is.False);
			textbox.ContextMenuStrip.Show();
			Assert.That(textbox.ContextMenuStrip.Visible, Is.True);
			textbox.ContextMenuStrip.Hide();
			Assert.That(textbox.ContextMenuStrip.Visible, Is.False);
			form.Controls.Add(textbox);
			return form;
		});

		Assert.That(await onOpeningFired.Task.WithTimeout(TimeSpan.FromSeconds(3.0)), Is.True);
		Assert.That(await onClosingFired.Task.WithTimeout(TimeSpan.FromSeconds(3.0)), Is.True);
	}

	[Test]
	public async Task ToolStripDropDownLeftClickMenuPosition()
	{
		using var ctx = new WinzorTestContext();
		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button { Text = "Context Menu Strip", Left = 10, Top = 4 };
			var contextMenuStrip = new ContextMenuStrip();
			button.ContextMenuStrip = contextMenuStrip;
			contextMenuStrip.Items.Add("Menu1");
			contextMenuStrip.Items.Add("Menu2");
			button.Click += (sender, args) => contextMenuStrip.Show(button, new Drawing.Point(5, 5));
			form.Controls.Add(button);
			return form;
		}, clientServices);

		await rendered.Find(".button").ClickAsync(new WebMouseEventArgs());

		Assert.That(mockMenuDisplayer.Invocations.Count, Is.EqualTo(1));
		var menuInteropModel = (MenuInteropModel)mockMenuDisplayer.Invocations[0].Arguments[0];
		Assert.That(menuInteropModel.X, Is.EqualTo(15));
		Assert.That(menuInteropModel.Y, Is.EqualTo(9));
	}

	[Test]
	public async Task HideDropDownShouldCloseMenuStrip()
	{
		using var ctx = new WinzorTestContext();
		ContextMenuStrip contextMenuStrip = null;
		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendCloseMenuRequestAsync());
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button = new Button { Text = "Context Menu Strip", Left = 10, Top = 4 };
			contextMenuStrip = new ContextMenuStrip();
			var contextItem = new ToolStripDropDownItem { Text = "item1" };
			button.ContextMenuStrip = contextMenuStrip;
			contextMenuStrip.Items.Add(contextItem);
			contextItem.DropDownItems.AddRange(new ToolStripMenuItem[] { new ToolStripMenuItem("subItem1") });
			button.Click += (sender, args) => contextMenuStrip.Hide(button);
			form.Controls.Add(button);
			return form;
		}, clientServices);

		await rendered.Find(".button").ClickAsync(new WebMouseEventArgs());

		Assert.That(mockMenuDisplayer.Invocations.Count, Is.EqualTo(1));
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripDropDownMenuItemShowImageIfAny()
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryTestServerContext(clientServiceProvider);

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var button = new Button { Text = "Context Menu Strip", Left = 10, Top = 4 };
			var contextMenuStrip = new ContextMenuStrip();
			button.ContextMenuStrip = contextMenuStrip;
			contextMenuStrip.Items.Add("Menu Item With Image", TestImage.GetImage(), null);
			contextMenuStrip.Items.Add("Menu Item Without Image");
			button.Click += (sender, args) => contextMenuStrip.Show(button, new Drawing.Point(5, 5));
			return button;
		});

		await Page.BringToFrontAsync();

		MenuInteropModel menu = null;
		clientServiceProvider.MockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()))
			.Callback<MenuInteropModel, Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>, Func<MenuClosedResult, Task>>(
			(model, _, _) => menu = model)
			.Returns(Task.FromResult(MenuShowResultCode.Shown));

		var button = page.Locator(".button");
		await SimulateClickAsync(button, MouseButton.Right);

		Assert.That(menu.MenuItems[0].Text, Is.EqualTo("Menu Item With Image"));
		Assert.That(menu.MenuItems[0].Image, Is.Not.Null);
		Assert.That(menu.MenuItems[1].Text, Is.EqualTo("Menu Item Without Image"));
		Assert.That(menu.MenuItems[1].Image, Is.Null);
	}

	[TestCaseSource(nameof(DefaultDropDownDirectionTestData))]
	[Diagnostics.CodeAnalysis.SuppressMessage("Naming", "EDI006:RightToLeftRule", Justification = "Testing code")]
	public async Task ToolStripDefaultDropDownDirectionIsSetCorrectly(ToolStripLayoutStyle layoutStyle, DockStyle dock, RightToLeft rightToLeft, ToolStripDropDownDirection expected)
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new ToolStripDropDown
			{
				LayoutStyle = layoutStyle,
				Dock = dock,
				RightToLeft = rightToLeft
			};
			Assert.That(control.DefaultDropDownDirection, Is.EqualTo(expected));
		});
	}

	[Test]
	public async Task SetVisibleFalseWhenParentNotVisible()
	{
		using var ctx = new WinzorTestContext();
		using var dispatcherContext = new CargoWiseTestWinzorDispatcherContext(ctx);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using (WinzorDispatcher.Current.WithContext(dispatcherContext))
			{
				var parent = new Form();
				var control = new ToolStripDropDown();
				parent.Controls.Add(control);

				parent.Visible = false;
				control.Visible = true;
				parent.Visible = true;

				Assert.That(control.Visible, Is.True);

				parent.Visible = false;
				control.Visible = false;
				parent.Visible = true;

				Assert.That(control.Visible, Is.False);
			}
		});
	}

	async Task MouseDownWithDelayAsync(MouseDownOptions options = default, int delay = 300)
	{
		await Page.Mouse.DownAsync(options);
		await Task.Delay(delay);
	}

	async Task MouseUpWithDelayAsync(MouseUpOptions options = default, int delay = 300)
	{
		await Page.Mouse.UpAsync(options);
		await Task.Delay(delay);
	}

	async Task SimulateClickAsync(ILocator elementHandle, MouseButton mouseButton = default)
	{
		var elementRect = await elementHandle.BoundingBoxAsync();
		await Page.Mouse.MoveAsync(elementRect.X + elementRect.Width / 2, elementRect.Y + elementRect.Height / 2);
		await Task.Delay(200);
		await MouseDownWithDelayAsync(new MouseDownOptions { Button = mouseButton });
		await MouseUpWithDelayAsync(new MouseUpOptions { Button = mouseButton });
	}

	MockCargoWiseClientSeviceProvider GetClientServiceProviderWithNoClientApp()
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		clientServiceProvider.MockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()))
			.Returns(Task.FromResult(MenuShowResultCode.ClientApplicationUnavailable));
		return clientServiceProvider;
	}

	class TestToolStripButton : ToolStripButton
	{
		public TestToolStripButton() : base() { }

		protected override void OnImageMouseEnter()
		{
			OnImageMouseEnterCalled = true;
		}
		public bool OnImageMouseEnterCalled;

		protected override void OnImageMouseLeave()
		{
			OnImageMouseLeaveCalled = true;
		}
		public bool OnImageMouseLeaveCalled;
	}

	static IEnumerable<object[]> DefaultDropDownDirectionTestData()
	{
		yield return new object[] { ToolStripLayoutStyle.Flow, DockStyle.None, RightToLeft.Yes, ToolStripDropDownDirection.BelowLeft };
		yield return new object[] { ToolStripLayoutStyle.Flow, DockStyle.None, RightToLeft.No, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.Flow, DockStyle.None, RightToLeft.Inherit, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.Flow, DockStyle.Left, RightToLeft.Yes, ToolStripDropDownDirection.BelowLeft };
		yield return new object[] { ToolStripLayoutStyle.Flow, DockStyle.Left, RightToLeft.No, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.Flow, DockStyle.Left, RightToLeft.Inherit, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.Flow, DockStyle.Right, RightToLeft.Yes, ToolStripDropDownDirection.BelowLeft };
		yield return new object[] { ToolStripLayoutStyle.Flow, DockStyle.Right, RightToLeft.No, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.Flow, DockStyle.Right, RightToLeft.Inherit, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.Flow, DockStyle.Top, RightToLeft.Yes, ToolStripDropDownDirection.BelowLeft };
		yield return new object[] { ToolStripLayoutStyle.Flow, DockStyle.Top, RightToLeft.No, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.Flow, DockStyle.Top, RightToLeft.Inherit, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.Flow, DockStyle.Bottom, RightToLeft.Yes, ToolStripDropDownDirection.BelowLeft };
		yield return new object[] { ToolStripLayoutStyle.Flow, DockStyle.Bottom, RightToLeft.No, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.Flow, DockStyle.Bottom, RightToLeft.Inherit, ToolStripDropDownDirection.BelowRight };

		yield return new object[] { ToolStripLayoutStyle.HorizontalStackWithOverflow, DockStyle.None, RightToLeft.Yes, ToolStripDropDownDirection.BelowLeft };
		yield return new object[] { ToolStripLayoutStyle.HorizontalStackWithOverflow, DockStyle.None, RightToLeft.No, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.HorizontalStackWithOverflow, DockStyle.None, RightToLeft.Inherit, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.HorizontalStackWithOverflow, DockStyle.Right, RightToLeft.Yes, ToolStripDropDownDirection.BelowLeft };
		yield return new object[] { ToolStripLayoutStyle.HorizontalStackWithOverflow, DockStyle.Right, RightToLeft.No, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.HorizontalStackWithOverflow, DockStyle.Right, RightToLeft.Inherit, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.HorizontalStackWithOverflow, DockStyle.Left, RightToLeft.Yes, ToolStripDropDownDirection.BelowLeft };
		yield return new object[] { ToolStripLayoutStyle.HorizontalStackWithOverflow, DockStyle.Left, RightToLeft.No, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.HorizontalStackWithOverflow, DockStyle.Left, RightToLeft.Inherit, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.HorizontalStackWithOverflow, DockStyle.Top, RightToLeft.Yes, ToolStripDropDownDirection.BelowLeft };
		yield return new object[] { ToolStripLayoutStyle.HorizontalStackWithOverflow, DockStyle.Top, RightToLeft.No, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.HorizontalStackWithOverflow, DockStyle.Top, RightToLeft.Inherit, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.HorizontalStackWithOverflow, DockStyle.Bottom, RightToLeft.Yes, ToolStripDropDownDirection.BelowLeft };
		yield return new object[] { ToolStripLayoutStyle.HorizontalStackWithOverflow, DockStyle.Bottom, RightToLeft.No, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.HorizontalStackWithOverflow, DockStyle.Bottom, RightToLeft.Inherit, ToolStripDropDownDirection.BelowRight };

		yield return new object[] { ToolStripLayoutStyle.StackWithOverflow, DockStyle.None, RightToLeft.Yes, ToolStripDropDownDirection.BelowLeft };
		yield return new object[] { ToolStripLayoutStyle.StackWithOverflow, DockStyle.None, RightToLeft.No, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.StackWithOverflow, DockStyle.None, RightToLeft.Inherit, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.StackWithOverflow, DockStyle.Right, RightToLeft.Yes, ToolStripDropDownDirection.BelowLeft };
		yield return new object[] { ToolStripLayoutStyle.StackWithOverflow, DockStyle.Right, RightToLeft.No, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.StackWithOverflow, DockStyle.Right, RightToLeft.Inherit, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.StackWithOverflow, DockStyle.Left, RightToLeft.Yes, ToolStripDropDownDirection.BelowLeft };
		yield return new object[] { ToolStripLayoutStyle.StackWithOverflow, DockStyle.Left, RightToLeft.No, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.StackWithOverflow, DockStyle.Left, RightToLeft.Inherit, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.StackWithOverflow, DockStyle.Top, RightToLeft.Yes, ToolStripDropDownDirection.BelowLeft };
		yield return new object[] { ToolStripLayoutStyle.StackWithOverflow, DockStyle.Top, RightToLeft.No, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.StackWithOverflow, DockStyle.Top, RightToLeft.Inherit, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.StackWithOverflow, DockStyle.Bottom, RightToLeft.Yes, ToolStripDropDownDirection.BelowLeft };
		yield return new object[] { ToolStripLayoutStyle.StackWithOverflow, DockStyle.Bottom, RightToLeft.No, ToolStripDropDownDirection.BelowRight };
		yield return new object[] { ToolStripLayoutStyle.StackWithOverflow, DockStyle.Bottom, RightToLeft.Inherit, ToolStripDropDownDirection.BelowRight };

		yield return new object[] { ToolStripLayoutStyle.VerticalStackWithOverflow, DockStyle.None, RightToLeft.Yes, ToolStripDropDownDirection.Right };
		yield return new object[] { ToolStripLayoutStyle.VerticalStackWithOverflow, DockStyle.None, RightToLeft.No, ToolStripDropDownDirection.Right };
		yield return new object[] { ToolStripLayoutStyle.VerticalStackWithOverflow, DockStyle.None, RightToLeft.Inherit, ToolStripDropDownDirection.Right };
		yield return new object[] { ToolStripLayoutStyle.VerticalStackWithOverflow, DockStyle.Right, RightToLeft.Yes, ToolStripDropDownDirection.Right };
		yield return new object[] { ToolStripLayoutStyle.VerticalStackWithOverflow, DockStyle.Right, RightToLeft.No, ToolStripDropDownDirection.Left };
		yield return new object[] { ToolStripLayoutStyle.VerticalStackWithOverflow, DockStyle.Right, RightToLeft.Inherit, ToolStripDropDownDirection.Right };
		yield return new object[] { ToolStripLayoutStyle.VerticalStackWithOverflow, DockStyle.Left, RightToLeft.Yes, ToolStripDropDownDirection.Right };
		yield return new object[] { ToolStripLayoutStyle.VerticalStackWithOverflow, DockStyle.Left, RightToLeft.No, ToolStripDropDownDirection.Right };
		yield return new object[] { ToolStripLayoutStyle.VerticalStackWithOverflow, DockStyle.Left, RightToLeft.Inherit, ToolStripDropDownDirection.Right };
		yield return new object[] { ToolStripLayoutStyle.VerticalStackWithOverflow, DockStyle.Top, RightToLeft.Yes, ToolStripDropDownDirection.Right };
		yield return new object[] { ToolStripLayoutStyle.VerticalStackWithOverflow, DockStyle.Top, RightToLeft.No, ToolStripDropDownDirection.Right };
		yield return new object[] { ToolStripLayoutStyle.VerticalStackWithOverflow, DockStyle.Top, RightToLeft.Inherit, ToolStripDropDownDirection.Right };
		yield return new object[] { ToolStripLayoutStyle.VerticalStackWithOverflow, DockStyle.Bottom, RightToLeft.Yes, ToolStripDropDownDirection.Right };
		yield return new object[] { ToolStripLayoutStyle.VerticalStackWithOverflow, DockStyle.Bottom, RightToLeft.No, ToolStripDropDownDirection.Right };
		yield return new object[] { ToolStripLayoutStyle.VerticalStackWithOverflow, DockStyle.Bottom, RightToLeft.Inherit, ToolStripDropDownDirection.Right };
	}
}
