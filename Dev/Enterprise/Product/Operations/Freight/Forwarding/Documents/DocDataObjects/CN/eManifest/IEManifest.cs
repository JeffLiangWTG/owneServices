using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	interface IEManifest : IDataSourceProvider, IAdditionalReferenceProvider
	{
		ZString CarrierBookingReference { get; }
		ZInt NumberOfOriginals { get; }
		ZInt NumberOfCopies { get; }
		ZDateTime RequestedDateOfIssue { get; }
		ZBool IsDoorPickup { get; }
		ZBool IsDoorDelivery { get; }
		ZString SpecialInstructions { get; }

		ZString BillOfLadingNumber { get; }
		ZString MasterBookingNumber { get; }

		ZBool SendAllBookings { get; }
		ZBool IsDirect { get; }

		ZBool IsFreightPrepaid { get; }
		ZBool IsFreightCollect { get; }
		ZBool IsChargesFreighted { get; }

		ZBool UseMasterBillAsMasterSO { get; }
		ZBool UseBkgRefAsMasterSO { get; }

		ICodeDescription ShipmentType { get; }
		ICodeDescription ReleaseType { get; }
		ICodeDescription ContainerMode { get; }

		IUnloco PortOfLoad { get; }
		IUnloco PortOfDischarge { get; }
		IUnloco PlaceOfReceipt { get; }
		IUnloco PlaceOfIssue { get; }
		IUnloco PlaceOfDelivery { get; }
		IUnloco FreightPayableAt { get; }
		IUnloco OperationalPort { get; }

		ITransports Transports { get; }

		IOtherCharges OtherCharges { get; }

		IAddress SendingAgent { get; }
		IAddress Carrier { get; }
		IAddress ReceivingAgent { get; }
		IAddress CarrierHandlingAgent { get; }
		IAddress CarrierBookingAgent { get; }
		IAddress NotifyParty { get; }
		IAddress NotifyParty2 { get; }
		IAddress Forwarder { get; }
		IAddress PickupFrom { get; }
		IAddress DeliverTo { get; }
		IAddress CurrentUser { get; }

		IReadOnlyCollection<IContainer> Containers { get; }
		IReadOnlyCollection<Booking> Bookings { get; }

		TaxInfo SendingAgentTaxInfo { get; }
		TaxInfo ReceivingAgentTaxInfo { get; }
		TaxInfo NotifyPartyTaxInfo { get; }
		TaxInfo NotifyParty2TaxInfo { get; }
	}
}
