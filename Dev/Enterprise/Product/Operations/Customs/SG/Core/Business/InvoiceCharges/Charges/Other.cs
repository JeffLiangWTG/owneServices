namespace Enterprise.Customs.SG.V4.Business
{
	partial class IncoTermAndCustomsChargeFactory
	{
		public static Common.CustomsChargeCode Other => new Common.CustomsChargeCode(Common.CustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeTypeList.Descriptions.OtherCharges)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsPercentageApplicable = true,
			IsIncoTermNeutral = true
		};
	}
}
