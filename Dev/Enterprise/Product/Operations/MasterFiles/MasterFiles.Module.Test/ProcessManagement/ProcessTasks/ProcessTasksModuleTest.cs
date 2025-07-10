using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ProcessTasksModule))]
	public class ProcessTasksModuleTest : ZModuleBasherTest
	{
		public void TestHasOperationalActionsPlugin()
		{
			using (processTasks = new ProcessTasksModuleForTest())
			{
				AssertNotNull(processTasks.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		public void TestHasOperationalActions()
		{
			using (processTasks = new ProcessTasksModuleForTest())
			{
				var supportable = processTasks as IOperationalActionSupportable;
				AssertNotNull(supportable);
				AssertNotNull(supportable.OperationalActionSupporter);
			}
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.Workflow, Module.LicenceCheckPoint);
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ProcessTasks;
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			processTasks = new ProcessTasksModuleForTest();
			IFilterControl controlForTest = processTasks.GetNewFilterControlForTest();
			Assert(controlForTest is ProcessTaskFilterControl);
			controlForTest.Dispose();
			processTasks.Dispose();
		}

		public void TestGridCollection()
		{
			processTasks = new ProcessTasksModuleForTest();
			IBusinessObjectCollection collectionForTest = processTasks.GetNewGridCollectionForTest();
			Assert(collectionForTest is BusinessObjectCollection);
			processTasks.Dispose();
		}

		public void TestFilterBusinessObject()
		{
			processTasks = new ProcessTasksModuleForTest();
			FilterBusinessObject businessForTest = processTasks.GetNewFilterBusinessObjectForTest();
			Assert(businessForTest is FilterBusinessObject);
			processTasks.Dispose();
		}

		[RequiresSTA]
		public void TestDeleteAction()
		{
			processTasks = new ProcessTasksModuleForTest();
			try
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				ProcessTask task1 = processTasks.GridCollection.AddNew() as ProcessTask;
				task1.P9_TaskID = "1";
				ProcessTask task2 = processTasks.GridCollection.AddNew() as ProcessTask;
				task2.P9_TaskID = "2";
				ProcessTask task3 = processTasks.GridCollection.AddNew() as ProcessTask;
				task3.P9_TaskID = "3";

				processTasks.selectedElements = new ProcessTask[2];
				processTasks.selectedElements[0] = task1;
				processTasks.selectedElements[1] = task2;

				task3.Factory.Save();

				List<MenuItem> menus = new List<MenuItem>(processTasks.FormActionMenu);

				MenuItem deleteMenuItem = menus.FindByText("&Delete");

				AssertNotNull(deleteMenuItem);

				deleteMenuItem.PerformClick();

				processTasks.GridForTest.Refresh();
				AssertEquals(1, processTasks.GridCollection.Count);
				AssertEquals(task3, processTasks.GridCollection[0]);

				Env.Security.WorkflowTasksDelete.IsAllowed = false;

				processTasks.selectedElements = new ProcessTask[1];
				processTasks.selectedElements[0] = task3;

				deleteMenuItem.PerformClick();

				processTasks.GridForTest.Refresh();
				AssertEquals(1, processTasks.GridCollection.Count);
				AssertEquals(task3, processTasks.GridCollection[0]);
			}
			finally
			{
				processTasks.Dispose();
			}
		}

		public void TestDeleteAction_HandleConcurrencyError()
		{
			using (processTasks = new ProcessTasksModuleForTest())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var task1 = (ProcessTask)processTasks.GridCollection.AddNew();
				task1.P9_TaskID = "1";

				processTasks.selectedElements = new[] { task1 };

				task1.Factory.Save();
				task1.Factory.RefreshEnabled = false;

				var loadedTaskOne = new BusinessObjectFactory { RefreshEnabled = false }.Load<ProcessTask>(task1.PK);

				loadedTaskOne.P9_Description = "Live";
				loadedTaskOne.P9_TaskID = "EVERY";
				loadedTaskOne.P9_NotesAsString = "Day";
				loadedTaskOne.Factory.Save();

				var deleteMenuItem = processTasks.FormActionMenu.ToList().FindByText("&Delete");

				var notificationHandler = NotificationHandler.Instance;
				var dummyHandler = new DummyNotificationHandler();
				try
				{
					NotificationHandler.Instance = dummyHandler;
					deleteMenuItem.PerformClick();
					AssertNotNull(dummyHandler.LastError);
					AssertEquals("Concurrency error means no delete", 1, processTasks.GridCollection.Count);
				}
				finally
				{
					NotificationHandler.Instance = notificationHandler;
				}
			}
		}

		#region Test View / Edit Menu

		public void TestNoTasksSelected_View()
		{
			using (processTasks = new ProcessTasksModuleForTest())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var menuItem = processTasks.FormActionMenu.ToList().FindByText("&View");
				AssertNotNull(menuItem);

				menuItem.PerformClick();
				Application.DoEvents();

				AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNoTasksSelected_Edit()
		{
			using (processTasks = new ProcessTasksModuleForTest())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var menuItem = processTasks.FormActionMenu.ToList().FindByText("&Edit");
				AssertNotNull(menuItem);

				menuItem.PerformClick();
				Application.DoEvents();

				AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestStandaloneTask_View_WithBMEnabled()
		{
			ViewEditMenuTest<TaskManagementForm>(true, string.Empty, "&View", string.Empty);
		}

		public void TestStandaloneTask_Edit_WithBMEnabled()
		{
			ViewEditMenuTest<TaskManagementForm>(true, string.Empty, "&Edit", string.Empty);
		}

		public void TestStandaloneTask_View_WithBMDisabled()
		{
			ViewEditMenuTest<TaskManagementForm>(false, string.Empty, "&View", string.Empty);
		}

		public void TestStandaloneTask_Edit_WithBMDisabled()
		{
			ViewEditMenuTest<TaskManagementForm>(false, string.Empty, "&Edit", string.Empty);
		}

		public void TestStandaloneTask_DefaultAction_WithBMEnabled()
		{
			ViewEditMenuTest<TaskManagementForm>(true, string.Empty, string.Empty, string.Empty);
		}

		public void TestStandaloneTask_DefaultAction_WithBMDisabled()
		{
			ViewEditMenuTest<TaskManagementForm>(false, string.Empty, string.Empty, string.Empty);
		}

		public void TestTaskWithParentJobHeader_View_WithBMEnabled()
		{
			ViewEditMenuTest<ZOrganisationsForm>(true, "ORG", "&View", "Management");
		}

		public void TestTaskWithParentJobHeader_Edit_WithBMEnabled()
		{
			ViewEditMenuTest<ZOrganisationsForm>(true, "ORG", "&Edit", "Management");
		}

		public void TestTaskWithParentJobHeader_View_WithBMDisabled()
		{
			ViewEditMenuTest<ZOrganisationsForm>(false, "ORG", "&View", "Tasks");
		}

		public void TestTaskWithParentJobHeader_Edit_WithBMDisabled()
		{
			ViewEditMenuTest<ZOrganisationsForm>(false, "ORG", "&Edit", "Tasks");
		}

		public void TestTaskWithParentJobHeader_View_WithBMEnabled_ButTaskTypeUnknownToSystem()
		{
			ViewEditMenuTest<ZOrganisationsForm>(true, "ABC", "&View", "Tasks");
		}

		public void TestTaskWithParentJobHeader_Edit_WithBMEnabled_ButTaskTypeUnknownToSystem()
		{
			ViewEditMenuTest<ZOrganisationsForm>(true, "ABC", "&Edit", "Tasks");
		}

		public void TestTaskWithParentJobHeader_View_WithBMEnabled_AndOpenTaskFormByDefault()
		{
			ViewEditMenuTest<TaskManagementForm>(true, "ORG", "&View", string.Empty, openTaskFormByDefault: true);
		}

		public void TestTaskWithParentJobHeader_Edit_WithBMEnabled_AndOpenTaskFormByDefault()
		{
			ViewEditMenuTest<TaskManagementForm>(true, "ORG", "&Edit", string.Empty, openTaskFormByDefault: true);
		}

		public void TestTaskWithParentJobHeader_View_WithBMDisabled_AndOpenTaskFormByDefault()
		{
			ViewEditMenuTest<TaskManagementForm>(false, "ORG", "&View", string.Empty, openTaskFormByDefault: true);
		}

		public void TestTaskWithParentJobHeader_Edit_WithBMDisabled_AndOpenTaskFormByDefault()
		{
			ViewEditMenuTest<TaskManagementForm>(false, "ORG", "&Edit", string.Empty, openTaskFormByDefault: true);
		}

		public void TestTaskWithParentJobHeader_View_WithBMEnabled_ButTaskTypeUnknownToSystem_AndOpenTaskFormByDefault()
		{
			ViewEditMenuTest<TaskManagementForm>(true, "ABC", "&View", string.Empty, openTaskFormByDefault: true);
		}

		public void TestTaskWithParentJobHeader_Edit_WithBMEnabled_ButTaskTypeUnknownToSystem_AndOpenTaskFormByDefault()
		{
			ViewEditMenuTest<TaskManagementForm>(true, "ABC", "&Edit", string.Empty, openTaskFormByDefault: true);
		}

		public void TestTaskWithParentJobHeader_DefaultAction_WithBMEnabled()
		{
			ViewEditMenuTest<ZOrganisationsForm>(true, "ORG", string.Empty, "Management");
		}

		public void TestTaskWithParentJobHeader_DefaultAction_WithBMDisabled()
		{
			ViewEditMenuTest<ZOrganisationsForm>(false, "ORG", string.Empty, "Tasks");
		}

		public void TestTaskWithParentJobHeader_DefaultAction_WithBMEnabled_ButTaskTypeUnknownToSystem()
		{
			ViewEditMenuTest<ZOrganisationsForm>(true, "ABC", string.Empty, "Tasks");
		}

		void ViewEditMenuTest<T>(bool enableBM, string workflowType, string menuName, string expectedTab, bool openTaskFormByDefault = false)
		{
			WorkflowDataRegistry.Instance.TaskDefaultOpeningBehaviour.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, openTaskFormByDefault ?
				TaskDefaultOpeningBehaviourOptions.Codes.Task :
				TaskDefaultOpeningBehaviourOptions.Codes.Job);

			try
			{
				using (processTasks = new ProcessTasksModuleForTest())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = enableBM;
					ObjectFactory.Get<IBMSRegistry>().AlwaysViewWorkflowManagementTab = true;

					ProcessTask task;
					var standaloneTask = (string.IsNullOrEmpty(workflowType));
					if (standaloneTask)
					{
						task = (ProcessTask)processTasks.GridCollection.AddNew();
						task.P9_TaskID = "1";
					}
					else
					{
						var helper = ObjectFactory.Get<IBMTestHelper>();
						helper.CreateSystem(Factory, workflowType);
						var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
						var jobLevelWorkflow = helper.GetJobHeaderForParent(orgHeader, Factory);
						task = orgHeader.WorkflowItems.Tasks.AddNew();
						task.P9_TaskID = "1";

						processTasks.GridCollection.Add(task);
					}

					processTasks.selectedElements = new[] { task };
					task.Factory.Save();

					if (!string.IsNullOrEmpty(menuName))
					{
						var menuItem = processTasks.FormActionMenu.ToList().FindByText(menuName);
						AssertNotNull(menuItem);
						menuItem.PerformClick();
					}
					else
					{
						var decisionProvider = processTasks.ModuleDecisionProvider;
						decisionProvider.HandleDefaultAction(new BusinessObject[] { task });
					}

					Application.DoEvents();
					var forms = Application.OpenForms.OfType<T>();
					Assert(forms.Count() == 1);

					if (typeof(T) == typeof(ZOrganisationsForm))
					{
						AssertEquals(expectedTab, ((forms.Single() as ZOrganisationsForm).OrganisationsTabControl.SelectedTab.
							Controls[0] as ZWorkflowUserControl).MainTabControl.SelectedTab.Text);
					}
				}
			}
			finally
			{
				(Application.OpenForms.OfType<T>().Single() as Form).Close();
			}
		}

		#endregion

		public void TestHandleDefaultAction()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var processTaskWithParent = parent.WorkflowItems.Tasks.AddNew();
			var processTaskWithoutParent = Factory.New<ProcessTask>();

			Factory.Save();

			using (processTasks = new ProcessTasksModuleForTest())
			{
				try
				{
					processTasks.ModuleDecisionProvider.HandleDefaultAction(new[] { processTaskWithParent });
					var form = ZApplication.GetOpenForms().FirstOrDefault(x => x.GetType() == typeof(ZDummyForm));
					AssertNotNull(form);
				}
				finally
				{
					ZApplication.GetOpenForms().First(x => x.GetType() == typeof(ZDummyForm)).Dispose();
				}

				try
				{
					processTasks.ModuleDecisionProvider.HandleDefaultAction(new[] { processTaskWithoutParent });
					var form = ZApplication.GetOpenForms().FirstOrDefault(x => x.GetType() == typeof(TaskManagementForm));
					AssertNotNull(form);
				}
				finally
				{
					ZApplication.GetOpenForms().First(x => x.GetType() == typeof(TaskManagementForm)).Dispose();
				}
			}
		}

		public void TestHandleDefaultActionIfParentControllerIDIsNull()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var processTaskWithParent = Factory.NewWithValidTestData<DummyProcessTaskNullParentControllerID>();
			processTaskWithParent.P9_ParentID = parent.PK;
			processTaskWithParent.P9_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			DummyWorkflowDescriptor.Instance.OverriddenControllerID = DummyControllerIDs.Dummy;

			Factory.Save();

			using (processTasks = new ProcessTasksModuleForTest())
			{
				AssertNoExceptionThrown(() =>
				{
					processTasks.ModuleDecisionProvider.HandleDefaultAction(new[] { processTaskWithParent });
				});
				using (var form = ZApplication.GetOpenForms().FirstOrDefault(x => x.GetType() == typeof(ZDummyForm)))
				{
					AssertNotNull(form);
				}
			}
		}

		public void TestHandleDefaultActionIfProcessTaskIsDeleted()
		{
			var processTask = Factory.New<ProcessTask>();
			Factory.Save();

			processTask.Delete();
			Factory.Save();

			using (processTasks = new ProcessTasksModuleForTest())
			{
				AssertNoExceptionThrown(() => processTasks.ModuleDecisionProvider.HandleDefaultAction([processTask]));
			}
		}

		public void TestDeleteFormActionMenuItemHasIcons()
		{
			processTasks = new ProcessTasksModuleForTest();
			var menus = new List<MenuItem>(processTasks.FormActionMenu);
			var deleteMenuItem = menus.FindByText("&Delete");

			AssertNotNull(deleteMenuItem);

			var deleteZMenuItem = deleteMenuItem as ZMenuItem;

			AssertEquals($"The {deleteZMenuItem.Text} menu item should have the delete rest icon.", deleteZMenuItem.RestIcon, IconTypes.DeleteButtonRest);
			AssertEquals($"The {deleteZMenuItem.Text} menu item should have the delete active icon.", deleteZMenuItem.ActiveIcon, IconTypes.DeleteButtonActive);

			processTasks.Dispose();
		}

		#region Implementation

		// /*  required for "looks like useless tests" above
		ProcessTasksModule Module
		{
			get
			{
				if (module == null)
				{
					module = (ProcessTasksModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessTasks);
				}
				return module;
			}
		}
		ProcessTasksModule module;

		protected override void TearDown()
		{
			base.TearDown();
			if (module != null)
			{
				module.Dispose();
			}
		}

		ProcessTasksModuleForTest processTasks;

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);

			var task = Factory.NewWithValidTestData<ProcessTask>();
			var workflow = Factory.New<IProcessHeader>();
			task.P9_FH_ProcessHeader = workflow.PK;

			collection.Add(task);
		}

		#endregion
	}
}
