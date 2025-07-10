using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Integration;

namespace Enterprise.MarketingManager.Business
{
	public class OrgSalesProductCollection : ActiveBusinessObjectCollection<OrgSalesProduct>, IOrgSalesProductCollection
	{
		public OrgSalesProductCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		IOrgSalesProduct IOrgSalesProductCollection.this[int i]
		{
			get { return this[i]; }
		}
	}
}
