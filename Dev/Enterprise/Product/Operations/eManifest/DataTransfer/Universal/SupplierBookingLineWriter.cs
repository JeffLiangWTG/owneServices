using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eManifest.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eManifest.DataTransfer.Universal
{
	public class SupplierBookingLineWriter : TopLevelDataObjectWriter<SupplierBookingLine, UniversalShipment>, IHierarchicalDataObjectWriter
	{
		public SupplierBookingLineWriter(IDataWritingManager manager)
			: base(manager)
		{
			IncludeParent = true;
			IncludeChildren = true;
		}

		public bool IncludeParent
		{
			get;
			set;
		}

		public bool IncludeChildren
		{
			get;
			set;
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.eManifestLine;
		}

		protected override void PopulateDataObject(SupplierBookingLine sourceBO, UniversalShipment dataObject)
		{
			dataObject.OwnerRef = sourceBO.BookingHeader.DH_SupplierReference;
			var listCache = BindToLists.GetCachedLists(sourceBO.Factory);

			dataObject.WayBillNumber = sourceBO.DL_ConsigneeReference;
			dataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList());
			dataObject.TotalVolume = sourceBO.DL_Cubic;
			dataObject.TotalVolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(sourceBO.DL_CubicUQ, listCache.VolumeUnits);
			dataObject.GoodsDescription = sourceBO.DL_GoodsDescription;
			dataObject.GoodsValue = sourceBO.DL_GoodsValue;
			dataObject.GoodsValueCurrency = ListHelper.GetWithDescription<Currency>(sourceBO.DL_RX_NKGoodsValueCurrency, listCache.RefCurrency_List);
			dataObject.PaymentMethod = ListHelper.GetWithDescription<UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair>(Core.Constants.PaymentType.Prepaid, sourceBO.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.PaymentType));
			dataObject.TotalWeight = sourceBO.DL_GrossWeight;
			dataObject.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(sourceBO.DL_GrossWeightUQ, listCache.WeightUnits);
			dataObject.TotalNoOfPieces = sourceBO.DL_PiecesManifested;
			dataObject.IsLastMileDeliverySelfBooked = sourceBO.DL_IsDeliveryTransportSelfBooked;
			dataObject.IsHazardous = sourceBO.DL_IsHazardous;
			dataObject.VendorIdentifier = sourceBO.DL_VendorIdentifier;

			if (!sourceBO.DL_RS_NKServiceLevel.IsEmpty)
			{
				var serviceLevel = new ServiceLevel();
				serviceLevel.Code = sourceBO.DL_RS_NKServiceLevel;

				var serviceLevelDescription = sourceBO.ServiceLevel;
				if (serviceLevelDescription != null)
				{
					serviceLevel.Description = sourceBO.ServiceLevel.RS_DescriptionMultilingual;
				}

				dataObject.ServiceLevel = serviceLevel;
			}

			if (sourceBO.Shipment != null)
			{
				dataObject.PortOfOrigin = ListHelper.GetWithName(sourceBO.Shipment.JS_RL_NKOrigin, listCache.RefUNLOCO_List);
				dataObject.PortOfDestination = ListHelper.GetWithName(sourceBO.Shipment.JS_RL_NKDestination, listCache.RefUNLOCO_List);
			}

			if (!sourceBO.DL_MarksAndNumbers.IsEmpty)
			{
				dataObject.SetNoteCollection(() => new DataObjectList<Note>(new[] {
					new Note()
					{
						Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description,
						IsCustomDescription = false,
						NoteText = sourceBO.DL_MarksAndNumbers,
					} })
				{ Content = CollectionContent.Partial });
			}

			AddConsignorAddress(sourceBO, dataObject);
			AddConsigneeAddress(sourceBO, dataObject);

			dataObject.AddOrgAddress(writeManager, sourceBO.BookingHeader.Consignor, DocAddressType.ConsignorDocumentaryAddress, sourceBO.BookingHeader.ConsignorContact);

			if (sourceBO.BookingHeader.Consignor.Header != null && sourceBO.BookingHeader.Consignor.Header.AllRelatedParties != null)
			{
				var returnAgent = sourceBO.BookingHeader.Consignor.Header.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.ReturnAgent, "");

				if (returnAgent != null && returnAgent.RelatedParty != null)
				{
					dataObject.AddOrgAddress(writeManager, returnAgent.RelatedParty, RelatedPartyTypeList.Descriptions.ReturnAgent);
				}
			}

			if (!sourceBO.DL_OrderTrackingNumber.IsEmpty)
			{
				if (dataObject.LocalProcessing == null)
				{
					dataObject.LocalProcessing = new LocalProcessing(writeManager.WriterStrategy);
				}

				if (dataObject.LocalProcessing.OrderNumberCollection == null)
				{
					dataObject.LocalProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>());
				}

				dataObject.LocalProcessing.OrderNumberCollection.Add(new OrderNumber() { OrderReference = sourceBO.DL_OrderTrackingNumber });
			}

			dataObject.SetPackingLineCollection(() =>
			{
				var packingLineData = new PackingLine(writeManager.WriterStrategy)
				{
					PackQty = new ZLong(sourceBO.DL_PiecesManifested),
					PackType = ListHelper.GetWithDescription<PackageType>(sourceBO.DL_F3_NKPackType, sourceBO.Lookups.PackTypes),
					IsSignatureRequired = sourceBO.DL_SignatureRequired,
					RequiresFumigationCertificate = sourceBO.DL_RequiresFumigation,
					IsPersonalEffects = sourceBO.DL_IsPersonalEffects,
					IsTimber = sourceBO.DL_IsTimber,
					IsPerishable = sourceBO.DL_IsPerishable
				};
				return new DataObjectList<PackingLine>(new[] { packingLineData });
			});
		}

		void AddConsigneeAddress(SupplierBookingLine sourceBO, UniversalShipment dataObject)
		{
			OrganizationAddress consigneeAddress =
				new SupplierBookingLineConsigneeAddressWriter(writeManager).GetDataObject(sourceBO);

			if (consigneeAddress == null)
			{
				return;
			}

			if (dataObject.OrganizationAddressCollection == null)
			{
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			}

			dataObject.OrganizationAddressCollection?.Add(consigneeAddress);
		}

		void AddConsignorAddress(SupplierBookingLine sourceBO, UniversalShipment dataObject)
		{
			var consignorAddress = new SupplierBookingLineConsignorAddressWriter(writeManager).GetDataObject(sourceBO);

			if (consignorAddress == null)
			{
				return;
			}

			if (dataObject.OrganizationAddressCollection == null)
			{
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			}

			dataObject.OrganizationAddressCollection?.Add(consignorAddress);
		}

		protected override void InsertParents(SupplierBookingLine bookingLine, ref UniversalShipment dataObject)
		{
			if (IncludeParent)
			{
				var parent = bookingLine.Shipment;
				if (parent != null)
				{
					var shipmentDataContextManager = (IShipmentDataContextManager)DataContextType.ForwardingShipment.GetUniversalDataContextManager();
					var shipmentDataWriter = shipmentDataContextManager.GetShipmentDataObjectWriter(writeManager);

					var hierarchicalDataObjectWriter = shipmentDataWriter as IHierarchicalDataObjectWriter;
					if (hierarchicalDataObjectWriter != null)
					{
						hierarchicalDataObjectWriter.IncludeChildren = false;
						hierarchicalDataObjectWriter.IncludeParent = true;
					}

					var parentDataObject = (UniversalShipment)shipmentDataWriter.GetDataObject((BusinessObject)parent);
					dataObject = GetTopLevelDataObjectWithParentLinked(dataObject, parentDataObject);
				}
			}
		}

		UniversalShipment GetTopLevelDataObjectWithParentLinked(UniversalShipment bookingLineData, UniversalShipment parentDataObject)
		{
			if (writeManager.Schema == UniversalXmlSchema.Version_2012_11_DO_NOT_USE)
			{
				bookingLineData.SetParentShipmentCollection(() => bookingLineData.ParentShipmentCollection.AddSafe(parentDataObject));
				return bookingLineData;
			}
			else
			{
				parentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>(new[] { bookingLineData }));
				return parentDataObject;
			}
		}
	}
}
