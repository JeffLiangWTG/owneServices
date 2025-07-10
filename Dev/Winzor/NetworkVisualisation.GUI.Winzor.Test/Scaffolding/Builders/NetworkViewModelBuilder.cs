using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;

internal class NetworkViewModelBuilder
{
	INetwork? network;
	INetwork Network => network ??= new NetworkBuilder().Build();

	NodeViewModelProvider? provider;
	NodeViewModelProvider Provider => provider ??= new NodeViewModelProviderForTest();

	public NetworkViewModel Build()
	{
		var networkViewModel = new NetworkViewModel(Network, Provider);
		networkViewModel.BuildNetwork(isReloading: false);

		return networkViewModel;
	}

	public NetworkViewModelBuilder WithNetwork(INetwork value)
	{
		network = value;
		return this;
	}

	public NetworkViewModelBuilder WithProvider(NodeViewModelProvider value)
	{
		provider = value;
		return this;
	}
}
