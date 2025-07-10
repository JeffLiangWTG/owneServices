using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusAuthorisationRuleCollection))]
	sealed class CusAuthorisationRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<CusAuthorisationRuleCollection>
	{
		public void TestLinkedRulesNotInCollection()
		{
			var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			var authorisationRule1 = authorisationHeader.CusAuthorisationRules.AddNew();
			var authorisationRule2 = authorisationHeader.CusAuthorisationRules.AddNew();
			authorisationRule2.LinkedCusAuthorisationRules.AddNew();
			AssertContainsExactElementsInAnyOrder("Linked rules should not be in the collection", new[] { authorisationRule1, authorisationRule2 }, authorisationHeader.CusAuthorisationRules);
		}

		protected override CusAuthorisationRuleCollection GetCollectionToTest()
		{
			return Factory.New<CusAuthorisationHeader>().CusAuthorisationRules;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return GetCollectionToTest().AddNew();
		}
	}
}
