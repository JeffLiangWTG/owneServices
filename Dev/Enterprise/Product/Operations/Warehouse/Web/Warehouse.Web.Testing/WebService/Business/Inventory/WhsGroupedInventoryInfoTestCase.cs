using System;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsGroupedInventoryInfo))]
	public class WhsGroupedInventoryInfoTestCase : WhsInventoryLineBaseInfoTestCase<WhsGroupedInventoryInfo>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var groupedInventory = new WhsGroupedInventoryInfo();
			AssertEquals(0m, groupedInventory.TotalUnitsOnHand);
			AssertEquals(0m, groupedInventory.TotalUnitsOnAvailable);
			AssertEquals(0, groupedInventory.TotalPalletIDs);
		}

		public void TestConstructor_ParameterIsInventory()
		{
			var now = ZDate.Today;
			var lineInfoCollection = new WhsGroupedInventoryLineInfoCollection();
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "P1", now, now, "A1", "A2", "A3", "SN1", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var groupedInventory = new WhsGroupedInventoryInfo(lineInfoCollection, receiveLine);

			AssertEquals("ClientCode", data.Org1.OH_Code, groupedInventory.ClientCode);
			AssertEquals("ClientPK", data.Org1.PK, groupedInventory.ClientPK);
			AssertEquals("ProductPK", data.Part1.PK, groupedInventory.ProductPK);
			AssertEquals("Qty", 1m, groupedInventory.Qty);
			AssertEquals("QtyUQ", data.Part1.OP_StockKeepingUnit, groupedInventory.QtyUQ);
			AssertEquals("Location", data.Whs1.DefaultLocation.ToLocationString(), groupedInventory.Location);
			AssertEquals("PalletID", "P1", groupedInventory.PalletID);
			AssertEquals("Attribute1", "A1", groupedInventory.Attribute1);
			AssertEquals("Attribute2", "A2", groupedInventory.Attribute2);
			AssertEquals("Attribute3", "A3", groupedInventory.Attribute3);
			AssertEquals("Attribute3", "SN1", groupedInventory.SerialNumber);
			AssertEquals("ExpiryDate", now, groupedInventory.ExpiryDate);
			AssertEquals("PackingDate", now, groupedInventory.PackingDate);
			AssertEquals("TotalUnitsOnHand", 0m, groupedInventory.TotalUnitsOnHand);
			AssertEquals("TotalUnitsOnAvailable", 0m, groupedInventory.TotalUnitsOnAvailable);
			AssertEquals("TotalPalletIDs", 0, groupedInventory.TotalPalletIDs);
		}

		public void TestConstructor_WithParameters_HasAttributes()
		{
			var now = ZDateTime.Now.ToDateTime();
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var lineInfoCollection = new WhsGroupedInventoryLineInfoCollection();
			var groupedInventory = new WhsGroupedInventoryInfo(lineInfoCollection, new Guid(), data.Org1, data.Part1, data.Whs1, "A1", "A2", "A3", "SN1", now, now, 2m, 2m, 2);

			AssertNotNull(groupedInventory);
			AssertEquals(data.Part1.OP_StockKeepingUnit, groupedInventory.QtyUQ);
			AssertEquals(data.Org1.OH_Code, groupedInventory.ClientCode);
			AssertEquals(data.Org1.PK, groupedInventory.ClientPK);
			AssertEquals("A1", groupedInventory.Attribute1);
			AssertEquals("A2", groupedInventory.Attribute2);
			AssertEquals("A3", groupedInventory.Attribute3);
			AssertEquals("SN1", groupedInventory.SerialNumber);
			AssertEquals(now, groupedInventory.ExpiryDate);
			AssertEquals(now, groupedInventory.PackingDate);
			AssertEquals(2m, groupedInventory.TotalUnitsOnHand);
			AssertEquals(2m, groupedInventory.TotalUnitsOnAvailable);
			AssertEquals(2, groupedInventory.TotalPalletIDs);
		}

		public void TestConstructor_WithInventoryParamater_UsesWarehouseCountryFormatString()
		{
			var now = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "P1", now, now, "A1", "A2", "A3", "SN1", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 1m, data.Whs1.DefaultLocation.PK, "P1", now, now, "A1", "A2", "A3", "SN1", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var lineInfoCollection1 = new WhsGroupedInventoryLineInfoCollection();
			var groupedInventory = new WhsGroupedInventoryInfo(lineInfoCollection1, receiveLine1);
			AssertEquals("ddMMyy", lineInfoCollection1.ProductPartAttributesInfos[0].ExpiryDateFormatString); // Date format for testing
			AssertEquals("ddMMyy", lineInfoCollection1.ProductPartAttributesInfos[0].PackingDateFormatString); // Date format for testing

			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "CN";

			var lineInfoCollection2 = new WhsGroupedInventoryLineInfoCollection();
			groupedInventory = new WhsGroupedInventoryInfo(lineInfoCollection2, receiveLine2);

			AssertEquals("yyMMdd", lineInfoCollection2.ProductPartAttributesInfos[0].ExpiryDateFormatString); // Date format for testing
			AssertEquals("yyMMdd", lineInfoCollection2.ProductPartAttributesInfos[0].PackingDateFormatString); // Date format for testing
		}

		public void TestConstructor_WithParameters_UsesWarehouseCountryFormatString()
		{
			var now = ZDateTime.Now.ToDateTime();
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);

			var lineInfoCollection1 = new WhsGroupedInventoryLineInfoCollection();
			var groupedInventory = new WhsGroupedInventoryInfo(lineInfoCollection1, new Guid(), data.Org1, data.Part1, data.Whs1, "A1", "A2", "A3", "SN1", now, now, 2m, 2m, 2);

			AssertEquals("ddMMyy", lineInfoCollection1.ProductPartAttributesInfos[0].ExpiryDateFormatString); // Date format for testing
			AssertEquals("ddMMyy", lineInfoCollection1.ProductPartAttributesInfos[0].PackingDateFormatString); // Date format for testing

			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "CN";

			var lineInfoCollection2 = new WhsGroupedInventoryLineInfoCollection();
			groupedInventory = new WhsGroupedInventoryInfo(lineInfoCollection2, new Guid(), data.Org1, data.Part2, data.Whs1, "A1", "A2", "A3", "SN1", now, now, 2m, 2m, 2);

			AssertEquals("yyMMdd", lineInfoCollection2.ProductPartAttributesInfos[0].ExpiryDateFormatString); // Date format for testing
			AssertEquals("yyMMdd", lineInfoCollection2.ProductPartAttributesInfos[0].PackingDateFormatString); // Date format for testing
		}

		#endregion

		#region Properties

		public void TestTotalUnitsOnHand()
		{
			AssertEquals(0m, Parent.TotalUnitsOnHand);

			Parent.TotalUnitsOnHand = 10m;
			AssertEquals("TotalUnitsOnHand", 10m, Parent.TotalUnitsOnHand);
		}

		public void TestTotalUnitsOnAvailable()
		{
			AssertEquals(0m, Parent.TotalUnitsOnAvailable);

			Parent.TotalUnitsOnAvailable = 10m;
			AssertEquals("TotalUnitsOnAvailable", 10m, Parent.TotalUnitsOnAvailable);
		}

		public void TestTotalPalletIDs()
		{
			AssertEquals(0, Parent.TotalPalletIDs);

			Parent.TotalPalletIDs = 10;
			AssertEquals("TotalPalletIDs", 10, Parent.TotalPalletIDs);
		}

		#endregion

		#region Implementation

		protected override WhsInventoryLineBaseInfoCollection<WhsGroupedInventoryInfo> GetCollection()
		{
			return new WhsGroupedInventoryLineInfoCollection();
		}

		protected override WhsGroupedInventoryInfo GetInventoryLineInfo(WhsInventoryLineBaseInfoCollection<WhsGroupedInventoryInfo> collection, WhsInventoryView inventory)
		{
			return new WhsGroupedInventoryInfo(collection, inventory);
		}

		protected new WhsGroupedInventoryInfo Parent
		{
			get { return (WhsGroupedInventoryInfo)base.Parent; }
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsGroupedInventoryInfo();
		}

		#endregion
	}
}
