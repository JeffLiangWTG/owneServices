using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace CargoWise.RefDbRepo.TRReferenceData.Services
{
	public static class CsvRepository
	{
		public static IEnumerable<T> GetAllFrom<T>(string filePath)
		{
			using (var resourceStream = File.OpenRead(filePath))
			using (var streamReader = new StreamReader(resourceStream))
			{
				var config = new CsvHelper.Configuration.Configuration(CultureInfo.InvariantCulture);
				using (var csv = new CsvReader(streamReader, config))
				{
					csv.Configuration.HasHeaderRecord = true;
					return csv.GetRecords<T>().ToList();
				}
			}
		}
	}
}
