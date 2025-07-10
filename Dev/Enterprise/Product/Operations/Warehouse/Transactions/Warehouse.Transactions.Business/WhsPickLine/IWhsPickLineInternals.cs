using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWhsPickLineInternals : ISyncWithDB
	{
		/// <summary>
		/// DO NOT USE THIS UNLESS YOU KNOW WHAT YOU ARE DOING! This method will allow
		/// you to set WZ_PickedDateTime to empty on a WhsPickLine when the Picked Time is set. This
		/// is not normally valid to do.
		/// </summary>
		IDisposable TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse();

		/// <summary>
		/// DO NOT USE THIS UNLESS YOU KNOW WHAT YOU ARE DOING! This method will allow
		/// Picking a PickLine without reducing the Stock attached to it. This is
		/// only used by Putaway Transfers as they do not reduce Total Units for now.
		/// </summary>
		void PickWithoutReducingStock_DoNotUse(ZDateTimeOffset pickTime);
	}
}
