using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Integration.QuotedBooking
{
	public interface IQuotedBooking : ICCACommonAssignmentValidationData
	{
		ZGuid ClientPK { get; set; }
		ZGuid ViewPK { get; }
		new ZString UniqueConsignRef { get; set; }
		ZString Mode { get; set; }
		new ZString TransportMode { get; set; }
		ZGuid CartagePK { get; }
		ZGuid SailingJX { get; set; }
		[DocumentFieldExcludeFromMap]
		IDependentBusinessObjectCollection QuotedBookingContainers { get; }
		ZString PackingMode { get; set; }
		BusinessObject ForwardingShipment { get; }
		BusinessObject JobSailing { get; }
		BusinessObject Quote { get; }
		IJobHeader Job { get; }
		IOrgHeader ControllingCustomer { get; }
		IOrgHeader Client { get; }
		IOrgHeader Consignee { get; }
		IOrgHeader Consignor { get; }
		ZString Via { get; }
		ZString Origin { get; }
		ZString Destination { get; }
	}
}
