using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsInventoryHeldCode
	{
		ZGuid PK { get; }
		ZString WHC_Code { get; set; }
		ZString WHC_Description { get; set; }
	}
}
