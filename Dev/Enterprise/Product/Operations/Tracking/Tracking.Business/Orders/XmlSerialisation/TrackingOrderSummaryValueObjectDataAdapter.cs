using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Forwarding.Orders.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Xml Data adapter for Orders on Shipments, delcarations and orders
	/// 
	/// </summary>
	public class TrackingOrderSummaryValueObjectDataAdapter : ValueObjectDataAdapter<Order, Xsd.WebOrderSummary>
	{
		protected override Type ValueObjectCollectionType
		{
			get { return typeof(Xsd.WebOrderSummaryCollection); }
		}

		public override string RootCollectionElementName
		{
			get { return "WebOrderSummaries"; }
		}

		public override string RootElementName
		{
			get { return "WebOrderSummary"; }
		}

		public override System.Xml.Schema.XmlSchema Schema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebOrderSummarySchema; }
		}

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebOrderSummarysSchema; }
		}

		protected override Order FindBusinessObject(Xsd.WebOrderSummary value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		protected override void ImportFromValueObjectCore(Order bizObj, Xsd.WebOrderSummary value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		public override Order CreateOrUpdateFromValueObject(Xsd.WebOrderSummary value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		public Xsd.WebOrderSummaryCollection ExportToXmlValueObjectCollection(OrderCollection collection, IValueObjectExportContext context)
		{
			return (Xsd.WebOrderSummaryCollection)ToValueObjectCollection(collection, context);
		}

		protected override void ExportToValueObjectCore(Order bizObj, Xsd.WebOrderSummary constructedValueObject, IValueObjectExportContext context)
		{
			Xsd.WebOrderSummary result = constructedValueObject;

			Order order = bizObj;
			if (order != null)
			{
				result.Packs = Xsd.DimensionValue.FromAmountAndUnit(order.JD_Packs, order.JD_F3_NKPackType);
				if (!order.JD_OrderDate.IsEmpty)
				{
					result.OrderDate = (ZDate)order.JD_OrderDate;
				}

				result.OrderNumber = order.JD_OrderNumber;
				if (!order.JD_OrderStatus.IsEmpty)
				{
					result.Status = order.JD_OrderStatus;
				}
			}
		}
	}
}
