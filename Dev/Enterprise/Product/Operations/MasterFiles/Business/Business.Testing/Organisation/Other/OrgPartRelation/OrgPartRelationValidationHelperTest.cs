using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrgPartRelationValidationHelper;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgPartRelationValidationHelperTest : TestCaseWithFactory
	{
		#region TestHasSameSupplierAndOwner

		public void TestHasSameSupplierAndOwner()
		{
			ZGuid orgPK1 = ZGuid.NewZGuid();
			ZGuid orgPK2 = ZGuid.NewZGuid();

			var shouldBeTrueCases = new ZString[]
			{
				"OWN - SUP",
				"SUP - OWN"
			};

			foreach (var rel1 in new ZString[] { "OWN", "SUP", "BTH", "WCN", "" })
			{
				foreach (var rel2 in new ZString[] { "OWN", "SUP", "BTH", "WCN", "" })
				{
					ZString testCase = rel1 + " - " + rel2;
					bool expectedValue = shouldBeTrueCases.Contains(testCase);
					AssertEquals(testCase + ", same organization", expectedValue, HasSameSupplierAndOwner(new List<RelatedParty> { new RelatedParty(rel1, orgPK1) }, orgPK1, rel2));
					AssertEquals(testCase + ", same organization", expectedValue, HasSameSupplierAndOwner(new List<RelatedPartyWithCode> { new RelatedPartyWithCode(rel1, "ORG1") }, "ORG1", rel2));

					AssertEquals(testCase + ", different organization", false, HasSameSupplierAndOwner(new List<RelatedParty> { new RelatedParty(rel1, orgPK1) }, orgPK2, rel2));
					AssertEquals(testCase + ", different organization", false, HasSameSupplierAndOwner(new List<RelatedPartyWithCode> { new RelatedPartyWithCode(rel1, "ORG1") }, "ORG2", rel2));
				}
			}
		}

		#endregion

		#region TestHasDuplicateRelationship

		public void TestHasDuplicateRelationship()
		{
			ZGuid orgPK1 = ZGuid.NewZGuid();
			ZGuid orgPK2 = ZGuid.NewZGuid();

			var shouldBeTrueCases = new ZString[]
			{
				"OWN - OWN",
				"OWN - BTH",
				"BTH - OWN",
				"BTH - SUP",
				"BTH - BTH",
				"SUP - SUP",
				"SUP - BTH",
				"WCN - WCN",
				" - "
			};

			foreach (var rel1 in new ZString[] { "OWN", "SUP", "BTH", "WCN", "" })
			{
				foreach (var rel2 in new ZString[] { "OWN", "SUP", "BTH", "WCN", "" })
				{
					ZString testCase = rel1 + " - " + rel2;
					bool expectedValue = shouldBeTrueCases.Contains(testCase);
					AssertEquals(testCase + ", same organization", expectedValue, HasDuplicateRelationship(new List<RelatedParty> { new RelatedParty(rel1, orgPK1) }, orgPK1, rel2));
					AssertEquals(testCase + ", same organization", expectedValue, HasDuplicateRelationship(new List<RelatedPartyWithCode> { new RelatedPartyWithCode(rel1, "ORG1") }, "ORG1", rel2));

					AssertEquals(testCase + ", different organization", false, HasDuplicateRelationship(new List<RelatedParty> { new RelatedParty(rel1, orgPK1) }, orgPK2, rel2));
					AssertEquals(testCase + ", different organization", false, HasDuplicateRelationship(new List<RelatedPartyWithCode> { new RelatedPartyWithCode(rel1, "ORG1") }, "ORG2", rel2));
				}
			}
		}

		#endregion

		#region TestGetProductNumbersWithDuplicateBarcode_SameBarcodeOfProductWithDifferentOwner

		public void TestGetProductNumbersWithDuplicateBarcode_SameBarcodeOfProductWithDifferentOwner()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var client2 = helper.CreateClient("ABC2");

			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "BAR1";
			barcode1.PH_F3_NKPackType = "TCE";
			Factory.Save();

			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "PRODUCT2");
			Factory.Save();

			AssertEquals("For Different owner should not get duplicate barcode.", false, OrgPartRelationValidationHelper.GetProductNumbersWithDuplicateBarcode(Factory, part2.PK, new[] { client2 }, new ZString[] { "BAR1" }).Any());
			AssertEquals("For Different owner should not get duplicate barcode.", "PRODUCT1", OrgPartRelationValidationHelper.GetProductNumbersWithDuplicateBarcode(Factory, part2.PK, new[] { client1 }, new ZString[] { "BAR1" }).First());
		}

		#endregion

		#region TestGetProductNumbersWithDuplicateBarcode_InactiveProductAreNotCheckForDuplicateBarcode

		public void TestGetProductNumbersWithDuplicateBarcode_InactiveProductAreNotCheckForDuplicateBarcode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var client2 = helper.CreateClient("ABC2");

			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			part1.OP_IsActive = false;
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "BAR1";
			barcode1.PH_F3_NKPackType = "TCE";
			Factory.Save();

			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "PRODUCT2");
			Factory.Save();

			AssertEquals("For different owner should not get duplicate barcode.", false, OrgPartRelationValidationHelper.GetProductNumbersWithDuplicateBarcode(Factory, part2.PK, new[] { client2 }, new ZString[] { "BAR1" }).Any());
			AssertEquals("For same owner if product is deactive, should not get duplicate barcode.", false, OrgPartRelationValidationHelper.GetProductNumbersWithDuplicateBarcode(Factory, part2.PK, new[] { client1 }, new ZString[] { "BAR1" }).Any());

			part1.OP_IsActive = true;
			Factory.Save();

			AssertEquals("For Different owner should not get duplicate barcode.", false, OrgPartRelationValidationHelper.GetProductNumbersWithDuplicateBarcode(Factory, part2.PK, new[] { client2 }, new ZString[] { "BAR1" }).Any());
			AssertEquals("For Different owner should not get duplicate barcode.", "PRODUCT1", OrgPartRelationValidationHelper.GetProductNumbersWithDuplicateBarcode(Factory, part2.PK, new[] { client1 }, new ZString[] { "BAR1" }).First());
		}

		#endregion

		#region TestGetProductNumbersWithDuplicateBarcode_Relationship

		public void TestGetProductNumbersWithDuplicateBarcode_Relationship_Both()
		{
			TestGetProductNumbersWithDuplicateBarcode_RelationshipCore(relationshipType: OrgPartRelation.RelationshipTypes.Both, expectedResultQuery: "PRODUCT1", whyExpectedResult: "Client ABC1 is owner of Product 1, the query should return product1 which is used barcode BAR1.");
		}

		public void TestGetProductNumbersWithDuplicateBarcode_Relationship_Owner()
		{
			TestGetProductNumbersWithDuplicateBarcode_RelationshipCore(relationshipType: OrgPartRelation.RelationshipTypes.Owner, expectedResultQuery: "PRODUCT1", whyExpectedResult: "Client ABC1 is owner of Product 1, the query should return product1 which is used barcode BAR1.");
		}

		public void TestGetProductNumbersWithDuplicateBarcode_Relationship_Supplier()
		{
			TestGetProductNumbersWithDuplicateBarcode_RelationshipCore(relationshipType: OrgPartRelation.RelationshipTypes.Supplier, expectedResultQuery: "", whyExpectedResult: "Client ABC1 is not the owner of Product 1, the query should not return any product which is used barcode BAR1");
		}

		void TestGetProductNumbersWithDuplicateBarcode_RelationshipCore(string relationshipType, string expectedResultQuery, string whyExpectedResult)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var client2 = helper.CreateClient("ABC2");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT2");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			part1.RelatedOrganisations.Cast<OrgPartRelation>().Single().OU_Relationship = relationshipType;
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "BAR1";
			barcode1.PH_F3_NKPackType = "TCE";
			Factory.Save();

			AssertEquals(whyExpectedResult, expectedResultQuery, OrgPartRelationValidationHelper.GetProductNumbersWithDuplicateBarcode(Factory, part2.PK, new[] { client1 }, new ZString[] { "BAR1" }).FirstOrDefault());
			AssertEquals("Same product should not return as duplicate barcode.", false, OrgPartRelationValidationHelper.GetProductNumbersWithDuplicateBarcode(Factory, part1.PK, new[] { client1 }, new ZString[] { "BAR1" }).Any());
		}

		#endregion

		#region TestConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife

		public void TestConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife_RelationIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife(null, 10));
		}

		public void TestConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse1 = helper.CreateWarehouse("MEL", "A");
			var warehouse2 = helper.CreateWarehouse("SYD", "A");
			var warehouse3 = helper.CreateWarehouse("ADL", "A");
			var client1PK = helper.CreateClient("Owner1");
			var client2 = Factory.Load<OrgHeader>(helper.CreateClient("Owner2"));
			var client3 = Factory.Load<OrgHeader>(helper.CreateClient("Owner3"));
			var part = (OrgSupplierPart)helper.CreateProduct(client1PK, "PartA");
			helper.CreateProductParamsByWhsAndClient(part.PK, client1PK, warehouse1.PK, 60);
			helper.CreateProductParamsByWhsAndClient(part.PK, client2.PK, warehouse2.PK, 0);
			helper.CreateProductParamsByWhsAndClient(part.PK, client3.PK, warehouse3.PK, 45);

			var relation1 = part.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			var relation2 = part.RelatedOrganisations.AddOwner(client2);
			var relation3 = part.RelatedOrganisations.AddOwner(client3);
			Factory.Save();

			AssertEquals("Should not return error message when less than Maximum Shelf Life",
				ZString.Empty, OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife(relation1, 40));
			AssertEquals("Should not return error message when this Product + Clients has no ProductParams",
				ZString.Empty, OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife(relation2, 70));
			AssertEquals("Should return error message", "Minimum shelf life 70 cannot be greater than Maximum Shelf Life 45.",
				OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife(relation3, 70));
		}

		#region TestConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife_RelationType

		public void TestConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife_RelationType_Owner()
		{
			TestConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife_RelationTypeCore(OrgPartRelation.RelationshipTypes.Owner, true);
		}

		public void TestConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife_RelationType_Supplier()
		{
			TestConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife_RelationTypeCore(OrgPartRelation.RelationshipTypes.Supplier, false);
		}

		public void TestConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife_RelationType_Both()
		{
			TestConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife_RelationTypeCore(OrgPartRelation.RelationshipTypes.Both, true);
		}

		public void TestConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife_RelationType_WarehouseConsignee()
		{
			TestConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife_RelationTypeCore(OrgPartRelation.RelationshipTypes.WarehouseConsignee, false);
		}

		void TestConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife_RelationTypeCore(string relationship, bool expectError)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("MEL", "A");
			var client = Factory.Load<OrgHeader>(helper.CreateClient("Owner"));
			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "PartA");
			helper.CreateProductParamsByWhsAndClient(part.PK, client.PK, warehouse.PK, 45);

			var relation = part.RelatedOrganisations.FindFirstByOrganisationPK(client.PK);
			AssertNoErrors("Precondition", relation.OU_ConsigneeMinShelfLifeAcceptedInfo);

			relation.OU_Relationship = relationship;
			relation.OU_ConsigneeMinShelfLifeAccepted = 70;
			if (expectError)
			{
				AssertEquals("Should return error message", "Minimum shelf life 70 cannot be greater than Maximum Shelf Life 45.",
					OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife(relation, 70));
			}
			else
			{
				AssertEquals("Should not return error message",
					ZString.Empty, OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife(relation, 70));

				relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
				AssertEquals("Should return error message", "Minimum shelf life 70 cannot be greater than Maximum Shelf Life 45.",
					OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife(relation, 70));
			}
		}

		#endregion

		#endregion

		#region TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress

		public void TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_RelationIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(null, 10));
		}

		public void TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_ExpiryDateNotUsed()
		{
			var today = ZDate.Today;
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("MEL", "A");
			var client = Factory.Load<OrgHeader>(helper.CreateClient("Owner"));
			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "PartA");
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";

			var relation = part.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			relation.OU_ConsigneeMinShelfLifeAccepted = 30;
			Factory.Save();

			AssertEquals("Expiry Date is not used", false, consignee.MiscServ.OM_IMUseExpiryDate);
			AssertEquals("Expiry Date is not used", false, relation.OU_UseExpiryDate);

			helper.CreateWhsReceiveWithInventory(client.PK, warehouse.PK, "R1", part.PK, 10m);
			Factory.Save();

			AssertEquals("Should not return error message if expiry date not used", ZString.Empty, OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 0));
			AssertEquals("Should not return error message if expiry date not used", ZString.Empty, OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 60));
		}

		public void TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress()
		{
			var today = ZDate.Today;
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("MEL", "A");
			var client = Factory.Load<OrgHeader>(helper.CreateClient("Owner"));
			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "PartA");
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			client.MiscServ.OM_IMUseExpiryDate = true;

			var relation = part.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			relation.OU_UseExpiryDate = true;
			Factory.Save();

			var receivePK = helper.CreateWhsReceive(client.PK, warehouse.PK, "R1", new ZArchitecture.NotificationBuffer());
			var receiveLinePK = helper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A");
			Factory.Load<IWhsDocketLine>(receiveLinePK).WE_ExpiryDate = today.AddDays(10);
			helper.WhsReceiveAllocateLocationsMock(receivePK);
			helper.FinaliseDocketWithoutUserConfirmation(receivePK);

			var expectedErrorMessage = "This product has been ordered and pick is not finalized therefore minimum Shelf Life cannot be increased.";

			// If there no Orders or Picks for the consignee.
			relation.OU_ConsigneeMinShelfLifeAccepted = 0;
			AssertEquals("Should not return error message when ConsigneeMinShelfLifeAccepted has no change", ZString.Empty, CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 0));

			AssertEquals("Should not return error message when ConsigneeMinShelfLifeAccepted be increased", ZString.Empty, CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 10));
			relation.OU_ConsigneeMinShelfLifeAccepted = 10;
			Factory.Save();

			AssertEquals("Should not return error message when ConsigneeMinShelfLifeAccepted be decreased", ZString.Empty, CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 8));
			relation.OU_ConsigneeMinShelfLifeAccepted = 8;
			Factory.Save();

			var order = helper.CreateWhsOrder(client.PK, warehouse.PK, consignee.PK, "O1");
			var orderLinePK = helper.CreateWhsOrderLine(order.PK, part.PK, 10m);
			Factory.Load<IWhsDocketLine>(orderLinePK).WE_ExpiryDate = today.AddDays(10);
			Factory.Save();

			AssertEquals("Should not return error message when ConsigneeMinShelfLifeAccepted be increased", ZString.Empty, CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 10));
			relation.OU_ConsigneeMinShelfLifeAccepted = 10;
			Factory.Save();

			AssertEquals("Should not return error message when ConsigneeMinShelfLifeAccepted be decreased", ZString.Empty, CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 7));
			relation.OU_ConsigneeMinShelfLifeAccepted = 7;
			Factory.Save();

			helper.CreateWhsPick(new ZGuid[] { order.PK });
			Factory.Save();
			AssertEquals("Should return error message when ConsigneeMinShelfLifeAccepted be increased", expectedErrorMessage, CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 10));
			relation.OU_ConsigneeMinShelfLifeAccepted = 10;
			Factory.Save();

			AssertEquals("Should not return error message when ConsigneeMinShelfLifeAccepted be decreased", ZString.Empty, CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 5));
			relation.OU_ConsigneeMinShelfLifeAccepted = 5;
			Factory.Save();

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 5;
			Factory.Save();
			AssertEquals("Should not return error message when consignee override the value", ZString.Empty, CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 20));
		}

		public void TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_OtherRelation()
		{
			var today = ZDate.Today;
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("MEL", "A");
			var client = Factory.Load<OrgHeader>(helper.CreateClient("Owner"));
			var otherClient = Factory.Load<OrgHeader>(helper.CreateClient("OC"));
			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "PartA");
			var otherPart = (OrgSupplierPart)helper.CreateProduct(client.PK, "PartB");
			helper.CreateProductParamsByWhsAndClient(part.PK, client.PK, warehouse.PK, 60);
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			client.MiscServ.OM_IMUseExpiryDate = true;

			var relation = part.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			relation.OU_UseExpiryDate = true;

			var relationOtherClient = Factory.New<OrgPartRelation>();
			relationOtherClient.OU_OH = otherClient.PK;
			relationOtherClient.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			part.RelatedOrganisations.Add(relationOtherClient);

			Factory.Save();

			var relationOtherProduct = otherPart.RelatedOrganisations.Cast<OrgPartRelation>().Single();

			var receivePK = helper.CreateWhsReceive(client.PK, warehouse.PK, "R1", new ZArchitecture.NotificationBuffer());
			var receiveLinePK = helper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A");
			Factory.Load<IWhsDocketLine>(receiveLinePK).WE_ExpiryDate = today.AddDays(10);
			helper.WhsReceiveAllocateLocationsMock(receivePK);
			helper.FinaliseDocketWithoutUserConfirmation(receivePK);

			var expectedErrorMessage = "This product has been ordered and pick is not finalized therefore minimum Shelf Life cannot be increased.";

			AssertEquals("Should not return error message", ZString.Empty, OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 7));
			AssertEquals("Should not return error message", ZString.Empty, OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relationOtherClient, 7));
			AssertEquals("Should not return error message", ZString.Empty, OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relationOtherProduct, 7));
			relation.OU_ConsigneeMinShelfLifeAccepted = 7;
			relationOtherClient.OU_ConsigneeMinShelfLifeAccepted = 7;
			relationOtherProduct.OU_ConsigneeMinShelfLifeAccepted = 7;
			Factory.Save();

			var order = helper.CreateWhsOrder(client.PK, warehouse.PK, consignee.PK, "O1");
			var orderLinePK = helper.CreateWhsOrderLine(order.PK, part.PK, 10m);
			Factory.Load<IWhsDocketLine>(orderLinePK).WE_ExpiryDate = today.AddDays(10);
			Factory.Save();

			helper.CreateWhsPick(new ZGuid[] { order.PK });
			Factory.Save();

			AssertEquals("Should return error message when ConsigneeMinShelfLifeAccepted be increased", expectedErrorMessage, OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 10));
			AssertEquals("Should not return error message when other client does not have pick in progress", ZString.Empty, OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relationOtherClient, 10));
			AssertEquals("Should not return error message when other product does not have pick in progress", ZString.Empty, OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relationOtherProduct, 10));
		}

		#region TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_RelationType

		[TestDate(2023, 07, 04)]
		public void TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_RelationType_Owner()
		{
			TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_RelationTypeCore(OrgPartRelation.RelationshipTypes.Owner, true);
		}

		[TestDate(2023, 07, 04)]
		public void TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_RelationType_Supplier()
		{
			TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_RelationTypeCore(OrgPartRelation.RelationshipTypes.Supplier, false);
		}

		[TestDate(2023, 07, 04)]
		public void TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_RelationType_Both()
		{
			TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_RelationTypeCore(OrgPartRelation.RelationshipTypes.Both, true);
		}

		[TestDate(2023, 07, 04)]
		public void TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_RelationType_WarehouseConsignee()
		{
			TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_RelationTypeCore(OrgPartRelation.RelationshipTypes.WarehouseConsignee, true);
		}

		void TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_RelationTypeCore(string relationship, bool expectError)
		{
			var today = ZDate.Today;
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("MEL", "A");
			var client = Factory.Load<OrgHeader>(helper.CreateClient("Owner"));
			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "PartA");
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			client.MiscServ.OM_IMUseExpiryDate = true;

			var ownerRelation = part.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			ownerRelation.OU_UseExpiryDate = true;

			var relation = ownerRelation;
			if (relationship != OrgPartRelation.RelationshipTypes.Owner && relationship != OrgPartRelation.RelationshipTypes.Both)
			{
				relation = (OrgPartRelation)helper.CreateProductClientRelationShip(consignee.PK, part.PK);
			}
			relation.OU_Relationship = relationship;
			Factory.Save();

			var receivePK = helper.CreateWhsReceive(client.PK, warehouse.PK, "R1", new ZArchitecture.NotificationBuffer());
			var receiveLinePK = helper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A");
			Factory.Load<IWhsDocketLine>(receiveLinePK).WE_ExpiryDate = today.AddDays(10);
			helper.WhsReceiveAllocateLocationsMock(receivePK);
			helper.FinaliseDocketWithoutUserConfirmation(receivePK);

			var order = helper.CreateWhsOrder(client.PK, warehouse.PK, consignee.PK, "O1");
			var orderLinePK = helper.CreateWhsOrderLine(order.PK, part.PK, 10m);
			Factory.Load<IWhsDocketLine>(orderLinePK).WE_ExpiryDate = today.AddDays(10);
			Factory.Save();

			AssertEquals(
				"Should not return error message when no pick is processing",
				ZString.Empty,
				CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 10));
			relation.OU_ConsigneeMinShelfLifeAccepted = 10;
			Factory.Save();

			helper.CreateWhsPick(new ZGuid[] { order.PK });
			Factory.Save();

			var expectedErrorMessage = "This product has been ordered and pick is not finalized therefore minimum Shelf Life cannot be increased.";
			if (expectError)
			{
				AssertEquals(
					expectedErrorMessage,
					CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 20));
			}
			else
			{
				AssertEquals(
					"Should not return error message",
					ZString.Empty, CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 20));
			}
		}

		[TestDate(2023, 07, 04)]
		public void TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_RelationType_Overrides()
		{
			var today = ZDate.Today;
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("MEL", "A");
			var client = Factory.Load<OrgHeader>(helper.CreateClient("Owner"));
			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "PartA");
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			client.MiscServ.OM_IMUseExpiryDate = true;

			var clientRelation = part.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			clientRelation.OU_UseExpiryDate = true;
			Factory.Save();

			var receivePK = helper.CreateWhsReceive(client.PK, warehouse.PK, "R1", new ZArchitecture.NotificationBuffer());
			var receiveLinePK = helper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A");
			Factory.Load<IWhsDocketLine>(receiveLinePK).WE_ExpiryDate = today.AddDays(10);
			helper.WhsReceiveAllocateLocationsMock(receivePK);
			helper.FinaliseDocketWithoutUserConfirmation(receivePK);

			var order = helper.CreateWhsOrder(client.PK, warehouse.PK, consignee.PK, "O1");
			var orderLinePK = helper.CreateWhsOrderLine(order.PK, part.PK, 10m);
			Factory.Load<IWhsDocketLine>(orderLinePK).WE_ExpiryDate = today.AddDays(10);
			Factory.Save();

			AssertEquals(
				"Should not return error message when no pick is processing",
				ZString.Empty,
				CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(clientRelation, 10));
			clientRelation.OU_ConsigneeMinShelfLifeAccepted = 10;
			Factory.Save();

			helper.CreateWhsPick(new ZGuid[] { order.PK });
			Factory.Save();

			var expectedErrorMessage = "This product has been ordered and pick is not finalized therefore minimum Shelf Life cannot be increased.";
			AssertEquals(
				"Should error when client relation not overridden",
				expectedErrorMessage,
				CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(clientRelation, 20));

			var consigneeRelation = (OrgPartRelation)helper.CreateProductClientRelationShip(consignee.PK, part.PK);
			consigneeRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			consigneeRelation.OU_ConsigneeMinShelfLifeAccepted = 15;
			Factory.Save();

			AssertEquals(
				"Warehouse consignee relation should override client relation",
				ZString.Empty,
				CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(clientRelation, 20));

			AssertEquals(
				"Warehouse consignee relation should override client relation",
				expectedErrorMessage,
				CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(consigneeRelation, 20));

			consigneeRelation.OU_ConsigneeMinShelfLifeAccepted = 0;
			Factory.Save();

			AssertEquals(
				"Should error when client relation not overridden",
				expectedErrorMessage,
				CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(clientRelation, 20));

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 15;
			Factory.Save();

			AssertEquals(
				"Consignee should override client relation",
				ZString.Empty,
				CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(clientRelation, 20));

			consigneeRelation.OU_ConsigneeMinShelfLifeAccepted = 15;
			Factory.Save();

			AssertEquals(
				"Warehouse consignee relation should override consignee",
				expectedErrorMessage,
				CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(consigneeRelation, 20));

			consigneeRelation.OU_ConsigneeMinShelfLifeAccepted = 0;
			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 0;
			Factory.Save();

			AssertEquals(
				"Should error when client relation not overridden",
				expectedErrorMessage,
				CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(clientRelation, 20));
		}

		#endregion

		#region TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_PickCancelOrFinalise

		public void TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_PickIsCancel()
		{
			TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_PickCancelOrFinalise(pickStatus: "CAN");
		}

		public void TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_PickIsFinalised()
		{
			TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_PickCancelOrFinalise(pickStatus: "FIN");
		}

		void TestConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress_PickCancelOrFinalise(string pickStatus)
		{
			var today = ZDate.Today;
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("MEL", "A");
			var client = Factory.Load<OrgHeader>(helper.CreateClient("Owner"));
			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "PartA");
			helper.CreateProductParamsByWhsAndClient(part.PK, client.PK, warehouse.PK, 60);
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			client.MiscServ.OM_IMUseExpiryDate = true;

			var relation = part.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			relation.OU_UseExpiryDate = true;
			Factory.Save();

			var receivePK = helper.CreateWhsReceive(client.PK, warehouse.PK, "R1", new ZArchitecture.NotificationBuffer());
			var receiveLinePK = helper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A");
			Factory.Load<IWhsDocketLine>(receiveLinePK).WE_ExpiryDate = today.AddDays(10);
			helper.WhsReceiveAllocateLocationsMock(receivePK);
			helper.FinaliseDocketWithoutUserConfirmation(receivePK);

			var expectedErrorMessage = "This product has been ordered and pick is not finalized therefore minimum Shelf Life cannot be increased.";

			AssertEquals("Should not return error message", ZString.Empty, OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 7));
			relation.OU_ConsigneeMinShelfLifeAccepted = 7;
			Factory.Save();

			var order = helper.CreateWhsOrder(client.PK, warehouse.PK, consignee.PK, "O1");
			var orderLinePK = helper.CreateWhsOrderLine(order.PK, part.PK, 10m);
			Factory.Load<IWhsDocketLine>(orderLinePK).WE_ExpiryDate = today.AddDays(10);
			Factory.Save();

			var pickPK = helper.CreateWhsPick(new ZGuid[] { order.PK });
			var pickLine = helper.GetPickLines(pickPK).Single();
			helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Now);
			Factory.Save();

			AssertEquals("Should return error message when ConsigneeMinShelfLifeAccepted be increased", expectedErrorMessage, OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 10));
			relation.OU_ConsigneeMinShelfLifeAccepted = 10;
			Factory.Save();

			if (pickStatus == "CAN")
			{
				Factory.Load<IWhsPick>(pickPK).WP_PickStatus = pickStatus;
			}
			else
			{
				helper.FinaliseDocketWithoutUserConfirmation(order.PK);
				helper.FinalisePick(pickPK);
			}
			Factory.Save();

			AssertEquals("Should not return error message when Pick is cancelled or finalised", ZString.Empty, OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relation, 7));
		}

		#endregion

		#endregion
	}
}
