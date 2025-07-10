using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccOrgTaxRateCollection))]
	class AccOrgTaxRateCollectionTest : ActiveBusinessObjectCollectionTestCase<AccOrgTaxRateCollection>
	{
		public void TestCollectionLoadsCorrectRecords()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();

			AccountingTestObjectCreator.Factory.RefreshEnabled = false;

			var taxConfiguration1 = AccountingTestObjectCreator.CreateTaxConfiguration("AR");
			var taxConfiguration2 = AccountingTestObjectCreator.CreateTaxConfiguration("AR");

			Factory.Save();

			var accOrgTaxConfiguration1 = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration1, companyData, true);

			var orgTaxRate1 = accOrgTaxConfiguration1.TaxRates.AddNew();
			orgTaxRate1.OTR_Source = AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.Quarterly.Code;
			var orgTaxRate2 = accOrgTaxConfiguration1.TaxRates.AddNew();
			orgTaxRate2.OTR_Source = AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.Monthly.Code;

			var accOrgTaxConfiguration2 = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration2, companyData, true);

			var orgTaxRate3 = accOrgTaxConfiguration2.TaxRates.AddNew();
			orgTaxRate3.OTR_Source = AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.ManualOverride.Code;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var option1 = newFactory.Load<AccOrgTaxConfiguration>(accOrgTaxConfiguration1.PK);
			AssertEquals("Collection count", 2, option1.TaxRates.Count);

			var option2 = newFactory.Load<AccOrgTaxConfiguration>(accOrgTaxConfiguration2.PK);
			AssertEquals("Collection count", 1, option2.TaxRates.Count);
		}

		protected override AccOrgTaxRateCollection GetCollectionToTest()
		{
			var accOrgTaxConfiguration = Factory.New<AccOrgTaxConfiguration>();
			return new AccOrgTaxRateCollection(accOrgTaxConfiguration);
		}

		AccountingTestObjectCreator accountingTestObjectCreator;
		protected AccountingTestObjectCreator AccountingTestObjectCreator
		{
			get
			{
				return accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
			}
		}
	}
}
