using Blazor.Diagrams.Core.Anchors;
using Blazor.Diagrams.Core.Events;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.GUI.Components;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.Integration;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;

namespace NetworkVisualisation.GUI.Winzor.Test.Components;

class NCNDiagramTests : BunitTestContext
{
	[Test]
	public void NodesFromNodesSourceRenderedOnDiagram()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1", X = 10, Y = 11 };
		var entity2 = new Entity() { JobNumber = "job2", X = 20, Y = 22 };
		var entity3 = new Entity() { JobNumber = "job3", X = 30, Y = 33 };
		network.Entities.Add(entity1);
		network.Entities.Add(entity2);
		network.Entities.Add(entity3);
		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var nodes = diagram.Instance.DiagramModel!.Nodes.OfType<JobNodeModel>().OrderBy(n => n.JobNumber).ToArray();
		Assert.That(nodes[0].Position.X, Is.EqualTo(10));
		Assert.That(nodes[0].Position.Y, Is.EqualTo(11));
		Assert.That(nodes[1].Position.X, Is.EqualTo(20));
		Assert.That(nodes[1].Position.Y, Is.EqualTo(22));
		Assert.That(nodes[2].Position.X, Is.EqualTo(30));
		Assert.That(nodes[2].Position.Y, Is.EqualTo(33));

