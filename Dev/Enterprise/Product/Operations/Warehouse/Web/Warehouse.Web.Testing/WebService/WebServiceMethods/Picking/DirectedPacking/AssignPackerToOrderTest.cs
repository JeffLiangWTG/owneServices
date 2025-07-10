using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class AssignPackerToOrderTest : WhsSecureServiceTestCase
	{
		public void TestAssignPackerToOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine1 = order.Lines.Single().PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", string.Empty, order.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", true, order.IsAttachedToPick);

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.AssignPackerToOrder(order.PK.ToGuid());
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertEquals("AAA", response.AssignedPackerCode);
			AssertEquals(false, response.IsAssignedToAnotherPacker);

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			AssertEquals("AAA", orderInNewFactory.WD_GS_NKAssignedPacker);
		}

		public void TestAssignPackerToOrder_PackerAlreadyAssignedToOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine1 = order.Lines.Single().PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			order.WD_GS_NKAssignedPacker = "AAA";
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", "AAA", order.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", true, order.IsAttachedToPick);

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.AssignPackerToOrder(order.PK.ToGuid());
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertEquals("AAA", response.AssignedPackerCode);
		}

		public void TestAssignPackerToOrder_UnpickedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Factory.Save();

			AssertEquals("Precondition", string.Empty, order.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", false, order.IsAttachedToPick);

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.AssignPackerToOrder(order.PK.ToGuid());
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Order 'O1' is not picked.", response.ErrorMessage);
			AssertNull(response.AssignedPackerCode);
		}

		public void TestAssignPackerToOrder_NotAnOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.AssignPackerToOrder(receive.PK.ToGuid());
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Order does not exist.", response.ErrorMessage);
			AssertNull(response.AssignedPackerCode);
		}

		public void TestAssignPackerToOrder_OrderDoesNotExist()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.AssignPackerToOrder(Guid.Empty);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Order does not exist.", response.ErrorMessage);
			AssertNull(response.AssignedPackerCode);
		}

		public void TestAssignPackerToOrder_OrderAssignedToAnotherPacker()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			Helper.CreateGlbStaff("BBB", "BBBBB");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine1 = order.Lines.Single().PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();
			order.WD_GS_NKAssignedPacker = "BBB";
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", "BBB", order.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", true, order.IsAttachedToPick);

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.AssignPackerToOrder(order.PK.ToGuid());
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Order 'O1' is already assigned to 'BBB'.", response.ErrorMessage);
			AssertNull(response.AssignedPackerCode);
		}

		public void TestAssignPackerToOrder_ConcurrencyError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			var otherUser = Helper.CreateGlbStaff("BBB", "BBBBB");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine1 = order.Lines.Single().PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", string.Empty, order.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", true, order.IsAttachedToPick);

			var webService = GetNewWebService(data.Whs1, staff: user);
			webService.Factory.Saving += delegate
			{
				var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
				orderInOtherFactory.WD_GS_NKAssignedPacker = otherUser.GS_Code;
				otherFactory.Save();
			};

			var response = webService.AssignPackerToOrder(order.PK.ToGuid());
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Another user has taken the order.", response.ErrorMessage);
			Assert(response.IsAssignedToAnotherPacker);
			AssertNull(response.AssignedPackerCode);
		}

		public void TestAssignPackerToOrder_OrderHasNothingToPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine1 = order.Lines.Single().PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", "", order.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", 1, order.PackageJob.Packages.Count);
			AssertEquals("Precondition", true, order.IsAttachedToPick);

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.AssignPackerToOrder(order.PK.ToGuid());
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Order 'O1' has nothing to pack.", response.ErrorMessage);
			AssertNull(response.AssignedPackerCode);
		}

		public void TestGetOrdersReadyToPackInPackingLocation_OrderNotReadyToPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = orderline1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderline2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;

			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.InTransit, transferLine2.WE_CurrentInventoryStatus);
			AssertNotEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);
			AssertEquals("Precondition", "", order.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", true, order.IsAttachedToPick);
			AssertEquals("Precondition", 0, order.PackageJob.Packages.Count);
			AssertEquals("Precondition", packingLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", packingLocation.PK, transferLine2.WE_WL);

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.AssignPackerToOrder(order.PK.ToGuid());
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Order 'O1' is not ready to pack.", response.ErrorMessage);
			AssertNull(response.AssignedPackerCode);
		}

		public void TestAssignPackerToOrder_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			var numberOfOrderLines = 10;
			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < numberOfOrderLines; i++)
			{
				var product = Helper.CreateProduct(data.Org1, $"P1{i}");
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}", product, 10m);
				products.Add(product);
			}
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			for (var i = 0; i < numberOfOrderLines; i++)
			{
				Helper.CreateWhsOrderLine(order, products[i], 10m);
			}
			Helper.CreatePickNew(order);

			order.Lines.ForEach(line =>
			{
				var pickLine = line.PickLines.Single();
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				transferLine.WE_WL = packingLocation.PK;
				transferLine.FinaliseDocketLine();
			});

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			for (var i = 5; i < numberOfOrderLines; i++)
			{
				var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
				package.Pack(order.Lines[i].ReleaseLines[0], 10m);
			}
			Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff: user);
			var expectedDBHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsOrderStatusViewSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.AssignPackerToOrder(order.PK.ToGuid());
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
				AssertEquals("AAA", response.AssignedPackerCode);

				var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
				AssertEquals("AAA", orderInNewFactory.WD_GS_NKAssignedPacker);
			}
		}

		#region Implementation

		BusinessObjectFactory Factory => Helper.Factory;

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		#endregion
	}
}
