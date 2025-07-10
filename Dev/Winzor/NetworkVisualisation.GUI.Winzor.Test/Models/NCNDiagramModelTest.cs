using System.Collections.ObjectModel;
using Blazor.Diagrams.Components;
using Blazor.Diagrams.Core.Anchors;
using Blazor.Diagrams.Core.Events;
using Bunit.Rendering;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.Winzor.Architecture.Test;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;

namespace NetworkVisualisation.GUI.Winzor.Test.Models;

class NCNDiagramModelTest : BunitTestContext
{
	[TearDown]
	public void AfterEach()
	{
		ExceptionReporterTestListener.Instance.Clear();
	}

	[Test]
	public void TestResetNodes()
	{
		var node1 = CreateNode();
		var node2 = CreateNode();
		var (model, nodes, _) = CreateDiagramModel(Renderer, new[] { node1, node2 });

		Assert.That(model.Nodes, Is.Not.Empty);
		nodes.Clear();
		Assert.That(model.Nodes, Is.Empty);
	}

	[Test]
	public void TestResetConnections()
	{
		var node1 = CreateNode();
		var node2 = CreateNode();
		var connection = CreateConnection(node1, node2);
		var (model, _, connections) = CreateDiagramModel(Renderer, new[] { node1, node2 }, new[] { connection });

		Assert.That(model.Links, Is.Not.Empty);
		connections.Clear();
		Assert.That(model.Links, Is.Empty);
	}

	[Test]
	public void TestAddNode()
	{
		var (model, nodes, _) = CreateDiagramModel(Renderer);
		var node = CreateNode();

		nodes.Add(node);
		Assert.That(model.Nodes.Count, Is.EqualTo(1));
		Assert.That(model.Nodes.OfType<NetworkNodeModel>().First().EntityPK, Is.EqualTo(node.Entity.EntityPK));
	}

	[Test]
	public void TestRemoveNode()
	{
		var (model, nodes, _) = CreateDiagramModel(Renderer);
		var node = CreateNode();

		nodes.Add(node);
		Assert.That(model.Nodes.Count, Is.EqualTo(1));
		Assert.That(model.Nodes.OfType<NetworkNodeModel>().First().EntityPK, Is.EqualTo(node.Entity.EntityPK));

		nodes.Remove(node);
		Assert.That(model.Nodes.Count, Is.EqualTo(0));
	}

	[Test]
	public void TestUpdateSources()
	{
		var (model, nodes, connections) = CreateDiagramModel(Renderer, new[] { CreateNode() });

		Assert.Multiple(() =>
		{
			Assert.That(nodes, Has.Count.EqualTo(1));
			Assert.That(connections, Is.Empty);
		});

		var os = new ObservableCollection<NodeViewModel>(nodes);
		os.Append(CreateNode());

		var cs = new ObservableCollection<ConnectionViewModel>(connections);
		cs.Append(CreateConnection(os.First(), os.Last()));

		model.UpdateSources(os, cs);

		Assert.Multiple(() =>
		{
			Assert.That(model.Nodes, Has.Count.EqualTo(os.Count));
			Assert.That(model.Links, Has.Count.EqualTo(cs.Count));
		});
	}

	[Test]
	public void TestAddLink()
	{
		var (model, nodes, connections) = CreateDiagramModel(Renderer);

		var sourceNode = CreateNode();
		var targetNode = CreateNode();
		var connection = CreateConnection(sourceNode, targetNode);

		nodes.Add(sourceNode);
		nodes.Add(targetNode);
		connections.Add(connection);
		Assert.That(model.Links.Count, Is.EqualTo(1));
		Assert.That(((NetworkLinkModel)model!.Links!.First()).SourceEntityPK, Is.EqualTo(sourceNode.Entity.EntityPK));
		Assert.That(((NetworkLinkModel)model!.Links!.First()).TargetEntityPK, Is.EqualTo(targetNode.Entity.EntityPK));
	}

