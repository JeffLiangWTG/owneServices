using System.Windows.Forms;
using Blazor.Diagrams.Core.Anchors;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Models;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.Playwright;
using NetworkVisualisation.GUI.Winzor.Test.Extensions;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace NetworkVisualisation.GUI.Winzor.Test.Components;

using static PlaywrightTestContext;
class DeleteIconTest : BunitTestContext
{
	[Test]
	public void TestNodeDeleteShow()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1", X = 10, Y = 11 };
		network.Entities.Add(entity1);
		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);
		var control = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, control);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var node = diagram.Find(".jobnode--indiagram");
		node.TriggerEvent("onmouseenter", new WebMouseEventArgs());

		Assert.That(diagram.Find(".deleteicon").ClassList, Does.Not.Contain("deleteicon--hide"));
	}

	[Test]
	public void TestNodeDeleteHide()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1", X = 10, Y = 11 };
		network.Entities.Add(entity1);
		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var control = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, control);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var node = diagram.Find(".jobnode--indiagram");
		node.TriggerEvent("onmouseenter", new WebMouseEventArgs());
		node.TriggerEvent("onmouseleave", new WebMouseEventArgs());

		Assert.That(diagram.Find(".deleteicon").ClassList, Does.Contain("deleteicon--hide"));
	}

	[Test]
	public void TestNodeDeleteClick()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1", X = 10, Y = 11, CanDeleteUnderlyingEntity = true };
		var entity2 = new Entity() { JobNumber = "job1", X = 10, Y = 11, SupportedActions = CargoWise.NetworkVisualisation.Integration.NetworkActions.Hide };
		network.Entities.Add(entity1);
		network.Entities.Add(entity2);
		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var node = diagram.Find(".jobnode--indiagram");
		node.TriggerEvent("onmouseenter", new WebMouseEventArgs());

		Assert.That(diagram.FindAll(".deleteicon").Count, Is.EqualTo(2));
		diagram.Find(".deleteicon__deletecircle").Click();
		diagram.Find(".deleteicon__deleteimage").Click();
		Assert.Throws<ElementNotFoundException>(() => diagram.Find(".deleteicon"));
	}

	[Test]
	public void TestDeleteIconNotDraggable()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1", X = 10, Y = 11 };
		network.Entities.Add(entity1);
		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var node = diagram.Find(".jobnode--indiagram");
		node.TriggerEvent("onmouseenter", new WebMouseEventArgs());

		Assert.That(diagram.FindAll(".deleteicon").Count, Is.EqualTo(1));
		var deleteButtonImage = diagram.Find(".deleteicon__deleteimage");
		Assert.That(deleteButtonImage.GetAttribute("draggable"), Is.EqualTo("false"));
	}

	[Test]
	public void TestLinkDeleteShow()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1" };
		var entity2 = new Entity() { JobNumber = "job2" };
		network.Entities.Add(entity1);
		network.Entities.Add(entity2);
		network.CreateRelationship(entity1, entity2);

		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var link = diagram.Find(".networklink > path");
		link.TriggerEvent("onmouseenter", new WebMouseEventArgs());

		Assert.That(diagram.Find(".networklink .deleteicon").ClassList, Does.Not.Contain("deleteicon--hide"));
	}

	[Test]
	public void TestLinkDeleteHide()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1" };
		var entity2 = new Entity() { JobNumber = "job2" };
		network.Entities.Add(entity1);
		network.Entities.Add(entity2);
		network.CreateRelationship(entity1, entity2);

		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var link = diagram.Find(".networklink > path");
		link.TriggerEvent("onmouseenter", new WebMouseEventArgs());
		link.TriggerEvent("onmouseleave", new WebMouseEventArgs());

		Assert.That(diagram.Find(".networklink .deleteicon").ClassList, Does.Contain("deleteicon--hide"));
	}

	[Test]
	public void TestLinkDeleteClick()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1" };
		var entity2 = new Entity() { JobNumber = "job2" };
		var entity3 = new Entity() { JobNumber = "job2" };
		network.Entities.Add(entity1);
		network.Entities.Add(entity2);
		network.Entities.Add(entity3);
		network.CreateRelationship(entity1, entity2);
		network.CreateRelationship(entity2, entity3);

		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var link = diagram.Find(".networklink > path");
		link.TriggerEvent("onmouseenter", new WebMouseEventArgs());

		Assert.That(diagram.FindAll(".networklink .deleteicon").Count, Is.EqualTo(2));
		diagram.Find(".networklink .deleteicon__deletecircle").Click();
		diagram.Find(".networklink .deleteicon__deleteimage").Click();
		Assert.Throws<ElementNotFoundException>(() => diagram.Find(".networklink .deleteicon"));
	}

	[Test]
	public void TestLinkDoesNotThrowErrorOnMouseEnterIfContainerIsNull()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1" };
		var entity2 = new Entity() { JobNumber = "job2" };
		network.Entities.Add(entity1);
		network.Entities.Add(entity2);
		network.CreateRelationship(entity1, entity2);

		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		diagramModel.SetContainer(null);

		var link = diagram.Find(".networklink > path");

		Assert.DoesNotThrow(() => link.TriggerEvent("onmouseenter", new WebMouseEventArgs()));
	}

	[Test]
	public void TestInProgressLinkDoesNotRenderDeleteIconOnMouseEnter()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1" };
		network.Entities.Add(entity1);

		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var unattachedLink = diagramModel.Options.Links.Factory(diagramModel, diagramModel.Nodes.Last().Ports.Last(),
			new PositionAnchor(diagramModel.GetRelativeMousePoint(0, 0)))!;

		diagramModel.Links.Add(unattachedLink);

		var link = diagram.Find(".networklink > path");
		link.TriggerEvent("onmouseenter", new WebMouseEventArgs());

		Assert.That(diagram.Find(".networklink .deleteicon--hide"), Is.Not.Null);
	}
	[Test, WithPlaywrightPage]
	public async Task TestCorrectPointerUnderneathTrashcanIconAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var entity1 = new EntityForTest()
		{
			Name = "Node",
			X = 100,
			Y = 100,
			Height = 200,
			Width = 200,
		};

		var network = new NetworkBuilder().WithEntities(entity1).Build();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 1250, Height = 700 };
			var networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.Width = 1250;
			networkUserControl.Height = 700;

			form.Controls.Add(networkUserControl);
			return form;
		});

		var nodeLocator = page.GetByTitle("Node: Node");
		await nodeLocator.HoverAsync(new LocatorHoverOptions() { Force = true });
		Assert.That(await nodeLocator.GetComputedStyleAsync("cursor"), Is.EqualTo("move"));

		var trashcanLocator = page.GetByTitle("Remove shape from diagram.");
		var box = (await trashcanLocator.BoundingBoxAsync())!;
		Assert.That(box, Is.Not.Null);

		var elementUnderCursor = (await Page.EvaluateHandleAsync(
				$"() => document.elementFromPoint({box.X + box.Width / 2}, {box.Y + box.Height / 2})"
			))
			.AsElement()!;

		Assert.That(await elementUnderCursor.GetComputedStyleAsync("cursor"), Is.EqualTo("pointer"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestCorrectPointerUnderneathScissorIconAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var entity1 = new EntityForTest()
		{
			Name = "From Entity",
			X = 100,
			Y = 100,
			Height = 200,
			Width = 200,
		};
		var entity2 = new EntityForTest()
		{
			Name = "To Entity",
			X = 400,
			Y = 200,
			Height = 200,
			Width = 200,
		};

		var network = new NetworkBuilder().WithEntities(entity1, entity2).Build();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 1250, Height = 700 };
			var networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.Width = 1250;
			networkUserControl.Height = 700;
			networkUserControl.Link(entity1, entity2);

			form.Controls.Add(networkUserControl);
			return form;
		});

		var portsLocator = page.Locator(".networklink");
		await portsLocator.HoverAsync(new LocatorHoverOptions() { Force = true });

		var trashcanLocator = page.GetByTitle("Remove arrow from diagram and delete any underlying relationship.");
		var box = (await trashcanLocator.BoundingBoxAsync())!;
		Assert.That(box, Is.Not.Null);

		var elementUnderCursor = (await Page.EvaluateHandleAsync(
				$"() => document.elementFromPoint({box.X + box.Width / 2}, {box.Y + box.Height / 2})"
			))
			.AsElement()!;

		Assert.That(await elementUnderCursor.GetComputedStyleAsync("cursor"), Is.EqualTo("pointer"));
	}

	[Test, WithPlaywrightPage]
	public async Task DeleteIcon_ShouldBe_HiddenOn_MovingTheNodeAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var entity1 = Stub.Entity(e =>
		{
			e.Name = "Node 1";
			e.X = 100;
			e.Y = 100;
			e.Height = 200;
			e.Width = 200;
		});

		var entity2 = Stub.Entity(e =>
		{
			e.Name = "Node 2";
			e.X = 500;
			e.Y = 100;
			e.Height = 200;
			e.Width = 200;
		});

		var network = new NetworkBuilder().WithEntities(entity1, entity2 ).Build();

		var page = await ctx.LoadFormAsync(() =>
		{
			var nwuc = new NetworkUserControlBuilder().WithNetwork(network).Build();
			nwuc.Width = 1250;
			nwuc.Height = 700;
			nwuc.Link(entity1, entity2);

			var form = new Form() { Width = 1250, Height = 700 };
			form.Controls.Add(nwuc);
			return form;
		});

		var link = page.Locator("css=.networklink");
		await link!.HoverAsync(new LocatorHoverOptions() { Force = true });
		await Task.Delay(2000);

		var node = page.GetByTitle("Node: Node 1");
		await node!.ClickAsync();
		await page.Mouse.DownAsync();
		await page.Mouse.MoveAsync(500, 500, new MouseMoveOptions() { Steps = 20 });
		await page.Mouse.UpAsync();
		await Task.Delay(100);

		var linkDeleteIcon = page.Locator("css=.networklink > .deleteicon");
		await Assertions.Expect(linkDeleteIcon).ToBeHiddenAsync();

		var nodeDeleteIcon = page.Locator("css=.networknode--indiagram_wrapper > .deleteicon").First;
		await Assertions.Expect(nodeDeleteIcon).ToBeHiddenAsync();
	}
}
