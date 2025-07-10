using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.Facts;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class AllocationFactLoaderTest : WhsTestCaseWithFactory
	{
		public void TestConstructor_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new AllocationFactLoader(null));
		}

		public void TestGetAllocationFacts_NullOrderedInventory_Throws()
		{
			var loader = new AllocationFactLoader(Mock.Of<IAvailableInventoryFactManagerFactory>());
			AssertExceptionThrown<ArgumentNullException>(
				() => loader.GetAllocationFacts(null, Mock.Of<IPickStrategy>()));
		}

		public void TestGetAllocationFacts_NullPickStrategy_Throws()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var loader = new AllocationFactLoader(Mock.Of<IAvailableInventoryFactManagerFactory>());
			AssertExceptionThrown<ArgumentNullException>(() =>
				loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(), null));
		}

		public void TestGetAllocationFacts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = (WhsPickOrderedInventory)pick.OrderedInventories.Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());

			var facts = factBatches.Single();
			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have one organisation.", 1, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());

			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var orderLineRelatedFacts = nestedFacts;
			var orderLineFact1 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == orderLine1.PK);
			AssertOrderLine(orderLineFact1, orderLineRelatedFacts, orderedInventory.PK, order, orderLine1);

			var orderLineFact2 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == orderLine2.PK);
			AssertOrderLine(orderLineFact2, orderLineRelatedFacts, orderedInventory.PK, order, orderLine2);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);

			invFactManagerFactory.Verify(f => f.GetNewManager(), Times.Once);

			loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(), GetMockedPickStrategy());
			invFactManagerFactory.Verify(f => f.GetNewManager(), Times.Exactly(2));
		}

		public void TestGetAllocationFacts_ProductStyle()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var style1 = Helper.CreateProductStyle("S1", "Style1", data.Org1.PK);
			var colour1 = style1.Colours.AddNew();
			colour1.WSC_Code = "RED";
			colour1.WSC_Description = "Color Red";
			var size1 = style1.Sizes.AddNew();
			size1.WSZ_Size = "size1";

			var style2 = Helper.CreateProductStyle("S2", "Style2", data.Org1.PK);
			var colour2 = style2.Colours.AddNew();
			colour2.WSC_Code = "WHT";
			colour2.WSC_Description = "Color White";
			var classification2 = style2.Classifications.AddNew();
			classification2.WSS_Code = "M";
			classification2.WSS_Description = "Male";
			var size2 = style2.Sizes.AddNew();
			size2.WSZ_Size = "size2";

			Factory.Save();

			// style without classification
			var part1 = Helper.CreateProduct(data.Org1, "Part1");
			var product1 = WhsProduct.GetWhsProduct(part1);
			product1.ProductStylePK = style1.PK;
			product1.ProductStyleColourPK = colour1.PK;
			product1.ProductStyleSizePK = size1.PK;
			var part1Relation = part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);

			// style with classification
			var part2 = Helper.CreateProduct(data.Org1, "Part2");
			var product2 = WhsProduct.GetWhsProduct(part2);
			product2.ProductStylePK = style2.PK;
			product2.ProductStyleClassificationPK = classification2.PK;
			product2.ProductStyleColourPK = colour2.PK;
			product2.ProductStyleSizePK = size2.PK;
			var part2Relation = part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);

			// no style
			var part3 = Helper.CreateProduct(data.Org1, "Part3");
			var product3 = WhsProduct.GetWhsProduct(part3);
			product3.ProductStylePK = ZGuid.Empty;
			var part3Relation = part3.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", part1, 50m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", part2, 50m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, part2, 20m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, part3, 30m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			var productFacts = nestedFacts.OfType<AllocationProductFact>().ToArray();
			AssertEquals("Should have one organisation.", 1, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have one product.", 3, productFacts.Length);
			var product1Fact = productFacts.Single(p => p.PK == part1Relation.PK);
			var product2Fact = productFacts.Single(p => p.PK == part2Relation.PK);
			var product3Fact = productFacts.Single(p => p.PK == part3Relation.PK);

			var orderLineFact1 = facts.OfType<OrderLineFact>().Single(ol => ol.Product.Fact.Code == part1.OP_PartNum);
			var orderLineFact2 = facts.OfType<OrderLineFact>().Single(ol => ol.Product.Fact.Code == part2.OP_PartNum);
			var orderLineFact3 = facts.OfType<OrderLineFact>().Single(ol => ol.Product.Fact.Code == part3.OP_PartNum);
			AssertEquals(nameof(OrderLineFact.Product), part1Relation.PK, orderLineFact1.Product.Fact.PK);
			AssertEquals(nameof(OrderLineFact.Product), part2Relation.PK, orderLineFact2.Product.Fact.PK);
			AssertEquals(nameof(OrderLineFact.Product), part3Relation.PK, orderLineFact3.Product.Fact.PK);

			AssertEquals(nameof(AllocationProductFact.Code), part1.OP_PartNum, product1Fact.Code);
			AssertEquals(nameof(AllocationProductFact.ProductStyleCode), "S1", product1Fact.ProductStyleCode);
			AssertEquals(nameof(AllocationProductFact.ProductStyleClassificationCode), string.Empty, product1Fact.ProductStyleClassificationCode);
			AssertEquals(nameof(AllocationProductFact.ProductStyleColorCode), colour1.WSC_Code, product1Fact.ProductStyleColorCode);
			AssertEquals(nameof(AllocationProductFact.ProductStyleSizeCode), size1.WSZ_Size, product1Fact.ProductStyleSizeCode);

			AssertEquals(nameof(AllocationProductFact.Code), part2.OP_PartNum, product2Fact.Code);
			AssertEquals(nameof(AllocationProductFact.ProductStyleCode), "S2", product2Fact.ProductStyleCode);
			AssertEquals(nameof(AllocationProductFact.ProductStyleClassificationCode), classification2.WSS_Code, product2Fact.ProductStyleClassificationCode);
			AssertEquals(nameof(AllocationProductFact.ProductStyleColorCode), colour2.WSC_Code, product2Fact.ProductStyleColorCode);
			AssertEquals(nameof(AllocationProductFact.ProductStyleSizeCode), size2.WSZ_Size, product2Fact.ProductStyleSizeCode);

			AssertEquals(nameof(AllocationProductFact.Code), part3.OP_PartNum, product3Fact.Code);
			AssertEquals(nameof(AllocationProductFact.ProductStyleCode), string.Empty, product3Fact.ProductStyleCode);
			AssertEquals(nameof(AllocationProductFact.ProductStyleClassificationCode), string.Empty, product3Fact.ProductStyleClassificationCode);
			AssertEquals(nameof(AllocationProductFact.ProductStyleColorCode), string.Empty, product3Fact.ProductStyleColorCode);
			AssertEquals(nameof(AllocationProductFact.ProductStyleSizeCode), string.Empty, product3Fact.ProductStyleSizeCode);
		}

		public void TestGetAllocationFacts_AttributeNeutral()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK).OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1");
			receiveLine1.WE_SerialNumber = "SN1";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1");
			receiveLine2.WE_SerialNumber = "SN2";

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			line2.WE_SerialNumber = "SN1";

			var line3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			line3.WE_SerialNumber = "SN2";
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().ToArray();
			var orderedInventorySn1 = orderedInventories.Single(oi => oi.SerialNumber == "SN1");
			var availableInventorySn1 = (WhsPickAvailableInventory)orderedInventorySn1.AvailableInventories.Single();

			var orderedInventorySn2 = orderedInventories.Single(oi => oi.SerialNumber == "SN2");
			var availableInventorySn2 = (WhsPickAvailableInventory)orderedInventorySn2.AvailableInventories.Single();

			var orderedInventoryNonSpecified = orderedInventories.Single(oi => oi.SerialNumber.IsEmpty);
			var availableInventoryNonSpecified = (WhsPickAvailableInventory)orderedInventoryNonSpecified.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(), GetMockedPickStrategy()).ToArray();

			var factBatch1 = factBatches[0];
			var factBatch2 = factBatches[1];
			var factBatch3 = factBatches[2];

			// Should process the specified serial numbers first
			AssertOrderLine(factBatch1.OfType<OrderLineFact>().Single(), factBatch1.GetNestedFacts(), orderedInventorySn1.PK, order, line2);
			AssertOrderLine(factBatch2.OfType<OrderLineFact>().Single(), factBatch2.GetNestedFacts(), orderedInventorySn2.PK, order, line3);
			AssertOrderLine(factBatch3.OfType<OrderLineFact>().Single(), factBatch3.GetNestedFacts(), orderedInventoryNonSpecified.PK, order, line1);

			var inventoryFactSn1 = factBatch1.OfType<AvailableInventoryFact>().Single();
			var inventoryFactSn2 = factBatch2.OfType<AvailableInventoryFact>().Single();
			var inventoryFactNonSpecified = factBatch3.OfType<AvailableInventoryFact>().Single();

			var keySn1 = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(availableInventorySn1.Inventory[0], orderedInventorySn1);
			var keySn2 = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(availableInventorySn2.Inventory[0], orderedInventorySn2);
			var keyNonSpecified = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(availableInventoryNonSpecified.Inventory[0], orderedInventoryNonSpecified);
			invFactManager.Verify(i => i.Register(keySn1, inventoryFactSn1), Times.Once);
			invFactManager.Verify(i => i.Register(keySn2, inventoryFactSn2), Times.Once);
			invFactManager.Verify(i => i.Register(keyNonSpecified, inventoryFactNonSpecified), Times.Once);

			invFactManager.Setup(i => i.Update(keySn1, inventoryFactSn1.PK, It.IsAny<decimal>()))
				.Returns(new[] { inventoryFactSn1 });

			invFactManager.Setup(i => i.Update(keySn2, inventoryFactSn2.PK, It.IsAny<decimal>()))
				.Returns(new[] { inventoryFactSn2 });

			invFactManager.Setup(i => i.Update(keyNonSpecified, inventoryFactNonSpecified.PK, It.IsAny<decimal>()))
				.Returns(new[] { inventoryFactNonSpecified });

			invFactManager.Setup(i => i.HasKeyRegistered(keyNonSpecified)).Returns(true);
			inventoryFactSn1.DecreaseQuantity(1.23m);
			invFactManager.Verify(i => i.Update(keySn1, inventoryFactSn1.PK, 1.23m), Times.Once);
			invFactManager.Verify(i => i.Update(keyNonSpecified, inventoryFactSn1.PK, 1.23m), Times.Once);
			invFactManager.Verify(i => i.HasKeyRegistered(keyNonSpecified), Times.Once);

			inventoryFactSn2.DecreaseQuantity(4.56m);
			invFactManager.Verify(i => i.Update(keySn2, inventoryFactSn2.PK, 4.56m), Times.Once);
			invFactManager.Verify(i => i.Update(keyNonSpecified, inventoryFactSn2.PK, 4.56m), Times.Once);
			invFactManager.Verify(i => i.HasKeyRegistered(keyNonSpecified), Times.Exactly(2));

			inventoryFactNonSpecified.DecreaseQuantity(7.89m);
			invFactManager.Verify(i => i.Update(keyNonSpecified, inventoryFactNonSpecified.PK, 7.89m), Times.Once);
			invFactManager.Verify(i => i.HasKeyRegistered(keyNonSpecified), Times.Exactly(2));

			invFactManager.VerifyNoOtherCalls();
		}

		public void TestGetAllocationFacts_ThrowsIfOrderedInventoryWithNoOwners()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 6m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInvs = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory1 = orderedInvs.Single(i => i.SupplierPartPK == data.Part1.PK);
			var availableInventory1 = (WhsPickAvailableInventory)orderedInventory1.AvailableInventories.Single();
			orderedInventory1.Owners.RemoveAllFromRelationship();

			var orderedInventory2 = orderedInvs.Single(i => i.SupplierPartPK == data.Part2.PK);
			var availableInventory2 = (WhsPickAvailableInventory)orderedInventory2.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);

			AssertExceptionThrown(typeof(ArgumentException),
				() => loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
					GetMockedPickStrategy()));
		}

		public void TestGetAllocationFacts_PartiallyAllocatedOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = (WhsPickOrderedInventory)pick.OrderedInventories.Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();
			availableInventory.PickLineQuantity = 4m;
			AssertEquals("Should have picked orderLine1.", 4m, orderLine1.PickLineQuantity);

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should already be allocated from.", true, location.IsAllocatedOnThisPick);

			var orderLineFact = facts.OfType<OrderLineFact>().Single();
			AssertOrderLine(orderLineFact, nestedFacts,
				orderedInventory.PK, order, orderLine1);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_FullyAllocatedOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = (WhsPickOrderedInventory)pick.OrderedInventories.Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();
			availableInventory.PickLineQuantity = 4m;
			AssertEquals("Should have picked orderLine1.", 4m, orderLine1.PickLineQuantity);

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should already be allocated from.", true, location.IsAllocatedOnThisPick);

			var orderLineFacts = facts.OfType<OrderLineFact>();
			AssertEquals("Should only create one order line fact.", 1, orderLineFacts.Count());
			AssertOrderLine(orderLineFacts.Single(), nestedFacts,
				orderedInventory.PK, order, orderLine2);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_FullyAllocatedAvailableInventoryLine()
		{
			var today = ZDateTimeOffset.Today;

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today.AddDays(-1), data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", today, data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = (WhsPickOrderedInventory)pick.OrderedInventories.Single();
			var availableInventories = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>();
			var availableInventory1 = availableInventories.Single(ai => ai.ArrivalDate == today.AddDays(-1));
			var availableInventory2 = availableInventories.Single(ai => ai.ArrivalDate == today);
			availableInventory1.PickLineQuantity = 5m;
			AssertEquals("Should have picked orderLine1.", 5m, orderLine1.PickLineQuantity);

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should already be allocated from.", true, location.IsAllocatedOnThisPick);

			var orderLineFacts = facts.OfType<OrderLineFact>();
			AssertEquals("Should only create one order line fact.", 1, orderLineFacts.Count());
			AssertOrderLine(orderLineFacts.Single(), nestedFacts,
				orderedInventory.PK, order, orderLine1);

			var availableInventoryFact2 = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact2, availableInventory2, nestedFacts);
		}

		public void TestGetAllocationFacts_Customs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			data.Whs1.DefaultLocation.WLV_WA_PutawayArea = bondedArea.PK;
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			inventory.CustomsData.WB_EntryKey = "ABC123";
			inventory.CustomsData.WB_EntryDate = ZDateTime.Today.AddDays(-2);
			inventory.CustomsData.WB_ValueForDuty = 5m;
			inventory.CustomsData.WB_BondedWhsQty = 10m;

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			Helper.SetOutwardsEntryKeyForOrderLine(order.Lines[0], "ABC");
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = (WhsPickOrderedInventory)pick.OrderedInventories.Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			var orderLineFact = facts.OfType<OrderLineFact>().Single();
			AssertOrderLine(orderLineFact, nestedFacts,
				orderedInventory.PK, order, order.Lines[0]);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_WorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, wheel.OP_StockKeepingUnit);
			Helper.CreateProductBOM(bike, frame, 1m, frame.OP_StockKeepingUnit);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", frame, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, bike, 10m);
			workOrder.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(workOrder);
			Factory.Save();

			var frameChildLine = workOrder.Lines[0].ChildComponentLines.Single(ol => ol.WE_OP == frame.PK);
			var wheelChildLine = workOrder.Lines[0].ChildComponentLines.Single(ol => ol.WE_OP == wheel.PK);

			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var frameOrderedInventory = orderedInventories.Single(oi => oi.SupplierPartPK == frame.PK);
			var frameAvailableInventory =
				(WhsPickAvailableInventory)frameOrderedInventory.AvailableInventories.Single();

			var wheelOrderedInventory = orderedInventories.Single(oi => oi.SupplierPartPK == wheel.PK);
			var wheelAvailableInventory =
				(WhsPickAvailableInventory)wheelOrderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy()).ToArray();
			var facts = factBatches.SelectMany(f => f).ToArray();

			AssertEquals("Should have 2 batches.", 2, factBatches.Length);
			AssertEquals("Should batch by ordered inventory.",
				true,
				factBatches.All(
					f => f.OfType<IOrderLineFact>().Select(olf => olf.OrderedInventoryPK)
						.Concat(f.OfType<IAvailableInventoryFact>().Select(aif => aif.OrderedInventoryPK))
						.Distinct().Count() == 1));

			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have one organisation.", 1, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have two products.", 2, nestedFacts.OfType<AllocationProductFact>().Count());

			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact1 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == frameChildLine.PK);
			AssertOrderLine(orderLineFact1, relatedOrderLineFacts, frameOrderedInventory.PK, workOrder, frameChildLine);

			var orderLineFact2 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == wheelChildLine.PK);
			AssertOrderLine(orderLineFact2, relatedOrderLineFacts, wheelOrderedInventory.PK, workOrder, wheelChildLine);

			var frameAvailableInventoryFact = facts.OfType<AvailableInventoryFact>()
				.Single(ai => ai.PK == frameAvailableInventory.PK);
			AssertAvailableInventory(invFactManager, frameAvailableInventoryFact, frameAvailableInventory, nestedFacts);

			var wheelAvailableInventoryFact = facts.OfType<AvailableInventoryFact>()
				.Single(ai => ai.PK == wheelAvailableInventory.PK);
			AssertAvailableInventory(invFactManager, wheelAvailableInventoryFact, wheelAvailableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_DynamicWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);
			setup.ReceiveStockForWorkOrder();
			Factory.Save();

			setup.WorkOrder.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(setup.WorkOrder);
			Factory.Save();

			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orangeOrderedInventory = orderedInventories.Single(oi => oi.SupplierPartPK == setup.Orange_ComponentProduct.PK);
			var orangeAvailableInventory =
				(WhsPickAvailableInventory)orangeOrderedInventory.AvailableInventories.Single();

			var waterOrderedInventory = orderedInventories.Single(oi => oi.SupplierPartPK == setup.Water_ComponentProduct.PK);
			var waterAvailableInventory =
				(WhsPickAvailableInventory)waterOrderedInventory.AvailableInventories.Single();

			var acidOrderedInventory = orderedInventories.Single(oi => oi.SupplierPartPK == setup.PotassiumBenzoate_ComponentProduct.PK);
			var acidAvailableInventory =
				(WhsPickAvailableInventory)acidOrderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy()).ToArray();
			var facts = factBatches.SelectMany(f => f).ToArray();

			AssertEquals("Should have 3 batches.", 3, factBatches.Length);
			AssertEquals("Should batch by ordered inventory.",
				true,
				factBatches.All(
					f => f.OfType<IOrderLineFact>().Select(olf => olf.OrderedInventoryPK)
						.Concat(f.OfType<IAvailableInventoryFact>().Select(aif => aif.OrderedInventoryPK))
						.Distinct().Count() == 1));

			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have one organisation.", 1, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have three products.", 3, nestedFacts.OfType<AllocationProductFact>().Count());
			AssertContainsExactElementsInAnyOrder("Should have three products.", new[] { "ORANGE", "WATER", "POTASSIUMBENZOATE" }, nestedFacts.OfType<AllocationProductFact>().Select(p => p.Code));

			var locationFact = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, locationFact.IsAllocatedOnThisPick);

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact1 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == setup.MainComponentLine_Orange.PK);
			AssertOrderLine(orderLineFact1, relatedOrderLineFacts, orangeOrderedInventory.PK, setup.WorkOrder, setup.MainComponentLine_Orange);

			var orderLineFact2 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == setup.MainComponentLine_Water.PK);
			AssertOrderLine(orderLineFact2, relatedOrderLineFacts, waterOrderedInventory.PK, setup.WorkOrder, setup.MainComponentLine_Water);

			var orderLineFact3 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == setup.MainComponentLine_PotassiumBenzoate.PK);
			AssertOrderLine(orderLineFact3, relatedOrderLineFacts, acidOrderedInventory.PK, setup.WorkOrder, setup.MainComponentLine_PotassiumBenzoate);

			var orangeAvailableInventoryFact = facts.OfType<AvailableInventoryFact>()
				.Single(ai => ai.PK == orangeAvailableInventory.PK);
			AssertAvailableInventory(invFactManager, orangeAvailableInventoryFact, orangeAvailableInventory, nestedFacts);

			var waterAvailableInventoryFact = facts.OfType<AvailableInventoryFact>()
				.Single(ai => ai.PK == waterAvailableInventory.PK);
			AssertAvailableInventory(invFactManager, waterAvailableInventoryFact, waterAvailableInventory, nestedFacts);

			var acidAvailableInventoryFact = facts.OfType<AvailableInventoryFact>()
				.Single(ai => ai.PK == acidAvailableInventory.PK);
			AssertAvailableInventory(invFactManager, acidAvailableInventoryFact, acidAvailableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_MultipleAvailableInventories()
		{
			var today = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today, data.Part1, 2m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", today, data.Part1, 3m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", today, data.Part1, 5m, location2, "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = (WhsPickOrderedInventory)pick.OrderedInventories.Single();
			var availableInventories = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>();
			var availableInventory1 = availableInventories.Single(i => i.LocationPK == location1.PK);
			var availableInventory2 = availableInventories.Single(i => i.LocationPK == location2.PK);

			availableInventory1.PickLineQuantity = 1m;
			AssertEquals("Should have picked orderLine.", 1m, orderLine.PickLineQuantity);

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have one organisation.", 1, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());
			AssertEquals("Should have two locations.", 2, nestedFacts.OfType<AllocationLocationFact>().Count());

			var location1Fact = nestedFacts.OfType<AllocationLocationFact>().Single(l => l.PK == location1.PK);
			AssertEquals("Location should already be allocated from.", true, location1Fact.IsAllocatedOnThisPick);

			var location2Fact = nestedFacts.OfType<AllocationLocationFact>().Single(l => l.PK == location2.PK);
			AssertEquals("Location should *not* already be allocated from.", false,
				location2Fact.IsAllocatedOnThisPick);

			var orderLineFact = facts.OfType<OrderLineFact>().Single();
			AssertOrderLine(orderLineFact, nestedFacts,
				orderedInventory.PK, order, orderLine);

			var availableInventory1Fact =
				facts.OfType<AvailableInventoryFact>().Single(i => i.Location.Fact.PK == location1.PK);
			AssertAvailableInventory(invFactManager, availableInventory1Fact, availableInventory1, nestedFacts);

			var availableInventory2Fact =
				facts.OfType<AvailableInventoryFact>().Single(i => i.Location.Fact.PK == location2.PK);
			AssertAvailableInventory(invFactManager, availableInventory2Fact, availableInventory2, nestedFacts);

			invFactManagerFactory.Verify(f => f.GetNewManager(), Times.Once);
		}

		public void TestGetAllocationFacts_MultipleOrderedInventories()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var part4 = Helper.CreateProduct(data.Org1, "P4");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R32", part3, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 6m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, part3, 8m); // Fully allocated already
			var orderLine4 = Helper.CreateWhsOrderLine(order, part4, 10m); // Completely short
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInvs = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory1 = orderedInvs.Single(i => i.SupplierPartPK == data.Part1.PK);
			var availableInventory1 = (WhsPickAvailableInventory)orderedInventory1.AvailableInventories.Single();

			var orderedInventory2 = orderedInvs.Single(i => i.SupplierPartPK == data.Part2.PK);
			var availableInventory2 = (WhsPickAvailableInventory)orderedInventory2.AvailableInventories.Single();

			var orderedInventory3 = orderedInvs.Single(i => i.SupplierPartPK == part3.PK);
			var availableInventory3 = (WhsPickAvailableInventory)orderedInventory3.AvailableInventories.Single();
			availableInventory3.Allocate = true;

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy()).ToArray();
			var facts = factBatches.SelectMany(f => f).ToArray();

			AssertEquals("Should have 2 batches.", 2, factBatches.Length);
			AssertEquals("Should batch by ordered inventory.",
				true,
				factBatches.All(
					f => f.OfType<IOrderLineFact>().Select(olf => olf.OrderedInventoryPK)
						.Concat(f.OfType<IAvailableInventoryFact>().Select(aif => aif.OrderedInventoryPK))
						.Distinct().Count() == 1));
			AssertEquals("Batch1 should be for orderedInventory1.", orderedInventory1.PK, factBatches[0].OfType<IOrderLineFact>().First().OrderedInventoryPK);
			AssertEquals("Batch2 should be for orderedInventory2.", orderedInventory2.PK, factBatches[1].OfType<IOrderLineFact>().First().OrderedInventoryPK);

			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have one organisation.", 1, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have two products.", 2, nestedFacts.OfType<AllocationProductFact>().Count());

			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var orderLineRelatedFacts = nestedFacts;
			var orderLineFact1 = facts.OfType<OrderLineFact>()
				.Single(ol => ol.Product.Fact.Code == data.Part1.OP_PartNum);
			AssertOrderLine(orderLineFact1, orderLineRelatedFacts, orderedInventory1.PK, order, orderLine1);

			var orderLineFact2 = facts.OfType<OrderLineFact>()
				.Single(ol => ol.Product.Fact.Code == data.Part2.OP_PartNum);
			AssertOrderLine(orderLineFact2, orderLineRelatedFacts, orderedInventory2.PK, order, orderLine2);

			var availableInventoryFact1 =
				facts.OfType<AvailableInventoryFact>().Single(i => i.PK == availableInventory1.PK);
			AssertAvailableInventory(invFactManager, availableInventoryFact1, availableInventory1, nestedFacts);

			var availableInventoryFact2 =
				facts.OfType<AvailableInventoryFact>().Single(i => i.PK == availableInventory2.PK);
			AssertAvailableInventory(invFactManager, availableInventoryFact2, availableInventory2, nestedFacts);

			invFactManagerFactory.Verify(f => f.GetNewManager(), Times.Once);
		}

		public void TestGetAllocationFacts_MultipleOrderedInventories_SameProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, "PLT", 5);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today.AddDays(-3), data.Part1, 4m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today.AddDays(-2), data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", ZDateTimeOffset.Today.AddDays(-1), data.Part1, 6m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInvs = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory1 = orderedInvs.Single(i => i.SupplierPartPK == data.Part1.PK);
			var availableInventories = orderedInventory1.AvailableInventories.Cast<WhsPickAvailableInventory>().ToArray();
			var availableInventory1 = availableInventories.Single(i => i.QuantityAvailableToPick == 4m);
			var availableInventory2 = availableInventories.Single(i => i.QuantityAvailableToPick == 5m);
			var availableInventory3 = availableInventories.Single(i => i.QuantityAvailableToPick == 6m);

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy()).ToArray();
			var facts = factBatches.Single().ToArray();

			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have one organisation.", 1, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());

			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var orderLineRelatedFacts = nestedFacts;
			var orderLineFact1 = facts.OfType<OrderLineFact>()
				.Single(ol => ol.Product.Fact.Code == data.Part1.OP_PartNum);
			AssertOrderLine(orderLineFact1, orderLineRelatedFacts, orderedInventory1.PK, order, orderLine1);

			var availableInventoryFact1 =
				facts.OfType<AvailableInventoryFact>().Single(i => i.PK == availableInventory1.PK);
			AssertAvailableInventory(invFactManager, availableInventoryFact1, availableInventory1, nestedFacts);
			AssertEquals("Should not be pallet overflow.", false, availableInventoryFact1.IsPalletOverflow);

			var availableInventoryFact2 =
				facts.OfType<AvailableInventoryFact>().Single(i => i.PK == availableInventory2.PK);
			AssertAvailableInventory(invFactManager, availableInventoryFact2, availableInventory2, nestedFacts);
			AssertEquals("Should be pallet overflow.", false, availableInventoryFact2.IsPalletOverflow);

			var availableInventoryFact3 =
				facts.OfType<AvailableInventoryFact>().Single(i => i.PK == availableInventory3.PK);
			AssertAvailableInventory(invFactManager, availableInventoryFact3, availableInventory3, nestedFacts);
			AssertEquals("Should be pallet overflow.", false, availableInventoryFact3.IsPalletOverflow);

			invFactManagerFactory.Verify(f => f.GetNewManager(), Times.Once);
		}

		public void TestGetAllocationFacts_MultipleOrderedInventories_SameProductDifferentClient()
		{
			var today = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var org2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today, data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", today, data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var order1Line = Helper.CreateWhsOrderLine(order1, data.Part1, 4m);

			var order2 = Helper.CreateWhsOrder(org2, data.Whs1, "O2", WhsPickOption.Codes.Manual);
			var order2Line = Helper.CreateWhsOrderLine(order2, data.Part1, 4m);

			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.Single(ordInv => ordInv.ClientPK == data.Org1.PK);
			var availableInventory1 = orderedInventory1.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();

			var orderedInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.Single(ordInv => ordInv.ClientPK == org2.PK);
			var availableInventory2 = orderedInventory2.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy()).ToArray();
			var facts = factBatches.SelectMany(f => f).ToArray();

			AssertEquals("Should have 2 batches.", 2, factBatches.Length);
			AssertEquals("Should batch by ordered inventory.",
				true,
				factBatches.All(
					f => f.OfType<IOrderLineFact>().Select(olf => olf.OrderedInventoryPK)
						.Concat(f.OfType<IAvailableInventoryFact>().Select(aif => aif.OrderedInventoryPK))
						.Distinct().Count() == 1));
			AssertEquals("Batch1 should be for orderedInventory1.", orderedInventory1.PK, factBatches[0].OfType<IOrderLineFact>().First().OrderedInventoryPK);
			AssertEquals("Batch2 should be for orderedInventory2.", orderedInventory2.PK, factBatches[1].OfType<IOrderLineFact>().First().OrderedInventoryPK);

			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have two organisations.", 2, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have two products.", 2, nestedFacts.OfType<AllocationProductFact>().Count());

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact1 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == order1Line.PK);
			AssertOrderLine(orderLineFact1, relatedOrderLineFacts, orderedInventory1.PK, order1, order1Line);

			var orderLineFact2 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == order2Line.PK);
			AssertOrderLine(orderLineFact2, relatedOrderLineFacts, orderedInventory2.PK, order2, order2Line);

			var availableInventoryFact1 =
				facts.OfType<AvailableInventoryFact>().Single(i => i.PK == availableInventory1.PK);
			AssertAvailableInventory(invFactManager, availableInventoryFact1, availableInventory1, nestedFacts);

			var availableInventoryFact2 =
				facts.OfType<AvailableInventoryFact>().Single(i => i.PK == availableInventory2.PK);
			AssertAvailableInventory(invFactManager, availableInventoryFact2, availableInventory2, nestedFacts);

			invFactManagerFactory.Verify(f => f.GetNewManager(), Times.Once);
		}

		public void TestGetAllocationFacts_OrderProperties()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var rateTransportProvider = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", data.Org1);
			var address = Helper.SetUpOrgAddress("12 Alexandria Street", "Alexandria", "2015", "SYDNEY", "NSW", "AUALX",
				data.Org1);

			var rateTransportZoneSydney = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Sydney");
			Helper.SetUpRateTransportZoneItem(rateTransportZoneSydney, "AU", "2015");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			order.WD_ExternalReference = "ORDER #1";
			order.WD_CustomerReference = "CUSTOMER REF";
			order.WD_RequiredDate = ZDateTimeOffset.Today.AddDays(3);
			order.WD_RS_NKServiceLevel = "STD";
			order.WD_PL_NKCarrierServiceLevel = "ABC";
			order.WD_PickPriority = 2;
			order.TransportCoPK = data.Org1.PK;
			order.ConsigneeAddressPK = address.PK;
			AssertEquals("Precondition: TransportZoneName", "Sydney", order.TransportZoneName);

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = (WhsPickOrderedInventory)pick.OrderedInventories.Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have one organisation.", 1, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());

			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var orderLineFact = facts.OfType<OrderLineFact>().Single();
			AssertOrderLine(orderLineFact, nestedFacts,
				orderedInventory.PK, order, orderLine);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_InventoryProperties()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			var owner = data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK,
				OrgPartRelation.RelationshipTypes.Owner);
			owner.OU_PickMode = WhsPickMode.Codes.AttributeSpecified;

			var date1 = ZDate.Today.AddDays(1);
			var date2 = date1.AddDays(1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "", date1, date2,
				"ABC", "DEF", "GHI", "SN1", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = (WhsPickOrderedInventory)pick.OrderedInventories.Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have one organisation.", 1, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());

			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var orderLineFact = facts.OfType<OrderLineFact>().Single();
			AssertOrderLine(orderLineFact, nestedFacts,
				orderedInventory.PK, order, orderLine);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_SharedInventoryAcrossFacts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var date1 = ZDate.Today.AddDays(1);
			var date2 = date1.AddDays(1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine =
				Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 10m, data.Whs1.DefaultLocation.PK, "");
			receiveLine.WE_PartAttrib1 = "BLUE";
			receiveLine.WE_PartAttrib2 = "LARGE";

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine1.WE_PartAttrib1 = "BLUE";

			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine2.WE_PartAttrib2 = "LARGE";

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInvs = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventory1 = orderedInvs.Single(oi => oi.PartAttrib1 == "BLUE");
			var availableInventory1 = (WhsPickAvailableInventory)orderedInventory1.AvailableInventories.Single();

			var orderedInventory2 = orderedInvs.Single(oi => oi.PartAttrib2 == "LARGE");
			var availableInventory2 = (WhsPickAvailableInventory)orderedInventory2.AvailableInventories.Single();

			AssertContainsExactElementsInAnyOrder("Precondition: Shared inventory.", availableInventory1.Inventory,
				availableInventory2.Inventory);

			var pickStrategyMock = new Mock<IPickStrategy>();
			pickStrategyMock.Setup(ps => ps.GetQuantityUnPicked(It.IsAny<WhsPickAvailableInventory>()))
				.Returns<WhsPickAvailableInventory>(ai => ai.QuantityUnPicked);

			var loader = new AllocationFactLoader(new AvailableInventoryFactManagerFactory());
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				pickStrategyMock.Object).ToArray();
			var facts = factBatches.SelectMany(f => f).ToArray();

			AssertEquals("Should have 2 batches.", 2, factBatches.Length);
			AssertEquals("Should batch by ordered inventory.",
				true,
				factBatches.All(
					f => f.OfType<IOrderLineFact>().Select(olf => olf.OrderedInventoryPK)
						.Concat(f.OfType<IAvailableInventoryFact>().Select(aif => aif.OrderedInventoryPK))
						.Distinct().Count() == 1));
			AssertEquals("Batch1 should be for orderedInventory1.", orderedInventory1.PK, factBatches[0].OfType<IOrderLineFact>().First().OrderedInventoryPK);
			AssertEquals("Batch2 should be for orderedInventory2.", orderedInventory2.PK, factBatches[1].OfType<IOrderLineFact>().First().OrderedInventoryPK);

			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have one organisation.", 1, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());

			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact1 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == orderLine1.PK);
			AssertOrderLine(orderLineFact1, relatedOrderLineFacts, orderedInventory1.PK, order, orderLine1);

			var orderLineFact2 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == orderLine2.PK);
			AssertOrderLine(orderLineFact2, relatedOrderLineFacts, orderedInventory2.PK, order, orderLine2);

			var availableInventoryFact1 =
				facts.OfType<AvailableInventoryFact>().Single(ai => ai.PK == availableInventory1.PK);
			AssertAvailableInventory(null, availableInventoryFact1, availableInventory1, nestedFacts);

			var availableInventoryFact2 =
				facts.OfType<AvailableInventoryFact>().Single(ai => ai.PK == availableInventory2.PK);
			AssertAvailableInventory(null, availableInventoryFact2, availableInventory2, nestedFacts);

			AssertEquals("Should have 10m available.", 10m, availableInventoryFact1.Quantity);
			AssertEquals("Should have 10m available.", 10m, availableInventoryFact2.Quantity);
			pickStrategyMock.Verify(
				ps => ps.OnInventoryPicked(It.IsAny<WhsPickOrderedInventory>(), It.IsAny<WhsPickAvailableInventory>(),
					It.IsAny<ZDecimal>()), Times.Never);

			availableInventoryFact1.DecreaseQuantity(2m);
			AssertEquals("Should have 8m available.", 8m, availableInventoryFact1.Quantity);
			AssertEquals("Should have 8m available.", 8m, availableInventoryFact2.Quantity);
			pickStrategyMock.Verify(ps => ps.OnInventoryPicked(orderedInventory1, availableInventory1, 2m), Times.Once);
			pickStrategyMock.Verify(
				ps => ps.OnInventoryPicked(It.IsAny<WhsPickOrderedInventory>(), It.IsAny<WhsPickAvailableInventory>(),
					It.IsAny<ZDecimal>()), Times.Once);

			availableInventoryFact2.DecreaseQuantity(3m);
			AssertEquals("Should have 5m available.", 5m, availableInventoryFact1.Quantity);
			AssertEquals("Should have 5m available.", 5m, availableInventoryFact2.Quantity);
			pickStrategyMock.Verify(ps => ps.OnInventoryPicked(orderedInventory2, availableInventory2, 3m), Times.Once);
			pickStrategyMock.Verify(
				ps => ps.OnInventoryPicked(It.IsAny<WhsPickOrderedInventory>(), It.IsAny<WhsPickAvailableInventory>(),
					It.IsAny<ZDecimal>()), Times.Exactly(2));
		}

		public void TestGetAllocationFacts_MultipleOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			order1.WD_ExternalReference = "ORDER #1";
			order1.WD_CustomerReference = "CUSTOMER REF";
			order1.WD_RequiredDate = ZDateTimeOffset.Today.AddDays(3);
			order1.WD_RS_NKServiceLevel = "STD";
			order1.WD_PL_NKCarrierServiceLevel = "ABC";
			order1.WD_PickPriority = 7;

			var order1Line = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			order2.WD_ExternalReference = "ORDER #2";
			order2.WD_CustomerReference = "WANT IT NOW";
			order2.WD_RequiredDate = order1.WD_RequiredDate.AddDays(-3);
			order2.WD_RS_NKServiceLevel = "EXP";
			order2.WD_PL_NKCarrierServiceLevel = "VIP";
			order2.WD_PickPriority = 1;

			var order2Line = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var orderedInventory = (WhsPickOrderedInventory)pick.OrderedInventories.Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have one organisation.", 1, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());

			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact1 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == order1Line.PK);
			AssertOrderLine(orderLineFact1, relatedOrderLineFacts, orderedInventory.PK, order1, order1Line);

			var orderLineFact2 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == order2Line.PK);
			AssertOrderLine(orderLineFact2, relatedOrderLineFacts, orderedInventory.PK, order2, order2Line);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);

			invFactManagerFactory.Verify(f => f.GetNewManager(), Times.Once);
		}

		public void TestGetAllocationFacts_MultipleOrders_Clients()
		{
			var today = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var org2 = Helper.CreateClient("O2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today, data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", today, data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var order1Line = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", WhsPickOption.Codes.Manual);
			var order2Line = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);

			var order3 = Helper.CreateWhsOrder(org2, data.Whs1, "O3", WhsPickOption.Codes.Manual);
			var order3Line = Helper.CreateWhsOrderLine(order3, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order1, order2, order3);
			Factory.Save();

			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var orderedInventoryOrg1 = orderedInventories.Single(oi => oi.ClientPK == data.Org1.PK);
			var availableInventory1 = (WhsPickAvailableInventory)orderedInventoryOrg1.AvailableInventories.Single();

			var orderedInventoryOrg2 = orderedInventories.Single(oi => oi.ClientPK == org2.PK);
			var availableInventory2 = (WhsPickAvailableInventory)orderedInventoryOrg2.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have two organisations.", 2, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have two products.", 2, nestedFacts.OfType<AllocationProductFact>().Count());

			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact1 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == order1Line.PK);
			AssertOrderLine(orderLineFact1, relatedOrderLineFacts, orderedInventoryOrg1.PK, order1, order1Line);

			var orderLineFact2 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == order2Line.PK);
			AssertOrderLine(orderLineFact2, relatedOrderLineFacts, orderedInventoryOrg1.PK, order2, order2Line);

			var orderLineFact3 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == order3Line.PK);
			AssertOrderLine(orderLineFact3, relatedOrderLineFacts, orderedInventoryOrg2.PK, order3, order3Line);

			var availableInventory1Fact =
				facts.OfType<AvailableInventoryFact>().Single(i => i.PK == availableInventory1.PK);
			AssertAvailableInventory(invFactManager, availableInventory1Fact, availableInventory1, nestedFacts);

			var availableInventory2Fact =
				facts.OfType<AvailableInventoryFact>().Single(i => i.PK == availableInventory2.PK);
			AssertAvailableInventory(invFactManager, availableInventory2Fact, availableInventory2, nestedFacts);

			invFactManagerFactory.Verify(f => f.GetNewManager(), Times.Once);
		}

		public void TestGetAllocationFacts_MultipleOrders_Consignee() =>
			TestGetAllocationFacts_MultipleOrders_DocAddressOrg((order, org) => order.ConsigneePK = org.PK);

		public void TestGetAllocationFacts_MultipleOrders_TransportCompany() =>
			TestGetAllocationFacts_MultipleOrders_DocAddressOrg((order, org) => order.TransportCoPK = org.PK);

		public void TestGetAllocationFacts_MultipleOrders_DistributionCentre() =>
			TestGetAllocationFacts_MultipleOrders_DocAddressOrg((order, org) =>
				order.DistributionCentreDocAddress.OrganisationPK = org.PK);

		void TestGetAllocationFacts_MultipleOrders_DocAddressOrg(Action<WhsOrder, OrgHeader> setDocAddressOrg)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var otherOrg1 = Helper.CreateClient("OO1");
			var otherOrg2 = Helper.CreateClient("OO2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			setDocAddressOrg(order1, otherOrg1);
			var order1Line = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", WhsPickOption.Codes.Manual);
			setDocAddressOrg(order2, otherOrg1);
			var order2Line = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3", WhsPickOption.Codes.Manual);
			setDocAddressOrg(order3, otherOrg2);
			var order3Line = Helper.CreateWhsOrderLine(order3, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order1, order2, order3);
			Factory.Save();

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have three organisations.", 3, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());

			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact1 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == order1Line.PK);
			AssertOrderLine(orderLineFact1, relatedOrderLineFacts, orderedInventory.PK, order1, order1Line);

			var orderLineFact2 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == order2Line.PK);
			AssertOrderLine(orderLineFact2, relatedOrderLineFacts, orderedInventory.PK, order2, order2Line);

			var orderLineFact3 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == order3Line.PK);
			AssertOrderLine(orderLineFact3, relatedOrderLineFacts, orderedInventory.PK, order3, order3Line);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_Consignee_IsOrgProxyOfCurrentCompany()
			=> TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfCurrentCompanyCore((order, org) => order.ConsigneePK = org.PK);

		public void TestGetAllocationFacts_TransportCompany_IsOrgProxyOfCurrentCompany()
			=> TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfCurrentCompanyCore((order, org) => order.TransportCoPK = org.PK);

		public void TestGetAllocationFacts_DistributionCentre_IsOrgProxyOfCurrentCompany()
			=> TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfCurrentCompanyCore((order, org) => order.DistributionCentreDocAddress.OrganisationPK = org.PK);

		void TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfCurrentCompanyCore(Action<WhsOrder, OrgHeader> setDocAddressOrg)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var otherOrg = Helper.CreateClient("OO1");
			var company = GlbCompany.GetCurrentCompany(Factory);
			company.GC_OH_OrgProxy = otherOrg.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			setDocAddressOrg(order, otherOrg);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(), GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();

			var orgFact = nestedFacts.OfType<OrganisationFact>().Single(fact => fact.PK == otherOrg.PK);
			AssertEquals(nameof(OrganisationFact.Code), otherOrg.OH_Code, orgFact.Code);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), true, orgFact.IsProxyOrgOfCurrentCompany);

			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());
			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact = facts.OfType<OrderLineFact>().Single(ol => ol.PK == orderLine.PK);
			AssertOrderLine(orderLineFact, relatedOrderLineFacts, orderedInventory.PK, order, orderLine);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_Client_IsOrgProxyOfCurrentCompany()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var company = GlbCompany.GetCurrentCompany(Factory);
			company.GC_OH_OrgProxy = data.Org1.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(), GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();

			var orgFact = nestedFacts.OfType<OrganisationFact>().Single(fact => fact.PK == data.Org1.PK);
			AssertEquals(nameof(OrganisationFact.Code), data.Org1.OH_Code, orgFact.Code);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), true, orgFact.IsProxyOrgOfCurrentCompany);

			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());
			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact = facts.OfType<OrderLineFact>().Single(ol => ol.PK == orderLine.PK);
			AssertOrderLine(orderLineFact, relatedOrderLineFacts, orderedInventory.PK, order, orderLine);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_Consignee_IsOrgProxyOfCurrentCompany_BranchOrgProxy()
			=> TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfCurrentCompany_BranchOrgProxyCore((order, org) => order.ConsigneePK = org.PK);

		public void TestGetAllocationFacts_TransportCompany_IsOrgProxyOfCurrentCompany_BranchOrgProxy()
			=> TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfCurrentCompany_BranchOrgProxyCore((order, org) => order.TransportCoPK = org.PK);

		public void TestGetAllocationFacts_DistributionCentre_IsOrgProxyOfCurrentCompany_BranchOrgProxy()
			=> TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfCurrentCompany_BranchOrgProxyCore((order, org) => order.DistributionCentreDocAddress.OrganisationPK = org.PK);

		void TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfCurrentCompany_BranchOrgProxyCore(Action<WhsOrder, OrgHeader> setDocAddressOrg)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var otherOrg = Helper.CreateClient("OO1");
			var company = GlbCompany.GetCurrentCompany(Factory);
			var branch = company.FirstActiveBranch;
			branch.GB_OH_OrgProxy = otherOrg.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			setDocAddressOrg(order, otherOrg);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(), GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();

			var orgFact = nestedFacts.OfType<OrganisationFact>().Single(fact => fact.PK == otherOrg.PK);
			AssertEquals(nameof(OrganisationFact.Code), otherOrg.OH_Code, orgFact.Code);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), true, orgFact.IsProxyOrgOfCurrentCompany);

			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());
			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact = facts.OfType<OrderLineFact>().Single(ol => ol.PK == orderLine.PK);
			AssertOrderLine(orderLineFact, relatedOrderLineFacts, orderedInventory.PK, order, orderLine);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_Client_IsOrgProxyOfCurrentCompany_BranchOrgProxy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var company = GlbCompany.GetCurrentCompany(Factory);
			var branch = company.FirstActiveBranch;
			branch.GB_OH_OrgProxy = data.Org1.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(), GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();

			var orgFact = nestedFacts.OfType<OrganisationFact>().Single(fact => fact.PK == data.Org1.PK);
			AssertEquals(nameof(OrganisationFact.Code), data.Org1.OH_Code, orgFact.Code);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), true, orgFact.IsProxyOrgOfCurrentCompany);

			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());
			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact = facts.OfType<OrderLineFact>().Single(ol => ol.PK == orderLine.PK);
			AssertOrderLine(orderLineFact, relatedOrderLineFacts, orderedInventory.PK, order, orderLine);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_Consignee_IsOrgProxyOfAnyCompany()
			=> TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfAnyCompanyCore((order, org) => order.ConsigneePK = org.PK);

		public void TestGetAllocationFacts_TransportCompany_IsOrgProxyOfAnyCompany()
			=> TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfAnyCompanyCore((order, org) => order.TransportCoPK = org.PK);

		public void TestGetAllocationFacts_DistributionCentre_IsOrgProxyOfAnyCompany()
			=> TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfAnyCompanyCore((order, org) => order.DistributionCentreDocAddress.OrganisationPK = org.PK);

		void TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfAnyCompanyCore(Action<WhsOrder, OrgHeader> setDocAddressOrg)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var otherOrg = Helper.CreateClient("OO1");
			var company = Factory.New<GlbCompany>();
			company.GC_OH_OrgProxy = otherOrg.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			setDocAddressOrg(order, otherOrg);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(), GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();

			var orgFact = nestedFacts.OfType<OrganisationFact>().Single(fact => fact.PK == otherOrg.PK);
			AssertEquals(nameof(OrganisationFact.Code), otherOrg.OH_Code, orgFact.Code);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), false, orgFact.IsProxyOrgOfCurrentCompany);

			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());
			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact = facts.OfType<OrderLineFact>().Single(ol => ol.PK == orderLine.PK);
			AssertOrderLine(orderLineFact, relatedOrderLineFacts, orderedInventory.PK, order, orderLine);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_Client_IsOrgProxyOfAnyCompany()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var company = Factory.New<GlbCompany>();
			company.GC_OH_OrgProxy = data.Org1.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(), GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();

			var orgFact = nestedFacts.OfType<OrganisationFact>().Single(fact => fact.PK == data.Org1.PK);
			AssertEquals(nameof(OrganisationFact.Code), data.Org1.OH_Code, orgFact.Code);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), false, orgFact.IsProxyOrgOfCurrentCompany);

			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());
			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact = facts.OfType<OrderLineFact>().Single(ol => ol.PK == orderLine.PK);
			AssertOrderLine(orderLineFact, relatedOrderLineFacts, orderedInventory.PK, order, orderLine);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_Consignee_IsOrgProxyOfAnyCompany_BranchOrgProxy()
			=> TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfAnyCompany_BranchOrgProxyCore((order, org) => order.ConsigneePK = org.PK);

		public void TestGetAllocationFacts_TransportCompany_IsOrgProxyOfAnyCompany_BranchOrgProxy()
			=> TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfAnyCompany_BranchOrgProxyCore((order, org) => order.TransportCoPK = org.PK);

		public void TestGetAllocationFacts_DistributionCentre_IsOrgProxyOfAnyCompany_BranchOrgProxy()
			=> TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfAnyCompany_BranchOrgProxyCore((order, org) => order.DistributionCentreDocAddress.OrganisationPK = org.PK);

		void TestGetAllocationFacts_DocAddressOrg_IsOrgProxyOfAnyCompany_BranchOrgProxyCore(Action<WhsOrder, OrgHeader> setDocAddressOrg)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var otherOrg = Helper.CreateClient("OO1");
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = otherOrg.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			setDocAddressOrg(order, otherOrg);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(), GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();

			var orgFact = nestedFacts.OfType<OrganisationFact>().Single(fact => fact.PK == otherOrg.PK);
			AssertEquals(nameof(OrganisationFact.Code), otherOrg.OH_Code, orgFact.Code);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), false, orgFact.IsProxyOrgOfCurrentCompany);

			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());
			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact = facts.OfType<OrderLineFact>().Single(ol => ol.PK == orderLine.PK);
			AssertOrderLine(orderLineFact, relatedOrderLineFacts, orderedInventory.PK, order, orderLine);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_Client_IsOrgProxyOfAnyCompany_BranchOrgProxy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = data.Org1.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(), GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();

			var orgFact = nestedFacts.OfType<OrganisationFact>().Single(fact => fact.PK == data.Org1.PK);
			AssertEquals(nameof(OrganisationFact.Code), data.Org1.OH_Code, orgFact.Code);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfAnyCompany), true, orgFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(OrganisationFact.IsProxyOrgOfCurrentCompany), false, orgFact.IsProxyOrgOfCurrentCompany);

			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());
			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact = facts.OfType<OrderLineFact>().Single(ol => ol.PK == orderLine.PK);
			AssertOrderLine(orderLineFact, relatedOrderLineFacts, orderedInventory.PK, order, orderLine);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventory, nestedFacts);
		}

		public void TestGetAllocationFacts_AllAddressesForSomeOrganisation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			order.ConsigneePK = data.Org1.PK;
			order.TransportCoPK = data.Org1.PK;
			order.DistributionCentreDocAddress.OrganisationPK = data.Org1.PK;

			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have one organisation.", 1, nestedFacts.OfType<OrganisationFact>().Count());

			var orderLineFact1 = facts.OfType<OrderLineFact>().Single();
			AssertOrderLine(orderLineFact1, nestedFacts,
				pick.OrderedInventories.Single().PK, order, orderLine1);
		}

		public void TestGetAllocationFacts_Product()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var category1 = Factory.New<OrgPartCategory>();
			category1.OPC_CategoryCode = "CAT";
			category1.OPC_CategoryDescription = "DESC";

			var part1Relation =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			part1Relation.OU_OPC_Category = category1.PK;
			data.Part1.OP_RH_NKCommodityCode = "HAZ";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = (WhsPickOrderedInventory)pick.OrderedInventories.Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have one organisation.", 1, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());

			var productFact = nestedFacts.OfType<AllocationProductFact>().Single();
			AssertEquals(nameof(AllocationProductFact.Code), data.Part1.OP_PartNum, productFact.Code);
			AssertEquals(nameof(AllocationProductFact.CommodityCode), "HAZ", productFact.CommodityCode);
			AssertEquals(nameof(AllocationProductFact.CategoryCode), "CAT", productFact.CategoryCode);
		}

		public void TestGetAllocationFacts_PickFaces()
		{
			var today = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var org2 = Helper.CreateClient("C2");
			var whs2 = Helper.CreateWarehouse("W2", "A");
			Factory.Save();

			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Helper.CreateProductPickFace(data.Part1, data.Org1, data.Whs1, "A-2");
			Helper.CreateProductPickFace(data.Part2, data.Org1, whs2, whs2.DefaultLocation.WLV_LocationString);
			Factory.Save();

			var relation1 = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>()
				.Single(r => r.OU_OH == data.Org1.PK);
			var relation2 = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(r => r.OU_OH == org2.PK);
			var relation3 = data.Part2.RelatedOrganisations.Cast<OrgPartRelation>()
				.Single(r => r.OU_OH == data.Org1.PK);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today, data.Part1, 10m,
				data.Whs1.DefaultLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", today, data.Part2, 10m,
				data.Whs1.DefaultLocation, "");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R3", today, data.Part1, 10m,
				data.Whs1.DefaultLocation, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 3m);
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part2, 3m);

			var order2 = Helper.CreateWhsOrder(org2, data.Whs1, "O2", WhsPickOption.Codes.Manual);
			var order2Line = Helper.CreateWhsOrderLine(order2, data.Part1, 3m);

			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have three products.", 3, nestedFacts.OfType<AllocationProductFact>().Count());

			var product1_Org1 = nestedFacts.OfType<AllocationProductFact>().Single(p => p.PK == relation1.PK);
			AssertEquals(nameof(AllocationProductFact.HasPickFaces), true, product1_Org1.HasPickFaces);

			var product1_Org2 = nestedFacts.OfType<AllocationProductFact>().Single(p => p.PK == relation2.PK);
			AssertEquals(nameof(AllocationProductFact.HasPickFaces), false, product1_Org2.HasPickFaces);

			var product2 = nestedFacts.OfType<AllocationProductFact>().Single(p => p.PK == relation3.PK);
			AssertEquals(nameof(AllocationProductFact.HasPickFaces), false, product2.HasPickFaces);
		}

		[TestDate(2022, 02, 02)]
		public void TestGetAllocationFacts_DynamicPickFaces()
		{
			var today = ZDateTimeOffset.Today;
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

			var productParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			var org2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Helper.CreateProductPickFace(data.Part1, data.Org1, data.Whs1, "A-2");
			Factory.Save();

			var relation1 = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>()
				.Single(r => r.OU_OH == data.Org1.PK);
			var relation2 = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(r => r.OU_OH == org2.PK);
			var relation3 = data.Part2.RelatedOrganisations.Cast<OrgPartRelation>()
				.Single(r => r.OU_OH == data.Org1.PK);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today, data.Part1, 10m,
				data.Whs1.DefaultLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", today, data.Part2, 10m,
				data.Whs1.DefaultLocation, "");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R3", today, data.Part1, 10m,
				data.Whs1.DefaultLocation, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 3m);
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part2, 3m);

			var order2 = Helper.CreateWhsOrder(org2, data.Whs1, "O2", WhsPickOption.Codes.Manual);
			var order2Line = Helper.CreateWhsOrderLine(order2, data.Part1, 3m);

			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have three products.", 3, nestedFacts.OfType<AllocationProductFact>().Count());

			var product1_Org1 = nestedFacts.OfType<AllocationProductFact>().Single(p => p.PK == relation1.PK);
			AssertEquals(nameof(AllocationProductFact.IsDynamic), true, product1_Org1.IsDynamic);

			var product1_Org2 = nestedFacts.OfType<AllocationProductFact>().Single(p => p.PK == relation2.PK);
			AssertEquals(nameof(AllocationProductFact.IsDynamic), false, product1_Org2.IsDynamic);

			var product2 = nestedFacts.OfType<AllocationProductFact>().Single(p => p.PK == relation3.PK);
			AssertEquals(nameof(AllocationProductFact.IsDynamic), false, product2.IsDynamic);
		}

		public void TestGetAllocationFacts_WithDynamicPickAreaOverride()
		{
			var today = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);

			var dynamicLocation = data.Whs1.FindLocation("A-1-2");
			var dynamicArea = Helper.CreateDynamicPF(data.Whs1, dynamicLocation);

			var normalLocation = data.Whs1.FindLocation("A-1-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today, data.Part1, 10m, "", normalLocation,
				"");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", today, data.Part1, 10m, "", normalLocation,
				"");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_WP_PickBeingReplenished = pick.PK;
			Factory.Save();

			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, normalLocation, dynamicLocation);
			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);
			Factory.Save();

			var orderedInventory = (WhsPickOrderedInventory)pick.OrderedInventories.Single();
			var availableInventoryDynamic = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>()
				.Single(ai => ai.LocationPK == dynamicLocation.PK);

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have one organisation.", 1, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());

			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var orderLineFact1 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == orderLine1.PK);
			AssertOrderLine(orderLineFact1, nestedFacts,
				orderedInventory.PK, order, orderLine1);

			AssertEquals("Should only have one available inventory, filtered by dynamic pick area override.", 1,
				facts.OfType<AvailableInventoryFact>().Count());
			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(invFactManager, availableInventoryFact, availableInventoryDynamic, nestedFacts);
		}

		public void TestGetAllocationFacts_PickStrategy() =>
			TestGetAllocationFacts_PickStrategy(canAllocateInQuantitiesDifferentToAutoAllocateQuantity: true);

		public void TestGetAllocationFacts_PickStrategy_CannotAllocate() =>
			TestGetAllocationFacts_PickStrategy(canAllocateInQuantitiesDifferentToAutoAllocateQuantity: false);

		public void TestGetAllocationFacts_PickStrategy(bool canAllocateInQuantitiesDifferentToAutoAllocateQuantity)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventory = (WhsPickOrderedInventory)pick.OrderedInventories.Single();
			var availableInventory = (WhsPickAvailableInventory)orderedInventory.AvailableInventories.Single();
			var key = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(availableInventory.Inventory[0], orderedInventory);

			var pickStrategy = new Mock<IPickStrategy>();
			pickStrategy.Setup(ps => ps.CanAllocateInQuantitiesDifferentToAutoAllocateQuantity(availableInventory))
				.Returns(canAllocateInQuantitiesDifferentToAutoAllocateQuantity);
			pickStrategy.Setup(ps => ps.GetQuantityUnPicked(availableInventory))
				.Returns(availableInventory.QuantityUnPicked);

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
				pickStrategy.Object);
			var facts = factBatches.SelectMany(f => f).ToArray();
			var nestedFacts = facts.GetNestedFacts();
			AssertEquals("Should have one organisation.", 1, nestedFacts.OfType<OrganisationFact>().Count());
			AssertEquals("Should have one product.", 1, nestedFacts.OfType<AllocationProductFact>().Count());

			var location = nestedFacts.OfType<AllocationLocationFact>().Single();
			AssertEquals("Location should *not* already be allocated from.", false, location.IsAllocatedOnThisPick);

			var relatedOrderLineFacts = nestedFacts;
			var orderLineFact1 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == orderLine1.PK);
			AssertOrderLine(orderLineFact1, relatedOrderLineFacts, orderedInventory.PK, order, orderLine1);

			var orderLineFact2 = facts.OfType<OrderLineFact>().Single(ol => ol.PK == orderLine2.PK);
			AssertOrderLine(orderLineFact2, relatedOrderLineFacts, orderedInventory.PK, order, orderLine2);

			var availableInventoryFact = facts.OfType<AvailableInventoryFact>().Single();
			AssertAvailableInventory(null, availableInventoryFact, availableInventory, nestedFacts);

			AssertEquals(canAllocateInQuantitiesDifferentToAutoAllocateQuantity,
				availableInventoryFact.CanAllocateInQuantitiesDifferentToPickStrategy);
			AssertEquals(canAllocateInQuantitiesDifferentToAutoAllocateQuantity,
				availableInventoryFact.CanAllocateInQuantitiesDifferentToPickStrategy);
			pickStrategy.Verify(ps => ps.CanAllocateInQuantitiesDifferentToAutoAllocateQuantity(availableInventory),
				Times.Once);
			pickStrategy.Verify(ps => ps.GetQuantityUnPicked(availableInventory),
				Times.Once);

			pickStrategy.Setup(ps => ps.GetAutoAllocateQuantity(10m, availableInventory)).Returns(9m);
			AssertEquals(9m, availableInventoryFact.GetQuantityThatCanBeAllocatedViaPickStrategy(10m));
			pickStrategy.Verify(ps => ps.GetAutoAllocateQuantity(10m, availableInventory), Times.Once);

			pickStrategy.Setup(ps => ps.GetAutoAllocateQuantity(5m, availableInventory)).Returns(5m);
			AssertEquals(5m, availableInventoryFact.GetQuantityThatCanBeAllocatedViaPickStrategy(5m));
			pickStrategy.Verify(ps => ps.GetAutoAllocateQuantity(5m, availableInventory), Times.Once);

			invFactManager.Setup(i => i.Update(key, availableInventoryFact.PK, 1.23m))
				.Returns(new[] { availableInventoryFact });

			availableInventoryFact.DecreaseQuantity(1.23m);
			invFactManager.Verify(i => i.Update(key, availableInventoryFact.PK, 1.23m), Times.Once);
			pickStrategy.Verify(
				ps => ps.OnInventoryPicked(availableInventory.OrderedInventory, availableInventory, 1.23m), Times.Once);
		}

		public void TestGetAllocationFacts_NoOrderedInventories()
		{
			var pick = Factory.New<WhsPick>();
			AssertEquals("Should be no Ordered Inventories on a Pick with no Orders.", 0,
				pick.OrderedInventories.Count);
			Factory.Save();

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			IEnumerable<IEnumerable<IInputFact>> facts = null;
			AssertNoExceptionThrown("GetAllocationFacts with no ordered inventories should not throw exception.",
				() => facts = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
					GetMockedPickStrategy()));
			AssertNotNull(facts);
			Assert(!facts.Any());
		}

		public void TestGetAllocationFacts_NotValidOrder_InvalidRequiredDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			order.WD_RequiredDate = ZDateTimeOffset.Empty;
			Factory.Save();

			var loader = new AllocationFactLoader(Mock.Of<IAvailableInventoryFactManagerFactory>());
			AssertExceptionThrown<FactLoadingException>("Error should be thrown when order is invalid.",
				"Order No. O1 cannot be allocated, Required Date is not valid.",
				() => loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
					GetMockedPickStrategy()));
		}

		public void TestGetAllocationFacts_IgnorePickByBOMKitInventoryLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 16m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, bike, 1m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 10m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			pick = newFactory.Load<WhsPick>(pick.PK);
			var bikeOrderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.ProductCode == "BIKE");
			AssertEquals("Inventory for assembled bike should exist.", 2, bikeOrderedInventory.AvailableInventories.Count);
			var bikeAvailableInventory = bikeOrderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().SingleOrDefault(ai => ai.IsPickByBOMKitInventory);
			AssertNotNull("This inventory is created from Pick by BOM.", bikeAvailableInventory);
			AssertEquals("This inventory has no location yet.", false, bikeAvailableInventory.LocationPK.IsValid);

			var invFactManager = new Mock<IAvailableInventoryFactManager>();
			var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
			invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
			var loader = new AllocationFactLoader(invFactManagerFactory.Object);
			var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(), GetMockedPickStrategy());
			var facts = factBatches.SelectMany(f => f).ToArray();
			AssertEquals("Should have returned no facts.", 0, facts.Length);
		}

		#region TestGetAllocationFacts_HeldInventoryOrder

		public void TestGetAllocationFacts_HeldInventoryOrder_OverrideHasPickFaces_IsHeldInventoryOrder() => TestGetAllocationFacts_HeldOrder_OverrideHasPickFaces_Core(isHeldInventoryOrder: true, expectedHasPickFaces: false);
		public void TestGetAllocationFacts_HeldInventoryOrder_OverrideHasPickFaces_NotIsHeldInventoryOrder() => TestGetAllocationFacts_HeldOrder_OverrideHasPickFaces_Core(isHeldInventoryOrder: false, expectedHasPickFaces: true);

		void TestGetAllocationFacts_HeldOrder_OverrideHasPickFaces_Core(bool isHeldInventoryOrder, bool expectedHasPickFaces)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isHeldInventoryOrder))
			{
				var today = ZDateTimeOffset.Today;
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				Factory.Save();

				Helper.CreateProductPickFace(data.Part1, data.Org1, data.Whs1, "A-1");
				Factory.Save();

				AssertNotNull("Precondition", data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(r => r.OU_OH == data.Org1.PK));

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "", isHeldInventoryOrder ? InventoryStatus.Codes.Held : InventoryStatus.Codes.Available, isHeldInventoryOrder ? InventoryHoldCodes.Codes.Damaged : "");
				receive.FinaliseDocketWithoutUserConfirmation();
				Assert(receive.IsFinalised);
				Factory.Save();

				var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
				var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 3m);
				var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part2, 3m);

				if (isHeldInventoryOrder)
				{
					order1Line1.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;
					order1Line2.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;
				}

				var pick = Helper.CreatePickNew(order1);
				Factory.Save();

				var invFactManager = new Mock<IAvailableInventoryFactManager>();
				var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
				invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
				var loader = new AllocationFactLoader(invFactManagerFactory.Object);
				var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
					GetMockedPickStrategy());
				var facts = factBatches.SelectMany(f => f).ToArray();
				var nestedFacts = facts.GetNestedFacts();

				var orderLineFacts = nestedFacts.OfType<OrderLineFact>().ToArray();
				Assert("Precondition", orderLineFacts.All(orderLineFact => orderLineFact.IsHeldInventoryOrder == isHeldInventoryOrder));

				var productFact = nestedFacts.OfType<AllocationProductFact>().Single();

				AssertEquals(nameof(AllocationProductFact.HasPickFaces), expectedHasPickFaces, productFact.HasPickFaces);
			}
		}

		public void TestGetAllocationFacts_HeldInventoryOrder_OverrideIsDynamic_IsHeldInventoryOrder() => TestGetAllocationFacts_HeldInventoryOrder_OverrideIsDynamic_Core(isHeldInventoryOrder: true, expectedIsDynamic: false);
		public void TestGetAllocationFacts_HeldInventoryOrder_OverrideIsDynamic_NotIsHeldInventoryOrder() => TestGetAllocationFacts_HeldInventoryOrder_OverrideIsDynamic_Core(isHeldInventoryOrder: false, expectedIsDynamic: true);

		void TestGetAllocationFacts_HeldInventoryOrder_OverrideIsDynamic_Core(bool isHeldInventoryOrder, bool expectedIsDynamic)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isHeldInventoryOrder))
			{
				var today = ZDateTimeOffset.Today;
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

				var productParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
				productParams.W3_OH = data.Org1.PK;
				productParams.W3_WW = data.Whs1.PK;
				productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

				AssertNotNull("Precondition", data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single(r => r.OU_OH == data.Org1.PK));

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "", isHeldInventoryOrder ? InventoryStatus.Codes.Held : InventoryStatus.Codes.Available, isHeldInventoryOrder ? InventoryHoldCodes.Codes.Damaged : "");
				receive.FinaliseDocketWithoutUserConfirmation();
				Assert(receive.IsFinalised);
				Factory.Save();

				var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
				var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 3m);

				if (isHeldInventoryOrder)
				{
					order1Line1.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;
				}

				var pick = Helper.CreatePickNew(order1);
				Factory.Save();

				var invFactManager = new Mock<IAvailableInventoryFactManager>();
				var invFactManagerFactory = new Mock<IAvailableInventoryFactManagerFactory>();
				invFactManagerFactory.Setup(f => f.GetNewManager()).Returns(invFactManager.Object);
				var loader = new AllocationFactLoader(invFactManagerFactory.Object);
				var factBatches = loader.GetAllocationFacts(pick.OrderedInventories.Cast<WhsPickOrderedInventory>(),
					GetMockedPickStrategy());
				var facts = factBatches.SelectMany(f => f).ToArray();
				var nestedFacts = facts.GetNestedFacts();

				var orderLineFacts = nestedFacts.OfType<OrderLineFact>().ToArray();
				Assert("Precondition", orderLineFacts.All(orderLineFact => orderLineFact.IsHeldInventoryOrder == isHeldInventoryOrder));

				var productFact = nestedFacts.OfType<AllocationProductFact>().Single();
				AssertEquals(nameof(AllocationProductFact.IsDynamic), expectedIsDynamic, productFact.IsDynamic);
			}
		}

		#endregion

		void AssertAvailableInventory(
			Mock<IAvailableInventoryFactManager> invFactManagerMock,
			AvailableInventoryFact availableInventoryFact,
			WhsPickAvailableInventory availableInventory,
			IEnumerable<IInputFact> facts)
		{
			CombineAssertions(() =>
			{
				AssertEquals(nameof(availableInventoryFact.PK), availableInventory.PK, availableInventoryFact.PK);
				AssertEquals(nameof(availableInventoryFact.OrderedInventoryPK), availableInventory.OrderedInventory.PK,
					availableInventoryFact.OrderedInventoryPK);

				AssertEquals(nameof(availableInventoryFact.Quantity), availableInventory.QuantityUnPicked,
					availableInventoryFact.Quantity);
				AssertEquals(nameof(availableInventoryFact.PartAttribute1), availableInventory.PartAttrib1,
					availableInventoryFact.PartAttribute1);
				AssertEquals(nameof(availableInventoryFact.PartAttribute2), availableInventory.PartAttrib2,
					availableInventoryFact.PartAttribute2);
				AssertEquals(nameof(availableInventoryFact.PartAttribute3), availableInventory.PartAttrib3,
					availableInventoryFact.PartAttribute3);
				AssertEquals(nameof(availableInventoryFact.SerialNumber), availableInventory.SerialNumber,
					availableInventoryFact.SerialNumber);
				AssertEquals(nameof(availableInventoryFact.ExpiryDate),
					availableInventory.ExpiryDate.ConvertToNullableDateTime(), availableInventoryFact.ExpiryDate);
				AssertEquals(nameof(availableInventoryFact.PackingDate),
					availableInventory.PackingDate.ConvertToNullableDateTime(), availableInventoryFact.PackingDate);

				AssertEquals(nameof(availableInventoryFact.BondedEntryKey), availableInventory.BondedEntryKey,
					availableInventoryFact.BondedEntryKey);
				AssertEquals(nameof(availableInventoryFact.BondedEntryDate),
					availableInventory.BondedEntryDate.ConvertToNullableDateTime(),
					availableInventoryFact.BondedEntryDate);

				AssertEquals(
					nameof(availableInventoryFact.VFDPerStockUnit) + ": Check greater than zero",
					availableInventory.VFDPerStockUnit > 0m,
					availableInventoryFact.VFDPerStockUnit > 0m);
				AssertEquals(
					nameof(availableInventoryFact.VFDPerStockUnit),
					availableInventory.VFDPerStockUnit,
					availableInventoryFact.VFDPerStockUnit);

				var locationFact = facts.OfType<AllocationLocationFact>()
					.SingleOrDefault(o => o.PK == availableInventory.LocationPK);
				AssertNotNull("Should have a client fact in memory.", locationFact);
				AssertEquals(nameof(availableInventoryFact.Location), locationFact,
					availableInventoryFact.Location.Fact);
			});

			if (invFactManagerMock != null)
			{
				var key = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(availableInventory.Inventory[0], availableInventory.OrderedInventory);
				invFactManagerMock.Verify(i => i.Register(key, availableInventoryFact), Times.Once);

				availableInventoryFact.DecreaseQuantity(1.23m);
				invFactManagerMock.Verify(i => i.Update(key, availableInventoryFact.PK, 1.23m), Times.Once);
			}
		}

		void AssertOrderLine(
			OrderLineFact orderLineFact,
			IEnumerable<IInputFact> facts,
			ZGuid orderedInventoryPK,
			WhsPickableDocket order,
			WhsPickableDocketLine orderLine)
		{
			CombineAssertions(() =>
			{
				AssertEquals(nameof(orderLineFact.PK), orderLine.PK, orderLineFact.PK);
				AssertEquals(nameof(orderLineFact.OrderedInventoryPK), orderedInventoryPK,
					orderLineFact.OrderedInventoryPK);
				AssertEquals(nameof(orderLineFact.Quantity), orderLine.QuantityNotMet, orderLineFact.Quantity);
				AssertEquals(nameof(orderLineFact.OrderType), order.WD_DocketSubType, orderLineFact.OrderType);
				AssertEquals(nameof(orderLineFact.OrderNumber), order.WD_ExternalReference, orderLineFact.OrderNumber);
				AssertEquals(nameof(orderLineFact.CustomerReference), order.WD_CustomerReference,
					orderLineFact.CustomerReference);
				AssertEquals(nameof(orderLineFact.RequiredDate), order.WD_RequiredDate.ToDateTime(),
					orderLineFact.RequiredDate);
				AssertEquals(nameof(orderLineFact.ServiceLevel), order.WD_RS_NKServiceLevel,
					orderLineFact.ServiceLevel);
				AssertEquals(nameof(orderLineFact.CarrierServiceLevel), order.WD_PL_NKCarrierServiceLevel,
					orderLineFact.CarrierServiceLevel);
				AssertEquals(nameof(orderLineFact.PickPriority), order.WD_PickPriority, orderLineFact.PickPriority);
				AssertEquals(nameof(orderLineFact.TransportZone), order.TransportZoneName, orderLineFact.TransportZone);

				var clientFact = facts.OfType<OrganisationFact>().SingleOrDefault(o => o.PK == orderLine.ClientPK);
				AssertNotNull("Should have a client fact in memory.", clientFact);
				AssertEquals(nameof(orderLineFact.Client), clientFact, orderLineFact.Client.Fact);

				var consigneeFact = facts.OfType<OrganisationFact>().SingleOrDefault(o => o.PK == order.ConsigneePK);
				AssertNotNull("Should have a consignee fact in memory.", consigneeFact);
				AssertEquals(nameof(orderLineFact.Consignee), consigneeFact, orderLineFact.Consignee.Fact);

				var productFact = facts.OfType<AllocationProductFact>()
					.SingleOrDefault(p => p.PK == orderLineFact.Product.Fact.PK);
				AssertNotNull("Should have the single product fact in memory.", productFact);

				var transportCompany = order.TransportCo;
				if (transportCompany != null)
				{
					var transportCompanyFact = facts.OfType<OrganisationFact>()
						.SingleOrDefault(o => o.PK == transportCompany.PK);
					AssertNotNull("Should have a transport company fact in memory.", transportCompanyFact);
					AssertEquals(nameof(orderLineFact.TransportCompany), transportCompanyFact,
						orderLineFact.TransportCompany.Fact);
				}
				else
				{
					AssertNull("Should *not* have a transport company.", orderLineFact.TransportCompany.Fact);
				}

				var distributionCentre = order.DistributionCentreDocAddress.Organisation;
				if (distributionCentre != null)
				{
					var distributionCentreFact = facts.OfType<OrganisationFact>()
						.SingleOrDefault(o => o.PK == distributionCentre.PK);
					AssertNotNull("Should have a distribution centre fact in memory.", distributionCentreFact);
					AssertEquals(nameof(orderLineFact.DistributionCentre), distributionCentreFact,
						orderLineFact.DistributionCentre.Fact);
				}
				else
				{
					AssertNull("Should *not* have a distribution centre.", orderLineFact.DistributionCentre.Fact);
				}
			});
		}

		static IPickStrategy GetMockedPickStrategy()
		{
			var pickStrategyMock = new Mock<IPickStrategy>();
			pickStrategyMock.Setup(ps =>
					ps.CanAllocateInQuantitiesDifferentToAutoAllocateQuantity(It.IsAny<WhsPickAvailableInventory>()))
				.Returns(true);
			pickStrategyMock.Setup(ps =>
					ps.GetQuantityUnPicked(It.IsAny<WhsPickAvailableInventory>()))
				.Returns<WhsPickAvailableInventory>(ai => ai.QuantityUnPicked);
			return pickStrategyMock.Object;
		}
	}
}
