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
	sealed class LoadListConsolFilterStripTest : BaseFreightTest
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
			var cfsLoadList1 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			var cfsLoadList2 = Factory.NewWithValidTestData<CFSLoadListConsol>();

			var milestone1 = ((IWorkflowProvider)cfsLoadList1).WorkflowItems.Milestones.AddNew();
			var milestone2 = ((IWorkflowProvider)cfsLoadList2).WorkflowItems.Milestones.AddNew();
			milestone1.SetMilestoneScheduledDateForTest(today);
			milestone2.SetMilestoneScheduledDateForTest(today.AddDays(30));

			var trigger1 = ((IWorkflowProvider)cfsLoadList1).WorkflowItems.Triggers.AddNew();
			var exception1 = ((IWorkflowProvider)cfsLoadList2).WorkflowItems.Exceptions.AddNew();

			milestone1.SetMilestoneActualDateForTest(today);
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			exception1.SetMilestoneActualDateForTest(today);
			trigger1.SetMilestoneActualDateForTest(ZDateTime.Empty);
			Factory.Save();

			Asserter.AddToScope(cfsLoadList1);
			Asserter.AddToScope(cfsLoadList2);

			var filterBO = new LoadListConsolFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO.ModuleFilters["Milestone Completed"];

			filter.Property = "Completed";
			filter.IsActive = true;
			Asserter.AssertMatches("Should contains load list with completed workflow", filter, cfsLoadList1);

			filter.Property = "Not Completed";
			filter.IsActive = true;
			Asserter.AssertMatches("Should contains load list with incomplete workflow", filter, cfsLoadList2);
		}

		public void TestMilestoneLastCompletedFilter()
		{
			var today = ZDateTimeOffset.Today;

			var cfsLoadList1 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			var cfsLoadList2 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			CreateMilestone(cfsLoadList1, "LST", today, today.AddDays(1), ZString.Empty);
			CreateMilestone(cfsLoadList2, "LST", today.AddDays(25), today.AddDays(30), ZString.Empty);
			Factory.Save();

			Asserter.AddToScope(cfsLoadList1);
			Asserter.AddToScope(cfsLoadList2);

			var filterBO = new LoadListConsolFilterBusinessObject();
			var filter = (WorkflowModuleFilter)filterBO.ModuleFilters["Last Completed Milestone"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.AddDays(-5).ToZDateTime();
			filter.Property2 = today.AddDays(5).ToZDateTime();
			filter.IsActive = true;
			Asserter.AssertMatches("Should contains load list within date range", filter, cfsLoadList1);
		}

		public void TestMilestoneNextFilter()
		{
			var today = ZDateTimeOffset.Today;

			var cfsLoadList1 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			var cfsLoadList2 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			CreateMilestone(cfsLoadList1, "NXT", today, ZDateTimeOffset.Empty, "AID");
			CreateMilestone(cfsLoadList2, "NXT", today.AddDays(30), ZDateTimeOffset.Empty, "AID");
			Factory.Save();

			Asserter.AddToScope(cfsLoadList1);
			Asserter.AddToScope(cfsLoadList2);

			var filterBO = new LoadListConsolFilterBusinessObject();
			var filter = (WorkflowModuleFilter)filterBO.ModuleFilters["Next Milestone"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.ToZDateTime().AddDays(-5);
			filter.Property2 = today.ToZDateTime().AddDays(5);
			filter.IsActive = true;
			Asserter.AssertMatches("Should contains load list within date range", filter, cfsLoadList1);
		}

		void CreateMilestone(CFSLoadListConsol parent, ZString status, ZDateTimeOffset scheduledDate, ZDateTimeOffset actualDate, ZString triggerEventCode)
		{
			var task = ((IWorkflowProvider)parent).WorkflowItems.Milestones.AddNew();
			task.P9_Type = "MIL";
			task.P9_Status = status;
			task.SetMilestoneScheduledDateForTest(scheduledDate);
			task.SetMilestoneActualDateForTest(actualDate);
			task.TriggerConditions.TriggerEventCode = triggerEventCode;
			Factory.Save();
		}

		FilterStripAsserter<CFSLoadListConsol> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<CFSLoadListConsol>(Factory, x => x.JK_UniqueConsignRef)); }
		}
		FilterStripAsserter<CFSLoadListConsol> asserter;

		#endregion

		#region Test Classes

		class TestJobConsolModuleStrip : LoadListConsolFilterStrip
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
			using (TestJobConsolModuleStrip strip = new TestJobConsolModuleStrip())
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
