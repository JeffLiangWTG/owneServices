using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
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
	public class USConsignmentCombinedFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filter Identifiers

		public static class FilterIdentifiers
		{
			public const string EntryNumber = nameof(EntryNumber);
			public const string JobNumber = nameof(JobNumber);
			public const string HouseBill = nameof(HouseBill);
			public const string MasterBill = nameof(MasterBill);
			public const string MessageStatus = nameof(MessageStatus);
			public const string ReleaseStatus = nameof(ReleaseStatus);
			public const string DepartureDate = nameof(DepartureDate);
			public const string DischargeDate = nameof(DischargeDate);
			public const string EntryDate = nameof(EntryDate);
			public const string ReleaseDate = nameof(ReleaseDate);
			public const string SubmittedDate = nameof(SubmittedDate);
			public const string TransportMode = nameof(TransportMode);
			public const string VesselFlightVoyage = nameof(VesselFlightVoyage);
			public const string LoadingPort = nameof(LoadingPort);
			public const string DischargePort = nameof(DischargePort);
			public const string EntryPort = nameof(EntryPort);
			public const string LoadDiscargeUNLOCO = nameof(LoadDiscargeUNLOCO);
			public const string Client = nameof(Client);
			public const string Consignee = nameof(Consignee);
			public const string ConsigneeName = nameof(ConsigneeName);
			public const string ImporterOfRecord = nameof(ImporterOfRecord);
			public const string Seller = nameof(Seller);
			public const string SellerName = nameof(SellerName);
			public const string ActionRequired = nameof(ActionRequired);
		}

		#endregion

		#region Add Module Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddNumbersAndReferencesFilters(filters);
			AddStatusAndFlagsFilters(filters);
			AddDatesFilters(filters);
			AddModesAndTypes(filters);
			AddLocationsFilters(filters);
			AddOrganisationFilters(filters);
			return filters;
		}

		void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			var entryNumberFilter = filters.AddTextFilter(FilterIdentifiers.EntryNumber, USConsignmentCombinedSchema.UBV_EntryNum);
			entryNumberFilter.Category = FilterCategories.NumbersAndReferences;
			entryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("c03628e1-db2f-44fe-8aa7-f60eab6aae53", "Entry Number");

			var jobNumberFilter = filters.AddTextFilter(FilterIdentifiers.JobNumber, USConsignmentCombinedSchema.UBV_JobReference);
			jobNumberFilter.Category = FilterCategories.NumbersAndReferences;
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("e7c9e841-12f3-4381-a318-169242beee4b", "Job Number");

			var houseBillFilter = filters.AddTextFilter(FilterIdentifiers.HouseBill, USConsignmentCombinedSchema.UBV_HouseBill);
			houseBillFilter.Category = FilterCategories.NumbersAndReferences;
			houseBillFilter.MultilingualDescription = ResString.GetMultilingualString("9f7c5c66-77fb-4dc7-a9a8-651bc5d0004c", "House Bill");

			var masterBillFilter = filters.AddTextFilter(FilterIdentifiers.MasterBill, USConsignmentCombinedSchema.UBV_MasterBill);
			masterBillFilter.Category = FilterCategories.NumbersAndReferences;
			masterBillFilter.MultilingualDescription = ResString.GetMultilingualString("d35844bd-70cd-45c3-8941-13d3d738c074", "Master Bill");
		}

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var messageStatusFilter = filters.AddTextFilter(FilterIdentifiers.MessageStatus, USConsignmentCombinedSchema.UBV_MessageStatus, MessageStatusList);
			messageStatusFilter.Category = FilterCategories.StatusAndFlags;
			messageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("0689b516-a86f-4a7f-ac93-e2b8523f6f98", "Message Status");

			var releaseStatusFilter = filters.AddTextFilter(FilterIdentifiers.ReleaseStatus, USConsignmentCombinedSchema.UBV_ReleaseStatus, ReleaseStatusList);
			releaseStatusFilter.Category = FilterCategories.StatusAndFlags;
			releaseStatusFilter.MultilingualDescription = ResString.GetMultilingualString("04fb051c-12b0-49d0-a246-b12e2932a4fa", "Release Status");

			var actionRequiredFilter = filters.AddTextFilter(FilterIdentifiers.ActionRequired, GetActionRequiredQuery, ActionRequiredList);
			actionRequiredFilter.MaxLength = 15;
			actionRequiredFilter.Category = FilterCategories.StatusAndFlags;
			actionRequiredFilter.MultilingualDescription = ResString.GetMultilingualString("a420c50f-80fa-4e06-9077-00226cd5ea7e", "Action Required");
		}

		ZQuery GetActionRequiredQuery(ZString value)
		{
			var result = new ZQuery();

			if (value == PGAApplyButNotDisclaimed)
			{
				result.AddToFilter(USConsignmentCombinedSchema.UBV_HasPGAPending, true);
			}
			else if (value == NotSupportedInLVS)
			{
				result.AddToFilter(USConsignmentCombinedSchema.UBV_PGANotSupported, true);
			}

			return result;
		}

		void AddDatesFilters(ModuleFilterCollection filters)
		{
			var departureDateFilter = filters.AddDateFilter(FilterIdentifiers.DepartureDate, USConsignmentCombinedSchema.UBV_DepartureDate);
			departureDateFilter.Category = FilterCategories.Dates;
			departureDateFilter.MultilingualDescription = ResString.GetMultilingualString("01c6d853-e57c-4988-8eef-57b6c9dca467", "Departure");

			var dischargeDateFilter = filters.AddDateFilter(FilterIdentifiers.DischargeDate, USConsignmentCombinedSchema.UBV_DischargeDate);
			dischargeDateFilter.Category = FilterCategories.Dates;
			dischargeDateFilter.MultilingualDescription = ResString.GetMultilingualString("21556be8-8178-4375-b07a-dabcc943cac8", "Discharge");

			var entryDateFilter = filters.AddDateFilter(FilterIdentifiers.EntryDate, USConsignmentCombinedSchema.UBV_EntryDate);
			entryDateFilter.Category = FilterCategories.Dates;
			entryDateFilter.MultilingualDescription = ResString.GetMultilingualString("b195d2b6-0b49-49da-9d2e-cac23bcd1beb", "Port of Entry");

			var releaseDateFilter = filters.AddDateFilter(FilterIdentifiers.ReleaseDate, USConsignmentCombinedSchema.UBV_ReleaseDate);
			releaseDateFilter.Category = FilterCategories.Dates;
			releaseDateFilter.MultilingualDescription = ResString.GetMultilingualString("b6d0633d-bf94-4617-9ecb-3d4a2565710f", "Released");

			var submittedDateFilter = filters.AddDateFilter(FilterIdentifiers.SubmittedDate, USConsignmentCombinedSchema.UBV_SubmittedDate);
			submittedDateFilter.Category = FilterCategories.Dates;
			submittedDateFilter.MultilingualDescription = ResString.GetMultilingualString("4604cabd-08e0-419f-8278-86bb92e24bd7", "Submitted");
		}

		void AddModesAndTypes(ModuleFilterCollection filters)
		{
			var transportModeFilter = filters.AddTextFilter(FilterIdentifiers.TransportMode, USConsignmentCombinedSchema.UBV_TransportMode, USConsignmentCombinedLookups.GetTransportModes(Factory));
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("9a40abac-5062-48da-919e-7d4d7cd921b3", "Transport Mode");

			var vesselFlightVoyageFilter = filters.AddTextAndNkFilter(FilterIdentifiers.VesselFlightVoyage, USConsignmentCombinedSchema.UBV_VoyageFlightNo, ModuleIDs.RefVessel, USConsignmentCombinedSchema.UBV_ConveyanceName, RefVessels);
			vesselFlightVoyageFilter.Category = FilterCategories.ModesAndTypes;
			vesselFlightVoyageFilter.MultilingualDescription = ResString.GetMultilingualString("7eb62218-c37d-40bf-a667-483d8cff8526", "Vessel and Flight/Voyage #");
		}

		void AddLocationsFilters(ModuleFilterCollection filters)
		{
			var dischargePortFilter = filters.AddNkFilter(FilterIdentifiers.DischargePort, USConsignmentCombinedSchema.UBV_PortOfDischarge, ModuleIDs.Customs.Universal.ZZRefCusCodeList, RegionDisctrictPorts);
			dischargePortFilter.Category = FilterCategories.Locations;
			dischargePortFilter.MultilingualDescription = ResString.GetMultilingualString("54d5a926-b1d2-4a1d-a885-1bb11e246e22", "Discharge Port");
			dischargePortFilter.ForeignCodeColumnOverride = ZZRefCusCodeListCombinedSchema.ZZD_Code;

			var entryPortFilter = filters.AddNkFilter(FilterIdentifiers.EntryPort, USConsignmentCombinedSchema.UBV_PortOfEntry, ModuleIDs.Customs.Universal.ZZRefCusCodeList, RegionDisctrictPorts);
			entryPortFilter.Category = FilterCategories.Locations;
			entryPortFilter.MultilingualDescription = ResString.GetMultilingualString("f452a7a0-29f1-4e60-9328-e9a530040461", "Entry Port");
			entryPortFilter.ForeignCodeColumnOverride = ZZRefCusCodeListCombinedSchema.ZZD_Code;

			var loadingPortFilter = filters.AddNkFilter(FilterIdentifiers.LoadingPort, USConsignmentCombinedSchema.UBV_PortOfLoading, ModuleIDs.Customs.Universal.ZZRefCusCodeList, ForeignPorts);
			loadingPortFilter.Category = FilterCategories.Locations;
			loadingPortFilter.MultilingualDescription = ResString.GetMultilingualString("f61ea635-000f-4bc2-9180-fb05318b3a86", "Loading Port");
			loadingPortFilter.ForeignCodeColumnOverride = ZZRefCusCodeListCombinedSchema.ZZD_Code;

			var loadDiscargeUNLOCOFilter = filters.AddLocationFilter(FilterIdentifiers.LoadDiscargeUNLOCO, USConsignmentCombinedSchema.UBV_RL_NKPortOfLoading, Locations, USConsignmentCombinedSchema.UBV_RL_NKPortOfDischarge, Locations);
			loadDiscargeUNLOCOFilter.Category = FilterCategories.Locations;
			loadDiscargeUNLOCOFilter.MultilingualDescription = ResString.GetMultilingualString("876a5c40-637d-4138-8b5b-82d4bbaf7604", "Load/Discharge");
			loadDiscargeUNLOCOFilter.SetItemDescriptions(Res.GetData("3dae6e81-722d-4bf5-b400-4e2189a4633d", "Load"), Res.GetData("617324a7-f0d4-41f5-8718-a4de99d0f720", "Discharge"));
		}

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var clientFilter = filters.AddGuidFilter(FilterIdentifiers.Client, ModuleIDs.Organisation, USConsignmentCombinedSchema.UBV_OH_Client, Organisations);
			clientFilter.Category = FilterCategories.Organisations;
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("dce9052f-ddba-450b-808d-e8ff56e31765", "Client");

			var consigneeFilter = filters.AddGuidFilter(FilterIdentifiers.Consignee, ModuleIDs.Organisation, USConsignmentCombinedSchema.UBV_OH_Consignee, Organisations);
			consigneeFilter.Category = FilterCategories.Organisations;
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("fe9c4f5d-5096-4f7d-84a1-0c23731b6e9f", "Consignee");

			var consigneeNameFilter = filters.AddTextFilter(FilterIdentifiers.ConsigneeName, USConsignmentCombinedSchema.UBV_ConsigneeName);
			consigneeNameFilter.Category = FilterCategories.Organisations;
			consigneeNameFilter.MultilingualDescription = ResString.GetMultilingualString("16a2954c-2a0c-422a-8dda-72cbce0be1af", "Consignee Name");

			var importerOfRecordFilter = filters.AddGuidFilter(FilterIdentifiers.ImporterOfRecord, ModuleIDs.Organisation, USConsignmentCombinedSchema.UBV_OH_Importer, Organisations);
			importerOfRecordFilter.Category = FilterCategories.Organisations;
			importerOfRecordFilter.MultilingualDescription = ResString.GetMultilingualString("a07c61cb-44fc-4142-b107-b7b988aeb7bb", "Importer of Record");

			var sellerFilter = filters.AddGuidFilter(FilterIdentifiers.Seller, ModuleIDs.Organisation, USConsignmentCombinedSchema.UBV_OH_Seller, Organisations);
			sellerFilter.Category = FilterCategories.Organisations;
			sellerFilter.MultilingualDescription = ResString.GetMultilingualString("6029515e-a113-4fc6-869b-a8768ff94abd", "Seller");

			var sellerNameFilter = filters.AddTextFilter(FilterIdentifiers.SellerName, USConsignmentCombinedSchema.UBV_SellerName);
			sellerNameFilter.Category = FilterCategories.Organisations;
			sellerNameFilter.MultilingualDescription = ResString.GetMultilingualString("e1e6f017-1d9e-4c00-99e8-c926de29914d", "Seller Name");
		}

		#endregion

		#region Collection and List Lookups

		ImportMessageStatusList MessageStatusList => Factory.GetCachedValue<ImportMessageStatusList>();

		CRLReleaseStatusList ReleaseStatusList => Factory.GetCachedValue<CRLReleaseStatusList>();

		OrgHeaderCollection Organisations => organisations ?? (organisations = new OrgHeaderCollection(Factory));
		OrgHeaderCollection organisations;

		LocationCollection Locations => locations ?? (locations = new LocationCollection(Factory));
		LocationCollection locations;

		RefVesselCollection RefVessels => refVessels ?? (refVessels = new RefVesselCollection(Factory));
		RefVesselCollection refVessels;

		ZZRefCusCodeListCombinedCollection RegionDisctrictPorts => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);

		ZZRefCusCodeListCombinedCollection ForeignPorts => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);

		CodeDescriptionPairList ActionRequiredList
		{
			get
			{
				return Factory.GetCachedValue("LowValueEntriesActionRequiredList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(PGAApplyButNotDisclaimed, "PGA applies but has not been disclaimed");
					result.AddPair(NotSupportedInLVS, "Not supported in this module");
					return result;
				});
			}
		}
		public const string PGAApplyButNotDisclaimed = "Not Disclaimed";
		public const string NotSupportedInLVS = "Not Supported";

		#endregion

	}
}
