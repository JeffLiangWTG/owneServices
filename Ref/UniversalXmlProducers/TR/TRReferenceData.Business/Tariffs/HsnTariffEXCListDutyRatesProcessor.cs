using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	public static class HsnTariffEXCListDutyRatesProcessor
	{
		public static void AttachEXCListDutyRates(IEnumerable<RefCusTariff> tariffs, StringBuilder logger = null)
		{
			var allTariffCodeRules = LoadAllTariffCodeRules();

			foreach (var tariff in tariffs)
			{
				ProcessTariffRates(tariff, allTariffCodeRules);
			}
		}

		static List<HsnTariffEXCListDutyRate> LoadAllTariffCodeRules()
		{
			var allTariffCodeRules = new List<HsnTariffEXCListDutyRate>();
			allTariffCodeRules.AddRange(new HsnTariffEXCListIDutyRatesLoader().GetRates());
			allTariffCodeRules.AddRange(new HsnTariffEXCListIIDutyRatesLoader().GetRates());
			allTariffCodeRules.AddRange(new HsnTariffEXCListIIIDutyRatesLoader().GetRates());
			return allTariffCodeRules;
		}

		static void ProcessTariffRates(RefCusTariff tariff, IEnumerable<HsnTariffEXCListDutyRate> rules)
		{
			var excRates = new List<RefCusRate>();
			var uoms = tariff.RefCusTariffUOMs.ToList();

			excRates.AddRange(ProcessTariffRules(rules, tariff, uoms));

			tariff.RefCusTariffUOMs = uoms.ToArray();

			if (excRates.Any())
			{
				tariff.RefCusRates = (tariff.RefCusRates ?? Enumerable.Empty<RefCusRate>()).Concat(excRates).ToArray();
			}
		}

		static IEnumerable<RefCusRate> ProcessTariffRules(IEnumerable<HsnTariffEXCListDutyRate> rulesList, RefCusTariff tariff, List<RefCusTariffUOM> uoms)
		{
			var filteredRules = FilterRules(rulesList, tariff);
			var rates = new List<RefCusRate>();

			foreach (var rule in filteredRules)
			{
				rates.Add(CreateScdRateFromRule(rule, tariff));

				foreach (var uom in new[] { (rule.UomCU3, Constants.TariffUOM.Type.CU3), (rule.UomCU4, Constants.TariffUOM.Type.CU4), (rule.UomCU5, Constants.TariffUOM.Type.CU5) })
				{
					if (!string.IsNullOrWhiteSpace(uom.Item1))
					{
						EnsureUniqueUom(uoms, uom.Item1, uom.Item2);
					}
				}
			}

			return rates;
		}

		static IEnumerable<HsnTariffEXCListDutyRate> FilterRules(IEnumerable<HsnTariffEXCListDutyRate> rulesList, RefCusTariff tariff)
		{
			return rulesList.Where(rule =>
				tariff.ZZ1_TariffCode.StartsWith(rule.TariffNo, StringComparison.Ordinal) &&
				!rule.ExemptedTariffCodes.Matches(tariff.ZZ1_TariffCode))
				.OrderBy(rule => rule.AdditionalCode);
		}

		static void EnsureUniqueUom(List<RefCusTariffUOM> uoms, string uomCode, string uomType)
		{
			if (!uoms.Any(x => x.ZZ8_Type == uomType && x.ZZ8_UOM == uomCode))
			{
				uoms.Add(new RefCusTariffUOM { ZZ8_Type = uomType, ZZ8_UOM = uomCode });
			}
		}

		static RefCusRate CreateScdRateFromRule(HsnTariffEXCListDutyRate scdRule, RefCusTariff tariff)
		{
			return new RefCusRate
			{
				ZZ2_RateFormula = scdRule.RateFormula,
				ZZ2_RateFormulaDerivedFrom = scdRule.RateFormula,
				ZZ2_StartDate = scdRule.StartDate,
				ZZ2_ZY1_NKRateCode = scdRule.RateCode,
				ZZ2_ZY1_ZZR_NKRateType = scdRule.RateType,
				RefCusApplicabilities = string.IsNullOrWhiteSpace(scdRule.AdditionalCode)
					? Array.Empty<RefCusApplicability>()
					: new[] { CreateApplicability(scdRule) }
			};
		}


		static RefCusApplicability CreateApplicability(HsnTariffEXCListDutyRate scdRule)
		{
			if (string.IsNullOrWhiteSpace(scdRule.AdditionalCode))
			{
				return null;
			}

			return new RefCusApplicability
			{
				ZZT_AdditionalCode = scdRule.AdditionalCode,
				ZZT_StartDate = scdRule.StartDate,
			};
		}
	}
}
