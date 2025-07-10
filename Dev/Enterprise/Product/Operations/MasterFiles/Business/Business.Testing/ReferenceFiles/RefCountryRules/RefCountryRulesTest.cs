using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCountryRules))]
	sealed class RefCountryRulesTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidateMatchWithIso()
		{
			var rule = Factory.NewWithValidTestData<RefCountryRules>();
			rule.CurrentCountryCode = "CN";

			rule.Validation.ValidateAll();
			AssertHasError(rule.R7_RN_NKOriginInfo, "At least one of Origin or Destination country/region should be entered");
			AssertHasError(rule.R7_RN_NKDestinationInfo, "At least one of Origin or Destination country/region should be entered");

			rule.R7_RN_NKOrigin = "US";
			AssertHasError(rule.R7_RN_NKOriginInfo, "At least one of Origin or Destination country/region should be 'CN'.");
			AssertHasError(rule.R7_RN_NKDestinationInfo, "At least one of Origin or Destination country/region should be 'CN'.");

			rule.R7_RN_NKDestination = "AU";
			AssertHasError(rule.R7_RN_NKOriginInfo, "At least one of Origin or Destination country/region should be 'CN'.");
			AssertHasError(rule.R7_RN_NKDestinationInfo, "At least one of Origin or Destination country/region should be 'CN'.");

			rule.R7_RN_NKOrigin = "CN";
			AssertNoErrors(rule.R7_RN_NKOriginInfo);
			AssertNoErrors(rule.R7_RN_NKDestinationInfo);
		}
	}
}
