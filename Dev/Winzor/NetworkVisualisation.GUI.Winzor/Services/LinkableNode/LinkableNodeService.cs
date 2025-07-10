using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Extensions;
using CargoWise.NetworkVisualisation.GUI.Services.NetworkNode;

namespace CargoWise.NetworkVisualisation.GUI.Services.LinkableNode;

public class LinkableNodeService : NetworkNodeService, ILinkableNodeService
{
	public LinkableNodeService(NodeViewModel viewModel, IWinzorDispatch winzorDispatcher) : base(viewModel, winzorDispatcher)
	{
	}

	#region ILinkableNodeViewModel

	public LinkableNodeData GetLinkableNodeData()
	{
		winzorDispatcher.AssertInWinzorThread();

		return new LinkableNodeData
		{
			IsOnCriticalPath = viewModel.Entity.IsOnCriticalPath,
		};
	}

	#endregion
}
