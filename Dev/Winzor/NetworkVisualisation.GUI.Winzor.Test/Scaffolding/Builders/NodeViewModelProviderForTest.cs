using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;

namespace NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;

class NodeViewModelProviderForTest : NodeViewModelProvider
{
	public override NodeViewModel Create(INetworkEntity entity, NetworkViewModel networkViewModel)
		=> entity.ShapeType switch
		{
			WinzorShapeTypes.Buffer => new BufferViewModel(entity, networkViewModel),
			_ => base.Create(entity, networkViewModel),
		};
}
