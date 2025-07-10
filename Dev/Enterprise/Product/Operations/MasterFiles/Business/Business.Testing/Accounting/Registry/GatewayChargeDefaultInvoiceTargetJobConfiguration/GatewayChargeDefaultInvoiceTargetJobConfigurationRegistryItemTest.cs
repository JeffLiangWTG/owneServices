using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GatewayChargeDefaultInvoiceTargetJobConfigurationRegistryItem))]
	sealed class GatewayChargeDefaultInvoiceTargetJobConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<GatewayChargeDefaultInvoiceTargetJobConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<GatewayChargeDefaultInvoiceTargetJobConfigurationCollection, GatewayChargeDefaultInvoiceTargetJobConfigurationCollection> GetNewRegistryItem()
		{
			return new GatewayChargeDefaultInvoiceTargetJobConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
