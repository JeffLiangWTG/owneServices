using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class FinalManifest : DocDataObject, IDataSourceProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable const value")]
		public const string CustomsStatusCleared = "Cleared";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable const value")]
		public const string CustomsStatusHeld = "Held";

		public FinalManifest(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region IDataSourceProviderMembers

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion

		#region Addresses

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}
		Address carrier;

		public Address SendingParty
		{
			get => sendingParty;
			set => sendingParty = SetChild(sendingParty, value);
		}
		Address sendingParty;

		public Address CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}
		Address currentUser;

		#endregion

		#region ReceivingForwarder

		public Address ReceivingForwarder
		{
			get => receivingForwarder;
			set => receivingForwarder = SetChild(receivingForwarder, value);
		}
		Address receivingForwarder;

		#endregion

		#region ShipmentType

		public ICodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(shipmentType, value);
		}
		ICodeDescription shipmentType;

		#endregion

		#region ContainerMode

		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		ICodeDescription containerMode;

		#endregion

		#region CarrierCI5

		public RegistrationNumber CarrierCI5
		{
			get => carrierCI5;
			set => carrierCI5 = SetChild(carrierCI5, value);
		}
		RegistrationNumber carrierCI5;

		#endregion

		#region CarrierSON

		public RegistrationNumber CarrierSON
		{
			get => carrierSON;
			set => carrierSON = SetChild(carrierSON, value);
		}
		RegistrationNumber carrierSON;

		#endregion

		#region FormattedCarrierProviderID

		public ZString FormattedCarrierProviderID
		{
			get => formattedCarrierProviderID;
			set
			{
				if (SetNonPersistentPropertyValue(FormattedCarrierProviderIDInfo, ref formattedCarrierProviderID, value))
				{
					Validate(FormattedCarrierProviderIDInfo);
				}
			}
		}
		ZString formattedCarrierProviderID;

		public ZPropertyInfo FormattedCarrierProviderIDInfo => GetZPropertyInfo(nameof(FormattedCarrierProviderID));

		#endregion

		#region SendingPartyCI5

		public RegistrationNumber SendingPartyCI5
		{
			get => sendingPartyCI5;
			set => sendingPartyCI5 = SetChild(sendingPartyCI5, value);
		}
		RegistrationNumber sendingPartyCI5;

		#endregion

		#region SendingPartySOA

		public RegistrationNumber SendingPartySOA
		{
			get => sendingPartySOA;
			set => sendingPartySOA = SetChild(sendingPartySOA, value);
		}
		RegistrationNumber sendingPartySOA;

		#endregion

		#region SendingPartySON

		public RegistrationNumber SendingPartySON
		{
			get => sendingPartySON;
			set => sendingPartySON = SetChild(sendingPartySON, value);
		}
		RegistrationNumber sendingPartySON;

		#endregion

		#region SendingPartySOW

		public RegistrationNumber SendingPartySOW
		{
			get => sendingPartySOW;
			set => sendingPartySOW = SetChild(sendingPartySOW, value);
		}
		RegistrationNumber sendingPartySOW;

		#endregion

		#region FormattedSendingPartyProviderID

		public ZString FormattedSendingPartyProviderID
		{
			get => formattedSendingPartyProviderID;
			set
			{
				if (SetNonPersistentPropertyValue(FormattedSendingPartyProviderIDInfo, ref formattedSendingPartyProviderID, value))
				{
					Validate(FormattedSendingPartyProviderIDInfo);
				}
			}
		}
		ZString formattedSendingPartyProviderID;

		public ZPropertyInfo FormattedSendingPartyProviderIDInfo => GetZPropertyInfo(nameof(FormattedSendingPartyProviderID));

		#endregion

		#region CurrentUserCI5

		public RegistrationNumber CurrentUserCI5
		{
			get => currentUserCI5;
			set => currentUserCI5 = SetChild(currentUserCI5, value);
		}
		RegistrationNumber currentUserCI5;

		#endregion

		#region CurrentUserSOA

		public RegistrationNumber CurrentUserSOA
		{
			get => currentUserSOA;
			set => currentUserSOA = SetChild(currentUserSOA, value);
		}
		RegistrationNumber currentUserSOA;

		#endregion

		#region CurrentUserSON

		public RegistrationNumber CurrentUserSON
		{
			get => currentUserSON;
			set => currentUserSON = SetChild(currentUserSON, value);
		}
		RegistrationNumber currentUserSON;

		#endregion

		#region CurrentUserSOW

		public RegistrationNumber CurrentUserSOW
		{
			get => currentUserSOW;
			set => currentUserSOW = SetChild(currentUserSOW, value);
		}
		RegistrationNumber currentUserSOW;

		#endregion

		#region ReceivingForwarderSON

		public RegistrationNumber ReceivingForwarderSON
		{
			get => receivingForwarderSON;
			set => receivingForwarderSON = SetChild(receivingForwarderSON, value);
		}

		RegistrationNumber receivingForwarderSON;

		#endregion

		#region ReceivingForwarderCI5

		public RegistrationNumber ReceivingForwarderCI5
		{
			get => receivingForwarderCI5;
			set => receivingForwarderCI5 = SetChild(receivingForwarderCI5, value);
		}

		RegistrationNumber receivingForwarderCI5;

		#endregion

		#region Vessel

		public ZString Vessel
		{
			get => vessel;
			set
			{
				if (SetNonPersistentPropertyValue(VesselInfo, ref vessel, value))
				{
					Validate(VesselInfo);
				}
			}
		}

		ZString vessel;

		public ZPropertyInfo VesselInfo => GetZPropertyInfo(nameof(Vessel));

		#endregion

		#region CBKReference

		public const string CBKReferenceUXmlNameUXmlName = "CBK";

		public ZString CBKReference
		{
			get => cbkReference;
			set
			{
				if (SetNonPersistentPropertyValue(CBKReferenceInfo, ref cbkReference, value))
				{
					Validate(CBKReferenceInfo);
				}
			}
		}

		ZString cbkReference;

		public ZPropertyInfo CBKReferenceInfo => GetZPropertyInfo(nameof(CBKReference));

		#endregion

		#region OTCReference

		public const string OTCReferenceUXmlName = "OTC";

		public ZString OTCReference
		{
			get => otcReference;
			set
			{
				if (SetNonPersistentPropertyValue(OTCReferenceInfo, ref otcReference, value))
				{
					Validate(OTCReferenceInfo);
				}
			}
		}

		ZString otcReference;

		public ZPropertyInfo OTCReferenceInfo => GetZPropertyInfo(nameof(OTCReference));

		#endregion

		#region VoyageServiceCode

		public ZString VoyageServiceCode
		{
			get => voyageServiceCode;
			set
			{
				if (SetNonPersistentPropertyValue(VoyageServiceCodeInfo, ref voyageServiceCode, value))
				{
					Validate(VoyageServiceCodeInfo);
				}
			}
		}

		ZString voyageServiceCode;

		public ZPropertyInfo VoyageServiceCodeInfo => GetZPropertyInfo(nameof(VoyageServiceCode));

		#region VoyageFlightNo

		public ZString VoyageFlightNo
		{
			get => voyageFlightNo;
			set
			{
				if (SetNonPersistentPropertyValue(VoyageFlightNoInfo, ref voyageFlightNo, value))
				{
					Validate(VoyageFlightNoInfo);
				}
			}
		}

		ZString voyageFlightNo;

		public ZPropertyInfo VoyageFlightNoInfo => GetZPropertyInfo(nameof(VoyageFlightNo));

		#endregion

		#endregion

		#region ATPReference

		public const string ATPReferenceUXmlName = "ATP";

		public ZString ATPReference
		{
			get => atpReference;
			set
			{
				if (SetNonPersistentPropertyValue(ATPReferenceInfo, ref atpReference, value))
				{
					Validate(ATPReferenceInfo);
				}
			}
		}

		ZString atpReference;

		public ZPropertyInfo ATPReferenceInfo => GetZPropertyInfo(nameof(ATPReference));

		#endregion

		#region PortOfOrigin

		public IUnloco PortOfOrigin
		{
			get => portOfOrigin;
			set => portOfOrigin = SetChild(portOfOrigin, value);
		}
		IUnloco portOfOrigin;

		#endregion

		#region PortOfDestination

		public IUnloco PortOfDestination
		{
			get => portOfDestination;
			set => portOfDestination = SetChild(portOfDestination, value);
		}
		IUnloco portOfDestination;

		#endregion

		#region PortLocation

		public ZString PortLocation
		{
			get => portLocation;
			set
			{
				if (SetNonPersistentPropertyValue(PortLocationInfo, ref portLocation, value))
				{
					Validate(PortLocationInfo);
				}
			}
		}
		ZString portLocation;

		public ZPropertyInfo PortLocationInfo => GetZPropertyInfo(nameof(PortLocation));

		#endregion

		#region PortArea

		public ZString PortArea
		{
			get => portArea;
			set
			{
				if (SetNonPersistentPropertyValue(PortAreaInfo, ref portArea, value))
				{
					Validate(PortAreaInfo);
				}
			}
		}
		ZString portArea;

		public ZPropertyInfo PortAreaInfo => GetZPropertyInfo(nameof(PortArea));

		#endregion

		#region Transporter

		public Address Transporter
		{
			get => transporter;
			set => transporter = SetChild(transporter, value);
		}
		Address transporter;

		#endregion

		#region TransporterCI5

		public RegistrationNumber TransporterCI5
		{
			get => transporterCI5;
			set => transporterCI5 = SetChild(transporterCI5, value);
		}
		RegistrationNumber transporterCI5;

		#endregion

		#region TransporterSON

		public RegistrationNumber TransporterSON
		{
			get => transporterSON;
			set => transporterSON = SetChild(transporterSON, value);
		}
		RegistrationNumber transporterSON;

		#endregion

		#region FormattedTransporterProviderID

		public ZString FormattedTransporterProviderID
		{
			get => formattedTransporterProviderID;
			set
			{
				if (SetNonPersistentPropertyValue(FormattedTransporterProviderIDInfo, ref formattedTransporterProviderID, value))
				{
					Validate(FormattedTransporterProviderIDInfo);
				}
			}
		}
		ZString formattedTransporterProviderID;

		public ZPropertyInfo FormattedTransporterProviderIDInfo => GetZPropertyInfo(nameof(FormattedTransporterProviderID));

		#endregion

		#region TransportMode

		public ZString TransportMode
		{
			get => transportMode;
			set
			{
				if (SetNonPersistentPropertyValue(TransportModeInfo, ref transportMode, value))
				{
					Validate(TransportModeInfo);
				}
			}
		}

		ZString transportMode;

		public ZPropertyInfo TransportModeInfo => GetZPropertyInfo(nameof(TransportMode));

		#endregion

		#region VehicleRegistration

		public ZString VehicleRegistration
		{
			get => vehicleRegistration;
			set
			{
				if (SetNonPersistentPropertyValue(VehicleRegistrationInfo, ref vehicleRegistration, value))
				{
					Validate(VehicleRegistrationInfo);
				}
			}
		}

		ZString vehicleRegistration;

		public ZPropertyInfo VehicleRegistrationInfo => GetZPropertyInfo(nameof(VehicleRegistration));

		#endregion

		#region DeliveryLocation

		public ZString DeliveryLocation
		{
			get => deliveryLocation;
			set
			{
				if (SetNonPersistentPropertyValue(DeliveryLocationInfo, ref deliveryLocation, value))
				{
					Validate(DeliveryLocationInfo);
				}
			}
		}
		ZString deliveryLocation;

		public ZPropertyInfo DeliveryLocationInfo => GetZPropertyInfo(nameof(DeliveryLocation));

		#endregion

		#region DeliveryArea

		public ZString DeliveryArea
		{
			get => deliveryArea;
			set
			{
				if (SetNonPersistentPropertyValue(DeliveryAreaInfo, ref deliveryArea, value))
				{
					Validate(DeliveryAreaInfo);
				}
			}
		}
		ZString deliveryArea;

		public ZPropertyInfo DeliveryAreaInfo => GetZPropertyInfo(nameof(DeliveryArea));

		#endregion

		#region ExpectedArrivalAtPort

		public ZDateTime ExpectedArrivalAtPort
		{
			get => expectedArrivalAtPort;
			set
			{
				if (SetNonPersistentPropertyValue(ExpectedArrivalAtPortInfo, ref expectedArrivalAtPort, value))
				{
					Validate(ExpectedArrivalAtPortInfo);
				}
			}
		}

		ZDateTime expectedArrivalAtPort;

		public ZPropertyInfo ExpectedArrivalAtPortInfo => GetZPropertyInfo(nameof(ExpectedArrivalAtPort));

		#endregion

		#region ConsolNumber

		public ZString ConsolNumber
		{
			get => consolNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ConsolNumberInfo, ref consolNumber, value))
				{
					Validate(ConsolNumberInfo);
				}
			}
		}

		ZString consolNumber;

		public ZPropertyInfo ConsolNumberInfo => GetZPropertyInfo(nameof(ConsolNumber));

		#endregion

		#region PackingFunctionalReference

		public ZString PackingFunctionalReference
		{
			get => packingFunctionalReference;
			set
			{
				if (SetNonPersistentPropertyValue(PackingFunctionalReferenceInfo, ref packingFunctionalReference, value))
				{
					Validate(PackingFunctionalReferenceInfo);
				}
			}
		}

		ZString packingFunctionalReference;

		public ZPropertyInfo PackingFunctionalReferenceInfo => GetZPropertyInfo(nameof(PackingFunctionalReference));

		#endregion

		#region CarrierBookingRef

		public ZString CarrierBookingRef
		{
			get => carrierBookingRef;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierBookingRefInfo, ref carrierBookingRef, value))
				{
					Validate(CarrierBookingRefInfo);
				}
			}
		}

		ZString carrierBookingRef;

		public ZPropertyInfo CarrierBookingRefInfo => GetZPropertyInfo(nameof(CarrierBookingRef));

		#endregion

		#region BookingConfirmation

		public ZString BookingConfirmation
		{
			get => bookingConfirmation;
			set
			{
				if (SetNonPersistentPropertyValue(BookingConfirmationInfo, ref bookingConfirmation, value))
				{
					Validate(BookingConfirmationInfo);
				}
			}
		}

		ZString bookingConfirmation;

		public ZPropertyInfo BookingConfirmationInfo => GetZPropertyInfo(nameof(BookingConfirmation));

		#endregion

		#region Containers

		public IReadOnlyCollection<BookingContainer> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}

		IReadOnlyCollection<BookingContainer> containers;

		#endregion

		#region GoodsDetails

		public IReadOnlyCollection<GoodsDetail> GoodsDetails
		{
			get => goodsDetails;
			set => goodsDetails = SetChildCollection(goodsDetails, value);
		}

		IReadOnlyCollection<GoodsDetail> goodsDetails;

		#endregion

		#region TemperatureMinimum

		public IMeasurement TemperatureMinimum
		{
			get => temperatureMinimum;
			set => temperatureMinimum = SetChild(temperatureMinimum, value);
		}

		IMeasurement temperatureMinimum;

		#endregion

		#region TemperatureMaximum

		public IMeasurement TemperatureMaximum
		{
			get => temperatureMaximum;
			set => temperatureMaximum = SetChild(temperatureMaximum, value);
		}

		IMeasurement temperatureMaximum;

		#endregion

		#region PackingLines

		public IReadOnlyCollection<BookingPackingLine> PackingLines
		{
			get => packingLines;
			set => packingLines = SetChildCollection(packingLines, value);
		}

		IReadOnlyCollection<BookingPackingLine> packingLines;

		#endregion

		#region Contractor

		public IAddress Contractor
		{
			get => contractor;
			set => contractor = SetChild(contractor, value);
		}

		IAddress contractor;

		#endregion Contractor

		#region IsFumigated

		public ZBool IsFumigated
		{
			get => isFumigated;
			set
			{
				if (SetNonPersistentPropertyValue(IsFumigatedInfo, ref isFumigated, value))
				{
					Validate(IsFumigatedInfo);
				}
			}
		}

		ZBool isFumigated;

		public ZPropertyInfo IsFumigatedInfo => GetZPropertyInfo(nameof(IsFumigated));

		#endregion

		#region OperationalPort

		public IUnloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		IUnloco operationalPort;

		#endregion

		#region PCS

		public ZString PCS
		{
			get => pcs;
			set
			{
				if (SetNonPersistentPropertyValue(PCSInfo, ref pcs, value))
				{
					Validate(PCSInfo);
				}
			}
		}
		ZString pcs;

		public ZPropertyInfo PCSInfo => GetZPropertyInfo(nameof(PCS));

		#endregion

		#region ErrorPlaceHolder

		public ZString ErrorPlaceHolder
		{
			get => errorPlaceHolder;
			set
			{
				if (SetNonPersistentPropertyValue(ErrorPlaceHolderInfo, ref errorPlaceHolder, value))
				{
					Validate(ErrorPlaceHolderInfo);
				}
			}
		}

		ZString errorPlaceHolder;

		public ZPropertyInfo ErrorPlaceHolderInfo => GetZPropertyInfo(nameof(ErrorPlaceHolder));

		#endregion ErrorPlaceHolder
	}
}
