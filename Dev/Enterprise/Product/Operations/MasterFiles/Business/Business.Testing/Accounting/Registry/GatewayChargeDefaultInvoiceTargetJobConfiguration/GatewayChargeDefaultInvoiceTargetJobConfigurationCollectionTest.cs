using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GatewayChargeDefaultInvoiceTargetJobConfigurationCollection))]
	sealed class GatewayChargeDefaultInvoiceTargetJobConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<GatewayChargeDefaultInvoiceTargetJobConfigurationCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override GatewayChargeDefaultInvoiceTargetJobConfigurationCollection GetCollectionToTest()
		{
			return new GatewayChargeDefaultInvoiceTargetJobConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GatewayChargeDefaultInvoiceTargetJobConfiguration();
		}
	}
}
