using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusOutturnLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackConditionList()
		{
			var outturn = Factory.New<CusOutturn>();
			AssertEquals("1, 2, 4, 3", outturn.Lookups.PackConditionList.CodesAsString);
		}

		public void TestWeightUQList()
		{
			var outturn = Factory.New<CusOutturn>();
			AssertEquals("DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN", outturn.Lookups.WeightUQList.CodesAsString);
		}

		public void TestVolumeUQList()
		{
			var outturn = Factory.New<CusOutturn>();
			AssertEquals("CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE", outturn.Lookups.VolumeUQList.CodesAsString);
		}

		public void TestCargoTypeList()
		{
			var outturn = Factory.New<CusOutturn>();
			AssertEquals("BB, CN, DB, LB", outturn.Lookups.CargoTypeList.CodesAsString);
		}

		public void TestExcessShortIndicatorList()
		{
			var outturn = Factory.New<CusOutturn>();
			AssertEquals("1, 3, 7, 5, 2, 8, 6, 4", outturn.Lookups.ExcessShortIndicatorList.CodesAsString);
		}

		public void TestParent()
		{
			var outturn = Factory.New<CusOutturn>();
			AssertType<CusOutturn>(outturn.Lookups.Parent);
		}
	}
}
