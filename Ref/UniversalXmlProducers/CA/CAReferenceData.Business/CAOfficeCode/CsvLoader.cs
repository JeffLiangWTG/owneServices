using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using CsvHelper;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAOfficeCode
{
	public static class CsvLoader
	{
		public static Dictionary<string,string> GetUSPortOfExitMapping(string filepath)
		{
			using (var resourceStream = File.OpenRead(filepath))
			using (var streamReader = new StreamReader(resourceStream))
			{
				var config = new CsvHelper.Configuration.Configuration(CultureInfo.InvariantCulture);
				using (var csv = new CsvReader(streamReader, config))
				{
					csv.Configuration.HasHeaderRecord = true;
					return csv.GetRecords<CAOfficeCodeMapping>().ToDictionary(x => x.OfficeCode.PadLeft(4, '0'), x => x.USPortOfExit.PadLeft(4, '0'));
				}
			}
		}
	}
}
