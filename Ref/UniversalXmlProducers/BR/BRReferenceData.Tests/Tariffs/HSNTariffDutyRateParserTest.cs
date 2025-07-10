using System;
using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.CmdLine;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	class HSNTariffDutyRateParserTest
	{
		[Test]
		public void TestGetPublicationDate()
		{
			var filepath = "C:\\GIT\\wtg\\RefDataRepo\\UniversalXmlProducers\\BR\\BRReferenceData.Tests\\Tariffs\\TestFiles\\Input\\CustomsTariffRate\\Anexo_I_Res_272_2021.pdf";
			Assert.AreEqual(new DateTime(2021, 9, 29, 0, 0, 0), TariffRateProgram.GetPublicationTime(filepath));
			filepath = "C:\\GIT\\wtg\\RefDataRepo\\UniversalXmlProducers\\BR\\BRReferenceData.Tests\\Tariffs\\TestFiles\\Input\\CustomsTariffRate\\tec_20211126.xlsx";
			Assert.AreEqual(new DateTime(2021, 11, 26, 0, 0, 0), TariffRateProgram.GetPublicationTime(filepath));
			filepath = "C:\\GIT\\wtg\\RefDataRepo\\UniversalXmlProducers\\BR\\BRReferenceData.Tests\\Tariffs\\TestFiles\\Input\\CustomsTariffRate\\tec_11126.xlsx";
			Assert.AreEqual(DateTime.Now.Date, TariffRateProgram.GetPublicationTime(filepath).Date);
		}

		[Test]
		public void TestRun()
		{
			var filepath = "test";
			var ex = Assert.Throws<ArgumentException>(() => new TariffRateProgram(filepath, "TestType").Run());
			Assert.AreEqual(ex?.Message, "Invalid file type");

			ex = Assert.Throws<ArgumentException>(() => new TariffRateProgram(string.Empty, string.Empty).Run());
			Assert.AreEqual(ex?.Message, "Please add a second parameter with the file path");

			ex = Assert.Throws<ArgumentException>(() => new TariffRateProgram(filepath, string.Empty).Run());
			Assert.AreEqual(ex?.Message, "Please add a third parameter with the file type");
		}

		[Test]
		public void TestXlsToXmlExport()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var inputStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsTariffRate.tec_20211126.xlsx"))
			using (var expectedStream = Utils.GetManifestResourceStream(@"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.BRRefCusTariffRate20211126.xml"))
			{
				new HSNTariffDutyRateParser(DataSourceName).ExportToXMLFile(inputStream, Constants.HSNTariffDutyRateFileType.XLS_TEC, TestOutputFilePath, new DateTime(2021, 11, 26, 00, 00, 00));
				using (var outputStream = new FileStream(TestOutputFilePath, FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
				}
			}
		}

		[Test]
		public void TestXlsRatesToXmlExport()
		{
			using (var inputStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsTariffRate.Rates.xlsx"))
			using (var expectedStream = Utils.GetManifestResourceStream(@"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.BRRefCusTariffRates_XLS_TEC.xml"))
			using (var expectedLogStream = Utils.GetManifestResourceStream(@"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.BRRefCusTariffRates_XLS_TEC_InvalidTariffs.txt"))
			{
				var parser = new HSNTariffDutyRateParser(DataSourceName);
				parser.ExportToXMLFile(inputStream, Constants.HSNTariffDutyRateFileType.XLS_RATES, TestOutputFilePath, new DateTime(2023, 05, 10, 00, 00, 00));
				using (var outputStream = new FileStream(TestOutputFilePath, FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
				}

				using (var actualReader = new StreamReader(expectedLogStream, Encoding.UTF8))
				{
					var ex = Assert.Throws<InvalidOperationException>(() => { ParserErrorCollector.Instance.ReportErrors(); });
					Assert.AreEqual(actualReader.ReadToEnd(), ex.Message);
				}
			}
		}

		[TearDown]
		public void TearDownCleanup()
		{
			if (File.Exists(TestOutputFilePath))
			{
				File.Delete(TestOutputFilePath);
			}
		}

		readonly string DataSourceName = "BR HSN Tariff II_IPI_PIS_COF Rates";

		readonly string TestOutputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.BRRefCusTariffRate20211126.xml");
	}
}
