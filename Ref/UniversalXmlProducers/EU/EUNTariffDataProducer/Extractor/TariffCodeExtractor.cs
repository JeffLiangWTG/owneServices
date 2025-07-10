using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class TariffCodeExtractor : ITariffCodeExtractor
	{
		readonly IEnumerable<RefCusTariff> extractedTariffs;
		public TariffCodeExtractor(IEnumerable<RefCusTariff> tariffs)
		{
			this.extractedTariffs = tariffs;
		}

		static readonly ConcurrentDictionary<string, IEnumerable<IWebTariffHeader>> cache = new ConcurrentDictionary<string, IEnumerable<IWebTariffHeader>>();

		public IEnumerable<IWebTariffHeader> GetActualTariffHeaders(string tariffHeader, IEnumerable<IWebTariffHeader> source = null)
		{
			Argument.NotNullOrEmpty(tariffHeader, nameof(tariffHeader));
			if (source == null)
			{
				var tariffCodePrefix = tariffHeader.Substring(0, 2);
				source = cache.GetOrAdd(tariffCodePrefix, ExtractTariffDetails);
			}

			var tariffHeaderWithoutTrailingZeros = Common.Utils.RemoveTrailingZeros(tariffHeader);
			var applicableTariffCodes = source.Where(x => x.TariffCode.StartsWith(tariffHeaderWithoutTrailingZeros, StringComparison.Ordinal));

			return applicableTariffCodes;
		}

		public static void ClearCache() => cache.Clear();

		IEnumerable<IWebTariffHeader> ExtractTariffDetails(string tariffHeader)
		{
			Argument.IsTrue(!string.IsNullOrEmpty(tariffHeader) && tariffHeader.Length == 2, nameof(tariffHeader));

			var firstDigit = tariffHeader[0];
			var secondDigit = tariffHeader[1];
			IEnumerable<WebTariffHeader> webTariffHeaders;

			if (secondDigit == '0')
			{
				webTariffHeaders = extractedTariffs.Where(t => t.ZZ1_TariffCode.StartsWith(firstDigit.ToString(), StringComparison.Ordinal))
					.Select(r => new WebTariffHeader(r.ZZ1_TariffCode, r.ZZ1_Description)
					{
						StartDate = r.ZZ1_StartDate,
						CompositeKey = r.ZZ1_CompositeKeyOnZZ5,
						TariffLanguage = r.RefCusTariffLanguages?.ToList() ?? new List<RefCusTariffLanguage>()
					});

				return webTariffHeaders;
			}

			webTariffHeaders = extractedTariffs.Where(t => t.ZZ1_TariffCode.StartsWith(tariffHeader, StringComparison.Ordinal))
				.Select(o => new WebTariffHeader(o.ZZ1_TariffCode, o.ZZ1_Description)
				{
					StartDate = o.ZZ1_StartDate,
					CompositeKey = o.ZZ1_CompositeKeyOnZZ5,
					TariffLanguage = o.RefCusTariffLanguages?.ToList() ?? new List<RefCusTariffLanguage>()
				});
			return webTariffHeaders;
		}
	}
}
