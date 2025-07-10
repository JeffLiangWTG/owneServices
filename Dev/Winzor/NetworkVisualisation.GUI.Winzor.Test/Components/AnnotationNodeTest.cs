using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Components;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.Integration;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Extensions;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Extentions;

namespace NetworkVisualisation.GUI.Winzor.Test.Components;

class AnnotationNodeTest : NetworkNodeTest<AnnotationViewModel, AnnotationNode, AnnotationNodeModel>
{
	protected override string ShapeType => WinzorShapeTypes.Annotation;

	protected override AnnotationNodeModel CreateNodeModel(INetworkEntity entity)
	{
		return new AnnotationNodeModelBuilder().WithEntity(entity).Build();
	}

	protected override AnnotationNodeModel CreateNodeModel(AnnotationViewModel nodeViewModel)
	{
		return new AnnotationNodeModelBuilder().WithNodeViewModel(nodeViewModel).Build();
	}

	[Test]
	public void TestSelectedIsBoundToViewModelBothWays()
	{
		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		var networkUserControl = new NetworkUserControlForTestBuilder().Build();
		var node = networkUserControl.CreateNodeViewModel(entity) as AnnotationViewModel;

		Assert.That(node, Is.Not.Null);

		networkUserControl.AddScheduledNode(node);
		var diagram = RenderDiagram(networkUserControl);
		var nodeModel = diagram.Instance.DiagramModel!.Nodes.First();

		Assert.That(nodeModel.Selected, Is.False);
		Assert.That(node.IsSelected, Is.False);

		diagram.Instance.DiagramModel!.SelectModel(nodeModel, false);

		Assert.That(nodeModel.Selected, Is.True);
		Assert.That(node.IsSelected, Is.True);

		diagram.Instance.DiagramModel!.UnselectModel(nodeModel);

		Assert.That(nodeModel.Selected, Is.False);
		Assert.That(node.IsSelected, Is.False);

		node.IsSelected = true;

		Assert.That(nodeModel.Selected, Is.True);
		Assert.That(node.IsSelected, Is.True);

		node.IsSelected = false;

		Assert.That(nodeModel.Selected, Is.False);
		Assert.That(node.IsSelected, Is.False);
	}

	[Test]
	public void DoubleClickDoesNotInvokeViewEntity()
	{
		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		var diagramEntity = Stub.Entity();
		var entities = new ImpObservableSet<INetworkEntity>(new[] { entity });
		var network = Mock.Of<INetwork>(x => x.Entities == entities && x.DiagramEntity == diagramEntity && x.EntityPositionStrategy == new EntityPositionStrategy());

		var networkUserControl = new NetworkUserControlForTestBuilder().WithNetwork(network).Build();
		networkUserControl.AddScheduledNode(entity);

		var diagram = RenderDiagram(networkUserControl);

		var diagramName = diagram.Find(".annotationnode__diagramname > .textarea__resizable");
		Assert.Throws<MissingEventHandlerException>(() => diagramName.DoubleClick());

		var notes = diagram.Find(".annotationnode__notes > .textarea__resizable");
		Assert.Throws<MissingEventHandlerException>(() => notes.DoubleClick());
	}

	[Test]
	public void TestDeleteIconRendered()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.X = 10; e.Y = 11; });

		var diagram = RenderDiagram(entity);

		Assert.That(diagram.Find(".deleteicon"), Is.Not.Null);
	}

	[Test]
	public void TextAreasRenderedAsDynamicTextAreas()
	{
		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		var diagram = RenderDiagram(entity);
		var annotationNode = diagram.FindComponent<AnnotationNode>();

		var textAreas = annotationNode.FindComponents<DynamicTextArea>();
		Assert.That(textAreas.Count, Is.EqualTo(2));

		Assert.That(textAreas[0].Find(".annotationnode__diagramname"), Is.Not.Null);
		Assert.That(textAreas[1].Find(".annotationnode__notes"), Is.Not.Null);
	}

	[Test]
	public void TestDraggingNodeDragsChildren()
	{
		var networkModel = new NetworkViewModel(new DummyNetwork());
		var entity = new Entity() { Width = 200, Height = 400, X = 0, Y = 0 };

		var annotationNodeViewModel = new AnnotationViewModel(entity, networkModel);
		var userControl = new NetworkUserControlForTest(Renderer, networkModel);

		networkModel.ScheduledNodes.Add(annotationNodeViewModel);

		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var annotationNodes = diagram.FindComponents<AnnotationNode>();
		var annotationNode = annotationNodes[0].Instance.Node!;

		Assert.That(annotationNode.Position.X, Is.EqualTo(0));
		Assert.That(annotationNode.Position.Y, Is.EqualTo(0));

		var initialPointerState = Pointer.LeftButton.At(0, 0);
		diagram.Instance.DiagramModel!.TriggerPointerDown(annotationNode, initialPointerState);

		Assert.That(annotationNode.IsSelected);

		var movePointerState = Pointer.LeftButton.At(150, 100);
		diagram.Instance.DiagramModel.TriggerPointerMove(annotationNode, movePointerState);

		diagram.Instance.DiagramModel.TriggerPointerUp(annotationNode, Pointer.LeftButton);

		Assert.That(annotationNode.Position.X, Is.EqualTo(150));
		Assert.That(annotationNode.Position.Y, Is.EqualTo(100));
	}
}
