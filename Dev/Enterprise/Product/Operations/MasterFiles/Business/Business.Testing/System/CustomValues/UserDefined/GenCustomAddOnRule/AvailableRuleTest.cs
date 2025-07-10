using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[TestedType(typeof(AvailableRule))]
	sealed class AvailableRuleTest : NonPersistentBusinessObjectTestCase
	{
		public void TestName()
		{
			GenCustomAddOnRule addOnRule = Factory.New<GenCustomAddOnRule>();
			foreach (AvailableRule rule in addOnRule.AllRules)
			{
				Assert("Please enter a resource string.", !rule.Name.EndsWith(".Name"));
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AvailableRule(Factory) { Rule = new CheckEnteredRule() };
		}

		#endregion
	}
}
