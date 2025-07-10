using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ZZRefCusMapFilters = Enterprise.Customs.Universal.Constants.ZZRefCusMapFilters;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCusMapFilterStripBusinessObject))]
	class ZZRefCusMapFilterStripBusinessObjectTests : ZArchitecture.Modules.Testing.FilterStripBusinessObjectTestCase
	{
		public void TestSystemFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusMapFilterStripBusinessObject();
			filterStrip.QueryObjectType = typeof(ZZRefCusMapCombined);
			var systemFilter = (ModuleTextFilter)filterStrip[ZZRefCusMapFilters.System];
			systemFilter.IsActive = true;
			systemFilter.Property = "System";
			var filter = filterStrip.Filter;
			AssertEquals("refCusMap1", true, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", true, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", true, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", false, refCusMap4.MatchesFilter(filter));
			systemFilter.Property = "Not System";
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", false, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", false, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", false, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", true, refCusMap4.MatchesFilter(filter));
			systemFilter.Property = "All";
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", true, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", true, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", true, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", true, refCusMap4.MatchesFilter(filter));
		}

		public void TestMapTypeFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusMapFilterStripBusinessObject();
			var mapTypeFilter = (ModuleTextFilter)filterStrip[ZZRefCusMapFilters.MapType];
			AssertEquals("mapTypeFilter.Visibility", FilterVisibility.AlwaysVisible, mapTypeFilter.Visibility);
			mapTypeFilter.IsActive = true;
			mapTypeFilter.Property = "~AA";
			var filter = filterStrip.Filter;
			AssertEquals("refCusMap1", true, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", false, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", true, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", false, refCusMap4.MatchesFilter(filter));
			mapTypeFilter.Property = "~BB";
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", false, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", true, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", false, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", true, refCusMap4.MatchesFilter(filter));
		}

		public void TestCountryFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusMapFilterStripBusinessObject();
			var countryFilter = (ModuleNkFilter)filterStrip[ZZRefCusMapFilters.CountryOrGrouping];
			AssertEquals("countryFilter.Visibility", FilterVisibility.AlwaysVisible, countryFilter.Visibility);
			AssertEquals("countryFilter.ModuleId", ModuleIDs.Customs.Universal.RefDataGrouping, countryFilter.ModuleId);
			AssertType<RefDataGroupingCollection>("countryFilter.list", countryFilter.List);
			AssertEquals("countryFilter.DefaultProperty", Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), countryFilter.DefaultProperty);
			countryFilter.IsActive = true;
			countryFilter.Property = "ZA";
			var filter = filterStrip.Filter;
			AssertEquals("refCusMap1", true, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", true, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", false, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", false, refCusMap4.MatchesFilter(filter));
			countryFilter.Property = "US";
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", false, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", false, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", true, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", true, refCusMap4.MatchesFilter(filter));
		}

		public void TestCustomsValueFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusMapFilterStripBusinessObject();
			var codeFilter = (ModuleTextFilter)filterStrip[ZZRefCusMapFilters.CustomsValue];
			codeFilter.IsActive = true;
			codeFilter.Property = "11";
			codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			var filter = filterStrip.Filter;
			AssertEquals("refCusMap1", true, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", false, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", false, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", false, refCusMap4.MatchesFilter(filter));
			codeFilter.Property = "22";
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", false, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", true, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", false, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", true, refCusMap4.MatchesFilter(filter));
			codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains;
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", true, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", true, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", false, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", true, refCusMap4.MatchesFilter(filter));
			codeFilter.Property = "23";
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", false, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", true, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", false, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", false, refCusMap4.MatchesFilter(filter));
			codeFilter.Property = "22";
			codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith;
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", true, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", false, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", true, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", false, refCusMap4.MatchesFilter(filter));
			codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain;
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", false, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", false, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", true, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", false, refCusMap4.MatchesFilter(filter));
			codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", false, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", false, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", false, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", false, refCusMap4.MatchesFilter(filter));
			codeFilter.Property = "2244";
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", false, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", false, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", false, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", true, refCusMap4.MatchesFilter(filter));
			codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", true, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", true, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", true, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", false, refCusMap4.MatchesFilter(filter));
		}

		public void TestCommercialValueFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusMapFilterStripBusinessObject();
			var descFilter = (ModuleTextFilter)filterStrip[ZZRefCusMapFilters.CW1OrCommercialValue];
			descFilter.IsActive = true;
			descFilter.Property = "YY";
			descFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			var filter = filterStrip.Filter;
			AssertEquals("refCusMap1", false, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", true, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", false, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", true, refCusMap4.MatchesFilter(filter));
			descFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith;
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", true, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", false, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", true, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", false, refCusMap4.MatchesFilter(filter));
			descFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains;
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", true, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", true, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", true, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", true, refCusMap4.MatchesFilter(filter));
			descFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain;
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", false, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", false, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", false, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", false, refCusMap4.MatchesFilter(filter));
			descFilter.Property = "YYZZ";
			descFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", false, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", true, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", false, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", false, refCusMap4.MatchesFilter(filter));
			descFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter = filterStrip.Filter;
			AssertEquals("refCusMap1", true, refCusMap1.MatchesFilter(filter));
			AssertEquals("refCusMap2", false, refCusMap2.MatchesFilter(filter));
			AssertEquals("refCusMap3", true, refCusMap3.MatchesFilter(filter));
			AssertEquals("refCusMap4", true, refCusMap4.MatchesFilter(filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ZZRefCusMapFilterStripBusinessObject();
		}

		void SetupData()
		{
			var mapType1 = Factory.NewWithValidTestData<RefCusMapType>();
			mapType1.ZZP_MapType = "~AA";
			mapType1.ZZP_Direction = "BTH";
			var mapType2 = Factory.NewWithValidTestData<RefCusMapType>();
			mapType2.ZZP_MapType = "~BB";
			mapType2.ZZP_Direction = "OUT";
			Factory.Save();
			var cusMapPK1 = CreateRefCusMap("~AA", "ZA", "1122", "XXYY");
			var cusMapPK2 = CreateRefCusMap("~BB", "ZA", "2233", "YYZZ");
			var cusMapPK3 = CreateRefCusMap("~AA", "US", "3344", "ZZYY");
			var cusMap4 = Factory.New<ZZRefCusMapCombined>();
			cusMap4.ZZM_ZZP_NKMapType = "~BB";
			cusMap4.ZZM_ZZZ_NKDataGrouping = "US";
			cusMap4.ZZM_CustomsValue = "2244";
			cusMap4.ZZM_CW1orCommercialValue = "YYXX";
			cusMap4.ZZM_StartDate = ZDateTime.Today;
			cusMap4.ZZM_EndDate = ZDateTime.MaxSmallDateTime;
			Factory.Save();
			refCusMap1 = Factory.Load<ZZRefCusMapCombined>(cusMapPK1);
			refCusMap2 = Factory.Load<ZZRefCusMapCombined>(cusMapPK2);
			refCusMap3 = Factory.Load<ZZRefCusMapCombined>(cusMapPK3);
			refCusMap4 = Factory.Load<ZZRefCusMapCombined>(cusMap4.PK);
		}

		ZZRefCusMapCombined refCusMap1;
		ZZRefCusMapCombined refCusMap2;
		ZZRefCusMapCombined refCusMap3;
		ZZRefCusMapCombined refCusMap4;
		public Guid CreateRefCusMap(string mapType, string country, string customsValue, string commercialValue)
		{
			var pk = Guid.NewGuid();
			using (var command = TestConnection.Command(@"
				if not exists (select * from RefDatabase_RefDataGrouping  where ZZZ_DataGrouping= @country)
					INSERT INTO RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) 
					values (newid(), @country, @country)
				if not exists (select * from RefDatabase_RefCusMapType where ZZP_MapType= @mapType)
					INSERT INTO RefDatabase_RefCusMapType ([ZZP_PK], [ZZP_MapType], [ZZP_Direction], [ZZP_Description], [ZZP_IsReadonly])
					values (newid(), @mapType, 'BTH', 'Description', 1)
				INSERT INTO RefDatabase_RefCusMap(ZZM_PK, ZZM_ZZZ_NKDataGrouping, ZZM_ZZP_NKMapType, ZZM_CustomsValue, ZZM_CW1OrCommercialValue, ZZM_StartDate, ZZM_EndDate)
				VALUES (@pk, @country, @mapType, @customsValue, @commercialValue, '1900-01-01', '2079-06-06')"))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, RefCusMapSchema.PK);
				command.AddParameterBasedOnDbColumn("@country", country, RefCusMapSchema.ZZM_ZZZ_NKDataGrouping);
				command.AddParameterBasedOnDbColumn("@mapType", mapType, RefCusMapSchema.ZZM_ZZP_NKMapType);
				command.AddParameterBasedOnDbColumn("@customsValue", customsValue, RefCusMapSchema.ZZM_CustomsValue);
				command.AddParameterBasedOnDbColumn("@commercialValue", commercialValue, RefCusMapSchema.ZZM_CW1orCommercialValue);
				command.ExecuteNonQuery();
			}

			return pk;
		}
	}
}
