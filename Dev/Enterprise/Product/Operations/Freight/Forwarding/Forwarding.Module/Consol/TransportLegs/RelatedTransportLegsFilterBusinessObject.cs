using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	class RelatedTransportLegsFilterBusinessObject : FilterStripBusinessObject
	{
		public class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string VoyageFlightNum = "Voyage / Flight";
			public const string VesselJourneyName = "Vessel / Journey Name";
			public const string LoadDischarge = "Load / Discharge";
			public const string CarrierProvider = "Carrier / Provider";
			public const string TransportMode = "Transport Mode";
			public const string AdditionalTransportMode = "Additional Transport Mode";
			public const string TransportType = "Transport Type";
			public const string IsLinked = "Is Linked";
			public const string FlightStatus = "Flight Status";

			public const string ETA = "ETA";
			public const string ATA = "ATA";
			public const string ETD = "ETD";
			public const string ATD = "ATD";
			public const string CTOCutOff = "CTO Cut Off";
			public const string CFSCutOff = "CFS Cut Off";
			public const string DocsDue = "Docs Due";
			public const string VGMCutOff = "VGM Cut Off";
			public const string CTOReceival = "CTO Receival";
			public const string CFSReceival = "CFS Receival";
			public const string CTOAvailable = "CTO Available";
			public const string CFSAvailable = "CFS Available";
			public const string CTOStorage = "CTO Storage";
			public const string CFSStorage = "CFS Storage";
			#endregion
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddDateFilters(filters);

			var voyageFlightFilter = filters.AddTextFilter(Descriptions.VoyageFlightNum, GetVoyageFlightQuery)
				.WithMaxLengthOf<ModuleTextFilter>(JobVoyageSchema.JV_VoyageFlight);
			voyageFlightFilter.Category = FilterCategories.NumbersAndReferences;
			voyageFlightFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|VoyageFlight", "Voyage / Flight");

			var vesselJourneyNameFilter = filters.AddNkFilter(Descriptions.VesselJourneyName, GetVesselJourneyNameQuery, ModuleIDs.RefVessel, BindingLists.RefVessel_List);
			vesselJourneyNameFilter.Category = FilterCategories.NumbersAndReferences;
			vesselJourneyNameFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|VesselJourneyName", "Vessel / Journey Name");

			var loadDischargePortsFilter = filters.AddLocationFilter(Descriptions.LoadDischarge, GetLoadDischargePortsQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List);
			loadDischargePortsFilter.SetItemDescriptions(Res.GetData("Forwarding|TransportLegs|Load", "Load"), Res.GetData("Forwarding|TransportLegs|Discharge", "Discharge"));
			loadDischargePortsFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|LoadDischarge", "Load / Discharge");

			var carrierProviderFilter = filters.AddGuidFilter(Descriptions.CarrierProvider, ModuleIDs.Organisation, GetCarrierQuery, BindingLists.ShippingProvider_List);
			carrierProviderFilter.Category = FilterCategories.Organisations;
			carrierProviderFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|CarrierProvider", "Carrier / Provider");

			var transportModeFilter = filters.AddTextFilter(Descriptions.TransportMode, JobConsolTransportSchema.JW_TransportMode, TransportMode_List);
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|TransportMode", "Transport Mode");

			var additionalTransportModeFilter = filters.AddTextFilter(Descriptions.AdditionalTransportMode, JobConsolTransportSchema.JW_AdditionalTransportMode, AdditionalTransportMode_List);
			additionalTransportModeFilter.Category = FilterCategories.ModesAndTypes;
			additionalTransportModeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|AdditionalTransportMode", "Additional Transport Mode");

			var isLinkedFilter = filters.AddFlagsFilter(Descriptions.IsLinked, new string[] { Res.GetString("Forwarding|TransportLegs|IsLinked", "Is Linked") }, new GetFlagsQuery[] { GetIsLinkedQuery });
			isLinkedFilter.Category = FilterCategories.StatusAndFlags;
			isLinkedFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|IsLinked", "Is Linked");

			var transportTypeFilter = filters.AddTextFilter(Descriptions.TransportType, JobConsolTransportSchema.JW_TransportType, TransportType_List);
			transportTypeFilter.Category = FilterCategories.ModesAndTypes;
			transportTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|TransportType", "Transport Type");

			var flightStatusFilter = filters.AddTextFilter(Descriptions.FlightStatus, GetFlightStatusQuery, FlightStatusList);
			flightStatusFilter.Category = FilterCategories.StatusAndFlags;
			flightStatusFilter.ErrorOnCodeNotPresent = true;
			flightStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|FlightStatus", "Flight Status");

			return filters;
		}

		ZQuery GetVoyageFlightQuery(SQLComparisonOperator comparisonOperator, ZString vessel)
		{
			var builder = new SailingFilterBuilder(Factory);
			builder.VoyageFlight = vessel;
			builder.VoyageFlightComparisonOperator = comparisonOperator;

			return builder.ToTransportFilter();
		}

		ZQuery GetVesselJourneyNameQuery(ZString vessel)
		{
			var builder = new SailingFilterBuilder(Factory);
			builder.Vessel = vessel;

			return builder.ToTransportFilter();
		}

		ZQuery GetLoadDischargePortsQuery(ZString loadPortNK, ZString dischargePortPK)
		{
			var builder = new SailingFilterBuilder(Factory);
			builder.LoadPort = loadPortNK;
			builder.DischargePort = dischargePortPK;

			return builder.ToTransportFilter();
		}

		ZQuery GetIsLinkedQuery(ZBool thisValueIsIgnored)
		{
			return new ZQuery(JobConsolTransportSchema.JW_IsLinked, thisValueIsIgnored);
		}

		ZQuery GetCarrierQuery(ZGuid carrierPK)
		{
			var transportCarrierAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolTransportSchema.JW_OA_CarrierAddress);
			transportCarrierAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, carrierPK);

			var notIncludeSeaLinkedTransportsQuery = new ZQuery();
			notIncludeSeaLinkedTransportsQuery.AddToFilter(JoinCondition.Or, JobConsolTransportSchema.JW_TransportMode, SQLComparisonOperator.NotEqual, Constants.TransportModes.Sea);
			notIncludeSeaLinkedTransportsQuery.AddToFilter(JoinCondition.Or, JobConsolTransportSchema.JW_JX, null);

			var consolTransportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.PK);
			consolTransportSubQuery.AddToFilter(notIncludeSeaLinkedTransportsQuery, JoinCondition.And);
			consolTransportSubQuery.AddSubQuery(transportCarrierAddressSubQuery, JoinCondition.And);

			var voyageSubQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);
			voyageSubQuery.AddToFilter(JobVoyageSchema.JV_OH_Line, carrierPK);

			var originSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			originSubQuery.AddSubQuery(voyageSubQuery, JoinCondition.And);

			var sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobConsolTransportSchema.JW_JX);
			sailingSubQuery.AddSubQuery(originSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(Transport));
			result.AddSubQuery(sailingSubQuery, JoinCondition.Or);
			result.AddSubQuery(consolTransportSubQuery, JoinCondition.Or);

			return result;
		}

		#region GetFlightStatusQuery

		ZQuery GetFlightStatusQuery(ZString flightStatus)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(Transport));
			result.AddToFilter(JobConsolTransportSchema.JW_TransportMode, Constants.TransportModes.Air);
			result.AddToFilter(JobConsolTransportSchema.JW_OnlineScheduleStatus, flightStatus);
			return result;
		}

		#endregion

		BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		CodeDescriptionPairList TransportMode_List
		{
			get { return FreightCodePairLists.RoutingTransportModeList(); }
		}

		CodeDescriptionPairList AdditionalTransportMode_List => FreightCodePairLists.AdditionalTransportModeList();

		CodeDescriptionPairList TransportType_List
		{
			get { return FreightCodePairLists.RoutingTransportTypeList(string.Empty); }
		}

		CodeDescriptionPairList FlightStatusList => JobConsolTransportLookups.GetFlightStatusList(Factory);

		#region Date Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(Descriptions.ETA, GetEtaQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|ETA", "ETA");
			filters.AddDateFilter(Descriptions.ATA, GetAtaQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|ATA", "ATA");
			filters.AddDateFilter(Descriptions.ETD, GetEtdQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|ETD", "ETD");
			filters.AddDateFilter(Descriptions.ATD, GetAtdQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|ATD", "ATD");
			filters.AddDateFilter(Descriptions.CTOCutOff, GetCTOCutOffQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|CTOCutOff", "CTO Cut Off");
			filters.AddDateFilter(Descriptions.CFSCutOff, GetCFSCutOffQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|CFSCutOff", "CFS Cut Off");
			filters.AddDateFilter(Descriptions.DocsDue, GetDocsDueQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|DocsDue", "Docs Due");
			filters.AddDateFilter(Descriptions.VGMCutOff, GetVGMCutOffQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|VGMCutOff", "VGM Cut Off");
			filters.AddDateFilter(Descriptions.CTOReceival, GetCTOReceivalQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|CTOReceival", "CTO Receival");
			filters.AddDateFilter(Descriptions.CFSReceival, GetCFSReceivalQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|CFSReceival", "CFS Receival");
			filters.AddDateFilter(Descriptions.CTOAvailable, GetCTOAvailableQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|CTOAvailable", "CTO Available");
			filters.AddDateFilter(Descriptions.CFSAvailable, GetCFSAvailableQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|CFSAvailable", "CFS Available");
			filters.AddDateFilter(Descriptions.CTOStorage, GetCTOStorageQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|CTOStorage", "CTO Storage");
			filters.AddDateFilter(Descriptions.CFSStorage, GetCFSStorageQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|TransportLegs|CFSStorage", "CFS Storage");
		}

		ZQuery GetEtaQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetTransportDateTimeQuery(comparisonOperator, SailingFilterBuilder.Dates.ETA, date1, date2);
		}

		ZQuery GetAtaQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetTransportDateTimeQuery(comparisonOperator, SailingFilterBuilder.Dates.ATA, date1, date2);
		}

		ZQuery GetEtdQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetTransportDateTimeQuery(comparisonOperator, SailingFilterBuilder.Dates.ETD, date1, date2);
		}

		ZQuery GetAtdQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetTransportDateTimeQuery(comparisonOperator, SailingFilterBuilder.Dates.ATD, date1, date2);
		}

		ZQuery GetCTOCutOffQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetTransportDateTimeQuery(comparisonOperator, SailingFilterBuilder.Dates.CTOCutOff, date1, date2);
		}

		ZQuery GetCFSCutOffQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetTransportDateTimeQuery(comparisonOperator, SailingFilterBuilder.Dates.CFSCutOff, date1, date2);
		}

		ZQuery GetDocsDueQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetTransportDateTimeQuery(comparisonOperator, SailingFilterBuilder.Dates.DocsDue, date1, date2);
		}

		ZQuery GetVGMCutOffQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetTransportDateTimeQuery(comparisonOperator, SailingFilterBuilder.Dates.VGMCutOff, date1, date2);
		}

		ZQuery GetCTOReceivalQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetTransportDateTimeQuery(comparisonOperator, SailingFilterBuilder.Dates.CTOReceival, date1, date2);
		}

		ZQuery GetCFSReceivalQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetTransportDateTimeQuery(comparisonOperator, SailingFilterBuilder.Dates.CFSReceival, date1, date2);
		}

		ZQuery GetCTOAvailableQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetTransportDateTimeQuery(comparisonOperator, SailingFilterBuilder.Dates.CTOAvailable, date1, date2);
		}

		ZQuery GetCFSAvailableQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetTransportDateTimeQuery(comparisonOperator, SailingFilterBuilder.Dates.CFSAvailable, date1, date2);
		}

		ZQuery GetCTOStorageQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetTransportDateTimeQuery(comparisonOperator, SailingFilterBuilder.Dates.CTOStorage, date1, date2);
		}

		ZQuery GetCFSStorageQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetTransportDateTimeQuery(comparisonOperator, SailingFilterBuilder.Dates.CFSStorage, date1, date2);
		}

		ZQuery GetTransportDateTimeQuery(DateComparisonOperator comparisonOperator, SailingFilterBuilder.Dates dateType, ZDateTime date1, ZDateTime date2)
		{
			var builder = new SailingFilterBuilder(Factory);
			builder.SetDateRange(dateType, comparisonOperator, date1, date2);
			return builder.ToTransportFilter();
		}

		#endregion
	}
}
