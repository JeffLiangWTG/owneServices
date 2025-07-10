using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IPickLinePair
	{
		WhsPickLine PickLineForPickingDetails { get; }
		WhsPickLine PickLineOnOrder { get; }
		GlbStaff AssignedTo { get; }
		ZString AssignedToCode { get; set; }
		ZDateTimeOffset PickedDateTime { get; set; }
	}
}
