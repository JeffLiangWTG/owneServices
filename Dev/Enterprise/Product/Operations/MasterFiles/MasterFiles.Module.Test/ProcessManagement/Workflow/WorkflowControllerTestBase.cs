using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	abstract class WorkflowControllerTestBase : ZControllerBasherTest
	{
		[RequiresSTA]
		public override void TestViewForm()
		{
			AssertShowForm((controller, task) => controller.ShowViewForm(task));
		}

		[RequiresSTA]
		public override void TestEditForm()
		{
			AssertShowForm((controller, task) => controller.ShowEditForm(task));
		}

		void AssertShowForm(Func<ZController, ProcessTask, IZForm> showFormAction)
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var processTaskWithParent = AddWorkflowItemToParent(parent);
			var processTaskWithoutParent = Factory.New<ProcessTask>();

			Factory.Save();

			var controller = ZControllerFactory.Create(GetControllerID());

			using (var form = showFormAction(controller, processTaskWithoutParent))
			{
				AssertNull(form);
			}

			using (var form = showFormAction(controller, processTaskWithParent))
			{
				AssertNotNull(form);
			}
		}

		protected abstract ProcessTask AddWorkflowItemToParent(IWorkflowProvider parent);

		public void TestShowForm_WithoutPermission_ShouldNotThrowExceptions()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			var workflowItem = AddWorkflowItemToParent(job);

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var controller = ZControllerFactory.Create(GetControllerID());
				AssertEquals(false, controller.GetCheckPointForView(job).IsAllowed);
				AssertEquals(false, controller.GetCheckPointForEdit(job).IsAllowed);

				IZForm form = null;

				try
				{
					AssertNoExceptionThrown(() => form = controller.ShowViewForm(workflowItem));
					AssertNull(form);

					AssertNoExceptionThrown(() => form = controller.ShowEditForm(workflowItem));
					AssertNull(form);
				}
				finally
				{
					form?.Dispose();
				}
			}

			AssertStartsWith("Got that error message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[RequiresSTA]
		public void TestRememberRecent()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var exception = AddWorkflowItemToParent(job);
			exception.P9_Description = "Mega Mileage";
			Factory.Save();

			var controller = ZControllerFactory.Create(GetControllerID());
			using (controller.ShowEditForm(exception))
			{
				UserIdleWorker.Flush();
				Application.DoEvents();
				var link = RecentItemManager.Instance.GetRecentItems(controller.ModuleID.Name).First();
				AssertEquals(exception.PK, link.STL_ItemPK);
			}
			Application.DoEvents();
		}
	}
}
