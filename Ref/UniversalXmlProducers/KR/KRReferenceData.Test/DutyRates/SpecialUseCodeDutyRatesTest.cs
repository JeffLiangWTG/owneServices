using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using System.IO;
using System;
using NUnit.Framework;
using System.Globalization;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using System.Collections.Generic;
using static CargoWise.RefDbRepo.KRReferenceData.Business.Constants;
using Org.BouncyCastle.Ocsp;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	[TestFixture]
	sealed class SpecialUseCodeDutyRatesTest
	{
		[Test]
		public void TestSpecialUseCodeDutyRates_2020()
		{
			AssertSpecialUseCodeDutyRates(2020, 01, 01, false);
		}
		[Test]
		public void TestSpecialUseCodeDutyRates_2021()
		{
			AssertSpecialUseCodeDutyRates(2021, 01, 01, false);
		}
		[Test]
		public void TestSpecialUseCodeDutyRates_2022()
		{
			AssertSpecialUseCodeDutyRates(2022, 01, 01, false);
		}
		[Test]
		public void TestSpecialUseCodeDutyRates_2023()
		{
			AssertSpecialUseCodeDutyRates(2023, 01, 01, false);
		}

		[Test]
		public void TestSpecialUseCodeDutyRates_2024()
		{
			AssertSpecialUseCodeDutyRates(2024, 01, 01, false);
		}

		[Test]
		public void TestSpecialUseCodeDutyRates_2025()
		{
			AssertSpecialUseCodeDutyRates(2025, 01, 01, false);
		}

		void AssertSpecialUseCodeDutyRates(int year, int month, int day, bool isXls)
		{
			var publicationDate = new DateTime(year, month, day);
			var fileExtension = isXls ? "xls" : "xlsx";
			var inputDataPath = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"DutyRates\Input\{0}\DutyRate_{0}DataFileSample.{1}", year, fileExtension));

			var inputConfigPath = ApplicationConfig.SpecialUseCodeDutyRateConfigFileInputPath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"DutyRates\Output\Y{0}\KRSpecialUseCodeDutyRate.xml", year));
			var expectedXML = TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.DutyRates.Output.Y{0}.KRSpecialUseCodeDutyRate.xml", year));
			new SpecialUseCodeDutyRateParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
