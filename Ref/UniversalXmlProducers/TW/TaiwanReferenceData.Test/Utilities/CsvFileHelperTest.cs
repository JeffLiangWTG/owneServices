using System.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	public class CsvFileHelperTest
	{
		[Test]
		public void TestGetDataTable()
		{
			var fileName = Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/TWER/EReg.csv");
			var bytes = File.ReadAllBytes(fileName);
			var dataTable = CsvFileHelper.GetDataTable(bytes);
			Assert.AreEqual(3, dataTable.Columns.Count);
			Assert.AreEqual(46, dataTable.Rows.Count);
			Assert.AreEqual("輸出規定代號", dataTable.Rows[0][0]);
			Assert.AreEqual("准許（免除簽發許可證）。", dataTable.Rows[1][1]);
			Assert.AreEqual("111", dataTable.Rows[2][0]);
			Assert.AreEqual("管制輸出。", dataTable.Rows[2][1]);
			Assert.AreEqual("Export controlled (licensed by the Bureau of Foreign Trade.)", dataTable.Rows[2][2]);
			Assert.AreEqual(string.Empty, dataTable.Rows[45][0]);
			Assert.AreEqual(System.DBNull.Value, dataTable.Rows[45][1]);
		}
	}
}
