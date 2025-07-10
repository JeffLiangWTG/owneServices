using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI
{
	[TestedType(typeof(FindBatchModulePopup))]
	class FindBatchModulePopupTest : ZFormBasherTest
	{
		FindBatchModulePopup GetPopupForm(FindBatchModulePopup.Operation operation)
		{
			var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ZAModuleIDs.WarehouseOperatorTransactions);
			return new FindBatchModulePopup(module, operation);
		}

		protected override Form GetFormToBashCore() => GetPopupForm(FindBatchModulePopup.Operation.ReverseBatch);

		[TestDate(2024, 1, 1)]
		public void TestReverseSelectedBatch()
		{
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "WZA");
			var (batch1, order1, receipt1) = CreateBatchWithTransactions(whsWarehouse, "BATCH1");
			var (batch2, order2, _) = CreateBatchWithTransactions(whsWarehouse, "BATCH2");
			order2.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.CLS;

			var transactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine1.WOL_Quantity = 100m;
			transactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order1.PK;
			transactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;

			var transactionLine2 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine2.WOL_Quantity = 200m;
			transactionLine2.WOL_WOT_WHSOperatorTransactionOrder = order2.PK;
			transactionLine2.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;

			Factory.Save();

			using (var form = new ZForm())
			using (var popupForm = GetPopupForm(FindBatchModulePopup.Operation.ReverseBatch))
			{
				popupForm.ShowModal(null, form);
				popupForm.Module_ForTest.PerformSearch_ForTest();
				AssertEquals(4, popupForm.Module_ForTest.GridCollection.Count);

				var userNotification = UnitTestUserNotification.Instance;
				userNotification.ClearMessagesAndAnswers();
				popupForm.Module_ForTest.DisplayGrid.Select(0);
				popupForm.Module_ForTest.DisplayGrid.Select(1);
				popupForm.ExposedOKButtonForTesting.PerformClick();
				AssertEquals("Please select only one transaction from the grid.", userNotification.LastMessage.Text);

				popupForm.Module_ForTest.DisplayGrid.UnSelect(1);
				popupForm.ExposedOKButtonForTesting.PerformClick();
				userNotification.AddUserResponse("yes");
				AssertEquals("Reversal completed!", userNotification.LastMessage.Text);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				batch1 = newFactory.Load<CusWHSOperatorTransactionBatch>(batch1.PK);
				var newOrder1 = batch1.WarehouseOperatorTransactions[0];
				var newReceipt1 = batch1.WarehouseOperatorTransactions[1];
				AssertEquals("Batch name with reverse notes", $"BATCH1 Reversed by {GlbStaff.CurrentUser.GS_Code} on 01 Jan 2024 00:00:00", batch1.WOB_Batch);
				AssertEquals(WarehouseOperatorTransactionStatusList.Codes.CAN, newOrder1.WOT_Status);
				AssertEquals(WarehouseOperatorTransactionStatusList.Codes.CAN, newReceipt1.WOT_Status);
				AssertEquals(0, newOrder1.TransactionLines.Count);
				AssertEquals(0, newReceipt1.TransactionLines.Count);

				batch2 = newFactory.Load<CusWHSOperatorTransactionBatch>(batch2.PK);
				var newOrder2 = batch2.WarehouseOperatorTransactions[0];
				var newReceipt2 = batch2.WarehouseOperatorTransactions[1];
				AssertEquals("Batch name is not affected", "BATCH2", batch2.WOB_Batch);
				AssertEquals(WarehouseOperatorTransactionStatusList.Codes.VAL, newOrder2.WOT_Status);
				AssertEquals(WarehouseOperatorTransactionStatusList.Codes.VAL, newReceipt2.WOT_Status);
				AssertEquals(0, newOrder2.TransactionLines.Count);
				AssertEquals(0, newReceipt2.TransactionLines.Count);
			}
		}

		public void TestResetBatchReceipt()
		{
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "WZA");
			var (batch1, order1, receipt1) = CreateBatchWithTransactions(whsWarehouse, "BATCH1");
			var (batch2, order2, receipt2) = CreateBatchWithTransactions(whsWarehouse, "BATCH2");
			order1.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.CLS;
			receipt1.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.CLS;
			order2.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.CLS;
			receipt2.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.CLS;

			var transactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine1.WOL_Quantity = 100m;
			transactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order1.PK;
			transactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;

			var transactionLine2 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine2.WOL_Quantity = 200m;
			transactionLine2.WOL_WOT_WHSOperatorTransactionOrder = order2.PK;
			transactionLine2.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;

			var transactionLine3 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine3.WOL_Quantity = 300m;
			transactionLine3.WOL_WOT_WHSOperatorTransactionOrder = order2.PK;
			transactionLine3.WOL_WOT_WHSOperatorTransactionReceipt = receipt2.PK;

			Factory.Save();

			using (var form = new ZForm())
			using (var popupForm = GetPopupForm(FindBatchModulePopup.Operation.ResetBatchReceipt))
			{
				var userNotification = UnitTestUserNotification.Instance;
				userNotification.ClearMessagesAndAnswers();
				popupForm.ShowModal(null, form);
				AssertEquals("Please check allocations for BLN and EXP Orders before selecting the batch.", userNotification.LastMessage.Text);

				popupForm.Module_ForTest.PerformSearch_ForTest();
				AssertEquals(4, popupForm.Module_ForTest.GridCollection.Count);

				popupForm.Module_ForTest.DisplayGrid.Select(0);
				popupForm.Module_ForTest.DisplayGrid.Select(1);
				popupForm.ExposedOKButtonForTesting.PerformClick();
				AssertEquals("Please select only one transaction from the grid.", userNotification.LastMessage.Text);

				popupForm.Module_ForTest.DisplayGrid.UnSelect(1);
				popupForm.ExposedOKButtonForTesting.PerformClick();
				AssertEquals("Script successfully executed.", userNotification.LastMessage.Text);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				batch1 = newFactory.Load<CusWHSOperatorTransactionBatch>(batch1.PK);
				var newOrder1 = batch1.WarehouseOperatorTransactions[0];
				var newReceipt1 = batch1.WarehouseOperatorTransactions[1];
				AssertEquals(WarehouseOperatorTransactionStatusList.Codes.VAL, newOrder1.WOT_Status);
				AssertEquals(WarehouseOperatorTransactionStatusList.Codes.QUE, newReceipt1.WOT_Status);
				AssertEquals(0, newOrder1.TransactionLines.Count);
				AssertEquals(0, newReceipt1.TransactionLines.Count);

				batch2 = newFactory.Load<CusWHSOperatorTransactionBatch>(batch2.PK);
				var newOrder2 = batch2.WarehouseOperatorTransactions[0];
				var newReceipt2 = batch2.WarehouseOperatorTransactions[1];
				AssertEquals(WarehouseOperatorTransactionStatusList.Codes.VAL, newOrder2.WOT_Status);
				AssertEquals(WarehouseOperatorTransactionStatusList.Codes.CLS, newReceipt2.WOT_Status);
				AssertEquals(1, newOrder2.TransactionLines.Count);
				AssertEquals(1, newReceipt2.TransactionLines.Count);
			}
		}

		(CusWHSOperatorTransactionBatch, CusWHSOperatorTransaction, CusWHSOperatorTransaction) CreateBatchWithTransactions(IWhsWarehouse whsWarehouse, ZString batchName)
		{
			var batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = batchName;
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = whsWarehouse.WW_OA_WarehouseAddress;

			var order = Factory.New<CusWHSOperatorTransaction>();
			order.WOT_BatchLineNo = 1;
			order.WOT_ExportType = "";
			order.WOT_IsCustomsControlled = false;
			order.WOT_OH_ProductOwner = Helper.Importer.PK;
			order.WOT_OP_Product = Helper.Part.PK;
			order.WOT_OwnerReference = "ORDER1";
			order.WOT_Quantity = 350m;
			order.WOT_RN_NKOrigin = "";
			order.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			order.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			order.WOT_TotalValue = 1000m;
			order.WOT_TransactionDate = ZDate.Today;
			order.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			order.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			var receipt = Factory.New<CusWHSOperatorTransaction>();
			receipt.WOT_BatchLineNo = 2;
			receipt.WOT_ExportType = "";
			receipt.WOT_IsCustomsControlled = true;
			receipt.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt.WOT_OP_Product = Helper.Part.PK;
			receipt.WOT_OwnerReference = "RECEIPT1";
			receipt.WOT_Quantity = 350m;
			receipt.WOT_RN_NKOrigin = Core.Constants.CountryCodes.SouthAfrica;
			receipt.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			receipt.WOT_TotalValue = 1000m;
			receipt.WOT_TransactionDate = ZDate.Today.AddDays(-1);
			receipt.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt.WOT_WOB_CusWHSTransactionBatch = batch.PK;
			receipt.WOT_CustomsEntryNumber = "EN00123-1";

			return (batch, order, receipt);
		}

		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
		WhsDataTestHelper helper;
	}
}
