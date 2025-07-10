using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.LicenseManager;
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
	sealed class ExportAddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_UltimateConsigneeType()
		{
			invoice.US_UltimateConsigneeType = UltimateConsigneeTypeList.Codes.DirectConsumer;
			invoice.US_UltimateConsigneeType = ZString.Empty;
			AssertHasMessageErrorContaining(invoice.US_UltimateConsigneeTypeInfo, MandatoryValidation.YouHaveNotEntered);
			invoice.US_UltimateConsigneeType = UltimateConsigneeTypeList.Codes.DirectConsumer;
			AssertNoMessageErrorContaining(invoice.US_UltimateConsigneeTypeInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.US_UltimateConsigneeType = "X";
			AssertHasMessageErrorContaining(invoice.US_UltimateConsigneeTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_JurisdictionNumber()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESJurisdictionNumber, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedStates;
				var invoice01 = declaration.Invoices.AddNew();

				invoice01.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.AircraftAndAssociatedEquipment;
				Assert("US_JurisdictionNumber is read-only when US_DDTCUSMLCategoryCode is not equal to 21", invoice01.US_JurisdictionNumberInfo.ReadOnly);

				invoice01.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.MiscellaneousArticles;

				invoice01.US_JurisdictionNumber = "";
				AssertHasMessageError(invoice01.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				invoice01.US_JurisdictionNumber = "1CJ1234567";
				AssertHasMessageError(invoice01.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				invoice01.US_JurisdictionNumber = "CJ12345678";
				AssertHasMessageError(invoice01.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				invoice01.US_JurisdictionNumber = "CJ 1234567";
				AssertHasMessageError(invoice01.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				invoice01.US_JurisdictionNumber = "CJ1234567";
				AssertNoMessageError(invoice01.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				invoice01.US_JurisdictionNumber = "CJ1234-67";
				AssertHasMessageError(invoice01.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				invoice01.US_JurisdictionNumber = "CJ 1234-67";
				AssertNoMessageError(invoice01.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);
			}
		}

		public void TestCheckUS_AESOriginIndicator()
		{
			invoice.US_AESOriginIndicator = "";
			AssertNoMessageErrorContaining(invoice.US_AESOriginIndicatorInfo, ListValidation.InvalidCodeMessageError);

			invoice.US_AESOriginIndicator = "B";
			AssertHasMessageErrorContaining(invoice.US_AESOriginIndicatorInfo, ListValidation.InvalidCodeMessageError);

			AESOriginIndicatorList list = new AESOriginIndicatorList();
			foreach (CodeDescriptionPair pair in list)
			{
				invoice.US_AESOriginIndicator = pair.Code;
				AssertNoMessageErrorContaining(invoice.US_AESOriginIndicatorInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_ExportCode()
		{
			string messageError = "The code you have selected is not in the list.";
			invoice.US_ExportCode = "";
			AssertNoMessageError(invoice.US_ExportCodeInfo, messageError);

			invoice.US_ExportCode = "BA";
			AssertHasMessageError(invoice.US_ExportCodeInfo, messageError);

			var list = new ExportInformationCodeList();
			foreach (CodeDescriptionPair pair in list)
			{
				invoice.US_ExportCode = pair.Code;
				AssertNoMessageError(invoice.US_ExportCodeInfo, messageError);
			}

			invoice.US_ExportCode = " ";
			AssertNoMessageError(invoice.US_ExportCodeInfo, messageError);

			invoice.US_LicenseType = USAESLicenseCode.Codes.C30;
			invoice.US_ExportCode = ExportInformationCodeList.Codes.MS;
			messageError = LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C30, ExportInformationCodeList.Codes.MS);
			AssertHasMessageError(invoice.US_ExportCodeInfo, messageError);

			invoice.US_ExportCode = ExportInformationCodeList.Codes.OS;
			AssertNoMessageError(invoice.US_ExportCodeInfo, messageError);
		}

		public void TestCheckUS_ExportCodeAdditional()
		{
			invoice.US_LicenseType = USAESLicenseCode.Codes.C30;
			invoice.US_ExportCode = ExportInformationCodeList.Codes.CH;
			AssertNoMessageErrors(invoice.US_ExportCodeInfo);

			invoice.US_LicenseType = USAESLicenseCode.Codes.OPA;
			invoice.US_ExportCode = ExportInformationCodeList.Codes.UG;
			AssertHasMessageErrors(invoice.US_ExportCodeInfo);

			invoice.US_LicenseType = USAESLicenseCode.Codes.VDS;
			invoice.US_ExportCode = ExportInformationCodeList.Codes.CH;
			AssertHasMessageErrors(invoice.US_ExportCodeInfo);

			invoice.US_LicenseType = USAESLicenseCode.Codes.VDO;
			invoice.US_ExportCode = ExportInformationCodeList.Codes.FI;
			AssertHasMessageErrors(invoice.US_ExportCodeInfo);
		}

		public void TestCheckUS_LicenseNo()
		{
			var factory = declaration.Factory;
			var exportDate = invoice.ExportDateForLicenseType;

			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(factory, new ZString[] { USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C60 });

			declaration.US_LicenseType = "";
			declaration.US_LicenseNo = "";
			invoice.US_LicenseType = USAESLicenseCode.Codes.C30;
			string messageError = LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C30, "", factory, exportDate);

			invoice.US_LicenseNo = "";
			AssertHasMessageError(invoice.US_LicenseNoInfo, messageError);
			invoice.US_LicenseNo = "LIC2343";
			AssertNoMessageError(invoice.US_LicenseNoInfo, messageError);

			invoice.US_LicenseType = USAESLicenseCode.Codes.C60;
			invoice.US_LicenseNo = "ABC";
			AssertHasMessageError(invoice.US_LicenseNoInfo, LicenseNumberValidation.C60LicenseInvalid);
			invoice.US_LicenseNo = LicenseExemptionTypeList.Codes.DY6;
			AssertNoMessageError(invoice.US_LicenseNoInfo, LicenseNumberValidation.C60LicenseInvalid);
		}

		public void TestCheckUS_LicenseNoWithT10LicenseType()
		{
			invoice.US_LicenseType = USAESLicenseCode.Codes.T10;
			invoice.US_LicenseNo = "12-4678";
			AssertHasMessageError(invoice.US_LicenseNoInfo, LicenseNumberValidation.T10LicenseInvalid);
			invoice.US_LicenseNo = "AA-2014-1";
			AssertHasMessageError(invoice.US_LicenseNoInfo, LicenseNumberValidation.T10LicenseInvalid);
			invoice.US_LicenseNo = "AA20141";
			AssertNoMessageError(invoice.US_LicenseNoInfo, LicenseNumberValidation.T10LicenseInvalid);
		}

		public void TestValidationE01Licence()
		{
			var factory = declaration.Factory;
			var exportDate = invoice.ExportDateForLicenseType;

			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(factory, new ZString[] { USAESLicenseCode.Codes.E01 });

			declaration.US_LicenseNo = "12345";
			declaration.US_LicenseType = USAESLicenseCode.Codes.E01;
			string messageError = LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.E01, "", factory, exportDate);
			declaration.US_LicenseNo = "";
			declaration.AddInfoValidation.ValidateUS_LicenseNo();

			AssertHasMessageError(invoice.US_LicenseNoInfo, messageError);
			declaration.US_LicenseNo = LicenseExemptionTypeList.Codes.AEA;
			AssertNoMessageError(invoice.US_LicenseNoInfo, messageError);

			messageError = LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.E01, "TEST", factory, exportDate);

			declaration.US_ECCN = "TEST";
			AssertHasMessageError(declaration.US_ECCNInfo, messageError);
			declaration.US_ECCN = "";
			AssertNoMessageError(declaration.US_ECCNInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.MS;
			messageError = LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.E01, ExportInformationCodeList.Codes.MS);
			AssertHasMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.OS;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;

			messageError = LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.E01, declaration.JE_TransportMode, factory, exportDate);
			declaration.AddInfoValidation.ValidateUS_LicenseType();
			AssertHasMessageError(declaration.US_LicenseTypeInfo, messageError);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.AddInfoValidation.ValidateUS_LicenseType();
			AssertNoMessageError(declaration.US_LicenseTypeInfo, messageError);

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.AddInfoValidation.ValidateUS_LicenseType();
			AssertHasMessageError(declaration.US_LicenseTypeInfo, messageError);

			declaration.JE_TransportMode = TransportTypeList.Codes.BorderWaterBorne;
			declaration.AddInfoValidation.ValidateUS_LicenseType();
			AssertHasMessageError(declaration.US_LicenseTypeInfo, messageError);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.AddInfoValidation.ValidateUS_LicenseType();
			AssertNoMessageError(declaration.US_LicenseTypeInfo, messageError);
		}

		public void TestValidationC57Licence()
		{
			var factory = declaration.Factory;
			var exportDate = invoice.ExportDateForLicenseType;

			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(factory, new ZString[] { USAESLicenseCode.Codes.C57 });

			declaration.US_LicenseNo = "12345";
			declaration.US_LicenseType = USAESLicenseCode.Codes.C57;
			string messageError = LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C57, "", factory, exportDate);
			declaration.US_LicenseNo = "";
			declaration.AddInfoValidation.ValidateUS_LicenseNo();

			AssertHasMessageError(invoice.US_LicenseNoInfo, messageError);
			declaration.US_LicenseNo = LicenseExemptionTypeList.Codes.VEU;
			AssertNoMessageError(invoice.US_LicenseNoInfo, messageError);

			messageError = LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C57, "", factory, exportDate);

			declaration.US_ECCN = "1A111";
			AssertNoMessageError(declaration.US_ECCNInfo, messageError);
			declaration.US_ECCN = "";
			AssertHasMessageError(declaration.US_ECCNInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.MS;
			messageError = LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C57, ExportInformationCodeList.Codes.MS);
			AssertHasMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.OS;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.TL;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.OI;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.IW;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;

			messageError = LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.C57, declaration.JE_TransportMode, factory, exportDate);
			declaration.AddInfoValidation.ValidateUS_LicenseType();
			AssertHasMessageError(declaration.US_LicenseTypeInfo, messageError);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.AddInfoValidation.ValidateUS_LicenseType();
			AssertNoMessageError(declaration.US_LicenseTypeInfo, messageError);
		}

		public void TestValidationC58Licence()
		{
			var factory = declaration.Factory;
			var exportDate = invoice.ExportDateForLicenseType;

			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(factory, new ZString[] { USAESLicenseCode.Codes.C58 });

			declaration.US_LicenseNo = "12345";
			declaration.US_LicenseType = USAESLicenseCode.Codes.C58;
			string messageError = LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C58, "", factory, exportDate);
			declaration.US_LicenseNo = "";
			declaration.AddInfoValidation.ValidateUS_LicenseNo();

			AssertHasMessageError(invoice.US_LicenseNoInfo, messageError);
			declaration.US_LicenseNo = LicenseExemptionTypeList.Codes.CCD;
			AssertNoMessageError(invoice.US_LicenseNoInfo, messageError);

			messageError = LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C58, "TEST", factory, exportDate);

			declaration.US_ECCN = "TEST";
			AssertHasMessageError(declaration.US_ECCNInfo, messageError);

			declaration.US_ECCN = "4A994";
			AssertNoMessageError(declaration.US_ECCNInfo, messageError);

			declaration.US_ECCN = "4D994";
			AssertNoMessageError(declaration.US_ECCNInfo, messageError);

			declaration.US_ECCN = "5A991";
			AssertNoMessageError(declaration.US_ECCNInfo, messageError);

			declaration.US_ECCN = "5D991";
			AssertNoMessageError(declaration.US_ECCNInfo, messageError);

			declaration.US_ECCN = "5D992";
			AssertNoMessageError(declaration.US_ECCNInfo, messageError);

			declaration.US_ECCN = "5A992";
			AssertNoMessageError(declaration.US_ECCNInfo, messageError);

			declaration.US_ECCN = "EAR99";
			AssertNoMessageError(declaration.US_ECCNInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.MS;
			messageError = LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C58, ExportInformationCodeList.Codes.MS);
			AssertHasMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.OS;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.CH;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.OI;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.CI;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;

			messageError = LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.C58, declaration.JE_TransportMode, factory, exportDate);
			declaration.AddInfoValidation.ValidateUS_LicenseType();
			AssertHasMessageError(declaration.US_LicenseTypeInfo, messageError);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.AddInfoValidation.ValidateUS_LicenseType();
			AssertNoMessageError(declaration.US_LicenseTypeInfo, messageError);
		}

		public void TestValidationC59Licence()
		{
			var factory = declaration.Factory;
			var exportDate = invoice.ExportDateForLicenseType;

			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(factory, new ZString[] { USAESLicenseCode.Codes.C59 });

			declaration.US_LicenseNo = "12345";
			declaration.US_LicenseType = USAESLicenseCode.Codes.C59;
			string messageError = LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C59, "", factory, exportDate);
			declaration.US_LicenseNo = "";
			declaration.AddInfoValidation.ValidateUS_LicenseNo();

			AssertHasMessageError(invoice.US_LicenseNoInfo, messageError);
			declaration.US_LicenseNo = LicenseExemptionTypeList.Codes.CCD;
			AssertNoMessageError(invoice.US_LicenseNoInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.MS;
			messageError = LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C59, ExportInformationCodeList.Codes.MS);
			AssertHasMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.OS;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.CH;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.OI;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.CI;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);
		}

		public void TestValidationC60Licence()
		{
			var factory = declaration.Factory;
			var exportDate = invoice.ExportDateForLicenseType;

			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(factory, new ZString[] { USAESLicenseCode.Codes.C60 });

			declaration.US_LicenseNo = "12345";
			declaration.US_LicenseType = USAESLicenseCode.Codes.C60;
			string messageError = LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C60, "", factory, exportDate);
			declaration.US_LicenseNo = "";
			declaration.AddInfoValidation.ValidateUS_LicenseNo();

			AssertHasMessageError(invoice.US_LicenseNoInfo, messageError);
			declaration.US_LicenseNo = LicenseExemptionTypeList.Codes.CCD;
			AssertNoMessageError(invoice.US_LicenseNoInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.MS;
			messageError = LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C60, ExportInformationCodeList.Codes.MS);
			AssertHasMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.OS;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.CH;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.OI;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.CI;
			AssertNoMessageError(declaration.US_ExportCodeInfo, messageError);
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

			var invoice = declaration.Invoices.AddNew();

			foreach (var licenseType in licenseTypeList)
			{
				invoice.US_LicenseType = licenseType;
				invoice.US_ECCN = "0A606";
				AssertHasMessageError(invoice.US_ECCNInfo, ECCNValidation.ECCN600SeriesDotYNotEligible);
				invoice.US_UltimateDestinationCountry = ZString.Empty;
				invoice.US_ECCN = "3A611";
				AssertHasMessageError(invoice.US_ECCNInfo, ECCNValidation.ECCN600SeriesDotYNotEligible);
				invoice.US_UltimateDestinationCountry = Core.Constants.CountryCodes.Australia;
				invoice.AddInfoValidation.ValidateUS_ECCN();
				AssertNoMessageError(invoice.US_ECCNInfo, ECCNValidation.ECCN600SeriesDotYNotEligible);
				invoice.US_UltimateDestinationCountry = Core.Constants.CountryCodes.UnitedKingdom;
				invoice.AddInfoValidation.ValidateUS_ECCN();
				AssertNoMessageError(invoice.US_ECCNInfo, ECCNValidation.ECCN600SeriesDotYNotEligible);
			}
		}

		public void TestCheckUS_ECCN()
		{
			var factory = declaration.Factory;
			var exportDate = invoice.ExportDateForLicenseType;

			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(factory, new ZString[] { USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C59 });

			invoice.US_LicenseType = USAESLicenseCode.Codes.C30;
			string messageError = LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C30, "", factory, exportDate);

			invoice.US_ECCN = "";
			AssertHasMessageError(invoice.US_ECCNInfo, messageError);
			invoice.US_ECCN = "C2343";
			AssertNoMessageError(invoice.US_ECCNInfo, messageError);

			invoice.US_LicenseType = USAESLicenseCode.Codes.C33;
			invoice.US_ECCN = "1C353";
			AssertHasMessageError(invoice.US_ECCNInfo, string.Format(ECCNValidation.ECCNNotAllowed, "1C353", USAESLicenseCode.Codes.C33));

			invoice.US_LicenseType = USAESLicenseCode.Codes.C32;
			invoice.US_ECCN = "1C351";
			AssertHasMessageError(invoice.US_ECCNInfo, string.Format(ECCNValidation.ECCNNotAllowed, "1C351", USAESLicenseCode.Codes.C32));

			invoice.US_ECCN = "1C356";
			AssertNoMessageErrors(invoice.US_ECCNInfo);
		}

		public void TestCheckUS_ECCNForStandaloneInvoice()
		{
			var factory = declaration.Factory;
			var exportDate = this.invoice.ExportDateForLicenseType;

			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(factory, new ZString[] { USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C33 });

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			invoice.US_LicenseType = USAESLicenseCode.Codes.C30;
			string messageError = LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C30, "", factory, exportDate);

			invoice.US_ECCN = "";
			AssertHasMessageError(invoice.US_ECCNInfo, messageError);
			invoice.US_ECCN = "C2343";
			AssertNoMessageError(invoice.US_ECCNInfo, messageError);

			invoice.US_LicenseType = USAESLicenseCode.Codes.C33;
			invoice.US_ECCN = "1C353";
			AssertHasMessageError(invoice.US_ECCNInfo, string.Format(ECCNValidation.ECCNNotAllowed, "1C353", USAESLicenseCode.Codes.C33));

			invoice.US_ECCN = "1C356";
			AssertNoMessageErrors(this.invoice.US_ECCNInfo);
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

			var requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAG,
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00
				};

			declaration.US_LicenseType = ZString.Empty;
			invoice.US_LicenseType = ZString.Empty;
			invoice.US_DDTCITARExemptionNo = ZString.Empty;
			AssertNoMessageErrors(invoice.US_DDTCITARExemptionNoInfo);

			ClassificationValidatorHelper.CheckLicensType(invoice.US_DDTCITARExemptionNoInfo, invoice.US_LicenseTypeInfo, requireDDTCDataList, System.Array.Empty<ZString>(), "123.11B",
				ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage,
				ClassificationValidator.DDTCITARExemptionNumberShouldNotBeEntered,
				Factory);

			declaration.US_LicenseType = USAESLicenseCode.Codes.SAG;
			invoice.US_DDTCITARExemptionNo = ZString.Empty;
			invoice.US_LicenseType = ZString.Empty;
			AssertHasMessageError(invoice.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);

			invoice.US_DDTCITARExemptionNo = "ZDFSDF";
			AssertNoMessageError(invoice.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);
			AssertHasMessageErrorContaining(invoice.US_DDTCITARExemptionNoInfo, ListValidation.InvalidCodeMessageError);

			invoice.US_DDTCITARExemptionNo = "123.12";
			AssertNoMessageError(invoice.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);
			AssertNoMessageErrorContaining(invoice.US_DDTCITARExemptionNoInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_EntrySubmittedDate = ZDateTime.Empty;
			invoice.US_DDTCITARExemptionNo = "123.17A";
			AssertHasMessageErrorContaining(invoice.US_DDTCITARExemptionNoInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_EntrySubmittedDate = new ZDateTime(2013, 9, 12);
			invoice.US_DDTCITARExemptionNo = "123.17A";
			AssertHasMessageErrorContaining(invoice.US_DDTCITARExemptionNoInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_EntrySubmittedDate = new ZDateTime(2013, 9, 10);
			invoice.US_DDTCITARExemptionNo = "123.17A";
			AssertNoMessageErrorContaining(invoice.US_DDTCITARExemptionNoInfo, ListValidation.InvalidCodeMessageError);
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

			declaration.US_LicenseType = ZString.Empty;
			invoice.US_LicenseType = ZString.Empty;
			invoice.US_DDTCRegistrationNo = ZString.Empty;
			AssertNoMessageErrors(invoice.US_DDTCRegistrationNoInfo);

			ClassificationValidatorHelper.CheckLicensType(invoice.US_DDTCRegistrationNoInfo, invoice.US_LicenseTypeInfo, requireDDTCDataList, allowedDDTCDataList, "REG123",
				ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage,
				ClassificationValidator.DDTCRegistrationNumberShouldNotBeEntered,
				Factory);

			declaration.US_LicenseType = USAESLicenseCode.Codes.SAG;
			invoice.US_DDTCRegistrationNo = ZString.Empty;
			invoice.US_LicenseType = ZString.Empty;
			AssertHasMessageError(invoice.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage);

			invoice.US_DDTCRegistrationNo = "REG123";
			AssertNoMessageError(invoice.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage);
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

			declaration.US_LicenseType = ZString.Empty;
			invoice.US_LicenseType = ZString.Empty;
			invoice.US_DDTCMilitaryEquipmentIndicator = ZString.Empty;
			AssertNoMessageErrors(invoice.US_DDTCMilitaryEquipmentIndicatorInfo);

			ClassificationValidatorHelper.CheckLicensType(invoice.US_DDTCMilitaryEquipmentIndicatorInfo, invoice.US_LicenseTypeInfo, requireDDTCDataList, System.Array.Empty<ZString>(), YesNoDefaultList.Codes.Yes,
				ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage,
				ClassificationValidator.DDTCMilitaryEquipmentIndicatorShouldNotBeEntered,
				Factory);

			declaration.US_LicenseType = USAESLicenseCode.Codes.SAG;
			invoice.US_DDTCMilitaryEquipmentIndicator = ZString.Empty;
			invoice.US_LicenseType = ZString.Empty;
			AssertHasMessageError(invoice.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);

			invoice.US_DDTCMilitaryEquipmentIndicator = "Z";
			AssertNoMessageError(invoice.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);
			AssertHasMessageErrorContaining(invoice.US_DDTCMilitaryEquipmentIndicatorInfo, ListValidation.InvalidCodeMessageError);

			invoice.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.No;
			AssertNoMessageError(invoice.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);
			AssertNoMessageErrorContaining(invoice.US_DDTCMilitaryEquipmentIndicatorInfo, ListValidation.InvalidCodeMessageError);
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

			declaration.US_LicenseType = ZString.Empty;
			invoice.US_LicenseType = ZString.Empty;
			invoice.US_DDTCPartyCertificationIndicator = ZString.Empty;
			AssertNoMessageErrors(invoice.US_DDTCPartyCertificationIndicatorInfo);

			ClassificationValidatorHelper.CheckLicensType(invoice.US_DDTCPartyCertificationIndicatorInfo, invoice.US_LicenseTypeInfo, requireDDTCDataList, System.Array.Empty<ZString>(), YesNoDefaultList.Codes.Yes,
				ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage,
				ClassificationValidator.DDTCPartyCertificationIndicatorShouldNotBeEntered,
				Factory);

			declaration.US_LicenseType = USAESLicenseCode.Codes.SCA;
			invoice.US_DDTCPartyCertificationIndicator = ZString.Empty;
			invoice.US_LicenseType = ZString.Empty;
			AssertHasMessageError(invoice.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);

			invoice.US_DDTCPartyCertificationIndicator = "Z";
			AssertNoMessageError(invoice.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);
			AssertHasMessageErrorContaining(invoice.US_DDTCPartyCertificationIndicatorInfo, ListValidation.InvalidCodeMessageError);

			invoice.US_DDTCPartyCertificationIndicator = YesNoDefaultList.Codes.No;
			AssertNoMessageError(invoice.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);
			AssertNoMessageErrorContaining(invoice.US_DDTCPartyCertificationIndicatorInfo, ListValidation.InvalidCodeMessageError);
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

			declaration.US_LicenseType = ZString.Empty;
			invoice.US_LicenseType = ZString.Empty;
			invoice.US_DDTCUSMLCategoryCode = ZString.Empty;
			AssertNoMessageErrors(invoice.US_DDTCUSMLCategoryCodeInfo);

			ClassificationValidatorHelper.CheckLicensType(invoice.US_DDTCUSMLCategoryCodeInfo, invoice.US_LicenseTypeInfo, requireDDTCDataList, System.Array.Empty<ZString>(), USMLCategoryCodes.Codes.Ammunition,
				ClassificationValidator.DDTCUSMLCategoryCodeIsRequiredMessage,
				ClassificationValidator.DDTCUSMLCategoryCodeShouldNotBeEntered,
				Factory);

			declaration.US_LicenseType = USAESLicenseCode.Codes.SAG;
			invoice.US_DDTCUSMLCategoryCode = ZString.Empty;
			invoice.US_LicenseType = ZString.Empty;
			AssertHasMessageError(invoice.US_DDTCUSMLCategoryCodeInfo, ClassificationValidator.DDTCUSMLCategoryCodeIsRequiredMessage);

			invoice.US_DDTCUSMLCategoryCode = "ZZ";
			AssertNoMessageError(invoice.US_DDTCUSMLCategoryCodeInfo, ClassificationValidator.DDTCUSMLCategoryCodeIsRequiredMessage);
			AssertHasMessageErrorContaining(invoice.US_DDTCUSMLCategoryCodeInfo, ListValidation.InvalidCodeMessageError);

			foreach (ICodeDescription code in new USMLCategoryCodes())
			{
				invoice.US_DDTCUSMLCategoryCode = code.Code;
				AssertNoMessageError(invoice.US_DDTCUSMLCategoryCodeInfo, ClassificationValidator.DDTCUSMLCategoryCodeIsRequiredMessage);
				AssertNoMessageErrorContaining(invoice.US_DDTCUSMLCategoryCodeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		[TestDate(2016, 09, 20)]
		public void TestLicenseTypeExemptionValidation()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(declaration.Factory, new ZString[] { USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32 });

			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.US_LicenseType = "";
			declaration.US_LicenseNo = "";
			declaration.US_ECCN = "";
			declaration.US_ExportCode = "";

			invoice.US_LicenseType = "";
			invoice.US_LicenseNo = "";
			invoice.US_ECCN = "";
			invoice.US_ExportCode = "";

			AssertNoMessageErrors(invoice.US_LicenseTypeInfo);
			AssertNoMessageErrors(invoice.US_LicenseNoInfo);
			AssertNoMessageErrors(invoice.US_ECCNInfo);
			AssertNoMessageErrors(invoice.US_ExportCodeInfo);

			invoice.US_LicenseType = USAESLicenseCode.Codes.C30;
			AssertNoMessageErrors(invoice.US_LicenseTypeInfo);
			AssertHasMessageErrors(invoice.US_LicenseNoInfo);
			AssertHasMessageErrors(invoice.US_ECCNInfo);
			AssertHasMessageErrors(invoice.US_ExportCodeInfo);

			invoice.US_LicenseType = USAESLicenseCode.Codes.C32;
			AssertHasMessageErrors(invoice.US_LicenseTypeInfo);
			AssertNoMessageErrors(invoice.US_LicenseNoInfo);
			AssertHasMessageErrors(invoice.US_ECCNInfo);
			AssertHasMessageErrors(invoice.US_ExportCodeInfo);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			invoice.US_LicenseType = USAESLicenseCode.Codes.C32;
			AssertNoMessageErrors(invoice.US_LicenseTypeInfo);
			AssertNoMessageErrors(invoice.US_LicenseNoInfo);
			AssertHasMessageErrors(invoice.US_ECCNInfo);
			AssertHasMessageErrors(invoice.US_ExportCodeInfo);
		}

		public void TestCheckUS_TariffType()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			invoice.US_TariffType = ZString.Empty;
			AssertHasMessageErrorContaining(invoice.US_TariffTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoice.US_TariffTypeInfo, ListValidation.InvalidCodeMessageError);

			var list = new TariffTypeList();
			foreach (CodeDescriptionPair pair in list)
			{
				invoice.US_TariffType = pair.Code;
				AssertNoMessageErrorContaining(invoice.US_TariffTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(invoice.US_TariffTypeInfo, ListValidation.InvalidCodeMessageError);
			}

			invoice.US_TariffType = "ZSD";
			AssertNoMessageErrorContaining(invoice.US_TariffTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoice.US_TariffTypeInfo, ListValidation.InvalidCodeMessageError);

			invoice.JZ_JE = declaration.PK;
			invoice.AddInfoValidation.ValidateUS_TariffType();
			AssertNoMessageErrorContaining(invoice.US_TariffTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoice.US_TariffTypeInfo, ListValidation.InvalidCodeMessageError);
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

			declaration.US_LicenseType = "";
			invoice.US_LicenseType = "BA";
			AssertHasMessageErrorContaining(invoice.US_LicenseTypeInfo, ExportAddInfoJobDeclarationValidation.CodeIsInvalid);

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ICodeDescription pair in list)
			{
				invoice.US_LicenseType = pair.Code;
				AssertNoMessageErrorContaining(invoice.US_LicenseTypeInfo, ExportAddInfoJobDeclarationValidation.CodeIsInvalid);
			}

			invoice.US_LicenseType = " ";
			AssertNoMessageErrorContaining(invoice.US_LicenseTypeInfo, ExportAddInfoJobDeclarationValidation.CodeIsInvalid);
		}

		public void TestCheckUS_LicenseTypeWithDifferentTypeOnInvoiceLine()
		{
			var factory = new BusinessObjectFactory();
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(factory, new ZString[] { USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.SAG });

			var org = factory.New<OrgHeader>();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.DDTCRegistrationNumber, "123345", Core.Constants.CountryCodes.UnitedStates);

			declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_LicenseType = "C33";
			AssertEquals(invoice.US_LicenseType, declaration.US_LicenseType);

			invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = org.PK;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceUQ = "M3";
			AssertEquals(invoiceLine.US_LicenseType, declaration.US_LicenseType);
			invoice.AddInfoValidation.ValidateUS_LicenseType();
			AssertNoWarning(invoice.US_LicenseTypeInfo, ExportAddInfoJobComInvoiceHeaderValidation.LicenseTypeSyncError);

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceUQ = "M3";
			AssertEquals(invoiceLine2.US_LicenseType, declaration.US_LicenseType);
			AssertNoWarning(invoice.US_LicenseTypeInfo, ExportAddInfoJobComInvoiceHeaderValidation.LicenseTypeSyncError);

			invoiceLine2.US_LicenseType = "SAG";
			invoice.AddInfoValidation.ValidateUS_LicenseType();
			AssertHasWarning(invoice.US_LicenseTypeInfo, ExportAddInfoJobComInvoiceHeaderValidation.LicenseTypeSyncError);
		}

		public void TestCheckUS_LicenseTypeWithDifferentLinesWhenTypeIsEmpty()
		{
			var factory = new BusinessObjectFactory();
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(factory, new ZString[] { USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.SAG });

			declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_LicenseType = "C33";
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "header1";

			var line1 = invoice.InvoiceLines.AddNew();
			line1.US_LicenseType = "C33";
			var line2 = invoice.InvoiceLines.AddNew();
			line2.US_LicenseType = "SAG";
			var line3 = invoice.InvoiceLines.AddNew();
			line3.US_LicenseType = "C41";
			invoice.AddInfoValidation.ValidateUS_LicenseType();
			AssertHasWarningContaining(invoice.US_LicenseTypeInfo, ExportAddInfoJobComInvoiceHeaderValidation.LicenseTypeSyncError);

			declaration.US_LicenseType = "";
			invoice.US_LicenseType = "";
			AssertNoWarningContaining(invoice.US_LicenseTypeInfo, ExportAddInfoJobComInvoiceHeaderValidation.LicenseTypeSyncError);
		}

		public void TestCheckUS_LicenseTypeWithInvalidExportDate()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals(ZDateTime.Today, invoiceLine.ExportDateForLicenseType);
			invoiceLine.US_DateOfExport = ZDateTime.Today.AddDays(-1);
			AssertEquals(ZDateTime.Today.AddDays(-1), invoiceLine.ExportDateForLicenseType);

			invoiceLine.US_DateOfExport = ZDateTime.Invalid;
			AssertEquals("Invalidat Date should now allow on US_DateOfExport in InvoiceLine", ZDateTime.Today, invoiceLine.ExportDateForLicenseType);
			dec.US_DateOfExport = ZDateTime.Today.AddDays(-5);
			AssertEquals("fallback to US_DateOfExport on JobDeclaration", dec.US_DateOfExport, invoiceLine.ExportDateForLicenseType);
		}

		public void TestCheckUS_LicenseTypeForCuba()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(declaration.Factory, new ZString[] { USAESLicenseCode.Codes.C46 });

			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Cuba;
			invoice.US_LicenseType = USAESLicenseCode.Codes.C46;

			AssertHasMessageErrorContaining(invoice.US_LicenseTypeInfo, LicenseValidationHelper.LicenseTypeForCuba);
		}

		public void TestCheckUS_ImportEntryNo()
		{
			string messageError = ExportAddInfoJobComInvoiceHeaderValidation.ImportEntryNoIsRequiredForWithdrawal;
			declaration.US_ImportEntryNo = "";
			AssertUS_ImportEntryNoHasNoMessageErrorSettingInvoice("", messageError);
			AssertUS_ImportEntryNoHasNoMessageErrorSettingDeclaration("", messageError);

			AssertUS_ImportEntryNoSettingInvoice(InbondTypeList.Codes.IEWarehouseWithdrawal, messageError);
			AssertUS_ImportEntryNoSettingInvoice(InbondTypeList.Codes.TAndEWarehouseWithdrawal, messageError);
			AssertUS_ImportEntryNoSettingInvoice(InbondTypeList.Codes.IEForeignTradeZoneWithdrawal, messageError);
			AssertUS_ImportEntryNoSettingInvoice(InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal, messageError);

			AssertUS_ImportEntryNoSettingDeclaration(InbondTypeList.Codes.IEWarehouseWithdrawal, messageError);
			AssertUS_ImportEntryNoSettingDeclaration(InbondTypeList.Codes.TAndEWarehouseWithdrawal, messageError);
			AssertUS_ImportEntryNoSettingDeclaration(InbondTypeList.Codes.IEForeignTradeZoneWithdrawal, messageError);
			AssertUS_ImportEntryNoSettingDeclaration(InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal, messageError);

			AssertUS_ImportEntryNoHasNoMessageErrorSettingInvoice(InbondTypeList.Codes.MerchandiseNOTShippedInbond, messageError);
			AssertUS_ImportEntryNoHasNoMessageErrorSettingDeclaration(InbondTypeList.Codes.MerchandiseNOTShippedInbond, messageError);

			declaration.US_ImportEntryNo = "123";
			AssertUS_ImportEntryNoHasNoMessageErrorSettingInvoice(InbondTypeList.Codes.IEWarehouseWithdrawal, messageError);
			AssertUS_ImportEntryNoHasNoMessageErrorSettingInvoice(InbondTypeList.Codes.TAndEWarehouseWithdrawal, messageError);
			AssertUS_ImportEntryNoHasNoMessageErrorSettingInvoice(InbondTypeList.Codes.IEForeignTradeZoneWithdrawal, messageError);
			AssertUS_ImportEntryNoHasNoMessageErrorSettingInvoice(InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal, messageError);
			AssertUS_ImportEntryNoHasNoMessageErrorSettingDeclaration(InbondTypeList.Codes.IEWarehouseWithdrawal, messageError);
			AssertUS_ImportEntryNoHasNoMessageErrorSettingDeclaration(InbondTypeList.Codes.TAndEWarehouseWithdrawal, messageError);
			AssertUS_ImportEntryNoHasNoMessageErrorSettingDeclaration(InbondTypeList.Codes.IEForeignTradeZoneWithdrawal, messageError);
			AssertUS_ImportEntryNoHasNoMessageErrorSettingDeclaration(InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal, messageError);

			declaration.US_ImportEntryNo = "";
			invoice.US_ImportEntryNo = "";
			invoice.US_InbondType = "";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DDTCITARExemptionNo = "123.13";
			invoice.AddInfoValidation.ValidateUS_ImportEntryNo();
			AssertNoMessageErrors(ExportAddInfoJobComInvoiceHeaderValidation.ImportEntryNoIsRequiredForLicenseExemption123dot4, invoice.US_ImportEntryNoInfo);

			invoiceLine.US_DDTCITARExemptionNo = "123.4A3";
			invoice.AddInfoValidation.ValidateUS_ImportEntryNo();
			AssertHasMessageErrors(ExportAddInfoJobComInvoiceHeaderValidation.ImportEntryNoIsRequiredForLicenseExemption123dot4, invoice.US_ImportEntryNoInfo);

			invoice.US_ImportEntryNo = "123";
			AssertNoMessageErrors(ExportAddInfoJobComInvoiceHeaderValidation.ImportEntryNoIsRequiredForLicenseExemption123dot4, invoice.US_ImportEntryNoInfo);

			declaration.US_ImportEntryNo = "123";
			invoice.US_ImportEntryNo = "";
			AssertNoMessageErrors(ExportAddInfoJobComInvoiceHeaderValidation.ImportEntryNoIsRequiredForLicenseExemption123dot4, invoice.US_ImportEntryNoInfo);

			invoiceLine.US_DDTCITARExemptionNo = "123.4B";
			declaration.US_ImportEntryNo = "";
			invoice.AddInfoValidation.ValidateUS_ImportEntryNo();
			AssertHasMessageErrors(ExportAddInfoJobComInvoiceHeaderValidation.ImportEntryNoIsRequiredForLicenseExemption123dot4, invoice.US_ImportEntryNoInfo);
		}

		[TestDate(2013, 03, 18)]
		public void TestCheckUS_ForeignTradeZone()
		{
			declaration.US_ForeignTradeZone = "";
			var messageError = ExportAddInfoJobComInvoiceHeaderValidation.ForeignTradeZoneIsRequiredForInbondType67or68;
			AssertUS_ForeignTradeZoneHasNoMessageErrorSettingInvoice("", messageError);
			AssertUS_ForeignTradeZoneHasNoMessageErrorSettingDeclaration("", messageError);

			AssertUS_ForeignTradeZoneSettingInvoice(InbondTypeList.Codes.IEForeignTradeZoneWithdrawal, messageError);
			AssertUS_ForeignTradeZoneSettingInvoice(InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal, messageError);
			AssertUS_ForeignTradeZoneSettingDeclaration(InbondTypeList.Codes.IEForeignTradeZoneWithdrawal, messageError);
			AssertUS_ForeignTradeZoneSettingDeclaration(InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal, messageError);

			AssertUS_ForeignTradeZoneHasNoMessageErrorSettingInvoice(InbondTypeList.Codes.IEWarehouseWithdrawal, messageError);
			AssertUS_ForeignTradeZoneHasNoMessageErrorSettingInvoice(InbondTypeList.Codes.MerchandiseNOTShippedInbond, messageError);
			AssertUS_ForeignTradeZoneHasNoMessageErrorSettingInvoice(InbondTypeList.Codes.TAndEWarehouseWithdrawal, messageError);
			AssertUS_ForeignTradeZoneHasNoMessageErrorSettingDeclaration(InbondTypeList.Codes.IEWarehouseWithdrawal, messageError);
			AssertUS_ForeignTradeZoneHasNoMessageErrorSettingDeclaration(InbondTypeList.Codes.MerchandiseNOTShippedInbond, messageError);
			AssertUS_ForeignTradeZoneHasNoMessageErrorSettingDeclaration(InbondTypeList.Codes.TAndEWarehouseWithdrawal, messageError);

			declaration.US_ForeignTradeZone = "ZZ";
			AssertUS_ForeignTradeZoneHasNoMessageErrorSettingInvoice(InbondTypeList.Codes.IEForeignTradeZoneWithdrawal, messageError);
			AssertUS_ForeignTradeZoneHasNoMessageErrorSettingInvoice(InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal, messageError);
			AssertUS_ForeignTradeZoneHasNoMessageErrorSettingDeclaration(InbondTypeList.Codes.IEForeignTradeZoneWithdrawal, messageError);
			AssertUS_ForeignTradeZoneHasNoMessageErrorSettingDeclaration(InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal, messageError);

			invoice.US_ForeignTradeZone = "123456";
			AssertHasMessageError(invoice.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);
			invoice.US_ForeignTradeZone = "123ABEE";
			AssertNoMessageError(invoice.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);
			invoice.US_ForeignTradeZone = "123AB00";
			AssertNoMessageError(invoice.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);
			invoice.US_ForeignTradeZone = "286A099";
			AssertNoMessageError(invoice.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);
			invoice.US_ForeignTradeZone = "312A099";
			AssertHasMessageError(invoice.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);
			invoice.US_ForeignTradeZone = "29";
			AssertHasMessageError(invoice.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);
			invoice.US_ForeignTradeZone = "AAA";
			AssertHasMessageError(invoice.US_ForeignTradeZoneInfo, ExportFieldsValidator.InvalidFTZIndicatorFormat);
		}

		public void TestCheckUS_InbondType()
		{
			declaration.US_InbondType = "";
			declaration.US_ImportEntryNo = "";
			declaration.US_ForeignTradeZone = "";
			invoice.US_InbondType = "";
			invoice.US_ImportEntryNo = "";
			invoice.US_ForeignTradeZone = "";
			AssertNoMessageErrorContaining(invoice.US_ImportEntryNoInfo, ExportAddInfoJobComInvoiceHeaderValidation.ImportEntryNoIsRequiredForWithdrawal);
			AssertNoMessageErrorContaining(invoice.US_ForeignTradeZoneInfo, ExportAddInfoJobComInvoiceHeaderValidation.ForeignTradeZoneIsRequiredForInbondType67or68);
			AssertNoMessageErrorContaining(invoice.US_InbondTypeInfo, ListValidation.InvalidCodeMessageError);

			invoice.US_InbondType = "BA";
			AssertNoMessageErrorContaining(invoice.US_ImportEntryNoInfo, ExportAddInfoJobComInvoiceHeaderValidation.ImportEntryNoIsRequiredForWithdrawal);
			AssertNoMessageErrorContaining(invoice.US_ForeignTradeZoneInfo, ExportAddInfoJobComInvoiceHeaderValidation.ForeignTradeZoneIsRequiredForInbondType67or68);
			AssertHasMessageErrorContaining(invoice.US_InbondTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(invoice.US_InbondTypeInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.US_InbondType = InbondTypeList.Codes.IEForeignTradeZoneWithdrawal;
			AssertHasMessageErrorContaining(invoice.US_ImportEntryNoInfo, ExportAddInfoJobComInvoiceHeaderValidation.ImportEntryNoIsRequiredForWithdrawal);
			AssertHasMessageErrorContaining(invoice.US_ForeignTradeZoneInfo, ExportAddInfoJobComInvoiceHeaderValidation.ForeignTradeZoneIsRequiredForInbondType67or68);
			AssertNoMessageErrorContaining(invoice.US_InbondTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(invoice.US_InbondTypeInfo, MandatoryValidation.YouHaveNotEntered);

			InbondTypeList list = new InbondTypeList();
			foreach (CodeDescriptionPair pair in list)
			{
				invoice.US_InbondType = pair.Code;
				AssertNoMessageErrorContaining(invoice.US_InbondTypeInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageErrorContaining(invoice.US_InbondTypeInfo, MandatoryValidation.YouHaveNotEntered);
			}

			invoice.US_InbondType = "";
			AssertNoMessageErrorContaining(invoice.US_ImportEntryNoInfo, ExportAddInfoJobComInvoiceHeaderValidation.ImportEntryNoIsRequiredForWithdrawal);
			AssertNoMessageErrorContaining(invoice.US_ForeignTradeZoneInfo, ExportAddInfoJobComInvoiceHeaderValidation.ForeignTradeZoneIsRequiredForInbondType67or68);

			invoice.US_InbondType = InbondTypeList.Codes.IEWarehouseWithdrawal;
			invoice.US_InbondType = "";
			AssertHasMessageErrorContaining(invoice.US_InbondTypeInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.US_InbondType = InbondTypeList.Codes.MerchandiseNOTShippedInbond;
			AssertNoMessageErrorContaining(invoice.US_InbondTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_StateOfOrigin()
		{
			declaration.US_StateOfOrigin = "";
			invoice.US_StateOfOrigin = "NY";
			AssertNotEquals("US_StateOfOrigin_Effective", "", invoice.US_StateOfOrigin);
			AssertNoMessageError(invoice.US_StateOfOriginInfo, ExportAddInfoJobComInvoiceHeaderValidation.StateOfOriginRequired);

			invoice.US_StateOfOrigin = "";
			AssertEquals("US_StateOfOrigin_Effective", "", invoice.US_StateOfOrigin);
			AssertHasMessageError(invoice.US_StateOfOriginInfo, ExportAddInfoJobComInvoiceHeaderValidation.StateOfOriginRequired);

			declaration.US_StateOfOrigin = "CA";
			invoice.US_StateOfOrigin = "";
			AssertNotEquals("US_StateOfOrigin_Effective", "", invoice.US_StateOfOrigin);
			AssertNoMessageError(invoice.US_StateOfOriginInfo, ExportAddInfoJobComInvoiceHeaderValidation.StateOfOriginRequired);

			declaration.US_StateOfOrigin = ZString.Empty;
			invoice.US_StateOfOrigin = "";
			AssertHasMessageError(invoice.US_StateOfOriginInfo, ExportAddInfoJobComInvoiceHeaderValidation.StateOfOriginRequired);

			invoice.US_StateOfOrigin = "~";
			AssertHasMessageError(invoice.US_StateOfOriginInfo, ValidationConstants.Declaration.DestinationStateShouldBeInList);

			invoice.US_StateOfOrigin = USStateList.Codes.Alabama;
			AssertNoMessageError(invoice.US_StateOfOriginInfo, ValidationConstants.Declaration.DestinationStateShouldBeInList);
		}

		public void TestCheckUS_TransactionsRelated()
		{
			string messageError = "The code you have selected is not in the list.";
			declaration.US_TransactionsRelated = "";
			invoice.US_TransactionsRelated = "K";
			AssertNotEquals("US_TransactionsRelated_Effective", "", invoice.US_TransactionsRelated);
			AssertNoMessageError(invoice.US_TransactionsRelatedInfo, ExportAddInfoJobComInvoiceHeaderValidation.TransactionsRelatedRequired);
			AssertHasMessageError(invoice.US_TransactionsRelatedInfo, messageError);

			invoice.US_TransactionsRelated = "";
			AssertEquals("US_TransactionsRelated_Effective", "", invoice.US_TransactionsRelated);
			AssertHasMessageError(invoice.US_TransactionsRelatedInfo, ExportAddInfoJobComInvoiceHeaderValidation.TransactionsRelatedRequired);
			AssertNoMessageError(invoice.US_TransactionsRelatedInfo, messageError);

			declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._2Predeparture;
			declaration.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			invoice.US_TransactionsRelated = "";
			AssertNotEquals("US_TransactionsRelated_Effective", "", invoice.US_TransactionsRelated);
			AssertNoMessageError(invoice.US_TransactionsRelatedInfo, ExportAddInfoJobComInvoiceHeaderValidation.TransactionsRelatedRequired);

			declaration.US_TransactionsRelated = "";
			CodeDescriptionPairList list = invoice.AddInfoLookups.US_YesNoList;
			foreach (CodeDescriptionPair pair in list)
			{
				invoice.US_TransactionsRelated = pair.Code;
				AssertNoMessageError(invoice.US_TransactionsRelatedInfo, messageError);
			}
		}

		public void TestCheckUS_HazardousCargo()
		{
			string messageError = "The code you have selected is not in the list.";
			declaration.US_HazardousCargo = "";
			invoice.US_HazardousCargo = "K";
			AssertNotEquals("US_HazardousCargo_Effective", "", invoice.US_HazardousCargo);
			AssertNoMessageError(invoice.US_HazardousCargoInfo, ExportAddInfoJobComInvoiceHeaderValidation.HazardousCargoRequired);
			AssertHasMessageError(invoice.US_HazardousCargoInfo, messageError);

			invoice.US_HazardousCargo = "";
			AssertEquals("US_HazardousCargo_Effective", "", invoice.US_HazardousCargo);
			AssertHasMessageError(invoice.US_HazardousCargoInfo, ExportAddInfoJobComInvoiceHeaderValidation.HazardousCargoRequired);
			AssertNoMessageError(invoice.US_HazardousCargoInfo, messageError);

			declaration.US_HazardousCargo = YesNoDefaultList.Codes.Yes;
			invoice.US_HazardousCargo = "";
			AssertNotEquals("US_HazardousCargo_Effective", "", invoice.US_HazardousCargo);
			AssertNoMessageError(invoice.US_HazardousCargoInfo, ExportAddInfoJobComInvoiceHeaderValidation.HazardousCargoRequired);

			declaration.US_HazardousCargo = "";
			CodeDescriptionPairList list = invoice.AddInfoLookups.US_YesNoList;
			foreach (CodeDescriptionPair pair in list)
			{
				invoice.US_HazardousCargo = pair.Code;
				AssertNoMessageError(invoice.US_HazardousCargoInfo, messageError);
			}
		}

		public void TestCheckUS_RoutedTransaction()
		{
			string messageError = "The code you have selected is not in the list.";
			declaration.US_RoutedTransaction = "";
			invoice.US_RoutedTransaction = "K";
			AssertNotEquals("US_RoutedTransaction_Effective", "", invoice.US_RoutedTransaction);
			AssertNoMessageError(invoice.US_RoutedTransactionInfo, ExportAddInfoJobComInvoiceHeaderValidation.RoutedTransactionRequired);
			AssertHasMessageError(invoice.US_RoutedTransactionInfo, messageError);

			invoice.US_RoutedTransaction = "";
			AssertEquals("US_RoutedTransaction_Effective", "", invoice.US_RoutedTransaction);
			AssertHasMessageError(invoice.US_RoutedTransactionInfo, ExportAddInfoJobComInvoiceHeaderValidation.RoutedTransactionRequired);
			AssertNoMessageError(invoice.US_RoutedTransactionInfo, messageError);

			declaration.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;
			invoice.US_RoutedTransaction = "";
			AssertNotEquals("US_RoutedTransaction_Effective", "", invoice.US_RoutedTransaction);
			AssertNoMessageError(invoice.US_RoutedTransactionInfo, ExportAddInfoJobComInvoiceHeaderValidation.RoutedTransactionRequired);

			declaration.US_RoutedTransaction = "";
			CodeDescriptionPairList list = invoice.AddInfoLookups.US_YesNoList;
			foreach (CodeDescriptionPair pair in list)
			{
				invoice.US_RoutedTransaction = pair.Code;
				AssertNoMessageError(invoice.US_RoutedTransactionInfo, messageError);
			}
		}

		public void TestCheckUS_UltimateDestinationCountry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedStates;

			var org01 = Factory.New<OrgHeader>();
			org01.FillWithValidTestData();

			declaration.JE_OH_Importer = org01.PK;
			var invoice01 = declaration.Invoices.AddNew();

			var usTerritory = new string[4] { Core.Constants.CountryCodes.Guam, Core.Constants.CountryCodes.UnitedStatesMinorIslands,
			Core.Constants.CountryCodes.AmericanSamoa, Core.Constants.CountryCodes.NorthernMarianaIslands  };

			foreach (string territory in usTerritory)
			{
				invoice01.US_UltimateDestinationCountry = territory;
				AssertHasMessageError(invoice01.US_UltimateDestinationCountryInfo, "The reporting of Goods to US territories (Guam, American Samoa, Northern Mariana Islands, Wake Island or Midway Island) is not required.");
			}
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice = declaration.Invoices.AddNew();
		}

		void AssertUS_ImportEntryNoHasNoMessageErrorSettingInvoice(string inbondType, string messageError)
		{
			declaration.US_InbondType = inbondType;
			invoice.US_InbondType = "";
			AssertUS_ImportEntryNoHasNoMessageError(inbondType, messageError);
		}

		void AssertUS_ImportEntryNoHasNoMessageErrorSettingDeclaration(string inbondType, string messageError)
		{
			declaration.US_InbondType = "";
			invoice.US_InbondType = inbondType;
			AssertUS_ImportEntryNoHasNoMessageError(inbondType, messageError);
		}

		void AssertUS_ImportEntryNoHasNoMessageError(string inbondType, string messageError)
		{
			AssertEquals("US_InbondType_Effective", inbondType, invoice.US_InbondType);
			invoice.US_ImportEntryNo = "";
			AssertNoMessageErrorContaining(invoice.US_ImportEntryNoInfo, messageError);
		}

		void AssertUS_ImportEntryNoSettingDeclaration(string inbondType, string messageError)
		{
			declaration.US_InbondType = inbondType;
			invoice.US_InbondType = "";
			AssertUS_ImportEntryNo(inbondType, messageError);
		}

		void AssertUS_ImportEntryNoSettingInvoice(string inbondType, string messageError)
		{
			declaration.US_InbondType = "";
			invoice.US_InbondType = inbondType;
			AssertUS_ImportEntryNo(inbondType, messageError);
		}

		void AssertUS_ImportEntryNo(string inbondType, string messageError)
		{
			AssertEquals("US_InbondType_Effective", inbondType, invoice.US_InbondType);
			invoice.US_ImportEntryNo = "";
			AssertHasMessageErrorContaining(invoice.US_ImportEntryNoInfo, messageError);
			invoice.US_ImportEntryNo = "2342";
			AssertNoMessageErrorContaining(invoice.US_ImportEntryNoInfo, messageError);
		}

		void AssertUS_ForeignTradeZoneHasNoMessageErrorSettingDeclaration(string inbondType, string messageError)
		{
			declaration.US_InbondType = inbondType;
			invoice.US_InbondType = "";
			AssertUS_ForeignTradeZoneHasNoMessageError(inbondType, messageError);
		}

		void AssertUS_ForeignTradeZoneHasNoMessageErrorSettingInvoice(string inbondType, string messageError)
		{
			declaration.US_InbondType = "";
			invoice.US_InbondType = inbondType;
			AssertUS_ForeignTradeZoneHasNoMessageError(inbondType, messageError);
		}

		void AssertUS_ForeignTradeZoneHasNoMessageError(string inbondType, string messageError)
		{
			AssertEquals("US_InbondType_Effective", inbondType, invoice.US_InbondType);
			invoice.US_ForeignTradeZone = "";
			AssertNoMessageErrorContaining(invoice.US_ForeignTradeZoneInfo, messageError);
		}

		void AssertUS_ForeignTradeZoneSettingDeclaration(string inbondType, string messageError)
		{
			declaration.US_InbondType = inbondType;
			invoice.US_InbondType = "";
			AssertUS_ForeignTradeZone(inbondType, messageError);
		}

		void AssertUS_ForeignTradeZoneSettingInvoice(string inbondType, string messageError)
		{
			declaration.US_InbondType = "";
			invoice.US_InbondType = inbondType;
			AssertUS_ForeignTradeZone(inbondType, messageError);
		}

		void AssertUS_ForeignTradeZone(string inbondType, string messageError)
		{
			AssertEquals("US_InbondType_Effective", inbondType, invoice.US_InbondType);
			invoice.US_ForeignTradeZone = "";
			AssertHasMessageErrorContaining(invoice.US_ForeignTradeZoneInfo, messageError);
			invoice.US_ForeignTradeZone = "2342";
			AssertNoMessageErrorContaining(invoice.US_ForeignTradeZoneInfo, messageError);
		}
	}
}
