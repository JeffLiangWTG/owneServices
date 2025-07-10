using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsPickAvailableInventorySplitBaseLookups : ZLookups
	{
		protected WhsPickAvailableInventorySplitBaseLookups(WhsPickAvailableInventorySplitBase parent)
			: base(parent)
		{
		}

		public GlbStaffCollection AssignedTos => new GlbStaffCollection(Factory);
	}
}