	[Test]
	public void TestRemoveLink()
	{
		var (model, nodes, connections) = CreateDiagramModel(Renderer);

		var sourceNode = CreateNode();
		var targetNode = CreateNode();
		var connection = CreateConnection(sourceNode, targetNode);

		nodes.Add(sourceNode);
		nodes.Add(targetNode);
		connections.Add(connection);

		Assert.That(connections.Count, Is.EqualTo(1));
		Assert.That(((NetworkLinkModel)model!.Links!.First()).SourceEntityPK, Is.EqualTo(sourceNode.Entity.EntityPK));
		Assert.That(((NetworkLinkModel)model!.Links!.First()).TargetEntityPK, Is.EqualTo(targetNode.Entity.EntityPK));

		connections.Remove(connection);
		Assert.That(connections.Count, Is.EqualTo(0));
		Assert.That(model.Links.Count, Is.EqualTo(0));
	}

	[Test]
	public async Task TestCopySelectionToClipboardAsync()
	{
		var entity = new Entity();
		var entities = new ImpObservableSet<INetworkEntity>(new[] { entity });
		var network = Mock.Of<INetwork>(x =>
			x.Entities == entities &&
			x.EntityPositionStrategy == new EntityPositionStrategy() &&
			x.DiagramEntity == new Entity());
		var networkViewModel = new NetworkViewModel(network);
		var userControl = new NetworkUserControlForTest(Renderer, networkViewModel);

		networkViewModel.BuildNetwork(isReloading: false);

		networkViewModel.Nodes.First().IsSelected = true;

		using var model = new NCNDiagramModel(networkViewModel.ScheduledNodes, networkViewModel.ScheduledConnections, userControl);

		await model.CopySelectionToClipboardAsync();
		Mock.Get(network).Verify(x => x.TryCopyShapeStateToClipBoard(It.Is<IEnumerable<INetworkEntity>>(
			l => l.Contains(entity) && l.Count() == 1
		)));
	}

	[Test]
	public async Task TestDuplicateLinksAreNotCreatedAsync()
	{
		using var ctx = new EnterpriseTestContext();
		NetworkUserControl? networkUserControl = null;
		var network = new NetworkBuilder().WithEntities(new Entity(), new Entity()).Build();
		var rendered = await ctx.RenderControlOnFormAsync(() => networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build());

		Assert.That(networkUserControl, Is.Not.Null);
		var ncnDiagramModel = rendered.FindComponent<DiagramCanvas>()?.Instance.BlazorDiagram as NCNDiagramModel;
		Assert.That(ncnDiagramModel, Is.Not.Null);

		var link = ncnDiagramModel!.Options.Links.Factory.Invoke(ncnDiagramModel, ((LinkableNodeModel)(ncnDiagramModel.Nodes[0])).Output, new SinglePortAnchor(((LinkableNodeModel)(ncnDiagramModel.Nodes[1])).Input))!;
		ncnDiagramModel.Links.Add(link);
		link!.TriggerTargetAttached();

		rendered.WaitForState(() => !ncnDiagramModel.Links.Contains(link));

		Assert.That(() => ncnDiagramModel!.Links.Count, Is.EqualTo(1));
		Assert.That(ncnDiagramModel.Links[0], Is.Not.EqualTo(link));
	}

	[Test]
	public async Task TestExistingConnectionIsDeletedOnAddingNewConnectionAsync()
	{
		using var ctx = new EnterpriseTestContext();
		NetworkUserControl? networkUserControl = null;
		var network = new NetworkBuilder().WithEntities(new Entity(), new Entity()).Build();
		var rendered = await ctx.RenderControlOnFormAsync(() => networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build());

		Assert.That(networkUserControl, Is.Not.Null);
		var ncnDiagramModel = rendered.FindComponent<DiagramCanvas>()?.Instance.BlazorDiagram as NCNDiagramModel;
		Assert.That(ncnDiagramModel, Is.Not.Null);

		var scheduledConnections = networkUserControl!.MainDiagramControl.NetworkViewModel.ScheduledConnections;

		var link = ncnDiagramModel!.Options.Links.Factory.Invoke(ncnDiagramModel, ((LinkableNodeModel)(ncnDiagramModel.Nodes[0])).Output, new SinglePortAnchor(((LinkableNodeModel)(ncnDiagramModel.Nodes[1])).Input))!;
		ncnDiagramModel.Links.Add(link);
		link!.TriggerTargetAttached();

		rendered.WaitForState(() => !ncnDiagramModel.Links.Contains(link));

		Assert.That(() => scheduledConnections.Count, Is.EqualTo(1));
		var oldConnectionViewModel = scheduledConnections[0];

		link = ncnDiagramModel!.Options.Links.Factory.Invoke(ncnDiagramModel, ((LinkableNodeModel)(ncnDiagramModel.Nodes[0])).Output, new SinglePortAnchor(((LinkableNodeModel)(ncnDiagramModel.Nodes[1])).Input))!;
		ncnDiagramModel.Links.Add(link);
		link!.TriggerTargetAttached();

		rendered.WaitForState(() => !scheduledConnections.Contains(oldConnectionViewModel)
									&& scheduledConnections.Count == 1);

		Assert.That(() => scheduledConnections.Count, Is.EqualTo(1));
		Assert.That(() => scheduledConnections[0], Is.Not.EqualTo(oldConnectionViewModel));
	}

