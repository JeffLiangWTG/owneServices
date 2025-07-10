using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class LoadedPackageInfoCacheTest : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LoadedPackageInfoCache(null, Enumerable.Empty<PkgPackageJob>()));
			AssertExceptionThrown<ArgumentNullException>(() => new LoadedPackageInfoCache(Factory, null));
		}

		public void TestGetLoadedPackageInfo_DoesNotAcceptNull()
		{
			var packageJobs = Enumerable.Empty<PkgPackageJob>();
			AssertExceptionThrown<ArgumentNullException>(() =>
				new LoadedPackageInfoCache(Factory, packageJobs).GetLoadedPackageInfo(null));
		}

		public void TestGetLoadedPackageInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			var packageJob = order.PackageJob;
			var loadedPackageInfo = new LoadedPackageInfoCache(Factory, new[] { packageJob }).GetLoadedPackageInfo(packageJob);
			AssertEquals("No Packages.", 0, loadedPackageInfo.Count);
		}

		public void TestGetLoadedPackageInfo_PackageJobOnOtherOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			Helper.CreatePickNew(order1);
			var packageJob1 = order1.PackageJob;
			var package1 = packageJob1.Packages.AddNew("CTN");

			var transportCo = Helper.CreateClient("Carrier");
			var load = Helper.CreateWhsLoad(transportCo, data.Whs1.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			Helper.CreateLoadPkgPackagePivot(package1.PK, load);

			Factory.Save();
			
			var loadedPackageInfo1 = new LoadedPackageInfoCache(Factory, new[] { packageJob1 }).GetLoadedPackageInfo(packageJob1);
			AssertEquals(1, loadedPackageInfo1.Count);
			AssertEquals(false, loadedPackageInfo1[package1.PK].IsLoaded);
			AssertEquals(load.WLO_JobID, loadedPackageInfo1[package1.PK].LoadID);

			Helper.CreatePickNew(order2);
			var packageJob2 = order2.PackageJob;
			var package2 = packageJob2.Packages.AddNew("BOX");
			Helper.CreateLoadPkgPackagePivot(package2.PK, load);

			Factory.Save();

			var loadedPackageInfo2 = new LoadedPackageInfoCache(Factory, new[] { packageJob1 }).GetLoadedPackageInfo(packageJob2);
			AssertEquals(0, loadedPackageInfo2.Count);
		}

		public void TestGetLoadedPackageInfo_MultipleOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			Helper.CreatePickNew(order1, order2);

			var packageJob1 = order1.PackageJob;
			var packageJob2 = order2.PackageJob;
			var package1 = packageJob1.Packages.AddNew("CTN");
			var package2 = packageJob2.Packages.AddNew("BOX");

			var transportCo = Helper.CreateClient("Carrier");
			var load = Helper.CreateWhsLoad(transportCo, data.Whs1.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			Helper.CreateLoadPkgPackagePivot(package2.PK, load);

			Factory.Save();

			var cache = new LoadedPackageInfoCache(Factory, new[] { packageJob1, packageJob2 });
			var loadedPackageInfo1 = cache.GetLoadedPackageInfo(packageJob1);
			AssertEquals(1, loadedPackageInfo1.Count);
			AssertEquals(false, loadedPackageInfo1[package1.PK].IsLoaded);
			AssertEquals(load.WLO_JobID, loadedPackageInfo1[package1.PK].LoadID);

			var loadedPackageInfo2 = cache.GetLoadedPackageInfo(packageJob2);
			AssertEquals(1, loadedPackageInfo2.Count);
			AssertEquals(false, loadedPackageInfo2[package2.PK].IsLoaded);
			AssertEquals(load.WLO_JobID, loadedPackageInfo2[package2.PK].LoadID);
		}

		public void TestGetLoadedPackageInfo_MultipleLoads()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			Helper.CreatePickNew(order1, order2);

			var packageJob1 = order1.PackageJob;
			var packageJob2 = order2.PackageJob;
			var package1 = packageJob1.Packages.AddNew("CTN");
			var package2 = packageJob1.Packages.AddNew("CTN");
			var package3 = packageJob2.Packages.AddNew("BOX");
			var package4 = packageJob2.Packages.AddNew("BOX");

			var transportCo = Helper.CreateClient("Carrier");
			var load1 = Helper.CreateWhsLoad(transportCo, data.Whs1.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			var load2 = Helper.CreateWhsLoad(transportCo, data.Whs1.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			var pivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load1);
			pivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot1.WLP_GS_NKLoadingUser = "E";
			Helper.CreateLoadPkgPackagePivot(package2.PK, load2);
			Helper.CreateLoadPkgPackagePivot(package3.PK, load1);
			var pivot2 = Helper.CreateLoadPkgPackagePivot(package4.PK, load2);
			pivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot2.WLP_GS_NKLoadingUser = "E";

			Factory.Save();

			var cache = new LoadedPackageInfoCache(Factory, new[] { packageJob1, packageJob2 });
			var loadedPackageInfo1 = cache.GetLoadedPackageInfo(packageJob1);
			AssertEquals(2, loadedPackageInfo1.Count);
			AssertEquals(true, loadedPackageInfo1[package1.PK].IsLoaded);
			AssertEquals(load1.WLO_JobID, loadedPackageInfo1[package1.PK].LoadID);
			AssertEquals(false, loadedPackageInfo1[package2.PK].IsLoaded);
			AssertEquals(load2.WLO_JobID, loadedPackageInfo1[package2.PK].LoadID);

			var loadedPackageInfo2 = cache.GetLoadedPackageInfo(packageJob2);
			AssertEquals(2, loadedPackageInfo2.Count);
			AssertEquals(false, loadedPackageInfo2[package3.PK].IsLoaded);
			AssertEquals(load1.WLO_JobID, loadedPackageInfo2[package3.PK].LoadID);
			AssertEquals(true, loadedPackageInfo2[package4.PK].IsLoaded);
			AssertEquals(load2.WLO_JobID, loadedPackageInfo2[package4.PK].LoadID);
		}
	}
}

