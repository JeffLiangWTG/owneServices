using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCCarrierAndFIRMSFilterStripBusinessObject))]
	sealed class USCCarrierAndFIRMSFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new USCCarrierAndFIRMSFilterStripBusinessObject();
			AssertNotNull(filter["Code"]);
			AssertNotNull(filter["Name"]);
			AssertNotNull(filter["Type"]);
			AssertNotNull(filter["District Port"]);
			AssertNotNull(filter["Facility Type"]);
			AssertNotNull(filter["Address"]);
			AssertNotNull(filter["Transportation Mode"]);
			AssertNotNull(filter["IsActive"]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCCarrierAndFIRMSFilterStripBusinessObject();
	}
}
