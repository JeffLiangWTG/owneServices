using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.DataTransfer;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public class InnerPackLineValueObjectDataAdapter<TBusinessObject, TValueObject> : ValueObjectDataAdapter<TBusinessObject, TValueObject>
		where TBusinessObject : PackLine
		where TValueObject : Xsd.PackageBase
	{
		#region Implementation

		public override string RootCollectionElementName { get { return "InnerPackages"; } }
		public override string RootElementName { get { return "InnerPackage"; } }
		public override XmlSchema Schema { get { return FreightXmlSchemaDefinitions.Instance.SingleInnerPackage; } }
		public override XmlSchema CollectionSchema { get { return null; } }

		#endregion

		#region Business Objects

		protected override TBusinessObject FindBusinessObject(TValueObject value, IValueObjectImportContext context)
		{
			return null;
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notify, BusinessObject bizObj)
		{
			// don't do this, the user doesn't care if a pack line is created/updated
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(TBusinessObject packline, TValueObject value, IValueObjectImportContext context)
		{
			string packlineContext = Res.GetString("f10a6e5c-f7c2-4ea2-8a89-5ece9c679c61", "The Package type ({0}) on a Pack line of {1}", value.PackType, (packline.Shipment != null ? packline.Shipment.HumanReadableName : ZString.Empty));
			CommonDefinedResourceStrings.SetPackageTypeProperty(packline.JL_F3_NKPackTypeInfo, value.PackType, packlineContext, context, value.IsSpecified);

			packline.JL_PackageCount = (int)value.NumberOfPacks;
			context.SetPropertyInfoValue(packline.JL_DescriptionInfo, value.GoodsDescription, value.GoodsDescriptionSpecified);
			context.SetPropertyInfoValue(packline.JL_RefNumberInfo, value.RefNumber, value.RefNumberSpecified);

			ImportDimensions(packline, value, context, packlineContext);
			ImportWeightVolume(packline, value, context, packlineContext);
		}

		void ImportDimensions(PackLine packline, Xsd.PackageBase packageValue, IValueObjectImportContext context, string errorContext)
		{
			bool isPackageDimensionTypesSame =
				(packageValue.Length != null && packageValue.Length.DimensionType != packageValue.Width.DimensionType) ||
				(packageValue.Width != null && packageValue.Width.DimensionType != packageValue.Height.DimensionType) ||
				(packageValue.Height != null && packageValue.Height.DimensionType != packageValue.Length.DimensionType);

			if (isPackageDimensionTypesSame)
			{
				context.Notify(new ErrorNotification(FreightErrorType.LengthWidthHeightDimensionTypeMustBeSame, errorContext));
			}

			if (packageValue.Length != null)
			{
				context.SetPropertyInfoValue(packline.JL_UnitOfDimensionInfo, DimensionUQXmlCodeMappings.Instance.GetEnterpriseCode(packageValue.Length.DimensionType, errorContext, context), packageValue.Length.DimensionTypeSpecified);
				context.SetPropertyInfoValue(packline.JL_LengthInfo, packageValue.Length.Value, JobPackLinesSchema.JL_Length);
			}
			if (packageValue.Width != null)
			{
				context.SetPropertyInfoValue(packline.JL_UnitOfDimensionInfo, DimensionUQXmlCodeMappings.Instance.GetEnterpriseCode(packageValue.Width.DimensionType, errorContext, context), packageValue.Width.DimensionTypeSpecified);
				context.SetPropertyInfoValue(packline.JL_WidthInfo, packageValue.Width.Value, JobPackLinesSchema.JL_Width);
			}
			if (packageValue.Height != null)
			{
				context.SetPropertyInfoValue(packline.JL_UnitOfDimensionInfo, DimensionUQXmlCodeMappings.Instance.GetEnterpriseCode(packageValue.Height.DimensionType, errorContext, context), packageValue.Height.DimensionTypeSpecified);
				context.SetPropertyInfoValue(packline.JL_HeightInfo, packageValue.Height.Value, JobPackLinesSchema.JL_Height);
			}
		}

		void ImportWeightVolume(PackLine packline, Xsd.PackageBase packageValue, IValueObjectImportContext context, string errorContext)
		{
			if (packageValue.Weight != null)
			{
				context.SetPropertyInfoValue(packline.JL_ActualWeightUQInfo, WeightUQXmlCodeMappings.Instance.GetEnterpriseCode(packageValue.Weight.DimensionType, errorContext, context), packageValue.Weight.DimensionTypeSpecified);
				context.SetPropertyInfoValue(packline.JL_ActualWeightInfo, packageValue.Weight.Value, JobPackLinesSchema.JL_ActualWeight);
			}
			if (packageValue.Volume != null)
			{
				context.SetPropertyInfoValue(packline.JL_ActualVolumeUQInfo, VolumeUQXmlCodeMappings.Instance.GetEnterpriseCode(packageValue.Volume.DimensionType, errorContext, context), packageValue.Volume.DimensionTypeSpecified);
				context.SetPropertyInfoValue(packline.JL_ActualVolumeInfo, packageValue.Volume.Value, JobPackLinesSchema.JL_ActualVolume);
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(TBusinessObject packline, TValueObject value, IValueObjectExportContext context)
		{
			string consolNumber = packline.CurrentConsol != null ? packline.CurrentConsol.ToString() : "";
			string shipmentNumber = packline.Shipment != null ? packline.Shipment.JS_UniqueConsignRef : ZString.Empty;
			string errorContext = Res.GetString("6f62e7a3-48d0-483c-b85c-dfc072b8e900", "Pack line on master bill {0}", consolNumber);

			if (packline.JL_F3_NKPackType.IsEmpty)
			{
				ZString packType = packline.JL_FreightMode == FreightConstants.OuterPackType ? Res.GetString("97b5f519-0eb7-4718-a5dd-848be76550d6", "outer") : Res.GetString("ad920ea1-9b6e-48d4-aafc-b137a1728919", "inner");
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("49f8d226-7388-4f23-8c6d-f6dd94e6add6", "Pack type is not specified for {0} packline on shipment {1}", packType, shipmentNumber)));
			}
			else
			{
				string externalPackType = PkgUnitXmlCodeMappings.Instance.GetExternalCode(packline.JL_F3_NKPackType, errorContext, context);
				if (externalPackType != null)
				{
					value.PackType = externalPackType;
				}
				else
				{
					string unknownExternalCodeErrorMessage = Res.GetString("83cb6c81-7ccd-4710-a7af-5d299017bc42", "A non-system defined package type ({0}) on shipment {1} has been exported. The organization that imports this XML file may not have that package type in their registry (they can add it in Registry -> Freight -> Shipment -> Packages -> Freight Packs. If they do not have the package type, their import will not fail, but the record will have an error on the package type field when they try to edit it.", packline.JL_F3_NKPackType, shipmentNumber);
					context.Notify(new WarningNotification(unknownExternalCodeErrorMessage));

					value.PackType = packline.JL_F3_NKPackType;
				}
			}

			value.NumberOfPacks = (uint)(int)packline.JL_PackageCount;

			if (!packline.JL_Description.IsEmpty)
			{
				value.GoodsDescription = packline.JL_Description;
			}

			if (!packline.JL_RefNumber.IsEmpty)
			{
				value.RefNumber = packline.JL_RefNumber;
			}

			ExportDimensions(packline, value, context, errorContext);
			ExportWeightVolume(packline, value, context, errorContext);
		}

		void ExportDimensions(PackLine packLine, Xsd.PackageBase packageValue, IValueObjectExportContext context, string errorContext)
		{
			packageValue.Length = Xsd.DimensionValue.FromAmountAndUnit(packLine.JL_Length, DimensionUQXmlCodeMappings.Instance.GetExternalCode(packLine.JL_UnitOfDimension, errorContext, context));
			packageValue.Width = Xsd.DimensionValue.FromAmountAndUnit(packLine.JL_Width, DimensionUQXmlCodeMappings.Instance.GetExternalCode(packLine.JL_UnitOfDimension, errorContext, context));
			packageValue.Height = Xsd.DimensionValue.FromAmountAndUnit(packLine.JL_Height, DimensionUQXmlCodeMappings.Instance.GetExternalCode(packLine.JL_UnitOfDimension, errorContext, context));
		}

		void ExportWeightVolume(PackLine packLine, Xsd.PackageBase packageValue, IValueObjectExportContext context, string errorContext)
		{
			packageValue.Weight = Xsd.DimensionValue.FromAmountAndUnit(packLine.JL_ActualWeight, WeightUQXmlCodeMappings.Instance.GetExternalCode(packLine.JL_ActualWeightUQ, errorContext, context));
			packageValue.Volume = Xsd.DimensionValue.FromAmountAndUnit(packLine.JL_ActualVolume, VolumeUQXmlCodeMappings.Instance.GetExternalCode(packLine.JL_ActualVolumeUQ, errorContext, context));
		}

		#endregion
	}
}
