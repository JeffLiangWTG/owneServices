using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierBuyerLinkCollection : BusinessObjectCollection<OrgSupplierBuyerLink>
	{
		public OrgSupplierBuyerLinkCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSupplierBuyerLinkCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
