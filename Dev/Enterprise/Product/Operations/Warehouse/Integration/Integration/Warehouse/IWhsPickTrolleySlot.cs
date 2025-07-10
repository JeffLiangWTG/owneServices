using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPickTrolleySlot
	{
		ZGuid PK { get; }

		ZGuid WTS_WTJ_TrolleyJob { get; set; }

		ZGuid WTS_KP_Package { get; set; }

		ZShort WTS_SlotNumber { get; set; }
	}
}
