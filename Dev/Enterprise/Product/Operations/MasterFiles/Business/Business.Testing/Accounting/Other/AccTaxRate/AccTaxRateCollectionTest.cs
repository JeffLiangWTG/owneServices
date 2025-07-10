using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTaxRateCollection))]
	class AccTaxRateCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccTaxRateCollection(Factory);
		}

		public void TestRelationshipFilterFiltersOnCurrentCompany()
		{
			int totalTaxRatesCount = Factory.GetDatabaseCount(typeof(AccTaxRate));
			AccTaxRateCollection testCollection = new AccTaxRateCollection(Factory);
			testCollection.Load();

			foreach (AccTaxRate accTaxRate in testCollection)
			{
				AssertEquals("Should belong to the current login company", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, accTaxRate.AT_RN_NKCountry);
			}

			Assert(testCollection.Count <= totalTaxRatesCount);
		}

		public void TestRelationshipFilterFiltersOnCountryFromPassedCompany()
		{
			GlbCompany otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			AssertNotNull("Pre-condition: SHould be company with other country", otherCompany);
			AccTaxRate rate = Factory.NewWithValidTestData<AccTaxRate>();
			rate.AT_RN_NKCountry = otherCompany.GC_RN_NKCountryCode;

			AccTaxRateCollection testCollection = new AccTaxRateCollection(Factory, new ZQuery(), otherCompany);
			testCollection.Load();

			Assert("Should be something loaded", testCollection.Count > 0);

			foreach (AccTaxRate accTaxRate in testCollection)
			{
				AssertEquals("Should belong to the other company", otherCompany.GC_RN_NKCountryCode, accTaxRate.AT_RN_NKCountry);
			}

			AccTaxRateCollection nullCompanyCollection = new AccTaxRateCollection(Factory, new ZQuery(), (GlbCompany)null);
			nullCompanyCollection.Load();

			Assert("Should load more without Company filter", nullCompanyCollection.Count > testCollection.Count);

			AccTaxRateCollection emptyCountryCollection = new AccTaxRateCollection(Factory, ZString.Empty);
			emptyCountryCollection.Load();

			Assert("Should load more without Country filter", emptyCountryCollection.Count > testCollection.Count);
			AssertEquals("Should load the same number of records as without Company filter", nullCompanyCollection.Count, emptyCountryCollection.Count);
		}
	}
}
