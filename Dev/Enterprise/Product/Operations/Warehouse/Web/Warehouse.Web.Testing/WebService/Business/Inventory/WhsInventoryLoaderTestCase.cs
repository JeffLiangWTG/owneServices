using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsInventoryLoaderWhsTestCaseWithFactory : WhsTestCaseWithFactory
	{
		#region TestLoadWhsInventory_SearchJoinCondition

		public void TestLoadWhsInventory_SearchJoinConditionIsAnd_PalletIDAndLocationJoinConditionIsAnd()
		{
			TestLoadWhsInventory_SearchJoinConditionCore(SearchJoinCondition.And, SearchJoinCondition.And);
		}

		public void TestLoadWhsInventory_SearchJoinConditionIsAnd_PalletIDAndLocationJoinConditionIsOr()
		{
			TestLoadWhsInventory_SearchJoinConditionCore(SearchJoinCondition.And, SearchJoinCondition.Or);
		}

		public void TestLoadWhsInventory_SearchJoinConditionIsOr_PalletIDAndLocationJoinConditionIsAnd()
		{
			TestLoadWhsInventory_SearchJoinConditionCore(SearchJoinCondition.Or, SearchJoinCondition.And);
		}

		public void TestLoadWhsInventory_SearchJoinConditionIsOr_PalletIDAndLocationJoinConditionIsOr()
		{
			TestLoadWhsInventory_SearchJoinConditionCore(SearchJoinCondition.Or, SearchJoinCondition.Or);
		}

		void TestLoadWhsInventory_SearchJoinConditionCore(SearchJoinCondition condition, SearchJoinCondition palletIDAndLocationJoinCondition)
		{
			var today = ZDate.Today;
			var todayAsDateTime = today.ToDateTime();
			var tomorrow = today.AddDays(1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 1m, locationA1.PK, "P1", today, today, "A1", "A1", "E3", "SN1", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 1m, locationA1.PK, "P2", today, today, "A1", "A2", "A3", "SN2", "");
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2.PK, 1m, locationA1.PK, "P2", tomorrow, today, "A1", "A2", "E3", "SN3", "");
			var receiveLine4 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 1m, locationA2.PK, "P3", today, tomorrow, "A1", "B2", "B3", "SN4", "");
			var receiveLine5 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 1m, locationA2.PK, "P4", today, tomorrow, "A2", "A3", "A4", "SN5", "");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			var noMatchingInventory = Array.Empty<WhsInventoryView>();
			var criteria0 = new WhsInventorySearchCriteriaInfo("notexist", "", "", "", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, condition, palletIDAndLocationJoinCondition);
			var loader0 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader0.LoadWhsInventory(criteria0), receiveLine1, receiveLine2, receiveLine3, receiveLine4, receiveLine5);

			var criteria1 = new WhsInventorySearchCriteriaInfo("", "notexist", "", "", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, condition, palletIDAndLocationJoinCondition);
			var loader1 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader1.LoadWhsInventory(criteria1), receiveLine1, receiveLine2, receiveLine3, receiveLine4, receiveLine5);

			var criteria2 = new WhsInventorySearchCriteriaInfo("", "", "notexist", "", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, condition, palletIDAndLocationJoinCondition);
			var loader2 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader2.LoadWhsInventory(criteria2), noMatchingInventory);

			var criteria3 = new WhsInventorySearchCriteriaInfo("", "", "", "notexist", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, condition, palletIDAndLocationJoinCondition);
			var loader3 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader3.LoadWhsInventory(criteria3), noMatchingInventory);

			var criteria4 = new WhsInventorySearchCriteriaInfo("", "", "", "notexist", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, condition, palletIDAndLocationJoinCondition);
			var loader4 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader4.LoadWhsInventory(criteria4), noMatchingInventory);

			var criteria5 = new WhsInventorySearchCriteriaInfo("", "", "", "", "notexist", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level3, palletIDAndLocationJoinCondition);
			var loader5 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader5.LoadWhsInventory(criteria5), noMatchingInventory);

			var criteria6 = new WhsInventorySearchCriteriaInfo("", "", "", "", "", "notexist", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader6 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader6.LoadWhsInventory(criteria6), noMatchingInventory);

			var criteria7 = new WhsInventorySearchCriteriaInfo("", "", "", "", "", "", "notexist", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader7 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader7.LoadWhsInventory(criteria7), noMatchingInventory);

			var criteria8 = new WhsInventorySearchCriteriaInfo("", "", "", "", "", "", "", "", tomorrow.AddDays(1).ToDateTime(), new DateTime(), WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader8 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader8.LoadWhsInventory(criteria8), noMatchingInventory);

			var criteria9 = new WhsInventorySearchCriteriaInfo("", "", "", "", "", "", "", "", new DateTime(), tomorrow.AddDays(1).ToDateTime(), WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader9 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader9.LoadWhsInventory(criteria9), noMatchingInventory);

			var criteria10 = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part1.OP_PartNum, "", "", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, condition, palletIDAndLocationJoinCondition);
			var loader10 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader10.LoadWhsInventory(criteria10), receiveLine1, receiveLine2, receiveLine4, receiveLine5);

			var criteria11 = new WhsInventorySearchCriteriaInfo("", "", "A-1", "", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, condition, palletIDAndLocationJoinCondition);
			var loader11 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader11.LoadWhsInventory(criteria11), receiveLine1, receiveLine2, receiveLine3);

			var criteria12 = new WhsInventorySearchCriteriaInfo("", "", "A-1", "", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, condition, palletIDAndLocationJoinCondition);
			var loader12 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader12.LoadWhsInventory(criteria12), receiveLine1, receiveLine2, receiveLine3);

			var criteria13 = new WhsInventorySearchCriteriaInfo("", "", "", "P2", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, condition, palletIDAndLocationJoinCondition);
			var loader13 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader13.LoadWhsInventory(criteria13), receiveLine2, receiveLine3);

			var criteria14 = new WhsInventorySearchCriteriaInfo("", "", "", "", "A1", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader14 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			if (condition == SearchJoinCondition.And)
			{
				AssertInventoryCollectionContains(loader14.LoadWhsInventory(criteria14), noMatchingInventory);
			}
			else
			{
				AssertInventoryCollectionContains(loader14.LoadWhsInventory(criteria14), receiveLine1, receiveLine2, receiveLine3, receiveLine4);
			}

			var criteria15 = new WhsInventorySearchCriteriaInfo("", "", "", "", "", "B2", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader15 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			if (condition == SearchJoinCondition.And)
			{
				AssertInventoryCollectionContains(loader15.LoadWhsInventory(criteria15), noMatchingInventory);
			}
			else
			{
				AssertInventoryCollectionContains(loader15.LoadWhsInventory(criteria15), receiveLine4);
			}

			var criteria16 = new WhsInventorySearchCriteriaInfo("", "", "", "", "", "", "E3", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader16 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			if (condition == SearchJoinCondition.And)
			{
				AssertInventoryCollectionContains(loader16.LoadWhsInventory(criteria16), noMatchingInventory);
			}
			else
			{
				AssertInventoryCollectionContains(loader16.LoadWhsInventory(criteria16), receiveLine1, receiveLine3);
			}

			var criteria17 = new WhsInventorySearchCriteriaInfo("", "", "", "", "", "", "", "", todayAsDateTime, new DateTime(), WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader17 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			if (condition == SearchJoinCondition.And)
			{
				AssertInventoryCollectionContains(loader17.LoadWhsInventory(criteria17), noMatchingInventory);
			}
			else
			{
				AssertInventoryCollectionContains(loader17.LoadWhsInventory(criteria17), receiveLine1, receiveLine2, receiveLine4, receiveLine5);
			}

			var criteria18 = new WhsInventorySearchCriteriaInfo("", "", "", "", "", "", "", "", new DateTime(), todayAsDateTime, WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader18 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			if (condition == SearchJoinCondition.And)
			{
				AssertInventoryCollectionContains(loader18.LoadWhsInventory(criteria18), noMatchingInventory);
			}
			else
			{
				AssertInventoryCollectionContains(loader18.LoadWhsInventory(criteria18), receiveLine1, receiveLine2, receiveLine3);
			}

			var criteria19 = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part1.OP_PartNum, "A-1", "P2", "A1", "A2", "A3", "SN2", todayAsDateTime, todayAsDateTime, WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader19 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			if (condition == SearchJoinCondition.And)
			{
				AssertInventoryCollectionContains(loader19.LoadWhsInventory(criteria19), receiveLine2);
			}
			else
			{
				AssertInventoryCollectionContains(loader19.LoadWhsInventory(criteria19), receiveLine1, receiveLine2, receiveLine3, receiveLine4, receiveLine5);
			}

			var criteria20 = new WhsInventorySearchCriteriaInfo("", "", "A-1", "P2", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, condition, palletIDAndLocationJoinCondition);
			var loader20 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			if (palletIDAndLocationJoinCondition == SearchJoinCondition.And)
			{
				AssertInventoryCollectionContains(loader20.LoadWhsInventory(criteria20), receiveLine2, receiveLine3);
			}
			else
			{
				AssertInventoryCollectionContains(loader20.LoadWhsInventory(criteria20), receiveLine1, receiveLine2, receiveLine3);
			}

			var criteria21 = new WhsInventorySearchCriteriaInfo("", "", "", "", "", "", "", "notexist", new DateTime(), new DateTime(), WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader21 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader21.LoadWhsInventory(criteria21), noMatchingInventory);

			var criteria22 = new WhsInventorySearchCriteriaInfo("", "", "", "", "", "", "", "SN5", new DateTime(), new DateTime(), WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader22 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			if (condition == SearchJoinCondition.And)
			{
				AssertInventoryCollectionContains(loader22.LoadWhsInventory(criteria22), noMatchingInventory);
			}
			else
			{
				AssertInventoryCollectionContains(loader22.LoadWhsInventory(criteria22), receiveLine5);
			}
		}

		#endregion

		#region TestLoadWhsInventory_MultipleClients

		public void TestLoadWhsInventory_MultipleClients_SearchJoinConditionIsAnd_PalletIDAndLocationJoinConditionIsAnd()
		{
			TestLoadWhsInventory_MultipleClientsCore(SearchJoinCondition.And, SearchJoinCondition.And);
		}

		public void TestLoadWhsInventory_MultipleClients_SearchJoinConditionIsAnd_PalletIDAndLocationJoinConditionIsOr()
		{
			TestLoadWhsInventory_MultipleClientsCore(SearchJoinCondition.And, SearchJoinCondition.Or);
		}

		public void TestLoadWhsInventory_MultipleClients_SearchJoinConditionIsOr_PalletIDAndLocationJoinConditionIsAnd()
		{
			TestLoadWhsInventory_MultipleClientsCore(SearchJoinCondition.Or, SearchJoinCondition.And);
		}

		public void TestLoadWhsInventory_MultipleClients_SearchJoinConditionIsOr_PalletIDAndLocationJoinConditionIsOr()
		{
			TestLoadWhsInventory_MultipleClientsCore(SearchJoinCondition.Or, SearchJoinCondition.Or);
		}

		void TestLoadWhsInventory_MultipleClientsCore(SearchJoinCondition condition, SearchJoinCondition palletIDAndLocationJoinCondition)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine11 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1");
			var receiveLine12 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA2, "P2");
			var receiveLine13 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P3");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "R2", Notify);
			var receiveLine21 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locationA1, "P1");
			var receiveLine22 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locationA2, "P2");
			var receiveLine23 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locationA1, "P3");
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			var criteria1 = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part1.OP_PartNum, "", "", joinCondition: condition, palletIDAndLocationJoinCondition: palletIDAndLocationJoinCondition);
			var loader1 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader1.LoadWhsInventory(criteria1), receiveLine11, receiveLine12, receiveLine13);

			var criteria2 = new WhsInventorySearchCriteriaInfo(client2.OH_Code, data.Part1.OP_PartNum, "", "", joinCondition: condition, palletIDAndLocationJoinCondition: palletIDAndLocationJoinCondition);
			var loader2 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader2.LoadWhsInventory(criteria2), receiveLine21, receiveLine22, receiveLine23);

			var criteria3 = new WhsInventorySearchCriteriaInfo("", "", "A-1", "P1", joinCondition: condition, palletIDAndLocationJoinCondition: palletIDAndLocationJoinCondition);
			var loader3 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);

			if (palletIDAndLocationJoinCondition == SearchJoinCondition.And)
			{
				AssertInventoryCollectionContains(loader3.LoadWhsInventory(criteria3), receiveLine11, receiveLine21);
			}
			else
			{
				AssertInventoryCollectionContains(loader3.LoadWhsInventory(criteria3), receiveLine11, receiveLine13, receiveLine21, receiveLine23);
			}
		}

		#endregion

		#region TestLoadWhsInventory_MultipleWarehouses

		public void TestLoadWhsInventory_MultipleWarehouses_SearchJoinConditionIsAnd_PalletIDAndLocationJoinConditionIsAnd()
		{
			TestLoadWhsInventory_MultipleWarehousesCore(SearchJoinCondition.And, SearchJoinCondition.And);
		}
		public void TestLoadWhsInventory_MultipleWarehouses_SearchJoinConditionIsAnd_PalletIDAndLocationJoinConditionIsOr()
		{
			TestLoadWhsInventory_MultipleWarehousesCore(SearchJoinCondition.And, SearchJoinCondition.Or);
		}

		public void TestLoadWhsInventory_MultipleWarehouses_SearchJoinConditionIsOr_PalletIDAndLocationJoinConditionIsAnd()
		{
			TestLoadWhsInventory_MultipleWarehousesCore(SearchJoinCondition.Or, SearchJoinCondition.And);
		}

		public void TestLoadWhsInventory_MultipleWarehouses_SearchJoinConditionIsOr_PalletIDAndLocationJoinConditionIsOr()
		{
			TestLoadWhsInventory_MultipleWarehousesCore(SearchJoinCondition.Or, SearchJoinCondition.Or);
		}

		void TestLoadWhsInventory_MultipleWarehousesCore(SearchJoinCondition condition, SearchJoinCondition palletIDAndLocationJoinCondition)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "A", 2, 1);
			Factory.Save();

			var locationA1_Whs1 = data.Whs1.FindLocation("A-1");
			var locationA2_Whs1 = data.Whs1.FindLocation("A-2");
			var locationA1_Whs2 = whs2.FindLocation("A-1");
			var locationA2_Whs2 = whs2.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine11 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1_Whs1, "P1");
			var receiveLine12 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA2_Whs1, "P2");
			var receiveLine13 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1_Whs1, "P3");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, whs2, "R2", Notify);
			var receiveLine21 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locationA1_Whs2, "P1");
			var receiveLine22 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locationA2_Whs2, "P2");
			var receiveLine23 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locationA2_Whs2, "P3");
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			var criteria1 = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part1.OP_PartNum, "A-1", "", joinCondition: condition, palletIDAndLocationJoinCondition: palletIDAndLocationJoinCondition);
			var loader1 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader1.LoadWhsInventory(criteria1), receiveLine11, receiveLine12, receiveLine13);

			var criteria2 = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part1.OP_PartNum, "A-2", "", joinCondition: condition, palletIDAndLocationJoinCondition: palletIDAndLocationJoinCondition);
			var loader2 = new WhsInventoryLoader(Factory, whs2.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader2.LoadWhsInventory(criteria2), receiveLine21, receiveLine22, receiveLine23);

			var criteria3 = new WhsInventorySearchCriteriaInfo("", data.Part1.OP_PartNum, "A-1", "P1", joinCondition: condition, palletIDAndLocationJoinCondition: palletIDAndLocationJoinCondition);
			var loader3 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader3.LoadWhsInventory(criteria3), receiveLine11, receiveLine12, receiveLine13);

			var criteria4 = new WhsInventorySearchCriteriaInfo("", "", "A-2", "P2", joinCondition: condition, palletIDAndLocationJoinCondition: palletIDAndLocationJoinCondition);
			var loader4 = new WhsInventoryLoader(Factory, whs2.WW_WarehouseCode);
			if (palletIDAndLocationJoinCondition == SearchJoinCondition.And)
			{
				AssertInventoryCollectionContains(loader4.LoadWhsInventory(criteria4), receiveLine22);
			}
			else
			{
				AssertInventoryCollectionContains(loader4.LoadWhsInventory(criteria4), receiveLine22, receiveLine23);
			}
		}

		#endregion

		#region TestLoadWhsInventory_FilterOutZeroUnit

		public void TestLoadWhsInventory_FilterOutZeroUnit_SearchJoinConditionIsAnd()
		{
			TestLoadWhsInventory_FilterOutZeroUnitCore(SearchJoinCondition.And);
		}

		public void TestLoadWhsInventory_FilterOutZeroUnit_SearchJoinConditionIsOr()
		{
			TestLoadWhsInventory_FilterOutZeroUnitCore(SearchJoinCondition.Or);
		}

		void TestLoadWhsInventory_FilterOutZeroUnitCore(SearchJoinCondition condition)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2");
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 0m, locationA1, "P3");

			Factory.Save();

			var criteria1 = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part1.OP_PartNum, "", "", joinCondition: condition);
			var loader1 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader1.LoadWhsInventory(criteria1), receiveLine1, receiveLine2);

			var criteria2 = new WhsInventorySearchCriteriaInfo("", data.Part1.OP_PartNum, "A-1", "", "", joinCondition: condition);
			var loader2 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader2.LoadWhsInventory(criteria2), receiveLine1, receiveLine2);
		}

		#endregion

		#region TestLoadWhsInventory_PartBarcode

		public void TestLoadWhsInventory_PartBarcode_SearchJoinConditionIsAnd()
		{
			TestLoadWhsInventory_PartBarcode(SearchJoinCondition.And);
		}

		public void TestLoadWhsInventory_PartBarcode_SearchJoinConditionIsOr()
		{
			TestLoadWhsInventory_PartBarcode(SearchJoinCondition.Or);
		}

		void TestLoadWhsInventory_PartBarcode(SearchJoinCondition condition)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			var partBarcode = data.Part1.PartBarcodes.AddNew();
			partBarcode.PH_Barcode = "PartBarCode";
			partBarcode.PH_F3_NKPackType = "PKG";

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2");
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, locationA1, "P3");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			var criteria1 = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, "PartBarcode", "", "", joinCondition: condition);
			var loader1 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader1.LoadWhsInventory(criteria1), receiveLine1, receiveLine2);
		}

		#endregion

		#region TestLoadWhsInventory_OldLocationBarcodes

		public void TestLoadWhsInventory_OldLocationBarcodes_SearchJoinConditionIsAnd()
		{
			TestLoadWhsInventory_OldLocationBarcodes(SearchJoinCondition.And);
		}

		public void TestLoadWhsInventory_OldLocationBarcodes_SearchJoinConditionIsOr()
		{
			TestLoadWhsInventory_OldLocationBarcodes(SearchJoinCondition.Or);
		}

		void TestLoadWhsInventory_OldLocationBarcodes(SearchJoinCondition condition)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2");
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, locationA2, "P3");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			var criteria1 = new WhsInventorySearchCriteriaInfo("", "", locationA1.OldBarcode, "", joinCondition: condition);
			var loader1 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader1.LoadWhsInventory(criteria1), receiveLine1, receiveLine2);
		}

		#endregion

		#region TestLoadWhsInventory_CriteriaIsSame

		public void TestLoadWhsInventory_CriteriaIsSame_SearchJoinConditionIsAnd_PalletIDAndLocationJoinConditionIsAnd()
		{
			TestLoadWhsInventory_CriteriaIsSameCore(SearchJoinCondition.And, SearchJoinCondition.And);
		}

		public void TestLoadWhsInventory_CriteriaIsSame_SearchJoinConditionIsAnd_PalletIDAndLocationJoinConditionIsOr()
		{
			TestLoadWhsInventory_CriteriaIsSameCore(SearchJoinCondition.And, SearchJoinCondition.Or);
		}

		public void TestLoadWhsInventory_CriteriaIsSame_SearchJoinConditionIsOr_PalletIDAndLocationJoinConditionIsAnd()
		{
			TestLoadWhsInventory_CriteriaIsSameCore(SearchJoinCondition.Or, SearchJoinCondition.And);
		}
		public void TestLoadWhsInventory_CriteriaIsSame_SearchJoinConditionIsOr_PalletIDAndLocationJoinConditionIsOr()
		{
			TestLoadWhsInventory_CriteriaIsSameCore(SearchJoinCondition.Or, SearchJoinCondition.Or);
		}

		void TestLoadWhsInventory_CriteriaIsSameCore(SearchJoinCondition condition, SearchJoinCondition palletIDAndLocationJoinCondition)
		{
			var search = "X";
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs = Helper.CreateWarehouse("WH2", search);
			Factory.Save();

			var row2 = Helper.CreateRowAndGenerateLocations(whs, "B", 1, 1);
			var location = whs.FindLocation(search);

			var part3 = Helper.CreateProduct(search, data.Org1);

			var partBarcode = data.Part1.PartBarcodes.AddNew();
			var barcode = "barcode1";
			partBarcode.PH_Barcode = barcode;
			partBarcode.PH_F3_NKPackType = "PKG";

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, part3, true, useSerialNumber: false);

			var receive1 = Helper.CreateWhsReceive(data.Org1, whs, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, location, "P1", today, today, "A1", "A2", "A3", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, row2.Locations[0], "P2", today, today, "A1", "A2", "A3", "");
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive1, part3, 10m, row2.Locations[0], "P2", today, today, "A1", "A2", "A3", "");
			var receiveLine4 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, location, search, today, today, "A1", "B2", "B3", "");
			var receiveLine5 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, row2.Locations[0], "P3", today, today, search, "B2", "B3", "");
			var receiveLine6 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, row2.Locations[0], "P3", today, today, "B1", search, "B3", "");
			var receiveLine7 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, row2.Locations[0], "P3", today, today, "B1", "B2", search, "");
			var receiveLine8 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, row2.Locations[0], "P3", today, today, "B1", "B2", "B3", "");
			var receiveLine9 = Helper.CreateWhsReceiveInventoryLine(receive1, part3.PK, 1m, location.PK, search, today, today, search, search, search, "", "");
			var receiveLine10 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 1m, location.PK, search, today, today, search, search, search, "", "");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			// hack to set serial
			receiveLine9.WI_SerialNumber = search;
			receiveLine10.WI_SerialNumber = search;

			Factory.Save();

			var dateFilterForTest = condition == SearchJoinCondition.And ? today.ToDateTime() : new DateTime(); // Match test as close as possible to before attribute filters were always applied

			var criteria1 = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, search, search, search, search, search, search, search, dateFilterForTest, dateFilterForTest, WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader1 = new WhsInventoryLoader(Factory, whs.WW_WarehouseCode);
			if (condition == SearchJoinCondition.And)
			{
				AssertInventoryCollectionContains(loader1.LoadWhsInventory(criteria1), receiveLine9);
			}
			else
			{
				if (palletIDAndLocationJoinCondition == SearchJoinCondition.And)
				{
					AssertInventoryCollectionContains(loader1.LoadWhsInventory(criteria1), receiveLine3, receiveLine4, receiveLine5, receiveLine6, receiveLine7, receiveLine9, receiveLine10);
				}
				else
				{
					AssertInventoryCollectionContains(loader1.LoadWhsInventory(criteria1), receiveLine1, receiveLine3, receiveLine4, receiveLine5, receiveLine6, receiveLine7, receiveLine9, receiveLine10);
				}
			}

			var criteria2 = new WhsInventorySearchCriteriaInfo("", search, search, search, search, search, search, search, dateFilterForTest, dateFilterForTest, WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader2 = new WhsInventoryLoader(Factory, whs.WW_WarehouseCode);
			if (condition == SearchJoinCondition.And)
			{
				AssertInventoryCollectionContains(loader2.LoadWhsInventory(criteria2), receiveLine9);
			}
			else
			{
				if (palletIDAndLocationJoinCondition == SearchJoinCondition.And)
				{
					AssertInventoryCollectionContains(loader2.LoadWhsInventory(criteria2), receiveLine3, receiveLine4, receiveLine5, receiveLine6, receiveLine7, receiveLine9, receiveLine10);
				}
				else
				{
					AssertInventoryCollectionContains(loader2.LoadWhsInventory(criteria2), receiveLine1, receiveLine3, receiveLine4, receiveLine5, receiveLine6, receiveLine7, receiveLine9, receiveLine10);
				}
			}

			var criteria3 = new WhsInventorySearchCriteriaInfo("", "", search, search, search, search, search, search, dateFilterForTest, dateFilterForTest, WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader3 = new WhsInventoryLoader(Factory, whs.WW_WarehouseCode);
			if (condition == SearchJoinCondition.And)
			{
				AssertInventoryCollectionContains(loader3.LoadWhsInventory(criteria3), receiveLine9, receiveLine10);
			}
			else
			{
				if (palletIDAndLocationJoinCondition == SearchJoinCondition.And)
				{
					AssertInventoryCollectionContains(loader3.LoadWhsInventory(criteria3), receiveLine4, receiveLine5, receiveLine6, receiveLine7, receiveLine9, receiveLine10);
				}
				else
				{
					AssertInventoryCollectionContains(loader3.LoadWhsInventory(criteria3), receiveLine1, receiveLine4, receiveLine5, receiveLine6, receiveLine7, receiveLine9, receiveLine10);
				}
			}

			var criteria4 = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, barcode, search, search, search, search, search, search, dateFilterForTest, dateFilterForTest, WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader4 = new WhsInventoryLoader(Factory, whs.WW_WarehouseCode);
			if (condition == SearchJoinCondition.And)
			{
				AssertInventoryCollectionContains(loader4.LoadWhsInventory(criteria4), receiveLine10);
			}
			else
			{
				if (palletIDAndLocationJoinCondition == SearchJoinCondition.And)
				{
					AssertInventoryCollectionContains(loader4.LoadWhsInventory(criteria4), receiveLine2, receiveLine4, receiveLine5, receiveLine6, receiveLine7, receiveLine9, receiveLine10);
				}
				else
				{
					AssertInventoryCollectionContains(loader4.LoadWhsInventory(criteria4), receiveLine1, receiveLine2, receiveLine4, receiveLine5, receiveLine6, receiveLine7, receiveLine9, receiveLine10);
				}
			}

			var criteria5 = new WhsInventorySearchCriteriaInfo("", barcode, search, search, search, search, search, search, dateFilterForTest, dateFilterForTest, WhsInventoryLevel.Level3, condition, palletIDAndLocationJoinCondition);
			var loader5 = new WhsInventoryLoader(Factory, whs.WW_WarehouseCode);
			if (condition == SearchJoinCondition.And)
			{
				AssertInventoryCollectionContains(loader5.LoadWhsInventory(criteria5), receiveLine10);
			}
			else
			{
				if (palletIDAndLocationJoinCondition == SearchJoinCondition.And)
				{
					AssertInventoryCollectionContains(loader5.LoadWhsInventory(criteria5), receiveLine2, receiveLine4, receiveLine5, receiveLine6, receiveLine7, receiveLine9, receiveLine10);
				}
				else
				{
					AssertInventoryCollectionContains(loader5.LoadWhsInventory(criteria5), receiveLine1, receiveLine2, receiveLine4, receiveLine5, receiveLine6, receiveLine7, receiveLine9, receiveLine10);
				}
			}
		}

		#endregion

		#region TestLoadWhsInventory_ClearInvalidParameterInCriteria

		public void TestLoadWhsInventory_ClearInvalidParameterInCriteria_Location()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", today, today, "A1", "A1", "E3", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2", today, today, "A1", "A2", "A3", "");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo("", "notexist", "A-1", "notexist", joinCondition: SearchJoinCondition.Or, palletIDAndLocationJoinCondition: SearchJoinCondition.Or);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1, receiveLine2);

			AssertEquals("Invalid Product should be clear", "", criteria.ProductCode);
			AssertEquals("Invalid info should be clear", "", criteria.PalletID);
			AssertEquals("Valid info should *not* be cleared", "A-1", criteria.Location);
		}

		public void TestLoadWhsInventory_ClearInvalidParameterInCriteria_PalletID()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", today, today, "A1", "A1", "E3", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", today, today, "A1", "A2", "A3", "");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo("", "notexist", "notexist", "P1", joinCondition: SearchJoinCondition.Or, palletIDAndLocationJoinCondition: SearchJoinCondition.Or);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1, receiveLine2);

			AssertEquals("Invalid Product should be clear", "", criteria.ProductCode);
			AssertEquals("Invalid info should be clear", "", criteria.Location);
			AssertEquals("Valid info should *not* be cleared", "P1", criteria.PalletID);
		}

		public void TestLoadWhsInventory_ClearInvalidParameterInCriteria_HasValidProduct_DestinationIsLevel1()
		{
			TestLoadWhsInventory_ClearInvalidParameterInCriteria_HasValidProduct_DestinationLevelCore(WhsInventoryLevel.Level1);
		}

		public void TestLoadWhsInventory_ClearInvalidParameterInCriteria_HasValidProduct_DestinationIsLevel2()
		{
			TestLoadWhsInventory_ClearInvalidParameterInCriteria_HasValidProduct_DestinationLevelCore(WhsInventoryLevel.Level2);
		}

		public void TestLoadWhsInventory_ClearInvalidParameterInCriteria_HasValidProduct_DestinationIsLevel3()
		{
			TestLoadWhsInventory_ClearInvalidParameterInCriteria_HasValidProduct_DestinationLevelCore(WhsInventoryLevel.Level3);
		}

		void TestLoadWhsInventory_ClearInvalidParameterInCriteria_HasValidProduct_DestinationLevelCore(WhsInventoryLevel level)
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", today, today, "A1", "A1", "E3", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2", today, today, "A1", "A2", "A3", "");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part1.OP_PartNum, "A-1", "P1", destLevel: level, joinCondition: SearchJoinCondition.Or);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1, receiveLine2);

			AssertEquals("Product Code should not be cleared", data.Part1.OP_PartNum, criteria.ProductCode);
			if (level == WhsInventoryLevel.Level1)
			{
				AssertEquals("When Product is exist, Location should be clear", "", criteria.Location);
				AssertEquals("When Product is exist, PalletID should be clear", "", criteria.PalletID);
			}
			else
			{
				AssertEquals("Location is valid, it should not be clear", "A-1", criteria.Location);
				AssertEquals("PalletID is valid, it should not be clear", "P1", criteria.PalletID);
			}
		}

		#endregion

		#region TestLoadWhsInventory_ProductPKSpecified

		public void TestLoadWhsInventory_ProductPKSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			var product = Helper.CreateProduct("Part", data.Org1);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, product, 10m, locationA1, "P3");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "R2", Notify);
			var receiveLine21 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locationA1, "P1");
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, product.OP_PartNum, "", "", destLevel: WhsInventoryLevel.Level1, joinCondition: SearchJoinCondition.Or)
			{
				ProductPK = data.Part1.PK.ToGuid()
			};
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1);

			var criteria2 = new WhsInventorySearchCriteriaInfo("", product.OP_PartNum, "", "", destLevel: WhsInventoryLevel.Level1, joinCondition: SearchJoinCondition.Or)
			{
				ProductPK = data.Part1.PK.ToGuid()
			};
			var loader2 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader2.LoadWhsInventory(criteria2), receiveLine1, receiveLine21);
		}

		#endregion

		#region TestLoadWhsInventory_ByLocation

		public void TestLoadWhsInventory_ByLocation_InTransitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var orderPickLine = orderLine.PickLines.Single();

			var transferLine = Helper.PickAndMakeInTransitTransfer(orderPickLine, ZDateTimeOffset.Now);
			Factory.Save();

			AssertEquals("Precondition:", "DOCKDOOR", transferLine.LocationString);

			var criteria = new WhsInventorySearchCriteriaInfo("", "", "DOCKDOOR", "", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, SearchJoinCondition.Or, SearchJoinCondition.Or);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertEquals("Should not find out In-Transit inventory", false, loader.LoadWhsInventory(criteria).Any());
		}

		public void TestLoadWhsInventory_ByLocation_PuttingAwayInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PLT1", 15m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT1", 15m);
			transfer.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals($"Precondition: Inventory Status should be {InventoryStatus.Codes.Received}", InventoryStatus.Codes.Received, inventory.WI_InventoryStatus);

			var criteria = new WhsInventorySearchCriteriaInfo("", "", "A-2", "", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, SearchJoinCondition.Or, SearchJoinCondition.Or);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertEquals("Should not find out Putting-Away inventory", false, loader.LoadWhsInventory(criteria).Any());
		}

		public void TestLoadWhsInventory_ByLocation_ReceivedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "PLT1", 15m);
			Factory.Save();

			AssertEquals($"Precondition: Inventory Status should be {InventoryStatus.Codes.Received}", InventoryStatus.Codes.Received, inventory.WI_InventoryStatus);

			var criteria = new WhsInventorySearchCriteriaInfo("", "", "A-1", "", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, SearchJoinCondition.Or, SearchJoinCondition.Or);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), inventory);
		}

		public void TestLoadWhsInventory_ByLocation_FixedWIdthLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			Factory.Save();

			var location = warehouse.FindLocation("Z040302");
			var receive = Helper.CreateWhsReceive(data.Org1, warehouse, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, location, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertEquals($"Precondition: Inventory Status should be {InventoryStatus.Codes.Available}", InventoryStatus.Codes.Available, inventory.WI_InventoryStatus);

			var criteria1 = new WhsInventorySearchCriteriaInfo("", "", "Z040302", "", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, SearchJoinCondition.Or, SearchJoinCondition.Or);
			var loader1 = new WhsInventoryLoader(Factory, warehouse.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader1.LoadWhsInventory(criteria1), inventory);

			var criteria2 = new WhsInventorySearchCriteriaInfo("", "", "Z-04-03-02", "", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, SearchJoinCondition.Or, SearchJoinCondition.Or);
			var loader2 = new WhsInventoryLoader(Factory, warehouse.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader2.LoadWhsInventory(criteria2), inventory);
		}

		#endregion

		#region TestLoadWhsInventory_WarehouseCode

		public void TestLoadWhsInventory_WarehouseCode()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var criteria = new WhsInventorySearchCriteriaInfo();
			var invalidWarehouseCode = "Bla";
			var loader1 = new WhsInventoryLoader(Factory, invalidWarehouseCode);
			AssertExceptionThrown<InvalidOperationException>(() => loader1.LoadWhsInventory(criteria));

			var loader2 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertNoExceptionThrown(() => loader2.LoadWhsInventory(criteria));
		}

		#endregion

		#region TestLoadWhsInventory_DoesNotFilterByAttributes

		public void TestLoadWhsInventory_Level1_DoesNotFilterByAttributes() => TestLoadWhsInventory_DoesNotFilterByAttributes(WhsInventoryLevel.Level1);
		public void TestLoadWhsInventory_Level2_DoesNotFilterByAttributes() => TestLoadWhsInventory_DoesNotFilterByAttributes(WhsInventoryLevel.Level2);

		void TestLoadWhsInventory_DoesNotFilterByAttributes(WhsInventoryLevel level)
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", today, today, "A1", "A1", "E3", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2", today, today, "A1", "A2", "A3", "");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo("", "", "", "", "PA1", "PA2", "PA3", "", today.AddDays(-1).ToDateTime(), today.AddDays(-1).ToDateTime(), level, SearchJoinCondition.Or);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1, receiveLine2);
		}

		#endregion

		#region TestLoadWhsInventory_Level3_PartAttribute1

		public void TestLoadWhsInventory_Level3_PartAttribute1()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", today, today, "A1", "A2", "A3", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2", today, today, "E1", "A2", "A3", "");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo("", "", "", "", "A1", "A2", "A3", "", today.ToDateTime(), today.ToDateTime(), WhsInventoryLevel.Level3, SearchJoinCondition.And);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1);
		}

		#endregion

		#region TestLoadWhsInventory_Level3_PartAttribute1_Empty

		public void TestLoadWhsInventory_Level3_PartAttribute1_Empty()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", today, today, "", "A2", "A3", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2", today, today, "A1", "A2", "A3", "");

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo("", "", "", "", "", "A2", "A3", "", today.ToDateTime(), today.ToDateTime(), WhsInventoryLevel.Level3, SearchJoinCondition.And);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1);
		}

		#endregion

		#region TestLoadWhsInventory_Level3_PartAttribute2

		public void TestLoadWhsInventory_Level3_PartAttribute2()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", today, today, "A1", "A2", "A3", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2", today, today, "A1", "E2", "A3", "");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo("", "", "", "", "A1", "A2", "A3", "", today.ToDateTime(), today.ToDateTime(), WhsInventoryLevel.Level3, SearchJoinCondition.And);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1);
		}

		#endregion

		#region TestLoadWhsInventory_Level3_PartAttribute2_Empty

		public void TestLoadWhsInventory_Level3_PartAttribute2_Empty()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", today, today, "A1", "", "A3", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2", today, today, "A1", "A2", "A3", "");
			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo("", "", "", "", "A1", "", "A3", "", today.ToDateTime(), today.ToDateTime(), WhsInventoryLevel.Level3, SearchJoinCondition.And);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1);
		}

		#endregion

		#region TestLoadWhsInventory_Level3_PartAttribute3

		public void TestLoadWhsInventory_Level3_PartAttribute3()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", today, today, "A1", "A2", "A3", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2", today, today, "A1", "A2", "E3", "");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo("", "", "", "", "A1", "A2", "A3", "", today.ToDateTime(), today.ToDateTime(), WhsInventoryLevel.Level3, SearchJoinCondition.And);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1);
		}

		#endregion

		#region TestLoadWhsInventory_Level3_PartAttribute3_Empty

		public void TestLoadWhsInventory_Level3_PartAttribute3_Empty()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", today, today, "A1", "A2", "", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2", today, today, "A1", "A2", "A3", "");
			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo("", "", "", "", "A1", "A2", "", "", today.ToDateTime(), today.ToDateTime(), WhsInventoryLevel.Level3, SearchJoinCondition.And);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1);
		}

		#endregion

		#region TestLoadWhsInventory_Level3_SerialNumber

		public void TestLoadWhsInventory_Level3_SerialNumber()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 1m, locationA1.PK, "P1", today, today, "", "", "", "SN1", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 1m, locationA1.PK, "P2", today, today, "", "", "", "SN2", "");
			Factory.Save();

			var criteria1 = new WhsInventorySearchCriteriaInfo("", "", "", "", "", "", "", "SN1", today.ToDateTime(), today.ToDateTime(), WhsInventoryLevel.Level3, SearchJoinCondition.And);
			var loader1 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader1.LoadWhsInventory(criteria1), receiveLine1);

			var criteria2 = new WhsInventorySearchCriteriaInfo("", "", "", "", "", "", "", "SN2", today.ToDateTime(), today.ToDateTime(), WhsInventoryLevel.Level3, SearchJoinCondition.And);
			var loader2 = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader2.LoadWhsInventory(criteria2), receiveLine2);
		}

		public void TestLoadWhsInventory_Level3_SerialNumber_Empty()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 1m, locationA1.PK, "P1", today, today, "", "", "", "", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 1m, locationA1.PK, "P2", today, today, "", "", "", "SN2", "");
			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo("", "", "", "", "", "", "", "", today.ToDateTime(), today.ToDateTime(), WhsInventoryLevel.Level3, SearchJoinCondition.And);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1);
		}

		#endregion

		#region TestLoadWhsInventory_Level3_ExpiryDate

		public void TestLoadWhsInventory_Level3_ExpiryDate()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", today, today, "A1", "A2", "A3", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2", today.AddDays(1), today, "A1", "A2", "A3", "");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo("", "", "", "", "A1", "A2", "A3", "", today.ToDateTime(), today.ToDateTime(), WhsInventoryLevel.Level3, SearchJoinCondition.And);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1);
		}

		#endregion

		#region TestLoadWhsInventory_Level3_ExpiryDate_Empty

		public void TestLoadWhsInventory_Level3_ExpiryDate_Empty()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", ZDate.Empty, today, "A1", "A2", "A3", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2", today, today, "A1", "A2", "A3", "");
			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo("", "", "", "", "A1", "A2", "A3", "", DateTime.MinValue, today.ToDateTime(), WhsInventoryLevel.Level3, SearchJoinCondition.And);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1);
		}

		#endregion

		#region TestLoadWhsInventory_Level3_PackingDate

		public void TestLoadWhsInventory_Level3_PackingDate()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", today, today, "A1", "A2", "A3", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2", today, today.AddDays(1), "A1", "A2", "A3", "");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo("", "", "", "", "A1", "A2", "A3", "", today.ToDateTime(), today.ToDateTime(), WhsInventoryLevel.Level3, SearchJoinCondition.And);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1);
		}

		#endregion

		#region TestLoadWhsInventory_Level3_PackingDate_Empty

		public void TestLoadWhsInventory_Level3_PackingDate_Empty()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", today, ZDate.Empty, "A1", "A2", "A3", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2", today, today, "A1", "A2", "A3", "");
			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo("", "", "", "", "A1", "A2", "A3", "", today.ToDateTime(), DateTime.MinValue, WhsInventoryLevel.Level3, SearchJoinCondition.And);
			var loader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			AssertInventoryCollectionContains(loader.LoadWhsInventory(criteria), receiveLine1);
		}

		#endregion

		#region AssertInventoryCollectionContains

		void AssertInventoryCollectionContains(WhsInventoryView[] collection, params WhsInventoryView[] inventory)
		{
			AssertContainsExactElementsInAnyOrder("The inventory you were looking for was not found in the collection.", inventory.Select(i => i.WI_LineNo), collection.Select(i => i.WI_LineNo));
		}

		#endregion

		#region TestLoadWhsInventoryIncludeIntransit__Pallets

		public void TestLoadWhsInventoryIncludeIntransit_Pallets_Finalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "P1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo(string.Empty, string.Empty, string.Empty, "P1");
			var inventoryLoader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			var result = inventoryLoader.LoadWhsInventory(criteria, JoinCondition.And, excludeIntransit: false);
			AssertEquals("Loader has 1 result", 1, result.Length);
			AssertEquals("Pallet is correct", "P1", result[0].WI_PalletID);
			AssertEquals("Receive is correct", receiveLine.PK, result[0].InDocketLine.PK);
			AssertEquals("Stock is correct", 10m, result[0].WI_TotalUnits);
		}

		public void TestLoadWhsInventoryIncludeIntransit_Pallets_PuttingAway()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var otherTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			otherTransfer.WD_IsPutawayTransfer = true;
			var otherTransferLine = Helper.SetupTransferLineForDockDoorLocation(otherTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			otherTransfer.RunPreSaveValidation();
			otherTransferLine.PickedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure inventory is Putting Away.", InventoryStatus.Codes.PuttingAway, otherTransferLine.Inventory[0].WI_InventoryStatus);
			AssertEquals("Precondition - ensure inventory is on transfer.", 50m, otherTransferLine.Inventory[0].WI_TotalUnits);

			var criteria = new WhsInventorySearchCriteriaInfo(string.Empty, string.Empty, string.Empty, "PLT-1");
			var inventoryLoader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			var result = inventoryLoader.LoadWhsInventory(criteria, JoinCondition.And, excludeIntransit: false);
			AssertEquals("Loader has 1 result", 1, result.Length);
			AssertEquals("Pallet is correct", "PLT-1", result[0].WI_PalletID);
			AssertEquals("Receive is correct", otherTransferLine.PK, result[0].InDocketLine.PK);
			AssertEquals("Stock is correct", 50m, result[0].WI_TotalUnits);
		}

		public void TestLoadWhsInventoryIncludeIntransit_Pallets_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var location = data.Whs1.FindLocation("A-1");
			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "Pallet-Here", today, today, "A1", "A2", "A3", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 10m, "A-1", "Pallet-Here", ZGuid.Empty, "", "Pallet-Here", new ZDateTimeOffset(today), today, today, "A1", "A2", "A3");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure inventory is Intransit.", InventoryStatus.Codes.InTransit, transferLine.Inventory[0].WI_InventoryStatus);
			AssertEquals("Precondition - ensure inventory is on transfer.", 10m, transferLine.Inventory[0].WI_TotalUnits);
			AssertEquals("Precondition - ensure transfer Pallet is correct.", "Pallet-Here", transferLine.Inventory[0].WI_PalletID);

			var criteria = new WhsInventorySearchCriteriaInfo(string.Empty, string.Empty, string.Empty, "Pallet-Here");
			var inventoryLoader = new WhsInventoryLoader(Helper.Factory, data.Whs1.WW_WarehouseCode);
			var result = inventoryLoader.LoadWhsInventory(criteria, JoinCondition.And, excludeIntransit: false);
			AssertEquals("Loader has 1 result", 1, result.Length);
			AssertEquals("Pallet is correct", "Pallet-Here", result[0].WI_PalletID);
			AssertEquals("Receive is correct", transferLine.PK, result[0].InDocketLine.PK);
			AssertEquals("Stock is correct", 10m, result[0].WI_TotalUnits);
		}

		public void TestLoadWhsInventoryIncludeIntransit_Pallets_NoLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "P1");
			Helper.Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo(string.Empty, string.Empty, string.Empty, "P1");
			var inventoryLoader = new WhsInventoryLoader(Factory, data.Whs1.WW_WarehouseCode);
			var result = inventoryLoader.LoadWhsInventory(criteria, JoinCondition.And, excludeIntransit: false);
			AssertEquals("Loader has 1 result", 1, result.Length);
			AssertEquals("Pallet is correct", "P1", result[0].WI_PalletID);
			AssertEquals("Receive is correct", receiveLine.PK, result[0].InDocketLine.PK);
			AssertEquals("Stock is correct", 10m, result[0].WI_TotalUnits);
		}

		#endregion

		#region TestPerformance

		public void TestPerformance_DBHits()
		{
			var today = ZDate.Today;
			var tomorrow = today.AddDays(1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, useSerialNumber: false);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P1", today, today, "A1", "A1", "E3", "");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "P2", today, today, "A1", "A2", "A3", "");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, locationA1, "P2", tomorrow, today, "A1", "A2", "E3", "");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 1m, locationA2.PK, "P3", today, tomorrow, "A1", "B2", "B3", "", "");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>();
			expectedDBHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsInventoryViewSchema.Constants.TableName, 1);
			expectedDBHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsWarehouseSchema.Constants.TableName, 1);

			var criteria = new WhsInventorySearchCriteriaInfo("notexist", "", "", "", "", "", "", "", new DateTime(), new DateTime(), WhsInventoryLevel.Level1, SearchJoinCondition.Or, SearchJoinCondition.Or);
			var otherFactory = new BusinessObjectFactory();
			var loader = new WhsInventoryLoader(otherFactory, data.Whs1.WW_WarehouseCode);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, otherFactory))
			{
				loader.LoadWhsInventory(criteria);
			}
		}

		#endregion
	}

	#region WhsInventoryLoaderTestCase

	public class WhsInventoryLoaderTestCase : TestCase
	{
		#region TestLoadWhsInventory_ConcurrentPicking

		[UseSnapshotProtection]
		public void TestLoadWhsInventory_ConcurrentPicking()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, location1);
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = helper.CreatePickNew(order);
			factory.Save();

			using (Db.DisposableActionForDbConnection())
			using (var con1 = Db.NewAdminConnection())
			using (var con2 = Db.NewAdminConnection())
			{
				con1.BeginTransaction();
				var factory1 = new BusinessObjectFactory(con1);
				factory1.RefreshEnabled = false;
				var pickLine1 = factory1.Load<WhsPickLine>(orderLine1.PickLines.Single().PK).Split(3);
				helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);

				con2.BeginTransaction();
				var factory2 = new BusinessObjectFactory(con2);
				factory2.RefreshEnabled = false;
				var pickLine2 = factory2.Load<WhsPickLine>(orderLine2.PickLines.Single().PK);
				helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
				factory2.Save();
				con2.CommitTransaction();

				factory1.Save();
				con1.CommitTransaction();
			}

			using (Db.DisposableActionForDbConnection())
			using (var con3 = Db.NewAdminConnection())
			{
				con3.BeginTransaction();
				var factory3 = new BusinessObjectFactory(con3);
				factory3.RefreshEnabled = false;
				var pickLine3 = factory3.Load<WhsPickLine>(orderLine1.PickLines.Single().PK).Split(3);
				AssertNoExceptionThrown("Just in case if CW1 create more than one In-Transit Transfer still it should be able to continue its work.", () => helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now));
				factory3.Save();
				con3.CommitTransaction();
			}
		}

		#endregion

		#endregion
	}
}
