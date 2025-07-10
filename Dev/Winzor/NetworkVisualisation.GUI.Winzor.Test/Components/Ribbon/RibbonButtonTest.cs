using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.Integration;
using Microsoft.AspNetCore.Components.Web;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;

namespace NetworkVisualisation.GUI.Winzor.Test;

class RibbonButtonTest : BunitTestContext
{
	[Test]
	public void RibbonButtonDoesNotRenderWithoutViewModel()
	{
		var cut = RenderComponent<RibbonButton>();

		Assert.That(cut.Markup, Is.Empty);

		var viewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), null);
		cut.SetParametersAndRender(parameters =>
			parameters.Add(p => p.ViewModel, viewModel)
		);

		Assert.That(cut.Find(".ribbonbutton"), Is.Not.Null);
	}

	[Test]
	public void RibbonButtonRendersAsButton()
	{
		var viewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), null);
		var cut = RenderComponent<RibbonButton>(parameters =>
			parameters.Add(p => p.ViewModel, viewModel)
		);

		var ribbonButton = cut.Find(".ribbonbutton");
		Assert.That(ribbonButton.TagName, Is.EqualTo("BUTTON"));
	}

	[Test]
	public async Task RibbonButtonClickFiresActionIfNoChildButtonsArePresentAsync()
	{
		var networkUserControl = new Mock<INetworkUserControl>();
		networkUserControl
			.Setup(n => n.InvokeWinzorDispatcherAsync(It.IsAny<Action>()))
			.Callback((Action action) => action())
			.Returns(() => Task.CompletedTask);
		networkUserControl
			.Setup(n => n.ExecuteRibbonAction(It.IsAny<INetworkAction>()))
			.Callback((INetworkAction networkAction) => networkAction.Execute());

		var actionWasFired = false;
		var action = new Mock<INetworkAction>();
		action.Setup(a => a.IsEnabled()).Returns(() =>
		{
			var isEnabled = new Mock<INetworkActionAccessibility>();
			isEnabled.Setup(i => i.IsAllowed).Returns(true);
			return isEnabled.Object;
		});
		action.Setup(a => a.Execute()).Callback(() => actionWasFired = true);

		var viewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), action.Object);
		var cut = RenderComponent<RibbonButton>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
			.Add(p => p.NetworkUserControl, networkUserControl.Object)
		);

		var ribbonButton = cut.Find(".ribbonbutton");
		await ribbonButton.ClickAsync(new MouseEventArgs());

		Assert.That(actionWasFired, Is.True);
		networkUserControl.Verify(m => m.InvokeWinzorDispatcherAsync(It.IsAny<Action>()), Times.Once());
		networkUserControl.Verify(m => m.ExecuteRibbonAction(It.IsAny<INetworkAction>()), Times.Once());
	}

	[Test]
	public async Task RibbonButtonClickDoesNotFireActionIfChildButtonsArePresentAsync()
	{
		var networkUserControl = new Mock<INetworkUserControl>();
		networkUserControl
			.Setup(n => n.InvokeWinzorDispatcherAsync(It.IsAny<Action>()))
			.Callback((Action action) => action())
			.Returns(() => Task.CompletedTask);
		networkUserControl
			.Setup(n => n.ExecuteRibbonAction(It.IsAny<INetworkAction>()))
			.Callback((INetworkAction networkAction) => networkAction.Execute());

		var actionWasFired = false;
		var action = new Mock<INetworkAction>();
		action.Setup(a => a.IsEnabled()).Returns(() =>
		{
			var isEnabled = new Mock<INetworkActionAccessibility>();
			isEnabled.Setup(i => i.IsAllowed).Returns(true);
			return isEnabled.Object;
		});
		action.Setup(a => a.GetChildActions()).Returns(() => new[] { new StaticNetworkAction() });
		action.Setup(a => a.Execute()).Callback(() => actionWasFired = true);

		var viewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), action.Object);
		var cut = RenderComponent<RibbonButton>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
			.Add(p => p.NetworkUserControl, networkUserControl.Object)
		);

		var ribbonButton = cut.Find(".ribbonbutton");
		await ribbonButton.ClickAsync(new MouseEventArgs());

		Assert.That(actionWasFired, Is.False);
		networkUserControl.Verify(m => m.InvokeWinzorDispatcherAsync(It.IsAny<Action>()), Times.Never());
		networkUserControl.Verify(m => m.ExecuteRibbonAction(It.IsAny<INetworkAction>()), Times.Never());
	}

	[TestCase(RibbonImageLayout.BothLargeAndSmallImages, "ribbonbutton--large", TestName = "{m}_BothLargeAndSmallImages")]
	[TestCase(RibbonImageLayout.SmallImageOnly, "ribbonbutton--small", TestName = "{m}_SmallImageOnly")]
	public void RibbonButtonIconLayoutClass(RibbonImageLayout ribbonImageLayout, string className)
	{
		var viewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), string.Empty, ribbonImageLayout, null);
		var cut = RenderComponent<RibbonButton>(parameters =>
			parameters.Add(p => p.ViewModel, viewModel)
		);

		var ribbonButton = cut.Find(".ribbonbutton");
		Assert.That(ribbonButton.ClassList, Does.Contain(className));
	}

	[Test]
	public void RibbonButtonRendersImage()
	{
		var ribbonViewModel = new RibbonViewModel();
		ribbonViewModel.AddResources(new Dictionary<string, string>() { { "TestImage", "test/image.svg" } });
		var viewModel = new RibbonButtonViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), "TestImage", RibbonImageLayout.SmallImageOnly, null);
		var cut = RenderComponent<RibbonButton>(parameters =>
			parameters.Add(p => p.ViewModel, viewModel)
		);

		var ribbonButtonImages = cut.FindAll(".ribbonbutton .ribbonbutton__image");
		Assert.That(ribbonButtonImages.Count, Is.EqualTo(1));
		Assert.That(ribbonButtonImages[0].GetAttribute("src"), Is.EqualTo("test/image.svg"));
	}

	[Test]
	public void RibbonButtonNoImageRendered()
	{
		var viewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), null);
		var cut = RenderComponent<RibbonButton>(parameters =>
			parameters.Add(p => p.ViewModel, viewModel)
		);

		var ribbonButtonImages = cut.FindAll(".ribbonbutton .ribbonbutton__image");
		Assert.That(ribbonButtonImages.Count, Is.EqualTo(0));
	}

	[Test]
	public void RibbonButtonRendersLabel()
	{
		var viewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), null);
		var cut = RenderComponent<RibbonButton>(parameters =>
			parameters.Add(p => p.ViewModel, viewModel)
		);

		var ribbonButtonLabel = cut.Find(".ribbonbutton__label");
		Assert.That(ribbonButtonLabel.TextContent, Is.EqualTo("Ribbon Button"));
	}

	[Test]
	public void RibbonButtonToolTip()
	{
		var viewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), null);
		var cut = RenderComponent<RibbonButton>(parameters =>
			parameters.Add(p => p.ViewModel, viewModel)
		);

		var ribbonButton = cut.Find(".ribbonbutton");
		Assert.That(ribbonButton.GetAttribute("title"), Is.EqualTo("Ribbon Button ToolTip"));
	}

	[Test]
	public void RibbonButtonDisabled()
	{
		var viewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), null) { IsEnabled = false };
		var cut = RenderComponent<RibbonButton>(parameters =>
			parameters.Add(p => p.ViewModel, viewModel)
		);

		var ribbonButton = cut.Find(".ribbonbutton").Attributes["disabled"];
		Assert.That(ribbonButton, Is.Not.Null);
	}

	[Test]
	public void RibbonButtonEnabled()
	{
		var viewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), null);
		var cut = RenderComponent<RibbonButton>(parameters =>
			parameters.Add(p => p.ViewModel, viewModel)
		);

		var ribbonButtonDisabledAttribute = cut.Find(".ribbonbutton").Attributes["disabled"];
		Assert.That(ribbonButtonDisabledAttribute, Is.Null);
	}

	[Test]
	public void RibbonButtonNotDraggable()
	{
		var ribbonViewModel = new RibbonViewModel();
		ribbonViewModel.AddResources(new Dictionary<string, string>() { { "TestImage", "test/image.svg" } });
		var viewModel = new RibbonButtonViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), "TestImage", RibbonImageLayout.SmallImageOnly, null);
		var cut = RenderComponent<RibbonButton>(parameters =>
			parameters.Add(p => p.ViewModel, viewModel)
		);

		var ribbonButtonImages = cut.FindAll(".ribbonbutton .ribbonbutton__image");
		Assert.That(ribbonButtonImages.Count, Is.EqualTo(1));
		Assert.That(ribbonButtonImages[0].GetAttribute("draggable"), Is.EqualTo("false"));
	}

	[Test]
	public void RibbonButtonRendersDropDownArrowIfChildButtonsArePresent()
	{
		var action = new StaticNetworkAction(childActions: new[] { new StaticNetworkAction() });
		var viewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), action);
		var cut = RenderComponent<RibbonButton>(parameters =>
			parameters.Add(p => p.ViewModel, viewModel)
		);

		var dropDownArrow = cut.FindAll(".ribbonbutton__dropdownarrow");
		Assert.That(dropDownArrow.Count, Is.EqualTo(1));
	}

	[Test]
	public void RibbonButtonDropDownArrowNotRenderedIfNoChildButtonsArePresent()
	{
		var viewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), new StaticNetworkAction());
		var cut = RenderComponent<RibbonButton>(parameters =>
			parameters.Add(p => p.ViewModel, viewModel)
		);

		var dropDownArrow = cut.FindAll(".ribbonbutton__dropdownarrow");
		Assert.That(dropDownArrow.Count, Is.EqualTo(0));
	}

	[Test]
	public void RibbonButtonEnabledChanges()
	{
		var viewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), null);
		var cut = RenderComponent<RibbonButton>(parameters =>
			{
				parameters.Add(p => p.ViewModel, viewModel)
					.Add(p => p.NetworkUserControl, new NetworkUserControlForTest(Renderer));
			}
		);

		var ribbonButtonDisabledAttribute = cut.Find(".ribbonbutton").Attributes["disabled"];
		Assert.That(ribbonButtonDisabledAttribute, Is.Null);
		cut.Instance.ViewModel!.IsEnabled = false;
		ribbonButtonDisabledAttribute = cut.Find(".ribbonbutton").Attributes["disabled"];
		Assert.That(ribbonButtonDisabledAttribute, Is.Not.Null);
		cut.Instance.ViewModel!.IsEnabled = true;
		ribbonButtonDisabledAttribute = cut.Find(".ribbonbutton").Attributes["disabled"];
		Assert.That(ribbonButtonDisabledAttribute, Is.Null);
	}

	[Test]
	public void RibbonButtonSubscribesToPropertyChangedOfNewViewModel()
	{
		var viewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), null);
		var cut = RenderComponent<RibbonButton>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
			.Add(p => p.NetworkUserControl, new NetworkUserControlForTest(Renderer))
		);

		viewModel.IsEnabled = false;
		Assert.That(cut.Find(".ribbonbutton").Attributes["disabled"], Is.Not.Null);
		viewModel.IsEnabled = true;
		Assert.That(cut.Find(".ribbonbutton").Attributes["disabled"], Is.Null);

		var newViewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), null);
		cut.SetParametersAndRender(parameters => parameters
			.Add(p => p.ViewModel, newViewModel)
		);

		newViewModel.IsEnabled = false;
		Assert.That(cut.Find(".ribbonbutton").Attributes["disabled"], Is.Not.Null);
		newViewModel.IsEnabled = true;
		Assert.That(cut.Find(".ribbonbutton").Attributes["disabled"], Is.Null);
	}

	[Test]
	public async Task RibbonButtonClickDoesNotFireActionWhenDisabledAsync()
	{
		var networkUserControl = new Mock<INetworkUserControl>();
		networkUserControl
			.Setup(n => n.InvokeWinzorDispatcherAsync(It.IsAny<Action>()))
			.Callback((Action action) => action())
			.Returns(() => Task.CompletedTask);
		networkUserControl
			.Setup(n => n.ExecuteRibbonAction(It.IsAny<INetworkAction>()))
			.Callback((INetworkAction networkAction) => networkAction.Execute());

		var actionWasFired = false;
		var action = new Mock<INetworkAction>();
		action.Setup(a => a.IsEnabled()).Returns(() =>
		{
			var isEnabled = new Mock<INetworkActionAccessibility>();
			isEnabled.Setup(i => i.IsAllowed).Returns(true);
			return isEnabled.Object;
		});
		action.Setup(a => a.Execute()).Callback(() => actionWasFired = true);

		var viewModel = new RibbonButtonViewModel(new RibbonViewModel(), ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), action.Object)
		{
			IsEnabled = false
		};
		var cut = RenderComponent<RibbonButton>(parameters => parameters
			.Add(p => p.ViewModel, viewModel)
			.Add(p => p.NetworkUserControl, networkUserControl.Object)
		);

		var ribbonButton = cut.Find(".ribbonbutton");
		await ribbonButton.ClickAsync(new MouseEventArgs());

		Assert.That(actionWasFired, Is.False);
		networkUserControl.Verify(m => m.InvokeWinzorDispatcherAsync(It.IsAny<Action>()), Times.Never());
		networkUserControl.Verify(m => m.ExecuteRibbonAction(It.IsAny<INetworkAction>()), Times.Never());
	}
}
