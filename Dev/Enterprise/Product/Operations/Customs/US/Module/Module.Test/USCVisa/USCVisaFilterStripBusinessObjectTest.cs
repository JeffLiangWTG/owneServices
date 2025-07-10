using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCVisaFilterStripBusinessObject))]
	sealed class USCVisaFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new USCVisaFilterStripBusinessObject();
			AssertNotNull(filter["Textile Category No"]);
			AssertNotNull(filter["Country of Origin"]);
			AssertNotNull(filter["Visa Beginning Date"]);
			AssertNotNull(filter["Visa Ending Date"]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCVisaFilterStripBusinessObject();
	}
}
