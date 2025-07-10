using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsRFRegistry
	{
		ZGuid WRR_WW_Whs { get; set; }
		ZGuid WRR_RQ_LastUsedEquipment { get; set; }
		ZString WRR_GS_NKAssignedTo { get; set; }
		ZShort WRR_PickGroupSequence { get; set; }
		ZString WRR_PickMethodCode { get; set; }
	}
}
