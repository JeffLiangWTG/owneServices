using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusRefRateCodeFilterSrtipBusinessObject))]
	sealed class CusRefRateCodeFilterSrtipBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var testCusRefRateCode = Factory.NewWithValidTestData<CusRefRateCode>();
			var filterBO = new CusRefRateCodeFilterSrtipBusinessObject();
			var countryFilter = (ModuleNkFilter)filterBO["Country Code"];
			countryFilter.IsActive = true;
			countryFilter.Property = "US";
			var rateCodeFilter = (ModuleTextFilter)filterBO["Rate Code"];
			rateCodeFilter.IsActive = true;
			rateCodeFilter.Property = "CD1";
			var descFilter = (ModuleTextFilter)filterBO["Description"];
			descFilter.IsActive = true;
			descFilter.Property = "Desc";
			var typeFilter = (ModuleTextFilter)filterBO["Rate Type"];
			typeFilter.IsActive = true;
			typeFilter.Property = "T1";
			testCusRefRateCode.CR7_RN_NKCountryCode = "US";
			AssertEquals(false, testCusRefRateCode.MatchesFilter(filterBO.Filter));
			testCusRefRateCode.CR7_RateCode = "CD1";
			AssertEquals(false, testCusRefRateCode.MatchesFilter(filterBO.Filter));
			testCusRefRateCode.CR7_Description = "Description";
			AssertEquals(false, testCusRefRateCode.MatchesFilter(filterBO.Filter));
			testCusRefRateCode.CR7_RateType = "T1";
			AssertEquals(true, testCusRefRateCode.MatchesFilter(filterBO.Filter));
		}

		public void TestCountryFilter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var filterBO = new CusRefRateCodeFilterSrtipBusinessObject();
				var countryFilter = (ModuleNkFilter)filterBO["Country Code"];
				AssertEquals("Default value is current country", Core.Constants.CountryCodes.Australia, countryFilter.Property);
				AssertEquals("Country Code is default filter", FilterVisibility.AlwaysVisible, countryFilter.Visibility);
				AssertEquals("Country Code filter is read only", true, countryFilter.ReadOnly);
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CusRefRateCodeFilterSrtipBusinessObject();
	}
}
