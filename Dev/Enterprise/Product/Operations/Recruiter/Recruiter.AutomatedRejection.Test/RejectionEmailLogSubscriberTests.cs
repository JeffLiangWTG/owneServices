using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.AutomatedRejection;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Registry;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Testing.AutomatedRejection
{
	[TestedType(typeof(RejectionEmailLogSubscriber))]
	sealed class RejectionEmailLogSubscriberTests : LogSubscriberTest<RejectionEmailLogSubscriber>
	{
		public void TestDetails()
		{
			var subscriber = new RejectionEmailLogSubscriber();

			AssertEquals("RejectionEmailLogSubscriber", subscriber.Name);

			AssertEquals("Rejection Email n-Day Log Subscriber", subscriber.FriendlyName);

			AssertEquals(1, subscriber.TableNames.Length);
			AssertEquals("HRJobApplication", subscriber.TableNames.First());

			AssertEquals(1, subscriber.EventTypes.Length);
			AssertEquals("REQ", subscriber.EventTypes.First());
		}

		[Serializable]
		class TestRejectionEmailLogSubscriber : RejectionEmailLogSubscriber
		{
			public void ProcessLogQueueItems_Exposed(IQueuedLog[] queuedLogs) => base.ProcessLogQueueItems(queuedLogs);
		}

		public void TestProcessLogQueueItems()
		{
			var factory = new BusinessObjectFactory();
			var hrJobApplication = factory.NewWithValidTestData<HRJobApplication>();
			factory.Save();

			// Queue a rejection email
			var handler = new AutomatedEmailRejectionHandler();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			handler.QueueRejectionEmail(hrJobApplication);
			factory.Save();

			var testQueuedLog = (BusinessObject)factory.New<IQueuedLog>();
			testQueuedLog[StmJobQueueSchema.SJ_ParentID] = hrJobApplication.PK;
			testQueuedLog[StmJobQueueSchema.SJ_SE_NKEvent] = AutoEvents.RejectionEmailQueued.Code;
			testQueuedLog[StmJobQueueSchema.SJ_GS_NKUser] = GlbStaff.CurrentUser.GS_Code;
			testQueuedLog[StmJobQueueSchema.SJ_Reference] = $"Test|{GlbBranch.CurrentBranch}|{GlbDepartment.CurrentDepartment}";

			var testAutomatedEmailRejectionHandler = new MockAutomatedEmailRejectionHandler();
			using (ObjectFactory.Substitute<IAutomatedEmailRejectionHandler>(testAutomatedEmailRejectionHandler))
			{
				var subscriber = new TestRejectionEmailLogSubscriber();
				AssertEquals(false, testAutomatedEmailRejectionHandler.SendRejectionEmailWasCalled);
				subscriber.ProcessLogQueueItems_Exposed([(IQueuedLog)testQueuedLog]);
				AssertEquals(true, testAutomatedEmailRejectionHandler.SendRejectionEmailWasCalled);
			}
		}

		public void TestProcessLogQueueItemsWithLWK()
		{
			var factory = new BusinessObjectFactory();
			var application = factory.NewWithValidTestData<HRJobApplication>();
			factory.Save();

			using (RecruitmentDataRegistry.Instance.AutomatedRejection_DaysToDelaySendingEmail.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, -5))
			{
				var handler = new AutomatedEmailRejectionHandler();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				handler.QueueRejectionEmail(application);
				factory.Save();

				var testAutomatedEmailRejectionHandler = new MockAutomatedEmailRejectionHandler();
				using (ObjectFactory.Substitute<IAutomatedEmailRejectionHandler>(testAutomatedEmailRejectionHandler))
				{
					AssertEquals(false, testAutomatedEmailRejectionHandler.SendRejectionEmailWasCalled);
					_ = ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
					AssertEquals(true, testAutomatedEmailRejectionHandler.SendRejectionEmailWasCalled);
				}
			}
		}
	}
}
