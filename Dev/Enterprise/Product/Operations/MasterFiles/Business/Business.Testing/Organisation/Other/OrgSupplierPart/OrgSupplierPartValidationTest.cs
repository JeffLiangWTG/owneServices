using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgSupplierPartValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckOP_IsActive

		public void TestCheckOP_IsActive()
		{
			AssertCheckOP_IsActive();
		}

		public void TestCheckOP_IsActive_HasAsnLineOnUnfinalisedReceive()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("Org1");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			AssertNoErrors("Precondition.", part.OP_IsActiveInfo);
			var whs = helper.CreateWarehouse("1", "A");
			Factory.Save();

			var receive = helper.CreateWhsReceive(client, whs.PK, "R1", null);
			helper.CreateWhsReceiveInventoryLine(receive, part.PK, 0m, "A");
			helper.CreateAsnLine(receive, part.PK, 0m);
			Factory.Save();

			part.OP_IsActive = false;
			AssertHasError("Has error when there is ASN Line on unfinalised Receive is referencing the product.", part.OP_IsActiveInfo, OrgSupplierPartValidation.CannotDeactivateAProductWithAsnLineOnUnfinalisedReceiveError);

			part.OP_IsActive = true;
			AssertNoError("No errors.", part.OP_IsActiveInfo, OrgSupplierPartValidation.CannotDeactivateAProductWithAsnLineOnUnfinalisedReceiveError);

			helper.FinaliseDocketWithoutUserConfirmation(receive);

			Factory.Save();

			part.OP_IsActive = false;
			AssertNoError("No errors.", part.OP_IsActiveInfo, OrgSupplierPartValidation.CannotDeactivateAProductWithAsnLineOnUnfinalisedReceiveError);

			part.OP_IsActive = true;
			AssertNoError("No errors.", part.OP_IsActiveInfo, OrgSupplierPartValidation.CannotDeactivateAProductWithAsnLineOnUnfinalisedReceiveError);
		}

		public void TestCheckOP_IsActive_WithInTransitQty()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("Org1");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			AssertNoErrors("Precondition.", part.OP_IsActiveInfo);
			var whsPK = helper.CreateWarehouse("1", "A").PK;
			Factory.Save();

			// Add some stock on hand
			var receiveLine = (IWhsDocketLine)helper.CreateStock(whsPK, client, part.PK, 10m);
			Factory.Save();

			part.OP_IsActive = false;
			AssertHasError("Precondition: Has error when there is stock on hand.", part.OP_IsActiveInfo, OrgSupplierPartValidation.CannotDeactivateAProductWithSOHError);

			part.OP_IsActive = true;
			AssertNoError("Precondition: No errors.", part.OP_IsActiveInfo, OrgSupplierPartValidation.CannotDeactivateAProductWithSOHError);

			var orderPK = helper.CreateWhsOrder(client, whsPK, "O1", null);
			var orderLinePK = helper.CreateWhsOrderLine(orderPK, part.PK, 10);
			var pickPK = helper.CreateWhsPick(new[] { orderPK });

			var pickLine = helper.GetPickLines(pickPK).Single();
			AssertEquals("Precondition: Stock On Hand exists.", 10m, receiveLine.WE_StockOnHand);
			helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Now);
			AssertEquals("Precondition: Stock On Hand reduced.", 0m, receiveLine.WE_StockOnHand);
			Factory.Save();

			part.OP_IsActive = false;
			AssertHasError("Has error when there is In-Transit stock.", part.OP_IsActiveInfo, OrgSupplierPartValidation.CannotDeactivateAProductWithSOHError);

			part.OP_IsActive = true;
			AssertNoError("Should be no errors.", part.OP_IsActiveInfo, OrgSupplierPartValidation.CannotDeactivateAProductWithSOHError);

			helper.FinaliseDocket(orderPK);
			helper.FinalisePick(pickPK);
			Factory.Save();

			part.OP_IsActive = false;
			AssertNoError("Should be no errors.", part.OP_IsActiveInfo, OrgSupplierPartValidation.CannotDeactivateAProductWithSOHError);
		}

		public void TestCheckOP_IsActive_WithDuplicatedBarcode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("Org1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			var barcode11 = part1.PartBarcodes.AddNew();
			barcode11.PH_Barcode = "123";
			var barcode12 = part1.PartBarcodes.AddNew();
			barcode12.PH_Barcode = "456";
			part1.OP_IsActive = false;
			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "P2");
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "123";
			var part3 = (OrgSupplierPart)helper.CreateProduct(client, "P3");
			var barcode3 = part3.PartBarcodes.AddNew();
			barcode3.PH_Barcode = "456";

			AssertNoErrors("Precondition: IsActive has no error.", part1.OP_IsActiveInfo);
			part1.OP_IsActive = true;
			AssertHasError(part1.OP_IsActiveInfo, string.Format(Culture.Invariant, OrgSupplierPartValidation.CannotActivateAProductWithDuplicatedBarcode, "P2, P3"));
		}

		public void TestCheckOP_IsActive_ProductCodeEqualToBarcode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("Org1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "TestCode");
			var barcode11 = part1.PartBarcodes.AddNew();
			barcode11.PH_Barcode = "TestCode";
			part1.OP_IsActive = false;

			AssertNoErrors("Precondition: IsActive has no error.", part1.OP_IsActiveInfo);

			part1.OP_IsActive = true;
			AssertHasError(part1.OP_IsActiveInfo, string.Format(Culture.Invariant, OrgSupplierPartValidation.CannotActivateAProductWithDuplicatedBarcode, "TESTCODE"));
		}

		public void TestCheckOP_IsActive_ProductCodeEqualToBarcode_TwoProducts()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("Org1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			var barcode11 = part1.PartBarcodes.AddNew();
			barcode11.PH_Barcode = "TestCode";
			part1.OP_IsActive = false;

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "Part2");
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "P1";

			AssertNoErrors("Precondition: IsActive has no error.", part1.OP_IsActiveInfo);

			part1.OP_IsActive = true;
			AssertHasError(part1.OP_IsActiveInfo, string.Format(Culture.Invariant, OrgSupplierPartValidation.CannotActivateAProductWithDuplicatedBarcode, "PART2"));
		}

		public void TestCheckOP_IsActive_ProductCodeEqualToBarcode_ProductHasNoBarcode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("Org1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			part1.OP_IsActive = false;

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "Part2");
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "P1";

			AssertNoErrors("Precondition: IsActive has no error.", part1.OP_IsActiveInfo);

			part1.OP_IsActive = true;
			AssertHasError(part1.OP_IsActiveInfo, string.Format(Culture.Invariant, OrgSupplierPartValidation.CannotActivateAProductWithDuplicatedBarcode, "PART2"));
		}

		public void TestCheckOP_IsActive_BarcodeIsOtherProductCode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("Org1");
			helper.CreateProduct(client, "P1");
			Factory.Save();

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "Part2");
			part2.OP_IsActive = false;
			Factory.Save();

			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "P1";

			AssertNoErrors("Precondition: IsActive has no error.", part2.OP_IsActiveInfo);

			part2.OP_IsActive = true;
			AssertHasError(part2.OP_IsActiveInfo, string.Format(Culture.Invariant, OrgSupplierPartValidation.CannotActivateAProductWithDuplicatedBarcode, "P1"));
		}

		protected void AssertCheckOP_IsActive()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = org.PK;
			relation.OU_OP = part.PK;
			Factory.Save();

			part.OP_IsActive = false;
			AssertNoErrors(part.OP_IsActiveInfo);
			part.OP_IsActive = true;
			AssertNoErrors(part.OP_IsActiveInfo);

			// add some stock on hand
			var whsPK = helper.CreateWarehouse("1", "A").PK;
			helper.CreateStock(whsPK, org.PK, part.PK, 10m);
			Factory.Save();

			part.OP_IsActive = false;
			AssertHasError(part.OP_IsActiveInfo, OrgSupplierPartValidation.CannotDeactivateAProductWithSOHError);
		}

		public void TestCheckOP_IsActive_WithActiveTransaction()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("Org1");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			AssertNoErrors("Precondition.", part.OP_IsActiveInfo);

			var whsPK = helper.CreateWarehouse("IPW", "A").PK;
			var warehouseMainAddressPK = OrgSupplierPartTestHelper.GetWarehouseMainAddressPK(Factory, whsPK);
			var importerPK = OrgSupplierPartTestHelper.GetImporterPK(Factory);
			var transactionPK = OrgSupplierPartTestHelper.GetNewTransactionPK(TestConnection, part, warehouseMainAddressPK, importerPK);
			OrgSupplierPartTestHelper.UpdateTransactionStatus(TestConnection, transactionPK, "VAL");

			var receiveLine = (IWhsDocketLine)helper.CreateStock(whsPK, client, part.PK, 10m);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				using (ObjectFactory.Get<Enterprise.Integration.Customs.ZA.IZACustomsRegistry>().WarehouseOperatorTransactionsModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					part.OP_IsActive = false;
					AssertHasError("Has error when transaction is active.", part.OP_IsActiveInfo, OrgSupplierPartValidation.CannotDeactivateAProductWithActiveTransactionAndSOHError);
				}

				part.OP_IsActive = true;
				AssertNoError("Revert OP_IsActive should not produce error.", part.OP_IsActiveInfo, OrgSupplierPartValidation.CannotDeactivateAProductWithActiveTransactionAndSOHError);

				using (ObjectFactory.Get<Enterprise.Integration.Customs.ZA.IZACustomsRegistry>().WarehouseOperatorTransactionsModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					part.OP_IsActive = false;
					AssertNoError("No error transaction is active but Whs Operator Trans disabled.", part.OP_IsActiveInfo, OrgSupplierPartValidation.CannotDeactivateAProductWithActiveTransactionAndSOHError);
				}
			}
		}

		#endregion

		public void TestMeaningfulCombinationsOfSkuWeightAndVolumeEtc()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PartNum";
			product.OP_Cubic = 2;
			product.OP_Weight = 2;
			product.OP_StockKeepingUnit = "KG";
			AssertNoError(product.OP_WeightUQInfo, "LB per KG is not meaningful because they measure the same dimension.");
			AssertHasError(product.OP_WeightUQInfo, "This unit is the same as the Stock Keeping Unit, therefore the value can only be 1 or 0.");
			product.OP_WeightUQ = "LB";
			AssertHasError(product.OP_WeightUQInfo, "LB per KG is not meaningful because they measure the same dimension.");

			product.OP_StockKeepingUnit = "M3";
			AssertHasError(product.OP_CubicUQInfo, "This unit is the same as the Stock Keeping Unit, therefore the value can only be 1 or 0.");
			product.OP_StockKeepingUnit = "CF";
			AssertHasError(product.OP_CubicUQInfo, "M3 per CF is not meaningful because they measure the same dimension.");
			product.OP_StockKeepingUnit = "LB";
			AssertNoError(product.OP_CubicUQInfo, "M3 per CF is not meaningful because they measure the same dimension.");

			product.OP_Cubic = 0;
			product.OP_Weight = 0;
			product.OP_WeightUQ = "KG";
			product.OP_CubicUQ = "M3";
			product.OP_StockKeepingUnit = "M3";
			AssertNoError(product.OP_CubicUQInfo, "This unit is the same as the Stock Keeping Unit, therefore the value can only be 1 or 0.");
			product.OP_StockKeepingUnit = "KG";
			AssertNoError(product.OP_WeightUQInfo, "This unit is the same as the Stock Keeping Unit, therefore the value can only be 1 or 0.");

			product.OP_Weight = 1;
			product.OP_WeightUQ = "KG";
			product.OP_StockKeepingUnit = "KG";
			AssertNoError(product.OP_WeightUQInfo, "This unit is the same as the Stock Keeping Unit, therefore the value can only be 1 or 0.");
			product.OP_Weight = 0;
			AssertNoError(product.OP_WeightUQInfo, "This unit is the same as the Stock Keeping Unit, therefore the value can only be 1 or 0.");
			product.OP_Weight = 2;
			AssertHasError(product.OP_WeightUQInfo, "This unit is the same as the Stock Keeping Unit, therefore the value can only be 1 or 0.");

			product.OP_Cubic = 1;
			product.OP_CubicUQ = "M3";
			product.OP_StockKeepingUnit = "M3";
			AssertNoError(product.OP_CubicUQInfo, "This unit is the same as the Stock Keeping Unit, therefore the value can only be 1 or 0.");
			product.OP_Cubic = 0;
			AssertNoError(product.OP_CubicUQInfo, "This unit is the same as the Stock Keeping Unit, therefore the value can only be 1 or 0.");
			product.OP_Cubic = 2;
			AssertHasError(product.OP_CubicUQInfo, "This unit is the same as the Stock Keeping Unit, therefore the value can only be 1 or 0.");
		}

		#region TestCheckOP_Width

		public void TestCheckOP_Width()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_Width = -1.09;
			AssertHasError(product.OP_WidthInfo, "Width cannot be negative.");

			product.OP_Width = 1.27;
			AssertNoError(product.OP_WidthInfo, "Width cannot be negative.");

			product.OP_Width = 0;
			AssertNoError(product.OP_WidthInfo, "Width cannot be negative.");
		}

		#endregion

		#region TestCheckOP_IsComponentPickedOnSalesOrder

		public void TestCheckOP_IsComponentPickedOnSalesOrder()
		{
			var mainProduct = CreateProduct("MP1", Constants.PkgUnit.Unit);
			var bomComponentProduct1 = CreateProduct("BOM1", Constants.PkgUnit.Unit);
			var bomComponentProduct2 = CreateProduct("BOM2", Constants.PkgUnit.Unit);

			var bomPartForMainProduct = CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Unit);
			AssertEquals("1 BillofMaterials should found.", 1, mainProduct.BillOfMaterials.Count);
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			AssertNoError(mainProduct.OP_IsComponentPickedOnSalesOrderInfo, mainProduct.Validation.CannotSetPickOnSalesOrderIfBOMHasMultiLevelBOMComponent);
			Factory.Save();

			var bomPartForBomProduct = CreateProductBOM(bomComponentProduct1, bomComponentProduct2, 1m, Constants.PkgUnit.Unit);
			AssertEquals("1 BillofMaterials should found.", 1, bomComponentProduct1.BillOfMaterials.Count);
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			AssertHasError(mainProduct.OP_IsComponentPickedOnSalesOrderInfo, mainProduct.Validation.CannotSetPickOnSalesOrderIfBOMHasMultiLevelBOMComponent);
		}

		public void TestCheckOP_IsPickOnOrderAfterAddSecondLevelBOM()
		{
			var mainProduct = CreateProduct("MP1", Constants.PkgUnit.Unit);
			var bomComponentProduct1 = CreateProduct("BOM1", Constants.PkgUnit.Unit);
			var bomComponentProduct2 = CreateProduct("BOM2", Constants.PkgUnit.Unit);

			var bomPartForMainProduct = CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Unit);
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var bomComponentProduct1AnotherFactory = anotherFactory.Load<OrgSupplierPart>(bomComponentProduct1.PK);
			var bomComponentProduct2AnotherFactory = anotherFactory.Load<OrgSupplierPart>(bomComponentProduct2.PK);
			var bomPartForBomProductAnotherFactory = CreateProductBOM(bomComponentProduct1AnotherFactory, bomComponentProduct2AnotherFactory, 1m, Constants.PkgUnit.Unit);
			anotherFactory.Save();

			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			AssertHasError(mainProduct.OP_IsComponentPickedOnSalesOrderInfo, mainProduct.Validation.CannotSetPickOnSalesOrderIfBOMHasMultiLevelBOMComponent);
		}

		#endregion

		#region TestCheckOP_Height

		public void TestCheckOP_Height()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_Height = -1.09;
			AssertHasError(product.OP_HeightInfo, "Height cannot be negative.");

			product.OP_Height = 1.27;
			AssertNoError(product.OP_HeightInfo, "Height cannot be negative.");

			product.OP_Height = 0;
			AssertNoError(product.OP_HeightInfo, "Height cannot be negative.");
		}

		#endregion

		#region TestCheckOP_Depth

		public void TestCheckOP_Depth()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_Depth = -1.09;
			AssertHasError(product.OP_DepthInfo, "Depth cannot be negative.");

			product.OP_Depth = 1.27;
			AssertNoError(product.OP_DepthInfo, "Depth cannot be negative.");

			product.OP_Depth = 0;
			AssertNoError(product.OP_DepthInfo, "Depth cannot be negative.");
		}

		#endregion

		#region TestCheckOP_Weight

		public void TestCheckOP_Weight()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_Weight = -1.09;
			AssertHasError(product.OP_WeightInfo, "Weight cannot be negative.");

			product.OP_Weight = 1.27;
			AssertNoError(product.OP_WeightInfo, "Weight cannot be negative.");

			product.OP_Weight = 0;
			AssertNoError(product.OP_WeightInfo, "Weight cannot be negative.");
		}

		#endregion

		#region TestCheckOP_NetWeight

		public void TestCheckOP_NetWeight()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_NetWeight = -1.09;
			AssertHasError(product.OP_NetWeightInfo, "Net Weight cannot be negative.");

			product.OP_NetWeight = 1.27;
			AssertNoError(product.OP_NetWeightInfo, "Net Weight cannot be negative.");

			product.OP_NetWeight = 0;
			AssertNoError(product.OP_NetWeightInfo, "Net Weight cannot be negative.");
		}

		#endregion

		#region TestOrganisationAndUnitOfQuantityValidationIsSuspendedForBulkTariffUpdate

		public void TestOrganisationAndUnitOfQuantityValidationIsSuspendedForBulkTariffUpdate()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = ZString.Empty;
			part.RunPreSaveValidation();
			AssertHasError(part.OP_PartNumInfo, OrgSupplierPartValidation.ErrorPartMustBeLinkedToOrganisation);
			AssertHasErrors(part.OP_StockKeepingUnitInfo);

			part = Factory.New<OrgSupplierPart>();
			part.RemoveNonEssentialValidationForBulkTariffUpdate = true;
			part.RunPreSaveValidation();
			AssertNoError(part.OP_PartNumInfo, OrgSupplierPartValidation.ErrorPartMustBeLinkedToOrganisation);
			AssertNoErrors(part.OP_StockKeepingUnitInfo);
		}

		#endregion

		#region TestValidationForNotHavingARelatedOrganisation

		public void TestValidationForNotHavingARelatedOrganisation()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			AssertNoError(part.OP_PartNumInfo, OrgSupplierPartValidation.ErrorPartMustBeLinkedToOrganisation);
			part.RunPreSaveValidation();
			AssertHasError(part.OP_PartNumInfo, OrgSupplierPartValidation.ErrorPartMustBeLinkedToOrganisation);
			OrgHeader supplier = OrgHeader.New(Factory);
			OrgPartRelation relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation.OU_OH = supplier.PK;
			AssertNoError(part.OP_PartNumInfo, OrgSupplierPartValidation.ErrorPartMustBeLinkedToOrganisation);
			part.RelatedOrganisations.RemoveAll();
			AssertHasError(part.OP_PartNumInfo, OrgSupplierPartValidation.ErrorPartMustBeLinkedToOrganisation);
		}

		#endregion

		#region TestValidateOP_CountDecimalPlaces

		public void TestValidateOP_CountDecimalPlaces()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_CountDecimalPlaces = 10;
			AssertHasErrors(product.OP_CountDecimalPlacesInfo);
			product.OP_CountDecimalPlaces = 9;
			AssertNoErrors(product.OP_CountDecimalPlacesInfo);
			product.OP_CountDecimalPlaces = 20;
			AssertHasErrors(product.OP_CountDecimalPlacesInfo);
			product.OP_CountDecimalPlaces = 0;
			AssertNoErrors(product.OP_CountDecimalPlacesInfo);
			product.OP_CountDecimalPlaces = 5;
			AssertNoErrors(product.OP_CountDecimalPlacesInfo);
		}

		#endregion

		#region TestCheckOP_RX_NKLastWeightedCostCurr

		public void TestCheckOP_RX_NKLastWeightedCostCurr()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_RX_NKLastWeightedCostCurr = "";
			AssertNoErrors(product.OP_RX_NKLastWeightedCostCurrInfo);

			product.OP_RX_NKLastWeightedCostCurr = "!@#";
			AssertHasError(product.OP_RX_NKLastWeightedCostCurrInfo, "Enter a valid " + product.OP_RX_NKLastWeightedCostCurrInfo.Description + ".");

			product.OP_RX_NKLastWeightedCostCurr = Core.Constants.CurrencyCodes.Australia;
			AssertNoErrors(product.OP_RX_NKLastWeightedCostCurrInfo);
		}

		#endregion

		#region TestCheckOP_RH_NKCommodityCode

		public void TestCheckOP_RH_NKCommodityCode()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_RH_NKCommodityCode = "";
			product.OP_PartNum = "PartNum";
			AssertNoErrors(product.OP_RH_NKCommodityCodeInfo);

			product.OP_RH_NKCommodityCode = "A";
			AssertHasError(product.OP_RH_NKCommodityCodeInfo, "Enter a valid " + product.OP_RH_NKCommodityCodeInfo.Description + ".");

			RefCommodityCode commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "C";
			Factory.Save();

			product.OP_RH_NKCommodityCode = "C";
			AssertNoErrors(product.OP_RH_NKCommodityCodeInfo);
		}

		#endregion

		#region TestCheckOP_StockKeepingUnitPerPallet

		public void TestCheckOP_StockKeepingUnitPerPallet()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			ZDecimal k = product.OP_StockKeepingUnitPerPallet;
			AssertEquals("Precondition", 0m, k);
			AssertHasWarning(product.OP_StockKeepingUnitPerPalletInfo, OrgSupplierPartValidation.NoPalletSizeWarningMsg);

			OrgPartUnit partUnit = product.PartUnits.AddNew();
			partUnit.OF_PackType = product.OP_StockKeepingUnit;
			partUnit.OF_ParentPackType = "PLT";
			partUnit.OF_QuantityInParent = 10;

			k = product.OP_StockKeepingUnitPerPallet;
			AssertEquals("Precondition", 10m, k);
			AssertNoWarnings(product.OP_StockKeepingUnitPerPalletInfo);
		}

		#endregion

		#region TestCheckOP_PartNum

		#region TestCheckOP_PartNum_ProductCodeSameWithBarcode

		public void TestCheckOP_PartNum_ProductCodeSameWithBarcode_ProductIsActive()
		{
			TestCheckOP_PartNum_ProductCodeSameWithBarcodeCore(true, true);
		}

		public void TestCheckOP_PartNum_ProductCodeSameWithBarcode_ProductIsInactive()
		{
			TestCheckOP_PartNum_ProductCodeSameWithBarcodeCore(false, false);
		}

		void TestCheckOP_PartNum_ProductCodeSameWithBarcodeCore(bool active, bool expectedHasError)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("Org1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			part1.OP_IsActive = active;
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";

			AssertNoErrors(part1.OP_PartNumInfo);

			part1.OP_PartNum = "TestCode";
			if (expectedHasError)
			{
				AssertHasError(part1.OP_PartNumInfo, "Product code cannot be the same to the barcode.");
			}
			else
			{
				AssertNoErrors("Product is in-active, should not have error", part1.OP_PartNumInfo);
			}
		}

		#endregion

		#region TestCheckOP_PartNum_ProductCodeSameWithBarcode_TwoProducts

		public void TestCheckOP_PartNum_ProductCodeSameWithBarcode_TwoProducts_Product1IsActiveAndProduct2IsActive()
		{
			TestCheckOP_PartNum_ProductCodeSameWithBarcode_TwoProductsCore(true, true, expectedHasError: true);
		}

		public void TestCheckOP_PartNum_ProductCodeSameWithBarcode_TwoProducts_Product1IsInactiveAndProduct2IsActive()
		{
			TestCheckOP_PartNum_ProductCodeSameWithBarcode_TwoProductsCore(false, true);
		}

		public void TestCheckOP_PartNum_ProductCodeSameWithBarcode_TwoProducts_Product1IsActiveAndProduct2IsInactive()
		{
			TestCheckOP_PartNum_ProductCodeSameWithBarcode_TwoProductsCore(true, false);
		}

		public void TestCheckOP_PartNum_ProductCodeSameWithBarcode_TwoProducts_Product1IsInactiveAndProduct2IsInactive()
		{
			TestCheckOP_PartNum_ProductCodeSameWithBarcode_TwoProductsCore(false, false);
		}

		void TestCheckOP_PartNum_ProductCodeSameWithBarcode_TwoProductsCore(bool active1, bool active2, bool expectedHasError = false)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("Org1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			part1.OP_IsActive = active1;
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "P2";
			barcode1.PH_F3_NKPackType = "P2";

			Factory.Save();

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "Part2");
			part2.OP_IsActive = active2;
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TESTCODE";
			barcode2.PH_F3_NKPackType = "TCE";

			AssertNoErrors(part2.OP_PartNumInfo);

			part2.OP_PartNum = "P2";
			if (expectedHasError)
			{
				AssertHasError(part2.OP_PartNumInfo, string.Format(Culture.Invariant, OrgSupplierPartValidation.PartNumCannotEqualToBarcode, "P1"));
			}
			else
			{
				AssertNoErrors("One of Products is in-active, should not have error", part2.OP_PartNumInfo);
			}
		}

		#endregion

		#region TestCheckOP_PartNum_ProductCodeSameWithBarcode_ProductHasNoBarcode

		public void TestCheckOP_PartNum_ProductCodeSameWithBarcode_ProductHasNoBarcode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("Org1");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "P1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "P2";
			barcode1.PH_F3_NKPackType = "P2T";

			Factory.Save();

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "Part2");

			AssertNoErrors(part2.OP_PartNumInfo);

			part2.OP_PartNum = "P2";
			AssertHasError(part2.OP_PartNumInfo, string.Format(Culture.Invariant, OrgSupplierPartValidation.PartNumCannotEqualToBarcode, "P1"));
		}

		#endregion

		#endregion

		#region TestCheckOP_StockKeepingUnitListValidation

		public void TestCheckOP_StockKeepingUnitListValidation()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "XXX";
			part.RunPreSaveValidation();
			AssertHasErrors(part.OP_StockKeepingUnitInfo);

			part.OP_StockKeepingUnit = "BOX";
			part.RunPreSaveValidation();
			AssertNoErrors(part.OP_StockKeepingUnitInfo);
		}

		#endregion

		#region TestCheckOP_Cubic

		public void TestCheckOP_Cubic()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			using (product.PostponedCubicUpdate())
			{
				product.OP_Depth = 1m;
				product.OP_Height = 1m;
				product.OP_Width = 1m;
				product.OP_MeasureUQ = Constants.Length.Metres;
			}
			AssertNoWarnings(product.OP_CubicInfo);

			product.OP_Cubic = 7m;
			AssertHasWarning(product.OP_CubicInfo, "The Cube does not match the Cubic Calculation of the Dimensions.");
		}

		public void TestCheckOP_Cubic_WithNegativeValue()
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_Cubic = -1.09;
			AssertHasError(product.OP_CubicInfo, "Cubic cannot be negative.");

			product.OP_Cubic = 1.27;
			AssertNoError(product.OP_CubicInfo, "Cubic cannot be negative.");

			product.OP_Cubic = 0;
			AssertNoError(product.OP_CubicInfo, "Cubic cannot be negative.");
		}

		#endregion

		#region TestCheckOP_StockKeepingUnitValidation

		public void TestCheckOP_StockKeepingUnitValidation()
		{
			var bomProduct1 = CreateProduct("B1", Constants.PkgUnit.Pallet);
			var bomProduct2 = CreateProduct("B2", Constants.PkgUnit.Pallet);
			var componentProduct = CreateProduct("C1", Constants.PkgUnit.Unit);
			var bomPartForBomProduct1 = CreateProductBOM(bomProduct1, componentProduct, 1m, Constants.PkgUnit.Box);
			var bomPartForBomProduct2 = CreateProductBOM(bomProduct2, componentProduct, 1m, Constants.PkgUnit.Sheet);
			CreatePartUnit(componentProduct.PartUnits, Constants.PkgUnit.Unit, Constants.PkgUnit.Sheet);
			CreatePartUnit(componentProduct.PartUnits, Constants.PkgUnit.Unit, Constants.PkgUnit.Box);
			AssertNoErrors("Precondition", componentProduct.OP_StockKeepingUnitInfo);

			componentProduct.OP_StockKeepingUnit = Constants.PkgUnit.Pallet;
			AssertHasError("There should be errors since there is no unit conversion between pallet and box.",
				componentProduct.OP_StockKeepingUnitInfo, "BOM Product 'B1' uses 'C1' as a component product. Make sure a unit conversion exists between PLT and BOX.");
			AssertHasError("There should be errors since there is no unit conversion between pallet and sheet.",
				componentProduct.OP_StockKeepingUnitInfo, "BOM Product 'B2' uses 'C1' as a component product. Make sure a unit conversion exists between PLT and SHT.");

			// setup unit conversion for the child component.
			CreatePartUnit(componentProduct.PartUnits, Constants.PkgUnit.Box, Constants.PkgUnit.Pallet);
			componentProduct.OP_StockKeepingUnit = Constants.PkgUnit.Unit; // To revalidate
			componentProduct.OP_StockKeepingUnit = Constants.PkgUnit.Pallet;
			AssertNoErrors("There should be no errors since units are convertible.", componentProduct.OP_StockKeepingUnitInfo);

			componentProduct.OP_StockKeepingUnit = "AAA";
			AssertHasError("No unit conversion errors should be displayed since stock keeping unit is invalid.", componentProduct.OP_StockKeepingUnitInfo, "Enter a valid Stock Keeping Unit.");

			componentProduct.OP_StockKeepingUnit = "";
			AssertHasError("No unit conversion errors should be displayed since stock keeping unit is empty.", componentProduct.OP_StockKeepingUnitInfo, "Please enter a Stock Keeping Unit.");
		}

		OrgSupplierPart CreateProduct(string partCode, string stockKeepingUnit)
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partCode;
			part.OP_Desc = partCode;
			part.OP_StockKeepingUnit = stockKeepingUnit;
			return part;
		}

		OrgPartBOM CreateProductBOM(OrgSupplierPart part, OrgSupplierPart subPart, ZDecimal componentQty, ZString componentPack)
		{
			var bom = Factory.New<OrgPartBOM>();
			bom.OE_OP_MainProduct = part.PK;
			bom.OE_OP_Component = subPart.PK;
			bom.OE_ComponentQty = componentQty;
			bom.OE_F3_NKPackType = componentPack;
			return bom;
		}

		OrgPartUnit CreatePartUnit(OrgPartUnitCollection partUnits, string childPackType, string parentPackType)
		{
			var partUnit = partUnits.AddNew();
			partUnit.OF_PackType = childPackType;
			partUnit.OF_QuantityInParent = 1;
			partUnit.OF_ParentPackType = parentPackType;
			return partUnit;
		}

		#endregion

		#region TestCheckOP_IsBarcoded

		#region TestCheckOP_IsBarcodedForPartBarcodes

		public void TestCheckOP_IsBarcodedForPartBarcodes()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "P1";
			product.OP_IsBarcoded = false;
			AssertNoErrors(product.OP_IsBarcodedInfo);

			product.PartBarcodes.AddNew();
			product.Validation.ValidateOP_IsBarcoded();
			AssertHasError(product.OP_IsBarcodedInfo, "Non barcoded products should not have barcodes defined.");
		}

		#endregion

		#region TestCheckOP_IsBarcodedForConfirmAttribute

		public void TestCheckOP_IsBarcodedForConfirmAttribute()
		{
			var client1 = Factory.New<OrgHeader>();
			client1.OH_Code = "XXX";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_IsBarcoded = false;
			var productClient1 = product.RelatedOrganisations.AddNew();
			productClient1.OU_OH = client1.PK;
			productClient1.OU_OP = product.PK;
			productClient1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			productClient1.OU_UsePartAttrib1 = true;

			AssertNoErrors(product.OP_IsBarcodedInfo);
			productClient1.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			product.Validation.ValidateOP_IsBarcoded();
			AssertHasError(product.OP_IsBarcodedInfo, "Non barcoded products should not have RF Confirm attributes. Confirm attributes are specified for the following clients: XXX.");

			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			client2.OH_Code = "ZZZ";
			var productClient2 = product.RelatedOrganisations.AddNew();
			productClient2.OU_OH = client2.PK;
			productClient2.OU_OP = product.PK;
			productClient2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			productClient2.OU_UsePartAttrib2 = true;
			productClient2.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute2;
			product.Validation.ValidateOP_IsBarcoded();
			AssertHasError(product.OP_IsBarcodedInfo, "Non barcoded products should not have RF Confirm attributes. Confirm attributes are specified for the following clients: XXX, ZZZ.");

			product.OP_IsBarcoded = true;
			product.Validation.ValidateOP_IsBarcoded();
			AssertNoErrors("If product is barcoded, no errors should be shown", product.OP_IsBarcodedInfo);
			product.OP_IsBarcoded = false;

			productClient1.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None;
			productClient2.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None;
			product.Validation.ValidateOP_IsBarcoded();
			AssertNoErrors(product.OP_IsBarcodedInfo);
		}

		#endregion

		#endregion
	}
}
