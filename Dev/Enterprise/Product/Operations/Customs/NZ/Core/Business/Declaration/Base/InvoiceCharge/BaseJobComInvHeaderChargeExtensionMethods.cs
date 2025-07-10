namespace Enterprise.Customs.NZ.Business.Declaration
{
	using Enterprise.Customs.Business;

	public static class BaseJobComInvHeaderChargeExtensionMethods
	{
		public static bool ShouldBeIncludedInFreight(this BaseJobComInvHeaderCharge charge)
		{
			return (charge.J7_ChargeType == CustomsChargeTypeList.Codes.ForeignInlandFreight || charge.J7_ChargeType == CustomsChargeTypeList.Codes.OtherCharges)
					&& !charge.J7_IsDutiable
					&& charge.J7_IsGSTApplicable;
		}
	}
}
