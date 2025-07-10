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

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	[TestFixture]
	class TRCustomsCodeListParserTest
	{
		[Test]
		public void GenerateCodeListDataXML()
		{
			string outputFilePath = Path.Combine(OutputFolder, Constants.XmlFileNames.TariffAdditionalCodeList);
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.CodeLists.TestFiles.Output.TRTariffAdditionalCodeLists.xml");
			ListParser.GenerateUniversalReferenceData(OutputFolder, Constants.RefCusCodeType.TariffAdditionalCodeListCode);
			Assert.That(expectedXML, Is.EqualTo(File.ReadAllText(outputFilePath)));

			outputFilePath = Path.Combine(OutputFolder, Constants.XmlFileNames.WarehouseCodes);
			expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.CodeLists.TestFiles.Output.TRWarehouseCodes.xml");
			ListParser.GenerateUniversalReferenceData(OutputFolder, Constants.RefCusCodeType.WarehouseCodes);
			Assert.That(expectedXML, Is.EqualTo(File.ReadAllText(outputFilePath)));

			outputFilePath = Path.Combine(OutputFolder, Constants.XmlFileNames.SupportingDocumentsCodes);
			expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.CodeLists.TestFiles.Output.TRSupportingDocumentsCodes.xml");
			ListParser.GenerateUniversalReferenceData(OutputFolder, Constants.RefCusCodeType.SupportingDocumentsCodes);
			Assert.That(expectedXML, Is.EqualTo(File.ReadAllText(outputFilePath)));

			outputFilePath = Path.Combine(OutputFolder, Constants.XmlFileNames.ExportUnionCountryCodes);
			expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.CodeLists.TestFiles.Output.TRExportUnionCountryCodes.xml");
			ListParser.GenerateUniversalReferenceData(OutputFolder, Constants.RefCusCodeType.ExportUnionCountryCodes);
			Assert.That(expectedXML, Is.EqualTo(File.ReadAllText(outputFilePath)));
		}

		[Test]
		public void GetRefCusCodeListsForTRTariffAdditionalCodeLists()
		{
			var codeLists = ListParser.GetRefCusCodeLists(TariffAdditionalCodesData, Constants.RefCusCodeType.TariffAdditionalCodeListCode);
			Assert.That(codeLists.Count, Is.EqualTo(99));

			var ban8 = codeLists.First(x => x.ZZD_Code == "BAN8");
			Assert.That(ban8.ZZD_Code, Is.EqualTo("BAN8"));
			Assert.That(ban8.ZZD_Description, Is.EqualTo("TRT Bandrol ücreti(KDV Matrahı (ÖTV Hariç) üzerinden % 8)"));
			Assert.That(ban8.ZZD_StartDate, Is.EqualTo(Constants.MinimumDateTime));
			Assert.That(ban8.ZZD_EndDate, Is.EqualTo(Constants.MaximumSmallDateTime));

			var test71 = codeLists.First(x => x.ZZD_Code == "7.1");
			Assert.That(test71.ZZD_Description, Is.EqualTo("(1) \"Tohumluk İthalatı Uygulama Genelgesi\" kapsamında Tarım ve Orman Bakanlığınca düzenlenen \"ithalat ön izin yazısı\"nın ilgili gümrük idaresine ibraz edilmesi halinde söz konusu gümrük vergisi %15 (Bosna-Hersek, Kosova, Venezuela ve Singapur Cumhuriyeti hariç), ithalat ön izin yazısında ithalat amacının \"çoğaltım\" olduğu belirtilirse %1 olarak uygulanır (Bosna-Hersek, Kosova ve Venezuela hariç)."));
			Assert.That(test71.ZZD_Code, Is.EqualTo("7.1"));
			Assert.That(test71.ZZD_StartDate, Is.EqualTo(new DateTime(2023, 01, 01, 00, 00, 00)));
			Assert.That(test71.ZZD_EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 00)));

			var test85174 = codeLists.First(x => x.ZZD_Code == "8517.4");
			Assert.That(test85174.ZZD_Description, Is.EqualTo("Diğerleri Yalnız Alçak güçlü (100 miliwatt`dan küçük) mobil telsiz telefon cihazları"));
			Assert.That(test85174.ZZD_Code, Is.EqualTo("8517.4"));
			Assert.That(test85174.ZZD_StartDate, Is.EqualTo(new DateTime(2023, 01, 01, 00, 00, 00)));
			Assert.That(test85174.ZZD_EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 00)));

			var test4103 = codeLists.First(x => x.ZZD_Code == "4103");
			Assert.That(test4103.ZZD_Description, Is.EqualTo("[Yalnız develerin (tek hörgüçlü dahil) ham derileri]"));
			Assert.That(test4103.ZZD_Code, Is.EqualTo("4103"));
			Assert.That(test4103.ZZD_StartDate, Is.EqualTo(new DateTime(2023, 01, 01, 00, 00, 00)));
			Assert.That(test4103.ZZD_EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 00)));

			var test4L14 = codeLists.First(x => x.ZZD_Code == "4L.14");
			Assert.That(test4L14.ZZD_Description, Is.EqualTo("(14) Singapur Cumhuriyeti için yalnızca şişelenmiş tavuk özünde gümrük vergisi ve ek mali yükümlülük %0 olarak uygulanır."));
			Assert.That(test4L14.ZZD_Code, Is.EqualTo("4L.14"));
			Assert.That(test4L14.ZZD_StartDate, Is.EqualTo(new DateTime(2023, 01, 01, 00, 00, 00)));
			Assert.That(test4L14.ZZD_EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 00)));

			var test3L9 = codeLists.First(x => x.ZZD_Code == "3L.9");
			Assert.That(test3L9.ZZD_Description, Is.EqualTo("(9) Güney Kore için yalnız nudıllar için ek mali yükümlülük sıfır olarak uygulanır."));
			Assert.That(test3L9.ZZD_Code, Is.EqualTo("3L.9"));
			Assert.That(test3L9.ZZD_StartDate, Is.EqualTo(new DateTime(2023, 01, 01, 00, 00, 00)));
			Assert.That(test3L9.ZZD_EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 00)));

			var test271119 = codeLists.First(x => x.ZZD_Code == "2711.19");
			Assert.That(test271119.ZZD_Description, Is.EqualTo("Sıvılaştırılmış petrol gazı (L.P.G) Diğerleri"));
			Assert.That(test271119.ZZD_Code, Is.EqualTo("2711.19"));
			Assert.That(test271119.ZZD_StartDate, Is.EqualTo(new DateTime(2023, 01, 01, 00, 00, 00)));
			Assert.That(test271119.ZZD_EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 00)));

			var test89031 = codeLists.First(x => x.ZZD_Code == "8903.1");
			Assert.That(test89031.ZZD_Description, Is.EqualTo("- Yatlar, kotralar, tekneler ve gezinti gemileri"));
			Assert.That(test89031.ZZD_Code, Is.EqualTo("8903.1"));
			Assert.That(test89031.ZZD_StartDate, Is.EqualTo(new DateTime(2023, 01, 01, 00, 00, 00)));
			Assert.That(test89031.ZZD_EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 00)));
		}

		[Test]
		public void TRCustomsRefCusCodesZZD_DescriptionReplaceAndTrimming()
		{
			var codeList = new TRCustomsRefCusCodes
			{
				Code = "8703.1.1",
				Description = " -Yük taşımasında kullanılıp azami ağırlığı 3,5 tonu aşmayan ve yolcu taşıma kapasitesi (Yolcu taşıma kapasitesi sürücü dâhil toplam yolcu sayısının 70 kilogramla çarpılması suretiyle hesaplanır. Bu hesaplamada koltuk olmasa dahi, koltuk montajı için bulunan sabit tertibatlar da koltuk olarak dikkate alınır) istiap haddinin (bir aracın güvenle taşıyabileceği sürücü ve yolcu dâhil toplam yük ağırlığı) % 50'sinin altında olan motorlu araçlardan (bütün tekerlekleri motordan güç alan veya alabilenler, binek otomobilleri, steyşın vagonlar, yarış arabaları, arazi taşıtları hariç)\r\n\r\n-- İstiap haddi 850 kilogramı geçmeyip motor silindir hacmi 2000 cm³'ün altında olanlar"
			};

			var refCusCodeLists = ListParser.GetRefCusCodeLists(new List<TRCustomsRefCusCodes> { codeList }, Constants.RefCusCodeType.TariffAdditionalCodeListCode);
			var result = refCusCodeLists.FirstOrDefault();

			Assert.NotNull(result);
			Assert.That(result.ZZD_Description, Is.EqualTo("-Yük taşımasında kullanılıp azami ağırlığı 3,5 tonu aşmayan ve yolcu taşıma kapasitesi (Yolcu taşıma kapasitesi sürücü dâhil toplam yolcu sayısının 70 kilogramla çarpılması suretiyle hesaplanır. Bu hesaplamada koltuk olmasa dahi, koltuk montajı için bulunan sabit tertibatlar da koltuk olarak dikkate alınır) istiap haddinin (bir aracın güvenle taşıyabileceği sürücü ve yolcu dâhil toplam yük ağırlığı) % 50'sinin altında olan motorlu araçlardan (bütün tekerlekleri motordan güç alan veya alabilenler, binek otomobilleri, steyşın vagonlar, yarış arabaları, arazi taşıtları hariç)-- İstiap haddi 850 kilogramı geçmeyip motor silindir hacmi 2000 cm³'ün altında olanlar"));
		}

		[Test]
		public void GetRefCusCodeListsForTRWarehouseCodes()
		{
			var codeLists = ListParser.GetRefCusCodeLists(WarehouseCodesData, Constants.RefCusCodeType.WarehouseCodes);
			Assert.That(codeLists.Count, Is.EqualTo(1681));

			var a0002 = codeLists.First(x => x.ZZD_Code == "A0002");
			Assert.That(a0002.ZZD_Code, Is.EqualTo("A0002"));
			Assert.That(a0002.ZZD_Description, Is.EqualTo("EKOL ULUSŞLARARASI TİC. A.Ş."));
			Assert.That(a0002.ZZD_StartDate, Is.EqualTo(Constants.MinimumDateTime));
			Assert.That(a0002.ZZD_EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 00, 00, 00)));

			var a0004 = codeLists.First(x => x.ZZD_Code == "A0004");
			Assert.That(a0004.ZZD_Code, Is.EqualTo("A0004"));
			Assert.That(a0004.ZZD_Description, Is.EqualTo("İNT.İNTERNAS NAK.TURİZM A.Ş."));
			Assert.That(a0004.ZZD_StartDate, Is.EqualTo(Constants.MinimumDateTime));
			Assert.That(a0004.ZZD_EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 00, 00, 00)));
		}

		[Test]
		public void GetRefCusCodeListsForTRSupportingDocuments()
		{
			var codeLists = ListParser.GetRefCusCodeLists(SupportingDocumentsCodesData, Constants.RefCusCodeType.SupportingDocumentsCodes);
			Assert.That(codeLists.Count, Is.EqualTo(634));

			var c0100 = codeLists.First(x => x.ZZD_Code == "0100");
			Assert.That(c0100.ZZD_Code, Is.EqualTo("0100"));
			Assert.That(c0100.ZZD_ZZK_NKCodeType, Is.EqualTo("DC44E"));
			Assert.That(c0100.ZZD_Description, Is.EqualTo("Fatura"));
			Assert.That(c0100.ZZD_StartDate, Is.EqualTo(Constants.TariffStartDate));
			Assert.That(c0100.ZZD_EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 00)));

			var c0101 = codeLists.First(x => x.ZZD_Code == "0101");
			Assert.That(c0101.ZZD_Code, Is.EqualTo("0101"));
			Assert.That(c0101.ZZD_ZZK_NKCodeType, Is.EqualTo("DC44I"));
			Assert.That(c0101.ZZD_Description, Is.EqualTo("Navlun Makbuzu"));
			Assert.That(c0101.ZZD_StartDate, Is.EqualTo(Constants.TariffStartDate));
			Assert.That(c0101.ZZD_EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 00)));
		}

		[Test]
		public void GetRefCusCodeListsForTRExportUnionCountryCodes()
		{
			var codeLists = ListParser.GetRefCusCodeLists(ExportUnionCountryCodesData, Constants.RefCusCodeType.ExportUnionCountryCodes);
			Assert.That(codeLists.Count, Is.EqualTo(524));

			var f01 = codeLists.First(x => x.ZZD_Code == "F01");
			Assert.That(f01.ZZD_Code, Is.EqualTo("F01"));
			Assert.That(f01.ZZD_Description, Is.EqualTo("ANTALYA SERBEST BÖL."));
			Assert.That(f01.ZZD_StartDate, Is.EqualTo(Constants.ExportUnionStartDate));
			Assert.That(f01.ZZD_EndDate, Is.EqualTo(Constants.MaximumSmallDateTime));

			var test329A = codeLists.First(x => x.ZZD_Code == "329A");
			Assert.That(test329A.ZZD_Code, Is.EqualTo("329A"));
			Assert.That(test329A.ZZD_Description, Is.EqualTo("ASCENSION ADASI"));
			Assert.That(test329A.ZZD_StartDate, Is.EqualTo(Constants.ExportUnionStartDate));
			Assert.That(test329A.ZZD_EndDate, Is.EqualTo(Constants.MaximumSmallDateTime));
		}

		[Test]
		public void GeneratedErrorMessages()
		{
			var parser = new CodeListParserForExceptionTest();

			parser.GenerateUniversalReferenceData(string.Empty, Constants.RefCusCodeType.TariffAdditionalCodeListCode);
			Assert.That(parser.ErrorMessage, Does.Contain("GetCodeList failure"));
			Assert.That(parser.ErrorMessage, Does.Contain("GenerateTariffUniversalReferenceData failure"));

			parser.GenerateUniversalReferenceData(string.Empty, Constants.RefCusCodeType.WarehouseCodes);
			Assert.That(parser.ErrorMessage, Does.Contain("GetCodeList failure"));
			Assert.That(parser.ErrorMessage, Does.Contain("GenerateTariffUniversalReferenceData failure"));

			parser.GenerateUniversalReferenceData(string.Empty, Constants.RefCusCodeType.SupportingDocumentsCodes);
			Assert.That(parser.ErrorMessage, Does.Contain("GetCodeList failure"));
			Assert.That(parser.ErrorMessage, Does.Contain("GenerateTariffUniversalReferenceData failure"));

			parser.GenerateUniversalReferenceData(string.Empty, Constants.RefCusCodeType.ExportUnionCountryCodes);
			Assert.That(parser.ErrorMessage, Does.Contain("GetCodeList failure"));
			Assert.That(parser.ErrorMessage, Does.Contain("GenerateTariffUniversalReferenceData failure"));
		}

		[Test]
		[TestCase("TariffAdditionalCodeListCode")]
		[TestCase("WarehouseCodes")]
		[TestCase("SupportingDocumentsCodes")]
		[TestCase("ExportUnionCountryCodes")]
		public void GetXmlWriterConfigurationTest(string codeTypeName)
		{
			var config = ListParser.GetXmlWriterConfiguration(codeTypeName, codeTypeName == "SupportingDocumentsCodes");
			Assert.That(config, Is.Not.Null, "Config should not be null.");

			var refType = typeof(RefCusCodeList);
			var entityConfig = config.GetConfiguration(refType);
			Assert.That(entityConfig, Is.Not.Null, "Entity configuration should not be null.");

			Assert.IsTrue(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_Code))),
				"ZZD_Code property should be included in the configuration.");
			Assert.IsTrue(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_Description))),
				"ZZD_Description property should be included in the configuration.");
			Assert.IsTrue(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_ZZK_NKCodeType))),
				"ZZD_ZZK_NKCodeType property should be included in the configuration.");
			Assert.IsTrue(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_ZZZ_NKDataGrouping))),
				"ZZD_ZZZ_NKDataGrouping property should be included in the configuration.");
			Assert.IsTrue(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_StartDate))),
				"ZZD_StartDate property should be included in the configuration.");
			Assert.IsTrue(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_EndDate))),
				"ZZD_EndDate property should be included in the configuration.");
		}

		[SetUp]
		public void Setup()
		{
			OutputFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(OutputFolder);
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			string DataFilePath;
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
			ListParser = new CodeListParserForTest();

			DataFilePath = Path.Combine(TempFolder, Constants.ExcelFileNames.TariffAdditionalCodeList);
			TestHelper.SimulateDownload(DataFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.CodeLists.TestFiles.TariffAdditionalCodesList.xlsx");
			TariffAdditionalCodesData = TRCustomsCodeListLoader.LoadData(DataFilePath, TRCodeListGenerateHelper.GetRefCusCodeListExcelConfig()[Constants.RefCusCodeType.TariffAdditionalCodeListCode]);

			DataFilePath = Path.Combine(TempFolder, Constants.ExcelFileNames.WarehouseCodes);
			TestHelper.SimulateDownload(DataFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.CodeLists.TestFiles.TRWarehouseCodes.xlsx");
			WarehouseCodesData = TRCustomsCodeListLoader.LoadData(DataFilePath, TRCodeListGenerateHelper.GetRefCusCodeListExcelConfig()[Constants.RefCusCodeType.WarehouseCodes]);

			DataFilePath = Path.Combine(TempFolder, Constants.ExcelFileNames.SupportingDocumentsCodes);
			TestHelper.SimulateDownload(DataFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.CodeLists.TestFiles.TRSupportingDocumentsCodes.xlsx");
			SupportingDocumentsCodesData = TRCustomsCodeListLoader.LoadData(DataFilePath, TRCodeListGenerateHelper.GetRefCusCodeListExcelConfig()[Constants.RefCusCodeType.SupportingDocumentsCodes]);

			DataFilePath = Path.Combine(TempFolder, Constants.ExcelFileNames.ExportUnionCountryCodes);
			TestHelper.SimulateDownload(DataFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.CodeLists.TestFiles.ExportUnionCountryCodes.xlsx");
			ExportUnionCountryCodesData = TRCustomsCodeListLoader.LoadData(DataFilePath, TRCodeListGenerateHelper.GetRefCusCodeListExcelConfig()[Constants.RefCusCodeType.ExportUnionCountryCodes]);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string OutputFolder;
		string TempFolder;
		CodeListParserForTest ListParser;
		IEnumerable<TRCustomsRefCusCodes> TariffAdditionalCodesData;
		IEnumerable<TRCustomsRefCusCodes> WarehouseCodesData;
		IEnumerable<TRCustomsRefCusCodes> SupportingDocumentsCodesData;
		IEnumerable<TRCustomsRefCusCodes> ExportUnionCountryCodesData;
	}

	public class CodeListParserForTest : TRCustomsCodeListParser
	{
		public new XmlWriterConfiguration GetXmlWriterConfiguration(string codeType, bool specifiedCodeTypeInExcelDocument) => TRCustomsCodeListParser.GetXmlWriterConfiguration(codeType, codeType == Constants.RefCusCodeType.SupportingDocumentsCodes);
		public new IEnumerable<RefCusCodeList> GetRefCusCodeLists(IEnumerable<TRCustomsRefCusCodes> data, string codeType) => base.GetRefCusCodeLists(data, codeType);
		protected override DateTime PublicationDateTime => new DateTime(2021, 01, 06, 15, 27, 30);
	}

	public class CodeListParserForExceptionTest : TRCustomsCodeListParser
	{
		protected override IEnumerable<RefCusCodeList> GetRefCusCodeLists(IEnumerable<TRCustomsRefCusCodes> codeLists, string codeType)
		{
			throw new InvalidDataException("Invalid data.");
		}
	}
}
