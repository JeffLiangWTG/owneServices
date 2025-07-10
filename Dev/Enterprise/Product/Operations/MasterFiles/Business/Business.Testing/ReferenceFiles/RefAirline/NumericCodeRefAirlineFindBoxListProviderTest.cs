using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class NumericCodeRefAirlineFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestNumericCodeRefAirlineFindBoxListProvider_ShowsInactiveAirline()
		{
			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "TES";
			airline.RM_IsActive = false;

			var collection = new NumericCodeRefAirlineCollection(Factory);
			var provider = new NumericCodeRefAirlineFindBoxListProvider(collection);
			var refAirlines = provider.GetBusinessObjectsFromCodeWithoutFilter("TES");

			AssertEquals("Inactive Airline should be found", airline.PK, refAirlines.First().PK);
		}
	}
}
