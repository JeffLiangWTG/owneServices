using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PutawayAutoAllocateInventoryTest : WhsSecureServiceTestCase
	{
		#region Putaway_AutoAllocateInventory

		#region TestPutaway_AutoAllocateInventory

		public void TestPutaway_AutoAllocateInventory()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PL1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 40m, null, "PL1");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 50m, null, "PL2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 20m, null, "PL1");
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 30m, null, "PL2");

			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory1.Location);
			AssertNull("Precondition: location should be empty.", inventory2.Location);
			AssertNull("Precondition: location should be empty.", inventory3.Location);
			AssertNull("Precondition: location should be empty.", inventory4.Location);
			AssertNull("Precondition: location should be empty.", inventory5.Location);

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					AssertEquals("Should use useLocationConcurrencyHandling.", true, useLocationConcurrencyHandling);

					foreach (var inv in receiveLines)
					{
						inv.WE_WL = data.Whs1.DefaultLocation.PK;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService1 = GetNewWebService(data.Whs1);
				var response1 = webService1.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
				AssertEquals(false, string.IsNullOrEmpty(response1.Location));
				AssertEquals(response1.Location, inventory1.LocationString);
				AssertEquals(response1.Location, inventory2.LocationString);
				AssertNull(inventory3.Location);
				AssertEquals(response1.Location, inventory4.LocationString);
				AssertNull(inventory5.Location);
				AssertEquals(data.Whs1.DefaultLocation.PK, response1.LocationPK);

				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_AutoAllocateInventory("PL2", "");
				AssertEquals(false, string.IsNullOrEmpty(response2.Location));
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertNotNull(inventory1.Location);
				AssertNotNull(inventory2.Location);
				AssertEquals(response2.Location, inventory3.LocationString);
				AssertNotNull(inventory4.Location);
				AssertEquals(response2.Location, inventory5.LocationString);
				AssertEquals(data.Whs1.DefaultLocation.PK, response2.LocationPK);
			}
		}

		public void TestPutaway_AutoAllocateInventory_FixedWidthLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "ABC", 5, 5, 5);

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PL1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 40m, null, "PL1");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 50m, null, "PL2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, warehouse, "INW2");
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 20m, null, "PL1");
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 30m, null, "PL2");

			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory1.Location);
			AssertNull("Precondition: location should be empty.", inventory2.Location);
			AssertNull("Precondition: location should be empty.", inventory3.Location);
			AssertNull("Precondition: location should be empty.", inventory4.Location);
			AssertNull("Precondition: location should be empty.", inventory5.Location);

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						inv.WE_WL = warehouse.FindLocation("ABC020202").PK;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService1 = GetNewWebService(warehouse);
				var response1 = webService1.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
				AssertEquals(false, string.IsNullOrEmpty(response1.Location));
				AssertEquals("ABC020202", response1.Location);
				AssertEquals("ABC-02-02-02", response1.LocationUserFriendly);
				AssertEquals(response1.LocationUserFriendly, inventory1.LocationString);
				AssertNull(inventory3.Location);
				AssertEquals(response1.LocationUserFriendly, inventory4.LocationString);
				AssertNull(inventory5.Location);

				var webService2 = GetNewWebService(warehouse);
				var response2 = webService2.Putaway_AutoAllocateInventory("PL2", "");
				AssertEquals(false, string.IsNullOrEmpty(response2.Location));
				AssertEquals("ABC020202", response2.Location);
				AssertEquals("ABC-02-02-02", response2.LocationUserFriendly);
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertNotNull(inventory1.Location);
				AssertNotNull(inventory2.Location);
				AssertEquals(response2.LocationUserFriendly, inventory3.LocationString);
				AssertNotNull(inventory4.Location);
				AssertEquals(response2.LocationUserFriendly, inventory5.LocationString);
			}
		}

		#endregion

		#region TestPutaway_AutoAllocateInventory_WithPutawayRulesEngine

		public void TestPutaway_AutoAllocateInventory_WithPutawayRulesEngine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Helper.Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Helper.Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = "Rules";
			ruleSet.PRS_Description = "Rules";
			ruleSet.PRS_Context = "PWP";
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;

			var rule = Helper.Factory.New<ProductionRule>();
			rule.PRL_PRS_RuleSet = ruleSet.PK;
			rule.PRL_Name = $"RULE";
			rule.PRL_Description = $"RULE";
			rule.PRL_Priority = 1;
			rule.PRL_RuleDefinition = $@"{{
    ""conditions"": [
    ],
    ""action"": {{
        ""$type"": ""PutawayActionState"",
        ""conditions"": [
        ],
        ""sortByCriteria"": [
        ]
    }}
}}";

			new WhsPutawayLocationCacheManager().CreateCache(Helper.Factory, data.Whs1.FindLocation("A").PK);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PL1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 40m, null, "PL1");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 50m, null, "PL2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 20m, null, "PL1");
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 30m, null, "PL2");

			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory1.Location);
			AssertNull("Precondition: location should be empty.", inventory2.Location);
			AssertNull("Precondition: location should be empty.", inventory3.Location);
			AssertNull("Precondition: location should be empty.", inventory4.Location);
			AssertNull("Precondition: location should be empty.", inventory5.Location);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService1 = GetNewWebService();
				webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				var response1 = webService1.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
				AssertEquals(false, string.IsNullOrEmpty(response1.Location));
				AssertEquals(response1.Location, inventory1.LocationString);
				AssertEquals(response1.Location, inventory2.LocationString);
				AssertNull(inventory3.Location);
				AssertEquals(response1.Location, inventory4.LocationString);
				AssertNull(inventory5.Location);

				var webService2 = GetNewWebService();
				webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				var response2 = webService2.Putaway_AutoAllocateInventory("PL2", "");
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals(false, string.IsNullOrEmpty(response2.Location));
				AssertNotNull(inventory1.Location);
				AssertNotNull(inventory2.Location);
				AssertEquals(response2.Location, inventory3.LocationString);
				AssertNotNull(inventory4.Location);
				AssertEquals(response2.Location, inventory5.LocationString);
			}
		}

		#endregion

		#region TestPutaway_AutoAllocateInventory_WithPutawayRulesEngine_Equipment

		public void TestPutaway_AutoAllocateInventory_WithPutawayRulesEngine_Equipment()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.Rows.First().Locations.First();

			var equipment = Helper.CreateEquipment("TRK", 1m, "KG", 1m, "M3");
			equipment.RQ_Registration = "TR1";

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PL1");

			Helper.Factory.Save();
			AssertNull("Precondition: location should be empty.", inventory1.Location);

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive1.PK),
				receiveLinesCondition: receiveLines => receiveLines.Single().PK == inventory1.PK,
				refEquipmentCondition: refEquipment => refEquipment.PK == equipment.PK,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) => receiveLines.Single().WE_WL = location.PK);

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService1 = GetNewWebService();
				webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				var response1 = webService1.Putaway_AutoAllocateInventory("PL1", "TR1");
				AssertEquals(false, string.IsNullOrEmpty(response1.Location));
				AssertEquals(response1.Location, inventory1.LocationString);
			}

			mockedEngine.Verify(re => re.Putaway(
				It.Is<IEnumerable<WhsReceive>>(r => r.Any(d => d.PK == receive1.PK)),
				It.Is<IEnumerable<WhsReceiveLine>>(invs => invs.Single().PK == inventory1.PK),
				It.IsAny<INotifications>(),
				It.Is<RefEquipment>(e => e.PK == equipment.PK),
				It.IsAny<IEnumerable<ZGuid>>(),
				It.IsAny<bool>(),
				It.IsAny<bool>()), Times.Once);
		}

		#endregion

		#region TestPutaway_AutoAllocateInventory_WithPutawayRulesEngine_NullEquipment

		public void TestPutaway_AutoAllocateInventory_WithPutawayRulesEngine_NullEquipment_Null()
		{
			TestPutaway_AutoAllocateInventory_WithPutawayRulesEngine_NullEquipment_Core(null);
		}

		public void TestPutaway_AutoAllocateInventory_WithPutawayRulesEngine_NullEquipment_Empty()
		{
			TestPutaway_AutoAllocateInventory_WithPutawayRulesEngine_NullEquipment_Core("");
		}

		public void TestPutaway_AutoAllocateInventory_WithPutawayRulesEngine_NullEquipment_NonExistent()
		{
			TestPutaway_AutoAllocateInventory_WithPutawayRulesEngine_NullEquipment_Core("LOL");
		}

		void TestPutaway_AutoAllocateInventory_WithPutawayRulesEngine_NullEquipment_Core(string code)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.Rows.First().Locations.First();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PL1");

			Helper.Factory.Save();
			AssertNull("Precondition: location should be empty.", inventory1.Location);

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive1.PK),
				receiveLinesCondition: receiveLines => receiveLines.Single().PK == inventory1.PK,
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) => receiveLines.Single().WE_WL = location.PK);

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService1 = GetNewWebService();
				webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				var response1 = webService1.Putaway_AutoAllocateInventory("PL1", code);
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
				AssertEquals(false, string.IsNullOrEmpty(response1.Location));
				AssertEquals(response1.Location, inventory1.LocationString);
			}

			mockedEngine.Verify(re => re.Putaway(
				It.Is<IEnumerable<WhsReceive>>(r => r.Any(d => d.PK == receive1.PK)),
				It.Is<IEnumerable<WhsReceiveLine>>(invs => invs.Single().PK == inventory1.PK),
				It.IsAny<INotifications>(),
				null,
				It.IsAny<IEnumerable<ZGuid>>(),
				It.IsAny<bool>(),
				It.IsAny<bool>()), Times.Once);
		}

		#endregion

		#region TestPutaway_AutoAllocateInventory_WithPutawayRulesEngine_Errors

		public void TestPutaway_AutoAllocateInventory_WithPutawayRulesEngine_Errors()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.Rows.First().Locations.First();

			var equipment = Helper.CreateEquipment("TRk", 1m, "KG", 1m, "M3");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PL1");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 20m, null, "PL1");

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW3");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 20m, null, "PL1");

			Helper.Factory.Save();
			AssertNull("Precondition: location should be empty.", inventory1.Location);
			AssertNull("Precondition: location should be empty.", inventory2.Location);
			AssertNull("Precondition: location should be empty.", inventory3.Location);

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Count() == 3,
				receiveLinesCondition: receiveLines => receiveLines.Count() == 3,
				refEquipmentCondition: refEquipment => refEquipment.PK == equipment.PK,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					var errors = new Queue<string>();
					errors.Enqueue("Error1");
					errors.Enqueue("Error1");
					errors.Enqueue("Error2");
					receiveLines.ForEach(
					i =>
					{
						i.WE_WL = location.PK;
						notifications.AddError(errors.Dequeue());
					});
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService1 = GetNewWebService();
				webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				var response1 = webService1.Putaway_AutoAllocateInventory("PL1", "TRK");
				AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
				AssertEquals("Error1\r\nError2", response1.ErrorMessage);
				AssertEquals(true, string.IsNullOrEmpty(response1.Location));
			}

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var inventory1InNewFactory = newFactory.Load<WhsInventoryView>(inventory1.PK);
			var inventory2InNewFactory = newFactory.Load<WhsInventoryView>(inventory2.PK);
			var inventory3InNewFactory = newFactory.Load<WhsInventoryView>(inventory3.PK);
			AssertNull("Should *not* have saved if there was an error.", inventory1InNewFactory.Location);
			AssertNull("Should *not* have saved if there was an error.", inventory2InNewFactory.Location);
			AssertNull("Should *not* have saved if there was an error.", inventory3InNewFactory.Location);

			mockedEngine.Verify(re => re.Putaway(
				It.Is<IEnumerable<WhsReceive>>(r => r.Count() == 3),
				It.Is<IEnumerable<WhsReceiveLine>>(i => i.Count() == 3),
				It.IsAny<INotifications>(),
				It.Is<RefEquipment>(e => e.PK == equipment.PK),
				It.IsAny<IEnumerable<ZGuid>>(),
				It.IsAny<bool>(),
				It.IsAny<bool>()));
		}

		#endregion

		#region TestPutaway_AutoAllocateInventoryMultipleWarehouses

		public void TestPutaway_AutoAllocateInventoryMultipleWarehouses()
		{
			var whs1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "A", 2, 1);
			var client = Helper.CreateClient();
			var product = Helper.CreateProduct(client, "P1");

			var receive1 = Helper.CreateWhsReceive(client, whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, product, 10m, null, "PL1");
			var receive2 = Helper.CreateWhsReceive(client, whs2, "R2");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, product, 8m, null, "PL1");
			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory1.Location);
			AssertNull("Precondition: location should be empty.", inventory2.Location);
			
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive();

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				// call Putaway_AutoAllocateInventory for Whs2
				var webService1 = GetNewWebService(whs2);
				var response1 = webService1.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals("A-1", response1.Location);
				AssertEquals("First inventory is in the first warehouse and should stay unallocated", ZGuid.Empty, inventory1.WI_WL);
				AssertEquals(whs2.DefaultLocation.PK, inventory2.WI_WL);

				var webService2 = GetNewWebService();
				webService2.SecurityHeader.WarehouseCode = whs1.WW_WarehouseCode;
				var response2 = webService2.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals("A-1", response2.Location);
				AssertEquals(whs1.DefaultLocation.PK, inventory1.WI_WL);
				AssertEquals(whs2.DefaultLocation.PK, inventory2.WI_WL);
			}
		}

		#endregion

		#endregion

		#region TestPutaway_AutoAllocateInventory_WithNoArrivalDate

		[TestDate(2017, 8, 11, 2, 1, 0)]
		public void TestPutaway_AutoAllocateInventory_WithNoArrivalDate()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PLT1");
			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory.Location);
			AssertEquals("Precondition", InventoryStatus.Codes.Pending, inventory.InDocketLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", 10m, inventory.InDocketLine.WE_StockOnHand);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, receive.WD_ArrivalDate);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, inventory.InDocketLine.WE_AdjustmentArrivalDate);

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive();

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService = GetNewWebService(data.Whs1);
				var response = webService.Putaway_AutoAllocateInventory("PLT1", "");
				AssertEquals("A", response.Location);
				AssertEquals(data.Whs1.DefaultLocation.PK, inventory.WI_WL);
				AssertEquals(ZDateTimeOffset.Now, receive.WD_ArrivalDate);
				AssertEquals(ZDateTimeOffset.Now, receive.Lines[0].WE_AdjustmentArrivalDate);
			}
		}

		#endregion

		#region TestPutaway_AutoAllocateInventory_BondedPallet

		public void TestPutaway_AutoAllocateInventory_BondedPallet()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var bondedLocation = data.Whs1.FindLocation("A-2");
			bondedLocation.WLV_WA_PutawayArea = bondedArea.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, null, "PLT1");
			Helper.Factory.Save();

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive.PK),
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) => receiveLines.ForEach(i => i.WE_WL = data.Whs1.FindLocation("A-2").PK));

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.Putaway_AutoAllocateInventory("PLT1", "");
				AssertEquals("Should allocate bonded location automatically", "A-2", response.Location);
				AssertEquals("Should allocate bonded location automatically", response.Location, inventory.LocationString);
			}
		}

		#endregion

		#region TestPutaway_AutoAllocateInventory_PalletId

		public void TestPutaway_AutoAllocateInventory_PalletId_NoPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");
			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory.Location);

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive.PK));

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService = GetNewWebService(data.Whs1);
				var response = webService.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals(false, string.IsNullOrEmpty(response.Location));
				AssertEquals(response.Location, inventory.LocationString);
				AssertEquals("PL1", response.PalletID);
			}
		}

		public void TestPutaway_AutoAllocateInventory_PalletId_WithPutawayTransfer_SamePalletId()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");
			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory.Location);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			AssertEquals(ErrorTypes.None, response1.Error);

			AssertEquals("Precondition: inventory has putaway transfer.", true, inventory.HasPutawayTransfer);

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive.PK),
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) => receiveLines.Single().PutawayTransferLine.WE_WL = data.Whs1.DefaultLocation.PK);

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals(false, string.IsNullOrEmpty(response2.Location));
				AssertEquals(response2.Location, ((WhsReceiveLine)inventory.InDocketLine).PutawayTransferLine.LocationString);
				AssertEquals("PL1", response2.PalletID);
			}
		}

		public void TestPutaway_AutoAllocateInventory_PalletId_WithPutawayTransfer_NewPalletId()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.Rows.First().Locations.First();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");
			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory.Location);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			AssertEquals(ErrorTypes.None, response1.Error);

			AssertEquals("Precondition: inventory has putaway transfer.", true, inventory.HasPutawayTransfer);
			AssertNotNull("Precondition: putaway transfer line is not null.", receive.Lines[0].PutawayTransferLine);

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive.PK),
				receiveLinesCondition: receiveLines => receiveLines.Single().PK == inventory.PK,
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					var putawayTransferLine = receiveLines.Single().PutawayTransferLine;
					putawayTransferLine.WE_WL = location.PK;
					putawayTransferLine.WE_PalletID = "PL2";
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals(false, string.IsNullOrEmpty(response2.Location));
				AssertEquals(response2.Location, ((WhsReceiveLine)inventory.InDocketLine).PutawayTransferLine.LocationString);
				AssertEquals("PL2", response2.PalletID);
			}
		}

		public void TestPutaway_AutoAllocateInventory_PalletId_EndToEndWithPutawayEngineManager()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, "UNT", "PLT", 10000);
			Helper.Factory.Save();

			var location = data.Whs1.FindLocation("A-1");
			var palletId = "ABC999";
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, location, palletId, allocateLocations: false);
			Helper.Factory.Save();

			AssertIsFinalisedPrecondition(receive1);

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Helper.Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Helper.Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = "Rules";
			ruleSet.PRS_Description = "Rules";
			ruleSet.PRS_Context = "PWP";
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;
			GetRule(ruleSet, 1, data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, palletId);

			new WhsPutawayLocationCacheManager().CreateCache(Helper.Factory, new[] { location.PK });
			Helper.Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, null, "PL1");
			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory.Location);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			AssertEquals(ErrorTypes.None, response1.Error);

			AssertEquals("Precondition: inventory has putaway transfer.", true, inventory.HasPutawayTransfer);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals(false, string.IsNullOrEmpty(response2.Location));
				AssertEquals(response2.Location, ((WhsReceiveLine)inventory.InDocketLine).PutawayTransferLine.LocationString);
				AssertEquals("ABC999", response2.PalletID);
			}
		}

		public void TestPutaway_AutoAllocateInventory_PalletId_WithPutawayTransferAndCrossDockedLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			order.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine = order.Lines[0].ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedLine.ReservedQuantity);
			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory.Location);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			AssertEquals(ErrorTypes.None, response1.Error);

			AssertEquals("Precondition: inventory has putaway transfer.", true, inventory.HasPutawayTransfer);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals("XDOCK", response2.Location);
				AssertEquals("PL1", response2.PalletID);
			}
		}

		public void TestPutaway_AutoAllocateInventory_PalletId_WithPutawayTransferAndMultipleCrossDockedLocations()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 6m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 4m);
			var crossDockLocation1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK1").Locations.Single();
			var crossDockLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK2").Locations.Single();
			order1.WD_WL_CrossDock = crossDockLocation1.PK;
			order2.WD_WL_CrossDock = crossDockLocation2.PK;
			var reservedLine1 = order1.Lines[0].ReserveStockIfAbleTo(inventory);
			var reservedLine2 = order2.Lines[0].ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 6m, reservedLine1.ReservedQuantity);
			AssertEquals("Precondition: Stock is reserved.", 4m, reservedLine2.ReservedQuantity);
			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory.Location);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			AssertEquals(ErrorTypes.None, response1.Error);

			AssertEquals("Precondition: inventory has putaway transfer.", true, inventory.HasPutawayTransfer);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
				AssertEquals("Error occurred while loading data: Cannot Cross Dock a Single Pallet to multiple Cross Dock Locations.", response2.ErrorMessage);
				AssertEquals("", response2.Location);
				AssertEquals("", response2.PalletID);
			}
		}

		ProductionRule GetRule(ProductionRuleSet ruleSet, int number, ZGuid productPk, string palletId)
		{
			var rule = Helper.Factory.New<ProductionRule>();
			rule.PRL_PRS_RuleSet = ruleSet.PK;
			rule.PRL_Name = $"RULE{number}";
			rule.PRL_Description = $"RULE{number}";
			rule.PRL_Priority = 1;
			rule.PRL_RuleDefinition = $@"{{
    ""conditions"": [
        {{
            ""fieldPath"": ""Product"",
            ""operation"": ""equals"",
            ""value"": ""{productPk}""
        }}
    ],
    ""action"": {{
        ""$type"": ""PutawayActionState"",
        ""conditions"": [
            {{
                ""fieldPath"": ""PartialPalletID"",
                ""operation"": ""equals"",
                ""value"": ""{palletId}""
            }}
        ],
        ""sortByCriteria"": [
        ]
    }}
}}";
			return rule;
		}

		#endregion

		#region TestPutaway_AutoAllocateInventory_PalletAcrossReceivesWithTransferLines

		public void TestPutaway_AutoAllocateInventory_PalletAcrossReceivesWithTransferLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.Rows.First().Locations.First();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PL1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, null, "PL1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			Helper.Factory.Save();

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						inv.PutawayTransferLine.WE_WL = location.PK;
					}
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals(location.PK, ((WhsReceiveLine)inventory1.InDocketLine).PutawayTransferLine.Location.PK);
				AssertEquals(location.PK, ((WhsReceiveLine)inventory2.InDocketLine).PutawayTransferLine.Location.PK);
			}
		}

		#endregion

		#region TestPutaway_ReallocationInventory_WithSkipLocationPKs

		public void TestPutaway_ReallocationInventory_WithSkipLocationPKs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.FindLocation("A");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, location, "PL1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 40m, location, "PL1");

			Helper.Factory.Save();
			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);

			IEnumerable<System.Guid> skipLocationPKsPassedIn = null;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					AssertEquals("Should use useLocationConcurrencyHandling.", true, useLocationConcurrencyHandling);

					foreach (var inv in receiveLines)
					{
						AssertEquals(ZGuid.Empty, inv.PutawayTransferLine.WE_WL);
						inv.PutawayTransferLine.WE_WL = data.Whs1.DefaultLocation.PK;
					}
					skipLocationPKsPassedIn = skipLocations.Select(x => x.ToGuid()).ToArray();
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var skipLocationPKs = new System.Guid[] { System.Guid.NewGuid(), System.Guid.NewGuid() };
				var response2 = webService2.Putaway_ReallocateInventory("PL1", "", skipLocationPKs);
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals(data.Whs1.DefaultLocation.ToLocationString(), response2.Location);
				AssertEquals(data.Whs1.DefaultLocation.PK, response2.LocationPK);
				AssertContainsExactElementsInExactOrder(skipLocationPKs, skipLocationPKsPassedIn);
			}
		}

		#endregion

		#region TestPutaway_ReallocationInventory_ValidateTransferLines

		public void TestPutaway_ReallocationInventory_ClearPalletIDForConsolidatedPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.Rows.First().Locations.First();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);

			AssertEquals("Precondition: inventory has putaway transfer.", true, inventory.HasPutawayTransfer);
			AssertNotNull("Precondition: putaway transfer line is not null.", receive.Lines[0].PutawayTransferLine);
			receive.Lines[0].PutawayTransferLine.WE_PalletID = "PL2";
			Helper.Factory.Save();

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive.PK),
				receiveLinesCondition: receiveLines => receiveLines.Single().PK == inventory.PK,
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) => receiveLines.Single().PutawayTransferLine.WE_WL = location.PK);

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_ReallocateInventory("PL1", "", new System.Guid[] { new System.Guid() });
				AssertEquals("PL1", ((WhsReceiveLine)inventory.InDocketLine).PutawayTransferLine.WE_PalletID);
			}
		}

		public void TestPutaway_ReallocationInventory_DoNotClearPalletIDWhenIDInDifferentLetterCase()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.Rows.First().Locations.First();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);

			AssertEquals("Precondition: inventory has putaway transfer.", true, inventory.HasPutawayTransfer);
			AssertNotNull("Precondition: putaway transfer line is not null.", receive.Lines[0].PutawayTransferLine);
			receive.Lines[0].PutawayTransferLine.WE_PalletID = "pl1";
			Helper.Factory.Save();

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive.PK),
				receiveLinesCondition: receiveLines => receiveLines.Single().PK == inventory.PK,
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) => receiveLines.Single().PutawayTransferLine.WE_WL = location.PK);

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_ReallocateInventory("PL1", "", new System.Guid[] { new System.Guid() });
				AssertEquals("pl1", ((WhsReceiveLine)inventory.InDocketLine).PutawayTransferLine.WE_PalletID);
			}
		}

		public void TestPutaway_ReallocationInventory_ClearPalletIDOnlyForReceivePutaway()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.Rows.First().Locations.First();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PL1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, null, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new string[] { "PL1", "PL2" }, false);
			receive1.Lines[0].PutawayTransferLine.WE_PalletID = "PL3";
			receive2.Lines[0].PutawayTransferLine.WE_PalletID = "PL3";
			Helper.Factory.Save();

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) => receiveLines.Single().PutawayTransferLine.WE_WL = location.PK);

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_ReallocateInventory("PL1", "", new System.Guid[] { new System.Guid() });
				AssertEquals("PL1", ((WhsReceiveLine)inventory1.InDocketLine).PutawayTransferLine.WE_PalletID);
				AssertEquals("PL3", ((WhsReceiveLine)inventory2.InDocketLine).PutawayTransferLine.WE_PalletID);
			}
		}

		public void TestPutaway_ReallocationInventory_ValidateTransferLineFinalized()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, null, "PL1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);

			var transferLine = receive.Lines[0].PutawayTransferLine;
			AssertNotNull("Precondition: putaway transfer line is not null.", transferLine);
			Helper.Factory.Save();

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive.PK),
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					receiveLines.ForEach(x =>
					{
						x.PutawayTransferLine.WE_WL = data.Whs1.DefaultLocation.PK;
					});
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals(false, string.IsNullOrEmpty(response2.Location));

				transferLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(transferLine);

				Helper.Factory.Save();
				var reallocateResponse = webService2.Putaway_ReallocateInventory("PL1", "", null);
				AssertEquals("Putaway has been already completed for the Pallet ID PL1.", reallocateResponse.ErrorMessage);
			}
		}

		#endregion

		#region TestPutaway_ReallocateInventory_PalletAcrossReceives_UpdateLocation

		public void TestPutaway_ReallocateInventory_PalletAcrossReceives_UpdateLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.Rows.First().Locations.First();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PL1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, null, "PL1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			Helper.Factory.Save();

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						inv.PutawayTransferLine.WE_WL = location.PK;
					}
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_ReallocateInventory("PL1", "", new System.Guid[] { new System.Guid() });
				AssertEquals(location.PK, ((WhsReceiveLine)inventory1.InDocketLine).PutawayTransferLine.Location.PK);
				AssertEquals(location.PK, ((WhsReceiveLine)inventory2.InDocketLine).PutawayTransferLine.Location.PK);
			}
		}

		#endregion

		#region TestPutaway_AllocateAndReallocate_EndToEnd

		public void TestPutaway_AllocateAndReallocate_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(data.Org1, data.Whs1);
			whsClientParameterByWarehouse1.WY_CycleCountOnAlternatePutaway = true;
			Helper.CreateProductUnit(data.Part1, "UNT", "PLT", 10000);
			Helper.Factory.Save();

			var location = data.Whs1.FindLocation("A-1");
			new WhsPutawayLocationCacheManager().CreateCache(Helper.Factory, new[] { location.PK });
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");
			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory.Location);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals("Precondition: inventory has putaway transfer.", true, inventory.HasPutawayTransfer);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertEquals(false, string.IsNullOrEmpty(response2.Location));
				AssertEquals("A-1", response2.Location);
				AssertEquals(response2.Location, ((WhsReceiveLine)inventory.InDocketLine).PutawayTransferLine.LocationString);

				var skipLocationPKs = new System.Guid[] { response2.LocationPK };
				var response = webService2.Putaway_ReallocateInventory("PL1", "", skipLocationPKs);
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals(false, string.IsNullOrEmpty(response.Location));
				AssertEquals("A-2", response.Location);

				var factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				var createCycleCounts = factory.Load<WhsCycleCountLocation>(new ZQuery());
				AssertEquals("Only 1 task should be created", 1, createCycleCounts.Length);

				var cycleCountTask = createCycleCounts[0];
				AssertEquals("1 Cycle Count should be created to the specified location", location.PK, cycleCountTask.WCL_WL_Location);
				AssertEquals("1 Cycle Count should be created to the specified granularity", CycleCountGranularity.Codes.ProductWithAttributes, cycleCountTask.WCL_Granularity);
				AssertEquals("1 Cycle Count should be created to the specified priority", (byte)1, cycleCountTask.WCL_Priority);
			}
		}

		#endregion

		#region TestPutaway_AllocateLocationConcurrency

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestPutaway_AllocateLocationConcurrency_ChangeIDInLocationCacheMaxRetry()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.Rows.First().Locations.First();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PL1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, null, "PL1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			Helper.Factory.Save();

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) => throw new PutawayAllocateLocationConcurrencyException());

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_AutoAllocateInventory("PL1", "");
				AssertNull(((WhsReceiveLine)inventory1.InDocketLine).PutawayTransferLine.Location);

				mockedEngine.Verify(re => re.Putaway(
					It.IsAny<IEnumerable<WhsReceive>>(),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					It.IsAny<INotifications>(),
					null,
					It.IsAny<IEnumerable<ZGuid>>(),
					It.IsAny<bool>(),
					It.IsAny<bool>()), Times.Exactly(5));
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestPutaway_AllocateLocationConcurrency_ChangeIDInLocationCacheRetryThreeTimes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.Rows.First().Locations.First();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PL1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, null, "PL1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			Helper.Factory.Save();

			var exceptionToThrow = 2;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					if (exceptionToThrow-- > 0)
					{
						throw new PutawayAllocateLocationConcurrencyException();
					}
					else
					{
						foreach (var inv in receiveLines)
						{
							inv.PutawayTransferLine.WE_WL = location.PK;
						}
					}
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals(location.PK, ((WhsReceiveLine)inventory1.InDocketLine).PutawayTransferLine.Location.PK);
				AssertEquals(location.PK, ((WhsReceiveLine)inventory2.InDocketLine).PutawayTransferLine.Location.PK);

				mockedEngine.Verify(re => re.Putaway(
					It.IsAny<IEnumerable<WhsReceive>>(),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					It.IsAny<INotifications>(),
					null,
					It.IsAny<IEnumerable<ZGuid>>(),
					It.IsAny<bool>(),
					It.IsAny<bool>()), Times.Exactly(3));
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestPutaway_AllocateLocationConcurrency_AllcateOnTwoConnections()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.FindLocation("A");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PL1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, null, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			webService1.Factory.RefreshEnabled = false;
			webService1.ValidatePalletIDOnPutaway("PL1", false);
			webService1.ValidatePalletIDOnPutaway("PL2", false);
			Helper.Factory.Save();

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var allocatedInSecondConnection = false;
				var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					var inv = receiveLines.Single();

					// Mimic setting location ConcurrencyPolicy to strict as PutawayEngineManager does.
					var locationInInventoryFactory = inv.Factory.Load<WhsLocation>(location.PK);
					ConcurrencyInfo.SetConcurrencyPolicy(locationInInventoryFactory, nameof(WhsLocationViewSchema.WLV_LastAllocatedOrChangedID), ConcurrencyPolicy.Strict);

					inv.PutawayTransferLine.WE_WL = location.PK;

					if (!allocatedInSecondConnection)
					{
						allocatedInSecondConnection = true;
						var webservice2 = GetNewWebService(data.Whs1);
						webservice2.Factory.RefreshEnabled = false;
						webservice2.Putaway_AutoAllocateInventory("PL2", "");
					}
				});

				using (ObjectFactory.Substitute(mockedEngine.Object))
				{
					webService1.Putaway_AutoAllocateInventory("PL1", "");
					inventory1 = webService1.Factory.Load<WhsReceiveLine>(inventory1.PK).Inventory[0];
					AssertEquals(location.PK, ((WhsReceiveLine)inventory1.InDocketLine).PutawayTransferLine.Location.PK);

					mockedEngine.Verify(re => re.Putaway(
						It.IsAny<IEnumerable<WhsReceive>>(),
						It.IsAny<IEnumerable<WhsReceiveLine>>(),
						It.IsAny<INotifications>(),
						null,
						It.IsAny<IEnumerable<ZGuid>>(),
						It.IsAny<bool>(),
						It.IsAny<bool>()), Times.Exactly(3),
						"Putaway should happen 3 times, 1 for connection2 and succeeds, 2 for connection1 and fails, 3 for connection1 and retry succeeds.");
				}
			}
		}

		#endregion

		#region TestSkipLocationCacheUpdate

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestPutaway_AutoAllocateInventory_SkipLocationCacheUpdate()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.Rows.First().Locations.First();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PL1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, null, "PL1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			Helper.Factory.Save();

			var runTimes = 0;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					runTimes++;
					if (runTimes == 1)
					{
						Assert("should shortcircuit rebuilding the cache on the first run of Allocate when it is on RF device", !needRebuildLocationCache);
						throw new PutawayAllocateLocationConcurrencyException();
					}
					else
					{
						Assert(needRebuildLocationCache);
						foreach (var inv in receiveLines)
						{
							inv.PutawayTransferLine.WE_WL = location.PK;
						}
					}
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_AutoAllocateInventory("PL1", "");
				AssertEquals(location.PK, ((WhsReceiveLine)inventory1.InDocketLine).PutawayTransferLine.Location.PK);
				AssertEquals(location.PK, ((WhsReceiveLine)inventory2.InDocketLine).PutawayTransferLine.Location.PK);
				AssertEquals("Should execute allocate twice in total", 2, runTimes);
				mockedEngine.Verify(re => re.Putaway(
					It.IsAny<IEnumerable<WhsReceive>>(),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					It.IsAny<INotifications>(),
					null,
					It.IsAny<IEnumerable<ZGuid>>(),
					It.IsAny<bool>(),
					It.IsAny<bool>()), Times.Exactly(2));
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestPutaway_ReallocateInventory_SkipLocationCacheUpdate()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.Rows.First().Locations.First();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PL1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, null, "PL1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			Helper.Factory.Save();

			var runTimes = 0;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					runTimes++;
					if (runTimes == 1)
					{
						Assert("should shortcircuit rebuilding the cache on the first run of Allocate when it is on RF device", !needRebuildLocationCache);
						throw new PutawayAllocateLocationConcurrencyException();
					}
					else
					{
						Assert(needRebuildLocationCache);
						foreach (var inv in receiveLines)
						{
							inv.PutawayTransferLine.WE_WL = location.PK;
						}
					}
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_ReallocateInventory("PL1", "", null);
				AssertEquals(location.PK, ((WhsReceiveLine)inventory1.InDocketLine).PutawayTransferLine.Location.PK);
				AssertEquals(location.PK, ((WhsReceiveLine)inventory2.InDocketLine).PutawayTransferLine.Location.PK);
				AssertEquals("Should execute allocate twice in total", 2, runTimes);
				mockedEngine.Verify(re => re.Putaway(
					It.IsAny<IEnumerable<WhsReceive>>(),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					It.IsAny<INotifications>(),
					null,
					It.IsAny<IEnumerable<ZGuid>>(),
					It.IsAny<bool>(),
					It.IsAny<bool>()), Times.Exactly(2));
			}
		}

		#endregion

		#region TestPutaway_ReallocateInventory_AnyErrorOrWarningOnFirstAttempt

		public void TestPutaway_ReallocateInventory_AnyErrorOrWarningOnFirstAttempt_NoErrorOrWarning() => TestPutaway_ReallocateInventory_AnyErrorOrWarningOnFirstAttempt(hasErrorOnFirstAttempt: false, hasWarningOnFirstAttempt: false, shouldRetry: false);

		public void TestPutaway_ReallocateInventory_AnyErrorOrWarningOnFirstAttempt_HasError() => TestPutaway_ReallocateInventory_AnyErrorOrWarningOnFirstAttempt(hasErrorOnFirstAttempt: true, hasWarningOnFirstAttempt: false, shouldRetry: true);

		public void TestPutaway_ReallocateInventory_AnyErrorOrWarningOnFirstAttempt_HasWarning() => TestPutaway_ReallocateInventory_AnyErrorOrWarningOnFirstAttempt(hasErrorOnFirstAttempt: false, hasWarningOnFirstAttempt: true, shouldRetry: true);

		public void TestPutaway_ReallocateInventory_AnyErrorOrWarningOnFirstAttempt_OnlyRetryOnce() => TestPutaway_ReallocateInventory_AnyErrorOrWarningOnFirstAttempt(hasErrorOnFirstAttempt: true, hasWarningOnFirstAttempt: true, shouldRetry: true, edgeCaseOccurrenceTimes: 3);

		[UseSnapshotProtection(skipTransaction: true)]
		void TestPutaway_ReallocateInventory_AnyErrorOrWarningOnFirstAttempt(bool hasErrorOnFirstAttempt, bool hasWarningOnFirstAttempt, bool shouldRetry, int edgeCaseOccurrenceTimes = 1)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.Rows.First().Locations.First();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PL1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, null, "PL1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			Helper.Factory.Save();

			var runTimes = 0;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					if (++runTimes <= edgeCaseOccurrenceTimes)
					{
						if (hasErrorOnFirstAttempt)
						{
							notifications.AddError("ERROR");
						}

						if (hasWarningOnFirstAttempt)
						{
							notifications.AddWarning("WARNING");
						}
					}

					if (runTimes == 1)
					{
						Assert("should shortcircuit rebuilding the cache on the first run of Allocate when it is on RF device", !needRebuildLocationCache);
					}
					else
					{
						Assert(needRebuildLocationCache);
					}
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.Putaway_ReallocateInventory("PL1", "", null);
				mockedEngine.Verify(re => re.Putaway(
					It.IsAny<IEnumerable<WhsReceive>>(),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					It.IsAny<INotifications>(),
					null,
					It.IsAny<IEnumerable<ZGuid>>(),
					It.IsAny<bool>(),
					It.IsAny<bool>()), Times.Exactly(shouldRetry ? 2 : 1));
			}
		}

		#endregion

		#region TestPutaway_AutoAllocateInventory_LocationFormattedCheckDigit

		public void TestPutaway_AutoAllocateInventory_LocationFormattedCheckDigit()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.DefaultLocation;
			location.FormattedCheckDigit = "11";
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, null, "PLT1");
			Helper.Factory.Save();

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive.PK),
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) => receiveLines.ForEach(i => i.WE_WL = location.PK));

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.Putaway_AutoAllocateInventory("PLT1", "");
				AssertEquals("11", response.LocationFormattedCheckDigit);
			}
		}

		#endregion
	}
}
