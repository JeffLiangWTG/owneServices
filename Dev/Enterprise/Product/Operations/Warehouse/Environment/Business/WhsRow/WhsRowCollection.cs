using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsRowCollection : ActiveBusinessObjectCollection<WhsRow>, IWhsRowCollection
	{
		public WhsRowCollection(WhsWarehouse master, BusinessObjectFactory factory)
			: base(factory, master, null, WhsRowSchema.WR_WW_Whs)
		{
			if (master != null)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(WhsLocationCollection.FilterSchema.Warehouse, "Property", master.PK, false));
			}
		}

		public WhsRowCollection(BusinessObjectFactory factory)
				: base(factory)
		{
		}
	}
}
