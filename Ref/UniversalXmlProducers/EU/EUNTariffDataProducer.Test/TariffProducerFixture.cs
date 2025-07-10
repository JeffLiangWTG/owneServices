using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class TariffProducerFixture
	{
		[Test]
		public void GenerateWebTariffHeadersFromRefCusTariffTest()
		{
			var result = EUNUtils.GenerateWebTariffHeadersFromRefCusTariff(new[] { "1001101010" }, new List<RefCusTariff>() { new RefCusTariff() { ZZ1_Description = "Test Description",
				ZZ1_TariffCode = "1001101010",
				ZZ1_CompositeKeyOnZZ5 = "10.01",
				ZZ1_StartDate = new DateTime(2020,1,1),
				RefCusTariffLanguages = new [] { new RefCusTariffLanguage() { ZX7_Description = "Other language", ZX7_ZX6_NKLanguage = "ES" } } } }).ToList();

			Assert.AreEqual("1001101010", result[0].TariffCode);
			Assert.AreEqual("10.01", result[0].CompositeKey);
			Assert.AreEqual("Test Description", result[0].Description);
			Assert.AreEqual(new DateTime(2020, 1, 1), result[0].StartDate);
			Assert.AreEqual("ES", result[0].TariffLanguage.First().ZX7_ZX6_NKLanguage);
			Assert.AreEqual("Other language", result[0].TariffLanguage.First().ZX7_Description);
		}
	}
}
