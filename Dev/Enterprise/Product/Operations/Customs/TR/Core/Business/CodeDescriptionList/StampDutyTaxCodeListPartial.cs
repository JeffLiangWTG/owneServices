using CargoWise.Types;

namespace Enterprise.Customs.TR.Business
{
	public partial class StampDutyTaxCodeList
	{
		public static bool IsFixValueOfCustomsFeesTotalByChargeType(ZString chargeType)
		{
			return StampDutyTaxCodeList.Codes.ETradeStampDuty == chargeType
			|| StampDutyTaxCodeList.Codes.ABS == chargeType
			|| StampDutyTaxCodeList.Codes.GMS == chargeType
			|| StampDutyTaxCodeList.Codes.OBS == chargeType
			|| StampDutyTaxCodeList.Codes.SBS == chargeType;
		}

		public static ZString GetChargeTypeCodeByDutyTaxCode(ZString dutyTaxCode)
		{
			var chargeType = string.Empty;
			switch (dutyTaxCode)
			{
				case StampDutyTaxCodeList.Codes.ETradeStampDuty:
					chargeType = StampDutyChargeTypeList.Codes.CustomsDec;
					break;
				case StampDutyTaxCodeList.Codes.ABS:
					chargeType = StampDutyChargeTypeList.Codes.BillOfLading;
					break;
				case StampDutyTaxCodeList.Codes.GMS:
					chargeType = StampDutyChargeTypeList.Codes.SummaryDec;
					break;
				case StampDutyTaxCodeList.Codes.OBS:
					chargeType = StampDutyChargeTypeList.Codes.Ordino;
					break;
				case StampDutyTaxCodeList.Codes.SBS:
					chargeType = StampDutyChargeTypeList.Codes.CarrierReceipt;
					break;
				case StampDutyTaxCodeList.Codes.ACC:
					chargeType = StampDutyChargeTypeList.Codes.Agreements;
					break;
				default:
					return dutyTaxCode;
			}

			return chargeType;
		}
	}
}
