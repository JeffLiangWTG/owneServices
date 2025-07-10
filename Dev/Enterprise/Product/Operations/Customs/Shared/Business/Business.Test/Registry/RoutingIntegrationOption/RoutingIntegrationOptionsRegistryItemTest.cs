using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(RoutingIntegrationOptionsRegistryItem))]
	sealed class RoutingIntegrationOptionsRegistryItemTest : StronglyTypedRegistryItemTestCase<RoutingIntegrationOptions>
	{
		protected override StronglyTypedRegistryItem<RoutingIntegrationOptions, RoutingIntegrationOptions> GetNewRegistryItem()
		{
			var options = new RoutingIntegrationOptions();
			options.NeverLink = true;

			return new RoutingIntegrationOptionsRegistryItem("", null, null, null, RegistryStorageFlags.System, options);
		}
	}
}
