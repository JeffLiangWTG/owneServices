using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(VATAccTaxRateCollection))]
	sealed class VATAccTaxRateCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new VATAccTaxRateCollection(Factory);
		}

		public void TestRelationshipFilter()
		{
			var dbCountOfAccTaxRate = Factory.GetDatabaseCount(typeof(AccTaxRate), new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

			var auCountry = Core.Constants.CountryCodes.Australia;
			var brazilCountry = Core.Constants.CountryCodes.Brazil;
			var rate = Factory.NewWithValidTestData<AccTaxRate>();
			rate.AT_RN_NKCountry = auCountry;
			rate.AT_Type = AccTaxRate.Types.NotReportable;
			rate.AT_TaxSystemCode = "Other";

			var rate2 = Factory.NewWithValidTestData<AccTaxRate>();
			rate2.AT_RN_NKCountry = auCountry;
			rate2.AT_Code = "BBCXYZ";
			rate2.AT_Type = AccTaxRate.Types.NotReportable;
			rate2.AT_TaxSystemCode = "Other1";

			var rate3 = Factory.NewWithValidTestData<AccTaxRate>();
			rate3.AT_RN_NKCountry = brazilCountry;
			rate3.AT_Code = "BBCXYZ1";
			rate3.AT_Type = AccTaxRate.Types.NotReportable;

			Factory.Save();

			var testCollection = new VATAccTaxRateCollection(Factory);
			testCollection.Load();
			AssertVATCollection(testCollection, dbCountOfAccTaxRate);

			var testCollectionBrazil = new VATAccTaxRateCollection(Factory, brazilCountry);
			testCollectionBrazil.Load();
			AssertVATCollection(testCollectionBrazil, 1);
			Assert(testCollectionBrazil.Cast<AccTaxRate>().All(item => item.AT_RN_NKCountry == brazilCountry));

			var testCollectionActive = new VATAccTaxRateCollection(Factory, new ZQuery(AccTaxRateSchema.AT_IsActive, true));
			testCollectionActive.Load();
			AssertVATCollection(testCollectionActive, 6);
			Assert(testCollectionActive.Cast<AccTaxRate>().All(item => item.AT_IsActive));

			var testCollectionCurrentCompany = new VATAccTaxRateCollection(Factory, new ZQuery(AccTaxRateSchema.AT_IsActive, true), GlbCompany.CurrentCompany);
			testCollectionCurrentCompany.Load();
			AssertVATCollection(testCollectionCurrentCompany, 6);
			Assert(testCollectionCurrentCompany.Cast<AccTaxRate>().All(item => item.AT_IsActive));
			Assert(testCollectionCurrentCompany.Cast<AccTaxRate>().All(item => item.AT_RN_NKCountry == GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		}

		void AssertVATCollection(VATAccTaxRateCollection testCollection, int expectedCount)
		{
			AssertEquals(expectedCount, testCollection.Count);
			Assert(testCollection.Cast<AccTaxRate>().All(item => item.AT_TaxSystemCode.IsEmpty));
		}
	}
}
