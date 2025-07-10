using System.Linq;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetWhsInventoryCollectionTest : WhsSecureServiceTestCase
	{
		#region TestGetWhsInventoryCollection

		public void TestGetWhsInventoryCollection()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "P1");
			var receiveLine2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "P2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			factory.Save();

			webService.AllowedToRunServiceHasBeenCalled = false;
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var criteria = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part1.OP_PartNum, "", "", joinCondition: SearchJoinCondition.Or);
			var response = webService.GetWhsInventoryCollection(criteria);
			AssertEquals("Should find out 1 result", 1, response.InventoryInfoCollection.InventoryLineInfos.Count);
			AssertEquals("Should go to Level 1", WhsInventoryLevel.Level1, response.CriteriaInfo.DestinationInventoryLevel);

			var groupedInventory = response.InventoryInfoCollection.InventoryLineInfos[0];
			AssertEquals("ProductPK", data.Part1.PK, groupedInventory.ProductPK);
			AssertEquals("ClientPK", data.Org1.PK, groupedInventory.ClientPK);
			AssertEquals("TotalUnitsOnHand", 20m, groupedInventory.TotalUnitsOnHand);
			AssertEquals("TotalUnitsOnAvailable", 20m, groupedInventory.TotalUnitsOnAvailable);
			AssertEquals("TotalPalletIDs", 2, groupedInventory.TotalPalletIDs);

			var criteria2 = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part1.OP_PartNum, "", "", joinCondition: SearchJoinCondition.Or, destLevel: WhsInventoryLevel.Level3);
			var response2 = webService.GetWhsInventoryCollection(criteria2);
			AssertEquals("Should find out 2 results", 2, response2.InventoryInfoCollection.InventoryLineInfos.Count);
			AssertEquals("The Product not enable attribute, should go to Level 3 directly.", WhsInventoryLevel.Level3, response2.CriteriaInfo.DestinationInventoryLevel);
			AssertInventoryCollectionContains(response2.InventoryInfoCollection, receiveLine1, receiveLine2);
		}

		void AssertInventoryCollectionContains(WhsGroupedInventoryLineInfoCollection collection, params WhsInventoryView[] inventory)
		{
			AssertContainsExactElementsInAnyOrder("The inventory you were looking for was not found in the collection.", inventory.Select(i => i.PK), collection.InventoryLineInfos.Select(i => i.PK));
		}

		#endregion

		#region TestGetWhsInventoryCollection_UsesWarehouseCountryFormatString

		public void TestGetWhsInventoryCollection_UsesWarehouseCountryFormatString()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "P1");
			var receiveLine2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "P2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure receive is finalised", true, receive.IsFinalised);

			helper.SetClientAllAttributeType(data.Org1, true);
			helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);

			factory.Save();

			webService.AllowedToRunServiceHasBeenCalled = false;
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var criteria1 = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part1.OP_PartNum, "", "", joinCondition: SearchJoinCondition.Or);
			var response1 = webService.GetWhsInventoryCollection(criteria1);

			AssertEquals("ddMMyy", response1.InventoryInfoCollection.ProductPartAttributesInfos[0].ExpiryDateFormatString);
			AssertEquals("ddMMyy", response1.InventoryInfoCollection.ProductPartAttributesInfos[0].PackingDateFormatString);

			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "CN";
			Helper.Factory.Save();

			var criteria2 = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part2.OP_PartNum, "", "", joinCondition: SearchJoinCondition.Or);
			var response2 = webService.GetWhsInventoryCollection(criteria2);

			AssertEquals("yyMMdd", response2.InventoryInfoCollection.ProductPartAttributesInfos[0].ExpiryDateFormatString);
			AssertEquals("yyMMdd", response2.InventoryInfoCollection.ProductPartAttributesInfos[0].PackingDateFormatString);
		}

		#endregion
	}
}
