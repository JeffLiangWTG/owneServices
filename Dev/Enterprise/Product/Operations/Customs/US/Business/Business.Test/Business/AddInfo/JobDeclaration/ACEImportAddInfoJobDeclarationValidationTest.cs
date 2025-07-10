using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEImportAddInfoJobDeclarationValidationTest : TestCaseWithFactory
	{
		public void TestCheckUS_FTZNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;

			var errorMsg = "FTZ Number must be 3 digits + 3 alpha numeric (+ 3 alpha numeric) or  3 digits + 2 alpha numeric (+ 2 digits).";
			declaration.US_FTZNo = "12345678";
			AssertHasMessageError(declaration.US_FTZNoInfo, errorMsg);
			declaration.US_FTZNo = "1234567";
			AssertNoMessageError(declaration.US_FTZNoInfo, errorMsg);
			declaration.US_FTZNo = "123456789";
			AssertNoMessageError(declaration.US_FTZNoInfo, errorMsg);
			declaration.US_FTZNo = "1A23456";
			AssertHasMessageError(declaration.US_FTZNoInfo, errorMsg);
		}

		[TestDate(1960, 2, 2)]
		public void TestMaxminLowValueShipmentValueComesFromRefTaxOrFee()
		{
			var grouping = Factory.NewWithValidTestData<RefDataGrouping>();
			grouping.ZZZ_DataGrouping = "US";
			grouping.ZZZ_Description = "United States";
			var deminimus = Factory.New<RefCusTaxOrFee>(); //.Loader(Factory)("US", "DEM", ZDateTime.Now);
			deminimus.ZZF_ZZZ_NKDataGrouping = "US";
			deminimus.ZZF_Code = "DEM";
			deminimus.ZZF_StartDate = new ZDateTime(1960, 1, 1);
			deminimus.ZZF_EndDate = new ZDateTime(1960, 3, 3);
			deminimus.ZZF_Value = 800;
			deminimus.ZZF_Description = "a value";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.US_98GoodsValue = 799;
			var errorMessage = "The maximum customs value allowed for entry type 86 is 800";
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoMessageError(declaration.US_EntryTypeInfo, errorMessage);
			invoiceLine.US_98GoodsValue = 801;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, errorMessage);
		}

		public void TestCheckUS_InsuranceAgent()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "Insurance Agent");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "IAA", "UnitedStates Code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			AssertInsuranceProperties(AutoUSAddInfo.Schema.US_InsuranceAgent, "IAA", "XXX", declaration => declaration.AddInfoValidation.ValidateUS_InsuranceAgent());
		}

		public void TestCheckUS_InsuranceDisposition()
		{
			AssertInsuranceProperties(AutoUSAddInfo.Schema.US_InsuranceDisposition, InsuranceDispositionCodeList.Codes.AcceptedByCBP, "@", declaration => declaration.AddInfoValidation.ValidateUS_InsuranceDisposition());
		}

		public void TestCheckUS_PGAExpeditedRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_PGAExpeditedRelease = true;
			AssertHasMessageError(declaration.US_PGAExpeditedReleaseInfo, ACEImportAddInfoJobDeclarationValidation.PGAReleaseIndicator);
			declaration.US_PGAExpeditedRelease = false;
			AssertNoMessageError(declaration.US_PGAExpeditedReleaseInfo, ACEImportAddInfoJobDeclarationValidation.PGAReleaseIndicator);
			declaration.US_EnableCRL = false;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_PGAExpeditedRelease = true;
			AssertHasMessageError(declaration.US_PGAExpeditedReleaseInfo, ACEImportAddInfoJobDeclarationValidation.PGAReleaseIndicator);
			declaration.US_PGAExpeditedRelease = false;
			AssertNoMessageError(declaration.US_PGAExpeditedReleaseInfo, ACEImportAddInfoJobDeclarationValidation.PGAReleaseIndicator);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.US_CertifyCargoRelease = false;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
			declaration.US_PGAExpeditedRelease = true;
			AssertHasWarning(declaration.US_PGAExpeditedReleaseInfo, ACEImportAddInfoJobDeclarationValidation.PGAFor06FTZ);
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.AddInfoValidation.ValidateUS_PGAExpeditedRelease();
			AssertNoWarning(declaration.US_PGAExpeditedReleaseInfo, ACEImportAddInfoJobDeclarationValidation.PGAFor06FTZ);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_PGAExpeditedRelease = false;
			AssertNoWarning(declaration.US_PGAExpeditedReleaseInfo, ACEImportAddInfoJobDeclarationValidation.PGAFor06FTZ);
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			declaration.US_PGAExpeditedRelease = true;
			AssertHasWarningContaining(declaration.US_PGAExpeditedReleaseInfo, ACEImportAddInfoJobDeclarationValidation.PGAForWarehouseWithdraw);
		}

		public void TestCheckUS_EnableCRL()
		{
			var dec2 = Factory.NewWithValidTestData<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec2.US_EntryMode = EntryModeList.Codes.RLF;
			dec2.US_EnableCRL = true;
			dec2.US_EnableENS = false;
			var invoice = dec2.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Mock<CusEntryHeader> ensEntry = Factory.NewMoq<CusEntryHeader>();
			ensEntry.Object.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			ensEntry.Object.CH_JE = dec2.PK;
			ensEntry.Object.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			ensEntry.Setup(m => m.HasBeenLodgedAtCustoms).Returns(true);
			ensEntry.Object.Messages.AddNew(typeof(EDIMessage));
			dec2.CustomsEntryHeaders.Add(ensEntry.Object);
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(ensEntry.Object.MergedLines.AddNew());
			dec2.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			dec2.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			dec2.US_EnableENS = true;
			dec2.US_EnableCRL = false;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			dec2.AddInfoValidation.ValidateUS_EnableCRL();
			AssertHasMessageError(dec2.US_EnableCRLInfo, ACEImportAddInfoJobDeclarationValidation.CargoReleaseShouldBeTickedForDomesticStatus);
			dec2.US_EnableCRL = true;
			AssertNoMessageError(dec2.US_EnableCRLInfo, ACEImportAddInfoJobDeclarationValidation.CargoReleaseShouldBeTickedForDomesticStatus);
			dec2.US_EnableCRL = false;
			invoiceLine.Delete();
			dec2.AddInfoValidation.ValidateUS_EnableCRL();
			AssertNoMessageError(dec2.US_EnableCRLInfo, ACEImportAddInfoJobDeclarationValidation.CargoReleaseShouldBeTickedForDomesticStatus);
			dec2.US_ImmediateDelivery = true;
			dec2.US_EnableCRL = false;
			AssertHasMessageError(dec2.US_EnableCRLInfo, ACEImportAddInfoJobDeclarationValidation.CargoReleaseShouldBeTickedForImmediateDelivery);
			dec2.US_EnableCRL = true;
			AssertNoMessageError(dec2.US_EnableCRLInfo, ACEImportAddInfoJobDeclarationValidation.CargoReleaseShouldBeTickedForImmediateDelivery);
		}

		public void TestEntryDateValidationRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_EnableENS = true;
			declaration.US_EntryDate = ZDateTime.Now.AddMonths(-4);
			AssertNoWarningContaining(declaration.US_EntryDateInfo, ACEImportAddInfoJobDeclarationValidation.DateAtEntryPortPastLimitWarning);
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableCRL = true;
			declaration.US_EntryDate = ZDateTime.Now.AddMonths(-4);
			AssertHasWarningContaining(declaration.US_EntryDateInfo, ACEImportAddInfoJobDeclarationValidation.DateAtEntryPortPastLimitWarning);
		}

		public void TestUS_EntryDateLimitWarning()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.BargeMovement;
			declaration.US_EnableENS = true;
			declaration.ValidationModes = ValidationModes.CargoRelease;
			declaration.US_EntryDate = ZDateTime.Today.AddDays(-100);
			AssertHasWarningContaining(declaration.US_EntryDateInfo, ACEImportAddInfoJobDeclarationValidation.DateAtEntryPortPastLimitWarning);
			declaration.US_EntryDate = ZDateTime.Today.AddDays(-89);
			AssertNoWarningContaining(declaration.US_EntryDateInfo, ACEImportAddInfoJobDeclarationValidation.DateAtEntryPortPastLimitWarning);
		}

		public void TestCheckUS_FDAADTA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var atfLine = invoiceLine.ATFLines.AddNew();
			atfLine.US_CategoryCode = "SG";
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			declaration.US_FDAADTA = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.US_FDAADTAInfo, ZString.Format(ACEImportAddInfoJobDeclarationValidation.RequiredFDAADTA, "ATF"));
			AssertHasMessageErrorContaining(declaration.US_FDAADTAInfo, ZString.Format(ACEImportAddInfoJobDeclarationValidation.RequiredFDAADTA, "DEA"));
			declaration.US_FDAADTA = ZDateTime.BrettsBirthday;
			AssertNoMessageErrorContaining(declaration.US_FDAADTAInfo, ZString.Format(ACEImportAddInfoJobDeclarationValidation.RequiredFDAADTA, "ATF"));
			AssertNoMessageErrorContaining(declaration.US_FDAADTAInfo, ZString.Format(ACEImportAddInfoJobDeclarationValidation.RequiredFDAADTA, "DEA"));
			invoiceLine.ATFLines.RemoveAndDeleteAll();
			invoiceLine.US_ATFInd = "";
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.DEAHeaders.RemoveAndDeleteAll();
			invoiceLine.US_DEAInd = "";
			invoiceLine.US_DEADisclaimReason = OGAIndicatorList.Codes.Disclaimed;
			declaration.US_FDAADTA = ZDateTime.Empty;
			AssertNoMessageErrorContaining(declaration.US_FDAADTAInfo, ZString.Format(ACEImportAddInfoJobDeclarationValidation.RequiredFDAADTA, "ATF"));
			AssertNoMessageErrorContaining(declaration.US_FDAADTAInfo, ZString.Format(ACEImportAddInfoJobDeclarationValidation.RequiredFDAADTA, "DEA"));
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_AMSInd = "C";
			invoiceLine.US_AMSDisclaimProgram = AMSProgramList.Codes.MO7;
			declaration.US_FDAADTA = ZDateTime.Today;
			AssertNoMessageErrorContaining(declaration.US_FDAADTAInfo, ACEImportAddInfoJobDeclarationValidation.DateTimeOfArrivalMandatoryForAMS);
			AssertHasMessageErrorContaining(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.TimeOfArrivalFormat);
			declaration.US_FDAADTA = ZDateTime.Today.AddHours(-1);
			AssertNoMessageErrorContaining(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.TimeOfArrivalFormat);
			declaration.US_FDAADTA = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.US_FDAADTAInfo, ACEImportAddInfoJobDeclarationValidation.DateTimeOfArrivalMandatoryForAMS);
			invoiceLine.US_AMSInd = "D";
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			declaration.AddInfoValidation.ValidateUS_FDAADTA();
			AssertHasMessageErrorContaining(declaration.US_FDAADTAInfo, ACEImportAddInfoJobDeclarationValidation.DateTimeOfArrivalMandatoryForAMS);
			declaration.US_FDAADTA = ZDateTime.Today;
			AssertNoMessageErrorContaining(declaration.US_FDAADTAInfo, ACEImportAddInfoJobDeclarationValidation.DateTimeOfArrivalMandatoryForAMS);
			AssertHasMessageErrorContaining(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.TimeOfArrivalFormat);
			declaration.US_FDAADTA = ZDateTime.Today.AddHours(-1);
			AssertNoMessageErrorContaining(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.TimeOfArrivalFormat);
			amsLine.US_Program = AMSProgramList.Codes.MO2;
			declaration.US_FDAADTA = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.US_FDAADTAInfo, ACEImportAddInfoJobDeclarationValidation.DateTimeOfArrivalMandatoryForAMS);
			declaration.US_FDAADTA = ZDateTime.Today;
			AssertNoMessageErrorContaining(declaration.US_FDAADTAInfo, ACEImportAddInfoJobDeclarationValidation.DateTimeOfArrivalMandatoryForAMS);
			AssertHasMessageErrorContaining(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.TimeOfArrivalFormat);
			declaration.US_FDAADTA = ZDateTime.Today.AddHours(-1);
			AssertNoMessageErrorContaining(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.TimeOfArrivalFormat);
		}

		[TestDate(2015, 07, 29)]
		public void TestCheckUS_CargoReleaseType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.Validation.ValidateJE_TransportMode();
			declaration.AddInfoValidation.ValidateUS_EntryType();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.AddInfoValidation.ValidateUS_CargoReleaseType();
			AssertNoMessageErrorContaining(declaration.US_CargoReleaseTypeInfo, ACEImportAddInfoJobDeclarationValidation.FTZNumberWillNotBeAcceptedinACS);
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			declaration.AddInfoValidation.ValidateUS_CargoReleaseType();
			AssertHasMessageErrorContaining(declaration.US_CargoReleaseTypeInfo, ACEImportAddInfoJobDeclarationValidation.FTZNumberWillNotBeAcceptedinACS);
		}

		public void TestCheckUS_BondProducerAccountNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondProducerAccNo = "";
			AssertHasMessageErrorContaining(declaration.US_BondProducerAccNoInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_BondProducerAccNo = "90843";
			AssertNoMessageErrors(declaration.US_BondProducerAccNoInfo);
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertNoMessageErrors(declaration.US_BondProducerAccNoInfo);
			declaration.US_BondProducerAccNo = "";
			AssertNoMessageErrors(declaration.US_BondProducerAccNoInfo);
		}

		public void TestCheckUS_PaymentType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.ImportEntryNumber = "1";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "1";
			Factory.Save();
			AssertNotNull(declaration.RelatedStatement);
		}

		public void TestCheckUS_BondWaiverCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_BondWaiverCode = BondWaiverReasonCodeList.Codes._995;
			AssertHasMessageError(declaration.US_BondWaiverCodeInfo, ACEImportAddInfoJobDeclarationValidation.BondWaiverCodeNotForNoBondRequired);
			declaration.US_BondWaiverCode = string.Empty;
			AssertNoMessageError(declaration.US_BondWaiverCodeInfo, ACEImportAddInfoJobDeclarationValidation.BondWaiverCodeNotForNoBondRequired);
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			declaration.US_EntryType = "07";
			declaration.US_BondWaiverCode = BondWaiverReasonCodeList.Codes._995;
			AssertHasMessageError(declaration.US_BondWaiverCodeInfo, ACEImportAddInfoJobDeclarationValidation.BondWaivedForAD_CVDEntryType);
			declaration.US_EntryType = "03";
			declaration.US_BondWaiverCode = BondWaiverReasonCodeList.Codes._995;
			AssertHasMessageError(declaration.US_BondWaiverCodeInfo, ACEImportAddInfoJobDeclarationValidation.BondWaivedForAD_CVDEntryType);
			declaration.US_EntryType = "34";
			declaration.US_BondWaiverCode = BondWaiverReasonCodeList.Codes._995;
			AssertHasMessageError(declaration.US_BondWaiverCodeInfo, ACEImportAddInfoJobDeclarationValidation.BondWaivedForAD_CVDEntryType);
			declaration.US_EntryType = "38";
			declaration.US_BondWaiverCode = BondWaiverReasonCodeList.Codes._995;
			AssertHasMessageError(declaration.US_BondWaiverCodeInfo, ACEImportAddInfoJobDeclarationValidation.BondWaivedForAD_CVDEntryType);
			declaration.US_EntryType = "01";
			declaration.US_BondWaiverCode = BondWaiverReasonCodeList.Codes._995;
			AssertNoMessageError(declaration.US_BondWaiverCodeInfo, ACEImportAddInfoJobDeclarationValidation.BondWaivedForAD_CVDEntryType);
		}

		public void TestCheckUS_BondAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_BondAmount = 10m;
			AssertHasMessageError(declaration.US_BondAmountInfo, ACEImportAddInfoJobDeclarationValidation.NotRelevantForContinuousBond);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			declaration.US_BondAmount = 0m;
			AssertNoMessageError(declaration.US_BondAmountInfo, ACEImportAddInfoJobDeclarationValidation.NotRelevantForContinuousBond);
			AssertHasMessageError(declaration.US_BondAmountInfo, "value cannot be zero.");
			declaration.US_BondAmount = 10m;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;
			AssertNoMessageError(declaration.US_BondAmountInfo, "value cannot be zero.");
		}

		public void TestCheckUS_BondAmountWithSTBRule()
		{
			var date = new ZDateTime(2023, 11, 30);

			var customsRule = Factory.New<CustomsRule>();
			customsRule.CPH_StartDate = date.Date.AddMonths(-1);
			customsRule.CPH_EndDate = date.Date.AddMonths(1);
			customsRule.CPH_PermitDescription = "TEST";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ensEntry = declaration.ActiveEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.US_DutyCalcDate = date;

			var expected = string.Format(ACEImportAddInfoJobDeclarationValidation.BondAmountExceedsSTBRuleAmount, "100", customsRule.HumanReadableName);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondAmount = 100m;
			AssertNoMessageError(declaration.US_BondAmountInfo, expected);

			var stbRule = customsRule.Rules.AddNew();
			stbRule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount;
			stbRule.CPR_ValueTo = "1000";
			stbRule = customsRule.Rules.AddNew();
			stbRule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount;
			stbRule.CPR_ValueTo = "110";
			Factory.Save();

			var addInfoValidation = declaration.AddInfoValidation;
			addInfoValidation.ValidateUS_BondAmount();
			AssertNoMessageError(declaration.US_BondAmountInfo, expected);

			stbRule.CPR_ValueTo = "100";
			Factory.Save();
			addInfoValidation.ValidateUS_BondAmount();
			AssertNoMessageError(declaration.US_BondAmountInfo, expected);

			stbRule.CPR_ValueTo = "90";
			Factory.Save();
			addInfoValidation.ValidateUS_BondAmount();
			AssertHasMessageError(declaration.US_BondAmountInfo, expected);

			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			addInfoValidation.ValidateUS_BondAmount();
			AssertNoMessageError(declaration.US_BondAmountInfo, expected);
		}

		public void TestCheckUS_BondProducerAccNo2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_BondType2 = BondTypeList.Codes.ContinuousBond;
			declaration.US_BondProducerAccNo2 = "897423";
			AssertHasMessageError(declaration.US_BondProducerAccNo2Info, ACEImportAddInfoJobDeclarationValidation.NotRelevantForContinuousBond);
			declaration.US_BondType2 = BondTypeList.Codes.SingleTransactionBond;
			AssertNoMessageError(declaration.US_BondProducerAccNo2Info, ACEImportAddInfoJobDeclarationValidation.NotRelevantForContinuousBond);
		}

		public void TestCheckUS_BondAmount2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_BondType2 = BondTypeList.Codes.ContinuousBond;
			declaration.US_BondAmount2 = 10m;
			AssertHasMessageError(declaration.US_BondAmount2Info, ACEImportAddInfoJobDeclarationValidation.NotRelevantForContinuousBond);
			declaration.US_BondType2 = BondTypeList.Codes.SingleTransactionBond;
			AssertNoMessageError(declaration.US_BondAmount2Info, ACEImportAddInfoJobDeclarationValidation.NotRelevantForContinuousBond);
		}

		public void TestCheckUS_BondType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_BondType = string.Empty;
			AssertHasMessageErrorContaining(declaration.US_BondTypeInfo, FormalImportAddInfoJobDeclarationValidation.BondTypeRequired);
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_BondType = string.Empty;
			AssertHasMessageErrorContaining(declaration.US_BondTypeInfo, FormalImportAddInfoJobDeclarationValidation.BondTypeRequired);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			var messageErrorText = ZString.Format(ACEImportAddInfoJobDeclarationValidation.BondCannotBeWaived, "Entry Type '03'");
			AssertHasMessageErrorContaining(declaration.US_BondTypeInfo, messageErrorText);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.AddInfoValidation.ValidateUS_BondType();
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, messageErrorText);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			messageErrorText = ZString.Format(ACEImportAddInfoJobDeclarationValidation.BondCannotBeWaived, ACEImportAddInfoJobDeclarationValidation.BondCannotBeWaivedDuetoADCVD);
			invoiceLine.US_ADDCaseNo = "A462105011";
			declaration.AddInfoValidation.ValidateUS_BondType();
			AssertHasMessageErrorContaining(declaration.US_BondTypeInfo, messageErrorText);
			invoiceLine.US_ADDCaseNo = ZString.Empty;
			declaration.AddInfoValidation.ValidateUS_BondType();
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, messageErrorText);
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = "XA";
			invoiceLine2.US_UC_NKCountryOfOrigin = "XB";
			invoiceLine3.US_UC_NKCountryOfOrigin = "CA";
			declaration.AddInfoValidation.ValidateUS_BondType();
			AssertHasMessageErrorContaining(declaration.US_BondTypeInfo, ACEImportAddInfoJobDeclarationValidation.BondTypeCanOnlyBeWaived);
			invoiceLine3.US_UC_NKCountryOfOrigin = "XA";
			declaration.AddInfoValidation.ValidateUS_BondType();
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, ACEImportAddInfoJobDeclarationValidation.BondTypeCanOnlyBeWaived);
		}

		public void TestCheckUS_BondType2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_BondType2 = BondTypeList.Codes.ContinuousBond;
			AssertHasMessageErrorContaining(declaration.US_BondType2Info, ListValidation.InvalidCodeMessageError);
			declaration.US_BondType2 = BondTypeList.Codes.SingleTransactionBond;
			AssertNoMessageErrorContaining(declaration.US_BondType2Info, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_BondSuperseding()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_BondSuperseding = true;
			AssertHasMessageError(declaration.US_BondSupersedingInfo, ACEImportAddInfoJobDeclarationValidation.ContinuousBondCoverageSupersedingWhileNotContinuousBondType);
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertNoMessageError(declaration.US_BondSupersedingInfo, ACEImportAddInfoJobDeclarationValidation.ContinuousBondCoverageSupersedingWhileNotContinuousBondType);
		}

		public void TestCheckUS_BondProducerAccountNo2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_BondType2 = BondTypeList.Codes.ContinuousBond;
			declaration.US_BondProducerAccNo2 = "90843";
			AssertHasMessageErrors(declaration.US_BondProducerAccNo2Info);
			declaration.US_BondProducerAccNo2 = "";
			AssertNoMessageErrors(declaration.US_BondProducerAccNo2Info);
			declaration.US_BondType2 = BondTypeList.Codes.SingleTransactionBond;
			AssertHasMessageErrors(declaration.US_BondProducerAccNo2Info);
			declaration.US_BondProducerAccNo2 = "124";
			AssertNoMessageErrors(declaration.US_BondProducerAccNo2Info);
		}

		public void TestCheckUS_SplitReleaseCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_SESplitRel = SplitShipmentReleaseCodeList.Codes.HoldAll;
			AssertNoMessageErrorContaining(declaration.US_SESplitRelInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_SESplitRel = "X";
			AssertHasMessageErrorContaining(declaration.US_SESplitRelInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_US_NKLocationOfGoodsForACEEntrySummary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration.US_EnableENS = true;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_US_NKLocationOfGoodsInfo, "Location of Goods (FIRMS code) is required for Entry Summary (where Entry Type is ");
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_US_NKLocationOfGoodsInfo, "Location of Goods (FIRMS code) is required for Entry Summary (where Entry Type is ");
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.US_US_NKLocationOfGoodsInfo, "Location of Goods (FIRMS code) is required for Entry Summary (where Entry Type is ");
		}

		public void TestCheckUS_US_NKLocationOfGoodsForCargoRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertHasMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsRequiredForCargoRelease);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertNoMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsRequiredForCargoRelease);
			declaration.US_EnableCRL = true;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertNoMessageError(declaration.US_US_NKLocationOfGoodsInfo, FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsRequiredForCargoRelease);
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertHasMessageError(declaration.US_US_NKLocationOfGoodsInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsRequiredForEntrySummaryWithEntryType, EntryTypeList.Codes.Warehouse));
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_EnableCRL = true;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertHasMessageError(declaration.US_US_NKLocationOfGoodsInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsRequiredForEntrySummaryWithEntryType, EntryTypeList.Codes.ReWarehouse));

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "Z2Z2", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityType, FacilityTypeList.Codes.BondedWarehouse_04);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "KNZ1", "Kanzaki", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityType, FacilityTypeList.Codes.CBPAdministrativeSite_08);
			Factory.Save();

			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertHasMessageError(declaration.US_US_NKLocationOfGoodsInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsRequiredForEntrySummaryWithEntryType, EntryTypeList.Codes.Warehouse));
			declaration.US_US_NKLocationOfGoods = "KNZ1";
			AssertNoMessageError(declaration.US_US_NKLocationOfGoodsInfo, string.Format(FormalImportAddInfoJobDeclarationValidation.LocationOfGoodsRequiredForEntrySummaryWithEntryType, EntryTypeList.Codes.Warehouse));
		}

		public void TestCheckUS_EntryDateElectionCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.AddInfoValidation.ValidateUS_CertifyCargoRelease();
			AssertHasMessageError(declaration.US_EntryDateElectionCodeInfo, ACEImportAddInfoJobDeclarationValidation.CertifyForCargoReleaseCannotBeUsedForWeeklyEstimate);
			AssertHasMessageError(declaration.US_CertifyCargoReleaseInfo, ACEImportAddInfoJobDeclarationValidation.CertifyForCargoReleaseCannotBeUsedForWeeklyEstimate);
			declaration.US_EstimatedEntryDate = ZDate.Today.AddDays(8);
			declaration.US_PresentationDate = declaration.US_EstimatedEntryDate.AddDays(-1);
			AssertNoMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateRequiredForElectionCode);
			AssertNoMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateForWeeklyCannotBeMoreThan7DaysInTheFuture);
			declaration.US_CertifyCargoRelease = true;
			declaration.US_PresentationDate = declaration.US_EstimatedEntryDate;
			AssertNoMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateRequiredForElectionCode);
			AssertHasMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateForWeeklyCannotBeMoreThan7DaysInTheFuture);
			declaration.US_EstimatedEntryDate = ZDate.Today.AddDays(7);
			declaration.US_PresentationDate = declaration.US_EstimatedEntryDate;
			AssertNoMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateRequiredForElectionCode);
			AssertNoMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateForWeeklyCannotBeMoreThan7DaysInTheFuture);
			var crlEntry = declaration.ActiveEntryHeaders.AddNew();
			crlEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			crlEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			declaration.US_EstimatedEntryDate = ZDate.Today.AddDays(8);
			declaration.US_PresentationDate = declaration.US_EstimatedEntryDate;
			AssertNoMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateRequiredForElectionCode);
			AssertNoMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateForWeeklyCannotBeMoreThan7DaysInTheFuture);
			crlEntry.Delete();
			declaration.US_PresentationDate = ZDateTime.Empty;
			AssertHasMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateRequiredForElectionCode);
			AssertNoMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateForWeeklyCannotBeMoreThan7DaysInTheFuture);
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.NonWeeklyEstimateFilingDate;
			declaration.AddInfoValidation.ValidateUS_CertifyCargoRelease();
			AssertNoMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateRequiredForElectionCode);
			AssertNoMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateNotRequiredForElectionCode);
			AssertNoMessageError(declaration.US_EntryDateElectionCodeInfo, ACEImportAddInfoJobDeclarationValidation.CertifyForCargoReleaseCannotBeUsedForWeeklyEstimate);
			AssertNoMessageError(declaration.US_CertifyCargoReleaseInfo, ACEImportAddInfoJobDeclarationValidation.CertifyForCargoReleaseCannotBeUsedForWeeklyEstimate);
			declaration.US_PresentationDate = ZDateTime.Today;
			AssertNoMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateRequiredForElectionCode);
			AssertHasMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateNotRequiredForElectionCode);
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_PresentationDate = ZDateTime.Today;
			AssertNoMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateRequiredForElectionCode);
			AssertNoMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateNotRequiredForElectionCode);
			declaration.US_CertifyCargoRelease = false;
			declaration.US_PresentationDate = declaration.US_EstimatedEntryDate.AddDays(-2);
			AssertNoMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateNotRequiredForElectionCode);
			declaration.US_PresentationDate = ZDateTime.Today.AddDays(8);
			AssertHasMessageError(declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateForWeeklyCannotBeMoreThan7DaysInTheFuture);
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_PresentationDate = ZDateTime.Empty;
			AssertHasMessageError("This is ACE Cargo Release validation mode - certify via entry summary", declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateRequiredForElectionCode);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			declaration.AddInfoValidation.ValidateUS_PresentationDate();
			AssertNoMessageError("This is ACE Cargo Release validation mode - certify via entry summary", declaration.US_PresentationDateInfo, ACEImportAddInfoJobDeclarationValidation.DateRequiredForElectionCode);
		}

		public void TestCheckWHSDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_EnableENS = true;
			declaration.US_WHSEntryNumber = "";
			AssertHasMessageErrorContaining(declaration.US_WHSEntryNumberInfo, FormalImportAddInfoJobDeclarationValidation.WarehouseEntryNumberRequired);
			declaration.US_WHSEntryNumber = "A";
			AssertNoMessageErrorContaining(declaration.US_WHSEntryNumberInfo, FormalImportAddInfoJobDeclarationValidation.WarehouseEntryNumberRequired);
			AssertHasMessageErrors(FormalImportAddInfoJobDeclarationValidation.WarehouseEntryNumberFormat, declaration.US_WHSEntryNumberInfo);
			declaration.US_WHSEntryNumber = "12345678";
			AssertNoMessageErrors(FormalImportAddInfoJobDeclarationValidation.WarehouseEntryNumberFormat, declaration.US_WHSEntryNumberInfo);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_WHSEntryNumber = "";
			AssertHasMessageError(declaration.US_WHSEntryNumberInfo, FormalImportAddInfoJobDeclarationValidation.WarehouseEntryNumberRequired);
			declaration.US_WHSEntryNumber = "12345A";
			AssertHasMessageError(declaration.US_WHSEntryNumberInfo, FormalImportAddInfoJobDeclarationValidation.WarehouseEntryNumberFormat);
			AssertNoMessageError(declaration.US_WHSEntryNumberInfo, FormalImportAddInfoJobDeclarationValidation.WarehouseEntryNumberRequired);
			declaration.US_WHSEntryNumber = "12345678";
			AssertNoMessageError(declaration.US_WHSEntryNumberInfo, FormalImportAddInfoJobDeclarationValidation.WarehouseEntryNumberFormat);
			declaration.US_WHSEntryFilerCode = "888";
			AssertNoMessageErrors(FormalImportAddInfoJobDeclarationValidation.WHSFilerCode, declaration.US_WHSEntryFilerCodeInfo);
			declaration.US_WHSEntryFilerCode = "A8";
			AssertHasMessageErrors(FormalImportAddInfoJobDeclarationValidation.WHSFilerCode, declaration.US_WHSEntryFilerCodeInfo);
		}

		public void TestCheckUS_FDAATAForACE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			line.ACE_FDALines.AddNew();
			var line2 = invoice.JobComInvoiceLines.AddNew();
			var fda = line2.ACE_FDALines.AddNew();
			fda.US_PND = false;
			declaration.AddInfoValidation.ValidateUS_FDAADTA();
			AssertHasMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateTimeOfArrivalMandatory);
			declaration.US_FDAADTA = ZDateTime.Today.AddDays(-11);
			AssertNoMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateTimeOfArrivalMandatory);
			AssertHasMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateOfArrivalRange);
			declaration.US_FDAADTA = ZDateTime.Today.AddDays(-9);
			AssertNoMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateOfArrivalRange);
			declaration.US_EntryDate = ZDateTime.Today;
			declaration.US_FDAADTA = ZDateTime.Today.AddDays(5);
			AssertNoMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateOfArrivalRange);
			AssertHasWarning(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateOfArrivalEntry);
			declaration.US_FDAADTA = ZDateTime.Today;
			AssertNoMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateOfArrivalRange);
			AssertNoWarning(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateOfArrivalEntry);
			AssertHasMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.TimeOfArrivalFormat);
			declaration.US_FDAADTA = ZDate.Today.AddHours(1);
			AssertNoMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.TimeOfArrivalFormat);
			line.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			declaration.US_FDAADTA = ZDateTime.Empty;
			AssertHasMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateTimeOfArrivalMandatory);
			declaration.US_FDAADTA = ZDate.Today.AddHours(1);
			AssertNoMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.TimeOfArrivalFormat);
			line.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = line.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			declaration.US_FDAADTA = ZDateTime.Empty;
			AssertHasMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.DateTimeOfArrivalMandatory);
			declaration.US_FDAADTA = ZDate.Today.AddHours(1);
			AssertNoMessageError(declaration.US_FDAADTAInfo, ValidationConstants.PriorNotice.TimeOfArrivalFormat);
		}

		public void TestBondDipositionCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_BondDispositionCode = "~";
			AssertHasMessageErrorContaining(declaration.US_BondDispositionCodeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.CVB;
			AssertNoMessageErrorContaining(declaration.US_BondDispositionCodeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_BondDispositionCode2 = "~";
			AssertHasMessageErrorContaining(declaration.US_BondDispositionCode2Info, ListValidation.InvalidCodeMessageError);
			declaration.US_BondDispositionCode2 = BondDispositionCodeList.Codes.CVB;
			AssertNoMessageErrorContaining(declaration.US_BondDispositionCode2Info, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_PipelineName()
		{
			var declaration = GetMergedDeclaration();
			declaration.AddInfoValidation.ValidateUS_PipelineName();
			AssertNoMessageErrorContaining(declaration.US_PipelineNameInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.AddInfoValidation.ValidateUS_PipelineName();
			AssertHasMessageErrorContaining(declaration.US_PipelineNameInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_PipelineName = "12345";
			AssertNoMessageErrorContaining(declaration.US_PipelineNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_NonAMS()
		{
			var declaration = GetMergedDeclaration();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.Bills.RemoveAndDeleteAll();
			declaration.JE_MasterBill = "TESTMASTER";
			declaration.JE_HouseBill = "TESTHOUSE";
			AssertEquals(2, declaration.Bills.Count);
			declaration.US_NonAMS = true;
			AssertNoMessageErrors(declaration.US_NonAMSInfo);
			declaration.Bills[1].US_SESplitShip = true;
			declaration.AddInfoValidation.ValidateUS_NonAMS();
			AssertHasMessageErrorContaining(declaration.US_NonAMSInfo, FormalImportAddInfoBillValidation.NonAMSBillCannotBeSplit);
			declaration.Bills[1].US_SESplitShip = false;
			declaration.AddInfoValidation.ValidateUS_NonAMS();
			AssertNoMessageErrors(declaration.US_NonAMSInfo);
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_NonAMS = true;
			AssertHasMessageErrorContaining(declaration.US_NonAMSInfo, FormalImportAddInfoBillValidation.NonTransportNeverBeNonAMS);
			declaration.US_NonAMS = false;
			AssertNoMessageErrorContaining(declaration.US_NonAMSInfo, FormalImportAddInfoBillValidation.NonTransportNeverBeNonAMS);
			declaration.US_GeneralOrderNo = "1232142141";
			AssertHasWarningContaining(declaration.US_NonAMSInfo, ACEImportAddInfoJobDeclarationValidation.NonAMSRequiredForGONumber);
			declaration.US_NonAMS = true;
			AssertNoWarningContaining(declaration.US_NonAMSInfo, ACEImportAddInfoJobDeclarationValidation.NonAMSRequiredForGONumber);
		}

		void AssertInsuranceProperties(ZString propertyName, ZString validValue, ZString invalidValue, Action<JobDeclaration> validateAction)
		{
			var message = "The code you have selected is not in the list.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			var propertyInfo = declaration.FindPropertyInfo(propertyName);
			propertyInfo.Value = invalidValue;
			AssertHasMessageError("Should have the expected message as the code is invalid.", propertyInfo, message);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			validateAction.Invoke(declaration);
			AssertNoMessageError("Should not have the expected message as the declaration is not ACE.", propertyInfo, message);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			validateAction.Invoke(declaration);
			AssertNoMessageError("Should not have the expected message as the declaration is not ACE.", propertyInfo, message);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			validateAction.Invoke(declaration);
			AssertNoMessageError("Should not have the expected message as the bond type is not SingleTransactionBond.", propertyInfo, message);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			propertyInfo.Value = validValue;
			AssertNoMessageError("Should not have the expected message as the code is valid.", propertyInfo, message);
		}

		JobDeclaration GetMergedDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			return declaration;
		}
	}
}
