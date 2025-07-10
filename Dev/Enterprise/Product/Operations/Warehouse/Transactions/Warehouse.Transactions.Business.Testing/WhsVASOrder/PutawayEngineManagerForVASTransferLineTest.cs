using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.ProductWarehousePutaway;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PutawayEngineManagerForVASTransferLineTest : PutawayEngineManagerTest<WhsVASOrder, VASReturnTransferLine>
	{
		#region TestIOCConfiguration

		public void TestIOCConfiguration()
		{
			var putawayEngineManager = ObjectFactory.Get<IPutawayEngineManagerForVASTransferLine>();
			AssertType<PutawayEngineManagerForVASTransferLine>(putawayEngineManager);
			AssertEquals("Should be a singleton.", true,
				ReferenceEquals(putawayEngineManager, ObjectFactory.Get<IPutawayEngineManagerForVASTransferLine>()));
		}

		#endregion

		#region TestConstructor

		public void TestConstructor_Engine_Null_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				new PutawayEngineManagerForVASTransferLine(null, Mock.Of<IPutawayLocationFactLoader>(),
					Mock.Of<IPutawayLocationCacheUpdater>()));
		}

		public void TestConstructor_PutawayLocationFactLoader_Null_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				new PutawayEngineManagerForVASTransferLine(Mock.Of<IUserHaltableProductionRulesEngineService>(), null,
					Mock.Of<IPutawayLocationCacheUpdater>()));
		}

		public void TestConstructor_PutawayLocationCacheUpdater_Null_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				new PutawayEngineManagerForVASTransferLine(Mock.Of<IUserHaltableProductionRulesEngineService>(),
					Mock.Of<IPutawayLocationFactLoader>(), null));
		}

		#endregion

		#region TestPutaway_GetFacts_VASLineData

		public void TestPutaway_GetFacts_VASLineData()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, data.Part1.OP_StockKeepingUnit)
				.F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			data.Part1.OP_Weight = 2m;
			data.Part1.OP_WeightUQ = Core.Constants.Weight.Pounds;
			data.Part1.OP_Cubic = 4m;
			data.Part1.OP_CubicUQ = Core.Constants.Volume.CubicDecimetres;

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation,
				new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation,
				new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			vasOrder.WVO_CustomerReferenceNo = "V1";

			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m, new ZDate(year, 1, 2),
				new ZDate(year, 1, 1));
			vasOrderLine.WVL_PartAttrib1 = "PA1";
			vasOrderLine.WVL_PartAttrib2 = "PA2";
			vasOrderLine.WVL_PartAttrib3 = "PA3";
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Successfully created Initial Transfer.", initialTransfer);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is Complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			Factory.Save();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			}

			AssertNotNull("Should create return transfer.", returnTransfer);
			AssertEquals("Transfer should have no problem being created.", "", Notify.AsString);
			AssertEquals("Transfer should have one Line.", 1, returnTransfer.Lines.Count);

			var vasReturnTransferLines = VASReturnTransferLine.GetLinesToPutawayFromTransfer(returnTransfer);
			((ILineToPutaway)vasReturnTransferLines.First()).LocationPK = ZGuid.Empty;

			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				locationFactLoader,
				vasOrder,
				vasReturnTransferLines);

			var resultingFacts = getFacts();

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.Quantity), 10m, inventoryFact.Quantity);
			AssertEquals(nameof(IInventoryFact.PackUnits), 10m, inventoryFact.PackUnits);
			AssertEquals(nameof(IInventoryFact.PackUQ), data.Part1.OP_StockKeepingUnit, inventoryFact.PackUQ);
			AssertEquals(nameof(IInventoryFact.UOMType), UOMPackTypesList.Codes.SplitCase, inventoryFact.UOMType);
			AssertEquals(nameof(IInventoryFact.ServiceLevel), "", inventoryFact.ServiceLevel);
			AssertEquals(nameof(IInventoryFact.ReceiveReference), "V1", inventoryFact.ReceiveReference);
			AssertEquals(nameof(IInventoryFact.CustomerReference), "V1", inventoryFact.CustomerReference);
			AssertEquals(nameof(IInventoryFact.HoldCode), "", inventoryFact.HoldCode);
			AssertEquals(nameof(IInventoryFact.ArrivalDate), null, inventoryFact.ArrivalDate);
			AssertEquals(nameof(IInventoryFact.RequiredDate), null, inventoryFact.RequiredDate);
			AssertEquals(nameof(IInventoryFact.PartAttribute1), "PA1", inventoryFact.PartAttribute1);
			AssertEquals(nameof(IInventoryFact.PartAttribute2), "PA2", inventoryFact.PartAttribute2);
			AssertEquals(nameof(IInventoryFact.PartAttribute3), "PA3", inventoryFact.PartAttribute3);
			AssertEquals(nameof(IInventoryFact.ExpiryDate), new ZDate(year, 1, 1), inventoryFact.ExpiryDate);
			AssertEquals(nameof(IInventoryFact.PackingDate), new ZDate(year, 1, 2), inventoryFact.PackingDate);
			AssertEquals(nameof(IInventoryFact.ProductWeight), 2m, inventoryFact.ProductWeight);
			AssertEquals(nameof(IInventoryFact.ProductWeightUQ), Core.Constants.Weight.Pounds,
				inventoryFact.ProductWeightUQ);
			AssertEquals(nameof(IInventoryFact.ProductVolume), 4m, inventoryFact.ProductVolume);
			AssertEquals(nameof(IInventoryFact.ProductVolumeUQ), Core.Constants.Volume.CubicDecimetres,
				inventoryFact.ProductVolumeUQ);
			AssertNull(nameof(IInventoryFact.Equipment), inventoryFact.Equipment.Fact);
		}

		#endregion

		#region TestPutaway_EndToEnd

		[GuiTest]
		public void TestPutaway_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = "Rules";
			ruleSet.PRS_Description = "Rules";
			ruleSet.PRS_Context = "PWP";
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;

			var owner1 =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			var owner2 =
				data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			var rule1 = GetRule(ruleSet, 1, owner1.PK, column: 1); // Set location to A-1
			var rule2 = GetRule(ruleSet, 2, owner2.PK, column: 3); // Set location to A-3

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			new WhsPutawayLocationCacheManager().CreateCache(Factory,
				new[] { location1.PK, location2.PK, location3.PK });
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, location2, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 2m, location2, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 6m, location2, "PLT-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 6m, location2, "PLT-2");

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 2,
				initialTransfer.Lines.Count);
			Factory.Save();

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is marked as completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			var returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			AssertNotNull("Should be able to create return transfer.", returnTransfer);
			AssertNull("No Error Message should have been made.", Notify.LastEvent);

			foreach (var line in returnTransfer.Lines)
			{
				line.WE_WL = ZGuid.Empty;
			}

			Factory.Save();
			AssertEquals("Precondition.", true, returnTransfer.Lines.All(l => l.WE_WL.IsEmpty));

			var linesToPutaway = GetLinesOnJob(vasOrder);
			var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForVASTransferLine>();
			putawayManager.Putaway(new[] { vasOrder }, linesToPutaway, Notify);
			AssertEquals("Should have set WE_WL.", true,
				returnTransfer.Lines.Where(l => l.WE_OP == data.Part1.PK).All(l => l.WE_WL == location1.PK));
			AssertEquals("Should have set WE_WL.", true,
				returnTransfer.Lines.Where(l => l.WE_OP == data.Part2.PK).All(l => l.WE_WL == location3.PK));
			AssertNull("No Error Message should have been made.", Notify.LastEvent);
		}

		[GuiTest]
		public void TestPutaway_EndToEnd_WithDangerousGoods()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.CreateUNDGDataItem(data.Part1, "0073a", "1.1D");

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = "Rules";
			ruleSet.PRS_Description = "Rules";
			ruleSet.PRS_Context = "PWP";
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;

			var rule1 = GetRule(ruleSet, 1,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			rule1.PRL_RuleDefinition = $@"{{
    ""conditions"": [
        {{
            ""fieldPath"": ""Product.FirstDG.ClassCode"",
            ""operation"": ""equals"",
            ""value"": ""1.1D""
        }}
    ],
    ""action"": {{
        ""$type"": ""PutawayActionState"",
        ""conditions"": [
            {{
                ""fieldPath"": ""Column"",
                ""operation"": ""equals"",
                ""value"": 1
            }}
        ],
        ""sortByCriteria"": [
        ]
    }}
}}";

			var rule2 = GetRule(ruleSet, 2,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			rule2.PRL_RuleDefinition = $@"{{
			""conditions"":[],
			""action"": {{
        ""$type"": ""PutawayActionState"",
        ""conditions"": [
            {{
                ""fieldPath"": ""Column"",
                ""operation"": ""equals"",
                ""value"": 2
            }}
        ],
        ""sortByCriteria"": [
        ]
    }}
}}";

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			new WhsPutawayLocationCacheManager().CreateCache(Factory,
				new[] { location1.PK, location2.PK, location3.PK });
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, location2, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 2m, location2, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 6m, location2, "PLT-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 6m, location2, "PLT-2");

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 2,
				initialTransfer.Lines.Count);
			Factory.Save();

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is marked as completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			var returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			AssertNotNull("Should be able to create return transfer.", returnTransfer);
			AssertNull("No Error Message should have been made.", Notify.LastEvent);

			foreach (var line in returnTransfer.Lines)
			{
				line.WE_WL = ZGuid.Empty;
			}

			Factory.Save();
			AssertEquals("Precondition.", true, returnTransfer.Lines.All(l => l.WE_WL.IsEmpty));

			var linesToPutaway = GetLinesOnJob(vasOrder);
			var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForVASTransferLine>();
			putawayManager.Putaway(new[] { vasOrder }, linesToPutaway, Notify);
			AssertEquals("Should have set WE_WL.", true,
				returnTransfer.Lines.Where(l => l.WE_OP == data.Part1.PK).All(l => l.WE_WL == location1.PK));
			AssertEquals("Should have set WE_WL.", true,
				returnTransfer.Lines.Where(l => l.WE_OP == data.Part2.PK).All(l => l.WE_WL == location2.PK));
			AssertNull("No Error Message should have been made.", Notify.LastEvent);
		}

		[GuiTest]
		public void TestPutaway_EndToEnd_WithMultipleDangerousGoods()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.CreateUNDGDataItem(data.Part1, "0073a", "1.1D");
			Helper.CreateUNDGDataItem(data.Part1, "0074a", "1.2D");
			Helper.CreateUNDGDataItem(data.Part2, "0073a", "1.1D");

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = "Rules";
			ruleSet.PRS_Description = "Rules";
			ruleSet.PRS_Context = "PWP";
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;

			var rule1 = GetRule(ruleSet, 1,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			rule1.PRL_RuleDefinition = $@"{{
    ""conditions"": [
        {{
            ""fieldPath"": ""Product.HasMultipleDangerousGoods"",
            ""operation"": ""equals"",
            ""value"": true
        }}
    ],
    ""action"": {{
        ""$type"": ""PutawayActionState"",
        ""conditions"": [
            {{
                ""fieldPath"": ""Column"",
                ""operation"": ""equals"",
                ""value"": 1
            }}
        ],
        ""sortByCriteria"": [
        ]
    }}
}}";

			var rule2 = GetRule(ruleSet, 2,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			rule2.PRL_RuleDefinition = $@"{{
			""conditions"":[],
			""action"": {{
        ""$type"": ""PutawayActionState"",
        ""conditions"": [
            {{
                ""fieldPath"": ""Column"",
                ""operation"": ""equals"",
                ""value"": 2
            }}
        ],
        ""sortByCriteria"": [
        ]
    }}
}}";

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			new WhsPutawayLocationCacheManager().CreateCache(Factory,
				new[] { location1.PK, location2.PK, location3.PK });
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, location2, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 2m, location2, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 6m, location2, "PLT-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 6m, location2, "PLT-2");

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 2,
				initialTransfer.Lines.Count);
			Factory.Save();

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is marked as completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			var returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			AssertNotNull("Should be able to create return transfer.", returnTransfer);
			AssertNull("No Error Message should have been made.", Notify.LastEvent);

			foreach (var line in returnTransfer.Lines)
			{
				line.WE_WL = ZGuid.Empty;
			}

			Factory.Save();
			AssertEquals("Precondition.", true, returnTransfer.Lines.All(l => l.WE_WL.IsEmpty));

			var linesToPutaway = GetLinesOnJob(vasOrder);
			var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForVASTransferLine>();
			putawayManager.Putaway(new[] { vasOrder }, linesToPutaway, Notify);
			AssertEquals("Should have set WE_WL.", true,
				returnTransfer.Lines.Where(l => l.WE_OP == data.Part1.PK).All(l => l.WE_WL == location1.PK));
			AssertEquals("Should have set WE_WL.", true,
				returnTransfer.Lines.Where(l => l.WE_OP == data.Part2.PK).All(l => l.WE_WL == location2.PK));
			AssertNull("No Error Message should have been made.", Notify.LastEvent);
		}

		#endregion

		#region TestPutaway_DbHits

		#region TestPutaway_DbHits

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits()
		{
			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 10, 10, 10);

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 10; i++)
			{
				var product = Helper.CreateProduct("P" + i, client);
				products.Add(product);

				if (i % 2 == 0)
				{
					var param = Helper.CreateProductParamsByWhsAndClient(product, client, whs);
					param.W3_WPG_PutawayGroup = Helper.CreatePutawayGroup("P" + i, "P" + i).PK;
				}
			}

			Factory.Save();

			for (var i = 0; i < 10; i++)
			{
				Helper.CreateWhsReceiveWithInventory(client, whs, $"R2{i}", products[i % products.Count], 100m);
			}

			Factory.Save();

			new WhsPutawayLocationCacheManager().CreateCache(Factory, row.Locations.Select(l => l.PK).ToArray());
			Factory.Save();

			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			for (var k = 0; k < 10; k++)
			{
				Helper.CreateWhsVASOrderLine(vasOrder, products[k % products.Count], 100m);
			}

			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 10,
				initialTransfer.Lines.Count);
			Factory.Save();

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is marked as completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			var returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			AssertNotNull("Should be able to create return transfer.", returnTransfer);
			AssertNull("No Error Message should have been made.", Notify.LastEvent);

			returnTransfer.Lines.ForEach(line => line.WE_WL = ZGuid.Empty);
			Factory.Save();
			AssertEquals("Precondition: Return transfer should have no locations", true,
				returnTransfer.Lines.All(l => l.WE_WL.IsEmpty));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ ProductionRuleSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 },
				{ WhsPutawayGroupSchema.Constants.TableName, 1 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsVASOrderSchema.Constants.TableName, 2 }
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var vasOrderInOtherFactory = otherFactory.Load<WhsVASOrder>(vasOrder.PK);
			var returnTransferInOtherFactory = otherFactory.Load<WhsTransfer>(returnTransfer.PK);
			var lineOnJob = GetLinesOnJob(vasOrderInOtherFactory);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (RowFactory.SetCachedTables())
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForVASTransferLine>();
				putawayManager.Putaway(new[] { vasOrderInOtherFactory }, lineOnJob, Notify);

				AssertNull("No Error Message should have been made.", Notify.LastEvent);
				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have putaway all inventory.", true,
					returnTransferInOtherFactory.Lines.All(i => i.WE_WL.IsValid));
			}
		}

		#endregion

		#region TestPutaway_DbHits_Split

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits_Split()
		{
			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 10, 10, 10);
			Factory.Save();

			var locations = row.Locations.Select(l => l.PK).ToArray();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, locations);

			var product = Helper.CreateProduct("P1", client);
			var receive = Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 1000m);
			Factory.Save();

			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			Helper.CreateWhsVASOrderLine(vasOrder, product, 1000m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 1,
				initialTransfer.Lines.Count);
			Factory.Save();

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is marked as completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			var returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			AssertNotNull("Should be able to create return transfer.", returnTransfer);
			AssertNull("No Error Message should have been made.", Notify.LastEvent);

			AssertEquals("Precondition: Number of returnTransfer correct", 1, returnTransfer.Lines.Count);
			var line = returnTransfer.Lines[0];
			line.WE_WL = ZGuid.Empty;
			Factory.Save();
			AssertEquals("Precondition: Return transfer should have no locations", true,
				returnTransfer.Lines.All(l => l.WE_WL.IsEmpty));

			var putawayInstructions = new List<PutawayResultFact>();
			for (var i = 0; i < 1000; i++)
			{
				putawayInstructions.Add(new PutawayResultFact(line.PK.ToGuid(), locations[i].ToGuid(), 1m, ""));
			}

			Factory.Save();

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 2 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 16 }, // max query size results in split query
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsVASOrderSchema.Constants.TableName, 2 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 }
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var returnTransferInOtherFactory = otherFactory.Load<WhsTransfer>(returnTransfer.PK);
			var vasOrderInOtherFactory = otherFactory.Load<WhsVASOrder>(vasOrder.PK);
			var linesToPutaway = GetLinesOnJob(vasOrderInOtherFactory);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (ObjectFactory.Substitute(engineMock.Object))
			using (RowFactory.SetCachedTables())
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForVASTransferLine>();
				putawayManager.Putaway(new[] { vasOrderInOtherFactory }, linesToPutaway, Notify);

				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertNull("No Error Message should have been made.", Notify.LastEvent);
				AssertEquals("Should have putaway all inventory.", true,
					returnTransferInOtherFactory.Lines.All(i => i.WE_WL.IsValid));
			}
		}

		#endregion

		#region TestPutaway_DbHits_Pallets

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits_Pallets()
		{
			// 1k inventory to putaway across 100 pallets, 25 on another receive, 25 already putaway on this receive, 50 to go to engine
			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 10, 10, 10);

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 50; i++)
			{
				products.Add(Helper.CreateProduct("P" + i, client));
			}

			Factory.Save();

			var locations = row.Locations.ToArray();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, locations.Select(l => l.PK));

			var receive = Helper.CreateWhsReceive(client, whs, "R2");
			for (var k = 0; k < 50; k++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, products[k], 20m, locations[0], $"PLT-{k}");
			}

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			for (var k = 0; k < 500; k++)
			{
				Helper.CreateWhsVASOrderLine(vasOrder, products[k % products.Count], 1m);
			}

			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 50,
				initialTransfer.Lines.Count);
			Factory.Save();

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is marked as completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			var returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			AssertNotNull("Should be able to create return transfer.", returnTransfer);
			AssertNull("No Error Message should have been made.", Notify.LastEvent);

			returnTransfer.Lines.ForEach(rtl => rtl.WE_WL = ZGuid.Empty);
			Factory.Save();
			AssertEquals("Precondition.", true, returnTransfer.Lines.All(l => l.WE_WL.IsEmpty));

			var putawayInstructions = new List<PutawayResultFact>();
			var lines = returnTransfer.Lines;
			var m = 0;
			foreach (var line in lines)
			{
				putawayInstructions.Add(new PutawayResultFact(line.PK.ToGuid(), locations[100 - m++].PK.ToGuid(), 10m,
					line.WE_PalletID));
			}

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 3 },
				{ WhsVASOrderSchema.Constants.TableName, 2 }
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var returnTransferInOtherFactory = otherFactory.Load<WhsTransfer>(returnTransfer.PK);
			var vasOrderInOtherFactory = otherFactory.Load<WhsVASOrder>(vasOrder.PK);
			var linesToPutaway = GetLinesOnJob(vasOrderInOtherFactory);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (ObjectFactory.Substitute(engineMock.Object))
			using (RowFactory.SetCachedTables())
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForVASTransferLine>();
				putawayManager.Putaway(new[] { vasOrderInOtherFactory }, linesToPutaway, Notify);

				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertNull("No Error Message should have been made.", Notify.LastEvent);
				AssertEquals("Should have putaway all inventory.", true,
					returnTransferInOtherFactory.Lines.All(i => i.WE_WL.IsValid));
			}
		}

		#endregion

		#region TestPutaway_DbHits_PartialPallets

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits_PartialPallets()
		{
			// 1k inventory to putaway across 100 pallets, 25 on another receive, 25 already putaway on this receive, 50 to go to engine
			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 10, 10, 10);

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 5; i++)
			{
				products.Add(Helper.CreateProduct("P" + i, client));
			}

			Factory.Save();

			var locations = row.Locations.ToArray();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, locations.Select(l => l.PK));

			var receive = Helper.CreateWhsReceive(client, whs, "R2");
			for (var i = 0; i < 200; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, products[0], 1m, locations[0], $"PLT-A");
				Helper.CreateWhsReceiveInventoryLine(receive, products[1], 1m, locations[0], $"PLT-B");
				Helper.CreateWhsReceiveInventoryLine(receive, products[2], 1m, locations[0], $"PLT-C");
				Helper.CreateWhsReceiveInventoryLine(receive, products[3], 1m, locations[0], $"PLT-D");
				Helper.CreateWhsReceiveInventoryLine(receive, products[4], 1m, locations[0], $"PLT-E");
			}

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			for (var k = 0; k < 500; k++)
			{
				Helper.CreateWhsVASOrderLine(vasOrder, products[k % products.Count], 1m);
			}

			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 5,
				initialTransfer.Lines.Count);
			Factory.Save();

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is marked as completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			var returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			AssertNotNull("Should be able to create return transfer.", returnTransfer);
			AssertNull("No Error Message should have been made.", Notify.LastEvent);

			returnTransfer.Lines.ForEach(rtl => rtl.WE_WL = ZGuid.Empty);
			Factory.Save();
			AssertEquals("Precondition.", true, returnTransfer.Lines.All(l => l.WE_WL.IsEmpty));

			var putawayInstructions = new List<PutawayResultFact>();
			var lines = returnTransfer.Lines;
			for (var m = 0; m < 50; m++)
			{
				putawayInstructions.Add(new PutawayResultFact(lines[m % lines.Count].PK.ToGuid(),
					locations[m].PK.ToGuid(), 10m, $"PLT-{m}"));
			}

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 16 }, // max query size results in split query
				{ WhsInventoryViewSchema.Constants.TableName, 8 }, // max query size results in split query
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 },
				{ WhsVASOrderSchema.Constants.TableName, 2 }
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var returnTransferInOtherFactory = otherFactory.Load<WhsTransfer>(returnTransfer.PK);
			var vasOrderInOtherFactory = otherFactory.Load<WhsVASOrder>(vasOrder.PK);
			var linesToPutaway = GetLinesOnJob(vasOrderInOtherFactory);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (ObjectFactory.Substitute(engineMock.Object))
			using (RowFactory.SetCachedTables())
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForVASTransferLine>();
				putawayManager.Putaway(new[] { vasOrderInOtherFactory }, linesToPutaway, Notify);

				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertNull("No Error Message should have been made.", Notify.LastEvent);
				AssertEquals("Should have putaway all inventory.", true,
					returnTransferInOtherFactory.Lines.All(i => i.WE_WL.IsValid));
			}
		}

		#endregion

		#region TestPutaway_DbHits_EndToEnd

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = "Rules";
			ruleSet.PRS_Description = "Rules";
			ruleSet.PRS_Context = "PWP";
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;

			var rule1 = GetRule(ruleSet, 1,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			var rule2 = GetRule(ruleSet, 2,
				data.Part2.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 2);

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var location4 = data.Whs1.FindLocation("A-4");
			new WhsPutawayLocationCacheManager().CreateCache(Factory,
				new[] { location1.PK, location2.PK, location3.PK, location4.PK });

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 1000; i++)
			{
				Helper.CreateWhsReceiveLine(receive, i % 2 != 0 ? data.Part1 : data.Part2, 5m, location1);
			}

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 500m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 500m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 2,
				initialTransfer.Lines.Count);
			Factory.Save();

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is marked as completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			var returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			AssertNotNull("Should be able to create return transfer.", returnTransfer);
			AssertNull("No Error Message should have been made.", Notify.LastEvent);

			returnTransfer.Lines.ForEach(line => line.WE_WL = ZGuid.Empty);
			Factory.Save();
			AssertEquals("Precondition: Return transfer should have no locations", true,
				returnTransfer.Lines.All(l => l.WE_WL.IsEmpty));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ ProductionRuleSchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 7 }, // max query size results in split query
				{ WhsInventoryViewSchema.Constants.TableName, 4 }, // max query size results in split query
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 },
				{ WhsVASOrderSchema.Constants.TableName, 2 }
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var vasOrderInOtherFactory = otherFactory.Load<WhsVASOrder>(vasOrder.PK);
			var returnTransferInOtherFactory = otherFactory.Load<WhsTransfer>(returnTransfer.PK);
			var linesOnJob = GetLinesOnJob(vasOrderInOtherFactory);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (RowFactory.SetCachedTables())
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForVASTransferLine>();
				putawayManager.Putaway(new[] { vasOrderInOtherFactory }, linesOnJob, Notify);

				AssertNull("No Error Message should have been made.", Notify.LastEvent);
				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have putaway all inventory.", true,
					returnTransferInOtherFactory.Lines.All(i => i.WE_WL.IsValid));
			}
		}

		#endregion

		#region TestPutaway_DbHits_ProductWithUNDG

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits_ProductWithUNDG()
		{
			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 10, 10, 10);

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 10; i++)
			{
				var product = Helper.CreateProduct("P" + i, client);
				products.Add(product);

				if (i % 2 == 0)
				{
					var param = Helper.CreateProductParamsByWhsAndClient(product, client, whs);
					param.W3_WPG_PutawayGroup = Helper.CreatePutawayGroup("P" + i, "P" + i).PK;
				}

				Helper.CreateUNDGDataItem(product, $"000{i}a", "1.1D");
			}

			Factory.Save();

			for (var i = 0; i < 10; i++)
			{
				Helper.CreateWhsReceiveWithInventory(client, whs, $"R2{i}", products[i % products.Count], 100m);
			}

			Factory.Save();

			new WhsPutawayLocationCacheManager().CreateCache(Factory, row.Locations.Select(l => l.PK).ToArray());
			Factory.Save();

			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			for (var k = 0; k < 10; k++)
			{
				Helper.CreateWhsVASOrderLine(vasOrder, products[k % products.Count], 100m);
			}

			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 10,
				initialTransfer.Lines.Count);
			Factory.Save();

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is marked as completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			var returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			AssertNotNull("Should be able to create return transfer.", returnTransfer);
			AssertNull("No Error Message should have been made.", Notify.LastEvent);

			returnTransfer.Lines.ForEach(line => line.WE_WL = ZGuid.Empty);
			Factory.Save();
			AssertEquals("Precondition: Return transfer should have no locations", true,
				returnTransfer.Lines.All(l => l.WE_WL.IsEmpty));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ ProductionRuleSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ UNDGSubstanceSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 },
				{ WhsPutawayGroupSchema.Constants.TableName, 1 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsVASOrderSchema.Constants.TableName, 2 }
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var vasOrderInOtherFactory = otherFactory.Load<WhsVASOrder>(vasOrder.PK);
			var returnTransferInOtherFactory = otherFactory.Load<WhsTransfer>(returnTransfer.PK);
			var lineOnJob = GetLinesOnJob(vasOrderInOtherFactory);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (RowFactory.SetCachedTables())
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForVASTransferLine>();
				putawayManager.Putaway(new[] { vasOrderInOtherFactory }, lineOnJob, Notify);

				AssertNull("No Error Message should have been made.", Notify.LastEvent);
				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have putaway all inventory.", true,
					returnTransferInOtherFactory.Lines.All(i => i.WE_WL.IsValid));
			}
		}

		#endregion

		#endregion

		#region TestPutaway_LocationCacheUpdateFailed

		public void TestPutaway_LocationCacheUpdateFailed()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, data.Part1.OP_StockKeepingUnit)
				.F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			data.Part1.OP_Weight = 2m;
			data.Part1.OP_WeightUQ = Core.Constants.Weight.Pounds;
			data.Part1.OP_Cubic = 4m;
			data.Part1.OP_CubicUQ = Core.Constants.Volume.CubicDecimetres;

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation,
				new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation,
				new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			vasOrder.WVO_CustomerReferenceNo = "V1";

			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m, new ZDate(year, 1, 2),
				new ZDate(year, 1, 1));
			vasOrderLine.WVL_PartAttrib1 = "PA1";
			vasOrderLine.WVL_PartAttrib2 = "PA2";
			vasOrderLine.WVL_PartAttrib3 = "PA3";
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Successfully created Initial Transfer.", initialTransfer);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is Complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			Factory.Save();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			}

			AssertNotNull("Should create return transfer.", returnTransfer);
			AssertEquals("Transfer should have no problem being created.", "", Notify.AsString);
			AssertEquals("Transfer should have one Line.", 1, returnTransfer.Lines.Count);

			var vasReturnTransferLines = VASReturnTransferLine.GetLinesToPutawayFromTransfer(returnTransfer);
			((ILineToPutaway)vasReturnTransferLines.First()).LocationPK = ZGuid.Empty;

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var mockedLocationCacheUpdater = new Mock<IPutawayLocationCacheUpdater>();
			mockedLocationCacheUpdater.Setup(mcu =>
					mcu.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, It.IsAny<ZGuid>(),
						It.IsAny<INotifications>(), 30))
				.Returns(false);

			var putawayManager = new PutawayEngineManagerForVASTransferLine(engineMock.Object,
				Mock.Of<IPutawayLocationFactLoader>(), mockedLocationCacheUpdater.Object);
			putawayManager.Putaway(new[] { vasOrder }, vasReturnTransferLines, new NotificationBuffer(), null);

			mockedLocationCacheUpdater.Verify(mcu =>
				mcu.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, data.Whs1.PK,
					It.IsAny<INotifications>(), 30));
			engineMock.VerifyNoOtherCalls();
			AssertEquals(ZGuid.Empty, ((ILineToPutaway)vasReturnTransferLines.First()).LocationPK);
		}

		#endregion

		#region Implementation

		protected override (Func<IEnumerable<IInputFact>> GetFacts, Func<ProductionRulesEngineResult, INotification>
			ProcessResults) CallRulesEngineMockAndReturnInputOutputFunctions(
				Mock<IUserHaltableProductionRulesEngineService> engineMock,
				IPutawayLocationFactLoader locationFactLoader, WhsVASOrder vasOrder,
				IEnumerable<VASReturnTransferLine> lines, RefEquipment equipment = null)
		{
			Func<IEnumerable<IInputFact>> getFacts = null;
			Func<ProductionRulesEngineResult, INotification> processResults = null;

			var notifications = new NotificationBuffer();

			engineMock.Setup(em => em.RunRulesEngine(Factory, notifications, RulesContextType.InventoryPutaway,
					It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == vasOrder.WarehousePK),
					It.IsAny<Func<IEnumerable<IInputFact>>>(),
					It.IsAny<Func<ProductionRulesEngineResult, INotification>>()))
				.Callback<BusinessObjectFactory, INotifications, RulesContextType, ProductionRuleSetFilter,
					Func<IEnumerable<IInputFact>>, Func<ProductionRulesEngineResult, INotification>>(
					(f, n, rct, filters, f1, f2) =>
					{
						getFacts = f1;
						processResults = f2;
					});

			var mockedLocationCacheUpdater = new Mock<IPutawayLocationCacheUpdater>();
			mockedLocationCacheUpdater.Setup(mcu =>
					mcu.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, It.IsAny<ZGuid>(),
						It.IsAny<INotifications>(), 30))
				.Returns(true);

			var putawayManager = new PutawayEngineManagerForVASTransferLine(engineMock.Object, locationFactLoader,
				mockedLocationCacheUpdater.Object);
			putawayManager.Putaway(new[] { vasOrder }, lines, notifications, equipment);

			AssertNotNull("Should have run RunRulesEngine.", getFacts);
			AssertNotNull("Should have run RunRulesEngine.", processResults);
			mockedLocationCacheUpdater.Verify(locationCacheUpdater =>
				locationCacheUpdater.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, vasOrder.WarehousePK,
					It.IsAny<INotifications>(), 30));

			return (getFacts, processResults);
		}

		protected override WhsVASOrder CreateJob(OrgHeader client, WhsWarehouse warehouse, string reference)
		{
			var serviceArea = Helper.CreateServiceAreaForVASOrder(warehouse);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			vasOrder.WVO_CustomerReferenceNo = reference;

			var returnTransfer = Helper.CreateWhsTransfer(client.PK, warehouse.PK);
			vasOrder.WVO_WD_TransferOutOfServiceArea = returnTransfer.PK;

			return vasOrder;
		}

		protected override VASReturnTransferLine CreateLine(WhsVASOrder job, OrgSupplierPart part, decimal units,
			string palletID = "")
		{
			var receive =
				Helper.CreateWhsReceiveWithInventory(job.Client, job.Warehouse, "R1", part, units, finalise: false);

			var vasOrderLine = Helper.CreateWhsVASOrderLine(job, part, units);
			var transfer = job.TransferOutOfServiceArea;

			var transferLine = Helper.CreateWhsTransferLine(transfer, part, units, "", "");
			transferLine.WE_TransferFromPalletId = palletID;
			transferLine.WE_PalletID = palletID;

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_TransactionLine = transferLine.PK;
			pickLine.WZ_WE_InventoryLine = receive.Lines[0].PK;
			pickLine.WZ_Units = units;

			return VASReturnTransferLine.GetPutawayLineFromTransferLine(transferLine);
		}

		protected override IEnumerable<VASReturnTransferLine> GetLinesOnJob(WhsVASOrder job)
		{
			return VASReturnTransferLine.GetLinesToPutawayFromTransfer(job.TransferOutOfServiceArea);
		}

		#endregion
	}
}
