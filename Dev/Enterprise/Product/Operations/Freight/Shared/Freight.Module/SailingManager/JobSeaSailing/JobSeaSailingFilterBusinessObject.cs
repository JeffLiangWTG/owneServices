using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module
{
	public class JobSeaSailingFilterBusinessObject : JobSailingFilterBusinessObject
	{
		static public class SeaFilterTypes
		{
			public static readonly string VesselType = (NoResString)"Voyage # and Vessel";
			public static readonly string TradeLane = (NoResString)"Trade Lane";
			public static readonly string StowPlanStatus = (NoResString)"AMS Stow Plan Status";
			public static readonly string HasStowPlan = (NoResString)"Has Stow Plan";
			public static readonly string IncludeArchived = (NoResString)"Include Archived";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var voyageVesselFilter = new VoyageVesselModuleFilter(SeaFilterTypes.VesselType, GetVoyageNumberAndVesselFilter, VesselList)
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			voyageVesselFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|LocalCartageJobTypeFilter|VoyageAndVessel", "Voyage # and Vessel");
			voyageVesselFilter.Category = FilterCategories.NumbersAndReferences;
			voyageVesselFilter.SupportsBlankComparisonOperators = false;
			filters.AddCustomFilter(voyageVesselFilter);

			ModuleGuidFilter tradeLaneFilter = filters.AddGuidFilter(SeaFilterTypes.TradeLane, ModuleIDs.TradeLane, GetSailingTradeLaneFilter, TradeLaneList);
			tradeLaneFilter.Category = FilterCategories.Locations;
			tradeLaneFilter.IsPublishedOnWeb = false;
			tradeLaneFilter.SubGroup = JobSailingFilterProcessor;
			tradeLaneFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|LocalCartageJobTypeFilter|TradeLane", "Trade Lane");

			var stwMessageStatusList = Factory.GetCachedValue<CodeDescriptionPairList>("MessageStatusListSTWWithoutNotSent", delegate
				{
					var result = new MessageStatusListSTW();
					result.RemoveCode(MessageStatusListSTW.Codes.NotSent);
					return result;
				});
			var stowPlanStatusFilter = filters.AddTextFilter(SeaFilterTypes.StowPlanStatus, GetAMSStowPlanStatusQuery, stwMessageStatusList);
			stowPlanStatusFilter.Category = FilterCategories.StatusAndFlags;
			stowPlanStatusFilter.IsPublishedOnWeb = false;
			stowPlanStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|LocalCartageJobTypeFilter|AMSStowPlanStatus", "AMS Stow Plan Status");

			var hasStowPlanFilter = filters.AddTextFilter(SeaFilterTypes.HasStowPlan, GetHasStowPlanQuery, HasStowPlanFilterList);
			hasStowPlanFilter.Category = FilterCategories.StatusAndFlags;
			hasStowPlanFilter.IsPublishedOnWeb = false;
			hasStowPlanFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|LocalCartageJobTypeFilter|HasStowPlan", "Has Stow Plan");

			var includeArchivedFilter = filters.AddFlagsFilter(SeaFilterTypes.IncludeArchived,
				new[] { Res.GetString("Freight|LocalCartageJobTypeFilter|IncludeArchived", "Include Archived") },
				new GetFlagsQuery[] { GetIncludeArchivedQuery });
			includeArchivedFilter.Category = FilterCategories.StatusAndFlags;
			includeArchivedFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|LocalCartageJobTypeFilter|IncludeArchived", "Include Archived");
			includeArchivedFilter.Visibility = FilterVisibility.AlwaysApplied;

			return filters;
		}

		ZQuery GetHasStowPlanQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(EDIMessage));

			if (value == HasStowPlanFilterOptions.All)
			{
				return result;
			}

			var stowPlanQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_LinkUniqueID, value == HasStowPlanFilterOptions.DoesNotHaveStowPlanFilter);
			stowPlanQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.StowPlan);
			result.AddSubQuery(JobSailingSchema.JX_JA, stowPlanQuery, value == HasStowPlanFilterOptions.DoesNotHaveStowPlanFilter ? JoinCondition.And : JoinCondition.Or);
			result.AddSubQuery(JobSailingSchema.JX_JB, stowPlanQuery, value == HasStowPlanFilterOptions.DoesNotHaveStowPlanFilter ? JoinCondition.And : JoinCondition.Or);

			return result;
		}

		ZQuery GetIncludeArchivedQuery(ZBool value)
		{
			var result = new ZDBOnlyQuery(typeof(JobSailing));

			if (!value)
			{
				AddVoyageSubQuery(result, JobVoyageSchema.JV_IsActive, SQLComparisonOperator.Equal, ZBool.True);
			}

			return result;
		}

		ZQuery GetAMSStowPlanStatusQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobSailing));
			if (!value.IsEmpty)
			{
				var statusQuery = new ZDBOnlySubQuery(typeof(GenCustomAddOnValue), GenCustomAddOnValueSchema.XV_ParentID);
				statusQuery.AddToFilter(GenCustomAddOnValueSchema.XV_Data, value);
				statusQuery.AddToFilter(GenCustomAddOnValueSchema.XV_Name, VoyageOrigin.Schema.StowPlanMessageStatus);
				result.AddSubQuery(JobSailingSchema.JX_JA, statusQuery, JoinCondition.Or);
				result.AddSubQuery(JobSailingSchema.JX_JB, statusQuery, JoinCondition.Or);
			}

			return result;
		}

		ZQuery GetSailingTradeLaneFilter(ZGuid value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobVoyage));

			ZDBOnlySubQuery tradeLaneVoyageQuery = new ZDBOnlySubQuery(typeof(JobTradeLaneVoyage), JobTradeLaneVoyageSchema.NB_JV);
			tradeLaneVoyageQuery.AddToFilter(JobTradeLaneVoyageSchema.NB_EJ, value);
			query.AddSubQuery(tradeLaneVoyageQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetVoyageNumberAndVesselFilter(SQLComparisonOperator textValueComparisonOperator, ZString voyageNumber, ZString nKVessel, ZBool includeArchived)
		{
			var query = new ZDBOnlyQuery(typeof(JobSailing));

			if (!nKVessel.IsEmpty || textValueComparisonOperator == SpecialComparisonOperator.IsBlank || textValueComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				AddVoyageSubQuery(query, JobVoyageSchema.JV_RV_NKVessel, textValueComparisonOperator, nKVessel);
			}

			if (!voyageNumber.IsEmpty || textValueComparisonOperator == SpecialComparisonOperator.IsBlank || textValueComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				AddVoyageSubQuery(query, JobVoyageSchema.JV_VoyageFlight, textValueComparisonOperator, voyageNumber);
			}

			return query;
		}

		#region Lookups

		RefVesselCollection VesselList
		{
			get { return vesselList ?? (vesselList = new RefVesselCollection(Factory)); }
		}
		RefVesselCollection vesselList;

		JobTradeLaneCollection TradeLaneList
		{
			get { return tradeLaneList ?? (tradeLaneList = new JobTradeLaneCollection(Factory)); }
		}
		JobTradeLaneCollection tradeLaneList;

		CodeDescriptionPairList HasStowPlanFilterList
		{
			get
			{
				if (hasStowPlanFilterList == null)
				{
					hasStowPlanFilterList = new CodeDescriptionPairList();
					hasStowPlanFilterList.AddPair(HasStowPlanFilterOptions.All, Res.GetString("8316bd88-0784-e2ba-47d6-cb8de96c8a8d", "All"));
					hasStowPlanFilterList.AddPair(HasStowPlanFilterOptions.HasStowPlanFilter, Res.GetString("dcabd972-d63e-48b2-4bd6-6cb514105899", "Has Stow Plan"));
					hasStowPlanFilterList.AddPair(HasStowPlanFilterOptions.DoesNotHaveStowPlanFilter, Res.GetString("9c49e1da-3124-5fa3-4fd1-f41f1d0d3d8f", "Does not have Stow Plan"));
				}

				return hasStowPlanFilterList;
			}
		}
		CodeDescriptionPairList hasStowPlanFilterList;

		protected override OrgHeaderCollection GetCarrierList()
		{
			return new SeaShippingProviderCollection(Factory);
		}

		#endregion

		#region Implementation

		void AddVoyageSubQuery(ZDBOnlyQuery sailingQuery, SchemaColumn column, SQLComparisonOperator comparisonOperator, IZType value)
		{
			var voyageQuery = CreateVoyageSubQuery(JobVoyOriginSchema.JA_JV);
			voyageQuery.AddToFilter(column, comparisonOperator, value);

			var originQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			originQuery.AddSubQuery(voyageQuery, JoinCondition.And);

			sailingQuery.AddSubQuery(originQuery, JoinCondition.And);
		}

		protected override ZString TransportMode
		{
			get { return Constants.TransportModes.Sea; }
		}

		#endregion

		#region HasStowPlanFilterList

		class HasStowPlanFilterOptions
		{
			public const string All = "ALL";
			public const string HasStowPlanFilter = "HAS";
			public const string DoesNotHaveStowPlanFilter = "NOT";
		}

		#endregion
	}
}
