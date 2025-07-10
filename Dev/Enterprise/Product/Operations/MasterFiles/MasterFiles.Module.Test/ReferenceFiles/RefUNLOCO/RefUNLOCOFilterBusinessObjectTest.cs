using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.ReferenceFiles.RefUNLOCO;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefUNLOCOFilterBusinessObject))]
	sealed class RefUNLOCOFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region ACI Filters

		public void TestACI()
		{
			RefUNLOCOFilterBusinessObject filterStripBizO = (RefUNLOCOFilterBusinessObject)GetNewFilterStripBusinessObject();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertNull(filterStripBizO["Servicing Postal Code"]);
			AssertNull(filterStripBizO["Airport Radius Search Distance"]);

			filterStripBizO = (RefUNLOCOFilterBusinessObject)GetNewFilterStripBusinessObject();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			AssertNotNull(filterStripBizO["Servicing Postal Code"]);
			AssertNotNull(filterStripBizO["Airport Radius Search Distance"]);

			RefDomesticCartageZone refDomesticCartageZone = Factory.New<RefDomesticCartageZone>();
			refDomesticCartageZone.F1_CityTownPostCode = "4300";
			refDomesticCartageZone.F1_RL_NKLoco = "AUSYD";
			refDomesticCartageZone.F1_Distance = 15m;

			Factory.Save();

			ModuleTextFilter postCodeFilter = (ModuleTextFilter)filterStripBizO["Servicing Postal Code"];
			postCodeFilter.IsActive = true;
			postCodeFilter.Property = "4300";

			ModuleTextFilter distainceFilter = (ModuleTextFilter)filterStripBizO["Airport Radius Search Distance"];
			distainceFilter.IsActive = true;
			distainceFilter.Property = "20";

			RefUNLOCOCollection refUNLOCOCollection = new RefUNLOCOCollection(Factory, filterStripBizO.Filter);
			AssertEquals(1, refUNLOCOCollection.Count);
			AssertEquals("AUSYD", refUNLOCOCollection[0].RL_Code);

			distainceFilter.Property = "10";
			refUNLOCOCollection = new RefUNLOCOCollection(Factory, filterStripBizO.Filter);
			AssertEquals(0, refUNLOCOCollection.Count);

			distainceFilter.Property = "30";
			postCodeFilter.Property = "1111";
			refUNLOCOCollection = new RefUNLOCOCollection(Factory, filterStripBizO.Filter);
			AssertEquals(0, refUNLOCOCollection.Count);
		}

		#endregion

		#region Filters

		readonly ZGuid stateESCAGuid = new ZGuid("9bcbbbeb-b5c4-4912-9332-49ebb1abf1dc");
		readonly ZGuid stateESCOGuid = new ZGuid("ce3d5337-5955-41a3-bfce-feef65f3ac8b");
		readonly ZGuid stateUSCAGuid = new ZGuid("62c2e725-11b0-4d20-b3f3-f66768bfc765");
		readonly ZGuid stateUSAZGuid = new ZGuid("9f5abef2-5088-4fe8-be69-ef415c93f263");

		public void TestFilterAddedToModule()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.RefUNLOCO))
			{
				var stripBO = module.FilterBusinessObject;
				var isSystemFilter = stripBO["Is System Defined"];
				var isSystemUpdatableFilter = stripBO["Is System Updatable"];
				var hasAirportFilter = stripBO["Has Airport"];
				var hasSeaportFilter = stripBO["Has Seaport"];

				AssertNotNull(isSystemFilter);
				AssertNotNull(isSystemUpdatableFilter);
				AssertNotNull(hasAirportFilter);
				AssertNotNull(hasSeaportFilter);
				AssertEquals("Is System Filter description is not correct", "Is System Defined", isSystemFilter.Description);
				AssertEquals("Is System Updatable is not correct", "Is System Updatable", isSystemUpdatableFilter.Description);
				AssertEquals("Has Airport is not correct", "Has Airport", hasAirportFilter.Description);
				AssertEquals("Has Seaport is not correct", "Has Seaport", hasSeaportFilter.Description);
			}
		}

		public void TestCountryStateFilterInCountryState()
		{
			string exact = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;

			RefUNLOCO uNLOCOSpainCA = Factory.NewWithValidTestData<RefUNLOCO>();
			uNLOCOSpainCA.RL_RN_NKCountryCode = "ES";
			uNLOCOSpainCA.RL_RW = stateESCAGuid;

			RefUNLOCO uNLOCOSpainCO = Factory.NewWithValidTestData<RefUNLOCO>();
			uNLOCOSpainCO.RL_RN_NKCountryCode = "ES";
			uNLOCOSpainCO.RL_RW = stateESCOGuid;

			RefUNLOCO uNLOCOUnitedStatesCA = Factory.NewWithValidTestData<RefUNLOCO>();
			uNLOCOUnitedStatesCA.RL_RN_NKCountryCode = "US";
			uNLOCOUnitedStatesCA.RL_RW = stateUSCAGuid;

			RefUNLOCO uNLOCOUnitedStatesAZ = Factory.NewWithValidTestData<RefUNLOCO>();
			uNLOCOUnitedStatesAZ.RL_RN_NKCountryCode = "US";
			uNLOCOUnitedStatesAZ.RL_RW = stateUSAZGuid;
			Factory.Save();

			RefUNLOCOFilterBusinessObject stateFilterESCABizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterESCA = ((RefUNLOCOCountryStateModuleFilter)stateFilterESCABizO["CountryState"]);
			stateFilterESCA.ComparisonOperator = exact;
			stateFilterESCA.Property1 = "ES";
			stateFilterESCA.Property2 = stateESCAGuid;
			stateFilterESCA.IsActive = true;
			stateFilterESCA.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterESCA.Property1Info.MaxLength);
			AssertNoErrors(stateFilterESCA.Property1Info);
			AssertNoErrors(stateFilterESCA.Property2Info);

			RefUNLOCOCollection collectionESCA = new RefUNLOCOCollection(Factory, stateFilterESCABizO.Filter);
			Assert("collectionESCA should contain UNLOCOSpainCA", collectionESCA.Contains(uNLOCOSpainCA));
			Assert("collectionESCA should NOT contain UNLOCOSpainCO", !collectionESCA.Contains(uNLOCOSpainCO));
			Assert("collectionESCA should NOT contain UNLOCOUnitedStatesCA", !collectionESCA.Contains(uNLOCOUnitedStatesCA));
			Assert("collectionESCA should NOT contain UNLOCOUnitedStatesAZ", !collectionESCA.Contains(uNLOCOUnitedStatesAZ));

			RefUNLOCOFilterBusinessObject stateFilterESCOBizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterESCO = ((RefUNLOCOCountryStateModuleFilter)stateFilterESCOBizO["CountryState"]);
			stateFilterESCO.ComparisonOperator = exact;
			stateFilterESCO.Property1 = "ES";
			stateFilterESCO.Property2 = stateESCOGuid;
			stateFilterESCO.IsActive = true;
			stateFilterESCO.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterESCO.Property1Info.MaxLength);
			AssertNoErrors(stateFilterESCO.Property1Info);
			AssertNoErrors(stateFilterESCO.Property2Info);

			RefUNLOCOCollection collectionESCO = new RefUNLOCOCollection(Factory, stateFilterESCOBizO.Filter);
			Assert("collectionESCO should NOT contain UNLOCOSpainCA", !collectionESCO.Contains(uNLOCOSpainCA));
			Assert("collectionESCO should contain UNLOCOSpainCO", collectionESCO.Contains(uNLOCOSpainCO));
			Assert("collectionESCO should NOT contain UNLOCOUnitedStatesCA", !collectionESCO.Contains(uNLOCOUnitedStatesCA));
			Assert("collectionESCO should NOT contain UNLOCOUnitedStatesAZ", !collectionESCO.Contains(uNLOCOUnitedStatesAZ));

			RefUNLOCOFilterBusinessObject stateFilterUSCABizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterUSCA = ((RefUNLOCOCountryStateModuleFilter)stateFilterUSCABizO["CountryState"]);
			stateFilterUSCA.ComparisonOperator = exact;
			stateFilterUSCA.Property1 = "US";
			stateFilterUSCA.Property2 = stateUSCAGuid;
			stateFilterUSCA.IsActive = true;
			stateFilterUSCA.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterUSCA.Property1Info.MaxLength);
			AssertNoErrors(stateFilterUSCA.Property1Info);
			AssertNoErrors(stateFilterUSCA.Property2Info);

			RefUNLOCOCollection collectionUSCA = new RefUNLOCOCollection(Factory, stateFilterUSCABizO.Filter);
			Assert("collectionUSCA should NOT contain UNLOCOSpainCA", !collectionUSCA.Contains(uNLOCOSpainCA));
			Assert("collectionUSCA should NOT contain UNLOCOSpainCO", !collectionUSCA.Contains(uNLOCOSpainCO));
			Assert("collectionUSCA should contain UNLOCOUnitedStatesCA", collectionUSCA.Contains(uNLOCOUnitedStatesCA));
			Assert("collectionUSCA should NOT contain UNLOCOUnitedStatesAZ", !collectionUSCA.Contains(uNLOCOUnitedStatesAZ));

			RefUNLOCOFilterBusinessObject stateFilterBizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilter = ((RefUNLOCOCountryStateModuleFilter)stateFilterBizO["CountryState"]);
			stateFilter.ComparisonOperator = exact;
			stateFilter.Property1 = "";
			stateFilter.Property2 = new ZGuid();
			stateFilter.IsActive = true;
			stateFilter.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilter.Property1Info.MaxLength);
			AssertNoErrors(stateFilter.Property1Info);
			AssertNoErrors(stateFilter.Property2Info);

			RefUNLOCOCollection collection = new RefUNLOCOCollection(Factory, stateFilterBizO.Filter);
			Assert("collection should contain UNLOCOSpainCA", collection.Contains(uNLOCOSpainCA));
			Assert("collection should contain UNLOCOSpainCO", collection.Contains(uNLOCOSpainCO));
			Assert("collection should contain UNLOCOUnitedStatesCA", collection.Contains(uNLOCOUnitedStatesCA));
			Assert("collection should contain UNLOCOUnitedStatesAZ", collection.Contains(uNLOCOUnitedStatesAZ));

			RefUNLOCOFilterBusinessObject stateFilterXXBizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterXX = ((RefUNLOCOCountryStateModuleFilter)stateFilterXXBizO["CountryState"]);
			stateFilterXX.ComparisonOperator = exact;
			stateFilterXX.Property1 = "XX";
			stateFilterXX.Property2 = new ZGuid();
			stateFilterXX.IsActive = true;
			stateFilterXX.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterXX.Property1Info.MaxLength);
			AssertHasError(stateFilterXX.Property1Info, "Enter a valid selection.");
			AssertNoErrors(stateFilterXX.Property2Info);

			RefUNLOCOFilterBusinessObject stateFilterCABizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterCA = ((RefUNLOCOCountryStateModuleFilter)stateFilterCABizO["CountryState"]);
			stateFilterCA.ComparisonOperator = exact;
			stateFilterCA.Property1 = "";
			stateFilterCA.Property2 = stateUSCAGuid;
			stateFilterCA.IsActive = true;
			stateFilterCA.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterCA.Property1Info.MaxLength);
			AssertNoErrors(stateFilterCA.Property1Info);
			AssertHasError(stateFilterCA.Property2Info, "Enter a country/region first.");

			RefUNLOCOFilterBusinessObject stateFilterUSBizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterUS = ((RefUNLOCOCountryStateModuleFilter)stateFilterUSBizO["CountryState"]);
			stateFilterUS.ComparisonOperator = exact;
			stateFilterUS.Property1 = "US";
			stateFilterUS.Property2 = new ZGuid();
			stateFilterUS.IsActive = true;
			stateFilterUS.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterUS.Property1Info.MaxLength);
			AssertNoErrors(stateFilterUS.Property1Info);
			AssertNoErrors(stateFilterUS.Property2Info);

			RefUNLOCOCollection collectionUS = new RefUNLOCOCollection(Factory, stateFilterUSBizO.Filter);
			Assert("collectionUS should NOT contain UNLOCOSpainCA", !collectionUS.Contains(uNLOCOSpainCA));
			Assert("collectionUS should NOT contain UNLOCOSpainCO", !collectionUS.Contains(uNLOCOSpainCO));
			Assert("collectionUS should contain UNLOCOUnitedStatesCA", collectionUS.Contains(uNLOCOUnitedStatesCA));
			Assert("collectionUS should contain UNLOCOUnitedStatesAZ", collectionUS.Contains(uNLOCOUnitedStatesAZ));

			RefUNLOCOFilterBusinessObject stateFilterUSXXBizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterUSXX = ((RefUNLOCOCountryStateModuleFilter)stateFilterUSXXBizO["CountryState"]);
			stateFilterUSXX.ComparisonOperator = exact;
			stateFilterUSXX.Property1 = "US";
			stateFilterUSXX.Property2 = new ZGuid("471BAE0F-87DD-4E60-96A0-6D318F39B8FE"); // Invalid state code
			stateFilterUSXX.IsActive = true;
			stateFilterUSXX.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterUSXX.Property1Info.MaxLength);
			AssertNoErrors(stateFilterUSXX.Property1Info);
			AssertHasError(stateFilterUSXX.Property2Info, "Enter a valid selection.");

			RefUNLOCOFilterBusinessObject stateFilterUS01BizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterUS01 = ((RefUNLOCOCountryStateModuleFilter)stateFilterUS01BizO["CountryState"]);
			stateFilterUS01.ComparisonOperator = exact;
			stateFilterUS01.Property1 = "US";
			stateFilterUS01.Property2 = new ZGuid("1cb2fa82-7e0f-4d44-acce-b7c46b2c7216"); // Valid state code, but not for US country
			stateFilterUS01.IsActive = true;
			stateFilterUS01.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterUS01.Property1Info.MaxLength);
			AssertNoErrors(stateFilterUS01.Property1Info);
			AssertHasError(stateFilterUS01.Property2Info, "State is not in country/region 'US'");
		}

		public void TestCountryStateFilterNotIntCountryState()
		{
			string notEquals = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;

			RefUNLOCO uNLOCOSpainCA = Factory.NewWithValidTestData<RefUNLOCO>();
			uNLOCOSpainCA.RL_RN_NKCountryCode = "ES";
			uNLOCOSpainCA.RL_RW = stateESCAGuid;

			RefUNLOCO uNLOCOSpainCO = Factory.NewWithValidTestData<RefUNLOCO>();
			uNLOCOSpainCO.RL_RN_NKCountryCode = "ES";
			uNLOCOSpainCO.RL_RW = stateESCOGuid;

			RefUNLOCO uNLOCOUnitedStatesCA = Factory.NewWithValidTestData<RefUNLOCO>();
			uNLOCOUnitedStatesCA.RL_RN_NKCountryCode = "US";
			uNLOCOUnitedStatesCA.RL_RW = stateUSCAGuid;

			RefUNLOCO uNLOCOUnitedStatesAZ = Factory.NewWithValidTestData<RefUNLOCO>();
			uNLOCOUnitedStatesAZ.RL_RN_NKCountryCode = "US";
			uNLOCOUnitedStatesAZ.RL_RW = stateUSAZGuid;

			Factory.Save();

			RefUNLOCOFilterBusinessObject stateFilterESCABizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterESCA = ((RefUNLOCOCountryStateModuleFilter)stateFilterESCABizO["CountryState"]);
			stateFilterESCA.ComparisonOperator = notEquals;
			stateFilterESCA.Property1 = "ES";
			stateFilterESCA.Property2 = stateESCAGuid;
			stateFilterESCA.IsActive = true;
			stateFilterESCA.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterESCA.Property1Info.MaxLength);
			AssertNoErrors(stateFilterESCA.Property1Info);
			AssertNoErrors(stateFilterESCA.Property2Info);

			RefUNLOCOCollection collectionNotESCA = new RefUNLOCOCollection(Factory, stateFilterESCABizO.Filter);
			Assert("collectionNotESCA should NOT contain UNLOCOSpainCA", !collectionNotESCA.Contains(uNLOCOSpainCA));
			Assert("collectionNotESCA should contain UNLOCOSpainCO", collectionNotESCA.Contains(uNLOCOSpainCO));
			Assert("collectionNotESCA should contain UNLOCOUnitedStatesCA", collectionNotESCA.Contains(uNLOCOUnitedStatesCA));
			Assert("collectionNotESCA should contain UNLOCOUnitedStatesAZ", collectionNotESCA.Contains(uNLOCOUnitedStatesAZ));

			RefUNLOCOFilterBusinessObject stateFilterESCOBizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterESCO = ((RefUNLOCOCountryStateModuleFilter)stateFilterESCOBizO["CountryState"]);
			stateFilterESCO.ComparisonOperator = notEquals;
			stateFilterESCO.Property1 = "ES";
			stateFilterESCO.Property2 = stateESCOGuid;
			stateFilterESCO.IsActive = true;
			stateFilterESCO.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterESCO.Property1Info.MaxLength);
			AssertNoErrors(stateFilterESCO.Property1Info);
			AssertNoErrors(stateFilterESCO.Property2Info);

			RefUNLOCOCollection collectionNotESCO = new RefUNLOCOCollection(Factory, stateFilterESCOBizO.Filter);
			Assert("collectionNotESCO should contain UNLOCOSpainCA", collectionNotESCO.Contains(uNLOCOSpainCA));
			Assert("collectionNotESCO should NOT contain UNLOCOSpainCO", !collectionNotESCO.Contains(uNLOCOSpainCO));
			Assert("collectionNotESCO should contain UNLOCOUnitedStatesCA", collectionNotESCO.Contains(uNLOCOUnitedStatesCA));
			Assert("collectionNotESCO should contain UNLOCOUnitedStatesAZ", collectionNotESCO.Contains(uNLOCOUnitedStatesAZ));

			RefUNLOCOFilterBusinessObject stateFilterUSCABizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterUSCA = ((RefUNLOCOCountryStateModuleFilter)stateFilterUSCABizO["CountryState"]);
			stateFilterUSCA.ComparisonOperator = notEquals;
			stateFilterUSCA.Property1 = "US";
			stateFilterUSCA.Property2 = stateUSCAGuid;
			stateFilterUSCA.IsActive = true;
			stateFilterUSCA.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterUSCA.Property1Info.MaxLength);
			AssertNoErrors(stateFilterUSCA.Property1Info);
			AssertNoErrors(stateFilterUSCA.Property2Info);

			RefUNLOCOCollection collectionNotUSCA = new RefUNLOCOCollection(Factory, stateFilterUSCABizO.Filter);
			Assert("collectionNotUSCA should contain UNLOCOSpainCA", collectionNotUSCA.Contains(uNLOCOSpainCA));
			Assert("collectiocollectionNotUSCAnUSCA should contain UNLOCOSpainCO", collectionNotUSCA.Contains(uNLOCOSpainCO));
			Assert("collectionNotUSCA should NOT contain UNLOCOUnitedStatesCA", !collectionNotUSCA.Contains(uNLOCOUnitedStatesCA));
			Assert("collectionNotUSCA should contain UNLOCOUnitedStatesAZ", collectionNotUSCA.Contains(uNLOCOUnitedStatesAZ));

			RefUNLOCOFilterBusinessObject stateFilterBizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilter = ((RefUNLOCOCountryStateModuleFilter)stateFilterBizO["CountryState"]);
			stateFilter.ComparisonOperator = notEquals;
			stateFilter.Property1 = "";
			stateFilter.Property2 = new ZGuid();
			stateFilter.IsActive = true;
			stateFilter.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilter.Property1Info.MaxLength);
			AssertNoErrors(stateFilter.Property1Info);
			AssertNoErrors(stateFilter.Property2Info);

			RefUNLOCOCollection collectionNot = new RefUNLOCOCollection(Factory, stateFilterBizO.Filter);
			Assert("collectionNot should contain UNLOCOSpainCA", collectionNot.Contains(uNLOCOSpainCA));
			Assert("collectionNot should contain UNLOCOSpainCO", collectionNot.Contains(uNLOCOSpainCO));
			Assert("collectionNot should contain UNLOCOUnitedStatesCA", collectionNot.Contains(uNLOCOUnitedStatesCA));
			Assert("collectionNot should contain UNLOCOUnitedStatesAZ", collectionNot.Contains(uNLOCOUnitedStatesAZ));

			RefUNLOCOFilterBusinessObject stateFilterXXBizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterXX = ((RefUNLOCOCountryStateModuleFilter)stateFilterXXBizO["CountryState"]);
			stateFilterXX.ComparisonOperator = notEquals;
			stateFilterXX.Property1 = "XX";
			stateFilterXX.Property2 = new ZGuid();
			stateFilterXX.IsActive = true;
			stateFilterXX.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterXX.Property1Info.MaxLength);
			AssertHasError(stateFilterXX.Property1Info, "Enter a valid selection.");
			AssertNoErrors(stateFilterXX.Property2Info);

			RefUNLOCOFilterBusinessObject stateFilterCABizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterCA = ((RefUNLOCOCountryStateModuleFilter)stateFilterCABizO["CountryState"]);
			stateFilterCA.ComparisonOperator = notEquals;
			stateFilterCA.Property1 = "";
			stateFilterCA.Property2 = stateUSCAGuid;
			stateFilterCA.IsActive = true;
			stateFilterCA.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterCA.Property1Info.MaxLength);
			AssertNoErrors(stateFilterCA.Property1Info);
			AssertHasError(stateFilterCA.Property2Info, "Enter a country/region first.");

			RefUNLOCOFilterBusinessObject stateFilterUSBizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterUS = ((RefUNLOCOCountryStateModuleFilter)stateFilterUSBizO["CountryState"]);
			stateFilterUS.ComparisonOperator = notEquals;
			stateFilterUS.Property1 = "US";
			stateFilterUS.Property2 = new ZGuid();
			stateFilterUS.IsActive = true;
			stateFilterUS.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterUS.Property1Info.MaxLength);
			AssertNoErrors(stateFilterUS.Property1Info);
			AssertNoErrors(stateFilterUS.Property2Info);

			RefUNLOCOCollection collectionNotUS = new RefUNLOCOCollection(Factory, stateFilterUSBizO.Filter);
			Assert("collectionNotUS should contain UNLOCOSpainCA", collectionNotUS.Contains(uNLOCOSpainCA));
			Assert("collectionNotUS should contain UNLOCOSpainCO", collectionNotUS.Contains(uNLOCOSpainCO));
			Assert("collectionNotUS should NOT contain UNLOCOUnitedStatesCA", !collectionNotUS.Contains(uNLOCOUnitedStatesCA));
			Assert("collectionNotUS should NOT contain UNLOCOUnitedStatesAZ", !collectionNotUS.Contains(uNLOCOUnitedStatesAZ));

			RefUNLOCOFilterBusinessObject stateFilterUSXXBizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterUSXX = ((RefUNLOCOCountryStateModuleFilter)stateFilterUSXXBizO["CountryState"]);
			stateFilterUSXX.ComparisonOperator = notEquals;
			stateFilterUSXX.Property1 = "US";
			stateFilterUSXX.Property2 = new ZGuid("3524DA6A-C1CF-4E3F-831E-2F7FEC0DF365"); // Invalid state code
			stateFilterUSXX.IsActive = true;
			stateFilterUSXX.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterUSXX.Property1Info.MaxLength);
			AssertNoErrors(stateFilterUSXX.Property1Info);
			AssertHasError(stateFilterUSXX.Property2Info, "Enter a valid selection.");

			RefUNLOCOFilterBusinessObject stateFilterUS01BizO = new RefUNLOCOFilterBusinessObject();
			RefUNLOCOCountryStateModuleFilter stateFilterUS01 = ((RefUNLOCOCountryStateModuleFilter)stateFilterUS01BizO["CountryState"]);
			stateFilterUS01.ComparisonOperator = notEquals;
			stateFilterUS01.Property1 = "US";
			stateFilterUS01.Property2 = new ZGuid("657bf2a5-b6f9-46d5-9ab4-5bbd00e360ea"); // Valid state code, but not for US country
			stateFilterUS01.IsActive = true;
			stateFilterUS01.Validation.ValidateAll();

			AssertEquals(RefCountry.Schema.RN_CodeMaxLength, stateFilterUS01.Property1Info.MaxLength);
			AssertNoErrors(stateFilterUS01.Property1Info);
			AssertHasError(stateFilterUS01.Property2Info, "State is not in country/region 'US'");
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefUNLOCOFilterBusinessObject();
		}

		#endregion

		public void TestIdentifiersFilter_DefaultSettings()
		{
			var stripBO = (RefUNLOCOFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (UNLCOIdentifierModuleFilter)stripBO["UNLOCO Identifiers"];
			AssertEquals("This should be true so the radio box shows: ", true, filter.ShowAddOrRadioBox);
			AssertEquals("AND join condition should default to false: ", false, filter.AndJoinCondition);
			AssertEquals("OR join condition should default to true: ", true, filter.OrJoinCondition);
		}

		public void TestServicingPostCodeFilterMaxLength()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var servicingPostalCodeFilter = (ModuleTextFilter)GetNewFilterStripBusinessObject()["Servicing Postal Code"];
			AssertEquals(10, servicingPostalCodeFilter.MaxLength);
		}

		public void TestSeaportAndAirportFiltersWork()
		{
			var nZCompany = Factory.New<GlbCompany>();
			nZCompany.GC_Code = "NZC";
			nZCompany.GC_RN_NKCountryCode = "NZ";
			var aucklandBranch = nZCompany.Branches.AddNew();
			aucklandBranch.GB_RL_NKHomePort = "NZAKL";
			aucklandBranch.GB_Code = "NZC";
			Factory.Save();

			using (DisposableEnvironment.ForBranch(aucklandBranch.PK.ToGuid()))
			{
				var stripBO = (RefUNLOCOFilterBusinessObject)GetNewFilterStripBusinessObject();
				var codeFilter = (ModuleTextFilter)stripBO["Code"];
				codeFilter.Property = "NZA";
				codeFilter.IsActive = true;
				codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains;
				var hasAirport = (ModuleFlagsFilter)stripBO["Has Airport"];
				var hasSeaport = (ModuleFlagsFilter)stripBO["Has Seaport"];

				AssertNotNull(hasAirport);
				AssertNotNull(hasSeaport);

				hasAirport.IsActive = true;
				hasSeaport.IsActive = true;
				hasAirport.Property0 = true;
				hasSeaport.Property0 = false;
				var collection = new RefUNLOCOCollection(Factory, stripBO.Filter);

				foreach (RefUNLOCO unloco in collection)
				{
					AssertEquals(String.Format("{0} SHOULD have an airport", unloco.RL_Code), true, unloco.RL_HasAirport);
					AssertEquals(String.Format("{0} should NOT have a seaport", unloco.RL_Code), false, unloco.RL_HasSeaport);
				}

				hasAirport.Property0 = false;
				hasSeaport.Property0 = true;
				collection = new RefUNLOCOCollection(Factory, stripBO.Filter);

				foreach (RefUNLOCO unloco in collection)
				{
					AssertEquals(String.Format("{0} SHOULD have a seaport", unloco.RL_Code), true, unloco.RL_HasSeaport);
					AssertEquals(String.Format("{0} should NOT have an airport", unloco.RL_Code), false, unloco.RL_HasAirport);
				}

				hasAirport.Property0 = true;
				hasSeaport.Property0 = true;
				collection = new RefUNLOCOCollection(Factory, stripBO.Filter);

				foreach (RefUNLOCO unloco in collection)
				{
					AssertEquals(String.Format("{0} SHOULD have BOTH an airport and seaport", unloco.RL_Code), true, unloco.RL_HasAirport);
					AssertEquals(String.Format("{0} SHOULD have BOTH an airport and seaport", unloco.RL_Code), true, unloco.RL_HasSeaport);
				}

				hasAirport.Property0 = false;
				hasSeaport.Property0 = false;
				collection = new RefUNLOCOCollection(Factory, stripBO.Filter);

				foreach (RefUNLOCO unloco in collection)
				{
					AssertEquals(String.Format("{0} should NOT have an airport OR a seaport", unloco.RL_Code), false, unloco.RL_HasAirport);
					AssertEquals(String.Format("{0} should NOT have an airport OR a seaport", unloco.RL_Code), false, unloco.RL_HasSeaport);
				}
			}
		}

		public void TestGetModuleFilters()
		{
			var lvCompany = Factory.New<GlbCompany>();
			lvCompany.GC_Code = "DAN";
			lvCompany.GC_RN_NKCountryCode = "LV";
			var rigaBranch = lvCompany.Branches.AddNew();
			rigaBranch.GB_RL_NKHomePort = "LVRIX";
			rigaBranch.GB_Code = "DAN";
			Factory.Save();
			using (DisposableEnvironment.ForBranch(rigaBranch.PK.ToGuid()))
			{
				var stripBO = (RefUNLOCOFilterBusinessObject)GetNewFilterStripBusinessObject();
				var codeFilter = (ModuleTextFilter)stripBO["Code"];
				codeFilter.Property = "RIX";
				codeFilter.IsActive = true;
				codeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains;
				var isInLatvia = (ModuleFlagsFilter)stripBO["Is in Latvia?"];
				var economicGroup = (ModuleTextFilter)stripBO["Economic Group"];

				AssertNotNull(isInLatvia);
				AssertNotNull(economicGroup);

				isInLatvia.Property0 = false;
				isInLatvia.IsActive = false;
				var collection = new RefUNLOCOCollection(Factory, stripBO.Filter);
				AssertEquals("All 8 *RIX* codes, including USRIX", 8, collection.Count);

				isInLatvia.Property0 = false;
				isInLatvia.IsActive = true;
				collection = new RefUNLOCOCollection(Factory, stripBO.Filter);
				AssertEquals("*RIX* codes, exclude those in Latvia", 7, collection.Count);
				AssertEquals(0, collection.Find(c => c.RL_Code == "LVRIX").Count());

				isInLatvia.Property0 = true;
				isInLatvia.IsActive = true;
				collection = new RefUNLOCOCollection(Factory, stripBO.Filter);
				AssertEquals("*RIX* codes, only those in Latvia", 1, collection.Count);
				AssertEquals("LVRIX", collection[0].RL_Code);

				isInLatvia.IsActive = false;
				economicGroup.IsActive = true;
				economicGroup.Property = EconomicGroupList.Codes.EuropeanUnion;
				collection = new RefUNLOCOCollection(Factory, stripBO.Filter);
				AssertEquals("*RIX* codes, only those in EU (excludes USRIX)", 7, collection.Count);
				AssertEquals(0, collection.Find(c => c.RL_Code == "USRIX").Count());

				isInLatvia.IsActive = true;
				isInLatvia.Property0 = false;
				economicGroup.IsActive = true;
				economicGroup.Property = EconomicGroupList.Codes.EuropeanUnion;
				collection = new RefUNLOCOCollection(Factory, stripBO.Filter);
				AssertEquals("*RIX* codes, exclude those in Latvia, only those in EU", 6, collection.Count);
				AssertEquals(0, collection.Find(c => c.RL_Code == "LVRIX").Count());
				AssertEquals(0, collection.Find(c => c.RL_Code == "USRIX").Count());

				economicGroup.IsActive = true;
				economicGroup.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
				economicGroup.Property = EconomicGroupList.Codes.EuropeanUnion;
				collection = new RefUNLOCOCollection(Factory, stripBO.Filter);
				AssertEquals("*RIX* codes, exclude those in EU", 1, collection.Count);
				AssertEquals("USRIX", collection[0].RL_Code);

				economicGroup.IsActive = false;
				isInLatvia.IsActive = false;
				economicGroup.IsActive = true;
				economicGroup.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
				economicGroup.Property = EconomicGroupList.Codes.EuropeanUnion;
				collection = new RefUNLOCOCollection(Factory, stripBO.Filter);
				AssertEquals("*RIX* codes, only those in EU", 7, collection.Count);
				AssertEquals(0, collection.Find(c => c.RL_Code == "USRIX").Count());

				economicGroup.Property = EconomicGroupList.Codes.NAFTA;
				collection = new RefUNLOCOCollection(Factory, stripBO.Filter);
				AssertEquals("*RIX* codes, only those in NAFTA", 1, collection.Count);
				AssertEquals("USRIX", collection[0].RL_Code);
			}
		}

		public void TestProperNameFilter()
		{
			var unloco1 = CreateRefUNLOCO("Blüthen");
			var unloco2 = CreateRefUNLOCO("Alt Krüssow");

			var filter = new RefUNLOCOFilterBusinessObject();
			var properNameFilter = (ModuleTextFilter)filter["Proper Name"];
			properNameFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Proper Name", properNameFilter.MultilingualDescription.ToString());
				AssertEquals("Category", FilterCategories.TextSearch, properNameFilter.Category);
				AssertEquals("MaxLength", RefUNLOCOSchema.RL_NameWithDiacriticals.MaxLength, properNameFilter.MaxLength);

				properNameFilter.Property = "Blüthen";
				AssertEquals("Filter by 'Blüthen', RL_NameWithDiacriticals 'Blüthen'", true, unloco1.MatchesFilter(filter.Filter));
				AssertEquals("Filter by 'Blüthen', RL_NameWithDiacriticals 'Alt Krüssow'", false, unloco2.MatchesFilter(filter.Filter));

				properNameFilter.Property = "Alt Krüssow";
				AssertEquals("Filter by 'Alt Krüssow', RL_NameWithDiacriticals 'Blüthen'", false, unloco1.MatchesFilter(filter.Filter));
				AssertEquals("Filter by 'Alt Krüssow', RL_NameWithDiacriticals 'Alt Krüssow'", true, unloco2.MatchesFilter(filter.Filter));
			});

			RefUNLOCO CreateRefUNLOCO(ZString properName)
			{
				var unloco = Factory.New<RefUNLOCO>();
				unloco.RL_NameWithDiacriticals = properName;
				return unloco;
			}
		}
	}
}
