using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsProductUNDGInfoTest : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			var productPK = new ZGuid();
			var dgSubstance = new ZGuid();
			var dgCode = "dg";
			var countryReference = new ZGuid();
			var dcrCode = "country";
			var undgClass = "1";

			var productUNDGInfo = new WhsProductUNDGInfo(
				productPK,
				dgSubstance,
				dgCode,
				countryReference,
				dcrCode,
				undgClass,
				10m,
				"KG",
				20m,
				"M3");
			CombineAssertions(() =>
			{
				AssertEquals(productPK, productUNDGInfo.ProductPK);
				AssertEquals(dgSubstance, productUNDGInfo.DG_PK);
				AssertEquals(countryReference, productUNDGInfo.DCR_PK);
				AssertEquals(undgClass, productUNDGInfo.UNDGClass);
				AssertEquals(10m, productUNDGInfo.DG_Weight);
				AssertEquals("KG", productUNDGInfo.DG_WeightUQ);
				AssertEquals(20m, productUNDGInfo.DG_Volume);
				AssertEquals("M3", productUNDGInfo.DG_VolumeUQ);
			});
		}
	}
}
