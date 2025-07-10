using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CPT
{
	sealed class CartaDePorte : DocDataObject, IDataSourceProvider, ICustomFieldProviderProxy
	{
		public CartaDePorte(ZString sourceType, ZString sourceID, CustomBusinessObject customBusinessObject = null)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
			this.customBusinessObject = customBusinessObject;
		}

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion

		#region ICustomFieldProviderProxy members

		CustomBusinessObject ICustomFieldProviderProxy.CustomBusinessObject => customBusinessObject;
		readonly CustomBusinessObject customBusinessObject;

		#endregion

		#region NumberOfCopies

		public ZInt NumberOfCopies
		{
			get => numberOfCopies;
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfCopiesInfo, ref numberOfCopies, value))
				{
				}
			}
		}

		ZInt numberOfCopies;

		public ZPropertyInfo NumberOfCopiesInfo => GetZPropertyInfo(nameof(NumberOfCopies));

		#endregion

		#region NumberOfOriginals

		public ZInt NumberOfOriginals
		{
			get => numberOfOriginals;
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfOriginalsInfo, ref numberOfOriginals, value))
				{
				}
			}
		}

		ZInt numberOfOriginals;

		public ZPropertyInfo NumberOfOriginalsInfo => GetZPropertyInfo(nameof(NumberOfOriginals));

		#endregion

		#region HouseBillNumber

		public ZString HouseBillNumber
		{
			get => houseBillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(HouseBillNumberInfo, ref houseBillNumber, value))
				{
				}
			}
		}

		ZString houseBillNumber;

		public ZPropertyInfo HouseBillNumberInfo => GetZPropertyInfo(nameof(HouseBillNumber));

		#endregion

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

		#region ShipmentNumber

		public ZString ShipmentNumber
		{
			get => shipmentNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ShipmentNumberInfo, ref shipmentNumber, value))
				{
				}
			}
		}

		ZString shipmentNumber;

		public ZPropertyInfo ShipmentNumberInfo => GetZPropertyInfo(nameof(ShipmentNumber));

		#endregion

		#region IsOriginal

		public ZBool IsOriginal
		{
			get => isOriginal;
			set
			{
				if (SetNonPersistentPropertyValue(IsOriginalInfo, ref isOriginal, value))
				{
				}
			}
		}

		ZBool isOriginal;

		public ZPropertyInfo IsOriginalInfo => GetZPropertyInfo(nameof(IsOriginal));

		#endregion

		#region DocumentType

		public ZString DocumentType
		{
			get => documentType;
			set
			{
				if (SetNonPersistentPropertyValue(DocumentTypeInfo, ref documentType, value))
				{
				}
			}
		}

		ZString documentType;

		public ZPropertyInfo DocumentTypeInfo => GetZPropertyInfo(nameof(DocumentType));

		#endregion

		#region ShippersReference

		public ZString ShippersReference
		{
			get => shippersReference;
			set
			{
				if (SetNonPersistentPropertyValue(ShippersReferenceInfo, ref shippersReference, value))
				{
				}
			}
		}

		ZString shippersReference;

		public ZPropertyInfo ShippersReferenceInfo => GetZPropertyInfo(nameof(ShippersReference));

		#endregion

		#region OtherReference

		public ZString OtherReference
		{
			get => otherReference;
			set
			{
				if (SetNonPersistentPropertyValue(OtherReferenceInfo, ref otherReference, value))
				{
				}
			}
		}

		ZString otherReference;

		public ZPropertyInfo OtherReferenceInfo => GetZPropertyInfo(nameof(OtherReference));

		#endregion

		#region DateOfIssue

		public ZDateTime DateOfIssue
		{
			get => dateOfIssue;
			set
			{
				if (SetNonPersistentPropertyValue(DateOfIssueInfo, ref dateOfIssue, value))
				{
				}
			}
		}

		ZDateTime dateOfIssue;

		public ZPropertyInfo DateOfIssueInfo => GetZPropertyInfo(nameof(DateOfIssue));

		#endregion

		#region Logo

		public IHouseBillLogo Logo
		{
			get => logo;
			set => logo = SetChild(logo, value);
		}
		IHouseBillLogo logo;

		#endregion

		#region Terms and Conditions

		public IHouseBillTermsAndConditions TermsAndConditions
		{
			get => termsAndConditions;
			set => termsAndConditions = SetChild(termsAndConditions, value);
		}
		IHouseBillTermsAndConditions termsAndConditions;

		#endregion

		#region ForwardingAgent

		public IAddress ForwardingAgent
		{
			get => forwardingAgent;
			set => forwardingAgent = SetChild(forwardingAgent, value);
		}
		IAddress forwardingAgent;

		#endregion

		#region Consignor

		public IAddress Consignor
		{
			get => consignor;
			set => consignor = SetChild(consignor, value);
		}
		IAddress consignor;

		#endregion

		#region Consignee

		public IAddress Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}
		IAddress consignee;

		#endregion

		#region NotifyParty

		public IAddress NotifyParty
		{
			get => notifyParty;
			set => notifyParty = SetChild(notifyParty, value);
		}
		IAddress notifyParty;

		#endregion

		#region SendingForwarder

		public IAddress SendingForwarder
		{
			get => sendingForwarder;
			set => sendingForwarder = SetChild(sendingForwarder, value);
		}
		IAddress sendingForwarder;

		#endregion

		#region Issuer

		public IAddress Issuer
		{
			get => issuer;
			set => issuer = SetChild(issuer, value);
		}
		IAddress issuer;

		#endregion

		#region ExportBroker

		public IAddress ExportBroker
		{
			get => exportBroker;
			set => exportBroker = SetChild(exportBroker, value);
		}
		IAddress exportBroker;

		#endregion

		#region Port of Origin

		public IUnloco PortOfOrigin
		{
			get => portOfOrigin;
			set => portOfOrigin = SetChild(portOfOrigin, value);
		}
		IUnloco portOfOrigin;

		#endregion

		#region Port of Destination

		public IUnloco PortOfDestination
		{
			get => portOfDestination;
			set => portOfDestination = SetChild(portOfDestination, value);
		}
		IUnloco portOfDestination;

		#endregion

		#region Port of Loading

		public IUnloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}
		IUnloco portOfLoading;

		#endregion

		#region Port of Discharge

		public IUnloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}
		IUnloco portOfDischarge;

		#endregion

		#region CommercialInvoiceNumber

		public ZString CommercialInvoiceNumber
		{
			get => commercialInvoiceNumber;
			set
			{
				if (SetNonPersistentPropertyValue(CommercialInvoiceNumberInfo, ref commercialInvoiceNumber, value))
				{
				}
			}
		}

		ZString commercialInvoiceNumber;

		public ZPropertyInfo CommercialInvoiceNumberInfo => GetZPropertyInfo(nameof(CommercialInvoiceNumber));

		#endregion

		#region OrderReferences

		public IReadOnlyCollection<string> OrderReferences
		{
			get => orderReferences;
			set => orderReferences = SetChildCollection(orderReferences, value);
		}
		IReadOnlyCollection<string> orderReferences;

		#endregion

		#region Orders

		public IReadOnlyCollection<OrderDataObject> Orders
		{
			get => orders;
			set => orders = SetChildCollection(orders, value);
		}
		IReadOnlyCollection<OrderDataObject> orders;

		#endregion

		#region Driver

		public ZString Driver
		{
			get => driver;
			set
			{
				if (SetNonPersistentPropertyValue(DriverInfo, ref driver, value))
				{
				}
			}
		}

		ZString driver;

		public ZPropertyInfo DriverInfo => GetZPropertyInfo(nameof(Driver));

		#endregion

		#region VehicleType

		public ZString VehicleType
		{
			get => vehicleType;
			set
			{
				if (SetNonPersistentPropertyValue(VehicleTypeInfo, ref vehicleType, value))
				{
				}
			}
		}

		ZString vehicleType;

		public ZPropertyInfo VehicleTypeInfo => GetZPropertyInfo(nameof(VehicleType));

		#endregion

		#region VehicleRegNo

		public ZString VehicleRegNoTruck
		{
			get => vehicleRegNoTruck;
			set
			{
				if (SetNonPersistentPropertyValue(VehicleRegNoTruckInfo, ref vehicleRegNoTruck, value))
				{
				}
			}
		}

		ZString vehicleRegNoTruck;

		public ZPropertyInfo VehicleRegNoTruckInfo => GetZPropertyInfo(nameof(VehicleRegNoTruck));

		public ZString VehicleRegNoWagon
		{
			get => vehicleRegNoWagon;
			set
			{
				if (SetNonPersistentPropertyValue(VehicleRegNoWagonInfo, ref vehicleRegNoWagon, value))
				{
				}
			}
		}

		ZString vehicleRegNoWagon;

		public ZPropertyInfo VehicleRegNoWagonInfo => GetZPropertyInfo(nameof(VehicleRegNoWagon));

		#endregion

		#region Charges

		public IChargesCollection Charges
		{
			get => charges;
			set => charges = (IChargesCollection)SetChildCollection(charges, value);
		}
		IChargesCollection charges;

		#endregion

		#region Containers

		public IReadOnlyCollection<IContainer> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}
		IReadOnlyCollection<IContainer> containers;

		#endregion

		#region LoosePackingLines

		public IReadOnlyCollection<IPackingLine> LoosePackingLines
		{
			get => loosePackingLines;
			set => loosePackingLines = SetChildCollection(loosePackingLines, value);
		}
		IReadOnlyCollection<IPackingLine> loosePackingLines;

		#endregion

		#region Notes

		public IReadOnlyCollection<INote> Notes
		{
			get => notes;
			set => notes = SetChildCollection(notes, value);
		}
		IReadOnlyCollection<INote> notes;

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

		#region TotalPackType

		public ICodeDescription TotalPackType
		{
			get => totalPackType;
			set => totalPackType = SetChild(totalPackType, value);
		}
		ICodeDescription totalPackType;

		#endregion

		#region TotalPrepaid

		public ZDecimal TotalPrepaid
		{
			get => totalPrepaid;
			set
			{
				if (SetNonPersistentPropertyValue(TotalPrepaidInfo, ref totalPrepaid, value))
				{
				}
			}
		}

		ZDecimal totalPrepaid;

		public ZPropertyInfo TotalPrepaidInfo => GetZPropertyInfo(nameof(TotalPrepaid));

		#endregion TotalPrepaid

		#region TotalCollect

		public ZDecimal TotalCollect
		{
			get => totalCollect;
			set
			{
				if (SetNonPersistentPropertyValue(TotalCollectInfo, ref totalCollect, value))
				{
				}
			}
		}

		ZDecimal totalCollect;

		public ZPropertyInfo TotalCollectInfo => GetZPropertyInfo(nameof(TotalCollect));

		#endregion TotalCollect
	}
}
