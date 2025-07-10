namespace Enterprise.Customs.SG.V4.Business
{
	partial class IncoTermAndCustomsChargeFactory
	{
		public static Common.CustomsChargeCode OverseasFreight => new Common.CustomsChargeCode(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsPercentageApplicable = true
		};
	}
}
