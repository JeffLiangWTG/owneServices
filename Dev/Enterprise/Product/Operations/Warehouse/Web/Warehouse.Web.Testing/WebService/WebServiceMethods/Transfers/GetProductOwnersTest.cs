using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetProductOwnersTest : WhsSecureServiceTestCase
	{
		#region TestGetProductOwners

		#region TestGetProductOwners

		public void TestGetProductOwners()
		{
			var client1 = Helper.CreateClient("CLIENT1");
			var client2 = Helper.CreateClient("CLIENT2");
			var client3 = Helper.CreateClient("CLIENT3");
			var client4_Inactive = Helper.CreateClient("CLIENT4");
			client4_Inactive.OH_IsActive = false;

			// different products with different related orgs, but same product code.
			Helper.CreateProduct(client1, "P1");
			Helper.CreateProduct(client2, "P1"); // same code as part1, but different client
			var part3_Inactive = Helper.CreateProduct(client3, "P1"); // same code as part 1 and part 2, but inactive product
			part3_Inactive.OP_IsActive = false;

			// same product with multiple related orgs with different relation types.
			var part4 = Helper.CreateProduct(client1, "P2"); // same product with multiple related orgs (1 Owner, 1 Supplier and 1 Both)
			Helper.CreateProductClientRelationShip(client2, part4, OrgPartRelation.RelationshipTypes.Supplier);
			Helper.CreateProductClientRelationShip(client3, part4, OrgPartRelation.RelationshipTypes.Both);
			Helper.CreateProductClientRelationShip(client4_Inactive, part4, OrgPartRelation.RelationshipTypes.Owner);

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			AssertBusinessValidationError(webService1, "Please provide a valid product code or barcode.", webService1.GetProductOwners(""));

			var webService2 = GetNewWebService();
			AssertBusinessValidationError(webService2, "Please provide a valid product code or barcode.", webService2.GetProductOwners("TEST"));

			var webService3 = GetNewWebService();
			var response3 = webService3.GetProductOwners("P1");
			AssertEquals(2, response3.Organisations.Length);
			response3.Organisations.Single(o => o.Code == client1.OH_Code);
			response3.Organisations.Single(o => o.Code == client2.OH_Code);

			var webService4 = GetNewWebService();
			var response4 = webService4.GetProductOwners("P2");
			AssertEquals(2, response4.Organisations.Length);
			response4.Organisations.Single(o => o.Code == client1.OH_Code);
			response4.Organisations.Single(o => o.Code == client3.OH_Code);
		}

		#endregion

		#region TestGetProductOwners_ByBarcode

		public void TestGetProductOwners_ByBarcode()
		{
			var client1 = Helper.CreateClient("CLIENT1");
			var client2 = Helper.CreateClient("CLIENT2");
			var client3 = Helper.CreateClient("CLIENT3");

			// different products with different related orgs, but same product code. Barcode match code of part3.
			Helper.CreateProduct(client1, "P1");
			var part2 = Helper.CreateProduct(client2, "P1");
			Helper.CreateProductBarcode(part2, Core.Constants.PkgUnit.Bag, "P2");

			// same product with multiple related orgs and random barcode.
			var part3 = Helper.CreateProduct(client1, "P2");
			Helper.CreateProductClientRelationShip(client3, part3);
			Helper.CreateProductBarcode(part3, Core.Constants.PkgUnit.Basket, "123");

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			var response1 = webService1.GetProductOwners("P1");
			AssertEquals(2, response1.Organisations.Length);
			response1.Organisations.Single(o => o.Code == client1.OH_Code);
			response1.Organisations.Single(o => o.Code == client2.OH_Code);

			var webService2 = GetNewWebService();
			var response2 = webService2.GetProductOwners("P2");
			AssertEquals(3, response2.Organisations.Length);
			response2.Organisations.Single(o => o.Code == client1.OH_Code);
			response2.Organisations.Single(o => o.Code == client2.OH_Code);
			response2.Organisations.Single(o => o.Code == client3.OH_Code);

			var webService3 = GetNewWebService();
			var response3 = webService3.GetProductOwners("123");
			AssertEquals(2, response3.Organisations.Length);
			response3.Organisations.Single(o => o.Code == client1.OH_Code);
			response3.Organisations.Single(o => o.Code == client3.OH_Code);
		}

		#endregion

		#region TestGetProductOwners_ByPalletId

		public void TestGetProductOwners_ByPalletId()
		{
			var client1 = Helper.CreateClient("CLIENT1");
			var client2 = Helper.CreateClient("CLIENT2");
			var client3 = Helper.CreateClient("CLIENT3");
			var client4 = Helper.CreateClient("CLIENT4");

			var whs = Helper.CreateWarehouse("WHS", "A");

			var part = Helper.CreateProduct(client1, "Part01");
			Helper.CreateProductClientRelationShip(client2, part);
			Helper.CreateProductClientRelationShip(client3, part);

			var part2 = Helper.CreateProduct(client4, "Part02");
			Helper.CreateProductBarcode(part2, Core.Constants.PkgUnit.Bag, "Part01");
			Helper.Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, whs, "R1", part, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, whs, "R2", part, 20m);

			var webService1 = GetNewWebService();
			var response1 = webService1.GetProductOwners("Part01");
			AssertEquals(4, response1.Organisations.Length);
			response1.Organisations.Single(o => o.Code == client1.OH_Code);
			response1.Organisations.Single(o => o.Code == client2.OH_Code);
			response1.Organisations.Single(o => o.Code == client3.OH_Code);
			response1.Organisations.Single(o => o.Code == client4.OH_Code);

			var receiveLine1 = receive1.Lines[0];
			receiveLine1.WE_PalletID = "PalletID";
			Helper.Factory.Save();
			var webService2 = GetNewWebService();
			var response2 = webService2.GetProductOwners("Part01", "PalletID");
			AssertEquals(1, response2.Organisations.Length);
			response2.Organisations.Single(o => o.Code == client1.OH_Code);

			var receiveLine2 = receive2.Lines[0];
			receiveLine2.WE_PalletID = "PalletID";
			Helper.Factory.Save();
			var webService3 = GetNewWebService();
			var response3 = webService3.GetProductOwners("Part01", "PalletID");
			AssertEquals(4, response3.Organisations.Length);
			response3.Organisations.Single(o => o.Code == client1.OH_Code);
			response3.Organisations.Single(o => o.Code == client2.OH_Code);
			response3.Organisations.Single(o => o.Code == client3.OH_Code);
			response3.Organisations.Single(o => o.Code == client4.OH_Code);

			receive1.FinaliseDocketWithoutUserConfirmation();
			receive2.FinaliseDocketWithoutUserConfirmation();

			var order = Helper.CreateWhsOrder(client1, whs);
			Helper.CreateWhsOrderLine(order, part, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Helper.Factory.Save();

			var webService4 = GetNewWebService();
			var response4 = webService3.GetProductOwners("Part01", "PalletID");
			AssertEquals(1, response4.Organisations.Length);
			response4.Organisations.Single(o => o.Code == client2.OH_Code);
		}

		#endregion

		#endregion
	}
}
