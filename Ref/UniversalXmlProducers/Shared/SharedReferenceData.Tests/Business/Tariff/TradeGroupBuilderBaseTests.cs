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
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Tests
{
	[TestFixture]
	class TradeGroupBuilderBaseTests
	{
		[Test]
		public void ConvertModelToRefModel()
		{
			var model1 = new GeographicalArea
			{
				GeographicalAreaId = "1011",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2020, 12, 31, 23, 59, 59),
				Description = "Test Model Convertion",
			}
			.SetCountries(new[]
			{
					new GeographicalArea.GeographicalAreaCountry { HJID = "1", CountryCode = "ZA", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59), Description = "South Africa" },
					new GeographicalArea.GeographicalAreaCountry { HJID = "2", CountryCode = "AU", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59), Description = "Australia" }
			});


			var model2 = new GeographicalArea
			{
				GeographicalAreaId = "ZA",
				Description = "South Africa",
				StartDate = new DateTime(2020, 02, 03),
				EndDate = new DateTime(2050, 06, 30),
			};

			var model3 = new GeographicalArea
			{
				GeographicalAreaId = "404",
				Description = "Homeless",
				StartDate = new DateTime(2020, 02, 03),
				EndDate = new DateTime(2050, 06, 30),
			};

			var model4 = new GeographicalArea
			{
				GeographicalAreaId = "BE",
				Description = "Belgium",
				StartDate = new DateTime(2020, 02, 03),
				EndDate = new DateTime(2050, 06, 30),
			}
			.SetDescriptions(new[]
			{
				new DescriptionPeriods.DescriptionModel { HJID = "1", LanguageCode = "NL", Description = "België" },
				new DescriptionPeriods.DescriptionModel { HJID = "2", LanguageCode = "EN", Description = "Belgium" },
				new DescriptionPeriods.DescriptionModel { HJID = "3", LanguageCode = "FR", Description = "Belgique" },
				new DescriptionPeriods.DescriptionModel { HJID = "4", LanguageCode = "DE", Description = "Belgien" },
			});

			var tariffModels = new List<ITariffModel>() { model1, model2, model3, model4 };

			var refModels = builder.ConvertToRefModels(tariffModels);
			Assert.That(refModels.Count, Is.EqualTo(4));

			var refModel = refModels.FirstOrDefault(x => x.ZZA_TradeGroup == "1011");

			Assert.That(refModel, Is.Not.Null);
			Assert.That(refModel.ZZA_Description, Is.EqualTo(model1.Description));
			Assert.That(refModel.ZZA_StartDate, Is.EqualTo(new DateTime(2019, 10, 1, 15, 14, 0)));
			Assert.That(refModel.ZZA_EndDate, Is.EqualTo(new DateTime(2020, 12, 31, 23, 59, 0)));
			Assert.That(refModel.RefCusTradeGroupCountries, Is.Not.Null);
			Assert.That(refModel.RefCusTradeGroupCountries.Length, Is.EqualTo(2));
			Assert.That(refModel.RefCusTradeGroupCountries.Any(x => x.ZZB_RN_NKTradeGroupCountryCode == "AU"));

			var zaCountry = refModel.RefCusTradeGroupCountries.FirstOrDefault(x => x.ZZB_RN_NKTradeGroupCountryCode == "ZA");
			Assert.That(zaCountry, Is.Not.Null);
			Assert.That(zaCountry.ZZB_StartDate, Is.EqualTo(new DateTime(2020, 01, 01)));
			Assert.That(zaCountry.ZZB_EndDate, Is.EqualTo(new DateTime(2021, 12, 31, 23, 59, 0)));
			Assert.That(zaCountry.ZZB_Description, Is.EqualTo("South Africa"));

			model1.StartDate = null;
			model1.EndDate = null;

			refModel = builder.ConvertToRefModels(tariffModels).First();

			Assert.That(refModel.ZZA_StartDate, Is.EqualTo(DefaultValues.MinimumDateTime));
			Assert.That(refModel.ZZA_EndDate, Is.EqualTo(DefaultValues.MaximumDateTime));

			refModel = refModels.FirstOrDefault(x => x.ZZA_TradeGroup == "ZA");
			Assert.That(refModel, Is.Not.Null);
			Assert.That(refModel.RefCusTradeGroupCountries, Is.Not.Null.And.Not.Empty);
			Assert.That(refModel.RefCusTradeGroupCountries.Length, Is.EqualTo(1));
			Assert.That(refModel.RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode, Is.EqualTo("ZA"));
			Assert.That(refModel.RefCusTradeGroupCountries[0].ZZB_StartDate, Is.EqualTo(model2.StartDate));
			Assert.That(refModel.RefCusTradeGroupCountries[0].ZZB_EndDate, Is.EqualTo(new DateTime(2050, 06, 29, 23, 59, 0)));

			refModel = refModels.FirstOrDefault(x => x.ZZA_TradeGroup == "404");
			Assert.That(refModel, Is.Not.Null);
			Assert.That(refModel.RefCusTradeGroupCountries, Is.Null.Or.Empty);

			refModel = refModels.FirstOrDefault(x => x.ZZA_TradeGroup == "BE");

			Assert.That(refModel, Is.Not.Null);
			Assert.That(refModel.ZZA_Description, Is.EqualTo(model4.Description));
			Assert.That(refModel.ZZA_StartDate, Is.EqualTo(model4.StartDate));
			Assert.That(refModel.ZZA_EndDate, Is.EqualTo(new DateTime(2050, 06, 29, 23, 59, 0)));
			Assert.That(refModel.RefCusTradeGroupLanguages, Is.Null);
		}

		[Test]
		public void ConvertModelToRefModel_MultipleLanguages()
		{
			builder.SupportsMultipleLanguagesForTesting = true;
			var area = new GeographicalArea
			{
				GeographicalAreaId = "BE",
				StartDate = new DateTime(2020, 02, 03),
				EndDate = new DateTime(2050, 06, 30),
				Description = "Item",
			}
			.SetDescriptions(new[]
			{
				new DescriptionPeriods.DescriptionModel { HJID = "1", LanguageCode = "NL", Description = "Dutch" },
				new DescriptionPeriods.DescriptionModel { HJID = "2", LanguageCode = "EN", Description = "English" },
				new DescriptionPeriods.DescriptionModel { HJID = "3", LanguageCode = "FR", Description = "French" },
				new DescriptionPeriods.DescriptionModel { HJID = "4", LanguageCode = "DE", Description = "German" },
			});

			var models = builder.ConvertToRefModels(new List<ITariffModel> { area }).ToList();

			Assert.That(models, Is.Not.Null);
			Assert.That(models.Count, Is.EqualTo(1));

			var model = models[0];
			Assert.That(model, Is.Not.Null);
			Assert.That(model.RefCusTradeGroupLanguages, Is.Not.Null);
			Assert.That(model.RefCusTradeGroupLanguages.Length, Is.EqualTo(4));
			Assert.That(model.RefCusTradeGroupLanguages.Any(x => x.ZXD_ZX6_NKLanguage == "NL"));
			Assert.That(model.RefCusTradeGroupLanguages.Any(x => x.ZXD_ZX6_NKLanguage == "EN"));
			Assert.That(model.RefCusTradeGroupLanguages.Any(x => x.ZXD_ZX6_NKLanguage == "FR"));
			Assert.That(model.RefCusTradeGroupLanguages.Any(x => x.ZXD_ZX6_NKLanguage == "DE"));
			Assert.That(model.RefCusTradeGroupLanguages.First(x => x.ZXD_ZX6_NKLanguage == "NL").ZXD_Description, Is.EqualTo("Dutch"));
		}

		[Test]
		public void TradeGroupCountryFiltering()
		{
			var model1 = new GeographicalArea
			{
				GeographicalAreaId = "1011",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Test Model Convertion",
			}
			.SetCountries(new[]
			{
				new GeographicalArea.GeographicalAreaCountry { HJID = "1", CountryCode = "ZA", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59) },
				new GeographicalArea.GeographicalAreaCountry { HJID = "2", CountryCode = "AU", StartDate = new DateTime(2020, 02, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59) },
				new GeographicalArea.GeographicalAreaCountry { HJID = "3", CountryCode = "ZA", StartDate = new DateTime(2019, 01, 01), EndDate = new DateTime(2020, 12, 31, 23, 59, 59) }, //Extend
				new GeographicalArea.GeographicalAreaCountry { HJID = "4", CountryCode = "DI", StartDate = new DateTime(2015, 01, 01), EndDate = new DateTime(2017, 12, 31, 23, 59, 59) }, //Expired
				new GeographicalArea.GeographicalAreaCountry { HJID = "5", CountryCode = "AU", StartDate = new DateTime(2019, 01, 01), EndDate = new DateTime(2020, 01, 31, 23, 59, 59) }  //Additional
			});

			var refModels = builder.ConvertToRefModels(new List<ITariffModel> { model1 });
			Assert.That(refModels.Count, Is.EqualTo(1));

			var refModel = refModels.FirstOrDefault(x => x.ZZA_TradeGroup == "1011");
			Assert.That(refModel.RefCusTradeGroupCountries, Is.Not.Null);
			Assert.That(refModel.RefCusTradeGroupCountries.Count, Is.EqualTo(3));
			Assert.That(refModel.RefCusTradeGroupCountries.Any(x => x.ZZB_RN_NKTradeGroupCountryCode == "DI"), Is.EqualTo(false));
			Assert.That(refModel.RefCusTradeGroupCountries.Where(x => x.ZZB_RN_NKTradeGroupCountryCode == "ZA").Count(), Is.EqualTo(1));
			Assert.That(refModel.RefCusTradeGroupCountries.Where(x => x.ZZB_RN_NKTradeGroupCountryCode == "AU").Count(), Is.EqualTo(2));
			var za = refModel.RefCusTradeGroupCountries.First(x => x.ZZB_RN_NKTradeGroupCountryCode == "ZA");
			Assert.That(za.ZZB_StartDate, Is.EqualTo(new DateTime(2019, 01, 01)));
			Assert.That(za.ZZB_EndDate, Is.EqualTo(new DateTime(2021, 12, 31, 23, 59, 0)));
		}

		[Test]
		public void XmlWriterConfig()
		{
			builder.SupportsMultipleLanguagesForTesting = false;
			var config = builder.XmlWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCusTradeGroup);
			var entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig, Is.Not.Null);
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroup.ZZA_TradeGroup))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroup.ZZA_Description))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroup.ZZA_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroup.ZZA_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroup.ZZA_ZZZ_NKDataGrouping))));
			Assert.That(!entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroup.RefCusTradeGroupLanguages))));

			refType = typeof(RefCusTradeGroupCountry);
			entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroupCountry.ZZB_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroupCountry.ZZB_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroupCountry.ZZB_Description))));

			refType = typeof(RefCusTradeGroupLanguage);
			Assert.Null(config.GetConfiguration(refType));
		}

		[Test]
		public void XmlWriterConfig_MultipleLanguages()
		{
			builder.SupportsMultipleLanguagesForTesting = true;
			var config = builder.XmlWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCusTradeGroup);
			var entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig, Is.Not.Null);
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroup.RefCusTradeGroupLanguages))));

			refType = typeof(RefCusTradeGroupLanguage);
			entityConfig = config.GetConfiguration(refType);
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroupLanguage.ZXD_ZX6_NKLanguage))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroupLanguage.ZXD_Description))));
		}

		[Test]
		public void UniqueId()
		{
			var model = new RefCusTradeGroup { ZZA_TradeGroup = "1234", ZZA_StartDate = new DateTime(2019, 01, 26, 12, 13, 14), ZZA_EndDate = new DateTime(2019, 02, 17, 09, 33, 04) };

			Assert.That(builder.UniqueId(model), Is.EqualTo("1234_20190126121314_20190217093304"));
		}

		[Test]
		public void DuplicateErrors()
		{
			var model = new RefCusTradeGroup { ZZA_TradeGroup = "1234", ZZA_Description = "Unit Testing" };

			builder.DuplicateError(model, "1234", "");
			Assert.That(errorCollector.ToString(), Contains.Substring("TradeGroup duplicate exists. Key: '1234'"));
		}

		[Test]
		public void IsValid()
		{
			var model = new RefCusTradeGroup();
			model.RefCusTradeGroupCountries = new RefCusTradeGroupCountry[] { new RefCusTradeGroupCountry() };

			Assert.That(builder.IsValid(model, ""), Is.False);
			Assert.That(errorCollector.ToString(), Contains.Substring("TradeGroup validation error")
														.And.Contains("ZZA_TradeGroup is required")
														.And.Contains("ZZA_Description is required")
														.And.Contains("ZZB_RN_NKTradeGroupCountryCode is required"));

			errorCollector.Clear();
			model.ZZA_TradeGroup = "1234t";
			Assert.That(builder.IsValid(model, ""), Is.False);
			Assert.That(errorCollector.ToString(), Does.Not.Contain("ZZA_TradeGroup is required"));

			errorCollector.Clear();
			model.ZZA_Description = "Unit Test";
			Assert.That(builder.IsValid(model, ""), Is.False);
			Assert.That(errorCollector.ToString(), Does.Not.Contain("ZZA_Description is required"));

			errorCollector.Clear();
			model.RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode = "ZA";
			Assert.That(builder.IsValid(model, ""), Is.True);
			Assert.That(errorCollector.ToString(), Does.Not.Contain("ZZB_RN_NKTradeGroupCountryCode is required"));

			Assert.That(errorCollector.ToString(), Does.Not.Contain("TradeGroup validation error"));
		}

		[Test]
		public void IsExpired()
		{
			var model = new RefCusTradeGroup();

			model.ZZA_EndDate = DateTime.MinValue;
			Assert.That(builder.IsExpired(model), Is.True);

			model.ZZA_EndDate = DateTime.Today.AddYears(-1);
			Assert.That(builder.IsExpired(model), Is.False);

			model.ZZA_EndDate = DateTime.MaxValue.Date;
			Assert.That(builder.IsExpired(model), Is.False);
		}

		[Test]
		public void BuildXmlFile()
		{
			var models = new List<ITariffModel>()
			{
				new GeographicalArea
				{
					GeographicalAreaId = "1011",
					StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
					EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
					Description = "Test Model Convertion",
				}
				.SetCountries(new[]
				{
					new GeographicalArea.GeographicalAreaCountry { HJID = "1", CountryCode = "ZA", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59), Description = "South Africa" },
					new GeographicalArea.GeographicalAreaCountry { HJID = "2", CountryCode = "AU", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59), Description = "Australia" }
				}),
				new GeographicalArea { GeographicalAreaId = "ZA", Description = "South Africa", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59) },
				new GeographicalArea { GeographicalAreaId = "ZA", Description = "Duplicate", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59) },
				new GeographicalArea { GeographicalAreaId = "GB", Description = "No Start Date", EndDate = new DateTime(2021, 12, 31, 23, 59, 59) },
				new GeographicalArea { GeographicalAreaId = "US", Description = "No End Date", StartDate = new DateTime(2020, 01, 01) },
				new GeographicalArea { GeographicalAreaId = "", Description = "Validation error 1" },
				new GeographicalArea { GeographicalAreaId = "FR", Description = "Validation error 2" }.SetCountries(new[] { new GeographicalArea.GeographicalAreaCountry { HJID = "1", CountryCode = "" } }),
				new GeographicalArea { GeographicalAreaId = "404", Description = "No trade group country" },
				new GeographicalArea
				{
					GeographicalAreaId = "BE",
					Description = "Belgium",
				}
				.SetDescriptions(new[]
				{
					new DescriptionPeriods.DescriptionModel { HJID = "1", LanguageCode = "NL", Description = "België" },
					new DescriptionPeriods.DescriptionModel { HJID = "2", LanguageCode = "EN", Description = "Belgium" },
					new DescriptionPeriods.DescriptionModel { HJID = "3", LanguageCode = "FR", Description = "Belgique" },
					new DescriptionPeriods.DescriptionModel { HJID = "4", LanguageCode = "DE", Description = "Belgien" },
				})
			};

			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			var errorCollector = new StringBuilder();
			var builder = new TradeGroupBuilderTester(dateTimeProvider.Object, errorCollector);

			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, "");

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Output.RefCusTradeGroup_001.xml");
			Assert.That(xml, Is.EqualTo(expectedXml));
		}


		[Test]
		public void BuildXmlFile_MultipleLanguages()
		{
			var models = new List<ITariffModel>()
			{
				new GeographicalArea
				{
					GeographicalAreaId = "1011",
					StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
					EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
					Description = "Test Model Convertion",
				}
				.SetCountries(new[]
				{
					new GeographicalArea.GeographicalAreaCountry { HJID = "1", CountryCode = "ZA", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59), Description = "South Africa" },
					new GeographicalArea.GeographicalAreaCountry { HJID = "2", CountryCode = "AU", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59), Description = "Australia" }
				}),
				new GeographicalArea { GeographicalAreaId = "ZA", Description = "South Africa", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59) },
				new GeographicalArea { GeographicalAreaId = "ZA", Description = "Duplicate", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59) },
				new GeographicalArea { GeographicalAreaId = "GB", Description = "No Start Date", EndDate = new DateTime(2021, 12, 31, 23, 59, 59) },
				new GeographicalArea { GeographicalAreaId = "US", Description = "No End Date", StartDate = new DateTime(2020, 01, 01) },
				new GeographicalArea { GeographicalAreaId = "", Description = "Validation error 1" },
				new GeographicalArea { GeographicalAreaId = "FR", Description = "Validation error 2" }.SetCountries(new[] { new GeographicalArea.GeographicalAreaCountry { HJID = "1", CountryCode = "" } }),
				new GeographicalArea { GeographicalAreaId = "404", Description = "No trade group country" },
				new GeographicalArea
				{
					GeographicalAreaId = "BE",
					Description = "Belgium",
				}
				.SetDescriptions(new[]
				{
					new DescriptionPeriods.DescriptionModel { HJID = "1", LanguageCode = "NL", Description = "België" },
					new DescriptionPeriods.DescriptionModel { HJID = "2", LanguageCode = "EN", Description = "Belgium" },
					new DescriptionPeriods.DescriptionModel { HJID = "3", LanguageCode = "FR", Description = "Belgique" },
					new DescriptionPeriods.DescriptionModel { HJID = "4", LanguageCode = "DE", Description = "Belgien" },
				})
			};

			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			var errorCollector = new StringBuilder();
			var builder = new TradeGroupBuilderTester(dateTimeProvider.Object, errorCollector);
			builder.SupportsMultipleLanguagesForTesting = true;

			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, "");

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Output.RefCusTradeGroup_001_MultipleLanguages.xml");
			Assert.That(xml, Is.EqualTo(expectedXml));
		}

		[Test]
		public void TestSupportsMultipleLanguages()
		{
			Assert.That(builder.SupportsMultipleLanguagesExposed, Is.EqualTo(false));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2020, 11, 01, 0, 0, 0, 0));
			dateTimeProvider.Setup(x => x.UTCHistoricalDate).Returns(new DateTime(2019, 11, 01, 0, 0, 0, 0));
			errorCollector = new StringBuilder();
			builder = new TradeGroupBuilderTester(dateTimeProvider.Object, errorCollector);
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

		TradeGroupBuilderTester builder;
		StringBuilder errorCollector;
		string TempFolder;
	}

	internal class TradeGroupBuilderTester : TradeGroupBuilderBase
	{
		public TradeGroupBuilderTester(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		public new IEnumerable<RefCusTradeGroup> ConvertToRefModels(List<ITariffModel> data) => base.ConvertToRefModels(data);
		public new XmlWriterConfiguration XmlWriterConfiguration() => base.XmlWriterConfiguration();
		public new bool IsValid(RefCusTradeGroup refModel, string source) => base.IsValid(refModel, source);
		public new bool IsExpired(RefCusTradeGroup refModel) => base.IsExpired(refModel);
		public new void DuplicateError(RefCusTradeGroup refModel, string uniqueId, string source) => base.DuplicateError(refModel, uniqueId, source);
		public new string UniqueId(RefCusTradeGroup refModel) => base.UniqueId(refModel);
		public new string OutputFileName => base.OutputFileName;

		protected override string DataGrouping => "ABC";
		protected override string FilePrefix => "UTTradeGroup";
		protected override string XMLWriterDataSource => "ABC Trade Groups";
		public bool? SupportsMultipleLanguagesForTesting { get; set; }
		public bool SupportsMultipleLanguagesExposed => base.SupportsMultipleLanguages;
		protected override bool SupportsMultipleLanguages => SupportsMultipleLanguagesForTesting.HasValue ? SupportsMultipleLanguagesForTesting.Value : base.SupportsMultipleLanguages;
	}
}
