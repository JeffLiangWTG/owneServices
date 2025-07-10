using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff
{
	[TestFixture]
	class GoodsNomenclatureBuilderTests
	{
		[Test]
		public void FilePrefix()
		{
			Assert.That(builder.FilePrefix, Is.EqualTo("GB_RefCusNomenclatureGroup"));
		}

		[Test]
		public void BuildXmlFile()
		{
			var models = new List<ITariffModel>()
			{
				new GoodsNomenclature { StartDate = new DateTime(2019,01,01), EndDate = new DateTime(2019,12,31, 23,59,59), Indent = 0, ItemId = "03", ProductLineSuffix = "10", SectionNumber = 3, IsSection = true, Description = "Section Test", Key = "03", CleanId = "03", Chapter = "03" },
				new GoodsNomenclature { StartDate = new DateTime(2019,01,01), EndDate = new DateTime(2019,12,31, 23,59,59), Indent = 1, ItemId = "1010000000", ProductLineSuffix = "80", SectionNumber = 3, IsSection = false, Description = "Item1", Key = "03.10", CleanId = "1010", Chapter = "10" },
				new GoodsNomenclature { StartDate = new DateTime(2019,01,01), EndDate = new DateTime(2019,12,31, 23,59,59), Indent = 2, ItemId = "2020000000", ProductLineSuffix = "80", SectionNumber = 3, IsSection = false, Description = "Item2", Key = "03.20", CleanId = "2020", Chapter = "20" },
				new GoodsNomenclature { StartDate = new DateTime(2019,01,01), EndDate = new DateTime(2019,12,31, 23,59,59), Indent = 2, ItemId = "2020000000", ProductLineSuffix = "80", SectionNumber = 3, IsSection = false, Description = "Duplicate", Key = "03.20", CleanId = "2020", Chapter = "20" },
				new GoodsNomenclature { StartDate = new DateTime(2019,01,01), Indent = 1, ItemId = "3456000000", ProductLineSuffix = "80", SectionNumber = 4, IsSection = false, Description = "NoEndDate", Key = "04.34", CleanId = "3456", Chapter = "34" },
				new GoodsNomenclature { EndDate = new DateTime(2019,12,31, 23,59,59), Indent = 1, ItemId = "4025000000", ProductLineSuffix = "80", SectionNumber = 4, IsSection = false, Description = "NoStartDate", Key = "04.40", CleanId = "4025", Chapter = "40" },
				new GoodsNomenclature { StartDate = new DateTime(2019,01,01), EndDate = new DateTime(2019,12,31, 23,59,59), Indent = 3, ItemId = "Invalid", ProductLineSuffix = "80", SectionNumber = 3, IsSection = false, Description = "", Key = "03.In", CleanId = "Invalid", Chapter = "In" },
			};

			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			var errorCollector = new StringBuilder();
			var builder = new GoodsNomenclatureBuilderTester(dateTimeProvider.Object, errorCollector);

			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, "UnitTest");

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Output.RefCusNomenclatureGroup_223645135.xml");
			Assert.That(xml, Is.EqualTo(expectedXml));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			dateTimeProvider.Setup(x => x.UTCHistoricalDate).Returns(new DateTime(2018, 11, 04, 0, 0, 0, 0));
			errorCollector = new StringBuilder();
			builder = new GoodsNomenclatureBuilderTester(dateTimeProvider.Object, errorCollector);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		[SetUp]
		public void Setup()
		{
			errorCollector.Clear();
		}

		GoodsNomenclatureBuilderTester builder;
		StringBuilder errorCollector;
		string TempFolder;
	}

	internal class GoodsNomenclatureBuilderTester : GoodsNomenclatureBuilder
	{
		public GoodsNomenclatureBuilderTester(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		public new string FilePrefix => base.FilePrefix;
		public new string OutputFileName => base.OutputFileName;
	}
}
