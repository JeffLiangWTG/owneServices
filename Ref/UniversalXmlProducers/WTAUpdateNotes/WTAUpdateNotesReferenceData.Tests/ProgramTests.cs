using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.WTAUpdateNotesReferenceData.CmdLine;
using Moq;
using NUnit.Framework;
using WiseTechAcademy.UpdateNotesContract;
using static CargoWise.RefDbRepo.WTAUpdateNotesReferenceData.Tests.TestHelper;

namespace CargoWise.RefDbRepo.WTAUpdateNotesReferenceData.Tests
{
	[TestFixture]
	public class ProgramTests
	{
		static readonly DateTime currentDateTime = DateTime.ParseExact("24-Feb-2025 3:00:16 PM", "dd-MMM-yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
		const string BaseTestFilePath = "CargoWise.RefDbRepo.WTAUpdateNotesReferenceData.Tests.TestFiles";
		TempDirectory tempDirectory;

		[SetUp]
		public void Setup()
		{
			var testConfig = AppConfig.Config;
			tempDirectory = new TestHelper.TempDirectory();
			testConfig["OutputPath"] = tempDirectory.FullPath;
			AppConfig.OverrideConfig(testConfig);
		}

		[TearDown]
		public void Teardown()
		{
			tempDirectory?.Dispose();
		}

		static readonly object[] ArgumentTestCases =
		[
			new object[] { new string[] {  }, "There should be a single argument that is 'full' or 'partial'.", typeof(ArgumentException)},
			new object[] { new string[] { "InvalidArg1" }, "There should be a single argument that is 'full' or 'partial'.", typeof(ArgumentException) },
			new object[] { new string[] { "full" }, "", null },
			new object[] { new string[] { "Full" }, "", null },
			new object[] { new string[] { "partial" }, "", null },
			new object[] { new string[] { "Partial" }, "", null },
			new object[] { new string[] { "InvalidArg1", "InvalidArg2" }, "There should be a single argument that is 'full' or 'partial'.", typeof(ArgumentException) },
			new object[] { new string[] { "Full", "InvalidArg2" }, "There should be a single argument that is 'full' or 'partial'.", typeof(ArgumentException) },
			new object[] { new string[] { "Partial", "InvalidArg2" }, "There should be a single argument that is 'full' or 'partial'.", typeof(ArgumentException) },
			new object[] { new string[] { "InvalidArg1", "Partial" }, "There should be a single argument that is 'full' or 'partial'.", typeof(ArgumentException) },
		];
		
		[TestCaseSource(nameof(ArgumentTestCases))]
		public void ArgumentTest(string[] args, string expectedOutput, Type exceptionType)
		{
			var apiReturnValue = "";

			var mockedXmlGenerator = new Mock<IXmlGenerator>();
			mockedXmlGenerator.Setup(mx => mx.CreateToken()).Returns(Task.FromResult("TestToken"));
			mockedXmlGenerator.Setup(mx => mx.CallApi(It.IsAny<string>())).Returns(Task.FromResult(apiReturnValue));

			XmlGenerator.instance = mockedXmlGenerator.Object;

			if (exceptionType != null)
			{
				Assert.ThrowsAsync(exceptionType, async () => await Program.Main(args), expectedOutput);
			}
			else
			{
				Assert.DoesNotThrowAsync(async () => await Program.Main(args));
			}
		}

		static readonly List<UpdateNoteData> CorrectOutputUpdateNoteData =
			[
				new()
				{
					GF_MinVersion = "1.2.3.4",
					GF_QS_PK = new Guid(),
					GF_ReleaseNoteDate = currentDateTime,
					GF_Section = "Wesley",
					GF_Summary = "The Princess Bride",
					GF_URL = "as_you_wish",
					IsInsertion = true,
				},
				new()
				{
					GF_MinVersion = null,
					GF_QS_PK = new Guid(),
					GF_ReleaseNoteDate = currentDateTime.AddDays(-2),
					GF_Section = "Now you see me",
					GF_Summary = "Now you dont",
					GF_URL = "nowyouseememovies.com",
					IsInsertion = false,
				},
				new()
				{
					GF_MinVersion = "59.52.43.1233",
					GF_QS_PK = new Guid(),
					GF_ReleaseNoteDate = currentDateTime.AddDays(-4),
					GF_Section = "Aang",
					GF_Summary = "Avatar",
					GF_URL = "LastAirbender.game",
					IsInsertion = true,
				}
			];

		static readonly object[] CorrectOutputTestCases =
		[
			new object[]
			{
				UpdateRunType.Full,
				UpdateType.Full,
				"fullCorrectXml.xml",
				CorrectOutputUpdateNoteData,
			},
			new object[]
			{
				UpdateRunType.Partial,
				UpdateType.Partial,
				"partialCorrectXml.xml",
				CorrectOutputUpdateNoteData,
			},
			new object[]
			{
				UpdateRunType.Partial,
				UpdateType.Deletion,
				"deleteCorrectXml.xml",
				CorrectOutputUpdateNoteData,
			}
		];

		[TestCaseSource(nameof(CorrectOutputTestCases))]
		public async Task CorrectXmlTest(UpdateRunType runType, UpdateType updateType, string outputFileName, IEnumerable<UpdateNoteData> inputUpdateNotesList)
		{
			var outputFileData = TestHelper.ReadManifestResourceContent($"{BaseTestFilePath}.{outputFileName}");

			using var xmlGenerator = new XmlGenerator(currentDateTime);
			xmlGenerator.ManageXmlCreation(inputUpdateNotesList, runType);

			var fileNameTime = updateType == UpdateType.Deletion ? currentDateTime.AddMinutes(-1) : currentDateTime;

			var xmlFileName = XmlGenerator.GetXmlFileName(fileNameTime, updateType);
			var outputData = await File.ReadAllTextAsync(Path.Combine(AppConfig.OutputPath, xmlFileName));
			Assert.That(outputData, Is.EqualTo(outputFileData));
		}

		[Test]
		public void NoXmlTest()
		{
			var runType = UpdateRunType.Full;
			var updateType = UpdateType.Full;
			var emptyList = new List<UpdateNoteData>();
			using var xmlGenerator = new XmlGenerator(currentDateTime);
			xmlGenerator.ManageXmlCreation(emptyList, runType);

			var fileNameTime = updateType == UpdateType.Deletion ? currentDateTime.AddMinutes(-1) : currentDateTime;

			var xmlFileName = XmlGenerator.GetXmlFileName(fileNameTime, updateType);
			var outputFilePath = Path.Combine(AppConfig.OutputPath, xmlFileName);
			Assert.That(File.Exists(outputFilePath), Is.EqualTo(false));
		}
	}
}
