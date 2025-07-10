using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Business.CDSPortData;
using CargoWise.RefDbRepo.GBReferenceData.Services;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Downloader;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.RefDbService;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using NUnit.Framework;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestHelperClasses;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData
{
	[TestFixture]
	class DeveloperTests
	{
		[Test]
		[Explicit("Debug/Developer test")]
		public void Dev_Step1_CreateFiles()
		{
			var errorCollector = new StringBuilder();

			var webWrapper = new WebClientWrapper();
			var configProvider = new ConfigProvider(ConfigFileName);
			var downloadManager = new DownloadManager(webWrapper);
			var portBuilder = new PortBuilder(errorCollector);
			var refDataLoader = new RefDataLoader(ConfigurationProvider.RefDbServiceURI, ConfigurationProvider.IsRefDbServiceSecure);

			var processManager = new ProcessManagerTester();
			processManager.TestConfigProvider = configProvider;
			processManager.TestDownloadManager = downloadManager;
			processManager.TestPortBuilder = portBuilder;
			processManager.TestRefDataLoader = refDataLoader;

			processManager.RunLocalProcess(@"C:\Temp\CDSPortData", errorCollector);

			Console.WriteLine($"Errors:\n{errorCollector}");

			Assert.That(errorCollector.ToString(), Does.Not.Contain("duplicate"));
		}

		[Test]
		[Explicit("Debug/Developer Test")]
		public void Dev_Step2_ImportIntoStaging()
		{
			var di = new DirectoryInfo(@"C:\Temp\CDSPortData");
			//var di = new DirectoryInfo(@"C:\RefDataRepo\RefDataRepo\Bin\UXmlFiles");

			foreach (var f in di.GetFiles("*.xml"))
			{
				var proc = System.Diagnostics.Process.Start(@"C:\RefDataRepo\RefDataRepo\Bin\Staging\CargoWise.RefDbRepo.UniversalXmlProcessor.exe", f.FullName);

				proc.WaitForExit(1000 * 60);
			}
		}

		[Test]
		[Explicit("Debug/Developer Test")]
		public void Dev_GetCCSUKData()
		{
			var refDataLoader = new RefDataLoader(ConfigurationProvider.RefDbServiceURI, ConfigurationProvider.IsRefDbServiceSecure);
			var ccsuk = new CCSUKLocationLoader(refDataLoader);

			var data = ccsuk.GetLocations().Result;

			var str = string.Join("\n", data.Select(x => x.Code));
			File.WriteAllText(@"C:\temp\CDSPortData\CCSUKData.txt", str);
		}

		const string ConfigFileName = "CDSPortConfig.xml";
	}
}
