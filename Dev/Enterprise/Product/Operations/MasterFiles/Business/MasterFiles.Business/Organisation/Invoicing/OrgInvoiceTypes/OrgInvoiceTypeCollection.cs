using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgInvoiceTypeCollection : DependentBusinessObjectCollection<OrgInvoiceType, OrgCompanyData>
	{
		public OrgInvoiceTypeCollection(OrgCompanyData parent) : base(parent)
		{
		}

		public OrgInvoiceTypeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
