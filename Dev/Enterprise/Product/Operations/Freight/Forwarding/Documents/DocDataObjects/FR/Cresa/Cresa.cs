using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class Cresa : DocDataObject, IDataSourceProvider
	{
		public Cresa(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

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

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion

		#region AdditionalReferences

		public ZString ECVReference
		{
			get => ecvReference;
			set
			{
				if (SetNonPersistentPropertyValue(ECVReferenceInfo, ref ecvReference, value))
				{
					Validate(ECVReferenceInfo);
				}
			}
		}
		ZString ecvReference;

		public ZPropertyInfo ECVReferenceInfo => GetZPropertyInfo(nameof(ECVReference));

		public ZString CRESAReference
		{
			get => cresaReference;
			set
			{
				if (SetNonPersistentPropertyValue(CRESAReferenceInfo, ref cresaReference, value))
				{
					Validate(CRESAReferenceInfo);
				}
			}
		}
		ZString cresaReference;

		public ZPropertyInfo CRESAReferenceInfo => GetZPropertyInfo(nameof(CRESAReference));

		#endregion

		#region Header Details - Addresses

		#region SendingForwarder

		public Address SendingForwarder
		{
			get => sendingForwarder;
			set => sendingForwarder = SetChild(sendingForwarder, value);
		}
		Address sendingForwarder;

		#endregion

		#region SendingForwarderSON

		public RegistrationNumber SendingForwarderSON
		{
			get => sendingForwarderSON;
			set => sendingForwarderSON = SetChild(sendingForwarderSON, value);
		}
		RegistrationNumber sendingForwarderSON;

		#endregion

		#region SendingForwarderSOA

		public RegistrationNumber SendingForwarderSOA
		{
			get => sendingForwarderSOA;
			set => sendingForwarderSOA = SetChild(sendingForwarderSOA, value);
		}
		RegistrationNumber sendingForwarderSOA;

		#endregion

		#region SendingForwarderSOW

		public RegistrationNumber SendingForwarderSOW
		{
			get => sendingForwarderSOW;
			set => sendingForwarderSOW = SetChild(sendingForwarderSOW, value);
		}
		RegistrationNumber sendingForwarderSOW;

		#endregion

		#region SendingForwarderCI5

		public RegistrationNumber SendingForwarderCI5
		{
			get => sendingForwarderCI5;
			set => sendingForwarderCI5 = SetChild(sendingForwarderCI5, value);
		}
		RegistrationNumber sendingForwarderCI5;

		#endregion

		#region FormattedSendingForwarderProviderID

		public ZString FormattedSendingForwarderProviderID
		{
			get => formattedSendingForwarderProviderID;
			set
			{
				if (SetNonPersistentPropertyValue(FormattedSendingForwarderProviderIDInfo, ref formattedSendingForwarderProviderID, value))
				{
					Validate(FormattedSendingForwarderProviderIDInfo);
				}
			}
		}
		ZString formattedSendingForwarderProviderID;

		public ZPropertyInfo FormattedSendingForwarderProviderIDInfo => GetZPropertyInfo(nameof(FormattedSendingForwarderProviderID));

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

		#region SendingPartySOA

		public RegistrationNumber SendingPartySOA
		{
			get => sendingPartySOA;
			set => sendingPartySOA = SetChild(sendingPartySOA, value);
		}

		RegistrationNumber sendingPartySOA;

		#endregion

		#region SendingPartySOW

		public RegistrationNumber SendingPartySOW
		{
			get => sendingPartySOW;
			set => sendingPartySOW = SetChild(sendingPartySOW, value);
		}

		RegistrationNumber sendingPartySOW;

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

		#region Agent

		public Address Agent
		{
			get => agent;
			set => agent = SetChild(agent, value);
		}

		Address agent;

		#endregion Agent

		#region AgentSON

		public RegistrationNumber AgentSON
		{
			get => agentSON;
			set => agentSON = SetChild(agentSON, value);
		}
		RegistrationNumber agentSON;

		#endregion

		#region AgentSOA

		public RegistrationNumber AgentSOA
		{
			get => agentSOA;
			set => agentSOA = SetChild(agentSOA, value);
		}
		RegistrationNumber agentSOA;

		#endregion

		#region AgentSOW

		public RegistrationNumber AgentSOW
		{
			get => agentSOW;
			set => agentSOW = SetChild(agentSOW, value);
		}
		RegistrationNumber agentSOW;

		#endregion

		#region AgentCI5

		public RegistrationNumber AgentCI5
		{
			get => agentCI5;
			set => agentCI5 = SetChild(agentCI5, value);
		}
		RegistrationNumber agentCI5;

		#endregion

		#region FormattedAgentProviderID

		public ZString FormattedAgentProviderID
		{
			get => formattedAgentProviderID;
			set
			{
				if (SetNonPersistentPropertyValue(FormattedAgentProviderIDInfo, ref formattedAgentProviderID, value))
				{
					Validate(FormattedAgentProviderIDInfo);
				}
			}
		}
		ZString formattedAgentProviderID;

		public ZPropertyInfo FormattedAgentProviderIDInfo => GetZPropertyInfo(nameof(FormattedAgentProviderID));

		#endregion

		#region Transporter

		public IAddress Transporter
		{
			get => transporter;
			set => transporter = SetChild(transporter, value);
		}

		IAddress transporter;

		#endregion Transporter

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

		#region Buyer

		public IAddress Buyer
		{
			get => buyer;
			set => buyer = SetChild(buyer, value);
		}

		IAddress buyer;

		#endregion Buyer

		#region BuyerSON

		public RegistrationNumber BuyerSON
		{
			get => buyerSON;
			set => buyerSON = SetChild(buyerSON, value);
		}
		RegistrationNumber buyerSON;

		#endregion

		#region BuyerCI5

		public RegistrationNumber BuyerCI5
		{
			get => buyerCI5;
			set => buyerCI5 = SetChild(buyerCI5, value);
		}
		RegistrationNumber buyerCI5;

		#endregion

		#region FormattedBuyerProviderID

		public ZString FormattedBuyerProviderID
		{
			get => formattedBuyerProviderID;
			set
			{
				if (SetNonPersistentPropertyValue(FormattedBuyerProviderIDInfo, ref formattedBuyerProviderID, value))
				{
					Validate(FormattedBuyerProviderIDInfo);
				}
			}
		}
		ZString formattedBuyerProviderID;

		public ZPropertyInfo FormattedBuyerProviderIDInfo => GetZPropertyInfo(nameof(FormattedBuyerProviderID));

		#endregion

		#region Supplier

		public IAddress Supplier
		{
			get => supplier;
			set => supplier = SetChild(supplier, value);
		}

		IAddress supplier;

		#endregion Supplier

		#region SupplierSON

		public RegistrationNumber SupplierSON
		{
			get => supplierSON;
			set => supplierSON = SetChild(supplierSON, value);
		}
		RegistrationNumber supplierSON;

		#endregion

		#region SupplierCI5

		public RegistrationNumber SupplierCI5
		{
			get => supplierCI5;
			set => supplierCI5 = SetChild(supplierCI5, value);
		}
		RegistrationNumber supplierCI5;

		#endregion

		#region FormattedSupplierProviderID

		public ZString FormattedSupplierProviderID
		{
			get => formattedSupplierProviderID;
			set
			{
				if (SetNonPersistentPropertyValue(FormattedSupplierProviderIDInfo, ref formattedSupplierProviderID, value))
				{
					Validate(FormattedSupplierProviderIDInfo);
				}
			}
		}
		ZString formattedSupplierProviderID;

		public ZPropertyInfo FormattedSupplierProviderIDInfo => GetZPropertyInfo(nameof(FormattedSupplierProviderID));

		#endregion

		#endregion

		#region Header Details - Transport

		#region TransporterID

		public ZString TransporterID
		{
			get => transporterID;
			set
			{
				if (SetNonPersistentPropertyValue(TransporterIDInfo, ref transporterID, value))
				{
					Validate(TransporterIDInfo);
				}
			}
		}
		ZString transporterID;

		public ZPropertyInfo TransporterIDInfo => GetZPropertyInfo(nameof(TransporterID));

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

		#region PortOfTranshipment

		public IUnloco PortOfTranshipment
		{
			get => portOfTranshipment;
			set => portOfTranshipment = SetChild(portOfTranshipment, value);
		}

		IUnloco portOfTranshipment;

		#endregion

		#region PortOfArrival

		public IUnloco PortOfArrival
		{
			get => portOfArrival;
			set => portOfArrival = SetChild(portOfArrival, value);
		}

		IUnloco portOfArrival;

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

		#region Header Details - Port Service Information

		#region PortServiceCodeReference

		public ZString PortServiceCodeReference
		{
			get => portServiceCodeReference;
			set
			{
				if (SetNonPersistentPropertyValue(PortServiceCodeReferenceInfo, ref portServiceCodeReference, value))
				{
					Validate(PortServiceCodeReferenceInfo);
				}
			}
		}
		ZString portServiceCodeReference;

		public ZPropertyInfo PortServiceCodeReferenceInfo => GetZPropertyInfo(nameof(PortServiceCodeReference));

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

		#endregion AreaWithinLocationAPPlusCode

		#region PortLocation within Area

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

		#endregion PortLocation

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

		#region AMQReference

		public ZString AMQReference
		{
			get => amqReference;
			set
			{
				if (SetNonPersistentPropertyValue(AMQReferenceInfo, ref amqReference, value))
				{
					Validate(AMQReferenceInfo);
				}
			}
		}
		ZString amqReference;

		public ZPropertyInfo AMQReferenceInfo => GetZPropertyInfo(nameof(AMQReference));

		#endregion

		#region EntryNumber

		public ZString EntryNumber
		{
			get => entryNumber;
			set
			{
				if (SetNonPersistentPropertyValue(EntryNumberInfo, ref entryNumber, value))
				{
					Validate(EntryNumberInfo);
				}
			}
		}
		ZString entryNumber;

		public ZPropertyInfo EntryNumberInfo => GetZPropertyInfo(nameof(EntryNumber));

		#endregion

		#endregion

		#region Shipment Details

		#region ShipmentNumber

		public ZString ShipmentNumber
		{
			get => shipmentNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ShipmentNumberInfo, ref shipmentNumber, value))
				{
					Validate(ShipmentNumberInfo);
				}
			}
		}
		ZString shipmentNumber;

		public ZPropertyInfo ShipmentNumberInfo => GetZPropertyInfo(nameof(ShipmentNumber));

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

		#region Cargo Receipt Date

		public ZDateTime CargoReceiptDate
		{
			get => cargoReceiptDate;
			set
			{
				if (SetNonPersistentPropertyValue(CargoReceiptDateInfo, ref cargoReceiptDate, value))
				{
					Validate(CargoReceiptDateInfo);
				}
			}
		}
		ZDateTime cargoReceiptDate;

		public ZPropertyInfo CargoReceiptDateInfo => GetZPropertyInfo(nameof(CargoReceiptDate));

		#endregion

		#region Good Details

		#region CommodityReference

		public ZString CommodityReference
		{
			get => commodityReference;
			set
			{
				if (SetNonPersistentPropertyValue(CommodityReferenceInfo, ref commodityReference, value))
				{
					Validate(CommodityReferenceInfo);
				}
			}
		}
		ZString commodityReference;

		public ZPropertyInfo CommodityReferenceInfo => GetZPropertyInfo(nameof(CommodityReference));

		#endregion

		#endregion

		#region GoodsInDateTime

		public ZDateTime GoodsInDateTime
		{
			get => goodsInDateTime;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsInDateTimeInfo, ref goodsInDateTime, value))
				{
					Validate(GoodsInDateTimeInfo);
				}
			}
		}

		ZDateTime goodsInDateTime;

		public ZPropertyInfo GoodsInDateTimeInfo => GetZPropertyInfo(nameof(GoodsInDateTime));

		#endregion

		#region GoodsSealed

		public ZBool GoodsSealed
		{
			get => goodsSealed;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsSealedInfo, ref goodsSealed, value))
				{
					Validate(GoodsSealedInfo);
				}
			}
		}

		ZBool goodsSealed;

		public ZPropertyInfo GoodsSealedInfo => GetZPropertyInfo(nameof(GoodsSealed));

		#endregion

		#region TotalPackCount

		public ZInt TotalPackCount
		{
			get => totalPackCount;
			set
			{
				if (SetNonPersistentPropertyValue(TotalPackCountInfo, ref totalPackCount, value))
				{
				}
			}
		}

		ZInt totalPackCount;

		public ZPropertyInfo TotalPackCountInfo => GetZPropertyInfo(nameof(TotalPackCount));

		#endregion

		#region PackType

		public ICodeDescription PackType
		{
			get => packType;
			set => packType = SetChild(packType, value);
		}
		ICodeDescription packType;

		#endregion

		#region TotalWeight

		public IMeasurement TotalWeight
		{
			get => totalWeight;
			set => totalWeight = SetChild(totalWeight, value);
		}
		IMeasurement totalWeight;

		#endregion

		#region TotalVolume

		public IMeasurement TotalVolume
		{
			get => totalVolume;
			set => totalVolume = SetChild(totalVolume, value);
		}
		IMeasurement totalVolume;

		#endregion

		#region GoodsDescription

		public ZString GoodsDescription
		{
			get => goodsDescription;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsDescriptionInfo, ref goodsDescription, value))
				{
					Validate(GoodsDescriptionInfo);
				}
			}
		}
		ZString goodsDescription;

		public ZPropertyInfo GoodsDescriptionInfo => GetZPropertyInfo(nameof(GoodsDescription));

		#endregion

		#region PackingLines

		public IReadOnlyCollection<BookingPackingLine> PackingLines
		{
			get => packingLines;
			set => packingLines = SetChildCollection(packingLines, value);
		}

		IReadOnlyCollection<BookingPackingLine> packingLines;

		#endregion

		#region Additional Instructions

		#region GoodsReceiptNotes

		public ZString GoodsReceiptNotes
		{
			get => goodsReceiptNotes;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsReceiptNotesInfo, ref goodsReceiptNotes, value))
				{
					Validate(GoodsReceiptNotesInfo);
				}
			}
		}
		ZString goodsReceiptNotes;

		public ZPropertyInfo GoodsReceiptNotesInfo => GetZPropertyInfo(nameof(GoodsReceiptNotes));

		#endregion

		#endregion

		#region DataObjectWriter Fields

		#region CurrentUser

		public IAddress CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}

		IAddress currentUser;

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

		#region ContainerMode

		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		ICodeDescription containerMode;

		#endregion

		#endregion

		#region Cresa Note for Transit Warehouse

		public ZString NoteForTransitWarehouse { get; set; } = ZString.Empty;

		#endregion
	}
}
