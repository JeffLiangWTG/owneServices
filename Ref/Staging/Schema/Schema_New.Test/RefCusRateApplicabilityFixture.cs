using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	class RefCusRateApplicabilityFixture
	{
		[Test]
		public void Constructor()
		{
			var rate = new RefCusRate
			{
				ZZ2_ZZ1_Tariff = Guid.NewGuid(),
				ZZ2_ZZW_TariffNationalCode = Guid.NewGuid(),
				ZZ2_StartDate = new DateTime(2001, 01, 01, 0, 0, 0, DateTimeKind.Utc),
				ZZ2_EndDate = new DateTime(2009, 01, 01, 0, 0, 0, DateTimeKind.Utc),
				ZZ2_RateFormula = "8% VFD",
				ZZ2_SelectorFormula = "SELECT 1",
				ZZ2_RateFormulaDerivedFrom = "TARIC",
				ZZ2_RX_NKCurrencyOverride = "AUD",
				ZZ2_ZY1_NKRateCode = "AAA",
				ZZ2_ZZS_NKPreference = "BBB",
				ZZ2_ZZZ_NKDataGrouping = "EUN",
				RefCusRateUOMs = new HashSet<RefCusRateUOM> { new RefCusRateUOM(), new RefCusRateUOM() }
			};
			var app = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_AdditionalCode = "C999",
				ZZT_OrderNumber = "S001",
				ZZT_StartDate = new DateTime(2001, 01, 01, 0, 0, 0, DateTimeKind.Utc),
				ZZT_EndDate = new DateTime(2009, 01, 01, 0, 0, 0, DateTimeKind.Utc),
				ZZT_ZZA_NKTradeGroup = "CCC",
				ZZT_ZZA_NKSecondTradeGroup = "DDD",
				RefCusExcludedTradeGroups = new HashSet<RefCusExcludedTradeGroup> { new RefCusExcludedTradeGroup() }
			};
			var rateApp = new RefCusRateApplicability(rate, app);
			Assert.That(rateApp.S01_ZZ1_Tariff == rate.ZZ2_ZZ1_Tariff);
			Assert.That(rateApp.S01_ZZW_TariffNationalCode == rate.ZZ2_ZZW_TariffNationalCode);
			Assert.That(rateApp.S01_StartDate == (app.ZZT_StartDate > rate.ZZ2_StartDate ? app.ZZT_StartDate : rate.ZZ2_StartDate));
			Assert.That(rateApp.S01_EndDate == (app.ZZT_EndDate < rate.ZZ2_EndDate ? app.ZZT_EndDate : rate.ZZ2_EndDate));
			Assert.That(rateApp.S01_ZY1_NKRateCode == rate.ZZ2_ZY1_NKRateCode);
			Assert.That(rateApp.S01_RateFormula == rate.ZZ2_RateFormula);
			Assert.That(rateApp.S01_ZZS_NKPreference == rate.ZZ2_ZZS_NKPreference);
			Assert.That(rateApp.S01_SelectorFormula == rate.ZZ2_SelectorFormula);
			Assert.That(rateApp.S01_ZZZ_NKDataGrouping == rate.ZZ2_ZZZ_NKDataGrouping);
			Assert.That(rateApp.S01_RateFormulaDerivedFrom == rate.ZZ2_RateFormulaDerivedFrom);
			Assert.That(rateApp.S01_RX_NKCurrencyOverride == rate.ZZ2_RX_NKCurrencyOverride);
			Assert.That(rateApp.S01_ZZA_NKTradeGroup == app.ZZT_ZZA_NKTradeGroup);
			Assert.That(rateApp.S01_AdditionalCode == app.ZZT_AdditionalCode);
			Assert.That(rateApp.S01_OrderNumber == app.ZZT_OrderNumber);
			Assert.That(rateApp.S01_ZZA_NKSecondTradeGroup == app.ZZT_ZZA_NKSecondTradeGroup);

			Assert.That(rateApp.RefCusRateApplicablityUOMs.Count == 2);
			Assert.That(rateApp.RefCusExcludedTradeGroupNews.Count == 1);
		}
	}
}
