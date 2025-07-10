using System;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class RefCusTradeGroupTest
	{
		[TestCase(1, 12, 2022)]
		public void RefCusTradeGroup(int day, int month, int year)
		{
			var publicationDate = new DateTime(year, month, day);
			var inputConfigPath = Path.Combine(TestHelper.BaseRealFilePath, @"TradeGroupAndCountryEntityConfiguration.xml");
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, @"RefCusTrades\Output\KRTradeGroupAndCountry.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.RefCusTrades.Output.KRTradeGroupAndCountry.xml");
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}

			var inputDataPath = Path.Combine(TestHelper.BaseRealFilePath, @"TradeGroupAndCountry.xlsx");
			new RefCusTradeGroupParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
