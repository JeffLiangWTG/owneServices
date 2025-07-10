using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	[TestFixture]
	sealed class TariffBuilderTest
	{
		[Test]
		public void FilePrefix()
		{
			Assert.That(builder.FilePrefix, Is.EqualTo("BE_RefCusTariff"));
		}

		[Test]
		public void XMLWriterDataSource()
		{
			Assert.That(builder.XMLWriterDataSource, Is.EqualTo("BE Tariff"));
		}

		[Test]
		public void DataGrouping()
		{
			Assert.That(builder.DataGrouping, Is.EqualTo("BE"));
		}

		[Test]
		public void ParentDataGrouping()
		{
			Assert.That(builder.ParentDataGrouping, Is.EqualTo("EUN"));
		}

		[Test]
		public void IsEntityTypeConfigurationRefCusTariffEnableNullOrEmptyKeyMatching()
		{
			Assert.That(builder.IsEntityTypeConfigurationRefCusTariffEnableNullOrEmptyKeyMatching, Is.EqualTo(false)); 
		}

		[Test]
		public void TestMergeImportTariffsWithEUNTarrifs()
		{
			SetupEUNTestData();
			var expected = new List<RefCusTariff>()
			{
				new RefCusTariff {
					ZZ1_TariffCode = "01012100",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "03011100",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "04011010",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "05010000",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "06011010",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "07019010",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "07019020",
					ZZ1_Description = "Test Description"
				}
			};

			var expander = new UniversalXMLProducers.EUNTariffDataProducer.EUNTariffExpander(eunTariffListForTest, null);
			var result = expander.MergeImportTariffsWithEUNTarrifs(beTariffListForTest);

			Assert.AreEqual(expected.Count, result.Count);

			foreach (var item in expected)
			{
				var whereResult = result.Where(x => x.ZZ1_TariffCode == item.ZZ1_TariffCode.PadRight(10, '0')).ToList();
				Assert.AreEqual(1, whereResult.Count);
			}
		}

		public void MergeImportTariffsWithEUNTarrifsFailingTest()
		{
			SetupEUNTestData();
			var expectedToFailData = new List<RefCusTariff>()
			{
				new RefCusTariff {
					ZZ1_TariffCode = "01012100",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "03011100",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "04011010",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "05010000",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "06011010",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "07019010",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "07019020",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "13012000",
					ZZ1_Description = "Test Description"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "24011035",
					ZZ1_Description = "Test Description"
				}
			};

			var expander = new UniversalXMLProducers.EUNTariffDataProducer.EUNTariffExpander(eunTariffListForTest, null);
			var result = expander.MergeImportTariffsWithEUNTarrifs(beTariffListForTest);

			Assert.AreNotEqual(expectedToFailData.Count, result.Count);
		}

		void SetupEUNTestData()
		{
			beTariffListForTest = new List<RefCusTariff>();
			eunTariffListForTest = new List<RefCusTariff>();

			// treat as exists in BE Data
			beTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "01010000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "01012100", ZZ1_Description = "Test Description" });

			beTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "03010000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "03011100", ZZ1_Description = "Test Description" });

			beTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "04010000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "04011010", ZZ1_Description = "Test Description" });

			beTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "05010000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "05010000", ZZ1_Description = "Test Description" });

			beTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "06010000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "06011010", ZZ1_Description = "Test Description" });

			beTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07000000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07019010", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07019020", ZZ1_Description = "Test Description" });

			// treat as not exists in BE Data
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "13012000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "24011035", ZZ1_Description = "Test Description" });
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
				.SetComponents(new List<MeasureComponent>
				{
					new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "1", DutyExpression = "01", HJID = "1" },
					new MeasureComponent { MeasurementUnit = "UOM", MeasurementUnitQualifier = "2", DutyExpression = "99", HJID = "2" }
				})
				.SetConditions(new List<MeasureCondition>
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
					.SetComponents(new List<MeasureComponent>
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
				.SetExcludedGeographicalAreas(new[] { new ExcludedGeographicalArea { Value = "EX1", HJID = "8" }, new ExcludedGeographicalArea { Value = "EX2", HJID = "9" } }),

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
				.SetComponents(new List<MeasureComponent> { new MeasureComponent { DutyExpression = "01", DutyAmount = 20.0m, HJID = "10" } })
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
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.TestFiles.Output.RefCusTariff_UT001.xml");
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
		List<RefCusTariff> beTariffListForTest;
		List<RefCusTariff> eunTariffListForTest;
	}

	internal class TariffBuilderTester : TariffBuilder
	{
		public TariffBuilderTester(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		public new string FilePrefix => base.FilePrefix;
		public new string XMLWriterDataSource => base.XMLWriterDataSource;
		public new string DataGrouping => base.DataGrouping;
		public new string ParentDataGrouping => base.ParentDataGrouping;
		public new string OutputFileName => base.OutputFileName;
		public new bool IsEntityTypeConfigurationRefCusTariffEnableNullOrEmptyKeyMatching => base.IsEntityTypeConfigurationRefCusTariffEnableNullOrEmptyKeyMatching;

		protected override List<RefCusTariff> MergeImportTariffsWithEUNTarrifs(List<RefCusTariff> refCusTariffData)
		{
			return refCusTariffData;
		}
	}
}
