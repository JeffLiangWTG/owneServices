using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCRule))]
	sealed class USCRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var rule = Factory.New<USCRule>();
			rule.U0_Code = "A99";

			AssertEquals("Rule A99", rule.HumanReadableName);
		}

		public void TestRuleCodeDescription()
		{
			USCRule rule = Factory.New<USCRule>();
			AssertEquals("", rule.RuleCodeDescription);

			rule.U0_Code = TariffRuleList.Codes.AdditionalTariffs;
			AssertEquals(TariffRuleList.Descriptions.AdditionalTariffs, rule.RuleCodeDescription);
		}

		public void TestReadOnly()
		{
			var rule = Factory.New<USCRule>();
			AssertEquals("USCRule should be read only", true, rule.ReadOnly);
		}

		public void TestTariffs()
		{
			USCRule rule = Factory.New<USCRule>();
			rule.U0_Code = "AAA";

			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = "AAA";
			tariffRule.U1_Tariff = "0000";

			USCTariffRule tariffRule2 = Factory.New<USCTariffRule>();
			tariffRule2.U1_RuleCode = "AAA";
			tariffRule2.U1_Tariff = "0001";

			USCTariffRule tariffRule3 = Factory.New<USCTariffRule>();
			tariffRule3.U1_RuleCode = "BBB";
			tariffRule3.U1_Tariff = "0001";

			USCRule rule2 = Factory.New<USCRule>();
			rule2.U0_Code = "BBB";

			AssertEquals("two elements", 2, rule.Tariffs.Count);
			AssertEquals(true, rule.Tariffs.Contains(tariffRule));
			AssertEquals(true, rule.Tariffs.Contains(tariffRule2));

			AssertEquals(1, rule2.Tariffs.Count);
			AssertEquals(true, rule2.Tariffs.Contains(tariffRule3));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			USCRule rule = factory.New<USCRule>();
			USCTariffRule tariffRule = rule.Tariffs.AddNew();
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			return rule;
		}
	}
}
