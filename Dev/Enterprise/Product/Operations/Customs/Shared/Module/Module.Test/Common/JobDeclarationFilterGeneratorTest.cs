using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module.Testing
{
	sealed class JobDeclarationFilterGeneratorTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestFlightVoyageVesselFilter()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_TransportMode = "SEA";
			declaration1.JE_VoyageFlightNo = "12345";
			declaration1.JE_VesselName = "The Spitfire";
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_TransportMode = "SEA";
			declaration2.JE_VoyageFlightNo = "6789";
			declaration2.JE_VesselName = "Flying Dutchman";
			Factory.Save();
			var filterObj = new FilterStripBusinessObjectForTest();
			var generator = new JobDeclarationFilterGenerator(filterObj);
			filterObj.ModuleFiltersExposed = new ModuleFilterCollection();
			var voyageAndVesselFilter = generator.AddFlightVoyageVesselFilter(filterObj.ModuleFiltersExposed, "FlightVoyageVessel");
			voyageAndVesselFilter.NkProperty = "The Spitfire";
			voyageAndVesselFilter.IsActive = true;
			AssertEquals(true, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.Property = "XXX";
			AssertEquals(false, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.Property = "12345";
			AssertEquals(true, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.NkProperty = "";
			AssertEquals(true, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.Property = "";
			AssertEquals(true, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.Property = "2";
			voyageAndVesselFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			AssertEquals(true, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.Property = "";
			voyageAndVesselFilter.NkProperty = "ch";
			AssertEquals(false, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.Property = "";
			voyageAndVesselFilter.NkProperty = "";
			voyageAndVesselFilter.IsActive = true;
			voyageAndVesselFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertEquals(true, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterObj.Filter));
			voyageAndVesselFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertEquals(false, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
			AssertEquals(JobDeclarationSchema.JE_VoyageFlightNo.MaxLength, voyageAndVesselFilter.MaxLength);
			AssertEquals(JobDeclarationSchema.JE_VesselName.MaxLength, voyageAndVesselFilter.NkMaxLength);
		}

		public void TestOriginDestinationfilter()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_RL_NKOrigin = "AUSYD";
			declaration1.JE_RL_NKFinalDestination = "NZAKL";
			declaration2.JE_RL_NKOrigin = "AUMEL";
			declaration2.JE_RL_NKFinalDestination = "USCHI";
			Factory.Save();
			var filterObj = new FilterStripBusinessObjectForTest();
			var generator = new JobDeclarationFilterGenerator(filterObj);
			filterObj.ModuleFiltersExposed = new ModuleFilterCollection();
			var originDestinationFilter = generator.AddOriginDestinationFilter(filterObj.ModuleFiltersExposed, "OriginDestination");
			originDestinationFilter.IsActive = true;
			originDestinationFilter.Property1 = "AUSYD";
			AssertEquals(true, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
			originDestinationFilter.Property2 = "NZAKL";
			AssertEquals(true, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
			originDestinationFilter.Property1 = "AUSYD";
			originDestinationFilter.Property2 = "USCHI";
			AssertEquals(false, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
			originDestinationFilter.Property1 = "AUMEL";
			originDestinationFilter.Property2 = "USCHI";
			AssertEquals(false, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterObj.Filter));
			originDestinationFilter.Property1 = "";
			originDestinationFilter.Property2 = "USCHI";
			AssertEquals(false, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterObj.Filter));
			originDestinationFilter.Property1 = "";
			originDestinationFilter.Property2 = "";
			AssertEquals(true, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterObj.Filter));
		}

		public void TestLoadDischargeFilter()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_RL_NKPortOfLoading = "AUSYD";
			declaration1.JE_RL_NKPortOfArrival = "NZAKL";
			declaration2.JE_RL_NKPortOfLoading = "AUMEL";
			declaration2.JE_RL_NKPortOfArrival = "USCHI";
			Factory.Save();
			var filterObj = new FilterStripBusinessObjectForTest();
			var generator = new JobDeclarationFilterGenerator(filterObj);
			filterObj.ModuleFiltersExposed = new ModuleFilterCollection();
			var filter = generator.AddLoadDischargeFilter(filterObj.ModuleFiltersExposed, "LoadDischarge");
			filter.IsActive = true;
			filter.Property1 = "AUSYD";
			AssertEquals(true, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
			filter.Property2 = "NZAKL";
			AssertEquals(true, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
			filter.Property1 = "AUSYD";
			filter.Property2 = "USCHI";
			AssertEquals(false, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
			filter.Property1 = "AUMEL";
			filter.Property2 = "USCHI";
			AssertEquals(false, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterObj.Filter));
			filter.Property1 = "";
			filter.Property2 = "USCHI";
			AssertEquals(false, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterObj.Filter));
			filter.Property1 = "";
			filter.Property2 = "";
			AssertEquals(true, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterObj.Filter));
		}

		public void TestAgentReferenceFilters()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_AgentsReference = "Test 1";
			declaration2.JE_AgentsReference = "Test 2";
			Factory.Save();
			var filterObj = new FilterStripBusinessObjectForTest();
			var generator = new JobDeclarationFilterGenerator(filterObj);
			filterObj.ModuleFiltersExposed = new ModuleFilterCollection();
			var filter = generator.AddAgentsReferenceFilter(filterObj.ModuleFiltersExposed, "AgentsReference");
			filter.IsActive = true;
			filter.Property = "Test 1";
			AssertEquals(true, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
			filter.Property = "Test 2";
			AssertEquals(false, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, declaration2.MatchesFilter(filterObj.Filter));
			filter.Property = "XXX";
			AssertEquals(false, declaration1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, declaration2.MatchesFilter(filterObj.Filter));
		}

		public void TestAdditionalReferenceNumberFilter()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.AdditionalReferenceNumbers.AddNew();
			entry.CE_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			entry.CE_EntryType = "ABC";
			entry.CE_EntryNum = "963258";
			Factory.Save();
			var filterObj = new FilterStripBusinessObjectForTest();
			var generator = new JobDeclarationFilterGenerator(filterObj);
			filterObj.ModuleFiltersExposed = new ModuleFilterCollection();
			var filter = generator.AddAdditionalReferenceNumberFilter(filterObj.ModuleFiltersExposed, Factory);
			filter.IsActive = true;
			CombineAssertions(() =>
			{
				AssertEquals("find for empty filter", true, declaration.MatchesFilter(filterObj.Filter));
				filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				filter.Property = "258";
				filter.Country = Core.Constants.CountryCodes.SouthAfrica;
				filter.Type = "ABC";
				AssertEquals("find when matching", true, declaration.MatchesFilter(filterObj.Filter));
				filter.Property = "123";
				AssertEquals("No result for number:123", false, declaration.MatchesFilter(filterObj.Filter));
				filter.Property = "258";
				filter.Type = "CCC";
				AssertEquals("No result for type:CCC", false, declaration.MatchesFilter(filterObj.Filter));
				filter.Type = "ABC";
				filter.Country = Core.Constants.CountryCodes.Afghanistan;
				AssertEquals("No result for country:AF", false, declaration.MatchesFilter(filterObj.Filter));
			});
		}

		sealed class FilterStripBusinessObjectForTest : FilterStripBusinessObject
		{
			protected override ModuleFilterCollection GetModuleFiltersCore() => ModuleFiltersExposed;
			public ModuleFilterCollection ModuleFiltersExposed { get; set; }
		}
	}
}
