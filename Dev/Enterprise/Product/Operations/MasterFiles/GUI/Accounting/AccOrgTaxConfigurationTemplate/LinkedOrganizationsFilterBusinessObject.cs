using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public class LinkedOrganizationsFilterBusinessObject : FilterStripBusinessObject
	{
		public LinkedOrganizationsFilterBusinessObject()
		{
			LayoutContext = nameof(FilterLinkedOrganizationsControl);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddFilterOption_Code(filters);
			AddFilterOption_FullName(filters);
			AddFilterOption_MainAddressCountryCodes(filters);
			AddFilterOption_MainAddressCity(filters);
			AddFilterOption_MainAddressState(filters);
			AddFilterOption_MainAddressCategory(filters);
			AddFilterOption_UNLOCO(filters);

			return filters;
		}

		void AddFilterOption_UNLOCO(ModuleFilterCollection filters)
		{
			var closestPortFilter = filters.AddNkFilter(OrgConstants.FilterControl.UNLOCOType.OrgPort, GetMainQuery, ModuleIDs.Location, Locations);
			closestPortFilter.Category = FilterCategories.Organisations;
			closestPortFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|LinkedOrganisationFilter|OrgPort", "Main UNLOCO");

			if (!Locations.AllowZones)
			{
				closestPortFilter.Visibility = FilterVisibility.AlwaysVisible;
				closestPortFilter.DefaultProperty = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				closestPortFilter.PropertyValidation = PropertyValidation;
			}

			ZQuery GetMainQuery(ZString value)
			{
				var query = new ZQuery();
				query.AddToFilter(LocationHelper.GetLocationFilter(Factory, value, OrgHeaderSchema.OH_RL_NKClosestPort, typeof(OrgHeader)));

				return query;
			}

			void PropertyValidation(ZPropertyInfo info)
			{
				if (!((ZString)info.Value).StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
				{
					string errorMessage = Res.GetString("MasterFiles|LinkedOrganisationFilter|UNLOCO", @"Your current security rights only allow you to view organizations based in your current login country/region ({0}).
If you think this is incorrect, please contact your system administrator.", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					info.AddError(errorMessage);
				}
			}
		}

		void AddFilterOption_Code(ModuleFilterCollection filters)
		{
			var closestPortFilter = filters.AddTextFilter("Code", OrgHeaderSchema.OH_Code);
			closestPortFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|LinkedOrganisationFilter|Code", "Code");
		}

		void AddFilterOption_FullName(ModuleFilterCollection filters)
		{
			var closestPortFilter = filters.AddTextFilter("Name", OrgHeaderSchema.OH_FullName);
			closestPortFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|LinkedOrganisationFilter|Name", "Name");
		}

		void AddFilterOption_MainAddressCountryCodes(ModuleFilterCollection filters)
		{
			var closestPortFilter = filters.AddNkFilter(OrgConstants.FilterControl.OrgAddress.Country, GetMainQuery, ModuleIDs.RefCountry, Countries);
			closestPortFilter.Category = FilterCategories.Organisations;
			closestPortFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|LinkedOrganisationFilter|MainAddressCountryCodes", "Country/Region");

			ZQuery GetMainQuery(ZString value)
			{
				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_RN_NKCountryCode, SQLComparisonOperator.Equal, value);

				var orgAddressCapabilitySubQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
				orgAddressCapabilitySubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, SQLComparisonOperator.Equal, 1);
				orgAddressCapabilitySubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, SQLComparisonOperator.Equal, ZArchitecture.Business.AddressType.OFC);

				orgAddressSubQuery.AddSubQuery(OrgAddressSchema.PK, orgAddressCapabilitySubQuery, JoinCondition.And);
				query.AddSubQuery(OrgHeaderSchema.PK, orgAddressSubQuery, JoinCondition.And);
				return query;
			}
		}

		void AddFilterOption_MainAddressCity(ModuleFilterCollection filters)
		{
			var closestPortFilter = filters.AddTextFilter("City", GetMainQuery);
			closestPortFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|LinkedOrganisationFilter|City", "City");
			closestPortFilter.SubGroup = new OrgHeaderSubGroup();

			ZQuery GetMainQuery(SQLComparisonOperator comparisonOperator, ZString value)
			{
				var query = new ZQuery();
				query.AddToFilter(OrgAddressSchema.OA_City, comparisonOperator, value.SubstringSafe(0, OrgAddressSchema.OA_City.MaxLength));
				return query;
			}
		}

		void AddFilterOption_MainAddressState(ModuleFilterCollection filters)
		{
			var closestPortFilter = filters.AddTextFilter("State", GetMainQuery);
			closestPortFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|LinkedOrganisationFilter|State", "State");
			closestPortFilter.SubGroup = new OrgHeaderSubGroup();

			ZQuery GetMainQuery(SQLComparisonOperator comparisonOperator, ZString value)
			{
				var query = new ZQuery();
				query.AddToFilter(OrgAddressSchema.OA_State, comparisonOperator, value.SubstringSafe(0, OrgAddressSchema.OA_State.MaxLength));
				return query;
			}
		}

		void AddFilterOption_MainAddressCategory(ModuleFilterCollection filters)
		{
			var categoryFilter = filters.AddTextFilter("Category", OrgHeaderSchema.OH_Category, CategoryList);
			categoryFilter.Category = FilterCategories.StatusAndFlags;
			categoryFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|LinkedOrganisationFilter|Category", "Category");
		}

		LocationCollection Locations => locations ?? (locations = new LocationCollection(Factory, Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed));
		LocationCollection locations;

		RefCountryCollection Countries => countries ?? (countries = new RefCountryCollection(Factory));
		RefCountryCollection countries;

		CodeDescriptionPairList CategoryList
		{
			get
			{
				if (categoryList == null)
				{
					categoryList = new CodeDescriptionPairList();
					categoryList.AddRange(new CodeDescriptionPairList(OLookUpEditType.OrgHeaderCategory));
					return categoryList;
				}
				return categoryList;
			}
		}
		CodeDescriptionPairList categoryList;

		class OrgHeaderSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}
	}
}
