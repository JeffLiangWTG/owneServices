using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Freight.CFS.Module.Testing
{
	sealed class ManifestTallyFilterStripTest : BaseFreightTest
	{
		public void TestHandlesReferenceNumbers()
		{
			AssertProvidesControlsFor(new ReferenceNumberFilter("description", delegate
			{ return new ZQuery(); }, new RefCountryCollection(Factory)));
		}

		#region Workflow Test

		public void TestMilestoneCompletedFilter()
		{
			var today = ZDateTimeOffset.Today;
			var container1 = GetNewTallyContainer("CONT0000001");
			var container2 = GetNewTallyContainer("CONT0000002");

			var milestone1 = ((IWorkflowProvider)container1).WorkflowItems.Milestones.AddNew();
			var milestone2 = ((IWorkflowProvider)container2).WorkflowItems.Milestones.AddNew();
			milestone1.SetMilestoneScheduledDateForTest(today);
			milestone2.SetMilestoneScheduledDateForTest(today.AddDays(30));

			var trigger1 = ((IWorkflowProvider)container1).WorkflowItems.Triggers.AddNew();
			var exception1 = ((IWorkflowProvider)container2).WorkflowItems.Exceptions.AddNew();

			milestone1.SetMilestoneActualDateForTest(today);
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			exception1.SetMilestoneActualDateForTest(today);
			trigger1.SetMilestoneActualDateForTest(ZDateTime.Empty);

			Asserter.AddToScope(container1);
			Asserter.AddToScope(container2);
			Factory.Save();

			var filterBO = new ManifestTallyFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO.ModuleFilters["Milestone Completed"];

			filter.Property = "Completed";
			filter.IsActive = true;
			Asserter.AssertMatches("Should contains container with completed workflow", filter, container1);

			filter.Property = "Not Completed";
			filter.IsActive = true;
			Asserter.AssertMatches("Should contains container with incomplete workflow", filter, container2);
		}

		public void TestMilestoneLastCompletedFilter()
		{
			var today = ZDateTimeOffset.Today;

			var container1 = GetNewTallyContainer("CONT0000001");
			var container2 = GetNewTallyContainer("CONT0000002");
			CreateMilestone(container1, "LST", today, today.AddDays(1), ZString.Empty);
			CreateMilestone(container2, "LST", today.AddDays(25), today.AddDays(30), ZString.Empty);

			Asserter.AddToScope(container1);
			Asserter.AddToScope(container2);

			var filterBO = new ManifestTallyFilterBusinessObject();
			var filter = (WorkflowModuleFilter)filterBO.ModuleFilters["Last Completed Milestone"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.AddDays(-5).ToZDateTime();
			filter.Property2 = today.AddDays(5).ToZDateTime();
			filter.IsActive = true;
			Asserter.AssertMatches("Should contains container within date range", filter, container1);
		}

		public void TestMilestoneNextFilter()
		{
			var today = ZDateTimeOffset.Today;

			var container1 = GetNewTallyContainer("CONT0000001");
			var container2 = GetNewTallyContainer("CONT0000002");
			CreateMilestone(container1, "NXT", today, ZDateTimeOffset.Empty, "AID");
			CreateMilestone(container2, "NXT", today.AddDays(30), ZDateTimeOffset.Empty, "AID");

			Asserter.AddToScope(container1);
			Asserter.AddToScope(container1);

			var filterBO = new ManifestTallyFilterBusinessObject();
			var filter = (WorkflowModuleFilter)filterBO.ModuleFilters["Next Milestone"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.AddDays(-5).ToZDateTime();
			filter.Property2 = today.AddDays(5).ToZDateTime();
			filter.IsActive = true;
			Asserter.AssertMatches("Should contains container within date range", filter, container1);
		}

		TallyContainer GetNewTallyContainer(string containerNumber)
		{
			CFSLoadListConsol consol = Factory.NewWithValidTestData<CFSLoadListConsol>();
			TallyContainer container = Factory.NewWithValidTestData<TallyContainer>();
			container.JC_JK = consol.PK;
			container.JC_ContainerNum = containerNumber;
			Factory.Save();
			return container;
		}

		void CreateMilestone(TallyContainer parent, ZString status, ZDateTimeOffset scheduledDate, ZDateTimeOffset actualDate, ZString triggerEventCode)
		{
			var task = ((IWorkflowProvider)parent).WorkflowItems.Milestones.AddNew();
			task.P9_Type = "MIL";
			task.P9_Status = status;
			task.SetMilestoneScheduledDateForTest(scheduledDate);
			task.SetMilestoneActualDateForTest(actualDate);
			task.TriggerConditions.TriggerEventCode = triggerEventCode;
			Factory.Save();
		}

		FilterStripAsserter<TallyContainer> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<TallyContainer>(Factory, x => x.JC_ContainerNum)); }
		}
		FilterStripAsserter<TallyContainer> asserter;

		#endregion

		#region Test Classes

		class TestTallyModuleStrip : ManifestTallyFilterStrip
		{
			public new ZBindingSource FilterControlBindingSource
			{
				get { return base.FilterControlBindingSource; }
			}

			public new Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}

		#endregion

		#region Implementation

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (TestTallyModuleStrip strip = new TestTallyModuleStrip())
			{
				Control[] controls = strip.GetCurrentFilterControls(filter);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);

				foreach (Control control in controls)
				{
					if (!IsExemptFromBindingRequirement(control))
					{
						bool isBound = !string.IsNullOrEmpty(strip.FilterControlBindingSource.GetBindingMember(control));
						AssertEquals("BindingMember set on FilterControlBindingSource", true, isBound);
					}

					control.Dispose();
				}
			}
		}

		bool IsExemptFromBindingRequirement(Control control)
		{
			return control is ZLabel;
		}

		#endregion
	}
}
