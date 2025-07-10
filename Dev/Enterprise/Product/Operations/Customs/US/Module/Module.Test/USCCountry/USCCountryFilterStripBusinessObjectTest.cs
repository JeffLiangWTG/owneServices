using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCCountryFilterStripBusinessObject))]
	sealed class USCCountryFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new USCCountryFilterStripBusinessObject();
			AssertNotNull(filter["Code"]);
			AssertNotNull(filter["Currency"]);
			AssertNotNull(filter["Name"]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCCountryFilterStripBusinessObject();
	}
}
