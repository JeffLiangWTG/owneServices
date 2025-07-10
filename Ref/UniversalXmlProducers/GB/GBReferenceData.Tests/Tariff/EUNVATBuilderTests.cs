using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff
{
	[TestFixture]
	class EUNVATBuilderTests
	{
		[Test]
		public void FilePrefix()
		{
			Assert.That(builder.FilePrefix, Is.EqualTo("GB_RefCusTariff_EUN_VAT"));
		}

		[Test]
		public void ConvertModelsToRefModel()
		{
			var measure1 = new Measure
			{
				ItemId = "0100000001",
				CleanId = "0100000001",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Test Model Convertion 1.1",
				CompositeKey = "03.12.34",
				OrderNumber = "ORD12345",
				AdditionalCode = "123",
				AdditionalCodeType = "4",
				GeographicalArea = "1001",
				Formula = "RateFormula",
				MeasureType = "103",
				MeasureTypeSeries = "C",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { "100" },
				ConditionClass = MeasureHelper.ConditionClass.Rate,
				TariffTypes = new List<string> { "IMP" },
			}
			.SetComponents(new[]
			{
				new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "1", DutyExpression = "01", HJID = "1" },
				new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "2", DutyExpression = "99", HJID = "2"}
			})
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					MeasurementUnit = "UOM",
					MeasurementUnitQualifier = "2",
					ConditionCode = "A",
					ConditionCodeDescription = "Condition Code Type A",
					MeasureAction = "01",
					IsRateFormulaCondition = true,
					HJID = "3"
				}
				.SetComponents(new[]
				{
					new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "2", DutyExpression = "01", HJID = "4" },
					new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "3", DutyExpression = "02", HJID = "5" }
				}),
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "Condition Code Type B",
					MeasureAction = "99",
					CertificateCode = "999",
					CertificateTypeCode = "X",
					HJID = "6"
				},
				new MeasureCondition
				{
					ConditionCode = "B",
					ConditionCodeDescription = "Condition Code Type B",
					MeasureAction = "99",
					CertificateCode = "888",
					CertificateTypeCode = "Y",
					HJID = "7"
				}
			})
			.SetExcludedGeographicalAreas(new[] { new ExcludedGeographicalArea { Value = "EX1", HJID = "1" }, new ExcludedGeographicalArea { Value = "EX2", HJID = "2" } });

			var measure2 = new Measure
			{
				ItemId = "0100000001",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Reduced VAT",
				CompositeKey = "03.12.34",
				GeographicalArea = "1001",
				Formula = "RateFormula",
				MeasureType = "305",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Vat",
				RateType = "DTY",
				RateCode = "A00",
				Preferences = new List<string> { null },
				ConditionClass = MeasureHelper.ConditionClass.Vat,
				VatCode = "650",

				TariffTypes = new List<string> { "IMP" },
			}
			.SetComponents(new[] { new MeasureComponent { DutyExpression = "01", DutyAmount = 20.0m, HJID = "8" } });

			var measure3 = new Measure // different tariff - No VAT
			{
				ItemId = "2020200000",
				CleanId = "202020",
				StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
				EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
				Description = "Test Model Convertion 2.1",
				CompositeKey = "04.20.20",
				MeasureType = "103",
				MeasureTypeSeries = "A",
				MeasureTypeDescription = "Third country duty",
				RateType = "DTY",
				Preferences = new List<string> { "100" },
				ConditionClass = MeasureHelper.ConditionClass.Class,
				Formula = "ConditionFormula",
				TariffTypes = new List<string> { "EXP" },
			}
			.SetConditions(new[]
			{
				new MeasureCondition
				{
					MeasurementUnit = "UOM",
					MeasurementUnitQualifier = "2",
					ConditionCode = "A",
					ConditionCodeDescription = "Condition Code Type A",
					MeasureAction = "55",
					HJID = "9"
				}
			});

			var tariffModels = new List<ITariffModel>() { measure1, measure2, measure3 };

			var refModels = builder.ConvertToRefModels(tariffModels);

			Assert.That(refModels, Is.Not.Null);
			Assert.That(refModels.Count, Is.EqualTo(1));

			var vatModel = refModels.First();

			Assert.That(vatModel.RefCusRates?.Any() ?? false, Is.EqualTo(false));
			Assert.That(vatModel.RefCusConditions?.Any() ?? false, Is.EqualTo(false));
			Assert.That(vatModel.RefCusTariffAttributes?.Any() ?? false, Is.EqualTo(false));
			Assert.That(vatModel.RefCusTariffUOMs?.Any() ?? false, Is.EqualTo(false));
		}

		[Test]
		public void XmlWriterConfig()
		{
			var config = builder.XmlWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCusTariff);
			var tariffConfig = config.GetConfiguration(refType);

			Assert.That(tariffConfig, Is.Not.Null);
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_TariffCode))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_NKTariffType))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_ZZZ_NKDataGrouping))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusVATApplicabilities))));
			Assert.That(tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_IAMUnique))));

			Assert.That(!tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_Description))));
			Assert.That(!tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_StartDate))));
			Assert.That(!tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_EndDate))));
			Assert.That(!tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_CompositeKeyOnZZ5))));
			Assert.That(!tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusConditions))));
			Assert.That(!tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusRates))));
			Assert.That(!tariffConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusTariffUOMs))));

			refType = typeof(RefCusVATApplicability);
			var vatApplConfig = config.GetConfiguration(refType);

			Assert.That(vatApplConfig, Is.Not.Null);
			Assert.That(vatApplConfig.IsIncluded(refType.GetProperty(nameof(RefCusVATApplicability.ZX5_AdditionalCode))));
			Assert.That(vatApplConfig.IsIncluded(refType.GetProperty(nameof(RefCusVATApplicability.ZX5_StartDate))));
			Assert.That(vatApplConfig.IsIncluded(refType.GetProperty(nameof(RefCusVATApplicability.ZX5_EndDate))));
			Assert.That(vatApplConfig.IsIncluded(refType.GetProperty(nameof(RefCusVATApplicability.ZX5_ZZF_NKTaxOrFeeCode))));
			Assert.That(vatApplConfig.IsIncluded(refType.GetProperty(nameof(RefCusVATApplicability.ZX5_ZZZ_NKDataGrouping))));

			var keys = vatApplConfig.GetKeySets().Select(x => x.PropertySchema.Name).ToList();
			Assert.That(string.Join(",", keys), Is.EqualTo("ZX5_ZZZ_NKDataGrouping,ZX5_ZZF_NKTaxOrFeeCode,ZX5_AdditionalCode"));

			refType = typeof(RefCusTariffUOM);
			Assert.Null(config.GetConfiguration(refType));

			refType = typeof(RefCusRate);
			Assert.Null(config.GetConfiguration(refType));

			refType = typeof(RefCusApplicability);
			Assert.Null(config.GetConfiguration(refType));

			refType = typeof(RefCusRateUOM);
			Assert.Null(config.GetConfiguration(refType));

			refType = typeof(RefCusExcludedTradeGroup);
			Assert.Null(config.GetConfiguration(refType));

			refType = typeof(RefCusCondition);
			Assert.Null(config.GetConfiguration(refType));

			refType = typeof(RefCusConditionValue);
			Assert.Null(config.GetConfiguration(refType));
		}

		[Test]
		public void BuildXmlFile()
		{
			var models = new List<ITariffModel>
			{
				new Measure
				{
					ItemId = "0100000001",
					StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
					EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
					Description = "Standard VAT",
					CompositeKey = "03.12.34",
					GeographicalArea = "1001",
					Formula = "RateFormula",
					MeasureType = "305",
					MeasureTypeSeries = "A",
					MeasureTypeDescription = "Vat",
					RateType = "DTY",
					RateCode = "A00",
					Preferences = new List<string> { null },
					ConditionClass = MeasureHelper.ConditionClass.Vat,
					VatCode = "666",
					AdditionalCode = "ABC",
					AdditionalCodeType = "V",

					TariffTypes = new List<string> { "IMP" },
				}
				.SetComponents(new[] { new MeasureComponent { DutyExpression = "01", DutyAmount = 20.0m, HJID = "1" } })
			};

			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			var errorCollector = new StringBuilder();
			var builder = new EUNVATBuilderTester(dateTimeProvider.Object, errorCollector);

			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, "UnitTest");

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Output.RefCusTariff_UT002.xml");
			Assert.That(xml, Is.EqualTo(expectedXml));
		}

		[Test]
		public void TariffFilter()
		{
			var tariff = new RefCusTariff();

			Assert.That(builder.TariffFilter(null), Is.EqualTo(false));
			Assert.That(builder.TariffFilter(tariff), Is.EqualTo(false));

			tariff.RefCusVATApplicabilities = new RefCusVATApplicability[] { };
			Assert.That(builder.TariffFilter(tariff), Is.EqualTo(false));

			tariff.RefCusVATApplicabilities = new RefCusVATApplicability[] { new RefCusVATApplicability() };
			Assert.That(builder.TariffFilter(tariff), Is.EqualTo(true));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			dateTimeProvider.Setup(x => x.UTCHistoricalDate).Returns(new DateTime(2018, 11, 04, 0, 0, 0, 0));
			errorCollector = new StringBuilder();
			builder = new EUNVATBuilderTester(dateTimeProvider.Object, errorCollector);
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

		EUNVATBuilderTester builder;
		StringBuilder errorCollector;
		string TempFolder;
	}

	internal class EUNVATBuilderTester : EUNVATBuilder
	{
		public EUNVATBuilderTester(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		public new string FilePrefix => base.FilePrefix;
		public new string XMLWriterDataSource => base.XMLWriterDataSource;
		public new IEnumerable<RefCusTariff> ConvertToRefModels(List<ITariffModel> data) => base.ConvertToRefModels(data);
		public new XmlWriterConfiguration XmlWriterConfiguration() => base.XmlWriterConfiguration();
		public new bool IsValid(RefCusTariff refModel, string source) => base.IsValid(refModel, source);
		public new bool IsExpired(RefCusTariff refModel) => base.IsExpired(refModel);
		public new void DuplicateError(RefCusTariff refModel, string uniqueId, string source) => base.DuplicateError(refModel, uniqueId, source);
		public new string UniqueId(RefCusTariff refModel) => base.UniqueId(refModel);
		public new string OutputFileName => base.OutputFileName;
		public new Predicate<RefCusTariff> TariffFilter => base.TariffFilter;
	}
}
