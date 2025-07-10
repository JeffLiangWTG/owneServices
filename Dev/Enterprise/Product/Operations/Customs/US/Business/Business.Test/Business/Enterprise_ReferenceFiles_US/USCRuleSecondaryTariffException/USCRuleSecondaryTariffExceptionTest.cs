using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCRuleSecondaryTariffException))]
	sealed class USCRuleSecondaryTariffExceptionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFormattedTariff()
		{
			USCRuleSecondaryTariffException ruleSecondaryTariff = Factory.New<USCRuleSecondaryTariffException>();
			ruleSecondaryTariff.U4_Tariff = "00000000";
			AssertEquals("0000.00.00", ruleSecondaryTariff.FormattedTariff);

			ruleSecondaryTariff.FormattedTariff = "";
			AssertEquals("", ruleSecondaryTariff.U4_Tariff);

			ruleSecondaryTariff.FormattedTariff = "0000.00.11";
			AssertEquals("00000011", ruleSecondaryTariff.U4_Tariff);
		}

		public void TestSecondaryTariffRule()
		{
			USCRuleSecondaryTariff ruleSecondaryTariff = Factory.New<USCRuleSecondaryTariff>();

			USCRuleSecondaryTariffException exception = ruleSecondaryTariff.Exceptions.AddNew();
			AssertEquals(ruleSecondaryTariff, exception.SecondaryTariffRule);
		}

		public void TestHasBroaderTariffRangesThan()
		{
			USCRuleSecondaryTariff ruleSecondaryTariff = Factory.New<USCRuleSecondaryTariff>();
			ruleSecondaryTariff.U3_TariffFrom = "9801";

			USCRuleSecondaryTariffException exception1 = ruleSecondaryTariff.Exceptions.AddNew();
			exception1.U4_Tariff = "980110";

			USCRuleSecondaryTariffException exception2 = ruleSecondaryTariff.Exceptions.AddNew();
			exception2.U4_Tariff = "98011010";

			AssertEquals(true, exception1.HasBroaderTariffRangesThan(exception2));
			AssertEquals(false, exception2.HasBroaderTariffRangesThan(exception1));
		}

		public void TestApplies()
		{
			USCRuleSecondaryTariff ruleSecondaryTariff = Factory.New<USCRuleSecondaryTariff>();

			USCRuleSecondaryTariffException exception = ruleSecondaryTariff.Exceptions.AddNew();
			exception.U4_Tariff = "980120";
			exception.U4_DateFrom = ZDateTime.BrettsBirthday;
			exception.U4_DateTo = ZDateTime.BrettsBirthday.AddYears(10);

			Assert(!exception.Applies("980121", ZDateTime.BrettsBirthday));
			Assert(exception.Applies("9801201010", ZDateTime.BrettsBirthday));
			Assert(!exception.Applies("98012011", ZDateTime.Today));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return SecondaryTariffRule.Exceptions.AddNew();
		}

		USCRuleSecondaryTariff SecondaryTariffRule
		{
			get
			{
				if (secondaryTariffRule == null)
				{
					secondaryTariffRule = TariffRule.SecondaryTariffs.AddNew();
					secondaryTariffRule.U3_DateFrom = ZDateTime.BrettsBirthday;
				}
				return secondaryTariffRule;
			}
		}
		USCRuleSecondaryTariff secondaryTariffRule;

		USCTariffRule TariffRule
		{
			get
			{
				if (fTariffRule == null)
				{
					fTariffRule = Factory.New<USCTariffRule>();
					fTariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
				}
				return fTariffRule;
			}
		}
		USCTariffRule fTariffRule;
	}
}
