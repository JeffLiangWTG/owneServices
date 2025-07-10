using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCTariffRuleExceptionCollection))]
	sealed class USCTariffRuleExceptionCollectionTest : ActiveBusinessObjectCollectionTestCase<USCTariffRuleExceptionCollection>
	{
		public void TestApplies()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = "AAA";
			tariffRule.U1_Tariff = "9902";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;

			USCTariffRuleException exception = tariffRule.RuleExceptions.AddNew();
			exception.U2_Tariff = "990215";
			exception.U2_DateFrom = ZDateTime.BrettsBirthday;
			exception.U2_DateTo = ZDateTime.BrettsBirthday.AddYears(10);

			USCTariffRuleException exception2 = tariffRule.RuleExceptions.AddNew();
			exception2.U2_Tariff = "990225";
			exception2.U2_DateFrom = ZDateTime.BrettsBirthday;

			AssertEquals("Applies to 9902.15.00 00", true, tariffRule.RuleExceptions.Applies("9902150000", ZDateTime.BrettsBirthday.AddYears(10)));
			AssertEquals("Applies to 9902.15.00 00", false, tariffRule.RuleExceptions.Applies("9902150000", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1)));

			AssertEquals("Applies to 9902.25.00 00", true, tariffRule.RuleExceptions.Applies("9902250000", ZDateTime.Now));
			AssertEquals("Applies to 9902.25.00 00", false, tariffRule.RuleExceptions.Applies("9902250000", ZDateTime.BrettsBirthday.AddDays(-1)));

			AssertEquals("Applies to 9902.35.00 00", false, tariffRule.RuleExceptions.Applies("9902350000", ZDateTime.BrettsBirthday));
		}

		public void TestGetDuplicatesFor()
		{
			USCTariffRule rule1 = CreateTariffRule("AAA", "3005101000", "3005905090", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));
			USCTariffRuleException exception1 = CreateRuleException(rule1, "3005101020", "3005201010", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));

			AssertEquals("no duplicate", null, rule1.RuleExceptions.GetABroaderExceptionThan(exception1));

			USCTariffRuleException exception2 = CreateRuleException(rule1, "3005101010", "3005201010", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			AssertEquals("exception1 is covered with exception2", exception2, rule1.RuleExceptions.GetABroaderExceptionThan(exception1));
			AssertEquals("exception1 is covered with exception2", null, rule1.RuleExceptions.GetABroaderExceptionThan(exception2));

			USCTariffRule rule2 = CreateTariffRule("AAA", "3005101000", "3005801000", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1), ZDateTime.Empty);
			USCTariffRuleException exception3 = CreateRuleException(rule2, "300520", "", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));
			USCTariffRuleException exception4 = CreateRuleException(rule2, "300530", "", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));
			AssertEquals("no duplicate", null, rule2.RuleExceptions.GetABroaderExceptionThan(exception4));

			exception4.U2_Tariff = "30052010";
			AssertEquals("exception4 is covered with exception3", exception3, rule2.RuleExceptions.GetABroaderExceptionThan(exception4));
			AssertEquals("exception4 is covered with exception3", null, rule2.RuleExceptions.GetABroaderExceptionThan(exception3));

			USCTariffRule rule4 = CreateTariffRule("AAA", "39269033", "", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			USCTariffRuleException exception5 = CreateRuleException(rule4, "3926903310", "3926903320", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));
			USCTariffRuleException exception6 = CreateRuleException(rule4, "3926903311", "", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));

			AssertEquals("exception6 is covered with exception5", exception5, rule4.RuleExceptions.GetABroaderExceptionThan(exception6));
			AssertEquals("exception6 is covered with exception5", null, rule4.RuleExceptions.GetABroaderExceptionThan(exception5));

			exception6.U2_DateTo = ZDateTime.Empty;
			AssertEquals("exception6 is valid up to today", null, rule4.RuleExceptions.GetABroaderExceptionThan(exception6));
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

		USCTariffRuleException CreateRuleException(USCTariffRule rule, ZString tariffFrom, ZString tariffTo, ZDateTime dateFrom, ZDateTime dateTo)
		{
			USCTariffRuleException result = rule.RuleExceptions.AddNew();
			result.U2_Tariff = tariffFrom;
			result.U2_TariffTo = tariffTo;
			result.U2_DateFrom = dateFrom;
			result.U2_DateTo = dateTo;
			return result;
		}

		protected override USCTariffRuleExceptionCollection GetCollectionToTest()
		{
			return new USCTariffRuleExceptionCollection(TariffRule);
		}

		USCTariffRule TariffRule
		{
			get
			{
				if (fTariffRule == null)
				{
					fTariffRule = Factory.New<USCTariffRule>();
				}
				return fTariffRule;
			}
		}
		USCTariffRule fTariffRule;
	}
}
