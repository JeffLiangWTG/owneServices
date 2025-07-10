using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	interface IShippingOrder : IDataSourceProvider
	{
		ZString CarrierBookingReference { get; }
		ZString ContractNamedAccount { get; }
		ZInt NumberOfOriginals { get; }
		ZInt NumberOfCopies { get; }
		ZDateTime RequestedDateOfIssue { get; }
		ZBool IsDoorPickup { get; }
		ZBool IsDoorDelivery { get; }

		ZString ForwardingInstructions { get; }
		ZString GoodsHandlingInstructions { get; }
		ZString SpecialInstructions { get; }

		ZString BillOfLadingNumber { get; }
		ZString ShipperReference { get; }
		ZString FreightForwarderReference { get; }
		ZString CarrierContractNumber { get; }
		ZBool CarrierContractNumberIsQuotationNumber { get; }
		ZString LetterOfCredit { get; }
		ZString USCanadaManifestSelfFilerID { get; }
		ZString CarrierBookingPrefix { get; }

		ZBool IsFreightPrepaid { get; }
		ZBool IsFreightCollect { get; }
		ZBool IssueFreightedBillOfLading { get; }
		ZBool IsDischargeInCanadaUSOrUSTerritory { get; }

		ZString VoyageFlightNumber { get; }

		RegistrationNumber NVOCCReference { get; }

		ICodeDescription ShipmentType { get; }
		ICodeDescription ReleaseType { get; }
		ICodeDescription ContainerMode { get; }

		IVessel Vessel { get; }

		Unloco PortOfLoading { get; }
		Unloco PortOfDischarge { get; }
		IUnloco CarrierBookingOffice { get; }
		Unloco PlaceOfReceipt { get; }
		Unloco PlaceOfIssue { get; }
		Unloco PlaceOfDelivery { get; }
		IUnloco FreightPayableAt { get; }
		IUnloco OperationalPort { get; }

		ITransports Transports { get; }

		IOtherCharges OtherCharges { get; }

		IOptionalCharge OptionalChargeDestinationHaulage { get; }
		IOptionalCharge OptionalChargeDestinationPort { get; }
		IOptionalCharge OptionalChargeOriginHaulage { get; }
		IOptionalCharge OptionalChargeOriginPort { get; }

		IAddress Shipper { get; }
		IAddress Carrier { get; }
		IAddress Consignee { get; }
		IAddress CarrierHandlingAgent { get; }
		IAddress CarrierBookingAgent { get; }
		IAddress NotifyParty { get; }
		IAddress NotifyParty2 { get; }
		IAddress Forwarder { get; }
		IAddress PickupFrom { get; }
		IAddress DeliverTo { get; }
		IAddress CurrentUser { get; }

		IReadOnlyCollection<Container> Containers { get; }
	}
}
