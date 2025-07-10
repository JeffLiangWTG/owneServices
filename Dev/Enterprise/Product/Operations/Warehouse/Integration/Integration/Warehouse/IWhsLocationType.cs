using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsLocationType
	{
		ZGuid PK { get; }
		ZString WLT_Code { get; set; }
		ZString WLT_Description { get; set; }
	}
}
