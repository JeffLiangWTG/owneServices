using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff
{
	[TestFixture]
	class TariffBuilderTests
	{
		[Test]
		public void FilePrefix()
		{
			Assert.That(builder.FilePrefix, Is.EqualTo("GB_RefCusTariff"));
		}

		[Test]
		public void BuildXmlFile()
		{
			var models = new List<ITariffModel>()
			{
				new Measure { StartDate = new DateTime(2019,01,01), EndDate = new DateTime(2019,12,31, 23,59,59), NomenclatureStartDate = new DateTime(2019,01,01), NomenclatureEndDate = new DateTime(2019,12,31, 23,59,59), ItemId = "1010000000", CleanId = "1010",  Description = "Item1", CompositeKey = "01.10.10", TariffTypes = new List<string> { "IMP" }, GeographicalArea = "1001" },
				new Measure { StartDate = new DateTime(2019,01,01), EndDate = new DateTime(2019,12,31, 23,59,59), NomenclatureStartDate = new DateTime(2019,01,01), NomenclatureEndDate = new DateTime(2019,12,31, 23,59,59), ItemId = "2020000000", CleanId = "2020",  Description = "Item2", CompositeKey = "01.20.20", TariffTypes = new List<string> { "IMP" }, GeographicalArea = "1001" },
				new Measure { StartDate = new DateTime(2019,01,01), EndDate = new DateTime(2019,12,31, 23,59,59), NomenclatureStartDate = new DateTime(2019,01,01), NomenclatureEndDate = new DateTime(2019,12,31, 23,59,59), ItemId = "2020000000", CleanId = "2020",  Description = "Duplicate Item2", CompositeKey = "01.20.20", TariffTypes = new List<string> { "IMP" }, GeographicalArea = "1001" },
				new Measure { StartDate = new DateTime(2019,01,01), NomenclatureStartDate = new DateTime(2019,01,01), ItemId = "3456000000", CleanId = "3456", Description = "NoEndDate", CompositeKey = "02.34.56", TariffTypes = new List<string> { "IMP" }, GeographicalArea = "1001" },
				new Measure { EndDate = new DateTime(2019,12,31, 23,59,59), NomenclatureEndDate = new DateTime(2019,12,31, 23,59,59), ItemId = "4025000000", CleanId = "4025", Description = "NoStartDate", CompositeKey = "03.40.25", TariffTypes = new List<string> { "IMP" }, GeographicalArea = "1001" },
				new Measure { StartDate = new DateTime(2019,01,01), EndDate = new DateTime(2019,12,31, 23,59,59), NomenclatureStartDate = new DateTime(2019,01,01), NomenclatureEndDate = new DateTime(2019,12,31, 23,59,59), ItemId = "9876543210", CleanId = "Invalid", Description = "", TariffTypes = new List<string> { "IMP" }, GeographicalArea = "1001" },
				new Measure
				{
					ItemId = "1234560000",
					CleanId = "123456",
					StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
					EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
					NomenclatureStartDate = new DateTime(2019, 10, 01, 15, 14, 13),
					NomenclatureEndDate = new DateTime(2019, 12, 31, 23, 59, 59),
					Description = "XML structure test",
					CompositeKey = "03.12.34",
					OrderNumber = "123456",
					AdditionalCode = "123",
					AdditionalCodeType = "4",
					GeographicalArea = "1001",
					Formula = "RateFormula",
					MeasureType = "103",
					MeasureTypeSeries = "A",
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
					new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "2", DutyExpression = "99", HJID = "2" }
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
				.SetExcludedGeographicalAreas(new[] { new ExcludedGeographicalArea { Value = "EX1", HJID = "1" }, new ExcludedGeographicalArea { Value = "EX2", HJID = "2" } }),
				new Measure
				{
					ItemId = "0100000001",
					StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
					EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
					NomenclatureStartDate = new DateTime(2019, 10, 01, 15, 14, 13),
					NomenclatureEndDate = new DateTime(2019, 12, 31, 23, 59, 59),
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
				.SetComponents(new[] { new MeasureComponent { DutyExpression = "01", DutyAmount = 20.0m, HJID = "8" } })
			};

			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			var errorCollector = new StringBuilder();
			var builder = new TariffBuilderTester(dateTimeProvider.Object, errorCollector);

			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, "UnitTest");

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Output.RefCusTariff_UT001.xml");
			Assert.That(xml, Is.EqualTo(expectedXml));
		}

		[TestCase("484")] // measure type mapped in MeasureMappingProvider
		[TestCase("XXX")] // measure type not mapped in MeasureMappingProvider
		public void MappedAndUnmappedMeasureTypesAreSupported(string measureType)
		{
			var models = new List<ITariffModel>()
			{
				new Measure
				{
					ItemId = "1234560000",
					CleanId = "123456",
					StartDate = new DateTime(2024, 10, 01, 15, 14, 13),
					EndDate = null,
					NomenclatureStartDate = new DateTime(2024, 10, 01, 15, 14, 13),
					NomenclatureEndDate = null,
					Description = "Measure Type mapping test",
					CompositeKey = "03.12.34",
					OrderNumber = "123456",
					AdditionalCode = "123",
					AdditionalCodeType = "4",
					GeographicalArea = "1001",
					Formula = "Formula",
					MeasureType = measureType,
					MeasureTypeSeries = "R",
					MeasureTypeDescription = "Test measure type",
					RateType = "DTY",
					RateCode = "A00",
					Preferences = new List<string> { "100" },
					ConditionClass = MeasureHelper.ConditionClass.Class,
					TariffTypes = new List<string> { "IMP" },
				}
				.SetComponents(new[]
				{
					new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "1", DutyExpression = "01", HJID = "1" },
					new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "2", DutyExpression = "99", HJID = "2" }
				})
				.SetConditions(new[]
				{
					new MeasureCondition
					{
						MeasurementUnit = "UOM",
						MeasurementUnitQualifier = "2",
						ConditionCode = "A",
						ConditionCodeDescription = "Condition Code Type A",
						Formula = "Formula",
						MeasureAction = "01",
						IsRateFormulaCondition = false,
						HJID = "3"
					}
					.SetComponents(new[]
					{
						new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "2", DutyExpression = "01", HJID = "4" },
						new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "3", DutyExpression = "02", HJID = "5" }
					})
				})
				.SetExcludedGeographicalAreas(new[] { new ExcludedGeographicalArea { Value = "EX1", HJID = "1" }, new ExcludedGeographicalArea { Value = "EX2", HJID = "2" } })
			};

			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2024, 11, 04, 22, 36, 45, 135));
			var errorCollector = new StringBuilder();
			var builder = new TariffBuilderTester(dateTimeProvider.Object, errorCollector);

			var publicationDate = new DateTime(2024, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, "UnitTest");

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			Assert.That(xml.Contains($"<ZX1_ZX2_NKConditionType>{measureType}</ZX1_ZX2_NKConditionType>"), Is.True);
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			dateTimeProvider.Setup(x => x.UTCHistoricalDate).Returns(new DateTime(2018, 11, 04, 0, 0, 0, 0));
			errorCollector = new StringBuilder();
			builder = new TariffBuilderTester(dateTimeProvider.Object, errorCollector);
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

		TariffBuilderTester builder;
		StringBuilder errorCollector;
		string TempFolder;
	}

	internal class TariffBuilderTester : TariffBuilder
	{
		public TariffBuilderTester(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		public new string FilePrefix => base.FilePrefix;
		public new string XMLWriterDataSource => base.XMLWriterDataSource;
		public new string OutputFileName => base.OutputFileName;
	}
}
