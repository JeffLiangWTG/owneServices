using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Integration.BondedWarehouse
{
	public interface IBondedWarehouseTransactionLineProvider
	{
		IWhsBondedWarehouseTransactionLine TransactionLine { get; }
	}

	public interface IWhsBondedWarehouseTransactionLine : IWhsWarehouseTransactionLine
	{
		ZDateTime EntryDate { get; }
		ZDecimal CustomsQuantity { get; }
		ZString CustomsQuantityUnit { get; }

		ZDecimal CustomsSecondQuantity { get; }
		ZString CustomsSecondQuantityUnit { get; }

		ZDecimal CustomsThirdQuantity { get; }
		ZString CustomsThirdQuantityUnit { get; }

		ZDecimal BondedWarehouseQuantity { get; }
		ZString BondedWarehouseQuantityUnit { get; }
		IRefCountry CountryOfOrigin { get; }
		ZDecimal ValueForDuty { get; }
		IMoney TILV { get; }
		ZString AddInfo { get; }
		ZGuid UniqueKey { get; }
		NotificationCollection BondedWarehouseQuantityProblems { get; }

		// Obsolete
		ZString OriginalEntryKey { get; }
		ZShort OriginalEntryLineNumber { get; }
	}
}
