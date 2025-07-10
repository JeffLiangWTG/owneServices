using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class FormalImportJobComInvoiceLineValidationTest : CommonImportJobComInvoiceLineValidationTest
	{
		public void TestFlags()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			FormalImportJobComInvoiceLineValidation validation = (FormalImportJobComInvoiceLineValidation)invoiceLine.Validation;

			Bill bill = declaration.Bills.AddNew();

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = false;
			AssertEquals(true, validation.IsEntrySummaryValidationMode);
			AssertEquals(false, validation.IsCargoReleaseValidationMode);

			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_EnableENS = false;
			declaration.US_EnableINB = true;
			AssertEquals(false, validation.IsEntrySummaryValidationMode);
			AssertEquals(false, validation.IsCargoReleaseValidationMode);

			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.Yes;
			AssertEquals(false, validation.IsEntrySummaryValidationMode);
			AssertEquals(false, validation.IsCargoReleaseValidationMode);

			declaration.US_EnableCRL = true;
			AssertEquals(false, validation.IsEntrySummaryValidationMode);
			AssertEquals(true, validation.IsCargoReleaseValidationMode);
		}

		public void TestIsEntrySummaryOrCargoReleaseValidationMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var validation = new FormalImportJobComInvoiceLineValidationForTest(invoiceLine);

			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, validation.IsTariffMandatory);

			declaration.US_EnableENS = true;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, validation.IsTariffMandatory);

			declaration.US_EnableCRL = true;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, validation.IsTariffMandatory);

			declaration.US_EnableENS = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, validation.IsTariffMandatory);

			declaration.US_EnableCRL = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, validation.IsTariffMandatory);

			declaration.US_CertifyCargoRelease = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, validation.IsTariffMandatory);
		}

		public void TestCheckJI_CustomsSecondQuantity()
		{
			declaration.US_EnableAII = true;

			AssertJI_CustomsSecondQuantity(invoiceLine);
		}
		public void TestCheckJI_CustomsThirdQuantity()
		{
			declaration.US_EnableAII = true;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_Unit3 = "LTR";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now;

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsThirdQuantity = 0;
			AssertHasMessageError(invoiceLine.JI_CustomsThirdQuantityInfo, ValidationConstants.StatQTYRequired);

			var childInvoiceLine = invoice.InvoiceLines.AddNew();
			childInvoiceLine.JI_ParentID = invoiceLine.PK;
			childInvoiceLine.JI_Tariff = tariff.UE_Tariff;
			childInvoiceLine.Validation.ValidateJI_CustomsThirdQuantity();
			AssertHasWarning(childInvoiceLine.JI_CustomsThirdQuantityInfo, ValidationConstants.WarnIfStatQTYisZero);

			invoiceLine.JI_CustomsThirdQuantity = -1m;
			AssertHasMessageError(invoiceLine.JI_CustomsThirdQuantityInfo, ValidationConstants.NegativeAmountNotAllowed);
		}

		public void TestDutyFreeUnderConsolidatedMonthlyFiling()
		{
			invoiceLine.Declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			invoiceLine.Declaration.US_MonthlyFiling = true;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2523900001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.USDASugarCertificate;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "2523900002";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff2.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.USDASugarCertificate;
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.Free;

			Factory.Save();

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.ConsolidatedMonthlyFilingOnlyForUnconditionallyDutyFreeGoods);

			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.ConsolidatedMonthlyFilingOnlyForUnconditionallyDutyFreeGoods);
		}

		public void TestTariffWithPermitType19RequiredAGOATextileClaims()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "98191103";
			tariff.UE_DateFrom = ZDateTime.Now.AddYears(-2);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(2);

			var tariffRule2 = Factory.New<USCTariffRule>();
			tariffRule2.U1_Tariff = "98191103";
			tariffRule2.U1_DateFrom = ZDateTime.Now.AddYears(-1);
			tariffRule2.U1_DateTo = ZDateTime.Now.AddYears(1);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			AssertEquals("PreCondition:EligibleForAGOA", true, invoiceLine.ImportSupTariff.IsEligibleForAGOATextileBenefits(invoiceLine.EffectiveDateForDutyRate));

			AssertNoMessageError(invoiceLine.US_VisaNoInfo, LicenceValidator.RequiredForAGOATextileClaims);
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, LicenceValidator.RequiredForAGOATextileClaims);

			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = LicencePermitTypeList.Codes._19;
			permit.CY_Data = "1";
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, LicenceValidator.RequiredForAGOATextileClaims);
		}

		public void TestLicencePermitRequirements()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._21, "USDA Sugar Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2523900001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.USDASugarCertificate;

			Factory.Save();

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			var messageText = "The USDA Sugar Certificate number is required.";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, messageText);

			invoiceLine.LicenceAndPermits.AddNew("21", "78978");
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError(invoiceLine.JI_TariffInfo, messageText);

			invoiceLine.LicenceAndPermits.RemoveAndDeleteAll();
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Factory.Save();
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError(invoiceLine.JI_TariffInfo, messageText);

			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.JI_Tariff = ZString.Empty;

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9800000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_PermitLicenseIndicator = "21";
			tariff.UE_AdditionalTariffNumberIndicator = true;

			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, messageText);

			invoiceLine.LicenceAndPermits.AddNew("21", "78978");
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, messageText);

			invoiceLine.LicenceAndPermits.RemoveAndDeleteAll();
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, messageText);
		}

		public void TestLicencePermitRequirementsForMXCement()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._09, "Mexican Cement Import License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2523900001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.MexicanCementImportLicense;

			Factory.Save();

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Japan;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			var messageText = "The Mexican Cement Import License number is required.";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, messageText);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine.JI_TariffInfo, messageText);

			invoiceLine.LicenceAndPermits.AddNew(MiscellaneousPermitLicenseList.Codes.MexicanCementImportLicense, "78978");
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError(invoiceLine.JI_TariffInfo, messageText);

			invoiceLine.LicenceAndPermits.RemoveAndDeleteAll();
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError(invoiceLine.JI_TariffInfo, messageText);

			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.JI_Tariff = ZString.Empty;

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9800000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.MexicanCementImportLicense;
			tariff.UE_AdditionalTariffNumberIndicator = true;

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Moldova;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, messageText);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, messageText);

			invoiceLine.LicenceAndPermits.AddNew(MiscellaneousPermitLicenseList.Codes.MexicanCementImportLicense, "78978");
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, messageText);

			invoiceLine.LicenceAndPermits.RemoveAndDeleteAll();
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, messageText);
		}

		public void TestWhenSecondaryTariffIsRequired()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.EligibleForSecondaryTariffNumbers;
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_Tariff = "0000000000";

			USCRuleSecondaryTariff secondaryTariff = tariffRule.SecondaryTariffs.AddNew();
			secondaryTariff.U3_TariffFrom = "0000000010";
			secondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday;

			secondaryTariff = tariffRule.SecondaryTariffs.AddNew();
			secondaryTariff.U3_TariffFrom = "0000000020";
			secondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_AdditionalTariffNumberIndicator = true;

			Factory.Save();
			invoiceLine.JI_Tariff = USCTariff.CBTPABenefitsApplicable;
			AssertNoMessageError(invoiceLine.JI_TariffInfo, TariffValidator.TariffRequiresAdditionalTariff);

			invoiceLine.JI_Tariff = "0000000000";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, TariffValidator.TariffRequiresAdditionalTariff);

			invoiceLine.AddSecondaryInvoiceLine();
			AssertNoMessageError(invoiceLine.JI_TariffInfo, TariffValidator.TariffRequiresAdditionalTariff);
		}

		public void TestRightNumberOfSecondaryLinesAreThere()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.EligibleForSecondaryTariffNumbers;
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_Tariff = "0000000000";

			USCRuleSecondaryTariff secondaryTariff = tariffRule.SecondaryTariffs.AddNew();
			secondaryTariff.U3_TariffFrom = "0000000010";
			secondaryTariff.U3_Tariff2 = "0000000020";
			secondaryTariff.U3_Tariff3 = "0000000030";
			secondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000010";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000020";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000030";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			Factory.Save();

			invoiceLine.JI_Tariff = "0000000000";

			AssertEquals("PreCondition", 3, invoiceLine.SecondaryTariffLines.Count());
			Assert(!invoiceLine.JI_TariffInfo.HasMessageError(string.Format(ValidationConstants.InvoiceLine.ImportTariff.EnteredSecondaryTariffsDoNotMatchRule, 3, 2)));

			invoiceLine.SecondaryTariffLines.ElementAt(0).Delete();
			AssertHasMessageError(invoiceLine.JI_TariffInfo, string.Format(ValidationConstants.InvoiceLine.ImportTariff.EnteredSecondaryTariffsDoNotMatchRule, 3, 2));

			invoiceLine.SecondaryTariffLines.ElementAt(0).JI_ParentID = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.JI_TariffInfo, string.Format(ValidationConstants.InvoiceLine.ImportTariff.EnteredSecondaryTariffsDoNotMatchRule, 3, 1));
		}

		public void TestRefreshValidationOnWeightWhenParentIDChanges()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			invoiceLine2.JI_Weight = 0m;
			invoiceLine2.JI_WeightUQ = ZString.Empty;

			AssertNoMessageErrors(invoiceLine2.JI_WeightInfo);
			AssertNoMessageErrors(invoiceLine2.JI_WeightUQInfo);

			invoiceLine2.JI_WeightUQ = "DD";
			AssertHasMessageError(invoiceLine2.JI_WeightUQInfo, JobComInvoiceLineValidation.WeightUQShouldBeInList);

			invoiceLine2.JI_WeightUQ = "KG";
			AssertNoMessageError(invoiceLine2.JI_WeightUQInfo, JobComInvoiceLineValidation.WeightUQShouldBeInList);
		}

		public void TestCheckForEntryDateRestriction()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000111100";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;

			tariff.TariffDateRestrictions.CreateWithRestrictionDatesIfNotExist("1", 101, 315);
			tariff.TariffDateRestrictions.CreateWithRestrictionDatesIfNotExist("2", 1101, 115);

			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 6, 1);
			invoiceLine.JI_Tariff = "0000111100";
			AssertNotNull("tariff is found", invoiceLine.ImportTariff);
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, TariffValidator.EntryDateRestrictionMessage);

			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 12, 1);
			invoiceLine.JI_Tariff = "0000111100";
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, TariffValidator.EntryDateRestrictionMessage);
		}

		public void TestValidateAllFeesWithSpecificSpecificRateCalculationHaveBeenAddedSupplimentary()
		{
			AsertFeesWithSpecificSpecificRateCalculation(invoiceLine.US_SupTariffInfo);
		}

		void AsertFeesWithSpecificSpecificRateCalculation(ZPropertyInfo tariffInfo)
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(+10);
			tariffInfo.Value = tariff.UE_Tariff;
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageError(invoiceLine, FormalImportJobComInvoiceLineValidation.FeesWithSpecificSpecificRateRequired + Core.Constants.USCustoms.FeeCodes.Wines);
			USCTariffDutyRate dutyRate1 = tariff.DutyRates.AddNew();
			dutyRate1.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate1.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			dutyRate1.UD_TaxFeeFlag = "1";
			USCTariffDutyRate dutyRate2 = tariff.DutyRates.AddNew();
			dutyRate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Tobacco;
			dutyRate2.UD_TaxFeeComputationCode = "";
			dutyRate2.UD_TaxFeeFlag = "1";
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageError(invoiceLine, FormalImportJobComInvoiceLineValidation.FeesWithSpecificSpecificRateRequired + Core.Constants.USCustoms.FeeCodes.Wines);
			dutyRate2.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, FormalImportJobComInvoiceLineValidation.FeesWithSpecificSpecificRateRequired);
			AssertHasRowMessageErrorContaining(invoiceLine, Core.Constants.USCustoms.FeeCodes.Wines);
			AssertHasRowMessageErrorContaining(invoiceLine, Core.Constants.USCustoms.FeeCodes.Tobacco);

			dutyRate2.UD_TaxFeeFlag = "2";
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, FormalImportJobComInvoiceLineValidation.FeesWithSpecificSpecificRateRequired);
			AssertHasRowMessageErrorContaining(invoiceLine, Core.Constants.USCustoms.FeeCodes.Wines);
			AssertNoRowMessageErrorContaining(invoiceLine, Core.Constants.USCustoms.FeeCodes.Tobacco);

			dutyRate2.UD_TaxFeeFlag = "1";
			FeeCusCodeData fee = invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Wines);
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, FormalImportJobComInvoiceLineValidation.FeesWithSpecificSpecificRateRequired);
			AssertNoRowMessageErrorContaining(invoiceLine, Core.Constants.USCustoms.FeeCodes.Wines);
			AssertHasRowMessageErrorContaining(invoiceLine, Core.Constants.USCustoms.FeeCodes.Tobacco);
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.Tobacco;
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, FormalImportJobComInvoiceLineValidation.FeesWithSpecificSpecificRateRequired);
			AssertHasRowMessageErrorContaining(invoiceLine, Core.Constants.USCustoms.FeeCodes.Wines);
			AssertNoRowMessageErrorContaining(invoiceLine, Core.Constants.USCustoms.FeeCodes.Tobacco);

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4915", "Test PR Port", startDate, endDate);
			var attributeNameState = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.State, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeState = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameState.ZXE_Name, USStateList.Codes.PuertoRico);
			newFactory.Save();

			declaration.US_SchDEntry = "4915";
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, FormalImportJobComInvoiceLineValidation.FeesWithSpecificSpecificRateRequired);
		}

		public void TestDerivedDutyTariff()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.Free;

			invoiceLine.JI_Tariff = "0000.00.0000";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, TariffValidator.TariffRequiresSecondaryTariffForDutyCalculation);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.Derived;
			invoiceLine.JI_Tariff = "0000.00.0000";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, TariffValidator.TariffRequiresSecondaryTariffForDutyCalculation);

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			AssertNoMessageError(invoiceLine.JI_TariffInfo, TariffValidator.TariffRequiresSecondaryTariffForDutyCalculation);
		}

		public void TestJI_DescriptionMandatoryWhenPGAIsIndicated()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_CertifyCargoRelease = true;

			invoiceLine.US_PSTIndicator = "D";
			invoiceLine.JI_Description = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Description = "TEST";
			AssertNoMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestJI_Weight()
		{
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			invoiceLine.JI_Weight = -1m;
			AssertHasErrors(invoiceLine.JI_WeightInfo);

			invoiceLine.JI_Weight = 1m;
			AssertNoMessageErrors(invoiceLine.JI_WeightInfo);

			invoiceLine.JI_NetWeight = 20000m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Milligrams;

			invoiceLine.JI_Weight = 20m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Grams;

			AssertNoMessageError("JI_NetWeight must be less then JI_Weight.", invoiceLine.JI_WeightInfo, FormalImportJobComInvoiceLineValidation.NetWeightBiggerThenGrossWeight);

			invoiceLine.JI_Weight = 19m;
			AssertHasMessageError("JI_NetWeight must be bigger then JI_Weight.", invoiceLine.JI_WeightInfo, FormalImportJobComInvoiceLineValidation.NetWeightBiggerThenGrossWeight);

			invoiceLine.JI_Weight = 20m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			AssertHasMessageError("JI_NetWeight must be bigger then JI_Weight.", invoiceLine.JI_WeightInfo, FormalImportJobComInvoiceLineValidation.NetWeightBiggerThenGrossWeight);

			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoMessageError("JI_NetWeight must be less then JI_Weight.", invoiceLine.JI_WeightInfo, FormalImportJobComInvoiceLineValidation.NetWeightBiggerThenGrossWeight);

			invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.JI_Weight = ZDecimal.Zero;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, FormalImportJobComInvoiceLineValidation.WeightIsRequired);

			invoiceLine.JI_CustomsQuantity = 20m;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, FormalImportJobComInvoiceLineValidation.WeightIsRequired);

			invoiceLine.JI_LinePrice = 200m;
			AssertHasMessageError(invoiceLine.JI_WeightInfo, FormalImportJobComInvoiceLineValidation.WeightIsRequired);

			invoiceLine.JI_Weight = 20m;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, FormalImportJobComInvoiceLineValidation.WeightIsRequired);

			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			invoiceLine.JI_LinePrice = 200m;
			invoiceLine.JI_Weight = 0m;
			AssertNoMessageError(invoiceLine.JI_WeightInfo, FormalImportJobComInvoiceLineValidation.WeightIsRequired);
		}

		public void TestInvalidTariff()
		{
			invoiceLine.US_SupTariff = "99";
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, TariffValidator.InvalidLength);

			invoiceLine.US_SupTariff = "99121212";
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, TariffValidator.InvalidLength);

			invoiceLine.US_SupTariff = "9912121212";
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, TariffValidator.InvalidLength);

			invoiceLine.JI_Tariff = "";
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, TariffValidator.InvalidLength);

			invoiceLine.JI_Tariff = "0101010101";
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, TariffValidator.InvalidLength);

			invoiceLine.JI_Tariff = "01010101";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, TariffValidator.InvalidLength);

			invoiceLine.JI_Tariff = "0901.10.0019";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff unable to be found");

			invoiceLine.JI_Tariff = "0101.10.0010";
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff unable to be found");
		}

		[TestDate(2007, 5, 18)]
		public void TestOutOfDateRangeTariff()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "99999999";
			tariff.UE_DateFrom = new ZDate(2000, 1, 1);
			tariff.UE_DateTo = new ZDate(2000, 12, 31);
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff was found but is not valid for 18-May-07");
			invoiceLine.US_SupTariff = "";
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, "Tariff was found but is not valid for 18-May-07");
		}

		public virtual void TestCheckJI_TariffForLaceyAct()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var rule = Factory.New<USCRule>();
			rule.U0_Code = TariffRuleList.Codes.LaceyAct;

			var tariff_rule = Factory.New<USCTariffRule>();
			tariff_rule.U1_RuleCode = TariffRuleList.Codes.LaceyAct;
			tariff_rule.U1_Tariff = "4406";
			tariff_rule.U1_DateFrom = new ZDate(2000, 1, 1);
			tariff_rule.U1_DateTo = ZDateTime.Today.AddYears(1);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "4406";
			tariff.UE_DateFrom = new ZDate(2000, 1, 1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertHasWarning(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.LaceyActDataMayBeRequired);

			invoiceLine.LaceyActLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertNoWarning(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.LaceyActDataMayBeRequired);

			invoiceLine.LaceyActLines.RemoveAndDeleteAll();
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertNoWarning(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.LaceyActDataMayBeRequired);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.US_SecondarySPI = ZString.Empty;
			invoiceLine.US_SetInd = ZString.Empty;
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarning(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.LaceyActDataMayBeRequired);
		}

		public void TestCheckJI_TariffForLaceyAct_CS00177517()
		{
			var rule = Factory.LoadTop1<USCRule>(new ZQuery(USCRuleSchema.U0_Code, TariffRuleList.Codes.LaceyAct));
			if (rule == null)
			{
				rule = Factory.New<USCRule>();
				rule.U0_Code = TariffRuleList.Codes.LaceyAct;
			}

			var tariff_rule = Factory.New<USCTariffRule>();
			tariff_rule.U1_RuleCode = TariffRuleList.Codes.LaceyAct;
			tariff_rule.U1_Tariff = "4406";
			tariff_rule.U1_DateFrom = new ZDate(2000, 1, 1);

			var tariff_rule2 = Factory.New<USCTariffRule>();
			tariff_rule2.U1_RuleCode = TariffRuleList.Codes.LaceyAct;
			tariff_rule2.U1_Tariff = "4412991020";
			tariff_rule2.U1_TariffTo = "44129951";
			tariff_rule2.U1_DateFrom = new ZDate(2000, 1, 1);

			var tariff = new USCTariff.Loader(Factory).LoadBestMatch("4412995100", ZDateTime.Today);
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "4412995100";
				tariff.UE_DateFrom = new ZDate(2000, 1, 1);
				tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			}

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertHasWarning(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.LaceyActDataMayBeRequired);

			invoiceLine.LaceyActLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertNoWarning(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.LaceyActDataMayBeRequired);

			invoiceLine.LaceyActLines.RemoveAndDeleteAll();
			tariff = new USCTariff.Loader(Factory).LoadBestMatch("4412995130", ZDateTime.Today);
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "4412995130";
				tariff.UE_DateFrom = new ZDate(2000, 1, 1);
				tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			}

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertHasWarning(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.LaceyActDataMayBeRequired);
		}

		public void TestCheckJI_TariffIsValidForSTN()
		{
			invoiceLine.US_SupTariff = "99990084";
			invoiceLine.JI_Tariff = "1902192020";
			invoiceLine.US_SupTariff = "99990084";

			AssertHasMessageError(invoiceLine.US_SupTariffInfo, ValidationConstants.InvoiceLine.ImportTariff.ChildLineTariffNotFoundInRuleTariffs);

			invoiceLine.JI_Tariff = "85189020";
			invoiceLine.US_SupTariff = "99990084";
			AssertNoMessageError(invoiceLine.US_SupTariffInfo, ValidationConstants.InvoiceLine.ImportTariff.ChildLineTariffNotFoundInRuleTariffs);

			invoiceLine.US_SupTariff = "";
			invoiceLine.JI_Tariff = "1902192020";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, ValidationConstants.InvoiceLine.ImportTariff.ChildLineTariffNotFoundInRuleTariffs);

			var tariff = new USCTariff.Loader(Factory).LoadBestMatch("99119700", ZDateTime.Today);
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "99119700";
				tariff.UE_DateFrom = new ZDate(2015, 1, 1);
				tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			}
			invoiceLine.JI_Tariff = "2008979030";
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			AssertNoMessageError(invoiceLine.JI_TariffInfo, ValidationConstants.InvoiceLine.ImportTariff.ChildLineTariffNotFoundInRuleTariffs);

			tariff = new USCTariff.Loader(Factory).LoadBestMatch("98178401", ZDateTime.Today);
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "98178401";
				tariff.UE_DateFrom = new ZDate(2015, 1, 1);
				tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			}

			invoiceLine.JI_Tariff = "84798994";
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			AssertNoMessageError(invoiceLine.JI_TariffInfo, ValidationConstants.InvoiceLine.ImportTariff.ChildLineTariffNotFoundInRuleTariffs);

			invoiceLine.JI_Tariff = "84798998";
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			AssertHasMessageError(invoiceLine.US_SupTariffInfo, ValidationConstants.InvoiceLine.ImportTariff.ChildLineTariffNotFoundInRuleTariffs);

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102124000", "7", 0m, "NO");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, "");

			invoiceLine.JI_Tariff = "9102124000";
			invoiceLine.US_SupTariff = "99030124";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError(invoiceLine.JI_TariffInfo, ValidationConstants.InvoiceLine.ImportTariff.ChildLineTariffNotFoundInRuleTariffs);
		}

		[TestDate(2009, 2, 17)]
		public void TestValueShouldBeDeclaredAtSecondaryForDerived()
		{
			invoiceLine.JI_Tariff = "6104220040";
			AssertEquals("PreCondition:ValueToBeDeclaredAtSecondary", true, invoiceLine.ImportTariff.IsValueToBeDeclaredInAlternateTariff(ZDateTime.Today));
			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();

			invoiceLine.JI_LinePrice = 10000m;
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, ValidationConstants.InvoiceLine.ValueShouldBeDeclaredUnderSecondaryLines);

			secondaryLine.JI_LinePrice = 0m;
			AssertHasMessageError(secondaryLine.JI_LinePriceInfo, ValidationConstants.InvoiceLine.ValueShouldBeDeclaredUnderSecondaryLines);

			invoiceLine.JI_LinePrice = 0m;
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, ValidationConstants.InvoiceLine.ValueShouldBeDeclaredUnderSecondaryLines);
			secondaryLine.JI_LinePrice = 10000m;
			AssertNoMessageError(secondaryLine.JI_LinePriceInfo, ValidationConstants.InvoiceLine.ValueShouldBeDeclaredUnderSecondaryLines);

			invoiceLine.JI_Tariff = "8536908085";
			AssertEquals("PreCondition:ValueToBeDeclaredAtSecondary", false, invoiceLine.ImportTariff.IsValueToBeDeclaredInAlternateTariff(ZDateTime.Today));
			invoiceLine.JI_LinePrice = 10000m;
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, ValidationConstants.InvoiceLine.ValueShouldBeDeclaredUnderSecondaryLines);
			secondaryLine.JI_LinePrice = 0m;
			AssertNoMessageError(secondaryLine.JI_LinePriceInfo, ValidationConstants.InvoiceLine.ValueShouldBeDeclaredUnderSecondaryLines);
		}

		public void TestCheckJI_OA_ManufacturerAddress()
		{
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;

			AssertJI_OA_ManufacturerAddressForMID(invoiceLine);

			declaration.ValidationModes = ValidationModes.CargoRelease;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;

			AssertJI_OA_ManufacturerAddressForMID(invoiceLine);

			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration2.US_EnableENS = true;

			JobComInvoiceHeader invoice2 = declaration2.Invoices.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();

			AssertJI_OA_ManufacturerAddressForMIDAgainstCanadianProvince(invoiceLine2);
		}

		public void TestCheckJI_OA_ManufacturerAddressForBorderCargoRelease()
		{
			declaration.ValidationModes = ValidationModes.CargoRelease;
			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, ManufacturerIDValidator.Constants.Canadian);

			OrgHeader party = Factory.New<OrgHeader>();
			OrgAddress mainAddress = party.MainAddress;
			string messageError = string.Format(OrganisationValidation.ManufacturerIDMissing, party.MainAddress.OA_Code);
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, messageError);

			OrgCusCode cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AU34567");
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, messageError);

			cusCode.OK_CustomsRegNo = ZString.Empty;

			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, messageError);

			cusCode.OK_CustomsRegNo = "AU";
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			AssertHasMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, ManufacturerIDValidator.Constants.Format);

			cusCode.OK_CustomsRegNo = "AU34567";
			invoiceLine.JI_OA_ManufacturerAddress = mainAddress.PK;
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XY;
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.Canadian);

			declaration.JE_OH_Supplier = party.PK;
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);
		}

		[TestDate(2007, 3, 19)]
		public void TestCheckLinePriceFor9802()
		{
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_SupTariff = "9802004020";
			invoiceLine.JI_Tariff = "8407344800";
			AssertEquals("PreCondition:IsRepairLine", true, invoiceLine.IsUSReturnedGoodsTransaction);
			invoiceLine.JI_LinePrice = 0m;
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);

			invoiceLine.US_98GoodsValue = 0m;
			invoiceLine.JI_LinePrice = 4000m;
			AssertHasMessageError(invoiceLine.US_98GoodsValueInfo, ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);

			invoiceLine.US_98GoodsValue = 10m;
			invoiceLine.JI_LinePrice = 0m;
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);

			invoiceLine.US_98GoodsValue = 10m;
			invoiceLine.JI_LinePrice = 400m;
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);
		}

		public void TestCheckJI_LinePriceShouldBeDeclared()
		{
			invoiceLine.US_SupTariff = "9802008042";
			AssertNotNull("PreCondition:ImportTariff is not null", invoiceLine.ImportSupTariff);
			AssertEquals("PreCondition:IsEligibleForAGOA", true, invoiceLine.ImportSupTariff.IsEligibleForAGOATextileBenefits(invoiceLine.EffectiveDateForDutyRate));

			invoiceLine.JI_LinePrice = 0m;
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);

			invoiceLine.JI_LinePrice = 0m;
			AssertHasMessageError(invoiceLine.JI_LinePriceInfo, ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);

			invoiceLine.JI_LinePrice = 100m;
			AssertNoMessageError(invoiceLine.JI_LinePriceInfo, ValidationConstants.InvoiceLine.RepairTransactionShouldHaveComponentPrice);
		}

		public void TestCheckJI_HazMatCodeQualifier()
		{
			invoiceLine.JI_HazMatCodeQualifier = "~";
			AssertHasMessageError(invoiceLine.JI_HazMatCodeQualifierInfo, FormalImportJobComInvoiceLineValidation.HazMatCodeQualifierShouldBeInList);
			invoiceLine.JI_HazMatCodeQualifier = HazMatQualifierList.Codes.UnitedNations;
			AssertNoMessageError(invoiceLine.JI_HazMatCodeQualifierInfo, FormalImportJobComInvoiceLineValidation.HazMatCodeQualifierShouldBeInList);
		}

		public void TestJI_ParentID()
		{
			invoiceLine.JI_Tariff = "8215.10.0000";
			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "1902192020";
			line2.JI_ParentID = invoiceLine.PK;
			line2.Validation.ValidateJI_Tariff();
			AssertHasMessageError(line2.JI_TariffInfo, ValidationConstants.InvoiceLine.ImportTariff.ChildLineTariffNotFoundInRuleTariffs);

			line2.JI_ParentID = ZGuid.Empty;
			line2.JI_Tariff = "8205203000";
			line2.JI_ParentID = invoiceLine.PK;
			line2.Validation.ValidateJI_Tariff();
			AssertNoMessageError(line2.JI_TariffInfo, ValidationConstants.InvoiceLine.ImportTariff.ChildLineTariffNotFoundInRuleTariffs);
		}

		public void TestCheckJI_BondedWhsQuantityAndCheckBondedWhsQuantityForGUI()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOwner(org);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 1m;
			AssertHasMessageError(invoiceLine.JI_BondedWhsQuantityInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
			AssertHasMessageError(invoiceLine.BondedWhsQuantityForGUIInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine2.PK;
			AssertNoMessageError(invoiceLine.JI_BondedWhsQuantityInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
			AssertNoMessageError(invoiceLine.BondedWhsQuantityForGUIInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
			invoiceLine.JI_ParentID = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.JI_BondedWhsQuantityInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
			AssertHasMessageError(invoiceLine.BondedWhsQuantityForGUIInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
			invoiceLine.US_JI_ParentProduct = invoiceLine2.PK;
			AssertNoMessageError(invoiceLine.JI_BondedWhsQuantityInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
			AssertNoMessageError(invoiceLine.BondedWhsQuantityForGUIInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
			invoiceLine.US_JI_ParentProduct = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.JI_BondedWhsQuantityInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
			AssertHasMessageError(invoiceLine.BondedWhsQuantityForGUIInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
			invoiceLine.JI_PartNo = ZString.Empty;
			AssertNoMessageError(invoiceLine.JI_BondedWhsQuantityInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
			AssertNoMessageError(invoiceLine.BondedWhsQuantityForGUIInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertHasMessageError(invoiceLine.JI_BondedWhsQuantityInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
			AssertHasMessageError(invoiceLine.BondedWhsQuantityForGUIInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.Validation.ValidateBondedWhsQuantityForGUI();
			AssertNoMessageError(invoiceLine.JI_BondedWhsQuantityInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
			AssertNoMessageError(invoiceLine.BondedWhsQuantityForGUIInfo, ValidationConstants.InvoiceLine.WarehousePackageQuantityIsRequired("Inventory Management"));
		}

		public void TestCheckJI_LinePriceBoundary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1m;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "3307301000";
			invoiceLine1.JI_LinePrice = 92.35m;
			invoiceLine1.JI_CustomsQuantity = 273m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3401305000";
			invoiceLine2.JI_LinePrice = 697.30m;
			invoiceLine2.JI_CustomsQuantity = 386m;

			var invoiceLine4 = invoice.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "3304995000";
			invoiceLine4.JI_LinePrice = 524.70m;

			var invoiceLine5 = invoice.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "3304995000";
			invoiceLine5.JI_LinePrice = 13.47m;

			var invoiceLine6 = invoice.InvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "9603294090";
			invoiceLine6.JI_LinePrice = 438.40m;
			invoiceLine6.JI_CustomsQuantity = 1096m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			IDutyData cusEntryLine = invoiceLine6.CusEntryLine;
			var unitPrice = ((ZDecimal)(invoiceLine6.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
			AssertHasMessageError(invoiceLine6.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.FirstQuantity, "tariff", " <= 0.4000", unitPrice));

			declaration.InvoiceLines.RemoveAndDeleteAll();

			invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "99119651";
			invoiceLine2.US_SupQty1 = 15000m;
			invoiceLine2.JI_Tariff = "2003900010";
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.JI_CustomsQuantity = 15000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			cusEntryLine = invoiceLine2.CusEntryLine;
			unitPrice = ((ZDecimal)(invoiceLine2.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
			AssertHasMessageError(invoiceLine2.JI_LinePriceInfo, string.Format(ValueQuantityBoundValidator.UnitPriceOutOfBound, ValueQuantityBoundValidator.FirstQuantity, "supplementary tariff", " < 0.3475", unitPrice));
			AssertHasMessageErrorContaining(invoiceLine2.JI_LinePriceInfo, " < 0.3475");

			invoiceLine2.US_SupQty1 = 30000m;
			invoiceLine2.JI_LinePrice = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNoMessageErrorContaining(invoiceLine2.JI_LinePriceInfo, " < 0.3475");
		}

		public void TestCheckJI_OA_ConsigneeAddressWithoutENSInInvoiceLine()
		{
			var emptyNumberOrg = Factory.New<OrgHeader>();
			emptyNumberOrg.OH_Code = "AFSFREMEL";
			emptyNumberOrg.OH_FullName = "AFS FREIGHT MANAGEMENT P/L";
			emptyNumberOrg.OH_RL_NKClosestPort = "AUMEL";
			emptyNumberOrg.MainAddress.OA_Address1 = "27-29 MIAC BLDG";
			emptyNumberOrg.PrimaryRegistrationNumber.Number = string.Empty;
			emptyNumberOrg.OH_IsConsignee = true;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			var ultimateConsignee = Factory.New<OrgHeader>();
			invoiceHeader.JZ_OA_ConsigneeAddress = ultimateConsignee.PK;

			var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			var line3 = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceHeader.JZ_InvoiceAmount = 800;
			line1.JI_LinePrice = 100;
			line2.JI_LinePrice = 400;
			line3.JI_LinePrice = 500;

			line1.JI_OA_ConsigneeAddress = emptyNumberOrg.PK;
			AssertHasMessageError(line1.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"));

			invoiceHeader.JZ_InvoiceAmount = 10000;
			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;

			line1.JI_OA_ConsigneeAddress = emptyNumberOrg.PK;
			AssertHasMessageError(line1.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"));

			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = false;

			invoiceHeader.JZ_InvoiceAmount = 800;
			line1.JI_LinePrice = 100;
			line2.JI_LinePrice = 400;
			line3.JI_LinePrice = 500;

			line1.JI_OA_ConsigneeAddress = emptyNumberOrg.PK;
			AssertNoMessageError(line1.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"));

			invoiceHeader.JZ_InvoiceAmount = 10000;
			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;

			line1.JI_OA_ConsigneeAddress = emptyNumberOrg.PK;
			AssertNoMessageError(line1.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"));
		}

		public void TestCheckJI_OA_ConsigneeAddressForAIIAndACE_CRL()
		{
			declaration.US_EnableAII = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var ultimateConsignee = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			AssertHasMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"));

			invoice.JZ_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			AssertNoMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"));

			invoice.ConsigneeAddressOrgPK = ZGuid.Empty;
			invoiceLine.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			AssertHasMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"));

			ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "89745387");
			invoice.JZ_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			AssertNoMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"));

			declaration.US_EnableAII = false;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			ultimateConsignee.CustomsCodes.RemoveAndDeleteAll();
			invoice.JZ_OA_ConsigneeAddress = ZGuid.Empty;
			invoiceLine.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			AssertHasMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"));

			ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "89745387");
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertNoMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"));

			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertNoMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"));
			ultimateConsignee.CustomsCodes.RemoveAndDeleteAll();

			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertHasMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"));

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertHasMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"));
		}

		public void TestCheckJI_OA_ConsigneeAddressForCargoRelease()
		{
			declaration.US_EnableCRL = true;

			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertHasMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBN_EncryptedCodeRequired, "Ultimate Consignee"));

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertHasMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBN_EncryptedCodeRequired, "Ultimate Consignee"));

			invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.CargoRelease, false).US_IJAccepted = true;
			invoiceLine.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertNoMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBN_EncryptedCodeRequired, "Ultimate Consignee"));
		}

		public void TestCheckJI_OA_ConsigneeAddressForStandAlonePriorNotice()
		{
			declaration.ValidationModes = ValidationModes.StandAlonePriorNotice;
			AssertEquals("PreCondition", true, declaration.IsStandAlone);

			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			FDAOrganisationValidatorTest.AssertOrganisation(invoiceLine.JI_OA_ConsigneeAddressInfo, (ZGuid organisationPK) =>
			{
				var orgHeader = Factory.Load<OrgHeader>(organisationPK);
				invoiceLine.JI_OA_ConsigneeAddress = orgHeader != null ? orgHeader.MainAddress.PK : ZGuid.Empty;
			});

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress();
			AssertNoNotifications("First Name (FDA Contact) is mandatory for Prior Notice", invoiceLine.JI_OA_ConsigneeAddressInfo);
		}

		public void TestCheckJI_TariffForFishNotFromRussia()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "2003900010", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.NonRUCertificationRequired, tariff);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "2003900011", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST55";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2003900010";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Russia;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarning(invoiceLine.JI_TariffInfo, TariffValidator.TariffRequiresNonRussianCertificationDocument);
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasWarning(invoiceLine.JI_TariffInfo, TariffValidator.TariffRequiresNonRussianCertificationDocument);

			invoiceLine.JI_Tariff = "2003900011";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Russia;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarning(invoiceLine.JI_TariffInfo, TariffValidator.TariffRequiresNonRussianCertificationDocument);
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarning(invoiceLine.JI_TariffInfo, TariffValidator.TariffRequiresNonRussianCertificationDocument);
		}

		protected JobDeclaration declaration;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}

		sealed class FormalImportJobComInvoiceLineValidationForTest : FormalImportJobComInvoiceLineValidation
		{
			public FormalImportJobComInvoiceLineValidationForTest(JobComInvoiceLine parent)
				: base(parent)
			{
			}

			internal new bool IsTariffMandatory => base.IsTariffMandatory;
		}
	}
}