	[Test]
	public async Task TestPasteFromClipboardAsync()
	{
		var network = Mock.Of<INetwork>(x => x.Entities == new ImpObservableSet<INetworkEntity>());
		var networkViewModel = new NetworkViewModel(network);
		var userControl = new NetworkUserControlForTest(Renderer, networkViewModel)
		{
			JSRuntime = TestHelpers.GetMockedJSRuntime()
		};
		using var model = new NCNDiagramModel(networkViewModel.ScheduledNodes, networkViewModel.ScheduledConnections, userControl);

		networkViewModel.BuildNetwork(isReloading: false);

		await model.PasteFromClipboardAsync();
	}
	[Test]
	public async Task TestFitNodesAfterPasteAsync()
	{
		var network = Mock.Of<INetwork>(x => x.Entities == new ImpObservableSet<INetworkEntity>());
		var networkViewModel = new NetworkViewModel(network);

		var mock = new Mock<NetworkUserControlForTest>(Renderer, networkViewModel) { CallBase = true };
		mock.Object.JSRuntime = TestHelpers.GetMockedJSRuntime();
		mock.Setup(n => n.FitNodes()).Verifiable();

		using var model = new NCNDiagramModel(networkViewModel.ScheduledNodes, networkViewModel.ScheduledConnections, mock.Object);
		await model.PasteFromClipboardAsync();
		mock.Verify(n => n.FitNodes(), Times.Once);
	}

	[Test]
	public void TestPasteFromClipboardWhenJSRuntimeIsNull()
	{
		var diagramEntityMock = new Mock<IDiagramEntity>();
		diagramEntityMock.SetupGet(x => x.Name).Returns("Diagram Entity Name");
		diagramEntityMock.SetupGet(x => x.Status).Returns(WorkStatus.Suspended);

		var networkMock = new Mock<INetwork>();
		networkMock.SetupGet(x => x.DiagramEntity).Returns(diagramEntityMock.Object);
		networkMock.SetupGet(x => x.Entities).Returns(new ImpObservableSet<INetworkEntity>());

		var networkViewModel = new NetworkViewModel(networkMock.Object);
		var userControl = new NetworkUserControlForTest(Renderer, networkViewModel)
		{
			JSRuntime = null,
		};

		using var model = new NCNDiagramModel(networkViewModel.ScheduledNodes, networkViewModel.ScheduledConnections, userControl);

		networkViewModel.BuildNetwork(isReloading: false);

		Assert.DoesNotThrowAsync(async () => await model.PasteFromClipboardAsync());

		Assert.That(ExceptionReporterTestListener.Instance.Count, Is.EqualTo(1));
		var ex = ExceptionReporterTestListener.Instance[0];
		Assert.That(ex.Message, Is.EqualTo("Value cannot be null. (Parameter 'jsRuntime')"));
		Assert.That(ex.Data["networkUserControl.NetworkViewModel.DiagramNodeViewModel.DiagramName"], Is.EqualTo("Diagram Entity Name"));
		Assert.That(ex.Data["networkUserControl.NetworkViewModel.DiagramNodeViewModel.Status"], Is.EqualTo(WorkStatus.Suspended));
		Assert.That(ex.Data["networkUserControl.JSRuntime"], Is.Null);
	}

