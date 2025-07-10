using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	[TestFixture]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1505:Avoid unmaintainable code", Justification = "<Pending>")]
	class ExcelEntityUpdaterTest
	{
		[Test]
		public void TestTariffs2017()
		{
			TariffUpdate(2017);
		}

		[Test]
		public void TestTariffs2018()
		{
			TariffUpdate(2018);
		}

		public void TariffUpdate(int year)
		{
			var workbook = new XSSFWorkbook(Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"Tariffs\Input\{0}\Tariffs_{0}DataFile_Sample.xlsx", year)));
			var configFilePath = string.Format(CultureInfo.CurrentCulture, @"Res\Tariffs\{0}\Tariffs_{0}Configuration.xml", year);
			var configuration = EntityConfigurationManager.DeserializeXML<EntityConfiguration>(configFilePath);
			var tariffs = new ExcelEntityUpdater<RefCusTariff>(configuration, new TariffAdditionalDataUpdaterForTest()).Update(workbook);

			DateTime endDate = new DateTime(year, 12, 31, 23, 59, 00);
			Assert.AreEqual(18, tariffs.Count);

			Assert.AreEqual("0101211000", tariffs[0].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2013, 01, 01), tariffs[0].ZZ1_StartDate);
			Assert.AreEqual("For farm breeding", tariffs[0].ZZ1_Description);
			Assert.AreEqual("01.01..01.2.1.1.0.0.0", tariffs[0].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[0].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("농가 사육용", tariffs[0].RefCusTariffLanguages[0].ZX7_Description);
			Assert.AreEqual(2, tariffs[0].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[0].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("U", tariffs[0].RefCusTariffUOMs[0].ZZ8_UOM);
			Assert.AreEqual("CU2", tariffs[0].RefCusTariffUOMs[1].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[0].RefCusTariffUOMs[1].ZZ8_UOM);

			Assert.AreEqual("0102211000", tariffs[1].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2017, 01, 01), tariffs[1].ZZ1_StartDate);
			Assert.AreEqual("For milk", tariffs[1].ZZ1_Description);
			Assert.AreEqual("01.01..02.2.1.1.0.0.0", tariffs[1].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[1].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("젖소", tariffs[1].RefCusTariffLanguages[0].ZX7_Description);
			Assert.AreEqual(2, tariffs[1].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[1].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("U", tariffs[1].RefCusTariffUOMs[0].ZZ8_UOM);
			Assert.AreEqual("CU2", tariffs[1].RefCusTariffUOMs[1].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[1].RefCusTariffUOMs[1].ZZ8_UOM);

			Assert.AreEqual("0201100000", tariffs[2].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2002, 01, 01), tariffs[2].ZZ1_StartDate);
			Assert.AreEqual("Carcasses and half-carcasses", tariffs[2].ZZ1_Description);
			Assert.AreEqual("01.02..01.1.0.0.0.0", tariffs[2].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[2].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("도체(屠體)와 이분도체(二分屠體)", tariffs[2].RefCusTariffLanguages[0].ZX7_Description);
			Assert.AreEqual(1, tariffs[2].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[2].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[2].RefCusTariffUOMs[0].ZZ8_UOM);

			Assert.AreEqual("0208401000", tariffs[3].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2017, 01, 01), tariffs[3].ZZ1_StartDate);
			Assert.AreEqual("Of whales, dolphins and porpoises(mammals of the order Cetacea); of manatees and dugongs(mammals of the order Sirenia)", tariffs[3].ZZ1_Description);
			Assert.AreEqual("01.02..08.4.1.0.0.0", tariffs[3].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[3].RefCusTariffLanguages.GetLength(0));
			if (year == 2017)
			{
				Assert.AreEqual("고래ㆍ돌고래류(고래목의 포유동물), 바다소(바다소목의 포유동물)의 것", tariffs[3].RefCusTariffLanguages[0].ZX7_Description);
			}
			else if (year == 2018)
			{
				Assert.AreEqual("고래 ∙ 돌고래류(고래목의 포유동물), 바다소(바다소목의 포유동물)의 것", tariffs[3].RefCusTariffLanguages[0].ZX7_Description);
			}
			Assert.AreEqual(1, tariffs[3].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[3].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[3].RefCusTariffUOMs[0].ZZ8_UOM);

			Assert.AreEqual("0305593000", tariffs[4].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2002, 01, 01), tariffs[4].ZZ1_StartDate);
			Assert.AreEqual("Alaska pollack", tariffs[4].ZZ1_Description);
			Assert.AreEqual("01.03..05.5.9.3.0.0.0", tariffs[4].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[4].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("명태(북어)", tariffs[4].RefCusTariffLanguages[0].ZX7_Description);
			Assert.AreEqual(1, tariffs[4].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[4].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[4].RefCusTariffUOMs[0].ZZ8_UOM);

			Assert.AreEqual("2402201000", tariffs[5].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2015, 01, 01), tariffs[5].ZZ1_StartDate);
			Assert.AreEqual("Filter tip cigarettes", tariffs[5].ZZ1_Description);
			Assert.AreEqual("04.24..02.2.1.0.0.0", tariffs[5].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[5].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("필터담배", tariffs[5].RefCusTariffLanguages[0].ZX7_Description);
			Assert.AreEqual(2, tariffs[5].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[5].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("U", tariffs[5].RefCusTariffUOMs[0].ZZ8_UOM);
			Assert.AreEqual("CU2", tariffs[5].RefCusTariffUOMs[1].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[5].RefCusTariffUOMs[1].ZZ8_UOM);
			Assert.AreEqual(1, tariffs[5].RefCusTariffAttributes.GetLength(0));
			Assert.AreEqual("InvoiceQuantity in CU1", tariffs[5].RefCusTariffAttributes[0].ZZ3_Name);

			Assert.AreEqual("2939791000", tariffs[6].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2017, 01, 01), tariffs[6].ZZ1_StartDate);
			Assert.AreEqual("Nicotine and its salts", tariffs[6].ZZ1_Description);
			Assert.AreEqual("06.29.12.39.7.9.1.0.0.0", tariffs[6].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[6].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("니코틴과 그 염", tariffs[6].RefCusTariffLanguages[0].ZX7_Description);
			Assert.AreEqual(2, tariffs[6].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[6].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("ML", tariffs[6].RefCusTariffUOMs[0].ZZ8_UOM);
			Assert.AreEqual("CU2", tariffs[6].RefCusTariffUOMs[1].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[6].RefCusTariffUOMs[1].ZZ8_UOM);

			Assert.AreEqual("3701100000", tariffs[7].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2013, 01, 01), tariffs[7].ZZ1_StartDate);
			Assert.AreEqual("For X-ray", tariffs[7].ZZ1_Description);
			Assert.AreEqual("06.37..01.1.0.0.0.0", tariffs[7].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[7].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("엑스선용", tariffs[7].RefCusTariffLanguages[0].ZX7_Description);
			Assert.AreEqual(2, tariffs[7].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[7].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("M2", tariffs[7].RefCusTariffUOMs[0].ZZ8_UOM);
			Assert.AreEqual("CU2", tariffs[7].RefCusTariffUOMs[1].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[7].RefCusTariffUOMs[1].ZZ8_UOM);

			Assert.AreEqual("3701309930", tariffs[8].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2017, 01, 01), tariffs[8].ZZ1_StartDate);
			Assert.AreEqual("For flat panel display (blank mask)", tariffs[8].ZZ1_Description);
			Assert.AreEqual("06.37..01.3.9.9.3.0", tariffs[8].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[8].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("평판디스플레이용[블랭크마스크(blank mask)로 한정한다]", tariffs[8].RefCusTariffLanguages[0].ZX7_Description);
			Assert.AreEqual(2, tariffs[8].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[8].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("M2", tariffs[8].RefCusTariffUOMs[0].ZZ8_UOM);
			Assert.AreEqual("CU2", tariffs[8].RefCusTariffUOMs[1].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[8].RefCusTariffUOMs[1].ZZ8_UOM);

			Assert.AreEqual("3702523000", tariffs[9].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2013, 01, 01), tariffs[9].ZZ1_StartDate);
			Assert.AreEqual("For printed circuit board", tariffs[9].ZZ1_Description);
			Assert.AreEqual("06.37..02.5.2.3.0.0.0", tariffs[9].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[9].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("인쇄회로기판용", tariffs[9].RefCusTariffLanguages[0].ZX7_Description);
			Assert.AreEqual(2, tariffs[9].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[9].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("M", tariffs[9].RefCusTariffUOMs[0].ZZ8_UOM);
			Assert.AreEqual("CU2", tariffs[9].RefCusTariffUOMs[1].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[9].RefCusTariffUOMs[1].ZZ8_UOM);

			Assert.AreEqual("3702530000", tariffs[10].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2017, 01, 01), tariffs[10].ZZ1_StartDate);
			Assert.AreEqual("Of a width exceeding 16 ㎜ but not exceeding 35 ㎜ and of a length not exceeding 30 m, for slides", tariffs[10].ZZ1_Description);
			Assert.AreEqual("06.37..02.5.3.0.0.0.0", tariffs[10].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[10].RefCusTariffLanguages.GetLength(0));
			if (year == 2017)
			{
				Assert.AreEqual("폭이 16밀리미터를 초과하고 35밀리미터 이하로서 길이가 30미터 이하인 것(슬라이드용으로 한정한다)", tariffs[10].RefCusTariffLanguages[0].ZX7_Description);
			}
			else if (year == 2018)
			{
				Assert.AreEqual("폭이 16밀리미터 초과 35밀리미터 이하로서 길이가 30미터 이하인 것(슬라이드용으로 한정한다)", tariffs[10].RefCusTariffLanguages[0].ZX7_Description);
			}
			Assert.AreEqual(2, tariffs[10].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[10].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("M", tariffs[10].RefCusTariffUOMs[0].ZZ8_UOM);
			Assert.AreEqual("CU2", tariffs[10].RefCusTariffUOMs[1].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[10].RefCusTariffUOMs[1].ZZ8_UOM);

			Assert.AreEqual("4403232000", tariffs[11].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2017, 01, 01), tariffs[11].ZZ1_StartDate);
			Assert.AreEqual("Spruce (Picea spp.)", tariffs[11].ZZ1_Description);
			Assert.AreEqual("09.44..03.2.3.2.0.0.0", tariffs[11].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[11].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("가문비나무[피세아(Picea)종]", tariffs[11].RefCusTariffLanguages[0].ZX7_Description);
			Assert.AreEqual(2, tariffs[11].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[11].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("M3", tariffs[11].RefCusTariffUOMs[0].ZZ8_UOM);
			Assert.AreEqual("CU2", tariffs[11].RefCusTariffUOMs[1].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[11].RefCusTariffUOMs[1].ZZ8_UOM);

			Assert.AreEqual("4403410000", tariffs[12].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2013, 01, 01), tariffs[12].ZZ1_StartDate);
			Assert.AreEqual("Dark Red Meranti, Light Red Meranti and Meranti Bakau", tariffs[12].ZZ1_Description);
			Assert.AreEqual("09.44..03.4.1.0.0.0.0", tariffs[12].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[12].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("다크레드메란티(Dark Red Meranti)ㆍ라이트레드메란티(Light Red Meranti)ㆍ메란티바카우(Meranti Bakau)", tariffs[12].RefCusTariffLanguages[0].ZX7_Description);
			Assert.AreEqual(2, tariffs[12].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[12].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("M3", tariffs[12].RefCusTariffUOMs[0].ZZ8_UOM);
			Assert.AreEqual("CU2", tariffs[12].RefCusTariffUOMs[1].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[12].RefCusTariffUOMs[1].ZZ8_UOM);

			Assert.AreEqual("4203211000", tariffs[13].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2002, 01, 01), tariffs[13].ZZ1_StartDate);
			Assert.AreEqual("Baseball glove", tariffs[13].ZZ1_Description);
			Assert.AreEqual(1, tariffs[13].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("08.42..03.2.1.1.0.0.0", tariffs[13].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual("야구장갑", tariffs[13].RefCusTariffLanguages[0].ZX7_Description);
			Assert.AreEqual(2, tariffs[13].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[13].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("2U", tariffs[13].RefCusTariffUOMs[0].ZZ8_UOM);
			Assert.AreEqual("CU2", tariffs[13].RefCusTariffUOMs[1].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[13].RefCusTariffUOMs[1].ZZ8_UOM);

			Assert.AreEqual("7102210000", tariffs[14].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2013, 01, 01), tariffs[14].ZZ1_StartDate);
			Assert.AreEqual("Unworked or simply sawn, cleaved or bruted", tariffs[14].ZZ1_Description);
			Assert.AreEqual("14.71.01.02.2.1.0.0.0.0", tariffs[14].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[14].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("원석(단순히 톱질한 것이나 쪼갠 것으로 한정한다)", tariffs[14].RefCusTariffLanguages[0].ZX7_Description);
			if (year == 2017)
			{
				Assert.AreEqual(1, tariffs[14].RefCusTariffUOMs.GetLength(0));
				Assert.AreEqual("CU1", tariffs[14].RefCusTariffUOMs[0].ZZ8_Type);
				Assert.AreEqual("CR", tariffs[14].RefCusTariffUOMs[0].ZZ8_UOM);
			}
			else if (year == 2018)
			{
				Assert.AreEqual(2, tariffs[14].RefCusTariffUOMs.GetLength(0));
				Assert.AreEqual("CU1", tariffs[14].RefCusTariffUOMs[0].ZZ8_Type);
				Assert.AreEqual("CR", tariffs[14].RefCusTariffUOMs[0].ZZ8_UOM);
				Assert.AreEqual("CU2", tariffs[14].RefCusTariffUOMs[1].ZZ8_Type);
				Assert.AreEqual("KG", tariffs[14].RefCusTariffUOMs[1].ZZ8_UOM);
			}
			Assert.AreEqual("8523292231", tariffs[15].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2007, 01, 01), tariffs[15].ZZ1_StartDate);
			Assert.AreEqual("Those recorded video", tariffs[15].ZZ1_Description);
			Assert.AreEqual("16.85..23.2.9.2.2.3.1", tariffs[15].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[15].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("비디오 녹화된 것", tariffs[15].RefCusTariffLanguages[0].ZX7_Description);
			Assert.AreEqual(3, tariffs[15].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[15].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("U", tariffs[15].RefCusTariffUOMs[0].ZZ8_UOM);
			Assert.AreEqual("CU2", tariffs[15].RefCusTariffUOMs[1].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[15].RefCusTariffUOMs[1].ZZ8_UOM);
			Assert.AreEqual("CU3", tariffs[15].RefCusTariffUOMs[2].ZZ8_Type);
			Assert.AreEqual("MIN", tariffs[15].RefCusTariffUOMs[2].ZZ8_UOM);

			Assert.AreEqual("2402209000", tariffs[16].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2015, 01, 01), tariffs[16].ZZ1_StartDate);
			Assert.AreEqual("Other", tariffs[16].ZZ1_Description);
			Assert.AreEqual("04.24..02.2.9.0.0.0", tariffs[16].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[16].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("기타", tariffs[16].RefCusTariffLanguages[0].ZX7_Description);
			Assert.AreEqual(2, tariffs[16].RefCusTariffUOMs.GetLength(0));
			Assert.AreEqual("CU1", tariffs[16].RefCusTariffUOMs[0].ZZ8_Type);
			Assert.AreEqual("U", tariffs[16].RefCusTariffUOMs[0].ZZ8_UOM);
			Assert.AreEqual("CU2", tariffs[16].RefCusTariffUOMs[1].ZZ8_Type);
			Assert.AreEqual("KG", tariffs[16].RefCusTariffUOMs[1].ZZ8_UOM);
			Assert.AreEqual(1, tariffs[16].RefCusTariffAttributes.GetLength(0));
			Assert.AreEqual("InvoiceQuantity in CU1", tariffs[16].RefCusTariffAttributes[0].ZZ3_Name);

			Assert.AreEqual("8523292239", tariffs[17].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2007, 01, 01), tariffs[17].ZZ1_StartDate);
			Assert.AreEqual("Other", tariffs[17].ZZ1_Description);
			Assert.AreEqual("16.85..23.2.9.2.2.3.9", tariffs[17].ZZ1_CompositeKeyOnZZ5);
			Assert.AreEqual(1, tariffs[17].RefCusTariffLanguages.GetLength(0));
			Assert.AreEqual("기타", tariffs[17].RefCusTariffLanguages[0].ZX7_Description);
		}

		public class TariffAdditionalDataUpdaterForTest : TariffAdditionalDataUpdater
		{
			protected override ISafeRepository SafeRepository => new RefDataEntityLoaderTest().GetSafeRepository(nomenclatureInputData);

			List<Tuple<string, string, string, string, DateTime, DateTime>> nomenclatureInputData = new List<Tuple<string, string, string, string, DateTime, DateTime>>() {
				new Tuple<string, string, string, string, DateTime, DateTime>("010121", "Test1", "01.01..01.2.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
				new Tuple<string, string, string, string, DateTime, DateTime>("010221", "Test2", "01.01..02.2.1", "KR",new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
				new Tuple<string, string, string, string, DateTime, DateTime>("020110", "Test3", "01.02..01.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
				new Tuple<string, string, string, string, DateTime, DateTime>("020840", "Test4", "01.02..08.4", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
				new Tuple<string, string, string, string, DateTime, DateTime>("030559", "Test5", "01.03..05.5.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
				new Tuple<string, string, string, string, DateTime, DateTime>("240220", "Test6", "04.24..02.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
				new Tuple<string, string, string, string, DateTime, DateTime>("293979", "Test7", "06.29.12.39.7.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
				new Tuple<string, string, string, string, DateTime, DateTime>("370110", "Test8", "06.37..01.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
				new Tuple<string, string, string, string, DateTime, DateTime>("370130", "Test9", "06.37..01.3", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
				new Tuple<string, string, string, string, DateTime, DateTime>("370252", "Test10", "06.37..02.5.2", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
				new Tuple<string, string, string, string, DateTime, DateTime>("370253", "Test11", "06.37..02.5.3", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
				new Tuple<string, string, string, string, DateTime, DateTime>("440323", "Test12", "09.44..03.2.3", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
				new Tuple<string, string, string, string, DateTime, DateTime>("440341", "Test13", "09.44..03.4.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
				new Tuple<string, string, string, string, DateTime, DateTime>("420321", "Test14", "08.42..03.2.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
				new Tuple<string, string, string, string, DateTime, DateTime>("710221", "Test15", "14.71.01.02.2.1", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
				new Tuple<string, string, string, string, DateTime, DateTime>("852329", "Test16", "16.85..23.2.9", "KR", new DateTime(1900, 1, 1), new DateTime(2079, 12, 31)),
			};
		}

		[Test]
		public void TestExportNonGAReasonUpdate()
		{
			IWorkbook workbook;
			using (var file = new FileStream(Path.Combine(TestHelper.BaseRealFilePath, @"NonGAReasonTypes\2022\NonGAReasonType_2022.xls"), FileMode.Open, FileAccess.Read))
			{
				workbook = new HSSFWorkbook(file);
			}
			var configFilePath = ApplicationConfig.NonGAReasonExportConfigFileInputPath;
			var configuration = EntityConfigurationManager.DeserializeXML<EntityConfiguration>(configFilePath);
			var nonGAReasonTypes = new ExcelEntityUpdater<RefCusCodeList>(configuration, new NonGAReasonAdditionalDataUpdater()).Update(workbook);

			Assert.AreEqual(67, nonGAReasonTypes.Count);

			Assert.AreEqual("13101", nonGAReasonTypes[0].ZZD_Code);
			Assert.AreEqual("가축전염병 예방법 제31조, 동법 시행규칙 제31조에 따른 지정검역물에 해당하지 않음", nonGAReasonTypes[0].ZZD_Description);
			Assert.AreEqual("13901", nonGAReasonTypes[1].ZZD_Code);
			Assert.AreEqual("기타 세관장확인 수출요건 비대상 등 사유를 기재", nonGAReasonTypes[1].ZZD_Description);

			Assert.AreEqual("60101", nonGAReasonTypes[2].ZZD_Code);
			Assert.AreEqual("남북교류협력에 관한 법률 제13조, 제14조, 동법 시행령 제25조, 반출ㆍ반입 승인대상품목 및 승인절차에 관한 고시 제2조에 따른 남북교류 반출대상 품목에 해당되지 않음", nonGAReasonTypes[2].ZZD_Description);
			Assert.AreEqual("60406", nonGAReasonTypes[3].ZZD_Code);
			Assert.AreEqual("관세법 제226조에 따른 세관장확인물품 및 확인방법 지정고시 제7조 제3항에 따라 반출ㆍ반입 승인대상품목 및 승인절차에 관한 고시 제5조에 따른 포괄승인대상 물품", nonGAReasonTypes[3].ZZD_Description);
			Assert.AreEqual("60901", nonGAReasonTypes[4].ZZD_Code);
			Assert.AreEqual("기타 세관장확인 수출요건 비대상 등 사유를 기재", nonGAReasonTypes[4].ZZD_Description);

			Assert.AreEqual("82101", nonGAReasonTypes[5].ZZD_Code);
			Assert.AreEqual("농업생명자원의 보존ㆍ관리 및 이용에 관한 법률 제18조에 따른 인삼종자에 해당하지 않음", nonGAReasonTypes[5].ZZD_Description);
			Assert.AreEqual("82401", nonGAReasonTypes[6].ZZD_Code);
			Assert.AreEqual("관세법 제226조에 따른 세관장확인물품 및 확인방법 지정고시 제7조 제2항 제1호에 따라 대외무역법 시행령 제19조에 따른 수출승인면제물품", nonGAReasonTypes[6].ZZD_Description);
			Assert.AreEqual("82901", nonGAReasonTypes[7].ZZD_Code);
			Assert.AreEqual("기타 세관장확인 수출요건 비대상 등 사유를 기재", nonGAReasonTypes[7].ZZD_Description);

			Assert.AreEqual("69101", nonGAReasonTypes[8].ZZD_Code);
			Assert.AreEqual("마약류 관리에 관한 법률 제2조 제2호 바목 단서에 따른 한외마약(限外麻藥)", nonGAReasonTypes[8].ZZD_Description);
			Assert.AreEqual("69102", nonGAReasonTypes[9].ZZD_Code);
			Assert.AreEqual("마약류 관리에 관한 법률 제2조 제3호 마목 단서에 따른 신체적 또는 정신적 의존성을 야기하지 아니하는 제제", nonGAReasonTypes[9].ZZD_Description);
			Assert.AreEqual(1, nonGAReasonTypes[9].RefCusCodeListAttributes.Length);
			Assert.AreEqual("향정신성의약품 제외인정 신청서", nonGAReasonTypes[9].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("69901", nonGAReasonTypes[10].ZZD_Code);
			Assert.AreEqual("기타 세관장확인 수출요건 비대상 등 사유를 기재", nonGAReasonTypes[10].ZZD_Description);

			Assert.AreEqual("57101", nonGAReasonTypes[11].ZZD_Code);
			Assert.AreEqual("문화재보호법 제39조 제1항에 따른 문화재보호법에 따른 문화재에 해당하지 않음", nonGAReasonTypes[11].ZZD_Description);
			Assert.AreEqual("57401", nonGAReasonTypes[12].ZZD_Code);
			Assert.AreEqual("관세법 제226조에 따른 세관장확인물품 및 확인방법 지정고시 제7조 제2항 제1호에 따라 대외무역법 시행령 제19조에 따른 수출승인면제물품", nonGAReasonTypes[12].ZZD_Description);
			Assert.AreEqual("57901", nonGAReasonTypes[13].ZZD_Code);
			Assert.AreEqual("기타 세관장확인 수출요건 비대상 등 사유를 기재", nonGAReasonTypes[13].ZZD_Description);

			Assert.AreEqual("34101", nonGAReasonTypes[14].ZZD_Code);
			Assert.AreEqual("방위사업법 제53조 제2항에 따른 군용 총포ㆍ도검ㆍ화약류 등에 해당하지 않음", nonGAReasonTypes[14].ZZD_Description);
			Assert.AreEqual("34401", nonGAReasonTypes[15].ZZD_Code);
			Assert.AreEqual("관세법 제226조에 따른 세관장확인물품 및 확인방법 지정고시 제7조 제2항 제1호에 따라 대외무역법 시행령 제19조에 따른 수출승인면제물품", nonGAReasonTypes[15].ZZD_Description);
			Assert.AreEqual("34901", nonGAReasonTypes[16].ZZD_Code);
			Assert.AreEqual("기타 세관장확인 수출요건 비대상 등 사유를 기재", nonGAReasonTypes[16].ZZD_Description);

			Assert.AreEqual("85101", nonGAReasonTypes[17].ZZD_Code);
			Assert.AreEqual("생물다양성 보전 및 이용에 관한 법률 제11조 제2항, 국외반출 승인대상 생물자원 지정 별표에 따른 국외반출 승인대상 생물자원에 해당하지 않음", nonGAReasonTypes[17].ZZD_Description);
			Assert.AreEqual("85207", nonGAReasonTypes[18].ZZD_Code);
			Assert.AreEqual("생물다양성 보전 및 이용에 관한 법률 제11조 제2항에 따른 농업생명자원의 보존ㆍ관리 및 이용에 관한 법률 제18조 제1항에 따른 국외반출승인을 받은 경우", nonGAReasonTypes[18].ZZD_Description);
			Assert.AreEqual("85208", nonGAReasonTypes[19].ZZD_Code);
			Assert.AreEqual("생물다양성 보전 및 이용에 관한 법률 제11조 제2항에 따른 해양수산생명자원의 확보ㆍ관리 및 이용 등에 관한 법률 제22조 제1항에 따른 국외반출승인을 받은 경우", nonGAReasonTypes[19].ZZD_Description);
			Assert.AreEqual("85901", nonGAReasonTypes[20].ZZD_Code);
			Assert.AreEqual("기타 세관장확인 수출요건 비대상 등 사유를 기재", nonGAReasonTypes[20].ZZD_Description);

			Assert.AreEqual("86101", nonGAReasonTypes[21].ZZD_Code);
			Assert.AreEqual("생활주변방사선 안전관리법 제2조 제1호에 따른 원자력안전법에 따라 관리되는 핵물질에서 방출되는 방사선은 제외", nonGAReasonTypes[21].ZZD_Description);
			Assert.AreEqual("86401", nonGAReasonTypes[22].ZZD_Code);
			Assert.AreEqual("관세법 제226조에 따른 세관장확인물품 및 확인방법 지정고시 제7조 제2항 제1호에 따라 대외무역법 시행령 제19조에 따른 수출승인면제물품", nonGAReasonTypes[22].ZZD_Description);
			Assert.AreEqual("86901", nonGAReasonTypes[23].ZZD_Code);
			Assert.AreEqual("기타 세관장확인 수출요건 비대상 등 사유를 기재", nonGAReasonTypes[23].ZZD_Description);

			Assert.AreEqual("71101", nonGAReasonTypes[24].ZZD_Code);
			Assert.AreEqual("야생생물 보호 및 관리에 관한 법률 제21조 제2항 제1호에 따른 문화재보호법 제25조에 따른 천연기념물로 같은 법에 따라 허가를 받은 경우", nonGAReasonTypes[24].ZZD_Description);
			Assert.AreEqual("71102", nonGAReasonTypes[25].ZZD_Code);
			Assert.AreEqual("야생생물 보호 및 관리에 관한 법률 제21조 제2항 제3호에 따른 생물다양성 보전 및 이용에 관한 법률 제11조에 따라 환경부장관이 지정ㆍ고시하는 생물자원을 수출하거나 반출하려는 경우", nonGAReasonTypes[25].ZZD_Description);
			Assert.AreEqual("71103", nonGAReasonTypes[26].ZZD_Code);
			Assert.AreEqual("야생생물 보호 및 관리에 관한 법률 제14제 제3항 제3호에 따른 문화재보호법 제25조에 따른 천연기념물로 같은 법에 따라 허가를 받은경우", nonGAReasonTypes[26].ZZD_Description);
			Assert.AreEqual("71201", nonGAReasonTypes[27].ZZD_Code);
			Assert.AreEqual("야생생물 보호 및 관리에 관한 법률 제16조 제1항에 따른 국제적 멸종위기종을 이용한 가공품으로서 약사법에 따른 수출허가를 받은 의약품", nonGAReasonTypes[27].ZZD_Description);
			Assert.AreEqual("71202", nonGAReasonTypes[28].ZZD_Code);
			Assert.AreEqual("야생생물법제16조제1항, 동법시행령제13조제1호에 따른 국제거래 과정에서 세관의 관할하에 영토를 경유하거나 영토 안에서 환적換積, 관세법 제2조 제14호에 따른 환적을 말한다되는 생물 및 그 가공품", nonGAReasonTypes[28].ZZD_Description);
			Assert.AreEqual("71203", nonGAReasonTypes[29].ZZD_Code);
			Assert.AreEqual("야생생물법제16조제1항, 동법시행령제13조제2호에 따른 환경부장관이 야생생법시행규칙으로 정하는 바에 따라 멸종위기종국제거래협약이 적용되기 전에 획득하였다는 증명서를 발급한 생물 및 그 가공품", nonGAReasonTypes[29].ZZD_Description);
			Assert.AreEqual("71204", nonGAReasonTypes[30].ZZD_Code);
			Assert.AreEqual("야생생물 보호 및 관리에 관한 법률 제16조 제1항, 동법 시행령 제13조 제3호에 따른 개인의 휴대품 또는 가재도구로서 합법적으로 취득한 것임을 증명할 수 있는 생물 및 그 가공품", nonGAReasonTypes[30].ZZD_Description);
			Assert.AreEqual("71205", nonGAReasonTypes[31].ZZD_Code);
			Assert.AreEqual("야생생물법제16조제1항, 동법시행령제13조제4호에 따른 멸종위기종국제거래협약 사무국에 등록된 과학기관 사이에 비상업적으로 대여, 증여 또는 교환되는 식물표본, 보존 처리된 동물표본 및 살아있는 식물", nonGAReasonTypes[31].ZZD_Description);
			Assert.AreEqual("71206", nonGAReasonTypes[32].ZZD_Code);
			Assert.AreEqual("야생생물법제16조제1항, 동법시행령제13조제5호에 따른 국제적 멸종위기종으로 제작된 악기로서 환경부장관이 야생생물법시행규칙으로 정하는 바에 따라 악기인증서를 발급한 악기비상업적 목적으로 반출하는 경우로 한정", nonGAReasonTypes[32].ZZD_Description);
			Assert.AreEqual("71901", nonGAReasonTypes[33].ZZD_Code);
			Assert.AreEqual("기타 세관장확인 수출요건 비대상 등 사유를 기재", nonGAReasonTypes[33].ZZD_Description);

			Assert.AreEqual("32101", nonGAReasonTypes[34].ZZD_Code);
			Assert.AreEqual("외국환거래법 제3조 제1항에 따른 외국환거래법 제3조의 지급수단 또는 증권에 해당하지 않는 경우", nonGAReasonTypes[34].ZZD_Description);
			Assert.AreEqual("32401", nonGAReasonTypes[35].ZZD_Code);
			Assert.AreEqual("관세법 제226조에 따른 세관장확인물품 및 확인방법 지정고시 제7조 제2항 제1호에 따라 대외무역법 시행령 제19조에 따른 수출승인면제물품", nonGAReasonTypes[35].ZZD_Description);
			Assert.AreEqual("32201", nonGAReasonTypes[36].ZZD_Code);
			Assert.AreEqual("외국환거래규정제6-2조제1항제3호에 따른 미화1만불이하 지급수단, 대외지급수단, 내국통화, 원화표시자기앞수표, 원화표시여행자수표 및 외국환거래규정제6-2조제3항의규정에서 정한 절차 거친 대외지급수단을 수출하는 경우", nonGAReasonTypes[36].ZZD_Description);
			Assert.AreEqual("32202", nonGAReasonTypes[37].ZZD_Code);
			Assert.AreEqual("외국환거래규정 제6-2조 제1항 제4호에 따른 영 제10조제2항제1호, 제2호 및 제6호가목 및 나목에 해당하는 자가 대외지급수단을 수출하는 경우", nonGAReasonTypes[37].ZZD_Description);
			Assert.AreEqual("32203", nonGAReasonTypes[38].ZZD_Code);
			Assert.AreEqual("외국환거래규정 제6-2조 제1항 제5호 가목에 따른 제5-11조의 규정에 의하여 인정된 대외지급수단을 수출하는 경우", nonGAReasonTypes[38].ZZD_Description);
			Assert.AreEqual("32204", nonGAReasonTypes[39].ZZD_Code);
			Assert.AreEqual("외국환거래규정 제6-2조 제1항 제5호 나목(1)에 따른 인정된 거래에 따른 대외지급을 위하여 송금수표 또는 우편환을 수출하는 경우", nonGAReasonTypes[39].ZZD_Description);
			Assert.AreEqual("32205", nonGAReasonTypes[40].ZZD_Code);
			Assert.AreEqual("외국환거래규정 제6-2조 제1항 제5호 나목(2)에 따른 최근 입국 시 휴대수입한 범위내 또는 국내에서 인정된 거래에 의하여 취득한 대외지급수단을 수출하는 경우", nonGAReasonTypes[40].ZZD_Description);
			Assert.AreEqual("32206", nonGAReasonTypes[41].ZZD_Code);
			Assert.AreEqual("외국환거래규정 제6-2조 제1항 제5호 나목(3)에 따른 이 법의 적용을 받지 않는 거래에 의하여 취득한 채권을 처분하고자 발행한 수표를 수출하는 경우", nonGAReasonTypes[41].ZZD_Description);
			Assert.AreEqual("32207", nonGAReasonTypes[42].ZZD_Code);
			Assert.AreEqual("외국환거래규정제6-2조제1항제5호나목4에 따른 주한미군 및 이에 준하는 국제연합군이 근무고용에 따라 취득하거나 외국의 원천으로부터 취득한대외지급수단 또는 당해 국가의 공금인 대외지급수단을 수출하는 경우", nonGAReasonTypes[42].ZZD_Description);
			Assert.AreEqual("32208", nonGAReasonTypes[43].ZZD_Code);
			Assert.AreEqual("외국환거래규정 제6-2조 제1항 제5호 다목에 따른 외국인거주자가 이 법의 적용을 받지 않는 거래에 의하여 취득한 대외지급수단을 수출하는 경우", nonGAReasonTypes[43].ZZD_Description);
			Assert.AreEqual("32209", nonGAReasonTypes[44].ZZD_Code);
			Assert.AreEqual("외국환거래규정 제6-2조 제1항 제5호 라목(1)에 따른 수출물품에 포함 또는 가공되어 대외무역법에서 정하는 바에 의해 내국지급수단을 수출하는 경우", nonGAReasonTypes[44].ZZD_Description);
			Assert.AreEqual("32210", nonGAReasonTypes[45].ZZD_Code);
			Assert.AreEqual("외국환거래규정 제6-2조 제1항 제5호 라목(2)에 따른 비거주자가 입국 시 휴대수입하거나 국내에서 매입한 원화표시여행자수표를 수출하는 경우", nonGAReasonTypes[45].ZZD_Description);
			Assert.AreEqual("32211", nonGAReasonTypes[46].ZZD_Code);
			Assert.AreEqual("외국환거래규정 제6-2조 제1항 제6호에 따른 외국환은행이 외국환은행해외지점, 외국환은행현지법인 또는 외국금융기관(외국환전영업자를 포함한다)과 내국통화를 수출하는경우", nonGAReasonTypes[46].ZZD_Description);
			Assert.AreEqual("32212", nonGAReasonTypes[47].ZZD_Code);
			Assert.AreEqual("외국환거래규정 제6-2조 제1항 제7호 나목(1)에 따른 자본거래의 신고를 한 자가 신고한 바에 따라 기명식증권을 수출하는 경우", nonGAReasonTypes[47].ZZD_Description);
			Assert.AreEqual("32213", nonGAReasonTypes[48].ZZD_Code);
			Assert.AreEqual("외국환거래규정 제6-2조 제1항 제7호 나목(2)에 따른 외국인투자촉진법에 의하여 취득한 기명식증권을 수출하는 경우", nonGAReasonTypes[48].ZZD_Description);
			Assert.AreEqual("32214", nonGAReasonTypes[49].ZZD_Code);
			Assert.AreEqual("외국환거래규정 제6-2조 제1항 제7호 나목(3)에 따른 제7-31조 제1항 제10호의 규정에 의하여 거주자가 취득한 본사의 주식이나 국제수익증권 등을 수출하는 경우", nonGAReasonTypes[49].ZZD_Description);
			Assert.AreEqual("32215", nonGAReasonTypes[50].ZZD_Code);
			Assert.AreEqual("외국환거래규정제6-2조제1항제7호다목에 따른 거주자가 미화5만불 상당액이내의 외국통화 또는 내국통화를 지급수단으로 사용하지 않고 수집용ㆍ기념용ㆍ시험용ㆍ외국전시용 또는 화폐수집가 등 판매 위해 수입하는 경우", nonGAReasonTypes[50].ZZD_Description);
			Assert.AreEqual("32216", nonGAReasonTypes[51].ZZD_Code);
			Assert.AreEqual("외국환거래규정 제6-2조 제1항 제7호 라목에 따른 한국은행ㆍ외국환은행 또는 체신관서가 인정된 업무를 영위함에 있어 대외지급수단을 수출하는 경우", nonGAReasonTypes[51].ZZD_Description);
			Assert.AreEqual("32901", nonGAReasonTypes[52].ZZD_Code);
			Assert.AreEqual("기타 세관장확인 수출요건 비대상 등 사유를 기재", nonGAReasonTypes[52].ZZD_Description);

			Assert.AreEqual("53101", nonGAReasonTypes[53].ZZD_Code);
			Assert.AreEqual("원자력안전법 제2조에 따른 핵물질(핵연료물질, 핵원료물질), 방사성동위원소, 방사선발생장치, 방사선동위원소가 내장된 기기에 해당하지 않음", nonGAReasonTypes[53].ZZD_Description);
			Assert.AreEqual("53102", nonGAReasonTypes[54].ZZD_Code);
			Assert.AreEqual("원자력안전법 제2조 제6호, 동법 시행령 제5조 제3호, 방사성동위원소에서 제외되는 물질 등에 관한 규정 제1조에 따른 방사성물질 또는 이를 내장한 장치 중 방사선장해의 우려가 없는 것으로서 위원회가 정하여 고시하는 것", nonGAReasonTypes[54].ZZD_Description);
			Assert.AreEqual("53103", nonGAReasonTypes[55].ZZD_Code);
			Assert.AreEqual("원자력안전법 제2조 제9호, 동법 시행령 제8조, 방사선발생장치에서 제외되는 용도 및 용량 등에 관한 규정에 따른 방사선발생장치 중 위원회가 정하는 용도의 것과 위원회가 정하는 용량 이하의 것", nonGAReasonTypes[55].ZZD_Description);
			Assert.AreEqual("53104", nonGAReasonTypes[56].ZZD_Code);
			Assert.AreEqual("원자력안전법 제107조 및 핵물질 수출입요건확인 요령 제2조에 따른 핵물질 수출입요건확인요령 제2조 각 호의 어느 하나에 해당하는 경우로서 대외무역법 제19조에 따른 전략물자 수출허가를 받은 핵물질을 수출하는 경우", nonGAReasonTypes[56].ZZD_Description);
			Assert.AreEqual("53401", nonGAReasonTypes[57].ZZD_Code);
			Assert.AreEqual("관세법 제226조에 따른 세관장확인물품 및 확인방법 지정고시 제7조 제2항 제1호에 따라 대외무역법 시행령 제19조에 따른 수출승인면제물품", nonGAReasonTypes[57].ZZD_Description);
			Assert.AreEqual("53201", nonGAReasonTypes[58].ZZD_Code);
			Assert.AreEqual("원자력안전법, 핵물질 수출입요건확인요령제8조제1항에 따른 다음 1,2를 모두 만족하는 경우(1. 사용허가 면제대상이거나 사용신고면제대상인 경우와 2. 보고 면제대상인 경우)", nonGAReasonTypes[58].ZZD_Description);
			Assert.AreEqual(1, nonGAReasonTypes[58].RefCusCodeListAttributes.Length);
			Assert.AreEqual("핵물질 수출입요건확인면제(신청)서", nonGAReasonTypes[58].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("53901", nonGAReasonTypes[59].ZZD_Code);
			Assert.AreEqual("기타 세관장확인 수출요건 비대상 등 사유를 기재", nonGAReasonTypes[59].ZZD_Description);

			Assert.AreEqual("55101", nonGAReasonTypes[60].ZZD_Code);
			Assert.AreEqual("총포ㆍ도검ㆍ화약류 등의 안전관리에 관한 법률 제3조 제2항에 따른 자동차에어백용 등 인체보호용 가스발생기로 총포ㆍ도검ㆍ화약류 등의 안전관리에 관한 법률 시행규칙의 안전기준에 해당하는 것", nonGAReasonTypes[60].ZZD_Description);
			Assert.AreEqual("55102", nonGAReasonTypes[61].ZZD_Code);
			Assert.AreEqual("총포ㆍ도검ㆍ화약류 등의 안전관리에 관한 법률 제3조 제3항에 따른 군수용으로 수출되는 총포ㆍ도검ㆍ화약류ㆍ분사기ㆍ전자충격기ㆍ석궁", nonGAReasonTypes[61].ZZD_Description);
			Assert.AreEqual("55901", nonGAReasonTypes[62].ZZD_Code);
			Assert.AreEqual("기타 세관장확인 수출요건 비대상 등 사유를 기재", nonGAReasonTypes[62].ZZD_Description);

			Assert.AreEqual("29101", nonGAReasonTypes[63].ZZD_Code);
			Assert.AreEqual("폐기물의 국가 간 이동 및 그 처리에 관한 법률 제3조 제1항에 따른 원자력안전법 제2조 제5호에 따른 방사성물질 및 이에 의하여 오염된 물질", nonGAReasonTypes[63].ZZD_Description);
			Assert.AreEqual("29102", nonGAReasonTypes[64].ZZD_Code);
			Assert.AreEqual("폐기물의 국가 간 이동 및 그 처리에 관한 법률 제3조 제2항에 따른 해양환경관리법 등에 따른 해역 배출폐기물과 선박 항행에 따라 배출되는 폐기물", nonGAReasonTypes[64].ZZD_Description);
			Assert.AreEqual("29201", nonGAReasonTypes[65].ZZD_Code);
			Assert.AreEqual("폐기물관리법 시행규칙에 따라 사업장폐기물 배출신고가 면제되는 폐지 및 고철비철금속 포함로 폐유 등에 오염되지 않거나, 이물질을 함유하고 있지 않은 경우에 한함", nonGAReasonTypes[65].ZZD_Description);
			Assert.AreEqual("29901", nonGAReasonTypes[66].ZZD_Code);
			Assert.AreEqual("기타 세관장확인 수출요건 비대상 등 사유를 기재", nonGAReasonTypes[66].ZZD_Description);
		}

		[Test]
		public void TestRefCusPreferenceUpdate()
		{
			var workbook = new XSSFWorkbook(Path.Combine(TestHelper.BaseRealFilePath, @"DutyRateClassificationCodes.xlsx"));
			var configFilePath = ApplicationConfig.PreferenceConfigFileInputPath;
			var configuration = EntityConfigurationManager.DeserializeXML<EntityConfiguration>(configFilePath);
			var refCusPreferences = new ExcelEntityUpdater<RefCusPreference>(configuration, null).Update(workbook);

			Assert.AreEqual(374, refCusPreferences.Count);
			var i = 0;

			Assert.AreEqual("A", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("기본세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("A1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("기본세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("B", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("잠정세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C2A1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택14)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C2A2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택15)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C2A3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택16)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C2A4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택17)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C2A5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택18)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C2A6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택19)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C2A7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택20)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C2A8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택21)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C2A9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택22)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택10)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택11)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택12)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("C6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택13)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("CIT", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("CIT1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("CIT2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("CIT3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("CIT4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("CIT5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("CIT6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO협정세율(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("D", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("WTO개도국협정세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(일반)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1A1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1A10", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택10)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1A2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1A3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1A4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1A5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1A6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1A7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1A8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1A9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1B1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1B2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1B3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1B4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1L1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1L2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1L3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1L4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1L5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1L6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E1L7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(방글라데시)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E2A1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(방글라데시)(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E2A2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(방글라데시)(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E2A3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(방글라데시)(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(라오스)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E3A1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(라오스)(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E3A2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(라오스)(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("E3A3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(라오스)(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("F", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("국제협력관세", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("F1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("국제협력관세(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAS", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA협정세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAS1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAS2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAS3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAS4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FASID", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA상호대응세율(인도네시아)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FASMM", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA상호대응세율(미얀마)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FASMY", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA상호대응세율(말레이시아)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FASPH1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA상호대응세율(필리핀1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FASPH2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA상호대응세율(필리핀2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FASPH3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA상호대응세율(필리핀3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FASTH", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA상호대응세율(태국)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FASVN1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA상호대응세율(베트남1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FASVN2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA상호대응세율(베트남2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAU1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ호주 FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAU10", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ호주 FTA협정세율(선택10)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAU11", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ호주 FTA협정세율(선택11)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAU2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ호주 FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAU3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ호주 FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAU4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ호주 FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAU5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ호주 FTA협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAU6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ호주 FTA협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAU7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ호주 FTA협정세율(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAU8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ호주 FTA협정세율(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FAU9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ호주 FTA협정세율(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCA1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캐나다 FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCA10", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캐나다 FTA협정세율(선택10)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCA11", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캐나다 FTA협정세율(선택11)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCA12", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캐나다 FTA협정세율(선택12)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCA2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캐나다 FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCA3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캐나다 FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCA4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캐나다 FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCA5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캐나다 FTA협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCA6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캐나다 FTA협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCA7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캐나다 FTA협정세율(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCA8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캐나다 FTA협정세율(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCA9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캐나다 FTA협정세율(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCECR1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_코스타리카(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCECR2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_코스타리카(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCECR3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_코스타리카(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCECR4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_코스타리카(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCECR5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_코스타리카(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCECR6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_코스타리카(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCEHN1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_온두라스(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCEHN2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_온두라스(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCEHN3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_온두라스(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCEHN4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_온두라스(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCEHN5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_온두라스(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCEHN6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_온두라스(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCEHN7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_온두라스(추천)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCENI1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_니카라과(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCENI2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_니카라과(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCENI3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_니카라과(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCENI4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_니카라과(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCENI5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_니카라과(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCENI6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_니카라과(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCENI7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_니카라과(추천)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCEPA1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_파나마(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCEPA2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_파나마(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCEPA3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_파나마(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCEPA4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_파나마(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCEPA5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_파나마(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCEPA6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_파나마(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCEPA7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_파나마(추천)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCEPA8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_파나마(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCESV1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_엘사바도르(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCESV2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_엘사바도르(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCESV3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_엘사바도르(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCESV4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_엘사바도르(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCESV5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_엘사바도르(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCESV6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_엘사바도르(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCESV7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_엘사바도르(추천)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCL", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ칠레FTA협정세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCL1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ칠레FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCL2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ칠레FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCL5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ칠레FTA협정세율(추천)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCL6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ칠레FTA협정세율(미추천)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCN1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중국 FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCN10", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중국 FTA협정세율(선택10)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCN11", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중국 FTA협정세율(선택11)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCN2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중국 FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCN3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중국 FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCN4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중국 FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCN5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중국 FTA협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCN6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중국 FTA협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCN7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중국 FTA협정세율(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCN8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중국 FTA협정세율(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCN9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ중국 FTA협정세율(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCO1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ콜롬비아FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCO2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ콜롬비아FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCO3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ콜롬비아FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCO4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ콜롬비아FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCO5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ콜롬비아FTA협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCO6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ콜롬비아FTA협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCO7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ콜롬비아FTA협정세율(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCO8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ콜롬비아FTA협정세율(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FCO9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ콜롬비아FTA협정세율(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEF1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEF2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEF3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEF4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEFCH", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEFCH1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEFCH2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEFIS", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEFIS1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택12)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEFIS2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택13)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEFNO", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEFNO1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택10)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEFNO2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택11)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEU", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEU FTA협정세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEU1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEU FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEU10", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEU FTA협정세율(선택10)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEU11", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEU FTA협정세율(선택11)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEU2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEU FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEU3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEU FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEU4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEU FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEU5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEU FTA협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEU6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEU FTA협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEU7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEU FTA협정세율(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEU8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEU FTA협정세율(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FEU9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEU FTA협정세율(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FGB1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ영국 FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FGB2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ영국 FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FGB3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ영국 FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FGB4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ영국 FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FGB5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ영국 FTA협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FGB6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ영국 FTA협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FGB7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ영국 FTA협정세율(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FGB8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ영국 FTA협정세율(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FGB9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ영국 FTA협정세율(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FID1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ인도네시아 CEPA(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FID2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ인도네시아 CEPA(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FID3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ인도네시아 CEPA(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FID4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ인도네시아 CEPA(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FID5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ인도네시아 CEPA(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FID6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ인도네시아 CEPA(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FID7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ인도네시아 CEPA(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FID8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ인도네시아 CEPA(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FID9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ인도네시아 CEPA(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FIL1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ이스라엘 FTA(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FIL2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ이스라엘 FTA(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FIL3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ이스라엘 FTA(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FIL4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ이스라엘 FTA(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FIL5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ이스라엘 FTA(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FIL6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ이스라엘 FTA(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FIL7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ이스라엘 FTA(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FIL8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ이스라엘 FTA(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FIL9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ이스라엘 FTA(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FIN", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ인도 FTA협정세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FIN1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ인도 FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FIN2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ인도 FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FIN3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ인도 FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FIN4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ인도 FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FKH1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캄보디아 FTA(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FKH2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캄보디아 FTA(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FKH3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캄보디아 FTA(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FKH4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캄보디아 FTA(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FKH5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캄보디아 FTA(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FKH6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캄보디아 FTA(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FKH7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캄보디아 FTA(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FKH8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캄보디아 FTA(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FKH9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ캄보디아 FTA(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FNZ1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ뉴질랜드 FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FNZ10", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ뉴질랜드 FTA협정세율(선택10)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FNZ11", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ뉴질랜드 FTA협정세율(선택11)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FNZ12", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ뉴질랜드 FTA협정세율(선택12)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FNZ2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ뉴질랜드 FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FNZ3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ뉴질랜드 FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FNZ4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ뉴질랜드 FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FNZ5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ뉴질랜드 FTA협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FNZ6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ뉴질랜드 FTA협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FNZ7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ뉴질랜드 FTA협정세율(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FNZ8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ뉴질랜드 FTA협정세율(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FNZ9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ뉴질랜드 FTA협정세율(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FPE1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ페루 FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FPE2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ페루 FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FPE3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ페루 FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FPE4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ페루 FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FPE5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ페루 FTA협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FPE6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ페루 FTA협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FPH1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ필리핀 FTA(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FPH2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ필리핀 FTA(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FPH3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ필리핀 FTA(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FPH4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ필리핀 FTA(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FPH5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ필리핀 FTA(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAS1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_아세안(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAS2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_아세안(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAS3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_아세안(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAS4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_아세안(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAS5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_아세안(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAS6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_아세안(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAS7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_아세안(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAS8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_아세안(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAS9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_아세안(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAU1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_호주(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAU2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_호주(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAU3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_호주(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAU4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_호주(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAU5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_호주(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAU6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_호주(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAU7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_호주(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAU8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_호주(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCAU9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_호주(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCCN1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_중국(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCCN2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_중국(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCCN3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_중국(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCCN4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_중국(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCCN5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_중국(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCCN6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_중국(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCCN7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_중국(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCCN8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_중국(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCCN9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_중국(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCJP1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_일본(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCJP2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_일본(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCJP3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_일본(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCJP4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_일본(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCJP5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_일본(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCJP6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_일본(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCJP7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_일본(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCJP8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_일본(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCJP9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_일본(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCNZ1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_뉴질랜드(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCNZ2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_뉴질랜드(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCNZ3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_뉴질랜드(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCNZ4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_뉴질랜드(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCNZ5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_뉴질랜드(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCNZ6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_뉴질랜드(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCNZ7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_뉴질랜드(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCNZ8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_뉴질랜드(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FRCNZ9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("RCEP협정세율_뉴질랜드(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FSG", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ싱가포르FTA협정세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FSG1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ싱가포르FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FSG2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ싱가포르FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FSG3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ싱가포르FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FSG4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ싱가포르FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FSG5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ싱가포르FTA협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTA", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA협정세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTA1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTA2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTA3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTA4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ아세안 FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTE1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTE2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTE3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTE4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTE5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTE6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTE7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍEFTA FTA협정세율(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTR1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ터키 FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTR2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ터키 FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTR3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ터키 FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTR4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ터키 FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTR5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ터키 FTA협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTR6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ터키 FTA협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTR7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ터키 FTA협정세율(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTR8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ터키 FTA협정세율(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FTR9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ터키 FTA협정세율(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FUS", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ미 FTA 협정세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FUS1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ미 FTA 협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FUS10", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ미 FTA 협정세율(선택10)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FUS11", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ미 FTA 협정세율(선택11)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FUS12", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ미 FTA 협정세율(선택12)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FUS2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ미 FTA 협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FUS3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ미 FTA 협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FUS4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ미 FTA 협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FUS5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ미 FTA 협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FUS6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ미 FTA 협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FUS7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ미 FTA 협정세율(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FUS8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ미 FTA 협정세율(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FUS9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ미 FTA 협정세율(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FVN1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ베트남 FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FVN10", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ베트남 FTA협정세율(선택10)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FVN2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ베트남 FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FVN3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ베트남 FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FVN4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ베트남 FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FVN5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ베트남 FTA협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FVN6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ베트남 FTA협정세율(선택6)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FVN7", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ베트남 FTA협정세율(선택7)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FVN8", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ베트남 FTA협정세율(선택8)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("FVN9", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ베트남 FTA협정세율(선택9)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("G1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("UN개도국협정세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("G2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("UN개도국협정세율(최빈국)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("H", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한중마늘 합의세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("I", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("덤핑방지관세", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("J", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("보복관세", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("K", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("긴급관세", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("L", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("조정관세", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("M", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("상계관세", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("N", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("편익관세", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("O", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("물가평형관세", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("P1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("할당관세(추천)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("P2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("할당예외사항관세(추천)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("P3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("할당관세(미추천)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("P4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("할당예외관세(미추천)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("Q", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("환특세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("R", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("최빈국특혜관세", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("S", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("쌍무협정세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("T1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("특별긴급관세(물량기준)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("T2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("특별긴급관세(가격기준)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("U", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("북한산", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("W1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("농림축산물양허관세(추천)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("W2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("농림축산물양허관세(미추천)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("W3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("농림축산물양허관세(증량분)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("X", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("간이세율", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("Y1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ칠레FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("Y2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ칠레FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("Y5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ칠레FTA협정세율(추천)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("Y6", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ칠레FTA협정세율(미추천)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("Z", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한,싱가폴FTA", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("Z1", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ싱가포르FTA협정세율(선택1)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("Z2", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ싱가포르FTA협정세율(선택2)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("Z3", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ싱가포르FTA협정세율(선택3)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("Z4", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ싱가포르FTA협정세율(선택4)", refCusPreferences[i++].ZZS_Description);
			Assert.AreEqual("Z5", refCusPreferences[i].ZZS_Preference);
			Assert.AreEqual("한ㆍ싱가포르FTA협정세율(선택5)", refCusPreferences[i++].ZZS_Description);
		}

		[Test]
		public void TestExportFTATypeUpdate()
		{
			IWorkbook workbook;
			string dataFile = Path.Combine(TestHelper.BaseRealFilePath, "ExportFTATypeDataFile.xlsx");
			using (var file = new FileStream(dataFile, FileMode.Open, FileAccess.Read))
			{
				workbook = new XSSFWorkbook(file);
			}
			var configFilePath = ApplicationConfig.ExportFTATypeConfigFilePath;
			var configuration = EntityConfigurationManager.DeserializeXML<EntityConfiguration>(configFilePath);
			var exportFTATypes = new ExcelEntityUpdater<RefCusCodeList>(configuration, new ExportFTATypeDataUpdater()).Update(workbook);

			Assert.AreEqual(21, exportFTATypes.Count);

			int index = 0;
			Assert.AreEqual("101", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-칠레", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("CL", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 1;
			Assert.AreEqual("102", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-싱가포르", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("SG", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 2;
			Assert.AreEqual("103", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-EFTA", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("EFTA", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 3;
			Assert.AreEqual("104", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-아세안", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("ASEAN", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 4;
			Assert.AreEqual("105", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-인도", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("IN", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 5;
			Assert.AreEqual("106", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-EU", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("EU", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 6;
			Assert.AreEqual("107", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-페루", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("PE", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 7;
			Assert.AreEqual("108", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-미국", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("US", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 8;
			Assert.AreEqual("109", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-터키", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("TR", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 9;
			Assert.AreEqual("110", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-호주", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("AU", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 10;
			Assert.AreEqual("111", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-캐나다", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("CA", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 11;
			Assert.AreEqual("112", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-중국", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("CN", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 12;
			Assert.AreEqual("113", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-베트남", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("VN", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 13;
			Assert.AreEqual("114", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-뉴질랜드", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("NZ", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 14;
			Assert.AreEqual("115", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-콜롬비아", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("CO", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 15;
			Assert.AreEqual("116", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-영국", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("GB", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 16;
			Assert.AreEqual("117", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-중미", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("CE", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 17;
			Assert.AreEqual("118", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-인도네시아", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("ID", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 18;
			Assert.AreEqual("119", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("RCEP", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("RCEP", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 19;
			Assert.AreEqual("120", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-이스라엘", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("IL", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);

			index = 20;
			Assert.AreEqual("125", exportFTATypes[index].ZZD_Code);
			Assert.AreEqual("한-캄보디아", exportFTATypes[index].ZZD_Description);
			Assert.AreEqual("EXFTA", exportFTATypes[index].ZZD_ZZK_NKCodeType);
			Assert.AreEqual(new DateTime(1900, 01, 01), exportFTATypes[index].ZZD_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), exportFTATypes[index].ZZD_EndDate);
			Assert.AreEqual("KH", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_Value);
			Assert.AreEqual("FTATradeGroup", exportFTATypes[index].RefCusCodeListAttributes[0].ZZE_ZXE_NKName);
		}

		[Test]
		public void TestRefCusTradeGroupUpdate()
		{
			IWorkbook workbook;
			string dataFile = Path.Combine(TestHelper.BaseRealFilePath, @"TradeGroupAndCountry.xlsx");
			using (var file = new FileStream(dataFile, FileMode.Open, FileAccess.Read))
			{
				workbook = new XSSFWorkbook(file);
			}
			var configFilePath = ApplicationConfig.TradeGroupConfigFilePath;
			var configuration = EntityConfigurationManager.DeserializeXML<EntityConfiguration>(configFilePath);
			var refCusTradeGroup = new ExcelEntityUpdater<RefCusTradeGroup>(configuration, null, new RefCusTradeGroupLookupManager()).Update(workbook);
			Assert.AreEqual(44, refCusTradeGroup.Count);
			int index = 0;
			Assert.AreEqual("ALL", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("모든국가 대상", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(253, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("AD", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 1;
			Assert.AreEqual("APTA", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(일반)", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(6, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("BD", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 2;
			Assert.AreEqual("ASEAN", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ아세안 FTA협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(10, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("BN", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 3;
			Assert.AreEqual("AU", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ호주 FTA협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("AU", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 4;
			Assert.AreEqual("BD", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(방글라데시)", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("BD", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 5;
			Assert.AreEqual("CA", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ캐나다 FTA협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("CA", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 6;
			Assert.AreEqual("CEPA", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ인도네시아 CEPA", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("ID", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 7;
			Assert.AreEqual("CL", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ칠레FTA협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("CL", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 8;
			Assert.AreEqual("CN", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ중국 FTA협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("CN", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 9;
			Assert.AreEqual("CO", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ콜롬비아FTA협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("CO", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 10;
			Assert.AreEqual("CR", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_코스타리카", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("CR", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 11;
			Assert.AreEqual("EFTA", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍEFTA FTA협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(4, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("CH", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 12;
			Assert.AreEqual("EU", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍEU FTA협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(27, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("AT", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 13;
			Assert.AreEqual("GB", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ영국 FTA협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("GB", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 14;
			Assert.AreEqual("GCC", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("페르시아만안협력회의(Gulf Cooperation Council)", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(2079, 06, 06), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(6, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("BH", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 15;
			Assert.AreEqual("GSP", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("일반특혜관세제도(Generalized System of Preferences)", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(4, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("AU", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 16;
			Assert.AreEqual("GSTP", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("개발도상국 간 특혜무역 제도(Global System of Trade Preferences Among Developing Countries)", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(43, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("AR", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 17;
			Assert.AreEqual("HN", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_온두라스", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("HN", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 18;
			Assert.AreEqual("ID", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ아세안 FTA상호대응세율(인도네시아)", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("ID", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 19;
			Assert.AreEqual("IL", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ이스라엘 FTA", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("IL", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 20;
			Assert.AreEqual("IN", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ인도 FTA협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("IN", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 21;
			Assert.AreEqual("JP", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("RCEP협정세율_일본", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("JP", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 22;
			Assert.AreEqual("KH", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ캄보디아 FTA", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("KH", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 23;
			Assert.AreEqual("KP", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("북한산", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("KP", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 24;
			Assert.AreEqual("LA", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("아시아ㆍ태평양 협정세율(라오스)", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("LA", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 25;
			Assert.AreEqual("LDC", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("최빈국특혜관세", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(47, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("AF", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 26;
			Assert.AreEqual("MERCOSUR", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("브라질, 아르헨티나, 우루과이, 파라과이 등 남미 4개국 공동시장", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(2079, 06, 06), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(4, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("BR", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 27;
			Assert.AreEqual("MM", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ아세안 FTA상호대응세율(미얀마)", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("MM", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 28;
			Assert.AreEqual("MY", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ아세안 FTA상호대응세율(말레이시아)", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("MY", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 29;
			Assert.AreEqual("NI", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_니카라과", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("NI", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 30;
			Assert.AreEqual("NZ", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ뉴질랜드 FTA협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("NZ", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 31;
			Assert.AreEqual("PA", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_파나마", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("PA", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 32;
			Assert.AreEqual("PE", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ페루 FTA협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("PE", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 33;
			Assert.AreEqual("PH", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ필리핀 FTA세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("PH", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 34;
			Assert.AreEqual("RCEP", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("RCEP협정세율_아세안", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(14, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("AU", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 35;
			Assert.AreEqual("RCEPAS", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("RCEP협정세율_아세안(RCEP중 호주,중국,일본,뉴질랜드 제외)", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(10, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("BN", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 36;
			Assert.AreEqual("SG", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ싱가포르FTA협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("SG", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 37;
			Assert.AreEqual("SV", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ중미 FTA협정세율_엘사바도르", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("SV", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 38;
			Assert.AreEqual("TH", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ아세안 FTA상호대응세율(태국)", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("TH", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 39;
			Assert.AreEqual("TNDC", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("WTO개도국협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(12, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("BD", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 40;
			Assert.AreEqual("TR", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ터키 FTA협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("TR", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 41;
			Assert.AreEqual("US", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ미 FTA 협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("US", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 42;
			Assert.AreEqual("VN", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("한ㆍ아세안 FTA상호대응세율(베트남)", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(1, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("VN", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);

			index = 43;
			Assert.AreEqual("WTO", refCusTradeGroup[index].ZZA_TradeGroup);
			Assert.AreEqual("WTO협정세율", refCusTradeGroup[index].ZZA_Description);
			Assert.AreEqual(new DateTime(1900, 01, 01), refCusTradeGroup[index].ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusTradeGroup[index].ZZA_EndDate);
			Assert.AreEqual(167, refCusTradeGroup[index].RefCusTradeGroupCountries.Length);
			Assert.AreEqual("AE", refCusTradeGroup[index].RefCusTradeGroupCountries[0].ZZB_RN_NKTradeGroupCountryCode);
		}
	}
}
