using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class WCONomenclatureCopierTest
	{
		[TestCase(01, 01, 2021)]
		public void TestWCOCopiedNomenclatures_2021(int day, int month, int year)
		{
			WCOCopiedNomenclatures(day, month, year, 2017);
		}

		[TestCase(01, 01, 2022)]
		public void TestWCOCopiedNomenclatures_2022(int day, int month, int year)
		{
			WCOCopiedNomenclatures(day, month, year, 2022);
		}

		void WCOCopiedNomenclatures(int day, int month, int year, int YearWCOPublished)
		{
			var wcoPublicationDate = new DateTime(year, month, day);
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, @"WCOCopiedNomenclatures\Output\RefCusNomenclature_KR.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.WCOCopiedNomenclatures.Output.KRRefCusNomenclature_CopiedFromWCO_{0}.xml", year));
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
			var pdfDataPath = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"WCOCopiedNomenclatures\Input\{0}Edition\HSKSample_{1}.pdf", YearWCOPublished, year));
			var xlsDataPath = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"WCOCopiedNomenclatures\Input\{0}Edition\HSKSample_{1}.xlsx", YearWCOPublished, year));
			var configFilePath = string.Format(CultureInfo.CurrentCulture, @"Res\WCOCopiedNomenclatures\{0}Edition\WCONomenclatureEntityConfiguration_{1}.xml", YearWCOPublished, year);
			new WCONomenclatureCopierForTest(pdfDataPath, xlsDataPath, configFilePath).ConvertToXMLFile(outputFile, wcoPublicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}

		class WCONomenclatureCopierForTest : WCONomenclatureCopier
		{
			public WCONomenclatureCopierForTest(string pdfFilePath, string xlsFilePath, string configFilePath) : base(pdfFilePath, xlsFilePath, configFilePath) { }

			protected override ISafeRepository SafeRepository => new RefDataEntityLoaderTest().GetSafeRepository(nomenclatureInputData, tariffInputData);

			List<Tuple<string, string, string, string, DateTime, DateTime>> nomenclatureInputData = new List<Tuple<string, string, string, string, DateTime, DateTime>>() {
				new Tuple<string, string, string, string, DateTime, DateTime>("01", "LIVE ANIMALS; ANIMAL PRODUCTS", "01", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("01", "Live animals", "01.01", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("03", "FISH AND CRUSTACEANS, MOLLUSCS AND OTHER AQUATIC INVERTEBRATES", "01.03", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("0101", "Live horses, asses, mules and hinnies", "01.01..01", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("01012", "Horses", "01.01..01.2", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("02", "VEGETABLE PRODUCTS", "02", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("07", "EDIBLE VEGETABLES AND CERTAIN ROOTS AND TUBERS", "02.07", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("13", "ARTICLES OF STONE, PLASTER, CEMENT, ASBESTOS, MICA OR SIMILAR MATERIALS; CERAMIC PRODUCTS; GLASS AND GLASSWARE", "13", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("69", "Ceramic products", "13.69", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("6902", "Refractory bricks, blocks, tiles and similar refractory ceramic constructional goods, other than those of siliceous fossil meals or similar siliceous earths", "13.69.01.02", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("6903", "Other refractory ceramic goods (for example, retorts, crucibles, muffles, nozzles, plugs, supports, cupels, tubes, pipes, sheaths, rods and slide gates), other than those of siliceous fossil meals or of similar siliceous earths", "13.69.01.03", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("6904", "Ceramic building bricks, flooring blocks, support or filler tiles and the like", "13.69.02.04", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("6905", "Roofing tiles, chimney-pots, cowls, chimney liners, architectural ornaments and other ceramic constructional goods", "13.69.02.05", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("II", "II.- OTHER CERAMIC PRODUCTS", "13.69.02", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("XI", "XI.- PROVITAMINS, VITAMINS AND HORMONES", "06.29.11", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("0503", "[DELETED]", "01.05..03", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),

				new Tuple<string, string, string, string, DateTime, DateTime>("01", "LIVE ANIMALS; ANIMAL PRODUCTS", "01", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("01", "Live animals", "01.01", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("03", "FISH AND CRUSTACEANS, MOLLUSCS AND OTHER AQUATIC INVERTEBRATES", "01.03", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("0101", "Live horses, asses, mules and hinnies", "01.01..01", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("01012", "Horses", "01.01..01.2", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("II", "II.- OTHER CERAMIC PRODUCTS", "13.69.02", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("XI", "XI.- PROVITAMINS, VITAMINS AND HORMONES", "06.29.11", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6))
			};

			List<Tuple<string, string, string, string, DateTime, DateTime>> tariffInputData = new List<Tuple<string, string, string, string, DateTime, DateTime>>() {
				new Tuple<string, string, string, string, DateTime, DateTime>("010121", "Pure-bred breeding animals", "01.01..01.2.1", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("010121", "Pure-bred breeding animals", "01.01..01.2.1", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("010129", "Other", "01.01..01.2.9", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("690210", "Containing by weight, singly or together, more than 50 % of the elements Mg, Ca or Cr, expressed as MgO, CaO or Cr2O3", "13.69.01.02.1", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("690220", "Containing by weight more than 50 % of alumina(Al2O3), of silica(SiO2) or of a mixture or compound of these products", "13.69.01.02.2", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("690290", "Other", "13.69.01.02.9", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("690310", "Containing by weight more than 50 % of free carbon", "13.69.01.03.1", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("690320", "Containing by weight more than 50 % of alumina (Al2O3) or of a mixture or compound of alumina and of silica (SiO2)", "13.69.01.03.2", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("690390", "Other", "13.69.01.03.9", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("6906", "Ceramic pipes, conduits, guttering and pipe fittings", "13.69.02.06", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00)),
				new Tuple<string, string, string, string, DateTime, DateTime>("6906", "Ceramic pipes, conduits, guttering and pipe fittings", "13.69.02.06", "WCO", new DateTime(2022, 1, 1), new DateTime(2079, 6, 6)),
				new Tuple<string, string, string, string, DateTime, DateTime>("4204", "[DELETED]", "08.42..04", "WCO", new DateTime(1900, 1, 1), new DateTime(2021, 12, 31, 23, 59, 00))
			};
		}
	}
}
