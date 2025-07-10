using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Integration.Business
{
	public interface IWhsPickAvailableInventory
	{
		ZGuid PK { get; }
		ZGuid LocationPK { get; }
		IOrgSupplierPart SupplierPart { get; }
		ZString BondedEntryKey { get; }
		ZDate PackingDate { get; }
		ZDate ExpiryDate { get; }
		ZDateTimeOffset ArrivalDate { get; }
		ZDateTime BondedEntryDate { get; }
		ZDecimal VFDPerStockUnit { get; }

		ZString PalletID { get; }
		ZString PartAttrib1 { get; }
		ZString PartAttrib2 { get; }
		ZString PartAttrib3 { get; }
		ZString SerialNumber { get; }
	}
}
