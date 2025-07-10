using System;
using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class UtilityTest
	{
		string TaiwanCustomsUrl = AppConfig.CodeLists.Shared.BaseUrl;

		string PackingHouseListDownloadPagePath => Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/TWPKH/PackingHouseListDownloadPage.html");

		[Test]
		public void TestTrimSeconds()
		{
			var dateTime = new DateTime(2024, 06, 30, 23, 59, 59);
			Assert.AreEqual(new DateTime(2024, 06, 30, 23, 59, 0), dateTime.TrimSeconds());
		}

		[Test]
		public void TestTryConvertTaiwanDateStringToDateTime()
		{
			Assert.AreEqual(true, Utility.TryConvertTaiwanDateStringToDateTime("106.05.09", out var date));
			Assert.AreEqual(new DateTime(2017, 05, 09), date);
			Assert.AreEqual(false, Utility.TryConvertTaiwanDateStringToDateTime("", out date));
			Assert.AreEqual(default(DateTime), date);
			Assert.AreEqual(false, Utility.TryConvertTaiwanDateStringToDateTime("ABC", out date));
			Assert.AreEqual(default(DateTime), date);
			Assert.AreEqual(true, Utility.TryConvertTaiwanDateStringToDateTime("106 . 5. 9 ", out date));
			Assert.AreEqual(new DateTime(2017, 05, 09), date);
			Assert.AreEqual(false, Utility.TryConvertTaiwanDateStringToDateTime("106.0.9", out date));
			Assert.AreEqual(default(DateTime), date);
		}

		[Test]
		public void TestGetResponseString()
		{
			var expected = File.ReadAllText(PackingHouseListDownloadPagePath);
			var actual = Utility.GetResponseString(PackingHouseListDownloadPagePath);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestGetPackingHouseListFileUrlFromHtml()
		{
			var html = File.ReadAllText(PackingHouseListDownloadPagePath);
			var url = Utility.GetPackingHouseListFileUrlFromHtml(html.Replace("downloadFileNameRegexTest", AppConfig.CodeLists.TaiwanPackingHouse.Regex.DownloadFileNameRegexTest));
			Assert.AreEqual("https://www.baphiq.gov.tw/redirect_files.php?id=62093&file_name=hKIHCU0dkqicOKQlKnpSEWGEqualdHRXXS0xyRCtF6MyhDbGN3WGSlas", url);
		}

		[Test]
		public void TestGetPackingHouseListFileUrlFromHtml_UrlNotFound()
		{
			AssertFileUrlNotFound(Utility.GetPackingHouseListFileUrlFromHtml, "Packing house list file url can not be found from");
		}

		void AssertFileUrlNotFound(Func<string, string> getFileUrlFromHtml, string exceptionMessage)
		{
			var message = string.Empty;
			var html = @"<!DOCTYPE html>
<html>
<head>
    <title>404 - Content Not Found</title>
</head>
<body>
    <h1>404 - Page not found</h1>
</body>
</html>";
			try
			{
				getFileUrlFromHtml(html);
			}
			catch (Exception ex)
			{
				message = ex.Message;
			}
			Assert.That(message, Does.Contain(exceptionMessage));
		}

		[Test]
		public void TestGetAttachmentFileUrlFromHtml()
		{
			var html = File.ReadAllText(Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/TWICI/UnitsOfMeasurementDownloadPage.html"));
			Assert.AreEqual("https://web.customs.gov.tw/download/cus1_73024_1220", Utility.GetAttachmentFileUrlFromHtml(html));

			html = File.ReadAllText(Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/SCECA/TWSCECADownloadPage.html"));
			Assert.AreEqual("https://web.customs.gov.tw/download/1cae3a99660e4131b880682ba1c164cc", Utility.GetAttachmentFileUrlFromHtml(html, "file-pdf"));
		}

		[Test]
		public void TestGetPublicationDateTimeUseRegex()
		{
			var now = new DateTime(2019, 6, 24, 16, 43, 31);
			SystemContext.Now = () => now;
			AssertGetPublicationDateTimeUseRegex(now, "發布日期");
			AssertGetPublicationDateTimeUseRegex(now, "更新日期");
			Assert.AreEqual(now, Utility.GetPublicationDateTimeUseRegex("<html></html>"));
			Assert.AreEqual(new DateTime(2024, 3, 6), Utility.GetPublicationDateTimeUseRegex(File.ReadAllText(Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/FAC/DischargingStoringKeelung.html"))));
			Assert.AreEqual(new DateTime(2024, 2, 19), Utility.GetPublicationDateTimeUseRegex(File.ReadAllText(Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/FAC/DischargingStoringKaohsiung.html"))));
			Assert.AreEqual(new DateTime(2024, 1, 3), Utility.GetPublicationDateTimeUseRegex(File.ReadAllText(Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/FAC/DischargingStoringTaichung.html"))));
			Assert.AreEqual(new DateTime(2024, 2, 15), Utility.GetPublicationDateTimeUseRegex(File.ReadAllText(Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/FAC/DischargingStoringTaipei.html"))));
			Assert.AreEqual(now, Utility.GetPublicationDateTimeUseRegex(File.ReadAllText(Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/FAC/DischargingStoringTaipei_noPublicTime.html"))));
		}

		void AssertGetPublicationDateTimeUseRegex(DateTime now, string chineseDateDescription)
		{
			Assert.AreEqual(new DateTime(2023, 9, 19), Utility.GetPublicationDateTimeUseRegex($"<html><span>{chineseDateDescription}：2023-09-19</span></html>"));
			Assert.AreEqual(new DateTime(2023, 9, 20), Utility.GetPublicationDateTimeUseRegex($"<html><span>{chineseDateDescription}：2023/09/20</span></html>"));
			Assert.AreEqual(new DateTime(2023, 9, 21), Utility.GetPublicationDateTimeUseRegex($"<html><span>{chineseDateDescription}：112-09-21</span></html>"));
			Assert.AreEqual(new DateTime(2023, 9, 22), Utility.GetPublicationDateTimeUseRegex($"<html><span>{chineseDateDescription}：112/09/22</span></html>"));
			Assert.AreEqual(now, Utility.GetPublicationDateTimeUseRegex($"<html><span>{chineseDateDescription}：2023-13-20</span></html>"));
			Assert.AreEqual(now, Utility.GetPublicationDateTimeUseRegex($"<html><span>{chineseDateDescription}：五月二十三日</span></html>"));
			Assert.AreEqual(now, Utility.GetPublicationDateTimeUseRegex($"<html><span>{chineseDateDescription}2023-09-20</span></html>"));
		}

		[Test]
		public void TestGetTaiwanAircraftPartCAACodeCategoryLastUpdateDateTimeFromHtml()
		{
			var now = new DateTime(2019, 6, 24, 16, 43, 31);
			SystemContext.Now = () => now;
			Assert.AreEqual(new DateTime(2018, 8, 24, 15, 51, 0), Utility.GetTaiwanAircraftPartCAACodeCategoryLastUpdateDateTimeFromHtml("<html><div class=\"lastupdated\">最後更新日期：<span> 2018/08/24 15:51</span></div></html>"));
			Assert.AreEqual(new DateTime(2018, 8, 24, 15, 52, 0), Utility.GetTaiwanAircraftPartCAACodeCategoryLastUpdateDateTimeFromHtml("<html><div class=\"lastupdated\">最後更新日期;<span> 2018-08-24 15:52</span></div></html>"));
			Assert.AreEqual(new DateTime(2023, 9, 21, 15, 53, 0), Utility.GetTaiwanAircraftPartCAACodeCategoryLastUpdateDateTimeFromHtml("<html><div class=\"lastupdated\">最後更新日期<span> 112-09-21 15:53</span></div></html>"));
			Assert.AreEqual(new DateTime(2023, 9, 22, 15, 54, 0), Utility.GetTaiwanAircraftPartCAACodeCategoryLastUpdateDateTimeFromHtml("<html><div class=\"lastupdated\">最後更新日期 <span> 112/09/22 15:54</span></div></html>"));
			Assert.AreEqual(now, Utility.GetTaiwanAircraftPartCAACodeCategoryLastUpdateDateTimeFromHtml("<html></html>"));
		}

		[Test]
		public void TestGetTariffLastUpdateDateTimeFromHtml()
		{
			var now = new DateTime(2019, 6, 24, 16, 43, 31);
			SystemContext.Now = () => now;
			Assert.AreEqual(new DateTime(2024, 3, 14, 16, 4, 0), Utility.GetTariffLastUpdateDateTimeFromHtml("<html><font color=\"blue\">最後更新時間&nbsp;  2024/03/14 16:04</font></html>"));
			Assert.AreEqual(new DateTime(2018, 8, 24, 16, 5, 0), Utility.GetTariffLastUpdateDateTimeFromHtml("<html><font color=\"blue\">最後更新時間：2018-08-24 16:05</font></html>"));
			Assert.AreEqual(new DateTime(2023, 9, 21, 16, 6, 0), Utility.GetTariffLastUpdateDateTimeFromHtml("<html><font color=\"blue\">最後更新時間112-09-21 16:06</font></html>"));
			Assert.AreEqual(new DateTime(2023, 9, 22, 16, 7, 0), Utility.GetTariffLastUpdateDateTimeFromHtml("<html><font color=\"blue\">最後更新時間 112/09/22 16:07</font></html>"));
			Assert.AreEqual(now, Utility.GetTariffLastUpdateDateTimeFromHtml("<html></html>"));
		}

		[Test]
		public void TestGetUnitsOfMeasurementFileUrlFromHtml()
		{
			var html = File.ReadAllText(Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/TWICI/UnitsOfMeasurementDownloadPage.html"));
			var url = Utility.GetUnitsOfMeasurementFileUrlFromHtml(html.Replace("downloadFileNameRegexTest", AppConfig.CodeLists.TaiwanUnitsOfMeasurement.Regex.DownloadFileNameRegexTest));
			Assert.AreEqual("https://web.customs.gov.tw/download/cus1_73025_1220", url);
		}

		[Test]
		public void TestGetUnitsOfMeasurementFileUrlFromHtml_UrlNotFound()
		{
			AssertFileUrlNotFound(Utility.GetUnitsOfMeasurementFileUrlFromHtml, "Units of measurement file url can not be found from");
		}

		[Test]
		public void TestGetDischargingStoringKeelungFileUrlsFromHtml()
		{
			var html = File.ReadAllText(Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/FAC/DischargingStoringKeelung.html"));
			var url = Utility.GetDischargingStoringFileUrlsFromHtml(html, TaiwanCustomsUrl);
			Assert.AreEqual("https://web.customs.gov.tw/keelung/download/7a1b9177ae4848f18af087c922199029;https://web.customs.gov.tw/keelung/download/8562260a69a14786853539961933dafe;https://web.customs.gov.tw/keelung/download/cus3_82524_3218;https://web.customs.gov.tw/keelung/download/cus3_72076_3218;https://web.customs.gov.tw/keelung/download/1cb35aeddea8412189f81bc6d58940b2;https://web.customs.gov.tw/keelung/download/6ba42833d5a042d5a87e77c0380b6eb0", url);
		}

		[Test]
		public void TestGetDischargingStoringTaipeiFileUrlsFromHtml()
		{
			var html = File.ReadAllText(Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/FAC/DischargingStoringTaipei.html"));
			var url = Utility.GetDischargingStoringFileUrlsFromHtml(html, TaiwanCustomsUrl);
			Assert.AreEqual("https://web.customs.gov.tw/download/f26db7637de74eae8efcec69891148d9;https://web.customs.gov.tw/download/2d29f7a4e717470c8e6379bd690e7e02;https://web.customs.gov.tw/download/8af78ad463894f6391909c446c12aa77", url);
		}

		[Test]
		public void TestGetDischargingStoringTaichungFileUrlsFromHtml()
		{
			var html = File.ReadAllText(Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/FAC/DischargingStoringTaichung.html"));
			var url = Utility.GetDischargingStoringFileUrlsFromHtml(html, TaiwanCustomsUrl);
			Assert.AreEqual("https://web.customs.gov.tw/download/67661d8deeae49499ce66f39b0e03d8d;https://web.customs.gov.tw/download/e3966ae23e87407088d70f2305699da3;https://web.customs.gov.tw/download/2c46a1c4a4b24ee4ac03fc8461bc8cf3", url);
		}

		[Test]
		public void TestGetDischargingStoringKaohsiungFileUrlsFromHtml()
		{
			var html = File.ReadAllText(Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/FAC/DischargingStoringKaohsiung.html"));
			var url = Utility.GetDischargingStoringFileUrlsFromHtml(html, TaiwanCustomsUrl);
			Assert.AreEqual("https://web.customs.gov.tw/download/cus1_81162_1220;https://web.customs.gov.tw/download/cus1_79594_1220;https://web.customs.gov.tw/download/1c1748909e484dcfaeffc0eb7a375713", url);
		}

		[Test]
		public void TestGetDischargingStoringFileUrlsFromHtml_UrlNotFound()
		{
			AssertFileUrlNotFound((html) => Utility.GetDischargingStoringFileUrlsFromHtml(html, TaiwanCustomsUrl), "Discharging Storing file url can not be found from");
		}

		[Test]
		public void TestTempDirectory()
		{
			DirectoryAssert.Exists(Utility.TempDirectory);
		}

		[Test]
		public void TestSafeSubstring()
		{
			var value = "6034C9C6-A8D0-4D07-B85D-14F73C1A8FB9";
			int startIndex = 0;
			int length = 8;
			Assert.AreEqual("6034C9C6", Utility.SafeSubstring(value, startIndex, length));

			value = string.Empty;
			startIndex = 0;
			length = 2;
			Assert.AreEqual(string.Empty, Utility.SafeSubstring(value, startIndex, length));

			value = "6034C9C6";
			startIndex = 0;
			length = 10;
			Assert.AreEqual("6034C9C6", Utility.SafeSubstring(value, startIndex, length));
		}

		[Test]
		public void TestCompileHsSectionsAndChaptersFromWord()
		{
			var expectedHsChapterAndSection = new Dictionary<int, int>
			{
				{ 1, 1 },
				{ 2, 1 },
				{ 6, 2 },
				{ 15, 3 }
			};

			var path = Path.Combine(FolderHelper.GetBinFolder(), "doc/Nomenclature/enname1.doc");
			using (var memoryStream = new MemoryStream(File.ReadAllBytes(path)))
			{
				var hsChapterAndSection = new Dictionary<int, int>();
				Utility.CompileHsSectionsAndChaptersFromWord(memoryStream, hsChapterAndSection);
				Assert.AreEqual(expectedHsChapterAndSection.Count, hsChapterAndSection.Count);
				CollectionAssert.AreEquivalent(expectedHsChapterAndSection, hsChapterAndSection);
			}
		}

		[Test]
		public void TestGetDataFromFile()
		{
			var mock = new Mock<IWebClient>();
			var responseTariff2Bytes = File.ReadAllBytes(Path.Combine(FolderHelper.GetBinFolder(), "doc/Tariff/TARIFF_2.txt"));
			mock.Setup(x => x.DownloadData("http://192.118.118.1/TARIFF_2.txt")).Returns(responseTariff2Bytes);

			var url = "http://192.118.118.1/TARIFF_2.txt";
			var result = Utility.GetDataFromFile<TariffColumn1And3DataRow>(url, mock.Object, line => new TariffColumn1And3DataRow(line));
			Assert.AreEqual(8, result.Count);
			Assert.IsNotNull(result.Find(x => x.TariffCode == "01012100003"));
			Assert.IsNotNull(result.Find(x => x.TariffCode == "01031000004"));
			Assert.IsNotNull(result.Find(x => x.TariffCode == "87120010109"));
			Assert.IsNotNull(result.Find(x => x.TariffCode == "01061490008"));
			Assert.IsNotNull(result.Find(x => x.TariffCode == "02075100009"));
			Assert.IsNotNull(result.Find(x => x.TariffCode == "02011010003"));
			Assert.IsNotNull(result.Find(x => x.TariffCode == "04011010001"));
			Assert.IsNotNull(result.Find(x => x.TariffCode == "04011020009"));
			Assert.IsNull(result.Find(x => x.TariffCode == "XXXXXXXXXXX"));
		}

		[Test]
		public void TestGetSubheadings()
		{
			var hsSubheadings = "00";
			Assert.AreEqual(".10", Utility.GetSubheadings(hsSubheadings));

			hsSubheadings = "X0";
			Assert.AreEqual(".X", Utility.GetSubheadings(hsSubheadings));

			hsSubheadings = "85";
			Assert.AreEqual(".8.5", Utility.GetSubheadings(hsSubheadings));

			hsSubheadings = "6";
			Assert.AreEqual(".6", Utility.GetSubheadings(hsSubheadings));
		}

		[Test]
		public void TestGetEnvironmentalProtectionTariffsFromExcel()
		{
			var expectedList = new List<string>() { "04011010001",
													"04011020009",
													"04011090004",
													"04012010009",
													"04012020007",
													"04012090002",
													"04014010005"};


			var path = Path.Combine(FolderHelper.GetBinFolder(), "doc/Utilities/EnvironmentalProtectionTariffs1.ods");
			var mock = new Mock<IWebClient>();
			var bytes = File.ReadAllBytes(path);
			mock.Setup(x => x.DownloadData("http://192.118.118.1/EnvironmentalProtectionTariffs1.ods")).Returns(bytes);
			var referenceList = Utility.GetEnvironmentalProtectionTariffsFromExcel("http://192.118.118.1/EnvironmentalProtectionTariffs1.ods", mock.Object);
			Assert.AreEqual(7, referenceList.Count);
			CollectionAssert.AreEquivalent(expectedList, referenceList);
		}

		[Test]
		public void TestParseCnToIntString()
		{
			Assert.AreEqual("1", Utility.ParseCnToIntString("一"));
			Assert.AreEqual("2", Utility.ParseCnToIntString("二"));
			Assert.AreEqual("3", Utility.ParseCnToIntString("三"));
			Assert.AreEqual("4", Utility.ParseCnToIntString("四"));
			Assert.AreEqual("5", Utility.ParseCnToIntString("五"));
			Assert.AreEqual("6", Utility.ParseCnToIntString("六"));
			Assert.AreEqual("7", Utility.ParseCnToIntString("七"));
			Assert.AreEqual("8", Utility.ParseCnToIntString("八"));
			Assert.AreEqual("9", Utility.ParseCnToIntString("九"));
			Assert.AreEqual("10", Utility.ParseCnToIntString("十"));
			Assert.AreEqual("11", Utility.ParseCnToIntString("十一"));
			Assert.AreEqual("12", Utility.ParseCnToIntString("十二"));
			Assert.AreEqual("13", Utility.ParseCnToIntString("十三"));
			Assert.AreEqual("99", Utility.ParseCnToIntString("九十九"));
			Assert.AreEqual("999", Utility.ParseCnToIntString("九百九十九"));
			Assert.AreEqual("999", Utility.ParseCnToIntString("九百九十九"));
			Assert.AreEqual("9999", Utility.ParseCnToIntString("九千九百九十九"));
			Assert.AreEqual("99999", Utility.ParseCnToIntString("九萬九千九百九十九"));
			Assert.AreEqual("999999", Utility.ParseCnToIntString("九十九萬九千九百九十九"));
			Assert.AreEqual("9999999", Utility.ParseCnToIntString("九百九十九萬九千九百九十九"));
			Assert.AreEqual("99999999", Utility.ParseCnToIntString("九千九百九十九萬九千九百九十九"));
			Assert.AreEqual("999999999", Utility.ParseCnToIntString("九億九千九百九十九萬九千九百九十九"));
			Assert.AreEqual("9999999999", Utility.ParseCnToIntString("九十九億九千九百九十九萬九千九百九十九"));
			Assert.AreEqual("99999999999", Utility.ParseCnToIntString("九百九十九億九千九百九十九萬九千九百九十九"));
			Assert.AreEqual("999999999999", Utility.ParseCnToIntString("九千九百九十九億九千九百九十九萬九千九百九十九"));
			Assert.AreEqual("90101", Utility.ParseCnToIntString("九萬零一百零一"));
			Assert.AreEqual("0", Utility.ParseCnToIntString(""));
			Assert.AreEqual("0", Utility.ParseCnToIntString("零"));
			Assert.AreEqual("0", Utility.ParseCnToIntString("萬萬"));
		}
	}
}
