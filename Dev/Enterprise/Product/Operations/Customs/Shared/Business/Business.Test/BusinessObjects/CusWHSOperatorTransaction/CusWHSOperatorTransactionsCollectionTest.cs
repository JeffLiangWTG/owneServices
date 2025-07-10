using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusWHSOperatorTransactionCollection))]
	public class CusWHSOperatorTransactionsCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CusWHSOperatorTransactionCollection(Factory.New<CusWHSOperatorTransactionBatch>());
		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusWHSOperatorTransaction>();

		public void TestWarehouseOperatorTransactionsBusinessObjectCollection()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_Code = "NRW";
			var testAddress1 = testOrg1.Addresses.AddNew();
			testAddress1.OA_Address1 = "ADD1";
			testAddress1.OA_Code = "PC1";
			Factory.Save();

			var batch1 = CreateBatch("Batch1", testAddress1);
			var batch2 = CreateBatch("Batch2", testAddress1);

			var transaction1 = CreateTransaction(testOrg1, batch1, 1);
			var transaction2 = CreateTransaction(testOrg1, batch2, 1);
			var transaction3 = CreateTransaction(testOrg1, batch2, 2);

			var transaction1Reloaded = Factory.Load<CusWHSOperatorTransaction>(transaction1.PK);
			var transaction2Reloaded = Factory.Load<CusWHSOperatorTransaction>(transaction2.PK);
			var transaction3Reloaded = Factory.Load<CusWHSOperatorTransaction>(transaction3.PK);
			CombineAssertions(() =>
			{
				var testCollection = new CusWHSOperatorTransactionCollection(batch1);
				testCollection.Load();
				AssertEquals(1, testCollection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { transaction1Reloaded }, testCollection);

				var testCollection2 = new CusWHSOperatorTransactionCollection(batch2);
				testCollection2.Load();
				AssertEquals(2, testCollection2.Count);
				AssertContainsExactElementsInAnyOrder(new[] { transaction2Reloaded, transaction3Reloaded }, testCollection2);
			});
		}

		CusWHSOperatorTransaction CreateTransaction(OrgHeader testOrg, CusWHSOperatorTransactionBatch batch, int batchLineNo)
		{
			var transaction = Factory.New<CusWHSOperatorTransaction>();
			transaction.WOT_TransactionType = "ORD";
			transaction.WOT_WOB_CusWHSTransactionBatch = batch.PK;
			transaction.WOT_Status = "QUE";
			transaction.WOT_ExportType = "EXP";
			transaction.WOT_OwnerReference = "OwnerReference";
			transaction.WOT_OH_ProductOwner = testOrg.PK;
			transaction.WOT_Quantity = 1;
			transaction.WOT_IsCustomsControlled = false;
			transaction.WOT_TotalValue = 2;
			transaction.WOT_RX_NKCurrency = "GBP";
			transaction.WOT_RN_NKOrigin = "";
			transaction.WOT_BatchLineNo = batchLineNo;
			transaction.WOT_TransactionDate = ZDate.BrettsBirthday;
			transaction.WOT_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			transaction.WOT_SystemCreateUser = "XXX";
			transaction.WOT_SystemLastEditTimeUtc = ZDateTime.BrettsBirthday;
			transaction.WOT_SystemLastEditUser = "XXX";
			Factory.Save();
			return transaction;
		}

		CusWHSOperatorTransactionBatch CreateBatch(ZString batchID, OrgAddress testAddress)
		{
			var batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = batchID;
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = testAddress.PK;
			batch.WOB_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			batch.WOB_SystemCreateUser = "XXX";
			batch.WOB_SystemLastEditTimeUtc = ZDateTime.BrettsBirthday;
			batch.WOB_SystemLastEditUser = "XXX";
			Factory.Save();
			return batch;
		}
	}
}
