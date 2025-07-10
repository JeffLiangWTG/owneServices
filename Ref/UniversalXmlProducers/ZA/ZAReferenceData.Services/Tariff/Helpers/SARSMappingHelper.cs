using System.Collections.Generic;
using System.Linq;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers
{
	public static class SARSMappingHelper
	{
		#region Mappings
		static List<(string Schedule, string RateType, string Preference)> PreferenceMapping = new List<(string Schedule, string RateType, string Preference)>
		{
			(Schedules.S1P1, RateTypes.Standard, Preferences.None),
			(Schedules.S1P1, RateTypes.AFCFTA, Preferences.PreferentialRate),
			(Schedules.S1P1, RateTypes.EFTA, Preferences.PreferentialRate),
			(Schedules.S1P1, RateTypes.EU, Preferences.PreferentialRate),
			(Schedules.S1P1, RateTypes.MERCOSUR, Preferences.PreferentialRate),
			(Schedules.S1P1, RateTypes.SADC, Preferences.PreferentialRate),
			(Schedules.S1P1, RateTypes.EUQuota, Preferences.PreferentialQuota),
			(Schedules.S1P1, RateTypes.EFTAQuota, Preferences.PreferentialQuota)
		};

		static readonly Dictionary<string, string> TradeGroupMappings = new Dictionary<string, string>
		{
			{ RateTypes.Standard, TradeGroups.Standard },
			{ RateTypes.AFCFTA, TradeGroups.AFCFTA },
			{ RateTypes.EFTA, TradeGroups.EFTA },
			{ RateTypes.EU, TradeGroups.EU },
			{ RateTypes.MERCOSUR, TradeGroups.MERCOSUR },
			{ RateTypes.SADC, TradeGroups.SADC },
			{ RateTypes.EUQuota, TradeGroups.EUQuota },
			{ RateTypes.EFTAQuota, TradeGroups.EFTAQuota }
		};

		static readonly Dictionary<string, string> RateTypeMappings = TradeGroupMappings.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

		static readonly Dictionary<string, string> UnitsOfMeasureMapping = new Dictionary<string, string>
		{
			{ "1000 KILOWATT HOUR", "MW" },
			{ "1000 KW.H", "MW" },
			{ "1000 U", "KU" },
			{ "1000 UNITS", "KU" },
			{ "10CIGARETTES", "NO" },
			{ "10STICKS", "NO" },
			{ "2U", "PR" },
			{ "BAG", "NO" },
			{ "BG", "NO" },
			{ "CARAT", "CT" },
			{ "CIGARETTES", "NO" },
			{ "CIGARS", "NO" },
			{ "CM", "CM" },
			{ "CT", "CT" },
			{ "CUBIC METRE", "MC" },
			{ "G/KM", "GK" },
			{ "G/M²", "SM" },
			{ "GJ", "GJ" },
			{ "GK", "GK" },
			{ "GRAM OF THE SUGAR CONTENT THAT EXCEEDS /100ML", "GJ" },
			{ "KG NET", "KN" },
			{ "KG", "KG" },
			{ "KILOGRAM", "KG" },
			{ "KILOWATT HOUR", "KW" },
			{ "KK", "KK" },
			{ "KN", "KN" },
			{ "KU", "KU" },
			{ "KW", "KW" },
			{ "KW.H", "KW" },
			{ "KWH", "KW" },
			{ "LA", "LA" },
			{ "LAMP", "NO" },
			{ "LI AA", "LA" },
			{ "LI", "LI" },
			{ "LITRE", "LI" },
			{ "M", "ME" },
			{ "M?", "SM" },
			{ "M²", "SM" },
			{ "M3 AT A PRESSURE OF 101,3 KPA AT 15C", "MC" },
			{ "M³", "MC" },
			{ "M³/101.3KP", "MC" },
			{ "MC", "MC" },
			{ "ME", "ME" },
			{ "METRE", "ME" },
			{ "MM", "MM" },
			{ "MW", "MW" },
			{ "NO", "NO" },
			{ "PACKS", "NO" },
			{ "PR", "PR" },
			{ "SM", "SM" },
			{ "SQUARE METRE", "SM" },
			{ "TON", "KK" },
			{ "TWO UNITS", "PR" },
			{ "U (JUE/PACK)", "NO" },
			{ "U", "NO" },
			{ "UNIT", "NO" }
		};
		#endregion

		public static string GetPreferenceFromScheduleAndRate(string schedule, string rateType)
		{
			string result = null;

			var mapping = PreferenceMapping.FirstOrDefault(x => x.Schedule == schedule && x.RateType == rateType);
			if (mapping.Schedule != null)
			{
				result = mapping.Preference;
			}

			return result;
		}

		public static string GetTradeGroupFromRateType(string rateType)
		{
			var result = rateType ?? string.Empty;

			if (TradeGroupMappings.TryGetValue(result, out var schedule))
			{
				result = schedule;
			}

			return result;
		}

		public static string GetRateTypeFromTradeGroup(string tradeGroup)
		{
			string result = string.Empty;

			if (RateTypeMappings.TryGetValue(tradeGroup, out var rateType))
			{
				result = rateType;
			}

			return result;
		}

		public static string GetUnitOfMeasure(string code)
		{
			return GetUnitOfMeasure(code, out _);
		}
		public static string GetUnitOfMeasure(string code, out bool found)
		{
			var result = code ?? string.Empty;

			if (UnitsOfMeasureMapping.TryGetValue(result, out var uom))
			{
				result = uom;
				found = true;
			}
			else
			{
				found = false;
			}

			return result;
		}
	}
}
