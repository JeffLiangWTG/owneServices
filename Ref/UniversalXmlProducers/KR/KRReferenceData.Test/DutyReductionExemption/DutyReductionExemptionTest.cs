using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class DutyReductionExemptionTest
	{
		[TestCase(01, 01, 2017)]
		public void TestDutyReductionExemption2017(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "");
		}

		[TestCase(01, 01, 2018)]
		public void TestDutyReductionExemption2018(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "");
		}

		[TestCase(01, 01, 2019)]
		public void TestDutyReductionExemption2019(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "");
		}

		[TestCase(01, 01, 2020)]
		public void TestDutyReductionExemption2020_0(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "0");
		}

		[TestCase(01, 01, 2020)]
		public void TestDutyReductionExemption2020_1(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "1");
		}

		[TestCase(01, 01, 2020)]
		public void TestDutyReductionExemption2020_2(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "2");
		}

		[TestCase(01, 01, 2020)]
		public void TestDutyReductionExemption2020_3(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "3");
		}

		[TestCase(01, 01, 2021)]
		public void TestDutyReduction2021_0(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "0");
		}

		[TestCase(01, 01, 2021)]
		public void TestDutyReduction2021_1(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "1");
		}

		[TestCase(01, 01, 2022)]
		public void TestDutyReductionExemption2022_0(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "0");
		}

		[TestCase(01, 02, 2022)]
		public void TestDutyReductionExemption2022_1(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "1");
		}

		[TestCase(01, 12, 2022)]
		public void TestDutyReductionExemption2022_2(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "2");
		}

		[TestCase(01, 01, 2023)]
		public void TestDutyReductionExemption2023_0(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "0");
		}

		[TestCase(01, 06, 2023)]
		public void TestDutyReductionExemption2023_1(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "1");
		}

		[TestCase(01, 01, 2024)]
		public void TestDutyReductionExemption2024(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "");
		}


		[TestCase(01, 01, 2025)]
		public void TestDutyReductionExemption2025(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "0");
		}

		[TestCase(14, 03, 2025)]
		public void TestDutyReductionExemption2025_1(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "1");
		}


		[TestCase(21, 03, 2025)]
		public void TestDutyReductionExemption2025_2(int day, int month, int year)
		{
			DutyReductionExemptionFile(day, month, year, true, "2");
		}

		void DutyReductionExemptionFile(int day, int month, int year, bool isXls, string seq)
		{
			var publicationDate = new DateTime(year, month, day);
			var fileType = isXls ? "xls" : "xlsx";
			var sequency = string.IsNullOrEmpty(seq) ? "" : string.Format(CultureInfo.CurrentCulture, @"_{0}", seq);

			var inputDataPath = string.Format(CultureInfo.CurrentCulture, @"Res\DutyReductionExemption\{0}\DutyReductionExemption_{0}DataFile{1}.{2}", year, sequency, fileType);

			var inputConfigPath = string.Format(CultureInfo.CurrentCulture, @"Res\DutyReductionExemption\{0}\DutyReductionExemption_{0}Configuration.xml", year);
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"DutyReductionExemption\Output\DutyReductionExemption_{0}{1}.xml", year, sequency));
			var expectedXML = TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.DutyReductionExemption.Output.DutyReductionExemption_{0}{1}.xml", year, sequency));
			new DutyReductionExemptionParser(inputConfigPath, inputDataPath).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
