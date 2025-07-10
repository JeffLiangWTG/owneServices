using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ImportedOrderLine : ImportedObject
	{
		public ImportedOrderLine(ImportedOrder parentOrder, OrderLine orderLine)
		{
			this.Order = parentOrder;
			Populate(orderLine);
		}

		public ImportedOrder Order { get; private set; }

		void Populate(OrderLine orderLine)
		{
			IsEmpty = orderLine.IsNull;
			IsProductOnFile = orderLine.Product != null;
			IsUNDGSubstanceValid = orderLine.UNDGs.Count == 0 || orderLine.UNDGs[0].DI_DG.IsEmpty || orderLine.UNDGs[0].Substance != null;

			JO_Partno = new ImportedProperty(orderLine.JO_PartnoInfo, Res.GetString("d60dabaa-067d-4ec7-aa14-58e704e4b630", "Product #"));
			JO_Description = new ImportedProperty(orderLine.JO_DescriptionInfo, Res.GetString("6fc56ec1-c872-43ec-9c82-62190aa72e06", "Description"));
			JO_InnerPacks = new ImportedProperty(orderLine.JO_InnerPacksInfo, Res.GetString("02f97244-6446-4ab2-9b05-8fe77a98964e", "Inner Packs"));
			JO_OuterPacks = new ImportedProperty(orderLine.JO_OuterPacksInfo, Res.GetString("1652ea25-db00-4363-90dc-6a5a99d6a85a", "Outer Packs"));
			JO_Quantity = new ImportedProperty(orderLine.JO_QuantityInfo, Res.GetString("183df63a-e507-4f37-9ce8-100b465334c3", "Qty Ordered"));
			JO_QtyInvoiced = new ImportedProperty(orderLine.JO_QtyInvoicedInfo, Res.GetString("e019bea9-502b-4a92-a31f-0c7c4ff3af6d", "Qty Invoiced"));
			JO_QtyReceived = new ImportedProperty(orderLine.JO_QtyReceivedInfo, Res.GetString("eef37e5f-80dc-4f34-9e0d-6668bbd87623", "Qty Received"));
			JO_F3_NKPackType = new ImportedProperty(orderLine.JO_F3_NKPackTypeInfo, Res.GetString("1a2b58b1-2c75-42ae-88f6-ca50bf2173e6", "Unit of Qty"));
			JO_ItemPrice = new ImportedProperty(orderLine.JO_ItemPriceInfo, Res.GetString("bebe2e94-0b68-44d4-94fb-f4db281b1dfb", "Item Price"));
			JO_LinePrice = new ImportedProperty(orderLine.JO_LinePriceInfo, Res.GetString("0e72a64d-01b8-4e59-a8b0-9e098165f4a1", "Total Price"));
			JO_CommercialInvoiceNo = new ImportedProperty(orderLine.JO_CommercialInvoiceNoInfo, Res.GetString("7f737b9c-6a3e-45ed-bce1-a1b100ba5a26", "Invoice #"));
			JO_RN_NKCountryOfOrigin = new ImportedProperty(orderLine.JO_RN_NKCountryOfOriginInfo, Res.GetString("3eb11b34-4754-4c65-b279-77ae2cc97b4c", "Country/Region Origin"));
			JO_LineStatus = new ImportedProperty(orderLine.JO_LineStatusInfo, Res.GetString("5857d76a-2068-4567-bc97-27c959ae1917", "Status"));
			JO_LineDropDate = new ImportedProperty(orderLine.JO_LineDropDateInfo, Res.GetString("331be88a-04b5-4858-a279-8f295fd563bd", "Required Date"));

			ImportedPropertyState state = ImportedPropertyState.New;
			if (orderLine.IsInDatabase)
			{
				state = orderLine.JO_LineNoInfo.HasChanges || orderLine.JO_SubLineNoInfo.HasChanges || orderLine.JO_LineSplitNumberInfo.HasChanges ? ImportedPropertyState.Modified : ImportedPropertyState.Unchanged;
			}
			JO_LineNoAndSplitAndSubLine = new ImportedProperty(orderLine.JO_LineNoAndSplitAndSubLineInfo, Res.GetString("a858ecbe-3473-4ba4-b512-cc79d8f9940f", "Line #"), state);

			state = ImportedPropertyState.New;
			if (orderLine.IsInDatabase)
			{
				state = orderLine.FirstUNDGSubstanceInfo.HasChanges ? ImportedPropertyState.Modified : ImportedPropertyState.Unchanged;
			}
			FirstUNDGSubstance = new ImportedProperty(orderLine.FirstUNDGSubstanceInfo, Res.GetString("fefa8701-eb60-4b59-be1b-0340d5c614ab", "DG Substance"), state);

			SetupOrderLineDeliveries(orderLine);
		}

		#region Relationships

		public ImportedOrderLineDeliveryCollection Deliveries
		{
			get { return deliveries; }
		}
		ImportedOrderLineDeliveryCollection deliveries;

		void SetupOrderLineDeliveries(OrderLine orderLine)
		{
			if (deliveries != null)
			{
				throw new NotSupportedException("Deliveries already setup");
			}

			deliveries = new ImportedOrderLineDeliveryCollection();
			foreach (OrderLineDelivery delivery in orderLine.Deliveries)
			{
				deliveries.Add(new ImportedOrderLineDelivery(this, delivery));
			}

			if (deliveries.Count == 0)
			{
				deliveries.Add(new ImportedOrderLineDelivery(this, orderLine.Factory.GetNull<OrderLineDelivery>())); //Dummy Delivery as the Order Changed Report groups Order Line Deliveries
			}
		}

		#endregion

		#region Properties

		public ZBool IsEmpty { get; private set; }
		public ZBool IsProductOnFile { get; private set; }
		public ZBool IsUNDGSubstanceValid { get; private set; }

		#endregion

		#region Amended Properties

		public ImportedProperty JO_LineNoAndSplitAndSubLine { get; private set; }
		public ImportedProperty JO_Partno { get; private set; }
		public ImportedProperty JO_Description { get; private set; }
		public ImportedProperty JO_InnerPacks { get; private set; }
		public ImportedProperty JO_OuterPacks { get; private set; }
		public ImportedProperty JO_Quantity { get; private set; }
		public ImportedProperty JO_QtyInvoiced { get; private set; }
		public ImportedProperty JO_QtyReceived { get; private set; }
		public ImportedProperty JO_F3_NKPackType { get; private set; }
		public ImportedProperty JO_ItemPrice { get; private set; }
		public ImportedProperty JO_LinePrice { get; private set; }
		public ImportedProperty JO_CommercialInvoiceNo { get; private set; }
		public ImportedProperty JO_RN_NKCountryOfOrigin { get; private set; }
		public ImportedProperty JO_LineStatus { get; private set; }
		public ImportedProperty JO_LineDropDate { get; private set; }
		public ImportedProperty FirstUNDGSubstance { get; private set; }

		#endregion

		#region GetAllImportedProperties / AmendedPropertiesToShowAlways

		public override ImportedProperty[] GetAllImportedProperties()
		{
			return new ImportedProperty[]
			{
				JO_LineNoAndSplitAndSubLine,
				JO_Partno,
				JO_Description,
				JO_InnerPacks,
				JO_OuterPacks,
				JO_Quantity,
				JO_QtyInvoiced,
				JO_QtyReceived,
				JO_F3_NKPackType,
				JO_ItemPrice,
				JO_LinePrice,
				JO_CommercialInvoiceNo,
				JO_RN_NKCountryOfOrigin,
				JO_LineStatus,
				JO_LineDropDate,
				FirstUNDGSubstance,
			};
		}

		public override List<string> PropertiesToShowAlways
		{
			get
			{
				if (propertiesToShowAlways == null)
				{
					propertiesToShowAlways = new List<string>();
					if (!IsEmpty)
					{
						propertiesToShowAlways.Add(JO_LineNoAndSplitAndSubLine.Name);
						propertiesToShowAlways.Add(JO_Partno.Name);
						propertiesToShowAlways.Add(JO_Description.Name);
					}
				}
				return propertiesToShowAlways;
			}
		}
		List<string> propertiesToShowAlways;

		public override List<string> OtherPropertiesToShow
		{
			get { return Order.ModifiedOrderLinePropertiesToShow; }
		}

		#endregion
	}
}
