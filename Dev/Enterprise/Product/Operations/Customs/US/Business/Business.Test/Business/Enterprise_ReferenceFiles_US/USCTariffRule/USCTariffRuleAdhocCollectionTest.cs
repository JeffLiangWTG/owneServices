using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCTariffRuleAdhocCollection))]
	sealed class USCTariffRuleAdhocCollectionTest : ActiveBusinessObjectCollectionTestCase<USCTariffRuleAdhocCollection>
	{
		public void TestLoadForSTNRule()
		{
			var tariffRule1 = CreateTariffRule(TariffRuleList.Codes.EligibleForSecondaryTariffNumbers, new ZDateTime(2007, 1, 1), ZDateTime.Empty, "0101101000", ZString.Empty);
			var tariffRule2 = CreateTariffRule(TariffRuleList.Codes.EligibleForSecondaryTariffNumbers, new ZDateTime(2007, 1, 1), ZDateTime.Empty, "0101101001", ZString.Empty);
			var tariffRule3 = CreateTariffRule(TariffRuleList.Codes.AssembledAbroadOfUSProducts, new ZDateTime(2007, 1, 1), ZDateTime.Empty, "0101", ZString.Empty);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0101101000";
			tariff.UE_DateFrom = new ZDateTime(2007, 1, 1);
			tariff.UE_DateTo = new ZDateTime(2007, 7, 1);

			Assert("Should contain tariffRule1", tariff.TariffRules.Contains(tariffRule1));
			Assert("Should not contain tariffRule2 due to unmatched tariff number", !tariff.TariffRules.Contains(tariffRule2));
			Assert("Should contain tariffRule3", tariff.TariffRules.Contains(tariffRule3));
		}

		public void TestLoadForNonSTNRule()
		{
			var tariffRule1 = CreateTariffRule(TariffRuleList.Codes.EligibleForSecondaryTariffNumbers, new ZDateTime(2007, 1, 1), ZDateTime.Empty, "0101101000", ZString.Empty);
			var tariffRule2 = CreateTariffRule(TariffRuleList.Codes.AssembledAbroadOfUSProducts, new ZDateTime(2007, 1, 1), ZDateTime.Empty, "011", ZString.Empty);
			var tariffRule3 = CreateTariffRule(TariffRuleList.Codes.ChileanPreferenceLevel, new ZDateTime(2007, 1, 1), ZDateTime.Empty, "01", ZString.Empty);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0101101000";
			tariff.UE_DateFrom = new ZDateTime(2007, 1, 1);
			tariff.UE_DateTo = new ZDateTime(2007, 7, 1);

			Assert("Should contain tariffRule1", tariff.TariffRules.Contains(tariffRule1));
			Assert("Should not contain tariffRule2 due to unmatched tariff number", !tariff.TariffRules.Contains(tariffRule2));
			Assert("Should contain tariffRule3", tariff.TariffRules.Contains(tariffRule3));
		}

		public void TestLoadForRecordsWithTariffToDefined()
		{
			var tariffRule1 = CreateTariffRule(TariffRuleList.Codes.AssembledAbroadOfUSProducts, new ZDateTime(2007, 1, 1), ZDateTime.Empty, "0110", "0112");
			var tariffRule2 = CreateTariffRule(TariffRuleList.Codes.CottonFeeExemption, new ZDateTime(2007, 1, 1), ZDateTime.Empty, "02", ZString.Empty);
			var tariffRule3 = CreateTariffRule(TariffRuleList.Codes.ChileanPreferenceLevel, new ZDateTime(2007, 1, 1), ZDateTime.Empty, "0111", ZString.Empty);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0111101000";
			tariff.UE_DateFrom = new ZDateTime(2007, 1, 1);
			tariff.UE_DateTo = new ZDateTime(2007, 7, 1);

			Assert("Should contain tariffRule1", tariff.TariffRules.Contains(tariffRule1));
			Assert("Should not contain tariffRule2 due to unmatched tariff number", !tariff.TariffRules.Contains(tariffRule2));
			Assert("Should contain tariffRule3", tariff.TariffRules.Contains(tariffRule3));

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0112101000";
			tariff.UE_DateFrom = new ZDateTime(2007, 1, 1);
			tariff.UE_DateTo = new ZDateTime(2007, 7, 1);

			Assert("Should contain tariffRule1", tariff.TariffRules.Contains(tariffRule1));
			Assert("Should not contain tariffRule2 due to unmatched tariff number", !tariff.TariffRules.Contains(tariffRule2));
			Assert("Should not contain tariffRule3 due to unmatched tariff number", !tariff.TariffRules.Contains(tariffRule3));
		}

		public void TestApplies()
		{
			var tariffRule1 = CreateTariffRule(TariffRuleList.Codes.AssembledAbroadOfUSProducts, new ZDateTime(2007, 1, 1), ZDateTime.Empty, "0110", "0112");
			var tariffRule2 = CreateTariffRule(TariffRuleList.Codes.CottonFeeExemption, new ZDateTime(2007, 1, 1), ZDateTime.Empty, "02", ZString.Empty);
			var tariffRule3 = CreateTariffRule(TariffRuleList.Codes.ChileanPreferenceLevel, new ZDateTime(2007, 1, 1), new ZDateTime(2007, 6, 30), "011", ZString.Empty);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0111101000";
			tariff.UE_DateFrom = new ZDateTime(2007, 1, 1);
			tariff.UE_DateTo = new ZDateTime(2007, 7, 1);

			Assert(tariff.Applies(TariffRuleList.Codes.AssembledAbroadOfUSProducts, new ZDateTime(2007, 7, 1)));
			Assert(!tariff.Applies(TariffRuleList.Codes.CottonFeeExemption, new ZDateTime(2007, 7, 1)));
			Assert(!tariff.Applies(TariffRuleList.Codes.ChileanPreferenceLevel, new ZDateTime(2007, 7, 1)));
		}

		USCTariffRule CreateTariffRule(ZString ruleCode, ZDateTime dateFrom, ZDateTime dateTo, ZString tariffFrom, ZString tariffTo)
		{
			var result = Factory.New<USCTariffRule>();

			result.U1_RuleCode = ruleCode;
			result.U1_Tariff = tariffFrom;
			result.U1_TariffTo = tariffTo;
			result.U1_DateFrom = dateFrom;
			result.U1_DateTo = dateTo;

			return result;
		}

		protected override USCTariffRuleAdhocCollection GetCollectionToTest()
		{
			return new USCTariffRuleAdhocCollection(Factory);
		}
	}
}
