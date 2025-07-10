using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusEntryHeaderValidationTest : Customs.Business.Testing.CusEntryHeaderValidationTest
	{
		public void TestParent()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestCheckCH_BGReferenceNFReconNoMessageError()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_DeclarationReference = "RECONTEST1";
			var messageError = ValidationConstants.Recon.AlreadyReconciled + declaration.JE_DeclarationReference;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingBorderCargoReleaseOriginal;
			entry.CH_BGMReference = "XJ560011299";
			entry.EntryNumber = "60011299";
			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
			entry2.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseUpdate;
			entry2.CH_BGMReference = "XJ571005051";
			entry2.EntryNumber = "71005051";
			Factory.Save();
			var recon = Factory.New<JobDeclaration>();
			recon.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var reconDec = new ReconDeclaration(recon);
			reconDec.US_EntryFilerCode = "XJ5";
			reconDec.IOROrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			reconDec.US_SuretyCode = "891";
			var invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "Line1";
			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = false;
			originalEntry.CH_OrigEntryReference = "XJ560011299";
			AssertNoMessageError(originalEntry.CH_OrigEntryReferenceInfo, messageError);
			originalEntry.CH_OrigEntryReference = "";
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.ValueRecon;
			originalEntry.CH_OrigEntryReference = "XJ560011299";
			AssertHasMessageError(originalEntry.CH_OrigEntryReferenceInfo, messageError);
			var recon2 = Factory.New<JobDeclaration>();
			recon2.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var reconDec2 = new ReconDeclaration(recon2);
			reconDec2.US_EntryFilerCode = "XJ5";
			reconDec2.IOROrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			reconDec2.US_SuretyCode = "891";
			var invoiceLine2 = reconDec2.InvoiceLines.AddNew();
			invoiceLine2.JI_Description = "Line2";
			var originalEntry2 = reconDec2.OriginalEntries.AddNew();
			originalEntry2.US_R_NoLineDetails = false;
			reconDec2.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			originalEntry2.CH_OrigEntryReference = "XJ507854371";
			AssertNoMessageError(originalEntry2.CH_OrigEntryReferenceInfo, messageError);
			originalEntry.CH_OrigEntryReference = "";
			reconDec2.US_IssueCode = ReconIssueCodeList.Codes.ValueRecon;
			originalEntry.CH_OrigEntryReference = "XJ560011299";
			AssertHasMessageError(originalEntry.CH_OrigEntryReferenceInfo, messageError);
		}

		public void TestCheckCH_BGReferenceAllow2ReconMessageErrorNFNF()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_DeclarationReference = "RECONTEST1";
			declaration.ImportEntryNumber = "60011299";
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var recon = Factory.New<JobDeclaration>();
			recon.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var reconDec = new ReconDeclaration(recon);
			reconDec.US_EntryFilerCode = "XJ5";
			reconDec.IOROrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.FTA; // Recon1, NF
			reconDec.US_SuretyCode = "891";
			var invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "Line1";
			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = false;
			originalEntry.CH_OrigEntryReference = "XJ560011299";
			var recon2 = Factory.New<JobDeclaration>();
			recon2.JE_MessageType = JobMessageTypeList.Codes.Recon; // Recon2, NF
			var reconDec2 = new ReconDeclaration(recon2);
			reconDec2.US_EntryFilerCode = "XJ5";
			reconDec2.IOROrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			reconDec2.US_SuretyCode = "891";
			var originalEntry2 = reconDec2.OriginalEntries.AddNew();
			originalEntry2.US_R_NoLineDetails = false;
			var invoiceLine2 = reconDec2.InvoiceLines.AddNew();
			invoiceLine2.JI_Description = "LineAdded";
			reconDec2.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			originalEntry2.CH_OrigEntryReference = "XJ560011299";
			AssertHasMessageError(originalEntry2.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.OneEntryShouldHaveMaximum2Recons);
		}

		public void TestCheckCH_BGReferenceEntryStatusCED()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			entry.CH_BGMReference = "XJ560011299";
			entry.EntryNumber = "60011299";
			Factory.Save();
			var recon = Factory.New<JobDeclaration>();
			recon.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var reconDec = new ReconDeclaration(recon);
			reconDec.US_EntryFilerCode = "XJ5";
			reconDec.IOROrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			reconDec.US_SuretyCode = "891";
			var invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "Line1";
			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = false;
			originalEntry.CH_OrigEntryReference = "XJ560011299";
			AssertHasMessageError(originalEntry.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.ClearEntrySummaryDelete);
			originalEntry.CH_OrigEntryReference = "";
			entry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			originalEntry.CH_OrigEntryReference = "XJ560011299";
			AssertNoMessageError(originalEntry.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.ClearEntrySummaryDelete);
		}

		public void TestCheckCH_BGMReferenceWithNoChangeAggregate()
		{
			var errorMessage = ValidationConstants.Recon.UnderlyingEntriesWithoutNoChangeAggregate;
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.IOROrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.ValueRecon;
			reconDec.US_R_IsNoChangeAgg = true;
			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "ABC20110000";
			originalEntry.Invoice.InvoiceLines.RemoveAndDeleteAll();
			var reconEntryHeader = originalEntry.GetWrappedEntry();
			reconEntryHeader.Validation.ValidateCH_BGMReference();
			AssertNoMessageError(reconEntryHeader.CH_BGMReferenceInfo, errorMessage);
			reconDec.US_R_IsNoChangeAgg = false;
			reconEntryHeader.Validation.ValidateCH_BGMReference();
			AssertHasMessageError(reconEntryHeader.CH_BGMReferenceInfo, errorMessage);
			originalEntry.Invoice.InvoiceLines.AddNew();
			reconEntryHeader.Validation.ValidateCH_BGMReference();
			AssertNoMessageError(reconEntryHeader.CH_BGMReferenceInfo, errorMessage);
		}

		public void TestCheckCH_BGMReferenceDeactivatedEntryWithActiveCustomsTransaction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			AssertEquals(true, entry.HasBeenLodgedAtCustoms);
			AssertEquals(false, entry.HasBeenWithdrawn);
			entry.US_IsDeactivated = false;
			AssertNoError(entry.CH_BGMReferenceInfo, CusEntryHeaderValidation.DeactivatedEntryWithActiveCustomsTransaction);
			AssertNoError(entry.CH_BGMReferenceInfo, ValidationConstants.DeactivatedEntryWithActiveCustomsTransactionAwaitingResponse);
			AssertNoWarning(entry.CH_BGMReferenceInfo, ValidationConstants.AES.DeactivatedEntryWithActiveCustomsTransaction);
			entry.US_IsDeactivated = true;
			AssertNoError(entry.CH_BGMReferenceInfo, CusEntryHeaderValidation.DeactivatedEntryWithActiveCustomsTransaction);
			AssertNoError(entry.CH_BGMReferenceInfo, ValidationConstants.DeactivatedEntryWithActiveCustomsTransactionAwaitingResponse);
			AssertHasWarning(entry.CH_BGMReferenceInfo, ValidationConstants.AES.DeactivatedEntryWithActiveCustomsTransaction);
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingReplacementResponse;
			entry.US_IsDeactivated = true;
			AssertNoError(entry.CH_BGMReferenceInfo, CusEntryHeaderValidation.DeactivatedEntryWithActiveCustomsTransaction);
			AssertHasError(entry.CH_BGMReferenceInfo, ValidationConstants.DeactivatedEntryWithActiveCustomsTransactionAwaitingResponse);
			AssertNoWarning(entry.CH_BGMReferenceInfo, ValidationConstants.AES.DeactivatedEntryWithActiveCustomsTransaction);
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear;
			entry.US_IsDeactivated = true;
			AssertNoError(entry.CH_BGMReferenceInfo, CusEntryHeaderValidation.DeactivatedEntryWithActiveCustomsTransaction);
			AssertNoError(entry.CH_BGMReferenceInfo, ValidationConstants.DeactivatedEntryWithActiveCustomsTransactionAwaitingResponse);
			AssertHasWarning(entry.CH_BGMReferenceInfo, ValidationConstants.AES.DeactivatedEntryWithActiveCustomsTransaction);
			Factory.Save();
			entry.US_IsDeactivated = true;
			AssertNoError(entry.CH_BGMReferenceInfo, CusEntryHeaderValidation.DeactivatedEntryWithActiveCustomsTransaction);
			AssertNoError(entry.CH_BGMReferenceInfo, ValidationConstants.DeactivatedEntryWithActiveCustomsTransactionAwaitingResponse);
			AssertHasWarning(entry.CH_BGMReferenceInfo, ValidationConstants.AES.DeactivatedEntryWithActiveCustomsTransaction);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			entry.US_IsDeactivated = true;
			AssertHasError(entry.CH_BGMReferenceInfo, CusEntryHeaderValidation.DeactivatedEntryWithActiveCustomsTransaction);
			AssertNoError(entry.CH_BGMReferenceInfo, ValidationConstants.DeactivatedEntryWithActiveCustomsTransactionAwaitingResponse);
			AssertNoWarning(entry.CH_BGMReferenceInfo, ValidationConstants.AES.DeactivatedEntryWithActiveCustomsTransaction);
		}

		public void TestCheckFilerCodeAndEntryNumber()
		{
			var originalEntryWrapped = ReconDeclaration.OriginalEntries.AddNew();
			originalEntryWrapped.CH_OrigEntryReference = "XJ5";
			AssertNoMessageError(originalEntryWrapped.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.EntryFilerEntryNumberMandatory);
			AssertHasMessageError(originalEntryWrapped.CH_OrigEntryReferenceInfo, EntryNumberValidator.EntryNumberFormat);
			originalEntryWrapped.CH_OrigEntryReference = "";
			AssertHasMessageError(originalEntryWrapped.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.EntryFilerEntryNumberMandatory);
			originalEntryWrapped.CH_OrigEntryReference = "XJ560011281";
			AssertNoMessageError(originalEntryWrapped.CH_OrigEntryReferenceInfo, EntryNumberValidator.EntryNumberFormat);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.US_EntryFilerCode = "XJ5";
			var reconDeclaration2 = new ReconDeclaration(declaration2);
			var reconOriginalEntry2 = reconDeclaration2.OriginalEntries.AddNew();
			reconOriginalEntry2.CH_OrigEntryReference = "XJ560011281";
			AssertHasMessageErrorContaining(reconOriginalEntry2.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.AlreadyReconciled);
			reconOriginalEntry2.CH_OrigEntryReference = "XJ560011299";
			AssertNoMessageErrorContaining(reconOriginalEntry2.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.AlreadyReconciled);
			var originalEntryWrapped1 = ReconDeclaration.OriginalEntries.AddNew();
			originalEntryWrapped1.CH_OrigEntryReference = originalEntryWrapped.CH_OrigEntryReference;
			AssertHasError(originalEntryWrapped1.CH_OrigEntryReferenceInfo, CusEntryHeaderValidation.DuplicateFilerCodeAndEntryNumber);
			originalEntryWrapped1.CH_OrigEntryReference = "XJ500023422";
			AssertNoError(originalEntryWrapped1.CH_OrigEntryReferenceInfo, CusEntryHeaderValidation.DuplicateFilerCodeAndEntryNumber);
		}

		public void TestCheckCH_OrigEntryReferenceMaxLength_ReconDeclaration()
		{
			var originalEntryWrapped = ReconDeclaration.OriginalEntries.AddNew();
			AssertNoError(originalEntryWrapped.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.MaxLengthOfEntryNumberIs11);
			originalEntryWrapped.CH_OrigEntryReference = "123456789012";
			AssertHasError(originalEntryWrapped.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.MaxLengthOfEntryNumberIs11);
			originalEntryWrapped.CH_OrigEntryReference = "12345678901";
			AssertNoError(originalEntryWrapped.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.MaxLengthOfEntryNumberIs11);
		}

		public void TestCheckAggregateRecon()
		{
			ReconDeclaration.US_IsAggregate = true;
			Declaration.IOROrgPK = ReconDeclaration.IOROrgPK;
			Declaration.US_OtherReconIndicator = ReconDeclaration.US_IssueCode;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_SuretyCode = ReconDeclaration.US_SuretyCode;
			var originalEntry = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = false;
			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 10m);
			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 20m);
			originalEntry.CH_OrigEntryReference = "ABC60011257";
			originalEntry.ReconCharges.RemoveAll();
			AssertHasMessageErrorContaining(originalEntry.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.NoImportEntriesShouldBeRefunded);
			originalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 10m);
			originalEntry.ReconCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 20m);
			originalEntry.CH_OrigEntryReference = "";
			originalEntry.CH_OrigEntryReference = "XJ560011257";
			AssertNoMessageErrorContaining(originalEntry.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.NoImportEntriesShouldBeRefunded);
		}

		public void TestCheckNoRefunded()
		{
			ReconDeclaration.US_IsAggregate = true;
			ReconDeclaration.US_R_Waive = true;
			Declaration.IOROrgPK = ReconDeclaration.IOROrgPK;
			Declaration.US_OtherReconIndicator = ReconDeclaration.US_IssueCode;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_SuretyCode = ReconDeclaration.US_SuretyCode;
			var originalEntry = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = false;
			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 10m);
			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 20m);
			originalEntry.CH_OrigEntryReference = "ABC60011257";
			originalEntry.ReconCharges.RemoveAll();
			AssertNoMessageErrorContaining(originalEntry.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.NoImportEntriesShouldBeRefunded);
			ReconDeclaration.US_R_Waive = false;
			originalEntry.CH_OrigEntryReference = "";
			originalEntry.CH_OrigEntryReference = "ABC60011257";
			AssertHasMessageErrorContaining(originalEntry.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.NoImportEntriesShouldBeRefunded);
		}

		public void TestCheckIssueCode()
		{
			Factory.Save();
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_OtherReconIndicator = ReconDeclaration.US_IssueCode;
			var originalEntry = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ560011257";
			AssertNoWarningContaining(originalEntry.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.IssueCodeConflict);
			Declaration.US_OtherReconIndicator = "";
			Factory.Save();
			ReconDeclaration.OriginalEntries.RemoveAndDeleteAll();
			originalEntry = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ560011257";
			AssertHasWarningContaining(originalEntry.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.IssueCodeConflict);
			Declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.ValueClass9802Recon;
			Factory.Save();
			ReconDeclaration.OriginalEntries.RemoveAndDeleteAll();
			originalEntry = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ560011257";
			AssertHasWarningContaining(originalEntry.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.IssueCodeConflict);
		}

		public void TestCheckSuretyCodeConflict()
		{
			Factory.Save();
			Declaration.IOROrgPK = ReconDeclaration.IOROrgPK;
			Declaration.US_OtherReconIndicator = ReconDeclaration.US_IssueCode;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_SuretyCode = ReconDeclaration.US_SuretyCode;
			var originalEntry = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ560011257";
			AssertNoMessageErrorContaining(originalEntry.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.SuretyCodeConflict);
			Declaration.US_SuretyCode = "741";
			Factory.Save();
			ReconDeclaration.OriginalEntries.RemoveAndDeleteAll();
			originalEntry = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ560011257";
			AssertHasMessageErrorContaining(originalEntry.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.SuretyCodeConflict);
		}

		public void TestHasValidEntryTypeForRecon()
		{
			Factory.Save();
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var originalEntry = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ560011257";
			AssertNoMessageErrorContaining(originalEntry.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.EntryTypeNotReconcilable);
			Declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			Factory.Save();
			ReconDeclaration.OriginalEntries.RemoveAndDeleteAll();
			originalEntry = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ560011257";
			AssertHasMessageErrorContaining(originalEntry.CH_OrigEntryReferenceInfo, ValidationConstants.Recon.EntryTypeNotReconcilable);
		}

		public void TestValidateAESExportEntry()
		{
			var exportDec = Factory.New<JobDeclaration>();
			exportDec.JE_DeclarationReference = "B000234567";
			exportDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			var aestirEntry = exportDec.CustomsEntryHeaders.AddNew();
			aestirEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			aestirEntry.CH_BGMReference = "B000234567";
			AssertNoWarning("AES entry without value overridden should not validate", aestirEntry.CH_BGMReferenceInfo, CusEntryHeaderValidation.OriginalShipmentNoWarning);
			aestirEntry.US_SendReplace = true;
			aestirEntry.CH_BGMReference = "OS03948299302";
			AssertHasWarning("AES entry with original shipment no entered should validate", aestirEntry.CH_BGMReferenceInfo, CusEntryHeaderValidation.OriginalShipmentNoWarning);
		}

		public void TestValidateReconImportEntry()
		{
			Declaration.US_EnableENS = true;
			var activeEntry = Declaration.ActiveEntryHeaders[0];
			activeEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			Factory.Save();
			var originalEntry = ReconDeclaration.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ560011257";
			var header = Factory.Load<CusEntryHeader>(originalEntry.CH_PK);
			AssertNotNull(header);
			AssertHasMessageError(header.CH_BGMReferenceInfo, ValidationConstants.Recon.ClearEntrySummaryDelete);
			activeEntry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryCanceled;
			originalEntry.CH_OrigEntryReference = "XJ560011256";
			originalEntry.CH_OrigEntryReference = "XJ560011257";
			header = Factory.Load<CusEntryHeader>(originalEntry.CH_PK);
			AssertNotNull(header);
			AssertHasMessageError(header.CH_BGMReferenceInfo, ValidationConstants.Recon.EntrySummaryCanceled);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
		}

		ReconDeclaration reconDeclaration;
		ReconDeclaration ReconDeclaration
		{
			get
			{
				if (reconDeclaration == null)
				{
					reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
					reconDeclaration.IOROrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
					reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.ValueRecon;
					reconDeclaration.US_SuretyCode = "891";
				}

				return reconDeclaration;
			}
		}

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryFilerCode = "XJ5";
					CusEntryHeader entry = declaration.ActiveEntryHeaders.AddNew();
					entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
					entry.EntryNumber = "60011257";
				}

				return declaration;
			}
		}
	}
}
