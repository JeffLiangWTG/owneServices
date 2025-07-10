using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class SharedCusPermitLineTransactionTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var permit = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			var permitTransaction = permit.CusPermitLineTransactions.AddNew();
			permitTransaction.CPL_Reference = "ABC";
			var guarantee = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			var guaranteeTransaction = guarantee.CusGuaranteeLineTransactions.AddNew();
			guaranteeTransaction.CPL_Reference = "ABC";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertType<BaseCusPermitLineTransaction>(newFactory.Load<SharedCusPermitLineTransaction>(permitTransaction.PK));
			AssertType<BaseCusGuaranteeLineTransaction>(newFactory.Load<SharedCusPermitLineTransaction>(guaranteeTransaction.PK));
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(typeof(BaseCusPermitLineTransaction), new SharedCusPermitLineTransactionTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertNull(new SharedCusPermitLineTransactionTypeDecider().GetTypeForBinding());
		}
	}
}
