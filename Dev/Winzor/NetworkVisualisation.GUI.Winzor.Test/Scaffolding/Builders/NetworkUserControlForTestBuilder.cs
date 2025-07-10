using Bunit.Rendering;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;

internal class NetworkUserControlForTestBuilder
{
	ITestRenderer? renderer;

	INetwork? network;
	INetwork Network => network ??= new NetworkBuilder().Build();

	NodeViewModelProvider? nodeViewModelProvider;
	NodeViewModelProvider NodeViewModelProvider => nodeViewModelProvider ??= new NodeViewModelProviderForTest();

	public NetworkUserControlForTest Build()
	{
		return new (renderer: renderer, Network, NodeViewModelProvider);
	}
	public NetworkUserControlForTestBuilder WithTestRenderer(ITestRenderer value)
	{
		renderer = value;
		return this;
	}

	public NetworkUserControlForTestBuilder WithNetwork(INetwork value)
	{
		network = value;
		return this;
	}

	public NetworkUserControlForTestBuilder WithNodeViewModelProvider(NodeViewModelProvider value)
	{
		nodeViewModelProvider = value;
		return this;
	}
}
