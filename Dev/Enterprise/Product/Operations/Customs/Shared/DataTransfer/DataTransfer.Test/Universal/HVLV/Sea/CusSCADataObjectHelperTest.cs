using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing
{
	sealed class CusSCADataObjectHelperTest : TestCaseWithFactory
	{
		public void TestLoadHouseBills()
		{
			var houseBills = CusSCADataObjectHelper.LoadHouseBills<BaseCusSCAHouse>(oceanBill, Factory).ToList();
			AssertEquals(2, houseBills.Count);
			Assert(houseBills.Any(x => x.PK == house1.PK));
			Assert(houseBills.Any(x => x.PK == house2.PK));
		}

		public void TestLoadHouseBill()
		{
			var houseBill1 = CusSCADataObjectHelper.LoadHouseBill<BaseCusSCAHouse>(oceanBill, "HBL1", Factory);
			AssertEquals(house1.PK, houseBill1.PK);

			var houseBill2 = CusSCADataObjectHelper.LoadHouseBill<BaseCusSCAHouse>(oceanBill, "HBL2", Factory);
			AssertEquals(house2.PK, houseBill2.PK);
		}

		public void TestLoadContainers()
		{
			var containers = CusSCADataObjectHelper.LoadContainers<BaseCusSCAContainer>(oceanBill, Factory).ToList();
			AssertEquals(2, containers.Count);
			Assert(containers.Any(x => x.PK == container1.PK));
			Assert(containers.Any(x => x.PK == container2.PK));
		}

		public void TestLoadPackages()
		{
			var pivots = CusSCADataObjectHelper.LoadPackages<BaseCusSCAPivot>(house1, Factory).ToList();
			AssertEquals(1, pivots.Count);
			Assert(pivots.Any(x => x.PK == pivot1.PK));
		}

		TestCusSCAOceanBill oceanBill;
		CusSCAHouseForTest house1;
		CusSCAHouseForTest house2;
		CusSCAContainerForTest container1;
		CusSCAContainerForTest container2;
		CusSCAPivotForTest pivot1;
		CusSCAPivotForTest pivot2;
		protected override void SetUp()
		{
			base.SetUp();
			oceanBill = Factory.New<TestCusSCAOceanBill>();
			house1 = Factory.New<CusSCAHouseForTest>();
			house1.CA_CB = oceanBill.PK;
			house1.CA_HouseBill = "HBL1";
			house2 = Factory.New<CusSCAHouseForTest>();
			house2.CA_CB = oceanBill.PK;
			house2.CA_HouseBill = "HBL2";
			container1 = Factory.New<CusSCAContainerForTest>();
			container1.CN_CB = oceanBill.PK;
			container2 = Factory.New<CusSCAContainerForTest>();
			container2.CN_CB = oceanBill.PK;
			pivot1 = Factory.New<CusSCAPivotForTest>();
			pivot1.CV_CN = container1.PK;
			pivot1.CV_CA = house1.PK;
			pivot2 = Factory.New<CusSCAPivotForTest>();
			pivot2.CV_CN = container2.PK;
			pivot2.CV_CA = house2.PK;
		}
	}
}
