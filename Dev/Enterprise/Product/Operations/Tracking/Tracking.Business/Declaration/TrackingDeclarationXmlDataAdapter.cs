using System;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.Declaration
{
	public class TrackingDeclarationValueObjectDataAdapter : ValueObjectDataAdapter<TrackingDeclaration, Xsd.WebShipment>
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
			get { return WebServicesXmlSchemaDefinitions.Instance.WebShipmentSchema; }
		}

		protected override TrackingDeclaration FindBusinessObject(Xsd.WebShipment value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		protected override void ImportFromValueObjectCore(TrackingDeclaration bizObj, Xsd.WebShipment value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		public override TrackingDeclaration CreateOrUpdateFromValueObject(Xsd.WebShipment value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		protected override void ExportToValueObjectCore(TrackingDeclaration declaration, Xsd.WebShipment result, IValueObjectExportContext context)
		{
			if ((declaration != null) && (declaration.Declaration != null))
			{
				result.Number = declaration.Declaration.JE_DeclarationReference.IsEmpty ? null : declaration.Declaration.JE_DeclarationReference.ToString();
				result.HouseBill = declaration.Declaration.JE_HouseBill.IsEmpty ? null : declaration.Declaration.JE_HouseBill.ToString();

				result.Consignee = new OrganisationValueObjectDataAdapter().ExportToValueObject(declaration.Declaration.Consignee, context);
				result.Shipper = new OrganisationValueObjectDataAdapter().ExportToValueObject(declaration.Declaration.Consignor, context);

				if (declaration.Declaration.JE_CartageCompleted.IsValid)
				{
					result.DeliveredDate = declaration.Declaration.JE_CartageCompleted.ToDateTime();
				}
				if (declaration.Declaration.JE_DateAtFinalDestination.IsValid)
				{
					result.ETA = declaration.Declaration.JE_DateAtFinalDestination.ToDateTime();
				}
				if (declaration.Declaration.JE_DateAtOrigin.IsValid)
				{
					result.ETD = declaration.Declaration.JE_DateAtOrigin.ToDateTime();
				}

				result.Origin = Xsd.UNLOCO.FromPort(declaration.Declaration.Origin);
				result.Destination = Xsd.UNLOCO.FromPort(declaration.Declaration.FinalDestination);

				result.GoodsDescription = declaration.Declaration.JE_GoodsDescription.IsEmpty ? null : declaration.Declaration.JE_GoodsDescription.ToString();
				result.Containers = new CusContainerValueObjectDataAdapter().ExportToXmlValueObjectCollection(declaration.Declaration.CusContainers, context);
				result.Notes = new NoteValueObjectDataAdapter().ExportToXmlValueObjectCollection(declaration.Declaration.Notes, context);
				result.Orders = new TrackingOrderSummaryValueObjectDataAdapter().ExportToXmlValueObjectCollection(declaration.Declaration.AttachedOrders, context);

				result.ServiceLevel = declaration.Declaration.JE_RS_NKServiceLevel.IsEmpty ? null : declaration.Declaration.JE_RS_NKServiceLevel.ToString();

				result.Quantity = Xsd.DimensionValue.FromAmountAndUnit(declaration.Declaration.JE_TotalNoOfPacks, declaration.Declaration.JE_TotalNoOfPacksPackType);
				result.Size = Xsd.DimensionValue.FromAmountAndUnit(declaration.Declaration.JE_TotalVolume, declaration.Declaration.JE_TotalVolumeUnit);
				result.Weight = Xsd.DimensionValue.FromAmountAndUnit(declaration.Declaration.JE_TotalWeight, declaration.Declaration.JE_TotalWeightUnit);

				if (declaration.DocumentHelper.Documents.Count > 0)
				{
					result.DocumentLinks = new DocumentLinkValueObjectDataAdapter().ExportToXmlValueObjectCollection(declaration.DocumentHelper.Documents, context);
				}
			}
		}
	}
}