	[Test]
	public async Task TestLinkingEntityToShapeFromClipboardAsync()
	{
		var entities = new ImpObservableSet<INetworkEntity>(new[] { new Entity() });
		var network = Mock.Of<INetwork>(x =>
			x.Entities == entities &&
			x.EntityPositionStrategy == new EntityPositionStrategy() &&
			x.DiagramEntity == new Entity());
		var networkViewModel = new NetworkViewModel(network);
		var userControl = new NetworkUserControlForTest(Renderer, networkViewModel)
		{
			JSRuntime = TestHelpers.GetMockedJSRuntime()
		};

		var linkFromClipboardAction = new Mock<INetworkAction>();
		using var model = new NCNDiagramModel(networkViewModel.ScheduledNodes, networkViewModel.ScheduledConnections, userControl)
		{
			linkFromClipboardAction = linkFromClipboardAction.Object
		};

		networkViewModel.BuildNetwork(isReloading: false);

		foreach (var nodeViewModel in networkViewModel.Nodes)
		{
			nodeViewModel.IsSelected = true;
		}

		await model.PasteFromClipboardAsync();

		Mock.Get(linkFromClipboardAction.Object).Verify(v => v.Execute());
	}

	[Test]
	public async Task TestInvertSelectionAsync()
	{
		var entities = new ImpObservableSet<INetworkEntity>(new[] { new Entity(), new Entity(), new Entity() });
		var network = Mock.Of<INetwork>(x =>
		x.Entities == entities &&
		x.EntityPositionStrategy == new EntityPositionStrategy() &&
		x.DiagramEntity == new Entity());
		var networkViewModel = new NetworkViewModel(network);
		var userControl = new NetworkUserControlForTest(Renderer, networkViewModel);
		using var model = new NCNDiagramModel(networkViewModel.ScheduledNodes, networkViewModel.ScheduledConnections, userControl);
		networkViewModel.BuildNetwork(isReloading: false);

		networkViewModel.Nodes.ElementAt(0).IsSelected = true;
		Assert.That(networkViewModel.Nodes.Count(n => n.IsSelected), Is.EqualTo(1));

		await model.InvertSelectionAsync();

		Assert.Multiple(() =>
		{
			Assert.That(networkViewModel.Nodes.Count(n => n.IsSelected), Is.EqualTo(2));
			Assert.That(networkViewModel.Nodes.ElementAt(0).IsSelected, Is.False);
			Assert.That(networkViewModel.Nodes.ElementAt(1).IsSelected, Is.True);
			Assert.That(networkViewModel.Nodes.ElementAt(2).IsSelected, Is.True);
		});
	}

	[Test]
	public void TestInvertSelectionHotKeyInvertsSelection()
	{
		var diagramEntity = new Entity();
		var entities = new ImpObservableSet<INetworkEntity>(new[] { new Entity(), new Entity(), new Entity() });
		var network = Mock.Of<INetwork>(x => x.Entities == entities && x.EntityPositionStrategy == new EntityPositionStrategy() && x.DiagramEntity == diagramEntity);
		var networkViewModel = new NetworkViewModel(network);
		var userControl = new NetworkUserControlForTest(Renderer, networkViewModel);
		networkViewModel.BuildNetwork(isReloading: false);
		using var diagramModel = new NCNDiagramModel(networkViewModel.ScheduledNodes, networkViewModel.ScheduledConnections, userControl);
		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		networkViewModel.Nodes.First().IsSelected = true;
		Assert.That(networkViewModel.Nodes.Count(n => n.IsSelected), Is.EqualTo(1));

		diagram.Instance.DiagramModel!.TriggerKeyDown(new KeyboardEventArgs("i", "", 0, true, false, false));
		Assert.Multiple(() =>
		{
			Assert.That(networkViewModel.Nodes.Count(n => n.IsSelected), Is.EqualTo(2));
			Assert.That(networkViewModel.Nodes.ElementAt(0).IsSelected, Is.False);
			Assert.That(networkViewModel.Nodes.ElementAt(1).IsSelected, Is.True);
			Assert.That(networkViewModel.Nodes.ElementAt(2).IsSelected, Is.True);
		});
	}

