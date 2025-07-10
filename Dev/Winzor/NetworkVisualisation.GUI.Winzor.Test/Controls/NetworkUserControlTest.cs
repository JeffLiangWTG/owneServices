using System.Windows.Forms;
using AngleSharp.Css.Dom;
using Blazor.Diagrams.Components;
using Bunit.TestDoubles;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Components;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.AspNetCore.Components;
using Microsoft.Playwright;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using Entity = CargoWise.NetworkVisualisation.Business.Entity;

namespace NetworkVisualisation.GUI.Winzor.Test.Controls;

class NetworkUserControlTest : BunitTestContext
{
	[Test, WithTransaction]
	public async Task NCNRenderedAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var cut = await ctx.RenderControlOnFormAsync(() =>
		{
			var network = new DummyNetwork();
			var networkUserControl = new NetworkUserControl(new Entity(), new NetworkRefresher());
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		Assert.That(cut.FindAll(".diagram-canvas"), Is.Not.Empty);
	}

	[Test, WithTransaction]
	public async Task ScaledNCNRenderedAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var cut = await ctx.RenderControlOnFormAsync(() =>
		{
			var diagramMock = new Mock<IDiagramEntity>();
			diagramMock.SetupGet(d => d.IsDiagramScaled).Returns(true);
			var network = new DummyNetwork();
			var networkUserControl = new NetworkUserControl(diagramMock.Object, new NetworkRefresher());
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		Assert.That(cut.FindAll(".diagram-canvas"), Is.Not.Empty);
	}

	[Test]
	public async Task TestNonScheduledSectionAndSplitter_ShouldBeShown_AfterEnablingTheOptionAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var diagramMock = new Mock<IDiagramEntity>();

		diagramMock.SetupGet(d => d.IsDiagramScaled).Returns(true);
		diagramMock.SetupGet(d => d.ScaleUnitPixelSize).Returns(100);
		diagramMock.Setup(d => d.ShouldShowNonScheduledSection).Returns(true);
		diagramMock.Setup(d => d.HiddenEntities).Returns(new ImpObservableCollection<IProposedNetworkEntity>());
		diagramMock.Setup(d => d.HiddenRelationships).Returns(new ImpObservableCollection<IEntityRelationship>());

		var cut = await ctx.RenderControlOnFormAsync(() =>
		{
			var network = new DummyNetwork();
			network.DiagramEntity = diagramMock.Object;
			var networkUserControl = new NetworkUserControl(diagramMock.Object, new NetworkRefresher());
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		Assert.That(cut.FindAll(".networkusercontrol__resizer"), Is.Not.Empty);
		Assert.That(cut.FindAll(".networkusercontrol__nonscheduleditems"), Is.Not.Empty);
	}

	[Test]
	public async Task TestNonScheduledSection_ShouldResizeWidthWithinLimits_OnSplitterMovedAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var form = default(Form);
		var network = new DummyNetwork();

		var diagramMock = network.DiagramEntity;
		diagramMock.ShouldShowNonScheduledSection = true;
		diagramMock.NonScheduledSectionWidth = 600;

		NetworkUserControl? networkUserControl = null;

		await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			networkUserControl = new NetworkUserControl(diagramMock, new NetworkRefresher());
			networkUserControl.Width = 1500;
			networkUserControl.SetDataContext(network, false);
			form.Controls.Add(networkUserControl);
			return form;
		});

		var cut = await ctx.RenderControlOnFormAsync(() =>
		{
			return networkUserControl;
		});

		var splitterObject = cut.Find(".networkusercontrol__nonscheduleditems");
		var styleBeforeMove = splitterObject.GetStyle().GetWidth();
		Assert.That(styleBeforeMove, Does.Contain("600px"), "first check that the width is 600px");

		await cut.Find(".networkusercontrol__resizer").TriggerEventAsync("onsplittermoved", new SplitterMovedEventArgs()
		{
			xOffset = -100,
			yOffset = 0
		});

		var splitterObject1 = cut.Find(".networkusercontrol__nonscheduleditems");
		var styleAfterMove1 = splitterObject1.GetStyle().GetWidth();

		Assert.That(styleAfterMove1, Does.Contain("700px"), "second check that the width is now 700px after moving it by 100px to left");

		diagramMock.NonScheduledSectionWidth = 600;

		await cut.Find(".networkusercontrol__resizer").TriggerEventAsync("onsplittermoved", new SplitterMovedEventArgs()
		{
			xOffset = 200,
			yOffset = 0
		});

		var splitterObject2 = cut.Find(".networkusercontrol__nonscheduleditems");
		var styleAfterMove2 = splitterObject2.GetStyle().GetWidth();

		Assert.That(styleAfterMove2, Does.Contain("500px"), "third check that the width is still 500px, within limits, after moving it by 200px to right");

		diagramMock.NonScheduledSectionWidth = 1100;

		await cut.Find(".networkusercontrol__resizer").TriggerEventAsync("onsplittermoved", new SplitterMovedEventArgs()
		{
			xOffset = -100,
			yOffset = 0
		});

		var splitterObject3 = cut.Find(".networkusercontrol__nonscheduleditems");
		var styleAfterMove3 = splitterObject3.GetStyle().GetWidth();

		Assert.That(styleAfterMove3, Does.Contain("1150px"), "third check that the width is still 1150px, within limits, after moving it by 100px to left");

		cut.Dispose();
	}

