using System;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.XmlSerialisation
{
	/// <summary>
	/// Xml Data adapter for Containers on Shipments, delcarations and orders
	/// </summary>
	public class WebContainerXmlDataAdapter : ValueObjectDataAdapter<CommonContainer, Xsd.WebContainer>
	{
		protected override Type ValueObjectCollectionType
		{
			get { return typeof(Xsd.WebContainerCollection); }
		}

		public override string RootCollectionElementName
		{
			get { return "WebContainers"; }
		}

		public override string RootElementName
		{
			get { return "WebContainer"; }
		}

		public override System.Xml.Schema.XmlSchema Schema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebContainerSchema; }
		}

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebContainersSchema; }
		}

		protected override CommonContainer FindBusinessObject(Xsd.WebContainer value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		protected override void ImportFromValueObjectCore(CommonContainer bizObj, Xsd.WebContainer value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		public override CommonContainer CreateOrUpdateFromValueObject(Xsd.WebContainer value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		public Xsd.WebContainerCollection ExportToValueObjectCollection(CommonContainer[] collection, IValueObjectExportContext context)
		{
			return (Xsd.WebContainerCollection)ToValueObjectCollections(collection, context);
		}

		protected override void ExportToValueObjectCore(CommonContainer container, Xsd.WebContainer result, IValueObjectExportContext context)
		{
			if (container != null)
			{
				result.ContainerNumber = container.JC_ContainerNum;
				if (!container.JC_DeliveryMode.IsEmpty)
				{
					result.DeliveryMode = container.JC_DeliveryMode;
				}

				if (!container.JC_ContainerMode.IsEmpty)
				{
					result.Mode = Freight.DataTransfer.TransportModeToXmlCodeMappings.Instance.GetExternalCode(container.JC_ContainerMode, "", context);
					result.ModeSpecified = true;
				}
				if (container.JC_ContainerCount > 0)
				{
					result.NumberOfContainers = container.JC_ContainerCount.ToString();
				}

				if (!container.JC_SealNum.IsEmpty)
				{
					result.SealNumber = container.JC_SealNum;
				}

				result.Weight = Xsd.DimensionValue.FromAmountAndUnit(container.JC_GrossWeight, container.JC_GrossWeightUQ);
			}
		}
	}
}
