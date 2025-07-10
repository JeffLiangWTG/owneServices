using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class ImportSiscomexProcedureManualParserTest
	{
		[Test]
		public void TestExportToXMLFile()
		{
			var assembly = Assembly.GetExecutingAssembly();
			Directory.CreateDirectory(testOutputFilePath);
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Input.FullCollectionByDecType.xlsx"))
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Output.RefCusProcedureDecType_BR_ISW.xml"))
			{
				var publicationDate = new DateTime(2023, 06, 09, 00, 00, 00);

				var filePath = Path.Combine(testOutputFilePath, "RefCusProcedure_BR_ISW_DeclarationType.xml");
				new ImportSiscomexProcedureManualParser("BR Import Siscomex Procedure Declaration Type").ExportToXMLFile(inputStream, filePath, publicationDate);

				using (var outputStream = new MemoryStream(File.ReadAllBytes(filePath)))
				{
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
				}
			}
		}

		[SetUp]
		public void SetUp()
		{
			defOut = Console.Out;
		}

		[TearDown]
		public void TestCleanup()
		{
			Console.SetOut(defOut);

			if (Directory.Exists(testOutputFilePath))
			{
				Directory.Delete(testOutputFilePath, true);
			}
		}

		TextWriter defOut;

		string testOutputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationProvider.Configuration["OutputFolder"]);
	}
}
