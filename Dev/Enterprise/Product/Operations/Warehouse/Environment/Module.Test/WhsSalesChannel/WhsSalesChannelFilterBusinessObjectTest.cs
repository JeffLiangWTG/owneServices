using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(WhsSalesChannelFilterBusinessObject))]
	class WhsSalesChannelFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCode()
		{
			var salesChannel1 = Helper.CreateWhsSalesChannel("SC1", "Test Channel 1");
			var salesChannel2 = Helper.CreateWhsSalesChannel("SC2", "Test Channel 2");
			Factory.Save();
			Asserter.AddToScope(salesChannel1, salesChannel2);

			var filterBizO = GetNewFilterStripBusinessObject();
			var moduleFilter = (ModuleTextFilter)filterBizO[WhsSalesChannelFilterBusinessObject.Schema.Code];
			moduleFilter.IsActive = true;
			moduleFilter.Property = "SC1";
			Asserter.AssertMatches("Shoud  return only salesChannel1", filterBizO.Filter, salesChannel1);

			moduleFilter.Property = "SC2";
			Asserter.AssertMatches("Shoud return only salesChannel2", filterBizO.Filter, salesChannel2);
		}

		public void TestDescription()
		{
			var salesChannel1 = Helper.CreateWhsSalesChannel("SC1", "Test Channel 1");
			var salesChannel2 = Helper.CreateWhsSalesChannel("SC2", "Test Channel 2");
			Factory.Save();
			Asserter.AddToScope(salesChannel1, salesChannel2);

			var filterBizO = GetNewFilterStripBusinessObject();
			var moduleFilter = (ModuleTextFilter)filterBizO[WhsSalesChannelFilterBusinessObject.Schema.Description];
			moduleFilter.IsActive = true;
			moduleFilter.Property = "Test Channel 1";
			Asserter.AssertMatches("Shoud  return only salesChannel1", filterBizO.Filter, salesChannel1);

			moduleFilter.Property = "Test Channel 2";
			Asserter.AssertMatches("Shoud return only salesChannel2", filterBizO.Filter, salesChannel2);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new WhsSalesChannelFilterBusinessObject();

		FilterStripAsserter<WhsSalesChannel> Asserter => asserter ?? (asserter = new FilterStripAsserter<WhsSalesChannel>(Factory, salesChannel => salesChannel.WSH_Code));
		FilterStripAsserter<WhsSalesChannel> asserter;

		protected WhsTestHelperFunctionsEnv Helper => helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory));
		WhsTestHelperFunctionsEnv helper;
	}
}
