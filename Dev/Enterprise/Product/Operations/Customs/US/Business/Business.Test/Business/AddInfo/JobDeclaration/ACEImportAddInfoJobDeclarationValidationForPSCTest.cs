using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEImportAddInfoJobDeclarationValidationForPSCTest : TestCaseWithFactory
	{
		public void TestCheckUS_EntryTypeForInformal()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertHasMessageError(declaration.US_EntryTypeInfo, ValidationConstants.PSC.EntrySummaryWasFiledAsInformalEntry);
		}

		public void TestCheckUS_NAFTAReconIndicator()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_NAFTAReconIndicator = true;
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			declaration.US_NAFTAReconIndicator = false;
			AssertHasMessageErrorContaining(declaration.US_NAFTAReconIndicatorInfo, ValidationConstants.PSC.NotAllowedToChangeReconIndicator);
			declaration.US_NAFTAReconIndicator = true;
			AssertNoMessageErrorContaining(declaration.US_NAFTAReconIndicatorInfo, ValidationConstants.PSC.NotAllowedToChangeReconIndicator);
			declaration = GetMergibleDeclaration();
			declaration.US_NAFTAReconIndicator = false;
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			declaration.US_NAFTAReconIndicator = true;
			AssertHasMessageErrorContaining(declaration.US_NAFTAReconIndicatorInfo, ValidationConstants.PSC.NotAllowedToChangeReconIndicator);
			declaration.US_NAFTAReconIndicator = false;
			AssertNoMessageErrorContaining(declaration.US_NAFTAReconIndicatorInfo, ValidationConstants.PSC.NotAllowedToChangeReconIndicator);
		}

		public void TestCheckOtherReconIndicator()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes._9802Recon;
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes._9802Recon;
			AssertNoMessageErrorContaining(declaration.US_OtherReconIndicatorInfo, ValidationConstants.PSC.NotAllowedToChangeReconIndicator);
			declaration.US_OtherReconIndicator = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_OtherReconIndicatorInfo, ValidationConstants.PSC.NotAllowedToChangeReconIndicator);
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.Class9802Recon;
			AssertHasMessageErrorContaining(declaration.US_OtherReconIndicatorInfo, ValidationConstants.PSC.NotAllowedToChangeReconIndicator);
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.NotApplicable; //005 was sent previously and NA is the difference, should be a message error
			AssertHasMessageErrorContaining(declaration.US_OtherReconIndicatorInfo, ValidationConstants.PSC.NotAllowedToChangeReconIndicator);
			declaration = GetMergibleDeclaration();
			declaration.US_OtherReconIndicator = ZString.Empty;
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			AssertHasMessageErrorContaining(declaration.US_OtherReconIndicatorInfo, ValidationConstants.PSC.NotAllowedToChangeReconIndicator);
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.NotApplicable;
			AssertNoMessageErrorContaining(declaration.US_OtherReconIndicatorInfo, ValidationConstants.PSC.NotAllowedToChangeReconIndicator);
			declaration.US_OtherReconIndicator = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.US_OtherReconIndicatorInfo, ValidationConstants.PSC.NotAllowedToChangeReconIndicator);
		}

		public void TestCheckUS_EntryType()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertHasMessageError(declaration.US_EntryTypeInfo, ValidationConstants.PSC.CannotChangeEntryTypeToADD_CVDTypeForPSC);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa;
			AssertNoMessageError(declaration.US_EntryTypeInfo, ValidationConstants.PSC.CannotChangeEntryTypeToADD_CVDTypeForPSC);
			declaration = GetMergibleDeclaration();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertNoMessageError(declaration.US_EntryTypeInfo, ValidationConstants.PSC.CannotChangeEntryTypeToADD_CVDTypeForPSC);
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			AssertHasMessageError(declaration.US_EntryTypeInfo, ValidationConstants.PSC.EntryTypeCannotBeChangedToTIBForPSC);
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_EntryType = EntryTypeList.Codes.AircraftVesselSupplyIE;
			AssertHasMessageError(declaration.US_EntryTypeInfo, ValidationConstants.PSC.EntryTypeCannotBeChangedFromTIBForPSC);
		}

		public void TestCheckIOROrgPK()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_CertifyCargoRelease = true;
			var organisation = Factory.New<OrgHeader>();
			organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			var organisation2 = Factory.New<OrgHeader>();
			organisation2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199001");
			declaration.IOROrgPK = organisation.PK;
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			declaration.IOROrgPK = organisation2.PK;
			AssertHasMessageError(declaration.IOROrgPKInfo, ValidationConstants.PSC.NotAllowedToChangeIORNumber);
			declaration.IOROrgPK = organisation.PK;
			AssertNoMessageError(declaration.IOROrgPKInfo, ValidationConstants.PSC.NotAllowedToChangeIORNumber);
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			declaration.IOROrgPK = organisation.PK;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			var errorMsg = "USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact.";
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			declaration.IOROrgPK = organisation.PK;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			declaration.IOROrgPK = organisation.PK;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			declaration.IOROrgPK = organisation.PK;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCAODSCertIndividual = PartyTypeList.Codes.Importer;
			declaration.IOROrgPK = organisation.PK;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
			invoiceLine.US_TSCAODSCertIndividual = PartyTypeList.Codes.CustomsBroker;
			declaration.IOROrgPK = organisation.PK;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			declaration.IOROrgPK = organisation.PK;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			declaration.IOROrgPK = organisation.PK;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
			var nhtsa = invoiceLine.NHTSALines.AddNew();
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
			var document = nhtsa.NHTSADocuments.AddNew();
			document.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.Importer;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
			nhtsa.NHTSADocuments.RemoveAndDeleteAll();
			nhtsa.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
			invoiceLine.NHTSALines.RemoveAndDeleteAll();
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			declaration.IOROrgPK = organisation.PK;
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
			DeclarationTestHelper.AddPGAContact(organisation, "AAAA", null, null, null, null);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
			DeclarationTestHelper.AddPGAContact(organisation, null, "BBBB", null, null, null);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
			DeclarationTestHelper.AddPGAContact(organisation, null, null, "1234567", null, null);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
			DeclarationTestHelper.AddPGAContact(organisation, null, null, null, null, "23456");
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageErrorContaining(declaration.IOROrgPKInfo, errorMsg);
		}

		public void TestCheckUS_SchDEntry()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_SchDEntry = "3901";
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			declaration.US_SchDEntry = "3902";
			AssertHasMessageError(declaration.US_SchDEntryInfo, ValidationConstants.PSC.NotAllowedToChangeEntryPort);
			declaration.US_SchDEntry = "3901";
			AssertNoMessageError(declaration.US_EntryTypeInfo, ValidationConstants.PSC.NotAllowedToChangeEntryPort);
		}

		public void TestCheckUS_AccLiqReq()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_PSC = true;
			declaration.US_AccLiqReq = true;
			AssertHasMessageError(declaration.US_AccLiqReqInfo, ValidationConstants.PSC.AccLiqRequestNotAllowedForAD_CVDEntry);
			declaration.US_AccLiqReq = false;
			AssertNoMessageError(declaration.US_AccLiqReqInfo, ValidationConstants.PSC.AccLiqRequestNotAllowedForAD_CVDEntry);
		}

		public void TestCheckUS_PSCStatementHasZeroAmountDue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.ImportEntryNumber = "1";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementAmount = 0;
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "1";
			Factory.Save();
			AssertNotNull(declaration.RelatedStatement);
			declaration.US_PSC = true;
			AssertNoMessageError(declaration.US_PSCInfo, ValidationConstants.PSC.MayFilePSCOnlyWhenPeriodicStatementIsFinalised);
			statement.B2_StatementAmount = 10;
			Factory.Save();
			declaration.US_PSC = true;
			AssertHasMessageError(declaration.US_PSCInfo, ValidationConstants.PSC.MayFilePSCOnlyWhenPeriodicStatementIsFinalised);
		}

		public void TestCheckUS_PSCDate()
		{
			var declaration = GetMergibleDeclaration();
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(-(ValidationConstants.PSC.MaxDaysFromEntryDateToFilePSC + 1));
			declaration.US_PSC = true;
			AssertHasMessageError(declaration.US_PSCInfo, string.Format(ValidationConstants.PSC.MayFilePSCUpTo300DaysFromEntryDate, ValidationConstants.PSC.MaxDaysFromEntryDateToFilePSC));
			declaration.US_PSC = false;
			AssertNoMessageError(declaration.US_PSCInfo, string.Format(ValidationConstants.PSC.MayFilePSCUpTo300DaysFromEntryDate, ValidationConstants.PSC.MaxDaysFromEntryDateToFilePSC));
		}

		public void TestCheckUS_PSCForRelatedStatement()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			AssertHasMessageError(declaration.US_PSCInfo, ValidationConstants.PSC.MayFilePSCOnlyWhenEntryOnStatement);
			Factory.Save();
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			statement.B2_StatementAmount = 10;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = declaration.ImportEntryNumber;
			statementLine.B3_EntryFilerCode = declaration.US_EntryFilerCode;
			AssertNotNull(statementLine.Declaration);
			AssertNotNull(declaration.RelatedStatement);
			declaration.US_PSC = true;
			AssertNoMessageError(declaration.US_PSCInfo, ValidationConstants.PSC.MayFilePSCOnlyWhenEntryOnStatement);
			AssertHasMessageError(declaration.US_PSCInfo, ValidationConstants.PSC.MayFilePSCOnlyWhenPeriodicStatementIsFinalised);
			var monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1234P";
			monthlyStatement.B2_Status = StatementHeaderStatusList.Codes.Final;
			statement.B2_B2_PeriodicStatement = monthlyStatement.PK;
			declaration.US_PSC = true;
			statement.B2_StatementAmount = 0;
			AssertNoMessageError(declaration.US_PSCInfo, ValidationConstants.PSC.MayFilePSCOnlyWhenPeriodicStatementIsFinalised);
		}

		public void TestCheckUS_PSCForEntryType()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_PSC = true;
			AssertHasMessageError(declaration.US_EntryTypeInfo, ValidationConstants.PSC.NotAllowedForInformalEntry);
			declaration.US_PSC = false;
			AssertNoMessageError(declaration.US_EntryTypeInfo, ValidationConstants.PSC.NotAllowedForInformalEntry);
		}

		public void TestEntryDateElectionCodeHasChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.ArrivalDate;
			Factory.Save();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			AssertHasErrorContaining(declaration.US_EntryDateElectionCodeInfo, JobDeclarationValidation.WarehouseTransactionExistsNeedsCancel);
		}

		public void TestCheckUS_PaymentType()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertHasMessageErrorContaining(declaration.US_PaymentTypeInfo, ValidationConstants.PSC.NotAllowedToChangePaymentType);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			AssertNoMessageErrorContaining(declaration.US_PaymentTypeInfo, ValidationConstants.PSC.NotAllowedToChangePaymentType);
		}

		[TestDate(2013, 01, 14, 11, 35, 17)]
		public void TestCheckUS_PSD()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(1);
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.Messages[0].EM_SystemCreateTimeUtc = ZDateTime.Today;
			entry.Messages[1].EM_SystemCreateTimeUtc = ZDateTime.Today;
			declaration.US_PSC = true;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(2);
			AssertHasMessageErrorContaining(declaration.US_PreliminaryStatementPrintDateInfo, ValidationConstants.PSC.NotAllowedToChangePSD);
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(1);
			AssertNoMessageErrorContaining(declaration.US_PreliminaryStatementPrintDateInfo, ValidationConstants.PSC.NotAllowedToChangePSD);
			declaration.Messages.Add(DeclarationTestHelper.CreateTransmittedSTUMsg(Factory, "~YEDUSCMT_142371", new ZDateTime(2013, 01, 29), "B011101SV9HP                                               ~YEDUSCMT_142371     H1101SV9 710004812012913                                                        Y  1101SV9HP00001"));
			declaration.Messages.Add(DeclarationTestHelper.CreateIncomingSTUMsg(Factory, "~YEDUSCMT_142371", new ZDateTime(2013, 01, 29), "B011101SV9HT                                               ~YEDUSCMT_142371     H11101SV9 710004812GBDATA REPLACED AS REQUESTED               2012913B00159398  Y  1101SV9HT00001"));
			declaration.EntryStatusesAndErrors.RefreshPSCEntryDataForTesting();
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2013, 01, 29);
			AssertNoMessageErrorContaining(declaration.US_PreliminaryStatementPrintDateInfo, ValidationConstants.PSC.NotAllowedToChangePSD);
		}

		public void TestCheckUS_PSDMonth()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(1);
			declaration.US_PeriodicStatementMM = "01";
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			declaration.US_PeriodicStatementMM = "02";
			AssertHasMessageErrorContaining(declaration.US_PeriodicStatementMMInfo, ValidationConstants.PSC.NotAllowedToChangeStatementMonth);
			declaration.US_PeriodicStatementMM = "01";
			AssertNoMessageErrorContaining(declaration.US_PeriodicStatementMMInfo, ValidationConstants.PSC.NotAllowedToChangeStatementMonth);
		}

		public void TestUS_ClientBranchDesignation()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(1);
			declaration.US_ClientBranchDesignation = "01";
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			declaration.US_ClientBranchDesignation = "02";
			AssertHasMessageErrorContaining(declaration.US_ClientBranchDesignationInfo, ValidationConstants.PSC.NotAllowedToChangeClientBranchDesig);
			declaration.US_ClientBranchDesignation = "01";
			AssertNoMessageErrorContaining(declaration.US_ClientBranchDesignationInfo, ValidationConstants.PSC.NotAllowedToChangeClientBranchDesig);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, "27");
			declaration.JE_GB = branch.PK;
			declaration.US_ClientBranchDesignation = "28";
			AssertHasMessageErrorContaining(declaration.US_ClientBranchDesignationInfo, "Client Branch Designation does not match the setting in the registry.  Should be 27.");
			declaration.US_ClientBranchDesignation = "27";
			AssertNoMessageErrorContaining(declaration.US_ClientBranchDesignationInfo, "Client Branch Designation does not match the setting in the registry.  Should be 27.");
		}

		public void TestLocationOfGoods()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_US_NKLocationOfGoods = "A001";
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			declaration.US_US_NKLocationOfGoods = "A002";
			AssertHasMessageErrorContaining(declaration.US_US_NKLocationOfGoodsInfo, ValidationConstants.PSC.NotAllowedToChangeLocationOfGoods);
			declaration.US_US_NKLocationOfGoods = "A001";
			AssertNoMessageErrorContaining(declaration.US_US_NKLocationOfGoodsInfo, ValidationConstants.PSC.NotAllowedToChangeLocationOfGoods);
		}

		public void TestCheckUS_LiveEntryIndicator()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_LiveEntryIndicator = ZString.Empty;
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			AssertHasMessageErrorContaining(declaration.US_LiveEntryIndicatorInfo, ValidationConstants.PSC.NotAllowedToChangeLiveIndicator);
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			AssertNoMessageErrorContaining(declaration.US_LiveEntryIndicatorInfo, ValidationConstants.PSC.NotAllowedToChangeLiveIndicator);
		}

		public void TestCheckUS_ConsolACE()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_ConsolACE = true;
			MergeAndSendDeclarationWithSuccessResponse(declaration);
			declaration.US_PSC = true;
			declaration.US_ConsolACE = false;
			AssertHasMessageErrorContaining(declaration.US_ConsolACEInfo, ValidationConstants.PSC.NotAllowedToChangeConsolidated);
			declaration.US_ConsolACE = true;
			AssertNoMessageErrorContaining(declaration.US_ConsolACEInfo, ValidationConstants.PSC.NotAllowedToChangeConsolidated);
		}

		public void TestCheckUS_ITDate()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_ITDate = ZDateTime.Today;
			AssertNoWarningContaining(declaration.US_ITDateInfo, ACEImportAddInfoJobDeclarationValidation.ITDateNotRequiredForNonAMSJob);
			declaration.US_NonAMS = true;
			declaration.AddInfoValidation.ValidateUS_ITDate();
			AssertHasWarningContaining(declaration.US_ITDateInfo, ACEImportAddInfoJobDeclarationValidation.ITDateNotRequiredForNonAMSJob);
		}

		public void TestCheckUS_SPNIDType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableSPN = true;
			declaration.US_SPNIDType = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_SPNIDTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_SPNIDTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_SPNIDType = "~";
			AssertNoMessageErrorContaining(declaration.US_SPNIDTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_SPNIDTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.ENT;
			AssertNoMessageErrorContaining(declaration.US_SPNIDTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(declaration.US_SPNIDTypeInfo, ValidationConstants.PriorNotice.EntryNumberRequiredForENTStandAlonePriorNotice);
			declaration.ImportEntryNumber = "12345678";
			declaration.AddInfoValidation.ValidateUS_SPNIDType();
			AssertNoMessageErrorContaining(declaration.US_SPNIDTypeInfo, ValidationConstants.PriorNotice.EntryNumberRequiredForENTStandAlonePriorNotice);
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			AssertHasMessageErrorContaining(declaration.US_SPNIDTypeInfo, ValidationConstants.PriorNotice.MasterBillNumberRequiredForBLNStandAlonePriorNotice);
			declaration.JE_MasterBill = "MB11111";
			declaration.AddInfoValidation.ValidateUS_SPNIDType();
			AssertNoMessageErrorContaining(declaration.US_SPNIDTypeInfo, ValidationConstants.PriorNotice.MasterBillNumberRequiredForBLNStandAlonePriorNotice);
		}

		public void TestCheckUS_EntryType_InformalEntryNotification()
		{
			var declaration = GetMergibleDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_Tariff = UsualTariffForCheckUS_EntryTypeTest;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoWarning(declaration.US_EntryTypeInfo, ACEImportAddInfoJobDeclarationValidation.PossibleInformalEntry);
			invoiceLine.JI_LinePrice = FormalImportAddInfoJobDeclarationValidation.InformalFreeDutiableMaxValueAfter2013Jan7 - 1;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertHasWarning(declaration.US_EntryTypeInfo, ACEImportAddInfoJobDeclarationValidation.PossibleInformalEntry);
			invoiceLine.JI_LinePrice += 5;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoWarning(declaration.US_EntryTypeInfo, ACEImportAddInfoJobDeclarationValidation.PossibleInformalEntry);
		}

		public void TestCheckUS_EntryType_InformalEntryNotification_Section301Or232()
		{
			var declaration = GetMergibleDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.US_SupTariff = "99011001";
			invoiceLine.JI_LinePrice = FormalImportAddInfoJobDeclarationValidation.InformalFreeDutiableMaxValueAfter2013Jan7 - 1;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertHasWarning(declaration.US_EntryTypeInfo, ACEImportAddInfoJobDeclarationValidation.PossibleInformalEntry);
			invoiceLine.US_SupTariff = "99031001";
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoWarning(declaration.US_EntryTypeInfo, ACEImportAddInfoJobDeclarationValidation.PossibleInformalEntry);
		}

		public void TestCheckUS_EntryType_InformalEntryNotification_Duties()
		{
			var declaration = GetMergibleDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_Tariff = UsualTariffForCheckUS_EntryTypeTest;
			invoiceLine.JI_LinePrice = FormalImportAddInfoJobDeclarationValidation.InformalFreeDutiableMaxValueAfter2013Jan7 - 1;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertHasWarning(declaration.US_EntryTypeInfo, ACEImportAddInfoJobDeclarationValidation.PossibleInformalEntry);
			invoiceLine.US_ADDCaseNo = "7839028";
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoWarning(declaration.US_EntryTypeInfo, ACEImportAddInfoJobDeclarationValidation.PossibleInformalEntry);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoWarning(declaration.US_EntryTypeInfo, ACEImportAddInfoJobDeclarationValidation.PossibleInformalEntry);
			invoiceLine.US_ADDCaseNo = "";
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertHasWarning(declaration.US_EntryTypeInfo, ACEImportAddInfoJobDeclarationValidation.PossibleInformalEntry);
			invoiceLine.US_ADDCaseNo = "19342789";
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoWarning(declaration.US_EntryTypeInfo, ACEImportAddInfoJobDeclarationValidation.PossibleInformalEntry);
		}

		public void TestCheckUS_EntryType_InformalEntryNotification_ReturnedGoods()
		{
			var declaration = GetMergibleDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_Tariff = GoodsReturnedTariffForCheckUS_EntryTypeTest;
			invoiceLine.JI_LinePrice = FormalImportAddInfoJobDeclarationValidation.AmericanGoodsReturnedFreeDutiableMaxValue - 1;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertHasWarning(declaration.US_EntryTypeInfo, ACEImportAddInfoJobDeclarationValidation.PossibleInformalEntry);
			invoiceLine.JI_LinePrice += 5;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoWarning(declaration.US_EntryTypeInfo, ACEImportAddInfoJobDeclarationValidation.PossibleInformalEntry);
		}

		public void TestCheckUS_EntryType_InformalEntryNotification_ErrorMessage()
		{
			var declaration = GetMergibleDeclaration();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			declaration.JE_GB = branch.PK;
			DataRegistry.Business.USCustomsDataRegistry.Instance.InformalImportWarningOrError.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, ErrorWarningInformalImport.Codes.ERR);
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_Tariff = UsualTariffForCheckUS_EntryTypeTest;
			invoiceLine.JI_LinePrice = FormalImportAddInfoJobDeclarationValidation.InformalFreeDutiableMaxValueAfter2013Jan7 - 1;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, ACEImportAddInfoJobDeclarationValidation.PossibleInformalEntry);
			invoiceLine.JI_LinePrice += 5;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, ACEImportAddInfoJobDeclarationValidation.PossibleInformalEntry);
		}

		public void TestCheckUS_EntryType_LowValue()
		{
			var declaration = GetMergibleDeclaration();
			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_Tariff = UsualTariffForCheckUS_EntryTypeTest;
			var deminimus = FeeCalculationHelper.GetDeminimus(Factory);
			invoiceLine.JI_LinePrice = deminimus - 10;
			var messageErrorText = string.Join(ACEImportAddInfoJobDeclarationValidation.CustomsValueExceedMaxminLowValue, deminimus);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, messageErrorText);
			invoiceLine.JI_LinePrice = deminimus + 10;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, messageErrorText);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, messageErrorText);
			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			declaration.JE_MasterBill = "MB129832184";
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, string.Format(US.Business.BillValidator.Constants.OnlyOneBillAllowedForLowValueEntries, BillTypeList.Descriptions.MasterBill));
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "812837181";
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, string.Format(US.Business.BillValidator.Constants.OnlyOneBillAllowedForLowValueEntries, BillTypeList.Descriptions.MasterBill));
			declaration.JE_HouseBill = "HB91029745";
			masterBill.CU_BillType = BillTypeList.Codes.HouseBill;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, string.Format(US.Business.BillValidator.Constants.OnlyOneBillAllowedForLowValueEntries, BillTypeList.Descriptions.HouseBill));
			masterBill.Delete();
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, string.Format(US.Business.BillValidator.Constants.OnlyOneBillAllowedForLowValueEntries, BillTypeList.Descriptions.HouseBill));
		}

		public void TestCheckIOROrgPK_LowValue()
		{
			var message = "You have not entered an IOR";
			var declaration = GetMergibleDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			line.US_TTBInd = "D";
			line.TTBLines.AddNew();
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, message);
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "TEST";
			declaration.IOROrgPK = organization.PK;
			organization.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123");
			AssertNoMessageErrorContaining(declaration.IOROrgPKInfo, message);
			declaration.IOROrgPK = ZGuid.Empty;
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			declaration.PGAFlags.RefreshInvoiceLinesWithPGAIndicators();
			declaration.Invoices[0].RefreshInvoiceLinesWithPGAIndicators();
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, message);
			invoiceLine.ACE_FDALines.AddNew();
			declaration.PGAFlags.RefreshInvoiceLinesWithPGAIndicators();
			declaration.Invoices[0].RefreshInvoiceLinesWithPGAIndicators();
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, message);
			declaration.IOROrgPK = organization.PK;
			AssertNoMessageErrorContaining(declaration.IOROrgPKInfo, message);
		}

		public void TestCheckUS_EnableENS()
		{
			var declaration = GetMergibleDeclaration();
			declaration.AddInfoValidation.ValidateUS_EnableENS();
			AssertNoMessageErrorContaining(declaration.US_EnableENSInfo, ACEImportAddInfoJobDeclarationValidation.EntrySummaryNotRequired);
			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			declaration.US_EnableENS = true;
			declaration.AddInfoValidation.ValidateUS_EnableENS();
			AssertHasMessageErrorContaining(declaration.US_EnableENSInfo, ACEImportAddInfoJobDeclarationValidation.EntrySummaryNotRequired);
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.AddInfoValidation.ValidateUS_EnableENS();
			AssertNoMessageErrorContaining(declaration.US_EnableENSInfo, ACEImportAddInfoJobDeclarationValidation.EntrySummaryNotRequired);
		}

		public void TestUS_GoodsFromFTZ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "A00#", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.City, "ABERDEEN");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_GoodsFromFTZ = "~";
			AssertHasMessageErrorContaining(declaration.US_GoodsFromFTZInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_GoodsFromFTZ = "A00#";
			AssertNoMessageErrorContaining(declaration.US_GoodsFromFTZInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_CertifyCargoRelease()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_PreparerDistrictPort = "3901";
			declaration.US_EntryType = "";
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			AssertHasMessageError(declaration.US_CertifyCargoReleaseInfo, ACEImportAddInfoJobDeclarationValidation.ShouldNotTickCertifyCargoRelease);
			declaration.US_EnableCRL = false;
			AssertNoMessageError(declaration.US_CertifyCargoReleaseInfo, ACEImportAddInfoJobDeclarationValidation.ShouldNotTickCertifyCargoRelease);
		}

		public void TestUS_UI_NKCarrierSCAC()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_UI_NKCarrierSCAC = "";
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			declaration.AddInfoValidation.ValidateUS_UI_NKCarrierSCAC();
			AssertNoMessageError("US_UI_NKCarrierSCAC can not be empty only applicable to BWB mode", declaration.US_UI_NKCarrierSCACInfo, MandatoryValidation.YouHaveNotEnteredMessage(declaration.US_UI_NKCarrierSCACInfo.HumanReadableName));
			declaration.US_UI_NKCarrierSCAC = "UNKN";
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			declaration.AddInfoValidation.ValidateUS_UI_NKCarrierSCAC();
			AssertNoMessageError("US_UI_NKCarrierSCAC is valid", declaration.US_UI_NKCarrierSCACInfo, MandatoryValidation.YouHaveNotEnteredMessage(declaration.US_UI_NKCarrierSCACInfo.HumanReadableName));
			declaration.US_UI_NKCarrierSCAC = "UNKN";
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = false;
			declaration.AddInfoValidation.ValidateUS_UI_NKCarrierSCAC();
			AssertHasMessageError("Use ZZZZ For Unknown CarrierSCAC", declaration.US_UI_NKCarrierSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);
			declaration.JE_TransportMode = Enterprise.Customs.US.Business.TransportTypeList.Codes.BorderWaterBorne;
			declaration.US_UI_NKCarrierSCAC = "";
			declaration.US_EnableCRL = false;
			AssertHasMessageError("US_UI_NKCarrierSCAC can not be empty", declaration.US_UI_NKCarrierSCACInfo, MandatoryValidation.YouHaveNotEnteredMessage(declaration.US_UI_NKCarrierSCACInfo.HumanReadableName));
			declaration.US_UI_NKCarrierSCAC = "UNKN";
			declaration.US_EnableCRL = true;
			declaration.AddInfoValidation.ValidateUS_UI_NKCarrierSCAC();
			AssertHasMessageError("Use ZZZZ For Unknown CarrierSCAC", declaration.US_UI_NKCarrierSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);
			declaration.US_UI_NKCarrierSCAC = "ZZZZ";
			AssertNoMessageError("Use ZZZZ For Unknown CarrierSCAC", declaration.US_UI_NKCarrierSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);
			declaration.US_UI_NKCarrierSCAC = "UNKN";
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = true;
			declaration.AddInfoValidation.ValidateUS_UI_NKCarrierSCAC();
			AssertHasMessageError("Use ZZZZ For Unknown CarrierSCAC", declaration.US_UI_NKCarrierSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);
			declaration.US_UI_NKCarrierSCAC = "ZZZZ";
			AssertNoMessageError("Use ZZZZ For Unknown CarrierSCAC", declaration.US_UI_NKCarrierSCACInfo, ValidationConstants.UseZZZZForUnknownCarrierSCAC);
		}

		public void TestBDRefNoValidationCheck()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Factory.Save();
			declaration.US_EntryFilerCode = "AAA";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EntryFiler()
			{ EntryFilerCode = "SV9" });
			declaration.US_PSC = true;
			declaration.US_BRDRefNo = "";
			AssertHasWarning(declaration.US_BRDRefNoInfo, ACEImportAddInfoJobDeclarationValidation.BrokerReferenceWillBeSentEmptyInPSCMessage);
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_BRDRefNo = "";
			AssertNoWarning(declaration.US_BRDRefNoInfo, ACEImportAddInfoJobDeclarationValidation.BrokerReferenceWillBeSentEmptyInPSCMessage);
			declaration.US_BRDRefNo = "123456";
			AssertNoWarning(declaration.US_BRDRefNoInfo, ACEImportAddInfoJobDeclarationValidation.BrokerReferenceWillBeSentEmptyInPSCMessage);
			declaration.US_EntryFilerCode = "AAA";
			declaration.US_BRDRefNo = "12345";
			AssertNoWarning(declaration.US_BRDRefNoInfo, ACEImportAddInfoJobDeclarationValidation.BrokerReferenceWillBeSentEmptyInPSCMessage);
			declaration.US_PSC = false;
			declaration.US_BRDRefNo = "";
			AssertNoWarning(declaration.US_BRDRefNoInfo, ACEImportAddInfoJobDeclarationValidation.BrokerReferenceWillBeSentEmptyInPSCMessage);
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_BRDRefNo = "";
			AssertNoWarning(declaration.US_BRDRefNoInfo, ACEImportAddInfoJobDeclarationValidation.BrokerReferenceWillBeSentEmptyInPSCMessage);
			declaration.US_BRDRefNo = "123456";
			AssertNoWarning(declaration.US_BRDRefNoInfo, ACEImportAddInfoJobDeclarationValidation.BrokerReferenceWillBeSentEmptyInPSCMessage);
			declaration.US_EntryFilerCode = "AAA";
			declaration.US_BRDRefNo = "123456";
			AssertNoWarning(declaration.US_BRDRefNoInfo, ACEImportAddInfoJobDeclarationValidation.BrokerReferenceWillBeSentEmptyInPSCMessage);
		}

		JobDeclaration GetMergibleDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			return declaration;
		}

		void MergeAndSendDeclarationWithSuccessResponse(JobDeclaration declaration)
		{
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, US.Messaging.Business.UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			entry.Messages.Add(message);
			message.EM_MessageNum = "~893427";
			entry.Messages.Add(PSCEntrySummaryDataTest.CreateCustomsResponseSuccess(Factory, "~893427"));
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
		}

		const string UsualTariffForCheckUS_EntryTypeTest = "1003002000";

		const string GoodsReturnedTariffForCheckUS_EntryTypeTest = ACEImportAddInfoJobDeclarationValidation.USGoodsReturnedTariffPrefix + "10"; //9801001010
	}
}
