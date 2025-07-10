using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsInventoryHeldCodeValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckWHC_Code

		#region TestCheckWHC_Code_ChangeSystemWideHoldCode

		public void TestCheckWHC_Code_ChangeSystemWideHoldCode()
		{
			var code = Factory.NewWithValidTestData<WhsInventoryHeldCode>();
			code.WHC_Code = string.Empty;
			AssertHasError(code.WHC_CodeInfo, "Please enter a Code.");

			code.WHC_Code = "123";
			AssertNoErrors(code.WHC_CodeInfo);

			code.WHC_Code = "AAA";
			AssertNoErrors(code.WHC_CodeInfo);

			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = iHelper.CreateClient("WHS1TST");
			var part = iHelper.CreateProduct(orgPK, "P1");
			var whs = iHelper.CreateWarehouse("W1", "A", 2, 2);
			Factory.Save();
			var receivePK = iHelper.CreateWhsReceive(orgPK, whs.PK, "1", new TestNotificationBuffer());
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1-1", code.WHC_Code);
			Factory.Save();

			code.WHC_Code = "AAA-1";
			AssertHasWarning(code.WHC_CodeInfo, "Inventory exists with Hold Code AAA. Changing the Hold Code will not update existing Inventory.");
			Factory.Save();

			var newCode = (new BusinessObjectFactory { RefreshEnabled = false }).NewWithValidTestData<WhsInventoryHeldCode>();
			newCode.WHC_Code = "AAA-1";
			AssertHasError(newCode.WHC_CodeInfo, "Hold Code AAA-1 already exists.");
		}

		#endregion

		#region TestCheckWHC_Code_ChangeSystemWideHoldCode_OriginalHoldCodeIsUsedByInventory_ChangedHoldCodeIsUsedByClient

		public void TestCheckWHC_Code_ChangeSystemWideHoldCode_OriginalHoldCodeIsUsedByInventory_ChangedHoldCodeIsUsedByClient()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = iHelper.CreateClient("C1");
			var code1 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for system");
			iHelper.CreateInventoryHeldCode("BBB", "BBB for client 1", client1);

			var part = iHelper.CreateProduct(client1, "P1");
			var whs = iHelper.CreateWarehouse("W1", "A", 2, 2);
			Factory.Save();

			var receivePK = iHelper.CreateWhsReceive(client1, whs.PK, "1", new TestNotificationBuffer());
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1-1", code1.WHC_Code);
			Factory.Save();

			code1.WHC_Code = "BBB";
			AssertHasWarning(code1.WHC_CodeInfo, "Inventory exists with Hold Code AAA. Changing the Hold Code will not update existing Inventory.");
			AssertHasError(code1.WHC_CodeInfo, "Hold Code BBB already exists.");
		}

		#endregion

		#region TestCheckWHC_Code_ChangeSystemWideHoldCode_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsUsedByClient

		public void TestCheckWHC_Code_ChangeSystemWideHoldCode_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsUsedByClient()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = iHelper.CreateClient("C1");
			var code1 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for system");
			iHelper.CreateInventoryHeldCode("BBB", "BBB for client 1", client1);

			code1.WHC_Code = "BBB";
			AssertNoWarnings(code1.WHC_CodeInfo);
			AssertHasError(code1.WHC_CodeInfo, "Hold Code BBB already exists.");
		}

		#endregion

		#region TestCheckWHC_Code_ChangeSystemWideHoldCode_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsUsedBySystem

		public void TestCheckWHC_Code_ChangeSystemWideHoldCode_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsUsedBySystem()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = iHelper.CreateClient("C1");
			var code1 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for system");
			var code2 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("BBB", "BBB for system");
			var code3 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("DDD", "DDD for client 1", client1);

			var part = iHelper.CreateProduct(client1, "P1");
			var whs = iHelper.CreateWarehouse("W1", "A", 2, 2);
			Factory.Save();

			var receive1PK = iHelper.CreateWhsReceive(client1, whs.PK, "1", new TestNotificationBuffer());
			iHelper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 10m, "A-1-1", code2.WHC_Code);
			var receive2PK = iHelper.CreateWhsReceive(client1, whs.PK, "2", new TestNotificationBuffer());
			iHelper.CreateWhsReceiveInventoryLine(receive2PK, part.PK, 10m, "A-1-1", code3.WHC_Code);
			Factory.Save();

			code1.WHC_Code = "BBB";
			AssertNoWarnings(code1.WHC_CodeInfo);
			AssertHasError(code1.WHC_CodeInfo, "Hold Code BBB already exists.");
		}

		#endregion

		#region TestCheckWHC_Code_ChangeSystemWideHoldCode_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsNotUsedBySystemOrClient

		public void TestCheckWHC_Code_ChangeSystemWideHoldCode_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsNotUsedBySystemOrClient()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = iHelper.CreateClient("C1");
			var code1 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for system");
			var code2 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("BBB", "BBB for client 1", client1);
			var code3 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("EEE", "EEE for system");

			var part = iHelper.CreateProduct(client1, "P1");
			var whs = iHelper.CreateWarehouse("W1", "A", 2, 2);
			Factory.Save();

			var receive1PK = iHelper.CreateWhsReceive(client1, whs.PK, "1", new TestNotificationBuffer());
			iHelper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 10m, "A-1-1", code2.WHC_Code);
			var receive2PK = iHelper.CreateWhsReceive(client1, whs.PK, "2", new TestNotificationBuffer());
			iHelper.CreateWhsReceiveInventoryLine(receive2PK, part.PK, 10m, "A-1-1", code3.WHC_Code);
			Factory.Save();

			code1.WHC_Code = "DDD";
			AssertNoWarnings(code1.WHC_CodeInfo);
			AssertNoErrors(code1.WHC_CodeInfo);
			Factory.Save();

			var newCode = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("DDD", "DDD for system");
			AssertHasError(newCode.WHC_CodeInfo, "Hold Code DDD already exists.");
		}

		#endregion

		#region TestCheckWHC_Code_ChangeClientSpecificHoldCode_OriginalHoldCodeIsUsedByInventory_ChangedHoldCodeIsUsedBySameClient

		public void TestCheckWHC_Code_ChangeClientSpecificHoldCode_OriginalHoldCodeIsUsedByInventory_ChangedHoldCodeIsUsedBySameClient()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = iHelper.CreateClient("C1");
			var code1 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for client 1", client1);
			iHelper.CreateInventoryHeldCode("BBB", "BBB for client 1", client1);

			var part = iHelper.CreateProduct(client1, "P1");
			var whs = iHelper.CreateWarehouse("W1", "A", 2, 2);
			Factory.Save();

			var receive1PK = iHelper.CreateWhsReceive(client1, whs.PK, "1", new TestNotificationBuffer());
			iHelper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 10m, "A-1-1", code1.WHC_Code);
			Factory.Save();

			code1.WHC_Code = "BBB";
			AssertHasWarning(code1.WHC_CodeInfo, "Inventory exists with Hold Code AAA. Changing the Hold Code will not update existing Inventory.");
			AssertHasError(code1.WHC_CodeInfo, "Hold Code BBB already exists.");
		}

		#endregion

		#region TestCheckWHC_Code_ChangeClientSpecificHoldCode_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsUsedBySameClient

		public void TestCheckWHC_Code_ChangeClientSpecificHoldCode_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsUsedBySameClient()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = iHelper.CreateClient("C1");
			var code1 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for client 1", client1);
			var code2 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("BBB", "BBB for client 1", client1);

			var part = iHelper.CreateProduct(client1, "P1");
			var whs = iHelper.CreateWarehouse("W1", "A", 2, 2);
			Factory.Save();

			var receive1PK = iHelper.CreateWhsReceive(client1, whs.PK, "1", new TestNotificationBuffer());
			iHelper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 10m, "A-1-1", code2.WHC_Code);
			Factory.Save();

			code1.WHC_Code = "BBB";
			AssertNoWarnings(code1.WHC_CodeInfo);
			AssertHasError(code1.WHC_CodeInfo, "Hold Code BBB already exists.");
		}

		#endregion

		#region TestCheckWHC_Code_ChangeClientSpecificHoldCode_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsUsedBySystem

		public void TestCheckWHC_Code_ChangeClientSpecificHoldCode_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsUsedBySystem()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = iHelper.CreateClient("C1");
			var code1 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for client 1", client1);
			var code2 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("BBB", "BBB for client 1", client1);
			var code3 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("DDD", "DDD for system");
			AssertNoWarnings(code3.WHC_CodeInfo);
			AssertNoErrors(code3.WHC_CodeInfo);
			AssertNoWarnings(code3.WHC_OH_ClientInfo);
			AssertNoErrors(code3.WHC_OH_ClientInfo);

			var part = iHelper.CreateProduct(client1, "P1");
			var whs = iHelper.CreateWarehouse("W1", "A", 2, 2);
			Factory.Save();

			var receive1PK = iHelper.CreateWhsReceive(client1, whs.PK, "1", new TestNotificationBuffer());
			iHelper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 10m, "A-1-1", code2.WHC_Code);
			iHelper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 10m, "A-1-1", code3.WHC_Code);
			Factory.Save();

			code1.WHC_Code = "DDD";
			AssertNoWarnings(code1.WHC_CodeInfo);
			AssertHasError(code1.WHC_CodeInfo, "Hold Code DDD already exists.");
		}

		#endregion

		#region TestCheckWHC_Code_ChangeClientSpecificHoldCode_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsUsedByOtherClient

		public void TestCheckWHC_Code_ChangeClientSpecificHoldCode_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsUsedByOtherClient()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = iHelper.CreateClient("C1");
			var client2 = iHelper.CreateClient("C2");
			var code1 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for client 1", client1);
			var code2 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("BBB", "BBB for client 1", client1);
			var code3 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("DDD", "DDD for client 2", client2);
			var code4 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("EEE", "EEE for system");

			var part = iHelper.CreateProduct(client1, "P1");
			var whs = iHelper.CreateWarehouse("W1", "A", 2, 2);
			Factory.Save();

			var receive1PK = iHelper.CreateWhsReceive(client1, whs.PK, "1", new TestNotificationBuffer());
			iHelper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 10m, "A-1-1", code2.WHC_Code);
			iHelper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 10m, "A-1-1", code3.WHC_Code);
			iHelper.CreateWhsReceiveInventoryLine(receive1PK, part.PK, 10m, "A-1-1", code4.WHC_Code);
			Factory.Save();

			code1.WHC_Code = "DDD";
			AssertNoWarnings(code1.WHC_CodeInfo);
			AssertHasError(code1.WHC_CodeInfo, "Hold Code DDD already exists.");
		}

		#endregion

		#endregion

		#region TestCheckWHC_Code_ShouldNotContainPipe

		public void TestCheckWHC_Code_ShouldNotContainPipe()
		{
			var holdCode = Factory.New<WhsInventoryHeldCode>();
			AssertNoErrors("Precondition:", holdCode.WHC_CodeInfo);

			holdCode.WHC_Code = "CHA|SHA";
			AssertHasError(holdCode.WHC_CodeInfo, "Cannot use the '|' character in your Hold Code.");
		}

		#endregion

		#region TestCheckWHC_Description

		public void TestCheckWHC_Description()
		{
			var heldCode = Factory.New<WhsInventoryHeldCode>();
			AssertNoErrors("Precondition:", heldCode.WHC_DescriptionInfo);

			heldCode.WHC_Description = "";
			AssertHasError(heldCode.WHC_DescriptionInfo, "Please enter a Description.");

			heldCode.WHC_Description = "Blah";
			AssertNoErrors(heldCode.WHC_DescriptionInfo);
		}

		#endregion

		#region TestCheckWHC_OH_Client

		#region TestCheckWHC_OH_Client_ChangeSystemWideToClientSpecific_OriginalHoldCodeIsUsedByInventory

		public void TestCheckWHC_OH_Client_ChangeNullClientToClientSpecific_OriginalHoldCodeIsUsedByInventory()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = iHelper.CreateClient("C1");
			var code1 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for system");
			iHelper.CreateInventoryHeldCode("BBB", "BBB for client 1", client1);

			var part = iHelper.CreateProduct(client1, "P1");
			var whs = iHelper.CreateWarehouse("W1", "A", 2, 2);
			Factory.Save();

			var receivePK = iHelper.CreateWhsReceive(client1, whs.PK, "1", new TestNotificationBuffer());
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1-1", code1.WHC_Code);
			Factory.Save();

			code1.WHC_OH_Client = client1;
			AssertHasWarning(code1.WHC_OH_ClientInfo, "Inventory exists with Hold Code AAA. Changing the Hold Code will not update existing Inventory.");
			AssertNoErrors(code1.WHC_OH_ClientInfo);
			Factory.Save();

			var newCode = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for client 1", client1);
			AssertHasError(newCode.WHC_CodeInfo, "Hold Code AAA already exists.");
		}

		#endregion

		#region TestCheckWHC_OH_Client_ChangeSystemWideToClientSpecific_OriginalHoldCodeIsNotUsedByInventory

		public void TestCheckWHC_OH_Client_ChangeSystemWideToClientSpecific_OriginalHoldCodeIsNotUsedByInventory()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = iHelper.CreateClient("C1");
			var code1 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for system");
			var code2 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("BBB", "BBB for client 1", client1);

			var part = iHelper.CreateProduct(client1, "P1");
			var whs = iHelper.CreateWarehouse("W1", "A", 2, 2);
			Factory.Save();

			var receivePK = iHelper.CreateWhsReceive(client1, whs.PK, "1", new TestNotificationBuffer());
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1-1", code2.WHC_Code);
			Factory.Save();

			code1.WHC_OH_Client = client1;
			AssertNoWarnings(code1.WHC_OH_ClientInfo);
			AssertNoErrors(code1.WHC_OH_ClientInfo);
			Factory.Save();

			var newCode = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for client 1", client1);
			AssertHasError(newCode.WHC_CodeInfo, "Hold Code AAA already exists.");
		}

		#endregion

		#region TestCheckWHC_OH_Client_ChangeOneClientToAnotherClient_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsNotUsedByClient

		public void TestCheckWHC_OH_Client_ChangeOneClientToAnotherClient_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsNotUsedByClient()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = iHelper.CreateClient("C1");
			var client2 = iHelper.CreateClient("C2");
			var code1 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for client 1", client1);
			var code2 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("BBB", "BBB for client 2", client2);

			var part = iHelper.CreateProduct(client2, "P1");
			var whs = iHelper.CreateWarehouse("W1", "A", 2, 2);
			Factory.Save();

			var receivePK = iHelper.CreateWhsReceive(client2, whs.PK, "1", new TestNotificationBuffer());
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1-1", code2.WHC_Code);
			Factory.Save();

			code1.WHC_OH_Client = client2;
			AssertNoWarnings(code1.WHC_OH_ClientInfo);
			AssertNoErrors(code1.WHC_OH_ClientInfo);
			Factory.Save();

			var newCode = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for client 2", client2);
			AssertHasError(newCode.WHC_CodeInfo, "Hold Code AAA already exists.");
		}

		#endregion

		#region TestCheckWHC_OH_Client_ChangeOneClientToSystemWide_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsNotUsedByAnotherClient

		public void TestCheckWHC_OH_Client_ChangeOneClientToSystemWide_OriginalHoldCodeIsNotUsedByInventory_ChangedHoldCodeIsNotUsedByAnotherClient()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = iHelper.CreateClient("C1");
			var client2 = iHelper.CreateClient("C2");
			var code1 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for client 1", client1);
			var code2 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("BBB", "BBB for system");
			var code3 = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("DDD", "DDD for client 2", client2);

			var part = iHelper.CreateProduct(client2, "P1");
			var whs = iHelper.CreateWarehouse("W1", "A", 2, 2);
			Factory.Save();

			var receivePK = iHelper.CreateWhsReceive(client2, whs.PK, "1", new TestNotificationBuffer());
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1-1", code2.WHC_Code);
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1-1", code3.WHC_Code);
			Factory.Save();

			code1.WHC_OH_Client = ZGuid.Empty;
			AssertNoWarnings(code1.WHC_OH_ClientInfo);
			AssertNoErrors(code1.WHC_OH_ClientInfo);
			Factory.Save();

			var newCode = (WhsInventoryHeldCode)iHelper.CreateInventoryHeldCode("AAA", "AAA for system");
			AssertHasError(newCode.WHC_CodeInfo, "Hold Code AAA already exists.");
		}

		#endregion

		#endregion
	}
}
