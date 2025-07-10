using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Accounting.EPayment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccEPaymentStaffTokenDependentCollection))]
	sealed class AccEPaymentStaffTokenDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var bankAccountData = Factory.NewWithValidTestData<AccBankAccount>();
			return new AccEPaymentStaffTokenDependentCollection(bankAccountData);
		}

		public void TestAccountDetailsCollection()
		{
			var collection = (AccEPaymentStaffTokenDependentCollection)GetCollectionToTest();
			var staffToken1 = Factory.New<AccEPaymentStaffToken>();
			staffToken1.TK_AB = collection.Master.PK;
			var staffToken2 = Factory.New<AccEPaymentStaffToken>();
			staffToken2.TK_AB = collection.Master.PK;
			var staffToken3 = Factory.New<AccEPaymentStaffToken>();
			staffToken3.TK_AB = Factory.NewWithValidTestData<AccBankAccount>().PK;
			collection.Load();
			AssertEquals("Should be 2 staff tokens in the collection", 2, collection.Count);
			AssertCollectionContains(staffToken1, collection);
			AssertCollectionContains(staffToken2, collection);
		}
	}
}
