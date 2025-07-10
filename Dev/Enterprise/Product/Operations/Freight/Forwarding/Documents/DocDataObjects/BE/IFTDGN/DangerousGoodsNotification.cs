using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	sealed class DangerousGoodsNotification : DocDataObject, IDataSourceProvider
	{
		public DangerousGoodsNotification(ZString sourceType, ZString sourceID)
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

		#region Header Information

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

		#endregion ConsolNumber

		#region BillOfLading

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

		#region DgnSecurityNumber

		public ZString DgnSecurityNumber
		{
			get => dgnSecurityNumber;
			set
			{
				if (SetNonPersistentPropertyValue(DgnSecurityNumberInfo, ref dgnSecurityNumber, value))
				{
					Validate(DgnSecurityNumberInfo);
				}
			}
		}

		ZString dgnSecurityNumber;

		public ZPropertyInfo DgnSecurityNumberInfo => GetZPropertyInfo(nameof(DgnSecurityNumber));

		#endregion DgnSecurityNumber

		#endregion

		#region Addresses

		#region Carrier

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		Address carrier;

		#endregion

		#region CarrierPortId

		public RegistrationNumber CarrierPortId
		{
			get => carrierPortId;
			set => carrierPortId = SetChild(carrierPortId, value);
		}

		RegistrationNumber carrierPortId;

		#endregion

		#region SendingParty

		public Address SendingParty
		{
			get => sendingParty;
			set => sendingParty = SetChild(sendingParty, value);
		}

		Address sendingParty;

		#endregion SendingParty

		#region SendingPartyPortId

		public RegistrationNumber SendingPartyPortId
		{
			get => sendingPartyPortId;
			set => sendingPartyPortId = SetChild(sendingPartyPortId, value);
		}

		RegistrationNumber sendingPartyPortId;

		#endregion

		#region SendingParty
		public RegistrationNumber SendingPartyEori
		{
			get => sendingPartyEori;
			set => sendingPartyEori = SetChild(sendingPartyEori, value);
		}

		RegistrationNumber sendingPartyEori;

		public RegistrationNumber SendingPartyDuns
		{
			get => sendingPartyDuns;
			set => sendingPartyDuns = SetChild(sendingPartyDuns, value);
		}

		RegistrationNumber sendingPartyDuns;

		#endregion SendingParty

		#region SendingForwarder

		public Address SendingForwarder
		{
			get => sendingForwarder;
			set => sendingForwarder = SetChild(sendingForwarder, value);
		}

		Address sendingForwarder;

		#endregion

		#region SendingForwarderPortId

		public RegistrationNumber SendingForwarderPortId
		{
			get => sendingForwarderPortId;
			set => sendingForwarderPortId = SetChild(sendingForwarderPortId, value);
		}

		RegistrationNumber sendingForwarderPortId;

		#endregion

		#region ReceivingForwarder

		public Address ReceivingForwarder
		{
			get => receivingForwarder;
			set => receivingForwarder = SetChild(receivingForwarder, value);
		}

		Address receivingForwarder;

		#endregion

		#region ReceivingForwarderPortId

		public RegistrationNumber ReceivingForwarderPortId
		{
			get => receivingForwarderPortId;
			set => receivingForwarderPortId = SetChild(receivingForwarderPortId, value);
		}

		RegistrationNumber receivingForwarderPortId;

		#endregion

		#region ArrivalCTO

		public Address ArrivalCTO
		{
			get => arrivalCTO;
			set => arrivalCTO = SetChild(arrivalCTO, value);
		}

		Address arrivalCTO;

		#endregion

		#region ArrivalCTOTerminalId

		public RegistrationNumber ArrivalCTOTerminalId
		{
			get => arrivalCTOTerminalId;
			set => arrivalCTOTerminalId = SetChild(arrivalCTOTerminalId, value);
		}

		RegistrationNumber arrivalCTOTerminalId;

		#endregion

		#region DepartureCTO

		public Address DepartureCTO
		{
			get => departureCTO;
			set => departureCTO = SetChild(departureCTO, value);
		}

		Address departureCTO;

		#endregion

		#region DepartureCTOTerminalId

		public RegistrationNumber DepartureCTOTerminalId
		{
			get => departureCTOTerminalId;
			set => departureCTOTerminalId = SetChild(departureCTOTerminalId, value);
		}

		RegistrationNumber departureCTOTerminalId;
		#endregion

		#endregion

		#region VesselInformation

		#region VesselStayReference

		public ZString VesselStayReference
		{
			get => vesselStayReference;
			set
			{
				if (SetNonPersistentPropertyValue(VesselStayReferenceInfo, ref vesselStayReference, value))
				{
					Validate(VesselStayReferenceInfo);
				}
			}
		}

		ZString vesselStayReference;

		public ZPropertyInfo VesselStayReferenceInfo => GetZPropertyInfo(nameof(VesselStayReference));

		#endregion

		#region VesselStayStartDate

		public ZDateTime VesselStayStartDate
		{
			get => vesselStayStartDate;
			set
			{
				if (SetNonPersistentPropertyValue(VesselStayStartDateInfo, ref vesselStayStartDate, value))
				{
					Validate(VesselStayStartDateInfo);
				}
			}
		}
		ZDateTime vesselStayStartDate;

		public ZPropertyInfo VesselStayStartDateInfo => GetZPropertyInfo(nameof(VesselStayStartDate));

		#endregion

		#region VesselStayEndDate

		public ZDateTime VesselStayEndDate
		{
			get => vesselStayEndDate;
			set
			{
				if (SetNonPersistentPropertyValue(VesselStayEndDateInfo, ref vesselStayEndDate, value))
				{
					Validate(VesselStayEndDateInfo);
				}
			}
		}
		ZDateTime vesselStayEndDate;

		public ZPropertyInfo VesselStayEndDateInfo => GetZPropertyInfo(nameof(VesselStayEndDate));

		#endregion

		#endregion

		#region RoutingInformation

		#region HandlingInstruction

		public ZString HandlingInstruction
		{
			get => handlingInstruction;
			set
			{
				if (SetNonPersistentPropertyValue(HandlingInstructionInfo, ref handlingInstruction, value))
				{
					Validate(HandlingInstructionInfo);
				}
			}
		}

		ZString handlingInstruction;

		public ZPropertyInfo HandlingInstructionInfo => GetZPropertyInfo(nameof(HandlingInstruction));

		#endregion

		#region HandlingDate

		public ZDateTime HandlingDate
		{
			get => handlingDate;
			set
			{
				if (SetNonPersistentPropertyValue(HandlingDateInfo, ref handlingDate, value))
				{
					Validate(HandlingDateInfo);
				}
			}
		}
		ZDateTime handlingDate;

		public ZPropertyInfo HandlingDateInfo => GetZPropertyInfo(nameof(HandlingDate));

		#endregion

		#region OperationalPort

		public IUnloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		IUnloco operationalPort;

		#endregion

		#region PreOrOnTransportMode
		public ICodeDescription PreOrOnTransportMode
		{
			get => preOrOnTransportMode;
			set => preOrOnTransportMode = SetChild(preOrOnTransportMode, value);
		}

		ICodeDescription preOrOnTransportMode;

		#endregion PreOrOnTransportMode

		#region PreOrOnVesselName
		public ZString PreOrOnVesselName
		{
			get => preOrOnVesselName;
			set
			{
				if (SetNonPersistentPropertyValue(PreOrOnVesselNameInfo, ref preOrOnVesselName, value))
				{
					Validate(PreOrOnVesselNameInfo);
				}
			}
		}

		ZString preOrOnVesselName;

		public ZPropertyInfo PreOrOnVesselNameInfo => GetZPropertyInfo(nameof(PreOrOnVesselName));

		#endregion PreOrOnVesselName

		#region PreOrOnVesselENINumber
		public ZString PreOrOnVesselENINumber
		{
			get => preOrOnVesselENINumber;
			set
			{
				if (SetNonPersistentPropertyValue(PreOrOnVesselENINumberInfo, ref preOrOnVesselENINumber, value))
				{
					Validate(PreOrOnVesselENINumberInfo);
				}
			}
		}

		ZString preOrOnVesselENINumber;

		public ZPropertyInfo PreOrOnVesselENINumberInfo => GetZPropertyInfo(nameof(PreOrOnVesselENINumber));

		#endregion PreOrOnVesselENINumber

		#region DeliveryDate
		public ZDateTime DeliveryDate
		{
			get => deliveryDate;
			set
			{
				if (SetNonPersistentPropertyValue(DeliveryDateInfo, ref deliveryDate, value))
				{
					Validate(DeliveryDateInfo);
				}
			}
		}

		ZDateTime deliveryDate;

		public ZPropertyInfo DeliveryDateInfo => GetZPropertyInfo(nameof(DeliveryDate));

		#endregion DeliveryDate

		#region PickupDate
		public ZDateTime PickupDate
		{
			get => pickupDate;
			set
			{
				if (SetNonPersistentPropertyValue(PickupDateInfo, ref pickupDate, value))
				{
					Validate(PickupDateInfo);
				}
			}
		}

		ZDateTime pickupDate;

		public ZPropertyInfo PickupDateInfo => GetZPropertyInfo(nameof(PickupDate));

		#endregion PickupDate

		#region Transports

		public Transports Transports
		{
			get => transports;
			set => transports = SetChild(transports, value);
		}

		Transports transports;

		#endregion

		#endregion

		#region Goods and Equipment Details

		#region PackingLines

		public IReadOnlyCollection<DGNPackingLine> PackingLines
		{
			get => packingLines;
			set => packingLines = SetChildCollection(packingLines, value);
		}

		IReadOnlyCollection<DGNPackingLine> packingLines;

		#endregion

		#region Containers

		public IReadOnlyCollection<DGNContainer> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}

		IReadOnlyCollection<DGNContainer> containers;

		#endregion

		#endregion

		#region DataObjectWriter Fields

		#region ContainerMode

		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		ICodeDescription containerMode;

		#endregion

		#region ShipmentType

		public ICodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(shipmentType, value);
		}

		ICodeDescription shipmentType;

		#endregion

		#endregion

	}
}
