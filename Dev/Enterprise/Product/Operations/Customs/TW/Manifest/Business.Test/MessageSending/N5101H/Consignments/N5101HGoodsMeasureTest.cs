using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HGoodsMeasureTest : TestCaseWithFactory
	{
		public void TestGrossVolumeMeasure()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_Volume = 1;
			var goodsMeasure = new N5101HGoodsMeasure(bill);
			AssertEquals(1m, goodsMeasure.GrossVolumeMeasure);
		}

		public void TestVolumeUnitCode()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_VolumeUQ = "UN";
			var goodsMeasure = new N5101HGoodsMeasure(bill);
			AssertEquals("UN", goodsMeasure.VolumeUnitCode);
		}
	}
}
