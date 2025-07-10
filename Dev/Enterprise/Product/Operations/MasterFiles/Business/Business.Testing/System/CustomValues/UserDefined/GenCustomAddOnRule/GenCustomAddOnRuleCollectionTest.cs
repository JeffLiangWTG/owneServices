using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[TestedType(typeof(GenCustomAddOnRuleCollection))]
	sealed class GenCustomAddOnRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<GenCustomAddOnRuleCollection>
	{
		public void TestFilter()
		{
			var cw1Rule = Factory.New<GenCustomAddOnRule>();
			var glowRule = Factory.New<GenCustomAddOnRule>();
			glowRule.XR_RuleType = "XYZ";

			var collection = new GenCustomAddOnRuleCollection(Factory);
			AssertCollectionContains(cw1Rule, collection);
			AssertCollectionNotContains(glowRule, collection);
		}
	}
}
