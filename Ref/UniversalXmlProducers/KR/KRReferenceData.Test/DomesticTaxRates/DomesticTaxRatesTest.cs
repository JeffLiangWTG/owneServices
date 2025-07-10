using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	[TestFixture]
	public class DomesticTaxRatesTest
	{
		[Test]
		public void TestDomesticTaxRates2020()
		{
			DomesticTaxRatesFile(01, 01, 2020, false);
			DomesticTaxRatesFile(01, 07, 2020, false);
		}
		[Test]
		public void TestDomesticTaxRates2021()
		{
			DomesticTaxRatesFile(01, 01, 2021, false);
			DomesticTaxRatesFile(01, 07, 2021, false);
			DomesticTaxRatesFile(12, 11, 2021, false);
		}
		[Test]
		public void TestDomesticTaxRates2022()
		{
			DomesticTaxRatesFile(01, 01, 2022, true);
			DomesticTaxRatesFile(01, 04, 2022, true);
			DomesticTaxRatesFile(01, 05, 2022, true);
			DomesticTaxRatesFile(01, 07, 2022, true);
			DomesticTaxRatesFile(01, 08, 2022, true);
		}
		[Test]
		public void TestDomesticTaxRates2023()
		{
			DomesticTaxRatesFile(01, 01, 2023, false);
			DomesticTaxRatesFile(01, 05, 2023, false);
			DomesticTaxRatesFile(01, 07, 2023, false);
			DomesticTaxRatesFile(01, 09, 2023, false);
			DomesticTaxRatesFile(01, 11, 2023, false);
		}
		[Test]
		public void TestDomesticTaxRates2024()
		{
			DomesticTaxRatesFile(01, 01, 2024, true);
			DomesticTaxRatesFile(01, 03, 2024, true);
			DomesticTaxRatesFile(01, 05, 2024, true);
			DomesticTaxRatesFile(01, 07, 2024, true);
		}
		[Test]
		public void TestDomesticTaxRates2025()
		{
			DomesticTaxRatesFile(01, 01, 2025, false);
			DomesticTaxRatesFile(28, 02, 2025, false);
		}

		void DomesticTaxRatesFile(int day, int month, int year, bool isXls)
		{
			var publicationDate = new DateTime(year, month, day);
			var fileType = isXls ? "xls" : "xlsx";

			var inputDataPath = string.Format(CultureInfo.CurrentCulture, $@"Res\DomesticTaxRates\DomesticTaxRates{year}_{month}.{fileType}");

			var inputConfigPath = string.Format(CultureInfo.CurrentCulture, $@"Res\DomesticTaxRates\DomesticTaxRatesConfiguration.xml");
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"DomesticTaxRates\Output\DomesticTaxRates{0}_{1}.xml", year, month));
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
			var expectedXML = TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.DomesticTaxRates.Output.Y{0}.DomesticTaxRates{0}_{1}.xml", year, month));
			new DomesticTaxRatesParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
