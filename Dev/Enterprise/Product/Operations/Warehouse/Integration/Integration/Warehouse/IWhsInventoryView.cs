using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsInventoryView
	{
		ZGuid PK { get; }
		object this[string propertyName] { get; set; }

		ZDateTimeOffset WI_ArrivalDate { get; set; }
		ZDecimal WI_AvailableToPickQuantity { get; }
		ZString WI_BondedEntryKey { get; set; }
		ZString WI_AllocationKey { get; set; }
		ZGuid WI_OH_Client { get; set; }
		ZGuid WI_OP { get; set; }
		ZDecimal WI_TotalUnits { get; set; }
		ZGuid WI_WD { get; set; }
		ZGuid WI_WE_InDocketLine { get; set; }
		ZString WI_InDocketLineType { get; set; }
		ZGuid WI_WE_OriginalInDocketLineForRating { get; set; }
		ZGuid WI_WL { get; set; }
		ZString WI_InventoryStatus { get; set; }
		ZString WI_HeldCode { get; }
		ZString WI_PartAttrib1 { get; set; }
		ZString WI_PartAttrib2 { get; set; }
		ZString WI_PartAttrib3 { get; set; }
		ZString WI_SerialNumber { get; set; }
		ZString WI_F3_NKPackType { get; set; }
		ZDecimal WI_InDocketLineUnits { get; set; }
	}
}
