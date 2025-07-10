using System;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	internal class WhsClientParameterByWarehouseValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWY_WW_Whs

		public void TestCheckWY_ReceiveCategory()
		{
			WarehouseDataRegistry.Instance.ReceiveCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetReceiveCategories());

			var expectedErrorMessage = "Enter a valid Receive Category.";
			var client = Helper.CreateClient("C1");
			var whs1 = Helper.CreateWarehouse("Whs1");

			var parameter1 = Factory.New<WhsClientParameterByWarehouse>();
			parameter1.WY_OH_Client = client.PK;
			parameter1.WY_WW_Whs = whs1.PK;

			var invalidCode = "XYZ";
			AssertEquals("Precondition: XYZ is not in the list.", false, parameter1.Lookups.ReceiveCategories.ContainsCode(invalidCode));
			parameter1.WY_ReceiveCategory = invalidCode;
			AssertHasError(parameter1.WY_ReceiveCategoryInfo, expectedErrorMessage);

			parameter1.WY_ReceiveCategory = "RC1";
			AssertNoError(parameter1.WY_ReceiveCategoryInfo, expectedErrorMessage);

			parameter1.WY_ReceiveCategory = "";
			AssertNoError(parameter1.WY_ReceiveCategoryInfo, expectedErrorMessage);
		}

		public void TestCheckWY_PreventReceivingOvers()
		{
			var expectedErrorMessage = "Prevent Receiving Overs must be checked when Receive Overage Tolerance Percent is greater than zero.";
			var client = Helper.CreateClient("C1");
			var whs1 = Helper.CreateWarehouse("Whs1");

			var parameter1 = Factory.New<WhsClientParameterByWarehouse>();
			parameter1.WY_OH_Client = client.PK;
			parameter1.WY_WW_Whs = whs1.PK;
			parameter1.WY_ReceiveOverageTolerancePercent = 10;
			parameter1.WY_PreventReceivingOvers = false;

			AssertHasError(parameter1.WY_PreventReceivingOversInfo, expectedErrorMessage);

			parameter1.WY_PreventReceivingOvers = true;
			AssertNoError(parameter1.WY_PreventReceivingOversInfo, expectedErrorMessage);
		}

		public void TestCheckWY_ReceiveOverageTolerancePercent()
		{
			var expectedErrorMessage = "Please enter a 'Receive Overage Tolerance Percent' within the range 0 to 500.";
			var client = Helper.CreateClient("C1");
			var whs1 = Helper.CreateWarehouse("Whs1");

			var parameter1 = Factory.New<WhsClientParameterByWarehouse>();
			parameter1.WY_OH_Client = client.PK;
			parameter1.WY_WW_Whs = whs1.PK;
			parameter1.WY_ReceiveOverageTolerancePercent = -1;

			AssertHasError(parameter1.WY_ReceiveOverageTolerancePercentInfo, expectedErrorMessage);

			parameter1.WY_ReceiveOverageTolerancePercent = 0;
			AssertNoError(parameter1.WY_ReceiveOverageTolerancePercentInfo, expectedErrorMessage);

			parameter1.WY_ReceiveOverageTolerancePercent = 1;
			AssertNoError(parameter1.WY_ReceiveOverageTolerancePercentInfo, expectedErrorMessage);

			parameter1.WY_ReceiveOverageTolerancePercent = 500;
			AssertNoError(parameter1.WY_ReceiveOverageTolerancePercentInfo, expectedErrorMessage);

			parameter1.WY_ReceiveOverageTolerancePercent = 501;
			AssertHasError(parameter1.WY_ReceiveOverageTolerancePercentInfo, expectedErrorMessage);
		}

		public void TestCheckClient_Warehouse_Category_IsUnique()
		{
			WarehouseDataRegistry.Instance.ReceiveCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetReceiveCategories());

			var expectedErrorMessage = "Warehouse and Receive Category should be unique.";
			var client = Helper.CreateClient("C1");
			var whs1 = Helper.CreateWarehouse("Whs1");

			var parameter1 = Factory.New<WhsClientParameterByWarehouse>();
			parameter1.WY_OH_Client = client.PK;
			parameter1.WY_WW_Whs = whs1.PK;
			parameter1.WY_ReceiveCategory = "RC1";
			AssertNoError(parameter1.WY_OH_ClientInfo, expectedErrorMessage);
			AssertNoError(parameter1.WY_WW_WhsInfo, expectedErrorMessage);
			AssertNoError(parameter1.WY_ReceiveCategoryInfo, expectedErrorMessage);

			var parameter2 = Factory.New<WhsClientParameterByWarehouse>();
			parameter2.WY_OH_Client = client.PK;
			parameter2.WY_WW_Whs = whs1.PK;
			parameter2.WY_ReceiveCategory = "RC1";
			AssertHasError(parameter2.WY_ReceiveCategoryInfo, expectedErrorMessage);

			parameter2.WY_ReceiveCategory = "RC2";
			AssertNoError(parameter1.WY_OH_ClientInfo, expectedErrorMessage);
			AssertNoError(parameter1.WY_WW_WhsInfo, expectedErrorMessage);
			AssertNoError(parameter1.WY_ReceiveCategoryInfo, expectedErrorMessage);
			AssertNoError(parameter2.WY_OH_ClientInfo, expectedErrorMessage);
			AssertNoError(parameter2.WY_WW_WhsInfo, expectedErrorMessage);
			AssertNoError(parameter2.WY_ReceiveCategoryInfo, expectedErrorMessage);

			parameter2.WY_ReceiveCategory = "RC1";
			parameter2.WY_OH_Client = ZGuid.NewZGuid();
			AssertNoError(parameter2.WY_OH_ClientInfo, expectedErrorMessage);
			AssertNoError(parameter2.WY_WW_WhsInfo, expectedErrorMessage);
			AssertNoError(parameter2.WY_ReceiveCategoryInfo, expectedErrorMessage);

			parameter2.WY_OH_Client = client.PK;
			AssertHasError(parameter2.WY_OH_ClientInfo, expectedErrorMessage);

			parameter2.WY_WW_Whs = ZGuid.NewZGuid();
			AssertNoError(parameter2.WY_OH_ClientInfo, expectedErrorMessage);
			AssertNoError(parameter2.WY_WW_WhsInfo, expectedErrorMessage);
			AssertNoError(parameter2.WY_ReceiveCategoryInfo, expectedErrorMessage);

			parameter2.WY_WW_Whs = whs1.PK;
			parameter2.WY_OH_Client = client.PK;
			parameter2.WY_ReceiveCategory = "";
			AssertNoError(parameter2.WY_OH_ClientInfo, expectedErrorMessage);
			AssertNoError(parameter2.WY_WW_WhsInfo, expectedErrorMessage);
			AssertNoError(parameter2.WY_ReceiveCategoryInfo, expectedErrorMessage);

			parameter1.WY_ReceiveCategory = "";
			AssertHasError(parameter1.WY_ReceiveCategoryInfo, expectedErrorMessage);

			parameter1.WY_OH_Client = ZGuid.NewZGuid();
			AssertNoError(parameter1.WY_OH_ClientInfo, expectedErrorMessage);
			AssertNoError(parameter1.WY_WW_WhsInfo, expectedErrorMessage);
			AssertNoError(parameter1.WY_ReceiveCategoryInfo, expectedErrorMessage);
		}

		static SystemDefinableCodeDescriptionBoolCollection GetReceiveCategories()
		{
			return new SystemDefinableCodeDescriptionBoolCollection
			{
				{ "RC1", (NoResString)"Receive Category 1", false },
				{ "RC2", (NoResString)"Receive Category 2", false }
			};
		}

		#endregion
	}
}
