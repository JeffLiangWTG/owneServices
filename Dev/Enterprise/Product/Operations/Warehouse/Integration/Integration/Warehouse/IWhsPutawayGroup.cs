using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPutawayGroup
	{
		ZGuid PK { get; }
		ZString WPG_Code { get; set; }
		ZString WPG_Description { get; set; }
	}
}
