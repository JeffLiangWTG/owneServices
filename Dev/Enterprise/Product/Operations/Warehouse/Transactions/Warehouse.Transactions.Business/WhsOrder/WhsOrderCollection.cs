using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ModuleID(ModuleId.WhsOrder)]
	public class WhsOrderCollection : WhsPickableDocketCollection
	{
		public WhsOrderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsOrderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public WhsOrderCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public new WhsOrder this[int index] => (WhsOrder)(base[index]);

		public new WhsOrder AddNew() => (WhsOrder)base.AddNew();

		protected override bool AllowNewDocket => false;

		protected override IEnumerable<string> DocketTypes => new[] { DocketType.Codes.Order };

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			return property.Name == nameof(WhsDocket.WD_WL_CrossDock)
				? new LocationComparer<WhsOrder>(property, direction, order => Factory.Load<WhsLocation>(order.WD_WL_CrossDock))
				: base.GetSortComparerForProperty(property, direction);
		}
	}
}
