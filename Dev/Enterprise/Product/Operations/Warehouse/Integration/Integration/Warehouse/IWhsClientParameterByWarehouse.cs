using CargoWise.Types;
namespace Enterprise.Warehouse.Integration
{
	public interface IWhsClientParameterByWarehouse
	{
		ZGuid PK { get; }
		ZGuid WY_OH_Client { get; set; }
		ZGuid WY_WW_Whs { get; set; }
	}
}
