using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(RefCarrierConfig))]
	sealed class RefCarrierConfigTest : TestCaseWithFactory
	{
		public void TestConfigurationSettings()
		{
			var config = new RefCarrierConfig();
			AssertContainsExactElementsInAnyOrder([RefCarrierAttributeNames.MASTER, RefCarrierAttributeNames.CARGOCARRIER], config.MandatoryAttributes);
			AssertEquals(true, config.IsSetDefaultCarrierType);
		}
	}
}
