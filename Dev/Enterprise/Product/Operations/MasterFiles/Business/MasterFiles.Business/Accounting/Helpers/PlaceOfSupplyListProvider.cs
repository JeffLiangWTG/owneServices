using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class PlaceOfSupplyListProvider
	{
		public static ReadOnlyCodeDescriptionPairList GetCurrentCompanyPlaceOfSupplyList() => GetPlaceOfSupplyList(GlbCompany.CurrentCompany);

		public static ReadOnlyCodeDescriptionPairList GetPlaceOfSupplyList(GlbCompany company)
		{
			return IsPlaceOfSupplyApplicable(company)
				? new ReadOnlyCodeDescriptionPairList(GetPlaceOfSupplyListAndTypeMapping(company).placeOfSupplyList)
				: new ReadOnlyCodeDescriptionPairList();
		}

		static (ReadOnlyCodeDescriptionPairList placeOfSupplyList, Dictionary<string, string> codeToTypeMapping) GetPlaceOfSupplyListAndTypeMapping(GlbCompany company)
			=> (company.Factory.GetCachedValue(nameof(PlaceOfSupplyListProvider) + company.GC_Code, () => GetPlaceOfSupplyListForCompany(company)));

		public static ReadOnlyCodeDescriptionPairList GetCurrentCompanyPlaceOfSupplyTypeList() => GetPlaceOfSupplyTypeList(GlbCompany.CurrentCompany);

		public static ReadOnlyCodeDescriptionPairList GetPlaceOfSupplyTypeList(GlbCompany company)
		{
			Argument.NotNull(company, nameof(company));

			var result = new CodeDescriptionPairList();

			var enabledTypeCodes = PlaceOfSupplyHelper.GetEnabledPlaceOfSupplyCodes(company);
			if (enabledTypeCodes.Length > 0)
			{
				var typesList = new PlaceOfSupplyTypes();
				enabledTypeCodes.ForEach(x => result.Add(typesList[x]));
			}

			return result;
		}

		public static string GetPlaceTypeFromPlaceCode(GlbCompany company, string placeCode)
		{
			Argument.NotNull(company, nameof(company));

			var result = string.Empty;

			if (IsPlaceOfSupplyApplicable(company))
			{
				var codeToTypeMapping = GetPlaceOfSupplyListAndTypeMapping(company).codeToTypeMapping;
				result = codeToTypeMapping.TryGetValue(placeCode, out result) ? result : string.Empty;
			}

			return result;
		}

		public static bool IsPlaceOfSupplyApplicable(GlbCompany company) => PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled(company);

		static (ReadOnlyCodeDescriptionPairList placeOfSupplyList, Dictionary<string, string> codeToTypeMapping) GetPlaceOfSupplyListForCompany(GlbCompany company)
		{
			Argument.NotNull(company, nameof(company));

			var placeOfSupplyList = new CodeDescriptionPairList();
			var codeToTypeMapping = new Dictionary<string, string>();

			var countryCode = company.GC_RN_NKCountryCode;
			var enabledTypeCodes = PlaceOfSupplyHelper.GetEnabledPlaceOfSupplyCodes(company);

			if (enabledTypeCodes.Contains(PlaceOfSupplyTypes.State.Code))
			{
				var refCountryStates = new RefCountryStatesDependentCollection(company.Country, company.Factory);
				refCountryStates.Load();
				refCountryStates.OfType<RefCountryStates>()
					.OrderBy(state => state.RW_Code)
					.Select(state => new CodeDescriptionPair(state.RW_Code.ToString(), state.RW_DescriptionMultilingual))
					.ForEach(state =>
						{
							placeOfSupplyList.Add(state);
							codeToTypeMapping[state.Code] = PlaceOfSupplyTypes.State.Code;
						});
			}
			if (enabledTypeCodes.Contains(PlaceOfSupplyTypes.TaxZone.Code))
			{
				var filter = new ZQuery(RefZoneHeaderSchema.FZ_ZoneType, RefZoneHeaderLookups.ZoneTypeCodes.Tax);
				filter.AddToFilter(RefZoneHeaderSchema.FZ_IsActive, true);
				var taxZones = new RefZoneHeaderCollection(company.Factory, filter);

				taxZones.Cast<RefZoneHeader>()
					.Where(taxZone => !codeToTypeMapping.ContainsKey(taxZone.FZ_Code))
					.OrderBy(taxZone => taxZone.FZ_Code)
					.Select(taxZone => new CodeDescriptionPair(taxZone.FZ_Code.ToString(), taxZone.FZ_DescriptionMultilingual))
					.ForEach(taxZone =>
						{
							placeOfSupplyList.Add(taxZone);
							codeToTypeMapping[taxZone.Code] = PlaceOfSupplyTypes.TaxZone.Code;
						});
			}
			if (enabledTypeCodes.Contains(PlaceOfSupplyTypes.Country.Code))
			{
				var countries = new RefCountryCollection(company.Factory);

				countries.Cast<RefCountry>()
					.Where(country => !codeToTypeMapping.ContainsKey(country.RN_Code))
					.OrderBy(country => country.RN_Code)
					.Select(country => new CodeDescriptionPair(country.RN_Code.ToString(), country.RN_DescMultilingual))
					.ForEach(country =>
						{
							placeOfSupplyList.Add(country);
							codeToTypeMapping[country.Code] = PlaceOfSupplyTypes.Country.Code;
						});
			}
			if (enabledTypeCodes.Contains(PlaceOfSupplyTypes.PredefinedRule.Code))
			{
				if (!codeToTypeMapping.ContainsKey(OutsideTheLoginCountry.Code))
				{
					placeOfSupplyList.AddPair(OutsideTheLoginCountry.Code, OutsideTheLoginCountry.Description);
					codeToTypeMapping[OutsideTheLoginCountry.Code] = PlaceOfSupplyTypes.PredefinedRule.Code;
				}
				if (!codeToTypeMapping.ContainsKey(OtherTerritories.Code))
				{
					placeOfSupplyList.AddPair(OtherTerritories.Code, OtherTerritories.Description);
					codeToTypeMapping[OtherTerritories.Code] = PlaceOfSupplyTypes.PredefinedRule.Code;
				}
			}

			return (placeOfSupplyList, codeToTypeMapping);
		}

		public static class Codes
		{
			public const string OutsideTheLoginCountry = "ALX";
			public const string OtherTerritories = "OTR";
		}

		public static class Descriptions
		{
			public static MultilingualString OutsideTheLoginCountry => ResString.GetMultilingualString("01330b91-2781-4b74-85cf-90d8d2c2b871", "Outside the Login Country/Region");
			public static MultilingualString OtherTerritories => ResString.GetMultilingualString("a4806065-8a43-4acb-a130-16dc2f47d9a2", "Other Territories");
		}

		static CodeDescriptionPair OutsideTheLoginCountry => new CodeDescriptionPair(Codes.OutsideTheLoginCountry, Descriptions.OutsideTheLoginCountry);
		static CodeDescriptionPair OtherTerritories => new CodeDescriptionPair(Codes.OtherTerritories, Descriptions.OtherTerritories);
	}

	public class PlaceOfSupplyTypes : CodeDescriptionPairList
	{
		public PlaceOfSupplyTypes()
		{
			this.Add(State);
			this.Add(TaxZone);
			this.Add(Country);
			this.Add(PredefinedRule);
		}

		public static CodeDescriptionPair State => new CodeDescriptionPair("STA", ResString.GetMultilingualString("df752400-517e-45f6-bc3f-bca6f64b2e04", "State"));
		public static CodeDescriptionPair PredefinedRule => new CodeDescriptionPair("RUL", ResString.GetMultilingualString("6f24e522-96f3-476c-b793-ad5aa3d69a42", "Predefined Rule in CW1"));

		public static CodeDescriptionPair Country => new CodeDescriptionPair("CON", ResString.GetMultilingualString("d09befd7-0e0a-4d98-9dc4-97c4e005567b", "Country/Region"));
		public static CodeDescriptionPair TaxZone => new CodeDescriptionPair("TZN", ResString.GetMultilingualString("dce6cb19-3725-465e-a61a-80b4f7927fa0", "Tax Zone"));
	}
}
