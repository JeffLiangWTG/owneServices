using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	[Parallelizable(ParallelScope.None)]
	sealed class NomenclaturePlusDailyProducerFixture
	{
		[Test]
		public void Run_WhenDailyFileIsGreaterThanMonthlyWhichHaveBeenPublishedInPreviousMonth()
		{
			var testDataProvider = new SampleTestDataProvider(
				monthlyFilePublishDate: new DateTime(2024, 06, 30),
				dailyFilePublishDate: new DateTime(2024, 07, 02));

			var dailyTariffUpdatesFileProvider = new DailyTariffUpdatesFileProviderMock(testDataProvider);
			var nomenclaturePlusDailyProducer = new NomenclaturePlusDailyProducerForTest(testDataProvider,
				dailyTariffUpdatesFileProvider,
				now: new DateTime(2024, 07, 02));

			nomenclaturePlusDailyProducer.Run();

			Assert.That(ApplicationConfig.NomenclatureUXmlFile, Does.Exist);

			Assert.That(File.ReadAllText(ApplicationConfig.NomenclatureUXmlFile),
				Is.EqualTo(GetExpectedNomenclatureUXmlFile("ExpectedEUNNomenclatureMergedWithDaily.xml", new DateTime(2024, 07, 02))));
			Assert.That(nomenclaturePlusDailyProducer.MinDate, Is.EqualTo(new DateTime(2024, 06, 30)));
		}

		[Test]
		public void Run_WhenLastMonthlyFileArePublishedGreaterThanDaily()
		{
			var testDataProvider = new SampleTestDataProvider(
				monthlyFilePublishDate: new DateTime(2024, 07, 30),
				dailyFilePublishDate: new DateTime(2024, 07, 02));

			var dailyTariffUpdatesFileProvider = new DailyTariffUpdatesFileProviderMock(testDataProvider);
			var nomenclaturePlusDailyProducer = new NomenclaturePlusDailyProducerForTest(testDataProvider,
				dailyTariffUpdatesFileProvider,
				now: new DateTime(2024, 07, 31));

			nomenclaturePlusDailyProducer.Run();

			Assert.That(ApplicationConfig.NomenclatureUXmlFile, Does.Exist);

			Assert.That(File.ReadAllText(ApplicationConfig.NomenclatureUXmlFile),
				Is.EqualTo(GetExpectedNomenclatureUXmlFile("ExpectedEUNNomenclature.xml", new DateTime(2024, 07, 30))));
			Assert.That(nomenclaturePlusDailyProducer.MinDate, Is.EqualTo(new DateTime(2024, 07, 30)));
		}

		[Test]
		public void Run_WhenDailyFileIsGreaterThanMonthlyWhichHaveBeenPublishedInCurrentMonth()
		{
			var testDataProvider = new SampleTestDataProvider(
				monthlyFilePublishDate: new DateTime(2024, 07, 30, 15, 10, 21),
				dailyFilePublishDate: new DateTime(2024, 07, 30, 19, 28, 00));

			var dailyTariffUpdatesFileProvider = new DailyTariffUpdatesFileProviderMock(testDataProvider);
			var nomenclaturePlusDailyProducer = new NomenclaturePlusDailyProducerForTest(testDataProvider,
				dailyTariffUpdatesFileProvider,
				now: new DateTime(2024, 07, 31));

			nomenclaturePlusDailyProducer.Run();

			Assert.That(ApplicationConfig.NomenclatureUXmlFile, Does.Exist);

			Assert.That(File.ReadAllText(ApplicationConfig.NomenclatureUXmlFile),
				Is.EqualTo(GetExpectedNomenclatureUXmlFile("ExpectedEUNNomenclatureMergedWithDaily.xml", new DateTime(2024, 07, 30, 19, 28, 00))));
			Assert.That(nomenclaturePlusDailyProducer.MinDate, Is.EqualTo(new DateTime(2024, 07, 30, 15, 10, 21)));
		}

		[Test]
		public void IsLeafIsRecalculatedForDailyUpdateRecords()
		{
			var testDataProvider = new SampleTestDataProvider(
				monthlyFilePublishDate: new DateTime(2024, 07, 30, 15, 10, 21),
				dailyFilePublishDate: new DateTime(2024, 07, 30, 19, 28, 00),
				"ExpiredDailyGoodsNomenclature.xlsx");

			var dailyTariffUpdatesFileProvider = new DailyTariffUpdatesFileProviderMock(testDataProvider);
			var nomenclaturePlusDailyProducer = new NomenclaturePlusDailyProducerForTest(testDataProvider,
				dailyTariffUpdatesFileProvider,
				now: new DateTime(2024, 07, 31));

			var result = nomenclaturePlusDailyProducer.Run();
			Assert.That(result.Tariffs.Any(x => x.ZZ1_TariffCode == "0102"), Is.True);
		}

		#region Invalid Hierarchy Position & Nomenclature Description Deletion

		[Test]
		public void InvalidHierarchyPositionFromDailyFilesDoesNotPropagate()
		{
			var testDataProvider = new InvalidHierarchyPositionAndNomenclatureDescriptionDeletionTestDataProvider(
				new DateTime(2024, 12, 30, 12, 57, 00),
				new DateTime(2024, 12, 30, 19, 15, 00));

			var nomenclaturePlusDailyProducer = new NomenclaturePlusDailyProducerForTest(
				testDataProvider,
				new DailyTariffUpdatesFileProviderMock(testDataProvider),
				now: new DateTime(2025, 01, 02));

			Assert.That(() => nomenclaturePlusDailyProducer.Run(), Throws.Nothing);

			Assert.That(ApplicationConfig.NomenclatureUXmlFile, Does.Exist);

			Assert.That(
				File.ReadAllText(ApplicationConfig.NomenclatureUXmlFile),
				Is.EqualTo(File.ReadAllText(Path.Combine(TestHelper.BasePath, "HierarchyPosition\\ExpectedNomenclature.xml"))));
		}

		class InvalidHierarchyPositionAndNomenclatureDescriptionDeletionTestDataProvider : INomenclaturePlusDailyDataTestProvider
		{
			public InvalidHierarchyPositionAndNomenclatureDescriptionDeletionTestDataProvider(DateTime monthlyFilePublishDate, DateTime dailyFilePublishDate)
			{
				this.monthlyFilePublishDate = monthlyFilePublishDate;
				this.dailyFilePublishDate = dailyFilePublishDate;
			}

			public IEnumerable<(string Path, DateTime PublicationDate)> DailyNomenclatureFileCollection
				=> new[] { (Path.Combine(BasePath, "HierarchyPosition\\NomenclatureDaily.xlsx"), dailyFilePublishDate) };

			public IEnumerable<(string Path, DateTime PublicationDate)> DailyRateFileCollection
				=> new[] { (Path.Combine(BasePath, "HierarchyPosition\\MeasuresDaily.xlsx"), dailyFilePublishDate) };

			public (string Path, DateTime PublicationDate) DeclarableCodeFile
				=> (Path.Combine(BasePath, "HierarchyPosition\\DeclarableCodes.xlsx"), monthlyFilePublishDate);

			public IEnumerable<(string Path, DateTime PublicationDate)> NomenclatureFileCollection
				=> new[]
					{
						(Path.Combine(BasePath, "HierarchyPosition\\NomenclatureMonthlyEN.xlsx"), monthlyFilePublishDate),
						(Path.Combine(BasePath, "HierarchyPosition\\NomenclatureMonthlyEL.xlsx"), monthlyFilePublishDate)
					};

			public IEnumerable<(int Number, string Description)> Sections
				=> new[] { (32, "PRODUCTS OF THE CHEMICAL OR ALLIED INDUSTRIES") };

			public (string Path, DateTime PublicationDate) DutiesImportFile
				=> (Path.Combine(BasePath, "HierarchyPosition\\DutiesImport.xlsx"), monthlyFilePublishDate);

			public (string Path, DateTime PublicationDate) DutiesExportFile
				=> (Path.Combine(BasePath, "HierarchyPosition\\DutiesExport.xlsx"), monthlyFilePublishDate);

			public (string Path, DateTime PublicationDate) MeasureExclusions
				=> (Path.Combine(BasePath, "HierarchyPosition\\MeasureExclusions.xlsx"), monthlyFilePublishDate);

			public (string Path, DateTime PublicationDate) MeasureConditions
				=> (Path.Combine(BasePath, "HierarchyPosition\\MeasureConditions.xlsx"), monthlyFilePublishDate);

			static string BasePath => TestHelper.BasePath;

			readonly DateTime monthlyFilePublishDate;
			readonly DateTime dailyFilePublishDate;
		}

		#endregion

		[Test]
		[TestCase("", true)]
		[TestCase("EN", true)]
		[TestCase("LV", false)]
		public void GenerateNomenclatureRecordsWhenDailyDeclarableRecordsDoNotMatchEnNomenclatureTest(string dailyUpdateLanguage, bool declarableRecordsShouldBeAddedFromDailyUpdate)
		{
			var rawNomenclatureRecords = new List<IRawNomenclatureRecord>
			{
				new RawNomenclatureRecord("0101000000 80", new DateTime(2022, 01, 17), new DateTime(2022, 02, 17), "EN", 0, 1, "English"),
				new RawNomenclatureRecord("0105119900 80", new DateTime(2021, 01, 17), new DateTime(2023, 01, 17), "EN", 1, 2, "English 2")
			};

			var rawDeclarableRecords = new List<IRawDeclarableCodeRecord>
			{
				new RawDeclarableCodeRecord("0101000000 80", new DateTime(2020, 01, 16), new DateTime(2020, 01, 17), "1", new DateTime(2022, 02, 17))
			};

			var dailyRecord = new RawDailyNomenclatureRecord("0102000000 80", new DateTime(2024, 01, 16), new DateTime(2025, 01, 16), dailyUpdateLanguage, "1", "-", "DESC", "UPDATE", new DateTime(2024, 01, 16), 1, "Nomenclature_20240116");

			var dailyProviderMock = new Mock<IDailyTariffUpdatesFileProvider>();
			dailyProviderMock.SetupGet(x => x.NomenclatureDailyRawRecordCollection).Returns(new IRawNomenclatureDailyRecord[] { dailyRecord });

			var nomenclatureProducer = new NomenclaturePlusDailyProducerTestHelper(dailyProviderMock.Object);

			var (_, declarableRecords) = nomenclatureProducer.FinalizeNomenclatureAndDeclarableFileParsingExposed(rawNomenclatureRecords, rawDeclarableRecords);

			var declarableFromDaily = declarableRecords.SingleOrDefault(x => x.TariffHeader == dailyRecord.TariffHeader);

			if (declarableRecordsShouldBeAddedFromDailyUpdate)
			{
				Assert.That(declarableFromDaily, Is.Not.Null, $"Declarable record added from daily update (Language={dailyUpdateLanguage})");
				Assert.Multiple(() =>
				{
					Assert.That(declarableFromDaily.DeclarableStartDate, Is.EqualTo(dailyRecord.StartDate));
					Assert.That(declarableFromDaily.EndDate, Is.EqualTo(dailyRecord.EndDate));
				});
			}
			else
			{
				Assert.That(declarableFromDaily, Is.Null, $"Declarable record not added from daily update (Language={dailyUpdateLanguage})");
			}
		}

		[Test]
		[TestCase("", true)]
		[TestCase("EN", false)]
		[TestCase("LV", false)]
		public void GenerateNomenclatureRecordsWhenDailyDeclarableRecordsMatchesEnNomenclatureTest(string dailyUpdateLanguage, bool expectedUpdatedFromDaily)
		{
			var originalEndDate = new DateTime(2023, 01, 17);
			var rawNomenclatureRecords = new List<IRawNomenclatureRecord>
			{
				new RawNomenclatureRecord("0101000000 80", new DateTime(2022, 01, 17), originalEndDate, "EN", 0, 1, "English"),
				new RawNomenclatureRecord("0105119900 80", new DateTime(2021, 01, 17), new DateTime(2023, 01, 17), "EN", 1, 2, "English 2")
			};

			var declarableRecord = new RawDeclarableCodeRecord("0101000000 80", new DateTime(2020, 01, 16), new DateTime(2020, 01, 16), "1", originalEndDate);
			var rawDeclarableRecords = new List<IRawDeclarableCodeRecord>
			{
				declarableRecord
			};

			var updatedEndDate = new DateTime(2024, 04, 17);
			var dailyRecord = new RawDailyNomenclatureRecord(declarableRecord.TariffHeader, declarableRecord.StartDate, updatedEndDate, dailyUpdateLanguage, "1", "-", "DESC", "UPDATE", new DateTime(2023, 01, 16), 1, "Nomenclature_20230116");

			var dailyProviderMock = new Mock<IDailyTariffUpdatesFileProvider>();
			dailyProviderMock.SetupGet(x => x.NomenclatureDailyRawRecordCollection).Returns(new IRawNomenclatureDailyRecord[] { dailyRecord });

			var nomenclatureProducer = new NomenclaturePlusDailyProducerTestHelper(dailyProviderMock.Object);

			var (_, declarableRecords) = nomenclatureProducer.FinalizeNomenclatureAndDeclarableFileParsingExposed(rawNomenclatureRecords, rawDeclarableRecords);

			var declarableFromDaily = declarableRecords.SingleOrDefault(x => x.TariffHeader == dailyRecord.TariffHeader);
			Assert.That(declarableFromDaily, Is.Not.Null, "Declarable record found");

			if (expectedUpdatedFromDaily)
			{
				Assert.That(declarableFromDaily.EndDate, Is.EqualTo(updatedEndDate), $"Declarable record is updated from daily record (Language={dailyUpdateLanguage})");
			}
			else
			{
				Assert.That(declarableFromDaily.EndDate, Is.EqualTo(originalEndDate), $"Declarable record is not updated from daily record (Language={dailyUpdateLanguage})");
			}
		}


		[Test]
		public void GenerateNomenclatureRecordsWhenDailyRecordHasInvalidHierarchyPosition()
		{
			var originalEndDate = new DateTime(2023, 01, 17);
			var rawNomenclatureRecords = new List<IRawNomenclatureRecord>
			{
				new RawNomenclatureRecord("0101000000 80", new DateTime(2022, 01, 17), originalEndDate, "EN", 2, 1, "English"),
				new RawNomenclatureRecord("0101000010 80", new DateTime(2022, 01, 17), originalEndDate, "EN", 4, 1, "English"),
				new RawNomenclatureRecord("0105119900 80", new DateTime(2021, 01, 17), new DateTime(2023, 01, 17), "EN", 1, 2, "English 2")
			};

			var rawDeclarableRecords = new List<IRawDeclarableCodeRecord>
			{
				new RawDeclarableCodeRecord("0101000000 80", new DateTime(2020, 01, 16), new DateTime(2020, 1, 16), "1", originalEndDate),
				new RawDeclarableCodeRecord("0101000010 80", new DateTime(2020, 01, 16), new DateTime(2020, 1, 16), "1", originalEndDate),
			};

			var updatedEndDate = new DateTime(2024, 04, 17);
			var dailyRecords = new List<IRawNomenclatureDailyRecord>
			{
				new RawDailyNomenclatureRecord("0101000010 80", new DateTime(2022, 01, 17), updatedEndDate, "EN", "1", "-", "DESC", "UPDATE", new DateTime(2023, 01, 16), 1, "Nomenclature_20230116"),
				new RawDailyNomenclatureRecord("0102000000 80", new DateTime(2022, 01, 17), updatedEndDate, "EN", "1", "-", "DESC", "UPDATE", new DateTime(2023, 01, 16), 1, "Nomenclature_20230116"),
				new RawDailyNomenclatureRecord("0102000010 80", new DateTime(2022, 01, 17), updatedEndDate, "EN", "0", "-", "DESC", "UPDATE", new DateTime(2023, 01, 16), 1, "Nomenclature_20230116")
			};

			var dailyProviderMock = new Mock<IDailyTariffUpdatesFileProvider>();
			dailyProviderMock.SetupGet(x => x.NomenclatureDailyRawRecordCollection).Returns(dailyRecords);

			var nomenclatureProducer = new NomenclaturePlusDailyProducerTestHelper(dailyProviderMock.Object);

			var (nomenclatures, _) = nomenclatureProducer.FinalizeNomenclatureAndDeclarableFileParsingExposed(rawNomenclatureRecords, rawDeclarableRecords);

			Assert.Multiple(() =>
			{
				Assert.That(nomenclatures.Single(x => x.TariffHeader == "0102000000 80").EndDate, Is.EqualTo(updatedEndDate), "Added from daily record with valid Hierarchy Position");
				Assert.That(nomenclatures.Count(x => x.TariffHeader == "0102000010 80"), Is.EqualTo(0), "Daily record with invalid Hierarchy Position is ignored");
			});
		}

		[SetUp]
		public void SetUp()
		{
			ApplicationConfig.ConfigEnvironment();
			CleanEnvironment();
		}

		[TearDown]
		public void TearDown()
		{
			CleanEnvironment();
		}

		void CleanEnvironment()
		{
			if (Directory.Exists(ApplicationConfig.DownloadsFolder))
			{
				Directory.Delete(ApplicationConfig.DownloadsFolder, true);
			}

			if (File.Exists(ApplicationConfig.NomenclatureUXmlFile))
			{
				File.Delete(ApplicationConfig.NomenclatureUXmlFile);
			}
		}

		string GetExpectedNomenclatureUXmlFile(string fileName, DateTime expectedPublicationDate)
		{
			return File.ReadAllText(Path.Combine(TestHelper.BasePath, fileName))
				.Replace("{publication_date}", expectedPublicationDate.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
		}

		class NomenclaturePlusDailyProducerTestHelper : NomenclaturePlusDailyProducer
		{
			public NomenclaturePlusDailyProducerTestHelper(IDailyTariffUpdatesFileProvider dailyTariffUpdatesFileProvider) : base(dailyTariffUpdatesFileProvider)
			{
			}

			public (List<IRawNomenclatureRecord> nomenclatures, List<IRawDeclarableCodeRecord> declarables) FinalizeNomenclatureAndDeclarableFileParsingExposed(List<IRawNomenclatureRecord> rawNomenclature, List<IRawDeclarableCodeRecord> rawDeclarableCodes)
			{
				return FinalizeNomenclatureAndDeclarableFileParsing(rawNomenclature, rawDeclarableCodes);
			}

			public ICompositeKeyTreeGenerator GetNewCompositeKeyTreeGeneratorExposed() => GetNewCompositeKeyTreeGenerator();
		}
	}
}
