using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SupplyTypeConfigurationByChargeGroupRegistryItem))]
	sealed class SupplyTypeConfigurationByChargeGroupRegistryItemTest : StronglyTypedRegistryItemTestCase<SupplyTypeConfigurationByChargeGroupCollection>
	{
		protected override StronglyTypedRegistryItem<SupplyTypeConfigurationByChargeGroupCollection, SupplyTypeConfigurationByChargeGroupCollection> GetNewRegistryItem()
		{
			return new SupplyTypeConfigurationByChargeGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new SupplyTypeConfigurationByChargeGroupCollection());
		}
	}
}
