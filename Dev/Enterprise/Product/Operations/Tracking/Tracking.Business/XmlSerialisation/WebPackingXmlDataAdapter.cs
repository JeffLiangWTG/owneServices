using System;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.XmlSerialisation
{
	/// <summary>
	/// Xml Data adapter for Packings on Shipments, delcarations and orders
	/// </summary>
	public class WebPackingXmlDataAdapter : ValueObjectDataAdapter<TrackingPackLine, Xsd.WebPacking>
	{
		protected override Type ValueObjectCollectionType
		{
			get { return typeof(Xsd.WebPackingCollection); }
		}

		public override string RootCollectionElementName
		{
			get { return "WebPackings"; }
		}

		public override string RootElementName
		{
			get { return "WebPacking"; }
		}

		public override System.Xml.Schema.XmlSchema Schema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebPackingSchema; }
		}

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { return WebServicesXmlSchemaDefinitions.Instance.WebPackingsSchema; }
		}

		protected override TrackingPackLine FindBusinessObject(Xsd.WebPacking value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		protected override void ImportFromValueObjectCore(TrackingPackLine bizObj, Xsd.WebPacking value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		public override TrackingPackLine CreateOrUpdateFromValueObject(Xsd.WebPacking value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		public Xsd.WebPackingCollection ExportToValueObjectCollection(TrackingPackLineCollection collection, IValueObjectExportContext context)
		{
			return (Xsd.WebPackingCollection)ToValueObjectCollection(collection, context);
		}

		protected override void ExportToValueObjectCore(TrackingPackLine packLine, Xsd.WebPacking result, IValueObjectExportContext context)
		{
			if (packLine != null)
			{
				if (!packLine.JL_Calc_ContainerNumber.IsEmpty)
				{
					result.ContainerNumber = packLine.JL_Calc_ContainerNumber;
				}

				if (!packLine.JL_Description.IsEmpty)
				{
					result.Description = packLine.JL_Description;
				}

				result.LinePrice = Xsd.FinancialValue.FromAmountAndCurrency(packLine.JL_LinePrice, packLine.Currency);
				if (!packLine.JL_F3_NKPackType.IsEmpty)
				{
					result.PackType = packLine.JL_F3_NKPackType;
				}

				result.Volume = Xsd.DimensionValue.FromAmountAndUnit(packLine.JL_ActualVolume, packLine.JL_ActualVolumeUQ);
				result.Weight = Xsd.DimensionValue.FromAmountAndUnit(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ);
			}
		}
	}
}
