using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.Testing
{
	class PGADataCorrectionSupporterTest : TestCaseWithFactory
	{
		public void TestMarkStatusAsSubmittingToCustoms()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			IPGADataCorrection correction = null;
			AssertNoExceptionThrown(() => correction.MarkStatusAsSubmittingToCustoms(true));
			var aphis = invoiceLine.APHISHeaders.AddNew();
			AssertEquals(ZString.Empty, aphis.US_TrackingStatus);
			correction = aphis;
			correction.MarkStatusAsSubmittingToCustoms(true);
			AssertEquals(PGATrackingStatusList.Codes.Adding, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			correction.MarkStatusAsSubmittingToCustoms(true);
			AssertEquals(PGATrackingStatusList.Codes.Updating, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;
			correction.MarkStatusAsSubmittingToCustoms(true);
			AssertEquals(PGATrackingStatusList.Codes.Adding, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			correction.MarkStatusAsSubmittingToCustoms(true);
			AssertEquals(PGATrackingStatusList.Codes.Deleted, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleting;
			correction.MarkStatusAsSubmittingToCustoms(true);
			AssertEquals(PGATrackingStatusList.Codes.Deleting, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			correction.MarkStatusAsSubmittingToCustoms(true);
			AssertEquals(PGATrackingStatusList.Codes.Deleting, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			correction.MarkStatusAsSubmittingToCustoms(true);
			AssertEquals(PGATrackingStatusList.Codes.Updating, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Updating;
			correction.MarkStatusAsSubmittingToCustoms(true);
			AssertEquals(PGATrackingStatusList.Codes.Updating, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = ZString.Empty;
			correction.MarkStatusAsSubmittingToCustoms(false);
			AssertEquals(ZString.Empty, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			correction.MarkStatusAsSubmittingToCustoms(false);
			AssertEquals(PGATrackingStatusList.Codes.Added, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;
			correction.MarkStatusAsSubmittingToCustoms(false);
			AssertEquals(PGATrackingStatusList.Codes.Adding, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			correction.MarkStatusAsSubmittingToCustoms(false);
			AssertEquals(PGATrackingStatusList.Codes.Deleted, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleting;
			correction.MarkStatusAsSubmittingToCustoms(false);
			AssertEquals(PGATrackingStatusList.Codes.Deleting, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			correction.MarkStatusAsSubmittingToCustoms(false);
			AssertEquals(PGATrackingStatusList.Codes.Deleting, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			correction.MarkStatusAsSubmittingToCustoms(false);
			AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Updating;
			correction.MarkStatusAsSubmittingToCustoms(false);
			AssertEquals(PGATrackingStatusList.Codes.Updating, aphis.US_TrackingStatus);
		}

		public void SetTrackStatusAfterMessageFailure()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			IPGADataCorrection correction = null;
			AssertNoExceptionThrown(() => correction.SetTrackStatusAfterMessageFailure());
			var aphis = invoiceLine.APHISHeaders.AddNew();
			AssertEquals(ZString.Empty, aphis.US_TrackingStatus);
			correction = aphis;
			correction.SetTrackStatusAfterMessageFailure();
			AssertEquals("", aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			correction.SetTrackStatusAfterMessageFailure();
			AssertEquals(PGATrackingStatusList.Codes.Added, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;
			correction.SetTrackStatusAfterMessageFailure();
			AssertEquals("", aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Updating;
			correction.SetTrackStatusAfterMessageFailure();
			AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleting;
			correction.SetTrackStatusAfterMessageFailure();
			AssertEquals(PGATrackingStatusList.Codes.ToBeDeleted, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			correction.SetTrackStatusAfterMessageFailure();
			AssertEquals(PGATrackingStatusList.Codes.ToBeDeleted, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			correction.SetTrackStatusAfterMessageFailure();
			AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, aphis.US_TrackingStatus);
		}

		public void TestCanBeChangedToBeUpdated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			IPGADataCorrection correction = null;
			AssertEquals(false, correction.CanBeChangedToBeUpdated());
			var aphis = invoiceLine.APHISHeaders.AddNew();
			correction = aphis;
			AssertEquals(false, correction.CanBeChangedToBeUpdated());
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			AssertEquals(false, correction.CanBeChangedToBeUpdated());
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			AssertEquals(true, correction.CanBeChangedToBeUpdated());
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			AssertEquals(true, correction.CanBeChangedToBeUpdated());
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;
			AssertEquals(false, correction.CanBeChangedToBeUpdated());
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			AssertEquals(false, correction.CanBeChangedToBeUpdated());
		}

		public void TestLoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var list = new List<IPGADataCorrection>();
			var aphis = invoiceLine.APHISHeaders.AddNew();
			list.Add(aphis);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, declaration.PGATrackerHelper.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(OGAIndicatorList.Codes.Declared, OGAIndicatorList.Codes.Disclaimed, list));
			AssertEquals(PGATrackingStatusList.Codes.ToBeDeleted, aphis.US_TrackingStatus);

			aphis.US_TrackingStatus = ZString.Empty;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, declaration.PGATrackerHelper.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(OGAIndicatorList.Codes.Declared, OGAIndicatorList.Codes.Disclaimed, list));
			AssertEquals(ZString.Empty, aphis.US_TrackingStatus);

			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, declaration.PGATrackerHelper.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(OGAIndicatorList.Codes.Declared, OGAIndicatorList.Codes.Disclaimed, list));
			AssertEquals(PGATrackingStatusList.Codes.Deleted, aphis.US_TrackingStatus);
			var warning = "";
			declaration.MarkPGAStatusToBeDeletedWarningChecker = new JobDeclaration.MarkPGAStatusToBeDeletedWarningCheckerDelegate((x) =>
			{
				warning = x;
				return true;
			});

			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, declaration.PGATrackerHelper.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(OGAIndicatorList.Codes.Declared, OGAIndicatorList.Codes.Disclaimed, list));
			AssertEquals(PGATrackingStatusList.Codes.ToBeDeleted, aphis.US_TrackingStatus);
			AssertEquals(PGADataChangeTracker.LodgedWillBeDeletedWarning, warning);

			warning = "";
			aphis.US_TrackingStatus = ZString.Empty;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, declaration.PGATrackerHelper.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(OGAIndicatorList.Codes.Declared, OGAIndicatorList.Codes.Disclaimed, list));
			AssertEquals(ZString.Empty, aphis.US_TrackingStatus);
			AssertEquals("", warning);

			warning = "";
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleting;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, declaration.PGATrackerHelper.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(OGAIndicatorList.Codes.Declared, OGAIndicatorList.Codes.Disclaimed, list));
			AssertEquals(PGATrackingStatusList.Codes.Deleting, aphis.US_TrackingStatus);
			AssertEquals("", warning);

			declaration.MarkPGAStatusToBeDeletedWarningChecker = new JobDeclaration.MarkPGAStatusToBeDeletedWarningCheckerDelegate((x) =>
			{
				warning = x;
				return false;
			});

			warning = "";
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Updating;
			AssertEquals(OGAIndicatorList.Codes.Declared, declaration.PGATrackerHelper.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(OGAIndicatorList.Codes.Declared, OGAIndicatorList.Codes.Disclaimed, list));
			AssertEquals(PGATrackingStatusList.Codes.Updating, aphis.US_TrackingStatus);
			AssertEquals(PGADataChangeTracker.LodgedWillBeDeletedWarning, warning);

			warning = "";
			aphis.US_TrackingStatus = ZString.Empty;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, declaration.PGATrackerHelper.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(OGAIndicatorList.Codes.Declared, OGAIndicatorList.Codes.Disclaimed, list));
			AssertEquals(ZString.Empty, aphis.US_TrackingStatus);
			AssertEquals("", warning);

			warning = "";
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, declaration.PGATrackerHelper.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(OGAIndicatorList.Codes.Declared, OGAIndicatorList.Codes.Disclaimed, list));
			AssertEquals(PGATrackingStatusList.Codes.Deleted, aphis.US_TrackingStatus);
			AssertEquals("", warning);
		}

		public void TestIncludedInMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			IPGADataCorrection correction = null;
			AssertEquals(true, correction.IncludedInMessage());
			var aphis = invoiceLine.APHISHeaders.AddNew();
			correction = aphis;
			AssertEquals(true, correction.IncludedInMessage());
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			AssertEquals(false, correction.IncludedInMessage());
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			AssertEquals(false, correction.IncludedInMessage());
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			AssertEquals(true, correction.IncludedInMessage());
		}

		public void TestIsPGALineReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			IPGADataCorrection correction = null;
			AssertEquals(false, correction.IsPGALineReadOnly());
			var aphis = invoiceLine.APHISHeaders.AddNew();
			correction = aphis;
			AssertEquals(false, correction.IsPGALineReadOnly());
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			AssertEquals(true, correction.IsPGALineReadOnly());
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			AssertEquals(false, correction.IsPGALineReadOnly());
		}

		public void TestSetTrackStatusAfterMessageIsLodged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			IPGADataCorrection correction = null;
			AssertNoExceptionThrown(() => correction.SetTrackStatusAfterMessageIsLodged());
			var aphis = invoiceLine.APHISHeaders.AddNew();
			correction = aphis;
			correction.SetTrackStatusAfterMessageIsLodged();
			AssertEquals("", aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;
			correction.SetTrackStatusAfterMessageIsLodged();
			AssertEquals(PGATrackingStatusList.Codes.Added, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			correction.SetTrackStatusAfterMessageIsLodged();
			AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Updating;
			correction.SetTrackStatusAfterMessageIsLodged();
			AssertEquals(PGATrackingStatusList.Codes.Added, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			correction.SetTrackStatusAfterMessageIsLodged();
			AssertEquals(PGATrackingStatusList.Codes.Deleted, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			correction.SetTrackStatusAfterMessageIsLodged();
			AssertEquals(PGATrackingStatusList.Codes.Added, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			correction.SetTrackStatusAfterMessageIsLodged();
			AssertEquals(PGATrackingStatusList.Codes.ToBeDeleted, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleting;
			correction.SetTrackStatusAfterMessageIsLodged();
			AssertEquals(PGATrackingStatusList.Codes.Deleted, aphis.US_TrackingStatus);

			var nmfs = invoiceLine.NMFSLines.AddNew();
			nmfs.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;
			correction = nmfs;
			correction.SetTrackStatusAfterMessageIsLodged();
			AssertEquals(PGATrackingStatusList.Codes.Added, nmfs.US_TrackingStatus);
		}

		public void TestSetTrackStatusAfterDeletionMessageIsAccepted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			IPGADataCorrection correction = null;
			AssertNoExceptionThrown(() => correction.SetTrackStatusAfterDeletionMessageIsAccepted());
			var aphis = invoiceLine.APHISHeaders.AddNew();
			correction = aphis;
			correction.SetTrackStatusAfterDeletionMessageIsAccepted();
			AssertEquals(ZString.Empty, aphis.US_TrackingStatus);
			foreach (ICodeDescription pair in new PGATrackingStatusList())
			{
				aphis.US_TrackingStatus = pair.Code;
				correction.SetTrackStatusAfterDeletionMessageIsAccepted();
				AssertEquals(pair.Code, ZString.Empty, aphis.US_TrackingStatus);
			}
		}

		public void TestUpdatePGACorrectionRequired()
		{
			JobDeclaration declaration = null;
			AssertNoExceptionThrown(() => declaration.UpdatePGADataReplacementUpdateRequired());
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.UpdatePGADataReplacementUpdateRequired();
			AssertEquals(ZString.Empty, declaration.US_PGAReplaceUpdateNeeded);
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			declaration.UpdatePGADataReplacementUpdateRequired();
			AssertEquals(YesNoList.Codes.Yes, declaration.US_PGAReplaceUpdateNeeded);
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;
			declaration.ActiveEntryHeaders.SimplifiedEntry.Logs.RemoveAndDeleteAll();
			declaration.US_PGAReplaceUpdateNeeded = YesNoList.Codes.No;
			declaration.UpdatePGADataReplacementUpdateRequired();
			AssertEquals(YesNoList.Codes.No, declaration.US_PGAReplaceUpdateNeeded);
			declaration.US_PGAReplaceUpdateNeeded = "@";
			declaration.UpdatePGADataReplacementUpdateRequired();
			AssertEquals("@", declaration.US_PGAReplaceUpdateNeeded);
		}

		public void TestPGALinesCanBeDeleted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			IPGADataCorrection correction = null;
			AssertEquals(false, correction.PGALinesCanBeDeleted());
			var aphis = invoiceLine.APHISHeaders.AddNew();
			correction = aphis;
			AssertEquals(true, correction.PGALinesCanBeDeleted());
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			AssertEquals(false, correction.PGALinesCanBeDeleted());
		}

		public void TestHasTrackingIDChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.US_ChildLineNum = 1;
			entryLine2.US_SupLine = true;
			entryLine2.US_CL_ParentLine = entryLine1.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			IPGADataCorrection aphis = null;
			AssertEquals(false, aphis.HasTrackingIDChanged());
			aphis = invoiceLine.APHISHeaders.AddNew();
			AssertEquals(false, aphis.HasTrackingIDChanged());
			invoiceLine.SetTrackingID();
			AssertEquals(false, aphis.HasTrackingIDChanged());
			invoiceLine.JI_Tariff = "1010101010";
			AssertEquals(true, aphis.HasTrackingIDChanged());
			invoiceLine.SetTrackingID();
			AssertEquals(false, aphis.HasTrackingIDChanged());
			invoiceLine.US_SupTariff = "9810101010";
			AssertEquals(true, aphis.HasTrackingIDChanged());
			invoiceLine.SetTrackingID();
			AssertEquals(false, aphis.HasTrackingIDChanged());
			invoiceLine.JI_CL = entryLine1.PK;
			AssertEquals(true, aphis.HasTrackingIDChanged());
			invoiceLine.SetTrackingID();
			AssertEquals(false, aphis.HasTrackingIDChanged());
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);
			AssertEquals(true, aphis.HasTrackingIDChanged());
			invoiceLine.SetTrackingID();
			AssertEquals(false, aphis.HasTrackingIDChanged());
			invoiceLine.US_SupTariff = "";
			invoiceLine.SetTrackingID();
			AssertEquals(false, aphis.HasTrackingIDChanged());

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			AssertEquals(true, aphis.HasTrackingIDChanged());
			invoiceLine.SetTrackingID();
			AssertEquals(false, aphis.HasTrackingIDChanged());
			invoiceLine2.JI_Tariff = "2020";
			AssertEquals(true, aphis.HasTrackingIDChanged());
			invoiceLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals("Should still be true as previous tracking has 1 secondary", true, aphis.HasTrackingIDChanged());
			invoiceLine.SetTrackingID();
			AssertEquals(false, aphis.HasTrackingIDChanged());
		}

		public void TestSetTrackStatusAfterSOMessageIsRejected()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			IPGADataCorrection correction = null;
			AssertNoExceptionThrown(() => correction.SetTrackStatusAfterSOMessageIsRejected());
			var aphis = invoiceLine.APHISHeaders.AddNew();
			AssertEquals(ZString.Empty, aphis.US_TrackingStatus);
			correction = aphis;
			correction.SetTrackStatusAfterSOMessageIsRejected();
			AssertEquals(ZString.Empty, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			correction.SetTrackStatusAfterSOMessageIsRejected();
			AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Adding;
			correction.SetTrackStatusAfterSOMessageIsRejected();
			AssertEquals(PGATrackingStatusList.Codes.Adding, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Updating;
			correction.SetTrackStatusAfterSOMessageIsRejected();
			AssertEquals(PGATrackingStatusList.Codes.Updating, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleting;
			correction.SetTrackStatusAfterSOMessageIsRejected();
			AssertEquals(PGATrackingStatusList.Codes.Deleting, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			correction.SetTrackStatusAfterSOMessageIsRejected();
			AssertEquals(PGATrackingStatusList.Codes.ToBeDeleted, aphis.US_TrackingStatus);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			correction.SetTrackStatusAfterSOMessageIsRejected();
			AssertEquals(PGATrackingStatusList.Codes.ToBeDeleted, aphis.US_TrackingStatus);
		}

		public void TestLodgedWillBeDeletedWarningMessage()
		{
			var warningMessage = "This line has accepted PGA data against it. The changes that you have made will cause the PGA Lines to be marked with a message status of “To Be Deleted” if you were to send a PGA data correction message. The overall PGA status of the line will remain. Are you sure that you wish to continue?";
			AssertEquals(warningMessage, PGADataChangeTracker.LodgedWillBeDeletedWarning);
		}

		public void TestGetReleaseDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.APHISHeaders.AddNew();
			AssertEquals(ZDateTime.BrettsBirthday, invoiceLine.APHISHeaders.GetReleaseDate());
		}

		public void TestIEnumerableHasSpecificPGALines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var aphisCorrection = invoiceLine.APHISHeaders.AddNew();
			AssertEquals(false, new List<IPGADataCorrection>() { aphisCorrection }.HasSpecificPGALines(ACEGovernmentAgenciesCodeList.Codes.FDA));
			AssertEquals(false, new List<IPGADataCorrection>() { aphisCorrection }.HasSpecificPGALines(ACEGovernmentAgenciesCodeList.Codes.FWS));
			var fdaCorrection = invoiceLine.ACE_FDALines.AddNew();
			var fwsCorrection = invoiceLine.FWSHeaders.AddNew();
			AssertEquals(true, new List<IPGADataCorrection>() { fdaCorrection, aphisCorrection }.HasSpecificPGALines(ACEGovernmentAgenciesCodeList.Codes.FDA));
			AssertEquals(true, new List<IPGADataCorrection>() { fdaCorrection, fwsCorrection }.HasSpecificPGALines(ACEGovernmentAgenciesCodeList.Codes.FWS));
		}

		public void TestCollectionHasSpecificPGALines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.APHISHeaders.AddNew();
			AssertEquals(false, invoiceLine.APHISHeaders.HasSpecificPGALines(ACEGovernmentAgenciesCodeList.Codes.FDA));
			AssertEquals(false, invoiceLine.APHISHeaders.HasSpecificPGALines(ACEGovernmentAgenciesCodeList.Codes.FWS));
			invoiceLine.ACE_FDALines.AddNew();
			invoiceLine.FWSHeaders.AddNew();
			AssertEquals(true, invoiceLine.ACE_FDALines.HasSpecificPGALines(ACEGovernmentAgenciesCodeList.Codes.FDA));
			AssertEquals(true, invoiceLine.FWSHeaders.HasSpecificPGALines(ACEGovernmentAgenciesCodeList.Codes.FWS));
		}
	}
}
