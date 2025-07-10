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

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Tests
{
	[TestFixture]
	sealed class AdditionalCodeBuilderBaseTests
	{
		[Test]
		public void ConvertModelToRefModel()
		{
			var ac1 = new AdditionalCode { CodeType = "A", Code = "111", Description = "Item 1" };
			var ac2 = new AdditionalCode { CodeType = "B", Code = "222", Description = "Item 2", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2020, 12, 31) };
			var ac3 = new AdditionalCode
			{
				CodeType = "C",
				Code = "333",
				Description = "Item 3",
			}
			.SetDescriptions(new[]
			{
				new DescriptionPeriods.DescriptionModel { HJID = "1", LanguageCode = "NL", Description = "Dutch" },
				new DescriptionPeriods.DescriptionModel { HJID = "2", LanguageCode = "EN", Description = "English" },
				new DescriptionPeriods.DescriptionModel { HJID = "3", LanguageCode = "FR", Description = "French" },
				new DescriptionPeriods.DescriptionModel { HJID = "4", LanguageCode = "DE", Description = "German" },
			});

			var models = builder.ConvertToRefModels(new List<ITariffModel> { ac1, ac2, ac3 }).ToList();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(3));

			var model = models[0];
			Assert.That(model.ZZD_Code, Is.EqualTo("A111"));
			Assert.That(model.ZZD_Description, Is.EqualTo("Item 1"));
			Assert.That(model.ZZD_StartDate, Is.EqualTo(CommonHelper.DefaultValues.MinimumDateTime));
			Assert.That(model.ZZD_EndDate, Is.EqualTo(CommonHelper.DefaultValues.MaximumDateTime));

			model = models[1];
			Assert.That(model.ZZD_StartDate, Is.EqualTo(ac2.StartDate));
			Assert.That(model.ZZD_EndDate, Is.EqualTo(new DateTime(2020, 12, 30, 23, 59, 0)));

			model = models[2];
			Assert.That(model, Is.Not.Null);
			Assert.That(model.RefCusCodeListLanguages, Is.Null);
		}

		[Test]
		public void ConvertModelToRefModel_SupportsMultipleLanguages()
		{
			builder.SupportsMultipleLanguagesForTesting = true;
			var ac = new AdditionalCode
			{
				CodeType = "A",
				Code = "111",
				Description = "Item",
			}
			.SetDescriptions(new[]
			{
				new DescriptionPeriods.DescriptionModel { HJID = "1", LanguageCode = "NL", Description = "Dutch" },
				new DescriptionPeriods.DescriptionModel { HJID = "2", LanguageCode = "EN", Description = "English" },
				new DescriptionPeriods.DescriptionModel { HJID = "3", LanguageCode = "FR", Description = "French" },
				new DescriptionPeriods.DescriptionModel { HJID = "4", LanguageCode = "DE", Description = "German" },
			});

			var models = builder.ConvertToRefModels(new List<ITariffModel> { ac }).ToList();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(1));

			var model = models[0];
			Assert.That(model, Is.Not.Null);
			Assert.That(model.RefCusCodeListLanguages, Is.Not.Null);
			Assert.That(model.RefCusCodeListLanguages.Length, Is.EqualTo(4));
			Assert.That(model.RefCusCodeListLanguages.Any(x => x.ZXA_ZX6_NKLanguage == "NL"));
			Assert.That(model.RefCusCodeListLanguages.Any(x => x.ZXA_ZX6_NKLanguage == "EN"));
			Assert.That(model.RefCusCodeListLanguages.Any(x => x.ZXA_ZX6_NKLanguage == "FR"));
			Assert.That(model.RefCusCodeListLanguages.Any(x => x.ZXA_ZX6_NKLanguage == "DE"));
			Assert.That(model.RefCusCodeListLanguages.First(x => x.ZXA_ZX6_NKLanguage == "NL").ZXA_Description, Is.EqualTo("Dutch"));
		}

		[Test]
		public void DescriptionMaxLength()
		{
			var xy2000 = "X".PadRight(2000, 'Y');
			var ac = new AdditionalCode { CodeType = "A", Code = "POC", Description = xy2000 + "Z" };

			Assert.That(ac.Description.Length, Is.EqualTo(2001));
			var models = builder.ConvertToRefModels(new List<ITariffModel> { ac }).ToList();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(1));
			Assert.That(models[0].ZZD_Description.Length, Is.EqualTo(2000));
			Assert.That(models[0].ZZD_Description, Is.EqualTo(xy2000));
		}

		[Test]
		public void XmlWriterConfig()
		{
			var config = builder.XmlWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCusCodeList);
			var entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig, Is.Not.Null);
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_Code))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_ZZK_NKCodeType))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_Description))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_EndDate))));
			Assert.That(!entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.RefCusCodeListLanguages))));

			refType = typeof(RefCusCodeListLanguage);
			Assert.Null(config.GetConfiguration(refType));
		}

		[Test]
		public void XmlWriterConfig_SupportsMultipleLanguages()
		{
			builder.SupportsMultipleLanguagesForTesting = true;
			var config = builder.XmlWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCusCodeList);
			var entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig, Is.Not.Null);
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.RefCusCodeListLanguages))));

			refType = typeof(RefCusCodeListLanguage);
			entityConfig = config.GetConfiguration(refType);
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeListLanguage.ZXA_ZX6_NKLanguage))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeListLanguage.ZXA_Description))));
		}

		[Test]
		public void UniqueId()
		{
			var model = new RefCusCodeList { ZZD_Code = "1234", ZZD_StartDate = new DateTime(2019, 01, 26, 12, 13, 14), ZZD_EndDate = new DateTime(2019, 02, 17, 09, 33, 04) };

			Assert.That(builder.UniqueId(model), Is.EqualTo("1234_20190126121314_20190217093304"));
		}

		[Test]
		public void DuplicateErrors()
		{
			var model = new RefCusCodeList { ZZD_Code = "12345", ZZD_ZZK_NKCodeType = "ABCDE", ZZD_Description = "Something Wrong" };

			builder.DuplicateError(model, "1234", "");
			Assert.That(errorCollector.ToString(), Contains.Substring("RefCusCodeList duplicate exists. Key: '1234' Description: Something Wrong"));
		}

		[Test]
		public void IsValid()
		{
			var model = new RefCusCodeList();

			Assert.That(builder.IsValid(model, ""), Is.False);
			Assert.That(errorCollector.ToString(), Contains.Substring("RefCusCodeList validation error")
														.And.Contains("ZZD_Code is required")
														.And.Contains("ZZD_Description is required"));

			errorCollector.Clear();
			model.ZZD_Code = "1234";
			Assert.That(builder.IsValid(model, ""), Is.False);
			Assert.That(errorCollector.ToString(), Does.Not.Contain("ZZD_Code is required"));

			errorCollector.Clear();
			model.ZZD_Description = "Unit Test";
			Assert.That(builder.IsValid(model, ""), Is.True);
			Assert.That(errorCollector.ToString(), Does.Not.Contain("ZZD_Description is required"));
		}

		[Test]
		public void IsExpired()
		{
			var model = new RefCusCodeList();

			model.ZZD_EndDate = DateTime.MinValue;
			Assert.That(builder.IsExpired(model), Is.True);

			model.ZZD_EndDate = DateTime.Today.AddYears(-1);
			Assert.That(builder.IsExpired(model), Is.False);

			model.ZZD_EndDate = DateTime.MaxValue.Date;
			Assert.That(builder.IsExpired(model), Is.False);
		}

		[Test]
		public void BuildXmlFile()
		{
			var models = new List<ITariffModel>
			{
				new AdditionalCode { CodeType = "A", Code = "111", Description = "Item 1" },
				new AdditionalCode { CodeType = "B", Code = "222", Description = "Item 2", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2020, 12, 31, 23, 59, 59) },
				new AdditionalCode { CodeType = "A", Code = "111", Description = "Duplicate" },
				new AdditionalCode { CodeType = "B", Code = "111", Description = "Item 3" },
				new AdditionalCode { Code = "", Description = "No Code" },
				new AdditionalCode { Code = "NoDescrip", Description = "" },
				new AdditionalCode {
					CodeType = "D",
					Code = "444",
					Description = "Item 4",
				}
				.SetDescriptions(new[]
				{
					new DescriptionPeriods.DescriptionModel { HJID = "1", LanguageCode = "NL", Description = "Dutch" },
					new DescriptionPeriods.DescriptionModel { HJID = "2", LanguageCode = "EN", Description = "English" },
					new DescriptionPeriods.DescriptionModel { HJID = "3", LanguageCode = "FR", Description = "French" },
					new DescriptionPeriods.DescriptionModel { HJID = "4", LanguageCode = "DE", Description = "German" },
				})
			};

			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			var errorCollector = new StringBuilder();
			var builder = new AdditionalCodeBuilderTester(dateTimeProvider.Object, errorCollector);

			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, "");

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Output.RefCusCodeList_001.xml");
			Assert.That(xml, Is.EqualTo(expectedXml));
		}

		[Test]
		public void BuildXmlFile_SupportsMultipleLanguages()
		{
			var models = new List<ITariffModel>
			{
				new AdditionalCode {
					CodeType = "A",
					Code = "111",
					Description = "Item 1",
				}
				.SetDescriptions(new[]
				{
					new DescriptionPeriods.DescriptionModel { HJID = "1", LanguageCode = "NL", Description = "Dutch" },
					new DescriptionPeriods.DescriptionModel { HJID = "2", LanguageCode = "EN", Description = "English" },
					new DescriptionPeriods.DescriptionModel { HJID = "3", LanguageCode = "FR", Description = "French" },
					new DescriptionPeriods.DescriptionModel { HJID = "4", LanguageCode = "DE", Description = "German" },
				}),
				new AdditionalCode {
					CodeType = "B",
					Code = "222",
					Description = "Item 2",
				},
				new AdditionalCode {
					CodeType = "C",
					Code = "333",
					Description = "Item 3",
				},
			};

			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			var errorCollector = new StringBuilder();
			var builder = new AdditionalCodeBuilderTester(dateTimeProvider.Object, errorCollector);
			builder.SupportsMultipleLanguagesForTesting = true;
			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);
			builder.BuildXml(publicationDate, models, TempFolder, "");

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Output.RefCusCodeList_001_MultipleLanguages.xml");
			Assert.That(xml, Is.EqualTo(expectedXml));
		}

		[Test]
		public void TestSupportsMultipleLanguages()
		{
			Assert.That(builder.SupportsMultipleLanguagesExposed, Is.False);
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
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2020, 11, 01, 0, 0, 0, 0));
			dateTimeProvider.Setup(x => x.UTCHistoricalDate).Returns(new DateTime(2019, 11, 01, 0, 0, 0, 0));
			errorCollector = new StringBuilder();
			builder = new AdditionalCodeBuilderTester(dateTimeProvider.Object, errorCollector);
			errorCollector.Clear();
		}

		AdditionalCodeBuilderTester builder;
		StringBuilder errorCollector;
		string TempFolder;
	}

	internal class AdditionalCodeBuilderTester : AdditionalCodeBuilderBase
	{
		public AdditionalCodeBuilderTester(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		public new IEnumerable<RefCusCodeList> ConvertToRefModels(List<ITariffModel> data) => base.ConvertToRefModels(data);
		public new XmlWriterConfiguration XmlWriterConfiguration() => base.XmlWriterConfiguration();
		public new bool IsValid(RefCusCodeList refModel, string source) => base.IsValid(refModel, source);
		public new bool IsExpired(RefCusCodeList refModel) => base.IsExpired(refModel);
		public new void DuplicateError(RefCusCodeList refModel, string uniqueId, string source) => base.DuplicateError(refModel, uniqueId, source);
		public new string UniqueId(RefCusCodeList refModel) => base.UniqueId(refModel);
		public new string OutputFileName => base.OutputFileName;

		protected override string DataGrouping => "ABC";
		protected override string FilePrefix => "UTAdditionalCode";
		protected override string XMLWriterDataSource => "ABC Additional Codes";
		public bool? SupportsMultipleLanguagesForTesting { get; set; }
		public bool SupportsMultipleLanguagesExposed => base.SupportsMultipleLanguages;
		protected override bool SupportsMultipleLanguages => SupportsMultipleLanguagesForTesting.HasValue ? SupportsMultipleLanguagesForTesting.Value : base.SupportsMultipleLanguages;
	}
}
