using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.MasterFiles.Module
{
	public class GlbDepartmentFilterBusinessObject : FilterStripBusinessObject
	{
		ModuleTextFilter filterMode;
		ModuleTextFilter filterActivity;
		ModuleTextFilter filterDirection;

		public GlbDepartmentFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddTextFilters(result);

			return result;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			//NOTE: DepartmentSubGroup cannot be re-used because it adds its filter at two different locations.
			//If AND'd together, splitting and re-combining filters in this way changes the logic.

			filters.AddFiltersForTranslatableText("Description", GlbDepartmentSchema.GE_Desc, typeof(GlbDepartment), ResString.GetMultilingualString("MasterFiles|GlbDepartmentFilter|Description", "Description"));
			filters.AddTextFilter("Code", GlbDepartmentSchema.GE_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbDepartmentFilter|Code", "Code");

			if (!DataRegistry.Instance.ProductivityWiseModeEnabled)
			{
				AddLogisticsFilters(filters);
			}
		}

		void AddLogisticsFilters(ModuleFilterCollection filters)
		{
			filterActivity = filters.AddTextFilter("Activity", GetGlbDepartmentActivityFilter, ActivityList);
			filterActivity.Category = FilterCategories.ModesAndTypes;
			filterActivity.MultilingualDescription = ActivityDescription;
			filterActivity.PropertyValidation = ActivityValidation;
			filterActivity.SubGroup = new DepartmentSubGroup();

			filterDirection = filters.AddTextFilter("Direction", GetGlbDepartmentDirectionFilter, DirectionList);
			filterDirection.Category = FilterCategories.ModesAndTypes;
			filterDirection.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbDepartmentFilter|Direction", "Direction");
			filterDirection.SubGroup = new DepartmentSubGroup();

			filterMode = filters.AddTextFilter("Mode", GetGlbDepartmentModeFilter, GetModeList);
			filterMode.Category = FilterCategories.ModesAndTypes;
			filterMode.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbDepartmentFilter|Mode", "Mode");
			filterMode.PropertyValidation = ModeValidation;
			filterMode.SubGroup = new DepartmentSubGroup();
		}

		MultilingualString ActivityDescription => ResString.GetMultilingualString("MasterFiles|GlbDepartmentFilter|Activity", "Activity");

		void ActivityValidation(ZPropertyInfo info)
		{
			filterMode.Validation.ValidateProperty();
		}

		void ModeValidation(ZPropertyInfo info)
		{
			if (Activity.IsEmpty)
			{
				info.AddWarning(Res.GetString("247C13B8-D3AB-48AA-A76F-9BE45C501A54", "Mode list will be populated once an Activity filter has been chosen"));
			}
		}

		#region Queries

		class DepartmentSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbDepartment));
				query.AddToFilter(GlbDepartmentSchema.GE_SystemCode, true);

				query.AddToFilter(filter);

				var subQuery = new ZDBOnlySubQuery(typeof(GlbDepartment), GlbDepartmentSchema.PK);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(GlbDepartmentSchema.GE_GE, subQuery, JoinCondition.Or);

				return query;
			}
		}

		ZQuery GetGlbDepartmentActivityFilter(ZString value)
		{
			var query = new ZQuery();
			var sql = string.Empty;

			if (value != "M")
			{
				query.AddToFilter(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.StartsWith, value.SubstringSafe(0, GlbDepartmentSchema.GE_Code.MaxLength));
			}
			else
			{
				sql = string.Format(CultureInfo.InvariantCulture,
									@"SUBSTRING({0}, 1, 1) not in ({1})",
									GlbDepartmentSchema.Constants.GE_Code,
									ActivityListMiscellaneousFilter);

				query.AddFilterAndZSQLParameterCollection(sql, null, JoinCondition.And);
			}
			return query;
		}

		ZQuery GetGlbDepartmentModeFilter(ZString value)
		{
			var query = new ZQuery();
			var modeListOtherFilter = string.Empty;

			if (value != "O")
			{
				query.AddToFilter(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.EndsWith, value.SubstringSafe(0, GlbDepartmentSchema.GE_Code.MaxLength));
			}
			else
			{
				modeListOtherFilter = ModeListOtherFilter(GlbDepartmentSchema.Constants.GE_Code);
				query.AddFilterAndZSQLParameterCollection(modeListOtherFilter, null, JoinCondition.And);
			}
			return query;
		}

		ZQuery GetGlbDepartmentDirectionFilter(ZString value)
		{
			var query = new ZQuery();

			string sql;

			if (value != "O")
			{
				sql = string.Format(@"SUBSTRING({0}, 2, 1) = '{1}'",
						  GlbDepartmentSchema.Constants.GE_Code,
						  value);
			}
			else
			{
				sql = string.Format(@"SUBSTRING({0}, 2, 1) not in ({1})",
						  GlbDepartmentSchema.Constants.GE_Code,
						  DirectionListOtherFilter);
			}

			query.AddFilterAndZSQLParameterCollection(sql, null, JoinCondition.And);
			return query;
		}

		#endregion

		#endregion

		#endregion

		#region Index Search Filters

		protected override SearchField ResolveSearchField(SearchField searchField)
		{
			if (searchField.FieldName.Equals(ActivityFieldName, StringComparison.OrdinalIgnoreCase))
			{
				return AddActivityFilter(searchField);
			}
			return base.ResolveSearchField(searchField);
		}

		SearchField AddActivityFilter(SearchField searchField)
		{
			if (!DataRegistry.Instance.ProductivityWiseModeEnabled)
			{
				var activityFilter = new IndexSearchModuleTextFilter("Activity", searchField, GetActivityGlowQuery, ActivityList, FilterCategories.ModesAndTypes);
				activityFilter.MultilingualDescription = ActivityDescription;
				return new SearchFieldOverride(searchField, activityFilter);
			}

			return null;
		}

		IGlowQuery GetActivityGlowQuery(SearchField searchField, ZString activity)
		{
			IGlowQuery activityQuery1;
			IGlowQuery activityQuery2;

			if (activity != "M")
			{
				activityQuery1 = new PrefixQuery(new Term(ActivityFieldName, activity));
				activityQuery2 = new PrefixQuery(new Term(ParentCodeFieldName, activity));
			}
			else
			{
				var codeList = ActivityList.ToArray().Where(o => o.Code != "M");
				activityQuery1 = new BooleanQuery(BooleanOperator.And, new BooleanQuery(BooleanOperator.And, codeList.Select(o => new NotQuery(new PrefixQuery(new Term(ActivityFieldName, o.Code)))).ToArray()), new IsNotBlankQuery(new Term(ActivityFieldName, string.Empty)));
				activityQuery2 = new BooleanQuery(BooleanOperator.And, new BooleanQuery(BooleanOperator.And, codeList.Select(o => new NotQuery(new PrefixQuery(new Term(ParentCodeFieldName, o.Code)))).ToArray()), new IsNotBlankQuery(new Term(ParentCodeFieldName, string.Empty)));
			}

			return new BooleanQuery(BooleanOperator.Or, activityQuery1, activityQuery2);
		}

		internal string ActivityFieldName => IndexSearchFilterHelper.DefaultHiddenPrefix + (NoResString)"Activity";
		internal string ParentCodeFieldName => IndexSearchFilterHelper.DefaultHiddenPrefix + "ParentCode";

		#endregion

		#region Lists

		#region Activity

		public CodeDescriptionPairList ActivityList
		{
			get
			{
				if (fActivityList == null)
				{
					fActivityList = new GlbDepartmentLookups(null).ActivityList;
				}

				return fActivityList;
			}
		}

		public ZString Activity
		{
			get { return filterActivity.IsActive ? filterActivity.Property : ZString.Empty; }
		}
		CodeDescriptionPairList fActivityList;

		string ActivityListMiscellaneousFilter
		{
			get
			{
				if (string.IsNullOrEmpty(activityListMiscellaneousFilter))
				{
					activityListMiscellaneousFilter = string.Join(",", ActivityList.ToArray().Where(o => o.Code != "M").Select(o => "'" + o.Code + "'"));
				}
				return activityListMiscellaneousFilter;
			}
		}
		string activityListMiscellaneousFilter;

		#endregion

		#region Direction

		public CodeDescriptionPairList DirectionList
		{
			get
			{
				if (fDirectionList == null)
				{
					fDirectionList = new GlbDepartmentLookups(null).DirectionList;
				}
				return fDirectionList;
			}
		}
		CodeDescriptionPairList fDirectionList;

		string DirectionListOtherFilter
		{
			get
			{
				if (string.IsNullOrEmpty(directionListOtherFilter))
				{
					directionListOtherFilter = string.Join(",", DirectionList.ToArray().Where(o => o.Code != "O").Select(o => "'" + o.Code + "'"));
				}
				return directionListOtherFilter;
			}
		}
		string directionListOtherFilter;

		#endregion

		#region Mode

		bool modeLoaded;
		public ICodeDescriptionPairList GetModeList()
		{
			// When the list is loaded for the first time we want it to go through validation to be marked as invalid
			if (Activity.IsEmpty && !modeLoaded)
			{
				modeLoaded = true;
				filterMode.Validation.ValidateProperty();
			}
			return new GlbDepartmentLookups(null).ModeListByActivity(Activity);
		}

		string ModeListOtherFilter(string column)
		{
			var modeList = GetModeList() as CodeDescriptionPairList;
			return string.Join((NoResString)" and ", modeList.ToArray().Where(o => o.Code != "O").Select(o => column + (NoResString)" not like '%" + o.Code + (NoResString)"'"));
		}

		#endregion

		#endregion
	}
}
