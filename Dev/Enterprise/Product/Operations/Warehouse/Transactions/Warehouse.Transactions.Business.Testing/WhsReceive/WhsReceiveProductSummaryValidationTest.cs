using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsReceiveProductSummaryValidationTest : WhsBusinessObjectValidationTestCase
	{
		public void TestAutoValidationType()
		{
			AssertEquals(typeof(WhsReceiveProductSummaryValidation), CreateReceiveProductSummary().Validation.AutoValidationType);
		}

		public void TestValidateReceivedQuantity_PreventReceivingOversIsFalse()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = Helper.CreateWarehouse("W1");
			var product = Helper.CreateProduct(client, "P1");
			Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			var productSummary = new WhsReceiveProductSummary(Factory, client.PK, warehouse.PK, product.PK, false, product.OP_PartNum, ZString.Empty, product.OP_Desc, 10m, 11m);
			AssertNoErrors(productSummary.ReceivedQuantityInfo);
		}

		public void TestValidateReceivedQuantity_PreventReceivingOversIsTrue_ReceivedQuantityIsValid()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = Helper.CreateWarehouse("W1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 100;
			Factory.Save();

			var productSummary = new WhsReceiveProductSummary(Factory, client.PK, warehouse.PK, product.PK, false, product.OP_PartNum, product.OP_Desc, ZString.Empty, 10m, 20m);
			AssertNoErrors(productSummary.ReceivedQuantityInfo);
		}

		public void TestValidateReceivedQuantity_PreventReceivingOversIsTrue_ReceivedQuantityIsInvalid()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = Helper.CreateWarehouse("W1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 100;
			Factory.Save();

			var productSummary = new WhsReceiveProductSummary(Factory, client.PK, warehouse.PK, product.PK, false, product.OP_PartNum, product.OP_Desc, ZString.Empty, 10m, 21m);
			AssertHasErrors("Received quantity of product 'P1' exceeds the allowed quantity.", productSummary.ReceivedQuantityInfo);
		}

		public void TestValidateReceivedQuantity_PreventReceivingOversIsTrue_BlindReceivedQuantityIsLargerThanAllowedQuantity()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = Helper.CreateWarehouse("W1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 100;
			Factory.Save();

			var productSummary = new WhsReceiveProductSummary(Factory, client.PK, warehouse.PK, product.PK, true, product.OP_PartNum, product.OP_Desc, ZString.Empty, 10m, 21m);
			AssertNoErrors(productSummary.ReceivedQuantityInfo);
		}

		public void TestValidateReceivedQuantity_PreventReceivingOversIsTrue_ExpectedQuantityIsZero()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = Helper.CreateWarehouse("W1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 100;
			Factory.Save();

			var productSummary = new WhsReceiveProductSummary(Factory, client.PK, warehouse.PK, product.PK, false, product.OP_PartNum, product.OP_Desc, ZString.Empty, 0m, 21m);
			AssertHasErrors("Received quantity of product 'P1' exceeds the allowed quantity.", productSummary.ReceivedQuantityInfo);
		}

		public void TestValidateReceivedQuantity_PreventReceivingOvers_OnProductIsFalse_OnReceiveParamsIsTrue_Invalid()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = Helper.CreateWarehouse("W1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = false;

			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(client, warehouse);
			whsClientParameterByWarehouse1.WY_ReceiveCategory = "RC1";
			whsClientParameterByWarehouse1.WY_PreventReceivingOvers = true;
			whsClientParameterByWarehouse1.WY_ReceiveOverageTolerancePercent = 100;

			Factory.Save();

			var productSummary = new WhsReceiveProductSummary(Factory, client.PK, warehouse.PK, product.PK, false, product.OP_PartNum, product.OP_Desc, "RC1", 1m, 3m);
			AssertHasErrors("Received quantity of product 'P1' exceeds the allowed quantity.", productSummary.ReceivedQuantityInfo);
		}

		public void TestValidateReceivedQuantity_PreventReceivingOvers_OnProductIsFalse_OnReceiveParamsIsTrue_Valid()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = Helper.CreateWarehouse("W1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = false;

			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(client, warehouse);
			whsClientParameterByWarehouse1.WY_ReceiveCategory = "RC1";
			whsClientParameterByWarehouse1.WY_PreventReceivingOvers = true;
			whsClientParameterByWarehouse1.WY_ReceiveOverageTolerancePercent = 100;

			Factory.Save();

			var productSummary = new WhsReceiveProductSummary(Factory, client.PK, warehouse.PK, product.PK, false, product.OP_PartNum, product.OP_Desc, "RC1", 10m, 11m);
			AssertNoErrors(productSummary.ReceivedQuantityInfo);
		}

		public void TestValidateReceivedQuantity_PreventReceivingOvers_OnProductIsFalse_OnReceiveParamsIsFalse()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = Helper.CreateWarehouse("W1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = false;

			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(client, warehouse);
			whsClientParameterByWarehouse1.WY_ReceiveCategory = "RC1";
			whsClientParameterByWarehouse1.WY_PreventReceivingOvers = false;

			Factory.Save();

			var productSummary = new WhsReceiveProductSummary(Factory, client.PK, warehouse.PK, product.PK, false, product.OP_PartNum, product.OP_Desc, "RC1", 10m, 11m);
			AssertNoErrors(productSummary.ReceivedQuantityInfo);
		}

		#region TestValidateAll

		public void TestValidateAll_ReceivedQuantityIsValid()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = Helper.CreateWarehouse("W1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 500;
			Factory.Save();

			WhsReceiveProductSummary productSummary = new WhsReceiveProductSummary(Factory, client.PK, warehouse.PK, product.PK, false, product.OP_PartNum, product.OP_Desc, ZString.Empty, 1m, 6m);
			productSummary.Validation.ValidateAll();
			AssertNoErrors(productSummary.ReceivedQuantityInfo);
		}

		public void TestValidateAll_ReceivedQuantityIsInvalid()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = Helper.CreateWarehouse("W1");
			var product = Helper.CreateProduct(client, "P1", OrgPartRelation.RelationshipTypes.Both);
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Both);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 0;
			Factory.Save();

			WhsReceiveProductSummary productSummary = new WhsReceiveProductSummary(Factory, client.PK, warehouse.PK, product.PK, false, product.OP_PartNum, product.OP_Desc, ZString.Empty, 1m, 2m);
			productSummary.Validation.ValidateAll();
			AssertHasErrors("Received quantity of product 'P1' exceeds the allowed quantity.", productSummary.ReceivedQuantityInfo);
		}

		#endregion

		#region Implementation

		WhsReceiveProductSummary CreateReceiveProductSummary() => new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, true, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 0m);

		#endregion
	}
}
