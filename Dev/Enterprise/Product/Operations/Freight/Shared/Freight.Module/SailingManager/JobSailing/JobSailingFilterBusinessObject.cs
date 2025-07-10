using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module
{
	public abstract class JobSailingFilterBusinessObject : FilterStripBusinessObject
	{
		#region SubGroups

		ModuleFilterSubGroup VoyageOriginProcessor => voyageOriginProcessor ?? (voyageOriginProcessor = new VoyageOriginSubGroup());
		VoyageOriginSubGroup voyageOriginProcessor;

		protected ModuleFilterSubGroup JobSailingFilterProcessor => jobSailingFilterProcessor ?? (jobSailingFilterProcessor = new JobSailingSubGroup(VoyageOriginProcessor));
		JobSailingSubGroup jobSailingFilterProcessor;

		ModuleFilterSubGroup LoadPortFilterProcessor => loadPortFilterProcessor ?? (loadPortFilterProcessor = new LoadPortSubGroup(VoyageOriginProcessor));
		LoadPortSubGroup loadPortFilterProcessor;

		ModuleFilterSubGroup DischargePortFilterProcessor => dischargePortFilterProcessor ?? (dischargePortFilterProcessor = new DischargePortSubGroup());
		DischargePortSubGroup dischargePortFilterProcessor;

		class VoyageOriginSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var jobVoySubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
				jobVoySubQuery.AddToFilter(filter);

				var jobSailingQuery = new ZDBOnlyQuery(typeof(JobSailing));
				jobSailingQuery.AddSubQuery(jobVoySubQuery, JoinCondition.And);
				return jobSailingQuery;
			}
		}

		protected class JobSailingSubGroup : ModuleFilterSubGroup
		{
			public JobSailingSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var jobVoySubQuery = new ZDBOnlyQuery(typeof(VoyageOrigin));
				var jobVoyageSubQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);
				jobVoyageSubQuery.IgnoreActiveFilter = true;
				jobVoyageSubQuery.AddToFilter(filter);
				jobVoySubQuery.AddSubQuery(jobVoyageSubQuery, JoinCondition.And);

				return jobVoySubQuery;
			}
		}

		class LoadPortSubGroup : ModuleFilterSubGroup
		{
			public LoadPortSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var originQuery = new ZDBOnlyQuery(typeof(VoyageOrigin));
				var refUNLocoQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), JobVoyOriginSchema.JA_RL_NKPortOfLoading);
				refUNLocoQuery.AddToFilter(filter);
				originQuery.AddSubQuery(JobVoyOriginSchema.JA_RL_NKPortOfLoading, RefUNLOCOSchema.RL_Code, refUNLocoQuery, JoinCondition.And);

				return originQuery;
			}
		}

		class DischargePortSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobSailing));
				ZDBOnlySubQuery destinationQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
				ZDBOnlySubQuery refUNLocoQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), JobVoyDestinationSchema.JB_RL_NKPortOfDischarge);
				refUNLocoQuery.AddToFilter(filter);
				destinationQuery.AddSubQuery(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, RefUNLOCOSchema.RL_Code, refUNLocoQuery, JoinCondition.And);
				query.AddSubQuery(destinationQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		public ZBool UserIsLoggedIn { get; set; }

		protected ZBool ReplaceFiltersRequiringAuthorisation
		{
			get
			{
				return Globals.IsWeb && !UserIsLoggedIn;
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddHiddenFilters(filters);
			AddModesAndTypesFilters(filters);
			AddNumbersFilter(filters);
			AddDateFilter(filters);
			AddOrgsFilter(filters);
			AddLocationFilter(filters);
			AddStatusFlagFilters(filters);
			AddTextFilters(filters);

			return filters;
		}

		protected ZDBOnlySubQuery CreateVoyageSubQuery(SchemaColumn foreignKey)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(JobVoyage), foreignKey);
			subQuery.IgnoreActiveFilter = true;
			return subQuery;
		}

		#region AddToFilters Functions

		void AddHiddenFilters(ModuleFilterCollection filters)
		{
			filters.AddFlagsFilter("HiddenFilters", new string[] { "<ERROR>" }, new GetFlagsQuery[] { GetHiddenFilter }).Visibility = FilterVisibility.AlwaysAppliedAndHidden;
		}

		void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddTextFilter("Charter", GetCharterFilter, CharterFilterList);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|Charter", "Charter");
		}

		void AddNumbersFilter(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddNumberFilter(BookingRefLabel, JobSailingSchema.JX_ReservedMasterBill);
			filter.MultilingualDescription = BookingRefCaption;
			ModuleFilter refNumFilter = filters.AddNumberFilter(ReferenceNumberLabel, JobSailingSchema.JX_UniqueReference);
			refNumFilter.MultilingualDescription = ReferenceNumberCaption;
		}

		void AddDateFilter(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(DateFilterTypes.LoadPortETA, GetSailingDateTimeLoadETAFilter).MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|LoadPortETA", "Load Port ETA");
			filters.AddDateFilter(DateFilterTypes.LoadPortATA, GetSailingDateTimeLoadATAFilter).MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|LoadPortATA", "Load Port ATA");
			filters.AddDateFilter(DateFilterTypes.LoadPortSTA, GetSailingDateTimeLoadSTAFilter).MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|LoadPortSTA", "Load Port STA");

			filters.AddDateFilter(DateFilterTypes.LoadPortETD, GetSailingDateTimeLoadETDFilter).MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|LoadPortETD", "Load Port ETD");
			filters.AddDateFilter(DateFilterTypes.LoadPortATD, GetSailingDateTimeLoadATDFilter).MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|LoadPortATD", "Load Port ATD");
			filters.AddDateFilter(DateFilterTypes.LoadPortSTD, GetSailingDateTimeLoadSTDFilter).MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|LoadPortSTD", "Load Port STD");

			filters.AddDateFilter(DateFilterTypes.DischargePortETA, GetSailingDateTimeDischargeETAFilter).MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|DischargePortETA", "Discharge Port ETA");
			filters.AddDateFilter(DateFilterTypes.DischargePortATA, GetSailingDateTimeDischargeATAFilter).MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|DischargePortATA", "Discharge Port ATA");
			filters.AddDateFilter(DateFilterTypes.DischargePortSTA, GetSailingDateTimeDischargeSTAFilter).MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|DischargePortSTA", "Discharge Port STA");
		}

		void AddOrgsFilter(ModuleFilterCollection filters)
		{
			if (Globals.IsWeb)
			{
				var lineFilter = filters.AddTextFilter(OrgFilterTypes.Line, GetLineFilter);
				lineFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|Line", "Carrier");
				lineFilter.SubGroup = JobSailingFilterProcessor;
			}
			else
			{
				var lineFilter = filters.AddGuidFilter(OrgsFilterTypes.Line, ModuleIDs.Organisation, JobVoyageSchema.JV_OH_Line, CarrierList);
				lineFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|Line", "Carrier");
				lineFilter.SubGroup = JobSailingFilterProcessor;
			}
		}

		void AddLocationFilter(ModuleFilterCollection filters)
		{
			if (ReplaceFiltersRequiringAuthorisation)
			{
				var loadPortFilter = filters.AddTextFilter("Load Port", RefUNLOCOSchema.RL_PortName);
				loadPortFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|LoadPort", "Load Port");
				loadPortFilter.SubGroup = LoadPortFilterProcessor;
				var dischargePortFilter = filters.AddTextFilter("Discharge Port", RefUNLOCOSchema.RL_PortName);
				dischargePortFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|DischargePort", "Discharge Port");
				dischargePortFilter.SubGroup = DischargePortFilterProcessor;
			}
			else
			{
				ModuleLocationFilter loadDischargeFilter = filters.AddLocationFilter(LocationFilterTypes.LoadDischarge, GetLocationFilter, LocationList, LocationList);
				loadDischargeFilter.SetItemDescriptions(Res.GetData("Freight|JobSailingFilter|Load", "Load"), Res.GetData("Freight|JobSailingFilter|Discharge", "Discharge"));
				loadDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|LoadDischarge", "Load / Discharge");
			}
		}

		void AddStatusFlagFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddTextFilter("Status", GetShipStatusFilter, StatusFilterList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|Status", "Status");

			filters.AddFlagsFilter(FlagsFilterTypes.ShowUnpublished, new string[] { Res.GetString("Freight|JobSailingFilter|ShowUnpublished", "Show Unpublished") }, new GetFlagsQuery[] { GetExcludedSailingsFilter }).MultilingualDescription = ResString.GetMultilingualString("Freight|JobSailingFilter|ShowUnpublished", "Show Unpublished");
			if (Globals.IsWeb)
			{
				filters[FlagsFilterTypes.ShowUnpublished].Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			}
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter serviceStringFilter = filters.AddTextFilter(ServiceStringLabel, JobSailingSchema.JX_ServiceString);
			serviceStringFilter.WithMaxLengthOf<ModuleTextFilter>(JobSailingSchema.JX_ServiceString);
			serviceStringFilter.MultilingualDescription = ServiceStringCaption;
			serviceStringFilter.Category = FilterCategories.NumbersAndReferences;
		}

		#endregion

		#region Delegates

		ZQuery GetHiddenFilter(ZBool ignoredFlag)
		{
			ZDBOnlySubQuery voyageFilter = CreateVoyageSubQuery(JobVoyOriginSchema.JA_JV);
			voyageFilter.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, TransportMode);

			ZDBOnlySubQuery originFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			originFilter.AddSubQuery(voyageFilter, JoinCondition.And);

			ZDBOnlyQuery sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));
			sailingFilter.AddSubQuery(originFilter, JoinCondition.And);

			return sailingFilter;
		}

		ZQuery GetShipStatusFilter(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobSailing));

			if (value == "CURRENT")
			{
				ZDBOnlySubQuery originQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
				originQuery.AddToFilter(JobVoyOriginSchema.JA_E_DEP, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Today);

				ZDBOnlySubQuery destinationQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
				destinationQuery.AddToFilter(JobVoyDestinationSchema.JB_E_ARV, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Today);

				query.AddSubQuery(originQuery, JoinCondition.And);
				query.AddSubQuery(destinationQuery, JoinCondition.Or);
			}
			else if (value == "ARRIVED")
			{
				ZDBOnlySubQuery originQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
				originQuery.AddToFilter(JobVoyOriginSchema.JA_E_DEP, SQLComparisonOperator.LessThan, ZDateTime.Today);

				ZDBOnlySubQuery destinationQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
				destinationQuery.AddToFilter(JobVoyDestinationSchema.JB_E_ARV, SQLComparisonOperator.LessThan, ZDateTime.Today);

				query.AddSubQuery(originQuery, JoinCondition.And);
				query.AddSubQuery(destinationQuery, JoinCondition.And);
			}
			return query;
		}

		ZQuery GetCharterFilter(ZString value)
		{
			ZQuery result = new ZQuery();

			if (value != FreightConstants.CharterFilter.All)
			{
				ZDBOnlySubQuery voyageFilter = CreateVoyageSubQuery(JobVoyageSchema.PK);

				switch (value)
				{
					case FreightConstants.CharterFilter.CharterOnlyCode:
						voyageFilter.AddToFilter(JobVoyageSchema.JV_IsChartered, true);
						break;

					case FreightConstants.CharterFilter.NonCharterOnlyCode:
						voyageFilter.AddToFilter(JobVoyageSchema.JV_IsChartered, false);
						break;
				}

				ZDBOnlySubQuery originFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.PK);
				originFilter.AddSubQuery(JobVoyOriginSchema.JA_JV, voyageFilter, JoinCondition.And);

				ZDBOnlyQuery sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));
				sailingFilter.AddSubQuery(JobSailingSchema.JX_JA, originFilter, JoinCondition.And);

				result.AddToFilter(sailingFilter);
			}

			return result;
		}

		ZQuery GetSailingDateTimeLoadETAFilter(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetDateTimeFilter(comparisonOperator, date1, date2, JobSailingSchema.JX_JA, JobVoyOriginSchema.JA_E_ARV);
		}

		ZQuery GetSailingDateTimeLoadATAFilter(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetDateTimeFilter(comparisonOperator, date1, date2, JobSailingSchema.JX_JA, JobVoyOriginSchema.JA_A_ARV);
		}

		ZQuery GetSailingDateTimeLoadETDFilter(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetDateTimeFilter(comparisonOperator, date1, date2, JobSailingSchema.JX_JA, JobVoyOriginSchema.JA_E_DEP);
		}

		ZQuery GetSailingDateTimeLoadATDFilter(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetDateTimeFilter(comparisonOperator, date1, date2, JobSailingSchema.JX_JA, JobVoyOriginSchema.JA_A_DEP);
		}

		ZQuery GetSailingDateTimeDischargeETAFilter(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetDateTimeFilter(comparisonOperator, date1, date2, JobSailingSchema.JX_JB, JobVoyDestinationSchema.JB_E_ARV);
		}

		ZQuery GetSailingDateTimeDischargeATAFilter(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetDateTimeFilter(comparisonOperator, date1, date2, JobSailingSchema.JX_JB, JobVoyDestinationSchema.JB_A_ARV);
		}

		ZQuery GetSailingDateTimeDischargeSTAFilter(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetDateTimeFilter(comparisonOperator, date1, date2, JobSailingSchema.JX_JB, JobVoyDestinationSchema.JB_S_ARV);
		}

		ZQuery GetSailingDateTimeLoadSTDFilter(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetDateTimeFilter(comparisonOperator, date1, date2, JobSailingSchema.JX_JA, JobVoyOriginSchema.JA_S_DEP);
		}

		ZQuery GetSailingDateTimeLoadSTAFilter(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetDateTimeFilter(comparisonOperator, date1, date2, JobSailingSchema.JX_JA, JobVoyOriginSchema.JA_S_ARV);
		}

		ZQuery GetDateTimeFilter(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2, SchemaGuidColumn sailingJoinColumn, SchemaDateTimeColumn dateColumn)
		{
			Type originOrDestination = sailingJoinColumn == JobSailingSchema.JX_JA ? typeof(VoyageOrigin) : typeof(VoyageDestination);
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobSailing));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(originOrDestination, sailingJoinColumn);
			AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, dateColumn, date1, date2, false, true);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetLineFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var jobVoyage = new ZDBOnlyQuery(typeof(JobVoyage));

			if (!value.IsEmpty)
			{
				ZDBOnlySubQuery orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), JobVoyageSchema.JV_OH_Line);
				orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, value);

				jobVoyage.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
			}

			return jobVoyage;
		}

		ZQuery GetLocationFilter(ZString value1, ZString value2)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobSailing));

			if (!value1.IsEmpty)
			{
				if (value1.Length == 2)
				{
					query.AddFilterAndZSQLParameterCollection(JobSailing.Schema.JX_JA + " IN (SELECT " + VoyageOrigin.Schema.PK +
						" FROM " + VoyageOrigin.Schema.TableName + " WHERE " + VoyageOrigin.Schema.JA_RL_NKPortOfLoading + " IN ( " +
						"SELECT " + RefUNLOCO.Schema.RL_Code + " FROM " + RefUNLOCOSchema.Constants.SqlSchemaName + "." + RefUNLOCOSchema.Constants.TableName + " WHERE " + RefUNLOCO.Schema.RL_RN_NKCountryCode + " = @CountryCode1 ))", new ZSqlParameterCollection(ZSqlParameter.New("@CountryCode1", value1, RefCountrySchema.RN_Code)));
				}
				else
				{
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
					subQuery.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, value1);
					query.AddSubQuery(subQuery, JoinCondition.And);
				}
			}

			if (!value2.IsEmpty)
			{
				if (value2.Length == 2)
				{
					query.AddFilterAndZSQLParameterCollection(JobSailing.Schema.JX_JB + " IN (SELECT " + VoyageDestination.Schema.PK +
						" FROM " + VoyageDestination.Schema.TableName + " WHERE " + VoyageDestination.Schema.JB_RL_NKPortOfDischarge + " IN ( " +
						"SELECT " + RefUNLOCO.Schema.RL_Code + " FROM " + RefUNLOCOSchema.Constants.SqlSchemaName + "." + RefUNLOCOSchema.Constants.TableName + " WHERE " + RefUNLOCO.Schema.RL_RN_NKCountryCode + " = @CountryCode2 ))", new ZSqlParameterCollection(ZSqlParameter.New("@CountryCode2", value2, RefCountrySchema.RN_Code)));
				}
				else
				{
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
					subQuery.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, value2);
					query.AddSubQuery(subQuery, JoinCondition.And);
				}
			}

			return query;
		}

		ZQuery GetExcludedSailingsFilter(ZBool value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobSailing));
			if (!value || Globals.IsWeb)
			{
				query.AddToFilter(JobSailingSchema.JX_IsPublished, SQLComparisonOperator.NotEqual, false);
			}

			return query;
		}

		#endregion

		#region Lists

		#region StatusFilterList

		CodeDescriptionPairList StatusFilterList
		{
			get
			{
				if (fStatusTypeList == null)
				{
					fStatusTypeList = new CodeDescriptionPairList();
					fStatusTypeList.AddPair(StatusFilterTypes.All, Res.GetString("JobSailingFilter|StatusFilterList|All", "ALL"));
					fStatusTypeList.AddPair(StatusFilterTypes.Current, Res.GetString("JobSailingFilter|StatusFilterList|Current", "CURRENT"));
					fStatusTypeList.AddPair(StatusFilterTypes.Arrived, Res.GetString("JobSailingFilter|StatusFilterList|Arrived", "ARRIVED"));
				}

				return fStatusTypeList;
			}
		}

		CodeDescriptionPairList fStatusTypeList;

		#endregion

		#region CharterFilterList

		CodeDescriptionPairList CharterFilterList
		{
			get
			{
				if (fCharterTypeList == null)
				{
					fCharterTypeList = new CodeDescriptionPairList();
					fCharterTypeList.AddPair(CharterFilterTypes.All, Res.GetString("Freight|JobSailingFilter|All", "ALL"));
					fCharterTypeList.AddPair(CharterFilterTypes.CHA, Res.GetString("Freight|JobSailingFilter|Charter", "Charter"));
					fCharterTypeList.AddPair(CharterFilterTypes.NCH, Res.GetString("Freight|JobSailingFilter|NonCharter", "Non Charter"));
				}

				return fCharterTypeList;
			}
		}

		CodeDescriptionPairList fCharterTypeList;

		#endregion

		#region  CarrierList

		OrgHeaderCollection CarrierList
		{
			get { return carrierList ?? (carrierList = GetCarrierList()); }
		}

		protected virtual OrgHeaderCollection GetCarrierList()
		{
			return new OrgHeaderCollection(Factory);
		}

		OrgHeaderCollection carrierList;

		#endregion

		#region LocationList

		LocationCollection LocationList
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}

		#endregion

		#endregion

		#region Filter Types

		protected internal virtual string BookingRefLabel
		{
			get { return (NoResString)"Booking Ref."; }
		}

		protected internal virtual MultilingualString BookingRefCaption
		{
			get { return ResString.GetMultilingualString("Freight|JobSailingFilter|BookingRef", "Booking Ref."); }
		}

		protected internal virtual string ReferenceNumberLabel
		{
			get { return (NoResString)"Reference Number"; }
		}

		protected internal virtual MultilingualString ReferenceNumberCaption
		{
			get { return ResString.GetMultilingualString("Freight|JobSailingFilter|ReferenceNumber", "Reference Number"); }
		}

		protected internal string ServiceStringLabel
		{
			get { return (NoResString)"Service String"; }
		}

		protected internal MultilingualString ServiceStringCaption
		{
			get { return ResString.GetMultilingualString("Freight|JobSailingFilter|ServiceString", "Service String"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		static public class OrgsFilterTypes
		{
			public const string Line = "Line";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		static public class LocationFilterTypes
		{
			public const string LoadDischarge = "Load / Discharge";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		static public class DateFilterTypes
		{
			public const string LoadPortETA = "Load Port ETA";
			public const string LoadPortATA = "Load Port ATA";
			public const string LoadPortSTA = "Load Port STA";
			public const string LoadPortETD = "Load Port ETD";
			public const string LoadPortATD = "Load Port ATD";
			public const string LoadPortSTD = "Load Port STD";
			public const string DischargePortETA = "Discharge Port ETA";
			public const string DischargePortATA = "Discharge Port ATA";
			public const string DischargePortSTA = "Discharge Port STA";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		static public class OrgFilterTypes
		{
			public const string Line = "Line";
		}

		static public class StatusFilterTypes
		{
			public const string All = "ALL";
			public const string Arrived = "ARRIVED";
			public const string Current = "CURRENT";
		}

		static public class CharterFilterTypes
		{
			public const string All = "ALL";
			public const string CHA = "CHA";
			public const string NCH = "NCH";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		static public class FlagsFilterTypes
		{
			public const string ShowUnpublished = "Show Unpublished";
		}

		#endregion

		protected abstract ZString TransportMode { get; }
	}
}
