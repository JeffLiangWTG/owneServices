using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	public class UomGeneratorFixture
	{
		[Test]
		public void Cu2UomGeneratorTest()
		{
			var result = TariffUOMGenerator.GenerateUomRecords(uomRecords);
			Assert.NotNull(result.Where(x => x.ZZ8_Type == "CU2" && x.ZZ8_UOM == "KGME"));
			var kgmeUom = result.Single(x => x.ZZ8_UOM == "KGME");

			Assert.That(kgmeUom.ZZ8_ZZA_NKTradeGroup, Is.EqualTo("1011"));
			Assert.That(kgmeUom.ZZ8_ZZA_ZZZ_NKDataGrouping, Is.EqualTo("EUN"));
		}

		[Test]
		public void Cu2UomGeneratorSkippingTrandeGroupAndDataGroupingTest()
		{
			var result = TariffUOMGenerator.GenerateUomRecords(uomRecords, skipTradeGroupAndDataGroupingMapping: true);
			Assert.NotNull(result.Where(x => x.ZZ8_Type == "CU2" && x.ZZ8_UOM == "KGME"));
			var kgmeUom = result.Single(x => x.ZZ8_UOM == "KGME");

			Assert.That(kgmeUom.ZZ8_ZZA_NKTradeGroup, Is.Null);
			Assert.That(kgmeUom.ZZ8_ZZA_ZZZ_NKDataGrouping, Is.Null);
		}

		[SetUp]
		public void Setup()
		{
			ApplicationConfig.ConfigEnvironment();
			uomRecords = new[] { new RawRateRecord("0101290000", null, "99180", new DateTime(2018, 1, 1, 0, 0, 0), new DateTime(2018, 6, 6, 23, 59, 0), "desc", "desc2", "", "1011", "109", "KGM E", "") };
		}

		RawRateRecord[] uomRecords;
	}
}
