using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public interface IItemHeader
	{
		WhsWarehouse Warehouse { get; }
		JobDocAddress TransportCompany { get; }
		ZString TransportReference { get; }
		ZString MasterBillNumber { get; }
		ZString ContainerNumber { get; }
		ZString CarrierBookingReference { get; }
		ZDateTimeOffset GateInTime { get; }
		ZString ContainerISOType { get; }
	}
}
