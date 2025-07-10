using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class OdfHelperTest
	{
		[Test]
		public void TestGetCellNodesWithAttribute_NumbercolumnsRepeated()
		{
			var filename = Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/DischargingStoringPlace_Taipei.ods");
			var data = File.ReadAllBytes(filename);
			var tables = OdfHelper.ExtractTableToDataTables(data);
			var table = tables[0];
			Assert.AreEqual("科學城物流股份有限公司竹科分公司出口貨棧", table.Rows[8][2]);
			Assert.AreEqual("93.11.16設立", table.Rows[8][7]);
		}

		[Test]
		public void TestGetXmlNamespaceManager()
		{
			var filename = Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/OdtContent.xml");
			var document = new XmlDocument();
			document.Load(filename);
			var nsManager = OdfHelper.GetXmlNamespaceManager(document);
			Assert.AreEqual(document.NameTable, nsManager.NameTable);
			var prefixes = new[] { "table", "office", "style", "text", "draw", "fo", "dc", "meta", "number", "presentation", "svg", "chart", "dr3d", "math", "form", "script", "ooo", "ooow", "oooc", "dom", "xforms", "xsd", "xsi", "rpt", "of", "rdfa", "config" };
			Assert.IsTrue(prefixes.All(prefix => nsManager.HasNamespace(prefix)));
		}


		[Test]
		public void TestExtractTableToDataTable()
		{
			var path = Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/OdtSample.odt");
			var data = File.ReadAllBytes(path);
			var tables = OdfHelper.ExtractTableToDataTables(data);
			Assert.AreEqual(3, tables.Length);
			var table = tables[0];
			Assert.AreEqual(7, table.Columns.Count);
			Assert.AreEqual(17, table.Rows.Count);
			Assert.AreEqual("KEL", table.Rows[1][0]);
			Assert.AreEqual("E14TS", table.Rows[2][1]);
			Assert.AreEqual("國免供應有限公司基隆分公司基隆港東14庫之1(自由貿易港區港區貨棧)", table.Rows[10][2]);
			Assert.AreEqual("AS", tables[2].Rows[3][5]);
		}

		[Test]
		public void TestExtractTextHNodeList()
		{
			var path = Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/TWCA/附件--請公告修正之「關港貿作業代碼」四十七、機關別.odt");
			var data = File.ReadAllBytes(path);
			var textHNodeList = OdfHelper.ExtractTextHNodeList(data);
			Assert.AreEqual(41, textHNodeList.Count);
			var textHNode = textHNodeList[0];
			Assert.AreEqual("四十七、機關別", textHNode.InnerText);
			Assert.AreEqual("<text:h text:style-name=\"P39\" text:outline-level=\"1\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">四十七、機關別</text:h>", textHNode.OuterXml);
		}
	}
}
