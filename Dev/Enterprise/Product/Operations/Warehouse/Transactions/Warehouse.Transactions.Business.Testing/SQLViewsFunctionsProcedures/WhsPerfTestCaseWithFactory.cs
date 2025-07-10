using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	#region WhsPerfTestCaseWithFactory

	abstract class WhsPerfTestCaseWithFactory : WhsTestCaseWithFactory
	{
		protected WhsReceive CreateWhsReceive(OrgHeader client, WhsWarehouse warehouse, ZString reference,
			OrgSupplierPart part, WhsLocation location, ZDecimal units, bool finaliseReceive)
		{
			var receive = Helper.CreateWhsReceive(client, warehouse, reference, Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, units);
			inventory.WI_WL = location.PK;

			if (finaliseReceive)
			{
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
			}

			Factory.Save();
			return receive;
		}

		protected WhsOrder CreateWhsOrder(OrgHeader client, WhsWarehouse warehouse, string reference,
			OrgSupplierPart part, ZDecimal units, bool finaliseOrder, bool finalisePick)
		{
			var order = Helper.CreateWhsOrder(client, warehouse, reference, Notify);
			Helper.CreateWhsOrderLine(order, part, units);
			var pick = Helper.CreatePickNew(order);

			if (finaliseOrder)
			{
				pick.FinaliseAllOrders();
				AssertIsFinalisedPrecondition(order);

				if (finalisePick)
				{
					pick.FinalisePick();
					AssertIsFinalisedPrecondition(pick);
				}
			}
			else if (finalisePick)
			{
				throw new ArgumentException("The pick job cant be finalised without finalising the order.");
			}

			Factory.Save();
			return order;
		}

		#region Setup

		protected void SetupData(bool withWorkingHours, bool withReceives)
		{
			Warehouse1 = Helper.CreateWarehouse("WHS1", "A", 5, 1);
			using (WarehouseDataRegistry.Instance.AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture
					   .SetTemporaryValue(Guid.Empty, Warehouse1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true))
			{
				if (withWorkingHours)
				{
					Warehouse1.WorkTimes.MondayWorkingHours = "                 *******************";
					Warehouse1.WorkTimes.TuesdayWorkingHours = "                 *******************";
					Warehouse1.WorkTimes.WednesdayWorkingHours = "                 *******************";
					Warehouse1.WorkTimes.ThursdayWorkingHours = "                 *******************";
					Warehouse1.WorkTimes.FridayWorkingHours = "                 *******************";
					Warehouse1.WorkTimes.SaturdayWorkingHours = "           ";
					Warehouse1.WorkTimes.SundayWorkingHours = "           ";
				}

				Warehouse2 = Helper.CreateWarehouse("WHS2", "B", 5, 1);

				var locations1 = Warehouse1.Rows.Single(r => r.WR_Name == "A").Locations;
				var locations2 = Warehouse2.Rows.Single(r => r.WR_Name == "B").Locations;

				Client1 = Helper.CreateClient("C1");
				Client2 = Helper.CreateClient("C2");

				Part = Helper.CreateProduct(Client1, "P1");

				if (withReceives)
				{
					CreateWhsReceive(Client1, Warehouse1, "R1", Part, locations1[0], 100m, true);
					CreateWhsReceive(Client1, Warehouse1, "R2", Part, locations1[1], 100m, true);
					CreateWhsReceive(Client1, Warehouse2, "R3", Part, locations2[0], 100m, true);
				}

				SetupDataCore();

				Factory.Save();
			}
		}

		protected virtual void SetupDataCore()
		{
		}

		#endregion

		#region LoadSQLFunction

		protected virtual string GetSqlFunctionName()
		{
			return string.Empty;
		}

		protected virtual bool IsPercentage()
		{
			return false;
		}

		protected DynamicBusinessObjectCollection LoadSQLFunction(DateTime startDate, DateTime endDate,
			ZGuid warehousePk, ZGuid clientPk)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var funcName = GetSqlFunctionName();

			var percentage = IsPercentage() ? " * 100" : "";
			var sql =
				$"select convert(decimal(38,2), {GetColumnName(funcName)}{percentage}) as result from dbo.{funcName}(@startDate, @endDate, @warehousePk, @clientPk) as tableResult";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@startDate", startDate, WhsPerfTestHelper.DateTimeColumn);
			parameters.Add("@endDate", endDate, WhsPerfTestHelper.DateTimeColumn);
			parameters.Add("@warehousePk", warehousePk, WhsPerfTestHelper.PKColumn);
			parameters.Add("@clientPk", clientPk, WhsPerfTestHelper.GuidColumn);

			result.Load(sql, parameters);
			return result;
		}

		string GetColumnName(string funcName)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = $"SELECT name FROM sys.columns WHERE object_id=object_id('dbo.{funcName}')";

			result.Load(sql);
			return result[0]["name"].ToString();
		}

		#endregion

		#region AllocateQty

		protected void AllocateQty(WhsPick pick, decimal qty)
		{
			var orderedInventory = pick.OrderedInventories[0];
			foreach (WhsPickAvailableInventory availableInventory in orderedInventory.AvailableInventories)
			{
				var qtyToAllocate = Math.Min(availableInventory.QuantityAvailableToPick, qty);
				availableInventory.PickLineQuantity = qtyToAllocate;
				qty -= qtyToAllocate;
				if (qty <= 0)
				{
					break;
				}
			}
		}

		#endregion

		#region Implementation

		protected WhsWarehouse Warehouse1;
		protected WhsWarehouse Warehouse2;
		protected OrgHeader Client1;
		protected OrgHeader Client2;
		protected OrgSupplierPart Part;

		#endregion
	}

	#endregion

	#region WhsPerfGetNumberOfWorkingHoursBetweenDates

	class WhsPerfGetNumberOfWorkingHoursBetweenDatesTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 16)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: false);

			// 2 consecutive days
			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 25), new DateTime(2012, 06, 27));
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(19), functionLoadResults1[0]["result"]);

			// 1 full week with not work during the week end
			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 18), new DateTime(2012, 06, 25));
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(47.50), functionLoadResults2[0]["result"]);

			// 4 consecutive days with one day of holiday
			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 25), new DateTime(2012, 06, 29));
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(28.50), functionLoadResults3[0]["result"]);
		}

		#endregion

		#region SetupDataCore

		protected override void SetupDataCore()
		{
			Warehouse1.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;

			var branch = Warehouse1.RelatedCompanyBranch;
			var holiday = branch.GlbHolidays.AddNew();
			holiday.GH_Date = new DateTime(2012, 06, 28);
		}

		#endregion

		#region LoadSQLFunction

		DynamicBusinessObjectCollection LoadSQLFunction(DateTime startDate, DateTime endDate)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql =
				@"select convert(decimal(38,2), NumberOfWorkingHours) as result from dbo.WhsPerfGetNumberOfWorkingHoursBetweenDates(@startDate, @endDate, @parentId, @branchId) as tableResult";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@startDate", startDate, WhsPerfTestHelper.DateTimeColumn);
			parameters.Add("@endDate", endDate, WhsPerfTestHelper.DateTimeColumn);
			parameters.Add("@parentId", Warehouse1.PK, WhsPerfTestHelper.PKColumn);
			parameters.Add("@branchId", Warehouse1.WW_GB_RelatedCompanyBranch, WhsPerfTestHelper.GuidColumn);

			result.Load(sql, parameters);
			return result;
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetNumberOfWorkingHoursInDayFromDateTimeTest

	class WhsPerfGetNumberOfWorkingHoursInDayFromDateTimeTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 06, 20)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: false);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 25), true); // Monday
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(9.5), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 25, 10, 0, 0), true);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(8), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 25, 10, 0, 0), false);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(1.50), functionLoadResults3[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			Warehouse1.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;

			var branch = Warehouse1.RelatedCompanyBranch;
			var holiday = branch.GlbHolidays.AddNew();
			holiday.GH_Date = new DateTime(2012, 06, 28);
			Factory.Save();
		}

		#endregion

		#region LoadSQLFunction

		DynamicBusinessObjectCollection LoadSQLFunction(DateTime date, bool isStart)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql =
				@"select convert(decimal(38,2), NumberOfWorkingHours) as result from dbo.WhsPerfGetNumberOfWorkingHoursInDayFromDateTime(@date, @isStart, @parentId, @branchId) as tableResult";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@date", date, WhsPerfTestHelper.DateTimeColumn);
			parameters.Add("@isStart", (byte)(isStart ? 1 : 0), WhsPerfTestHelper.ByteColumn);
			parameters.Add("@parentId", Warehouse1.PK, WhsPerfTestHelper.PKColumn);
			parameters.Add("@branchId", Warehouse1.WW_GB_RelatedCompanyBranch, WhsPerfTestHelper.GuidColumn);

			result.Load(sql, parameters);
			return result;
		}

		#endregion
	}

	#endregion

	#region WhsPerfIsInWorkingHoursTest

	class WhsPerfIsInWorkingHoursTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 20)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: false);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 25));
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(ZBool.False, functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 25, 10, 0, 0));
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(ZBool.True, functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 28, 10, 0, 0));
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(ZBool.False, functionLoadResults3[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			Warehouse1.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;

			var branch = Warehouse1.RelatedCompanyBranch;
			var holiday = branch.GlbHolidays.AddNew();
			holiday.GH_Date = new DateTime(2012, 06, 28);
		}

		#endregion

		#region LoadSQLFunction

		DynamicBusinessObjectCollection LoadSQLFunction(DateTime date)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql =
				@"select IsInWorkingHours as result from dbo.WhsPerfIsInWorkingHours(@date, @parentId, @branchId) as tableResult";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@date", date, WhsPerfTestHelper.DateTimeColumn);
			parameters.Add("@parentId", Warehouse1.PK, WhsPerfTestHelper.PKColumn);
			parameters.Add("@branchId", Warehouse1.WW_GB_RelatedCompanyBranch, WhsPerfTestHelper.GuidColumn);

			result.Load(sql, parameters);
			return result;
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetPercentageOfOnTimeReadyToShip

	class WhsPerfGetPercentageOfOnTimeReadyToShipTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		public void TestFunction()
		{
			SetupData(withWorkingHours: false, withReceives: true);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 05, 25), new DateTime(2012, 06, 25),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(60.00), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 05, 25), new DateTime(2012, 06, 25),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 05, 25), new DateTime(2012, 06, 25),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 04, 25), new DateTime(2012, 05, 25),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			for (var i = 0; i < 10; i++)
			{
				var order = CreateWhsOrder(Client1, Warehouse1, string.Format("OR{0}", i), Part, 1m, true, false);
				order.WD_RequiredDate = new ZDateTimeOffset(2012, 06, 20);
				order.WD_FinalisedDate = i < 6 ? new ZDateTimeOffset(2012, 06, 19) : new ZDateTimeOffset(2012, 06, 21);
				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetPercentageOfOnTimeReadyToShip";
		}

		protected override bool IsPercentage()
		{
			return true;
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetOrderCycleTimesEnteredToReleased

	class WhsPerfGetOrderCycleTimesEnteredToReleasedTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: true);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(19.00), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			for (var i = 0; i < 12; i++)
			{
				var order = CreateWhsOrder(Client1, Warehouse1, string.Format("OR{0}", i), Part, 1m, false, false);
				var entered = new DateTime(2012, 6, 25);
				var pickedUp = i < 4 ? new DateTime(2012, 6, 26) :
					i < 8 ? new DateTime(2012, 6, 27) : new DateTime(2012, 6, 28);
				var tasks = order.WorkflowItems.Cast<ProcessTask>();
				tasks.First(t => t.P9_SE_NKMilestoneEvent == Events.WarehouseJobEntered.Code)
					.SetMilestoneActualDateForTest(entered);
				tasks.First(t => t.P9_SE_NKMilestoneEvent == Events.PickedUp.Code)
					.SetMilestoneActualDateForTest(pickedUp);

				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetOrderCycleTimesEnteredToReleased";
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetOrderCycleTimesPickedToFinalized

	class WhsPerfGetOrderCycleTimesPickedToFinalizedTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: true);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(19.00), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			for (var i = 0; i < 12; i++)
			{
				var order = CreateWhsOrder(Client1, Warehouse1, string.Format("OR{0}", i), Part, 1m, false, false);
				var picked = new DateTime(2012, 6, 25);
				var finalized = i < 4 ? new DateTime(2012, 6, 26) :
					i < 8 ? new DateTime(2012, 6, 27) : new DateTime(2012, 6, 28);
				var tasks = order.WorkflowItems.Cast<ProcessTask>();
				tasks.First(t => t.P9_SE_NKMilestoneEvent == Events.WarehouseOrderPicking.Code)
					.SetMilestoneActualDateForTest(picked);
				tasks.First(t => t.P9_SE_NKMilestoneEvent == Events.ItemDocumentJobFinalised.Code)
					.SetMilestoneActualDateForTest(finalized);

				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetOrderCycleTimesPickedToFinalized";
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetOrderFillRate

	class WhsPerfGetOrderFillRateTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: false, withReceives: true);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(60.00), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region TestFunction_DoesntShowReserveLines

		public void TestFunction_DoesntShowReserveLines()
		{
			var today = ZDateTime.Today;

			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("1", "A", 1, 1);
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(client, whs, "R1", part, 10m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(client, whs, part, 10m);
			AssertNotNull("Precondition - divot is created.",
				order.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0]));
			Factory.Save();

			// Hack to test filter out lines with no Pick
			order.WD_DocketStatus = DocketStatus.Codes.Entered;
			Factory.Save();

			var functionLoadResults = LoadSQLFunction(today.AddDays(-1).ToDateTime(), today.AddDays(1).ToDateTime(),
				whs.PK, client.PK);
			AssertEquals(1, functionLoadResults.Count);
			AssertEquals("Should not include reserve lines", 0m, functionLoadResults[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			TestDateAttribute.Date = new DateTime(2012, 6, 1);
			Warehouse1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			using (WarehouseDataRegistry.Instance.AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture
					   .SetTemporaryValue(Guid.Empty, Warehouse1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true))
			{
				for (var i = 0; i < 10; i++)
				{
					var order = Helper.CreateWhsOrder(Client1.PK, Warehouse1.PK, Client1.PK, string.Format("OR{0}", i),
						new DateTime(2012, 6, 25), Notify, WhsPickOption.Codes.Manual);
					Helper.CreateWhsOrderLine(order, Part, 2m);
					Helper.CreateWhsOrderLine(order, Part, 3m);

					var pick = Helper.CreatePickNew(order);
					var qty = (i < 6) ? 5m // first 5 orders fully filled
						: (i < 8) ? 4m // order half filled (for orders 6-7)
						: 1m; // order not filled.
					AllocateQty(pick, qty);
					pick.FinaliseAllOrders();
					AssertIsFinalisedPrecondition(order);

					Factory.Save();
				}
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetOrderFillRate";
		}

		protected override bool IsPercentage()
		{
			return true;
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetOrderLineFillRate

	class WhsPerfGetOrderLineFillRateTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: true);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(66.67), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region TestFunction_DoesntShowReserveLines

		public void TestFunction_DoesntShowReserveLines()
		{
			var today = ZDateTime.Today;

			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS", "A", 1, 1);
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(client, whs, "R1", part, 10m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(client, whs, part, 10m);
			AssertNotNull("Precondition - divot is created.",
				order.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0]));
			Factory.Save();

			// Hack to test filter out lines with no Pick
			order.WD_DocketStatus = DocketStatus.Codes.Entered;
			Factory.Save();

			var functionLoadResults = LoadSQLFunction(today.AddDays(-1).ToDateTime(), today.AddDays(1).ToDateTime(),
				whs.PK, client.PK);
			AssertEquals(1, functionLoadResults.Count);
			AssertEquals("Should not include reserve lines", 0m, functionLoadResults[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			TestDateAttribute.Date = new DateTime(2012, 6, 1);
			Warehouse1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			for (var i = 0; i < 10; i++)
			{
				var order = Helper.CreateWhsOrder(Client1.PK, Warehouse1.PK, Client1.PK, string.Format("OR{0}", i),
					new DateTime(2012, 6, 25), Notify, WhsPickOption.Codes.Manual);
				Helper.CreateWhsOrderLine(order, Part, 2m);
				Helper.CreateWhsOrderLine(order, Part, 2m);
				Helper.CreateWhsOrderLine(order, Part, 2m);

				var pick = Helper.CreatePickNew(order);
				AllocateQty(pick, 5m);
				pick.FinaliseAllOrders();
				AssertIsFinalisedPrecondition(order);

				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetOrderLineFillRate";
		}

		protected override bool IsPercentage()
		{
			return true;
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetPercentageOfBackOrders

	class WhsPerfGetPercentageOfBackOrdersTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: false, withReceives: true);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(60.00), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			for (var i = 0; i < 10; i++)
			{
				TestDateAttribute.Date = new DateTime(2012, 6, 1);
				var order = Helper.CreateWhsOrder(Client1, Warehouse1, $"OR{i}", Notify);
				if (i < 6)
				{
					order.WD_DocketSubType = "BAK";
				}

				Helper.CreateWhsOrderLine(order, Part, 1m);
				Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);

				order.WD_FinalisedDate = new DateTime(2012, 6, 25);

				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetPercentageOfBackOrders";
		}

		protected override bool IsPercentage()
		{
			return true;
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetNumberOfOrdersEnteredPerHour

	class WhsPerfGetNumberOfOrdersEnteredPerHourTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: true);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 25), new DateTime(2012, 06, 26),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(0.53),
				functionLoadResults1[0][
					"result"]); // 5 orders out of working hours and 5 orders in 9.5 working hours = 0.53 order per hour

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			for (var i = 0; i < 10; i++)
			{
				if (i < 5)
				{
					// in working hours
					TestDateAttribute.Date = new DateTime(2012, 6, 25, 10 + i, 1, 0);
				}
				else
				{
					// out of working hours
					TestDateAttribute.Date = new DateTime(2012, 6, 25, 8, 1, 0);
				}

				CreateWhsOrder(Client1, Warehouse1, $"OR{i}", Part, 1m, false, false);
				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetNumberOfOrdersEnteredPerHour";
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetNumberOfOrdersSentToPickPerHour

	class WhsPerfGetNumberOfOrdersSentToPickPerHourTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: true);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 25), new DateTime(2012, 06, 26),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(0.53),
				functionLoadResults1[0][
					"result"]); // 5 orders out of working hours and 5 orders in 9.5 working hours = 0.53 order per hour

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			for (var i = 0; i < 10; i++)
			{
				TestDateAttribute.Date = new DateTime(2012, 6, 24);
				var order = CreateWhsOrder(Client1, Warehouse1, $"OR{i}", Part, 1m, false, false);

				if (i < 5)
				{
					// in working hours
					TestDateAttribute.Date = new DateTime(2012, 6, 25, 10 + i, 1, 0);
				}
				else
				{
					// out of working hours
					TestDateAttribute.Date = new DateTime(2012, 6, 25, 8, 1, 0);
				}

				foreach (ProcessTask task in order.WorkflowItems)
				{
					if (task.P9_SE_NKMilestoneEvent == Events.WarehouseOrderPicking.Code)
					{
						task.SetMilestoneActualDateForTest(ZDateTime.Now);
						break;
					}
				}

				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetNumberOfOrdersSentToPickPerHour";
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetNumberOfOrderLinesEnteredPerHour

	class WhsPerfGetNumberOfOrderLinesEnteredPerHourTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: true);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 25), new DateTime(2012, 06, 26),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(2.11), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			for (var i = 0; i < 10; i++)
			{
				TestDateAttribute.Date = new DateTime(2012, 6, 25);
				var order = Helper.CreateWhsOrder(Client1, Warehouse1, $"OR{i}", Notify);

				if (i < 5)
				{
					// in working hours
					TestDateAttribute.Date = new DateTime(2012, 6, 25, 10 + i, 1, 0);
				}
				else
				{
					// out of working hours
					TestDateAttribute.Date = new DateTime(2012, 6, 25, 8, 1, 0);
				}

				Helper.CreateWhsOrderLine(order, Part, 2m);
				Helper.CreateWhsOrderLine(order, Part, 2m);
				Helper.CreateWhsOrderLine(order, Part, 2m);
				Helper.CreateWhsOrderLine(order, Part, 2m);

				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetNumberOfOrderLinesEnteredPerHour";
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetNumberOfOrderLinesSentToPickPerHour

	class WhsPerfGetNumberOfOrderLinesSentToPickPerHourTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: true);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 25), new DateTime(2012, 06, 26),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(2.11), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			for (var i = 0; i < 10; i++)
			{
				TestDateAttribute.Date = new DateTime(2012, 6, 24);
				var order = Helper.CreateWhsOrder(Client1, Warehouse1, $"OR{i}", Notify);
				Helper.CreateWhsOrderLine(order, Part, 2m);
				Helper.CreateWhsOrderLine(order, Part, 2m);
				Helper.CreateWhsOrderLine(order, Part, 2m);
				Helper.CreateWhsOrderLine(order, Part, 2m);

				Factory.Save();

				if (i < 5)
				{
					// in working hours
					TestDateAttribute.Date = new DateTime(2012, 6, 25, 10 + i, 1, 0);
				}
				else
				{
					// out of working hours
					TestDateAttribute.Date = new DateTime(2012, 6, 25, 8, 1, 0);
				}

				foreach (ProcessTask task in order.WorkflowItems)
				{
					if (task.P9_SE_NKMilestoneEvent == Events.WarehouseOrderPicking.Code)
					{
						task.SetMilestoneActualDateForTest(ZDateTime.Now);
						break;
					}
				}

				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetNumberOfOrderLinesSentToPickPerHour";
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetDockToStockAverageTime

	class WhsPerfGetDockToStockAverageTimeTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: false);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(19.00), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			var locationA1 = Warehouse1.Rows.Single(r => r.WR_Name == "A").Locations
				.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1);
			for (var i = 0; i < 12; i++)
			{
				var receive = CreateWhsReceive(Client1, Warehouse1, $"R{i}", Part, locationA1, 100m, true);
				var arrived = new DateTime(2012, 6, 25);
				var finalized = i < 4 ? new DateTime(2012, 6, 26) :
					i < 8 ? new DateTime(2012, 6, 27) : new DateTime(2012, 6, 28);
				var tasks = receive.WorkflowItems.Cast<ProcessTask>();
				tasks.First(t => t.P9_SE_NKMilestoneEvent == Events.WarehouseReceiptArrived.Code)
					.SetMilestoneActualDateForTest(arrived);
				tasks.First(t => t.P9_SE_NKMilestoneEvent == Events.ItemDocumentJobFinalised.Code)
					.SetMilestoneActualDateForTest(finalized);

				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetDockToStockAverageTime";
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetPercentageOfEarlyReceipts

	class WhsPerfGetPercentageOfEarlyReceiptsTest : WhsPerfGetPercentageOfReceiptsTimingTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: false);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(41.67), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			var locationA1 = Warehouse1.Rows.Single(r => r.WR_Name == "A").Locations
				.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1);
			for (var i = 0; i < 12; i++)
			{
				var receive = CreateWhsReceive(Client1, Warehouse1, $"R{i}", Part, locationA1, 100m, true);
				var eta = new DateTime(2012, 6, 25, 10, 0, 0);
				var ata = i < 5 ? new DateTime(2012, 6, 25, 6, 0, 0) :
					i < 8 ? new DateTime(2012, 6, 25, 10, 0, 0) : new DateTime(2012, 6, 25, 14, 0, 0);
				var tasks = receive.WorkflowItems.Cast<ProcessTask>();
				tasks.First(t => t.P9_SE_NKMilestoneEvent == Events.WarehouseReceiptETANotification.Code)
					.SetMilestoneActualDateForTest(eta);
				tasks.First(t => t.P9_SE_NKMilestoneEvent == Events.WarehouseReceiptArrived.Code)
					.SetMilestoneActualDateForTest(ata);

				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetPercentageOfEarlyReceipts";
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetPercentageOfOnTimeReceipts

	class WhsPerfGetPercentageOfOnTimeReceiptsTest : WhsPerfGetPercentageOfReceiptsTimingTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: false);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(25.00), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			var locationA1 = Warehouse1.Rows.Single(r => r.WR_Name == "A").Locations
				.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1);
			for (var i = 0; i < 12; i++)
			{
				var receive = CreateWhsReceive(Client1, Warehouse1, $"R{i}", Part, locationA1, 100m, true);
				var eta = new DateTime(2012, 6, 25, 10, 0, 0);
				var ata = i < 5 ? new DateTime(2012, 6, 25, 6, 0, 0) :
					i < 8 ? new DateTime(2012, 6, 25, 10, 0, 0) : new DateTime(2012, 6, 25, 14, 0, 0);
				var tasks = receive.WorkflowItems.Cast<ProcessTask>();
				tasks.First(t => t.P9_SE_NKMilestoneEvent == Events.WarehouseReceiptETANotification.Code)
					.SetMilestoneActualDateForTest(eta);
				tasks.First(t => t.P9_SE_NKMilestoneEvent == Events.WarehouseReceiptArrived.Code)
					.SetMilestoneActualDateForTest(ata);

				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetPercentageOfOnTimeReceipts";
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetPercentageOfLateReceipts

	class WhsPerfGetPercentageOfLateReceiptsTest : WhsPerfGetPercentageOfReceiptsTimingTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: false);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(33.33), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetPercentageOfLateReceipts";
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetPercentageOfReceiptsTimingTestCaseWithFactory

	abstract class WhsPerfGetPercentageOfReceiptsTimingTestCaseWithFactory : WhsPerfTestCaseWithFactory
	{
		#region SetupData

		protected override void SetupDataCore()
		{
			var locationA1 = Warehouse1.Rows.Single(r => r.WR_Name == "A").Locations
				.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1);
			for (var i = 0; i < 12; i++)
			{
				var receive = CreateWhsReceive(Client1, Warehouse1, $"R{i}", Part, locationA1, 100m, true);
				var eta = new DateTime(2012, 6, 25, 10, 0, 0);
				var ata = i < 5 ? new DateTime(2012, 6, 25, 6, 0, 0) :
					i < 8 ? new DateTime(2012, 6, 25, 10, 0, 0) : new DateTime(2012, 6, 25, 14, 0, 0);
				var tasks = receive.WorkflowItems.Cast<ProcessTask>();
				tasks.First(t => t.P9_SE_NKMilestoneEvent == Events.WarehouseReceiptETANotification.Code)
					.SetMilestoneActualDateForTest(eta);
				tasks.First(t => t.P9_SE_NKMilestoneEvent == Events.WarehouseReceiptArrived.Code)
					.SetMilestoneActualDateForTest(ata);

				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override bool IsPercentage()
		{
			return true;
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetPercentageOfDamageFreeReceipts

	class WhsPerfGetPercentageOfDamageFreeReceiptsTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: false);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(60.00), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			var locationA1 = Warehouse1.Rows.Single(r => r.WR_Name == "A").Locations
				.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1);
			for (var i = 0; i < 10; i++)
			{
				TestDateAttribute.Date = new DateTime(2012, 6, 1);
				var receive = Helper.CreateWhsReceive(Client1, Warehouse1, $"R{i}", Notify);

				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, Part, 10m, locationA1);
				inventory.OriginalInventoryHeldCode =
					i < 6 ? InventoryHoldCodes.Codes.Held : InventoryHoldCodes.Codes.Damaged;

				receive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive);

				receive.FinaliseDocketWithoutUserConfirmation();
				receive.WD_FinalisedDate = new DateTime(2012, 6, 25, 10, 0, 0);

				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetPercentageOfDamageFreeReceipts";
		}

		protected override bool IsPercentage()
		{
			return true;
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetPercentageOfDamageFreeReceiptInventory

	class WhsPerfGetPercentageOfDamageFreeReceiptInventoryTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: false);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(40.00), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			var locationA1 = Warehouse1.Rows.Single(r => r.WR_Name == "A").Locations
				.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1);
			for (var i = 0; i < 10; i++)
			{
				TestDateAttribute.Date = new DateTime(2012, 6, 1);
				var receive = Helper.CreateWhsReceive(Client1, Warehouse1, $"R{i}", Notify);

				var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, Part, 10m, locationA1);
				inventory1.OriginalInventoryHeldCode =
					i < 4 ? InventoryHoldCodes.Codes.Held : InventoryHoldCodes.Codes.Damaged;

				var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, Part, 10m, locationA1);
				inventory3.OriginalInventoryHeldCode =
					i < 4 ? InventoryHoldCodes.Codes.Held : InventoryHoldCodes.Codes.Damaged;

				var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, Part, 10m, locationA1);
				inventory4.OriginalInventoryHeldCode =
					i < 4 ? InventoryHoldCodes.Codes.Held : InventoryHoldCodes.Codes.Damaged;

				receive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive);

				receive.WD_FinalisedDate = new DateTime(2012, 6, 25, 10, 0, 0);

				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetPercentageOfDamageFreeReceiptInventory";
		}

		protected override bool IsPercentage()
		{
			return true;
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetNumberOfReceivesEnteredPerHour

	class WhsPerfGetNumberOfReceivesEnteredPerHourTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: false);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 25), new DateTime(2012, 06, 26),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(0.53), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			var locationA1 = Warehouse1.Rows.Single(r => r.WR_Name == "A").Locations
				.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1);
			for (var i = 0; i < 10; i++)
			{
				if (i < 5)
				{
					// in working hours
					TestDateAttribute.Date = new DateTime(2012, 6, 25, 10 + i, 1, 0);
				}
				else
				{
					// out of working hours
					TestDateAttribute.Date = new DateTime(2012, 6, 25, 8, 1, 0);
				}

				var receive = CreateWhsReceive(Client1, Warehouse1, $"R{i}", Part, locationA1, 100m, true);

				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetNumberOfReceivesEnteredPerHour";
		}

		#endregion
	}

	#endregion

	#region WhsPerfGetNumberOfReceiveLinesEnteredPerHour

	class WhsPerfGetNumberOfReceiveLinesEnteredPerHourTest : WhsPerfTestCaseWithFactory
	{
		#region TestFunction

		[TestDate(2012, 6, 24)]
		public void TestFunction()
		{
			SetupData(withWorkingHours: true, withReceives: false);

			var functionLoadResults1 = LoadSQLFunction(new DateTime(2012, 06, 25), new DateTime(2012, 06, 26),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults1.Count);
			AssertEquals(new ZDecimal(2.11), functionLoadResults1[0]["result"]);

			var functionLoadResults2 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse1.PK, Client2.PK);
			AssertEquals(1, functionLoadResults2.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults2[0]["result"]);

			var functionLoadResults3 = LoadSQLFunction(new DateTime(2012, 06, 20), new DateTime(2012, 06, 27),
				Warehouse2.PK, Client1.PK);
			AssertEquals(1, functionLoadResults3.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults3[0]["result"]);

			var functionLoadResults4 = LoadSQLFunction(new DateTime(2012, 06, 27), new DateTime(2012, 06, 28),
				Warehouse1.PK, Client1.PK);
			AssertEquals(1, functionLoadResults4.Count);
			AssertEquals(new ZDecimal(0.00), functionLoadResults4[0]["result"]);
		}

		#endregion

		#region SetupData

		protected override void SetupDataCore()
		{
			for (var i = 0; i < 10; i++)
			{
				var receive = Helper.CreateWhsReceive(Client1, Warehouse1, $"R{i}", Notify);

				if (i < 5)
				{
					// in working hours
					TestDateAttribute.Date = new DateTime(2012, 6, 25, 10 + i, 1, 0);
				}
				else
				{
					// out of working hours
					TestDateAttribute.Date = new DateTime(2012, 6, 25, 8, 1, 0);
				}

				Helper.CreateWhsReceiveInventoryLine(receive, Part, 10m, Warehouse1.Rows[0].Locations[0]);
				Helper.CreateWhsReceiveInventoryLine(receive, Part, 10m, Warehouse1.Rows[0].Locations[0]);
				Helper.CreateWhsReceiveInventoryLine(receive, Part, 10m, Warehouse1.Rows[0].Locations[0]);
				Helper.CreateWhsReceiveInventoryLine(receive, Part, 10m, Warehouse1.Rows[0].Locations[0]);

				Factory.Save();
			}
		}

		#endregion

		#region LoadSQLFunction

		protected override string GetSqlFunctionName()
		{
			return "WhsPerfGetNumberOfReceiveLinesEnteredPerHour";
		}

		#endregion
	}

	#endregion

	#region WhsPerfTestHelper

	class WhsPerfTestHelper
	{
		public static SchemaDateTimeColumn DateTimeColumn => GlbHolidaySchema.GH_Date;
		public static SchemaByteColumn ByteColumn => OrgCompanyDataSchema.OB_WhsClientFreeStorageDays;
		public static SchemaPKColumn PKColumn => WhsWarehouseSchema.PK;
		public static SchemaGuidColumn GuidColumn => WhsWarehouseSchema.WW_GB_RelatedCompanyBranch;
	}

	#endregion
}
