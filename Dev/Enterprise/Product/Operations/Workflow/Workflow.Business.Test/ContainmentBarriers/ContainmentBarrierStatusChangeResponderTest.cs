using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestDate(2014, 8, 20)]
	class ContainmentBarrierStatusChangeResponderTest : WorkflowTestCase
	{
		#region RespondsToStatusChange

		public void TestRespondsToStatusChange()
		{
			AssertEquals(true, responder.RespondsToStatusChange(task, "CLS"));
			AssertEquals(true, responder.RespondsToStatusChange(task, "CAN"));

			AssertEquals(false, responder.RespondsToStatusChange(task, "OPN"));
			AssertEquals(false, responder.RespondsToStatusChange(task, "ASN"));
			AssertEquals(false, responder.RespondsToStatusChange(task, "SUS"));
			AssertEquals(false, responder.RespondsToStatusChange(task, "WRK"));
		}

		public void TestRespondsToStatusChange_NonQCBTask()
		{
			var nonQCBTask = (ProcessTask)BMTestHelper.CreateTask((BusinessObject)Factory.New<IWorkItem>());

			AssertEquals(false, responder.RespondsToStatusChange(nonQCBTask, "CLS"));
			AssertEquals(false, responder.RespondsToStatusChange(nonQCBTask, "CAN"));

			AssertEquals(false, responder.RespondsToStatusChange(nonQCBTask, "OPN"));
			AssertEquals(false, responder.RespondsToStatusChange(nonQCBTask, "ASN"));
			AssertEquals(false, responder.RespondsToStatusChange(nonQCBTask, "SUS"));
			AssertEquals(false, responder.RespondsToStatusChange(nonQCBTask, "WRK"));
		}

		public void TestRespondsToStatusChange_StandaloneTask()
		{
			var task = Factory.New<ProcessTask>();
			AssertEquals(false, responder.RespondsToStatusChange(task, "CLS"));
		}

		#endregion

		#region RespondToChange

		[GuiTest]
		public void TestRespondToChange()
		{
			using (var viewModel = new ContainmentBarrierViewModel(task, "CLS", deselectCancelledTasksFromIteration: true))
			{
				var view = new DummyView();
				using (ObjectFactory.Substitute<IContainmentBarrierResponseView>(view))
				{
					DummyView.Response_ForTest = ContainmentBarrierResponses.Canceled;
					AssertEquals(StatusChangeResult.ChangeNotHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.Passed;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.IterationRequired;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.DeferredToAnotherResource;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.None;
					AssertEquals(StatusChangeResult.Unknown, responder.RespondToChange(task, "CLS"));
				}
			}
		}

		[GuiTest]
		public void TestRespondToChange_TaskCancelled()
		{
			var view = new DummyViewThatThrowsExceptionOnView();
			using (ObjectFactory.Substitute<IContainmentBarrierResponseView>(view))
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				var result = responder.RespondToChange(task, "CAN");
				AssertEquals(StatusChangeResult.ChangeHandled, result);

				AssertRtfText("FRO 20-Aug-14 00:00: Containment Barrier verification canceled", task.P9_Notes);
			}
		}

		public void TestRespondToChange_CantShowDialog()
		{
			using (var viewModel = new ContainmentBarrierViewModel(task, "CLS", deselectCancelledTasksFromIteration: true))
			{
				var view = new DummyView();
				using (ObjectFactory.Substitute<IContainmentBarrierResponseView>(view))
				using (Globals.SetIsUserInteractiveForTest(true))
				{
					Globals.IsConsoleSession = false;
					Globals.IsWebService = false;

					DummyView.Response_ForTest = ContainmentBarrierResponses.Canceled;
					AssertEquals(StatusChangeResult.ChangeNotHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.Passed;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.IterationRequired;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.DeferredToAnotherResource;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.None;
					AssertEquals(StatusChangeResult.Unknown, responder.RespondToChange(task, "CLS"));

					Globals.IsConsoleSession = true;
					Globals.IsWeb = false;

					DummyView.Response_ForTest = ContainmentBarrierResponses.Canceled;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.Passed;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.IterationRequired;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.DeferredToAnotherResource;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.None;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));

					Globals.IsConsoleSession = false;
					Globals.IsWeb = true;

					DummyView.Response_ForTest = ContainmentBarrierResponses.Canceled;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.Passed;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.IterationRequired;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.DeferredToAnotherResource;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));

					DummyView.Response_ForTest = ContainmentBarrierResponses.None;
					AssertEquals(StatusChangeResult.ChangeHandled, responder.RespondToChange(task, "CLS"));
				}
			}
		}

		#endregion

		#region Implementation

		ProcessTask task;
		IStatusChangeResponder responder;
		IDisposable userContextDisposable;

		protected override void SetUp()
		{
			base.SetUp();

			EnableBufferManagement();
			SetAsQCBTaskType("QCB", "WKI");

			var job = (IWorkflowProvider)Factory.New<IWorkItem>();
			task = job.WorkflowItems.Tasks.AddNew();
			task.P9_Type = "QCB";

			responder = ObjectFactory.Get<System.Collections.IEnumerable>("TaskStatusChangeResponders").OfType<ContainmentBarrierStatusChangeResponder>().Single();

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_Code = "FRO";
			Factory.Save();

			userContextDisposable = Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
		}

		protected override void TearDown()
		{
			base.TearDown();
			userContextDisposable.Dispose();
		}

		class DummyView : IContainmentBarrierResponseView
		{
			internal static ContainmentBarrierResponses Response_ForTest { get; set; }

			ContainmentBarrierResponses IContainmentBarrierResponseView.GetResponseFromUser()
			{
				return Response_ForTest;
			}
		}

		class DummyViewThatThrowsExceptionOnView : IContainmentBarrierResponseView
		{
			ContainmentBarrierResponses IContainmentBarrierResponseView.GetResponseFromUser()
			{
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}
