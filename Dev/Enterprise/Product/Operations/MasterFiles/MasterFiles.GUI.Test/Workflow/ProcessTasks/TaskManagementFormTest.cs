using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(TaskManagementForm))]
	sealed class TaskManagementFormTest : ZFormBasherTest
	{
		public void TestNotStandAloneTask_DontLoadOrgContacts()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsActive, true));
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ConkBoy";
			contact.OC_Email = "chimmy@fill.em";
			contact.SetHashedPassword("DroppingTheBassEveryDay");

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Tasks.AddNew();
			task.P9_OC = contact.PK;
			task.P9_OA = org.MainAddress.PK;

			Factory.Save();
			var newTask = Factory.CreateNewFactory().Load<ProcessTask>(task.PK);

			using (var form = new TaskManagementForm(newTask))
			{
				form.Show();
				Application.DoEvents();
				var contacts = newTask.Factory.Load<OrgContact>(new ZQuery { FetchOnlyFromLocalCache = true });
				AssertEquals("Don't load the contacts.", 0, contacts.Length);
			}
		}

		public void TestStandAloneTask_CanSelectAddress()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsActive, true));
			var task = Factory.New<ProcessTask>();
			task.OrganisationPK = org.PK;

			using (var form = new TaskManagementForm(task))
			{
				form.Show();
				Application.DoEvents();

				var addressControl = form.FindAll<ZAddressControl>().Single(s => s.Name == "P9_OAAddressControl").FindAll<ZDropEdit>().Single();
				AssertEquals(false, addressControl.ReadOnly);
			}
		}

		public void TestShowParent()
		{
			ProcessTask standaloneTask = Factory.New<ProcessTask>();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			ProcessTask oppTask = opp.WorkflowItems.AddNew();
			Factory.Save();

			using (TaskManagementForm form = new TaskManagementForm(standaloneTask))
			{
				form.Show();
				form.ShowParentButton.PerformClick();
				AssertEquals("Error shown", "This is a stand-alone task.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(form.lastShownParentForm);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (TaskManagementForm form = new TaskManagementForm(oppTask))
			{
				form.Show();
				form.ShowParentButton.PerformClick();
				AssertNull("No Error shown", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(form.lastShownParentForm);
				form.lastShownParentForm.Dispose();
			}
		}

		public void TestShowParent_WithoutPermission_ShouldNotThrowExceptions()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNewMainAddress();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.O1_OA_LinkedAddress = address.PK;
			var task = job.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var controller = ZControllerFactory.Create(task.ParentControllerID);
				AssertEquals(false, controller.GetCheckPointForView(job).IsAllowed);
				AssertEquals(false, controller.GetCheckPointForEdit(job).IsAllowed);

				using (var taskForm = new TaskManagementForm(task))
				{
					taskForm.Show();
					Application.DoEvents();

					try
					{
						AssertNoExceptionThrown(() =>
						{
							taskForm.ShowParentButton.PerformClick();
							Application.DoEvents();
						});
					}
					finally
					{
						taskForm.lastShownParentForm?.Dispose();
					}
				}
			}

			AssertEquals("Access Denied: Search and View Records Assigned to Other Login Staff", UnitTestUserNotification.Instance.LastMessage.Caption);
		}

		[DeveloperOnlyTest]
		public void TestCopyToHyperlink()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			ProcessTask orgTask = org.WorkflowItems.AddNew();

			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			ProcessTask oppTask = opp.WorkflowItems.AddNew();
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.ProcessTasks);
			AssertEquals("precondition", true, org.WorkflowItems.AreTasksCompanySpecific);
			oppTask.P9_ShareTasksForAllCompanies = false;

			using (TaskManagementForm form = (TaskManagementForm)controller.ShowEditForm(orgTask))
			{
				form.Show();
				form.ActionsMenuItem.MenuItems[0].PerformClick();
				AssertEquals("Clipboard has licence code", true, ((string)SafeClipboard.GetData(DataFormats.Html)).Contains("ShowEditForm&LicenceCode="));
			}

			orgTask.P9_ShareTasksForAllCompanies = true;
			Factory.Save();

			using (TaskManagementForm form = (TaskManagementForm)controller.ShowEditForm(orgTask))
			{
				form.Show();
				form.ActionsMenuItem.MenuItems[0].PerformClick();
				AssertEquals("Clipboard doesn't have licence code", false, ((string)SafeClipboard.GetData(DataFormats.Html)).Contains("ShowEditForm&LicenceCode="));
			}

			controller = ZControllerFactory.Create(ControllerIDs.ProcessTasks);
			AssertEquals("precondition", false, opp.WorkflowItems.AreTasksCompanySpecific);
			oppTask.P9_ShareTasksForAllCompanies = true;
			Factory.Save();

			using (TaskManagementForm form = (TaskManagementForm)controller.ShowEditForm(oppTask))
			{
				form.Show();
				form.ActionsMenuItem.MenuItems[0].PerformClick();
				AssertEquals("Clipboard doesn't have licence code", false, ((string)SafeClipboard.GetData(DataFormats.Html)).Contains("ShowEditForm&LicenceCode="));
			}
		}

		public void TestTaskManagementFormDisplaysCompletedLocalTime()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			ProcessTask orgTask = org.WorkflowItems.AddNew();

			orgTask.P9_CompletedTime = ZDateTimeOffset.UtcToday;
			orgTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNotNull("Pre-condition", orgTask.CompletedTimeLocal);

			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.ProcessTasks);

			using (TaskManagementForm form = (TaskManagementForm)controller.ShowEditForm(orgTask))
			{
				form.Show();
				AssertEquals("CompletedTimeLocal ZDateEdit should be in the SchedulingGroupBox.Controls", true, form.SchedulingGroupBox.Controls.Find("CompletedTimeLocal", true).Length > 0);
				AssertEquals(typeof(ZDateEdit), form.SchedulingGroupBox.Controls.Find("CompletedTimeLocal", true)[0].GetType());
			}
		}

		public void TestBMSTabsHiddenByRegistry()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = helper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "No");
			var task = (ProcessTask)helper.CreateTask(workflow);
			workflow.FH_CompletionStatement = "I am a completion statement!";

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;

			Factory.Save();

			using (TaskManagementForm form = new TaskManagementForm(task))
			{
				Assert("When BMS is disabled, hide the workflow tab.", !form.WorkflowDetailsTabPage.TabVisible);
				Assert("When BMS is disabled, hide the tags tab.", !form.BMTagsTab.TabVisible);
			}

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			Factory.Save();

			using (TaskManagementForm form = new TaskManagementForm(task))
			{
				Assert("It looks like BMS is back! Workflows are your friend.", form.WorkflowDetailsTabPage.TabVisible);
				Assert("It looks like BMS is back! Tags are your friend.", form.BMTagsTab.TabVisible);
			}
		}

		#region Test EDocs Visibility

		public void TestEDocsVisibilityWhenProcessTaskIsInWorkflowTemplate()
		{
			ProcessTask standaloneTask = Factory.New<ProcessTask>();
			TemplateProcessTask templateProcessTask = Factory.New<TemplateProcessTask>();

			using (TaskManagementForm form = new TaskManagementForm(standaloneTask))
			{
				form.Show();
				Assert("EDocs should be visible", form.SupportsEDocs);
			}

			using (TaskManagementForm form = new TaskManagementForm(templateProcessTask))
			{
				form.Show();
				Assert("EDocs should not be visible", !form.SupportsEDocs);
			}
		}

		#endregion

		public void TestTemplateTaskObeysSecurity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNewMainAddress();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var templateProcessTask = template.WorkflowItems.Tasks.AddNew();
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.WorkflowTaskTemplatesView.IsAllowed = true;
				Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = false;
				Env.Security.WorkflowTaskTemplatesEditInactive.IsAllowed = false;

				var controller = ZControllerFactory.Create(ControllerIDs.ProcessTasks);
				using (var form = (TaskManagementForm)controller.ShowEditForm(templateProcessTask))
				{
					AssertEquals(true, controller.GetCheckPointForView(templateProcessTask).IsAllowed);
					AssertEquals(false, controller.GetCheckPointForEdit(templateProcessTask).IsAllowed);

					form.Show();
					Assert("Form should be read-only", form.P9_NotesRichTextBox.ReadOnly);
				}
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.WorkflowTaskTemplatesView.IsAllowed = true;
				Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = true;
				Env.Security.WorkflowTaskTemplatesEditInactive.IsAllowed = false;

				var controller = ZControllerFactory.Create(ControllerIDs.ProcessTasks);
				using (var form = (TaskManagementForm)controller.ShowEditForm(templateProcessTask))
				{
					AssertEquals(true, controller.GetCheckPointForView(templateProcessTask).IsAllowed);
					AssertEquals(true, controller.GetCheckPointForEdit(templateProcessTask).IsAllowed);

					form.Show();
					Assert("Form should be editable", !form.P9_NotesRichTextBox.ReadOnly);
				}
			}
		}

		public void TestOrgDetailsVisibleBasedOnParentOpportunity()
		{
			// Supports contacts and addresses
			OrgOpportunity opportunity = Factory.New<OrgOpportunity>();
			ProcessTask task1 = opportunity.WorkflowItems.AddNew();

			// No parent - so implicitly supports contacts and addresses
			ProcessTask task2 = Factory.New<ProcessTask>();

			// Does NOT support contacts and addresses

			DummyWithWorkflow dummyWithTasks = Factory.New<DummyWithWorkflow>();
			var task3 = dummyWithTasks.WorkflowItems.AddNew();

			using (TaskManagementForm form = new TaskManagementForm(task1))
			{
				form.Show();
				AssertEquals("Org Details panel shown", false, form.OrgDetailsPanel.IsDisposed);
			}

			using (TaskManagementForm form = new TaskManagementForm(task2))
			{
				form.Show();
				AssertEquals("Org Details panel shown for a stand-alone task", false, form.OrgDetailsPanel.IsDisposed);
			}

			using (TaskManagementForm form = new TaskManagementForm(task3))
			{
				form.Show();
				AssertEquals("Org Details panel NOT shown for this task type", true, form.OrgDetailsPanel.IsDisposed);
			}
		}

		[CargoWise.Data.Testing.UseSnapshotProtection(true)]
		public void TestTaskOwnerPasswordRequestedEvent()
		{
			SetupDummyStandaloneWorkflowTaskTypes();

			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_Type = "RVW";
			task.P9_GS_NKAssignedStaffMember = "ZZ";

			using (TaskManagementForm form = new TaskManagementForm(task))
			{
				Assert("Should be managed by TaskOwnerPasswordEventManager", Enterprise.MasterFiles.GUI.Testing.TaskOwnerPasswordEventManagerTest.IsManaged(task));
				form.Show();
				AssertNull("Precondition", ZFormModaliser.LastFormShownDialogForTest);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				AssertEquals(typeof(TaskOwnerPasswordRequestForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}

			Assert("Should be unhooked on dispose", !Enterprise.MasterFiles.GUI.Testing.TaskOwnerPasswordEventManagerTest.IsManaged(task));
		}

		public void TestFormCaption()
		{
			ProcessTask standaloneTask = Factory.New<ProcessTask>();
			Factory.Save();

			using (TaskManagementForm form = new TaskManagementForm(standaloneTask))
			{
				form.Show();
				AssertEquals("Caption must contain Edit Task [TaskID]", true, form.Text.Contains(String.Format("{0} {1}", "Edit Task", standaloneTask.P9_TaskID)));
			}
		}

		public void TestDeletedBusinessObject()
		{
			ProcessTask standaloneTask = Factory.New<ProcessTask>();
			Factory.Save();

			using (TaskManagementForm form = new TaskManagementForm(standaloneTask))
			{
				form.Show();
				form.BusinessEntity.Delete();
				AssertNoExceptionThrown(() => { var caption = form.FormCaption; });
			}
		}

		void SetupDummyStandaloneWorkflowTaskTypes()
		{
			CategorisedWorkflowTaskTypesCollection categorisedTaskTypesCollection = new CategorisedWorkflowTaskTypesCollection();
			CategorisedWorkflowTaskTypes categorisedTaskTypes = categorisedTaskTypesCollection.AddNew();
			categorisedTaskTypes.Code = "STA";
			WorkflowTaskType codingTask = categorisedTaskTypes.TaskTypes.AddNew();
			codingTask.Code = "COD";
			WorkflowTaskType reviewTask = categorisedTaskTypes.TaskTypes.AddNew();
			reviewTask.Code = "RVW";
			reviewTask.CanCloseTaskNotAssignedToSelf = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypesCollection);
			Env.Security.WorkflowTasksCloseTaskNotAssignedToSelf.IsAllowed = false;
		}

		public void TestWhenChangeStatusViaTaskManagementForm_ShouldSetTaskStatusChangeModeToTSK()
		{
			var processTaskStatusChangeModeTracker = ObjectFactory.Get<ProcessTaskStatusChangeModeTracker>();
			processTaskStatusChangeModeTracker.Clear();

			AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.Other, processTaskStatusChangeModeTracker.Current);

			var workflow = Factory.New<DummyWithWorkflow>();
			var user = Factory.New<GlbStaff>();
			var task = workflow.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = user.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			using (var form = new TaskManagementForm(task))
			{
				form.Show();

				var statusDropEdit = form.FindSingle<ZDropEdit>("P9_StatusDropEdit");
				statusDropEdit.Focus();
				statusDropEdit.SelectItem(ProcessTaskStatusCodeList.Codes.Working);
				statusDropEdit.CommitBoundValue();

				AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.EditTaskScreen, processTaskStatusChangeModeTracker.Current);

				form.FindSingle<ZTextBox>("IDTextBox").Focus();
				Factory.Save();
				Application.DoEvents();
			}

			var lastLogReference = task.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;

			AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.Other, processTaskStatusChangeModeTracker.Current);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.EditTaskScreen}", lastLogReference);
		}

		public void TestControlsCharacterCasingShouldBeUpperCase()
		{
			using (var form = (TaskManagementForm)GetFormToBashCore())
			{
				AssertEquals(CharacterCasing.Upper, form.P9_StatusDropEdit.CharacterCasing);
				AssertEquals(CharacterCasing.Upper, form.P9_TypeDropEdit.CharacterCasing);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			return new TaskManagementForm(Factory.New<ProcessTask>());
		}

		#endregion
	}
}
