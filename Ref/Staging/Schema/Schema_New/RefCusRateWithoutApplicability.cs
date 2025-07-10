using System.ComponentModel.DataAnnotations.Schema;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	[NotMapped]
	[NonPersistentObject]
	public class RefCusRateWithoutApplicability : RefCusRate
	{
		public RefCusRateWithoutApplicability(RefCusRate rate)
		{
			ZZ2_PK = rate.ZZ2_PK;
			ZZ2_ZZ1_Tariff = rate.ZZ2_ZZ1_Tariff;
			ZZ2_ZZW_TariffNationalCode = rate.ZZ2_ZZW_TariffNationalCode;
			ZZ2_StartDate = rate.ZZ2_StartDate;
			ZZ2_EndDate = rate.ZZ2_EndDate;
			ZZ2_ZY1_NKRateCode = rate.ZZ2_ZY1_NKRateCode;
			ZZ2_ZY1_ZZR_NKRateType = rate.ZZ2_ZY1_ZZR_NKRateType;
			ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping = rate.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping;
			ZZ2_RateFormula = rate.ZZ2_RateFormula;
			ZZ2_ZZS_NKPreference = rate.ZZ2_ZZS_NKPreference;
			ZZ2_ZZS_ZZZ_NKDataGrouping = rate.ZZ2_ZZS_ZZZ_NKDataGrouping;
			ZZ2_SelectorFormula = rate.ZZ2_SelectorFormula;
			ZZ2_ZZZ_NKDataGrouping = rate.ZZ2_ZZZ_NKDataGrouping;
			ZZ2_RateFormulaDerivedFrom = rate.ZZ2_RateFormulaDerivedFrom;
			ZZ2_RX_NKCurrencyOverride = rate.ZZ2_RX_NKCurrencyOverride;

			RefCusRateUOMs = rate.RefCusRateUOMs;
			ZZ2_ZZ1_TariffNavigation = rate.ZZ2_ZZ1_TariffNavigation;
			ZZ2_ZZW_TariffNationalCodeNavigation = rate.ZZ2_ZZW_TariffNationalCodeNavigation;
		}
	}
}
