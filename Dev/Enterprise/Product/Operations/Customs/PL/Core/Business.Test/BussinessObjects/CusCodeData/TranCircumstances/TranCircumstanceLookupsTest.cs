using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class TranCircumstanceLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestTranCircumstancesList() => AssertEquals("A00PL, B00PL, C00PL, D00PL, J00PL, K00PL", Factory.New<TranCircumstance>().Lookups.TranCircumstancesList.CodesAsString);
}
