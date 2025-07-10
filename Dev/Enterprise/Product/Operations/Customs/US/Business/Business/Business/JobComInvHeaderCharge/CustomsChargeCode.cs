using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business
{
	partial class DeliveryTermAndChargeFactory
	{
		public static CustomsChargeCode OverseasFreight => new CustomsChargeCode(USCustomsChargeTypeList.Codes.OverseasFreight, USCustomsChargeTypeList.Descriptions.OverseasFreight)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true
		};

		public static CustomsChargeCode OverseasInsurance => new CustomsChargeCode(USCustomsChargeTypeList.Codes.OverseasInsurance, USCustomsChargeTypeList.Descriptions.OverseasInsurance)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsPercentageApplicable = true
		};

		public static CustomsChargeCode ForeignInlandFreight => new CustomsChargeCode(USCustomsChargeTypeList.Codes.ForeignInlandFreight, USCustomsChargeTypeList.Descriptions.ForeignInlandFreight)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true
		};

		public static CustomsChargeCode LandingCharges => new CustomsChargeCode(USCustomsChargeTypeList.Codes.LandingCharges, USCustomsChargeTypeList.Descriptions.LandingCharges)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true
		};

		public static CustomsChargeCode DisbursementCharge => new CustomsChargeCode(USCustomsChargeTypeList.Codes.DisbursementCharge, USCustomsChargeTypeList.Descriptions.DisbursementCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = false
		};

		public static CustomsChargeCode AdditionCharge => new CustomsChargeCode(USCustomsChargeTypeList.Codes.AdditionCharge, USCustomsChargeTypeList.Descriptions.AdditionCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true
		};

		public static CustomsChargeCode DeductionCharge => new CustomsChargeCode(USCustomsChargeTypeList.Codes.DeductionCharge, USCustomsChargeTypeList.Descriptions.DeductionCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true
		};
	}
}
