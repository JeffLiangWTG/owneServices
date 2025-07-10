using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(VATAccTaxRateCollectionForRegistryForTest))]
	sealed class VATAccTaxRateCollectionForRegistryTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new VATAccTaxRateCollectionForRegistryForTest(Factory);
		}

		public void TestRelationshipFilterIsEmpty()
		{
			VATAccTaxRateCollectionForRegistryForTest testCollection = (VATAccTaxRateCollectionForRegistryForTest)GetCollectionToTest();
			Assert(testCollection.RelationshipFilterForTest.IsEmpty);
		}

		public void TestVATAccTaxRateCollectionForRegistry()
		{
			var auCountry = Core.Constants.CountryCodes.Australia;
			var brazilCountry = Core.Constants.CountryCodes.Brazil;
			var accTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			accTaxRate.AT_RN_NKCountry = auCountry;
			accTaxRate.AT_Type = AccTaxRate.Types.NotReportable;
			accTaxRate.AT_TaxSystemCode = "Other";

			var accTaxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			accTaxRate2.AT_RN_NKCountry = auCountry;
			accTaxRate2.AT_Code = "BBCXYZ";
			accTaxRate2.AT_Type = AccTaxRate.Types.NotReportable;
			accTaxRate2.AT_TaxSystemCode = "Other1";

			var accTaxRate3 = Factory.NewWithValidTestData<AccTaxRate>();
			accTaxRate3.AT_RN_NKCountry = brazilCountry;
			accTaxRate3.AT_Code = "BBCXYZ1";
			accTaxRate3.AT_Type = AccTaxRate.Types.NotReportable;

			Factory.Save();

			VATAccTaxRateCollectionForRegistry testCollectionCurrentCompany = new VATAccTaxRateCollectionForRegistry(Factory, new ZQuery(AccTaxRateSchema.AT_IsActive, true), GlbCompany.CurrentCompany);
			testCollectionCurrentCompany.Load();
			AssertVATRegistryCollection(testCollectionCurrentCompany, 6);
			Assert(testCollectionCurrentCompany.Cast<AccTaxRate>().All(item => item.AT_RN_NKCountry == GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

			GlbCompany.CurrentCompany.SetCountry(brazilCountry);

			VATAccTaxRateCollectionForRegistry testCollectionBrazilCompany = new VATAccTaxRateCollectionForRegistry(Factory, new ZQuery(AccTaxRateSchema.AT_IsActive, true), GlbCompany.CurrentCompany);
			testCollectionBrazilCompany.Load();
			AssertVATRegistryCollection(testCollectionBrazilCompany, 7);
			AssertEquals("Should contain tax rate", true, testCollectionBrazilCompany.Contains(accTaxRate3));
		}

		void AssertVATRegistryCollection(VATAccTaxRateCollectionForRegistry testCollection, int collectionCount)
		{
			AssertEquals(collectionCount, testCollection.Count);
			Assert(testCollection.Cast<AccTaxRate>().All(item => item.AT_TaxSystemCode.IsEmpty));
			Assert(testCollection.Cast<AccTaxRate>().All(item => item.AT_RN_NKCountry == GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		}

		sealed class VATAccTaxRateCollectionForRegistryForTest : VATAccTaxRateCollectionForRegistry
		{
			public VATAccTaxRateCollectionForRegistryForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			internal ZQuery RelationshipFilterForTest
			{
				get { return RelationshipFilter; }
			}
		}
	}
}
