using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using CargoWise.RefDbRepo.EUReferenceData.CustomsMeursing.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.CustomsMeursing.Tests
{
	[TestFixture]
	class MeursingDataParserTests
	{
		[Test]
		public void TestParse()
		{
			var meursingData = ExcelParser.ReadXlsFileIntoResults(excelFileNameAndPath);
			Assert.AreEqual(16931, meursingData.Count);

			var firstRecord = meursingData[0];
			Assert.AreEqual(null, firstRecord.GoodsNomItemID);
			Assert.AreEqual("7000", firstRecord.TariffCode);
			Assert.AreEqual(null, firstRecord.OrdNumb);
			Assert.AreEqual("2002-01-01 00:00", firstRecord.DatStart);
			Assert.AreEqual("", firstRecord.DatEnd);
			Assert.AreEqual("1", firstRecord.RedInd);
			Assert.AreEqual("European Economic Area", firstRecord.Description);
			Assert.AreEqual("Amount of additional duty on sugar", firstRecord.Description2);
			Assert.AreEqual("Decision 0140/01", firstRecord.Decode);
			Assert.AreEqual("0.000 EUR DTN ", firstRecord.DutyCondFull);
			Assert.AreEqual("2012", firstRecord.GeogrAreaID);
			Assert.AreEqual("672", firstRecord.MeasTypID);

			var lastRecord = meursingData.Last();
			Assert.AreEqual(null, lastRecord.GoodsNomItemID);
			Assert.AreEqual("7996", lastRecord.TariffCode);
			Assert.AreEqual(null, lastRecord.OrdNumb);
			Assert.AreEqual("2017-01-01 00:00", lastRecord.DatStart);
			Assert.AreEqual("2017-12-31 00:00", lastRecord.DatEnd);
			Assert.AreEqual("1", lastRecord.RedInd);
			Assert.AreEqual("Peru", lastRecord.Description);
			Assert.AreEqual("Agricultural component", lastRecord.Description2);
			Assert.AreEqual("Decision 0735/12", lastRecord.Decode);
			Assert.AreEqual("18.200 EUR DTN ", lastRecord.DutyCondFull);
			Assert.AreEqual("PE", lastRecord.GeogrAreaID);
			Assert.AreEqual("674", lastRecord.MeasTypID);
		}

		[Test]
		public void TestGeogrpahicalFileParse()
		{
			var geographical = ExcelParser.ReadGeographicalXlsFileIntoResults(geoDataExcelFilenameAndPath);
			Assert.AreEqual(1921, geographical.Count);

			var firstRecord = geographical[0];
			Assert.AreEqual("1005", firstRecord.CountryGroup);
			Assert.AreEqual("SURV", firstRecord.GroupAbbreviation);
			Assert.AreEqual("AD", firstRecord.MemberCountry);

			var lastRecord = geographical.Last();
			Assert.AreEqual("5002", lastRecord.CountryGroup);
			Assert.AreEqual("DEF_SAVG", lastRecord.GroupAbbreviation);
			Assert.AreEqual("XS", lastRecord.MemberCountry);
		}

		[Test]
		public void TestExportXmlForEU()
		{
			var publicationTime = DateTime.Parse("2017-02-06T00:00:00", CultureInfo.InvariantCulture);
			var fileName = Path.Combine(binPath, "MeursingData.xml");

			//	7000		01/01/2002 		1	European Economic Area	Amount of additional duty on sugar	Decision 0140/01	0.000 EUR DTN	2012	672
			//7000        01 / 01 / 2004      1   Egypt Amount of additional duty on sugar Regulation 2276 / 03  0.000 EUR DTN   EG  672

			var meursingDatas = new List<CustomsMeursingData>() {
				new CustomsMeursingData() {
					TariffCode = "7000",
					DatStart = "2002-01-01 00:00",
					DatEnd = "2079-06-06 23:59",
					RedInd = "1",
					Description = "European Economic Area",
					Description2 = "Amount of additional duty on sugar",
					MeasTypID = "672",
					GeogrAreaID = "2012",
					DutyCondFull = "9.750 EUR DTN"
				},
				new CustomsMeursingData() {
					TariffCode = "7000",
					DatStart = "2002-01-01 00:00",
					DatEnd = "2079-06-06 23:59",
					RedInd = "1",
					Description = "Germany",
					Description2 = "Amount of additional duty on sugar",
					MeasTypID = "672",
					GeogrAreaID = "DE",
					DutyCondFull = "7.750 EUR DTN"
				}
			};
			var geoData = ExcelParser.ReadGeographicalXlsFileIntoResults(geoDataExcelFilenameAndPath);

			XMLGeneration.ExportToXMLFile(publicationTime, meursingDatas, geoData, fileName, null);
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(fileName);

			var tariffNodes = xmlDoc.SelectNodes("UniversalReferenceData/RefCusTariff");
			var firstTariffNode = tariffNodes[0];
			var firstTariffRateNodes = firstTariffNode.SelectNodes("RefCusRate");
			Assert.AreEqual("2002-01-01 00:00", firstTariffRateNodes[0]["ZZ2_StartDate"].InnerText);
			Assert.AreEqual("2079-06-06 23:59", firstTariffRateNodes[0]["ZZ2_EndDate"].InnerText);
			Assert.AreEqual("ADSZR", firstTariffRateNodes[0]["ZZ2_ZY1_NKRateCode"].InnerText);
			Assert.AreEqual("DUT", firstTariffRateNodes[0]["ZZ2_ZY1_ZZR_NKRateType"].InnerText);
			Assert.AreEqual("EUN", firstTariffRateNodes[0]["ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"].InnerText);
			Assert.AreEqual("9.750 * [DTN]", firstTariffRateNodes[0]["ZZ2_RateFormula"].InnerText);
			Assert.AreEqual("EUN", firstTariffRateNodes[0]["ZZ2_ZZZ_NKDataGrouping"].InnerText);
			Assert.AreEqual("2002-01-01 00:00", firstTariffRateNodes[0]["RefCusApplicability"]["ZZT_StartDate"].InnerText);
			Assert.AreEqual("2079-06-06 23:59", firstTariffRateNodes[0]["RefCusApplicability"]["ZZT_EndDate"].InnerText);
			Assert.AreEqual("2012", firstTariffRateNodes[0]["RefCusApplicability"]["ZZT_ZZA_NKTradeGroup"].InnerText);
			Assert.AreEqual("EUN", firstTariffRateNodes[0]["RefCusApplicability"]["ZZT_ZZA_ZZZ_NKDataGrouping"].InnerText);
			Assert.AreEqual("1", firstTariffRateNodes[0]["RefCusApplicability"]["ZZT_AdditionalCode"].InnerText);

			var firstExcludedTradeGroups = firstTariffRateNodes[0]["RefCusApplicability"].SelectNodes("RefCusExcludedTradeGroup");
			Assert.AreEqual("DE", firstExcludedTradeGroups[0]["ZZC_ZZA_NKTradeGroup"].InnerText);

			Assert.AreEqual("2002-01-01 00:00", firstTariffRateNodes[1]["ZZ2_StartDate"].InnerText);
			Assert.AreEqual("2079-06-06 23:59", firstTariffRateNodes[1]["ZZ2_EndDate"].InnerText);
			Assert.AreEqual("ADSZR", firstTariffRateNodes[1]["ZZ2_ZY1_NKRateCode"].InnerText);
			Assert.AreEqual("DUT", firstTariffRateNodes[1]["ZZ2_ZY1_ZZR_NKRateType"].InnerText);
			Assert.AreEqual("EUN", firstTariffRateNodes[1]["ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"].InnerText);
			Assert.AreEqual("7.750 * [DTN]", firstTariffRateNodes[1]["ZZ2_RateFormula"].InnerText);
			Assert.AreEqual("EUN", firstTariffRateNodes[1]["ZZ2_ZZZ_NKDataGrouping"].InnerText);
			Assert.AreEqual("2002-01-01 00:00", firstTariffRateNodes[1]["RefCusApplicability"]["ZZT_StartDate"].InnerText);
			Assert.AreEqual("2079-06-06 23:59", firstTariffRateNodes[1]["RefCusApplicability"]["ZZT_EndDate"].InnerText);
			Assert.AreEqual("DE", firstTariffRateNodes[1]["RefCusApplicability"]["ZZT_ZZA_NKTradeGroup"].InnerText);
			Assert.AreEqual("EUN", firstTariffRateNodes[1]["RefCusApplicability"]["ZZT_ZZA_ZZZ_NKDataGrouping"].InnerText);
			Assert.AreEqual("1", firstTariffRateNodes[1]["RefCusApplicability"]["ZZT_AdditionalCode"].InnerText);
		}

		[Test]
		public void TestExportXml()
		{
			var publicationTime = DateTime.Parse("2017-02-06T00:00:00", CultureInfo.InvariantCulture);
			var fileName = Path.Combine(binPath, "MeursingData.xml");

			var meursingData = ExcelParser.ReadXlsFileIntoResults(excelFileNameAndPath);
			var geoData = ExcelParser.ReadGeographicalXlsFileIntoResults(geoDataExcelFilenameAndPath);

			XMLGeneration.ExportToXMLFile(publicationTime, meursingData, geoData, fileName, null);
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(fileName);

			var dataSource = xmlDoc.SelectNodes("UniversalReferenceData/DataSource")[0];
			var publicationTypeNode = xmlDoc.SelectNodes("UniversalReferenceData/UpdateType")[0];
			var publicationTimeNode = xmlDoc.SelectNodes("UniversalReferenceData/PublicationTime")[0];

			Assert.AreEqual("EUN Meursing Data", dataSource.InnerText);
			Assert.AreEqual("FULL", publicationTypeNode.InnerText);
			Assert.AreEqual("2017-02-06T00:00:00", publicationTimeNode.InnerText);

			var tariffNodes = xmlDoc.SelectNodes("UniversalReferenceData/RefCusTariff");
			Assert.AreEqual(504, tariffNodes.Count);

			var firstTariffNode = tariffNodes[0];
			Assert.AreEqual("MEU", firstTariffNode["ZZ1_ZZI_NKTariffType"].InnerText);
			Assert.AreEqual("7000", firstTariffNode["ZZ1_TariffCode"].InnerText);
			Assert.AreEqual("7000 Meursing Additional Code", firstTariffNode["ZZ1_Description"].InnerText);
			Assert.AreEqual("EUN", firstTariffNode["ZZ1_ZZZ_NKDataGrouping"].InnerText);
			Assert.AreEqual("2002-01-01 00:00", firstTariffNode["ZZ1_StartDate"].InnerText);
			Assert.AreEqual("2079-06-06 23:59", firstTariffNode["ZZ1_EndDate"].InnerText);

			var firstTariffRateNodes = firstTariffNode.SelectNodes("RefCusRate");
			Assert.AreEqual("2002-01-01 00:00", firstTariffRateNodes[0]["ZZ2_StartDate"].InnerText);
			Assert.AreEqual("2079-06-06 23:59", firstTariffRateNodes[0]["ZZ2_EndDate"].InnerText);
			Assert.AreEqual("ADSZR", firstTariffRateNodes[0]["ZZ2_ZY1_NKRateCode"].InnerText);
			Assert.AreEqual("DUT", firstTariffRateNodes[0]["ZZ2_ZY1_ZZR_NKRateType"].InnerText);
			Assert.AreEqual("EUN", firstTariffRateNodes[0]["ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"].InnerText);
			Assert.AreEqual("0", firstTariffRateNodes[0]["ZZ2_RateFormula"].InnerText);
			Assert.AreEqual("EUN", firstTariffRateNodes[0]["ZZ2_ZZZ_NKDataGrouping"].InnerText);
			Assert.AreEqual("2002-01-01 00:00", firstTariffRateNodes[0]["RefCusApplicability"]["ZZT_StartDate"].InnerText);
			Assert.AreEqual("2079-06-06 23:59", firstTariffRateNodes[0]["RefCusApplicability"]["ZZT_EndDate"].InnerText);
			Assert.AreEqual("2012", firstTariffRateNodes[0]["RefCusApplicability"]["ZZT_ZZA_NKTradeGroup"].InnerText);
			Assert.AreEqual("EUN", firstTariffRateNodes[0]["RefCusApplicability"]["ZZT_ZZA_ZZZ_NKDataGrouping"].InnerText);
			Assert.AreEqual("1", firstTariffRateNodes[0]["RefCusApplicability"]["ZZT_AdditionalCode"].InnerText);
			Assert.AreEqual("ADSZ", firstTariffRateNodes[9]["ZZ2_ZY1_NKRateCode"].InnerText);

			var firstExcludedTradeGroups = firstTariffRateNodes[0]["RefCusApplicability"].SelectNodes("RefCusExcludedTradeGroup");
			Assert.AreEqual("NO", firstExcludedTradeGroups[0]["ZZC_ZZA_NKTradeGroup"].InnerText);
			Assert.AreEqual("IS", firstExcludedTradeGroups[1]["ZZC_ZZA_NKTradeGroup"].InnerText);

			var lastTariffNode = tariffNodes[tariffNodes.Count - 1];
			Assert.AreEqual("MEU", lastTariffNode["ZZ1_ZZI_NKTariffType"].InnerText);
			Assert.AreEqual("7996", lastTariffNode["ZZ1_TariffCode"].InnerText);
			Assert.AreEqual("7996 Meursing Additional Code", lastTariffNode["ZZ1_Description"].InnerText);
			Assert.AreEqual("EUN", lastTariffNode["ZZ1_ZZZ_NKDataGrouping"].InnerText);
			Assert.AreEqual("2002-01-01 00:00", lastTariffNode["ZZ1_StartDate"].InnerText);
			Assert.AreEqual("2079-06-06 23:59", lastTariffNode["ZZ1_EndDate"].InnerText);

			var lastTariffRateNodes = lastTariffNode.SelectNodes("RefCusRate");
			Assert.AreEqual("2002-01-01 00:00", lastTariffRateNodes[0]["ZZ2_StartDate"].InnerText);
			Assert.AreEqual("2079-06-06 23:59", lastTariffRateNodes[0]["ZZ2_EndDate"].InnerText);
			Assert.AreEqual("ADSZR", lastTariffRateNodes[0]["ZZ2_ZY1_NKRateCode"].InnerText);
			Assert.AreEqual("DUT", lastTariffRateNodes[0]["ZZ2_ZY1_ZZR_NKRateType"].InnerText);
			Assert.AreEqual("EUN", lastTariffRateNodes[0]["ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"].InnerText);
			Assert.AreEqual("9.750 * [DTN]", lastTariffRateNodes[0]["ZZ2_RateFormula"].InnerText);
			Assert.AreEqual("EUN", lastTariffRateNodes[0]["ZZ2_ZZZ_NKDataGrouping"].InnerText);
			Assert.AreEqual("2002-01-01 00:00", lastTariffRateNodes[0]["RefCusApplicability"]["ZZT_StartDate"].InnerText);
			Assert.AreEqual("2079-06-06 23:59", lastTariffRateNodes[0]["RefCusApplicability"]["ZZT_EndDate"].InnerText);
			Assert.AreEqual("2012", lastTariffRateNodes[0]["RefCusApplicability"]["ZZT_ZZA_NKTradeGroup"].InnerText);
			Assert.AreEqual("EUN", lastTariffRateNodes[0]["RefCusApplicability"]["ZZT_ZZA_ZZZ_NKDataGrouping"].InnerText);
			Assert.AreEqual("1", lastTariffRateNodes[0]["RefCusApplicability"]["ZZT_AdditionalCode"].InnerText);

			File.Delete(fileName);
		}

		[SetUp]
		public void Setup()
		{
			binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			excelFileNameAndPath = Path.Combine(binPath, @"CustomsMeursing\TestFiles\Additional agricultural duties - Meursing.xlsx");
			geoDataExcelFilenameAndPath = Path.Combine(binPath, @"CustomsMeursing\TestFiles\Geographical areas composition.xlsx");
		}
		string binPath;
		string excelFileNameAndPath;
		string geoDataExcelFilenameAndPath;
	}
}
