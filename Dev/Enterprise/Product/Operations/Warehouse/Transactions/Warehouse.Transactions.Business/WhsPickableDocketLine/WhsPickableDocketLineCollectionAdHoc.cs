using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickableDocketLineCollectionAdHoc : WhsDocketLineCollectionAdHoc<WhsPickableDocketLine>
	{
		public WhsPickableDocketLineCollectionAdHoc(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
