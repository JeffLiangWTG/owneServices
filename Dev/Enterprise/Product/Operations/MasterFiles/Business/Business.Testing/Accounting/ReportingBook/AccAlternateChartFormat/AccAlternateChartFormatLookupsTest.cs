using CargoWise.EntityFramework.Testing;
using static Enterprise.MasterFiles.Business.AccAlternateChartFormatLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccAlternateChartFormatLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSeparatorList()
		{
			var format = Factory.NewWithValidTestData<AccAlternateChartFormat>();
			var separatorList = format.Lookups.SeparatorList;

			AssertEquals("SeparatorList.Count", 2, separatorList.Count);

			AssertEquals("0th Element (Code)", SeparatorCode.DOT, separatorList[0].Code);
			AssertEquals("0th Element (Description)", SeparatorCode.DOT, separatorList[0].Description);

			AssertEquals("1th Element (Code)", SeparatorCode.LINE, separatorList[1].Code);
			AssertEquals("1th Element (Description)", SeparatorCode.LINE, separatorList[1].Description);
		}
	}
}
