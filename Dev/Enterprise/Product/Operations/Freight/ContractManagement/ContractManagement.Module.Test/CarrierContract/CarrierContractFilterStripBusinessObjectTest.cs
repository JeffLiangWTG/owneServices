using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Module.Testing
{
	[TestedType(typeof(CarrierContractFilterStripBusinessObject))]
	class CarrierContractFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CarrierContractFilterStripBusinessObject();
		}

		#region Filters

		public void TestContractNumberFilter()
		{
			var contract1 = Factory.NewWithValidTestData<RatingContract>();
			var contract2 = Factory.NewWithValidTestData<RatingContract>();
			var contract3 = Factory.NewWithValidTestData<RatingContract>();
			var contract4 = Factory.NewWithValidTestData<RatingContract>();

			contract1.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract2.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract3.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract4.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;

			contract1.RCT_ContractNumber = "1234";
			contract2.RCT_ContractNumber = "2345";
			contract3.RCT_ContractNumber = "3456";
			contract4.RCT_ContractNumber = "4567";

			Factory.Save();

			var filterStrip = new CarrierContractFilterStripBusinessObject();
			var contractNumberFilter = (ModuleTextFilter)filterStrip["Contract #"];
			contractNumberFilter.Property = "12";
			contractNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			contractNumberFilter.IsActive = true;

			var collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("1 Contract Loaded", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract1 }, collection);

			contractNumberFilter.Property = "23";
			contractNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;

			collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("2 Contracts Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract1, contract2 }, collection);
		}

		public void TestContractNumberFilterExclusivity()
		{
			var contract1 = Factory.NewWithValidTestData<RatingContract>();
			var contract2 = Factory.NewWithValidTestData<RatingContract>();

			contract1.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract2.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;

			contract1.RCT_ContractNumber = "1234";
			contract1.RCT_TransportMode = "SEA";
			contract2.RCT_ContractNumber = "2345";
			contract2.RCT_TransportMode = "AIR";

			Factory.Save();

			var filterStrip = new CarrierContractFilterStripBusinessObject();

			var transportModeFilter = (ModuleTextFilter)filterStrip["Transport Mode"];
			transportModeFilter.Property = "AIR";
			transportModeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			transportModeFilter.IsActive = true;

			var contractNumberFilter = (ModuleTextFilter)filterStrip["Contract #"];
			contractNumberFilter.Property = "12";
			contractNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			contractNumberFilter.IsActive = true;

			var collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("1 Contract Loaded", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract1 }, collection);
		}

		public void TestTransportModeFilter()
		{
			var contract1 = Factory.NewWithValidTestData<RatingContract>();
			var contract2 = Factory.NewWithValidTestData<RatingContract>();
			var contract3 = Factory.NewWithValidTestData<RatingContract>();
			var contract4 = Factory.NewWithValidTestData<RatingContract>();

			contract1.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract2.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract3.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract4.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;

			contract1.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			contract2.RCT_TransportMode = Core.Constants.TransportModes.Air;
			contract3.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			contract4.RCT_TransportMode = Core.Constants.TransportModes.Air;

			Factory.Save();

			var filterStrip = new CarrierContractFilterStripBusinessObject();
			var transportModeFilter = (ModuleTextFilter)filterStrip["Transport Mode"];
			transportModeFilter.Property = "SEA";
			transportModeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			transportModeFilter.IsActive = true;

			var collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("2 Contracts Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract1, contract3 }, collection);

			transportModeFilter.Property = "AIR";
			transportModeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("2 Contracts Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract2, contract4 }, collection);
		}

		public void TestStartDateFilter()
		{
			var contract1 = Factory.NewWithValidTestData<RatingContract>();
			var contract2 = Factory.NewWithValidTestData<RatingContract>();
			var contract3 = Factory.NewWithValidTestData<RatingContract>();
			var contract4 = Factory.NewWithValidTestData<RatingContract>();

			contract1.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract2.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract3.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract4.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;

			contract1.RCT_StartDate = new ZDate(2021, 1, 27);
			contract2.RCT_StartDate = new ZDate(2021, 4, 27);
			contract3.RCT_StartDate = new ZDate(2021, 6, 27);
			contract4.RCT_StartDate = new ZDate(2021, 10, 27);

			contract1.RCT_EndDate = new ZDate(2021, 2, 27);
			contract2.RCT_EndDate = new ZDate(2021, 5, 27);
			contract3.RCT_EndDate = new ZDate(2021, 7, 27);
			contract4.RCT_EndDate = new ZDate(2021, 11, 27);

			Factory.Save();

			var filterStrip = new CarrierContractFilterStripBusinessObject();
			var startDateFilter = (ModuleDateFilter)filterStrip["Start Date"];
			startDateFilter.Property1 = new ZDateTime(2021, 2, 27);
			startDateFilter.Property2 = new ZDateTime(2021, 7, 27);
			startDateFilter.IsActive = true;
			startDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			var collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("2 Contracts Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract2, contract3 }, collection);

			startDateFilter.Property1 = new ZDateTime(2021, 1, 10);
			startDateFilter.Property2 = new ZDateTime(2021, 11, 27);

			collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("4 Contracts Loaded", 4, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract1, contract2, contract3, contract4 }, collection);
		}

		public void TestExpiryDateFilter()
		{
			var contract1 = Factory.NewWithValidTestData<RatingContract>();
			var contract2 = Factory.NewWithValidTestData<RatingContract>();
			var contract3 = Factory.NewWithValidTestData<RatingContract>();
			var contract4 = Factory.NewWithValidTestData<RatingContract>();

			contract1.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract2.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract3.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract4.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;

			contract1.RCT_StartDate = new ZDate(2021, 1, 15);
			contract2.RCT_StartDate = new ZDate(2021, 4, 15);
			contract3.RCT_StartDate = new ZDate(2021, 6, 15);
			contract4.RCT_StartDate = new ZDate(2021, 10, 15);

			contract1.RCT_EndDate = new ZDate(2021, 1, 27);
			contract2.RCT_EndDate = new ZDate(2021, 4, 27);
			contract3.RCT_EndDate = new ZDate(2021, 6, 27);
			contract4.RCT_EndDate = new ZDate(2021, 10, 27);

			Factory.Save();

			var filterStrip = new CarrierContractFilterStripBusinessObject();
			var expiryDateFilter = (ModuleDateFilter)filterStrip["Expiry Date"];
			expiryDateFilter.Property1 = new ZDateTime(2021, 2, 27);
			expiryDateFilter.Property2 = new ZDateTime(2021, 7, 27);
			expiryDateFilter.IsActive = true;
			expiryDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			var collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("2 Contracts Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract2, contract3 }, collection);

			expiryDateFilter.Property1 = new ZDateTime(2021, 1, 10);
			expiryDateFilter.Property2 = new ZDateTime(2021, 11, 27);

			collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("4 Contracts Loaded", 4, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract1, contract2, contract3, contract4 }, collection);
		}

		public void TestDescriptionFilter()
		{
			var contract1 = Factory.NewWithValidTestData<RatingContract>();
			var contract2 = Factory.NewWithValidTestData<RatingContract>();
			var contract3 = Factory.NewWithValidTestData<RatingContract>();
			var contract4 = Factory.NewWithValidTestData<RatingContract>();

			contract1.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract2.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract3.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract4.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;

			contract1.RCT_Description = "A description";
			contract2.RCT_Description = "Another description";
			contract3.RCT_Description = "A different description";
			contract4.RCT_Description = "Bad description";

			Factory.Save();

			var filterStrip = new CarrierContractFilterStripBusinessObject();
			var descriptionFilter = (ModuleTextFilter)filterStrip["Description"];
			descriptionFilter.Property = "An";
			descriptionFilter.IsActive = true;
			descriptionFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

			var collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("1 Contracts Loaded", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract2 }, collection);

			descriptionFilter.Property = "A";
			descriptionFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

			collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("3 Contracts Loaded", 3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract1, contract2, contract3 }, collection);
		}

		public void TestServiceProviderFilter()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

			var contract1 = Factory.NewWithValidTestData<RatingContract>();
			var contract2 = Factory.NewWithValidTestData<RatingContract>();
			var contract3 = Factory.NewWithValidTestData<RatingContract>();
			var contract4 = Factory.NewWithValidTestData<RatingContract>();

			contract1.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract2.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract3.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract4.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;

			contract1.RCT_OH = orgHeader1.PK;
			contract2.RCT_OH = orgHeader1.PK;
			contract3.RCT_OH = orgHeader2.PK;
			contract4.RCT_OH = orgHeader2.PK;

			Factory.Save();

			var filterStrip = new CarrierContractFilterStripBusinessObject();
			var serviceProviderFilter = (ModuleGuidFilterForOrg)filterStrip["Service Provider"];
			serviceProviderFilter.Property = orgHeader1.PK;
			serviceProviderFilter.IsActive = true;
			serviceProviderFilter.ComparisonOperator = ModuleGuidFilterForOrg.ComparisonConstants.Exact;

			var collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("2 Contracts Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract1, contract2 }, collection);

			serviceProviderFilter.Property = orgHeader2.PK;
			serviceProviderFilter.ComparisonOperator = ModuleGuidFilterForOrg.ComparisonConstants.Exact;

			collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("2 Contracts Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract3, contract4 }, collection);
		}

		public void TestNamedAccountsFilter()
		{
			var filterStrip = new CarrierContractFilterStripBusinessObject();
			var namedAccountsFilter = (NamedAccountsOfContractFilter)filterStrip[CarrierContractFilterConstants.NamedAccountClients];
			namedAccountsFilter.IsActive = true;

			var orgFilter = namedAccountsFilter.SelectedFilters;
			var orgCodeFilter = (ModuleTextFilter)orgFilter["Code"];
			orgCodeFilter.Property = "AAA";
			orgCodeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			orgCodeFilter.IsActive = true;

			namedAccountsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			AssertNotNull(LoadContractCollection(filterStrip));

			namedAccountsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			AssertNotNull(LoadContractCollection(filterStrip));
		}

		public void TestContainerTypeFilter()
		{
			var contract1 = Factory.NewWithValidTestData<RatingContract>();
			var contract2 = Factory.NewWithValidTestData<RatingContract>();
			var contract3 = Factory.NewWithValidTestData<RatingContract>();
			var contract4 = Factory.NewWithValidTestData<RatingContract>();

			contract1.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract2.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract3.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract4.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;

			contract1.RCT_ContainerType = "DRY";
			contract2.RCT_ContainerType = "TNK";
			contract3.RCT_ContainerType = "TOP";
			contract4.RCT_ContainerType = "OTH";

			Factory.Save();

			var filterStrip = new CarrierContractFilterStripBusinessObject();
			var containerTypeFilter = (ModuleTextFilter)filterStrip["Container Type"];
			containerTypeFilter.Property = "DR";
			containerTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			containerTypeFilter.IsActive = true;

			var collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("1 Contract Loaded", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract1 }, collection);

			containerTypeFilter.Property = "O";
			containerTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;

			collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("2 Contracts Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract3, contract4 }, collection);
		}

		public void TestContractOwnerFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "USR";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ABC";

			var contract1 = Factory.NewWithValidTestData<RatingContract>();
			var contract2 = Factory.NewWithValidTestData<RatingContract>();
			var contract3 = Factory.NewWithValidTestData<RatingContract>();
			var contract4 = Factory.NewWithValidTestData<RatingContract>();

			contract1.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract2.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract3.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract4.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;

			contract1.RCT_GS_NKContractOwner = staff1.GS_Code;
			contract2.RCT_GS_NKContractOwner = staff1.GS_Code;
			contract3.RCT_GS_NKContractOwner = staff2.GS_Code;
			contract4.RCT_GS_NKContractOwner = staff2.GS_Code;

			Factory.Save();

			var filterStrip = new CarrierContractFilterStripBusinessObject();
			var contractOwnerFilter = (ModuleNkFilter)filterStrip["Contract Owner"];

			contractOwnerFilter.Property = staff1.GS_Code;
			contractOwnerFilter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.Exact;
			contractOwnerFilter.IsActive = true;

			var collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;
			AssertEquals("2 Contract Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract1, contract2 }, collection);

			contractOwnerFilter.Property = ZString.Empty;
			contractOwnerFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			contractOwnerFilter.IsActive = true;

			collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;
			AssertEquals("4 Contracts Loaded", 4, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract1, contract2, contract3, contract4 }, collection);

			contractOwnerFilter.Property = ZString.Empty;
			contractOwnerFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			contractOwnerFilter.IsActive = true;

			collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;
			AssertEquals("0 Contracts Loaded", 0, collection.Count);
		}

		public void TestHasAllocationRoutesFilter()
		{
			var route = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			var contract1 = route.Contract;
			var contract2 = Factory.NewWithValidTestData<RatingContract>();
			var contract3 = Factory.NewWithValidTestData<RatingContract>();

			contract1.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract2.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract3.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;

			contract1.RCT_ContractNumber = "1234";
			contract2.RCT_ContractNumber = "2345";
			contract3.RCT_ContractNumber = "3456";

			Factory.Save();

			var filterStrip = new CarrierContractFilterStripBusinessObject();
			var hasAllocationRoutesFilter = (ModuleFlagsFilter)filterStrip[CarrierContractFilterConstants.HasAllocationRoutes];
			hasAllocationRoutesFilter.Property0 = false;
			hasAllocationRoutesFilter.IsActive = true;

			var collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("2 Contracts Loaded", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract2, contract3 }, collection);

			hasAllocationRoutesFilter.Property0 = true;

			collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("1 Contracts Loaded", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract1 }, collection);
		}

		public void TestDefaultFiltersAreRemovable()
		{
			var contract = Factory.NewWithValidTestData<RatingContract>();

			Factory.Save();
			var contractDefaults = new FilterBusinessObjectDefaults
			{
				new FilterBusinessObjectDefault(CarrierContractFilterConstants.ContractNumber, "Property", contract.RCT_ContractNumber, true),
				new FilterBusinessObjectDefault(CarrierContractFilterConstants.TransportMode, "Property", contract.RCT_TransportMode, true)
			};
			var filterStripStrip = new CarrierContractFilterStripBusinessObject();
			filterStripStrip.SetExternalDefaults(contractDefaults);
			foreach (var moduleFilter in filterStripStrip.AlwaysVisibleModuleFilters)
			{
				AssertEquals(FilterVisibility.Visible, moduleFilter.Visibility);
			}
		}

		RatingContract[] LoadContractCollection(CarrierContractFilterStripBusinessObject filterStrip)
		{
			var collection = new CarrierContractCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;
			return collection.ToArray();
		}

		#endregion
	}
}
