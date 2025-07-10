using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using PickTrolleyStatus = Enterprise.Warehouse.Transactions.TrolleyPicking.PickTrolleyStatus;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetCartonGroupUsingTrolleyJobPKAndPackageIDTest : WhsSecureServiceTestCase
	{
		#region TestGetCartonGroupInfo

		#region TestGetCartonGroupUsingTrolleyJobPKAndPackageID

		public void TestGetCartonGroupUsingTrolleyJobPKAndPackageID()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Box, 4);

			var cartonGroup1 = Helper.CreateWhsCartonGroup("OWN", "OWN");
			var smallCarton = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
			var bigCarton = Helper.CreateWhsCartonSize("BIG1", 20, 20, 20, 20, 40, 80, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
			cartonGroup1.CartonSizes.AddRange(new[] { smallCarton, bigCarton });
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = cartonGroup1.PK;

			var relationshipOwner = data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			relationshipOwner.OU_WCG_CartonGroup = cartonGroup1.PK;

			// Supplier relationship should be ignored
			var cartonGroup2 = Helper.CreateWhsCartonGroup("SUP", "SUP");
			cartonGroup2.CartonSizes.AddRange(new[] { smallCarton, bigCarton });
			var relationshipSupplier = data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var cartonGroup3 = Helper.CreateWhsCartonGroup("BTH", "BTH");
			cartonGroup3.CartonSizes.AddRange(new[] { smallCarton, bigCarton });
			var relationshipBoth = data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Both);
			relationshipBoth.OU_WCG_CartonGroup = cartonGroup3.PK;

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

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetCartonGroupUsingTrolleyJobPKAndPackageID(trolleyJob.PK.ToGuid(), splitCasePkg.KP_PackageID);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var cartonGroups = response.CartonGroups;
			AssertNotNull(cartonGroups);
			AssertEquals(2, cartonGroups.Count);

			AssertContainsExactElementsInAnyOrder(new[] { "OWN", "BTH" }, cartonGroups.Select(g => g.Code));

			CombineAssertions(() =>
			{
				AssertEquals(2, cartonGroups[0].CartonSizes.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SML", "BIG1" }, cartonGroups[0].CartonSizes.Select(g => g.Code));

				AssertEquals(2, cartonGroups[1].CartonSizes.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SML", "BIG1" }, cartonGroups[1].CartonSizes.Select(g => g.Code));
			});
		}

		public void TestGetCartonGroupUsingTrolleyJobPKAndPackageID_TrolleyJobNotFound()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Box, 4);

			var cartonGroup1 = Helper.CreateWhsCartonGroup("OWN", "OWN");
			var smallCarton = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
			cartonGroup1.CartonSizes.Add(smallCarton);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = cartonGroup1.PK;

			var relationshipOwner = data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			relationshipOwner.OU_WCG_CartonGroup = cartonGroup1.PK;

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
			var response = webService.GetCartonGroupUsingTrolleyJobPKAndPackageID(Guid.NewGuid(), casePkg.KP_PackageID);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Trolley job was not found. Please start building Trolley again.", response.ErrorMessage);
			});
		}

		public void TestGetCartonGroupUsingTrolleyJobPKAndPackageID_PackageMissingFromTrolley()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Box, 4);

			var cartonGroup1 = Helper.CreateWhsCartonGroup("OWN", "OWN");
			var smallCarton = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
			cartonGroup1.CartonSizes.Add(smallCarton);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = cartonGroup1.PK;

			var relationshipOwner = data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			relationshipOwner.OU_WCG_CartonGroup = cartonGroup1.PK;

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
			var response = webService.GetCartonGroupUsingTrolleyJobPKAndPackageID(trolleyJob.PK.ToGuid(), "ABC");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Package 'ABC' was not on this Trolley.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestGetCartonGroupUsingTrolleyJobPKAndPackageID_CartonGroupNotExist

		public void TestGetCartonGroupUsingTrolleyJobPKAndPackageID_CartonGroupNotExist()
		{
			var webService = GetNewWebService();
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Box, 4);

			var whsGroup = Helper.CreateWhsCartonGroup("OWN", "OWN");
			var smallCarton = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
			var bigCarton = Helper.CreateWhsCartonSize("BIG1", 20, 20, 20, 20, 40, 80, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
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

			AssertEquals("Precondition:", "OWN - SML", splitCasePkg.CartonGroupAndSize);

			whsGroup.Delete();
			Helper.Factory.Save();

			var response1 = webService.GetCartonGroupUsingTrolleyJobPKAndPackageID(trolleyJob.PK.ToGuid(), splitCasePkg.KP_PackageID);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
			AssertEquals("Package 'O1-001' has not available Carton Groups & Sizes.", response1.ErrorMessage);

			// Supplier relationship should be ignored
			var cartonGroup2 = Helper.CreateWhsCartonGroup("SUP", "SUP");
			cartonGroup2.CartonSizes.AddRange(new[] { smallCarton, bigCarton });
			var relationshipSupplier = data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Supplier);

			var response2 = webService.GetCartonGroupUsingTrolleyJobPKAndPackageID(trolleyJob.PK.ToGuid(), splitCasePkg.KP_PackageID);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Package 'O1-001' has not available Carton Groups & Sizes.", response2.ErrorMessage);

			var newGroup = Helper.CreateWhsCartonGroup("NEW", "NEW");
			var newSize = Helper.CreateWhsCartonSize("NCS", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
			newGroup.CartonSizes.AddRange(new[] { newSize });
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = newGroup.PK;
			Helper.Factory.Save();

			var response3 = webService.GetCartonGroupUsingTrolleyJobPKAndPackageID(trolleyJob.PK.ToGuid(), splitCasePkg.KP_PackageID);
			AssertEquals(ErrorTypes.None, response3.Error);
			AssertEquals(true, string.IsNullOrEmpty(response3.ErrorMessage));

			var cartonGroups = response3.CartonGroups;
			AssertNotNull(cartonGroups);
			AssertEquals(1, cartonGroups.Count);
			AssertEquals("NEW", cartonGroups[0].Code);
			AssertEquals(1, cartonGroups[0].CartonSizes.Count);
			AssertEquals("NCS", cartonGroups[0].CartonSizes[0].Code);

			var cartonGroup3 = Helper.CreateWhsCartonGroup("BTH", "BTH");
			cartonGroup3.CartonSizes.AddRange(new[] { smallCarton, bigCarton });
			var relationshipBoth = data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Both);
			relationshipBoth.OU_WCG_CartonGroup = cartonGroup3.PK;
			var relationshipOwn = data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			relationshipOwn.OU_WCG_CartonGroup = newGroup.PK;
			Helper.Factory.Save();

			var response4 = webService.GetCartonGroupUsingTrolleyJobPKAndPackageID(trolleyJob.PK.ToGuid(), splitCasePkg.KP_PackageID);
			AssertEquals(ErrorTypes.None, response4.Error);
			AssertEquals(true, string.IsNullOrEmpty(response4.ErrorMessage));

			var cartonGroups2 = response4.CartonGroups.OrderByDescending(g => g.Code);
			AssertNotNull(cartonGroups2);
			AssertEquals(2, cartonGroups2.Count());

			AssertEquals("NEW", cartonGroups2.ElementAt(0).Code);
			AssertEquals(1, cartonGroups2.ElementAt(0).CartonSizes.Count);
			AssertEquals("NCS", cartonGroups2.ElementAt(0).CartonSizes[0].Code);
			AssertEquals("BTH", cartonGroups2.ElementAt(1).Code);
			AssertEquals(2, cartonGroups2.ElementAt(1).CartonSizes.Count);
			AssertEquals("SML", cartonGroups2.ElementAt(1).CartonSizes.OrderByDescending(g => g.Code).ElementAt(0).Code);
			AssertEquals("BIG1", cartonGroups2.ElementAt(1).CartonSizes.OrderByDescending(g => g.Code).ElementAt(1).Code);
		}

		#endregion

		#endregion
	}
}
