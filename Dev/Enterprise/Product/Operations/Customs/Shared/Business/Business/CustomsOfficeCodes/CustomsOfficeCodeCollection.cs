using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	[ModuleID(ModuleId.ZZRefCusCodeList)]
	public class CustomsOfficeCodeCollection : ZZRefCusCodeListCombinedCollection
	{
		public CustomsOfficeCodeCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType)
			: base(factory, dataGroupingCode, codeType, ZDateTime.Today)
		{
			InitialiseFilterDefaults();
		}

		public CustomsOfficeCodeCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString[] codeTypes, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters)
			: base(factory, dataGroupingCode, codeTypes, ZDateTime.Today, attributeFilters)
		{
			InitialiseFilterDefaults();
		}

		public CustomsOfficeCodeCollection(BusinessObjectFactory factory, ZString[] dataGroupingCodes, ZString codeType, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters)
			: base(factory, dataGroupingCodes, codeType, ZDateTime.Today, attributeFilters)
		{
			InitialiseFilterDefaults();
		}

		public CustomsOfficeCodeCollection(BusinessObjectFactory factory, IEnumerable<ZString> dataGroupingCodes, ZString codeType, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, ZString countryFilter)
			: base(factory, dataGroupingCodes, codeType, ZDateTime.Today, attributeFilters)
		{
			InitialiseFilterDefaults();
			filterCountryCode = countryFilter;
		}

		public CustomsOfficeCodeCollection(BusinessObjectFactory factory, ZString parentDataGroupingCode, ZString codeType, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, ZString countryFilter, ZString[] otherCountries)
			: base(factory, parentDataGroupingCode, codeType, ZDateTime.Today, attributeFilters, otherCountries)
		{
			InitialiseFilterDefaults();
			filterCountryCode = countryFilter;
		}

		public static CustomsOfficeCodeCollection LocalCountryOnlyCustomsOfficesWithRequiredRoles(BusinessObjectFactory factory, ZString dataGroupingCode, params ZString[] roles)
		{
			var result = LocalCountryOnlyCustomsOfficesWithRequiredRoles(factory, new ZString[] { dataGroupingCode }, roles);
			return result;
		}

		public static CustomsOfficeCodeCollection LocalCountryOnlyCustomsOfficesWithRequiredRoles(BusinessObjectFactory factory, ZString[] dataGroupingCodes, params ZString[] roles)
		{
			var collection = new CustomsOfficeCodeCollection(factory, dataGroupingCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, CreateRoleAttributeFilters(roles));
			for (var defaultIndex = 0; defaultIndex < dataGroupingCodes.Length; defaultIndex++)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", dataGroupingCodes[defaultIndex], (dataGroupingCodes.Length > 1) ? ZArchitecture.Business.FilterOrCategory.Green : ZArchitecture.Business.FilterOrCategory.None, defaultIndex));
			}
			return collection;
		}

		public static CustomsOfficeCodeCollection LocalCountryOnlyCustomsOfficesWithRequiredAttributes(BusinessObjectFactory factory, ZString dataGroupingCode, Dictionary<ZString, ZString[]> attributeFilters)
			=> LocalCountryOnlyCustomsOfficesWithRequiredAttributes(factory, new ZString[] { dataGroupingCode }, attributeFilters);

		public static CustomsOfficeCodeCollection LocalCountryOnlyCustomsOfficesWithRequiredAttributes(BusinessObjectFactory factory, ZString[] dataGroupingCodes, Dictionary<ZString, ZString[]> attributeFilters)
			=> new CustomsOfficeCodeCollection(factory, dataGroupingCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, attributeFilters.Select(x => CreateAttributeFilter(x)));

		public static CustomsOfficeCodeCollection GetCachedCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZDateTime date) => GetCachedCollection(factory, dataGroupingCode, date, null);

		public static CustomsOfficeCodeCollection GetCachedCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZDateTime date, Dictionary<ZString, ZString[]> attributeFilters)
		{
			var attributeFiltersList = attributeFilters?.Select(x => CreateAttributeFilter(x)).ToList() ?? new List<RefCusCodeListAttributeFilter>(0);
			var attrFilterKeyPart = string.Join(";", attributeFiltersList.Select(x => x.Key));
			var key = string.Join("_", "CustomsOfficeCodeCollection", dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, date.Date, attrFilterKeyPart);
			return factory.GetCachedValue(key,
				() =>
				{
					var collection = new CustomsOfficeCodeCollection(factory, dataGroupingCode, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice }, attributeFiltersList);
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.ListType, "Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice));
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", date));
					return collection;
				});
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			if (!filterCountryCode.IsEmpty)
			{
				result.AddToFilter(ZArchitecture.Schema.ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, SQLComparisonOperator.NotEqual, filterCountryCode);
			}
			return result;
		}

		protected static RefCusCodeListAttributeFilter[] CreateRoleAttributeFilters(ZString[] roles)
		{
			var result = CreateAttributeFilter(new KeyValuePair<ZString, ZString[]>(RefCusCodeListAttributeTypes.Codes.ROLE, roles));
			return result == null ? null : new[] { result };
		}

		static RefCusCodeListAttributeFilter CreateAttributeFilter(KeyValuePair<ZString, ZString[]> attributeFilter)
		{
			RefCusCodeListAttributeFilter result = null;
			var attributeName = attributeFilter.Key;
			var attributeValues = attributeFilter.Value?.Where(x => !x.IsEmpty).ToArray() ?? Array.Empty<ZString>();
			if (!attributeName.IsEmpty && attributeValues.Any())
			{
				result = new RefCusCodeListAttributeFilter(attributeName, JoinCondition.And, attributeValues);
			}
			return result;
		}

		void InitialiseFilterDefaults()
		{
			FilterBusinessObjectDefaults.RemoveAll();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.Code, "Property", ZString.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", ZString.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", ZDate.Today));
		}

		readonly ZString filterCountryCode;
	}
}
