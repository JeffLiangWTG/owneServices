using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests.TestHelperClasses;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors.Tests
{
	[TestFixture]
	class GoodsNomenclatureProcessorTests
	{
		[Test]
		public void ProcessChapters()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Goodsnomenclature_001.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_001.xml");

			var file = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath };
			var files = new List<FileDetails>() { file };

			var errorCollector = new StringBuilder();
			var processor = new GoodsNomenclatureProcessorTester();
			processor.SimulateProcessing("", files, errorCollector, "");

			Assert.That(processor.TestBuilders.First(), Is.Not.Null);
			Assert.That(processor.TestBuilders.First().PublicationDate, Is.EqualTo(file.Content.ExecutionDate));
			Assert.That(processor.TestBuilders.First().BuildCount, Is.EqualTo(26));

			errorCollector.Clear();
			processor = new GoodsNomenclatureProcessorTester();
			processor.SimulateProcessing("37", files, errorCollector, "");

			Assert.That(processor.TestBuilders.First().BuildCount, Is.EqualTo(2));
			Assert.That(errorCollector.ToString(), Is.Empty);

			errorCollector.Clear();
			processor = new GoodsNomenclatureProcessorTester();
			processor.SimulateProcessing("47", files, errorCollector, "");
			Assert.That(processor.TestBuilders.First().BuildCount, Is.EqualTo(2));
		}

		[Test]
		public void GoodsNomenclatureSuccessor()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Goodsnomenclature_002.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_Goodsnomenclature_002.xml");

			var file = new FileDetails { Content = new ContentDetails(), Filename = filePath };
			var files = new List<FileDetails>() { file };

			var refData = new List<ITariffModel>()
			{
				new Measure { ItemId = "2710122100", StartDate = new DateTime(2021, 01, 01) },
			};

			var errorCollector = new StringBuilder();
			var datetimeHelper = new Common.Tests.CommonHelpers.DateTimeProvider { TestDateTime = new DateTime(2028, 1, 1), TestHistoricalDateTime = new DateTime(2027, 1, 1) };
			var processor = new GoodsNomenclatureProcessorTester(datetimeHelper, new IRefXmlBuilder[] { new VirtualBuilder() });
			processor.LoadData("27", files, errorCollector);
			processor.UpdateModels("27", refData, errorCollector);

			var models = processor.Models.Cast<GoodsNomenclature>();

			var expectedEndDate = new DateTime(2024, 3, 31, 23, 59, 59);
			Assert.That(models.Single(x => x.ItemId == "2710122190").EndDate, Is.EqualTo(expectedEndDate), "2710122190 EndDate");
			Assert.That(models.Single(x => x.ItemId == "2710122119").EndDate, Is.EqualTo(expectedEndDate), "2710122119 EndDate");
			Assert.That(models.Single(x => x.ItemId == "2710122111" && x.ProductLineSuffix == "80").EndDate, Is.EqualTo(expectedEndDate), "2710122111/80 EndDate");
			Assert.That(models.Single(x => x.ItemId == "2710122111" && x.ProductLineSuffix == "10").EndDate, Is.EqualTo(expectedEndDate), "2710122111/10 EndDate");
			Assert.That(models.SingleOrDefault(x => x.ItemId == "2710122100" && x.EndDate == expectedEndDate), Is.Not.Null, "'old' 2710122100");

			var item2710122100 = models.FirstOrDefault(x => x.ItemId == "2710122100");
			Assert.That(item2710122100, Is.Not.Null, "2710122100");
			Assert.That(item2710122100.StartDate, Is.EqualTo(new DateTime(2024, 4, 1, 0, 0, 0)), "2710122100 StartDate");
			Assert.That(item2710122100.EndDate, Is.Null, "2710122100 EndDate");
			Assert.That(item2710122100.ProductLineSuffix, Is.EqualTo("80"), "2710122100 ProductLineSuffix");
			Assert.That(item2710122100.Description, Is.EqualTo("White spirit"), "2710122100 Description");
			Assert.That(item2710122100.Indent, Is.EqualTo(5), "2710122100 Indent");
			Assert.That(item2710122100.IsForMeasure, Is.True, "2710122100 IsForMeasure");
		}

		[Test]
		public void EmptyDescriptionUpdatedToNotProvided()
		{
			var dateTimeProvider = new Common.Tests.CommonHelpers.DateTimeProvider();
			var errorCollector = new StringBuilder();
			var builder = new VirtualBuilder();
			var processor = new GoodsNomenclatureProcessorTester(dateTimeProvider, new IRefXmlBuilder[] { builder });

			var dataModels = new List<ITariffModel>()
			{
				new GoodsNomenclature { ItemId = "0100000000", ProductLineSuffix = "10", Description = "No Update Expected", Indent = 0 },
				new GoodsNomenclature { ItemId = "0101000000", ProductLineSuffix = "80", Description = "", Indent = 1, HJID = "ShouldChange" },
			};

			processor.Models = dataModels;

			processor.UpdateModels("XX", null, errorCollector);

			var models = processor.Models.Cast<GoodsNomenclature>().ToList();

			Assert.That(models.Count, Is.EqualTo(2));
			Assert.That(models.First(x => x.ItemId == "0100000000").Description, Is.EqualTo("No Update Expected"));
			Assert.That(models.First(x => x.ItemId == "0101000000").Description, Is.EqualTo("(Commodity description was not provided)"));
		}

		[Test]
		public void SectionModelMapping()
		{
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			var errorCollector = new StringBuilder();

			var builder = new VirtualBuilder();
			var processor = new GoodsNomenclatureProcessorTester(dateTimeProvider.Object, new IRefXmlBuilder[] { builder });

			processor.Models = new List<ITariffModel>() { new GoodsNomenclature() { ItemId = "010101", ProductLineSuffix = "80", Indent = 02, SectionNumber = 1, Description = "Item", StartDate = new DateTime(1996, 01, 01, 00, 00, 00), EndDate = new DateTime(2006, 12, 31, 23, 59, 59) } };
			processor.UpdateModels("01", null, errorCollector);

			Assert.That(processor.Models, Is.Not.Null);
			Assert.That(processor.Models.Count, Is.EqualTo(2));

			var goodsNomenclature = (GoodsNomenclature)processor.Models[0];
			Assert.That(goodsNomenclature.ItemId, Is.EqualTo("01"));
			Assert.That(goodsNomenclature.Indent, Is.EqualTo(0));
			Assert.That(goodsNomenclature.Description, Is.EqualTo("Live animals; animal products"));
			Assert.That(goodsNomenclature.SectionNumber, Is.EqualTo(1));
			Assert.That(goodsNomenclature.IsSection, Is.EqualTo(true));
		}

		[Test]
		public void CompositeKeyGenerationV2()
		{
			var testData = new Dictionary<string, GoodsNomenclature>()
			{
				// heading
				{ "06.28", new GoodsNomenclature { ItemId = "2800000000", ProductLineSuffix = "80", Indent = 0, Description = "Chapter 28" } },
				{ "06.28.01", new GoodsNomenclature { ItemId = "2801000000", ProductLineSuffix = "10", Indent = 0, Description = "Heading 28.01" } },
				{ "06.28.02", new GoodsNomenclature { ItemId = "2826000000", ProductLineSuffix = "10", Indent = 0, Description = "Heading 28.26" } },
				{ "06.28.01.01", new GoodsNomenclature { ItemId = "2801000000", ProductLineSuffix = "80", Indent = 0, Description = "Sub-Heading 28.01" } },
				{ "06.28.02.26", new GoodsNomenclature { ItemId = "2826000000", ProductLineSuffix = "80", Indent = 0, Description = "Sub-Heading 28.26" } },
				{ "06.28.01.04", new GoodsNomenclature { ItemId = "2804000000", ProductLineSuffix = "80", Indent = 0, Description = "Heading 28.04" } },
				// No heading
				{ "04.16", new GoodsNomenclature { ItemId = "1600000000", ProductLineSuffix = "80", Indent = 0, Description = "Chapter 16" } },
				{ "04.16..01", new GoodsNomenclature { ItemId = "1601000000", ProductLineSuffix = "80", Indent = 0, Description = "Heading 16.01" } },
				{ "04.16..02", new GoodsNomenclature { ItemId = "1602000000", ProductLineSuffix = "80", Indent = 0, Description = "Heading 16.02" } },
				// First Child
				{ "06.28.01.01.1", new GoodsNomenclature { ItemId = "2801100000", ProductLineSuffix = "80", Indent = 1, Description = "Commodity 1 28.01.10" } },
				{ "04.16..01.10", new GoodsNomenclature { ItemId = "1601001000", ProductLineSuffix = "80", Indent = 1, Description = "Commodity 2 16.01.00.10" } },
				{ "04.16..01.2.9", new GoodsNomenclature { ItemId = "1601290000", ProductLineSuffix = "80", Indent = 1, Description = "Commodity 5 16.01.29" } },
				{ "04.16..02.3.9", new GoodsNomenclature { ItemId = "1602390000", ProductLineSuffix = "80", Indent = 1, Description = "Commodity 7 16.02.39" } },
				{ "06.28.01.01.5.5", new GoodsNomenclature { ItemId = "2801550000", ProductLineSuffix = "80", Indent = 1, Description = "Commodity 11 28.55" } },
				// Second Child
				{ "06.28.01.01.1.10", new GoodsNomenclature { ItemId = "2801101000", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 3 28.01.10.10" } },
				{ "04.16..01.10.10", new GoodsNomenclature { ItemId = "1601001010", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 4 16.01.00.10.10" } },
				{ "04.16..01.2.9.10", new GoodsNomenclature { ItemId = "1601291000", ProductLineSuffix = "80", Indent = 3, Description = "Commodity 6 16.01.29.10" } },
				{ "04.16..02.3.9.40", new GoodsNomenclature { ItemId = "1602670000", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 24 16.02.67" } },
				{ "04.16..02.3.9.40.10", new GoodsNomenclature { ItemId = "1602673000", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 25 16.02.67.30" } },
				// Muliple (less than 10)
				{ "04.16..02.3.9.10", new GoodsNomenclature { ItemId = "1602391100", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 8 16.02.39.11" } },
				{ "04.16..02.3.9.20", new GoodsNomenclature { ItemId = "1602392200", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 9 16.02.39.22" } },
				{ "04.16..02.3.9.30", new GoodsNomenclature { ItemId = "1602395500", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 10 16.02.39.55" } },
				// Multiple (more than 10)
				{ "06.28.01.01.5.5.010", new GoodsNomenclature { ItemId = "2801550100", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 12 28.01.55.01" } },
				{ "06.28.01.01.5.5.020", new GoodsNomenclature { ItemId = "2801550200", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 13 28.01.55.02" } },
				{ "06.28.01.01.5.5.030", new GoodsNomenclature { ItemId = "2801550300", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 14 28.01.55.03" } },
				{ "06.28.01.01.5.5.040", new GoodsNomenclature { ItemId = "2801550400", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 15 28.01.55.04" } },
				{ "06.28.01.01.5.5.050", new GoodsNomenclature { ItemId = "2801550500", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 16 28.01.55.05" } },
				{ "06.28.01.01.5.5.060", new GoodsNomenclature { ItemId = "2801552600", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 17 28.01.55.26" } },
				{ "06.28.01.01.5.5.070", new GoodsNomenclature { ItemId = "2801552700", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 18 28.01.55.27" } },
				{ "06.28.01.01.5.5.080", new GoodsNomenclature { ItemId = "2801552800", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 19 28.01.55.28" } },
				{ "06.28.01.01.5.5.090", new GoodsNomenclature { ItemId = "2801553000", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 20 28.01.55.30" } },
				{ "06.28.01.01.5.5.100", new GoodsNomenclature { ItemId = "2801553100", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 21 28.01.55.31" } },
				{ "06.28.01.01.5.5.110", new GoodsNomenclature { ItemId = "2801553200", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 22 28.01.55.32" } },
				{ "06.28.01.01.5.5.120", new GoodsNomenclature { ItemId = "2801553300", ProductLineSuffix = "80", Indent = 2, Description = "Commodity 23 28.01.55.33" } },
				// Conflicting scenario
				{ "02.10", new GoodsNomenclature { ItemId = "1000000000", ProductLineSuffix = "80", Indent = 0, Description = "CEREALS" } },
				{ "02.10..01", new GoodsNomenclature { ItemId = "1001000000", ProductLineSuffix = "80", Indent = 0, Description = "Wheat and meslin" } },
				{ "02.10..01.1.1", new GoodsNomenclature { ItemId = "1001110000", ProductLineSuffix = "10", Indent = 1, Description = "Durum wheat" } },
				{ "02.10..01.1.1.10", new GoodsNomenclature { ItemId = "1001110000", ProductLineSuffix = "80", Indent = 2, Description = "Seed" } },
				{ "02.10..01.1.1.20", new GoodsNomenclature { ItemId = "1001190000", ProductLineSuffix = "80", Indent = 2, Description = "Other - 1" } },
				{ "02.10..01.1.1.20.10", new GoodsNomenclature { ItemId = "1001190012", ProductLineSuffix = "10", Indent = 3, Description = "High quality durum wheat" } },
				{ "02.10..01.1.1.20.10.10", new GoodsNomenclature { ItemId = "1001190012", ProductLineSuffix = "80", Indent = 4, Description = "Durum wheat with..." } },
				{ "02.10..01.1.1.20.10.20", new GoodsNomenclature { ItemId = "1001190018", ProductLineSuffix = "80", Indent = 4, Description = "Other - 2" } },
				{ "02.10..01.1.1.20.20", new GoodsNomenclature { ItemId = "1001190020", ProductLineSuffix = "80", Indent = 3, Description = "Medium quality durum wheat" } },
				{ "02.10..01.1.1.20.30", new GoodsNomenclature { ItemId = "1001190030", ProductLineSuffix = "80", Indent = 3, Description = "Low quality durum wheat" } },
			};

			var expectedSections = new Dictionary<string, GoodsNomenclature>()
			{
				{ "02", new GoodsNomenclature { ItemId = "02", Description = "Section 2" } },
				{ "04", new GoodsNomenclature { ItemId = "04", Description = "Section 4" } },
				{ "05", new GoodsNomenclature { ItemId = "05", Description = "Section 5" } },
				{ "06", new GoodsNomenclature { ItemId = "06", Description = "Section 6" } },
			};

			var dataModels = testData.Values.Cast<ITariffModel>().ToList();

			var dateTimeProvider = new Common.Tests.CommonHelpers.DateTimeProvider();
			var errorCollector = new StringBuilder();
			var builder = new VirtualBuilder();
			var processor = new GoodsNomenclatureProcessorTester(dateTimeProvider, new IRefXmlBuilder[] { builder });

			processor.Models = dataModels;

			processor.UpdateModels("2", null, errorCollector);

			var models = processor.Models.Cast<GoodsNomenclature>().ToList();

			foreach (var model in models)
			{
				if (model.IsSection)
				{
					Assert.That(expectedSections.ContainsKey(model.Key), $"Checking section Key: '{model.Key}' Description: '{model.Description}'");
					var testItem = expectedSections[model.Key];

					Assert.That(model.ItemId, Is.EqualTo(testItem.ItemId));
				}
				else
				{
					Assert.That(testData.ContainsKey(model.Key), $"Checking item Key: '{model.Key}' Description: '{model.Description}'");
					var testItem = testData[model.Key];

					Assert.That(model.Description, Is.EqualTo(testItem.Description), $"Checking item Key: {model.Key} Description: {model.Description}");
				}
			}
		}

		[Test]
		public void IncompleteDataLoggedAndAborted()
		{
			var dataModels = new List<ITariffModel>()
			{
				{ new GoodsNomenclature { ItemId = "2901001000", ProductLineSuffix = "80", Indent = 0, Description = "Commodity 2 21.01.Crash" } },
				{ new GoodsNomenclature { ItemId = "2902001000", ProductLineSuffix = "80", Indent = 1, Description = "Commodity 2 21.02" } },
			};

			var dateTimeProvider = new Common.Tests.CommonHelpers.DateTimeProvider();
			var errorCollector = new StringBuilder();
			var builder = new VirtualBuilder();
			var processor = new GoodsNomenclatureProcessorTester(dateTimeProvider, new IRefXmlBuilder[] { builder });

			processor.Models = dataModels;

			var ex = Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.StartsWith($"Incomplete GoodsNomenclature data:"), () => processor.UpdateModels("2", null, errorCollector));
			Assert.That(ex.GetBaseException().Message,
				Does.Contain("1 item(s) are missing parents")
				.And.Contains("Item: 2901001000 ProductLineSuffix: 80 Indent: 0"));
		}

		[Test]
		public void UpdateModels()
		{
			var dateTimeProvider = new Common.Tests.CommonHelpers.DateTimeProvider();
			var errorCollector = new StringBuilder();
			var builder = new VirtualBuilder();
			var processor = new GoodsNomenclatureProcessorTester(dateTimeProvider, new IRefXmlBuilder[] { builder });

			var dataModels = new List<ITariffModel>()
			{
				new GoodsNomenclature { ItemId = "0100000000", ProductLineSuffix = "10", Description = "Level 1 - Chapter", Indent = 0, IsSection = true },
				new GoodsNomenclature { ItemId = "0101000000", ProductLineSuffix = "80", Description = "Level 2 - Heading", Indent = 1 },
				new GoodsNomenclature { ItemId = "0101010000", ProductLineSuffix = "80", Description = "Level 3 - Commodity 1", Indent = 2 },
				new GoodsNomenclature { ItemId = "0200000000", ProductLineSuffix = "10", Description = "Level 1 - Chapter", Indent = 0, IsSection = true },
				new GoodsNomenclature { ItemId = "0202000000", ProductLineSuffix = "80", Description = "Level 2 - Heading", Indent = 1 },
				new GoodsNomenclature { ItemId = "0202020000", ProductLineSuffix = "80", Description = "Level 3 - Commodity 2", Indent = 2 },
				new GoodsNomenclature { ItemId = "0202020200", ProductLineSuffix = "80", Description = "Level 4 - Commodity 2 Expired", Indent = 3, EndDate = dateTimeProvider.UTCHistoricalDate.AddDays(-1) },
				new GoodsNomenclature { ItemId = "0300000000", ProductLineSuffix = "10", Description = "Level 1 - Chapter", Indent = 0, IsSection = true },
				new GoodsNomenclature { ItemId = "0303000000", ProductLineSuffix = "80", Description = "Level 2 - Heading", Indent = 1 },
				new GoodsNomenclature { ItemId = "0303030000", ProductLineSuffix = "80", Description = "Level 3 - Commodity 3", Indent = 2 },
				new GoodsNomenclature { ItemId = "0303030300", ProductLineSuffix = "80", Description = "Level 4 - Commodity 3 partial expiry", Indent = 3, EndDate = dateTimeProvider.UTCDateTime.AddDays(-1) }
			};

			var refData = new List<ITariffModel>()
			{
				new Measure { ItemId = "0101010000", StartDate = DateTime.Today.AddDays(-1) },
				new Measure { ItemId = "0202020000", StartDate = DateTime.Today.AddDays(-1) },
				new Measure { ItemId = "0303030000", StartDate = DateTime.Today.AddDays(-1) }
			};

			processor.Models = dataModels;

			processor.UpdateModels("XX", refData, errorCollector);

			var models = processor.Models.Cast<GoodsNomenclature>().ToList();

			Assert.That(models.Count, Is.EqualTo(11));
			var forMeasure = models.Where(x => x.IsForMeasure ?? false);
			Assert.That(forMeasure.Count, Is.EqualTo(4));
			Assert.That(forMeasure.Any(x => x.ItemId == "0101010000"));
			Assert.That(forMeasure.Any(x => x.ItemId == "0202020000"));
			Assert.That(forMeasure.Any(x => x.ItemId == "0303030000"));
			Assert.That(forMeasure.Any(x => x.ItemId == "0303030300"));

			Assert.That(models[0].SectionNumber, Is.EqualTo(1));
		}

		[Test]
		public void IsForMeasureAndIsForNomenclature()
		{
			var dateTimeProvider = new Common.Tests.CommonHelpers.DateTimeProvider();
			var errorCollector = new StringBuilder();
			var builder = new VirtualBuilder();
			var processor = new GoodsNomenclatureProcessorTester(dateTimeProvider, new IRefXmlBuilder[] { builder });

			dateTimeProvider.TestDateTime = new DateTime(2020, 11, 20);

			var dataModels = new List<ITariffModel>()
			{
				new GoodsNomenclature { ItemId = "0100000000", ProductLineSuffix = "10", Description = "01 Level 1 - Chapter", Indent = 0, IsSection = true },
				new GoodsNomenclature { ItemId = "0101000000", ProductLineSuffix = "80", Description = "02 Level 2 - Heading", Indent = 1 },
				new GoodsNomenclature { ItemId = "0101010000", ProductLineSuffix = "80", Description = "03 Level 3 - Commodity 1", Indent = 2 },												//IsForMeasure
				new GoodsNomenclature { ItemId = "0101010100", ProductLineSuffix = "10", Description = "04 Level 3 - Commodity 1 - NonActive GN", Indent = 3 },
				new GoodsNomenclature { ItemId = "0101010101", ProductLineSuffix = "80", Description = "05 Level 3 - Commodity 1 - NonActive Leaf", Indent = 4, EndDate = dateTimeProvider.UTCHistoricalDate.AddDays(-1) },
				//Actual data
				new GoodsNomenclature { ItemId = "2300000000", ProductLineSuffix = "10", Description = "06 Chapter", Indent = 0, IsSection = true },
				new GoodsNomenclature { ItemId = "2300000000", ProductLineSuffix = "80", Description = "07 RESIDUES AND WASTE FROM THE FOOD...", Indent = 0 },
				new GoodsNomenclature { ItemId = "2308000000", ProductLineSuffix = "80", Description = "08 Vegetable materials and v...", Indent = 0, StartDate = new DateTime(1972, 01, 01) },
				new GoodsNomenclature { ItemId = "2308001100", ProductLineSuffix = "10", Description = "09 Grape marc", Indent = 1, StartDate = new DateTime(2002, 01, 01) },
				new GoodsNomenclature { ItemId = "2308001100", ProductLineSuffix = "80", Description = "10 Having a total alcoholic...", Indent = 2, StartDate = new DateTime(2002, 01, 01) },	//IsForMeasure
				new GoodsNomenclature { ItemId = "2308001900", ProductLineSuffix = "80", Description = "11 Other", Indent = 2, StartDate = new DateTime(2002, 01, 01) },						//IsForMeasure
				new GoodsNomenclature { ItemId = "2308004000", ProductLineSuffix = "80", Description = "12 Acorns and horse-chestnut...", Indent = 1, StartDate = new DateTime(2002, 01, 01) },
				new GoodsNomenclature { ItemId = "2308004010", ProductLineSuffix = "80", Description = "13 Citrus pulp residues", Indent = 2, StartDate = new DateTime(2020, 03, 01) },			//IsForMeasure
				new GoodsNomenclature { ItemId = "2308004010", ProductLineSuffix = "80", Description = "14 Citrus pulp residues", Indent = 2, StartDate = new DateTime(2008, 07, 01), EndDate = new DateTime(2018, 12, 31, 23, 59, 59) },
				new GoodsNomenclature { ItemId = "2308004090", ProductLineSuffix = "80", Description = "15 Other", Indent = 2, StartDate = new DateTime(2020, 03, 01) },						//IsForMeasure
				new GoodsNomenclature { ItemId = "2308004090", ProductLineSuffix = "80", Description = "16 Other", Indent = 2, StartDate = new DateTime(2008, 07, 01), EndDate = new DateTime(2018, 12, 31, 23, 59, 59) },
				new GoodsNomenclature { ItemId = "2308009000", ProductLineSuffix = "80", Description = "17 Other", Indent = 1, StartDate = new DateTime(2002, 01, 01) },						//IsForMeasure
				// IsForNomenclature
				new GoodsNomenclature { ItemId = "0300000000", ProductLineSuffix = "10", Description = "18 Level 1 - Chapter", Indent = 0, IsSection = true },
				new GoodsNomenclature { ItemId = "0301000000", ProductLineSuffix = "80", Description = "19 Level 2 - Heading", Indent = 1 },
				new GoodsNomenclature { ItemId = "0301010000", ProductLineSuffix = "80", Description = "20 Level 3 - Commodity 1", Indent = 2 },					//IsForMeasure & Nomenclature
				new GoodsNomenclature { ItemId = "0301010100", ProductLineSuffix = "10", Description = "21 Level 3 - Commodity 1 - NonActive GN", Indent = 3 },
				new GoodsNomenclature { ItemId = "0301010101", ProductLineSuffix = "80", Description = "22 Level 3 - Commodity 1 - Paertial Leaf", Indent = 4, EndDate = dateTimeProvider.UTCHistoricalDate.AddDays(1) }, //IsForMeasure
			};

			var refData = new List<ITariffModel>()
			{
				new Measure { ItemId = "0101010000", StartDate = dateTimeProvider.UTCDateTime.AddDays(-1) },
				new Measure { ItemId = "2308000000", StartDate = dateTimeProvider.UTCDateTime.AddDays(-1) },
				new Measure { ItemId = "0301000000", StartDate = dateTimeProvider.UTCDateTime.AddDays(-1) },
			};

			processor.Models = dataModels;

			processor.UpdateModels("XX", refData, errorCollector);

			var models = processor.Models.Cast<GoodsNomenclature>().ToList();
			Assert.That(models.Count, Is.EqualTo(22));

			var forMeasure = models.Where(x => x.IsForMeasure ?? false).ToList();
			Assert.That(forMeasure, Is.Not.Null);

			var items = string.Join(", ", forMeasure.Select(x => x.Description.Substring(0, 2)));

			Assert.That(forMeasure.Count, Is.EqualTo(8), $"Actual: {items}");

			Assert.That(forMeasure.Any(x => x.Description.StartsWith("03")));
			Assert.That(forMeasure.Any(x => x.Description.StartsWith("10")));
			Assert.That(forMeasure.Any(x => x.Description.StartsWith("11")));
			Assert.That(forMeasure.Any(x => x.Description.StartsWith("13")));
			Assert.That(forMeasure.Any(x => x.Description.StartsWith("15")));
			Assert.That(forMeasure.Any(x => x.Description.StartsWith("17")));
			Assert.That(forMeasure.Any(x => x.Description.StartsWith("20")));
			Assert.That(forMeasure.Any(x => x.Description.StartsWith("22")));

			var swinger = models.First(x => x.Description.StartsWith("20"));
			Assert.That(swinger.IsForMeasure.Value);
			Assert.That(swinger.IsForNomenclature.Value);
		}

		[Test]
		public void FutureChildrenInNomenclatureTree()
		{
			var dateTimeProvider = new Common.Tests.CommonHelpers.DateTimeProvider();
			var errorCollector = new StringBuilder();
			var builder = new VirtualBuilder();
			var processor = new GoodsNomenclatureProcessorTester(dateTimeProvider, new IRefXmlBuilder[] { builder });

			var dataModels = new List<ITariffModel>()
			{
				new GoodsNomenclature { ItemId = "0100000000", ProductLineSuffix = "10", Description = "Level 1 - Chapter", Indent = 0, IsSection = true },
				new GoodsNomenclature { ItemId = "0101000000", ProductLineSuffix = "80", Description = "Level 2 - Heading", Indent = 1 },
				new GoodsNomenclature { ItemId = "0101010000", ProductLineSuffix = "80", Description = "Level 3 - Commodity 1", Indent = 2 },
				new GoodsNomenclature { ItemId = "0101010100", ProductLineSuffix = "80", Description = "Level 4 - Commodity Future", Indent = 3, StartDate = dateTimeProvider.UTCDateTime.AddDays(1) },
			};

			var refData = new List<ITariffModel>()
			{
				new Measure { ItemId = "0101010000", StartDate = DateTime.Today.AddDays(-1) }
			};

			processor.Models = dataModels;

			processor.UpdateModels("XX", refData, errorCollector);

			var models = processor.Models.Cast<GoodsNomenclature>().ToList();

			Assert.That(models.Count, Is.EqualTo(4));
			var forMeasure = models.Where(x => x.IsForMeasure ?? false);
			Assert.That(forMeasure.Count, Is.EqualTo(2));
			Assert.That(forMeasure.Any(x => x.ItemId == "0101010000"));
			Assert.That(forMeasure.Any(x => x.ItemId == "0101010100"));
		}

		[Test]
		public void HtmlTagsInDescriptionConverted()
		{
			var dateTimeProvider = new Common.Tests.CommonHelpers.DateTimeProvider();
			var errorCollector = new StringBuilder();
			var builder = new VirtualBuilder();
			var processor = new GoodsNomenclatureProcessorTester(dateTimeProvider, new IRefXmlBuilder[] { builder });

			var dataModels = new List<ITariffModel>()
			{
				new GoodsNomenclature { ItemId = "0100000000", ProductLineSuffix = "10", Description = "Subscript X<sub>0</sub><sub>1</sub><sub>2</sub><sub>3</sub><sub>4</sub><sub>5</sub><sub>6</sub><sub>7</sub><sub>8</sub><sub>9</sub>", Indent = 0 },
				new GoodsNomenclature { ItemId = "0200000000", ProductLineSuffix = "10", Description = "Superscript X<sup>0</sup><sup>1</sup><sup>2</sup><sup>3</sup><sup>4</sup><sup>5</sup><sup>6</sup><sup>7</sup><sup>8</sup><sup>9</sup><sup>o</sup>", Indent = 0 },
				new GoodsNomenclature { ItemId = "0300000000", ProductLineSuffix = "10", Description = "BR-P X<br>Y<p/>Z", Indent = 0 },
			};

			processor.Models = dataModels;

			processor.UpdateModels("XX", null, errorCollector);

			var models = processor.Models.Cast<GoodsNomenclature>().ToList();
			Assert.That(models.Count, Is.EqualTo(3));

			var sub = models.FirstOrDefault(x => x.ItemId == "0100000000");
			Assert.That(sub, Is.Not.Null);
			Assert.That(sub.Description, Is.EqualTo("Subscript X\u2080\u2081\u2082\u2083\u2084\u2085\u2086\u2087\u2088\u2089"));

			var sup = models.FirstOrDefault(x => x.ItemId == "0200000000");
			Assert.That(sup, Is.Not.Null);
			Assert.That(sup.Description, Is.EqualTo("Superscript X\u2070\u00B9\u00B2\u00B3\u2074\u2075\u2076\u2077\u2078\u2079\u00B0"));

			var brp = models.FirstOrDefault(x => x.ItemId == "0300000000");
			Assert.That(brp, Is.Not.Null);
			Assert.That(brp.Description, Is.EqualTo("BR-P X\r\nY\r\nZ"));
		}

		[Test]
		public void IsChapterSpecific()
		{
			Assert.That(sharedProcessor.IsChapterSpecific, Is.EqualTo(true));
		}

		#region Setup
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
			sharedProcessor = new GoodsNomenclatureProcessorTester();
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
		GoodsNomenclatureProcessorTester sharedProcessor;
		#endregion
	}
}
