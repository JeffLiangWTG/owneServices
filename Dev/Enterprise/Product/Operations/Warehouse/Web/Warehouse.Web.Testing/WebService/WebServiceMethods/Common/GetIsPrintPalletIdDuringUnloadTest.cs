using System;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetIsPrintPalletIdDuringUnloadTest : WhsSecureServiceTestCase
	{
		#region TestGetIsPrintPalletIdDuringUnload

		public void TestGetIsPrintPalletIdDuringUnload_PerfectMatching_True()
		{
			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var warehouse = Helper.CreateWarehouse("W1");
			var client1 = Helper.CreateClient("Org1");
			var part1 = Helper.CreateProduct(client1, "P1");
			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(client1, warehouse);
			whsClientParameterByWarehouse1.WY_PrintPalletIDDuringUnload = true;
			whsClientParameterByWarehouse1.WY_ReceiveCategory = "RC1";
			var whsClientParameterByWarehouse2 = Helper.CreateWhsClientParameterByWarehouse(client1, warehouse);
			whsClientParameterByWarehouse2.WY_PrintPalletIDDuringUnload = false;
			whsClientParameterByWarehouse2.WY_ReceiveCategory = ZString.Empty;

			var receive1 = Helper.CreateWhsReceive(client1, warehouse);
			receive1.WD_ReceiveCategory = "RC1";
			Helper.CreateWhsReceiveLine(receive1, part1, 1m);

			Helper.Factory.Save();

			var webService1 = GetNewWebService(warehouse);
			var response1 = webService1.GetIsPrintPalletIdDuringUnload(receive1.PK.ToGuid());
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals(true, response1.IsPrintPalletIDDuringUnload);
			AssertEquals("No error.", true, string.IsNullOrEmpty(response1.ErrorMessage));
		}

		public void TestGetIsPrintPalletIdDuringUnload_PerfectMatching_False()
		{
			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var warehouse = Helper.CreateWarehouse("W1");
			var client1 = Helper.CreateClient("Org1");
			var part1 = Helper.CreateProduct(client1, "P1");
			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(client1, warehouse);
			whsClientParameterByWarehouse1.WY_PrintPalletIDDuringUnload = false;
			whsClientParameterByWarehouse1.WY_ReceiveCategory = "RC1";
			var whsClientParameterByWarehouse2 = Helper.CreateWhsClientParameterByWarehouse(client1.PK, ZGuid.Empty);
			whsClientParameterByWarehouse2.WY_PrintPalletIDDuringUnload = true;
			whsClientParameterByWarehouse2.WY_ReceiveCategory = ZString.Empty;

			var receive1 = Helper.CreateWhsReceive(client1, warehouse);
			receive1.WD_ReceiveCategory = "RC1";
			Helper.CreateWhsReceiveLine(receive1, part1, 1m);

			Helper.Factory.Save();

			var webService = GetNewWebService(warehouse);
			var response = webService.GetIsPrintPalletIdDuringUnload(receive1.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);
			AssertEquals(false, response.IsPrintPalletIDDuringUnload);
			AssertEquals("No error.", true, string.IsNullOrEmpty(response.ErrorMessage));
		}

		public void TestGetIsPrintPalletIdDuringUnload_FallbackToDefaultFalse()
		{
			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var warehouse = Helper.CreateWarehouse("W1");
			var client1 = Helper.CreateClient("Org1");
			var part1 = Helper.CreateProduct(client1, "P1");
			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(client1, warehouse);
			whsClientParameterByWarehouse1.WY_PrintPalletIDDuringUnload = true;
			whsClientParameterByWarehouse1.WY_ReceiveCategory = "RC1";

			var receive1 = Helper.CreateWhsReceive(client1, warehouse);
			receive1.WD_ReceiveCategory = "";
			Helper.CreateWhsReceiveLine(receive1, part1, 1m);

			Helper.Factory.Save();

			var webService = GetNewWebService(warehouse);
			var response = webService.GetIsPrintPalletIdDuringUnload(receive1.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);
			AssertEquals("No configs found, fallback to default value", false, response.IsPrintPalletIDDuringUnload);
			AssertEquals("No error.", true, string.IsNullOrEmpty(response.ErrorMessage));
		}

		public void TestGetIsPrintPalletIdDuringUnload_FallbackToEmptyReceiveCategory()
		{
			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var warehouse = Helper.CreateWarehouse("W1");
			var client1 = Helper.CreateClient("Org1");
			var part1 = Helper.CreateProduct(client1, "P1");
			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(client1, warehouse);
			whsClientParameterByWarehouse1.WY_PrintPalletIDDuringUnload = false;
			whsClientParameterByWarehouse1.WY_ReceiveCategory = "RC1";
			var whsClientParameterByWarehouse2 = Helper.CreateWhsClientParameterByWarehouse(client1, warehouse);
			whsClientParameterByWarehouse2.WY_PrintPalletIDDuringUnload = true;
			whsClientParameterByWarehouse2.WY_ReceiveCategory = string.Empty;

			var receive1 = Helper.CreateWhsReceive(client1, warehouse);
			receive1.WD_ReceiveCategory = "XYZ";
			Helper.CreateWhsReceiveLine(receive1, part1, 1m);

			Helper.Factory.Save();

			var webService = GetNewWebService(warehouse);
			var response = webService.GetIsPrintPalletIdDuringUnload(receive1.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Config for Receive Category XYZ not exists, fallback to empty Receive Category", true, response.IsPrintPalletIDDuringUnload);
			AssertEquals("No error.", true, string.IsNullOrEmpty(response.ErrorMessage));
		}

		public void TestGetIsPrintPalletIdDuringUnload_InvalidReceivePK()
		{
			var warehouse = Helper.CreateWarehouse("W1");
			Helper.Factory.Save();

			var webService = GetNewWebService(warehouse);
			var response = webService.GetIsPrintPalletIdDuringUnload(new Guid());
			AssertEquals("Invalid receive PK", response.ErrorMessage);
			AssertEquals(false, response.IsPrintPalletIDDuringUnload);
		}

		public void TestGetIsPrintPalletIdDuringUnload_ValidReceiveWithNoWhsClientParameterByWarehouseConfigured()
		{
			var warehouse = Helper.CreateWarehouse("W1");
			var client1 = Helper.CreateClient("Org1");
			var part1 = Helper.CreateProduct(client1, "P1");

			var receive1 = Helper.CreateWhsReceive(client1, warehouse);
			receive1.WD_ReceiveCategory = "RC1";
			Helper.CreateWhsReceiveLine(receive1, part1, 1m);

			Helper.Factory.Save();

			var webService = GetNewWebService(warehouse);
			var response = webService.GetIsPrintPalletIdDuringUnload(receive1.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);
			AssertEquals(false, response.IsPrintPalletIDDuringUnload);
			AssertEquals("No error.", true, string.IsNullOrEmpty(response.ErrorMessage));
		}

		public void TestGetIsPrintPalletIdDuringUnload_FallbackToEmptyWarehouse()
		{
			// Arrange
			var warehouse = Helper.CreateWarehouse("W1");
			var client1 = Helper.CreateClient("Org1");
			var part1 = Helper.CreateProduct(client1, "P1");

			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(client1, warehouse);
			whsClientParameterByWarehouse1.WY_WW_Whs = ZGuid.Empty;
			whsClientParameterByWarehouse1.WY_PrintPalletIDDuringUnload = true;

			var receive1 = Helper.CreateWhsReceive(client1, warehouse);
			receive1.WD_ReceiveCategory = "RC1";
			Helper.CreateWhsReceiveLine(receive1, part1, 1m);

			Helper.Factory.Save();

			// Act
			var webService = GetNewWebService(warehouse);
			var response = webService.GetIsPrintPalletIdDuringUnload(receive1.PK.ToGuid());

			// Assert
			AssertSuccessfulResponse(response, webService);
			AssertEquals(true, response.IsPrintPalletIDDuringUnload);
			AssertEquals("No error.", true, string.IsNullOrEmpty(response.ErrorMessage));
		}

		#endregion
	}
}
