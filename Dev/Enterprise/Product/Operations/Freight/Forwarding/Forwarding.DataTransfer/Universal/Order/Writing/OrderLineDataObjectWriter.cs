using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using IncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using OrderLine = Enterprise.Freight.Forwarding.Orders.Business.OrderLine;
using UniversalOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.OrderLine;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class OrderLineDataObjectWriter : DataObjectWriter<OrderLine, UniversalOrderLine>
	{
		internal OrderLineDataObjectWriter(IDataWritingManager writeManager, IOrderLineLinkManager orderLineLinkManager, LineRelatedDataWriterHelper lineRelatedDataWriterHelper)
			: base(writeManager)
		{
			this.orderLineLinkManager = orderLineLinkManager;
			this.lineRelatedDataWriterHelper = lineRelatedDataWriterHelper;
		}

		readonly IOrderLineLinkManager orderLineLinkManager;
		readonly LineRelatedDataWriterHelper lineRelatedDataWriterHelper;

		#region PopulateDataObject

		protected override UniversalOrderLine PopulateDataObject(OrderLine orderLineBO)
		{
			var orderLineDataObject = new UniversalOrderLine(writeManager.WriterStrategy);

			PopulateData(orderLineBO, orderLineDataObject);
			PopulateDangerousGoods(orderLineBO, orderLineDataObject);
			PopulateDeliveryPoint(orderLineBO, orderLineDataObject);
			PopulateOrganisations(orderLineBO, orderLineDataObject);

			if (orderLineLinkManager != null)
			{
				orderLineLinkManager.AllocateOrderLineLink(orderLineBO, orderLineDataObject);
			}
			lineRelatedDataWriterHelper.SetRelatedEntityCollection(writeManager.WriterStrategy, orderLineBO, orderLineDataObject);
			return orderLineDataObject;
		}

		static void PopulateData(OrderLine orderLineBO, UniversalOrderLine orderLineDataObject)
		{
			orderLineDataObject.AdditionalInformation = orderLineBO.JO_AdditionalInformation;
			orderLineDataObject.AdditionalTerms = orderLineBO.JO_AdditionalTerms;
			orderLineDataObject.CommercialInvoiceNumber = orderLineBO.JO_CommercialInvoiceNo;
			orderLineDataObject.ConfirmationNumber = orderLineBO.JO_ConfirmationNum;
			orderLineDataObject.ContainerNumber = orderLineBO.JO_ContainerNumber;
			orderLineDataObject.ContainerPackingOrder = orderLineBO.JO_ContainerPackingOrder;
			orderLineDataObject.CustomsData = new CustomsEntryInfo
			{
				CountryOfOrigin = Country.New(orderLineBO.CountryOfOrigin)
			};
			orderLineDataObject.ExpectedQuantity = orderLineBO.JO_QtyInvoiced;
			orderLineDataObject.ExtendedLinePrice = orderLineBO.JO_LinePrice;
			orderLineDataObject.IncoTerm = ListHelper.GetWithDescription<IncoTerm>(orderLineBO.JO_INCO, orderLineBO.JO_INCO_List);
			orderLineDataObject.InnerPacksQty = orderLineBO.JO_InnerPacks;
			orderLineDataObject.InnerPacksQtyUnit = ListHelper.GetWithDescription<PackageType>(orderLineBO.JO_InnerPacksUQ, orderLineBO.Lookups.PackTypes);
			orderLineDataObject.LineNumber = orderLineBO.JO_LineNo;
			orderLineDataObject.LineSplitNumber = orderLineBO.JO_LineSplitNumber;
			orderLineDataObject.LineReference = orderLineBO.JO_LineReference;
			orderLineDataObject.OrderedQty = orderLineBO.JO_Quantity;
			orderLineDataObject.OrderedQtyUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(orderLineBO.JO_F3_NKPackType, orderLineBO.JO_F3_NKPackType_List);
			orderLineDataObject.PackageQty = orderLineBO.JO_OuterPacks;
			orderLineDataObject.PackageQtyUnit = ListHelper.GetWithDescription<PackageType>(orderLineBO.JO_OuterPacksUQ, orderLineBO.Lookups.PackTypes);
			orderLineDataObject.PackageLength = orderLineBO.JO_OuterPackLength;
			orderLineDataObject.PackageHeight = orderLineBO.JO_OuterPackHeight;
			orderLineDataObject.PackageWidth = orderLineBO.JO_OuterPackWidth;
			orderLineDataObject.PackageLengthUnit = new UnitOfLength()
			{
				Code = orderLineBO.JO_OuterPackUnitOfDimension,
				Description = ListHelper.GetDescription(orderLineBO.JO_OuterPackUnitOfDimension, orderLineBO.Lookups.JO_OuterPackUnitOfDimension_List)
			};
			orderLineDataObject.PartAttribute1 = orderLineBO.JO_PartAttrib1;
			orderLineDataObject.PartAttribute2 = orderLineBO.JO_PartAttrib2;
			orderLineDataObject.PartAttribute3 = orderLineBO.JO_PartAttrib3;
			orderLineDataObject.SerialNumber = orderLineBO.JO_SerialNumber;
			orderLineDataObject.Product = !orderLineBO.JO_Partno.IsEmpty || !orderLineBO.JO_Description.IsEmpty
				? new Product { Code = orderLineBO.JO_Partno, Description = orderLineBO.JO_Description } : null;
			orderLineDataObject.QuantityMet = orderLineBO.JO_QtyReceived;
			orderLineDataObject.RequiredExWorks = orderLineBO.JO_ExWorksDate;
			orderLineDataObject.RequiredInStore = orderLineBO.JO_LineDropDate;
			orderLineDataObject.SupplierConfirmedAcceptance = orderLineBO.JO_ConfirmationDate;
			orderLineDataObject.SpecialInstructions = orderLineBO.JO_SpecialInstructions;
			orderLineDataObject.Status = ListHelper.GetWithDescription<CodeDescriptionPair>(orderLineBO.JO_LineStatus, orderLineBO.JO_LineStatus_List);
			orderLineDataObject.SubLineNumber = orderLineBO.JO_SubLineNo;
			orderLineDataObject.UnitPriceRecommended = orderLineBO.JO_ItemPrice;
			orderLineDataObject.Volume = orderLineBO.JO_ActualVolume;
			orderLineDataObject.VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(orderLineBO.JO_UnitOfVolume, orderLineBO.VolumeUnit_List);
			orderLineDataObject.Weight = orderLineBO.JO_ActualWeight;
			orderLineDataObject.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(orderLineBO.JO_UnitOfWeight, orderLineBO.WeightUnit_List);
			orderLineDataObject.OverQuantityPercentageLimit = orderLineBO.JO_OverQuantityPercentageLimit;
			orderLineDataObject.UnderQuantityPercentageLimit = orderLineBO.JO_UnderQuantityPercentageLimit;
			orderLineDataObject.EarlyShipmentLimitDays = orderLineBO.JO_EarlyShipmentLimitDays;
			orderLineDataObject.LateShipmentLimitDays = orderLineBO.JO_LateShipmentLimitDays;
			orderLineDataObject.QtyPacked = orderLineBO.JO_QtyPacked;
			orderLineDataObject.QtyBooked = orderLineBO.JO_Quantity - orderLineBO.JO_OpenQuantity;
			orderLineDataObject.ShipmentWindowStart = orderLineBO.JO_ShipmentWindowStart;
			orderLineDataObject.ShipmentWindowEnd = orderLineBO.JO_ShipmentWindowEnd;
			orderLineDataObject.HarmonisedCode = orderLineBO.JO_HSCode;
			orderLineDataObject.Commodity = !orderLineBO.JO_RH_NKCommodityCode.IsEmpty ? new Commodity { Code = orderLineBO.JO_RH_NKCommodityCode } : null;

			var order = orderLineBO.Order;
			if (order != null)
			{
				CustomLabelsCustomizedFieldDataObjectWriter.Write(JobOrderLineSchema.Instance, orderLineBO, orderLineDataObject, OrderLine.NewCustomLabelsProvider(order));
			}
		}

		#region PopulateDangerousGoods

		void PopulateDangerousGoods(OrderLine orderLineBO, UniversalOrderLine orderLineDataObject)
		{
			orderLineDataObject.SetUNDGCollection(() => ProcessCollection(orderLineBO.UNDGs, new UNDGDataObjectWriter(writeManager)));
		}

		#endregion

		#region PopulateDeliveryPoint

		void PopulateDeliveryPoint(OrderLine orderLineBO, UniversalOrderLine orderLineDataObject)
		{
			var deliveryPoint = GetFirstDeliveryPoint(orderLineBO);
			if (deliveryPoint != null)
			{
				orderLineDataObject.Consignee = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ConsigneeAddress)).GetDataObject(deliveryPoint);
				orderLineDataObject.CrossDockOrderNumber = orderLineBO.Order.JD_OrderNumber;
				orderLineDataObject.RequiredBy = orderLineBO.JO_LineDropDate.ToOffset();
			}
		}

		OrgAddress GetFirstDeliveryPoint(OrderLine orderLineBO)
		{
			foreach (var delivery in orderLineBO.Deliveries)
			{
				var deliveryPoint = delivery.DeliveryPoint;
				if (deliveryPoint != null)
				{
					return deliveryPoint;
				}
			}

			return null;
		}

		#endregion

		#region PopulateOrganisations

		void PopulateOrganisations(OrderLine orderLineBO, UniversalOrderLine orderDataObject)
		{
			orderDataObject.SetOrganizationAddressCollection(() => ProcessCollection(orderLineBO.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)));

			orderDataObject.OrganizationAddressCollection?.RemoveAll(address => (address.AddressType ?? ZString.Empty) == nameof(DocAddressType.GoodsAvailableAt));
			orderDataObject.OrganizationAddressCollection?.RemoveAll(address => (address.AddressType ?? ZString.Empty) == nameof(DocAddressType.GoodsDeliveredTo));
			orderDataObject.OrganizationAddressCollection?.RemoveAll(address => (address.AddressType ?? ZString.Empty) == nameof(DocAddressType.ConsigneeDocumentaryAddress));

			orderDataObject.AddOrgAddress(writeManager, orderLineBO.GoodsDeliveredToAddress, DocAddressType.ConsigneePickupDeliveryAddress);
			orderDataObject.AddOrgAddress(writeManager, orderLineBO.GoodsAvailableAtAddress, DocAddressType.ConsignorPickupDeliveryAddress);
			orderDataObject.AddOrgAddress(writeManager, orderLineBO.ConsigneeDocumentaryAddress);
		}

		#endregion

		#region Custom Fields

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(OrderLine orderLineBO)
		{
			return orderLineBO.GetUserDefinedValues();
		}

		#endregion

		#endregion
	}
}
