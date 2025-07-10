using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IManifestMessageAttachee : IMessageAttachee
	{
		ZString ApplicationCode { get; }
		ZString SupApplicationCode { get; }
		ZString JobNumber { get; }
		ZString CarrierCode { get; }
		ZString ModeOfTransportationCode { get; }
		ZString ConveyanceCountryCode { get; }
		ZString ConveyanceName { get; }
		ZString VoyageNumber { get; }
		ZString ManifestSequenceNumber { get; set; }
		ZBool IsPaperlessMIBParticipant { get; }
		ZString ConveyanceCode { get; }
		ZBool IsOutboundCargo { get; }
		ZString UniqueVoyageIdentifier { get; }
		ZString InBondNumber { get; }

		void UpdatConveyanceEventInformation(ZString eventCode, ZDateTime eventDate);
		void UpdateDispositionInformation(ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString dispositionCode, ZDateTime dispositionDate);
		void UpdateIncomingBillStatus(ZString issuerCode, ZString billOfLading, ZString subtype, bool isFailure, CBPEDIMessage responseMessage);
		void UpdateEstimatedDateOfArrival(ZDateTime estimatedDateOfArrival);
		void LinkMessageToBill(ZString issuerCode, ZString billOfLading, CBPEDIMessage responseMessage);
	}
}
