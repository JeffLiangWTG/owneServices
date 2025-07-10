using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsSalesChannel
	{
		ZGuid PK { get; }
		ZString WSH_Code { get; set; }
		ZString WSH_Description { get; set; }
	}
}
