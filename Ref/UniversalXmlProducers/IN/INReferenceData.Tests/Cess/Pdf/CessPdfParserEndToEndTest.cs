using System;
using System.IO;
using CargoWise.RefDbRepo.INReferenceData.Business;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	public sealed class CessPdfParserEndToEndTest
	{
		[Test]
		public void TestParseFileAsJsonGeneratesExpectedOutput()
		{
			var jsonFilePath = parser.ParseFileAsJson(PdfPath, TempOutputFolder);

			Assert.IsNotNull(jsonFilePath);
			Assert.IsTrue(File.Exists(jsonFilePath), "Output JSON file was not created.");
			Assert.That(jsonFilePath, Is.EqualTo(ExpectedJsonFilePath).NoClip);

			var actualJson = File.ReadAllText(jsonFilePath);
			var expectedJsonPath = Path.Combine(AppContext.BaseDirectory, "Cess", "INTestFiles", "Output", "TheSecondSchedule.json");
			var expectedJson = File.ReadAllText(expectedJsonPath);

			Assert.That(actualJson, Is.EqualTo(expectedJson).NoClip);
		}

		[SetUp]
		public void Setup()
		{
			loggerMock = new Mock<ILogger>();
			parser = new CessPdfPigParser(loggerMock.Object);

			var baseDir = AppContext.BaseDirectory;
			PdfPath = Path.Combine(baseDir, "Cess", "INTestFiles", "Input", "TheSecondSchedule.pdf");

			TempOutputFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempOutputFolder);

			var outputFileName = Path.GetFileNameWithoutExtension(PdfPath) + ".json";
			ExpectedJsonFilePath = Path.Combine(TempOutputFolder, outputFileName);
		}

		[TearDown]
		public void Cleanup()
		{
			if (Directory.Exists(TempOutputFolder))
			{
				Directory.Delete(TempOutputFolder, recursive: true);
			}
		}

		string PdfPath;
		string TempOutputFolder;
		string ExpectedJsonFilePath;
		Mock<ILogger> loggerMock;
		CessPdfPigParser parser;
	}
}
