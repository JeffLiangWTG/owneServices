using CargoWise.Types;
using Enterprise.Integration;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Integration
{
	public interface IConsignment
	{
		IWhsWarehouse Warehouse { get; }
		IJobDocAddress BookingPartyDocAddress { get; }
		ZString MasterBillNumber { get; }
		ZString HouseBillNumber { get; }
		ZString JobID { get; }
		ZString TransportMode { get; }
		ZString ShipmentNumber { get; }
		ZString Direction { get; }
		ICusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers { get; }
	}
}
