using CargoWise.Types;

namespace Enterprise.Warehouse.GateManagement.Integration
{
	public interface IGteBooking
	{
		ZGuid PK { get; }
		ZString GBK_BookingType { get; set; }
		ZGuid GBK_WW_Facility { get; set; }
		ZGuid GBK_OH_TransportCompany { get; set; }
	}
}
