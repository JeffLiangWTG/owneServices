using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCAffirmationOfComplianceFilterStripBusinessObject))]
	sealed class USCAffirmationOfComplianceFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new USCAffirmationOfComplianceFilterStripBusinessObject();
			AssertNotNull(filter["Code"]);
			AssertNotNull(filter["Description"]);
			AssertNotNull(filter["Is Qualifier"]);
			AssertNotNull(filter["Is Expired"]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCAffirmationOfComplianceFilterStripBusinessObject();
	}
}
