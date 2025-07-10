using AngleSharp.Dom;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.AspNetCore.Components;
using Moq;
using WTG.PlaywrightTesting;

namespace NetworkVisualisation.GUI.Winzor.Test;

class ControlBarTest : BunitTestContext
{
	[Test]
	public void ControlBarRendersDiv()
	{
		var viewModel = new NetworkUserControlViewModel(new DummyNetwork());
		var component = RenderComponent<ControlBar>();

		var bar = component.Find(".controlbar");
		Assert.That(bar.TagName, Is.EqualTo("DIV"));
	}

	[Test]
	public void ControlBarDoesNotRenderButtonsWithoutActions()
	{
		var cut = RenderComponent<ControlBar>();

		Assert.That(cut.FindAll(".controlbar"), Is.Not.Empty);
		Assert.That(cut.FindAll("button"), Is.Empty);
	}

	[Test]
	public void ControlBarRendersButtonsForActions()
	{
		INetworkAction SetupNetworkAction(string actionName)
		{
			var action = new Mock<INetworkAction>();
			action.Setup(a => a.GetName()).Returns(ResString.GetMultilingualString(string.Empty, actionName));
			action.Setup(a => a.GetDescription()).Returns(ResString.GetMultilingualString(string.Empty, actionName + " Description"));
			return action.Object;
		}

		void AssertButton(IElement element, string actionName)
		{
			Assert.That(element.TagName, Is.EqualTo("BUTTON"));
			Assert.That(element.TextContent, Is.EqualTo(actionName));
			Assert.That(element.GetAttribute("title"), Is.EqualTo($"{actionName} Description"));
		}

		var viewModel = new NetworkUserControlViewModel(new DummyNetwork());
		var component = RenderComponent<ControlBar>(parameters => parameters
			.Add(p => p.RefreshAction, SetupNetworkAction("Refresh"))
			.Add(p => p.PopoutAction, SetupNetworkAction("Pop Out"))
			.Add(p => p.FitAction, SetupNetworkAction("Fit"))
			.Add(p => p.FillAction, SetupNetworkAction("Fill"))
			.Add(p => p.OneHundredPercentAction, SetupNetworkAction("100%"))
			.Add(p => p.ZoomOutAction, SetupNetworkAction("-"))
			.Add(p => p.ZoomInAction, SetupNetworkAction("+"))
		);

		var buttons = component.FindAll(".controlbar__button");
		Assert.That(buttons.Count, Is.EqualTo(7));
		AssertButton(buttons[0], "Refresh");
		AssertButton(buttons[1], "Pop Out");
		AssertButton(buttons[2], "Fit");
		AssertButton(buttons[3], "Fill");
		AssertButton(buttons[4], "100%");
		AssertButton(buttons[5], "-");
		AssertButton(buttons[6], "+");
	}

	[TestCase("RefreshAction", TestName = "{m}_Refresh")]
	[TestCase("PopoutAction", TestName = "{m}_Popout")]
	[TestCase("FitAction", TestName = "{m}_Fit}")]
	[TestCase("FillAction", TestName = "{m}_Fill")]
	[TestCase("OneHundredPercentAction", TestName = "{m}_100%")]
	[TestCase("ZoomOutAction", TestName = "{m}_-")]
	[TestCase("ZoomInAction", TestName = "{m}_+")]
	public async Task ControlBarButtonClickFiresActionAsync(string actionName)
	{
		var networkUserControl = new Mock<INetworkUserControl>();
		networkUserControl
			.Setup(n => n.InvokeWinzorDispatcherAsync(It.IsAny<Action>()))
			.Callback((Action action) => action())
			.Returns(() => Task.CompletedTask);
		var action = new Mock<INetworkAction>();
		action.Setup(a => a.IsEnabled()).Returns(() =>
		{
			var isEnabled = new Mock<INetworkActionAccessibility>();
			isEnabled.Setup(i => i.IsAllowed).Returns(true);
			return isEnabled.Object;
		});

		var cut = RenderComponent<ControlBar>(parameters => parameters
			.Add(p => p.UserControl, networkUserControl.Object)
			.TryAdd(actionName, action.Object)
		);

		Assert.That(cut.FindAll("button").Count, Is.EqualTo(1));
		var button = cut.Find(".controlbar__button");
		await button.ClickAsync(new WebMouseEventArgs());

		action.Verify(a => a.Execute(), Times.Once);
		networkUserControl.Verify(m => m.InvokeWinzorDispatcherAsync(It.IsAny<Action>()), Times.Once());
	}

