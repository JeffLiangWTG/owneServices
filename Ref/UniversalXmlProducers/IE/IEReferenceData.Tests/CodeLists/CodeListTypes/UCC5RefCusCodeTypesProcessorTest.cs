using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Business;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using CargoWise.RefDbRepo.IEReferenceData.Tests;
using Microsoft.VisualBasic;
using NetTopologySuite.Algorithm;
using NUnit.Framework;
using static CargoWise.RefDbRepo.IEReferenceData.Services.Constants;
using DownloadCodeLists = CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.DownloadCodeLists;
using IRevenueCodeListDetails = CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.IRevenueCodeListDetails;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class UCC5RefCusCodeTypesProcessorTest
	{
		[TestCaseSource(nameof(CodeListDetailsList))]
		public void TestProcess(IRevenueCodeListDetails codeListDetails, bool shouldCreateNewCodeType)
		{
			var codeType = codeListDetails.Code;
			actualRefCusCodeListFile = Path.Combine(outputPath, $"RefCusCodeListZZ_{DataGroupings.IEUCC5}_{codeType}.xml");

			var aisUCC5CodeListsXlsxPath = Path.Combine(assemblyDirectory, @"CodeLists\TestFiles\Input\ais-codelists_UCC5_2024December.xlsx");

			var downloader = new DownloadCodeLists(new[] { codeListDetails });
			var (_, extractedCodeLists) = downloader.Download(new[] {
				(ApplicationType.AISUCC5, aisUCC5CodeListsXlsxPath),
			}, forceLoadFromFilePath: true);

			var extractedItem = extractedCodeLists[(DataGroupings.IEUCC5, codeType)];

			new RevenueRefCusCodeListProducer(codeType).ConvertCodeListToXml(
				extractedItem.CodeList,
				extractedItem.VersionDate,
				outputPath,
				DataGroupings.IEUCC5,
				UCC5RefCusCodeTypesProcessor.GetDependingOn(extractedItem)
			);
			Processor.Process(
				extractedCodeLists.Select(item => (item.Key.Code, item.Key.DataGrouping, item.Value)),
				new[] { codeListDetails },
				outputPath
			);

			var ieJsonObject = Processor.GetJsonIERefCusCodeTypes().FirstOrDefault(type => type.ZZK_CodeType == codeListDetails.Code);
			var expectedMaxLength = (int?)ieJsonObject?.ZZK_MaxLength ?? extractedCodeLists.Values.Max(v => v.CodeList.Max(w => w.Code.Length));

			var expectedRefCusCodeTypeXml = TestHelper.ReadManifestResourceContent(
				$"CargoWise.RefDbRepo.IEReferenceData.Tests.CodeLists.TestFiles.Output.RefCusCodeTypeZZ_{DataGroupings.IEUCC5}.xml"
			).Replace("CODE_TYPE_PLACE_HOLDER", ieJsonObject?.ZZK_CodeType ?? codeType)
			.Replace("CODE_TYPE_DESCRIPTION_PLACE_HOLDER", ieJsonObject?.ZZK_Description ?? codeListDetails.NameInFile)
			.Replace("CODE_TYPE_MAXLENGTH_PLACE_HOLDER", expectedMaxLength.ToString(CultureInfo.InvariantCulture));

			var actualRefCusCodeListXml = File.ReadAllText(actualRefCusCodeListFile);
			Assert.That(actualRefCusCodeListXml, Does.Contain(@"Dependency DataSource=""IE5 RefCusCodeType"""));

			if (shouldCreateNewCodeType)
			{
				var actualRefCusCodeTypeXml = File.ReadAllText(actualRefCusCodeTypeFile);
				Assert.That(actualRefCusCodeTypeXml, Is.EqualTo(expectedRefCusCodeTypeXml).NoClip, "Output file matches");
			}
			else
			{
				Assert.IsFalse(File.Exists(actualRefCusCodeTypeFile));
			}
		}

		static IEnumerable<TestCaseData> CodeListDetailsList
		{
			get
			{
				yield return new TestCaseData(new AdditionalDeclarationTypesDetails(), false);
				yield return new TestCaseData(new AdditionalFiscalRefRoleCodeDetails(), true);
			}
		}

		[OneTimeSetUp]
		public void Setup()
		{
			Processor = new UCC5RefCusCodeTypesProcessorForTesting();
			assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			outputPath = Path.Combine(assemblyDirectory, @"CodeLists\TestFiles\Output\");
			actualRefCusCodeTypeFile = Path.Combine(outputPath, $"RefCusCodeTypeZZ_{DataGroupings.IEUCC5}.xml");
			if (File.Exists(actualRefCusCodeTypeFile))
			{
				File.Delete(actualRefCusCodeTypeFile);
			}
		}
		UCC5RefCusCodeTypesProcessorForTesting Processor;
		string outputPath;
		string assemblyDirectory;
		string actualRefCusCodeTypeFile;
		string actualRefCusCodeListFile;

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(actualRefCusCodeTypeFile))
			{
				File.Delete(actualRefCusCodeTypeFile);
			}
			if (File.Exists(actualRefCusCodeListFile))
			{
				File.Delete(actualRefCusCodeListFile);
			}
		}
	}

	class UCC5RefCusCodeTypesProcessorForTesting : UCC5RefCusCodeTypesProcessor
	{
		protected override Task<List<RefCusCodeType>> AsyncGetExistingIE5RefCusCodeTypes()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var ie5ListPath = Path.Combine(binPath, @"CodeLists\TestFiles\Input\RefDBRepoUpdate_RefCusCodeTypeUpdate_IE5.json");
			return RefDataLoader.GetRefDataAsync<RefCusCodeType>(ie5ListPath);
		}

		protected override Task<List<RefCusCodeType>> AsyncGetExistingIERefCusCodeTypes()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var ieListPath = Path.Combine(binPath, @"CodeLists\TestFiles\Input\RefDBRepoUpdate_RefCusCodeTypeUpdate_IE.json");
			return RefDataLoader.GetRefDataAsync<RefCusCodeType>(ieListPath);
		}

		public List<RefCusCodeType> GetJsonIERefCusCodeTypes() => AsyncGetExistingIERefCusCodeTypes().Result;
	}
}
