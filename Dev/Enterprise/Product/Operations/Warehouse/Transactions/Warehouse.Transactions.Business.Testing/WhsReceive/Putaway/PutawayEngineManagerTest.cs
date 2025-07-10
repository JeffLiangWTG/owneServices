using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Moq;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehousePutaway;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class PutawayEngineManagerTest<TJob, TLine> : WhsTestCaseWithFactory
		where TJob : ILineToPutawayParent
		where TLine : ILineToPutaway
	{
		#region GetFacts

		public void TestPutaway_GetFacts_Client()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 10m);

			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				locationFactLoader,
				job,
				new[] { line });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var clientFact = nestedFacts.OfType<IOrganisationFact>().Single();
			AssertEquals(nameof(IOrganisationFact.PK), data.Org1.PK, clientFact.PK);
			AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, clientFact.Code);

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.Client), data.Org1.PK, inventoryFact.Client.Fact.PK);
		}

		public void TestPutaway_GetFacts_Client_IsOrgProxyOfCurrentCompany()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var company = GlbCompany.GetCurrentCompany(Factory);
			company.GC_OH_OrgProxy = data.Org1.PK;
			Factory.Save();

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 10m);

			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				locationFactLoader,
				job,
				new[] { line });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var clientFact = nestedFacts.OfType<IOrganisationFact>().Single();
			AssertEquals(nameof(IOrganisationFact.PK), data.Org1.PK, clientFact.PK);
			AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, clientFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, clientFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), true, clientFact.IsProxyOrgOfCurrentCompany);

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.Client), data.Org1.PK, inventoryFact.Client.Fact.PK);
		}

		public void TestPutaway_GetFacts_Client_IsOrgProxyOfCurrentCompany_BranchOrgProxy()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var company = GlbCompany.GetCurrentCompany(Factory);
			var branch = company.FirstActiveBranch;
			branch.GB_OH_OrgProxy = data.Org1.PK;
			Factory.Save();

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 10m);

			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				locationFactLoader,
				job,
				new[] { line });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var clientFact = nestedFacts.OfType<IOrganisationFact>().Single();
			AssertEquals(nameof(IOrganisationFact.PK), data.Org1.PK, clientFact.PK);
			AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, clientFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, clientFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), true, clientFact.IsProxyOrgOfCurrentCompany);

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.Client), data.Org1.PK, inventoryFact.Client.Fact.PK);
		}

		public void TestPutaway_GetFacts_Client_IsOrgProxyOfAnyCompany()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var company = Factory.New<GlbCompany>();
			company.GC_OH_OrgProxy = data.Org1.PK;
			Factory.Save();

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 10m);

			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				locationFactLoader,
				job,
				new[] { line });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var clientFact = nestedFacts.OfType<IOrganisationFact>().Single();
			AssertEquals(nameof(IOrganisationFact.PK), data.Org1.PK, clientFact.PK);
			AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, clientFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, clientFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, clientFact.IsProxyOrgOfCurrentCompany);

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.Client), data.Org1.PK, inventoryFact.Client.Fact.PK);
		}

		public void TestPutaway_GetFacts_Client_IsOrgProxyOfAnyCompany_BranchOrgProxy()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = data.Org1.PK;
			Factory.Save();

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 10m);

			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				locationFactLoader,
				job,
				new[] { line });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var clientFact = nestedFacts.OfType<IOrganisationFact>().Single();
			AssertEquals(nameof(IOrganisationFact.PK), data.Org1.PK, clientFact.PK);
			AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, clientFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, clientFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, clientFact.IsProxyOrgOfCurrentCompany);

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.Client), data.Org1.PK, inventoryFact.Client.Fact.PK);
		}

		public void TestPutaway_GetFacts_Locations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 10m);

			var locationFact = Mock.Of<IPutawayLocationFact>();
			var locationFactLoader = new Mock<IPutawayLocationFactLoader>(MockBehavior.Strict);
			locationFactLoader
				.Setup(lfl => lfl.GetPutawayLocationFacts(Factory, data.Whs1.PK, new[] { data.Org1.PK },
					It.Is<IEnumerable<ZGuid>>(invs => invs.Single() == data.Part1.PK), It.IsAny<IEnumerable<ZGuid>>()))
				.Returns(new[] { locationFact });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				locationFactLoader.Object,
				job,
				new[] { line });

			var resultingFacts = getFacts();
			AssertEquals("Should have only returned one location.", locationFact,
				resultingFacts.OfType<IPutawayLocationFact>().Single());
			AssertEquals("Should have returned one inventory for running putaway rules on.", line.PK,
				resultingFacts.OfType<IInventoryFact>().Single().PK);
		}

		public void TestPutaway_GetFacts_Locations_ShortCircuitedWhenNoInventoryToPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 10m);
			line.LocationPK = data.Whs1.DefaultLocation.PK;

			var locationFactLoader = new Mock<IPutawayLocationFactLoader>(MockBehavior.Strict);
			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				locationFactLoader.Object,
				job,
				new[] { line });

			var resultingFacts = getFacts();
			AssertEquals("Should have returned no facts, not even locations.", false, resultingFacts.Any());
		}

		public void TestPutaway_GetFacts_Locations_DistinctsProductPKs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line1 = CreateLine(job, data.Part1, 4m);
			var line2 = CreateLine(job, data.Part1, 1m);
			var line3 = CreateLine(job, data.Part2, 1m);
			var line4 = CreateLine(job, data.Part2, 1m);

			var locationFact = Mock.Of<IPutawayLocationFact>();
			var locationFactLoader = new Mock<IPutawayLocationFactLoader>(MockBehavior.Strict);
			locationFactLoader
				.Setup(lfl => lfl.GetPutawayLocationFacts(Factory, data.Whs1.PK, new[] { data.Org1.PK },
					It.Is<IEnumerable<ZGuid>>(prods =>
						prods.Count() == 2 && prods.Contains(data.Part1.PK) && prods.Contains(data.Part2.PK)), It.IsAny<IEnumerable<ZGuid>>()))
				.Returns(new[] { locationFact });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				locationFactLoader.Object,
				job,
				new[] { line1, line2, line3, line4 });

			var resultingFacts = getFacts();
			AssertEquals("Should have only returned one location.", locationFact,
				resultingFacts.OfType<IPutawayLocationFact>().Single());
		}

		public void TestPutaway_GetFacts_Equipment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 10m);

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "FOR";

			var equipment = Helper.CreateEquipment("FRK", 1m, Core.Constants.Weight.Kilograms, 1m,
				Core.Constants.Volume.CubicMetres);
			equipment.RQ_EquipmentType = "FRL";
			equipment.RQ_EquipmentGroup = "GRP";
			equipment.RQ_RC_RoadContainerType = containerType.PK;

			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				locationFactLoader,
				job,
				new[] { line },
				equipment: equipment);

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var equipmentFact = nestedFacts.OfType<IEquipmentFact>().Single();
			AssertEquals(nameof(IEquipmentFact.PK), equipment.PK, equipmentFact.PK);
			AssertEquals(nameof(IEquipmentFact.TypeCode), "FOR", equipmentFact.TypeCode);
			AssertEquals(nameof(IEquipmentFact.GroupCode), equipment.RQ_EquipmentGroup, equipmentFact.GroupCode);

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.Equipment), equipment.PK, inventoryFact.Equipment.Fact.PK);
		}

		public void TestPutaway_GetFacts_ProductStyle()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var style1 = Helper.CreateProductStyle("S1", "Style1", data.Org1.PK);
			var colour1 = style1.Colours.AddNew();
			var size1 = style1.Sizes.AddNew();

			var style2 = Helper.CreateProductStyle("S2", "Style2", data.Org1.PK);
			var colour2 = style2.Colours.AddNew();
			var classification2 = style2.Classifications.AddNew();
			var size2 = style2.Sizes.AddNew();

			// style without classification
			var part1 = Helper.CreateProduct(data.Org1, "P1");
			var product1 = WhsProduct.GetWhsProduct(part1);
			product1.ProductStylePK = style1.PK;
			product1.ProductStyleColourPK = colour1.PK;
			product1.ProductStyleSizePK = size1.PK;
			var part1Relation = part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);

			// style with classification
			var part2 = Helper.CreateProduct(data.Org1, "P2");
			var product2 = WhsProduct.GetWhsProduct(part2);
			product2.ProductStylePK = style2.PK;
			product2.ProductStyleClassificationPK = classification2.PK;
			product2.ProductStyleColourPK = colour2.PK;
			product2.ProductStyleSizePK = size2.PK;
			var part2Relation = part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);

			// no style
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var product3 = WhsProduct.GetWhsProduct(part3);
			product3.ProductStylePK = ZGuid.Empty;
			var part3Relation = part3.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line1 = CreateLine(job, part1, 10m);
			var line2 = CreateLine(job, part2, 20m);
			var line3 = CreateLine(job, part3, 30m);

			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				locationFactLoader,
				job,
				new[] { line1, line2, line3 });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var productFacts = nestedFacts.OfType<IPutawayProductFact>().ToArray();
			AssertEquals(3, productFacts.Length);
			var product1Fact = productFacts.Single(p => p.PK == part1Relation.PK);
			var product2Fact = productFacts.Single(p => p.PK == part2Relation.PK);
			var product3Fact = productFacts.Single(p => p.PK == part3Relation.PK);

			var inventoryFact1 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line1.PK);
			var inventoryFact2 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line2.PK);
			var inventoryFact3 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line3.PK);
			AssertEquals(nameof(IInventoryFact.Product), part1Relation.PK, inventoryFact1.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Product), part2Relation.PK, inventoryFact2.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Product), part3Relation.PK, inventoryFact3.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.PartPK), part1.PK, inventoryFact1.PartPK);
			AssertEquals(nameof(IInventoryFact.PartPK), part2.PK, inventoryFact2.PartPK);
			AssertEquals(nameof(IInventoryFact.PartPK), part3.PK, inventoryFact3.PartPK);

			AssertEquals(nameof(IPutawayProductFact.Code), part1.OP_PartNum, product1Fact.Code);
			AssertEquals(nameof(IPutawayProductFact.ProductStyleCode), "S1", product1Fact.ProductStyleCode);
			AssertEquals(nameof(IPutawayProductFact.ProductStyleClassificationCode), string.Empty, product1Fact.ProductStyleClassificationCode);
			AssertEquals(nameof(IPutawayProductFact.ProductStyleColorCode), colour1.WSC_Code, product1Fact.ProductStyleColorCode);
			AssertEquals(nameof(IPutawayProductFact.ProductStyleSizeCode), size1.WSZ_Size, product1Fact.ProductStyleSizeCode);

			AssertEquals(nameof(IPutawayProductFact.Code), part2.OP_PartNum, product2Fact.Code);
			AssertEquals(nameof(IPutawayProductFact.ProductStyleCode), "S2", product2Fact.ProductStyleCode);
			AssertEquals(nameof(IPutawayProductFact.ProductStyleClassificationCode), classification2.WSS_Code, product2Fact.ProductStyleClassificationCode);
			AssertEquals(nameof(IPutawayProductFact.ProductStyleColorCode), colour2.WSC_Code, product2Fact.ProductStyleColorCode);
			AssertEquals(nameof(IPutawayProductFact.ProductStyleSizeCode), size2.WSZ_Size, product2Fact.ProductStyleSizeCode);

			AssertEquals(nameof(IPutawayProductFact.Code), part3.OP_PartNum, product3Fact.Code);
			AssertEquals(nameof(IPutawayProductFact.ProductStyleCode), string.Empty, product3Fact.ProductStyleCode);
			AssertEquals(nameof(IPutawayProductFact.ProductStyleClassificationCode), string.Empty, product3Fact.ProductStyleClassificationCode);
			AssertEquals(nameof(IPutawayProductFact.ProductStyleColorCode), string.Empty, product3Fact.ProductStyleColorCode);
			AssertEquals(nameof(IPutawayProductFact.ProductStyleSizeCode), string.Empty, product3Fact.ProductStyleSizeCode);
		}

		public void TestPutaway_GetFacts_Products()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var category1 = Factory.New<OrgPartCategory>();
			category1.OPC_CategoryCode = "CAT";

			var putawayGroup = Helper.CreatePutawayGroup("ABC", "ABC");

			var part1Relation =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			part1Relation.OU_OPC_Category = category1.PK;
			part1Relation.OU_UnitPrice = 154.1548m;
			part1Relation.OU_RX_NKUnitPriceCurrency = "AUD";

			var part2Relation =
				data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);

			var paramsByWhsAndClient = Factory.New<WhsProductParamsByWhsAndClient>();
			paramsByWhsAndClient.W3_WW = data.Whs1.PK;
			paramsByWhsAndClient.W3_OH = data.Org1.PK;
			paramsByWhsAndClient.W3_OP = data.Part1.PK;
			paramsByWhsAndClient.W3_WPG_PutawayGroup = putawayGroup.PK;

			var dateTimeOffset = ZDateTimeOffset.Today.AddDays(-10);
			var abcCategory = Helper.CreateABCCategory(data.Part2, data.Org1, data.Whs1, "ABC",
				dateTimeOffset, dateTimeOffset);

			data.Part1.OP_StockKeepingUnit = Core.Constants.PkgUnit.Carton;
			data.Part2.OP_StockKeepingUnit = Core.Constants.PkgUnit.Pallet;

			data.Part1.OP_RH_NKCommodityCode = "HAZ";

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line1 = CreateLine(job, data.Part1, 4m);
			var line2 = CreateLine(job, data.Part1, 1m);
			var line3 = CreateLine(job, data.Part2, 1m);
			var line4 = CreateLine(job, data.Part2, 1m);

			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				locationFactLoader,
				job,
				new[] { line1, line2, line3, line4 });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var productFacts = nestedFacts.OfType<IPutawayProductFact>().ToArray();
			AssertEquals(2, productFacts.Length);
			var product1Fact = productFacts.Single(p => p.PK == part1Relation.PK);
			var product2Fact = productFacts.Single(p => p.PK == part2Relation.PK);

			var inventoryFact1 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line1.PK);
			var inventoryFact2 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line2.PK);
			AssertEquals(nameof(IInventoryFact.Product), part1Relation.PK, inventoryFact1.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Product), part1Relation.PK, inventoryFact2.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.PartPK), data.Part1.PK, inventoryFact1.PartPK);
			AssertEquals(nameof(IInventoryFact.PartPK), data.Part1.PK, inventoryFact2.PartPK);

			AssertEquals(nameof(IPutawayProductFact.Code), data.Part1.OP_PartNum, product1Fact.Code);
			AssertEquals(nameof(IPutawayProductFact.StockUQ), Core.Constants.PkgUnit.Carton, product1Fact.StockUQ);
			AssertEquals(nameof(IPutawayProductFact.CommodityCode), "HAZ", product1Fact.CommodityCode);
			AssertEquals(nameof(IPutawayProductFact.CategoryCode), "CAT", product1Fact.CategoryCode);
			AssertEquals(nameof(IPutawayProductFact.ABCCategoryCode), string.Empty, product1Fact.ABCCategoryCode);
			AssertEquals(nameof(IPutawayProductFact.PutawayGroupCode), "ABC", product1Fact.PutawayGroupCode);
			AssertEquals(nameof(IPutawayProductFact.UnitPrice) + "Value", 154.1548m, product1Fact.UnitPrice.Value);
			AssertEquals(nameof(IPutawayProductFact.UnitPrice) + "Unit", "AUD", product1Fact.UnitPrice.Unit);
			AssertEquals(nameof(IPutawayProductFact.FirstDG), null, product1Fact.FirstDG.Fact);
			AssertEquals(nameof(IPutawayProductFact.HasMultipleDangerousGoods), false, product1Fact.HasMultipleDangerousGoods);

			var inventoryFact3 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line3.PK);
			var inventoryFact4 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line4.PK);
			AssertEquals(nameof(IInventoryFact.PartPK), data.Part2.PK, inventoryFact3.PartPK);
			AssertEquals(nameof(IInventoryFact.PartPK), data.Part2.PK, inventoryFact4.PartPK);
			AssertEquals(nameof(IInventoryFact.Product), part2Relation.PK, inventoryFact3.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Product), part2Relation.PK, inventoryFact4.Product.Fact.PK);

			AssertEquals(nameof(IPutawayProductFact.Code), data.Part2.OP_PartNum, product2Fact.Code);
			AssertEquals(nameof(IPutawayProductFact.StockUQ), Core.Constants.PkgUnit.Pallet, product2Fact.StockUQ);
			AssertEquals(nameof(IPutawayProductFact.CommodityCode), string.Empty, product2Fact.CommodityCode);
			AssertEquals(nameof(IPutawayProductFact.CategoryCode), string.Empty, product2Fact.CategoryCode);
			AssertEquals(nameof(IPutawayProductFact.ABCCategoryCode), "ABC", product2Fact.ABCCategoryCode);
			AssertEquals(nameof(IPutawayProductFact.PutawayGroupCode), "", product2Fact.PutawayGroupCode);
			AssertEquals(nameof(IPutawayProductFact.FirstDG), null, product2Fact.FirstDG.Fact);
			AssertEquals(nameof(IPutawayProductFact.HasMultipleDangerousGoods), false, product2Fact.HasMultipleDangerousGoods);
		}

		public void TestPutaway_GetFacts_Product_WithDangerousGoods()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var category1 = Factory.New<OrgPartCategory>();
			category1.OPC_CategoryCode = "CAT";

			var part1Relation = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			part1Relation.OU_OPC_Category = category1.PK;
			part1Relation.OU_UnitPrice = 154.1548m;
			part1Relation.OU_RX_NKUnitPriceCurrency = "AUD";

			var part2Relation = data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var paramsByWhsAndClient = Factory.New<WhsProductParamsByWhsAndClient>();
			paramsByWhsAndClient.W3_WW = data.Whs1.PK;
			paramsByWhsAndClient.W3_OH = data.Org1.PK;
			paramsByWhsAndClient.W3_OP = data.Part1.PK;

			data.Part1.OP_StockKeepingUnit = Core.Constants.PkgUnit.Carton;
			var dgPart1 = Helper.CreateUNDGDataItem(data.Part1, "0073a", "1.1D");
			data.Part2.OP_StockKeepingUnit = Core.Constants.PkgUnit.Pallet;
			var dgPart2 = Helper.CreateUNDGDataItem(data.Part2, "0084b", "2.2D");

			data.Part1.OP_RH_NKCommodityCode = "HAZ";

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line1 = CreateLine(job, data.Part1, 4m);
			var line2 = CreateLine(job, data.Part1, 1m);
			var line3 = CreateLine(job, data.Part2, 1m);
			var line4 = CreateLine(job, data.Part2, 1m);

			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				locationFactLoader,
				job,
				new[] { line1, line2, line3, line4 });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var productFacts = nestedFacts.OfType<IPutawayProductFact>().ToArray();
			AssertEquals(2, productFacts.Length);
			var product1Fact = productFacts.Single(p => p.PK == part1Relation.PK);
			var product2Fact = productFacts.Single(p => p.PK == part2Relation.PK);

			var inventoryFact1 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line1.PK);
			var inventoryFact2 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line2.PK);
			AssertEquals(nameof(IInventoryFact.Product), part1Relation.PK, inventoryFact1.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Product), part1Relation.PK, inventoryFact2.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.PartPK), data.Part1.PK, inventoryFact1.PartPK);
			AssertEquals(nameof(IInventoryFact.PartPK), data.Part1.PK, inventoryFact2.PartPK);

			AssertEquals(nameof(IPutawayProductFact.Code), data.Part1.OP_PartNum, product1Fact.Code);
			AssertEquals(nameof(IPutawayProductFact.StockUQ), Core.Constants.PkgUnit.Carton, product1Fact.StockUQ);
			AssertEquals(nameof(IPutawayProductFact.CommodityCode), "HAZ", product1Fact.CommodityCode);
			AssertEquals(nameof(IPutawayProductFact.CategoryCode), "CAT", product1Fact.CategoryCode);
			AssertEquals(nameof(IPutawayProductFact.ABCCategoryCode), string.Empty, product1Fact.ABCCategoryCode);
			AssertEquals(nameof(IPutawayProductFact.PutawayGroupCode), string.Empty, product1Fact.PutawayGroupCode);
			AssertEquals(nameof(IPutawayProductFact.UnitPrice) + "Value", 154.1548m, product1Fact.UnitPrice.Value);
			AssertEquals(nameof(IPutawayProductFact.UnitPrice) + "Unit", "AUD", product1Fact.UnitPrice.Unit);
			AssertEquals(nameof(IPutawayProductFact.FirstDG), dgPart1.PK, product1Fact.FirstDG.Fact.PK);
			AssertEquals(nameof(IPutawayProductFact.FirstDG), data.Part1.PK, product1Fact.FirstDG.Fact.ParentPK);
			AssertEquals(nameof(IPutawayProductFact.FirstDG), "0073a-IMO", product1Fact.FirstDG.Fact.SubstanceCode);
			AssertEquals(nameof(IPutawayProductFact.FirstDG), "1.1D", product1Fact.FirstDG.Fact.ClassCode);
			AssertEquals(nameof(IPutawayProductFact.HasMultipleDangerousGoods), false, product1Fact.HasMultipleDangerousGoods);

			var inventoryFact3 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line3.PK);
			var inventoryFact4 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line4.PK);
			AssertEquals(nameof(IInventoryFact.PartPK), data.Part2.PK, inventoryFact3.PartPK);
			AssertEquals(nameof(IInventoryFact.PartPK), data.Part2.PK, inventoryFact4.PartPK);
			AssertEquals(nameof(IInventoryFact.Product), part2Relation.PK, inventoryFact3.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Product), part2Relation.PK, inventoryFact4.Product.Fact.PK);

			AssertEquals(nameof(IPutawayProductFact.Code), data.Part2.OP_PartNum, product2Fact.Code);
			AssertEquals(nameof(IPutawayProductFact.StockUQ), Core.Constants.PkgUnit.Pallet, product2Fact.StockUQ);
			AssertEquals(nameof(IPutawayProductFact.CommodityCode), string.Empty, product2Fact.CommodityCode);
			AssertEquals(nameof(IPutawayProductFact.CategoryCode), string.Empty, product2Fact.CategoryCode);
			AssertEquals(nameof(IPutawayProductFact.ABCCategoryCode), string.Empty, product2Fact.ABCCategoryCode);
			AssertEquals(nameof(IPutawayProductFact.PutawayGroupCode), string.Empty, product2Fact.PutawayGroupCode);
			AssertEquals(nameof(IPutawayProductFact.FirstDG), dgPart2.PK, product2Fact.FirstDG.Fact.PK);
			AssertEquals(nameof(IPutawayProductFact.FirstDG), data.Part2.PK, product2Fact.FirstDG.Fact.ParentPK);
			AssertEquals(nameof(IPutawayProductFact.FirstDG), "0084b-IMO", product2Fact.FirstDG.Fact.SubstanceCode);
			AssertEquals(nameof(IPutawayProductFact.FirstDG), "2.2D", product2Fact.FirstDG.Fact.ClassCode);
			AssertEquals(nameof(IPutawayProductFact.HasMultipleDangerousGoods), false, product1Fact.HasMultipleDangerousGoods);
		}

		public void TestPutaway_GetFacts_Product_WithMultipleDangerousGoods()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var category1 = Factory.New<OrgPartCategory>();
			category1.OPC_CategoryCode = "CAT";

			var part1Relation = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			part1Relation.OU_OPC_Category = category1.PK;
			part1Relation.OU_UnitPrice = 154.1548m;
			part1Relation.OU_RX_NKUnitPriceCurrency = "AUD";

			var part2Relation = data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var paramsByWhsAndClient = Factory.New<WhsProductParamsByWhsAndClient>();
			paramsByWhsAndClient.W3_WW = data.Whs1.PK;
			paramsByWhsAndClient.W3_OH = data.Org1.PK;
			paramsByWhsAndClient.W3_OP = data.Part1.PK;

			data.Part1.OP_StockKeepingUnit = Core.Constants.PkgUnit.Carton;
			var dgPart1_1 = Helper.CreateUNDGDataItem(data.Part1, "0073a", "2.2D");
			var dgPart1_2 = Helper.CreateUNDGDataItem(data.Part1, "0073a", "1.1D");
			var dgPart1_3 = Helper.CreateUNDGDataItem(data.Part1, "0073a", "3.3D");

			data.Part2.OP_StockKeepingUnit = Core.Constants.PkgUnit.Pallet;
			var dgPart2_1 = Helper.CreateUNDGDataItem(data.Part2, "0065a", "2.2D");
			var dgPart2_2 = Helper.CreateUNDGDataItem(data.Part2, "0100b", "2.2D");
			var dgPart2_3 = Helper.CreateUNDGDataItem(data.Part2, "0059c", "2.2D");

			data.Part1.OP_RH_NKCommodityCode = "HAZ";

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line1 = CreateLine(job, data.Part1, 4m);
			var line2 = CreateLine(job, data.Part1, 1m);
			var line3 = CreateLine(job, data.Part2, 1m);
			var line4 = CreateLine(job, data.Part2, 1m);

			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				locationFactLoader,
				job,
				new[] { line1, line2, line3, line4 });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var productFacts = nestedFacts.OfType<IPutawayProductFact>().ToArray();
			AssertEquals(2, productFacts.Length);
			var product1Fact = productFacts.Single(p => p.PK == part1Relation.PK);
			var product2Fact = productFacts.Single(p => p.PK == part2Relation.PK);

			var inventoryFact1 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line1.PK);
			var inventoryFact2 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line2.PK);
			AssertEquals(nameof(IInventoryFact.Product), part1Relation.PK, inventoryFact1.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Product), part1Relation.PK, inventoryFact2.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.PartPK), data.Part1.PK, inventoryFact1.PartPK);
			AssertEquals(nameof(IInventoryFact.PartPK), data.Part1.PK, inventoryFact2.PartPK);

			AssertEquals(nameof(IPutawayProductFact.Code), data.Part1.OP_PartNum, product1Fact.Code);
			AssertEquals(nameof(IPutawayProductFact.StockUQ), Core.Constants.PkgUnit.Carton, product1Fact.StockUQ);
			AssertEquals(nameof(IPutawayProductFact.CommodityCode), "HAZ", product1Fact.CommodityCode);
			AssertEquals(nameof(IPutawayProductFact.CategoryCode), "CAT", product1Fact.CategoryCode);
			AssertEquals(nameof(IPutawayProductFact.ABCCategoryCode), string.Empty, product1Fact.ABCCategoryCode);
			AssertEquals(nameof(IPutawayProductFact.PutawayGroupCode), string.Empty, product1Fact.PutawayGroupCode);
			AssertEquals(nameof(IPutawayProductFact.UnitPrice) + "Value", 154.1548m, product1Fact.UnitPrice.Value);
			AssertEquals(nameof(IPutawayProductFact.UnitPrice) + "Unit", "AUD", product1Fact.UnitPrice.Unit);
			AssertEquals(nameof(IPutawayProductFact.FirstDG), dgPart1_2.PK, product1Fact.FirstDG.Fact.PK);
			AssertEquals(nameof(IPutawayProductFact.HasMultipleDangerousGoods), true, product1Fact.HasMultipleDangerousGoods);

			var inventoryFact3 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line3.PK);
			var inventoryFact4 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line4.PK);
			AssertEquals(nameof(IInventoryFact.PartPK), data.Part2.PK, inventoryFact3.PartPK);
			AssertEquals(nameof(IInventoryFact.PartPK), data.Part2.PK, inventoryFact4.PartPK);
			AssertEquals(nameof(IInventoryFact.Product), part2Relation.PK, inventoryFact3.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Product), part2Relation.PK, inventoryFact4.Product.Fact.PK);

			AssertEquals(nameof(IPutawayProductFact.Code), data.Part2.OP_PartNum, product2Fact.Code);
			AssertEquals(nameof(IPutawayProductFact.StockUQ), Core.Constants.PkgUnit.Pallet, product2Fact.StockUQ);
			AssertEquals(nameof(IPutawayProductFact.CommodityCode), string.Empty, product2Fact.CommodityCode);
			AssertEquals(nameof(IPutawayProductFact.CategoryCode), string.Empty, product2Fact.CategoryCode);
			AssertEquals(nameof(IPutawayProductFact.ABCCategoryCode), string.Empty, product2Fact.ABCCategoryCode);
			AssertEquals(nameof(IPutawayProductFact.PutawayGroupCode), string.Empty, product2Fact.PutawayGroupCode);
			AssertEquals(nameof(IPutawayProductFact.FirstDG), dgPart2_3.PK, product2Fact.FirstDG.Fact.PK);
			AssertEquals(nameof(IPutawayProductFact.HasMultipleDangerousGoods), true, product2Fact.HasMultipleDangerousGoods);
		}

		#endregion

		#region ProcessResults

		public void TestPutaway_ProcessResults()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-2");

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 5m);

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<IPutawayLocationFactLoader>(),
				job,
				new[] { line });

			var putawayResult = new ProductionRulesEngineResult(new[]
			{
				new PutawayResultFact(line.PK.ToGuid(), location.PK.ToGuid(), 5m, "")
			});

			AssertNull(processResults(putawayResult));
			AssertEquals("Should *not* have split lines.", 1, GetLinesOnJob(job).Count());
			AssertEquals("Should *not* have split lines.", 5m, line.QuantityToPutaway);
			AssertEquals("Should have putaway inventory.", location.PK, line.LocationPK);
			AssertEquals("Should *not* have set pallet.", string.Empty, line.PalletID);
		}

		public void TestPutaway_ProcessResults_Warning()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-2");

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 10m);

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<IPutawayLocationFactLoader>(),
				job,
				new[] { line });

			var putawayResult = new ProductionRulesEngineResult(Enumerable.Empty<PutawayResultFact>());

			var warning = processResults(putawayResult);
			AssertEquals("Should *not* have putaway inventory.", ZGuid.Empty, line.LocationPK);
			AssertEquals("Should have returned warning.", false, warning.Type.IsFatal);
			AssertEquals("Should have returned warning.",
				"Warning: Some inventory did not have locations allocated, this might be due to full locations or an incomplete rule setup.",
				warning.Message);
		}

		public void TestPutaway_ProcessResults_NoQuantity_NoWarning()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-2");

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 0m);

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<IPutawayLocationFactLoader>(),
				job,
				new[] { line });

			var putawayResult = new ProductionRulesEngineResult(Enumerable.Empty<PutawayResultFact>());

			var result = processResults(putawayResult);
			AssertEquals("Should *not* have putaway inventory.", ZGuid.Empty, line.LocationPK);
			AssertNull("Should *not* have returned warning.", result);
		}

		public void TestPutaway_InvalidLinePK()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-2");

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 5m);

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<IPutawayLocationFactLoader>(),
				job,
				new[] { line });

			var invalidResult = new PutawayResultFact(Guid.NewGuid(), location.PK.ToGuid(), 5m, "");
			var validResult = new PutawayResultFact(line.PK.ToGuid(), location.PK.ToGuid(), 5m, "");
			var putawayResult = new ProductionRulesEngineResult(new[] { invalidResult, validResult });

			AssertNull(processResults(putawayResult));
			AssertEquals("Should *not* have split lines.", 1, GetLinesOnJob(job).Count());
			AssertEquals("Should *not* have split lines.", 5m, line.QuantityToPutaway);
			AssertEquals("Should have putaway inventory.", location.PK, line.LocationPK);
			AssertEquals("Should *not* have set pallet.", string.Empty, line.PalletID);
		}

		public void TestPutaway_ProcessResults_MultipleLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line1 = CreateLine(job, data.Part1, 5m);
			var line2 = CreateLine(job, data.Part1, 5m);

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<IPutawayLocationFactLoader>(),
				job,
				new[] { line1, line2 });

			var result1 = new PutawayResultFact(line1.PK.ToGuid(), location1.PK.ToGuid(), 5m, "");
			var result2 = new PutawayResultFact(line2.PK.ToGuid(), location2.PK.ToGuid(), 5m, "");

			var putawayResult = new ProductionRulesEngineResult(new[] { result1, result2 });
			AssertNull(processResults(putawayResult));
			AssertEquals("Should *not* have split line.", 2, GetLinesOnJob(job).Count());
			AssertEquals("Should *not* have split line.", 5m, line1.QuantityToPutaway);
			AssertEquals("Should have putaway line.", location1.PK, line1.LocationPK);
			AssertEquals("Should *not* have set pallet.", string.Empty, line1.PalletID);

			AssertEquals("Should *not* have split line.", 5m, line2.QuantityToPutaway);
			AssertEquals("Should have putaway line.", location2.PK, line2.LocationPK);
			AssertEquals("Should *not* have set pallet.", string.Empty, line2.PalletID);
		}

		public void TestPutaway_ProcessResults_PartiallyPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-2");

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 5m);

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<IPutawayLocationFactLoader>(),
				job,
				new[] { line });

			var putawayResult = new ProductionRulesEngineResult(new[]
			{
				new PutawayResultFact(line.PK.ToGuid(), location.PK.ToGuid(), 3m, "")
			});
			var warning = processResults(putawayResult);

			AssertEquals("Should have returned warning.", false, warning.Type.IsFatal);
			AssertEquals("Should have returned warning.",
				"Warning: Some inventory did not have locations allocated, this might be due to full locations or an incomplete rule setup.",
				warning.Message);

			var lines = GetLinesOnJob(job);
			AssertEquals("Should have split inventory.", 2, lines.Count());

			var inv1 = lines.Single(i => i.LocationPK == location.PK);
			AssertEquals("Should have split inventory.", 3m, inv1.QuantityToPutaway);
			AssertEquals("Should *not* have set pallet.", string.Empty, inv1.PalletID);

			var inv2 = lines.Single(i => i.LocationPK.IsEmpty);
			AssertEquals("Should *not* have split inventory.", 2m, inv2.QuantityToPutaway);
			AssertEquals("Should *not* have set pallet.", string.Empty, inv2.PalletID);
		}

		public void TestPutaway_ProcessResults_Pallet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-2");

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 5m, "PLT-123");

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<IPutawayLocationFactLoader>(),
				job,
				new[] { line });

			var putawayResult = new ProductionRulesEngineResult(new[]
			{
				new PutawayResultFact(line.PK.ToGuid(), location.PK.ToGuid(), 5m, "")
			});
			AssertNull(processResults(putawayResult));
			AssertEquals("Should *not* have split line.", 1, GetLinesOnJob(job).Count());
			AssertEquals("Should *not* have split line.", 5m, line.QuantityToPutaway);
			AssertEquals("Should have putaway line.", location.PK, line.LocationPK);
			AssertEquals("Should *not* have changed pallet.", "PLT-123", line.PalletID);
		}

		public void TestPutaway_ProcessResults_SplitsInventory_Location()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 5m);

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<IPutawayLocationFactLoader>(),
				job,
				new[] { line });

			var putawayResult = new ProductionRulesEngineResult(
				new[]
				{
					new PutawayResultFact(line.PK.ToGuid(), location1.PK.ToGuid(), 3m, ""),
					new PutawayResultFact(line.PK.ToGuid(), location2.PK.ToGuid(), 2m, "")
				});
			AssertNull(processResults(putawayResult));

			var lines = GetLinesOnJob(job);
			AssertEquals("Should have split inventory.", 2, lines.Count());

			var inv1 = lines.Single(i => i.LocationPK == location1.PK);
			AssertEquals("Should have split inventory.", 3m, inv1.QuantityToPutaway);
			AssertEquals("Should *not* have set pallet.", string.Empty, inv1.PalletID);

			var inv2 = lines.Single(i => i.LocationPK == location2.PK);
			AssertEquals("Should have split inventory.", 2m, inv2.QuantityToPutaway);
			AssertEquals("Should *not* have set pallet.", string.Empty, inv2.PalletID);
		}

		public void TestPutaway_ProcessResults_SplitsInventory_Pallet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-2");

			var job = CreateJob(data.Org1, data.Whs1, "1");
			var line = CreateLine(job, data.Part1, 5m);

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<IPutawayLocationFactLoader>(),
				job,
				new[] { line });

			var putawayResult = new ProductionRulesEngineResult(
				new[]
				{
					new PutawayResultFact(line.PK.ToGuid(), location.PK.ToGuid(), 3m, "PLT-123"),
					new PutawayResultFact(line.PK.ToGuid(), location.PK.ToGuid(), 2m, "")
				});
			AssertNull(processResults(putawayResult));

			var lines = GetLinesOnJob(job);
			AssertEquals("Should have split inventory.", 2, lines.Count());

			var inv1 = lines.Single(i => i.PalletID == "PLT-123");
			AssertEquals("Should have split inventory.", 3m, inv1.QuantityToPutaway);
			AssertEquals("Should have set location.", location.PK, inv1.LocationPK);

			var inv2 = lines.Single(i => i.PalletID.IsEmpty);
			AssertEquals("Should have split inventory.", 2m, inv2.QuantityToPutaway);
			AssertEquals("Should have set location.", location.PK, inv2.LocationPK);
		}

		#endregion

		#region TestPutaway_SkipLocationCacheUpdate

		public void TestPutaway_SkipLocationCacheUpdate_NotNeedToRebuildLocationCache()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory = receive.Lines[0];
			var inventories = new[] { inventory };

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var mockedLocationCacheUpdater = new Mock<IPutawayLocationCacheUpdater>();

			engineMock.Setup(em => em.RunRulesEngine(Factory, It.IsAny<NotificationBuffer>(), RulesContextType.InventoryPutaway,
					It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
					It.IsAny<Func<IEnumerable<IInputFact>>>(),
					It.IsAny<Func<ProductionRulesEngineResult, INotification>>()));
			mockedLocationCacheUpdater.Setup(mcu =>
					mcu.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, It.IsAny<ZGuid>(),
						It.IsAny<INotifications>(), 30))
				.Returns(true);

			var putawayManager = new PutawayEngineManagerForReceive(
				engineMock.Object,
				Mock.Of<ICrossDockManager>(),
				Mock.Of<IPutawayExistingPalletManager>(),
				Mock.Of<IPutawayLocationFactLoader>(),
				mockedLocationCacheUpdater.Object);
			putawayManager.Putaway(new[] { receive }, inventories, new NotificationBuffer(), null, needRebuildLocationCache: false);

			mockedLocationCacheUpdater.VerifyNoOtherCalls(); // skip rebuilding cache
			engineMock.Verify(em => em.RunRulesEngine(Factory, It.IsAny<NotificationBuffer>(), RulesContextType.InventoryPutaway,
					It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
					It.IsAny<Func<IEnumerable<IInputFact>>>(),
					It.IsAny<Func<ProductionRulesEngineResult, INotification>>()));
			engineMock.VerifyNoOtherCalls();
			AssertEquals(ZGuid.Empty, inventory.WE_WL);
		}

		public void TestPutaway_SkipLocationCacheUpdate_NeedToRebuildLocationCache()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory = receive.Lines[0];
			var inventories = new[] { inventory };

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var mockedLocationCacheUpdater = new Mock<IPutawayLocationCacheUpdater>();

			engineMock.Setup(em => em.RunRulesEngine(Factory, It.IsAny<NotificationBuffer>(), RulesContextType.InventoryPutaway,
					It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
					It.IsAny<Func<IEnumerable<IInputFact>>>(),
					It.IsAny<Func<ProductionRulesEngineResult, INotification>>())).Callback(() => receive.Lines[0].WE_WL = location.PK);
			mockedLocationCacheUpdater.Setup(mcu =>
					mcu.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, It.IsAny<ZGuid>(),
						It.IsAny<INotifications>(), 30))
				.Returns(true);

			var putawayManager = new PutawayEngineManagerForReceive(
				engineMock.Object,
				Mock.Of<ICrossDockManager>(),
				Mock.Of<IPutawayExistingPalletManager>(),
				Mock.Of<IPutawayLocationFactLoader>(),
				mockedLocationCacheUpdater.Object);
			putawayManager.Putaway(new[] { receive }, inventories, new NotificationBuffer(), null, needRebuildLocationCache: true);

			mockedLocationCacheUpdater.Verify(mcu =>
					mcu.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, It.IsAny<ZGuid>(),
						It.IsAny<INotifications>(), 30));
			mockedLocationCacheUpdater.VerifyNoOtherCalls();

			engineMock.Verify(em => em.RunRulesEngine(Factory, It.IsAny<NotificationBuffer>(), RulesContextType.InventoryPutaway,
					It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
					It.IsAny<Func<IEnumerable<IInputFact>>>(),
					It.IsAny<Func<ProductionRulesEngineResult, INotification>>()));
			engineMock.VerifyNoOtherCalls();

			AssertEquals(location.PK, inventory.WE_WL);
		}

		#endregion

		protected ProductionRule GetRule(ProductionRuleSet ruleSet, ZShort number, ZGuid productPk, int column)
		{
			var rule = Factory.New<ProductionRule>();
			rule.PRL_PRS_RuleSet = ruleSet.PK;
			rule.PRL_Name = $"RULE{number}";
			rule.PRL_Description = $"RULE{number}";
			rule.PRL_Priority = number;
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
                ""fieldPath"": ""Column"",
                ""operation"": ""equals"",
                ""value"": {column}
            }}
        ],
        ""sortByCriteria"": [
        ]
    }}
}}";
			return rule;
		}

		protected abstract TJob CreateJob(OrgHeader client, WhsWarehouse warehouse, string reference);

		protected abstract TLine CreateLine(TJob job, OrgSupplierPart part, decimal units, string palletID = "");

		protected abstract IEnumerable<TLine> GetLinesOnJob(TJob job);

		protected abstract (Func<IEnumerable<IInputFact>> GetFacts, Func<ProductionRulesEngineResult, INotification>
			ProcessResults) CallRulesEngineMockAndReturnInputOutputFunctions(
				Mock<IUserHaltableProductionRulesEngineService> engineMock,
				IPutawayLocationFactLoader locationFactLoader,
				TJob receive,
				IEnumerable<TLine> lines,
				RefEquipment equipment = null);
	}
}
