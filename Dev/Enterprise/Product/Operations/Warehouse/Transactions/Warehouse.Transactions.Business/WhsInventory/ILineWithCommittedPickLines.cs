using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ILineWithCommittedPickLines : ILineWithInventory
	{
		ZGuid PK { get; }

		// Related Entities
		ICommittedInventoryStrategy CommittedStrategy { get; }
		ZGuid ParentDocketPK { get; set; }
		WhsLocation LocationToCommit { get; }

		// Collections
		WhsPickLineCollection PickLines { get; }

		// Attributes to Match on
		ZShort LineNo { get; }
		ZString PackageGroupID { get; }
		ZString PalletIDToCommit { get; }

		// Descriptions
		string Noun { get; }
		string Verb { get; }

		// Quantities
		ZDecimal PerPackageQty { set; }
		ZDecimal TransactionQty { get; set; }

		bool IsInTransit { get; }
	}
}
