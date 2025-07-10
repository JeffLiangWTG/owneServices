using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(LinkedCusAuthorisationRuleCollection))]
	sealed class LinkedCusAuthorisationRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<LinkedCusAuthorisationRuleCollection>
	{
		public void TestDefaultsForNewElement()
		{
			var authRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
			authRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			var linkedRule = authRule.LinkedCusAuthorisationRules.AddNew();
			CombineAssertions("New linked rules for Non MCO should be created with standard defaults", () =>
			{
				AssertEquals(nameof(linkedRule.CPR_CPH_PermitHeader), authRule.CPR_CPH_PermitHeader, linkedRule.CPR_CPH_PermitHeader);
				AssertEquals(nameof(linkedRule.CPR_RuleCode), LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, linkedRule.CPR_RuleCode);
			});

			authRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.MainCustomsOffice;
			linkedRule = authRule.LinkedCusAuthorisationRules.AddNew();
			CombineAssertions("New linked rules for MCO should be created with specific defaults", () =>
			{
				AssertEquals(nameof(linkedRule.CPR_CPH_PermitHeader), authRule.CPR_CPH_PermitHeader, linkedRule.CPR_CPH_PermitHeader);
				AssertEquals(nameof(linkedRule.CPR_RuleCode), LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices, linkedRule.CPR_RuleCode);
			});
		}

		protected override LinkedCusAuthorisationRuleCollection GetCollectionToTest() => new LinkedCusAuthorisationRuleCollection(Factory.NewWithValidTestData<CusAuthorisationRule>());
	}
}
