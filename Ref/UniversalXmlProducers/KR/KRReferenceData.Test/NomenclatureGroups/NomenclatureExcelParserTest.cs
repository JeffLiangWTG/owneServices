using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using NPOI.SS.Formula.Functions;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	[TestFixture]
	public class NomenclatureExcelParserTest
	{
		[TestCase(1, 1, 2017)]
		public void TestNomenclatures2017(int day, int month, int year)
		{
			Nomenclatures(day, month, year);
		}

		[TestCase(1, 1, 2018)]
		public void TestNomenclatures2018(int day, int month, int year)
		{
			Nomenclatures(day, month, year);
		}

		[TestCase(1, 1, 2019)]
		public void TestNomenclatures2019(int day, int month, int year)
		{
			Nomenclatures(day, month, year);
		}

		[TestCase(1, 1, 2020)]
		public void TestNomenclatures2020(int day, int month, int year)
		{
			Nomenclatures(day, month, year);
		}

		[TestCase(1, 1, 2021)]
		public void TestNomenclatures2021(int day, int month, int year)
		{
			Nomenclatures(day, month, year);
		}

		[TestCase(1, 1, 2022)]
		public void TestNomenclatures2022(int day, int month, int year)
		{
			Nomenclatures(day, month, year);
		}

		[TestCase(1, 1, 2023)]
		public void TestNomenclatures2023(int day, int month, int year)
		{
			Nomenclatures(day, month, year);
		}

		[TestCase(1, 1, 2024)]
		public void TestNomenclatures2024(int day, int month, int year)
		{
			Nomenclatures(day, month, year);
		}

		[TestCase(1, 1, 2025)]
		public void TestNomenclatures2025(int day, int month, int year)
		{
			Nomenclatures(day, month, year);
		}

		public void Nomenclatures(int day, int month, int year)
		{
			var publicationDate = new DateTime(year, month, day);
			var inputConfigPath = string.Format(CultureInfo.CurrentCulture, @"Res\Nomenclatures\{0}\KRNomenclatures_{0}Configuration.xml", year);
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"Nomenclatures\Output\KRNomenclatures_{0}OutFile.xml", year));
			string resourceContent = string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.Nomenclatures.Output.KRNomenclatures_{0}OutFile.xml", year);
			var expectedXML = TestHelper.ReadManifestResourceContent(resourceContent);
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}

			var inputDataPathXls = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"Nomenclatures\Input\{0}\KRNomenclatures_{0}DataFile_Sample.xls", year));
			new NomenclatureExcelParserForTest(inputConfigPath, inputDataPathXls).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}

	class NomenclatureExcelParserForTest : NomenclatureExcelParser
	{
		public NomenclatureExcelParserForTest(string configFilePath, string dataFilePath) : base(configFilePath, dataFilePath) { }
		protected override NomenclatureAdditionalDataUpdater AdditionalDataUpdater => new NomenclatureAdditionalDataUpdaterForTest();
	}

	class NomenclatureAdditionalDataUpdaterForTest : NomenclatureAdditionalDataUpdater
	{
		protected override ISafeRepository SafeRepository => new RefDataEntityLoaderTest().GetSafeRepository(nomenclatureInputData);

		readonly List<Tuple<string, string, string, string, DateTime, DateTime>> nomenclatureInputData = new List<Tuple<string, string, string, string, DateTime, DateTime>>() {
			new Tuple<string, string, string, string, DateTime, DateTime>("010290", "Test1_1", "01.01..02.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("010599", "Test1_2", "01.01..05.9.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0106", "Test1_3", "01.01..06", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("010619", "Test1_4", "01.01..06.1.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("010690", "Test1_5", "01.01..06.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0207", "Test1_6", "01.02..07", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("020890", "Test1_7", "01.02..08.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0210", "Test1_8", "01.02..10", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0301", "Test1_9", "01.03..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("030289", "Test1_10", "01.03..02.8.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0303", "Test1_11", "01.03..03", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0304", "Test1_12", "01.03..04", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("030520", "Test1_13", "01.03..05.2.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0306", "Test1_14", "01.03..06", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("030695", "Test1_15", "01.03..06.9.5", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0307", "Test1_16", "01.03..07", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0308", "Test1_17", "01.03..08", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("040210", "Test1_18", "01.04..02.1.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0404", "Test1_19", "01.04..04", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("040610", "Test1_20", "01.04..06.1.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0504", "Test1_21", "01.05..04", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("050690", "Test1_22", "01.05..06.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0507", "Test1_23", "01.05..07", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("050800", "Test1_24", "01.05..08.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0511", "Test1_25", "01.05..11", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("060120", "Test1_26", "01.06..01.2.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0602", "Test1_27", "01.06..02", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("070310", "Test1_28", "01.07..03.1.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("070959", "Test1_29", "01.07..09.5.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0710", "Test1_30", "01.07..10", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0711", "Test1_31", "01.07..11", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0712", "Test1_32", "01.07..12", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("0714", "Test1_33", "01.07..14", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("080299", "Test1_34", "01.08..02.9.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("080550", "Test1_35", "01.08..05.5.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),

			new Tuple<string, string, string, string, DateTime, DateTime>("1001", "Test2_1", "02.10..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("100199", "Test2_2", "02.10..01.9.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("1209", "Test2_3", "02.12..09", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("1211", "Test2_4", "02.12..11", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("1212", "Test2_5", "02.12..12", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("1214", "Test2_6", "02.12..14", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("121490", "Test2_7", "02.12..14.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("130190", "Test2_8", "02.13..01.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("1302", "Test2_9", "02.13..02", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("1404", "Test2_10", "02.14..04", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("150210", "Test2_11", "02.15..02.1.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("150790", "Test2_12", "02.15..07.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("1512", "Test2_13", "02.15..12", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("151329", "Test2_14", "02.15..13.2.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("151499", "Test2_15", "02.15..14.9.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("151590", "Test2_16", "02.15..15.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("1516", "Test2_17", "02.15..16", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("151800", "Test2_18", "02.15..18.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("160232", "Test2_19", "02.16..02.3.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("1604", "Test2_20", "02.16..04", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("1605", "Test2_21", "02.16..05", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("170490", "Test2_22", "02.17..04.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("1806", "Test2_23", "02.18..06", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("180690", "Test2_24", "04.18..06.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("1901", "Test2_25", "02.19..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("190230", "Test2_26", "02.19..02.3.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("190490", "Test2_27", "02.19..04.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("1905", "Test2_28", "02.19..05", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),

			new Tuple<string, string, string, string, DateTime, DateTime>("200190", "Test3_1", "03.20..01.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2006", "Test3_2", "03.20..06", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2008", "Test3_3", "03.20..08", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("200897", "Test3_4", "03.20..08.9.7", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("200899", "Test3_5", "03.20..08.9.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2009", "Test3_6", "03.20..09", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2101", "Test3_7", "03.21..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2102", "Test3_8", "03.21..02", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2103", "Test3_9", "03.21..03", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2105", "Test3_10", "03.21..05", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2106", "Test3_11", "03.21..06", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2206", "Test3_12", "03.22..06", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("220710", "Test3_13", "03.22..07.1.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2309", "Test3_14", "03.23..09", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("240419", "Test3_15", "03.24..04.1.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2501", "Test3_16", "03.25..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("250590", "Test3_17", "03.25..05.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("250700", "Test3_18", "03.25..07.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2513", "Test3_19", "03.25..13", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2530", "Test3_20", "03.25..30", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("261900", "Test3_21", "03.26..19.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("270112", "Test3_22", "03.27..01.1.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("270400", "Test3_23", "03.27..04.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("270900", "Test3_24", "03.27..09.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2710", "Test3_25", "03.27..10", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("271020", "Test3_26", "05.27..10.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2712", "Test3_27", "03.27..12", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2803", "Test3_28", "03.28..03", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("280461", "Test3_29", "03.28..04.6.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("280700", "Test3_30", "03.28..07.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("280800", "Test3_31", "03.28..08.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("280920", "Test3_32", "03.28..09.2.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("281000", "Test3_33", "03.28..10.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("281122", "Test3_34", "03.28..11.2.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("281290", "Test3_35", "03.28..12.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("281990", "Test3_36", "03.28..19.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2822", "Test3_37", "03.28..22", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2825", "Test3_38", "03.28..25", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("282619", "Test3_39", "03.28..26.1.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2827", "Test3_40", "03.28..27", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2833", "Test3_41", "03.28..33", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2836", "Test3_42", "03.28..36", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("283719", "Test3_43", "03.28..37.1.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2841", "Test3_44", "03.28..41", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("284390", "Test3_45", "03.28..43.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("284990", "Test3_46", "03.28..49.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("290420", "Test3_47", "03.29..04.2.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2905", "Test3_48", "03.29..05", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("290930", "Test3_49", "03.29..09.3.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2915", "Test3_50", "03.29..15", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2916", "Test3_51", "03.29..16", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("291712", "Test3_52", "03.29..17.1.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2918", "Test3_53", "03.29..18", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("291990", "Test3_54", "03.29..19.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2920", "Test3_55", "03.29..20", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2921", "Test3_56", "03.29..21", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2924", "Test3_57", "03.29..24", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2925", "Test3_58", "03.29..25", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2927", "Test3_59", "03.29..27", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2930", "Test3_60", "03.29..30", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("293190", "Test3_61", "03.29..31.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2932", "Test3_62", "03.29..32", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2933", "Test3_63", "03.29..33", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("293410", "Test3_64", "03.29..34.1.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("293629", "Test3_65", "03.29..36.2.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2937", "Test3_66", "03.29..37", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("293790", "Test3_67", "06.29.11.37.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("293980", "Test3_68", "03.29..39.8.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("2940", "Test3_69", "03.29..40", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("294110", "Test3_70", "03.29..41.1.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),

			new Tuple<string, string, string, string, DateTime, DateTime>("3001", "Test4_1", "04.30..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("300190", "Test4_2", "04.30..01.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3004", "Test4_3", "04.30..04", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("300420", "Test4_4", "04.30..04.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("300439", "Test4_5", "04.30..04.3.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("300449", "Test4_6", "06.30..04.4.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("300450", "Test4_7", "06.30..04.5", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("300490", "Test4_8", "04.30..04.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3006", "Test4_9", "04.30..06", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("300610", "Test4_10", "04.30..06.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("310490", "Test4_11", "04.31..04.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("320300", "Test4_12", "04.32..03.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("320490", "Test4_13", "04.32..04.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3208", "Test4_14", "04.32..08", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3209", "Test4_15", "04.32..09", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3210", "Test4_16", "04.32..10", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("321410", "Test4_17", "04.32..14.1.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3215", "Test4_18", "04.32..15", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3301", "Test4_19", "04.33..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3302", "Test4_20", "04.33..02", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3306", "Test4_21", "04.33..06", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("340119", "Test4_22", "04.34..01.1.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("340490", "Test4_23", "04.34..04.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("340590", "Test4_24", "04.34..05.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3501", "Test4_25", "04.35..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("350190", "Test4_26", "06.35..01.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("350300", "Test4_27", "04.35..03.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3504", "Test4_28", "04.35..04", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3505", "Test4_29", "04.35..05", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("350790", "Test4_30", "04.35..07.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3701", "Test4_31", "04.37..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("370400", "Test4_32", "04.37..04.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3705", "Test4_33", "04.37..05", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3707", "Test4_34", "04.37..07", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("380700", "Test4_35", "04.38..07.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3814", "Test4_36", "04.38..14", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("381600", "Test4_37", "04.38..16.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("381800", "Test4_38", "04.38..18.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3822", "Test4_39", "04.38..22", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("382370", "Test4_40", "04.38..23.7.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3824", "Test4_41", "04.38..24", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3907", "Test4_42", "04.39..07", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("391000", "Test4_43", "04.39..10.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("391390", "Test4_44", "04.39..13.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("391590", "Test4_45", "04.39..15.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("392099", "Test4_46", "04.39..20.9.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("3921", "Test4_47", "04.39..21", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),

			new Tuple<string, string, string, string, DateTime, DateTime>("400239", "Test5_1", "05.40..02.3.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4004", "Test5_2", "05.40..04", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("400400", "Test5_3", "05.40..04.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4011", "Test5_4", "05.40..11", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("401120", "Test5_5", "05.40..11.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("401180", "Test5_6", "05.40..11.8", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4012", "Test5_7", "05.40..12", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("401220", "Test5_8", "05.40..12.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("401390", "Test5_9", "05.40..13.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("401699", "Test5_10", "05.40..16.9.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4101", "Test5_11", "05.41..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4103", "Test5_12", "05.41..03", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4202", "Test5_13", "05.42..02", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4203", "Test5_14", "05.42..03", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4205", "Test5_15", "05.42..05", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4302", "Test5_16", "05.43..02", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4303", "Test5_17", "05.43..03", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4402", "Test5_18", "05.44..02", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4403", "Test5_19", "05.44..03", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4407", "Test5_20", "05.44..07", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4408", "Test5_21", "05.44..08", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("440839", "Test5_22", "09.44..08.3.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4411", "Test5_23", "05.44..11", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4412", "Test5_24", "05.44..12", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("442090", "Test5_25", "05.44..20.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("442199", "Test5_26", "05.44..21.9.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4802", "Test5_27", "05.48..02", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("480591", "Test5_28", "05.48..05.9.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("480990", "Test5_29", "05.48..09.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("481092", "Test5_30", "05.48..10.9.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4811", "Test5_31", "05.48..11", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("481490", "Test5_32", "05.48..14.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("481690", "Test5_33", "05.48..16.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4823", "Test5_34", "05.48..23", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("4902", "Test5_35", "05.49..02", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),

			new Tuple<string, string, string, string, DateTime, DateTime>("5002", "Test6_1", "06.50..02", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("500200", "Test6_2", "06.50..02.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("500720", "Test6_3", "06.50..07.7", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("5201", "Test6_4", "06.50..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("5205", "Test6_5", "06.52..05", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("520512", "Test6_6", "06.05..05.1.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("530290", "Test6_7", "06.53..02.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("5303", "Test6_8", "06.53..03", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("5402", "Test6_9", "06.54..02", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("5509", "Test6_10", "06.55..09", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("5608", "Test6_11", "06.56..08", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),

			new Tuple<string, string, string, string, DateTime, DateTime>("6109", "Test7_1", "07.61..09", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("6201", "Test7_2", "07.62..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("6202", "Test7_3", "07.62..02", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("620799", "Test7_4", "07.62..07.9.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("6210", "Test7_5", "07.62..10", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("621020", "Test7_6", "11.62..10.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("630790", "Test7_7", "07.63..07.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("6401", "Test7_8", "07.64..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("6505", "Test7_9", "07.65..05", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("690100", "Test7_10", "07.69..01.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("6903", "Test7_11", "07.69..03", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("691200", "Test7_12", "07.69..12.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),

			new Tuple<string, string, string, string, DateTime, DateTime>("7005", "Test8_1", "08.70..05", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("700510", "Test8_2", "08.70..05.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("700521", "Test8_3", "08.70..05.2.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("700529", "Test8_4", "08.70..05.2.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("701400", "Test8_5", "08.70..14.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("701690", "Test8_6", "08.70..16.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("701969", "Test8_7", "08.70..19.6.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7020", "Test8_8", "08.70..20", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7103", "Test8_9", "08.71..03", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("710499", "Test8_10", "08.71..04.9.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7108", "Test8_11", "08.71..08", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7115", "Test8_12", "08.71..15", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("711620", "Test8_13", "08.71..16.2.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7210", "Test8_14", "08.72..10", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7212", "Test8_15", "08.72..12", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7213", "Test8_16", "08.72..13", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7219", "Test8_17", "08.72..19", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7220", "Test8_18", "08.72..20", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7225", "Test8_19", "08.72..25", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("722790", "Test8_20", "08.72..27.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("722870", "Test8_21", "08.72..28.7.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7229", "Test8_22", "08.72..29", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7302", "Test8_23", "08.73..02", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("730300", "Test8_24", "08.73..03.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7306", "Test8_25", "08.73..06", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7312", "Test8_26", "08.73..12", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7317", "Test8_27", "08.73..17", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("731990", "Test8_28", "08.73..19.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("732290", "Test8_29", "08.73..22.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("740329", "Test8_30", "08.74..03.2.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("740729", "Test8_31", "08.74..07.2.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7409", "Test8_32", "08.74..09", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7418", "Test8_33", "08.74..18", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("741980", "Test8_34", "15.74..19.8", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7501", "Test8_35", "08.75..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("750890", "Test8_36", "08.75..08.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("760410", "Test8_37", "08.76..04.1.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("761290", "Test8_38", "08.76..12.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("761510", "Test8_39", "08.76..15.1.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("761699", "Test8_40", "08.76..16.9.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("780199", "Test8_41", "08.78..01.9.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("780600", "Test8_42", "08.78..06.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("7907", "Test8_43", "08.79..07", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),

			new Tuple<string, string, string, string, DateTime, DateTime>("8003", "Test9_1", "09.80..03", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8007", "Test9_2", "09.80..07", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8101", "Test9_3", "09.81..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("810199", "Test9_4", "09.80..01.9.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8109", "Test9_5", "09.81..09", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("810991", "Test9_6", "09.80..09.9.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("810999", "Test9_7", "09.81..09.9.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("820239", "Test9_8", "09.82..02.3.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("820750", "Test9_9", "09.82..07.5.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8209", "Test9_10", "09.82..09", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("821300", "Test9_11", "09.82..13.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("830140", "Test9_12", "09.83..01.4.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("831130", "Test9_13", "09.83..11.3.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8408", "Test9_14", "09.84..08", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8409", "Test9_15", "09.84..09", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8410", "Test9_16", "09.84..10", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8411", "Test9_17", "09.84..11", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("841290", "Test9_18", "09.84..12.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8413", "Test9_19", "09.84..13", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8414", "Test9_20", "09.84..14", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8415", "Test9_21", "09.84..15", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8417", "Test9_22", "09.84..17", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8418", "Test9_23", "09.84..18", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8419", "Test9_24", "09.84..19", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8421", "Test9_25", "09.84..21", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("842240", "Test9_26", "09.84..22.4.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8423", "Test9_27", "09.84..23", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8424", "Test9_28", "09.84..24", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8425", "Test9_29", "09.84..25", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8427", "Test9_30", "09.84..27", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("842833", "Test9_31", "09.84..28.3.3", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8429", "Test9_32", "09.84..29", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("843360", "Test9_33", "09.84..33.6.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8443", "Test9_34", "09.84..43", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("844520", "Test9_35", "09.84..45.2.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("844630", "Test9_36", "09.84..46.3.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8447", "Test9_37", "09.84..47", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("845180", "Test9_38", "09.84..51.8.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("845210", "Test9_39", "09.84..52.1.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("845430", "Test9_40", "09.84..54.3.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("845630", "Test9_41", "09.84..56.3.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("845710", "Test9_42", "09.84..57.1.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("846140", "Test9_43", "09.84..61.4.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8462", "Test9_44", "09.84..62", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("846789", "Test9_45", "09.84..67.8.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8471", "Test9_46", "09.84..71", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("847290", "Test9_47", "09.84..72.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8473", "Test9_48", "09.84..73", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8479", "Test9_49", "09.84..79", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8481", "Test9_50", "09.84..81", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8483", "Test9_51", "09.84..83", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8486", "Test9_52", "09.84..86", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("848620", "Test9_53", "16.84..86.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("848790", "Test9_54", "09.84..87.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8501", "Test9_55", "09.85..01", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8502", "Test9_56", "09.85..02", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("850300", "Test9_57", "09.85..03.0.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8504", "Test9_58", "09.85..04", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("850590", "Test9_59", "09.85..05.9.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8512", "Test9_60", "09.85..12", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8513", "Test9_61", "09.85..13", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8515", "Test9_62", "09.85..15", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8517", "Test9_63", "09.85..17", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("851810", "Test9_64", "09.85..18.1.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8523", "Test9_65", "09.85..23", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8526", "Test9_66", "09.85..26", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8527", "Test9_67", "09.85..27", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8528", "Test9_68", "09.85..28", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8529", "Test9_69", "09.85..29", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8536", "Test9_70", "09.85..36", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("853710", "Test9_71", "09.85..37.1.0", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8539", "Test9_72", "09.85..39", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("854232", "Test9_73", "09.85..42.3.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8543", "Test9_74", "09.85..43", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("8544", "Test9_75", "09.85..44", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("870191", "Test9_76", "09.87..01.9.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),

			new Tuple<string, string, string, string, DateTime, DateTime>("900211", "Test10_1", "10.90..02.1.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("900290", "Test10_2", "10.90..02.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("900653", "Test10_3", "10.90..06.5.3", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("901010", "Test10_4", "10.90..10.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("901090", "Test10_5", "10.90..10.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("901120", "Test10_6", "10.90..11.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("901210", "Test10_7", "10.90..12.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("901410", "Test10_8", "10.90..14.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			new Tuple<string, string, string, string, DateTime, DateTime>("9503", "Test10_9", "20.95..03", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
		};
	}
}
