using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCTariffRuleCollection))]
	sealed class USCTariffRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<USCTariffRuleCollection>
	{
		public void TestSetDefaultValuesForNewElement()
		{
			USCRule rule = Factory.New<USCRule>();
			rule.U0_Code = "AAA";

			USCTariffRuleCollection coll = new USCTariffRuleCollection(rule);
			USCTariffRule newElement = coll.AddNew();

			AssertEquals("RuleCode is defaulted", "AAA", newElement.U1_RuleCode);
			AssertEquals("U1_DateFrom", ZDateTime.Empty, newElement.U1_DateFrom);
			AssertEquals("U1_DateTo", ZDateTime.Empty, newElement.U1_DateTo);

			newElement.U1_DateFrom = ZDateTime.BrettsBirthday;
			newElement.U1_DateTo = ZDateTime.BrettsBirthday.AddYears(10);

			newElement = coll.AddNew();
			AssertEquals("RuleCode is defaulted", "AAA", newElement.U1_RuleCode);
			AssertEquals("U1_DateFrom", ZDateTime.BrettsBirthday, newElement.U1_DateFrom);
			AssertEquals("U1_DateTo", ZDateTime.BrettsBirthday.AddYears(10), newElement.U1_DateTo);
		}
	}
}
