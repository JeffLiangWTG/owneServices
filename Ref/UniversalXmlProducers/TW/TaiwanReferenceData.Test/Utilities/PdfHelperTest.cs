using System.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class PdfHelperTest
	{
		[Test]
		public void TestExtractTableToDataTable()
		{
			var unitsOfMeasurementPath = Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/TWCIU/計量單位.pdf");
			var bytes = File.ReadAllBytes(unitsOfMeasurementPath);
			var dataTable = PdfHelper.ExtractTableToDataTable(bytes);
			Assert.AreEqual(4, dataTable.Columns.Count);
			Assert.AreEqual(413, dataTable.Rows.Count);
			Assert.AreEqual("BAG*", dataTable.Rows[3][0]);
			Assert.AreEqual("袋 , 包", dataTable.Rows[3][1]);
			Assert.AreEqual("Bag", dataTable.Rows[3][2]);
		}
	}
}
