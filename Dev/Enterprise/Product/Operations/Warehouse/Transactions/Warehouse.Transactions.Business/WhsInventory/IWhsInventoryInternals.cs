using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWhsInventoryInternals
	{
		/// <summary>
		/// *** DO NOT USE UNLESS YOU KNOW WHAT YOU ARE DOING ***
		///
		/// This will return the amount committed to this inventory but without considering whether this
		/// inventory is finalised or not. This property primarily exists for the GUI. Generally speaking,
		/// 'CommittedQuantityIncludingUnfinalisedReceipt' should be used instead.
		/// </summary>
		ZDecimal CommittedToTransactionQuantity { get; }
		ZPropertyInfo CommittedToTransactionQuantityInfo { get; }
	}
}
