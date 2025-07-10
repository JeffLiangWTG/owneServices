using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using PickTrolleyStatus = Enterprise.Warehouse.Transactions.TrolleyPicking.PickTrolleyStatus;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ChangePackageCartonGroupAndSizeInfoTest : WhsSecureServiceTestCase
	{
		#region TestChangePackageCartonInfo

		#region TestChangePackageCartonGroupAndSizeInfo

		public void TestChangePackageCartonGroupAndSizeInfo()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Box, 4);

			var whsGroup = Helper.CreateWhsCartonGroup("GRP", "WhsGrp");
			var smallCarton = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Centimetres, Constants.Weight.Tonnes);
			smallCarton.WCS_VolumeUQ = Constants.Volume.CubicInches;
			var bigCarton = Helper.CreateWhsCartonSize("BIG", 20, 20, 20, 20, 40, 80, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
			whsGroup.CartonSizes.AddRange(new[] { smallCarton, bigCarton });
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			// BOX is a cases and UNT is split case
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock

			// Order 5 units, 1 box and 1 unt
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.WP_PickCasesByLabel = true;
			AssertEquals("Precondition:", 5m, order.Lines[0].PickLineQuantity);
			Helper.Factory.Save(); // to create stock
			pick.AllocatePackageLabels();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var casePkg = pkgJob.Packages.Single(p => p.PackedItems.Cast<PkgPackageItemDivotsWrapper>().Any(d => d.PackedQty == 4m));
			var splitCasePkg = pkgJob.Packages.Single(p => p.PackedItems.Cast<PkgPackageItemDivotsWrapper>().Any(d => d.PackedQty == 1m));

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var slot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, casePkg, 1);
			var slot2 = Helper.CreateWhsPickTrolleySlot(trolleyJob, splitCasePkg, 2);

			Helper.Factory.Save();

			//CasePkg
			AssertEquals("Precondition:", string.Empty, casePkg.CartonGroupAndSize);
			AssertEquals("Precondition:", 0m, casePkg.KP_Length);
			AssertEquals("Precondition:", 0m, casePkg.KP_Width);
			AssertEquals("Precondition:", 0m, casePkg.KP_Height);
			AssertEquals("Precondition:", 0m, casePkg.KP_Volume);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ChangePackageCartonGroupAndSizeInfo(trolleyJob.PK.ToGuid(), casePkg.KP_PackageID, "GRP", "SML");
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
			});

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var newCasePkg = newFactory.Load<PkgPackage>(casePkg.PK);

			CombineAssertions(() =>
			{
				AssertNotNull(newCasePkg);
				AssertEquals("GRP - SML", newCasePkg.CartonGroupAndSize);
				AssertEquals(10m, newCasePkg.KP_Length);
				AssertEquals(10m, newCasePkg.KP_Width);
				AssertEquals(10m, newCasePkg.KP_Height);
				AssertEquals(61.024m, newCasePkg.KP_Volume);
				AssertEquals(Constants.Length.Centimetres, newCasePkg.KP_DimensionUQ);
				AssertEquals(Constants.Volume.CubicInches, newCasePkg.KP_VolumeUQ);
			});

			//SplicCasePkg
			AssertEquals("Precondition:", "GRP - BIG", splitCasePkg.CartonGroupAndSize);
			AssertEquals("Precondition:", 20m, splitCasePkg.KP_Length);
			AssertEquals("Precondition:", 20m, splitCasePkg.KP_Width);
			AssertEquals("Precondition:", 20m, splitCasePkg.KP_Height);
			AssertEquals("Precondition:", 8000m, splitCasePkg.KP_Volume);

			var webService2 = GetNewWebService(data.Whs1);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				var response2 = webService2.ChangePackageCartonGroupAndSizeInfo(trolleyJob.PK.ToGuid(), splitCasePkg.KP_PackageID, "GRP", "SML");
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
			});

			var newSplitCasePkg = newFactory.Load<PkgPackage>(splitCasePkg.PK);

			CombineAssertions(() =>
			{
				AssertNotNull(newSplitCasePkg);
				AssertEquals("GRP - SML", newSplitCasePkg.CartonGroupAndSize);
				AssertEquals(10m, newSplitCasePkg.KP_Length);
				AssertEquals(10m, newSplitCasePkg.KP_Width);
				AssertEquals(10m, newSplitCasePkg.KP_Height);
				AssertEquals(61.024m, newSplitCasePkg.KP_Volume);
				AssertEquals(Constants.Length.Centimetres, newSplitCasePkg.KP_DimensionUQ);
				AssertEquals(Constants.Volume.CubicInches, newSplitCasePkg.KP_VolumeUQ);
			});
		}

		public void TestChangePackageCartonGroupAndSizeInfo_TrolleyJobNotFound()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Box, 4);

			var whsGroup = Helper.CreateWhsCartonGroup("GRP", "WhsGrp");
			var bigCarton = Helper.CreateWhsCartonSize("BIG", 20, 20, 20, 20, 40, 80, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
			whsGroup.CartonSizes.Add(bigCarton);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			// BOX is a cases and UNT is split case
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock

			// Order 5 units, 1 box and 1 unt
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			AssertEquals("Precondition:", 5m, order.Lines[0].PickLineQuantity);
			Helper.Factory.Save(); // to create stock
			pick.AllocatePackageLabels();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var casePkg = pkgJob.Packages.First();

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var slot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, casePkg, 1);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ChangePackageCartonGroupAndSizeInfo(Guid.NewGuid(), casePkg.KP_PackageID, "GRP", "BIG");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Trolley job was not found. Please start building Trolley again.", response.ErrorMessage);
			});
		}

		public void TestChangePackageCartonGroupAndSizeInfo_PackageMissingFromTrolley()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var whsGroup = Helper.CreateWhsCartonGroup("GRP", "WhsGrp");
			var bigCarton = Helper.CreateWhsCartonSize("BIG", 20, 20, 20, 20, 40, 80, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
			whsGroup.CartonSizes.Add(bigCarton);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ChangePackageCartonGroupAndSizeInfo(trolleyJob.PK.ToGuid(), "ABC", whsGroup.WCG_Code, "BIG");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Package 'ABC' was not on this Trolley.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestChangePackageCartonGroupAndSizeInfo_WrongCartonInfo

		public void TestChangePackageCartonGroupAndSizeInfo_CartonGroupNotExist()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Box, 4);

			var whsGroup = Helper.CreateWhsCartonGroup("GRP", "WhsGrp");
			var smallCarton = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
			var bigCarton = Helper.CreateWhsCartonSize("BIG", 20, 20, 20, 20, 40, 80, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
			whsGroup.CartonSizes.AddRange(new[] { smallCarton, bigCarton });
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			// BOX is a cases and UNT is split case
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock

			// Order 5 units, 1 box and 1 unt
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.WP_PickCasesByLabel = true;
			AssertEquals("Precondition:", 5m, order.Lines[0].PickLineQuantity);
			Helper.Factory.Save(); // to create stock
			pick.AllocatePackageLabels();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var casePkg = pkgJob.Packages.Single(p => p.PackedItems.Cast<PkgPackageItemDivotsWrapper>().Any(d => d.PackedQty == 4m));
			var splitCasePkg = pkgJob.Packages.Single(p => p.PackedItems.Cast<PkgPackageItemDivotsWrapper>().Any(d => d.PackedQty == 1m));

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var slot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, casePkg, 1);
			var slot2 = Helper.CreateWhsPickTrolleySlot(trolleyJob, splitCasePkg, 2);

			AssertEquals("Precondition:", "GRP - SML", splitCasePkg.CartonGroupAndSize);

			whsGroup.Delete();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ChangePackageCartonGroupAndSizeInfo(trolleyJob.PK.ToGuid(), splitCasePkg.KP_PackageID, "GRP", "SML");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Carton Group was not found.", response.ErrorMessage);
			});
		}

		public void TestChangePackageCartonGroupAndSizeInfo_CartonSizeNotExist()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Box, 4);

			var whsGroup = Helper.CreateWhsCartonGroup("GRP", "WhsGrp");
			var smallCarton = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
			whsGroup.CartonSizes.AddRange(smallCarton);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			// BOX is a cases and UNT is split case
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock

			// Order 5 units, 1 box and 1 unt
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.WP_PickCasesByLabel = true;
			AssertEquals("Precondition:", 5m, order.Lines[0].PickLineQuantity);
			Helper.Factory.Save(); // to create stock
			pick.AllocatePackageLabels();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var casePkg = pkgJob.Packages.First();

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var slot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, casePkg, 1);

			AssertEquals("Precondition:", "GRP - SML", casePkg.CartonGroupAndSize);

			whsGroup.CartonSizes.DeleteAll();
			Helper.Factory.Save();

			AssertEquals(false, whsGroup.CartonSizes.Any());

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ChangePackageCartonGroupAndSizeInfo(trolleyJob.PK.ToGuid(), casePkg.KP_PackageID, "GRP", "SML");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Specified Carton Size was not in Carton Group 'GRP'.", response.ErrorMessage);
			});
		}

		#endregion

		#endregion
	}
}
