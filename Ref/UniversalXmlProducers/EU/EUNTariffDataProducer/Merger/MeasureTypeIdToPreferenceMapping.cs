using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	internal static class MeasureTypeIdToPreferenceMapping
	{
		static readonly Dictionary<string, IEnumerable<Preference>> Map = new Dictionary<string, IEnumerable<Preference>>
		{
			{ "103", new[] { new Preference() { Code = "100" } } },
			{ "105", new[] { new Preference() { Code = "140" } } },
			{ "106", new[] { new Preference() { Code = "400" } } },
			{ "112", new[] { new Preference() { Code = "110" } } },
			{ "115", new[] { new Preference() { Code = "115" } } },
			{ "117", new[] { new Preference() { Code = "140" } } },
			{ "119", new[] { new Preference() { Code = "119" } } },
			{ "122", new[] { new Preference() { Code = "120" }, new Preference() { Code = "125" }, new Preference() { Code = "128" } } },
			{ "123", new[] { new Preference() { Code = "123" } } },
			{ "141", new[] { new Preference() { Code = "210" }, new Preference() { Code = "310" } } },
			{ "142", new[] { new Preference() { Code = "200" }, new Preference() { Code = "300" } } },
			{ "143", new[] { new Preference() { Code = "220" }, new Preference() { Code = "225" }, new Preference() { Code = "320" }, new Preference() { Code = "325" } } },
			{ "144", new[] { new Preference() { Code = "200" }, new Preference() { Code = "300" } } },
			{ "145", new[] { new Preference() { Code = "240" }, new Preference() { Code = "340" } } },
			{ "146", new[] { new Preference() { Code = "223" }, new Preference() { Code = "323" } } },
			{ "147", new[] { new Preference() { Code = "420" } } },
			{ "464", new[] { new Preference() { Code = string.Empty, DataGrouping = string.Empty } } },
			{ "551", new[] { new Preference() { Code = string.Empty, DataGrouping = string.Empty } } },
			{ "552", new[] { new Preference() { Code = string.Empty, DataGrouping = string.Empty } } },
			{ "553", new[] { new Preference() { Code = string.Empty, DataGrouping = string.Empty } } },
			{ "554", new[] { new Preference() { Code = string.Empty, DataGrouping = string.Empty } } },
			{ "651", new[] { new Preference() { Code = string.Empty, DataGrouping = string.Empty } } },
			{ "652", new[] { new Preference() { Code = string.Empty, DataGrouping = string.Empty } } },
			{ "653", new[] { new Preference() { Code = string.Empty, DataGrouping = string.Empty } } },
			{ "654", new[] { new Preference() { Code = string.Empty, DataGrouping = string.Empty } } },
			{ "657", new[] { new Preference() { Code = "240" }, new Preference() { Code = "340" } } },
			{ "658", new[] { new Preference() { Code = "240" }, new Preference() { Code = "340" } } },
			{ "690", new[] { new Preference() { Code = string.Empty, DataGrouping = string.Empty } } },
			{ "695", new[] { new Preference() { Code = string.Empty, DataGrouping = string.Empty } } },
			{ "696", new[] { new Preference() { Code = string.Empty, DataGrouping = string.Empty } } }
		};

		internal static IEnumerable<Preference> GetPreferenceCodes(string measureTypeId, IEnumerable<string> tradeGroups)
		{
			Argument.NotNullOrEmpty(measureTypeId, nameof(measureTypeId));

			if (!Map.ContainsKey(measureTypeId))
			{
				return null;
			}

			var results = Map[measureTypeId];
			if (results.Any(x => x.Code.StartsWith("2", StringComparison.Ordinal)) && results.Any(x => x.Code.StartsWith("3", StringComparison.Ordinal)))
			{
				return tradeGroups.Any(x => GSPTradeGroup.Contains(x))
					? results.Where(x => !x.Code.StartsWith("3", StringComparison.Ordinal))
					: results.Where(x => !x.Code.StartsWith("2", StringComparison.Ordinal));
			}
			else
			{
				return results;
			}
		}

		static readonly string[] GSPTradeGroup = { "2005", "2020", "2027" };
	}

	internal class Preference
	{
		public string Code;
		public string DataGrouping = ApplicationConfig.DataGrouping;
	}
}
