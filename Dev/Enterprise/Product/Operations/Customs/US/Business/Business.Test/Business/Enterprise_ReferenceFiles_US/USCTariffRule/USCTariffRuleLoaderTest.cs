using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCTariffRule.Loader))]
	public class USCTariffRuleLoaderTest : LoaderTestCase
	{
		public void TestHumanReadableName()
		{
			var tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_Tariff = "1901208000";

			AssertEquals("Tariff Rule 1901208000", tariffRule.HumanReadableName);
		}

		public void TestLoadWithOneDate()
		{
			USCTariffRule rule1 = CreateTariffRule("AAA", "000000", "", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));
			USCTariffRule rule2 = CreateTariffRule("AAA", "000000", "", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1), ZDateTime.Empty);
			USCTariffRule rule3 = CreateTariffRule("AAA", "0000010000", "", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			USCTariffRule rule4 = CreateTariffRule("AAA", "000002", "", ZDateTime.BrettsBirthday, ZDateTime.Empty);

			USCTariffRule[] loaded = loader.LoadDuplicates("AAA", "000000", "000000", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			AssertEquals(1, loaded.Length);
			AssertEquals(rule1, loaded[0]);

			loaded = loader.LoadDuplicates("AAA", "000000", "000000", ZDateTime.BrettsBirthday.AddYears(10), ZDateTime.Empty);
			AssertEquals(1, loaded.Length);
			AssertEquals(rule1, loaded[0]);

			loaded = loader.LoadDuplicates("AAA", "000000", "000000", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1), ZDateTime.Empty);
			AssertEquals(2, loaded.Length);
		}

		public void TestReadOnly()
		{
			var rule = Factory.New<USCTariffRule>();
			AssertEquals("USCTariffRule should be read only", true, rule.ReadOnly);
		}

		public void TestLoadWithTwoDates()
		{
			USCTariffRule rule1 = CreateTariffRule("AAA", "3005101000", "3005905090", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));
			USCTariffRule rule2 = CreateTariffRule("AAA", "3005101000", "3005905090", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1), ZDateTime.Empty);
			USCTariffRule rule3 = CreateTariffRule("AAA", "3926903300", "", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			USCTariffRule rule4 = CreateTariffRule("AAA", "3926905500", "3926906090", ZDateTime.BrettsBirthday, ZDateTime.Empty);

			//load a rule that is valid between ZDateTime.BrettsBirthday and ZDateTime.BrettsBirthday.AddYears(1)
			USCTariffRule[] loaded = loader.LoadDuplicates("AAA", "3005102000", "3005805090", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(1));
			AssertEquals(1, loaded.Length);
			AssertEquals(rule1, loaded[0]);

			ZDateTime tenYearsAndOneDayPassed = ZDateTime.BrettsBirthday.AddYears(10).AddDays(1);
			loaded = loader.LoadDuplicates("AAA", "3005102000", "3005805090", tenYearsAndOneDayPassed, tenYearsAndOneDayPassed.AddDays(30));
			AssertEquals(1, loaded.Length);
			AssertEquals(rule2, loaded[0]);
		}

		public void TestLoadWithNoTariffNumberOrDateFrom()
		{
			USCTariffRule rule1 = CreateTariffRule("AAA", "000000", "", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));
			USCTariffRule rule2 = CreateTariffRule("AAA", "000000", "", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1), ZDateTime.Empty);
			USCTariffRule rule3 = CreateTariffRule("AAA", "0000010000", "", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			USCTariffRule rule4 = CreateTariffRule("AAA", "000002", "", ZDateTime.BrettsBirthday, ZDateTime.Empty);

			USCTariffRule[] loaded = loader.LoadDuplicates("AAA", "", "000000", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(1));
			AssertEquals(0, loaded.Length);

			loaded = loader.LoadDuplicates("AAA", "000000", "000000", ZDateTime.Empty, ZDateTime.Empty);
			AssertEquals(0, loaded.Length);
		}

		public void TestLoadWithCompleteTariffNumberAgainstRuleWithPartialTariffNumber()
		{
			USCTariffRule rule1 = CreateTariffRule("AAA", "000000", "", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));
			USCTariffRule rule2 = CreateTariffRule("AAA", "000000", "", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1), ZDateTime.Empty);
			USCTariffRule rule3 = CreateTariffRule("AAA", "0000010000", "", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			USCTariffRule rule4 = CreateTariffRule("AAA", "000002", "", ZDateTime.BrettsBirthday, ZDateTime.Empty);

			USCTariffRule[] loaded = loader.LoadDuplicates("AAA", "0000020000", "0000020000", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			AssertEquals(1, loaded.Length);
			AssertEquals(rule4, loaded[0]);//it also tests loading records with empty date-to as rule4 has no date-to
		}

		public void TestLoadRecordsWhichHaveTwoTariffNumbers()
		{
			USCTariffRule rule1 = CreateTariffRule("AAA", "3005101000", "3005905090", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddYears(10));
			USCTariffRule rule2 = CreateTariffRule("AAA", "3005101000", "3005801000", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1), ZDateTime.Empty);
			USCTariffRule rule3 = CreateTariffRule("AAA", "3918103110", "3918905000", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1), ZDateTime.Empty);
			USCTariffRule rule4 = CreateTariffRule("AAA", "3926903300", "", ZDateTime.BrettsBirthday, ZDateTime.Empty);

			USCTariffRule[] loaded = loader.LoadDuplicates("AAA", "3005401000", "3005401000", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			AssertEquals(1, loaded.Length);
			AssertEquals(rule1, loaded[0]);

			loaded = loader.LoadDuplicates("AAA", "3005905099", "3005905099", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			AssertEquals(0, loaded.Length);

			loaded = loader.LoadDuplicates("AAA", "3918203110", "3918203110", ZDateTime.Today, ZDateTime.Empty);
			AssertEquals(1, loaded.Length);
			AssertEquals(rule3, loaded[0]);

			loaded = loader.LoadDuplicates("AAA", "3926903400", "3926903400", ZDateTime.Today, ZDateTime.Empty);
			AssertEquals(0, loaded.Length);
		}

		USCTariffRule CreateTariffRule(ZString ruleCode, ZString tariffNumberFrom, ZString tariffNumberTo, ZDateTime dateFrom, ZDateTime dateTo)
		{
			USCTariffRule result = Factory.New<USCTariffRule>();
			result.U1_RuleCode = ruleCode;
			result.U1_Tariff = tariffNumberFrom;
			result.U1_TariffTo = tariffNumberTo;
			result.U1_DateFrom = dateFrom;
			result.U1_DateTo = dateTo;
			return result;
		}

		USCTariffRule.Loader loader;

		protected override void SetUp()
		{
			base.SetUp();
			loader = new USCTariffRule.Loader(Factory);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new USCTariffRule.Loader(Factory);
		}
	}
}
