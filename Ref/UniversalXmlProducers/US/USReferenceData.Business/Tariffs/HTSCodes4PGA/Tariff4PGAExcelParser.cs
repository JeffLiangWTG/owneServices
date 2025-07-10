using System.Collections.Generic;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class Tariff4PGAExcelParser
	{
		readonly string filePath;
		List<Tariff4PGA> tariff4PGAList;

		public Tariff4PGAExcelParser(List<Tariff4PGA> tariff4PGAList, string filePath)
		{
			this.filePath = filePath;
			this.tariff4PGAList = tariff4PGAList;
		}

		public void Parse()
		{
			XlsFile xls = new XlsFile(filePath);
			xls.ActiveSheet = 1;
			var indexHTSCode = 0;
			var indexScheduleB = 0;
			for (int col = 1; col <= xls.ColCount; col++)
			{
				var columnName = xls.GetStringFromCell(1, col).Trim();
				if (columnName == Constants.ExcelColumnName.HTSCode)
				{
					indexHTSCode = col;
				}
				if (columnName == Constants.ExcelColumnName.ScheduleB)
				{
					indexScheduleB = col;
				}

				if (indexHTSCode > 0 && indexScheduleB > 0)
				{
					break;
				}
			}

			var isHTSCodeExist = indexHTSCode > 0;
			var isScheduleBExist = indexScheduleB > 0;
			for (int row = 2; row <= xls.RowCount; row++)
			{
				if (isHTSCodeExist)
				{
					var htsCode = xls.GetStringFromCell(row, indexHTSCode).Trim();
					if (!string.IsNullOrEmpty(htsCode))
					{
						tariff4PGAList.Add(new Tariff4PGA(htsCode, false, Constants.PGACodes.DEA));
					}
				}

				if (isScheduleBExist)
				{
					var scheduleB = xls.GetStringFromCell(row, indexScheduleB).Trim();
					if (!string.IsNullOrEmpty(scheduleB))
					{
						tariff4PGAList.Add(new Tariff4PGA(scheduleB, false, Constants.PGACodes.DEA));
					}
				}
			}
		}
	}
}
