using System;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Tracking.Business.Declaration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Xml Data adapter for Orders on Shipments, delcarations and orders
	/// </summary>
	public class TrackingOrderValueObjectDataAdapter : OrderValueObjectDataAdapter<TrackingOrder, Xsd.WebOrder>
	{
		protected override Type ValueObjectCollectionType
		{
			get { return typeof(Xsd.WebOrderCollection); }
		}

		public override string RootCollectionElementName
		{
			get { return "WebOrders"; }
		}

		public override string RootElementName
		{
			get { return "WebOrder"; }
		}

		public override System.Xml.Schema.XmlSchema Schema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebOrderSchema; }
		}

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebOrdersSchema; }
		}

		public Xsd.WebOrderCollection ExportToValueObjectCollection(TrackingLegacyOrderCollection collection, IValueObjectExportContext context)
		{
			return (Xsd.WebOrderCollection)ToValueObjectCollection(collection, context);
		}

		protected override TrackingOrder FindBusinessObject(Xsd.WebOrder value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		protected override void ImportFromValueObjectCore(TrackingOrder bizObj, Xsd.WebOrder value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		protected override void ExportToValueObjectCore(TrackingOrder order, Xsd.WebOrder result, IValueObjectExportContext context)
		{
			base.ExportToValueObjectCore(order, result, context);

			if (order != null)
			{
				if (order.DocumentHelper.Documents.Count > 0)
				{
					result.DocumentLinks = new DocumentLinkValueObjectDataAdapter().ExportToXmlValueObjectCollection(order.DocumentHelper.Documents, context);
				}

				result.Shipment = GetShipmentInfo(order, context);
			}
		}

		/// <summary>
		/// Returns Shipment information for order
		/// </summary>
		/// <param name="order"></param>
		/// <param name="Notify"></param>
		/// <returns></returns>
		protected Xsd.WebShipment GetShipmentInfo(TrackingOrder order, IValueObjectExportContext context)
		{
			if (order.Shipment != null)
			{
				return new TrackingShipmentValueObjectDataAdapter().ExportToValueObject(order.Shipment, context);
			}
			else if (order.Declaration != null)
			{
				TrackingDeclaration declaration = new TrackingDeclaration((BaseJobDeclaration)order.Declaration);
				return new TrackingDeclarationValueObjectDataAdapter().ExportToValueObject(declaration, context);
			}
			return null;
		}
	}
}
