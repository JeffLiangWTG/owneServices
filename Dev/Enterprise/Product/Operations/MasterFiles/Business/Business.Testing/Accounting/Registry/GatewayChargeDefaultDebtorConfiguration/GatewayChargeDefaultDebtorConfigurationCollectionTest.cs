using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GatewayChargeDefaultDebtorConfigurationCollection))]
	sealed class GatewayChargeDefaultDebtorConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<GatewayChargeDefaultDebtorConfigurationCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override GatewayChargeDefaultDebtorConfigurationCollection GetCollectionToTest()
		{
			return new GatewayChargeDefaultDebtorConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GatewayChargeDefaultDebtorConfiguration();
		}
	}
}
