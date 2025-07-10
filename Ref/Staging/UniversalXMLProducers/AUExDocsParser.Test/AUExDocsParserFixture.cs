using System.IO;
using System.Reflection;
using System.Xml;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.AUExDocsParser.Test
{
	[TestFixture]
	public class AUExDocsParserFixture
	{
		[Test]
		public void TestParseE21()
		{
			string fileCode = "E21Test";
			var parser = new E21_ProductTypeParser();
			var result = parser.Parse(Path.Combine(BinPath, $@"Res\{fileCode}.TXT"));
			var dumpFilePath = Path.Combine(BinPath, $@"RefCusCodeListZZ_{fileCode}_AU.xml");
			parser.ExportToXml(result, dumpFilePath, true);

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(dumpFilePath);
			var codeListNodes = xmlDoc.SelectNodes("UniversalReferenceData/RefCusCodeList");
			Assert.AreEqual(1, codeListNodes.Count);

			var firstNode = codeListNodes[0];
			Assert.AreEqual("PRODD", firstNode["ZZD_ZZK_NKCodeType"].InnerText);
			Assert.AreEqual("AMF", firstNode["ZZD_Code"].InnerText);
			Assert.AreEqual("ANHYDROUS MILK FAT", firstNode["ZZD_Description"].InnerText);
			Assert.AreEqual("1900-01-01T00:00:00", firstNode["ZZD_StartDate"].InnerText);

			Assert.AreEqual("ScientificName", firstNode["RefCusCodeListAttribute"]["ZZE_ZXE_NKName"].InnerText);
			Assert.AreEqual("BOS TAURUS,  BOS INDICUS, BOS TAURUS INDICUS CROSS", firstNode["RefCusCodeListAttribute"]["ZZE_Value"].InnerText);

			File.Delete(dumpFilePath);
		}

		[Test]
		public void TestParseE07()
		{
			string fileCode = "E07Test";
			var parser = new E07_CutCodeParser();
			var result = parser.Parse(Path.Combine(BinPath, $@"Res\{fileCode}.TXT"));
			var dumpFilePath = Path.Combine(BinPath, $@"RefCusCodeListZZ_{fileCode}_AU.xml");
			parser.ExportToXml(result, dumpFilePath, true);

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(dumpFilePath);
			var codeListNodes = xmlDoc.SelectNodes("UniversalReferenceData/RefCusCodeList");
			Assert.AreEqual(1, codeListNodes.Count);

			var firstNode = codeListNodes[0];
			Assert.AreEqual("CUTCM", firstNode["ZZD_ZZK_NKCodeType"].InnerText);
			Assert.AreEqual("1010H", firstNode["ZZD_Code"].InnerText);
			Assert.AreEqual("HINDQUARTER  3 RIB (EU HIGH QUALITY BEEF)", firstNode["ZZD_Description"].InnerText);
			Assert.AreEqual("1900-01-01T00:00:00", firstNode["ZZD_StartDate"].InnerText);

			var attributeNodes = firstNode.SelectNodes("RefCusCodeListAttribute");
			Assert.AreEqual("BoneInIndicator", attributeNodes[0]["ZZE_ZXE_NKName"].InnerText);
			Assert.AreEqual("I", attributeNodes[0]["ZZE_Value"].InnerText);
			Assert.AreEqual("IsBeefVeal", attributeNodes[1]["ZZE_ZXE_NKName"].InnerText);
			Assert.AreEqual("N", attributeNodes[1]["ZZE_Value"].InnerText);
			Assert.AreEqual("IsChemicalLean", attributeNodes[2]["ZZE_ZXE_NKName"].InnerText);
			Assert.AreEqual("N", attributeNodes[2]["ZZE_Value"].InnerText);

			File.Delete(dumpFilePath);
		}

		[Test]
		public void TestParseE38()
		{
			string fileCode = "E38Test";
			var parser = new E38_DominantProductParser();
			var result = parser.Parse(Path.Combine(BinPath, $@"Res\{fileCode}.TXT"));
			var dumpFilePath = Path.Combine(BinPath, $@"RefCusCodeListZZ_{fileCode}_AU.xml");
			parser.ExportToXml(result, dumpFilePath, true);

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(dumpFilePath);
			var codeListNodes = xmlDoc.SelectNodes("UniversalReferenceData/RefCusCodeList");
			Assert.AreEqual(1, codeListNodes.Count);

			var firstNode = codeListNodes[0];
			Assert.AreEqual("BEEF", firstNode["ZZD_Code"].InnerText);
			Assert.AreEqual("BEEF", firstNode["ZZD_Description"].InnerText);
			Assert.AreEqual("1900-01-01T00:00:00", firstNode["ZZD_StartDate"].InnerText);

			File.Delete(dumpFilePath);
		}

		[Test]
		public void TestParseE25()
		{
			string fileCode = "E25Test";
			var parser = new E25_SupplementaryCodeParser();
			var result = parser.Parse(Path.Combine(BinPath, $@"Res\{fileCode}.TXT"));
			var dumpFilePath = Path.Combine(BinPath, $@"RefCusCodeListZZ_{fileCode}_AU.xml");
			parser.ExportToXml(result, dumpFilePath, true);

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(dumpFilePath);
			var codeListNodes = xmlDoc.SelectNodes("UniversalReferenceData/RefCusCodeList");
			Assert.AreEqual(1, codeListNodes.Count);

			var firstNode = codeListNodes[0];
			Assert.AreEqual("A", firstNode["ZZD_Code"].InnerText);
			Assert.AreEqual("AGED", firstNode["ZZD_Description"].InnerText);
			Assert.AreEqual("1900-01-01T00:00:00", firstNode["ZZD_StartDate"].InnerText);

			var attributeNodes = firstNode.SelectNodes("RefCusCodeListAttribute");
			Assert.AreEqual("IsMeat", attributeNodes[0]["ZZE_ZXE_NKName"].InnerText);

			File.Delete(dumpFilePath);
		}

		[Test]
		public void TestParseE01()
		{
			string fileCode = "E01Test";
			var parser = new E01_AqisPlaceParser();
			var result = parser.Parse(Path.Combine(BinPath, $@"Res\{fileCode}.TXT"));
			var dumpFilePath = Path.Combine(BinPath, $@"RefCusCodeListZZ_{fileCode}_AU.xml");
			parser.ExportToXml(result, dumpFilePath, true);

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(dumpFilePath);
			var codeListNodes = xmlDoc.SelectNodes("UniversalReferenceData/RefCusCodeList");
			Assert.AreEqual(1, codeListNodes.Count);

			var firstNode = codeListNodes[0];
			Assert.AreEqual("ADL", firstNode["ZZD_Code"].InnerText);
			Assert.AreEqual("ADELAIDE", firstNode["ZZD_Description"].InnerText);
			Assert.AreEqual("1900-01-01T00:00:00", firstNode["ZZD_StartDate"].InnerText);

			var attributeNodes = firstNode.SelectNodes("RefCusCodeListAttribute");
			Assert.AreEqual("IsQuarantineRegion", attributeNodes[0]["ZZE_ZXE_NKName"].InnerText);
			Assert.AreEqual("IsMeat", attributeNodes[1]["ZZE_ZXE_NKName"].InnerText);
			Assert.AreEqual("IsDairy", attributeNodes[2]["ZZE_ZXE_NKName"].InnerText);
			Assert.AreEqual("IsGrain", attributeNodes[3]["ZZE_ZXE_NKName"].InnerText);
			Assert.AreEqual("IsHorticulture", attributeNodes[4]["ZZE_ZXE_NKName"].InnerText);
			Assert.AreEqual("IsWool", attributeNodes[5]["ZZE_ZXE_NKName"].InnerText);
			Assert.AreEqual("IsSkins", attributeNodes[6]["ZZE_ZXE_NKName"].InnerText);
			Assert.AreEqual("IsInedibleMeat", attributeNodes[7]["ZZE_ZXE_NKName"].InnerText);

			File.Delete(dumpFilePath);
		}

		[Test]
		public void TestParseE29()
		{
			string fileCode = "E29Test";
			var parser = new E29_AqisPlaceParser();
			var result = parser.Parse(Path.Combine(BinPath, $@"Res\{fileCode}.TXT"));
			var dumpFilePath = Path.Combine(BinPath, $@"RefCusCodeListZZ_{fileCode}_AU.xml");
			parser.ExportToXml(result, dumpFilePath, true);

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(dumpFilePath);
			var codeListNodes = xmlDoc.SelectNodes("UniversalReferenceData/RefCusCodeList");
			Assert.AreEqual(1, codeListNodes.Count);

			var firstNode = codeListNodes[0];
			Assert.AreEqual("FYW", firstNode["ZZD_Code"].InnerText);
			Assert.AreEqual("FYSWICK", firstNode["ZZD_Description"].InnerText);
			Assert.AreEqual("1900-01-01T00:00:00", firstNode["ZZD_StartDate"].InnerText);

			var attributeNodes = firstNode.SelectNodes("RefCusCodeListAttribute");
			Assert.AreEqual("IsQuarantineOffice", attributeNodes[0]["ZZE_ZXE_NKName"].InnerText);
			Assert.AreEqual("State", attributeNodes[1]["ZZE_ZXE_NKName"].InnerText);
			Assert.AreEqual("ACT", attributeNodes[1]["ZZE_Value"].InnerText);

			File.Delete(dumpFilePath);
		}

		[Test]
		public void TestParseE39()
		{
			string fileCode = "E39Test";
			var parser = new E39_ApprovedCertifierParser();
			var result = parser.Parse(Path.Combine(BinPath, $@"Res\{fileCode}.TXT"));
			var dumpFilePath = Path.Combine(BinPath, $@"RefCusCodeListZZ_{fileCode}_AU.xml");
			parser.ExportToXml(result, dumpFilePath, true);

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(dumpFilePath);
			var codeListNodes = xmlDoc.SelectNodes("UniversalReferenceData/RefCusCodeList");
			Assert.AreEqual(1, codeListNodes.Count);

			var firstNode = codeListNodes[0];
			Assert.AreEqual("H0001", firstNode["ZZD_Code"].InnerText);
			Assert.AreEqual("ADELAIDE MOSQUE ISLAMIC SOCIETY OF SOUTH AUSTRALIA", firstNode["ZZD_Description"].InnerText);
			Assert.AreEqual("1900-01-01T00:00:00", firstNode["ZZD_StartDate"].InnerText);

			File.Delete(dumpFilePath);
		}

		string BinPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}
}