	[TestCase(nameof(NetworkUserControlViewModel.ShapeInspectorVisible))]
	[TestCase(nameof(NetworkUserControlViewModel.ShapeInspectorNodeViewModel))]
	public async Task NotifiesRenderRequiredWhenViewModelPropertiesChangeAsync(string propertyName)
	{
		using var ctx = new EnterpriseTestContext();
		NetworkUserControl control = null!;
		await ctx.RenderControlOnFormAsync(() => control = CreateAndSetup());

		var mockDispatcherContext = Mock.Of<IWinzorDispatcherContext>();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using var context = ctx.WinzorDispatcher.WithContext(mockDispatcherContext);
			control.ViewModel.SetPropertyValue(propertyName, Activator.CreateInstance(control.ViewModel.GetPropertyType(propertyName)));
		});

		Mock.Get(mockDispatcherContext).Verify(dispatcherContext => dispatcherContext.NotifyRenderRequired(control), Times.Once);
	}

	[Test]
	public async Task ShapeInspectorVisibilityMatchesViewModelAsync([Values] bool visible)
	{
		using var ctx = new EnterpriseTestContext();
		ctx.ComponentFactories.AddStub<ShapeInspector>();
		var cut = await ctx.RenderControlOnFormAsync(() => CreateAndSetup(visible));
		Assert.That(cut.HasComponent<Stub<ShapeInspector>>(), Is.EqualTo(visible));
	}

	[Test]
	public async Task GivesShapeInspectorNodeFromViewModelAsync()
	{
		using var ctx = new EnterpriseTestContext();
		ctx.ComponentFactories.AddStub<ShapeInspector>();
		NetworkUserControl control = null!;
		var cut = await ctx.RenderControlOnFormAsync(() => control = CreateAndSetup(true));
		var nodeViewModel = Mock.Of<NodeViewModel>();
		await control.InvokeWinzorDispatcherAsync(() => control.ViewModel!.ShapeInspectorNodeViewModel = nodeViewModel);
		IRenderedComponent<Stub<ShapeInspector>> shapeInspectorStub = cut.FindComponent<Stub<ShapeInspector>>();
		Assert.That(shapeInspectorStub.Instance.Parameters.Get(stub => stub.ViewModel), Is.EqualTo(nodeViewModel));
	}

	[Test]
	public async Task InvokesSelectionChangedEventAndRefreshesRibbonViewModelWhenRefreshTypeIsCloseOrNoneAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var selectionChangeCount = 0;
		var actionRefreshCount = 0;
		var entity = new Entity();
		var network = new DummyNetwork();
		network.Entities.Add(entity);

		var mockAction = new Mock<INetworkAction>();
		mockAction.Setup(x => x.Refresh(It.IsAny<RefreshArgs>())).Callback(() => ++actionRefreshCount);
		mockAction.Setup(x => x.IsEnabled()).Returns(NetworkActionAccessibility.Allowed);

		var ribbon = new RibbonViewModel();
		ribbon.AddResources(new() { { "Shape", "" } });
		var tab = new RibbonTabViewModel(ResString.GetMultilingualString("DefaultRibbon|Home", "Home"));
		ribbon.Tabs.Add(tab);
		var group = new RibbonGroupViewModel(ribbon,
			ResString.GetMultilingualString("DefaultRibbon|Home|Insert", "Insert"), "Shape");
		tab.Groups.Add(group);
		var ribbonButtonViewModel = new RibbonButtonViewModel(ribbon, mockAction.Object);
		group.Items.Add(ribbonButtonViewModel);
		var ribbonProvider = new Mock<IRibbonDataProvider>();
		ribbonProvider
			.Setup(x => x.GetRibbonViewModel(It.IsAny<NetworkViewModel>(), It.IsAny<NetworkUserControl>()))
			.Returns(ribbon);
		NetworkUserControl control = null!;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			control = new(network.DiagramEntity, new NetworkRefresher(), ribbonDataProvider: ribbonProvider.Object);
			control.SetDataContext(network, false);
			control.SelectionChanged += (sender, args) =>
			{
				Assert.That(args.RefreshType, Is.EqualTo(RefreshType.None));
				Assert.That(args.Entities, Is.EquivalentTo(new[] { entity }));
				++selectionChangeCount;
			};
		});
		Assert.That(selectionChangeCount, Is.EqualTo(0));
		Assert.That(actionRefreshCount, Is.EqualTo(0));

		//Refresh Type close doesn't invoke selectionChanged event.
		network.Refresher.Refresh(RefreshType.Close);

		Assert.That(selectionChangeCount, Is.EqualTo(0));
		Assert.That(actionRefreshCount, Is.EqualTo(0));

		//Refresh Type None, invokes selectionChanged event.
		await control.InvokeWinzorDispatcherAsync(() => control.NetworkViewModel!.SelectEntities(new[] { entity }));
		Assert.That(selectionChangeCount, Is.EqualTo(1));
		Assert.That(actionRefreshCount, Is.EqualTo(1));
	}
	[Test]
	public async Task InvokesSelectionChangedEventAndRefreshesRibbonViewModelWhenASelectionIsMadeAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var selectionChangeCount = 0;
		var actionRefreshCount = 0;
		var entity = new Entity();
		var network = new DummyNetwork();
		network.Entities.Add(entity);

		var mockAction = new Mock<INetworkAction>();
		mockAction.Setup(x => x.Refresh(It.IsAny<RefreshArgs>())).Callback(() => ++actionRefreshCount);
		mockAction.Setup(x => x.IsEnabled()).Returns(NetworkActionAccessibility.Allowed);

		var ribbon = new RibbonViewModel();
		ribbon.AddResources(new() { { "Shape", "" } });
		var tab = new RibbonTabViewModel(ResString.GetMultilingualString("DefaultRibbon|Home", "Home"));
		ribbon.Tabs.Add(tab);
		var group = new RibbonGroupViewModel(ribbon,
			ResString.GetMultilingualString("DefaultRibbon|Home|Insert", "Insert"), "Shape");
		tab.Groups.Add(group);
		var ribbonButtonViewModel = new RibbonButtonViewModel(ribbon, mockAction.Object);
		group.Items.Add(ribbonButtonViewModel);
		var ribbonProvider = new Mock<IRibbonDataProvider>();
		ribbonProvider
			.Setup(x => x.GetRibbonViewModel(It.IsAny<NetworkViewModel>(), It.IsAny<NetworkUserControl>()))
			.Returns(ribbon);
		NetworkUserControl control = null!;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			control = new(network.DiagramEntity, new NetworkRefresher(), ribbonDataProvider: ribbonProvider.Object);
			control.SetDataContext(network, false);
			control.SelectionChanged += (sender, args) =>
			{
				Assert.That(args.RefreshType, Is.EqualTo(RefreshType.None));
				Assert.That(args.Entities, Is.EquivalentTo(new[] { entity }));
				++selectionChangeCount;
			};
		});
		Assert.That(selectionChangeCount, Is.EqualTo(0));
		Assert.That(actionRefreshCount, Is.EqualTo(0));
		await control.InvokeWinzorDispatcherAsync(() => control.NetworkViewModel!.SelectEntities(new[] { entity }));
		Assert.That(selectionChangeCount, Is.EqualTo(1));
		Assert.That(actionRefreshCount, Is.EqualTo(1));
	}

	NetworkUserControl CreateAndSetup(bool shapeInspectorVisible = false)
	{
		var mockNetwork = Mock.Of<INetwork>(x =>
			x.Entities == Mock.Of<IObservableReloadableCollection<INetworkEntity>>()
			&& x.DiagramEntity.ShapeInspectorVisible == shapeInspectorVisible
			&& x.DiagramEntity.HiddenEntities == Mock.Of<IObservableReloadableCollection<IProposedNetworkEntity>>()
			&& x.DiagramEntity.HiddenRelationships == Mock.Of<IObservableReloadableCollection<IEntityRelationship>>());
		var control = new NetworkUserControl(Mock.Of<IDiagramEntity>(), Mock.Of<INetworkRefresher>());
		control.SetDataContext(mockNetwork, false);
		return control;
	}

	[Test]
	public async Task OneHundredPercentChangesDiagramZoomAsync()
	{
		NetworkUserControl? control = null;
		using var ctx = new EnterpriseTestContext();

		var cut = await ctx.RenderControlOnFormAsync(() =>
		{
			var entity = new Entity();
			var diagramEntity = new Entity();
			var entities = new ImpObservableSet<INetworkEntity>(new[] { entity });
			var network = Mock.Of<INetwork>(x => x.Entities == entities && x.DiagramEntity == diagramEntity && x.EntityPositionStrategy == new EntityPositionStrategy());
			control = new NetworkUserControl(diagramEntity, Mock.Of<INetworkRefresher>());
			control.SetDataContext(network, false);
			return control;
		});

		var canvas = cut.FindComponent<DiagramCanvas>();

		await control!.InvokeWinzorDispatcherAsync(() => { control.ZoomOut(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.Not.EqualTo(1).Within(0.0005));
		await control.InvokeWinzorDispatcherAsync(() => { control.OneHundredPercent(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(1).Within(0.0005));
	}

	[Test]
	public async Task ZoomOutChangesDiagramZoomAsync()
	{
		NetworkUserControl? control = null;
		using var ctx = new EnterpriseTestContext();

		var cut = await ctx.RenderControlOnFormAsync(() =>
		{
			var entity = new Entity();
			var diagramEntity = new Entity();
			var entities = new ImpObservableSet<INetworkEntity>(new[] { entity });
			var network = Mock.Of<INetwork>(x => x.Entities == entities && x.DiagramEntity == diagramEntity && x.EntityPositionStrategy == new EntityPositionStrategy());
			control = new NetworkUserControl(diagramEntity, Mock.Of<INetworkRefresher>());
			control.SetDataContext(network, false);
			return control;
		});

		var canvas = cut.FindComponent<DiagramCanvas>();

		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(1).Within(0.0005));
		await control!.InvokeWinzorDispatcherAsync(() => { control.ZoomOut(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(0.9).Within(0.0005));
		await control.InvokeWinzorDispatcherAsync(() => { control.ZoomOut(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(0.8).Within(0.0005));
		await control.InvokeWinzorDispatcherAsync(() => { control.ZoomOut(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(0.7).Within(0.0005));
		await control.InvokeWinzorDispatcherAsync(() => { control.ZoomOut(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(0.6).Within(0.0005));
		await control.InvokeWinzorDispatcherAsync(() => { control.ZoomOut(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(0.5).Within(0.0005));
		await control.InvokeWinzorDispatcherAsync(() => { control.ZoomOut(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(0.4).Within(0.0005));
		await control.InvokeWinzorDispatcherAsync(() => { control.ZoomOut(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(0.3).Within(0.0005));
		await control.InvokeWinzorDispatcherAsync(() => { control.ZoomOut(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(0.2).Within(0.0005));
		await control.InvokeWinzorDispatcherAsync(() => { control.ZoomOut(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(0.1).Within(0.0005));

		// check ok if zoom out beyond limit
		await control.InvokeWinzorDispatcherAsync(() => { control.ZoomOut(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(0.1).Within(0.0005));
	}

	[Test]
	public async Task ZoomInChangesDiagramZoomAsync()
	{
		NetworkUserControl? control = null;
		using var ctx = new EnterpriseTestContext();

		var cut = await ctx.RenderControlOnFormAsync(() =>
		{
			var entity = new Entity();
			var diagramEntity = new Entity();
			var entities = new ImpObservableSet<INetworkEntity>(new[] { entity });
			var network = Mock.Of<INetwork>(x => x.Entities == entities && x.DiagramEntity == diagramEntity && x.EntityPositionStrategy == new EntityPositionStrategy());
			control = new NetworkUserControl(diagramEntity, Mock.Of<INetworkRefresher>());
			control.SetDataContext(network, false);
			return control;
		});

		var canvas = cut.FindComponent<DiagramCanvas>();

		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(1).Within(0.0005));
		await control!.InvokeWinzorDispatcherAsync(() => { control.ZoomIn(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(1.1).Within(0.0005));
		await control.InvokeWinzorDispatcherAsync(() => { control.ZoomIn(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(1.2).Within(0.0005));
		await control.InvokeWinzorDispatcherAsync(() => { control.ZoomIn(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(1.3).Within(0.0005));
		await control.InvokeWinzorDispatcherAsync(() => { control.ZoomIn(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(1.4).Within(0.0005));
	}

	[Test]
	public async Task ControlBarDoesRenderPopOutButtonForNCNAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram };

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher, null, null, false);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var controlBar = rendered.FindAll(".controlbar__button");
		var hasPopOutButton = controlBar.Any(button => button.TextContent.Contains("Pop Out"));

		Assert.That(controlBar.Count, Is.EqualTo(7));
		Assert.That(hasPopOutButton, Is.True, "Button with text Pop Out was found.");
	}

	[Test]
	public async Task ControlBarDoesNotRenderPopOutButtonForMiniNCNAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.None };

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher, null, null, true);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var controlBar = rendered.FindAll(".controlbar__button");
		var hasPopOutButton = controlBar.Any(button => button.TextContent.Contains("Pop Out"));

		Assert.That(controlBar.Count, Is.EqualTo(6));
		Assert.That(hasPopOutButton, Is.False, "Button with text Pop Out was not found.");
	}

	[Test]
	public async Task RefreshRibbonButtonActionsOnNetworkRefreshAsync()
	{
		NetworkUserControl? control = null;
		using var ctx = new EnterpriseTestContext();
		var timesRefreshed = 0;
		var action = new Mock<INetworkAction>();
		action.Setup(a => a.IsEnabled()).Returns(() =>
		{
			var isEnabled = new Mock<INetworkActionAccessibility>();
			isEnabled.Setup(i => i.IsAllowed).Returns(true);
			return isEnabled.Object;
		});
		action.Setup(a => a.Refresh(It.IsAny<RefreshArgs>())).Callback(() => timesRefreshed++);

		var ribbonDataProvider = new Mock<IRibbonDataProvider>();
		ribbonDataProvider
			.Setup(n => n.GetRibbonViewModel(It.IsAny<NetworkViewModel>(), It.IsAny<NetworkUserControl>()))
			.Returns(() =>
			{
				var ribbonViewModel = new RibbonViewModel();

				var ribbonTabViewModel = new RibbonTabViewModel(ResString.GetMultilingualString(string.Empty, "Tab"));
				ribbonViewModel.Tabs.Add(ribbonTabViewModel);

				var ribbonGroupViewModel = new RibbonGroupViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Group"), string.Empty);
				ribbonTabViewModel.Groups.Add(ribbonGroupViewModel);

				var ribbonButtonViewModel = new RibbonButtonViewModel(ribbonViewModel, ResString.GetMultilingualString(string.Empty, "Ribbon Button"), ResString.GetMultilingualString(string.Empty, "Ribbon Button ToolTip"), action.Object);
				ribbonGroupViewModel.Items.Add(ribbonButtonViewModel);

				return ribbonViewModel;
			});

		var cut = await ctx.RenderControlOnFormAsync(() =>
		{
			control = new NetworkUserControl(new Entity(), Mock.Of<INetworkRefresher>(), ribbonDataProvider: ribbonDataProvider.Object);
			control.SetDataContext(new DummyNetwork(), false);
			return control;
		});

		Assert.That(timesRefreshed, Is.EqualTo(0));
		await control!.InvokeWinzorDispatcherAsync(() => control.NetworkViewModel!.Network.Refresher.Refresh(RefreshType.None, Array.Empty<INetworkEntity>()));
		Assert.That(timesRefreshed, Is.EqualTo(1));
	}

	static IEnumerable<TestCaseData> JumpBackToPrevZoomTestCaseData
	{
		get
		{
			yield return new TestCaseData(
				(IPage page) => { return Task.CompletedTask; },
				1, 1, "100%"
			)
			{ TestName = "JumpBackToPrevZoom_NoPrevZoom_DoesNothing" };
			yield return new TestCaseData(
				async (IPage page) =>
				{
					var zoomIn = page.GetByTitle("Zoom in on the content (Ctrl++)");
					await zoomIn.ClickAsync();
					await zoomIn.ClickAsync();
					await zoomIn.ClickAsync();
				},
				1.3, 1.2, "120%"
			)
			{ TestName = "JumpBackToPrevZoom_AfterZoomIn_ZoomsToPrevZoom" };
			yield return new TestCaseData(
				async (IPage page) =>
				{
					var zoomOut = page.GetByTitle("Zoom out from the content (Ctrl+-)");
					await zoomOut.ClickAsync();
					await zoomOut.ClickAsync();
					await zoomOut.ClickAsync();
				},
				0.7, 0.8, "80%"
			)
			{ TestName = "JumpBackToPrevZoom_AfterZoomOut_ZoomsToPrevZoom" };
			yield return new TestCaseData(
				async (IPage page) =>
				{
					var zoomOut = page.GetByTitle("Zoom out from the content (Ctrl+-)");
					await zoomOut.ClickAsync();
					var oneHundred = page.GetByTitle("Scale the content to 100%");
					await oneHundred.ClickAsync();
				},
				1, 0.9, "90%"
			)
			{ TestName = "JumpBackToPrevZoom_AfterOneHundredPercent_ZoomsToPrevZoom" };
			yield return new TestCaseData(
			async (IPage page) =>
			{
				var slider = page.GetByRole(AriaRole.Slider);
				await slider.ClickAsync();
				await slider.PressAsync("ArrowLeft");
			},
			0.99, 1, "100%"
)
			{ TestName = "JumpBackToPrevZoom_AfterChangeSlider_ZoomsToPrevZoom" };
			yield return new TestCaseData(
				async (IPage page) =>
				{
					var fit = page.GetByTitle("Fit all nodes to the view-port");
					await fit.ClickAsync();
				},
				1, 1, "100%" // update these after fit nodes implemented
			)
			{ TestName = "JumpBackToPrevZoom_AfterFitNodes_ZoomsToPrevZoom" };
			yield return new TestCaseData(
				async (IPage page) =>
				{
					var fill = page.GetByTitle("Fit the entire content area to the view-port");
					await fill.ClickAsync();
				},
				1, 1, "100%" // update these after fill implemented
			)
			{ TestName = "JumpBackToPrevZoom_AfterFill_ZoomsToPrevZoom" };
		}
	}

	[TestCaseSource(nameof(JumpBackToPrevZoomTestCaseData)), WithTransaction, WithPlaywrightPage]
	public async Task TestJumpBackToPrevZoomAsync(Func<IPage, Task> adjustZoom, double scaleBeforeJumpBack, double scaleAfterJumpBack, string expectedDisplayedScale)
	{
		NetworkUserControl? networkUserControl = null;

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };

			var ribbonDataProvider = new Mock<IRibbonDataProvider>();
			ribbonDataProvider
				.Setup(n => n.GetRibbonViewModel(It.IsAny<NetworkViewModel>(), It.IsAny<NetworkUserControl>()))
				.Returns(() =>
				{
					var ribbonViewModel = new DefaultNetworkRibbonViewModel(new NetworkViewModel(network), networkUserControl);

					return ribbonViewModel;
				});

			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { BackColor = System.Drawing.Color.Red, SupportedActions = NetworkActions.StyleDiagram };
			networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher, ribbonDataProvider: ribbonDataProvider.Object);
			networkUserControl.SetDataContext(network, false);

			return networkUserControl;
		});

		await page.SetViewportSizeAsync(1250, 700);
		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		await adjustZoom(page);

		Assert.That(() => networkUserControl!.ViewModel.ContentScale, Is.EqualTo(scaleBeforeJumpBack).Within(0.0005).After(3000, 100));

		var viewTab = page.GetByText("View");
		await viewTab.ClickAsync();
		var previousZoom = page.GetByTitle("Return to the previous zoom level");
		await previousZoom.ClickAsync();

		Assert.That(() => networkUserControl!.ViewModel.ContentScale, Is.EqualTo(scaleAfterJumpBack).Within(0.0005).After(3000, 100));

		Assert.That(async () => await page.GetByTitle("Content Scale").TextContentAsync(), Is.EqualTo(expectedDisplayedScale).After(3000, 100));
	}

	[WithPlaywrightPage]
	[TestCase(100, 150, 150, 70, new[] { 0, 18, 0.935 })]
	[TestCase(1000, 1500, 1500, 700, new[] { 126, 170, 0.262 })]
	public async Task TestFitWithTwoNodesAsync(double x1, double y1, double x2, double y2, double[] expected)
	{
		var expectedOffsetX = expected[0];
		var expectedOffsetY = expected[1];
		var expectedScale = expected[2];

		var entity1 = Stub.Entity(e =>
		{
			e.Name = "Node 1";
			e.Width = 100;
			e.Height = 100;
			e.X = x1;
			e.Y = y1;
		});

		var entity2 = Stub.Entity(e =>
		{
			e.Name = "Node 2";
			e.Width = 100;
			e.Height = 100;
			e.X = x2;
			e.Y = y2;
		});

		var network = new NetworkBuilder().WithEntities(entity1, entity2).Build();

		NetworkUserControl? networkUserControl = null;

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			return networkUserControl;
		});

		var diagramControl = networkUserControl!.MainDiagramControl;

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		Assert.Multiple(() =>
		{
			Assert.That(() => diagramControl.ContentOffsetY, Is.EqualTo(0).After(3000, 100));
			Assert.That(() => diagramControl.ContentOffsetX, Is.EqualTo(0).After(3000, 100));
			Assert.That(() => networkUserControl!.ViewModel.ContentScale, Is.EqualTo(1).After(3000, 100));
		});

		await page.GetByRole(AriaRole.Button, new() { Name = "Fit" }).ClickAsync();

		Assert.Multiple(() =>
		{
			Assert.That(() => diagramControl.ContentOffsetY, Is.EqualTo(expectedOffsetY).Within(20d).After(3000, 100));
			Assert.That(() => diagramControl.ContentOffsetX, Is.EqualTo(expectedOffsetX).Within(30d).After(3000, 100));
			Assert.That(() => networkUserControl!.ViewModel.ContentScale, Is.EqualTo(expectedScale).Within(0.001).After(3000, 100));
		});
	}

	[WithPlaywrightPage]
	[TestCase(100, 50, new[] { 8, 0, 1.31 })]
	[TestCase(1500, 1000, new[] { 1842, 1244, 1.31 })]
	public async Task TestFitWithOneNodeAsync(double x, double y, double[] expected)
	{
		var expectedCurrentX = expected[0];
		var expectedCurrentY = expected[1];
		var expectedScale = expected[2];

		var entity = Stub.Entity(e =>
		{
			e.Name = "Node 1";
			e.Width = 100;
			e.Height = 100;
			e.X = x;
			e.Y = y;
		});

		var network = new NetworkBuilder().WithEntities(entity).Build();

		NetworkUserControl? networkUserControl = null;

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			return networkUserControl;
		});

		var diagramControl = networkUserControl!.MainDiagramControl;

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		Assert.Multiple(() =>
		{
			Assert.That(() => diagramControl.ContentOffsetY, Is.EqualTo(0).After(3000, 100));
			Assert.That(() => diagramControl.ContentOffsetX, Is.EqualTo(0).After(3000, 100));
			Assert.That(() => networkUserControl!.ViewModel.ContentScale, Is.EqualTo(1).After(3000, 100));
		});

		await page.GetByRole(AriaRole.Button, new() { Name = "Fit" }).ClickAsync();

		Assert.Multiple(() =>
		{
			Assert.That(() => diagramControl.ContentOffsetX, Is.EqualTo(expectedCurrentX).Within(30d).After(3000, 100));
			Assert.That(() => diagramControl.ContentOffsetY, Is.EqualTo(expectedCurrentY).Within(30d).After(3000, 100));
			Assert.That(() => networkUserControl!.ViewModel.ContentScale, Is.EqualTo(expectedScale).Within(0.001).After(3000, 100));
		});
	}

	[WithPlaywrightPage]
	[TestCase(100, 50, 150, 70, new[] { 34, 0, 1.151 })]
	[TestCase(1000, 1500, 1500, 700, new[] { 126, 170, 0.262 })]
	public async Task TestFillWithTwoNodesAsync(double x1, double y1, double x2, double y2, double[] expected)
	{
		var expectedOffsetX = expected[0];
		var expectedOffsetY = expected[1];
		var expectedScale = expected[2];

		var entity1 = Stub.Entity(e =>
		{
			e.Name = "Node 1";
			e.Width = 100;
			e.Height = 100;
			e.X = x1;
			e.Y = y1;
		});

		var entity2 = Stub.Entity(e =>
		{
			e.Name = "Node 2";
			e.Width = 100;
			e.Height = 100;
			e.X = x2;
			e.Y = y2;
		});

		var network = new NetworkBuilder().WithEntities(entity1, entity2).Build();

		NetworkUserControl? networkUserControl = null;

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			return networkUserControl;
		});

		var diagramControl = networkUserControl!.MainDiagramControl;

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		Assert.Multiple(() =>
		{
			Assert.That(() => diagramControl.ContentOffsetX, Is.EqualTo(0).After(3000, 100));
			Assert.That(() => diagramControl.ContentOffsetY, Is.EqualTo(0).After(3000, 100));
			Assert.That(() => networkUserControl!.ViewModel.ContentScale, Is.EqualTo(1).Within(0.001).After(3000, 100));
		});

		await page.GetByRole(AriaRole.Button, new() { Name = "Fill" }).ClickAsync();

		Assert.Multiple(() =>
		{
			Assert.That(() => diagramControl.ContentOffsetX, Is.EqualTo(expectedOffsetX).Within(30d).After(3000, 100));
			Assert.That(() => diagramControl.ContentOffsetY, Is.EqualTo(expectedOffsetY).Within(30d).After(3000, 100));
			Assert.That(() => networkUserControl!.ViewModel.ContentScale, Is.EqualTo(expectedScale).Within(0.001).After(3000, 100));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task ScrollTo_WhenNonScheduledSectionIsVisible_ShouldSyncVerticalScrollAsync()
	{
		var diagramEntity = Stub.Entity(e =>
		{
			e.IsDiagramScaled = true;
			e.ShouldShowNonScheduledSection = true;
		});

		var entities = new[] {
			Stub.Entity(e =>
			{
				e.Width = 150;
				e.Height = 100;
				e.X = 10;
				e.Y = 20;
			}),
			Stub.Entity(e =>
			{
				e.Width = 150;
				e.Height = 100;
				e.X = 1000;
				e.Y = 1200;
			})
		};

		var network = new NetworkBuilder().WithEntities(entities).WithDiagramEntity(diagramEntity).Build();

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new NetworkUserControlBuilder().WithNetwork(network).Build());

		var diagramLocator = page.Locator(".diagramareausercontrol");
		var diagramOne = diagramLocator.Nth(0);
		var diagramTwo = diagramLocator.Nth(1);

		await Assert.MultipleAsync(async () =>
		{
			Assert.That(await diagramOne.EvaluateAsync<double[]>("(e) => [e.scrollLeft, e.scrollTop]"), Is.EqualTo(new[] { 0, 0 }));
			Assert.That(await diagramTwo.EvaluateAsync<double[]>("(e) => [e.scrollLeft, e.scrollTop]"), Is.EqualTo(new[] { 0, 0 }));
		});

		await diagramOne.EvaluateAsync("(e) => e.scrollTo(100, 200)");

		Assert.Multiple(() =>
		{
			Assert.That(() => diagramOne.EvaluateAsync<double[]>("(e) => [e.scrollLeft, e.scrollTop]"), Is.EqualTo(new[] { 100, 200 }).After(3000, 50));
			Assert.That(() => diagramTwo.EvaluateAsync<double[]>("(e) => [e.scrollLeft, e.scrollTop]"), Is.EqualTo(new[] { 0, 200 }).After(3000, 50));
		});

		await diagramTwo.EvaluateAsync("(e) => e.scrollTo(0, 150)");

		Assert.Multiple(() =>
		{
			Assert.That(() => diagramOne.EvaluateAsync<double[]>("(e) => [e.scrollLeft, e.scrollTop]"), Is.EqualTo(new[] { 100, 150 }).After(3000, 50));
			Assert.That(() => diagramTwo.EvaluateAsync<double[]>("(e) => [e.scrollLeft, e.scrollTop]"), Is.EqualTo(new[] { 0, 150 }).After(3000, 50));
		});
	}

	[WithPlaywrightPage]
	[TestCase(100, 50, new[] { 8, 0, 1.31 })]
	[TestCase(1500, 1000, new[] { 1842, 1244, 1.31 })]
	public async Task TestFillWithOneNodeAsync(double x, double y, double[] expected)
	{
		var expectedOffsetX = expected[0];
		var expectedOffsetY = expected[1];
		var expectedScale = expected[2];

		var entity = Stub.Entity(e =>
		{
			e.Name = "Node 1";
			e.Width = 100;
			e.Height = 100;
			e.X = x;
			e.Y = y;
		});

		var network = new NetworkBuilder().WithEntities(entity).Build();

		NetworkUserControl? networkUserControl = null;

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			return networkUserControl;
		});

		var diagramControl = networkUserControl!.MainDiagramControl;

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		Assert.Multiple(() =>
		{
			Assert.That(() => diagramControl.ContentOffsetX, Is.EqualTo(0).After(3000, 100));
			Assert.That(() => diagramControl.ContentOffsetY, Is.EqualTo(0).After(3000, 100));
			Assert.That(() => networkUserControl!.ViewModel.ContentScale, Is.EqualTo(1).Within(0.001).After(3000, 100));
		});

		await page.GetByRole(AriaRole.Button, new() { Name = "Fill" }).ClickAsync();

		Assert.Multiple(() =>
		{
			Assert.That(() => diagramControl.ContentOffsetX, Is.EqualTo(expectedOffsetX).Within(30d).After(3000, 100));
			Assert.That(() => diagramControl.ContentOffsetY, Is.EqualTo(expectedOffsetY).Within(30d).After(3000, 100));
			Assert.That(() => networkUserControl!.ViewModel.ContentScale, Is.EqualTo(expectedScale).Within(0.001).After(3000, 100));
		});
	}

	[Test]
	public async Task ZoomSliderSavesPrevZoomAsync()
	{
		NetworkUserControl? control = null;
		using var ctx = new EnterpriseTestContext();

		var cut = await ctx.RenderControlOnFormAsync(() =>
		{
			var entity = new Entity();
			var diagramEntity = new Entity();
			var entities = new ImpObservableSet<INetworkEntity>(new[] { entity });
			var network = Mock.Of<INetwork>(x => x.Entities == entities && x.DiagramEntity == diagramEntity && x.EntityPositionStrategy == new EntityPositionStrategy());
			control = new NetworkUserControl(diagramEntity, Mock.Of<INetworkRefresher>());
			control.SetDataContext(network, false);
			return control;
		});

		await control!.InvokeWinzorDispatcherAsync(() => control.ViewModel.ContentScale = 0.5);
		var input = cut.Find(".controlbar__zoomslider");
		await input.PointerDownAsync(new Microsoft.AspNetCore.Components.Web.PointerEventArgs());
		await input.InputAsync(new ChangeEventArgs() { Value = "125" });

		double scale = 0;
		await control!.InvokeWinzorDispatcherAsync(() => scale = control.ViewModel.ContentScale);
		Assert.That(() => scale, Is.EqualTo(1.25).Within(0.0005));
		await control.InvokeWinzorDispatcherAsync(() => control.JumpBackToPrevZoom());
		await control.InvokeWinzorDispatcherAsync(() => scale = control.ViewModel.ContentScale);
		Assert.That(scale, Is.EqualTo(0.5));
	}

	[Test]
	public async Task ControlBarHasCorrectNetworkActionsAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var controlBar = new Mock<ControlBar>() { CallBase = true };
		ctx.ComponentFactories.Add(controlBar.Object);

		await ctx.RenderControlOnFormAsync(() => CreateAndSetup());

		Assert.That(controlBar.Object.RefreshAction?.GetName().ToString(), Is.EqualTo("Refresh"));
		Assert.That(controlBar.Object.PopoutAction?.GetName().ToString(), Is.EqualTo("Pop Out"));
		Assert.That(controlBar.Object.FitAction?.GetName().ToString(), Is.EqualTo("Fit"));
		Assert.That(controlBar.Object.FillAction?.GetName().ToString(), Is.EqualTo("Fill"));
		Assert.That(controlBar.Object.OneHundredPercentAction?.GetName().ToString(), Is.EqualTo("100%"));
		Assert.That(controlBar.Object.ZoomOutAction?.GetName().ToString(), Is.EqualTo("-"));
		Assert.That(controlBar.Object.ZoomInAction?.GetName().ToString(), Is.EqualTo("+"));
	}

	[Test]
	public async Task ControlBarZoomIsBoundToViewModelAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var controlBar = new Mock<ControlBar>() { CallBase = true };
		ctx.ComponentFactories.Add(controlBar.Object);

		var rendered = await ctx.RenderControlOnFormAsync(() => CreateAndSetup());
		var networkUserControl = rendered?.GetControl<NetworkUserControl>()!;

		Assert.That(networkUserControl.ViewModel.ContentScale, Is.EqualTo(1));
		Assert.That(controlBar.Object.Zoom, Is.EqualTo(100));

		await controlBar.Object.ZoomChanged.InvokeAsync(200);
		Assert.That(networkUserControl.ViewModel.ContentScale, Is.EqualTo(2));

		await networkUserControl.InvokeWinzorDispatcherAsync(() => networkUserControl.ViewModel.ContentScale = 0.5);
		Assert.That(networkUserControl.ViewModel.ContentScale, Is.EqualTo(0.5));
		Assert.That(controlBar.Object.Zoom, Is.EqualTo(50));
	}

	[Test]
	public async Task SelectingNodeInNonScheduledSectionUnselectsAllNodesInScheduledSectionAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var network = new DummyNetwork();

		var diagramMock = network.DiagramEntity;
		diagramMock.ShouldShowNonScheduledSection = true;

		var scheduledEntity1 = new Entity() { IsNonScheduled = false };
		var scheduledEntity2 = new Entity() { IsNonScheduled = false };

		var nonScheduledEntity1 = new Entity() { IsNonScheduled = true };
		var nonScheduledEntity2 = new Entity() { IsNonScheduled = true };
		network.Entities.Add(scheduledEntity1);
		network.Entities.Add(scheduledEntity2);
		network.Entities.Add(nonScheduledEntity1);
		network.Entities.Add(nonScheduledEntity2);

		NetworkUserControl? networkUserControl = null!;

		var cut = await ctx.RenderControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControl(diagramMock, new NetworkRefresher());
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var diagrams = cut.FindComponents<DiagramCanvas>();

		var scheduledDiagram = diagrams[0].Instance.BlazorDiagram;
		var nonScheduledDiagram = diagrams[1].Instance.BlazorDiagram;

		scheduledDiagram.SelectModel(scheduledDiagram.Nodes[0], false);
		scheduledDiagram.SelectModel(scheduledDiagram.Nodes[1], false);

		Assert.That(() => networkUserControl.MainDiagramControl.NetworkViewModel.SelectedNodes.Count(), Is.EqualTo(2).After(2000, 100));
		Assert.That(networkUserControl.MainDiagramControl.NetworkViewModel.SelectedNodes.ElementAt(0).Entity, Is.EqualTo(scheduledEntity1));
		Assert.That(networkUserControl.MainDiagramControl.NetworkViewModel.SelectedNodes.ElementAt(1).Entity, Is.EqualTo(scheduledEntity2));

		nonScheduledDiagram.SelectModel(nonScheduledDiagram.Nodes[0], false);

		Assert.That(() => networkUserControl.MainDiagramControl.NetworkViewModel.SelectedNodes.Count(), Is.EqualTo(1).After(2000, 100));
		Assert.That(networkUserControl.MainDiagramControl.NetworkViewModel.SelectedNodes.ElementAt(0).Entity, Is.EqualTo(nonScheduledEntity1));

		cut.Dispose();
	}

	[Test]
	public async Task SelectingNodeInScheduledSectionUnselectsAllNodesInNonScheduledSectionAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var network = new DummyNetwork();

		var diagramMock = network.DiagramEntity;
		diagramMock.ShouldShowNonScheduledSection = true;

		var scheduledEntity1 = new Entity() { IsNonScheduled = false };
		var scheduledEntity2 = new Entity() { IsNonScheduled = false };

		var nonScheduledEntity1 = new Entity() { IsNonScheduled = true };
		var nonScheduledEntity2 = new Entity() { IsNonScheduled = true };
		network.Entities.Add(scheduledEntity1);
		network.Entities.Add(scheduledEntity2);
		network.Entities.Add(nonScheduledEntity1);
		network.Entities.Add(nonScheduledEntity2);

		NetworkUserControl? networkUserControl = null!;

		var cut = await ctx.RenderControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControl(diagramMock, new NetworkRefresher());
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var diagrams = cut.FindComponents<DiagramCanvas>();

		var scheduledDiagram = diagrams[0].Instance.BlazorDiagram;
		var nonScheduledDiagram = diagrams[1].Instance.BlazorDiagram;

		nonScheduledDiagram.SelectModel(nonScheduledDiagram.Nodes[0], false);
		nonScheduledDiagram.SelectModel(nonScheduledDiagram.Nodes[1], false);

		Assert.That(() => networkUserControl.MainDiagramControl.NetworkViewModel.SelectedNodes.Count(), Is.EqualTo(2).After(2000, 100));
		Assert.That(networkUserControl.MainDiagramControl.NetworkViewModel.SelectedNodes.ElementAt(0).Entity, Is.EqualTo(nonScheduledEntity1));
		Assert.That(networkUserControl.MainDiagramControl.NetworkViewModel.SelectedNodes.ElementAt(1).Entity, Is.EqualTo(nonScheduledEntity2));

		scheduledDiagram.SelectModel(scheduledDiagram.Nodes[0], false);

		Assert.That(() => networkUserControl.MainDiagramControl.NetworkViewModel.SelectedNodes.Count(), Is.EqualTo(1).After(2000, 100));
		Assert.That(networkUserControl.MainDiagramControl.NetworkViewModel.SelectedNodes.ElementAt(0).Entity, Is.EqualTo(scheduledEntity1));

		cut.Dispose();
	}

	[WithPlaywrightPage]
	[TestCase(true, 1396d, 1000d)]
	[TestCase(false, 1384d, 1000d)]
	public async Task ScrollsToEntityOnFocusInvokedAsync(bool shouldShowNonScheduledSection, double expectedOffsetX, double expectedOffsetY)
	{
		var diagramEntity = Stub.Entity(e => e.ShouldShowNonScheduledSection = shouldShowNonScheduledSection);

		var entity1 = Stub.Entity(e =>
		{
			e.Name = "Node 1";
			e.Width = 100;
			e.Height = 100;
			e.X = 100;
			e.Y = 50;
		});

		var entity2 = Stub.Entity(e =>
		{
			e.Name = "Node 2";
			e.Width = 100;
			e.Height = 100;
			e.X = 1550;
			e.Y = 1000;
		});

		var network = new NetworkBuilder().WithEntities(entity1, entity2).WithDiagramEntity(diagramEntity).Build();

		NetworkUserControl? networkUserControl = null;

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.NetworkViewModel!.SelectSingleEntity(entity2, shouldFocusOnSelection: true);
			return networkUserControl;
		});

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		var diagramControl = networkUserControl!.MainDiagramControl;
		Assert.Multiple(() =>
		{
			Assert.That(() => diagramControl.Nodes.Where(n => n.IsSelected).Select(n => n.Name).First(), Is.EqualTo(entity2.Name));
			Assert.That(() => diagramControl.ContentOffsetX, Is.EqualTo(expectedOffsetX).Within(30d).After(3000, 100));
			Assert.That(() => diagramControl.ContentOffsetY, Is.EqualTo(expectedOffsetY).Within(30d).After(3000, 100));
			Assert.That(() => networkUserControl!.ViewModel.ContentScale, Is.EqualTo(1d).After(3000, 100));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task ScrollToEntityOnSelectEntityAsync()
	{
		var entity1 = Stub.Entity(e =>
		{
			e.Name = "Node 1";
			e.Width = 100;
			e.Height = 100;
			e.X = 100;
			e.Y = 50;
		});

		var entity2 = Stub.Entity(e =>
		{
			e.Name = "Node 2";
			e.Width = 100;
			e.Height = 100;
			e.X = 1500;
			e.Y = 1000;
		});

		var network = new NetworkBuilder().WithEntities(entity1, entity2).Build();

		NetworkUserControl? networkUserControl = null;

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.MainDiagramControl.SelectAndScrollToEntity(entity2);
			return networkUserControl;
		});

		var diagramControl = networkUserControl!.MainDiagramControl;

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		Assert.Multiple(() =>
		{
			Assert.That(() => diagramControl.Nodes.Where(n => n.IsSelected).Select(n => n.Name).First(), Is.EqualTo(entity2.Name));
			Assert.That(() => diagramControl.ContentOffsetX, Is.EqualTo(1346d).Within(30d).After(3000, 100));
			Assert.That(() => diagramControl.ContentOffsetY, Is.EqualTo(1000d).Within(30d).After(3000, 100));
			Assert.That(() => networkUserControl!.ViewModel.ContentScale, Is.EqualTo(1d).After(3000, 100));
		});
	}

	[Test]
	public async Task SpellCheckIsDisabledAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new NetworkUserControlBuilder().Build());

		var networkUserControl = rendered.Find(".networkusercontrol");
		Assert.That(networkUserControl.GetAttribute("spellcheck"), Is.EqualTo("false"));
	}

	[Test]
	public async Task TestNonScheduledSectionVisibilityChangeAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var diagramObject = new EntityForTest();
		diagramObject.IsDiagramScaled = true;
		diagramObject.ShouldShowNonScheduledSection = true;

		var cut = await ctx.RenderControlOnFormAsync(() =>
		{
			var network = new DummyNetwork();
			network.DiagramEntity = diagramObject;
			var networkUserControl = new NetworkUserControl(diagramObject, new NetworkRefresher());
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		Assert.That(cut.FindAll(".networkusercontrol__nonscheduleditems"), Is.Not.Empty);

		diagramObject.ShouldShowNonScheduledSection = false;
		diagramObject.TriggerPropertyChanged(nameof(Entity.ShouldShowNonScheduledSection));
		cut.Render();

		Assert.That(cut.Find(".networkusercontrol__nonscheduleditems").GetAttribute("style"), Is.EqualTo("width: 0px"));
	}

	[Test]
	public async Task TestNonScheduledSectionVisibileAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var diagramMock = new Mock<IDiagramEntity>();

		diagramMock.SetupGet(d => d.IsDiagramScaled).Returns(true);
		diagramMock.SetupGet(d => d.ScaleUnitPixelSize).Returns(100);
		diagramMock.Setup(d => d.ShouldShowNonScheduledSection).Returns(true);
		diagramMock.Setup(d => d.HiddenEntities).Returns(new ImpObservableCollection<IProposedNetworkEntity>());
		diagramMock.Setup(d => d.HiddenRelationships).Returns(new ImpObservableCollection<IEntityRelationship>());

		var cut = await ctx.RenderControlOnFormAsync(() =>
		{
			var network = new DummyNetwork();
			network.DiagramEntity = diagramMock.Object;
			var networkUserControl = new NetworkUserControl(diagramMock.Object, new NetworkRefresher());
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		Assert.That(cut.FindAll(".networkusercontrol__nonscheduleditems"), Is.Not.Empty);
	}

	[Test]
	public async Task TestNonScheduledSectionNotVisibileAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var diagramMock = new Mock<IDiagramEntity>();

		diagramMock.SetupGet(d => d.IsDiagramScaled).Returns(true);
		diagramMock.SetupGet(d => d.ScaleUnitPixelSize).Returns(100);
		diagramMock.Setup(d => d.ShouldShowNonScheduledSection).Returns(false);
		diagramMock.Setup(d => d.HiddenEntities).Returns(new ImpObservableCollection<IProposedNetworkEntity>());
		diagramMock.Setup(d => d.HiddenRelationships).Returns(new ImpObservableCollection<IEntityRelationship>());

		var cut = await ctx.RenderControlOnFormAsync(() =>
		{
			var network = new DummyNetwork();
			network.DiagramEntity = diagramMock.Object;
			var networkUserControl = new NetworkUserControl(diagramMock.Object, new NetworkRefresher());
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		Assert.That(cut.FindAll(".networkusercontrol__nonscheduleditems"), Is.Empty);
	}

	[Test]
	public async Task FocusOnSearchBoxIsCalledAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var refresher = new NetworkRefresher();
		var network = new DummyNetwork { Refresher = refresher };
		var viewModel = new RibbonViewModel();
		NetworkUserControl control = null!;

		var mockRibbon = new Mock<Ribbon>();
		mockRibbon.Setup(m => m.FocusOnSearchBox());
		ctx.ComponentFactories.Add(mockRibbon.Object);

		var mockRibbonDataProvider = new Mock<IRibbonDataProvider>();
		mockRibbonDataProvider.Setup(m => m.GetRibbonViewModel(It.IsAny<NetworkViewModel>(), It.IsAny<NetworkUserControl>())).Returns(viewModel);

		var rendered = await ctx.RenderControlOnFormAsync(() => control = new NetworkUserControlBuilder()
																				.WithNetwork(network)
																				.WithRibbonDataProvider(mockRibbonDataProvider.Object).Build());

		control.SwitсhFocusToFinder();

		mockRibbon.Verify(m => m.FocusOnSearchBox(), Times.Once());
	}

	[Test, WithPlaywrightPage]
	public async Task ZIndexStackIsMaintainedCorrectlyAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var channel = new DummyChannel("Top", 500);
		var diagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsDiagramScaled = true, DiagramChannels = new[] { channel }, ShouldShowNonScheduledSection = false, ShapeInspectorVisible = true };
		var network = new NetworkBuilder().WithDiagramEntity(diagramEntity).Build();
		var page = await ctx.LoadControlOnFormAsync(() => new NetworkUserControlBuilder().WithNetwork(network).Build());

		var diagramCanvas = page.Locator(".diagramareausercontrol .diagramnode__diagramarea").First;
		var channelDividers = page.Locator(".diagramareausercontrol .diagramnode__channeldividers").First;
		var diagramBackgrounds = page.Locator(".diagramareausercontrol .diagramnode__backgrounds").First;
		var shapeInspector = page.Locator(".shapeinspector").First;

		//The diagram backgrounds and channel dividers should stay behind the diagram canvas, while the shape inspector should be above it.
		Assert.That(async () => int.Parse((await diagramBackgrounds.GetComputedStyleAsync("z-index")).Raw), Is.EqualTo(0));
		Assert.That(async () => int.Parse((await channelDividers.GetComputedStyleAsync("z-index")).Raw), Is.EqualTo(1));
		Assert.That(async () => int.Parse((await diagramCanvas.GetComputedStyleAsync("z-index")).Raw), Is.EqualTo(2));
		Assert.That(async () => int.Parse((await shapeInspector.GetComputedStyleAsync("z-index")).Raw), Is.EqualTo(3));
	}

	[Test]
	public async Task ZoomCanBeChangedAfterRefreshAsync()
	{
		var entities = new[] {
			Stub.Entity(e => { e.IsNonScheduled = false; })
		};

		NetworkUserControl? control = null;
		using var ctx = new EnterpriseTestContext();
		var cut = await ctx.RenderControlOnFormAsync(() => control = new NetworkUserControlBuilder().Build());
		var canvas = cut.FindComponent<DiagramCanvas>();

		await cut.Find("[title='Refreshes the diagram']").ClickAsync(new WebMouseEventArgs());
		await control!.InvokeWinzorDispatcherAsync(() => { control.ZoomOut(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.Not.EqualTo(1).Within(0.0005));
		await control.InvokeWinzorDispatcherAsync(() => { control.OneHundredPercent(); });
		Assert.That(canvas.Instance.BlazorDiagram.Zoom, Is.EqualTo(1).Within(0.0005));
	}

	[Test, WithPlaywrightPage]
	public async Task ZoomInChangesScrollAsync()
	{
		var diagramEntity = Stub.Entity();
		var network = new NetworkBuilder().WithDiagramEntity(diagramEntity).Build();
		NetworkUserControl? networkUserControl = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			return networkUserControl;
		});
		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		await page.SetViewportSizeAsync(1000, 500);
		await Task.Delay(1000);

		var prevScrollX = networkUserControl!.ViewModel.ContentOffsetX;
		var prevScrollY = networkUserControl!.ViewModel.ContentOffsetY;
		var zoomInBtn = page.GetByTitle("Zoom in on the content (Ctrl++)");
		await zoomInBtn.ClickAsync();
		await page.WaitForLoadStateAsync();
		await zoomInBtn.ClickAsync();
		await page.WaitForLoadStateAsync();

		Assert.Multiple(() =>
		{
			Assert.That(() => networkUserControl!.ViewModel.ContentOffsetX, Is.GreaterThan(prevScrollX).After(3000, 100));
			Assert.That(() => networkUserControl!.ViewModel.ContentOffsetY, Is.GreaterThan(prevScrollY).After(3000, 100));
		});
	}
}
