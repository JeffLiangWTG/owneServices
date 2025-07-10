using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Freight.CFS.Module.Testing
{
	sealed class ShipmentReceivalModuleStripTest : BaseFreightTest
	{
		public void TestHandlesModuleWarehouseLocationFilter()
		{
			AssertProvidesControlsFor(new ModuleWarehouseLocationFilter("Blah", GetFilter, null));
		}

		public void TestHandlesReferenceNumbers()
		{
			AssertProvidesControlsFor(new ReferenceNumberFilter("description", delegate
			{ return new ZQuery(); }, new RefCountryCollection(Factory)));
		}

		#region Workflow Test

		public void TestMilestoneCompletedFilter()
		{
			var today = ZDateTimeOffset.Today;
			var shipment1 = GetNewShipmentReceival("S00010001");
			var shipment2 = GetNewShipmentReceival("S00010002");

			var milestone1 = ((IWorkflowProvider)shipment1).WorkflowItems.Milestones.AddNew();
			var milestone2 = ((IWorkflowProvider)shipment2).WorkflowItems.Milestones.AddNew();
			milestone1.SetMilestoneScheduledDateForTest(today);
			milestone2.SetMilestoneScheduledDateForTest(today.AddDays(30));

			var trigger1 = ((IWorkflowProvider)shipment1).WorkflowItems.Triggers.AddNew();
			var exception1 = ((IWorkflowProvider)shipment2).WorkflowItems.Exceptions.AddNew();

			milestone1.SetMilestoneActualDateForTest(today);
			milestone2.SetMilestoneActualDateForTest(ZDateTimeOffset.Empty);
			exception1.SetMilestoneActualDateForTest(today);
			trigger1.SetMilestoneActualDateForTest(ZDateTimeOffset.Empty);

			Asserter.AddToScope(shipment1);
			Asserter.AddToScope(shipment2);
			Factory.Save();

			var filterBO = new ShipmentReceivalFilterStrip();
			var filter = (ModuleTextFilter)filterBO.ModuleFilters["Milestone Completed"];

			filter.Property = "Completed";
			filter.IsActive = true;
			Asserter.AssertMatches("Should contains shipment with completed workflow", filter, shipment1);

			filter.Property = "Not Completed";
			filter.IsActive = true;
			Asserter.AssertMatches("Should contains shipment with incomplete workflow", filter, shipment2);
		}

		public void TestMilestoneLastCompletedFilter()
		{
			var today = ZDateTimeOffset.Today;

			var shipment1 = GetNewShipmentReceival("S00010001");
			var shipment2 = GetNewShipmentReceival("S00010002");
			CreateMilestone(shipment1, "LST", today, today.AddDays(1), ZString.Empty);
			CreateMilestone(shipment2, "LST", today.AddDays(25), today.AddDays(30), ZString.Empty);

			Asserter.AddToScope(shipment1);
			Asserter.AddToScope(shipment2);

			var filterBO = new ShipmentReceivalFilterStrip();
			var filter = (WorkflowModuleFilter)filterBO.ModuleFilters["Last Completed Milestone"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.AddDays(-5).ToZDateTime();
			filter.Property2 = today.AddDays(5).ToZDateTime();
			filter.IsActive = true;
			Asserter.AssertMatches("Should contains shipment within date range", filter, shipment1);
		}

		public void TestMilestoneNextFilter()
		{
			var today = ZDateTimeOffset.Today;

			var shipment1 = GetNewShipmentReceival("S00010001");
			var shipment2 = GetNewShipmentReceival("S00010002");
			CreateMilestone(shipment1, "NXT", today, ZDateTimeOffset.Empty, "AID");
			CreateMilestone(shipment2, "NXT", today.AddDays(30), ZDateTimeOffset.Empty, "AID");

			Asserter.AddToScope(shipment1);
			Asserter.AddToScope(shipment1);

			var filterBO = new ShipmentReceivalFilterStrip();
			var filter = (WorkflowModuleFilter)filterBO.ModuleFilters["Next Milestone"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.AddDays(-5).ToZDateTime();
			filter.Property2 = today.AddDays(5).ToZDateTime();
			filter.IsActive = true;
			Asserter.AssertMatches("Should contains shipment within date range", filter, shipment1);
		}

		CFSShipment GetNewShipmentReceival(string shipmentNumber)
		{
			CFSShipment shipment = Factory.NewWithValidTestData<CFSShipment>();
			shipment.JS_UniqueConsignRef = shipmentNumber;
			Factory.Save();
			return shipment;
		}

		void CreateMilestone(CFSShipment parent, ZString status, ZDateTimeOffset scheduledDate, ZDateTimeOffset actualDate, ZString triggerEventCode)
		{
			var task = ((IWorkflowProvider)parent).WorkflowItems.Milestones.AddNew();
			task.P9_Type = "MIL";
			task.P9_Status = status;
			task.SetMilestoneScheduledDateForTest(scheduledDate);
			task.SetMilestoneActualDateForTest(actualDate);
			task.TriggerConditions.TriggerEventCode = triggerEventCode;
			Factory.Save();
		}

		FilterStripAsserter<CFSShipment> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<CFSShipment>(Factory, x => x.JS_UniqueConsignRef)); }
		}
		FilterStripAsserter<CFSShipment> asserter;

		#endregion

		#region Implementation

		ZQuery GetFilter(ZQuery filter)
		{
			return new ZQuery();
		}

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (ShipmentReceivalModuleStrip strip = new ShipmentReceivalModuleStrip())
			{
				Control[] controls = GetCurrentFilterControls(strip, filter);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);

				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}

		Control[] GetCurrentFilterControls(ShipmentReceivalModuleStrip strip, ModuleFilter filter)
		{
			MethodInfo info = typeof(ShipmentReceivalModuleStrip).GetMethod(
				"GetCurrentFilterControls", BindingFlags.Instance | BindingFlags.NonPublic);

			return (Control[])info.Invoke(strip, new object[] { filter });
		}

		#endregion
	}
}
