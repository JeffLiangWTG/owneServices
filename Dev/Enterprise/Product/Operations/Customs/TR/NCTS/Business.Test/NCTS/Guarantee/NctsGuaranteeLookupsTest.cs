using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	public class NctsGuaranteeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBondTypeList()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = header.Guarantees.AddNew();
			var lookups = new NctsGuaranteeLookups(guarantee);
			CombineAssertions(() =>
			{
				var list = lookups.BondTypeList;
				AssertSame("Cached", new NctsGuaranteeLookups(guarantee).BondTypeList, list);
				AssertEquals("1, 2, 3, 7, 8, B, D, I, R, S", list.CodesAsString);
			});
		}
	}
}
