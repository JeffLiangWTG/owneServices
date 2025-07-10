using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ARAccountDetailsDependentCollection))]
	sealed class ARAccountDetailsDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgCompanyData companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			return new ARAccountDetailsDependentCollection(companyData);
		}

		public void TestAccountDetailsCollection()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var collection = (ARAccountDetailsDependentCollection)GetCollectionToTest();
			var accDetails = Factory.New<AccAPAccountDetails>();
			accDetails.A1_OB = collection.Master.PK;
			var accDetails1 = Factory.New<AccARAccountDetails>();
			accDetails1.A1_OB = collection.Master.PK;
			accDetails1.A1_PaymentMethod = "TAX";
			var accDetails2 = Factory.New<AccARAccountDetails>();
			accDetails2.A1_OB = collection.Master.PK;
			accDetails2.A1_PaymentMethod = "CRQ";
			var accDetails3 = Factory.New<AccARAccountDetails>();
			accDetails3.A1_OB = collection.Master.PK;
			accDetails3.A1_PaymentMethod = "NET";
			collection.Load();
			AssertEquals("Should be 3 Accounts in the collection", 3, collection.Count);
			AssertCollectionContains(accDetails1, collection);
			AssertCollectionContains(accDetails2, collection);
			AssertCollectionContains(accDetails3, collection);
		}
	}
}
