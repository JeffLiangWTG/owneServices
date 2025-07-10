using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	class RefCusCodeListUpdateInfoTest
	{
		[Test]
		public void TestPopulateCusCodeListsForFacilityCode()
		{
			AssertPopulateCusCodeListsForMultipleFiles(RefCusCodeListUpdateInfo.CodeTypes.FacilityCode, "doc/RefCusCodeList/FAC/TestStdInput", "doc/RefCusCodeList/FAC/TestOutput/STD20230818_123136.xml");
		}

		[Test]
		public void TestPopulateCusCodeListsForFacilityCode_Logging()
		{
			var expectLogs = "[ERROR]680CG126: Analyzation Error (Description: 長榮空廚股份有限公司自主管理保稅倉庫, Note: 110.4.15設立撤銷): Unknown activity 設立撤銷\t650CG115: Analyzation Error (Description: 高雄空廚股份有限公司自主管理保稅倉庫, Note: 111.11.1): Cannot find activity for date 111.11.1\r\n";
			var output = new StringWriter();
			Console.SetError(output);
			AssertPopulateCusCodeListsForMultipleFiles(RefCusCodeListUpdateInfo.CodeTypes.FacilityCode, "doc/RefCusCodeList/FAC/TestLogInput", "doc/RefCusCodeList/FAC/TestOutput/LOG20230818_165212.xml");
			Assert.AreEqual(Encoding.UTF8, Console.OutputEncoding);
			Assert.AreEqual(expectLogs, output.ToString());
		}

		[Test]
		public void TestPopulateCusCodeListsForFacilityCode_Keelung()
		{
			AssertPopulateCusCodeListsForMultipleFiles(RefCusCodeListUpdateInfo.CodeTypes.FacilityCode, "doc/RefCusCodeList/FAC/Keelung", "doc/RefCusCodeList/FAC/TestOutput/Keelung.xml");
		}

		[Test]
		public void TestPopulateCusCodeListsForFacilityCode_Kaohsiung()
		{
			AssertPopulateCusCodeListsForMultipleFiles(RefCusCodeListUpdateInfo.CodeTypes.FacilityCode, "doc/RefCusCodeList/FAC/Kaohsiung", "doc/RefCusCodeList/FAC/TestOutput/Kaohsiung.xml");
		}

		[Test]
		public void TestPopulateCusCodeListsForFacilityCode_Taichung()
		{
			AssertPopulateCusCodeListsForMultipleFiles(RefCusCodeListUpdateInfo.CodeTypes.FacilityCode, "doc/RefCusCodeList/FAC/Taichung", "doc/RefCusCodeList/FAC/TestOutput/Taichung.xml");
		}

		[Test]
		public void TestPopulateCusCodeListsForFacilityCode_Taipei()
		{
			AssertPopulateCusCodeListsForMultipleFiles(RefCusCodeListUpdateInfo.CodeTypes.FacilityCode, "doc/RefCusCodeList/FAC/Taipei", "doc/RefCusCodeList/FAC/TestOutput/Taipei.xml");
		}

		void AssertPopulateCusCodeListsForMultipleFiles(string codeType, string testFileDirectory, string expectedFileName)
		{
			SystemContext.Now = () => new DateTime(2019, 06, 24, 16, 43, 31);

			var fileName = Path.Combine(Utility.TempDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.xml", Guid.NewGuid().ToString()));
			var testFiles = Directory.GetFiles(Path.Combine(FolderHelper.GetBinFolder(), testFileDirectory));
			var datas = testFiles.Select(filename => File.ReadAllBytes(filename));
			var info = new RefCusCodeListUpdateInfo(codeType, fileName, datas, SystemContext.Now());
			info.Run();
			var actualXml = XDocument.Load(fileName);
			var expectedFilePath = Path.Combine(FolderHelper.GetBinFolder(), expectedFileName);
			var expectedXml = XDocument.Load(expectedFilePath);
			TestHelper.AssertXMLEquals(expectedXml.ToString(), actualXml.ToString());
			File.Delete(fileName);
		}

		[Test]
		public void TestUnitsOfMeasurementExecute()
		{
			TestExecuteForData(RefCusCodeListUpdateInfo.CodeTypes.TaiwanUnitsOfMeasurement, "doc/RefCusCodeList/TWCIU/計量單位.pdf", "doc/RefCusCodeList/TWCIU/TWCIU.xml");
		}

		[Test]
		public void TestSCECAExecute()
		{
			TestExecuteForData(RefCusCodeListUpdateInfo.CodeTypes.TaiwanSCECA, "doc/RefCusCodeList/SCECA/簽審機關免證專用代碼彙整表-1130813更新版.pdf", "doc/RefCusCodeList/SCECA/TWSCECA.xml");
		}

		[Test]
		public void TestTWCAExecute()
		{
			TestExecuteForData(RefCusCodeListUpdateInfo.CodeTypes.TaiwanControllingAgency, "doc/RefCusCodeList/TWCA/附件--請公告修正之「關港貿作業代碼」四十七、機關別.odt", "doc/RefCusCodeList/TWCA/TWCA.xml");
		}

		[Test]
		public void TestCUSOFExecute()
		{
			TestExecuteForData(RefCusCodeListUpdateInfo.CodeTypes.TaiwanCustomsOffices, "doc/RefCusCodeList/CUSOF/一、關別.odt", "doc/RefCusCodeList/CUSOF/CUSOF.xml");
		}

		[Test]
		public void TestCAACCExecute()
		{
			SystemContext.Now = () => new DateTime(2019, 06, 24, 16, 43, 31);

			var fileName = Path.Combine(Utility.TempDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.xml", Guid.NewGuid().ToString()));
			var inputFilePath = Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/CAACC/DownLoadPage.html");
			var html = File.ReadAllText(inputFilePath);
			var updateInfo = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.TaiwanAircraftPartCAACodeCategory, fileName, SystemContext.Now(), html);
			updateInfo.Run();
			var actualXml = XDocument.Load(fileName);
			var expectedFilePath = Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/CAACC/CAACC.xml");
			var expectedXml = XDocument.Load(expectedFilePath);
			TestHelper.AssertXMLEquals(expectedXml.ToString(), actualXml.ToString());
			File.Delete(fileName);
		}

		[Test]
		public void TestCAACExecute()
		{
			AssertPopulateCusCodeListsForMultipleFiles(RefCusCodeListUpdateInfo.CodeTypes.TaiwanAircraftPartCAACCode, "doc/RefCusCodeList/CAAC/TestInput", "doc/RefCusCodeList/CAAC/TestOutput/CAAC.xml");
		}

		[Test]
		public void TestCommodityInspectionExecute()
		{
			SystemContext.Now = () => new DateTime(2019, 06, 24, 16, 43, 31);

			var fileName = Path.Combine(Utility.TempDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.xml", Guid.NewGuid().ToString()));
			var testFiles = Directory.GetFiles(Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/TWICI/TestInput"));
			var datas = testFiles.Select(filename => File.ReadAllBytes(filename));
			var info = new RefCusCodeListUpdateInfo(RefCusCodeListUpdateInfo.CodeTypes.TaiwanCommodityInspection, fileName, datas, SystemContext.Now());
			info.Run();
			var actualXml = XDocument.Load(fileName);
			var expectedFilePath = Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/TWICI/TestOutput/TWICI.xml");
			var expectedXml = XDocument.Load(expectedFilePath);
			TestHelper.AssertXMLEquals(expectedXml.ToString(), actualXml.ToString());
			File.Delete(fileName);
		}

		[Test]
		public void TestPackingHouseExecute()
		{
			TestExecuteForData(RefCusCodeListUpdateInfo.CodeTypes.TaiwanPackingHouse, "doc/RefCusCodeList/TWPKH/PackingHouseList.xlsx", "doc/RefCusCodeList/TWPKH/TWPKH.xml");
		}

		static void TestExecuteForData(string codeType, string inputFileName, string expectedFileName)
		{
			SystemContext.Now = () => new DateTime(2019, 06, 24, 16, 43, 31);

			var fileName = Path.Combine(Utility.TempDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.xml", Guid.NewGuid().ToString()));
			var inputFilePath = Path.Combine(FolderHelper.GetBinFolder(), inputFileName);
			var data = File.ReadAllBytes(inputFilePath);
			var updateInfo = new RefCusCodeListUpdateInfo(codeType, fileName, data, SystemContext.Now());
			updateInfo.Run();
			var actualXml = XDocument.Load(fileName);
			var expectedFilePath = Path.Combine(FolderHelper.GetBinFolder(), expectedFileName);
			var expectedXml = XDocument.Load(expectedFilePath);
			TestHelper.AssertXMLEquals(expectedXml.ToString(), actualXml.ToString());
			File.Delete(fileName);
		}

		[Test]
		public void TestRejectionReasonExecute()
		{
			TestExecute(RefCusCodeListUpdateInfo.CodeTypes.RejectionReason, "doc/RefCusCodeList/TWRR/Rejection Reason.xlsx", "doc/RefCusCodeList/TWRR/TWRR.xml");
		}

		[Test]
		public void TestRequiredFormalitiesExecute()
		{
			TestExecute(RefCusCodeListUpdateInfo.CodeTypes.RequiredFormalities, "doc/RefCusCodeList/TWRF/Required formalities.xlsx", "doc/RefCusCodeList/TWRF/TWRF.xml");
		}

		[Test]
		public void TestTaiwanCustomsRequirementsExecute()
		{
			TestExecute(RefCusCodeListUpdateInfo.CodeTypes.TaiwanCustomsRequirements, "doc/RefCusCodeList/TWCR/Taiwan Customs Requirements.xlsx", "doc/RefCusCodeList/TWCR/TWCR.xml");
		}

		[Test]
		public void TestTaiwanImportRegulationsExecute()
		{
			TestExecuteForData(RefCusCodeListUpdateInfo.CodeTypes.TaiwanImportRegulations, "doc/RefCusCodeList/TWIR/IReg.csv", "doc/RefCusCodeList/TWIR/TWIR.xml");
		}

		[Test]
		public void TestTaiwanExportRegulationsExecute()
		{
			TestExecuteForData(RefCusCodeListUpdateInfo.CodeTypes.TaiwanExportRegulations, "doc/RefCusCodeList/TWER/EReg.csv", "doc/RefCusCodeList/TWER/TWER.xml");
		}

		static void TestExecute(string codeType, string excelPath, string expectedFileName)
		{
			SystemContext.Now = () => new DateTime(2019, 06, 24, 16, 43, 31);

			var fileName = Path.Combine(Utility.TempDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.xml", Guid.NewGuid().ToString()));
			var updateInfo = new RefCusCodeListUpdateInfo(codeType, fileName, SystemContext.Now()) { ExcelPath = Path.Combine(FolderHelper.GetBinFolder(), excelPath) };
			updateInfo.Run();
			var actualXml = XDocument.Load(fileName);
			var path = Path.Combine(FolderHelper.GetBinFolder(), expectedFileName);
			var expectedXml = XDocument.Load(path);
			Assert.AreEqual(expectedXml.ToString(), actualXml.ToString());
			File.Delete(fileName);
		}

		[Test]
		[SetCulture("en-US")]
		public void TestExecuteWhenUS()
		{
			TestRejectionReasonExecute();
			TestRequiredFormalitiesExecute();
		}

		[Test]
		[SetCulture("fr-FR")]
		public void TestExecuteWhenFR()
		{
			TestRejectionReasonExecute();
			TestRequiredFormalitiesExecute();
		}

		[Test]
		public void TestTWREJExecute()
		{
			TestExecuteForData(RefCusCodeListUpdateInfo.CodeTypes.TWCPT016RejectionReason, "doc/RefCusCodeList/TWREJ/不受理報關原因.odt", "doc/RefCusCodeList/TWREJ/CPT_016_RejectionReason.xml");
		}

		[Test]
		public void TestTWREQExecute()
		{
			TestExecuteForData(RefCusCodeListUpdateInfo.CodeTypes.TWCPT_017_Error_DocumentOrRequiredFormalities, "doc/RefCusCodeList/TWREQ/錯單或應補辦事項_1140501.odt", "doc/RefCusCodeList/TWREQ/TWREQ.xml");
		}

		[Test]
		public void TestTWREWExecute()
		{
			TestExecuteForData(RefCusCodeListUpdateInfo.CodeTypes.CPT018ResponseToWarehouse, "doc/RefCusCodeList/TWREW/倉儲或運輸業申報訊息回覆原因或應補辦事項通知.odt", "doc/RefCusCodeList/TWREW/TWCPT_018_TWREW.xml");
		}
	}
}
