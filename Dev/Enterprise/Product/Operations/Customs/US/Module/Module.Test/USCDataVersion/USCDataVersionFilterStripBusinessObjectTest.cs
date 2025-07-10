using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCDataVersionFilterStripBusinessObject))]
	sealed class USCDataVersionFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new USCDataVersionFilterStripBusinessObject();
			AssertNotNull(filter["Name"]);
			AssertNotNull(filter["Note"]);
			AssertNotNull(filter["Version"]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCDataVersionFilterStripBusinessObject();
	}
}
