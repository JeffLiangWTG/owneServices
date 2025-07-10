using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRJobApplicationAutomatedRejectionExtensionTests : TransactionedTestCase
	{
		public void TestAnyActiveLogs()
		{
			var factory = new BusinessObjectFactory();
			var application = AutomatedRejectionTestHelper.CreateApplication(factory, "Bob Ross");
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var otherLog = application.Logs.AddNew(AutoEvents.EditedARecord, $"test");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			AssertEquals(expected: false, otherLog.IsCancelled);
			AssertEquals(expected: false, HasActiveRejectionLogs(application));

			var log = application.QueueRejectionEmail();
			AssertEquals(expected: false, log.IsCancelled);
			AssertEquals(expected: false, otherLog.IsCancelled);
			AssertEquals(log.Event, AutoEvents.RejectionEmailQueued);
			Assert(HasActiveRejectionLogs(application));
		}

		public void TestQueueRejectionEmail()
		{
			var factory = new BusinessObjectFactory();
			var application = AutomatedRejectionTestHelper.CreateApplication(factory, "Bob Ross");

			var log = application.QueueRejectionEmail();
			AssertEquals(AutoEvents.RejectionEmailQueued.Code, log.Event.SE_Code);
			AssertEquals(AutoEvents.RejectionEmailQueued.Code, log.SL_SE_NKEvent);
			Assert(ZDateTimesWithinRange(log.SL_EventTime, ZDateTime.Now.AddDays(3), new System.TimeSpan(0, 0, 10)));
			AssertEquals(expected: false, log.IsCancelled);
			AssertEquals("HRJobApplication", log.SL_Table);
			AssertEquals(application.PK.ToString(), log.SL_Parent.ToString());
			AssertEquals("Rejection Email Queued", log.SL_Reference.ToString());
		}

		bool ZDateTimesWithinRange(ZDateTime earlier, ZDateTime later, System.TimeSpan window)
			=> earlier.ToDateTime().Subtract(later.ToDateTime()) < window;

		public void TestCancelRejectionEmail()
		{
			var factory = new BusinessObjectFactory();
			var application = AutomatedRejectionTestHelper.CreateApplication(factory, "Bob Ross");
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var otherLog = application.Logs.AddNew(AutoEvents.EditedARecord, $"test");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			AssertEquals(expected: false, otherLog.IsCancelled);

			var log = application.QueueRejectionEmail();
			AssertEquals(expected: false, log.IsCancelled);

			application.CancelRejectionEmail();
			AssertEquals(expected: true, log.IsCancelled);
			AssertEquals(expected: false, otherLog.IsCancelled);
		}

		public void TestCancelRejectionEmailMultiple()
		{
			var factory = new BusinessObjectFactory();
			var application = AutomatedRejectionTestHelper.CreateApplication(factory, "Bob Ross");

			for (var i = 0; i < 10; ++i)
			{
				_ = application.QueueRejectionEmail();
			}

			AssertEquals(expected: true, HasActiveRejectionLogs(application));
			AssertEquals(10, GetRejectionLogs(application).Count());

			application.CancelRejectionEmail();
			AssertEquals(expected: false, HasActiveRejectionLogs(application));
			AssertEquals("logs should be cancelled, not deleted", 10, GetRejectionLogs(application).Count());
		}

		const string RejectionEmailSentEvent = nameof(HRJobApplicationEvent.RejectionEmailSent);

		IEnumerable<StmALog> GetLogs(HRJobApplication application) => application
			.Logs
			.Find(l => l.SL_Parent == application.PK);

		IEnumerable<StmALog> GetRejectionLogs(HRJobApplication application) => GetLogs(application)
			.Where(l => l.Event.SE_Code == AutoEvents.RejectionEmailQueued.Code);

		bool HasActiveRejectionLogs(HRJobApplication application) => GetLogs(application)
			.Where(l => l.Event.SE_Code == AutoEvents.RejectionEmailQueued.Code)
			.Any(l => !l.IsCancelled);

		bool HasSentRejectionLogs(HRJobApplication application) => GetLogs(application)
			.Where(l => l.Event.SE_Code == AutoEvents.RecruitmentCandidateEvent.Code)
			.Where(l => l.Parameters.TryGetValue("EVT", out var eventType) && eventType == RejectionEmailSentEvent)
			.Any(l => !l.IsCancelled);

		public void TestAnySentRejectionLogs_NoLogs()
		{
			var factory = new BusinessObjectFactory();
			var application = AutomatedRejectionTestHelper.CreateApplication(factory, "Bob Ross");
			factory.Save();
			AssertEquals(expected: false, application.AnySentRejectionLogs());
		}

		public void TestAnySentRejectionLogs_WithSentLog()
		{
			var factory = new BusinessObjectFactory();
			var application = AutomatedRejectionTestHelper.CreateApplication(factory, "Bob Ross");
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var log = application.Logs.AddNew(AutoEvents.RecruitmentCandidateEvent, string.Empty);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.Parameters["EVT"] = RejectionEmailSentEvent;
			}

			AssertEquals(expected: true, application.AnySentRejectionLogs());
			AssertEquals(expected: true, HasSentRejectionLogs(application));
		}

		public void TestAnySentRejectionLogs_WithNonSentLog()
		{
			var factory = new BusinessObjectFactory();
			var application = AutomatedRejectionTestHelper.CreateApplication(factory, "Bob Ross");
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			_ = application.Logs.AddNew(AutoEvents.RecruitmentCandidateEvent, string.Empty);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			AssertEquals(expected: false, application.AnySentRejectionLogs());
		}
	}
}
