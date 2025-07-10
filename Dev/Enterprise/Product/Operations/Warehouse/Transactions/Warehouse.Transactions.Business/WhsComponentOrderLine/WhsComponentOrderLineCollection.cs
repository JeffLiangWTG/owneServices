using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsComponentOrderLineCollection : WhsPickableDocketLineCollection
	{
		protected WhsComponentOrderLineCollection(WhsComponentOrder master)
			: base(master)
		{
		}

		protected WhsComponentOrderLineCollection(WhsComponentOrder master, ZQuery filter)
			: base(master, filter)
		{
		}

		protected WhsComponentOrderLineCollection(WhsComponentOrderLine master, ZQuery filter, SchemaGuidColumn fk)
			: base(master, filter, fk)
		{
		}
	}
}
