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
	class GeographicalAreaProcessorTests
	{
		[Test]
		public void ProcessChapters()
		{
			var filePath = Path.Combine(ContentFolder, "UT_GeographicalArea_001.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_GeographicalArea_001.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var processor = new GeographicalAreaProcessorTester();
			processor.SimulateProcessing("", files, errorCollector, "");

			Assert.That(processor.TestBuilders.First(), Is.Not.Null);
			Assert.That(processor.TestBuilders.First().PublicationDate, Is.EqualTo(file.Content.ExecutionDate));
			Assert.That(processor.TestBuilders.First().BuildCount, Is.EqualTo(4));
			Assert.That(errorCollector.ToString(), Does.Contain("GeographicalAreaId is required"));
		}

		[Test]
		public void UpdateModels()
		{
			var dateTimeProvider = new Common.Tests.CommonHelpers.DateTimeProvider();
			var errorCollector = new StringBuilder();
			var builder = new VirtualBuilder();
			builder.TestDateTimeProvider = dateTimeProvider;
			var processor = new GeographicalAreaProcessorTester(dateTimeProvider, new IRefXmlBuilder[] { builder });

			var dataModels = new List<ITariffModel>()
			{
				new GeographicalArea { HJID = "0001", GeographicalAreaId = "ZA", Description = "South Africa" },
				new GeographicalArea { HJID = "0002", GeographicalAreaId = "AU", Description = "Australia" },
				new GeographicalArea
				{
					HJID = "0003", GeographicalAreaId = "GROUP"
				}
				.SetCountries(new[]
				{
					new GeographicalArea.GeographicalAreaCountry { HJID = "1001", GeographicalAreaHjid = "0001" },
					new GeographicalArea.GeographicalAreaCountry { HJID = "1002", GeographicalAreaHjid = "0002" },
					new GeographicalArea.GeographicalAreaCountry { HJID = "1003", GeographicalAreaHjid = "0004" }
				})
			};

			var refData = new List<ITariffModel>() { };

			processor.Models = dataModels;

			processor.UpdateModels(string.Empty, refData, errorCollector);

			var models = processor.Models.Cast<GeographicalArea>().ToList();

			Assert.That(models.Count, Is.EqualTo(3));
			var grp = models.FirstOrDefault(x => x.GeographicalAreaId == "GROUP");
			Assert.That(grp, Is.Not.Null);
			Assert.That(grp.Countries.Any(x => x.GeographicalAreaHjid == "0001" && x.CountryCode == "ZA"));
			Assert.That(grp.Countries.Any(x => x.GeographicalAreaHjid == "0002" && x.CountryCode == "AU"));
			Assert.That(grp.Countries.Any(x => x.GeographicalAreaHjid == "0004" && string.IsNullOrEmpty(x.CountryCode)));
			Assert.That(grp.Countries.Single(x => x.GeographicalAreaHjid == "0001").Description, Is.EqualTo("South Africa"));
			Assert.That(grp.Countries.Single(x => x.GeographicalAreaHjid == "0002").Description, Is.EqualTo("Australia"));
			Assert.That(grp.Countries.Single(x => x.GeographicalAreaHjid == "0004").Description, Is.EqualTo(string.Empty));
		}

		[Test]
		public void IsChapterSpecific()
		{
			Assert.That(sharedProcessor.IsChapterSpecific, Is.EqualTo(false));
		}

		[Test]
		public void PartialUpdates()
		{
			var filePath = Path.Combine(ContentFolder, "UT_GeographicalArea_002.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_GeographicalArea_002.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var processor = new GeographicalAreaProcessorTester();
			processor.SimulateProcessing("", files, errorCollector, "");

			Assert.That(errorCollector.ToString(), Is.Empty);
			var models = processor.Models.Cast<GeographicalArea>().ToList();
			Assert.That(models.Count, Is.EqualTo(1));
			Assert.That(models[0].Description, Is.EqualTo("Updated description"));
			Assert.That(models[0].Countries.Select(x => x.GeographicalAreaHjid), Is.EquivalentTo(new[] { "23571", "23746", "23866" }));
		}

		#region Setup
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
			sharedProcessor = new GeographicalAreaProcessorTester();
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

		internal class GeographicalAreaProcessorTester : GeographicalAreaProcessor
		{
			public GeographicalAreaProcessorTester() : this(new Common.Tests.CommonHelpers.DateTimeProvider(), new IRefXmlBuilder[] { new VirtualBuilder() }) { }
			public GeographicalAreaProcessorTester(IDateTimeProvider dateTimeProvider, IRefXmlBuilder[] builders) : base(dateTimeProvider, builders)
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
		GeographicalAreaProcessorTester sharedProcessor;
		#endregion
	}
}
