using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using System.IO;
using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Globalization;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class SimpleDrawbackTest
	{
		[TestCase(12, 1, 2025)]

		public void TestSimpleDrawback_2025(int day, int month, int year)
		{
			SimpleDrawback(day, month, year);
		}
		[TestCase(12, 1, 2024)]

		public void TestSimpleDrawback_2024(int day, int month, int year)
		{
			SimpleDrawback(day, month, year);
		}
		[TestCase(12, 1, 2023)]

		public void TestSimpleDrawback_2023(int day, int month, int year)
		{
			SimpleDrawback(day, month, year);
		}
		[TestCase(12, 1, 2022)]
		public void TestSimpleDrawback_2022(int day, int month, int year)
		{
			SimpleDrawback(day, month, year);
		}
		[TestCase(12, 1, 2021)]
		public void TestSimpleDrawback_2021(int day, int month, int year)
		{
			SimpleDrawback(day, month, year);
		}
		[TestCase(12, 1, 2020)]
		public void TestSimpleDrawback_2020(int day, int month, int year)
		{
			SimpleDrawback(day, month, year);
		}
		[TestCase(12, 1, 2019)]
		public void TestSimpleDrawback_2019(int day, int month, int year)
		{
			SimpleDrawback(day, month, year);
		}
		[TestCase(12, 1, 2018)]
		public void TestSimpleDrawback_2018(int day, int month, int year)
		{
			SimpleDrawback(day, month, year);
		}
		[TestCase(12, 1, 2017)]
		public void TestSimpleDrawback_2017(int day, int month, int year)
		{
			SimpleDrawback(day, month, year);
		}

		void SimpleDrawback(int day, int month, int year)
		{
			var publicationDate = new DateTime(year, month, day);
			var inputConfigPath = @"Res\SimpleDrawback_Configuration.xml";
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"SimpleDrawback\Output\Y{0}\SimpleDrawback{0}_Result.xml", year));
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}

			var expectedOutputFiles = new List<string>
			{
				TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.SimpleDrawback.Output.Y{0}.SimpleDrawback{0}_Result.xml", year))
			};
			var inputDataPathXlsx = Path.Combine(TestHelper.BaseTestFilePath, @"SimpleDrawback\Input\SimpleDrawback_DataFile_Sample.xlsx");
			new SimpleDrawbackParser(inputConfigPath, inputDataPathXlsx).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedOutputFiles[0]));
		}
	}
}
