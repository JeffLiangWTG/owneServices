using System.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class ExcelHelperTest
	{
		[Test]
		public void TestBytesToDataTable()
		{
			var packingHouseListPath = Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/TWPKH/PackingHouseList.xlsx");
			var bytes = File.ReadAllBytes(packingHouseListPath);
			var dataTable = ExcelHelper.BytesToDataTable(ExcelFileFormat.Xlsx, bytes, 0, 0, true);
			Assert.AreEqual(1358, dataTable.Rows.Count);
			Assert.AreEqual("07061000005", dataTable.Rows[0]["號列"]);
			Assert.AreEqual("KR9998", dataTable.Rows[1357]["包裝場代號"]);
		}
	}
}
