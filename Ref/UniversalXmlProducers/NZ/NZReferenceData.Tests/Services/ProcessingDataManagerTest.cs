using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using CargoWise.RefDbRepo.NZReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	[TestFixture]
	class ProcessingDataManagerTest
	{
		[Test]
		public void TestLoad()
		{
			var inputFolderPath = Path.Combine(AssemblyDirectoryPath, @"Services\TestFiles\Input");
			var manager = new ProcessingDataManager<NZTariffProcessingData>(inputFolderPath, "NZTariffProcessingData.json");
			var processingData = manager.ProcessingData;
			Assert.AreEqual(new DateTime(2025, 1, 2, 3, 4, 5), processingData.LastRunDateTariff, "LastRunDateTariff");		}

		[Test]
		public void TestSave()
		{
			var actualOutputFolderPath = Path.Combine(AssemblyDirectoryPath, "TestFiles");
			Directory.CreateDirectory(actualOutputFolderPath);
			var manager = new ProcessingDataManager<NZTariffProcessingData>(actualOutputFolderPath, "NZTariffProcessingData.json");
			var processingData = manager.ProcessingData;
			processingData.LastRunDateTariff = new DateTime(2026, 9, 8, 7, 6, 5);
			manager.SaveData();

			var actualOutputFilePath = Path.Combine(actualOutputFolderPath, "NZTariffProcessingData.json");
			var actual = File.ReadAllText(actualOutputFilePath);
			var expected = File.ReadAllText(Path.Combine(AssemblyDirectoryPath, @"Services\TestFiles\Output\NZTariffProcessingData.json"));
			Assert.AreEqual(expected, actual);
			File.Delete(actualOutputFilePath);
		}

		[SetUp]
		public void Setup()
		{
			var assembly = Assembly.GetExecutingAssembly();
			AssemblyDirectoryPath = Path.GetDirectoryName(assembly.Location);
		}

		string AssemblyDirectoryPath { get; set; }
	}
}
