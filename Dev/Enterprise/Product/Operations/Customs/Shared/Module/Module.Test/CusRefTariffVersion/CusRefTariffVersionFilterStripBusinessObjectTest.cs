using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusRefTariffVersionFilterStripBusinessObject))]
	sealed class CusRefTariffVersionFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new CusRefTariffVersionFilterStripBusinessObject();
			AssertNotNull(filter["Code"]);
			AssertNotNull(filter["Description"]);
			AssertNotNull(filter["EffectiveDate"]);
			AssertNotNull(filter["Country/Region"]);
		}

		public void TestCountryFilter()
		{
			var tariffVersion3 = Factory.NewWithValidTestData<CusRefTariffVersion>();
			tariffVersion3.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			var filterObject = GetNewFilterStripBusinessObject();
			var nkFilter = (ModuleNkFilter)filterObject["Country/Region"];
			nkFilter.IsActive = true;
			CombineAssertions(() =>
			{
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, nkFilter.Visibility);
				AssertEquals("DefaultProperty", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, nkFilter.DefaultProperty);
				AssertEquals("Match", true, tariffVersion1.MatchesFilter(filterObject.Filter));
				AssertEquals("Not match", false, tariffVersion3.MatchesFilter(filterObject.Filter));
			});
		}

		public void TestCodeFilter()
		{
			var filterObject = GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterObject["Code"];
			textFilter.IsActive = true;
			textFilter.Property = "CD1";
			CombineAssertions(() =>
			{
				AssertEquals("Match", true, tariffVersion1.MatchesFilter(filterObject.Filter));
				AssertEquals("Not match", false, tariffVersion2.MatchesFilter(filterObject.Filter));
			});
		}

		public void TestDescriptionFilter()
		{
			var filterObject = GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterObject["Description"];
			textFilter.IsActive = true;
			textFilter.Property = "desc1";
			CombineAssertions(() =>
			{
				AssertEquals("Match", true, tariffVersion1.MatchesFilter(filterObject.Filter));
				AssertEquals("Not match", false, tariffVersion2.MatchesFilter(filterObject.Filter));
			});
		}

		public void TestEffectiveDateFilter()
		{
			var filterObject = GetNewFilterStripBusinessObject();
			var dateFilter = (ModuleDateFilter)filterObject["EffectiveDate"];
			dateFilter.IsActive = true;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDate.BrettsBirthday;
			dateFilter.Property2 = ZDate.BrettsBirthday.AddDays(1);
			CombineAssertions(() =>
			{
				AssertEquals("Match", true, tariffVersion1.MatchesFilter(filterObject.Filter));
				AssertEquals("Not match", false, tariffVersion2.MatchesFilter(filterObject.Filter));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CusRefTariffVersionFilterStripBusinessObject();

		CusRefTariffVersion tariffVersion1;
		CusRefTariffVersion tariffVersion2;
		protected override void SetUp()
		{
			base.SetUp();
			tariffVersion1 = Factory.NewWithValidTestData<CusRefTariffVersion>();
			tariffVersion2 = Factory.NewWithValidTestData<CusRefTariffVersion>();
			tariffVersion1.CRT_Version = "CD1";
			tariffVersion2.CRT_Version = "CD2";
			tariffVersion1.CRT_Description = "desc1";
			tariffVersion2.CRT_Description = "desc2";
			tariffVersion1.CRT_EffectiveDate = ZDate.BrettsBirthday;
			tariffVersion2.CRT_EffectiveDate = ZDate.BrettsBirthday.AddDays(2);
			Factory.Save();
		}
	}
}
