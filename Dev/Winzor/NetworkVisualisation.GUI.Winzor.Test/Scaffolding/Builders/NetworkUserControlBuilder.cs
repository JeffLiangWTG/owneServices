using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.Integration;

namespace NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;

internal class NetworkUserControlBuilder
{
	INetwork? network;
	INetwork Network => network ??= new NetworkBuilder().Build();

	NodeViewModelProvider? nodeViewModelProvider;
	IRibbonDataProvider? ribbonDataProvider;

	public NetworkUserControl Build()
	{
		var networkUserControl = new NetworkUserControl(
			Network.DiagramEntity, Network
			.Refresher,
			nodeViewModelProvider,
			ribbonDataProvider);

		networkUserControl.SetDataContext(Network, isReloading: false);

		return networkUserControl;
	}

	public NetworkUserControlBuilder WithNetwork(INetwork value)
	{
		network = value;
		return this;
	}

	public NetworkUserControlBuilder WithNodeViewModelProvider(NodeViewModelProvider value)
	{
		nodeViewModelProvider = value;
		return this;
	}

	public NetworkUserControlBuilder WithRibbonDataProvider(IRibbonDataProvider value)
	{
		ribbonDataProvider = value;
		return this;
	}
}
