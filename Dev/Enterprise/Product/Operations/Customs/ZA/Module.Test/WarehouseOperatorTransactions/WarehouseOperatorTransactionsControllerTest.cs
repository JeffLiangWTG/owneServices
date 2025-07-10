using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	class WarehouseOperatorTransactionsControllerTest : ZControllerTest
	{
		public void TestSecurityCheckpoints()
		{
			var controller = new WarehouseOperatorTransactionsController();
			var message = Factory.New<CusWHSOperatorTransaction>();
			var expectedCheckpoint = Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAWarehouseOperatorTransactions);
			AssertEquals(expectedCheckpoint, controller.GetCheckPointForView(message));
			AssertEquals(expectedCheckpoint, controller.GetCheckPointForNew(message));
			AssertEquals(expectedCheckpoint, controller.GetCheckPointForEdit(message));
			AssertEquals(expectedCheckpoint, controller.GetCheckPointForDelete(message));
		}
	}

	[TestedType(typeof(WarehouseOperatorTransactionsController))]
	class WarehouseOperatorTransactionsControllerBasherTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ZAControllerIDs.WarehouseOperatorTransactions;

		public override Type ControllerToBashType => typeof(WarehouseOperatorTransactionsController);

		protected override string CountryCode => Core.Constants.CountryCodes.SouthAfrica;

		protected override Type GetBusinessObjectType() => typeof(CusWHSOperatorTransaction);

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_Code = "NRW";
			var testAddress1 = testOrg1.Addresses.AddNew();
			testAddress1.OA_Address1 = "ADD1";
			testAddress1.OA_Code = "PC1";
			Factory.Save();

			var batch1 = Factory.New<CusWHSOperatorTransactionBatch>();
			batch1.WOB_Batch = "Batch";
			batch1.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch1.WOB_OA_Warehouse = testAddress1.PK;
			batch1.WOB_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			batch1.WOB_SystemCreateUser = "XXX";
			batch1.WOB_SystemLastEditTimeUtc = ZDateTime.BrettsBirthday;
			batch1.WOB_SystemLastEditUser = "XXX";
			Factory.Save();

			var transaction1 = Factory.New<CusWHSOperatorTransaction>();
			transaction1.WOT_TransactionType = "ORD";
			transaction1.WOT_WOB_CusWHSTransactionBatch = batch1.PK;
			transaction1.WOT_Status = "QUE";
			transaction1.WOT_ExportType = "EXP";
			transaction1.WOT_OwnerReference = "OwnerReference";
			transaction1.WOT_OH_ProductOwner = testOrg1.PK;
			transaction1.WOT_Quantity = 1;
			transaction1.WOT_IsCustomsControlled = false;
			transaction1.WOT_TotalValue = 2;
			transaction1.WOT_RX_NKCurrency = "GBP";
			transaction1.WOT_RN_NKOrigin = "";
			transaction1.WOT_BatchLineNo = 3;
			transaction1.WOT_TransactionDate = ZDate.BrettsBirthday;
			transaction1.WOT_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			transaction1.WOT_SystemCreateUser = "XXX";
			transaction1.WOT_SystemLastEditTimeUtc = ZDateTime.BrettsBirthday;
			transaction1.WOT_SystemLastEditUser = "XXX";
			Factory.Save();
			return transaction1;
		}
	}
}