	[Test]
	public async Task TestDeleteSelectionAsync()
	{
		var entities = new ImpObservableSet<INetworkEntity>(new[] { new Entity(), new Entity(), new Entity() });
		var network = Mock.Of<INetwork>(x =>
			x.Entities == entities &&
			x.EntityPositionStrategy == new EntityPositionStrategy() &&
			x.DiagramEntity == new Entity());
		var networkViewModel = new NetworkViewModel(network);
		var userControl = new NetworkUserControlForTest(Renderer, networkViewModel);
		using var model = new NCNDiagramModel(networkViewModel.ScheduledNodes, networkViewModel.ScheduledConnections, userControl);

		networkViewModel.BuildNetwork(isReloading: false);
		foreach (var nodeViewModel in networkViewModel.Nodes)
		{
			nodeViewModel.IsSelected = true;
		}

		await model.DeleteSelectionAsync();

		foreach (var entity in entities)
		{
			Mock.Get(network).Verify(x => x.DeleteEntity(entity));
		}
	}

	[Test]
	public void TestSelectAll()
	{
		var entities = new ImpObservableSet<INetworkEntity>(new[] { new Entity(), new Entity(), new Entity() });
		var network = Mock.Of<INetwork>(x =>
			x.Entities == entities &&
			x.EntityPositionStrategy == new EntityPositionStrategy() &&
			x.DiagramEntity == new Entity());
		var networkViewModel = new NetworkViewModel(network);
		var userControl = new NetworkUserControlForTest(Renderer, networkViewModel);
		using var model = new NCNDiagramModel(networkViewModel.ScheduledNodes, networkViewModel.ScheduledConnections, userControl);

		networkViewModel.BuildNetwork(isReloading: false);

		Assert.That(networkViewModel.Nodes, Is.All.Matches<NodeViewModel>(obj => !obj.IsSelected));

		model.SelectAll();

		Assert.That(networkViewModel.Nodes, Is.All.Matches<NodeViewModel>(obj => obj.IsSelected));
	}

	[Test]
	public void TestSelectAllHotKeySelectsEntities()
	{
		var diagramEntity = new Entity();
		var entities = new ImpObservableSet<INetworkEntity>(new[] { new Entity(), new Entity(), new Entity() });
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
			nodeViewModel.IsSelected = false;
		}

		Assert.That(networkViewModel.Nodes, Is.All.Matches<NodeViewModel>(obj => !obj.IsSelected));

		diagram.Instance.DiagramModel!.TriggerKeyDown(new KeyboardEventArgs("a", "", 0, true, false, false));

