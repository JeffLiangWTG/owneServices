using System.IO;
using System.Text;
using NUnit.Framework;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestHelperClasses;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData
{
	[TestFixture]
	class ProcessManagerTests
	{
		[Test]
		public void RunProcess()
		{
			var errorCollector = new StringBuilder();
			var path = Path.Combine(TempFolder, "TestConfigFile");
			TestHelper.SimulateDownload(path, "CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Input.ConfigFile.xml");

			var processManager = new ProcessManagerTester();
			(processManager.TestConfigProvider as ConfigProviderTester).ConfigFile = path;

			processManager.RunProcess(string.Empty, errorCollector);

			var builder = processManager.TestPortBuilder as VirtualPortBuilder;

			Assert.That(builder, Is.Not.Null);
			Assert.That(builder.BuildCallCount, Is.EqualTo(3));
			Assert.That(builder.BuildModelCount, Is.EqualTo(39));
			Assert.That(errorCollector.ToString(), Does.Contain("Processing failure for Source 'Crasher'"));
			Assert.That(errorCollector.ToString(), Does.Contain("Warning: source 'EmptyReturn' returned no data"));

			var refdataLoader = processManager.TestRefDataLoader as VirtualRefDataLoader;
			Assert.That(refdataLoader, Is.Not.Null);
			Assert.That(refdataLoader.CallCount, Is.Not.Null);
			Assert.That(refdataLoader.CallCount.Count, Is.EqualTo(1));
			Assert.That(refdataLoader.CallCount.ContainsKey("RefCusCodeList"));
			Assert.That(refdataLoader.CallCount["RefCusCodeList"], Is.EqualTo(1));
		}

		[Test]
		public void MultpleCCSUKConfig()
		{
			var errorCollector = new StringBuilder();
			var path = Path.Combine(TempFolder, "TestConfigFile2");
			TestHelper.SimulateDownload(path, "CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Input.ConfigFile2.xml");

			var processManager = new ProcessManagerTester();
			(processManager.TestConfigProvider as ConfigProviderTester).ConfigFile = path;

			processManager.RunProcess(string.Empty, errorCollector);

			var builder = processManager.TestPortBuilder as VirtualPortBuilder;

			Assert.That(builder, Is.Not.Null);
			Assert.That(builder.BuildCallCount, Is.EqualTo(2));
			Assert.That(builder.BuildModelCount, Is.EqualTo(11));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));

			var refdataLoader = processManager.TestRefDataLoader as VirtualRefDataLoader;
			Assert.That(refdataLoader, Is.Not.Null);
			Assert.That(refdataLoader.CallCount, Is.Not.Null);
			Assert.That(refdataLoader.CallCount.Count, Is.EqualTo(1));
			Assert.That(refdataLoader.CallCount.ContainsKey("RefCusCodeList"));
			Assert.That(refdataLoader.CallCount["RefCusCodeList"], Is.EqualTo(1));
			Assert.That(builder.BuildCCSUKCount, Is.EqualTo(6 * 2)); // 6 items twice
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
		#endregion
	}
}
