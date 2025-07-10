namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.EntityFramework.Testing;

	public class GoodsLocatedAtListTest : TestCaseWithFactory
	{
		public void TestGoodsLocatedAtListForSeaImport()
		{
			AssertEquals("Has import sea code for Port of Discharge", "DIS", GoodsLocatedAtListForSeaImport.Codes.DIS);
			AssertEquals("Has import sea code for Final Destination", "DES", GoodsLocatedAtListForSeaImport.Codes.DES);
		}
	}
}
