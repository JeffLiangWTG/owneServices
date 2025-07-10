using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using NUnit.Framework;
using System.Data;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs
{
	public class HsnTariffSCDListIVDutyRatesProcessorTest
	{
		[TestCaseSource(nameof(TestCases))]
		public void TestRules(
			(string codeMatch, string[] codesExp) codePattern,
			(string rateFormula, string rateFormulaDerivedFrom, string additionalCode)[] rateRules
		)
		{
			foreach (var tariff in PopulatedTariffs.Where(tariff => tariff.ZZ1_TariffCode.StartsWith(codePattern.codeMatch)))
			{
				var scdRates = tariff.RefCusRates.Where(rate => rate.ZZ2_ZY1_ZZR_NKRateType == Constants.TariffRateType.Code.SpecialConsumptionDuty);

				if (codePattern.codesExp != null && codePattern.codesExp.Any(exempt => tariff.ZZ1_TariffCode.StartsWith(exempt)))
				{
					Assert.That(!scdRates.Any(), $"Tariff {tariff.ZZ1_TariffCode}() should NOT have SCD rates. (tariff code exempted)");
				}
				else
				{
					Assert.That(scdRates.Any(), $"Tariff {tariff.ZZ1_TariffCode} should have SCD rates.");

					var isRateHasAdditionalCode = rateRules.All(rule => scdRates.Any(rate => rate.RefCusApplicabilities?.FirstOrDefault()?.ZZT_AdditionalCode == rule.additionalCode && rule.additionalCode.IsNullOrEmpty()));

					if (isRateHasAdditionalCode)
					{
						Assert.That(rateRules.All(rule => scdRates.Any(rate =>
							rate.ZZ2_RateFormula == rule.rateFormula
							&& rate.ZZ2_RateFormulaDerivedFrom == rule.rateFormulaDerivedFrom
							&& rate.RefCusApplicabilities?.FirstOrDefault()?.ZZT_AdditionalCode == rule.additionalCode)),
						$"All requirements of Rates and Applicability should be met.({rateRules})");
					}
					else
					{
						Assert.That(rateRules.All(rule => scdRates.Any(rate =>
							rate.ZZ2_RateFormula == rule.rateFormula
							&& rate.ZZ2_RateFormulaDerivedFrom == rule.rateFormulaDerivedFrom)),
						$"All requirements of Rates should be met.({rateRules})");
					}
				}
			}
		}

		static IEnumerable<TestCaseData> TestCases()
		{
			yield return CrateCase(codePattern: ("160431000000", null), rates: new[] { ("VFD * 0.20", "20%", (string)null) });
			yield return CrateCase(codePattern: ("3303", new[] { "330300900011" }), rates: new[] { ("VFD * 0.20", "20%", (string)null) });
			yield return CrateCase(codePattern: ("8516", new[] { "851640" }), rates: new[] { ("VFD * 0.067", "6.7%", (string)null) });
			yield return CrateCase(
				codePattern: ("4902", null),
				rates: new[] { ("VFD * 0.20", "20%", null), ("VFD * 0.20", "20%", "1117") });
			yield return CrateCase(
				codePattern: ("8415", new[] { "841520000000", "841581001000", "841582001000", "841583001000", "841590001000", "841590009019" }),
				rates: new[] { ("VFD * 0.067", "6.7%", (string)null) });
			yield return CrateCase(
				codePattern: ("851714000019", null),
				rates: new[] { ("VFD * 0.25", "25%", "8517.1"), ("VFD * 0.40", "40%", "8517.2"), ("VFD * 0.50", "50%", "8517.3"), });
			yield return CrateCase(("9601", null), rates: new[] { ("VFD * 0.20", "20%", (string)null) });
		}

		static TestCaseData CrateCase(
			(string codeMatch, string[] codesExp) codePattern,
			(string rateFormula, string rateFormulaDerivedFrom, string additionalCode)[] rates
		)
		{
			return new TestCaseData(codePattern, rates);
		}

		[OneTimeSetUp]
		public void SetupTariffLists()
		{
			var nomenclatureTariffParser = new NomenclatureTariffParser();
			var tariffs = nomenclatureTariffParser.GetTariffs();

			var hsnTariffBanDataParser = new HsnTariffBanDataParser();
			PopulatedTariffs = hsnTariffBanDataParser.PopulateTariffs(tariffs);

			HsnTariffSCDListIVDutyRatesProcessor.AttachSCDListIVDutyRates(PopulatedTariffs);
		}
		IEnumerable<RefCusTariff> PopulatedTariffs;
	}
}
