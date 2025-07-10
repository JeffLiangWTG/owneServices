using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class TariffCodeExtractorFixture
	{
		[Test]
		public void GetTariffCodes()
		{
			TariffCodeExtractor.ClearCache();

			var tariffs = new List<RefCusTariff>
			{
				new RefCusTariff { ZZ1_TariffCode = "0101210000", ZZ1_Description = "Pure-bred breeding animals", ZZ1_StartDate = new DateTime(2010,1,1) },
				new RefCusTariff { ZZ1_TariffCode = "0101291000", ZZ1_Description = "For slaughter", ZZ1_StartDate = new DateTime(2011,1,1) },
				new RefCusTariff { ZZ1_TariffCode = "0101299000", ZZ1_Description = "Other", ZZ1_StartDate = new DateTime(2012,1,1) },
				new RefCusTariff { ZZ1_TariffCode = "0102211000", ZZ1_Description = "Heifers (female bovines that have never calved)", ZZ1_StartDate = new DateTime(2013,1,1) },
				new RefCusTariff { ZZ1_TariffCode = "0102213000", ZZ1_Description = "Cows", ZZ1_StartDate = new DateTime(2014,1,1) },
				new RefCusTariff { ZZ1_TariffCode = "0102219000", ZZ1_Description = "Other", ZZ1_StartDate = new DateTime(2015,1,1) }
			};

			var tariffCodeExtractor = new TariffCodeExtractor(tariffs);
			var webTariffHeaders = tariffCodeExtractor.GetActualTariffHeaders("0102210000").ToList();

			Assert.IsNotNull(webTariffHeaders);
			Assert.AreEqual(3, webTariffHeaders.Count);
			var tariffCodes = webTariffHeaders.Select(x => x.TariffCode).ToList();
			Assert.True(tariffCodes.Contains("0102211000"));
			Assert.AreEqual(new DateTime(2013, 1, 1),webTariffHeaders.FirstOrDefault(x => x.TariffCode == "0102211000").StartDate);
			Assert.True(tariffCodes.Contains("0102213000"));
			Assert.AreEqual(new DateTime(2014, 1, 1), webTariffHeaders.FirstOrDefault(x => x.TariffCode == "0102213000").StartDate);
			Assert.True(tariffCodes.Contains("0102219000"));
			Assert.AreEqual(new DateTime(2015, 1, 1), webTariffHeaders.FirstOrDefault(x => x.TariffCode == "0102219000").StartDate);

			webTariffHeaders = tariffCodeExtractor.GetActualTariffHeaders("0101290000").ToList();

			Assert.IsNotNull(webTariffHeaders);
			Assert.AreEqual(webTariffHeaders.Count, 2);
			tariffCodes = webTariffHeaders.Select(x => x.TariffCode).ToList();
			Assert.True(tariffCodes.Contains("0101291000"));
			Assert.AreEqual(new DateTime(2011, 1, 1), webTariffHeaders.FirstOrDefault(x => x.TariffCode == "0101291000").StartDate);
			Assert.True(tariffCodes.Contains("0101299000"));
			Assert.AreEqual(new DateTime(2012, 1, 1), webTariffHeaders.FirstOrDefault(x => x.TariffCode == "0101299000").StartDate);

			webTariffHeaders = tariffCodeExtractor.GetActualTariffHeaders("0101000000").ToList();

			Assert.IsNotNull(webTariffHeaders);
			Assert.AreEqual(3, webTariffHeaders.Count);
			tariffCodes = webTariffHeaders.Select(x => x.TariffCode).ToList();
			Assert.True(tariffCodes.Contains("0101210000"));
			Assert.AreEqual(new DateTime(2010, 1, 1), webTariffHeaders.FirstOrDefault(x => x.TariffCode == "0101210000").StartDate);
			Assert.True(tariffCodes.Contains("0101291000"));
			Assert.AreEqual(new DateTime(2011, 1, 1), webTariffHeaders.FirstOrDefault(x => x.TariffCode == "0101291000").StartDate);
			Assert.True(tariffCodes.Contains("0101299000"));
			Assert.AreEqual(new DateTime(2012, 1, 1), webTariffHeaders.FirstOrDefault(x => x.TariffCode == "0101299000").StartDate);
		}
	}
}
