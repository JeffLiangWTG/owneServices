using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class UnderbondMovementRequest : DocDataObject, IDataSourceProvider
	{
		public UnderbondMovementRequest(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

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

		#region DTI

		public ZBool DTI
		{
			get => dti;
			set
			{
				if (SetNonPersistentPropertyValue(DTIInfo, ref dti, value))
				{
					Validate(DTIInfo);
				}
			}
		}

		ZBool dti;

		public ZPropertyInfo DTIInfo => GetZPropertyInfo(nameof(DTI));

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

		#region ATPReference

		public ZString ATP
		{
			get => atp;
			set
			{
				if (SetNonPersistentPropertyValue(ATPInfo, ref atp, value))
				{
					Validate(ATPInfo);
				}
			}
		}

		ZString atp;

		public ZPropertyInfo ATPInfo => GetZPropertyInfo(nameof(ATP));

		#endregion

		#region OperationalPort

		public IUnloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		IUnloco operationalPort;

		#endregion

		#region PortOfOrigin

		public IUnloco PortOfOrigin
		{
			get => portOfOrigin;
			set => portOfOrigin = SetChild(portOfOrigin, value);
		}

		IUnloco portOfOrigin;

		#endregion

		#region PortOfTranshipment

		public IUnloco PortOfTranshipment
		{
			get => portOfTranshipment;
			set => portOfTranshipment = SetChild(portOfTranshipment, value);
		}

		IUnloco portOfTranshipment;

		#endregion

		#region PortOfDestination

		public IUnloco PortOfDestination
		{
			get => portOfDestination;
			set => portOfDestination = SetChild(portOfDestination, value);
		}

		IUnloco portOfDestination;

		#endregion

		#region ReceivingForwarder

		public IAddress ReceivingForwarder
		{
			get => receivingForwarder;
			set => receivingForwarder = SetChild(receivingForwarder, value);
		}

		IAddress receivingForwarder;

		#endregion

		#region ReceivingForwarderCI5

		public RegistrationNumber ReceivingForwarderCI5
		{
			get => receivingForwarderCI5;
			set => receivingForwarderCI5 = SetChild(receivingForwarderCI5, value);
		}

		RegistrationNumber receivingForwarderCI5;

		#endregion

		#region ReceivingForwarderSON

		public RegistrationNumber ReceivingForwarderSON
		{
			get => receivingForwarderSON;
			set => receivingForwarderSON = SetChild(receivingForwarderSON, value);
		}

		RegistrationNumber receivingForwarderSON;

		#endregion

		#region SendingForwarder

		public IAddress SendingForwarder
		{
			get => sendingForwarder;
			set => sendingForwarder = SetChild(sendingForwarder, value);
		}

		IAddress sendingForwarder;

		#endregion

		#region SendingForwarderCI5

		public RegistrationNumber SendingForwarderCI5
		{
			get => sendingForwarderCI5;
			set => sendingForwarderCI5 = SetChild(sendingForwarderCI5, value);
		}

		RegistrationNumber sendingForwarderCI5;

		#endregion

		#region SendingForwarderSON

		public RegistrationNumber SendingForwarderSON
		{
			get => sendingForwarderSON;
			set => sendingForwarderSON = SetChild(sendingForwarderSON, value);
		}

		RegistrationNumber sendingForwarderSON;

		#endregion

		#region Carrier

		public IAddress Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		IAddress carrier;

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

		#region CarrierCCC

		public RegistrationNumber CarrierCCC
		{
			get => carrierCCC;
			set => carrierCCC = SetChild(carrierCCC, value);
		}

		RegistrationNumber carrierCCC;

		#endregion

		#region SendingParty

		public IAddress SendingParty
		{
			get => sendingParty;
			set => sendingParty = SetChild(sendingParty, value);
		}

		IAddress sendingParty;

		#endregion SendingParty

		#region SendingPartyCI5

		public RegistrationNumber SendingPartyCI5
		{
			get => sendingPartyCI5;
			set => sendingPartyCI5 = SetChild(sendingPartyCI5, value);
		}

		RegistrationNumber sendingPartyCI5;

		#endregion

		#region SendingPartySON

		public RegistrationNumber SendingPartySON
		{
			get => sendingPartySON;
			set => sendingPartySON = SetChild(sendingPartySON, value);
		}

		RegistrationNumber sendingPartySON;

		#endregion

		#region Underbond Movement Request

		#region LoadPort

		public ICodeDescription PortLocationFrom
		{
			get => loadPort;
			set => loadPort = SetChild(loadPort, value);
		}

		ICodeDescription loadPort;

		#endregion

		#region LoadPortArea

		public ICodeDescription PortAreaFrom
		{
			get => loadPortArea;
			set => loadPortArea = SetChild(loadPortArea, value);
		}

		ICodeDescription loadPortArea;

		#endregion

		#region DischargePort

		public ICodeDescription PortLocationTo
		{
			get => dischargePort;
			set => dischargePort = SetChild(dischargePort, value);
		}

		ICodeDescription dischargePort;

		#endregion

		#region DischargePortArea

		public ICodeDescription PortAreaTo
		{
			get => dischargePortArea;
			set => dischargePortArea = SetChild(dischargePortArea, value);
		}

		ICodeDescription dischargePortArea;

		#endregion

		#region TransporterName

		public IAddress Transporter
		{
			get { return transporter; }
			set => transporter = SetChild(transporter, value);
		}

		IAddress transporter;

		#endregion

		#region TransporterCI5

		public RegistrationNumber TransporterCI5
		{
			get { return transporterCI5; }
			set => transporterCI5 = SetChild(transporterCI5, value);
		}

		RegistrationNumber transporterCI5;

		#endregion

		#region TransporterSON

		public RegistrationNumber TransporterSON
		{
			get { return transporterSON; }
			set => transporterSON = SetChild(transporterSON, value);
		}

		RegistrationNumber transporterSON;

		#endregion

		#region TransportMode

		[MaxLength(NotificationTypes.MessageError, 3)]
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

		#region RequestStatus

		public UnderbondMovementRequestStatus RequestStatus
		{
			get => requestStatus;
			set => requestStatus = SetChild(requestStatus, value);
		}

		UnderbondMovementRequestStatus requestStatus = new UnderbondMovementRequestStatus();

		#endregion

		#region ReasonID

		public UnderbondMovementRequestReasonID ReasonID
		{
			get => reasonID;
			set => reasonID = SetChild(reasonID, value);
		}

		UnderbondMovementRequestReasonID reasonID = new UnderbondMovementRequestReasonID();

		#endregion

		#region Subcontracted

		public ZBool Subcontracted
		{
			get => subcontracted;
			set
			{
				if (SetNonPersistentPropertyValue(SubcontractedInfo, ref subcontracted, value))
				{
					Validate(SubcontractedInfo);
				}
			}
		}

		ZBool subcontracted;

		public ZPropertyInfo SubcontractedInfo => GetZPropertyInfo(nameof(Subcontracted));

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

		#region ReasonNote

		public ZString ReasonNote
		{
			get => reasonNote;
			set
			{
				if (SetNonPersistentPropertyValue(ReasonNoteInfo, ref reasonNote, value))
				{
					Validate(ReasonNoteInfo);
				}
			}
		}

		ZString reasonNote;

		public ZPropertyInfo ReasonNoteInfo => GetZPropertyInfo(nameof(ReasonNote));

		#endregion

		#region AuthorizationRequired

		public UnderbondMovementRequestAuthorizationRequired AuthorizationRequired
		{
			get => authorizationRequired;
			set => authorizationRequired = SetChild(authorizationRequired, value);
		}
		UnderbondMovementRequestAuthorizationRequired authorizationRequired = new UnderbondMovementRequestAuthorizationRequired();

		#endregion

		#region DateOfResponse

		public ZDateTime DateOfResponse
		{
			get => dateOfResponse;
			set
			{
				if (SetNonPersistentPropertyValue(DateOfResponseInfo, ref dateOfResponse, value))
				{
					Validate(DateOfResponseInfo);
				}
			}
		}

		ZDateTime dateOfResponse;

		public ZPropertyInfo DateOfResponseInfo => GetZPropertyInfo(nameof(DateOfResponse));

		#endregion

		#region ResponseType

		public UnderbondMovementRequestAuthorizationResponseType ResponseType
		{
			get => responseType;
			set => responseType = SetChild(responseType, value);
		}
		UnderbondMovementRequestAuthorizationResponseType responseType = new UnderbondMovementRequestAuthorizationResponseType();

		#endregion

		#endregion

		#region Customs Information

		#region DeclarationDate

		public ZDateTime DeclarationDate
		{
			get => declarationDate;
			set
			{
				if (SetNonPersistentPropertyValue(DeclarationDateInfo, ref declarationDate, value))
				{
					Validate(DeclarationDateInfo);
				}
			}
		}

		ZDateTime declarationDate;

		public ZPropertyInfo DeclarationDateInfo => GetZPropertyInfo(nameof(DeclarationDate));

		#endregion

		#region DeclarationNumber

		public ZString DeclarationNumber
		{
			get => declarationNumber;
			set
			{
				if (SetNonPersistentPropertyValue(DeclarationNumberInfo, ref declarationNumber, value))
				{
					Validate(DeclarationNumberInfo);
				}
			}
		}

		ZString declarationNumber;

		public ZPropertyInfo DeclarationNumberInfo => GetZPropertyInfo(nameof(DeclarationNumber));

		#endregion

		#region DeclarationVersion

		public ZString DeclarationVersion
		{
			get => declarationVersion;
			set
			{
				if (SetNonPersistentPropertyValue(DeclarationVersionInfo, ref declarationVersion, value))
				{
					Validate(DeclarationVersionInfo);
				}
			}
		}

		ZString declarationVersion;

		public ZPropertyInfo DeclarationVersionInfo => GetZPropertyInfo(nameof(DeclarationVersion));

		#endregion

		#endregion

		#region Additional References

		#region CarrierBookingReference

		public ZString CarrierBookingReference
		{
			get => carrierBookingReference;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierBookingReferenceInfo, ref carrierBookingReference, value))
				{
					Validate(CarrierBookingReferenceInfo);
				}
			}
		}

		ZString carrierBookingReference;

		public ZPropertyInfo CarrierBookingReferenceInfo => GetZPropertyInfo(nameof(CarrierBookingReference));

		#endregion

		#region WayBillNumber

		public ZString BillOfLading
		{
			get => billOfLading;
			set
			{
				if (SetNonPersistentPropertyValue(BillOfLadingInfo, ref billOfLading, value))
				{
					Validate(BillOfLadingInfo);
				}
			}
		}

		ZString billOfLading;

		public ZPropertyInfo BillOfLadingInfo => GetZPropertyInfo(nameof(BillOfLading));

		#endregion

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

		#region BookingConfirmation

		public ZString BookingConfirmationCBK
		{
			get => bookingConfirmationCBK;
			set
			{
				if (SetNonPersistentPropertyValue(BookingConfirmationCBKInfo, ref bookingConfirmationCBK, value))
				{
					Validate(BookingConfirmationCBKInfo);
				}
			}
		}

		ZString bookingConfirmationCBK;

		public ZPropertyInfo BookingConfirmationCBKInfo => GetZPropertyInfo(nameof(BookingConfirmationCBK));

		#endregion

		#region BOL

		public ZString BOL
		{
			get => bol;
			set
			{
				if (SetNonPersistentPropertyValue(BOLInfo, ref bol, value))
				{
					Validate(BOLInfo);
				}
			}
		}

		ZString bol;

		public ZPropertyInfo BOLInfo => GetZPropertyInfo(nameof(BOL));

		#endregion

		#region BOLAPPlusID

		public ZString BOLAPPlusID
		{
			get => bolAPPlusID;
			set
			{
				if (SetNonPersistentPropertyValue(BOLAPPlusIDInfo, ref bolAPPlusID, value))
				{
					Validate(BOLAPPlusIDInfo);
				}
			}
		}
		ZString bolAPPlusID;
		public ZPropertyInfo BOLAPPlusIDInfo => GetZPropertyInfo(nameof(BOLAPPlusID));

		#endregion

		#region CFSUnpackingRef

		public ZString CFSUnpackingRef
		{
			get => cfsUnpackingRef;
			set
			{
				if (SetNonPersistentPropertyValue(CFSUnpackingRefInfo, ref cfsUnpackingRef, value))
				{
					Validate(CFSUnpackingRefInfo);
				}
			}
		}

		ZString cfsUnpackingRef;

		public ZPropertyInfo CFSUnpackingRefInfo => GetZPropertyInfo(nameof(CFSUnpackingRef));

		#endregion

		#region MoveReqAPPlusID

		public ZString MoveReqAPPlusID
		{
			get => moveReqAPPlusID;
			set
			{
				if (SetNonPersistentPropertyValue(MoveReqAPPlusIDInfo, ref moveReqAPPlusID, value))
				{
					Validate(MoveReqAPPlusIDInfo);
				}
			}
		}
		ZString moveReqAPPlusID;
		public ZPropertyInfo MoveReqAPPlusIDInfo => GetZPropertyInfo(nameof(MoveReqAPPlusID));

		#endregion

		#endregion

		#region Containers

		public IReadOnlyCollection<UnderbondMovementRequestContainer> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}

		IReadOnlyCollection<UnderbondMovementRequestContainer> containers;

		#endregion

		#region Port Dues

		#region PortDuesPort

		public IUnloco PortDuesPort
		{
			get => portDuesPort;
			set => portDuesPort = SetChild(portDuesPort, value);
		}

		IUnloco portDuesPort;

		#endregion

		#region PortDuesAmount

		public ZDecimal PortDuesAmount
		{
			get => portDuesAmount;
			set
			{
				if (SetNonPersistentPropertyValue(PortDuesAmountInfo, ref portDuesAmount, value))
				{
					Validate(PortDuesAmountInfo);
				}
			}
		}

		ZDecimal portDuesAmount;

		public ZPropertyInfo PortDuesAmountInfo => GetZPropertyInfo(nameof(PortDuesAmount));

		#endregion

		#region PortDuesCurrency

		public ICodeDescription PortDuesCurrency
		{
			get { return portDuesCurrency; }
			set => portDuesCurrency = SetChild(portDuesCurrency, value);
		}

		ICodeDescription portDuesCurrency;

		#endregion

		#region PortDuesPayingPartyAPPlusID

		public ZString PortDuesPayingPartyAPPlusID
		{
			get => portDuesPayingPartyAPPlusID;
			set
			{
				if (SetNonPersistentPropertyValue(PortDuesPayingPartyAPPlusIDInfo, ref portDuesPayingPartyAPPlusID, value))
				{
					Validate(PortDuesPayingPartyAPPlusIDInfo);
				}
			}
		}

		ZString portDuesPayingPartyAPPlusID;

		public ZPropertyInfo PortDuesPayingPartyAPPlusIDInfo => GetZPropertyInfo(nameof(PortDuesPayingPartyAPPlusID));

		#endregion

		#endregion

		#region Additional Instructions

		#region Notes

		public ZString Notes
		{
			get => notes;
			set
			{
				if (SetNonPersistentPropertyValue(NotesInfo, ref notes, value))
				{
					Validate(NotesInfo);
				}
			}
		}

		ZString notes;

		public ZPropertyInfo NotesInfo => GetZPropertyInfo(nameof(Notes));

		#endregion

		#endregion

	}
}
