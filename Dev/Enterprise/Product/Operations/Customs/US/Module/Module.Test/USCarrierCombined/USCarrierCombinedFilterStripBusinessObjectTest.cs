using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCarrierCombinedFilterStripBusinessObject))]
	sealed class USCarrierCombinedFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new USCarrierCombinedFilterStripBusinessObject();
			AssertNotNull(filter["Code"]);
			AssertNotNull(filter["Name"]);
			AssertNotNull(filter["Address"]);
			AssertNotNull(filter["Airway Bill Prefix"]);
			var moduleFilter = filter["Mode Of Transportation"] as ModuleTextFilter;
			AssertNotNull(moduleFilter);
			AssertEquals("List", typeof(TransportModeCodes), moduleFilter.List.GetType());
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCarrierCombinedFilterStripBusinessObject();
	}
}
