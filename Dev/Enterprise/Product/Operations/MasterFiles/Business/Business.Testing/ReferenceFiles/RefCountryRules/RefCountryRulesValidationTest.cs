using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCountryRulesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMatchIso()
		{
			var rule = Factory.NewWithValidTestData<RefCountryRules>();
			rule.CurrentCountryCode = "AU";

			rule.R7_RN_NKDestination = "CN";
			rule.R7_RN_NKOrigin = "US";
			AssertEquals("At least one of Origin or Destination country/region should be 'AU'.", rule.R7_RN_NKOriginInfo.GetErrors().ToMessageListString());
			AssertEquals("At least one of Origin or Destination country/region should be 'AU'.", rule.R7_RN_NKDestinationInfo.GetErrors().ToMessageListString());

			rule.R7_RN_NKOrigin = "AU";
			rule.Validation.ValidateAll();
			AssertNoErrors(rule.R7_RN_NKOriginInfo);
			AssertNoErrors(rule.R7_RN_NKDestinationInfo);
		}

		public void TestOriginDestinationAllEmpty()
		{
			var rule = Factory.NewWithValidTestData<RefCountryRules>();
			rule.CurrentCountryCode = "AU";

			rule.R7_RN_NKDestination = "";
			rule.R7_RN_NKOrigin = "";
			AssertEquals("At least one of Origin or Destination country/region should be entered", rule.R7_RN_NKOriginInfo.GetErrors().ToMessageListString());
			AssertEquals("At least one of Origin or Destination country/region should be entered", rule.R7_RN_NKDestinationInfo.GetErrors().ToMessageListString());
		}

		public void TestUltimateConsigneeRuleValidation()
		{
			var rule = Factory.New<RefCountryRules>();
			rule.R7_UltimateConsigneeRule = "XXX";
			AssertHasError(rule.R7_UltimateConsigneeRuleInfo, "Enter a valid selection.");

			rule.R7_UltimateConsigneeRule = "VER";
			AssertNoErrors(rule.R7_UltimateConsigneeRuleInfo);

			rule.R7_UltimateConsigneeRule = "";
			AssertNoErrors(rule.R7_UltimateConsigneeRuleInfo);
		}

		public void TestDestinationValidationForUltimateConsigneeRule()
		{
			var expectedError = "Destination is mandatory if Ultimate Consignee Required is set to 'MAN' or 'VER'.";

			var rule = Factory.New<RefCountryRules>();
			rule.R7_UltimateConsigneeRule = "NON";
			AssertNoError(rule.R7_RN_NKDestinationInfo, expectedError);

			rule.R7_UltimateConsigneeRule = "MAN";
			AssertHasError(rule.R7_RN_NKDestinationInfo, expectedError);

			rule.R7_UltimateConsigneeRule = "";
			AssertNoError(rule.R7_RN_NKDestinationInfo, expectedError);
		}
	}
}
