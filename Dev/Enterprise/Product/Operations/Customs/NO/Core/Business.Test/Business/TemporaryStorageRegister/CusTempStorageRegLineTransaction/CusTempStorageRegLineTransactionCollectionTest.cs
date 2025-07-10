using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineTransactionCollection))]
sealed class CusTempStorageRegLineTransactionCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageRegLineTransactionCollection>
{
	public void TestDefaultValues()
	{
		var line = Factory.New<CusTempStorageRegLine>();
		var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("Opening Balance", CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, transaction1.SRT_TransactionType);

			var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
			AssertEquals("Adjustment", CusTempStorageRegLineTransactionTypeList.Codes.Adjustment, transaction2.SRT_TransactionType);
		});
	}

	protected override CusTempStorageRegLineTransactionCollection GetCollectionToTest() => new CusTempStorageRegLineTransactionCollection(Factory.New<CusTempStorageRegLine>());
}
