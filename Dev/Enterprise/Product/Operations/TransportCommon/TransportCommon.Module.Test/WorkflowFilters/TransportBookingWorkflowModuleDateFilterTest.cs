using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Module.Testing
{
	[TestedType(typeof(TransportBookingWorkflowModuleDateFilter))]
	class TransportBookingWorkflowModuleDateFilterTest : WorkflowModuleFilterTest
	{
		#region TestNextMilestoneQueryChecksParentTableCodeWithExcludeParentTableCode

		public void TestNextMilestoneQueryChecksParentTableCodeWithExcludeParentTableCode()
		{
			var year = ZDateTime.Now.Year;

			var bizOWithParentTableCode = CreateWorkflow(new ZDateTime(year, 06, 06), "EV1", "P0");
			var bizOWitSomeOtherParentTableCode = CreateWorkflow(new ZDateTime(year, 06, 06), "EV1", "Z0");
			Factory.Save();

			var filterStrip = new DummyFilterStripBusinessObject();
			var filter = new TransportBookingWorkflowModuleDateFilter("filter", typeof(DummyWithWorkflow), WorkflowModuleFilterTypes.MilestoneDate, "", null);
			filterStrip.AddModuleFilterForTest(filter);
			var milestoneFilter = (WorkflowModuleFilter)filterStrip["filter"];
			milestoneFilter.MilestoneEvent = "EV1";
			milestoneFilter.DatesToFilter = ZString.Empty;
			milestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneFilter.Property1 = new ZDateTime(year, 06, 01);
			milestoneFilter.Property2 = new ZDateTime(year, 07, 01);
			milestoneFilter.IsActive = true;

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(filterStrip.Filter);
			AssertEquals("There should be only two elements", 2, collection.Count);
			Assert(collection.Contains(bizOWithParentTableCode));
			Assert(collection.Contains(bizOWitSomeOtherParentTableCode));
		}

		DummyWithWorkflow CreateWorkflow(ZDateTime actualDate, string eventType, string parentTableCode)
		{
			var bizO = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = bizO.WorkflowItems.Milestones.AddNew();
			milestone.SetMilestoneActualDateForTest(actualDate);
			milestone.TriggerConditions.TriggerEventCode = eventType;
			milestone.P9_ParentTableCode = parentTableCode;
			return bizO;
		}

		#endregion

		protected override WorkflowModuleFilter GetNewModuleFilter()
		{
			return new TransportBookingWorkflowModuleDateFilter("moo", typeof(DummyWithWorkflow), WorkflowModuleFilterTypes.MilestoneDate, "", null);
		}
	}
}
