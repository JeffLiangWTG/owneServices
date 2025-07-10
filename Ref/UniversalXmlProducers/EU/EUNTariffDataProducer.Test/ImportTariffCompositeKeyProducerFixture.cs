using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class ImportTariffCompositeKeyProducerFixture
	{
		[Test]
		public void GenerateImportTariffCompositeKeyXmlFile()
		{
			var tariffs = new List<RefCusTariff>()
			{
				new RefCusTariff { ZZ1_TariffCode = "10101010", ZZ1_CompositeKeyOnZZ5 = "10.10" },
				new RefCusTariff { ZZ1_TariffCode = "4407919000", ZZ1_CompositeKeyOnZZ5 = "09.44..07.9.1.20.20" }
			};
			var producer = new ImportTariffCompositeKeyProducer();
			Assert.DoesNotThrow(() => producer.Run(tariffs));
			Assert.True(File.Exists(outputFilePath));
			var expectedXml = File.ReadAllText(testFilesPath);
			var generatedXml = File.ReadAllText(outputFilePath);
			Assert.AreEqual(expectedXml, generatedXml);
		}

		[SetUp]
		public void Setup()
		{
			ApplicationConfig.ConfigEnvironment();
			outputFilePath = ApplicationConfig.ImportTariffCompositeKeyUXmlFile;
			ApplicationConfig.SetPublishTime(new DateTime(2022, 8, 4));
			testFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "ImportTariffCompositeKeyTest.xml");
		}
		string outputFilePath;
		string testFilesPath;

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(outputFilePath))
			{
				File.Delete(outputFilePath);
			}
		}
	}
}
