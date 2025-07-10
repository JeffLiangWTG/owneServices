using Blazor.Diagrams.Core.Models;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.GUI.Services.AnnotationNode;
using CargoWise.NetworkVisualisation.GUI.Services.BufferNode;
using CargoWise.NetworkVisualisation.GUI.Services.JobNode;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Extentions;

namespace NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;

abstract class NodeModelBuilder<TNodeModel> where TNodeModel : NodeModel
{
	INetworkEntity? entity;
	INetwork? network;
	NodeViewModel? nodeViewModel;
	INetworkUserControl? networkUserControl;

	protected INetworkEntity Entity => entity ??= Stub.Entity();
	protected INetwork Network => network ??= new NetworkBuilder().Build();
	protected INetworkUserControl NetworkUserControl => networkUserControl ??= new NetworkUserControlForTestBuilder().WithNetwork(Network).Build();
	protected NodeViewModel NodeViewModel => nodeViewModel ??= NetworkUserControl.CreateNodeViewModel(Entity);

	public abstract TNodeModel Build();

	public NodeModelBuilder<TNodeModel> WithEntity(INetworkEntity value)
	{
		entity = value;
		nodeViewModel = null;
		return this;
	}

	public NodeModelBuilder<TNodeModel> WithNetwork(INetwork value)
	{
		network = value;
		return this;
	}

	public NodeModelBuilder<TNodeModel> WithNodeViewModel(NodeViewModel value)
	{
		nodeViewModel = value;
		return this;
	}
}

class BufferNodeModelBuilder : NodeModelBuilder<BufferNodeModel>
{
	public override BufferNodeModel Build()
	{
		var nodeViewModel = NodeViewModel as BufferViewModel;

		Assert.That(nodeViewModel, Is.Not.Null, $"Failed to create BufferNodeModel for Entity.ShapeType = '{Entity.ShapeType}'. Shouldn't it be '{WinzorShapeTypes.Buffer}'?");

		return new BufferNodeModel(new BufferNodeService(nodeViewModel, NetworkUserControl));
	}
}

class JobNodeModelBuilder : NodeModelBuilder<JobNodeModel>
{
	public override JobNodeModel Build()
	{
		return new JobNodeModel(new JobNodeService(NodeViewModel, NetworkUserControl));
	}
}

class AnnotationNodeModelBuilder : NodeModelBuilder<AnnotationNodeModel>
{
	public override AnnotationNodeModel Build()
	{
		return new AnnotationNodeModel(new AnnotationNodeService(NodeViewModel, NetworkUserControl));
	}
}
