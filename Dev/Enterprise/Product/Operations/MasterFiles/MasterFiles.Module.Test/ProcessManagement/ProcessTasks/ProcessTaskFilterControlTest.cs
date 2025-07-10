using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ProcessTaskFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestColourDeciding()
		{
			ProcessTaskCollection coll = new ProcessTaskCollection(Factory);

			ProcessTask onTimeTask = coll.AddNew();

			ProcessTask lateTask = coll.AddNew();
			lateTask.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Today.AddDays(-1));

			ProcessTask waitingTask = coll.AddNew();
			waitingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			ProcessTaskFilterBusinessObject filterBizo = new ProcessTaskFilterBusinessObject();

			using (ProcessTaskFilterControl control = new ProcessTaskFilterControl(coll, filterBizo))
			{
				ColourDecidingEventArgs args = new ColourDecidingEventArgs(onTimeTask);
				control.FilteredGrid_ColourDeciding(this, args);
				AssertEquals(Color.Empty, args.Colour);

				args = new ColourDecidingEventArgs(lateTask);
				control.FilteredGrid_ColourDeciding(this, args);
				AssertEquals(Color.LightSalmon, args.Colour);

				args = new ColourDecidingEventArgs(waitingTask);
				control.FilteredGrid_ColourDeciding(this, args);
				AssertEquals(Color.Yellow, args.Colour);
			}
		}

		[RequiresSTA]
		public void TestOnlyLoadContactsThatAreBeingDisplayed()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "contact1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "contact2";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "contact3";

			var tasks = new ProcessTaskCollection(Factory);
			var task = tasks.AddNew();
			task.OrganisationPK = org.PK;
			task.P9_OA = org.MainAddress.PK;
			task.P9_OC = contact1.PK;

			Factory.Save();

			using (var module = new ProcessTasksModule())
			using (var form = new ZForm(new ProcessTaskCollection(Factory)))
			{
				var filterControl = ((ProcessTaskFilterControl)module.EmbeddedControl);
				form.Controls.Add(filterControl);
				form.Size = new Size(800, 600);
				form.Show();

				filterControl.FilteredGrid.Columns["ContactName"].IsVisible = true;
				filterControl.FilteredGrid.ReOrderColumns(["ContactName"]);
				filterControl.FirePerformSearch();

				Application.DoEvents(); // force FilteredGrid to draw

				AssertEquals("Only one task", 1, filterControl.GridCollection.Count);

				//if Windows decides to skip repainting the grid, then contact1 is never loaded. Explicitly do so
				AssertEquals("contact1", ((ProcessTask)filterControl.GridCollection.ToArray()[0]).ContactName);

				AssertContainsExactElementsInAnyOrder("Only the contact assigned to the task should be loaded", new LambdaComparer<OrgContact>((x, y) => x.PK == y.PK, x => x.PK.GetHashCode()), contact => contact.OC_ContactName,
					new[] { contact1 },
					((IBusinessObjectFactoryInternals)filterControl.GridCollection.Factory).AllBusinessObjects.OfType<OrgContact>());
			}
		}

		[RequiresSTA]
		public void TestHideBMSColumns()
		{
			var collection = new ProcessTaskCollection(Factory);

			var filterBizo = new ProcessTaskFilterBusinessObject();

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;
			Factory.Save();

			using (var control = new ProcessTaskFilterControl(collection, filterBizo))
			{
				AssertEquals("Grid shouldn't have any of the restricted columns", false,
					ProcessTaskFilterControl.BMSRestrictedColumnNames.All(name => control.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Any(c => c.ColumnName == name)));
			}

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			Factory.Save();

			using (var control = new ProcessTaskFilterControl(collection, filterBizo))
			{
				AssertEquals("Grid should have all BMS columns", true,
					ProcessTaskFilterControl.BMSRestrictedColumnNames.All(name => control.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Any(c => c.ColumnName == name)));
			}
		}

		[RequiresSTA]
		public void TestWorkflowFilter_WhenShowEditForm_ShouldOpenParentJobForm()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode);

			var enquiry = Factory.New<SalesEnquiry>();
			var jobHeader = helper.GetJobHeaderForParent(enquiry, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "The workflow");

			Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessTasks))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Workflow");
				var filter = (WorkflowJobModuleFilter)strip.CurrentModuleFilter;
				filter.Property = workflow.PK;

				using (var moduleForm = (ZForm)module.ShowPopup())
				{
					var filterControl = moduleForm.FindSingle<ZFilterStripCommonControl>();
					filterControl.AddFilterStrip(strip);
					var findBox = filterControl.FindSingle<ProcessTaskFilterStrip.FindBoxForWorkflowJobFilterStrip>();
					ZForm jobForm = null;

					try
					{
						findBox.ShowEditForm_ForTest();

						var openForms = ZApplication.GetOpenForms();
						jobForm = (ZForm)openForms.SingleOrDefault(x => x.Text.StartsWith("Edit Inquiry"));
						AssertNotNull("The parent job form should have opened, and yet...", jobForm);
						AssertEquals("The form should be bound to the correct business object, and yet...", enquiry.PK, ((BusinessObject)jobForm.BusinessEntity).PK);
					}
					finally
					{
						jobForm?.Dispose();
					}
				}
			}
		}

		[RequiresSTA]
		public void TestColumnStylesShouldBeUpperCase()
		{
			var collection = new ProcessTaskCollection(Factory);
			var filterBizo = new ProcessTaskFilterBusinessObject();
			Factory.Save();

			using (var control = new ProcessTaskFilterControl(collection, filterBizo))
			{
				AssertEquals(CharacterCasing.Upper, control.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Single(c => c.ColumnName == "P9_Type").CharacterCasing);
				AssertEquals(CharacterCasing.Upper, control.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Single(c => c.ColumnName == "P9_Status").CharacterCasing);
			}
		}

		[RequiresSTA]
		public void TestIfActualStartTimeColumnsHaveBeenAdded()
		{
			var taskCollection = new ProcessTaskCollection(Factory);
			var filterBusinessObject = new ProcessTaskFilterBusinessObject();
			Factory.Save();

			using (var control = new ProcessTaskFilterControl(taskCollection, filterBusinessObject))
			{
				var actualStartTimeColumn = control.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().SingleOrDefault(c => c.ColumnName == "P9_ActualDateForBinding");
				var actualStartTimeLocalColumn = control.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().SingleOrDefault(c => c.ColumnName == "P9_ActualDateLocalForBinding");
				var actualStartTimeUtcColumn = control.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().SingleOrDefault(c => c.ColumnName == "P9_ActualDateUtc");

				AssertNotNull("The 'P9_ActualDateForBinding' column should exist", actualStartTimeColumn);
				AssertNotNull("The 'P9_ActualDateUtc' column should exist", actualStartTimeLocalColumn);
				AssertNotNull("The 'P9_ActualDateLocalForBinding' column should exist", actualStartTimeUtcColumn);
			}
		}
		[RequiresSTA]
		public void TestIfCompletedTimeColumnsHaveBeenAdded()
		{
			var taskCollection = new ProcessTaskCollection(Factory);
			var filterBusinessObject = new ProcessTaskFilterBusinessObject();
			Factory.Save();

			using (var control = new ProcessTaskFilterControl(taskCollection, filterBusinessObject))
			{
				var completedTimeLocalColumn = control.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().SingleOrDefault(c => c.ColumnName == "CompletedTimeLocal");
				var completedTimeUtcColumn = control.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().SingleOrDefault(c => c.ColumnName == "P9_CompletedTimeUtc");

				AssertNotNull("The 'CompletedTimeLocal' column should exist", completedTimeLocalColumn);
				AssertNotNull("The 'P9_CompletedTimeUtc' column should exist", completedTimeUtcColumn);
			}
		}
		[RequiresSTA]
		public void TestIfScheduledStartTimeColumnsHaveBeenAdded()
		{
			var taskCollection = new ProcessTaskCollection(Factory);
			var filterBusinessObject = new ProcessTaskFilterBusinessObject();
			Factory.Save();

			using (var control = new ProcessTaskFilterControl(taskCollection, filterBusinessObject))
			{
				var scheduledStartTimeColumn = control.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().SingleOrDefault(c => c.ColumnName == "P9_ScheduledDateForBinding");
				var scheduledStartTimeLocalColumn = control.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().SingleOrDefault(c => c.ColumnName == "P9_ScheduledDateLocalForBinding");
				var scheduledStartTimeUtcColumn = control.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().SingleOrDefault(c => c.ColumnName == "P9_ScheduledDateUtc");

				AssertNotNull("The 'P9_ScheduledDateForBinding' column should exist", scheduledStartTimeColumn);
				AssertNotNull("The 'P9_ScheduledDateLocalForBinding' column should exist", scheduledStartTimeLocalColumn);
				AssertNotNull("The 'P9_ScheduledDateUtc' column should exist", scheduledStartTimeUtcColumn);
			}
		}

		[RequiresSTA]
		public void TestOpenTaskShouldAlertWhenTaskIsNotStandaloneAndParentControllerIdIsNull()
		{
			var msg = Globals.Message as UnitTestUserNotification;
			var taskCollection = new ProcessTaskCollection(Factory);
			var processTask = taskCollection.AddNew();
			processTask.P9_ParentID = ZGuid.NewZGuid();
			using (var processTasksModule = new ProcessTasksModule())
			{
				processTasksModule.ModuleDecisionProvider.HandleDefaultAction([processTask]);
			Assert(msg.PreviousMessages.Any(s => s.Text == "This task is not linked to a specific workflow or process. Do you still want to open it in a task form?"));
		}
	}
}
}
