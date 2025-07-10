using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsClientParameterByWarehouse))]
	internal class WhsClientParameterByWarehouseTest : WhsEnvBusinessObjectTestCase
	{
		#region Related Business Objects

		public void TestWarehouse()
		{
			var clientParams = (WhsClientParameterByWarehouse)GetNewBusinessObject();
			var warehouse = Factory.New<WhsWarehouse>();
			clientParams.WY_WW_Whs = warehouse.PK;
			AssertEquals(warehouse, clientParams.Warehouse);
		}

		#endregion

		#region Properties

		public void TestWY_OH_Client()
		{
			var clientParams = (WhsClientParameterByWarehouse)GetNewBusinessObject();
			ZGuid guid1 = ZGuid.NewZGuid();
			clientParams.WY_OH_Client = guid1;
			AssertEquals(guid1, clientParams.WY_OH_Client);

			ZGuid guid2 = ZGuid.NewZGuid();
			clientParams.WY_OH_Client = guid2;
			AssertEquals(guid2, clientParams.WY_OH_Client);
		}

		public void TestWY_WW_Whs()
		{
			var clientParams = (WhsClientParameterByWarehouse)GetNewBusinessObject();
			ZGuid guid1 = ZGuid.NewZGuid();
			clientParams.WY_WW_Whs = guid1;
			AssertEquals(guid1, clientParams.WY_WW_Whs);

			ZGuid guid2 = ZGuid.NewZGuid();
			clientParams.WY_WW_Whs = guid2;
			AssertEquals(guid2, clientParams.WY_WW_Whs);
		}

		public void TestWY_PrintPalletIDDuringUnload()
		{
			var clientParams = (WhsClientParameterByWarehouse)GetNewBusinessObject();
			clientParams.WY_PrintPalletIDDuringUnload = false;
			AssertEquals(false, clientParams.WY_PrintPalletIDDuringUnload);

			clientParams.WY_PrintPalletIDDuringUnload = true;
			AssertEquals(true, clientParams.WY_PrintPalletIDDuringUnload);
		}

		public void TestWY_EnforcePalletIDs()
		{
			var clientParams = (WhsClientParameterByWarehouse)GetNewBusinessObject();
			clientParams.WY_EnforcePalletIDEntry = true;
			AssertEquals(true, clientParams.WY_EnforcePalletIDEntry);

			clientParams.WY_EnforcePalletIDEntry = false;
			AssertEquals(false, clientParams.WY_EnforcePalletIDEntry);
		}

		#endregion
	}
}
