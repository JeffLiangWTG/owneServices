using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using NUnit.Framework;
using XMLTools;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Business
{
	[TestFixture]
	public class TradeGroupsProcessorTest
	{
		[Test]
		public void TestGenerateFiles_WhenBothExist()
		{
			File.Copy(
				Path.Combine(testFilesFolder, "IL_TradeGroupsResponse.xml"),
				Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupsResponse.xml"),
				true);
			File.Copy(
				Path.Combine(testFilesFolder, "IL_TradeGroupCountriesResponse.xml"),
				Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupCountriesResponse.xml"),
				true);

			var publicationDate = DateTime.ParseExact("20250504172057", "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
			TradeGroupsProcessor.GenerateFiles(publicationDate, new Logger());

			Assert.IsFalse(
				File.Exists(Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupsResponse.xml")),
				"IL_TradeGroupsResponse.xml file was not cleaned.");
			Assert.IsFalse(
				File.Exists(Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupCountriesResponse.xml")),
				"IL_TradeGroupCountriesResponse.xml file was not cleaned.");
			Assert.IsTrue(
				File.Exists(Path.Combine(ApplicationConfig.Instance.OutputDirectory, "ILTradeGroups.xml")),
				"ILTradeGroups.xml file was not generated.");

			var actualFileContent = File.ReadAllText(Path.Combine(ApplicationConfig.Instance.OutputDirectory, "ILTradeGroups.xml"));
			var expectedFileContent = File.ReadAllText(Path.Combine(testFilesFolder, "ILTradeGroups.xml"));
			XmlComparer xmlComparer = new XmlComparer();
			var (res, diff) = xmlComparer.CompareXml(expectedFileContent, actualFileContent, true);

			Assert.IsTrue(res, $"XML files are not equal. Differences: {diff}");
		}

		[Test]
		public void TestGenerateFiles_WhenOnlyTradeGroupsResponse()
		{
			File.Copy(
				Path.Combine(testFilesFolder, "IL_TradeGroupsResponse.xml"),
				Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupsResponse.xml"),
				true);

			var publicationDate = DateTime.ParseExact("20250504172057", "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
			TradeGroupsProcessor.GenerateFiles(publicationDate, new Logger());

			Assert.IsFalse(
				File.Exists(Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupsResponse.xml")),
				"IL_TradeGroupsResponse.xml file was not cleaned.");
			Assert.IsFalse(
				File.Exists(Path.Combine(ApplicationConfig.Instance.OutputDirectory, "ILTradeGroups.xml")),
				"IL_TradeGroups.xml file should not be generated.");

			var outputLines = consoleError.ToString().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

			Assert.That(outputLines, Has.Some.Contains("TradeGroupCountries file not found"), "Missing message");
		}

		[Test]
		public void TestGenerateFiles_WhenOnlyTradeGroupCountriesResponse()
		{
			File.Copy(
				Path.Combine(testFilesFolder, "IL_TradeGroupCountriesResponse.xml"),
				Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupCountriesResponse.xml"),
				true);

			var publicationDate = DateTime.ParseExact("20250504172057", "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
			TradeGroupsProcessor.GenerateFiles(publicationDate, new Logger());

			Assert.IsFalse(
				File.Exists(Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupCountriesResponse.xml")),
				"IL_TradeGroupCountriesResponse.xml file was not cleaned.");
			Assert.IsFalse(
				File.Exists(Path.Combine(ApplicationConfig.Instance.OutputDirectory, "ILTradeGroups.xml")),
				"ILTradeGroups.xml file should not be generated.");

			var outputLines = consoleError.ToString().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

			Assert.That(outputLines, Has.Some.Contains("TradeGroups file not found"), "Missing message");
		}

		[Test]
		public void TestGenerateFiles_WhenMultipleTradeGroupsResponses()
		{
			File.Copy(
				Path.Combine(testFilesFolder, "IL_TradeGroupsResponse.xml"),
				Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupsResponse_1.xml"),
				true);
			File.Copy(
				Path.Combine(testFilesFolder, "IL_TradeGroupsResponse.xml"),
				Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupsResponse_2.xml"),
				true);
			File.Copy(
				Path.Combine(testFilesFolder, "IL_TradeGroupCountriesResponse.xml"),
				Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupCountriesResponse.xml"),
				true);

			var publicationDate = DateTime.ParseExact("20250504172057", "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
			TradeGroupsProcessor.GenerateFiles(publicationDate, new Logger());

			Assert.IsFalse(
				File.Exists(Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupsResponse_1.xml")),
				"IL_TradeGroupsResponse_1.xml file was not cleaned.");
			Assert.IsFalse(
				File.Exists(Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupsResponse_2.xml")),
				"IL_TradeGroupsResponse_2.xml file was not cleaned.");
			Assert.IsFalse(
				File.Exists(Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupCountriesResponse.xml")),
				"IL_TradeGroupCountriesResponse.xml file was not cleaned.");
			Assert.IsFalse(
				File.Exists(Path.Combine(ApplicationConfig.Instance.OutputDirectory, "ILTradeGroups.xml")),
				"IL_TradeGroups.xml file should not be generated.");

			var outputLines = consoleError.ToString().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

			Assert.That(outputLines, Has.Some.Contains("Multiple TradeGroups files were found"), "Missing message");
		}

		[Test]
		public void TestGenerateFiles_WhenMultipleTradeGroupCountriesResponses()
		{
			File.Copy(
				Path.Combine(testFilesFolder, "IL_TradeGroupsResponse.xml"),
				Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupsResponse.xml"),
				true);
			File.Copy(
				Path.Combine(testFilesFolder, "IL_TradeGroupCountriesResponse.xml"),
				Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupCountriesResponse_1.xml"),
				true);
			File.Copy(
				Path.Combine(testFilesFolder, "IL_TradeGroupCountriesResponse.xml"),
				Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupCountriesResponse_2.xml"),
				true);

			var publicationDate = DateTime.ParseExact("20250504172057", "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
			TradeGroupsProcessor.GenerateFiles(publicationDate, new Logger());

			Assert.IsFalse(
				File.Exists(Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupsResponse.xml")),
				"IL_TradeGroupsResponse.xml file was not cleaned.");
			Assert.IsFalse(
				File.Exists(Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupCountriesResponse_1.xml")),
				"IL_TradeGroupCountriesResponse_1.xml file was not cleaned.");
			Assert.IsFalse(
				File.Exists(Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupCountriesResponse_2.xml")),
				"IL_TradeGroupCountriesResponse_2.xml file was not cleaned.");
			Assert.IsFalse(
				File.Exists(Path.Combine(ApplicationConfig.Instance.OutputDirectory, "ILTradeGroups.xml")),
				"ILTradeGroups.xml file should not be generated.");

			var outputLines = consoleError.ToString().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

			Assert.That(outputLines, Has.Some.Contains("Multiple TradeGroupCountries files were found"), "Missing message");
		}

		[Test]
		public void TestGenerateFiles_WhenNoTradeGroupsAndTradeGroupCountriesResponse()
		{
			File.Delete(Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupsResponse.xml"));
			File.Delete(Path.Combine(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupCountriesResponse.xml"));

			var publicationDate = DateTime.ParseExact("20250504172057", "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
			TradeGroupsProcessor.GenerateFiles(publicationDate, new Logger());

			Assert.IsFalse(
				File.Exists(Path.Combine(ApplicationConfig.Instance.OutputDirectory, "ILTradeGroups.xml")),
				"ILTradeGroups.xml file should not be generated.");

			var outputLines = consoleOutput.ToString().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
			Assert.That(0, Is.EqualTo(outputLines.Length));
		}

		[SetUp]
		public void Setup()
		{
			originalConsoleOutput = Console.Out;
			originalConsoleError = Console.Error;
			consoleOutput = new StringWriter();
			consoleError = new StringWriter();
			Console.SetOut(consoleOutput);
			Console.SetError(consoleError);

			testFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			string tempInputFolderName = Path.GetRandomFileName();
			string tempInputFolderPath = Path.Combine(Path.GetTempPath(), tempInputFolderName);
			Directory.CreateDirectory(tempInputFolderPath);
			ApplicationConfig.Instance.DownloadsDirectory = tempInputFolderPath;

			string tempOutputFolderName = Path.GetRandomFileName();
			string tempOutputFolderPath = Path.Combine(Path.GetTempPath(), tempOutputFolderName);
			Directory.CreateDirectory(tempOutputFolderPath);
			ApplicationConfig.Instance.OutputDirectory = tempOutputFolderPath;
		}

		[TearDown]
		public void TearDown()
		{
			Console.SetOut(originalConsoleOutput);
			Console.SetError(originalConsoleError);
			consoleOutput.Dispose();
			consoleError.Dispose();

			if (Directory.Exists(ApplicationConfig.Instance.DownloadsDirectory))
			{
				Directory.Delete(ApplicationConfig.Instance.DownloadsDirectory, true);
			}
			if (Directory.Exists(ApplicationConfig.Instance.OutputDirectory))
			{
				Directory.Delete(ApplicationConfig.Instance.OutputDirectory, true);
			}
		}

		string testFilesFolder;
		StringWriter consoleOutput;
		TextWriter originalConsoleOutput;
		StringWriter consoleError;
		TextWriter originalConsoleError;
	}
}
