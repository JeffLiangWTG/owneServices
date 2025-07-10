using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickCollection : BusinessObjectCollection<WhsPick>, IWhsPickCollection
	{
		public WhsPickCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsPickCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			IComparer comparer;
			if (property.Name == nameof(WhsPick.WP_WL_DockDoor) || property.Name == nameof(WhsPick.DockDoorPK))
			{
				comparer = new LocationComparer<WhsPick>(property, direction, pick => pick.DockDoorLocation);
			}
			else
			{
				comparer = base.GetComparerForSort(property, direction);
			}

			return comparer;
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy() => new WhsPickCollectionFetchStrategy(this);
	}
}
