using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class MPFAndInformalFeeExemptConditionChecker
	{
		public MPFAndInformalFeeExemptConditionChecker(bool ignoreTIBExemptionCondition)
		{
			this.ignoreTIBExemptionCondition = ignoreTIBExemptionCondition;
		}

		readonly bool ignoreTIBExemptionCondition;

		public bool IsExempt(IDutyData line)
		{
			return (!ignoreTIBExemptionCondition && Is98TariffsWithExemption(line.Tariff))// TIB parent tariff always starts with 98
				|| Is99TariffsWithExemption(line.Tariff, line.DateForDutyCalculation, line.Factory)
				|| IsPrimaySPIWithExemption(line.SpecialProgramsIndicatorPrimary)
				|| IsSPICountryWithExemption(line.SpecialProgramsIndicatorCountry, line.Tariff, line.DateForDutyCalculation, line.Factory)
				|| IsCountryOfOriginWithExemption(line.DateForDutyCalculation, line.CountryOfOrigin, line.Factory)
				|| line.IsDomesticMerchandise
				|| (!ignoreTIBExemptionCondition && Chapter98Helper.IsExemptMPFForCombineLines(line));
		}

		bool Is98TariffsWithExemption(ZString tariff)
		{
			return tariff.StartsWith("98");//While the actual 98 invoice line does not have the fee, its secondary for 9802.00.60 and 9802.00.80 can have the fee
		}

		public bool Is99TariffsWithExemption(ZString tariff, ZDate effectiveDutyDate, BusinessObjectFactory factory)
		{
			USCTariff importTariff = new USCTariff.Loader(factory).LoadBestMatch(tariff, effectiveDutyDate);

			return importTariff != null && importTariff.Applies(TariffRuleList.Codes.SGFTA, effectiveDutyDate);
		}

		bool IsPrimaySPIWithExemption(ZString primarySPI)
		{
			return primarySPI == PrimarySpecProgramIndicatorList.Codes.E
				|| primarySPI == PrimarySpecProgramIndicatorList.Codes.P
				|| primarySPI == PrimarySpecProgramIndicatorList.Codes.R
				|| primarySPI == PrimarySpecProgramIndicatorList.Codes.Y
				|| primarySPI == PrimarySpecProgramIndicatorList.Codes.W;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		bool IsSPICountryWithExemption(ZString spiCountry, ZString tariff, ZDate effectiveDutyDate, BusinessObjectFactory factory)
		{
			return spiCountry == SpecialProgramList.Codes.BH && !tariff.StartsWith("99149920")
				|| spiCountry == SpecialProgramList.Codes.CA && !tariff.StartsWith("9999005")
				|| spiCountry == SpecialProgramList.Codes.CL && !IsApplicableForRule(factory, TariffRuleList.Codes.ChileanPreferenceLevel, tariff, effectiveDutyDate)
				|| spiCountry == SpecialProgramList.Codes.AU
				|| spiCountry == SpecialProgramList.Codes.KR
				|| spiCountry == SpecialProgramList.Codes.PA
				|| spiCountry == SpecialProgramList.Codes.CO
				|| spiCountry == SpecialProgramList.Codes.MX && !tariff.StartsWith("9999006")
				|| spiCountry == SpecialProgramList.Codes.OM && !tariff.StartsWith("99169920")
				|| spiCountry == SpecialProgramList.Codes.PE
				|| spiCountry == SpecialProgramList.Codes.PPlus
				|| spiCountry == SpecialProgramList.Codes.SG && !IsApplicableForRule(factory, TariffRuleList.Codes.SingaporePreferenceLevel, tariff, effectiveDutyDate)
				|| spiCountry == SpecialProgramList.Codes.BSharp
				|| spiCountry == SpecialProgramList.Codes.CSharp
				|| spiCountry == SpecialProgramList.Codes.KSharp
				|| spiCountry == SpecialProgramList.Codes.LSharp
				|| (spiCountry == SpecialProgramList.Codes.S || spiCountry == SpecialProgramList.Codes.SPlus) && !tariff.StartsWith("9999005") && !tariff.StartsWith("9999006");
		}

		bool IsApplicableForRule(BusinessObjectFactory factory, ZString ruleCode, ZString tariff, ZDate effectiveDutyDate)
		{
			USCTariff importTariff = new USCTariff.Loader(factory).LoadBestMatch(tariff, effectiveDutyDate);

			return importTariff != null && importTariff.Applies(ruleCode, effectiveDutyDate);
		}

		bool IsCountryOfOriginWithExemption(ZDateTime dutyDate, ZString countryOfOrigin, BusinessObjectFactory factory)
		{
			var originCountry = factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, countryOfOrigin);

			var result = false;

			if (originCountry != null)
			{
				result = originCountry.IsLeastDevelopedCountry(dutyDate) ||
					originCountry.UC_Code == Core.Constants.CountryCodes.Israel ||
					originCountry.UC_Code == Core.Constants.CountryCodes.SaintKittsAndNevis ||
					originCountry.IsEligibleForCBI(dutyDate) ||
					originCountry.IsInsularPossessionsCountry(dutyDate);
			}

			return result;
		}
	}
}
