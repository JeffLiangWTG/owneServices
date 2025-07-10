using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Services.NetworkNode;

namespace CargoWise.NetworkVisualisation.GUI.Services.AnnotationNode;

public class AnnotationNodeService : NetworkNodeService, IAnnotationNodeService
{
	public AnnotationNodeService(NodeViewModel viewModel, IWinzorDispatch winzorDispatcher) : base(viewModel, winzorDispatcher)
	{
	}
}
