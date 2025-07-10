using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTradeDetailCollection : DependentBusinessObjectCollection<OrgTradeDetail, OrgSales>
	{
		public OrgTradeDetailCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgTradeDetailCollection(OrgSales salesOrg)
			: base(salesOrg)
		{
		}
	}
}
