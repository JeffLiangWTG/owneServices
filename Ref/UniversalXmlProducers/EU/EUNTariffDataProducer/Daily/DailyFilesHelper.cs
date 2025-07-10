using System;
using System.Collections.Generic;
using System.IO;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public static class DailyFilesHelper
	{
		public static bool CheckFileLastTimeWrite(IEnumerable<string> files)
		{
			var outOfDate = false;
			foreach (var file in files)
			{
				var filePath = Path.Combine(ApplicationConfig.DownloadsFolder, file);
				var fileInfo = new FileInfo(filePath);
				var lastWriteTime = fileInfo.LastWriteTimeUtc;
				// files are published a few days before the beginning of the month. So files published on
				// 31-July-21 will cover data for 1-Aug-21 to 31-Aug-21
				if (new DateTime(lastWriteTime.Year, lastWriteTime.Month, 01, 23, 59, 59).AddMonths(2).AddDays(-1) < DateTime.UtcNow)
				{
					outOfDate = true;
					break;
				}
			}
			return outOfDate;
		}
	}
}
