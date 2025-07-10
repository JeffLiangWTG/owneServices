using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using CargoWise.RefDbRepo.NZReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	sealed class TariffParserTest
	{
		/// <summary>
		/// Use this test only locally when concession or tariff test data is added.
		/// Original data is to big to test it on DAT so this code reduces it and limit it to only 2 sections.
		/// </summary>
		[Explicit]
		[Test]
		public void FilterTestFilesToSectionOneAndFourOnly()
		{
			var assemblyDirectoryPath = Path.GetDirectoryName(assembly.Location);
			var inputFolderPath = Path.GetFullPath(assemblyDirectoryPath +
			                                       "..\\..\\..\\..\\UniversalXmlProducers\\NZ\\NZReferenceData.Tests\\Tariff\\TestFiles\\Input");
			var concessionToTariffFile = $"{inputFolderPath}/Concession/Concession_to_Tariff.csv";
			var concessionDetailsFile = $"{inputFolderPath}/Concession/Concession_Details.csv";
			var concessionRatesFile = $"{inputFolderPath}/Concession/Concession_Rates.csv";

			var tariffDetailsFile = $"{inputFolderPath}/Tariff/Tariff_Details.csv";
			var tariffLeviesFile = $"{inputFolderPath}/Tariff/Tariff_Levies.csv";
			var tariffRatesFile = $"{inputFolderPath}/Tariff/Tariff_Rates.csv";

			var concessionCodes = FilterFile(concessionToTariffFile, line => line[6] == "1" || line[6] == "4",
				line => line[0]);
			FilterFile(concessionDetailsFile, line => concessionCodes.Contains(line[0]));
			FilterFile(concessionRatesFile, line => concessionCodes.Contains(line[0]));
			var tariffs =
				FilterFile(tariffDetailsFile, line => line[6] == "1" || line[6] == "4", line => string.Concat(line.Take(5)));
			FilterFile(tariffLeviesFile, line => tariffs.Contains(string.Concat(line.Take(5))));
			FilterFile(tariffRatesFile, line => tariffs.Contains(string.Concat(line.Take(5))));

			HashSet<string> FilterFile(string fileLocation, Func<string[], bool> filterFunc, Func<string[], string> keyFunc = null)
			{
				var addedKeys = new HashSet<string>();
				var lines = File.ReadAllLines(fileLocation);
				File.WriteAllText(fileLocation, string.Empty);
				using (var writer = new StreamWriter(fileLocation))
				{
					writer.WriteLine(lines[0]);
					foreach (var line in lines.Skip(1))
					{
						var lineValues = line.Split('~');
						if (filterFunc.Invoke(lineValues))
						{
							if (keyFunc != null)
							{
								addedKeys.Add(keyFunc.Invoke(lineValues));
							}

							writer.WriteLine(line);
						}
					}
				}

				return addedKeys;
			}
		}

		[Test]
		public void TestParseTariffDetails()
		{
			var assemblyDirectoryPath = Path.GetDirectoryName(assembly.Location);
			var outputFolderPath = Path.Combine(assemblyDirectoryPath, "TestFiles");
			Directory.CreateDirectory(outputFolderPath);

			var processingData = new NZTariffProcessingData();
			var parser = new TariffParserForTest(new Logger());
			parser.Parse(null, GetDateProvider(), processingData);
			Assert.That(processingData.LastRunDateTariff, Is.EqualTo(new DateTime(2022, 12, 14, 4, 0, 0)));

			parser.ExportToXMLFile(outputFolderPath);

			var chapters = new[] { "00-09", "20-29" };

			foreach (var chapter in chapters)
			{
				var fileName = $"NZRefCusTariff_{chapter}.xml";
				var expectedImportXML = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.NZReferenceData.Tests.Tariff.TestFiles.Output." + fileName);
				var outputFilePath = Path.Combine(outputFolderPath, fileName);

				var actualOutputXml = File.ReadAllText(outputFilePath);

				Assert.AreEqual(expectedImportXML, actualOutputXml);

				var xmlDoc = XDocument.Parse(actualOutputXml);
				var refCusRateNodes = xmlDoc.Descendants("RefCusRate");
				foreach (var refCusRateNode in refCusRateNodes)
				{
					var applicabilityNode = refCusRateNode.Descendants("RefCusApplicability");
					Assert.IsNotNull(applicabilityNode, "RefCusApplicability node is missing.");
				}

				File.Delete(outputFilePath);
			}
		}

		[Test]
		public void TestParse_NotNewPublication()
		{
			var assemblyDirectoryPath = Path.GetDirectoryName(assembly.Location);
			var inputFolderPath = Path.Combine(assemblyDirectoryPath, @"Tariff\TestFiles\Input");

			var processingData = new NZTariffProcessingData()
			{
				LastRunDateTariff = new DateTime(2025, 1, 1)
			};
			var parser = new TariffParserForTest(new Logger());
			var parseResult = parser.Parse(inputFolderPath, GetDateProvider(), processingData);
			Assert.That(!parseResult);
			Assert.That( parser.DataRepo.Get(), Has.Count.EqualTo(0));
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
		}
		Assembly assembly;

		IDateProvider GetDateProvider()
		{
			return Mock.Of<IDateProvider>(x => x.Today == new DateTime(2022, 12, 30) && x.ActiveDate == x.Today.AddYears(-5));
		}
	}

	class TariffParserForTest : TariffParser
	{
		public TariffParserForTest(ILogger logger) : base(logger)
		{
		}

		protected override IBuildersProvider BuildersProvider { get; } = new BuildersProviderForTest();
	}

	class BuildersProviderForTest : IBuildersProvider
	{
		public IEnumerable<(BuildersFilePath[] FilePaths, IBuilder<NZTariffProcessingData> Builder)> GetBuilderInfos(string dir, IDateProvider dateProvider, ILogger logger)
		{
			var assemblyDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var inputFolderPath = Path.Combine(assemblyDirectoryPath, @"Tariff\TestFiles\Input");
			var tariffInputFolder = Path.Combine(inputFolderPath, "Tariff");

			yield return (
				[new BuildersFilePath(Path.Combine(tariffInputFolder, "time_stamp.txt"))],
				new TariffPublicationTimeBuilder<NZTariffProcessingData>(processingData => processingData.LastRunDateTariff, (processingData, dateTime) => processingData.LastRunDateTariff = dateTime)
			);

			yield return (
				[
					new BuildersFilePath(Path.Combine(tariffInputFolder, "Tariff_Details.csv"), BuilderFilePathSymbol.TariffDetail),
					new BuildersFilePath(Path.Combine(inputFolderPath, "NZTariffOverride.json"), BuilderFilePathSymbol.TariffOverride)
				],
				new TariffDetailsBuilder(dateProvider, logger)
			);

			yield return (
				[new BuildersFilePath(Path.Combine(tariffInputFolder, "Tariff_Rates.csv"))],
				new TariffRatesBuilder(dateProvider, logger)
			);

			yield return (
				[
					new BuildersFilePath(Path.Combine(tariffInputFolder, "Tariff_Levy_Formulas.csv"), BuilderFilePathSymbol.LevyFormula),
					new BuildersFilePath(Path.Combine(tariffInputFolder, "Tariff_Levies.csv"), BuilderFilePathSymbol.Levy)
				],
				new TariffLeviesBuilder(dateProvider, logger)
			);
		}
	}
}
