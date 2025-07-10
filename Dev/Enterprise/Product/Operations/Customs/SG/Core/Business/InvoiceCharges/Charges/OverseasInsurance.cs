namespace Enterprise.Customs.SG.V4.Business
{
	partial class IncoTermAndCustomsChargeFactory
	{
		public static Common.CustomsChargeCode OverseasInsurance => new Common.CustomsChargeCode(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsPercentageApplicable = true
		};
	}
}
