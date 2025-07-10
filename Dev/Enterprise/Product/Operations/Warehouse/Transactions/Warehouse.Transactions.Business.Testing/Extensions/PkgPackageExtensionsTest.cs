using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PkgPackageExtensionsTest : WhsTestCaseWithFactory
	{
		#region TestIsTote

		public void TestTotePackageType()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = Factory.New<PkgPackage>();
			package.KP_KJ_ParentPackageJob = packageJob.PK;
			package.KP_F3_NKPackType = "TOT";
			package.CartonGroupAndSize = "TEST1"; // CartonGroupAndSize also used GenAddOnColumn
			AssertEquals("Precondition: GetIsTote should be false.", false, package.GetIsTote());
			package.SetIsTote(true);
			AssertEquals("Should be set true.", true, package.GetIsTote());
			package.KP_F3_NKPackType = "TES";
			AssertEquals("Should remain true even if PackType is not Tote.", true, package.GetIsTote());
			package.SetIsTote(true);
			var totePackageAddOnColumnsQuery = new ZQuery(GenAddOnColumnSchema.XA_ParentID, package.PK);
			totePackageAddOnColumnsQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, "TotePackage");
			AssertEquals("1 row created for CartonGroupAndSize, 1 for totes", 1,
				Factory.Load<GenAddOnColumn>(totePackageAddOnColumnsQuery).Length);
			AssertEquals("Should be set true.", true, package.GetIsTote());
			package.SetIsTote(false);
			AssertEquals("Should be set false.", false, package.GetIsTote());
			AssertEquals("Should have deleted the genaddoncolumn for totes.", 0,
				Factory.Load<GenAddOnColumn>(totePackageAddOnColumnsQuery).Length);
			AssertEquals("But still 1 row for CartonGroupAndSize.", 1,
				Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_ParentID, package.PK)).Length);
			AssertNoExceptionThrown(() => package.SetIsTote(false));
		}

		public void TestSetIsTote_ClearsPackageActionStrategy()
		{
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, "1", Data.Part1, 5m);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew();
			AssertEquals("Precondition: GetIsTote package should be false.", false, package.GetIsTote());
			AssertEquals("Precondition: Package should be Not ReadOnly.", false, package.ReadOnly);
			package.SetIsTote(true);
			AssertEquals("Package is a tote.", true, package.IsToteOrParentIsTote());
			AssertEquals("Package is ReadOnly.", true, package.ReadOnly);
		}

		#endregion

		#region TestIsToteOrParentIsTote

		public void TestIsToteOrParentIsTote_PackageIsTote()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = PackingHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			AssertEquals("Precondition: GetIsTote should be false.", false, package.GetIsTote());
			package.SetIsTote(true);
			AssertEquals("Package is a tote.", true, package.IsToteOrParentIsTote());
		}

		public void TestIsToteOrParentIsTote_ParentIsTote()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = PackingHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			var childPackageOne = package.Packages.AddNew();
			var secondOrderChildPackage = childPackageOne.Packages.AddNew();
			AssertEquals("Precondition: GetIsTote package should be false.", false, package.GetIsTote());
			AssertEquals("Precondition: GetIsTote childPackageOne should be false.", false,
				childPackageOne.GetIsTote());
			AssertEquals("Precondition: GetIsTote secondOrderChildPackage should be false.", false,
				secondOrderChildPackage.GetIsTote());
			package.SetIsTote(true);
			AssertEquals("Package should be tote.", true, package.IsToteOrParentIsTote());
			AssertEquals("ChildPackageOne should be tote.", true, childPackageOne.IsToteOrParentIsTote());
			AssertEquals("SecondOrderChildPackage should be tote.", true,
				secondOrderChildPackage.IsToteOrParentIsTote());
			package.SetIsTote(false);
			childPackageOne.SetIsTote(true);
			AssertEquals("Package should not be tote.", false, package.IsToteOrParentIsTote());
			AssertEquals("ChildPackageOne should be tote.", true, childPackageOne.IsToteOrParentIsTote());
			AssertEquals("SecondOrderChildPackage should be tote.", true,
				secondOrderChildPackage.IsToteOrParentIsTote());
			childPackageOne.SetIsTote(false);
			secondOrderChildPackage.SetIsTote(true);
			AssertEquals("Package should not be tote.", false, package.IsToteOrParentIsTote());
			AssertEquals("ChildPackageOne should not be tote.", false, childPackageOne.IsToteOrParentIsTote());
			AssertEquals("SecondOrderChildPackage should be tote.", true,
				secondOrderChildPackage.IsToteOrParentIsTote());
		}

		#endregion

		#region TestGetMostRecentAudit

		public void TestGetMostRecentAudit_SingleAudit()
		{
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, "Order", Data.Part1, 50m);
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);

			var audit = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m,
				completeTimeoffset);
			var mostRecentAudit = package.GetMostRecentAudit();
			AssertNotNull("Most recent audit must be set.", mostRecentAudit);
			AssertEquals("Most recent audit must correspond to the only one we created for the package.", audit,
				mostRecentAudit);
		}

		public void TestGetMostRecentAudit_MultipleAudits()
		{
			var date = ZDateTimeOffset.Now;
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, "Order", Data.Part1, 50m);
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var audit1 = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m, date.AddHours(-2));
			var audit2 = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 48m, date.AddHours(-1));
			var audit3 = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 51m, date);
			var mostRecentAudit = package.GetMostRecentAudit();
			AssertNotNull("Most recent audit must be set.", mostRecentAudit);
			AssertEquals("Most recent audit must correspond to the last one we created for the package.", audit3,
				mostRecentAudit);
		}

		public void TestGetMostRecentAudit_NullParent()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = ZGuid.Empty;
			packageJob.KJ_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			var package = PackingHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			var date = ZDateTimeOffset.Now;
			var audit1 =
				Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m, date.AddHours(-2));
			AssertNull("Most recent audit must be null since the parent is null (needs valid order).",
				package.GetMostRecentAudit());
		}

		#endregion

		#region TestGetPickLines

		public void TestGetPickLines_PickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			package.Pack(order.Lines[1].ReleaseLines[0], 5m);
			Factory.Save();
			var pickLines = package.GetPickLines();
			AssertEquals("GetPickLines should return 2; we set up 2 pick lines on the package.", 2, pickLines.Count());
			Assert("GetPickLines should return a colleciton containing the pick line we put on the package.",
				pickLines.Contains(order.Lines[0].PickLines[0]));
			Assert("GetPickLines should return a colleciton containing the pick line we put on the package.",
				pickLines.Contains(order.Lines[1].PickLines[0]));
		}

		public void TestGetPickLines_ReleaseCaptureAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Factory.Save();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var orderline = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			var releaseLine = orderline.ReleaseLines[0];
			releaseLine.PartAttribute1 = "BLUE";
			var package = order.PackageJob.Packages.AddNew();
			var pickline = pick.GetAllPickLines().Single();
			package.Pack(pickline, releaseLine);
			Factory.Save();
			var pickLines = package.GetPickLines();
			AssertEquals("GetPickLines should return 1; we set up 1 release capture attributes on the package.", 1,
				pickLines.Count());
			Assert("GetPickLines should return a colleciton containing the pick line we put on the package.",
				pickLines.Contains(order.Lines[0].PickLines[0]));
		}

		public void TestGetPickLines_NullCase()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			Factory.Save();
			var pickLines = package.GetPickLines();
			AssertEquals("GetPickLines should return 0 for an empty package.", 0, pickLines.Count());
		}

		#endregion

		#region TestCannotDeleteAndUnpackPackage

		public void TestCannotDeleteAndUnpackPackage_EmptyPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT");
			Factory.Save();
			AssertEquals("CannotDeleteAndPackUnpackPackage should return false", false,
				package.CannotDeleteAndPackUnpackPackage());
		}

		public void TestCannotDeleteAndUnpackPackage_PickedPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();
			var package = order.PackageJob.Packages.AddNew("PLT");
			Factory.Save();
			AssertEquals("CannotDeleteAndPackUnpackPackage should be false when package is empty", false,
				package.CannotDeleteAndPackUnpackPackage());
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			order.Lines[0].PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("CannotDeleteAndPackUnpackPackage should be false when package is fully picked", false,
				package.CannotDeleteAndPackUnpackPackage());
		}

		public void TestCannotDeleteAndUnpackPackage_PackageBeingPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();
			var package = order.PackageJob.Packages.AddNew("PLT");
			Factory.Save();
			AssertEquals("CannotDeleteAndPackUnpackPackage should be false when package is empty", false,
				package.CannotDeleteAndPackUnpackPackage());
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			package.Pack(order.Lines[1].ReleaseLines[0], 5m);
			order.Lines[0].PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("CannotDeleteAndPackUnpackPackage should be true when package is partially picked", true,
				package.CannotDeleteAndPackUnpackPackage());
		}

		#endregion

		#region TestHasLoadedPivot

		public void TestHasLoadedPivot_NoPivots()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package = order.PackageJob.Packages.AddNew();

			Factory.Save();

			AssertEquals("Package has no WhsLoadPkgPackagePivots.", false, package.HasLoadedPivot());
		}

		public void TestHasLoadedPivot_HasLoadedPivot()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;

			Factory.Save();

			AssertEquals("Package has one Loaded WhsLoadPkgPackagePivot.", true, package.HasLoadedPivot());
		}

		public void TestHasLoadedPivot_HasUnloadedPivots()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot1.WLP_GS_NKUnloadingUser = "E";
			loadPkgPackagePivot1.WLP_UnloadedTime = ZDateTimeOffset.Now;

			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			loadPkgPackagePivot2.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot2.WLP_GS_NKUnloadingUser = "E";
			loadPkgPackagePivot2.WLP_UnloadedTime = ZDateTimeOffset.Now;

			Factory.Save();

			AssertEquals("Package has no Loaded WhsLoadPkgPackagePivot.", false, package.HasLoadedPivot());
		}

		public void TestHasLoadedPivot_HasLoadedAndUnloadedPivots()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot1.WLP_GS_NKUnloadingUser = "E";
			loadPkgPackagePivot1.WLP_UnloadedTime = ZDateTimeOffset.Now;

			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			loadPkgPackagePivot2.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot2.WLP_GS_NKUnloadingUser = "E";
			loadPkgPackagePivot2.WLP_UnloadedTime = ZDateTimeOffset.Now;

			var loadPkgPackagePivot3 = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			loadPkgPackagePivot3.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot3.WLP_LoadedTime = ZDateTimeOffset.Now;

			Factory.Save();

			AssertEquals("Package has one Loaded WhsLoadPkgPackagePivot.", true, package.HasLoadedPivot());
		}

		#endregion

		#region TestPacking_LongDecimal

		public void TestPack_LongDecimal()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 1m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			AssertExceptionThrown<InvalidOperationException>("Exception should be correct",
				$@"Split() Function on IPackableItem should Split the Quantity Correctly.
Packable Item PK: {order.Lines[0].PickLines[0].PK}
Table Prefix: WZ
Packable Item Quantity: 0.167
Quantity to split: 0.166667
Is Deleted: False
Is In Database: True
This error is seen because Packable Item Qty and Qty to split are different.
This can be because Split has failed, the scales of the two decimals are different because one was rounded to fit a database property scale (i.e. 1.55556 vs 1.556), or other unknown reason.",
				() => package.Pack(order.Lines[0].ReleaseLines[0], 0.166667m));
		}

		#endregion

		#region TestHelpers

		protected PackingTestHelper PackingHelper
		{
			get
			{
				return packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
			}
		}

		PackingTestHelper packingHelper;

		TestDataSimpleEnvironment Data
		{
			get
			{
				return data ?? (data = new TestDataSimpleEnvironment(Factory, 2, 2));
			}
		}

		TestDataSimpleEnvironment data;

		#endregion
	}
}
