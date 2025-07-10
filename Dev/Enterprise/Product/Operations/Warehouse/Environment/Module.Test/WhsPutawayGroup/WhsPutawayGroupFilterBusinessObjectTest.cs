using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(WhsPutawayGroupFilterBusinessObject))]
	class WhsPutawayGroupFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCode()
		{
			var putawayGroup1 = Helper.CreatePutawayGroup("PG1", "Test Group 1");
			var putawayGroup2 = Helper.CreatePutawayGroup("PG2", "Test Group 2");
			Factory.Save();
			Asserter.AddToScope(putawayGroup1, putawayGroup2);

			var filterBizO = GetNewFilterStripBusinessObject();
			var moduleFilter = (ModuleTextFilter)filterBizO[WhsPutawayGroupFilterBusinessObject.Schema.Code];
			moduleFilter.IsActive = true;
			moduleFilter.Property = "PG1";
			Asserter.AssertMatches("Shoud  return only putawayGroup1", filterBizO.Filter, putawayGroup1);

			moduleFilter.Property = "PG2";
			Asserter.AssertMatches("Shoud return only putawayGroup2", filterBizO.Filter, putawayGroup2);
		}

		public void TestDescription()
		{
			var putawayGroup1 = Helper.CreatePutawayGroup("PG1", "Test Group 1");
			var putawayGroup2 = Helper.CreatePutawayGroup("PG2", "Test Group 2");
			Factory.Save();
			Asserter.AddToScope(putawayGroup1, putawayGroup2);

			var filterBizO = GetNewFilterStripBusinessObject();
			var moduleFilter = (ModuleTextFilter)filterBizO[WhsPutawayGroupFilterBusinessObject.Schema.Description];
			moduleFilter.IsActive = true;
			moduleFilter.Property = "Test Group 1";
			Asserter.AssertMatches("Shoud return only putawayGroup1", filterBizO.Filter, putawayGroup1);

			moduleFilter.Property = "Test Group 2";
			Asserter.AssertMatches("Shoud return only putawayGroup2", filterBizO.Filter, putawayGroup2);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WhsPutawayGroupFilterBusinessObject();
		}

		FilterStripAsserter<WhsPutawayGroup> Asserter => asserter ?? (asserter = new FilterStripAsserter<WhsPutawayGroup>(Factory, putawayGroup => putawayGroup.WPG_Code));
		FilterStripAsserter<WhsPutawayGroup> asserter;

		protected WhsTestHelperFunctionsEnv Helper => helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory));
		WhsTestHelperFunctionsEnv helper;
	}
}
