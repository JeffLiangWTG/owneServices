using System;
using CargoWise.RefDbRepo.CAReferenceData.Business.CASIMAData;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests.CASIMAData
{
	[TestFixture]
	class SIMARatesFactoryFixture
	{
		[Test]
		public void GetSIMARate()
		{
			var webSIMAText = new WebSIMAText
			{
				ClassificationNumbers = new[] { "8428310000" },
				CBSAReferenceNumber = "AD1734",
				Description = "Copper Pipe Fittings 2",
				Duties = new[] {
					new WebSIMADuty
					{
						DutyType = "ADD",
						DutyValue = "0.25 * KGM",
						DutyCurrency = "RMB",
						CountryOfOriginOrExport = new [] { "CN", "US" },
						EffectiveDate = "2014-02-14"
					},
					new WebSIMADuty
					{
						DutyType = "CVD",
						DutyValue = "UNDEFINED",
						CountryOfOriginOrExport = new [] { "CN" },
						EffectiveDate = "2014-03-14"
					}
				}
			};
			var producer = new SIMARatesFactory();
			var result = producer.GetSIMARate(webSIMAText);
			Assert.AreEqual(new DateTime(2014, 02, 14), result.ZZ1_StartDate);
			Assert.AreEqual("AD1734", result.ZZ1_TariffCode);
			Assert.AreEqual("Copper Pipe Fittings 2", result.ZZ1_Description);
			Assert.AreEqual("SIMA", result.ZZ1_ZZI_NKTariffType);
			var rate1 = result.RefCusRates[0];
			Assert.AreEqual(new DateTime(2014, 02, 14), rate1.ZZ2_StartDate);
			Assert.AreEqual("ADD", rate1.ZZ2_ZY1_NKRateCode);
			Assert.AreEqual("0.25 * KGM", rate1.ZZ2_RateFormula);
			Assert.AreEqual("RMB", rate1.ZZ2_RX_NKCurrencyOverride);

			var rate2 = result.RefCusRates[1];
			Assert.AreEqual(new DateTime(2014, 03, 14), rate2.ZZ2_StartDate);
			Assert.AreEqual("CVD", rate2.ZZ2_ZY1_NKRateCode);
			Assert.AreEqual("0", rate2.ZZ2_RateFormula);
			Assert.AreEqual(null, rate2.ZZ2_RX_NKCurrencyOverride);


			var app1 = rate1.RefCusApplicabilities[0];
			Assert.AreEqual(new DateTime(2014, 02, 14), app1.ZZT_StartDate);
			Assert.AreEqual("CN", app1.ZZT_ZZA_NKTradeGroup);

			var app2 = rate1.RefCusApplicabilities[1];
			Assert.AreEqual(new DateTime(2014, 02, 14), app2.ZZT_StartDate);
			Assert.AreEqual("US", app2.ZZT_ZZA_NKTradeGroup);

			var app3 = rate2.RefCusApplicabilities[0];
			Assert.AreEqual(new DateTime(2014, 03, 14), app3.ZZT_StartDate);
			Assert.AreEqual("CN", app3.ZZT_ZZA_NKTradeGroup);
		}
	}
}
