using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Integration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	public class USProductValueObjectDataAdapter : CustomsProductValueObjectDataAdapter, Integration.Customs.US.IUSProductValueObjectDataAdapter
	{
		public USProductValueObjectDataAdapter() { }

		#region Import

		protected override void ImportCountryClassification(Customs.Business.BaseCusClassPartPivot pivot, Xsd.ClassificationCountryClassifications xmlCountryClassifications, IValueObjectImportContext context)
		{
			var usClassification = xmlCountryClassifications.USClassification;
			ImportUSClassification(pivot, usClassification, context);
		}

		void ImportUSClassification(Customs.Business.BaseCusClassPartPivot pivot, Xsd.USProductClassification usClassification, IValueObjectImportContext context)
		{
			var usPivot = (CusClassPartPivot)pivot;

			if (usClassification.Manufacturer.IsSpecified)
			{
				AdaptersHelper.FindOrCreateOrgFromMID(usClassification.Manufacturer.Item, context, usPivot.CD_OA_ManufacturerInfo);
			}

			context.SetPropertyInfoValueWithinMaxLengthAndWarn(usPivot.CD_UC_NKCountryOfOriginInfo, usClassification.CountryOfOrigin, usClassification.CountryOfOriginSpecified);

			if (usClassification.NAFTANetCostSpecified)
			{
				usPivot.CD_NAFTANetCost = usClassification.NAFTANetCost == Xsd.TrueFalse.@true;
			}

			context.SetPropertyInfoValueWithinMaxLengthAndWarn(usPivot.CD_AgricultureLicenceNoInfo, usClassification.PermitsLicenses.AgricultureLicNo, usClassification.PermitsLicenses.AgricultureLicNoSpecified);
			context.SetPropertyInfoValueWithinMaxLengthAndWarn(usPivot.CD_SugarCertificateInfo, usClassification.PermitsLicenses.CAExportCertificate, usClassification.PermitsLicenses.CAExportCertificateSpecified);
			context.SetPropertyInfoValueWithinMaxLengthAndWarn(usPivot.CD_CBTPACertificateInfo, usClassification.PermitsLicenses.CBTPACertificate, usClassification.PermitsLicenses.CBTPACertificateSpecified);
			context.SetPropertyInfoValueWithinMaxLengthAndWarn(usPivot.CD_CottonFeeExemptInfo, usClassification.PermitsLicenses.CottonFeeExemptIndicator, usClassification.PermitsLicenses.CottonFeeExemptIndicatorSpecified);

			if (usClassification.PermitsLicenses.CottonFeeExemptIndicator != "Y")
			{
				context.SetPropertyInfoValueWithinMaxLengthAndWarn(usPivot.CD_CottonCertificateInfo, usClassification.PermitsLicenses.CottonCertificateNo, usClassification.PermitsLicenses.CottonCertificateNoSpecified);
			}

			context.SetPropertyInfoValueWithinMaxLengthAndWarn(usPivot.CD_MiscLicenceNoInfo, usClassification.PermitsLicenses.MiscPermitNo, usClassification.PermitsLicenses.MiscPermitNoSpecified);
			context.SetPropertyInfoValueWithinMaxLengthAndWarn(usPivot.CD_WoolLicenceNoInfo, usClassification.PermitsLicenses.WoolLicenceNo, usClassification.PermitsLicenses.WoolLicenceNoSpecified);
			context.SetPropertyInfoValueWithinMaxLengthAndWarn(usPivot.CD_ADDCaseNoInfo, usClassification.AntiDumping.CaseNo, usClassification.AntiDumping.CaseNoSpecified);
			context.SetPropertyInfoValueWithinMaxLengthAndWarn(usPivot.CD_CVDCaseNoInfo, usClassification.Countervailing.CaseNo, usClassification.Countervailing.CaseNoSpecified);
			context.SetPropertyInfoValueWithinMaxLengthAndWarn(usPivot.CD_RulingNumberInfo, usClassification.PIRPRuling.Number, usClassification.PIRPRuling.NumberSpecified);
			context.SetPropertyInfoValueWithinMaxLengthAndWarn(usPivot.CD_RulingTypeInfo, usClassification.PIRPRuling.Type, usClassification.PIRPRuling.TypeSpecified);
			context.SetPropertyInfoValueWithinMaxLengthAndWarn(usPivot.CD_ProductClaimInfo, usClassification.ProductClaim, usClassification.ProductClaimSpecified);
			context.SetPropertyInfoValueWithinMaxLengthAndWarn(usPivot.CD_SPIInfo, usClassification.SPI, usClassification.SPISpecified);

			if (usClassification.PerUnit.IsSpecified)
			{
				if (usClassification.PerUnit.AMMVPerUnitSpecified)
				{
					usPivot.CD_AMMVPerUnit = usClassification.PerUnit.AMMVPerUnit;
				}
				if (usClassification.PerUnit.CostPerUnitSpecified)
				{
					usPivot.CD_PerUnitCost = usClassification.PerUnit.CostPerUnit.Value;
					usPivot.CD_RX_NKPerUnitCostCurr = usClassification.PerUnit.CostPerUnit.CurrencyCode;
				}
				if (usClassification.PerUnit.US9802InInvCurrPerUnitSpecified)
				{
					usPivot.CD_9802ValuePerUnit = usClassification.PerUnit.US9802InInvCurrPerUnit.Value;
					usPivot.CD_RX_NK9802ValuePerUnitCurr = usClassification.PerUnit.US9802InInvCurrPerUnit.CurrencyCode;
				}
				if (usClassification.PerUnit.US9802PerUnitSpecified)
				{
					usPivot.CD_9802USDValuePerUnit = usClassification.PerUnit.US9802PerUnit;
				}
			}

			if (usClassification.Weight.IsSpecified)
			{
				if (usClassification.Weight.GrossSpecified)
				{
					usPivot.CD_GrossWeight = usClassification.Weight.Gross;
				}
				if (usClassification.Weight.NetSpecified)
				{
					usPivot.CD_NetWeight = usClassification.Weight.Net;
				}
				if (usClassification.Weight.UQSpecified)
				{
					usPivot.CD_WeightUQ = usClassification.Weight.UQ;
				}
			}

			if (usClassification.PercentageOfActiveIngredientSpecified)
			{
				usPivot.CD_ActiveIngredientPercentage = usClassification.PercentageOfActiveIngredient;
			}

			if (usClassification.ZoneStatusSpecified)
			{
				context.SetPropertyInfoValue(usPivot.CD_ZoneStatusInfo, ZoneStatusToXmlCodeMappings.Instance.GetEnterpriseCode(usClassification.ZoneStatus.ToString(), "", context), true, "Zone Status");
			}
			context.SetPropertyInfoValue(usPivot.CD_TSCAIndicatorInfo, usClassification.TSCAIndicator, usClassification.TSCAIndicatorSpecified);

			if (usClassification.OverriddenTaxRateSpecified || usClassification.TaxCodeSpecified)
			{
				usPivot.CD_TaxApplicability = TaxApplyList.Codes.Override;
				usPivot.CD_TaxCode = USProductClassificationTaxCodeMappings.Instance.GetEnterpriseCode(usClassification.TaxCode, "", context);
			}

			if (usClassification.TaxRateTypeSpecified)
			{
				usPivot.CD_TaxRateType = USProductClassificationTaxRateTypeMappings.Instance.GetEnterpriseCode(usClassification.TaxRateType, "", context);
			}

			if (usClassification.OverriddenTaxRateSpecified || usClassification.TaxCodeSpecified)
			{
				usPivot.CD_TaxRateDesc = AppendixBTaxRateList.Codes.Specify;
				usPivot.CD_TaxRate = usClassification.OverriddenTaxRate;
			}

			if (usClassification.LaceyActDetails.Count > 0)
			{
				usPivot.PGAs.RemoveAndDeleteAll();
				OGADataTransferTool.ImportLaceyActDetails(usClassification.LaceyActDetails, usPivot.PGAs, pivot.Factory, false, null);
			}
		}

		protected override Customs.Business.BaseCusClassification GetOrCreateLookup(Customs.Business.OrgSupplierPart product, Xsd.Classification xmlClassification, IValueObjectImportContext context, ZString countryCode)
		{
			return xmlClassification.LookupCode.Value.IsEmpty && !xmlClassification.Tariff.IsEmpty ? null : base.GetOrCreateLookup(product, xmlClassification, context, countryCode);
		}

		protected override void ProcessChildren(Customs.Business.BaseCusClassPartPivot pivot, Xsd.ClassificationChildCollection xmlchildClassifications, IValueObjectImportContext context)
		{
			var usPivot = (CusClassPartPivot)pivot;
			if (usPivot.IsImportClassification)
			{
				if (xmlchildClassifications != null)
				{
					foreach (Xsd.ClassificationChild xmlChild in xmlchildClassifications)
					{
						var childPivot = ((CusClassPartPivot)pivot).Children.AddNew();
						if (xmlChild.OrderSpecified)
						{
							childPivot.CI_ChildListOrder = (byte)xmlChild.Order;
						}

						childPivot.CI_ChildType = xmlChild.Type;
						childPivot.CI_TariffNum = xmlChild.Tariff;
						ImportUSClassification(childPivot, xmlChild.CountryClassifications.USClassification, context);
					}
				}
			}
		}

		protected override void ProcessAttributes(Customs.Business.BaseCusClassPartPivot pivot, Xsd.ClassificationAttributeCollection xmlAttributes, IValueObjectImportContext context)
		{
			var usPivot = (CusClassPartPivot)pivot;
			if (usPivot.IsImportClassification)
			{
				if (xmlAttributes != null)
				{
					foreach (Xsd.ClassificationAttribute xmlAttribute in xmlAttributes)
					{
						var attributeType = Xsd.ClassificationAttributeType.AT1;
						if (xmlAttribute.TypeSpecified)
						{
							attributeType = xmlAttribute.Type;
						}
						Customs.Business.CusAttributeFilterCollection attributes = null;
						switch (attributeType)
						{
							case Xsd.ClassificationAttributeType.AT1:
								attributes = pivot.Attributes1;
								break;
							case Xsd.ClassificationAttributeType.AT2:
								attributes = pivot.Attributes2;
								break;
							case Xsd.ClassificationAttributeType.AT3:
								attributes = pivot.Attributes3;
								break;
						}
						if (attributes != null)
						{
							var existingAttribute = attributes.FirstOrDefault(x => x.BG_AttributeValue1 == xmlAttribute.Value);
							if (existingAttribute == null)
							{
								var attribute = attributes.AddNew();
								attribute.BG_AttributeValue1 = xmlAttribute.Value;
							}
						}
					}
				}
			}
		}

		protected override bool ShouldCheckAttributes(Customs.Business.BaseCusClassPartPivot pivot)
		{
			var usPivot = (CusClassPartPivot)pivot;
			return usPivot.IsImportClassification;
		}

		#endregion

		#region Export

		protected override void ExportChildren(Customs.Business.BaseCusClassPartPivot pivot, Xsd.Classification xmlClassification, IValueObjectExportContext context)
		{
			var children = ((CusClassPartPivot)pivot).Children;
			if (children.Count > 0)
			{
				xmlClassification.Children = new Xsd.ClassificationChildCollection();

				foreach (CusClassPartPivot childPivot in children)
				{
					var xmlChild = xmlClassification.Children.AddNew();
					if (childPivot.CI_ChildListOrder > 0)
					{
						xmlChild.OrderSpecified = true;
						xmlChild.Order = (sbyte)((int)childPivot.CI_ChildListOrder);
					}
					xmlChild.Type = childPivot.CI_ChildType;
					xmlChild.Tariff = childPivot.CI_TariffNum;

					xmlChild.CountryClassifications = new Xsd.ClassificationChildCountryClassifications();
					ExportUSClassificationData(childPivot, xmlChild.CountryClassifications.USClassification, context);
				}
			}
		}

		protected override void ExportAttributes(Customs.Business.BaseCusClassPartPivot pivot, Xsd.Classification xmlClassification, IValueObjectExportContext context)
		{
			ExportAttributes(pivot.Attributes1, xmlClassification.Attributes, context);
			ExportAttributes(pivot.Attributes2, xmlClassification.Attributes, context);
			ExportAttributes(pivot.Attributes3, xmlClassification.Attributes, context);
		}

		void ExportAttributes(Customs.Business.CusAttributeFilterCollection attributes, Xsd.ClassificationAttributeCollection xmlAttributes, IValueObjectExportContext context)
		{
			if (attributes.Count > 0)
			{
				foreach (Customs.Business.CusAttributeFilter attribute in attributes)
				{
					var xmlAttribute = xmlAttributes.AddNew();
					xmlAttribute.Value = attribute.BG_AttributeValue1;
					xmlAttribute.Type = ClassificationAttributeTypeToXmlCodeMappings.Instance.GetExternalCode(attribute.BG_AttributeName, "Attribute Type", context);
				}
			}
		}

		protected override void ExportCountryClassificationData(Customs.Business.BaseCusClassPartPivot pivot, Xsd.Classification xmlClassification, IValueObjectExportContext context)
		{
			xmlClassification.CountryClassifications = new Xsd.ClassificationCountryClassifications();
			ExportUSClassificationData((CusClassPartPivot)pivot, xmlClassification.CountryClassifications.USClassification, context);
		}

		void ExportUSClassificationData(CusClassPartPivot usPivot, Xsd.USProductClassification usClassification, IValueObjectExportContext context)
		{
			usClassification.IsSpecified = true;
			usClassification.CountryOfOrigin = usPivot.CD_UC_NKCountryOfOrigin;

			OGADataTransferTool.ExportOrganisation(usClassification.Manufacturer, usPivot.ManufacturerAddress, context);

			#region OGA

			usClassification.LaceyActDetails = OGADataTransferTool.ExportLaceyActData(false, usPivot.PGAs);

			usClassification.OGAIndicators.IsSpecified = usClassification.OGAIndicators.FDAIndicatorSpecified || usClassification.OGAIndicators.FCCIndicatorSpecified || usClassification.OGAIndicators.DOTIndicatorSpecified;

			#endregion

			usClassification.NAFTANetCost = usPivot.CD_NAFTANetCost ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			usClassification.NAFTANetCostSpecified = true;

			usClassification.PermitsLicenses.AgricultureLicNo = usPivot.CD_AgricultureLicenceNo;
			usClassification.PermitsLicenses.CAExportCertificate = usPivot.CD_SugarCertificate;
			usClassification.PermitsLicenses.CBTPACertificate = usPivot.CD_CBTPACertificate;
			usClassification.PermitsLicenses.CottonCertificateNo = usPivot.CD_CottonCertificate;
			usClassification.PermitsLicenses.CottonFeeExemptIndicator = usPivot.CD_CottonFeeExempt;
			usClassification.PermitsLicenses.MiscPermitNo = usPivot.CD_MiscLicenceNo;
			usClassification.PermitsLicenses.WoolLicenceNo = usPivot.CD_WoolLicenceNo;
			usClassification.AntiDumping.CaseNo = usPivot.CD_ADDCaseNo;
			usClassification.Countervailing.CaseNo = usPivot.CD_CVDCaseNo;
			usClassification.PIRPRuling.Number = usPivot.CD_RulingNumber;
			usClassification.PIRPRuling.Type = usPivot.CD_RulingType;
			usClassification.ProductClaim = usPivot.CD_ProductClaim;
			usClassification.SPI = usPivot.CD_SPI;

			usClassification.PerUnit = new Xsd.USProductClassificationPerUnit();
			usClassification.PerUnit.AMMVPerUnit = usPivot.CD_AMMVPerUnit;
			usClassification.PerUnit.CostPerUnit = Xsd.FinancialValue.FromAmountAndCurrencyCode(usPivot.CD_PerUnitCost, usPivot.CD_RX_NKPerUnitCostCurr);
			usClassification.PerUnit.US9802PerUnit = usPivot.CD_9802USDValuePerUnit;
			usClassification.PerUnit.US9802InInvCurrPerUnit = Xsd.FinancialValue.FromAmountAndCurrencyCode(usPivot.CD_9802ValuePerUnit, usPivot.CD_RX_NK9802ValuePerUnitCurr);

			usClassification.Weight = new Xsd.USProductClassificationWeight();
			usClassification.Weight.Gross = usPivot.CD_GrossWeight;
			usClassification.Weight.Net = usPivot.CD_NetWeight;
			usClassification.Weight.UQ = usPivot.CD_WeightUQ;

			if (usPivot.CD_TaxApplicability == TaxApplyList.Codes.Override)
			{
				usClassification.TaxCode = USProductClassificationTaxCodeMappings.Instance.GetExternalCode(usPivot.CD_TaxCode, "", context);
				usClassification.OverriddenTaxRate = usPivot.CD_TaxRate;
			}

			if (!usPivot.CD_TaxRateType.IsEmpty)
			{
				usClassification.TaxRateType = USProductClassificationTaxRateTypeMappings.Instance.GetExternalCode(usPivot.CD_TaxRateType, "", context);
			}

			if (!usPivot.CD_ActiveIngredientPercentage.IsEmpty)
			{
				usClassification.PercentageOfActiveIngredient = usPivot.CD_ActiveIngredientPercentage;
				usClassification.PercentageOfActiveIngredientSpecified = true;
			}

			if (!usPivot.CD_ZoneStatus.IsEmpty)
			{
				usClassification.ZoneStatus = ZoneStatusToXmlCodeMappings.Instance.GetExternalCode(usPivot.CD_ZoneStatus, "", context);
				usClassification.ZoneStatusSpecified = true;
			}
			usClassification.TSCAIndicator = usPivot.CD_TSCAIndicator;
		}

		#endregion

		#region Related Objects

		public AdaptersHelper AdaptersHelper
		{
			get { return adaptersHelper ?? (adaptersHelper = new AdaptersHelper()); }
		}
		AdaptersHelper adaptersHelper;

		OGADataTransferTool OGADataTransferTool
		{
			get { return ogaDataTransferTool ?? (ogaDataTransferTool = new OGADataTransferTool()); }
		}
		OGADataTransferTool ogaDataTransferTool;

		#endregion
	}
}
