using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPickByLabelLabel
	{
		ZGuid PK { get; }

		ZGuid WTL_KP_Package { get; set; }

		ZGuid WTL_WTK_PickByLabelJob { get; set; }
	}
}
