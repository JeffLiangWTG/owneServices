using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ClosePackageTest : WhsSecureServiceTestCase
	{
		#region TestClosePackage

		public void TestClosePackage_PackageIsNull()
		{
			var webService = GetNewWebService();
			AssertBusinessValidationError(webService, "packagePackInto cannot be null.", webService.ClosePackage(null, false));
		}

		public void TestClosePackage_PackageIsEmpty()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			var packageInfo = new PackageInfo(package);
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response = webService1.ClosePackage(packageInfo, false);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("There is nothing packed in Package with ID 'ABC'.", response.ErrorMessage);
			AssertEquals("Package should be closed", false, package.KP_ClosedTimeUtc.IsValid);
		}

		public void TestClosePackage_PackageIsClosed()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			var packageInfo = new PackageInfo(package);
			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response = webService1.ClosePackage(packageInfo, false);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Package with ID 'ABC' is already closed.", response.ErrorMessage);
		}

		public void TestClosePackage_PackageDoesNotExists()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			var packageInfo = new PackageInfo(package);
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response = webService1.ClosePackage(new PackageInfo { PackageID = "DEF", PK = Guid.NewGuid() }, false);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Package ID 'DEF' does not exist.", response.ErrorMessage);
		}

		public void TestClosePackage()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			var packageInfo = new PackageInfo(package);
			package.Pack(order.Lines[0].ReleaseLines[0], 2m);
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response = webService1.ClosePackage(packageInfo, false);
			AssertEquals("Package should be closed", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Package should be closed", true, package.KP_ClosedTimeUtc.IsValid);
		}

		public void TestClosePackage_SaveFails()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 100m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			var packageInfo = new PackageInfo(package);
			package.Pack(order.Lines[0].ReleaseLines[0], 2m);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			webService.Factory.Saving += f => throw new ZCannotSaveException("Test", "Test");
			var response = webService.ClosePackage(packageInfo, false);
			AssertEquals("ZCannotSaveException should be logged as an Error.", "Test", response.ErrorMessage);
			AssertEquals("ZCannotSaveException should be logged as an Error.", ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestClosePackage_ZSaveConcurrencyExceptionThrown()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 100m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			var packageInfo = new PackageInfo(package);
			package.Pack(order.Lines[0].ReleaseLines[0], 2m);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)package).Row, Db.Connection);
			webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);
			var response = webService.ClosePackage(packageInfo, false);
			AssertEquals("ZSaveConcurrencyException should be logged as an Error.", "Another user has changed the package while you have been working on it. Please restart the operation and try again.", response.ErrorMessage);
			AssertEquals("ZSaveConcurrencyException should be logged as an Error.", ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestClosePackage_DeferClosingPackage()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 100m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "ABC");
			var packageInfo = new PackageInfo(package);
			package.Pack(order.Lines[0].ReleaseLines[0], 2m);
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response = webService1.ClosePackage(packageInfo, deferClosingPackage: true);
			AssertEquals("Package should have no error for attempted Close.", true, string.IsNullOrEmpty(response.ErrorMessage));

			var otherFactory = new BusinessObjectFactory();
			AssertEquals("Package should *not* be closed in DB as we are deferring the Close.", false, otherFactory.Load<PkgPackage>(package.PK).KP_ClosedTimeUtc.IsValid);
		}

		#endregion

		#region TestPackIntoPackage

		public void TestPackIntoPackage()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Helper.Factory.Save();

			var location = receive.Inventory[0].Location;
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location, location);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 5m, transferLine.QtyCommittedIncludingMatchingLines);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew("PLT", "ABC");
			var package2 = order.PackageJob.Packages.AddNew("PLT", "XYZ");
			package2.Pack(order.Lines[0].ReleaseLines[0], 2m);

			var nonOrderPickLine = transferLine.PickLines.Single();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { nonOrderPickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1m, false, true),
				new PackageInfo { PackageID = "ABC", PK = package1.PK.ToGuid() },
				false);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
				AssertEquals(string.Format("No Warehouse Order found.", receive.WD_DocketID), response1.ErrorMessage);
			});

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pick.GetAllPickLines().First().PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1m, false, true),
				new PackageInfo { PackageID = "DEF", PK = Guid.NewGuid() },
				false);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
				AssertEquals("Package ID 'DEF' does not exist.", response2.ErrorMessage);
			});

			package1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			var webService3 = GetNewWebService(data.Whs1);
			var response3 = webService3.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pick.GetAllPickLines().First().PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1m, false, true),
				new PackageInfo { PackageID = "ABC", PK = package1.PK.ToGuid() },
				false);
			AssertSuccessfulResponse(response3, webService3);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response3.Error);
				AssertEquals("Package with ID 'ABC' is already closed.", response3.ErrorMessage);
			});

			package1.KP_ClosedTimeUtc = ZDateTime.Empty;
			Helper.Factory.Save();

			var webService4 = GetNewWebService(data.Whs1);
			var response4 = webService4.ClosePackage(new PackageInfo { PackageID = "ABC", PK = package1.PK.ToGuid() }, false);
			AssertSuccessfulResponse(response4, webService4);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response4.Error);
				AssertEquals("There is nothing packed in Package with ID 'ABC'.", response4.ErrorMessage);
			});

			order.Lines[0].ClearReleaseLines();
			var pickLine = order.Lines[0].PickLines.Single(pl => pl.IsUnpacked(Helper.Factory));
			var packedPickLine = order.Lines[0].PickLines.Single(pl => !pl.IsUnpacked(Helper.Factory));

			var webService5 = GetNewWebService(data.Whs1);
			var response5 = webService5.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1m, false, true),
				new PackageInfo { PackageID = "ABC", PK = package1.PK.ToGuid() },
				false);
			AssertSuccessfulResponse(response5, webService5);

			var newPickLine = order.Lines[0].PickLines.Single(pl => pl != pickLine && pl != packedPickLine);
			CombineAssertions(() =>
			{
				AssertEquals("Should have packed Goods.", 1, package1.PackedItems.Count);
				AssertEquals("Should have packed Correct Release Line.", order.PackableItemParents.Typed.Single(), package1.PackedItems[0].PackableItemParent);
				AssertEquals("Should have packed Correct Pick Line.", newPickLine, package1.PackedItems[0].PackedItems.Single());
				AssertEquals("Should have packed Correct Amount of Goods.", 1m, package1.PackedItems[0].PackedQty);
				AssertEquals("Should have saved Packing.", false, ((BusinessObject)package1.PackedItems[0].PackedItems.Single()).HasChanges);
			});

			var webService6 = GetNewWebService(data.Whs1);
			var response6 = webService6.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(7m, false, true),
				new PackageInfo { PackageID = "ABC", PK = package1.PK.ToGuid() },
				false);
			AssertSuccessfulResponse(response6, webService6);

			var webService7 = GetNewWebService(data.Whs1);
			var response7 = webService7.ClosePackage(new PackageInfo { PackageID = "ABC", PK = package1.PK.ToGuid() }, false);
			AssertSuccessfulResponse(response7, webService7);

			CombineAssertions(() =>
			{
				AssertEquals("Should have packed Goods.", 1, package1.PackedItems.Count);
				AssertEquals("Should have packed Correct Pick Line and Amount of Goods.", 1,
					package1.PackedItems.Typed.Count(p => p.PackableItemParent == order.PackableItemParents.Typed.Single() && p.PackedItems.Any(d => d == newPickLine && d.Quantity == 1m)));
				AssertEquals("Should have packed Correct Pick Line and Amount of Goods.", 1,
					package1.PackedItems.Typed.Count(p => p.PackableItemParent == order.PackableItemParents.Typed.Single() && p.PackedItems.Any(d => d == pickLine && d.Quantity == 7m)));
				AssertEquals("Should have Closed Package.", true, package1.KP_ClosedTimeUtc.IsValid);
				AssertEquals("Should have saved Packing and Closing of Package.", false, package1.HasChanges);
			});

			var webService8 = GetNewWebService(data.Whs1);
			var response8 = webService8.ClosePackage(new PackageInfo { PackageID = "XYZ", PK = package2.PK.ToGuid() }, false);
			AssertSuccessfulResponse(response8, webService8);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response5.Error);
				AssertEquals("Should have Closed Package.", true, package2.KP_ClosedTimeUtc.IsValid);
			});
		}

		#endregion
	}
}
