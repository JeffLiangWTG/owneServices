using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACSImportAddInfoJobComInvoiceLineValidationTest : TestCaseWithFactory
	{
		public void TestReRunJI_ParentIDWhenVIsSet()
		{
			invoiceLine.US_SecondarySPI = "X";
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();
			var invoiceLine8 = invoiceLine.AddSecondaryInvoiceLine();
			AssertEquals("PreCondition", "V", invoiceLine8.US_SecondarySPI);
			Assert(!invoiceLine8.JI_ParentIDInfo.HasMessageError(JobComInvoiceLineValidation.MaxSecondaryLinesCount));
		}

		public void TestCheckUS_CAExportCertificate()
		{
			#region Setup Ref Db Test Data

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._16, "Canadian Export Sugar Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"\w{1,8}");
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatErrorText, "1 to 8 alpha-numeric characters");
			Factory.Save();

			#endregion

			var sugarCertValidFormatText = "The Canadian Export Sugar Certificate number is invalid. The format should be 1 to 8 alpha-numeric characters.";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.AddInfoValidation.ValidateUS_CAExportCertificate();
			AssertEquals(false, invoiceLine.US_CAExportCertificateInfo.HasMessageErrors());
			invoiceLine.US_CAExportCertificate = "1";
			AssertEquals(true, invoiceLine.US_CAExportCertificateInfo.HasMessageError(CanadaExportSugarCertificateValidator.CASugarCertCountryOfExport));
			AssertEquals(false, invoiceLine.US_CAExportCertificateInfo.HasMessageError(CanadaExportSugarCertificateValidator.CASugarCertCountryOfOrigin));
			AssertEquals(false, invoiceLine.US_CAExportCertificateInfo.HasMessageError(sugarCertValidFormatText));
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;
			invoiceLine.AddInfoValidation.ValidateUS_CAExportCertificate();
			AssertEquals(false, invoiceLine.US_CAExportCertificateInfo.HasMessageError(CanadaExportSugarCertificateValidator.CASugarCertCountryOfExport));
			AssertEquals(true, invoiceLine.US_CAExportCertificateInfo.HasMessageError(CanadaExportSugarCertificateValidator.CASugarCertCountryOfOrigin));
			AssertEquals(false, invoiceLine.US_CAExportCertificateInfo.HasMessageError(sugarCertValidFormatText));
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Canada;
			invoiceLine.AddInfoValidation.ValidateUS_CAExportCertificate();
			AssertEquals(false, invoiceLine.US_CAExportCertificateInfo.HasMessageError(CanadaExportSugarCertificateValidator.CASugarCertCountryOfOrigin));
			AssertEquals(false, invoiceLine.US_CAExportCertificateInfo.HasMessageError(sugarCertValidFormatText));
			invoiceLine.US_CAExportCertificate = "123456";
			AssertEquals(false, invoiceLine.US_CAExportCertificateInfo.HasMessageError(sugarCertValidFormatText));
			invoiceLine.US_CAExportCertificate = "12345678";
			AssertEquals(false, invoiceLine.US_CAExportCertificateInfo.HasMessageError(sugarCertValidFormatText));
			invoiceLine.US_CAExportCertificate = "1234#67";
			AssertEquals(true, invoiceLine.US_CAExportCertificateInfo.HasMessageError(sugarCertValidFormatText));
			invoiceLine.US_CAExportCertificate = "1234567";
			AssertEquals(false, invoiceLine.US_CAExportCertificateInfo.HasMessageError(sugarCertValidFormatText));
			invoiceLine.US_CAExportCertificate = "ABCRREFG";
			AssertEquals(false, invoiceLine.US_CAExportCertificateInfo.HasMessageError(sugarCertValidFormatText));
			invoiceLine.US_CAExportCertificate = "1234 678";
			AssertEquals(true, invoiceLine.US_CAExportCertificateInfo.HasMessageError(sugarCertValidFormatText));
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_CAExportCertificate = "12344556";
			AssertHasMessageError(invoiceLine.US_CAExportCertificateInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit));
		}

		public void TestLumberPermitNo()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._11, "Atlantic Lumber Board (ALB) Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "44091090";
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.CanadaSoftwoodLumberExportNumber;
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			invoiceLine.JI_Tariff = "44091090";
			invoiceLine.US_UC_NKCountryOfOrigin = "XA";
			invoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.Yes;
			invoiceLine.US_MiscPermitNo = "";
			AssertHasMessageErrorContaining(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberRequiredForOriginCanadaProvinceAndSomeTariffs);
			invoiceLine.US_UC_NKCountryOfOrigin = "XC";
			invoiceLine.US_MiscPermitNo = "111";
			AssertHasWarning(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.OriginCannotBeBCForSoftwoodLumber);
			invoiceLine.US_UC_NKCountryOfOrigin = "XD";
			invoiceLine.US_MiscPermitNo = "112";
			AssertNoWarning(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.OriginCannotBeBCForSoftwoodLumber);
			invoiceLine.US_MiscPermitNo = "123456789";
			AssertNoMessageErrorContaining(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberRequiredForOriginCanadaProvinceAndSomeTariffs);
			invoiceLine.US_UC_NKCountryOfOrigin = "AD";
			invoiceLine.US_MiscPermitNo = "123456789";
			AssertNoMessageErrorContaining(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberRequiredForOriginCanadaProvinceAndSomeTariffs);
			declaration.US_EntryType = EntryTypeList.Codes.Appraisement;
			invoiceLine.US_MiscPermitNo = "";
			AssertNoMessageErrorContaining(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberRequiredForOriginCanadaProvinceAndSomeTariffs);
			tariff.UE_Tariff = "4407100001";
			invoiceLine.JI_Tariff = "4407100001";
			invoiceLine.US_UC_NKCountryOfOrigin = "GB";
			invoiceLine.US_MiscPermitNo = "778";
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberShouldNotBeEntered);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.US_MiscPermitNo = "";
			AssertNoMessageError(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberShouldNotBeEntered);
			declaration.US_EntryType = EntryTypeList.Codes.Appraisement;
			invoiceLine.US_MiscPermitNo = "778";
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberShouldNotBeEntered);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			tariff.UE_Tariff = "44091090";
			invoiceLine.JI_Tariff = "44091090";
			invoiceLine.US_UC_NKCountryOfOrigin = "IT";
			invoiceLine.US_MiscPermitNo = "778";
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberShouldNotBeEntered);
			invoiceLine.US_MiscPermitNo = "";
			AssertNoMessageError(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberShouldNotBeEntered);
			declaration.US_EntryType = EntryTypeList.Codes.Appraisement;
			invoiceLine.US_MiscPermitNo = "778";
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberShouldNotBeEntered);
		}

		public void TestLumberPermitNo2()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._11, "Atlantic Lumber Board (ALB) Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "44091090";
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.CanadaSoftwoodLumberExportNumber;
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			invoiceLine.US_SupTariff = "98010060"; // permit indicator is empty
			invoiceLine.JI_Tariff = "44091090"; // 11
			invoiceLine.US_UC_NKCountryOfOrigin = "XA";
			invoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.Yes;
			invoiceLine.US_MiscPermitNo = "";
			AssertHasMessageErrorContaining(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberRequiredForOriginCanadaProvinceAndSomeTariffs);
			invoiceLine.US_UC_NKCountryOfOrigin = "XC";
			invoiceLine.US_MiscPermitNo = "111";
			AssertHasWarningContaining(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.OriginCannotBeBCForSoftwoodLumber);
			invoiceLine.US_UC_NKCountryOfOrigin = "XD";
			invoiceLine.US_MiscPermitNo = "112";
			AssertNoWarningContaining(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.OriginCannotBeBCForSoftwoodLumber);
			invoiceLine.US_MiscPermitNo = "123456789";
			AssertNoMessageErrorContaining(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberRequiredForOriginCanadaProvinceAndSomeTariffs);
			invoiceLine.US_UC_NKCountryOfOrigin = "AD";
			invoiceLine.US_MiscPermitNo = "123456789";
			AssertNoMessageErrorContaining(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberRequiredForOriginCanadaProvinceAndSomeTariffs);
			declaration.US_EntryType = EntryTypeList.Codes.Appraisement;
			invoiceLine.US_MiscPermitNo = "";
			AssertNoMessageError(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberRequiredForOriginCanadaProvinceAndSomeTariffs);
			tariff.UE_Tariff = "4407100001";
			invoiceLine.JI_Tariff = "4407100001";
			invoiceLine.US_UC_NKCountryOfOrigin = "GB";
			invoiceLine.US_MiscPermitNo = "778";
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberShouldNotBeEntered);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.US_MiscPermitNo = "";
			AssertNoMessageError(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberShouldNotBeEntered);
			declaration.US_EntryType = EntryTypeList.Codes.Appraisement;
			invoiceLine.US_MiscPermitNo = "778";
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberShouldNotBeEntered);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			tariff.UE_Tariff = "44091090";
			invoiceLine.JI_Tariff = "44091090";
			invoiceLine.US_UC_NKCountryOfOrigin = "IT";
			invoiceLine.US_MiscPermitNo = "778";
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberShouldNotBeEntered);
			invoiceLine.US_MiscPermitNo = "";
			AssertNoMessageError(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberShouldNotBeEntered);
			declaration.US_EntryType = EntryTypeList.Codes.Appraisement;
			invoiceLine.US_MiscPermitNo = "778";
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, CanadaSoftwoodLumberExportPermitValidator.LumberPermitNumberShouldNotBeEntered);
		}

		public void TestCementLicenseNo()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._09, "Mexican Cement Import License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2523900000";
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.MexicanCementImportLicense;
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Japan;
			invoiceLine.US_MiscPermitNo = "";
			var messageErrorText = "The Mexican Cement Import License number is required.";
			AssertNoMessageErrorContaining(invoiceLine.US_MiscPermitNoInfo, messageErrorText);
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasMessageErrorContaining(invoiceLine.US_MiscPermitNoInfo, messageErrorText);
			invoiceLine.US_MiscPermitNo = "111";
			AssertNoMessageErrorContaining(invoiceLine.US_MiscPermitNoInfo, messageErrorText);
		}

		public void TestCheckUS_ADDuty()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_ADDuty = ZDecimal.Zero;
			AssertHasMessageError(invoiceLine.US_ADDutyInfo, ACSImportAddInfoJobComInvoiceLineValidation.NoSpecificDutyAmount);
			invoiceLine.US_ADDuty = 10m;
			AssertNoMessageError(invoiceLine.US_ADDutyInfo, ACSImportAddInfoJobComInvoiceLineValidation.NoSpecificDutyAmount);
		}

		public void TestCheckUS_CVDuty()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_CVDuty = ZDecimal.Zero;
			AssertHasMessageError(invoiceLine.US_CVDutyInfo, ACSImportAddInfoJobComInvoiceLineValidation.NoSpecificDutyAmount);
			invoiceLine.US_CVDuty = 10m;
			AssertNoMessageError(invoiceLine.US_CVDutyInfo, ACSImportAddInfoJobComInvoiceLineValidation.NoSpecificDutyAmount);
		}

		public void TestCheckADDCVDCases()
		{
			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "AXXAAABBB";
			addCase.U5_ISOCountryCode = "AU";
			addCase.U5_CaseStatus = "IO";
			addCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			addCase.U5_ManufacturerMID = ZString.Empty;
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			invoiceLine.US_CVDCaseNo = "CXXAAABBB";
			invoiceLine.US_ADDCaseNo = "CXXAAABBB";
			AssertHasMessageError(invoiceLine.US_CVDCaseNoInfo, ACSImportAddInfoJobComInvoiceLineValidation.ADD_CVDNotAllowedForRLF);
			AssertHasMessageError(invoiceLine.US_ADDCaseNoInfo, ACSImportAddInfoJobComInvoiceLineValidation.ADD_CVDNotAllowedForRLF);
			declaration.US_EntryMode = ZString.Empty;
			invoiceLine.US_CVDCaseNo = "CXXAAABBB";
			invoiceLine.US_ADDCaseNo = "CXXAAABBB";
			AssertNoMessageError(invoiceLine.US_CVDCaseNoInfo, ACSImportAddInfoJobComInvoiceLineValidation.ADD_CVDNotAllowedForRLF);
			AssertNoMessageError(invoiceLine.US_ADDCaseNoInfo, ACSImportAddInfoJobComInvoiceLineValidation.ADD_CVDNotAllowedForRLF);
		}

		public void TestCheckUS_SecondarySPI()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_OGACodes = "AAAFD4";
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "1234567891";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff2.UE_OGACodes = "AAAFD2";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.JI_Tariff = "1234567890";
			var firstVLine = invoice.InvoiceLines.AddNew();
			var secondVline = invoice.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			firstVLine.JI_ParentID = invoiceLine.PK;
			firstVLine.JI_Tariff = "1234567891";
			firstVLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			secondVline.JI_ParentID = invoiceLine.PK;
			secondVline.JI_Tariff = "1234567891";
			secondVline.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("Precondition : Second V line has line number greater than first V line", true, secondVline.JI_LineNo > firstVLine.JI_LineNo);
			AssertHasMessageError("There is a error in first V line due to different tariff number from X line", firstVLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);
			AssertNoMessageError("There is no error in second V line despite of having different tariff number from X line", secondVline.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.SameTariffNumberForTheFirstVLine);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
