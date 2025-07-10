using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierBulkDeactivator))]
	sealed class OrgSupplierBulkDeactivatorTest : NonPersistentBusinessObjectTestCase
	{
		[StressTest]
		public void TestDeactivate()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whsPK = helper.CreateWarehouse("1", "A").PK;
			var whsPK1 = helper.CreateWarehouse("2", "A").PK;
			Factory.Save();

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "1";

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "2";

			var part3 = Factory.New<OrgSupplierPart>();
			part3.OP_PartNum = "3";

			var part4 = Factory.New<OrgSupplierPart>();
			part4.OP_PartNum = "4";
			part4.OP_IsActive = false;

			var part5 = Factory.New<OrgSupplierPart>();
			part5.OP_PartNum = "5";
			part5.OP_IsActive = false;

			Factory.Save();

			var parts = new OrgSupplierPartCollection(Factory);
			parts.Load();
			AssertEquals(5, parts.Count);

			var query = new ZQuery();
			var changer = new OrgSupplierBulkDeactivator(Factory);
			changer.ProductFilter = query;
			AssertNotNull(changer.ProductFilter);
			changer.CalculateEstimatedProductCount();
			AssertEquals(3, changer.EstimatedProductCount);

			Assert(changer.ShouldDeactivate);
			AssertEquals(false, changer.ShouldActivate);

			changer.Deactivate();
			AssertEquals(3, changer.ProductsChanged);

			changer = new OrgSupplierBulkDeactivator(Factory);
			changer.ProductFilter = query;
			changer.Deactivate();
			AssertEquals(0, changer.ProductsChanged);

			part1.Reload();
			part1.OP_PartNum = "Test 1";
			part1.OP_IsActive = true;

			part4.Reload();
			part4.OP_PartNum = "Test 4";
			Factory.Save();

			changer = new OrgSupplierBulkDeactivator(Factory);
			query = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.StartsWith, "Test");

			changer.ProductFilter = query;
			AssertNotNull(changer.ProductFilter);
			changer.CalculateEstimatedProductCount();
			AssertEquals(1, changer.EstimatedProductCount);

			changer.Deactivate();
			AssertEquals(1, changer.ProductsChanged);

			part1.Reload();
			part1.OP_IsActive = true;
			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();
			var part6 = Factory.New<OrgSupplierPart>();
			part6.OP_PartNum = "Test with stock on hand";
			var relation = part6.RelatedOrganisations.AddNew();
			relation.OU_OH = org.PK;
			relation.OU_OP = part6.PK;
			// add some stock on hand
			helper.CreateStock(whsPK, org.PK, part6.PK, 10m);

			var org2 = Factory.New<OrgHeader>();
			org2.FillWithValidTestData();
			var part7 = Factory.New<OrgSupplierPart>();
			part7.OP_PartNum = "Test 2 with stock on hand";
			var relation1 = part7.RelatedOrganisations.AddNew();
			relation1.OU_OH = org2.PK;
			relation1.OU_OP = part7.PK;
			// add some stock on hand
			helper.CreateStock(whsPK1, org2.PK, part7.PK, 10m);

			Factory.Save();

			changer.Deactivate();
			AssertEquals(1, changer.ProductsChanged);
		}

		public void TestDeactivate_WithStockOnHand_InTransit()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ORG1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "P2");
			AssertEquals("Precondition: Should *not* have Stock On Hand.", false, part1.HasStockOnHandOrInTransit);
			var whsPK = helper.CreateWarehouse("1", "A").PK;
			Factory.Save();

			// Add some stock on hand
			var receiveLine = (IWhsDocketLine)helper.CreateStock(whsPK, client, part1.PK, 10m);
			Factory.Save();
			AssertEquals("Precondition: Should have Stock On Hand.", true, part1.HasStockOnHandOrInTransit);
			AssertEquals("Precondition: Should *not* have Stock On Hand.", false, part2.HasStockOnHandOrInTransit);

			var orderPK = helper.CreateWhsOrder(client, whsPK, "O1", null);
			var orderLinePK = helper.CreateWhsOrderLine(orderPK, part1.PK, 10);
			var pickPK = helper.CreateWhsPick(new[] { orderPK });

			AssertEquals("Precondition: Stock On Hand exists.", 10m, receiveLine.WE_StockOnHand);
			var pickLine = helper.GetPickLines(pickPK).Single();
			helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Today);
			AssertEquals("Precondition: Stock On Hand reduced.", 0m, receiveLine.WE_StockOnHand);
			Factory.Save();

			AssertEquals("Precondition: Should have Stock In-Transit.", true, part1.HasStockOnHandOrInTransit);
			AssertEquals("Precondition: Should *not* have Stock In-Transit.", false, part2.HasStockOnHandOrInTransit);

			var changer1 = new OrgSupplierBulkDeactivator(Factory);
			changer1.CalculateEstimatedProductCount();
			AssertEquals("Estimated count does not consider inventory, should show 2 records.", 2, changer1.EstimatedProductCount);

			changer1.Deactivate();
			AssertEquals("Should have deactivated 1 record.", 1, changer1.ProductsChanged);
			part1.Reload();
			part2.Reload();
			AssertEquals("Should *not* have deactived product with In-Transit Qty.", true, part1.OP_IsActive);
			AssertEquals("Should have deactived product with In-Transit Qty.", false, part2.OP_IsActive);

			helper.FinaliseDocketWithoutUserConfirmation(orderPK);
			helper.FinalisePick(pickPK);
			Factory.Save();
			AssertEquals("Precondition: Should *not* have Stock On Hand or In-Transit.", false, part1.HasStockOnHandOrInTransit);
			AssertEquals("Precondition: Should *not* have Stock On Hand or In-Transit.", false, part2.HasStockOnHandOrInTransit);

			var changer2 = new OrgSupplierBulkDeactivator(Factory);
			changer2.CalculateEstimatedProductCount();
			AssertEquals("Estimated count should show 1 record.", 1, changer2.EstimatedProductCount);

			changer2.Deactivate();
			part1.Reload();
			part2.Reload();
			AssertEquals("Should have deactivated part1.", false, part1.OP_IsActive);
			AssertEquals("Should *not* have reactivated part2.", false, part2.OP_IsActive);
		}

		public void TestDeactivate_HasAsnLineOnUnfinalisedReceive()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ORG1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "P2");
			var whs = helper.CreateWarehouse("1", "A");
			Factory.Save();

			var receive1 = helper.CreateWhsReceive(client, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receive1, part1.PK, 10m, "A");
			helper.CreateAsnLine(receive1, part1.PK, 10m);

			var receive2 = helper.CreateWhsReceive(client, whs.PK, "R2", null);
			helper.CreateWhsReceiveInventoryLine(receive2, part2.PK, 10m, "A");
			helper.CreateAsnLine(receive2, part2.PK, 10m);
			Factory.Save();

			var changer1 = new OrgSupplierBulkDeactivator(Factory);
			changer1.CalculateEstimatedProductCount();
			AssertEquals("Estimated count does not consider inventory, should show 2 records.", 2, changer1.EstimatedProductCount);

			changer1.Deactivate();
			AssertEquals("Should *not* have deactivated any record.", 0, changer1.ProductsChanged);
			part1.Reload();
			part2.Reload();
			AssertEquals("Should *not* have deactived product when unfinalised Receive is referencing the product.", true, part1.OP_IsActive);
			AssertEquals("Should *not* have deactived product when unfinalised Receive is referencing the product.", true, part2.OP_IsActive);

			helper.FinaliseDocketWithoutUserConfirmation(receive1);
			Factory.Save();

			var order = helper.CreateWhsOrder(client, whs.PK, "O1", null);
			helper.CreateWhsOrderLine(order, part1.PK, 10m);
			var pick = helper.CreateWhsPick(new[] { order });
			helper.FinaliseDocketWithoutUserConfirmation(order);
			helper.FinalisePick(pick);
			Factory.Save();

			var changer2 = new OrgSupplierBulkDeactivator(Factory);
			changer2.CalculateEstimatedProductCount();
			AssertEquals("Estimated count does not consider inventory, should show 2 records.", 2, changer2.EstimatedProductCount);

			changer2.Deactivate();
			AssertEquals("Should have deactivated 1 record.", 1, changer2.ProductsChanged);
			part1.Reload();
			part2.Reload();
			AssertEquals("Should *not* have deactived product when unfinalised Receive is referencing the product.", false, part1.OP_IsActive);
			AssertEquals("Should *not* have deactived product when unfinalised Receive is referencing the product.", true, part2.OP_IsActive);

			var part2Receive = (IWhsReceive)Factory.Load(ObjectFactory.GetType<IWhsReceive>(), receive2);
			part2Receive.WD_DocketStatus = "ERR";
			part2Receive.WD_DocketStatus = "CAN";
			Factory.Save();

			var changer3 = new OrgSupplierBulkDeactivator(Factory);
			changer3.CalculateEstimatedProductCount();
			AssertEquals("Estimated count does not consider inventory, should show 1 records.", 1, changer3.EstimatedProductCount);

			changer3.Deactivate();
			part1.Reload();
			part2.Reload();
			AssertEquals("Should *not* have deactived product when unfinalised Receive is referencing the product.", false, part1.OP_IsActive);
			AssertEquals("Should *not* have deactived product when unfinalised Receive is referencing the product.", false, part2.OP_IsActive);
		}

		public void TestCancel()
		{
			OrgSupplierBulkDeactivator changer = new OrgSupplierBulkDeactivator(Factory);
			changer.Cancel();
			Assert(changer.Cancelled);
		}

		public void TestIsActiveFilter()
		{
			for (int i = 0; i < 1500; i++)
			{
				OrgSupplierPart part = Factory.New<OrgSupplierPart>();
				part.OP_PartNum = i.ToString() + "A";

				if (i == 500 || i == 1001)
				{
					part.OP_IsActive = false;
				}
			}
			Factory.Save();

			OrgSupplierBulkDeactivator deactivator = new OrgSupplierBulkDeactivator(Factory);
			ZQuery query = new ZQuery(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.Contains, "A");
			query.AddToFilter(OrgSupplierPartSchema.OP_IsActive, true);
			query.AddToFilter(JoinCondition.Or, OrgSupplierPartSchema.OP_IsActive, false);

			deactivator.ProductFilter = query;
			AssertNotNull(deactivator.ProductFilter);
			deactivator.CalculateEstimatedProductCount();
			AssertEquals(1498, deactivator.EstimatedProductCount);

			deactivator.ShouldDeactivate = false;
			deactivator.ShouldActivate = true;
			AssertEquals(2, deactivator.EstimatedProductCount);

			deactivator.ShouldDeactivate = true;
			AssertEquals(1498, deactivator.EstimatedProductCount);
		}

		public void TestBulkDeactivatePerformance()
		{
			for (int count = 0; count < 10; count++)
			{
				var warehouse = Helper.CreateWarehouse("W1", "R" + count.ToString());
				var clientPK = Helper.CreateClient("C" + count.ToString());
				var product = (OrgSupplierPart)Helper.CreateProduct(clientPK, "P" + count.ToString());
				Helper.CreateStock(warehouse.PK, clientPK, "R" + count.ToString(), product.PK, 2m);
				Factory.Save();
			}

			var bulkDeactivator = new OrgSupplierBulkDeactivator(Factory);
			bulkDeactivator.ProductFilter = new ZQuery();
			bulkDeactivator.Deactivate();
			var factoryUsedInDeactivator = bulkDeactivator.BulkDeactivatorFactoryForTesting;
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(OrgSupplierPartSchema.Constants.TableName, 1); // Original was 10
			AssertDbHits(expectedDbHits, factoryUsedInDeactivator);
		}

		public void TestDeactivateDuplicate()
		{
			var ownerA = Factory.NewWithValidTestData<OrgHeader>();
			ownerA.OH_Code = "OWNERA";
			var ownerB = Factory.NewWithValidTestData<OrgHeader>();
			ownerB.OH_Code = "OWNERB";
			var ownerC = Factory.NewWithValidTestData<OrgHeader>();
			ownerC.OH_Code = "OWNERC";
			var supplierA = Factory.NewWithValidTestData<OrgHeader>();
			supplierA.OH_Code = "SUPPLIERA";
			var supplierB = Factory.NewWithValidTestData<OrgHeader>();
			supplierB.OH_Code = "SUPPLIERB";
			var supplierC = Factory.NewWithValidTestData<OrgHeader>();
			supplierC.OH_Code = "SUPPLIERC";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PART#@1";
			part1.OP_Desc = "PART DESC 1 DUPLICATE";
			part1.OP_IsActive = false;
			part1.RelatedOrganisations.AddOwner(ownerA);
			part1.RelatedOrganisations.AddOrganisationIfNotExist(ownerB.PK, OrgPartRelation.RelationshipTypes.Both);
			part1.RelatedOrganisations.AddSupplier(supplierA);

			var part1Active = Factory.New<OrgSupplierPart>();
			part1Active.OP_PartNum = "PART#@1";
			part1Active.OP_Desc = "1 Active";
			part1Active.OP_IsActive = true;
			part1Active.RelatedOrganisations.AddOwner(ownerA);
			part1Active.RelatedOrganisations.AddSupplier(ownerB);

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PART#@2";
			part2.OP_Desc = "PART DESC 2 DUPLICATE";
			part2.OP_IsActive = false;
			part2.RelatedOrganisations.AddOwner(ownerC);
			part2.RelatedOrganisations.AddSupplier(supplierA);

			var part2Active = Factory.New<OrgSupplierPart>();
			part2Active.OP_PartNum = "PART#@2";
			part2Active.OP_Desc = "2 Active";
			part2Active.OP_IsActive = true;
			part2Active.RelatedOrganisations.AddOrganisationIfNotExist(ownerC.PK, OrgPartRelation.RelationshipTypes.Both);
			part2Active.RelatedOrganisations.AddSupplier(supplierA);

			var part3 = Factory.New<OrgSupplierPart>();
			part3.OP_PartNum = "PART#@3";
			part3.OP_Desc = "PART DESC 3 DUPLICATE";
			part3.OP_IsActive = false;
			part3.RelatedOrganisations.AddOwner(ownerA);
			part3.RelatedOrganisations.AddSupplier(supplierB);

			var part3Active = Factory.New<OrgSupplierPart>();
			part3Active.OP_PartNum = "PART#@3";
			part3Active.OP_Desc = "3 Active";
			part3Active.OP_IsActive = true;
			part3Active.RelatedOrganisations.AddOwner(ownerA);
			part3Active.RelatedOrganisations.AddSupplier(supplierB);

			var part4A = Factory.New<OrgSupplierPart>();
			part4A.OP_PartNum = "PART#@4";
			part4A.OP_Desc = "PART DESC 4A DUPLICATE";
			part4A.OP_IsActive = false;
			part4A.RelatedOrganisations.AddOwner(ownerA);
			part4A.RelatedOrganisations.AddSupplier(ownerB);

			var part4B = Factory.New<OrgSupplierPart>();
			part4B.OP_PartNum = "PART#@4";
			part4B.OP_Desc = "PART DESC 4B DUPLICATE";
			part4B.OP_IsActive = false;
			part4B.RelatedOrganisations.AddOwner(ownerB);
			part4B.RelatedOrganisations.AddSupplier(supplierA);

			var part4C = Factory.New<OrgSupplierPart>();
			part4C.OP_PartNum = "PART#@4";
			part4C.OP_Desc = "PART DESC 4C DUPLICATE";
			part4C.OP_IsActive = false;
			part4C.RelatedOrganisations.AddOwner(ownerA);
			part4C.RelatedOrganisations.AddSupplier(supplierA);

			var part4D = Factory.New<OrgSupplierPart>();
			part4D.OP_PartNum = "PART#@4";
			part4D.OP_Desc = "PART DESC 4D DUPLICATE";
			part4D.OP_IsActive = false;
			part4D.RelatedOrganisations.AddOwner(ownerB);
			part4D.RelatedOrganisations.AddSupplier(ownerB);

			var part4Active = Factory.New<OrgSupplierPart>();
			part4Active.OP_PartNum = "PART#@4";
			part4Active.OP_Desc = "4 Active";
			part4Active.OP_IsActive = true;
			part4Active.RelatedOrganisations.AddOwner(ownerA);
			part4Active.RelatedOrganisations.AddOrganisationIfNotExist(ownerB.PK, OrgPartRelation.RelationshipTypes.Both);
			part4Active.RelatedOrganisations.AddSupplier(supplierA);

			var part5A = Factory.New<OrgSupplierPart>();
			part5A.OP_PartNum = "PART#@5";
			part5A.OP_Desc = "PART DESC 5A DUPLICATE";
			part5A.OP_IsActive = false;
			part5A.RelatedOrganisations.AddOwner(ownerC);

			var part5B = Factory.New<OrgSupplierPart>();
			part5B.OP_PartNum = "PART#@5";
			part5B.OP_Desc = "PART DESC 5B DUPLICATE";
			part5B.OP_IsActive = false;
			part5B.RelatedOrganisations.AddSupplier(ownerC);

			var part5C = Factory.New<OrgSupplierPart>();
			part5C.OP_PartNum = "PART#@5";
			part5C.OP_Desc = "PART DESC 5C DUPLICATE";
			part5C.OP_IsActive = false;
			part5C.RelatedOrganisations.AddOwner(ownerC);
			part5C.RelatedOrganisations.AddSupplier(ownerC);

			var part5Active = Factory.New<OrgSupplierPart>();
			part5Active.OP_PartNum = "PART#@5";
			part5Active.OP_Desc = "5 Active";
			part5Active.OP_IsActive = true;
			part5Active.RelatedOrganisations.AddOrganisationIfNotExist(ownerC.PK, OrgPartRelation.RelationshipTypes.Both);

			var part6 = Factory.New<OrgSupplierPart>();
			part6.OP_PartNum = "PART#@6";
			part6.OP_Desc = "PART DESC 6";
			part6.OP_IsActive = false;
			part6.RelatedOrganisations.AddOwner(ownerB);

			var part6Active = Factory.New<OrgSupplierPart>();
			part6Active.OP_PartNum = "PART#@6";
			part6Active.OP_Desc = "6 Active";
			part6Active.OP_IsActive = false;
			part6Active.RelatedOrganisations.AddOwner(ownerB);
			part6Active.RelatedOrganisations.AddSupplier(supplierC);

			var part7 = Factory.New<OrgSupplierPart>();
			part7.OP_PartNum = "PART#@7";
			part7.OP_Desc = "PART DESC 7";
			part7.OP_IsActive = false;
			part7.RelatedOrganisations.AddSupplier(supplierC);

			var part7Active = Factory.New<OrgSupplierPart>();
			part7Active.OP_PartNum = "PART#@7";
			part7Active.OP_Desc = "7 Active";
			part7Active.OP_IsActive = true;
			part7Active.RelatedOrganisations.AddOwner(ownerB);
			part7Active.RelatedOrganisations.AddSupplier(supplierC);

			var part8 = Factory.New<OrgSupplierPart>();
			part8.OP_PartNum = "PART#@8";
			part8.OP_Desc = "PART DESC 8 DUPLICATE";
			part8.OP_IsActive = false;
			part8.RelatedOrganisations.AddOrganisationIfNotExist(ownerC.PK, OrgPartRelation.RelationshipTypes.Both);

			var part8Active = Factory.New<OrgSupplierPart>();
			part8Active.OP_PartNum = "PART#@8";
			part8Active.OP_Desc = "8 Active";
			part8Active.OP_IsActive = true;
			part8Active.RelatedOrganisations.AddOwner(ownerC);

			var part9 = Factory.New<OrgSupplierPart>();
			part9.OP_PartNum = "PART#@9";
			part9.OP_Desc = "PART DESC 9 DUPLICATE";
			part9.OP_IsActive = false;
			part9.RelatedOrganisations.AddOrganisationIfNotExist(ownerC.PK, OrgPartRelation.RelationshipTypes.Both);

			var part9Active = Factory.New<OrgSupplierPart>();
			part9Active.OP_PartNum = "PART#@9";
			part9Active.OP_Desc = "9 Active";
			part9Active.OP_IsActive = true;
			part9Active.RelatedOrganisations.AddSupplier(ownerC);

			var part10 = Factory.New<OrgSupplierPart>();
			part10.OP_PartNum = "PART#@10";
			part10.OP_Desc = "PART DESC 10 DUPLICATE";
			part10.OP_IsActive = false;
			part10.RelatedOrganisations.AddOrganisationIfNotExist(ownerC.PK, OrgPartRelation.RelationshipTypes.Both);

			var part10Active = Factory.New<OrgSupplierPart>();
			part10Active.OP_PartNum = "PART#@10";
			part10Active.OP_Desc = "10 Active";
			part10Active.OP_IsActive = true;
			part10Active.RelatedOrganisations.AddOwner(ownerC);
			part10Active.RelatedOrganisations.AddSupplier(ownerC);

			var part11 = Factory.New<OrgSupplierPart>();
			part11.OP_PartNum = "PART#@11";
			part11.OP_Desc = "PART DESC 11 DUPLICATE";
			part11.OP_IsActive = false;
			part11.RelatedOrganisations.AddSupplier(ownerC);

			var part11Active = Factory.New<OrgSupplierPart>();
			part11Active.OP_PartNum = "PART#@11";
			part11Active.OP_Desc = "11 Active";
			part11Active.OP_IsActive = true;
			part11Active.RelatedOrganisations.AddOwner(ownerC);
			part11Active.RelatedOrganisations.AddSupplier(ownerC);

			var part12 = Factory.New<OrgSupplierPart>();
			part12.OP_PartNum = "PART#@12";
			part12.OP_Desc = "PART DESC 12 DUPLICATE";
			part12.OP_IsActive = false;
			part12.RelatedOrganisations.AddOwner(ownerC);

			var part12Active = Factory.New<OrgSupplierPart>();
			part12Active.OP_PartNum = "PART#@12";
			part12Active.OP_Desc = "12 Active";
			part12Active.OP_IsActive = true;
			part12Active.RelatedOrganisations.AddOwner(ownerC);
			part12Active.RelatedOrganisations.AddSupplier(ownerC);

			var part13B = Factory.New<OrgSupplierPart>();
			part13B.OP_PartNum = "PART#@13";
			part13B.OP_Desc = "PART DESC 13B DUPLICATE";
			part13B.OP_IsActive = false;
			part13B.RelatedOrganisations.AddOwner(ownerC);

			var part13A = Factory.New<OrgSupplierPart>();
			part13A.OP_PartNum = "PART#@13";
			part13A.OP_Desc = "PART DESC 13A";
			part13A.OP_IsActive = false;
			part13A.RelatedOrganisations.AddOwner(ownerC);
			part13A.RelatedOrganisations.AddSupplier(ownerC);

			var part14B = Factory.New<OrgSupplierPart>();
			part14B.OP_PartNum = "PART#@14";
			part14B.OP_Desc = "PART DESC 14B DUPLICATE";
			part14B.OP_IsActive = false;
			part14B.RelatedOrganisations.AddOrganisationIfNotExist(ownerC.PK, OrgPartRelation.RelationshipTypes.Both);

			var part14A = Factory.New<OrgSupplierPart>();
			part14A.OP_PartNum = "PART#@14";
			part14A.OP_Desc = "PART DESC 14A";
			part14A.OP_IsActive = false;
			part14A.RelatedOrganisations.AddOwner(ownerC);

			var part15B = Factory.New<OrgSupplierPart>();
			part15B.OP_PartNum = "PART#@15";
			part15B.OP_Desc = "PART DESC 15B DUPLICATE";
			part15B.OP_IsActive = false;
			part15B.RelatedOrganisations.AddSupplier(ownerC);

			var part15A = Factory.New<OrgSupplierPart>();
			part15A.OP_PartNum = "PART#@15";
			part15A.OP_Desc = "PART DESC 15A";
			part15A.OP_IsActive = false;
			part15A.RelatedOrganisations.AddOrganisationIfNotExist(ownerC.PK, OrgPartRelation.RelationshipTypes.Both);

			var part16 = Factory.New<OrgSupplierPart>();
			part16.OP_PartNum = "PART#@16";
			part16.OP_Desc = "16";
			part16.OP_IsActive = false;
			part16.RelatedOrganisations.AddOwner(ownerC);
			part16.RelatedOrganisations.AddSupplier(supplierA);

			var part17A = Factory.New<OrgSupplierPart>();
			part17A.OP_PartNum = "PART#@17";
			part17A.OP_Desc = "PART DESC 17 (Active, Matches filter)";
			part17A.OP_IsActive = true;
			part17A.RelatedOrganisations.AddOwner(ownerA);
			part17A.RelatedOrganisations.AddSupplier(supplierA);

			var part17B = Factory.New<OrgSupplierPart>();
			part17B.OP_PartNum = "PART#@17";
			part17B.OP_Desc = "PART DESC 17 (Duplicate)";
			part17B.OP_IsActive = false;
			part17B.RelatedOrganisations.AddOwner(ownerA);
			part17B.RelatedOrganisations.AddSupplier(supplierB);

			var part18A = Factory.New<OrgSupplierPart>();
			part18A.OP_PartNum = "PART#@18";
			part18A.OP_Desc = "18 (Active, Does not match filter)";
			part18A.OP_IsActive = true;
			part18A.RelatedOrganisations.AddOwner(ownerA);
			part18A.RelatedOrganisations.AddSupplier(supplierA);

			var part18B = Factory.New<OrgSupplierPart>();
			part18B.OP_PartNum = "PART#@18";
			part18B.OP_Desc = "PART DESC 18 (Duplicate)";
			part18B.OP_IsActive = false;
			part18B.RelatedOrganisations.AddOwner(ownerA);
			part18B.RelatedOrganisations.AddSupplier(supplierB);

			var part19A = Factory.New<OrgSupplierPart>();
			part19A.OP_PartNum = "PART#@19";
			part19A.OP_Desc = "PART DESC 19 (Active, Matches filter)";
			part19A.OP_IsActive = true;
			part19A.RelatedOrganisations.AddSupplier(supplierA);

			var part19B = Factory.New<OrgSupplierPart>();
			part19B.OP_PartNum = "PART#@19";
			part19B.OP_Desc = "PART DESC 19 (Duplicate)";
			part19B.OP_IsActive = false;
			part19B.RelatedOrganisations.AddSupplier(supplierA);

			var part20A = Factory.New<OrgSupplierPart>();
			part20A.OP_PartNum = "PART#@20";
			part20A.OP_Desc = "PART DESC 20 (Active, Matches filter)";
			part20A.OP_IsActive = true;
			part20A.RelatedOrganisations.AddOwner(supplierA);

			var part20B = Factory.New<OrgSupplierPart>();
			part20B.OP_PartNum = "PART#@20";
			part20B.OP_Desc = "PART DESC 20 (Activated, Matches filter)";
			part20B.OP_IsActive = false;
			part20B.RelatedOrganisations.AddSupplier(supplierA);

			var part21A = Factory.New<OrgSupplierPart>();
			part21A.OP_PartNum = "PART#@21";
			part21A.OP_Desc = "PART DESC 21 (Active, Matches filter)";
			part21A.OP_IsActive = true;
			part21A.RelatedOrganisations.AddSupplier(supplierA);

			var part21B = Factory.New<OrgSupplierPart>();
			part21B.OP_PartNum = "PART#@21";
			part21B.OP_Desc = "PART DESC 21 (Activated, Matches filter)";
			part21B.OP_IsActive = false;
			part21B.RelatedOrganisations.AddOwner(supplierA);

			var part22A = Factory.New<OrgSupplierPart>();
			part22A.OP_PartNum = "PART#@22";
			part22A.OP_Desc = "PART DESC 22 (Active, Matches filter)";
			part22A.OP_IsActive = true;
			part22A.RelatedOrganisations.AddOrganisationIfNotExist(supplierA.PK, OrgPartRelation.RelationshipTypes.Both);

			var part22B = Factory.New<OrgSupplierPart>();
			part22B.OP_PartNum = "PART#@22";
			part22B.OP_Desc = "PART DESC 22";
			part22B.OP_IsActive = false;
			part22B.RelatedOrganisations.AddOwner(supplierA);

			var part23A = Factory.New<OrgSupplierPart>();
			part23A.OP_PartNum = "PART#@23";
			part23A.OP_Desc = "PART DESC 23 (Active, Matches filter)";
			part23A.OP_IsActive = true;
			part23A.RelatedOrganisations.AddOwner(supplierA);

			var part23B = Factory.New<OrgSupplierPart>();
			part23B.OP_PartNum = "PART#@23";
			part23B.OP_Desc = "PART DESC 23";
			part23B.OP_IsActive = false;
			part23B.RelatedOrganisations.AddOrganisationIfNotExist(supplierA.PK, OrgPartRelation.RelationshipTypes.Both);

			Factory.Save();
			var bulkDeactivator = new OrgSupplierBulkDeactivator(new BusinessObjectFactory());
			var query = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			query.AddToFilter(OrgSupplierPartSchema.OP_Desc, SQLComparisonOperator.StartsWith, "PART DESC");
			query.OrderBy = OrgSupplierPartSchema.Constants.OP_PartNum + ", " + OrgSupplierPartSchema.Constants.OP_Desc;
			bulkDeactivator.ProductFilter = query.DeepClone();
			bulkDeactivator.ShouldActivate = true;
			bulkDeactivator.ShouldDeactivate = false;
			bulkDeactivator.Deactivate();
			var factory = new BusinessObjectFactory();
			query.AddToFilter(OrgSupplierPartSchema.OP_IsActive, true);
			CombineAssertions(() =>
			{
				AssertMultilineASCIIEquals("Active products", @"PART#@13 - PART DESC 13A
PART#@14 - PART DESC 14A
PART#@15 - PART DESC 15A
PART#@17 - PART DESC 17 (Active, Matches filter)
PART#@19 - PART DESC 19 (Active, Matches filter)
PART#@20 - PART DESC 20 (Activated, Matches filter)
PART#@20 - PART DESC 20 (Active, Matches filter)
PART#@21 - PART DESC 21 (Activated, Matches filter)
PART#@21 - PART DESC 21 (Active, Matches filter)
PART#@22 - PART DESC 22 (Active, Matches filter)
PART#@23 - PART DESC 23 (Active, Matches filter)
PART#@6 - PART DESC 6
PART#@7 - PART DESC 7", new ZStringBuilder(factory.Load<OrgSupplierPart>(query).Select(x => $"{x.OP_PartNum} - {x.OP_Desc}")).ToStringWithNewLineBetweenAppends());
				AssertEquals("ProductsChanged", 7, bulkDeactivator.ProductsChanged);

				var factoryUsedInDeactivator = bulkDeactivator.BulkDeactivatorFactoryForTesting;
				var expectedDbHits = new Dictionary<string, int>();
				expectedDbHits.Add(OrgPartRelationSchema.Constants.TableName, 1);
				expectedDbHits.Add(OrgSupplierPartSchema.Constants.TableName, 4);   // +2 additional hits from product barcode validation
				expectedDbHits.Add(OrgSupplierPartBarcodeSchema.Constants.TableName, 2);
				expectedDbHits.Add(ProcessTasksSchema.Constants.TableName, 1);
				expectedDbHits.Add(ProcessTaskTemplateSchema.Constants.TableName, 1);
				expectedDbHits.Add(StmALogSchema.Constants.TableName, 1);
				AssertDbHits(expectedDbHits, factoryUsedInDeactivator);
			});
		}

		public void TestActivate_DuplicateBarcode()
		{
			var client = Helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)Helper.CreateProduct(client, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "TCE";
			var part2 = (OrgSupplierPart)Helper.CreateProduct(client, "PRODUCT2");
			part2.OP_IsActive = false;
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode";
			barcode2.PH_F3_NKPackType = "TCE";
			Factory.Save();

			var bulkDeactivator = new OrgSupplierBulkDeactivator(new BusinessObjectFactory());
			var query = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			query.AddToFilter(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.StartsWith, "PRODUCT");
			query.AddToFilter(OrgSupplierPartSchema.OP_IsActive, SQLComparisonOperator.Equal, false);
			bulkDeactivator.ProductFilter = query.DeepClone();
			bulkDeactivator.ShouldActivate = true;
			bulkDeactivator.ShouldDeactivate = false;
			AssertNoExceptionThrown("No exceptions are thrown", bulkDeactivator.Deactivate);

			var newFactory = new BusinessObjectFactory();
			var part2InNewFactory = newFactory.Load<OrgSupplierPart>(part2.PK);
			AssertEquals("Product is not active.", false, part2InNewFactory.OP_IsActive);
		}

		public void TestActivate_BarcodeIsOtherProductCode()
		{
			var client = Helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)Helper.CreateProduct(client, "PRODUCT1");
			var part2 = (OrgSupplierPart)Helper.CreateProduct(client, "PRODUCT2");
			part2.OP_IsActive = false;
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "PRODUCT1";
			barcode2.PH_F3_NKPackType = "TCE";
			Factory.Save();

			var bulkDeactivator = new OrgSupplierBulkDeactivator(new BusinessObjectFactory());
			var query = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			query.AddToFilter(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.StartsWith, "PRODUCT");
			query.AddToFilter(OrgSupplierPartSchema.OP_IsActive, SQLComparisonOperator.Equal, false);
			bulkDeactivator.ProductFilter = query.DeepClone();
			bulkDeactivator.ShouldActivate = true;
			bulkDeactivator.ShouldDeactivate = false;
			AssertNoExceptionThrown("No exceptions are thrown", bulkDeactivator.Deactivate);

			var newFactory = new BusinessObjectFactory();
			var part2InNewFactory = newFactory.Load<OrgSupplierPart>(part2.PK);
			AssertEquals("Product is not active.", false, part2InNewFactory.OP_IsActive);
		}

		public void TestActivate_ProductCodeIsBarcodeOnOtherProduct()
		{
			var client = Helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)Helper.CreateProduct(client, "PRODUCT1");
			var barcode = part1.PartBarcodes.AddNew();
			barcode.PH_Barcode = "PRODUCT2";
			barcode.PH_F3_NKPackType = "TCE";

			var part2 = (OrgSupplierPart)Helper.CreateProduct(client, "PRODUCT2");
			part2.OP_IsActive = false;
			Factory.Save();

			var bulkDeactivator = new OrgSupplierBulkDeactivator(new BusinessObjectFactory());
			var query = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			query.AddToFilter(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.StartsWith, "PRODUCT");
			query.AddToFilter(OrgSupplierPartSchema.OP_IsActive, SQLComparisonOperator.Equal, false);
			bulkDeactivator.ProductFilter = query.DeepClone();
			bulkDeactivator.ShouldActivate = true;
			bulkDeactivator.ShouldDeactivate = false;
			AssertNoExceptionThrown("No exceptions are thrown", bulkDeactivator.Deactivate);

			var newFactory = new BusinessObjectFactory();
			var part2InNewFactory = newFactory.Load<OrgSupplierPart>(part2.PK);
			AssertEquals("Product is not active.", false, part2InNewFactory.OP_IsActive);
		}

		public void TestActivate_DuplicateBarcode_DBHits()
		{
			var numberOfProducts = 10;
			var client = Helper.CreateClient("ABC");

			for (var counter = 1; counter <= numberOfProducts; counter++)
			{
				var part = (OrgSupplierPart)Helper.CreateProduct(client, $"PRODUCT{counter}");
				var barcode = part.PartBarcodes.AddNew();
				barcode.PH_Barcode = "TestCode";
				barcode.PH_F3_NKPackType = "TCE";
				part.OP_IsActive = false;
			}
			Factory.Save();

			var productQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			productQuery.AddToFilter(OrgSupplierPartSchema.OP_IsActive, true);

			var activeProducts = Factory.Load<OrgSupplierPart>(productQuery);
			AssertEquals("Precondition: There are no active products.", 0, activeProducts.Length);

			var bulkDeactivator = new OrgSupplierBulkDeactivator(new BusinessObjectFactory());
			var query = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			query.AddToFilter(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.StartsWith, "PRODUCT");
			query.AddToFilter(OrgSupplierPartSchema.OP_IsActive, SQLComparisonOperator.Equal, false);
			bulkDeactivator.ProductFilter = query.DeepClone();
			bulkDeactivator.ShouldActivate = true;
			bulkDeactivator.ShouldDeactivate = false;

			AssertNoExceptionThrown("No exceptions are thrown", bulkDeactivator.Deactivate);
			var factoryUsedInDeactivator = bulkDeactivator.BulkDeactivatorFactoryForTesting;
			var expectedDBHitsForDeactivation = new Dictionary<string, int>()
			{
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 4 },
				{ OrgSupplierPartBarcodeSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 1 }
			};

			AssertDbHits(expectedDBHitsForDeactivation, factoryUsedInDeactivator);
			AssertEquals("Only 1 product is activated.", 1, bulkDeactivator.ProductsChanged);

			var newFactory = new BusinessObjectFactory();
			var activeProductsInNewFactory = newFactory.Load<OrgSupplierPart>(productQuery);
			AssertEquals("Only 1 product is activated.", 1, activeProductsInNewFactory.Length);
		}

		#region Helper

		IWhsTransactionTestHelper Helper
		{
			get { return helper ?? (helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory)); }
		}
		IWhsTransactionTestHelper helper;

		#endregion
	}
}
