using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Module;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.ModuleRegistration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using AsycudaBill = Enterprise.Customs.ZA.Business.AsycudaBill;
using AsycudaManifestHeader = Enterprise.Customs.ZA.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.ZA.Module
{
	public class OutturnAndGateInOutFilterBusinessObject : FilterStripBusinessObject
	{
		public static class FilterConstants
		{
			public const string JobNumber = "Job #";
			public const string BillNumber = "Bill Number";
			public const string CustomsOffice = "Customs Office";
			public const string EstimatedArrivalDate = "Estimated Arrival Date";
			public const string EstimatedDepartureDate = "Estimated Departure Date";
			public const string TransportMode = "Transport Mode";
			public const string VoyageCode = "Voyage Code";
			public const string PortOfLoadingDischarge = "Port of Loading / Discharge";
			public const string OutturnStatus = "Outturn Status";
			public const string OutturnType = "Outturn Type";
			public const string GateInOutMessageType = "Gate In/Out Message Type";
			public const string Nature = "Nature";
			public const string GateInOutDate = "Gate In/Out Date";
			public const string ContainerMode = "Container Mode";
			public const string UnpackedDate = "Date Time Unpacked";
			public const string GateInOutCustomsStatus = "Gate In/Out Status";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var jobNumberFilter = result.AddTextFilter(FilterConstants.JobNumber, AsycudaManifestHeaderSchema.AMA_JobReference);
			jobNumberFilter.Category = FilterCategories.NumbersAndReferences;
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|JobNumber", "Job #");

			var billNumberFilter = result.AddTextFilter(FilterConstants.BillNumber, GetMasterBillQuery);
			billNumberFilter.WithMaxLengthOf<ModuleTextFilter>(AsycudaBillSchema.ABL_BillNumber);
			billNumberFilter.Category = FilterCategories.NumbersAndReferences;
			billNumberFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|BillNumber", "Bill Number");

			var customsOfficeFilter = result.AddNkFilter(FilterConstants.CustomsOffice, AsycudaManifestHeaderSchema.AMA_CustomsOffice, CustomsModuleIDs.Universal.ZZRefCusCodeList, CustomsOffices);
			customsOfficeFilter.Category = FilterCategories.TextSearch;
			customsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|CustomsOffice", "Customs Office");

			var voyageCodeFilter = result.AddTextFilter(FilterConstants.VoyageCode, AsycudaManifestHeaderSchema.AMA_Voyage);
			voyageCodeFilter.Category = FilterCategories.TextSearch;
			voyageCodeFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|VoyageCode", "Voyage Code");

			var estimatedArrivalDateFilter = result.AddDateFilter(FilterConstants.EstimatedArrivalDate, GetMasterBillArrivalDateQuery);
			estimatedArrivalDateFilter.Category = FilterCategories.Dates;
			estimatedArrivalDateFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|EstimatedArrivalDate", "Estimated Arrival Date");

			var estimatedDepartureDateFilter = result.AddDateFilter(FilterConstants.EstimatedDepartureDate, GetMasterBillDepartureDateQuery);
			estimatedDepartureDateFilter.Category = FilterCategories.Dates;
			estimatedDepartureDateFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|EstimatedDepartureDate", "Estimated Departure Date");

			var gateInOutDateFilter = result.AddDateFilter(FilterConstants.GateInOutDate, (comparisonOperator, value1, value2) => GetGateInOutDate(comparisonOperator, value1, value2));
			gateInOutDateFilter.Category = FilterCategories.Dates;
			gateInOutDateFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|GateInOutDate", "Gate In/Out Date");

			var unpackedDateFilter = result.AddDateFilter(FilterConstants.UnpackedDate, (comparisonOperator, value1, value2) => GetUnpackedDate(comparisonOperator, value1, value2));
			unpackedDateFilter.Category = FilterCategories.Dates;
			unpackedDateFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|UnpackedDate", "Date Time Unpacked");

			var transportModeFilter = result.AddTextFilter(FilterConstants.TransportMode, AsycudaManifestHeaderSchema.AMA_TransportMode, TransportModeList);
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|TransportMode", "Transport Mode");

			var outturnTypeFilter = result.AddTextFilter(FilterConstants.OutturnType, AsycudaManifestHeaderSchema.AMA_ManifestType, ManifestTypeList);
			outturnTypeFilter.Category = FilterCategories.ModesAndTypes;
			outturnTypeFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|OutturnType", "Outturn Type");

			var gateInOutMessageTypeFilter = result.AddTextFilter(FilterConstants.GateInOutMessageType, GetGateInOutMessageType, GateInOutMessageTypeCodeList);
			gateInOutMessageTypeFilter.Category = FilterCategories.ModesAndTypes;
			gateInOutMessageTypeFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|GateInOutMessageType", "Gate In/Out Message Type");

			var natureFilter = new HideComparisonOperatorModuleTextFilter(FilterConstants.Nature, AsycudaManifestHeaderSchema.AMA_Nature, Natures);
			natureFilter.Category = FilterCategories.ModesAndTypes;
			natureFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|Nature", "Nature");
			result.AddFilter(natureFilter);

			var containerModeFilter = new HideComparisonOperatorModuleTextFilter(FilterConstants.ContainerMode, AsycudaManifestHeaderSchema.AMA_ContainerMode, ContainerModeList);
			containerModeFilter.Category = FilterCategories.ModesAndTypes;
			containerModeFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|ContainerMode", "Container Mode");
			result.AddFilter(containerModeFilter);

			var locationFilter = result.AddLocationFilter(FilterConstants.PortOfLoadingDischarge, GetTransportLoadDiscPort, Location_List, Location_List);
			locationFilter.SetItemDescriptions(Res.GetData("OutturnAndGateInOutFilterBusinessObject|Load", "Loading"), Res.GetData("OutturnAndGateInOutFilterBusinessObject|Discharge", "Discharge"));
			locationFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|PortOfLoadingDischarge", "Port of Loading / Discharge");
			locationFilter.Category = FilterCategories.Locations;

			var outturnStatusFilter = result.AddTextFilter(FilterConstants.OutturnStatus, GetOutturnStatusQuery, CustomsStatusList);
			outturnStatusFilter.Category = FilterCategories.StatusAndFlags;
			outturnStatusFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|OutturnStatus", "Outturn Status");

			var gateInOutCustomsStatusFilter = result.AddTextFilter(FilterConstants.GateInOutCustomsStatus, GetOutCustomsStatusQuery, CustomsStatusList);
			gateInOutCustomsStatusFilter.Category = FilterCategories.StatusAndFlags;
			gateInOutCustomsStatusFilter.MultilingualDescription = ResString.GetMultilingualString("OutturnAndGateInOutFilterBusinessObject|GateInOutCustomsStatus", "Gate In/Out Status");

			return result;
		}

		static ZQuery GetTransportLoadDiscPort(ZString loadPort, ZString discPort)
		{
			var query = new ZQuery();

			if (!loadPort.IsEmpty || !discPort.IsEmpty)
			{
				var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
				var transportSubQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
				transportSubQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);

				if (!loadPort.IsEmpty)
				{
					var isCountryCode = loadPort.Length == 2;
					var comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

					transportSubQuery.AddToFilter(JoinCondition.And, AsycudaBillSchema.ABL_RL_NKPortOfLoading, comparisonOperator, loadPort);
				}

				if (!discPort.IsEmpty)
				{
					var isCountryCode = discPort.Length == 2;
					var comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

					transportSubQuery.AddToFilter(JoinCondition.And, AsycudaBillSchema.ABL_RL_NKPortOfDischarge, comparisonOperator, discPort);
				}

				headerQuery.AddSubQuery(transportSubQuery, JoinCondition.And);

				query.AddToFilter(headerQuery);
			}

			return query;
		}

		static ZQuery GetOutCustomsStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var genAddOnColumnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, AsycudaManifestHeader.Schema.GateInOutCustomsStatus);
			genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, comparisonOperator, value);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, genAddOnColumnQuery, JoinCondition.And);
			return headerQuery;
		}

		static ZQuery GetUnpackedDate(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));

			var genAddOnColumnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, AsycudaManifestHeader.Schema.UnpackedDate);

			switch (comparisonOperator)
			{
				case DateComparisonOperator.HasDateEntered:
					genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.EqualToDatePartOnly, dateFrom.SqlFormat);
					break;
				case DateComparisonOperator.HasNoDateEntered:
					genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, ZDateTime.Empty);
					break;
				default:
					genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, dateFrom.SqlFormat);
					genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.LessThan, dateTo.SqlFormat);
					break;
			}

			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, genAddOnColumnQuery, JoinCondition.And);
			return headerQuery;
		}

		static ZQuery GetGateInOutDate(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var genAddOnColumnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, AsycudaManifestHeader.Schema.GateInOutDate);

			switch (comparisonOperator)
			{
				case DateComparisonOperator.HasDateEntered:
					genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.EqualToDatePartOnly, dateFrom.SqlFormat);
					break;
				case DateComparisonOperator.HasNoDateEntered:
					genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, ZDateTime.Empty);
					break;
				default:
					genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, dateFrom.SqlFormat);
					genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.LessThan, dateTo.SqlFormat);
					break;
			}

			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, genAddOnColumnQuery, JoinCondition.And);
			return headerQuery;
		}

		static ZQuery GetGateInOutMessageType(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var genAddOnColumnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, AsycudaManifestHeader.Schema.GateInOutMessageType);
			genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, comparisonOperator, value);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, genAddOnColumnQuery, JoinCondition.And);
			return headerQuery;
		}

		static ZQuery GetOutturnStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var cusEntryNumfilter = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumfilter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration);
			cusEntryNumfilter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.SouthAfrica);
			cusEntryNumfilter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryStatus, comparisonOperator, value);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, cusEntryNumfilter, JoinCondition.And);
			return headerQuery;
		}

		public CodeDescriptionPairList CustomsStatusList => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus);

		ZZRefCusCodeListCombinedCollection CustomsOffices => new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Now);

		CodeDescriptionPairList TransportModeList
		{
			get
			{
				return Factory.GetCachedValue("OutturnAndGateInOutFilterBusinessObject|TransportModeList", delegate
				{
					var allPossibleModes = new CodeDescriptionPairList();
					allPossibleModes.AddRange(new Customs.Business.TransportTypeList());
					allPossibleModes.AddPairIfNotExist(Core.Constants.TransportModes.Road, "Road");
					return allPossibleModes;
				});
			}
		}

		CodeDescriptionPairList ContainerModeList => Factory.GetCachedValue("CC7BB063-1170-477D-9A8D-64ACE06B5930", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
			result.AddPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
			result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
			result.AddPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
			result.AddPair(Core.Constants.ContainerModes.Other, Core.Constants.ContainerModeDescriptions.Other);
			return result;
		});

		CodeDescriptionPairList Natures => Factory.GetCachedValue<NatureList>();

		CodeDescriptionPairList ManifestTypeList => Factory.GetCachedValue<ManifestTypeList>();

		CodeDescriptionPairList GateInOutMessageTypeCodeList => Factory.GetCachedValue<GateInOutMessageTypeCodeList>();

		LocationCollection Location_List => fLocation_List ?? (fLocation_List = new LocationCollection(Factory));
		LocationCollection fLocation_List;

		ZQuery GetMasterBillArrivalDateQuery(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, AsycudaBillSchema.ABL_E_ARV, dateFrom, dateTo);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, subQuery, JoinCondition.And);
			return headerQuery;
		}

		ZQuery GetMasterBillDepartureDateQuery(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, AsycudaBillSchema.ABL_E_DEP, dateFrom, dateTo);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, subQuery, JoinCondition.And);
			return headerQuery;
		}

		static ZQuery GetMasterBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var subquery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			subquery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, comparisonOperator, value);
			subquery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, subquery, JoinCondition.And);
			return headerQuery;
		}
	}
}
