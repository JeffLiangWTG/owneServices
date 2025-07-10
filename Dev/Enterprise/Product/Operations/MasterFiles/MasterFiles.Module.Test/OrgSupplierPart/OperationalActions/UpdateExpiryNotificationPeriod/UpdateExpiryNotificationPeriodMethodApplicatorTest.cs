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
	[TestedType(typeof(UpdateExpiryNotificationPeriodMethodApplicator))]
	sealed class UpdateExpiryNotificationPeriodMethodApplicatorTest : OperationalActionMethodApplicatorTest
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
			Applicator.ExpiryNotificationPeriod = 5;
			ApplyApplicator(new[] { product1, product2, product3 }, "ERROR: Enter valid arguments.");

			Applicator.ClientPK = client.PK;
			Applicator.WarehousePK = ZGuid.Empty;
			Applicator.ExpiryNotificationPeriod = 5;
			ApplyApplicator(new[] { product1, product2, product3 }, "ERROR: Enter valid arguments.");

			Applicator.ClientPK = client.PK;
			Applicator.WarehousePK = warehouse.PK;
			Applicator.ExpiryNotificationPeriod = -5;
			ApplyApplicator(new[] { product1, product2, product3 }, "ERROR: Enter valid arguments.");
		}

		#endregion

		#region TestAction_SetExpiryNotificationPeriod

		public void TestAction_SetExpiryNotificationPeriod()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);

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
			Applicator.ExpiryNotificationPeriod = 3;
			ApplyApplicator(new[] { product1, product2, product3 },
@"INFO: Updated Expiry Notification Period on Product/Warehouse/Client parameter for P1 to 3 day(s).
INFO: Updated Expiry Notification Period on Product/Warehouse/Client parameter for P2 to 3 day(s).
INFO: Updated Expiry Notification Period on Product/Warehouse/Client parameter for P3 to 3 day(s).");

			AssertEquals("Updates Expiry Notification Period", 3, product1Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Updates Expiry Notification Period", 3, product2Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Updates Expiry Notification Period", 3, product3Params.W3_ExpiryNotificationPeriod);
		}

		#endregion

		#region TestAction_CreateNewRelationshipForExistingClients

		public void TestAction_CreateNewRelationshipForExistingClients()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);

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
			Applicator.ExpiryNotificationPeriod = 3;
			ApplyApplicator(new[] { product1, product2, product3 },
@"INFO: Created new Product/Warehouse/Client parameter for P1 and assigned Expiry Notification Period to 3 day(s).
INFO: Updated Expiry Notification Period on Product/Warehouse/Client parameter for P2 to 3 day(s).
INFO: Created new Product/Warehouse/Client parameter for P3 and assigned Expiry Notification Period to 3 day(s).");

			var product1Params = Helper.GetProductParamsByWhsAndClient(product1.PK, warehouse.PK, client.PK);
			var product3Params = Helper.GetProductParamsByWhsAndClient(product3.PK, warehouse.PK, client.PK);

			AssertEquals("Updates Expiry Notification Period", 3, product1Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Updates Expiry Notification Period", 3, product2Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Updates Expiry Notification Period", 3, product3Params.W3_ExpiryNotificationPeriod);
		}

		#endregion

		#region TestAction_UpdatesExpiryNotificationPeriod

		public void TestAction_UpdatesExpiryNotificationPeriod_WhenUsingOverrideExistingNonZeroValues()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);

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
			product1Params.W3_ExpiryNotificationPeriod = 0;
			var product2Params = Helper.CreateProductParamsByWhsAndClient(product2.PK, client.PK, warehouse.PK, 1);
			product2Params.W3_ExpiryNotificationPeriod = 5;
			var product3Params = Helper.CreateProductParamsByWhsAndClient(product3.PK, client.PK, warehouse.PK, 1);
			product3Params.W3_ExpiryNotificationPeriod = 10;

			AssertEquals("Precondition", 0, product1Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Precondition", 5, product2Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Precondition", 10, product3Params.W3_ExpiryNotificationPeriod);

			Applicator.ClientPK = client.PK;
			Applicator.WarehousePK = warehouse.PK;
			Applicator.ExpiryNotificationPeriod = 3;
			Applicator.OverrideNonZeroExpiryNotificationPeriod = true;

			ApplyApplicator(new[] { product1, product2, product3 },
@"INFO: Updated Expiry Notification Period on Product/Warehouse/Client parameter for P1 to 3 day(s).
INFO: Updated Expiry Notification Period on Product/Warehouse/Client parameter for P2 to 3 day(s).
INFO: Updated Expiry Notification Period on Product/Warehouse/Client parameter for P3 to 3 day(s).");

			AssertEquals("Updates Expiry Notification Period", 3, product1Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Updates Expiry Notification Period", 3, product2Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Updates Expiry Notification Period", 3, product3Params.W3_ExpiryNotificationPeriod);
		}

		#endregion

		#region TestAction_DoesNotUpdateExpiryNotificationPeriod_WhenNotUsingOverrideExistingNonZeroValues

		public void TestAction_DoesNotUpdateExpiryNotificationPeriod_WhenNotUsingOverrideExistingNonZeroValues()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);

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
			product1Params.W3_ExpiryNotificationPeriod = 0;
			var product2Params = Helper.CreateProductParamsByWhsAndClient(product2.PK, client.PK, warehouse.PK, 1);
			product2Params.W3_ExpiryNotificationPeriod = 5;
			var product3Params = Helper.CreateProductParamsByWhsAndClient(product3.PK, client.PK, warehouse.PK, 1);
			product3Params.W3_ExpiryNotificationPeriod = 10;

			AssertEquals("Precondition", 0, product1Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Precondition", 5, product2Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Precondition", 10, product3Params.W3_ExpiryNotificationPeriod);

			Applicator.ClientPK = client.PK;
			Applicator.WarehousePK = warehouse.PK;
			Applicator.ExpiryNotificationPeriod = 3;
			Applicator.OverrideNonZeroExpiryNotificationPeriod = false;

			ApplyApplicator(new[] { product1, product2, product3 },
@"INFO: Updated Expiry Notification Period on Product/Warehouse/Client parameter for P1 to 3 day(s).
INFO: Expiry Notification Period on Product/Warehouse/Client parameter previously set for P2. Value was left unchanged.
INFO: Expiry Notification Period on Product/Warehouse/Client parameter previously set for P3. Value was left unchanged.");

			AssertEquals("Updates Expiry Notification Period if initial value zero", 3, product1Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Does not update Expiry Notification Period if initial value non-zero", 5, product2Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Does not update Expiry Notification Period if initial value non-zero", 10, product3Params.W3_ExpiryNotificationPeriod);
		}

		#endregion

		#region TestAction_ClearExpiryNotificationPeriod

		public void TestAction_ClearExpiryNotificationPeriod()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("WHS", "A", 3, 1);

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
			product1Params.W3_ExpiryNotificationPeriod = 3;
			var product2Params = Helper.CreateProductParamsByWhsAndClient(product2.PK, client.PK, warehouse.PK, 1);
			product2Params.W3_ExpiryNotificationPeriod = 5;
			var product3Params = Helper.CreateProductParamsByWhsAndClient(product3.PK, client.PK, warehouse.PK, 1);
			product3Params.W3_ExpiryNotificationPeriod = 10;

			AssertEquals("Precondition", 3, product1Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Precondition", 5, product2Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Precondition", 10, product3Params.W3_ExpiryNotificationPeriod);

			Applicator.ClientPK = client.PK;
			Applicator.WarehousePK = warehouse.PK;
			Applicator.ExpiryNotificationPeriod = 0;

			ApplyApplicator(new[] { product1, product2, product3 },
@"INFO: Cleared Expiry Notification Period on Product/Warehouse/Client parameter for P1.
INFO: Cleared Expiry Notification Period on Product/Warehouse/Client parameter for P2.
INFO: Cleared Expiry Notification Period on Product/Warehouse/Client parameter for P3.");

			AssertEquals("Clears Expiry Notification Period", 0, product1Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Clears Expiry Notification Period", 0, product2Params.W3_ExpiryNotificationPeriod);
			AssertEquals("Clears Expiry Notification Period", 0, product3Params.W3_ExpiryNotificationPeriod);
		}

		#endregion

		#region TestAction_SetExpiryNotificationPeriod_ProductDoesNotHaveClientRelationship

		public void TestAction_SetExpiryNotificationPeriod_ProductDoesNotHaveClientRelationship()
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
			Applicator.ExpiryNotificationPeriod = 3;

			ApplyApplicator(new[] { product1, product2, product3 },
@"WARNING: Unable to update Product/Warehouse/Client parameter as Client is not an owner of P1.
WARNING: Unable to update Product/Warehouse/Client parameter as Client is not an owner of P2.
WARNING: Unable to update Product/Warehouse/Client parameter as Client is not an owner of P3.");

			AssertNull("Product Params should not exist.", Helper.GetProductParamsByWhsAndClient(product1.PK, warehouse.PK, client.PK));
			AssertNull("Product Params should not exist.", Helper.GetProductParamsByWhsAndClient(product2.PK, warehouse.PK, client.PK));
			AssertNull("Product Params should not exist.", Helper.GetProductParamsByWhsAndClient(product3.PK, warehouse.PK, client.PK));
		}

		#endregion

		#region TestAction_ClearExpiryNotificationPeriod_ProductDoesNotHaveClientWarehouseRelationship

		public void TestAction_ClearExpiryNotificationPeriod_ProductDoesNotHaveClientWarehouseRelationship()
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
			Applicator.ExpiryNotificationPeriod = 0;

			ApplyApplicator(new[] { product1, product2, product3 },
@"WARNING: Unable to clear Expiry Notification Period on Product/Warehouse/Client parameter as Client is not an owner of P1.
WARNING: Unable to clear Expiry Notification Period on Product/Warehouse/Client parameter as Client is not an owner of P2.
WARNING: Unable to clear Expiry Notification Period on Product/Warehouse/Client parameter as Client is not an owner of P3.");

			AssertNull("Product Params should not exist.", Helper.GetProductParamsByWhsAndClient(product1.PK, warehouse.PK, client.PK));
			AssertNull("Product Params should not exist.", Helper.GetProductParamsByWhsAndClient(product2.PK, warehouse.PK, client.PK));
			AssertNull("Product Params should not exist.", Helper.GetProductParamsByWhsAndClient(product3.PK, warehouse.PK, client.PK));
		}

		#endregion

		#region TestClientPK

		public void TestClientPK()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(UpdateExpiryNotificationPeriodMethodApplicator), "ClientPK", false,
				la => la.ListDataSourceMember == "Lookups.Clients");
		}

		public void TestClientPK_SetDefaultExpiryNotificationPeriodOnChange()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.MiscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 3;

			Applicator.ClientPK = client.PK;
			AssertEquals("Should set Expiry Notification Period to client default.", 3, Applicator.ExpiryNotificationPeriod);
		}

		public void TestClientPK_DoNotSetDefaultExpiryNotificationPeriodOnChange_WhenPreviouslySet()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.MiscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 3;

			Applicator.ExpiryNotificationPeriod = 10;

			Applicator.ClientPK = client.PK;
			AssertEquals("Should not set Expiry Notification Period to client default when value was previously set.", 10, Applicator.ExpiryNotificationPeriod);
		}

		#endregion

		#region TestWarehousePK

		public void TestWarehousePK()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(UpdateExpiryNotificationPeriodMethodApplicator), "WarehousePK", false,
				la => la.ListDataSourceMember == "Lookups.Warehouses");
		}

		#endregion

		#region TestExpiryNotificationPeriod

		public void TestExpiryNotificationPeriod_SetsOverrideOnZero()
		{
			Applicator.OverrideNonZeroExpiryNotificationPeriod = false;
			Applicator.ExpiryNotificationPeriod = 5;
			AssertEquals("Should not modify Override Non-Zero Expiry Notification Period.", false, Applicator.OverrideNonZeroExpiryNotificationPeriod);

			Applicator.ExpiryNotificationPeriod = 0;
			AssertEquals("Should modify Override Non-Zero Expiry Notification Period to be true.", true, Applicator.OverrideNonZeroExpiryNotificationPeriod);
		}

		public void TestExpiryNotificationPeriod_TriggersOverrideOnZeroValidation()
		{
			Applicator.OverrideNonZeroExpiryNotificationPeriod = false;
			Applicator.ExpiryNotificationPeriod = 5;
			AssertNoErrors(Applicator.OverrideNonZeroExpiryNotificationPeriodInfo);

			Applicator.ExpiryNotificationPeriod = 0;
			Applicator.OverrideNonZeroExpiryNotificationPeriod = false;
			AssertHasError(Applicator.OverrideNonZeroExpiryNotificationPeriodInfo, "Override Non-Zero Expiry Notification Period must be true when clearing Expiry Notification Periods.");

			Applicator.ExpiryNotificationPeriod = 5;
			AssertNoErrors(Applicator.OverrideNonZeroExpiryNotificationPeriodInfo);
		}

		#endregion

		#region TestOverrideNonZeroExpiryNotificationPeriod

		public void TestOverrideNonZeroExpiryNotificationPeriod_DefaultsToTrue()
		{
			AssertEquals("Should default Override Non-Zero Expiry Notification Period to true.", true, Applicator.OverrideNonZeroExpiryNotificationPeriod);
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
			return new UpdateExpiryNotificationPeriodMethodApplicator("test", Factory);
		}

		#endregion

		new UpdateExpiryNotificationPeriodMethodApplicator Applicator => (UpdateExpiryNotificationPeriodMethodApplicator)base.Applicator;
	}
}
