using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefFacilityFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddRelatedItemFilters(filters);
			AddFlagsFilters(filters);

			return filters;
		}

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			var activeStatusString = ResString.GetMultilingualString("c1ba4805-782c-4c0a-bd02-b1093a29429b", "Active Status");
			ModuleFilter filter = filters.AddTextFilter(activeStatusString.GetUnresolvedString(), GetActiveStatusQuery, CancelledStatusList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = activeStatusString;

			filter = filters.AddFlagsFilter("Is Sea", new string[] { Res.GetString("4e58a2d5-c8d1-445c-8bfa-ef4b5889bef0", "Show Marked As Sea") },
				new SchemaBoolColumn[] { RefFacilitySchema.RFT_IsSea });
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefFacilityFilter|IsSea", "Is Sea");

			filter = filters.AddFlagsFilter("Is Rail", new string[] { Res.GetString("4e58a2d5-c8d1-428c-8bfa-e54b5889bef0", "Show Marked As Rail") },
				new SchemaBoolColumn[] { RefFacilitySchema.RFT_IsRail });
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefFacilityFilter|IsRail", "Is Rail");

			filter = filters.AddFlagsFilter("Is Road", new string[] { Res.GetString("4e58a2a5-c8d1-428c-8bfa-e54b5889beb0", "Show Marked As Road") },
				new SchemaBoolColumn[] { RefFacilitySchema.RFT_IsRoad });
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefFacilityFilter|IsRoad", "Is Road");

			filter = filters.AddFlagsFilter("Is Air", new string[] { Res.GetString("4a58a2a5-c7d1-328c-8bfa-e54b5889bcb0", "Show Marked As Air") },
				new SchemaBoolColumn[] { RefFacilitySchema.RFT_IsAir });
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefFacilityFilter|IsAir", "Is Air");

			filter = filters.AddFlagsFilter("Is Inland Waterway/Ferry", new string[] { Res.GetString("5a58a2a5-c7d1-327c-8bfa-e54b5859beb1", "Show Marked As Inland Waterway/Ferry") },
				new SchemaBoolColumn[] { RefFacilitySchema.RFT_IsInlandWaterway });
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefFacilityFilter|IsInlandWaterway/Ferry", "Is Inland Waterway/Ferry");
		}

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddNkFilter("UNLOCO", RefFacilitySchema.RFT_RL_NKLocationCode, ModuleIDs.RefUNLOCO, UNLOCOs);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefFacilityFilter|UNLOCO", "UNLOCO");

			filter = filters.AddNkFilter("Country", RefFacilitySchema.RFT_RN_NKCountryCode, ModuleIDs.RefCountry, Countries);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefFacilityFilter|Country", "Country");
			filter.Category = FilterCategories.Locations;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddTextFilter("Facility Type", RefFacilitySchema.RFT_FacilityType, FacilityTypes);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefFacilityFilter|FacilityType", "Facility Type");
			filter.Category = FilterCategories.ModesAndTypes;

			filter = filters.AddTextFilter("C1F Code", RefFacilitySchema.RFT_Code);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefFacilityFilter|C1FCode", "C1F Code");
			filter.Category = FilterCategories.TextSearch;

			filter = filters.AddTextFilter("BIC Code", RefFacilitySchema.RFT_BICCode);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefFacilityFilter|BICCode", "BIC Code");
			filter.Category = FilterCategories.TextSearch;

			filter = filters.AddTextFilter("SMDG Code", RefFacilitySchema.RFT_SMDGCode);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefFacilityFilter|SMDGCode", "SMDG Code");
			filter.Category = FilterCategories.TextSearch;

			filter = filters.AddTextFilter("Facility Name", RefFacilitySchema.RFT_Name);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefFacilityFilter|FacilityName", "Facility Name");
			filter.Category = FilterCategories.TextSearch;
		}

		RefUNLOCOCollection UNLOCOs
		{
			get { return fUNLOCOs ?? (fUNLOCOs = new RefUNLOCOCollection(Factory)); }
		}

		RefUNLOCOCollection fUNLOCOs;

		ZQuery GetActiveStatusQuery(ZString active)
		{
			var query = new ZQuery();

			var activeStatus = ActiveStatusEnum.Active;

			if (StatusInactive.EqualsUnresolvedOrLocalized(active, ignoreCase: false))
			{
				activeStatus = ActiveStatusEnum.Inactive;
			}
			else if (StatusAll.EqualsUnresolvedOrLocalized(active, ignoreCase: false))
			{
				activeStatus = ActiveStatusEnum.Combined;
			}

			query.AddToFilter(GetActiveStatusQueryFromColumn(activeStatus, RefFacilitySchema.RFT_IsActive));
			query.IgnoreActiveFilter = true;
			return query;
		}

		ZQuery GetActiveStatusQueryFromColumn(ActiveStatusEnum activeType, SchemaBoolColumn column)
		{
			if (activeType == ActiveStatusEnum.Active)
			{
				return new ZQuery(column, true);
			}
			else if (activeType == ActiveStatusEnum.Inactive)
			{
				return new ZQuery(column, false);
			}
			else
			{
				return new ZQuery();
			}
		}

		#region Countries

		RefCountryCollection Countries
		{
			get
			{
				if (fCountries == null)
				{
					fCountries = new RefCountryCollection(Factory);
				}

				return fCountries;
			}
		}

		RefCountryCollection fCountries;

		#endregion

		#region Lookups

		public CodeDescriptionPairList FacilityTypes => new RefFacilityLookups(null).FacilityTypes;

		#endregion
	}
}
