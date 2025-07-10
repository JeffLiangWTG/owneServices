using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageWorkflowModuleTextFilter))]
	public class CartageWorkflowModuleTextFilterTest : WorkflowModuleTextFilterTest
	{
		public void TestMilestoneCartageWorkflowModuleTextFilter()
		{
			var year = ZDateTime.Now.Year;

			var bizOWithParentTableCode = CreateWorkflow(new ZDateTime(year, 3, 4), "EV1", "P0");
			var bizoWithSomeOtherParentTableCode = CreateWorkflow(new ZDateTime(year, 3, 4), "EV1", "Z0");
			Factory.Save();

			var filterStrip = new DummyFilterStripBusinessObject();
			var filter = new CartageWorkflowModuleTextFilter("filter", GetMilestoneTextFilter, new CodeDescriptionPairList(), typeof(DummyWithWorkflow), "", null);
			filterStrip.AddModuleFilterForTest(filter);
			var milestoneFilter = (CartageWorkflowModuleTextFilter)filterStrip["filter"];
			milestoneFilter.MilestoneEvent = "EV1";
			milestoneFilter.Property = "rightvalue";
			milestoneFilter.IsActive = true;

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(filterStrip.Filter);
			AssertEquals("Filter should return both dummy business objects", 2, collection.Count);
			Assert("Filter should return bizOWithParentTableCode", collection.Contains(bizOWithParentTableCode));
			Assert("Filter should return bizOWithSomeOtherTableCode", collection.Contains(bizoWithSomeOtherParentTableCode));
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

		protected override WorkflowModuleTextFilter GetNewModuleFilter()
		{
			return new CartageWorkflowModuleTextFilter("moo", GetMilestoneTextFilter, new CodeDescriptionPairList(), typeof(DummyWithWorkflow), "", null);
		}
	}
}
