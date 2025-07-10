using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(USCRegionDistrictPortFilterStripBusinessObject))]
	public class USCRegionDistrictPortFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new USCRegionDistrictPortFilterStripBusinessObject();
			AssertNotNull(filter["Code"]);
			AssertNotNull(filter["Name"]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCRegionDistrictPortFilterStripBusinessObject();
	}
}
