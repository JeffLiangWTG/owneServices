using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSPackLocation))]
	public class CFSPackLocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPackLine()
		{
			CFSPackLine line = Factory.NewWithValidTestData<CFSPackLine>();
			CFSPackLocation location = line.PackLocations.AddNew();

			AssertNotNull(location.PackLine);
			AssertEquals("CFSPackLocation.PackLine should be of type CFSPackLine.", typeof(CFSPackLine), ((PackLocation)location).PackLine.GetType());
			AssertEquals("CFSPackLocation.PackLine should load the parent CFSPackLine.", line, location.PackLine);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CFSPackLine line = factory.NewWithValidTestData<CFSPackLine>();
			CFSPackLocation location = line.PackLocations.AddNew();

			return location;
		}

		#endregion
	}
}
