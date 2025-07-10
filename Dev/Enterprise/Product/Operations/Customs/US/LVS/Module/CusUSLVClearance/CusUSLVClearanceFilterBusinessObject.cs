using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Module
{
	public class CusUSLVClearanceFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filter Identifiers

		public static class FilterIdentifiers
		{
			public const string JobNumber = nameof(JobNumber);
			public const string EntryNumber = nameof(EntryNumber);
			public const string MasterBill = nameof(MasterBill);
			public const string HouseBill = nameof(HouseBill);
			public const string Container = nameof(Container);
			public const string TransportMode = nameof(TransportMode);
			public const string ContainerMode = nameof(ContainerMode);
			public const string VesselFlightVoyage = nameof(VesselFlightVoyage);
			public const string LoadingPort = nameof(LoadingPort);
			public const string DischargePort = nameof(DischargePort);
			public const string EntryPort = nameof(EntryPort);
			public const string LoadDiscargeUNLOCO = nameof(LoadDiscargeUNLOCO);
			public const string Importer = nameof(Importer);
			public const string LocalClient = nameof(LocalClient);
			public const string ArrivalDate = nameof(ArrivalDate);
			public const string DischargeDate = nameof(DischargeDate);
			public const string DepartureDate = nameof(DepartureDate);
			public const string MatchingKey = nameof(MatchingKey);
		}

		#endregion

		#region Add Module Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddNumbersAndReferencesFilters(filters);
			AddModesAndTypesFilters(filters);
			AddLocationsFilters(filters);
			AddOrganisationFilters(filters);
			AddDatesFilters(filters);
			return filters;
		}

		void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			var containerFilter = filters.AddTextFilter(FilterIdentifiers.Container, GetContainerQuery);
			containerFilter.Category = FilterCategories.NumbersAndReferences;
			containerFilter.MultilingualDescription = ResString.GetMultilingualString("26efc259-cf9d-45c0-a635-5f4c90870406", "Container");
			containerFilter.MaxLength = CusUSLVConsignmentSchema.ULB_EquipmentNumber.MaxLength;

			var entryNumberFilter = filters.AddTextFilter(FilterIdentifiers.EntryNumber, GetEntryNumberQuery);
			entryNumberFilter.Category = FilterCategories.NumbersAndReferences;
			entryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("0831e8aa-75b8-4158-b467-02e65af0aec0", "Entry Number");
			entryNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;

			var houseBillFilter = filters.AddTextFilter(FilterIdentifiers.HouseBill, GetHouseBillQuery);
			houseBillFilter.Category = FilterCategories.NumbersAndReferences;
			houseBillFilter.MultilingualDescription = ResString.GetMultilingualString("aaf880f4-a58b-4834-8629-2bca9ebe00b1", "House Bill");
			containerFilter.MaxLength = CusUSLVConsignmentSchema.ULB_HouseBill.MaxLength;

			var jobNumberFilter = filters.AddTextFilter(FilterIdentifiers.JobNumber, CusUSLVClearanceSchema.ULH_JobNumber);
			jobNumberFilter.Category = FilterCategories.NumbersAndReferences;
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("cd7be860-9ef0-41a6-99fb-ab869f4062ea", "Job Number");
			jobNumberFilter.MaxLength = CusUSLVClearanceSchema.ULH_JobNumber.MaxLength;

			var masterBillFilter = filters.AddTextFilter(FilterIdentifiers.MasterBill, CusUSLVClearanceSchema.ULH_MasterBill);
			masterBillFilter.Category = FilterCategories.NumbersAndReferences;
			masterBillFilter.MultilingualDescription = ResString.GetMultilingualString("7a2880dd-4b7f-4509-8547-c1f074f3c93a", "Master Bill");
			masterBillFilter.MaxLength = CusUSLVClearanceSchema.ULH_MasterBill.MaxLength;

			var matchingKeyFilter = filters.AddTextFilter(FilterIdentifiers.MatchingKey, CusUSLVClearanceSchema.ULH_MatchingKey);
			matchingKeyFilter.Category = FilterCategories.NumbersAndReferences;
			matchingKeyFilter.MultilingualDescription = ResString.GetMultilingualString("39e55535-4ce8-4da2-be83-3b1dbc4dcb75", "Matching Key");
			matchingKeyFilter.MaxLength = CusUSLVClearanceSchema.ULH_MatchingKey.MaxLength;
		}

		void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			var containerModeFilter = filters.AddTextFilter(FilterIdentifiers.ContainerMode, CusUSLVClearanceSchema.ULH_ContainerMode, CusUSLVClearanceLookups.GetULH_ContainerModeList(Factory));
			containerModeFilter.Category = FilterCategories.ModesAndTypes;
			containerModeFilter.MultilingualDescription = ResString.GetMultilingualString("46066f06-7181-4d1c-9ccb-fcb3d008e4cc", "Container Mode (Customs)");
			containerModeFilter.MaxLength = CusUSLVClearanceSchema.ULH_ContainerMode.MaxLength;

			var transportModeFilter = filters.AddTextFilter(FilterIdentifiers.TransportMode, CusUSLVClearanceSchema.ULH_TransportMode, CusUSLVClearanceLookups.GetULH_TransportModeList(Factory));
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("c02de6d3-ed4d-4616-bfe6-44b8b789c61b", "Transport Mode");
			transportModeFilter.MaxLength = CusUSLVClearanceSchema.ULH_TransportMode.MaxLength;

			var vesselFlightVoyageFilter = filters.AddTextAndNkFilter(FilterIdentifiers.VesselFlightVoyage, CusUSLVClearanceSchema.ULH_VoyageFlightNo, ModuleIDs.RefVessel, CusUSLVClearanceSchema.ULH_ConveyanceName, RefVessels);
			vesselFlightVoyageFilter.Category = FilterCategories.ModesAndTypes;
			vesselFlightVoyageFilter.MultilingualDescription = ResString.GetMultilingualString("f83fc81e-5cdd-4fe7-9b67-a2841d4e658c", "Vessel and Flight/Voyage #");
			vesselFlightVoyageFilter.MaxLength = CusUSLVClearanceSchema.ULH_VoyageFlightNo.MaxLength;
			vesselFlightVoyageFilter.NkMaxLength = CusUSLVClearanceSchema.ULH_ConveyanceName.MaxLength;
		}

		void AddLocationsFilters(ModuleFilterCollection filters)
		{
			var dischargePortFilter = filters.AddNkFilter(FilterIdentifiers.DischargePort, CusUSLVClearanceSchema.ULH_PortOfDischarge, ModuleIDs.Customs.Universal.ZZRefCusCodeList, RegionDisctrictPorts);
			dischargePortFilter.Category = FilterCategories.Locations;
			dischargePortFilter.MultilingualDescription = ResString.GetMultilingualString("704e4e8e-1876-48af-964a-5566c4c3dcac", "Discharge Port");
			dischargePortFilter.ForeignCodeColumnOverride = ZZRefCusCodeListCombinedSchema.ZZD_Code;
			dischargePortFilter.MaxLength = CusUSLVClearanceSchema.ULH_PortOfDischarge.MaxLength;

			var entryPortFilter = filters.AddNkFilter(FilterIdentifiers.EntryPort, CusUSLVClearanceSchema.ULH_PortOfEntry, ModuleIDs.Customs.Universal.ZZRefCusCodeList, RegionDisctrictPorts);
			entryPortFilter.Category = FilterCategories.Locations;
			entryPortFilter.MultilingualDescription = ResString.GetMultilingualString("161bff6a-2cbc-4c97-b0c0-02b17f4f59f9", "Entry Port");
			entryPortFilter.ForeignCodeColumnOverride = ZZRefCusCodeListCombinedSchema.ZZD_Code;
			entryPortFilter.MaxLength = CusUSLVClearanceSchema.ULH_PortOfEntry.MaxLength;

			var loadingPortFilter = filters.AddNkFilter(FilterIdentifiers.LoadingPort, CusUSLVClearanceSchema.ULH_PortOfLoading, ModuleIDs.Customs.Universal.ZZRefCusCodeList, ForeignPorts);
			loadingPortFilter.Category = FilterCategories.Locations;
			loadingPortFilter.MultilingualDescription = ResString.GetMultilingualString("376b16ee-0faf-476f-be46-4acc94406ccc", "Loading Port");
			loadingPortFilter.ForeignCodeColumnOverride = ZZRefCusCodeListCombinedSchema.ZZD_Code;
			loadingPortFilter.MaxLength = CusUSLVClearanceSchema.ULH_PortOfLoading.MaxLength;

			var loadDiscargeUNLOCOFilter = filters.AddLocationFilter(FilterIdentifiers.LoadDiscargeUNLOCO, CusUSLVClearanceSchema.ULH_RL_NKPortOfLoading, Locations, CusUSLVClearanceSchema.ULH_RL_NKPortOfDischarge, Locations);
			loadDiscargeUNLOCOFilter.Category = FilterCategories.Locations;
			loadDiscargeUNLOCOFilter.MultilingualDescription = ResString.GetMultilingualString("fc8e023d-69f3-4482-9929-04477f0ec73d", "Load/Discharge");
			loadDiscargeUNLOCOFilter.MaxLength = CusUSLVClearanceSchema.ULH_RL_NKPortOfLoading.MaxLength;
			loadDiscargeUNLOCOFilter.SetItemDescriptions(Res.GetData("1903af96-096a-44a3-bf1a-f1781d1ea48f", "Load"), Res.GetData("f11b4e2b-07a4-44b8-a5e3-c50fdf58a67d", "Discharge"));
		}

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var importerFilter = filters.AddGuidFilter(FilterIdentifiers.Importer, ModuleIDs.Organisation, CusUSLVClearanceSchema.ULH_OH_Importer, Organisations);
			importerFilter.Category = FilterCategories.Organisations;
			importerFilter.MultilingualDescription = ResString.GetMultilingualString("888648cd-cde7-4d6b-901f-a46284a3e87a", "Importer of Record");

			var localClientFilter = filters.AddGuidFilter(FilterIdentifiers.LocalClient, ModuleIDs.Organisation, CusUSLVClearanceSchema.ULH_OH_Client, Organisations);
			localClientFilter.Category = FilterCategories.Organisations;
			localClientFilter.MultilingualDescription = ResString.GetMultilingualString("7b079f97-1884-44e1-bd40-f1db378b82c2", "Local Client");
		}

		void AddDatesFilters(ModuleFilterCollection filters)
		{
			var entryDateFilter = filters.AddDateFilter(FilterIdentifiers.ArrivalDate, CusUSLVClearanceSchema.ULH_EntryDate);
			entryDateFilter.Category = FilterCategories.Dates;
			entryDateFilter.MultilingualDescription = ResString.GetMultilingualString("e7e4e0e6-04e3-4118-92be-4ab7bab2122e", "Arrival Date");

			var dischargeDateFilter = filters.AddDateFilter(FilterIdentifiers.DischargeDate, CusUSLVClearanceSchema.ULH_DischargeDate);
			dischargeDateFilter.Category = FilterCategories.Dates;
			dischargeDateFilter.MultilingualDescription = ResString.GetMultilingualString("5b51d11e-7e25-41a8-8d45-b92a66b81737", "Discharge Date");

			var departureDateFilter = filters.AddDateFilter(FilterIdentifiers.DepartureDate, CusUSLVClearanceSchema.ULH_DepartureDate);
			departureDateFilter.Category = FilterCategories.Dates;
			departureDateFilter.MultilingualDescription = ResString.GetMultilingualString("76fc795d-dde6-47d6-8740-a790e5127fc3", "Departure Date");
		}

		#endregion

		#region Filter Queries

		static ZQuery GetHouseBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var mainQuery = new ZDBOnlyQuery(typeof(CusUSLVClearance));
			var consignmentSubQuery = GetConsignmentSubQuery();
			consignmentSubQuery.AddToFilter(new ZQuery(CusUSLVConsignmentSchema.ULB_HouseBill, comparisonOperator, value));
			mainQuery.AddSubQuery(consignmentSubQuery, JoinCondition.And);
			return mainQuery;
		}

		static ZQuery GetEntryNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var mainQuery = new ZDBOnlyQuery(typeof(CusUSLVClearance));
			var consignmentSubQuery = GetConsignmentSubQuery();
			var entryNumberSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			entryNumberSubQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
			entryNumberSubQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
			entryNumberSubQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			consignmentSubQuery.AddSubQuery(entryNumberSubQuery, JoinCondition.And);
			mainQuery.AddSubQuery(consignmentSubQuery, JoinCondition.And);
			return mainQuery;
		}

		static ZQuery GetContainerQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var mainQuery = new ZDBOnlyQuery(typeof(CusUSLVClearance));
			var consignmentSubQuery = GetConsignmentSubQuery();
			consignmentSubQuery.AddToFilter(new ZQuery(CusUSLVConsignmentSchema.ULB_EquipmentNumber, comparisonOperator, value));
			mainQuery.AddSubQuery(consignmentSubQuery, JoinCondition.And);
			return mainQuery;
		}

		static ZDBOnlySubQuery GetConsignmentSubQuery(bool isActive = true)
		{
			var consignmentSubQuery = new ZDBOnlySubQuery(typeof(CusUSLVConsignment), CusUSLVConsignmentSchema.ULB_ULH);
			consignmentSubQuery.AddToFilter(new ZQuery(CusUSLVConsignmentSchema.ULB_IsActive, isActive));
			return consignmentSubQuery;
		}

		#endregion

		#region Collection Lookups

		OrgHeaderCollection Organisations => organisations ?? (organisations = new OrgHeaderCollection(Factory));
		OrgHeaderCollection organisations;

		LocationCollection Locations => locations ?? (locations = new LocationCollection(Factory));
		LocationCollection locations;

		RefVesselCollection RefVessels => refVessels ?? (refVessels = new RefVesselCollection(Factory));
		RefVesselCollection refVessels;

		ZZRefCusCodeListCombinedCollection RegionDisctrictPorts => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);

		ZZRefCusCodeListCombinedCollection ForeignPorts => foreignPorts ?? (foreignPorts = new ZZRefCusCodeListCombinedCollection(Factory));
		ZZRefCusCodeListCombinedCollection foreignPorts;

		#endregion

	}
}
