using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsProductParamsByWhsAndClientValidationHelperTest : WhsTestCaseWithFactory
	{
		#region TestCheckMaximumShelfLifeIsValidWhenHasStock

		#region TestCheckMaximumShelfLifeIsValidWhenHasStock_PartParamsIsNull

		public void TestCheckMaximumShelfLifeIsValidWhenHasStock_PartParamsIsNull()
		{
			var helper = new WhsProductParamsByWhsAndClientValidationHelper();
			AssertExceptionThrown<ArgumentNullException>(() =>
				helper.CheckMaximumShelfLifeIsValidWhenHasStock(null, 0));
		}

		#endregion

		#region TestCheckMaximumShelfLifeIsValidWhenHasStock_CannotBeZeroIfStockExist

		public void TestCheckMaximumShelfLifeIsValidWhenHasStock_CannotBeZeroIfStockExist_NormalAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			var productParams1 = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var productParams2 = Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m, true, false);
			Factory.Save();

			var helper = new WhsProductParamsByWhsAndClientValidationHelper();
			AssertEquals(
				"No error when Maximum Shelf Life be changed if no Julian Batch Number attribute is used and no stock.",
				ZString.Empty, helper.CheckMaximumShelfLifeIsValidWhenHasStock(productParams1, 5));
			AssertEquals(
				"No error when Maximum Shelf Life be changed if no Julian Batch Number attribute is used and with stock.",
				ZString.Empty, helper.CheckMaximumShelfLifeIsValidWhenHasStock(productParams2, 5));

			productParams1.W3_MaximumShelfLife = 5;
			productParams2.W3_MaximumShelfLife = 5;
			Factory.Save();

			AssertEquals(
				"No error when Maximum Shelf Life be changed to 0 if no Julian Batch Number attribute is used and no stock.",
				ZString.Empty, helper.CheckMaximumShelfLifeIsValidWhenHasStock(productParams1, 0));
			AssertEquals(
				"No error when Maximum Shelf Life be changed to 0 if no Julian Batch Number attribute is used and with stock.",
				ZString.Empty, helper.CheckMaximumShelfLifeIsValidWhenHasStock(productParams2, 0));
		}

		public void TestCheckMaximumShelfLifeIsValidWhenHasStock_CannotBeZeroIfStockExist_JulianBatchNumberAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One,
				PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			var productParams1 = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var productParams2 = Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m, true, false);
			Factory.Save();

			var expectedErrorMessage =
				"Maximum Shelf Life cannot be 0 when there are stock for this product, client and warehouse currently in use.";
			var helper = new WhsProductParamsByWhsAndClientValidationHelper();
			AssertEquals(
				"No error when Maximum Shelf Life be changed if Julian Batch Number attribute is used and no stock.",
				ZString.Empty, helper.CheckMaximumShelfLifeIsValidWhenHasStock(productParams1, 5));
			AssertEquals(
				"No error when Maximum Shelf Life be changed if Julian Batch Number attribute is used and with stock.",
				ZString.Empty, helper.CheckMaximumShelfLifeIsValidWhenHasStock(productParams2, 5));

			productParams1.W3_MaximumShelfLife = 5;
			productParams2.W3_MaximumShelfLife = 5;
			Factory.Save();

			AssertEquals(
				"No error when Maximum Shelf Life be changed to 0 if Julian Batch Number attribute is used but no stock.",
				ZString.Empty, helper.CheckMaximumShelfLifeIsValidWhenHasStock(productParams1, 0));
			AssertEquals(
				"Has error when Maximum Shelf Life be changed to 0 if Julian Batch Number attribute is used and with stock.",
				expectedErrorMessage, helper.CheckMaximumShelfLifeIsValidWhenHasStock(productParams2, 0));
		}

		#endregion

		#region TestCheckMaximumShelfLifeIsValidWhenHasStock_CannotModified

		public void TestCheckMaximumShelfLifeIsValidWhenHasStock_CannotModified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One,
				PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, part3, AttributeNumber.One, true);

			var productParam_WithoutStock = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var productParam_WithStock = Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);
			var productParam_WithStock_ZeroMaxShelfLife =
				Helper.CreateProductParamsByWhsAndClient(part3, data.Org1, data.Whs1);
			productParam_WithoutStock.W3_MaximumShelfLife = 5;
			productParam_WithStock.W3_MaximumShelfLife = 5;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m, true, false);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", part3, 10m, true, false);
			Factory.Save();

			var expectedErrorMessage =
				PartAttributeValidation.GetThereIsCurrentInventoryUsingJulianBatchNumbersErrorMessage(
					"Maximum Shelf Life");
			var helper = new WhsProductParamsByWhsAndClientValidationHelper();
			AssertEquals(
				"No error when Maximum Shelf Life changed if Julian Batch Number attribute is used but there are no current stock for the Client-Warehouse-Product.",
				ZString.Empty, helper.CheckMaximumShelfLifeIsValidWhenHasStock(productParam_WithoutStock, 10));

			AssertEquals(
				"Has error when Maximum Shelf Life can NOT be changed if Julian Batch Number attribute is used and there are current stock for the Client-Warehouse-Product.",
				expectedErrorMessage, helper.CheckMaximumShelfLifeIsValidWhenHasStock(productParam_WithStock, 10));

			AssertEquals(
				"No error when Maximum Shelf Life be changed if Julian Batch Number attribute is used and there are current stock for the Client-Warehouse-Product and previous value was 0.",
				ZString.Empty,
				helper.CheckMaximumShelfLifeIsValidWhenHasStock(productParam_WithStock_ZeroMaxShelfLife, 10));

			AssertEquals(
				"No error when Maximum Shelf Life be changed if Julian Batch Number attribute is used but there are no current stock for the Client-Warehouse-Product.",
				ZString.Empty, helper.CheckMaximumShelfLifeIsValidWhenHasStock(productParam_WithoutStock, 5));

			AssertEquals(
				"No error when Maximum Shelf Life has no change if Julian Batch Number attribute is used and there are current stock for the Client-Warehouse-Product.",
				ZString.Empty, helper.CheckMaximumShelfLifeIsValidWhenHasStock(productParam_WithStock, 5));

			AssertEquals(
				"No error when Maximum Shelf Life be changed if Julian Batch Number attribute is used and there are current stock for the Client-Warehouse-Product and previous value was 0.",
				ZString.Empty,
				helper.CheckMaximumShelfLifeIsValidWhenHasStock(productParam_WithStock_ZeroMaxShelfLife, 5));
		}

		#endregion

		#endregion

		#region CheckMaximumShelfLifeIsLessThanConsigneeMinShelfLifeAccepted

		public void TestCheckMaximumShelfLifeIsLessThanConsigneeMinShelfLifeAccepted()
		{
			var helper = new WhsProductParamsByWhsAndClientValidationHelper();
			AssertEquals("No error when Maximum Shelf Life not be changed", ZString.Empty,
				helper.CheckMaximumShelfLifeIsLessThanConsigneeMinShelfLifeAccepted(30, 60));
			AssertEquals("No error when Maximum Shelf Life is equal to ConsigneeMinShelfLifeAccepted.", ZString.Empty,
				helper.CheckMaximumShelfLifeIsLessThanConsigneeMinShelfLifeAccepted(30, 30));
			AssertEquals("Has error when Maximum Shelf Life is less than ConsigneeMinShelfLifeAccepted.",
				"Maximum shelf life 15 cannot be less than Minimum Shelf Life 20.",
				helper.CheckMaximumShelfLifeIsLessThanConsigneeMinShelfLifeAccepted(20, 15));
		}

		#endregion
	}
}
