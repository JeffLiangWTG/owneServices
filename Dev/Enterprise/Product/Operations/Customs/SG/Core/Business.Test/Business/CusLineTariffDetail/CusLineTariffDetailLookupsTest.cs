using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CusLineTariffDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProductCodeUQList()
		{
			CusLineTariffDetailLookups lookups = new CusLineTariffDetailLookups(Factory.New<CusLineTariffDetail>());
			Assert(lookups.ProductCodeUQList is ProductCodeUQList);
		}
	}
}
