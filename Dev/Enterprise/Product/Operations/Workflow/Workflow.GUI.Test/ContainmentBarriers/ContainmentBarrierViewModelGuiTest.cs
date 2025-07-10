using System;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Workflow.Business;
using Enterprise.Workflow.Business.Test;

namespace Enterprise.Workflow.GUI.Test
{
	class ContainmentBarrierViewModelGuiTest : TestCaseWithFactory
	{
		public void TestGetDisposableLeakListenerTrackedObjects_ShouldRequireApplicationDoEvents()
		{
			WorkflowTestCase.EnableBufferManagement();
			WorkflowTestCase.SetAsQCBTaskType("QCB", "WKI");

			var invokeActionCalled = false;

			using (var form = new Form()) // Need this form so we can access the WinForms message queue. It mustn't be a ZForm because that would be tracked as a GUI element, forcing DoEvents to be called before we want it to.
			{
				form.Show();
				form.BeginInvoke(new Action(() => invokeActionCalled = true));

				AssertEquals("Pre-condition: we've not yet called DoEvents anywhere in this test", false, invokeActionCalled);

				DisposableLeakListener.Instance.GetDisposedNotCollectedTypes();

				AssertEquals("Getting disposed, non-GCd objects does not yet call DoEvents because it's not tracking anything GUI-related yet.", false, invokeActionCalled);

				var qcbTask = WorkflowTestCase.BMTestHelper.CreateTask((BusinessObject)Factory.New<IWorkItem>(), GlbStaff.CurrentUser.GS_Code, taskType: "QCB");

				using (var viewModel = new ContainmentBarrierViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed, deselectCancelledTasksFromIteration: true))
				{
					AssertEquals("Creating a viewmodel shouldn't call DoEvents because that may cause defects in production", false, invokeActionCalled);

					DisposableLeakListener.Instance.GetDisposedNotCollectedTypes();

					AssertEquals("Getting disposed, non-GCd objects should now call DoEvents, because it's tracking a disposable type that touches GUI elements - the viewModel.", true, invokeActionCalled);
				}
			}
		}
	}
}
