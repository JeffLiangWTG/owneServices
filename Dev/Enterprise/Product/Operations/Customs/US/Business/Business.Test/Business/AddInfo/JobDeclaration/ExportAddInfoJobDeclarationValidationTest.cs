using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.LicenseManager;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ExportAddInfoJobDeclarationValidationTest : AddInfoJobDeclarationValidationAbstractTest
	{
		public void TestCheckUS_HazardousCargo()
		{
			string messageError = "The code you have selected is not in the list.";
			Declaration.US_HazardousCargo = "";
			AssertNoMessageError(Declaration.US_HazardousCargoInfo, messageError);
			Declaration.US_HazardousCargo = "G";
			AssertHasMessageError(Declaration.US_HazardousCargoInfo, messageError);

			CodeDescriptionPairList list = Declaration.AddInfoLookups.US_YesNoList;
			foreach (CodeDescriptionPair pair in list)
			{
				Declaration.US_HazardousCargo = pair.Code;
				AssertNoMessageError(Declaration.US_HazardousCargoInfo, messageError);
			}

			Declaration.US_HazardousCargo = "k";
			AssertHasMessageError(Declaration.US_HazardousCargoInfo, messageError);
			Declaration.US_HazardousCargo = " ";
			AssertNoMessageError(Declaration.US_HazardousCargoInfo, messageError);
		}

		public void TestCheckUS_JurisdictionNumber()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESJurisdictionNumber, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				Declaration.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.AircraftAndAssociatedEquipment;
				Assert("US_JurisdictionNumber is read-only when US_DDTCUSMLCategoryCode is not equal to 21", Declaration.US_JurisdictionNumberInfo.ReadOnly);

				Declaration.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.MiscellaneousArticles;
				Declaration.US_JurisdictionNumber = "";
				AssertHasMessageError(Declaration.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				Declaration.US_JurisdictionNumber = "1CJ1234567";
				AssertHasMessageError(Declaration.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				Declaration.US_JurisdictionNumber = "CJ12345678";
				AssertHasMessageError(Declaration.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				Declaration.US_JurisdictionNumber = "CJ 1234567";
				AssertHasMessageError(Declaration.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				Declaration.US_JurisdictionNumber = "CJ1234567";
				AssertNoMessageError(Declaration.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				Declaration.US_JurisdictionNumber = "CJ1234-67";
				AssertHasMessageError(Declaration.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				Declaration.US_JurisdictionNumber = "CJ 1234-67";
				AssertNoMessageError(Declaration.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);
			}
		}

		public void TestCheckUS_CommodityFilingOption()
		{
			var messageError = "The code you have selected is not in the list.";
			Declaration.US_CommodityFilingOption = "";
			AssertNoMessageError(Declaration.US_CommodityFilingOptionInfo, messageError);
			AssertHasMessageErrorContaining(Declaration.US_CommodityFilingOptionInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.US_CommodityFilingOption = "B";
			AssertHasMessageError(Declaration.US_CommodityFilingOptionInfo, messageError);
			AssertNoMessageErrorContaining(Declaration.US_CommodityFilingOptionInfo, MandatoryValidation.YouHaveNotEntered);

			var list = new AESCommodityFilingOptionList();
			foreach (CodeDescriptionPair pair in list)
			{
				Declaration.US_CommodityFilingOption = pair.Code;
				AssertNoMessageError(Declaration.US_CommodityFilingOptionInfo, messageError);
			}

			Declaration.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.Ammunition;
			Declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._2Predeparture;
			AssertNoError(Declaration.US_CommodityFilingOptionInfo, ExportAddInfoJobDeclarationValidation.USMLCannotBePostDepartureFiling);

			Declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._4Postdeparture;
			AssertHasError(Declaration.US_CommodityFilingOptionInfo, ExportAddInfoJobDeclarationValidation.USMLCannotBePostDepartureFiling);

			Declaration.US_DDTCUSMLCategoryCode = "";
			Declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._2Predeparture;
			AssertNoError(Declaration.US_CommodityFilingOptionInfo, ExportAddInfoJobDeclarationValidation.USMLCannotBePostDepartureFiling);

			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 100m;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.Firearms;
			AssertNoError(Declaration.US_CommodityFilingOptionInfo, ExportAddInfoJobDeclarationValidation.USMLCannotBePostDepartureFiling);

			Declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._4Postdeparture;
			AssertHasError(Declaration.US_CommodityFilingOptionInfo, ExportAddInfoJobDeclarationValidation.USMLCannotBePostDepartureFiling);
		}

		public void TestCheckUS_TransactionsRelated()
		{
			string messageError = "The code you have selected is not in the list.";
			Declaration.US_TransactionsRelated = "";
			AssertNoMessageError(Declaration.US_TransactionsRelatedInfo, messageError);
			Declaration.US_TransactionsRelated = "G";
			AssertHasMessageError(Declaration.US_TransactionsRelatedInfo, messageError);

			CodeDescriptionPairList list = Declaration.AddInfoLookups.US_YesNoList;
			foreach (CodeDescriptionPair pair in list)
			{
				Declaration.US_TransactionsRelated = pair.Code;
				AssertNoMessageError(Declaration.US_TransactionsRelatedInfo, messageError);
			}

			Declaration.US_TransactionsRelated = "k";
			AssertHasMessageError(Declaration.US_TransactionsRelatedInfo, messageError);
			Declaration.US_TransactionsRelated = " ";
			AssertNoMessageError(Declaration.US_TransactionsRelatedInfo, messageError);
		}

		public void TestCheckUS_RoutedTransaction()
		{
			string messageError = "The code you have selected is not in the list.";
			Declaration.US_RoutedTransaction = "";
			AssertNoMessageError(Declaration.US_RoutedTransactionInfo, messageError);
			Declaration.US_RoutedTransaction = "G";
			AssertHasMessageError(Declaration.US_RoutedTransactionInfo, messageError);

			CodeDescriptionPairList list = Declaration.AddInfoLookups.US_YesNoList;
			foreach (CodeDescriptionPair pair in list)
			{
				Declaration.US_RoutedTransaction = pair.Code;
				AssertNoMessageError(Declaration.US_RoutedTransactionInfo, messageError);
			}

			Declaration.US_RoutedTransaction = "k";
			AssertHasMessageError(Declaration.US_RoutedTransactionInfo, messageError);
			Declaration.US_RoutedTransaction = " ";
			AssertNoMessageError(Declaration.US_RoutedTransactionInfo, messageError);
		}

		public void TestCheckUS_InbondType()
		{
			Declaration.US_InbondType = "";
			AssertNoMessageError(Declaration.US_InbondTypeInfo, ExportAddInfoJobDeclarationValidation.InbondTypeShouldBeInList);

			Declaration.US_InbondType = "BA";
			AssertHasMessageError(Declaration.US_InbondTypeInfo, ExportAddInfoJobDeclarationValidation.InbondTypeShouldBeInList);

			InbondTypeList list = new InbondTypeList();
			foreach (CodeDescriptionPair pair in list)
			{
				Declaration.US_InbondType = pair.Code;
				AssertNoMessageError(Declaration.US_InbondTypeInfo, ExportAddInfoJobDeclarationValidation.InbondTypeShouldBeInList);
			}

			Declaration.US_InbondType = " ";
			AssertNoMessageError(Declaration.US_InbondTypeInfo, ExportAddInfoJobDeclarationValidation.InbondTypeShouldBeInList);
		}

		public void TestCheckUS_ExportCode()
		{
			string messageError = "The code you have selected is not in the list.";
			Declaration.US_ExportCode = "";
			AssertNoMessageError(Declaration.US_ExportCodeInfo, messageError);

			Declaration.US_ExportCode = "BA";
			AssertHasMessageError(Declaration.US_ExportCodeInfo, messageError);

			var list = new ExportInformationCodeList();
			foreach (CodeDescriptionPair pair in list)
			{
				Declaration.US_ExportCode = pair.Code;
				AssertNoMessageError(Declaration.US_ExportCodeInfo, messageError);
			}

			Declaration.US_ExportCode = " ";
			AssertNoMessageError(Declaration.US_ExportCodeInfo, messageError);

			Declaration.US_LicenseType = USAESLicenseCode.Codes.C30;
			Declaration.US_ExportCode = ExportInformationCodeList.Codes.MS;
			messageError = LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C30, ExportInformationCodeList.Codes.MS);
			AssertHasMessageError(Declaration.US_ExportCodeInfo, messageError);

			Declaration.US_ExportCode = ExportInformationCodeList.Codes.OS;
			AssertNoMessageError(Declaration.US_ExportCodeInfo, messageError);
		}

		public void TestCheckUS_ExportCodeAdditional()
		{
			Declaration.US_LicenseType = USAESLicenseCode.Codes.C30;
			Declaration.US_ExportCode = ExportInformationCodeList.Codes.CH;
			AssertNoMessageErrors(Declaration.US_ExportCodeInfo);

			Declaration.US_LicenseType = USAESLicenseCode.Codes.OPA;
			Declaration.US_ExportCode = ExportInformationCodeList.Codes.UG;
			AssertHasMessageErrors(Declaration.US_ExportCodeInfo);

			Declaration.US_LicenseType = USAESLicenseCode.Codes.VDS;
			Declaration.US_ExportCode = ExportInformationCodeList.Codes.CH;
			AssertHasMessageErrors(Declaration.US_ExportCodeInfo);

			Declaration.US_LicenseType = USAESLicenseCode.Codes.VDO;
			Declaration.US_ExportCode = ExportInformationCodeList.Codes.FI;
			AssertHasMessageErrors(Declaration.US_ExportCodeInfo);
		}

		public void TestCheckUS_LicenseNo()
		{
			var factory = Declaration.Factory;
			var exportDate = Declaration.GetEffectiveDateForECR();

			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(factory, new ZString[] { USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C60 });

			Declaration.US_LicenseType = USAESLicenseCode.Codes.C30;
			string messageError = LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C30, "", factory, exportDate);

			Declaration.US_LicenseNo = "";
			AssertHasMessageError(Declaration.US_LicenseNoInfo, messageError);
			AssertNoWarning(Declaration.US_LicenseNoInfo, ExportAddInfoJobDeclarationValidation.BISLicenseWarning);

			Declaration.US_LicenseNo = "LIC2343";
			AssertNoMessageError(Declaration.US_LicenseNoInfo, messageError);
			string invalidMessageError = LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C30, Declaration.US_LicenseNo, factory, exportDate);
			AssertHasMessageError(Declaration.US_LicenseNoInfo, invalidMessageError);
			AssertNoWarning(Declaration.US_LicenseNoInfo, ExportAddInfoJobDeclarationValidation.BISLicenseWarning);

			Declaration.US_LicenseNo = "D752343";
			string validLicenseCheck = LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C30, Declaration.US_LicenseNo, factory, exportDate);
			AssertNoMessageError(Declaration.US_LicenseNoInfo, invalidMessageError);
			AssertEquals("Licence number starting with 'D' is valid for C30", "", validLicenseCheck);
			AssertHasWarning(Declaration.US_LicenseNoInfo, ExportAddInfoJobDeclarationValidation.BISLicenseWarning);

			Declaration.US_LicenseType = USAESLicenseCode.Codes.C60;
			Declaration.US_LicenseNo = "ABC";
			AssertHasMessageError(Declaration.US_LicenseNoInfo, LicenseNumberValidation.C60LicenseInvalid);
			Declaration.US_LicenseNo = LicenseExemptionTypeList.Codes.DY6;
			AssertNoMessageError(Declaration.US_LicenseNoInfo, LicenseNumberValidation.C60LicenseInvalid);
		}

		public void TestcheckUS_LicenseNoWithT10LicenseTypeInDeclaration()
		{
			Declaration.US_LicenseType = USAESLicenseCode.Codes.T10;
			Declaration.US_LicenseNo = "12-4678";
			AssertHasMessageError(Declaration.US_LicenseNoInfo, LicenseNumberValidation.T10LicenseInvalid);
			Declaration.US_LicenseNo = "AA-2014-1";
			AssertHasMessageError(Declaration.US_LicenseNoInfo, LicenseNumberValidation.T10LicenseInvalid);
			Declaration.US_LicenseNo = "AA20141";
			AssertNoMessageError(Declaration.US_LicenseNoInfo, LicenseNumberValidation.T10LicenseInvalid);
		}

		public void TestCheckUS_ECCN()
		{
			var factory = Declaration.Factory;
			var exportDate = Declaration.GetEffectiveDateForECR();

			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(factory, new ZString[] { USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C59 });

			Declaration.US_LicenseType = USAESLicenseCode.Codes.C30;
			string messageError = LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C30, "", factory, exportDate);

			Declaration.US_ECCN = "";
			AssertHasMessageError(Declaration.US_ECCNInfo, messageError);
			Declaration.US_ECCN = "C2343";
			AssertNoMessageError(Declaration.US_ECCNInfo, messageError);

			Declaration.US_LicenseType = USAESLicenseCode.Codes.C33;
			Declaration.US_ECCN = "1C352";
			AssertHasMessageError(Declaration.US_ECCNInfo, string.Format(ECCNValidation.ECCNNotAllowed, "1C352", USAESLicenseCode.Codes.C33));

			Declaration.US_LicenseType = USAESLicenseCode.Codes.C32;
			Declaration.US_ECCN = "1C351";
			AssertHasMessageError(Declaration.US_ECCNInfo, string.Format(ECCNValidation.ECCNNotAllowed, "1C351", USAESLicenseCode.Codes.C32));

			Declaration.US_ECCN = "1C355";
			AssertNoMessageErrors(Declaration.US_ECCNInfo);
		}

		public void TestValidationC32AndC33Licence()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eccnNumber1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "0A606", "0A606", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(eccnNumber1.PK, Universal.RefCusCodeListAttributeTypes.Codes.AllowECCNNLR, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.NotEligible);

			var eccnNumber2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "3A611", "3A611", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(eccnNumber2.PK, Universal.RefCusCodeListAttributeTypes.Codes.AllowECCNNLR, Core.Constants.CountryCodes.Australia);
			helper.CreateNewOrGetExistingCusCodeListAttribute(eccnNumber2.PK, Universal.RefCusCodeListAttributeTypes.Codes.AllowECCNNLR, Core.Constants.CountryCodes.UnitedKingdom);
			Factory.Save();

			var licenseTypeList = new ZString[] { USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33 };

			foreach (var licenseType in licenseTypeList)
			{
				Declaration.US_LicenseType = licenseType;
				Declaration.US_ECCN = "0A606";
				AssertHasMessageError(Declaration.US_ECCNInfo, ECCNValidation.ECCN600SeriesDotYNotEligible);
				Declaration.US_RN_NKCountryOfDestination = ZString.Empty;
				Declaration.US_ECCN = "3A611";
				AssertHasMessageError(Declaration.US_ECCNInfo, ECCNValidation.ECCN600SeriesDotYNotEligible);
				Declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Australia;
				Declaration.AddInfoValidation.ValidateUS_ECCN();
				AssertNoMessageError(Declaration.US_ECCNInfo, ECCNValidation.ECCN600SeriesDotYNotEligible);
				Declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedKingdom;
				Declaration.AddInfoValidation.ValidateUS_ECCN();
				AssertNoMessageError(Declaration.US_ECCNInfo, ECCNValidation.ECCN600SeriesDotYNotEligible);
			}
		}

		public void TestCheckUS_DDTCITARExemptionNo()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS }, false);

			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ITARExemptionNumber);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ITARExemptionNumber, "123.11B", "22 CFR 123.11 (b)", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ITARExemptionNumber, "123.12", "22 CFR 123.12", startDate, endDate);
			Factory.Save();

			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAG,
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00
				};

			Declaration.US_LicenseType = ZString.Empty;
			Declaration.US_DDTCITARExemptionNo = ZString.Empty;
			AssertNoMessageErrors(Declaration.US_DDTCITARExemptionNoInfo);

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString licenseType in requireDDTCDataList)
			{
				Declaration.US_LicenseType = licenseType;
				Declaration.US_DDTCITARExemptionNo = ZString.Empty;
				AssertHasMessageError(Declaration.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);
				AssertNoMessageError(Declaration.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberShouldNotBeEntered);
				Declaration.US_DDTCITARExemptionNo = "123.11B";
				AssertNoMessageError(Declaration.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);
				AssertNoMessageError(Declaration.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberShouldNotBeEntered);
			}

			foreach (ICodeDescription code in list)
			{
				if (!requireDDTCDataList.Contains(code.Code))
				{
					Declaration.US_LicenseType = code.Code;
					Declaration.US_DDTCITARExemptionNo = ZString.Empty;
					AssertNoMessageError(Declaration.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);
					AssertNoMessageError(Declaration.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberShouldNotBeEntered);
					Declaration.US_DDTCITARExemptionNo = "123.11B";
					AssertNoMessageError(Declaration.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);
					AssertHasMessageError(Declaration.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberShouldNotBeEntered);
				}
			}

			Declaration.US_LicenseType = USAESLicenseCode.Codes.SAG;
			Declaration.US_DDTCITARExemptionNo = "ZDFSDF";
			AssertNoMessageError(Declaration.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);
			AssertHasMessageErrorContaining(Declaration.US_DDTCITARExemptionNoInfo, ListValidation.InvalidCodeMessageError);

			Declaration.US_DDTCITARExemptionNo = "123.12";
			AssertNoMessageError(Declaration.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);
			AssertNoMessageErrorContaining(Declaration.US_DDTCITARExemptionNoInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DDTCRegistrationNo()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS });

			var requireDDTCDataList = new ZString[]
			{
				USAESLicenseCode.Codes.SAG,
				USAESLicenseCode.Codes.SAU,
				USAESLicenseCode.Codes.SCA,
				USAESLicenseCode.Codes.SGB,
				USAESLicenseCode.Codes.S05,
				USAESLicenseCode.Codes.S61,
				USAESLicenseCode.Codes.S73,
				USAESLicenseCode.Codes.S85,
				USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS
			};

			var allowedDDTCDataList = new ZString[]
			{
				USAESLicenseCode.Codes.S00
			};

			Declaration.US_LicenseType = ZString.Empty;
			Declaration.US_DDTCRegistrationNo = ZString.Empty;
			AssertNoMessageErrors(Declaration.US_DDTCRegistrationNoInfo);

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString licenseType in requireDDTCDataList)
			{
				Declaration.US_LicenseType = licenseType;
				Declaration.US_DDTCRegistrationNo = ZString.Empty;
				AssertHasMessageError(Declaration.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage);
				AssertNoMessageError(Declaration.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberShouldNotBeEntered);
				Declaration.US_DDTCRegistrationNo = "REG123";
				AssertNoMessageError(Declaration.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage);
				AssertNoMessageError(Declaration.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberShouldNotBeEntered);
			}

			foreach (ICodeDescription code in list)
			{
				if (!requireDDTCDataList.Contains(code.Code) && !allowedDDTCDataList.Contains(code.Code))
				{
					Declaration.US_LicenseType = code.Code;
					Declaration.US_DDTCRegistrationNo = ZString.Empty;
					AssertNoMessageError(Declaration.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage);
					AssertNoMessageError(Declaration.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberShouldNotBeEntered);
					Declaration.US_DDTCRegistrationNo = "REG123";
					AssertNoMessageError(Declaration.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage);
					AssertHasMessageError(Declaration.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberShouldNotBeEntered);
				}
			}

			Declaration.US_LicenseType = USAESLicenseCode.Codes.SAG;
			Declaration.US_DDTCRegistrationNo = ZString.Empty;
			AssertHasMessageError(Declaration.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage);

			Declaration.US_DDTCRegistrationNo = "REG123";
			AssertNoMessageError(Declaration.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage);
		}

		public void TestCheckUS_DDTCMilitaryEquipmentIndicator()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS });

			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAG,
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00,
					USAESLicenseCode.Codes.S05,
					USAESLicenseCode.Codes.S61,
					USAESLicenseCode.Codes.S73,
					USAESLicenseCode.Codes.S85,
					USAESLicenseCode.Codes.S94,
					USAESLicenseCode.Codes.VDS
				};

			Declaration.US_LicenseType = ZString.Empty;
			Declaration.US_DDTCMilitaryEquipmentIndicator = ZString.Empty;
			AssertNoMessageErrors(Declaration.US_DDTCMilitaryEquipmentIndicatorInfo);

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString licenseType in requireDDTCDataList)
			{
				Declaration.US_LicenseType = licenseType;
				Declaration.US_DDTCMilitaryEquipmentIndicator = ZString.Empty;
				AssertHasMessageError(Declaration.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);
				AssertNoMessageError(Declaration.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorShouldNotBeEntered);
				Declaration.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.Yes;
				AssertNoMessageError(Declaration.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);
				AssertNoMessageError(Declaration.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorShouldNotBeEntered);
			}

			foreach (ICodeDescription code in list)
			{
				if (!requireDDTCDataList.Contains(code.Code))
				{
					Declaration.US_LicenseType = code.Code;
					Declaration.US_DDTCMilitaryEquipmentIndicator = ZString.Empty;
					AssertNoMessageError(Declaration.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);
					AssertNoMessageError(Declaration.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorShouldNotBeEntered);
					Declaration.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.Yes;
					AssertNoMessageError(Declaration.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);
					AssertHasMessageError(Declaration.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorShouldNotBeEntered);
				}
			}

			Declaration.US_LicenseType = USAESLicenseCode.Codes.SAG;
			Declaration.US_DDTCMilitaryEquipmentIndicator = "Z";
			AssertNoMessageError(Declaration.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);
			AssertHasMessageErrorContaining(Declaration.US_DDTCMilitaryEquipmentIndicatorInfo, ListValidation.InvalidCodeMessageError);

			Declaration.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.No;
			AssertNoMessageError(Declaration.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);
			AssertNoMessageErrorContaining(Declaration.US_DDTCMilitaryEquipmentIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DDTCPartyCertificationIndicator()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS });

			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00
				};

			Declaration.US_LicenseType = ZString.Empty;
			Declaration.US_DDTCPartyCertificationIndicator = ZString.Empty;
			AssertNoMessageErrors(Declaration.US_DDTCPartyCertificationIndicatorInfo);

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString licenseType in requireDDTCDataList)
			{
				Declaration.US_LicenseType = licenseType;
				Declaration.US_DDTCPartyCertificationIndicator = ZString.Empty;
				AssertHasMessageError(Declaration.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);
				AssertNoMessageError(Declaration.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorShouldNotBeEntered);
				Declaration.US_DDTCPartyCertificationIndicator = YesNoDefaultList.Codes.Yes;
				AssertNoMessageError(Declaration.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);
				AssertNoMessageError(Declaration.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorShouldNotBeEntered);
			}

			foreach (ICodeDescription code in list)
			{
				if (!requireDDTCDataList.Contains(code.Code))
				{
					Declaration.US_LicenseType = code.Code;
					Declaration.US_DDTCPartyCertificationIndicator = ZString.Empty;
					AssertNoMessageError(Declaration.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);
					AssertNoMessageError(Declaration.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorShouldNotBeEntered);
					Declaration.US_DDTCPartyCertificationIndicator = YesNoDefaultList.Codes.Yes;
					AssertNoMessageError(Declaration.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);
					AssertHasMessageError(Declaration.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorShouldNotBeEntered);
				}
			}

			Declaration.US_LicenseType = USAESLicenseCode.Codes.SCA;
			Declaration.US_DDTCPartyCertificationIndicator = "Z";
			AssertNoMessageError(Declaration.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);
			AssertHasMessageErrorContaining(Declaration.US_DDTCPartyCertificationIndicatorInfo, ListValidation.InvalidCodeMessageError);

			Declaration.US_DDTCPartyCertificationIndicator = YesNoDefaultList.Codes.No;
			AssertNoMessageError(Declaration.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);
			AssertNoMessageErrorContaining(Declaration.US_DDTCPartyCertificationIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DDTCUSMLCategoryCode()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS });

			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAG,
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00,
					USAESLicenseCode.Codes.S05,
					USAESLicenseCode.Codes.S61,
					USAESLicenseCode.Codes.S73,
					USAESLicenseCode.Codes.S85,
					USAESLicenseCode.Codes.S94,
					USAESLicenseCode.Codes.VDS
				};

			Declaration.US_LicenseType = ZString.Empty;
			Declaration.US_DDTCUSMLCategoryCode = ZString.Empty;
			AssertNoMessageErrors(Declaration.US_DDTCUSMLCategoryCodeInfo);

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString licenseType in requireDDTCDataList)
			{
				Declaration.US_LicenseType = licenseType;
				Declaration.US_DDTCUSMLCategoryCode = ZString.Empty;
				AssertHasMessageError(Declaration.US_DDTCUSMLCategoryCodeInfo, ClassificationValidator.DDTCUSMLCategoryCodeIsRequiredMessage);
				AssertNoMessageError(Declaration.US_DDTCUSMLCategoryCodeInfo, ClassificationValidator.DDTCUSMLCategoryCodeShouldNotBeEntered);
				Declaration.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.Ammunition;
				AssertNoMessageError(Declaration.US_DDTCUSMLCategoryCodeInfo, ClassificationValidator.DDTCUSMLCategoryCodeIsRequiredMessage);
				AssertNoMessageError(Declaration.US_DDTCUSMLCategoryCodeInfo, ClassificationValidator.DDTCUSMLCategoryCodeShouldNotBeEntered);
			}

			foreach (ICodeDescription code in list)
			{
				if (!requireDDTCDataList.Contains(code.Code))
				{
					Declaration.US_LicenseType = code.Code;
					Declaration.US_DDTCUSMLCategoryCode = ZString.Empty;
					AssertNoMessageError(Declaration.US_DDTCUSMLCategoryCodeInfo, ClassificationValidator.DDTCUSMLCategoryCodeIsRequiredMessage);
					AssertNoMessageError(Declaration.US_DDTCUSMLCategoryCodeInfo, ClassificationValidator.DDTCUSMLCategoryCodeShouldNotBeEntered);
					Declaration.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.Ammunition;
					AssertNoMessageError(Declaration.US_DDTCUSMLCategoryCodeInfo, ClassificationValidator.DDTCUSMLCategoryCodeIsRequiredMessage);
					AssertHasMessageError(Declaration.US_DDTCUSMLCategoryCodeInfo, ClassificationValidator.DDTCUSMLCategoryCodeShouldNotBeEntered);
				}
			}

			Declaration.US_LicenseType = USAESLicenseCode.Codes.SAG;
			Declaration.US_DDTCUSMLCategoryCode = "ZZ";
			AssertNoMessageError(Declaration.US_DDTCUSMLCategoryCodeInfo, ClassificationValidator.DDTCUSMLCategoryCodeIsRequiredMessage);
			AssertHasMessageErrorContaining(Declaration.US_DDTCUSMLCategoryCodeInfo, ListValidation.InvalidCodeMessageError);

			foreach (ICodeDescription code in new USMLCategoryCodes())
			{
				Declaration.US_DDTCUSMLCategoryCode = code.Code;
				AssertNoMessageError(Declaration.US_DDTCUSMLCategoryCodeInfo, ClassificationValidator.DDTCUSMLCategoryCodeIsRequiredMessage);
				AssertNoMessageErrorContaining(Declaration.US_DDTCUSMLCategoryCodeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		[TestDate(2016, 09, 20)]
		public void TestLicenseTypeExemptionValidation()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Declaration.Factory, new ZString[] { USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32 });

			Declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			Declaration.US_LicenseType = "";
			Declaration.US_LicenseNo = "";
			Declaration.US_ECCN = "";
			Declaration.US_ExportCode = "";

			AssertNoMessageErrors(Declaration.US_LicenseTypeInfo);
			AssertNoMessageErrors(Declaration.US_LicenseNoInfo);
			AssertNoMessageErrors(Declaration.US_ECCNInfo);
			AssertNoMessageErrors(Declaration.US_ExportCodeInfo);

			Declaration.US_LicenseType = USAESLicenseCode.Codes.C30;
			AssertNoMessageErrors(Declaration.US_LicenseTypeInfo);
			AssertHasMessageErrors(Declaration.US_LicenseNoInfo);
			AssertHasMessageErrors(Declaration.US_ECCNInfo);
			AssertHasMessageErrors(Declaration.US_ExportCodeInfo);

			Declaration.US_LicenseType = USAESLicenseCode.Codes.C32;
			AssertHasMessageErrors(Declaration.US_LicenseTypeInfo);
			AssertNoMessageErrors(Declaration.US_LicenseNoInfo);
			AssertHasMessageErrors(Declaration.US_ECCNInfo);
			AssertHasMessageErrors(Declaration.US_ExportCodeInfo);

			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Declaration.US_LicenseType = USAESLicenseCode.Codes.C32;
			AssertNoMessageErrors(Declaration.US_LicenseTypeInfo);
			AssertNoMessageErrors(Declaration.US_LicenseNoInfo);
			AssertHasMessageErrors(Declaration.US_ECCNInfo);
			AssertHasMessageErrors(Declaration.US_ExportCodeInfo);
		}

		public void TestCheckUS_RL_NKPortOfExport()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			Declaration.US_RL_NKPortOfExport = "";
			AssertNoMessageErrorContaining(Declaration.US_RL_NKPortOfExportInfo, ListValidation.InvalidCodeMessageError);
			Declaration.US_RL_NKPortOfExport = helper.AUSYD.Code;
			AssertHasMessageErrorContaining(Declaration.US_RL_NKPortOfExportInfo, ListValidation.InvalidCodeMessageError);
			Declaration.US_RL_NKPortOfExport = helper.USLAX.Code;
			AssertNoMessageErrorContaining(Declaration.US_RL_NKPortOfExportInfo, ListValidation.InvalidCodeMessageError);

			var portVICTD = helper.CreateUnlocoIfNotExists("VICTD", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.VirginIslands));
			Declaration.US_RL_NKPortOfExport = portVICTD.Code;
			AssertNoMessageErrorContaining(Declaration.US_RL_NKPortOfExportInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_TariffType()
		{
			Declaration.US_TariffType = ZString.Empty;
			AssertHasMessageErrorContaining(Declaration.US_TariffTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Declaration.US_TariffTypeInfo, ListValidation.InvalidCodeMessageError);

			var list = new TariffTypeList();
			foreach (CodeDescriptionPair pair in list)
			{
				Declaration.US_TariffType = pair.Code;
				AssertNoMessageErrorContaining(Declaration.US_TariffTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(Declaration.US_TariffTypeInfo, ListValidation.InvalidCodeMessageError);
			}

			Declaration.US_TariffType = "ZSD";
			AssertNoMessageErrorContaining(Declaration.US_TariffTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Declaration.US_TariffTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2016, 09, 20)]
		public void TestCheckUS_LicenseType()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS });

			Declaration.US_LicenseType = "";
			AssertNoMessageErrorContaining(Declaration.US_LicenseTypeInfo, ExportAddInfoJobDeclarationValidation.CodeIsInvalid);

			Declaration.US_LicenseType = "BA";
			AssertHasMessageErrorContaining(Declaration.US_LicenseTypeInfo, ExportAddInfoJobDeclarationValidation.CodeIsInvalid);

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ICodeDescription pair in list)
			{
				Declaration.US_LicenseType = pair.Code;
				AssertNoMessageErrorContaining(Declaration.US_LicenseTypeInfo, ExportAddInfoJobDeclarationValidation.CodeIsInvalid);
			}

			Declaration.US_LicenseType = " ";
			AssertNoMessageErrorContaining(Declaration.US_LicenseTypeInfo, ExportAddInfoJobDeclarationValidation.CodeIsInvalid);
		}

		public void TestCheckUS_LicenseTypeForCuba()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Declaration.Factory, new ZString[] { USAESLicenseCode.Codes.C46 });

			Declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Cuba;
			Declaration.US_LicenseType = USAESLicenseCode.Codes.C46;

			AssertHasMessageErrorContaining(Declaration.US_LicenseTypeInfo, LicenseValidationHelper.LicenseTypeForCuba);
		}

		public void TestCheckUS_SchDPortOfExport()
		{
			Declaration.US_SchDExport = "";
			AssertHasMessageErrors(Declaration.US_SchDExportInfo);

			var cusCodeList = Factory.NewWithValidTestData(typeof(ZZRefCusCodeListCombined)) as ZZRefCusCodeListCombined;
			cusCodeList.ZZD_Code = "2809";
			cusCodeList.ZZD_StartDate = ZDateTime.BrettsBirthday;
			cusCodeList.ZZD_EndDate = ZDateTime.MaxSmallDateTimeValue;
			cusCodeList.ZZD_IsSea = true;
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.UnitedStates;
			cusCodeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;

			var refCusCodeListAttribute = Factory.NewWithValidTestData(typeof(ZZRefCusCodeListAttributeCombined)) as ZZRefCusCodeListAttributeCombined;
			refCusCodeListAttribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.ROLE;
			refCusCodeListAttribute.ZZE_Value = "EXP";
			refCusCodeListAttribute.ZZE_ZZD_CodeList = cusCodeList.PK;

			Factory.Save();

			Declaration.US_SchDExport = "2809";
			AssertNoMessageErrors(Declaration.US_SchDExportInfo);

			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Declaration.AddInfoValidation.ValidateUS_SchDExport();
			AssertNoMessageErrors(Declaration.US_SchDExportInfo);

			AssertUS_SchDExportHasMessageError(Enterprise.Core.Constants.TransportModes.Air);
			AssertUS_SchDExportHasMessageError(Enterprise.Core.Constants.TransportModes.FixedTransportInstallations);
			AssertUS_SchDExportHasMessageError(Enterprise.Core.Constants.TransportModes.Rail);
			AssertUS_SchDExportHasMessageError(Enterprise.Core.Constants.TransportModes.Road);
			AssertNoMessageErrors(Enterprise.Core.Constants.TransportModes.Auto);
			AssertNoMessageErrors(Enterprise.Core.Constants.TransportModes.Mail);
		}

		public void TestCheckUS_SchDExportForSelectSpecificLoadingMsg()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2970", "2970 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			var officeCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2973", "2973 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			Factory.Save();
			CreateTestLocoMapping();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoMessageError(Declaration.US_SchDExportInfo, AddInfoJobDeclarationValidation.ExportMultipleMatches);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.US_SchDExport = "";
			Declaration.US_RL_NKPortOfExport = "USLAX";
			AssertHasMessageError(Declaration.US_SchDExportInfo, AddInfoJobDeclarationValidation.ExportMultipleMatches);
		}

		public void TestCheckUS_DateOfExport()
		{
			Declaration.US_DateOfExport = ZDateTime.Empty;
			string messageError = MandatoryValidation.YouHaveNotEntered + " a Date Of Export.";
			AssertHasMessageError(Declaration.US_DateOfExportInfo, messageError);

			Declaration.US_DateOfExport = new ZDateTime(2006, 2, 3);
			AssertNoMessageError(Declaration.US_DateOfExportInfo, messageError);

			Declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._2Predeparture;
			Declaration.US_DateOfExport = ZDateTime.Today.AddDays(-1);
			AssertHasWarning(Declaration.US_DateOfExportInfo, ExportAddInfoJobDeclarationValidation.LateDepartureFiling);

			Declaration.US_DateOfExport = ZDateTime.Today.AddDays(1);
			AssertNoWarning(Declaration.US_DateOfExportInfo, ExportAddInfoJobDeclarationValidation.LateDepartureFiling);

			Declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._2Predeparture;
			Declaration.US_DateOfExport = ZDateTime.Today.AddDays(-1);
			AssertHasWarning(Declaration.US_DateOfExportInfo, ExportAddInfoJobDeclarationValidation.LateDepartureFiling);

			Declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._4Postdeparture;
			AssertNoWarning(Declaration.US_DateOfExportInfo, ExportAddInfoJobDeclarationValidation.LateDepartureFiling);
			AssertNoMessageError(Declaration.US_DateOfExportInfo, string.Format(ExportAddInfoJobDeclarationValidation.ExportDateTooFar, ZDateTime.Today.AddDays(120).ToShortDateString()));

			Declaration.US_DateOfExport = ZDateTime.Today.AddDays(121);
			AssertHasMessageError(Declaration.US_DateOfExportInfo, string.Format(ExportAddInfoJobDeclarationValidation.ExportDateTooFar, ZDateTime.Today.AddDays(120).ToShortDateString()));
		}

		public void TestCheckUS_TransportReferenceForbidden()
		{
			var list = new TransportTypeList();
			foreach (var mode in new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Air, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Truck })
			{
				Declaration.JE_TransportMode = mode;
				Declaration.US_TransportReference = "A";
				AssertNoMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefMustBeBlank);
				list.RemoveCode(mode);
			}

			foreach (ICodeDescription pair in list)
			{
				Declaration.JE_TransportMode = pair.Code;
				Declaration.US_TransportReference = "A";
				AssertHasMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefMustBeBlank);
			}
		}

		public void TestCheckUS_TransportReferenceNotRequired()
		{
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.US_TransportReference = ZString.Empty;
			AssertHasMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefRequired);

			var list = new TransportTypeList();
			list.RemoveCode(TransportTypeList.Codes.Sea);
			foreach (ICodeDescription pair in list)
			{
				Declaration.JE_TransportMode = pair.Code;
				Declaration.US_TransportReference = ZString.Empty;
				AssertNoMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefRequired);
			}
		}

		public void TestCheckUS_TransportReferenceForAir()
		{
			var checkDigitmessageError = "Invalid check digit. The last digit should be ";

			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			Declaration.US_TransportReference = ZString.Empty;
			AssertNoMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefMustHaveThisFormat);

			Declaration.US_TransportReference = "AMF123789432";
			AssertHasMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefMustHaveThisFormat);

			Declaration.US_TransportReference = "123-237894321";
			AssertHasMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefMustHaveThisFormat);

			Declaration.US_TransportReference = "123-23789432";
			AssertNoMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefMustHaveThisFormat);
			AssertHasWarningContaining(Declaration.US_TransportReferenceInfo, checkDigitmessageError);

			Declaration.US_TransportReference = "123-23789430";
			AssertNoMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefMustHaveThisFormat);
			AssertNoWarningContaining(Declaration.US_TransportReferenceInfo, checkDigitmessageError);
		}

		public void TestNoExceptionThrownWithInvalidDate()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			Declaration.US_TransportReference = ZString.Empty;
			Declaration.US_DateOfExport = ZDate.Invalid;

			AssertNoExceptionThrown(() => Declaration.AddInfoValidation.ValidateUS_LicenseType());
		}

		public void TestCheckUS_TransportReferenceForLeadingOrEmbeddedSpaces()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			Declaration.US_TransportReference = ZString.Empty;
			AssertNoMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefCannotContainSpaces);

			Declaration.US_TransportReference = " APLUMST121916";
			AssertHasMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefCannotContainSpaces);

			Declaration.US_TransportReference = "APLU MST121916";
			AssertHasMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefCannotContainSpaces);

			Declaration.US_TransportReference = "APLUMST121916 ";
			AssertNoMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefCannotContainSpaces);

			Declaration.US_TransportReference = "APLUMST121916";
			AssertNoMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefCannotContainSpaces);
		}

		public void TestCheckUS_TransportReference()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.US_TransportReference = "AS";
			AssertNoMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefRequired);

			Declaration.US_TransportReference = "";
			AssertHasMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefRequired);

			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			Declaration.US_TransportReference = "AS";
			AssertNoMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefRequired);

			Declaration.JE_TransportMode = Declaration.TransportModeRoadCodeForTesting;
			Declaration.US_TransportReference = "";
			AssertNoMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefRequired);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.US_TransportReference = "081 003948548";
			AssertHasMessageError(Declaration.US_TransportReferenceInfo, ExportAddInfoJobDeclarationValidation.TransportRefCannotContainSpaces);

			Declaration.US_TransportReference = "081-003948548";
			AssertNoNotifications(Declaration.US_TransportReferenceInfo);
		}

		public void TestCheckUS_SchDArrival()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "01", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.AES);
			Factory.Save();

			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.US_SchDArrival = "";
			AssertHasMessageError(Declaration.US_SchDArrivalInfo, ExportAddInfoJobDeclarationValidation.EmptyPortOfDischardSea);

			Declaration.US_SchDArrival = "77877";
			AssertNoMessageError(Declaration.US_SchDArrivalInfo, ExportAddInfoJobDeclarationValidation.EmptyPortOfDischardSea);

			Declaration.US_SchDArrival = "~";
			AssertHasMessageError("Port Of Discharge", Declaration.US_SchDArrivalInfo, ListValidation.InvalidCodeMessageError);

			Declaration.US_SchDArrival = "01";
			AssertNoMessageError("Port Of Discharge", Declaration.US_SchDArrivalInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(Declaration.US_SchDArrivalInfo, ExportAddInfoJobDeclarationValidation.PortOfDischardIsOnlyAllowedForSeaOrForAirBetweenUSAndPuertoRico);
			AssertNoMessageError(Declaration.US_SchDArrivalInfo, ExportAddInfoJobDeclarationValidation.EmptyPortOfDischardAirForBetweenUSAndPuertoRico);

			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Declaration.US_SchDArrival = "01";
			AssertHasMessageError(Declaration.US_SchDArrivalInfo, ExportAddInfoJobDeclarationValidation.PortOfDischardIsOnlyAllowedForSeaOrForAirBetweenUSAndPuertoRico);
			AssertNoMessageError(Declaration.US_SchDArrivalInfo, ExportAddInfoJobDeclarationValidation.EmptyPortOfDischardAirForBetweenUSAndPuertoRico);

			Declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageError(Declaration.US_SchDArrivalInfo, ExportAddInfoJobDeclarationValidation.PortOfDischardIsOnlyAllowedForSeaOrForAirBetweenUSAndPuertoRico);
			AssertNoMessageError(Declaration.US_SchDArrivalInfo, ExportAddInfoJobDeclarationValidation.EmptyPortOfDischardAirForBetweenUSAndPuertoRico);

			Declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.PuertoRico;
			AssertNoMessageError(Declaration.US_SchDArrivalInfo, ExportAddInfoJobDeclarationValidation.PortOfDischardIsOnlyAllowedForSeaOrForAirBetweenUSAndPuertoRico);
			AssertNoMessageError(Declaration.US_SchDArrivalInfo, ExportAddInfoJobDeclarationValidation.EmptyPortOfDischardAirForBetweenUSAndPuertoRico);

			Declaration.US_SchDArrival = ZString.Empty;
			AssertNoMessageError(Declaration.US_SchDArrivalInfo, ExportAddInfoJobDeclarationValidation.PortOfDischardIsOnlyAllowedForSeaOrForAirBetweenUSAndPuertoRico);
			AssertHasMessageError(Declaration.US_SchDArrivalInfo, ExportAddInfoJobDeclarationValidation.EmptyPortOfDischardAirForBetweenUSAndPuertoRico);

			Declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageError(Declaration.US_SchDArrivalInfo, ExportAddInfoJobDeclarationValidation.PortOfDischardIsOnlyAllowedForSeaOrForAirBetweenUSAndPuertoRico);
			AssertHasMessageError(Declaration.US_SchDArrivalInfo, ExportAddInfoJobDeclarationValidation.EmptyPortOfDischardAirForBetweenUSAndPuertoRico);
		}

		public void TestDestinationStateIsNotValidatedForExport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.US_DestinationState = "XX";
			AssertNoNotifications(Declaration.US_DestinationStateInfo);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_DestinationState = "YY";
			AssertHasNotifications(Declaration.US_DestinationStateInfo);
		}

		public void TestCheckUS_FirstPortOfCallCity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			declaration.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_FirstPortOfCallCity = "";
			AssertHasMessageError(declaration.US_FirstPortOfCallCityInfo, ExportAddInfoJobDeclarationValidation.FirstPortOfCallCityRequired);

			declaration.US_FirstPortOfCallCity = "OSAKA";
			AssertNoMessageError(declaration.US_FirstPortOfCallCityInfo, ExportAddInfoJobDeclarationValidation.FirstPortOfCallCityRequired);

			declaration.US_FirstPortOfCallCity = ZString.Empty;
			AssertHasMessageError(declaration.US_FirstPortOfCallCityInfo, ExportAddInfoJobDeclarationValidation.FirstPortOfCallCityRequired);
		}

		public void TestCheckUS_RN_NKFirstPortOfCallCountry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			declaration.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_RN_NKFirstPortOfCallCountry = "";
			AssertHasMessageError(declaration.US_RN_NKFirstPortOfCallCountryInfo, ExportAddInfoJobDeclarationValidation.FirstPortOfCallCountryRequired);

			declaration.US_RN_NKFirstPortOfCallCountry = "JP";
			AssertNoMessageError(declaration.US_RN_NKFirstPortOfCallCountryInfo, ExportAddInfoJobDeclarationValidation.FirstPortOfCallCountryRequired);

			declaration.US_RN_NKFirstPortOfCallCountry = ZString.Empty;
			AssertHasMessageError(declaration.US_RN_NKFirstPortOfCallCountryInfo, ExportAddInfoJobDeclarationValidation.FirstPortOfCallCountryRequired);
		}

		public void TestCheckUS_SoldEnRouteIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoMessageErrorContaining(declaration.US_SoldEnRouteIndicatorInfo, ListValidation.InvalidCodeMessageError);

			declaration.US_SoldEnRouteIndicator = "X";
			AssertHasMessageErrorContaining(declaration.US_SoldEnRouteIndicatorInfo, ListValidation.InvalidCodeMessageError);

			declaration.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.Yes;
			AssertNoMessageErrorContaining(declaration.US_SoldEnRouteIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_RN_NKCountryOfDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_DateOfExport = new ZDateTime(2019, 9, 01);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.US_SchDLoading = "4909";
			declaration.US_RN_NKCountryOfDestination = "-";
			AssertHasMessageError(declaration.US_RN_NKCountryOfDestinationInfo, ListValidation.InvalidCodeMessageError);

			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageError(declaration.US_RN_NKCountryOfDestinationInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(declaration.US_RN_NKCountryOfDestinationInfo, ExportAddInfoJobDeclarationValidation.InvalidMOTForPRShipmentToUS);

			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.VirginIslands;
			AssertNoMessageError(declaration.US_RN_NKCountryOfDestinationInfo, ExportAddInfoJobDeclarationValidation.InvalidMOTForPRShipmentToUS);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageError(declaration.US_RN_NKCountryOfDestinationInfo, ExportAddInfoJobDeclarationValidation.InvalidMOTForPRShipmentToUS);

			declaration.US_LicenseType = USAESLicenseCode.Codes.SCA;
			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Mexico;
			AssertHasMessageError(declaration.US_RN_NKCountryOfDestinationInfo, ExportAddInfoJobDeclarationValidation.CountryForSCA);

			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Canada;
			AssertNoMessageError(declaration.US_RN_NKCountryOfDestinationInfo, ExportAddInfoJobDeclarationValidation.CountryForSCA);

			declaration.US_LicenseType = USAESLicenseCode.Codes.C62;
			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Mexico;
			AssertHasMessageError(declaration.US_RN_NKCountryOfDestinationInfo, ExportAddInfoJobDeclarationValidation.CountryForC62);

			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Cuba;
			AssertNoMessageError(declaration.US_RN_NKCountryOfDestinationInfo, ExportAddInfoJobDeclarationValidation.CountryForC62);

			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Myanmar;
			AssertHasWarning(declaration.US_RN_NKCountryOfDestinationInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);

			declaration.US_DateOfExport = new ZDateTime(2019, 9, 21);
			declaration.AddInfoValidation.ValidateUS_RN_NKCountryOfDestination();
			AssertNoWarning(declaration.US_RN_NKCountryOfDestinationInfo, AESCountryCodeValidator.CountryCodeBUWillBeSentInsteadOfMM);

			declaration.US_RN_NKCountryOfDestination = USCCountry.Burma;
			AssertHasWarning(declaration.US_RN_NKCountryOfDestinationInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);

			declaration.US_DateOfExport = new ZDateTime(2019, 9, 01);
			declaration.AddInfoValidation.ValidateUS_RN_NKCountryOfDestination();
			AssertNoWarning(declaration.US_RN_NKCountryOfDestinationInfo, AESCountryCodeValidator.CountryCodeBUNotAcceptable);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;

			var usTerritory = new string[4] { Core.Constants.CountryCodes.Guam, Core.Constants.CountryCodes.UnitedStatesMinorIslands,
			Core.Constants.CountryCodes.AmericanSamoa, Core.Constants.CountryCodes.NorthernMarianaIslands };

			foreach (string territory in usTerritory)
			{
				declaration2.US_RN_NKCountryOfDestination = territory;
				AssertHasWarning(declaration2.US_RN_NKCountryOfDestinationInfo, ExportAddInfoJobDeclarationValidation.DestinationIsUSTerritory);
			}
		}

		public void TestCheckUS_StateOfOrigin()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_SchDLoading = "4909";
			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedStates;

			declaration.US_StateOfOrigin = "~";
			AssertHasMessageError(declaration.US_StateOfOriginInfo, ValidationConstants.Declaration.DestinationStateShouldBeInList);

			declaration.US_StateOfOrigin = USStateList.Codes.Alabama;
			AssertNoMessageError(declaration.US_StateOfOriginInfo, ValidationConstants.Declaration.DestinationStateShouldBeInList);
			AssertHasMessageError(declaration.US_StateOfOriginInfo, ExportAddInfoJobDeclarationValidation.StateOfOriginMustBePuertoRicoOrVirginIslands);

			declaration.US_StateOfOrigin = Core.Constants.CountryCodes.VirginIslands;
			AssertNoMessageError(declaration.US_StateOfOriginInfo, ExportAddInfoJobDeclarationValidation.StateOfOriginMustBePuertoRicoOrVirginIslands);

			declaration.US_StateOfOrigin = Core.Constants.CountryCodes.PuertoRico;
			AssertNoMessageError(declaration.US_StateOfOriginInfo, ExportAddInfoJobDeclarationValidation.StateOfOriginMustBePuertoRicoOrVirginIslands);
			AssertNoMessageError(declaration.US_StateOfOriginInfo, ValidationConstants.Declaration.DestinationStateShouldBeInList);
		}

		public void TestCheckUS_ForeignTradeZone()
		{
			Declaration.US_ForeignTradeZone = "AAA123";
			AssertHasMessageError(Declaration.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);

			Declaration.US_ForeignTradeZone = "AAA1234";
			AssertHasMessageError(Declaration.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);

			Declaration.US_ForeignTradeZone = "1234HH";
			AssertHasMessageError(Declaration.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);

			Declaration.US_ForeignTradeZone = "12341AA";
			AssertNoMessageError(Declaration.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);
		}

		public void TestCheckUS_SchDExport()
		{
			Declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			Declaration.AddInfoValidation.ValidateUS_SchDExport();
			AssertNoMessageErrorContaining(Declaration.US_SchDExportInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(Declaration.US_SchDExportInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.US_SchDExport = "8000";
			AssertNoMessageErrorContaining(Declaration.US_SchDExportInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Declaration.US_SchDExportInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.AddInfoValidation.ValidateUS_SchDExport();
			AssertHasMessageErrorContaining(Declaration.US_SchDExportInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestTruckExportsValidation()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			Declaration.JE_ContainerMode = ContainerModeList.Codes.Bulk;

			Declaration.US_UI_NKCarrierSCAC = "1234";
			Declaration.AddInfoValidation.ValidateUS_UI_NKCarrierSCAC();
			AssertNoMessageError("4 character code for truck, rail and vessel shipments and an abbreviation (2 or 3 characters) for air",
					Declaration.US_UI_NKCarrierSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormatForExport);

			Declaration.US_UI_NKCarrierSCAC = "~~";
			Declaration.AddInfoValidation.ValidateUS_UI_NKCarrierSCAC();
			AssertHasMessageError("4 character code for truck, rail and vessel shipments and an abbreviation (2 or 3 characters) for air",
					Declaration.US_UI_NKCarrierSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormatForExport);

			Declaration.US_UI_NKCarrierSCAC = "";
			Declaration.AddInfoValidation.ValidateUS_UI_NKCarrierSCAC();
			AssertHasMessageError("You have not entered a Carrier.", Declaration.US_UI_NKCarrierSCACInfo, "You have not entered a Carrier.");
		}

		public void TestCheckUS_CarrierName()
		{
			Declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			Declaration.US_UI_NKCarrierSCAC = "UNKN";
			Declaration.US_CarrierName = ZString.Empty;
			Declaration.AddInfoValidation.ValidateUS_CarrierName();
			AssertHasMessageErrorContaining(Declaration.US_CarrierNameInfo, ValidationConstants.Declaration.CarrierNameRequired);

			Declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			Declaration.US_UI_NKCarrierSCAC = "UNKN";
			Declaration.US_CarrierName = ZString.Empty;
			Declaration.AddInfoValidation.ValidateUS_CarrierName();
			AssertHasMessageErrorContaining(Declaration.US_CarrierNameInfo, ValidationConstants.Declaration.CarrierNameRequired);

			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.US_UI_NKCarrierSCAC = "UNKN";
			Declaration.US_CarrierName = ZString.Empty;
			Declaration.AddInfoValidation.ValidateUS_CarrierName();
			AssertNoMessageErrorContaining(Declaration.US_CarrierNameInfo, ValidationConstants.Declaration.CarrierNameRequired);

			Declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			Declaration.US_UI_NKCarrierSCAC = "!!";
			Declaration.US_CarrierName = ZString.Empty;
			Declaration.AddInfoValidation.ValidateUS_CarrierName();
			AssertNoMessageErrorContaining(Declaration.US_CarrierNameInfo, ValidationConstants.Declaration.CarrierNameRequired);

			Declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			Declaration.US_UI_NKCarrierSCAC = "UNKN";
			Declaration.US_CarrierName = "123";
			Declaration.AddInfoValidation.ValidateUS_CarrierName();
			AssertNoMessageErrorContaining(Declaration.US_CarrierNameInfo, ValidationConstants.Declaration.CarrierNameRequired);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		}

		void AssertNoMessageErrors(string transportMode)
		{
			Declaration.JE_TransportMode = transportMode;
			Declaration.AddInfoValidation.ValidateUS_SchDExport();
			AssertNoMessageErrors(Declaration.US_SchDExportInfo);
		}

		void AssertUS_SchDExportHasMessageError(string transportMode)
		{
			Declaration.JE_TransportMode = transportMode;
			Declaration.AddInfoValidation.ValidateUS_SchDExport();
			AssertHasMessageError("US_SchDExportInfo", Declaration.US_SchDExportInfo, ValidationConstants.Declaration.InvalidPortForTransportMode(transportMode));
		}
	}
}
