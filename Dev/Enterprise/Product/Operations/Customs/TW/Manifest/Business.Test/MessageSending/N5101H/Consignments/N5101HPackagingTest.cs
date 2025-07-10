using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HPackagingTest : TestCaseWithFactory
	{
		public void TestMarksNumbers()
		{
			AssertEquals("ABCD1234", packing.MarksNumbers);
		}

		public void TestPackagingMaterialDescription()
		{
			AssertEquals("RMARK123", packing.PackagingMaterialDescription);
		}

		public void TestCombination()
		{
			AssertEquals(YesNoList.Codes.Yes, packing.Combination);
		}

		public void TestTypeCode()
		{
			bill.ABL_ManifestUQ = "UNT";
			AssertEquals("UNT", packing.TypeCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_MarksAndNumbers = "ABCD1234";
			bill.ABL_Remarks = "RMARK123";
			packing = new N5101HPackaging(bill);
		}

		AsycudaBill bill;
		IPackaging packing;
	}
}
