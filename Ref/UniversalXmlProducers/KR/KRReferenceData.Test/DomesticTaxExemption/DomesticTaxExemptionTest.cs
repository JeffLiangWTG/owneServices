using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using System.IO;
using System;
using NUnit.Framework;
using System.Globalization;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	[TestFixture]
	sealed class DomesticTaxExemptionTest
	{
		[Test]
		public void TestDomesticTaxExemption_2018()
		{
			AssertDomesticTaxExemption(2018, 01, 01, "xls");
		}

		[Test]
		public void TestDomesticTaxExemption_2019()
		{
			AssertDomesticTaxExemption(2019, 01, 01, "xlsx");
		}

		[Test]
		public void TestDomesticTaxExemption_2020()
		{
			AssertDomesticTaxExemption(2020, 01, 01, "xlsx");
		}

		[Test]
		public void TestDomesticTaxExemption_2021()
		{
			AssertDomesticTaxExemption(2021, 01, 01, "xlsx");
		}

		[Test]
		public void TestDomesticTaxExemption_2022()
		{
			AssertDomesticTaxExemption(2022, 01, 01, "xls");
		}

		[Test]
		public void TestDomesticTaxExemption_2023()
		{
			AssertDomesticTaxExemption(2023, 01, 01, "xls");
		}

		[Test]
		public void TestDomesticTaxExemption_2024()
		{
			AssertDomesticTaxExemption(2024, 01, 01, "xls");
		}

		[Test]
		public void TestDomesticTaxExemption_2025()
		{
			AssertDomesticTaxExemption(2025, 01, 01, "xlsx");
		}

		void AssertDomesticTaxExemption(int year, int month, int day, string fileExtension)
		{
			var publicationDate = new DateTime(year, month, day);
			var inputConfigPath = ApplicationConfig.DomesticTaxExemptionConfigFileInputPath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"DomesticTaxExemption\Output\KRDomesticTaxExemption_{0}.xml", year));
			var expectedXML = TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.DomesticTaxExemption.Output.KRDomesticTaxExemption_{0}.xml", year));
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
			var inputDataPath = Path.Combine(TestHelper.BaseRealFilePath, string.Format(CultureInfo.CurrentCulture, @"DomesticTaxExemption\{0}\DomesticTaxExemption_{0}.{1}", year, fileExtension));
			new DomesticTaxExemptionParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
