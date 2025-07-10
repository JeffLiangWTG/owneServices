using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeaderLookups))]
sealed class CusTempStorageRegHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestStatusList()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		var lookups = header.Lookups;
		var statusList = lookups.StatusList;

		CombineAssertions(() =>
		{
			AssertEquals("List Values", "DEL, FIN, PAC, TST", statusList.CodesAsString);
			AssertEquals("Default Code", "TST", statusList.DefaultCode);
			AssertSame("Cached", statusList, lookups.StatusList);
		});
	}
}
