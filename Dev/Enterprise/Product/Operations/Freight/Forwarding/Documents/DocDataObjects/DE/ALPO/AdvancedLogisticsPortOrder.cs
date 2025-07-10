using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE
{
	sealed class AdvancedLogisticsPortOrder : DocDataObject, IDataSourceProvider
	{
		public AdvancedLogisticsPortOrder(ZString sourceType, ZString sourceID)
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

		#region HeaderInformation

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

		#endregion BillOfLading

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

		#endregion CarrierBookingReference

		#region OperationalPort
		public IUnloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		IUnloco operationalPort;

		#endregion OperationalPort

		#region Direction
		public ZString Direction
		{
			get => direction;
			set
			{
				if (SetNonPersistentPropertyValue(DirectionInfo, ref direction, value))
				{
					Validate(DirectionInfo);
				}
			}
		}

		ZString direction;

		public ZPropertyInfo DirectionInfo => GetZPropertyInfo(nameof(Direction));

		#endregion Direction

		#region ContainerMode
		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		ICodeDescription containerMode;

		#endregion ContainerMode

		#region FreightForwarderReference
		public ZString FreightForwarderReference
		{
			get => freightForwarderReference;
			set
			{
				if (SetNonPersistentPropertyValue(FreightForwarderReferenceInfo, ref freightForwarderReference, value))
				{
					Validate(FreightForwarderReferenceInfo);
				}
			}
		}

		ZString freightForwarderReference;

		public ZPropertyInfo FreightForwarderReferenceInfo => GetZPropertyInfo(nameof(FreightForwarderReference));

		#endregion FreightForwarderReference

		#region PortOfLoading
		public IUnloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}

		IUnloco portOfLoading;

		#endregion PortOfLoading

		#region PortOfDischarge
		public IUnloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}

		IUnloco portOfDischarge;

		#endregion PortOfDischarge

		#region PortOfOrigin
		public IUnloco PortOfOrigin
		{
			get => portOfOrigin;
			set => portOfOrigin = SetChild(portOfOrigin, value);
		}

		IUnloco portOfOrigin;

		#endregion PortOfOrigin

		#region PortOfDestination
		public IUnloco PortOfDestination
		{
			get => portOfDestination;
			set => portOfDestination = SetChild(portOfDestination, value);
		}

		IUnloco portOfDestination;

		#endregion PortOfDestination

		#region Vessel
		public IVessel Vessel
		{
			get => vessel;
			set => vessel = SetChild(vessel, value);
		}

		IVessel vessel;

		#endregion Vessel

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

		#endregion VoyageFlightNo

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

		#endregion ETD

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

		#endregion ETA

		#region MarksAndNumbers
		public ZString MarksAndNumbers
		{
			get => marksAndNumbers;
			set
			{
				if (SetNonPersistentPropertyValue(MarksAndNumbersInfo, ref marksAndNumbers, value))
				{
					Validate(MarksAndNumbersInfo);
				}
			}
		}

		ZString marksAndNumbers;

		public ZPropertyInfo MarksAndNumbersInfo => GetZPropertyInfo(nameof(MarksAndNumbers));

		#endregion MarksAndNumbers

		#region TransportModePreCarriageOrOnForwarding

		public ICodeDescription TransportModePreCarriageOrOnForwarding
		{
			get => transportModePreCarriageOrOnForwarding;
			set => transportModePreCarriageOrOnForwarding = SetChild(transportModePreCarriageOrOnForwarding, value);
		}

		ICodeDescription transportModePreCarriageOrOnForwarding;

		#endregion

		#region PreCarriageOrOnForwardingID

		public ZString PreCarriageOrOnForwardingID
		{
			get => preCarriageOrOnforwardingID;
			set
			{
				if (SetNonPersistentPropertyValue(PreCarriageOrOnForwardingIDInfo, ref preCarriageOrOnforwardingID, value))
				{
					Validate(PreCarriageOrOnForwardingIDInfo);
				}
			}
		}

		ZString preCarriageOrOnforwardingID;

		public ZPropertyInfo PreCarriageOrOnForwardingIDInfo => GetZPropertyInfo(nameof(PreCarriageOrOnForwardingID));

		#endregion

		#region ALPOReference

		public ZString ALPOReference
		{
			get => aLPOReference;
			set
			{
				if (SetNonPersistentPropertyValue(ALPOReferenceInfo, ref aLPOReference, value))
				{
					Validate(ALPOReferenceInfo);
				}
			}
		}

		ZString aLPOReference;

		public ZPropertyInfo ALPOReferenceInfo => GetZPropertyInfo(nameof(ALPOReference));

		#endregion

		#region ALPOUserID

		public ZString ALPOUserID
		{
			get => aLPOUserID;
			set
			{
				if (SetNonPersistentPropertyValue(ALPOUserIDInfo, ref aLPOUserID, value))
				{
					Validate(ALPOUserIDInfo);
				}
			}
		}

		ZString aLPOUserID;

		public ZPropertyInfo ALPOUserIDInfo => GetZPropertyInfo(nameof(ALPOUserID));

		#endregion

		#region SisNumber

		public const string SisNumberReferenceUXmlName = "SIS_Number";

		public ZString SisNumber
		{
			get => sisNumber;
			set
			{
				if (SetNonPersistentPropertyValue(SisNumberInfo, ref sisNumber, value))
				{
					Validate(SisNumberInfo);
				}
			}
		}

		ZString sisNumber;

		public ZPropertyInfo SisNumberInfo => GetZPropertyInfo(nameof(SisNumber));

		#endregion

		#endregion HeaderInformation

		#region Parties

		#region EoriNumber

		public RegistrationNumber EoriNumber
		{
			get => eoriNumber;
			set => eoriNumber = SetChild(eoriNumber, value);
		}

		RegistrationNumber eoriNumber;

		#endregion

		#region EoriBranchSuffix

		public RegistrationNumber EoriBranchSuffix
		{
			get => eoriBranchSuffix;
			set => eoriBranchSuffix = SetChild(eoriBranchSuffix, value);
		}

		RegistrationNumber eoriBranchSuffix;

		#endregion

		#region Carrier
		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		Address carrier;

		#endregion Carrier

		#region CarrierCode
		public RegistrationNumber CarrierCode
		{
			get => carrierCode;
			set => carrierCode = SetChild(carrierCode, value);
		}

		RegistrationNumber carrierCode;

		#endregion CarrierCode

		#region SendingParty
		public Address SendingParty
		{
			get => sendingParty;
			set => sendingParty = SetChild(sendingParty, value);
		}

		Address sendingParty;

		#endregion SendingParty

		#region Forwarder

		public Address Forwarder
		{
			get => forwarder;
			set => forwarder = SetChild(forwarder, value);
		}

		Address forwarder;

		#endregion Forwarder

		#region CTO

		public Address CTO
		{
			get => ctoAddress;
			set => ctoAddress = SetChild(ctoAddress, value);
		}

		Address ctoAddress;

		#endregion

		#region CTOWarehouseCode
		public RegistrationNumber CTOWarehouseCode
		{
			get => ctoWarehouseCode;
			set => ctoWarehouseCode = SetChild(ctoWarehouseCode, value);
		}

		RegistrationNumber ctoWarehouseCode;

		#endregion

		#endregion Parties

		#region Goods and Equipment Details

		#region Containers

		public IReadOnlyCollection<Container> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}

		IReadOnlyCollection<Container> containers;

		#endregion

		#region Shipments

		public IReadOnlyCollection<Shipment> Shipments
		{
			get => shipments;
			set => shipments = SetChildCollection(shipments, value);
		}

		IReadOnlyCollection<Shipment> shipments;

		#endregion

		#endregion

	}
}
