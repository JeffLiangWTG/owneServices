using System.Windows.Forms;
using Blazor.Diagrams.Components;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Extensions;
using Bunit.Extensions;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Components;
using CargoWise.NetworkVisualisation.GUI.Events.Scroll;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.Winzor.Architecture.Test;
using Enterprise.ZArchitecture.Core.Testing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Microsoft.Playwright;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using WinzorFramework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using DiagramPointerArgs = Blazor.Diagrams.Core.Events.PointerEventArgs;
using Location = CargoWise.NetworkVisualisation.Integration.Location;
using Point = Blazor.Diagrams.Core.Geometry.Point;

namespace NetworkVisualisation.GUI.Winzor.Test.Controls;

class DiagramAreaUserControlTest
{
	const float ScrollBarWidth = 17f;

	[Test]
	public async Task DiagramAreaUserControlRightClickOpensContextMenuAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity();

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var diagramArea = rendered.Find(".diagramareausercontrol");
		await diagramArea.ContextMenuAsync(new WebMouseEventArgs());

		ctx.MockCargoWiseClientServices.MenuDisplayer.Verify(m => m.SendShowMenuRequestAsync(
			It.IsAny<MenuInteropModel>(),
			It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
			It.IsAny<Func<MenuClosedResult, Task>>()), Times.Once);

