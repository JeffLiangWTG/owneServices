using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public static class TRCustomsCodeListLoader
	{
		public static IEnumerable<TRCustomsRefCusCodes> LoadData(string path, List<RefCusCodeListExcelConfig> excelConfig)
		{
			var records = new List<TRCustomsRefCusCodes>();

			XlsFile xls = new XlsFile(path);
			xls.ActiveSheet = 1;

			for (int row = 2; row <= xls.RowCount; row++)
			{
				var customsRefCusCode = new TRCustomsRefCusCodes();

				var codeType = GetColIndexByDBFieldName("ZZD_ZZK_NKCodeType", excelConfig);
				if (codeType != null)
				{
					customsRefCusCode.CodeType = xls.GetStringFromCell(row, codeType.ColumnIndex).Trim();
				}

				var code = GetColIndexByDBFieldName("ZZD_Code", excelConfig);
				if (code != null)
				{
					customsRefCusCode.Code = xls.GetStringFromCell(row, code.ColumnIndex).Trim();
				}

				var description = GetColIndexByDBFieldName("ZZD_Description", excelConfig);
				if (description != null)
				{
					customsRefCusCode.Description = xls.GetStringFromCell(row, description.ColumnIndex).Trim();
				}

				var startDateConfig = GetColIndexByDBFieldName("ZZD_StartDate", excelConfig);
				if (startDateConfig != null)
				{
					customsRefCusCode.StartDate = DateTime.TryParse(xls.GetStringFromCell(row, startDateConfig.ColumnIndex).Trim(), out var startDate) ? startDate : DateTime.MinValue;
				}

				var endDateConfig = GetColIndexByDBFieldName("ZZD_EndDate", excelConfig);
				if (endDateConfig != null)
				{
					customsRefCusCode.EndDate = DateTime.TryParse(xls.GetStringFromCell(row, endDateConfig.ColumnIndex).Trim(), out var endDate) ? endDate : DateTime.MinValue;
				}

				if (!string.IsNullOrEmpty(customsRefCusCode.Code))
				{
					records.Add(customsRefCusCode);
				}
			}

			return records;
		}

		public static RefCusCodeListExcelConfig GetColIndexByDBFieldName(string fieldName, List<RefCusCodeListExcelConfig> excelConfig)
		{
			return excelConfig.FirstOrDefault(x => x.FieldNameInDB == fieldName);
		}
	}

	public class RefCusCodeListExcelConfig
	{
		public RefCusCodeListExcelConfig(int columnIndex, string fieldNameInDB, string excelColumnText)
		{
			this.ColumnIndex = columnIndex;
			this.FieldNameInDB = fieldNameInDB;
			this.ExcelColumnText = excelColumnText;
		}
		public int ColumnIndex { get; set; }
		public string FieldNameInDB { get; set; }
		public string ExcelColumnText { get; set; }
	}
}
