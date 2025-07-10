using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.ExportMeasures
{
	[TestFixture]
	sealed class ExportMeasureMapperFixture
	{
		[Test]
		public void GetMapping()
		{
			var mapper = new ExportMeasureMapper();

			var startDate = new DateTime(2025, 2, 10);
			var mapperInput = new TariffInput("22082086",
			[
				new AdditionalCodeInput("U001", "Grappa piemontese o Grappa del Piemonte IG",
				[
					new ApplicabilityInput(startDate, "1011"),
					new ApplicabilityInput(startDate, "1014"),
				]),
				new AdditionalCodeInput("U002", "Grappa friulana o Grappa del Friuli IG",
				[
					new ApplicabilityInput(startDate, "1014"),
				])
			]);

			var tariff = mapper.GetMapping(mapperInput);

			Assert.AreEqual("22082086", tariff.ZZ1_TariffCode);
			Assert.AreEqual(2, tariff.RefCusTariffAdditionalCodes.Length);

			AssertAdditionalCode(tariff.RefCusTariffAdditionalCodes[0], "U001", "Grappa piemontese o Grappa del Piemonte IG", startDate, ["1011", "1014"]);
			AssertAdditionalCode(tariff.RefCusTariffAdditionalCodes[1], "U002", "Grappa friulana o Grappa del Friuli IG", startDate, ["1014"]);
		}

		void AssertAdditionalCode(RefCusTariffAdditionalCode additionalCode, string code, string description, DateTime startDate, string[] tradeGroupCodes)
		{
			Assert.AreEqual(code, additionalCode.ZY2_AdditionalCode);
			Assert.AreEqual(description, additionalCode.ZY2_Description);
			Assert.AreEqual("ESM", additionalCode.ZY2_ZY3_NKCategory);
			Assert.AreEqual(tradeGroupCodes.Length, additionalCode.RefCusApplicabilities.Length);

			for (var i = 0; i < additionalCode.RefCusApplicabilities.Length; i++)
			{
				AssertApplicability(additionalCode.RefCusApplicabilities[i], startDate, tradeGroupCodes[i]);
			}
		}

		void AssertApplicability(RefCusApplicability applicability, DateTime startDate, string tradeGroupCode)
		{
			Assert.AreEqual(startDate, applicability.ZZT_StartDate);
			Assert.AreEqual(tradeGroupCode, applicability.ZZT_ZZA_NKTradeGroup);
		}
	}
}
