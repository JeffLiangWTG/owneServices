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
	[TestedType(typeof(UpdateDynamicPickFaceAreaMethodApplicator))]
	sealed class UpdateDynamicPickFaceAreaMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestAction_InvalidArguments

		public void TestAction_InvalidArguments()
		{
			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "P1";

			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product2.OP_PartNum = "P2";

			var product3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product3.OP_PartNum = "P3";

			var client = Factory.NewWithValidTestData<OrgHeader>();

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WHS", "A", 3, 1);

			Applicator.ClientPK = client.PK;
			Applicator.WarehousePK = ZGuid.Empty;
			Applicator.DynamicPickAreaPK = ZGuid.NewZGuid();
			ApplyApplicator(new[] { product1, product2, product3 }, "ERROR: Enter valid arguments.");

			Applicator.ClientPK = client.PK;
			Applicator.WarehousePK = ZGuid.Empty;
			Applicator.DynamicPickAreaPK = ZGuid.NewZGuid();
			ApplyApplicator(new[] { product1, product2, product3 }, "ERROR: Enter valid arguments.");

			Applicator.ClientPK = client.PK;
			Applicator.WarehousePK = warehouse.PK;
			Applicator.DynamicPickAreaPK = ZGuid.Empty;
			Applicator.OverrideNonEmptyDynamicPickArea = false;
			ApplyApplicator(new[] { product1, product2, product3 }, "ERROR: Enter valid arguments.");
		}

		#endregion

		#region TestAction_SetDynamicPickArea

		public void TestAction_SetDynamicPickArea()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var dynamicAreaPK = ((IWhsArea)Helper.CreateWhsArea(warehouse.PK, "Dynamic", "DPF")).PK;

			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "P1";

			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product2.OP_PartNum = "P2";

			var product3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product3.OP_PartNum = "P3";

			product1.RelatedOrganisations.AddOwner(client);
			product2.RelatedOrganisations.AddOwner(client);
			product3.RelatedOrganisations.AddOwner(client);

			var product1Params = Helper.CreateProductParamsByWhsAndClient(product1.PK, client.PK, warehouse.PK, 1);
			var product2Params = Helper.CreateProductParamsByWhsAndClient(product2.PK, client.PK, warehouse.PK, 1);
			var product3Params = Helper.CreateProductParamsByWhsAndClient(product3.PK, client.PK, warehouse.PK, 1);

			Applicator.ClientPK = client.PK;
			Applicator.WarehousePK = warehouse.PK;
			Applicator.DynamicPickAreaPK = dynamicAreaPK;
			ApplyApplicator(
			new[] { product1, product2, product3 },
			@"
INFO: Updated Dynamic Pick Face Area on Product/Warehouse/Client parameter for P1 to 'Dynamic'.
INFO: Updated Dynamic Pick Face Area on Product/Warehouse/Client parameter for P2 to 'Dynamic'.
INFO: Updated Dynamic Pick Face Area on Product/Warehouse/Client parameter for P3 to 'Dynamic'.");

			AssertEquals("Updates Dynamic Pick Face Area", dynamicAreaPK, product1Params.W3_WA_DynamicPickFaceArea);
			AssertEquals("Updates Dynamic Pick Face Area", dynamicAreaPK, product2Params.W3_WA_DynamicPickFaceArea);
			AssertEquals("Updates Dynamic Pick Face Area", dynamicAreaPK, product3Params.W3_WA_DynamicPickFaceArea);
		}

		#endregion

		#region TestAction_CreateNewRelationshipForExistingClients

		public void TestAction_CreateNewRelationshipForExistingClients()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var dynamicAreaPK = ((IWhsArea)Helper.CreateWhsArea(warehouse.PK, "Dynamic", "DPF")).PK;

			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "P1";

			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product2.OP_PartNum = "P2";

			var product3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product3.OP_PartNum = "P3";

			product1.RelatedOrganisations.AddOwner(client);
			product2.RelatedOrganisations.AddOwner(client);
			product3.RelatedOrganisations.AddOwner(client);

			var product2Params = Helper.CreateProductParamsByWhsAndClient(product2.PK, client.PK, warehouse.PK, 1);

			Applicator.ClientPK = client.PK;
			Applicator.WarehousePK = warehouse.PK;
			Applicator.DynamicPickAreaPK = dynamicAreaPK;
			ApplyApplicator(
			new[] { product1, product2, product3 },
			@"INFO: Created new Product/Warehouse/Client parameter for P1 and assigned Dynamic Pick Face Area to 'Dynamic'.
INFO: Updated Dynamic Pick Face Area on Product/Warehouse/Client parameter for P2 to 'Dynamic'.
INFO: Created new Product/Warehouse/Client parameter for P3 and assigned Dynamic Pick Face Area to 'Dynamic'.");

			var product1Params = Helper.GetProductParamsByWhsAndClient(product1.PK, warehouse.PK, client.PK);
			var product3Params = Helper.GetProductParamsByWhsAndClient(product3.PK, warehouse.PK, client.PK);

			AssertEquals("Updates Dynamic Pick Face Area", dynamicAreaPK, product1Params.W3_WA_DynamicPickFaceArea);
			AssertEquals("Updates Dynamic Pick Face Area", dynamicAreaPK, product2Params.W3_WA_DynamicPickFaceArea);
			AssertEquals("Updates Dynamic Pick Face Area", dynamicAreaPK, product3Params.W3_WA_DynamicPickFaceArea);
		}

		#endregion

		#region TestAction_UpdatesDynamicPickArea

		public void TestAction_UpdatesDynamicPickArea_WhenUsingOverrideNonEmptyDynamicPickArea()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var dynamicAreaPK = ((IWhsArea)Helper.CreateWhsArea(warehouse.PK, "Dynamic", "DPF")).PK;
			var dynamicArea1PK = ((IWhsArea)Helper.CreateWhsArea(warehouse.PK, "Dynamic1", "DPF")).PK;
			var dynamicArea2PK = ((IWhsArea)Helper.CreateWhsArea(warehouse.PK, "Dynamic2", "DPF")).PK;

			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "P1";

			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product2.OP_PartNum = "P2";

			var product3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product3.OP_PartNum = "P3";

			product1.RelatedOrganisations.AddOwner(client);
			product2.RelatedOrganisations.AddOwner(client);
			product3.RelatedOrganisations.AddOwner(client);

			var product1Params = Helper.CreateProductParamsByWhsAndClient(product1.PK, client.PK, warehouse.PK, 1);
			product1Params.W3_WA_DynamicPickFaceArea = ZGuid.Empty;
			var product2Params = Helper.CreateProductParamsByWhsAndClient(product2.PK, client.PK, warehouse.PK, 1);
			product2Params.W3_WA_DynamicPickFaceArea = dynamicArea1PK;
			var product3Params = Helper.CreateProductParamsByWhsAndClient(product3.PK, client.PK, warehouse.PK, 1);
			product3Params.W3_WA_DynamicPickFaceArea = dynamicArea2PK;

			AssertEquals("Precondition", ZGuid.Empty, product1Params.W3_WA_DynamicPickFaceArea);
			AssertEquals("Precondition", dynamicArea1PK, product2Params.W3_WA_DynamicPickFaceArea);
			AssertEquals("Precondition", dynamicArea2PK, product3Params.W3_WA_DynamicPickFaceArea);

			Applicator.ClientPK = client.PK;
			Applicator.WarehousePK = warehouse.PK;
			Applicator.DynamicPickAreaPK = dynamicAreaPK;
			Applicator.OverrideNonEmptyDynamicPickArea = true;

			ApplyApplicator(
			new[] { product1, product2, product3 },
			@"INFO: Updated Dynamic Pick Face Area on Product/Warehouse/Client parameter for P1 to 'Dynamic'.
INFO: Updated Dynamic Pick Face Area on Product/Warehouse/Client parameter for P2 to 'Dynamic'.
INFO: Updated Dynamic Pick Face Area on Product/Warehouse/Client parameter for P3 to 'Dynamic'.");

			AssertEquals("Updates Dynamic Pick Face Area", dynamicAreaPK, product1Params.W3_WA_DynamicPickFaceArea);
			AssertEquals("Updates Dynamic Pick Face Area", dynamicAreaPK, product2Params.W3_WA_DynamicPickFaceArea);
			AssertEquals("Updates Dynamic Pick Face Area", dynamicAreaPK, product3Params.W3_WA_DynamicPickFaceArea);
		}

		#endregion

		#region TestAction_DoesNotUpdateDynamicPickFaceArea_WhenNotUsingOverrideNonEmptyDynamicPickArea

		public void TestAction_DoesNotUpdateDynamicPickFaceArea_WhenNotUsingOverrideNonEmptyDynamicPickArea()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var dynamicArea1PK = ((IWhsArea)Helper.CreateWhsArea(warehouse.PK, "Dynamic1", "DPF")).PK;
			var dynamicArea2PK = ((IWhsArea)Helper.CreateWhsArea(warehouse.PK, "Dynamic2", "DPF")).PK;
			var dynamicArea3PK = ((IWhsArea)Helper.CreateWhsArea(warehouse.PK, "Dynamic3", "DPF")).PK;

			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "P1";

			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product2.OP_PartNum = "P2";

			var product3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product3.OP_PartNum = "P3";

			product1.RelatedOrganisations.AddOwner(client);
			product2.RelatedOrganisations.AddOwner(client);
			product3.RelatedOrganisations.AddOwner(client);

			var product1Params = Helper.CreateProductParamsByWhsAndClient(product1.PK, client.PK, warehouse.PK, 1);
			product1Params.W3_WA_DynamicPickFaceArea = ZGuid.Empty;
			var product2Params = Helper.CreateProductParamsByWhsAndClient(product2.PK, client.PK, warehouse.PK, 1);
			product2Params.W3_WA_DynamicPickFaceArea = dynamicArea1PK;
			var product3Params = Helper.CreateProductParamsByWhsAndClient(product3.PK, client.PK, warehouse.PK, 1);
			product3Params.W3_WA_DynamicPickFaceArea = dynamicArea2PK;

			AssertEquals("Precondition", ZGuid.Empty, product1Params.W3_WA_DynamicPickFaceArea);
			AssertEquals("Precondition", dynamicArea1PK, product2Params.W3_WA_DynamicPickFaceArea);
			AssertEquals("Precondition", dynamicArea2PK, product3Params.W3_WA_DynamicPickFaceArea);

			Applicator.ClientPK = client.PK;
			Applicator.WarehousePK = warehouse.PK;
			AssertEquals("Precondition", true, Applicator.OverrideNonEmptyDynamicPickArea);
			Applicator.DynamicPickAreaPK = dynamicArea3PK;
			AssertEquals("Set DynamicPickAreaPK does not change Override.", true, Applicator.OverrideNonEmptyDynamicPickArea);
			Applicator.OverrideNonEmptyDynamicPickArea = false;

			ApplyApplicator(
			new[] { product1, product2, product3 },
			@"INFO: Updated Dynamic Pick Face Area on Product/Warehouse/Client parameter for P1 to 'Dynamic3'.
INFO: Dynamic Pick Face Area on Product/Warehouse/Client parameter previously set for P2. Value was left unchanged.
INFO: Dynamic Pick Face Area on Product/Warehouse/Client parameter previously set for P3. Value was left unchanged.");

			AssertEquals("Updates Dynamic Pick Face Area if initial value empty", dynamicArea3PK, product1Params.W3_WA_DynamicPickFaceArea);
			AssertEquals("Does not update Dynamic Pick Face Area if initial value not empty", dynamicArea1PK, product2Params.W3_WA_DynamicPickFaceArea);
			AssertEquals("Does not update Dynamic Pick Face Area if initial value not empty", dynamicArea2PK, product3Params.W3_WA_DynamicPickFaceArea);
		}

		#endregion

		#region TestAction_ClearDynamicPickArea

		public void TestAction_ClearDynamicPickArea()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var dynamicArea1PK = ((IWhsArea)Helper.CreateWhsArea(warehouse.PK, "Dynamic1", "DPF")).PK;
			var dynamicArea2PK = ((IWhsArea)Helper.CreateWhsArea(warehouse.PK, "Dynamic2", "DPF")).PK;

			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "P1";

			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product2.OP_PartNum = "P2";

			var product3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product3.OP_PartNum = "P3";

			product1.RelatedOrganisations.AddOwner(client);
			product2.RelatedOrganisations.AddOwner(client);
			product3.RelatedOrganisations.AddOwner(client);

			var product1Params = Helper.CreateProductParamsByWhsAndClient(product1.PK, client.PK, warehouse.PK, 1);
			product1Params.W3_WA_DynamicPickFaceArea = dynamicArea1PK;
			var product2Params = Helper.CreateProductParamsByWhsAndClient(product2.PK, client.PK, warehouse.PK, 1);
			product2Params.W3_WA_DynamicPickFaceArea = dynamicArea1PK;
			var product3Params = Helper.CreateProductParamsByWhsAndClient(product3.PK, client.PK, warehouse.PK, 1);
			product3Params.W3_WA_DynamicPickFaceArea = dynamicArea2PK;

			AssertEquals("Precondition", false, product1Params.W3_WA_DynamicPickFaceArea.IsEmpty);
			AssertEquals("Precondition", false, product2Params.W3_WA_DynamicPickFaceArea.IsEmpty);
			AssertEquals("Precondition", false, product3Params.W3_WA_DynamicPickFaceArea.IsEmpty);

			Applicator.ClientPK = client.PK;
			Applicator.WarehousePK = warehouse.PK;
			Applicator.OverrideNonEmptyDynamicPickArea = false;
			Applicator.DynamicPickAreaPK = ZGuid.Empty;
			AssertEquals("When DynamicPickAreaPK set empty it should change to true.", true, Applicator.OverrideNonEmptyDynamicPickArea);

			ApplyApplicator(
			new[] { product1, product2, product3 },
			@"INFO: Cleared Dynamic Pick Face Area on Product/Warehouse/Client parameter for P1.
INFO: Cleared Dynamic Pick Face Area on Product/Warehouse/Client parameter for P2.
INFO: Cleared Dynamic Pick Face Area on Product/Warehouse/Client parameter for P3.");

			AssertEquals("Clears Dynamic Pick Face Area", true, product1Params.W3_WA_DynamicPickFaceArea.IsEmpty);
			AssertEquals("Clears Dynamic Pick Face Area", true, product2Params.W3_WA_DynamicPickFaceArea.IsEmpty);
			AssertEquals("Clears Dynamic Pick Face Area", true, product3Params.W3_WA_DynamicPickFaceArea.IsEmpty);
		}

		#endregion

		#region TestAction_SetDynamicPickArea_ProductDoesNotHaveClientRelationship

		public void TestAction_SetDynamicPickArea_ProductDoesNotHaveClientRelationship()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);
			var dynamicAreaPK = ((IWhsArea)Helper.CreateWhsArea(warehouse.PK, "Dynamic", "DPF")).PK;

			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "P1";

			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product2.OP_PartNum = "P2";

			var product3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product3.OP_PartNum = "P3";

			Applicator.ClientPK = client.PK;
			Applicator.WarehousePK = warehouse.PK;
			Applicator.DynamicPickAreaPK = dynamicAreaPK;

			ApplyApplicator(
			new[] { product1, product2, product3 },
			@"WARNING: Unable to update Product/Warehouse/Client parameter as Client is not an owner of P1.
WARNING: Unable to update Product/Warehouse/Client parameter as Client is not an owner of P2.
WARNING: Unable to update Product/Warehouse/Client parameter as Client is not an owner of P3.");

			AssertNull("Product Params should not exist.", Helper.GetProductParamsByWhsAndClient(product1.PK, warehouse.PK, client.PK));
			AssertNull("Product Params should not exist.", Helper.GetProductParamsByWhsAndClient(product2.PK, warehouse.PK, client.PK));
			AssertNull("Product Params should not exist.", Helper.GetProductParamsByWhsAndClient(product3.PK, warehouse.PK, client.PK));
		}

		#endregion

		#region TestAction_ClearDynamicPickArea_ProductDoesNotHaveClientWarehouseRelationship

		public void TestAction_ClearDynamicPickArea_ProductDoesNotHaveClientWarehouseRelationship()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);

			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "P1";

			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product2.OP_PartNum = "P2";

			var product3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product3.OP_PartNum = "P3";

			Applicator.ClientPK = client.PK;
			Applicator.WarehousePK = warehouse.PK;
			Applicator.DynamicPickAreaPK = ZGuid.Empty;
			Applicator.OverrideNonEmptyDynamicPickArea = true;

			ApplyApplicator(
			new[] { product1, product2, product3 },
			@"WARNING: Unable to clear Dynamic Pick Face Area on Product/Warehouse/Client parameter as Client is not an owner of P1.
WARNING: Unable to clear Dynamic Pick Face Area on Product/Warehouse/Client parameter as Client is not an owner of P2.
WARNING: Unable to clear Dynamic Pick Face Area on Product/Warehouse/Client parameter as Client is not an owner of P3.");

			AssertNull("Product Params should not exist.", Helper.GetProductParamsByWhsAndClient(product1.PK, warehouse.PK, client.PK));
			AssertNull("Product Params should not exist.", Helper.GetProductParamsByWhsAndClient(product2.PK, warehouse.PK, client.PK));
			AssertNull("Product Params should not exist.", Helper.GetProductParamsByWhsAndClient(product3.PK, warehouse.PK, client.PK));
		}

		#endregion

		#region TestClientPK

		public void TestClientPK()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(UpdateDynamicPickFaceAreaMethodApplicator), "ClientPK", false, la => la.ListDataSourceMember == "Lookups.Clients");
		}

		#endregion

		#region TestWarehousePK

		public void TestWarehousePK()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(UpdateDynamicPickFaceAreaMethodApplicator), "WarehousePK", false, la => la.ListDataSourceMember == "Lookups.Warehouses");
		}

		#endregion

		#region TestDynamicPickAreaPK

		public void TestDynamicPickAreaPK()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(UpdateDynamicPickFaceAreaMethodApplicator), "DynamicPickAreaPK", false, la => la.ListDataSourceMember == "Lookups.DynamicPickAreas");
		}

		#endregion

		#region TestDynamicPickArea

		public void TestDynamicPickArea_SetsOverrideOnZero()
		{
			Applicator.OverrideNonEmptyDynamicPickArea = false;
			Applicator.DynamicPickAreaPK = ZGuid.NewZGuid();
			AssertEquals("Should not modify Override not empty Dynamic Pick Face Area.", false, Applicator.OverrideNonEmptyDynamicPickArea);

			Applicator.DynamicPickAreaPK = ZGuid.Empty;
			AssertEquals("Should modify Override not empty Dynamic Pick Face Area to be true.", true, Applicator.OverrideNonEmptyDynamicPickArea);
		}

		public void TestDynamicPickArea_TriggersOverrideOnZeroValidation()
		{
			Applicator.OverrideNonEmptyDynamicPickArea = false;
			Applicator.DynamicPickAreaPK = ZGuid.NewZGuid();
			AssertNoErrors(Applicator.OverrideNonEmptyDynamicPickAreaInfo);

			Applicator.DynamicPickAreaPK = ZGuid.Empty;
			Applicator.OverrideNonEmptyDynamicPickArea = false;
			AssertHasError(Applicator.OverrideNonEmptyDynamicPickAreaInfo, "Override Empty Dynamic Pick Area must be ticked when clearing Dynamic Pick Area.");

			Applicator.DynamicPickAreaPK = ZGuid.NewZGuid();
			AssertNoErrors(Applicator.OverrideNonEmptyDynamicPickAreaInfo);
		}

		#endregion

		#region TestOverrideNonEmptyDynamicPickArea

		public void TestOverrideNonEmptyDynamicPickArea_DefaultsToTrue()
		{
			AssertEquals("Should default Override not empty Dynamic Pick Face Area to true.", true, Applicator.OverrideNonEmptyDynamicPickArea);
		}

		#endregion

		#region Implementation

		IWhsTransactionTestHelper Helper => helper ?? (helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory));
		IWhsTransactionTestHelper helper;

		protected override BusinessObject GetNewBusinessObject() => new UpdateDynamicPickFaceAreaMethodApplicator("test", Factory);

		new UpdateDynamicPickFaceAreaMethodApplicator Applicator => (UpdateDynamicPickFaceAreaMethodApplicator)base.Applicator;

		#endregion
	}
}
