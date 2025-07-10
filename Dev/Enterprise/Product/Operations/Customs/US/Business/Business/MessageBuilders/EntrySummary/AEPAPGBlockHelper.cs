using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public static class AEPAPGBlockHelper
	{
		#region PGAOI

		public static AENSOI MakePGAOI(ZString commercialDescription)
		{
			var result = new AENSOI();
			result.CommercialDescriptionText = commercialDescription.Replace("\r\n", " ").Left(AutoUSFDAAddInfo.Schema.US_FDACommercialDescMaxLength);
			return result;
		}

		#endregion

		#region PG01

		public static AEPAPG01 MakePG01(int currentLineNumber, ZString agencyCode, ZString programCode)
		{
			return MakePG01(currentLineNumber, agencyCode, programCode, ZString.Empty);
		}

		public static AEPAPG01 MakePG01(int currentLineNumber, ZString agencyCode, ZString programCode, ZString processingCode)
		{
			return new AEPAPG01()
			{
				PGALineNumber = currentLineNumber,
				GovernmentAgencyCode = agencyCode,
				GovernmentAgencyProgramCode = programCode,
				GovernmentAgencyProcessingCode = processingCode
			};
		}

		public static AEPAPG01 MakePG01(int currentLineNumber, ZString agencyCode, ZString programCode, ZString processingCode, bool confidentialInformationIndicator)
		{
			var result = MakePG01(currentLineNumber, agencyCode, programCode, processingCode);
			result.ConfidentialInformationIndicator = confidentialInformationIndicator ? "Y" : "";
			return result;
		}

		public static AEPAPG01 MakePG01(int currentLineNumber, ZString agencyCode, ZString programCode, ZString processingCode, bool electronicImageSubmitted, bool confidentialInformationIndicator)
		{
			var result = MakePG01(currentLineNumber, agencyCode, programCode, processingCode);
			result.ElectronicImageSubmitted = electronicImageSubmitted ? "Y" : "";
			result.ConfidentialInformationIndicator = confidentialInformationIndicator ? "Y" : "";
			return result;
		}

		public static AEPAPG01 MakePG01ForDisclaimer(int currentLineNumber, ZString agencyCode, ZString programCode, ZString disclaimer)
		{
			var result = MakePG01(currentLineNumber, agencyCode, programCode);
			result.Disclaimer = disclaimer;
			return result;
		}

		public static AEPAPG01 MakePG01ForDataCorrectionDeleteAll(ZString agencyCode)
		{
			var result = MakePG01(0, agencyCode, "COR");
			result.CorrectionIndicator = "D";
			return result;
		}

		public static AEPAPG01 MakePG01(IVNEData commonData, int currentLineNumber)
		{
			var result = MakePG01(currentLineNumber, ACEGovernmentAgenciesCodeList.Codes.EPA, GovernmentAgencyProgramCodeList.Codes.VNE);
			result.ElectronicImageSubmitted = commonData.ElectronicImageSubmitted;
			return result;
		}

		public static AEPAPG01 MakePG01(IFSISLine commonData, int currentLineNumber)
		{
			var result = MakePG01(currentLineNumber, ACEGovernmentAgenciesCodeList.Codes.FSI, "FSI");
			result.GloballyUniqueProductIdentificationCodeQualifier = commonData.ProductIDQualifier;
			result.GloballyUniqueProductIdentificationCode = commonData.ProductID;
			result.IntendedUseCode = commonData.IntendedUseCode;
			return result;
		}

		public static AEPAPG01 MakePG01(IOMCHeader commonData, int currentLineNumber)
		{
			var result = MakePG01(currentLineNumber, ACEGovernmentAgenciesCodeList.Codes.OMC, "OMC");
			result.ElectronicImageSubmitted = commonData.ElectronicImageSubmitted ? "Y" : "";
			return result;
		}

		public static AEPAPG01 MakePG01(IPSTData commonData, int currentLineNumber)
		{
			var result = MakePG01(currentLineNumber, ACEGovernmentAgenciesCodeList.Codes.EPA, commonData.ProductType);
			result.IntendedUseCode = commonData.IntendedUseCode;
			result.IntendedUseDescription = commonData.IntendedUseDescription;
			result.ElectronicImageSubmitted = commonData.IsPSTLabelsSent ? "Y" : "";
			result.ConfidentialInformationIndicator = commonData.ConfidentialInfoIncluded ? "Y" : "";
			return result;
		}

		public static AEPAPG01 MakePG01(INHTSAHeader commonData, int currentLineNumber)
		{
			var result = MakePG01(currentLineNumber, ACEGovernmentAgenciesCodeList.Codes.NHT, commonData.ProgramCode);
			result.ElectronicImageSubmitted = commonData.ElectronicImageSubmitted ? "Y" : "";
			result.IntendedUseCode = commonData.IntendedUseCode;
			result.IntendedUseDescription = commonData.IntendedUseDesc;
			return result;
		}

		public static AEPAPG01 MakePG01(IAMSData commonData, int currentLineNumber)
		{
			var programCode = commonData.Program.SubstringSafe(0, 2);
			var processingCode = commonData.Program.SubstringSafe(2, 1);
			var result = MakePG01(currentLineNumber, ACEGovernmentAgenciesCodeList.Codes.AMS, programCode, processingCode);
			result.IntendedUseCode = commonData.IntendedUseCode;
			result.IntendedUseDescription = commonData.IntendedUseCodeDescription;
			return result;
		}

		public static AEPAPG01 MakePG01(ICPSCHeader commonData, int currentLineNumber)
		{
			var result = MakePG01(currentLineNumber, ACEGovernmentAgenciesCodeList.Codes.CPS, "CPS", commonData.ProcessingCode);
			result.GloballyUniqueProductIdentificationCodeQualifier = commonData.ProductIDType;
			result.GloballyUniqueProductIdentificationCode = commonData.ProductID;
			result.IntendedUseCode = commonData.IntendedUseCode;
			result.IntendedUseDescription = commonData.IntendedUseDescription;
			return result;
		}

		public static AEPAPG01 MakePG01(int currentLineNumber)
		{
			return MakePG01(currentLineNumber, ACEGovernmentAgenciesCodeList.Codes.DEA, "DEA", string.Empty, true);
		}

		#endregion

		#region PG02

		public static AEPAPG02 MakePG02(ZString itemType)
		{
			return new AEPAPG02() { ItemType = itemType };
		}

		public static AEPAPG02 MakePG02(ZString itemType, ZString productCodeQualifier, ZString productCodeNumber)
		{
			var result = MakePG02(itemType);
			result.ProductCodeQualifier = productCodeQualifier;
			result.ProductCodeNumber = productCodeNumber;
			return result;
		}

		public static AEPAPG02 MakePG02(IPSTLine commonData)
		{
			return MakePG02(PG02ItemTypeList.Codes.Component, commonData.LPCOType, commonData.LPCONumber);
		}

		public static AEPAPG02 MakePG02(ZString productCodeQualifier, ZString productCodeNumber)
		{
			return MakePG02(PG02ItemTypeList.Codes.Product, productCodeQualifier, productCodeNumber);
		}

		public static AEPAPG02 MakePG02(ICPSCHeader cpscHeader)
		{
			var result = MakePG02(PG02ItemTypeList.Codes.Product);
			if (cpscHeader.ProcessingCode == CPSCProcessingCodeList.Codes.REF)
			{
				result.ProductCodeQualifier = "PRI";
				result.ProductCodeNumber = cpscHeader.ProductCode;
				result.ProductCodeQualifier1 = "PRIV";
				result.ProductCodeNumber1 = cpscHeader.ProductCodeVersionNumber;
			}
			else
			{
				if (!cpscHeader.SKUProductCode.IsEmpty)
				{
					result.ProductCodeQualifier = "SKU";
					result.ProductCodeNumber = cpscHeader.SKUProductCode;
				}
			}
			return result;
		}

		public static AEPAPG02 MakePG02(IDEAConstituent commonData)
		{
			return MakePG02(PG02ItemTypeList.Codes.Component, ProductCodeQualifiersList.Codes.ControlledSubstancesActNumber, commonData.ProductCode);
		}

		#endregion

		#region PG04

		public static AEPAPG04 MakePG04(ZString constituentActiveIngredientQualifier, ZString nameOfTheConstituentElement, ZDecimal percentOfConstituentElement)
		{
			return new AEPAPG04()
			{
				ConstituentActiveIngredientQualifier = constituentActiveIngredientQualifier,
				NameOfTheConstituentElement = nameOfTheConstituentElement,
				PercentOfConstituentElement = percentOfConstituentElement
			};
		}

		public static AEPAPG04 MakePG04(IPSTLine commonData)
		{
			return MakePG04("Y", commonData.NameOfActiveIngredient, commonData.ActiveIngredientPercentage);
		}

		public static AEPAPG04 MakePG04(IDEAConstituent commonData)
		{
			return new AEPAPG04()
			{
				QuantityOfConstituentElement = commonData.Weight,
				UnitOfMeasureConstituentElement = commonData.WeightUQ
			};
		}

		#endregion

		#region PG05

		public static PGAPG05 MakePG05(ZString scientificGenusName, ZString scientificSpeciesName, ZString scientificSubSpeciesName)
		{
			return new PGAPG05() { ScientificGenusName = scientificGenusName, ScientificSpeciesName = scientificSpeciesName, ScientificSubSpeciesName = scientificSubSpeciesName };
		}

		public static PGAPG05 MakePG05(ZString scientificSpeciesCode)
		{
			return new PGAPG05() { ScientificSpeciesCode = scientificSpeciesCode };
		}

		#endregion

		#region PG06

		public static AEPAPG06 MakePG06(ZString sourceType, ZString countryCode)
		{
			return MakePG06(sourceType, countryCode, ZString.Empty, ZString.Empty);
		}

		public static AEPAPG06 MakePG06(ZString sourceType, ZString countryCode, ZDateTime processingStartDate)
		{
			var result = MakePG06(sourceType, countryCode, ZString.Empty, ZString.Empty);
			result.ProcessingStartDate = processingStartDate.Date;
			return result;
		}

		public static AEPAPG06 MakePG06(ZString sourceType, ZString countryCode, ZString geographicLocation, ZString processingTypeCode)
		{
			return new AEPAPG06() { SourceTypeCode = sourceType, CountryCode = countryCode, GeographicLocation = geographicLocation, ProcessingTypeCode = processingTypeCode };
		}

		public static AEPAPG06 MakePG06(ZString sourceType, ZString countryCode, ZString geographicLocation, ZDate processingStartDate, ZString processingTypeCode, ZString processingDescription)
		{
			return new AEPAPG06() { SourceTypeCode = sourceType, CountryCode = countryCode, GeographicLocation = geographicLocation, ProcessingStartDate = processingStartDate, ProcessingTypeCode = processingTypeCode, ProcessingDescription = processingDescription };
		}

		public static AEPAPG06 MakePG06(ZString sourceType, ZString countryCode, ZString geographicLocation, ZDate processingStartDate, ZDate processingEndDate, ZString processingTypeCode, ZString processingDescription)
		{
			return new AEPAPG06() { SourceTypeCode = sourceType, CountryCode = countryCode, GeographicLocation = geographicLocation, ProcessingStartDate = processingStartDate, ProcessingEndDate = processingEndDate, ProcessingTypeCode = processingTypeCode, ProcessingDescription = processingDescription };
		}

		#endregion

		#region PG07

		public static AEPAPG07 MakePG07(ZString brandName)
		{
			return new AEPAPG07() { TradeNameBrandName = brandName };
		}

		public static AEPAPG07 MakePG07(ZString itemIdentityNumberQualifier, ZString itemIdentityNumber)
		{
			return new AEPAPG07()
			{
				ItemIdentityNumberQualifier = itemIdentityNumberQualifier,
				ItemIdentityNumber = itemIdentityNumber
			};
		}

		public static AEPAPG07 MakePG07(INHTSADetails commonData, ZString numberType, ZString number)
		{
			var result = MakePG07(commonData.BrandName);
			result.Model = commonData.Model;
			result.ManufactureMonthAndYear = commonData.MonthOfManufacturer + commonData.YearOfManufacturer;
			result.ItemIdentityNumberQualifier = numberType;
			result.ItemIdentityNumber = number;
			return result;
		}

		public static AEPAPG07 MakePG07(IATFData commonData)
		{
			var result = MakePG07(commonData.Model);
			result.Model = commonData.CaliberGaugeSize;
			return result;
		}

		public static AEPAPG07 MakePG07(ICPSCHeader cpscHeader, ZString numberType, ZString number)
		{
			return new AEPAPG07()
			{
				TradeNameBrandName = cpscHeader.BrandName,
				ItemIdentityNumberQualifier = numberType,
				ItemIdentityNumber = number,
				Model = cpscHeader.ProductName,
				ManufactureMonthAndYear = cpscHeader.ManufacturerMonthAndYear
			};
		}

		#endregion

		#region PG08

		public static AEPAPG08 MakePG08(ZString[] additionalNumbers)
		{
			return new AEPAPG08()
			{
				ItemIdentityNumber = additionalNumbers[0],
				ItemIdentityNumber1 = additionalNumbers[1],
				ItemIdentityNumber2 = additionalNumbers[2],
				ItemIdentityNumber3 = additionalNumbers[3]
			};
		}

		public static AEPAPG08 MakePG08(IEnumerable<ZString> additionalNumbers)
		{
			return new AEPAPG08()
			{
				ItemIdentityNumber = additionalNumbers.ElementAtOrDefault(0),
				ItemIdentityNumber1 = additionalNumbers.ElementAtOrDefault(1),
				ItemIdentityNumber2 = additionalNumbers.ElementAtOrDefault(2),
				ItemIdentityNumber3 = additionalNumbers.ElementAtOrDefault(3)
			};
		}

		#endregion

		#region PG10

		public static AEPAPG10 MakePG10(ZString code, ZString description, ZString qualifier)
		{
			return new AEPAPG10() { CommodityQualifierCode = code, CommodityCharacteristicDescription = description, CommodityCharacteristicQualifier = qualifier };
		}

		public static AEPAPG10 MakePG10(IAPHISCharacteristic characteristic)
		{
			return MakePG10(characteristic.CommodityQualifierCode, characteristic.CommodityCharacteristicDescription, characteristic.CommodityCharacteristicQualifier);
		}

		public static AEPAPG10 MakePG10(IFSISLot lot)
		{
			var result = new AEPAPG10();
			result.CategoryTypeCode = "FS1";
			result.CategoryCode = lot.Species;
			result.CommodityQualifierCode = lot.ProductQualifierCode;
			result.CommodityCharacteristicQualifier = lot.ProductCharacteristic;
			return result;
		}

		public static AEPAPG10 MakePG10(INHTSADetails nhtsa, ZString commodityQualifierCode, ZString commodityCharacteristic)
		{
			var result = new AEPAPG10();
			result.CategoryTypeCode = nhtsa.CategoryType;
			result.CategoryCode = nhtsa.CategoryCode;
			result.CommodityQualifierCode = commodityQualifierCode;
			result.CommodityCharacteristicQualifier = commodityCharacteristic;
			return result;
		}

		public static AEPAPG10 MakePG10(IATFData atfData)
		{
			var result = new AEPAPG10();
			result.CategoryTypeCode = "AT1";
			result.CategoryCode = atfData.CategoryCode;
			result.CommodityQualifierCode = "PC9";
			result.CommodityCharacteristicDescription = atfData.ExtendedDescription;
			return result;
		}

		#endregion

		#region PG13

		public static AEPAPG13 MakePG13(ZString issued)
		{
			return new AEPAPG13() { LPCOIssuerGovernmentGeographicCodeQualifier = "ISO", LocationCountryStateProvinceOfIssuerOfTheLPCO = issued };
		}

		public static AEPAPG13 MakePG13(ZString qualifier, ZString issued, ZString issuerOfLPCO)
		{
			return new AEPAPG13() { LPCOIssuerGovernmentGeographicCodeQualifier = qualifier, LocationCountryStateProvinceOfIssuerOfTheLPCO = issued, IssuerOfLPCO = issuerOfLPCO };
		}

		#endregion

		#region PG14

		public static AEPAPG14 MakePG14(ZString type, ZString numberOrName)
		{
			return new AEPAPG14() { LPCOType = type, LPCONumberorName = numberOrName };
		}

		public static AEPAPG14 MakePG14(ZString transactionType, ZString type, ZString numberorName)
		{
			var result = MakePG14(type, numberorName);
			result.LPCOTransactionType = transactionType;
			return result;
		}

		public static AEPAPG14 MakePG14(ZString transactionType, ZString type, ZString numberorName, ZDecimal quantity, ZString unit)
		{
			var result = MakePG14(transactionType, type, numberorName);
			result.LPCOQuantity = quantity;
			result.LPCOUnitOfMeasure = unit;
			return result;
		}

		public static AEPAPG14 MakePG14(INHTSAPermitAndLicense permitAndLicense)
		{
			return new AEPAPG14()
			{
				LPCOTransactionType = permitAndLicense.TransactionType,
				LPCOType = permitAndLicense.LPCOType,
				LPCONumberorName = permitAndLicense.LPCONumber,
				LPCODateQualifier = permitAndLicense.DateType,
				LPCODate = permitAndLicense.LPCODate,
				LPCOQuantity = permitAndLicense.LPCOQuantity,
				LPCOUnitOfMeasure = permitAndLicense.UnitOfMeasure
			};
		}

		public static AEPAPG14 MakePG14(IDEAHeader data)
		{
			return new AEPAPG14()
			{
				LPCOTransactionType = LPCOTransactionTypeList.Codes.SingleUse,
				LPCONumberorName = data.PermitNumber
			};
		}

		#endregion

		#region PG19

		public static AEPAPG19 MakePG19(ZString role)
		{
			return new AEPAPG19() { EntityRoleCode = role };
		}

		public static AEPAPG19 MakePG19WithNumber(ZString role, ZString number)
		{
			var result = MakePG19(role);
			result.EntityNumber = number.Left(AEPAPG19EntityNumberMaxLength);
			return result;
		}

		public static AEPAPG19 MakePG19WithNumber(ZString role, ZString numberType, ZString number)
		{
			var result = MakePG19WithNumber(role, number);
			result.EntityIdentificationCode = numberType;
			return result;
		}

		public static IEnumerable<MessageBlock> MakePG19WithName(ZString role, ZString name)
		{
			var result = MakePG19(role);
			result.EntityName = name.Left(AEPAPG19EntityNameMaxLength);
			yield return result;

			if (name.Length > AEPAPG19EntityNameMaxLength)
			{
				foreach (var block in MakeEntityNameOverflowBlock(name))
				{
					yield return block;
				}
			}
		}

		public static IEnumerable<MessageBlock> MakePG19(ZString roleCode, ZString numberType, ZString number, ZString entityName, ZString address1)
		{
			var result = MakePG19WithNumber(roleCode, numberType, number);
			result.EntityName = entityName.Left(AEPAPG19EntityNameMaxLength);
			result.EntityAddress1 = address1.Left(AEPAPG19EntityAddress1MaxLength);
			yield return result;

			if (entityName.Length > AEPAPG19EntityNameMaxLength)
			{
				foreach (var block in MakeEntityNameOverflowBlock(entityName))
				{
					yield return block;
				}
			}

			if (address1.Length > AEPAPG19EntityAddress1MaxLength)
			{
				foreach (var block in MakeEntityAddress1OverflowBlock(address1))
				{
					yield return block;
				}
			}
		}

		public static AEPAPG19 MakePG19WithoutOverflowingBlock(ZString roleCode, ZString numberType, ZString number, ZString entityName, ZString address1)
		{
			var result = MakePG19WithNumber(roleCode, numberType, number);
			result.EntityName = entityName.Left(AEPAPG19EntityNameMaxLength);
			result.EntityAddress1 = address1.Left(AEPAPG19EntityAddress1MaxLength);
			return result;
		}

		public static IEnumerable<AEPAPG19> MakePG19(IPSTData pstData)
		{
			if (!pstData.ProducerEstForNo.IsEmpty)
			{
				yield return MakePG19WithNumber(EntityRoleCodeList.Codes.EPAProducerEstablishmentNumber, pstData.ProducerEstForNo);
			}
			if (!pstData.ProducerEstNo.IsEmpty)
			{
				yield return MakePG19WithNumber(EntityRoleCodeList.Codes.EPAProducerEstablishmentNumber, pstData.ProducerEstNo);
			}
		}

		public static IEnumerable<MessageBlock> MakePG19(ZString role, IPGAContactDetailsWithID contact)
		{
			if (contact != null)
			{
				var pg19 = MakePG19WithoutOverflowingBlock(role, contact);
				if (pg19 != null)
				{
					yield return pg19;
				}

				if (contact.CompanyAddress.CompanyName.Length > AEPAPG19EntityNameMaxLength)
				{
					foreach (var block in MakeEntityNameOverflowBlock(contact.CompanyAddress.CompanyName))
					{
						yield return block;
					}
				}

				if (contact.CompanyAddress.AddressLine1.Length > AEPAPG19EntityAddress1MaxLength)
				{
					foreach (var block in MakeEntityAddress1OverflowBlock(contact.CompanyAddress.AddressLine1))
					{
						yield return block;
					}
				}
			}
		}

		public static MessageBlock MakePG19WithoutOverflowingBlock(ZString role, IPGAContactDetailsWithID contact)
		{
			if (contact != null)
			{
				var pg19 = new AEPAPG19();
				pg19.EntityRoleCode = role;
				pg19.EntityName = contact.CompanyAddress.CompanyName.Left(AEPAPG19EntityNameMaxLength);
				pg19.EntityAddress1 = contact.CompanyAddress.AddressLine1.Left(AEPAPG19EntityAddress1MaxLength);

				var pgaContactWithID = contact;
				if (pgaContactWithID != null)
				{
					if (!pgaContactWithID.IDType.IsEmpty && !pgaContactWithID.IDNumber.IsEmpty)
					{
						pg19.EntityIdentificationCode = pgaContactWithID.IDType;
						pg19.EntityNumber = pgaContactWithID.IDNumber.Left(AEPAPG19EntityNumberMaxLength);
					}
				}
				return pg19;
			}
			return null;
		}

		public static IEnumerable<MessageBlock> MakePG19(ZString roleCode, OrgCusCodeForFDA customsNumber, IPGAContactDetails contactDetails)
		{
			var entityName = contactDetails != null ? contactDetails.CompanyAddress.CompanyName : ZString.Empty;
			var entityAddress = contactDetails != null ? contactDetails.CompanyAddress.AddressLine1 : ZString.Empty;

			foreach (var block in MakePG19(roleCode, customsNumber.ID, customsNumber.Number, entityName, entityAddress))
			{
				yield return block;
			}
		}

		public static IEnumerable<MessageBlock> MakePG19(ZString roleCode, OrgCusCodeForFDA customsNumber, ICustomsBrokerDetails details)
		{
			var entityName = details != null && details.Address != null ? details.Address.CompanyName : ZString.Empty;
			var entityAddress = details != null && details.Address != null ? details.Address.AddressLine1 : ZString.Empty;

			foreach (var block in MakePG19(roleCode, customsNumber.ID, customsNumber.Number, entityName, entityAddress))
			{
				yield return block;
			}
		}

		public static AEPAPG19 MakePG19WithoutOverflowingBlock(ZString roleCode, ICustomsBrokerDetails details)
		{
			var entityName = details != null && details.Address != null ? details.Address.CompanyName : ZString.Empty;
			var entityAddress = details != null && details.Address != null ? details.Address.AddressLine1 : ZString.Empty;
			return MakePG19WithoutOverflowingBlock(roleCode, ZString.Empty, ZString.Empty, entityName, entityAddress);
		}

		public static AEPAPG19 MakePG19(IDEAHeader commonData)
		{
			return new AEPAPG19()
			{
				EntityRoleCode = EntityRoleCodeList.Codes.LPCOAuthorizedParty,
				EntityIdentificationCode = EntityIdentificationCodesList.Codes.DEARegistrationNumber,
				EntityNumber = commonData.RegistrationNumber.Left(AEPAPG19EntityNumberMaxLength)
			};
		}

		#endregion

		#region PG20

		public static IEnumerable<MessageBlock> MakePG20(ZString address2, ZString city, ZString state, ZString country, ZString postCode)
		{
			yield return MakePG20BlockWithoutOverflowingBlock(address2, city, state, country, postCode);

			if (address2.Length > AEPAPG20EntityAddress2MaxLength)
			{
				foreach (var block in MakeEntityAddress2OverflowBlock(address2))
				{
					yield return block;
				}
			}
		}

		public static MessageBlock MakePG20BlockWithoutOverflowingBlock(ZString address2, ZString city, ZString state, ZString country, ZString postCode)
		{
			var pg20 = new AEPAPG20();
			pg20.EntityAddress2 = address2.Left(AEPAPG20EntityAddress2MaxLength);
			pg20.EntityCity = city;
			pg20.EntityStateProvince = state;
			pg20.EntityCountry = country;
			pg20.EntityZipPostalCode = postCode.Replace("-", "");
			return pg20;
		}

		public static IEnumerable<MessageBlock> MakePG20(IPGAContactDetails contact)
		{
			foreach (var block in MakePG20(contact.CompanyAddress.AddressLine2, contact.CompanyAddress.City, contact.CompanyAddress.State, contact.CompanyAddress.Country, contact.CompanyAddress.PostCode))
			{
				yield return block;
			}
		}

		public static IEnumerable<MessageBlock> MakePG20(IAddressDetails address)
		{
			foreach (var block in MakePG20(address.AddressLine2, address.City, address.State, address.Country, address.PostCode))
			{
				yield return block;
			}
		}

		public static MessageBlock MakePG20WithoutOverflowingBlock(IAddressDetails address)
		{
			return MakePG20BlockWithoutOverflowingBlock(address.AddressLine2, address.City, address.State, address.Country, address.PostCode);
		}

		#endregion

		#region PG21

		public static IEnumerable<MessageBlock> MakePG21(ZString individualQualifier, ZString individualName, ZString telephoneNumber, ZString emailAddress, ZString faxNumber)
		{
			yield return MakePG21BlockWithoutOverflowingBlock(individualQualifier, individualName, telephoneNumber, emailAddress, faxNumber);

			if (individualName.Length > AEPAPG21IndividualNameMaxLength)
			{
				foreach (var block in MakeIndividualNameOverflowBlock(individualName))
				{
					yield return block;
				}
			}

			if (emailAddress.Length > AEPAPG21EmailMaxLength)
			{
				foreach (var block in MakeEmailOverflowBlock(emailAddress))
				{
					yield return block;
				}
			}
		}

		public static MessageBlock MakePG21BlockWithoutOverflowingBlock(ZString individualQualifier, ZString individualName, ZString telephoneNumber, ZString emailAddress, ZString faxNumber)
		{
			var result = new AEPAPG21();
			result.IndividualQualifier = individualQualifier;
			result.IndividualName = individualName.Left(AEPAPG21IndividualNameMaxLength);
			result.TelephoneNumberOfTheIndividual = telephoneNumber.KeepNumericCharacters();
			result.EmailAddressOrFaxNumberForTheIndividual = !emailAddress.IsEmpty ? emailAddress.Left(AEPAPG21EmailMaxLength) : faxNumber;
			return result;
		}

		public static IEnumerable<MessageBlock> MakePG21(ZString role, IPGAContactDetails contact)
		{
			foreach (var block in MakePG21(role, contact.Name, contact.PhoneNumber, contact.EmailAddress, contact.Fax))
			{
				yield return block;
			}
		}

		public static MessageBlock MakePG21WithoutOverflowingBlock(ZString role, IPGAContactDetails contact)
		{
			return MakePG21BlockWithoutOverflowingBlock(role, contact.Name, contact.PhoneNumber, contact.EmailAddress, contact.Fax);
		}

		public static IEnumerable<MessageBlock> MakePG21(ZString role, ICustomsBrokerDetails staff)
		{
			foreach (var block in MakePG21(role, staff.ContactName, staff.ContactPhone, staff.ContactEmail, ZString.Empty))
			{
				yield return block;
			}
		}

		public static MessageBlock MakePG21WithoutOverflowingBlock(ZString role, ICustomsBrokerDetails staff)
		{
			return MakePG21BlockWithoutOverflowingBlock(role, staff.ContactName, staff.ContactPhone, staff.ContactEmail, ZString.Empty);
		}

		#endregion

		#region PG22

		public static AEPAPG22 MakePG22(ZString conformanceDeclaration, ZString roleCode, ZString declarationCode, ZDateTime dateOfSignature)
		{
			var result = new AEPAPG22();
			result.DocumentIdentifier = "924";
			result.ConformanceDeclaration = conformanceDeclaration;
			result.EntityRoleCode = roleCode;
			result.DeclarationCode = declarationCode;
			result.DeclarationCertification = "Y";
			result.DateOfSignature = dateOfSignature.Date;
			return result;
		}

		public static AEPAPG22 MakePG22(ZString roleCode, ZString declarationCode, ZString declarationCertification)
		{
			var result = new AEPAPG22();
			result.EntityRoleCode = roleCode;
			result.DeclarationCode = declarationCode;
			result.DeclarationCertification = declarationCertification;
			return result;
		}

		public static AEPAPG22 MakePG22FWS(ZString declarationCode, ZDate certifySignatureDate)
		{
			return new AEPAPG22()
			{
				DeclarationCode = declarationCode,
				DateOfSignature = certifySignatureDate,
				DeclarationCertification = "Y",
				EntityRoleCode = EntityRoleCodeList.Codes.FWSImporter
			};
		}

		public static AEPAPG22 MakePG22EPATSCA(ZString roleCode, ZString declarationCode, ZString declarationCertification, ZDate certifySignatureDate)
		{
			var result = new AEPAPG22();
			result.EntityRoleCode = roleCode;
			result.DeclarationCode = declarationCode;
			result.DeclarationCertification = declarationCertification;
			result.DateOfSignature = certifySignatureDate;
			return result;
		}

		internal static AEPAPG22 MakePG22WithDocId(ZString docId, ZString declarationCode, ZString declarationCertificate, ZDateTime signDate, string entityRoleCode = EntityRoleCodeList.Codes.CertifyingIndividual)
		{
			return new AEPAPG22()
			{
				DocumentIdentifier = docId,
				EntityRoleCode = entityRoleCode,
				DeclarationCode = declarationCode,
				DeclarationCertification = declarationCertificate,
				DateOfSignature = signDate.IsValid ? signDate.Date : ZDate.Empty
			};
		}

		public static AEPAPG22 MakePG22(IVNEData commonData)
		{
			return MakePG22WithDocId(commonData.DocumentIdentifier, commonData.DocumentIdentifier == "943" ? EP1Code : EP2Code, commonData.DeclarationCertificate, commonData.CertifySignatureDate);
		}
		const string EP1Code = "EP1";
		const string EP2Code = "EP2";

		public static AEPAPG22 MakePG22(IFSISLine cert)
		{
			return MakePG22WithDocId("956", "FS3", cert.DeclarationCertificate, cert.CertifySignatureDate);
		}

		public static AEPAPG22 MakePG22(INHTSADocument commonData, ZString declarationCertificate, ZDateTime signDate, ZString boxNumber)
		{
			var documentType = commonData.DocumentType;

			return new AEPAPG22()
			{
				ImportersSubstantiatingSignedDocumentSignedConfirmationLetter = "Y",
				DocumentIdentifier = documentType,
				ConformanceDeclaration = documentType == NHTSADocumentTypeList.Codes._946 ? boxNumber : ZString.Empty,
				EntityRoleCode = commonData.OwnerCode,
				DeclarationCode = "NH1",
				DeclarationCertification = declarationCertificate,
				DateOfSignature = signDate.Date
			};
		}

		public static AEPAPG22 MakePG22(IDEAHeader commonData)
		{
			return new AEPAPG22()
			{
				DocumentIdentifier = DEAFormTypeList.GetDocumentIdentifierFromFormType(commonData.FormID)
			};
		}

		public static AEPAPG22 MakePG22(INMFSDocument document, bool is370)
		{
			return new AEPAPG22()
			{
				ImportersSubstantiatingSignedDocumentSignedConfirmationLetter = "Y",
				DocumentIdentifier = document.DocumentIdentifier,
				ConformanceDeclaration = is370 ? document.DocumentNumber : ZString.Empty,
				ComplianceDescription = !is370 ? document.DocumentNumber : ZString.Empty
			};
		}

		public static AEPAPG22 MakePG22COA()
		{
			return new AEPAPG22()
			{
				DeclarationCode = "COA1",
				DocumentIdentifier = "894",
				ImportersSubstantiatingSignedDocumentSignedConfirmationLetter = "Y"
			};
		}

		#endregion

		#region PG24

		internal static AEPAPG24 MakePG24WithRemarkText(ZString typeCode, ZString remarks)
		{
			return new AEPAPG24() { RemarksTypeCode = typeCode, RemarksText = remarks };
		}

		static AEPAPG24 MakePG24WithRemarkCode(ZString typeCode, ZString remarkCode)
		{
			return new AEPAPG24() { RemarksTypeCode = typeCode, RemarksCode = remarkCode };
		}

		public static IEnumerable<AEPAPG24> MakePG24(IVNEData commonData)
		{
			if (!commonData.BondExemptionCode.IsEmpty)
			{
				yield return MakePG24WithRemarkCode(RemarksTypeCodeList.Codes.EP1, commonData.BondExemptionCode);
			}
			if (!commonData.ImportCode.IsEmpty)
			{
				yield return MakePG24WithRemarkCode(RemarksTypeCodeList.Codes.EP2, commonData.ImportCode);
			}
			if (!commonData.IndustryCode.IsEmpty)
			{
				yield return MakePG24WithRemarkCode(RemarksTypeCodeList.Codes.EP3, commonData.IndustryCode);
			}
			if (!commonData.ExemptionRemarks.IsEmpty)
			{
				yield return MakePG24WithRemarkText(RemarksTypeCodeList.Codes.EP4, commonData.ExemptionRemarks);
			}
			if (!commonData.GeneralRemarks.IsEmpty)
			{
				yield return MakePG24WithRemarkText(RemarksTypeCodeList.Codes.GEN, commonData.GeneralRemarks);
			}
		}

		public static IEnumerable<AEPAPG24> MakePG24(IFSISLine cer)
		{
			foreach (var sealNumber in cer.SealNumbers)
			{
				yield return MakePG24WithRemarkText(RemarksTypeCodeList.Codes.GEN, sealNumber);
			}
		}

		public static IEnumerable<MessageBlock> MakePG24(IPSTData pstData)
		{
			if (!pstData.ReasonRemarks.IsEmpty)
			{
				var result = MakePG24WithRemarkCode("GEN", ZString.Empty);
				result.RemarksText = pstData.ReasonRemarks;
				yield return result;
			}

			if (!pstData.ConfidentialityRemarks.IsEmpty || !pstData.ReasonCode.IsEmpty)
			{
				var result = MakePG24WithRemarkCode("EP5", pstData.ReasonCode);
				result.RemarksText = pstData.ConfidentialityRemarks;
				yield return result;
			}
		}

		public static AEPAPG24 MakePG24(INHTSAHeader commonData)
		{
			var result = MakePG24WithRemarkCode("NHE", "NEM");
			result.RemarksText = commonData.EmbassyNationality;
			return result;
		}

		#endregion

		#region PG25

		public static AEPAPG25 MakePG25(IFSISLot lot)
		{
			return new AEPAPG25()
			{
				LotNumber = lot.LotNumber,
				ProductionStartDateOfTheLot = lot.StartDate.Date,
				ProductionEndDateOfTheLot = lot.EndDate.Date
			};
		}

		public static AEPAPG25 MakePG25(ILotCode lot)
		{
			return MakePG25(lot.Code, lot.Value);
		}

		public static AEPAPG25 MakePG25(ZString code, ZString value)
		{
			return new AEPAPG25() { LotNumberQualifier = code, LotNumber = value };
		}

		#endregion

		#region PG26

		internal static AEPAPG26 MakePG26(ZInt packagingLevel, ZDecimal quantity, ZString unit)
		{
			return new AEPAPG26() { PackagingQualifier = packagingLevel, Quantity = quantity, UnitOfMeasurePackagingLevel = unit };
		}

		public static IEnumerable<AEPAPG26> MakePG26(IFSISLot lot)
		{
			var result = new List<AEPAPG26>();

			var pg26 = MakePG26(1, (ZDecimal)lot.Quantity1, lot.UQ1);
			pg26.PackageIdentifier = lot.ShippingMarks;
			result.Add(pg26);

			if (!lot.Quantity2.IsEmpty)
			{
				result.Add(MakePG26(2, (ZDecimal)lot.Quantity2, lot.UQ2));
			}
			return result;
		}

		public static IEnumerable<AEPAPG26> MakePG26(IAMSLine eg1Data)
		{
			var result = new List<AEPAPG26>();
			var packagingLevel = 1;
			if (!eg1Data.OuterPackage.IsEmpty)
			{
				result.Add(MakePG26(packagingLevel, eg1Data.OuterPackage, eg1Data.OuterPackageUQ));
				packagingLevel++;
			}

			if (!eg1Data.InnerPackage.IsEmpty)
			{
				result.Add(MakePG26(packagingLevel, eg1Data.InnerPackage, eg1Data.InnerPackageUQ));
				packagingLevel++;
			}

			if (!eg1Data.InnerAmount.IsEmpty)
			{
				result.Add(MakePG26(packagingLevel, eg1Data.InnerAmount, eg1Data.InnerAmountUQ));
			}

			if (!eg1Data.InnerWeight.IsEmpty)
			{
				result.Add(MakePG26(packagingLevel, eg1Data.InnerWeight, eg1Data.InnerWeightUQ));
			}
			return result;
		}

		public static AEPAPG26 MakePG26(ZDecimal quantity, ZString unit)
		{
			return new AEPAPG26() { Quantity = quantity, UnitOfMeasurePackagingLevel = unit };
		}

		#endregion

		#region PG27

		public static AEPAPG27 MakePG27(ZString equipmentID)
		{
			return new AEPAPG27() { ContainerNumberEquipmentID = equipmentID };
		}

		#endregion

		#region PG29

		public static AEPAPG29 MakePG29(ZDecimal weight, ZString uQ)
		{
			var result = new AEPAPG29();
			result.CommodityNetQuantityPGALineNet = weight;
			result.UnitOfMeasurePGALineNet = uQ;
			return result;
		}

		public static AEPAPG29 MakePG29(IATFData commonData)
		{
			var result = new AEPAPG29();
			if (!commonData.BarrelLength.IsEmpty)
			{
				result.UnitOfMeasureIndividualUnitNet = "IN";
				result.CommodityNetQuantityIndividualUnitNet = commonData.BarrelLength;
			}

			if (!commonData.OverallLength.IsEmpty)
			{
				result.UnitOfMeasureIndividualUnitGross = "IN";
				result.CommodityGrossQuantityIndividualUnitGross = commonData.OverallLength;
			}
			return result;
		}

		#endregion

		#region PG30

		static AEPAPG30 MakePG30(ZString inspectionLocation)
		{
			return new AEPAPG30() { InspectionLaboratoryTestingStatus = InspectionStatusList.Codes.ProductLocationForRegulatoryAuthorityInspection, ArrivalLocation = inspectionLocation };
		}

		static AEPAPG30 MakePG30(ZString inspectionLocation, ZString inspectionDateType)
		{
			return new AEPAPG30() { InspectionLaboratoryTestingStatus = inspectionDateType, ArrivalLocation = inspectionLocation };
		}

		public static AEPAPG30 MakePG30(ZString inspectionDateType, ZString arrivalLocationCode, ZString inspectionLocation)
		{
			return new AEPAPG30() { InspectionLaboratoryTestingStatus = inspectionDateType, AnticipatedArrivalLocationCode = arrivalLocationCode, ArrivalLocation = inspectionLocation };
		}

		public static AEPAPG30 MakePG30(IFSISLine cer)
		{
			var result = MakePG30(cer.ImportingEstNo);
			result.AnticipatedArrivalDate = cer.ScheduleInspectionDate.IsValid ? cer.ScheduleInspectionDate.Date : ZDate.Empty;
			result.AnticipatedArrivalLocationCode = InspectionLocationCodeList.Codes.InspectionEstablishmentNumberQualifier;
			return result;
		}

		public static AEPAPG30 MakePG30(IATFData data)
		{
			var result = new AEPAPG30();
			result.InspectionLaboratoryTestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation;
			result.AnticipatedArrivalDate = data.ArrivalDate.IsValid ? data.ArrivalDate.Date : ZDate.Empty;
			result.ArrivalLocation = data.ArrivalLocation;
			return result;
		}

		public static AEPAPG30 MakePG30(IDEAHeader data)
		{
			return new AEPAPG30()
			{
				InspectionLaboratoryTestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation,
				AnticipatedArrivalDate = data.ArrivalDate.IsValid ? data.ArrivalDate : ZDate.Empty
			};
		}

		#endregion

		#region PG32

		public static AEPAPG32 MakePG32(ZString commodityRouting, ZString exportCountry)
		{
			var result = new AEPAPG32();
			result.CommodityRoutingTypeCode = commodityRouting;
			result.CommodityRoutingCountryCode = exportCountry;
			return result;
		}

		public static AEPAPG30 MakePG30(ZString inspectionDateType, ZDateTime inspectionDateTime, ZString inspectionLocation, bool onlyEstimatedDate = false)
		{
			var result = MakePG30(inspectionLocation, inspectionDateType);
			result.AnticipatedArrivalDate = inspectionDateTime.IsValid ? inspectionDateTime.Date : ZDate.Empty;
			if (!onlyEstimatedDate)
			{
				result.ArrivalTime = inspectionDateTime.IsValid ? inspectionDateTime.ToString("HHmm") : "";
			}
			return result;
		}

		#endregion

		#region PG34

		static AEPAPG34 MakePG34(ZString documentTypeCode, ZString documentNationality, ZString documentIdentifier)
		{
			return new AEPAPG34() { TravelDocumentTypeCode = documentTypeCode, TravelDocumentNationality = documentNationality, TravelDocumentIdentifier = documentIdentifier };
		}

		public static AEPAPG34 MakePG34(INHTSAHeader commonData)
		{
			return MakePG34(commonData.DocumentType, commonData.DocumentNationality, commonData.DocumentNumber);
		}

		#endregion

		#region PG35

		static AEPAPG35 MakePG35(ZString suretyCode, ZString serialNumber, ZString bondQualifier, ZInt bondAmount)
		{
			return new AEPAPG35() { DOTSuretyCode = suretyCode, DOTBondSerialNumber = serialNumber, DOTBondQualifier = bondQualifier, DOTBondAmount = bondAmount };
		}

		public static AEPAPG35 MakePG35(INHTSAHeader commonData)
		{
			return MakePG35(commonData.DOTSuretyCode, commonData.DOTBondSerialNumber, commonData.DOTBondType, commonData.DOTBondAmount);
		}

		#endregion

		#region PG50

		public static AEPAPG50 MakePG50()
		{
			return new AEPAPG50();
		}

		#endregion

		#region PG51

		public static AEPAPG51 MakePG51()
		{
			return new AEPAPG51();
		}

		#endregion

		#region PG55

		public static AEPAPG55 MakePG55(IEnumerable<ZString> roles)
		{
			if (roles.Count() > 10)
			{
				throw new ArgumentException("cannot take more than 10 roles");
			}

			return new AEPAPG55()
			{
				EntityRoleCode = roles.ElementAtOrDefault(0),
				EntityRoleCode1 = roles.ElementAtOrDefault(1),
				EntityRoleCode2 = roles.ElementAtOrDefault(2),
				EntityRoleCode3 = roles.ElementAtOrDefault(3),
				EntityRoleCode4 = roles.ElementAtOrDefault(4),
				EntityRoleCode5 = roles.ElementAtOrDefault(5),
				EntityRoleCode6 = roles.ElementAtOrDefault(6),
				EntityRoleCode7 = roles.ElementAtOrDefault(7),
				EntityRoleCode8 = roles.ElementAtOrDefault(8),
				EntityRoleCode9 = roles.ElementAtOrDefault(9)
			};
		}

		#endregion

		#region PG60

		public static IEnumerable<MessageBlock> MakePG60(ZString additionalInfoType, ZString additionalInfo)
		{
			while (additionalInfo.Length > 72)
			{
				yield return new AEPAPG60() { AdditionalInformationQualifierCode = additionalInfoType, AdditionalInformation = additionalInfo.Left(72) };

				additionalInfo = additionalInfo.SubstringSafe(72);
			}

			if (additionalInfo.Length > 0)
			{
				yield return new AEPAPG60() { AdditionalInformationQualifierCode = additionalInfoType, AdditionalInformation = additionalInfo };
			}
		}

		#endregion

		#region Contact

		public static IEnumerable<MessageBlock> MakeNHTSAContactBlocks(ZString role, IPGAContactDetailsWithID contact)
		{
			if (contact != null && !contact.IDNumber.IsEmpty)
			{
				yield return MakePG19WithNumber(role, contact.IDType, contact.IDNumber);
			}
			else
			{
				foreach (var block in MakeNHTSAContactBlocksWithConditionalPG21(role, contact))
				{
					yield return block;
				}
			}
		}

		public static IEnumerable<MessageBlock> MakeNHTSAContactBlocksWithConditionalPG21(ZString role, IPGAContactDetails contact)
		{
			if (contact != null)
			{
				foreach (var block in MakeEntityBlocks(role, contact.CompanyAddress))
				{
					yield return block;
				}

				if (!IsContactDetailEmpty(contact))
				{
					foreach (var block in MakePG21(role, contact))
					{
						yield return block;
					}
				}
			}
		}

		public static IEnumerable<MessageBlock> MakeFDAPGAContactBlocks(ZString roleCode, OrgCusCodeForFDA customsNumber, IPGAContactDetails contactDetails)
		{
			if (!customsNumber.Number.IsEmpty || contactDetails != null)
			{
				foreach (var block in MakePG19(roleCode, customsNumber, contactDetails))
				{
					yield return block;
				}
			}

			if (contactDetails != null)
			{
				foreach (var block in MakePG20(contactDetails))
				{
					yield return block;
				}

				var contactName = contactDetails.Name;
				if (!contactName.IsEmpty && IsPG21Allowed(roleCode))
				{
					foreach (var block in MakePG21(roleCode, contactDetails))
					{
						yield return block;
					}
				}
			}
		}

		static bool IsPG21Allowed(string roleCode)
		{
			return roleCode == EntityRoleCodeList.Codes.FDAImporter1
				|| roleCode == EntityRoleCodeList.Codes.PointOfContact
				|| roleCode == EntityRoleCodeList.Codes.PNSubmitter
				|| roleCode == EntityRoleCodeList.Codes.PNTransmitter
				|| roleCode == EntityRoleCodeList.Codes.FSVPImporter;
		}

		public static IEnumerable<MessageBlock> MakeFDAIndividualBlocks(ZString roleCode, OrgCusCodeForFDA customsNumber, ICustomsBrokerDetails brokerDetails)
		{
			if (!customsNumber.Number.IsEmpty || brokerDetails != null)
			{
				foreach (var block in MakePG19(roleCode, customsNumber, brokerDetails))
				{
					yield return block;
				}
			}

			if (brokerDetails != null)
			{
				if (brokerDetails.Address != null)
				{
					foreach (var block in MakePG20(brokerDetails.Address))
					{
						yield return block;
					}
				}

				var contactName = brokerDetails.ContactName;

				if (!contactName.IsEmpty && IsPG21Allowed(roleCode))
				{
					foreach (var block in MakePG21(roleCode, brokerDetails.ContactName, brokerDetails.ContactPhone, brokerDetails.ContactEmail, ZString.Empty))
					{
						yield return block;
					}
				}
			}
		}

		public static IEnumerable<MessageBlock> MakeContactBlocks(ZString role, ICustomsBrokerDetails customsBrokerDetails, ZString? idType = null, ZString? idNumber = null, bool isCompanyNameRequiredAlways = false)
		{
			if (customsBrokerDetails != null)
			{
				foreach (var block in MakeEntityBlocks(role, customsBrokerDetails.Address, idType, idNumber, isCompanyNameRequiredAlways: isCompanyNameRequiredAlways))
				{
					yield return block;
				}

				foreach (var block in MakePG21(role, customsBrokerDetails.ContactName, customsBrokerDetails.ContactPhone, customsBrokerDetails.ContactEmail, customsBrokerDetails.Address != null ? customsBrokerDetails.Address.Fax : ZString.Empty))
				{
					yield return block;
				}
			}
		}

		public static IEnumerable<MessageBlock> MakeContactBlocksWithoutOverflowingBlock(ZString role, ICustomsBrokerDetails customsBrokerDetails)
		{
			if (customsBrokerDetails != null)
			{
				var pg19 = new AEPAPG19();
				pg19.EntityRoleCode = role;
				pg19.EntityAddress1 = customsBrokerDetails.Address.AddressLine1.Left(AEPAPG19EntityAddress1MaxLength);
				pg19.EntityName = customsBrokerDetails.Address.CompanyName.Left(AEPAPG19EntityNameMaxLength);
				yield return pg19;

				yield return MakePG20WithoutOverflowingBlock(customsBrokerDetails.Address);

				yield return MakePG21BlockWithoutOverflowingBlock(role, customsBrokerDetails.ContactName, customsBrokerDetails.ContactPhone, customsBrokerDetails.ContactEmail, customsBrokerDetails.Address != null ? customsBrokerDetails.Address.Fax : ZString.Empty);
			}
		}

		public static IEnumerable<MessageBlock> OMCMakeContactBlocks(ZString role, IPGAContactDetails contact, ICustomsBrokerDetails additionalContactInfoForPG21)
		{
			if (contact != null)
			{
				foreach (var block in MakeEntityBlocks(role, contact.CompanyAddress, null, null, isCompanyNameRequiredAlways: true))
				{
					yield return block;
				}

				if (additionalContactInfoForPG21 != null)
				{
					foreach (var block in MakePG21(role, additionalContactInfoForPG21.ContactName, additionalContactInfoForPG21.ContactPhone, additionalContactInfoForPG21.ContactEmail, ZString.Empty))
					{
						yield return block;
					}
				}
			}
		}

		public static IEnumerable<MessageBlock> MakeContactBlocks(ZString role, IPGAContactDetails contact, ZString? idType = null, ZString? idNumber = null, bool isCompanyNameRequiredAlways = false)
		{
			if (contact != null)
			{
				foreach (var block in MakeEntityBlocks(role, contact.CompanyAddress, idType: idType, idNumber: idNumber, isCompanyNameRequiredAlways: isCompanyNameRequiredAlways))
				{
					yield return block;
				}

				foreach (var block in MakePG21(role, contact))
				{
					yield return block;
				}
			}
		}

		public static IEnumerable<MessageBlock> MakeContactBlocksWithoutIndividualQualifierAndName(ZString role, IPGAContactDetails contact)
		{
			if (contact != null)
			{
				foreach (var block in MakeEntityBlocks(role, contact.CompanyAddress))
				{
					yield return block;
				}

				if (!contact.PhoneNumber.IsEmpty || !contact.EmailAddress.IsEmpty || !contact.Fax.IsEmpty)
				{
					foreach (var block in MakePG21(ZString.Empty, ZString.Empty, contact.PhoneNumber, contact.EmailAddress, contact.Fax))
					{
						yield return block;
					}
				}
			}
		}

		public static IEnumerable<MessageBlock> FWSMakeContactBlocks(ZString role, IPGAContactDetails contact, ZString idType, ZString idNumber, ZString declarationCode, ZDate certifySignatureDate)
		{
			if (contact != null)
			{
				foreach (var block in MakeEntityBlocks(role, contact.CompanyAddress, idType, idNumber, true))
				{
					yield return block;
				}

				foreach (var block in MakePG21(role, contact))
				{
					yield return block;
				}
			}
		}

		public static IEnumerable<MessageBlock> FWSMakeContactBlocks(ZString role, ICustomsBrokerDetails customsBrokerDetails, ZString idType, ZString idNumber, ZString declarationCode, ZDate certifySignatureDate)
		{
			if (customsBrokerDetails != null)
			{
				foreach (var block in MakeEntityBlocks(role, customsBrokerDetails.Address, idType, idNumber, true))
				{
					yield return block;
				}

				foreach (var block in MakePG21(role, customsBrokerDetails.ContactName, customsBrokerDetails.ContactPhone, customsBrokerDetails.ContactEmail, customsBrokerDetails.Address != null ? customsBrokerDetails.Address.Fax : ZString.Empty))
				{
					yield return block;
				}
			}
		}

		public static IEnumerable<MessageBlock> FWSMakeContactBlocks(ZString role, IPGAContactDetails contact, ZString? idType, ZString? idNumber, ZString declarationCode, ZDate certifySignatureDate)
		{
			if (contact != null)
			{
				foreach (var block in MakeEntityBlocks(role, contact.CompanyAddress, idType: idType, idNumber: idNumber, isCompanyNameRequiredAlways: true))
				{
					yield return block;
				}
				foreach (var block in MakePG21(role, contact.Name, contact.PhoneNumber, contact.EmailAddress, contact.Fax))
				{
					yield return block;
				}
			}
		}

		public static IEnumerable<MessageBlock> MakeContactBlocks(ZString role, IPGAContactDetails contact, ICustomsBrokerDetails additionalContactInfoForPG21)
		{
			if (contact != null)
			{
				foreach (var block in MakeEntityBlocks(role, contact.CompanyAddress))
				{
					yield return block;
				}

				if (additionalContactInfoForPG21 != null)
				{
					foreach (var block in MakePG21(role, additionalContactInfoForPG21.ContactName, additionalContactInfoForPG21.ContactPhone, additionalContactInfoForPG21.ContactEmail, ZString.Empty))
					{
						yield return block;
					}
				}
				else
				{
					foreach (var block in MakePG21(role, contact.Name, contact.PhoneNumber, contact.EmailAddress, contact.Fax))
					{
						yield return block;
					}
				}
			}
		}

		public static IEnumerable<MessageBlock> MakeContactBlocksWithConditionalPG21(ZString role, IPGAContactDetails contact, ICustomsBrokerDetails additionalContactInfoForPG21)
		{
			if (contact != null)
			{
				foreach (var block in MakeEntityBlocks(role, contact.CompanyAddress))
				{
					yield return block;
				}

				if (additionalContactInfoForPG21 != null)
				{
					foreach (var block in MakePG21(role, additionalContactInfoForPG21.ContactName, additionalContactInfoForPG21.ContactPhone, additionalContactInfoForPG21.ContactEmail, ZString.Empty))
					{
						yield return block;
					}
				}
				else
				{
					if (!IsContactDetailEmpty(contact))
					{
						foreach (var block in MakePG21(role, contact))
						{
							yield return block;
						}
					}
				}
			}
		}

		public static IEnumerable<MessageBlock> MakeContactBlocks(ZString pg19role, ZString pg21role, IPGAContactDetailsWithID contact)
		{
			if (contact != null)
			{
				foreach (var block in MakePG19(pg19role, contact))
				{
					yield return block;
				}

				foreach (var block in MakePG20(contact))
				{
					yield return block;
				}

				foreach (var block in MakePG21(pg21role, contact))
				{
					yield return block;
				}
			}
		}

		public static IEnumerable<MessageBlock> MakeContactBlocksWithoutOverflowingBlock(ZString pg19role, ZString pg21role, IPGAContactDetailsWithID contact)
		{
			if (contact != null)
			{
				var pg19 = MakePG19WithoutOverflowingBlock(pg19role, contact);
				if (pg19 != null)
				{
					yield return pg19;
				}

				yield return MakePG20BlockWithoutOverflowingBlock(contact.CompanyAddress.AddressLine2, contact.CompanyAddress.City, contact.CompanyAddress.State, contact.CompanyAddress.Country, contact.CompanyAddress.PostCode);

				yield return MakePG21BlockWithoutOverflowingBlock(pg21role, contact.Name, contact.PhoneNumber, contact.EmailAddress, contact.Fax);
			}
		}

		public static IEnumerable<MessageBlock> MakeContactBlocksForOR1(ZString pg19role, ZString pg21role, IPGAContactDetailsWithID contact)
		{
			if (contact != null)
			{
				var pg19 = MakePG19WithoutOverflowingBlock(pg19role, contact);
				if (pg19 != null)
				{
					yield return pg19;
				}

				yield return MakePG20BlockWithoutOverflowingBlock(contact.CompanyAddress.AddressLine2, contact.CompanyAddress.City, contact.CompanyAddress.State, contact.CompanyAddress.Country, contact.CompanyAddress.PostCode);

				yield return MakePG21BlockWithoutOverflowingBlock(pg21role, contact.Name, contact.PhoneNumber, contact.EmailAddress, ZString.Empty);
			}
		}

		public static IEnumerable<MessageBlock> MakeEntityBlocks(ZString role, IAddressDetails addressDetails, ZString? idType = null, ZString? idNumber = null, bool isCompanyNameRequiredAlways = false)
		{
			if (addressDetails != null)
			{
				var pg19 = new AEPAPG19();
				pg19.EntityIdentificationCode = idType.GetValueOrDefault();
				pg19.EntityRoleCode = role;
				var addressLine1 = addressDetails.AddressLine1;
				pg19.EntityAddress1 = addressLine1.Left(AEPAPG19EntityAddress1MaxLength);
				var idNumberHasValidValue = idNumber.HasValue && !idNumber.Value.IsEmpty;

				if (idNumberHasValidValue)
				{
					pg19.EntityNumber = idNumber.Value.Left(AEPAPG19EntityNumberMaxLength);
				}

				var companyName = addressDetails.CompanyName;
				var entityNameRequired = !idNumberHasValidValue || isCompanyNameRequiredAlways;

				if (entityNameRequired)
				{
					pg19.EntityName = companyName.Left(AEPAPG19EntityNameMaxLength);
				}

				yield return pg19;

				if (entityNameRequired && companyName.Length > AEPAPG19EntityNameMaxLength)
				{
					foreach (var block in MakeEntityNameOverflowBlock(companyName))
					{
						yield return block;
					}
				}

				if (addressLine1.Length > AEPAPG19EntityAddress1MaxLength)
				{
					foreach (var block in MakeEntityAddress1OverflowBlock(addressLine1))
					{
						yield return block;
					}
				}

				foreach (var block in MakePG20(addressDetails))
				{
					yield return block;
				}
			}
		}

		public static IEnumerable<MessageBlock> MakeEntityNameOverflowBlock(ZString entityName)
		{
			return MakePG60(AdditionalInformationQualifierList.Codes.EntityNameOverflow, entityName.SubstringSafe(AEPAPG19EntityNameMaxLength));
		}

		public static IEnumerable<MessageBlock> MakeEntityAddress1OverflowBlock(ZString entityAddress1)
		{
			return MakePG60(AdditionalInformationQualifierList.Codes.EntityAddress1Overflow, entityAddress1.SubstringSafe(AEPAPG19EntityAddress1MaxLength));
		}

		public static IEnumerable<MessageBlock> MakeEntityAddress2OverflowBlock(ZString entityAddress2)
		{
			return MakePG60(AdditionalInformationQualifierList.Codes.EntityAddress2Overflow, entityAddress2.SubstringSafe(AEPAPG20EntityAddress2MaxLength));
		}

		public static IEnumerable<MessageBlock> MakeIndividualNameOverflowBlock(ZString contactName)
		{
			return MakePG60(AdditionalInformationQualifierList.Codes.IndividualNameOverflow, contactName.SubstringSafe(AEPAPG21IndividualNameMaxLength));
		}

		public static IEnumerable<MessageBlock> MakeEmailOverflowBlock(ZString email)
		{
			return MakePG60(AdditionalInformationQualifierList.Codes.EmailOverflow, email.SubstringSafe(AEPAPG21EmailMaxLength));
		}

		static bool IsContactDetailEmpty(IPGAContactDetails contact)
		{
			return contact.Name.IsEmpty && contact.PhoneNumber.IsEmpty && contact.EmailAddress.IsEmpty && contact.Fax.IsEmpty;
		}

		public const int AEPAPG19EntityNameMaxLength = 32;
		public const int AEPAPG19EntityNumberMaxLength = 15;
		public const int AEPAPG19EntityAddress1MaxLength = 23;
		public const int AEPAPG20EntityAddress2MaxLength = 32;
		public const int AEPAPG21EmailMaxLength = 35;
		public const int AEPAPG21IndividualNameMaxLength = 23;

		#endregion
	}
}
