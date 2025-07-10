using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPick
	{
		ZGuid PK { get; }
		ZGuid WP_WW_Whs { get; set; }
		ZGuid WP_WL_DockDoor { get; set; }
		ZGuid WP_WL_PackingStation { get; set; }
		ZGuid WP_WDA_DockDoorAssignment { get; set; }
		ZGuid DockDoorPK { get; set; }

		bool IsWorkOrderPick { get; }
		bool IsCustomsTransaction { get; }

		ZString WP_PickStatus { get; set; }
		ZDateTime WP_FinalizedDateUtc { get; set; }

		ZString WP_TaskPlanningStatus { get; set; }

		ZByte PickPriority { get; set; }
	}
}
