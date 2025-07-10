using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class CustomsProductValueObjectDataAdapter : ProductValueObjectDataAdapter, Integration.Customs.Shared.ICustomsProductValueObjectDataAdapter
	{
		public CustomsProductValueObjectDataAdapter() { }

		#region Import

		protected override void ImportFromValueObjectCore(MasterFiles.Business.OrgSupplierPart product, Xsd.Product value, IValueObjectImportContext context)
		{
			base.ImportFromValueObjectCore(product, value, context);

			var customsProduct = (OrgSupplierPart)product;
			if (!SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.Value)
			{
				customsProduct.PivotsForBinding.RemoveAndDeleteAll();
			}

			foreach (Xsd.Classification classification in value.Customs.Classifications)
			{
				ImportClassifications(customsProduct, classification, context);
			}
		}

		void ImportClassifications(OrgSupplierPart product, Xsd.Classification xmlClassification, IValueObjectImportContext context)
		{
			if (!xmlClassification.Tariff.IsEmpty || !xmlClassification.LookupCode.Value.IsEmpty)
			{
				var countryCode = GetCountryCode(product, xmlClassification, context);

				BaseCusClassPartPivot matchingPivot = null;

				//find matching lookup
				BaseCusClassification matchingLookup = GetOrCreateLookup(product, xmlClassification, context, countryCode);

				//if no match for pivot on child type, match based on classification
				if (matchingLookup != null)
				{
					//maybe only for non us -IMP BTH EXP
					foreach (BaseCusClassPartPivot pivot in product.PivotsForBinding)
					{
						if (pivot.CI_CC == matchingLookup.PK)
						{
							matchingPivot = pivot;
							break;
						}
					}
				}

				if (matchingPivot == null)
				{
					matchingPivot = FindPivotByKeyValues(xmlClassification, product.PivotsForBinding);
				}

				//else create a new one
				if (matchingPivot == null)
				{
					matchingPivot = (BaseCusClassPartPivot)product.Factory.New(new BaseCusClassPartPivotTypeDecider().GetTypeForCountryCode(countryCode));
					matchingPivot.CI_RN_NKCountry = countryCode;
					matchingPivot.CI_OP = product.PK;
					matchingPivot.CI_ChildType = xmlClassification.ClassificationType;

					if (xmlClassification.RelatedOrg.IsSpecified)
					{
						var relation = product.RelatedOrganisations.FindByOH_CodeAndRelationship(xmlClassification.RelatedOrg.Code, xmlClassification.RelatedOrg.Relationship.ToString());
						if (relation != null)
						{
							matchingPivot.CI_OH = relation.OU_OH;
						}
					}
					ProcessAttributes(matchingPivot, xmlClassification.Attributes, context);
				}

				//update lookup on pivot
				if (matchingLookup == null)
				{
					context.SetPropertyInfoValue(matchingPivot.CI_TariffNumInfo, xmlClassification.Tariff, xmlClassification.TariffSpecified);
				}
				else
				{
					matchingPivot.CI_CC = matchingLookup.PK;

					if (matchingPivot.CI_ChildType.IsEmpty)
					{
						matchingPivot.CI_ChildType = matchingLookup.CC_ClassificationType;
					}
				}
				context.SetPropertyInfoValue(matchingPivot.CI_UsageCommentInfo, xmlClassification.Name, xmlClassification.NameSpecified);
				context.SetPropertyInfoValue(matchingPivot.CI_SupplementalTariffInfo, xmlClassification.SupplementaryTariff, xmlClassification.SupplementaryTariffSpecified);
				ProcessChildren(matchingPivot, xmlClassification.Children, context);

				if (xmlClassification.CountryClassifications.IsSpecified)
				{
					ImportCountryClassification(matchingPivot, xmlClassification.CountryClassifications, context);
				}
			}
		}

		protected BaseCusClassPartPivot FindPivotByKeyValues(Xsd.Classification classification, ICusClassPartPivotCollection<BaseCusClassPartPivot> pivots)
		{
			return pivots.Count == 0 ? null : pivots.Cast<BaseCusClassPartPivot>().FirstOrDefault(x => IsKeyMatch(classification, x));
		}

		bool IsKeyMatch(Xsd.Classification classification, BaseCusClassPartPivot pivot)
		{
			var relatedOrganisationCode = pivot.RelatedOrganisation?.OH_Code ?? ZString.Empty;
			return IsClassificationTypeMatch(pivot, classification.ClassificationType) &&
				classification.CountryCode == pivot.CI_RN_NKCountry &&
				(relatedOrganisationCode.IsEmpty && !classification.RelatedOrg.IsSpecified ||
				!relatedOrganisationCode.IsEmpty && relatedOrganisationCode == classification.RelatedOrg.Code) &&
				IsKeyAttributesMatch(classification.Attributes, pivot);
		}

		bool IsClassificationTypeMatch(BaseCusClassPartPivot pivot, ZString classificationType)
		{
			return pivot.CI_ChildType == classificationType ||
				(pivot.Classification != null && pivot.Classification.CC_ClassificationType == classificationType);
		}

		bool IsKeyAttributesMatch(Xsd.ClassificationAttributeCollection attributes, BaseCusClassPartPivot pivot)
		{
			if (ShouldCheckAttributes(pivot))
			{
				var attribute1Match = attributes.GetAttributesByType(Xsd.ClassificationAttributeType.AT1).Length == pivot.Attributes1.Count;
				var attribute2Match = attributes.GetAttributesByType(Xsd.ClassificationAttributeType.AT2).Length == pivot.Attributes2.Count;
				var attribute3Match = attributes.GetAttributesByType(Xsd.ClassificationAttributeType.AT3).Length == pivot.Attributes3.Count;

				if (attribute1Match && attribute2Match && attribute3Match)
				{
					foreach (Xsd.ClassificationAttribute xmlAttribute in attributes)
					{
						switch (xmlAttribute.Type)
						{
							case Xsd.ClassificationAttributeType.AT1:
								attribute1Match = pivot.Attributes1.FirstOrDefault(x => x.BG_AttributeValue1 == xmlAttribute.Value) != null;
								break;
							case Xsd.ClassificationAttributeType.AT2:
								attribute2Match = pivot.Attributes2.FirstOrDefault(x => x.BG_AttributeValue1 == xmlAttribute.Value) != null;
								break;
							case Xsd.ClassificationAttributeType.AT3:
								attribute3Match = pivot.Attributes3.FirstOrDefault(x => x.BG_AttributeValue1 == xmlAttribute.Value) != null;
								break;
						}
					}
				}
				return attribute1Match && attribute2Match && attribute3Match;
			}
			return true;
		}

		protected virtual bool ShouldCheckAttributes(BaseCusClassPartPivot pivot)
		{
			return false;
		}

		protected virtual BaseCusClassification GetOrCreateLookup(OrgSupplierPart product, Xsd.Classification xmlClassification, IValueObjectImportContext context, ZString countryCode)
		{
			BaseCusClassification result = null;

			if (!xmlClassification.LookupCode.Value.IsEmpty)
			{
				result = FindEnterpriseLookup(product.Factory, xmlClassification.LookupCode.Value, xmlClassification.LookupCode.LookupType, countryCode);
			}

			if (result == null)
			{
				result = product.Factory.New<BaseCusClassification>();

				ZString lookupCode = xmlClassification.LookupCode.Value;
				if (lookupCode.IsEmpty)
				{
					lookupCode = ZGuid.NewZGuid().ToString();
					lookupCode = lookupCode.Replace("-", "");
					result.CC_IsUnpublished = true;
				}
				result.CC_LookupCode = lookupCode;
				result.CC_RN_NKCountryCode = countryCode;
				result.CC_TariffNum = xmlClassification.Tariff;

				var lookupType = xmlClassification.LookupCode.LookupType;
				result.CC_ClassificationType = lookupType.IsEmpty ? GetClassificationTypeFromPivotChildType(xmlClassification.ClassificationType, countryCode) : lookupType;

				var description = xmlClassification.Description;
				result.CC_Description = description.IsEmpty ? xmlClassification.Tariff : description;
			}

			return result;
		}

		ZString GetClassificationTypeFromPivotChildType(ZString pivotChildType, ZString countryCode)
		{
			if (countryCode == Core.Constants.CountryCodes.Singapore)
			{
				return "SG4";
			}

			if (pivotChildType == BaseCusClassification.ClassificationType.EXP
				|| pivotChildType == BaseCusClassification.ClassificationType.IMP
				|| pivotChildType == BaseCusClassification.ClassificationType.Both)
			{
				return pivotChildType;
			}

			if (pivotChildType == Business.ClassificationTypeList.Codes.HTE)
			{
				return BaseCusClassification.ClassificationType.EXP;
			}
			if (pivotChildType == Business.ClassificationTypeList.Codes.HTI)
			{
				return BaseCusClassification.ClassificationType.IMP;
			}

			return BaseCusClassification.ClassificationType.Both;
		}

		ZString GetCountryCode(OrgSupplierPart product, Xsd.Classification xmlClassification, IValueObjectImportContext context)
		{
			ZString result = MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			if (!xmlClassification.CountryCode.IsEmpty)
			{
				MasterFiles.Business.RefCountry country = MasterFiles.Business.RefCountry.LoadFromCountryCode(product.Factory, xmlClassification.CountryCode);
				if (country == null)
				{
					ErrorNotification errorNotification = new ErrorNotification(ErrorType.DataErrorPreventSave, Res.GetString("723dd3e2-f9b0-46ed-993e-2b8749f3c673", "Invalid Country Code '{0}'", xmlClassification.CountryCode));
					context.Add(errorNotification);
				}
				else
				{
					result = country.Code;
				}
			}

			return result;
		}

		protected virtual void ProcessChildren(BaseCusClassPartPivot pivot, Xsd.ClassificationChildCollection xmlchildClassifications, IValueObjectImportContext context)
		{
		}

		protected virtual void ProcessAttributes(BaseCusClassPartPivot pivot, Xsd.ClassificationAttributeCollection xmlAttributes, IValueObjectImportContext context)
		{
		}

		protected virtual void ImportCountryClassification(BaseCusClassPartPivot pivot, Xsd.ClassificationCountryClassifications xmlLookupCountryClassifications, IValueObjectImportContext context)
		{
		}

		BaseCusClassification FindEnterpriseLookup(BusinessObjectFactory factory, ZString lookupCode, ZString classificationType, ZString countryCode)
		{
			ZQuery classificationQuery = new ZQuery(CusClassificationSchema.CC_LookupCode, lookupCode);
			classificationQuery.AddToFilter(CusClassificationSchema.CC_ClassificationType, classificationType);
			classificationQuery.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, countryCode);

			return factory.LoadTop1<BaseCusClassification>(classificationQuery);
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(MasterFiles.Business.OrgSupplierPart product, Xsd.Product result, IValueObjectExportContext context)
		{
			base.ExportToValueObjectCore(product, result, context);
			result.Customs = new Xsd.ProductCustoms();

			OrgSupplierPart part = product as OrgSupplierPart;
			if (part != null)
			{
				foreach (BaseCusClassPartPivot pivot in part.PivotsForBinding)
				{
					ExportClassification(part, pivot, result.Customs.Classifications, context);
				}
			}
		}

		void ExportClassification(OrgSupplierPart customsProduct, BaseCusClassPartPivot pivot, Xsd.ClassificationCollection xmlClassifications, IValueObjectExportContext context)
		{
			Xsd.Classification xmlClassification = xmlClassifications.AddNew();
			xmlClassification.ClassificationType = pivot.CI_ChildType;
			xmlClassification.CountryCode = pivot.CI_RN_NKCountry;
			xmlClassification.Name = pivot.CI_UsageComment;

			var relatedOrganisation = pivot.RelatedOrganisation;
			if (relatedOrganisation != null)
			{
				xmlClassification.RelatedOrg.Code = relatedOrganisation.OH_Code;
				xmlClassification.RelatedOrg.Relationship = RelationshipToXmlCodeMappings.Instance.GetExternalCode(pivot.CI_ChildType, "", context);
			}

			xmlClassification.SupplementaryTariff = pivot.CI_SupplementalTariff;

			if (pivot.Classification != null)
			{
				xmlClassification.LookupCode = new Xsd.ClassificationLookupCode();
				xmlClassification.LookupCode.Value = pivot.Classification.CC_LookupCode;
				xmlClassification.LookupCode.LookupType = pivot.Classification.CC_ClassificationType;
				xmlClassification.Description = pivot.Classification.CC_Description;
				xmlClassification.Tariff = pivot.Classification.CC_TariffNum;
				if (xmlClassification.ClassificationType.IsEmpty)
				{
					xmlClassification.ClassificationType = pivot.Classification.CC_ClassificationType;
				}
			}

			if (xmlClassification.Tariff.IsEmpty)
			{
				xmlClassification.Tariff = pivot.CI_TariffNum;
			}

			ExportChildren(pivot, xmlClassification, context);
			ExportAttributes(pivot, xmlClassification, context);

			ExportCountryClassificationData(pivot, xmlClassification, context);
		}

		protected virtual void ExportCountryClassificationData(BaseCusClassPartPivot pivot, Xsd.Classification xmlClassification, IValueObjectExportContext context)
		{
		}

		protected virtual void ExportChildren(BaseCusClassPartPivot pivot, Xsd.Classification xmlClassification, IValueObjectExportContext context)
		{
		}

		protected virtual void ExportAttributes(BaseCusClassPartPivot pivot, Xsd.Classification xmlClassification, IValueObjectExportContext context)
		{
		}

		#endregion
	}
}
