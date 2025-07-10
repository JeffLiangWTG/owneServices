using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs
{
	public class HsnTariffEXCListDutyRatesProcessorTest
	{
		[TestCaseSource(nameof(TestCases))]
		public void TestRules(
		(string codeMatch, string[] codesExp) codePattern,
		(string rateFormula, string rateFormulaDerivedFrom, string additionalCode)[] rateRules)
		{
			foreach (var tariff in PopulatedTariffs.Where(tariff => tariff.ZZ1_TariffCode.StartsWith(codePattern.codeMatch)))
			{
				var excRates = tariff.RefCusRates.Where(rate => rate.ZZ2_ZY1_ZZR_NKRateType == Constants.TariffRateType.Code.Excise);

				if (codePattern.codesExp != null && codePattern.codesExp.Any(exempt => tariff.ZZ1_TariffCode.StartsWith(exempt)))
				{
					Assert.That(!excRates.Any(), $"Tariff {tariff.ZZ1_TariffCode}() should NOT have Excise rates. (tariff code exempted)");
				}
				else
				{
					Assert.That(excRates.Any(), $"Tariff {tariff.ZZ1_TariffCode} should have Excise rates.");

					var isRateHasAdditionalCode = rateRules.All(rule => excRates.Any(rate => rate.RefCusApplicabilities?.FirstOrDefault()?.ZZT_AdditionalCode == rule.additionalCode && !rule.additionalCode.IsNullOrEmpty()));

					if (isRateHasAdditionalCode)
					{
						Assert.That(rateRules.All(rule => excRates.Any(rate =>
							rate.ZZ2_RateFormula == rule.rateFormula && rate.RefCusApplicabilities?.FirstOrDefault()?.ZZT_AdditionalCode == rule.additionalCode)),
						$"All requirements of Rates and Applicability should be met.({rateRules})");
					}
					else
					{
						Assert.That(rateRules.All(rule => excRates.Any(rate => rate.ZZ2_RateFormula == rule.rateFormula)),
						$"All requirements of Rates should be met.({rateRules.FirstOrDefault()})");
					}
				}
			}
		}

		static IEnumerable<TestCaseData> TestCases()
		{
			yield return CrateCase(codePattern: ("870220900000", null), rates: new[] { ("VFD * 0.01", "", "8702.1") });
			yield return CrateCase(codePattern: ("870210911300", null), rates: new[] { ("VFD * 0.09", "", (string)null) });
			yield return CrateCase(codePattern: ("870240000000", null), rates: new[] { ("VFD * 0.04", "", "8702.4") });
			yield return CrateCase(codePattern: ("8326", null), rates: new[] { ("VFD * 0.05", "", (string)null) });
			yield return CrateCase(codePattern: ("271111000000", new[] { "271111000000,271112,271113,271119000011,271121000000,271129000011,271129000012" }), rates: new[] { ("2.7944 * [MTQ]", "", (string)null) });
			yield return CrateCase(codePattern: ("271119000011", null), rates: new[] { ("5.778  * [KGM]", "", "2711.19") });
			yield return CrateCase(codePattern: ("340311000000", null), rates: new[] { ("5.1504 * [KGM]", "", (string)null) });
			yield return CrateCase(codePattern: ("271020900000", null), rates: new[] { ("5.1504 * [KGM]", "", "2711.19") });
			yield return CrateCase(codePattern: ("220510100000", null), rates: new[] { ("MAX(246.5234 * [LTR], 0 / 100 * VDF)", "", (string)null) });
			yield return CrateCase(codePattern: ("2207.20", null), rates: new[] { ("MAX(845.9684 * [LPA] / 100 * [LTR], 0 / 100 * VDF)", "", (string)null) });
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
			if (PopulatedTariffs == null)
			{
				var nomenclatureTariffParser = new NomenclatureTariffParser();
				var tariffs = nomenclatureTariffParser.GetTariffs();

				var hsnTariffBanDataParser = new HsnTariffBanDataParser();
				PopulatedTariffs = hsnTariffBanDataParser.PopulateTariffs(tariffs);

				HsnTariffEXCListDutyRatesProcessor.AttachEXCListDutyRates(PopulatedTariffs);
			}
		}
		IEnumerable<RefCusTariff> PopulatedTariffs;
	}
}
