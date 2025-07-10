using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCRuleSecondaryTariffCollection))]
	sealed class USCRuleSecondaryTariffCollectionTest : ActiveBusinessObjectCollectionTestCase<USCRuleSecondaryTariffCollection>
	{
		protected override USCRuleSecondaryTariffCollection GetCollectionToTest()
		{
			return new USCRuleSecondaryTariffCollection(TariffRule);
		}

		public void TestGetMatchingSecondaryTariffRule()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = "AAA";
			tariffRule.U1_Tariff = "9902";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;

			USCRuleSecondaryTariff secondaryTariff1 = tariffRule.SecondaryTariffs.AddNew();
			secondaryTariff1.U3_TariffFrom = "020200";
			secondaryTariff1.U3_DateFrom = ZDateTime.BrettsBirthday;
			secondaryTariff1.U3_DateTo = ZDateTime.BrettsBirthday.AddYears(10);

			USCRuleSecondaryTariff secondaryTariff2 = tariffRule.SecondaryTariffs.AddNew();
			secondaryTariff2.U3_TariffFrom = "010125";
			secondaryTariff2.U3_DateFrom = ZDateTime.BrettsBirthday;

			AssertEquals("Applies to 0202.00.00.00", secondaryTariff1, tariffRule.SecondaryTariffs.GetMatchingSecondaryTariffRule("0202000000", ZDateTime.BrettsBirthday.AddYears(10)));
			AssertEquals("Applies to 0202.00.06.54", secondaryTariff1, tariffRule.SecondaryTariffs.GetMatchingSecondaryTariffRule("0202000654", ZDateTime.BrettsBirthday));
			AssertNull("Applies to 0202.00.00.00", tariffRule.SecondaryTariffs.GetMatchingSecondaryTariffRule("0202000000", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1)));

			AssertEquals("Applies to 0101.25.00.00", secondaryTariff2, tariffRule.SecondaryTariffs.GetMatchingSecondaryTariffRule("0101250000", ZDateTime.Now));
			AssertNull("Applies to 0101.25.00.00", tariffRule.SecondaryTariffs.GetMatchingSecondaryTariffRule("0101250000", ZDateTime.BrettsBirthday.AddDays(-1)));

			AssertNull("Applies to 9999.35.00 00", tariffRule.SecondaryTariffs.GetMatchingSecondaryTariffRule("9999350000", ZDateTime.BrettsBirthday));
		}

		public void TestGetDuplicatesFor()
		{
			USCTariffRule rule1 = CreateTariffRule("AAA", "3005101000", "3005905090", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));
			USCRuleSecondaryTariff secondaryTariff1 = CreateSecondaryTariff(rule1, "3005101020", "3005201010", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));

			AssertEquals("no duplicate", null, rule1.SecondaryTariffs.GetABroaderSecondaryTariffThan(secondaryTariff1));

			USCRuleSecondaryTariff secondaryTariff2 = CreateSecondaryTariff(rule1, "3005101010", "3005201010", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			AssertEquals("secondaryTariff1 is covered with secondaryTariff2", secondaryTariff2, rule1.SecondaryTariffs.GetABroaderSecondaryTariffThan(secondaryTariff1));

			USCTariffRule rule2 = CreateTariffRule("AAA", "3005101000", "3005801000", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1), ZDateTime.Empty);
			USCRuleSecondaryTariff secondaryTariff3 = CreateSecondaryTariff(rule2, "300520", "", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));
			USCRuleSecondaryTariff secondaryTariff4 = CreateSecondaryTariff(rule2, "300530", "", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));
			AssertEquals("no duplicate", null, rule2.SecondaryTariffs.GetABroaderSecondaryTariffThan(secondaryTariff4));

			secondaryTariff4.U3_TariffFrom = "30052010";
			AssertEquals("secondaryTariff4 is covered with secondaryTariff3", secondaryTariff3, rule2.SecondaryTariffs.GetABroaderSecondaryTariffThan(secondaryTariff4));
			AssertEquals("secondaryTariff4 is covered with secondaryTariff3", null, rule2.SecondaryTariffs.GetABroaderSecondaryTariffThan(secondaryTariff3));

			USCTariffRule rule4 = CreateTariffRule("AAA", "39269033", "", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			USCRuleSecondaryTariff secondaryTariff5 = CreateSecondaryTariff(rule4, "3926903310", "3926903320", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));
			USCRuleSecondaryTariff secondaryTariff6 = CreateSecondaryTariff(rule4, "3926903311", "", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));

			AssertEquals("secondaryTariff6 is covered with secondaryTariff5", secondaryTariff5, rule4.SecondaryTariffs.GetABroaderSecondaryTariffThan(secondaryTariff6));
			AssertEquals("secondaryTariff6 is covered with secondaryTariff5", null, rule4.SecondaryTariffs.GetABroaderSecondaryTariffThan(secondaryTariff5));

			secondaryTariff6.U3_DateTo = ZDateTime.Empty;
			AssertEquals("secondaryTariff6 is valid up to today", null, rule4.SecondaryTariffs.GetABroaderSecondaryTariffThan(secondaryTariff6));
		}

		USCTariffRule CreateTariffRule(ZString ruleCode, ZString tariffFrom, ZString tariffTo, ZDateTime dateFrom, ZDateTime dateTo)
		{
			USCTariffRule result = Factory.New<USCTariffRule>();
			result.U1_RuleCode = ruleCode;
			result.U1_Tariff = tariffFrom;
			result.U1_TariffTo = tariffTo;
			result.U1_DateFrom = dateFrom;
			result.U1_DateTo = dateTo;
			return result;
		}

		USCRuleSecondaryTariff CreateSecondaryTariff(USCTariffRule rule, ZString tariffFrom, ZString tariffTo, ZDateTime dateFrom, ZDateTime dateTo)
		{
			USCRuleSecondaryTariff result = rule.SecondaryTariffs.AddNew();
			result.U3_TariffFrom = tariffFrom;
			result.U3_TariffTo = tariffTo;
			result.U3_DateFrom = dateFrom;
			result.U3_DateTo = dateTo;
			return result;
		}

		USCTariffRule TariffRule
		{
			get
			{
				if (tariffRule == null)
				{
					tariffRule = Factory.New<USCTariffRule>();
				}
				return tariffRule;
			}
		}
		USCTariffRule tariffRule;
	}
}
