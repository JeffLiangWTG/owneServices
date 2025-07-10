using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPickByLabelJob
	{
		ZGuid PK { get; }

		ZDateTimeOffset WTK_FinalisedDate { get; set; }

		ZString WTK_GS_NKAssignedTo { get; set; }

		ZGuid WTK_WL_DockDoor { get; set; }

		ZGuid WTK_WW_Warehouse { get; set; }

		ZGuid WTK_P9_Task { get; set; }
	}
}
