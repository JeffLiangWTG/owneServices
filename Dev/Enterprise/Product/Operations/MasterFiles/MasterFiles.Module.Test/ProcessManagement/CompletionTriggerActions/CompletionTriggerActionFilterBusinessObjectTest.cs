using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CompletionTriggerActionFilterBusinessObject))]
	sealed class CompletionTriggerActionFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestActionFilter()
		{
			TestConnection.ExecuteNonQuery("DELETE dbo.ProcessTaskNotification");
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.FillWithValidTestData();

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.FillWithValidTestData();
			action1.PQ_TriggerType = "NTF";

			var action2 = trigger.ProcessTaskNotifications.AddNew();
			action2.FillWithValidTestData();
			action2.PQ_TriggerType = "FLD";

			var filterBizo = GetNewFilterStripBusinessObject();
			var actionFilter = filterBizo.AddTextFilterStrip("Action", "NTF");

			var results = Factory.Load<ProcessTaskNotification>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "NTF" }, results.Select(x => x.PQ_TriggerType));

			actionFilter.Property = "FLD";
			results = Factory.Load<ProcessTaskNotification>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "FLD" }, results.Select(x => x.PQ_TriggerType));

			actionFilter.Property = "XUE";
			results = Factory.Load<ProcessTaskNotification>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), results.Select(x => x.PQ_TriggerType));
		}

		public void TestActionFilter_List()
		{
			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo.AddTextFilterStrip("Action");
			var codes = ((CodeDescriptionPairList)filter.List).GetAllCodes();
			AssertCollectionContains("The lookups for this filter should be the actions from WorkflowTriggerActionTypeConstants (if people add actions without adding them to WorkflowTriggerActionTypeConstants they won't appear here).", "NTF", codes);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CompletionTriggerActionFilterBusinessObject();
		}
	}
}
