using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(LinkedCusAuthorisationRuleCollection))]
	sealed class LinkedCusAuthorisationRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<LinkedCusAuthorisationRuleCollection>
	{
		public void TestDefaultsForNewElement()
		{
			var authorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
			var linkedRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("CPR_CPH_PermitHeader", authorisationRule.CPR_CPH_PermitHeader, linkedRule.CPR_CPH_PermitHeader);
				AssertEquals("CPR_RuleCode", LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, linkedRule.CPR_RuleCode);
			});
		}

		public void TestLoad()
		{
			var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			var authorisationRule1 = authorisationHeader.CusAuthorisationRules.AddNew();
			var authorisationRule2 = authorisationHeader.CusAuthorisationRules.AddNew();
			var authorisationRule3 = authorisationHeader.CusAuthorisationRules.AddNew();

			var linkedRule1 = authorisationRule2.LinkedCusAuthorisationRules.AddNew();
			var linkedRule2 = authorisationRule3.LinkedCusAuthorisationRules.AddNew();
			var linkedRule3 = authorisationRule3.LinkedCusAuthorisationRules.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("authorisationRule1.LinkedCusAuthorisationRules", 0, authorisationRule1.LinkedCusAuthorisationRules.Count);
				AssertContainsExactElementsInAnyOrder("authorisationRule2.LinkedCusAuthorisationRules", new[] { linkedRule1 }, authorisationRule2.LinkedCusAuthorisationRules);
				AssertContainsExactElementsInAnyOrder("authorisationRule3.LinkedCusAuthorisationRules", new[] { linkedRule2, linkedRule3 }, authorisationRule3.LinkedCusAuthorisationRules);
			});
		}

		protected override LinkedCusAuthorisationRuleCollection GetCollectionToTest() => Factory.New<CusAuthorisationRule>().LinkedCusAuthorisationRules;

		protected override BusinessObject GetNewElementToAddToTheCollection() => GetCollectionToTest().AddNew();
	}
}
