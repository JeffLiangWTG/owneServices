using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Controls.NodeResizing;
using CargoWise.NetworkVisualisation.GUI.Models;

namespace NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;

internal class NCNDiagramModelBuilder
{
	INetworkUserControl? networkUserControl;
	NodeResizeControlFactory? nodeResizeControlFactory;

	INetworkUserControl NetworkUserControl => networkUserControl ??= new NetworkUserControlForTestBuilder().Build();

	NetworkViewModel NetworkViewModel => NetworkUserControl.NetworkViewModel;

	public NCNDiagramModel Build()
		=> new (NetworkViewModel.ScheduledNodes, NetworkViewModel.ScheduledConnections, NetworkUserControl, nodeResizeControlFactory);

	public NCNDiagramModelBuilder WithNetworkUserControl(INetworkUserControl value)
	{
		networkUserControl = value;
		return this;
	}

	public NCNDiagramModelBuilder WithNodeResizeControlFactory(NodeResizeControlFactory? value)
	{
		nodeResizeControlFactory = value;
		return this;
	}
}
