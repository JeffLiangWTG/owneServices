using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class ProductCodeUQListTest : TestCase
	{
		public void TestProductCodeUQList()
		{
			ProductCodeUQList productCodeUQList = new ProductCodeUQList();
			Assert(productCodeUQList is UnitOfQuantityCodeList);
			Assert(productCodeUQList.ContainsCode("HDS"));
			Assert(productCodeUQList.ContainsCode("PCE"));
			Assert(productCodeUQList.ContainsCode("PCS"));
			Assert(productCodeUQList.ContainsCode("KGS"));
			Assert(productCodeUQList.ContainsCode("MTQ"));
			Assert(productCodeUQList.ContainsCode("MC"));
		}
	}
}
