using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public abstract class ExcelDataParser
	{
		public abstract IDictionary<string, int> HeaderMap { get; }
		public abstract IExcelDataRecord ParseRecord(IRow rawDataRow);
		protected void UpdateHeaderMap(IRow headerRow)
		{
			foreach (var headerRowCell in headerRow.Cells)
			{
				var cellValue = headerRowCell.StringCellValue;

				if (string.IsNullOrEmpty(cellValue) || !HeaderMap.ContainsKey(cellValue))
				{
					continue;
				}

				HeaderMap[cellValue] = headerRowCell.ColumnIndex;
			}
		}

		protected virtual ICollection<IExcelDataRecord> ParseRecords(string filePath)
		{
			Argument.NotNullOrEmpty(filePath, nameof(filePath));
			var rawRecords = new List<IExcelDataRecord>();

			using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				var workbook = WorkbookFactory.Create(fileStream);
				var worksheet = workbook.GetSheetAt(0);
				var headerRow = worksheet.GetRow(0);
				UpdateHeaderMap(headerRow);

				var rowCount = worksheet.LastRowNum;

				for (var i = 1; i <= rowCount; i++)
				{
					var dataRow = worksheet.GetRow(i);
					var record = ParseRecord(dataRow);

					if (record is not null)
					{
						rawRecords.Add(record);
					}
				}
			}
			return rawRecords;
		}
	}
}
