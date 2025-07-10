using System;
using CargoWise.RefDbRepo.CHReferenceData.Business;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests
{
	[TestFixture]
	internal class ExcelHelperTest
	{
		[Test]
		public void FindColumn() => Assert.Multiple(() =>
		{
			var workbook = GetTestWorkbook();
			var sheet = workbook.GetSheet("FindColumn") as XSSFSheet;
			var row = sheet.GetRow(1);
			Assert.That(row.FindColumn("Column A"), Is.EqualTo(0), "Column A");
			Assert.That(row.FindColumn("Column B"), Is.EqualTo(1), "Column B");
			Assert.That(row.FindColumn("Column D"), Is.EqualTo(3), "Column D");
			Assert.That(row.FindColumn("Column_D", "Column D"), Is.EqualTo(3), "Column_D");
			Assert.That(row.FindColumn("Column d"), Is.EqualTo(3), "Column d");
			Assert.Throws<InvalidOperationException>(() => row.FindColumn("XXX"), "non-existiong columns");
			row = null;
			Assert.Throws<InvalidOperationException>(() => row.FindColumn("Column A"), "row is null");
		});

		[Test]
		public void GetIntValueSafe() => Assert.Multiple(() =>
		{
			var workbook = GetTestWorkbook();
			var sheet = workbook.GetSheet("GetIntValueSafe") as XSSFSheet;

			var cell = sheet.GetRow(0).GetCell(1);
			Assert.That(cell.CellType, Is.EqualTo(CellType.Numeric), "Precondition: Numeric celltype");
			Assert.That(cell.GetIntValueSafe(), Is.EqualTo(12), "Numeric value");

			cell = sheet.GetRow(1).GetCell(1);
			Assert.That(cell.CellType, Is.EqualTo(CellType.String), "Precondition: String celltype");
			Assert.That(cell.GetIntValueSafe(), Is.EqualTo(13), "String value");

			cell = sheet.GetRow(2).GetCell(1);	
			Assert.That(cell, Is.Null, "Empty cell (null)");
			Assert.That(cell.GetIntValueSafe(), Is.EqualTo(0), "Empty (null)");

			cell = sheet.GetRow(3).GetCell(1);
			Assert.That(cell.CellType, Is.EqualTo(CellType.String), "Precondition: String celltype");
			Assert.That(cell.GetIntValueSafe(), Is.EqualTo(0), "Not a number");
		});

		[Test]
		public void GetDecimalValueSafe() => Assert.Multiple(() =>
		{
			var workbook = GetTestWorkbook();
			var sheet = workbook.GetSheet("GetDecimalValueSafe") as XSSFSheet;

			var cell = sheet.GetRow(0).GetCell(1);
			Assert.That(cell.CellType, Is.EqualTo(CellType.Numeric), "Precondition: Numeric celltype");
			Assert.That(cell.GetDecimalValueSafe(), Is.EqualTo(12.5m), "Numeric value");

			cell = sheet.GetRow(1).GetCell(1);
			Assert.That(cell.CellType, Is.EqualTo(CellType.String), "Precondition: String celltype");
			Assert.That(cell.GetDecimalValueSafe(), Is.EqualTo(13.5m), "String value");

			cell = sheet.GetRow(2).GetCell(1);
			Assert.That(cell, Is.Null, "Empty cell (null)");
			Assert.That(cell.GetDecimalValueSafe(), Is.EqualTo(0m), "Empty (null)");

			cell = sheet.GetRow(3).GetCell(1);
			Assert.That(cell.CellType, Is.EqualTo(CellType.String), "Precondition: String celltype");
			Assert.That(cell.GetDecimalValueSafe(), Is.EqualTo(0m), "Not a number");
		});

		[Test]
		public void GetStringValueSafe() => Assert.Multiple(() =>
		{
			var workbook = GetTestWorkbook();
			var sheet = workbook.GetSheet("GetStringValueSafe") as XSSFSheet;

			var cell = sheet.GetRow(0).GetCell(1);
			Assert.That(cell.CellType, Is.EqualTo(CellType.Numeric), "Precondition: Numeric celltype");
			Assert.That(cell.GetStringValueSafe(), Is.EqualTo("12.5"), "Numeric value");

			cell = sheet.GetRow(1).GetCell(1);
			Assert.That(cell.CellType, Is.EqualTo(CellType.String), "Precondition: String celltype");
			Assert.That(cell.GetStringValueSafe(), Is.EqualTo("abc"), "String value");

			cell = sheet.GetRow(2).GetCell(1);
			Assert.That(cell, Is.Null, "Empty cell (null)");
			Assert.That(cell.GetStringValueSafe(), Is.EqualTo(string.Empty), "Empty (null)");
		});

		XSSFWorkbook GetTestWorkbook()
		{
			using (var stream = typeof(ExcelHelperTest).GetTestStream("Helpers.TestFiles.ExcelHelperTest.xlsx"))
			{
				return new XSSFWorkbook(stream);
			}
		}
	}
}
