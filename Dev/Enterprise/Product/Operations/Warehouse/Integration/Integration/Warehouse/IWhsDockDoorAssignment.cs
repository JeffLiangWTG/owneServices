using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsDockDoorAssignment
	{
		ZGuid PK { get; }
		ZGuid WDA_WL_AssignedDockDoor { get; set; }
		ZDateTime WDA_SystemLastEditTimeUtc { get; set; }
	}
}
