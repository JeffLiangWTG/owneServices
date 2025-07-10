using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.NL.ExitControl.Business.Testing;

sealed class ExitControlMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestEntryTypeList()
	{
		var exitReport = Factory.New<CusExitReport>();
		var control = new ExitControlMessageSendingObject(exitReport);
		var lookups = control.Lookups;
		var list = lookups.EntryTypeList;

		CombineAssertions(() =>
		{
			AssertEquals("Entry Type List", "5, 9", list.CodesAsString);
			AssertSame("Cached", list, lookups.EntryTypeList);
		});
	}
}
