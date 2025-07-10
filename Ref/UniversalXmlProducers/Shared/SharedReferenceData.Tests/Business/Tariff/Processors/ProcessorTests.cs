using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Helpers.Tests;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Common.Tests.CommonHelpers;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests.TestHelperClasses;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors.Tests
{
	[TestFixture]
	class ProcessorTests
	{
		[Test]
		public void LoadData()
		{
			var filePath = Path.Combine(ContentFolder, "GenericTestData.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.GenericTestData.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();

			var processor = new ProcessorTester<TestHelperClasses.ProcessDataOne>(new DateTimeProvider(), new IRefXmlBuilder[] { new VirtualBuilder() });
			processor.LoadData("1", files, errorCollector);
			var models = processor.Models.Cast<TestHelperClasses.ProcessDataOne>().ToList();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(2));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));

			errorCollector.Clear();
			processor.LoadData("2", files, errorCollector);
			models = processor.Models.Cast<TestHelperClasses.ProcessDataOne>().ToList();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(1));
			Assert.That(errorCollector.ToString(), Is.EqualTo("Invalid Key: P1-3-INVALID\r\n"));
		}

		[Test]
		public void SimulateProcess()
		{
			var filePath = Path.Combine(ContentFolder, "GenericTestData.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.GenericTestData.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var dateTimeProvider = new DateTimeProvider();
			var builder = new VirtualBuilder();
			var processor = new ProcessorTester<TestHelperClasses.ProcessDataOne>(dateTimeProvider, new IRefXmlBuilder[] { builder });

			var refData = new List<ITariffModel> { new TestHelperClasses.LoadDataOne() { Key = "L1-2" }, new TestHelperClasses.LoadDataTwo() { Key = "L2" } };

			processor.SimulateProcessing("1", files, errorCollector, refData, "");
			processor.SimulateProcessing("2", files, errorCollector, refData, "");

			var results = builder.BuiltContent;
			Assert.That(results.Count, Is.EqualTo(2));
			Assert.That(results[0], Is.EqualTo("P1-1.x.y.z, P1-2.L1-2.y.z"));
			Assert.That(results[1], Is.EqualTo("P1-4.x.y.z"));
		}

		[Test]
		public void IsChapterSpecific()
		{
			var processor = new ProcessorTester<TestHelperClasses.ProcessDataOne>(new DateTimeProvider(), new IRefXmlBuilder[] { new VirtualBuilder() });

			Assert.That(processor.IsChapterSpecific, Is.EqualTo(true), "Should be Chpter specific by default");
			processor.TestIsChapterSpecific = false;
			Assert.That(processor.IsChapterSpecific, Is.EqualTo(false), "But can be overridden");
		}

		[Test]
		public void MultipleBuilders()
		{
			var filePath = Path.Combine(ContentFolder, "GenericTestData.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.GenericTestData.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var dateTimeProvider = new DateTimeProvider();
			var builder1 = new VirtualBuilder();
			var builder2 = new VirtualBuilder();
			var processor = new ProcessorTester<TestHelperClasses.ProcessDataOne>(dateTimeProvider, new IRefXmlBuilder[] { builder1, builder2 });

			var refData = new List<ITariffModel> { new TestHelperClasses.LoadDataOne() { Key = "L1-2" }, new TestHelperClasses.LoadDataTwo() { Key = "L2" } };

			Assert.That(processor.TestBuilders, Is.Not.Null);
			Assert.That(processor.TestBuilders.Count, Is.EqualTo(2));

			processor.SimulateProcessing("1", files, errorCollector, refData, "");

			Assert.That(processor.TestBuilders[0].CallCount, Is.EqualTo(1));
			Assert.That(processor.TestBuilders[1].CallCount, Is.EqualTo(1));
		}

		[Test]
		public void EmptyBuilders()
		{
			var filePath = Path.Combine(ContentFolder, "GenericTestData.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.GenericTestData.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();

			var processor = new ProcessorTester<TestHelperClasses.ProcessDataOne>(new DateTimeProvider(), Array.Empty<VirtualBuilder>());
			processor.LoadData("1", files, errorCollector);
			var models = processor.Models.Cast<TestHelperClasses.ProcessDataOne>().ToList();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(2));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));

			errorCollector.Clear();
			processor.LoadData("2", files, errorCollector);
			models = processor.Models.Cast<TestHelperClasses.ProcessDataOne>().ToList();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(1));
			Assert.That(errorCollector.ToString(), Is.EqualTo("Invalid Key: P1-3-INVALID\r\n"));

			Assert.That(processor.TestBuilders, Is.Not.Null);
			Assert.That(processor.TestBuilders.Count, Is.EqualTo(0));

			var refData = new List<ITariffModel> { new TestHelperClasses.LoadDataOne() { Key = "L1-2" }, new TestHelperClasses.LoadDataTwo() { Key = "L2" } };
			Assert.DoesNotThrow(() =>processor.SimulateProcessing("1", files, errorCollector, refData, ""));
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
