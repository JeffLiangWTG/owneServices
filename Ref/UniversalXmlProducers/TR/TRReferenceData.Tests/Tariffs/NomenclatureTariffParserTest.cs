using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs
{
	[TestFixture]
	public class NomenclatureTariffParserTest
	{
		[Test]
		public void GetProcessedData()
		{
			parser.DataFileFolder = TempFolder;
			SimulateDownloadToTempFolder("05 fasil 2021.xls", "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.NomenclatureTariff.fasil.05 fasil 2021.xls");
			SimulateDownloadToTempFolder("10 fasıl 2022.xls", "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.NomenclatureTariff.fasil.10 fasıl 2022.xls");
			var processedData = parser.GetProcessedData();
			Assert.AreEqual(287, processedData.Count(), "Multi files.");

			Directory.Delete(TempFolder, true);
			Directory.CreateDirectory(TempFolder);
			SimulateDownloadToTempFolder("2023 Tariff Codes.xls", "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.NomenclatureTariff.fasil.2023 Tariff Codes.xls");
			processedData = parser.GetProcessedData();
			Assert.AreEqual(309, processedData.Count(), "Single file.");
		}

		[Test]
		public void GetTariffCode()
		{
			var tariffCode = NomenclatureTariffParser.GetTariffCode("0101.90.00.90.00");
			Assert.AreEqual("010190009000", tariffCode);

			tariffCode = NomenclatureTariffParser.GetTariffCode("9603.21");
			Assert.AreEqual("960321", tariffCode);
		}

		[Test]
		public void ParseDescription()
		{
			var parsedDescription = NomenclatureTariffParser.ParseDescription(" - - Kıymetli veya yarı kıymetli taş taklitleri (bir mesnete monteli olsun olmasın)");
			Assert.AreEqual(2, parsedDescription.DashCount);
			Assert.AreEqual("Kıymetli veya yarı kıymetli taş taklitleri (bir mesnete monteli olsun olmasın)", parsedDescription.Content);
			Assert.AreEqual(1, parsedDescription.Indent);
			Assert.AreEqual(5, parsedDescription.IndexOfFirstLetter);

			parsedDescription = NomenclatureTariffParser.ParseDescription(" - - - - - Yol süpürme fırçaları");
			Assert.AreEqual(5, parsedDescription.DashCount);
			Assert.AreEqual("Yol süpürme fırçaları", parsedDescription.Content);
			Assert.AreEqual(1, parsedDescription.Indent);
			Assert.AreEqual(11, parsedDescription.IndexOfFirstLetter);

			parsedDescription = NomenclatureTariffParser.ParseDescription("Akrilik polimerler (ilk şekillerde):");
			Assert.AreEqual(0, parsedDescription.DashCount);
			Assert.AreEqual("Akrilik polimerler (ilk şekillerde):", parsedDescription.Content);
			Assert.AreEqual(0, parsedDescription.Indent);
			Assert.AreEqual(0, parsedDescription.IndexOfFirstLetter);
		}

		[Test]
		public void GetTariffUOM()
		{
			var tariffUOM = NomenclatureTariffParser.GetTariffUOM("-");
			Assert.AreEqual("-", tariffUOM);

			tariffUOM = NomenclatureTariffParser.GetTariffUOM("Ct/I");
			Assert.AreEqual("CCT", tariffUOM);

			tariffUOM = NomenclatureTariffParser.GetTariffUOM("Kg/net eda");
			Assert.AreEqual("K58", tariffUOM);

			tariffUOM = NomenclatureTariffParser.GetTariffUOM("Kg-net eda");
			Assert.AreEqual("K58", tariffUOM);

			tariffUOM = NomenclatureTariffParser.GetTariffUOM("m³ (1)");
			Assert.AreEqual("MTQ", tariffUOM);
		}

		[Test]
		public void GetTariffRate()
		{
			var tariffRate = NomenclatureTariffParser.GetTariffRate("50");
			Assert.AreEqual("VFD * 0.50", tariffRate.formula);
			Assert.AreEqual("50%", tariffRate.derivedFrom);

			tariffRate = NomenclatureTariffParser.GetTariffRate("5");
			Assert.AreEqual("VFD * 0.05", tariffRate.formula);
			Assert.AreEqual("5%", tariffRate.derivedFrom);

			tariffRate = NomenclatureTariffParser.GetTariffRate("150");
			Assert.AreEqual("VFD * 1.50", tariffRate.formula);
			Assert.AreEqual("150%", tariffRate.derivedFrom);

			tariffRate = NomenclatureTariffParser.GetTariffRate("100");
			Assert.AreEqual("VFD * 1.00", tariffRate.formula);
			Assert.AreEqual("100%", tariffRate.derivedFrom);

			tariffRate = NomenclatureTariffParser.GetTariffRate("");
			Assert.AreEqual("", tariffRate.formula);
			Assert.AreEqual("", tariffRate.derivedFrom);
		}

		[Test]
		public void GetCompositeKey()
		{
			var record = new RawNomenclatureTariff
			{
				Code = "01.01",
				HSCodeInfo = new Nomenclature
				{
					Section = "01",
					Chapter = "01",
					Subchapter = null,
					Heading = "01",
					SubHeading = null,
					LowLevelSubHeading = null,
				}
			};
			var compositeKey = NomenclatureTariffParser.GetCompositeKey("", record, 0, 0);
			Assert.AreEqual("01.01..01", compositeKey);

			record = new RawNomenclatureTariff
			{
				Code = "",
				HSCodeInfo = new Nomenclature
				{
					Section = "07",
					Chapter = "39",
					Subchapter = "01",
					Heading = "01",
					SubHeading = "10",
					LowLevelSubHeading = null,
				}
			};
			compositeKey = NomenclatureTariffParser.GetCompositeKey("07.39.01.01.10.01.01", record, 2, 2);
			Assert.AreEqual("07.39.01.01.10.01.01.02.02", compositeKey);

			record = new RawNomenclatureTariff
			{
				Code = "3901.10.90.00.19",
				HSCodeInfo = new Nomenclature
				{
					Section = "07",
					Chapter = "39",
					Subchapter = "01",
					Heading = "01",
					SubHeading = "10",
					LowLevelSubHeading = "90.00.19",
				}
			};
			compositeKey = NomenclatureTariffParser.GetCompositeKey("07.39.01.01.10.01.01.02.02", record, 3, 3);
			Assert.AreEqual("07.39.01.01.10.01.01.02.02.03.03", compositeKey);
		}

		[Test]
		public void MergeData()
		{
			SimulateDownloadToTempFolder("05 fasil 2021.xls", "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.NomenclatureTariff.fasil.05 fasil 2021.xls");
			var chapter = NomenclatureTariffLoader.LoadData(DataFilePath, "05");
			var records = chapter.Records.ToList();
			Assert.AreEqual(86, records.Count);
			Assert.AreEqual("İnsan saçı (işlenmemiş, yıkanmış veya yağı alınmış olsun", records[0].Description);
			Assert.AreEqual("olmasın) ; insan saçı döküntüleri", records[1].Description);
			Assert.AreEqual("Evcil domuz veya yaban domuzu kılları; porsuk kılları", records[2].Description);
			Assert.AreEqual("veya fırça imali için diğer kıllar; bu kılların döküntüleri:", records[3].Description);

			parser.SetParsedDescriptions(chapter);
			parser.MergeData(chapter);
			records = chapter.Records.ToList();
			Assert.AreEqual(57, records.Count);
			Assert.AreEqual("İnsan saçı (işlenmemiş, yıkanmış veya yağı alınmış olsun olmasın) ; insan saçı döküntüleri", records[0].ParsedDescription.Content);
			Assert.AreEqual("Evcil domuz veya yaban domuzu kılları; porsuk kılları veya fırça imali için diğer kıllar; bu kılların döküntüleri:", records[1].ParsedDescription.Content);
		}

		[Test]
		public void GetProcessedDataForChapter()
		{
			SimulateDownloadToTempFolder("05 fasil 2021.xls", "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.NomenclatureTariff.fasil.05 fasil 2021.xls");
			var chapter = NomenclatureTariffLoader.LoadData(DataFilePath, "05");
			parser.SetParsedDescriptions(chapter);
			parser.MergeData(chapter);
			var processedData = parser.GetProcessedDataForChapter(chapter).ToList();
			Assert.AreEqual(57, processedData.Count);
			var target = processedData[37];
			Assert.AreEqual("051110000000", target.Code);
			Assert.AreEqual("01.05..11.01.01", target.CompositeKey);
			Assert.AreEqual("Sığır spermleri", target.Description);
			Assert.AreEqual(1, target.Level);
			Assert.AreEqual(1, target.LevelOrder);
			Assert.AreEqual("VFD * 0.30", target.Rate);
			Assert.AreEqual("30%", target.RateDerivedFrom);
			Assert.AreEqual("C62", target.UOM);

			target = processedData[38];
			Assert.AreEqual("", target.Code);
			Assert.AreEqual("01.05..11.01.02", target.CompositeKey);
			Assert.AreEqual("Diğerleri:", target.Description);
			Assert.AreEqual(1, target.Level);
			Assert.AreEqual(2, target.LevelOrder);
			Assert.AreEqual("", target.Rate);
			Assert.AreEqual("", target.RateDerivedFrom);
			Assert.AreEqual("", target.UOM);
		}

		[Test]
		public void GetRefCusNomenclatureGroup()
		{
			var processedNomenclatures = parser.ProcessedNomenclatureTariffs.Where(x => !x.IsTariff);
			var nomenclatureEntities = parser.GetNomenclatureEntities(processedNomenclatures);
			Assert.AreEqual(3, nomenclatureEntities.Count());

			var first = nomenclatureEntities.First();
			Assert.AreEqual("3901", first.ZZ5_Value);
			Assert.AreEqual("Etilen polimerleri (ilk şekillerde):", first.ZZ5_Description);
			Assert.AreEqual(Constants.HsnTariffStartDate, first.ZZ5_StartDate);
			Assert.AreEqual("07.39.01.01", first.ZZ5_CompositeKey);

			var last = nomenclatureEntities.Last();
			Assert.AreEqual("", last.ZZ5_Value);
			Assert.AreEqual("Diğerleri", last.ZZ5_Description);
			Assert.AreEqual(Constants.HsnTariffStartDate, last.ZZ5_StartDate);
			Assert.AreEqual("07.39.01.01.01.01.02.02", last.ZZ5_CompositeKey);
		}

		[Test]
		public void GetRefCusTariff()
		{
			var processedTariffs = parser.ProcessedNomenclatureTariffs.Where(x => x.IsTariff);
			var tariffEntities = parser.GetTariffEntities(processedTariffs);
			Assert.AreEqual(6, tariffEntities.Count());

			var first = tariffEntities.First();
			Assert.AreEqual("390110100000", first.ZZ1_TariffCode);
			Assert.AreEqual("Lineer polietilen", first.ZZ1_Description);
			Assert.AreEqual(Constants.HsnTariffStartDate, first.ZZ1_StartDate);
			Assert.AreEqual("07.39.01.01.01.01.02.01", first.ZZ1_CompositeKeyOnZZ5);

			var firstUOMs = first.RefCusTariffUOMs;
			Assert.AreEqual(2, firstUOMs.Length);
			Assert.AreEqual("CU1,CU2", string.Join(",", firstUOMs.Select(x => x.ZZ8_Type)));
			Assert.AreEqual("KGM,-", string.Join(",", firstUOMs.Select(x => x.ZZ8_UOM)));

			var firstRates = first.RefCusRates;
			Assert.AreEqual(1, firstRates.Length);
			var firstRate = firstRates[0];
			Assert.AreEqual(Constants.HsnTariffStartDate, firstRate.ZZ2_StartDate);
			Assert.AreEqual("VFD * 0.50", firstRate.ZZ2_RateFormula);
			Assert.AreEqual("50%", firstRate.ZZ2_RateFormulaDerivedFrom);

			var last = tariffEntities.Last();
			Assert.AreEqual("991900000012", last.ZZ1_TariffCode);
			Assert.AreEqual("Miras yoluyla intikal eden eşya", last.ZZ1_Description);
			Assert.AreEqual(Constants.HsnTariffStartDate, last.ZZ1_StartDate);
			Assert.AreEqual("21.99..19.01.02", last.ZZ1_CompositeKeyOnZZ5);

			var lastUOMs = last.RefCusTariffUOMs;
			Assert.AreEqual(1, lastUOMs.Length);
			var lastUOM = last.RefCusTariffUOMs.First();
			Assert.AreEqual("CU1", lastUOM.ZZ8_Type);
			Assert.AreEqual("KGM", lastUOM.ZZ8_UOM);

			Assert.IsNull(last.RefCusRates);
		}

		[Test]
		public void GetNomenclatureWriterConfiguration()
		{
			var config = parser.GetNomenclatureWriterConfiguration();
			Assert.That(config, Is.Not.Null);

			var refType = typeof(RefCusNomenclatureGroup);
			var entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig, Is.Not.Null);
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusNomenclatureGroup.ZZ5_ZZ9_NKNomenclatureGroupType))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusNomenclatureGroup.ZZ5_Value))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusNomenclatureGroup.ZZ5_Description))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusNomenclatureGroup.ZZ5_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusNomenclatureGroup.ZZ5_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusNomenclatureGroup.ZZ5_CompositeKey))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusNomenclatureGroup.ZZ5_ZZZ_NKDataGrouping))));
		}

		[Test]
		public void GetTariffWriterConfiguration()
		{
			var config = parser.GetTariffWriterConfiguration();
			Assert.That(config, Is.Not.Null);

			var refType = typeof(RefCusTariff);
			var entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_NKTariffType))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZI_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_TariffCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_Description))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.ZZ1_CompositeKeyOnZZ5))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusTariffUOMs))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariff.RefCusRates))));

			refType = typeof(RefCusTariffUOM);
			entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffUOM.ZZ8_Type))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffUOM.ZZ8_UOM))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTariffUOM.ZZ8_ZZZ_NKDataGrouping))));

			refType = typeof(RefCusRate);
			entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZY1_NKRateCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZY1_ZZR_NKRateType))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_RateFormula))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZZS_NKPreference))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZZS_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRate.ZZ2_RateFormulaDerivedFrom))));
		}

		void SimulateDownloadToTempFolder(string fileName, string resourceName)
		{
			DataFilePath = Path.Combine(TempFolder, fileName);
			TestHelper.SimulateDownload(DataFilePath, resourceName);
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			parser = new NomenclatureTariffParserForTest();
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		NomenclatureTariffParserForTest parser;
		string TempFolder;
		string DataFilePath;
	}

	public class NomenclatureTariffParserForTest : NomenclatureTariffParser
	{
		public new IOrderedEnumerable<ProcessedNomenclatureTariff> GetProcessedData() => base.GetProcessedData();

		public new void SetParsedDescriptions(Chapter chapter) => NomenclatureTariffParser.SetParsedDescriptions(chapter);

		public new void MergeData(Chapter chapter) => base.MergeData(chapter);

		public new IEnumerable<ProcessedNomenclatureTariff> GetProcessedDataForChapter(Chapter chapter) => NomenclatureTariffParser.GetProcessedDataForChapter(chapter);

		public new XmlWriterConfiguration GetNomenclatureWriterConfiguration() => NomenclatureTariffParser.GetNomenclatureWriterConfiguration();

		public new XmlWriterConfiguration GetTariffWriterConfiguration() => NomenclatureTariffParser.GetTariffWriterConfiguration();

		public new IEnumerable<RefCusNomenclatureGroup> GetNomenclatureEntities(IEnumerable<ProcessedNomenclatureTariff> nomenclatures) => NomenclatureTariffParser.GetNomenclatureEntities(nomenclatures);

		public new IEnumerable<RefCusTariff> GetTariffEntities(IEnumerable<ProcessedNomenclatureTariff> tariffs) => base.GetTariffEntities(tariffs);

		protected override string DataFileDirectory => string.IsNullOrEmpty(DataFileFolder) ? base.DataFileDirectory : DataFileFolder;

		public string DataFileFolder { get; set; }

		public IEnumerable<ProcessedNomenclatureTariff> ProcessedNomenclatureTariffs
		{
			get
			{
				if (processedNomenclatures == null)
				{
					processedNomenclatures = new ProcessedNomenclatureTariff[]
					{
						new ProcessedNomenclatureTariff { Code = "3901", Description = "Etilen polimerleri (ilk şekillerde):", CompositeKey = "07.39.01.01", Level = 0, LevelOrder = 1, UOM = "", Rate = "", RateDerivedFrom = "" },
						new ProcessedNomenclatureTariff { Code = "390110", Description = "Özgül kütlesi 0,94'ten az olan polietilen:", CompositeKey = "07.39.01.01.01.01", Level = 1, LevelOrder = 1, UOM = "", Rate = "", RateDerivedFrom = "" },
						new ProcessedNomenclatureTariff { Code = "390110100000", Description = "Lineer polietilen", CompositeKey = "07.39.01.01.01.01.02.01",  Level = 2, LevelOrder = 1, UOM = "-", Rate = "VFD * 0.50", RateDerivedFrom = "50%" },
						new ProcessedNomenclatureTariff { Code = "", Description = "Diğerleri", CompositeKey = "07.39.01.01.01.01.02.02", Level = 2, LevelOrder = 2,  UOM = "", Rate = "", RateDerivedFrom = "" },
						new ProcessedNomenclatureTariff { Code = "390110900011", Description = "Alçak yoğunluk polietilen", CompositeKey = "07.39.01.01.01.01.02.02.03.01", Level = 3, LevelOrder = 1,  UOM = "-", Rate = "VFD * 0.50", RateDerivedFrom = "50%" },
						new ProcessedNomenclatureTariff { Code = "390110900012", Description = "Polietilen kompaundları", CompositeKey = "07.39.01.01.01.01.02.02.03.02", Level = 3, LevelOrder = 2,  UOM = "-", Rate = "VFD * 0.50", RateDerivedFrom = "50%" },
						new ProcessedNomenclatureTariff { Code = "390110900019", Description = "Diğerleri", CompositeKey = "07.39.01.01.01.01.02.02.03.03", Level = 3, LevelOrder = 3,  UOM = "Adet", Rate = "VFD * 0.20", RateDerivedFrom = "20%" },
						new ProcessedNomenclatureTariff { Code = "991900000011", Description = "Evlilik nedeniyle serbest dolaşıma giren eşya", CompositeKey = "21.99..19.01.01", Level = 1, LevelOrder = 1,  UOM = "", Rate = "", RateDerivedFrom = "" },
						new ProcessedNomenclatureTariff { Code = "991900000012", Description = "Miras yoluyla intikal eden eşya", CompositeKey = "21.99..19.01.02", Level = 1, LevelOrder = 2,  UOM = "", Rate = "", RateDerivedFrom = "" },
					};
				}

				return processedNomenclatures;
			}
		}
		IEnumerable<ProcessedNomenclatureTariff> processedNomenclatures;
	}
}
