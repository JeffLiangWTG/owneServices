using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class AccOrgTaxConfigurationCollectionByLedgerTest : AccOrgTaxConfigurationCollectionTest
	{
		public void TestSetDefaultForNewElementForCollection()
		{
			var orgCompanyData = Factory.New<OrgCompanyData>();
			var collection = GetCollectionToTest(orgCompanyData);
			var element = collection.AddNew();

			AssertEquals("OTC_OB", orgCompanyData.PK, element.OTC_OB);
			AssertEquals("Ledger", ExpectedLedger, element.Ledger);
		}

		public void TestRecordsOnlyForPassedOrgCompanyData()
		{
			var otherLedger = ExpectedLedger == LedgerTypes.AccountsPayable ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
			AssertNotEquals("Precondition", ExpectedLedger, otherLedger);

			var companyData = Factory.New<OrgCompanyData>();
			var companyData2 = Factory.New<OrgCompanyData>();

			var expectedElement = GetNewElementToAddToTheCollection(ExpectedLedger);
			expectedElement.OTC_OB = companyData.PK;
			var otherLedgerElement = GetNewElementToAddToTheCollection(otherLedger);
			otherLedgerElement.OTC_OB = companyData.PK;
			var otherCompanyElement = GetNewElementToAddToTheCollection(ExpectedLedger);
			otherCompanyElement.OTC_OB = companyData2.PK;

			var collection = GetCollectionToTest(companyData);

			AssertEquals("collection.Count", 1, collection.Count);
			AssertCollectionContains(nameof(collection), expectedElement, collection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return GetNewElementToAddToTheCollection(ExpectedLedger);
		}

		AccOrgTaxConfiguration GetNewElementToAddToTheCollection(string ledger)
		{
			var taxConfiguration = AnotherBusinessObjectFactory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_Ledger = ledger;
			taxConfiguration.Factory.Save();

			var element = Factory.New<AccOrgTaxConfiguration>();
			element.Ledger = ledger;
			element.OTC_ETC = taxConfiguration.PK;
			return element;
		}

		BusinessObjectFactory AnotherBusinessObjectFactory => anotherBusinessObjectFactory ?? (anotherBusinessObjectFactory = new BusinessObjectFactory());
		BusinessObjectFactory anotherBusinessObjectFactory;

		protected abstract ZString ExpectedLedger { get; }

		protected abstract AccOrgTaxConfigurationCollection GetCollectionToTest(OrgCompanyData parent);
	}
}
