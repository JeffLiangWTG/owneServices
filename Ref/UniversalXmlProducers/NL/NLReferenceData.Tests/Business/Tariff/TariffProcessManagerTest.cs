using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Business.Testing
{
	[TestFixture]
	sealed class TariffProcessManagerTest
	{
		List<RefCusTariff> nlTariffListForTest;
		List<RefCusTariff> eunTariffListForTest;

		[Test]
		public void TestDownloadTimeout()
		{
			var downloadManager = typeof(TariffProcessManager).GetField("DownloadManager", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
			var fileDownloadWrapper = typeof(DownloadManager).GetField("WebClient", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(downloadManager);
			var httpClientOverride = (HttpClient)typeof(FileDownloaderWrapper).GetField("httpClientOverride", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(fileDownloadWrapper);
			Assert.That(httpClientOverride.Timeout.TotalSeconds, Is.EqualTo(ApplicationConfig.DownloadTimeoutInSeconds));
		}

		[Test]
		public void DownloadManager()
		{
			Assert.That(typeof(TariffProcessManager).GetField("DownloadManager", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null).GetType().Equals(typeof(DownloadManager)));
		}

		[Test]
		public void RunProcess()
		{
			var xmlDocument = TestHelper.GetResourceContentAsXmlDocument("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.ValidTariffFileList.xml");
			var tempFolderTariffData = Path.GetFullPath(Path.Combine(TempFolder, "TariffData"));
			var tempFolderTariffOutputDir = Path.GetFullPath(Path.Combine(TempFolder, "TariffOutputDir"));

			var prManager = new Mock<TariffProcessManager>();
			prManager.Protected().Setup<XmlDocument>("DownloadFileList", ItExpr.IsAny<string>()).Returns(xmlDocument);
			prManager.Protected().Setup<string>("DownloadDir").Returns(tempFolderTariffData);
			prManager.Protected().Setup<string>("TariffOutputDirectory").Returns(tempFolderTariffOutputDir);
			prManager.Protected().Setup<bool>("MergeWithEUN").Returns(false);
			prManager.Protected().Setup<DateTime>("PublicationTime").Returns(new DateTime(2021, 01, 18, 13, 31, 25));
			prManager.Protected().Setup<List<string>>("DownloadZipFiles", ItExpr.IsAny<List<TariffDownloadElement>>()).Returns((List<TariffDownloadElement> tariffDownloadElements) => new List<string>());

			Directory.CreateDirectory(tempFolderTariffData);
			var file1 = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2023_12_30_22_00_11_448.zip");
			TestHelper.SimulateDownload(file1, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2023_12_30_22_00_11_448.zip");

			Directory.CreateDirectory(tempFolderTariffData);
			Directory.CreateDirectory(tempFolderTariffOutputDir);

			prManager.Object.RunProcess();

			string expectedXml, generatedXml = string.Empty;
			var expectedFileName = Path.Combine(ApplicationConfig.OutputPath, "RefCusTariff_NL Tariff_133125000.xml");
			if (File.Exists(expectedFileName))
			{
				generatedXml = File.ReadAllText(expectedFileName);
			}

			expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Output.RefCusTariffsFullTest.xml");

			Assert.IsTrue(File.Exists(expectedFileName), $"File with name '{expectedFileName}' could not be found");
			Assert.AreEqual(expectedXml, generatedXml, "Generated Universal Reference Xml does not match the correct format.");

			Assert.That(!Directory.Exists(tempFolderTariffData), "Cleanup should have run and deleted the TariffData folder.");
			Assert.That(!Directory.Exists(tempFolderTariffOutputDir), "Cleanup should have run and deleted the Tariff Output Dir.");
		}

		[Test]
		public void RunProcess_MultipleMeasureConditionCodeFiles()
		{
			var xmlDocument = TestHelper.GetResourceContentAsXmlDocument("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.ValidTariffFileList.xml");
			var tempFolderTariffData = Path.GetFullPath(Path.Combine(TempFolder, "TariffData"));
			var tempFolderTariffOutputDir = Path.GetFullPath(Path.Combine(TempFolder, "TariffOutputDir"));

			var prManager = new Mock<TariffProcessManager>();
			prManager.Protected().Setup<XmlDocument>("DownloadFileList", ItExpr.IsAny<string>()).Returns(xmlDocument);
			prManager.Protected().Setup<string>("DownloadDir").Returns(tempFolderTariffData);
			prManager.Protected().Setup<string>("TariffOutputDirectory").Returns(tempFolderTariffOutputDir);
			prManager.Protected().Setup<bool>("MergeWithEUN").Returns(false);
			prManager.Protected().Setup<DateTime>("PublicationTime").Returns(new DateTime(2021, 01, 18, 13, 31, 25));
			prManager.Protected().Setup<List<string>>("DownloadZipFiles", ItExpr.IsAny<List<TariffDownloadElement>>()).Returns((List<TariffDownloadElement> tariffDownloadElements) => new List<string>());

			Directory.CreateDirectory(tempFolderTariffData);

			var fileIncremental1 = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2025_03_03_22_00_12_590.zip");
			var fileFull1 = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2025_02_28_20_30_26_783.zip");
			var fileIncremental2 = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2025_02_25_22_00_08_973.zip");
			var fileFull2 = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2025_02_22_12_50_33_072.zip");
			TestHelper.SimulateDownload(fileIncremental1, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2025_03_03_22_00_12_590.zip");
			TestHelper.SimulateDownload(fileFull1, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2025_02_28_20_30_26_783.zip");
			TestHelper.SimulateDownload(fileIncremental2, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2025_02_25_22_00_08_973.zip");
			TestHelper.SimulateDownload(fileFull2, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2025_02_22_12_50_33_072.zip");

			Directory.CreateDirectory(tempFolderTariffOutputDir);

			prManager.Object.RunProcess();

			var expectedFileName = Path.Combine(ApplicationConfig.OutputPath, $"RefCusConditionCode_NL MeasureConditionCode_{new DateTime(2025, 02, 22, 12, 50, 33, 072):HHmmssfff}.xml");

			Assert.Multiple(() =>
			{
				Assert.That(File.Exists(expectedFileName), $"File with name '{expectedFileName}' could not be found");

				var generatedXml = File.ReadAllText(expectedFileName);
				var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Output.RefCusConditionCodes_MultipleFiles.xml");
				Assert.AreEqual(expectedXml, generatedXml, "List are only from fileIncremental1 and fileFull1, generated Universal Reference Xml does not match the correct format.");

				File.Delete(expectedFileName);
			});

		}

		[Test]
		public void DownloadFileListException()
		{
			var prManager = new TariffProcessManagerForTest();
			Assert.Throws<ProcessingException>(() => prManager.DownloadFileListExposed("DummyLink"));
		}

		[Test]
		public void DownloadZipFilesException()
		{
			var tariffDownloadElement = new TariffDownloadElement();
			tariffDownloadElement.FileName = "A dummy file";
			tariffDownloadElement.Url = "Dummy.zip";

			var list = new List<TariffDownloadElement>
			{
				tariffDownloadElement
			};

			var prManager = new TariffProcessManagerForTest();
			Assert.Throws<ProcessingException>(() => prManager.DownloadZipFilesExposed(list));
		}

		[Test]
		public void TestMergeImportTariffsWithEUNTarrifs()
		{
			SetupEUNTestData();
			var expected = new List<RefCusTariff>()
			{
				new RefCusTariff {
					ZZ1_TariffCode = "01012100",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "03011100",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "04011010",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "05010000",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "06011010",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "07019010",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "07019020",
					ZZ1_Description = "Test Description"
				}
			};

			var expander = new UniversalXMLProducers.EUNTariffDataProducer.EUNTariffExpander(eunTariffListForTest, null);
			var result = expander.MergeImportTariffsWithEUNTarrifs(nlTariffListForTest);

			Assert.AreEqual(expected.Count, result.Count);

			foreach (var item in expected)
			{
				var whereResult = result.Where(x => x.ZZ1_TariffCode == item.ZZ1_TariffCode.PadRight(10, '0')).ToList();
				Assert.AreEqual(1, whereResult.Count);
			}
		}

		public void MergeImportTariffsWithEUNTarrifsFailingTest()
		{
			SetupEUNTestData();
			var expectedToFailData = new List<RefCusTariff>()
			{
				new RefCusTariff {
					ZZ1_TariffCode = "01012100",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "03011100",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "04011010",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "05010000",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "06011010",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "07019010",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "07019020",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "13012000",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "24011035",
					ZZ1_Description = "Test Description"
				}
			};

			var expander = new UniversalXMLProducers.EUNTariffDataProducer.EUNTariffExpander(eunTariffListForTest, null);
			var result = expander.MergeImportTariffsWithEUNTarrifs(nlTariffListForTest);

			Assert.AreNotEqual(expectedToFailData.Count, result.Count);
		}

		void SetupEUNTestData()
		{
			nlTariffListForTest = new List<RefCusTariff>();
			eunTariffListForTest = new List<RefCusTariff>();

			// treat as exists in NL Data
			nlTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "01010000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "01012100", ZZ1_Description = "Test Description" });

			nlTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "03010000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "03011100", ZZ1_Description = "Test Description" });

			nlTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "04010000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "04011010", ZZ1_Description = "Test Description" });

			nlTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "05010000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "05010000", ZZ1_Description = "Test Description" });

			nlTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "06010000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "06011010", ZZ1_Description = "Test Description" });

			nlTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07000000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07019010", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07019020", ZZ1_Description = "Test Description" });

			// treat as not exists in NL Data
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "13012000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "24011035", ZZ1_Description = "Test Description" });
		}

		[SetUp]
		public void Setup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;
	}

	class TariffProcessManagerForTest : TariffProcessManager
	{
		public TariffProcessManagerForTest() : base()
		{
		}

		public XmlDocument DownloadFileListExposed(string downloadUrlTariff) => base.DownloadFileList(downloadUrlTariff);

		public List<string> DownloadZipFilesExposed(List<TariffDownloadElement> downloadElements) => base.DownloadZipFiles(downloadElements);
	}
}
