using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ILineWithInventory : ILineAttributes
	{
		// Related Entities
		ZGuid ProductPK { get; }
		WhsDocket ParentDocket { get; }
		WhsProduct Product { get; }

		// Collections
		WhsInventoryViewCollection Inventory { get; }

		// Inventory Data
		ZDateTimeOffset ArrivalDate { get; }
		ZString PackType { get; }
		ZString TransactionInventoryStatus { get; set; }
		ZString TransactionInventoryHeldCode { get; }
		ZGuid TransactionLocation { get; }
		ZString TransactionPalletID { get; }

		// Flags
		bool IsFinalising { get; }
		bool CanCreateInventory { get; }

		// Reserved Quantity Details
		ZDecimal ReservedQuantity { get; }
		ReservedPickLineCollection ReservedPickLines { get; }
	}
}
