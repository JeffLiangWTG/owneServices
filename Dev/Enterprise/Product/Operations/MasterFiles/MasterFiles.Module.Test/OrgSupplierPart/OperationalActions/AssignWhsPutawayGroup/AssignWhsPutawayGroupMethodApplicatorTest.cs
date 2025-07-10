using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AssignWhsPutawayGroupMethodApplicator))]
	sealed class AssignWhsPutawayGroupMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestAction_EntersNewPutawayGroup

		public void TestAction_EntersNewPutawayGroup()
		{
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var clientPK = Helper.CreateClient("Client24");
			var product = Helper.CreateProduct(clientPK, "P1");

			var productParams = Helper.CreateProductParamsByWhsAndClient(product.PK, clientPK, warehouse.PK, 1);
			AssertEquals("Precondition: WhsPutawayGroup is empty", true, productParams.W3_WPG_PutawayGroup == ZGuid.Empty);
			Factory.Save();

			var whsPutawayGroup = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup.WPG_Code = "G1";
			whsPutawayGroup.WPG_Description = "Group";

			Applicator.WarehousePK = warehouse.PK;
			Applicator.WhsPutawayGroupPK = whsPutawayGroup.PK;
			ApplyApplicator(new[] { product },
@"INFO: Assigned Warehouse Putaway Group to 1 Product/Warehouse/Client parameter(s) for P1.");
			AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup.PK, productParams.W3_WPG_PutawayGroup);
		}

		#endregion

		#region TestAction_UpdatesPutawayGroup

		public void TestAction_UpdatesPutawayGroup()
		{
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var clientPK = Helper.CreateClient("Client24");
			var product = Helper.CreateProduct(clientPK, "P82");

			var whsPutawayGroup1 = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup1.WPG_Code = "G5";
			whsPutawayGroup1.WPG_Description = "Put here";

			var productParams = Helper.CreateProductParamsByWhsAndClient(product.PK, clientPK, warehouse.PK, 1);
			productParams.W3_WPG_PutawayGroup = whsPutawayGroup1.PK;
			AssertEquals("Precondition: WhsPutawayGroup is NOT empty", whsPutawayGroup1.PK, productParams.W3_WPG_PutawayGroup);

			var whsPutawayGroup2 = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup2.WPG_Code = "G2";
			whsPutawayGroup2.WPG_Description = "Group2";

			Applicator.WarehousePK = warehouse.PK;
			Applicator.WhsPutawayGroupPK = whsPutawayGroup2.PK;
			ApplyApplicator(new[] { product },
@"INFO: Assigned Warehouse Putaway Group to 1 Product/Warehouse/Client parameter(s) for P82.");
			AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup2.PK, productParams.W3_WPG_PutawayGroup);
		}

		#endregion

		#region TestAction_SetOrUpdatesAllProductsParamsWithPutawayGroup

		public void TestAction_SetOrUpdatesAllProductsParamsWithPutawayGroup()
		{
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var client1PK = Helper.CreateClient("Client24");
			var client2PK = Helper.CreateClient("Client48");
			var product1 = Helper.CreateProduct(client1PK, "P82");
			Helper.CreateProductClientRelationShip(client2PK, product1.PK);

			var product2 = Helper.CreateProduct(client2PK, "P981");
			Helper.CreateProductClientRelationShip(client1PK, product2.PK);

			var whsPutawayGroup1 = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup1.WPG_Code = "G5";
			whsPutawayGroup1.WPG_Description = "Put here";

			var productParams1 = Helper.CreateProductParamsByWhsAndClient(product1.PK, client1PK, warehouse.PK, 1);
			AssertEquals("Precondition: productParams1 WhsPutawayGroup is empty", true, productParams1.W3_WPG_PutawayGroup == ZGuid.Empty);
			var productParams2 = Helper.CreateProductParamsByWhsAndClient(product1.PK, client2PK, warehouse.PK, 1);
			productParams2.W3_WPG_PutawayGroup = whsPutawayGroup1.PK;
			AssertEquals("Precondition: productParams2 WhsPutawayGroup is NOT empty", whsPutawayGroup1.PK, productParams2.W3_WPG_PutawayGroup);

			var productParams3 = Helper.CreateProductParamsByWhsAndClient(product2.PK, client1PK, warehouse.PK, 1);
			AssertEquals("Precondition: productParams3 WhsPutawayGroup is empty", true, productParams1.W3_WPG_PutawayGroup == ZGuid.Empty);
			var productParams4 = Helper.CreateProductParamsByWhsAndClient(product2.PK, client2PK, warehouse.PK, 1);
			productParams4.W3_WPG_PutawayGroup = whsPutawayGroup1.PK;
			AssertEquals("Precondition: productParams4 WhsPutawayGroup is NOT empty", whsPutawayGroup1.PK, productParams2.W3_WPG_PutawayGroup);

			var whsPutawayGroup2 = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup2.WPG_Code = "G2";
			whsPutawayGroup2.WPG_Description = "Group2";

			Applicator.WarehousePK = warehouse.PK;
			Applicator.WhsPutawayGroupPK = whsPutawayGroup2.PK;
			ApplyApplicator(new[] { product1 },
@"INFO: Assigned Warehouse Putaway Group to 2 Product/Warehouse/Client parameter(s) for P82.");

			AssertEquals("productParams1 WhsPutawayGroup is correct", whsPutawayGroup2.PK, productParams1.W3_WPG_PutawayGroup);
			AssertEquals("productParams2 WhsPutawayGroup is correct", whsPutawayGroup2.PK, productParams2.W3_WPG_PutawayGroup);
			AssertEquals("productParams3 WhsPutawayGroup is correct", ZGuid.Empty, productParams3.W3_WPG_PutawayGroup);
			AssertEquals("productParams4 WhsPutawayGroup is correct", whsPutawayGroup1.PK, productParams4.W3_WPG_PutawayGroup);
		}

		#endregion

		#region TestAction_CreatesProductParamWithCorrectValues

		public void TestAction_CreatesProductParamWithCorrectValues()
		{
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var clientPK = Helper.CreateClient("Client24");
			var product = Helper.CreateProduct(clientPK, "P4G");

			AssertNull("No ProductParamsByWhsAndClient should exist", Helper.GetProductParamsByWhsAndClient(product.PK, warehouse.PK, clientPK));

			var whsPutawayGroup = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup.WPG_Code = "G1";
			whsPutawayGroup.WPG_Description = "Group";

			Applicator.WarehousePK = warehouse.PK;
			Applicator.WhsPutawayGroupPK = whsPutawayGroup.PK;
			ApplyApplicator(new[] { product },
@"INFO: Created Product/Warehouse/Client 1 parameter(s) for P4G and assigned Warehouse Putaway Group.");

			var productParams = Helper.GetProductParamsByWhsAndClient(product.PK, warehouse.PK, clientPK);
			AssertNotNull(nameof(productParams), productParams);
			AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup.PK, productParams.W3_WPG_PutawayGroup);
			AssertEquals("Warehouse is correct", warehouse.PK, productParams.W3_WW);
			AssertEquals("ClientPK is correct", clientPK, productParams.W3_OH);
		}

		public void TestAction_CreatesMultipleProductParamsWithCorrectValues()
		{
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var client1PK = Helper.CreateClient("Client24");
			var client2PK = Helper.CreateClient("Client456");
			var client3PK = Helper.CreateClient("Clients2");
			var product = Helper.CreateProduct(client1PK, "P4G");
			Helper.CreateProductClientRelationShip(client2PK, product.PK);
			Helper.CreateProductClientRelationShip(client3PK, product.PK);

			AssertNull("No ProductParamsByWhsAndClient should exist for client1", Helper.GetProductParamsByWhsAndClient(product.PK, warehouse.PK, client1PK));
			AssertNull("No ProductParamsByWhsAndClient should exist for client2", Helper.GetProductParamsByWhsAndClient(product.PK, warehouse.PK, client2PK));
			AssertNull("No ProductParamsByWhsAndClient should exist for client3", Helper.GetProductParamsByWhsAndClient(product.PK, warehouse.PK, client3PK));

			var whsPutawayGroup = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup.WPG_Code = "G1";
			whsPutawayGroup.WPG_Description = "Group";

			Applicator.WarehousePK = warehouse.PK;
			Applicator.WhsPutawayGroupPK = whsPutawayGroup.PK;
			ApplyApplicator(new[] { product },
@"INFO: Created Product/Warehouse/Client 3 parameter(s) for P4G and assigned Warehouse Putaway Group.");

			var productParams1 = Helper.GetProductParamsByWhsAndClient(product.PK, warehouse.PK, client1PK);
			AssertNotNull(nameof(productParams1), productParams1);
			AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup.PK, productParams1.W3_WPG_PutawayGroup);
			AssertEquals("Warehouse is correct", warehouse.PK, productParams1.W3_WW);
			AssertEquals("ClientPK is correct", client1PK, productParams1.W3_OH);

			var productParams2 = Helper.GetProductParamsByWhsAndClient(product.PK, warehouse.PK, client2PK);
			AssertNotNull(nameof(productParams2), productParams2);
			AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup.PK, productParams2.W3_WPG_PutawayGroup);
			AssertEquals("Warehouse is correct", warehouse.PK, productParams2.W3_WW);
			AssertEquals("ClientPK is correct", client2PK, productParams2.W3_OH);

			var productParams3 = Helper.GetProductParamsByWhsAndClient(product.PK, warehouse.PK, client3PK);
			AssertNotNull(nameof(productParams3), productParams3);
			AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup.PK, productParams3.W3_WPG_PutawayGroup);
			AssertEquals("Warehouse is correct", warehouse.PK, productParams3.W3_WW);
			AssertEquals("ClientPK is correct", client3PK, productParams3.W3_OH);
		}

		public void TestAction_CreatesOrSetsMultipleProductParamsWithCorrectValues()
		{
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var client1PK = Helper.CreateClient("Client24");
			var client2PK = Helper.CreateClient("Client456");
			var client3PK = Helper.CreateClient("Clients2");
			var product = Helper.CreateProduct(client1PK, "P4G");
			Helper.CreateProductClientRelationShip(client2PK, product.PK);
			Helper.CreateProductClientRelationShip(client3PK, product.PK);

			var whsPutawayGroup1 = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup1.WPG_Code = "G5";
			whsPutawayGroup1.WPG_Description = "Put here";

			var productParams1 = Helper.CreateProductParamsByWhsAndClient(product.PK, client1PK, warehouse.PK, 1);
			AssertEquals("Precondition: productParams1 WhsPutawayGroup is empty", true, productParams1.W3_WPG_PutawayGroup == ZGuid.Empty);
			var productParams2 = Helper.CreateProductParamsByWhsAndClient(product.PK, client2PK, warehouse.PK, 1);
			productParams2.W3_WPG_PutawayGroup = whsPutawayGroup1.PK;
			AssertEquals("Precondition: productParams2 WhsPutawayGroup is NOT empty", whsPutawayGroup1.PK, productParams2.W3_WPG_PutawayGroup);

			AssertNull("No ProductParamsByWhsAndClient should exist for client3", Helper.GetProductParamsByWhsAndClient(product.PK, warehouse.PK, client3PK));

			var whsPutawayGroup2 = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup2.WPG_Code = "G2";
			whsPutawayGroup2.WPG_Description = "Group";

			Applicator.WarehousePK = warehouse.PK;
			Applicator.WhsPutawayGroupPK = whsPutawayGroup2.PK;
			ApplyApplicator(new[] { product },
@"INFO: Created Product/Warehouse/Client 1 parameter(s) for P4G and assigned Warehouse Putaway Group.
INFO: Assigned Warehouse Putaway Group to 2 Product/Warehouse/Client parameter(s) for P4G.");

			AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup2.PK, productParams1.W3_WPG_PutawayGroup);
			AssertEquals("Warehouse is correct", warehouse.PK, productParams1.W3_WW);
			AssertEquals("ClientPK is correct", client1PK, productParams1.W3_OH);

			AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup2.PK, productParams2.W3_WPG_PutawayGroup);
			AssertEquals("Warehouse is correct", warehouse.PK, productParams2.W3_WW);
			AssertEquals("ClientPK is correct", client2PK, productParams2.W3_OH);

			var productParams3 = Helper.GetProductParamsByWhsAndClient(product.PK, warehouse.PK, client3PK);
			AssertNotNull(nameof(productParams3), productParams3);
			AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup2.PK, productParams3.W3_WPG_PutawayGroup);
			AssertEquals("Warehouse is correct", warehouse.PK, productParams3.W3_WW);
			AssertEquals("ClientPK is correct", client3PK, productParams3.W3_OH);
		}

		#endregion

		#region TestAction_WhsAndPutawayGroupAlreadyOnProduct

		public void TestAction_WhsAndPutawayGroupAlreadyOnProduct()
		{
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var clientPK = Helper.CreateClient("Client24");
			var product = Helper.CreateProduct(clientPK, "P82");

			var whsPutawayGroup = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup.WPG_Code = "G2";
			whsPutawayGroup.WPG_Description = "Group2";

			var productParams = Helper.CreateProductParamsByWhsAndClient(product.PK, clientPK, warehouse.PK, 1);
			productParams.W3_WPG_PutawayGroup = whsPutawayGroup.PK;
			AssertEquals("Precondition: WhsPutawayGroup is NOT empty", whsPutawayGroup.PK, productParams.W3_WPG_PutawayGroup);

			Applicator.WarehousePK = warehouse.PK;
			Applicator.WhsPutawayGroupPK = whsPutawayGroup.PK;
			ApplyApplicator(new[] { product },
@"INFO: Assigned Warehouse Putaway Group to 1 Product/Warehouse/Client parameter(s) for P82.");
			AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup.PK, productParams.W3_WPG_PutawayGroup);
		}

		#endregion

		#region TestAction_OnEmptyEnteredValue_ClearsPutawayGroup

		public void TestAction_OnEmptyEnteredValue_ClearsPutawayGroup()
		{
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var clientPK = Helper.CreateClient("Client24");
			var product = Helper.CreateProduct(clientPK, "P82");

			var whsPutawayGroup = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup.WPG_Code = "G2";
			whsPutawayGroup.WPG_Description = "Group2";

			var productParams = Helper.CreateProductParamsByWhsAndClient(product.PK, clientPK, warehouse.PK, 1);
			productParams.W3_WPG_PutawayGroup = whsPutawayGroup.PK;
			AssertEquals("Precondition: WhsPutawayGroup is NOT empty", whsPutawayGroup.PK, productParams.W3_WPG_PutawayGroup);

			Applicator.WarehousePK = warehouse.PK;
			ApplyApplicator(new[] { product },
@"INFO: Unassigned Warehouse Putaway Group from P82.");
			AssertEquals("WhsPutawayGroup is now empty", true, productParams.W3_WPG_PutawayGroup == ZGuid.Empty);
		}

		public void TestAction_OnEmptyEnteredValue_DoesNotCreateNewParams()
		{
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var client1PK = Helper.CreateClient("Client24");
			var client2PK = Helper.CreateClient("Client456");
			var client3PK = Helper.CreateClient("Clients2");
			var product = Helper.CreateProduct(client1PK, "P4G");
			Helper.CreateProductClientRelationShip(client2PK, product.PK);
			Helper.CreateProductClientRelationShip(client3PK, product.PK);

			var whsPutawayGroup = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup.WPG_Code = "G2";
			whsPutawayGroup.WPG_Description = "Group2";

			var productParams = Helper.CreateProductParamsByWhsAndClient(product.PK, client1PK, warehouse.PK, 1);
			productParams.W3_WPG_PutawayGroup = whsPutawayGroup.PK;
			AssertEquals("Precondition: client1 WhsPutawayGroup is NOT empty", whsPutawayGroup.PK, productParams.W3_WPG_PutawayGroup);

			AssertNull("No ProductParamsByWhsAndClient should exist for client2", Helper.GetProductParamsByWhsAndClient(product.PK, warehouse.PK, client2PK));
			AssertNull("No ProductParamsByWhsAndClient should exist for client3", Helper.GetProductParamsByWhsAndClient(product.PK, warehouse.PK, client3PK));

			Applicator.WarehousePK = warehouse.PK;
			ApplyApplicator(new[] { product },
@"INFO: Unassigned Warehouse Putaway Group from P4G.");

			AssertEquals("WhsPutawayGroup is now empty", true, productParams.W3_WPG_PutawayGroup == ZGuid.Empty);
			AssertNull("No ProductParamsByWhsAndClient should exist for client2", Helper.GetProductParamsByWhsAndClient(product.PK, warehouse.PK, client2PK));
			AssertNull("No ProductParamsByWhsAndClient should exist for client3", Helper.GetProductParamsByWhsAndClient(product.PK, warehouse.PK, client3PK));
		}

		#endregion

		#region TestAction_MultipleProducts_DifferentClients

		public void TestAction_MultipleProducts_DifferentClients()
		{
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var client1PK = Helper.CreateClient("Client24");
			var client2PK = Helper.CreateClient("Client99");
			var client3PK = Helper.CreateClient("Client8");
			var product1 = Helper.CreateProduct(client1PK, "P82");
			var product2 = Helper.CreateProduct(client2PK, "P4G");
			var product3 = Helper.CreateProduct(client3PK, "P34");

			var consignee = (OrgPartRelation)Helper.CreateProductClientRelationShip(client3PK, product2.PK);
			consignee.OU_Relationship = "WCN";

			var supplier = (OrgPartRelation)Helper.CreateProductClientRelationShip(client3PK, product1.PK);
			supplier.OU_Relationship = "SUP";

			var both = (OrgPartRelation)Helper.CreateProductClientRelationShip(client2PK, product1.PK);
			both.OU_Relationship = "BTH";

			var whsPutawayGroup1 = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup1.WPG_Code = "G5";
			whsPutawayGroup1.WPG_Description = "Put here";

			var productParams1 = Helper.CreateProductParamsByWhsAndClient(product1.PK, client1PK, warehouse.PK, 1);
			productParams1.W3_WPG_PutawayGroup = whsPutawayGroup1.PK;
			AssertEquals("Precondition: WhsPutawayGroup is NOT empty", whsPutawayGroup1.PK, productParams1.W3_WPG_PutawayGroup);

			var productParams3 = Helper.CreateProductParamsByWhsAndClient(product3.PK, client3PK, warehouse.PK, 1);
			AssertEquals("Precondition: WhsPutawayGroup is empty", true, productParams3.W3_WPG_PutawayGroup == ZGuid.Empty);

			var whsPutawayGroup2 = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup2.WPG_Code = "G2";
			whsPutawayGroup2.WPG_Description = "Group2";

			CombineAssertions(() =>
			{
				Applicator.WarehousePK = warehouse.PK;
				Applicator.WhsPutawayGroupPK = whsPutawayGroup2.PK;
				ApplyApplicator(new[] { product1, product2, product3 },
	@"INFO: Created Product/Warehouse/Client 1 parameter(s) for P82 and assigned Warehouse Putaway Group.
INFO: Assigned Warehouse Putaway Group to 1 Product/Warehouse/Client parameter(s) for P82.
INFO: Created Product/Warehouse/Client 1 parameter(s) for P4G and assigned Warehouse Putaway Group.
INFO: Assigned Warehouse Putaway Group to 1 Product/Warehouse/Client parameter(s) for P34.");

				var productParams2 = Helper.GetProductParamsByWhsAndClient(product2.PK, warehouse.PK, client2PK);
				AssertNotNull(nameof(productParams2), productParams2);
				AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup2.PK, productParams1.W3_WPG_PutawayGroup);
				AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup2.PK, productParams2.W3_WPG_PutawayGroup);
				AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup2.PK, productParams3.W3_WPG_PutawayGroup);

				var productParams4 = Helper.GetProductParamsByWhsAndClient(product2.PK, warehouse.PK, client3PK);
				AssertNull(nameof(productParams4), productParams4);

				var productParams5 = Helper.GetProductParamsByWhsAndClient(product1.PK, warehouse.PK, client3PK);
				AssertNull(nameof(productParams5), productParams5);

				var productParams6 = Helper.GetProductParamsByWhsAndClient(product1.PK, warehouse.PK, client2PK);
				AssertNotNull(nameof(productParams6), productParams6);
				AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup2.PK, productParams6.W3_WPG_PutawayGroup);
			});
		}

		#endregion

		#region TestAction_MultipleProducts_DifferentWarehouses

		public void TestAction_MultipleProducts_DifferentWarehouses()
		{
			var warehouse1 = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var warehouse2 = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var clientPK = Helper.CreateClient("Client24");
			var product1 = Helper.CreateProduct(clientPK, "P82");
			var product2 = Helper.CreateProduct(clientPK, "P4G");
			var product3 = Helper.CreateProduct(clientPK, "P34");

			var whsPutawayGroup1 = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup1.WPG_Code = "G5";
			whsPutawayGroup1.WPG_Description = "Put here";

			var productParams1 = Helper.CreateProductParamsByWhsAndClient(product1.PK, clientPK, warehouse2.PK, 1);
			productParams1.W3_WPG_PutawayGroup = whsPutawayGroup1.PK;
			AssertEquals("Precondition: WhsPutawayGroup is NOT empty", whsPutawayGroup1.PK, productParams1.W3_WPG_PutawayGroup);

			var productParams3 = Helper.CreateProductParamsByWhsAndClient(product3.PK, clientPK, warehouse1.PK, 1);
			AssertEquals("Precondition: WhsPutawayGroup is empty", true, productParams3.W3_WPG_PutawayGroup == ZGuid.Empty);

			var whsPutawayGroup2 = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup2.WPG_Code = "G2";
			whsPutawayGroup2.WPG_Description = "Group2";

			Applicator.WarehousePK = warehouse1.PK;
			Applicator.WhsPutawayGroupPK = whsPutawayGroup2.PK;
			ApplyApplicator(new[] { product1, product2, product3 },
@"INFO: Created Product/Warehouse/Client 1 parameter(s) for P82 and assigned Warehouse Putaway Group.
INFO: Created Product/Warehouse/Client 1 parameter(s) for P4G and assigned Warehouse Putaway Group.
INFO: Assigned Warehouse Putaway Group to 1 Product/Warehouse/Client parameter(s) for P34.");

			AssertEquals("WhsPutawayGroup stays the same for Whs2", whsPutawayGroup1.PK, productParams1.W3_WPG_PutawayGroup);

			var product1Params2 = Helper.GetProductParamsByWhsAndClient(product1.PK, warehouse1.PK, clientPK);
			AssertNotNull(nameof(product1Params2), product1Params2);
			AssertEquals("WhsPutawayGroup is correct for new param for Product1", whsPutawayGroup2.PK, product1Params2.W3_WPG_PutawayGroup);

			var productParams2 = Helper.GetProductParamsByWhsAndClient(product2.PK, warehouse1.PK, clientPK);
			AssertNotNull(nameof(productParams2), productParams2);
			AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup2.PK, productParams2.W3_WPG_PutawayGroup);
			AssertEquals("WhsPutawayGroup is correct", whsPutawayGroup2.PK, productParams3.W3_WPG_PutawayGroup);
		}

		#endregion

		#region TestAction_ErrorIfNothingEntered

		public void TestAction_ErrorIfNothingEntered()
		{
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var clientPK = Helper.CreateClient("Client24");
			var product = Helper.CreateProduct(clientPK, "P1");

			var whsPutawayGroup = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup.WPG_Code = "G1";
			whsPutawayGroup.WPG_Description = "Group";

			ApplyApplicator(new[] { product }, "ERROR: Enter a Warehouse.");

			Applicator.WarehousePK = warehouse.PK;
			Applicator.WhsPutawayGroupPK = whsPutawayGroup.PK;
			ApplyApplicator(new[] { product }, "INFO: Created Product/Warehouse/Client 1 parameter(s) for P1 and assigned Warehouse Putaway Group.");
		}

		#endregion

		#region TestAction_ErrorIfProductHasNoRelatedOrganisations

		public void TestAction_ErrorIfProductHasNoRelatedOrganisations()
		{
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "P1";
			product.OP_Desc = "P1";

			var whsPutawayGroup = Factory.New<IWhsPutawayGroup>();
			whsPutawayGroup.WPG_Code = "G1";
			whsPutawayGroup.WPG_Description = "Group";

			Applicator.WarehousePK = warehouse.PK;
			Applicator.WhsPutawayGroupPK = whsPutawayGroup.PK;
			ApplyApplicator(new[] { product }, "ERROR: Product P1 does not have any Related Organizations.");
		}

		#endregion

		#region TestWarehousePK

		public void TestWarehousePK()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(AssignWhsPutawayGroupMethodApplicator), "WarehousePK", false,
				la => la.ListDataSourceMember == "Lookups.Warehouses");
		}

		#endregion

		#region TestWhsPutawayGroupPK

		public void TestWhsPutawayGroupPK()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(AssignWhsPutawayGroupMethodApplicator), "WhsPutawayGroupPK", false,
				la => la.ListDataSourceMember == "Lookups.WhsPutawayGroups");
		}

		#endregion

		#region Implementation

		IWhsTransactionTestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
				}
				return fHelper;
			}
		}
		IWhsTransactionTestHelper fHelper;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AssignWhsPutawayGroupMethodApplicator("test", Factory);
		}

		#endregion

		new AssignWhsPutawayGroupMethodApplicator Applicator => (AssignWhsPutawayGroupMethodApplicator)base.Applicator;
	}
}
