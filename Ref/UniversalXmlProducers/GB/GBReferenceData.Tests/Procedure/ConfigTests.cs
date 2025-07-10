using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Config;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure
{
	[TestFixture]
	class ConfigTests
	{
		[Test]
		public void TestLoadConfig_MissingFile()
		{
			var path = Path.Combine(TempFolder, "InvalidConfigFileName");

			var provider = new Mock<IConfigProvider>();
			provider.Setup(x => x.ConfigFile).Returns(path);

			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.StartsWith("Failed to load configuration file:"),
				() => ConfigLoader.LoadConfigFile(provider.Object));
		}

		[Test]
		public void TestLoadConfig_MalformedXml()
		{
			var path = Path.Combine(TempFolder, "MalformedConfigFileName");
			TestHelper.SimulateDownload(path, "CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Input.ConfigFile_Malformed.xml");

			var provider = new Mock<IConfigProvider>();
			provider.Setup(x => x.ConfigFile).Returns(path);

			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.StartsWith("Failed to load configuration file:"),
				() => ConfigLoader.LoadConfigFile(provider.Object));
		}

		[Test]
		public void TestLoadConfig_NonXmlFormat()
		{
			var path = Path.Combine(TempFolder, "NonXmlConfigFileName");
			TestHelper.SimulateDownload(path, "CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Input.ConfigFile_NonXml.xml");

			var provider = new Mock<IConfigProvider>();
			provider.Setup(x => x.ConfigFile).Returns(path);

			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.StartsWith("Failed to load configuration file:"),
				() => ConfigLoader.LoadConfigFile(provider.Object));
		}

		[Test]
		public void TestLoadConfig_ValidFile_Single()
		{
			var path = Path.Combine(TempFolder, "ValidConfigSingleSource");
			TestHelper.SimulateDownload(path, "CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Input.ConfigFile_ValidSingle.xml");

			var provider = new Mock<IConfigProvider>();
			provider.Setup(x => x.ConfigFile).Returns(path);
			var configs = ConfigLoader.LoadConfigFile(provider.Object).ToList();
			Assert.That(configs, Is.Not.Null);
			Assert.That(configs.Count, Is.EqualTo(1));

			Assert.That(configs[0].Name, Is.EqualTo("Import"));
			Assert.That(configs[0].ProcedureUrl, Is.EqualTo("https://www.gov1.uk"));
			Assert.That(configs[0].AdditionalProcedureUrl, Is.EqualTo("https://www.gov2.uk"));
			Assert.That(configs[0].AdditionalProcedureMatrixUrl, Is.EqualTo("https://www.gov3.uk"));
			Assert.That(configs[0].ShipmentType, Is.EqualTo("IMP"));
		}

		[Test]
		public void TestLoadConfig_ValidFile_Multi()
		{
			var path = Path.Combine(TempFolder, "ValidConfigMultiSource");
			TestHelper.SimulateDownload(path, "CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Input.ConfigFile_ValidMulti.xml");

			var provider = new Mock<IConfigProvider>();
			provider.Setup(x => x.ConfigFile).Returns(path);
			var configs = ConfigLoader.LoadConfigFile(provider.Object).ToList();
			Assert.That(configs, Is.Not.Null);
			Assert.That(configs.Count, Is.EqualTo(2));

			Assert.That(configs[0].Name, Is.EqualTo("Import"));
			Assert.That(configs[0].ProcedureUrl, Is.EqualTo("https://www.gov1.uk"));
			Assert.That(configs[0].AdditionalProcedureUrl, Is.EqualTo("https://www.gov2.uk"));
			Assert.That(configs[0].AdditionalProcedureMatrixUrl, Is.EqualTo("https://www.gov3.uk"));
			Assert.That(configs[0].ShipmentType, Is.EqualTo("IMP"));

			Assert.That(configs[1].Name, Is.EqualTo("Export"));
			Assert.That(configs[1].ProcedureUrl, Is.EqualTo("https://www.gov4.uk"));
			Assert.That(configs[1].AdditionalProcedureUrl, Is.EqualTo("https://www.gov5.uk"));
			Assert.That(configs[1].AdditionalProcedureMatrixUrl, Is.EqualTo("https://www.gov6.uk"));
			Assert.That(configs[1].ShipmentType, Is.EqualTo("EXP"));
		}

		[Test]
		public void TestLoadConfig_ActualConfig()
		{
			var provider = new ConfigProvider(ActualConfigFileName);
			Assert.That(File.Exists(provider.ConfigFile));

			var configs = ConfigLoader.LoadConfigFile(provider).ToList();
			Assert.That(configs, Is.Not.Null);
			Assert.That(configs.Count, Is.EqualTo(5));
			foreach (var procedureSource in configs)
			{
				Assert.That(procedureSource, Is.Not.EqualTo(string.Empty));
			}
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
		const string ActualConfigFileName = "CDSProcedureConfig.xml";
		#endregion
	}
}
