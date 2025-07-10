namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AutoOrgInvoiceRollupOrGroupCompanyDataNullRef : AutoOrgInvoiceRollupOrGroup
	{
		public AutoOrgInvoiceRollupOrGroupCompanyDataNullRef(CargoWise.EntityFramework.BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		public override OrgCompanyData CompanyData
		{
			get { return null; }
		}
	}
}
