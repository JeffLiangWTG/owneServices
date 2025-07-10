using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Rectangle = Blazor.Diagrams.Core.Geometry.Rectangle;

namespace NetworkVisualisation.GUI.Winzor.Test;

class RibbonMenuButtonTest : BunitTestContext
{
	[Test]
	public async Task RibbonMenuButtonShowsRibbonContextMenuOnClickIfHasChildButtonsAsync()
	{
		var menuDisplayerMock = new Mock<IMenuDisplayer>();

		Services.AddSingleton(menuDisplayerMock.Object);

		var viewModel = new RibbonMenuButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), new StaticNetworkAction(childActions: new[] { new StaticNetworkAction() }));
		var cut = RenderComponent<RibbonMenuButton>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var ribbonMenuButton = cut.Find(".ribbonmenubutton");
		await ribbonMenuButton.ClickAsync(new WebMouseEventArgs());

		menuDisplayerMock.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);
	}

	[Test]
	public async Task RibbonMenuButtonDoesNotShowRibbonContextMenuOnClickIfNoChildButtonsPresentAsync()
	{
		var menuDisplayerMock = new Mock<IMenuDisplayer>();

		Services.AddSingleton(menuDisplayerMock.Object);

		var viewModel = new RibbonMenuButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), new StaticNetworkAction());
		var cut = RenderComponent<RibbonMenuButton>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var ribbonMenuButton = cut.Find(".ribbonmenubutton");
		await ribbonMenuButton.ClickAsync(new WebMouseEventArgs());

		menuDisplayerMock.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Never);
	}

	[Test]
	public async Task RibbonMenuButtonParametersPassedCorrectlyToMenuDisplayerAsync()
	{
		var menuDisplayerMock = new Mock<IMenuDisplayer>();

		Services.AddSingleton(menuDisplayerMock.Object);

		JSInterop.SetBoundingClientRect(new Rectangle(100, 100, 250, 250));

		var ribbonViewModel = new RibbonViewModel();
		ribbonViewModel.AddResources(new Dictionary<string, string>() {
			{ "TestImage1/base64", "SomeBase64ImageString1" },
			{ "TestImage2/base64", "SomeBase64ImageString2" },
			{ "TestImage3/base64", "SomeBase64ImageString3" },
		});

		var action = new StaticNetworkAction(childActions: new[]
		{
			new StaticNetworkAction(name: ResString.GetMultilingualString(string.Empty, "Ribbon Button 1"), description: ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip 1"), iconName: "TestImage1"),
			new StaticNetworkAction(name: ResString.GetMultilingualString(string.Empty, "Ribbon Button 2"), description: ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip 2"), iconName: "TestImage2"),
			new StaticNetworkAction(name: ResString.GetMultilingualString(string.Empty, "Ribbon Button 3"), description: ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip 3"), iconName: "TestImage3"),
			new StaticNetworkAction(name: ResString.GetMultilingualString(string.Empty, "Ribbon Button 4"), description: ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip 4")),
		});

		var viewModel = new RibbonMenuButtonViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), action);
		var cut = RenderComponent<RibbonMenuButton>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
		);

		var ribbonMenuButton = cut.Find(".ribbonmenubutton");
		await ribbonMenuButton.ClickAsync(new WebMouseEventArgs());

		menuDisplayerMock.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		var menuInteropModel = menuDisplayerMock.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel!.X, Is.EqualTo(250));
		Assert.That(menuInteropModel.Y, Is.EqualTo(250));
		Assert.That(menuInteropModel.DropDownDirection, Is.EqualTo(DropDownDirection.BelowRight));
		Assert.That(viewModel.ChildButtons.Count(), Is.EqualTo(menuInteropModel!.MenuItems!.Length));
		Assert.Multiple(() => {
			for (var i = 0; i < viewModel.ChildButtons.Count(); i++)
			{
				var expected = viewModel.ChildButtons.ElementAt(i);
				var actual = menuInteropModel.MenuItems[i];
				Assert.That(actual!.Text, Is.EqualTo(expected.Label.ToString()));
				Assert.That(actual.Description, Is.EqualTo(expected.Tooltip));
				Assert.That(actual.Image, Is.EqualTo(expected.Image64));
			}
		});
	}
}
