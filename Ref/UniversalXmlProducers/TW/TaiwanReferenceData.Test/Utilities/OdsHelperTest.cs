using System.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class OdsHelperTest
	{
		[Test]
		public void TestGetData()
		{
			byte[] array = File.ReadAllBytes(Path.Combine(FolderHelper.GetBinFolder(), "doc/Utilities/OdsHelperTestFile.ods"));
			using (var file = new MemoryStream(array))
			{
				var document = OdsHelper.GetDocument(file);
				var nmsManager = OdsHelper.InitializeXmlNamespaceManager(document);

				var tables = OdsHelper.GetTableNodes(document, nmsManager);
				Assert.AreEqual(2, tables.Count);
				Assert.AreEqual("注意事項", OdsHelper.GetTableName(tables[0]));
				Assert.AreEqual("鎖檔稅則", OdsHelper.GetTableName(tables[1]));

				var rows = OdsHelper.GetRowNodes(tables[1], nmsManager);
				Assert.AreEqual(11, rows.Count);

				var cells = OdsHelper.GetCellNodes(rows[0], nmsManager);
				Assert.AreEqual(3, cells.Count);
				Assert.AreEqual("貨品分類號列", OdsHelper.GetDataValue(cells[0]));
				Assert.AreEqual("中文貨名", OdsHelper.GetDataValue(cells[1]));

				cells = OdsHelper.GetCellNodes(rows[1], nmsManager);
				Assert.AreEqual(3, cells.Count);
				Assert.AreEqual("04011010001", OdsHelper.GetDataValue(cells[0]));
				Assert.AreEqual("未濃縮且未加糖及未含其他甜味料之鮮乳（生乳及羊乳除外），含脂重量不超過１%者", OdsHelper.GetDataValue(cells[1]));
			}
		}
	}
}
