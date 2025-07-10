using System;
using System.Linq;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Warehouse
{
	public abstract class WhsTemplateTestCase : TemplateTestCase
	{
		public void TestNoLeadingWhitespaceBeforeMacros()
		{
			CombineAssertions(() =>
			{
				foreach (var workSheet in Report.XlInterface.WorkSheets)
				{
					for (int i = 0; i <= workSheet.RowCount; i++)
					{
						for (int j = 0; j < workSheet.ColumnCount; j++)
						{
							var cell = workSheet[i, j];
							var cellValue = cell.ToString();
							var trimmedCellValue = cellValue.TrimStart();

							if (trimmedCellValue.StartsWith("<"))
							{
								Assert($"Redundant leading whitespace in Sheet Name: {workSheet.SheetName}, Cell: {ExcelWorkSheet.GetCellName(i, j)}, Value={cellValue}", cellValue == trimmedCellValue);
							}
						}
					}
				}
			});
		}

		public void TestNoTrailingWhitespaceAfterMacros()
		{
			CombineAssertions(() =>
			{
				foreach (var workSheet in Report.XlInterface.WorkSheets)
				{
					for (int i = 0; i <= workSheet.RowCount; i++)
					{
						for (int j = 0; j < workSheet.ColumnCount; j++)
						{
							var cell = workSheet[i, j];
							var cellValue = cell.ToString();
							var trimmedCellValue = cellValue.TrimEnd();

							if (trimmedCellValue.EndsWith(">") &&
								!cellValue.StartsWith("Data:", StringComparison.OrdinalIgnoreCase) &&
								!cellValue.StartsWith("<HideRowIf", StringComparison.OrdinalIgnoreCase))
							{
								Assert($"Redundant trailing whitespace in Sheet Name: {workSheet.SheetName}, Cell: {ExcelWorkSheet.GetCellName(i, j)}, Value={cellValue}", cellValue == trimmedCellValue);
							}
						}
					}
				}
			});
		}

		public void TestDoesNotUseCustomsProductFilter()
		{
			Report.PrepareForRender();
			var hasCustomsSupplierPartLookup = Report.FilterCollection.ToArray().OfType<LookupField>().Any(l => l.ModuleID == ModuleIDs.SupplierPart);

			AssertEquals("Should not use the customs SupplierPart module/lookup in Warehouse reports.\r\nWarehouse reports should use 'warehouse product' as it uses the Whs Product securities.", false, hasCustomsSupplierPartLookup);
		}
	}
}
