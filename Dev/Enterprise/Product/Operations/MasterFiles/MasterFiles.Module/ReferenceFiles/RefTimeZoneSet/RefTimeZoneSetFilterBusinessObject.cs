using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefTimeZoneSetFilterBusinessObject : FilterStripBusinessObject
	{
		public RefTimeZoneSetFilterBusinessObject()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
		internal const string ActiveStatus = "Active Status";

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddRelatedItemFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var refStandardTimeZoneSubGroup = new RefStandardTimeZoneSubGroup();
			var refDaylightSavingTimeZoneSubGroup = new RefDaylightSavingTimeZoneSubGroup();

			filters.AddTextFilter("Time Zone Description", RefTimeZoneSetSchema.R3_TimeZoneSetName).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefTimeZoneSetFilter|TimeZoneDescription", "Time Zone Description");
			var filter = filters.AddTextFilter("Standard Zone Code", RefTimeZoneSchema.R2_CivilianTimeZoneCode);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefTimeZoneSetFilter|StandardZoneCode", "Standard Zone Code");
			filter.SubGroup = refStandardTimeZoneSubGroup;
			filter = filters.AddTextFilter("Standard Zone Name", RefTimeZoneSchema.R2_CivilianTimeZoneFullName);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefTimeZoneSetFilter|StandardZoneName", "Standard Zone Name");
			filter.SubGroup = refStandardTimeZoneSubGroup;
			filter = filters.AddTextFilter("Daylight Saving Zone Code", RefTimeZoneSchema.R2_CivilianTimeZoneCode);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefTimeZoneSetFilter|DaylightSavingZoneCode", "Daylight Saving Zone Code");
			filter.SubGroup = refDaylightSavingTimeZoneSubGroup;
			filter = filters.AddTextFilter("Daylight Saving Zone Name", RefTimeZoneSchema.R2_CivilianTimeZoneFullName);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefTimeZoneSetFilter|DaylightSavingZoneName", "Daylight Saving Zone Name");
			filter.SubGroup = refDaylightSavingTimeZoneSubGroup;
		}

		class RefStandardTimeZoneSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RefTimeZoneSet));
				ZDBOnlySubQuery standardZoneCodeSubquery = new ZDBOnlySubQuery(typeof(RefTimeZone), RefTimeZoneSetSchema.R3_R2_StandardZone);
				standardZoneCodeSubquery.AddToFilter(filter);
				query.AddSubQuery(standardZoneCodeSubquery, JoinCondition.And);

				return query;
			}
		}

		class RefDaylightSavingTimeZoneSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RefTimeZoneSet));
				ZDBOnlySubQuery standardZoneCodeSubquery = new ZDBOnlySubQuery(typeof(RefTimeZone), RefTimeZoneSetSchema.R3_R2_DaylightSavingZone);
				standardZoneCodeSubquery.AddToFilter(filter);
				query.AddSubQuery(standardZoneCodeSubquery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter("Port", ModuleIDs.RefUNLOCO, GetUNLOCOQuery, UNLOCOs);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefTimeZoneSetFilter|Port", "Port");
			filter.SubGroup = new UNLOCOSubGroup();
		}

		class UNLOCOSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RefTimeZoneSet));
				ZDBOnlySubQuery uNLOCOSubQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), RefUNLOCOSchema.RL_R3);
				uNLOCOSubQuery.AddToFilter(filter);

				query.AddSubQuery(uNLOCOSubQuery, JoinCondition.And);
				return query;
			}
		}

		ZQuery GetUNLOCOQuery(ZGuid uNLOCOPK)
		{
			var query = new ZQuery();
			if (!uNLOCOPK.IsEmpty)
			{
				query.AddToFilter(RefUNLOCOSchema.PK, uNLOCOPK);
			}
			return query;
		}

		#endregion

		#endregion

		#region Lookups

		#region UNLOCOs

		RefUNLOCOCollection UNLOCOs
		{
			get
			{
				if (fUNLOCOs == null)
				{
					fUNLOCOs = new RefUNLOCOCollection(Factory);
				}

				return fUNLOCOs;
			}
		}

		RefUNLOCOCollection fUNLOCOs;

		#endregion

		#endregion
	}
}
