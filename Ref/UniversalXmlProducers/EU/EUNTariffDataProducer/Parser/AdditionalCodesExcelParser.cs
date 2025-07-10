using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public static class AdditionalCodesExcelParser
	{
		public static IList<AdditionalCodesData> ReadAdditionalCodesXlsxFileIntoResults(string fileNameAndPath)
		{
			var allExcelDatas = new Dictionary<string, AdditionalCodesData>();
			using (var reader = File.OpenRead(fileNameAndPath))
			{
				var workbook = WorkbookFactory.Create(reader);
				var worksheet = workbook.GetSheetAt(0);
				var rowCount = worksheet.LastRowNum;

				for (var rowId = 1; rowId <= rowCount; rowId++)
				{

					var dataRow = worksheet.GetRow(rowId);
					var additionalCode = dataRow.GetStringValue(0);
					var language = dataRow.GetStringValue(1);
					var description = dataRow.GetStringValue(2);
					var startDate = GetBoundaryDate(dataRow.GetStringValue(3), true);
					var endDate = GetBoundaryDate(dataRow.GetStringValue(5), false);
					if (!string.IsNullOrWhiteSpace(additionalCode))
					{
						if (allExcelDatas.TryGetValue(additionalCode, out var existingAdditionalCodeData))
						{
							existingAdditionalCodeData.MultilingualDescriptions.Add(language, description);
						}
						else
						{
							allExcelDatas[additionalCode] = new AdditionalCodesData()
							{
								Code = additionalCode,
								StartDate = startDate,
								EndDate = endDate,
								MultilingualDescriptions = new Dictionary<string, string>
								{
									{ language, description }
								}
							};
						}
					}
				}
			}
			return allExcelDatas.Select(x => x.Value).ToList();
		}

		public static DateTime? GetBoundaryDate(string dateStringValue, bool isStartDate)
		{
			DateTime? result;

			if (string.IsNullOrEmpty(dateStringValue))
			{
				result = isStartDate ? EUNUtils.MinDateTime : EUNUtils.MaxDateTime;
			}
			else if(DateTime.TryParseExact(dateStringValue, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
			{
				result = parsedDate;
			}
			else
			{
				result = null;
			}

			return result;
		}
	}
}
