using CargoWise.NetworkVisualisation.GUI.Services.NetworkNode;

namespace CargoWise.NetworkVisualisation.GUI.Services.LinkableNode;

public interface ILinkableNodeService : INetworkNodeService
{
	LinkableNodeData GetLinkableNodeData();
}
