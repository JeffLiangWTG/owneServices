using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Extentions;

static class NetworkViewModelExtentions
{
	public static NodeViewModel CreateNodeViewModel(this NetworkViewModel networkViewModel, INetworkEntity entity)
		=> networkViewModel.NodeViewModelProvider.Create(entity, networkViewModel);
}
