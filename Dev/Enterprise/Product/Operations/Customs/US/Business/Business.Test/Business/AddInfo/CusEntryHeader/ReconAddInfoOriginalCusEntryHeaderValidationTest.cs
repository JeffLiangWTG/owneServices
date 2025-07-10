using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.DataRegistry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconAddInfoOriginalCusEntryHeaderValidationTest : TestCaseWithFactory
	{
		public void TestCheckUS_NAFTAReconIndicator()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				reconOriginalEntry.US_NAFTAReconIndicator = true;
				Assert(reconOriginalEntry.US_NAFTAReconIndicator);
				AssertNoMessageErrorContaining(reconOriginalEntry.US_NAFTAReconIndicatorInfo, ReconAddInfoOriginalCusEntryHeaderValidation.FTAReconFiledFlagCannotBeYes);
				reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.FTA;
				reconOriginalEntry.US_NAFTAReconIndicator = true;
				AssertHasMessageErrorContaining(reconOriginalEntry.US_NAFTAReconIndicatorInfo, ReconAddInfoOriginalCusEntryHeaderValidation.FTAReconFiledFlagCannotBeYes);
			}
		}

		public void TestCheckUS_PaymentDate()
		{
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.ClassRecon;
			reconOriginalEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Tobacco, 1m);
			AssertEquals("ShortPaid", true, reconOriginalEntry.HasBeenShortPaid);
			reconOriginalEntry.US_PaymentDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(reconOriginalEntry.US_PaymentDateInfo, MandatoryValidation.YouHaveNotEntered);
			reconOriginalEntry.US_PaymentDate = ZDateTime.BrettsBirthday;
			AssertNoMessageErrorContaining(reconOriginalEntry.US_PaymentDateInfo, MandatoryValidation.YouHaveNotEntered);
			USCustomsDataRegistry.Instance.ReconInterestRates.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, new ReconInterestRateCollection());
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 2, 29);
			reconOriginalEntry.US_PaymentDate = new ZDateTime(2008, 1, 1);
			AssertHasMessageErrorContaining(reconOriginalEntry.US_PaymentDateInfo, "System needs interest rates from ");
			DeclarationTestHelper.SetReconInterestInRegistry(new ZDate(2008, 1, 1), new ZDate(2008, 2, 29), 9m);
			reconOriginalEntry.US_PaymentDate = new ZDateTime(2008, 1, 1);
			AssertNoMessageErrorContaining(reconOriginalEntry.US_PaymentDateInfo, "System needs interest rates from ");
			AssertNoMessageErrorContaining(reconOriginalEntry.US_PaymentDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.PaymentDateLate);
			reconOriginalEntry.US_PaymentDate = reconDeclaration.US_PreliminaryStatementPrintDate.AddMonths(-21);
			AssertNoMessageErrorContaining(reconOriginalEntry.US_PaymentDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.PaymentDateLate);
			reconOriginalEntry.US_PaymentDate = reconOriginalEntry.US_PaymentDate.AddDays(-1);
			AssertHasMessageErrorContaining(reconOriginalEntry.US_PaymentDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.PaymentDateLate);
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			reconOriginalEntry.US_PaymentDate = reconOriginalEntry.US_PaymentDate.AddDays(-1);
			AssertNoMessageErrorContaining(reconOriginalEntry.US_PaymentDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.PaymentDateLate);
			reconOriginalEntry.US_PaymentDate = ZDateTime.Today.AddYears(-new TypeValidationLimits().PastYearsBeforeError).AddMinutes(-1);
			AssertNoErrors(reconOriginalEntry.US_PaymentDateInfo);
			reconOriginalEntry.US_R_ReleaseDate = ZDateTime.BrettsBirthday;
			reconOriginalEntry.US_PaymentDate = ZDateTime.BrettsBirthday.AddDays(-1);
			AssertHasMessageError(reconOriginalEntry.US_PaymentDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.PaymentDateShouldBeGreaterThanOrEqualToEntryDate);
			reconOriginalEntry.US_R_ReleaseDate = ZDateTime.BrettsBirthday.AddDays(-2);
			AssertNoMessageError(reconOriginalEntry.US_PaymentDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.PaymentDateShouldBeGreaterThanOrEqualToEntryDate);
			reconOriginalEntry.US_PaymentDate = ZDateTime.BrettsBirthday;
			AssertNoMessageError(reconOriginalEntry.US_PaymentDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.PaymentDateShouldBeGreaterThanOrEqualToEntryDate);
			USCustomsDataRegistry.Instance.ReconInterestRates.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, new ReconInterestRateCollection());
			reconOriginalEntry.US_PaymentDate = new ZDate(2008, 2, 26);
			AssertHasMessageErrorContaining(reconOriginalEntry.US_PaymentDateInfo, string.Format(ReconAddInfoOriginalCusEntryHeaderValidation.NotAllInterestRatesExistFromThisDate, reconOriginalEntry.US_PaymentDate.Date.ToShortDateString(), reconOriginalEntry.ReconDeclaration.ReconPaymentDate.ToShortDateString()));
			reconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
			reconDeclaration.US_R_EntrySumDateLodged = ZDateTime.Today.AddDays(-1);
			reconOriginalEntry.US_PaymentDate = ZDateTime.Today.AddDays(-3);
			var erorText = string.Format(ReconAddInfoOriginalCusEntryHeaderValidation.PaymentDateEarlierEntrySumDateLodgedDate, reconDeclaration.US_R_EntrySumDateLodged.ToString());
			AssertHasMessageErrorContaining(reconOriginalEntry.US_PaymentDateInfo, erorText);
			reconOriginalEntry.US_PaymentDate = ZDateTime.Today;
			AssertNoMessageErrorContaining(reconOriginalEntry.US_PaymentDateInfo, erorText);
		}

		public void TestCheckUS_R_OrigCV()
		{
			var errStr = "Please enter an original Customs value for HMF calculation. This is required as this entry has only changed lines entered.";
			reconOriginalEntry.US_R_OrigCV = 0m;
			AssertNoMessageError(reconOriginalEntry.US_R_OrigCVInfo, errStr);
			reconOriginalEntry.US_R_ChangedLinesOnly = true;
			reconOriginalEntry.US_R_OrigCV = 0m;
			AssertNoMessageError(reconOriginalEntry.US_R_OrigCVInfo, errStr);
			reconOriginalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			reconOriginalEntry.US_R_OrigCV = 0m;
			AssertHasMessageError(reconOriginalEntry.US_R_OrigCVInfo, errStr);
			reconOriginalEntry.US_R_OrigCV = 20m;
			AssertNoMessageError(reconOriginalEntry.US_R_OrigCVInfo, errStr);
		}

		public void TestCheckUS_R_DateForMPFCalc()
		{
			reconOriginalEntry.US_R_DateForMPFCalc = ZDateTime.Empty;
			AssertHasMessageError(reconOriginalEntry.US_R_DateForMPFCalcInfo, ReconAddInfoOriginalCusEntryHeaderValidation.MPFDutyRateDateIsRequired);
			reconOriginalEntry.US_R_DateForMPFCalc = ZDateTime.Today.AddDays(-1);
			AssertNoMessageError(reconOriginalEntry.US_R_DateForMPFCalcInfo, ReconAddInfoOriginalCusEntryHeaderValidation.MPFDutyRateDateIsRequired);
			reconDeclaration.US_R_IsNoChangeAgg = true;
			reconOriginalEntry.US_R_DateForMPFCalc = ZDateTime.Empty;
			AssertNoMessageError("Date is not required because no fees calculation", reconOriginalEntry.US_R_DateForMPFCalcInfo, ReconAddInfoOriginalCusEntryHeaderValidation.MPFDutyRateDateIsRequired);
		}

		public void TestCheckUS_R_DutyRateDate()
		{
			reconOriginalEntry.US_R_DutyRateDate = ZDateTime.Empty;
			AssertHasMessageError(reconOriginalEntry.US_R_DutyRateDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.DutyRateDateIsRequired);
			reconOriginalEntry.US_R_DutyRateDate = ZDateTime.BrettsBirthday;
			AssertNoMessageError(reconOriginalEntry.US_R_DutyRateDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.DutyRateDateIsRequired);
			reconOriginalEntry.US_R_DutyRateDate = ZDateTime.Today.AddYears(-new TypeValidationLimits().PastYearsBeforeError).AddMinutes(-1);
			AssertNoErrors(reconOriginalEntry.US_R_DutyRateDateInfo);
			reconOriginalEntry.US_R_ReleaseDate = ZDateTime.BrettsBirthday;
			reconOriginalEntry.US_R_DutyRateDate = ZDateTime.BrettsBirthday.AddDays(1);
			AssertHasWarning(reconOriginalEntry.US_R_DutyRateDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.DutyRateDateNotMatchEntryDate);
			reconOriginalEntry.US_R_ReleaseDate = ZDateTime.Empty;
			AssertNoWarning(reconOriginalEntry.US_R_DutyRateDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.DutyRateDateNotMatchEntryDate);
			reconOriginalEntry.US_R_ReleaseDate = ZDateTime.BrettsBirthday;
			reconOriginalEntry.US_R_DutyRateDate = ZDateTime.Empty;
			AssertNoWarning(reconOriginalEntry.US_R_DutyRateDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.DutyRateDateNotMatchEntryDate);
			reconDeclaration.US_R_IsNoChangeAgg = true;
			reconOriginalEntry.US_R_DutyRateDate = ZDateTime.Empty;
			AssertNoMessageError(reconOriginalEntry.US_R_DutyRateDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.DutyRateDateIsRequired);
		}

		public void TestCheckUS_SchDEntry()
		{
			reconOriginalEntry.US_SchDEntry = ZString.Empty;
			AssertHasMessageErrorContaining(reconOriginalEntry.US_SchDEntryInfo, MandatoryValidation.YouHaveNotEntered);
			reconOriginalEntry.US_SchDEntry = "2210";
			AssertNoMessageErrorContaining(reconOriginalEntry.US_SchDEntryInfo, MandatoryValidation.YouHaveNotEntered);
			reconDeclaration.US_ImportEntrySource = ReconciliationImportEntrySourceList.Codes.PuertoRico;
			reconOriginalEntry.US_SchDEntry = "3901";
			AssertHasMessageErrorContaining(reconOriginalEntry.US_SchDEntryInfo, string.Format(ReconAddInfoOriginalCusEntryHeaderValidation.SourceIndicatorWrong, ReconciliationImportEntrySourceList.Codes.PuertoRico, ReconciliationImportEntrySourceList.Descriptions.PuertoRico));
			reconDeclaration.US_ImportEntrySource = "0"; //Invalid
			reconOriginalEntry.US_SchDEntry = "3901";
			AssertNoMessageErrors(reconOriginalEntry.US_SchDEntryInfo);
		}

		public void TestCheckUS_R_ReleaseDate()
		{
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.Value9802Recon;
			reconOriginalEntry.US_R_ReleaseDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(reconOriginalEntry.US_R_ReleaseDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.EntrySummaryDateRequiredForNonFTARecon);
			reconOriginalEntry.US_R_ReleaseDate = ZDateTime.BrettsBirthday;
			AssertNoMessageErrorContaining(reconOriginalEntry.US_R_ReleaseDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.EntrySummaryDateRequiredForNonFTARecon);
			reconOriginalEntry.US_R_ReleaseDate = ZDateTime.Today.AddYears(-new TypeValidationLimits().PastYearsBeforeError).AddMinutes(-1);
			AssertNoErrors(reconOriginalEntry.US_R_ReleaseDateInfo);
			reconOriginalEntry.US_ImportDate = ZDateTime.BrettsBirthday;
			reconOriginalEntry.US_R_ReleaseDate = ZDateTime.BrettsBirthday.AddDays(-5);
			AssertNoMessageError(reconOriginalEntry.US_R_ReleaseDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.EntryDateShouldNotBeEarlierThan5DaysFromImportDate);
			reconOriginalEntry.US_R_ReleaseDate = ZDateTime.BrettsBirthday.AddDays(-6);
			AssertHasMessageError(reconOriginalEntry.US_R_ReleaseDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.EntryDateShouldNotBeEarlierThan5DaysFromImportDate);
		}

		[TestDate(2011, 02, 15)]
		public void TestCheckUS_ImportDate()
		{
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			reconOriginalEntry.US_ImportDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(reconOriginalEntry.US_ImportDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.ImportDateRequiredForNonFTARecon);
			reconOriginalEntry.US_ImportDate = ZDateTime.BrettsBirthday;
			AssertNoMessageErrorContaining(reconOriginalEntry.US_ImportDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.ImportDateRequiredForNonFTARecon);
			reconOriginalEntry.US_ImportDate = ZDateTime.Today.AddYears(-new TypeValidationLimits().PastYearsBeforeError).AddMinutes(-1);
			AssertNoErrors(reconOriginalEntry.US_ImportDateInfo);
			reconOriginalEntry.US_ImportDate = ZDateTime.BrettsBirthday;
			AssertHasMessageError(reconOriginalEntry.US_ImportDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.ImportDateCannotBeMore1YearForFTARecon);
			reconOriginalEntry.US_ImportDate = new ZDateTime(2010, 01, 13);
			AssertHasMessageError(reconOriginalEntry.US_ImportDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.ImportDateCannotBeMore1YearForFTARecon);
			reconOriginalEntry.US_ImportDate = new ZDateTime(2010, 02, 15);
			AssertNoMessageError(reconOriginalEntry.US_ImportDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.ImportDateCannotBeMore1YearForFTARecon);
			reconOriginalEntry.US_ImportDate = new ZDateTime(2010, 03, 15);
			AssertNoMessageError(reconOriginalEntry.US_ImportDateInfo, ReconAddInfoOriginalCusEntryHeaderValidation.ImportDateCannotBeMore1YearForFTARecon);
		}

		public void TestCheckUS_R_IsHMFApplicable()
		{
			reconOriginalEntry.Invoice.InvoiceLines.AddNew();
			reconOriginalEntry.US_R_IsHMFApplicable = ZString.Empty;
			AssertHasMessageError(reconOriginalEntry.US_R_IsHMFApplicableInfo, ReconAddInfoOriginalCusEntryHeaderValidation.IsHMFApplicableRequired);
			reconOriginalEntry.US_R_IsHMFApplicable = "~";
			AssertNoMessageError(reconOriginalEntry.US_R_IsHMFApplicableInfo, ReconAddInfoOriginalCusEntryHeaderValidation.IsHMFApplicableRequired);
			AssertHasMessageError(reconOriginalEntry.US_R_IsHMFApplicableInfo, ReconAddInfoOriginalCusEntryHeaderValidation.InvalidHMFApplicable);
			reconOriginalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.No;
			AssertNoMessageError(reconOriginalEntry.US_R_IsHMFApplicableInfo, ReconAddInfoOriginalCusEntryHeaderValidation.InvalidHMFApplicable);
			reconOriginalEntry.US_R_NoLineDetails = true;
			reconOriginalEntry.US_R_IsHMFApplicable = ZString.Empty;
			AssertNoMessageError(reconOriginalEntry.US_R_IsHMFApplicableInfo, ReconAddInfoOriginalCusEntryHeaderValidation.IsHMFApplicableRequired);
		}

		public void TestCheckUS_R_MsgMode()
		{
			reconOriginalEntry.US_R_MsgMode = ZString.Empty;
			AssertHasMessageErrorContaining(reconOriginalEntry.US_R_MsgModeInfo, ReconAddInfoOriginalCusEntryHeaderValidation.PleaseEnterMessagingMode);
			reconOriginalEntry.US_R_MsgMode = "~~~";
			AssertNoMessageErrorContaining(reconOriginalEntry.US_R_MsgModeInfo, ReconAddInfoOriginalCusEntryHeaderValidation.PleaseEnterMessagingMode);
			AssertHasMessageErrorContaining(reconOriginalEntry.US_R_MsgModeInfo, ListValidation.InvalidCodeMessageError);
			reconOriginalEntry.US_R_MsgMode = JobApplicationCodeList.Codes.ACE;
			AssertNoMessageErrorContaining(reconOriginalEntry.US_R_MsgModeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_NAFTAClaimStat()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.Class9802Recon;
			var origEntry = reconDec.OriginalEntries.AddNew();
			origEntry.US_NAFTAClaimStat = false;
			AssertNoMessageErrorContaining(origEntry.US_NAFTAClaimStatInfo, ReconAddInfoOriginalCusEntryHeaderValidation.FieldIsNotAllowed("NAFTA 303?"));
			origEntry.US_NAFTAClaimStat = true;
			AssertHasMessageErrorContaining(origEntry.US_NAFTAClaimStatInfo, ReconAddInfoOriginalCusEntryHeaderValidation.FieldIsNotAllowed("NAFTA 303?"));
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			origEntry.US_NAFTAClaimStat = false;
			AssertNoMessageErrorContaining(origEntry.US_NAFTAClaimStatInfo, ReconAddInfoOriginalCusEntryHeaderValidation.FieldIsNotAllowed("NAFTA 303?"));
			origEntry.US_NAFTAClaimStat = true;
			AssertNoMessageErrorContaining(origEntry.US_NAFTAClaimStatInfo, ReconAddInfoOriginalCusEntryHeaderValidation.FieldIsNotAllowed("NAFTA 303?"));
		}

		public void TestCheckUS_ProtestStat()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.Class9802Recon;
			var origEntry = reconDec.OriginalEntries.AddNew();
			origEntry.US_ProtestStat = false;
			AssertNoMessageErrorContaining(origEntry.US_ProtestStatInfo, ReconAddInfoOriginalCusEntryHeaderValidation.FieldIsNotAllowed("Protest Filed?"));
			origEntry.US_ProtestStat = true;
			AssertHasMessageErrorContaining(origEntry.US_ProtestStatInfo, ReconAddInfoOriginalCusEntryHeaderValidation.FieldIsNotAllowed("Protest Filed?"));
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			origEntry.US_ProtestStat = false;
			AssertNoMessageErrorContaining(origEntry.US_ProtestStatInfo, ReconAddInfoOriginalCusEntryHeaderValidation.FieldIsNotAllowed("Protest Filed?"));
			origEntry.US_ProtestStat = true;
			AssertNoMessageErrorContaining(origEntry.US_ProtestStatInfo, ReconAddInfoOriginalCusEntryHeaderValidation.FieldIsNotAllowed("Protest Filed?"));
			origEntry.US_ProtestID = "Protest ID";
			origEntry.US_ProtestStat = false;
			AssertHasMessageErrorContaining(origEntry.US_ProtestStatInfo, ReconAddInfoOriginalCusEntryHeaderValidation.protestFiledMustBeTicked);
			origEntry.US_ProtestStat = true;
			AssertNoMessageErrorContaining(origEntry.US_ProtestStatInfo, ReconAddInfoOriginalCusEntryHeaderValidation.protestFiledMustBeTicked);
		}

		public void TestCheckUS_ProtestID()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_ProtestStat = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders[0];
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			ReconOriginalEntryHeader reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry.CH_OrigEntryReference = "XJ5" + ensEntry.EntryNumber;
			Assert(reconOriginalEntry.US_ProtestStat);
			reconOriginalEntry.US_ProtestID = "1111";
			AssertNoMessageErrorContaining(reconOriginalEntry.US_ProtestIDInfo, MandatoryValidation.YouHaveNotEntered);
			reconOriginalEntry.US_ProtestID = ZString.Empty;
			AssertHasMessageErrorContaining(reconOriginalEntry.US_ProtestIDInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_PendingActionIDType()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.Class9802Recon;
			var origEntry = reconDec.OriginalEntries.AddNew();
			origEntry.US_PendingActionIDType = "1";
			origEntry.US_PendingActionIDType = ZString.Empty;
			AssertHasMessageErrorContaining(origEntry.US_PendingActionIDTypeInfo, MandatoryValidation.YouHaveNotEntered);
			origEntry.US_PendingActionIDType = "S";
			AssertHasMessageError(origEntry.US_PendingActionIDTypeInfo, ListValidation.InvalidCodeMessageError);
			origEntry.US_PendingActionIDType = "A";
			AssertNoMessageErrorContaining(origEntry.US_PendingActionIDTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(origEntry.US_PendingActionIDTypeInfo, MandatoryValidation.YouHaveNotEntered);
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.Value9802Recon;
			origEntry.US_PendingActionIDType = ZString.Empty;
			AssertNoMessageErrorContaining(origEntry.US_PendingActionIDTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(origEntry.US_PendingActionIDTypeInfo, ReconAddInfoOriginalCusEntryHeaderValidation.FieldIsNotAllowed("Pending Action ID"));
			origEntry.US_PendingActionIDType = "A";
			AssertHasMessageErrorContaining(origEntry.US_PendingActionIDTypeInfo, ReconAddInfoOriginalCusEntryHeaderValidation.FieldIsNotAllowed("Pending Action ID"));
		}

		public void TestCheckUS_PendingActionID()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.Class9802Recon;
			var origEntry = reconDec.OriginalEntries.AddNew();
			origEntry.US_PendingActionID = "1";
			origEntry.US_PendingActionID = ZString.Empty;
			AssertHasMessageErrorContaining(origEntry.US_PendingActionIDInfo, MandatoryValidation.YouHaveNotEntered);
			origEntry.US_PendingActionID = "1111";
			AssertNoMessageErrorContaining(origEntry.US_PendingActionIDInfo, MandatoryValidation.YouHaveNotEntered);
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.Value9802Recon;
			origEntry.US_PendingActionID = ZString.Empty;
			AssertNoMessageErrorContaining(origEntry.US_PendingActionIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(origEntry.US_PendingActionIDInfo, ReconAddInfoOriginalCusEntryHeaderValidation.FieldIsNotAllowed("Pending Action ID"));
			origEntry.US_PendingActionID = "1111";
			AssertHasMessageErrorContaining(origEntry.US_PendingActionIDInfo, ReconAddInfoOriginalCusEntryHeaderValidation.FieldIsNotAllowed("Pending Action ID"));
		}

		ReconDeclaration reconDeclaration;
		ReconOriginalEntryHeader reconOriginalEntry;
		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
		}
	}
}
