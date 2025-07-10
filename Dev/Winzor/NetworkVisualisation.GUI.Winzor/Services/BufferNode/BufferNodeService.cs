using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Extensions;
using CargoWise.NetworkVisualisation.GUI.Services.LinkableNode;
using Enterprise.BufferManagement.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI.Services.BufferNode;

public class BufferNodeService : LinkableNodeService, IBufferNodeService
{
	public BufferNodeService(NodeViewModel viewModel, IWinzorDispatch winzorDispatcher) : base(viewModel, winzorDispatcher)
	{
	}

	#region IBufferNodeViewModel

	public BufferNodeData GetBufferNodeModelData()
	{
		winzorDispatcher.AssertInWinzorThread();

		return new BufferNodeData
		{
			Description = viewModel.Entity.Description,
			AdditionalDetail = viewModel.Entity.AdditionalDetail,
			StatusTooltip = viewModel.StatusTooltip,
			PenetrationPercentLabel = (viewModel as BufferViewModel)?.PenetrationPercentLabel,
			Status = viewModel.Status,
		};
	}

	#endregion
}
