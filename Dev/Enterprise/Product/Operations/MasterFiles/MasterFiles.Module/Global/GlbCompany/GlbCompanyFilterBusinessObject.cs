using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.MasterFiles.Module
{
	public class GlbCompanyFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddTextFilters(result);
			AddRelatedItemFilters(result);

			return result;
		}

		protected override SearchField ResolveSearchField(SearchField searchField)
		{
			var fieldNameUpper = searchField.FieldName.ToUpperInvariant();
			switch (fieldNameUpper)
			{
				case SearchFieldConstants.City:
				case SearchFieldConstants.State:
				case SearchFieldConstants.WebAddress:
				case SearchFieldConstants.Country:
					return new SearchFieldOverride(searchField, FilterCategories.Locations);
				case SearchFieldConstants.Currency:
					return new SearchFieldOverride(searchField, FilterCategories.Other);
				default:
					return base.ResolveSearchField(searchField);
			}
		}

		public static class SearchFieldConstants
		{
			public const string City = "CITY";
			public const string Country = "COUNTRY";
			public const string State = "STATE";
			public const string WebAddress = "WEBADDRESS";
			public const string Currency = "CURRENCY";
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", GlbCompanySchema.GC_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbCompanyFilter|Code", "Code");
			filters.AddTextFilter("Name", GlbCompanySchema.GC_Name).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbCompanyFilter|Name", "Name");

			ModuleFilter filter = filters.AddTextFilter("City", GlbCompanySchema.GC_City);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbCompanyFilter|City", "City");

			filter = filters.AddTextFilter("State", GlbCompanySchema.GC_State);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbCompanyFilter|State", "State");

			filter = filters.AddTextFilter("Web address", GlbCompanySchema.GC_WebAddress);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbCompanyFilter|WebAddress", "Web address");
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			ModuleNkFilter countryFilter = filters.AddNkFilter("Country", GlbCompanySchema.GC_RN_NKCountryCode, ModuleIDs.RefCountry, Countries);
			countryFilter.Category = FilterCategories.Locations;
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbCompanyFilter|Country", "Country/Region");

			ModuleNkFilter currencyFilter = filters.AddNkFilter("Currency", GlbCompanySchema.GC_RX_NKLocalCurrency, ModuleIDs.RefCurrency, Currencies);
			currencyFilter.Category = FilterCategories.Other;
			currencyFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbCompanyFilter|Currency", "Currency");
		}

		#endregion

		#endregion

		#region Lookups

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

		#region Currencies

		public RefCurrencyCollection Currencies
		{
			get
			{
				if (fCurrencies == null)
				{
					fCurrencies = new RefCurrencyCollection(Factory);
				}

				return fCurrencies;
			}
		}
		RefCurrencyCollection fCurrencies;

		#endregion

		#endregion
	}
}
