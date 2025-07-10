namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia
{
	public class Supplier : TransactionParty
	{
		public Supplier(GlbBranch branch) : base(branch.OrgProxy ?? branch.Company.OrgProxy)
		{
		}

		protected override OrgCusCode Identification =>
			GetCustomsCodeOrNull(OrgCusCode.CodeTypes.CompanyRegistrationNumber) ??
			GetCustomsCodeOrNull(OrgCusCode.CodeTypes.CorporationCode);
	}
}
