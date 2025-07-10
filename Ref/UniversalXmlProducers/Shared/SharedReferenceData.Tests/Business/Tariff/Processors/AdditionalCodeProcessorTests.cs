using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests.TestHelperClasses;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors.Tests
{
	[TestFixture]
	class AdditionalCodeProcessorTests
	{
		[Test]
		public void ProcessChapters()
		{
			var filePath = Path.Combine(ContentFolder, "UT_AdditionalCode_001.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_AdditionalCode_001.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var processor = new AdditionalCodeProcessorTester();
			processor.SimulateProcessing("", files, errorCollector, "");

			Assert.That(processor.TestBuilders.First(), Is.Not.Null);
			Assert.That(processor.TestBuilders.First().PublicationDate, Is.EqualTo(file.Content.ExecutionDate));
			Assert.That(processor.TestBuilders.First().BuildCount, Is.EqualTo(3));
			Assert.That(errorCollector.ToString(), Does.Contain("Code is required"));
		}

		[Test]
		public void IsChapterSpecific()
		{
			Assert.That(sharedProcessor.IsChapterSpecific, Is.EqualTo(false));
		}

		[Test]
		public void DescriptionCleanup()
		{
			var dateTimeProvider = new Common.Tests.CommonHelpers.DateTimeProvider();
			var errorCollector = new StringBuilder();
			var builder = new VirtualBuilder();
			var processor = new AdditionalCodeProcessorTester(dateTimeProvider, new IRefXmlBuilder[] { builder });

			var dataModels = new List<ITariffModel>()
			{
				new AdditionalCode { Code = "001", CodeType = "A", Description = "Subscript X<sub>0</sub><sub>1</sub><sub>2</sub><sub>3</sub><sub>4</sub><sub>5</sub><sub>6</sub><sub>7</sub><sub>8</sub><sub>9</sub>" },
				new AdditionalCode { Code = "002", CodeType = "A", Description = "Superscript X<sup>0</sup><sup>1</sup><sup>2</sup><sup>3</sup><sup>4</sup><sup>5</sup><sup>6</sup><sup>7</sup><sup>8</sup><sup>9</sup><sup>o</sup>" },
				new AdditionalCode { Code = "003", CodeType = "A", Description = "BR-P X<br>Y<p/>Z" },
			};

			processor.Models = dataModels;

			processor.UpdateModels("XX", new List<ITariffModel>(), errorCollector);

			var models = processor.Models.Cast<AdditionalCode>().ToList();

			Assert.That(models.Count, Is.EqualTo(3));

			var sub = models.FirstOrDefault(x => x.Code == "001");
			Assert.That(sub, Is.Not.Null);
			Assert.That(sub.Description, Is.EqualTo("Subscript X\u2080\u2081\u2082\u2083\u2084\u2085\u2086\u2087\u2088\u2089"));

			var sup = models.FirstOrDefault(x => x.Code == "002");
			Assert.That(sup, Is.Not.Null);
			Assert.That(sup.Description, Is.EqualTo("Superscript X\u2070\u00B9\u00B2\u00B3\u2074\u2075\u2076\u2077\u2078\u2079\u00B0"));

			var brp = models.FirstOrDefault(x => x.Code == "003");
			Assert.That(brp, Is.Not.Null);
			Assert.That(brp.Description, Is.EqualTo("BR-P X; Y; Z"));
		}

		[Test]
		public void UpdateWithoutDescription()
		{
			var filePath = Path.Combine(ContentFolder, "UT_AdditionalCode_002.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_AdditionalCode_002.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var processor = new AdditionalCodeProcessorTester();
			processor.SimulateProcessing("", files, errorCollector, "");

			Assert.That(errorCollector.ToString(), Is.Empty);

			var models = processor.Models.Cast<AdditionalCode>().ToList();
			Assert.That(models.Count, Is.EqualTo(1));

			Assert.That(models[0].EndDate, Is.EqualTo(new DateTime(2050, 12, 31, 11, 12, 13)));
			Assert.That(models[0].Description, Is.EqualTo("Sample Additional Code"));
		}

		#region Setup
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
			sharedProcessor = new AdditionalCodeProcessorTester();
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

		internal class AdditionalCodeProcessorTester : AdditionalCodeProcessor
		{
			public AdditionalCodeProcessorTester() : this(new Common.Tests.CommonHelpers.DateTimeProvider(), new IRefXmlBuilder[] { new VirtualBuilder() }) { }
			public AdditionalCodeProcessorTester(IDateTimeProvider dateTimeProvider, IRefXmlBuilder[] builders) : base(dateTimeProvider, builders)
			{
				TestBuilders = builders.OfType<VirtualBuilder>().ToArray();
			}



			public VirtualBuilder[] TestBuilders { get; private set; }
			public new List<ITariffModel> Models { get { return base.Models; } set { base.Models = value; } }

			public void SimulateProcessing(string chapterFilter, List<FileDetails> files, StringBuilder errorCollector, string outputFolder)
			{
				LoadData(chapterFilter, files, errorCollector);
				UpdateModels(chapterFilter, null, errorCollector);
				ProcessChapter(chapterFilter, outputFolder);
			}
		}

		string OutputFolder;
		string ContentFolder;
		string TempFolder;
		AdditionalCodeProcessorTester sharedProcessor;
		#endregion
	}
}
