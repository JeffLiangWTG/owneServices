using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDynamicWorkOrderLineCollection : WhsComponentOrderLineCollection
	{
		public WhsDynamicWorkOrderLineCollection(WhsDynamicWorkOrder master)
			: base(master)
		{
		}

		public WhsDynamicWorkOrderLineCollection(WhsDynamicWorkOrder master, ZQuery filter)
			: base(master, filter)
		{
		}

		protected WhsDynamicWorkOrderLineCollection(WhsDynamicWorkOrderLine master, ZQuery filter, SchemaGuidColumn fk)
			: base(master, filter, fk)
		{
		}

		public new WhsDynamicWorkOrderLine this[int index] => (WhsDynamicWorkOrderLine)base[index];

		public new WhsDynamicWorkOrderLine AddNew() => (WhsDynamicWorkOrderLine)base.AddNew();
	}
}
