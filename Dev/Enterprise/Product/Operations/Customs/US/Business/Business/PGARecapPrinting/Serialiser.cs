using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.PGARecapPrinting
{
	public static class Serialiser
	{
		public static KeyValuePair<ZString, ZString> CreateValue(ZString prefix, ZString value)
		{
			return new KeyValuePair<ZString, ZString>(prefix, value);
		}

		public static string CreateLine(bool isEntryLine, params KeyValuePair<ZString, ZString>[] values)
		{
			var builder = new ZStringBuilder();
			if (values != null)
			{
				foreach (var pair in values)
				{
					builder.AppendIfNotEmpty(pair.Key, pair.Value.Trim());
				}
			}
			var result = builder.ToStringWithDelimiterBetweenAppends("   ");
			if (!isEntryLine && !string.IsNullOrEmpty(result))
			{
				result = result.Insert(0, EntityPadding);
			}
			return result;
		}

		public static ZString AddAdditionalInfo(this IEnumerable<AEPAPG60> pg60s, ZString data, ZString type)
		{
			var result = new ZStringBuilder();
			result.AppendIfNotEmpty(data);
			if (pg60s != null)
			{
				foreach (var pg60 in pg60s.Where(x => x.AdditionalInformationQualifierCode == type))
				{
					result.AppendIfNotEmpty(pg60.AdditionalInformation);
				}
			}
			return result.ToString();
		}

		public enum DelimitedType { Space, Hyphen, Parenthesis }

		public static string AddAdditionalRole(this IEnumerable<AEPAPG55> pg55s, ZString role, EntityRoleCodeList entityRoleCodeList)
		{
			var roles = new ZStringBuilder();
			roles.AppendIfNotEmpty(AppendDescription(role, entityRoleCodeList, DelimitedType.Hyphen));
			if (pg55s != null)
			{
				foreach (var pg55 in pg55s)
				{
					roles.AppendIfNotEmpty(AppendDescription(pg55.EntityRoleCode, entityRoleCodeList, DelimitedType.Hyphen));
					roles.AppendIfNotEmpty(AppendDescription(pg55.EntityRoleCode1, entityRoleCodeList, DelimitedType.Hyphen));
					roles.AppendIfNotEmpty(AppendDescription(pg55.EntityRoleCode2, entityRoleCodeList, DelimitedType.Hyphen));
					roles.AppendIfNotEmpty(AppendDescription(pg55.EntityRoleCode3, entityRoleCodeList, DelimitedType.Hyphen));
					roles.AppendIfNotEmpty(AppendDescription(pg55.EntityRoleCode4, entityRoleCodeList, DelimitedType.Hyphen));
					roles.AppendIfNotEmpty(AppendDescription(pg55.EntityRoleCode5, entityRoleCodeList, DelimitedType.Hyphen));
					roles.AppendIfNotEmpty(AppendDescription(pg55.EntityRoleCode6, entityRoleCodeList, DelimitedType.Hyphen));
					roles.AppendIfNotEmpty(AppendDescription(pg55.EntityRoleCode7, entityRoleCodeList, DelimitedType.Hyphen));
					roles.AppendIfNotEmpty(AppendDescription(pg55.EntityRoleCode8, entityRoleCodeList, DelimitedType.Hyphen));
					roles.AppendIfNotEmpty(AppendDescription(pg55.EntityRoleCode9, entityRoleCodeList, DelimitedType.Hyphen));
				}
			}
			return roles.IsEmpty ? null : CreateLine(false, CreateValue("Additional Roles: ", roles.ToStringWithDelimiterBetweenAppends(",  ")));
		}

		public static ZString PrependDescription(ZString code, ICodeDescriptionPairList sourceList, DelimitedType delimited = Serialiser.DelimitedType.Parenthesis)
		{
			var result = code;
			if (!code.IsEmpty)
			{
				var description = sourceList.GetDescriptionFromCode(code);
				if (!string.IsNullOrEmpty(description))
				{
					result = AddDelimited(description, code, delimited);
				}
			}
			return result;
		}

		public static ZString AppendDescription(ZString code, ICodeDescriptionPairList sourceList, DelimitedType delimited = DelimitedType.Space)
		{
			var result = code;
			if (!code.IsEmpty)
			{
				var description = sourceList.GetDescriptionFromCode(code);
				if (!string.IsNullOrEmpty(description))
				{
					result = AddDelimited(code, description, delimited);
				}
			}
			return result;
		}

		public static ZString AddDelimited(ZString data1, ZString data2, DelimitedType delimited)
		{
			var result = ZString.Empty;
			switch (delimited)
			{
				case DelimitedType.Hyphen:
					result = data1 + " - " + data2;
					break;
				case DelimitedType.Parenthesis:
					result = data1 + " (" + data2 + ")";
					break;
				default:
					result = data1 + " " + data2;
					break;
			}
			return result;
		}

		public static ZString GetLocationWithDesc(BusinessObjectFactory factory, ZString qualifier, ZString code)
		{
			var result = ZString.Empty;
			if (factory != null)
			{
				switch (qualifier)
				{
					case LPCOIssuerLocationTypeList.Codes.CanadianProvince:
						result = AppendDescription(code, factory.GetCachedValue<CanadaStatesList>());
						break;
					case LPCOIssuerLocationTypeList.Codes.EuropeanUnion:
						result = AppendDescription(code, factory.GetCachedValue<EuropeanUnionCountryList>());
						break;
					case LPCOIssuerLocationTypeList.Codes.ISOCountryCode:
						result = AppendCountryDesc(factory, code);
						break;
					case LPCOIssuerLocationTypeList.Codes.MexicanState:
						result = AppendDescription(code, code.Length == 3 ? factory.GetCachedValue<MexicoState3CharsList>() : factory.GetCachedValue<MexicoStateList>());
						break;
					case LPCOIssuerLocationTypeList.Codes.USState:
						result = AppendDescription(code, factory.GetCachedValue<USStateList>());
						break;
					default:
						result = code;
						break;
				}
			}
			return result;
		}

		public static ZString AppendCountryDesc(BusinessObjectFactory factory, ZString countryCode, DelimitedType delimited = Serialiser.DelimitedType.Space)
		{
			var result = countryCode;
			if (!countryCode.IsEmpty && factory != null)
			{
				var country = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
				if (country != null)
				{
					result = AddDelimited(countryCode, country.RN_DescMultilingual, delimited);
				}
			}
			return result;
		}

		public static IEnumerable<ZString> SerialiseHeaderDetailForRecap(this AEPAPG01 pg01, BusinessObjectFactory factory, ZString entryLineNo, ICusEntryLine entryLine, ZString description)
		{
			if (pg01 != null && factory != null)
			{
				var randomLine = entryLine?.RandomLine;
				var productCode = randomLine?.Part?.OP_PartNum ?? randomLine?.JI_PartNo ?? ZString.Empty;
				if (pg01.Disclaimer.IsEmpty)
				{
					yield return CreateLine(true, CreateValue("Entry Line: ", entryLineNo), CreateValue("Product: ", productCode));
					yield return CreateLine(false, CreateValue("PGA Line: ", pg01.PGALineNumber.ToString()), CreateValue("Description: ", description), CreateValue("Intended Use Code: ", AppendDescription(pg01.IntendedUseCode, factory.GetCachedValue<IntendedUseCodesList>())));
					yield return CreateLine(false, CreateValue("Intended Use Description:  ", pg01.IntendedUseDescription));
				}
				else
				{
					yield return CreateLine(true, CreateValue("Entry Line: ", entryLineNo), CreateValue("Product: ", productCode));
					yield return CreateLine(false, CreateValue("PGA Line: ", pg01.PGALineNumber.ToString()), CreateValue("Description: ", description), CreateValue("Disclaimer: ", AppendDescription(pg01.Disclaimer, factory.GetCachedValue<PGADisclaimReasonList>(), Serialiser.DelimitedType.Hyphen)));
				}
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG01 pg01, BusinessObjectFactory factory)
		{
			if (pg01 != null && factory != null)
			{
				ZString globallyUniqueProduct = "Globally Unique Product";
				if (!pg01.GloballyUniqueProductIdentificationCodeQualifier.IsEmpty)
				{
					var desc = factory.GetCachedValue<GlobalUniqueProductCodeQualifierList>().GetDescriptionFromCode(pg01.GloballyUniqueProductIdentificationCodeQualifier);
					if (string.IsNullOrEmpty(desc))
					{
						globallyUniqueProduct += string.Format(CultureInfo.CurrentCulture, " ({0})", pg01.GloballyUniqueProductIdentificationCodeQualifier);
					}
					else
					{
						globallyUniqueProduct = desc;
					}
				}
				yield return CreateLine(false, CreateValue("PGA: ", pg01.GovernmentAgencyCode), CreateValue("Program: ", AppendDescription(pg01.GovernmentAgencyProgramCode, factory.GetCachedValue<PGAAgencyProgramCodeList>(), Serialiser.DelimitedType.Parenthesis)), CreateValue("Processing: ", AppendDescription(pg01.GovernmentAgencyProcessingCode, factory.GetCachedValue<PGAAgencyProcessingCodeList>(), Serialiser.DelimitedType.Parenthesis)), CreateValue(globallyUniqueProduct + ": ", pg01.GloballyUniqueProductIdentificationCode));
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG28 pg28)
		{
			if (pg28 != null)
			{
				yield return CreateLine(false, CreateValue("Dimension 1: ", pg28.CanDimensions1), CreateValue("Dimension 2: ", pg28.CanDimensions2), CreateValue("Dimension 3: ", pg28.CanDimension3), CreateValue("Tracking Issuer and Number: ", pg28.PackageTrackingNumberDetails));
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG29 pg29)
		{
			if (pg29 != null)
			{
				var lineDetail = new ZStringBuilder();
				if (!pg29.CommodityNetQuantityPGALineNet.IsEmpty)
				{
					lineDetail.Append("Net - " + (pg29.CommodityNetQuantityPGALineNet.ToString() + " " + pg29.UnitOfMeasurePGALineNet).Trim());
				}
				if (!pg29.CommodityGrossQuantityPGALineGross.IsEmpty)
				{
					lineDetail.Append("Gross - " + (pg29.CommodityGrossQuantityPGALineGross.ToString() + " " + pg29.UnitOfMeasurePGALineGross).Trim());
				}
				var individualDetail = new ZStringBuilder();
				if (!pg29.CommodityNetQuantityIndividualUnitNet.IsEmpty)
				{
					individualDetail.Append("Net - " + (pg29.CommodityNetQuantityIndividualUnitNet.ToString() + " " + pg29.UnitOfMeasureIndividualUnitNet).Trim());
				}
				if (!pg29.CommodityGrossQuantityIndividualUnitGross.IsEmpty)
				{
					individualDetail.Append("Gross - " + (pg29.CommodityGrossQuantityIndividualUnitGross.ToString() + " " + pg29.UnitOfMeasureIndividualUnitGross).Trim());
				}
				yield return CreateLine(false, CreateValue("Line Detail:  ", lineDetail.ToStringWithDelimiterBetweenAppends(" ")), CreateValue("Individual Detail: ", individualDetail.ToStringWithDelimiterBetweenAppends(" ")));
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG30 pg30, BusinessObjectFactory factory)
		{
			if (pg30 != null && factory != null)
			{
				var inspection = factory.GetCachedValue<InspectionStatusList>().GetDescriptionFromCode(pg30.InspectionLaboratoryTestingStatus) ?? (pg30.InspectionLaboratoryTestingStatus.IsEmpty ? "" : ("Status " + pg30.InspectionLaboratoryTestingStatus).TrimEnd());
				var inspectionData = new ZStringBuilder();
				if (!pg30.AnticipatedArrivalDate.IsEmpty)
				{
					inspectionData.AppendIfNotEmpty("Date: ", pg30.AnticipatedArrivalDate.ToShortDateString() + (" " + pg30.ArrivalTime).TrimEnd());
				}
				if (!pg30.AnticipatedArrivalLocationCode.IsEmpty)
				{
					inspectionData.AppendIfNotEmpty("Location: ", PrependDescription(pg30.AnticipatedArrivalLocationCode, factory.GetCachedValue<InspectionLocationCodeList>()) + (pg30.ArrivalLocation.IsEmpty ? "" : " - " + pg30.ArrivalLocation));
				}
				if (string.IsNullOrEmpty(inspection) && !inspectionData.IsEmpty)
				{
					yield return CreateLine(false, CreateValue("", inspectionData.ToStringWithDelimiterBetweenAppends(" ")));
				}
				else
				{
					yield return CreateLine(false, CreateValue("", inspection + (inspectionData.IsEmpty ? "" : (" (" + inspectionData.ToStringWithDelimiterBetweenAppends(" ") + ")"))));
				}
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG35 pg35, BusinessObjectFactory factory)
		{
			if (pg35 != null && factory != null)
			{
				yield return CreateLine(false, CreateValue("Surety Code: ", pg35.DOTSuretyCode), CreateValue("Bond Number: ", pg35.DOTBondSerialNumber), CreateValue("Bond Type: ", factory.GetCachedValue<DOTBondQualifierList>().GetDescriptionFromCode(pg35.DOTBondQualifier) ?? pg35.DOTBondQualifier), CreateValue("Bond Amount: ", pg35.DOTBondAmount.IsEmpty ? "" : pg35.DOTBondAmount.ToString()));
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG34 pg34, BusinessObjectFactory factory)
		{
			if (pg34 != null && factory != null)
			{
				string documentType;
				switch (pg34.TravelDocumentTypeCode)
				{
					case "1":
						documentType = "Passport Number: ";
						break;
					case "2":
						documentType = "Visa Number: ";
						break;
					case "3":
						documentType = "Enhanced Tribal Card: ";
						break;
					case "4":
						documentType = "Driver License Number: ";
						break;
					default:
						documentType = "Document Type" + (pg34.TravelDocumentTypeCode.IsEmpty ? "" : " (" + pg34.TravelDocumentTypeCode + ")") + ": ";
						break;
				}
				yield return CreateLine(false, CreateValue(documentType, pg34.TravelDocumentIdentifier), CreateValue("Country Of Issue: ", AppendCountryDesc(factory, pg34.TravelDocumentNationality)));
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG33 pg33, BusinessObjectFactory factory)
		{
			if (pg33 != null && factory != null)
			{
				yield return CreateLine(false, CreateValue("Routing Geographic Area: ", (PrependDescription(pg33.CommodityGeographicAreaCode, factory.GetCachedValue<OceanGeographicAreaCodeList>()) + " " + pg33.CommodityGeographicAreaName).Trim()));
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG32 pg32, BusinessObjectFactory factory)
		{
			if (pg32 != null && factory != null)
			{
				var routingType = PrependDescription(pg32.CommodityRoutingTypeCode, factory.GetCachedValue<RoutingTypeList>());
				if (!routingType.IsEmpty && routingType == pg32.CommodityRoutingTypeCode)
				{
					routingType = "Routing Type " + pg32.CommodityRoutingTypeCode;
				}
				string location;
				switch (pg32.CommodityPoliticalSubunitOfRoutingQualifier)
				{
					case "1":
						location = "Schedule K: ";
						break;
					case "2":
						location = "UN/LOCODE: ";
						break;
					default:
						location = "Location: ";
						break;
				}
				yield return CreateLine(false, CreateValue(routingType + ": ", AppendCountryDesc(factory, pg32.CommodityRoutingCountryCode)), CreateValue(location, (pg32.CommodityPoliticalSubunitOfRoutingNumber + " " + pg32.CommodityPoliticalSubunitOfRoutingName).Trim()));
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG31 pg31, BusinessObjectFactory factory)
		{
			if (pg31 != null && factory != null)
			{
				var characteristicType = PrependDescription(pg31.CommodityHarvestingVesselCharacteristicTypeCode, factory.GetCachedValue<HarvestingVesselCharacteristicTypeList>());
				if (!characteristicType.IsEmpty && characteristicType == pg31.CommodityHarvestingVesselCharacteristicTypeCode)
				{
					characteristicType = "Characteristic Type " + pg31.CommodityHarvestingVesselCharacteristicTypeCode;
				}
				yield return CreateLine(false, CreateValue(characteristicType + ": ", pg31.CommodityHarvestingVesselCharacteristic), CreateValue("UQ: ", pg31.UnitOfMeasureconveyance), CreateValue("Net Weight: ", pg31.HarvestedCommodityNetWeight.IsEmpty ? "" : pg31.HarvestedCommodityNetWeight.ToString()));
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG22 pg22, BusinessObjectFactory factory)
		{
			if (pg22 != null && factory != null)
			{
				yield return CreateLine(false, CreateValue("Has Document: ", pg22.ImportersSubstantiatingSignedDocumentSignedConfirmationLetter), CreateValue("Type: ", PrependDescription(pg22.DocumentIdentifier, factory.GetCachedValue<DocumentIdentifierList>(), Serialiser.DelimitedType.Hyphen)), CreateValue("Conformance Declaration: ", pg22.ConformanceDeclaration),
					CreateValue("Entity Role: ", factory.GetCachedValue<EntityRoleCodeList>().GetDescriptionFromCode(pg22.EntityRoleCode) ?? pg22.EntityRoleCode), CreateValue("Declaration: ", AppendDescription(pg22.DeclarationCode, factory.GetCachedValue<DeclarationCodeList>(), Serialiser.DelimitedType.Hyphen)), CreateValue("Certification ", pg22.DeclarationCertification), CreateValue("Date Of Signature: ", pg22.DateOfSignature.IsEmpty ? "" : pg22.DateOfSignature.ToShortDateString()),
					CreateValue("Invoice Number: ", pg22.InvoiceNumber), CreateValue("Description: ", pg22.ComplianceDescription));
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG18 pg18)
		{
			if (pg18 != null)
			{
				yield return CreateLine(false, CreateValue("UNDG: ", pg18.UNDangerousGoodsCode), CreateValue("Class: ", pg18.HazardousClassCode), CreateValue("EPA Waste: ", pg18.EPAHazardousWasteCode), CreateValue("Description: ", pg18.HazardousMaterialDescription), CreateValue("Group: ", pg18.PackagingGroupCode));
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG17 pg17)
		{
			if (pg17 != null)
			{
				yield return CreateLine(false, CreateValue("Specific Name: ", pg17.CommonNameSpecific), CreateValue("General Name: ", pg17.CommonNameGeneral), CreateValue("Live Venomous: ", pg17.LiveVenomousWildlifeCode), CreateValue("Wildlife Cartons: ", pg17.CartonsContainingWildlife.IsEmpty ? "" : pg17.CartonsContainingWildlife.ToString()));
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG10 pg10, BusinessObjectFactory factory)
		{
			if (pg10 != null && factory != null)
			{
				yield return CreateLine(false, CreateValue("", PrependDescription(pg10.CategoryTypeCode, factory.GetCachedValue<CategoryTypeCodeList>(), Serialiser.DelimitedType.Parenthesis)), CreateValue("Category: ", pg10.CategoryCode), CreateValue("Commodity: ", pg10.CommodityQualifierCode), CreateValue("Characteristic: ", pg10.CommodityCharacteristicQualifier), CreateValue("Commodity Description: ", pg10.CommodityCharacteristicDescription));
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG06 pg06, BusinessObjectFactory factory)
		{
			if (pg06 != null && factory != null)
			{
				var builder = new ZStringBuilder();
				if (!pg06.CountryCode.IsEmpty || !pg06.GeographicLocation.IsEmpty)
				{
					ZString sourceType = factory.GetCachedValue<SourceTypeCodesList>().GetDescriptionFromCode(pg06.SourceTypeCode);
					if (sourceType.IsEmpty)
					{
						sourceType = pg06.SourceTypeCode;
					}
					else
					{
						sourceType += " (" + pg06.SourceTypeCode + ")";
					}
					var countryDetail = AppendCountryDesc(factory, pg06.CountryCode);
					builder.Append(sourceType + ": " + (countryDetail + " " + pg06.GeographicLocation).Trim());
				}
				if (!pg06.ProcessingStartDate.IsEmpty || !pg06.ProcessingEndDate.IsEmpty || !pg06.ProcessingTypeCode.IsEmpty || !pg06.ProcessingDescription.IsEmpty)
				{
					var processingDetail = new ZStringBuilder("Processing Details (");
					if (!pg06.ProcessingStartDate.IsEmpty)
					{
						processingDetail.Append("Start Date: " + pg06.ProcessingStartDate.ToShortDateString());
					}
					if (!pg06.ProcessingEndDate.IsEmpty)
					{
						processingDetail.Append("End Date: " + pg06.ProcessingEndDate.ToShortDateString());
					}
					if (!pg06.ProcessingTypeCode.IsEmpty)
					{
						processingDetail.Append("Type: " + AppendDescription(pg06.ProcessingTypeCode, factory.GetCachedValue<ProcessingTypeCodeList>(), Serialiser.DelimitedType.Parenthesis));
					}
					if (!pg06.ProcessingDescription.IsEmpty)
					{
						processingDetail.Append("Description: " + pg06.ProcessingDescription);
					}
					processingDetail.Append(")");
					builder.Append(processingDetail.ToString());
				}
				if (!builder.IsEmpty)
				{
					yield return CreateLine(false, CreateValue("", builder.ToStringWithDelimiterBetweenAppends("   ")));
				}
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG05 pg05, BusinessObjectFactory factory)
		{
			if (pg05 != null && factory != null)
			{
				yield return CreateLine(false, CreateValue("Genus: ", pg05.ScientificGenusName), CreateValue("Species: ", pg05.ScientificSpeciesName), CreateValue("Sub Species: ", pg05.ScientificSubSpeciesName), CreateValue("FWS Wildlife: ", AppendDescription(pg05.ScientificSpeciesCode, factory.GetCachedValue<FWSWildlifeCategoryCodesList>(), Serialiser.DelimitedType.Parenthesis)), CreateValue("FWS Description: ", AppendDescription(pg05.FWSDescriptionCode, factory.GetCachedValue<FWSWildlifeDescriptionCodesList>(), Serialiser.DelimitedType.Parenthesis)));
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG04 pg04)
		{
			if (pg04 != null)
			{
				yield return CreateLine(false, CreateValue("Active Ingredient: ", pg04.ConstituentActiveIngredientQualifier), CreateValue("Name Of Element: ", pg04.NameOfTheConstituentElement), CreateValue("Qty: ", pg04.QuantityOfConstituentElement.IsEmpty ? "" : pg04.QuantityOfConstituentElement.ToString()), CreateValue("UQ: ", pg04.UnitOfMeasureConstituentElement), CreateValue("% Of Ingredient: ", pg04.PercentOfConstituentElement.IsEmpty ? "" : pg04.PercentOfConstituentElement.ToString()));
			}
		}

		public static IEnumerable<ZString> SerialiseForRecap(this AEPAPG02 pg02)
		{
			if (pg02 != null)
			{
				yield return CreateLine(false, CreateValue(ProductCodeQualifiersList.GetShortDescription(pg02.ProductCodeQualifier) + ": ", pg02.ProductCodeNumber), CreateValue(ProductCodeQualifiersList.GetShortDescription(pg02.ProductCodeQualifier1) + ": ", pg02.ProductCodeNumber1), CreateValue(ProductCodeQualifiersList.GetShortDescription(pg02.ProductCodeQualifier2) + ": ", pg02.ProductCodeNumber2));
			}
		}

		const string EntityPadding = "        ";
	}
}
