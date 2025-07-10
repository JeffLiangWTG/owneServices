using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCTeamSpecialistFilterStripBusinessObject))]
	sealed class USCTeamSpecialistFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new USCTeamSpecialistFilterStripBusinessObject();
			AssertNotNull(filter["District/Port Code"]);
			AssertNotNull(filter["Tariff From"]);
			AssertNotNull(filter["Tariff To"]);
			AssertNotNull(filter["Country Code (From)"]);
			AssertNotNull(filter["Country Code (To)"]);
			AssertNotNull(filter["Team Number"]);
			AssertNotNull(filter["Importer Name"]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCTeamSpecialistFilterStripBusinessObject();
	}
}
