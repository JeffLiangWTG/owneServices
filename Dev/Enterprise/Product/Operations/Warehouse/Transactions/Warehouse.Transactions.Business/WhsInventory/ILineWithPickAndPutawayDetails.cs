using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ILineWithPickAndPutawayDetails
	{
		ZString PickedBy { get; }

		ZString PutawayBy { get; set; }
	}
}
