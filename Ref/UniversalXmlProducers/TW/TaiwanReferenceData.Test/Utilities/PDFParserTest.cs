using System.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class PDFParserTest
	{
		[Test]
		public void TestExtractSCECATable()
		{
			var pdfPath = Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/SCECA/簽審機關免證專用代碼彙整表-1130813更新版.pdf");
			var bytes = File.ReadAllBytes(pdfPath);
			var parser = new PDFParser();
			var list = parser.Parse(bytes);
			Assert.AreEqual(118, list.Count);
			Assert.AreEqual("經濟部國際貿易署（參據經濟部國際貿易局103年4月24日貿管字第1032850117號公告、經濟部111年8月19日經貿字第11104603570號公告及經濟部111 年8月19日經貿字第11104603571號公告）", list[0].source);
			Assert.AreEqual("FT999999999990", list[0].code);
			Assert.AreEqual("一、輸入下列「限制輸入貨品表」表外之大陸工業產品（稅則第25章至第97章），其起岸價格（CIF）在新臺幣32,000元以內，且單項產品在24件以內者（即24 PIECES/UNITS，不能以件數論計者，在40公斤以內），除特定物品項目因管理需要經經濟部公告排除適用外，准許免證並免依「MXX」規定進口，但屬稅則第6802節下之石材、稅則第6907及6908節下之磁磚者，其單項產品需同時符合在24件以內且在40公斤以內：（一）非屬經濟部公告准許輸入大陸物品項目。（二）經濟部公告准許輸入之大陸物品項目，列有特別規定「MXX」代號者。二、排除適用物品項目：貨品分類號列第8543.70.99.50-4號「第8711.60 目電動機器腳踏車或電動腳踏車用控制器」。", list[0].scope);
			Assert.AreEqual("進口人需於報單填列本專用證號代碼，可免除申請輸入許可證進口少量大陸物品。", list[0].remark);
		}
	}
}
