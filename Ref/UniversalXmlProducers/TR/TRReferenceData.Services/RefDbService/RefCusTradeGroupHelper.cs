using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.TRReferenceData.Services.RefDataLoader;

namespace CargoWise.RefDbRepo.TRReferenceData.Services
{
	public static class RefCusTradeGroupHelper
	{
		public static RefCusTradeGroup GetBestMatchRefCusTradeGroupFor(string countryOrTradeGroup)
		{
			if (TradeGroupDictionaryByCountries.Value.TryGetValue(countryOrTradeGroup, out var tradeGroupMatching))
			{
				if (tradeGroupMatching.MatchingDegree < 1 && FuzzyLookupKeysForLogging.Value.Add(countryOrTradeGroup))
				{
					var fuzzyLookupsMessage = $@"Warning: Fuzzy lookup trying to find a RefCusTradeGroup for: {countryOrTradeGroup} ({GetCountryName(countryOrTradeGroup)}),
	returning: {tradeGroupMatching.TradeGroup.ZZA_TradeGroup} ({tradeGroupMatching.TradeGroup.ZZA_Description}), matching Degree: {tradeGroupMatching.MatchingDegree}.";
					Console.Error.WriteLine(fuzzyLookupsMessage);
				}
				return tradeGroupMatching.TradeGroup;
			}

			var countryName = GetCountryName(countryOrTradeGroup);
			if (FuzzyLookupKeysForLogging.Value.Add(countryOrTradeGroup))
			{
				var tradeGroupNotFoundMessage = $@"Warning: Unable to find a RefCusTradeGroup for {countryOrTradeGroup} ({countryName}).";
				Console.Error.WriteLine(tradeGroupNotFoundMessage);
			}
			return new RefCusTradeGroup { ZZA_TradeGroup = countryOrTradeGroup, ZZA_Description = countryName };
		}

		public static string GetCountryName(string countryCode) => GetCountry(countryCode)?.ZZB_Description ?? string.Empty;

		public static RefCusTradeGroupCountry GetCountry(string countryCode) => AllTradeGroups.Value
			.SelectMany(group => group.RefCusTradeGroupCountries).FirstOrDefault(
				country => country.ZZB_RN_NKTradeGroupCountryCode == countryCode
			);

		static readonly Lazy<HashSet<string>> FuzzyLookupKeysForLogging = new Lazy<HashSet<string>>(() => new HashSet<string>());

		// expands: new[] { $"{nameof(RefCusTradeGroup.RefCusTradeGroupCountries)}($filter={nameof(RefCusTradeGroupCountry.ZZB_StartDate)} le {ToFilterString(DateTimeOffset.UtcNow)} AND {nameof(RefCusTradeGroupCountry.ZZB_EndDate)} ge {ToFilterString(DateTimeOffset.UtcNow)})" },
		// TODO: this is a workaround. Handle this on RefDataLoader. -M12 2024-04-30 
		// TODO: The server does not consider the filter. Raised an incident CS01615199. -M12 2024-04-30 

		public static readonly Lazy<List<RefCusTradeGroup>> AllTradeGroups = new Lazy<List<RefCusTradeGroup>>(() =>
		{
			var result = GetRefDataAsync<RefCusTradeGroup>(
				actionName: RefDataActionNames.RefCusTradeGroupUpdate,
				expands: new[] { nameof(RefCusTradeGroup.RefCusTradeGroupCountries) },
				(nameof(RefCusTradeGroup.ZZA_ZZZ_NKDataGrouping), RefDataComparisonOperator.Equal, Constants.CountryCodeTR),
				(nameof(RefCusTradeGroup.ZZA_StartDate), RefDataComparisonOperator.LessThan, DateTimeOffset.UtcNow),
				(nameof(RefCusTradeGroup.ZZA_EndDate), RefDataComparisonOperator.GreaterThan, DateTimeOffset.UtcNow)
			).Result;
			return result;
		});

		public static readonly Lazy<Dictionary<string, (RefCusTradeGroup TradeGroup, decimal MatchingDegree)>> TradeGroupDictionaryByCountries = new Lazy<Dictionary<string, (RefCusTradeGroup, decimal)>>(() =>
		{
			var allTradeGroups = AllTradeGroups.Value;
			var result = allTradeGroups.ToDictionary(
				tradeGroup => tradeGroup.ZZA_TradeGroup,
				tradeGroup => (tradeGroup, 1m)
			);

			foreach (var tradeGroup in allTradeGroups)
			{
				var matching = tradeGroup.TryGetBestMatchingCountry();
				if (matching.MatchingDegree > 0.5m)
				{
					result.TryAdd(matching.Country.ZZB_RN_NKTradeGroupCountryCode, (tradeGroup, matching.MatchingDegree));
				}
			}
			return result;
		});

		public static (RefCusTradeGroupCountry Country, decimal MatchingDegree) TryGetBestMatchingCountry(this RefCusTradeGroup tradeGroup)
		{
			return tradeGroup.RefCusTradeGroupCountries
				.Select(country => (Country: country, MatchingDegree: tradeGroup.GetMatchingDegreeWith(country)))
				.OrderByDescending(item => item.MatchingDegree)
				.FirstOrDefault();
		}

		public static decimal GetMatchingDegreeWith(this RefCusTradeGroup tradeGroup, RefCusTradeGroupCountry tradeGroupCountry)
		{
			var comparer = new RefCusTradeGroupCountryComparer();
			bool isContainsTradeGroupCountry = tradeGroup.RefCusTradeGroupCountries.Contains(tradeGroupCountry, comparer);

			if (!isContainsTradeGroupCountry)
			{
				return 0;
			}
			else
			{
				if (tradeGroup.RefCusTradeGroupCountries.Length == 1 || tradeGroup.ZZA_TradeGroup.SameSearchKeyWith(tradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode))
				{
					return 1;
				}
				else if (tradeGroup.ZZA_TradeGroup.SameSearchKeyWith(Constants.AllCountriesTradeGroup))
				{
					return 0;
				}
				else
				{
					var countryName = CountryNameTranslator.Value.TryGetValue(tradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode, out var translatedCountryName)
						? translatedCountryName
						: tradeGroupCountry.ZZB_Description;
					var similarity = tradeGroup.ZZA_Description.Replace(" STA", string.Empty).ToSearchKey().GetSimilarity(countryName.ToSearchKey());
					return similarity;
				}
			}
		}

		static readonly Lazy<Dictionary<string, string>> CountryNameTranslator = new Lazy<Dictionary<string, string>>(() => new Dictionary<string, string> {
			{ "EG", "Mısır" },
			{ "BA", "Bosna Hersek" },
			{ "MK", "Makedonya" },
			{ "IL", "İsrail" },
			{ "NO", "Norveç" },
			{ "IS", "İzlanda" },
			{ "AL", "Arnavutluk" },
			{ "RS", "Sırbistan" },
			{ "ME", "Karadağ" }
		});
	}

	public class RefCusTradeGroupCountryComparer : IEqualityComparer<RefCusTradeGroupCountry>
	{
		public bool Equals(RefCusTradeGroupCountry x, RefCusTradeGroupCountry y)
		{
			if (x == null || y == null)
				return x == y;

			return x.ZZB_RN_NKTradeGroupCountryCode == y.ZZB_RN_NKTradeGroupCountryCode &&
				   x.ZZB_Description == y.ZZB_Description;
		}

		public int GetHashCode(RefCusTradeGroupCountry obj)
		{
			return HashCode.Combine(obj?.ZZB_RN_NKTradeGroupCountryCode, obj?.ZZB_Description);
		}
	}
}
