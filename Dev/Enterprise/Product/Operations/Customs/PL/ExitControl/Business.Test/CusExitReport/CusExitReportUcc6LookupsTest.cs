using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.PL;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class CusExitReportUcc6LookupsTest : TestCaseWithFactory
{
	public void TestStatusList() => CombineAssertions(() =>
	{
		var statusList = cusExitReportUcc6Lookups.StatusList;

		AssertType<AESEntryStatusList>("List type", statusList);
		AssertSame("List is cached", statusList, cusExitReportUcc6Lookups.StatusList);
		AssertEquals("list contains empty code", true, statusList.ContainsCode(string.Empty));
		AssertEquals("list contains ERR code", true, statusList.ContainsCode("ERR"));
	});

	protected override void SetUp()
	{
		base.SetUp();

		var exitReport = Factory.New<CusExitReport>();
		cusExitReportUcc6Lookups = new(exitReport);
	}

	CusExitReportUcc6Lookups cusExitReportUcc6Lookups;
}