	[Test]
	public void ZoomSliderRenders()
	{
		var control = new Mock<INetworkUserControl>();
		var viewModel = new NetworkUserControlViewModel();
		control.Setup(c => c.ViewModel).Returns(viewModel);
		var component = RenderComponent<ControlBar>(parameters => parameters
			.Add(p => p.UserControl, control.Object)
		);

		var container = component.Find(".controlbar__zoomslidecontainer");
		Assert.That(container.TagName, Is.EqualTo("DIV"));
		var input = component.Find(".controlbar__zoomslider");
		Assert.That(input.TagName, Is.EqualTo("INPUT"));
	}

	[Test]
	public async Task ZoomSliderBindingAsync()
	{
		var zoomLevel = 100;
		var cut = RenderComponent<ControlBar>(parameters => parameters
			.Bind(p => p.Zoom, zoomLevel, newValue => zoomLevel = newValue, () => zoomLevel)
		);

		var zoomSlider = cut.Find(".controlbar__zoomslider");
		var zoomLabel = cut.Find(".controlbar__zoomlabel");
		Assert.That(zoomSlider!.GetAttribute("value"), Is.EqualTo("100"));
		Assert.That(zoomLabel.TextContent, Is.EqualTo("100%"));

		await zoomSlider.InputAsync(new ChangeEventArgs() { Value = "200" });
		Assert.That(zoomLevel, Is.EqualTo(200));
		Assert.That(zoomSlider.GetAttribute("value"), Is.EqualTo("200"));
		Assert.That(zoomLabel.TextContent, Is.EqualTo("200%"));

		zoomLevel = 50;
		cut.SetParametersAndRender(parameters => parameters
			.Add(p => p.Zoom, zoomLevel)
		);
		Assert.That(zoomSlider.GetAttribute("value"), Is.EqualTo("50"));
		Assert.That(zoomLabel.TextContent, Is.EqualTo("50%"));
		Assert.That(zoomLevel, Is.EqualTo(50));
	}

	[Test]
	public void StatusMessageRenderedWithTooltip()
	{
		var control = new Mock<INetworkUserControl>();
		var viewModel = new NetworkUserControlViewModel();
		control.Setup(c => c.StatusMessage).Returns("This is the status message");
		control.Setup(c => c.StatusTooltip).Returns("This is the status tooltip");
		var component = RenderComponent<ControlBar>(parameters => parameters
			.Add(p => p.UserControl, control.Object)
		);

		var statusMessage = component.Find(".controlbar__statusmessage");

		Assert.That(statusMessage.TextContent, Is.EqualTo("This is the status message"));
		Assert.That(statusMessage.GetAttribute("title"), Is.EqualTo("This is the status tooltip"));
	}

	[Test]
	public void StatusMessageRenderedWithoutTooltip()
	{
		var control = new Mock<INetworkUserControl>();
		var viewModel = new NetworkUserControlViewModel();
		control.Setup(c => c.StatusMessage).Returns("This is the status message");
		control.Setup(c => c.StatusTooltip).Returns((string?)null);
		var component = RenderComponent<ControlBar>(parameters => parameters
			.Add(p => p.UserControl, control.Object)
		);

		var statusMessage = component.Find(".controlbar__statusmessage");

		Assert.That(statusMessage.TextContent, Is.EqualTo("This is the status message"));
		Assert.That(statusMessage.GetAttribute("title"), Is.EqualTo(null));
	}

	[Test]
	public void StatusMessageNotRendered()
	{
		var control = new Mock<INetworkUserControl>();
		var viewModel = new NetworkUserControlViewModel();
		control.Setup(c => c.StatusMessage).Returns((string?)null);
		var component = RenderComponent<ControlBar>(parameters => parameters
			.Add(p => p.UserControl, control.Object)
		);

		Assert.Throws<ElementNotFoundException>(() => component.Find(".controlbar__statusmessage"));
	}

	[Test, WithPlaywrightPage]
	public async Task ControlBarContainsMarginBetweenBottomWindowAndControlButtonsAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		// Run on dispatcher thread to avoid InvalidOperationException
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			// Set up the refresher and network on the dispatcher thread
			var refresher = new NetworkRefresher();
			var diagramEntity = new CargoWise.NetworkVisualisation.Business.Entity() { SupportedActions = NetworkActions.None };
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = diagramEntity;

			// Initialize NetworkUserControl with the dispatcher-safe setup
			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher, null, null, true);
			networkUserControl.SetDataContext(network, false);

			return networkUserControl;
		});

		// Now interact with the control elements
		var controlBar = page.Locator(".controlbar");
		var controlBarButtonsMargin = page.Locator(".controlbar__button").First;

		var controlBarMarginBottom = await controlBarButtonsMargin.EvaluateAsync<string>("el => window.getComputedStyle(el).margin");
		var controlBarPadding = await controlBar.EvaluateAsync<string>("el => window.getComputedStyle(el).paddingRight");

		Assert.That(controlBarMarginBottom, Is.EqualTo("9px 0px"));
		Assert.That(controlBarPadding, Is.EqualTo("15px"));
	}
}
