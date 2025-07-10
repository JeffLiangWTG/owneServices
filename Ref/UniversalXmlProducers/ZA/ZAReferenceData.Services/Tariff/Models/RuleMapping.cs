using System;
using System.Collections.Generic;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models
{
	public class RuleMapping
	{
		public Guid RuleId { get; set; }
		public string DataGrouping { get; set; }
		public string TariffCode { get; set; }
		public string TariffType { get; set; }
		public string CheckDigit { get; set; }

		public IEnumerable<RuleTariffAttribute> Attributes { get; set; }
		public IEnumerable<RuleTariffUOM> UnitsOfMeasure { get; set; }
		public IEnumerable<RuleRate> Rates { get; set; }

		public bool IsMatch(string tariffCode, string tariffType, string checkDigit)
		{
			return !string.IsNullOrWhiteSpace(TariffCode)
					&& tariffCode.StartsWith(TariffCode, StringComparison.Ordinal)
					&& (TariffType == null || TariffType == tariffType)
					&& (CheckDigit == null || (!string.IsNullOrWhiteSpace(checkDigit) && CheckDigit == checkDigit));
		}

		static readonly Dictionary<string, string> SelectorRateTypeMapping = new Dictionary<string, string>
		{
			{ RuleSelectors.EUQuota, RateTypes.EUQuota },
			{ RuleSelectors.EFTAQuota, RateTypes.EFTAQuota }
		};

		public static string GetRateTypeFromSelector(string selector)
		{
			var result = selector ?? string.Empty;

			if (SelectorRateTypeMapping.TryGetValue(result, out var rateType))
			{
				result = rateType;
			}

			return result;
		}
	}
}
