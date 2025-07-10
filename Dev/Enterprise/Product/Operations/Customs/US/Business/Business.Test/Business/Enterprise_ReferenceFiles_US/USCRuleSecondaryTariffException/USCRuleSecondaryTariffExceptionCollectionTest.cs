using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCRuleSecondaryTariffExceptionCollection))]
	sealed class USCRuleSecondaryTariffExceptionCollectionTest : ActiveBusinessObjectCollectionTestCase<USCRuleSecondaryTariffExceptionCollection>
	{
		public void TestSetDefaultsForNewElement()
		{
			USCRuleSecondaryTariff tariffRule = Factory.New<USCRuleSecondaryTariff>();
			tariffRule.U3_TariffFrom = "9902";
			tariffRule.U3_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U3_DateTo = ZDateTime.BrettsBirthday.AddYears(10);

			USCRuleSecondaryTariffException exception = tariffRule.Exceptions.AddNew();

			AssertEquals(ZDateTime.BrettsBirthday, exception.U4_DateFrom);
			AssertEquals(ZDateTime.BrettsBirthday.AddYears(10), exception.U4_DateTo);
		}

		public void TestApplies()
		{
			USCRuleSecondaryTariff tariffRule = Factory.New<USCRuleSecondaryTariff>();
			tariffRule.U3_TariffFrom = "9902";
			tariffRule.U3_DateFrom = ZDateTime.BrettsBirthday;

			USCRuleSecondaryTariffException exception = tariffRule.Exceptions.AddNew();
			exception.U4_Tariff = "990215";
			exception.U4_DateFrom = ZDateTime.BrettsBirthday;
			exception.U4_DateTo = ZDateTime.BrettsBirthday.AddYears(10);

			USCRuleSecondaryTariffException exception2 = tariffRule.Exceptions.AddNew();
			exception2.U4_Tariff = "990225";
			exception2.U4_DateFrom = ZDateTime.BrettsBirthday;

			AssertEquals("Applies to 9902.15.00 00", true, tariffRule.Exceptions.Applies("9902150000", ZDateTime.BrettsBirthday.AddYears(10)));
			AssertEquals("Applies to 9902.15.00 00", false, tariffRule.Exceptions.Applies("9902150000", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1)));

			AssertEquals("Applies to 9902.25.00 00", true, tariffRule.Exceptions.Applies("9902250000", ZDateTime.Now));
			AssertEquals("Applies to 9902.25.00 00", false, tariffRule.Exceptions.Applies("9902250000", ZDateTime.BrettsBirthday.AddDays(-1)));

			AssertEquals("Applies to 9902.35.00 00", false, tariffRule.Exceptions.Applies("9902350000", ZDateTime.BrettsBirthday));
		}

		public void TestGetDuplicatesFor()
		{
			USCRuleSecondaryTariff tariffRule = Factory.New<USCRuleSecondaryTariff>();
			tariffRule.U3_TariffFrom = "9902";
			tariffRule.U3_DateFrom = ZDateTime.BrettsBirthday;

			USCRuleSecondaryTariffException exception = tariffRule.Exceptions.AddNew();
			exception.U4_Tariff = "99021510";
			exception.U4_DateFrom = ZDateTime.BrettsBirthday;
			exception.U4_DateTo = ZDateTime.BrettsBirthday.AddYears(10);

			USCRuleSecondaryTariffException exception2 = tariffRule.Exceptions.AddNew();
			exception2.U4_Tariff = "990215";
			exception2.U4_DateFrom = ZDateTime.BrettsBirthday;

			Assert(!exception.HasBroaderTariffRangesThan(exception2));
			Assert(exception2.HasBroaderTariffRangesThan(exception));

			exception2.U4_DateTo = ZDateTime.BrettsBirthday.AddYears(10);
			Assert(exception2.HasBroaderTariffRangesThan(exception));

			exception2.U4_Tariff = "99021511";
			Assert(!exception2.HasBroaderTariffRangesThan(exception));
		}

		protected override USCRuleSecondaryTariffExceptionCollection GetCollectionToTest()
		{
			return new USCRuleSecondaryTariffExceptionCollection(SecondaryTariffRule);
		}

		USCRuleSecondaryTariff SecondaryTariffRule
		{
			get
			{
				if (secondaryTariffRule == null)
				{
					secondaryTariffRule = TariffRule.SecondaryTariffs.AddNew();
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
				}
				return fTariffRule;
			}
		}
		USCTariffRule fTariffRule;
	}
}
