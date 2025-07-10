using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class ImportManifest : DocDataObject, IDataSourceProvider
	{
		public ImportManifest(ZString sourceType, ZString sourceID)
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

		#region SendingParty

		public Address SendingParty
		{
			get => sendingParty;
			set => sendingParty = SetChild(sendingParty, value);
		}

		Address sendingParty;

		#endregion SendingParty

		#region SendingPartySON

		public RegistrationNumber SendingPartySON
		{
			get => sendingPartySON;
			set => sendingPartySON = SetChild(sendingPartySON, value);
		}

		RegistrationNumber sendingPartySON;

		#endregion

		#region SendingPartyCI5

		public RegistrationNumber SendingPartyCI5
		{
			get => sendingPartyCI5;
			set => sendingPartyCI5 = SetChild(sendingPartyCI5, value);
		}

		RegistrationNumber sendingPartyCI5;

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

		#region OTCReference

		public ZString OTCReference
		{
			get => otcReference;
			set
			{
				if (SetNonPersistentPropertyValue(OTCReferenceInfo, ref otcReference, value))
				{
					Validate(OTCReferenceInfo);
					Validate(ATPReferenceInfo);
				}
			}
		}
		ZString otcReference;

		public ZPropertyInfo OTCReferenceInfo => GetZPropertyInfo(nameof(OTCReference));

		#endregion

		#region ATPReference

		public ZString ATPReference
		{
			get => atpReference;
			set
			{
				if (SetNonPersistentPropertyValue(ATPReferenceInfo, ref atpReference, value))
				{
					Validate(OTCReferenceInfo);
					Validate(ATPReferenceInfo);
				}
			}
		}
		ZString atpReference;

		public ZPropertyInfo ATPReferenceInfo => GetZPropertyInfo(nameof(ATPReference));

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

		#region TransporterSON

		public RegistrationNumber TransporterSON
		{
			get => transporterSON;
			set => transporterSON = SetChild(transporterSON, value);
		}

		RegistrationNumber transporterSON;

		#endregion

		#region TransporterCI5

		public RegistrationNumber TransporterCI5
		{
			get => transporterCI5;
			set => transporterCI5 = SetChild(transporterCI5, value);
		}

		RegistrationNumber transporterCI5;

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

		#region SendingForwarderSON

		public RegistrationNumber SendingForwarderSON
		{
			get => sendingForwarderSON;
			set => sendingForwarderSON = SetChild(sendingForwarderSON, value);
		}

		RegistrationNumber sendingForwarderSON;

		#endregion

		#region SendingForwarderCI5

		public RegistrationNumber SendingForwarderCI5
		{
			get => sendingForwarderCI5;
			set => sendingForwarderCI5 = SetChild(sendingForwarderCI5, value);
		}

		RegistrationNumber sendingForwarderCI5;

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

		#region FormattedReceivingForwarderProviderID

		public ZString FormattedReceivingForwarderProviderID
		{
			get => formattedReceivingForwarderProviderID;
			set
			{
				if (SetNonPersistentPropertyValue(FormattedReceivingForwarderProviderIDInfo, ref formattedReceivingForwarderProviderID, value))
				{
					Validate(FormattedReceivingForwarderProviderIDInfo);
				}
			}
		}
		ZString formattedReceivingForwarderProviderID;

		public ZPropertyInfo FormattedReceivingForwarderProviderIDInfo => GetZPropertyInfo(nameof(FormattedReceivingForwarderProviderID));

		#endregion

		#region ContainerMode

		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		ICodeDescription containerMode;

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

		#region RequiresTemperatureControl

		public ZBool RequiresTemperatureControl
		{
			get => requiresTemperatureControl;
			set
			{
				if (SetNonPersistentPropertyValue(RequiresTemperatureControlInfo, ref requiresTemperatureControl, value))
				{
					Validate(RequiresTemperatureControlInfo);
				}
			}
		}

		ZBool requiresTemperatureControl;

		public ZPropertyInfo RequiresTemperatureControlInfo => GetZPropertyInfo(nameof(RequiresTemperatureControl));

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

		#region ShipmentType

		public ICodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(shipmentType, value);
		}

		ICodeDescription shipmentType;

		#endregion

		#region SendingForwarder

		public Address SendingForwarder
		{
			get => sendingForwarder;
			set => sendingForwarder = SetChild(sendingForwarder, value);
		}

		Address sendingForwarder;

		#endregion

		#region ReceivingForwarder

		public Address ReceivingForwarder
		{
			get => receivingForwarder;
			set => receivingForwarder = SetChild(receivingForwarder, value);
		}

		Address receivingForwarder;

		#endregion

		#region BillOfLadingNumber

		public ZString BillOfLadingNumber
		{
			get => billOfLadingNumber;
			set
			{
				if (SetNonPersistentPropertyValue(BillOfLadingNumberInfo, ref billOfLadingNumber, value))
				{
				}
			}
		}

		ZString billOfLadingNumber;

		public ZPropertyInfo BillOfLadingNumberInfo => GetZPropertyInfo(nameof(BillOfLadingNumber));

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

		#region Carrier

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		Address carrier;

		#endregion

		#region CarrierSON

		public RegistrationNumber CarrierSON
		{
			get => carrierSON;
			set => carrierSON = SetChild(carrierSON, value);
		}

		RegistrationNumber carrierSON;

		#endregion

		#region CarrierCI5

		public RegistrationNumber CarrierCI5
		{
			get => carrierCI5;
			set => carrierCI5 = SetChild(carrierCI5, value);
		}

		RegistrationNumber carrierCI5;

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
	}
}
