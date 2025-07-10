using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests.TestHelperClasses;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Tests
{
	[TestFixture]
	class GoodsNomenclatureBuilderBaseTests
	{
		[Test]
		public void ConvertModelToRefModel()
		{
			var model1 = new GoodsNomenclature
			{
				ItemId = "1234560000",
				ProductLineSuffix = "80",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Test Model Convertion",
				Indent = 2,
				SectionNumber = 3,
				Key = "UNIT-TEST1",
				CleanId = "123456"
			};

			var model2 = new GoodsNomenclature { IsForMeasure = true };

			var model3 = new GoodsNomenclature
			{
				ItemId = "2020202000",
				ProductLineSuffix = "80",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Test IsForNomenclature",
				Indent = 2,
				SectionNumber = 3,
				Key = "UNIT-TEST3",
				CleanId = "20202020",
				IsForMeasure = true,
				IsForNomenclature = true
			};

			var model4 = new GoodsNomenclature
			{
				ItemId = "3030300000",
				ProductLineSuffix = "10",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "ZZ5 Value filled",
				Indent = 2,
				SectionNumber = 3,
				Key = "UNIT-TEST4",
				CleanId = "303030",
				IsForMeasure = false,
				IsForNomenclature = true
			};

			var tariffModels = new List<ITariffModel>() { model1, model2, model3, model4 };

			var refModels = builder.ConvertToRefModels(tariffModels);
			Assert.That(refModels.Count, Is.EqualTo(3));

			var refModel = refModels.First(x => x.ZZ5_CompositeKey == "UNIT-TEST1");

			Assert.That(refModel, Is.Not.Null);
			Assert.That(refModel.ZZ5_Value, Is.EqualTo("123456"));
			Assert.That(refModel.ZZ5_Description, Is.EqualTo(model1.Description));
			Assert.That(refModel.ZZ5_CompositeKey, Is.EqualTo("UNIT-TEST1"));
			Assert.That(refModel.ZZ5_ZZ9_NKNomenclatureGroupType, Is.EqualTo("ABC"));
			Assert.That(refModel.ZZ5_ZZZ_NKDataGrouping, Is.EqualTo("ABC"));
			Assert.That(refModel.ZZ5_StartDate, Is.EqualTo(new DateTime(2019, 10, 1, 15, 14, 0)));
			Assert.That(refModel.ZZ5_EndDate, Is.EqualTo(new DateTime(2019, 12, 31, 23, 59, 0)));

			model1.StartDate = null;
			model1.EndDate = null;

			refModel = builder.ConvertToRefModels(tariffModels).First(x => x.ZZ5_CompositeKey == "UNIT-TEST1");

			Assert.That(refModel.ZZ5_StartDate, Is.EqualTo(DefaultValues.MinimumDateTime));
			Assert.That(refModel.ZZ5_EndDate, Is.EqualTo(DefaultValues.MaximumDateTime));

			refModel = builder.ConvertToRefModels(tariffModels).First(x => x.ZZ5_CompositeKey == "UNIT-TEST4");
			Assert.That(refModel.ZZ5_Value, Is.EqualTo("303030"));
		}

		[Test]
		public void SectionHeadersShouldHaveZZ5_Value()
		{
			var errorCollector = new StringBuilder();
			var processor = new GoodsNomenclatureProcessorTester();

			processor.Models = new List<ITariffModel> { new GoodsNomenclature { ItemId = "0101010000", Description = "Unit Test", ProductLineSuffix = "80", Indent = 1 } };
			var refData = new List<ITariffModel> { new Measure { ItemId = "0101010000" } };

			processor.UpdateModels("01", refData, errorCollector);

			var models = processor.Models;
			Assert.That(models.Count, Is.EqualTo(2));
			var refModels = builder.ConvertToRefModels(models).ToList();
			Assert.That(refModels.Count, Is.EqualTo(1));

			var refModel = refModels.First();

			Assert.That(refModel, Is.Not.Null);
			Assert.That(refModel.ZZ5_Value, Is.EqualTo("01"));
		}

		[Test]
		public void XmlWriterConfig()
		{
			var config = builder.XmlWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCusNomenclatureGroup);
			var entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig, Is.Not.Null);
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusNomenclatureGroup.ZZ5_Value))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusNomenclatureGroup.ZZ5_CompositeKey))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusNomenclatureGroup.ZZ5_Description))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusNomenclatureGroup.ZZ5_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusNomenclatureGroup.ZZ5_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusNomenclatureGroup.ZZ5_ZZ9_NKNomenclatureGroupType))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusNomenclatureGroup.ZZ5_ZZZ_NKDataGrouping))));
		}

		[Test]
		public void UniqueId()
		{
			var model = new RefCusNomenclatureGroup { ZZ5_CompositeKey = "12.3456.78.90", ZZ5_StartDate = new DateTime(2019, 01, 26, 12, 13, 14), ZZ5_EndDate = new DateTime(2019, 02, 17, 09, 33, 04) };

			Assert.That(builder.UniqueId(model), Is.EqualTo("12.3456.78.90_20190126121314_20190217093304"));
		}

		[Test]
		public void DuplicateErrors()
		{
			var model = new RefCusNomenclatureGroup { ZZ5_Value = "1234567890", ZZ5_Description = "Unit Testing" };

			builder.DuplicateError(model, "0987654321", "UnitTest");
			Assert.That(errorCollector.ToString(), Contains.Substring("GoodsNomenclature duplicate exists. Key: '0987654321' Filter: 'UnitTest'"));
		}

		[Test]
		public void IsValid()
		{
			var model = new RefCusNomenclatureGroup();

			Assert.That(builder.IsValid(model, "UnitTest"), Is.False);
			Assert.That(errorCollector.ToString(), Contains.Substring("GoodsNomenclature validation error")
														.And.Contains("ZZ5_Value is required")
														.And.Contains("ZZ5_Description is required")
														.And.Contains("Filter: 'UnitTest'"));
			errorCollector.Clear();
			model.ZZ5_Value = "Suprise! Enjoy the new DB constraint";
			Assert.That(builder.IsValid(model, "UnitTest"), Is.False);
			Assert.That(errorCollector.ToString(), Does.Not.Contain("ZZ5_Value is required"));

			errorCollector.Clear();
			model.ZZ5_Description = "Unit Test";
			Assert.That(builder.IsValid(model, "UnitTest"), Is.True);
			Assert.That(errorCollector.ToString(), Does.Not.Contain("GoodsNomenclature validation error"));
		}

		[Test]
		public void IsExpired()
		{
			var model = new RefCusNomenclatureGroup();

			model.ZZ5_EndDate = DateTime.MinValue;
			Assert.That(builder.IsExpired(model), Is.True);

			model.ZZ5_EndDate = DateTime.Today.AddYears(-1);
			Assert.That(builder.IsExpired(model), Is.False);

			model.ZZ5_EndDate = DateTime.MaxValue.Date;
			Assert.That(builder.IsExpired(model), Is.False);
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
			var builder = new GoodsNomenclatureBuilderBaseTester(dateTimeProvider.Object, errorCollector);

			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, "UnitTest");

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Output.RefCusNomenclatureGroup_223645135.xml");
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
			builder = new GoodsNomenclatureBuilderBaseTester(dateTimeProvider.Object, errorCollector);
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

		GoodsNomenclatureBuilderBaseTester builder;
		StringBuilder errorCollector;
		string TempFolder;
	}

	internal class GoodsNomenclatureBuilderBaseTester : GoodsNomenclatureBuilderBase
	{
		public GoodsNomenclatureBuilderBaseTester(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		public new IEnumerable<RefCusNomenclatureGroup> ConvertToRefModels(List<ITariffModel> data) => base.ConvertToRefModels(data);
		public new XmlWriterConfiguration XmlWriterConfiguration() => base.XmlWriterConfiguration();
		public new bool IsValid(RefCusNomenclatureGroup refModel, string source) => base.IsValid(refModel, source);
		public new bool IsExpired(RefCusNomenclatureGroup refModel) => base.IsExpired(refModel);
		public new void DuplicateError(RefCusNomenclatureGroup refModel, string uniqueId, string source) => base.DuplicateError(refModel, uniqueId, source);
		public new string UniqueId(RefCusNomenclatureGroup refModel) => base.UniqueId(refModel);
		public new string OutputFileName => base.OutputFileName;

		protected override string DataGrouping => "ABC";
		protected override string FilePrefix => "UTGoodsNomenclature";
		protected override string XMLWriterDataSource => "ABC Goods Nomenclature";
	}
}
