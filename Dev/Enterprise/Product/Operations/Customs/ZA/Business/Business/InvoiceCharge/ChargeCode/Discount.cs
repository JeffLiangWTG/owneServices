using Enterprise.Customs.Common;

namespace Enterprise.Customs.ZA.Business
{
	partial class IncoTermAndCustomsChargeFactory
	{
		public static CustomsChargeCode Discount => new CustomsChargeCode(CustomsChargeTypeList.Codes.Discount, CustomsChargeTypeList.Descriptions.Discount)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = false,
			IsPercentageApplicable = true,
			IsIncoTermNeutral = true
		};
	}
}
