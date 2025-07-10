using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCQuotaFilterStripBusinessObject))]
	sealed class USCQuotaFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new USCQuotaFilterStripBusinessObject();
			AssertNotNull(filter["Tariff/Category/Visa Number"]);
			AssertNotNull(filter["Country of Origin"]);
			AssertNotNull(filter["Second Tariff Number"]);
			AssertNotNull(filter["First Namesake"]);
			AssertNotNull(filter["Second Namesake"]);
			AssertNotNull(filter["Quota Beginning Date"]);
			AssertNotNull(filter["Quota Ending Date"]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCQuotaFilterStripBusinessObject();
	}
}
