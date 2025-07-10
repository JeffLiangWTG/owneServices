using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public static class CsvFileHelper
	{
		public static DataTable GetDataTable(byte[] data)
		{
			var dataTable = new DataTable();

			using (var stream = new MemoryStream(data))
			using (var reader = new StreamReader(stream))
			using (var csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration(CultureInfo.InvariantCulture) { TrimOptions = CsvHelper.Configuration.TrimOptions.Trim }))
			{
				var records = csv.GetRecords<dynamic>();
				if (records.FirstOrDefault() is IDictionary<string, object> firstRecord)
				{
					var pairs = firstRecord.ToArray();
					var row = dataTable.Rows.Add();
					var row2 = dataTable.Rows.Add();
					var columnCount = pairs.Length;
					for (var i = 0; i < columnCount; i++)
					{
						dataTable.Columns.Add();
						row[i] = pairs[i].Key;
						row2[i] = pairs[i].Value;
					}

					foreach (IDictionary<string, object> record in records)
					{
						if (record != null)
						{
							pairs = record.ToArray();
							row = dataTable.Rows.Add();
							for (var i = 0; i < columnCount; i++)
							{
								row[i] = pairs[i].Value;
							}
						}
					}
				}
			}

			return dataTable;
		}
	}
}
