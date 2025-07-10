using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs.US;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class WorkflowParentFormFactoryTest : TestCaseWithFactory
	{
		public void TestStatementControllerPopUp()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var statement = Factory.New(ObjectFactory.GetType<ICusStatementHeader>());
				var workflowProvider = (IWorkflowProvider)statement;
				var task = workflowProvider.WorkflowItems.AddNew();
				Factory.Save();

				AssertNoExceptionThrown(() =>
				{
					using (WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
					{ }
				});
			}
		}

		public void TestShowForm_NullController()
		{
			var job = Factory.New<DummyWithWorkflow>();
			Factory.Save();

			using (var form = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(job))
			{
				AssertNull(form);
			}

			Assert("When the Controller is null, error should be reported",
				ErrorReporter.LastMessageReported.Contains("Unable to show the form due to null controller. Task is null. WorkflowProvider: ") &&
				ErrorReporter.LastMessageReported.Contains("WorkflowType: DUM."));

			ErrorReporter.Clear();
		}

		[RequiresSTA]
		public void TestShowForm_TaskInsideWorkflowTemplate()
		{
			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = "ACA";
			workflowTemplate.P0_Name = "Test Template";

			var task = workflowTemplate.WorkflowItems.AddNew();
			task.P9_Description = "Test Task";

			Factory.Save();

			using (var form = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				AssertEquals("If task belongs to workflow template ProcessTaskTemplateForm should open", form is ProcessTaskTemplateForm, true);
			}
		}

		public void TestShowForm_DeletedParentForProcessHeader()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.CreateSystem(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			Factory.Save();

			var testFactory = new BusinessObjectFactory();
			var shipment = testFactory.New<IForwardingShipment>() as IWorkflowProvider;
			var task = shipment.WorkflowItems.AddNew();
			task.P9_Description = "Test Task";

			var workflow = ProcessJobHeaderProvider.GetForParent(shipment, testFactory).ProcessHeaders.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			testFactory.Save();

			var sql = $"DELETE FROM dbo.JobShipment WHERE JS_PK = '{shipment.PK}'";
			Db.Connection.ExecuteNonQuery(sql);

			var updatedTask = Factory.Load<ProcessTask>(task.PK);

			WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(updatedTask);
			AssertEquals("Process task parent no longer exist.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
		}

		public void TestShowForm_NullParentForProcessHeader()
		{
			var mockProcessHeader = new Mock<IProcessHeader>();
			mockProcessHeader.Setup(m => m.Parent).Returns((IWorkflowProviderCore)null);

			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_CardNote = "something";
			task.P9_Description = "Test 1";
			var tasks = new List<ProcessTask> { task };
			mockProcessHeader.Setup(m => m.Tasks).Returns(tasks.ToArray());

			WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(mockProcessHeader.Object);
			AssertEquals("Process task parent no longer exist.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
		}

		public void TestShowForm_NullBizo()
		{
			AssertExceptionThrown<ArgumentNullException>(() => WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(null));
		}

		public void TestShowForm_UnSupportedBizoType()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			AssertEquals("The whole point of this test is to see what happens when the business object doesn't support workflow. This one does. SAD!", false, dummy is IWorkflowProvider);

			Factory.Save();

			AssertExceptionThrown<ArgumentException>(() => WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(dummy));
		}

		public void TestGetFormForWorkflowProvider()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			using (var form = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(job))
			{
				AssertNotNull(form);
			}
		}

		public void TestShowForm_ForJobWorkflow_ShouldSelectJobWorkflowOnForm()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "INQ");

			var jobHeader = helper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow");
			var task = helper.CreateTask(workflow, Env.CurrentUser.Initials);

			Factory.Save();

			using (var form = (Control)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(jobHeader))
			{
				Application.DoEvents();
				var grid = form.FindSingle<ZGrid>(x => x.Name == "WorkflowsGrid");
				var selected = (BusinessObject)grid.ListManager.Current;

				AssertEquals("The job header should have been selected instead of the workflow, and yet...", jobHeader.PK, selected.PK);
			}
		}

		public void TestShowForm_ForWorkflowWithNoTasks_ShouldSelectWorkflowOnForm()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "INQ");

			var jobHeader = helper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow");
			var workflow2 = helper.CreateWorkflow(jobHeader, "Workflow");
			var workflow3 = helper.CreateWorkflow(jobHeader, "Workflow");

			Factory.Save();

			using (var form = (Control)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow2))
			{
				Application.DoEvents();
				var grid = form.FindSingle<ZGrid>(x => x.Name == "WorkflowsGrid");
				var selected = (BusinessObject)grid.ListManager.Current;

				AssertEquals("The workflow2 should have been selected, and yet...", workflow2.PK, selected.PK);
			}
		}

		public void TestNavigateToTask_WithMultipleWorkflowTabs_WithoutImplementingOverridableInterface_ShouldReportError()
		{
			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			var task = job.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			using (var form = new FormWithMultipleWorkflowTabs(job))
			{
				form.Show();
				WorkflowParentFormFactory.NavigateToWorkflowItem(form, task);
				Application.DoEvents();
			}

			AssertEquals("The form has multiple workflow tabs, and custom navigation interface is not implemented on it, so an error should have been reported, and yet..", true, ErrorReporter.HasBeenReported("WorkflowParentFormFactory.GetWorkflowTab.MultipleWorkflowTabs"));
			ErrorReporter.Clear();
		}

		public void TestNavigateToWorkflow_WithMultipleWorkflowTabs_WithoutImplementingOverridableInterface_ShouldReportError()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "INQ");

			var jobHeader = helper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow");
			var task = helper.CreateTask(workflow, Env.CurrentUser.Initials);

			Factory.Save();

			using (var form = new FormWithMultipleWorkflowTabs((BusinessObject)jobHeader.Parent))
			{
				form.Show();
				WorkflowParentFormFactory.NavigateToWorkflowItem(form, workflow);
				Application.DoEvents();
			}

			AssertEquals("The form has multiple workflow tabs, and custom navigation interface is not implemented on it, so an error should have been reported, and yet..", true, ErrorReporter.HasBeenReported("WorkflowParentFormFactory.GetWorkflowTab.MultipleWorkflowTabs"));
			ErrorReporter.Clear();
		}

		public void TestNavigateToWorkflow_WithNullForm_ShouldDoNothingAndNotThrowExceptions()
		{
			var task = Factory.New<ProcessTask>();
			var workflow = Factory.New<IProcessHeader>();
			ZForm nullForm = null;

			AssertNoExceptionThrown(() => WorkflowParentFormFactory.NavigateToWorkflowItem(nullForm, task));
			AssertNoExceptionThrown(() => WorkflowParentFormFactory.NavigateToWorkflowItem(nullForm, workflow));
		}

		public void TestNavigateToWorkflow_WhenFormIsDisposed_ShouldNotReportErrorAndNotThrowExceptions()
		{
			var task = Factory.New<ProcessTask>();
			var workflow = Factory.New<IProcessHeader>();
			ZForm form = new ZForm();
			form.Dispose();

			AssertNoExceptionThrown(() => WorkflowParentFormFactory.NavigateToWorkflowItem(form, task));
			AssertNoExceptionThrown(() => WorkflowParentFormFactory.NavigateToWorkflowItem(form, workflow));
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		public void TestNavigateToWorkflow_WhenFormHasNoHandle_ShouldNotReportErrorAndNotThrowExceptions()
		{
			var task = Factory.New<ProcessTask>();
			var workflow = Factory.New<IProcessHeader>();
			ZForm form = new ZForm();

			AssertNoExceptionThrown(() => WorkflowParentFormFactory.NavigateToWorkflowItem(form, task));
			AssertNoExceptionThrown(() => WorkflowParentFormFactory.NavigateToWorkflowItem(form, workflow));
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			form.Dispose();
		}

		public void TestOpenForm_NavigateToTask_ForAllWorkflowProviders()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			AssertOpenFormForAllWorkflowProviders((job, workflowType) =>
			{
				if (WorkflowDescriptors.Instance.TryGetValue(workflowType, out WorkflowDescriptor descriptor) && descriptor.SupportsBufferManagement)
				{
					helper.CreateSystem(Factory, workflowType);
					var jobHeader = helper.GetJobHeaderForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
					var workflow = helper.CreateWorkflow(jobHeader, "Workflow");
					var task1 = helper.CreateTask(workflow, Env.CurrentUser.Initials, sequence: 1);
					var task2 = helper.CreateTask(workflow, Env.CurrentUser.Initials, sequence: 2);
					var task3 = helper.CreateTask(workflow, Env.CurrentUser.Initials, sequence: 3);

					Factory.Save();

					using (var form = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem((ProcessTask)task2))
					{
						AssertWorkflowItemSelected("The correct task should have been selected, and yet...", form, task2.PK, "TasksGrid");
					}
				}
			});
		}

		public void TestShowForm_ForStandaloneTask_ShouldOpenTaskForm()
		{
			var task = Factory.New<ProcessTask>();

			Factory.Save();

			using (var form = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				AssertType<TaskManagementForm>(form);
			}
		}

		public void TestOpenForm_NavigateToWorkflow_ForAllWorkflowProviders()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			AssertOpenFormForAllWorkflowProviders((job, workflowType) =>
			{
				if (WorkflowDescriptors.Instance.TryGetValue(workflowType, out WorkflowDescriptor descriptor) && descriptor.SupportsBufferManagement)
				{
					helper.CreateSystem(Factory, workflowType);
					var jobHeader = helper.GetJobHeaderForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
					var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow 1");
					var workflow2 = helper.CreateWorkflow(jobHeader, "Workflow 2");
					var workflow3 = helper.CreateWorkflow(jobHeader, "Workflow 3");
					var task1 = helper.CreateTask(workflow1, Env.CurrentUser.Initials, sequence: 1);
					var task2 = helper.CreateTask(workflow2, Env.CurrentUser.Initials, sequence: 2);
					var task3 = helper.CreateTask(workflow3, Env.CurrentUser.Initials, sequence: 3);

					Factory.Save();

					using (var form = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow2))
					{
						AssertWorkflowItemSelected("The workflow should have been selected, and yet...", form, workflow2.PK, "WorkflowsGrid");
					}
				}
			});
		}

		public void TestOpenForm_NavigateToJobHeader_ForAllWorkflowProviders()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			AssertOpenFormForAllWorkflowProviders((job, workflowType) =>
			{
				if (WorkflowDescriptors.Instance.TryGetValue(workflowType, out WorkflowDescriptor descriptor) && descriptor.SupportsBufferManagement)
				{
					helper.CreateSystem(Factory, workflowType);
					var jobHeader = helper.GetJobHeaderForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
					var workflow = helper.CreateWorkflow(jobHeader, "Workflow");
					var task = helper.CreateTask(workflow, Env.CurrentUser.Initials);

					Factory.Save();

					using (var form = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(jobHeader))
					{
						AssertWorkflowItemSelected("The job header should have been selected instead of the workflow, and yet...", form, jobHeader.PK, "WorkflowsGrid");
					}
				}
			});
		}

		static void AssertWorkflowItemSelected(string message, IZForm form, ZGuid expected, string gridName)
		{
			Application.DoEvents();
			var grid = ((Form)form).FindSingleOrDefault<ZGrid>(x => x.Name == gridName);

			if (grid != null && grid.CurrentRowIndex >= 0)
			{
				var selected = (BusinessObject)grid.ListManager.Current;
				AssertEquals(message, expected, selected.PK);
			}
		}

		void AssertOpenFormForAllWorkflowProviders(Action<IWorkflowProvider, string> testAction)
		{
			var types = (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorList>();

			CombineAssertions(() =>
			{
				foreach (var type in types.GetAllCodes())
				{
					try
					{
						var controller = WorkflowProviderHelper.GetControllerForWorkflowType(type);

						if (controller != null && !ControllerDoesNotSupportFormAttribute.HasAttribute(controller))
						{
							var job = ((ZControllerInternals)controller).GetNewBusinessEntityInFactory(Factory) as IWorkflowProvider;

							if (job != null)
							{
								((BusinessObject)job).FillWithValidTestData();
								testAction(job, type);
							}
						}
					}
					catch (ModuleGuiNotSupportedException)
					{
					}
				}
			});
		}

		public void TestShowForm_ForProcessTask_WithoutBufferManagement_ShouldShowFormAndNavigateToTask()
		{
			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			var task = job.WorkflowItems.Tasks.AddNew();
			task.FillWithValidTestData();
			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();
				AssertNotNull(form);
				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");

				AssertEquals("Workflow && Tracking", tabControl.SelectedTab.Text);
				AssertWorkflowItemSelected("The tasks tab should have been selected. SAD!", form, task.PK, "TasksGrid");
			}
		}

		public void TestShowForm_WhenJobIsDeleted_ShouldNotReportError()
		{
			DummyWorkflowDescriptor.Instance.OverriddenControllerID = DummyControllerIDs.Dummy;
			Factory.RefreshEnabled = false;
			var job = Factory.NewWithValidTestData<DummyForDeletedJobTest>();
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var jobInOtherFactory = otherFactory.Load<DummyForDeletedJobTest>(job.PK);

			job.WorkflowTypeAccessed += (s, e) =>
			{
				jobInOtherFactory.Delete();
				otherFactory.Save();
			};

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(job))
			{
				AssertNull(form);
			}

			AssertEquals("No error reports should be logged because it's normal to not show a form for a deleted object. SAD!", string.Empty, ErrorReporter.LastMessageReported);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Get<IBMSRegistry>().AlwaysViewWorkflowManagementTab = true;
		}
	}
}
