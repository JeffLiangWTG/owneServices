namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgInvoiceRollupOrGroupValidationForTest : OrgInvoiceRollupOrGroupValidation
	{
		public OrgInvoiceRollupOrGroupValidationForTest(AutoOrgInvoiceRollupOrGroup parent)
			: base(parent)
		{
		}
		public void CheckPG_AllInOne()
		{
			base.CheckPG_GroupOrSubTotal();
			base.CheckPG_GroupOrSubtotalStyle();
			base.CheckPG_InvoiceLineDisplayOption();
			base.CheckPG_InvoicePostingStyle();
			base.CheckPG_JobType();
			base.CheckPG_ServiceDirection();
			base.CheckPG_TransportMode();
			base.CheckPG_RX_NKInvoicePostingCurrency();
		}
	}
}
