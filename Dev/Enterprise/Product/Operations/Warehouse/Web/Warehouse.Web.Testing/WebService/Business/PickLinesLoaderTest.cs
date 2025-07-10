using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DbUpgrader.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class PickLinesLoaderTest : WhsTestCaseWithFactory
	{
		#region Constructors

		public void TestConstructorWithNullWarehouse()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PickLinesLoader(Factory, null, Factory.New<GlbStaff>(), new SearchFilterCriteriaInfo { }));
		}

		public void TestConstructorWithNullPicker()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PickLinesLoader(Factory, Factory.New<WhsWarehouse>(), null, new SearchFilterCriteriaInfo { }));
		}

		public void TestConstructor()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var picker = Factory.New<GlbStaff>();
			var linesPicker = new PickLinesLoader(Factory, warehouse, picker, new SearchFilterCriteriaInfo { AreaCode = "Area", PickMethod = "PickMethod", PickGroup = 1 });
			AssertNotNull(linesPicker);
			AssertEquals("AREA", linesPicker.Criteria.AreaCode);
			AssertEquals("PICKMETHOD", linesPicker.Criteria.PickMethod);
			AssertEquals(new ZShort(1), linesPicker.Criteria.PickGroup);
			AssertEquals(warehouse, linesPicker.Warehouse);
			AssertEquals(picker, linesPicker.Picker);
		}

		#endregion

		#region TestFindAndAssignNextPickByPickNo

		public void TestFindAndAssignNextPickByPickNo()
		{
			SetupEnvironmentData();
			TestByPickOrOrderReference(Pick1.WP_PickNo, Pick2.WP_PickNo, Pick3.WP_PickNo, "NoPickGroup", "OnePickGroup");
		}

		#endregion

		#region TestFindAndAssignNextPick_FactoryConcurrencySaveError

		public void TestFindAndAssignNextPick_FactoryConcurrencySaveError()
		{
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("ST2", "Staff2");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			Helper.CreatePickNew(order2);

			Factory.Saving += delegate
			{
				var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var orderInOtherFactory = otherFactory.Load<WhsOrder>(order1.PK);
				orderInOtherFactory.Lines[0].PickLines[0].WZ_GS_NKAssignedTo = staff2.GS_Code;
				otherFactory.Save();
			};

			FindPickResult result1 = null;
			AssertNoExceptionThrown(() => result1 = new PickLinesLoader(Factory, data.Whs1, staff1,
				new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(""));
			AssertEquals("Should be a concurrency error.", true, result1.IsConcurrencyError);
			AssertNull("Should not return a Pick when there is a concurrency error.", result1.Pick);
			AssertEquals("Should not return any PickLines when there is a concurrency error.", 0, result1.PickLines.Count());
			AssertEquals("Error Message should mention concurrency error.", "Another user has taken the next Pick.", result1.ErrorMessage);

			// should grab second order on the second go as the first order has been assigned
			var result2 = new PickLinesLoader(Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick("");
			AssertEquals("Retry should *not* be a concurrency error.", false, result2.IsConcurrencyError);
			AssertNotNull("Should return a Pick on retry.", result2.Pick);
			AssertEquals("Should return PickLines on retry.", 1, result2.PickLines.Count());
			AssertEquals("Should have no Error Message on retry.", "", result2.ErrorMessage);
		}

		#endregion

		#region TestPalletPartition_CheckMaxLength

		public void TestPalletPartition_CheckMaxLength()
		{
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("ST2", "Staff2");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines[0];
			receiveLine.WE_PalletID = "123456789012345678901234567890";
			Factory.Save();
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			Helper.CreatePickNew(order1);
			AssertNoExceptionThrown(() => new PickLinesLoader(Factory, data.Whs1, staff1,
				new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(""));
		}

		#endregion

		#region TestFindAndAssignNextPick_WithDataRefresh

		[UseSnapshotProtection]
		public void TestFindAndAssignNextPick_WithDataRefresh()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection1);
				var helper = new WhsTestHelperFunctions(factory);
				var staff1 = helper.CreateGlbStaff("ST1", "Staff1");
				var staff2 = helper.CreateGlbStaff("ST2", "Staff2");
				var data = new TestDataSimpleEnvironment(factory);
				var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				factory.Save();

				var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
				var firstPick = helper.CreatePickNew(order1);

				var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
				var secondPick = helper.CreatePickNew(order2);

				var errorMessage = new ZStringBuilder();
				bool taskRan = false;
				Thread thread = null;

				factory.Saving += delegate
				{
					Action<Action> handleException = action =>
					{
						try
						{
							action();
						}
						// If a thread throws an unhandled exception the application will crash, so we catch it here
						catch (Exception exception)
						{
							errorMessage.Append(exception.Message);
						}

						taskRan = true;
					};

					thread = new Thread(() => handleException(() =>
					{
						using (Db.DisposableActionForDbConnection())
						using (var connection2 = Db.NewExtraConnectionToMainDb())
						{
							connection2.BeginTransaction();
							var otherFactory = new BusinessObjectFactory(connection2);
							var warehouseInOtherFactory = otherFactory.Load<WhsWarehouse>(data.Whs1.PK);

							var staffQuery = new ZDBOnlyQuery(typeof(GlbStaff));
							staffQuery.AddToFilter(GlbStaffSchema.PK, staff2.PK);
							var staffInOtherFactory = otherFactory.LoadTop1<GlbStaff>(staffQuery); // need to load with a DB Only query as GlbStaff is cached in architecture.

							var resultInOtherThread = new PickLinesLoader(otherFactory, warehouseInOtherFactory, staffInOtherFactory, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick("");
							if (resultInOtherThread.IsConcurrencyError)
							{
								errorMessage.Append("Should not be a concurrency error.");
							}

							if (resultInOtherThread.Pick == null || resultInOtherThread.Pick.PK != secondPick.PK)
							{
								errorMessage.Append("Should return the correct Pick as Data Refresh would have updated the assigned pick lines.");
							}

							if (!resultInOtherThread.PickLines.Any())
							{
								errorMessage.Append("Should return correct PickLines.");
							}

							if (!resultInOtherThread.ErrorMessage.IsEmpty)
							{
								errorMessage.Append("There should be no Error Message. Error message was " + resultInOtherThread.ErrorMessage);
							}

							connection2.RollbackTransaction();
						}
					}));

					thread.Start();
					thread.Join(5000); // allow the second thread to start
				};

				FindPickResult findPickResult = null;
				AssertNoExceptionThrown(() => findPickResult = new PickLinesLoader(factory, data.Whs1, staff1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(""));

				thread.Join(); // allow the thread to finish
				AssertEquals("Concurrent Task should have finished correctly.", true, taskRan);

				if (!errorMessage.IsEmpty)
				{
					Fail(errorMessage.ToStringWithNewLineBetweenAppends());
				}

				AssertEquals("Should not be a concurrency error.", false, findPickResult.IsConcurrencyError);
				AssertEquals("Should return the correct Pick as Data Refresh would have updated the assigned pick lines.", firstPick, findPickResult.Pick);
				AssertEquals("Should return correct PickLines.", 1, findPickResult.PickLines.Count());
				AssertEquals("There should be no Error Message.", "", findPickResult.ErrorMessage);

				var pickLinesForOrder1 = order1.Lines.SelectMany(l => l.PickLines);
				AssertEquals("Precondition: Order is picked.", 1, pickLinesForOrder1.Count());
				AssertEquals("Order 1 should still be assigned to staff 1.", 1, order1.Lines.SelectMany(l => l.PickLines).Count(p => p.WZ_GS_NKAssignedTo == "ST1"));
			}
		}

		#endregion

		#region TestFindAndAssignNextPick_FactorySaveError

		public void TestFindAndAssignNextPick_FactorySaveError()
		{
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			BusinessObjectFactory.SavingEventHandler dropWZ_IsPicking = f =>
			{
				new DbColumnDependencyRemover(WhsPickLineSchema.Constants.SqlSchemaName, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.WZ_IsPicking).DropRelateObjects(TestConnection);
				TestConnection.ExecuteNonQuery(@"
ALTER TABLE dbo.WhsPickLine
DROP
	COLUMN WZ_IsPicking");
			};

			Factory.Saving += dropWZ_IsPicking;

			FindPickResult result = null;
			var loader = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			AssertNoExceptionThrown(() => result = loader.FindAndAssignNextPick(""));
			AssertEndsWith("Should have Error Message from Factory Save.", "Inner Message = Invalid column name 'WZ_IsPicking'.\r\n\r\n", result.ErrorMessage);

			Factory.Saving -= dropWZ_IsPicking;
		}

		#endregion

		#region TestFindAndAssignNextPick_ExcludesZeroUnitPickLines

		public void TestFindAndAssignNextPick_ExcludesZeroUnitPickLines()
		{
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var reservePickLine = order.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0]);
			AssertEquals("Precondition: Stock is reserved.", 5m, reservePickLine.ReservedQuantity);

			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 0m;
			Factory.Save();
			var result = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick("");
			AssertNull("Should not match any pick as the pick has only Zero Unit PickLines.", result.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_RequiresPutaway

		public void TestFindAndAssignNextPick_RequiresPutaway()
		{
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var result1 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick("");
			AssertNull("Should not return a putaway only pick if the user didn't explicitly scan the PickNo.", result1.Pick);

			var result2 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertEquals("Should return a Putaway Only result.", true, result2.IsPutawayOnlyPickResult);
			AssertEquals("Should return a Putaway Only result.", false, result2.PickLines.Any());
			AssertEquals(pick, result2.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_RequiresPutaway_WorkOrder

		public void TestFindAndAssignNextPick_RequiresPutaway_WorkOrder()
		{
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Factory);
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var bomPart1 = Helper.CreateProductBOM(data.Part2, componentProduct1, 1m, Constants.PkgUnit.Unit);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", componentProduct1, 100m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part2, 10m);
			var workOrderPick = Helper.CreatePickNew(workOrder);
			var pickLine = workOrderPick.GetAllPickLines().Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var result = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(workOrderPick.WP_PickNo);
			AssertNull("Should not return a pick as we do not support putaway to DDL for work orders.", result.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_RequiresPutaway_DifferentPutawayAndPickUser

		public void TestFindAndAssignNextPick_RequiresPutaway_DifferentPutawayAndPickUser()
		{
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("ST2", "Staff2");

			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff1.GS_Code;

			var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			inTransitLine.WE_GS_NKPutawayBy = staff2.GS_Code; // PutawayBy dfferent person to Picker
			Factory.Save();

			var result = new PickLinesLoader(Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertNull("Should not return a pick as there are no lines to pick or putaway for this user.", result.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_RequiresPutaway_LinesToPickForOtherUser

		public void TestFindAndAssignNextPick_RequiresPutaway_LinesToPickForOtherUser()
		{
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("ST2", "Staff2");

			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine.PickLines.Single();
			var pickLine2 = pickLine1.Split(5m);

			var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			inTransitLine.WE_GS_NKPutawayBy = staff1.GS_Code;
			pickLine2.WZ_GS_NKAssignedTo = staff2.GS_Code;
			Factory.Save();

			var result = new PickLinesLoader(Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertEquals("Should return a Putaway Only result.", true, result.IsPutawayOnlyPickResult);
			AssertEquals("Should return a Putaway Only result.", false, result.PickLines.Any());
			AssertEquals(pick, result.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_RequiresPutaway_LinesUnassigned

		public void TestFindAndAssignNextPick_RequiresPutaway_LinesUnassigned()
		{
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("ST2", "Staff2");

			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff1.GS_Code;
			var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			inTransitLine.WE_GS_NKPutawayBy = "";
			Factory.Save();

			var result = new PickLinesLoader(Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertNull("Should not return a pick as there are no lines to pick or putaway for this user.", result.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_RequiresPutaway_FiltersByClient

		public void TestFindAndAssignNextPick_RequiresPutaway_FiltersByClient()
		{
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var result1 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", ClientCode = "OTH" }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertNull("Should not return a pick as there are no lines to pick or putaway for this user.", result1.Pick);

			var result2 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", ClientCode = data.Org1.OH_Code }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertEquals("Should return a Putaway Only result.", true, result2.IsPutawayOnlyPickResult);
			AssertEquals("Should return a Putaway Only result.", false, result2.PickLines.Any());
			AssertEquals(pick, result2.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_RequiresPutaway_FiltersByPickMethod

		public void TestFindAndAssignNextPick_RequiresPutaway_FiltersByPickMethod()
		{
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			receive.Lines[0].Location.WLV_PickMethod = "PM1";
			data.Whs1.DefaultOutboundDockDoorLocation.WLV_PickMethod = "PM2";
			Factory.Save();

			var result1 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "PM2" }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertNull("Should not use the PickMethod from the dock door location.", result1.Pick);

			var result2 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "PM1" }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertEquals("Should return a Putaway Only result.", true, result2.IsPutawayOnlyPickResult);
			AssertEquals("Should return a Putaway Only result.", false, result2.PickLines.Any());
			AssertEquals(pick, result2.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_RequiresPutaway_FiltersByAreaCode

		public void TestFindAndAssignNextPick_RequiresPutaway_FiltersByAreaCode()
		{
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var result1 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = data.Whs1.DefaultOutboundDockDoorLocation.PickingArea.WA_Name, PickMethod = "ANY" }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertNull("Should not use the Area from the dock door location.", result1.Pick);

			var result2 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = receive.Lines[0].Location.PickingArea.WA_Name, PickMethod = "ANY" }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertEquals("Should return a Putaway Only result.", true, result2.IsPutawayOnlyPickResult);
			AssertEquals("Should return a Putaway Only result.", false, result2.PickLines.Any());
			AssertEquals(pick, result2.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_RequiresPutaway_FiltersByPickGroup

		public void TestFindAndAssignNextPick_RequiresPutaway_FiltersByPickGroup()
		{
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			orderLine.WE_PickGroup = 2;

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var result1 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 1 }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertNull("Should not find a pick with PickGroup = 1.", result1.Pick);

			var result2 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 2 }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertEquals("Should return a Putaway Only result.", true, result2.IsPutawayOnlyPickResult);
			AssertEquals("Should return a Putaway Only result.", false, result2.PickLines.Any());
			AssertEquals(pick, result2.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_RequiresPutaway_FiltersByUOMType

		public void TestFindAndAssignNextPick_RequiresPutaway_FiltersByUOMType()
		{
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "PLT", 10m);
			PackingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var result1 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", UOMType = UOMPackTypesList.Codes.Case }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertNull("Should not find a pick with UOM Type = Case.", result1.Pick);

			var result2 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", UOMType = UOMPackTypesList.Codes.Pallet }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertEquals("Should return a Putaway Only result.", true, result2.IsPutawayOnlyPickResult);
			AssertEquals("Should return a Putaway Only result.", false, result2.PickLines.Any());
			AssertEquals(pick, result2.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_RequiresPutaway_DisregardsEquipmentCapacity_UOMExceedsVolumeLimit

		public void TestFindAndAssignNextPick_RequiresPutaway_DisregardsEquipmentCapacity_UOMExceedsVolumeLimit()
		{
			var forkLift = Helper.CreateEquipment("testy", 0, Constants.Weight.Kilograms, 7, Constants.Volume.Litre, 0, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0, Constants.Weight.Kilograms, 0.5m, Constants.Volume.Litre);
			var cartonUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			cartonUnit.OF_Cubic = 8m;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Carton, Constants.PkgUnit.Skid, 4m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Skid, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 83m);
			var pick = Helper.CreatePickNew(order);

			var pickLineSkid = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Skid);
			var pickLineCarton = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton);
			var pickLineUnit = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit);
			AssertEquals("Precondition: Correct amount allocated for Skid.", 64m, pickLineSkid.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Carton.", 16m, pickLineCarton.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Unit.", 3m, pickLineUnit.WZ_Units);

			foreach (var pickLine in pick.GetAllPickLines())
			{
				pickLine.WZ_GS_NKAssignedTo = picker.GS_Code;
				Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			}
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(pick.WP_PickNo);
			AssertEquals("Should return a Putaway Only result.", true, result.IsPutawayOnlyPickResult);
			AssertEquals("Should return a Putaway Only result.", false, result.PickLines.Any());
			AssertEquals(pick, result.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_RequiresPutaway_DisregardsEquipmentCapacity_UOMExceedsVolumeLimit_PartiallyPicked

		public void TestFindAndAssignNextPick_RequiresPutaway_DisregardsEquipmentCapacity_UOMExceedsVolumeLimit_PartiallyPicked()
		{
			var forkLift = Helper.CreateEquipment("testy", 0, Constants.Weight.Kilograms, 7, Constants.Volume.Litre, 0, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0, Constants.Weight.Kilograms, 0.5m, Constants.Volume.Litre);
			var cartonUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			cartonUnit.OF_Cubic = 8m;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Carton, Constants.PkgUnit.Skid, 4m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Skid, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 91m);
			var pick = Helper.CreatePickNew(order);

			var pickLineSkid = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Skid);
			var pickLineCarton = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton);
			var pickLineUnit = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit);
			AssertEquals("Precondition: Correct amount allocated for Skid.", 64m, pickLineSkid.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Carton.", 24m, pickLineCarton.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Unit.", 3m, pickLineUnit.WZ_Units);

			foreach (var pickLine in pick.GetAllPickLines())
			{
				pickLine.WZ_GS_NKAssignedTo = picker.GS_Code;
			}

			var pickLineCartonSplit = pickLineCarton.Split(8m);
			Helper.PickAndMakeInTransitTransfer(pickLineCartonSplit, ZDateTimeOffset.Now);
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(pick.WP_PickNo);
			AssertEquals("Should return a Putaway Only result.", true, result.IsPutawayOnlyPickResult);
			AssertEquals("Should return a Putaway Only result.", false, result.PickLines.Any());
			AssertEquals(pick, result.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_RequiresPutaway_DisregardsEquipmentCapacity_UOMExceedsWeightLimit

		public void TestFindAndAssignNextPick_RequiresPutaway_DisregardsEquipmentCapacity_UOMExceedsWeightLimit()
		{
			var forkLift = Helper.CreateEquipment("testy", 7, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 0, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0.5m, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			var cartonUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			cartonUnit.OF_Weight = 8m;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Carton, Constants.PkgUnit.Skid, 4m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Skid, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 83m);
			var pick = Helper.CreatePickNew(order);

			var pickLineSkid = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Skid);
			var pickLineCarton = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton);
			var pickLineUnit = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit);
			AssertEquals("Precondition: Correct amount allocated for Skid.", 64m, pickLineSkid.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Carton.", 16m, pickLineCarton.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Unit.", 3m, pickLineUnit.WZ_Units);

			foreach (var pickLine in pick.GetAllPickLines())
			{
				pickLine.WZ_GS_NKAssignedTo = picker.GS_Code;
				Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			}
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(pick.WP_PickNo);
			AssertEquals("Should return a Putaway Only result.", true, result.IsPutawayOnlyPickResult);
			AssertEquals("Should return a Putaway Only result.", false, result.PickLines.Any());
			AssertEquals(pick, result.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_RequiresPutaway_CartonisedOrPickByLabelPick

		public void TestFindAndAssignNextPick_RequiresPutaway_CartonisedOrPickByLabelPick_Case()
		{
			TestFindAndAssignNextPick_RequiresPutaway_CartonisedOrPickByLabelPick(UOMPackTypesList.Codes.Case);
		}

		public void TestFindAndAssignNextPick_RequiresPutaway_CartonisedOrPickByLabelPick_Pallet()
		{
			TestFindAndAssignNextPick_RequiresPutaway_CartonisedOrPickByLabelPick(UOMPackTypesList.Codes.Pallet);
		}

		public void TestFindAndAssignNextPick_RequiresPutaway_CartonisedOrPickByLabelPick_SplitCase()
		{
			TestFindAndAssignNextPick_RequiresPutaway_CartonisedOrPickByLabelPick(UOMPackTypesList.Codes.SplitCase);
		}

		void TestFindAndAssignNextPick_RequiresPutaway_CartonisedOrPickByLabelPick(string uomType)
		{
			var operator1 = Helper.CreateGlbStaff("AAA", "A.A");
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);

			var part = Helper.CreateProduct(data.Org1, "1");
			part.OP_StockKeepingUnit = refType1.F3_Code;
			part.OP_Cubic = 0m; // Make it easy to cartonise
			part.OP_Weight = 0m; // Make it easy to cartonise

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", part, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", part, 10m);
			Factory.Save();

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10, 10, 10, 10, 20, 1, new ZByte(99), Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK; // lowest priority, has to exhaust all fallbacks

			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = uomType == UOMPackTypesList.Codes.SplitCase;
			pick.WP_PickPalletsByLabel = uomType == UOMPackTypesList.Codes.Pallet;
			pick.WP_PickCasesByLabel = uomType == UOMPackTypesList.Codes.Case;
			Factory.Save();

			pick.AllocatePackageLabels();
			AssertEquals("Precondition", 10, pick.OuterPackages.Count);
			pick.GetAllPickLines().ForEach(pl =>
			{
				pl.WZ_GS_NKAssignedTo = operator1.GS_Code;
				pl.WZ_PickedDateTime = ZDateTimeOffset.Now;
			});
			Factory.Save();

			var loader = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { });
			var result = loader.FindAndAssignNextPick(pick.WP_PickNo);
			AssertNull("Should not find a pick.", result.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_LinesAlreadyPutaway

		public void TestFindAndAssignNextPick_LinesAlreadyPutaway()
		{
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			Factory.Save();

			var result = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertNull("Should not find a pick.", result.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_ExcludeHoldCustomsOrder

		public void TestFindAndAssignNextPick_ExcludeHoldCustomsOrder()
		{
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Helper.SetOutwardsEntryKeyForOrderLine(order.Lines[0], "ABC");
			order.Logs.AddNew(Events.HoldTheWarehouseOrder);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var result = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick("");
			AssertNull("Should not load is order is not accepted", result.Pick);

			var allowFinaliseLog = order.Logs.AddNew(Events.WarehouseJobCanNowBeFinalised);
			Helper.SetLogUTCTimeOnFactorySave(TestConnection, allowFinaliseLog, ZDateTime.Now.AddDays(1));
			Factory.Save();

			result = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick("");
			AssertEquals("Should load is accepted order.", pick, result.Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_ExcludesInTransitPickLines_WithUnpickedLines

		public void TestFindAndAssignNextPick_ExcludesInTransitPickLines_WithUnpickedLines()
		{
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;

			var splitPickLine = pickLine.Split(4m);
			Helper.PickAndMakeInTransitTransfer(splitPickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var result = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertEquals("Should find the pick.", pick, result.Pick);
			AssertEquals("Should *not* be a Putaway Only result.", false, result.IsPutawayOnlyPickResult);
			AssertContainsExactElementsInAnyOrder("Should only contain pick lines for the unpicked order line.", orderLine.PickLines.Where(pl => pl.WZ_WE_OriginalPickedInventoryLine.IsEmpty), result.PickLines);
		}

		#endregion

		#region TestFindAndAssignNextPickByOrderRef

		public void TestFindAndAssignNextPickByOrderRef()
		{
			SetupEnvironmentData();
			TestByPickOrOrderReference("Order Ref1", "Order Ref2", "Order Ref3", "Order Ref4", "Order Ref5");
		}

		public void TestFindAndAssignNextPickByOrderRef_IsLocationEmptyAfterFinalisingPick_VerifyEmptyLocationsOn()
		{
			AssertFindAndAssignNextPickByOrderRef(true, p => p.WP_PickNo);
		}

		public void TestFindAndAssignNextPickByOrderRef_IsLocationEmptyAfterFinalisingPick_VerifyEmptyLocationsOff()
		{
			AssertFindAndAssignNextPickByOrderRef(false, p => p.WP_PickNo);
		}

		#endregion

		#region TestFindAndAssignNextPickByOrderExternalRef

		public void TestFindAndAssignNextPickByOrderExternalRef()
		{
			SetupEnvironmentData();
			TestByPickOrOrderReference(Pick1.Orders[0].WD_ExternalReference, Pick2.Orders[0].WD_ExternalReference, Pick3.Orders[0].WD_ExternalReference, "OrderWithNoPickGroup", "OrderWithOnePickGroup");
		}

		public void TestFindAndAssignNextPickByOrderExternalRef_IsLocationEmptyAfterFinalisingPick_VerifyEmptyLocationsOn()
		{
			AssertFindAndAssignNextPickByOrderRef(true, p => p.Orders[0].WD_ExternalReference);
		}

		public void TestFindAndAssignNextPickByOrderExternalRef_IsLocationEmptyAfterFinalisingPick_VerifyEmptyLocationsOff()
		{
			AssertFindAndAssignNextPickByOrderRef(false, p => p.Orders[0].WD_ExternalReference);
		}

		#endregion

		#region TestFindAndAssignNextPickByDocketID

		public void TestFindAndAssignNextPickByDocketID()
		{
			SetupEnvironmentData();
			TestByPickOrOrderReference(Pick1.Orders[0].WD_DocketID, Pick2.Orders[0].WD_DocketID, Pick3.Orders[0].WD_DocketID, "IDNoPickGroup", "IDOnePickGroup");
		}

		public void TestFindAndAssignNextPickByDocketID_IsLocationEmptyAfterFinalisingPick_VerifyEmptyLocationsOn()
		{
			AssertFindAndAssignNextPickByOrderRef(true, p => p.Orders[0].WD_DocketID);
		}

		public void TestFindAndAssignNextPickByDocketID_IsLocationEmptyAfterFinalisingPick_VerifyEmptyLocationsOff()
		{
			AssertFindAndAssignNextPickByOrderRef(false, p => p.Orders[0].WD_DocketID);
		}

		void AssertFindAndAssignNextPickByOrderRef(bool isVerifyEmptyLocation, Func<WhsPick, string> getReference)
		{
			// setup test data
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			data.Whs1.WW_VerifyEmptyLocations = isVerifyEmptyLocation;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "OP1";
			var nonEmptyLocationAfterFinalisingPick = data.Whs1.FindLocation("A-1-1");
			var emptyLocationAfterFinalisingPick = data.Whs1.FindLocation("A-1-2");

			// create receive, order and pick
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, emptyLocationAfterFinalisingPick);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m, nonEmptyLocationAfterFinalisingPick);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 100m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var loaderWithVerifyEmptyLocationsOn = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { });
			var result1 = loaderWithVerifyEmptyLocationsOn.FindAndAssignNextPick(getReference(pick));

			var nonEmptyLocationWithVerifyEmptyLocationsOn = result1.PickLines.Single(l => l.Inventory.Location == nonEmptyLocationAfterFinalisingPick);
			AssertEquals(false, ((IEmptyLocationAfterPickFinalisation)nonEmptyLocationWithVerifyEmptyLocationsOn).IsLocationEmptyAfterFinalisingPick);

			var emptyLocationWithVerifyEmptyLocationsOn = result1.PickLines.Single(l => l.Inventory.Location == emptyLocationAfterFinalisingPick);
			AssertEquals(isVerifyEmptyLocation, ((IEmptyLocationAfterPickFinalisation)emptyLocationWithVerifyEmptyLocationsOn).IsLocationEmptyAfterFinalisingPick);
		}

		#endregion

		#region TestFindAndAssignNextPickOldestUnfinalized

		public void TestFindAndAssignNextPickOldestUnfinalized()
		{
			SetupEnvironmentData();

			var operator1 = Factory.NewWithValidTestData<GlbStaff>();
			var operator2 = Factory.NewWithValidTestData<GlbStaff>();
			operator1.GS_Code = "OP1";
			operator2.GS_Code = "OP2";

			var location1Whs1 = Warehouse1.Rows.Single(r => r.WR_Name == "Row11").Locations;
			var location2Whs1 = Warehouse1.Rows.Single(r => r.WR_Name == "Row12").Locations;
			var location1Whs2 = Warehouse2.Rows.Single(r => r.WR_Name == "Row2").Locations;

			var result1 = new PickLinesLoader(Factory, Warehouse1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(ZString.Empty);
			AssertNotNull(result1.Pick);
			AssertAssignedPickAndLines(result1, Pick1, location1Whs1[0], location1Whs1[1], location1Whs1[2], location2Whs1[0], location2Whs1[1], location2Whs1[2]);

			SetupPickLine(Pick1, 0, 1, ZString.Empty, operator1, ZDateTimeOffset.Now);
			SetupPickLine(Pick1, 1, 0, "PM1", null, ZDateTimeOffset.Empty);
			SetupPickLine(Pick1, 1, 1, "PM1", operator1, ZDateTimeOffset.Now);
			SetupPickLine(Pick1, 2, 0, "PM2", operator2, ZDateTimeOffset.Empty);
			Factory.Save();

			var result2 = new PickLinesLoader(Factory, Warehouse1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(ZString.Empty);
			AssertNotNull(result2.Pick);
			AssertAssignedPickAndLines(result2, Pick1, location1Whs1[2], location2Whs1[0], location1Whs1[1]);

			var result3 = new PickLinesLoader(Factory, Warehouse1, operator1, new SearchFilterCriteriaInfo { AreaCode = "A1", PickMethod = "ANY" }).FindAndAssignNextPick(ZString.Empty);
			AssertNotNull(result3.Pick);
			AssertAssignedPickAndLines(result3, Pick1, location1Whs1[2], location1Whs1[1]);

			var result4 = new PickLinesLoader(Factory, Warehouse1, operator1, new SearchFilterCriteriaInfo { AreaCode = "A1", PickMethod = "PM1" }).FindAndAssignNextPick(ZString.Empty);
			AssertNotNull(result4.Pick);
			AssertAssignedPickAndLines(result4, Pick1, location1Whs1[2], location1Whs1[1]);

			var result5 = new PickLinesLoader(Factory, Warehouse1, operator1, new SearchFilterCriteriaInfo { AreaCode = "A2", PickMethod = "ANY" }).FindAndAssignNextPick(ZString.Empty);
			AssertNotNull(result5.Pick);
			AssertAssignedPickAndLines(result5, Pick1, location2Whs1[0]);

			var result6 = new PickLinesLoader(Factory, Warehouse1, operator1, new SearchFilterCriteriaInfo { AreaCode = "A1", PickMethod = "PM2" }).FindAndAssignNextPick(ZString.Empty);
			AssertNotNull(result6.Pick);
			AssertAssignedPickAndLines(result6, Pick1, location1Whs1[2]);

			var result7 = new PickLinesLoader(Factory, Warehouse1, operator2, new SearchFilterCriteriaInfo { AreaCode = "A1", PickMethod = "PM2" }).FindAndAssignNextPick(ZString.Empty);
			AssertNotNull(result7.Pick);
			AssertAssignedPickAndLines(result7, Pick1, location1Whs1[0]);

			Pick1.FinaliseAllOrders();
			AssertEquals(true, Pick1.GetFinalisableStatus().IsFinalisable);
			Pick1.FinalisePick();
			AssertEquals(true, Pick1.IsFinalised);
			Factory.Save();

			var result8 = new PickLinesLoader(Factory, Warehouse1, operator2, new SearchFilterCriteriaInfo { AreaCode = "A1", PickMethod = "PM2" }).FindAndAssignNextPick(ZString.Empty);
			AssertNotNull(result8.Pick);
			AssertEquals(2, result8.PickLines.Count());

			//not sure what these test for, but after spending way too much time reverse engineering this
			//result 9 seems to ensure that when picks are ordered "assigned to" comes before "pick method" but the pick lines for the selected pick are "pick method" before "assigned to"
			var result9 = new PickLinesLoader(Factory, Warehouse1, operator2, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(ZString.Empty);
			AssertNotNull(result9.Pick);
			AssertAssignedPickAndLines(result9, Pick3, location1Whs1[2], location2Whs1[0], location1Whs1[0], location1Whs1[1], location2Whs1[1], location2Whs1[2]);

			var result10 = new PickLinesLoader(Factory, Warehouse2, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(ZString.Empty);
			AssertNotNull(result10.Pick);
			AssertAssignedPickAndLines(result10, Pick2, location1Whs2[0], location1Whs2[1], location1Whs2[2]);
		}

		#endregion

		#region TestFindAndAssignNextPick_WaitingReplenishmentPicksAreNotReturned

		public void TestFindAndAssignNextPick_WaitingReplenishmentPicksAreNotReturned()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3m, true, true);
			Factory.Save();

			var orderForPickWithCreatedStatus = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m, WhsPickOption.Codes.Manual);
			var orderForPickWithWaitingReplenishmentUnAssigned = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m, WhsPickOption.Codes.Manual);
			var orderForPickWithWaitingReplenishmentAssigned = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 1m, WhsPickOption.Codes.Manual);

			var pickWithCreatedStatus = CreatePick(orderForPickWithCreatedStatus, PickStatus.Codes.Created, true, staff);
			var pickWithWaitingReplenishmentUnAssigned = CreatePick(orderForPickWithWaitingReplenishmentUnAssigned, PickStatus.Codes.Created, false, staff, true);
			var pickWithWaitingReplenishmentAssigned = CreatePick(orderForPickWithWaitingReplenishmentAssigned, PickStatus.Codes.Created, true, staff, true);
			Factory.Save();

			var loader = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			var result = loader.FindAndAssignNextPick(ZString.Empty);
			AssertEquals("Pick with created status should be returned.", pickWithCreatedStatus, result.Pick);

			((WhsPickAvailableInventory)result.Pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Single()).AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitByPickedDetails>().Single().PickedDate = ZDateTimeOffset.Now;
			Factory.Save();

			((WhsTransferLine)result.Pick.Transfers.Single().Lines.Single()).FinaliseDocketLine();
			Factory.Save();

			var nextPick = loader.FindAndAssignNextPick(ZString.Empty);
			AssertNull("All not finished picks are WaitingReplenishment, therefore should not be returned.", nextPick.Pick);

			var nextPickForPickNo = loader.FindAndAssignNextPick(pickWithWaitingReplenishmentAssigned.WP_PickNo);
			AssertNull("Pick no passed in is WaitingReplenishment, therefore should not be returned.", nextPickForPickNo.Pick);
		}

		WhsPick CreatePick(WhsOrder order, string status, bool isAllocate, GlbStaff staff, bool isAwaitingReplenishment = false)
		{
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickStatus = status;
			pick.WP_IsAwaitingReplenishment = isAwaitingReplenishment;
			var availableInventory = (WhsPickAvailableInventory)pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Single();
			availableInventory.Allocate = isAllocate;
			Helper.SetAssignToPk(availableInventory, staff.PK);
			return pick;
		}

		#endregion

		#region TestFindAndAssignNextPick_BuildingPicksFiltered

		public void TestFindAndAssignNextPick_BuildingPicksFiltered_RegistryOn()
		{
			TestFindAndAssignNextPick_BuildingPicksFilteredCore(true);
		}

		public void TestFindAndAssignNextPick_BuildingPicksFiltered_RegistryOff()
		{
			TestFindAndAssignNextPick_BuildingPicksFilteredCore(false);
		}

		void TestFindAndAssignNextPick_BuildingPicksFilteredCore(bool regEnabled)
		{
			using (WarehouseDataRegistry.Instance.EnablePickLineLoaderBuildingStatusFilter.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regEnabled))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var staff = Helper.CreateGlbStaff("ST1", "Staff1");

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, true, true);
				Factory.Save();

				var order_WithoutShortfall = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
				order_WithoutShortfall.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
				Helper.CreateWhsOrderLine(order_WithoutShortfall, data.Part1, 10m);

				var order_WithShortfallAndAllFullfillment1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2");
				order_WithShortfallAndAllFullfillment1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
				Helper.CreateWhsOrderLine(order_WithShortfallAndAllFullfillment1, data.Part1, 1000m);

				var order_WithShortfallAndAllFulfillment2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "3");
				order_WithShortfallAndAllFulfillment2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
				Helper.CreateWhsOrderLine(order_WithShortfallAndAllFulfillment2, data.Part1, 1000m);
				Factory.Save();

				var pickWithCreatedStatus = Helper.CreatePickNew(order_WithoutShortfall);
				AssertEquals("Precondition", PickStatus.Codes.Created, pickWithCreatedStatus.WP_PickStatus);

				var pickWithBuildingStatus1 = Helper.CreatePickNew(order_WithShortfallAndAllFullfillment1);
				AssertEquals("Precondition", PickStatus.Codes.Building, pickWithBuildingStatus1.WP_PickStatus);

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 100m, true, true);
				Factory.Save();

				var pickWithBuildingStatus2 = Helper.CreatePickNew(order_WithShortfallAndAllFulfillment2);
				AssertEquals("Precondition", PickStatus.Codes.Building, pickWithBuildingStatus2.WP_PickStatus);

				var loader = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
				var result = loader.FindAndAssignNextPick(ZString.Empty);
				AssertEquals("Pick with created status should be returned.", pickWithCreatedStatus, result.Pick);

				var nextPickForBuilding1 = loader.FindAndAssignNextPick(pickWithBuildingStatus1.WP_PickNo);
				var nextPickForBuilding2 = loader.FindAndAssignNextPick(pickWithBuildingStatus2.WP_PickNo);

				if (!regEnabled)
				{
					AssertEquals("Pick Num passed in is Building, therefore should be returned.", pickWithBuildingStatus1, nextPickForBuilding1.Pick);
					AssertEquals("Pick Num passed in is Building, therefore should be returned.", pickWithBuildingStatus2, nextPickForBuilding2.Pick);
				}
				else
				{
					AssertNull("Pick Num passed in is Building, therefore should NOT be returned.", nextPickForBuilding1.Pick);
					AssertNull("Pick Num passed in is Building, therefore should NOT be returned.", nextPickForBuilding2.Pick);
				}
			}
		}

		#endregion

		#region TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick

		public void TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick_Case()
		{
			TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick(UOMPackTypesList.Codes.Case);
		}

		public void TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick_Pallet()
		{
			TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick(UOMPackTypesList.Codes.Pallet);
		}

		public void TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick_SplitCase()
		{
			TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick(UOMPackTypesList.Codes.SplitCase);
		}

		void TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick(string uomType)
		{
			var operator1 = Helper.CreateGlbStaff("AAA", "A.A");
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);

			var part = Helper.CreateProduct(data.Org1, "1");
			part.OP_StockKeepingUnit = refType1.F3_Code;
			part.OP_Cubic = 0m; // Make it easy to cartonise
			part.OP_Weight = 0m; // Make it easy to cartonise

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", part, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", part, 10m);
			Factory.Save();

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10, 10, 10, 10, 20, 1, new ZByte(99), Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK; // lowest priority, has to exhaust all fallbacks

			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = uomType == UOMPackTypesList.Codes.SplitCase;
			pick.WP_PickPalletsByLabel = uomType == UOMPackTypesList.Codes.Pallet;
			pick.WP_PickCasesByLabel = uomType == UOMPackTypesList.Codes.Case;
			Factory.Save();

			pick.AllocatePackageLabels();
			AssertEquals("Precondition", 10, pick.OuterPackages.Count);
			Factory.Save();

			var loader = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { });
			var result_NoReference = loader.FindAndAssignNextPick("");
			AssertNull("Should *not* have returned pick.", result_NoReference.Pick);
			AssertEquals("Should *not* have returned pick.", false, result_NoReference.PickLines.Any());

			var result_WithReference = loader.FindAndAssignNextPick(pick.WP_PickNo);
			AssertNull("Should *not* have returned pick.", result_WithReference.Pick);
			AssertEquals("Should *not* have returned pick.", false, result_WithReference.PickLines.Any());

			// Assert query uses AND correctly
			pick.WP_CartoniseSplitCases = true;
			pick.WP_PickPalletsByLabel = true;
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			AssertNull("Should not find the pick.", loader.FindAndAssignNextPick("").Pick);
			AssertNull("Should not find the pick.", new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { UOMType = uomType }).FindAndAssignNextPick("").Pick);

			// Hack to make not cartonised / picked by label
			pick.WP_CartoniseSplitCases = false;
			pick.WP_PickPalletsByLabel = false;
			pick.WP_PickCasesByLabel = false;
			Factory.Save();

			AssertNotNull("Should now find the pick.", loader.FindAndAssignNextPick("").Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick_UsesUOMTypeAndNotPackType

		public void TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick_UsesUOMTypeAndNotPackType_Case()
		{
			TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick_UsesUOMTypeAndNotPackType(UOMPackTypesList.Codes.Case);
		}

		public void TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick_UsesUOMTypeAndNotPackType_Pallet()
		{
			TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick_UsesUOMTypeAndNotPackType(UOMPackTypesList.Codes.Pallet);
		}

		public void TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick_UsesUOMTypeAndNotPackType_SplitCase()
		{
			TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick_UsesUOMTypeAndNotPackType(UOMPackTypesList.Codes.SplitCase);
		}

		void TestFindAndAssignNextPick_FiltersCartonisedOrPickByLabelPick_UsesUOMTypeAndNotPackType(string packType)
		{
			var operator1 = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 =
				Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, packType)
				?? PackingHelper.CreateRefPackType(packType, "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, "");

			var part = Helper.CreateProduct(data.Org1, "1");
			part.OP_StockKeepingUnit = refType1.F3_Code;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", part, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, part, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = packType == UOMPackTypesList.Codes.SplitCase;
			pick.WP_PickPalletsByLabel = packType == UOMPackTypesList.Codes.Pallet;
			pick.WP_PickCasesByLabel = packType == UOMPackTypesList.Codes.Case;
			Factory.Save();

			var loader = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { });
			var result_NoReference = loader.FindAndAssignNextPick("");
			AssertNotNull("Should have returned the pick", result_NoReference.Pick);
			AssertEquals("Should have returned the pick", true, result_NoReference.PickLines.Any());
		}

		#endregion

		#region TestFindAndAssignNextPick_FiltersOutCartonisedOrPickByLabelLines

		public void TestFindAndAssignNextPick_FiltersOutCartonisedOrPickByLabelLines_Case()
		{
			TestFindAndAssignNextPick_FiltersOutCartonisedOrPickByLabelLines(UOMPackTypesList.Codes.Case);
		}

		public void TestFindAndAssignNextPick_FiltersOutCartonisedOrPickByLabelLines_Pallet()
		{
			TestFindAndAssignNextPick_FiltersOutCartonisedOrPickByLabelLines(UOMPackTypesList.Codes.Pallet);
		}

		public void TestFindAndAssignNextPick_FiltersOutCartonisedOrPickByLabelLines_SplitCase()
		{
			TestFindAndAssignNextPick_FiltersOutCartonisedOrPickByLabelLines(UOMPackTypesList.Codes.SplitCase);
		}

		void TestFindAndAssignNextPick_FiltersOutCartonisedOrPickByLabelLines(string uomType)
		{
			var operator1 = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType_SplitCase = PackingHelper.CreateRefPackType("1", "1", 1m, 2m, 3m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			var refType_Case = PackingHelper.CreateRefPackType("2", "2", 1m, 4m, 9m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Case);
			var refType_Pallet = PackingHelper.CreateRefPackType("3", "3", 1m, 1m, 3m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Pallet);

			var part_SplitCase = Helper.CreateProduct(data.Org1, "1");
			var part_Case = Helper.CreateProduct(data.Org1, "2");
			var part_Pallet = Helper.CreateProduct(data.Org1, "3");
			part_SplitCase.OP_StockKeepingUnit = refType_SplitCase.F3_Code;
			part_Case.OP_StockKeepingUnit = refType_Case.F3_Code;
			part_Pallet.OP_StockKeepingUnit = refType_Pallet.F3_Code;
			part_SplitCase.OP_Cubic = 0m; // Make it easy to cartonise
			part_SplitCase.OP_Weight = 0m; // Make it easy to cartonise

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", part_SplitCase, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", part_Case, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part_Pallet, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, part_SplitCase, 10m);
			Helper.CreateWhsOrderLine(order, part_Case, 10m);
			Helper.CreateWhsOrderLine(order, part_Pallet, 10m);
			Factory.Save();

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10, 10, 10, 10, 20, 1, new ZByte(99), Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK; // lowest priority, has to exhaust all fallbacks

			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = uomType != UOMPackTypesList.Codes.SplitCase;
			pick.WP_PickPalletsByLabel = uomType != UOMPackTypesList.Codes.Pallet;
			pick.WP_PickCasesByLabel = uomType != UOMPackTypesList.Codes.Case;
			Factory.Save();

			pick.AllocatePackageLabels();
			AssertEquals("Precondition", 20, pick.OuterPackages.Count);
			AssertContainsExactElementsInAnyOrder("Precondition",
				new[] { UOMPackTypesList.Codes.SplitCase, UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.Pallet },
				pick.GetAllPickLines().Select(pl => (string)pl.AllocatedPackType.F3_UOMType).Distinct());
			Factory.Save();

			var loader = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { });
			var result_NoReference = loader.FindAndAssignNextPick("");
			AssertNotNull("Should have returned pick.", result_NoReference.Pick);
			AssertEquals("Should have filtered returned lines.", true, result_NoReference.PickLines.Cast<WhsPickLine>().All(pl => pl.AllocatedPackType.F3_UOMType == uomType));

			var result_WithReference = loader.FindAndAssignNextPick(pick.WP_PickNo);
			AssertNotNull("Should have returned pick.", result_WithReference.Pick);
			AssertEquals("Should have filtered returned lines.", true, result_WithReference.PickLines.Cast<WhsPickLine>().All(pl => pl.AllocatedPackType.F3_UOMType == uomType));

			var result_WithSameUOMRegistered = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { UOMType = uomType }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertNotNull("Should have returned pick.", result_WithSameUOMRegistered.Pick);
			AssertEquals("Should have filtered returned lines.", true, result_WithSameUOMRegistered.PickLines.Cast<WhsPickLine>().All(pl => pl.AllocatedPackType.F3_UOMType == uomType));

			var otherUOMType = new[] { UOMPackTypesList.Codes.SplitCase, UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.Pallet }.First(uom => uom != uomType);
			var result_WithDifferentUOMRegistered = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { UOMType = otherUOMType }).FindAndAssignNextPick(pick.WP_PickNo);
			AssertNull("Should *not* have returned pick.", result_WithDifferentUOMRegistered.Pick);
			AssertEquals("Should *not* have returned pick.", 0, result_WithDifferentUOMRegistered.PickLines.Count());

			// Hack to make not cartonised / picked by label
			pick.WP_CartoniseSplitCases = false;
			pick.WP_PickPalletsByLabel = false;
			pick.WP_PickCasesByLabel = false;
			Factory.Save();

			AssertEquals("Should *not* have filtered returned lines.", false, loader.FindAndAssignNextPick("").PickLines.Cast<WhsPickLine>().All(pl => pl.AllocatedPackType.F3_UOMType == uomType));
		}

		#endregion

		#region TestFindAndAssignNextPick_SortAttributes

		public void TestFindAndAssignNextPick_SortAttributes_NotPalletIDNeutral()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			data.Whs1.LocationType.WLT_IsPalletIDNeutral = false;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");

			var part1 = Helper.CreateProduct(data.Org1, "PART1");
			var part2 = Helper.CreateProduct(data.Org1, "PART2");
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, part1, true);
			Helper.SetProductAllAttributeUse(data.Org1, part2, true);

			var tomorrow = ZDate.Today.AddDays(1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o1");

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			SetInventory(receive, tomorrow, tomorrow, "", "", "att3", "", locationPK, part1.PK, palletID: "PALLET1");
			SetInventory(receive, tomorrow.AddDays(1), tomorrow, "", "", "", "", locationPK, part1.PK, palletID: "PALLET2");
			SetInventory(receive, tomorrow, tomorrow, "att1", "", "", "", locationPK, part1.PK, palletID: "PALLET3");
			SetInventory(receive, tomorrow, tomorrow.AddDays(1), "", "", "", "", locationPK, part1.PK, palletID: "PALLET4");
			SetInventory(receive, tomorrow, tomorrow, "", "att2", "", "", locationPK, part1.PK, palletID: "PALLET5");
			SetInventory(receive, tomorrow, tomorrow, "", "", "", "", locationPK, part1.PK, palletID: "PALLET6", qty: 15m);
			SetInventory(receive, tomorrow, tomorrow, "", "", "", "", locationPK, part2.PK, palletID: "PALLET7");
			SetInventory(receive, tomorrow, tomorrow, "att1", "", "", "", locationPK, part2.PK, palletID: "PALLET7", qty: 15m);
			SetInventory(receive, tomorrow, tomorrow, "", "att2", "", "", locationPK, part2.PK, palletID: "PALLET7");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			Helper.CreateWhsOrderLine(order, part1, 65m);
			Helper.CreateWhsOrderLine(order, part2, 35m);
			Helper.CreatePickNew(order);
			Factory.Save();
			order.WD_DocketID = order.WD_ExternalReference;
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { });
			var result = linesPicker.FindAndAssignNextPick(order.WD_ExternalReference);
			AssertEquals("All picklines should be retained.", 9, result.PickLines.Count());
			AssertEquals("Pallet ID should be correct.", "PALLET7", result.PickLines.ElementAt(0).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET7", result.PickLines.ElementAt(1).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET7", result.PickLines.ElementAt(2).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET6", result.PickLines.ElementAt(3).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET5", result.PickLines.ElementAt(4).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET4", result.PickLines.ElementAt(5).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET3", result.PickLines.ElementAt(6).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET2", result.PickLines.ElementAt(7).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET1", result.PickLines.ElementAt(8).InventoryLine.WE_PalletID);
		}

		public void TestFindAndAssignNextPick_SortAttributes_NotPalletIDNeutral_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			data.Whs1.LocationType.WLT_IsPalletIDNeutral = false;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");

			var part1 = Helper.CreateProduct(data.Org1, "PART1");
			var part2 = Helper.CreateProduct(data.Org1, "PART2");
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, part1, true);
			Helper.SetProductAllAttributeUse(data.Org1, part2, true);

			var tomorrow = ZDate.Today.AddDays(1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o1");

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var inventory = SetInventory(receive, tomorrow, tomorrow, "", "", "", "", locationPK, part1.PK, palletID: "PALLET0", qty: 1);
			SetInventory(receive, tomorrow, tomorrow, "", "", "att3", "", locationPK, part1.PK, palletID: "PALLET1", qty: 1);
			SetInventory(receive, tomorrow.AddDays(1), tomorrow, "", "", "", "", locationPK, part1.PK, palletID: "PALLET2", qty: 1);
			SetInventory(receive, tomorrow, tomorrow, "att1", "", "", "", locationPK, part1.PK, palletID: "PALLET3", qty: 1);
			SetInventory(receive, tomorrow, tomorrow.AddDays(1), "", "", "", "", locationPK, part1.PK, palletID: "PALLET4", qty: 1);
			SetInventory(receive, tomorrow, tomorrow, "", "att2", "", "", locationPK, part1.PK, palletID: "PALLET5", qty: 1);
			SetInventory(receive, tomorrow, tomorrow, "", "", "", "", locationPK, part1.PK, palletID: "PALLET6", qty: 1);
			SetInventory(receive, tomorrow, tomorrow, "", "", "", "", locationPK, part2.PK, palletID: "PALLET7", qty: 1);
			SetInventory(receive, tomorrow, tomorrow, "att1", "", "", "", locationPK, part2.PK, palletID: "PALLET7", qty: 1);
			SetInventory(receive, tomorrow, tomorrow, "", "att2", "", "", locationPK, part2.PK, palletID: "PALLET7", qty: 1);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			inventory.WI_SerialNumber = "SN1"; // Hack to set serial
			Factory.Save();

			Helper.CreateWhsOrderLine(order, part1, 7m);
			Helper.CreateWhsOrderLine(order, part2, 3m);
			Helper.CreatePickNew(order);
			Factory.Save();
			order.WD_DocketID = order.WD_ExternalReference;
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { });
			var result = linesPicker.FindAndAssignNextPick(order.WD_ExternalReference);
			AssertEquals("All picklines should be retained.", 10, result.PickLines.Count());
			AssertEquals("Pallet ID should be correct.", "PALLET7", result.PickLines.ElementAt(0).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET7", result.PickLines.ElementAt(1).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET7", result.PickLines.ElementAt(2).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET6", result.PickLines.ElementAt(3).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET5", result.PickLines.ElementAt(4).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET4", result.PickLines.ElementAt(5).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET3", result.PickLines.ElementAt(6).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET2", result.PickLines.ElementAt(7).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET1", result.PickLines.ElementAt(8).InventoryLine.WE_PalletID);
			AssertEquals("Pallet ID should be correct.", "PALLET0", result.PickLines.ElementAt(9).InventoryLine.WE_PalletID);
		}

		public void TestFindAndAssignNextPick_SortAttributes_PalletIDNeutral()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");

			var part1 = Helper.CreateProduct(data.Org1, "PART1");
			var part2 = Helper.CreateProduct(data.Org1, "PART2");
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, part1, true);
			Helper.SetProductAllAttributeUse(data.Org1, part2, true);

			data.Whs1.LocationType.WLT_IsPalletIDNeutral = true;
			Assert("Precondition: Is Pallet ID Neutral.", data.Whs1.LocationType.WLT_IsPalletIDNeutral);
			var expiryDate1 = ZDate.Today.AddDays(1);
			var expiryDate2 = expiryDate1.AddDays(1);
			var expiryDate3 = expiryDate1.AddDays(10);
			var packingDate = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o1");

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			SetInventory(receive, expiryDate1, packingDate, "att1", "", "", "", locationPK, part1.PK, palletID: "PALLET1");
			SetInventory(receive, expiryDate1, packingDate, "att1", "", "", "", locationPK, part1.PK, palletID: "PALLET2", qty: 20m);
			SetInventory(receive, expiryDate1, packingDate, "", "att2", "", "", locationPK, part1.PK, palletID: "PALLET3");
			SetInventory(receive, expiryDate1, packingDate, "", "att2", "", "", locationPK, part1.PK, palletID: "PALLET4");
			SetInventory(receive, expiryDate1, packingDate, "", "", "att3", "", locationPK, part1.PK, palletID: "PALLET5");
			SetInventory(receive, expiryDate1, packingDate, "", "", "att3", "", locationPK, part1.PK, palletID: "PALLET6");
			SetInventory(receive, expiryDate2, packingDate, "", "", "", "", locationPK, part1.PK, palletID: "PALLET7");
			SetInventory(receive, expiryDate2, packingDate, "", "", "", "", locationPK, part1.PK, palletID: "PALLET8", qty: 15m);
			SetInventory(receive, expiryDate3, packingDate, "", "", "", "", locationPK, part1.PK, palletID: "PALLET9");
			SetInventory(receive, expiryDate3, packingDate, "", "", "", "", locationPK, part1.PK, palletID: "PALLET10");
			SetInventory(receive, expiryDate1, packingDate, "", "", "", "", locationPK, part2.PK, palletID: "PALLET11");
			SetInventory(receive, expiryDate1, packingDate, "", "", "", "", locationPK, part2.PK, palletID: "PALLET12", qty: 15m);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var orderLine1 = Helper.CreateWhsOrderLine(order, part1, 25m);
			orderLine1.WE_ExpiryDate = expiryDate2;
			var orderLine2 = Helper.CreateWhsOrderLine(order, part1, 20m);
			orderLine2.WE_ExpiryDate = expiryDate3;
			orderLine2.WE_PackingDate = packingDate;
			var orderLine3 = Helper.CreateWhsOrderLine(order, part1, 30m);
			orderLine3.WE_PartAttrib1 = "att1";
			var orderLine4 = Helper.CreateWhsOrderLine(order, part1, 20m);
			orderLine4.WE_PartAttrib2 = "att2";
			var orderLine5 = Helper.CreateWhsOrderLine(order, part2, 25m);
			var orderLine6 = Helper.CreateWhsOrderLine(order, part1, 20m);
			orderLine6.WE_PartAttrib3 = "att3";

			Helper.CreatePickNew(order);
			Factory.Save();
			order.WD_DocketID = order.WD_ExternalReference;
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { });
			var result = linesPicker.FindAndAssignNextPick(order.WD_ExternalReference);
			AssertEquals("All picklines should be retained.", 12, result.PickLines.Distinct().Count());
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(0), "PART1", 10m, "", "", "", "", expiryDate: expiryDate2);
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(1), "PART1", 10m, "", "", "", "", expiryDate: expiryDate3, packingDate: packingDate);
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(2), "PART1", 10m, "", "", "", "", expiryDate: expiryDate3, packingDate: packingDate);
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(3), "PART1", 10m, "", "", "att3", "");
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(4), "PART1", 10m, "", "", "att3", "");
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(5), "PART1", 10m, "", "att2", "", "");
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(6), "PART1", 10m, "", "att2", "", "");
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(7), "PART1", 10m, "att1", "", "", "");
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(8), "PART1", 15m, "", "", "", "", expiryDate: expiryDate2);
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(9), "PART1", 20m, "att1", "", "", "");
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(10), "PART2", 10m, "", "", "", "");
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(11), "PART2", 15m, "", "", "", "");
		}

		public void TestFindAndAssignNextPick_SortAttributes_PalletIDNeutral_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");

			var part1 = Helper.CreateProduct(data.Org1, "PART1");
			var part2 = Helper.CreateProduct(data.Org1, "PART2");
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, part1, true);
			Helper.SetProductAllAttributeUse(data.Org1, part2, true);

			data.Whs1.LocationType.WLT_IsPalletIDNeutral = true;
			Assert("Precondition: Is Pallet ID Neutral.", data.Whs1.LocationType.WLT_IsPalletIDNeutral);
			var expiryDate1 = ZDate.Today.AddDays(1);
			var expiryDate2 = expiryDate1.AddDays(1);
			var expiryDate3 = expiryDate1.AddDays(10);
			var packingDate = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o1");

			var locationPK = data.Whs1.FindLocation("A-1-1").PK;
			var inventory = SetInventory(receive, expiryDate1, packingDate, "", "", "", "", locationPK, part1.PK, palletID: "PALLET0", qty: 1m);
			SetInventory(receive, expiryDate1, packingDate, "att1", "", "", "", locationPK, part1.PK, palletID: "PALLET1", qty: 1m);
			SetInventory(receive, expiryDate1, packingDate, "att1", "", "", "", locationPK, part1.PK, palletID: "PALLET2", qty: 2m);
			SetInventory(receive, expiryDate1, packingDate, "", "att2", "", "", locationPK, part1.PK, palletID: "PALLET3", qty: 1m);
			SetInventory(receive, expiryDate1, packingDate, "", "att2", "", "", locationPK, part1.PK, palletID: "PALLET4", qty: 1m);
			SetInventory(receive, expiryDate1, packingDate, "", "", "att3", "", locationPK, part1.PK, palletID: "PALLET5", qty: 1m);
			SetInventory(receive, expiryDate1, packingDate, "", "", "att3", "", locationPK, part1.PK, palletID: "PALLET6", qty: 1m);
			SetInventory(receive, expiryDate2, packingDate, "", "", "", "", locationPK, part1.PK, palletID: "PALLET7", qty: 1m);
			SetInventory(receive, expiryDate2, packingDate, "", "", "", "", locationPK, part1.PK, palletID: "PALLET8", qty: 2m);
			SetInventory(receive, expiryDate3, packingDate, "", "", "", "", locationPK, part1.PK, palletID: "PALLET9", qty: 1m);
			SetInventory(receive, expiryDate3, packingDate, "", "", "", "", locationPK, part1.PK, palletID: "PALLET10", qty: 1m);
			SetInventory(receive, expiryDate1, packingDate, "", "", "", "", locationPK, part2.PK, palletID: "PALLET11", qty: 1m);
			SetInventory(receive, expiryDate1, packingDate, "", "", "", "", locationPK, part2.PK, palletID: "PALLET12", qty: 1m);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// hack to set serial number
			inventory.WI_SerialNumber = "SN1";

			var orderLine1 = Helper.CreateWhsOrderLine(order, part1, 3m);
			orderLine1.WE_ExpiryDate = expiryDate2;
			var orderLine2 = Helper.CreateWhsOrderLine(order, part1, 2m);
			orderLine2.WE_ExpiryDate = expiryDate3;
			orderLine2.WE_PackingDate = packingDate;
			var orderLine3 = Helper.CreateWhsOrderLine(order, part1, 3m);
			orderLine3.WE_PartAttrib1 = "att1";
			var orderLine4 = Helper.CreateWhsOrderLine(order, part1, 2m);
			orderLine4.WE_PartAttrib2 = "att2";
			var orderLine5 = Helper.CreateWhsOrderLine(order, part2, 2m);
			var orderLine6 = Helper.CreateWhsOrderLine(order, part1, 2m);
			orderLine6.WE_PartAttrib3 = "att3";
			var orderLine7 = Helper.CreateWhsOrderLine(order, part1, 1m);
			orderLine7.WE_SerialNumber = "SN1";

			Helper.CreatePickNew(order);
			Factory.Save();
			order.WD_DocketID = order.WD_ExternalReference;
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { });
			var result = linesPicker.FindAndAssignNextPick(order.WD_ExternalReference);
			AssertEquals("All picklines should be retained.", 13, result.PickLines.Distinct().Count());
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(0), "PART1", 1m, "", "", "", "", expiryDate: expiryDate2);
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(1), "PART1", 1m, "", "", "", "", expiryDate: expiryDate3, packingDate: packingDate);
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(2), "PART1", 1m, "", "", "", "", expiryDate: expiryDate3, packingDate: packingDate);
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(3), "PART1", 1m, "", "", "", "SN1");
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(4), "PART1", 1m, "", "", "att3", "");
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(5), "PART1", 1m, "", "", "att3", "");
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(6), "PART1", 1m, "", "att2", "", "");
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(7), "PART1", 1m, "", "att2", "", "");
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(8), "PART1", 1m, "att1", "", "", "");
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(9), "PART1", 2m, "", "", "", "", expiryDate: expiryDate2);
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(10), "PART1", 2m, "att1", "", "", "");
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(11), "PART2", 1m, "", "", "", "");
			AssertSortedPickLinesMatchOrder(result.PickLines.ElementAt(12), "PART2", 1m, "", "", "", "");
		}

		void AssertSortedPickLinesMatchOrder(WhsPickLine pickLine, ZString productCode, ZDecimal quantity, ZString attribute1, ZString attribute2, ZString attribute3, ZString serialNumber, ZDate expiryDate = default, ZDate packingDate = default)
		{
			var orderLine = pickLine.DocketLine;
			AssertEquals("ProductCode should be correct.", productCode, orderLine.ProductCode);
			AssertEquals("Quantity should be correct.", quantity, pickLine.WZ_Units);
			AssertEquals("Attribute1 should be correct.", attribute1, orderLine.WE_PartAttrib1);
			AssertEquals("Attribute2 should be correct.", attribute2, orderLine.WE_PartAttrib2);
			AssertEquals("Attribute3 should be correct.", attribute3, orderLine.WE_PartAttrib3);
			AssertEquals("SerialNumber should be correct.", serialNumber, orderLine.WE_SerialNumber);
			AssertEquals("Expiry Date should be correct.", expiryDate, orderLine.WE_ExpiryDate);
			AssertEquals("Packing Date should be correct.", packingDate, orderLine.WE_PackingDate);
		}

		#endregion

		#region TestGetPickWithANYEquipment

		public void TestGetPickWithANYEquipment()
		{
			var operator1 = Factory.NewWithValidTestData<GlbStaff>();
			var whs1 = Helper.CreateWarehouse("Whs1");
			var area = Helper.CreateArea(whs1, "A1", Warehouse.Environment.CodeLists.AreaTypes.Codes.FreeStore);
			Helper.CreateRowAndGenerateLocations(whs1, "Row1", 1, 1);
			Helper.Factory.Save();

			var location = whs1.FindLocation("Row1");
			location.WLV_WA_PickingArea = whs1.Areas[1].PK;
			var client = Helper.CreateClient("Cient");
			var part1 = Helper.CreateProduct(client, "Part1");

			location.WLV_PickMethod = "ANY";

			var receive1 = Helper.CreateWhsReceive(client, whs1, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, part1, 100m);
			receive1.Inventory[0].WI_WL = location.PK;
			receive1.FinaliseDocket();
			AssertEquals(true, receive1.IsFinalised);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(client, whs1, "Order1", Notify);
			Helper.CreateWhsOrderLine(order1, part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 5;
			pick1.WP_PickNo = "111";
			Factory.Save();

			var linesPicker1 = new PickLinesLoader(Factory, whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "FRK" });
			AssertNotNull(linesPicker1.FindAndAssignNextPick("111").Pick);

			var linesPicker2 = new PickLinesLoader(Factory, whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ABC" });
			AssertNotNull(linesPicker2.FindAndAssignNextPick("111").Pick);

			var linesPicker3 = new PickLinesLoader(Factory, whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			AssertNotNull(linesPicker3.FindAndAssignNextPick("111").Pick);
		}

		#endregion

		#region TestEnteredPickMethodGetsPriorityOverANYPickMethod

		public void TestEnteredPickMethodGetsPriorityOverANYPickMethod()
		{
			var operator1 = Factory.NewWithValidTestData<GlbStaff>();
			var operator2 = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			locationA1.WLV_PickMethod = "ANY";

			var locationA2 = data.Whs1.FindLocation("A-2");
			locationA2.WLV_PickMethod = "FRK";

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 100m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 100m, locationA2);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part2, 10m);
			var pick2 = Helper.CreatePickNew(order2);
			Factory.Save();

			var linesPicker1 = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "FRK" });
			var result1 = linesPicker1.FindAndAssignNextPick(ZString.Empty);
			AssertNotNull(result1.Pick);
			AssertAssignedPickAndLines(result1, pick2, locationA2);

			var linesPicker2 = new PickLinesLoader(Factory, data.Whs1, operator2, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "FRK" });
			var result2 = linesPicker2.FindAndAssignNextPick(ZString.Empty);
			AssertNotNull(result2.Pick);
			AssertAssignedPickAndLines(result2, pick1, locationA1);
		}

		#endregion

		#region TestGetPickWithPickPriority

		public void TestGetPickWithPickPriority()
		{
			var operator1 = Factory.NewWithValidTestData<GlbStaff>();
			var operator2 = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 100m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 100m, locationA2);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 10m);
			order1.WD_PickPriority = 0;
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part2, 10m);
			order2.WD_PickPriority = 1;
			var pick2 = Helper.CreatePickNew(order2);

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order3", data.Part2, 10m);
			order3.WD_PickPriority = 2;
			var pick3 = Helper.CreatePickNew(order3);

			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.FindAndAssignNextPick(ZString.Empty);
			AssertEquals(result.Pick.PK, pick2.PK);
		}

		#endregion

		#region TestGetPickWithPickPriority_SortsByPickNoLast

		public void TestGetPickWithPickPriority_SortsByPickNoLast()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			locationA1.WLV_PickMethod = "FRK";
			locationA2.WLV_PickMethod = "FRK";

			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m, locationA2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 10m);
			order1.WD_PickPriority = 1;
			var pick1 = Helper.CreatePickNew(order1);
			order1.Lines[0].PickLines.Single().WZ_GS_NKAssignedTo = staff.GS_Code;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part2, 10m);
			order2.WD_PickPriority = 1;
			var pick2 = Helper.CreatePickNew(order2);
			order2.Lines[0].PickLines.Single().WZ_GS_NKAssignedTo = staff.GS_Code;

			Factory.Save();

			var linesPicker1 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "FRK" });
			var result1 = linesPicker1.FindAndAssignNextPick(ZString.Empty);
			AssertEquals("Should return First Pick as the Pick's assigned staff, Priority and Pick Method are the same and so it should grab the earliest pick.", result1.Pick.PK, pick1.PK);

			locationA1.WLV_PickMethod = "ANY";
			Factory.Save();
			var linesPicker2 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "FRK" });
			var result2 = linesPicker2.FindAndAssignNextPick(ZString.Empty);
			AssertEquals("Should return Second Pick as the Pick's assigned staff & Priority are the same and so and it should grab the pick with the same registered equipment.", result2.Pick.PK, pick2.PK);
			locationA1.WLV_PickMethod = "FRK"; // cleanup

			order1.WD_PickPriority = 2;
			Factory.Save();
			var linesPicker3 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "FRK" });
			var result3 = linesPicker3.FindAndAssignNextPick(ZString.Empty);
			AssertEquals("Should return Second Pick as the Pick's assigned staff are the same and so it should grab the pick with higher priority.", result3.Pick.PK, pick2.PK);

			var pickline = order2.Lines[0].PickLines.Single();
			pickline.WZ_GS_NKAssignedTo = "";
			pickline.WZ_IsPicking = false;
			Factory.Save();
			var linesPicker4 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "FRK" });
			var result4 = linesPicker4.FindAndAssignNextPick(ZString.Empty);
			AssertEquals("Should return first Pick as it's the only Pick assigned to the same staff.", result4.Pick.PK, pick1.PK);
		}

		#endregion

		#region TestGetPickWithPickPriority_AssignedToUserShouldNotEffectPicklineSorting

		public void TestGetPickWithPickPriority_AssignedToUserShouldNotEffectPicklineSorting()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, locationA2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Helper.CreateWhsOrderLine(order, data.Part1, 30m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = order.Lines.Single(l => l.WE_OP.Equals(data.Part1.PK) && l.WE_TransactionQuantity == 10m).PickLines.Single();
			var pickLine2 = order.Lines.Single(l => l.WE_OP.Equals(data.Part2.PK) && l.WE_TransactionQuantity == 20m).PickLines.Single();
			var pickLine3 = order.Lines.Single(l => l.WE_OP.Equals(data.Part1.PK) && l.WE_TransactionQuantity == 30m).PickLines.Single();
			pickLine1.WZ_GS_NKAssignedTo = staff.GS_Code; // Product 1 in location A1
			pickLine3.WZ_GS_NKAssignedTo = staff.GS_Code; // Product 1 in location A2

			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.FindAndAssignNextPick(ZString.Empty);
			var expectedPicklines = new List<ZGuid> { pickLine1.PK, pickLine2.PK, pickLine3.PK };
			AssertEquals("Assigned to the user should not prioritised over location sorting.", "pickLine1 pickLine2 pickLine3", string.Join(" ", result.PickLines.Select(pl => $"pickLine{(expectedPicklines.IndexOf(pl.PK) + 1)}")));

			foreach (var pl in pick.GetAllPickLines())
			{
				pl.WZ_IsPicking = false;
				pl.WZ_GS_NKAssignedTo = string.Empty;
			}
			Factory.Save();

			var linesPicker2 = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			var result2 = linesPicker2.FindAndAssignNextPick(ZString.Empty);
			AssertEquals("Assigned to the user should not prioritised over location sorting.", "pickLine1 pickLine2 pickLine3", string.Join(" ", result2.PickLines.Select(pl => $"pickLine{(expectedPicklines.IndexOf(pl.PK) + 1)}")));
		}

		#endregion

		#region TestGetPickWithPickPriority_AssignedPickLineDoesEffectOnSortingPick

		public void TestGetPickWithPickPriority_AssignedPickLineDoesEffectOnSortingPick()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part1, 10m);
			var pick2 = Helper.CreatePickNew(order2);

			var pickLine1 = pick1.GetAllPickLines().Single();
			var pickLine2 = pick2.GetAllPickLines().Single();
			pickLine1.WZ_GS_NKAssignedTo = staff.GS_Code;
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.FindAndAssignNextPick(ZString.Empty);
			AssertEquals("Assigned to the user should prioritised over pick.", pickLine1.PK, result.PickLines.Single().PK);

			pickLine1.WZ_IsPicking = false;
			pickLine1.WZ_GS_NKAssignedTo = string.Empty;
			pickLine2.WZ_IsPicking = false;
			pickLine2.WZ_GS_NKAssignedTo = staff.GS_Code;
			Factory.Save();

			var linesPicker2 = new PickLinesLoader(new BusinessObjectFactory(), data.Whs1, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			var result2 = linesPicker2.FindAndAssignNextPick(ZString.Empty);
			AssertEquals("Assigned to the user should prioritised over pick.", pickLine2.PK, result2.PickLines.Single().PK);
		}

		#endregion

		#region TestGetPickWithAndWithOutPicker_SamePickPriority

		public void TestGetPickWithAndWithOutPicker_SamePickPriority()
		{
			var operator1 = Factory.NewWithValidTestData<GlbStaff>();
			var operator2 = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 100m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 100m, locationA2);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part2, 10m);
			var pick2 = Helper.CreatePickNew(order2);
			SetupPickLine(pick2, 0, 0, "ANY", operator1, ZDateTimeOffset.Empty);

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order3", data.Part2, 10m);
			var pick3 = Helper.CreatePickNew(order3);

			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.FindAndAssignNextPick(ZString.Empty);
			AssertEquals(result.Pick.PK, pick2.PK);
		}

		#endregion

		#region TestGetPickWithPickMethodAndPickPriority

		public void TestGetPickWithPickMethodAndPickPriority()
		{
			var operator1 = Factory.NewWithValidTestData<GlbStaff>();
			var operator2 = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			locationA1.WLV_PickMethod = "ANY";

			var locationA2 = data.Whs1.FindLocation("A-2");
			locationA2.WLV_PickMethod = "FRK";
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 100m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 100m, locationA2);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part2, 10m);
			order1.WD_PickPriority = 0;
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part1, 10m);
			order2.WD_PickPriority = 1;
			var pick2 = Helper.CreatePickNew(order2);

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order3", data.Part2, 10m);
			order3.WD_PickPriority = 2;
			var pick3 = Helper.CreatePickNew(order3);

			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "FRK" });
			var result = linesPicker.FindAndAssignNextPick(ZString.Empty);
			AssertEquals(result.Pick.PK, pick2.PK);
		}

		#endregion

		#region TestGetPickWithPickPriorityAndPickMethodAndPicker

		public void TestGetPickWithPickPriorityAndPickMethodAndPicker()
		{
			SetupEnvironmentData();

			var operator1 = Factory.NewWithValidTestData<GlbStaff>();
			var operator2 = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			locationA1.WLV_PickMethod = "ANY";

			var locationA2 = data.Whs1.FindLocation("A-2");
			locationA2.WLV_PickMethod = "FRK";
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 100m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 100m, locationA2);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part2, 10m);
			order1.WD_PickPriority = 0;
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part1, 10m);
			order2.WD_PickPriority = 1;
			var pick2 = Helper.CreatePickNew(order2);

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order3", data.Part2, 10m);
			order3.WD_PickPriority = 2;
			var pick3 = Helper.CreatePickNew(order3);
			SetupPickLine(pick3, 0, 0, ZString.Empty, operator1, ZDateTimeOffset.Empty);

			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.FindAndAssignNextPick(ZString.Empty);
			AssertEquals(result.Pick.PK, pick3.PK);
		}

		#endregion

		#region TestFindAndAssignNextPickWithUOMType

		public void TestFindAndAssignNextPickWithUOMType()
		{
			var rfUser = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true; // to generate required picklines more easily
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "PLT", 100);
			Helper.CreateProductUnit(data.Part1, "BOX", 10);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);

			var refPackTypes = new RefPackTypeCollection(Factory);
			refPackTypes.Single(p => p.F3_Code == "PLT").F3_UOMType = UOMPackTypesList.Codes.Pallet;
			refPackTypes.Single(p => p.F3_Code == "BOX").F3_UOMType = "";
			refPackTypes.Single(p => p.F3_Code == "UNT").F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 326m);
			var pick1 = Helper.CreatePickNew(order1);
			var picklines1 = pick1.GetAllPickLines();
			AssertEquals(3, pick1.GetAllPickLines().Count());
			var pick1LinePLT = picklines1.Single(pl => pl.WZ_Units == 300 && pl.WZ_F3_NKAllocatedPackType == "PLT");
			var pick1LineBOX = picklines1.Single(pl => pl.WZ_Units == 20 && pl.WZ_F3_NKAllocatedPackType == "BOX");
			var pick1LineUNT = picklines1.Single(pl => pl.WZ_Units == 6 && pl.WZ_F3_NKAllocatedPackType == "UNT");

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 42m);
			var pick2 = Helper.CreatePickNew(order2);
			var picklines2 = pick2.GetAllPickLines();
			AssertEquals(2, picklines2.Count());
			var pick2LineBOX = picklines2.Single(pl => pl.WZ_Units == 40 && pl.WZ_F3_NKAllocatedPackType == "BOX");
			var pick2LineUNT = picklines2.Single(pl => pl.WZ_Units == 2 && pl.WZ_F3_NKAllocatedPackType == "UNT");

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 8m);
			var pick3 = Helper.CreatePickNew(order3);
			var picklines3 = pick3.GetAllPickLines();
			AssertEquals(1, picklines3.Count());
			var pick3LineUNT = picklines3.Single(pl => pl.WZ_Units == 8 && pl.WZ_F3_NKAllocatedPackType == "UNT");

			Factory.Save();

			var linesPickerForPLT = new PickLinesLoader(Factory, data.Whs1, rfUser, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0, UOMType = UOMPackTypesList.Codes.Pallet });
			var pickResult1 = linesPickerForPLT.FindAndAssignNextPick("O1");
			AssertNotNull(pickResult1.Pick);
			AssertContainsExactElementsInAnyOrder("pick1LineUNT should not be picked as UNT pack type assigned to Split-Case UOM Type", new[] { pick1LinePLT, pick1LineBOX }, pickResult1.PickLines);

			var pickResult2 = linesPickerForPLT.FindAndAssignNextPick("O2");
			AssertNotNull(pickResult2.Pick);
			AssertContainsExactElementsInAnyOrder("pick2LineUNT should not be picked as UNT pack type assigned to Split-Case UOM Type", new[] { pick2LineBOX }, pickResult2.PickLines);

			var pickResult3 = linesPickerForPLT.FindAndAssignNextPick("O3");
			AssertNull("pick should not be found as it contains only pickLines for UNT which is linked to Split-Case UOM Type", pickResult3.Pick);

			var linesPickerForAnyUOMType = new PickLinesLoader(Factory, data.Whs1, rfUser, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			pickResult1 = linesPickerForAnyUOMType.FindAndAssignNextPick("O1");
			AssertNotNull(pickResult1.Pick);
			AssertContainsExactElementsInAnyOrder(new[] { pick1LinePLT, pick1LineBOX, pick1LineUNT }, pickResult1.PickLines);

			pickResult2 = linesPickerForAnyUOMType.FindAndAssignNextPick("O2");
			AssertNotNull(pickResult2.Pick);
			AssertContainsExactElementsInAnyOrder(new[] { pick2LineBOX, pick2LineUNT }, pickResult2.PickLines);

			pickResult3 = linesPickerForAnyUOMType.FindAndAssignNextPick("O3");
			AssertNotNull(pickResult3.Pick);
			AssertContainsExactElementsInAnyOrder(new[] { pick3LineUNT }, pickResult3.PickLines);
		}

		#endregion

		#region TestFiltersPickLinesAssignedToTote

		public void TestFiltersPickLinesAssignedToTote()
		{
			var packingHelper = new PackingTestHelper(Factory);

			var operator1 = Factory.NewWithValidTestData<GlbStaff>();
			var operator2 = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			locationA1.WLV_PickMethod = "ANY";

			var locationA2 = data.Whs1.FindLocation("A-2");
			locationA2.WLV_PickMethod = "FRK";
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 100m, locationA1);

			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 10m);
			order1.WD_PickPriority = 1;
			var pick1 = Helper.CreatePickNew(order1);
			SetupPickLine(pick1, 0, 0, ZString.Empty, operator1, ZDateTimeOffset.Empty);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part1, 10m);
			order2.WD_PickPriority = 2;
			var pick2 = Helper.CreatePickNew(order2);
			SetupPickLine(pick2, 0, 0, ZString.Empty, operator1, ZDateTimeOffset.Empty);

			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.FindAndAssignNextPick(ZString.Empty);
			AssertEquals("Precondition: ", result.Pick.PK, pick1.PK);

			Factory.Save();

			result = linesPicker.FindAndAssignNextPick(ZString.Empty);
			AssertEquals("Ensure that it still finds pick1: ", result.Pick.PK, pick1.PK);

			// assign order1's pickline to tote
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package = packingHelper.CreatePackage(pkgJob, "AAA", 1, Constants.PkgUnit.Tote);
			package.SetIsTote(true);
			packingHelper.CreatePackageDivot(package, order1.Lines.Single().PickLines.Single());

			Factory.Save();

			result = linesPicker.FindAndAssignNextPick(ZString.Empty);
			AssertEquals("The one assigned to tote is now invalid ", result.Pick.PK, pick2.PK);
		}

		#endregion

		#region TestGetPickForDifferentClients

		public void TestGetPickForDifferentClients()
		{
			var operator1 = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_Code = "O1";
			var org2 = Helper.CreateClient("O2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var orderForOrg1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 1m);
			var pickForOrg1 = Helper.CreatePickNew(orderForOrg1);
			pickForOrg1.WP_PickNo = "1";
			var orderForOrg2 = Helper.CreateWhsOrderWithOrderLine(org2, data.Whs1, "Order2", data.Part1, 1m);
			var pickForOrg2 = Helper.CreatePickNew(orderForOrg2);
			pickForOrg2.WP_PickNo = "2";

			Factory.Save();

			var linesPickerForNoClient = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", ClientCode = "" });
			AssertEquals(linesPickerForNoClient.FindAndAssignNextPick(ZString.Empty).Pick.PK, pickForOrg1.PK);

			var linesPickerForOrg1 = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", ClientCode = "O1" });
			AssertEquals(linesPickerForOrg1.FindAndAssignNextPick(ZString.Empty).Pick.PK, pickForOrg1.PK);

			var linesPickerForOrg2 = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", ClientCode = "O2" });
			AssertEquals(linesPickerForOrg2.FindAndAssignNextPick(ZString.Empty).Pick.PK, pickForOrg2.PK);

			var linesPickerForNonExistingOrg = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", ClientCode = "O3" });
			AssertNull(linesPickerForNonExistingOrg.FindAndAssignNextPick(ZString.Empty).Pick);
		}

		public void TestGetPickForMultiOrderPicksWithDifferentClients()
		{
			var operator1 = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_Code = "O1";
			var org2 = Helper.CreateClient("O2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var orderForOrg1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 1m);
			var orderForOrg2 = Helper.CreateWhsOrderWithOrderLine(org2, data.Whs1, "Order2", data.Part1, 1m);
			var pick = Helper.CreatePickNew(orderForOrg1, orderForOrg2);
			Factory.Save();

			var linesPickerWithNoClient = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", ClientCode = "" });
			AssertEquals(linesPickerWithNoClient.FindAndAssignNextPick(ZString.Empty).Pick.PK, pick.PK);
			var pickLinesWhenNoClientCode = linesPickerWithNoClient.FindAndAssignNextPick(ZString.Empty).PickLines;
			AssertEquals(2, pickLinesWhenNoClientCode.Count());
			pickLinesWhenNoClientCode.Single(l => l.Inventory.WI_OH_Client == data.Org1.PK);
			pickLinesWhenNoClientCode.Single(l => l.Inventory.WI_OH_Client == org2.PK);

			var linesPickerForOrg1 = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", ClientCode = "O1" });
			AssertEquals(linesPickerForOrg1.FindAndAssignNextPick(ZString.Empty).Pick.PK, pick.PK);
			var pickLinesForOrg1 = linesPickerForOrg1.FindAndAssignNextPick(ZString.Empty).PickLines;
			AssertEquals(data.Org1.PK, pickLinesForOrg1.Single().Inventory.WI_OH_Client);

			var linesPickerForOrg2 = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", ClientCode = "O2" });
			AssertEquals(linesPickerForOrg2.FindAndAssignNextPick(ZString.Empty).Pick.PK, pick.PK);
			var pickLinesForOrg2 = linesPickerForOrg2.FindAndAssignNextPick(ZString.Empty).PickLines;
			AssertEquals(org2.PK, pickLinesForOrg2.Single().Inventory.WI_OH_Client);

			var linesPickerForNonExistingOrg = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", ClientCode = "O3" });
			AssertNull(linesPickerForNonExistingOrg.FindAndAssignNextPick(ZString.Empty).Pick);
		}

		#endregion

		#region TestFindAndAssignNextPick_ForCrossDockOrders

		public void TestFindAndAssignNextPick_ForCrossDockOrders()
		{
			var operator1 = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, false, false);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m, true, false);
			var finalisedReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 20m, true, true);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var inventory1 = (WhsInventoryView)receive1.Inventory.Single();
			var inventory2 = (WhsInventoryView)receive2.Inventory.Single();
			Helper.CreateReservePickLine(orderLine1, inventory1, 5m);
			Helper.CreateReservePickLine(orderLine1, inventory2, 1m);
			AssertEquals(5m, inventory1.WI_CrossDockQuantity);
			AssertEquals(1m, inventory2.WI_CrossDockQuantity);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var loader = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			AssertEquals(pick, loader.FindAndAssignNextPick(ZString.Empty).Pick);
			AssertEquals(finalisedReceive.Inventory.Single(), loader.FindAndAssignNextPick(ZString.Empty).PickLines.Single().Inventory);
		}

		#endregion

		#region TestFindAndAssignNextPick_WhenAllocatedPackTypeHasNoMatchingUOM

		public void TestFindAndAssignNextPick_WhenAllocatedPackTypeHasNoMatchingUOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var cartonUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			PackingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 80m);
			Helper.CreatePickNew(order);

			var pickLineCarton = orderLine.PickLines.Single();
			AssertEquals("Precondition: Correct amount allocated for Carton.", 80m, pickLineCarton.WZ_Units);
			AssertEquals("Precondition: Only Cartons are allocated.", Constants.PkgUnit.Carton, pickLineCarton.WZ_F3_NKAllocatedPackType);

			cartonUnit.Delete(); // simulate a missing Carton Unit
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate all Cartons.", 80m, PickLineUnitSum(result, picker));
		}

		#endregion

		#region TestFindAndAssignNextPick_EquipmentCapacity

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType()
		{
			var forkLift = Helper.CreateEquipment("testy", 0, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 0, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Carton, Constants.PkgUnit.Skid, 4m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Skid, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 83m);
			Helper.CreatePickNew(order);

			var pickLineSkid = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Skid);
			var pickLineCarton = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton);
			var pickLineUnit = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit);
			AssertEquals("Precondition: Correct amount allocated for Skid.", 64m, pickLineSkid.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Carton.", 16m, pickLineCarton.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Unit.", 3m, pickLineUnit.WZ_Units);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate all Cartons.", 16m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_WeightLimitProductHasNoWeight()
		{
			var forkLift = Helper.CreateEquipment("testy", 1, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 0, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Carton, Constants.PkgUnit.Skid, 4m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Skid, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 83m);
			Helper.CreatePickNew(order);

			var pickLineSkid = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Skid);
			var pickLineCarton = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton);
			var pickLineUnit = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit);
			AssertEquals("Precondition: Correct amount allocated for Skid.", 64m, pickLineSkid.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Carton.", 16m, pickLineCarton.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Unit.", 3m, pickLineUnit.WZ_Units);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate all Cartons.", 16m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_DoesNotSplitUOM_WeightLimit()
		{
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 0, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0.5m, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			var cartonUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			cartonUnit.OF_Weight = 8m;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Carton, Constants.PkgUnit.Skid, 4m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Skid, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 83m);
			Helper.CreatePickNew(order);

			var pickLineSkid = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Skid);
			var pickLineCarton = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton);
			var pickLineUnit = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit);
			AssertEquals("Precondition: Correct amount allocated for Skid.", 64m, pickLineSkid.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Carton.", 16m, pickLineCarton.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Unit.", 3m, pickLineUnit.WZ_Units);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate 1 Carton.", 8m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_UOMExceedsWeightLimit()
		{
			var forkLift = Helper.CreateEquipment("testy", 7, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 0, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0.5m, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			var cartonUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			cartonUnit.OF_Weight = 8m;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Carton, Constants.PkgUnit.Skid, 4m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Skid, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 83m);
			Helper.CreatePickNew(order);

			var pickLineSkid = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Skid);
			var pickLineCarton = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton);
			var pickLineUnit = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit);
			AssertEquals("Precondition: Correct amount allocated for Skid.", 64m, pickLineSkid.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Carton.", 16m, pickLineCarton.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Unit.", 3m, pickLineUnit.WZ_Units);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertNull("Should not have been able to assign any Pick Lines.", result.Pick);
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_VolumeLimitProductHasNoVolume()
		{
			var forkLift = Helper.CreateEquipment("testy", 0, Constants.Weight.Kilograms, 1, Constants.Volume.Litre, 0, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Carton, Constants.PkgUnit.Skid, 4m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Skid, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 83m);
			Helper.CreatePickNew(order);

			var pickLineSkid = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Skid);
			var pickLineCarton = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton);
			var pickLineUnit = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit);
			AssertEquals("Precondition: Correct amount allocated for Skid.", 64m, pickLineSkid.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Carton.", 16m, pickLineCarton.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Unit.", 3m, pickLineUnit.WZ_Units);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate all Cartons.", 16m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_DoesNotSplitUOM_VolumeLimit()
		{
			var forkLift = Helper.CreateEquipment("testy", 0, Constants.Weight.Kilograms, 10, Constants.Volume.Litre, 0, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0, Constants.Weight.Kilograms, 0.5m, Constants.Volume.Litre);
			var cartonUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			cartonUnit.OF_Cubic = 8m;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Carton, Constants.PkgUnit.Skid, 4m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Skid, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 83m);
			Helper.CreatePickNew(order);

			var pickLineSkid = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Skid);
			var pickLineCarton = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton);
			var pickLineUnit = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit);
			AssertEquals("Precondition: Correct amount allocated for Skid.", 64m, pickLineSkid.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Carton.", 16m, pickLineCarton.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Unit.", 3m, pickLineUnit.WZ_Units);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate 1 Carton.", 8m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_UOMExceedsVolumeLimit()
		{
			var forkLift = Helper.CreateEquipment("testy", 0, Constants.Weight.Kilograms, 7, Constants.Volume.Litre, 0, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0, Constants.Weight.Kilograms, 0.5m, Constants.Volume.Litre);
			var cartonUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			cartonUnit.OF_Cubic = 8m;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Carton, Constants.PkgUnit.Skid, 4m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Skid, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 83m);
			Helper.CreatePickNew(order);

			var pickLineSkid = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Skid);
			var pickLineCarton = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton);
			var pickLineUnit = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit);
			AssertEquals("Precondition: Correct amount allocated for Skid.", 64m, pickLineSkid.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Carton.", 16m, pickLineCarton.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Unit.", 3m, pickLineUnit.WZ_Units);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertNull("Should not have been able to assign any Pick Lines.", result.Pick);
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_PackCountLimit()
		{
			var forkLift = Helper.CreateEquipment("testy", 0, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 2, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Carton, Constants.PkgUnit.Skid, 4m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Skid, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 56m);
			Helper.CreatePickNew(order);

			var pickLineSkid = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Skid);
			var pickLineCarton = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton);
			AssertEquals("Precondition: Correct amount allocated for Skid.", 32m, pickLineSkid.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Carton.", 24m, pickLineCarton.WZ_Units);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate 2 Cartons.", 16m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_PackCountLimit_MultiplePickLines()
		{
			var year = ZDateTime.Now.Year;
			var forkLift = Helper.CreateEquipment("testy", 0, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 2, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 8m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 8m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", new ZDateTimeOffset(year, 1, 1), data.Part1, 8m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 24m);
			Helper.CreatePickNew(order);

			AssertEquals("Precondition: Correct amount allocated for Carton.", 24m, orderLine.PickLines.Where(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton).Sum(pl => pl.WZ_Units));

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate 2 Cartons.", 16m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_DoesNotSplitUOM_MixedLimit()
		{
			var forkLift = Helper.CreateEquipment("testy", 16, Constants.Weight.Kilograms, 10, Constants.Volume.Litre, 0, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0.5m, Constants.Weight.Kilograms, 0.5m, Constants.Volume.Litre);
			var cartonUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			cartonUnit.OF_Cubic = 8m;
			cartonUnit.OF_Weight = 8m;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Carton, Constants.PkgUnit.Skid, 4m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Skid, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 83m);
			Helper.CreatePickNew(order);

			var pickLineSkid = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Skid);
			var pickLineCarton = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton);
			var pickLineUnit = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit);
			AssertEquals("Precondition: Correct amount allocated for Skid.", 64m, pickLineSkid.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Carton.", 16m, pickLineCarton.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Unit.", 3m, pickLineUnit.WZ_Units);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate 1 Carton.", 8m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_WhenUsingSplitCasePackUnit()
		{
			var forkLift = Helper.CreateEquipment("testy", 0.3m, Constants.Weight.Kilograms, 1500, Constants.Volume.CubicCentimeters, 2, Constants.PkgUnit.Bundle);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0.025m, Constants.Weight.Kilograms, 125m, Constants.Volume.CubicCentimeters);
			var bundleUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Bundle, 6m);
			bundleUnit.OF_Weight = 0.2m;
			bundleUnit.OF_Cubic = 800m;
			PackingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Bundle, UOMPackTypesList.Codes.SplitCase);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 60m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 36m);
			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition: Correct amount allocated for Bundle.", 36m, pickLine.WZ_Units);
			AssertEquals("Precondition: Only bundles Allocated.", Constants.PkgUnit.Bundle, pickLine.WZ_F3_NKAllocatedPackType);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate only one Bundle.", 6m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Bundles.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Bundle));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_AllConstraintsInUse()
		{
			var forkLift = Helper.CreateEquipment("testy", 16, Constants.Weight.Kilograms, 18, Constants.Volume.Litre, 1, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0.5m, Constants.Weight.Kilograms, 0.5m, Constants.Volume.Litre);
			var cartonUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			cartonUnit.OF_Cubic = 8m;
			cartonUnit.OF_Weight = 8m;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Carton, Constants.PkgUnit.Skid, 4m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Skid, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 83m);
			Helper.CreatePickNew(order);

			var pickLineSkid = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Skid);
			var pickLineCarton = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton);
			var pickLineUnit = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit);
			AssertEquals("Precondition: Correct amount allocated for Skid.", 64m, pickLineSkid.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Carton.", 16m, pickLineCarton.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Unit.", 3m, pickLineUnit.WZ_Units);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate 1 Carton.", 8m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_AllConstraintsInUseWithStockKeepingUnit()
		{
			var forkLift = Helper.CreateEquipment("testy", 0.09m, Constants.Weight.Kilograms, 400, Constants.Volume.CubicCentimeters, 6, Constants.PkgUnit.Unit);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0.025m, Constants.Weight.Kilograms, 125m, Constants.Volume.CubicCentimeters);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition: Correct amount allocated for Bundle.", 4m, pickLine.WZ_Units);
			AssertEquals("Precondition: Only bundles Allocated.", Constants.PkgUnit.Unit, pickLine.WZ_F3_NKAllocatedPackType);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate only three Units.", 3m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Units.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_AllConstraintsInUseWithStockKeepingUnit_WhenPackUnitIsLimit()
		{
			var forkLift = Helper.CreateEquipment("testy", 20m, Constants.Weight.Kilograms, 100000, Constants.Volume.CubicCentimeters, 3, Constants.PkgUnit.Unit);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0.025m, Constants.Weight.Kilograms, 125m, Constants.Volume.CubicCentimeters);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition: Correct amount allocated for Bundle.", 4m, pickLine.WZ_Units);
			AssertEquals("Precondition: Only bundles Allocated.", Constants.PkgUnit.Unit, pickLine.WZ_F3_NKAllocatedPackType);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate only three Units.", 3m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Units.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit));
		}

		/// <summary>
		/// Equipment has a limit of 16KG and unlimited Cartons
		/// 
		/// A Carton of Part1 is made up of 8 Stock Units, Weighs 8KG and has no Volume
		/// 
		/// Should Assign only two Cartons of Part 1, since the weight limit is reached. This will result in one of the Pick Lines being split
		/// 
		/// </summary>
		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_DoesNotSplitUOM_SplitsPickLine()
		{
			var year = ZDateTime.Now.Year;
			var forkLift = Helper.CreateEquipment("testy", 16, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 0, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0.5m, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			var cartonUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			cartonUnit.OF_Weight = 8m;
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 11m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 13m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 24m);
			Helper.CreatePickNew(order);
			AssertEquals("Precondition: Only allocated Cartons.", true, orderLine.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
			AssertEquals("Precondition: Correct amount allocated for Carton.", 24m, orderLine.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition: 2 Picklines Allocated.", 2, orderLine.PickLines.Count);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate 2 Cartons.", 16m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
			AssertEquals("1 PickLine should have been Split.", 3, orderLine.PickLines.Count);
			AssertEquals("Only allocated Cartons.", true, orderLine.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
			AssertEquals("Correct amount still allocated to Order.", 24m, orderLine.PickLines.Sum(pl => pl.WZ_Units));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_PackExceedsWeightLimitOverPackCountLimit()
		{
			var year = ZDateTime.Now.Year - 1;
			var forkLift = Helper.CreateEquipment("testy", 40, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 10, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			var cartonUnit1 = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			var cartonUnit2 = Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			cartonUnit1.OF_Weight = 8m;
			cartonUnit2.OF_Weight = 16m;
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 24m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part2, 16m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 24m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 16m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Only allocated Cartons.", true, pick.GetAllPickLines().All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
			AssertEquals("Precondition: Correct amount allocated for Carton.", 24m, orderLine1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition: Correct amount allocated for Carton.", 16m, orderLine2.PickLines.Sum(pl => pl.WZ_Units));

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate 4 Cartons.", 32m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
			AssertEquals("Correct amount of Part 1 assigned.", 24m, result.PickLines.Where(pl => pl.SupplierPart == data.Part1).Sum(pl => pl.WZ_Units));
			AssertEquals("Correct amount of Part 2 assigned.", 8m, result.PickLines.Where(pl => pl.SupplierPart == data.Part2).Sum(pl => pl.WZ_Units));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_DoesNotSplitUOM_SplitsPickLineWeightLimitHigh()
		{
			var year = ZDateTime.Now.Year;
			var forkLift = Helper.CreateEquipment("testy", 15, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 0, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0.5m, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			var cartonUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			cartonUnit.OF_Weight = 8m;
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 16m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 16m);
			Helper.CreatePickNew(order);
			AssertEquals("Precondition: Only allocated Cartons.", true, orderLine.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
			AssertEquals("Precondition: Correct amount allocated for Carton.", 16m, orderLine.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition: 1 Pickline Allocated.", 1, orderLine.PickLines.Count);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate 1 Carton.", 8m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
			AssertEquals("1 PickLine should have been Split.", 2, orderLine.PickLines.Count);
			AssertEquals("Only allocated Cartons.", true, orderLine.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
			AssertEquals("Correct amount still allocated to Order.", 16m, orderLine.PickLines.Sum(pl => pl.WZ_Units));
		}

		/// <summary>
		/// Equipment has a limit of 7KG and unlimited Cartons
		/// 
		/// A Carton of Part1 is made up of 8 Stock Units, Weighs 8KG and has no Volume
		/// A Carton of Part2 is made up of 6 Stock Units, Weighs 6KG and has no Volume
		/// 
		/// Pick 1 should be skipped since each carton weighs 8KG which is over the 7KG limit
		/// Pick 2 should have one carton assigned, since each carton weighs 6KG which is under the 7KG limit
		/// 
		/// </summary>
		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_CannotPickSingleUOMOnFirstPick()
		{
			var forkLift = Helper.CreateEquipment("testy", 7, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 0, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0.5m, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Helper.SetProductWeightAndVolume(data.Part2, 0.5m, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			var cartonUnit1 = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 8m);
			cartonUnit1.OF_Weight = 8m;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Carton, Constants.PkgUnit.Skid, 4m);
			var cartonUnit2 = Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 6m);
			cartonUnit2.OF_Weight = 6m;
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Carton, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Skid, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order1.WD_PickPriority = 1;
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 83m);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			order2.WD_PickPriority = 2;
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 16m);
			var pick2 = Helper.CreatePickNew(order2);

			var pickLineSkid = orderLine1.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Skid);
			var pickLineCarton1 = orderLine1.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton);
			var pickLineUnit1 = orderLine1.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit);
			var pickLineCarton2 = orderLine2.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton);
			var pickLineUnit2 = orderLine2.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit);
			AssertEquals("Precondition: Correct amount allocated for Skid.", 64m, pickLineSkid.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Carton.", 16m, pickLineCarton1.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Unit.", 3m, pickLineUnit1.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Carton.", 12m, pickLineCarton2.WZ_Units);
			AssertEquals("Precondition: Correct amount allocated for Unit.", 4m, pickLineUnit2.WZ_Units);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Should have returned second Pick.", pick2, result.Pick);
			AssertEquals("Expected to Allocate 1 Carton.", 6m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_WithPallets()
		{
			// Strange use case, but this ensures the full pallet must be allowed to be Picked
			var forkLift = Helper.CreateEquipment("testy", 24, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 0, Constants.PkgUnit.Pallet);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetProductWeightAndVolume(data.Part1, 0.5m, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			var palletUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 24m);
			palletUnit.OF_Weight = 24m;
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 48m, data.Whs1.DefaultLocation, "PLT-1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 48m);
			Helper.CreatePickNew(order);

			var pickLinePallet = orderLine.PickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Pallet);
			AssertEquals("Precondition: Correct amount allocated for Pallet.", 48m, pickLinePallet.WZ_Units);

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertNull("Should not get the Pick as, even though he can pick a UOM pallet, the Pallet ID cannot be split.", result.Pick);
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_WithPalletNumberConstraint()
		{
			var forkLift = Helper.CreateEquipment("testy", 0, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 2, Constants.PkgUnit.Pallet);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 24m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 24m, data.Whs1.DefaultLocation, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 24m, data.Whs1.DefaultLocation, "PLT-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 24m, data.Whs1.DefaultLocation, "PLT-3");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 72m);
			Helper.CreatePickNew(order);

			var palletPickLines = orderLine.PickLines.Where(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Pallet);
			AssertEquals("Precondition: Correct amount allocated for Pallet.", 72m, palletPickLines.Sum(pl => pl.WZ_Units));

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate 2 Pallets.", 48m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_WithPalletNumberConstraintAndWeightConstraint()
		{
			var forkLift = Helper.CreateEquipment("testy", 48, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 2, Constants.PkgUnit.Pallet);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			var palletUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 24m);
			palletUnit.OF_Weight = 24m;
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 24m, data.Whs1.DefaultLocation, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 24m, data.Whs1.DefaultLocation, "PLT-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 24m, data.Whs1.DefaultLocation, "PLT-3");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 72m);
			Helper.CreatePickNew(order);

			var palletPickLines = orderLine.PickLines.Where(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Pallet);
			AssertEquals("Precondition: Correct amount allocated for Pallet.", 72m, palletPickLines.Sum(pl => pl.WZ_Units));

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate 2 Pallets.", 48m, PickLineUnitSum(result, picker));
		}

		/// <summary>
		/// Equipment has a limit of 48KG and 3 Pallets
		/// 
		/// A Pallet of Part1 is made up of 24 Stock Units, Weighs 24KG and has no Volume
		/// A Pallet of Part2 is made up of 24 Stock Units, Weighs nothing and has no Volume
		/// 
		/// Should Assign only two Pallets of Part 1, since the weight limit is reached
		/// Should Assign one Pallet of Part 2, since Part 2 has no weight and volume. Only one as there is a limit 3 Pallets
		/// 
		/// </summary>
		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_WithPalletNumberConstraintAndWeightConstraint_AddsPalletWithZeroWeight()
		{
			var forkLift = Helper.CreateEquipment("testy", 48, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 3, Constants.PkgUnit.Pallet);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			var palletUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 24m);
			palletUnit.OF_Weight = 24m;
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 24m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 24m, data.Whs1.DefaultLocation, "PLT-5");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 24m, data.Whs1.DefaultLocation, "PLT-4");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 24m, data.Whs1.DefaultLocation, "PLT-3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 24m, data.Whs1.DefaultLocation, "PLT-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part2, 24m, data.Whs1.DefaultLocation, "PLT-1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 72m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 48m);
			Helper.CreatePickNew(order);

			var palletPickLines1 = orderLine1.PickLines.Where(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Pallet);
			var palletPickLines2 = orderLine2.PickLines.Where(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Pallet);
			AssertEquals("Precondition: Correct amount allocated for Pallet.", 72m, palletPickLines1.Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition: Correct amount allocated for Pallet.", 48m, palletPickLines2.Sum(pl => pl.WZ_Units));

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate 2 Pallets.", 72m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate 2 Pallets of Part 1.", 48m, result.PickLines.Where(pl => pl.InventoryLine.WE_OP == data.Part1.PK).Sum(pl => pl.WZ_Units));
			AssertEquals("Expected to Allocate 1 Pallet of Part 2.", 24m, result.PickLines.Where(pl => pl.InventoryLine.WE_OP == data.Part2.PK).Sum(pl => pl.WZ_Units));
		}

		/// <summary>
		/// Equipment has a limit of 10KG, 10L and unlimited Cartons
		/// 
		/// A Carton of Part1 is made up of 5 Stock Units, Weighs 10KG and has no Volume
		/// A Carton of Part2 is made up of 5 Stock Units, Weighs nothing and has no Volume
		/// 
		/// Should Assign only one Carton of Part 1, since the weight limit is reached, which results in the pick line of 20 Units being split
		/// Should Assign all Cartons of Part 2, since Part 2 has no weight and volume.
		/// 
		/// </summary>
		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_CapacityNotExceededSplitWithFollowingUnlimitedItem()
		{
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 10, Constants.Volume.Litre, 0, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			var cartonUnit1 = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 5m);
			cartonUnit1.OF_Weight = 10m;
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 5m); // 0,0 items will always be included if available
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("precondition - 2 lines", 2, pick.GetAllPickLines().Count());
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("Expected split pickline and zero mass pickline.", 25m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to allocate Only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
			AssertEquals("Expected two pick lines.", 2, result.PickLines.Count());
			AssertEquals("Part 1 should be the split line of 5.", data.Part1, result.PickLines.Single(x => x.WZ_Units == 5m).SupplierPart);
			AssertEquals("Part 2 should be allocated fully.", data.Part2, result.PickLines.Single(x => x.WZ_Units == 20m).SupplierPart);
		}

		/// <summary>
		/// Equipment has a limit of 10KG, 10L and 3 Cartons
		/// 
		/// A Carton of Part1 is made up of 5 Stock Units, Weighs 10KG and has no Volume
		/// A Carton of Part2 is made up of 5 Stock Units, Weighs nothing and has no Volume
		/// 
		/// Should Assign only one Carton of Part 1, since the weight limit is reached, which results in the pick line of 20 Units being split
		/// Should Assign two Cartons of Part 2, since Part 2 has no weight and volume. Only two as there is a limit of 3 Cartons. This
		/// will result in the pick line of 20 Part 2 Units being split also.
		/// 
		/// </summary>
		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_CapacityNotExceededSplitWithFollowingUnlimitedItem_WithPackAndWeightCapacity()
		{
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 10, Constants.Volume.Litre, 3, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			var cartonUnit1 = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 5m);
			cartonUnit1.OF_Weight = 10m;
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 5m); // 0,0 items will always be included if available
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 2 lines", 2, pick.GetAllPickLines().Count());
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("Expected split pickline and zero mass pickline.", 15m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to allocate Only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
			AssertEquals("Expected two pick lines.", 2, result.PickLines.Count());
			AssertEquals("Part 1 should be the split line of 5.", data.Part1, result.PickLines.Single(x => x.WZ_Units == 5m).SupplierPart);
			AssertEquals("Part 2 should allocate 2 more Cartons.", data.Part2, result.PickLines.Single(x => x.WZ_Units == 10m).SupplierPart);
		}

		/// <summary>
		/// Equipment has a limit of 10KG, 10L and 3 Cartons
		/// 
		/// A Carton of Part1 is made up of 5 Stock Units, Weighs nothing and has a Volume of 10L
		/// A Carton of Part2 is made up of 5 Stock Units, Weighs nothing and has no Volume
		/// 
		/// Should Assign only one Carton of Part 1, since the volume limit is reached, which results in the pick line of 20 Units being split
		/// Should Assign two Cartons of Part 2, since Part 2 has no weight and volume. Only two as there is a limit of 3 Cartons. This
		/// will result in the pick line of 20 Part 2 Units being split also.
		/// 
		/// </summary>
		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_CapacityNotExceededSplitWithFollowingUnlimitedItem_WithPackAndVolumeCapacity()
		{
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 10, Constants.Volume.Litre, 3, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			var cartonUnit1 = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 5m);
			cartonUnit1.OF_Cubic = 10m;
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 5m); // 0,0 items will always be included if available
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 2 lines", 2, pick.GetAllPickLines().Count());
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("Expected split pickline and zero mass pickline.", 15m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to allocate Only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
			AssertEquals("Expected two pick lines.", 2, result.PickLines.Count());
			AssertEquals("Part 1 should be the split line of 5.", data.Part1, result.PickLines.Single(x => x.WZ_Units == 5m).SupplierPart);
			AssertEquals("Part 2 should allocate 2 more Cartons.", data.Part2, result.PickLines.Single(x => x.WZ_Units == 10m).SupplierPart);
		}

		/// <summary>
		/// Equipment has a limit of 10KG, 10L and 3 Cartons
		/// 
		/// A Carton of Part1 is made up of 5 Stock Units, Weighs 5KG and has no Volume
		/// A Carton of Part2 is made up of 5 Stock Units, Weighs nothing and has no Volume
		/// 
		/// Should Assign only two Cartons of Part 1, since the weight limit is reached, which results in the pick line of 15 Units being split
		/// Should Assign one Carton of Part 2, since Part 2 has no weight and volume. Only one as there is a limit of 3 Cartons. This
		/// will result in the PickLine of 20 Part 2 Units being split also.
		/// 
		/// </summary>
		public void TestFindAndAssignNextPick_EquipmentCapacity_PackType_ComplexSplitting()
		{
			var year = ZDateTime.Now.Year;
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 10, Constants.Volume.Litre, 3, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			var cartonUnit1 = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 5m);
			cartonUnit1.OF_Weight = 5m;
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 5m); // 0,0 items will always be included if available

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", new ZDateTimeOffset(year - 1, 1, 1), data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r2", new ZDateTimeOffset(year - 1, 1, 1), data.Part1, 15m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r3", new ZDateTimeOffset(year - 1, 1, 1), data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine.WE_PickGroup = 1;
			orderLine.ReserveStockIfAbleTo(receive1.Inventory[0]);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			orderLine2.ReserveStockIfAbleTo(receive2.Inventory[0]);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			orderLine3.ReserveStockIfAbleTo(receive3.Inventory[0]);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 3 lines", 3, pick.GetAllPickLines().Count());
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("Expected first pickline, split pickline and zero mass pickline.", 15m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to allocate Only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
			AssertEquals("Expected three pick lines.", 3, result.PickLines.Count());

			var part1Lines = result.PickLines.Where(x => x.SupplierPart == data.Part1).ToArray();
			var part2Lines = result.PickLines.Where(x => x.SupplierPart == data.Part2).ToArray();
			AssertEquals("Part 1 should be the split line of 5 + the first pick line of 5.", 10m, part1Lines.Sum(pl => pl.WZ_Units));
			AssertEquals("Part 2 should allocate 1 more Carton.", 5m, part2Lines.Sum(pl => pl.WZ_Units));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacityNotExceededWeight()
		{
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 10, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 3, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Receive20LinesOf(data);
			var order = CreateOrderOf20Lines(data);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("3 units of 3 kg expected.", 3m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacityNotExceededWeightOtherUnit()
		{
			var forkLift = Helper.CreateEquipment("testy", 10000, Constants.Weight.Grams, 10, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 3, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Receive20LinesOf(data);
			var order = CreateOrderOf20Lines(data);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("3 units of 3 kg expected.", 3m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacityNotExceededWeightExact()
		{
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 10, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 1, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Receive20LinesOf(data);
			var order = CreateOrderOf20Lines(data);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("10 units of 1kg expected.", 10m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacityNotExceededVolume()
		{
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 10, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0, Constants.Weight.Kilograms, 3, Constants.Volume.Litre);
			Receive20LinesOf(data);
			var order = CreateOrderOf20Lines(data);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("3 units of 3L expected.", 3m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacityNotExceededVolumeOtherUnit()
		{
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 10000, Constants.Volume.CubicCentimeters);
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0, Constants.Weight.Kilograms, 3, Constants.Volume.Litre);
			Receive20LinesOf(data);
			var order = CreateOrderOf20Lines(data);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("3 units of 3L expected.", 3m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacityNotExceededVolumeExact()
		{
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 10, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0, Constants.Weight.Kilograms, 1, Constants.Volume.Litre);
			Receive20LinesOf(data);
			var order = CreateOrderOf20Lines(data);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("10 units of 1L expected.", 10m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacityNotExceededDual()
		{
			var forkLift = Helper.CreateEquipment("testy", 12, Constants.Weight.Kilograms, 15, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 4, Constants.Weight.Kilograms, 5, Constants.Volume.Litre);
			Receive20LinesOf(data);
			var order = CreateOrderOf20Lines(data);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("3 units expected.", 3m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacityUnlimited()
		{
			var forkLift = Helper.CreateEquipment("testy", 0, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 100, Constants.Weight.Kilograms, 50, Constants.Volume.Litre);
			Receive20LinesOf(data, 10);
			var order = CreateOrderOf20Lines(data, 200);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("200 units expected.", 200m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_AvailablePalletInventories_MaxStockOnHand()
		{
			var forkLift = Helper.CreateEquipment("testy", 0, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 0m;
			data.Part1.OP_Cubic = 0m;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 999999999999999m, null, "PLT-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 999999999999999m, null, "PLT-1");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 1 line.", 1, pick.GetAllPickLines().Count());

			FindPickResult result = null;
			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			AssertNoExceptionThrown(() => result = linesPicker.FindAndAssignNextPick(order.WD_DocketID));
			AssertEquals("1 unit expected.", 1m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_MaxEquipmentCapacity()
		{
			var forkLift = Helper.CreateEquipment("testy", 0, Constants.Weight.Kilograms, 0, Constants.Volume.Litre, 2147483647, Constants.PkgUnit.Carton);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			var cartonUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 1 line", 1, pick.GetAllPickLines().Count());
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			FindPickResult result = null;
			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			AssertNoExceptionThrown(() => result = linesPicker.FindAndAssignNextPick(order.WD_DocketID));
			AssertEquals("Expected pickline allocated.", 20m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to allocate Only Cartons.", true, result.PickLines.All(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Carton));
		}

		public void TestFindAndAssignNextPick_SumOfUnitsOnSinglePalletBiggerThanMaxInt()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			data.Part1.OP_Weight = 0m; // To bypass the WD_TotalWeight checking
			data.Part1.OP_Cubic = 0m;
			data.Part2.OP_Weight = 0m;
			data.Part2.OP_Cubic = 0m;
			part3.OP_Weight = 0m;
			part3.OP_Cubic = 0m;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			part3.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(part3, Constants.PkgUnit.Bag, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 21474836470m, data.Whs1.FindLocation("A-1"), "PLT01");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r2", data.Part2, 21474836470m, data.Whs1.FindLocation("A-1"), "PLT01");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r3", part3, 21474836470m, data.Whs1.FindLocation("A-1"), "PLT01");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 999999998m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 999999998m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, part3, 999999998m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 3 lines", 3, pick.GetAllPickLines().Count());
			orderLine1.PickLines[0].WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
			orderLine2.PickLines[0].WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
			orderLine3.PickLines[0].WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Bag;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			FindPickResult result = null;
			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			AssertNoExceptionThrown(() => result = linesPicker.FindAndAssignNextPick(""));
			AssertEquals("Ignore the line which causing overflow.", 1999999996m, PickLineUnitSum(result, picker));
		}

		void Receive20LinesOf(TestDataSimpleEnvironment data, decimal quantity = 1)
		{
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "r1");

			for (var i = 0; i < 20; i++)
			{
				Helper.CreateWhsReceiveLine(receive, data.Part1, quantity);
			}

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			AssertIsFinalisedPrecondition(receive);
		}

		WhsOrder CreateOrderOf20Lines(TestDataSimpleEnvironment data, decimal lineQuantity = 20m)
		{
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, lineQuantity);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 20 lines.", 20, pick.GetAllPickLines().Count());

			return order;
		}

		decimal PickLineUnitSum(FindPickResult pickResult, GlbStaff picker)
		{
			var assignedPicklines = pickResult.PickLines;
			AssertEquals("All Pick Lines should be assigned to the correct Picker.", true, assignedPicklines.All(pl => pl.WZ_GS_NKAssignedTo == picker.GS_Code));

			return assignedPicklines.Sum(pl => pl.WZ_Units);
		}

		#endregion

		#region TestFindAndAssignNextPick_SplitEquipmentCapacity

		public void TestFindAndAssignNextPick_EquipmentCapacityNotExceededSplitVolume()
		{
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 10, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0, Constants.Weight.Kilograms, 2, Constants.Volume.Litre);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20);
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 20);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 1 bulk line.", 1, pick.GetAllPickLines().Count());

			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("5 (split) units expected.", 5m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacityNotExceededSplitVolumeDecimal()
		{
			var forkLift = Helper.CreateEquipment("testy", 50, Constants.Weight.Kilograms, 30, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 5, Constants.Weight.Kilograms, 20, Constants.Volume.Litre);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20);
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 20);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 1 bulk line.", 1, pick.GetAllPickLines().Count());

			data.Part1.OP_CountDecimalPlaces = 1;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("1.5 (split) units expected.", 1.5m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacityNotExceededSplitWeight()
		{
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 10, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20);
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 20);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 1 bulk line.", 1, pick.GetAllPickLines().Count());

			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("5 (split) units expected.", 5m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacityNotExceededSplitWeightDecimal()
		{
			var forkLift = Helper.CreateEquipment("testy", 30, Constants.Weight.Kilograms, 50, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 20, Constants.Weight.Kilograms, 5, Constants.Volume.Litre);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20);
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 20);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 1 bulk line.", 1, pick.GetAllPickLines().Count());

			data.Part1.OP_CountDecimalPlaces = 1;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("1.5 (split) units expected.", 1.5m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_EquipmentCapacityNotExceededSplitWithFollowingUnlimitedItem()
		{
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 10, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Helper.SetProductWeightAndVolume(data.Part2, 0, Constants.Weight.Kilograms, 0, Constants.Volume.Litre); // 0,0 items will always be included if available
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r2", data.Part2, 20);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 20);
			Helper.CreateWhsOrderLine(order, data.Part2, 20);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("precondition - 2 lines", 2, pick.GetAllPickLines().Count());
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("Expected split pickline and zero mass pickline.", 25m, PickLineUnitSum(result, picker));
			AssertEquals("Expected two pick lines.", 2, result.PickLines.Count());
			AssertEquals("Part 1 should be the split line of 5.", data.Part1, result.PickLines.Single(x => x.WZ_Units == 5).SupplierPart);
			AssertEquals("Part 2 should be allocated fully.", data.Part2, result.PickLines.Single(x => x.WZ_Units == 20).SupplierPart);
		}

		public void TestFindAndAssignNextPick_EquipmentCapacityNotExceededSplitWithFollowingItem()
		{
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 10, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			// items with a weight or volume will not be picked once a capacity is reached, even though they may fit
			Helper.SetProductWeightAndVolume(data.Part2, 0, Constants.Weight.Kilograms, 1, Constants.Volume.Litre);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r2", data.Part2, 1);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 20);
			Helper.CreateWhsOrderLine(order, data.Part2, 1);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("precondition - 2 lines", 2, pick.GetAllPickLines().Count());
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("Expected only the split pickline.", 5m, PickLineUnitSum(result, picker));
			AssertEquals("Expected only the split pickline.", 1, result.PickLines.Count());
			AssertEquals("Part 1 should be the only pickline.", data.Part1, result.PickLines.Single().SupplierPart);
		}

		public void TestFindAndAssignNextPick_UomSplitCasesNoEquipment()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, 10);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			Helper.SetProductWeightAndVolume(data.Part1, 1, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);

			var receiveDocket = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("precondition - 1 bulk line.", 1, pick.GetAllPickLines().Count());
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("Should allocated in full.", 10m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_UomSplitCasesSplittable()
		{
			var forkLift = Helper.CreateEquipment("testy", 9, Constants.Weight.Kilograms, 9, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, 10);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			Helper.SetProductWeightAndVolume(data.Part1, 1, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("precondition - 1 bulk line", 1, pick.GetAllPickLines().Count());
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("Should be split for capacity.", 9m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_UomCasesAredAssignedWhenEquipmentRegistered()
		{
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 10, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			var boxUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Box, 5);
			boxUnit.OF_Weight = 5m;
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 1 bulk line.", 1, pick.GetAllPickLines().Count());
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);
			AssertEquals("Should all be allocated.", 10m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_UomCasesAllocateable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Box, 5);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);
			Helper.SetProductWeightAndVolume(data.Part1, 1, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 1 bulk line.", 1, pick.GetAllPickLines().Count());
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("Should be allocated in full.", 10m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_UomPalletsAredAssignedWhenEquipmentRegistered()
		{
			var forkLift = Helper.CreateEquipment("testy", 10, Constants.Weight.Kilograms, 10, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Box, 5);
			var palletUnit = Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Pallet, 10);
			palletUnit.OF_Weight = 10m;
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 1 bulk line.", 1, pick.GetAllPickLines().Count());
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);
			AssertEquals("Should all be allocated.", 10m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_UomPalletsAllocateable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Box, 5);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Pallet, 10);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);
			Helper.SetProductWeightAndVolume(data.Part1, 1, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 1 bulk line.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition - Allocated a PLT.", Constants.PkgUnit.Pallet, pick.GetAllPickLines().Single().WZ_F3_NKAllocatedPackType);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);

			AssertEquals("Should be allocated in full", 10m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_PalletsNotSplit()
		{
			var forkLift = Helper.CreateEquipment("testy", 9, Constants.Weight.Kilograms, 9, Constants.Volume.Litre);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 1, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			var receiveDocket = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 10, data.Whs1.FindLocation("A-1"), "palletID");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 1 bulk line.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition - 10 units allocated.", 10m, pick.GetAllPickLines().Sum(l => l.WZ_Units));

			var criteria = new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" };
			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, criteria);
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);
			AssertNull("Picker's equipment cannot pick full pallet, therefore, pick nothing.", result.Pick);
			AssertEquals("Pick line was not split.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Allocated qty should not change.", 10m, pick.GetAllPickLines().Sum(l => l.WZ_Units));
		}

		public void TestFindAndAssignNextPick_PalletsNotSplit_CannotPickSecondPallet_WeightLimit()
		{
			var forkLift = Helper.CreateEquipment("testy", 8, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Helper.SetProductWeightAndVolume(data.Part1, 1m, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Helper.SetProductWeightAndVolume(data.Part2, 1m, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3m, data.Whs1.DefaultLocation, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 3m, data.Whs1.DefaultLocation, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 1m, data.Whs1.DefaultLocation, "PLT-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 5m, data.Whs1.DefaultLocation, "PLT-2");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 6m);
			Helper.CreatePickNew(order);
			AssertEquals("Precondition: Correct amount allocated for Order 1.", 6m, orderLine1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition: Correct amount allocated for Order 2.", 6m, orderLine2.PickLines.Sum(pl => pl.WZ_Units));

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate first descending Pallet Only.", 6m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate first descending Pallet Only.", true, result.PickLines.All(pl => pl.InventoryLine.WE_PalletID == "PLT-2"));
		}

		public void TestFindAndAssignNextPick_PalletsNotSplit_CannotPickSecondPallet_VolumeLimit()
		{
			var forkLift = Helper.CreateEquipment("testy", 0, Constants.Weight.Kilograms, 8, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Helper.SetProductWeightAndVolume(data.Part1, 0m, Constants.Weight.Kilograms, 1m, Constants.Volume.Litre);
			Helper.SetProductWeightAndVolume(data.Part2, 0m, Constants.Weight.Kilograms, 1m, Constants.Volume.Litre);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3m, data.Whs1.DefaultLocation, "PLT-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 3m, data.Whs1.DefaultLocation, "PLT-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 1m, data.Whs1.DefaultLocation, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 5m, data.Whs1.DefaultLocation, "PLT-1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 6m);
			Helper.CreatePickNew(order);
			AssertEquals("Precondition: Correct amount allocated for Order 1.", 6m, orderLine1.PickLines.Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition: Correct amount allocated for Order 2.", 6m, orderLine2.PickLines.Sum(pl => pl.WZ_Units));

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick("");
			AssertEquals("Expected to Allocate first descending Pallet Only.", 6m, PickLineUnitSum(result, picker));
			AssertEquals("Expected to Allocate first descending Pallet Only.", true, result.PickLines.All(pl => pl.InventoryLine.WE_PalletID == "PLT-2"));
		}

		public void TestFindAndAssignNextPick_PalletsNotSplit_MultiplePickLines()
		{
			var forkLift = Helper.CreateEquipment("testy", 9, Constants.Weight.Kilograms, 9, Constants.Volume.Litre);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 1, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 5m, data.Whs1.FindLocation("A-1"), "palletID");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r2", data.Part1, 5m, data.Whs1.FindLocation("A-1"), "palletID");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 2 pick lines Allocated.", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition - 10 units allocated.", 10m, pick.GetAllPickLines().Sum(l => l.WZ_Units));

			var criteria = new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" };
			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, criteria);
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);
			AssertNull("Picker's equipment cannot pick full pallet, therefore, pick nothing.", result.Pick);
			AssertEquals("Pick line was not split.", 2, pick.GetAllPickLines().Count());
			AssertEquals("Allocated qty should not change.", 10m, pick.GetAllPickLines().Sum(l => l.WZ_Units));
		}

		public void TestFindAndAssignNextPick_PalletsSplit_MultiProduct()
		{
			// Existing undocumented behaviour: multi-product pallets will NOT use any complete/full pallet picking logic.
			// All multi-product pallets are splittable.
			var forkLift = Helper.CreateEquipment("testy", 9, Constants.Weight.Kilograms, 9, Constants.Volume.Litre);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 1, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Helper.SetProductWeightAndVolume(data.Part2, 1, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 5, data.Whs1.FindLocation("A-1"), "palletID");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r2", data.Part2, 5, data.Whs1.FindLocation("A-1"), "palletID");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 5);
			Helper.CreateWhsOrderLine(order, data.Part2, 5);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 2 lines.", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition - 10 units allocated.", 10m, pick.GetAllPickLines().Sum(l => l.WZ_Units));

			var criteria = new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" };
			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, criteria);
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);
			AssertEquals("Picker's equipment cannot pick full pallet, therefore split.", 9m, PickLineUnitSum(result, picker));
			AssertEquals("Pick lines split.", 3, pick.GetAllPickLines().Count());
			AssertEquals("Allocated qty should not change.", 10m, pick.GetAllPickLines().Sum(l => l.WZ_Units));
		}

		public void TestFindAndAssignNextPick_PalletsSplit_SubsetPalletPicking()
		{
			var forkLift = Helper.CreateEquipment("testy", 9, Constants.Weight.Kilograms, 9, Constants.Volume.Litre);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 1, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 15, data.Whs1.FindLocation("A-1"), "palletID");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 1 bulk line.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition - 10 units allocated.", 10m, pick.GetAllPickLines().Sum(l => l.WZ_Units));

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);
			AssertEquals("Should be split.", 9m, PickLineUnitSum(result, picker));
			AssertEquals("Pick line should be split.", 2, pick.GetAllPickLines().Count());
			AssertEquals("Allocated qty should not change.", 10m, pick.GetAllPickLines().Sum(l => l.WZ_Units));
		}

		public void TestFindAndAssignNextPick_PalletsNotSplit_MultiPalletPicking()
		{
			var forkLift = Helper.CreateEquipment("testy", 20, Constants.Weight.Kilograms, 20, Constants.Volume.Litre);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 1, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 15, data.Whs1.FindLocation("A-1"), "palletID");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r2", data.Part1, 15, data.Whs1.FindLocation("A-2"), "palletID2");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 30);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 2 bulk lines.", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition - 30 units allocated.", 30m, pick.GetAllPickLines().Sum(l => l.WZ_Units));

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);
			AssertEquals("Should only pick one pallet.", 15m, PickLineUnitSum(result, picker));
			AssertEquals("Pick line was not split.", 2, pick.GetAllPickLines().Count());
			AssertEquals("Allocated qty should not change.", 30m, pick.GetAllPickLines().Sum(l => l.WZ_Units));
		}

		public void TestFindAndAssignNextPick_PalletsSplit_MultiPalletPicking_OnePalletPartiallyAllocated()
		{
			var forkLift = Helper.CreateEquipment("testy", 20, Constants.Weight.Kilograms, 20, Constants.Volume.Litre);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetProductWeightAndVolume(data.Part1, 1, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 15, data.Whs1.FindLocation("A-1"), "palletID");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r2", data.Part1, 15, data.Whs1.FindLocation("A-2"), "palletID2");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 21m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 2 bulk lines", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition - 21 units allocated", 21m, pick.GetAllPickLines().Sum(l => l.WZ_Units));

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);
			AssertEquals("Should split palletID2 to fit capacity as not picking full pallet.", 20m, PickLineUnitSum(result, picker));
			AssertEquals("Pick should allocate all of Pallet 1 and part of Pallet 2.", 2, result.PickLines.Count());
			AssertEquals("Pick should allocate all of Pallet 1 and part of Pallet 2.", "palletID", result.PickLines.Single(pl => pl.WZ_Units == 15m).InventoryLine.WE_PalletID);
			AssertEquals("Pick should allocate all of Pallet 1 and part of Pallet 2.", "palletID2", result.PickLines.Single(pl => pl.WZ_Units == 5m).InventoryLine.WE_PalletID);
			AssertEquals("Pick line should be split.", 3, pick.GetAllPickLines().Count());
			AssertEquals("Allocated qty should not change.", 21m, pick.GetAllPickLines().Sum(l => l.WZ_Units));
		}

		public void TestFindAndAssignNextPick_PalletsSplit_MultiProductPalletPicking()
		{
			var forkLift = Helper.CreateEquipment("testy", 20, Constants.Weight.Kilograms, 20, Constants.Volume.Litre);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 15, data.Whs1.FindLocation("A-1"), "palletID");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r2", data.Part2, 15, data.Whs1.FindLocation("A-1"), "palletID");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 1 line.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition - 15 units allocated.", 15m, pick.GetAllPickLines().Sum(l => l.WZ_Units));

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);
			AssertEquals("Should split palletID.", 10m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_ReturnsPicklinesWhenSplittableTooBig()
		{
			var forkLift = Helper.CreateEquipment("testy", 20, Constants.Weight.Kilograms, 20, Constants.Volume.Litre);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 30, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Helper.SetProductWeightAndVolume(data.Part2, 1, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r2", data.Part2, 1);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 2m); // order two units to make sure it's splittable
			Helper.CreateWhsOrderLine(order, data.Part2, 1m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 2 bulk lines.", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition - 3 units allocated.", 3m, pick.GetAllPickLines().Sum(l => l.WZ_Units));

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_DocketID);
			AssertEquals("Should return part2 as part1 when split is too big.", data.Part2.PK, result.PickLines.Single().SupplierPart.PK);
			AssertEquals("Can't pick part 1.", 1m, PickLineUnitSum(result, picker));
			AssertEquals("Allocated qty should not change.", 3m, pick.GetAllPickLines().Sum(l => l.WZ_Units));
		}

		#endregion

		#region TestFindAndAssignNextPick_PickByReference

		public void TestFindAndAssignNextPick_PicksWithSameExternalReference()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10);
			var pick1 = Helper.CreatePickNew(order1);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o1");
			order2.WD_ExternalReferenceSplit = 1;
			pick1.PickPriority = 1;
			Helper.CreateWhsOrderLine(order2, data.Part1, 10);
			Helper.CreatePickNew(order2);
			Factory.Save();

			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.FindAndAssignNextPick(order1.WD_ExternalReference);

			AssertEquals("Pick 1 should be returned due to higher priority.", pick1.PK, result.Pick.PK);
		}

		public void TestFindAndAssignNextPick_PickWithDoubleMatchingReference()
		{
			var forkLift = Helper.CreateEquipment("testy", 8, Constants.Weight.Kilograms, 20, Constants.Volume.Litre);
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 3, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Helper.SetProductWeightAndVolume(data.Part2, 3, Constants.Weight.Kilograms, 0, Constants.Volume.Litre);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r1", data.Part1, 20);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "r2", data.Part2, 20);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o1");
			Helper.CreateWhsOrderLine(order, data.Part1, 1);
			Helper.CreateWhsOrderLine(order, data.Part2, 1);
			Helper.CreatePickNew(order);
			Factory.Save();
			order.WD_DocketID = order.WD_ExternalReference;
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", EquipmentRegistrationNumber = "testy" });
			var result = linesPicker.FindAndAssignNextPick(order.WD_ExternalReference);

			AssertEquals("Matching references should not affect result.", 2m, PickLineUnitSum(result, picker));
			AssertContainsExactElementsInAnyOrder("Result should contain one pick line for part 1 and one pick line for part 2.",
				new[] { data.Part1.PK, data.Part2.PK }, result.PickLines.Select(x => x.SupplierPart.PK));
		}

		#endregion

		#region TestFindAndAssignNextPickPerformance_ForDifferentClients

		[StressTest]
		public void TestFindAndAssignNextPickPerformance_ForDifferentClients()
		{
			var operator1 = Factory.NewWithValidTestData<GlbStaff>();
			var data = new TestDataSimpleEnvironment(Factory);
			var orders = new List<WhsOrder>();

			for (int count = 0; count < 10; count++)
			{
				var client = Helper.CreateClient("C" + count.ToString());
				var product = Helper.CreateProduct(client, "P" + count.ToString());
				Helper.CreateProductClientRelationShip(client, product);

				Helper.CreateWhsReceiveWithInventory(client, data.Whs1, "R" + count, product, 10m);
				var order = Helper.CreateWhsOrderWithOrderLine(client, data.Whs1, "O" + count, product, 1m);
				Factory.Save();
				orders.Add(order);
			}

			var pick = Helper.CreatePickNew(orders.ToArray());
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var linesPickerForFirstClient = new PickLinesLoader(newFactory, data.Whs1, operator1, new SearchFilterCriteriaInfo { ClientCode = orders[0].Client.OH_Code });
			AssertEquals(linesPickerForFirstClient.FindAndAssignNextPick(ZString.Empty).Pick.PK, pick.PK);
			var expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 0 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};
			AssertDbHits(expectedDBHits, newFactory);
		}

		#endregion

		#region TestFindAndAssignNextPick_LargeMetrics

		public void TestFindAndAssignNextPick_LargeMetrics_LargeWeightUQ()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, null, "PLT-1");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 1 line.", 1, pick.GetAllPickLines().Count());

			data.Part1.OP_Weight = 15m;
			data.Part1.OP_WeightUQ = Constants.Volume.MegaLitre;

			FindPickResult result = null;
			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			AssertNoExceptionThrown(() => result = linesPicker.FindAndAssignNextPick(order.WD_DocketID));
			AssertEquals(pick, result.Pick);
			AssertEquals(1m, PickLineUnitSum(result, picker));
		}

		public void TestFindAndAssignNextPick_LargeMetrics_LargeVolumeUQ()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Part1.OP_Weight = 0m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, null, "PLT-1");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition - 1 line.", 1, pick.GetAllPickLines().Count());

			data.Part1.OP_Cubic = 1000m;
			data.Part1.OP_CubicUQ = Constants.Volume.MegaLitre;

			FindPickResult result = null;
			var linesPicker = new PickLinesLoader(Factory, data.Whs1, picker, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
			AssertNoExceptionThrown(() => result = linesPicker.FindAndAssignNextPick(order.WD_DocketID));
			AssertEquals(pick, result.Pick);
			AssertEquals(1m, PickLineUnitSum(result, picker));
		}

		#endregion

		#region TestGetOrder

		#region TestGetOrder_WithPickLinesCommittingDockDoorStock

		public void TestGetOrder_WithPickLinesCommittingDockDoorStock()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			order1.WD_PickPriority = 1;
			var order1Line = order1.Lines[0];

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			order2.WD_PickPriority = 2;
			var order2Line = order2.Lines[0];

			// Create (higher priority) pick only committing dock door stock
			var pick1 = Helper.CreatePickNew(order1);
			var pick1Line = order1Line.PickLines.Single();
			pick1Line.WZ_GS_NKAssignedTo = staff.GS_Code;
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pick1Line, ZDateTimeOffset.Now);
			transferLine1.WE_GS_NKPutawayBy = "";

			// Create pick with pick lines valid to be sent to RF
			var pick2 = Helper.CreatePickNew(order2);
			var pick2Line = order2Line.PickLines.Single();

			var splitPick2Line = pick2Line.Split(1m);
			splitPick2Line.WZ_GS_NKAssignedTo = staff.GS_Code;
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(splitPick2Line, ZDateTimeOffset.Now);
			transferLine2.WE_GS_NKPutawayBy = "";
			Helper.Factory.Save();

			var linesPicker = new PickLinesLoader(Helper.Factory, data.Whs1, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.GetOrder(null, false);

			AssertEquals("Should return order2.", order2, result);
		}

		#endregion

		#region TestGetOrder_WithPickLinesCommittingDockDoorStock_MultiOrderPick

		public void TestGetOrder_WithPickLinesCommittingDockDoorStock_MultiOrderPick()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			order1.WD_PickPriority = 1;
			var order1Line = order1.Lines[0];

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			order2.WD_PickPriority = 2;
			var order2Line = order2.Lines[0];

			var pick = Helper.CreatePickNew(order1, order2);
			var pickLine = order1Line.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Helper.Factory.Save();

			var linesPicker = new PickLinesLoader(Helper.Factory, data.Whs1, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.GetOrder(null, false);

			AssertEquals("Should return order2.", order2, result);
		}

		#endregion

		public void TestGetOrder_OnlyFindsOrders()
		{
			// link unit type with splitcase, box with case
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var staff1 = Helper.CreateGlbStaff("AAA", "A.A");
			var staff2 = Helper.CreateGlbStaff("BBB", "B.B");

			var bikeFrame = Helper.CreateProduct(data.Org1, "FRAME");
			var bikeWheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var bomBike = Helper.CreateProduct(data.Org1, "BIKE");
			Helper.CreateProductBOM(bomBike, bikeFrame);
			Helper.CreateProductBOM(bomBike, bikeWheel, 2m, bikeWheel.OP_StockKeepingUnit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var frameInventory = Helper.CreateWhsReceiveLine(receive, bikeFrame, 10m);
			var wheelInventory = Helper.CreateWhsReceiveLine(receive, bikeWheel, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			var year = ZDateTime.Now.Year;
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Helper.Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, bomBike, 10m);
			var workOrderPick = Helper.CreatePickNew(workOrder);
			workOrderPick.WP_CartoniseSplitCases = false;
			workOrderPick.GetAllPickLines().First().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
			Helper.Factory.Save();

			var linesPicker = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.GetOrder(null, false);

			AssertNull("No valid orders", result);
		}

		public void TestGetOrder()
		{
			// link unit type with splitcase, box with case
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("AAA", "A.A");
			var staff2 = Helper.CreateGlbStaff("BBB", "B.B");

			// create 2 locations in 2 different areas
			var location1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "Row11", 1, 1).Locations[0];
			var location2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "Row12", 1, 1).Locations[0];
			location1.WLV_WA_PickingArea = Helper.CreateArea(data.Whs1, "AREA1").PK;
			location2.WLV_WA_PickingArea = Helper.CreateArea(data.Whs1, "AREA2").PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, location2, "");

			Helper.Factory.Save();

			var nonSplitCaseOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var nonSplitCaseOrderLine = Helper.CreateWhsOrderLine(nonSplitCaseOrder, data.Part1, 1m);
			var nonSplitCasePick = Helper.CreatePickNew(nonSplitCaseOrder);
			nonSplitCaseOrderLine.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Box;

			var linesPicker1 = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result1 = linesPicker1.GetOrder(null, false);
			AssertNull("No valid orders", result1);

			nonSplitCasePick.WP_CartoniseSplitCases = false;
			Helper.Factory.Save();

			var linesPicker2 = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result2 = linesPicker2.GetOrder(null, false);
			AssertNull("No valid orders", result2);

			var assignedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var assignedOrderLine = Helper.CreateWhsOrderLine(assignedOrder, data.Part1, 1m);
			var assignedPick = Helper.CreatePickNew(assignedOrder);
			assignedPick.WP_CartoniseSplitCases = false;
			assignedOrderLine.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
			assignedOrderLine.PickLines.Single().WZ_GS_NKAssignedTo = staff1.GS_Code;
			Helper.Factory.Save();

			var linesPicker3 = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result3 = linesPicker3.GetOrder(null, false);
			AssertNull("No valid orders", result3);

			var cartonisedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var cartonisedOrderLine = Helper.CreateWhsOrderLine(cartonisedOrder, data.Part1, 1m);
			var cartonisedPick = Helper.CreatePickNew(cartonisedOrder);
			cartonisedPick.WP_CartoniseSplitCases = true;
			cartonisedOrderLine.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
			Helper.Factory.Save();

			var linesPicker4 = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result4 = linesPicker4.GetOrder(null, false);
			AssertNull("No valid orders", result4);

			var validOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O4");
			var validOrderLine1 = Helper.CreateWhsOrderLine(validOrder, data.Part1, 1m);
			var validOrderLine2 = Helper.CreateWhsOrderLine(validOrder, data.Part1, 1m);
			var validPick = Helper.CreatePickNew(validOrder);
			validPick.WP_CartoniseSplitCases = false;
			validOrderLine1.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
			validOrderLine2.PickLines.Single().WZ_GS_NKAssignedTo = staff1.GS_Code;
			Helper.Factory.Save();

			var linesPicker5 = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result5 = linesPicker5.GetOrder(null, false);
			AssertEquals("order4 is valid as it is - unassigned, uncartonised, has split and right area", validOrder, result5);

			var linesPicker6 = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "AREA2", PickMethod = "ANY" });
			var result6 = linesPicker6.GetOrder(null, false);
			AssertEquals("order4 is valid as it is - unassigned, uncartonised, has split and right area", validOrder, result6);

			var linesPicker7 = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "AREA1", PickMethod = "ANY" });
			var result7 = linesPicker7.GetOrder(null, false);
			AssertNull("order4 is invalid as it is - unassigned, uncartonised, has split but in the wrong area", result7);
		}

		public void TestGetOrder_FiltersByDDL()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var staff1 = Helper.CreateGlbStaff("AAA", "A.A");

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var otherDockDoorLocation = data.Whs1.FindLocation("A-2");
			otherDockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save(); // to create stock

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_WL_DockDoor = otherDockDoorLocation.PK;
			Helper.Factory.Save();

			var linesPicker1 = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result1 = linesPicker1.GetOrder(null, false);
			AssertEquals("Should return the order if no dock door passed in", order, result1);

			var linesPicker2 = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result2 = linesPicker2.GetOrder(otherDockDoorLocation.PK, false);
			AssertEquals("Should return the order if correct dock door passed in", order, result2);

			var linesPicker3 = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result3 = linesPicker3.GetOrder(data.Whs1.WW_DefaultOutboundDockDoor, false);
			AssertNull("Should *not* return the order if another dock door is passed in", result3);
		}

		public void TestGetOrder_HigherPriorityPickHasADifferentDDL()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var staff1 = Helper.CreateGlbStaff("AAA", "A.A");

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var otherDockDoorLocation = data.Whs1.FindLocation("A-2");
			otherDockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			order1.WD_PickPriority = 2;
			order2.WD_PickPriority = 1;
			pick2.WP_WL_DockDoor = otherDockDoorLocation.PK;
			Helper.Factory.Save();

			var linesPicker1 = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result1 = linesPicker1.GetOrder(null, false);
			AssertEquals("Precondition: Higher priority pick returned.", order2, result1);

			var linesPicker2 = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result2 = linesPicker2.GetOrder(data.Whs1.WW_DefaultOutboundDockDoor, false);
			AssertEquals("Should return the lower priority pick with the correct dock door location.", order1, result2);
		}

		public void TestGetOrder_Filtering()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			//link unit type with splitcase, box with case
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Box, UOMPackTypesList.Codes.Case);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("AAA", "A.A");
			var staff2 = Helper.CreateGlbStaff("BBB", "B.B");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save();

			var invalidOrder1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O");
			var invalidOrderLine1 = Helper.CreateWhsOrderLine(invalidOrder1, data.Part1, 1m);
			invalidOrder1.WD_PickPriority = 5;

			var invalidOrder2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var invalidOrder1Line = Helper.CreateWhsOrderLine(invalidOrder2, data.Part1, 1m);
			invalidOrder2.WD_PickPriority = 5;

			var validOrder1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var validOrder1Line1 = Helper.CreateWhsOrderLine(validOrder1, data.Part1, 1m);
			validOrder1.WD_PickPriority = 3;

			var validOrder2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var validOrder2Line1 = Helper.CreateWhsOrderLine(validOrder2, data.Part1, 1m);
			validOrder2.WD_PickPriority = 2;
			//if there is at least 1 order correct in the pick, it will return the whole pick
			Helper.CreatePickNew(new[] { invalidOrder1, invalidOrder2, validOrder2 });
			Helper.CreatePickNew(new[] { validOrder1 });

			invalidOrderLine1.PickLines.Single().WZ_GS_NKAssignedTo = staff1.GS_Code;
			invalidOrderLine1.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
			invalidOrder1Line.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Box;
			validOrder1Line1.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
			validOrder2Line1.PickLines.Single().WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;

			Helper.Factory.Save();

			var linesPicker = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.GetOrder(null, false);
			AssertEquals("Pick from valid by highest priority.", validOrder2, result);
		}

		public void TestGetOrder_MultipleOrdersOnPick_ReferencedOrderIsReturned()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("AAA", "A.A");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			order1.WD_PickPriority = 4;

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			order2.WD_PickPriority = 3;

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			Helper.CreateWhsOrderLine(order3, data.Part1, 1m);
			order3.WD_PickPriority = 2;

			var order4 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O4");
			Helper.CreateWhsOrderLine(order4, data.Part1, 1m);
			order4.WD_PickPriority = 1;

			Helper.CreatePickNew(new[] { order1, order2, order3, order4 });
			Helper.Factory.Save();

			var linesPicker = new PickLinesLoader(Helper.Factory, data.Whs1, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result1 = linesPicker.GetOrder(null, false, "O1");
			AssertEquals("Correct order is returned.", "O1", result1.WD_ExternalReference);

			var result2 = linesPicker.GetOrder(null, false, "O2");
			AssertEquals("Correct order is returned.", "O2", result2.WD_ExternalReference);

			var result3 = linesPicker.GetOrder(null, false, "O3");
			AssertEquals("Correct order is returned.", "O3", result3.WD_ExternalReference);

			var result4 = linesPicker.GetOrder(null, false, "O4");
			AssertEquals("Correct order is returned.", "O4", result4.WD_ExternalReference);
		}

		public void TestGetOrder_MultipleOrdersOnPick_SameReference_FirstIsReturned()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("AAA", "A.A");

			var org2 = Helper.CreateClient("000");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 10m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "SameOrderRef");
			Helper.CreateWhsOrderLine(order1, data.Part1, 1m);

			var order2 = Helper.CreateWhsOrder(org2, data.Whs1, "SameOrderRef");
			Helper.CreateWhsOrderLine(order2, data.Part1, 1m);

			Helper.CreatePickNew(new[] { order1, order2 });
			Helper.Factory.Save();

			var linesPicker = new PickLinesLoader(Helper.Factory, data.Whs1, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.GetOrder(null, false, "SameOrderRef");
			AssertEquals("Correct order is returned.", order2, result);
		}

		public void TestGetOrder_BuildingPicksFiltered_RegistryOn()
		{
			TestGetOrder_BuildingPicksFilteredCore(true);
		}

		public void TestGetOrder_BuildingPicksFiltered_RegistryOff()
		{
			TestGetOrder_BuildingPicksFilteredCore(false);
		}

		void TestGetOrder_BuildingPicksFilteredCore(bool regEnabled)
		{
			using (WarehouseDataRegistry.Instance.EnablePickLineLoaderBuildingStatusFilter.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regEnabled))
			{
				var packingHelper = new PackingTestHelper(Helper.Factory);
				packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

				var data = new TestDataSimpleEnvironment(Helper.Factory);
				data.Whs1.WW_IsPickByUOMEnabled = true;
				var staff1 = Helper.CreateGlbStaff("AAA", "A.A");

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, true, true);
				Factory.Save();

				var order_WithoutShortfall = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
				order_WithoutShortfall.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
				Helper.CreateWhsOrderLine(order_WithoutShortfall, data.Part1, 10m);

				var order_WithShortfallAndAllFulfillment1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2");
				order_WithShortfallAndAllFulfillment1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
				Helper.CreateWhsOrderLine(order_WithShortfallAndAllFulfillment1, data.Part1, 1000m);

				var order_WithShortfallAndAllFulfillment2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "3");
				order_WithShortfallAndAllFulfillment2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
				Helper.CreateWhsOrderLine(order_WithShortfallAndAllFulfillment2, data.Part1, 1000m);
				Factory.Save();

				var pickWithCreatedStatus = Helper.CreatePickNew(order_WithoutShortfall);
				AssertEquals("Precondition", PickStatus.Codes.Created, pickWithCreatedStatus.WP_PickStatus);

				var pickWithBuildingStatus1 = Helper.CreatePickNew(order_WithShortfallAndAllFulfillment1);
				AssertEquals("Precondition", PickStatus.Codes.Building, pickWithBuildingStatus1.WP_PickStatus);

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 100m, true, true);
				Factory.Save();

				var pickWithBuildingStatus2 = Helper.CreatePickNew(order_WithShortfallAndAllFulfillment2);
				AssertEquals("Precondition", PickStatus.Codes.Building, pickWithBuildingStatus2.WP_PickStatus);
				Factory.Save();

				var linesPicker = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
				var result = linesPicker.GetOrder(null, false);
				AssertEquals("Order from only non-building pick.", order_WithoutShortfall, result);

				var orderBuilding1 = linesPicker.GetOrder(null, false, "2");
				var orderBuilding2 = linesPicker.GetOrder(null, false, "3");

				if (!regEnabled)
				{
					AssertEquals("OrderID passed in is Building, therefore should be returned.", order_WithShortfallAndAllFulfillment1, orderBuilding1);
					AssertEquals("OrderID passed in is Building, therefore should be returned.", order_WithShortfallAndAllFulfillment2, orderBuilding2);
				}
				else
				{
					AssertNull("OrderID passed in is Building, therefore should NOT be returned.", orderBuilding1);
					AssertNull("OrderID passed in is Building, therefore should NOT be returned.", orderBuilding2);
				}
			}
		}

		public void TestGetOrder_OrderExcludedFromTotePicking()
		{
			TestGetOrder_OrderExcludedFromTotePickingCore(null);
		}

		public void TestGetOrder_OrderExcludedFromTotePicking_WithOrderId()
		{
			TestGetOrder_OrderExcludedFromTotePickingCore("O1");
		}

		void TestGetOrder_OrderExcludedFromTotePickingCore(string orderId)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("AAA", "A.A");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_ExcludeFromTotePicking = true;
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			order.WD_PickPriority = 4;

			Helper.CreatePickNew(new[] { order });
			Helper.Factory.Save();

			var linesPicker = new PickLinesLoader(Helper.Factory, data.Whs1, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			AssertNull("No order is returned.", linesPicker.GetOrder(null, false, orderId));
		}

		public void TestGetOrder_PickByBOM()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			var staff1 = Helper.CreateGlbStaff("AAA", "A.A");

			Helper.Factory.Save();

			var kitOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var kitOrderLine1 = Helper.CreateWhsOrderLine(kitOrder, bike, 1m);
			var pick = Helper.CreatePickNew(kitOrder);
			var wheelOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines.Single();
			pick.WP_CartoniseSplitCases = false;
			wheelOrderLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
			Helper.Factory.Save();

			var linesPicker = new PickLinesLoader(Helper.Factory, data.Whs1, staff1, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" });
			var result = linesPicker.GetOrder(null, false);
			AssertEquals("O1 returned.", kitOrder, result);
		}

		public void TestGetOrder_PickWithInventoryAtDockDoor_DisallowPickWithInventoriesAtDockdoor() => TestGetOrder_PickWithInventoryAtDockDoorCore(disallowPickWithInventoriesAtDockdoor: true);
		public void TestGetOrder_PickWithInventoryAtDockDoor_AllowPickWithInventoriesAtDockdoor() => TestGetOrder_PickWithInventoryAtDockDoorCore(disallowPickWithInventoriesAtDockdoor: false);

		void TestGetOrder_PickWithInventoryAtDockDoorCore(bool disallowPickWithInventoriesAtDockdoor)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 1m);
			Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var pickLine = orderLine2.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			Factory.Save();

			var result = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo()).GetOrder(data.Whs1.DefaultOutboundDockDoorLocation.PK, disallowWithInventoriesAtDockdoor: disallowPickWithInventoriesAtDockdoor);
			if (disallowPickWithInventoriesAtDockdoor)
			{
				AssertNull(result);
			}
			else
			{
				AssertNotNull(result);
				AssertEquals(result.PK, order1.PK);
			}
		}

		public void TestGetOrder_PickWithInventoryAtDockDoor_WithOtherPicks()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 1m);
			Helper.CreatePickNew(order1, order2);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			Helper.CreateWhsOrderLine(order3, data.Part1, 1m);
			Helper.CreatePickNew(order3);
			Factory.Save();

			var pickLine = orderLine2.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			Factory.Save();

			var result = new PickLinesLoader(Factory, data.Whs1, staff, new SearchFilterCriteriaInfo()).GetOrder(data.Whs1.DefaultOutboundDockDoorLocation.PK, disallowWithInventoriesAtDockdoor: true);
			AssertNotNull(result);
			AssertEquals(result.PK, order3.PK);
		}

		#endregion

		#region Implementation

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		void TestByPickOrOrderReference(ZString reference1, ZString reference2, ZString reference3, ZString reference4, ZString reference5)
		{
			var operator1 = Factory.NewWithValidTestData<GlbStaff>();
			var operator2 = Factory.NewWithValidTestData<GlbStaff>();
			operator1.GS_Code = "OP1";
			operator2.GS_Code = "OP2";

			var location1Whs1 = Warehouse1.Rows.Single(r => r.WR_Name == "Row11").Locations;
			var location2Whs1 = Warehouse1.Rows.Single(r => r.WR_Name == "Row12").Locations;
			var location1Whs2 = Warehouse2.Rows.Single(r => r.WR_Name == "Row2").Locations;

			var result1 = new PickLinesLoader(Factory, Warehouse1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(reference1);
			AssertNotNull(result1.Pick);
			AssertAssignedPickAndLines(result1, Pick1, location1Whs1[0], location1Whs1[1], location1Whs1[2], location2Whs1[0], location2Whs1[1], location2Whs1[2]);

			var result2 = new PickLinesLoader(Factory, Warehouse1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(reference2);
			AssertNull(result2.Pick);
			AssertEquals(0, result2.PickLines.Count());

			var result3 = new PickLinesLoader(Factory, Warehouse2, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(reference2);
			AssertNotNull(result3.Pick);
			AssertAssignedPickAndLines(result3, Pick2, location1Whs2[0], location1Whs2[1], location1Whs2[2]);

			var result4 = new PickLinesLoader(Factory, Warehouse1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(reference3);
			AssertNotNull(result4.Pick);
			AssertAssignedPickAndLines(result4, Pick3, location1Whs1[0], location1Whs1[1], location1Whs1[2], location2Whs1[0], location2Whs1[1], location2Whs1[2]);

			Pick1.FinaliseAllOrders();
			AssertEquals(true, Pick1.GetFinalisableStatus().IsFinalisable);
			Pick1.FinalisePick();
			AssertEquals(true, Pick1.IsFinalised);
			Factory.Save();

			var result5 = new PickLinesLoader(Factory, Warehouse1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(reference1);
			AssertNull(result5.Pick);
			AssertEquals(0, result5.PickLines.Count());

			var result6 = new PickLinesLoader(Factory, Warehouse2, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(reference2);
			AssertNotNull(result6.Pick);
			AssertAssignedPickAndLines(result6, Pick2, location1Whs2[0], location1Whs2[1], location1Whs2[2]);

			var result7 = new PickLinesLoader(Factory, Warehouse1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }).FindAndAssignNextPick(reference3);
			AssertNotNull(result7.Pick);
			AssertAssignedPickAndLines(result7, Pick3, location1Whs1[0], location1Whs1[1], location1Whs1[2], location2Whs1[0], location2Whs1[1], location2Whs1[2]);

			SetupPickLine(Pick2, 2, 0, "PM2", operator2, ZDateTimeOffset.Empty);
			SetupPickLine(Pick3, 2, 0, "PM2", operator2, ZDateTimeOffset.Empty);
			Factory.Save();

			var result8 = new PickLinesLoader(Factory, Warehouse1, operator2, new SearchFilterCriteriaInfo { AreaCode = "A1", PickMethod = "PM2" }).FindAndAssignNextPick(reference1);
			AssertNull(result8.Pick);
			AssertEquals(0, result8.PickLines.Count());

			var result9 = new PickLinesLoader(Factory, Warehouse1, operator2, new SearchFilterCriteriaInfo { AreaCode = "A2", PickMethod = "PM2" }).FindAndAssignNextPick(reference1);
			AssertNull(result9.Pick);
			AssertEquals(0, result9.PickLines.Count());

			var result10 = new PickLinesLoader(Factory, Warehouse2, operator2, new SearchFilterCriteriaInfo { AreaCode = "A1", PickMethod = "PM2" }).FindAndAssignNextPick(reference2);
			AssertNotNull(result10.Pick);
			AssertAssignedPickAndLines(result10, Pick2, location1Whs2[0]);

			var result11 = new PickLinesLoader(Factory, Warehouse2, operator2, new SearchFilterCriteriaInfo { AreaCode = "A2", PickMethod = "PM2" }).FindAndAssignNextPick(reference2);
			AssertNull(result11.Pick);
			AssertEquals(0, result11.PickLines.Count());

			var result12 = new PickLinesLoader(Factory, Warehouse1, operator2, new SearchFilterCriteriaInfo { AreaCode = "A1", PickMethod = "PM2" }).FindAndAssignNextPick(reference3);
			AssertNotNull(result12.Pick);
			AssertAssignedPickAndLines(result12, Pick3, location1Whs1[0]);

			var result13 = new PickLinesLoader(Factory, Warehouse1, operator2, new SearchFilterCriteriaInfo { AreaCode = "A2", PickMethod = "PM2" }).FindAndAssignNextPick(reference3);
			AssertNull(result13.Pick);
			AssertEquals(0, result13.PickLines.Count());

			var pickGroupCollection = new PickGroupCollection();
			var pickGroup = pickGroupCollection.AddNew();
			pickGroup.Description = (NoResString)"Desc";

			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pickGroupCollection))
			{
				var data = new TestDataSimpleEnvironment(Factory, 1, 2);
				var location1 = data.Whs1.FindLocation("A-1-1");
				var location2 = data.Whs1.FindLocation("A-1-2");
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "");
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, location2, "");
				Factory.Save();

				var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				order1.WD_ExternalReference = "OrderWithNoPickGroup";
				order1.WD_DocketID = "IDNoPickGroup";
				var referenceOnOrder1 = order1.References.AddNew();
				referenceOnOrder1.WX_RefType = referenceOnOrder1.Lookups.ReferenceTypes[0].Code;
				referenceOnOrder1.WX_Reference = "Order Ref4";
				var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 4m);

				var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
				order2.WD_ExternalReference = "OrderWithOnePickGroup";
				order2.WD_DocketID = "IDOnePickGroup";
				var referenceOnOrder2 = order2.References.AddNew();
				referenceOnOrder2.WX_RefType = referenceOnOrder2.Lookups.ReferenceTypes[0].Code;
				referenceOnOrder2.WX_Reference = "Order Ref5";
				var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 6m);
				var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part2, 10m, pickGroup: 1);

				var pickWithNoPickGroups = Helper.CreatePickNew(order1);
				pickWithNoPickGroups.WP_PickNo = "NoPickGroup";
				var pickWithOnePickGroup = Helper.CreatePickNew(order2);
				pickWithOnePickGroup.WP_PickNo = "OnePickGroup";

				Factory.Save();

				var linesLoader1 = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" });
				var result14 = linesLoader1.FindAndAssignNextPick(reference4);
				AssertNotNull(result14.Pick);
				AssertAssignedPickAndLines(result14, pickWithNoPickGroups, location1);

				var result15 = linesLoader1.FindAndAssignNextPick(reference5);
				AssertNotNull(result15.Pick);
				AssertAssignedPickAndLines(result15, pickWithOnePickGroup, location2, location1);

				// clean-up
				orderLine1.PickLines[0].WZ_GS_NKAssignedTo = "";
				orderLine2.PickLines[0].WZ_GS_NKAssignedTo = "";
				orderLine3.PickLines[0].WZ_GS_NKAssignedTo = "";
				orderLine1.PickLines[0].WZ_IsPicking = false;
				orderLine2.PickLines[0].WZ_IsPicking = false;
				orderLine3.PickLines[0].WZ_IsPicking = false;

				Factory.Save();

				var linesLoader2 = new PickLinesLoader(Factory, data.Whs1, operator1, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 1 });
				var result16 = linesLoader2.FindAndAssignNextPick(reference4);
				AssertNull(result16.Pick);

				var result17 = linesLoader2.FindAndAssignNextPick(reference5);
				AssertNotNull(result17.Pick);
				AssertAssignedPickAndLines(result17, pickWithOnePickGroup, location2);
			}
		}

		void AssertAssignedPickAndLines(FindPickResult actualResult, WhsPick expectedPick, params WhsLocation[] locations)
		{
			AssertEquals(expectedPick, actualResult.Pick);
			int expectedLinesCount = 0;
			var pickLines = actualResult.PickLines.ToArray();
			foreach (var location in locations)
			{
				AssertEquals("Wrong location on line " + expectedLinesCount, location.PK, pickLines[expectedLinesCount++].Inventory.Location.PK);
			}
			AssertEquals("Pick contains wrong amount of lines", expectedLinesCount, pickLines.Length);
		}

		void SetupPickLine(WhsPick pick, ZInt lineIndex, ZInt inventoryIndex, ZString pickMethod, GlbStaff assignedTo, ZDateTimeOffset pickedDate)
		{
			var availableInventories = pick.OrderedInventories[lineIndex].AvailableInventories[inventoryIndex];
			availableInventories.Location.WLV_PickMethod = pickMethod;
			if (assignedTo == null)
			{
				var picklines = availableInventories.PickLines;
				foreach (WhsPickLine pickline in picklines)
				{
					pickline.WZ_GS_NKAssignedTo = "";
					pickline.WZ_IsPicking = false;
				}
			}
			else
			{
				Helper.SetAssignToPk(availableInventories, assignedTo.PK);
			}

			// suspend trigger to allow old test to incorrectly test multiple cases in single test with same data.
			using (SuspendTrigger("TG_PreventReassigningPickLine", WhsPickLineSchema.Constants.TableName))
			{
				Factory.Save();
			}

			Helper.SetPickedDate(availableInventories, pickedDate);
		}

		IDisposable SuspendTrigger(string triggerName, string tableName)
		{
			return new DisposableAction(
				() => Db.Connection.ExecuteNonQuery($"DISABLE TRIGGER {triggerName} ON {tableName}"),
				() => Db.Connection.ExecuteNonQuery($"ENABLE TRIGGER {triggerName} ON {tableName}"));
		}

		protected void SetupEnvironmentData()
		{
			Warehouse1 = Helper.CreateWarehouse("Whs1");
			var whs1Area1 = Helper.CreateArea(Warehouse1, "A1", AreaTypes.Codes.FreeStore);
			var whs1Area2 = Helper.CreateArea(Warehouse1, "A2", AreaTypes.Codes.FreeStore);

			var whs1Locations1 = Helper.CreateRowAndGenerateLocations(Warehouse1, "Row11", 3, 1).Locations;
			var whs1Locations2 = Helper.CreateRowAndGenerateLocations(Warehouse1, "Row12", 3, 1).Locations;

			whs1Locations1[0].WLV_WA_PickingArea = whs1Area1.PK;
			whs1Locations1[1].WLV_WA_PickingArea = whs1Area1.PK;
			whs1Locations1[2].WLV_WA_PickingArea = whs1Area1.PK;

			whs1Locations2[0].WLV_WA_PickingArea = whs1Area2.PK;
			whs1Locations2[1].WLV_WA_PickingArea = whs1Area2.PK;
			whs1Locations2[2].WLV_WA_PickingArea = whs1Area2.PK;

			Warehouse2 = Helper.CreateWarehouse("Whs2");
			var whs2Area1 = Helper.CreateArea(Warehouse2, "A1", AreaTypes.Codes.FreeStore);
			var whs2Area2 = Helper.CreateArea(Warehouse2, "A2", AreaTypes.Codes.FreeStore);
			var whs2Area3 = Helper.CreateArea(Warehouse2, "A3", AreaTypes.Codes.FreeStore);

			var locations1Whs2 = Helper.CreateRowAndGenerateLocations(Warehouse2, "Row2", 3, 1).Locations;
			locations1Whs2[0].WLV_WA_PickingArea = whs2Area1.PK;
			locations1Whs2[1].WLV_WA_PickingArea = whs2Area2.PK;
			locations1Whs2[2].WLV_WA_PickingArea = whs2Area3.PK;

			OrgHeader client = Helper.CreateClient("Cient");

			OrgSupplierPart part1 = Helper.CreateProduct(client, "Part1");
			OrgSupplierPart part2 = Helper.CreateProduct(client, "Part2");
			OrgSupplierPart part3 = Helper.CreateProduct(client, "Part3");

			WhsReceive receive1 = Helper.CreateWhsReceive(client, Warehouse1, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, part1, 100m);
			Helper.CreateWhsReceiveInventoryLine(receive1, part2, 100m);
			Helper.CreateWhsReceiveInventoryLine(receive1, part3, 100m);

			WhsReceive receive2 = Helper.CreateWhsReceive(client, Warehouse2, "Receive2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, part1, 200m);
			Helper.CreateWhsReceiveInventoryLine(receive2, part2, 200m);
			Helper.CreateWhsReceiveInventoryLine(receive2, part3, 200m);

			WhsReceive receive3 = Helper.CreateWhsReceive(client, Warehouse1, "Receive3", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive3, part1, 300m);
			Helper.CreateWhsReceiveInventoryLine(receive3, part2, 300m);
			Helper.CreateWhsReceiveInventoryLine(receive3, part3, 300m);

			receive1.Inventory[0].WI_WL = whs1Locations1[2].PK;
			receive1.Inventory[1].WI_WL = whs1Locations1[1].PK;
			receive1.Inventory[2].WI_WL = whs1Locations1[0].PK;
			receive1.FinaliseDocket();
			AssertEquals(true, receive1.IsFinalised);

			receive2.Inventory[0].WI_WL = locations1Whs2[2].PK;
			receive2.Inventory[1].WI_WL = locations1Whs2[1].PK;
			receive2.Inventory[2].WI_WL = locations1Whs2[0].PK;
			receive2.FinaliseDocket();
			AssertEquals(true, receive2.IsFinalised);

			receive3.Inventory[0].WI_WL = whs1Locations2[2].PK;
			receive3.Inventory[1].WI_WL = whs1Locations2[1].PK;
			receive3.Inventory[2].WI_WL = whs1Locations2[0].PK;
			receive3.FinaliseDocket();
			AssertEquals(true, receive3.IsFinalised);

			WhsOrder order1 = Helper.CreateWhsOrder(client, Warehouse1, "Order1", Notify);
			Helper.CreateWhsOrderLine(order1, part1, 10m);
			Helper.CreateWhsOrderLine(order1, part2, 10m);
			Helper.CreateWhsOrderLine(order1, part3, 10m);
			order1.References.AddNew();
			order1.References[0].WX_RefType = order1.References[0].Lookups.ReferenceTypes[0].Code;
			order1.References[0].WX_Reference = "Order Ref1";

			WhsOrder order2 = Helper.CreateWhsOrder(client, Warehouse2, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, part1, 20m);
			Helper.CreateWhsOrderLine(order2, part2, 20m);
			Helper.CreateWhsOrderLine(order2, part3, 20m);
			order2.References.AddNew();
			order2.References[0].WX_RefType = order2.References[0].Lookups.ReferenceTypes[0].Code;
			order2.References[0].WX_Reference = "Order Ref2";

			WhsOrder order3 = Helper.CreateWhsOrder(client, Warehouse1, "Order3", Notify);
			Helper.CreateWhsOrderLine(order3, part1, 30m);
			Helper.CreateWhsOrderLine(order3, part2, 30m);
			Helper.CreateWhsOrderLine(order3, part3, 30m);
			order3.References.AddNew();
			order3.References[0].WX_RefType = order3.References[0].Lookups.ReferenceTypes[0].Code;
			order3.References[0].WX_Reference = "Order Ref3";
			Factory.Save();

			Pick1 = Helper.CreatePickNew(order1);
			Pick1.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 5;
			Pick1.OrderedInventories[0].AvailableInventories[1].PickLineQuantity = 5;
			Pick1.OrderedInventories[1].AvailableInventories[0].PickLineQuantity = 5;
			Pick1.OrderedInventories[1].AvailableInventories[1].PickLineQuantity = 5;
			Pick1.OrderedInventories[2].AvailableInventories[0].PickLineQuantity = 5;
			Pick1.OrderedInventories[2].AvailableInventories[1].PickLineQuantity = 5;

			Pick2 = Helper.CreatePickNew(order2);
			Pick2.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 20;
			Pick2.OrderedInventories[1].AvailableInventories[0].PickLineQuantity = 20;
			Pick2.OrderedInventories[2].AvailableInventories[0].PickLineQuantity = 20;

			Pick3 = Helper.CreatePickNew(order3);
			Pick3.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 10;
			Pick3.OrderedInventories[0].AvailableInventories[1].PickLineQuantity = 20;
			Pick3.OrderedInventories[1].AvailableInventories[0].PickLineQuantity = 10;
			Pick3.OrderedInventories[1].AvailableInventories[1].PickLineQuantity = 20;
			Pick3.OrderedInventories[2].AvailableInventories[0].PickLineQuantity = 10;
			Pick3.OrderedInventories[2].AvailableInventories[1].PickLineQuantity = 20;

			Factory.Save();
		}

		WhsInventoryView SetInventory(WhsReceive receive, ZDate expiryDate, ZDate packingDate, string partAttrib1, string partAttrib2, string partAttrib3, string serialNumber, ZGuid location, ZGuid part, string palletID = "", Decimal qty = 10m)
		{
			var line = Helper.CreateWhsReceiveInventoryLine(receive, part, qty);
			line.SetAttributes(new TestILineAttributes("", expiryDate, packingDate, partAttrib1, partAttrib2, partAttrib3, serialNumber));
			line.WI_WL = location;
			line.WI_PalletID = palletID;

			return line;
		}

		WhsWarehouse Warehouse1;
		WhsWarehouse Warehouse2;
		WhsPick Pick1;
		WhsPick Pick2;
		WhsPick Pick3;

		#endregion
	}
}
