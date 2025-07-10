using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.ReferenceFiles.RefCityTown;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefCityTownFilterBusinessObject))]
	sealed class RefCityTownFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestInternationalNameFilter()
		{
			RefCityTown cityTown1 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown1.R9_InternationalName = "TTT";

			RefCityTown cityTown2 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown2.R9_InternationalName = "TTA";

			RefCityTown cityTown3 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown3.R9_InternationalName = "XXX";

			RefCityTown cityTown4 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown4.R9_InternationalName = "XXY";

			Factory.Save();

			RefCityTownFilterBusinessObject internationalNameFilterT = new RefCityTownFilterBusinessObject();
			((ModuleTextFilter)internationalNameFilterT["International Name"]).Property = "T";
			((ModuleTextFilter)internationalNameFilterT["International Name"]).IsActive = true;

			RefCityTownCollection collection1 = new RefCityTownCollection(Factory);
			collection1.AdditionalFilter = internationalNameFilterT.Filter;

			Assert("Collection should contain CityTown1", collection1.Contains(cityTown1));
			Assert("Collection should contain CityTown2", collection1.Contains(cityTown2));
			Assert("Collection should NOT contain CityTown3", !collection1.Contains(cityTown3));
			Assert("Collection should NOT contain CityTown4", !collection1.Contains(cityTown4));

			RefCityTownFilterBusinessObject internationalNameFilterX = new RefCityTownFilterBusinessObject();
			((ModuleTextFilter)internationalNameFilterX["International Name"]).Property = "X";
			((ModuleTextFilter)internationalNameFilterX["International Name"]).IsActive = true;

			RefCityTownCollection collection2 = new RefCityTownCollection(Factory);
			collection2.AdditionalFilter = internationalNameFilterX.Filter;

			Assert("Collection should NOT contain CityTown1", !collection2.Contains(cityTown1));
			Assert("Collection should NOT contain CityTown2", !collection2.Contains(cityTown2));
			Assert("Collection should contain CityTown3", collection2.Contains(cityTown3));
			Assert("Collection should contain CityTown4", collection2.Contains(cityTown4));

			RefCityTownFilterBusinessObject internationalNameFilterZ = new RefCityTownFilterBusinessObject();
			((ModuleTextFilter)internationalNameFilterZ["International Name"]).Property = "Z";
			((ModuleTextFilter)internationalNameFilterZ["International Name"]).IsActive = true;

			RefCityTownCollection collection3 = new RefCityTownCollection(Factory);
			collection3.AdditionalFilter = internationalNameFilterZ.Filter;

			Assert("Collection should NOT contain CityTown1", !collection3.Contains(cityTown1));
			Assert("Collection should NOT contain CityTown2", !collection3.Contains(cityTown2));
			Assert("Collection should NOT contain CityTown3", !collection3.Contains(cityTown3));
			Assert("Collection should NOT contain CityTown4", !collection3.Contains(cityTown4));

			RefCityTownFilterBusinessObject internationalNameFilterTTT = new RefCityTownFilterBusinessObject();
			((ModuleTextFilter)internationalNameFilterTTT["International Name"]).Property = "TTT";
			((ModuleTextFilter)internationalNameFilterTTT["International Name"]).IsActive = true;

			RefCityTownCollection collection4 = new RefCityTownCollection(Factory);
			collection4.AdditionalFilter = internationalNameFilterTTT.Filter;

			Assert("Collection should contain CityTown1", collection4.Contains(cityTown1));
			Assert("Collection should NOT contain CityTown2", !collection4.Contains(cityTown2));
			Assert("Collection should NOT contain CityTown3", !collection4.Contains(cityTown3));
			Assert("Collection should NOT contain CityTown4", !collection4.Contains(cityTown4));
		}

		public void TestLocalLanguageNameFilter()
		{
			RefCityTown cityTown1 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown1.R9_LocalLanguageName = "TTT";

			RefCityTown cityTown2 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown2.R9_LocalLanguageName = "TTA";

			RefCityTown cityTown3 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown3.R9_LocalLanguageName = "XXX";

			RefCityTown cityTown4 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown4.R9_LocalLanguageName = "XXY";

			Factory.Save();

			RefCityTownFilterBusinessObject localLanguageNameFilterT = new RefCityTownFilterBusinessObject();
			((ModuleTextFilter)localLanguageNameFilterT["Local Language Name"]).Property = "T";
			((ModuleTextFilter)localLanguageNameFilterT["Local Language Name"]).IsActive = true;

			RefCityTownCollection collection1 = new RefCityTownCollection(Factory);
			collection1.AdditionalFilter = localLanguageNameFilterT.Filter;

			Assert("Collection should contain CityTown1", collection1.Contains(cityTown1));
			Assert("Collection should contain CityTown2", collection1.Contains(cityTown2));
			Assert("Collection should NOT contain CityTown3", !collection1.Contains(cityTown3));
			Assert("Collection should NOT contain CityTown4", !collection1.Contains(cityTown4));

			RefCityTownFilterBusinessObject localLanguageNameFilterX = new RefCityTownFilterBusinessObject();
			((ModuleTextFilter)localLanguageNameFilterX["Local Language Name"]).Property = "X";
			((ModuleTextFilter)localLanguageNameFilterX["Local Language Name"]).IsActive = true;

			RefCityTownCollection collection2 = new RefCityTownCollection(Factory);
			collection2.AdditionalFilter = localLanguageNameFilterX.Filter;

			Assert("Collection should NOT contain CityTown1", !collection2.Contains(cityTown1));
			Assert("Collection should NOT contain CityTown2", !collection2.Contains(cityTown2));
			Assert("Collection should contain CityTown3", collection2.Contains(cityTown3));
			Assert("Collection should contain CityTown4", collection2.Contains(cityTown4));

			RefCityTownFilterBusinessObject localLanguageNameFilterZ = new RefCityTownFilterBusinessObject();
			((ModuleTextFilter)localLanguageNameFilterZ["Local Language Name"]).Property = "Z";
			((ModuleTextFilter)localLanguageNameFilterZ["Local Language Name"]).IsActive = true;

			RefCityTownCollection collection3 = new RefCityTownCollection(Factory, localLanguageNameFilterZ.Filter);

			Assert("Collection should NOT contain CityTown1", !collection3.Contains(cityTown1));
			Assert("Collection should NOT contain CityTown2", !collection3.Contains(cityTown2));
			Assert("Collection should NOT contain CityTown3", !collection3.Contains(cityTown3));
			Assert("Collection should NOT contain CityTown4", !collection3.Contains(cityTown4));

			RefCityTownFilterBusinessObject localLanguageNameFilterTTT = new RefCityTownFilterBusinessObject();
			((ModuleTextFilter)localLanguageNameFilterTTT["Local Language Name"]).Property = "TTT";
			((ModuleTextFilter)localLanguageNameFilterTTT["Local Language Name"]).IsActive = true;

			RefCityTownCollection collection4 = new RefCityTownCollection(Factory, localLanguageNameFilterTTT.Filter);

			Assert("Collection should contain CityTown1", collection4.Contains(cityTown1));
			Assert("Collection should NOT contain CityTown2", !collection4.Contains(cityTown2));
			Assert("Collection should NOT contain CityTown3", !collection4.Contains(cityTown3));
			Assert("Collection should NOT contain CityTown4", !collection4.Contains(cityTown4));
		}

		public void TestCountryStateFilterInCountryState()
		{
			string exact = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;

			RefCityTown cityTownSpainCA = Factory.NewWithValidTestData<RefCityTown>();
			cityTownSpainCA.R9_RN_NKCountry = "ES";
			cityTownSpainCA.R9_RW_NKState = "CA";

			RefCityTown cityTownSpainCO = Factory.NewWithValidTestData<RefCityTown>();
			cityTownSpainCO.R9_RN_NKCountry = "ES";
			cityTownSpainCO.R9_RW_NKState = "CO";

			RefCityTown cityTownUnitedStatesCA = Factory.NewWithValidTestData<RefCityTown>();
			cityTownUnitedStatesCA.R9_RN_NKCountry = "US";
			cityTownUnitedStatesCA.R9_RW_NKState = "CA";

			RefCityTown cityTownUnitedStatesAZ = Factory.NewWithValidTestData<RefCityTown>();
			cityTownUnitedStatesAZ.R9_RN_NKCountry = "US";
			cityTownUnitedStatesAZ.R9_RW_NKState = "AZ";

			Factory.Save();

			RefCityTownFilterBusinessObject stateFilterESCABizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterESCA = ((RefCityTownCountryStateModuleFilter)stateFilterESCABizO["CountryState"]);
			stateFilterESCA.ComparisonOperator = exact;
			stateFilterESCA.Property1 = "ES";
			stateFilterESCA.Property2 = "CA";
			stateFilterESCA.IsActive = true;
			stateFilterESCA.Validation.ValidateAll();

			AssertNoErrors(stateFilterESCA.Property1Info);
			AssertNoErrors(stateFilterESCA.Property2Info);

			RefCityTownCollection collectionESCA = new RefCityTownCollection(Factory, stateFilterESCABizO.Filter);
			Assert("collectionESCA should contain cityTownSpainCA", collectionESCA.Contains(cityTownSpainCA));
			Assert("collectionESCA should NOT contain cityTownSpainCO", !collectionESCA.Contains(cityTownSpainCO));
			Assert("collectionESCA should NOT contain cityTownUnitedStatesCA", !collectionESCA.Contains(cityTownUnitedStatesCA));
			Assert("collectionESCA should NOT contain cityTownUnitedStatesAZ", !collectionESCA.Contains(cityTownUnitedStatesAZ));

			RefCityTownFilterBusinessObject stateFilterESCOBizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterESCO = ((RefCityTownCountryStateModuleFilter)stateFilterESCOBizO["CountryState"]);
			stateFilterESCO.ComparisonOperator = exact;
			stateFilterESCO.Property1 = "ES";
			stateFilterESCO.Property2 = "CO";
			stateFilterESCO.IsActive = true;
			stateFilterESCO.Validation.ValidateAll();

			AssertNoErrors(stateFilterESCO.Property1Info);
			AssertNoErrors(stateFilterESCO.Property2Info);

			RefCityTownCollection collectionESCO = new RefCityTownCollection(Factory, stateFilterESCOBizO.Filter);
			Assert("collectionESCO should NOT contain cityTownSpainCA", !collectionESCO.Contains(cityTownSpainCA));
			Assert("collectionESCO should contain cityTownSpainCO", collectionESCO.Contains(cityTownSpainCO));
			Assert("collectionESCO should NOT contain cityTownUnitedStatesCA", !collectionESCO.Contains(cityTownUnitedStatesCA));
			Assert("collectionESCO should NOT contain cityTownUnitedStatesAZ", !collectionESCO.Contains(cityTownUnitedStatesAZ));

			RefCityTownFilterBusinessObject stateFilterUSCABizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterUSCA = ((RefCityTownCountryStateModuleFilter)stateFilterUSCABizO["CountryState"]);
			stateFilterUSCA.ComparisonOperator = exact;
			stateFilterUSCA.Property1 = "US";
			stateFilterUSCA.Property2 = "CA";
			stateFilterUSCA.IsActive = true;
			stateFilterUSCA.Validation.ValidateAll();

			AssertNoErrors(stateFilterUSCA.Property1Info);
			AssertNoErrors(stateFilterUSCA.Property2Info);

			RefCityTownCollection collectionUSCA = new RefCityTownCollection(Factory, stateFilterUSCABizO.Filter);
			Assert("collectionUSCA should NOT contain cityTownSpainCA", !collectionUSCA.Contains(cityTownSpainCA));
			Assert("collectionUSCA should NOT contain cityTownSpainCO", !collectionUSCA.Contains(cityTownSpainCO));
			Assert("collectionUSCA should contain cityTownUnitedStatesCA", collectionUSCA.Contains(cityTownUnitedStatesCA));
			Assert("collectionUSCA should NOT contain cityTownUnitedStatesAZ", !collectionUSCA.Contains(cityTownUnitedStatesAZ));

			RefCityTownFilterBusinessObject stateFilterBizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilter = ((RefCityTownCountryStateModuleFilter)stateFilterBizO["CountryState"]);
			stateFilter.ComparisonOperator = exact;
			stateFilter.Property1 = "";
			stateFilter.Property2 = "";
			stateFilter.IsActive = true;
			stateFilter.Validation.ValidateAll();

			AssertNoErrors(stateFilter.Property1Info);
			AssertNoErrors(stateFilter.Property2Info);

			RefCityTownCollection collection = new RefCityTownCollection(Factory, stateFilterBizO.Filter);
			Assert("collection should contain cityTownSpainCA", collection.Contains(cityTownSpainCA));
			Assert("collection should contain cityTownSpainCO", collection.Contains(cityTownSpainCO));
			Assert("collection should contain cityTownUnitedStatesCA", collection.Contains(cityTownUnitedStatesCA));
			Assert("collection should contain cityTownUnitedStatesAZ", collection.Contains(cityTownUnitedStatesAZ));

			RefCityTownFilterBusinessObject stateFilterXXBizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterXX = ((RefCityTownCountryStateModuleFilter)stateFilterXXBizO["CountryState"]);
			stateFilterXX.ComparisonOperator = exact;
			stateFilterXX.Property1 = "XX";
			stateFilterXX.Property2 = "";
			stateFilterXX.IsActive = true;
			stateFilterXX.Validation.ValidateAll();

			AssertHasError(stateFilterXX.Property1Info, "Enter a valid selection.");
			AssertNoErrors(stateFilterXX.Property2Info);

			RefCityTownFilterBusinessObject stateFilterCABizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterCA = ((RefCityTownCountryStateModuleFilter)stateFilterCABizO["CountryState"]);
			stateFilterCA.ComparisonOperator = exact;
			stateFilterCA.Property1 = "";
			stateFilterCA.Property2 = "CA";
			stateFilterCA.IsActive = true;
			stateFilterCA.Validation.ValidateAll();

			AssertNoErrors(stateFilterCA.Property1Info);
			AssertHasError(stateFilterCA.Property2Info, "Enter a country/region first.");

			RefCityTownFilterBusinessObject stateFilterUSBizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterUS = ((RefCityTownCountryStateModuleFilter)stateFilterUSBizO["CountryState"]);
			stateFilterUS.ComparisonOperator = exact;
			stateFilterUS.Property1 = "US";
			stateFilterUS.Property2 = "";
			stateFilterUS.IsActive = true;
			stateFilterUS.Validation.ValidateAll();

			AssertNoErrors(stateFilterUS.Property1Info);
			AssertNoErrors(stateFilterUS.Property2Info);

			RefCityTownCollection collectionUS = new RefCityTownCollection(Factory, stateFilterUSBizO.Filter);
			Assert("collectionUS should NOT contain cityTownSpainCA", !collectionUS.Contains(cityTownSpainCA));
			Assert("collectionUS should NOT contain cityTownSpainCO", !collectionUS.Contains(cityTownSpainCO));
			Assert("collectionUS should contain cityTownUnitedStatesCA", collectionUS.Contains(cityTownUnitedStatesCA));
			Assert("collectionUS should contain cityTownUnitedStatesAZ", collectionUS.Contains(cityTownUnitedStatesAZ));

			RefCityTownFilterBusinessObject stateFilterUSXXBizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterUSXX = ((RefCityTownCountryStateModuleFilter)stateFilterUSXXBizO["CountryState"]);
			stateFilterUSXX.ComparisonOperator = exact;
			stateFilterUSXX.Property1 = "US";
			stateFilterUSXX.Property2 = "XX"; // Invalid state code
			stateFilterUSXX.IsActive = true;
			stateFilterUSXX.Validation.ValidateAll();

			AssertNoErrors(stateFilterUSXX.Property1Info);
			AssertHasError(stateFilterUSXX.Property2Info, "Enter a valid selection.");

			RefCityTownFilterBusinessObject stateFilterUS01BizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterUS01 = ((RefCityTownCountryStateModuleFilter)stateFilterUS01BizO["CountryState"]);
			stateFilterUS01.ComparisonOperator = exact;
			stateFilterUS01.Property1 = "US";
			stateFilterUS01.Property2 = "01"; // Valid state code, but not for US country
			stateFilterUS01.IsActive = true;
			stateFilterUS01.Validation.ValidateAll();

			AssertNoErrors(stateFilterUS01.Property1Info);
			AssertHasError(stateFilterUS01.Property2Info, "State is not in country/region 'US'");
		}

		public void TestCountryStateFilterNotIntCountryState()
		{
			string notEquals = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;

			RefCityTown cityTownSpainCA = Factory.NewWithValidTestData<RefCityTown>();
			cityTownSpainCA.R9_RN_NKCountry = "ES";
			cityTownSpainCA.R9_RW_NKState = "CA";

			RefCityTown cityTownSpainCO = Factory.NewWithValidTestData<RefCityTown>();
			cityTownSpainCO.R9_RN_NKCountry = "ES";
			cityTownSpainCO.R9_RW_NKState = "CO";

			RefCityTown cityTownUnitedStatesCA = Factory.NewWithValidTestData<RefCityTown>();
			cityTownUnitedStatesCA.R9_RN_NKCountry = "US";
			cityTownUnitedStatesCA.R9_RW_NKState = "CA";

			RefCityTown cityTownUnitedStatesAZ = Factory.NewWithValidTestData<RefCityTown>();
			cityTownUnitedStatesAZ.R9_RN_NKCountry = "US";
			cityTownUnitedStatesAZ.R9_RW_NKState = "AZ";

			Factory.Save();

			RefCityTownFilterBusinessObject stateFilterESCABizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterESCA = ((RefCityTownCountryStateModuleFilter)stateFilterESCABizO["CountryState"]);
			stateFilterESCA.ComparisonOperator = notEquals;
			stateFilterESCA.Property1 = "ES";
			stateFilterESCA.Property2 = "CA";
			stateFilterESCA.IsActive = true;
			stateFilterESCA.Validation.ValidateAll();

			AssertNoErrors(stateFilterESCA.Property1Info);
			AssertNoErrors(stateFilterESCA.Property2Info);

			RefCityTownCollection collectionNotESCA = new RefCityTownCollection(Factory, stateFilterESCABizO.Filter);
			Assert("collectionNotESCA should NOT contain cityTownSpainCA", !collectionNotESCA.Contains(cityTownSpainCA));
			Assert("collectionNotESCA should contain cityTownSpainCO", collectionNotESCA.Contains(cityTownSpainCO));
			Assert("collectionNotESCA should contain cityTownUnitedStatesCA", collectionNotESCA.Contains(cityTownUnitedStatesCA));
			Assert("collectionNotESCA should contain cityTownUnitedStatesAZ", collectionNotESCA.Contains(cityTownUnitedStatesAZ));

			RefCityTownFilterBusinessObject stateFilterESCOBizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterESCO = ((RefCityTownCountryStateModuleFilter)stateFilterESCOBizO["CountryState"]);
			stateFilterESCO.ComparisonOperator = notEquals;
			stateFilterESCO.Property1 = "ES";
			stateFilterESCO.Property2 = "CO";
			stateFilterESCO.IsActive = true;
			stateFilterESCO.Validation.ValidateAll();

			AssertNoErrors(stateFilterESCO.Property1Info);
			AssertNoErrors(stateFilterESCO.Property2Info);

			RefCityTownCollection collectionNotESCO = new RefCityTownCollection(Factory, stateFilterESCOBizO.Filter);
			Assert("collectionNotESCO should contain cityTownSpainCA", collectionNotESCO.Contains(cityTownSpainCA));
			Assert("collectionNotESCO should NOT contain cityTownSpainCO", !collectionNotESCO.Contains(cityTownSpainCO));
			Assert("collectionNotESCO should contain cityTownUnitedStatesCA", collectionNotESCO.Contains(cityTownUnitedStatesCA));
			Assert("collectionNotESCO should contain cityTownUnitedStatesAZ", collectionNotESCO.Contains(cityTownUnitedStatesAZ));

			RefCityTownFilterBusinessObject stateFilterUSCABizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterUSCA = ((RefCityTownCountryStateModuleFilter)stateFilterUSCABizO["CountryState"]);
			stateFilterUSCA.ComparisonOperator = notEquals;
			stateFilterUSCA.Property1 = "US";
			stateFilterUSCA.Property2 = "CA";
			stateFilterUSCA.IsActive = true;
			stateFilterUSCA.Validation.ValidateAll();

			AssertNoErrors(stateFilterUSCA.Property1Info);
			AssertNoErrors(stateFilterUSCA.Property2Info);

			RefCityTownCollection collectionNotUSCA = new RefCityTownCollection(Factory, stateFilterUSCABizO.Filter);
			Assert("collectionNotUSCA should contain cityTownSpainCA", collectionNotUSCA.Contains(cityTownSpainCA));
			Assert("collectiocollectionNotUSCAnUSCA should contain cityTownSpainCO", collectionNotUSCA.Contains(cityTownSpainCO));
			Assert("collectionNotUSCA should NOT contain cityTownUnitedStatesCA", !collectionNotUSCA.Contains(cityTownUnitedStatesCA));
			Assert("collectionNotUSCA should contain cityTownUnitedStatesAZ", collectionNotUSCA.Contains(cityTownUnitedStatesAZ));

			RefCityTownFilterBusinessObject stateFilterBizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilter = ((RefCityTownCountryStateModuleFilter)stateFilterBizO["CountryState"]);
			stateFilter.ComparisonOperator = notEquals;
			stateFilter.Property1 = "";
			stateFilter.Property2 = "";
			stateFilter.IsActive = true;
			stateFilter.Validation.ValidateAll();

			AssertNoErrors(stateFilter.Property1Info);
			AssertNoErrors(stateFilter.Property2Info);

			RefCityTownCollection collectionNot = new RefCityTownCollection(Factory, stateFilterBizO.Filter);
			Assert("collectionNot should contain cityTownSpainCA", collectionNot.Contains(cityTownSpainCA));
			Assert("collectionNot should contain cityTownSpainCO", collectionNot.Contains(cityTownSpainCO));
			Assert("collectionNot should contain cityTownUnitedStatesCA", collectionNot.Contains(cityTownUnitedStatesCA));
			Assert("collectionNot should contain cityTownUnitedStatesAZ", collectionNot.Contains(cityTownUnitedStatesAZ));

			RefCityTownFilterBusinessObject stateFilterXXBizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterXX = ((RefCityTownCountryStateModuleFilter)stateFilterXXBizO["CountryState"]);
			stateFilterXX.ComparisonOperator = notEquals;
			stateFilterXX.Property1 = "XX";
			stateFilterXX.Property2 = "";
			stateFilterXX.IsActive = true;
			stateFilterXX.Validation.ValidateAll();

			AssertHasError(stateFilterXX.Property1Info, "Enter a valid selection.");
			AssertNoErrors(stateFilterXX.Property2Info);

			RefCityTownFilterBusinessObject stateFilterCABizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterCA = ((RefCityTownCountryStateModuleFilter)stateFilterCABizO["CountryState"]);
			stateFilterCA.ComparisonOperator = notEquals;
			stateFilterCA.Property1 = "";
			stateFilterCA.Property2 = "CA";
			stateFilterCA.IsActive = true;
			stateFilterCA.Validation.ValidateAll();

			AssertNoErrors(stateFilterCA.Property1Info);
			AssertHasError(stateFilterCA.Property2Info, "Enter a country/region first.");

			RefCityTownFilterBusinessObject stateFilterUSBizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterUS = ((RefCityTownCountryStateModuleFilter)stateFilterUSBizO["CountryState"]);
			stateFilterUS.ComparisonOperator = notEquals;
			stateFilterUS.Property1 = "US";
			stateFilterUS.Property2 = "";
			stateFilterUS.IsActive = true;
			stateFilterUS.Validation.ValidateAll();

			AssertNoErrors(stateFilterUS.Property1Info);
			AssertNoErrors(stateFilterUS.Property2Info);

			RefCityTownCollection collectionNotUS = new RefCityTownCollection(Factory, stateFilterUSBizO.Filter);
			Assert("collectionNotUS should contain cityTownSpainCA", collectionNotUS.Contains(cityTownSpainCA));
			Assert("collectionNotUS should contain cityTownSpainCO", collectionNotUS.Contains(cityTownSpainCO));
			Assert("collectionNotUS should NOT contain cityTownUnitedStatesCA", !collectionNotUS.Contains(cityTownUnitedStatesCA));
			Assert("collectionNotUS should NOT contain cityTownUnitedStatesAZ", !collectionNotUS.Contains(cityTownUnitedStatesAZ));

			RefCityTownFilterBusinessObject stateFilterUSXXBizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterUSXX = ((RefCityTownCountryStateModuleFilter)stateFilterUSXXBizO["CountryState"]);
			stateFilterUSXX.ComparisonOperator = notEquals;
			stateFilterUSXX.Property1 = "US";
			stateFilterUSXX.Property2 = "XX"; // Invalid state code
			stateFilterUSXX.IsActive = true;
			stateFilterUSXX.Validation.ValidateAll();

			AssertNoErrors(stateFilterUSXX.Property1Info);
			AssertHasError(stateFilterUSXX.Property2Info, "Enter a valid selection.");

			RefCityTownFilterBusinessObject stateFilterUS01BizO = new RefCityTownFilterBusinessObject();
			RefCityTownCountryStateModuleFilter stateFilterUS01 = ((RefCityTownCountryStateModuleFilter)stateFilterUS01BizO["CountryState"]);
			stateFilterUS01.ComparisonOperator = notEquals;
			stateFilterUS01.Property1 = "US";
			stateFilterUS01.Property2 = "01"; // Valid state code, but not for US country
			stateFilterUS01.IsActive = true;
			stateFilterUS01.Validation.ValidateAll();

			AssertNoErrors(stateFilterUS01.Property1Info);
			AssertHasError(stateFilterUS01.Property2Info, "State is not in country/region 'US'");
		}

		public void TestPostCodeFilter()
		{
			RefPostCode postCode1 = Factory.NewWithValidTestData<RefPostCode>();
			RefPostCode postCode2 = Factory.NewWithValidTestData<RefPostCode>();
			RefPostCode postCode3 = Factory.NewWithValidTestData<RefPostCode>();

			RefCityTown cityTown1 = Factory.NewWithValidTestData<RefCityTown>();
			RefCityTown cityTown2 = Factory.NewWithValidTestData<RefCityTown>();
			RefCityTown cityTown3 = Factory.NewWithValidTestData<RefCityTown>();

			cityTown1.PostCodes.Add(postCode1);
			cityTown1.PostCodes.Add(postCode2);
			cityTown2.PostCodes.Add(postCode1);

			Factory.Save();

			RefCityTownFilterBusinessObject postCode1Filter = new RefCityTownFilterBusinessObject();
			((ModuleGuidFilter)postCode1Filter["Postcode"]).Property = postCode1.PK;
			((ModuleGuidFilter)postCode1Filter["Postcode"]).IsActive = true;

			RefCityTownCollection cityTownCollection = new RefCityTownCollection(Factory, postCode1Filter.Filter);

			Assert("Filtered for PostCode 1 CityTown, Collection should contain CityTown1", cityTownCollection.Contains(cityTown1));
			Assert("Filtered for PostCode 1 CityTown, Collection should contain CityTown2", cityTownCollection.Contains(cityTown2));
			Assert("Filtered for PostCode 1 CityTonw, Collection should NOT contain CityTown3", !cityTownCollection.Contains(cityTown3));

			RefCityTownFilterBusinessObject postCode2Filter = new RefCityTownFilterBusinessObject();
			((ModuleGuidFilter)postCode2Filter["Postcode"]).Property = postCode2.PK;
			((ModuleGuidFilter)postCode2Filter["Postcode"]).IsActive = true;

			cityTownCollection = new RefCityTownCollection(Factory, postCode2Filter.Filter);

			Assert("Filtered for PostCode 2 CityTown, Collection should contain CityTown1", cityTownCollection.Contains(cityTown1));
			Assert("Filtered for PostCode 2 CityTown, Collection should NOT contain CityTown2", !cityTownCollection.Contains(cityTown2));
			Assert("Filtered for PostCode 2 CityTown, Collection should NOT contain CityTown3", !cityTownCollection.Contains(cityTown3));

			RefCityTownFilterBusinessObject postCode3Filter = new RefCityTownFilterBusinessObject();
			((ModuleGuidFilter)postCode3Filter["Postcode"]).Property = postCode3.PK;
			((ModuleGuidFilter)postCode3Filter["Postcode"]).IsActive = true;

			cityTownCollection = new RefCityTownCollection(Factory, postCode3Filter.Filter);

			Assert("Filtered for PostCode 3 CityTown, Collection should NOT contain CityTown1", !cityTownCollection.Contains(cityTown1));
			Assert("Filtered for PostCode 3 CityTown, Collection should NOT contain CityTown2", !cityTownCollection.Contains(cityTown2));
			Assert("Filtered for PostCode 3 CityTown, Collection should NOT contain CityTown3", !cityTownCollection.Contains(cityTown3));

			RefCityTownFilterBusinessObject emptyFilter = new RefCityTownFilterBusinessObject();
			((ModuleGuidFilter)emptyFilter["Postcode"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)emptyFilter["Postcode"]).IsActive = false;

			cityTownCollection = new RefCityTownCollection(Factory, emptyFilter.Filter);

			Assert("Filtered for PostCode 3 CityTown, Collection should contain CityTown1", cityTownCollection.Contains(cityTown1));
			Assert("Filtered for PostCode 3 CityTown, Collection should contain CityTown2", cityTownCollection.Contains(cityTown2));
			Assert("Filtered for PostCode 3 CityTown, Collection should contain CityTown3", cityTownCollection.Contains(cityTown3));
		}

		public void TestZoneSetFilter()
		{
			RefCityTown cityTown1 = Factory.NewWithValidTestData<RefCityTown>();
			RefCityTown cityTown2 = Factory.NewWithValidTestData<RefCityTown>();
			RefCityTown cityTown3 = Factory.NewWithValidTestData<RefCityTown>();
			RefCityTown cityTown4 = Factory.NewWithValidTestData<RefCityTown>();

			IRateTransportProvider provider = (IRateTransportProvider)Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportProvider>());
			provider.TP_RN_NKCountry = "AU";
			provider.TP_ZoneType = "All";

			var zone = (IRateTransportZone)provider.Zones.AddNew();
			var zoneItem1 = (IRateTransportZoneItem)zone.Items.AddNew();
			zoneItem1.TQ_R9_CityTown = cityTown1.PK;
			var zoneItem2 = (IRateTransportZoneItem)zone.Items.AddNew();
			zoneItem2.TQ_R9_CityTown = cityTown2.PK;

			IRateTransportProvider provider2 = (IRateTransportProvider)Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportProvider>());
			provider2.TP_RN_NKCountry = "NZ";
			provider2.TP_ZoneType = "RAT";

			var zone2 = (IRateTransportZone)provider2.Zones.AddNew();
			var zoneItem3 = (IRateTransportZoneItem)zone2.Items.AddNew();
			zoneItem3.TQ_R9_CityTown = cityTown3.PK;

			Factory.Save();

			RefCityTownFilterBusinessObject zoneSetFilter1 = new RefCityTownFilterBusinessObject();
			((ModuleGuidFilter)zoneSetFilter1["Transport Zone Membership"]).Property = provider.PK;
			((ModuleGuidFilter)zoneSetFilter1["Transport Zone Membership"]).IsActive = true;
			((ModuleGuidFilter)zoneSetFilter1["Transport Zone Membership"]).SqlComparisonOperator = SQLComparisonOperator.Equal;

			RefCityTownCollection cityTownCollection = new RefCityTownCollection(Factory, zoneSetFilter1.Filter);
			Assert("Filtered for zoneSetFilter 1, Collection should contain CityTown1", cityTownCollection.Contains(cityTown1));
			Assert("Filtered for zoneSetFilter 1, Collection should contain CityTown2", cityTownCollection.Contains(cityTown2));
			Assert("Filtered for zoneSetFilter 1, Collection should NOT contain CityTown3", !cityTownCollection.Contains(cityTown3));
			Assert("Filtered for zoneSetFilter 1, Collection should NOT contain CityTown4", !cityTownCollection.Contains(cityTown4));

			RefCityTownFilterBusinessObject zoneSetFilter2 = new RefCityTownFilterBusinessObject();
			((ModuleGuidFilter)zoneSetFilter2["Transport Zone Membership"]).Property = provider.PK;
			((ModuleGuidFilter)zoneSetFilter2["Transport Zone Membership"]).IsActive = true;
			((ModuleGuidFilter)zoneSetFilter2["Transport Zone Membership"]).SqlComparisonOperator = SQLComparisonOperator.NotEqual;

			cityTownCollection = new RefCityTownCollection(Factory, zoneSetFilter2.Filter);
			Assert("Filtered for zoneSetFilter 2, Collection should NOT contain CityTown1", !cityTownCollection.Contains(cityTown1));
			Assert("Filtered for zoneSetFilter 2, Collection should NOT contain CityTown2", !cityTownCollection.Contains(cityTown2));
			Assert("Filtered for zoneSetFilter 2, Collection should contain CityTown3", cityTownCollection.Contains(cityTown3));
			Assert("Filtered for zoneSetFilter 2, Collection should contain CityTown4", cityTownCollection.Contains(cityTown4));

			RefCityTownFilterBusinessObject zoneSetFilter3 = new RefCityTownFilterBusinessObject();
			((ModuleGuidFilter)zoneSetFilter3["Transport Zone Membership"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)zoneSetFilter3["Transport Zone Membership"]).IsActive = true;
			((ModuleGuidFilter)zoneSetFilter3["Transport Zone Membership"]).SqlComparisonOperator = SpecialComparisonOperator.IsBlank;

			cityTownCollection = new RefCityTownCollection(Factory, zoneSetFilter3.Filter);
			Assert("Filtered for zoneSetFilter 3, Collection should NOT contain CityTown1", !cityTownCollection.Contains(cityTown1));
			Assert("Filtered for zoneSetFilter 3, Collection should NOT contain CityTown2", !cityTownCollection.Contains(cityTown2));
			Assert("Filtered for zoneSetFilter 3, Collection should NOT contain CityTown3", !cityTownCollection.Contains(cityTown3));
			Assert("Filtered for zoneSetFilter 3, Collection should contain CityTown4", cityTownCollection.Contains(cityTown4));

			RefCityTownFilterBusinessObject zoneSetFilter4 = new RefCityTownFilterBusinessObject();
			((ModuleGuidFilter)zoneSetFilter4["Transport Zone Membership"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)zoneSetFilter4["Transport Zone Membership"]).IsActive = true;
			((ModuleGuidFilter)zoneSetFilter4["Transport Zone Membership"]).SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;

			cityTownCollection = new RefCityTownCollection(Factory, zoneSetFilter4.Filter);
			Assert("Filtered for zoneSetFilter 4, Collection should contain CityTown1", cityTownCollection.Contains(cityTown1));
			Assert("Filtered for zoneSetFilter 4, Collection should contain CityTown2", cityTownCollection.Contains(cityTown2));
			Assert("Filtered for zoneSetFilter 4, Collection should contain CityTown3", cityTownCollection.Contains(cityTown3));
			Assert("Filtered for zoneSetFilter 4, Collection should NOT contain CityTown4", !cityTownCollection.Contains(cityTown4));
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefCityTownFilterBusinessObject();
		}

		#endregion
	}
}
