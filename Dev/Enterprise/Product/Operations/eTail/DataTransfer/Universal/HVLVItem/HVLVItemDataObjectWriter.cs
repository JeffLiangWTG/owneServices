using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.eTail.DataTransfer.Universal
{
	class HVLVItemDataObjectWriter : DataObjectWriter<HVLVItem, PackingLine>
	{
		public HVLVItemDataObjectWriter(IDataWritingManager manager, Shipment headerShipment)
			: base(manager)
		{
			this.headerShipment = Argument.NotNull(headerShipment, nameof(headerShipment));
		}

		readonly Shipment headerShipment;

		protected override PackingLine PopulateDataObject(HVLVItem itemBO)
		{
			var itemDataObject = new PackingLine(writeManager.WriterStrategy);
			var consignmentBO = itemBO.Consignment;

			itemDataObject.BillNumber = consignmentBO.HVC_WaybillNumber;
			itemDataObject.BillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };
			itemDataObject.PackQty = 1;
			itemDataObject.OrderReference = itemBO.HVI_ShipperReference;
			itemDataObject.ReferenceNumber = itemBO.HVI_ItemId;
			itemDataObject.Barcode = itemBO.HVI_CurrentBarcode;
			itemDataObject.Height = itemBO.HVI_Height;
			itemDataObject.Width = itemBO.HVI_Width;
			itemDataObject.Length = itemBO.HVI_Length;
			itemDataObject.LengthUnit = ListHelper.GetWithDescription<UnitOfLength>(itemBO.HVI_UnitOfDimension, itemBO.Lookups.HVI_UnitOfDimensionList);
			itemDataObject.PackType = ListHelper.GetWithDescription<PackageType>(itemBO.HVI_F3_NKPackType, itemBO.Lookups.PackTypes);
			itemDataObject.ManifestedWeight = itemBO.HVI_ManifestedWeight;
			itemDataObject.Weight = itemBO.HVI_ActualWeight.IsDefault ? itemBO.HVI_ManifestedWeight : itemBO.HVI_ActualWeight;
			itemDataObject.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(consignmentBO.HVC_WeightUQ, consignmentBO.Lookups.HVC_WeightUQ_List);
			itemDataObject.ManifestedVolume = itemBO.HVI_ManifestedVolume;
			itemDataObject.Volume = itemBO.HVI_ActualVolume.IsDefault ? itemBO.HVI_ManifestedVolume : itemBO.HVI_ActualVolume;
			itemDataObject.VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(consignmentBO.HVC_VolumeUQ, consignmentBO.Lookups.HVC_VolumeUQ_List);
			itemDataObject.GoodsDescription = itemBO.HVI_GoodsDescription;
			itemDataObject.ContainerNumber = itemBO.HVI_ContainerNumber;

			itemDataObject.RequiresFumigationCertificate = consignmentBO.HVC_RequiresFumigation;
			itemDataObject.IsPersonalEffects = consignmentBO.HVC_IsPersonalEffects;
			itemDataObject.IsTimber = consignmentBO.HVC_IsTimber;
			itemDataObject.IsPerishable = consignmentBO.HVC_IsPerishable;

			var isOutturned = itemBO.HasArrivedAtDestinationDepot;
			itemDataObject.OutturnQty = isOutturned ? 1 : 0;
			itemDataObject.OutturnDamagedQty = isOutturned && itemBO.HVI_IsDamaged ? 1 : 0;
			itemDataObject.OutturnPillagedQty = isOutturned && itemBO.HVI_IsPillaged ? 1 : 0;

			itemDataObject.SetPackedItemCollection(() =>
			{
				List<PackedItem> result = null;
				if (itemBO.Lines.Count > 0)
				{
					result = new List<PackedItem>();
					var consignment = itemBO.Consignment;
					var commercialInvoiceLineCollection = GetOrCreateCommercialInvoiceLineCollection(consignment.HVC_WaybillNumber);
					var isExport = consignment.IsExport;
					foreach (var line in itemBO.Lines.Cast<HVLVItemLine>())
					{
						result.Add(ProcessItemLine(commercialInvoiceLineCollection, isExport, line));
					}
				}

				return result;
			});

			itemDataObject.SetUNDGCollection(() =>
			{
				List<UNDG> result = null;
				if (itemBO.UNDGs.Any())
				{
					result = new List<UNDG>();
					var undgWriter = new UNDGDataObjectWriter(writeManager);
					itemBO.UNDGs.ForEach(undg => result.Add(undgWriter.GetDataObject(undg)));
				}
				else if (!consignmentBO.HVC_UndgClass.IsEmpty)
				{
					result = new List<UNDG>
					{
						new UNDG(writeManager.WriterStrategy) { IMOClass = consignmentBO.HVC_UndgClass }
					};
				}

				return result;
			});

			return itemDataObject;
		}

		PackedItem ProcessItemLine(DataObjectList<CommercialInvoiceLine> commercialInvoiceLineCollection, bool isExport, HVLVItemLine line)
		{
			return new PackedItem
			{
				GoodsValue = line.HVS_IntrinsicValue,
				GrossWeight = line.HVS_GrossWeight,
				GrossWeightUnit = new UnitOfWeight() { Code = line.HVS_WeightUnit },
				NetWeight = line.HVS_NetWeight,
				Product = new Product() { Code = line.HVS_ProductCode },
				Description = line.HVS_GoodsDescription,
				NetWeightUnit = new UnitOfWeight() { Code = line.HVS_WeightUnit },
				PackedQuantity = new ZDecimal(line.HVS_Quantity),
				CIFValue = new ZDecimal(line.HVS_CustomsValue),
				ItemSpecificationUrl = line.HVS_ItemURL,
				CommercialInvoiceLineLink = GetCommercialInvoiceLineLink(commercialInvoiceLineCollection, isExport, line)
			};
		}

		public CommercialInvoiceLine WriteInvoiceLine(HVLVItemLine line, bool isExport)
		{
			var harmonisedCode = isExport ? line.HVS_OriginTariff : line.HVS_DestinationTariff;

			var invoiceLineData = new CommercialInvoiceLine()
			{
				HarmonisedCode = TariffFormatterDecider.GetByCountryCode(line.ShipmentDestinationCountryCode).Format(harmonisedCode),
				CustomsValue = line.HVS_CustomsValue,
				Description = line.HVS_GoodsDescription,
				NetWeight = line.HVS_NetWeight,
				NetWeightUnit = new UnitOfWeight() { Code = line.HVS_WeightUnit },
				Weight = line.HVS_GrossWeight,
				WeightUnit = new UnitOfWeight() { Code = line.HVS_WeightUnit },
				CustomsQuantity = (ZDecimal)line.HVS_Quantity,
				PartNo = line.HVS_ProductCode,
				ClassificationCode = line.ClassificationLookup?.CC_LookupCode,
				InvoiceQuantity = (ZDecimal)line.HVS_Quantity,
				InvoiceQuantityUnit = new CodeDescriptionPair() { Code = PkgUnit.Piece },
				CountryOfOrigin = new Country() { Code = line.HVS_RN_NKOriginCountryCode },
				LinePrice = line.HVS_CustomsValue,
				LocalDescription = line.HVS_OriginGoodsDescription
			};

			invoiceLineData.CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>()
			{
				new CustomsSupportingInformation()
				{
					Country = new Country() { Code = line.HVS_RN_NKOriginCountryCode },
					Tariff = line.HVS_OriginTariff,
					Category = new CodeDescriptionPair() { Code = RefCusConditionValueTypes.Codes.SupportingDocument }
				}
			};

			return invoiceLineData;
		}

		ZInt? GetCommercialInvoiceLineLink(DataObjectList<CommercialInvoiceLine> commercialInvoiceLineCollection, bool isExport, HVLVItemLine line)
		{
			if (commercialInvoiceLineCollection == null)
			{
				return null;
			}

			var link = commercialInvoiceLineCollection.Count + 1;

			var invoiceLineData = WriteInvoiceLine(line, isExport);
			invoiceLineData.Link = link;
			invoiceLineData.LineNo = link;

			commercialInvoiceLineCollection.Add(invoiceLineData);
			return link;
		}

		DataObjectList<CommercialInvoiceLine> GetOrCreateCommercialInvoiceLineCollection(ZString invoiceNumber)
		{
			DataObjectList<CommercialInvoiceLine> result;
			var commercialInfo = headerShipment.CommercialInfo ?? (headerShipment.CommercialInfo = new CommercialInfo());
			var commercialInvoiceCollection = commercialInfo.CommercialInvoiceCollection ?? (commercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>());
			if (commercialInvoiceCollection.Any())
			{
				var matchingInvoiceHeader = commercialInvoiceCollection.FirstOrDefault(x => x.InvoiceNumber.Equals(invoiceNumber));
				if (matchingInvoiceHeader != null)
				{
					result = matchingInvoiceHeader.CommercialInvoiceLineCollection;
				}
				else
				{
					result = commercialInvoiceCollection[0].CommercialInvoiceLineCollection;
				}
			}
			else
			{
				result = new DataObjectList<CommercialInvoiceLine>();
				commercialInvoiceCollection.Add(new CommercialInvoiceHeader(writeManager.WriterStrategy)
				.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => result)));
			}

			return result;
		}
	}
}
