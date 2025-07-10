using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTaxOverrideGroupCollection))]
	sealed class AccTaxOverrideGroupCollection02Test : BusinessObjectCollectionTestCase
	{
		public void TestCollectionLoadsOnlyCompanyCountryGroups()
		{
			AccTaxOverrideGroup taxOverrideGroupAU1 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroupAU1.AX_RN_NKCountry = "AU";
			AccTaxOverrideGroup taxOverrideGroupAU2 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroupAU2.AX_RN_NKCountry = "AU";
			AccTaxOverrideGroup taxOverrideGroupUS = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroupUS.AX_RN_NKCountry = "US";

			GlbCompany aUCompany = Factory.NewWithValidTestData<GlbCompany>();
			aUCompany.SetCountry("AU");
			GlbCompany uSCompany = Factory.NewWithValidTestData<GlbCompany>();
			uSCompany.SetCountry("US");

			AccTaxOverrideGroupCollection testCollection = GetCollectionToTest(aUCompany);
			testCollection.Load();
			AssertEquals("AccTaxOverrideGroupCollection count.", 2, testCollection.Count);
			AssertCollectionContains("Only AU groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroupAU1.PK), testCollection);
			AssertCollectionContains("Only AU groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroupAU2.PK), testCollection);

			testCollection = GetCollectionToTest(uSCompany);
			testCollection.Load();
			AssertEquals("AccTaxOverrideGroupCollection count.", 1, testCollection.Count);
			AssertCollectionContains("Only US groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroupUS.PK), testCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccTaxOverrideGroupCollection(Factory);
		}

		AccTaxOverrideGroupCollection GetCollectionToTest(GlbCompany company)
		{
			return new AccTaxOverrideGroupCollection(Factory, company);
		}
	}
}
