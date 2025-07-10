using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefDocOrgCusCodeFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string RegulatingCountry = "Regulating Country";
			public const string RegistrationCountryCode = "Registration Country / Code Type";
			public const string DocumentType = "Document Type";
			public const string DocumentRegistrationNumberShortLabel = "Short Label";
			public const string DocumentRegistrationNumberLongLabel = "Long Label";
			public const string DocumentRegistrationNumberDescription = "Description";
			public const string Direction = "Direction";

			#endregion
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddCountryFilters(filters);
			AddTextFilters(filters);
			AddCustomFilters(filters);

			return filters;
		}

		void AddCountryFilters(ModuleFilterCollection filters)
		{
			var regulatingCountryFilter = filters.AddNkFilter(Descriptions.RegulatingCountry, RefDocOrgCusCodeSchema.DOC_RN_NKRegulatingCountry, ModuleIDs.RefCountry, Countries);
			regulatingCountryFilter.Category = FilterCategories.Locations;
			regulatingCountryFilter.MultilingualDescription = ResString.GetMultilingualString("495a9c37-3c35-36ac-4ca8-7c89575e0380", "Regulating Country/Region");
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var documentTypeFilter = filters.AddTextFilter(Descriptions.DocumentType, RefDocOrgCusCodeSchema.DOC_DocumentType, RefDocOrgCusCodeLookups.GetDocumentTypeList(Factory));
			documentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("4dea6c0e-6563-a891-46e6-3e43067f00c3", "Document Type");

			var registrationNumberCategory = new FilterCategory(ResString.GetMultilingualString("975313b8-1dc0-4245-bd0c-d86a0e665de1", "Document Registration No."));

			var shortLabelFilter = filters.AddTextFilter(Descriptions.DocumentRegistrationNumberShortLabel, RefDocOrgCusCodeSchema.DOC_ShortLabel);
			shortLabelFilter.MultilingualDescription = ResString.GetMultilingualString("6e55bf95-2672-46d8-a19f-0757a9b37b2e", "Short Label");
			shortLabelFilter.Category = registrationNumberCategory;

			var longLabelFilter = filters.AddTextFilter(Descriptions.DocumentRegistrationNumberLongLabel, RefDocOrgCusCodeSchema.DOC_LongLabel);
			longLabelFilter.MultilingualDescription = ResString.GetMultilingualString("7d25a54f-8715-4a51-8a8f-4ad5dde69f7a", "Long Label");
			longLabelFilter.Category = registrationNumberCategory;

			var descriptionFilter = filters.AddTextFilter(Descriptions.DocumentRegistrationNumberDescription, RefDocOrgCusCodeSchema.DOC_Description);
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("81e41b5d-e915-4496-9d43-f1228a5ce1d2", "Description");
			descriptionFilter.Category = registrationNumberCategory;

			var directionFilter = filters.AddTextFilter(Descriptions.Direction, RefDocOrgCusCodeSchema.DOC_Direction, RefDocOrgCusCodeLookups.GetDirectionList(Factory));
			directionFilter.MultilingualDescription = ResString.GetMultilingualString("554B94F5-A3CE-4212-A747-62C93276A980", "Direction");
		}

		void AddCustomFilters(ModuleFilterCollection filters)
		{
			var registrationCountryAndTypeFilter = new CountryCustomsCodeFilter(Descriptions.RegistrationCountryCode, GetRegistrationCountryAndTypeQuery, Countries, GetDefaultRegistrationTypeList(), UpdateRegistrationTypeList, GetDefaultRegistrationTypeList)
			{
				MultilingualDescription = ResString.GetMultilingualString("04520d3f-f27e-8b9e-4cf0-aa9845f95ecc", "Registration Country/Region / Code Type")
			};

			filters.AddCustomFilter(registrationCountryAndTypeFilter);
		}

		ZQuery GetRegistrationCountryAndTypeQuery(ZString country, ZString code)
		{
			var query = new ZDBOnlyQuery(typeof(RefDocOrgCusCode));

			if (!country.IsEmpty)
			{
				query.AddToFilter(RefDocOrgCusCodeSchema.DOC_RN_NKCodeCountry, country);
			}

			if (!code.IsEmpty)
			{
				query.AddToFilter(RefDocOrgCusCodeSchema.DOC_CodeType, code);
			}

			return query;
		}

		CodeDescriptionPairList GetDefaultRegistrationTypeList()
		{
			return new OrgCodeLists().CustomsCodes_List((RefCountry)null);
		}

		void UpdateRegistrationTypeList(IList typeList, ZString countryCode)
		{
			if (typeList is CodeDescriptionPairList listToUpdate)
			{
				listToUpdate.Clear();

				if (!countryCode.IsEmpty)
				{
					var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
					if (country != null)
					{
						listToUpdate.AddRangeOverwriteIfExists(new OrgCodeLists().CustomsCodes_List(country));
					}
				}

				if (listToUpdate.Count == 0)
				{
					listToUpdate.AddRangeOverwriteIfExists(new OrgCodeLists().CustomsCodes_List((RefCountry)null));
				}

				listToUpdate.Sort();
			}
		}

		RefCountryCollection Countries
		{
			get
			{
				if (countries == null)
				{
					countries = new RefCountryCollection(Factory);
				}

				return countries;
			}
		}

		RefCountryCollection countries;
	}
}
