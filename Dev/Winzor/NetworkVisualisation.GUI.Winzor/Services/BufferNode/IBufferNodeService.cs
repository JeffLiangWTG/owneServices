using CargoWise.NetworkVisualisation.GUI.Services.LinkableNode;
using CargoWise.NetworkVisualisation.GUI.Services.ProgressBarModel;

namespace CargoWise.NetworkVisualisation.GUI.Services.BufferNode;

public interface IBufferNodeService : ILinkableNodeService
{
	BufferNodeData GetBufferNodeModelData();

	IProgressBarModelService GetProgressBarService();
}
