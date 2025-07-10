using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineTransactionLookups))]
sealed class CusTempStorageRegLineTransactionLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestPreviousReferenceTypeList()
	{
		Assert(ReferenceEquals(Factory.GetCachedValue<CusTempStorageRegLineTransactionTypeList>(), lineTransaction.Lookups.TransactionTypeList));
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<CusTempStorageRegHeader>();
		var line = header.CusTempStorageRegLines.AddNew();
		lineTransaction = line.CusTempStorageRegLineTransactions.AddNew();
	}
	CusTempStorageRegLineTransaction lineTransaction;
}
