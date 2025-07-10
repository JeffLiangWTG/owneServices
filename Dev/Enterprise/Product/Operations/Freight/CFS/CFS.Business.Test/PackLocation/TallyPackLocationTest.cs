using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(TallyPackLocation))]
	public class TallyPackLocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPackLine()
		{
			TallyPackLine line = Factory.New<TallyPackLine>();
			TallyPackLocation location = line.PackLocations.AddNew();

			AssertNotNull(location.PackLine);
			AssertEquals("TallyPackLocation.PackLine should be of type TallyPackLine.", typeof(TallyPackLine), ((PackLocation)location).PackLine.GetType());
			AssertEquals("TallyPackLocation.PackLine should load the parent TallyPackLine.", line, location.PackLine);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			TallyPackLine line = factory.NewWithValidTestData<TallyPackLine>();
			TallyPackLocation location = line.PackLocations.AddNew();

			return location;
		}

		#endregion
	}
}
