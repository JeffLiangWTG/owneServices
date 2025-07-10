using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class BookingConfirmation : DocDataObject
	{
		public BookingConfirmation(ZGuid consolPK)
		{
			ConsolPK = consolPK;
		}

		public ZGuid ConsolPK { get; }

		public CodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(ShipmentType, value);
		}
		CodeDescription shipmentType;

		public Address Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}
		Address shipper;

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}
		Address carrier;

		public Address Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}
		Address consignee;

		public Address NotifyParty
		{
			get => notifyParty;
			set => notifyParty = SetChild(notifyParty, value);
		}
		Address notifyParty;

		public ImportableZTypeWrapper<ZString> CarrierBookingReference
		{
			get => carrierBookingReference;
			set => carrierBookingReference = SetChild(carrierBookingReference, value);
		}
		ImportableZTypeWrapper<ZString> carrierBookingReference;

		public Unloco CarrierBookingOffice
		{
			get => carrierBookingOffice;
			set => carrierBookingOffice = SetChild(carrierBookingOffice, value);
		}
		Unloco carrierBookingOffice;

		public Unloco Origin
		{
			get => origin;
			set => origin = SetChild(origin, value);
		}
		Unloco origin;

		public Unloco Destination
		{
			get => destination;
			set => destination = SetChild(destination, value);
		}
		Unloco destination;

		#region VesselName

		public ZString VesselName
		{
			get => vesselName;
			set
			{
				if (SetNonPersistentPropertyValue(VesselNameInfo, ref vesselName, value))
				{
					Validate(VesselNameInfo);
				}
			}
		}
		ZString vesselName;

		public ZPropertyInfo VesselNameInfo => GetZPropertyInfo(nameof(VesselName));

		#endregion

		#region LloydsIMO

		public ZString LloydsIMO
		{
			get => lloydsIMO;
			set
			{
				if (SetNonPersistentPropertyValue(LloydsIMOInfo, ref lloydsIMO, value))
				{
					Validate(LloydsIMOInfo);
				}
			}
		}
		ZString lloydsIMO;

		public ZPropertyInfo LloydsIMOInfo => GetZPropertyInfo(nameof(LloydsIMO));

		#endregion

		#region VoyageNumber

		public ZString VoyageNumber
		{
			get => voyageNumber;
			set
			{
				if (SetNonPersistentPropertyValue(VoyageNumberInfo, ref voyageNumber, value))
				{
					Validate(VoyageNumberInfo);
				}
			}
		}
		ZString voyageNumber;

		public ZPropertyInfo VoyageNumberInfo => GetZPropertyInfo(nameof(VoyageNumber));

		#endregion

		public CodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(ContainerMode, value);
		}
		CodeDescription containerMode;

		#region IsDoorPickup

		public ZBool IsDoorPickup
		{
			get => isDoorPickup;
			set
			{
				if (SetNonPersistentPropertyValue(IsDoorPickupInfo, ref isDoorPickup, value))
				{
					Validate(IsDoorPickupInfo);
				}
			}
		}
		ZBool isDoorPickup;

		public ZPropertyInfo IsDoorPickupInfo => GetZPropertyInfo(nameof(IsDoorPickup));

		#endregion

		#region IsDoorDelivery

		public ZBool IsDoorDelivery
		{
			get => isDoorDelivery;
			set
			{
				if (SetNonPersistentPropertyValue(IsDoorDeliveryInfo, ref isDoorDelivery, value))
				{
					Validate(IsDoorDeliveryInfo);
				}
			}
		}
		ZBool isDoorDelivery;

		public ZPropertyInfo IsDoorDeliveryInfo => GetZPropertyInfo(nameof(IsDoorDelivery));

		#endregion

		public Unloco PlaceOfReceipt
		{
			get => placeOfReceipt;
			set => placeOfReceipt = SetChild(placeOfReceipt, value);
		}
		Unloco placeOfReceipt;

		public Unloco PlaceOfDelivery
		{
			get => placeOfDelivery;
			set => placeOfDelivery = SetChild(placeOfDelivery, value);
		}
		Unloco placeOfDelivery;

		public Unloco PortOfLoad
		{
			get => portOfLoad;
			set => portOfLoad = SetChild(portOfLoad, value);
		}
		Unloco portOfLoad;

		public Unloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}
		Unloco portOfDischarge;

		#region EarliestDepartureDate

		public ZDateTime EarliestDepartureDate
		{
			get => earliestDepartureDate;
			set
			{
				if (SetNonPersistentPropertyValue(EarliestDepartureDateInfo, ref earliestDepartureDate, value))
				{
					Validate(EarliestDepartureDateInfo);
				}
			}
		}
		ZDateTime earliestDepartureDate;

		public ZPropertyInfo EarliestDepartureDateInfo => GetZPropertyInfo(nameof(EarliestDepartureDate));

		#endregion

		#region LatestDeliveryDate

		public ZDateTime LatestDeliveryDate
		{
			get => latestDeliveryDate;
			set
			{
				if (SetNonPersistentPropertyValue(LatestDeliveryDateInfo, ref latestDeliveryDate, value))
				{
					Validate(LatestDeliveryDateInfo);
				}
			}
		}
		ZDateTime latestDeliveryDate;

		public ZPropertyInfo LatestDeliveryDateInfo => GetZPropertyInfo(nameof(LatestDeliveryDate));

		#endregion

		public ImportableWrapper<IReadOnlyCollection<Transport>> Transports
		{
			get => transports;
			set => transports = SetChild(transports, value);
		}
		ImportableWrapper<IReadOnlyCollection<Transport>> transports;

		public ImportableZTypeWrapper<ZString> CarrierConfirmationNotes
		{
			get => carrierConfirmationNotes;
			set => carrierConfirmationNotes = SetChild(carrierConfirmationNotes, value);
		}
		ImportableZTypeWrapper<ZString> carrierConfirmationNotes;

		public IReadOnlyCollection<ImportableWrapper<Container>> Containers
		{
			get => containers;
			set => containers = SetChild(containers, value);
		}
		IReadOnlyCollection<ImportableWrapper<Container>> containers;

		#region BillOfLadingNumber

		public ImportableZTypeWrapper<ZString> BillOfLadingNumber
		{
			get => billOfLadingNumber;
			set => billOfLadingNumber = SetChild(billOfLadingNumber, value);
		}
		ImportableZTypeWrapper<ZString> billOfLadingNumber;

		#endregion

		#region ShipperReferenceNumber

		public ZString ShipperReferenceNumber
		{
			get => shipperReferenceNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ShipperReferenceNumberInfo, ref shipperReferenceNumber, value))
				{
					Validate(ShipperReferenceNumberInfo);
				}
			}
		}
		ZString shipperReferenceNumber;

		public ZPropertyInfo ShipperReferenceNumberInfo => GetZPropertyInfo(nameof(ShipperReferenceNumber));

		#endregion

		#region FreightForwarderReferenceNumber

		public ZString FreightForwarderReferenceNumber
		{
			get => freightForwarderReferenceNumber;
			set
			{
				if (SetNonPersistentPropertyValue(FreightForwarderReferenceNumberInfo, ref freightForwarderReferenceNumber, value))
				{
					Validate(FreightForwarderReferenceNumberInfo);
				}
			}
		}
		ZString freightForwarderReferenceNumber;

		public ZPropertyInfo FreightForwarderReferenceNumberInfo => GetZPropertyInfo(nameof(FreightForwarderReferenceNumber));

		#endregion

		#region CarrierContractNumber

		public ImportableZTypeWrapper<ZString> CarrierContractNumber
		{
			get => carrierContractNumber;
			set => carrierContractNumber = SetChild(carrierContractNumber, value);
		}
		ImportableZTypeWrapper<ZString> carrierContractNumber;

		#endregion

		#region CarrierQuoteNumber

		public ZString CarrierQuoteNumber
		{
			get => carrierQuoteNumber;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierQuoteNumberInfo, ref carrierQuoteNumber, value))
				{
					Validate(CarrierQuoteNumberInfo);
				}
			}
		}
		ZString carrierQuoteNumber;

		public ZPropertyInfo CarrierQuoteNumberInfo => GetZPropertyInfo(nameof(CarrierQuoteNumber));

		#endregion

		#region ContractNamedAccountNumber

		public ZString ContractNamedAccountNumber
		{
			get => contractNamedAccountNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ContractNamedAccountNumberInfo, ref contractNamedAccountNumber, value))
				{
					Validate(ContractNamedAccountNumberInfo);
				}
			}
		}
		ZString contractNamedAccountNumber;

		public ZPropertyInfo ContractNamedAccountNumberInfo => GetZPropertyInfo(nameof(ContractNamedAccountNumber));

		#endregion

		public ImportableWrapper<Address> ContainerTerminalOperator
		{
			get => containerTerminalOperator;
			set => containerTerminalOperator = SetChild(containerTerminalOperator, value);
		}
		ImportableWrapper<Address> containerTerminalOperator;

		public Address PickUpFrom
		{
			get => pickUpFrom;
			set => pickUpFrom = SetChild(pickUpFrom, value);
		}
		Address pickUpFrom;

		public Address DeliverTo
		{
			get => deliverTo;
			set => deliverTo = SetChild(deliverTo, value);
		}
		Address deliverTo;
	}
}