		Assert.That(networkViewModel.Nodes, Is.All.Matches<NodeViewModel>(obj => obj.IsSelected));
	}

	[Test]
	public void TestDeselectAll()
	{
		var diagramEntity = new Entity();
		var entities = new ImpObservableSet<INetworkEntity>(new[] { new Entity(), new Entity(), new Entity() });
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

		Assert.That(networkViewModel.Nodes, Is.All.Matches<NodeViewModel>(obj => obj.IsSelected));

		diagram.Instance.DiagramModel!.DeselectAll();

		Assert.That(networkViewModel.Nodes, Is.All.Matches<NodeViewModel>(obj => !obj.IsSelected));
	}

	[Test]
	public void TestDeselectHotKeyDeselectsEntities()
	{
		var diagramEntity = new Entity();
		var entities = new ImpObservableSet<INetworkEntity>(new[] { new Entity(), new Entity(), new Entity() });
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

		Assert.That(networkViewModel.Nodes, Is.All.Matches<NodeViewModel>(obj => obj.IsSelected));

		diagram.Instance.DiagramModel!.TriggerKeyDown(new KeyboardEventArgs("Escape", "", 0, false, false, false));

		Assert.That(networkViewModel.Nodes, Is.All.Matches<NodeViewModel>(obj => !obj.IsSelected));
	}

	[Test]
	public void TestLinkWhenSourceIsInvalid()
	{
		var (model, nodes, connections) = CreateDiagramModel(Renderer);

		var sourceNode = CreateNode();
		var targetNode = CreateNode();
		var connection = CreateConnection(sourceNode, targetNode);

		nodes.Add(sourceNode);
		nodes.Add(targetNode);
		connections.Add(connection);

		Assert.That(connections.Count, Is.EqualTo(1));
		Assert.That(((NetworkLinkModel)model!.Links!.First()).SourceEntityPK, Is.EqualTo(sourceNode.Entity.EntityPK));
		Assert.That(((NetworkLinkModel)model!.Links!.First()).TargetEntityPK, Is.EqualTo(targetNode.Entity.EntityPK));

		nodes.Remove(sourceNode);
		nodes.Add(sourceNode);
		Assert.That(model.Links.Count, Is.EqualTo(0));
	}

	[Test]
	public void TestLinkWhenTargetIsInvalid()
	{
		var (model, nodes, connections) = CreateDiagramModel(Renderer);

		var sourceNode = CreateNode();
		var targetNode = CreateNode();
		var connection = CreateConnection(sourceNode, targetNode);

		nodes.Add(sourceNode);
		nodes.Add(targetNode);
		connections.Add(connection);

		Assert.That(connections.Count, Is.EqualTo(1));
		Assert.That(((NetworkLinkModel)model!.Links!.First()).SourceEntityPK, Is.EqualTo(sourceNode.Entity.EntityPK));
		Assert.That(((NetworkLinkModel)model!.Links!.First()).TargetEntityPK, Is.EqualTo(targetNode.Entity.EntityPK));

		nodes.Remove(targetNode);
		nodes.Add(targetNode);
		Assert.That(model.Links.Count, Is.EqualTo(0));
	}

	[Test]
	public void TestLinkWhenSourceAndTargetAreValid()
	{
		var (model, nodes, connections) = CreateDiagramModel(Renderer);

		var sourceNode = CreateNode();
		var targetNode = CreateNode();
		var connection = CreateConnection(sourceNode, targetNode);

		nodes.Add(sourceNode);
		nodes.Add(targetNode);
		connections.Add(connection);

		Assert.That(connections.Count, Is.EqualTo(1));
		Assert.That(((NetworkLinkModel)model!.Links!.First()).SourceEntityPK, Is.EqualTo(sourceNode.Entity.EntityPK));
		Assert.That(((NetworkLinkModel)model!.Links!.First()).TargetEntityPK, Is.EqualTo(targetNode.Entity.EntityPK));

		Assert.That(model.Links.Count, Is.EqualTo(1));
	}

#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
	static (NCNDiagramModel, ObservableCollection<NodeViewModel>, ObservableCollection<ConnectionViewModel>) CreateDiagramModel(ITestRenderer renderer, NodeViewModel[]? initialNodes = null, ConnectionViewModel[]? initialConnections = null)
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly
	{
		var nodes = new ObservableCollection<NodeViewModel>(initialNodes ?? Array.Empty<NodeViewModel>());
		var connections = new ObservableCollection<ConnectionViewModel>(initialConnections ?? Array.Empty<ConnectionViewModel>());
		var control = new NetworkUserControlForTest(renderer);
#pragma warning disable CA2000 // Dispose objects before losing scope
		var model = new NCNDiagramModel(nodes, connections, control);
#pragma warning restore CA2000 // Dispose objects before losing scope
		return (model, nodes, connections);
	}

	static NodeViewModel CreateNode()
	{
		var entity = new Entity() { Width = 10, Height = 10 };
		return new NodeViewModel(entity, new NetworkViewModel(new DummyNetwork()));
	}

	static ConnectionViewModel CreateConnection(NodeViewModel sourceNode, NodeViewModel targetNode)
	{
		sourceNode.OutputConnectors.Add(new ConnectorViewModel("out"));
		targetNode.InputConnectors.Add(new ConnectorViewModel("in"));
		return new ConnectionViewModel()
		{
			SourceConnector = sourceNode.OutputConnectors[0],
			DestConnector = targetNode.InputConnectors[0]
		};
	}
}
