using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Tracking.Business.XmlSerialisation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business
{
	public class TrackingShipmentValueObjectDataAdapter : ValueObjectDataAdapter<TrackingShipment, Xsd.WebShipment>
	{
		protected override Type ValueObjectCollectionType
		{
			get { return typeof(Xsd.WebShipmentCollection); }
		}

		public override string RootCollectionElementName
		{
			get { return "WebShipments"; }
		}

		public override string RootElementName
		{
			get { return "WebShipment"; }
		}

		public override System.Xml.Schema.XmlSchema Schema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebShipmentSchema; }
		}

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebShipmentsSchema; }
		}

		protected override TrackingShipment FindBusinessObject(Xsd.WebShipment value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		protected override void ImportFromValueObjectCore(TrackingShipment bizObj, Xsd.WebShipment value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		public override TrackingShipment CreateOrUpdateFromValueObject(Xsd.WebShipment value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		protected override void ExportToValueObjectCore(TrackingShipment shipment, Xsd.WebShipment result, IValueObjectExportContext context)
		{
			if (shipment != null)
			{
				result.Number = (shipment.JS_UniqueConsignRef.IsEmpty) ? null : shipment.JS_UniqueConsignRef.ToString();
				result.HouseBill = (shipment.JS_HouseBill.IsEmpty) ? null : shipment.JS_HouseBill.ToString();
				result.ClientReference = (shipment.JS_BookingReference.IsEmpty) ? null : shipment.JS_BookingReference.ToString();

				result.Consignee = new OrganisationValueObjectDataAdapter().ExportToValueObject(shipment.Consignee, context);
				result.Shipper = new OrganisationValueObjectDataAdapter().ExportToValueObject(shipment.Consignor, context);

				if (shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsValid)
				{
					result.DeliveredDate = shipment.DocsAndCartage.JP_DeliveryCartageCompleted.ToDateTime();
				}
				if (shipment.JS_E_ARV.IsValid)
				{
					result.ETA = shipment.JS_E_ARV.ToDateTime();
				}
				if (shipment.JS_E_DEP.IsValid)
				{
					result.ETD = shipment.JS_E_DEP.ToDateTime();
				}

				result.Origin = Xsd.UNLOCO.FromPort(shipment.Origin);
				result.Destination = Xsd.UNLOCO.FromPort(shipment.Destination);
				result.GoodsDescription = (shipment.JS_GoodsDescription.IsEmpty) ? null : shipment.JS_GoodsDescription.ToString();

				result.Consols = new TrackingConsolValueObjectDataAdapter().ExportToValueObjectCollection(shipment.Consols, context);
				result.Containers = new WebContainerXmlDataAdapter().ExportToValueObjectCollection(shipment.Containers.ToArray(), context);
				result.Notes = new NoteValueObjectDataAdapter().ExportToXmlValueObjectCollection(shipment.Notes, context);
				result.Orders = new TrackingOrderSummaryValueObjectDataAdapter().ExportToXmlValueObjectCollection(shipment.AttachedOrders, context);
				result.Packings = new WebPackingXmlDataAdapter().ExportToValueObjectCollection(shipment.OuterPackLines, context);

				result.ServiceLevel = (shipment.ServiceLevel == null || shipment.ServiceLevel.RS_DescriptionMultilingual.IsEmpty) ? null : shipment.ServiceLevel.RS_DescriptionMultilingual;
				result.Quantity = Xsd.DimensionValue.FromAmountAndUnit(shipment.JS_OuterPacks, shipment.JS_F3_NKPackType);
				result.Size = Xsd.DimensionValue.FromAmountAndUnit(shipment.JS_ActualVolume, shipment.JS_UnitOfVolume);
				result.Weight = Xsd.DimensionValue.FromAmountAndUnit(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight);

				result.Custom.CustomAttrib1 = shipment.DocsAndCartage.JP_CustomAttrib1;
				result.Custom.CustomAttrib2 = shipment.DocsAndCartage.JP_CustomAttrib2;
				if (shipment.DocsAndCartage.JP_CustomDate1.IsValid)
				{
					result.Custom.CustomDate1 = shipment.DocsAndCartage.JP_CustomDate1;
				}
				if (shipment.DocsAndCartage.JP_CustomDate2.IsValid)
				{
					result.Custom.CustomDate2 = shipment.DocsAndCartage.JP_CustomDate2;
				}
				if (result.Custom.CustomDecimal1Specified = !shipment.DocsAndCartage.JP_CustomDecimal1.IsEmpty)
				{
					result.Custom.CustomDecimal1 = shipment.DocsAndCartage.JP_CustomDecimal1;
				}
				if (result.Custom.CustomDecimal2Specified = !shipment.DocsAndCartage.JP_CustomDecimal2.IsEmpty)
				{
					result.Custom.CustomDecimal2 = shipment.DocsAndCartage.JP_CustomDecimal2;
				}
				if (result.Custom.CustomFlag1Specified = !shipment.DocsAndCartage.JP_CustomFlag1.IsEmpty)
				{
					result.Custom.CustomFlag1 = shipment.DocsAndCartage.JP_CustomFlag1;
				}
				if (result.Custom.CustomFlag2Specified = !shipment.DocsAndCartage.JP_CustomFlag2.IsEmpty)
				{
					result.Custom.CustomFlag2 = shipment.DocsAndCartage.JP_CustomFlag2;
				}

				if (shipment.DocsAndCartage.JP_LCLAvailable.IsValid)
				{
					result.Deliver.Available = shipment.DocsAndCartage.JP_LCLAvailable.ToDateTime();
				}
				if (shipment.DocsAndCartage.JP_LCLStorageCommences.IsValid)
				{
					result.Deliver.StorageCommences = shipment.DocsAndCartage.JP_LCLStorageCommences;
				}
				if (shipment.DocsAndCartage.JP_EstimatedDelivery.IsValid)
				{
					result.Deliver.DeliveryFrom = shipment.DocsAndCartage.JP_EstimatedDelivery;
				}
				if (shipment.DocsAndCartage.JP_DeliveryRequiredBy.IsValid)
				{
					result.Deliver.DeliveryRequiredBy = shipment.DocsAndCartage.JP_DeliveryRequiredBy;
				}
				if (shipment.DocsAndCartage.JP_DeliveryCartageAdvised.IsValid)
				{
					result.Deliver.CartageAdvised = shipment.DocsAndCartage.JP_DeliveryCartageAdvised;
				}
				if (shipment.DocumentHelper.Documents.Count > 0)
				{
					result.DocumentLinks = new DocumentLinkValueObjectDataAdapter().ExportToXmlValueObjectCollection(shipment.DocumentHelper.Documents, context);
				}

				TrackingInvoiceLoader invoiceLoader = new TrackingInvoiceLoader(shipment);
				if (invoiceLoader.Transactions.Count > 0)
				{
					result.RelatedInvoiceLinks = new InvoiceLinkValueObjectDataAdapter().ExportToXmlValueObjectCollection(invoiceLoader.Transactions, context);
				}
			}
		}
	}
}
