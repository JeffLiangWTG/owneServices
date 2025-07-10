using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.CmdLine;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class CMRReferenceDataProgramTest
	{
		[Test]
		public void TestRunFullParsersTriggersDownloadAndParse()
		{
			var parserTypes = new List<Type>()
			{
				typeof(AQISCommodityCodesParser),
				typeof(AQISConcernCodesParser),
				typeof(AQISDocumentTypesParser),
				typeof(AQISEntityCodesParser),
				typeof(AQISPremisesCodesParser),
				typeof(AQISProcessingTypeParser),
				typeof(AQISProducerCodesParser),
				typeof(BerthCodesParser),
				typeof(CharacteristicCodesParser),
				typeof(EstablishmentCodesParser),
				typeof(PreferenceRulesParser),
				typeof(PreferenceRulesTestParser),
				typeof(RefCusTradeGroupParser),
				typeof(RefundReasonCodesParser),
			};

			var parserMocks = parserTypes.Select(CreateParserMock).ToArray();
			var fileNames = string.Join("\n", parserMocks.Select(m => $"{m.Object.FileNamePrefix}-2505270905.txt"));
			var outputFolder = Path.GetTempPath();

			var mockHttpClientHelper = new Mock<IHttpClientHelper>();
			mockHttpClientHelper
				.Setup(x => x.GetWebPageAsync(It.Is<string>(uri => uri.EndsWith("/reference/production/main/"))))
				.Returns<string>(x => Task.FromResult(fileNames));


			var program = new CMRReferenceDataProgramForTest(mockHttpClientHelper.Object, outputFolder)
			{
				Parsers = parserMocks.Select(m => m.Object).ToArray(),
			};
			program.Run();

			mockHttpClientHelper.Verify(
				x => x.GetWebPageAsync(It.Is<string>(uri => uri.EndsWith("/reference/production/main/"))),
				Times.Exactly(parserMocks.Length), $"Reference files should be downloaded {parserMocks.Length} times");

			foreach (var parserMock in parserMocks)
			{
				parserMock.Verify(p => p.Parse(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>()), Times.Once, $"{parserMock.Name}.Parse() should be called during run");
			}

			ApplicationConfigTestHelper.ResetApplicationConfig();
		}

		[Test]
		public void TestPartialRunsOncePerDay()
		{
			var outputFolder = Path.GetTempPath();
			var outputFilePath = Path.Combine(outputFolder, "RefCusCodeList_AU_AQISPremisesCodes_Partial.xml");
			var processingDataFilePath = Path.Combine(outputFolder, "AUCustomsProcessingData", "PartialParserForTestProcessingData.json");

			DeleteIfExists(outputFilePath);
			DeleteIfExists(processingDataFilePath);

			var mockHttpClientHelper = new Mock<IHttpClientHelper>();
			mockHttpClientHelper.Setup(x => x.GetWebPageAsync(It.Is<string>(uri => uri.EndsWith("/reference/production/change/today/"))))
								.Returns<string>(x => Task.FromResult("<A HREF=\"AQSPREMS-P1-EDCHNG-2501020001.txt\">"));
			mockHttpClientHelper.Setup(x => x.GetWebPageAsync(It.Is<string>(uri => uri.Contains("AQSPREMS-P1-EDCHNG-2501020001"))))
								.Returns<string>(x => Task.FromResult(""));

			var runTime = new DateTime(2025, 01, 02, 04, 15, 00);

			try
			{
				var program = new CMRReferenceDataProgramForTest(mockHttpClientHelper.Object, outputFolder)
				{
					RunTimeOverride = runTime,
					PartialParsers = new[] { new PartialParserForTest() }
				};
				program.RunPartial();

				var processingDataContent = File.ReadAllText(processingDataFilePath);
				Assert.IsTrue(processingDataContent.Contains("\"PartialParserLastRun\": \"2025-01-02T04:15:00\""));
				Assert.IsTrue(File.Exists(outputFilePath));
				File.Delete(outputFilePath);

				program.RunTimeOverride = runTime.AddHours(12);
				program.RunPartial();

				processingDataContent = File.ReadAllText(processingDataFilePath);
				Assert.IsTrue(processingDataContent.Contains("\"PartialParserLastRun\": \"2025-01-02T04:15:00\""));
				Assert.IsFalse(File.Exists(outputFilePath), "Does not run twice in one day");

				program.RunTimeOverride = runTime.AddDays(1);
				program.RunPartial();

				processingDataContent = File.ReadAllText(processingDataFilePath);
				Assert.IsTrue(processingDataContent.Contains("\"PartialParserLastRun\": \"2025-01-03T04:15:00\""));
				Assert.IsTrue(File.Exists(outputFilePath));
			}
			finally
			{
				DeleteIfExists(outputFilePath);
				DeleteIfExists(processingDataFilePath);
			}
		}

		[Test]
		public void TestPartialSkipsOnException()
		{
			var outputFolder = Path.GetTempPath();
			var outputFilePath = Path.Combine(outputFolder, "RefCusCodeList_AU_AQISPremisesCodes_Partial.xml");
			var processingDataFilePath = Path.Combine(outputFolder, "AUCustomsProcessingData", "PartialParserForTestProcessingData.json");
			var exceptionProcessingDataFilePath = Path.Combine(outputFolder, "AUCustomsProcessingData", "PartialParserThatThrowsProcessingData.json");

			DeleteIfExists(outputFilePath);
			DeleteIfExists(processingDataFilePath);
			DeleteIfExists(exceptionProcessingDataFilePath);

			var mockHttpClientHelper = new Mock<IHttpClientHelper>();
			mockHttpClientHelper.Setup(x => x.GetWebPageAsync(It.Is<string>(uri => uri.EndsWith("/reference/production/change/today/"))))
								.Returns<string>(x => Task.FromResult("<A HREF=\"AQSPREMS-P1-EDCHNG-2501020001.txt\">"));
			mockHttpClientHelper.Setup(x => x.GetWebPageAsync(It.Is<string>(uri => uri.Contains("AQSPREMS-P1-EDCHNG-2501020001"))))
								.Returns<string>(x => Task.FromResult(""));

			var runTime = new DateTime(2025, 01, 02, 04, 15, 00);

			using (var stringWriter = new StringWriter())
			{
				var originalOut = Console.Out;
				try
				{
					Console.SetOut(stringWriter);
					var program = new CMRReferenceDataProgramForTest(mockHttpClientHelper.Object, outputFolder)
					{
						RunTimeOverride = runTime,
						PartialParsers = new[]
					{
						(ICMRDataParser)new PartialParserThatThrows(),
						(ICMRDataParser)new PartialParserForTest()
					}
					};
					program.RunPartial();

					Assert.IsFalse(File.Exists(exceptionProcessingDataFilePath), "Not created due to exception");

					var processingDataContent = File.ReadAllText(processingDataFilePath);
					Assert.IsTrue(processingDataContent.Contains("\"PartialParserLastRun\": \"2025-01-02T04:15:00\""));
					Assert.IsTrue(File.Exists(outputFilePath));

					var output = stringWriter.ToString().Trim();
					StringAssert.Contains("PartialParserThatThrows failed. Dummy Exception", output);
				}
				finally
				{
					Console.SetOut(originalOut);
					DeleteIfExists(outputFilePath);
					DeleteIfExists(processingDataFilePath);
					DeleteIfExists(exceptionProcessingDataFilePath);
				}
			}
		}

		static Mock<ICMRDataParser> CreateParserMock(Type parserType)
		{
			var mock = new Mock<ICMRDataParser>();
			var baseName = parserType.Name.Replace("Parser", "");

			string prefixPropertyName = $"{baseName}FilePrefix";
			var prefixProperty = typeof(ApplicationConfig).GetProperty(prefixPropertyName, BindingFlags.Public | BindingFlags.Static);
			var filePrefix = prefixProperty?.GetValue(null)?.ToString() ?? "UnknownPrefix";

			mock.SetupGet(p => p.FileNamePrefix).Returns(filePrefix);
			mock.SetupGet(p => p.IndexUri).Returns(ApplicationConfig.AUReferenceFilesDirectory);
			mock.Setup(p => p.Parse(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>()));

			return mock;
		}

		void DeleteIfExists(string filePath)
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}

		class CMRReferenceDataProgramForTest : CMRReferenceDataProgram
		{
			public CMRReferenceDataProgramForTest(IHttpClientHelper httpClient, string outputDirectory) : base()
			{
				this.httpClient = httpClient;
				this.outputDirectory = outputDirectory;
			}
			IHttpClientHelper httpClient;
			string outputDirectory;

			public DateTime RunTimeOverride;
			public ICMRDataParser[] Parsers;
			public ICMRDataParser[] PartialParsers;

			protected override IHttpClientHelper HttpClient => httpClient;
			protected override string OutputDirectory => outputDirectory;
			protected override DateTime RunTime => RunTimeOverride;
			protected override IEnumerable<ICMRDataParser> RefDataParsers => Parsers;
			protected override IEnumerable<ICMRDataParser> RefDataPartialParsers => PartialParsers;
		}

		public class PartialParserForTest : AQISPremisesCodesPartialParser
		{
			protected override void ParseCore(IXmlWriter xmlWriter, string content)
			{ }
		}

		public class PartialParserThatThrows : AQISPremisesCodesPartialParser
		{
			protected override void ParseCore(IXmlWriter xmlWriter, string content)
			{
				throw new UnhandledApplicationException("Dummy Exception");
			}
		}
	}
}
