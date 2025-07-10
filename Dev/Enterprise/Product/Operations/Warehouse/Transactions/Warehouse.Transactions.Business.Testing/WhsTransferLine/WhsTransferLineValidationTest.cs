using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsTransferLineValidationTest : WhsDocketLineValidationTestCase<WhsTransferLine, WhsTransfer>
	{
		#region PerformanceTest

		public void TestDBHits_FinalizeTransferLinesWithPalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "P123");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			Assert("Receive should be finalised.", receive.IsFinalised);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			for (int i = 0; i < 100; i++)
			{
				Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "P123", "A-2", "P123");
			}
			transfer.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transferInOtherFactory = newfactory.Load<WhsTransfer>(transfer.PK);
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 2 },
				{ JobServiceSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketContainerSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 7 },
				{ WhsDocketReferenceSchema.Constants.TableName, 1 },
				{ WhsDocketPalletSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 4 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 6 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsVASOrderSchema.Constants.TableName, 2 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
			};

			using (RowFactory.SetCachedTables())
			{
				transferInOtherFactory.FinaliseDocketWithoutUserConfirmation();
			}

			AssertDbHits(expectedDbHits, newfactory);
			Assert("Transfer should be finalised.", transferInOtherFactory.IsFinalised);
		}

		public void TestDBHits_FinalizeTransferLinesWithPalletIDs_ManyLines_WhsLocation()
		{
			// It is expected that the 100 lines hit count is roughly half of the 200 lines hit count.
			// Anything more is considered an exponential hit growth and needs to fixed.
			TestDBHits_FinalizeTransferLinesWithPalletIDs_ManyLinesCore(typeof(WhsLocation));
		}

		public void TestDBHits_FinalizeTransferLinesWithPalletIDs_ManyLines_WhsInventoryView()
		{
			// It is expected that the 100 lines hit count is roughly half of the 200 lines hit count.
			// Anything more is considered an exponential hit growth and needs to fixed.
			TestDBHits_FinalizeTransferLinesWithPalletIDs_ManyLinesCore(typeof(WhsInventoryView));
		}

		void TestDBHits_FinalizeTransferLinesWithPalletIDs_ManyLinesCore(Type bizOTypeToTest)
		{
			var data = new TestDataSimpleEnvironment(Factory, 8, 1);
			Factory.Save();
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "P123");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 50m, data.Whs1.FindLocation("A-2"), "P456");
			receive1.FinaliseDocketWithoutUserConfirmation();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 100m, data.Whs1.FindLocation("A-3"), "P111");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 100m, data.Whs1.FindLocation("A-4"), "P222");
			receive2.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			Assert("Receive1 should be finalised.", receive1.IsFinalised);
			Assert("Receive2 should be finalised.", receive2.IsFinalised);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			for (int i = 0; i < 50; i++)
			{
				Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "P123", "A-5", "P123");
				Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-2", "P456", "A-6", "P456");
			}
			transfer.RunPreSaveValidation(); // to commit inventory

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T2");
			for (int i = 0; i < 100; i++)
			{
				Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, "A-3", "P111", "A-7", "P111");
				Helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, "A-4", "P222", "A-8", "P222");
			}
			transfer2.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			CombineAssertions(() =>
			{
				var propertyHits100Lines = GetPersistentPropertiesHitCount(bizOTypeToTest, () =>
				{
					transfer.FinaliseDocketWithoutUserConfirmation();
				});
				Assert("Transfer should be finalised.", transfer.IsFinalised);

				var propertyHits200Lines = GetPersistentPropertiesHitCount(bizOTypeToTest, () =>
				{
					transfer2.FinaliseDocketWithoutUserConfirmation();
				});
				Assert("Transfer2 should be finalised.", transfer2.IsFinalised);

				Assert($"Expected that the 100 lines hit count '{propertyHits100Lines}' is roughly half of the 200 lines hit count '{propertyHits200Lines}'", (propertyHits100Lines * 2.05) >= propertyHits200Lines);
				/* Roughly because properties like WLV_LastInventoryChangeDate have checks like
				 * if (WLV_LastInventoryChangeDate.IsEmpty || now.CompareTo(WLV_LastInventoryChangeDate) > 0)
					 {
							WLV_LastInventoryChangeDate = now;
					 }
				which can change sporadically.
				*/
			});
		}

		public void TestLoad_FinalizeTransferLinesWithPalletIDs_ManyLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Factory.Save();
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "P123");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-2"), "P456");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-3"), "P789");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-4"), "P101");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			Assert("Receive should be finalised.", receive.IsFinalised);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			for (int i = 0; i < 100; i++)
			{
				Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "P123", "A-5", "P123");
				Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-2", "P456", "A-6", "P456");
				Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-3", "P789", "A-7", "P789");
				Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-4", "P101", "A-8", "P101");
			}
			transfer.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transferInOtherFactory = newfactory.Load<WhsTransfer>(transfer.PK);

			BusinessObjectFactory.StartLogging();
			transferInOtherFactory.FinaliseDocketWithoutUserConfirmation();
			var loadLog = BusinessObjectFactory.DebugLog;
			BusinessObjectFactory.StopLogging();

			CombineAssertions(() =>
			{
				var logCount = Regex.Matches(loadLog, Regex.Escape("WE_StockOnHand > 0 and WE_PalletID <> '' and WE_PalletID =")).Count;
				AssertEquals("Should load minimum Pallets.", 4, logCount);

				var allLoadedBusinessObjects = ((IBusinessObjectFactoryInternals)newfactory).AllBusinessObjects;
				AssertEquals("Should not load any additional inventories.", 404, allLoadedBusinessObjects.OfType<WhsInventoryView>().Count());
				AssertEquals("Should not load any additional receive lines.", 404, allLoadedBusinessObjects.OfType<WhsDocketLine>().Count());
				AssertEquals("Should not load any additional receives.", 2, allLoadedBusinessObjects.OfType<WhsDocket>().Count());
			});
		}

		#endregion

		#region TestCheckWE_PalletID -- Inner-Warehouse

		#region TestCheckWE_PalletID_InnerWhsTransfer_PartialWithPalletID_Fails

		public void TestCheckWE_PalletID_InnerWhsTransfer_PartialWithPalletID_Fails()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			// receive 100 units with Pallet ID "P123" into Whs1.A-1
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "P123");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// FAIL: attempt partial transfer from A-1 to A-2.
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var transferLine_DifferentLocation = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "P123", "A-2", "P123");
			transferLine_DifferentLocation.FinaliseDocketLine();
			AssertEquals("Transfer line should not be finalised if Pallet is transferred partially into different location.", false, transferLine_DifferentLocation.IsFinalised);
			AssertHasError(transferLine_DifferentLocation.WE_PalletIDInfo, "This transfer would split the Pallet ID into multiple locations. You must transfer ALL units of this Pallet ID at once.");

			// ensure we can finalise if the destination PalletID is removed
			transferLine_DifferentLocation.WE_PalletID = "";
			transferLine_DifferentLocation.FinaliseDocketLine();
			AssertEquals("Transfer line should be finalised if no Destination Pallet specified.", true, transferLine_DifferentLocation.IsFinalised);
			AssertNoErrors(transferLine_DifferentLocation.WE_PalletIDInfo);

			// ensure we can do partial transfer of Pallet ID if source and destination locations are the same.
			var transferLine_SameLocation = Helper.CreateWhsTransferLine(transfer, data.Part1, 25m, "A-1", "P123", "A-1", "P123");
			transferLine_SameLocation.FinaliseDocketLine();
			AssertEquals("Transfer line should be finalised if Pallet is transferred partially into same location.", true, transferLine_SameLocation.IsFinalised);
			AssertNoErrors(transferLine_SameLocation.WE_PalletIDInfo);
		}

		#endregion

		#region TestCheckWE_PalletID_InnerWhsTransfer_PartialWithPalletID_WhenPicked

		public void TestCheckWE_PalletID_InnerWhsTransfer_PartialWithPalletID_WhenPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			// receive 100 units with Pallet ID "P123" into Whs1.A-1
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "P123");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// FAIL: attempt partial transfer from A-1 to A-2 with half the Pallet Picked
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var transferLine_DifferentLocation = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "P123", "A-2", "P123");
			transferLine_DifferentLocation.PickedTime = ZDateTimeOffset.Now;
			transferLine_DifferentLocation.FinaliseDocketLine();
			AssertEquals("Transfer line should not be finalised if Pallet is transferred partially into different location.", false, transferLine_DifferentLocation.IsFinalised);
			AssertHasError(transferLine_DifferentLocation.WE_PalletIDInfo, "This transfer would split the Pallet ID into multiple locations. You must transfer ALL units of this Pallet ID at once.");
		}

		#endregion

		#region TestCheckWE_PalletID_InnerWhsTransfer_ToMultiLocationsWithPalletID_Fails

		public void TestCheckWE_PalletID_InnerWhsTransfer_ToMultiLocationsWithPalletID_Fails()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			// receive 100 units with Pallet ID "P123" into Whs1.A-1
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "P123");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			// FAIL: attempt transfer from A-1 to A-2 + A-3
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "A-3");
			transferLine1.WE_TransferFromPalletId = "P123";
			transferLine1.WE_PalletID = "P123";
			transferLine2.WE_TransferFromPalletId = "P123";
			transferLine2.WE_PalletID = "P123";
			transfer.FinaliseDocket();
			AssertHasError(transferLine1.WE_PalletIDInfo, "This transfer would cause the same Pallet ID to exist in multiple locations.");
			AssertHasError(transferLine2.WE_PalletIDInfo, "This transfer would cause the same Pallet ID to exist in multiple locations.");
			AssertEquals(false, transfer.IsFinalised);
		}

		#endregion

		#region TestCheckWE_PalletID_InnerWhsTransfer_ToSingleLocationMatchingExistingPalletID_Fails

		public void TestCheckWE_PalletID_InnerWhsTransfer_ToSingleLocationMatchingExistingPalletID_Fails()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			// receive 100 units with Pallet ID "P123" into Whs1.A-1
			// receive 100 units with Pallet ID "P456" into Whs1.A-3
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "P123");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-3"), "P456");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			// FAIL: attempt transfer from A-1 to A-2, changing dest PalletID to match an existing PalletID in A-3
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "A-2");
			transferLine.WE_TransferFromPalletId = "P123";
			transferLine.WE_PalletID = "P456";
			transfer.FinaliseDocket();
			AssertHasError(transferLine.WE_PalletIDInfo, "Another location (A-3) was already used for the same Pallet ID. Please select another location or Pallet ID.");
			AssertEquals(false, transfer.IsFinalised);
		}

		#endregion

		#region TestCheckWE_PalletID_InnerWhsTransfer_ToSingleLocationWithPalletID_Succeeds

		public void TestCheckWE_PalletID_InnerWhsTransfer_ToSingleLocationWithPalletID_Succeeds()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			// receive 100 units with Pallet ID "P123" into Whs1.A-1
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "P123");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "P123");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			// OK: attempt full transfer from A-1 to A-2
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", "A-2");
			transferLine.WE_TransferFromPalletId = "P123";
			transferLine.WE_PalletID = "P123";
			transfer.FinaliseDocket();
			AssertNoErrors(transferLine.WE_PalletIDInfo);
			AssertEquals(true, transfer.IsFinalised);
		}

		#endregion

		#region TestCheckWE_PalletID_InnerWhsTransfer_ToSingleLocationMatchingExistingPalletID_Staged

		public void TestCheckWE_PalletID_InnerWhsTransfer_ToSingleLocationMatchingExistingPalletID_Staged()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, data.Whs1.DefaultLocation, "PLT-123");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			Helper.CreatePickNew(order1);

			// Make all 20m units on PLT-123 Staged
			var pickLine = order1.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_PalletID = "PLT-123";
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			// Receive another pallet into "A-1"
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, data.Whs1.FindLocation("A-1"), "PLT-456");
			Factory.Save();

			// FAIL: attempt transfer from A-1 to A-2, changing dest PalletID to match an existing PalletID in the dock door
			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T2");
			var transfer2Line = Helper.CreateWhsTransferLine(transfer2, data.Part1, 20m, "A-1", "A-2");
			transfer2Line.WE_TransferFromPalletId = "PLT-456";
			transfer2Line.WE_PalletID = "PLT-123";
			transfer2.FinaliseDocket();
			AssertHasError(transfer2Line.WE_PalletIDInfo, "Another location (DOCKDOOR) was already used for the same Pallet ID. Please select another location or Pallet ID.");
			AssertEquals("Should have failed to finalise.", false, transfer2.IsFinalised);
		}

		#endregion

		#region TestCheckWE_PalletID_InnerWhsTransfer_SourceAndDestinationPalletIsDifferent_PalletExistsInAnotherWhs

		public void TestCheckWE_PalletID_InnerWhsTransfer_SourceAndDestinationPalletIsDifferent_PalletExistsInAnotherWhs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationB = whs2.FindLocation("B");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R2", data.Part1, 10m, locationB, "PLT-1");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "", "A-2", "PLT-1");
			transferLine.FinaliseDocketLine();
			AssertNoErrors("Destination Pallet ID should have no errors.", transferLine.WE_PalletIDInfo);
			AssertEquals("Transfer Line should be finalised.", true, transferLine.IsFinalised);
		}

		#endregion

		#region TestCheckWE_PalletID_OnlyRunWhenFinalisedOrFinalising

		public void TestCheckWE_PalletID_OnlyRunWhenFinalisedOrFinalising()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			// receive 100 units with Pallet ID "P123" into Whs1.A-1
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "P123");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			// FAIL: attempt partial transfer from A-1 to A-2.
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "A-2");
			transferLine.WE_TransferFromPalletId = "P123";
			transferLine.WE_PalletID = "P123";

			transferLine.Validation.ValidateWE_PalletID();
			AssertNoErrors("Validation should only be run when finalising (due to cost).", transferLine.WE_PalletIDInfo);

			transfer.FinaliseDocket();
			AssertHasErrors("If this fails, the previous assertion is not proven.", transferLine.WE_PalletIDInfo);
		}

		#endregion

		#region TestCheckWE_PalletID_AllowToReusePalletID

		public void TestCheckWE_PalletID_AllowToReusePalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m, data.Whs1.FindLocation("A-1"), "PLT-2"); // PLT-2 to have 0 units in stock
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// creating one inventory with 0 TotalUnits. To ensure we filter them out.
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 30m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "PLT-1", "A-2", "PLT-2");
			transferLine.FinaliseDocketLine();
			AssertEquals("Transfer line should be finalised without issues.", true, transferLine.IsFinalised);
			AssertNoErrors("System should allow to reuse Pallet ID's that have no current stock.", transferLine.WE_PalletIDInfo);
		}

		#endregion

		#region TestCheckWE_PalletID_InTransitInventory_FinaliseForSingleLine

		public void TestCheckWE_PalletID_InTransitInventory_FinaliseForSingleLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLine = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 100m, locationA, "PLT123", locationB, "PLT123", picker);
			Factory.Save();

			transfer.FinaliseDocket();
			AssertEquals("Transfer should be finalized.", true, transfer.IsFinalised);
			AssertEquals("Inventory should be available.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
		}

		#endregion

		#region TestCheckWE_PalletID_InTransitInventory_FinaliseForMultipleLines

		public void TestCheckWE_PalletID_InTransitInventory_FinaliseForMultipleLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLine1 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 40m, locationA, "PLT123", locationB, "PLT123", picker);
			var transferLine2 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 60m, locationA, "PLT123", locationB, "PLT123", picker);
			Factory.Save();

			transferLine1.FinaliseDocketLine();
			AssertEquals("TransferLine1 should be finalized.", true, transferLine1.IsFinalised);
			AssertEquals("Inventory should be available for line 1.", InventoryStatus.Codes.Available, transferLine1.WE_OriginalInventoryStatus);

			transferLine2.FinaliseDocketLine();
			AssertEquals("TransferLine2 should be finalized.", true, transferLine2.IsFinalised);
			AssertEquals("Inventory should be available for line 2.", InventoryStatus.Codes.Available, transferLine2.WE_OriginalInventoryStatus);
		}

		#endregion

		#region TestCheckWE_PalletID_InTransitInventory_TransferToMultipleLocations

		public void TestCheckWE_PalletID_InTransitInventory_TransferToMultipleLocations()
		{
			// Testing CheckDestinationPalletIDIsNotTransferringToMultipleLocations for In-Transit scenarios.
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var locationC = data.Whs1.FindLocation("A-3");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");
			var expectedErrorMsg = "This transfer would cause the same Pallet ID to exist in multiple locations.";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLine1 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 40m, locationA, "PLT123", locationB, "PLT123", picker);
			var transferLine2 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 60m, locationA, "PLT123", locationC, "PLT123", picker);
			Factory.Save();

			transfer.FinaliseDocket();
			AssertEquals("Docket should not be finalized.", false, transfer.IsFinalised);
			AssertEquals("Transfer Line 1 should be in transit.", InventoryStatus.Codes.InTransit, transferLine1.WE_OriginalInventoryStatus);
			AssertHasError("Transfer Line 1 should have error related to split pallet into many locations.", transferLine1.WE_PalletIDInfo, expectedErrorMsg);

			AssertEquals("Transfer Line 2 should be in transit.", InventoryStatus.Codes.InTransit, transferLine2.WE_OriginalInventoryStatus);
			AssertHasError("Transfer Line 2 should have error related to split pallet into many locations.", transferLine2.WE_PalletIDInfo, expectedErrorMsg);
		}

		#endregion

		#region TestCheckWE_PalletID_InTransitInventory_MultiStageTransfer

		public void TestCheckWE_PalletID_InTransitInventory_MultiStageTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var locationC = data.Whs1.FindLocation("A-3");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLineStage1 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 100m, locationA, "PLT123", locationB, "PLT123", picker);
			transferLineStage1.FinaliseDocketLine();

			AssertEquals("transfer line should be finalized.", true, transferLineStage1.IsFinalised);
			Factory.Save();

			var transferLineStage2 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 100m, locationB, "PLT123", locationC, "PLT123", picker);
			transferLineStage2.FinaliseDocketLine();
			AssertEquals("transfer line should be finalized.", true, transferLineStage2.IsFinalised);
		}

		public void TestCheckWE_PalletID_InTransitInventory_MultipleInventories_MultiStageTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var locationC = data.Whs1.FindLocation("A-3");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive2", data.Part2, 100m, locationA, "PLT123");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLine1Stage1 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 100m, locationA, "PLT123", locationB, "PLT123", picker);
			var transferLine2Stage1 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part2, 100m, locationA, "PLT123", locationB, "PLT123", picker);
			transferLine1Stage1.FinaliseDocketLine();
			transferLine2Stage1.FinaliseDocketLine();

			AssertEquals("transfer line should be finalized.", true, transferLine1Stage1.IsFinalised);
			AssertEquals("transfer line should be finalized.", true, transferLine2Stage1.IsFinalised);
			Factory.Save();

			var transferLine1Stage2 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 100m, locationB, "PLT123", locationC, "PLT123", picker);
			var transferLine2Stage2 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part2, 100m, locationB, "PLT123", locationC, "PLT123", picker);
			transferLine1Stage2.FinaliseDocketLine();
			AssertEquals("transfer line should be finalized.", true, transferLine1Stage2.IsFinalised);

			transferLine2Stage2.FinaliseDocketLine();
			AssertEquals("transfer line should be finalized.", true, transferLine2Stage2.IsFinalised);
		}

		#endregion

		#region TestCheckWE_PalletID_InTransitInventory_ExistInLocationLocalWhs

		public void TestCheckWE_PalletID_InTransitInventory_ExistInLocationLocalWhs()
		{
			// Testing CheckDestinationPalletIdNotAlreadyInAnotherLocation_InterWhsXfer for In-Transit scenarios.
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var locationC = data.Whs1.FindLocation("A-3");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");
			var expectedErrorMsg = "Another location (A-3-1) was already used for the same Pallet ID. Please select another location or Pallet ID.";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLine = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 100m, locationA, "PLT123", locationB, "PLT123", picker);
			Factory.Save();

			var otherReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Rec2", data.Part1, 100m, locationC, "PLT456");
			Factory.Save();

			transferLine.WE_PalletID = "PLT456";
			transfer.FinaliseDocket();
			AssertEquals("Original Receipt should be finalized.", true, receive.IsFinalised);
			AssertEquals("Receipt for location C should be finalized.", true, otherReceive.IsFinalised);
			AssertEquals("Transfer should not be finalized.", false, transfer.IsFinalised);
			AssertHasError("Transfer should have error related to split pallet into many locations.", transferLine.WE_PalletIDInfo, expectedErrorMsg);
		}

		#endregion

		#region TestCheckWE_PalletID_InTransitInventory_ExistInLocationInterWhs

		public void TestCheckWE_PalletID_InTransitInventory_ExistInLocationInterWhs()
		{
			// Objective: Make sure that a PalletID exist only in one location in destination Whs.
			var data = new TestDataSimpleEnvironment(Factory, 2, 1, saveFactory_doNotUseForNewTests: false);
			var dstWhs = Helper.CreateWarehouse("W2", "B", 2, 1);
			Factory.Save();

			var locationA = data.Whs1.FindLocation("A-1");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");
			var expectedErrorMsg = "This transfer would cause the same Pallet ID to exist in multiple locations.";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1", Notify, TransferType.Codes.InterWhsSource);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 50m, "A-1", "PLT123", dstWhs.PK, "B-1", "PLT123", ZDateTimeOffset.Empty);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 50m, "A-1", "PLT123", dstWhs.PK, "B-2", "PLT123", ZDateTimeOffset.Empty);

			transferLine1.GS_NKPickedBy = picker.GS_Code;
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.GS_NKPickedBy = picker.GS_Code;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transfer.FinaliseDocket();

			AssertEquals("Transfer must not be finalized.", false, transfer.IsFinalised);
			AssertHasError("Transfer must have error in Line 1", transferLine1.WE_PalletIDInfo, expectedErrorMsg);
			AssertHasError("Transfer must have error in Line 2", transferLine2.WE_PalletIDInfo, expectedErrorMsg);
		}

		#endregion

		#region TestCheckWE_PalletID_InTransitInventory_AvoidPartialTransfer

		public void TestCheckWE_PalletID_InTransitInventory_AvoidPartialTransfer()
		{
			// Testing CheckPalletIDCannotBeTransferredUnlessAllUnitsAreTransferred for In-Transit scenarios.
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");
			var expectedErrorMsg = "This transfer would split the Pallet ID into multiple locations. You must transfer ALL units of this Pallet ID at once.";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLine = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 30m, locationA, "PLT123", locationB, "PLT123", picker);
			Factory.Save();
			AssertEquals("Precondition: Transfer should NOT be finalized.", false, transferLine.IsFinalised);
			AssertNoErrors("", transferLine.WE_PalletIDInfo);

			transferLine.FinaliseDocketLine();
			AssertEquals("Transfer should NOT be finalized.", false, transferLine.IsFinalised);
			AssertHasError("Transfer should have error", transferLine.WE_PalletIDInfo, expectedErrorMsg);
		}

		#endregion

		#endregion

		#region TestCheckWE_TransferFromPalletID & TestCheckWE_PalletID -- Inter-Warehouse

		#region TestCheckWE_PalletID_InterWhsTransfer_ToMultiLocationsWithPalletID_Fails

		public void TestCheckWE_PalletID_InterWhsTransfer_ToMultiLocationsWithPalletID_Fails()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1, saveFactory_doNotUseForNewTests: false);
			var dstWhs = Helper.CreateWarehouse("W2", "A", 2, 1);
			Factory.Save();

			AssertCheckWE_PalletID_InterWhsTransfer_ToMultiLocationsWithPalletID_Fails(data, dstWhs, TransferType.Codes.InterWhsSource, WhsDocketLineSchema.WE_PalletID);
			AssertCheckWE_PalletID_InterWhsTransfer_ToMultiLocationsWithPalletID_Fails(data, dstWhs, TransferType.Codes.InterWhsDest, WhsDocketLineSchema.WE_PalletID);
		}

		void AssertCheckWE_PalletID_InterWhsTransfer_ToMultiLocationsWithPalletID_Fails(TestDataSimpleEnvironment data, WhsWarehouse dstWhs, ZString interWhsTransferType, SchemaColumn fieldToTest)
		{
			var srcWhs = data.Whs1;
			var srcLocation = srcWhs.FindLocation("A-1");

			// receive 100 units with Pallet ID "P123" into Whs1.A-1
			var receive = Helper.CreateWhsReceive(data.Org1, srcWhs, interWhsTransferType, Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, srcLocation, "P123");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			// FAIL: attempt transfer from Whs1.A-1 to Whs2.A-1 + Whs2.A-2
			WhsWarehouse transferSrcWhs, transferDstWhs;
			SetTransferWhs(interWhsTransferType, srcWhs, dstWhs, out transferSrcWhs, out transferDstWhs);

			var transfer = Helper.CreateWhsTransfer(data.Org1, transferSrcWhs, "T1", Notify, interWhsTransferType);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 50m, "A-1", "P123", transferDstWhs.PK, "A-1", "P123", ZDateTimeOffset.Empty);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 50m, "A-1", "P123", transferDstWhs.PK, "A-2", "P123", ZDateTimeOffset.Empty);

			transfer.FinaliseDocket();
			AssertHasError(transferLine1.ZPropertyInfoHash[fieldToTest.Name], "This transfer would cause the same Pallet ID to exist in multiple locations.");
			AssertHasError(transferLine2.ZPropertyInfoHash[fieldToTest.Name], "This transfer would cause the same Pallet ID to exist in multiple locations.");
			AssertEquals(false, transfer.IsFinalised);
		}

		#endregion

		#region TestCheckWE_PalletID_ToMultiLocationsWithPalletID_PutawayTransfers

		public void TestCheckWE_PalletID_ToMultiLocationsWithPalletID_PutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-3");
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty, Notify);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "P123", 1m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation1, "P123", 1m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPart1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, nonDockDoorLocation, "P123", 1m);
			var transferLineForPart2 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part2, dockDoorLocation1, nonDockDoorLocation, "P123", 1m);
			transferLineForPart1.WE_WL = nonDockDoorLocation.PK;
			transferLineForPart2.WE_WL = nonDockDoorLocation.PK;
			transfer.FinaliseDocket();
			AssertNoErrors(transferLineForPart1.WE_PalletIDInfo);
			AssertNoErrors(transferLineForPart2.WE_PalletIDInfo);
			AssertEquals(true, transfer.IsFinalised);
		}

		#endregion

		#region TestCheckWE_PalletID_InterWhsTransfer_WhenPalletIdExistsInOtherLocation_Fails

		public void TestCheckWE_PalletID_InterWhsTransfer_WhenPalletIdExistsInOtherLocation_Fails()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			AssertCheckWE_PalletID_InterWhsTransfer_WhenPalletIdExistsInOtherLocation_Fails(TransferType.Codes.InterWhsSource, data);
			AssertCheckWE_PalletID_InterWhsTransfer_WhenPalletIdExistsInOtherLocation_Fails(TransferType.Codes.InterWhsDest, data);
		}

		void AssertCheckWE_PalletID_InterWhsTransfer_WhenPalletIdExistsInOtherLocation_Fails(ZString interWhsTransferType, TestDataSimpleEnvironment data)
		{
			var srcWhs = data.Whs1;
			var dstWhs = Helper.CreateWarehouse("W2", "A", 2, 1);
			Factory.Save();

			// receive 100 units with Pallet ID "P123" into Whs1.A-1
			var receive1 = Helper.CreateWhsReceive(data.Org1, srcWhs, interWhsTransferType + "1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 100m, srcWhs.FindLocation("A-1"), "P123");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			// receive 100 units with Pallet ID "P123" into Whs1.A-1
			// receive 100 units with Pallet ID "P123" into Whs2.A-1
			var receive2 = Helper.CreateWhsReceive(data.Org1, dstWhs, interWhsTransferType + "2", Notify);
			var receiveLine2a = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 100m, dstWhs.FindLocation("A-1"), "P123");
			var receiveLine2b = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 100m, dstWhs.FindLocation("A-1"), "");
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);

			// FAIL: attempt any (partial of full) transfer from Whs1.A-1 to Whs2.A-2, when Whs2.A-1 already has the PalletID
			WhsWarehouse transferSrcWhs, transferDstWhs;
			SetTransferWhs(interWhsTransferType, srcWhs, dstWhs, out transferSrcWhs, out transferDstWhs);

			var transfer = Helper.CreateWhsTransfer(data.Org1, srcWhs, "T1", Notify, interWhsTransferType);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", dstWhs.PK, "A-2");
			transferLine.WE_TransferFromPalletId = "P123";
			transferLine.WE_PalletID = "P123";
			transfer.FinaliseDocket();
			AssertHasError(transferLine.WE_PalletIDInfo, "Another location (A-1) was already used for the same Pallet ID. Please select another location or Pallet ID.");
			AssertEquals(false, transfer.IsFinalised);

			// ensure we can finalise if the destination PalletID is removed
			Factory.Save(); // to make received stock available
			transferLine.WE_PalletID = "";
			transfer.FinaliseDocket();
			AssertNoErrors("No destination PalletID is entered thus validation should succeed.", transferLine.WE_PalletIDInfo);
			AssertEquals(true, transfer.IsFinalised);
		}

		#endregion

		#region TestCheckWE_PalletID_InterWhsTransfer_ToSingleLocationWithPalletID_Succeeds

		public void TestCheckWE_PalletID_InterWhsTransfer_ToSingleLocationWithPalletID_Succeeds()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			AssertCheckWE_PalletID_InterWhsTransfer_ToSingleLocationWithPalletID_Succeeds(TransferType.Codes.InterWhsSource, WhsDocketLineSchema.WE_TransferFromPalletId, data);
			AssertCheckWE_PalletID_InterWhsTransfer_ToSingleLocationWithPalletID_Succeeds(TransferType.Codes.InterWhsDest, WhsDocketLineSchema.WE_TransferFromPalletId, data);
		}

		void AssertCheckWE_PalletID_InterWhsTransfer_ToSingleLocationWithPalletID_Succeeds(ZString interWhsTransferType, SchemaColumn fieldToTest, TestDataSimpleEnvironment data)
		{
			var srcWhs = data.Whs1;
			var dstWhs = Helper.CreateWarehouse("W2", "A", 2, 1);
			Factory.Save();

			// receive 100 units with Pallet ID "P123" into Whs1.A-1
			var receive = Helper.CreateWhsReceive(data.Org1, srcWhs, interWhsTransferType, Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, srcWhs.FindLocation("A-1"), "P123");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, srcWhs.FindLocation("A-1"), "P123");
			receive.FinaliseDocket();
			var errors = receive.GetErrors();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// OK: attempt full transfer from Whs1.A-1 to Whs2.A-1
			WhsWarehouse transferSrcWhs, transferDstWhs;
			SetTransferWhs(interWhsTransferType, srcWhs, dstWhs, out transferSrcWhs, out transferDstWhs);

			var transfer = Helper.CreateWhsTransfer(data.Org1, transferSrcWhs, "T1", Notify, interWhsTransferType);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", transferDstWhs.PK, "A-2");
			transferLine.WE_TransferFromPalletId = "P123";
			transferLine.WE_PalletID = "P123";
			transfer.FinaliseDocket();
			AssertNoErrors(transferLine.ZPropertyInfoHash[fieldToTest.Name]);
			AssertEquals(true, transfer.IsFinalised);
		}

		#endregion

		#region TestCheckWE_PalletID_InterWhsTransfer_WhenPalletIdExistsInOtherLocation_Staged

		public void TestCheckWE_PalletID_InterWhsTransfer_WhenPalletIdExistsInOtherLocation_InterWhsSource_Staged()
		{
			AssertCheckWE_PalletID_InterWhsTransfer_WhenPalletIdExistsInOtherLocation_Staged(TransferType.Codes.InterWhsSource);
		}

		public void TestCheckWE_PalletID_InterWhsTransfer_WhenPalletIdExistsInOtherLocation_InterWhsDest_Staged()
		{
			AssertCheckWE_PalletID_InterWhsTransfer_WhenPalletIdExistsInOtherLocation_Staged(TransferType.Codes.InterWhsDest);
		}

		void AssertCheckWE_PalletID_InterWhsTransfer_WhenPalletIdExistsInOtherLocation_Staged(ZString interWhsTransferType)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var srcWhs = data.Whs1;
			var dstWhs = Helper.CreateWarehouse("W2", "A");
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, srcWhs, "R1" + interWhsTransferType, data.Part1, 20m, srcWhs.FindLocation("A"), "PLT-123");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, dstWhs, "R2", data.Part1, 20m, dstWhs.FindLocation("A"), "PLT-123");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, dstWhs, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order1);

			// Make all 20m units on PLT-123 in destination warehouse Staged
			var pickLine = order1.Lines[0].PickLines.Single();
			var dockDoorTransferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			dockDoorTransferLine.WE_PalletID = "PLT-123";
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, dockDoorTransferLine.WE_CurrentInventoryStatus);
			dockDoorTransferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, dockDoorTransferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			// FAIL: attempt any transfer from Whs1.A to Whs2.A, when Whs2.DockDoorLocation already has the PalletID
			WhsWarehouse transferSrcWhs, transferDstWhs;
			SetTransferWhs(interWhsTransferType, srcWhs, dstWhs, out transferSrcWhs, out transferDstWhs);

			var transfer = Helper.CreateWhsTransfer(data.Org1, transferSrcWhs, "T1", Notify, interWhsTransferType);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A", transferDstWhs.PK, "A");
			transferLine.WE_TransferFromPalletId = "PLT-123";
			transferLine.WE_PalletID = "PLT-123";
			transfer.FinaliseDocket();
			AssertHasError(transferLine.WE_PalletIDInfo, "Another location (DOCKDOOR) was already used for the same Pallet ID. Please select another location or Pallet ID.");
			AssertEquals(false, transfer.IsFinalised);

			// ensure we can finalise if the destination PalletID is removed
			transferLine.WE_PalletID = "";
			transfer.FinaliseDocket();
			AssertNoErrors("No destination PalletID is entered thus validation should succeed.", transferLine.WE_PalletIDInfo);
			AssertEquals(true, transfer.IsFinalised);
		}

		#endregion

		#region TestCheckWE_PalletID_InterWhsTransfer_PartialWithPalletID_Succeeds

		public void TestCheckWE_PalletID_InterWhsTransfer_PartialWithPalletID_Succeeds()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			AssertCheckWE_PalletID_InterWhsTransfer_PartialWithPalletID_Succeeds(TransferType.Codes.InterWhsSource, WhsDocketLineSchema.WE_TransferFromPalletId, data);
			AssertCheckWE_PalletID_InterWhsTransfer_PartialWithPalletID_Succeeds(TransferType.Codes.InterWhsDest, WhsDocketLineSchema.WE_TransferFromPalletId, data);
		}

		void AssertCheckWE_PalletID_InterWhsTransfer_PartialWithPalletID_Succeeds(ZString interWhsTransferType, SchemaColumn fieldToTest, TestDataSimpleEnvironment data)
		{
			var srcWhs = data.Whs1;
			var dstWhs = Helper.CreateWarehouse("W2", "A", 2, 1);
			Factory.Save();

			// receive 100 units with Pallet ID "P123" into Whs1.A-1
			var receive = Helper.CreateWhsReceive(data.Org1, srcWhs, interWhsTransferType, Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, srcWhs.FindLocation("A-1"), "P123");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, srcWhs.FindLocation("A-1"), "P123");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// OK: attempt partial transfer from Whs1.A-1 to Whs2.A-1
			WhsWarehouse transferSrcWhs, transferDstWhs;
			SetTransferWhs(interWhsTransferType, srcWhs, dstWhs, out transferSrcWhs, out transferDstWhs);

			var transfer = Helper.CreateWhsTransfer(data.Org1, transferSrcWhs, "T1", Notify, interWhsTransferType);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", transferDstWhs.PK, "A-1");
			transferLine.WE_TransferFromPalletId = "P123";
			transferLine.WE_PalletID = "P123";
			transfer.FinaliseDocket();
			AssertNoErrors("Partial transfers using the same Pallet ID between different Warehouses should be allowed.", transferLine.ZPropertyInfoHash[fieldToTest.Name]);
			AssertEquals("Transfer was not finalised. Partial transfers using the same Pallet ID between different Warehouses should be allowed.", true, transfer.IsFinalised);
		}

		#endregion

		#region TestCheckWE_PalletID_CheckPalletIDCannotBeTransferredIfItAlreadyExistsInAnotherLocationThatIsNotTransferSource

		public void TestCheckWE_PalletID_CheckPalletIDCannotBeTransferredIfItAlreadyExistsInAnotherLocationThatIsNotTransferSource()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			var sourceLocationForSecondTransfer = data.Whs1.FindLocation("A-3");
			var destLocationForSecondTransfer = data.Whs1.FindLocation("A-4");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "P1", 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "P1", 10m);
			transfer.FinaliseDocket();
			transfer.RunPreSaveValidation();
			Factory.Save();

			AssertIsFinalisedPrecondition(transfer);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, transferLine.WE_OriginalInventoryStatus);

			var invalidTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T2");
			invalidTransfer.WD_IsPutawayTransfer = true;
			var invalidTransferLine = Helper.SetupTransferLineForDockDoorLocation(invalidTransfer, data.Part1, sourceLocationForSecondTransfer, destLocationForSecondTransfer, "P1", 10m);
			invalidTransferLine.WE_TransferFromPalletId = "P1";
			invalidTransferLine.FinaliseDocketLine();

			AssertHasError("Should not be able to create a transfer on a pallet that exists in another location", invalidTransferLine.WE_PalletIDInfo, "Another location (A-2) was already used for the same Pallet ID. Please select another location or Pallet ID.");
		}

		#endregion

		#region TestCheckWE_PalletID_CheckPalletIDCannotBeTransferredUnlessAllUnitsAreTransferred_TransferFinalisation

		public void TestCheckWE_PalletID_CheckPalletIDCannotBeTransferredUnlessAllUnitsAreTransferred_TransferFinalisation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, locationA1, "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, locationA1, "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationA1, "PLT-2");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transfer1Line1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 2m, "A-1", "PLT-1", "A-2", "PLT-1");
			var transfer1Line2 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 3m, "A-1", "PLT-1", "A-2", "PLT-1");

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transfer2Line1 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 5m, "A-1", "PLT-2", "A-2", "PLT-2");
			var transfer2Line2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 5m, "A-1", "PLT-2", "A-2", "PLT-2");

			var expectedErrorMessage = "This transfer would split the Pallet ID into multiple locations. You must transfer ALL units of this Pallet ID at once.";

			transfer1.FinaliseDocket();
			AssertEquals("System should not allow finalise transfers that cause Pallet to be split between multiple locations.", false, transfer1.IsFinalised);
			AssertHasError(transfer1Line1.WE_PalletIDInfo, expectedErrorMessage);
			AssertHasError(transfer1Line2.WE_PalletIDInfo, expectedErrorMessage);

			transfer2.FinaliseDocket();
			AssertEquals("System should allow finalise transfers that transfer full Pallet.", true, transfer2.IsFinalised);
			AssertNoError(transfer2Line1.WE_PalletIDInfo, expectedErrorMessage);
			AssertNoError(transfer2Line2.WE_PalletIDInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_PalletID_CheckPalletIDCannotBeTransferredUnlessAllUnitsAreTransferred_MultipleTransferLinesFinalisation

		public void TestCheckWE_PalletID_CheckPalletIDCannotBeTransferredUnlessAllUnitsAreTransferred_MultipleTransferLinesFinalisation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT-2");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, "A-1", "PLT-1", "A-2", "PLT-1");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1", "PLT-1", "A-2", "PLT-1");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A-1", "PLT-1", "A-2", "PLT-1");
			var transferLine4 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A-1", "PLT-2", "A-2", "PLT-2");

			var expectedErrorMessage = "This transfer would split the Pallet ID into multiple locations. You must transfer ALL units of this Pallet ID at once.";

			transfer.ValidateAndFinaliseDocketLines(new WhsTransferLine[] { transferLine1, transferLine2 }, false);
			AssertEquals("System should not allow to finalise multiple lines that do not represent full pallet transfer.", false, transferLine1.IsFinalised);
			AssertEquals("System should not allow to finalise multiple lines that do not represent full pallet transfer.", false, transferLine2.IsFinalised);
			AssertEquals("System should not allow to finalise multiple lines that do not represent full pallet transfer.", false, transferLine3.IsFinalised);
			AssertHasError(transferLine1.WE_PalletIDInfo, expectedErrorMessage);
			AssertHasError(transferLine2.WE_PalletIDInfo, expectedErrorMessage);
			AssertNoError(transferLine3.WE_PalletIDInfo, expectedErrorMessage);

			transfer.ValidateAndFinaliseDocketLines(new WhsTransferLine[] { transferLine1, transferLine2, transferLine3 }, false);
			AssertEquals("System should finalise multiple lines that represent full pallet transfer.", true, transferLine1.IsFinalised);
			AssertEquals("System should finalise multiple lines that represent full pallet transfer.", true, transferLine2.IsFinalised);
			AssertEquals("System should finalise multiple lines that represent full pallet transfer.", true, transferLine3.IsFinalised);
			AssertNoError(transferLine1.WE_PalletIDInfo, expectedErrorMessage);
			AssertNoError(transferLine2.WE_PalletIDInfo, expectedErrorMessage);
			AssertNoError(transferLine3.WE_PalletIDInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_PalletID_CheckPalletIDCannotBeTransferredUnlessAllUnitsAreTransferred_SingleTransferLineFinalisation

		public void TestCheckWE_PalletID_CheckPalletIDCannotBeTransferredUnlessAllUnitsAreTransferred_SingleTransferLineFinalisation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, locationA1, "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, locationA1, "PLT-2");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 4m, "A-1", "PLT-1", "A-2", "PLT-1");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 6m, "A-1", "PLT-1", "A-2", "PLT-1");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-2", "A-2", "PLT-2");

			var expectedErrorMessage = "This transfer would split the Pallet ID into multiple locations. You must transfer ALL units of this Pallet ID at once.";

			transferLine2.FinaliseDocketLine();
			AssertEquals("System should not allow to finalise a single transfer line if it doesn't represent a full pallet transfer.", false, transferLine2.IsFinalised);
			AssertHasError(transferLine2.WE_PalletIDInfo, expectedErrorMessage);

			transferLine3.FinaliseDocketLine();
			AssertEquals("System should allow to finalise a single transfer line if it represent a full pallet transfer.", true, transferLine3.IsFinalised);
			AssertNoError(transferLine3.WE_PalletIDInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_TransferFromPalletId_OnlyRunWhenFinalisedOrFinalising

		public void TestCheckWE_TransferFromPalletId_OnlyRunWhenFinalisedOrFinalising()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1, saveFactory_doNotUseForNewTests: false);
			var srcWhs = data.Whs1;
			var dstWhs = Helper.CreateWarehouse("W2", "A", 2, 1);
			Factory.Save();

			// receive 100 units with Pallet ID "P123" into Whs1.A-1
			var receive = Helper.CreateWhsReceive(data.Org1, srcWhs, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, srcWhs.FindLocation("A-1"), "P123");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			// FAIL: attempt transfer from Whs1.A-1 to Whs2.A-1 + Whs2.A-2
			var transfer = Helper.CreateWhsTransfer(data.Org1, dstWhs, "T1", Notify, TransferType.Codes.InterWhsDest);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 50m, "A-1", "P123", srcWhs.PK, "A-1", "P123", ZDateTimeOffset.Empty);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 50m, "A-1", "P123", srcWhs.PK, "A-2", "P123", ZDateTimeOffset.Empty);

			transferLine1.Validation.ValidateWE_PalletID();
			AssertNoErrors("Validation should only be run when finalising (due to cost).", transferLine1.WE_PalletIDInfo);

			transfer.FinaliseDocket();
			AssertHasErrors("If this fails, the previous assertion is not proven.", transferLine1.WE_PalletIDInfo);
		}

		#endregion

		#region SetTransferWhs

		void SetTransferWhs(ZString interWhsTransferType, WhsWarehouse srcWhs, WhsWarehouse dstWhs, out WhsWarehouse transferSrcWhs, out WhsWarehouse transferDstWhs)
		{
			if (interWhsTransferType == TransferType.Codes.InterWhsSource)
			{
				transferSrcWhs = srcWhs;
				transferDstWhs = dstWhs;
			}
			else if (interWhsTransferType == TransferType.Codes.InterWhsDest)
			{
				transferSrcWhs = dstWhs;
				transferDstWhs = srcWhs;
			}
			else
			{
				throw new ArgumentException("Transfer type {0} is not supported by SetTransferWhsAndLocations().", interWhsTransferType);
			}
		}

		#endregion

		#endregion

		#region Locations

		#region TestCheckDestinationWarehousePK

		public void TestCheckDestinationWarehousePK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("2", "B");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "");
			transferLine.DestinationWarehousePK = whs2.PK;
			AssertNoErrors(transferLine.DestinationWarehousePKInfo);

			transferLine.DestinationWarehousePK = data.Whs1.PK;
			AssertHasError(transferLine.DestinationWarehousePKInfo, "The Source and Destination Warehouse cannot be the same on an Inter-Warehouse Transfer.");

			transferLine.DestinationWarehousePK = whs2.PK;
			AssertNoErrors(transferLine.DestinationWarehousePKInfo);

			transferLine.DestinationWarehousePK = ZGuid.Invalid; // guid set invalid when enter invalid warehouse
			AssertHasError(transferLine.DestinationWarehousePKInfo, "Enter a valid Destination Warehouse.");

			transferLine.DestinationWarehousePK = whs2.PK;
			AssertNoErrors(transferLine.DestinationWarehousePKInfo);
		}

		#endregion

		#region TestCheckTransferFromWarehousePK

		public void TestCheckTransferFromWarehousePK()
		{
			var whs = Helper.CreateWarehouse("WHS2");
			var transfer = Factory.New<WhsTransfer>();
			var line = transfer.Lines.AddNew();
			line.TransferFromWarehousePK = whs.PK;
			AssertNoErrors(nameof(line.TransferFromWarehousePK), line.TransferFromWarehousePKInfo);
		}

		#endregion

		#region TestCheckSourceLocationIsRequired

		public void TestCheckSourceLocationIsRequired()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "", "", "");
			AssertNoErrors(transferLine.TransferFromLocationStringInfo);

			transferLine.TransferFromLocationString = "A-1";
			AssertNoErrors(transferLine.TransferFromLocationStringInfo);

			transferLine.TransferFromLocationString = "";
			AssertHasError(transferLine.TransferFromLocationStringInfo, "Please enter a value.");
		}

		#endregion

		#region TestLocation_MandatoryForInterWhsSourceTransfer

		public void TestLocation_MandatoryForInterWhsSourceTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("Wh2", "A", 1, 1);

			var interWhsTransferSource = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			interWhsTransferSource.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var interWhsTransferSourceLine = Helper.CreateWhsTransferLine(interWhsTransferSource, data.Part1, 75m, "A-1", ZGuid.Empty, "");
			AssertHasError("Precondition: Destination Whs Mandatory.", interWhsTransferSourceLine.DestinationWarehousePKInfo, "Please enter a Destination Warehouse.");

			interWhsTransferSourceLine.DestinationWarehousePK = whs2.PK;
			AssertNoErrors("Precondition: No error about Destination Whs being mandatory.", interWhsTransferSourceLine.DestinationWarehousePKInfo);

			AssertNull("Precondition: No Location.", interWhsTransferSourceLine.Location);
			interWhsTransferSourceLine.Validation.ValidateLocationString();
			AssertHasError("Destination Location Should Be Mandatory As It Is Used To Persist Destination Warehouse.", interWhsTransferSourceLine.LocationStringInfo, "Please enter a Location.");

			interWhsTransferSource.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			interWhsTransferSourceLine.Validation.ValidateLocationString();
			AssertNoErrors("Other transfer types should not have errors.", interWhsTransferSourceLine.LocationStringInfo);

			interWhsTransferSource.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			interWhsTransferSourceLine.Validation.ValidateLocationString();
			AssertHasError("Precondition: Has error after changing type back.", interWhsTransferSourceLine.LocationStringInfo, "Please enter a Location.");

			interWhsTransferSourceLine.LocationString = whs2.DefaultLocation.WLV_LocationString;
			interWhsTransferSourceLine.Validation.ValidateLocationString();
			AssertNoError("Should be no errors after setting location.", interWhsTransferSourceLine.LocationStringInfo, "Please enter a Location.");
		}

		#endregion

		#region TestLocation_NotMandatoryForChildInterWhsSourceTransfer

		public void TestLocation_NotMandatoryForChildInterWhsSourceTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("Warehouse1", "A", 3, 3);
			Factory.Save();

			var sourceLocation = data.Whs1.FindLocation("A");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var interWhsDestTransfer = Helper.CreateWhsTransfer(data.Org1, whs2, "TR2", Notify, TransferType.Codes.InterWhsDest);
			var interWhsDestTransferLine = Helper.CreateWhsTransferLine(interWhsDestTransfer, data.Part1.PK, 10m, "A", data.Whs1.PK, "");
			interWhsDestTransferLine.PickedTime = ZDateTimeOffset.Now;

			var childLine = interWhsDestTransferLine.ChildTransferLine;
			AssertNotNull("Precondition: Create Child Line.", childLine);
			AssertEquals("Precondition: Child Line has no dest location.", ZGuid.Empty, childLine.WE_WL);

			childLine.Validation.ValidateLocationString();
			AssertNoErrors("Should have no errors.", childLine.LocationStringInfo);
		}

		#endregion

		#region TestCheckLocationIsNotVoid

		public void TestCheckLocationIsNotVoid()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 3, 1);
			Factory.Save();

			var docket = GetNewDocket();
			docket.WD_WW_Whs = whs.PK;
			var locations = whs.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[1].WLV_LocationStatus = LocationStatus.Codes.Void;
			locations[2].WLV_LocationStatus = LocationStatus.Codes.Void;
			docket.WD_DocketSubType = TransferType.Codes.Internal;

			var docketLine = docket.Lines.AddNew();
			docketLine.TransferFromLocationString = "A-2";
			AssertNoError(docketLine.TransferFromLocationStringInfo, WhsTransferLineValidation.ErrorTransferIntoVoidLocation);

			docketLine.LocationString = "A-2";
			AssertHasError(docketLine.LocationStringInfo, WhsTransferLineValidation.ErrorTransferIntoVoidLocation);

			docket.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			docketLine.DestinationWarehousePK = whs.PK;
			docketLine.TransferFromLocationString = "A-3";
			AssertNoError(docketLine.TransferFromLocationStringInfo, WhsTransferLineValidation.ErrorTransferIntoVoidLocation);

			docketLine.LocationString = "A-3";
			AssertHasError(docketLine.LocationStringInfo, WhsTransferLineValidation.ErrorTransferIntoVoidLocation);

			docket.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			docketLine.LocationString = "A-2";
			AssertHasError(docketLine.LocationStringInfo, WhsTransferLineValidation.ErrorTransferIntoVoidLocation);

			docketLine.TransferFromLocationString = "A-2";
			AssertNoError(docketLine.TransferFromLocationStringInfo, WhsTransferLineValidation.ErrorTransferIntoVoidLocation);
		}

		#endregion

		#region TestCheckLocationForMaxWeightVolume

		#region TestCheckLocationForMaxWeight

		public void TestCheckLocationForMaxWeight()
		{
			var client = Helper.CreateClient();
			var whs1 = Helper.CreateWarehouse("WHS1", "A", 3, 1);
			var whs2 = Helper.CreateWarehouse("WHS2", "B", 3, 1);

			var locations1 = whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			Helper.SetLocationMaxWeightAndVolume(locations1[0], 0m, "", 0m, ""); // A-1
			Helper.SetLocationMaxWeightAndVolume(locations1[1], 100m, "KG", 0m, ""); // A-2
			Helper.SetLocationMaxWeightAndVolume(locations1[2], 100m, "KG", 0m, ""); // A-3

			var locations2 = whs2.Rows.Single(r => r.WR_Name == "B").Locations;
			Helper.SetLocationMaxWeightAndVolume(locations2[0], 0m, "", 0m, ""); // B-1
			Helper.SetLocationMaxWeightAndVolume(locations2[1], 100m, "KG", 0m, ""); // B-2
			Helper.SetLocationMaxWeightAndVolume(locations2[2], 100m, "KG", 0m, ""); // B-3

			var part1 = Helper.CreateProduct(client, "P1");
			var part2 = Helper.CreateProduct(client, "P2");
			Helper.SetProductWeightAndVolume(part1, 1m, "KG", 0m, "");
			Helper.SetProductWeightAndVolume(part2, 1000m, "G", 0m, "");

			var receive1 = Helper.CreateWhsReceive(client, whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, part1, 20m, locations1[0]); // A-1
			Helper.CreateWhsReceiveInventoryLine(receive1, part1, 55m, locations1[0]); // A-1
			Helper.CreateWhsReceiveInventoryLine(receive1, part2, 20m, locations1[1]); // A-2
			Helper.CreateWhsReceiveInventoryLine(receive1, part2, 55m, locations1[1]); // A-2
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(client, whs2, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, part1, 20m, locations2[0]); // B-1
			Helper.CreateWhsReceiveInventoryLine(receive2, part1, 55m, locations2[0]); // B-1
			Helper.CreateWhsReceiveInventoryLine(receive2, part2, 20m, locations2[1]); // B-2
			Helper.CreateWhsReceiveInventoryLine(receive2, part2, 55m, locations2[1]); // B-2
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			AssertCheckLocationForMaxWeightVolumeForAllTransferTypes(client, whs1, whs2, part1, part2, new string[] { "Total required Weight (", ") exceeds the maximum available Weight (", ") for this location." });
		}

		#endregion

		#region TestCheckLocationForMaxVolume

		public void TestCheckLocationForMaxVolume()
		{
			var client = Helper.CreateClient();
			var whs1 = Helper.CreateWarehouse("WHS1", "A", 3, 1);
			var whs2 = Helper.CreateWarehouse("WHS2", "B", 3, 1);

			var locations1 = whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			Helper.SetLocationMaxWeightAndVolume(locations1[0], 0m, "", 0m, ""); // A-1
			Helper.SetLocationMaxWeightAndVolume(locations1[1], 0m, "", 2m, "M3"); // A-2
			Helper.SetLocationMaxWeightAndVolume(locations1[2], 0m, "", 2m, "M3"); // A-3

			var locations2 = whs2.Rows.Single(r => r.WR_Name == "B").Locations;
			Helper.SetLocationMaxWeightAndVolume(locations2[0], 0m, "", 0m, ""); // B-1
			Helper.SetLocationMaxWeightAndVolume(locations2[1], 0m, "", 2m, "M3"); // B-2
			Helper.SetLocationMaxWeightAndVolume(locations2[2], 0m, "", 2m, "M3"); // B-3

			var part1 = Helper.CreateProduct(client, "P1");
			var part2 = Helper.CreateProduct(client, "P2");
			Helper.SetProductWeightAndVolume(part1, 0m, "", 20m, "D3");
			Helper.SetProductWeightAndVolume(part2, 0m, "", 0.02m, "M3");

			var receive1 = Helper.CreateWhsReceive(client, whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, part1, 20m, locations1[0]); // A-1
			Helper.CreateWhsReceiveInventoryLine(receive1, part1, 55m, locations1[0]); // A-1
			Helper.CreateWhsReceiveInventoryLine(receive1, part2, 20m, locations1[1]); // A-2
			Helper.CreateWhsReceiveInventoryLine(receive1, part2, 55m, locations1[1]); // A-2
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(client, whs2, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, part1, 20m, locations2[0]); // B-1
			Helper.CreateWhsReceiveInventoryLine(receive2, part1, 55m, locations2[0]); // B-1
			Helper.CreateWhsReceiveInventoryLine(receive2, part2, 20m, locations2[1]); // B-2
			Helper.CreateWhsReceiveInventoryLine(receive2, part2, 55m, locations2[1]); // B-2
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			AssertCheckLocationForMaxWeightVolumeForAllTransferTypes(client, whs1, whs2, part1, part2, new string[] { "Total required Volume (", ") exceeds the maximum available Volume (", ") for this location." });
		}

		#endregion

		#region TestCheckLocationForMaxQuantity

		public void TestCheckLocationForMaxQuantity()
		{
			var client = Helper.CreateClient();
			var whs1 = Helper.CreateWarehouse("WHS1", "A", 3, 1);
			var whs2 = Helper.CreateWarehouse("WHS2", "B", 3, 1);

			var locations1 = whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations1[0].WLV_MaxQuantity = 0m;     // A-1
			locations1[1].WLV_MaxQuantity = 100m;   // A-2
			locations1[2].WLV_MaxQuantity = 100m;   // A-3

			var locations2 = whs2.Rows.Single(r => r.WR_Name == "B").Locations;
			locations2[0].WLV_MaxQuantity = 0m;     // B-1
			locations2[1].WLV_MaxQuantity = 100m;   // B-2
			locations2[2].WLV_MaxQuantity = 100m;   // B-3

			var part1 = Helper.CreateProduct(client, "P1");
			var part2 = Helper.CreateProduct(client, "P2");

			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(client, whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, part1, 20m, locations1[0]); // A-1
			Helper.CreateWhsReceiveInventoryLine(receive1, part1, 55m, locations1[0]); // A-1
			Helper.CreateWhsReceiveInventoryLine(receive1, part2, 20m, locations1[1]); // A-2
			Helper.CreateWhsReceiveInventoryLine(receive1, part2, 55m, locations1[1]); // A-2
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(client, whs2, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, part1, 20m, locations2[0]); // B-1
			Helper.CreateWhsReceiveInventoryLine(receive2, part1, 55m, locations2[0]); // B-1
			Helper.CreateWhsReceiveInventoryLine(receive2, part2, 20m, locations2[1]); // B-2
			Helper.CreateWhsReceiveInventoryLine(receive2, part2, 55m, locations2[1]); // B-2
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			AssertCheckLocationForMaxWeightVolumeForAllTransferTypes(client, whs1, whs2, part1, part2, new[] { "Total required Quantity (", ") exceeds the maximum available Quantity (", ") for this location." }, true);
		}

		#endregion

		#region TestCheckLocationForMaxWeightVolume_DoesNotCauseNullReferencException

		[ExpectNoExceptions]
		public void TestCheckLocationForMaxWeightVolume_DoesNotCauseNullReferencException()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			Helper.SetLocationMaxWeightAndVolume(locations[1], 150m, "KG", 2m, "M3"); // A-2
			Helper.SetProductWeightAndVolume(data.Part1, 1000m, "G", 20m, "D3");
			Factory.Save();

			WhsTransfer transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = CodeLists.TransferType.Codes.Internal;

			WhsTransferLine transferLine1 = Helper.CreateWhsTransferLine(transfer, ZGuid.Empty, 200m, "A-1", "A-2"); // no product, and no warning, since unknown product weight/
			AssertEquals(false, transferLine1.LocationStringInfo.HasWarning("Total required Weight (0.00 ) exceeds the maximum available Weight (150.00 KG) for this location.")); // check only LocationString2 - destination location.
			AssertEquals(false, transferLine1.LocationStringInfo.HasWarning("Total required Volume (0.000 ) exceeds the maximum available Volume (2.000 M3) for this location."));

			WhsTransferLine transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 200m, "", "A-2"); // no Source Location, but still calculate destination location, both overfloow, but weight calculated first.
			AssertEquals(true, transferLine2.LocationStringInfo.HasWarning("Total required Weight (200.00 KG) exceeds the maximum available Weight (150.00 KG) for this location."));
			AssertEquals(false, transferLine2.LocationStringInfo.HasWarning("Total required Volume (4.000 M3) exceeds the maximum available Volume (2.000 M3) for this location."));

			WhsTransferLine transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 75m, "A-2", ""); // no Destination Location, but some taken from source location, should still be taken in account when calculating
			AssertEquals(false, transferLine3.LocationStringInfo.HasWarning("Total inventory Weight (0.00 ) exceeds the maximum available Weight (150.00 KG) for this location."));
			AssertEquals(false, transferLine3.LocationStringInfo.HasWarning("Total inventory Volume (0.000 ) exceeds the maximum available Volume (2.000 M3) for this location."));

			WhsTransferLine transferLine4 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2"); // should include 200m - 75m + 10m = 135m > 100m Volume < 150m Weight, expect Volume overfloow warning.
			AssertEquals(false, transferLine4.LocationStringInfo.HasWarning("Total required Weight (135.00 KG) exceeds the maximum available Weight (150.00 KG) for this location."));
			AssertEquals(true, transferLine4.LocationStringInfo.HasWarning("Total required Volume (2.700 M3) exceeds the maximum available Volume (2.000 M3) for this location."));
		}

		#endregion

		#region AssertCheckLocationForMaxWeightVolumeForAllTransferTypes

		void AssertCheckLocationForMaxWeightVolumeForAllTransferTypes(OrgHeader client, WhsWarehouse whs1, WhsWarehouse whs2,
			OrgSupplierPart part1, OrgSupplierPart part2, string[] warningMessageToExpect, bool isErrorExpected = false)
		{
			// ONLY DESTINATION LOCATIONS SHOULD EVER GET WARNINGS, WE DON'T CARE ABOUT SOURCE LOCATIONS
			// Internal Transfers
			var internalTransfer = Helper.CreateWhsTransfer(client, whs1);
			internalTransfer.WD_DocketSubType = CodeLists.TransferType.Codes.Internal;
			var internalTransferLine1 = Helper.CreateWhsTransferLine(internalTransfer, part1, 75m, "A-1", "A-2"); // 75m from Receive + 75m from A-1 > 100m max, should show warning.
			internalTransferLine1.RunPreSaveValidation();
			AssertEquals("Precondition: Inventory is committed.", 75m, internalTransferLine1.QtyCommittedIncludingMatchingLines);
			AssertEquals(false, HasMessageThatContainParts(internalTransferLine1.TransferFromLocationStringInfo, warningMessageToExpect, isErrorExpected));
			AssertEquals(true, HasMessageThatContainParts(internalTransferLine1.LocationStringInfo, warningMessageToExpect, isErrorExpected));
			// clean-up
			internalTransferLine1.MatchingLines.DeleteAll();
			internalTransferLine1.PickLines.DeleteAll();

			var internalTransferLine2 = Helper.CreateWhsTransferLine(internalTransfer, part1, 75m, "A-2", "A-3"); // 75m from A-2 < 100m max, should not show warning.
			AssertEquals(false, HasMessageThatContainParts(internalTransferLine2.TransferFromLocationStringInfo, warningMessageToExpect, isErrorExpected));
			AssertEquals(false, HasMessageThatContainParts(internalTransferLine2.LocationStringInfo, warningMessageToExpect, isErrorExpected));

			var internalTransferLine3 = Helper.CreateWhsTransferLine(internalTransfer, part2, 75m, "A-2", "A-3"); // 75m + 75m from A-2 > 100m max, should show warning.
			internalTransferLine3.RunPreSaveValidation();
			AssertEquals("Precondition: Inventory is committed.", 75m, internalTransferLine3.QtyCommittedIncludingMatchingLines);
			AssertEquals(false, HasMessageThatContainParts(internalTransferLine3.TransferFromLocationStringInfo, warningMessageToExpect, isErrorExpected));
			AssertEquals(true, HasMessageThatContainParts(internalTransferLine3.LocationStringInfo, warningMessageToExpect, isErrorExpected));
			// clean-up
			internalTransferLine3.MatchingLines.DeleteAll();
			internalTransferLine3.PickLines.DeleteAll();

			var internalTransferLine4 = Helper.CreateWhsTransferLine(internalTransfer, part2, 75m, "A-3", "A-2"); // 75m from Receive - (75m + 75m from A-2 to A-3) + 75m from A3 < 100m max, should not show warning.
			AssertEquals(false, HasMessageThatContainParts(internalTransferLine4.TransferFromLocationStringInfo, warningMessageToExpect, isErrorExpected));
			AssertEquals(false, HasMessageThatContainParts(internalTransferLine4.LocationStringInfo, warningMessageToExpect, isErrorExpected));

			var internalTransferLine5 = Helper.CreateWhsTransferLine(internalTransfer, part1, 150m, "A-3", "A-1");// 75m from Receive - 75m from A-1 to A-2 + 150m from A3 > 100m max, but no warning since no Max Weight specification for A-1
			AssertEquals(false, HasMessageThatContainParts(internalTransferLine5.TransferFromLocationStringInfo, warningMessageToExpect, isErrorExpected));
			AssertEquals(false, HasMessageThatContainParts(internalTransferLine5.LocationStringInfo, warningMessageToExpect, isErrorExpected));

			// Inter Transfer Source
			var interTransferSource = Helper.CreateWhsTransfer(client, whs1);
			interTransferSource.WD_DocketSubType = CodeLists.TransferType.Codes.InterWhsSource;
			var interTransferSourceLine1 = Helper.CreateWhsTransferLine(interTransferSource, part1, 75m, "A-1", whs2.PK, "B-2"); // 75m from Receive + 75m from A-1 > 100m max, should show warning.
			interTransferSourceLine1.RunPreSaveValidation();
			AssertEquals("Precondition: Inventory is committed.", 75m, interTransferSourceLine1.QtyCommittedIncludingMatchingLines);
			AssertEquals(false, HasMessageThatContainParts(interTransferSourceLine1.TransferFromLocationStringInfo, warningMessageToExpect, isErrorExpected));
			AssertEquals(true, HasMessageThatContainParts(interTransferSourceLine1.LocationStringInfo, warningMessageToExpect, isErrorExpected));
			//clean - up
			interTransferSourceLine1.MatchingLines.DeleteAll();
			interTransferSourceLine1.PickLines.DeleteAll();

			var interTransferSourceLine2 = Helper.CreateWhsTransferLine(interTransferSource, part1, 75m, "A-2", whs2.PK, "B-3"); // 75m from A-2 < 100m max, should not show warning.
			AssertEquals(false, HasMessageThatContainParts(interTransferSourceLine2.TransferFromLocationStringInfo, warningMessageToExpect, isErrorExpected));
			AssertEquals(false, HasMessageThatContainParts(interTransferSourceLine2.LocationStringInfo, warningMessageToExpect, isErrorExpected));

			var interTransferSourceLine3 = Helper.CreateWhsTransferLine(interTransferSource, part2, 75m, "A-2", whs2.PK, "B-3"); // 75m + 75m from A-2 > 100m max, should show warning.
			interTransferSourceLine3.RunPreSaveValidation();
			AssertEquals("Precondition: Inventory is committed.", 75m, interTransferSourceLine3.QtyCommittedIncludingMatchingLines);
			AssertEquals(false, HasMessageThatContainParts(interTransferSourceLine3.TransferFromLocationStringInfo, warningMessageToExpect, isErrorExpected));
			AssertEquals(true, HasMessageThatContainParts(interTransferSourceLine3.LocationStringInfo, warningMessageToExpect, isErrorExpected));
			// clean-up
			interTransferSourceLine3.MatchingLines.DeleteAll();
			interTransferSourceLine3.PickLines.DeleteAll();

			var interTransferSourceLine4 = Helper.CreateWhsTransferLine(interTransferSource, part1, 75m, "A-3", whs2.PK, "B-1"); // 75m from Receive + 75m from A-3 > 100m max, but no warning since no Max Weight specified for B-1.
			AssertEquals(false, HasMessageThatContainParts(interTransferSourceLine4.TransferFromLocationStringInfo, warningMessageToExpect, isErrorExpected));
			AssertEquals(false, HasMessageThatContainParts(interTransferSourceLine4.LocationStringInfo, warningMessageToExpect, isErrorExpected));

			// Inter Transfer Destination
			var interTransferDestination = Helper.CreateWhsTransfer(client, whs2);
			interTransferDestination.WD_DocketSubType = CodeLists.TransferType.Codes.InterWhsDest;
			var interTransferDestinationLine1 = Helper.CreateWhsTransferLine(interTransferDestination, part1, 75m, "A-1", whs1.PK, "B-2"); // 75m from Receive + 75m from A-1 > 100m max, should show warning.
			interTransferDestinationLine1.RunPreSaveValidation();
			AssertEquals("Precondition: Inventory is committed.", 75m, interTransferDestinationLine1.QtyCommittedIncludingMatchingLines);
			AssertEquals(false, HasMessageThatContainParts(interTransferDestinationLine1.TransferFromLocationStringInfo, warningMessageToExpect, isErrorExpected));
			AssertEquals(true, HasMessageThatContainParts(interTransferDestinationLine1.LocationStringInfo, warningMessageToExpect, isErrorExpected));
			// clean-up
			interTransferDestinationLine1.MatchingLines.DeleteAll();
			interTransferDestinationLine1.PickLines.DeleteAll();

			var interTransferDestinationLine2 = Helper.CreateWhsTransferLine(interTransferDestination, part1, 75m, "A-2", whs1.PK, "B-3"); // 75m from A-2 < 100m max, should not show warning.
			AssertEquals(false, HasMessageThatContainParts(interTransferDestinationLine2.TransferFromLocationStringInfo, warningMessageToExpect, isErrorExpected));
			AssertEquals(false, HasMessageThatContainParts(interTransferDestinationLine2.LocationStringInfo, warningMessageToExpect, isErrorExpected));

			var interTransferDestinationLine3 = Helper.CreateWhsTransferLine(interTransferDestination, part2, 75m, "A-2", whs1.PK, "B-3"); // 75m + 75m from A-2 > 100m max, should show warning.
			interTransferDestinationLine3.RunPreSaveValidation();
			AssertEquals("Precondition: Inventory is committed.", 75m, interTransferDestinationLine3.QtyCommittedIncludingMatchingLines);
			AssertEquals(false, HasMessageThatContainParts(interTransferDestinationLine3.TransferFromLocationStringInfo, warningMessageToExpect, isErrorExpected));
			AssertEquals(true, HasMessageThatContainParts(interTransferDestinationLine3.LocationStringInfo, warningMessageToExpect, isErrorExpected));
			// clean-up
			interTransferDestinationLine3.MatchingLines.DeleteAll();
			interTransferDestinationLine3.PickLines.DeleteAll();

			var interTransferDestinationLine4 = Helper.CreateWhsTransferLine(interTransferDestination, part1, 75m, "A-3", whs1.PK, "B-1"); // 75m from Receive + 75m from A-3 > 100m max, but no warning since no Max Weight specified for B-1.
			AssertEquals(false, HasMessageThatContainParts(interTransferDestinationLine4.TransferFromLocationStringInfo, warningMessageToExpect, isErrorExpected));
			AssertEquals(false, HasMessageThatContainParts(interTransferDestinationLine4.LocationStringInfo, warningMessageToExpect, isErrorExpected));
		}

		#endregion

		#endregion

		#region TestCheckBothLocationsAreDifferent

		public void TestCheckBothLocationsAreDifferent()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("1", "A", 2, 1);
			Factory.Save();
			Docket.WD_WW_Whs = whs.PK;

			DocketLine.LocationString = "";
			AssertNoWarning(DocketLine.LocationStringInfo, WhsTransferLineValidation.WarningLocationsAreSame);

			DocketLine.LocationString = "";
			AssertNoWarning(DocketLine.LocationStringInfo, WhsTransferLineValidation.WarningLocationsAreSame);

			DocketLine.TransferFromLocationString = "A-1";
			AssertNoWarning(DocketLine.TransferFromLocationStringInfo, WhsTransferLineValidation.WarningLocationsAreSame);

			DocketLine.LocationString = "A-2";
			AssertNoWarning(DocketLine.LocationStringInfo, WhsTransferLineValidation.WarningLocationsAreSame);

			DocketLine.TransferFromLocationString = "A-2";
			AssertHasWarning(DocketLine.TransferFromLocationStringInfo, WhsTransferLineValidation.WarningLocationsAreSame);

			DocketLine.TransferFromLocationString = "A-1";
			DocketLine.LocationString = "A-1";
			AssertHasWarning(DocketLine.LocationStringInfo, WhsTransferLineValidation.WarningLocationsAreSame);
		}

		#endregion

		#region TestCheckLocationForInternalTransfer

		public virtual void TestCheckLocationForInternalTransfer()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("1", "A", 2, 2);
			Factory.Save();

			whs.FindLocation("A-1-1").WLV_WA_PickingArea = whs.FindLocation("A-1-2").WLV_WA_PickingArea = Helper.CreateArea(whs, "Free", AreaTypes.Codes.FreeStore).PK;
			whs.FindLocation("A-2-1").WLV_WA_PickingArea = Helper.CreateArea(whs, "Bonded", AreaTypes.Codes.Bonded).PK;
			whs.FindLocation("A-2-2").WLV_WA_PickingArea = Helper.CreateArea(whs, "Excise", AreaTypes.Codes.Excise).PK;

			Docket.WD_WW_Whs = whs.PK;
			DocketLine.TransferFromLocationString = "A-1-1";
			DocketLine.LocationString = "A-1-2";
			AssertNoErrors(DocketLine.LocationStringInfo);

			DocketLine.TransferFromLocationString = "A-1-1";
			DocketLine.LocationString = "A-2-1";
			AssertNoWarning(DocketLine.LocationStringInfo, WhsTransferLineValidation.WarningLocationsAreSame);
			AssertHasError(DocketLine.LocationStringInfo, GetAreaError());

			DocketLine.TransferFromLocationString = "A-2-1";
			AssertHasWarning(DocketLine.TransferFromLocationStringInfo, WhsTransferLineValidation.WarningLocationsAreSame);

			DocketLine.LocationString = "A-2-2";
			AssertNoErrors(DocketLine.LocationStringInfo);

			DocketLine.TransferFromLocationString = "A-1-2";
			AssertNoWarning(DocketLine.TransferFromLocationStringInfo, WhsTransferLineValidation.WarningLocationsAreSame);
			AssertHasError(DocketLine.TransferFromLocationStringInfo, GetAreaError());
		}

		#endregion

		#region TestCheckLocationForInternalTransfer_DockDoorArea

		public void TestCheckLocationForInternalTransfer_DockDoorArea()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, data.Whs1.DefaultLocation);
			transferLine.Validation.ValidateTransferFromLocationString();
			AssertHasError("Precondition", transferLine.TransferFromLocationStringInfo, "Only Putaway Transfers and Cross Dock Orders can transfer out from Dock Door Locations.");

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine1 = Helper.CreateWhsTransferLine(putawayTransfer, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, data.Whs1.DefaultLocation);
			var putawayTransferLine2 = Helper.CreateWhsTransferLine(putawayTransfer, data.Part1, 10m, data.Whs1.DefaultLocation, data.Whs1.DefaultOutboundDockDoorLocation);
			putawayTransferLine1.Validation.ValidateTransferFromLocationString();
			AssertNoErrors(putawayTransferLine1.TransferFromLocationStringInfo);

			putawayTransferLine2.Validation.ValidateTransferFromLocationString();
			AssertHasError(putawayTransferLine2.TransferFromLocationStringInfo, GetAreaError(putawayTransferLine2));
		}

		#endregion

		#region TestCheckLocationForInterWarehouseTransfer

		public virtual void TestCheckLocationForInterWarehouseTransfer()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("1", "A", 2, 2);
			Factory.Save();

			whs.FindLocation("A-1-1").WLV_WA_PickingArea = whs.FindLocation("A-1-2").WLV_WA_PickingArea = Helper.CreateArea(whs, "Free", AreaTypes.Codes.FreeStore).PK;
			whs.FindLocation("A-2-1").WLV_WA_PickingArea = Helper.CreateArea(whs, "Bonded", AreaTypes.Codes.Bonded).PK;
			whs.FindLocation("A-2-2").WLV_WA_PickingArea = Helper.CreateArea(whs, "Excise", AreaTypes.Codes.Excise).PK;

			Docket.WD_WW_Whs = whs.PK;
			DocketLine.TransferFromLocationString = "A-1-1";
			DocketLine.LocationString = "A-1-2";
			AssertNoErrors(DocketLine.LocationStringInfo);

			DocketLine.TransferFromLocationString = "A-1-1";
			DocketLine.LocationString = "A-2-1";
			AssertNoWarning(DocketLine.LocationStringInfo, WhsTransferLineValidation.WarningLocationsAreSame);
			AssertHasError(DocketLine.LocationStringInfo, GetAreaError());

			DocketLine.TransferFromLocationString = "A-2-1";
			AssertHasWarning(DocketLine.TransferFromLocationStringInfo, WhsTransferLineValidation.WarningLocationsAreSame);

			DocketLine.LocationString = "A-2-2";
			AssertNoErrors(DocketLine.LocationStringInfo);

			DocketLine.TransferFromLocationString = "A-1-2";
			AssertNoWarning(DocketLine.TransferFromLocationStringInfo, WhsTransferLineValidation.WarningLocationsAreSame);
			AssertHasError(DocketLine.TransferFromLocationStringInfo, GetAreaError());
		}

		#endregion

		#region TestCheckLocationSOH

		public void TestCheckLocationSOH()
		{
			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Enterprise.Environment.Env.CurrentCompany.PK, System.Guid.Empty, System.Guid.Empty, false);

			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA2);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			Docket.WD_OH_Client = receive1.WD_OH_Client;
			Docket.WD_WW_Whs = data.Whs1.PK;
			DocketLine.TransferFromLocationString = locationA1.ToLocationString();
			DocketLine.LocationString = locationA2.ToLocationString();
			AssertNoWarnings("Location should have no warnings.", DocketLine.LocationStringInfo);

			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Enterprise.Environment.Env.CurrentCompany.PK, System.Guid.Empty, System.Guid.Empty, true);
			DocketLine.LocationString = locationA3.ToLocationString();
			AssertNoWarnings("Location should have no warnings.", DocketLine.LocationStringInfo);

			DocketLine.LocationString = locationA2.ToLocationString();
			var warning = DocketLine.LocationStringInfo.GetWarnings().Single().Message;
			AssertMultilineASCIIEquals("Should have warning message.", string.Format(@"Stock On Hand exists.
Client: {0}, Product: {1}", Docket.Client.OH_Code, data.Part1.OP_PartNum), warning);
		}

		public void TestCheckLocationSOH_DockdoorLocation()
		{
			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var locationA1 = data.Whs1.FindLocation("A-1");
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "ABC", false, false);

				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
				Helper.CreatePickNew(order);

				var pickLine = order.Lines[0].PickLines.Single();
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				Assert("TransferLine destination location is a dock door location.", transferLine.Location.IsDockDoorLocation);
				transferLine.Validation.ValidateLocationString();
				AssertNoWarnings("Location should have no warnings.", transferLine.LocationStringInfo);
			}
		}

		public void TestCheckLocationSOH_Fixed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");

			var fixLocationType = Helper.CreateLocationType("TST", "Test", false, 1, LocationClasses.Codes.FIX);
			locationA1.WLV_WLT_LocationType = fixLocationType.PK;
			locationA2.WLV_WLT_LocationType = fixLocationType.PK;
			locationA3.WLV_WLT_LocationType = fixLocationType.PK;

			// SOH
			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Enterprise.Environment.Env.CurrentCompany.PK, System.Guid.Empty, System.Guid.Empty, false);

			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, data.Whs1, "A-1");
			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, data.Whs1, "A-2");
			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, data.Whs1, "A-3");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA2);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			Docket.WD_OH_Client = receive1.WD_OH_Client;
			Docket.WD_WW_Whs = data.Whs1.PK;
			DocketLine.TransferFromLocationString = locationA1.ToLocationString();
			DocketLine.LocationString = locationA2.ToLocationString();
			AssertNoWarnings("Location should have no warnings.", DocketLine.LocationStringInfo);

			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Enterprise.Environment.Env.CurrentCompany.PK, System.Guid.Empty, System.Guid.Empty, true);
			DocketLine.LocationString = locationA3.ToLocationString();
			AssertNoWarnings("Location should have no warnings.", DocketLine.LocationStringInfo);

			DocketLine.LocationString = locationA2.ToLocationString();
			AssertNoWarnings("Location should have no warnings, cause A2 is fixed.", DocketLine.LocationStringInfo);
		}

		#endregion

		#region TestCheckLocationSOH_InTransit

		public void TestCheckLocationSOH_InTransit()
		{
			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Enterprise.Environment.Env.CurrentCompany.PK, System.Guid.Empty, System.Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var sourceLocation = data.Whs1.FindLocation("A-1");
				var destLocation = data.Whs1.FindLocation("A-2");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, sourceLocation);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, destLocation);
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation, destLocation);
				transferLine.RunPreSaveValidation(); // to commit inventory
				Factory.Save();

				transferLine.PickedTime = ZDateTimeOffset.Now;
				AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit,
					transferLine.WE_CurrentInventoryStatus);
				AssertEquals("Inventory status should be InTransit.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

				Factory.Save();

				var warning = transferLine.LocationStringInfo.GetWarnings().Single().Message;
				AssertMultilineASCIIEquals("Should have warning message.", $@"Stock On Hand exists.
Client: {transfer.Client.OH_Code}, Product: {data.Part1.OP_PartNum}", warning);
			}
		}

		#endregion

		#region TestCheckLocationSOH_InTransit_Fixed

		public void TestCheckLocationSOH_InTransit_Fixed()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Enterprise.Environment.Env.CurrentCompany.PK, System.Guid.Empty, System.Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var locationA1 = data.Whs1.FindLocation("A-1");
				var locationA2 = data.Whs1.FindLocation("A-2");

				var fixLocationType = Helper.CreateLocationType("TST", "Test", false, 1, LocationClasses.Codes.FIX);
				locationA1.WLV_WLT_LocationType = fixLocationType.PK;
				locationA2.WLV_WLT_LocationType = fixLocationType.PK;

				Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, data.Whs1, "A-1");
				Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, data.Whs1, "A-2");

				var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1);
				Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA2);
				receive1.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive1);
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(),
					locationA2.ToLocationString());
				AssertNoWarnings("Location should have no warnings.", transferLine.LocationStringInfo);

				transferLine.PickedTime = ZDateTimeOffset.Now;
				AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit,
					transferLine.WE_CurrentInventoryStatus);
				AssertEquals("Inventory status should be InTransit.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
				Factory.Save();

				transferLine.Validation.ValidateLocationString();
				AssertNoWarnings("Location should have no warnings, cause A2 is fixed.", transferLine.LocationStringInfo);
			}
		}

		#endregion

		#region TestCheckLocationSOH_Staged

		public void TestCheckLocationSOH_Staged()
		{
			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Enterprise.Environment.Env.CurrentCompany.PK, System.Guid.Empty, System.Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				data.Whs1.DefaultOutboundDockDoorLocation.WLV_MaxQuantity = 15m;
				var differentClient = Helper.CreateClient("C2", "C2");
				Helper.CreateProductClientRelationShip(differentClient, data.Part1);

				var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				var receive2 = Helper.CreateWhsReceiveWithInventory(differentClient, data.Whs1, "R2", data.Part1, 10m);
				Factory.Save();

				var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
				var order2 = Helper.CreateWhsOrderWithOrderLine(differentClient, data.Whs1, data.Part1, 8m);
				var pick = Helper.CreatePickNew(order1, order2);

				var pickLine1 = order1.Lines[0].PickLines.Single();
				var pickLine2 = order2.Lines[0].PickLines.Single();
				var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
				AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine1.WE_CurrentInventoryStatus);
				transferLine1.FinaliseDocketLine();
				AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, transferLine1.WE_CurrentInventoryStatus);

				var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
				AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine2.WE_CurrentInventoryStatus);

				transferLine2.Validation.ValidateLocationString();
				AssertNoWarnings("Location should have no warnings.", transferLine2.LocationStringInfo);
			}
		}

		#endregion

		#region TestCheckTransferFromLocationString

		public void TestCheckTransferFromLocationString()
		{
			var whs = Helper.CreateWarehouse("WHS01", "A");
			var whs1 = Helper.CreateWarehouse("WHS02", "B");
			Factory.Save();

			var transfer = Factory.New<WhsTransfer>();
			transfer.WD_WW_Whs = whs.PK;

			var transferLine = transfer.Lines.AddNew();
			transferLine.TransferFromLocationString = "A";
			AssertLocationHasError(transferLine.TransferFromLocationStringInfo, "");

			transferLine.TransferFromLocationString = "B";
			AssertLocationHasError(transferLine.TransferFromLocationStringInfo, "Please enter a valid Location.");

			transferLine.TransferFromLocationString = "X";
			AssertLocationHasError(transferLine.TransferFromLocationStringInfo, "Please enter a valid Location.");
		}

		#endregion

		#region TestCheckLocationString2_InnerTransfer_DestLocationWhsIsSameAsSourceLocationWhs

		public void TestCheckLocationString2_InnerTransfer_DestLocationWhsIsSameAsSourceLocationWhs()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "B");
			AssertNoError(transferLine.LocationStringInfo, WhsTransferLineValidation.ErrorOriginAndDestLocationShouldBeFromSameWarehouse);

			transferLine.WE_WD = ZGuid.Empty; // hack to double check that dest location is validated.
			transfer.WD_DocketSubType = TransferType.Codes.Internal;
			transferLine.WE_WD = transfer.PK;
			transferLine.Validation.ValidateLocationString();
			AssertHasError(transferLine.LocationStringInfo, WhsTransferLineValidation.ErrorOriginAndDestLocationShouldBeFromSameWarehouse);
		}

		#endregion

		#region TestCheckTransferFromLocation_DockDoorLocation

		public void TestCheckTransferFromLocation_DockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, dockDoorLocation, "", false, false);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, nonDockDoorLocation, "", false, false);
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.CreateWhsTransferLine(putawayTransfer, data.Part1, data.Whs1.DefaultLocation, "");
			putawayTransferLine.TransferFromLocationString = dockDoorLocation.ToLocationString();
			AssertNoErrors(putawayTransferLine.TransferFromLocationStringInfo);

			putawayTransferLine.TransferFromLocationString = nonDockDoorLocation.ToLocationString();
			AssertHasError(putawayTransferLine.TransferFromLocationStringInfo, "Only Putaway Transfers and Cross Dock Orders can transfer out from Dock Door Locations.");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			transferLine.TransferFromLocationString = dockDoorLocation.ToLocationString();
			AssertHasError(transferLine.TransferFromLocationStringInfo, "Only Putaway Transfers and Cross Dock Orders can transfer out from Dock Door Locations.");

			transferLine.TransferFromLocationString = nonDockDoorLocation.ToLocationString();
			AssertNoErrors(transferLine.TransferFromLocationStringInfo);
		}

		#endregion

		#region TestCheckTransferFromLocation_DockDoorLocation_ReturnStockTransfer

		public void TestCheckTransferFromLocation_DockDoorLocation_ReturnStockTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, dockDoorLocation, "", false, false);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, nonDockDoorLocation, "", false, false);
			Factory.Save();

			var returnStockTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			returnStockTransfer.WD_WD_ParentDocket = ZGuid.NewZGuid();
			var returnStockTransferLine = Helper.CreateWhsTransferLine(returnStockTransfer, data.Part1, data.Whs1.DefaultLocation, "");
			returnStockTransferLine.TransferFromLocationString = dockDoorLocation.ToLocationString();
			AssertNoErrors(returnStockTransferLine.TransferFromLocationStringInfo);

			returnStockTransferLine.TransferFromLocationString = nonDockDoorLocation.ToLocationString();
			AssertNoErrors(returnStockTransferLine.TransferFromLocationStringInfo);
		}

		#endregion

		#region TestCheckTransferToLocation_DockDoorLocation

		public void TestCheckTransferToLocation_DockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, dockDoorLocation, "", false, false);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, nonDockDoorLocation, "", false, false);

			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.CreateWhsTransferLine(putawayTransfer, data.Part1, data.Whs1.DefaultLocation, "");
			putawayTransferLine.LocationString = dockDoorLocation.ToLocationString();
			AssertHasError(putawayTransferLine.LocationStringInfo, "You cannot putaway to Dock Door locations.");

			putawayTransferLine.LocationString = nonDockDoorLocation.ToLocationString();
			AssertNoErrors(putawayTransferLine.LocationStringInfo);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");
			transferLine.LocationString = dockDoorLocation.ToLocationString();
			AssertHasError(transferLine.LocationStringInfo, "You cannot putaway to Dock Door locations.");

			transferLine.LocationString = nonDockDoorLocation.ToLocationString();
			AssertNoErrors(transferLine.LocationStringInfo);
		}

		#endregion

		#region TestCheckTransferToLocation_DockDoorLocation_ForOrders

		public void TestCheckTransferToLocation_DockDoorLocation_ForOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, nonDockDoorLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, nonDockDoorLocation, dockDoorLocation);
			AssertHasError(transferLine.LocationStringInfo, "You cannot transfer stock from a Free Store area to a Dock Door area.");

			var pick = Factory.New<WhsPick>();
			transfer.WD_WP_ParentPickForTransfer = pick.PK;
			transferLine.Validation.ValidateLocationString();
			AssertNoErrors(transferLine.LocationStringInfo);
		}

		#endregion

		#region TestCheckTransferFromLocation_DockDoorLocation_ForOrders

		public void TestCheckTransferFromLocation_DockDoorLocation_ForCrossDockOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation2 = data.Whs1.FindLocation("A-2");
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m, null, "PLT158");
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 15m);
			Helper.CreateReservePickLine(orderLine1, receiveLine.Inventory[0], 15m);
			order1.WD_WL_CrossDock = dockDoorLocation.PK;
			Factory.Save();

			receiveLine.WE_WL = dockDoorLocation.PK;
			Factory.Save();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.CreatePickNew(order1);
			var transferLine = Helper.PickAndMakeInTransitTransfer(order1.Lines[0].PickLines.Single(), ZDateTimeOffset.Now);
			transferLine.RunPreSaveValidation();
			AssertNoErrors(transferLine.TransferFromLocationStringInfo);
		}

		#endregion

		#region TestCheckTransferToLocation_PackingLocation

		public void TestCheckTransferToLocation_PackingStationLocation()
		{
			TestCheckTransferToLocation_PackingLocationCore(LocationClasses.Codes.PST, "You cannot putaway to Packing Station locations.");
		}

		public void TestCheckTransferToLocation_PackingConsolidationLocation()
		{
			TestCheckTransferToLocation_PackingLocationCore(LocationClasses.Codes.CON, "You cannot putaway to Packing Consolidation locations.");
		}

		void TestCheckTransferToLocation_PackingLocationCore(string locationClass, string errorMsg)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var packingLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, locationClass);
			var packingLocation = data.Whs1.FindLocation("A-1");
			packingLocation.WLV_WLT_LocationType = packingLocationType.PK;
			var normalLocation = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, packingLocation, "", false, false);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, normalLocation, "", false, false);

			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.CreateWhsTransferLine(putawayTransfer, data.Part1, normalLocation, "");
			putawayTransferLine.LocationString = packingLocation.ToLocationString();
			AssertHasError(putawayTransferLine.LocationStringInfo, errorMsg);

			putawayTransferLine.LocationString = normalLocation.ToLocationString();
			AssertNoErrors(putawayTransferLine.LocationStringInfo);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, normalLocation, "");
			transferLine.LocationString = packingLocation.ToLocationString();
			AssertHasError(transferLine.LocationStringInfo, errorMsg);

			transferLine.LocationString = normalLocation.ToLocationString();
			AssertNoErrors(transferLine.LocationStringInfo);
		}

		#endregion

		#region TestCheckTransferToLocation_PackingLocation_ForOrders

		public void TestCheckTransferToLocation_PackingStationLocation_ForOrders()
		{
			TestCheckTransferToLocation_PackingLocation_ForOrdersCore(LocationClasses.Codes.PST);
		}

		public void TestCheckTransferToLocation_PackingConsolidationLocation_ForOrders()
		{
			TestCheckTransferToLocation_PackingLocation_ForOrdersCore(LocationClasses.Codes.CON);
		}

		void TestCheckTransferToLocation_PackingLocation_ForOrdersCore(string locationClass)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			var packingLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, locationClass);
			var packingLocation = data.Whs1.FindLocation("A-1");
			packingLocation.WLV_WLT_LocationType = packingLocationType.PK;
			var normalLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, normalLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var pick = Factory.New<WhsPick>();
			transfer.WD_WP_ParentPickForTransfer = pick.PK;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, normalLocation, packingLocation);
			transferLine.Validation.ValidateLocationString();
			AssertNoErrors(transferLine.LocationStringInfo);
		}

		#endregion

		#region TestCheckTransferFromLocation_PackingLocation_ForOrders

		public void TestCheckTransferFromLocation_PackingStationLocation_ForOrders()
		{
			TestCheckTransferFromLocation_PackingLocation_ForOrdersCore(LocationClasses.Codes.PST);
		}

		public void TestCheckTransferFromLocation_PackingConsolidationLocation_ForOrders()
		{
			TestCheckTransferFromLocation_PackingLocation_ForOrdersCore(LocationClasses.Codes.CON);
		}

		void TestCheckTransferFromLocation_PackingLocation_ForOrdersCore(string locationClass)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var packingLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, locationClass);
			var packingLocation = data.Whs1.FindLocation("A-1");
			packingLocation.WLV_WLT_LocationType = packingLocationType.PK;
			var normalLocation = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, normalLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			pick.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			Factory.Save();

			var transfer1 = pick.Transfers.Single();
			var transferLine1 = transfer1.Lines[0];
			transferLine1.LocationString = packingLocation.WLV_LocationString;
			AssertNoErrors(transferLine1.LocationStringInfo);

			transfer1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer1);

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T2");
			transfer2.WD_WP_ParentPickForTransfer = pick.PK;

			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, packingLocation, dockDoorLocation);
			AssertNoErrors(transferLine2.LocationStringInfo);
		}

		#endregion

		// TODO: Uncomment the test and make sure it is working in Location Binder change around WI. A.V.
		//#region TestCheckLocation_IsValidatedWhenWarehouseChanges

		//public void TestCheckLocation_IsValidatedWhenWarehouseChanges()
		//{
		//    var data = new TestDataSimpleEnvironment(Factory, 2, 1);
		//    var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
		//    var whs3 = Helper.CreateWarehouse("WH3", "C", 2, 1);

		//    var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
		//    var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
		//    var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", whs3.PK, "C-1");

		//    transfer.RunPreSaveValidation();
		//    AssertNoErrors(transferLine.LocationStringInfo);

		//    transfer.WD_WW_Whs = whs2.PK;
		//    transfer.RunPreSaveValidation();
		//    AssertHasErrors(transferLine.LocationStringInfo);
		//}

		//#endregion

		#endregion

		#region Attributes

		#region TestIsAttributeValidationRequired

		public override void TestIsAttributeValidationRequired()
		{
			AssertEquals(false, DocketLine.Validation.IsAttributeValidationRequired);
			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			Docket.WD_OH_Client = Factory.New<OrgHeader>().PK;
			AssertEquals(true, DocketLine.Validation.IsAttributeValidationRequired);

			DocketLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			DocketLine.WE_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(true, DocketLine.IsFinalised);
			AssertEquals(false, DocketLine.Validation.IsAttributeValidationRequired);
		}

		#endregion

		#region TestCheckWE_SerialNumber

		public void TestCheckWE_SerialNumber_QtyGreaterThanOne()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AA1");
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, data.Whs1.DefaultLocation);
			adjustmentLine.WE_SerialNumber = "Ser";
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, data.Whs1.DefaultLocation, data.Whs1.DefaultLocation);
			transferLine.WE_SerialNumber = "Ser";

			transferLine.QtyToMoveIncludingMatchingLines = 10m;
			AssertHasError("Errors for serial number and 10 Qty", transferLine.QtyToMoveIncludingMatchingLinesInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			transferLine.WE_SerialNumber = "";
			AssertNoErrors("No errors for No serial number and 10 Qty", transferLine.QtyToMoveIncludingMatchingLinesInfo);

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				transferLine.WE_SerialNumber = "Ser";
				AssertNoErrors("No errors for No serial number and 10 Qty", transferLine.QtyToMoveIncludingMatchingLinesInfo);
			}
		}

		public void TestCheckWE_SerialNumber_QtyGreaterThanOne_InterWhsDestination()
		{
			TestCheckWE_SerialNumber_QtyGreaterThanOne_InterWhsCore(isSource: false);
		}

		public void TestCheckWE_SerialNumber_QtyGreaterThanOne_InterWhsSource()
		{
			TestCheckWE_SerialNumber_QtyGreaterThanOne_InterWhsCore(isSource: true);
		}

		void TestCheckWE_SerialNumber_QtyGreaterThanOne_InterWhsCore(bool isSource)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("W2", "B");
			var transferWhs = isSource ? data.Whs1 : whs2;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var fromLocation = data.Whs1.FindLocation("A-1");
			var whsReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(whsReceive, data.Part1, 1m, fromLocation, "");
			receiveLine.WE_SerialNumber = "ERT";
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(whsReceive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, transferWhs, "TR1");
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A", isSource ? whs2.PK : data.Whs1.PK, "B");
			transferLine.WE_SerialNumber = "ERT";
			AssertNoErrors("No errors for serial number and 1 Qty", transferLine.QtyToMoveIncludingMatchingLinesInfo);

			transferLine.QtyToMoveIncludingMatchingLines = 10m;
			AssertHasError("Errors for serial number and 10 Qty", transferLine.QtyToMoveIncludingMatchingLinesInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			transferLine.WE_SerialNumber = "";
			AssertNoErrors("No errors for No serial number and 10 Qty", transferLine.QtyToMoveIncludingMatchingLinesInfo);
		}

		#endregion

		#endregion

		#region TestCheckWE_TransactionQuantity

		protected override void TestCheckWE_TransactionQuantityCore()
		{
			Assert("This property is not visible for user, so should not be validated.", true);
		}

		#endregion

		#region TestCheckWE_GS_NKPutawayBy_OnlyOnePersonCanPutawaySamePallet

		public void TestCheckWE_GS_NKPutawayBy_OnlyOnePersonCanPutawaySamePallet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var user1 = Helper.CreateGlbStaff("AAA", "A.A");
			var user2 = Helper.CreateGlbStaff("BBB", "B.B");
			var errorMessage = "Only one user can putaway a Pallet that is fully transferred. This Pallet is already assigned to user '{0}'.";
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);

			// Same Pallet ID Transfer into different Location
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A-1", "PLT-1", "A-2", "PLT-1");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A-1", "PLT-1", "A-2", "PLT-1");

			transferLine1.WE_GS_NKPutawayBy = "AAA";
			AssertNoErrors("If only 1 line for same Pallet ID assigned to a user, then validation should pass.", transferLine1.WE_GS_NKPutawayByInfo);

			transferLine2.WE_GS_NKPutawayBy = "AAA";
			AssertNoErrors("If all lines for same Pallet ID assigned to a single user, then validation should pass.", transferLine2.WE_GS_NKPutawayByInfo);

			transferLine2.WE_GS_NKPutawayBy = "BBB";
			AssertHasError("If lines for same Pallet ID assigned to different user, then validation should fail.", transferLine2.WE_GS_NKPutawayByInfo, ZString.Format(errorMessage, "AAA"));

			// Same Pallet ID Transfer into same Location
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A-1", "PLT-2", "A-1", "PLT-2");
			var transferLine4 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A-1", "PLT-2", "A-1", "PLT-2");

			transferLine3.WE_GS_NKPutawayBy = "AAA";
			AssertNoErrors("If source and destination locations are same, then Putting away user is not important.", transferLine3.WE_GS_NKPutawayByInfo);

			transferLine4.WE_GS_NKPutawayBy = "AAA";
			AssertNoErrors("If source and destination locations are same, then Putting away user is not important.", transferLine4.WE_GS_NKPutawayByInfo);

			transferLine4.WE_GS_NKPutawayBy = "BBB";
			AssertNoErrors("If source and destination locations are same, then Putting away user is not important.", transferLine4.WE_GS_NKPutawayByInfo);

			// different Pallet ID Transfer
			var transferLine5 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A-1", "PLT-1", "A-2", "PLT-3");
			var transferLine6 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A-1", "PLT-1", "A-2", "PLT-3");

			transferLine5.WE_GS_NKPutawayBy = "AAA";
			AssertNoErrors("If source and destination Pallet IDs are different, then Putting away user is not important.", transferLine5.WE_GS_NKPutawayByInfo);

			transferLine6.WE_GS_NKPutawayBy = "AAA";
			AssertNoErrors("If source and destination Pallet IDs are different, then Putting away user is not important.", transferLine6.WE_GS_NKPutawayByInfo);

			transferLine6.WE_GS_NKPutawayBy = "BBB";
			AssertNoErrors("If source and destination Pallet IDs are different, then Putting away user is not important.", transferLine6.WE_GS_NKPutawayByInfo);
		}

		#endregion

		#region TestCheckWE_GS_NKPutawayBy_CheckPutawayLinesHavePutawayBySet

		public void TestCheckWE_GS_NKPutawayBy_CheckPutawayLinesHavePutawayBySet()
		{
			var user = Helper.CreateGlbStaff("AAA", "A.A");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			data.Whs1.FindLocation("A-2").WLV_MaxQuantity = 10m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, loc1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, loc1, loc2);
			AssertNoErrors("Precondition.", transferLine.GS_NKPickedByInfo);

			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.WE_PutawayTime = ZDateTimeOffset.Today;
			}

			transferLine.WE_GS_NKPutawayBy = "";
			AssertHasError(transferLine.WE_GS_NKPutawayByInfo, "Enter the user that put this product away.");

			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.WE_PutawayTime = ZDateTimeOffset.Empty;
			}
			AssertNoErrors("Should *not* have an error after setting PutawayTime.", transferLine.WE_GS_NKPutawayByInfo);

			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.WE_PutawayTime = ZDateTimeOffset.Today;
			}
			transferLine.WE_GS_NKPutawayBy = "";
			AssertHasError(transferLine.WE_GS_NKPutawayByInfo, "Enter the user that put this product away.");

			transferLine.WE_GS_NKPutawayBy = "AAA";
			AssertNoErrors("Should have *not* have an error after setting PutawayBy.", transferLine.WE_GS_NKPutawayByInfo);
		}

		#endregion

		#region TestCheckGS_NKPickedBy

		public void TestCheckGS_NKPickedBy()
		{
			var user = Helper.CreateGlbStaff("AAA", "A.A");
			var errorMessage = ListValidation.InvalidCodeError + "GS_NKPickedBy.";
			var transferLine = DocketLine;

			transferLine.GS_NKPickedBy = "";
			AssertNoErrors(transferLine.GS_NKPickedByInfo);

			transferLine.GS_NKPickedBy = "AAA";
			AssertNoErrors(transferLine.GS_NKPickedByInfo);

			transferLine.GS_NKPickedBy = "BBB";
			AssertHasError(transferLine.GS_NKPickedByInfo, errorMessage);
		}

		public void TestCheckGS_NKPickedBy_CheckPickedLinesHaveAPickerAssigned()
		{
			var user = Helper.CreateGlbStaff("AAA", "A.A");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			data.Whs1.FindLocation("A-2").WLV_MaxQuantity = 10m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, loc1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, loc1, loc2);
			AssertNoErrors("Precondition.", transferLine.GS_NKPickedByInfo);

			transferLine.PickedTime = ZDateTimeOffset.Today;
			transferLine.GS_NKPickedBy = "";
			AssertHasError(transferLine.GS_NKPickedByInfo, "Lines that have been picked must have a Picker.");

			transferLine.PickedTime = ZDateTimeOffset.Empty;
			AssertNoErrors("Should *not* have an error after setting PickedTime.", transferLine.GS_NKPickedByInfo);

			transferLine.PickedTime = ZDateTimeOffset.Today;
			transferLine.GS_NKPickedBy = "";
			AssertHasError(transferLine.GS_NKPickedByInfo, "Lines that have been picked must have a Picker.");

			transferLine.GS_NKPickedBy = "AAA";
			AssertNoErrors("Should have *not* have an error after setting PickedBy.", transferLine.GS_NKPickedByInfo);
		}

		#endregion

		#region TestCheckWE_PutawayTime_IsNotSetBeforePickedTime

		public void TestCheckWE_PutawayTime_IsNotSetBeforePickedTime()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, loc1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, loc1, loc2);
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			AssertEquals("Precondition: Is Not Picked.", false, transferLine.IsPicked);
			AssertEquals("Precondition: Is Not Putaway.", false, transferLine.IsPutaway);
			AssertNoErrors("Precondition: No Errors.", transferLine.WE_PutawayTimeInfo);

			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.WE_PutawayTime = ZDateTimeOffset.Today;
			}
			AssertHasError("PutawayTime should be in error.", transferLine.WE_PutawayTimeInfo, "This product cannot be put away because it is not yet picked.");

			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.WE_PutawayTime = ZDateTimeOffset.Empty;
			}
			AssertNoErrors("PutawayTime should *not* be in error.", transferLine.WE_PutawayTimeInfo);

			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.WE_PutawayTime = ZDateTimeOffset.Today;
			}
			AssertHasError("Precondition.", transferLine.WE_PutawayTimeInfo, "This product cannot be put away because it is not yet picked.");

			transferLine.PickedTime = ZDateTimeOffset.Today;
			AssertEquals("Should not change PickedTime if the line is in error. If this fails we must now test and handle the case where PickedTime is set after PutawayTime.", ZDateTimeOffset.Empty, transferLine.PickedTime);

			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.WE_PutawayTime = ZDateTimeOffset.Empty;
			}
			transferLine.PickedTime = ZDateTimeOffset.Today;
			AssertEquals("Precondition.", false, transferLine.PickedTime.IsEmpty);

			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.WE_PutawayTime = ZDateTimeOffset.Today;
			}
			AssertNoErrors("PutawayTime should *not* be in error.", transferLine.WE_PutawayTimeInfo);
		}

		#endregion

		#region TestCheckWE_PutawayTime_IsNotSetWithoutDestinationLocation

		public void TestCheckWE_PutawayTime_IsNotSetWithoutDestinationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, loc1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "");
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			AssertEquals("Precondition: Is Not Putaway.", false, transferLine.IsPutaway);
			AssertNoErrors("Precondition: No Errors.", transferLine.WE_PutawayTimeInfo);

			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.WE_PutawayTime = ZDateTimeOffset.Today;
			}
			AssertHasError("WE_PutawayTime should be in error.", transferLine.WE_PutawayTimeInfo, "This product cannot be put away because it does not have a Destination Location.");
			AssertEquals("If LocationString is ReadOnly, we don't have to test/handle it being set while PutawayTime is in error.", true, transferLine.LocationStringInfo.ReadOnly);

			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.WE_PutawayTime = ZDateTimeOffset.Empty;
			}
			AssertNoErrors("WE_PutawayTime should *not* be in error.", transferLine.WE_PutawayTimeInfo);

			transferLine.PickedTime = ZDateTimeOffset.Today;
			transferLine.LocationString = loc2.ToLocationString();
			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.WE_PutawayTime = ZDateTimeOffset.Today;
			}
			AssertNoErrors("WE_PutawayTime should *not* be in error.", transferLine.WE_PutawayTimeInfo);
		}

		#endregion

		#region TestCheckPickedTime

		public void TestCheckPickedTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", "");
			AssertNoWarnings("Precondition", transferLine.PickedTimeInfo);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertHasWarning(transferLine.PickedTimeInfo, "Transfer Line must be fully Committed with no Errors before Picking.");

			transferLine.Validation.ValidateAll();
			AssertNoWarnings(transferLine.PickedTimeInfo);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: PickedTime is set.", true, transferLine.PickedTime.IsValid);
			AssertNoWarnings(transferLine.PickedTimeInfo);
		}

		#endregion

		#region TestCheckQtyToMoveIncludingMatchingLines

		public void TestCheckQtyToMoveIncludingMatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			AssertQuantityForSerialNumberProduct(data, WhsTransferLine.Schema.QtyToMoveIncludingMatchingLines);
			AssertQtyIsNotNegativeAndNotZero();
		}

		void AssertQtyIsNotNegativeAndNotZero()
		{
			var transferLine = Factory.New<WhsTransferLine>();
			transferLine.QtyToMoveIncludingMatchingLines = 0m;
			AssertHasError(transferLine.QtyToMoveIncludingMatchingLinesInfo, "Quantity cannot be zero.");

			transferLine.QtyToMoveIncludingMatchingLines = -1m;
			AssertHasError(transferLine.QtyToMoveIncludingMatchingLinesInfo, "Quantity cannot be negative.");

			transferLine.QtyToMoveIncludingMatchingLines = 5m;
			AssertNoErrors(transferLine.QtyToMoveIncludingMatchingLinesInfo);
		}

		#endregion

		#region TestCheckPalledIdIsEmptyIfDestinationLocationIsPickFace

		public void TestCheckPalledIdIsEmptyIfDestinationLocationIsPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var product = WhsProduct.GetWhsProduct(data.Part1);
			var pickFace = product.PickFaces.AddNew();
			var pickFaceLocation = locations[1];
			pickFace.WF_WL = pickFaceLocation.PK;
			var normalLocation = locations[2];
			var stockLocation = locations[0];

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, stockLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			pickFaceLocation.LocationType.WLT_RetainPalletIDsInFixedPickFaces = false;

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, stockLocation.ToLocationString(), "");
			transferLine.WE_WL = pickFaceLocation.PK;
			transferLine.WE_PalletID = "ABC";
			AssertHasError(transferLine.WE_PalletIDInfo, "A Destination Pallet ID cannot be entered as the Destination Location is a Pick Face that does not Retain Pallet IDs.");

			transferLine.WE_WL = normalLocation.PK;
			transferLine.WE_PalletID = "ABC";
			AssertNoErrors("When location is not pick face - we don't mind to have a pallet id", transferLine.WE_PalletIDInfo);

			transferLine.WE_WL = pickFaceLocation.PK;
			transferLine.WE_PalletID = "";
			AssertNoErrors("It is OK to not to have a palletId for pick face location", transferLine.WE_PalletIDInfo);

			pickFaceLocation.LocationType.WLT_RetainPalletIDsInFixedPickFaces = true;

			transferLine.WE_WL = pickFaceLocation.PK;
			transferLine.WE_PalletID = "ABC";
			AssertNoErrors("When WLT_RetainPalletIDsInFixedPickFaces is set to true, we can keep Pallet ID, therefore the validation should have no errors.", transferLine.WE_PalletIDInfo);

			transferLine.WE_WL = normalLocation.PK;
			transferLine.WE_PalletID = "ABC";
			AssertNoErrors("Normal location, therefore the validation should have no errors.", transferLine.WE_PalletIDInfo);

			transferLine.WE_WL = pickFaceLocation.PK;
			transferLine.WE_PalletID = "";
			AssertNoErrors("Empty Pallet ID, therefore the validation should have no errors.", transferLine.WE_PalletIDInfo);
		}

		public void TestCheckPalledIdIsEmptyIfDestinationLocationIsPickFace_DifferentClientPickfaces()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var differentClient = Helper.CreateClient("C2", "C2");
			Helper.CreateProductClientRelationShip(differentClient, data.Part1);
			var pickFaceLocationForDifferentClient = locations[0];
			Helper.CreateProductPickFace(data.Part1, differentClient, pickFaceLocationForDifferentClient);
			var transferFromBulkLocation = locations[1];

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, transferFromBulkLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, transferFromBulkLocation.ToLocationString(), "");
			transferLine.WE_WL = pickFaceLocationForDifferentClient.PK;
			transferLine.WE_PalletID = "ABC";
			AssertNoErrors("Location is a pickface for a different client. Therefore, it should not have any errors.", transferLine.WE_PalletIDInfo);

			transferLine.WE_WL = pickFaceLocationForDifferentClient.PK;
			transferLine.WE_PalletID = "";
			AssertNoErrors(transferLine.WE_PalletIDInfo);
		}

		public void TestCheckPalletId_InPickFaceError_NotshownIfOtherErrorExists()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var product = WhsProduct.GetWhsProduct(data.Part1);
			var pickFace = product.PickFaces.AddNew();
			var pickFaceLocation = locations[1];
			pickFace.WF_WL = pickFaceLocation.PK;
			var normalLocation = locations[2];
			var stockLocation = locations[0];

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, stockLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, stockLocation.ToLocationString(), "");
			transferLine.WE_WL = pickFaceLocation.PK;
			transferLine.WE_PalletID = "测试区";

			AssertNoError(transferLine.WE_PalletIDInfo, "A Destination Pallet ID cannot be entered as the Destination Location is a Pick Face that does not Retain Pallet IDs.");

			transferLine.WE_WL = pickFaceLocation.PK;
			transferLine.WE_PalletID = "DGR";
			AssertHasError(transferLine.WE_PalletIDInfo, "A Destination Pallet ID cannot be entered as the Destination Location is a Pick Face that does not Retain Pallet IDs.");
		}

		#endregion

		#region TestCheckWE_AdjustmentArrivalDateIsValidZDateTimeRange

		public override void TestCheckWE_AdjustmentArrivalDateIsValidZDateTimeRange()
		{
			DocketLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			AssertNoErrors(DocketLine.WE_AdjustmentArrivalDateInfo);

			DocketLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(1941, 06, 22);
			AssertNoErrors(DocketLine.WE_AdjustmentArrivalDateInfo);
			AssertHasWarning(DocketLine.WE_AdjustmentArrivalDateInfo, "The date '22-Jun-1941' is more than 1 year old.");
		}

		#endregion

		#region TestCheckWE_ExpiryDateIsValidZDateTimeRange

		public override void TestCheckWE_ExpiryDateIsValidZDateTimeRange()
		{
			DocketLine.WE_ExpiryDate = ZDate.Today;
			AssertNoErrors(DocketLine.WE_ExpiryDateInfo);

			DocketLine.WE_ExpiryDate = new ZDate(1941, 06, 22);
			AssertNoErrors(DocketLine.WE_ExpiryDateInfo);
			AssertHasWarning(DocketLine.WE_ExpiryDateInfo, "The date '22-Jun-1941' is more than 1 year old.");
		}

		#endregion

		#region TestCheckWE_PackingDateIsValidZDateTimeRange

		public override void TestCheckWE_PackingDateIsValidZDateTimeRange()
		{
			DocketLine.WE_PackingDate = ZDate.Today;
			AssertNoErrors(DocketLine.WE_PackingDateInfo);

			DocketLine.WE_PackingDate = new ZDate(1941, 06, 22);
			AssertNoErrors(DocketLine.WE_PackingDateInfo);
			AssertHasWarning(DocketLine.WE_PackingDateInfo, "The date '22-Jun-1941' is more than 1 year old.");
		}

		#endregion

		#region TestCheckWE_RequiredByDate_IsValidZDateTimeRange

		public override void TestCheckWE_RequiredByDate_IsValidZDateTimeRange()
		{
			DocketLine.WE_RequiredByDate = ZDateTimeOffset.Now;
			AssertNoErrors(DocketLine.WE_RequiredByDateInfo);

			DocketLine.WE_RequiredByDate = new ZDateTimeOffset(1941, 06, 22);
			AssertNoErrors(DocketLine.WE_RequiredByDateInfo);
			AssertHasWarning(DocketLine.WE_RequiredByDateInfo, "The date '22-Jun-1941' is more than 1 year old.");
		}

		#endregion

		#region TestCheckHeldCode_MandatoryWhenHeld_PutawayTransfer

		public void TestCheckHeldCode_MandatoryWhenHeld_PutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "PLT1", 15m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, nonDockDoorLocation, "PLT1", 15m);
			transfer.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;

			AssertEquals($"Precondition: InventoryStatus of Transfer Line should be {InventoryStatus.Codes.PuttingAway}.", InventoryStatus.Codes.PuttingAway, transferLine.WE_CurrentInventoryStatus);

			AssertNoErrors(transferLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);
			AssertNoErrors(transferLine.WE_WHC_NKCurrentInventoryHeldCodeInfo);
		}

		#endregion

		#region TestCheckWE_WHC_NKOriginalInventoryHeldCode_MandatoryWhenHeld_InTransit

		public void TestCheckWE_WHC_NKOriginalInventoryHeldCode_MandatoryWhenHeld_InTransit()
		{
			DocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo, "Please enter a Hold Code.");

			DocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.InTransit;
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo);

			DocketLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			DocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo, "Please do not enter a Hold Code.");

			DocketLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			DocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.InTransit;
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo);
		}

		#endregion

		#region TestCheckWE_WHC_NKCurrentInventoryHeldCode_MandatoryWhenHeld_InTransit

		public void TestCheckWE_WHC_NKCurrentInventoryHeldCode_MandatoryWhenHeld_InTransit()
		{
			DocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo, "Please enter a Hold Code.");

			DocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.InTransit;
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo);
		}

		#endregion

		#region TestCheckWE_WHC_NKOriginalInventoryHeldCode_MandatoryWhenHeld

		public override void TestCheckWE_WHC_NKOriginalInventoryHeldCode_MandatoryWhenHeld()
		{
			DocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo, "Please enter a Hold Code.");

			DocketLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);

			DocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo, "Please do not enter a Hold Code.");
		}

		#endregion

		#region TestCheckWE_WHC_NKCurrentInventoryHeldCode_MandatoryWhenHeld

		public override void TestCheckWE_WHC_NKCurrentInventoryHeldCode_MandatoryWhenHeld()
		{
			DocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo, "Please enter a Hold Code.");

			DocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo);

			DocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo, "Please do not enter a Hold Code.");
		}

		#endregion

		#region ValidStatuses

		protected override IEnumerable<ZString> ValidStatuses => new ZString[] { "ENT", "HFT", "FIN" };

		#endregion

		#region TestCheckArrivalDateForBindingHasNoErrorForVeryOldDate

		public void TestCheckArrivalDateForBindingHasNoErrorForVeryOldDate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10, "A-1", "A-2");
			AssertNoErrors(transferLine.ArrivalDateForBindingInfo);

			var veryOldDate = new ZDateTimeOffset(1945, 05, 09);
			transferLine.ArrivalDateForBinding = veryOldDate;
			AssertNoErrors(transferLine.ArrivalDateForBindingInfo);
			AssertHasWarning(transferLine.ArrivalDateForBindingInfo, "The date '09-May-1945' is more than 1 year old.");
		}

		#endregion

		#region TestCheckLocationString_DynamicLocations

		public void TestCheckLocationString_IsNotDynamicLocationForNonDynamicProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);

			var normalLocationType = Helper.CreateLocationType("NLC", LocationClasses.Codes.NOR);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var normalLocation = locations[0];
			normalLocation.WLV_WLT_LocationType = normalLocationType.PK;

			var dynamicLocation = locations[1];
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, normalLocation, "", false, true);
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 2m, normalLocation, dynamicLocation);
			AssertHasError(transferLine1.LocationStringInfo, $"Cannot put product {data.Part1.OP_PartNum} in dynamic location {dynamicLocation.ToLocationString()}, as it is not a dynamic product.");

			var productParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 2m, normalLocation, dynamicLocation);
			AssertNoErrors("Can putaway as product is assigned to a dynamic location", transferLine2.LocationStringInfo);
		}

		public void TestCheckLocationString_IsLocatedInCorrectDynamicAreaForDynamicProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var dynamicArea1 = Helper.CreateArea(data.Whs1, "DYNAMIC1", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicArea2 = Helper.CreateArea(data.Whs1, "DYNAMIC2", AreaTypes.Codes.DynamicPickFace, true, false);

			var normalLocationType = Helper.CreateLocationType("NLC", LocationClasses.Codes.NOR);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var normalLocation = locations[0];
			normalLocation.WLV_WLT_LocationType = normalLocationType.PK;

			var dynamicLocation1 = locations[1];
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea1.PK;

			var dynamicLocation2 = locations[2];
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea2.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, normalLocation, "", false, true);
			Factory.Save();

			var productParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 2m, normalLocation, dynamicLocation2);

			AssertHasError(transferLine1.LocationStringInfo, $"Cannot put dynamic product {data.Part1.OP_PartNum} in dynamic location {dynamicLocation2.ToLocationString()}, as the location is not within the product's designated dynamic area ({dynamicArea1.WA_Name}).");

			productParams.W3_WA_DynamicPickFaceArea = dynamicArea2.PK;
			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 2m, normalLocation, dynamicLocation2);

			AssertNoErrors("Can putaway as product is assigned to a dynamic location within the correct area", transferLine2.LocationStringInfo);
		}

		public void TestCheckLocationString_CanBeNonPickfaceLocationForDynamicProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);

			var normalLocationType = Helper.CreateLocationType("NLC", LocationClasses.Codes.NOR);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var normalLocation1 = locations[0];
			normalLocation1.WLV_WLT_LocationType = normalLocationType.PK;

			var normalLocation2 = locations[0];
			normalLocation2.WLV_WLT_LocationType = normalLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, normalLocation1, "", false, true);
			Factory.Save();

			var productParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, normalLocation1, normalLocation2);

			AssertNoErrors("Can putaway dynamic product as location is not a pick face", transferLine1.LocationStringInfo);
		}

		public void TestCheckLocationString_CanTransferNonDynamicProductIntoDynamicLocationIfSetForPickReplenishment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var dynamicAreaCorrect = Helper.CreateArea(data.Whs1, "DYNAMIC1", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicAreaWrong = Helper.CreateArea(data.Whs1, "DYNAMIC2", AreaTypes.Codes.DynamicPickFace, true, false);

			var normalLocationType = Helper.CreateLocationType("NLC", LocationClasses.Codes.NOR);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var normalLocation = locations[0];
			normalLocation.WLV_WLT_LocationType = normalLocationType.PK;

			var dynamicLocation1 = locations[1];
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicAreaWrong.PK;

			var dynamicLocation2 = locations[2];
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicAreaCorrect.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m, normalLocation, "", false, true);
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 2m, normalLocation, dynamicLocation1);
			AssertHasError(transferLine1.LocationStringInfo, $"Cannot put product {data.Part1.OP_PartNum} in dynamic location {dynamicLocation1.ToLocationString()}, as it is not a dynamic product.");

			var pick = Helper.CreatePickNew();
			pick.WP_WA_DynamicPickAreaOverride = dynamicAreaCorrect.PK;

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 2m, normalLocation, dynamicLocation1);
			AssertHasError(transferLine1.LocationStringInfo, $"Cannot put product {data.Part1.OP_PartNum} in dynamic location {dynamicLocation1.ToLocationString()}, as it is not a dynamic product.");

			var transfer3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer3.WD_WP_PickBeingReplenished = pick.PK;
			var transferLine3 = Helper.CreateWhsTransferLine(transfer3, data.Part1, 2m, normalLocation, dynamicLocation2);
			AssertNoErrors("Can putaway as transfer is replenishing Pick", transferLine3.LocationStringInfo);
		}

		public void TestCheckLocationString_DoesNotConsiderProductsDynamicAreaIfSet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var dynamicArea1 = Helper.CreateArea(data.Whs1, "DYNAMIC1", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicArea2 = Helper.CreateArea(data.Whs1, "DYNAMIC2", AreaTypes.Codes.DynamicPickFace, true, false);

			var normalLocationType = Helper.CreateLocationType("NLC", LocationClasses.Codes.NOR);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var normalLocation = locations[0];
			normalLocation.WLV_WLT_LocationType = normalLocationType.PK;

			var dynamicLocation1 = locations[1];
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea1.PK;

			var dynamicLocation2 = locations[2];
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea2.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, normalLocation, "", false, true);
			Factory.Save();

			var productParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 2m, normalLocation, dynamicLocation2);

			AssertHasError(transferLine1.LocationStringInfo, $"Cannot put dynamic product {data.Part1.OP_PartNum} in dynamic location {dynamicLocation2.ToLocationString()}, as the location is not within the product's designated dynamic area ({dynamicArea1.WA_Name}).");

			var pick = Helper.CreatePickNew();
			pick.WP_WA_DynamicPickAreaOverride = dynamicArea2.PK;

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer2.WD_WP_PickBeingReplenished = pick.PK;
			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 2m, normalLocation, dynamicLocation2);

			AssertNoErrors("Can putaway as product is assigned to a dynamic location within the correct area", transferLine2.LocationStringInfo);
		}

		#endregion

		#region TestCheckLocationString_CheckFixLocationMaxProductType

		public void TestCheckLocationString_CheckFixLocationMaxProductType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var maximumNumberOfProducts = 1;
			var fixLocationType = Helper.CreateLocationType("AAA", "AAA Test", false, maximumNumberOfProducts, LocationClasses.Codes.FIX);
			var normalLocationType = Helper.CreateLocationType("BBB", "BBB Test", false, 0, LocationClasses.Codes.NOR);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var fixedLcoation = locations[0];
			var normalLocation = locations[1];
			var normalLocation2 = locations[2];
			fixedLcoation.WLV_WLT_LocationType = fixLocationType.PK;
			normalLocation.WLV_WLT_LocationType = normalLocationType.PK;
			normalLocation2.WLV_WLT_LocationType = normalLocationType.PK;
			var fixedLcoationName = fixedLcoation.ToLocationString();
			var normalLocationName = normalLocation.ToLocationString();

			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, data.Whs1, fixedLcoationName);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, normalLocation, "", false, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 2m, normalLocation, "", false, true);
			Factory.Save();

			var expectedErrMessages = "This location is a fixed pick face location and '{0}' is not assigned to this location.";

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, normalLocation, fixedLcoation);
			AssertNoErrors(transferLine1.LocationStringInfo);

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 2m, normalLocation, fixedLcoation);
			AssertNoErrors("Adjustment same product does not have error.", transferLine2.LocationStringInfo);

			var transfer3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine3 = Helper.CreateWhsTransferLine(transfer3, data.Part2, 2m, normalLocation, fixedLcoation);
			AssertHasError("When try to put other product to that location it should error.", transferLine3.LocationStringInfo, string.Format(expectedErrMessages, data.Part2.OP_PartNum));

			var transfer4 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine4_1 = Helper.CreateWhsTransferLine(transfer4, data.Part1, 2m, normalLocation, normalLocation2);
			var transferLine4_2 = Helper.CreateWhsTransferLine(transfer4, data.Part2, 2m, normalLocation, normalLocation2);
			AssertNoErrors("No Error for nurmal location", transferLine4_1.LocationStringInfo);
			AssertNoErrors("No Error for nurmal location", transferLine4_2.LocationStringInfo);
		}

		#endregion

		#region TestCheckLocationString_CheckFixLocationMaxProductType_TransferForPick_WorkOrder_Assembly

		public void TestCheckLocationString_CheckFixLocationMaxProductType_TransferForPick_WorkOrder_Assembly()
		{
			// SHould not show the fixed location error message on a Transfer for a Pick. Real stock on hand is not created by these transfers.
			// For an assembly work order, it may be valid for the user to have a fixed location setup for a kit but not its individual components.
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 2m);

			var fixLocationType = Helper.CreateLocationType("AAA", "AAA Test", false, 1, LocationClasses.Codes.FIX);
			var stagingArea1 = Helper.CreateArea(data.Whs1, "STAGING1");
			var stagingRow1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING1");
			var stagingLocation1 = stagingRow1.Locations.Single();
			stagingLocation1.WLV_WA_PickingArea = stagingArea1.PK;
			stagingLocation1.WLV_WLT_LocationType = fixLocationType.PK;
			Factory.Save();

			var whsClientParams = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			whsClientParams.W3_OH = data.Org1.PK;
			whsClientParams.W3_WW = data.Whs1.PK;
			whsClientParams.W3_WL_StagingLocationBOM = stagingLocation1.PK;

			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part2), data.Org1, data.Whs1, stagingLocation1.WLV_LocationString); // Kit on pick face, component is not
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);

			var pick = Helper.CreatePickNew(workOrder);
			foreach (var pickLine in pick.GetAllPickLines().ToArray())
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			}

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);

			var newTransfer = pick.Transfers.Single();
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals("Should finalise WorkOrder.", true, workOrder.IsFinalised);
			AssertNoErrors("Location should not have an error.", newTransfer.Lines[0].LocationStringInfo);
		}

		#endregion

		#region TestCheckLocationString_CheckFixLocationMaxProductType_TransferForPick_WorkOrder_Disassembly

		public void TestCheckLocationString_CheckFixLocationMaxProductType_TransferForPick_WorkOrder_Disassembly()
		{
			// SHould not show the fixed location error message on a Transfer for a Pick. Real stock on hand is not created by these transfers.
			// For a disassembly work order, it may be valid for the user to have a fixed location setup for the components but not the kit.
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 1m);

			var fixLocationType = Helper.CreateLocationType("AAA", "AAA Test", false, 1, LocationClasses.Codes.FIX);
			var stagingArea1 = Helper.CreateArea(data.Whs1, "STAGING1");
			var stagingRow1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "STAGING1");
			var stagingLocation1 = stagingRow1.Locations.Single();
			stagingLocation1.WLV_WA_PickingArea = stagingArea1.PK;
			stagingLocation1.WLV_WLT_LocationType = fixLocationType.PK;
			Factory.Save();

			var whsClientParams = WhsProduct.GetWhsProduct(data.Part2).ParamsByWhsAndClient.AddNew();
			whsClientParams.W3_OH = data.Org1.PK;
			whsClientParams.W3_WW = data.Whs1.PK;
			whsClientParams.W3_WL_StagingLocationBOM = stagingLocation1.PK;

			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, data.Whs1, stagingLocation1.WLV_LocationString); // Component on pick face, kit is not
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1", WorkOrderType.Codes.Disassemble);
			Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 1m);

			var pick = Helper.CreatePickNew(workOrder);
			foreach (var pickLine in pick.GetAllPickLines().ToArray())
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			}

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Should create one new Transfer.", 1, pick.Transfers.Count);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals("Should finalise WorkOrder.", true, workOrder.IsFinalised);

			var newTransfer = pick.Transfers.Single();
			AssertNoErrors("Location should not have an error.", newTransfer.Lines[0].LocationStringInfo);
		}

		#endregion

		#region TestCheckLocationString_MaxQuantityWithOtherPendingTransactions

		public void TestCheckLocationString_MaxQuantityWithOtherPendingTransactions()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			data.Whs1.FindLocation("A-2").WLV_MaxQuantity = 10m;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, loc1, "");
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 6m, loc1, loc2);
			transfer1.RunPreSaveValidation(); // to commit inventory.
			Factory.Save();

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2");
			var transferLine21 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 2m, loc1, loc2);
			var transferLine22 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 2m, loc1, loc2);
			AssertNoErrors(transferLine21.LocationStringInfo);
			AssertNoErrors(transferLine22.LocationStringInfo);
			transferLine21.WE_TransactionQuantity = 3m;
			transferLine21.Validation.ValidateLocationString();
			AssertHasError(transferLine21.LocationStringInfo, "Total required Quantity (5) exceeds the maximum available Quantity (4.000) for this location.");

			WhsEnvironment.IsRF = true;
			try
			{
				transferLine21.Validation.ValidateLocationString();
				AssertHasError(transferLine21.LocationStringInfo, "The Maximum Qty(10) for the destination location will be exceeded. Please select another location.");
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		#endregion

		#region TestCheckLocationString_ValidateMatchingLine_WE_WL

		#region TestCheckLocationString_ValidateMatchingLine_WE_WL_TRF

		public void TestCheckLocationString_ValidateMatchingLine_WE_WL_TRF()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fromLocation = data.Whs1.FindLocation("A-1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, fromLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 3m, fromLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 5m, fromLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to generate pick lines
			var matchingLine = transferLine.MatchingLines[0];
			AssertEquals("Precondition", 2, transferLine.MatchingLines.Count);
			AssertEquals("Precondition", false, transferLine.LocationStringInfo.HasErrors());
			AssertEquals("Precondition", true, transferLine.MatchingLines.All(l => l.WE_WL.Equals(transferLine.WE_WL)));
			AssertEquals("Precondition", "", ErrorReporter.LastMessageReported);

			const string expectedErrorMessage = "Something went wrong when saving this Transfer line. Please delete the line and re-enter the details.";
			matchingLine.WE_WL = ZGuid.Empty;
			transfer.RunPreSaveValidation();
			AssertHasRowError(transferLine, expectedErrorMessage);

			matchingLine.WE_WL = transferLine.WE_WL;
			transfer.RunPreSaveValidation();
			AssertNoRowError(transferLine, expectedErrorMessage);
		}

		#endregion

		#region TestCheckLocationString_InterWhs_ValidateMatchingLine_WE_WL

		public void TestCheckLocationString_InterWhs_ValidateMatchingLine_WE_WL_Dest()
		{
			TestCheckLocationString_InterWhs_ValidateMatchingLine_WE_WLCore(isSource: false);
		}

		public void TestCheckLocationString_InterWhs_ValidateMatchingLine_WE_WL_Source()
		{
			TestCheckLocationString_InterWhs_ValidateMatchingLine_WE_WLCore(isSource: true);
		}

		void TestCheckLocationString_InterWhs_ValidateMatchingLine_WE_WLCore(bool isSource)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("W2", "B");
			var transferWhs = isSource ? data.Whs1 : whs2;

			var fromLocation = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, fromLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 3m, fromLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 5m, fromLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, transferWhs, "TR1");
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", isSource ? whs2.PK : data.Whs1.PK, "B");
			transfer.RunPreSaveValidation(); // to generate pick lines
			var matchingLine = transferLine.MatchingLines[0];
			AssertEquals("Precondition", 2, transferLine.MatchingLines.Count);
			AssertEquals("Precondition", false, transferLine.LocationStringInfo.HasErrors());
			AssertEquals("Precondition", true, transferLine.MatchingLines.All(l => l.WE_WL.Equals(transferLine.WE_WL)));

			const string expectedErrorMessage = "Something went wrong when saving this Transfer line. Please delete the line and re-enter the details.";
			matchingLine.WE_WL = ZGuid.Empty;
			transfer.RunPreSaveValidation();
			AssertHasRowError(transferLine, expectedErrorMessage);

			matchingLine.WE_WL = transferLine.WE_WL;
			transfer.RunPreSaveValidation();
			AssertNoRowError(transferLine, expectedErrorMessage);

			matchingLine.WE_WL = data.Whs1.FindLocation("A-2").PK;
			transfer.RunPreSaveValidation();
			AssertHasRowError(transferLine, expectedErrorMessage);
		}

		#endregion

		#region TestCheckLocationString_InterWhs_ValidateWarehouseAndLocation_Source

		public void TestCheckLocationString_InterWhs_ValidateWarehouseAndLocation_Source_WithoutMatching()
		{
			TestCheckLocationString_InterWhs_ValidateWarehouseAndLocation_SourceCore(hasMatchingLine: false);
		}

		public void TestCheckLocationString_InterWhs_ValidateWarehouseAndLocation_Source_WithMatching()
		{
			TestCheckLocationString_InterWhs_ValidateWarehouseAndLocation_SourceCore(hasMatchingLine: true);
		}

		void TestCheckLocationString_InterWhs_ValidateWarehouseAndLocation_SourceCore(bool hasMatchingLine)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("W2", "B", 2, 1);

			var fromLocation = data.Whs1.FindLocation("A-1");
			if (hasMatchingLine)
			{
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, fromLocation, "");
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 3m, fromLocation, "");
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 5m, fromLocation, "");
			}
			else
			{
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, fromLocation, "");
			}

			Factory.Save();
			var toLocation = whs2.FindLocation("B-1");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", whs2.PK, "B-1");
			transfer.RunPreSaveValidation(); // to generate pick lines
			AssertEquals("Precondition", hasMatchingLine, transferLine.MatchingLines.Count > 0);
			AssertEquals("Precondition", false, transferLine.LocationStringInfo.HasErrors());
			AssertEquals("Precondition", true, transferLine.MatchingLines.All(l => l.WE_WL.Equals(transferLine.WE_WL)));

			const string expectedErrorMessage = "Location 'B-1' is not valid location.";
			transferLine.WE_WL = ZGuid.Empty;
			AssertEquals("All matching lines (if exists) should have same location", true, transferLine.MatchingLines.All(ml => ml.WE_WL.IsEmpty));
			transfer.FinaliseDocket(); // it runs validation
			AssertHasError(transferLine.LocationStringInfo, expectedErrorMessage);

			transferLine.WE_WL = toLocation.PK;
			AssertEquals("All matching lines (if exists) should have same location", true, transferLine.MatchingLines.All(ml => ml.WE_WL.Equals(toLocation.PK)));
			transfer.FinaliseDocket(); // it runs validation
			AssertNoError(transferLine.LocationStringInfo, expectedErrorMessage);
		}

		#endregion

		#endregion

		#region TestCheckLocationString_InwardProcessingArea

		public void TestCheckLocationString_InwardProcessingArea()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var location1 = locations[0];
			var location2 = locations[0];
			location1.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			location1.WLV_WA_PickingArea = inwardProcessingArea.PK;
			location2.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			location2.WLV_WA_PickingArea = inwardProcessingArea.PK;
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, location1, location2);
			AssertHasError(transferLine1.LocationStringInfo, "You cannot transfer stock from or to an Inward Processing area.");
			AssertHasError(transferLine1.TransferFromLocationStringInfo, "You cannot transfer stock from or to an Inward Processing area.");
		}

		#endregion

		#region TestCheckLocationString_InwardProcessingArea_NullTransfer

		public void TestCheckLocationString_InwardProcessingArea_NullTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var location1 = locations[0];
			var location2 = locations[0];
			location1.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			location1.WLV_WA_PickingArea = inwardProcessingArea.PK;
			location2.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			location2.WLV_WA_PickingArea = inwardProcessingArea.PK;
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, location1, location2);
			transferLine1.WE_WD = ZGuid.Empty;

			AssertNoExceptionThrown(() => transferLine1.Validation.ValidateAll());
			AssertHasError(transferLine1.TransferFromLocationStringInfo, "You cannot transfer stock from or to an Inward Processing area.");
		}

		#endregion

		#region TestCheckLocationString_InwardProcessingArea_StagingTransfer

		public void TestCheckLocationString_InwardProcessingArea_StagingTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var location1 = locations[0];
			var location2 = locations[0];
			location1.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			location1.WLV_WA_PickingArea = inwardProcessingArea.PK;
			location2.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			location2.WLV_WA_PickingArea = inwardProcessingArea.PK;
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, location1, location2);
			AssertHasError(transferLine1.LocationStringInfo, "You cannot transfer stock from or to an Inward Processing area.");
			AssertHasError(transferLine1.TransferFromLocationStringInfo, "You cannot transfer stock from or to an Inward Processing area.");

			transfer1.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			transferLine1.Validation.ValidateAll();
			AssertNoError(transferLine1.LocationStringInfo, "You cannot transfer stock from or to an Inward Processing area.");
			AssertNoError(transferLine1.TransferFromLocationStringInfo, "You cannot transfer stock from or to an Inward Processing area.");
		}

		#endregion

		#region TestCheckLocationString_CrossDocked

		public void TestCheckLocationString_CrossDocked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "A", 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			crossDockLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order.WD_WL_CrossDock = crossDockLocation.PK;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.ReserveStockIfAbleTo(inventory, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = (WhsTransferLine)transfer.CreateDocketLineFromInventory(inventory);
			transferLine.GS_NKPickedBy = staff.GS_Code;
			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;

			var pickLine = transferLine.PickLines.AddNew();
			pickLine.WZ_WE_InventoryLine = inventory.PK;
			pickLine.WZ_Units = inventory.WI_AvailableToTransferQuantity;
			pickLine.WZ_GS_NKAssignedTo = "AAA";

			transferLine.PickedTime = ZDateTimeOffset.Today;
			inventory.ReservedPickLines.ToArray().ForEach(pickLine => pickLine.WZ_WE_InventoryLine = transferLine.PK);
			Factory.Save();

			Assert("Transferline is a crossdock putaway.", transferLine.IsCrossDockPutaway);
			AssertNoErrors(transferLine.LocationStringInfo);

			transferLine.WE_WL = crossDockLocation.PK;
			transfer.RunPreSaveValidation();
			AssertNoErrors(transferLine.LocationStringInfo);
		}

		public void TestCheckLocationString_CrossDocked_OrderHasNoCrossDockLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "A", 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.ReserveStockIfAbleTo(inventory, 10m);

			var dockDoorLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCK").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = (WhsTransferLine)transfer.CreateDocketLineFromInventory(inventory);
			transferLine.GS_NKPickedBy = staff.GS_Code;
			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;

			var pickLine = transferLine.PickLines.AddNew();
			pickLine.WZ_WE_InventoryLine = inventory.PK;
			pickLine.WZ_Units = inventory.WI_AvailableToTransferQuantity;
			pickLine.WZ_GS_NKAssignedTo = "AAA";

			transferLine.PickedTime = ZDateTimeOffset.Today;
			inventory.ReservedPickLines.ToArray().ForEach(pickLine => pickLine.WZ_WE_InventoryLine = transferLine.PK);
			Factory.Save();

			Assert("Transferline is a crossdock putaway.", transferLine.IsCrossDockPutaway);
			Assert("Location is a dockdoor", dockDoorLocation.IsDockDoorLocation);
			AssertNoErrors(transferLine.LocationStringInfo);

			transferLine.WE_WL = dockDoorLocation.PK;
			transfer.RunPreSaveValidation();
			AssertHasError(transferLine.LocationStringInfo, "You cannot putaway to Dock Door locations.");
		}

		public void TestCheckLocationString_CrossDocked_OrderHasNoCrossDockLocation_NonDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "A", 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.ReserveStockIfAbleTo(inventory, 10m);

			var location = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = (WhsTransferLine)transfer.CreateDocketLineFromInventory(inventory);
			transferLine.GS_NKPickedBy = staff.GS_Code;
			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;

			var pickLine = transferLine.PickLines.AddNew();
			pickLine.WZ_WE_InventoryLine = inventory.PK;
			pickLine.WZ_Units = inventory.WI_AvailableToTransferQuantity;
			pickLine.WZ_GS_NKAssignedTo = "AAA";

			transferLine.PickedTime = ZDateTimeOffset.Today;
			inventory.ReservedPickLines.ToArray().ForEach(pickLine => pickLine.WZ_WE_InventoryLine = transferLine.PK);
			Factory.Save();

			Assert("Transferline is a crossdock putaway.", transferLine.IsCrossDockPutaway);
			AssertEquals("Location is not a dockdoor", false, location.IsDockDoorLocation);
			AssertNoErrors(transferLine.LocationStringInfo);

			transferLine.WE_WL = location.PK;
			transfer.RunPreSaveValidation();
			AssertNoErrors(transferLine.LocationStringInfo);
		}

		public void TestCheckLocationString_CrossDocked_InvalidCrossDockPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "A", 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			crossDockLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order.WD_WL_CrossDock = crossDockLocation.PK;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.ReserveStockIfAbleTo(inventory, 5m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = (WhsTransferLine)transfer.CreateDocketLineFromInventory(inventory);
			transferLine.GS_NKPickedBy = staff.GS_Code;
			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;

			var pickLine = transferLine.PickLines.AddNew();
			pickLine.WZ_WE_InventoryLine = inventory.PK;
			pickLine.WZ_Units = inventory.WI_AvailableToTransferQuantity;
			pickLine.WZ_GS_NKAssignedTo = "AAA";

			transferLine.PickedTime = ZDateTimeOffset.Today;
			inventory.ReservedPickLines.ToArray().ForEach(pickLine => pickLine.WZ_WE_InventoryLine = transferLine.PK);
			Factory.Save();

			Assert("Transferline is a crossdock putaway.", transferLine.IsCrossDockPutaway);
			AssertNoErrors(transferLine.LocationStringInfo);

			transferLine.WE_WL = crossDockLocation.PK;
			transfer.RunPreSaveValidation();
			AssertHasError(transferLine.LocationStringInfo, "Invalid cross dock putaway.");
		}

		public void TestCheckLocationString_CrossDocked_MultipleCrossDockLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "A", 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var crossDockLocation1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK1").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			crossDockLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order1.WD_WL_CrossDock = crossDockLocation1.PK;
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			orderLine1.ReserveStockIfAbleTo(inventory, 5m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var crossDockLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK2").Locations.Single();
			crossDockLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order2.WD_WL_CrossDock = crossDockLocation2.PK;
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			orderLine2.ReserveStockIfAbleTo(inventory, 5m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = (WhsTransferLine)transfer.CreateDocketLineFromInventory(inventory);
			transferLine.GS_NKPickedBy = staff.GS_Code;
			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;

			var pickLine = transferLine.PickLines.AddNew();
			pickLine.WZ_WE_InventoryLine = inventory.PK;
			pickLine.WZ_Units = inventory.WI_AvailableToTransferQuantity;
			pickLine.WZ_GS_NKAssignedTo = "AAA";

			transferLine.PickedTime = ZDateTimeOffset.Today;
			inventory.ReservedPickLines.ToArray().ForEach(pickLine => pickLine.WZ_WE_InventoryLine = transferLine.PK);
			Factory.Save();

			Assert("Transferline is a crossdock putaway.", transferLine.IsCrossDockPutaway);
			AssertNoErrors(transferLine.LocationStringInfo);

			transferLine.WE_WL = crossDockLocation1.PK;
			transfer.RunPreSaveValidation();
			AssertHasError(transferLine.LocationStringInfo, "Cross docked inventory cannot go to multiple cross dock locations.");
		}

		public void TestCheckLocationString_CrossDocked_WrongLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "A", 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			crossDockLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order.WD_WL_CrossDock = crossDockLocation.PK;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.ReserveStockIfAbleTo(inventory, 10m);

			var crossDockLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK2").Locations.Single();
			crossDockLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = (WhsTransferLine)transfer.CreateDocketLineFromInventory(inventory);
			transferLine.GS_NKPickedBy = staff.GS_Code;
			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;

			var pickLine = transferLine.PickLines.AddNew();
			pickLine.WZ_WE_InventoryLine = inventory.PK;
			pickLine.WZ_Units = inventory.WI_AvailableToTransferQuantity;
			pickLine.WZ_GS_NKAssignedTo = "AAA";

			transferLine.PickedTime = ZDateTimeOffset.Today;
			inventory.ReservedPickLines.ToArray().ForEach(pickLine => pickLine.WZ_WE_InventoryLine = transferLine.PK);
			Factory.Save();

			Assert("Transferline is a crossdock putaway.", transferLine.IsCrossDockPutaway);
			AssertNoErrors(transferLine.LocationStringInfo);

			transferLine.WE_WL = crossDockLocation2.PK;
			transfer.RunPreSaveValidation();
			AssertHasError(transferLine.LocationStringInfo, "Putaway location is not the same as the Order Cross Dock Location.");
		}

		#endregion

		#region	TestCheckChangePicker

		public void TestCheckChangePicker_NotPicking_SameUser()
		{
			SetPickerValueAndAssertAndSave("Set to same user should not have error.", assignToSameUser: true, isPicking: false, expectedError: false);
		}

		public void TestCheckChangePicker_NotPicking_DefferentUser()
		{
			SetPickerValueAndAssertAndSave("Can set user when is not picking.", assignToSameUser: false, isPicking: false, expectedError: false);
		}

		public void TestCheckChangePicker_Picking_SameUser()
		{
			SetPickerValueAndAssertAndSave("Can change the picker if is not picking.", assignToSameUser: true, isPicking: true, expectedError: false);
		}

		public void TestCheckChangePicker_Picking_DefferentUser()
		{
			SetPickerValueAndAssertAndSave("Cannot change the picker When is picking.", assignToSameUser: false, isPicking: true, expectedError: true);
		}

		void SetPickerValueAndAssertAndSave(string message, bool assignToSameUser, bool isPicking, bool expectedError)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var ben = Helper.CreateGlbStaff("Ben", "Ben");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, loc1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.Option = AssignLineOptions.PickOnly;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 6m, loc1, loc2);
			var lineAssigner = (ILineStaffAssigner)transferLine;
			lineAssigner.AssignLine(ben);
			transfer.RunPreSaveValidation(); // to commit inventory.
			Factory.Save();

			transferLine.PickLines[0].WZ_IsPicking = isPicking;

			if (assignToSameUser)
			{
				lineAssigner.AssignLine(ben);
			}
			else
			{
				var eli = Helper.CreateGlbStaff("Eli", "Eli");
				lineAssigner.AssignLine(eli);
			}

			if (expectedError)
			{
				AssertHasError(message, transferLine.GS_NKPickedByInfo, "Cannot be reassigned, Picking has already commenced.");
			}
			else
			{
				AssertNoErrors(message, transferLine.GS_NKPickedByInfo);
			}
		}

		#endregion

		#region TestValidateWE_WLAndLocationString_InTransit

		public void TestValidateWE_WLAndLocationString_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var client = data.Org1;
			var sourceLocation = data.Whs1.FindLocation("A-1");
			sourceLocation.WLV_MaxQuantity = 10m;
			var destLocation = data.Whs1.FindLocation("A-2");
			destLocation.WLV_MaxQuantity = 15m;
			var myProduct = Helper.CreateProduct(client, "Part4");
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(client, data.Whs1, "R1", myProduct, 10m, sourceLocation, "");
			var transfer = Helper.CreateWhsTransfer(client, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, myProduct, 10m, sourceLocation, destLocation);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
			Factory.Save();

			transferLine.WE_TransactionQuantity = 17;
			transferLine.Validation.ValidateWE_WL();
			AssertHasError(transferLine.WE_WLInfo, "Total required Quantity (17) exceeds the maximum available Quantity (15.000) for this location.");

			transferLine.Validation.ValidateLocationString();
			AssertHasError(transferLine.LocationStringInfo, "Total required Quantity (17) exceeds the maximum available Quantity (15.000) for this location.");
		}

		#endregion

		#region TestValidateWE_WLAndLocationString_Staged

		public void TestValidateWE_WLAndLocationString_Staged()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.DefaultOutboundDockDoorLocation.WLV_MaxQuantity = 15m;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 12m);
			var pick = Helper.CreatePickNew(order1);

			var pickLine = order1.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Entered; // Hack, validation won't run on finalised line
			transferLine.WE_FinalisedDate = ZDateTimeOffset.Empty;
			transferLine.WE_TransactionQuantity = 20m;
			transferLine.Validation.ValidateWE_WL();
			AssertHasError(transferLine.WE_WLInfo, "Total required Quantity (20) exceeds the maximum available Quantity (15.000) for this location.");

			transferLine.Validation.ValidateLocationString();
			AssertHasError(transferLine.LocationStringInfo, "Total required Quantity (20) exceeds the maximum available Quantity (15.000) for this location.");
		}

		#endregion

		#region TestCheckWE_WL_TotalPallets

		public void TestCheckWE_WL_TotalPallets()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");

			location1.WLV_PalletFloorSpaces = 2;
			location1.WLV_PalletStackHeight = 1;
			location2.WLV_PalletFloorSpaces = 1;
			location2.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-2");
			receive.FinaliseDocket();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 500m, location1.ToLocationString(), "Pallet-1", location2.ToLocationString(), "Pallet-3");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 500m, location1.ToLocationString(), "Pallet-2", location2.ToLocationString(), "Pallet-4");

			transfer.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			transferLine1.RunPreSaveValidation();
			transferLine2.RunPreSaveValidation();
			transfer.FinaliseDocket();

			var expectedErrorMessage = "Total required Pallets (2) exceeds the maximum available Pallets (1) for this location.";

			AssertHasError(transferLine1.WE_WLInfo, expectedErrorMessage);
			AssertHasError(transferLine2.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckLocationString_EmptyPalletID

		public void TestCheckLocationString_EmptyPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");

			location1.WLV_PalletFloorSpaces = 2;
			location1.WLV_PalletStackHeight = 1;
			location2.WLV_PalletFloorSpaces = 2;
			location2.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			receive.FinaliseDocket();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 500m, location1.ToLocationString(), "Pallet-1", location2.ToLocationString(), "");
			transferLine1.RunPreSaveValidation();

			AssertHasError(transferLine1.LocationStringInfo, "Inventory without Pallet ID cannot be put in locations using pallet spaces.");
		}

		public void TestCheckLocationString_EmptyPalletID_InventoryReturnToStockTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location = data.Whs1.FindLocation("A-1-1");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, location, string.Empty);
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			Helper.CreatePickNew(order);
			var putawayTransferLine = Helper.PickAndMakeInTransitTransfer(orderLine.PickLines.Single(), ZDateTimeOffset.Now);
			putawayTransferLine.WE_WL = data.Whs1.WW_DefaultOutboundDockDoor;
			putawayTransferLine.FinaliseDocketLine();

			Factory.Save();

			location.WLV_PalletFloorSpaces = 2;
			location.WLV_PalletStackHeight = 1;

			Factory.Save();

			var returnToStockTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T2");
			returnToStockTransfer.WD_DocketSubType = TransferType.Codes.Internal;
			returnToStockTransfer.WD_WD_ParentDocket = order.PK;

			var returnToStockLine = Helper.CreateWhsTransferLine(returnToStockTransfer, data.Part1, 50m, data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString(), string.Empty, location.ToLocationString(), string.Empty);
			returnToStockLine.RunPreSaveValidation();

			AssertHasWarning(returnToStockLine.LocationStringInfo, "Inventory without Pallet ID cannot be put in locations using pallet spaces.");
			AssertHasWarning(returnToStockLine.WE_PalletIDInfo, "Inventory without Pallet ID cannot be put in locations using pallet spaces.");

			returnToStockLine.FinaliseDocketLine();

			AssertHasError(returnToStockLine.LocationStringInfo, "Inventory without Pallet ID cannot be put in locations using pallet spaces.");
			AssertHasError(returnToStockLine.WE_PalletIDInfo, "Inventory without Pallet ID cannot be put in locations using pallet spaces.");
			AssertEquals(false, returnToStockLine.IsFinalised);
		}

		#endregion

		#region TestCheckWE_PalletID_EmptyPalletID

		public void TestCheckWE_PalletID_EmptyPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");

			location1.WLV_PalletFloorSpaces = 2;
			location1.WLV_PalletStackHeight = 1;
			location2.WLV_PalletFloorSpaces = 2;
			location2.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			receive.FinaliseDocket();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 500m, location1.ToLocationString(), "Pallet-1", location2.ToLocationString(), "");
			transferLine1.RunPreSaveValidation();

			AssertHasError(transferLine1.WE_PalletIDInfo, "Inventory without Pallet ID cannot be put in locations using pallet spaces.");
		}

		#endregion

		#region TestCheckWE_WL_MixedProducts

		public void TestCheckWE_WL_MixedProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			var location3 = data.Whs1.FindLocation("A-2-1");

			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 1;
			location2.WLV_PalletFloorSpaces = 1;
			location2.WLV_PalletStackHeight = 1;
			location3.WLV_PalletFloorSpaces = 2;
			location3.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 500m, location2, "Pallet-2");
			receive.FinaliseDocket();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 500m, location1.ToLocationString(), "Pallet-1", location3.ToLocationString(), "Pallet-3");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 500m, location2.ToLocationString(), "Pallet-2", location3.ToLocationString(), "Pallet-4");

			transfer.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			transferLine1.RunPreSaveValidation();
			transferLine2.RunPreSaveValidation();
			transfer.FinaliseDocket();

			var expectedErrorMessage = "Only a single product can be used in locations using Pallet Space capacities.";

			AssertHasError(transferLine1.WE_WLInfo, expectedErrorMessage);
			AssertHasError(transferLine2.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_OP_PalletConversion

		public void TestCheckWE_OP_WithoutPalletConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");

			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 1;
			location2.WLV_PalletFloorSpaces = 1;
			location2.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			receive.FinaliseDocket();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 500m, location1.ToLocationString(), "Pallet-1", location2.ToLocationString(), "Pallet-3");
			transferLine1.RunPreSaveValidation();

			AssertHasWarning(transferLine1.WE_OPInfo, "Products without pallet conversions cannot be put in locations using pallet spaces.");
		}

		public void TestCheckWE_OP_WithPalletConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");

			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 1;
			location2.WLV_PalletFloorSpaces = 1;
			location2.WLV_PalletStackHeight = 1;
			Helper.CreateProductUnit(data.Part1, "PLT", 500m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location1, "Pallet-1");
			receive.FinaliseDocket();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 500m, location1.ToLocationString(), "Pallet-1", location2.ToLocationString(), "Pallet-3");
			transferLine1.RunPreSaveValidation();

			AssertEquals(500m, data.Part1.OP_StockKeepingUnitPerPallet);
			AssertNoWarnings(transferLine1.WE_OPInfo);
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			var transferLine = DocketLine;

			// set incorrect data with suspended validation
			using (transferLine.GetValidationSuspender())
			{
				transferLine.GS_NKPickedBy = "BBB";
				transferLine.QtyToMoveIncludingMatchingLines = -5m;
			}
			AssertNoErrors(transferLine.GS_NKPickedByInfo);
			AssertNoErrors(transferLine.QtyToMoveIncludingMatchingLinesInfo);

			// ValidateAll should call the validation of following properties
			transferLine.Validation.ValidateAll();
			AssertHasErrors(transferLine.GS_NKPickedByInfo);
			AssertHasErrors(transferLine.QtyToMoveIncludingMatchingLinesInfo);
		}

		public void TestValidateAll_DestinationWarehousePK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("2", "B");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "");
			transferLine.DestinationWarehousePK = whs2.PK;
			AssertNoErrors(transferLine.DestinationWarehousePKInfo);

			using (transferLine.GetValidationSuspender())
			{
				transferLine.DestinationWarehousePK = data.Whs1.PK;
				AssertNoErrors(transferLine.DestinationWarehousePKInfo);
			}

			transferLine.Validation.ValidateAll();
			AssertHasError(transferLine.DestinationWarehousePKInfo, "The Source and Destination Warehouse cannot be the same on an Inter-Warehouse Transfer.");
		}

		#endregion

		#region Implementation

		string GetAreaError() => GetAreaError(DocketLine);

		string GetAreaError(WhsTransferLine transferLine)
		{
			var areaTypes = new AreaTypes();
			return "You cannot transfer stock from a " + areaTypes.GetDescriptionFromCode(transferLine.TransferFromLocation.PickingArea.WA_AreaType) +
				" area to a " + areaTypes.GetDescriptionFromCode(transferLine.Location.PickingArea.WA_AreaType) + " area.";
		}

		protected override FinalisableDocketHelper<WhsTransfer> GetNewDocketHelper()
		{
			return new FinalisableTransferHelper(Factory);
		}

		#endregion
	}
}
