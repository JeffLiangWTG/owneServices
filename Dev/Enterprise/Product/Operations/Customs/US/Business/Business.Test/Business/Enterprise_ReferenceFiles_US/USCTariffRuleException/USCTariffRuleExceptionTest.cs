using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCTariffRuleException))]
	sealed class USCTariffRuleExceptionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestApplies()
		{
			USCTariffRule rule = Factory.New<USCTariffRule>();
			rule.U1_Tariff = "9801";
			rule.U1_DateFrom = ZDateTime.BrettsBirthday;

			USCTariffRuleException exception = rule.RuleExceptions.AddNew();
			exception.U2_Tariff = "98012010";
			exception.U2_DateFrom = ZDateTime.BrettsBirthday;
			exception.U2_DateTo = ZDateTime.BrettsBirthday.AddYears(10);

			Assert(exception.Applies("9801201010", ZDateTime.BrettsBirthday));
			Assert(!exception.Applies("98012011", ZDateTime.Today));

			exception.U2_TariffTo = "98012020";
			Assert(exception.Applies("98012011", ZDateTime.BrettsBirthday));
		}

		public void TestEffectiveTariffTo()
		{
			USCTariffRule rule = Factory.New<USCTariffRule>();
			USCTariffRuleException exception = rule.RuleExceptions.AddNew();
			exception.U2_Tariff = "98012010";
			AssertEquals("98012010", exception.EffectiveTariffTo);

			exception.U2_TariffTo = "98012020";
			AssertEquals("98012020", exception.EffectiveTariffTo);
		}

		public void TestHasBroaderTariffRangesThan()
		{
			USCTariffRule rule = Factory.New<USCTariffRule>();
			USCTariffRuleException exception = rule.RuleExceptions.AddNew();
			exception.U2_Tariff = "9801";

			USCTariffRuleException exception2 = rule.RuleExceptions.AddNew();
			exception2.U2_Tariff = "980110";
			AssertEquals(true, exception.HasBroaderTariffRangesThan(exception2));
			AssertEquals(false, exception2.HasBroaderTariffRangesThan(exception));

			exception2.U2_TariffTo = "980111";
			AssertEquals(true, exception.HasBroaderTariffRangesThan(exception2));

			USCTariffRuleException exception3 = rule.RuleExceptions.AddNew();
			exception3.U2_Tariff = "980100";
			AssertEquals(false, exception3.HasBroaderTariffRangesThan(exception2));

			exception3.U2_TariffTo = "980112";
			AssertEquals(true, exception3.HasBroaderTariffRangesThan(exception2));
		}

		public void TestFormattedTariff()
		{
			USCTariffRule rule = Factory.New<USCTariffRule>();
			USCTariffRuleException exception = rule.RuleExceptions.AddNew();

			exception.U2_Tariff = "00000000";
			AssertEquals("0000.00.00", exception.FormattedTariff);

			exception.FormattedTariff = "";
			AssertEquals("", exception.U2_Tariff);

			exception.FormattedTariff = "0000.00.11";
			AssertEquals("00000011", exception.U2_Tariff);
		}

		public void TestTariffRule()
		{
			USCTariffRule rule = Factory.New<USCTariffRule>();
			USCTariffRuleException exception = rule.RuleExceptions.AddNew();
			AssertEquals(rule, exception.TariffRule);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			USCTariffRule rule = factory.New<USCTariffRule>();
			rule.U1_DateFrom = ZDateTime.BrettsBirthday;
			return rule.RuleExceptions.AddNew();
		}
	}
}
