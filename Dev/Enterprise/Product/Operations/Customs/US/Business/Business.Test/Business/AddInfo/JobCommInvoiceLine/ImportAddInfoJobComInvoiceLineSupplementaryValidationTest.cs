using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ImportAddInfoJobComInvoiceLineSupplementaryValidationTest : CommonImportAddInfoComInvoiceLineValidationTest
	{
		[TestDate(2009, 1, 1)]
		public void TestCheckOverrideTaxIndicator()
		{
			invoiceLine.US_SupTariff = "9802008044";
			// When tax is required
			invoiceLine.JI_Tariff = USCTariff.DistilledSpiritsFeeApplicable;
			invoiceLine.US_TaxApply = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_TaxApplyInfo, USCTariff.TaxIsRequired);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			AssertHasMessageError(invoiceLine.US_TaxApplyInfo, USCTariff.TaxIsRequired);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			AssertNoMessageError(invoiceLine.US_TaxApplyInfo, USCTariff.TaxIsRequired);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			AssertNoMessageError(invoiceLine.US_TaxApplyInfo, USCTariff.TaxIsRequired);
			// When tax is conditional
			invoiceLine.JI_Tariff = USCTariff.TaxConditional;
			invoiceLine.US_TaxApply = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_TaxApplyInfo, USCTariff.TaxApplicabilityMustBeSelected);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			AssertNoMessageError(invoiceLine.US_TaxApplyInfo, USCTariff.TaxApplicabilityMustBeSelected);
			AssertNoMessageError(invoiceLine.US_TaxApplyInfo, USCTariff.TaxIsRequired);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			AssertNoMessageError(invoiceLine.US_TaxApplyInfo, USCTariff.TaxApplicabilityMustBeSelected);
			AssertNoMessageError(invoiceLine.US_TaxApplyInfo, USCTariff.TaxIsRequired);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			AssertNoMessageError(invoiceLine.US_TaxApplyInfo, USCTariff.TaxApplicabilityMustBeSelected);
			AssertNoMessageError(invoiceLine.US_TaxApplyInfo, USCTariff.TaxIsRequired);
		}

		[TestDate(2009, 1, 1)]
		public void TestInvalidDutyRateWithoutSPI()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99100466";
			invoiceLine.US_UC_NKCountryOfOrigin = "SG";
			invoiceLine.US_SPI = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidDutyRateWithEmptySPI);
			invoiceLine.US_SPI = "SG";
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidDutyRateWithEmptySPI);
		}

		[TestDate(2010, 03, 31)]
		public void TestTextileCategoryNumberNotRequiredForCertainTariffs()
		{
			invoiceLine.US_SupTariff = "9802008044";
			invoiceLine.JI_Tariff = "2204215015";
			invoiceLine.US_TextileCategoryNo = "123";
			string messageError = string.Format(FormalImportAddInfoJobComInvoiceLineValidation.ParentTariffNumberMakesCategoryNumberExempt, "9802008044");
			AssertHasMessageError(invoiceLine.US_TextileCategoryNoInfo, messageError);
			invoiceLine.US_TextileCategoryNo = "";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, messageError);
		}

		public void TestTextileCategoryNo()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1111111111";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today;
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.HaitiTariffHope;
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_Tariff = "1111111111";
			invoiceLine.US_SupTariff = "1111111111";
			invoiceLine.JI_Tariff = "2204215015";
			invoiceLine.US_TextileCategoryNo = "";
			AssertHasMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryIsMandatory);
			invoiceLine.US_TextileCategoryNo = "55";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryIsMandatory);
		}

		public void TestTextileExportDateNotEnteredForNonTextileCategoryClass()
		{
			invoiceLine.US_SupTariff = USCTariff.AdditionalDutyCalculationApplicable; //99040237
			invoiceLine.US_TextileCategoryNo = "123";
			AssertNoMessageErrors(invoiceLine.US_DateOfExportFromCountryOfOriginInfo);
			invoiceLine.US_TextileCategoryNo = "";
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Today;
			AssertHasMessageError(invoiceLine.US_DateOfExportFromCountryOfOriginInfo, "Please do not enter an Export Date from C/O if there is no Textile Category No.");
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Empty;
			AssertNoMessageErrors(invoiceLine.US_DateOfExportFromCountryOfOriginInfo);
		}

		public void TestTextileExportDate()
		{
			invoiceLine.US_SupTariff = USCTariff.AdditionalDutyCalculationApplicable; //99040237
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine.US_TextileCategoryNo = "123";
			invoiceLine.US_UC_NKCountryOfOrigin = "IT";
			invoiceLine.US_UC_NKCountryOfExport = "IT";
			invoiceLine.InvoiceHeader.US_DateOfExport = ZDateTime.Today.AddDays(1);
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Today;
			AssertHasMessageError(invoiceLine.US_DateOfExportFromCountryOfOriginInfo, FormalImportAddInfoJobComInvoiceLineValidation.OriginAndExportDateMustBeTheSame);
			invoiceLine.InvoiceHeader.US_DateOfExport = ZDateTime.Today;
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Today;
			AssertNoMessageError(invoiceLine.US_DateOfExportFromCountryOfOriginInfo, FormalImportAddInfoJobComInvoiceLineValidation.OriginAndExportDateMustBeTheSame);
			invoiceLine.US_UC_NKCountryOfOrigin = "FR";
			invoiceLine.InvoiceHeader.US_DateOfExport = ZDateTime.Today.AddDays(-1);
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Today;
			AssertHasMessageError(invoiceLine.US_DateOfExportFromCountryOfOriginInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileExportDateNotLater);
			invoiceLine.InvoiceHeader.US_DateOfExport = ZDateTime.Today.AddDays(3);
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Today;
			AssertNoMessageError(invoiceLine.US_DateOfExportFromCountryOfOriginInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileExportDateNotLater);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Today;
			AssertHasMessageError(invoiceLine.US_DateOfExportFromCountryOfOriginInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.TextileCLassificationDetails));
		}

		public void TestTextileCategoryForSPI()
		{
			invoiceLine.US_SupTariff = USCTariff.AdditionalDutyCalculationApplicable; //99040237
			invoiceLine.JI_Tariff = "5407820090";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.G;
			invoiceLine.US_TextileCategoryNo = "228";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryForSPI);
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_TextileCategoryNo = "228";
			AssertHasMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryForSPI);
		}

		[TestDate(2009, 12, 12)]
		public void TestCheckMIDForTextileTariffs()
		{
			OrgHeader aUManufacturer = Factory.New<OrgHeader>();
			OrgAddress aUManufacturerMainAddress = aUManufacturer.MainAddress;
			OrgHeader cAManufacturer = Factory.New<OrgHeader>();
			OrgAddress cAManufacturerMainAddress = cAManufacturer.MainAddress;
			OrgCusCode cAManCusCode = cAManufacturerMainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XB734957834");
			OrgHeader tWManufacturer = Factory.New<OrgHeader>();
			OrgAddress tWManufacturerMainAddress = tWManufacturer.MainAddress;
			OrgCusCode tWManCusCode = tWManufacturerMainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "TW929384527");
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextile);
			invoiceLine.US_SupTariff = "99990064";
			invoiceLine.JI_Tariff = "3921902550";
			invoiceLine.JI_OA_ManufacturerAddress = aUManufacturerMainAddress.PK;
			AssertHasMessageErrors("No MID on manufacturer", invoiceLine.JI_OA_ManufacturerAddressInfo);
			invoiceLine.JI_Tariff = "6401100000";
			AssertNoMessageError("Non textile tariff", invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextile);
			OrgCusCode aUManCusCode = aUManufacturerMainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AU345675329");
			invoiceLine.JI_Tariff = "3921902550";
			invoiceLine.JI_OA_ManufacturerAddress = aUManufacturerMainAddress.PK;
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextile);
			declaration.JE_OH_Supplier = aUManufacturer.PK;
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextile);
			invoice.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_OA_ManufacturerAddress = cAManufacturerMainAddress.PK;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertHasMessageError("MID now Canadian, origin still AU", invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextile);
			invoiceLine.JI_Tariff = "6401100000";
			AssertNoMessageError("Non textile tariff - validation irrelevant again", invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextile);
			invoice.US_UC_NKCountryOfOrigin = "CA";
			invoiceLine.JI_Tariff = "3921902550";
			invoiceLine.JI_OA_ManufacturerAddress = cAManufacturerMainAddress.PK;
			invoiceLine.US_UC_NKCountryOfOrigin = "XY";
			AssertNoMessageError("MID & Origin is Canadian", invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextile);
			AssertHasWarning("MID & Origin is Canadian but provinces mismatch", invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextileCanadianProvinces);
			invoiceLine.JI_OA_ManufacturerAddress = tWManufacturerMainAddress.PK;
			invoiceLine.US_UC_NKCountryOfOrigin = "CA";
			AssertHasMessageError("MID now Taiwan, origin still CA", invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextile);
			invoiceLine.JI_Tariff = "6401100000";
			AssertNoMessageError("Non textile tariff - validation irrelevant again", invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextile);
			invoice.US_UC_NKCountryOfOrigin = "TW";
			invoiceLine.US_UC_NKCountryOfOrigin = "TW";
			invoiceLine.JI_Tariff = "3921902550";
			invoiceLine.JI_OA_ManufacturerAddress = tWManufacturerMainAddress.PK;
			AssertNoMessageError("MID & Origin is Taiwanese", invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextile);
			invoice.US_UC_NKCountryOfOrigin = "XO";
			invoiceLine.JI_Tariff = "3921902550";
			invoiceLine.JI_OA_ManufacturerAddress = cAManufacturerMainAddress.PK;
			invoiceLine.US_UC_NKCountryOfOrigin = "XO";
			AssertHasWarning("MID & Origin is Canadian but provinces mismatch", invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextileCanadianProvinces);
			invoice.US_UC_NKCountryOfOrigin = "XB";
			invoiceLine.US_UC_NKCountryOfOrigin = "XB";
			invoiceLine.US_SupTariff = "9612109010";
			invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoWarning("MID & Origin is Canadian but provinces mismatch", invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextileCanadianProvinces);
		}

		public void TestMoreThanOneADDDetails()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802008044";
			invoiceLine.JI_Tariff = "2204215015";
			invoiceLine.US_ADDCaseNo = "A475819004";
			AssertNoMessageError(invoiceLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.SecondaryADD_CVDCannotBeSentWhenParentLineHasADDOrCVD);
			AssertNoMessageError(invoiceLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.AnotherSecondaryHasADD_CVD);
			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.US_ADDCaseNo = "A475819004";
			JobComInvoiceLine secondaryLine2 = invoiceLine2.AddSecondaryInvoiceLine();
			secondaryLine2.US_ADDCaseNo = "A475819004";
			AssertHasMessageError(secondaryLine2.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.SecondaryADD_CVDCannotBeSentWhenParentLineHasADDOrCVD);
			invoiceLine2.US_ADDCaseNo = "";
			secondaryLine2.US_ADDCaseNo = "A475819004";
			AssertNoMessageError(secondaryLine2.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.SecondaryADD_CVDCannotBeSentWhenParentLineHasADDOrCVD);
			JobComInvoiceLine secondaryLine3 = invoiceLine2.AddSecondaryInvoiceLine();
			secondaryLine3.US_ADDCaseNo = "A475819004";
			AssertHasMessageError(secondaryLine3.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.AnotherSecondaryHasADD_CVD);
			secondaryLine3.US_ADDCaseNo = "";
			AssertNoMessageError(secondaryLine3.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.AnotherSecondaryHasADD_CVD);
		}

		public void TestMoreThanOneCVDDetails()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802008044";
			invoiceLine.JI_Tariff = "2204215015";
			invoiceLine.US_CVDCaseNo = "C475819004";
			AssertNoMessageError(invoiceLine.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.SecondaryADD_CVDCannotBeSentWhenParentLineHasADDOrCVD);
			AssertNoMessageError(invoiceLine.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.AnotherSecondaryHasADD_CVD);
			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.US_CVDCaseNo = "C475819004";
			invoiceLine2.US_UC_NKCountryOfOrigin = "AU";
			JobComInvoiceLine secondaryLine = invoiceLine2.AddSecondaryInvoiceLine();
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "CXXAAABBB";
			addCase.U5_ISOCountryCode = "AU";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			secondaryLine.US_UC_NKCountryOfOrigin = "AU";
			secondaryLine.US_CVDCaseNo = "CXXAAABBB";
			AssertHasMessageError(secondaryLine.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.SecondaryADD_CVDCannotBeSentWhenParentLineHasADDOrCVD);
			invoiceLine2.US_CVDCaseNo = "";
			secondaryLine.US_CVDCaseNo = "CXXAAABBB";
			AssertNoMessageError(secondaryLine.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.SecondaryADD_CVDCannotBeSentWhenParentLineHasADDOrCVD);
			JobComInvoiceLine secondaryLine2 = invoiceLine2.AddSecondaryInvoiceLine();
			secondaryLine2.US_CVDCaseNo = "CXXAAABBB";
			AssertHasMessageError(secondaryLine2.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.AnotherSecondaryHasADD_CVD);
			secondaryLine2.US_CVDCaseNo = "";
			AssertNoMessageError(secondaryLine2.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.AnotherSecondaryHasADD_CVD);
		}

		[TestDate(2008, 9, 11)]
		public void TestCottonFeeExemptCertificateShouldNotBeEnteredForExemptTariff()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1111111111";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "6206900040";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today;
			USCTariffDutyRate rate1 = tariff1.DutyRates.AddNew();
			rate1.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Cotton;
			rate1.UD_TaxFeeFlag = "1";
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "1111111111";
			invoiceLine.JI_Tariff = "6206900040";
			invoiceLine.US_UC_NKCountryOfOrigin = "TW";
			invoiceLine.US_UC_NKCountryOfExport = "TW";
			invoiceLine.JI_CustomsSecondQuantity = 500m; //affects the cotton fee (Second UQ: KG)
			AssertEquals("PreCondition:Exempt Indicator is empty", ZString.Empty, invoiceLine.US_CottonFeeExempt);
			invoiceLine.AddInfoValidation.ValidateUS_CottonFeeExempt();
			AssertHasMessageErrorContaining(invoiceLine.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeApplicable);
			invoiceLine.US_CottonFeeExempt = "~";
			AssertHasMessageErrorContaining(invoiceLine.US_CottonFeeExemptInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			AssertNoMessageErrorContaining(invoiceLine.US_CottonFeeExemptInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(invoiceLine.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeApplicable);
		}

		public void TestVisaNumberNotRequired()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ExportDate = new ZDateTime(2007, 1, 1);
			invoiceLine.JI_Tariff = "2517100015";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Lesotho;
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Empty;
			invoiceLine.US_SupTariff = "99990051";
			invoiceLine.US_VisaNo = "";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaNumberNotRequired);
			invoiceLine.US_VisaNo = "ABB";
			invoiceLine.AddInfoValidation.ValidateUS_VisaNo();
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaNumberNotRequired);
			invoiceLine = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.BrettsBirthday;
			invoiceLine.US_VisaNo = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaNoRequired);
			invoiceLine.US_VisaNo = "324098234";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaNoRequired);
			invoiceLine2.US_VisaNo = ZString.Empty;
			AssertNoMessageError(invoiceLine2.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaNoRequired);
		}

		public void TestCheckVisaFormat()
		{
			declaration.JE_ExportDate = new ZDateTime(2007, 1, 1);
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Lesotho;
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Empty;
			invoiceLine.US_SupTariff = FormalImportAddInfoJobComInvoiceLineValidation.CBTPAGoodsBrassiereTariff98201115;
			invoiceLine.US_VisaNo = "FCB123456";
			AssertHasMessageErrorContaining(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.InvalidFormatForBrassiereTariff98201115);
			invoiceLine.US_VisaNo = "0CB123456";
			AssertNoMessageErrorContaining(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.InvalidFormatForBrassiereTariff98201115);
			invoiceLine.US_SupTariff = "9802008042";
			invoiceLine.US_VisaNo = "ABB";
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaNumberNot9Characters);
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.SecondThirdLettersVisaNumbersShouldBeCountryOfOrigin);
			invoiceLine.US_VisaNo = "0LS456789";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaNumberNot9Characters);
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.FirstCharacterVisaNumberShouldBeANumber);
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.SecondThirdLettersVisaNumbersShouldBeCountryOfOrigin);
			invoiceLine.US_SupTariff = "9802004040";
			invoiceLine.US_VisaNo = "ABB";
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.FirstCharacterVisaNumberShouldBeANumber);
		}

		public void TestCheckVisaFormatForAGOATariffs()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Lesotho;
			AssertFirstCharacterOnAGOA(invoiceLine, "9802008042", "7LS456789", "1LS456789");
			AssertFirstCharacterOnAGOA(invoiceLine, "98191103", "7LS456789", "2LS456789");
			AssertFirstCharacterOnAGOA(invoiceLine, "98191106", "7LS456789", "3LS456789");
			AssertFirstCharacterOnAGOA(invoiceLine, "98191109", "7LS456789", "4LS456789");
			AssertFirstCharacterOnAGOA(invoiceLine, "98191112", "7LS456789", "5LS456789");
			AssertFirstCharacterOnAGOA(invoiceLine, "98191115", "7LS456789", "6LS456789");
			AssertFirstCharacterOnAGOA(invoiceLine, "98191118", "1LS456789", "7LS456789");
			AssertFirstCharacterOnAGOA(invoiceLine, "98191121", "1LS456789", "8LS456789");
			AssertFirstCharacterOnAGOA(invoiceLine, "98191124", "1LS456789", "8LS456789");
			AssertFirstCharacterOnAGOA(invoiceLine, "98191127", "1LS456789", "9LS456789");
		}

		public void TestCheckUS_VisaNoForConsumptionQuery()
		{
			var visa = Factory.New<USCVisa>();
			visa.UO_TextileCategoryNo = "123";
			visa.UO_UC_NKOriginCountry = "KR";
			visa.UO_BeginDate = ZDateTime.BrettsBirthday;
			visa.UO_EndDate = ZDateTime.Today.AddDays(3);
			var supTariff = new USCTariff.Loader(Factory).LoadBestMatch("99990051", ZDateTime.Today);
			if (supTariff == null)
			{
				supTariff = Factory.New<USCTariff>();
				supTariff.UE_Tariff = "99990051";
				supTariff.UE_DateFrom = new ZDateTime(1995, 01, 01);
				supTariff.UE_DateTo = new ZDateTime(2099, 01, 01);
				supTariff.UE_DutyComputationCode = "X";
				supTariff.UE_ShortDescription = "CT/M/M APR KNT/WV,NAFTA,3A";
				supTariff.UE_IsBaseRate = false;
				supTariff.UE_CountervailingDutyFlag = false;
				supTariff.UE_AdditionalTariffNumberIndicator = true;
				supTariff.UE_PermitLicenseIndicator = "03";
				supTariff.UE_AntiDumping = false;
				supTariff.UE_QuotaIndicator = true;
				supTariff.UE_ISOCountryofOriginEditCode = "CA";
			}

			Factory.Save();
			invoiceLine.US_SupTariff = supTariff.UE_Tariff;
			invoiceLine.Declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Empty;
			invoiceLine.US_TextileCategoryNo = "123";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaRequiredForTariffOriginAndCategory);
			invoiceLine.US_UC_NKCountryOfOrigin = "JP";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaRequiredForTariffOriginAndCategory);
			invoiceLine.US_TextileCategoryNo = "234";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaRequiredForTariffOriginAndCategory);
			invoiceLine.US_VisaNo = "7ZM123456";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaRequiredForTariffOriginAndCategory);
		}

		public void TestCheckUS_SPINotEnteredWhenThereAreApplicableSPIs()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9801007000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_SPICode = "P AUCACLILJOMAMXSG";
			invoiceLine.US_SupTariff = "9801007000";
			invoiceLine.JI_Tariff = "0201305000";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "";
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.ThereAreSPIsApplicableNonEntered);
			invoiceLine.US_SPI = "AU";
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.ThereAreSPIsApplicableNonEntered);
		}

		[TestDate(2008, 9, 11)]
		public void TestPrimarySPIIsNotEntereredForDutyFreeTariffItems()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.Free;
			tariff.UE_Tariff = "0705112000";
			tariff.UE_QuotaIndicator = true;
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			//set duty tariff item
			invoiceLine.US_SupTariff = "9801001069";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.B;
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.MayNotBeSpecifiedForDutyFree);
			invoiceLine.US_SupTariff = "";
			invoiceLine.US_SPI = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.MayNotBeSpecifiedForDutyFree);
			invoiceLine.JI_Tariff = "9706000060";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.Y;
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.MayNotBeSpecifiedForDutyFree);
		}

		public void TestCheckUS_SPI()
		{
			AssertUS_SPI(invoiceLine);
		}

		[TestDate(2007, 2, 12)]
		public void TestTariffForUnsupportedGSP()
		{
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Colombia;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Colombia;
			invoiceLine.JI_Tariff = "0603107010";
			invoiceLine.US_SPI = "A";
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, "is excluded for GSP for the selected tariff");
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Afghanistan;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Albania;
			invoiceLine.US_SupTariff = "9201900000";
			invoiceLine.US_SPI = "A";
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, "is excluded for GSP for the selected tariff");
		}

		[TestDate(2008, 9, 11)]
		public void TestUS_MiscPermitNo()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._02, "Singapore TPL Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "98206225";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today;
			tariff1.UE_PermitLicenseIndicator = "13"; //no specific validator exists for this #
			invoiceLine.US_SupTariff = "98206225";
			invoiceLine.US_MiscPermitNo = "";
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.MiscPermitNoRequired);
			invoiceLine.US_MiscPermitNo = "AAA";
			AssertNoMessageError(invoiceLine.US_MiscPermitNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.MiscPermitNoRequired);
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "99106101";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;
			tariff2.UE_PermitLicenseIndicator = "02";
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.SingaporePreferenceLevel;
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_Tariff = "99106101";
			invoiceLine.US_SupTariff = "99106101";
			invoiceLine.US_MiscPermitNo = "";
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, "The Singapore TPL Certificate number is required.");
			invoiceLine.US_MiscPermitNo = "AAA";
			AssertNoMessageError(invoiceLine.US_MiscPermitNoInfo, "The Singapore TPL Certificate number is required.");
		}

		public void TestUS_MiscPermitNo_Steel()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._01, "Steel Import License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "99990084";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today;
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "7208260060";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;
			tariff2.UE_PermitLicenseIndicator = "01";
			invoiceLine.US_SupTariff = "99990084";
			invoiceLine.JI_Tariff = "7208260060"; //01
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, "The Steel Import License number is required.");
		}

		[TestDate(2010, 03, 25)]
		public void TestUS_MiscPermitNo_Singapore()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._02, "Singapore TPL Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.US_SupTariff = "99106168"; //02
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, "The Singapore TPL Certificate number is required.");
		}

		[TestDate(2010, 03, 25)]
		public void TestUS_MiscPermitNo_CANAFTA()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._03, "Canadian NAFTA TPL Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.US_SupTariff = "99990053"; // 03
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, "The Canadian NAFTA TPL Certificate number is required.");
		}

		[TestDate(2010, 03, 25)]
		public void TestUS_MiscPermitNo_MXNAFTA()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._04, "Mexican NAFTA TPL Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.US_SupTariff = "99990061"; //04
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, "The Mexican NAFTA TPL Certificate number is required.");
		}

		public void TestUS_MiscPermitNo_Beef()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._05, "Beef Export Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "99990064";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today;
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0201305000";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;
			tariff2.UE_PermitLicenseIndicator = "05";
			invoiceLine.US_SupTariff = "99990064"; // permit indicator is empty
			invoiceLine.JI_Tariff = "0201305000"; //05
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, "The Beef Export Certificate number is required.");
		}

		public void TestUS_MiscPermitNo_Diamond()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._06, "Diamond Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "99990064";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today;
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "7102310000";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;
			tariff2.UE_PermitLicenseIndicator = "06";
			invoiceLine.US_SupTariff = "99990064"; // permit indicator is empty
			invoiceLine.JI_Tariff = "7102310000"; //06
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, "The Diamond Certificate number is required.");
		}

		[TestDate(2008, 02, 29)]
		public void TestUS_MiscPermitNo_ATPDEA()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._07, "Andean Trade Partnership Drug Eradication Act (ATPDEA) Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.US_SupTariff = "98211119"; // 07
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, "The Andean Trade Partnership Drug Eradication Act (ATPDEA) Certificate number is required.");
		}

		[TestDate(2010, 03, 25)]
		public void TestUS_MiscPermitNo_AUFreeTrade()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._08, "Australia Free Trade Export Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.US_SupTariff = "99130425"; //08
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, "The Australia Free Trade Export Certificate number is required.");
		}

		[TestDate(2010, 03, 25)]
		public void TestUS_MiscPermitNo_MXCement()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._09, "Mexican Cement Import License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.US_SupTariff = "99990064"; // permit indicator is empty
			invoiceLine.JI_Tariff = "2523290000"; //UE_PermitLicenseIndicator 09
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasWarning(invoiceLine.US_MiscPermitNoInfo, "Permit/License(Mexican Cement Import License) may be required for the selected tariff number.");
		}

		[TestDate(2010, 03, 25)]
		public void TestUS_MiscPermitNo_NicaraguaCAFTA()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._10, "CAFTA TPL Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.US_SupTariff = "99156101"; //10
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, "The CAFTA TPL Certificate number is required.");
		}

		[TestDate(2009, 12, 12)]
		public void TestUS_MiscPermitNo_CottonShirting()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._12, "Cotton Shirting Fabric License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.US_SupTariff = "99025208"; // 12
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, "The Cotton Shirting Fabric License number is required.");
		}

		public void TestFDAIndicator()
		{
			//FDA Admissibility Required
			invoiceLine.US_SupTariff = "9810008500";
			invoiceLine.US_FDAIndicator = "~";
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_FDAIndicator = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDARequiredButBlank);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDARequiredButBlank);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDARequiredButDisclaimed);
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDADisclaimedInvalid);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDARequiredButDisclaimed);
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineRequired);
			AssertNoWarning(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDANotRequired);
			invoiceLine.FDAs.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineRequired);
			invoiceLine.US_SupTariff = "9810006500";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineNotAllowed);
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDADisclaimedInvalid);
			invoiceLine.FDAs.RemoveAndDeleteAll();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineNotAllowed);
			//Not required
			invoiceLine.US_SupTariff = "99140479";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDADisclaimedInvalid);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDANotRequired);
			invoiceLine.US_FDAIndicator = "";
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDADisclaimedInvalid);
			AssertNoWarning(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDANotRequired);
		}

		public void TestDOTIndicator()
		{
			invoiceLine.US_SupTariff = "98130035";
			invoiceLine.US_DOTIndicator = "~";
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_DOTIndicator = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTRequiredButBlank);
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTRequiredButBlank);
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTRequiredButDisclaimed);
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTRequiredButDisclaimed);
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTLineRequired);
			invoiceLine.DOTs.AddNew();
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTLineRequired);
			invoiceLine.US_SupTariff = "9801001069";
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTLineNotAllowed);
			invoiceLine.DOTs.RemoveAndDeleteAll();
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTLineNotAllowed);
		}

		public void TestDOTIndicatorWithPGA()
		{
			invoiceLine.JI_Tariff = "98130035";
			invoiceLine.US_DOTIndicator = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTRequiredButBlank);
			invoiceLine.ImportTariff.UE_OGACodes = string.Empty;
			invoiceLine.ImportTariff.UE_PGACodes = "DT2";
			invoiceLine.US_DOTIndicator = "~";
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_DOTIndicator = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTRequiredButBlank);
		}

		public void TestCheckUS_98GoodsValue()
		{
			invoiceLine.US_SupTariff = "9822005060";
			invoiceLine.US_98GoodsValue = 900m;
			AssertHasMessageError(invoiceLine.US_98GoodsValueInfo, FormalImportAddInfoJobComInvoiceLineValidation._98GoodsValueExpectedToBeEnteredOnlyFor9801And9802);
			invoiceLine.US_98GoodsValue = 0m;
			invoiceLine.US_SupTariff = "9802005060";
			AssertNoMessageError(invoiceLine.US_98GoodsValueInfo, FormalImportAddInfoJobComInvoiceLineValidation._98GoodsValueExpectedToBeEnteredOnlyFor9801And9802);
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_98GoodsValue = 0m;
			AssertHasMessageError(invoiceLine.US_98GoodsValueInfo, ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);
			AssertHasMessageError(invoiceLine.US_98ValueInvCurrInfo, ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);
			invoiceLine.US_98GoodsValue = 900m;
			AssertNoMessageError(invoiceLine.US_98GoodsValueInfo, ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);
			AssertNoMessageError(invoiceLine.US_98ValueInvCurrInfo, ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);
		}

		public void TestChecUS_98ValueInvCurr()
		{
			invoiceLine.US_SupTariff = "9822005060";
			invoiceLine.US_98ValueInvCurr = 900m;
			AssertHasMessageError(invoiceLine.US_98ValueInvCurrInfo, FormalImportAddInfoJobComInvoiceLineValidation._98GoodsValueExpectedToBeEnteredOnlyFor9801And9802);
			invoiceLine.US_98ValueInvCurr = 0m;
			invoiceLine.US_SupTariff = "9802005060";
			AssertNoMessageError(invoiceLine.US_98ValueInvCurrInfo, FormalImportAddInfoJobComInvoiceLineValidation._98GoodsValueExpectedToBeEnteredOnlyFor9801And9802);
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_98ValueInvCurr = 0m;
			AssertHasMessageError(invoiceLine.US_98GoodsValueInfo, ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);
			AssertHasMessageError(invoiceLine.US_98ValueInvCurrInfo, ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);
			invoiceLine.US_98ValueInvCurr = 900m;
			AssertNoMessageError(invoiceLine.US_98GoodsValueInfo, ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);
			AssertNoMessageError(invoiceLine.US_98ValueInvCurrInfo, ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);
		}

		public void TestCheckUS_DestinationState()
		{
			invoiceLine.US_SupTariff = "9802008044";
			invoiceLine.JI_Tariff = "6206900040";
			invoiceLine.US_DestinationState = "";
			AssertHasMessageError(invoiceLine.US_DestinationStateInfo, FormalImportAddInfoJobComInvoiceLineValidation.DestinationStateIsRequired);
			invoiceLine.InvoiceHeader.US_DestinationState = "IL";
			invoiceLine.US_DestinationState = "";
			AssertNoMessageError(invoiceLine.US_DestinationStateInfo, FormalImportAddInfoJobComInvoiceLineValidation.DestinationStateIsRequired);
			invoiceLine.InvoiceHeader.US_DestinationState = "";
			invoiceLine.US_DestinationState = "";
			AssertHasMessageError(invoiceLine.US_DestinationStateInfo, FormalImportAddInfoJobComInvoiceLineValidation.DestinationStateIsRequired);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			invoiceLine.US_DestinationState = "";
			AssertNoMessageError(invoiceLine.US_DestinationStateInfo, FormalImportAddInfoJobComInvoiceLineValidation.DestinationStateIsRequired);
		}

		public void TestWhenNoDutyCalculationFormulaAvailable()
		{
			//RepairTariffs rule
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "98020060";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			invoiceLine.US_OverrideDuty = false;
			AssertNoWarning(invoiceLine.US_OverrideDutyInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);
			//AssembledAbroadOfUSProducts rule
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "98020080";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Now;
			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
			invoiceLine.US_SupTariff = tariff1.UE_Tariff;
			invoiceLine.US_OverrideDuty = false;
			AssertNoWarning(invoiceLine.US_OverrideDutyInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);
			invoiceLine.US_OverrideDuty = true;
			AssertNoWarning(invoiceLine.US_OverrideDutyInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "99990064";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Now;
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
			invoiceLine.US_SupTariff = tariff2.UE_Tariff;
			invoiceLine.US_OverrideDuty = false;
			AssertHasWarning(invoiceLine.US_OverrideSupDutyInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);
			invoiceLine.US_OverrideSupDuty = true;
			AssertNoWarning(invoiceLine.US_OverrideSupDutyInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);

			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "5407820090";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.Now;
			tariff3.UE_DutyComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
			invoiceLine.JI_Tariff = tariff3.UE_Tariff;
			invoiceLine.US_OverrideDuty = true;
			AssertNoWarning(invoiceLine.US_OverrideDutyInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);
			invoiceLine.US_OverrideDuty = false;
			AssertHasWarning(invoiceLine.US_OverrideDutyInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);
		}

		public void TestCheckUS_ZoneStatus()
		{
			invoiceLine.US_SupTariff = "99025208";
			invoiceLine.JI_Tariff = "0201305000";
			invoiceLine.US_ZoneStatus = "~";
			AssertNoMessageError(invoiceLine.US_ZoneStatusInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.US_ZoneStatus = "~";
			AssertHasMessageError(invoiceLine.US_ZoneStatusInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_ZoneStatus = "";
			AssertHasMessageErrorContaining(invoiceLine.US_ZoneStatusInfo, FormalImportAddInfoJobComInvoiceLineValidation.ZoneStatusShouldBeEntered);
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			AssertNoMessageErrors(invoiceLine.US_ZoneStatusInfo);
			invoiceLine.US_PrivilegedStatusDate = ZDateTime.Invalid;
			AssertHasNotifications("Preconditions: US_PrivilegedStatusDate validation should add at least 1 notification when invalid date was set in US_PrivilegedStatusDate", invoiceLine.US_PrivilegedStatusDateInfo);
			AssertUS_PrivilegedStatusDate(invoiceLine);
		}

		[TestDate(2009, 12, 12)]
		public void TestCheckUS_WoolLicenceNo()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._17, "Wool License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"W[a-zA-Z0-9]{8}");
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatErrorText, "first character should be 'W', characters 2-3 must represent the year of issue (e.g. 2008=08), the final six characters should be alpha numeric.");
			Factory.Save();

			var woolLicenseRequiredMessageText = "The Wool License number is required.";
			var woolLicenseShouldBeInCorrectFormatMessageText = "The Wool License number is invalid. The format should be first character should be 'W', characters 2-3 must represent the year of issue (e.g. 2008=08), the final six characters should be alpha numeric.";
			invoiceLine.US_SupTariff = "99025111";
			invoiceLine.US_WoolLicenceNo = "";
			AssertHasMessageError(invoiceLine.US_WoolLicenceNoInfo, woolLicenseRequiredMessageText);
			AssertNoMessageError(invoiceLine.US_WoolLicenceNoInfo, WoolLicenseValidator.WoolLicenseShouldNotBeEnteredForTariff);
			invoiceLine.US_WoolLicenceNo = "123456789";
			AssertNoMessageError(invoiceLine.US_WoolLicenceNoInfo, woolLicenseRequiredMessageText);
			AssertNoMessageError(invoiceLine.US_WoolLicenceNoInfo, WoolLicenseValidator.WoolLicenseShouldNotBeEnteredForTariff);
			AssertHasMessageError(invoiceLine.US_WoolLicenceNoInfo, woolLicenseShouldBeInCorrectFormatMessageText);
			invoiceLine.US_WoolLicenceNo = "1234GF";
			AssertHasMessageError(invoiceLine.US_WoolLicenceNoInfo, woolLicenseShouldBeInCorrectFormatMessageText);
			invoiceLine.US_WoolLicenceNo = "123456FD9";
			AssertHasMessageError(invoiceLine.US_WoolLicenceNoInfo, woolLicenseShouldBeInCorrectFormatMessageText);
			invoiceLine.US_WoolLicenceNo = "W23456FD9";
			AssertNoMessageError(invoiceLine.US_WoolLicenceNoInfo, woolLicenseShouldBeInCorrectFormatMessageText);
			invoiceLine.US_WoolLicenceNo = "WGTFRHJJK";
			AssertNoMessageError(invoiceLine.US_WoolLicenceNoInfo, woolLicenseShouldBeInCorrectFormatMessageText);
			invoiceLine.US_WoolLicenceNo = "W12345678";
			AssertNoMessageError(invoiceLine.US_WoolLicenceNoInfo, woolLicenseShouldBeInCorrectFormatMessageText);
			invoiceLine.US_WoolLicenceNo = "W1234";
			AssertHasMessageError(invoiceLine.US_WoolLicenceNoInfo, woolLicenseShouldBeInCorrectFormatMessageText);
			invoiceLine.US_WoolLicenceNo = "WDFGR";
			AssertHasMessageError(invoiceLine.US_WoolLicenceNoInfo, woolLicenseShouldBeInCorrectFormatMessageText);
			invoiceLine.US_SupTariff = "";
			invoiceLine.JI_Tariff = "88025111 00";
			invoiceLine.US_WoolLicenceNo = "";
			AssertNoMessageError(invoiceLine.US_WoolLicenceNoInfo, woolLicenseRequiredMessageText);
			AssertNoMessageError(invoiceLine.US_WoolLicenceNoInfo, WoolLicenseValidator.WoolLicenseShouldNotBeEnteredForTariff);
			invoiceLine.US_WoolLicenceNo = "W23456789";
			AssertNoMessageError(invoiceLine.US_WoolLicenceNoInfo, woolLicenseRequiredMessageText);
			AssertHasMessageError(invoiceLine.US_WoolLicenceNoInfo, WoolLicenseValidator.WoolLicenseShouldNotBeEnteredForTariff);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_WoolLicenceNo = "123456789";
			AssertHasMessageError(invoiceLine.US_WoolLicenceNoInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit));
		}

		public void TestCheckUS_APHISInd()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertUS_APHISInd();
			var errorMessage = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequiredIfNotApplicable, "APHIS", EntryTypeList.Codes.ReWarehouse);
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = true;
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_APHISIndInfo, errorMessage);
		}

		public void TestCheckUS_APHISDisclaimReason()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010999999";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now.AddYears(1);
			tariff.UE_PGACodes = "AQ1";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_APHISDisclaimReason = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_APHISDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_APHISDisclaimReason = "!";
			AssertHasMessageErrorContaining(invoiceLine.US_APHISDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoiceLine.US_APHISDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			var list = new PGADisclaimReasonList();
			foreach (var code in new[] { PGADisclaimReasonList.Codes.A, PGADisclaimReasonList.Codes.B, PGADisclaimReasonList.Codes.D })
			{
				list.RemoveCode(code);
				invoiceLine.US_APHISDisclaimReason = code;
				AssertNoMessageErrors(code, invoiceLine.US_APHISDisclaimReasonInfo);
			}

			foreach (ICodeDescription pair in list)
			{
				invoiceLine.US_APHISDisclaimReason = pair.Code;
				AssertHasMessageErrorContaining(invoiceLine.US_APHISDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageError(invoiceLine.US_APHISDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			}
		}

		public void TestCheckUS_NMFS370DisclaimReason()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010999999";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now.AddYears(1);
			tariff.UE_PGACodes = "NM1";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NMFS370DisclaimReason = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_NMFS370DisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_NMFS370DisclaimReason = "!";
			AssertHasMessageErrorContaining(invoiceLine.US_NMFS370DisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoiceLine.US_NMFS370DisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			var list = new PGADisclaimReasonList();
			foreach (var code in new[] { PGADisclaimReasonList.Codes.A, PGADisclaimReasonList.Codes.B })
			{
				list.RemoveCode(code);
				invoiceLine.US_NMFS370DisclaimReason = code;
				AssertNoMessageErrors(code, invoiceLine.US_NMFS370DisclaimReasonInfo);
			}

			foreach (ICodeDescription pair in list)
			{
				invoiceLine.US_NMFS370DisclaimReason = pair.Code;
				AssertHasMessageErrorContaining(invoiceLine.US_NMFS370DisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageError(invoiceLine.US_NMFS370DisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			}
		}

		public void TestCheckUS_NMFSAMRDisclaimReason()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010999999";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now.AddYears(1);
			tariff.UE_PGACodes = "NM3";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NMFSAMRDisclaimReason = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_NMFSAMRDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_NMFSAMRDisclaimReason = "!";
			AssertHasMessageErrorContaining(invoiceLine.US_NMFSAMRDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoiceLine.US_NMFSAMRDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			var list = new PGADisclaimReasonList();
			foreach (var code in new[] { PGADisclaimReasonList.Codes.A, PGADisclaimReasonList.Codes.B })
			{
				list.RemoveCode(code);
				invoiceLine.US_NMFSAMRDisclaimReason = code;
				AssertNoMessageErrors(code, invoiceLine.US_NMFSAMRDisclaimReasonInfo);
			}

			foreach (ICodeDescription pair in list)
			{
				invoiceLine.US_NMFSAMRDisclaimReason = pair.Code;
				AssertHasMessageErrorContaining(invoiceLine.US_NMFSAMRDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageError(invoiceLine.US_NMFSAMRDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			}
		}

		public void TestCheckUS_NMFSHMSDisclaimReason()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010999999";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now.AddYears(1);
			tariff.UE_PGACodes = "NM5";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NMFSHMSDisclaimReason = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_NMFSHMSDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_NMFSHMSDisclaimReason = "!";
			AssertHasMessageErrorContaining(invoiceLine.US_NMFSHMSDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoiceLine.US_NMFSHMSDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			var list = new PGADisclaimReasonList();
			foreach (var code in new[] { PGADisclaimReasonList.Codes.A, PGADisclaimReasonList.Codes.B })
			{
				list.RemoveCode(code);
				invoiceLine.US_NMFSHMSDisclaimReason = code;
				AssertNoMessageErrors(code, invoiceLine.US_NMFSHMSDisclaimReasonInfo);
			}

			foreach (ICodeDescription pair in list)
			{
				invoiceLine.US_NMFSHMSDisclaimReason = pair.Code;
				AssertHasMessageErrorContaining(invoiceLine.US_NMFSHMSDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageError(invoiceLine.US_NMFSHMSDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			}
		}

		public void TestCheckUS_DEAInd()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0407000020";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_PGACodes = "DE1";
			Factory.Save();
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.US_DEAInd = "~";
			AssertHasMessageError(invoiceLine.US_DEAIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			var errorText2 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "DEA");
			AssertHasMessageError(invoiceLine.US_DEAIndInfo, errorText2);
			var dEAHeader = invoiceLine.DEAHeaders.AddNew();
			invoiceLine.AddInfoValidation.ValidateUS_DEAInd();
			AssertNoMessageError(invoiceLine.US_DEAIndInfo, errorText2);
			var header2 = invoiceLine.DEAHeaders.AddNew();
			invoiceLine.AddInfoValidation.ValidateUS_DEAInd();
			AssertNoMessageError(invoiceLine.US_DEAIndInfo, errorText2);
			errorText2 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequiredIfNotApplicable, "DEA", EntryTypeList.Codes.Warehouse);
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = true;
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.AddInfoValidation.ValidateUS_DEAInd();
			AssertHasMessageErrorContaining(invoiceLine.US_DEAIndInfo, errorText2);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_DEAIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_DEAIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_DEAInd = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_DEAIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
		}

		public void TestCheckUS_CBTPACertificateNo()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._18, "Caribbean Basin Trade Partnership Act(CBTPA) Certification", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"[0-9]{1}CB[0-9]{6}");
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatErrorText, "character 1 & 3-9 must be numeric, characters 2-3 must be 'CB' - e.g. 9CB999999.");
			Factory.Save();

			var certificateNoRequiredMessageText = "The Caribbean Basin Trade Partnership Act(CBTPA) Certification number is required.";
			var ertificateNoIncorrectFormatMessageText = "The Caribbean Basin Trade Partnership Act(CBTPA) Certification number is invalid. The format should be character 1 & 3-9 must be numeric, characters 2-3 must be 'CB' - e.g. 9CB999999.";
			invoiceLine.US_SupTariff = "98201115";
			invoiceLine.US_CBTPACertificateNo = "";
			AssertHasMessageError(invoiceLine.US_CBTPACertificateNoInfo, certificateNoRequiredMessageText);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			invoiceLine.US_CBTPACertificateNo = "";
			AssertNoMessageError(invoiceLine.US_CBTPACertificateNoInfo, certificateNoRequiredMessageText);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			invoiceLine.US_CBTPACertificateNo = "";
			AssertNoMessageError(invoiceLine.US_CBTPACertificateNoInfo, certificateNoRequiredMessageText);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.US_CBTPACertificateNo = "";
			AssertHasMessageError(invoiceLine.US_CBTPACertificateNoInfo, certificateNoRequiredMessageText);
			invoiceLine.US_CBTPACertificateNo = "123456789";
			AssertHasMessageError(invoiceLine.US_CBTPACertificateNoInfo, ertificateNoIncorrectFormatMessageText);
			invoiceLine.US_CBTPACertificateNo = "1AB456789";
			AssertHasMessageError(invoiceLine.US_CBTPACertificateNoInfo, ertificateNoIncorrectFormatMessageText);
			invoiceLine.US_CBTPACertificateNo = "1CB45";
			AssertHasMessageError(invoiceLine.US_CBTPACertificateNoInfo, ertificateNoIncorrectFormatMessageText);
			invoiceLine.US_CBTPACertificateNo = "ACB123456";
			AssertHasMessageError(invoiceLine.US_CBTPACertificateNoInfo, ertificateNoIncorrectFormatMessageText);
			invoiceLine.US_CBTPACertificateNo = "1CB456789";
			AssertNoMessageError(invoiceLine.US_CBTPACertificateNoInfo, certificateNoRequiredMessageText);
			AssertNoMessageError(invoiceLine.US_CBTPACertificateNoInfo, ertificateNoIncorrectFormatMessageText);
			AssertNoMessageError(invoiceLine.US_CBTPACertificateNoInfo, CaribbeanBasinTradePartnershipActCertificateValidator.CBTPACertificateNoMayOnlyBeEnteredForTariff9820115);
			invoiceLine.US_SupTariff = "99201115 00";
			invoiceLine.US_CBTPACertificateNo = "1CB456789";
			AssertHasMessageError(invoiceLine.US_CBTPACertificateNoInfo, CaribbeanBasinTradePartnershipActCertificateValidator.CBTPACertificateNoMayOnlyBeEnteredForTariff9820115);
			invoiceLine.US_CBTPACertificateNo = "";
			AssertNoMessageError(invoiceLine.US_CBTPACertificateNoInfo, CaribbeanBasinTradePartnershipActCertificateValidator.CBTPACertificateNoMayOnlyBeEnteredForTariff9820115);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_CBTPACertificateNo = "ACB123456";
			AssertHasMessageError(invoiceLine.US_CBTPACertificateNoInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit));
		}

		public void TestMiscPermitNumberWithSupTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._01, "Steel Import License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.US_SupTariff = "9817009040";
			invoiceLine.JI_Tariff = "7222110050";
			invoiceLine.US_MiscPermitNo = "";
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, "The Steel Import License number is required.");
			invoiceLine.US_MiscPermitNo = "0LV123456";
			AssertNoMessageError(invoiceLine.US_MiscPermitNoInfo, "The Steel Import License number is required.");
			Assert(!invoiceLine.US_MiscPermitNoInfo.HasWarning(FormalImportAddInfoJobComInvoiceLineValidation.MiscPermitNoIsNotRequired));
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_MiscPermitNo = "0LV123456";
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit));
		}

		public void TestCheckUS_WHSEntryNumber()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PD3234";
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertEquals(true, declaration.IsOutwardBondedWarehousingEnabled);
			invoiceLine.US_WHSEntryNumber = "D";
			AssertNoMessageError(invoiceLine.US_WHSEntryNumberInfo, "A valid FTZ Admission Number is required for an FTZ withdrawal.");
			invoiceLine.US_WHSEntryNumber = "";
			AssertHasMessageError(invoiceLine.US_WHSEntryNumberInfo, "A valid FTZ Admission Number is required for an FTZ withdrawal.");
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.US_WHSEntryNumber = "";
			AssertNoMessageError(invoiceLine.US_WHSEntryNumberInfo, "A valid FTZ Admission Number is required for an FTZ withdrawal.");
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.AddInfoValidation.ValidateUS_WHSEntryNumber();
			AssertHasMessageError(invoiceLine.US_WHSEntryNumberInfo, "A valid FTZ Admission Number is required for an FTZ withdrawal.");
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			AssertHasMessageError(invoiceLine.US_WHSEntryNumberInfo, "A valid FTZ Admission Number is required for an FTZ withdrawal.");
			invoiceLine.JI_ParentID = invoiceLine2.PK;
			AssertNoMessageError(invoiceLine.US_WHSEntryNumberInfo, "A valid FTZ Admission Number is required for an FTZ withdrawal.");
			invoiceLine.JI_ParentID = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.US_WHSEntryNumberInfo, "A valid FTZ Admission Number is required for an FTZ withdrawal.");
			invoiceLine.US_JI_ParentProduct = invoiceLine2.PK;
			AssertNoMessageError(invoiceLine.US_WHSEntryNumberInfo, "A valid FTZ Admission Number is required for an FTZ withdrawal.");
			invoiceLine.US_JI_ParentProduct = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.US_WHSEntryNumberInfo, "A valid FTZ Admission Number is required for an FTZ withdrawal.");
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			invoiceLine.AddInfoValidation.ValidateUS_WHSEntryNumber();
			AssertNoMessageError(invoiceLine.US_WHSEntryNumberInfo, FormalImportAddInfoJobComInvoiceLineValidation.WarehouseEntryNoIsRequired);
		}

		public void TestCheckUS_WHSEntryLineNo()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PD3234";
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVD;
			AssertEquals(true, declaration.IsOutwardBondedWarehousingEnabled);
			invoiceLine.US_WHSEntryLineNo = 1;
			AssertNoMessageError(invoiceLine.US_WHSEntryLineNoInfo, "A valid Whs Line Number is required for a WHS withdrawal.");
			invoiceLine.US_WHSEntryLineNo = -1;
			AssertHasMessageError(invoiceLine.US_WHSEntryLineNoInfo, "A valid Whs Line Number is required for a WHS withdrawal.");
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.US_WHSEntryLineNo = -1;
			AssertNoMessageError(invoiceLine.US_WHSEntryLineNoInfo, "A valid Whs Line Number is required for a WHS withdrawal.");
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			invoiceLine.AddInfoValidation.ValidateUS_WHSEntryLineNo();
			AssertHasMessageError(invoiceLine.US_WHSEntryLineNoInfo, "A valid Whs Line Number is required for a WHS withdrawal.");
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			AssertHasMessageError(invoiceLine.US_WHSEntryLineNoInfo, "A valid Whs Line Number is required for a WHS withdrawal.");
			invoiceLine.JI_ParentID = invoiceLine2.PK;
			AssertNoMessageError(invoiceLine.US_WHSEntryLineNoInfo, "A valid Whs Line Number is required for a WHS withdrawal.");
			invoiceLine.JI_ParentID = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.US_WHSEntryLineNoInfo, "A valid Whs Line Number is required for a WHS withdrawal.");
			invoiceLine.US_JI_ParentProduct = invoiceLine2.PK;
			AssertNoMessageError(invoiceLine.US_WHSEntryLineNoInfo, "A valid Whs Line Number is required for a WHS withdrawal.");
			invoiceLine.US_JI_ParentProduct = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.US_WHSEntryLineNoInfo, "A valid Whs Line Number is required for a WHS withdrawal.");
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			invoiceLine.JI_ParentID = ZGuid.Empty;
			invoiceLine.AddInfoValidation.ValidateUS_WHSEntryLineNo();
			AssertHasMessageError(invoiceLine.US_WHSEntryLineNoInfo, "A valid Whs Line Number is required for a WHS withdrawal.");
			invoiceLine.US_WHSEntryLineNo = 1;
			AssertNoMessageError(invoiceLine.US_WHSEntryLineNoInfo, "A valid Whs Line Number is required for a WHS withdrawal.");
			invoiceLine.US_WHSEntryLineNo = -1;
			AssertHasMessageError(invoiceLine.US_WHSEntryLineNoInfo, "A valid Whs Line Number is required for a WHS withdrawal.");
			invoiceLine.JI_ParentID = invoiceLine2.PK;
			AssertNoMessageError(invoiceLine.US_WHSEntryLineNoInfo, "A valid Whs Line Number is required for a WHS withdrawal.");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_ParentID = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.US_WHSEntryLineNoInfo, "A valid FTZ Admission Line Number is required for an FTZ withdrawal.");
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			invoiceLine.AddInfoValidation.ValidateUS_WHSEntryLineNo();
			AssertNoMessageError(invoiceLine.US_WHSEntryLineNoInfo, "A valid FTZ Admission Line Number is required for an FTZ withdrawal.");
		}

		public void TestPGAIndicatorsOnXLineAPHISNMFS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_EntryType = "01";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "SET X Test";
			invoiceLine.US_SetInd = "";
			var errorAPHIS = string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, "APHIS");
			AssertNoMessageErrorContaining(invoiceLine.US_APHISIndInfo, errorAPHIS);
			var errorHMS = string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, "NMFS HMS");
			AssertNoMessageErrorContaining(invoiceLine.US_NMFSHMSIndInfo, errorHMS);
			var errorAMR = string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, "NMFS AMR");
			AssertNoMessageErrorContaining(invoiceLine.US_NMFSAMRIndInfo, errorAMR);
			var error370 = string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, "NMFS 370");
			AssertNoMessageErrorContaining(invoiceLine.US_NMFS370IndInfo, error370);
			invoiceLine.US_SetInd = "X";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_APHISIndInfo, errorAPHIS);
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_NMFSHMSIndInfo, errorHMS);
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_NMFSAMRIndInfo, errorAMR);
			invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_NMFS370IndInfo, error370);
		}

		[TestDate(2009, 12, 12)]
		public void TestUS_UC_NKCountryOfOriginForBorderCargoRelease()
		{
			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			declaration.ValidationModes = ValidationModes.CargoRelease;
			AssertOnCountryInRelationToSpecialTradeAgreement(invoiceLine.US_UC_NKCountryOfOriginInfo);
		}

		[TestDate(2009, 12, 12)]
		public void TestUS_UC_NKCountryOfOrigin()
		{
			AssertEquals("IsCargoReleaseValidationMode", true, declaration.IsCargoReleaseValidationMode);
		}

		public void TestCountryOfExportAndOriginShouldBeSameForCBTPA()
		{
			AssertCountryOfExportAndOriginShouldBeSame(USCTariff.CBTPABenefitsApplicable);
		}

		public void TestTIBEntryTariff()
		{
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			invoiceLine.US_SupTariff = "6203322030";
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.TIBEntryMustUseChapter9813Tariffs);
			invoiceLine.US_SupTariff = "9813000540";
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.TIBEntryMustUseChapter9813Tariffs);
			invoiceLine.JI_Tariff = "6203322030";
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.TIBEntryMustUseChapter9813Tariffs);
			declaration.US_EntryType = EntryTypeList.Codes.Appraisement;
			invoiceLine.US_SupTariff = "9813000540";
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.EntryTypeShouldBeTIB);
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.EntryTypeShouldBeTIB);
			var childLine = invoice.JobComInvoiceLines.AddNew();
			childLine.JI_ParentID = invoiceLine.PK;
			childLine.US_SupTariff = "9813000540";
			AssertHasMessageError(childLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.No9813TariffsOnSecondaryLines);
			childLine.US_SupTariff = "9813000000";
			AssertNoMessageError(childLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.No9813TariffsOnSecondaryLines);
			declaration.US_EntryType = EntryTypeList.Codes.Appraisement;
			childLine.US_SupTariff = "9813000540";
			AssertNoMessageError(childLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.No9813TariffsOnSecondaryLines);
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			childLine.JI_ParentID = ZGuid.Empty;
			childLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(childLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.No9813TariffsOnSecondaryLines);
		}

		public void Test981800Tariffs()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			invoiceLine.US_SupTariff = "9818000500";
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.EntryTypeMustBe05);
			declaration.US_EntryType = EntryTypeList.Codes.VesselRepair;
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.EntryTypeMustBe05);
		}

		public void TestValidateForTariffAgainstFormalEntryRequirement()
		{
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "99990054";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now;
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.RequiresFormalEntryRegardlessOfValue;
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_Tariff = "99990054";
			invoiceLine.JI_Tariff = "3926.90.55 00";
			invoiceLine.US_SupTariff = "99990054";
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, TariffValidator.FormalEntryRequired);
			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9802105111";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now;
			invoiceLine.US_SupTariff = "9802105111";
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, TariffValidator.FormalEntryRequired);
		}

		public void TestSupTariffIsUsedOnlyWhenTariffRequiresSecondaryLine()
		{
			invoiceLine.US_SupTariff = "9802002000";
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, TariffValidator.NoSecondaryTariffNumberAllowed);
			invoiceLine.US_SupTariff = "9802004040";
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, TariffValidator.NoSecondaryTariffNumberAllowed);
			invoiceLine.US_SupTariff = USCTariff.LumberPermitApplicable;
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, TariffValidator.TariffNumber98_99ShouldBeEnteredHere);
			invoiceLine.US_SupTariff = "9802004040";
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, TariffValidator.TariffNumber98_99ShouldBeEnteredHere);
		}

		[TestDate(2009, 1, 1)]
		public void TestValidateNegativeCustomsQuantities()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "99990054";
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "KG";
			tariff.UE_Unit3 = "KG";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now;
			declaration.US_EnableAII = true;
			invoiceLine.US_SupTariff = "99990054";
			invoiceLine.US_SupQty1 = -1m;
			AssertHasMessageError(invoiceLine.US_SupQty1Info, ValidationConstants.NegativeAmountNotAllowed);
			invoiceLine.US_SupQty2 = -1m;
			AssertHasMessageError(invoiceLine.US_SupQty2Info, ValidationConstants.NegativeAmountNotAllowed);
			invoiceLine.US_SupQty3 = -1m;
			AssertHasMessageError(invoiceLine.US_SupQty3Info, ValidationConstants.NegativeAmountNotAllowed);
			invoiceLine.US_SupQty1 = 1m;
			AssertNoMessageError(invoiceLine.US_SupQty1Info, ValidationConstants.NegativeAmountNotAllowed);
			invoiceLine.US_SupQty2 = 1m;
			AssertNoMessageError(invoiceLine.US_SupQty2Info, ValidationConstants.NegativeAmountNotAllowed);
			invoiceLine.US_SupQty3 = 1m;
			AssertNoMessageError(invoiceLine.US_SupQty3Info, ValidationConstants.NegativeAmountNotAllowed);
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

		void AssertFirstCharacterOnAGOA(JobComInvoiceLine invoiceLine, ZString tariffNumber, ZString wrongVisaNumber, ZString rightVisaNumber)
		{
			invoiceLine.US_SupTariff = tariffNumber;
			invoiceLine.US_VisaNo = wrongVisaNumber;
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.FirstCharacterVisaNumberShouldStartWith + rightVisaNumber.Left(1));
			invoiceLine.US_VisaNo = rightVisaNumber;
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.FirstCharacterVisaNumberShouldStartWith + rightVisaNumber.Left(1));
		}

		void AssertUS_APHISInd()
		{
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
			var invalidCodeMessageError = "The code you have selected is not in the list.";
			AssertNoMessageError(invoiceLine.US_APHISIndInfo, invalidCodeMessageError);
			invoiceLine.US_APHISInd = "$";
			AssertHasMessageError(invoiceLine.US_APHISIndInfo, invalidCodeMessageError);
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_APHISIndInfo, invalidCodeMessageError);
			var tariff = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "0102294082")) ?? Factory.New<USCTariff>();
			tariff.UE_Tariff = "0102294082";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now.AddYears(1);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			var warning = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "APHIS", "Declared");
			var pGARequiredButBlank = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "APHIS");
			var pGALineRequired = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "APHIS");
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(invoiceLine.US_APHISIndInfo, warning);
			Assert(!invoiceLine.US_APHISIndInfo.HasMessageError(pGARequiredButBlank));
			AssertHasMessageError(invoiceLine.US_APHISIndInfo, pGALineRequired);
			tariff.UE_PGACodes = "AQ2";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			AssertNoWarning(invoiceLine.US_APHISIndInfo, warning);
			Assert(!invoiceLine.US_APHISIndInfo.HasMessageError(pGARequiredButBlank));
			AssertHasMessageError(invoiceLine.US_APHISIndInfo, pGALineRequired);
			var aphisHeader = invoiceLine.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			AssertNoWarning(invoiceLine.US_APHISIndInfo, warning);
			Assert(!invoiceLine.US_APHISIndInfo.HasMessageError(pGARequiredButBlank));
			AssertNoMessageError(invoiceLine.US_APHISIndInfo, pGALineRequired);
			invoiceLine.US_APHISInd = ZString.Empty;
			AssertNoWarning(invoiceLine.US_APHISIndInfo, warning);
			AssertNoMessageError(invoiceLine.US_APHISIndInfo, pGALineRequired);
			aphisHeader.Delete();
			tariff.UE_PGACodes = "AQX";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			AssertNoWarning(invoiceLine.US_APHISIndInfo, FormalImportAddInfoJobComInvoiceLineValidation.APHISDataMayBeRequired);
			AssertNoWarnings(invoiceLine.US_APHISIndInfo);
			invoiceLine.US_APHISInd = ZString.Empty;
			AssertHasWarning(invoiceLine.US_APHISIndInfo, FormalImportAddInfoJobComInvoiceLineValidation.APHISDataMayBeRequired);
			tariff.UE_PGACodes = "";
		}

		void AssertOnCountryInRelationToSpecialTradeAgreement(ZPropertyInfo countryInfo)
		{
			invoiceLine.US_SupTariff = "9802008042";
			AssertNotNull("PreCondition:Import Sup tariff should not be null", invoiceLine.ImportSupTariff);
			AssertEquals("IsEligibleForAGOATextile", true, invoiceLine.ImportSupTariff.IsEligibleForAGOATextileBenefits(invoiceLine.EffectiveDateForDutyRate));
			countryInfo.Value = (ZString)"RU";
			AssertHasMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoAGOACountry);
			countryInfo.Value = (ZString)"ZA";
			AssertNoMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoAGOACountry);
			invoiceLine.US_SupTariff = "9802008044";
			AssertNotNull("PreCondition:Import Sup tariff should not be null", invoiceLine.ImportSupTariff);
			AssertEquals("IsEligibleForAGOATextile", true, invoiceLine.ImportSupTariff.IsEligibleForCBTPATextileBenefits(invoiceLine.EffectiveDateForDutyRate));
			countryInfo.Value = (ZString)"ZA";
			AssertHasMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoCBTPACountry);
			countryInfo.Value = (ZString)"JM";
			AssertNoMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoCBTPACountry);
			invoiceLine.US_SupTariff = "9802008048";
			AssertNotNull("PreCondition:Import Sup tariff should not be null", invoiceLine.ImportSupTariff);
			AssertEquals("IsEligibleForATPDEA", true, invoiceLine.ImportSupTariff.IsEligibleForATPDEATextileAndTunaClaims(invoiceLine.EffectiveDateForDutyRate));
			countryInfo.Value = (ZString)"JM";
			AssertHasMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoATPDEACountry);
			countryInfo.Value = (ZString)"EC";
			AssertNoMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoATPDEACountry);
			invoiceLine.US_SupTariff = USCTariff.CAFTABenefitsApplicable;
			AssertNotNull("PreCondition:Import Sup tariff should not be null", invoiceLine.ImportSupTariff);
			AssertEquals("IsEligibleForATPDEA", true, invoiceLine.ImportSupTariff.IsEligibleForCAFTAClaims(invoiceLine.EffectiveDateForDutyRate));
			countryInfo.Value = (ZString)"EC";
			AssertHasMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoCAFTACountry);
			countryInfo.Value = (ZString)Core.Constants.CountryCodes.ElSalvador;
			AssertNoMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoCAFTACountry);
		}

		void AssertCountryOfExportAndOriginShouldBeSame(ZString tariffNumber)
		{
			invoiceLine.US_SupTariff = tariffNumber;
			invoiceLine.US_UC_NKCountryOfExport = "";
			invoiceLine.InvoiceHeader.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
			invoiceLine.US_UC_NKCountryOfExport = "NZ";
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
		}
	}
}
