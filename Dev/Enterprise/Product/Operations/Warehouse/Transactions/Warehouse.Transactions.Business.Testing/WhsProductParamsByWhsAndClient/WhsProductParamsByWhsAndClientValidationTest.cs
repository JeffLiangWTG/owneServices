using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsProductParamsByWhsAndClientValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestValidateAll

		public void TestValidateAll()
		{
			var productParams = Factory.New<WhsProductParamsByWhsAndClient>();
			using (productParams.GetValidationSuspender())
			{
				productParams.PickGroupForBinding = "3";
			}
			AssertNoErrors("Precondition:", productParams.PickGroupForBindingInfo);

			productParams.Validation.ValidateAll();
			AssertHasError(productParams.PickGroupForBindingInfo, "Enter a valid Pick Group.");
		}

		#endregion

		#region TestCheckPickGroupForBinding

		public void TestCheckPickGroupForBinding()
		{
			var productParams = Factory.New<WhsProductParamsByWhsAndClient>();
			var collection = new PickGroupCollection();
			var pickGroup = collection.AddNew();
			pickGroup.Description = (NoResString)"Desc";

			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AssertNoErrors("Precondition:", productParams.PickGroupForBindingInfo);

				productParams.PickGroupForBinding = "1";
				AssertNoErrors(productParams.PickGroupForBindingInfo);

				productParams.PickGroupForBinding = "2";
				AssertHasError(productParams.PickGroupForBindingInfo, "Enter a valid Pick Group.");

				productParams.PickGroupForBinding = "";
				AssertNoErrors(productParams.PickGroupForBindingInfo);

				productParams.W3_PickGroup = 3;
				AssertHasError(productParams.PickGroupForBindingInfo, "Enter a valid Pick Group.");
			}
		}

		#endregion

		#region TestCheckDuplicates

		public void TestCheckDuplicates()
		{
			WhsProduct product = WhsProduct.GetWhsProduct(Factory.New<OrgSupplierPart>());
			WhsProductParamsByWhsAndClient parent1 = product.ParamsByWhsAndClient.AddNew();
			WhsProductParamsByWhsAndClient parent2 = product.ParamsByWhsAndClient.AddNew();

			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();
			WhsWarehouse whs1 = Factory.New<WhsWarehouse>();
			WhsWarehouse whs2 = Factory.New<WhsWarehouse>();

			string errorMsg = "This is a duplicate entry. There is already an entry for this Client / Warehouse";

			parent1.W3_OH = org1.PK;
			AssertNoError(parent1.W3_OHInfo, errorMsg);
			AssertNoError(parent1.W3_WWInfo, errorMsg);
			parent1.W3_WW = whs1.PK;
			AssertNoError(parent1.W3_OHInfo, errorMsg);
			AssertNoError(parent1.W3_WWInfo, errorMsg);

			parent2.W3_OH = org2.PK;
			AssertNoError(parent1.W3_OHInfo, errorMsg);
			AssertNoError(parent1.W3_WWInfo, errorMsg);
			parent2.W3_WW = whs2.PK;
			AssertNoError(parent1.W3_OHInfo, errorMsg);
			AssertNoError(parent1.W3_WWInfo, errorMsg);

			parent2.W3_OH = org1.PK;
			AssertNoError(parent1.W3_OHInfo, errorMsg);
			AssertNoError(parent1.W3_WWInfo, errorMsg);
			parent2.W3_WW = whs1.PK;
			AssertHasError(parent2.W3_OHInfo, errorMsg);
			AssertHasError(parent2.W3_WWInfo, errorMsg);

			parent2.W3_WW = whs2.PK;
			AssertNoError(parent1.W3_OHInfo, errorMsg);
			AssertNoError(parent1.W3_WWInfo, errorMsg);
		}

		#endregion

		#region TestCheckW3_OH

		public void TestCheckW3_OH()
		{
			var org = Helper.CreateClient("Org");
			var product = WhsProduct.GetWhsProduct(Factory.New<OrgSupplierPart>());
			var parent = product.ParamsByWhsAndClient.AddNew();
			parent.W3_OH = org.PK;
			AssertHasError(parent.W3_OHInfo, "This organization has no relationship with this product. You can create relationships in the Related Organizations tab on this form");

			var relation = parent.SupplierPart.RelatedOrganisations.AddNew();
			relation.OU_OH = org.PK;
			parent.W3_OH = org.PK;
			AssertNoErrors(parent.W3_OHInfo);
		}

		public void TestCheckW3_OH_InvalidClientPK()
		{
			var product = WhsProduct.GetWhsProduct(Factory.New<OrgSupplierPart>());
			var parent = product.ParamsByWhsAndClient.AddNew();

			parent.W3_OH = ZGuid.Empty;
			AssertHasError(parent.W3_OHInfo, "Please enter a Client.");

			parent.W3_OH = ZGuid.NewZGuid();
			AssertHasError(parent.W3_OHInfo, "Enter a valid Client.");
		}

		#endregion

		#region TestCheckW3_OH_CannotBeModified

		public void TestCheckW3_OH_CannotBeModified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			var productParam_WithoutStock = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var productParam_WithStock = Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);
			var otherClient = Helper.CreateClient("CLIENT2");
			Helper.CreateProductClientRelationShip(otherClient, data.Part1);
			Helper.CreateProductClientRelationShip(otherClient, data.Part2);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m, true, false);

			Factory.Save();

			var expectedErrorMessage = PartAttributeValidation.GetThereIsCurrentInventoryUsingJulianBatchNumbersErrorMessage("Client");

			productParam_WithoutStock.W3_OH = otherClient.PK;
			productParam_WithStock.W3_OH = otherClient.PK;
			AssertNoError(productParam_WithoutStock.W3_OHInfo, expectedErrorMessage);
			AssertHasError(productParam_WithStock.W3_OHInfo, expectedErrorMessage);

			productParam_WithoutStock.W3_OH = data.Org1.PK;
			productParam_WithStock.W3_OH = data.Org1.PK;
			AssertNoError(productParam_WithoutStock.W3_OHInfo, expectedErrorMessage);
			AssertNoError(productParam_WithStock.W3_OHInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckW3_WW

		public void TestCheckW3_WW()
		{
			var org = Factory.New<OrgHeader>();
			var whs = Helper.CreateWarehouse("Whs");
			var part = Helper.CreateProduct(org, "P1");
			var parent = Helper.CreateProductParamsByWhsAndClient(part, org, whs);
			AssertNoErrors(parent.W3_WWInfo);

			parent.W3_WW = ZGuid.Empty;
			AssertHasError(parent.W3_WWInfo, "Please enter a Warehouse.");

			parent.W3_WW = ZGuid.NewZGuid();
			AssertHasError(parent.W3_WWInfo, "Enter a valid Warehouse.");
		}

		#endregion

		#region TestCheckW3_WW_CannotBeModified

		public void TestCheckW3_WW_CannotBeModified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			var productParam_WithoutStock = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var productParam_WithStock = Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);
			var otherWarehouse = Helper.CreateWarehouse("WH2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m, true, false);

			Factory.Save();

			var expectedErrorMessage = PartAttributeValidation.GetThereIsCurrentInventoryUsingJulianBatchNumbersErrorMessage("Warehouse");

			productParam_WithoutStock.W3_WW = otherWarehouse.PK;
			productParam_WithStock.W3_WW = otherWarehouse.PK;
			AssertNoError(productParam_WithoutStock.W3_WWInfo, expectedErrorMessage);
			AssertHasError(productParam_WithStock.W3_WWInfo, expectedErrorMessage);

			productParam_WithoutStock.W3_WW = data.Whs1.PK;
			productParam_WithStock.W3_WW = data.Whs1.PK;
			AssertNoError(productParam_WithoutStock.W3_WWInfo, expectedErrorMessage);
			AssertNoError(productParam_WithStock.W3_WWInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckW3_OP

		public void TestCheckW3_OP()
		{
			Parent.W3_OP = ZGuid.NewZGuid();
			AssertNoErrors(Parent.W3_OPInfo);
			Parent.W3_OP = ZGuid.Empty;
			AssertHasErrors(Parent.W3_OPInfo);
		}

		#endregion

		#region TestCheckW3_StockTakeCycle

		public void TestCheckW3_StockTakeCycle()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair("B", "B");
			WarehouseDataRegistry.Instance.StocktakeCycle.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, result);

			Parent.W3_StockTakeCycle = "B";
			AssertNoErrors(Parent.W3_StockTakeCycleInfo);

			Parent.W3_StockTakeCycle = "";
			AssertNoErrors(Parent.W3_StockTakeCycleInfo);

			Parent.W3_StockTakeCycle = "A";
			AssertHasErrors(Parent.W3_StockTakeCycleInfo);
		}

		#endregion

		#region TestCheckW3_EconomicQuantity

		public void TestCheckW3_EconomicQuantity()
		{
			TestMinDecimal(Parent.W3_EconomicQuantityInfo, ErrorCheckType.HasErrors, 0);
		}

		public void TestCheckW3_EconomicQuantity_MustGreaterThanReplenishmentMinimum()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var expectedErrorMessageReplenishmentMinimum = "Replenishment Minimum must be less than Economic Quantity";
			var expectedErrorMessageEconomicQuantity = "Economic Quantity must be greater than or equal to Replenishment Minimum";
			AssertEquals("Precondition", 0m, productParam.W3_ReplenishmentMinimum);
			AssertEquals("Precondition", 0m, productParam.W3_EconomicQuantity);

			productParam.W3_EconomicQuantity = 10m;
			AssertNoError(productParam.W3_EconomicQuantityInfo, expectedErrorMessageEconomicQuantity);
			AssertNoError(productParam.W3_ReplenishmentMinimumInfo, expectedErrorMessageReplenishmentMinimum);

			productParam.W3_ReplenishmentMinimum = 20m;
			productParam.W3_EconomicQuantity = 10m;
			AssertHasError(productParam.W3_EconomicQuantityInfo, expectedErrorMessageEconomicQuantity);
			AssertHasError(productParam.W3_ReplenishmentMinimumInfo, expectedErrorMessageReplenishmentMinimum);

			productParam.W3_EconomicQuantity = 20m;
			AssertHasError(productParam.W3_EconomicQuantityInfo, expectedErrorMessageEconomicQuantity);
			AssertHasError(productParam.W3_ReplenishmentMinimumInfo, expectedErrorMessageReplenishmentMinimum);

			productParam.W3_EconomicQuantity = 21m;
			AssertNoError(productParam.W3_EconomicQuantityInfo, expectedErrorMessageEconomicQuantity);
			AssertNoError(productParam.W3_ReplenishmentMinimumInfo, expectedErrorMessageReplenishmentMinimum);

			productParam.W3_EconomicQuantity = -1m;
			AssertHasError("Precondition Economic Quantity Info has already error.", productParam.W3_EconomicQuantityInfo, "Economic Quantity cannot be negative.");
			AssertNoError("When Economic Quantity Info has already error.does not add this error", productParam.W3_EconomicQuantityInfo, expectedErrorMessageEconomicQuantity);
		}

		public void TestCheckW3_EconomicQuantity_MustGreaterThanOrEqualToReplenishmentMultiple()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var expectedErrorMessage = "Economic Quantity must be greater than or equal to Replenishment Multiple";

			productParam.W3_EconomicQuantity = 10m;
			AssertNoError(productParam.W3_EconomicQuantityInfo, expectedErrorMessage);

			productParam.W3_ReplenishmentMultiple = 20m;
			productParam.W3_EconomicQuantity = 10m;
			AssertHasError(productParam.W3_EconomicQuantityInfo, expectedErrorMessage);

			productParam.W3_EconomicQuantity = 20m;
			AssertNoError(productParam.W3_EconomicQuantityInfo, expectedErrorMessage);

			productParam.W3_EconomicQuantity = 21m;
			AssertNoError(productParam.W3_EconomicQuantityInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckW3_ReplenishmentMinimum

		public void TestCheckW3_ReplenishmentMinimum()
		{
			var productParam = Factory.New<WhsProductParamsByWhsAndClient>();
			productParam.W3_EconomicQuantity = 10001;
			TestMinDecimal(productParam.W3_ReplenishmentMinimumInfo, ErrorCheckType.HasErrors, 0);
		}

		public void TestCheckW3_ReplenishmentMinimum_MustBeLessThanOrEqualToEconomicQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			AssertEquals("Precondition", 0m, productParam.W3_EconomicQuantity);
			AssertEquals("Precondition", 0m, productParam.W3_ReplenishmentMinimum);

			var expectedErrorMessageReplenishmentMinimum = "Replenishment Minimum must be less than Economic Quantity";
			var expectedErrorMessageEconomicQuantity = "Economic Quantity must be greater than or equal to Replenishment Minimum";

			productParam.W3_ReplenishmentMinimum = 10m;
			AssertHasError(productParam.W3_ReplenishmentMinimumInfo, expectedErrorMessageReplenishmentMinimum);
			AssertHasError(productParam.W3_EconomicQuantityInfo, expectedErrorMessageEconomicQuantity);

			productParam.W3_EconomicQuantity = 10m;
			productParam.W3_ReplenishmentMinimum = 10m;
			AssertHasError(productParam.W3_ReplenishmentMinimumInfo, expectedErrorMessageReplenishmentMinimum);
			AssertHasError(productParam.W3_EconomicQuantityInfo, expectedErrorMessageEconomicQuantity);

			productParam.W3_ReplenishmentMinimum = 9m;
			AssertNoError(productParam.W3_ReplenishmentMinimumInfo, expectedErrorMessageReplenishmentMinimum);
			AssertNoError(productParam.W3_EconomicQuantityInfo, expectedErrorMessageEconomicQuantity);
		}

		#endregion

		#region TestCheckW3_ReplenishmentMultiple

		public void TestCheckW3_ReplenishmentMultiple()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var expectedErrorMessage = "Replenishment Multiple cannot be negative.";

			productParam.W3_ReplenishmentMultiple = 0m;
			AssertNoErrors(productParam.W3_ReplenishmentMultipleInfo);

			productParam.W3_ReplenishmentMultiple = -1m;
			AssertHasError(productParam.W3_ReplenishmentMultipleInfo, expectedErrorMessage);

			// We should set EconomicQty first because of another check about ReplenishmentMultiple < EconomicQuantity in CheckW3_ReplenishmentMultiple
			productParam.W3_EconomicQuantity = 10001m;
			productParam.W3_ReplenishmentMultiple = 10001m;
			AssertNoErrors(productParam.W3_ReplenishmentMultipleInfo);
		}

		public void TestCheckW3_ReplenishmentMultiple_MustLessThanOrQualEconomicQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var expectedErrorMessage = "Economic Quantity must be greater than or equal to Replenishment Multiple";

			productParam.W3_ReplenishmentMultiple = 10m;
			AssertNoError(productParam.W3_ReplenishmentMinimumInfo, expectedErrorMessage);

			productParam.W3_EconomicQuantity = 10m;
			productParam.W3_ReplenishmentMultiple = 10m;
			AssertNoError(productParam.W3_ReplenishmentMinimumInfo, expectedErrorMessage);

			productParam.W3_ReplenishmentMultiple = 11m;
			AssertHasError(productParam.W3_ReplenishmentMultipleInfo, expectedErrorMessage);
		}

		public void TestCheckW3_ReplenishmentMultipleIsZeroWhenEconomicQuantityGreaterThanZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part = data.Part1;
			var productParam = Helper.CreateProductParamsByWhsAndClient(part, data.Org1, data.Whs1);
			var expectedErrorMessage = "Please enter a Replenishment Multiple greater than or equal to 1";

			AssertNoError(productParam.W3_ReplenishmentMultipleInfo, expectedErrorMessage);

			productParam.W3_EconomicQuantity = 1m;
			AssertEquals(1m, productParam.W3_ReplenishmentMultiple);
			AssertNoError(productParam.W3_ReplenishmentMultipleInfo, expectedErrorMessage);

			productParam.W3_ReplenishmentMultiple = 0m;
			AssertHasError(productParam.W3_ReplenishmentMultipleInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckW3_ExpiryNotificationPeriod

		public void TestCheckW3_ExpiryNotificationPeriod()
		{
			TestMinInt(Parent.W3_ExpiryNotificationPeriodInfo, ErrorCheckType.HasErrors, 0);
		}

		#endregion

		#region TestCheckW3_WL_StagingLocationBOM

		public void TestCheckW3_WL_StagingLocationBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var normalLocation = data.Whs1.DefaultLocation;
			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var expectedErrorMessage = "Please enter a non Dock Door Location.";
			AssertNoErrors(productParam.W3_WL_StagingLocationBOMInfo);

			productParam.W3_WL_StagingLocationBOM = dockDoorLocation.PK;
			AssertHasError(productParam.W3_WL_StagingLocationBOMInfo, expectedErrorMessage);

			productParam.W3_WL_StagingLocationBOM = normalLocation.PK;
			AssertNoErrors(productParam.W3_WL_StagingLocationBOMInfo);
		}

		#endregion

		#region TestCheckW3_WL_StagingLocationBOM_MustBeNonBonded

		public void TestCheckW3_WL_StagingLocationBOM_MustBeNonBonded()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			Factory.Save();

			var locationPickingBonded = data.Whs1.FindLocation("A-1");
			locationPickingBonded.WLV_WA_PickingArea = bondedArea.PK;

			var locationPutawayBonded = data.Whs1.FindLocation("A-2");
			locationPutawayBonded.WLV_WA_PutawayArea = bondedArea.PK;

			var locationBothBonded = data.Whs1.FindLocation("A-3");
			locationBothBonded.WLV_WA_PickingArea = bondedArea.PK;
			locationBothBonded.WLV_WA_PutawayArea = bondedArea.PK;

			var normalLocation = data.Whs1.FindLocation("A-4");
			Factory.Save();

			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var expectedErrorMessage = "Staging Locations cannot be in Bonded Areas.";
			AssertNoErrors(productParam.W3_WL_StagingLocationBOMInfo);

			productParam.W3_WL_StagingLocationBOM = locationPickingBonded.PK;
			AssertHasError(productParam.W3_WL_StagingLocationBOMInfo, expectedErrorMessage);

			productParam.W3_WL_StagingLocationBOM = normalLocation.PK;
			AssertNoErrors(productParam.W3_WL_StagingLocationBOMInfo);

			productParam.W3_WL_StagingLocationBOM = locationPutawayBonded.PK;
			AssertHasError(productParam.W3_WL_StagingLocationBOMInfo, expectedErrorMessage);

			productParam.W3_WL_StagingLocationBOM = locationBothBonded.PK;
			AssertHasError(productParam.W3_WL_StagingLocationBOMInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckW3_WL_StagingLocationBOM_NonInwardProcessing

		public void TestCheckW3_WL_StagingLocationBOM_NonInwardProcessing()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			Factory.Save();

			var locationPickingIpr = data.Whs1.FindLocation("A-1");
			locationPickingIpr.WLV_WA_PickingArea = inwardProcessingArea.PK;
			locationPickingIpr.WLV_WA_PutawayArea = inwardProcessingArea.PK;

			var locationPutawayIpr = data.Whs1.FindLocation("A-2");
			locationPutawayIpr.WLV_WA_PickingArea = inwardProcessingArea.PK;
			locationPutawayIpr.WLV_WA_PutawayArea = inwardProcessingArea.PK;

			var locationBothIpr = data.Whs1.FindLocation("A-3");
			locationBothIpr.WLV_WA_PickingArea = inwardProcessingArea.PK;
			locationBothIpr.WLV_WA_PutawayArea = inwardProcessingArea.PK;

			var normalLocation = data.Whs1.FindLocation("A-4");
			Factory.Save();

			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var expectedErrorMessage = "Staging Locations cannot be in Inward Processing Areas.";
			AssertNoErrors(productParam.W3_WL_StagingLocationBOMInfo);

			productParam.W3_WL_StagingLocationBOM = locationPickingIpr.PK;
			AssertHasError(productParam.W3_WL_StagingLocationBOMInfo, expectedErrorMessage);

			productParam.W3_WL_StagingLocationBOM = normalLocation.PK;
			AssertNoErrors(productParam.W3_WL_StagingLocationBOMInfo);

			productParam.W3_WL_StagingLocationBOM = locationPutawayIpr.PK;
			AssertHasError(productParam.W3_WL_StagingLocationBOMInfo, expectedErrorMessage);

			productParam.W3_WL_StagingLocationBOM = locationBothIpr.PK;
			AssertHasError(productParam.W3_WL_StagingLocationBOMInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckW3_WL_InwardsProcessingStagingLocationBOM

		public void TestCheckW3_WL_InwardsProcessingStagingLocationBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			Factory.Save();

			var locationPickingIpr = data.Whs1.FindLocation("A-1");
			locationPickingIpr.WLV_WA_PickingArea = inwardProcessingArea.PK;

			var locationPutawayIpr = data.Whs1.FindLocation("A-2");
			locationPutawayIpr.WLV_WA_PutawayArea = inwardProcessingArea.PK;

			var locationBothIpr = data.Whs1.FindLocation("A-3");
			locationBothIpr.WLV_WA_PickingArea = inwardProcessingArea.PK;
			locationBothIpr.WLV_WA_PutawayArea = inwardProcessingArea.PK;

			var normalLocation = data.Whs1.FindLocation("A-4");

			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var expectedErrorMessage = "Inward Processing Staging Locations must be in Inward Processing Areas.";
			AssertNoErrors(productParam.W3_WL_InwardsProcessingStagingLocationBOMInfo);

			productParam.W3_WL_InwardsProcessingStagingLocationBOM = locationPickingIpr.PK;
			AssertNoErrors(productParam.W3_WL_InwardsProcessingStagingLocationBOMInfo);

			productParam.W3_WL_InwardsProcessingStagingLocationBOM = normalLocation.PK;
			AssertHasError(productParam.W3_WL_InwardsProcessingStagingLocationBOMInfo, expectedErrorMessage);

			productParam.W3_WL_InwardsProcessingStagingLocationBOM = locationPutawayIpr.PK;
			AssertNoErrors(productParam.W3_WL_InwardsProcessingStagingLocationBOMInfo);

			productParam.W3_WL_InwardsProcessingStagingLocationBOM = locationBothIpr.PK;
			AssertNoErrors(productParam.W3_WL_InwardsProcessingStagingLocationBOMInfo);
		}

		#endregion

		#region TestCheckW3_WL_InwardsProcessingStagingLocationBOM_DifferentWarehouse

		public void TestCheckW3_WL_InwardsProcessingStagingLocationBOM_DifferentWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);

			var whs2 = Helper.CreateWarehouse("W2", "A", 1, 1);
			whs2.WW_IsVirtualWarehouse = true;
			var inwardProcessingAreaWhs2 = Helper.CreateArea(whs2, "IPR", AreaTypes.Codes.InwardProcessing);
			Factory.Save();

			var locationBothIpr = data.Whs1.FindLocation("A");
			locationBothIpr.WLV_WA_PickingArea = inwardProcessingArea.PK;
			locationBothIpr.WLV_WA_PutawayArea = inwardProcessingArea.PK;

			var locationWhs2 = whs2.FindLocation("A");
			locationWhs2.WLV_WA_PickingArea = inwardProcessingAreaWhs2.PK;
			locationWhs2.WLV_WA_PutawayArea = inwardProcessingAreaWhs2.PK;

			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var expectedErrorMessage = "Location A does not exist in Warehouse 1.";
			AssertNoErrors(productParam.W3_WL_InwardsProcessingStagingLocationBOMInfo);

			productParam.W3_WL_InwardsProcessingStagingLocationBOM = locationWhs2.PK;
			AssertHasError(productParam.W3_WL_InwardsProcessingStagingLocationBOMInfo, expectedErrorMessage);

			productParam.W3_WL_InwardsProcessingStagingLocationBOM = locationBothIpr.PK;
			AssertNoErrors(productParam.W3_WL_InwardsProcessingStagingLocationBOMInfo);
		}

		#endregion

		#region Areas

		#region TestW3_WL_StagingAreaBOM

		public void TestW3_WL_StagingLocationBOM()
		{
			var whs1 = Helper.CreateWarehouse("WHS1");
			var whs2 = Helper.CreateWarehouse("WHS2");
			var loc1 = Helper.CreateRowAndGenerateLocations(whs1, "A").Locations[0];
			var loc2 = Helper.CreateRowAndGenerateLocations(whs2, "B").Locations[0];

			Parent.W3_WL_StagingLocationBOM = loc1.PK;
			AssertNoErrors("Parent has no Warehouse thus any BOM Staging Area is valid.", Parent.W3_WL_StagingLocationBOMInfo);

			Parent.W3_WW = whs1.PK;
			Parent.W3_WL_StagingLocationBOM = loc2.PK;
			AssertHasError(Parent.W3_WL_StagingLocationBOMInfo, "Location B does not exist in Warehouse WHS1.");

			Parent.W3_WL_StagingLocationBOM = loc1.PK;
			AssertNoErrors(Parent.W3_WL_StagingLocationBOMInfo);
		}

		#endregion

		#region TestW3_WA_DynamicPickFaceArea

		public void TestW3_WA_DynamicPickFaceArea()
		{
			var client = Helper.CreateClient("CL1");
			var product = Helper.CreateProduct("P1", client);
			var warehouse1 = Helper.CreateWarehouse("WHS1");
			var warehouse2 = Helper.CreateWarehouse("WHS2");
			var area1 = Helper.CreateArea(warehouse1, "A1", Warehouse.Environment.CodeLists.AreaTypes.Codes.DynamicPickFace);
			var area2 = Helper.CreateArea(warehouse2, "A2", Warehouse.Environment.CodeLists.AreaTypes.Codes.DynamicPickFace);

			Parent.W3_OP = product.PK;
			Parent.W3_OH = client.PK;

			Parent.W3_WA_DynamicPickFaceArea = area1.PK;
			AssertNoErrors("Parent has no Warehouse thus any Dynamic Pick Face is valid.", Parent.W3_WA_DynamicPickFaceAreaInfo);

			Parent.W3_WW = warehouse1.PK;
			Parent.W3_WA_DynamicPickFaceArea = area2.PK;
			AssertHasError(Parent.W3_WA_DynamicPickFaceAreaInfo, "Area A2 does not exist in Warehouse WHS1.");

			Parent.W3_WA_DynamicPickFaceArea = area1.PK;
			AssertNoErrors(Parent.W3_WL_StagingLocationBOMInfo);
		}

		public void TestW3_WA_DynamicPickFaceArea_OnlyDynamicAreaType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var fixedArea = Helper.CreateArea(data.Whs1, "A1", "");
			var dynamicArea = Helper.CreateArea(data.Whs1, "A2", Warehouse.Environment.CodeLists.AreaTypes.Codes.DynamicPickFace);
			var dynamicLocation = data.Whs1.FindLocation("A-1");

			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Parent.W3_WW = data.Whs1.PK;
			Parent.W3_OP = data.Part1.PK;
			Parent.W3_OH = data.Org1.PK;

			Parent.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			AssertNoErrors("Parent is assigned to valid dynamic pick face area.", Parent.W3_WA_DynamicPickFaceAreaInfo);

			Parent.W3_WA_DynamicPickFaceArea = fixedArea.PK;
			AssertHasError(Parent.W3_WA_DynamicPickFaceAreaInfo, "Area A1 is not a dynamic picking area.");
		}

		public void TestW3_WA_DynamicPickFaceArea_CannotAlsoBeAssignedToFixedPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "A2", "DPF");
			var dynamicLocation = data.Whs1.FindLocation("A-1");

			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Parent.W3_WW = data.Whs1.PK;
			Parent.W3_OP = data.Part1.PK;
			Parent.W3_OH = data.Org1.PK;

			Parent.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			AssertNoErrors("Parent is only assigned to dynamic pick face area.", Parent.W3_WA_DynamicPickFaceAreaInfo);

			var fixedPickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, data.Whs1.DefaultLocation);
			Parent.Validation.ValidateAll();

			AssertHasError(Parent.W3_WA_DynamicPickFaceAreaInfo, "Product is also assigned to a fixed pick face.");
		}

		public void TestW3_WA_DynamicPickFaceArea_CanAlsoBeAssignedToFixedPickFace_WithSameClientButDifferentWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse2 = Helper.CreateWarehouse("WHS2", "A", 2, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "A2", "DPF");
			var dynamicLocation = data.Whs1.FindLocation("A-1");

			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Parent.W3_WW = data.Whs1.PK;
			Parent.W3_OP = data.Part1.PK;
			Parent.W3_OH = data.Org1.PK;

			Parent.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			AssertNoErrors("Parent is only assigned to dynamic pick face area.", Parent.W3_WA_DynamicPickFaceAreaInfo);

			var fixedPickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, warehouse2.DefaultLocation);
			Parent.Validation.ValidateAll();

			AssertNoErrors("Fixed Pick Face is located in different warehouse", Parent.W3_WA_DynamicPickFaceAreaInfo);
		}

		public void TestW3_WA_DynamicPickFaceArea_CannotBeAssignedWithNoLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var dynamicArea = Helper.CreateArea(data.Whs1, "A2", "DPF");

			Parent.W3_WW = data.Whs1.PK;
			Parent.W3_OP = data.Part1.PK;
			Parent.W3_OH = data.Org1.PK;

			Parent.W3_WA_DynamicPickFaceArea = dynamicArea.PK;

			AssertHasError(Parent.W3_WA_DynamicPickFaceAreaInfo, string.Format("Dynamic area {0} contains no locations.", dynamicArea.WA_NameMultilingual));

			var dynamicLocation = data.Whs1.FindLocation("A-1");
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Parent.Validation.ValidateAll();

			AssertNoErrors("Dynamic area now contains locations", Parent.W3_WA_DynamicPickFaceAreaInfo);
		}

		#endregion

		#endregion

		#region TestCheckW3_F3_NKReceivedPackType, TestCheckW3_F3_NKReleasedPackType

		public void TestCheckW3_F3_NKReceivedPackType()
		{
			AssertPackTypeValidation(Parent.W3_F3_NKReceivedPackTypeInfo);
		}

		public void TestCheckW3_F3_NKReleasedPackType()
		{
			AssertPackTypeValidation(Parent.W3_F3_NKReleasedPackTypeInfo);
		}

		void AssertPackTypeValidation(ZPropertyInfo packTypeInfo)
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			Parent.W3_OP = part.PK;

			packTypeInfo.Value = (ZString)"xXX";
			AssertHasErrors(packTypeInfo);

			// create unit conversion for "BOX"
			OrgPartUnit partUnit = part.PartUnits.AddNew();
			partUnit.OF_QuantityInParent = 1.5;
			partUnit.OF_PackType = "BOX";
			partUnit.OF_ParentPackType = "UNT";

			string warning = string.Format(
				"There is no unit conversion from Stock Keeping Unit to {0}. This means the Client Quantities " +
				"on reports and web tracker may be incorrect. If the conversion is 1.0 then ignore this warning, otherwise enter " +
				"a unit conversion on the Unit Conversions Tab from Stock Keeping Unit to {0}",
				packTypeInfo.Description);

			packTypeInfo.Value = (ZString)"BAG";
			AssertNoErrors(packTypeInfo);
			AssertHasWarning(packTypeInfo, warning);

			packTypeInfo.Value = (ZString)"BOX";
			AssertNoErrors(packTypeInfo);
			AssertNoWarnings(packTypeInfo);

			packTypeInfo.Value = (ZString)"";
			AssertNoErrors(packTypeInfo);
		}

		#endregion

		#region TestCheckW3_MaximumShelfLife

		#region TestCheckW3_MaximumShelfLife

		public void TestCheckW3_MaximumShelfLife()
		{
			TestMinShort(Parent.W3_MaximumShelfLifeInfo, ErrorCheckType.HasErrors, 0);
		}

		#endregion

		#region TestCheckW3_MaximumShelfLife_CannotBeZeroIfStockExist_JulianBatchNumberAttribute

		public void TestCheckW3_MaximumShelfLife_CannotBeZeroIfStockExist_JulianBatchNumberAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			var productParam_WithoutStock = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var productParam_WithStock = Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m, true, false);

			Factory.Save();

			var expectedErrorMessage = "Maximum Shelf Life cannot be 0 when there are stock for this product, client and warehouse currently in use.";

			productParam_WithoutStock.W3_MaximumShelfLife = 5;
			productParam_WithStock.W3_MaximumShelfLife = 5;
			AssertNoError(productParam_WithoutStock.W3_MaximumShelfLifeInfo, expectedErrorMessage);
			AssertNoError(productParam_WithStock.W3_MaximumShelfLifeInfo, expectedErrorMessage);

			productParam_WithoutStock.W3_MaximumShelfLife = 0;
			productParam_WithStock.W3_MaximumShelfLife = 0;
			AssertNoError(productParam_WithoutStock.W3_MaximumShelfLifeInfo, expectedErrorMessage);
			AssertHasError(productParam_WithStock.W3_MaximumShelfLifeInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckW3_MaximumShelfLife_CannotBeModified

		public void TestCheckW3_MaximumShelfLife_CannotBeModified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, part3, AttributeNumber.One, true);

			var productParam_WithoutStock = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var productParam_WithStock = Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);
			var productParam_WithStock_ZeroMaxShelfLife = Helper.CreateProductParamsByWhsAndClient(part3, data.Org1, data.Whs1);
			productParam_WithoutStock.W3_MaximumShelfLife = 5;
			productParam_WithStock.W3_MaximumShelfLife = 5;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m, true, false);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", part3, 10m, true, false);

			Factory.Save();

			var expectedErrorMessage = PartAttributeValidation.GetThereIsCurrentInventoryUsingJulianBatchNumbersErrorMessage("Maximum Shelf Life");

			productParam_WithoutStock.W3_MaximumShelfLife = 10;
			productParam_WithStock.W3_MaximumShelfLife = 10;
			productParam_WithStock_ZeroMaxShelfLife.W3_MaximumShelfLife = 10;
			AssertNoError("Maximum Shelf Life can be changed if Julian Batch Number attribute is used but there are no current stock for the Client-Warehouse-Product.", productParam_WithoutStock.W3_MaximumShelfLifeInfo, expectedErrorMessage);
			AssertHasError("Maximum Shelf Life can NOT be changed if Julian Batch Number attribute is used and there are current stock for the Client-Warehouse-Product.", productParam_WithStock.W3_MaximumShelfLifeInfo, expectedErrorMessage);
			AssertNoError("Maximum Shelf Life can be changed if Julian Batch Number attribute is used and there are current stock for the Client-Warehouse-Product and previous value was 0.", productParam_WithStock_ZeroMaxShelfLife.W3_MaximumShelfLifeInfo, expectedErrorMessage);

			productParam_WithoutStock.W3_MaximumShelfLife = 5;
			productParam_WithStock.W3_MaximumShelfLife = 5;
			productParam_WithStock_ZeroMaxShelfLife.W3_MaximumShelfLife = 5;
			AssertNoError("Maximum Shelf Life can be changed if Julian Batch Number attribute is used but there are no current stock for the Client-Warehouse-Product.", productParam_WithoutStock.W3_MaximumShelfLifeInfo, expectedErrorMessage);
			AssertNoError("Maximum Shelf Life can be changed back if Julian Batch Number attribute is used and there are current stock for the Client-Warehouse-Product.", productParam_WithStock.W3_MaximumShelfLifeInfo, expectedErrorMessage);
			AssertNoError("Maximum Shelf Life can be changed if Julian Batch Number attribute is used and there are current stock for the Client-Warehouse-Product and previous value was 0.", productParam_WithStock_ZeroMaxShelfLife.W3_MaximumShelfLifeInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckW3_MaximumShelfLife_CannotBeLessThenConsigneeMinShelfLifeAccepted

		public void TestCheckW3_MaximumShelfLife_CannotBeLessThenConsigneeMinShelfLifeAccepted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var orgPartRelation = data.Part1.RelatedOrganisations[0];

			orgPartRelation.OU_ConsigneeMinShelfLifeAccepted = 0;
			productParam.W3_MaximumShelfLife = 0;
			AssertNoErrors(productParam.W3_MaximumShelfLifeInfo);

			orgPartRelation.OU_ConsigneeMinShelfLifeAccepted = 10;
			productParam.W3_MaximumShelfLife = 5;
			AssertHasError(productParam.W3_MaximumShelfLifeInfo, "Maximum shelf life 5 cannot be less than Minimum Shelf Life 10.");

			orgPartRelation.OU_ConsigneeMinShelfLifeAccepted = 10;
			productParam.W3_MaximumShelfLife = 15;
			AssertNoErrors(productParam.W3_MaximumShelfLifeInfo);

			orgPartRelation.OU_ConsigneeMinShelfLifeAccepted = 20;
			productParam.W3_MaximumShelfLife = 15;
			AssertHasError(productParam.W3_MaximumShelfLifeInfo, "Maximum shelf life 15 cannot be less than Minimum Shelf Life 20.");

			orgPartRelation.OU_ConsigneeMinShelfLifeAccepted = 20;
			productParam.W3_MaximumShelfLife = 0;
			AssertNoErrors(productParam.W3_MaximumShelfLifeInfo);
		}

		#endregion

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var productParam = Factory.New<WhsProductParamsByWhsAndClient>();
			var validation = new TestWhsProductParamsByWhsAndClientValidation(productParam);

			var list = new string[]
			{
				WhsProductParamsByWhsAndClientSchema.Constants.W3_WA_DynamicPickFaceArea,
				WhsProductParamsByWhsAndClientSchema.Constants.W3_WL_StagingLocationBOM,
				WhsProductParamsByWhsAndClientSchema.Constants.W3_WPG_PutawayGroup,
				WhsProductParamsByWhsAndClientSchema.Constants.W3_WL_InwardsProcessingStagingLocationBOM,
				WhsProductParamsByWhsAndClientSchema.Constants.W3_F3_NKReceivedPackType,
				WhsProductParamsByWhsAndClientSchema.Constants.W3_F3_NKReleasedPackType
			};

			foreach (var propertyInfo in productParam.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (list.Contains(propertyInfo.Name))
				{
					AssertEquals("NK/FK which cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
				else
				{
					AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
			}
		}

		#endregion

		#region TestWhsProductParamsByWhsAndClientValidation

		class TestWhsProductParamsByWhsAndClientValidation : WhsProductParamsByWhsAndClientValidation
		{
			public TestWhsProductParamsByWhsAndClientValidation(WhsProductParamsByWhsAndClient parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion

		#region Implementation

		WhsProductParamsByWhsAndClient Parent
		{
			get { return parent ?? (parent = Factory.New<WhsProductParamsByWhsAndClient>()); }
			set { parent = value; }
		}

		WhsProductParamsByWhsAndClient parent;

		#endregion
	}
}
