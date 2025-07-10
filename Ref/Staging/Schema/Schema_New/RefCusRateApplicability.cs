using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	[NotMapped]
	[NonPersistentObject]
	public partial class RefCusRateApplicability
	{
		public RefCusRateApplicability()
		{
			RefCusExcludedTradeGroupNews = new HashSet<RefCusExcludedTradeGroupNew>();
			RefCusRateApplicablityUOMs = new HashSet<RefCusRateApplicabilityUOM>();
		}

		public RefCusRateApplicability(RefCusRate rate, RefCusApplicability app) : this()
		{
			S01_ZZ1_Tariff = rate.ZZ2_ZZ1_Tariff;
			S01_ZZW_TariffNationalCode = rate.ZZ2_ZZW_TariffNationalCode;
			S01_StartDate = app.ZZT_StartDate > rate.ZZ2_StartDate ? app.ZZT_StartDate : rate.ZZ2_StartDate;
			S01_EndDate = app.ZZT_EndDate < rate.ZZ2_EndDate ? app.ZZT_EndDate : rate.ZZ2_EndDate;
			S01_ZY1_NKRateCode = rate.ZZ2_ZY1_NKRateCode;
			S01_ZY1_ZZR_NKRateType = rate.ZZ2_ZY1_ZZR_NKRateType;
			S01_ZY1_ZZR_ZZZ_NKDataGrouping = rate.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping;
			S01_RateFormula = rate.ZZ2_RateFormula;
			S01_ZZS_NKPreference = rate.ZZ2_ZZS_NKPreference;
			S01_ZZS_ZZZ_NKDataGrouping = rate.ZZ2_ZZS_ZZZ_NKDataGrouping;
			S01_SelectorFormula = rate.ZZ2_SelectorFormula;
			S01_ZZZ_NKDataGrouping = rate.ZZ2_ZZZ_NKDataGrouping;
			S01_RateFormulaDerivedFrom = rate.ZZ2_RateFormulaDerivedFrom;
			S01_RX_NKCurrencyOverride = rate.ZZ2_RX_NKCurrencyOverride;

			OriginalRefCusApplicabilityPK = app.ZZT_PK;
			S01_ZZA_NKTradeGroup = app.ZZT_ZZA_NKTradeGroup;
			S01_ZZA_ZZZ_NKDataGrouping = app.ZZT_ZZA_ZZZ_NKDataGrouping;
			S01_AdditionalCode = app.ZZT_AdditionalCode;
			S01_OrderNumber = app.ZZT_OrderNumber;
			S01_ZZA_NKSecondTradeGroup = app.ZZT_ZZA_NKSecondTradeGroup;
			S01_ZZA_ZZZ_NKSecondDataGrouping = app.ZZT_ZZA_ZZZ_NKSecondDataGrouping;
			foreach (var uom in rate.RefCusRateUOMs)
			{
				var rateAppUOM = new RefCusRateApplicabilityUOM(uom)
				{
					S02_S01_RateApplicability = S01_PK
				};
				RefCusRateApplicablityUOMs.Add(rateAppUOM);
			}
			foreach (var ex in app.RefCusExcludedTradeGroups)
			{
				var exNew = new RefCusExcludedTradeGroupNew(ex)
				{
					S03_S01_RateApplicability = S01_PK
				};
				RefCusExcludedTradeGroupNews.Add(exNew);
			}
		}

		public Guid S01_PK { get; set; }
		public Guid? S01_ZZ1_Tariff { get; set; }
		public Guid? S01_ZZW_TariffNationalCode { get; set; }
		public DateTime S01_StartDate { get; set; }
		public DateTime S01_EndDate { get; set; }
		public string S01_ZY1_NKRateCode { get; set; }
		public string S01_ZY1_ZZR_NKRateType { get; set; }
		public string S01_ZY1_ZZR_ZZZ_NKDataGrouping { get; set; }
		public string S01_RateFormula { get; set; }
		public string S01_ZZS_NKPreference { get; set; }
		public string S01_ZZS_ZZZ_NKDataGrouping { get; set; }
		public string S01_SelectorFormula { get; set; }
		public string S01_ZZZ_NKDataGrouping { get; set; }
		public string S01_RateFormulaDerivedFrom { get; set; }
		public string S01_RX_NKCurrencyOverride { get; set; }
		public string S01_ZZA_NKTradeGroup { get; set; }
		public string S01_ZZA_ZZZ_NKDataGrouping { get; set; }
		public string S01_AdditionalCode { get; set; }
		public string S01_OrderNumber { get; set; }
		public string S01_ZZA_NKSecondTradeGroup { get; set; }
		public string S01_ZZA_ZZZ_NKSecondDataGrouping { get; set; }

		public ICollection<RefCusExcludedTradeGroupNew> RefCusExcludedTradeGroupNews { get; set; }
		public RefCusTariff RefCusTariff { get; set; }
		public ICollection<RefCusRateApplicabilityUOM> RefCusRateApplicablityUOMs { get; set; }

		public Guid OriginalRefCusApplicabilityPK { get; private set; }
	}
}
