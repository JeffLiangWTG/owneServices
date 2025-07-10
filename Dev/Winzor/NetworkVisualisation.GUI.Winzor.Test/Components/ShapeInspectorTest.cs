using Bunit.Rendering;
using Bunit.TestDoubles;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Components;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Moq;
using NetworkVisualisation.GUI.Winzor.Models.Factories;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;

namespace NetworkVisualisation.GUI.Winzor.Test.Components;

class AnnotationNodeShapeInspectorTest : ShapeInspectorTest<AnnotationViewModel, AnnotationNode, AnnotationNodeModel>
{
	protected override string NodeElement => ".annotationnode";
}
class JobNodeShapeInspectorTest : ShapeInspectorTest<NodeViewModel, JobNode, JobNodeModel>
{
	protected override string NodeElement => ".jobnode";
}
class BufferNodeShapeInspectorTest : ShapeInspectorTest<BufferViewModel, BufferNode, BufferNodeModel>
{
	protected override string NodeElement => ".buffernode";
}

abstract class ShapeInspectorTest<TNodeViewModel, TNetworkNode, TNetworkNodeModel> : BunitTestContext where TNodeViewModel : NodeViewModel where TNetworkNode : NetworkNode<TNetworkNodeModel> where TNetworkNodeModel : NetworkNodeModel
{
	protected abstract string NodeElement { get; }
	protected string ShapeInspectorInstructions => ".shapeinspector__instructions";

	static TNodeViewModel GetViewModel(Entity entity, NetworkViewModel network)
	{
		return (TNodeViewModel)Activator.CreateInstance(typeof(TNodeViewModel), entity, network)!;
	}

	static TNetworkNodeModel GetNetworkNodeModel(TNodeViewModel viewModel, IWinzorDispatch winzorDispatch)
	{
		return (TNetworkNodeModel)new NodeModelFactory(winzorDispatch).GetNodeModel(viewModel);
	}

	[Test]
	public void ShowsMessageIfViewModelIsNull()
	{
		TestContext?.ComponentFactories.AddStub<TNetworkNode>();
		var component = RenderComponent<ShapeInspector>();
		component.Find(ShapeInspectorInstructions);
		Assert.That(() => component.FindComponent<Stub<TNetworkNode>>(), Throws.InstanceOf<ComponentNotFoundException>());
		Assert.That(component.Instance.Model, Is.Null);
	}
	[Test]
	public void DisposesOldModelWhenNewModelIsProvided()
	{
		TestContext?.ComponentFactories.AddStub<TNetworkNode>();
		var mockViewModel1 = new Mock<TNodeViewModel>(new Entity(), new NetworkViewModel(new DummyNetwork())).Object;
		var mockViewModel2 = new Mock<TNodeViewModel>(new Entity(), new NetworkViewModel(new DummyNetwork())).Object;
		var component = RenderComponent<ShapeInspector>(parameters =>
		{
			parameters.Add(p => p.ViewModel, mockViewModel1);
			parameters.Add(p => p.UserControl, new NetworkUserControlForTest(Renderer));
		});

		var isDisposed = false;
		var oldModel = component.Instance.Model;
		oldModel.Disposed += (sender, args) => isDisposed = true;

		Assert.That(isDisposed, Is.False);

		component.SetParametersAndRender(parameters =>
			parameters.Add(p => p.ViewModel, mockViewModel2)
		);

		Assert.That(isDisposed, Is.True);
	}

	[Test]
	public void ShowsElement()
	{
		TestContext?.ComponentFactories.AddStub<TNetworkNode>();
		var mockViewModel = new Mock<TNodeViewModel>(new Entity(), new NetworkViewModel(new DummyNetwork())).Object;
		var component = RenderComponent<ShapeInspector>(parameters =>
		{
			parameters.Add(p => p.ViewModel, mockViewModel);
			parameters.Add(p => p.UserControl, new NetworkUserControlForTest(Renderer));
		});

		Assert.That(() => component.Find(ShapeInspectorInstructions), Throws.InstanceOf<ElementNotFoundException>());
		var card = component.FindComponent<Stub<TNetworkNode>>();
		Assert.That(card.Instance.Parameters.Get(stub => stub.Node), Is.EqualTo(component.Instance.Model));
		Assert.That(component.Instance.Model, Is.Not.Null);
	}

	[Test]
	public void DoubleClickDoesNotInvokeOutsideOfDiagram()
	{
		var entity = new Entity();
		var diagramEntity = new Entity();
		var entities = new ImpObservableSet<INetworkEntity>(new[] { entity });
		var network = Mock.Of<INetwork>(x => x.Entities == entities && x.DiagramEntity == diagramEntity && x.EntityPositionStrategy == new EntityPositionStrategy());
		var networkViewModel = new NetworkViewModel(network);
		var viewModel = GetViewModel(entity, networkViewModel);
		viewModel.X = 0;
		viewModel.Y = 0;

		var node = RenderComponent<TNetworkNode>(parameters =>
		{
			parameters.Add(p => p.Node, GetNetworkNodeModel(viewModel, new NetworkUserControlForTest(Renderer)));
		});

		Assert.Throws<MissingEventHandlerException>(() => node.Find(NodeElement).DoubleClick());
		Mock.Get(network).Verify(x => x.ViewEntity(entity), Times.Never);
	}
}
