using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.Integration;

namespace NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Extentions;

internal static class NetworkUserControlExtensions
{
	public static NodeViewModel CreateNodeViewModel(this INetworkUserControl networkUserControl, INetworkEntity entity)
		=> networkUserControl.NetworkViewModel.CreateNodeViewModel(entity);

	public static void AddScheduledNode(this INetworkUserControl networkUserControl, NodeViewModel nodeViewModel)
	{
		var networkViewModel = networkUserControl.NetworkViewModel;

		networkViewModel.ScheduledNodes.Add(nodeViewModel);
	}

	public static void AddScheduledNode(this INetworkUserControl networkUserControl, INetworkEntity entity, Action<NodeViewModel>? config = null)
	{
		var networkViewModel = networkUserControl.NetworkViewModel;

		var nodeViewModel = networkViewModel.CreateNodeViewModel(entity);
		config?.Invoke(nodeViewModel);

		networkUserControl.AddScheduledNode(nodeViewModel);
	}

	public static void AddScheduledNodes(this INetworkUserControl networkUserControl, INetworkEntity[] entities, Action<NodeViewModel>? config = null)
	{
		foreach (var entity in entities)
		{
			networkUserControl.AddScheduledNode(entity, config);
		}
	}

	public static NodeViewModelCollection ScheduledNodes(this INetworkUserControl networkUserControl)
		=> networkUserControl.NetworkViewModel.ScheduledNodes;
}