		var jobNodes = diagram.FindComponents<JobNode>();
		Assert.That(jobNodes, Has.Count.EqualTo(3));
	}

	[Test]
	public void NodeAddedToNodesSourceAddedToDiagram()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1", X = 10, Y = 11 };
		var entity2 = new Entity() { JobNumber = "job2", X = 20, Y = 22 };
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

		var jobNodes = diagram.FindComponents<JobNode>();
		Assert.That(jobNodes, Has.Count.EqualTo(2));

		NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkModel, new Entity() { JobNumber = "job3", X = 30, Y = 33 });

		jobNodes = diagram.FindComponents<JobNode>();
		Assert.That(jobNodes, Has.Count.EqualTo(3));
	}

	[Test]
	public void NodeRemovedFromNodeSourceRemovedFromDiagram()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1", X = 10, Y = 11 };
		var entity2 = new Entity() { JobNumber = "job2", X = 20, Y = 22 };
		var entity3 = new Entity() { JobNumber = "job3", X = 30, Y = 33 };
		network.Entities.Add(entity1);
		network.Entities.Add(entity2);
		network.Entities.Add(entity3);
		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var jobNodes = diagram.FindComponents<JobNode>();
		Assert.That(jobNodes, Has.Count.EqualTo(3));

		networkModel.DeleteNode(networkModel.GetNodeForEntity(entity3));

		jobNodes = diagram.FindComponents<JobNode>();
		Assert.That(jobNodes, Has.Count.EqualTo(2));
	}

	[Test]
	public async Task DeleteNodeFromViewModel_HideNotSupported_RemovesNodeAsync()
	{
		var network = new NetworkForTest();
		var entity1 = new Entity() { JobNumber = "job1", X = 10, Y = 11 };
		var entity2 = new Entity() { JobNumber = "job2", X = 20, Y = 22 };
		var entity3 = new Entity() { JobNumber = "job3", X = 30, Y = 33, CanDeleteUnderlyingEntity = true };
		network.Entities.Add(entity1);
		network.Entities.Add(entity2);
		network.Entities.Add(entity3);
		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var jobNodes = diagram.FindComponents<JobNode>();
		Assert.That(jobNodes, Has.Count.EqualTo(3));
		Assert.That(network.Entities, Does.Contain(entity3));
		Assert.That(network.HiddenEntities, Is.Empty);

		var nodeToDelete = diagram.Instance.DiagramModel!.Nodes.Last();
		await diagram.Instance.DiagramModel.DeleteNodeFromViewModelAsync(nodeToDelete);

		Assert.That(network.Entities, Does.Not.Contain(entity3));
		Assert.That(network.HiddenEntities, Is.Empty);
		jobNodes = diagram.FindComponents<JobNode>();
		Assert.That(jobNodes, Has.Count.EqualTo(2));
	}

	[Test]
	public async Task DeleteNodeFromViewModel_HideSupported_HidesNodeAsync()
	{
		var network = new NetworkForTest();
		var entity1 = new Entity() { JobNumber = "job1", X = 10, Y = 11 };
		var entity2 = new Entity() { JobNumber = "job2", X = 20, Y = 22 };
		var entity3 = new Entity() { JobNumber = "job3", X = 30, Y = 33, SupportedActions = NetworkActions.Hide };
		network.Entities.Add(entity1);
		network.Entities.Add(entity2);
		network.Entities.Add(entity3);
		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var jobNodes = diagram.FindComponents<JobNode>();
		Assert.That(jobNodes, Has.Count.EqualTo(3));
		Assert.That(network.Entities, Does.Contain(entity3));
		Assert.That(network.HiddenEntities, Is.Empty);

		var nodeToDelete = diagram.Instance.DiagramModel!.Nodes.Last();
		await diagram.Instance.DiagramModel.DeleteNodeFromViewModelAsync(nodeToDelete);

		Assert.That(network.Entities, Does.Not.Contain(entity3));
		Assert.That(network.HiddenEntities, Does.Contain(entity3));
		jobNodes = diagram.FindComponents<JobNode>();
		Assert.That(jobNodes, Has.Count.EqualTo(2));
	}

	[Test]
	public void LinksFromConnectionSourceRenderedOnDiagram()
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

		var networklink = diagram.Find(".networklink");
		Assert.That(networklink.GetAttribute("data-source"), Is.EqualTo(entity1.JobNumber));
		Assert.That(networklink.GetAttribute("data-target"), Is.EqualTo(entity2.JobNumber));
	}

	[Test]
	public void LinkAddedToConnectionsSourceAddedToDiagram()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1" };
		var entity2 = new Entity() { JobNumber = "job2" };
		var entity3 = new Entity() { JobNumber = "job3" };
		network.Entities.Add(entity1);
		network.Entities.Add(entity2);
		network.Entities.Add(entity3);
		network.CreateRelationship(entity1, entity2);

		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		Assert.That(diagram.FindAll(".networklink").Count, Is.EqualTo(1));

		entity3.Links.Add(new Relationship { From = entity3, To = entity2 });
		networkModel.AddRelationships(new [] { entity3 });

		Assert.That(diagram.FindAll(".networklink").Count, Is.EqualTo(2));
	}

	[Test]
	public void TestCircularLink()
	{
		var network = new Mock<DummyNetwork>() { CallBase = true };
		var entity1 = new Entity() { JobNumber = "job1" };
		var entity2 = new Entity() { JobNumber = "job2" };
		network.Object.Entities.Add(entity1);
		network.Object.Entities.Add(entity2);
		network.Object.CreateRelationship(entity1, entity2);

		var networkModel = new NetworkViewModel(network.Object);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		network.Setup(n => n.CreateRelationship(It.IsAny<Entity>(), It.IsAny<Entity>()))
			.Returns<Entity, Entity>((a, b) => null!);

		var jobNodes = diagram.Instance.DiagramModel!.Nodes.OfType<JobNodeModel>();
		var source = jobNodes.First(n => n.JobNumber == "job2");
		var target = jobNodes.First(n => n.JobNumber == "job1");

		var newLink = diagram.Instance.DiagramModel!.Options.Links.Factory(diagram.Instance.DiagramModel!,
			source.Output, new SinglePortAnchor(target.Input));
		diagram.Instance.DiagramModel!.Links.Add(newLink!);
		newLink!.TriggerTargetAttached();

		Assert.That(diagram.Instance.DiagramModel!.Links, Has.Count.EqualTo(1));
		Assert.That(networkModel.Connections.Count(), Is.EqualTo(1));
		Assert.That(entity1.Links, Has.Count.EqualTo(1));
		Assert.That(entity2.Links, Has.Count.EqualTo(1));
	}

	[Test]
	public void LinkRemovedFromConnectionsSourceRemovedFromDiagram()
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

		Assert.That(diagram.Instance.DiagramModel!.Links.Count, Is.EqualTo(1));

		networkModel.RemoveConnection(networkModel.Connections.First());

		Assert.That(networkModel.Connections.Count(), Is.EqualTo(0));
	}

	[Test]
	public void LinkAddedOnDiagramAddedToConnectionsSource()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity() { JobNumber = "job1" };
		var entity2 = new Entity() { JobNumber = "job2" };
		var entity3 = new Entity() { JobNumber = "job3" };
		network.Entities.Add(entity1);
		network.Entities.Add(entity2);
		network.Entities.Add(entity3);
		network.CreateRelationship(entity1, entity2);

		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		Assert.That(diagram.FindAll(".networklink").Count, Is.EqualTo(1));

		var jobNodes = diagram.FindComponents<JobNode>();
		var newLink = diagram.Instance.DiagramModel!.Options.Links.Factory(
			diagram.Instance.DiagramModel!,
			jobNodes[1].Instance.Node!.Ports[1],
			new SinglePortAnchor(jobNodes[2].Instance.Node!.Ports[0]));
		diagram.Instance.DiagramModel!.Links.Add(newLink!);
		newLink!.TriggerTargetAttached();

		Assert.That(networkModel.ScheduledConnections.Count, Is.EqualTo(2));
	}

	[Test]
	public async Task LinkRemovedFromDiagramRemovedFromConnectionsSourceAsync()
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

		Assert.That(diagram.Instance.DiagramModel!.Links.Count, Is.EqualTo(1));

		var linkToDelete = diagram.Instance.DiagramModel!.Links.Last();
		await diagram.Instance.DiagramModel.DeleteLinkFromViewModelAsync(linkToDelete);

		Assert.That(networkModel.Connections.Count(), Is.EqualTo(0));
	}

	[Test]
	public void TestCopyHotKeyInvokesTryCopyShapeFromClipboard()
	{
		var diagramEntity = new Entity();
		var entities = new ImpObservableSet<INetworkEntity>(new[] { new Entity() });
		var network = Mock.Of<INetwork>(x => x.Entities == entities && x.DiagramEntity == diagramEntity && x.EntityPositionStrategy == new EntityPositionStrategy());
		var networkViewModel = new NetworkViewModel(network);
		var userControl = new NetworkUserControlForTest(Renderer, networkViewModel);
		networkViewModel.BuildNetwork(isReloading: false);
		networkViewModel.Nodes.First().IsSelected = true;
		using var diagramModel = new NCNDiagramModel(networkViewModel.ScheduledNodes, networkViewModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		diagram.Instance.DiagramModel!.TriggerKeyDown(new KeyboardEventArgs("c", "", 0, true, false, false));
		Mock.Get(network).Verify(x => x.TryCopyShapeStateToClipBoard(It.IsAny<IEnumerable<INetworkEntity>>()));
	}

	[Test]
	public void TestPasteHotKeyInvokesTryCopyPasteShapeFromClipboard()
	{
		var diagramEntity = new Entity();
		var network = Mock.Of<INetwork>(x => x.Entities == new ImpObservableSet<INetworkEntity>() && x.DiagramEntity == diagramEntity);
		var networkViewModel = new NetworkViewModel(network);
		var userControl = new NetworkUserControlForTest(Renderer, networkViewModel)
		{
			JSRuntime = TestHelpers.GetMockedJSRuntime()
		};
		networkViewModel.BuildNetwork(isReloading: false);
		using var diagramModel = new NCNDiagramModel(networkViewModel.ScheduledNodes, networkViewModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		diagram.Instance.DiagramModel!.TriggerKeyDown(new KeyboardEventArgs("v", "", 0, true, false, false));
		Mock.Get(network).Verify(v => v.PasteShapeFromClipBoard(It.IsAny<NetworkViewModel>()));
	}

	[Test]
	public void TestDeleteHotKeyDeletesEntitiesFromNetwork()
	{
		var diagramEntity = new Entity();
		var entities = new ImpObservableSet<INetworkEntity>(new [] { new Entity(), new Entity(), new Entity() });
		var network = Mock.Of<INetwork>(x => x.Entities == entities && x.EntityPositionStrategy == new EntityPositionStrategy() && x.DiagramEntity == diagramEntity);
		var networkViewModel = new NetworkViewModel(network);
		var userControl = new NetworkUserControlForTest(Renderer, networkViewModel);
		networkViewModel.BuildNetwork(isReloading: false);
		using var diagramModel = new NCNDiagramModel(networkViewModel.ScheduledNodes, networkViewModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		foreach (var nodeViewModel in networkViewModel.Nodes)
		{
			nodeViewModel.IsSelected = true;
		}

		diagram.Instance.DiagramModel!.TriggerKeyDown(new KeyboardEventArgs("Delete", "", 0,  false, false, false));
		foreach (var entity in entities)
		{
			Mock.Get(network).Verify(x => x.DeleteEntity(entity));
		}
	}

	[TestCaseSource(nameof(ShiftArrowKeysShouldMoveSelectedNode_TestData))]
	public void ShiftArrowKeysShouldMoveSelectedNode(string arrowKey, int initialXcoordinate, int initialYcoordinate,int expectedXcoordinate,int expectedYcoordinate)
	{
		var diagramEntity = new Entity();
		var entities = new ImpObservableSet<INetworkEntity>(new[] { new Entity() { JobNumber = "job1", X = initialXcoordinate, Y = initialYcoordinate } });
		var network = Mock.Of<INetwork>(x => x.Entities == entities && x.DiagramEntity == diagramEntity && x.EntityPositionStrategy == new EntityPositionStrategy());
		var networkViewModel = new NetworkViewModel(network);
		var userControl = new NetworkUserControlForTest(Renderer, networkViewModel);
		networkViewModel.BuildNetwork(isReloading: false);
		networkViewModel.Nodes.First().IsSelected = true;
		using var diagramModel = new NCNDiagramModel(networkViewModel.ScheduledNodes, networkViewModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		diagram.Instance.DiagramModel!.TriggerKeyDown(new KeyboardEventArgs(arrowKey, "", 0, false, true, false));
		var finalPosition = diagram.Instance.DiagramModel!.Nodes.First().Position;

		Assert.That(finalPosition.X, Is.EqualTo(expectedXcoordinate));
		Assert.That(finalPosition.Y, Is.EqualTo(expectedYcoordinate));
	}

	static IEnumerable<TestCaseData> ShiftArrowKeysShouldMoveSelectedNode_TestData
	{
		get
		{
			yield return new TestCaseData("ArrowUp", 100, 100, 100,95);
			yield return new TestCaseData("ArrowDown", 200, 200, 200,205);
			yield return new TestCaseData("ArrowLeft", 300, 300, 295,300);
			yield return new TestCaseData("ArrowRight", 400, 400,405,400);
		}
	}

	[TestCaseSource(nameof(ShiftArrowKeysShouldMoveMultipleNodes_TestData))]
	public void ShiftArrowKeysShouldMoveMultipleNodes(string arrowKey, int expectedXcoordinate1, int expectedYcoordinate1, int expectedXcoordinate2, int expectedYcoordinate2)
	{
		var diagramEntity = new Entity();
		var entities = new ImpObservableSet<INetworkEntity>(new[] { new Entity() { JobNumber = "job1", X = 100, Y = 100 }, new Entity() { JobNumber = "job2", X = 200, Y = 200 } });
		var network = Mock.Of<INetwork>(x => x.Entities == entities && x.DiagramEntity == diagramEntity && x.EntityPositionStrategy == new EntityPositionStrategy());
		var networkViewModel = new NetworkViewModel(network);
		var userControl = new NetworkUserControlForTest(Renderer, networkViewModel);
		networkViewModel.BuildNetwork(isReloading: false);
		networkViewModel.Nodes.First().IsSelected = true;
		networkViewModel.Nodes.Last().IsSelected = true;
		using var diagramModel = new NCNDiagramModel(networkViewModel.ScheduledNodes, networkViewModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		diagram.Instance.DiagramModel!.TriggerKeyDown(new KeyboardEventArgs(arrowKey, "", 0, false, true, false));
		var finalPosition1 = diagram.Instance.DiagramModel!.Nodes.First().Position;
		var finalPosition2 = diagram.Instance.DiagramModel!.Nodes.Last().Position;

		Assert.That(finalPosition1.X, Is.EqualTo(expectedXcoordinate1));
		Assert.That(finalPosition1.Y, Is.EqualTo(expectedYcoordinate1));
		Assert.That(finalPosition2.X, Is.EqualTo(expectedXcoordinate2));
		Assert.That(finalPosition2.Y, Is.EqualTo(expectedYcoordinate2));
	}

	static IEnumerable<TestCaseData> ShiftArrowKeysShouldMoveMultipleNodes_TestData
	{
		get
		{
			yield return new TestCaseData("ArrowUp", 100, 95,200,195);
			yield return new TestCaseData("ArrowDown", 100, 105, 200, 205);
			yield return new TestCaseData("ArrowLeft", 95, 100, 195, 200);
			yield return new TestCaseData("ArrowRight", 105, 100, 205, 200);
		}
	}

	[Test]
	public void TestPortCompatibilityTestsReturnNullWhenNoLinkDraggingHasOccured()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity();
		var entity2 = new Entity();
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

		var ports = diagram.FindComponents<NetworkPort>();
		foreach (var port in ports)
		{
			var result = diagramModel.TryCanAttachToInProgressLink(port.Instance.Port);
			Assert.That(result, Is.Null);
		}
	}

	[Test]
	public void TestPortCompatibilityTestsAreAccurateWhenThereIsALinkToDrag()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity();
		var entity2 = new Entity();
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

		var newLink = diagram.Instance.DiagramModel!.Options.Links.Factory(diagram.Instance.DiagramModel!, diagram.Instance.DiagramModel!.Nodes.Last().Ports.Last(),
			new PositionAnchor(diagram.Instance.DiagramModel!.GetRelativeMousePoint(0, 0)));
		diagram.Instance.DiagramModel!.Links.Add(newLink!);
		Assert.That(diagram.Instance.DiagramModel!.Links.Count, Is.EqualTo(1));

		var ports = diagram.FindComponents<NetworkPort>();
		foreach (var  port in ports)
		{
			var result = diagramModel.TryCanAttachToInProgressLink(port.Instance.Port);
			var expected = newLink!.Source.Model!.CanAttachTo(port.Instance.Port);
			Assert.That(result, Is.EqualTo(expected));
		}
		diagram.Instance.DiagramModel!.Links.Add(newLink!);
	}

	[Test]
	public void TestPortCompatibilityTestsReturnNullAfterDraggedLinkIsAttached()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity();
		var entity2 = new Entity();
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

		var ports = diagram.FindComponents<NetworkPort>();
		var newLink = diagram.Instance.DiagramModel!.Options.Links.Factory(diagram.Instance.DiagramModel!, diagram.Instance.DiagramModel!.Nodes.Last().Ports.Last(),
			new PositionAnchor(diagram.Instance.DiagramModel!.GetRelativeMousePoint(0, 0)));
		diagram.Instance.DiagramModel!.Links.Add(newLink!);
		Assert.That(diagram.Instance.DiagramModel!.Links.Count, Is.EqualTo(1));
		newLink!.SetTarget(new SinglePortAnchor(diagram.Instance.DiagramModel!.Nodes.First().Ports.First()));
		newLink.TriggerTargetAttached();
		foreach (var  port in ports)
		{
			var result = diagramModel.TryCanAttachToInProgressLink(port.Instance.Port);
			Assert.That(result, Is.Null);
		}
	}

	[Test]
	public void TestPortCompatibilityTestsReturnNullAfterDraggedLinkIsRemoved()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity();
		var entity2 = new Entity();
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

		var ports = diagram.FindComponents<NetworkPort>();
		var newLink = diagram.Instance.DiagramModel!.Options.Links.Factory(diagram.Instance.DiagramModel!, diagram.Instance.DiagramModel!.Nodes.Last().Ports.Last(),
			new PositionAnchor(diagram.Instance.DiagramModel!.GetRelativeMousePoint(0, 0)));
		diagram.Instance.DiagramModel!.Links.Add(newLink!);
		Assert.That(diagram.Instance.DiagramModel!.Links.Count, Is.EqualTo(1));
		diagram.Instance.DiagramModel!.Links.Remove(newLink!);
		Assert.That(diagram.Instance.DiagramModel!.Links.Count, Is.EqualTo(0));
		foreach (var  port in ports)
		{
			var result = diagramModel.TryCanAttachToInProgressLink(port.Instance.Port);
			Assert.That(result, Is.Null);
		}
		diagram.Instance.DiagramModel!.Links.Add(newLink!);
	}

	[Test]
	public void TestDeleteLinkAsyncThrowsNotImplementedForUnrecognisedLinks()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity();
		network.Entities.Add(entity1);

		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});
		Assert.That(() => diagram.Instance.DiagramModel!.DeleteLinkFromViewModelAsync(new LinkModel(new PositionAnchor(Point.Zero), new PositionAnchor(Point.Zero))), Throws.TypeOf<NotImplementedException>());
	}

	[Test]
	public void TestDeleteLinkAsyncThrowsArgumentNullIfNoSourceGuid()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity();
		network.Entities.Add(entity1);

		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});
		var inputPort = diagram
			.FindComponents<NetworkPort>()
			.Select(c => c.Instance.Port)
			.Single(m => m.Type == NetworkPortModel.PortType.Input);
		Assert.That(() => diagram.Instance.DiagramModel!.DeleteLinkFromViewModelAsync(new NetworkLinkModel(new PositionAnchor(Point.Zero), new SinglePortAnchor(inputPort))), Throws.TypeOf<ArgumentNullException>());
	}

	[Test]
	public void TestDeleteLinkAsyncThrowsArgumentNullIfNoTargetGuid()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity();
		network.Entities.Add(entity1);

		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});
		var outputPort = diagram
			.FindComponents<NetworkPort>()
			.Select(c => c.Instance.Port)
			.Single(m => m.Type == NetworkPortModel.PortType.Output);
		Assert.That(() => diagram.Instance.DiagramModel!.DeleteLinkFromViewModelAsync(new NetworkLinkModel(new PositionAnchor(Point.Zero), new SinglePortAnchor(outputPort))), Throws.TypeOf<ArgumentNullException>());
	}

	[Test]
	public void TestDoesNotThrowWhenDiagramModelIsNull()
	{
		var network = new DummyNetwork();
		var entity1 = new Entity();
		network.Entities.Add(entity1);

		var networkModel = new NetworkViewModel(network);
		networkModel.BuildNetwork(isReloading: false);

		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});
		var ports = diagram
			.FindComponents<NetworkPort>()
			.Select(c => c.Instance.Port)
			.ToArray();
		var inputPort = ports.Single(m => m.Type == NetworkPortModel.PortType.Input);
		var outputPort = ports.Single(m => m.Type == NetworkPortModel.PortType.Output);
		Assert.That(() => diagram.Instance.DiagramModel!.DeleteLinkFromViewModelAsync(new NetworkLinkModel(inputPort, outputPort)), Throws.Nothing);
	}

	[Test]
	public void TestNewChildrenAreAddedToNode()
	{
		var networkModel = new NetworkViewModel(new DummyNetwork());
		var parentEntity = new Entity() { Width = 100, Height = 200 };

		var parentViewModel = new NodeViewModel(parentEntity, networkModel);
		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		networkModel.ScheduledNodes.Add(parentViewModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var jobNodes = diagram.FindComponents<JobNode>();
		var parentNode = jobNodes[0].Instance.Node!;

		Assert.That(parentNode.GetAllChildNodes().Count, Is.EqualTo(0));

		// create new child
		var childEntity = new Entity() { Width = 100, Height = 200, Parent = parentEntity };
		var childViewModel = new NodeViewModel(childEntity, networkModel);
		networkModel.ScheduledNodes.Add(childViewModel);

		Assert.That(parentNode.GetAllChildNodes().Count, Is.EqualTo(1));
	}
}
