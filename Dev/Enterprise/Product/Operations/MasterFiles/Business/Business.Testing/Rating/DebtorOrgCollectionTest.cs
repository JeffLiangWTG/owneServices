using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DebtorOrgCollectionTest : TestCaseWithFactory
	{
		public void TestDebtorOrgCollectionIndexer()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var collection = new DebtorOrgCollection();

			collection[RatingDebtorOrgTypes.CNE] = org;
			AssertEquals(org, collection[RatingDebtorOrgTypes.CNE]);

			collection[RatingDebtorOrgTypes.CNR] = org;
			AssertEquals(2, collection.Count);

			collection[RatingDebtorOrgTypes.CNR] = org2;
			AssertEquals(2, collection.Count);
			AssertEquals(org, collection[RatingDebtorOrgTypes.CNE]);
			AssertEquals(org2, collection[RatingDebtorOrgTypes.CNR]);

			AssertExceptionThrown<InvalidOperationException>(() => collection.Add(new DebtorOrg(org, RatingDebtorOrgTypes.CNR)));
		}
	}
}
