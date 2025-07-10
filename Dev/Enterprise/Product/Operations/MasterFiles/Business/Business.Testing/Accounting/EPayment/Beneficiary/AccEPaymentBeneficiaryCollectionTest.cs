using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccEPaymentBeneficiaryCollection))]
	sealed class AccEPaymentBeneficiaryCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccEPaymentBeneficiaryCollection(Factory);
		}

		public void TestEPaymentBeneficiaryCollection()
		{
			var beneficiary1 = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			var beneficiary2 = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			var beneficiary3 = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			Factory.Save();
			var collection = (AccEPaymentBeneficiaryCollection)GetCollectionToTest();
			collection.Load();
			AssertEquals("Should be 3 beneficiaries in the collection", 3, collection.Count);
			AssertCollectionContains(beneficiary1, collection);
			AssertCollectionContains(beneficiary2, collection);
			AssertCollectionContains(beneficiary3, collection);
		}

		public void TestAllowNew()
		{
			var collection = (AccEPaymentBeneficiaryCollection)GetCollectionToTest();
			Assert(!collection.AllowNew);
		}
	}
}
