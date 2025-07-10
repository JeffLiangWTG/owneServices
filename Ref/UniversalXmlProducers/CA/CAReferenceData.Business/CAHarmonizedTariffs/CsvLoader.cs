using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CsvHelper;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public static class CsvLoader
	{
		public static IEnumerable<string> GetConveyanceRequiredTariffList()
		{
			var result = new List<string>();
			string value;
			using (var resourceStream = File.OpenRead(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CAHarmonizedTariffs\\ConveyanceRequiredTariffList.csv")))
			using (var streamReader = new StreamReader(resourceStream))
			{
				var config = new CsvHelper.Configuration.Configuration(CultureInfo.InvariantCulture);
				using (var csv = new CsvReader(streamReader, config))
				{
					csv.Configuration.HasHeaderRecord = false;
					while (csv.Read())
					{
						for (int i = 0; csv.TryGetField<string>(i, out value); i++)
						{
							result.Add(value.Replace(".", ""));
						}
					}
				}
			}
			return result;
		}

		public static List<T> Deserialize<T>(string filepath)
		{
			using (var resourceStream = File.OpenRead(filepath))
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
