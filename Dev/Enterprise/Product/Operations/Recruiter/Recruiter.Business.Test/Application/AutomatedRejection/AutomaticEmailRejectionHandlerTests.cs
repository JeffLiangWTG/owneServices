using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class AutomaticEmailRejectionHandlerTests : TransactionedTestCase
	{
		public void TestQueueAndCancelRejectionEmail()
		{
			var factory = new BusinessObjectFactory();
			var application = AutomatedRejectionTestHelper.CreateApplication(factory, "Bob Ross");
			factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			_ = application.QueueRejectionEmail();
			var rejectionLogs = application.Logs
				.Find(l => l.SL_Parent == application.PK)
				.Where(l => l.Event.SE_Code == AutoEvents.RejectionEmailQueued.Code);

			AssertEquals(1, rejectionLogs.Count());

			var log = rejectionLogs.First();
			AssertEquals(false, log.IsCancelled);
			application.CancelRejectionEmail();
			AssertEquals(true, log.IsCancelled);
			AssertEquals(1, rejectionLogs.Count());
		}

		public void TestSendRejectionEmail()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var application = AutomatedRejectionTestHelper.CreateApplication(factory, "Bob Ross");

			factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			using (RecruitmentDataRegistryForTest.Instance.SenderAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test@dummytest.com"))
			{
				_ = application.QueueRejectionEmail();

				// act
				application.SendRejectionEmail(factory, null);

				// assert that the "RejectionEmailSent" event was logged
				var rejectionSentLogs = application.Logs
					.Find(l => l.Event.SE_Code == AutoEvents.RecruitmentCandidateEvent.Code)
					.Where(l => !l.IsCancelled)
					.ToList();

				AssertEquals("There should be one log for 'RejectionEmailSent'.", 1, rejectionSentLogs.Count);
				AssertEquals("The event code should be of 'RecruitmentCandidateEvent'.", "RCE", rejectionSentLogs[0].Event.SE_Code);
			}
		}

		public void TestRejectionEmail_NotSentTwice()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var application = AutomatedRejectionTestHelper.CreateApplication(factory, "Alice Waltz");

			factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (RecruitmentDataRegistryForTest.Instance.SenderAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test@dummytest.com"))
			{
				_ = application.QueueRejectionEmail();
				application.SendRejectionEmail(factory, null);

				// assert that the "RejectionEmailSent" event was logged first time
				var rejectionSentLogs = application.Logs
					.Find(l => l.Event.SE_Code == AutoEvents.RecruitmentCandidateEvent.Code)
					.Where(l => !l.IsCancelled)
					.ToList();

				AssertEquals("There should be one log for 'RejectionEmailSent'.", 1, rejectionSentLogs.Count);

				// send rejection email again
				application.SendRejectionEmail(factory, null);

				// assert that the "RejectionEmailSent" event was not logged this time
				var rejectionSentLogs_2ndTime = application.Logs
					.Find(l => l.Event.SE_Code == AutoEvents.RecruitmentCandidateEvent.Code)
					.Where(l => !l.IsCancelled)
					.ToList();

				AssertEquals("There should still be only one log for 'RejectionEmailSent'.", 1, rejectionSentLogs_2ndTime.Count);
			}
		}

		sealed class RecruitmentDataRegistryForTest : RegistryItemSet
		{
			public static RecruitmentDataRegistryForTest Instance => fInstance = fInstance ?? new RecruitmentDataRegistryForTest();
			[ThreadStatic]
			static RecruitmentDataRegistryForTest fInstance;

			public override bool IsForProductivityWise => false;

			public abstract class Categories : RawDataRegistry.Categories
			{
				public static MultilingualString Candidate_Management => CombineCategories(Recruiter, (NoResString)"Candidate Management");
				public static MultilingualString Candidate_Management_Mail => CombineCategories(Candidate_Management, (NoResString)"Mail");
				public static MultilingualString Candidate_Management_Mail_Outgoing => CombineCategories(Candidate_Management_Mail, (NoResString)"Outgoing");
			}

			public StringRegistryItem SenderAddress =>
				GetItem("RecruitmentOutgoingSenderAddress", () =>
					new StringRegistryItem(
						"RecruitmentOutgoingSenderAddress",
						Categories.Candidate_Management_Mail_Outgoing,
						(NoResString)"Sender Address",
						(NoResString)"The email address to use as the sender for outgoing messages",
						new EmailStringRegistryDataType(),
						RegistryStorageFlags.System));
		}
	}
}
