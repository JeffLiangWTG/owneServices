using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccTaxRateFilterBusinessObject))]
	sealed class AccTaxRateFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCodeFilter()
		{
			AccTaxRate code1 = Factory.NewWithValidTestData<AccTaxRate>();
			AccTaxRate code2 = Factory.NewWithValidTestData<AccTaxRate>();
			code1.AT_Code = "Indescr";
			code1.AT_RN_NKCountry = Env.CurrentCompany.Country.Code;
			code2.AT_Code = "SomeDescr";
			code2.AT_RN_NKCountry = Env.CurrentCompany.Country.Code;

			Factory.Save();

			AccTaxRateFilterBusinessObject filter = new AccTaxRateFilterBusinessObject();
			((ModuleTextFilter)filter["Code"]).Property = "Indescr";
			((ModuleTextFilter)filter["Code"]).IsActive = true;

			AccTaxRateCollection bankAccounts = new AccTaxRateCollection(Factory, filter.Filter);
			bankAccounts.Load();

			AssertCollectionContains(code1, bankAccounts);
			AssertCollectionNotContains(code2, bankAccounts);
		}

		public void TestDescriptionFilter()
		{
			AccTaxRate description1 = Factory.NewWithValidTestData<AccTaxRate>();
			AccTaxRate description2 = Factory.NewWithValidTestData<AccTaxRate>();
			description1.AT_Description = "Indescr";
			description1.AT_RN_NKCountry = Env.CurrentCompany.Country.Code;
			description2.AT_Description = "SomeDescr";
			description2.AT_RN_NKCountry = Env.CurrentCompany.Country.Code;

			Factory.Save();

			AccTaxRateFilterBusinessObject filter = new AccTaxRateFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "Indescr";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			AccTaxRateCollection bankAccounts = new AccTaxRateCollection(Factory, filter.Filter);
			bankAccounts.Load();

			AssertCollectionContains(description1, bankAccounts);
			AssertCollectionNotContains(description2, bankAccounts);
		}

		public void TestPostingGroup()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				AssertEquals(false, AccTaxRate.IsPostingGroupsEnabled(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
				var filter = new AccTaxRateFilterBusinessObject();
				AssertNull(filter["Posting Group"]);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("VN"))
			{
				var tax1 = Factory.NewWithValidTestData<AccTaxRate>();
				var tax2 = Factory.NewWithValidTestData<AccTaxRate>();
				tax1.AT_PostingGroupId = 1;
				tax2.AT_PostingGroupId = 2;

				tax1.AT_RN_NKCountry = "VN";
				tax2.AT_RN_NKCountry = "VN";

				Factory.Save();

				AssertEquals(true, AccTaxRate.IsPostingGroupsEnabled(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
				var filter = new AccTaxRateFilterBusinessObject();
				AssertNotNull(filter["Posting Group"]);
				((ModuleNumberFilter)filter["Posting Group"]).Property = "1";
				((ModuleNumberFilter)filter["Posting Group"]).IsActive = true;

				var collection = new AccTaxRateCollection(Factory, filter.Filter);
				collection.Load();

				AssertCollectionContains(tax1, collection);
				AssertCollectionNotContains(tax2, collection);
			}
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccTaxRateFilterBusinessObject();
		}

		#endregion
	}
}
