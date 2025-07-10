using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test.NonPersistentObject
{
	[TestFixture]
	internal class RefCusRateWithoutApplicabilityFixture
	{
		[Test]
		public void RefCusRateWithoutApplicabilityConstructor()
		{
			var tariff = new RefCusTariff { ZZ1_PK = Guid.NewGuid(), ZZ1_TariffCode = "0001" };
			var tariffNationalCode = new RefCusTariffNationalCode { ZZW_PK = Guid.NewGuid(), ZZW_NationalCode = "AAA" };
			var rateUOM = new RefCusRateUOM { ZXG_PK = Guid.NewGuid(), ZXG_UOM = "KG" };
			var rate = new RefCusRate
			{
				ZZ2_PK = Guid.NewGuid(),
				ZZ2_ZZ1_Tariff = tariff.ZZ1_PK,
				ZZ2_StartDate = new DateTimeOffset(2005, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZZ2_EndDate = new DateTimeOffset(2026, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZZ2_RateFormula = "8% VFD",
				ZZ2_SelectorFormula = "SELECT 1",
				ZZ2_RateFormulaDerivedFrom = "TARIC",
				ZZ2_RX_NKCurrencyOverride = "AUD",
				ZZ2_ZY1_RateCode = Guid.NewGuid(),
				ZZ2_ZZS_Preference = Guid.NewGuid(),
				ZZ2_ZZZ_NKDataGrouping = "EUN",
				ZZ2_DataSetCode = "ZZ1",
				ZZ2_DataSetPK = tariff.ZZ1_PK
			};
			rate.RefCusTariff = tariff;
			rate.RefCusTariffNationalCode = tariffNationalCode;
			rate.RefCusRateUOMs.Add(rateUOM);

			var safeRepository = new Mock<ISafeRepository>().Object;
			var result = new RefCusRateWithoutApplicability(rate, safeRepository);
			Assert.AreEqual(rate.ZZ2_PK, result.ZZ2_PK);
			Assert.AreEqual(rate.ZZ2_ZZ1_Tariff, result.ZZ2_ZZ1_Tariff);
			Assert.AreEqual(rate.ZZ2_ZZW_TariffNationalCode, result.ZZ2_ZZW_TariffNationalCode);
			Assert.AreEqual(rate.ZZ2_StartDate, result.ZZ2_StartDate);
			Assert.AreEqual(rate.ZZ2_EndDate, result.ZZ2_EndDate);
			Assert.AreEqual(rate.ZZ2_RateFormula, result.ZZ2_RateFormula);
			Assert.AreEqual(rate.ZZ2_SelectorFormula, result.ZZ2_SelectorFormula);
			Assert.AreEqual(rate.ZZ2_RateFormulaDerivedFrom, result.ZZ2_RateFormulaDerivedFrom);
			Assert.AreEqual(rate.ZZ2_RX_NKCurrencyOverride, result.ZZ2_RX_NKCurrencyOverride);
			Assert.AreEqual(rate.ZZ2_ZY1_RateCode, result.ZZ2_ZY1_RateCode);
			Assert.AreEqual(rate.ZZ2_ZZS_Preference, result.ZZ2_ZZS_Preference);
			Assert.AreEqual(rate.ZZ2_ZZZ_NKDataGrouping, result.ZZ2_ZZZ_NKDataGrouping);
			Assert.AreEqual(rate.ZZ2_DataSetCode, result.ZZ2_DataSetCode);
			Assert.AreEqual(rate.ZZ2_DataSetPK, result.ZZ2_DataSetPK);

			Assert.AreEqual(rate.RefCusTariff, result.RefCusTariff);
			Assert.AreEqual(rate.RefCusTariffNationalCode, result.RefCusTariffNationalCode);
			Assert.AreEqual(rate.RefCusRateUOMs, result.RefCusRateUOMs);
			Assert.AreEqual(0, rate.RefCusApplicabilities.Count);
		}

		[Test]
		public void BuildNonPersistentObjects_Tariff()
		{
			var tariff = new RefCusTariff();
			var rate1 = new RefCusRate { ZZ2_RateFormula = "1", ZZ2_ZZZ_NKDataGrouping = "EUN" };
			var rate2 = new RefCusRate { ZZ2_RateFormula = "2", ZZ2_ZZZ_NKDataGrouping = "EUN" };
			var rate3 = new RefCusRate { ZZ2_RateFormula = "3", ZZ2_ZZZ_NKDataGrouping = "EUN" };
			var app1 = new RefCusApplicability { ZZT_OrderNumber = "1" };
			rate1.RefCusApplicabilities.Add(app1);
			tariff.RefCusRates.Add(rate1);
			tariff.RefCusRates.Add(rate2);
			tariff.RefCusRates.Add(rate3);

			tariff.BuildNonPersistentObjects(new Mock<ISafeRepository>().Object);
			Assert.AreEqual(2, tariff.RefCusRateWithoutApplicabilities.Count);
			Assert.True(tariff.RefCusRateWithoutApplicabilities.Any(x => x.ZZ2_RateFormula == "2"));
			Assert.True(tariff.RefCusRateWithoutApplicabilities.Any(x => x.ZZ2_RateFormula == "3"));
		}

		[Test]
		public void BuildNonPersistentObjects_TariffNationalCode()
		{
			var tariffNationalCode = new RefCusTariffNationalCode();
			var rate1 = new RefCusRate { ZZ2_RateFormula = "1", ZZ2_ZZZ_NKDataGrouping = "EUN" };
			var rate2 = new RefCusRate { ZZ2_RateFormula = "2", ZZ2_ZZZ_NKDataGrouping = "EUN" };
			var rate3 = new RefCusRate { ZZ2_RateFormula = "3", ZZ2_ZZZ_NKDataGrouping = "EUN" };
			var app1 = new RefCusApplicability { ZZT_OrderNumber = "1" };
			rate1.RefCusApplicabilities.Add(app1);
			tariffNationalCode.RefCusRates.Add(rate1);
			tariffNationalCode.RefCusRates.Add(rate2);
			tariffNationalCode.RefCusRates.Add(rate3);

			tariffNationalCode.BuildNonPersistentObjects(new Mock<ISafeRepository>().Object);
			Assert.AreEqual(2, tariffNationalCode.RefCusRateWithoutApplicabilities.Count);
			Assert.True(tariffNationalCode.RefCusRateWithoutApplicabilities.Any(x => x.ZZ2_RateFormula == "2"));
			Assert.True(tariffNationalCode.RefCusRateWithoutApplicabilities.Any(x => x.ZZ2_RateFormula == "3"));
		}
	}
}
