using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCRuleSecondaryTariff))]
	sealed class USCRuleSecondaryTariffTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsValid()
		{
			USCTariffRule parentRule = Factory.New<USCTariffRule>();
			parentRule.U1_Tariff = "9101401010";
			parentRule.U1_DateFrom = ZDateTime.BrettsBirthday;

			USCRuleSecondaryTariff secondaryTariff = parentRule.SecondaryTariffs.AddNew();
			secondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday;
			secondaryTariff.U3_DateTo = ZDateTime.BrettsBirthday.AddYears(10);
			secondaryTariff.U3_TariffFrom = "9101401020";
			secondaryTariff.U3_Tariff2 = "9101401030";
			secondaryTariff.U3_Tariff3 = "9101401040";

			Assert(secondaryTariff.IsValid("9101401020", ZDateTime.BrettsBirthday));
			Assert(secondaryTariff.IsValid("9101401030", ZDateTime.BrettsBirthday));
			Assert(secondaryTariff.IsValid("9101401040", ZDateTime.BrettsBirthday));

			Assert(!secondaryTariff.IsValid("9101401020", ZDateTime.Today));
			Assert(!secondaryTariff.IsValid("9101401030", ZDateTime.Today));
			Assert(!secondaryTariff.IsValid("9101401040", ZDateTime.Today));

			Assert(!secondaryTariff.IsValid("9101401025", ZDateTime.BrettsBirthday));
		}

		public void TestFormattedTariff()
		{
			USCRuleSecondaryTariff ruleSecondaryTariff = Factory.New<USCRuleSecondaryTariff>();
			ruleSecondaryTariff.U3_TariffFrom = "00000000";
			AssertEquals("0000.00.00", ruleSecondaryTariff.FormattedTariffFrom);

			ruleSecondaryTariff.FormattedTariffFrom = "";
			AssertEquals("", ruleSecondaryTariff.U3_TariffFrom);

			ruleSecondaryTariff.FormattedTariffFrom = "0000.00.11";
			AssertEquals("00000011", ruleSecondaryTariff.U3_TariffFrom);

			ruleSecondaryTariff.U3_TariffTo = "10000000";
			AssertEquals("1000.00.00", ruleSecondaryTariff.FormattedTariffTo);

			ruleSecondaryTariff.FormattedTariffTo = "";
			AssertEquals("", ruleSecondaryTariff.U3_TariffTo);

			ruleSecondaryTariff.FormattedTariffTo = "1000.00.11";
			AssertEquals("10000011", ruleSecondaryTariff.U3_TariffTo);
		}

		public void TestFormattedTariff2()
		{
			USCRuleSecondaryTariff ruleSecondaryTariff = Factory.New<USCRuleSecondaryTariff>();
			ruleSecondaryTariff.U3_Tariff2 = "00000000";
			AssertEquals("0000.00.00", ruleSecondaryTariff.FormattedTariff2);

			ruleSecondaryTariff.FormattedTariff2 = "";
			AssertEquals("", ruleSecondaryTariff.U3_Tariff2);

			ruleSecondaryTariff.FormattedTariff2 = "0000.00.11";
			AssertEquals("00000011", ruleSecondaryTariff.U3_Tariff2);
		}

		public void TestFormattedTariff3()
		{
			USCRuleSecondaryTariff ruleSecondaryTariff = Factory.New<USCRuleSecondaryTariff>();
			ruleSecondaryTariff.U3_Tariff3 = "00000000";
			AssertEquals("0000.00.00", ruleSecondaryTariff.FormattedTariff3);

			ruleSecondaryTariff.FormattedTariff3 = "";
			AssertEquals("", ruleSecondaryTariff.U3_Tariff3);

			ruleSecondaryTariff.FormattedTariff3 = "0000.00.11";
			AssertEquals("00000011", ruleSecondaryTariff.U3_Tariff3);
		}

		public void TestEffectiveTariffTo()
		{
			USCTariffRule rule = Factory.New<USCTariffRule>();
			USCRuleSecondaryTariff ruleSecondaryTariff = rule.SecondaryTariffs.AddNew();
			ruleSecondaryTariff.U3_TariffFrom = "98012010";
			AssertEquals("98012010", ruleSecondaryTariff.EffectiveTariffTo);

			ruleSecondaryTariff.U3_TariffTo = "98012020";
			AssertEquals("98012020", ruleSecondaryTariff.EffectiveTariffTo);
		}

		public void TestHasBroaderTariffRangesThan()
		{
			USCTariffRule rule = Factory.New<USCTariffRule>();
			USCRuleSecondaryTariff ruleSecondaryTariff = rule.SecondaryTariffs.AddNew();
			ruleSecondaryTariff.U3_TariffFrom = "9801";

			USCRuleSecondaryTariff ruleSecondaryTariff2 = rule.SecondaryTariffs.AddNew();
			ruleSecondaryTariff2.U3_TariffFrom = "980110";
			AssertEquals(true, ruleSecondaryTariff.HasBroaderTariffRangesThan(ruleSecondaryTariff2));
			AssertEquals(false, ruleSecondaryTariff2.HasBroaderTariffRangesThan(ruleSecondaryTariff));

			ruleSecondaryTariff2.U3_TariffTo = "980111";
			AssertEquals(true, ruleSecondaryTariff.HasBroaderTariffRangesThan(ruleSecondaryTariff2));

			USCRuleSecondaryTariff ruleSecondaryTariff3 = rule.SecondaryTariffs.AddNew();
			ruleSecondaryTariff3.U3_TariffFrom = "980100";
			AssertEquals(false, ruleSecondaryTariff3.HasBroaderTariffRangesThan(ruleSecondaryTariff2));

			ruleSecondaryTariff3.U3_TariffTo = "980112";
			AssertEquals(true, ruleSecondaryTariff3.HasBroaderTariffRangesThan(ruleSecondaryTariff2));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			USCRuleSecondaryTariff ruleSecondaryTariff = factory.New<USCRuleSecondaryTariff>();
			ruleSecondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday;
			return ruleSecondaryTariff;
		}
	}
}