		var menuInteropModel = ctx.MockCargoWiseClientServices.MenuDisplayer.Invocations.Single().Arguments.First() as MenuInteropModel;
		Assert.That(menuInteropModel!.MenuItems, Is.Not.Empty);
	}

	[Test]
	public async Task DiagramAreaUserControlRightClickReadsClipboardOpensContextMenuAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity();

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var diagramArea = rendered.Find(".diagramareausercontrol");
		await diagramArea.ContextMenuAsync(new WebMouseEventArgs());

		ctx.JSInterop.VerifyInvoke("clipboard.getDataObject", 1);
	}

	[Test]
	public async Task DiagramAreaUserControlDetailsinNCNAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram };

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var diagramName = rendered.FindAll(".diagramnode__header");
		Assert.That(diagramName[0].ChildElementCount, Is.EqualTo(3));
	}

	[Test]
	public async Task DiagramAreaUserControlDetailsinMiniNCNAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.None };

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var diagramName = rendered.FindAll(".diagramdetails__header");
		Assert.That(diagramName, Is.Empty);
	}

	[Test]
	public async Task DiagramAreaUserControlDiagramNameinMiniNCNAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.None };

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var diagramName = rendered.FindAll(".diagramnode__diagramname");
		Assert.That(diagramName, Is.Empty);
	}
	[Test]
	public async Task DiagramAreaUserControlDiagramNameinNCNAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram };

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var diagramName = rendered.FindAll(".diagramnode__diagramname");
		Assert.That(diagramName.Count, Is.EqualTo(1));
	}

	[Test]
	public async Task DiagramAreaUserControlJobNameinMiniNCNAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.None };

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var jobName = rendered.FindAll(".diagramnode__diagramnumber");
		Assert.That(jobName.Count, Is.EqualTo(0));
	}

	[Test]
	public async Task DiagramAreaUserControlJobNameinNCNAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram };

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var jobName = rendered.FindAll(".diagramnode__diagramnumber");
		Assert.That(jobName.Count, Is.EqualTo(1));
	}

	[Test]
	public async Task DiagramAreaUserControlCompletionCriteriainMiniNCNAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.None };
			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var completioncriteria = rendered.FindAll(".diagramnode__completioncriteria");
		Assert.That(completioncriteria.Count, Is.EqualTo(0));
	}

	[Test]
	public async Task DiagramAreaUserControlCompletionCriteriainNCNAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram };
			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var completioncriteria = rendered.FindAll(".diagramnode__completioncriteria");
		Assert.That(completioncriteria.Count, Is.EqualTo(1));
	}

	[Test]
	public async Task DiagramAreaUserControlChangeBackgroundColorinMiniNCNAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var network = new NetworkViewModel(new DummyNetwork());
		var entity = new Entity() { Height = 100, Width = 200 };

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { BackColor = System.Drawing.Color.Red, SupportedActions = NetworkActions.None };
			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);

			return networkUserControl;
		});

		var jobname = rendered.FindAll(".main__diagram");
		Assert.That(jobname, Is.Empty);
	}

	[Test]
	public async Task TestDiagramModelHasBeenDisposedAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		DiagramAreaUserControl diagramControl = null!;
		Form testForm = null!;
		await ctx.InitializeFormAsync(() =>
		{
			var networkUserControl = new NetworkUserControlBuilder().Build();
			diagramControl = networkUserControl.MainDiagramControl;

			testForm = new Form { Height = 800, Width = 1000 };
			testForm.Controls.Add(networkUserControl);

			return testForm;
		});

		Assert.That(diagramControl?.IsDisposed, Is.False);
		Assert.That(diagramControl?.DiagramModel?.IsDisposed, Is.False);

		await ctx.WinzorDispatcher.InvokeAsync(() => testForm.Close());

		Assert.That(diagramControl?.IsDisposed, Is.True);
		Assert.That(diagramControl?.DiagramModel?.IsDisposed, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task DiagramAreaUserControlChangeBackgroundColorinNCNAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { BackColor = System.Drawing.Color.Red, SupportedActions = NetworkActions.StyleDiagram };
			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);

			return networkUserControl;
		});

		var diagramnode = page.Locator(".diagramareausercontrol__diagramnode");
		Assert.That(() => diagramnode.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(255, 0, 0)").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task DiagramAreaUserControlChangeForegroundColorNCNAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { ForeColor = System.Drawing.Color.Red, SupportedActions = NetworkActions.StyleDiagram, JobName = "Job Name", JobNumber = "12345", CompletionCriteria = "Completion_Criteria" };
			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);

			return networkUserControl;
		});

		var jobname = page.Locator(".diagramnode__jobname");
		var jobnumber = page.Locator(".diagramnode__diagramnumber");
		var completionCriteria = page.Locator(".diagramnode__completioncriteria");

		Assert.That(() => completionCriteria.GetComputedStyleAsync("color"), Is.EqualTo("rgb(255, 0, 0)").After(3000, 100));
		Assert.That(() => jobname.GetComputedStyleAsync("color"), Is.EqualTo("rgb(255, 0, 0)").After(3000, 100));
		Assert.That(() => jobnumber.GetComputedStyleAsync("color"), Is.EqualTo("rgb(255, 0, 0)").After(3000, 100));
		Assert.That(() => jobnumber.GetComputedStyleAsync("font-family"), Is.EqualTo("Tahoma").After(3000, 100));
	}

	[Test]
	public async Task DiagramAreaUserControlChangeForegroundColorinMiniNCNAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var network = new NetworkViewModel(new DummyNetwork());
		var entity = new Entity() { Height = 100, Width = 200 };

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { ForeColor = System.Drawing.Color.Red, SupportedActions = NetworkActions.None };
			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);

			return networkUserControl;
		});

		var jobname = rendered.FindAll(".diagramnode__jobname");
		var jobnumber = rendered.FindAll(".diagramnode__diagramnumber");

		Assert.That(jobname.Count, Is.EqualTo(0));
		Assert.That(jobnumber.Count, Is.EqualTo(0));
	}

	static async Task<Size> GetDifferenceInBoundsAsync(ILocator container, ILocator content)
	{
		var userControlBounds = (await container.BoundingBoxAsync())!;
		var userControlContentsBounds = (await content.BoundingBoxAsync())!;
		return new Size(
			userControlBounds.Width - userControlContentsBounds.Width,
			userControlBounds.Height - userControlContentsBounds.Height
		);
	}

	[Test, WithPlaywrightPage]
	public async Task DiagramAreaUserControlHasScrollBarsAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => TestHelpers.SetupNetworkUserControl());

		var diagramAreaUserControl = page.Locator(".diagramareausercontrol");
		Assert.That(() => diagramAreaUserControl.GetComputedStyleAsync("overflow-x"), Is.EqualTo("scroll").After(3000, 100));
		Assert.That(() => diagramAreaUserControl.GetComputedStyleAsync("overflow-y"), Is.EqualTo("scroll").After(3000, 100));
		Assert.That(diagramAreaUserControl.IsOverflowingAsync, Is.EqualTo(false).After(3000, 100));
		var content = page.Locator(".diagramareausercontrol__diagramnode");
		var differenceBetweenControlAndContent = await GetDifferenceInBoundsAsync(diagramAreaUserControl, content);
		Assert.That(differenceBetweenControlAndContent.Height, Is.GreaterThan(0));
		Assert.That(differenceBetweenControlAndContent.Width, Is.GreaterThan(0));
	}

	[Test, WithPlaywrightPage]
	public async Task DiagramAreaUserControlWhenClickToDeleteNodeShouldNotThrowAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		NetworkForTest network = null!;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var entities = new[] {
				Stub.Entity(e => {
					e.X = 500;
					e.Y = 500;
					e.Width = 300;
					e.Height = 200;
					e.CanDeleteUnderlyingEntity = true;
					e.JobName = "Test";
				})
			};

			var diagramEntity = Stub.DiagramEntity(supportDiagramVisualStyles: true, isDiagramScaled: false, showNonscheduledSection: false);
			network = new NetworkBuilder()
				.WithEntities(entities)
				.WithDiagramEntity(diagramEntity).Build();
			return new NetworkUserControlBuilder().WithNetwork(network).Build();
		});
		Assert.That(network.Entities.Count, Is.EqualTo(1));

		var diagramAreaUserControl = page.Locator(".diagramareausercontrol");
		Assert.That(diagramAreaUserControl.IsOverflowingAsync, Is.EqualTo(true).After(3000, 100));

		var content = page.Locator(".diagramareausercontrol__diagramnode");
		var differenceBetweenControlAndContent = await GetDifferenceInBoundsAsync(diagramAreaUserControl, content);
		Assert.That(differenceBetweenControlAndContent.Height, Is.LessThan(0));
		Assert.That(differenceBetweenControlAndContent.Width, Is.LessThan(0));

		var networkNode = await page.WaitForSelectorAsync(".networknode--indiagram");
		await networkNode!.HoverAsync();

		var deleteIcon = page.Locator(".deleteicon__deletecircle");
		await Assertions.Expect(deleteIcon).Not.ToBeInViewportAsync();

		Assert.DoesNotThrowAsync(async () =>
		{
			await deleteIcon.ClickAsync();
			await page.Locator(".networknode--indiagram").WaitForAsync(new() { State = WaitForSelectorState.Detached, Timeout = 3000 });
		});

		Assert.That(network.Entities.Count, Is.EqualTo(0));
	}

	[TestCaseSource(nameof(VisualStylesTestCaseSource)), WithPlaywrightPage]
	public async Task DiagramAreaUserControlContentSizeAppliedToDiagramAreaAsync(bool supportDiagramVisualStyles)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		NetworkUserControl? networkUserControl = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			networkUserControl = TestHelpers.SetupNetworkUserControl(supportDiagramVisualStyles: supportDiagramVisualStyles);
			networkUserControl.MainDiagramControl.ViewModel.ContentWidth = 1234;
			networkUserControl.MainDiagramControl.ViewModel.ContentHeight = 5678;
			return networkUserControl;
		});

		var diagramAreaUserControl = page.Locator(".diagramareausercontrol");
		Assert.That(() => diagramAreaUserControl.GetComputedStyleAsync("--diagram-content-min-width"), Is.EqualTo("1234px").After(3000, 100));
		Assert.That(() => diagramAreaUserControl.GetComputedStyleAsync("--diagram-content-min-height"), Is.EqualTo("5703px").After(3000, 100));

		var diagramArea = page.Locator(".diagramnode__diagramarea");
		Assert.That(() => diagramArea.GetComputedStyleAsync("min-width"), Is.EqualTo("1234px").After(3000, 100));
		Assert.That(() => diagramArea.GetComputedStyleAsync("min-height"), Is.EqualTo("5703px").After(3000, 100));

		await networkUserControl!.InvokeWinzorDispatcherAsync(() =>
		{
			networkUserControl.MainDiagramControl.ViewModel.ContentWidth = 4321;
			networkUserControl.MainDiagramControl.ViewModel.ContentHeight = 8765;
		});

		Assert.That(() => diagramAreaUserControl.GetComputedStyleAsync("--diagram-content-min-width"), Is.EqualTo("4321px").After(3000, 100));
		Assert.That(() => diagramAreaUserControl.GetComputedStyleAsync("--diagram-content-min-height"), Is.EqualTo("8790px").After(3000, 100));

		Assert.That(() => diagramArea.GetComputedStyleAsync("min-width"), Is.EqualTo("4321px").After(3000, 100));
		Assert.That(() => diagramArea.GetComputedStyleAsync("min-height"), Is.EqualTo("8790px").After(3000, 100));
	}

	[TestCaseSource(nameof(VisualStylesTestCaseSource)), WithPlaywrightPage]
	public async Task DiagramAreaUserControlDiagramAreaTakesFullScreenWhenBiggerThanContentSizeAsync(bool supportDiagramVisualStyles)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var networkUserControl = TestHelpers.SetupNetworkUserControl(supportDiagramVisualStyles: supportDiagramVisualStyles);
			networkUserControl.MainDiagramControl.ViewModel.ContentWidth = 0;
			networkUserControl.MainDiagramControl.ViewModel.ContentHeight = 0;
			return networkUserControl;
		});

		var diagramAdditionalWidth = ScrollBarWidth;
		var diagramAdditionalHeight = ScrollBarWidth;
		if (supportDiagramVisualStyles)
		{
			var borderWidth = 4;
			diagramAdditionalWidth += borderWidth;
			var diagramHeaderBounds = await page.Locator(".diagramnode__header").BoundingBoxAsync();
			var diagramDividerBounds = await page.Locator(".diagramnode__divider").BoundingBoxAsync();
			diagramAdditionalHeight += diagramHeaderBounds!.Height + diagramDividerBounds!.Height + borderWidth;
		}

		var diagramArea = page.Locator(".diagramnode__diagramarea");
		var diagramAreaUserControl = page.Locator(".diagramareausercontrol");
		Assert.That(() => diagramAreaUserControl.GetComputedStyleAsync("overflow"), Is.EqualTo("scroll"));

		var clientHeight = await diagramArea.EvaluateAsync<int>("x => x.clientHeight");
		var scrollHeight = await diagramArea.EvaluateAsync<int>("x => x.scrollHeight");
		Assert.That(clientHeight, Is.EqualTo(scrollHeight));
		Assert.That(clientHeight - scrollHeight, Is.EqualTo(0));
	}

	[TestCaseSource(nameof(VisualStylesTestCaseSource)), WithPlaywrightPage]
	public async Task DiagramAreaUserControlContentSizeUpdatesWithZoomAsync(bool supportDiagramVisualStyles)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		NetworkUserControl? networkUserControl = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			networkUserControl = TestHelpers.SetupNetworkUserControl(supportDiagramVisualStyles: supportDiagramVisualStyles);
			networkUserControl.MainDiagramControl.ViewModel.ContentWidth = 1000;
			networkUserControl.MainDiagramControl.ViewModel.ContentHeight = 1000;
			networkUserControl.ViewModel.ContentScale = 1;
			return networkUserControl;
		});

		var diagramAreaUserControl = page.Locator(".diagramareausercontrol");
		Assert.That(() => diagramAreaUserControl.GetComputedStyleAsync("--diagram-content-min-width"), Is.EqualTo("1000px").After(3000, 100));
		Assert.That(() => diagramAreaUserControl.GetComputedStyleAsync("--diagram-content-min-height"), Is.EqualTo("1025px").After(3000, 100));

		var diagramArea = page.Locator(".diagramnode__diagramarea");
		Assert.That(() => diagramArea.GetComputedStyleAsync("min-width"), Is.EqualTo("1000px").After(3000, 100));
		Assert.That(() => diagramArea.GetComputedStyleAsync("min-height"), Is.EqualTo("1025px").After(3000, 100));

		await networkUserControl!.InvokeWinzorDispatcherAsync(() =>
		{
			networkUserControl.ViewModel.ContentScale = 2;
		});

		Assert.That(() => diagramAreaUserControl.GetComputedStyleAsync("--diagram-content-min-width"), Is.EqualTo("2000px").After(3000, 100));
		Assert.That(() => diagramAreaUserControl.GetComputedStyleAsync("--diagram-content-min-height"), Is.EqualTo("2025px").After(3000, 100));

		Assert.That(() => diagramArea.GetComputedStyleAsync("min-width"), Is.EqualTo("2000px").After(3000, 100));
		Assert.That(() => diagramArea.GetComputedStyleAsync("min-height"), Is.EqualTo("2025px").After(3000, 100));
	}

	[Test]
	public async Task DiagramAreaUserControlContentSizeIsRecalculatedOnNodeMoveAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() => TestHelpers.SetupNetworkUserControl(new[] { new Entity { Width = 101, Height = 102, X = 100, Y = 100 } }));

		Assert.That(() => rendered.Find(".diagramareausercontrol").GetAttribute("style"), Does.Contain("--diagram-content-min-width: 201px;"));
		Assert.That(() => rendered.Find(".diagramareausercontrol").GetAttribute("style"), Does.Contain("--diagram-content-min-height: 227px;"));

		var jobNodeModel = rendered.FindComponent<JobNode>().Instance.Node!;
		await ctx.Renderer.Dispatcher.InvokeAsync(() => jobNodeModel.SetPosition(200, 200));
		jobNodeModel.TriggerMoved();

		Assert.That(() => rendered.Find(".diagramareausercontrol").GetAttribute("style"), Does.Contain("--diagram-content-min-width: 301px;").After(3000, 100));
		Assert.That(() => rendered.Find(".diagramareausercontrol").GetAttribute("style"), Does.Contain("--diagram-content-min-height: 327px;").After(3000, 100));
	}

	[Test]
	public async Task CheckForNodeSizeWithScaledDiagramMinHeightAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var node = Stub.Entity();
		node.Width = 300;
		node.Height = 700;
		node.X = 100;
		node.Y = 100;

		var network = new NetworkBuilder().WithEntities(node).Build();

		NetworkUserControl control = null!;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			control = new NetworkUserControlBuilder().WithNetwork(network).Build();
			return control;
		});

		Assert.That(() => rendered.Find(".diagramareausercontrol").GetAttribute("style"), Does.Contain("--diagram-content-min-width: 400px;"));
		Assert.That(() => rendered.Find(".diagramareausercontrol").GetAttribute("style"), Does.Contain("--diagram-content-min-height: 825px;"));
	}

	[Test]
	public async Task DiagramAreaUserControlContentSizeIsRecalculatedOnNodeResizeAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() => TestHelpers.SetupNetworkUserControl(new[] { new Entity { Width = 100, Height = 100, X = 101, Y = 102 } }));

		Assert.That(() => rendered.Find(".diagramareausercontrol").GetAttribute("style"), Does.Contain("--diagram-content-min-width: 201px;"));
		Assert.That(() => rendered.Find(".diagramareausercontrol").GetAttribute("style"), Does.Contain("--diagram-content-min-height: 227px;"));

		var jobNodeModel = rendered.FindComponent<JobNode>().Instance.Node!;
		await ctx.Renderer.Dispatcher.InvokeAsync(() => jobNodeModel.Size = new Size(200, 200));
		jobNodeModel.TriggerResized();

		Assert.That(() => rendered.Find(".diagramareausercontrol").GetAttribute("style"), Does.Contain("--diagram-content-min-width: 301px;").After(3000, 100));
		Assert.That(() => rendered.Find(".diagramareausercontrol").GetAttribute("style"), Does.Contain("--diagram-content-min-height: 327px;").After(3000, 100));
	}

	static IEnumerable<TestCaseData> VisualStylesTestCaseSource
	{
		get
		{
			yield return new TestCaseData(true) { TestName = "{m}_WithVisualStyles" };
			yield return new TestCaseData(false) { TestName = "{m}_WithoutVisualStyles" };
		}
	}

	[Test]
	public async Task DiagramNetworkUserControl_DiagramScroll_UpdatesViewModelAsync()
	{
		using var ctx = new EnterpriseTestContext();
		NetworkUserControl? networkUserControl = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var network = new NetworkBuilder().WithEntities(new EntityForTest()).Build();
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram };
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.ViewModel.ContentScale = 1;
			return networkUserControl;
		});

		Assert.That(networkUserControl, Is.Not.Null);
		var diagramAreaUserControl = rendered.Find(".diagramareausercontrol");

		await diagramAreaUserControl.TriggerEventAsync("onthrottledscroll", new ThrottledScrollEventArgs() { ScrollLeft = 123, ScrollTop = 456 });
		Assert.That(networkUserControl!.ViewModel.ContentOffsetX, Is.EqualTo(123));
		Assert.That(networkUserControl.ViewModel.ContentOffsetY, Is.EqualTo(456));

		await networkUserControl.InvokeWinzorDispatcherAsync(() => networkUserControl.ViewModel.ContentScale = 2);
		await diagramAreaUserControl.TriggerEventAsync("onthrottledscroll", new ThrottledScrollEventArgs() { ScrollLeft = 200, ScrollTop = 400 });
		Assert.That(networkUserControl.ViewModel.ContentOffsetX, Is.EqualTo(200));
		Assert.That(networkUserControl.ViewModel.ContentOffsetY, Is.EqualTo(400));

		await networkUserControl.InvokeWinzorDispatcherAsync(() => networkUserControl.ViewModel.ContentScale = 0.75);
		await diagramAreaUserControl.TriggerEventAsync("onthrottledscroll", new ThrottledScrollEventArgs() { ScrollLeft = 75, ScrollTop = 150 });
		Assert.That(networkUserControl.ViewModel.ContentOffsetX, Is.EqualTo(75));
		Assert.That(networkUserControl.ViewModel.ContentOffsetY, Is.EqualTo(150));
	}

	[Test]
	public async Task DiagramNetworkUserControl_ViewModelScroll_UpdatesDiagramAsync()
	{
		using var ctx = new EnterpriseTestContext();
		NetworkUserControl? networkUserControl = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			networkUserControl = TestHelpers.SetupNetworkUserControl();
			networkUserControl.ViewModel.ContentScale = 1;
			return networkUserControl;
		});

		Assert.That(networkUserControl, Is.Not.Null);
		var diagramAreaUserControl = rendered.Find(".diagramareausercontrol");
	}

	[Test]
	public async Task DiagramNetworkUserControl_DiagramContainerChanged_UpdatesViewModelAsync()
	{
		using var ctx = new EnterpriseTestContext();
		NetworkUserControl? networkUserControl = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			networkUserControl = TestHelpers.SetupNetworkUserControl();
			networkUserControl.ViewModel.ContentScale = 1;
			return networkUserControl;
		});

		Assert.That(networkUserControl, Is.Not.Null);
		var ncnDiagramModel = rendered.FindComponent<DiagramCanvas>()?.Instance.BlazorDiagram as NCNDiagramModel;
		Assert.That(ncnDiagramModel, Is.Not.Null);

		Assert.That(ncnDiagramModel!.Container, Is.Null);
		Assert.That(networkUserControl!.MainDiagramControl.ViewModel.ContentViewportWidth, Is.EqualTo(0));
		Assert.That(networkUserControl.MainDiagramControl.ViewModel.ContentViewportHeight, Is.EqualTo(0));

		ncnDiagramModel.SetContainer(new Rectangle(new Point(0, 0), new Size(123, 456)));
		Assert.That(ncnDiagramModel.Container!.Width, Is.EqualTo(123));
		Assert.That(ncnDiagramModel.Container.Height, Is.EqualTo(456));
		Assert.That(() => networkUserControl.MainDiagramControl.ViewModel.ContentViewportWidth, Is.EqualTo(123).After(3000, 100));
		Assert.That(() => networkUserControl.MainDiagramControl.ViewModel.ContentViewportHeight, Is.EqualTo(456).After(3000, 100));
	}

	[Test]
	public async Task TextAreasRenderedAsDynamicTextAreasAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() => TestHelpers.SetupNetworkUserControl());

		var textAreas = rendered.FindComponents<DynamicTextArea>();
		Assert.That(textAreas.Count, Is.EqualTo(3));

		Assert.That(textAreas[0].Find(".diagramnode__diagramname"), Is.Not.Null);
		Assert.That(textAreas[1].Find(".diagramnode__diagramnumber"), Is.Not.Null);
		Assert.That(textAreas[2].Find(".diagramnode__completioncriteria"), Is.Not.Null);
	}

	[Test, WithPlaywrightPage]
	public async Task DiagramAreaUserControlDiagramAreaShowsDeleteIconWhenTheShapeIsDraggedToHeaderAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => TestHelpers.SetupNetworkUserControl(new[] { new Entity { Width = 101, Height = 102, X = 0, Y = 0 } }));

		var networkNode = await page.WaitForSelectorAsync(".networknode--indiagram");
		await networkNode!.HoverAsync();

		var deleteIcon = page.Locator(".deleteicon__deletecircle");

		await Assertions.Expect(deleteIcon).ToBeInViewportAsync();

		var diagramnode = page.Locator(".diagramareausercontrol__diagramnode");
		Assert.That(() => diagramnode.GetComputedStyleAsync("overflow"), Is.EqualTo("clip"));
	}

	[Test, WithPlaywrightPage]
	public async Task DiagramAreaUserControlJobNameFontFamilyAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, JobName = "Job Name", JobNumber = "12345" };
			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);

			return networkUserControl;
		});

		var jobname = page.Locator(".diagramnode__jobname");
		var jobnumber = page.Locator(".diagramnode__diagramnumber");

		Assert.That(() => jobnumber.GetComputedStyleAsync("font-family"), Is.EqualTo("Tahoma").After(3000, 100));
		Assert.That(() => jobname.GetComputedStyleAsync("font-family"), Is.EqualTo("Tahoma").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task DiagramAreaUserControlNetworkNodeFontFamilyAsync()
	{
		NetworkUserControl? networkUserControl = null;
		var entity = new EntityForTest()
		{
			Width = 200,
			Height = 200,
			Notes = "aaaaaaaaaaaaaaaaaaaaaaaaa",
			CompletionCriteria = "aaaaaaaaaaaaaaaaaaaaaaaaa",
			Name = "aaaaaaaaaaaaaaaaaaaaaaaaa",
			HasLinkedEntity = true,
			JobName = "job name",
			JobNumber = "1234"
		};

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			network.Entities.Add(entity);

			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { BackColor = System.Drawing.Color.Red, SupportedActions = NetworkActions.StyleDiagram };
			networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);

			return networkUserControl;
		});

		var node = page.Locator(".networknode");
		Assert.That(() => node.GetComputedStyleAsync("font-family"), Is.EqualTo("Tahoma").After(3000, 100));
	}

	static IEnumerable<TestCaseData> ZoomAndMergedColumns_TestData
	{
		get
		{
			yield return new TestCaseData(0.9, 1);
			yield return new TestCaseData(0.7, 2);
			yield return new TestCaseData(0.4, 4);
			yield return new TestCaseData(0.2, 8);
		}
	}

	[Test, WithPlaywrightPage]
	public async Task ScaledDiagram_ShowsCorrectColumnsAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var networkUserControl = TestHelpers.SetupNetworkUserControl(new[] { new Entity { Width = 300, Height = 300, X = 0, Y = 0 } }, isDiagramScaled: true);
			networkUserControl.MainDiagramControl.ViewModel.ContentWidth = 540;
			networkUserControl.MainDiagramControl.ViewModel.ContentHeight = 1000;
			return networkUserControl;
		});

		var grid = page.Locator(".diagramareausercontrol_schedulegrid");
		IReadOnlyList<ILocator> boxes = null!;
		Assert.That(async () =>
		{
			boxes = await grid.Locator(".schedulegrid_box").AllAsync();
			return boxes.Count;
		}, Is.EqualTo(6).After(3000, 100));
		Assert.That(async () => (await boxes[0].BoundingBoxAsync())!.Width, Is.EqualTo(100).After(3000, 100));
		Assert.That(async () => (await boxes[1].BoundingBoxAsync())!.Width, Is.EqualTo(100).After(3000, 100));
		Assert.That(async () => (await boxes[2].BoundingBoxAsync())!.Width, Is.EqualTo(100).After(3000, 100));
		Assert.That(async () => (await boxes[3].BoundingBoxAsync())!.Width, Is.EqualTo(100).After(3000, 100));
		Assert.That(async () => (await boxes[4].BoundingBoxAsync())!.Width, Is.EqualTo(100).After(3000, 100));
		Assert.That(async () => (await boxes[5].BoundingBoxAsync())!.Width, Is.EqualTo(40).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task NonScaledDiagram_DoesNotShowColumnsAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var networkUserControl = TestHelpers.SetupNetworkUserControl(new[] { new Entity { Width = 300, Height = 300, X = 0, Y = 0 } }, isDiagramScaled: false);
			networkUserControl.MainDiagramControl.ViewModel.ContentViewportWidth = 500;
			networkUserControl.MainDiagramControl.ViewModel.ContentViewportHeight = 1000;
			return networkUserControl;
		});
		var grid = await page.Locator(".diagramareausercontrol_schedulegrid").AllAsync();
		Assert.That(grid.IsNullOrEmpty());
	}

	[Test]
	public async Task ScaledDiagram_ShowsCorrectColumnsAfterRefresh_WhenScaledAsync()
	{
		var refresher = new NetworkRefresher();
		var network = new NetworkBuilder().Build();
		var zoom = 2.0;
		refresher.AssociateWithNetwork(network);
		network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsDiagramScaled = true, ScaleUnitPixelSize = 100 };

		NetworkUserControl control = null!;
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			control = new NetworkUserControlBuilder().WithNetwork(network).Build();
			control.MainDiagramControl.ViewModel.ContentViewportWidth = 1200;
			control.MainDiagramControl.ViewModel.ContentViewportHeight = 500;
			control.ViewModel.ContentScale = zoom;
			return control;
		});

		Assert.That(() => rendered.FindAll(".timescale_label_text").Count, Is.EqualTo(6).After(3000, 100));
		Assert.That(() => rendered.FindAll(".schedulegrid_box").Count, Is.EqualTo(6).After(3000, 100));

		// Breaks after refreshing 2 times
		var refreshButton = rendered.Find("[title='Refreshes the diagram']");
		refreshButton.Click();
		refreshButton.Click();

		var scheduleGrid = rendered.Find(".diagramareausercontrol_schedulegrid");
		Assert.That(scheduleGrid.GetAttribute("style"), Does.Contain($"height: {100 / zoom}%;"));

		Assert.That(() => rendered.FindAll(".timescale_label_text").Count, Is.EqualTo(6).After(3000, 100));
		Assert.That(() => rendered.FindAll(".schedulegrid_box").Count, Is.EqualTo(6).After(3000, 100));
	}

	[TestCaseSource(nameof(ZoomAndMergedColumns_TestData))]
	public async Task ScaledDiagram_ShowsCorrectColumnsOnZoomOutAsync(double zoom, int numberOfMergedColumns)
	{
		var refresher = new NetworkRefresher();
		var network = new NetworkBuilder().Build();
		refresher.AssociateWithNetwork(network);
		network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsDiagramScaled = true, ScaleUnitPixelSize = 100 };

		NetworkUserControl control = null!;
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			control = new NetworkUserControlBuilder().WithNetwork(network).Build();
			control.MainDiagramControl.ViewModel.ContentWidth = 1200;
			control.MainDiagramControl.ViewModel.ContentHeight = 500;
			control.ViewModel.ContentScale = zoom;
			return control;
		});

		var scheduleGrid = rendered.Find(".diagramareausercontrol_schedulegrid");
		Assert.That(scheduleGrid.GetAttribute("style"), Does.Contain($"height: {100 / zoom}%;"));

		var scheduleGridScales = rendered.Find(".schedulegrid__scales");
		Assert.That(scheduleGridScales.GetAttribute("style"), Does.Contain($"grid-column-gap: {(numberOfMergedColumns - 1) * 100}px;"));
	}

	[TestCaseSource(nameof(ZoomAndMergedColumns_TestData))]
	public async Task ScaledDiagram_WithChannels_ShowsCorrectColumnsOnZoomOutAsync(double zoom, int numberOfMergedColumns)
	{
		var refresher = new NetworkRefresher();
		var network = new NetworkBuilder().Build();
		refresher.AssociateWithNetwork(network);
		var channel1 = new DummyChannel("Top", 500);
		var channel2 = new DummyChannel("Middle", 200);
		var channel3 = new DummyChannel("Bottom", 100);

		network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsDiagramScaled = true, DiagramChannels = new[] { channel1, channel2, channel3 }, ScaleUnitPixelSize = 100 };

		NetworkUserControl control = null!;
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			control = new NetworkUserControlBuilder().WithNetwork(network).Build();
			control.MainDiagramControl.ViewModel.ContentWidth = 1200;
			control.MainDiagramControl.ViewModel.ContentHeight = 500;
			control.ViewModel.ContentScale = zoom;
			return control;
		});

		var scheduleGrid = rendered.Find(".diagramareausercontrol_schedulegrid");
		Assert.That(scheduleGrid.GetAttribute("style"), Does.Contain($"height: {100 / zoom}%;"));

		var scheduleGridScales = rendered.Find(".schedulegrid__scales");
		Assert.That(scheduleGridScales.GetAttribute("style"), Does.Contain($"grid-column-gap: {(numberOfMergedColumns - 1) * 100}px;"));
		Assert.That(scheduleGridScales.GetAttribute("style"), Does.Contain($"padding-left: {(numberOfMergedColumns - 1) * 100}px;"));
	}

	[Test]
	public async Task DiagramAreaUserControlExecuteRibbonActionDoesNotThrowAsync([Values] bool isHandledByVisualiser)
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram };

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var networkAction = new Mock<INetworkAction>();
		networkAction.Setup(i => i.Execute()).Returns(new EntityForTest { IsHandledByVisualiser = isHandledByVisualiser });
		var diagramAreaUserControl = rendered.GetControl<NetworkUserControl>().MainDiagramControl;
		await diagramAreaUserControl.InvokeWinzorDispatcherAsync(() =>
		{
			Assert.That(() => diagramAreaUserControl.ExecuteRibbonAction(networkAction.Object), Throws.Nothing);
		});
	}

	[Test]
	public async Task DiagramAreaUserControlExecuteContextMenuActionDoesNotThrowAsync([Values] bool isHandledByVisualiser)
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram };

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var networkAction = new Mock<INetworkAction>();
		networkAction.Setup(i => i.Execute()).Returns(new EntityForTest { IsHandledByVisualiser = isHandledByVisualiser });
		var diagramAreaUserControl = rendered.GetControl<NetworkUserControl>().MainDiagramControl;
		diagramAreaUserControl.DiagramModel?.SetContainer(new Rectangle(new Point(0, 0), new Size(500, 500)));

		await diagramAreaUserControl.InvokeWinzorDispatcherAsync(() =>
		{
			Assert.That(() => diagramAreaUserControl.ExecuteContextMenuAction(networkAction.Object, new WebMouseEventArgs()), Throws.Nothing);
		});
	}

	[Test]
	public async Task DiagramUserControl_ShouldNotThrowOnBoxSelectAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var networkUserControl = new NetworkUserControlBuilder().Build();
			return networkUserControl.MainDiagramControl;
		});
		var diagramAreaUserControl = rendered.GetControl<DiagramAreaUserControl>();
		var diagramModel = diagramAreaUserControl.DiagramModel;
		diagramModel!.SetContainer(null);

		diagramModel.TriggerPointerDown(null, new PointerEventArgs().ToCore());

		var ex = Assert.Throws<Exception>(() => diagramModel.TriggerPointerMove(null, new PointerEventArgs().ToCore()));
		Assert.That(ex.Message, Does.Contain("Container not available"));

		diagramModel!.SetContainer(Rectangle.Zero);
		diagramModel!.TriggerPointerDown(null, new PointerEventArgs().ToCore());
		Assert.DoesNotThrow(() => diagramModel.TriggerPointerMove(null, new PointerEventArgs().ToCore()));
	}

	[Test, WithPlaywrightPage]
	public async Task ScaledDiagram_ChannelsShownAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			var channel1 = new DummyChannel("Top", 500);
			var channel2 = new DummyChannel("Middle", 200);
			var channel3 = new DummyChannel("Bottom", 100);

			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsDiagramScaled = true, DiagramChannels = new[] { channel1, channel2, channel3 } };

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var channelHeaderColumn = page.Locator(".diagramnode__channelheaders");

		Assert.That(await channelHeaderColumn.GetComputedStyleAsync("width"), Is.EqualTo("100px"));
		Assert.That(await channelHeaderColumn.GetComputedStyleAsync("border-right"), Is.EqualTo("2px solid rgb(0, 0, 0)"));

		var channelHeaders = await channelHeaderColumn.Locator(".channelheader").AllAsync();

		Assert.That(channelHeaders.Count, Is.EqualTo(3));

		Assert.That(await channelHeaders[0].GetComputedStyleAsync("height"), Is.EqualTo("500px"));
		Assert.That(await channelHeaders[0].Locator(".channelheader__label").TextContentAsync(), Is.EqualTo("Top"));

		Assert.That(await channelHeaders[1].GetComputedStyleAsync("height"), Is.EqualTo("200px"));
		Assert.That(await channelHeaders[1].Locator(".channelheader__label").TextContentAsync(), Is.EqualTo("Middle"));

		Assert.That(await channelHeaders[2].GetComputedStyleAsync("height"), Is.EqualTo("125px"));
		Assert.That(await channelHeaders[2].Locator(".channelheader__label").TextContentAsync(), Is.EqualTo("Bottom"));

		var channelDividersDiv = page.Locator(".diagramnode__channeldividers");

		var channelDividers = await channelDividersDiv.Locator(".channel__divider").AllAsync();

		Assert.That(channelDividers.Count, Is.EqualTo(2));

		Assert.That(await channelDividers[0].GetComputedStyleAsync("top"), Is.EqualTo("500px"));
		Assert.That(await channelDividers[0].GetComputedStyleAsync("border-bottom"), Is.EqualTo("1px solid rgb(192, 192, 192)"));
		Assert.That(await channelDividers[1].GetComputedStyleAsync("top"), Is.EqualTo("700px"));
		Assert.That(await channelDividers[1].GetComputedStyleAsync("border-bottom"), Is.EqualTo("1px solid rgb(192, 192, 192)"));
	}

	[Test, WithPlaywrightPage]
	public async Task CheckForWidthOfChannelHeaderWhenScheduledDatesAreSetAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			var channel = new DummyChannel("Top", 500);

			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsDiagramScaled = true, DiagramChannels = new[] { channel }, ScheduledStartTimeLocal = new DateTime(2024, 4, 1), ScheduledEndTimeLocal = new DateTime(2024, 4, 3) };

			var networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.ViewModel.ContentScale = 1.5;
			return networkUserControl;
		});

		var channelHeaderColumn = page.Locator(".diagramnode__channelheaders");

		Assert.That(await channelHeaderColumn.GetComputedStyleAsync("min-width"), Is.EqualTo("150px"));
	}

	[Test, WithPlaywrightPage]
	public async Task ScaledDiagram_ChannelsShownCorrectly_AfterZoomAsync()
	{
		NetworkUserControl control = null!;
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			var channel1 = new DummyChannel("Top", 500);
			var channel2 = new DummyChannel("Middle", 200);
			var channel3 = new DummyChannel("Bottom", 100);

			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsDiagramScaled = true, DiagramChannels = new[] { channel1, channel2, channel3 } };

			control = new NetworkUserControl(network.DiagramEntity, refresher);
			control.SetDataContext(network, false);
			return control;
		});

		await page.WaitForSelectorAsync(".diagramnode__channelheaders");
		var channelHeaderColumn = page.Locator(".diagramnode__channelheaders");

		Assert.That(await channelHeaderColumn.GetComputedStyleAsync("width"), Is.EqualTo("100px"));
		Assert.That(await channelHeaderColumn.GetComputedStyleAsync("border-right"), Is.EqualTo("2px solid rgb(0, 0, 0)"));

		var channelHeaders = await channelHeaderColumn.Locator(".channelheader").AllAsync();

		Assert.That(await channelHeaders[0].GetComputedStyleAsync("height"), Is.EqualTo("500px"));
		Assert.That(await channelHeaders[1].GetComputedStyleAsync("height"), Is.EqualTo("200px"));
		Assert.That(await channelHeaders[2].GetComputedStyleAsync("height"), Is.EqualTo("125px"));

		var channelDividersDiv = page.Locator(".diagramnode__channeldividers");

		var channelDividers = await channelDividersDiv.Locator(".channel__divider").AllAsync();

		Assert.That(await channelDividers[0].GetComputedStyleAsync("top"), Is.EqualTo("500px"));
		Assert.That(await channelDividers[0].GetComputedStyleAsync("border-bottom"), Is.EqualTo("1px solid rgb(192, 192, 192)"));
		Assert.That(await channelDividers[1].GetComputedStyleAsync("top"), Is.EqualTo("700px"));
		Assert.That(await channelDividers[1].GetComputedStyleAsync("border-bottom"), Is.EqualTo("1px solid rgb(192, 192, 192)"));

		await control.InvokeWinzorDispatcherAsync(() =>
		{
			control.ViewModel.ContentScale = 2;
		});

		Assert.That(await channelHeaderColumn.GetComputedStyleAsync("width"), Is.EqualTo("200px"));
		Assert.That(await channelHeaderColumn.GetComputedStyleAsync("border-right"), Is.EqualTo("4px solid rgb(0, 0, 0)"));

		Assert.That(await channelHeaders[0].GetComputedStyleAsync("height"), Is.EqualTo("1000px"));
		Assert.That(await channelHeaders[1].GetComputedStyleAsync("height"), Is.EqualTo("400px"));
		Assert.That(await channelHeaders[2].GetComputedStyleAsync("height"), Is.EqualTo("225px"));

		Assert.That(await channelDividers[0].GetComputedStyleAsync("top"), Is.EqualTo("1000px"));
		Assert.That(await channelDividers[0].GetComputedStyleAsync("border-bottom"), Is.EqualTo("2px solid rgb(192, 192, 192)"));
		Assert.That(await channelDividers[1].GetComputedStyleAsync("top"), Is.EqualTo("1400px"));
		Assert.That(await channelDividers[1].GetComputedStyleAsync("border-bottom"), Is.EqualTo("2px solid rgb(192, 192, 192)"));
	}

	[Test]
	public async Task ScaledDiagram_ChannelsNotShownAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() => TestHelpers.SetupNetworkUserControl(isDiagramScaled: true));

		Assert.Throws<ElementNotFoundException>(() => rendered.Find(".diagramnode__channelheaders"));

		Assert.Throws<ElementNotFoundException>(() => rendered.Find(".diagramnode__channeldividers"));
	}

	[Test]
	public async Task DiagramAreaUserControlScrollsToContentOffsetOnRenderAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new NetworkUserControlBuilder().Build().MainDiagramControl);

		var jsInvocations = ctx.JSInterop.Invocations.ToList();

		var scrollInvocation = jsInvocations.Single(i => i.Identifier == "scrollElementTo");
		Assert.That(scrollInvocation.Arguments[1], Is.EqualTo(0));
		Assert.That(scrollInvocation.Arguments[2], Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task DiagramAreaUserControlDiagramCanvasHasNoTranslateAppliedAsync()
	{
		NetworkUserControl? networkUserControl = null;
		var entity = Stub.Entity(e => { e.Width = 300; e.Height = 300; });
		var diagramEntity = Stub.Entity(e =>
		{
			e.SupportedActions = NetworkActions.StyleDiagram;
			e.IsDiagramScaled = false;
			e.ShouldShowNonScheduledSection = false;
			e.ScaleUnitPixelSize = 50;
		});
		var network = new NetworkBuilder().WithEntities(entity).WithDiagramEntity(diagramEntity).Build();

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.MainDiagramControl.ViewModel.NetworkControlViewModel.ContentScale = 2;
			return networkUserControl;
		});

		var diagramAreaUserControl = page.Locator(".diagramareausercontrol");
		await diagramAreaUserControl.DispatchEventAsync("scroll", new Dictionary<string, string> {
			{ "clientX", "123" },
			{ "clientY", "456" } });

		var svgLayer = page.Locator(".diagram-svg-layer").First;
		var htmlLayer = page.Locator(".diagram-html-layer").First;

		Assert.Multiple(() =>
		{
			// Check the Blazor.Diagrams styles are being added to the inline styles
			Assert.That(() => svgLayer.GetAttributeAsync("style"), Does.Contain("transform: translate(0px, 0px) scale(2);").After(3000, 100));
			Assert.That(() => htmlLayer.GetAttributeAsync("style"), Does.Contain("transform: translate(0px, 0px) scale(2);").After(3000, 100));

			// Check that we are overriding the inline styles to apply scale but not translate
			Assert.That(() => svgLayer.GetComputedStyleAsync("transform"), Is.EqualTo("matrix(2, 0, 0, 2, 0, 0)").After(3000, 100));
			Assert.That(() => htmlLayer.GetComputedStyleAsync("transform"), Is.EqualTo("matrix(2, 0, 0, 2, 0, 0)").After(3000, 100));
		});
	}

	[Test]
	public async Task NodesUnselectedOnClickOutsideDiagramCanvasAsync()
	{
		using var ctx = new EnterpriseTestContext();
		NetworkUserControl? networkUserControl = null!;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var network = new NetworkBuilder().WithEntities(new EntityForTest(), new EntityForTest()).Build();
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsDiagramScaled = true, DiagramChannels = new[] { new DummyChannel("ABC", 100) } };
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			return networkUserControl;
		});
		Assert.That(networkUserControl, Is.Not.Null);

		var dummyEventArgs = new PointerEventArgs();

		var diagramModel = rendered.FindComponent<DiagramCanvas>().Instance.BlazorDiagram!;
		await ctx.Renderer.Dispatcher.InvokeAsync(() =>
		{
			diagramModel.SelectModel(diagramModel.Nodes[0], false);
			diagramModel.SelectModel(diagramModel.Nodes[1], false);
		});
		Assert.That(() => networkUserControl.NetworkViewModel.SelectedNodes.Count(), Is.EqualTo(2).After(3000, 100));

		//Clicking on header
		var diagramHeader = rendered.Find(".diagramnode__header");
		diagramHeader.TriggerEvent("onpointerdown", dummyEventArgs);
		Assert.That(() => networkUserControl.NetworkViewModel.SelectedNodes.Count(), Is.EqualTo(0).After(3000, 100));

		await ctx.Renderer.Dispatcher.InvokeAsync(() =>
		{
			diagramModel.SelectModel(diagramModel.Nodes[0], false);
			diagramModel.SelectModel(diagramModel.Nodes[1], false);
		});
		Assert.That(() => networkUserControl.NetworkViewModel.SelectedNodes.Count(), Is.EqualTo(2).After(3000, 100));

		//Clicking on channels
		var diagramChannelHeaders = rendered.Find(".diagramnode__channelheaders");
		diagramChannelHeaders.TriggerEvent("onpointerdown", dummyEventArgs);
		Assert.That(() => networkUserControl.NetworkViewModel.SelectedNodes.Count(), Is.EqualTo(0).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ScaledDiagramWithChannels_MoveNodeToNonScheduledSection_NoExceptionThrownAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var node = Stub.Entity();
		node.IsNonScheduled = false;
		node.IsDiagramScaled = true;

		var channel1 = new DummyChannel("Top", 500);
		var channel2 = new DummyChannel("Middle", 200);
		var diagramEntity = new EntityForTest { IsDiagramScaled = true, ScaleUnitPixelSize = 100 };
		diagramEntity.ShouldShowNonScheduledSection = true;
		diagramEntity.NonScheduledSectionWidth = 500;
		diagramEntity.DiagramChannels = new[] { channel1, channel2 };
		var network = new NetworkBuilder().Build();
		network.DiagramEntity = diagramEntity;
		network.Entities.Add(node);

		var mockScaleDescriptor = new Mock<INetworkScaleDescriptor>();
		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i < 0))).Throws(new ArgumentException());
		var mockScaleSet = new Mock<INetworkScaleSet>();
		var scalePoints = new List<INetworkScalePoint>();
		mockScaleSet.Setup(m => m.ScalePoints).Returns(scalePoints);
		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i >= 0))).Returns(mockScaleSet.Object);
		network.ScaleDescriptor = mockScaleDescriptor.Object;

		NetworkUserControl control = null!;
		await ctx.LoadControlOnFormAsync(() => control = new NetworkUserControlBuilder().WithNetwork(network).Build());
		await control.InvokeWinzorDispatcherAsync(() =>
		{
			control.MainDiagramControl.ViewModel.ContentViewportWidth = 0;
			control.MainDiagramControl.ViewModel.ContentWidth = 0;
		});

		Assert.DoesNotThrow(() => node.SwapNonScheduledState());
	}

	static IEnumerable<TestCaseData> ScaledDiagramScheduledSectionChecksWidth_TestData
	{
		get
		{
			yield return new TestCaseData(300, true, false, new DateTime(2024, 3, 1), new DateTime(2024, 3, 3), ".networkusercontrol__diagramareacontrols > .diagramareausercontrol > .diagramareausercontrol__diagramnode", "width", "300px");
			yield return new TestCaseData(300, true, true, new DateTime(2024, 3, 1), new DateTime(2024, 3, 3), ".networkusercontrol__nonscheduleditems > .diagramareausercontrol > .diagramareausercontrol__diagramnode", "min-width", "100%");
			yield return new TestCaseData(300, false, true, new DateTime(2024, 3, 1), new DateTime(2024, 3, 3), ".networkusercontrol__nonscheduleditems > .diagramareausercontrol > .diagramareausercontrol__diagramnode", "min-width", "100%");
			yield return new TestCaseData(300, false, false, new DateTime(2024, 3, 1), new DateTime(2024, 3, 3), ".networkusercontrol__nonscheduleditems > .diagramareausercontrol > .diagramareausercontrol__diagramnode", "min-width", "100%");
			yield return new TestCaseData(300, true, false, new DateTime(), new DateTime(), ".networkusercontrol__diagramareacontrols > .diagramareausercontrol > .diagramareausercontrol__diagramnode", "min-width", "100%");
			yield return new TestCaseData(300, true, true, new DateTime(), new DateTime(), ".networkusercontrol__diagramareacontrols > .diagramareausercontrol > .diagramareausercontrol__diagramnode", "min-width", "100%");
			yield return new TestCaseData(300, false, true, new DateTime(), new DateTime(), ".networkusercontrol__diagramareacontrols > .diagramareausercontrol > .diagramareausercontrol__diagramnode", "min-width", "100%");
			yield return new TestCaseData(300, false, false, new DateTime(), new DateTime(), ".networkusercontrol__diagramareacontrols > .diagramareausercontrol > .diagramareausercontrol__diagramnode", "min-width", "100%");
		}
	}

	[TestCaseSource(nameof(ScaledDiagramScheduledSectionChecksWidth_TestData))]
	[Test, WithPlaywrightPage]
	public async Task ScaledDiagramScheduledSectionChecksWidthAsync(int contentWidth, bool isDiagramScaled, bool isNonScheduled, DateTime startDate, DateTime finishDate, string diagramAreaUserCssStyle, string expectedStyleProperty, string expectedStyleValue)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		NetworkUserControl? networkUserControl = null;
		var entity1 = new Entity()
		{
			Name = "Node 1",
			Width = 100,
			Height = 100,
			X = 100,
			Y = 100,
			ZIndex = 2
		};

		var diagramEntity = new EntityForTest { IsDiagramScaled = isDiagramScaled, IsNonScheduled = isNonScheduled, ScaleUnitPixelSize = 100, ScheduledStartTimeLocal = startDate, ScheduledEndTimeLocal = finishDate };

		var network = new NetworkBuilder().WithEntities(entity1).WithDiagramEntity(diagramEntity).Build();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.MainDiagramControl.ViewModel.ContentWidth = contentWidth;
			networkUserControl.ViewModel.ContentScale = 1;
			return networkUserControl;
		});

		var diagramAreaUserControlStyle = page.Locator(diagramAreaUserCssStyle);

		Assert.That(await diagramAreaUserControlStyle.GetComputedStyleAsync(expectedStyleProperty), Is.EqualTo(expectedStyleValue));
	}

	[Test]
	public async Task DiagramAreaUserControlRegistersGlobalPointerEventListenersAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var pointerMoveTcs = new TaskCompletionSource<DiagramPointerArgs>();
		var pointerUpTcs = new TaskCompletionSource<DiagramPointerArgs>();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var networkUserControl = new NetworkUserControlBuilder().Build();
			return networkUserControl.MainDiagramControl;
		});

		var diagramAreaUserControl = rendered.GetControl<DiagramAreaUserControl>();
		var diagramModel = rendered.FindComponent<DiagramCanvas>().Instance.BlazorDiagram as NCNDiagramModel;
		diagramModel!.SetContainer(new Rectangle(0, 0, 100, 100));
		diagramModel.PointerMove += (_, args) => pointerMoveTcs.SetResult(args);
		diagramModel.PointerUp += (_, args) => pointerUpTcs.SetResult(args);

		diagramModel.TriggerPointerDown(null, new PointerEventArgs().ToCore());

		var interopInvoke = ctx.JSInterop.Invocations.Single(i => i.Identifier == "subscribeToPointerEventsOutsideElement");
		Assert.That(interopInvoke.Arguments[0], Is.EqualTo(diagramAreaUserControl.diagramCanvasRef));

		var pointerMoveCallback = interopInvoke.Arguments[1] as DotNetObjectReference<ClientEventCallback<PointerEventArgs>>;
		await pointerMoveCallback!.Value.InvokeCallbackAsync(new PointerEventArgs { ClientX = 123, ClientY = 456 });
		Assert.That(() => pointerMoveTcs.Task.IsCompleted, Is.True.After(3000, 100));
		var pointerMoveArgs = await pointerMoveTcs.Task;
		Assert.That(pointerMoveArgs.ClientX, Is.EqualTo(123));
		Assert.That(pointerMoveArgs.ClientY, Is.EqualTo(456));

		var pointerUpCallback = interopInvoke.Arguments[2] as DotNetObjectReference<ClientEventCallback<PointerEventArgs>>;
		await pointerUpCallback!.Value.InvokeCallbackAsync(new PointerEventArgs { ClientX = 123, ClientY = 456 });
		Assert.That(() => pointerUpTcs.Task.IsCompleted, Is.True.After(3000, 100));
		var pointerUpArgs = await pointerUpTcs.Task;
		Assert.That(pointerUpArgs.ClientX, Is.EqualTo(123));
		Assert.That(pointerUpArgs.ClientY, Is.EqualTo(456));
	}

	[Test, WithPlaywrightPage]
	public async Task DiagramAreaUserControlAnnotationAsChildAsync()
	{
		const string jobName = "my job ";
		const string annotationName = "my annotation";

		NetworkUserControl? networkUserControl = null;
		var entity = new EntityForTest()
		{
			JobName = jobName,
			JobNumber = "1234"
		};

		var childEntity = new EntityForTest()
		{
			ShapeType = ShapeTypes.Annotation,
			Parent = entity,
			Name = annotationName
		};

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			network.Entities.Add(entity);
			network.Entities.Add(childEntity);

			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { BackColor = System.Drawing.Color.Red, SupportedActions = NetworkActions.StyleDiagram };
			networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);

			return networkUserControl;
		});

		var annotation = page.GetByTitle(annotationName);
		Assert.That(annotation, Is.Not.Null);
		var job = page.GetByTitle(jobName);
		Assert.That(job, Is.Not.Null);
	}

	[Test, WithPlaywrightPage]
	public async Task DiagramAreaUserControlCtrlSAsync()
	{
		const string annotationName = "my annotation";

		NetworkUserControl? networkUserControl = null;
		var entity = new EntityForTest()
		{
			ShapeType = ShapeTypes.Annotation,
			Name = annotationName
		};

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			network.Entities.Add(entity);

			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { BackColor = System.Drawing.Color.Red, SupportedActions = NetworkActions.StyleDiagram };
			networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);

			return networkUserControl;
		});

		var annotation = page.GetByTitle(annotationName);
		Assert.That(annotation, Is.Not.Null);

		var textArea = annotation.Locator(".annotationnode__notes .textarea__resizable");

		await textArea.FocusAsync();
		var isFocusedIn = await textArea.EvaluateAsync<bool>("e => e === document.activeElement");
		Assert.That(isFocusedIn, Is.True, "The text area should be focused after FocusAsync.");

		await textArea.FillAsync("Test content for Ctrl+S simulation.");

		await page.Keyboard.PressAsync("Control+s");

		var isFocused = await textArea.EvaluateAsync<bool>("e => e === document.activeElement");
		Assert.That(isFocused, Is.False, "The focus should have been lost after pressing Ctrl+S.");
	}

	[Test, WithPlaywrightPage]
	public async Task ScaledDiagram_ShowsLabelsAsync()
	{
		var entity = Stub.Entity(e => { e.Width = 300; e.Height = 300; });
		var diagramEntity = Stub.Entity(e =>
		{
			e.SupportedActions = NetworkActions.StyleDiagram;
			e.IsDiagramScaled = true;
			e.ShouldShowNonScheduledSection = false;
			e.ScaleUnitPixelSize = 50;
		});
		var network = new NetworkBuilder().WithEntities(entity).WithDiagramEntity(diagramEntity).Build();

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new NetworkUserControlBuilder().WithNetwork(network).Build());

		await Assertions.Expect(page.Locator(".diagramareausercontrol_schedulegrid")).ToHaveCountAsync(1);
		await Assertions.Expect(page.Locator(".schedulegrid_timescale_label")).ToHaveCountAsync(6);
		await Assertions.Expect(page.Locator(".timescale_label_text")).ToHaveCountAsync(6);
	}

	[TestCaseSource(nameof(ZoomAndMergedColumns_TestData))]
	public async Task ScaledDiagram_ShowsCorrectLabelTextOnZoomOutAsync(double zoom, int numberOfMergedColumns)
	{
		var mockScaleDescriptor = new Mock<INetworkScaleDescriptor>();
		var mockScaleSet = new Mock<INetworkScaleSet>();
		var scalePoints = new List<INetworkScalePoint>();
		for (int i = 0; i < 100; i++)
		{
			var mockScalePoint = new Mock<INetworkScalePoint>();
			mockScalePoint.SetupGet(x => x.Label).Returns($"test-label{i}");
			scalePoints.Add(mockScalePoint.Object);
		}
		mockScaleSet.SetupGet(x => x.ScalePoints).Returns(scalePoints);

		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i >= 0))).Returns(mockScaleSet.Object);

		var refresher = new NetworkRefresher();
		var network = new NetworkBuilder().Build();
		refresher.AssociateWithNetwork(network);
		network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsDiagramScaled = true };
		network.ScaleDescriptor = mockScaleDescriptor.Object;

		NetworkUserControl control = null!;
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			control = new NetworkUserControlBuilder().WithNetwork(network).Build();
			control.MainDiagramControl.ViewModel.ContentWidth = 1200;
			control.MainDiagramControl.ViewModel.ContentHeight = 500;
			control.ViewModel.ContentScale = zoom;
			return control;
		});

		var timeScaleLabels = rendered.FindAll(".schedulegrid_timescale_label > .timescale_label_text");
		var index = numberOfMergedColumns - 1;

		foreach (var timeScaleLabel in timeScaleLabels)
		{
			Assert.That(timeScaleLabel.TextContent, Is.EqualTo($"test-label{index}"));
			index += numberOfMergedColumns;
		}
	}

	[TestCaseSource(nameof(ZoomAndMergedColumns_TestData))]
	public async Task ScaledDiagram_WithChannels_ShowsCorrectLabelTextOnZoomOutAsync(double zoom, int numberOfMergedColumns)
	{
		var mockScaleDescriptor = new Mock<INetworkScaleDescriptor>();
		var mockScaleSet = new Mock<INetworkScaleSet>();
		var scalePoints = new List<INetworkScalePoint>();
		for (int i = 0; i < 100; i++)
		{
			var mockScalePoint = new Mock<INetworkScalePoint>();
			mockScalePoint.SetupGet(x => x.Label).Returns($"test-label{i}");
			scalePoints.Add(mockScalePoint.Object);
		}
		mockScaleSet.SetupGet(x => x.ScalePoints).Returns(scalePoints);

		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i >= 0))).Returns(mockScaleSet.Object);

		var refresher = new NetworkRefresher();
		var network = new NetworkBuilder().Build();
		refresher.AssociateWithNetwork(network);
		var channel1 = new DummyChannel("Top", 500);
		var channel2 = new DummyChannel("Middle", 200);
		var channel3 = new DummyChannel("Bottom", 100);

		network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsDiagramScaled = true, DiagramChannels = new[] { channel1, channel2, channel3 }, ScaleUnitPixelSize = 100 };
		network.ScaleDescriptor = mockScaleDescriptor.Object;

		NetworkUserControl control = null!;
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			control = new NetworkUserControlBuilder().WithNetwork(network).Build();
			control.MainDiagramControl.ViewModel.ContentWidth = 1200;
			control.MainDiagramControl.ViewModel.ContentHeight = 500;
			control.ViewModel.ContentScale = zoom;
			return control;
		});

		var timeScaleLabels = rendered.FindAll(".schedulegrid_timescale_label > .timescale_label_text");
		var index = 2 * (numberOfMergedColumns - 1);

		foreach (var timeScaleLabel in timeScaleLabels)
		{
			Assert.That(timeScaleLabel.TextContent, Is.EqualTo($"test-label{index}"));
			index += numberOfMergedColumns;
		}
	}

	[Test]
	public async Task ScaledDiagram_WithChannels_DoesNotRenderExtraColumnAsync()
	{
		var refresher = new NetworkRefresher();
		var network = new NetworkBuilder().Build();
		refresher.AssociateWithNetwork(network);
		var channel1 = new DummyChannel("Top", 500);
		var channel2 = new DummyChannel("Middle", 200);
		var channel3 = new DummyChannel("Bottom", 100);

		network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsDiagramScaled = true, DiagramChannels = new[] { channel1, channel2, channel3 }, ScaleUnitPixelSize = 100 };

		var entity = new Entity() { X = 500, Y = 0, Width = 2600, Height = 100 };
		network.Entities.Add(entity);

		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() => new NetworkUserControlBuilder().WithNetwork(network).Build());

		Assert.That(() => rendered.Find(".diagramareausercontrol").GetAttribute("style"), Does.Contain("--diagram-content-min-width: 3100px;"));
	}

	[Test, WithPlaywrightPage]
	public async Task ScaledDiagram_WithChannels_ShowsLastLabelAfterNodeRepositionedAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var mockScaleDescriptor = new Mock<INetworkScaleDescriptor>();
		Func<int, INetworkScaleSet> getMockScaleSet = (numberOfScalePoints) =>
		{
			var mockScaleSet = new Mock<INetworkScaleSet>();
			var scalePoints = new List<INetworkScalePoint>();
			for (int i = 0; i < numberOfScalePoints; i++)
			{
				var mockScalePoint = new Mock<INetworkScalePoint>();
				mockScalePoint.SetupGet(x => x.Label).Returns($"test-label{i}");
				scalePoints.Add(mockScalePoint.Object);
			}

			mockScaleSet.SetupGet(x => x.ScalePoints).Returns(scalePoints);
			return mockScaleSet.Object;
		};

		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i >= 0))).Returns((int i) => getMockScaleSet(i));

		var refresher = new NetworkRefresher();
		var network = new NetworkBuilder().Build();
		refresher.AssociateWithNetwork(network);
		var channel1 = new DummyChannel("Top", 500);
		var channel2 = new DummyChannel("Middle", 200);
		var channel3 = new DummyChannel("Bottom", 100);

		network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsDiagramScaled = true, DiagramChannels = new[] { channel1, channel2, channel3 }, ScaleUnitPixelSize = 100 };
		network.ScaleDescriptor = mockScaleDescriptor.Object;

		var entity = new Entity() { Name = "Test", X = 500, Y = 0, Width = 2600, Height = 100 };
		network.Entities.Add(entity);

		NetworkUserControl control = null!;
		var page = await ctx.LoadControlOnFormAsync(() => control = new NetworkUserControlBuilder().WithNetwork(network).Build());

		var node = page.GetByTitle("Node: Test");
		Assert.That(node, Is.Not.Null);
		await node.ClickAsync(new LocatorClickOptions { Position = new Position { X = 0, Y = 0 } });

		var nodeBoundingBox = await node.BoundingBoxAsync();
		Assert.That(nodeBoundingBox, Is.Not.Null);
		var x = nodeBoundingBox.X;
		var y = nodeBoundingBox.Y;

		await page.Mouse.MoveAsync(x + 1, y + 1);
		await page.Mouse.DownAsync();
		await page.Locator(".networknode--indiagram[data-selected]").First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

		await page.Mouse.MoveAsync(x - 100, y + 100, new MouseMoveOptions { Steps = 20 });
		await page.Mouse.UpAsync();
		await page.Mouse.ClickAsync(0, 0);

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

		var node1 = page.GetByTitle("Node: Test");
		Assert.That(node1, Is.Not.Null);
		await node1.ClickAsync(new LocatorClickOptions { Position = new Position { X = 0, Y = 0 } });
		var nodeBoundingBox1 = await node1.BoundingBoxAsync();
		Assert.That(nodeBoundingBox1, Is.Not.Null);

		var diagramareausercontrol = page.Locator(".diagram-canvas");
		Assert.That(diagramareausercontrol, Is.Not.Null);
		var boundingBox = await diagramareausercontrol.BoundingBoxAsync();
		Assert.That(boundingBox, Is.Not.Null);

		var relativeNodeLabels = (int)(nodeBoundingBox1.X - boundingBox.X) / 100;
		var nodeLabels = 26;

		var finalLabelsCount = relativeNodeLabels + nodeLabels;

		await Assertions.Expect(page.Locator(".schedulegrid_timescale_label")).ToHaveCountAsync(finalLabelsCount);
		await Assertions.Expect(page.Locator(".timescale_label_text")).ToHaveCountAsync(finalLabelsCount);
	}

	[TestCaseSource(nameof(ZoomAndMergedColumns_TestData))]
	public async Task ScaledDiagram_AppliesCorrectLabelStylesOnZoomOutAsync(double zoom, int numberOfMergedColumns)
	{
		var mockScaleDescriptor = new Mock<INetworkScaleDescriptor>();
		var mockScaleSet = new Mock<INetworkScaleSet>();
		var scalePoints = new List<INetworkScalePoint>();
		for (int i = 0; i < 100; i++)
		{
			var mockScalePoint = new Mock<INetworkScalePoint>();
			mockScalePoint.SetupGet(x => x.Label).Returns($"test-label{i}");
			scalePoints.Add(mockScalePoint.Object);
		}
		mockScaleSet.SetupGet(x => x.ScalePoints).Returns(scalePoints);

		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i >= 0))).Returns(mockScaleSet.Object);

		var refresher = new NetworkRefresher();
		var network = new NetworkBuilder().Build();
		refresher.AssociateWithNetwork(network);
		network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsDiagramScaled = true, ScaleUnitPixelSize = 100 };
		network.ScaleDescriptor = mockScaleDescriptor.Object;

		NetworkUserControl control = null!;
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			control = new NetworkUserControlBuilder().WithNetwork(network).Build();
			control.MainDiagramControl.ViewModel.ContentWidth = 1200;
			control.MainDiagramControl.ViewModel.ContentHeight = 500;
			control.ViewModel.ContentScale = zoom;
			return control;
		});

		var timeScaleLabelContainers = rendered.FindAll(".schedulegrid_timescale_label");
		var index = 0;

		foreach (var timeScaleLabelContainer in timeScaleLabelContainers)
		{
			Assert.That(timeScaleLabelContainer.GetAttribute("style"), Does.Contain($"left: {(100 * index) + 4}"));
			Assert.That(timeScaleLabelContainer.GetAttribute("style"), Does.Contain($"width: {(100 * numberOfMergedColumns) - 6}px;"));
			Assert.That(timeScaleLabelContainer.GetAttribute("style"), Does.Contain($"font-size: {12 * 1 / zoom}px;"));
			Assert.That(timeScaleLabelContainer.GetAttribute("style"), Does.Contain($"height: {(12 * 1 / zoom) * 1.2}px;"));
			index += numberOfMergedColumns;
		}
	}

	[Test]
	public async Task ScaledDiagram_ShowsTodaysLabelInBoldAsync()
	{
		var mockScaleDescriptor = new Mock<INetworkScaleDescriptor>();
		var mockScaleSet = new Mock<INetworkScaleSet>();
		var scalePoints = new List<INetworkScalePoint>();
		var mockScalePoint1 = new Mock<INetworkScalePoint>();
		mockScalePoint1.SetupGet(x => x.Label).Returns($"test-label0");
		mockScalePoint1.SetupGet(x => x.Column).Returns(0);

		var mockScalePoint2 = new Mock<INetworkScalePoint>();
		mockScalePoint2.SetupGet(x => x.Label).Returns($"test-label1");
		mockScalePoint2.SetupGet(x => x.Column).Returns(1);

		var mockScalePoint3 = new Mock<INetworkScalePoint>();
		mockScalePoint3.SetupGet(x => x.Label).Returns($"test-label2");
		mockScalePoint3.SetupGet(x => x.Column).Returns(2);

		scalePoints.Add(mockScalePoint1.Object);
		scalePoints.Add(mockScalePoint2.Object);
		scalePoints.Add(mockScalePoint3.Object);

		mockScaleSet.SetupGet(x => x.ScalePoints).Returns(scalePoints);
		mockScaleSet.SetupGet(x => x.IndexOfColumnInPresent).Returns(1);

		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i >= 0))).Returns(mockScaleSet.Object);

		var refresher = new NetworkRefresher();
		var network = new NetworkBuilder().Build();
		refresher.AssociateWithNetwork(network);
		network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsDiagramScaled = true, ScaleUnitPixelSize = 100 };
		network.ScaleDescriptor = mockScaleDescriptor.Object;

		NetworkUserControl control = null!;
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			control = new NetworkUserControlBuilder().WithNetwork(network).Build();
			control.MainDiagramControl.ViewModel.ContentWidth = 300;
			control.MainDiagramControl.ViewModel.ContentHeight = 500;
			return control;
		});

		var timeScaleLabelContainers = rendered.FindAll(".schedulegrid_timescale_label");

		Assert.That(timeScaleLabelContainers[0].GetAttribute("style"), Does.Not.Contain("font-weight: bold;"));
		Assert.That(timeScaleLabelContainers[1].GetAttribute("style"), Does.Contain("font-weight: bold;"));
		Assert.That(timeScaleLabelContainers[2].GetAttribute("style"), Does.Not.Contain("font-weight: bold;"));
	}

	[Test, WithPlaywrightPage]
	public async Task ScaledDiagram_BackgroundsShownCorrectlyAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var node = Stub.Entity();

		var diagramEntity = new EntityForTest { IsNonScheduled = false, IsDiagramScaled = true, ScaleUnitPixelSize = 100, ShouldShowNonScheduledSection = false };

		var network = new NetworkBuilder().Build();
		network.DiagramEntity = diagramEntity;
		network.Entities.Add(node);

		var mockScaleDescriptor = new Mock<INetworkScaleDescriptor>();
		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i < 0))).Throws(new ArgumentException());

		var mockScaleSet = new Mock<INetworkScaleSet>();
		var scalePoints = new List<INetworkScalePoint>();

		var mockBackground1 = new Mock<INetworkScaleBackground>();
		mockBackground1.Setup(b => b.StartColumn).Returns(0);
		mockBackground1.Setup(b => b.EndColumn).Returns(5);
		mockBackground1.Setup(b => b.Color).Returns(System.Drawing.Color.Red);

		var mockBackground2 = new Mock<INetworkScaleBackground>();
		mockBackground2.Setup(b => b.StartColumn).Returns(7);
		mockBackground2.Setup(b => b.EndColumn).Returns(8);
		mockBackground2.Setup(b => b.Color).Returns(System.Drawing.Color.Blue);

		var mockBackground3 = new Mock<INetworkScaleBackground>();
		mockBackground3.Setup(b => b.StartColumn).Returns(12);
		mockBackground3.Setup(b => b.EndColumn).Returns(20);
		mockBackground3.Setup(b => b.Color).Returns(System.Drawing.Color.Green);

		var backgrounds = new List<INetworkScaleBackground>(new[] { mockBackground1.Object, mockBackground2.Object, mockBackground3.Object });

		mockScaleSet.Setup(m => m.ScalePoints).Returns(scalePoints);
		mockScaleSet.Setup(m => m.BackgroundPoints).Returns(backgrounds);

		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i >= 0))).Returns(mockScaleSet.Object);
		network.ScaleDescriptor = mockScaleDescriptor.Object;

		NetworkUserControl control = null!;
		var page = await ctx.LoadControlOnFormAsync(() => control = new NetworkUserControlBuilder().WithNetwork(network).Build());

		var backgroundsContainer = page.Locator(".diagramnode__backgrounds").First;
		Assert.That(async () => await backgroundsContainer.GetAttributeAsync("style"), Does.Contain("margin-left: 0px;"));

		var backgroundStrips = await backgroundsContainer.Locator(".background__strip").AllAsync();

		Assert.That(backgroundStrips.Count, Is.EqualTo(3));

		Assert.That(async () => await backgroundStrips[0].GetAttributeAsync("style"), Does.Contain("width: 500px;"));
		Assert.That(async () => await backgroundStrips[0].GetAttributeAsync("style"), Does.Contain("left: 0px;"));
		Assert.That(async () => await backgroundStrips[0].GetAttributeAsync("style"), Does.Contain("background-color: #FF0000FF"));

		Assert.That(async () => await backgroundStrips[1].GetAttributeAsync("style"), Does.Contain("width: 100px;"));
		Assert.That(async () => await backgroundStrips[1].GetAttributeAsync("style"), Does.Contain("left: 700px;"));
		Assert.That(async () => await backgroundStrips[1].GetAttributeAsync("style"), Does.Contain("background-color: #0000FFFF"));

		Assert.That(async () => await backgroundStrips[2].GetAttributeAsync("style"), Does.Contain("width: 800px;"));
		Assert.That(async () => await backgroundStrips[2].GetAttributeAsync("style"), Does.Contain("left: 1200px;"));
		Assert.That(async () => await backgroundStrips[2].GetAttributeAsync("style"), Does.Contain("background-color: #008000FF"));
	}

	[Test]
	public async Task ScaledDiagram_BackgroundsShownCorrectly_AfterZoomAsync()
	{
		var node = Stub.Entity();
		node.IsNonScheduled = false;
		node.IsDiagramScaled = true;

		var diagramEntity = new EntityForTest { IsDiagramScaled = true, ScaleUnitPixelSize = 100, ShouldShowNonScheduledSection = false };
		var refresher = new NetworkRefresher();
		var network = new NetworkBuilder().Build();
		refresher.AssociateWithNetwork(network);

		var mockScaleDescriptor = new Mock<INetworkScaleDescriptor>();
		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i < 0))).Throws(new ArgumentException());
		var mockScaleSet = new Mock<INetworkScaleSet>();
		var scalePoints = new List<INetworkScalePoint>();

		var mockBackground = new Mock<INetworkScaleBackground>();
		mockBackground.Setup(b => b.StartColumn).Returns(3);
		mockBackground.Setup(b => b.EndColumn).Returns(8);
		mockBackground.Setup(b => b.Color).Returns(System.Drawing.Color.Red);

		network.DiagramEntity = diagramEntity;
		network.ScaleDescriptor = mockScaleDescriptor.Object;
		network.Entities.Add(node);

		var backgrounds = new List<INetworkScaleBackground>(new[] { mockBackground.Object });

		mockScaleSet.Setup(m => m.ScalePoints).Returns(scalePoints);
		mockScaleSet.Setup(m => m.BackgroundPoints).Returns(backgrounds);

		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i >= 0))).Returns(mockScaleSet.Object);
		network.ScaleDescriptor = mockScaleDescriptor.Object;

		NetworkUserControl control = null!;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			control = new NetworkUserControlBuilder().WithNetwork(network).Build();
			control.MainDiagramControl.ViewModel.ContentWidth = 1000;
			control.MainDiagramControl.ViewModel.ContentHeight = 1000;
			control.ViewModel.ContentScale = 1;
			return control;
		});

		var backgroundsContainer = rendered.Find(".diagramnode__backgrounds");
		Assert.That(() => backgroundsContainer.GetAttribute("style"), Does.Contain("margin-left: 0px;"));

		var backgroundStrip = rendered.Find(".background__strip");
		Assert.That(() => backgroundStrip.GetAttribute("style"), Does.Contain("width: 500px;"));
		Assert.That(() => backgroundStrip.GetAttribute("style"), Does.Contain("left: 300px;"));

		await control.InvokeWinzorDispatcherAsync(() =>
		{
			control.ViewModel.ContentScale = 0.5;
		});

		Assert.That(() => backgroundsContainer.GetAttribute("style"), Does.Contain("margin-left: 0px;").After(3000, 100));
		Assert.That(() => backgroundStrip.GetAttribute("style"), Does.Contain("width: 250px;").After(3000, 100));
		Assert.That(() => backgroundStrip.GetAttribute("style"), Does.Contain("left: 150px;").After(3000, 100));
	}

	[Test]
	public async Task ScaledDiagram_WithChannels_BackgroundsShownCorrectlyAsync()
	{
		var node = Stub.Entity();

		var refresher = new NetworkRefresher();
		var network = new NetworkBuilder().Build();
		refresher.AssociateWithNetwork(network);

		var channel1 = new DummyChannel("Top", 500);
		var channel2 = new DummyChannel("Middle", 200);
		var diagramEntity = new EntityForTest { IsNonScheduled = false, IsDiagramScaled = true, ScaleUnitPixelSize = 100, ShouldShowNonScheduledSection = false, DiagramChannels = new[] { channel1, channel2 } };

		var mockScaleDescriptor = new Mock<INetworkScaleDescriptor>();
		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i < 0))).Throws(new ArgumentException());

		network.DiagramEntity = diagramEntity;
		network.ScaleDescriptor = mockScaleDescriptor.Object;
		network.Entities.Add(node);

		var mockScaleSet = new Mock<INetworkScaleSet>();
		var scalePoints = new List<INetworkScalePoint>();

		var mockBackground1 = new Mock<INetworkScaleBackground>();
		mockBackground1.Setup(b => b.StartColumn).Returns(3);
		mockBackground1.Setup(b => b.EndColumn).Returns(8);
		mockBackground1.Setup(b => b.Color).Returns(System.Drawing.Color.Red);

		var backgrounds = new List<INetworkScaleBackground>(new[] { mockBackground1.Object });

		mockScaleSet.Setup(m => m.ScalePoints).Returns(scalePoints);
		mockScaleSet.Setup(m => m.BackgroundPoints).Returns(backgrounds);

		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i >= 0))).Returns(mockScaleSet.Object);
		network.ScaleDescriptor = mockScaleDescriptor.Object;

		NetworkUserControl control = null!;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => control = new NetworkUserControlBuilder().WithNetwork(network).Build());

		var backgroundsContainer = rendered.Find(".diagramnode__backgrounds");
		Assert.That(() => backgroundsContainer.GetAttribute("style"), Does.Contain("margin-left: 100px;"));

		var backgroundStrips = rendered.FindAll(".background__strip");

		Assert.That(backgroundStrips.Count, Is.EqualTo(1));

		Assert.That(() => backgroundStrips[0].GetAttribute("style"), Does.Contain("width: 500px;"));
		Assert.That(() => backgroundStrips[0].GetAttribute("style"), Does.Contain("left: 300px;"));
		Assert.That(() => backgroundStrips[0].GetAttribute("style"), Does.Contain("background-color: #FF0000FF"));
	}

	[Test]
	public async Task ScaledDiagram_WithChannels_BackgroundsShownCorrectly_AfterZoomAsync()
	{
		var node = Stub.Entity();
		node.IsNonScheduled = false;
		node.IsDiagramScaled = true;

		var diagramEntity = new EntityForTest { IsDiagramScaled = true, ScaleUnitPixelSize = 100, ShouldShowNonScheduledSection = false };
		var refresher = new NetworkRefresher();
		var network = new NetworkBuilder().Build();
		refresher.AssociateWithNetwork(network);

		var channel1 = new DummyChannel("Top", 500);
		var channel2 = new DummyChannel("Middle", 200);
		diagramEntity.DiagramChannels = new[] { channel1, channel2 };

		network.DiagramEntity = diagramEntity;
		network.Entities.Add(node);

		var mockScaleDescriptor = new Mock<INetworkScaleDescriptor>();
		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i < 0))).Throws(new ArgumentException());

		var mockScaleSet = new Mock<INetworkScaleSet>();
		var scalePoints = new List<INetworkScalePoint>();

		var mockBackground = new Mock<INetworkScaleBackground>();
		mockBackground.Setup(b => b.StartColumn).Returns(3);
		mockBackground.Setup(b => b.EndColumn).Returns(8);
		mockBackground.Setup(b => b.Color).Returns(System.Drawing.Color.Red);

		var backgrounds = new List<INetworkScaleBackground>(new[] { mockBackground.Object });

		mockScaleSet.Setup(m => m.ScalePoints).Returns(scalePoints);
		mockScaleSet.Setup(m => m.BackgroundPoints).Returns(backgrounds);

		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i >= 0))).Returns(mockScaleSet.Object);
		network.ScaleDescriptor = mockScaleDescriptor.Object;

		NetworkUserControl control = null!;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			control = new NetworkUserControlBuilder().WithNetwork(network).Build();
			control.MainDiagramControl.ViewModel.ContentWidth = 1000;
			control.MainDiagramControl.ViewModel.ContentHeight = 1000;
			control.ViewModel.ContentScale = 1;
			return control;
		});

		var backgroundsContainer = rendered.Find(".diagramnode__backgrounds");
		Assert.That(() => backgroundsContainer.GetAttribute("style"), Does.Contain("margin-left: 100px;"));

		var backgroundStrip = rendered.Find(".background__strip");
		Assert.That(() => backgroundStrip.GetAttribute("style"), Does.Contain("width: 500px;"));
		Assert.That(() => backgroundStrip.GetAttribute("style"), Does.Contain("left: 300px;"));

		await control.InvokeWinzorDispatcherAsync(() =>
		{
			control.ViewModel.ContentScale = 0.5;
		});

		Assert.That(() => backgroundsContainer.GetAttribute("style"), Does.Contain("margin-left: 50px;").After(3000, 100));
		Assert.That(() => backgroundStrip.GetAttribute("style"), Does.Contain("width: 250px;").After(3000, 100));
		Assert.That(() => backgroundStrip.GetAttribute("style"), Does.Contain("left: 150px;").After(3000, 100));
	}

	[Test]
	public async Task DiagramAreaUserControl_ExecuteContextMenuWithContainerAsync()
	{
		using var ctx = new EnterpriseTestContext();
		NetworkUserControl? networkUserControl = null;

		_ = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity();

			networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, isReloading: false);
			return networkUserControl;
		});

		var diagramAreaUserControl = networkUserControl!.MainDiagramControl;
		var action = new Mock<INetworkAction>();
		var args = new WebMouseEventArgs();
		_ = Assert.Throws<Exception>(() => diagramAreaUserControl.ExecuteContextMenuAction(action.Object, args));

		var exceptionReporter = ExceptionReporterTestListener.Instance;
		Assert.That(exceptionReporter, Has.Count.EqualTo(1));
		exceptionReporter.Clear();

		diagramAreaUserControl.DiagramModel!.SetContainer(new Rectangle(30, 30, 1000, 1000));
		await diagramAreaUserControl.ExecuteContextMenuActionAsync(action.Object, args);
		Mock.Get(action.Object).Verify(v => v.Execute());
	}

	[Test]
	public async Task ScaledDiagram_NonScheduledSectionBackgroundColorsAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsNonScheduled = true, IsDiagramScaled = true, ShouldShowNonScheduledSection = true };

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var nonScheduledSectionBackground = rendered.FindAll(".networkusercontrol__nonscheduleditems .diagramnode__backgrounds");
		Assert.That(nonScheduledSectionBackground.Count, Is.EqualTo(0));
	}

	[Test]
	public async Task ScaledDiagram_ScheduledSectionBackgroundColorsAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsNonScheduled = false, IsDiagramScaled = true, ShouldShowNonScheduledSection = false };

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var scheduledSectionBackground = rendered.FindAll((".diagramareausercontrol .diagramareausercontrol__diagramnode .diagramnode__backgrounds"));
		Assert.That(scheduledSectionBackground.Count, Is.Not.EqualTo(0));
	}

	[Test]
	public async Task UnScaledNCNBackgroundColorsAsync()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsNonScheduled = false, IsDiagramScaled = false, ShouldShowNonScheduledSection = false };

			var networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var scaledDiagramBackground = rendered.FindAll((".diagramareausercontrol .diagramareausercontrol__diagramnode .diagramnode__backgrounds"));
		Assert.That(scaledDiagramBackground.Count, Is.EqualTo(0));
	}

	[Test]
	public async Task WheelDownWithCtrlTriggersZoomOutOnDiagramCanvasAsync()
	{
		var entities = new[] {
			Stub.Entity(e => { e.IsNonScheduled = false; }),
			Stub.Entity(e => { e.IsNonScheduled = true; }),
		};

		using var ctx = new EnterpriseTestContext();
		var cut = await ctx.RenderControlOnFormAsync(() => new NetworkUserControlBuilder().Build());

		var networkControl = cut.Find(".networkusercontrol");
		var diagrams = cut.FindComponents<DiagramCanvas>();

		var scheduledDiagram = diagrams[0].Instance.BlazorDiagram;
		var nonScheduledDiagram = diagrams[1].Instance.BlazorDiagram;

		var ctrlKeyEventArgs = new ThrottledWheelEventArgs { CtrlKey = true, DeltaY = 10 };
		var noCtrlKeyEventArgs = new ThrottledWheelEventArgs { CtrlKey = false, DeltaY = 10 };

		Assert.That(scheduledDiagram.Zoom, Is.EqualTo(1).Within(0.0005));
		Assert.That(nonScheduledDiagram.Zoom, Is.EqualTo(1).Within(0.0005));

		networkControl.TriggerEvent("onthrottledwheel", ctrlKeyEventArgs);
		Assert.That(() => scheduledDiagram.Zoom, Is.EqualTo(0.9).Within(0.0005).After(1000, 100));
		Assert.That(() => nonScheduledDiagram.Zoom, Is.EqualTo(0.9).Within(0.0005).After(1000, 100));

		networkControl.TriggerEvent("onthrottledwheel", noCtrlKeyEventArgs);
		Assert.That(() => scheduledDiagram.Zoom, Is.EqualTo(0.9).Within(0.0005).After(500));
		Assert.That(() => nonScheduledDiagram.Zoom, Is.EqualTo(0.9).Within(0.0005).After(500));
	}

	[Test]
	public async Task WheeUpWithCtrlTriggersZoomInOnDiagramCanvasAsync()
	{
		var entities = new[] {
			Stub.Entity(e => { e.IsNonScheduled = false; }),
			Stub.Entity(e => { e.IsNonScheduled = true; }),
		};
		var network = new NetworkBuilder().WithEntities(entities).Build();
		using var ctx = new EnterpriseTestContext();
		var cut = await ctx.RenderControlOnFormAsync(() => new NetworkUserControlBuilder().WithNetwork(network).Build());

		var networkControl = cut.Find(".networkusercontrol");
		var diagrams = cut.FindComponents<DiagramCanvas>();

		var scheduledDiagram = diagrams[0].Instance.BlazorDiagram;
		var nonScheduledDiagram = diagrams[1].Instance.BlazorDiagram;

		var ctrlKeyEventArgs = new ThrottledWheelEventArgs { CtrlKey = true, DeltaY = -10 };
		var noCtrlKeyEventArgs = new ThrottledWheelEventArgs { CtrlKey = false, DeltaY = -10 };

		Assert.That(scheduledDiagram.Zoom, Is.EqualTo(1).Within(0.0005));
		Assert.That(nonScheduledDiagram.Zoom, Is.EqualTo(1).Within(0.0005));

		networkControl.TriggerEvent("onthrottledwheel", ctrlKeyEventArgs);
		Assert.That(() => scheduledDiagram.Zoom, Is.EqualTo(1.1).Within(0.0005).After(1000, 100));
		Assert.That(() => nonScheduledDiagram.Zoom, Is.EqualTo(1.1).Within(0.0005).After(1000, 100));

		networkControl.TriggerEvent("onthrottledwheel", noCtrlKeyEventArgs);
		Assert.That(() => scheduledDiagram.Zoom, Is.EqualTo(1.1).Within(0.0005).After(500));
		Assert.That(() => nonScheduledDiagram.Zoom, Is.EqualTo(1.1).Within(0.0005).After(500));
	}

	[Test, WithPlaywrightPage]
	public async Task OnSelectionChangedToLinkNodeDoesNotThrowExceptionAsync()
	{
		NetworkUserControl? networkUserControl = null;
		var entity1 = new Entity() { Width = 200, Height = 200, JobName = "first node", JobNumber = "1234" };
		var entity2 = new Entity() { Width = 200, Height = 200, JobName = "second node", JobNumber = "2234", X = 400 };

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			network.Entities.Add(entity1);
			network.Entities.Add(entity2);
			network.CreateRelationship(entity1, entity2);

			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity();
			networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);

			return networkUserControl;
		});

		var link = page.Locator(".diagram-link");
		Assert.DoesNotThrowAsync(() => link.ClickAsync());

		var node = page.Locator(".diagram-node").First;
		await node.ClickAsync();
		await Assertions.Expect(node).ToHaveClassAsync("diagram-node selected");
	}

	[Test, WithPlaywrightPage]
	public async Task ScaledDiagram_ShowsLabels_AfterZoomInAndOutAsync()
	{
		var mockScaleDescriptor = new Mock<INetworkScaleDescriptor>();
		var mockScaleSet = new Mock<INetworkScaleSet>();
		var scalePoints = new List<INetworkScalePoint>();
		for (var i = 0; i < 100; i++)
		{
			var mockScalePoint = new Mock<INetworkScalePoint>();
			mockScalePoint.SetupGet(x => x.Label).Returns($"test-label{i}");
			scalePoints.Add(mockScalePoint.Object);
		}
		mockScaleSet.SetupGet(x => x.ScalePoints).Returns(scalePoints);
		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i >= 0))).Returns(mockScaleSet.Object);

		var entityDiagram = Stub.Entity(e =>
		{
			e.SupportedActions = NetworkActions.StyleDiagram;
			e.IsDiagramScaled = true;
			e.IsNonScheduled = true;
		});

		var network = new NetworkBuilder().WithDiagramEntity(entityDiagram).Build();
		network.ScaleDescriptor = mockScaleDescriptor.Object;
		var refresher = new NetworkRefresher();
		refresher.AssociateWithNetwork(network);

		NetworkUserControl control = null!;

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			control = new NetworkUserControlBuilder().WithNetwork(network).Build();
			return control;
		});

		async Task CheckTimescaleLabels()
		{
			await page.WaitForSelectorAsync(".timescale_label_text");
			var labels = await page.Locator(".timescale_label_text").AllAsync();

			Assert.That(labels, Is.Not.Empty);
			for (var i = 0; i < labels.Count; i++)
			{
				Assert.That(() => labels[i].TextContentAsync(), Is.EqualTo($"test-label{i}").After(3000, 100));
			}
		}

		await control.InvokeWinzorDispatcherAsync(() => control.ViewModel.ContentScale = 1.5);
		await CheckTimescaleLabels();

		await control.InvokeWinzorDispatcherAsync(() => control.ViewModel.ContentScale = 2.0);
		await CheckTimescaleLabels();

		await control.InvokeWinzorDispatcherAsync(() => control.ViewModel.ContentScale = 1.0);
		await CheckTimescaleLabels();
	}

	[Test, WithPlaywrightPage]
	public async Task FrozenChannelHeaderIsAlwaysDisplayedAsync()
	{
		NetworkUserControl? networkUserControl = null;
		var entity = Stub.Entity(e => { e.Width = 300; e.Height = 300; });
		var channel1 = new DummyChannel("Top", 300);
		var channel2 = new DummyChannel("Middle", 200);
		var channel3 = new DummyChannel("Bottom", 100);
		var diagramEntity = Stub.Entity(e =>
		{
			e.SupportedActions = NetworkActions.StyleDiagram;
			e.IsDiagramScaled = true;
			e.ShouldShowNonScheduledSection = false;
			e.ScaleUnitPixelSize = 50;
			e.DiagramChannels = new[] { channel1, channel2, channel3 };
		});
		var network = new NetworkBuilder().WithEntities(entity).WithDiagramEntity(diagramEntity).Build();

		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.MainDiagramControl.ViewModel.NetworkControlViewModel.ContentScale = 1;
			networkUserControl.MainDiagramControl.ToggleFreezeChannelHeaders();
			return networkUserControl;
		});

		var header = page.Locator(".diagramnode__channelheaders").First;
		await Assertions.Expect(header).ToHaveClassAsync("diagramnode__channelheaders diagramnode__channelheaders--frozen");
		Assert.That(await header.GetComputedStyleAsync("position"), Is.EqualTo("sticky"));

		await page.EvaluateAsync("() => window.scrollTo(200, 0)");
		await Assertions.Expect(header).ToBeInViewportAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task FrozenTimeLabelsAreAlwaysDisplayedAsync()
	{
		NetworkUserControl? networkUserControl = null;
		var entity = Stub.Entity(e => { e.Width = 300; e.Height = 300; });
		var diagramEntity = Stub.Entity(e =>
		{
			e.SupportedActions = NetworkActions.StyleDiagram;
			e.IsDiagramScaled = true;
			e.ShouldShowNonScheduledSection = false;
			e.ScaleUnitPixelSize = 50;
		});
		var network = new NetworkBuilder().WithEntities(entity).WithDiagramEntity(diagramEntity).Build();

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.MainDiagramControl.ViewModel.NetworkControlViewModel.ContentScale = 1;
			networkUserControl.MainDiagramControl.ToggleFreezeTimeLabels();
			return networkUserControl;
		});

		var diagramArea = page.Locator(".diagramnode__diagramarea").First;
		await Assertions.Expect(diagramArea).ToHaveClassAsync("diagramnode__diagramarea schedulegrid_labels--frozen");
		var label = page.Locator(".schedulegrid_labels").First;
		Assert.That(await label.GetComputedStyleAsync("position"), Is.EqualTo("sticky"));

		await page.EvaluateAsync("() => window.scrollTo(0, 0)");
		await Assertions.Expect(label).ToBeInViewportAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task TestFrequentSelectionAndDeselectionOfNodesDoesnotThrowExceptionAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var entity = new Entity() { Name = "Test", X = 50, Y = 50, Width = 50, Height = 50 };
		var network = new NetworkBuilder().WithEntities(entity).Build();

		NetworkUserControl control = null!;
		var page = await ctx.LoadControlOnFormAsync(() => control = new NetworkUserControlBuilder().WithNetwork(network).Build());

		for (int i = 0; i < 10; i++)
		{
			await page.Mouse.ClickAsync(10, 50);

			var node = page.GetByTitle("Node: Test");
			await node.ClickAsync(new LocatorClickOptions { Position = new Position { X = 0, Y = 0 } });

			var nodeBoundingbox = await node.BoundingBoxAsync();
			Assert.That(nodeBoundingbox, Is.Not.Null);

			var x = nodeBoundingbox.X;
			var y = nodeBoundingbox.Y;

			await page.Mouse.MoveAsync(x + 1, y + 1);
			await page.Mouse.DownAsync();
			await page.Mouse.UpAsync();
		}

		await page.Locator(".networknode--indiagram[data-selected]").First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

		Assert.That(page.Locator(".networknode__resizer.topleft").First, Is.Not.Null);
		Assert.That(page.Locator(".networknode__resizer.topright").First, Is.Not.Null);
		Assert.That(page.Locator(".networknode__resizer.bottomleft").First, Is.Not.Null);
		Assert.That(page.Locator(".networknode__resizer.bottomright").First, Is.Not.Null);
	}

	[Test]
	public async Task DiagramAreaUserControlSelectShapesAsync()
	{
		var entity = Stub.Entity(e =>
		{
			e.X = 10;
			e.Y = 20;
			e.Width = 150;
			e.Height = 100;
		});
		NetworkUserControl networkUserControl = null!;
		NetworkViewModel networkViewModel = null!;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var network = new NetworkBuilder().WithEntities(entity).Build();
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkViewModel = networkUserControl.NetworkViewModel;
			return networkUserControl;
		});
		var actionResultMock = new Mock<INetworkActionResult>();
		actionResultMock.Setup(ar => ar.IsHandledByVisualiser).Returns(true);
		actionResultMock.As<INetworkEntityCollection>().Setup(ec => ec.Entities).Returns(new[] { entity });
		var actionMock = new Mock<INetworkAction>();
		actionMock.Setup(a => a.Execute()).Returns(actionResultMock.Object);

		Assert.That(networkViewModel.SelectedNodes.Count(), Is.EqualTo(0), "Should not have nodes selected.");
		await networkUserControl.InvokeWinzorDispatcherAsync(() => networkUserControl.ExecuteRibbonAction(actionMock.Object));
		Assert.That(networkViewModel.SelectedNodes.Count(), Is.EqualTo(1), "Should have one node selected.");
	}

	[TestCaseSource(nameof(TestData))]
	[Test, WithPlaywrightPage]
	public async Task FreezenTimeLabelAndFreezeChannelHeadersAvailabliltyWithDiagramsAsync(bool checkFreezeTimeLabels, bool isDiagramScaled, bool expectedValue)
	{
		NetworkUserControl? networkUserControl = null;
		DynamicGuiNetworkAction action;
		INetworkActionAccessibility isApplicable;
		var entity = Stub.Entity(e => { e.Width = 300; e.Height = 300; });
		var diagramEntity = Stub.Entity(e =>
		{
			e.SupportedActions = NetworkActions.StyleDiagram;
			e.IsDiagramScaled = isDiagramScaled;
			e.ShouldShowNonScheduledSection = false;
			e.ScaleUnitPixelSize = 50;
		});
		var network = new NetworkBuilder().WithEntities(entity).WithDiagramEntity(diagramEntity).Build();
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.MainDiagramControl.ViewModel.NetworkControlViewModel.ContentScale = 1;
			networkUserControl.MainDiagramControl.ToggleFreezeTimeLabels();
			return networkUserControl;
		});
		if (checkFreezeTimeLabels)
		{
			action = new FreezeChannelHeadersAction(networkUserControl?.NetworkViewModel, networkUserControl);
		}
		else
		{
			action = new FreezeTimeLabelsAction(networkUserControl?.NetworkViewModel, networkUserControl);
		}
		isApplicable = action.IsApplicableToEntity(entity);
		Assert.That(isApplicable!.IsAllowed, Is.EqualTo(expectedValue));
	}
	static IEnumerable<TestCaseData> TestData
	{
		get
		{
			yield return new TestCaseData(true, true, true);
			yield return new TestCaseData(true, false, false);
			yield return new TestCaseData(false, true, true);
			yield return new TestCaseData(false, false, false);
		}
	}

	[Test, WithPlaywrightPage]
	public async Task NumberOfTimeLabelColumnsShouldBeUnchangedAfterZoomOutThenZoomInAsync()
	{
		NetworkUserControl control = null!;
		await using var ctx = new InMemoryAppServerTestContext();

		var mockScaleDescriptor = new Mock<INetworkScaleDescriptor>();
		var mockScaleSet = new Mock<INetworkScaleSet>();
		var scalePoints = new List<INetworkScalePoint>();
		for (int i = 0; i < 10; i++)
		{
			var mockScalePoint = new Mock<INetworkScalePoint>();
			mockScalePoint.SetupGet(x => x.Label).Returns($"test-label{i}");
			scalePoints.Add(mockScalePoint.Object);
		}
		mockScaleSet.SetupGet(x => x.ScalePoints).Returns(scalePoints);

		mockScaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.Is<int>(i => i >= 0))).Returns(mockScaleSet.Object);

		var diagramEntity = new Entity()
		{
			SupportedActions = NetworkActions.StyleDiagram,
			IsDiagramScaled = true,
			IsNonScheduled = false
		};
		var refresher = new NetworkRefresher();
		var network = new NetworkBuilder().WithDiagramEntity(diagramEntity).Build();
		network.ScaleDescriptor = mockScaleDescriptor.Object;
		refresher.AssociateWithNetwork(network);

		var rendered = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			control = new NetworkUserControlBuilder().WithNetwork(network).Build();
			control.MainDiagramControl.ViewModel.ContentWidth = 1000;
			control.MainDiagramControl.ViewModel.ContentHeight = 600;
			form.Controls.Add(control);
			return form;
		});

		Assert.That(() => control.ViewModel.ContentScale, Is.EqualTo(1).After(3000, 100));
		Assert.That(() => rendered.Locator(".timescale_label_text").CountAsync(), Is.EqualTo(10).After(3000, 100));

		await control.InvokeWinzorDispatcherAsync(() => control.ViewModel.ContentScale = 0.5);
		Assert.That(() => control.ViewModel.ContentScale, Is.EqualTo(0.5).Within(0.0005).After(3000, 100));
		Assert.That(() => rendered.Locator(".timescale_label_text").CountAsync(), Is.EqualTo(5).After(3000, 100));

		await control.InvokeWinzorDispatcherAsync(() => control.ViewModel.ContentScale = 2);
		Assert.That(() => control.ViewModel.ContentScale, Is.EqualTo(2).Within(0.0005).After(3000, 100));
		Assert.That(() => rendered.Locator(".timescale_label_text").CountAsync(), Is.EqualTo(10).After(3000, 100));
	}

	[TestCaseSource(nameof(DefaultViewCases))]
	[TestCaseSource(nameof(ZoomedOutViewCases))]
	[TestCaseSource(nameof(ZoomedInViewCases))]
	[TestCaseSource(nameof(InitialLastNodePresentCases))]
	public async Task GetPositionForNewEntitiesAsync(ViewportRect viewport, Location initialLastNodeLocation, Location expectedLocationForNewNode)
	{
		using var ctx = new EnterpriseTestContext();
		NetworkUserControl? networkUserControl = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			networkUserControl = TestHelpers.SetupNetworkUserControl();
			return networkUserControl;
		});

		var diagramAreaUserControl = networkUserControl!.MainDiagramControl;
		var actualLocationForNewNode = diagramAreaUserControl.GetPositionForNewEntities(initialLastNodeLocation, viewport, new NetworkViewModel(new DummyNetwork()));

		Assert.That(actualLocationForNewNode.X, Is.EqualTo(expectedLocationForNewNode.X));
		Assert.That(actualLocationForNewNode.Y, Is.EqualTo(expectedLocationForNewNode.Y));
	}

	static IEnumerable<TestCaseData> DefaultViewCases
	{
		get
		{
			// ViewportRect x, y values considering contentZoom value as 1.0
			yield return new TestCaseData(new ViewportRect(0, 0, 1000, 1000), new Location(0, 0), new Location(30, 25)) { TestName = "{m}_DefaultViewAndTopLeftPosition" };
			yield return new TestCaseData(new ViewportRect(900, 0, 1000, 1000), new Location(0, 0), new Location(930, 25)) { TestName = "{m}_DefaultViewAndTopRightPosition" };
			yield return new TestCaseData(new ViewportRect(0, 900, 1000, 1000), new Location(0, 0), new Location(30, 925)) { TestName = "{m}_DefaultViewAndBottomLeftPosition" };
			yield return new TestCaseData(new ViewportRect(600, 600, 1000, 1000), new Location(0, 0), new Location(630, 625)) { TestName = "{m}_DefaultViewAndMiddlePosition" };
			yield return new TestCaseData(new ViewportRect(900, 900, 1000, 1000), new Location(0, 0), new Location(930, 925)) { TestName = "{m}_DefaultViewAndBottomRightPosition" };
		}
	}

	static IEnumerable<TestCaseData> ZoomedOutViewCases
	{
		get
		{
			// ViewportRect x, y values considering contentZoom value as 0.5
			yield return new TestCaseData(new ViewportRect(0, 0, 1000, 1000), new Location(0, 0), new Location(30, 25)) { TestName = "{m}_ZoomedOutViewAndTopLeftPosition" };
			yield return new TestCaseData(new ViewportRect(450, 0, 1000, 1000), new Location(0, 0), new Location(480, 25)) { TestName = "{m}_ZoomedOutViewAndTopRightPosition" };
			yield return new TestCaseData(new ViewportRect(0, 450, 1000, 1000), new Location(0, 0), new Location(30, 475)) { TestName = "{m}_ZoomedOutViewAndBottomLeftPosition" };
			yield return new TestCaseData(new ViewportRect(300, 300, 1000, 1000), new Location(0, 0), new Location(330, 325)) { TestName = "{m}_ZoomedOutViewAndMiddlePosition" };
			yield return new TestCaseData(new ViewportRect(450, 450, 1000, 1000), new Location(0, 0), new Location(480, 475)) { TestName = "{m}_ZoomedOutViewAndBottomRightPosition" };
		}
	}

	static IEnumerable<TestCaseData> ZoomedInViewCases
	{
		get
		{
			// ViewportRect x, y values considering contentZoom value as 1.5
			yield return new TestCaseData(new ViewportRect(0, 0, 1000, 1000), new Location(0, 0), new Location(30, 25)) { TestName = "{m}_ZoomedInViewAndTopLeftPosition" };
			yield return new TestCaseData(new ViewportRect(600, 0, 1000, 1000), new Location(0, 0), new Location(630, 25)) { TestName = "{m}_ZoomedInViewAndTopRightPosition" };
			yield return new TestCaseData(new ViewportRect(0, 600, 1000, 1000), new Location(0, 0), new Location(30, 625)) { TestName = "{m}_ZoomedInViewAndBottomLeftPosition" };
			yield return new TestCaseData(new ViewportRect(400, 400, 1000, 1000), new Location(0, 0), new Location(430, 425)) { TestName = "{m}_ZoomedInViewAndMiddlePosition" };
			yield return new TestCaseData(new ViewportRect(600, 600, 1000, 1000), new Location(0, 0), new Location(630, 625)) { TestName = "{m}_ZoomedInViewAndBottomRightPosition" };
		}
	}

	static IEnumerable<TestCaseData> InitialLastNodePresentCases
	{
		get
		{
			yield return new TestCaseData(new ViewportRect(900, 900, 1000, 1000), new Location(100, 25), new Location(1030, 950)) { TestName = "{m}_InitialLastNodePresentAndDefaultView" };
			yield return new TestCaseData(new ViewportRect(450, 450, 1000, 1000), new Location(100, 25), new Location(580, 500)) { TestName = "{m}_InitialLastNodePresentAndZoomedOutView" };
			yield return new TestCaseData(new ViewportRect(600, 600, 1000, 1000), new Location(100, 25), new Location(730, 650)) { TestName = "{m}_InitialLastNodePresentAndZoomedInView" };
		}
	}

	[Test, WithPlaywrightPage]
	public async Task ScrollToEntityWhenEntityIsOffScreenAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var entity = Stub.Entity(e => { e.JobNumber = "job1"; e.X = 1000; e.Y = 1100; });
		var network = new NetworkBuilder()
			.WithEntities(entity)
			.Build();

		SearchFinderViewModel searchFinder = null!;
		NetworkUserControl control = null!;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			control = new NetworkUserControlBuilder().WithNetwork(network).Build();
			searchFinder = new SearchFinderViewModel(control.NetworkViewModel);
			searchFinder.PerformSearch_ForTest("job1");

			return control;
		});

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		Assert.Multiple(() =>
		{
			Assert.That(searchFinder!.HasPerformedSearch, Is.True);
			Assert.That(control.NetworkViewModel.SelectedNodes.Count, Is.EqualTo(1));
			Assert.That(() => control.MainDiagramControl.ContentOffsetX, Is.EqualTo(1000).Within(20d).After(3000, 50));
			Assert.That(() => control.MainDiagramControl.ContentOffsetY, Is.EqualTo(1077).Within(20d).After(3000, 50));
		});
	}

	static IEnumerable<TestCaseData> ScrollToChannelWhenChannelIsSelected_TestData
	{
		get
		{
			yield return new TestCaseData(0, 200, 200, 100);
			yield return new TestCaseData(1, 800, 800, 600);
			yield return new TestCaseData(2, 500, 500, 1200);
		}
	}

	[TestCaseSource(nameof(ScrollToChannelWhenChannelIsSelected_TestData)), WithPlaywrightPage]
	public async Task ScrollToChannelWhenChannelIsSelectedAsync(int channelIndex, double contentOffsetX, double expectedContentOffsetX,
		double expectedContentOffsetY)
	{
		NetworkUserControl? networkUserControl = null;
		var entity1 = Stub.Entity(e => { e.X = 200; e.Y = 200; e.Width = 300; e.Height = 300; });
		var entity2 = Stub.Entity(e => { e.X = 800; e.Y = 700; e.Width = 300; e.Height = 300; });
		var entity3 = Stub.Entity(e => { e.X = 500; e.Y = 1100; e.Width = 300; e.Height = 300; });

		List<DummyChannel> channels = new List<DummyChannel>() { new("Top", 500),
		new DummyChannel("Middle", 600),
		new DummyChannel("Bottom", 700) };

		var diagramEntity = Stub.Entity(e =>
		{
			e.SupportedActions = NetworkActions.StyleDiagram;
			e.IsDiagramScaled = true;
			e.ShouldShowNonScheduledSection = false;
			e.ScaleUnitPixelSize = 100;
		});
		var network = new NetworkBuilder().WithEntities(new[] { entity1, entity2, entity3 }).WithDiagramEntity(diagramEntity).Build();

		await using var ctx = new InMemoryAppServerTestContext();

		await ctx.LoadControlOnFormAsync(() =>
		{
			network.DiagramEntity = new Entity() { SupportedActions = NetworkActions.StyleDiagram, IsDiagramScaled = true, DiagramChannels = channels };

			networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.MainDiagramControl.ViewModel.NetworkControlViewModel.ContentOffsetX = contentOffsetX;
			networkUserControl.MainDiagramControl.ViewModel.NetworkControlViewModel.ContentOffsetY = 100;
			return networkUserControl;
		});

		Assert.That(networkUserControl, Is.Not.Null);

		await networkUserControl.InvokeWinzorDispatcherAsync(() =>
		{
			networkUserControl.MainDiagramControl.OnChannelSelected(channels[channelIndex]);
		});

		Assert.That(() => networkUserControl.MainDiagramControl.ViewModel.ContentOffsetX, Is.EqualTo(expectedContentOffsetX).After(3000, 100));
		Assert.That(() => networkUserControl.MainDiagramControl.ViewModel.NetworkControlViewModel.ContentOffsetY, Is.EqualTo(expectedContentOffsetY).After(3000, 100));
	}
}
