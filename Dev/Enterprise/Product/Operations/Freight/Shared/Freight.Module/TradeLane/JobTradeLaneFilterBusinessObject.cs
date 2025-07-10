using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module
{
	public class JobTradeLaneFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Descriptions
		{
			public static readonly string TradeLaneCode = (NoResString)"Trade Lane Code";
			public static readonly string TradeLaneDescription = (NoResString)"Trade Lane Description";
			public static readonly string TradeLaneDirectionType = (NoResString)"Trade Lane Direction Types";
			public static readonly string TradeLaneLocations = (NoResString)"Trade Lane Locations";
			public static readonly string Principal = (NoResString)"Principal";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			ModuleFilter filter = filters.AddTextFilter(Descriptions.TradeLaneCode, JobTradeLaneSchema.EJ_Code);
			filter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobTradeLaneFilter|TradeLaneCode", "Trade Lane Code");

			filter = filters.AddTextFilter(Descriptions.TradeLaneDescription, JobTradeLaneSchema.EJ_Description);
			filter.Category = FilterCategories.TextSearch;
			filter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobTradeLaneFilter|TradeLaneDescription", "Trade Lane Description");

			filter = filters.AddTextFilter(Descriptions.TradeLaneDirectionType, JobTradeLaneSchema.EJ_Direction, new DirectionTypeList());
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobTradeLaneFilter|TradeLaneDirectionTypes", "Trade Lane Direction Types");

			ModuleLocationFilter locationFilter = filters.AddLocationFilter(Descriptions.TradeLaneLocations, JobTradeLaneSchema.EJ_Location1, LocationList, JobTradeLaneSchema.EJ_Location2, LocationList);
			locationFilter.Category = FilterCategories.Locations;
			locationFilter.SetItemDescriptions(Res.GetData("Freight|JobTradeLaneFilter|Location1", "Location 1"), Res.GetData("Freight|JobTradeLaneFilter|Location2", "Location 2"));
			locationFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobTradeLaneFilter|TradeLaneLocations", "Trade Lane Locations");

			filter = filters.AddGuidFilter(Descriptions.Principal, ModuleIDs.Organisation, JobTradeLaneSchema.EJ_OH_RelatedOrg, PrincipalList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobTradeLaneFilter|Principal", "Principal");

			return filters;
		}

		#region Lists

		#region LocationList

		LocationCollection LocationList
		{
			get
			{
				if (locationList == null)
				{
					locationList = new LocationCollection(Factory);
				}
				return locationList;
			}
		}
		LocationCollection locationList;
		#endregion

		#region PrincipalList

		ShipsAgencyPrincipalCollection PrincipalList
		{
			get
			{
				if (principalList == null)
				{
					principalList = new ShipsAgencyPrincipalCollection(Factory);
				}

				return principalList;
			}
		}
		ShipsAgencyPrincipalCollection principalList;

		#endregion

		#endregion

	}
}
