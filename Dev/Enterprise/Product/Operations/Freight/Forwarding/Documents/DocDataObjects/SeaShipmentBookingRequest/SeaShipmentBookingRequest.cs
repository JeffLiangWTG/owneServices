using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class SeaShipmentBookingRequest : DocDataObject, IDataSourceProvider
	{
		public SeaShipmentBookingRequest(ZString sourceType, ZString sourceID)
		{
			SourceType = sourceType;
			SourceID = sourceID;
			goodsAndEquipmentDetails = new List<SeaShipmentBookingRequestPackLine>();
		}

		#region IDataSourceProvider members

		public ZString SourceID
		{
			get => sourceID;
			set
			{
				sourceID = value;
				SourceIDInfo.RefreshBinding();
			}
		}
		ZString sourceID;

		public ZPropertyInfo SourceIDInfo => GetZPropertyInfo(nameof(SourceID));

		public ZString SourceType { get; }

		#endregion IDataSourceProvider members

		#region TransportMode

		public CodeDescription TransportMode
		{
			get => transportMode;
			set => transportMode = SetChild(TransportMode, value);
		}
		CodeDescription transportMode;

		#endregion

		#region ReleaseType

		public CodeDescription ReleaseType
		{
			get => releaseType;
			set => releaseType = SetChild(ReleaseType, value);
		}
		CodeDescription releaseType;

		#endregion

		#region Shipper

		public Address Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}

		Address shipper;

		#endregion

		#region Consignee

		public Address Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}

		Address consignee;

		#endregion

		#region Carrier

		public Address Recipient
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		Address carrier;

		#endregion

		#region CurrentUser

		public Address CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}

		Address currentUser;

		#endregion

		#region PickupFrom

		public Address PickupFrom
		{
			get => pickupFrom;
			set => pickupFrom = SetChild(pickupFrom, value);
		}

		Address pickupFrom;

		#endregion

		#region DeliverTo

		public Address NotifyParty
		{
			get => deliverTo;
			set => deliverTo = SetChild(deliverTo, value);
		}
		public Address DeliverTo
		{
			get => deliverTo;
			set => deliverTo = SetChild(deliverTo, value);
		}

		Address deliverTo;

		#endregion

		#region CarrierBookingReference

		public ZString BookingReference
		{
			get => bookingReferencess;
			set
			{
				if (SetNonPersistentPropertyValue(BookingReferenceInfo, ref bookingReferencess, value))
				{
					Validate(BookingReferenceInfo);
				}
			}
		}
		ZString bookingReferencess;

		public ZPropertyInfo BookingReferenceInfo => GetZPropertyInfo(nameof(BookingReference));

		#endregion

		#region CarrierBookingOffice

		public Unloco CarrierBookingOffice
		{
			get => carrierBookingOffice;
			set => carrierBookingOffice = SetChild(carrierBookingOffice, value);
		}

		Unloco carrierBookingOffice;

		#endregion

		#region Origin

		public Unloco Origin
		{
			get => origin;
			set => origin = SetChild(origin, value);
		}
		Unloco origin;

		#endregion

		#region Destination

		public Unloco Destination
		{
			get => destination;
			set => destination = SetChild(destination, value);
		}
		Unloco destination;

		#endregion

		#region GoodsAndEquipmentDetails

		public IReadOnlyCollection<SeaShipmentBookingRequestPackLine> GoodsAndEquipmentDetails
		{
			get => goodsAndEquipmentDetails;
			set => goodsAndEquipmentDetails = SetChildCollection(goodsAndEquipmentDetails, value);
		}

		IReadOnlyCollection<SeaShipmentBookingRequestPackLine> goodsAndEquipmentDetails;

		#endregion

		#region Mode

		public CodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(ContainerMode, value);
		}
		CodeDescription containerMode;

		#endregion

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

		#region PlaceOfReceipt

		public Unloco PlaceOfReceipt
		{
			get => placeOfReceipt;
			set => placeOfReceipt = SetChild(placeOfReceipt, value);
		}
		Unloco placeOfReceipt;

		#endregion

		#region PlaceOfDelivery

		public Unloco PlaceOfDelivery
		{
			get => placeOfDelivery;
			set => placeOfDelivery = SetChild(placeOfDelivery, value);
		}
		Unloco placeOfDelivery;

		#endregion

		#region PortOfLoading

		public Unloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}
		Unloco portOfLoading;

		#endregion

		#region PortOfDischarge

		public Unloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}
		Unloco portOfDischarge;

		#endregion

		#region FreightPayableAt

		public Unloco FreightPayableAt
		{
			get => freightPayableAt;
			set => freightPayableAt = SetChild(freightPayableAt, value);
		}

		Unloco freightPayableAt;

		#endregion

		#region OperationalPort

		public Unloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		Unloco operationalPort;

		#endregion

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

		#region Est. Cargo Pickup Date Time

		public ZDateTime EstCargoPickupDateTime
		{
			get => estCargoPickupDateTime;
			set
			{
				if (SetNonPersistentPropertyValue(EstCargoPickupDateTimeInfo, ref estCargoPickupDateTime, value))
				{
					Validate(EstCargoPickupDateTimeInfo);
				}
			}
		}
		ZDateTime estCargoPickupDateTime;

		public ZPropertyInfo EstCargoPickupDateTimeInfo => GetZPropertyInfo(nameof(EstCargoPickupDateTime));

		#endregion

		#region ShipperReference

		public ZString ShipperReference
		{
			get => shipperReference;
			set
			{
				if (SetNonPersistentPropertyValue(ShipperReferenceInfo, ref shipperReference, value))
				{
					Validate(ShipperReferenceInfo);
				}
			}
		}
		ZString shipperReference;

		public ZPropertyInfo ShipperReferenceInfo => GetZPropertyInfo(nameof(ShipperReference));

		#endregion

		#region CarrierContractNumber

		public ZString CarrierContractNumbersFormatted
		{
			get => carrierContractNumber;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierContractNumberInfo, ref carrierContractNumber, value))
				{
					Validate(CarrierContractNumberInfo);
				}
			}
		}

		ZString carrierContractNumber;

		public ZPropertyInfo CarrierContractNumberInfo => GetZPropertyInfo(nameof(CarrierContractNumbersFormatted));

		#endregion

		#region PaymentTerms

		public IOptionalCharge PaymentTerms
		{
			get => paymentTerms;
			set => paymentTerms = SetChild(paymentTerms, value);
		}

		IOptionalCharge paymentTerms;

		#endregion

		#region AdditionalTerms

		public ZString AdditionalTerms
		{
			get => additionalTerms;
			set
			{
				if (SetNonPersistentPropertyValue(AdditionalTermsInfo, ref additionalTerms, value))
				{
					Validate(AdditionalTermsInfo);
				}
			}
		}

		ZString additionalTerms;

		public ZPropertyInfo AdditionalTermsInfo => GetZPropertyInfo(nameof(AdditionalTerms));

		#endregion

		#region GoodsHandlingInstruction

		public ZString GoodsHandlingInstructions
		{
			get => goodsHandlingInstruction;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsHandlingInstructionInfo, ref goodsHandlingInstruction, value))
				{
					Validate(GoodsHandlingInstructionInfo);
				}
			}
		}

		ZString goodsHandlingInstruction;

		public ZPropertyInfo GoodsHandlingInstructionInfo => GetZPropertyInfo(nameof(GoodsHandlingInstructions));

		#endregion

		#region TotalPacks

		public ZInt TotalPacks
		{
			get => totalPacks;
			set
			{
				if (SetNonPersistentPropertyValue(TotalPacksInfo, ref totalPacks, value))
				{
					Validate(TotalPacksInfo);
				}
			}
		}

		ZInt totalPacks;

		public ZPropertyInfo TotalPacksInfo => GetZPropertyInfo(nameof(TotalPacks));

		#endregion

		#region TotalCargoWeight

		public Measurement TotalCargoWeight
		{
			get => totalCargoWeight;
			set => totalCargoWeight = SetChild(totalCargoWeight, value);
		}

		Measurement totalCargoWeight;

		#endregion

		#region TotalCargoVolume

		public Measurement TotalCargoVolume
		{
			get => totalCargoVolume;
			set => totalCargoVolume = SetChild(totalCargoVolume, value);
		}

		Measurement totalCargoVolume;

		#endregion

		#region Numbers

		public IReadOnlyCollection<IReferenceNumber> Numbers
		{
			get => numbers;
			set => numbers = SetChildCollection(numbers, value);
		}

		IReadOnlyCollection<IReferenceNumber> numbers;

		#endregion

		#region Appearance Properties

		#region IsCoload

		public ZBool IsCoload
		{
			get => false;
		}

		#endregion

		#region IsGatewayCoload

		public ZBool IsGatewayCoload
		{
			get => false;
		}

		#endregion

		#region Transport

		public Transports Transports
		{
			get => transports;
			set => transports = SetChild(transports, value);
		}
		Transports transports;

		#endregion

		#region ErrorPlaceHolder

		public ZString ErrorPlaceHolder
		{
			get => errorPlaceHolder;
			set
			{
				if (SetNonPersistentPropertyValue(ErrorPlaceHolderInfo, ref errorPlaceHolder, value))
				{
				}
			}
		}

		ZString errorPlaceHolder;

		public ZPropertyInfo ErrorPlaceHolderInfo => GetZPropertyInfo(nameof(ErrorPlaceHolder));

		#endregion

		#region MasterBillNumber

		public ZString MasterBillNumber
		{
			get => masterBillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(MasterBillNumberInfo, ref masterBillNumber, value))
				{
					Validate(MasterBillNumberInfo);
				}
			}
		}
		ZString masterBillNumber;

		public ZPropertyInfo MasterBillNumberInfo => GetZPropertyInfo(nameof(MasterBillNumber));

		#endregion

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

		#region LegTransportMode

		public ZString LegTransportMode
		{
			get => legTransportMode;
			set
			{
				if (SetNonPersistentPropertyValue(LegTransportModeInfo, ref legTransportMode, value))
				{
					Validate(LegTransportModeInfo);
				}
			}
		}
		ZString legTransportMode;

		public ZPropertyInfo LegTransportModeInfo => GetZPropertyInfo(nameof(LegTransportMode));

		#endregion

		#region LegType

		public ZString LegType
		{
			get => legType;
			set
			{
				if (SetNonPersistentPropertyValue(LegTypeInfo, ref legType, value))
				{
					Validate(LegTypeInfo);
				}
			}
		}
		ZString legType;

		public ZPropertyInfo LegTypeInfo => GetZPropertyInfo(nameof(LegType));

		#endregion

		#region LegOrder

		public ZByte LegOrder
		{
			get => legOrder;
			set
			{
				if (SetNonPersistentPropertyValue(LegOrderInfo, ref legOrder, value))
				{
					Validate(LegOrderInfo);
				}
			}
		}
		ZByte legOrder;

		public ZPropertyInfo LegOrderInfo => GetZPropertyInfo(nameof(LegOrder));

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

		#region ETD

		public ZDateTime ETD
		{
			get => etd;
			set
			{
				if (SetNonPersistentPropertyValue(ETDInfo, ref etd, value))
				{
					Validate(ETDInfo);
				}
			}
		}

		ZDateTime etd;

		public ZPropertyInfo ETDInfo => GetZPropertyInfo(nameof(ETD));

		#endregion

		#region ETA

		public ZDateTime ETA
		{
			get => eta;
			set
			{
				if (SetNonPersistentPropertyValue(ETAInfo, ref eta, value))
				{
					Validate(ETAInfo);
				}
			}
		}

		ZDateTime eta;

		public ZPropertyInfo ETAInfo => GetZPropertyInfo(nameof(ETA));

		#endregion

		#endregion

		#region IsConsolAttached

		public ZBool IsConsolAttached
		{
			get => isConsolAttached;
			set
			{
				if (SetNonPersistentPropertyValue(IsConsolAttachedInfo, ref isConsolAttached, value))
				{
					Validate(IsConsolAttachedInfo);
				}
			}
		}
		ZBool isConsolAttached;

		public ZPropertyInfo IsConsolAttachedInfo => GetZPropertyInfo(nameof(IsConsolAttached));

		#endregion

		#region IsNVO

		public bool IsNVO => true;

		#endregion

		#region IsRequiredSendAttachment

		public ZBool IsRequiredSendAttachment
		{
			get => isRequiredSendAttachment;
			set
			{
				if (SetNonPersistentPropertyValue(IsRequiredSendAttachmentInfo, ref isRequiredSendAttachment, value))
				{
					Validate(IsRequiredSendAttachmentInfo);
				}
			}
		}
		ZBool isRequiredSendAttachment;

		public ZPropertyInfo IsRequiredSendAttachmentInfo => GetZPropertyInfo(nameof(IsRequiredSendAttachment));

		#endregion
	}
}
