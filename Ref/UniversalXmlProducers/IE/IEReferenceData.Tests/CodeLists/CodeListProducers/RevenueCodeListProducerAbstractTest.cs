using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Business;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using CargoWise.RefDbRepo.IEReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	abstract class RevenueCodeListProducerAbstractTest
	{
		[Test]
		public void ExpectedXml()
		{
			var codeList = CombinedCodeListDetails == null ? new[] { CodeListDetails } : new[] { CodeListDetails, CombinedCodeListDetails };
			var downloader = new DownloadCodeLists(codeList);
			var (_, extractedCodeLists) = downloader.Download(new[] {
				(ApplicationType.AIS, aisCodeListsPdfPath),
				(ApplicationType.AISUCC5, aisUCC5CodeListsXlsxPath),
				(ApplicationType.AES, aesCodeListsPdfPath),
				(ApplicationType.NCTS, nctsCodeListsPdfPath)
			}, forceLoadFromFilePath: true);
			var extractedItem = extractedCodeLists[(DataGrouping, codeListCode)];
			var errors = new RevenueRefCusCodeListProducer(codeListCode).ConvertCodeListToXml(extractedItem.CodeList, extractedItem.VersionDate, extractedItem.UpdateType, outputPath, DataGrouping,
				requiresAttribute: ((ICodeListAttribute)extractedItem)?.IsCodeListAttributeNeeded ?? false);
			var expectedImportXml = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.IEReferenceData.Tests.CodeLists.TestFiles.Output.{ExpectedTestFileName}.xml");
			var actualXml = File.ReadAllText(actualOutputFileNameAndPath);
			Assert.Multiple(() =>
			{
				Assert.That(errors, Is.Empty, "No errors producing");
				Assert.That(actualXml, Is.EqualTo(expectedImportXml).NoClip, "Output file matches");
			});
		}

		protected virtual string DataGrouping => Constants.IECountryCode;

		protected abstract string ExpectedTestFileName { get; }

		protected abstract IRevenueCodeListDetails GetCodeListDetails();

		protected virtual IRevenueCodeListDetails GetCombinedCodeListDetails() => null;

		IRevenueCodeListDetails CodeListDetails => codeListDetails ?? (codeListDetails = GetCodeListDetails());
		IRevenueCodeListDetails codeListDetails;

		IRevenueCodeListDetails CombinedCodeListDetails => combinedCodeListDetails ?? (combinedCodeListDetails = GetCombinedCodeListDetails());
		IRevenueCodeListDetails combinedCodeListDetails;

		[OneTimeSetUp]
		public void Setup()
		{
			var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			outputPath = Path.Combine(assemblyDirectory, @"CodeLists\TestFiles\Output\");
			aisCodeListsPdfPath = Path.Combine(assemblyDirectory, @"CodeLists\TestFiles\Input\ais-cci-codelists_2025Mar.pdf");
			aisUCC5CodeListsXlsxPath = Path.Combine(assemblyDirectory, @"CodeLists\TestFiles\Input\ais-codelists_UCC5_2024December.xlsx");
			aesCodeListsPdfPath = Path.Combine(assemblyDirectory, @"CodeLists\TestFiles\Input\aes-codelists_2022September.pdf");
			nctsCodeListsPdfPath = Path.Combine(assemblyDirectory, @"CodeLists\TestFiles\Input\ncts-codelists_2024October.pdf");
			codeListCode = CodeListDetails.Code;
			actualOutputFileNameAndPath = Path.Combine(outputPath, $"RefCusCodeListZZ_{DataGrouping}_{codeListCode}.xml");
		}
		string outputPath;
		string aisCodeListsPdfPath;
		string aisUCC5CodeListsXlsxPath;
		string aesCodeListsPdfPath;
		string nctsCodeListsPdfPath;
		string codeListCode;
		string actualOutputFileNameAndPath;

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(actualOutputFileNameAndPath))
			{
				File.Delete(actualOutputFileNameAndPath);
			}
		}
	}
}
