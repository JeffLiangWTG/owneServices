using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Helpers.Tests.TestHelperClasses;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders.Tests
{
	[TestFixture]
	class LoaderTests
	{
		[Test]
		public void LoadData()
		{
			var result = LoadTestData();

			var errorCollector = result.ErrorCollector;
			var models = result.Models;

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(2));
			Assert.That(errorCollector.ToString(), Is.EqualTo("Invalid Key: L1-2-INVALID\r\n"));

			Assert.That(models[0].Key, Is.EqualTo("L1-1"));
			Assert.That(models[1].Key, Is.EqualTo("L1-3"));
			Assert.That(models[1].Priority, Is.EqualTo(1));
		}

		[Test]
		public void LoadMetaInfo()
		{
			var result = LoadTestData();
			var models = result.Models;

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(2));

			Assert.That(models[0].OpType, Is.EqualTo("U"));
			Assert.That(models[0].OpDate, Is.Not.Null);
			Assert.That(models[0].OpDate, Is.EqualTo(new DateTime(2019, 10, 04, 11, 07, 54)));
		}

		(List<LoadDataOne> Models, StringBuilder ErrorCollector) LoadTestData()
		{
			var filePath = Path.Combine(ContentFolder, "GenericTestData.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.GenericTestData.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();

			var loader = new LoaderTester<LoadDataOne>();
			var models = loader.LoadData(files, errorCollector).OfType<LoadDataOne>().ToList();

			return (models, errorCollector);
		}

		#region Setup
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[SetUp]
		public void Setup()
		{
			OutputFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(OutputFolder);
			ContentFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(ContentFolder);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string OutputFolder;
		string ContentFolder;
		string TempFolder;
		#endregion
	}
}
