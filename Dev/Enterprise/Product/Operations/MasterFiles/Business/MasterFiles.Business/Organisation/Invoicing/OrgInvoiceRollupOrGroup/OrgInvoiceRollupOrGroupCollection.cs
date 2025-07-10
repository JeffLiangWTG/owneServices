using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgInvoiceRollupOrGroupCollection : DependentBusinessObjectCollection<OrgInvoiceRollupOrGroup, OrgCompanyData>
	{
		public OrgInvoiceRollupOrGroupCollection(OrgCompanyData companyData) : base(companyData)
		{
		}

		public new OrgCompanyData Master
		{
			get { return base.Master; }
		}
	}
}
