using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GatewayChargeDefaultDebtorConfigurationRegistryItem))]
	sealed class GatewayChargeDefaultDebtorConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<GatewayChargeDefaultDebtorConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<GatewayChargeDefaultDebtorConfigurationCollection, GatewayChargeDefaultDebtorConfigurationCollection> GetNewRegistryItem()
		{
			return new GatewayChargeDefaultDebtorConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
