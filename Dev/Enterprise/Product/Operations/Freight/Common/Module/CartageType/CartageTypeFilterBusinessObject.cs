using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Common.Module
{
	public class CartageTypeFilterBusinessObject : FilterStripBusinessObject
	{
		public CartageTypeFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddTextFilters(result);
			AddFlagsFilters(result);

			return result;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			//Don't Show OLD JobTypes
			ModuleTextFilter oldJobTypes = filters.AddTextFilter("OLD Code", LocalCartageJobTypeSchema.E3_JobType);
			oldJobTypes.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			oldJobTypes.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			oldJobTypes.DefaultProperty = "";

			filters.AddTextFilter("Code", LocalCartageJobTypeSchema.E3_JobType).MultilingualDescription = ResString.GetMultilingualString("Freight|CartageTypeFilter|Code", "Code");
			filters.AddFiltersForTranslatableText("Description", LocalCartageJobTypeSchema.E3_Description, typeof(CommonCartageType), ResString.GetMultilingualString("Freight|CartageTypeFilter|Description", "Description"));

			filters.AddTextFilter("Transport Mode", LocalCartageJobTypeSchema.E3_ShippingTransportMode, ShippingTransportModes).MultilingualDescription = ResString.GetMultilingualString("Freight|CartageTypeFilter|TransportMode", "Transport Mode");
			filters.AddTextFilter("Organisation", GetOrganisationFilter, OrgTypes).MultilingualDescription = ResString.GetMultilingualString("Freight|CartageTypeFilter|Organisation", "Organization");
		}

		#endregion

		#region Flags

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter hiddenFilter = filters.AddTextFilter("Hidden Status", GetHiddenStatusFilter, HiddenStatusList);
			hiddenFilter.Category = FilterCategories.StatusAndFlags;
			hiddenFilter.DefaultProperty = HiddenStatusCodes.Hidden;
			hiddenFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|CartageTypeFilterBusinessObject|HiddenStatus", "Hidden Status");
		}

		ZQuery GetHiddenStatusFilter(ZString value)
		{
			ZQuery query = new ZQuery();

			if (value == HiddenStatusCodes.Hidden)
			{
				query.AddToFilter(LocalCartageJobTypeSchema.E3_IsHidden, ZBool.True);
			}
			else if (value == HiddenStatusCodes.NonHidden)
			{
				query.AddToFilter(LocalCartageJobTypeSchema.E3_IsHidden, ZBool.False);
			}

			return query;
		}

		#endregion

		#endregion

		#region Filter Overrides

		protected ZQuery GetOrganisationFilter(ZString value)
		{
			if (!value.IsEmpty)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonCartageType));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CommonCartageOrg), LocalCartageJobOrgSchema.E5_E3);
				subQuery.AddToFilter(LocalCartageJobOrgSchema.E5_OrgType, value);
				result.AddSubQuery(subQuery, JoinCondition.And);
				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		#endregion

		#region Lookups

		#region Lists

		public CodeDescriptionPairList ShippingTransportModes
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.LocalCartageTransportModes); }
		}

		public LocalCartageJobOrgTypeList OrgTypes
		{
			get { return LocalCartageJobOrgTypeList.Instance; }
		}

		//public CodeDescriptionPairList LegTypes
		//{
		//    get { return new AllLegsHelper(Factory).LegTypeList; }
		//}

		#endregion

		#region CheckBoxLists

		#region HiddenList

		CodeDescriptionPairList HiddenStatusList
		{
			get
			{
				if (hE3StatusList == null)
				{
					hE3StatusList = new CodeDescriptionPairList();
					hE3StatusList.AddPair(HiddenStatusCodes.All, Res.GetString("Freight|CartageTypeFilter|HiddenStatusList|All", "Both Hidden and Non-Hidden"));
					hE3StatusList.AddPair(HiddenStatusCodes.Hidden, Res.GetString("Freight|CartageTypeFilter|HiddenStatusList|Hidden", "Hidden Only"));
					hE3StatusList.AddPair(HiddenStatusCodes.NonHidden, Res.GetString("Freight|CartageTypeFilter|HiddenStatusList|NonHidden", "Non-Hidden Only"));
				}
				return hE3StatusList;
			}
		}

		CodeDescriptionPairList hE3StatusList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
		class HiddenStatusCodes
		{
			public const string All = "All";
			public const string Hidden = "Is Hidden";
			public const string NonHidden = "Is Not Hidden";
		}

		#endregion

		#endregion

		#endregion
	}
}
