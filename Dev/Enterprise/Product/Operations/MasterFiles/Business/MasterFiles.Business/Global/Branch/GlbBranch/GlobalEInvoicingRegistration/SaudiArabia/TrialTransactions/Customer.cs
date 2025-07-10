namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia
{
	public class Customer : TransactionParty
	{
		public Customer(OrgHeader org) : base(org.OH_FullName, org.AddressForSendingARDocuments, org.CustomsCodes)
		{
		}

		protected override OrgCusCode Identification =>
			GetCustomsCodeOrNull(SaudiArabiaOrgCusCodeInfo.OrgCusCodes.TIN) ??
			GetCustomsCodeOrNull(OrgCusCode.CodeTypes.CompanyRegistrationNumber) ??
			GetCustomsCodeOrNull(OrgCusCode.CodeTypes.CorporationCode) ??
			GetCustomsCodeOrNull(SaudiArabiaOrgCusCodeInfo.OrgCusCodes.NAT) ??
			GetCustomsCodeOrNull(OrgCusCode.CodeTypes.PassportID);
	}
}
