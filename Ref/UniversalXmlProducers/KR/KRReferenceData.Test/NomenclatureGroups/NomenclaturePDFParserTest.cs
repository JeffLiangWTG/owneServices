using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class NomenclaturePDFParserTest
	{
		[Test]
		public void TestSectionAndChapterDescriptions()
		{
			var inputDataPath = Path.Combine(TestHelper.BaseTestFilePath, @"WCOCopiedNomenclatures\Input\2017Edition\HSKSample_2021.pdf");
			var nomenclatures = PrepareNomanclatureData();
			new NomenclaturePDFParser(nomenclatures).Update(inputDataPath);
			for (var index = 0; index < nomenclatures.Count; index++)
			{
				Assert.AreEqual(1, nomenclatures[index].RefCusNomenclatureLanguages.Length);
				Assert.AreEqual(expectedKRDescriptions[index], nomenclatures[index].RefCusNomenclatureLanguages[0].ZX8_Description);
			}
		}

		List<RefCusNomenclatureGroup> PrepareNomanclatureData()
		{
			var result = new List<RefCusNomenclatureGroup>();
			foreach (var item in testData)
			{
				var nomenclature = new RefCusNomenclatureGroup
				{
					ZZ5_Value = item.Item1,
					ZZ5_Description = item.Item2,
					ZZ5_CompositeKey = item.Item3,
					ZZ5_StartDate = new DateTime(2022, 1, 1),
					ZZ5_EndDate = new DateTime(2079, 06, 06),
					ZZ5_ZZZ_NKDataGrouping = "KR",
					ZZ5_ZZ9_NKNomenclatureGroupType = "KR"
				};
				result.Add(nomenclature);	
			}
			return result;
		}

		List<Tuple<string, string, string>> testData = new List<Tuple<string, string, string>>() {
			new Tuple<string, string, string>("01", "LIVE ANIMALS; ANIMAL PRODUCTS", "01"),
			new Tuple<string, string, string>("01", "Live animals", "01.01"),
			new Tuple<string, string, string>("03", "FISH AND CRUSTACEANS, MOLLUSCS AND OTHER AQUATIC INVERTEBRATES", "01.03"),

			new Tuple<string, string, string>("02", "VEGETABLE PRODUCTS", "02"),
			new Tuple<string, string, string>("07", "EDIBLE VEGETABLES AND CERTAIN ROOTS AND TUBERS", "02.07"),

			new Tuple<string, string, string>("06", "PRODUCTS OF THE CHEMICAL OR ALLIED INDUSTRIES", "06"),
			new Tuple<string, string, string>("29", "Organic chemicals", "06.29"),
			new Tuple<string, string, string>("II", "II.- ALCOHOLS AND THEIR HALOGENATED, SULPHONATED, NITRATED OR NITROSATED DERIVATIVES", "06.29.02"),
			new Tuple<string, string, string>("XI", "XI.- PROVITAMINS, VITAMINS AND HORMONES", "06.29.11"),

			new Tuple<string, string, string>("07", "PLASTICS AND ARTICLES THEREOF; RUBBER AND ARTICLES THEREOF", "07"),
			new Tuple<string, string, string>("39", "Plastics and articles thereof", "07.39"),
			new Tuple<string, string, string>("II", "II.- WASTE, PARINGS AND SCRAP; SEMI-MANUFACTURES; ARTICLES", "07.39.02"),

			new Tuple<string, string, string>("II", "II.- SETS", "11.63.02"),
			new Tuple<string, string, string>("III", "III.- WORN CLOTHING AND WORN TEXTILE ARTICLES; RAGS", "11.63.03"),
			new Tuple<string, string, string>("12", "FOOTWEAR, HEADGEAR, UMBRELLAS, SUN UMBRELLAS, WALKING-STICKS, SEAT-STICKS, WHIPS, RIDING-CROPS AND PARTS THEREOF; PREPARED FEATHERS AND ARTICLES MADE THEREWITH; ARTIFICIAL FLOWERS; ARTICLES OF HUMAN HAIR", "12"),
			new Tuple<string, string, string>("64", "Footwear, gaiters and the like; parts of such articles", "12.64"),

			new Tuple<string, string, string>("13", "ARTICLES OF STONE, PLASTER, CEMENT, ASBESTOS, MICA OR SIMILAR MATERIALS; CERAMIC PRODUCTS; GLASS AND GLASSWARE", "13"),
			new Tuple<string, string, string>("68", "Articles of stone, plaster, cement, asbestos, mica or similar materials", "13.68"),
			new Tuple<string, string, string>("69", "Ceramic products", "13.69"),
			new Tuple<string, string, string>("I", "I.- GOODS OF SILICEOUS FOSSIL MEALS OR OF SIMILAR SILICEOUS EARTHS, AND REFRACTORY GOODS", "13.69.01"),
			new Tuple<string, string, string>("II", "II.- OTHER CERAMIC PRODUCTS", "13.69.02"),
	};

		List<string> expectedKRDescriptions = new List<string>
		{
			"살아 있는 동물과 동물성 생산품",
			"살아 있는 동물",
			"어류ㆍ갑각류ㆍ연체동물과 그 밖의 수생(水生) 무척추 동물",

			"식물성 생산품",
			"식용의 채소ㆍ뿌리ㆍ괴경(塊莖)",

			"화학공업이나 연관공업의 생산품",
			"유기화학품",
			"알코올과 이들의 할로겐화유도체ㆍ술폰화유도체ㆍ니트 로화유도체ㆍ니트로소화유도체",
			"프로비타민ㆍ비타민ㆍ호르몬",

			"플라스틱과 그 제품, 고무와 그 제품",
			"플라스틱과 그 제품",
			"웨이스트(waste)ㆍ페어링(paring)ㆍ스크랩(scrap)과 반제품ㆍ완제품",

			"세트",
			"사용하던 의류ㆍ방직용 섬유제품, 넝마",
			"신발류ㆍ모자류ㆍ산류(傘類)ㆍ지팡이ㆍ시트스틱 (seat-stick)ㆍ채찍ㆍ승마용 채찍과 이들의 부분품, 조제 깃털 과 그 제품, 조화, 사람 머리카락으로 된 제품",
			"신발류ㆍ각반과 이와 유사한 것, 이들의 부분품",

			"돌ㆍ플라스터(plaster)ㆍ시멘트ㆍ석면ㆍ운모나 이와 유사한 재료의 제품, 도자제품, 유리와 유리제품",
			"돌ㆍ플라스터(plaster)ㆍ시멘트ㆍ석면ㆍ운모나 이와 유사한 재료의 제품",
			"도자제품",
			"규산질의 화석 가루나 이와 유사한 규산질의 흙으로 만 든 제품과 내화제품",
			"그 밖의 도자제품"
		};
	}
}
