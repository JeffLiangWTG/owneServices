using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickShortLine : AutoWhsPickShortLine
	{
		public WhsPickShortLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		#region InventoryLine

		/// <summary>
		/// This is the InventoryLine that has been shorted.
		/// </summary>
		public WhsDocketLine InventoryLine
			=> Factory.Load<WhsDocketLine>(WZS_WE_InventoryLine);

		#endregion

		#region TransactionLine

		/// <summary>
		/// This is the Work Order, Order or Transfer Line that the Committed Inventory is allocated to.
		/// </summary>
		public WhsDocketLine TransactionLine
			=> Factory.Load<WhsDocketLine>(WZS_WE_TransactionLine);

		#endregion

		#endregion

		#region BizO Overrides

		public override bool CanDelete => false;

		public override void Delete()
			=> throw new NotSupportedException("You cannot delete this.");

		#endregion
	}
}
