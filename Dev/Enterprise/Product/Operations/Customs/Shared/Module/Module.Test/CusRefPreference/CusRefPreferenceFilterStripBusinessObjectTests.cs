using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusRefPreferenceFilterStripBusinessObject))]
	sealed class CusRefPreferenceFilterStripBusinessObjectTests : ZArchitecture.Modules.Testing.FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CusRefPreferenceFilterStripBusinessObject();
		}

		public void TestCountrycodeFilter()
		{
			var filterBO = new CusRefPreferenceFilterStripBusinessObject();
			var filter = (ModuleNkFilter)filterBO["Country Code"];
			filter.IsActive = true;
			AssertEquals("Country code filter should be defaulted to the logged in country.", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, filter.Property);
			AssertEquals("Country code filter should be read only.", true, filter.ReadOnly);
			var preference = Factory.New<CusRefPreference>();
			preference.CR8_RN_NKCountryCode = "ZA";
			preference.CR8_Preference = "P1";
			preference.CR8_Description = "P1 Dec";
			var preference2 = Factory.New<CusRefPreference>();
			preference2.CR8_RN_NKCountryCode = "CN";
			preference2.CR8_Preference = "P2";
			preference2.CR8_Description = "P2 Dec";
			Factory.Save();
			filter.Property = "ZA";
			AssertEquals(true, preference.MatchesFilter(filterBO.Filter));
			AssertEquals(false, preference2.MatchesFilter(filterBO.Filter));
		}

		public void TestPreferenceFilter()
		{
			var preference = Factory.New<CusRefPreference>();
			preference.CR8_RN_NKCountryCode = "ZA";
			preference.CR8_Preference = "P1";
			preference.CR8_Description = "P1 Dec";
			var preference2 = Factory.New<CusRefPreference>();
			preference2.CR8_RN_NKCountryCode = "CN";
			preference2.CR8_Preference = "P2";
			preference2.CR8_Description = "P2 Dec";
			Factory.Save();
			var filterBO = new CusRefPreferenceFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO["Preference"];
			filter.IsActive = true;
			filter.Property = "P1";
			AssertEquals(true, preference.MatchesFilter(filterBO.Filter));
			AssertEquals(false, preference2.MatchesFilter(filterBO.Filter));
		}

		public void TestDescriptionFilter()
		{
			var preference = Factory.New<CusRefPreference>();
			preference.CR8_RN_NKCountryCode = "ZA";
			preference.CR8_Preference = "P1";
			preference.CR8_Description = "P1 Dec";
			var preference2 = Factory.New<CusRefPreference>();
			preference2.CR8_RN_NKCountryCode = "CN";
			preference2.CR8_Preference = "P2";
			preference2.CR8_Description = "P2 Dec";
			Factory.Save();
			var filterBO = new CusRefPreferenceFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO["Description"];
			filter.IsActive = true;
			filter.Property = "P1 Dec";
			AssertEquals(true, preference.MatchesFilter(filterBO.Filter));
			AssertEquals(false, preference2.MatchesFilter(filterBO.Filter));
		}
	}
}
