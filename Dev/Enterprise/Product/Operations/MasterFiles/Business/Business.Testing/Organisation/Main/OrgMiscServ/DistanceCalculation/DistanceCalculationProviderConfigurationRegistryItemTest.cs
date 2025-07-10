using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DistanceCalculationProviderConfigurationRegistryItem))]
	sealed class DistanceCalculationProviderConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<DistanceCalculationProviderConfiguration>
	{
		protected override StronglyTypedRegistryItem<DistanceCalculationProviderConfiguration, DistanceCalculationProviderConfiguration> GetNewRegistryItem()
		{
			return new DistanceCalculationProviderConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System, DistanceCalculationProviderConfiguration.GetDefault());
		}
	}
}
