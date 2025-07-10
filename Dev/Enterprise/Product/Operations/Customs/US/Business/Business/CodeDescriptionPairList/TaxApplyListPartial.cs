namespace Enterprise.Customs.US.Business
{
	partial class TaxApplyList
	{
		public static bool IsTaxApplicable(string taxApplyCode)
		{
			return taxApplyCode == Codes.Yes ||
				taxApplyCode == Codes.Override;
		}
	}
}
