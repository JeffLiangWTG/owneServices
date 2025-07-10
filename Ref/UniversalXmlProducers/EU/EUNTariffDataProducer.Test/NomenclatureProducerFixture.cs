using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class NomenclatureProducerFixture
	{
		[Test]
		public void GenerateNomenclatureRecordsTest()
		{
			var nomenclatureProducer = new NomenclatureProducerTestHelper();

			var rawNomenclatures = new Dictionary<string, List<IRawNomenclatureRecord>>()
				{
					{ "1000", new List<IRawNomenclatureRecord>() {
						new RawNomenclatureRecord("1000",DateTime.MinValue,DateTime.MaxValue,"EN","1","1","English"),
						new RawNomenclatureRecord("1000",DateTime.MinValue,DateTime.MaxValue,"FR","1","1","French") }
					}
				};

			var rawDeclarableCodes = new Dictionary<string, IRawDeclarableCodeRecord>() { { "1000", new RawDeclarableCodeRecord("1000", DateTime.MinValue, new DateTime(2020, 1, 1), "1", new DateTime(2079, 12, 31, 23, 59, 00)) }, };

			var result = nomenclatureProducer.GenerateNomenclatureRecordsExposed(rawNomenclatures, rawDeclarableCodes);

			Assert.That(result, Is.Not.Null);
			Assert.That(result, Has.Count.EqualTo(1));
			var record = result.Single();
			Assert.Multiple(() =>
			{
				Assert.That(record.Language, Has.Count.EqualTo(1));
				Assert.That(record.IsTariff, Is.True);
				Assert.That(record.DeclarableStartDate, Is.EqualTo(new DateTime(2020, 1, 1)));
			});
		}

		[Test]
		public void GenerateNomenclatureRecordWhenDeclarableFileDoesNotHaveTheRecord()
		{
			var nomenclatureProducer = new NomenclatureProducerTestHelper();
			var rawNomenclatures = new Dictionary<string, List<IRawNomenclatureRecord>>()
				{
					{ "1000", new List<IRawNomenclatureRecord>() {
						new RawNomenclatureRecord("1000",DateTime.MinValue,DateTime.MaxValue,"EN","1","1","English"),
						new RawNomenclatureRecord("1000",DateTime.MinValue,DateTime.MaxValue,"FR","1","1","French") }
					}
				};

			var rawDeclarableCodes = new Dictionary<string, IRawDeclarableCodeRecord>() { { "1002", new RawDeclarableCodeRecord("1000", DateTime.MinValue, new DateTime(2020, 1, 1), "1", new DateTime(2079, 12, 31, 23, 59, 00)) }, };

			var result = nomenclatureProducer.GenerateNomenclatureRecordsExposed(rawNomenclatures, rawDeclarableCodes);

			Assert.That(result, Is.Not.Null);
			Assert.That(result, Has.Count.EqualTo(1));
			var record = result.Single();
			Assert.Multiple(() =>
			{
				Assert.That(record.IsTariff, Is.False);
				Assert.That(record.DeclarableStartDate, Is.EqualTo(DateTime.MinValue));
			});
		}

		[TestCase("Goods nomenclature EN.xlsx")]
		[TestCase("Nomenclature EN(0).xlsx")]
		[TestCase("nomenclature DE.xlsx")]
		[TestCase("nomenclature  DE .xlsx")]
		[TestCase(" NOMENCLATURE  de .xlsx")]
		[TestCase("nomenclature DE.xls")]
		public void NomenclaturePatternRegexTest(string value)
		{
			Assert.IsTrue(new NomenclatureProducerTestHelper().GetNomenclatureLinkRegex().IsMatch(value));
		}

		[TestCase("Goods nomenclature EN.xlsx", "Nomenclature EN.xlsx", true)]
		[TestCase("Nomenclature EN", "Nomenclature EN.xlsx", true)]
		[TestCase("Nomenclature EN.xls", "Nomenclature EN.xlsx", true)]
		[TestCase("nomenclature DE.xlsx", "Nomenclature EN.xlsx", false)]
		public void NomenclaturePatternRegexTest(string value1, string value2, bool result)
		{
			var regex = new NomenclatureProducerTestHelper().GetNomenclatureMatchingFilenameRegex();
			Assert.That(regex.Match(value1).Groups[1].Value == regex.Match(value2).Groups[1].Value, Is.EqualTo(result));
		}

		[Test]
		public void GenerateNomenclatureRecordsWhenDeclarableCodeRecordsDoNotMatchWithEnNomenclatureTest()
		{
			var rawNomenclatureDictionary = new Dictionary<string, List<IRawNomenclatureRecord>>
			{
				{ "0101000000 80", new[] { new RawNomenclatureRecord("0101000000 80", new DateTime(2022, 01, 17), new DateTime(2022, 02, 17), "EN", 0, 1, "English") }.Cast<IRawNomenclatureRecord>().ToList() },
				{ "0105119900 80", new[] { new RawNomenclatureRecord("0105119900 80", new DateTime(2021, 01, 17), new DateTime(2023, 01, 17), "EN", 1, 2, "English 2") }.Cast<IRawNomenclatureRecord>().ToList() },
			};

			var rawDeclarableCodeDictionary = new Dictionary<string, IRawDeclarableCodeRecord>
			{
				{ "0101000000 80", new RawDeclarableCodeRecord("0101000000 80", new DateTime(2020, 01, 16), new DateTime(2020, 01, 17), "1", new DateTime(2022, 02, 17)) }
			};

			var nomenclatureProducer = new NomenclatureProducerTestHelper();

			var nomenclatureRecords = nomenclatureProducer.GenerateNomenclatureRecordsExposed(rawNomenclatureDictionary, rawDeclarableCodeDictionary);

			Assert.That(nomenclatureRecords, Has.Count.EqualTo(2));
			AssertNomenclature("0101000000 80", new DateTime(2020, 01, 17), new DateTime(2022, 02, 17), 0, 1, "English", true);
			AssertNomenclature("0105119900 80", new DateTime(2021, 01, 17), new DateTime(2023, 01, 17), 1, 2, "English 2", false);

			void AssertNomenclature(string tariffHeader, DateTime expectedDeclarableStartDate, DateTime expectedEndDate, int expectedHierarchyPosition, int expectedLevel, string expectedDescription, bool expectedIsLeaf)
			{
				var nomenclature = nomenclatureRecords.Single(x => x.TariffHeader == tariffHeader);
				Assert.Multiple(() =>
				{
					Assert.That(nomenclature.DeclarableStartDate, Is.EqualTo(expectedDeclarableStartDate));
					Assert.That(nomenclature.EndDate, Is.EqualTo(expectedEndDate));
					Assert.That(nomenclature.HierarchyPosition, Is.EqualTo(expectedHierarchyPosition));
					Assert.That(nomenclature.Level, Is.EqualTo(expectedLevel));
					Assert.That(nomenclature.Description, Is.EqualTo(expectedDescription));
					Assert.That(nomenclature.IsTariff, Is.EqualTo(expectedIsLeaf));
				});
			}
		}

		[Test]
		public void TestGetNewCompositeKeyTreeGenerator()
		{
			var nomenclatureProducer = new NomenclatureProducerTestHelper();

			Assert.That(nomenclatureProducer.GetNewCompositeKeyTreeGeneratorExposed(), Is.TypeOf<CompositeKeyTreeGenerator>());
		}

		[Test]
		public void TestRunWithProduceXML()
		{
			AssertProduceXml(false);
			AssertProduceXml(true);
		}

		void AssertProduceXml(bool shouldProduce)
		{
			var parsedData = new CompositeKeyGeneratorResult();
			var nomenclatureGroup = new RefCusNomenclatureGroup
			{
				ZZ5_Value = "Valu",
				ZZ5_CompositeKey = "CompositeKey",
				ZZ5_StartDate = DateTime.Today,
				ZZ5_EndDate = DateTime.MaxValue,
				ZZ5_Description = "Description",
			};
			parsedData.NomenclatureGroups.Add(nomenclatureGroup);

			var nomenclatureProducer = new Mock<NomenclatureProducer>();
			var webfileInfoList = new[] { new WebFileInfo("Nomenclature EN.xlsx", "NotImportant", DateTime.MinValue) };
			nomenclatureProducer.Setup(x => x.LocateWebFiles()).Returns(webfileInfoList);
			nomenclatureProducer.Setup(x => x.DownloadFiles(webfileInfoList)).Returns(true);
			nomenclatureProducer.Setup(x => x.ParseNomenclatureData(webfileInfoList)).Returns(parsedData);
			nomenclatureProducer.Object.Run(shouldProduce);

			var filesInOutputFolder = Directory.GetFiles(OutputFolder);
			var outputFileExists = File.Exists(Path.Combine(OutputFolder, "EUNNomenclature.xml"));
			Assert.That(outputFileExists, Is.EqualTo(shouldProduce));
		}

		[OneTimeSetUp]
		public void Setup()
		{
			OutputFolder = Path.Combine(Path.GetTempPath(), "UXmlFiles");
			if (Directory.Exists(OutputFolder))
			{
				Directory.Delete(OutputFolder, true);
			}
			Directory.CreateDirectory(OutputFolder);

			ApplicationConfig.ConfigEnvironment();
			ApplicationConfig.SetDownloadsFolder(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\"));
			ApplicationConfig.SetNomenclatureUXmlFile(Path.Combine(OutputFolder, "EUNNomenclature.xml"));

		}

		[OneTimeTearDown]
		public void TearDown()
		{
			Directory.Delete(OutputFolder, true);
		}

		string OutputFolder;
	}

	class NomenclatureProducerTestHelper : NomenclatureProducer
	{
		public Regex GetNomenclatureLinkRegex()
		{
			return new Regex(nomenclatureLinkRegex, RegexOptions.IgnoreCase);
		}

		public Regex GetNomenclatureMatchingFilenameRegex()
		{
			return new Regex(nomenclatureMatchingFilenameRegex, RegexOptions.IgnoreCase);
		}

		public List<INomenclatureRecord> GenerateNomenclatureRecordsExposed(Dictionary<string, List<IRawNomenclatureRecord>> rawNomenclature, Dictionary<string, IRawDeclarableCodeRecord> rawDeclarableCodes)
		{
			return GenerateNomenclatureRecords(rawNomenclature, rawDeclarableCodes);
		}

		public ICompositeKeyTreeGenerator GetNewCompositeKeyTreeGeneratorExposed() => GetNewCompositeKeyTreeGenerator();
	}
}
