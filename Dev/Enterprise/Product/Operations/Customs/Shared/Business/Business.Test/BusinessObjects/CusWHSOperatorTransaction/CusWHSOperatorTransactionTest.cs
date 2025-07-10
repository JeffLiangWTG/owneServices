using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusWHSOperatorTransaction))]
	sealed class CusWHSOperatorTransactionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var transaction = Factory.New<CusWHSOperatorTransaction>();
			AssertEquals("WOT_RN_NKOrigin", ZString.Empty, transaction.WOT_RN_NKOrigin);
			AssertEquals("WOT_IsCustomsControlled", ZBool.False, transaction.WOT_IsCustomsControlled);
		}

		public void TestIsOrder()
		{
			var transaction = Factory.New<CusWHSOperatorTransaction>();
			transaction.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			AssertEquals("IsOrder should be true", true, transaction.IsOrder);

			transaction.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			AssertEquals("IsOrder should be false", false, transaction.IsOrder);
		}

		public void TestIsReceipt()
		{
			var transaction = Factory.New<CusWHSOperatorTransaction>();
			transaction.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			AssertEquals("IsReceipt should be true", true, transaction.IsReceipt);

			transaction.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			AssertEquals("IsReceipt should be false", false, transaction.IsReceipt);
		}

		public void TestBatch()
		{
			var batch = Factory.NewWithValidTestData<CusWHSOperatorTransactionBatch>();
			var transaction = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			transaction.WOT_Quantity = 1;
			transaction.WOT_WOB_CusWHSTransactionBatch = batch.PK;
			Factory.Save();

			AssertEquals("Batch", batch, transaction.Batch);
		}

		public void TestTransactionLines()
		{
			var order = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			order.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			order.WOT_Quantity = 10;
			var receipt1 = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			receipt1.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt1.WOT_Quantity = 6;
			var receipt2 = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			receipt2.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt2.WOT_Quantity = 4;

			var transactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine1.WOL_Quantity = 1;
			transactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;

			var transactionLine2 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine2.WOL_Quantity = 2;
			transactionLine2.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine2.WOL_WOT_WHSOperatorTransactionReceipt = receipt2.PK;

			Factory.Save();

			var orderTransactionLines = order.TransactionLines;
			AssertEquals("Should be Two CusWHSOperatorTransactionLines", 2, orderTransactionLines.Count);
			AssertEquals("Available Quantity Should be 7", 7m, order.AvailableQuantity);

			var receipt1TransactionLines = receipt1.TransactionLines;
			AssertEquals("Should be One CusWHSOperatorTransactionLines", 1, receipt1TransactionLines.Count);
			AssertEquals("Available Quantity Should be 5", 5m, receipt1.AvailableQuantity);

			var receipt2TransactionLines = receipt2.TransactionLines;
			AssertEquals("Should be One CusWHSOperatorTransactionLines", 1, receipt2TransactionLines.Count);
			AssertEquals("Available Quantity Should be 2", 2m, receipt2.AvailableQuantity);
		}
		public void TestSupportsCloneCore()
		{
			var transaction = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			AssertEquals("Clone support - CusWHSOperatorTransaction", expected: true, transaction.SupportsClone());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = "123";
			batch.WOB_GC_Company = company.PK;
			batch.WOB_OA_Warehouse = orgAddress.PK;
			var transaction = Factory.New<CusWHSOperatorTransaction>();
			transaction.WOT_Quantity = 1;
			transaction.WOT_WOB_CusWHSTransactionBatch = batch.PK;
			transaction.WOT_TransactionDate = ZDate.Today;
			transaction.WOT_OwnerReference = "REF";
			transaction.WOT_OH_ProductOwner = orgHeader.PK;
			return transaction;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}
	}
}
