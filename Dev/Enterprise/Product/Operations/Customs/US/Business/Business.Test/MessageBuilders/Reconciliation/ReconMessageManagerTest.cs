using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ReconMessageManagerTest : TestCaseWithFactory
	{
		public void TestPopulateMessages()
		{
			var reconDec = GetReconDeclaration();

			var newFactoryForLoad = new BusinessObjectFactory();
			var loadedMessages = newFactoryForLoad.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, "RA"));
			AssertEquals("Precondition: no RA messages exists in the database", 0, loadedMessages.Length);

			var manager = new ReconMessageManager(new ReconDeclarationIReconciliation(reconDec), UpdateActionCode.Add);
			manager.PopulateMessage();
			AssertEquals("1 message", 1, reconDec.Messages.Count);
			AssertEquals("EM_MessageSubType", EM_MessageSubTypeList.Codes.ReconOriginal, reconDec.Messages[0].EM_MessageSubType);
			AssertEquals("Message status", ReconMessageStatusList.Descriptions.AwaitingReconOriginal, reconDec.MessageStatusDescription);
			AssertEquals("Entry Submitted Date", ZDateTime.Today.Date, reconDec.ReconWrappedJobDeclaration.JE_EntrySubmittedDate.Date);
			AssertEquals("Entry Submitted Date", ZDateTime.Today.Date, reconDec.ReconEntry.GetEntry().CH_EntrySubmittedDate.Date);

			loadedMessages = newFactoryForLoad.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, "RA"));
			AssertEquals("Should be one RA message", 1, loadedMessages.Length);

			var query = new ZQuery(StmALogSchema.SL_Parent, reconDec.ReconWrappedJobDeclaration.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "CCC");

			var customsCommencedLogs = reconDec.ReconWrappedJobDeclaration.Logs.Find(query);
			AssertEquals("One Customs Commenced Log exists", 1, customsCommencedLogs.Length);
			AssertEquals("Log event description", "Customs Commenced", customsCommencedLogs[0].SL_EventDescription);

			reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			manager = new ReconMessageManager(new ReconDeclarationIReconciliation(reconDec), UpdateActionCode.Delete);
			manager.PopulateMessage();
			AssertEquals("1 message", 1, reconDec.Messages.Count);
			AssertEquals("EM_MessageSubType", EM_MessageSubTypeList.Codes.ReconDelete, reconDec.Messages[0].EM_MessageSubType);
			AssertEquals("Message status", ReconMessageStatusList.Descriptions.AwaitingReconDelete, reconDec.MessageStatusDescription);
			reconDec.ReconWrappedJobDeclaration.Logs.Find(query);
			AssertEquals("One Customs Commenced Log exists", 1, customsCommencedLogs.Length);

			reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			manager = new ReconMessageManager(new ReconDeclarationIReconciliation(reconDec), UpdateActionCode.Replace);
			manager.PopulateMessage();
			AssertEquals("1 message", 1, reconDec.Messages.Count);
			AssertEquals("EM_MessageSubType", EM_MessageSubTypeList.Codes.ReconReplace, reconDec.Messages[0].EM_MessageSubType);
			AssertEquals("Message status", ReconMessageStatusList.Descriptions.AwaitingReconReplace, reconDec.MessageStatusDescription);
			AssertEquals("Message status is set to declaration.JE_MessageStatus as well", ReconMessageStatusList.Codes.AwaitingReconReplace, reconDec.MessageStatus);
			reconDec.ReconWrappedJobDeclaration.Logs.Find(query);
			AssertEquals("One Customs Commenced Log exists", 1, customsCommencedLogs.Length);

			reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDec.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
			var action = new ACEReconMessageSendingAction(new ReconDeclarationIReconciliation(reconDec), UpdateActionCode.Add);
			action.PaymentFinalized = "Y";
			manager = new ReconMessageManager(action);
			manager.PopulateMessage();
			AssertEquals("PaymentFinalized", "Y", reconDec.ReconWrappedJobDeclaration.US_Paid);
		}

		public void TestGetMessageSendingNotificationsCheckMutex()
		{
			var reconDec = GetReconDeclaration();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var decInFactory2 = factory2.Load<JobDeclaration>(reconDec.ReconWrappedJobDeclaration.PK);
			var reconDecInFactory2 = new ReconDeclaration(decInFactory2);
			AssertEquals(true, decInFactory2.LockImportEntryNumberAllocationMutex);

			var manager = new ReconMessageManager(new ReconDeclarationIReconciliation(reconDec), UpdateActionCode.Add);
			Env.Security.USReconSendWithMessageErrors.IsAllowed = true;
			var notifications = manager.MessageSendingNotifications;
			Assert("Allocation error", notifications.ContainsError(decInFactory2.GetImportEntryNumberAllocationMutexLockInfo() + " is in the process of allocating Entry Number for this job; system cannot send the data as it will result in a different Entry Number being allocated.\r\nPlease retry sending when the other user has finished."));

			reconDecInFactory2.ReconEntry.GetEntry().EntryNumber = "ENDS322";
			factory2.Save();
			AssertEquals(true, decInFactory2.LockImportEntryNumberAllocationMutex);
			AssertEquals("", reconDec.ReconEntryNumber);
			var manager2 = new ReconMessageManager(new ReconDeclarationIReconciliation(reconDec), UpdateActionCode.Add);
			notifications = manager2.MessageSendingNotifications;
			AssertEquals("No allocation error", 0, notifications.ErrorCount);
			decInFactory2.UnlockImportEntryNumberAllocationMutex();
		}

		public void TestGetMessageSendingNotifications()
		{
			var reconDec = GetReconDeclaration();
			reconDec.ReconEntry.GetEntry().EntryNumber = "12345";
			var manager = new ReconMessageManager(new ReconDeclarationIReconciliation(reconDec), UpdateActionCode.Add);

			Env.Security.USReconSendWithMessageErrors.IsAllowed = false;
			var notifications = manager.MessageSendingNotifications;
			Assert("Notifications from recon declaration should be there",
				notifications.WarningNotificationsAsString().Contains("You have not entered a comment"));

			string error = Customs.Business.SingleMessageManager.MessageErrorsExistWithNoSecurityRight + " " + Env.Security.USReconSendWithMessageErrors.DisplayTextPathToSecurityRight;
			Assert("Should contain security right error message", notifications.ContainsError(error));

			var manager2 = new ReconMessageManager(new ReconDeclarationIReconciliation(reconDec), UpdateActionCode.Add);
			Env.Security.USReconSendWithMessageErrors.IsAllowed = true;
			CreateStatementWithLine("XJ5", "12345");
			notifications = manager2.MessageSendingNotifications;
			Assert("Entry on Statement notification should be there",
				notifications.WarningNotificationsAsString().Contains(ValidationConstants.EntrySummary.AlreadyOnStatement("reconciliation entry")));

			Assert("Should contain security right error message", !notifications.ContainsError(error));
		}

		public void TestGetMessageSendingNotificationsUseFetchHints()
		{
			var reconDec = GetReconDeclaration();
			reconDec.ReconEntry.GetEntry().EntryNumber = "12345";
			var originalEntry1 = reconDec.OriginalEntries.AddNew();
			originalEntry1.CH_OrigEntryReference = "XJ5ENTRY1";
			var originalEntry2 = reconDec.OriginalEntries.AddNew();
			originalEntry2.CH_OrigEntryReference = "XJ5ENTRY2";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();

			var dec = newFactory.Load<JobDeclaration>(reconDec.PK);
			reconDec = new ReconDeclaration(dec);

			var manager = new ReconMessageManager(new ReconDeclarationIReconciliation(reconDec), UpdateActionCode.Add);
			_ = manager.MessageSendingNotifications;
			AssertEquals("Fetch Hints should be added for CusEntryHeader", 1, newFactory.GetTableHitCount(CusEntryHeaderSchema.Constants.TableName));
		}

		CusStatementHeader CreateStatementWithLine(ZString entryFilerCode, ZString entryNumber)
		{
			var header = Factory.New<CusStatementHeader>();

			var statementLine = header.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = entryFilerCode;
			statementLine.B3_EntryNum = entryNumber;
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;

			return header;
		}

		ReconDeclaration GetReconDeclaration()
		{
			DeclarationTestHelper.SetupBranchSpecificFormalEntryNumber("XJ5");
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			return reconDec;
		}
	}
}
