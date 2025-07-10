using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class ConditionApplicabilitiesByCriteria
	{
		public ConditionApplicabilitiesByCriteria()
		{
		}

		public ConditionApplicabilitiesByCriteria(ZDateTime effectiveDate, ZString tradeGroupCountry, ZString zX2_ConditionClass, ZString zX2_ConditionType, ZString zZT_OrderNumber, ZString zZT_AdditionalCode, ZString zZA_TradeGroup, ZString zZA_Description, ZString zZS_Preference, ZString zZS_Description, string secondTradeGroup = "")
		{
			EffectiveDate = effectiveDate;
			TradeGroupCountry = tradeGroupCountry;
			ZX2_ConditionClass = zX2_ConditionClass;
			ZX2_ConditionType = zX2_ConditionType;
			ZZT_OrderNumber = zZT_OrderNumber;
			ZZT_AdditionalCode = zZT_AdditionalCode;
			ZZA_TradeGroup = zZA_TradeGroup;
			ZZA_Description = zZA_Description;
			ZZS_Preference = zZS_Preference;
			ZZS_Description = zZS_Description;
			SecondTradeGroup = secondTradeGroup;
		}

		public ZDateTime EffectiveDate { get; set; }
		public ZString TradeGroupCountry { get; set; }
		public ZString ZX2_ConditionClass { get; set; }
		public ZString ZX2_ConditionType { get; set; }
		public ZString ZZT_OrderNumber { get; set; }
		public ZString ZZT_AdditionalCode { get; set; }
		public ZString ZZA_TradeGroup { get; set; }
		public ZString ZZA_Description { get; set; }
		public ZString ZZS_Preference { get; set; }
		public ZString ZZS_Description { get; set; }
		public ZString SecondTradeGroup { get; set; }
	}
}
