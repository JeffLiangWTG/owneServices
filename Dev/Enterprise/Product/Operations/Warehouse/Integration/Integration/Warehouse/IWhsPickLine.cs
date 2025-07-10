using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPickLine
	{
		ZGuid PK { get; }
		object this[string propertyName] { get; set; }

		ZDateTimeOffset WZ_PickedDateTime { get; set; }
		ZString WZ_GS_NKAssignedTo { get; set; }
		ZDecimal WZ_Units { get; set; }
		ZString WZ_VerifiedEmpty { get; set; }
		ZGuid WZ_WE_TransactionLine { get; set; }
		ZGuid WZ_WE_InventoryLine { get; set; }
		ZGuid WZ_WE_OriginalPickedInventoryLine { get; set; }

		IWhsDocketLine InventoryLine { get; }
	}
}
