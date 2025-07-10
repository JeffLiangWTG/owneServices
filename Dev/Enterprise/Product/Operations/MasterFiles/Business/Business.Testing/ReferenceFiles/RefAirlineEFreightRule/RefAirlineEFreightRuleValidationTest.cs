using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefAirlineEFreightRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateRME_OriginLocation()
		{
			var rule = Factory.New<RefAirlineEFreightRule>();
			rule.RME_OriginLocation = "XXXXX";

			Assert("RME_OriginLocation has a wrong location", rule.RME_OriginLocationInfo.HasError("Enter a valid selection."));

			rule.RME_OriginLocation = "AU";

			Assert("RME_OriginLocation has a correct location", !rule.RME_OriginLocationInfo.HasErrors());
		}

		public void TestValidateRME_DestinationLocation()
		{
			var rule = Factory.New<RefAirlineEFreightRule>();
			rule.RME_DestinationLocation = "XXXXX";

			Assert("RME_DestinationLocation has a wrong location", rule.RME_DestinationLocationInfo.HasError("Enter a valid selection."));

			rule.RME_DestinationLocation = "AU";

			Assert("RME_DestinationLocation has a correct location", !rule.RME_DestinationLocationInfo.HasErrors());
		}

		public void TestValidateRME_EFreightStatus()
		{
			var rule = Factory.New<RefAirlineEFreightRule>();
			rule.RME_EFreightStatus = "XXX";

			Assert("XXX is not a correct efreight status", rule.RME_EFreightStatusInfo.HasError("Enter a valid selection."));

			rule.RME_EFreightStatus = "NON";

			Assert("NON is a correct efreight status", !rule.RME_EFreightStatusInfo.HasErrors());

			rule.RME_EFreightStatus = string.Empty;

			Assert("RME_EFreightStatus can be empty", !rule.RME_EFreightStatusInfo.HasErrors());
		}
	}
}
