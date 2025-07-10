using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IINPM01
	{
		ZString CarrierCode { get; }
		ZString ManifestSequenceNumber { get; }
		ZString VesselName { get; }
		ZString VoyageNumber { get; }
		ZString ModeOfTransportation { get; }
		ZString VesselCountry { get; }
	}
}
