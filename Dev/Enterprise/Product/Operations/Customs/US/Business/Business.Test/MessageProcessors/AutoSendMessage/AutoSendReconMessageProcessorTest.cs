using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AutoSendReconMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessWhenCanSendOriginalIsFalse()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = false;
			declaration.ValidationModes = ValidationModes.EntrySummary;

			var recon = ReconDeclaration.Get(declaration);
			var reconEntry = recon.ReconEntry.GetEntry();
			recon.OriginalEntries.AddNew();
			Factory.Save();

			var processor = new AutoSendReconMessageProcessor(declaration) as IProcessor;
			var notifications = new NotificationBuffer();

			notifications.Clear();
			reconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
			Factory.Save();
			processor.Process(notifications);
			AssertEquals(true, notifications.HasWarnings);
			AssertEquals(true, notifications.AsString.Contains("as the entry has already been added."));
			AssertEquals(0, reconEntry.Messages.Count);
		}

		public void TestProcess()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = false;
			declaration.ValidationModes = ValidationModes.EntrySummary;

			var recon = ReconDeclaration.Get(declaration);
			var reconEntry = recon.ReconEntry.GetEntry();
			Factory.Save();

			var processor = new AutoSendReconMessageProcessor(declaration) as IProcessor;
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			AssertEquals(true, notifications.HasWarnings);
			AssertEquals(true, notifications.AsString.Contains("please enter at least one entry record under Recon Declaration > Entries."));
			AssertEquals(0, reconEntry.Messages.Count);

			notifications.Clear();
			recon.OriginalEntries.AddNew();
			reconEntry.CH_Status = ReconMessageStatusList.Codes.AwaitingReconOriginal;
			Factory.Save();
			processor.Process(notifications);
			AssertEquals(true, notifications.HasWarnings);
			AssertEquals(true, notifications.AsString.Contains("please check whether the job is waiting for response from customs."));
			AssertEquals(0, reconEntry.Messages.Count);

			notifications.Clear();
			var newFactory = new BusinessObjectFactory();
			var loadedDec = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(true, loadedDec.LockImportEntryNumberAllocationMutex);
			reconEntry.CH_Status = string.Empty;
			Factory.Save();
			processor.Process(notifications);
			AssertEquals(true, notifications.HasWarnings);
			AssertEquals(true, notifications.AsString.Contains("is in the process of allocating Entry Number for this job; system cannot send the data as it will result in a different Entry Number being allocated."));
			AssertEquals(0, reconEntry.Messages.Count);

			notifications.Clear();
			loadedDec.UnlockImportEntryNumberAllocationMutex();
			newFactory.Save();
			recon.US_IsAggregate = true;
			recon.US_R_IsNoChangeAgg = false;
			Factory.Save();
			processor.Process(notifications);
			AssertEquals(true, notifications.HasWarnings);
			AssertEquals(true, notifications.AsString.Contains("as this job is marked as aggregate with changes, but there are no changed lines."));
			AssertEquals(0, reconEntry.Messages.Count);

			notifications.Clear();
			recon.US_IsAggregate = false;
			AssertEquals(true, loadedDec.LockSendCustomsMessageMutex);
			Factory.Save();
			processor.Process(notifications);
			AssertEquals(true, notifications.HasErrors);
			AssertEquals(true, notifications.AsString.Contains("is trying to send the same message for this job. Please wait unitl the lock has been released before trying to send the message again."));
			AssertEquals(0, reconEntry.Messages.Count);

			notifications.Clear();
			loadedDec.UnlockSendCustomsMessageMutex();
			newFactory.Save();
			Factory.Save();
			processor.Process(notifications);
			AssertEquals(false, notifications.HasErrors);
			AssertEquals(false, notifications.HasWarnings);
			AssertEquals(true, notifications.AsString.Contains("Reconciliation Add message has been sent to customs for"));
			AssertEquals(1, reconEntry.Messages.Count);
		}
	}
}
