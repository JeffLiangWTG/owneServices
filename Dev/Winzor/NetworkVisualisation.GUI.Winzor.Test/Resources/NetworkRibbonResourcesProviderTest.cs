using CargoWise.NetworkVisualisation.GUI;

namespace NetworkVisualisation.GUI.Winzor.Test;

class NetworkRibbonResourcesProviderTest
{
	static readonly Dictionary<string, string> TestResources = new Dictionary<string, string>()
	{
		{ "ResourceOne", "path/to/resource/one" },
		{ "ResourceTwo", "path/to/resource/two" },
	};

	[Test]
	public void NetworkRibbonResourcesProviderHasResources()
	{
		var provider = new NetworkRibbonResourcesProvider();
		Assert.That(provider.HasResources(), Is.False);

		provider.AddRibbonResources(TestResources);
		Assert.That(provider.HasResources(), Is.True);
	}

	[Test]
	public void NetworkRibbonResourcesProviderAddAndGetResource()
	{
		var provider = new NetworkRibbonResourcesProvider();
		provider.AddRibbonResources(TestResources);

		Assert.That(provider.GetResource("ResourceOne"), Is.EqualTo("path/to/resource/one"));
		Assert.That(provider.GetResource("ResourceTwo"), Is.EqualTo("path/to/resource/two"));
	}

	[Test]
	public void NetworkRibbonResourcesProviderAddDuplicateResourcesThrowsException()
	{
		var provider = new NetworkRibbonResourcesProvider();
		provider.AddRibbonResources(TestResources);

		Assert.Throws<ArgumentException>(() =>
		{
			provider.AddRibbonResources(TestResources);
		});
	}

	[Test]
	public void NetworkRibbonResourcesProviderGetResourceThatHasNotBeenAddedReturnsNull()
	{
		var provider = new NetworkRibbonResourcesProvider();

		Assert.That(provider.GetResource("ResourceOne"), Is.Null);
	}
}
