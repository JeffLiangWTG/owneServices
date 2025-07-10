using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class InstalmentCodesTest
	{
		[TestCase(01, 01, 2018)]
		public void TestInstalmentCode2018(int day, int month, int year)
		{
			InstalmentCodeFile(day, month, year, true);
		}

		[TestCase(01, 01, 2019)]
		public void TestInstalmentCode2019(int day, int month, int year)
		{
			InstalmentCodeFile(day, month, year, true);
		}

		[TestCase(01, 01, 2020)]
		public void TestInstalmentCode2020(int day, int month, int year)
		{
			InstalmentCodeFile(day, month, year, true);
		}

		[TestCase(01, 01, 2021)]
		public void TestInstalmentCode2021(int day, int month, int year)
		{
			InstalmentCodeFile(day, month, year, true);
		}

		[TestCase(01, 01, 2022)]
		public void TestInstalmentCode2022(int day, int month, int year)
		{
			InstalmentCodeFile(day, month, year, true);
		}

		[TestCase(01, 01, 2023)]
		public void TestInstalmentCode2023(int day, int month, int year)
		{
			InstalmentCodeFile(day, month, year, true);
		}

		[TestCase(01, 01, 2024)]
		public void TestInstalmentCode2024(int day, int month, int year)
		{
			InstalmentCodeFile(day, month, year, true);
		}

		void InstalmentCodeFile(int day, int month, int year, bool isXls)
		{
			var publicationDate = new DateTime(year, month, day);
			var fileType = isXls ? "xls" : "xlsx";

			var inputDataPath = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"InstalmentCodes\Input\{0}\InstalmentCode_{0}DataFileSample.{1}", year, fileType));

			var inputConfigPath = ApplicationConfig.InstalmentCodesConfigFileInputPath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"InstalmentCodes\Output\Y{0}\InstalmentCode_{0}_Result.xml", year));
			var expectedXML = TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.InstalmentCodes.Output.Y{0}.InstalmentCode_{0}_Result.xml", year));
			new InstalmentCodesParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
