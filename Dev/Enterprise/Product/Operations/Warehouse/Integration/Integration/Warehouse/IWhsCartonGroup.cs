using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsCartonGroup
	{
		ZGuid PK { get; }
		ZString WCG_Code { get; set; }
		ZString WCG_Description { get; set; }
	}
}
