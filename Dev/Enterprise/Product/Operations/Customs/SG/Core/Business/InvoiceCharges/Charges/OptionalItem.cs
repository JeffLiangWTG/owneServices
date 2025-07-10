namespace Enterprise.Customs.SG.V4.Business
{
	partial class IncoTermAndCustomsChargeFactory
	{
		public static Common.CustomsChargeCode OptionalItem =>
			new Common.CustomsChargeCode(InvoiceLineCharge.ChargeTypes.OptionalItemCharges,
				ResString.GetMultilingualString("6AB3933E-775B-4A11-9216-6C6745ECB03E", "Optional Item Charges"))
			{
				IsDutiable = false,
				IsDutiableDeemedForThisCharge = true,
				IsVATible = true,
				IsVATibleDeemedForThisCharge = true,
				IsPercentageApplicable = false,
				IsIncoTermNeutral = true
			};
	}
}
