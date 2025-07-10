using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class ETradeDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var eTradeData = Factory.New<ETradeData>();
			AssertEquals("CLNO, DRNO, INSCLK, RGNO, TRGNO", eTradeData.Lookups.CY_CodeList.CodesAsString);
		}
	}
}
