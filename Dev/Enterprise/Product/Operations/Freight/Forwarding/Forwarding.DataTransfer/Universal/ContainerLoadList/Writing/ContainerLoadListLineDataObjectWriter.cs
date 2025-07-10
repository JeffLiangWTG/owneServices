using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using UniversalCommodity = Enterprise.UniversalDataBuss.DataObjects.Universal.Commodity;
using UniversalOrder = Enterprise.UniversalDataBuss.DataObjects.Universal.Order;
using UniversalOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.OrderLine;
using UniversalOrganizationAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalPackageType = Enterprise.UniversalDataBuss.DataObjects.Universal.PackageType;
using UniversalPackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalUnitOfVolume = Enterprise.UniversalDataBuss.DataObjects.Universal.UnitOfVolume;
using UniversalUnitOfWeight = Enterprise.UniversalDataBuss.DataObjects.Universal.UnitOfWeight;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ContainerLoadListLineDataObjectWriter : DataObjectWriter<ContainerLoadListLine, UniversalShipment>
	{
		public ContainerLoadListLineDataObjectWriter(IDataWritingManager manager, Dictionary<ZGuid, string> shipmentIDMapping, ContainerLoadListContainerLinkManager containerLinkManager)
			: base(manager)
		{
			this.shipmentIDMapping = shipmentIDMapping;
			this.containerLinkManager = containerLinkManager;
		}

		readonly Dictionary<ZGuid, string> shipmentIDMapping;
		readonly ContainerLoadListContainerLinkManager containerLinkManager;

		protected override UniversalShipment PopulateDataObject(ContainerLoadListLine sourceBO)
		{
			var dataObject = new UniversalShipment(writeManager.WriterStrategy);

			dataObject.TotalNoOfPacksDecimal = sourceBO.CLL_PackedQuantity;
			dataObject.TotalNoOfPacksPackageType = ListHelper.GetWithDescription<UniversalPackageType>(
				sourceBO.SupplierBookingLine.OrderLine.JO_F3_NKPackType,
				sourceBO.SupplierBookingLine.Lookups.BookedPackagesUnits
			);

			PopulateShipmentID(sourceBO, dataObject);
			PopulatePackingLineCollection(sourceBO, dataObject);
			PopulateSupplierBooking(sourceBO, dataObject);

			return dataObject;
		}

		void PopulateShipmentID(ContainerLoadListLine sourceBO, UniversalShipment dataObject)
		{
			if (shipmentIDMapping.TryGetValue(sourceBO.PK, out var shipmentID))
			{
				dataObject.SetAddInfoCollection(() => new List<AddInfo>
				{
					new AddInfo { Key = "ShipmentID", Value = shipmentID }
				});
			}
		}

		void PopulateSupplierBooking(ContainerLoadListLine sourceBO, UniversalShipment dataObject)
		{
			var orderLine = sourceBO.SupplierBookingLine.OrderLine;
			var orderData = new UniversalOrder(writeManager.WriterStrategy)
			{
				OrderNumber = orderLine.Order.JD_OrderNumber,
				OrderNumberSplit = orderLine.Order.JD_OrderNumberSplit
			};

			var product = !orderLine.JO_Partno.IsEmpty || !orderLine.JO_Description.IsEmpty
				? new Product { Code = orderLine.JO_Partno, Description = orderLine.JO_Description } : null;
			var orderLineData = new UniversalOrderLine
			{
				LineNumber = orderLine.JO_LineNo,
				SubLineNumber = orderLine.JO_SubLineNo,
				Product = product
			};
			orderData.SetOrderLineCollection(() => new DataObjectList<UniversalOrderLine>() { orderLineData });

			var data = new UniversalShipment(writeManager.WriterStrategy);
			data.DataContext = DataContextFactory.New();
			data.DataContext.AddDataSource(DataContextType.JobSupplierBooking, sourceBO.SupplierBookingLine.SupplierBooking.JSB_BookingId);
			data.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { new UniversalShipment(writeManager.WriterStrategy) { Order = orderData } });
			data.SubShipmentCollection[0].SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress> {
				new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.BuyerDocumentaryAddress)).GetDataObject(orderLine.Order.BuyerAddress)
			});
			dataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { data });
		}

		void PopulatePackingLineCollection(ContainerLoadListLine sourceBO, UniversalShipment dataObject)
		{
			dataObject.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine>() { BuildUniversalPackingLine(sourceBO) });
		}

		UniversalPackingLine BuildUniversalPackingLine(ContainerLoadListLine sourceBO)
		{
			var packLine = new UniversalPackingLine();

			packLine.ContainerPackingOrder = sourceBO.CLL_LoadSequence;
			packLine.PackQty = Convert.ToInt64(sourceBO.CLL_Packages);
			packLine.PackType = ListHelper.GetWithDescription<UniversalPackageType>(sourceBO.CLL_F3_NKPackagesUnit, sourceBO.Lookups.PackagesUnits);
			packLine.Volume = sourceBO.CLL_Volume;
			packLine.VolumeUnit = ListHelper.GetWithDescription<UniversalUnitOfVolume>(sourceBO.CLL_VolumeUnit, new CodeDescriptionPairList(OLookUpEditType.Volume));
			packLine.Weight = sourceBO.CLL_Weight;
			packLine.WeightUnit = ListHelper.GetWithDescription<UniversalUnitOfWeight>(sourceBO.CLL_WeightUnit, new CodeDescriptionPairList(OLookUpEditType.Weight));
			packLine.HarmonisedCode = sourceBO.CLL_HarmonizedCode;
			packLine.Commodity = ListHelper.GetWithDescription<UniversalCommodity>(sourceBO.CLL_RH_NKCommodityCode, sourceBO.Lookups.CommodityCodes);
			packLine.ReferenceNumber = sourceBO.CLL_ReferenceNumber;
			packLine.GoodsDescription = sourceBO.CLL_Description;
			packLine.MarksAndNos = sourceBO.CLL_MarksAndNumbers;
			packLine.PackingLineID = sourceBO.SupplierBookingLine.JSL_BookingLineId;
			if (sourceBO.Container != null)
			{
				packLine.ContainerLink = containerLinkManager.GetContainerLink(sourceBO.Container);
			}

			return packLine;
		}
	}
}
