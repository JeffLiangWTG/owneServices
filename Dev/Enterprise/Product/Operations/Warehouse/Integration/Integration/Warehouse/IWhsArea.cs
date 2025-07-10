using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsArea
	{
		ZGuid PK { get; }
		object this[string propertyName] { get; set; }

		ZGuid WA_WW_Whs { get; set; }
		ZString WA_Name { get; set; }
		ZString WA_AreaType { get; set; }
		ZBool WA_IsDefaultPickArea { get; set; }
		ZBool WA_IsDefaultPutawayArea { get; set; }
		ZBool WA_IsPickingArea { get; set; }
		ZBool WA_IsPutawayArea { get; set; }
	}
}
