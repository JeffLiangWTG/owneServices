using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public class PreferentialCountryGroupCodeList : CodeDescriptionPairList
	{
		public static PreferentialCountryGroupCodeList GetListFor(ZString countryOfOrigin, ZDateTime dateForDutyRate, BusinessObjectFactory factory, string preferenceCode = "")
		{
			ListCache cache = factory.GetCachedValue("PreferentialCountryGroupCodeListCache", delegate
			{ return new ListCache() { DateForDutyRate = dateForDutyRate, PreferenceCode = preferenceCode }; });
			if (dateForDutyRate != cache.DateForDutyRate || (UniversalTariffHelper.UseRefDatabaseData && preferenceCode != cache.PreferenceCode))
			{
				cache.Clear();
				cache.DateForDutyRate = dateForDutyRate;
				cache.PreferenceCode = preferenceCode;
			}

			PreferentialCountryGroupCodeList result;
			if (!cache.TryGetValue(countryOfOrigin, out result))
			{
				cache[countryOfOrigin] = new PreferentialCountryGroupCodeList(countryOfOrigin, dateForDutyRate, factory, preferenceCode);
				result = cache[countryOfOrigin];
			}

			return result;
		}

		PreferentialCountryGroupCodeList(ZString countryOfOrigin, ZDateTime dateForDutyRate, BusinessObjectFactory factory, string preferenceCode)
		{
			if (UniversalTariffHelper.UseRefDatabaseData)
			{
				var groups = UniversalTariffHelper.GetTradeGroupsFromCountry(countryOfOrigin, dateForDutyRate, factory, preferenceCode);
				foreach (var group in groups)
				{
					AddPair(group.ZZA_TradeGroup, group.ZZA_Description);
				}
			}
			else
			{
				var groups = NZCGroup.FromCountry(countryOfOrigin, dateForDutyRate, factory);
				foreach (var group in groups)
				{
					AddPair(group.Q4_Code, group.Q4_Name);
				}
			}
		}

		class ListCache : Dictionary<string, PreferentialCountryGroupCodeList>
		{
			internal ZDateTime DateForDutyRate;
			internal ZString PreferenceCode;
		}
	}
}
