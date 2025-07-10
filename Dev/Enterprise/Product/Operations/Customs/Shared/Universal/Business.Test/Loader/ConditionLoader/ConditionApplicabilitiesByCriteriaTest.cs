using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	class ConditionApplicabilitiesByCriteriaTest : TestCase
	{
		public void TestConditionApplicabilitiesByCriteria()
		{
			var conditionApplicabilitiesByCriteria = new ConditionApplicabilitiesByCriteria(new ZDateTime(2020, 8, 12, 23, 40, 21), "TGC", "CC", "CT", "ON", "AC", "TG", "TG Description", "PP", "PP Description", "SecondTG");
			CombineAssertions(() =>
			{
				AssertEquals("EffectiveDate", new ZDateTime(2020, 8, 12, 23, 40, 21), conditionApplicabilitiesByCriteria.EffectiveDate);
				AssertEquals("TradeGroupCountry", "TGC", conditionApplicabilitiesByCriteria.TradeGroupCountry);
				AssertEquals("ZX2_ConditionClass", "CC", conditionApplicabilitiesByCriteria.ZX2_ConditionClass);
				AssertEquals("ZX2_ConditionType", "CT", conditionApplicabilitiesByCriteria.ZX2_ConditionType);
				AssertEquals("ZZT_OrderNumber", "ON", conditionApplicabilitiesByCriteria.ZZT_OrderNumber);
				AssertEquals("ZZT_AdditionalCode", "AC", conditionApplicabilitiesByCriteria.ZZT_AdditionalCode);
				AssertEquals("ZZA_TradeGroup", "TG", conditionApplicabilitiesByCriteria.ZZA_TradeGroup);
				AssertEquals("ZZA_Description", "TG Description", conditionApplicabilitiesByCriteria.ZZA_Description);
				AssertEquals("ZZS_Preference", "PP", conditionApplicabilitiesByCriteria.ZZS_Preference);
				AssertEquals("ZZS_Description", "PP Description", conditionApplicabilitiesByCriteria.ZZS_Description);
				AssertEquals("SecondTradeGroup", "SecondTG", conditionApplicabilitiesByCriteria.SecondTradeGroup);
			});
		}
	}
}
