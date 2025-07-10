using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using static Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(TWSpecialCodeListFilterStripBusinessObject))]
	public sealed class TWSpecialCodeListFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new TWSpecialCodeListFilterStripBusinessObject();

		public void TestCountryOrGroupingFilter()
		{
			var sp99 = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "SP99", "TW", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SpecialCodesForExemptionOfControllingAgencies, new ZDateTime(2016, 6, 20));
			var ns99 = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "NS99", "TW", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SpecialCodesForExemptionOfControllingAgencies, new ZDateTime(2021, 6, 20));
			var fr99 = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "FR99", "FR", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SpecialCodesForExemptionOfControllingAgencies, ZDateTime.Now);
			var filterObj = GetNewFilterStripBusinessObject();
			var countryFilter = (ModuleNkFilter)filterObj[Enterprise.Customs.Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping];
			countryFilter.IsActive = true;
			countryFilter.Property = "TW";
			Assert(sp99.MatchesFilter(filterObj.Filter));
			Assert(ns99.MatchesFilter(filterObj.Filter));
			Assert(!fr99.MatchesFilter(filterObj.Filter));
			countryFilter.Property = "FR";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			Assert(fr99.MatchesFilter(filterObj.Filter));
		}

		public void TestListTypeFilter()
		{
			var sp99 = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "SP99", "TW", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SpecialCodesForExemptionOfControllingAgencies, new ZDateTime(2016, 6, 20));
			var ns99 = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "NS99", "TW", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SpecialCodesForExemptionOfControllingAgencies, new ZDateTime(2021, 6, 20));
			var fr99 = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "FR99", "FR", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SpecialCodesForExemptionOfControllingAgencies, ZDateTime.Now);
			var filterObj = GetNewFilterStripBusinessObject();
			var listTypeFilter = (ModuleTextFilter)filterObj[Enterprise.Customs.Universal.Constants.ZZRefCusCodeListFilters.ListType];
			listTypeFilter.IsActive = true;
			listTypeFilter.Property = "TWCA";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			Assert(!fr99.MatchesFilter(filterObj.Filter));
			listTypeFilter.Property = Codes.SpecialCodesForExemptionOfControllingAgencies;
			Assert(sp99.MatchesFilter(filterObj.Filter));
			Assert(ns99.MatchesFilter(filterObj.Filter));
			Assert(fr99.MatchesFilter(filterObj.Filter));
		}

		public void TestEffectiveDateFilter()
		{
			var sp99 = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "SP99", "TW", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SpecialCodesForExemptionOfControllingAgencies, new ZDateTime(2016, 6, 20));
			var ns99 = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "NS99", "TW", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SpecialCodesForExemptionOfControllingAgencies, new ZDateTime(2021, 6, 20));
			var filterObj = GetNewFilterStripBusinessObject();
			var effectiveDateFilter = (ModuleSingleDateFilter)filterObj[Enterprise.Customs.Universal.Constants.ZZRefCusCodeListFilters.EffectiveDate];
			effectiveDateFilter.IsActive = true;
			effectiveDateFilter.Property1 = new ZDateTime(2016, 6, 30);
			Assert(sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			effectiveDateFilter.Property1 = new ZDateTime(2021, 6, 30);
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(ns99.MatchesFilter(filterObj.Filter));
		}

		public void TestControllingAgencyFilter()
		{
			var sp99 = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "SP99", "TW", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SpecialCodesForExemptionOfControllingAgencies, new ZDateTime(2016, 6, 20));
			var ns99 = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "NS99", "TW", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SpecialCodesForExemptionOfControllingAgencies, new ZDateTime(2021, 6, 20));
			var filterObj = GetNewFilterStripBusinessObject();
			var controllingAgencyFilter = (ModuleTextFilter)filterObj[RefCusCodeListAttributes.ControllingAgency];
			controllingAgencyFilter.IsActive = true;
			controllingAgencyFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			controllingAgencyFilter.Property = "SP";
			Assert(sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			controllingAgencyFilter.Property = "NS";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(ns99.MatchesFilter(filterObj.Filter));
			controllingAgencyFilter.Property = "SPRemarks";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			controllingAgencyFilter.Property = "NSRemarks";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			controllingAgencyFilter.Property = "SPSource";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			controllingAgencyFilter.Property = "NSSource";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
		}

		public void TestRemarksFilter()
		{
			var sp99 = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "SP99", "TW", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SpecialCodesForExemptionOfControllingAgencies, new ZDateTime(2016, 6, 20));
			var ns99 = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "NS99", "TW", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SpecialCodesForExemptionOfControllingAgencies, new ZDateTime(2021, 6, 20));
			var filterObj = GetNewFilterStripBusinessObject();
			var remarksFilter = (ModuleTextFilter)filterObj[RefCusCodeListAttributes.Remarks];
			remarksFilter.IsActive = true;
			remarksFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			remarksFilter.Property = "SP";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			remarksFilter.Property = "NS";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			remarksFilter.Property = "SPRemarks";
			Assert(sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			remarksFilter.Property = "NSRemarks";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(ns99.MatchesFilter(filterObj.Filter));
			remarksFilter.Property = "SPSource";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			remarksFilter.Property = "NSSource";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
		}

		public void TestSourceFilter()
		{
			var sp99 = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "SP99", "TW", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SpecialCodesForExemptionOfControllingAgencies, new ZDateTime(2016, 6, 20));
			var ns99 = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "NS99", "TW", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SpecialCodesForExemptionOfControllingAgencies, new ZDateTime(2021, 6, 20));
			var filterObj = GetNewFilterStripBusinessObject();
			var sourceFilter = (ModuleTextFilter)filterObj[RefCusCodeListAttributes.Source];
			sourceFilter.IsActive = true;
			sourceFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			sourceFilter.Property = "SP";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			sourceFilter.Property = "NS";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			sourceFilter.Property = "SPRemarks";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			sourceFilter.Property = "NSRemarks";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			sourceFilter.Property = "SPSource";
			Assert(sp99.MatchesFilter(filterObj.Filter));
			Assert(!ns99.MatchesFilter(filterObj.Filter));
			sourceFilter.Property = "NSSource";
			Assert(!sp99.MatchesFilter(filterObj.Filter));
			Assert(ns99.MatchesFilter(filterObj.Filter));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var twCountryCode = Core.Constants.CountryCodes.Taiwan;
			var specialCodesForExemptionOfControllingAgencies = Codes.SpecialCodesForExemptionOfControllingAgencies;
			var twca = "TWCA";
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(twca, "Control Agency", twCountryCode);
			var controllingAgency = helper.CreateNewOrGetExistingCusCodeType(specialCodesForExemptionOfControllingAgencies, "Permit Exemption Codes", twCountryCode);
			controllingAgency.ZZK_IsReadonly = false;
			var controllingAgencyAttributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(UniversalReferenceConstants.RefCusCodeListAttributes.ControllingAgency, "ControllingAgency", specialCodesForExemptionOfControllingAgencies, twCountryCode);
			controllingAgencyAttributeName.ZXE_ZZK_NKCodeTypeForValueList = twca;
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(UniversalReferenceConstants.RefCusCodeListAttributes.Remarks, "Remarks", specialCodesForExemptionOfControllingAgencies, twCountryCode);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(UniversalReferenceConstants.RefCusCodeListAttributes.Source, "Source", specialCodesForExemptionOfControllingAgencies, twCountryCode);
			helper.CreateCusCodeList(twCountryCode, twca, "SP", "科技部中部科學工業園區管理局", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(twCountryCode, twca, "NS", "科技部南部科學工業園區管理局", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var sp99CodeList = helper.CreateCusCodeList(twCountryCode, specialCodesForExemptionOfControllingAgencies, "SP99", "SP99Desc", new ZDateTime(2016, 5, 19), new ZDateTime(2017, 5, 19));
			sp99CodeList.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.ControllingAgency, "SP");
			sp99CodeList.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Remarks, "SPRemarks");
			sp99CodeList.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Source, "SPSource");
			var ns99codeList = helper.CreateCusCodeList(twCountryCode, specialCodesForExemptionOfControllingAgencies, "NS99", "NS99Desc", new ZDateTime(2021, 5, 19), new ZDateTime(2022, 5, 19));
			ns99codeList.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.ControllingAgency, "NS");
			ns99codeList.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Remarks, "NSRemarks");
			ns99codeList.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Source, "NSSource");
			helper.CreateCusCodeList("FR", specialCodesForExemptionOfControllingAgencies, "FR99", "FR99Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();
		}
	}
}
