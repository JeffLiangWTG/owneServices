using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestHelperClasses;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData
{
	[TestFixture]
	class ConfigTests
	{
		[Test]
		public void LoadConfig()
		{
			var path = Path.Combine(TempFolder, "TestConfigFile");
			TestHelper.SimulateDownload(path, "CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Input.ConfigFile.xml");

			var provider = new Mock<IConfigProvider>();
			provider.Setup(x => x.ConfigFile).Returns(path);

			var configs = ConfigLoader.LoadConfigFile(provider.Object).ToList();
			Assert.That(configs, Is.Not.Null);
			Assert.That(configs.Count, Is.EqualTo(5));

			Assert.That(configs[0].Name, Is.EqualTo("Alvin"));
			Assert.That(configs[0].Code, Is.EqualTo("ABC"));
			Assert.That(configs[0].PageURL, Is.EqualTo("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Input.PageContent_003.html"));
			Assert.That(configs[0].CodeColumn, Is.EqualTo(3));
			Assert.That(configs[0].DescriptionColumns, Is.Not.Null);
			Assert.That(configs[0].DescriptionColumns.Length, Is.EqualTo(2));
			Assert.That(configs[0].DescriptionColumns[0], Is.EqualTo(0));
			Assert.That(configs[0].DescriptionColumns[1], Is.EqualTo(1));
			Assert.That(configs[0].CheckCCSUKLocation, Is.EqualTo(true));
			Assert.That(configs[0].AdditionalInfoColumn, Is.EqualTo(2));
			Assert.That(configs[0].UseODS, Is.EqualTo(false));
			Assert.That(configs[0].AnchorText, Is.EqualTo("Download CSV"));
			Assert.That(configs[0].ODSDataTag, Is.EqualTo(""));

			Assert.That(configs[1].Name, Is.EqualTo("Crasher"));

			Assert.That(configs[2].Name, Is.EqualTo("NoNameBrand"));
			Assert.That(configs[2].Code, Is.EqualTo(string.Empty));
			Assert.That(configs[2].PageURL, Is.EqualTo("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Input.PageContent_004.html"));
			Assert.That(configs[2].CodeColumn, Is.EqualTo(1));
			Assert.That(configs[2].DescriptionColumns, Is.Not.Null);
			Assert.That(configs[2].DescriptionColumns.Length, Is.EqualTo(1));
			Assert.That(configs[2].DescriptionColumns[0], Is.EqualTo(0));
			Assert.That(configs[2].CheckCCSUKLocation, Is.EqualTo(false));
			Assert.That(configs[2].AdditionalInfoColumn, Is.EqualTo(-1));
			Assert.That(configs[2].UseODS, Is.EqualTo(false));
			Assert.That(configs[2].AnchorText, Is.EqualTo("Download CSV"));
			Assert.That(configs[2].ODSDataTag, Is.EqualTo(""));

			Assert.That(configs[3].Name, Is.EqualTo("ODSTest"));
			Assert.That(configs[3].PageURL, Is.EqualTo("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Input.PageContent_005.html"));
			Assert.That(configs[3].UseODS, Is.EqualTo(true));
			Assert.That(configs[3].AnchorText, Is.EqualTo("GVMS codes"));
			Assert.That(configs[3].ODSDataTag, Is.EqualTo("Location"));
		}

		[Test]
		public void LoadActualConfig()
		{
			var provider = new ConfigProvider(ActualConfigFileName);
			Assert.That(File.Exists(provider.ConfigFile));

			var configs = ConfigLoader.LoadConfigFile(provider).ToList();
			Assert.That(configs, Is.Not.Null);
			Assert.That(configs.Count, Is.EqualTo(13));
		}

		[TestCase("None", true, "Maritime ports and wharves location codes for Data Element 5/23 of the Customs Declaration Service", "Location and Facility Name", 1)]
		[TestCase("BIP", true, "Border inspection post codes for Data Element 5/23 of the Customs Declaration Service", "Name", 2)]
		[TestCase("COA", true, "Regulated aerodrome location codes for Data Element 5/23 of the Customs Declaration Service", "Location & Facility Name ", 1)]
		[TestCase("CSE", true, "Location codes to declare goods for export at CSE premises for Data Element 5/23 of the Customs Declaration Service", "Location and facility name", 1)]
		[TestCase("DEP", true, "Designated export place (DEP) codes for Data Element 5/23 of the Customs Declaration Service", "Name", 2)]
		[TestCase("DES", true, "Location code for airports for Data Element 5/23 of the Customs Declaration Service", "Name", 1)]
		[TestCase("ETSF", true, "External temporary storage facilities codes for Data Element 5/23 of the Customs Declaration Service", "Name", 2)]
		[TestCase("ITSF", true, "Internal temporary storage facilities codes for Data Element 5/23 of the Customs Declaration Service", "Name", 2)]
		[TestCase("ITFSR", true, "Remote internal temporary storage facilities codes for Data Element 5/23 of the Customs Declaration Service", "Name", 2)]
		[TestCase("Other", true, "Other location codes for Data Element 5/23 of the Customs Declaration Service", "Location and facility name", 1)]
		[TestCase("GVMS", true, "Goods Vehicle Movement Service codes for Data Element 5/23 of the Customs Declaration Service", "Location", 2)]
		[TestCase("Rail", true, "Rail location codes for Data Element 5/23 of the Customs Declaration Service", "Name", 2)]
		[TestCase("RORO", true, "Roll on roll off ports location codes for Data Element 5/23 of the Customs Declaration Service", "Location and Facility Name", 1)]
		// Use the 'RunProcessWithActualConfig' test below to ensure config changes are working as expected
		public void CurrentConfig(string name, bool useODS, string AnchorText, string odsDataTag, int codeColumn)
		{
			var provider = new ConfigProvider(ActualConfigFileName);
			var configs = ConfigLoader.LoadConfigFile(provider).ToList();

			var currentConfig = configs.FirstOrDefault(x => x.Name == name);
			Assert.That(currentConfig, Is.Not.Null, $"Config missing for '{name}'");
			Assert.Multiple(() =>
			{
				Assert.That(currentConfig.CodeColumn, Is.EqualTo(codeColumn), $"{name}: CodeColumn");
				Assert.That(currentConfig.UseODS, Is.EqualTo(useODS), $"{name}: UseODS");

				if (useODS)
				{
					Assert.That(currentConfig.AnchorText, Is.EqualTo(AnchorText), $"{name}: AnchorText");
					Assert.That(currentConfig.ODSDataTag, Is.EqualTo(odsDataTag), $"{name}: ODSDataTag");
				}
			});
		}

		[Test]
		[Explicit("Developer test to be run if config changes are made")]
		public void RunProcessWithActualConfig()
		{
			var errorCollector = new StringBuilder();

			var webWrapper = new Services.Common.WebClientWrapper();
			var configProvider = new ConfigProvider(ActualConfigFileName);
			var downloadManager = new Services.CDSPortData.Downloader.DownloadManager(webWrapper);
			var portBuilder = new Business.CDSPortData.PortBuilder(errorCollector);
			var refDataLoader = new VirtualRefDataLoader();

			var processManager = new ProcessManagerTester
			{
				TestConfigProvider = configProvider,
				TestDownloadManager = downloadManager,
				TestPortBuilder = portBuilder,
				TestRefDataLoader = refDataLoader
			};

			var configs = ConfigLoader.LoadConfigFile(configProvider).ToList();
			var subFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			processManager.RunLocalProcess(subFolder, errorCollector);
			var files = Directory.GetFiles(subFolder);
			Assert.That(files, Is.Not.Null);

			foreach (var cfg in configs)
			{
				if (!files.Any(x => x.Contains($"RefCusCodeList_{cfg.Name}_")))
				{
					Assert.Fail($"No file created for '{cfg.Name}'");
				}
			}

			Assert.That(errorCollector.ToString(), Does.Not.Contain("Error"));
		}

		#region Setup
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;
		const string ActualConfigFileName = "CDSPortConfig.xml";
		#endregion
	}
}
