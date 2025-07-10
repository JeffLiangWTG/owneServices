using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(NonVATAccTaxRateCollection))]
	sealed class NonVATAccTaxRateCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NonVATAccTaxRateCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public void TestRelationshipFilterFiltersOnCurrentCompany()
		{
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
			rate3.AT_TaxSystemCode = "Other2";

			Factory.Save();

			NonVATAccTaxRateCollection testCollection = new NonVATAccTaxRateCollection(Factory, auCountry);
			testCollection.Load();
			AssertEquals(2, testCollection.Count);
			Assert(testCollection.Cast<AccTaxRate>().All(item => !item.AT_TaxSystemCode.IsEmpty));

			NonVATAccTaxRateCollection testCollectionBrazil = new NonVATAccTaxRateCollection(Factory, brazilCountry);
			testCollectionBrazil.Load();
			AssertEquals(1, testCollectionBrazil.Count);
			Assert(testCollectionBrazil.Cast<AccTaxRate>().All(item => !item.AT_TaxSystemCode.IsEmpty));
			Assert(testCollectionBrazil.Cast<AccTaxRate>().All(item => item.AT_RN_NKCountry == brazilCountry));
		}

		public void TestAdditionalFilter()
		{
			var auCountry = Core.Constants.CountryCodes.Australia;

			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfiguration.ETC_TaxSystemCode = "TEST";

			//TaxID TaxSystem same as TaxConfiguration TaxSystem
			var taxRate1 = AccountingTestObjectCreator.CreateTaxRate("AAAA", "TEST");

			//TaxID TaxSystem not same as TaxConfiguration TaxSystem
			var taxRate2 = AccountingTestObjectCreator.CreateTaxRate("BBBB", "OTHER");

			Factory.Save();

			var nonVATAccTaxRateCollection = new NonVATAccTaxRateCollection(Factory, auCountry, null);
			Assert("Precondition:", nonVATAccTaxRateCollection.Cast<AccTaxRate>().All(item => !item.AT_TaxSystemCode.IsEmpty));
			var expectedCollectionItems = new[] { taxRate1, taxRate2 };
			nonVATAccTaxRateCollection.Load();
			AssertEquals(2, nonVATAccTaxRateCollection.Count);
			AssertContainsExactElementsInAnyOrder("When Tax Configuration NULL, lists all Tax IDs with Tax system not EMPTY", expectedCollectionItems, nonVATAccTaxRateCollection);

			var nonVATAccTaxRateCollection1 = new NonVATAccTaxRateCollection(Factory, auCountry, taxConfiguration);
			expectedCollectionItems = new[] { taxRate1 };
			nonVATAccTaxRateCollection1.Load();
			AssertEquals(1, nonVATAccTaxRateCollection1.Count);
			AssertContainsExactElementsInAnyOrder("When Tax Configuration NOT NULL, lists all Tax IDs with Tax system same for Tax Configuration", expectedCollectionItems, nonVATAccTaxRateCollection1);
		}

		public void TestAddNotificationWhenAdditionalFilterNotMet()
		{
			var auCountry = Core.Constants.CountryCodes.Australia;

			//TaxID TaxSystem same as TaxConfiguration TaxSystem
			var taxRate1 = AccountingTestObjectCreator.CreateTaxRate("AAAA", "TEST");

			//TaxID TaxSystem not same as TaxConfiguration TaxSystem
			var taxRate2 = AccountingTestObjectCreator.CreateTaxRate("BBBB", "OTHER");

			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfiguration.ETC_TaxSystemCode = "TEST";

			Factory.Save();

			var expectedNotificationMessage = "Tax ID chosen here is not relevant to the Tax System 'TEST' of the Tax Configuration '" + taxConfiguration.ETC_Code + "'. Please select a different Tax ID.";

			var nonVATAccTaxRateCollection = new NonVATAccTaxRateCollection(Factory, auCountry, null);
			AssertNotEquals(expectedNotificationMessage, nonVATAccTaxRateCollection.GetAllNotificationsWhenAdditionalFilterNotMet(taxRate1));
			AssertNotEquals(expectedNotificationMessage, nonVATAccTaxRateCollection.GetAllNotificationsWhenAdditionalFilterNotMet(taxRate2));

			nonVATAccTaxRateCollection = new NonVATAccTaxRateCollection(Factory, auCountry, taxConfiguration);
			AssertNotEquals(expectedNotificationMessage, nonVATAccTaxRateCollection.GetAllNotificationsWhenAdditionalFilterNotMet(taxRate1));
			AssertEquals(expectedNotificationMessage, nonVATAccTaxRateCollection.GetAllNotificationsWhenAdditionalFilterNotMet(taxRate2));
		}

		AccountingTestObjectCreator AccountingTestObjectCreator => accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator accountingTestObjectCreator;
	}
}
