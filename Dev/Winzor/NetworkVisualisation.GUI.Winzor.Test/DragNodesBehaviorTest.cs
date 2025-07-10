using System.Drawing;
using Blazor.Diagrams.Core.Events;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Components;
using CargoWise.NetworkVisualisation.GUI.Models;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.Playwright;
using NetworkVisualisation.GUI.Winzor.Test.Extensions;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using WTG.PlaywrightTesting;
using Entity = CargoWise.NetworkVisualisation.Business.Entity;

namespace NetworkVisualisation.GUI.Winzor.Test;

class DragNodesBehaviorTest : BunitTestContext
{
	[Test]
	public void TestDragNode()
	{
		var networkModel = new NetworkViewModel(new DummyNetwork());
		var entity = new Entity() { Width = 100, Height = 200 };
		var viewModel = new AnnotationViewModel(entity, networkModel);
		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		networkModel.ScheduledNodes.Add(viewModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var annotationNode = diagram.FindComponent<AnnotationNode>();
		var node = annotationNode.Instance.Node!;
		Assert.That(node.Position.X, Is.EqualTo(0));
		Assert.That(node.Position.Y, Is.EqualTo(0));

		diagram.Instance.DiagramModel!.TriggerPointerDown(node, Pointer.LeftButton.At(100, 50));
		Assert.That(node.Selected);
		diagram.Instance.DiagramModel.TriggerPointerMove(node, Pointer.LeftButton.At(300, 150));
		diagram.Instance.DiagramModel.TriggerPointerUp(node, Pointer.LeftButton);

		Assert.That(node.Position.X, Is.EqualTo(200));
		Assert.That(node.Position.Y, Is.EqualTo(100));
	}

	[Test, WithPlaywrightPage(Headless = true)]
	public async Task TestDragNodeWhileScrollingAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		NetworkForTest network = null!;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var entities = new[] {
				new Entity()
				{
					Name = "Test",
					JobName = "Test",
					Width = 200,
					Height = 200,
					X = 10,
					Y = 200,
				}
			};

			var diagramEntity = Stub.DiagramEntity(supportDiagramVisualStyles: true, isDiagramScaled: false, showNonscheduledSection: false);

			network = new NetworkBuilder().WithEntities(entities).WithDiagramEntity(diagramEntity).Build();

			var networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();

			return networkUserControl;
		});

		await page.EvaluateAsync(@"() => {
					const element = document.querySelector('.diagramareausercontrol');
					element.scrollBy(0, 300);
			}");

		var scrollTop = await page.EvaluateAsync<int>("() => document.querySelector('.diagramareausercontrol').scrollTop");
		Assert.That(() => scrollTop, Is.EqualTo(290).Within(4).After(3000, 100));

		var node = page.Locator(selector: "div.networknode--indiagram.jobnode--indiagram[title=\"Node: Test\"]");

		await node.WaitForAsync();

		var nodeBoundingBox = await node.BoundingBoxAsync();
		Assert.That(nodeBoundingBox, Is.Not.Null);
		var x = nodeBoundingBox.X + 10;
		var y = nodeBoundingBox.Y + 10;

		await page.Mouse.MoveAsync(x, y);
		await page.Mouse.DownAsync();
		await page.Mouse.MoveAsync(x + 1, y + 1);

		await page.Locator(".networknode--indiagram[data-selected]").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

		//Wait for mouse action to complete
		Assert.That(() => nodeBoundingBox.Y, Is.EqualTo(17).Within(4).After(3000, 100));

		await page.EvaluateAsync(@"() => {
					const element = document.querySelector('.diagramareausercontrol');
					element.scrollBy(0, -100); 
			}");

		// Wait for the scroll to complete
		scrollTop = await page.EvaluateAsync<int>("() => document.querySelector('.diagramareausercontrol').scrollTop");
		Assert.That(() => scrollTop, Is.EqualTo(190).Within(4).After(3000, 100));

		await page.Mouse.UpAsync();
		await page.Mouse.ClickAsync(0, 0);

		var boxAfterScroll = await node.BoundingBoxAsync();
		Assert.That(boxAfterScroll, Is.Not.Null);

		Assert.That(() => boxAfterScroll.Y, Is.EqualTo(expected: 17).Within(4).After(3000, 100));
	}

	[Test, WithPlaywrightPage(Headless = false)]
	public async Task TestDraggingNodeWhileScrollingDragsChildrenAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		NetworkForTest network = null!;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var parentEntity = new Entity()
			{
				Name = "Test",
				JobName = "Test",
				Width = 200,
				Height = 200,
				X = 10,
				Y = 200,
				CanHaveChildren = true
			};

			var childEntity = new Entity()
			{
				Name = "Test Child",
				JobName = "Test Child",
				Width = 100,
				Height = 100,
				X = 10,
				Y = 240,
			};
			var entities = new[] {
			parentEntity, childEntity
		};

			parentEntity.AddChildEntity(childEntity);

			var diagramEntity = Stub.DiagramEntity(supportDiagramVisualStyles: true, isDiagramScaled: false, showNonscheduledSection: false);

			network = new NetworkBuilder().WithEntities(entities).WithDiagramEntity(diagramEntity).Build();

			var networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();

			return networkUserControl;
		});

		await page.EvaluateAsync(@"() => {
					const element = document.querySelector('.diagramareausercontrol');
					element.scrollBy(0, 300);
			}");

		var scrollTop = await page.EvaluateAsync<int>("() => document.querySelector('.diagramareausercontrol').scrollTop");
		Assert.That(() => scrollTop, Is.EqualTo(290).Within(4).After(3000, 100));

		var parentNode = page.Locator(selector: "div.networknode--indiagram.jobnode--indiagram[title=\"Node: Test\"]");
		var childNode = page.Locator(selector: "div.networknode--indiagram.jobnode--indiagram[title=\"Node: Test Child\"]");

		await parentNode.WaitForAsync();
		await childNode.WaitForAsync();

		var parentBox = await parentNode.BoundingBoxAsync();
		var childBox = await childNode.BoundingBoxAsync();
		Assert.That(parentBox, Is.Not.Null);
		Assert.That(childBox, Is.Not.Null);

		var x = parentBox.X + 10;
		var y = parentBox.Y + 10;

		await page.Mouse.MoveAsync(x, y);

		await page.Mouse.DownAsync();
		await page.Mouse.MoveAsync(x + 1, y + 1);

		await page.Locator(".networknode--indiagram[data-selected]").First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

		//Wait for mouse action to complete
		Assert.That(() => parentBox.Y, Is.EqualTo(17).Within(4).After(3000, 100));
		Assert.That(() => childBox.Y, Is.EqualTo(57).Within(4));

		await page.EvaluateAsync(@"() => {
					const element = document.querySelector('.diagramareausercontrol');
					element.scrollBy(0, -100); 
			}");

		// Wait for the scroll to complete
		scrollTop = await page.EvaluateAsync<int>("() => document.querySelector('.diagramareausercontrol').scrollTop");
		Assert.That(() => scrollTop, Is.EqualTo(190).Within(4).After(3000, 100));

		await page.Mouse.UpAsync();
		await page.Mouse.ClickAsync(0, 0);

		var parentBoxAfterScroll = await parentNode.BoundingBoxAsync();
		var childBoxAfterScroll = await childNode.BoundingBoxAsync();
		Assert.That(parentBoxAfterScroll, Is.Not.Null);
		Assert.That(childBoxAfterScroll, Is.Not.Null);

		Assert.That(() => parentBoxAfterScroll.Y, Is.EqualTo(17).Within(4).After(3000, 100));
		Assert.That(childBoxAfterScroll.Y, Is.EqualTo(57).Within(4));
	}

	[Test]
	public void TestDraggingNodeDragsChildren()
	{
		var networkModel = new NetworkViewModel(new DummyNetwork());
		var parentEntity = new Entity() { Width = 200, Height = 400, X = 0, Y = 0 };
		var childEntity = new Entity() { Width = 100, Height = 200, X = 0, Y = 50 };
		var child2Entity = new Entity() { Width = 50, Height = 100, X = 0, Y = 100 };

		parentEntity.AddChildEntity(childEntity);
		childEntity.AddChildEntity(child2Entity);
		var parentViewModel = new NodeViewModel(parentEntity, networkModel);
		var childViewModel = new NodeViewModel(childEntity, networkModel);
		var child2ViewModel = new NodeViewModel(child2Entity, networkModel);
		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		networkModel.ScheduledNodes.Add(parentViewModel);
		networkModel.ScheduledNodes.Add(childViewModel);
		networkModel.ScheduledNodes.Add(child2ViewModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var jobNodes = diagram.FindComponents<JobNode>();
		var parentNode = jobNodes[0].Instance.Node!;
		var childNode = jobNodes[1].Instance.Node!;
		var child2Node = jobNodes[2].Instance.Node!;

		Assert.That(parentNode.Position.X, Is.EqualTo(0));
		Assert.That(parentNode.Position.Y, Is.EqualTo(0));
		Assert.That(childNode.Position.X, Is.EqualTo(0));
		Assert.That(childNode.Position.Y, Is.EqualTo(50));
		Assert.That(child2Node.Position.X, Is.EqualTo(0));
		Assert.That(child2Node.Position.Y, Is.EqualTo(100));

		var initialPointerState = Pointer.LeftButton.At(150, 50);

		diagram.Instance.DiagramModel!.TriggerPointerDown(parentNode, initialPointerState);

		Assert.That(parentNode.Selected);
		Assert.That(!childNode.Selected);
		Assert.That(!child2Node.Selected);

		diagram.Instance.DiagramModel.TriggerPointerMove(parentNode, initialPointerState.MovedBy(150, 100));

		Assert.That(childNode.Selected);
		Assert.That(child2Node.Selected);

		diagram.Instance.DiagramModel.TriggerPointerUp(parentNode, Pointer.LeftButton);

		Assert.That(parentNode.Position.X, Is.EqualTo(150));
		Assert.That(parentNode.Position.Y, Is.EqualTo(100));

		Assert.That(childNode.Position.X, Is.EqualTo(150));
		Assert.That(childNode.Position.Y, Is.EqualTo(150));

		Assert.That(child2Node.Position.X, Is.EqualTo(150));
		Assert.That(child2Node.Position.Y, Is.EqualTo(200));
	}

	[Test]
	public void TestChildStaysWithinParent()
	{
		var networkModel = new NetworkViewModel(new DummyNetwork());
		var parentEntity = new Entity() { Width = 200, Height = 400, X = 0, Y = 0 };
		var childEntity = new Entity() { Width = 100, Height = 200, X = 0, Y = 50 };

		parentEntity.AddChildEntity(childEntity);
		var parentViewModel = new NodeViewModel(parentEntity, networkModel);
		var childViewModel = new NodeViewModel(childEntity, networkModel);
		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		networkModel.ScheduledNodes.Add(parentViewModel);
		networkModel.ScheduledNodes.Add(childViewModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var jobNodes = diagram.FindComponents<JobNode>();
		var parentNode = jobNodes[0].Instance.Node!;
		var childNode = jobNodes[1].Instance.Node!;

		Assert.That(parentNode.Position.X, Is.EqualTo(0));
		Assert.That(parentNode.Position.Y, Is.EqualTo(0));
		Assert.That(childNode.Position.X, Is.EqualTo(0));
		Assert.That(childNode.Position.Y, Is.EqualTo(50));

		diagram.Instance.DiagramModel!.TriggerPointerDown(childNode, Pointer.LeftButton.At(25, 60));

		Assert.That(!parentNode.Selected);
		Assert.That(childNode.Selected);

		diagram.Instance.DiagramModel.TriggerPointerMove(childNode, Pointer.LeftButton.At(200, 250));

		diagram.Instance.DiagramModel.TriggerPointerUp(childNode, Pointer.LeftButton);

		Assert.That(parentNode.Position.X, Is.EqualTo(0));
		Assert.That(parentNode.Position.Y, Is.EqualTo(0));

		Assert.That(childNode.Position.X, Is.EqualTo(100));
		Assert.That(childNode.Position.Y, Is.EqualTo(195));

		diagram.Instance.DiagramModel!.TriggerPointerDown(childNode, Pointer.LeftButton.At(125, 200));

		Assert.That(!parentNode.Selected);
		Assert.That(childNode.Selected);

		diagram.Instance.DiagramModel.TriggerPointerMove(parentNode, Pointer.LeftButton.At(0, 0));

		diagram.Instance.DiagramModel.TriggerPointerUp(parentNode, Pointer.LeftButton);

		Assert.That(parentNode.Position.X, Is.EqualTo(0));
		Assert.That(parentNode.Position.Y, Is.EqualTo(0));

		Assert.That(childNode.Position.X, Is.EqualTo(0));
		Assert.That(childNode.Position.Y, Is.EqualTo(40));
	}

	[Test]
	public void TestAnnotationChildStaysWithinParent()
	{
		var networkModel = new NetworkViewModel(new DummyNetwork());
		var parentEntity = new Entity() { Width = 200, Height = 400, X = 0, Y = 0 };
		var childEntity = new Entity() { Width = 100, Height = 200, X = 0, Y = 50 };

		parentEntity.AddChildEntity(childEntity);
		var parentViewModel = new NodeViewModel(parentEntity, networkModel);
		var childViewModel = new AnnotationViewModel(childEntity, networkModel);
		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		networkModel.ScheduledNodes.Add(parentViewModel);
		networkModel.ScheduledNodes.Add(childViewModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var jobNodes = diagram.FindComponents<JobNode>();
		var parentNode = jobNodes[0].Instance.Node!;

		var annotationNodes = diagram.FindComponents<AnnotationNode>();
		var childNode = annotationNodes[0].Instance.Node!;

		Assert.That(parentNode.Position.X, Is.EqualTo(0));
		Assert.That(parentNode.Position.Y, Is.EqualTo(0));
		Assert.That(childNode.Position.X, Is.EqualTo(0));
		Assert.That(childNode.Position.Y, Is.EqualTo(50));

		diagram.Instance.DiagramModel!.TriggerPointerDown(childNode, Pointer.LeftButton.At(25, 60));

		Assert.That(!parentNode.Selected);
		Assert.That(childNode.Selected);

		diagram.Instance.DiagramModel.TriggerPointerMove(childNode, Pointer.LeftButton.At(200, 250));

		diagram.Instance.DiagramModel.TriggerPointerUp(childNode, Pointer.LeftButton);

		Assert.That(parentNode.Position.X, Is.EqualTo(0));
		Assert.That(parentNode.Position.Y, Is.EqualTo(0));

		Assert.That(childNode.Position.X, Is.EqualTo(100));
		Assert.That(childNode.Position.Y, Is.EqualTo(195));

		diagram.Instance.DiagramModel!.TriggerPointerDown(childNode, Pointer.LeftButton.At(125, 200));

		Assert.That(!parentNode.Selected);
		Assert.That(childNode.Selected);

		diagram.Instance.DiagramModel.TriggerPointerMove(parentNode, Pointer.LeftButton.At(0, 0));

		diagram.Instance.DiagramModel.TriggerPointerUp(parentNode, Pointer.LeftButton);

		Assert.That(parentNode.Position.X, Is.EqualTo(0));
		Assert.That(parentNode.Position.Y, Is.EqualTo(0));

		Assert.That(childNode.Position.X, Is.EqualTo(0));
		Assert.That(childNode.Position.Y, Is.EqualTo(40));
	}

	[Test]
	public void TestChildStopsWhenParentStops()
	{
		var networkModel = new NetworkViewModel(new DummyNetwork());
		var parentEntity = new Entity() { Width = 400, Height = 400, X = 0, Y = 0 };
		var childEntity = new Entity() { Width = 200, Height = 200, X = 0, Y = 100 };
		var child2Entity = new Entity() { Width = 100, Height = 100, X = 0, Y = 150 };

		parentEntity.AddChildEntity(childEntity);
		childEntity.AddChildEntity(child2Entity);
		var parentViewModel = new NodeViewModel(parentEntity, networkModel);
		var childViewModel = new NodeViewModel(childEntity, networkModel);
		var child2ViewModel = new NodeViewModel(child2Entity, networkModel);
		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		networkModel.ScheduledNodes.Add(parentViewModel);
		networkModel.ScheduledNodes.Add(childViewModel);
		networkModel.ScheduledNodes.Add(child2ViewModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var jobNodes = diagram.FindComponents<JobNode>();
		var parentNode = jobNodes[0].Instance.Node!;
		var childNode = jobNodes[1].Instance.Node!;
		var child2Node = jobNodes[2].Instance.Node!;

		diagram.Instance.DiagramModel!.TriggerPointerDown(childNode, CreatePointerEventArgs(150, 120));
		diagram.Instance.DiagramModel.TriggerPointerMove(childNode, CreatePointerEventArgs(1000, 120));
		diagram.Instance.DiagramModel.TriggerPointerUp(childNode, CreatePointerEventArgs(1150, 120));

		Assert.That(parentNode.Position.X, Is.EqualTo(0));
		Assert.That(parentNode.Position.Y, Is.EqualTo(0));

		Assert.That(childNode.Position.X, Is.EqualTo(200));
		Assert.That(childNode.Position.Y, Is.EqualTo(100));

		Assert.That(child2Node.Position.X, Is.EqualTo(200));
		Assert.That(child2Node.Position.Y, Is.EqualTo(150));
	}

	[Test]
	public async Task ScaledDiagram_ChannelsUpdatesChildNodesColorAndPositionCorrectly_AfterDraggedAsync()
	{
		NetworkUserControl networkUserControl = null!;
		using var ctx = new EnterpriseTestContext();
		var parentEntity = new Entity() { Width = 200, Height = 500, X = 400, Y = 100 };
		var childEntity = new Entity() { Width = 150, Height = 150, X = 400, Y = 300 };
		parentEntity.AddChildEntities(new[] { childEntity });

		var refresher = new NetworkRefresher();
		var network = new DummyNetwork { Refresher = refresher };
		network.Entities.Add(parentEntity);
		network.Entities.Add(childEntity);
		refresher.AssociateWithNetwork(network);

		var channel1 = new DummyChannel("Top", 600, Color.Blue);
		var channel2 = new DummyChannel("Lower", 500, Color.Red);

		network.DiagramEntity = new Entity() { IsDiagramScaled = true, DiagramChannels = new[] { channel1, channel2 }, ScaleUnitPixelSize = 100 };

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			networkUserControl = new NetworkUserControl(network.DiagramEntity, refresher);
			networkUserControl.SetDataContext(network, false);
			return networkUserControl;
		});

		var jobNode = rendered.FindComponent<InDiagramNetworkNode<JobNodeModel, JobNode>>();
		var parentJobNodeModel = jobNode.Instance.Node!;
		var childNodeModel = parentJobNodeModel.GetAllChildNodes().First();
		var diagramModel = jobNode.Instance.Diagram!;

		Assert.That(() => parentJobNodeModel.StatusColors.colors.First().color, Is.EqualTo(Color.Blue).After(3000, 100));
		Assert.That(() => childNodeModel.StatusColors.colors.First().color, Is.EqualTo(Color.Blue).After(3000, 100));

		var initialPointerState = Pointer.LeftButton.At(200, 120);
		diagramModel!.TriggerPointerDown(parentJobNodeModel, initialPointerState);

		Assert.That(parentJobNodeModel.Selected);

		diagramModel.TriggerPointerMove(parentJobNodeModel, initialPointerState.MovedBy(230, 600));
		diagramModel!.TriggerPointerUp(parentJobNodeModel, Pointer.LeftButton);

		Assert.That(() => parentJobNodeModel.Position.X, Is.EqualTo(600).After(3000, 100));
		Assert.That(() => parentJobNodeModel.Position.Y, Is.EqualTo(700).After(3000, 100));
		Assert.That(() => parentJobNodeModel.StatusColors.colors.First().color, Is.EqualTo(Color.Red).After(3000, 100));
		Assert.That(() => childNodeModel.Position.X, Is.EqualTo(600).After(3000, 100));
		Assert.That(() => childNodeModel.Position.Y, Is.EqualTo(900).After(3000, 100));
		Assert.That(() => childNodeModel.StatusColors.colors.First().color, Is.EqualTo(Color.Red).After(3000, 100));
	}

	PointerEventArgs CreatePointerEventArgs(double clientX, double clientY)
	{
		return new PointerEventArgs(clientX, clientY, 0, 0, false, false, false, 0, 0, 0, 0, 0, 0, string.Empty, true);
	}
}
