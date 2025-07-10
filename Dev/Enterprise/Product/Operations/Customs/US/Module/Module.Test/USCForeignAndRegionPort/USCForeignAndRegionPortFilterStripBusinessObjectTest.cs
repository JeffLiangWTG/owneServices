using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCForeignAndRegionPortFilterStripBusinessObject))]
	sealed class USCForeignAndRegionPortFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new USCForeignAndRegionPortFilterStripBusinessObject();
			AssertNotNull(filter["Port Code"]);
			AssertNotNull(filter["Port Name"]);
			AssertNotNull(filter["Port Type"]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCForeignAndRegionPortFilterStripBusinessObject();
	}
}
