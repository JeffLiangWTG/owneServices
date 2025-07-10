using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Common;
using CargoWise.Main.Navigation;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;
internal sealed class RecentItemsControlTest
{
	[Test]
	[WithTransaction]
	public async Task RecentItemsControlContextMenuOpensAtExpectedPosition()
	{
		using var ctx = new EnterpriseTestContext();

		var contextMenuRequest = new TaskCompletionSource();
		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.Is<MenuInteropModel>(m => m.MenuType == MenuType.ContextMenu), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>())).Callback(() => contextMenuRequest.SetResult());
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		DummyZFilterStripControlCommon dummyFilterStrip = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var filterBizO = new DummyFilterStripBusinessObject();
			dummyFilterStrip = new DummyZFilterStripControlCommon(filterBizO);
			form.Controls.Add(dummyFilterStrip);
			return form;
		}, clientServices);

		Assert.That(ErrorReporter.LastExceptionReported, Is.Null);

		await rendered.GetForm().InvokeWinzorDispatcherAsync(() =>
		{
			var dummyModule = new DummyFilterGridModule();
			dummyFilterStrip.Grid.SetParentFilterGridModule(dummyModule);
			dummyFilterStrip.LoadRecentItems();

			var viewModel = dummyFilterStrip.RecentItemsControlExposed.DataContext as MenuSection;
			Assert.That(viewModel, Is.Not.Null);

			var recentItem = new LinkWrapper(dummyModule.ID.Name, Guid.NewGuid(), "http://", "description");
			RecentItemManager.Reset();
			RecentItemManager.Instance.AddOrUpdateRecentItems(dummyModule.ID.Name, recentItem);

			viewModel = dummyFilterStrip.RecentItemsControlExposed.DataContext as MenuSection;
			ControlForTest.MousePosition = new Point(123, 456);
			viewModel.Items[0].LinkRightClickAction.Execute(null);
		});

		Assert.That(ErrorReporter.LastExceptionReported, Is.Null);
		Assert.That(await contextMenuRequest.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		mockMenuDisplayer.Verify(o => o.SendShowMenuRequestAsync(It.Is<MenuInteropModel>(m => m.MenuType == MenuType.ContextMenu), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once());
		var menuInteropModel = (MenuInteropModel)mockMenuDisplayer.Invocations.First(i => ((MenuInteropModel)i.Arguments[0]).MenuType == MenuType.ContextMenu).Arguments[0];
		Assert.That(menuInteropModel.X, Is.EqualTo(123));
		Assert.That(menuInteropModel.Y, Is.EqualTo(456));
	}

	[Test, WithPlaywrightPage]
	public async Task RecentItemsControlAndLabelDoNotOverlap()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Width = 2000;

			var dummyFilterStrip = new DummyZFilterStripControlCommon(new DummyFilterStripBusinessObject());
			dummyFilterStrip.Grid.SetParentFilterGridModule(new DummyFilterGridModule());
			dummyFilterStrip.LoadRecentItems();

			form.Controls.Add(dummyFilterStrip);
			return form;
		});
		var recentItemsControl = await (await page.WaitForSelectorAsync(".recentitemscontrol")).BoundingBoxAsync();
		var label = await (await page.QuerySelectorAsync(".panel div:nth-of-type(2)")).BoundingBoxAsync();

		Assert.That(recentItemsControl.Y, Is.GreaterThan(label.Y + label.Height));
	}

	[Test, WithPlaywrightPage]
	public async Task RecentItemsControlIsCorrectlySized()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Width = 2000;

			var dummyFilterStrip = new DummyZFilterStripControlCommon(new DummyFilterStripBusinessObject());
			dummyFilterStrip.Grid.SetParentFilterGridModule(new DummyFilterGridModule());
			dummyFilterStrip.LoadRecentItems();

			form.Controls.Add(dummyFilterStrip);
			return form;
		});

		var recentItemsControl = page.Locator(".recentitemscontrol");
		var label = page.Locator(".panel div:nth-of-type(2)").First;

		Assert.That(async () => await recentItemsControl.EvaluateAsync<int>("e => e.getBoundingClientRect().left"), Is.EqualTo(800));
		Assert.That(async () => await recentItemsControl.EvaluateAsync<int>("e => e.getBoundingClientRect().top"), Is.EqualTo(22));
		Assert.That(async () => await recentItemsControl.EvaluateAsync<int>("e => e.getBoundingClientRect().width"), Is.EqualTo(456).After(3000, 100));
		Assert.That(async () => await recentItemsControl.EvaluateAsync<int>("e => e.getBoundingClientRect().height"), Is.EqualTo(34).After(3000, 100));

		Assert.That(async () => await label.EvaluateAsync<int>("e => e.getBoundingClientRect().left"), Is.EqualTo(800));
		Assert.That(async () => await label.EvaluateAsync<int>("e => e.getBoundingClientRect().top"), Is.EqualTo(4));
		Assert.That(async () => await label.EvaluateAsync<int>("e => e.getBoundingClientRect().width"), Is.EqualTo(456).After(3000, 100));
		Assert.That(async () => await label.EvaluateAsync<int>("e => e.getBoundingClientRect().height"), Is.EqualTo(16).After(3000, 100));
	}

	class ControlForTest : Control
	{
		public new static Point MousePosition
		{
			get => Control.MousePosition;
			set => Control.MousePosition = value;
		}
	}
}
