using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AutoRateDateByChargeGroupRegistryItem))]
	sealed class AutoRateDateByChargeGroupRegistryItemTest : StronglyTypedRegistryItemTestCase<AutoRateDateByChargeGroupConfiguration>
	{
		protected override StronglyTypedRegistryItem<AutoRateDateByChargeGroupConfiguration, AutoRateDateByChargeGroupConfiguration> GetNewRegistryItem()
		{
			return new AutoRateDateByChargeGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, new AutoRateDateByChargeGroupConfiguration());
		}
	}
}
