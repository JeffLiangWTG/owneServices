using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusWHSOperatorTransactionLine))]
	sealed class CusWHSOperatorTransactionLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOrder()
		{
			var order = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			order.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			order.WOT_Quantity = 5;
			var receipt1 = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			receipt1.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt1.WOT_Quantity = 10;

			var transactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine1.WOL_Quantity = 1;
			transactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;

			Factory.Save();

			AssertEquals("Order", order, transactionLine1.Order);
		}

		public void TestReceipt()
		{
			var order = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			order.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			order.WOT_Quantity = 5;
			var receipt1 = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			receipt1.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt1.WOT_Quantity = 10;

			var transactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine1.WOL_Quantity = 1;
			transactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;

			Factory.Save();

			AssertEquals("Receipt", receipt1, transactionLine1.Receipt);
		}

		public void TestReceiptReference()
		{
			var order = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			order.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			order.WOT_Quantity = 5;
			var receipt1 = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			receipt1.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt1.WOT_OwnerReference = "RECEIPT1";
			receipt1.WOT_Quantity = 10;

			var transactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine1.WOL_Quantity = 1;
			transactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;
			Factory.Save();

			AssertEquals("ReceiptReference", "RECEIPT1", transactionLine1.ReceiptReference);
		}

		public void TestReceiptQuantity()
		{
			var order = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			order.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			order.WOT_Quantity = 5;
			var receipt1 = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			receipt1.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt1.WOT_OwnerReference = "RECEIPT1";
			receipt1.WOT_Quantity = 10;

			var transactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine1.WOL_Quantity = 1;
			transactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;
			Factory.Save();

			AssertEquals("ReceiptQuantity", 10m, transactionLine1.ReceiptQuantity);
		}

		public void TestOrderReference()
		{
			var order = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			order.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			order.WOT_OwnerReference = "ORDER1";
			order.WOT_Quantity = 5;
			var receipt1 = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			receipt1.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt1.WOT_Quantity = 10;

			var transactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine1.WOL_Quantity = 1;
			transactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;
			Factory.Save();

			AssertEquals("OrderReference", "ORDER1", transactionLine1.OrderReference);
		}

		public void TestOrderQuantity()
		{
			var order = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			order.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			order.WOT_OwnerReference = "ORDER1";
			order.WOT_Quantity = 5;
			var receipt1 = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			receipt1.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt1.WOT_Quantity = 10;

			var transactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine1.WOL_Quantity = 1;
			transactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;
			Factory.Save();

			AssertEquals("OrderQuantity", 5m, transactionLine1.OrderQuantity);
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
			var transactionLine = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine.WOL_WOT_WHSOperatorTransactionOrder = transaction.PK;
			transactionLine.WOL_WOT_WHSOperatorTransactionReceipt = transaction.PK;
			transactionLine.WOL_Quantity = 1;
			return transactionLine;
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
