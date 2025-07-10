using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsWarehouseTransactionLine
	{
		IOrgAddress Warehouse { get; }
		IOrgSupplierPart Product { get; }
		ZDecimal Quantity { get; }
		ZString QuantityUnit { get; }
		ZString EntryKey { get; }
		ZShort EntryLineNumber { get; }
		ZString PartAttrib1 { get; }
		ZString PartAttrib2 { get; }
		ZString PartAttrib3 { get; }
		ZString SerialNumber { get; }
		bool HasErrors { get; }
		bool HasWarnings { get; }

		// these properties should never return null
		NotificationCollection WarehouseProblems { get; }
		NotificationCollection QuantityProblems { get; }
		NotificationCollection EntryKeyProblems { get; }
		NotificationCollection PartAttrib1Problems { get; }
		NotificationCollection PartAttrib2Problems { get; }
		NotificationCollection PartAttrib3Problems { get; }
		NotificationCollection SerialNumberProblems { get; }
	}
}
