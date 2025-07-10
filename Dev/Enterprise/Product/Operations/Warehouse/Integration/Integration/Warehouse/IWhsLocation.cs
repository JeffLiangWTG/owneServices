using CargoWise.Types;
namespace Enterprise.Warehouse.Integration
{
	public interface IWhsLocation
	{
		ZGuid PK { get; }

		ZString WLV_LocationClass { get; }
		ZString WLV_RowName { get; }
		ZShort WLV_Column { get; }
		ZShort WLV_Level { get; }
		ZShort WLV_Tray { get; }
		ZString WLV_LocationStatus { get; }
		ZString WLV_LocationString { get; }
		ZDecimal WLV_MaxWeight { get; set; }
		ZString WLV_MaxWeightUnit { get; set; }
		ZInt WLV_PickPathSequence { get; }
		ZInt WLV_PutawayPathSequence { get; }
		ZString WLV_PickingAreaType { get; }

		ZGuid WLV_WW_Whs { get; set; }
		ZGuid WLV_WA_PutawayArea { get; set; }
		ZGuid WLV_WA_PickingArea { get; set; }
		ZGuid WLV_WLT_LocationType { get; set; }

		ZShort RowPathSequence { get; }
		ZString PickingAreaName { get; }
		ZString WLV_LocationTypeCode { get; }

		IWhsWarehouse Warehouse { get; }

		object this[string propertyName] { get; set; }
	}
}
