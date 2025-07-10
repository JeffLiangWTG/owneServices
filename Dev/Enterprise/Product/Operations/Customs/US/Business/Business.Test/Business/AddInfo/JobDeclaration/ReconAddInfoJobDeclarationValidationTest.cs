using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconAddInfoJobDeclarationValidationTest : TestCaseWithFactory
	{
		public void TestCheckIOROrgPK()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ORG" + new Random().Next(1000000).ToString();
			OrgHeaderWrapper iorWrapper = OrgHeaderWrapper.New(importer);
			iorWrapper.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.No;
			string formattedErrorMessage = string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Importer Of Record");
			reconDeclaration.IOROrgPK = importer.PK;
			AssertHasMessageErrorContaining(reconDeclaration.IOROrgPKInfo, formattedErrorMessage);
			ZString eIN = "123";
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, eIN);
			reconDeclaration.ReconWrappedJobDeclaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageErrorContaining(reconDeclaration.IOROrgPKInfo, formattedErrorMessage);
			AssertHasMessageError(reconDeclaration.IOROrgPKInfo, OrganisationValidation.OrganisationShouldBeRegisteredInCustoms);
			iorWrapper.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.Yes;
			reconDeclaration.ReconWrappedJobDeclaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageError(reconDeclaration.IOROrgPKInfo, OrganisationValidation.OrganisationShouldBeRegisteredInCustoms);
			reconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
			ZString erorText = string.Format(ReconAddInfoJobDeclarationValidation.R10DataLodged, eIN, "Importer ID");
			reconDeclaration.ReconWrappedJobDeclaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageError(reconDeclaration.IOROrgPKInfo, erorText);
			importer.CustomsCodes.RemoveAndDeleteAll();
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "547");
			reconDeclaration.US_R_ImporterIDLodged = eIN;
			reconDeclaration.ReconWrappedJobDeclaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageError(reconDeclaration.IOROrgPKInfo, erorText);
			AssertNoMessageError(reconDeclaration.IOROrgPKInfo, "Importer Of Record is required.");
			reconDeclaration.IOROrgPK = ZGuid.Empty;
			AssertHasMessageError(reconDeclaration.IOROrgPKInfo, "Importer Of Record is required.");
			iorWrapper.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.No;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			importer.CustomsCodes.RemoveAndDeleteAll();
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123", Core.Constants.CountryCodes.UnitedStates);
			reconDeclaration.US_R_ImporterIDLodged = eIN;
			reconDeclaration.IOROrgPK = importer.PK;
			AssertNoMessageError(reconDeclaration.IOROrgPKInfo, erorText);
		}

		public void TestCheckUS_IssueCode()
		{
			reconDeclaration.US_IssueCode = "";
			AssertHasMessageErrorContaining(reconDeclaration.US_IssueCodeInfo, MandatoryValidation.YouHaveNotEntered);
			reconDeclaration.US_IssueCode = "891";
			AssertNoMessageErrorContaining(reconDeclaration.US_IssueCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(reconDeclaration.US_IssueCodeInfo, ListValidation.InvalidCodeMessageError);
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.ClassRecon;
			AssertNoMessageErrorContaining(reconDeclaration.US_IssueCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertEquals(0, reconDeclaration.OriginalEntries.Count);
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = true;
			originalEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Duty, 400m);
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Duty, 300m);
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			AssertHasMessageError(reconDeclaration.US_IssueCodeInfo, ReconAddInfoJobDeclarationValidation.FTACannotIncreaseDuties);
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Duty, 500m);
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			AssertNoMessageError(reconDeclaration.US_IssueCodeInfo, ReconAddInfoJobDeclarationValidation.FTACannotIncreaseDuties);
			originalEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.FreshLimes, 350m);
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.FreshLimes, 300m);
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			AssertHasMessageError(reconDeclaration.US_IssueCodeInfo, ReconAddInfoJobDeclarationValidation.FTACannotIncreaseDuties);
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.FreshLimes, 380m);
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			AssertNoMessageError(reconDeclaration.US_IssueCodeInfo, ReconAddInfoJobDeclarationValidation.FTACannotIncreaseDuties);
			originalEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, 250m);
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, 180m);
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			AssertHasMessageError(reconDeclaration.US_IssueCodeInfo, ReconAddInfoJobDeclarationValidation.FTACannotIncreaseDuties);
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, 280m);
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			AssertNoMessageError(reconDeclaration.US_IssueCodeInfo, ReconAddInfoJobDeclarationValidation.FTACannotIncreaseDuties);
		}

		public void TestCheckUS_ImportEntrySource()
		{
			reconDeclaration.US_ImportEntrySource = "";
			AssertHasMessageErrorContaining(reconDeclaration.US_ImportEntrySourceInfo, MandatoryValidation.YouHaveNotEntered);
			reconDeclaration.US_ImportEntrySource = "7";
			AssertNoMessageErrorContaining(reconDeclaration.US_ImportEntrySourceInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(reconDeclaration.US_ImportEntrySourceInfo, ListValidation.InvalidCodeMessageError);
			reconDeclaration.US_ImportEntrySource = ReconciliationImportEntrySourceList.Codes.FiftyStates;
			AssertNoMessageErrorContaining(reconDeclaration.US_ImportEntrySourceInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_SuretyCode()
		{
			reconDeclaration.US_SuretyCode = "";
			AssertHasMessageErrorContaining(reconDeclaration.US_SuretyCodeInfo, MandatoryValidation.YouHaveNotEntered);
			reconDeclaration.US_SuretyCode = "891";
			AssertNoMessageErrorContaining(reconDeclaration.US_SuretyCodeInfo, MandatoryValidation.YouHaveNotEntered);
			OrgHeader ior = Factory.New<OrgHeader>();
			reconDeclaration.IOROrgPK = ior.PK;
			OrgHeaderWrapper iorWrapper = OrgHeaderWrapper.New(reconDeclaration.ImporterOfRecord);
			CusBondDetail oneBondData = iorWrapper.BondDetails.AddNew();
			oneBondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			oneBondData.PW_BondAmount = 50000m;
			oneBondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(1);
			oneBondData.PW_BondNumber = "123456";
			oneBondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			oneBondData.PW_SuretyCode = "891";
			oneBondData.PW_BondFiledPort = "3901";
			oneBondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(2);
			reconDeclaration.US_SuretyCode = "891";
			AssertHasMessageErrorContaining(reconDeclaration.US_SuretyCodeInfo, ReconAddInfoJobDeclarationValidation.BondDataNotEffective);
			oneBondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-1);
			reconDeclaration.US_SuretyCode = "891";
			AssertNoMessageErrorContaining(reconDeclaration.US_SuretyCodeInfo, ReconAddInfoJobDeclarationValidation.BondDataNotEffective);
		}

		public void TestCheckUS_EstimatedEntryDate()
		{
			reconDeclaration.US_EstimatedEntryDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(reconDeclaration.US_EstimatedEntryDateInfo, MandatoryValidation.YouHaveNotEntered);
			reconDeclaration.US_EstimatedEntryDate = ZDateTime.BrettsBirthday;
			AssertNoMessageErrorContaining(reconDeclaration.US_EstimatedEntryDateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(reconDeclaration.US_EstimatedEntryDateInfo, ReconAddInfoJobDeclarationValidation.EstimatedReconDateCannotBePast);
			reconDeclaration.US_EstimatedEntryDate = ZDateTime.Today;
			AssertNoMessageErrorContaining(reconDeclaration.US_EstimatedEntryDateInfo, ReconAddInfoJobDeclarationValidation.EstimatedReconDateCannotBePast);
			reconDeclaration.MessageStatus = ReconMessageStatusList.Codes.ClearReconOriginal;
			AssertEquals("ADD is lodged", false, reconDeclaration.CanSendOriginal);
			reconDeclaration.US_EstimatedEntryDate = ZDateTime.Today.AddDays(-1);
			AssertNoMessageErrorContaining(reconDeclaration.US_EstimatedEntryDateInfo, ReconAddInfoJobDeclarationValidation.EstimatedReconDateCannotBePast);
			reconDeclaration.US_EstimatedEntryDate = new ZDate(2036, 2, 1);
			AssertHasMessageError(reconDeclaration.US_EstimatedEntryDateInfo, ReconAddInfoJobDeclarationValidation.EstimatedReconDateMustHaveInterestRate);
			DeclarationTestHelper.SetReconInterestInRegistry(new ZDate(2036, 1, 1), new ZDate(2036, 2, 29), 9m);
			reconDeclaration.US_EstimatedEntryDate = new ZDate(2036, 2, 1);
			AssertNoMessageError(reconDeclaration.US_EstimatedEntryDateInfo, ReconAddInfoJobDeclarationValidation.EstimatedReconDateMustHaveInterestRate);
		}

		public void TestCheckUS_SchDEntry()
		{
			reconDeclaration.US_SchDEntry = "";
			AssertHasMessageErrorContaining(reconDeclaration.US_SchDEntryInfo, MandatoryValidation.YouHaveNotEntered);
			reconDeclaration.US_SchDEntry = "5";
			AssertNoMessageErrorContaining(reconDeclaration.US_SchDEntryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(reconDeclaration.US_SchDEntryInfo, ListValidation.InvalidCodeMessageError);
			var messageErr = "You have not entered a recon team.";
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDeclaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining(reconDeclaration.US_TeamNoInfo, messageErr);
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			reconDeclaration.US_TeamNo = "";
			reconDeclaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining(reconDeclaration.US_TeamNoInfo, messageErr);
		}

		public void TestCheckUS_TeamNo()
		{
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			reconDeclaration.US_TeamNo = "";
			AssertHasMessageErrorContaining(reconDeclaration.US_TeamNoInfo, MandatoryValidation.YouHaveNotEntered);
			reconDeclaration.US_TeamNo = "1R1";
			AssertNoMessageErrorContaining(reconDeclaration.US_TeamNoInfo, MandatoryValidation.YouHaveNotEntered);
			reconDeclaration.US_SchDEntry = "2402";
			reconDeclaration.US_TeamNo = "1R1";
			AssertHasMessageErrorContaining(reconDeclaration.US_TeamNoInfo, ReconAddInfoJobDeclarationValidation.WrongTeamNoForTheSelectedPort);
			reconDeclaration.US_TeamNo = "6R3";
			AssertNoMessageErrorContaining(reconDeclaration.US_TeamNoInfo, ReconAddInfoJobDeclarationValidation.WrongTeamNoForTheSelectedPort);
			reconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
			reconDeclaration.US_R_TeamNoLodged = "6R3";
			reconDeclaration.US_TeamNo = "6R3";
			ZString erorText = string.Format(ReconAddInfoJobDeclarationValidation.R10DataLodged, reconDeclaration.US_R_TeamNoLodged, "Team No");
			AssertNoMessageErrorContaining(reconDeclaration.US_TeamNoInfo, erorText);
			reconDeclaration.US_TeamNo = "123";
			AssertHasMessageErrorContaining(reconDeclaration.US_TeamNoInfo, erorText);
		}

		public void TestCheckUS_PaymentType()
		{
			reconDeclaration.US_PaymentType = "";
			AssertHasMessageErrorContaining(reconDeclaration.US_PaymentTypeInfo, MandatoryValidation.YouHaveNotEntered);
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			AssertNoMessageErrorContaining(reconDeclaration.US_PaymentTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(reconDeclaration.US_PaymentTypeInfo, ListValidation.InvalidCodeMessageError); //only daily is permitted
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			AssertNoMessageErrorContaining(reconDeclaration.US_PaymentTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_Comment()
		{
			reconDeclaration.US_Comment = "";
			AssertHasMessageErrorContaining(reconDeclaration.US_CommentInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(reconDeclaration.US_CommentInfo, ReconAddInfoJobDeclarationValidation.CommentLengthShouldBeMoreThan5);
			reconDeclaration.US_Comment = "blah";
			AssertNoMessageErrorContaining(reconDeclaration.US_CommentInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(reconDeclaration.US_CommentInfo, ReconAddInfoJobDeclarationValidation.CommentLengthShouldBeMoreThan5);
			reconDeclaration.US_Comment = "blah blah";
			AssertNoMessageErrorContaining(reconDeclaration.US_CommentInfo, ReconAddInfoJobDeclarationValidation.CommentLengthShouldBeMoreThan5);
		}

		public void TestCheckUS_IsAggregate()
		{
			reconDeclaration = new DeclarationTestHelper().GetDutiableReconDeclaration(Factory);
			reconDeclaration.OriginalEntries[0].US_R_DutyRateDate = ZDateTime.Today;
			reconDeclaration.InvoiceLines[0].JI_LinePrice = 2000m; //less to pay
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			reconDeclaration.US_IsAggregate = true;
			AssertHasMessageError(reconDeclaration.US_IsAggregateInfo, ReconAddInfoJobDeclarationValidation.AggregateReconciliationAllowedForIncreaseOrNoChange);
			reconDeclaration.InvoiceLines[0].JI_LinePrice = 3000m; //same
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			reconDeclaration.US_IsAggregate = true;
			AssertNoMessageError(reconDeclaration.US_IsAggregateInfo, ReconAddInfoJobDeclarationValidation.AggregateReconciliationAllowedForIncreaseOrNoChange);
			reconDeclaration.US_IsAggregate = false;
			reconDeclaration.InvoiceLines[0].JI_LinePrice = 2000m; //less to pay
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			reconDeclaration.US_IsAggregate = true;
			AssertHasMessageError(reconDeclaration.US_IsAggregateInfo, ReconAddInfoJobDeclarationValidation.AggregateReconciliationAllowedForIncreaseOrNoChange);
			reconDeclaration.US_R_Waive = true;
			AssertNoMessageError(reconDeclaration.US_IsAggregateInfo, ReconAddInfoJobDeclarationValidation.AggregateReconciliationAllowedForIncreaseOrNoChange);
		}

		public void TestChangeBetweenOriginalAndRecon()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "ACE";
			var recDec = new ReconDeclaration(dec);
			dec.JE_MessageType = JobMessageTypeList.Codes.Recon;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.US_EnableENS = true;
			dec.US_ProtestStat = true;
			var invoiceHeader = dec.Invoices.AddNew();
			dec.RunPreSaveValidation();
			AssertHasMessageError(recDec.US_IsAggregateInfo, ReconAddInfoJobDeclarationValidation.LineHasNoChangeBetweenOriginalAndRecon);
			recDec.US_IsAggregate = true;
			recDec.US_R_IsNoChangeAgg = true;
			dec.RunPreSaveValidation();
			AssertNoNotifications(ReconAddInfoJobDeclarationValidation.LineHasNoChangeBetweenOriginalAndRecon, recDec.US_IsAggregateInfo);
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			AssertEquals("Empty invoiceline should have no changes", 0, recDec.ChangedLines.Count);
			invoiceLine.JI_CustomsQuantity = 1;
			invoiceLine.US_R_OrigFirstQty = 6;
			recDec.US_R_IsNoChangeAgg = false;
			dec.RunPreSaveValidation();
			AssertNoNotifications(ReconAddInfoJobDeclarationValidation.LineHasNoChangeBetweenOriginalAndRecon, recDec.US_IsAggregateInfo);
			invoiceLine.JI_CustomsQuantity = 6;
			invoiceLine.US_R_OrigFirstQty = 6;
			recDec.US_R_IsNoChangeAgg = false;
			dec.RunPreSaveValidation();
			AssertHasMessageError(recDec.US_IsAggregateInfo, ReconAddInfoJobDeclarationValidation.LineHasNoChangeBetweenOriginalAndRecon);
		}

		public void TestCheckUS_PreliminaryStatementPrintDate()
		{
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDateTime(2006, 1, 14); // Saturday
			AssertHasMessageErrorContaining(reconDeclaration.US_PreliminaryStatementPrintDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend);
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDateTime(2006, 1, 10); // Tuesday
			AssertNoMessageErrorContaining(reconDeclaration.US_PreliminaryStatementPrintDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend);
			//holidays (2010, 02, 15)
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDateTime(2010, 02, 15);
			AssertHasMessageErrorContaining(reconDeclaration.US_PreliminaryStatementPrintDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday);
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDateTime(2010, 02, 16);
			AssertNoMessageErrorContaining(reconDeclaration.US_PreliminaryStatementPrintDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday);
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDateTime(2010, 02, 16);
			AssertHasMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintNotRequired);
			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			AssertNoMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintNotRequired);
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			AssertHasMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateRequired);
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertNoMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateRequired);
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(95);
			AssertHasMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateMoreThan90DaysInTheFuture);
			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(1);
			AssertNoMessageError(reconDeclaration.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PreliminaryPrintDateMoreThan90DaysInTheFuture);
		}

		public void TestUS_ClientBranchDesignation()
		{
			var branchPK = reconDeclaration.Branch != null ? reconDeclaration.Branch.PK.ToGuid() : GlbBranch.CurrentBranch.PK.ToGuid();
			var oldClientRegistryBranchDesignation = USCustomsDataRegistry.Instance.ClientBranchDesignation.GetFallBackValueAtAllLevels(Guid.Empty, branchPK, GlbDepartment.CurrentDepartment.PK.ToGuid());
			var newClientRegistryBranchDesignation = "11";
			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, branchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), newClientRegistryBranchDesignation);
			reconDeclaration.US_ClientBranchDesignation = "15";
			AssertHasMessageErrorContaining("Client Branch Designation should have message error because value differs from registry.", reconDeclaration.US_ClientBranchDesignationInfo, string.Format(ValidationConstants.ClientBranchDesignationNotMatchRegistrySetting, newClientRegistryBranchDesignation));
			reconDeclaration.US_ClientBranchDesignation = newClientRegistryBranchDesignation;
			AssertNoMessageErrorContaining("Client Branch Designation should not have message error because value from registry.", reconDeclaration.US_ClientBranchDesignationInfo, string.Format(ValidationConstants.ClientBranchDesignationNotMatchRegistrySetting, newClientRegistryBranchDesignation));
			reconDeclaration.US_ClientBranchDesignation = "";
			AssertNoMessageErrorContaining("Client Branch Designation should not have message error because value of US_ClientBranchDesignation is empty.", reconDeclaration.US_ClientBranchDesignationInfo, string.Format(ValidationConstants.ClientBranchDesignationNotMatchRegistrySetting, newClientRegistryBranchDesignation));
			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, branchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), "");
			reconDeclaration.US_ClientBranchDesignation = "~";
			AssertNoMessageErrorContaining("Client Branch Designation should not have message error because value from registry is empty.", reconDeclaration.US_ClientBranchDesignationInfo, string.Format(ValidationConstants.ClientBranchDesignationNotMatchRegistrySetting, newClientRegistryBranchDesignation));
			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, branchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), oldClientRegistryBranchDesignation);
		}

		public void TestCheckUS_DocProvidedDate()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			reconDec.SummaryDocRecipientAddress.E2_AddressOverride = true;
			reconDec.SummaryDocRecipientAddress.E2_Address1 = "AAA";
			reconDec.US_DocProvidedDate = ZDate.Today;
			AssertNoMessageErrorContaining(reconDec.US_DocProvidedDateInfo, MandatoryValidation.YouHaveNotEntered);
			reconDec.US_DocProvidedDate = ZDate.Empty;
			AssertHasMessageErrorContaining(reconDec.US_DocProvidedDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
		}

		ReconDeclaration reconDeclaration;
	}
}
