using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZAddInfoJobComInvoiceLineValidationTest : CommonImportAddInfoComInvoiceLineValidationTest
	{
		public void TestCheckUS_LicenseType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType);
			var codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType, "STL", "STL Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodePK = codeList.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ImportPermitLicenceType, "01");
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_PermitLicenseIndicator = "01";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.US_LicenseTypeInfo, "123", "STL");
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertNoMessageErrorContaining(invoiceLine.US_LicenseTypeInfo, FTZAddInfoJobComInvoiceLineValidation.LicenseNotApplicableToTariff);
			invoiceLine.US_LicenseType = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_LicenseTypeInfo, FTZAddInfoJobComInvoiceLineValidation.LicenseNotApplicableToTariff);
			invoiceLine.US_LicenseType = "123";
			AssertHasMessageErrorContaining(invoiceLine.US_LicenseTypeInfo, FTZAddInfoJobComInvoiceLineValidation.LicenseNotApplicableToTariff);
		}

		public void TestCheckPNFTZPGAIndicator()
		{
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			importTariff.UE_OGACodes = "";
			importTariff.UE_PGACodes = "FD2";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EnableSPN = true;
			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = importTariff.UE_Tariff;
			invoiceLine.US_FDAIndicator = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDARequiredButBlank);
			invoiceLine.US_FDAIndicator = "D";
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineRequired);
			invoiceLine.ACE_FDALines.AddNew();
			invoiceLine.AddInfoValidation.ValidateUS_FDAIndicator();
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineRequired);
			AssertNoWarning(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDANotRequired);
			importTariff.UE_PGACodes = "";
			importTariff.Factory.Save();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = importTariff.UE_Tariff;
			invoiceLine2.ACE_FDALines.AddNew();
			invoiceLine2.US_FDAIndicator = "D";
			AssertHasWarning(invoiceLine2.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDANotRequired);
		}

		public void TestCheckJI_OA_ManufacturerAddress()
		{
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			AssertNoNotifications(invoiceLine.JI_OA_ManufacturerAddressInfo);
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			string messageError = string.Format(OrganisationValidation.ManufacturerIDMissing, party.MainAddress.OA_Code);
			AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, messageError);
			var cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AU34567");
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, messageError);
			AssertJI_OA_ManufacturerAddressForMIDAgainstCanadianProvince(invoiceLine);
		}

		[TestDate(2009, 12, 12)]
		public void TestCheckUS_UC_NKCountryOfExport()
		{
			invoiceLine.US_UC_NKCountryOfExport = "US";
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport);
			invoiceLine.US_UC_NKCountryOfExport = "PR";
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport);
			invoiceLine.US_UC_NKCountryOfExport = "~";
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_UC_NKCountryOfExport = CanadaProvinceTerritoryCodes.Codes.XA;
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, ExternalValidation.CanadianProvinceCodeNotAllowedForCountryOfExport);
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, ExternalValidation.CanadianProvinceCodeNotAllowedForCountryOfExport);
			invoiceLine.US_UC_NKCountryOfExport = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportNeeded);
		}

		[TestDate(2009, 12, 12)]
		public void TestUS_UC_NKCountryOfOrigin()
		{
			AssertUS_UC_NKCountryOfOrigin(invoiceLine);
		}

		public void TestCheckUS_F_PNDisclaimer()
		{
			invoiceLine.US_F_PNDisclaimer = "$";
			AssertHasMessageErrorContaining(invoiceLine.US_F_PNDisclaimerInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_ZoneStatus()
		{
			invoiceLine.US_ZoneStatus = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_ZoneStatusInfo, MandatoryValidation.YouHaveNotEntered);
			AssertUS_PrivilegedStatusDate(invoiceLine);
		}

		public void TestCheckUS_SPI()
		{
			AssertUS_SPI(invoiceLine);
		}

		public void TestCheckUS_TexileCategoryNo()
		{
			AssertUS_TexileCategoryNo(invoiceLine);
			AssertTExtileCategoryNoAganstXLine(invoiceLine);
		}

		public void TestCheckUS_ManifestQty()
		{
			declaration.JE_MasterBill = "TEST";
			declaration.PrimaryMasterBill.US_AMSCarrierIndicator = "N";
			var invoice = declaration.Invoices.AddNew();
			var setHeaderLine = invoice.JobComInvoiceLines.AddNew();
			setHeaderLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			var setComponentLine = invoice.JobComInvoiceLines.AddNew();
			setComponentLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			var normalLine = invoice.JobComInvoiceLines.AddNew();
			setHeaderLine.US_ManifestQty = 0;
			setComponentLine.US_ManifestQty = 0;
			normalLine.US_ManifestQty = 0;
			AssertNoMessageErrorContaining(setHeaderLine.US_ManifestQtyInfo, FTZAddInfoJobComInvoiceLineValidation.ManifestQtyRequired);
			AssertEquals("no message errors expected", false, setComponentLine.US_ManifestQtyInfo.HasMessageErrors());
			AssertNoMessageErrorContaining(normalLine.US_ManifestQtyInfo, FTZAddInfoJobComInvoiceLineValidation.ManifestQtyRequired);
			setHeaderLine.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.NoUnitRequired;
			setComponentLine.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.NoUnitRequired;
			normalLine.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.NoUnitRequired;
			setHeaderLine.AddInfoValidation.ValidateUS_ManifestQty();
			setComponentLine.AddInfoValidation.ValidateUS_ManifestQty();
			normalLine.AddInfoValidation.ValidateUS_ManifestQty();
			AssertHasMessageErrorContaining(setHeaderLine.US_ManifestQtyInfo, FTZAddInfoJobComInvoiceLineValidation.ManifestQtyRequired);
			AssertHasMessageErrorContaining(setComponentLine.US_ManifestQtyInfo, FTZAddInfoJobComInvoiceLineValidation.ManifestQtyRequired);
			AssertHasMessageErrorContaining(normalLine.US_ManifestQtyInfo, FTZAddInfoJobComInvoiceLineValidation.ManifestQtyRequired);
		}

		public void TestCheckUS_FDAIndicator()
		{
			USCTariff importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			importTariff.UE_OGACodes = "FD4";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EnableSPN = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineRequired);
		}

		[TestDate(2015, 01, 12)]
		public override void TestCheckUS_MiscPermitNo()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType);
			var codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType, "STL", "STL Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodePK = codeList.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ImportPermitLicenceType, "01");
			codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType, "DIA", "DIA Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			refCusCodePK = codeList.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ImportPermitLicenceType, "06");
			codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType, "ALU", "ALU Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			refCusCodePK = codeList.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ImportPermitLicenceType, "28");
			Factory.Save();
			invoiceLine.US_MiscPermitNo = ZString.Empty;
			invoiceLine.US_LicenseType = "STL";
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasMessageErrorContaining(invoiceLine.US_MiscPermitNoInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_LicenseType = "DIA";
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasMessageErrorContaining(invoiceLine.US_MiscPermitNoInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_LicenseType = "ALU";
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasMessageErrorContaining(invoiceLine.US_MiscPermitNoInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_LicenseType = ZString.Empty;
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertNoMessageErrorContaining(invoiceLine.US_MiscPermitNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_SupTariffForA9903()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffHelper = new USCTariffTestCase(Factory);
			var startDate = new ZDate(2016, 01, 01);
			var endDate = new ZDate(2079, 01, 01);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();
			var tariff73 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "6307909870", startDate, endDate);
			var tariff9903 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038001", startDate, endDate);
			var tariff99038842 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038842", startDate, endDate);
			tariffHelper.CreateNewTariffIfNotExists("6307909870", startDate, endDate, "KG", "", "", "");
			tariffHelper.CreateNewTariffIfNotExists("99038001", startDate, endDate, "KG", "", "", "");
			tariffHelper.CreateNewTariffIfNotExists("99038842", startDate, endDate, "KG", "", "", "");
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			Factory.Save();
			var rate = helper.CreateRate(tariff9903, rateCode.PK, startDate, endDate, "0.25");
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.HongKong, "HK", startDate, endDate);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.HongKong, startDate, endDate);
			var applicability = helper.CreateCusApplicability(rate, tradeGroup, startDate, endDate);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6307909870";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertEquals(ZString.Empty, invoiceLine.US_SupTariff);
			AssertNoWarningContaining(invoiceLine.US_SupTariffInfo, "Provisional/Program Tariff should not be used in a 214 message. It is only needed on the Entry Type 06 withdrawal from the zone.");
			invoiceLine.US_SupTariff = "99038842";
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertHasWarningContaining(invoiceLine.US_SupTariffInfo, "Provisional/Program Tariff should not be used in a 214 message. It is only needed on the Entry Type 06 withdrawal from the zone.");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "6307909870";
			invoiceLine.US_SupTariff = "99038842";
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoWarningContaining(invoiceLine.US_SupTariffInfo, "Provisional/Program Tariff should not be used in a 214 message. It is only needed on the Entry Type 06 withdrawal from the zone.");
		}

		public void TestCheckUS_SupTariffForA99()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffHelper = new USCTariffTestCase(Factory);
			var startDate = new ZDate(2016, 01, 01);
			var endDate = new ZDate(2079, 01, 01);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();
			var tariff73 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "7301100000", startDate, endDate);
			var tariff9903 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038001", startDate, endDate);
			tariffHelper.CreateNewTariffIfNotExists("7301100000", startDate, endDate, "KG", "", "", "");
			tariffHelper.CreateNewTariffIfNotExists("99038001", startDate, endDate, "KG", "", "", "");
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			Factory.Save();
			var rate = helper.CreateRate(tariff9903, rateCode.PK, startDate, endDate, "0.25");
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.HongKong, "HK", startDate, endDate);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.HongKong, startDate, endDate);
			var applicability = helper.CreateCusApplicability(rate, tradeGroup, startDate, endDate);
			var relationship = helper.CreateTariffRelationship(tariff9903.PK, hsnTariffType.PK, "73");
			var tariffAttribute = helper.CreateTariffAttribute("RULE", "A99", tariff9903);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7301100000";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertEquals(ZString.Empty, invoiceLine.US_SupTariff);
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
			invoiceLine.US_SupTariff = "12341234";
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Only certain tariffs can be selected");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_CertifyCargoRelease = false;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Brazil;
			AssertEquals(ZString.Empty, invoiceLine.US_SupTariff);
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			AssertEquals("99038001", invoiceLine.US_SupTariff);
			invoiceLine.US_SupTariff = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff number is required");
			invoiceLine.US_SupTariff = "12341234";
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Only certain tariffs can be selected");
		}

		public void TestValidateADDCVDCases()
		{
			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "ADD111";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			addCase.U5_ISOCountryCode = "CA";
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "22030060";
			var cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "CVD111";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			cvdCase.U5_ISOCountryCode = "CA";
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "22030060";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_ADDCaseNo = "ADD111";
			invoiceLine.US_CVDCaseNo = "CVD111";
			AssertNotNull("PreCondition(AntidumpingDutyCase)", invoiceLine.AntidumpingDutyCase);
			AssertNotNull("PreCondition(CountervailingDutyCase)", invoiceLine.CountervailingDutyCase);
			AssertHasMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, CommonImportAddInfoJobComInvoiceLineValidation.CaseNoAgainstCountryOfOriginPartialMessage);
			AssertHasMessageErrorContaining(invoiceLine.US_CVDCaseNoInfo, CommonImportAddInfoJobComInvoiceLineValidation.CaseNoAgainstCountryOfOriginPartialMessage);
			invoiceLine.US_UC_NKCountryOfOrigin = "XC";
			AssertNoMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, CommonImportAddInfoJobComInvoiceLineValidation.CaseNoAgainstCountryOfOriginPartialMessage);
			AssertNoMessageErrorContaining(invoiceLine.US_CVDCaseNoInfo, CommonImportAddInfoJobComInvoiceLineValidation.CaseNoAgainstCountryOfOriginPartialMessage);
		}

		public void TestADDCVD_ZoneStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "ADD111";
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			AssertHasMessageErrorContaining(invoiceLine.US_ZoneStatusInfo, "Privileged Foreign Merchandise");
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			AssertNoMessageErrorContaining(invoiceLine.US_ZoneStatusInfo, "Privileged Foreign Merchandise");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		void AssertJI_OA_ManufacturerAddressForMIDAgainstCanadianProvince(JobComInvoiceLine invoiceLine)
		{
			var party = Factory.New<OrgHeader>();
			var mainAddress = party.MainAddress;
			var cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AU34567");
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			cusCode.OK_CustomsRegNo = "AU34567";
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XY;
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertHasMessageErrorContaining(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.Canadian);
			cusCode.OK_CustomsRegNo = "XO1";
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertNoMessageErrorContaining(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.Canadian);
		}
	}
}
