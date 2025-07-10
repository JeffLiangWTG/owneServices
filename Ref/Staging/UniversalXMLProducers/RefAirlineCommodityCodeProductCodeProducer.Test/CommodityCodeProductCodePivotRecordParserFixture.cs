using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProductCodeProducer;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProductCode.Test
{
	public class CommodityCodeProductCodePivotRecordParserFixture
	{
		[Test]
		public void Output_Full()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			Assert.NotNull(binPath, "Bin path should not be null");

			var csvFilePath = Path.Combine(binPath, @"TestFiles\CommodityCodesSample.csv");
			var outputFolderPath = Path.Combine(binPath, @"TestFiles");
			var expectedCommodityCodesXmlFilePath = Path.Combine(binPath, @"TestFiles\CommodityCodesExpected.xml");
			var expectedProductCodesAndPivotsXmlFilePath = Path.Combine(binPath, @"TestFiles\ProductCodesAndPivotsExpected.xml");

			var resultCommodityCodesXmlFilePath = Path.Combine(binPath, @"TestFiles\CommodityCodesResult.xml");
			var resultProductCodesAndPivotsXmlFilePath = Path.Combine(binPath, @"TestFiles\ProductCodesAndPivotsResult.xml");

			XMLCreator.CreateXMLsFromCSVFile(csvFilePath, outputFolderPath, resultCommodityCodesXmlFilePath, resultProductCodesAndPivotsXmlFilePath, new DateTime(1900, 1, 1));

			AssertXMLsEqual(resultCommodityCodesXmlFilePath, expectedCommodityCodesXmlFilePath);
			AssertXMLsEqual(resultProductCodesAndPivotsXmlFilePath, expectedProductCodesAndPivotsXmlFilePath);
		}

		void AssertXMLsEqual(string resultXmlFilePath, string expectedXmlFilePath)
		{
			var expected = File.ReadAllText(expectedXmlFilePath);
			var result = File.ReadAllText(resultXmlFilePath);

			Assert.That(result, Is.EqualTo(expected));
		}
	}
}
