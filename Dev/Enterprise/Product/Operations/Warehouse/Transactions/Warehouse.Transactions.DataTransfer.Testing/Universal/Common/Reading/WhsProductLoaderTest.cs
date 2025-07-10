using System;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsProductLoaderTest : WhsUniversalTestCase
	{
		#region TestGetMatchedProductOrMaybeCreateNew

		public void TestGetMatchedProductOrMaybeCreateNew_Create()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var supplier1 = Helper.CreateClient("SUPPLIER1");
			Factory.SaveForTesting();

			var receive = Helper.CreateWhsReceive(client, Data.GetOrCreateWarehouseInDB());
			receive.SupplierDocAddress.E2_OA_Address = supplier1.MainAddress.PK;
			Factory.SaveForTesting();

			var inventoryLineDataObject = new OrderLine
			{
				Product = new Product { Code = "PrdCode" },
				OrderedQtyUnit = new CodeDescriptionPair() { Code = "cas", Description = "CASE" },
				PartAttribute1 = "Green",
				PartAttribute2 = "Large",
				SerialNumber = "SNN",
				ExpiryDate = DateTime.UtcNow
			};

			var result = WhsProductLoader.GetMatchedProductOrMaybeCreateNew("Receipt", 3, "PrdCode", inventoryLineDataObject, client, true, Factory, Logger);

			CombineAssertions(() =>
			{
				AssertMultilineASCIIEquals("Logger.Logs", "Information - Unable to match Product: PrdCode. New Product created.".Trim(), Logger.Logs);

				AssertEquals(nameof(result.OP_PartNum), "PRDCODE", result.OP_PartNum);
				AssertEquals(nameof(result.OP_Desc), "PrdCode", result.OP_Desc);
				AssertEquals(nameof(result.OP_StockKeepingUnit), "CAS", result.OP_StockKeepingUnit);

				AssertEquals("Number of OrgPartRelations", 1, result.RelatedOrganisations.Count);
				var relation = result.RelatedOrganisations[0];
				AssertEquals(nameof(relation.OU_UsePartAttrib1), true, actual: relation.OU_UsePartAttrib1);
				AssertEquals(nameof(relation.OU_UsePartAttrib2), true, actual: relation.OU_UsePartAttrib2);
				AssertEquals(nameof(relation.OU_UsePartAttrib3), false, actual: relation.OU_UsePartAttrib3);
				AssertEquals(nameof(relation.OU_UseSerialNumber), true, actual: relation.OU_UseSerialNumber);
				AssertEquals(nameof(relation.OU_UsePackingDate), false, actual: relation.OU_UsePackingDate);
				AssertEquals(nameof(relation.OU_UseExpiryDate), true, actual: relation.OU_UseExpiryDate);
			});
		}

		#endregion

		#region TestWhenMultipleProductsMatchClient_ShouldNotFallsBackToDocketSupplier

		public void TestWhenMultipleProductsMatchClient_ShouldNotFallsBackToDocketSupplier()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var supplier1 = Helper.CreateClient("SUPPLIER1");
			var supplier2 = Helper.CreateClient("SUPPLIER2");
			var product1 = Helper.CreateProduct(client, "DupCode");
			product1.RelatedOrganisations.AddSupplier(supplier1);

			var product2 = Helper.CreateProduct(client, "DupCode");
			product2.RelatedOrganisations.AddSupplier(supplier2);

			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				Factory.SaveForTesting();
			}

			var receive = Helper.CreateWhsReceive(client, Data.GetOrCreateWarehouseInDB());
			receive.SupplierDocAddress.E2_OA_Address = supplier1.MainAddress.PK;
			Factory.SaveForTesting();

			var inventoryLineDataObject = new OrderLine { Product = new Product { Code = "DupCode" } };

			AssertExceptionThrown("Cannot import OrderLine with mulitple product match.", typeof(DataObjectReadFailureException),
	string.Format(@"Cannot Import Receipt Line 3.
Multiple Product matched: DupCode for Client {0}.", receive.Client.OH_Code), () => WhsProductLoader.GetMatchedProductOrMaybeCreateNew("Receipt", 3, "DupCode", inventoryLineDataObject, client, false, Factory, Logger));
		}

		#endregion

		#region TestExceptionIsThrownWhenMultipleProductCodeIsMatched

		public void TestExceptionIsThrownWhenMultipleProductCodeIsMatched_GetMatchedProductOrMaybeCreateNew()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var whsReceive = WhsReceiveLineDataObjectReaderTest.GetReceiveLineParent(Factory);
				var supplier1 = Factory.New<OrgHeader>();
				supplier1.OH_Code = "SUP1@#$";
				whsReceive.SupplierDocAddress.E2_OA_Address = supplier1.MainAddress.PK;
				Factory.SaveForTesting();

				var inventoryLineDataObject = new OrderLine();
				inventoryLineDataObject.Product = new Product { Code = "BOWLHAT" };
				OrgSupplierPart partReadInto = null;
				AssertNoExceptionThrown("OrderLine reads in fine.", () => partReadInto = WhsProductLoader.GetMatchedProductOrMaybeCreateNew("DUMMY", 3, "BOWLHAT", inventoryLineDataObject, whsReceive.Client, false, Factory, Logger));

				var part = Factory.New<OrgSupplierPart>();
				part.OP_PartNum = "BOWLHAT";
				part.OP_Desc = "Bowler Hat";
				part.OP_StockKeepingUnit = "UNT";
				part.OP_RH_NKCommodityCode = "CMM";

				var supplier2 = Factory.New<OrgHeader>();
				supplier2.OH_Code = "SUP2@#$";
				part.RelatedOrganisations.AddOwner(whsReceive.Client);
				part.RelatedOrganisations.AddSupplier(supplier2);

				using (DuplicateProductTriggerSuspenderForTest.Suspend())
				{
					Factory.SaveForTesting();
				}

				AssertExceptionThrown("Cannot import OrderLine with mulitple product match.", typeof(DataObjectReadFailureException),
					string.Format(@"Cannot Import DUMMY Line 3.
Multiple Product matched: BOWLHAT for Client {0}.", whsReceive.Client.OH_Code, supplier1.OH_Code), () => partReadInto = WhsProductLoader.GetMatchedProductOrMaybeCreateNew("DUMMY", 3, "BOWLHAT", inventoryLineDataObject, whsReceive.Client, false, Factory, Logger));
			}
		}

		#endregion

		#region TestGetMatchedProductOrMaybeCreateNew_ProductCodeMatchWithExistingBarcode

		public void TestGetMatchedProductOrMaybeCreateNew_ProductCodeMatchWithExistingBarcode_InDB()
		{
			TestGetMatchedProductOrMaybeCreateNew_ProductCodeMatchWithExistingBarcode_Code(savedBarcode: true);
		}

		public void TestGetMatchedProductOrMaybeCreateNew_ProductCodeMatchWithExistingBarcode_InMemory()
		{
			TestGetMatchedProductOrMaybeCreateNew_ProductCodeMatchWithExistingBarcode_Code(savedBarcode: false);
		}

		void TestGetMatchedProductOrMaybeCreateNew_ProductCodeMatchWithExistingBarcode_Code(bool savedBarcode)
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var supplier1 = Helper.CreateClient("SUPPLIER1");
			var product = Helper.CreateProduct(client, "P1");
			Factory.SaveForTesting();
			Helper.CreateProductBarcode(product, product.OP_StockKeepingUnit, "A123");
			if (savedBarcode)
			{
				Factory.SaveForTesting();
			}

			AssertEquals("Precondition", "P1", product.OP_PartNum);
			AssertEquals("Precondition", "P1", product.OP_Desc);

			var inventoryLineDataObject = new OrderLine
			{
				Product = new Product { Code = "A123" }
			};

			var result = WhsProductLoader.GetMatchedProductOrMaybeCreateNew("Receipt", 3, "A123", inventoryLineDataObject, client, true, Factory, Logger);

			CombineAssertions(() =>
			{
				AssertMultilineASCIIEquals("Logger.Logs", "", Logger.Logs);

				AssertEquals("Should use same Product", product.PK, result.PK);
				AssertEquals(nameof(result.OP_PartNum), "P1", result.OP_PartNum);
				AssertEquals(nameof(result.OP_Desc), "P1", result.OP_Desc);
			});
		}

		#endregion

		#region TestUnableToMatchProductMessage

		public void TestUnableToMatchProductMessage()
		{
			var inventoryLineDataObject = new OrderLine();
			inventoryLineDataObject.Product = new Product { Code = "BOWLHAT" };
			AssertEquals("Unable to match Product: BOWLHAT.", WhsProductLoader.GetUnableToMatchProductMessage(inventoryLineDataObject));
		}

		#endregion

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);
	}
}
