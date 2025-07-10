using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class AdditionalCodesParserFixture
	{
		[Test]
		public void TestCreateRefCusCodeListXML_EmptyData()
		{
			var processingErrors = additionalCodesParser.ConvertToRefCusCodeListXML(new List<AdditionalCodesData>()).processingErrors;
			Assert.That(processingErrors, Does.Contain("AdditionalCodesData is empty => Nothing to import."));
		}

		[Test]
		public void TestCreateRefCusCodeListXML_BoundaryDatesCorrectlyFormatted()
		{
			var additionalCodesData = GetTestData("ABCD", new Dictionary<string, string> { { DefaultLanguage, "Bla" } }, new DateTime(2014, 02, 18, 00, 00, 00), new DateTime(2079, 06, 06, 23, 59, 00));
			var processingErrors = additionalCodesParser.ConvertToRefCusCodeListXML(additionalCodesData).processingErrors;
			Assert.That(processingErrors, Is.EqualTo(string.Empty));
		}

		[Test]
		public void TestCreateRefCusCodeListXML_EndDateBadlyFormatted()
		{
			var additionalCodesData = GetTestData("ABCD", new Dictionary<string, string> { { DefaultLanguage, "Bla" } }, new DateTime(2014, 02, 18, 00, 00, 00), null);
			var processingErrors = additionalCodesParser.ConvertToRefCusCodeListXML(additionalCodesData).processingErrors;
			var expectedError =
$@"Unable to import record from AdditionalCodes into RefCusCodeList.
DETAILS:
Code: ABCD
Has English record: True
English Description: Bla
Start Date: 18-02-2014
End Date: Badly formatted";
			Assert.That(processingErrors, Does.Contain(expectedError));
		}

		[Test]
		public void TestCreateRefCusCodeListXML_StartDateBadlyFormatted()
		{
			var additionalCodesData = GetTestData("ABCD", new Dictionary<string, string> { { DefaultLanguage, "Bla" } }, null, new DateTime(2079, 06, 06, 23, 59, 00));
			var processingErrors = additionalCodesParser.ConvertToRefCusCodeListXML(additionalCodesData).processingErrors;
			var expectedError =
$@"Unable to import record from AdditionalCodes into RefCusCodeList.
DETAILS:
Code: ABCD
Has English record: True
English Description: Bla
Start Date: Badly formatted
End Date: 06-06-2079";
			Assert.That(processingErrors, Does.Contain(expectedError));
		}

		[Test]
		public void TestCreateRefCusCodeListXML_MissingEnglishDescription()
		{
			var additionalCodesData = GetTestData("ABCD", new Dictionary<string, string> { { "BG", "Приложения от 3 до 6, Част 3, Секция II (фармацевтични субстанции) Регламент 2019/1776 (OВ L 280)" } }, new DateTime(1900, 01, 01, 00, 00, 00), new DateTime(2079, 06, 06, 23, 59, 00));

			var processingErrors = additionalCodesParser.ConvertToRefCusCodeListXML(additionalCodesData).processingErrors;
			var expectedError =
@"Unable to import record from AdditionalCodes into RefCusCodeList.
DETAILS:
Code: ABCD
Has English record: False
English Description: ";

			Assert.That(processingErrors, Does.Contain(expectedError));
		}

		[Test]
		public void TestCreateRefCusCodeListXML_TooLongCode()
		{
			const string tooLongCode = "111111111122222222223333333333444444";
			var additionalCodesData = GetTestData(tooLongCode, new Dictionary<string, string> { { DefaultLanguage, "Annex I \"Combined Nomenclature\", Part Three, Section II (pharmaceutical products), Tariff Annexes 3 to 6 – R 2019/1776 (OJ L 280)" } }, new DateTime(1900, 01, 01, 00, 00, 00), new DateTime(2079, 06, 06, 23, 59, 00));

			var processingErrors = additionalCodesParser.ConvertToRefCusCodeListXML(additionalCodesData).processingErrors;
			var expectedError =
@"Unable to import record from AdditionalCodes into RefCusCodeList.
DETAILS:
Code: 111111111122222222223333333333444444
Has English record: True
English Description: Annex I ""Combined Nomenclature"", Part Three, Section II (pharmaceutical products), Tariff Annexes 3 to 6 – R 2019/1776 (OJ L 280)";

			Assert.That(processingErrors, Does.Contain(expectedError));
		}

		[Test]
		public void TestTruncateEnglishDescription()
		{
			const string tooLongDescription = "Dongguan He Mei Ceramics Co. Ltd. Dongpeng Ceramic (Qingyuan) Co. Ltd. Eagle Brand Ceramics Industrial (Heyuan) Co. Ltd. Enping City Huachang Ceramic Co. Ltd. Enping Huiying Ceramics Industry Co. Ltd. Enping Yungo Ceramic Co. Ltd. Foshan Aoling Jinggong Ceramics Co. Ltd. Foshan ASGF Ceramics Co. Ltd. Foshan Bailifeng Building Materials Co. Ltd. Foshan Boli Import& Export Co. Ltd. Foshan Bragi Ceramic Co. Ltd. Foshan City Fangyuan Ceramic Co. Ltd. Foshan Dunhuang Building Materials Co. Ltd. Foshan Eminent Industry Development Co. Ltd. Foshan Everlasting Enterprise Co. Ltd. Foshan Gaoming Shuncheng Ceramic Co. Ltd. Foshan Gaoming Yaju Ceramics Co. Ltd. Foshan Guanzhu Ceramics Co. Ltd. Foshan Huashengchang Ceramic Co. Ltd. Foshan Huitao Economic & Trading Co. Ltd. Foshan Jiajun Ceramics Co. Ltd. Foshan Mingzhao Technology Development Co. Ltd. Foshan Nanhai Jingye Ceramics Co. Ltd. Foshan Nanhai Shengdige Decoration Material Co. Ltd. Foshan Nanhai Xiaotang Jinzun Border Factory Co. Ltd Foshan Nanhai Yonghong Ceramic Co. Ltd. Foshan Oceanland Ceramics Co. Ltd. Foshan Oceano Ceramics Co. Ltd.  Foshan Sanshui Hongyuan Ceramics Enterprise Co. Ltd. Foshan Sanshui Huiwanjia Ceramics Co. Ltd. Foshan Sanshui New Pearl Construction Ceramics Industrial Co. Ltd. Foshan Sheng Tao Fang Ceramics Co. Ltd. Foshan Shiwan Eagle Brand Ceramic Co. Ltd. Foshan Shiwan Yulong Ceramics Co. Ltd. Foshan Summit Ceramics Co. Ltd. Foshan Tidiy Ceramics Co. Ltd. Foshan VIGORBOOM Ceramic Co. Ltd. Foshan Xingtai Ceramics Co. Ltd. Foshan Yueyang Alumina Products Co. Ltd. Foshan Zhuyangyang Ceramics Co. Ltd. Fujian Fuzhou Zhongxin Ceramics Co. Ltd. Fujian Jinjiang Lianxing Building Material Co. Ltd. Fujian Minqing Jiali Ceramics Co. Ltd. Fujian Minqing Ruimei Ceramics Co. Ltd. Fujian Minqing Shuangxing Ceramics Co. Ltd. Gaoyao Yushan Ceramics Industry Co. Ltd. Guangdong Bode Fine Building Materials Co. Ltd. Guangdong Foshan Redpearl Building Material Co. Ltd. Guangdong Gold Medal Ceramics Co. Ltd. Guangdong Grifine Ceramics Co. Ltd. Guangdong Homeway Ceramics Industry Co. Ltd. Guangdong Huiya Ceramics Co. Ltd. Guangdong Juimsi Ceramics Co. Ltd. Guangdong Kaiping Tilee's Building Materials Co. Ltd. Guangdong Kingdom Ceramics Co. Ltd. Guangdong Kito Ceramics Co. Ltd. Guangdong Monalisa Ceramics Co. Ltd. Guangdong New Zhong Yuan Ceramics Co. Ltd. Shunde Yuezhong Branch Guangdong Ouya Ceramic Co. Ltd Guangdong Overland Ceramics Co. Ltd. Guangdong Qianghui (QHTC) Ceramics Co. Ltd. Guangdong Sihui Kedi Ceramics Co. Ltd. Guangdong Summit Ceramics Co. Ltd. Guangdong Tianbi Ceramics Co. Ltd. Guangdong Winto Ceramics Co. Ltd. Guangdong Xinghui Ceramics Group Co. Ltd. Guangning County Oudian Art Ceramic Co. Ltd. Guangzhou Cowin Ceramics Co. Ltd. Hangzhou Nabel Ceramics Co. Ltd. Hangzhou Nabel Group Co. Ltd. Hangzhou Venice Ceramics Co. Ltd. Heyuan Wanfeng Ceramics Co. Ltd. Hitom Ceramics Co. Ltd.  Heyuan Becarry Ceramics Co. Ltd. Huiyang Kingtile Ceramics Co. Ltd. Jiangxi Ouya Ceramics Co. Ltd. Jingdezhen Kito Ceramics Co. Ltd. Jingdezhen Lehua Ceramic Sanitary Ware Co. Ltd. Jingdezhen Tidiy Ceramics Co. Ltd. Kim Hin Ceramics (Shanghai) Co. Ltd.  Lixian Xinpeng Ceramic Co. Ltd. Louis Valentino (Inner Mongolia) Ceramic Co. Ltd. Louverenike (Foshan) Ceramics Co. Ltd. Nabel Ceramics Co. Ltd. Ordos Xinghui Ceramics Co. Ltd. Qingdao Diya Ceramics Co. Ltd. Qingyuan Guanxingwang Ceramics Co. Ltd Qingyuan Oudian Art Ceramic Co. Ltd. Qingyuan Ouya Ceramics Co. Ltd. RAK (Gaoyao) Ceramics Co. Ltd.  Shandong ASA Ceramic Co. Ltd. Shandong Dongpeng Ceramic Co. Ltd. Shandong Jialiya Ceramic Co. Ltd. Shanghai Cimic Tile Co. Ltd.  Shaoguan City Lehua Ceramic Sanitary Ware Co. Ltd. Shunde Area Foshan Lehua Ceramic Sanitary Ware Co. Ltd. Sinyih Ceramic (China) Co. Ltd.  Sinyih Ceramics (Penglai) Co. Ltd. Southern building materials and Sanitary Co. Ltd. of Qingyuan Tangshan Huida Ceramic group Co. Ltd. Tangshan Huida Ceramic Group Huiquin Co. Ltd. Tegaote Ceramics Co. Ltd Tianjin (TEDA) Honghui Industry & Trade Co. Ltd. Topbro Ceramics Co. Ltd. Xingning Christ Craftworks Co. Ltd. Zhaoqing City Shenghui Ceramics Co. Ltd. Zhaoqing Jin Ouya Ceramics Co. Ltd. Zhaoqing Lehua Ceramic Sanitary Ware Co. Ltd. Zhaoqing Zhongheng Ceramics Co. Ltd. Zibo Hualiansheng Ceramics Co. Ltd. Zibo Huaruinuo Ceramics Co. Ltd. Zibo Tongyi Ceramics Co. Ltd.";
			var additionalCodesData = GetTestData("ABCD", new Dictionary<string, string> { { DefaultLanguage, tooLongDescription } }, new DateTime(1900, 01, 01, 00, 00, 00), new DateTime(2079, 06, 06, 23, 59, 00));

			var result = additionalCodesParser.ConvertToRefCusCodeListXML(additionalCodesData).result;
			Assert.That(tooLongDescription.Length, Is.GreaterThan(2000));
			Assert.That(result.SingleOrDefault().ZZD_Description.Length, Is.EqualTo(2000));
		}

		[Test]
		public void TestDescriptionsCleanedUp()
		{
			const string expectedResult = "this; is; a; multiline; description";
			const string descripWithNewLines = "this\nis\r\na\nmultiline\rdescription";

			var additionalCodesData = GetTestData("ABCD", new Dictionary<string, string> { { DefaultLanguage, descripWithNewLines }, { "DE", descripWithNewLines } }, new DateTime(1900, 01, 01, 00, 00, 00), new DateTime(2079, 06, 06, 23, 59, 00));

			var result = additionalCodesParser.ConvertToRefCusCodeListXML(additionalCodesData).result;
			var codeList = result.Single();

			Assert.That(codeList.ZZD_Description, Is.EqualTo(expectedResult));
			Assert.That(codeList.RefCusCodeListLanguages.Single().ZXA_Description, Is.EqualTo(expectedResult));
		}

		[SetUp]
		public void Setup()
		{
			ApplicationConfig.ConfigEnvironment();
			additionalCodesParser = new AdditionalCodesParser();
		}
		const string DefaultLanguage = "EN";
		AdditionalCodesParser additionalCodesParser;

		List<AdditionalCodesData> GetTestData(string code, Dictionary<string, string> multilingualDescriptions, DateTime? startDate, DateTime? endDate)
		{
			return new List<AdditionalCodesData>
			{
				new AdditionalCodesData
				{
					Code = code,
					MultilingualDescriptions = multilingualDescriptions,
					StartDate = startDate,
					EndDate = endDate,
				}
			};
		}
	}
}
