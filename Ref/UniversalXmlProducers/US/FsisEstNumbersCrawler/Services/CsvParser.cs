using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace FsisEstNumbersCrawler.Services
{
	public class CsvParser : ICsvParser
	{
		public List<T> Parse<T>(string filePath, Configuration config = null) where T : new()
		{
			if(config == null)
			{
				config = new Configuration();
				config.HasHeaderRecord = true;
			}
			using (var streamReader = new StreamReader(filePath))
			using (var csv = new CsvReader(streamReader, config))
			{
				return csv.GetRecords<T>().ToList();
			}
		}
	}
}
