using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class AddSlotToTrolleyTest : WhsSecureServiceTestCase
	{
		#region TestAddSlotToTrolleyUsingToteID

		#region TestAddSlotToTrolleyUsingToteID

		public void TestAddSlotToTrolleyUsingToteID()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order).WP_CartoniseSplitCases = false;
			orderLine.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var saveCount = 0;
			var webService = GetNewWebService(data.Whs1);
			webService.Factory.Saving += _ => saveCount++;

			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors. Should have packed the pickline.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var slot = trolleyJob.Slots.Single();
			AssertEquals("A single Slot #4 should be added to the Trolley.", 4, (int)slot.WTS_SlotNumber);

			var slotPackage = slot.Package;
			var orderPackage = order.PackageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("The Package in the Slot should be the same as that on Order.", slotPackage, orderPackage);
				AssertTotePackage(slotPackage, "PKG1", orderLine.PickLines);
			});

			AssertEquals("Should have performed a single factory save.", 1, saveCount);
		}

		public void TestAddSlotToTrolleyUsingToteID_DockDoorAssignment()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Tote, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			order1.WD_PickPriority = 1;

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			order2.WD_PickPriority = 2;

			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_CartoniseSplitCases = false;
			orderLine1.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_CartoniseSplitCases = false;
			orderLine2.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg_OnTrolley1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, Constants.PkgUnit.Box);
			pkg_OnTrolley1.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley1, orderLine1.PickLines.Single());

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley1, 1);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo(), "O2");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors. Should have packed the pickline.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error message.", true, string.IsNullOrEmpty(response.ErrorMessage));

				AssertEquals("A slot must be added.", 2, trolleyJob.Slots.Count);

				trolleyJob.Slots.Single(t => t.WTS_SlotNumber == 1);
				var addedSlot = trolleyJob.Slots.Single(t => t.WTS_SlotNumber == (short)4);
				var slotPackage = addedSlot.Package;
				var orderPackage = order2.PackageJob.Packages.Single();

				AssertEquals("The Package in the Slot should be the same as that on Order.", slotPackage, orderPackage);
				AssertTotePackage(slotPackage, "PKG1", order2.Lines[0].PickLines);

				var dockDoorAssignments = webService.Factory.Load<WhsDockDoorAssignment>(new ZQuery());
				AssertEquals("Only 1 DDA created", 1, dockDoorAssignments.Length);

				var dda = dockDoorAssignments[0];
				AssertEquals("DDA correct DDL", dda.WDA_WL_AssignedDockDoor, data.Whs1.WW_DefaultOutboundDockDoor);
				AssertEquals("DDA correct PutawayTime", ZDateTime.Empty, dda.WDA_FirstPutawayToDockDoorUtc);

				AssertEquals("Pick1 DDA set", dda.PK, pick1.WP_WDA_DockDoorAssignment);
				AssertEquals("Pick1 DDL cleared", ZGuid.Empty, pick1.WP_WL_DockDoor);

				AssertEquals("Pick2 DDA set", dda.PK, pick2.WP_WDA_DockDoorAssignment);
				AssertEquals("Pick2 DDL cleared", ZGuid.Empty, pick2.WP_WL_DockDoor);
			});
		}

		public void TestAddSlotToTrolleyUsingToteID_WeightAndVolumeOnOrderIsCorrect()
		{
			var data = SetupDataForTote();
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order).WP_CartoniseSplitCases = false;
			orderLine.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();
			AssertEquals("Precondition", false, order.UsePackingWeightAndVolume);
			AssertEquals("Precondition", 0, order.WD_PackagesSent);
			AssertEquals("Precondition", 20m, order.WD_WeightSent);
			AssertEquals("Precondition", 5m, order.WD_CubicSent);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors. Should have packed the pickline.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var slot = trolleyJob.Slots.Single();
			AssertEquals("A single Slot #4 should be added to the Trolley.", 4, (int)slot.WTS_SlotNumber);

			var slotPackage = slot.Package;
			var orderPackage = order.PackageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("The Package in the Slot should be the same as that on Order.", slotPackage, orderPackage);
				AssertTotePackage(slotPackage, "PKG1", orderLine.PickLines);
			});

			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			AssertEquals("Use Packing Weight and Volume should be defaulted to true.", true, orderInOtherFactory.UsePackingWeightAndVolume);
			AssertEquals("Packages Sent should be updated.", 1, orderInOtherFactory.WD_PackagesSent);
			AssertEquals("Weight Sent should not be changed (Packed Weight should be identical to Unpacked Weight previously).", 20m, orderInOtherFactory.WD_WeightSent);
			AssertEquals("Volume Sent should not be Updated (Tote has no Volume set).", 5m, orderInOtherFactory.WD_CubicSent);
		}

		public void TestAddSlotToTrolleyUsingToteID_WithOrderID()
		{
			var data = SetupDataForTote();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order).WP_CartoniseSplitCases = false;
			orderLine.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo(), "O2");
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals("Response should be in error as order does not exist.", ErrorTypes.BusinessValidationError, response1.Error);
				AssertEquals("Response message should explain that there are no valid orders to assign.", "O2 is not a valid order to assign.", response1.ErrorMessage);
			});

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo(), "O1");
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors. Should have packed the pickline.", ErrorTypes.None, response2.Error);
				AssertEquals("Should have no error message.", true, string.IsNullOrEmpty(response2.ErrorMessage));
			});

			var slot = trolleyJob.Slots.Single();
			AssertEquals("A single Slot #4 should be added to the Trolley.", 4, (int)slot.WTS_SlotNumber);

			var slotPackage = slot.Package;
			var orderPackage = order.PackageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("The Package in the Slot should be the same as that on Order.", slotPackage, orderPackage);
				AssertTotePackage(slotPackage, "PKG1", orderLine.PickLines);
			});
		}

		public void TestAddSlotToTrolleyUsingToteID_WithOrderID_MultipleOrdersOnPick()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Tote, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			order1.WD_PickPriority = 1;

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			order2.WD_PickPriority = 2;

			Helper.CreatePickNew(order1, order2).WP_CartoniseSplitCases = false;
			orderLine1.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			orderLine2.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo(), "O2");
			AssertEquals("Should have no errors. Should have packed the pickline.", ErrorTypes.None, response1.Error);
			AssertEquals("Should have no error message.", true, string.IsNullOrEmpty(response1.ErrorMessage));

			var slot = trolleyJob.Slots.Single();
			AssertEquals("A single Slot #4 should be added to the Trolley.", 4, (int)slot.WTS_SlotNumber);

			var slotPackage = slot.Package;
			var orderPackage = order2.PackageJob.Packages.Single();
			AssertEquals("The Package in the Slot should be the same as that on Order.", slotPackage, orderPackage);
			AssertTotePackage(slotPackage, "PKG1", orderLine2.PickLines);

			AssertEquals("There should be no packages in order1.", false, order1.PackageJob.Packages.Any());
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_DifferentDDL

		public void TestAddSlotToTrolleyUsingToteID_DifferentDDL()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var staff = Helper.CreateGlbStaff("AAA", "A.A");

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var otherDockDoorLocation = data.Whs1.FindLocation("A-2");
			otherDockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Helper.Factory.Save(); // to create stock

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);
			order1.WD_PickPriority = 1;
			order2.WD_PickPriority = 2;
			order3.WD_PickPriority = 3;
			pick2.WP_WL_DockDoor = otherDockDoorLocation.PK;
			Helper.Factory.Save();

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 1, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors.", ErrorTypes.None, response1.Error);
				AssertTotePackage(trolleyJob.Slots.First().Package, "PKG1", order1.Lines[0].PickLines);
			});

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG2", 2, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors.", ErrorTypes.None, response2.Error);
				AssertTotePackage(trolleyJob.Slots.Single(s => s.WTS_SlotNumber == 2).Package, "PKG2", order3.Lines[0].PickLines);
			});

			var webService3 = GetNewWebService(data.Whs1, staff);
			var response3 = webService3.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG3", 3, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response3, webService3);

			CombineAssertions(() =>
			{
				AssertEquals("Response should be in error as the pick has another dock door.", ErrorTypes.BusinessValidationError, response3.Error);
				AssertEquals("Response messaage should explain that there are no valid orders to assign.", "No valid orders to assign.", response3.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_Multiple

		public void TestAddSlotToTrolleyUsingToteID_Multiple()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order).WP_CartoniseSplitCases = false;
			orderLine.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			orderLine2.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors. Should have packed the pickline.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var slot = trolleyJob.Slots.Single();
			AssertEquals("A single Slot #4 should be added to the Trolley.", 4, (int)slot.WTS_SlotNumber);

			var slotPackage = slot.Package;
			var orderPackage = order.PackageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("The Package in the Slot should be the same as that on Order.", slotPackage, orderPackage);
				AssertTotePackage(slotPackage, "PKG1", new[] { orderLine.PickLines.Single(), orderLine2.PickLines.Single() });
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_NonUniquePackageID_DbHits

		public void TestAddSlotToTrolleyUsingToteID_DbHits() => TestAddSlotToTrolleyUsingToteID_DbHits(false);
		public void TestAddSlotToTrolleyUsingToteID_DbHits_WithExistingSlots() => TestAddSlotToTrolleyUsingToteID_DbHits(true);

		void TestAddSlotToTrolleyUsingToteID_DbHits(bool withExistingSlots)
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 60m);
			Helper.Factory.Save(); // to create stock

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var order1Line3 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var order1Line4 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var order1Line5 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);

			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_CartoniseSplitCases = false;

			order1Line1.PickLines.Single().WZ_F3_NKAllocatedPackType = "ABC";
			order1Line2.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Bag;
			order1Line3.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Bottle;
			order1Line4.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.BreakBulk;
			order1Line5.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);

			if (withExistingSlots)
			{
				for (var i = 1; i < 6; i++)
				{
					var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + (2 + i), data.Part1, 1m);
					var pick2 = Helper.CreatePickNew(order2);
					pick2.WP_CartoniseSplitCases = false;

					var pickLine = order2.Lines[0].PickLines.Single();
					pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

					var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order2);
					var packageABC = packingHelper.CreatePackage(pkgJob, "DEF", 1, Constants.PkgUnit.Box);
					packageABC.SetIsTote(true);

					packingHelper.CreatePackageDivot(packageABC, pickLine);

					var slot = trolleyJob.Slots.AddNew();
					slot.WTS_SlotNumber = (short)i;
					slot.WTS_KP_Package = packageABC.PK;
				}
			}

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var webServiceFactory = webService.Factory;

			var expectedDBHits = new Dictionary<string, int>
			{
				{ nameof(OrgHeader), 1 },
				{ nameof(JobDocAddress), 1 },
				{ nameof(OrgAddress), 3 },
				{ nameof(OrgMiscServ), 1 },
				{ nameof(OrgSupplierPart), 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ nameof(ProcessTasks), withExistingSlots ? 3 : 2 },
				{ nameof(ProcessTaskTemplate), 2 },
				{ nameof(RefEquipment), 1 },
				{ nameof(RefPackType), 2 },
				{ nameof(StmEvent), 1 },
				{ nameof(GenAddOnColumn), withExistingSlots ? 1 : 0 },
				{ nameof(GlbBranch), 1 },
				{ nameof(GlbStaff), 1 },
				{ nameof(PkgPackage), withExistingSlots ? 3 : 1 },
				{ nameof(PkgPackageItemDivot), withExistingSlots ? 2 : 1 },
				{ nameof(PkgPackageJob), withExistingSlots ? 2 : 1 },
				{ nameof(WhsDocket), withExistingSlots ? 7 : 3 }, // 1 for DDL, 1 from pick lines loader, 1 for release lines, 2x for pick saving FTZ permit check, 1 for grouping key creation, 1 for checking pick by BOM Pick line, 1 for process task fetch hint, 1 for generate DDA
				{ nameof(WhsPick), withExistingSlots ? 3 : 2 },
				{ nameof(WhsPickLine), 1 },
				{ nameof(WhsDocketLine), 2 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ nameof(WhsPickTrolleyJob), 1 },
				{ nameof(WhsPickTrolleySlot), withExistingSlots ? 3 : 2 },
				{ nameof(WhsWarehouse), 1 },
				{ nameof(WhsDockDoorAssignment), withExistingSlots ?  1 : 0 },
			};

			PackageWebServiceResponse response;
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webServiceFactory))
			using (RowFactory.SetCachedTables())
			{
				response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "AAA", 8, new SearchFilterCriteriaInfo());
			}

			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors.", ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_Mix

		public void TestAddSlotToTrolleyUsingToteID_Mix()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var invalidOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var invalidOrderLine = Helper.CreateWhsOrderLine(invalidOrder, data.Part1, 1m);
			var invalidOrderPick = Helper.CreatePickNew(invalidOrder);
			invalidOrderPick.WP_CartoniseSplitCases = false;
			invalidOrderLine.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Box;

			var validOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var validOrderLine1 = Helper.CreateWhsOrderLine(validOrder, data.Part1, 1m);
			var validOrderLine2 = Helper.CreateWhsOrderLine(validOrder, data.Part1, 2m);
			var nonSplitCaseLine = Helper.CreateWhsOrderLine(validOrder, data.Part1, 1m);
			var assignedLine = Helper.CreateWhsOrderLine(validOrder, data.Part1, 1m);
			var packedLine = Helper.CreateWhsOrderLine(validOrder, data.Part1, 1m);

			var validOrderPick = Helper.CreatePickNew(validOrder);
			validOrderPick.WP_CartoniseSplitCases = false;

			validOrderLine1.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			validOrderLine2.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			nonSplitCaseLine.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Box;
			assignedLine.PickLines.Single().WZ_GS_NKAssignedTo = "AAA";
			assignedLine.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			packedLine.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(validOrder);
			var packageAAA = packingHelper.CreatePackage(pkgJob, "PAAA", 1, Constants.PkgUnit.Tote);
			packingHelper.CreatePackageDivot(packageAAA, packedLine.PickLines.Single());

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors. Should have packed the valid picklines.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var slot = trolleyJob.Slots.Single();
			AssertEquals("A single Slot #4 should be added to the Trolley.", 4, (int)slot.WTS_SlotNumber);

			var slotPackage = slot.Package;
			var orderPackage = validOrder.PackageJob.Packages.Single(p => p.KP_PackageID != "PAAA");
			CombineAssertions(() =>
			{
				AssertEquals("The Package in the Slot should be the same as that on Order.", slotPackage, orderPackage);
				AssertTotePackage(slotPackage, "PKG1", new[] { validOrderLine1.PickLines.Single(), validOrderLine2.PickLines.Single() });
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_WithPickLinesCommittingDockDoorStock

		public void TestAddSlotToTrolleyUsingToteID_WithPickLinesCommittingDockDoorStock()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order);
			orderLine1.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			orderLine2.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			Helper.PickAndMakeInTransitTransfer(orderLine1.PickLines.Single(), ZDateTimeOffset.Now);
			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 1, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors. Should have packed the pickline.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var slot = trolleyJob.Slots.Single();
			AssertEquals("A single Slot #1 should be added to the Trolley.", 1, (int)slot.WTS_SlotNumber);

			var slotPackage = slot.Package;
			var orderPackage = order.PackageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("The Package in the Slot should be the same as that on Order.", slotPackage, orderPackage);
				AssertTotePackage(slotPackage, "PKG1", orderLine2.PickLines); // Should not add orderLine1
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_Error_NoPick

		public void TestAddSlotToTrolleyUsingToteID_Error_NoPick()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Response should be in error as the Order is not CartoniseSplitCases.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Response messaage should explain that there are no valid orders to assign.", "No valid orders to assign.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_Error_TrolleyDoesNotExist

		public void TestAddSlotToTrolleyUsingToteID_Error_TrolleyDoesNotExist()
		{
			var webService = GetNewWebService();
			var response = webService.AddSlotToTrolleyUsingToteID(Guid.NewGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Response should be in error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Trolley job was not found. Please start building trolley again.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_Error_IsCartoniseSplitCases

		public void TestAddSlotToTrolleyUsingToteID_Error_IsCartoniseSplitCases()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Response should be in error as the Order is not CartoniseSplitCases.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Response messaage should explain that there are no valid orders to assign.", "No valid orders to assign.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_Error_WrongTrolley

		public void TestAddSlotToTrolleyUsingToteID_Error_WrongTrolley()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var trolley = Helper.CreateTrolley("T1");
			var pickingTrolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			var finalisedTrolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Finalised);
			finalisedTrolleyJob.WTJ_FinalisedDateUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var responseForPickingTrolley = webService1.AddSlotToTrolleyUsingToteID(pickingTrolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(responseForPickingTrolley, webService1);

			CombineAssertions(() =>
			{
				AssertEquals("Response should be in error as the Trolley is in Picking state.", ErrorTypes.BusinessValidationError, responseForPickingTrolley.Error);
				AssertEquals("Response messaage should explain that the Trolley is in Picking state.", "This trolley job is in PIC state. No slots can be added to it.", responseForPickingTrolley.ErrorMessage);
			});

			var webService2 = GetNewWebService(data.Whs1);
			var responseForFinalisedTrolley = webService2.AddSlotToTrolleyUsingToteID(finalisedTrolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(responseForFinalisedTrolley, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Response should be in error as the Trolley is in Picking state.", ErrorTypes.BusinessValidationError, responseForFinalisedTrolley.Error);
				AssertEquals("Response messaage should explain that the Trolley is in Picking state.", "This trolley job is in FIN state. No slots can be added to it.", responseForFinalisedTrolley.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_Error_AllocatedPackType_NotSplitCase

		public void TestAddSlotToTrolleyUsingToteID_Error_AllocatedPackType_NotSplitCase()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = false;
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Box;

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Response should be in error as the Pickline is not SplitCase UOM.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Response messaage should explain that there are no valid orders to assign.", "No valid orders to assign.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_Error_PickerAlreadyAssigned

		public void TestAddSlotToTrolleyUsingToteID_Error_PickerAlreadyAssigned()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = false;
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			pickLine.WZ_GS_NKAssignedTo = "AAA";

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Response should be in error as the Pickline is already assigned a Picker.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Response messaage should explain that there are no valid orders to assign.", "No valid orders to assign.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_Error_AlreadyPackedIntoTote

		public void TestAddSlotToTrolleyUsingToteID_Error_AlreadyPackedIntoTote()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var line_PackedIntoCarton = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = false;
			var pickLine = line_PackedIntoCarton.PickLines.Single();
			pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var totePackage = packingHelper.CreatePackage(packageJob, "P1", 1, Constants.PkgUnit.Tote);
			packingHelper.CreatePackageDivot(totePackage, pickLine);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Order Pickline is already packed into a Carton.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Order Pickline is already packed into a Carton.", "No valid orders to assign.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_Error_NonTotePackageWithIDAlreadyExists

		public void TestAddSlotToTrolleyUsingToteID_Error_NonTotePackageWithIDAlreadyExists()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var line_PackedIntoCarton = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = false;
			var pickLine = line_PackedIntoCarton.PickLines.Single();
			pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var box = packingHelper.CreatePackage(packageJob, "PKG1", 1, Constants.PkgUnit.Box);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Order already has a Package with the same ID, so should error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error message should explain that there is a non Tote Package already on the order with the same ID.", "A non Tote Package was found using Package ID 'PKG1' already.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_TotePackageWithIDAlreadyExists

		public void TestAddSlotToTrolleyUsingToteID_TotePackageWithIDAlreadyExists()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order).WP_CartoniseSplitCases = false;
			orderLine.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var tote = packingHelper.CreatePackage(packageJob, "PKG1", 1, Constants.PkgUnit.Tote);
			tote.SetIsTote(true);
			tote.KP_GoodsDescription = "Warehouse Tote";
			AssertEquals("The Tote Package should not have any Packed Item Divots yet.", false, tote.PackedItemDivots.Any());

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors. Should have packed the pickline.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var slot = trolleyJob.Slots.Single();
			AssertEquals("A single Slot #4 should be added to the Trolley.", 4, (int)slot.WTS_SlotNumber);

			var slotPackage = slot.Package;
			var orderPackage = order.PackageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("The Package in the Slot should be the same as that on Order.", slotPackage, orderPackage);
				AssertTotePackage(slotPackage, "PKG1", orderLine.PickLines);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_TotePackageWithIDAlreadyExists_PackTypeWithNoRefPackType

		public void TestAddSlotToTrolleyUsingToteID_TotePackageWithIDAlreadyExists_PackTypeWithNoRefPackType()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order).WP_CartoniseSplitCases = false;
			orderLine1.PickLines.Single().WZ_F3_NKAllocatedPackType = "AAA";
			orderLine2.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var tote = packingHelper.CreatePackage(packageJob, "PKG1", 1, Constants.PkgUnit.Tote);
			tote.SetIsTote(true);
			tote.KP_GoodsDescription = "Warehouse Tote";
			AssertEquals("The Tote Package should not have any Packed Item Divots yet.", false, tote.PackedItemDivots.Any());

			packingHelper.CreatePackageDivot(tote, orderLine1.PickLines.Single());

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors. Should have packed the pickline.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var slot = trolleyJob.Slots.Single();
			AssertEquals("A single Slot #4 should be added to the Trolley.", 4, (int)slot.WTS_SlotNumber);

			var slotPackage = slot.Package;
			var orderPackage = order.PackageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("The Package in the Slot should be the same as that on Order.", slotPackage, orderPackage);
				AssertTotePackage(slotPackage, "PKG1", orderLine1.PickLines.Concat(orderLine2.PickLines));
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_Error_ToteAlreadyUsedOnThisOrder

		public void TestAddSlotToTrolleyUsingToteID_Error_ToteAlreadyUsedOnThisOrder()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = false;
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var tote = packingHelper.CreatePackage(packageJob, "PKG1", 1, Constants.PkgUnit.Tote);
			tote.SetIsTote(true);
			tote.KP_GoodsDescription = "Warehouse Tote";

			var firstPickLine = pick.GetAllPickLines().First();
			firstPickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			packingHelper.CreatePackageDivot(tote, firstPickLine);

			var trolley = Helper.CreateTrolley("T1");
			var finalisedTrolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Finalised);
			finalisedTrolleyJob.WTJ_FinalisedDateUtc = ZDateTime.Now;
			var slot = finalisedTrolleyJob.Slots.AddNew();
			slot.WTS_SlotNumber = 1;
			slot.WTS_KP_Package = tote.PK;

			var newTrolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(newTrolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should have an error message.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Should an error message.", "Tote 'PKG1' is already used on this order, select another Tote or first Pack the existing contents into a Package.", response.ErrorMessage);
				AssertEquals("Should not have created a new slot.", 0, newTrolleyJob.Slots.Count);
				AssertTotePackage(tote, "PKG1", new[] { firstPickLine }); // Should not have repacked the old tote
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_Error_AlreadyPackedIntoCarton

		public void TestAddSlotToTrolleyUsingToteID_Error_AlreadyPackedIntoCarton()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var line_PackedIntoCarton = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = false;
			var pickLine = line_PackedIntoCarton.PickLines.Single();
			pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var cartonPackage = packingHelper.CreatePackage(packageJob, "P1", 1, Constants.PkgUnit.Carton);
			packingHelper.CreatePackageDivot(cartonPackage, pickLine);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Order Pickline is already packed into a Carton.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Order Pickline is already packed into a Carton.", "No valid orders to assign.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_Error_CartonTrolley

		public void TestAddSlotToTrolleyUsingToteID_Error_CartonTrolley()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var otherPackageJob = PkgPackageJob.LoadOrCreatePackageJob(Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2"));
			var cartonPackage = packingHelper.CreatePackage(otherPackageJob, "C1", 1, Constants.PkgUnit.Carton);

			var cartonTrolley = Helper.CreateTrolley("T1");
			var cartonTrolleyJob = Helper.CreateWhsPickTrolleyJob(cartonTrolley, PickTrolleyStatus.Codes.Building);
			var slot = cartonTrolleyJob.Slots.AddNew();
			slot.WTS_SlotNumber = 1;
			slot.WTS_KP_Package = cartonPackage.PK;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(cartonTrolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Response should be in error asCannot add Tote to Carton trolley.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Response messaage should explain that you cannot add Tote to Carton trolley.", "Cannot add 'Tote' to Trolley of type 'Carton'", response.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_Error_SlotAlreadyHasATote

		public void TestAddSlotToTrolleyUsingToteID_Error_SlotAlreadyHasATote()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var otherPackageJob = PkgPackageJob.LoadOrCreatePackageJob(Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2"));
			var totePackage = packingHelper.CreatePackage(otherPackageJob, "TP1", 1, Constants.PkgUnit.Tote);
			totePackage.SetIsTote(true);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			var slot = trolleyJob.Slots.AddNew();
			slot.WTS_SlotNumber = 4;
			slot.WTS_KP_Package = totePackage.PK;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Response should be in error asCannot add Tote to Carton trolley.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Response messaage should explain that you cannot add Tote to Carton trolley.", "Slot 4 on trolley 'T1' already filled with package 'TP1'. Please choose another slot.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_ClearReleaseCapturedAttributesOnPacking

		public void TestAddSlotToTrolleyUsingToteID_ClearReleaseCapturedAttributesOnPacking()
		{
			var data = SetupDataForTote();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = false;
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "GREEN";
			releaseLine1.Quantity = 1m;

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors. Should have packed the pickline.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var slot = trolleyJob.Slots.Single();
			AssertEquals("A single Slot #4 should be added to the Trolley.", 4, (int)slot.WTS_SlotNumber);

			var slotPackage = slot.Package;
			var orderPackage = order.PackageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("The Package in the Slot should be the same as that on Order.", slotPackage, orderPackage);
				AssertTotePackage(slotPackage, "PKG1", orderLine.PickLines);
				AssertEquals("Manually entered Release Captured Attrbiutes should be cleared.", true, orderLine.PickLines.All(pl => !pl.HasReleaseCapturedAttribs));
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_PickByBOMKitLines

		public void TestAddSlotToTrolleyUsingToteID_PickByBOMKitLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			packingHelper.SetRefPackTypeUOM(PkgUnit.Carton, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(PkgUnit.Tote, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(PkgUnit.Box, UOMPackTypesList.Codes.Case);

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, bike, 4m);
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 6m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 10m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItemsWithMock();
			pick.WP_CartoniseSplitCases = false;
			var wheelOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines.Single();
			wheelOrderLine.WZ_F3_NKAllocatedPackType = PkgUnit.Tote;
			orderLine.PickLines.ForEach(pl => pl.WZ_F3_NKAllocatedPackType = PkgUnit.Tote);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);
			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var newFactory = new BusinessObjectFactory();
			trolleyJob = newFactory.Load<WhsPickTrolleyJob>(trolleyJob.PK);
			AssertEquals(1, trolleyJob.Slots.Count);

			order = newFactory.Load<WhsOrder>(order.PK);
			var packages = order.PackageJob.Packages;
			AssertEquals(1, packages.Count);

			AssertEquals((short)4, trolleyJob.Slots[0].WTS_SlotNumber);
			AssertEquals(packages[0].PK, trolleyJob.Slots[0].WTS_KP_Package);
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_AllReleaseLinesWithReleaseCapturedAttributes

		public void TestAddSlotToTrolleyUsingToteID_AllReleaseLinesWithReleaseCapturedAttributes()
		{
			var data = SetupDataForTote();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = false;
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "GREEN";
			releaseLine1.Quantity = 3m;

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			var releaseLinesInNewFactory = orderInNewFactory.Lines[0].ReleaseLines;
			AssertEquals(2, releaseLinesInNewFactory.Count);
			releaseLinesInNewFactory[1].PartAttribute1 = "RED";
			newFactory.Save();

			AssertEquals(3m, releaseLinesInNewFactory[0].Quantity);
			AssertEquals(7m, releaseLinesInNewFactory[1].Quantity);

			AssertEquals("GREEN", releaseLinesInNewFactory[0].PartAttribute1);
			AssertEquals("RED", releaseLinesInNewFactory[1].PartAttribute1);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors. Should have packed the pickline.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var slot = trolleyJob.Slots.Single();
			AssertEquals("A single Slot #4 should be added to the Trolley.", 4, (int)slot.WTS_SlotNumber);

			var slotPackage = slot.Package;
			var orderPackage = order.PackageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("The Package in the Slot should be the same as that on Order.", slotPackage, orderPackage);
				AssertTotePackage(slotPackage, "PKG1", orderLine.PickLines);
			});
		}

		public void TestAddSlotToTrolleyUsingToteID_ReservedPickLineWithZeroUnitsAndDifferentGroupingKey()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 4, 1);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Tote, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);

			Helper.Factory.Save();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var today = ZDate.Today;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.FindLocation("A-1"), today, today, "", "", "", "");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, data.Whs1.FindLocation("A-2"), today, today, "", "", "", "");
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3", Notify);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 10m, data.Whs1.FindLocation("A-3"), today, today, "", "", "", "");
			receive3.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive3);
			var tomorrow = today.AddDays(1);
			var receive4 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R4", Notify);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive4, data.Part1, 20m, data.Whs1.FindLocation("A-4"), tomorrow, tomorrow, "", "", "", "");
			receive4.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive4);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine1 = orderLine1.ReserveStockIfAbleTo(inventory1);
			var reservedPickLine2 = orderLine2.ReserveStockIfAbleTo(inventory2);
			var reservedPickLine5 = orderLine3.ReserveStockIfAbleTo(inventory3);
			Helper.Factory.Save();
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories[0];
			orderedInventory.AvailableInventories[0].Allocate = false;
			orderedInventory.AvailableInventories[1].Allocate = false;
			orderedInventory.AvailableInventories[3].Allocate = true;

			pick.WP_CartoniseSplitCases = false;
			orderLine1.PickLines.ForEach(pl => pl.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote);
			orderLine2.PickLines.ForEach(pl => pl.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote);
			orderLine3.PickLines.ForEach(pl => pl.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote);
			Helper.Factory.Save();
			AssertEquals("Precondition - There should be a pick line with Expiry Date", today, orderLine1.PickLines[0].InventoryLine.WE_ExpiryDate);
			AssertEquals("Precondition - There should be a pick line with Packing Date", today, orderLine2.PickLines[0].InventoryLine.WE_PackingDate);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo(), "O1");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error message.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_DisallowPicksWithInventoriesAtDockDoor

		public void TestAddSlotToTrolleyUsingToteID_DisallowPicksWithInventoriesAtDockDoor()
			=> TestAddSlotToTrolleyUsingToteID_DisallowPicksWithInventoriesAtDockDoorCore(hasPackingStations: true);

		public void TestAddSlotToTrolleyUsingToteID_DisallowPicksWithInventoriesAtDockDoor_NoPackingStations()
			=> TestAddSlotToTrolleyUsingToteID_DisallowPicksWithInventoriesAtDockDoorCore(hasPackingStations: false);

		void TestAddSlotToTrolleyUsingToteID_DisallowPicksWithInventoriesAtDockDoorCore(bool hasPackingStations)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Tote, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			if (hasPackingStations)
			{
				var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
				var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
				packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			}
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 15m);
			Helper.CreatePickNew(order1, order2).WP_CartoniseSplitCases = false;
			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;

			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, pickLine1);

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package.PK);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var putawayResponse = webService1.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, putawayResponse.Error);
			AssertNull("Should be no error.", putawayResponse.ErrorMessage);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response = webService2.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "TOTE", 4, new SearchFilterCriteriaInfo());
			if (hasPackingStations)
			{
				AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Should be error.", "No valid orders to assign.", response.ErrorMessage);
			}
			else
			{
				AssertSuccessfulResponseWithNoErrors(response, webService2);
				var slot = trolleyJob.Slots.Single();
				AssertEquals("A single Slot #4 should be added to the Trolley.", 4, (int)slot.WTS_SlotNumber);

				var slotPackage = slot.Package;
				var orderPackage = order2.PackageJob.Packages.Single();
				CombineAssertions(() =>
				{
					AssertEquals("The Package in the Slot should be the same as that on Order.", slotPackage, orderPackage);
					AssertTotePackage(slotPackage, "TOTE", orderLine2.PickLines);
				});
			}
		}

		public void TestAddSlotToTrolleyUsingToteID_DisallowPicksWithInventoriesAtDockDoor_WithOrderId()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Tote, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 15m);
			Helper.CreatePickNew(order1, order2).WP_CartoniseSplitCases = false;
			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;

			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, pickLine1);

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package.PK);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var putawayResponse = webService1.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, putawayResponse.Error);
			AssertNull("Should be no error.", putawayResponse.ErrorMessage);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response = webService2.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "TOTE", 4, new SearchFilterCriteriaInfo(), "O2");
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "O2 is not a valid order to assign.", response.ErrorMessage);
		}

		public void TestAddSlotToTrolleyUsingToteID_DisallowPicksWithInventoriesAtDockDoor_NoInventoriesAtDockDoor()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Tote, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 15m);
			Helper.CreatePickNew(order1, order2).WP_CartoniseSplitCases = false;
			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			Helper.Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "TOTE", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponseWithNoErrors(response, webService);
		}

		public void TestAddSlotToTrolleyUsingToteID_DisallowPicksWithInventoriesAtDockDoor_WithOtherOrders()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Tote, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive2.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			receive2.FinaliseDocket();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 15m);
			Helper.CreatePickNew(order1, order2).WP_CartoniseSplitCases = false;
			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;

			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, pickLine1);

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package.PK);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var putawayResponse = webService1.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, putawayResponse.Error);
			AssertNull("Should be no error.", putawayResponse.ErrorMessage);

			var orderOnAnotherPick = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(orderOnAnotherPick, data.Part1, 10m);
			Helper.CreatePickNew(orderOnAnotherPick).WP_CartoniseSplitCases = false;

			var pickLine3 = orderLine3.PickLines.Single();
			pickLine3.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			Helper.Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response = webService2.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "TOTE", 4, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponseWithNoErrors(response, webService2);

			var slot = trolleyJob.Slots.Single();
			AssertEquals("A single Slot #4 should be added to the Trolley.", 4, (int)slot.WTS_SlotNumber);

			var slotPackage = slot.Package;
			var orderPackage = orderOnAnotherPick.PackageJob.Packages.Single();
			CombineAssertions(() =>
			{
				AssertEquals("The Package in the Slot should be the same as that on Order.", slotPackage, orderPackage);
				AssertTotePackage(slotPackage, "TOTE", orderLine3.PickLines);
			});
		}

		#endregion

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID

		#region TestAddSlotToTrolleyUsingPackageID

		public void TestAddSlotToTrolleyUsingPackageID()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save(); // to create stock
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			AssertEquals("Precondition:", 10m, order.Lines[0].PickLineQuantity);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageAAA = packingHelper.CreatePackage(pkgJob, "AAA", 1, Constants.PkgUnit.Box); // some other package to make sure that correct one will be used
			var packageABC = packingHelper.CreatePackage(pkgJob, "ABC", 1, Constants.PkgUnit.Box);
			var picklines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(packageABC, picklines[0]);
			packingHelper.CreatePackageDivot(packageABC, picklines[1]);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			AssertEquals("Precondition:", false, trolleyJob.Slots.Any());

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "ABC", 4);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
				AssertEquals("A slot must be added.", 1, trolleyJob.Slots.Count);
				AssertEquals("Slot assignment must be saved to DB.", true, trolleyJob.Slots[0].IsInDatabase);
				AssertEquals("Slot number must be populated.", (short)4, trolleyJob.Slots[0].WTS_SlotNumber);
				AssertEquals("Slot must be linked to correct package.", packageABC.PK, trolleyJob.Slots[0].WTS_KP_Package);
			});

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 1, new SearchFilterCriteriaInfo { });
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("cannot add Tote to Carton trolley", ErrorTypes.BusinessValidationError, response2.Error);
				AssertEquals("cannot add Tote to Carton trolley", "Cannot add 'Tote' to Trolley of type 'Carton'", response2.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID_DockDoorAssignment

		public void TestAddSlotToTrolleyUsingPackageID_DockDoorAssignment()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save(); // to create stock

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			pick1.WP_CartoniseSplitCases = true;
			pick2.WP_CartoniseSplitCases = true;
			pick1.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			pick2.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var packageAAA = packingHelper.CreatePackage(pkgJob1, "AAA", 1, Constants.PkgUnit.Box);
			var packageBBB = packingHelper.CreatePackage(pkgJob2, "BBB", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(packageAAA, pick1.GetAllPickLines().First());
			packingHelper.CreatePackageDivot(packageBBB, pick2.GetAllPickLines().First());

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageAAA.PK, 1);
			Helper.Factory.Save();
			AssertEquals("Precondition: slots count", 1, trolleyJob.Slots.Count);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "BBB", 4);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
				AssertEquals("A slot must be added.", 2, trolleyJob.Slots.Count);

				trolleyJob.Slots.Single(t => t.WTS_SlotNumber == 1);
				var addedSlot = trolleyJob.Slots.Single(t => t.WTS_SlotNumber == (short)4);
				AssertEquals("Slot assignment must be saved to DB.", true, addedSlot.IsInDatabase);
				AssertEquals("Slot must be linked to correct package.", packageBBB.PK, addedSlot.WTS_KP_Package);

				var dockDoorAssignments = webService1.Factory.Load<WhsDockDoorAssignment>(new ZQuery());
				AssertEquals("Only 1 DDA created", 1, dockDoorAssignments.Length);

				var dda = dockDoorAssignments[0];
				AssertEquals("DDA correct DDL", dda.WDA_WL_AssignedDockDoor, data.Whs1.WW_DefaultOutboundDockDoor);
				AssertEquals("DDA correct PutawayTime", ZDateTime.Empty, dda.WDA_FirstPutawayToDockDoorUtc);

				AssertEquals("Pick1 DDA set", dda.PK, pick1.WP_WDA_DockDoorAssignment);
				AssertEquals("Pick1 DDL cleared", ZGuid.Empty, pick1.WP_WL_DockDoor);

				AssertEquals("Pick2 DDA set", dda.PK, pick2.WP_WDA_DockDoorAssignment);
				AssertEquals("Pick2 DDL cleared", ZGuid.Empty, pick2.WP_WL_DockDoor);
			});
		}

		public void TestAddSlotToTrolleyUsingPackageID_DockDoorAssignment_Error()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save(); // to create stock

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			pick1.WP_CartoniseSplitCases = true;
			pick2.WP_CartoniseSplitCases = true;
			pick1.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			pick2.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var packageAAA = packingHelper.CreatePackage(pkgJob1, "AAA", 1, Constants.PkgUnit.Box);
			var packageBBB = packingHelper.CreatePackage(pkgJob2, "BBB", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(packageAAA, pick1.GetAllPickLines().First());
			packingHelper.CreatePackageDivot(packageBBB, pick2.GetAllPickLines().First());

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageAAA.PK, 1);
			Helper.Factory.Save();
			AssertEquals("Precondition: slots count", 1, trolleyJob.Slots.Count);

			var ddaService = new Mock<IWhsPickDockDoorAssignmentService>();
			ddaService.Setup(m =>
				m.GeneratePickDockDoorAssignment(
					It.IsAny<ZGuid>(),
					It.Is<DockDoorAssignmentLinkType>(t => t == DockDoorAssignmentLinkType.Trolley),
					It.IsAny<ZGuid>(),
					It.IsAny<BusinessObjectFactory>()))
				.Returns("Error");

			using (ObjectFactory.Substitute(ddaService.Object))
			{
				var webService1 = GetNewWebService(data.Whs1);
				var response1 = webService1.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "BBB", 4);
				AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
				AssertEquals("Error", response1.ErrorMessage);
			}
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID_WrongTrolley

		public void TestAddSlotToTrolleyUsingPackageID_WrongTrolley()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.AddSlotToTrolleyUsingPackageID(Guid.NewGuid(), "ABC", 4);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
				AssertEquals("Trolley job was not found. Please start building trolley again.", response1.ErrorMessage);
			});

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "ABC", 4);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
				AssertEquals("This trolley job is in PIC state. No slots can be added to it.", response2.ErrorMessage);
			});

			trolleyJob.WTJ_Status = PickTrolleyStatus.Codes.Finalised;
			trolleyJob.WTJ_FinalisedDateUtc = ZDateTime.UtcNow;

			Helper.Factory.Save();

			var webService3 = GetNewWebService(data.Whs1);
			var response3 = webService3.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "ABC", 4);
			AssertSuccessfulResponse(response3, webService3);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response3.Error);
				AssertEquals("This trolley job is in FIN state. No slots can be added to it.", response3.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID_WrongPackage

		public void TestAddSlotToTrolleyUsingPackageID_WrongPackage()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock
			var orderFinalised = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pickFinalised = Helper.CreatePickNew(orderFinalised);
			pickFinalised.WP_CartoniseSplitCases = true;
			pickFinalised.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			var pkgJobFinalised = PkgPackageJob.LoadOrCreatePackageJob(orderFinalised);
			var packageABC = packingHelper.CreatePackage(pkgJobFinalised, "ABC", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(packageABC, pickFinalised.GetAllPickLines().First());
			pickFinalised.FinaliseAllOrders();
			pickFinalised.WP_IsCartonised = true;
			pickFinalised.FinalisePick();
			AssertIsFinalisedPrecondition(orderFinalised);
			AssertIsFinalisedPrecondition(pickFinalised);

			var orderForNotFinalisedPick = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pickNotFinalised = Helper.CreatePickNew(orderForNotFinalisedPick);
			pickNotFinalised.WP_CartoniseSplitCases = true;
			pickNotFinalised.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			var pkgJobNotFinalised = PkgPackageJob.LoadOrCreatePackageJob(orderForNotFinalisedPick);
			var packageXYZ = packingHelper.CreatePackage(pkgJobNotFinalised, "XYZ", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(packageXYZ, pickNotFinalised.GetAllPickLines().First());
			pickNotFinalised.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(orderForNotFinalisedPick);
			AssertEquals("Precondition: ", false, pickNotFinalised.IsFinalised);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var responseNoPackageID = webService1.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "MMM", 4);
			AssertSuccessfulResponse(responseNoPackageID, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, responseNoPackageID.Error);
				AssertEquals("Package 'MMM' was not found.", responseNoPackageID.ErrorMessage);
			});

			var webService2 = GetNewWebService(data.Whs1);
			var responsePackageFromFinalisedPick = webService2.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "ABC", 4);
			AssertSuccessfulResponse(responsePackageFromFinalisedPick, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, responsePackageFromFinalisedPick.Error);
				AssertEquals("Package 'ABC' was not found.", responsePackageFromFinalisedPick.ErrorMessage);
			});

			var webService3 = GetNewWebService(data.Whs1);
			var responseCorrectPackageID = webService3.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "XYZ", 4);
			AssertSuccessfulResponse(responseCorrectPackageID, webService3);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, responseCorrectPackageID.Error);
				AssertEquals(true, string.IsNullOrEmpty(responseCorrectPackageID.ErrorMessage));
			});

			var webService4 = GetNewWebService(data.Whs1);
			var responseSamePackageOtherSlot = webService4.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "XYZ", 1);
			AssertSuccessfulResponse(responseSamePackageOtherSlot, webService4);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, responseSamePackageOtherSlot.Error);
				AssertEquals("Package 'XYZ' already assigned to another trolley.", responseSamePackageOtherSlot.ErrorMessage);

				AssertEquals(1, trolleyJob.Slots.Count);
				AssertEquals((short)4, trolleyJob.Slots[0].WTS_SlotNumber);
				AssertEquals(packageXYZ.PK, trolleyJob.Slots[0].WTS_KP_Package);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID_DuplicateSlotNumber

		public void TestAddSlotToTrolleyUsingPackageID_DuplicateSlotNumber()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageAAA = packingHelper.CreatePackage(pkgJob, "AAA", 1, Constants.PkgUnit.Box);
			var packageBBB = packingHelper.CreatePackage(pkgJob, "BBB", 1, Constants.PkgUnit.Box);
			packageAAA.Pack(order.Lines[0].ReleaseLines[0], 5m);
			packageBBB.Pack(order.Lines[0].ReleaseLines[0], 5m);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response = webService1.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "AAA", 4);
			AssertSuccessfulResponse(response, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			AssertEquals(1, trolleyJob.Slots.Count);
			AssertEquals((short)4, trolleyJob.Slots[0].WTS_SlotNumber);
			AssertEquals(packageAAA.PK, trolleyJob.Slots[0].WTS_KP_Package);

			var webService2 = GetNewWebService(data.Whs1);
			var responseWrongSlot = webService2.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "BBB", 4);
			AssertSuccessfulResponse(responseWrongSlot, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, responseWrongSlot.Error);
				AssertEquals("Slot 4 on trolley 'T1' already filled with package 'AAA'. Please choose another slot.", responseWrongSlot.ErrorMessage);
				AssertEquals(1, trolleyJob.Slots.Count);
			});

			var webService3 = GetNewWebService(data.Whs1);
			var responseCorrectSlot = webService3.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "BBB", 88);
			AssertSuccessfulResponse(responseCorrectSlot, webService3);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, responseCorrectSlot.Error);
				AssertEquals(true, string.IsNullOrEmpty(responseCorrectSlot.ErrorMessage));

				AssertEquals(2, trolleyJob.Slots.Count);
				AssertEquals((short)88, trolleyJob.Slots[1].WTS_SlotNumber);
				AssertEquals(packageBBB.PK, trolleyJob.Slots[1].WTS_KP_Package);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID_NonUniquePackageID

		public void TestAddSlotToTrolleyUsingPackageID_NonUniquePackageID()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m);
			Helper.Factory.Save(); // to create stock

			// Create 3 packages with same ID
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_CartoniseSplitCases = true;
			pick1.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = packingHelper.CreatePackage(pkgJob1, "AAA", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pick1.GetAllPickLines().First());

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_CartoniseSplitCases = true;
			pick2.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = packingHelper.CreatePackage(pkgJob2, "AAA", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pick2.GetAllPickLines().First());

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var pick3 = Helper.CreatePickNew(order3);
			pick3.WP_CartoniseSplitCases = true;
			pick3.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var package3 = packingHelper.CreatePackage(pkgJob3, "AAA", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(package3, pick3.GetAllPickLines().ElementAt(0));

			// Create 2 trolleys
			var trolley1 = Helper.CreateTrolley("T1");
			var trolleyJob1 = Helper.CreateWhsPickTrolleyJob(trolley1, PickTrolleyStatus.Codes.Building);

			var trolley2 = Helper.CreateTrolley("T2");
			var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley2, PickTrolleyStatus.Codes.Building);

			Helper.Factory.Save();

			// try to Allocate one of the packages to one of the trolleys. Should give user 3 packages to choose from
			var webService1 = GetNewWebService(data.Whs1);
			var response1NonUniquePackageID = webService1.AddSlotToTrolleyUsingPackageID(trolleyJob1.PK.ToGuid(), "AAA", 4);
			AssertSuccessfulResponse(response1NonUniquePackageID, webService1);

			CombineAssertions(() =>
			{
				AssertEquals("When multiple packages exist with the Package ID, then they all should be returned to user to choose the correct one.", ErrorTypes.None, response1NonUniquePackageID.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1NonUniquePackageID.ErrorMessage));
				AssertEquals(3, response1NonUniquePackageID.PackageChoices.Count);
				response1NonUniquePackageID.PackageChoices.Single(p => p.PK == package1.PK);
				response1NonUniquePackageID.PackageChoices.Single(p => p.PK == package2.PK);
				response1NonUniquePackageID.PackageChoices.Single(p => p.PK == package3.PK);
				AssertEquals(false, trolleyJob1.Slots.Any());
			});

			// allocate one of the packages to one of the trolleys. Now only 2 packages should be available to choose from for second trolley
			var slot = Helper.CreateWhsPickTrolleySlot(trolleyJob1, package1, 2);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response2NonUniquePackageID = webService2.AddSlotToTrolleyUsingPackageID(trolleyJob2.PK.ToGuid(), "AAA", 4);
			AssertSuccessfulResponse(response2NonUniquePackageID, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("When multiple packages exist with the Package ID, then they all should be returned to user to choose the correct one.", ErrorTypes.None, response2NonUniquePackageID.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2NonUniquePackageID.ErrorMessage));
				AssertEquals(2, response2NonUniquePackageID.PackageChoices.Count);
				response2NonUniquePackageID.PackageChoices.Single(p => p.PK == package2.PK);
				response2NonUniquePackageID.PackageChoices.Single(p => p.PK == package3.PK);
				AssertEquals(false, trolleyJob2.Slots.Any());
			});

			// now if we finalise one of the picks, only one package AAA will be "available"
			pick2.FinaliseAllOrders();
			pick2.WP_IsCartonised = true;
			pick2.FinalisePick();
			AssertIsFinalisedPrecondition(order2);
			AssertIsFinalisedPrecondition(pick2);
			Helper.Factory.Save();

			var webService3 = GetNewWebService(data.Whs1);
			var responseCorrect = webService3.AddSlotToTrolleyUsingPackageID(trolleyJob2.PK.ToGuid(), "AAA", 4);
			AssertSuccessfulResponse(responseCorrect, webService3);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, responseCorrect.Error);
				AssertEquals(true, string.IsNullOrEmpty(responseCorrect.ErrorMessage));
				AssertEquals(1, trolleyJob2.Slots.Count);
				AssertEquals((short)4, trolleyJob2.Slots[0].WTS_SlotNumber);
				AssertEquals(package3.PK, trolleyJob2.Slots[0].WTS_KP_Package);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID_NonUniquePackageID_DbHits

		public void TestAddSlotToTrolleyUsingPackageID_NonUniquePackageID_DbHits() => TestAddSlotToTrolleyUsingPackageID_NonUniquePackageID_DbHits(false);
		public void TestAddSlotToTrolleyUsingPackageID_NonUniquePackageID_DbHits_WithExistingSlots() => TestAddSlotToTrolleyUsingPackageID_NonUniquePackageID_DbHits(true);

		void TestAddSlotToTrolleyUsingPackageID_NonUniquePackageID_DbHits(bool withExistingSlots)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 60m);
			Helper.Factory.Save(); // to create stock

			// Create 5 packages with same ID
			for (var i = 0; i < 5; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O" + i);
				var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
				var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
				var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
				var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
				var orderLine5 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

				var pick = Helper.CreatePickNew(order);
				pick.WP_CartoniseSplitCases = true;

				orderLine1.PickLines.Single().WZ_F3_NKAllocatedPackType = "AB" + i;
				orderLine2.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Bag;
				orderLine3.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Bottle;
				orderLine4.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.BreakBulk;
				orderLine5.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;

				var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
				var package = packingHelper.CreatePackage(pkgJob, "AAA", 1, Constants.PkgUnit.Box);

				packingHelper.CreatePackageDivot(package, orderLine1.PickLines.Single());
				packingHelper.CreatePackageDivot(package, orderLine2.PickLines.Single());
				packingHelper.CreatePackageDivot(package, orderLine3.PickLines.Single());
				packingHelper.CreatePackageDivot(package, orderLine4.PickLines.Single());
				packingHelper.CreatePackageDivot(package, orderLine5.PickLines.Single());
			}

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);

			if (withExistingSlots)
			{
				for (var i = 1; i < 6; i++)
				{
					var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + (5 + i), data.Part1, 1m);
					var pick = Helper.CreatePickNew(order);
					pick.WP_CartoniseSplitCases = true;

					var pickLine = order.Lines[0].PickLines.Single();
					pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;

					var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
					var packageABC = packingHelper.CreatePackage(pkgJob, "DEF", 1, Constants.PkgUnit.Box);
					packingHelper.CreatePackageDivot(packageABC, pickLine);

					var slot = trolleyJob.Slots.AddNew();
					slot.WTS_SlotNumber = (short)i;
					slot.WTS_KP_Package = packageABC.PK;
				}
			}

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var webServiceFactory = webService.Factory;

			var expectedDBHits = new Dictionary<string, int>
			{
				{ nameof(OrgHeader), 1 },
				{ nameof(GenAddOnColumn), withExistingSlots ? 2 : 1 },
				{ nameof(PkgPackage), withExistingSlots ? 2 : 1 },
				{ nameof(PkgPackageItemDivot), 1 },
				{ nameof(PkgPackageJob), withExistingSlots ? 2 : 1 },
				{ nameof(WhsDocket), withExistingSlots ? 2 : 1 },
				{ nameof(WhsPick), withExistingSlots ? 2 : 0 },
				{ nameof(WhsPickLine), 1 },
				{ nameof(WhsPickTrolleyJob), 1 },
				{ nameof(WhsPickTrolleySlot), withExistingSlots ? 3 : 2 },
				{ nameof(WhsWarehouse), 1 },
			};

			PackageWebServiceResponse response;
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webServiceFactory))
			{
				response = webService.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "AAA", 8);
			}

			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("When multiple packages exist with the Package ID, then they all should be returned to user to choose the correct one.", ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals(5, response.PackageChoices.Count);
				AssertEquals(withExistingSlots, trolleyJob.Slots.Any());
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID_PackageWithoutPickLines

		public void TestAddSlotToTrolleyUsingPackageID_PackageWithoutPickLines()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save(); // to create stock
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			AssertEquals("Precondition:", 5m, order.Lines[0].PickLineQuantity);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageAAA = packingHelper.CreatePackage(pkgJob, "AAA", 1, Constants.PkgUnit.Box);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			AssertEquals("Precondition", false, trolleyJob.Slots.Any());
			Helper.Factory.Save();

			// when package have no pick lines
			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "AAA", 4);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
				AssertEquals("Package scanned is not valid for Trolley Picking.", response1.ErrorMessage);
				AssertEquals("No new slots should be added", false, trolleyJob.Slots.Any());
			});

			// add pick line to the package
			packingHelper.CreatePackageDivot(packageAAA, pick.GetAllPickLines().First());
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "AAA", 4);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals("A slot must be added.", 1, trolleyJob.Slots.Count);
				AssertEquals("Slot assignment must be saved to DB.", true, trolleyJob.Slots[0].IsInDatabase);
				AssertEquals("Slot number must be populated.", (short)4, trolleyJob.Slots[0].WTS_SlotNumber);
				AssertEquals("Slot must be linked to correct package.", packageAAA.PK, trolleyJob.Slots[0].WTS_KP_Package);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID_PackageFromWrongWarehouse

		public void TestAddSlotToTrolleyUsingPackageID_PackageFromWrongWarehouse()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var whs2 = Helper.CreateWarehouse("WH2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save(); // to create stock
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition:", 5m, order.Lines[0].PickLineQuantity);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageAAA = packingHelper.CreatePackage(pkgJob, "AAA", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(packageAAA, pick.GetAllPickLines().First());

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			AssertEquals("Precondition", 0, trolleyJob.Slots.Count);
			Helper.Factory.Save();

			// trying to add packageAAA from whs1 while logged in as whs2.
			var webService = GetNewWebService(whs2);
			var response = webService.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "AAA", 4);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Package 'AAA' was not found.", response.ErrorMessage);
				AssertEquals("No new slots should be added", false, trolleyJob.Slots.Any());
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID_PackageFromADifferentPick

		public void TestAddSlotToTrolleyUsingPackageID_PackageFromADifferentPick()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var otherDockDoorLocation = data.Whs1.FindLocation("A-2");
			otherDockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_WL_DockDoor = otherDockDoorLocation.PK;
			pick1.WP_CartoniseSplitCases = true;
			pick2.WP_CartoniseSplitCases = true;
			pick1.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			pick2.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var packageAAA = packingHelper.CreatePackage(pkgJob1, "AAA", 1, Constants.PkgUnit.Box);
			var packageBBB = packingHelper.CreatePackage(pkgJob2, "BBB", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(packageAAA, pick1.GetAllPickLines().First());
			packingHelper.CreatePackageDivot(packageBBB, pick2.GetAllPickLines().First());

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			AssertEquals("Precondition", false, trolleyJob.Slots.Any());
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "AAA", 1);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
				AssertEquals("Precondition: Added package to slot.", 1, trolleyJob.Slots.Count);
			});

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "BBB", 2);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
				AssertEquals("Package 'BBB' is assigned to another Dock Door Location.", response2.ErrorMessage);
				AssertEquals("No new slots should be added", 1, trolleyJob.Slots.Count);
			});

			pick2.WP_WL_DockDoor = data.Whs1.WW_DefaultOutboundDockDoor;
			Helper.Factory.Save();

			var webService3 = GetNewWebService(data.Whs1);
			var response3 = webService3.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "BBB", 2);
			AssertSuccessfulResponse(response3, webService3);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response3.Error);
				AssertEquals(true, string.IsNullOrEmpty(response3.ErrorMessage));
				AssertEquals("Precondition: Added package to slot.", 2, trolleyJob.Slots.Count);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID_NonUniquePackageID_PackageForADifferentDockDoor

		public void TestAddSlotToTrolleyUsingPackageID_NonUniquePackageID_PackageForADifferentDockDoor()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var otherDockDoorLocation = data.Whs1.FindLocation("A-2");
			otherDockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_WL_DockDoor = otherDockDoorLocation.PK;
			pick1.WP_CartoniseSplitCases = true;
			pick2.WP_CartoniseSplitCases = true;
			pick1.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			pick2.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var packageAAA = packingHelper.CreatePackage(pkgJob1, "AAA", 1, Constants.PkgUnit.Box);
			var packageBBB_Pick1 = packingHelper.CreatePackage(pkgJob1, "BBB", 1, Constants.PkgUnit.Box);
			var packageBBB_Pick2 = packingHelper.CreatePackage(pkgJob2, "BBB", 1, Constants.PkgUnit.Box);

			var pickLine1 = pick1.GetAllPickLines().First();
			var pickLine2 = pickLine1.Split(2m);
			packingHelper.CreatePackageDivot(packageAAA, pickLine1);
			packingHelper.CreatePackageDivot(packageBBB_Pick1, pickLine2);
			packingHelper.CreatePackageDivot(packageBBB_Pick2, pick2.GetAllPickLines().First());

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			AssertEquals("Precondition", false, trolleyJob.Slots.Any());
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "AAA", 1);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
				AssertEquals("Precondition: Added package to slot.", 1, trolleyJob.Slots.Count);
			});

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "BBB", 2);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals("Should *not* have prompted user for another package.", 0, response2.PackageChoices.Count);
				AssertEquals("Precondition: Added package to slot.", 2, trolleyJob.Slots.Count);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID_NonUniquePackageID_PackageForADifferentDockDoorAndSameDockDoor

		public void TestAddSlotToTrolleyUsingPackageID_NonUniquePackageID_PackageFrorADifferentDockDoorAndSameDockDoor()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var otherDockDoorLocation = data.Whs1.FindLocation("A-2");
			otherDockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Helper.Factory.Save(); // to create stock

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1, order2);
			var pick2 = Helper.CreatePickNew(order3);
			pick2.WP_WL_DockDoor = otherDockDoorLocation.PK;
			pick1.WP_CartoniseSplitCases = true;
			pick2.WP_CartoniseSplitCases = true;
			pick1.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			pick2.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var packageAAA = packingHelper.CreatePackage(pkgJob1, "AAA", 1, Constants.PkgUnit.Box);
			var packageBBB_1_Pick1 = packingHelper.CreatePackage(pkgJob1, "BBB", 1, Constants.PkgUnit.Box);
			var packageBBB_2_Pick1 = packingHelper.CreatePackage(pkgJob2, "BBB", 1, Constants.PkgUnit.Box);
			var packageBBB_Pick2 = packingHelper.CreatePackage(pkgJob3, "BBB", 1, Constants.PkgUnit.Box);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var pickLine2 = pickLine1.Split(2m);
			packingHelper.CreatePackageDivot(packageAAA, pickLine1);
			packingHelper.CreatePackageDivot(packageBBB_1_Pick1, pickLine2);
			packingHelper.CreatePackageDivot(packageBBB_2_Pick1, order2.Lines[0].PickLines.Single());
			packingHelper.CreatePackageDivot(packageBBB_Pick2, pick2.GetAllPickLines().First());

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			AssertEquals("Precondition", false, trolleyJob.Slots.Any());
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "AAA", 1);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
				AssertEquals("Precondition: Added package to slot.", 1, trolleyJob.Slots.Count);
			});

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "BBB", 2);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("When multiple packages exist with the Package ID, then they all should be returned to user to choose the correct one.", ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals(2, response2.PackageChoices.Count);
				response2.PackageChoices.Single(p => p.PK == packageBBB_1_Pick1.PK);
				response2.PackageChoices.Single(p => p.PK == packageBBB_2_Pick1.PK);
				AssertEquals("Did not yet add a package to slot.", 1, trolleyJob.Slots.Count);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID_PackageFromNotCartonizedPick

		public void TestAddSlotToTrolleyUsingPackageID_PackageFromNotCartonizedPick()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var untPackType = Helper.Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Unit));
			untPackType.F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save(); // to create stock
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition:", 5m, order.Lines[0].PickLineQuantity);

			var pickLine = pick.GetAllPickLines().First();
			pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageAAA = packingHelper.CreatePackage(pkgJob, "AAA", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(packageAAA, pickLine);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			AssertEquals("Precondition", false, trolleyJob.Slots.Any());
			Helper.Factory.Save();

			// when package attached to Pick that is not Cartonised
			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "AAA", 4);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
				AssertEquals("Package 'AAA' was not found.", response1.ErrorMessage);
				AssertEquals("No new slots should be added", false, trolleyJob.Slots.Any());
			});

			// cartonise pick
			pick.WP_CartoniseSplitCases = true;
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "AAA", 4);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals("A slot must be added.", 1, trolleyJob.Slots.Count);
				AssertEquals("Slot assignment must be saved to DB.", true, trolleyJob.Slots[0].IsInDatabase);
				AssertEquals("Slot number must be populated.", (short)4, trolleyJob.Slots[0].WTS_SlotNumber);
				AssertEquals("Slot must be linked to correct package.", packageAAA.PK, trolleyJob.Slots[0].WTS_KP_Package);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID_PickLinesWithNotSplitCaseUOM

		public void TestAddSlotToTrolleyUsingPackageID_PickLinesWithNotSplitCaseUOM()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Box, 4);

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var smallCarton = Helper.CreateWhsCartonSize("SML", 10, 10, 10, 10, 20, 40, new ZByte(80), Constants.Length.Metres, Constants.Weight.Tonnes);
			whsGroup.CartonSizes.AddRange(new[] { smallCarton });
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
			AssertEquals("Precondition", false, trolleyJob.Slots.Any());
			Helper.Factory.Save();

			// trying to attach package with Case Unit of Measure
			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), casePkg.KP_PackageID, 4);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
				AssertEquals("Package scanned is not valid for Trolley Picking.", response1.ErrorMessage);
				AssertEquals("No new slots should be added", false, trolleyJob.Slots.Any());
			});

			// trying to attach package with Split Case Unit of Measure
			pick.WP_CartoniseSplitCases = true;
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), splitCasePkg.KP_PackageID, 4);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals("A slot must be added.", 1, trolleyJob.Slots.Count);
				AssertEquals("Slot assignment must be saved to DB.", true, trolleyJob.Slots[0].IsInDatabase);
				AssertEquals("Slot number must be populated.", (short)4, trolleyJob.Slots[0].WTS_SlotNumber);
				AssertEquals("Slot must be linked to correct package.", splitCasePkg.PK, trolleyJob.Slots[0].WTS_KP_Package);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID_Error_ToteTrolley

		public void TestAddSlotToTrolleyUsingPackageID_Error_ToteTrolley()
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageABC = packingHelper.CreatePackage(pkgJob, "ABC", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(packageABC, pickLine);

			var otherPackageJob = PkgPackageJob.LoadOrCreatePackageJob(Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2"));
			var totePackage = packingHelper.CreatePackage(otherPackageJob, "TP1", 1, Constants.PkgUnit.Tote);
			totePackage.SetIsTote(true);

			var toteTrolley = Helper.CreateTrolley("T1");
			var toteTrolleyJob = Helper.CreateWhsPickTrolleyJob(toteTrolley, PickTrolleyStatus.Codes.Building);
			var slot = toteTrolleyJob.Slots.AddNew();
			slot.WTS_SlotNumber = 1;
			slot.WTS_KP_Package = totePackage.PK;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingPackageID(toteTrolleyJob.PK.ToGuid(), "ABC", 2);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Response should be in error as you cannot add Carton to Tote trolley.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Response messaage should explain that you cannot add Carton to Tote trolley.", "Cannot add 'Package' to Trolley of type 'Tote'", response.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackageID_PreSaveValidation

		public void TestAddSlotToTrolleyUsingPackageID_PreSaveValidation()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Helper.Factory.Save(); // to create stock
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			AssertEquals("Precondition:", 1m, order.Lines[0].PickLineQuantity);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageAAA = packingHelper.CreatePackage(pkgJob, "AAA", 1, Constants.PkgUnit.Box); // some other package to make sure that correct one will be used
			var picklines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(packageAAA, picklines[0]);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();
			AssertEquals(0, trolleyJob.Slots.Count);

			var webService = GetNewWebService(data.Whs1);
			var trolleyJobInOtherFactory = webService.Factory.Load<WhsPickTrolleyJob>(trolleyJob.PK);
			// if job have an error,
			trolleyJobInOtherFactory.AddRowError("Random Job Error.");
			trolleyJobInOtherFactory.Slots.CountChanged += (o, e) =>
			{
				trolleyJobInOtherFactory.Slots[0].AddRowError("Random Slot Error.");
			};

			var expectedError = @"Error - WhsPickTrolleyJob: Random Job Error.
Error - WhsPickTrolleySlot: Random Slot Error.";

			var response = webService.AddSlotToTrolleyUsingPackageID(trolleyJobInOtherFactory.PK.ToGuid(), "AAA", 4);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals(expectedError, response.ErrorMessage);
				AssertEquals("A slot must be added.", 1, trolleyJobInOtherFactory.Slots.Count);
				AssertEquals("Slot assignment must *NOT* be saved to DB.", false, trolleyJobInOtherFactory.Slots[0].IsInDatabase);
				AssertEquals("Slot number must be populated.", (short)4, trolleyJobInOtherFactory.Slots[0].WTS_SlotNumber);
				AssertEquals("Slot must be linked to correct package.", packageAAA.PK, trolleyJobInOtherFactory.Slots[0].WTS_KP_Package);
			});
		}

		#endregion

		#endregion

		#region TestAddSlotToTrolleyUsingPackagePK

		#region TestAddSlotToTrolleyUsingPackagePK

		public void TestAddSlotToTrolleyUsingPackagePK()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save(); // to create stock
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			AssertEquals("Precondition:", 10m, order.Lines[0].PickLineQuantity);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageAAA = packingHelper.CreatePackage(pkgJob, "AAA", 1, Constants.PkgUnit.Box); // some other package to make sure that correct one will be used
			var packageABC = packingHelper.CreatePackage(pkgJob, "ABC", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(packageABC, pick.GetAllPickLines().ElementAt(0));
			packingHelper.CreatePackageDivot(packageABC, pick.GetAllPickLines().ElementAt(1));

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();
			AssertEquals("Precondition", false, trolleyJob.Slots.Any());

			// When adding random guid
			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.AddSlotToTrolleyUsingPackagePK(trolleyJob.PK.ToGuid(), Guid.NewGuid(), 4);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
				AssertEquals("Package was not found.", response1.ErrorMessage);
				AssertEquals("No slots should have been added.", false, trolleyJob.Slots.Any());
			});

			// When adding Package 
			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.AddSlotToTrolleyUsingPackagePK(trolleyJob.PK.ToGuid(), packageABC.PK.ToGuid(), 4);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals("A slot should have been added.", 1, trolleyJob.Slots.Count);
				AssertEquals("Slot assignment must be saved to DB.", true, trolleyJob.Slots[0].IsInDatabase);
				AssertEquals("Slot number must be populated.", (short)4, trolleyJob.Slots[0].WTS_SlotNumber);
				AssertEquals("Slot must be linked to correct package.", packageABC.PK, trolleyJob.Slots[0].WTS_KP_Package);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackagePK_PackageFromADifferentPick

		public void TestAddSlotToTrolleyUsingPackagePK_PackageFromADifferentPick()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var otherDockDoorLocation = data.Whs1.FindLocation("A-2");
			otherDockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_WL_DockDoor = otherDockDoorLocation.PK;
			pick1.WP_CartoniseSplitCases = true;
			pick2.WP_CartoniseSplitCases = true;
			pick1.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			pick2.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var packageAAA = packingHelper.CreatePackage(pkgJob1, "AAA", 1, Constants.PkgUnit.Box);
			var packageBBB = packingHelper.CreatePackage(pkgJob2, "BBB", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(packageAAA, pick1.GetAllPickLines().First());
			packingHelper.CreatePackageDivot(packageBBB, pick2.GetAllPickLines().First());

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			AssertEquals("Precondition", false, trolleyJob.Slots.Any());
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.AddSlotToTrolleyUsingPackagePK(trolleyJob.PK.ToGuid(), packageAAA.PK.ToGuid(), 1);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
				AssertEquals("Precondition: Added package to slot.", 1, trolleyJob.Slots.Count);
			});

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.AddSlotToTrolleyUsingPackagePK(trolleyJob.PK.ToGuid(), packageBBB.PK.ToGuid(), 2);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
				AssertEquals("Package 'BBB' is assigned to another Dock Door Location.", response2.ErrorMessage);
				AssertEquals("No new slots should be added", 1, trolleyJob.Slots.Count);
			});

			pick2.WP_WL_DockDoor = data.Whs1.WW_DefaultOutboundDockDoor;
			Helper.Factory.Save();

			var webService3 = GetNewWebService(data.Whs1);
			var response3 = webService3.AddSlotToTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "BBB", 2);
			AssertSuccessfulResponse(response3, webService3);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response3.Error);
				AssertEquals(true, string.IsNullOrEmpty(response3.ErrorMessage));
				AssertEquals("Precondition: Added package to slot.", 2, trolleyJob.Slots.Count);
			});
		}

		#endregion

		#region TestAddSlotToTrolleyUsingPackagePK_DockDoorAssignment

		public void TestAddSlotToTrolleyUsingPackagePK_DockDoorAssignment()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save(); // to create stock

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			pick1.WP_CartoniseSplitCases = true;
			pick2.WP_CartoniseSplitCases = true;
			pick1.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			pick2.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var packageAAA = packingHelper.CreatePackage(pkgJob1, "AAA", 1, Constants.PkgUnit.Box);
			var packageBBB = packingHelper.CreatePackage(pkgJob2, "BBB", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(packageAAA, pick1.GetAllPickLines().First());
			packingHelper.CreatePackageDivot(packageBBB, pick2.GetAllPickLines().First());

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageAAA.PK, 1);
			Helper.Factory.Save();
			AssertEquals("Precondition: slots count", 1, trolleyJob.Slots.Count);

			// When adding Package 
			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.AddSlotToTrolleyUsingPackagePK(trolleyJob.PK.ToGuid(), packageBBB.PK.ToGuid(), 4);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals("A slot should have been added.", 2, trolleyJob.Slots.Count);

				trolleyJob.Slots.Single(t => t.WTS_SlotNumber == 1);
				var addedSlot = trolleyJob.Slots.Single(t => t.WTS_SlotNumber == (short)4);
				AssertEquals("Slot assignment must be saved to DB.", true, addedSlot.IsInDatabase);
				AssertEquals("Slot must be linked to correct package.", packageBBB.PK, addedSlot.WTS_KP_Package);

				var dockDoorAssignments = webService2.Factory.Load<WhsDockDoorAssignment>(new ZQuery());
				AssertEquals("Only 1 DDA created", 1, dockDoorAssignments.Length);

				var dda = dockDoorAssignments[0];
				AssertEquals("DDA correct DDL", dda.WDA_WL_AssignedDockDoor, data.Whs1.WW_DefaultOutboundDockDoor);
				AssertEquals("DDA correct PutawayTime", ZDateTime.Empty, dda.WDA_FirstPutawayToDockDoorUtc);

				AssertEquals("Pick1 DDA set", dda.PK, pick1.WP_WDA_DockDoorAssignment);
				AssertEquals("Pick1 DDL cleared", ZGuid.Empty, pick1.WP_WL_DockDoor);

				AssertEquals("Pick2 DDA set", dda.PK, pick2.WP_WDA_DockDoorAssignment);
				AssertEquals("Pick2 DDL cleared", ZGuid.Empty, pick2.WP_WL_DockDoor);
			});
		}

		#endregion

		#endregion

		#region TestAddSlotToTrolleyUsingToteID_OrderExcludedFromTotePicking

		public void TestAddSlotToTrolleyUsingToteID_OrderExcludedFromTotePicking()
		{
			TestAddSlotToTrolleyUsingToteID_OrderExcludedFromTotePickingCore(string.Empty, "No valid orders to assign.");
		}

		public void TestAddSlotToTrolleyUsingToteID_OrderExcludedFromTotePicking_WithOrderId()
		{
			TestAddSlotToTrolleyUsingToteID_OrderExcludedFromTotePickingCore("O1", "O1 is not a valid order to assign.");
		}

		void TestAddSlotToTrolleyUsingToteID_OrderExcludedFromTotePickingCore(string orderId, string expectedErrorMsg)
		{
			var data = SetupDataForTote();
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_ExcludeFromTotePicking = true;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order).WP_CartoniseSplitCases = false;
			orderLine.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG1", 4, new SearchFilterCriteriaInfo(), orderId);
			AssertSuccessfulResponse(response, webService);
			CombineAssertions(() =>
			{
				AssertEquals("Should have errors.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Should have error message.", expectedErrorMsg, response.ErrorMessage);
			});
		}

		#endregion

		#region TestAddSlotToTrolley_ShouldBumpCriticalVersionIDForTrolley

		public void TestAddSlotToTrolley_ShouldBumpCriticalVersionIDForTrolley()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageABC = packingHelper.CreatePackage(pkgJob, "ABC", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(packageABC, pick.GetAllPickLines().ElementAt(0));

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();
			AssertEquals("Precondition", false, trolleyJob.Slots.Any());
			AssertEquals("Precondition - WTJ_CriticalChangesVersionID: ", ZGuid.Empty, trolleyJob.WTJ_CriticalChangesVersionID);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.AddSlotToTrolleyUsingPackagePK(trolleyJob.PK.ToGuid(), packageABC.PK.ToGuid(), 4);
			AssertSuccessfulResponse(response, webService);
			var newFactory = new BusinessObjectFactory();
			var trolleyJobInNewFactory = newFactory.Load<WhsPickTrolleyJob>(trolleyJob.PK);
			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("A slot should have been added.", 1, trolleyJobInNewFactory.Slots.Count);
				AssertEquals("Slot assignment must be saved to DB.", true, trolleyJobInNewFactory.Slots[0].IsInDatabase);
				AssertEquals("Slot number must be populated.", (short)4, trolleyJobInNewFactory.Slots[0].WTS_SlotNumber);
				AssertEquals("Slot must be linked to correct package.", packageABC.PK, trolleyJobInNewFactory.Slots[0].WTS_KP_Package);
				AssertNotEquals("Should bump WTJ_CriticalChangesVersionID when slot is added to the trolley", ZGuid.Empty, trolleyJobInNewFactory.WTJ_CriticalChangesVersionID);
			});
		}

		#endregion

		#region TestAddSlotToTrolley_ThrowErrorIfSetPackingStationForTotePickingAtTheSameTime

		public void TestAddSlotToTrolley_ThrowErrorIfSetPackingStationForTotePickingAtTheSameTime()
		{
			var factory = Helper.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory);

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var pick = helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 2 pick lines.", 2, pick.GetAllPickLines().Count());
			factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			pkg_OnTrolley1.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley1, pickLines[0]);

			var pkg_OnTrolley2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, Constants.PkgUnit.Box);
			pkg_OnTrolley2.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley2, pickLines[1]);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley1, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley2, 2);
			factory.Save();
			pickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			factory.Save();
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Tote, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			factory.Save();
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_CartoniseSplitCases = false;
			orderLine1.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			factory.Save();
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);

			var webService1 = GetNewWebService(data.Whs1);
			webService1.Factory.RefreshEnabled = false;
			webService1.Factory.Saving += delegate
			{
				var webService = GetNewWebService();
				webService.Factory.RefreshEnabled = false;
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
				var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, packingLocation.WLV_LocationString);
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			};

			var response1 = webService1.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG3", 3, new SearchFilterCriteriaInfo());
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
			AssertEquals("Concurrency error message should be prompted",
				"While you have been working with this job another user has made changes. Please restart the operation and try again.",
				response1.ErrorMessage);
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInFactory = newFactory.Load<WhsPick>(pick.PK);
			var pick1InFactory = newFactory.Load<WhsPick>(pick1.PK);
			var trolleyJobInFactory = newFactory.Load<WhsPickTrolleyJob>(trolleyJob.PK);
			AssertEquals("Pick's Packing Station should be set: ", packingLocation.PK, pickInFactory.WP_WL_PackingStation);
			Assert("Pick1's Packing Station should NOT be set: ", pick1InFactory.WP_WL_PackingStation.IsEmpty);
			AssertEquals("Should NOT add a new slot.", 2, trolleyJobInFactory.Slots.Count);
		}

		#endregion

		#region TestAddSlotToTrolley_NewPick

		public void TestAddSlotToTrolley_NewPickWithDifferentPackingStation()
		{
			TestAddSlotToTrolley_NewPickWithDifferentPackingStation(isTrolleyJobHasPackingStation: true);
		}

		public void TestAddSlotToTrolley_NewPickWithDifferentPackingStation_TrolleyJobHasNoPackingStation()
		{
			TestAddSlotToTrolley_NewPickWithDifferentPackingStation(isTrolleyJobHasPackingStation: false);
		}

		void TestAddSlotToTrolley_NewPickWithDifferentPackingStation(bool isTrolleyJobHasPackingStation)
		{
			var factory = Helper.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory);

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var pick = helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 2 pick lines.", 2, pick.GetAllPickLines().Count());
			factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			pkg_OnTrolley1.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley1, pickLines[0]);

			var pkg_OnTrolley2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, Constants.PkgUnit.Box);
			pkg_OnTrolley2.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley2, pickLines[1]);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley1, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley2, 2);
			factory.Save();
			pickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			factory.Save();
			if (isTrolleyJobHasPackingStation)
			{
				var webService = GetNewWebService();
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
				var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, packingLocation.WLV_LocationString);
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			}
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInFactory = newFactory.Load<WhsPick>(pick.PK);
			AssertEquals("Pick's Packing Station: ", isTrolleyJobHasPackingStation, pickInFactory.WP_WL_PackingStation == packingLocation.PK);

			var newRow1 = helper.CreateRowAndGenerateLocations(data.Whs1, "K");
			var packingLocation1 = newRow1.Locations[0];
			packingLocation1.WLV_WLT_LocationType = packingStationLocationType.PK;
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Tote, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			factory.Save();
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_WL_PackingStation = packingLocation1.PK;
			pick1.WP_CartoniseSplitCases = false;
			orderLine1.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			factory.Save();

			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG3", 3, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response1, webService1);

			AssertEquals("Should has an Error.", ErrorTypes.BusinessValidationError, response1.Error);
			AssertEquals("Should not allow to add package from a pick with different packing station.", "Package 'PKG3' has a different Packing station from other Picks on the trolley.", response1.ErrorMessage);
			AssertEquals("Should not add a new slot.", 2, trolleyJob.Slots.Count);
		}

		public void TestAddSlotToTrolley_NewPickWithoutPackingStationAndTrolleyJobWithPackingStation()
		{
			var factory = Helper.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory);

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var pick = helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 2 pick lines.", 2, pick.GetAllPickLines().Count());
			factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			pkg_OnTrolley1.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley1, pickLines[0]);

			var pkg_OnTrolley2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, Constants.PkgUnit.Box);
			pkg_OnTrolley2.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley2, pickLines[1]);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley1, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley2, 2);
			factory.Save();
			pickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, packingLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInFactory = newFactory.Load<WhsPick>(pick.PK);
			AssertEquals("Pick's Packing Station should be set: ", true, pickInFactory.WP_WL_PackingStation == packingLocation.PK);

			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Tote, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			factory.Save();
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_CartoniseSplitCases = false;
			orderLine1.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Tote;
			factory.Save();

			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "PKG3", 3, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response1, webService1);

			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
			AssertEquals("Should add a new slot.", 3, trolleyJob.Slots.Count);
			var pick1InFactory = newFactory.Load<WhsPick>(pick1.PK);
			AssertEquals("Pick1's Packing Station should be set: ", true, pick1InFactory.WP_WL_PackingStation == packingLocation.PK);
		}

		#endregion

		#region Implementation

		#region SetupDataForTote

		TestDataSimpleEnvironment SetupDataForTote()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);

			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Tote, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);

			Helper.Factory.Save();

			return data;
		}

		#endregion

		#region AssertTotePackage

		void AssertTotePackage(PkgPackage tote, ZString expectedPackageID, IEnumerable<WhsPickLine> expectedToteContents)
		{
			AssertEquals("The Package should have ID of the scanned tote.", expectedPackageID, tote.KP_PackageID);
			AssertEquals("The Package should be a Tote  Package.", true, tote.GetIsTote());
			AssertEquals("The Package should have a Goods Description of 'Warehouse Tote'.", "Warehouse Tote", tote.KP_GoodsDescription);
			AssertEquals("The Package should have the expected Packed Item Divots.", expectedToteContents.Count(), tote.PackedItemDivots.Count);

			var expectedToteContentsAsString = expectedToteContents.Select(p => string.Join("|", WhsPickLineSchema.Constants.Prefix, p.PK, p.WZ_Units.ToString("#.###")));
			var actualToteContentsAsString = tote.PackedItemDivots.Select(t => string.Join("|", t.KI_ParentTableCode, t.KI_ParentID, t.KI_PackedQty.ToString("#.###")));
			AssertContainsExactElementsInAnyOrder("Tote Package should contain expected Packing.", expectedToteContentsAsString, actualToteContentsAsString);
		}

		#endregion

		#endregion
	}
}
