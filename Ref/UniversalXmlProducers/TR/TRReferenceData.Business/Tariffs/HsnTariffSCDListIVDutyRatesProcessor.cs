using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	public static class HsnTariffSCDListIVDutyRatesProcessor
	{
		public static void AttachSCDListIVDutyRates(IEnumerable<RefCusTariff> tariffs, StringBuilder logger = null)
		{
			var inputPath = Path.Combine(ApplicationConfig.ResPath, @"OTV Liste IV.xlsx");
			var tariffCodeRulesArray = HsnTariffSCDListIVDutyRatesLoader.GetRates(inputPath).ToArray();

			foreach (var tariff in tariffs)
			{
				var scdRules = tariffCodeRulesArray.Where(
					rule => tariff.ZZ1_TariffCode.StartsWith(rule.TariffCodePattern, StringComparison.Ordinal)
					&& !rule.ExemptedTariffCodes.Matches(tariff.ZZ1_TariffCode)
				);

				var scdTates = new List<RefCusRate>();
				foreach (var scdRule in scdRules.OrderBy(rule=>rule.AdditionalCode))
				{
					var scdRate = new RefCusRate
					{
						ZZ2_RateFormula = $"VFD * {scdRule.Rate:0.00#}",
						ZZ2_RateFormulaDerivedFrom = $"{scdRule.Rate * 100:0.##}%",
						ZZ2_StartDate = tariff.ZZ1_StartDate,
						ZZ2_ZY1_NKRateCode = Constants.TariffRateCode.Code.SpecialConsumptionDuty,
						ZZ2_ZY1_ZZR_NKRateType = Constants.TariffRateType.Code.SpecialConsumptionDuty,
					};
					
					var applicability = new RefCusApplicability { ZZT_AdditionalCode = scdRule.AdditionalCode, ZZT_StartDate = Constants.TariffStartDate };
					scdRate.RefCusApplicabilities = (scdRate.RefCusApplicabilities ?? new RefCusApplicability[] { }).Append(applicability).ToArray();
					scdTates.Add(scdRate);
				}
				tariff.RefCusRates =
					(tariff.RefCusRates ?? new RefCusRate[] { }).AsEnumerable()
					.Union(scdTates)
					.ToArray();
			}
		}
	}
}
