using System.Collections.Generic;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using UniversalCommodity = Enterprise.UniversalDataBuss.DataObjects.Universal.Commodity;
using UniversalOrder = Enterprise.UniversalDataBuss.DataObjects.Universal.Order;
using UniversalOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.OrderLine;
using UniversalPackageType = Enterprise.UniversalDataBuss.DataObjects.Universal.PackageType;
using UniversalPackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalProduct = Enterprise.UniversalDataBuss.DataObjects.Universal.Product;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalUnitOfVolume = Enterprise.UniversalDataBuss.DataObjects.Universal.UnitOfVolume;
using UniversalUnitOfWeight = Enterprise.UniversalDataBuss.DataObjects.Universal.UnitOfWeight;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class JobSupplierBookingLineDataObjectWriter : DataObjectWriter<JobSupplierBookingLine, UniversalShipment>
	{
		public JobSupplierBookingLineDataObjectWriter(IDataWritingManager manager, LineRelatedDataWriterHelper lineRelatedDataWriterHelper)
			: base(manager)
		{
			this.lineRelatedDataWriterHelper = lineRelatedDataWriterHelper;
		}

		readonly LineRelatedDataWriterHelper lineRelatedDataWriterHelper;

		protected override UniversalShipment PopulateDataObject(JobSupplierBookingLine sourceBO)
		{
			var dataObject = new UniversalShipment(writeManager.WriterStrategy);

			dataObject.TotalNoOfPacksDecimal = sourceBO.JSL_BookedQuantity;
			dataObject.TotalNoOfPacksPackageType = ListHelper.GetWithDescription<UniversalPackageType>(
				sourceBO.OrderLine.JO_F3_NKPackType,
				sourceBO.Lookups.BookedPackagesUnits
			);

			PopulatePackingLineCollection(sourceBO, dataObject, dataObject.TotalNoOfPacksPackageType);
			PopulateOrder(sourceBO, dataObject);
			PopulateOrganisations(sourceBO, dataObject);
			PopulateDates(sourceBO, dataObject);
			return dataObject;
		}

		void PopulatePackingLineCollection(JobSupplierBookingLine sourceBO, UniversalShipment dataObject, UniversalPackageType quantityType)
		{
			dataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine>() { BuildUniversalPackingLine(sourceBO, quantityType) });
		}

		void PopulateOrder(JobSupplierBookingLine sourceBO, UniversalShipment dataObject)
		{
			if (sourceBO.OrderLine?.Order == null)
			{
				return;
			}

			dataObject.Order = new UniversalOrder(writeManager.WriterStrategy)
			{
				OrderNumber = sourceBO.OrderLine.Order.JD_OrderNumber,
				OrderNumberSplit = sourceBO.OrderLine.Order.JD_OrderNumberSplit
			};

			var orderLineBO = sourceBO.OrderLine;
			var product = !orderLineBO.JO_Partno.IsEmpty || !orderLineBO.JO_Description.IsEmpty
				? new UniversalProduct { Code = orderLineBO.JO_Partno, Description = orderLineBO.JO_Description } : null;
			dataObject.Order.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>()
			{
				new UniversalOrderLine
				{
					LineNumber = sourceBO.OrderLine.JO_LineNo,
					SubLineNumber = sourceBO.OrderLine.JO_SubLineNo,
					LineReference = sourceBO.OrderLine.JO_LineReference,
					Product = product,
					UnitPriceRecommended = sourceBO.OrderLine.JO_ItemPrice,
					ExtendedLinePrice = sourceBO.OrderLine.JO_LinePrice,
					RequiredExWorks = sourceBO.OrderLine.JO_ExWorksDate,
					RequiredInStore = sourceBO.OrderLine.JO_LineDropDate,
					ShipmentWindowStart = sourceBO.OrderLine.JO_ShipmentWindowStart,
					ShipmentWindowEnd = sourceBO.OrderLine.JO_ShipmentWindowEnd,
				}
			});

			var orderContextKey = orderLineBO.Order.GetUniversalDataContextManager().DataContextKey;
			dataObject.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo { Key = "OrderContextKey", Value = orderContextKey }
			});
		}

		void PopulateOrganisations(JobSupplierBookingLine sourceBO, UniversalShipment dataObject)
		{
			dataObject.AddOrgAddress(writeManager, sourceBO.ManufacturerAddress);
			dataObject.AddOrgAddress(writeManager, sourceBO.OrderLine?.Order?.BuyerAddress, DocAddressType.BuyerDocumentaryAddress);
		}

		void PopulateDates(JobSupplierBookingLine sourceBO, UniversalShipment dataObject)
		{
			dataObject.TryAddDateToCollection(DateType.ShipmentWindowStart, sourceBO.JSL_ShipmentWindowStart);
			dataObject.TryAddDateToCollection(DateType.ShipmentWindowEnd, sourceBO.JSL_ShipmentWindowEnd);
		}

		UniversalPackingLine BuildUniversalPackingLine(JobSupplierBookingLine sourceBO, UniversalPackageType quantityType)
		{
			var packLine = new UniversalPackingLine(writeManager.WriterStrategy);

			packLine.PackingLineID = sourceBO.JSL_BookingLineId;
			packLine.PackQty = sourceBO.JSL_BookedPackages.ToZLong();
			packLine.PackType = ListHelper.GetWithDescription<UniversalPackageType>(sourceBO.JSL_F3_NKBookedPackagesUnit, sourceBO.Lookups.BookedPackagesUnits);
			packLine.Weight = sourceBO.JSL_GrossWeight;
			packLine.WeightUnit = ListHelper.GetWithDescription<UniversalUnitOfWeight>(sourceBO.JSL_GrossWeightUnit, new CodeDescriptionPairList(OLookUpEditType.Weight));
			packLine.Volume = sourceBO.JSL_Volume;
			packLine.VolumeUnit = ListHelper.GetWithDescription<UniversalUnitOfVolume>(sourceBO.JSL_VolumeUnit, new CodeDescriptionPairList(OLookUpEditType.Volume));
			packLine.GoodsDescription = sourceBO.JSL_Description;
			packLine.MarksAndNos = sourceBO.JSL_MarksAndNumbers;
			packLine.Commodity = ListHelper.GetWithDescription<UniversalCommodity>(sourceBO.JSL_RH_NKCommodityCode, sourceBO.Lookups.CommodityCodes);
			packLine.HarmonisedCode = sourceBO.JSL_HarmonisedCode;

			var cfsAddress = sourceBO.SupplierBooking.CFSAddress;
			packLine.FirstCFSReceiptDate = sourceBO.JSL_FirstReceiptDateUtc.ToLocationTime(cfsAddress?.EffectiveRelatedPortCode).ToZDateTime();
			packLine.LastCFSReceiptDate = sourceBO.JSL_LastReceiptDateUtc.ToLocationTime(cfsAddress?.EffectiveRelatedPortCode).ToZDateTime();

			packLine.ReceivedQuantity = sourceBO.JSL_ReceivedQuantity;
			packLine.ReceivedQuantityType = quantityType;
			packLine.ReceivedPacks = sourceBO.JSL_ReceivedPackages;
			packLine.ReceivedPacksType = packLine.PackType;
			packLine.ReceivedWeight = sourceBO.JSL_ReceivedWeight;
			packLine.ReceivedWeightUnit = packLine.WeightUnit;
			packLine.ReceivedVolume = sourceBO.JSL_ReceivedVolume;
			packLine.ReceivedVolumeUnit = packLine.VolumeUnit;

			lineRelatedDataWriterHelper.SetRelatedEntityCollection(writeManager.WriterStrategy, sourceBO, packLine);
			return packLine;
		}
	}
}
