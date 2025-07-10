using Enterprise.Customs.Common;

namespace Enterprise.Customs.ZA.Business
{
	partial class IncoTermAndCustomsChargeFactory
	{
		public static CustomsChargeCode IntellectualValue => new CustomsChargeCode(InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue, InvoiceLineCustomsChargeTypeList.Descriptions.IntellectualValue)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = false,
			IsPercentageApplicable = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true,
			ParentTypes = ChargeParentTypes.InvoiceLine
		};
	}
}
