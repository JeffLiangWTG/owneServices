using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Module.Testing
{
	[TestedType(typeof(TransportBookingWorkflowModuleTextFilter))]
	public class TransportBookingWorkflowModuleTextFilterTest : WorkflowModuleTextFilterTest
	{
		#region TestDateAndTypeQueryChecksParentTableCodeWithExludeParentTableCode

		public void TestDateAndTypeQueryChecksParentTableCodeWithExludeParentTableCode()
		{
			var year = ZDateTime.Now.Year;

			var bizOWithParentTableCode = CreateWorkflow(new ZDateTime(year, 3, 4), "EV1", "P0");
			var bizoWithSomeOtherParentTableCode = CreateWorkflow(new ZDateTime(year, 3, 4), "EV1", "Z0");
			Factory.Save();

			var filterStrip = new DummyFilterStripBusinessObject();
			var filter = new TransportBookingWorkflowModuleTextFilter("filter", GetMilestoneTextFilter, new CodeDescriptionPairList(), typeof(DummyWithWorkflow), "", null);
			filterStrip.AddModuleFilterForTest(filter);
			var milestoneFilter = (TransportBookingWorkflowModuleTextFilter)filterStrip["filter"];
			milestoneFilter.MilestoneEvent = "EV1";
			milestoneFilter.Property = "rightvalue";
			milestoneFilter.IsActive = true;

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(filterStrip.Filter);
			AssertEquals("There should be only two elements", 2, collection.Count);
			Assert(collection.Contains(bizOWithParentTableCode));
			Assert(collection.Contains(bizoWithSomeOtherParentTableCode));
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

		protected override WorkflowModuleTextFilter GetNewModuleFilter()
		{
			return new TransportBookingWorkflowModuleTextFilter("moo", GetMilestoneTextFilter, new CodeDescriptionPairList(), typeof(DummyWithWorkflow), "", null);
		}
	}
}
