using System;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.Declaration
{
	public class CusContainerValueObjectDataAdapter : ValueObjectDataAdapter<BaseCusContainer, Xsd.WebContainer>
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

		protected override BaseCusContainer FindBusinessObject(Xsd.WebContainer value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		protected override void ImportFromValueObjectCore(BaseCusContainer bizObj, Xsd.WebContainer value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		public override BaseCusContainer CreateOrUpdateFromValueObject(Xsd.WebContainer value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		public Xsd.WebContainerCollection ExportToXmlValueObjectCollection(ICusContainerCollection<BaseCusContainer> collection, IValueObjectExportContext context)
		{
			return (Xsd.WebContainerCollection)ToValueObjectCollection(collection, context);
		}

		protected override void ExportToValueObjectCore(BaseCusContainer container, Xsd.WebContainer result, IValueObjectExportContext context)
		{
			if (container != null)
			{
				result.ContainerNumber = container.CO_ContainerNumber;
				if (!container.CO_FCL_LCL_AIR.IsEmpty)
				{
					result.DeliveryMode = container.CO_FCL_LCL_AIR;
				}

				if (!container.CO_Seal.IsEmpty)
				{
					result.SealNumber = container.CO_Seal;
				}

				result.Weight = Xsd.DimensionValue.FromAmountAndUnit(container.CO_Weight, container.CO_WeightUQ);
			}
		}
	}
}
