using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickAvailableInventoryLookups : ZLookups
	{
		#region Constructors

		public WhsPickAvailableInventoryLookups(WhsPickAvailableInventory parent)
			: base(parent)
		{
		}

		#endregion

		#region AssignedTos

		public virtual GlbStaffCollection AssignedTos
		{
			get { return new GlbStaffCollection(Factory); }
		}

		#endregion
	}
}
