using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgAddressesFilterBusinessObject : FilterStripBusinessObject
	{
		public OrgAddressesFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "OrgAddresses";
		}

		public static class Schema
		{
			public const string Address1 = "Address1";
			public const string City = "City";
			public const string Code = "Code";
			public const string Organisation = "Organisation";
			public const string RelatedPort = "Related Port";
			public const string State = "State";
			public const string Type = "Type";
			public const string PostCode = "PostCode";
			public const string Country = "Country";
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTypeFilter(filters);

			ModuleGuidFilter organisationFilter = filters.AddGuidFilter(Schema.Organisation, ModuleIDs.Organisation, OrgAddressSchema.OA_OH, new OrgHeaderCollection(Factory));
			organisationFilter.Category = FilterCategories.Organisations;
			organisationFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAddressesFilter|Organisation", "Organization");

			ModuleTextFilter address1Filter = filters.AddTextFilter(Schema.Address1, OrgAddressSchema.OA_Address1);
			address1Filter.Category = FilterCategories.Locations;
			address1Filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAddressesFilter|Address1", "Address 1");

			ModuleTextFilter cityFilter = filters.AddTextFilter(Schema.City, OrgAddressSchema.OA_City);
			cityFilter.Category = FilterCategories.Locations;
			cityFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAddressesFilter|City", "City");

			ModuleTextFilter codeFilter = filters.AddTextFilter(Schema.Code, OrgAddressSchema.OA_Code);
			codeFilter.Category = FilterCategories.TextSearch;
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAddressesFilter|Code", "Code");

			ModuleTextFilter postcodeFilter = filters.AddTextFilter(Schema.PostCode, OrgAddressSchema.OA_PostCode);
			postcodeFilter.Category = FilterCategories.Locations;
			postcodeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAddressesFilter|PostCode", "Postcode");

			ModuleNkFilter countryFilter = filters.AddNkFilter(Schema.Country, OrgAddressSchema.OA_RN_NKCountryCode, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			countryFilter.Category = FilterCategories.Locations;
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAddressesFilter|Country", "Country/Region");

			ModuleNkFilter relatedPortFilter = filters.AddNkFilter(Schema.RelatedPort, OrgAddressSchema.OA_RL_NKRelatedPortCode, ModuleIDs.RefUNLOCO, new RefUNLOCOCollection(Factory));
			relatedPortFilter.Category = FilterCategories.Locations;
			relatedPortFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAddressesFilter|RelatedPort", "Related Port");

			ModuleTextFilter stateFilter = filters.AddTextFilter(Schema.State, OrgAddressSchema.OA_State);
			stateFilter.Category = FilterCategories.Locations;
			stateFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAddressesFilter|State", "State");

			return filters;
		}

		#region Status

		void AddTypeFilter(ModuleFilterCollection filters)
		{
			ModuleTextFilter typesFilter = filters.AddTextFilter(Schema.Type, GetTypeFilter, AddressType_List);
			typesFilter.Category = FilterCategories.StatusAndFlags;
			typesFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgAddressesFilter|Type", "Type");
			typesFilter.SubGroup = new TypeSubGroup();
		}

		class TypeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgAddress));
				query.DefaultJoinCondition = JoinCondition.And;

				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
		}

		ZQuery GetTypeFilter(ZString value)
		{
			if (!value.IsEmpty)
			{
				var query = new ZQuery();

				query.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, value);

				return query;
			}
			else
			{
				return new ZQuery();
			}
		}

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList AddressType_List
		{
			get
			{
				if (addressType_List == null)
				{
					addressType_List = new CodeDescriptionPairList();
					var list = OrgCodeLists.AddressType_List(Factory);
					foreach (var type in Enum.GetNames(typeof(AddressType))) //Enum "AddressType" exists under both ZArchitecture.Business and MasterFiles.Business, take the first one here because ZAddressDropDown did so.
					{
						if (list.ContainsCode(type))
						{
							addressType_List.Add(list[type, StringComparison.Ordinal]);
						}
					}
					addressType_List.AddPair(nameof(AddressType.NoDefault), ResString.GetMultilingualString("4180e27e-0d69-4058-a568-6722ea4da2ed", "No Default"));
				}
				return addressType_List;
			}
		}

		CodeDescriptionPairList addressType_List;
		#endregion
	}
}
