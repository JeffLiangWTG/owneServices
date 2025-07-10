using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Microsoft.Extensions.Logging;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public static class ExciseDutyRate
	{
		public static (IReadOnlyCollection<RefCusTariff> Tariffs, IEnumerable<(RuntimeDataRecorder.Keys Key, DateTime UpdateDate)> UpdateDates) ExtractAndTryMapping(IApplicationConfig config, ILogger logger)
		{
			var mappings = ExciseDutyRateMappingReader.ReadAll(config).ToList();
			var pageData = ExciseDutyRatePageReader.ReadAll(config).ToList();

			var matches = new List<(ExciseDutyRatePageData PageData, ExciseDutyRateMapping Mapping, string SearchKey, decimal BestSimilarity)>();

			void TryGetMatches(Func<ExciseDutyRateMapping, string> getKey = null)
			{
				foreach (var pageDataItem in pageData.ToArray())
				{
					var bestMapping = pageDataItem.GetBestFrom(mappings.ToArray(), getKey);
					if (bestMapping != default)
					{
						if (bestMapping.Mapping.MappingType != ExciseDutyRateMappingType.ignore)
						{
							matches.Add((pageDataItem, bestMapping.Mapping, bestMapping.bestMatchingSearchWord, bestMapping.Similarity));
							mappings.Remove(bestMapping.Mapping);
						}
						pageData.Remove(pageDataItem);
					}
				}
			}
			TryGetMatches();

			if (pageData.Any())
			{
				TryGetMatches(mapping => mapping.SupplementaryMappingKey);
			}

			var tariffs = new List<RefCusTariff>();
			foreach (var matching in matches)
			{
				var formula = matching.PageData.Formula;
				if (formula.Success)
				{
					tariffs.Add(new RefCusTariff
					{
						ZZ1_Description = matching.PageData.SearchText,
						ZZ1_TariffCode = matching.Mapping.TaxCode,
						ZZ1_ZZI_NKTariffType = matching.Mapping.TaxType,
						RefCusRates = new []
						{
							new RefCusRate
							{
								ZZ2_RateFormula = formula.Formula
							}
						}
					});
				}
				else
				{
					logger.Log(LogLevel.Error, $"Unable to create a formula, {matching.SearchKey}, {matching.PageData.Reader.Url}");
				}
			}

			if (matches.Any())
			{
				foreach (var matching in matches)
				{
					mappings.Remove(matching.Mapping);
				}
				foreach (var constMapping in mappings.ToArray().Where(mapping => mapping.MappingType == ExciseDutyRateMappingType.constant))
				{
					tariffs.Add(new RefCusTariff
					{
						ZZ1_Description = constMapping.MappingRow[3],
						ZZ1_TariffCode = constMapping.MappingRow[2],
						ZZ1_ZZI_NKTariffType = constMapping.MappingRow[1],
						RefCusRates = new RefCusRate[]
						{
							new RefCusRate
							{
								ZZ2_RateFormula=constMapping.MappingRow[6]
							}
						}
					});
					mappings.Remove(constMapping);
				}
			}

			if (mappings.Any())
			{
				logger.Log(LogLevel.Information, $"Unable to find a matching page item using the mapping:{Environment.NewLine}{string.Join(Environment.NewLine, mappings.Select(mapping => $"  {string.Join(",", mapping.MappingRow)}"))}");
			}

			var pageDatasZeroRate = pageData.Where(page => page.Formula.Success && page.Formula.Rate == 0);
			if (pageDatasZeroRate.Any())
			{
				logger.Log(LogLevel.Information, $"Page items unmatched but with zero rate:{Environment.NewLine}{string.Join(Environment.NewLine, pageDatasZeroRate.Select(page => $"  URL: {page.Reader.Url}, text: {page.SearchText}"))}");
			}

			var pageDatasNonZero = pageData.Except(pageDatasZeroRate);
			if (pageDatasNonZero.Any())
			{
				logger.Log(LogLevel.Error, $"Unable to find mapping for the following page items:{Environment.NewLine}{string.Join(Environment.NewLine, pageDatasNonZero.Select(page => $"  URL: {page.Reader.Url}, text: {page.SearchText}"))}");
			}

			var updateDates = matches.GroupBy(matching => matching.PageData.Reader).Select(group => group.First()).Select(item => (item.PageData.Reader.LogKey, item.PageData.Reader.PagePublishDate)).ToList();

			return (tariffs.OrderBy(tariff => tariff.ZZ1_ZZI_NKTariffType).ThenBy(tariff => tariff.ZZ1_TariffCode).ToArray(), updateDates);
		}

		static (ExciseDutyRateMapping Mapping, string bestMatchingSearchWord, decimal Similarity) GetBestFrom(this ExciseDutyRatePageData pageData, IReadOnlyCollection<ExciseDutyRateMapping> mappings, Func<ExciseDutyRateMapping, string> getMappingKeyToUse = null)
		{
			var pageSearchKey = pageData.SearchText.GetSearchKey();

			var bestMatching = mappings
				.Select(mapping => new
				{
					Mapping = mapping,
					MappingKeyUsed = getMappingKeyToUse?.Invoke(mapping) ?? mapping.MappingKey
				})
				.Select(mappingItem => new
				{
					Similarity = pageSearchKey.GetSimilarity(mappingItem.MappingKeyUsed),
					MappingItem = mappingItem
				})
				.Where(searchResult => searchResult.Similarity >= pageData.TolerableSimilarity)
				.OrderByDescending(searchResult => searchResult.Similarity)
				.FirstOrDefault();

			if (bestMatching != null)
			{
				return (bestMatching.MappingItem.Mapping, bestMatching.MappingItem.MappingKeyUsed, bestMatching.Similarity);
			}
			return default;
		}
	}
}
