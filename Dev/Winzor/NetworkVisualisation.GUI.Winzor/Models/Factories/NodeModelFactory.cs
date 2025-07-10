using System;
using System.Threading.Tasks;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.GUI.Services.AnnotationNode;
using CargoWise.NetworkVisualisation.GUI.Services.BufferNode;
using CargoWise.NetworkVisualisation.GUI.Services.JobNode;
using Enterprise.BufferManagement.NetworkVisualisation.Business;

namespace NetworkVisualisation.GUI.Winzor.Models.Factories;

internal class NodeModelFactory : INodeModelFactory
{
	IWinzorDispatch WinzorDispatcher { get; }

	public NodeModelFactory(IWinzorDispatch winzorDispatcher)
	{
		WinzorDispatcher = winzorDispatcher;
	}

	public NetworkNodeModel GetNodeModel(NodeViewModel node) => node switch
	{
		AnnotationViewModel => GetAnnotationNodeModel(node),
		BufferViewModel => GetBufferNodeModel(node),
		_ => GetJobNodeModel(node),
	};

	public Task<NetworkNodeModel> GetNodeModelAsync(NodeViewModel node) => RunOnWinzorThreadAsync(() => GetNodeModel(node));

	JobNodeModel GetJobNodeModel(NodeViewModel node) => new (new JobNodeService(node, WinzorDispatcher));
	BufferNodeModel GetBufferNodeModel(NodeViewModel node) => new (new BufferNodeService(node, WinzorDispatcher));
	AnnotationNodeModel GetAnnotationNodeModel(NodeViewModel node) => new (new AnnotationNodeService(node, WinzorDispatcher));

	async Task<TNodeModel> RunOnWinzorThreadAsync<TNodeModel>(Func<TNodeModel> factoryFunc) where TNodeModel : NetworkNodeModel
	{
		TNodeModel nodeModel = default!;
		await WinzorDispatcher.InvokeWinzorDispatcherAsync(() =>
		{
			nodeModel = factoryFunc();
		});

		return nodeModel;
	}
}
