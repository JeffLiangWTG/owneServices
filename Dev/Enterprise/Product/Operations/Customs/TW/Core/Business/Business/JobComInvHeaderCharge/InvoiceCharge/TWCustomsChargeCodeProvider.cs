using Enterprise.Customs.Common;

namespace Enterprise.Customs.TW.Business
{
	class TWCustomsChargeCodeProvider
	{
		public static CustomsChargeCode ExWorks => new CustomsChargeCode(CustomsChargeTypeList.Codes.ExWorks, CustomsChargeTypeList.Descriptions.ExWorks)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = true,
			IsPercentageApplicable = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true
		};
	}
}
